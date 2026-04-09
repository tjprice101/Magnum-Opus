using System;
using MagnumOpus.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// Shared rendering pipeline for IncisorOrb-style homing projectiles.
    /// Exact 1-to-1 replica of IncisorOrbProj/LunarBeamProj visual architecture:
    ///   LAYER 1: Shader-driven beam body via VertexStrip (InfernalBeamBodyShader)
    ///   LAYER 2: Multi-layer bloom head (SoftGlow + PointBloom, palette color cycling)
    ///
    /// Each theme provides its own ThemeConfig with palette colors and texture paths.
    /// Call DrawOrbVisuals() from any homing projectile's PreDraw to get the full effect.
    /// </summary>
    public static class IncisorOrbRenderer
    {
        // =====================================================================
        //  THEME CONFIGURATION
        // =====================================================================

        /// <summary>
        /// Per-theme configuration for IncisorOrb rendering.
        /// Defines the color palette, gradient LUT, and beam textures.
        /// </summary>
        public struct ThemeConfig
        {
            /// <summary>6-color palette for MulticolorLerp bloom cycling (Pianissimo → Sforzando).</summary>
            public Color[] Palette;
            /// <summary>Path to gradient LUT texture (e.g. "MagnumOpus/Assets/VFX Asset Library/ColorGradients/EroicaGradientLUTandRAMP").</summary>
            public string GradientLUTPath;
            /// <summary>Path to body texture (beam body layer). Falls back to generic SoundWaveBeam if null.</summary>
            public string BodyTexPath;
            /// <summary>Path to detail texture 1 (scrolling detail). Falls back to generic EnergyMotion if null.</summary>
            public string DetailTex1Path;
            /// <summary>Path to detail texture 2 (counter-scrolling detail). Falls back to generic EnergySurgeBeam if null.</summary>
            public string DetailTex2Path;
            /// <summary>Light color vector for Lighting.AddLight (RGB 0-1 scale).</summary>
            public Vector3 LightColor;
            /// <summary>Vanilla dust ID for spark trail particles.</summary>
            public int DustType;
        }

        // =====================================================================
        //  SHARED TEXTURE CACHING
        // =====================================================================

        // Shared across all themes (same textures)
        private static Asset<Texture2D> _basicTrail;
        private static Asset<Texture2D> _fbmNoise;
        private static Asset<Texture2D> _softGlow;
        private static Asset<Texture2D> _pointBloom;
        private static Effect _beamShader;

        // Per-theme cached textures (keyed by gradient LUT path as unique identifier)
        private static string _cachedThemeKey;
        private static Asset<Texture2D> _cachedGradientLUT;
        private static Asset<Texture2D> _cachedBodyTex;
        private static Asset<Texture2D> _cachedDetailTex1;
        private static Asset<Texture2D> _cachedDetailTex2;

        private const string Beams = "MagnumOpus/Assets/VFX Asset Library/BeamTextures/";
        private const string Trails = "MagnumOpus/Assets/VFX Asset Library/TrailsAndRibbons/";
        private const string Bloom = "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/";
        private const string Noise = "MagnumOpus/Assets/VFX Asset Library/NoiseTextures/";

        private static void EnsureSharedTextures()
        {
            _basicTrail ??= ModContent.Request<Texture2D>(Trails + "BasicTrail", AssetRequestMode.ImmediateLoad);
            _fbmNoise ??= ModContent.Request<Texture2D>(Noise + "TileableFBMNoise", AssetRequestMode.ImmediateLoad);
            _softGlow ??= ModContent.Request<Texture2D>(Bloom + "SoftGlow", AssetRequestMode.ImmediateLoad);
            _pointBloom ??= ModContent.Request<Texture2D>(Bloom + "PointBloom", AssetRequestMode.ImmediateLoad);
        }

        private static void EnsureThemeTextures(in ThemeConfig config)
        {
            string key = config.GradientLUTPath ?? "";
            if (_cachedThemeKey == key) return;

            _cachedThemeKey = key;
            _cachedGradientLUT = ModContent.Request<Texture2D>(config.GradientLUTPath, AssetRequestMode.ImmediateLoad);
            _cachedBodyTex = ModContent.Request<Texture2D>(
                config.BodyTexPath ?? (Beams + "SoundWaveBeam"), AssetRequestMode.ImmediateLoad);
            _cachedDetailTex1 = ModContent.Request<Texture2D>(
                config.DetailTex1Path ?? (Beams + "EnergyMotion"), AssetRequestMode.ImmediateLoad);
            _cachedDetailTex2 = ModContent.Request<Texture2D>(
                config.DetailTex2Path ?? (Beams + "EnergySurgeBeam"), AssetRequestMode.ImmediateLoad);
        }

        // =====================================================================
        //  MAIN RENDERING ENTRY POINT
        // =====================================================================

        /// <summary>
        /// Draws the full IncisorOrb visual pipeline for a projectile.
        /// Call from PreDraw (return false after calling).
        /// Requires: TrailingMode[Type] = 2, TrailCacheLength[Type] >= 16.
        /// </summary>
        /// <param name="sb">Main.spriteBatch</param>
        /// <param name="proj">The projectile being drawn</param>
        /// <param name="config">Theme-specific visual configuration</param>
        /// <param name="strip">A VertexStrip instance (store as a field on the projectile, create with new VertexStrip())</param>
        public static void DrawOrbVisuals(SpriteBatch sb, Projectile proj, in ThemeConfig config, ref VertexStrip strip)
        {
            if (Main.dedServ) return;

            try
            {
                EnsureSharedTextures();
                EnsureThemeTextures(in config);

                // Count valid trail positions
                int count = 0;
                for (int i = 0; i < proj.oldPos.Length; i++)
                {
                    if (proj.oldPos[i] == Vector2.Zero) break;
                    count++;
                }

                sb.End(); // End current SpriteBatch for raw vertex drawing

                // === LAYER 1: Shader-driven beam body via VertexStrip ===
                if (count >= 2)
                {
                    Vector2[] positions = new Vector2[count];
                    float[] rotations = new float[count];
                    float totalLength = 0f;

                    for (int i = 0; i < count; i++)
                    {
                        positions[i] = proj.oldPos[i] + proj.Size / 2f;
                        rotations[i] = proj.oldRot[i];
                        if (i > 0) totalLength += Vector2.Distance(positions[i - 1], positions[i]);
                    }

                    strip ??= new VertexStrip();
                    strip.PrepareStrip(positions, rotations,
                        (float progress) => Color.White * (1f - progress * 0.85f),
                        (float progress) => MathHelper.Lerp(24f, 2f, progress),
                        -Main.screenPosition, includeBacksides: true);

                    _beamShader ??= ModContent.Request<Effect>(
                        "MagnumOpus/Content/FoundationWeapons/InfernalBeamFoundation/Shaders/InfernalBeamBodyShader",
                        AssetRequestMode.ImmediateLoad).Value;

                    if (_beamShader != null)
                    {
                        float repVal = MathHelper.Max(totalLength / 800f, 0.3f);
                        float time = (float)Main.timeForVisualEffects * -0.024f;

                        _beamShader.Parameters["WorldViewProjection"].SetValue(
                            Main.GameViewMatrix.NormalizedTransformationmatrix);
                        _beamShader.Parameters["onTex"].SetValue(_basicTrail.Value);
                        _beamShader.Parameters["gradientTex"].SetValue(_cachedGradientLUT.Value);
                        _beamShader.Parameters["bodyTex"].SetValue(_cachedBodyTex.Value);
                        _beamShader.Parameters["detailTex1"].SetValue(_cachedDetailTex1.Value);
                        _beamShader.Parameters["detailTex2"].SetValue(_cachedDetailTex2.Value);
                        _beamShader.Parameters["noiseTex"].SetValue(_fbmNoise.Value);

                        _beamShader.Parameters["bodyReps"].SetValue(1.5f * repVal);
                        _beamShader.Parameters["detail1Reps"].SetValue(2.0f * repVal);
                        _beamShader.Parameters["detail2Reps"].SetValue(1.2f * repVal);
                        _beamShader.Parameters["gradientReps"].SetValue(0.75f * repVal);
                        _beamShader.Parameters["bodyScrollSpeed"].SetValue(0.8f);
                        _beamShader.Parameters["detail1ScrollSpeed"].SetValue(1.2f);
                        _beamShader.Parameters["detail2ScrollSpeed"].SetValue(-0.6f);
                        _beamShader.Parameters["noiseDistortion"].SetValue(0.025f);
                        _beamShader.Parameters["totalMult"].SetValue(1.3f);
                        _beamShader.Parameters["uTime"].SetValue(time);

                        _beamShader.CurrentTechnique.Passes["MainPS"].Apply();
                        strip.DrawTrail();
                        Main.pixelShader.CurrentTechnique.Passes[0].Apply();
                    }
                }

                // === LAYER 2: Multi-layer bloom head ===
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                    SamplerState.LinearClamp, DepthStencilState.None,
                    RasterizerState.CullCounterClockwise, null,
                    Main.GameViewMatrix.TransformationMatrix);

                Vector2 drawPos = proj.Center - Main.screenPosition;
                Texture2D glowTex = _softGlow?.Value;
                Texture2D bloomTex = _pointBloom?.Value;

                if (glowTex != null && bloomTex != null)
                {
                    float paletteT = (Main.GlobalTimeWrappedHourly * 2f) % 1f;
                    Color orbGlow = MulticolorLerp(paletteT, config.Palette);
                    Color innerGlow = MulticolorLerp(MathHelper.Clamp(paletteT + 0.25f, 0f, 0.999f), config.Palette);
                    float pulse = 0.9f + 0.1f * MathF.Sin((float)Main.timeForVisualEffects * 0.1f + proj.whoAmI * 1.7f);

                    // Layer 1: Wide soft outer halo (atmospheric presence)
                    sb.Draw(glowTex, drawPos, null, (orbGlow with { A = 0 }) * 0.3f * pulse, 0f,
                        glowTex.Size() / 2f, 0.28f, SpriteEffects.None, 0f);

                    // Layer 2: Mid bloom (color identity)
                    sb.Draw(bloomTex, drawPos, null, (orbGlow with { A = 0 }) * 0.45f * pulse, 0f,
                        bloomTex.Size() / 2f, 0.12f, SpriteEffects.None, 0f);

                    // Layer 3: Inner bright bloom (warm inner glow)
                    sb.Draw(bloomTex, drawPos, null, (innerGlow with { A = 0 }) * 0.6f, 0f,
                        bloomTex.Size() / 2f, 0.07f, SpriteEffects.None, 0f);

                    // Layer 4: White-hot core (pinpoint brightness)
                    sb.Draw(bloomTex, drawPos, null, (Color.White with { A = 0 }) * 0.75f, 0f,
                        bloomTex.Size() / 2f, 0.035f, SpriteEffects.None, 0f);

                    // Layer 5: Rotating star flare accent
                    float flareRot = (float)Main.timeForVisualEffects * 0.03f + proj.whoAmI * 0.8f;
                    sb.Draw(glowTex, drawPos, null, (innerGlow with { A = 0 }) * 0.25f * pulse,
                        flareRot, glowTex.Size() / 2f, new Vector2(0.18f, 0.06f), SpriteEffects.None, 0f);
                    sb.Draw(glowTex, drawPos, null, (innerGlow with { A = 0 }) * 0.25f * pulse,
                        flareRot + MathHelper.PiOver2, glowTex.Size() / 2f, new Vector2(0.18f, 0.06f), SpriteEffects.None, 0f);
                }

                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                    Main.DefaultSamplerState, DepthStencilState.None,
                    Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
            catch
            {
                // Swallow exceptions to prevent black squares — ensure SpriteBatch is always restored
            }
            finally
            {
                try { sb.End(); } catch { }
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                    Main.DefaultSamplerState, DepthStencilState.None,
                    Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
        }

        // =====================================================================
        //  HELPER: MulticolorLerp (identical to IOFUtils)
        // =====================================================================

        public static Color MulticolorLerp(float t, Color[] colors)
        {
            if (colors == null || colors.Length == 0) return Color.White;
            if (colors.Length == 1) return colors[0];

            t = MathHelper.Clamp(t, 0f, 0.999f);
            float scaled = t * (colors.Length - 1);
            int lo = (int)scaled;
            int hi = Math.Min(lo + 1, colors.Length - 1);
            return Color.Lerp(colors[lo], colors[hi], scaled - lo);
        }

        // =====================================================================
        //  PRE-BUILT THEME CONFIGURATIONS
        // =====================================================================

        /// <summary>Eroica: Scarlet → Gold → HotCore fire gradient.</summary>
        public static readonly ThemeConfig Eroica = new ThemeConfig
        {
            Palette = new Color[]
            {
                new Color(139, 0, 0),       // DeepScarlet (Pianissimo)
                new Color(200, 50, 50),      // Scarlet (Piano)
                new Color(255, 100, 50),     // Flame (Mezzo)
                new Color(255, 215, 0),      // Gold (Forte)
                new Color(255, 240, 200),    // HotCore (Fortissimo)
                new Color(255, 255, 240),    // White-gold (Sforzando)
            },
            GradientLUTPath = "MagnumOpus/Assets/VFX Asset Library/ColorGradients/EroicaGradientLUTandRAMP",
            BodyTexPath = "MagnumOpus/Assets/VFX Asset Library/BeamTextures/SoundWaveBeam",
            DetailTex1Path = "MagnumOpus/Assets/VFX Asset Library/Theme Specific/Eroica/Beam Textures/ER Energy Motion Beam",
            DetailTex2Path = "MagnumOpus/Assets/VFX Asset Library/Theme Specific/Eroica/Beam Textures/ER Energy Surge Beam",
            LightColor = new Vector3(0.7f, 0.3f, 0.1f),
            DustType = 6, // DustID.Torch
        };

        /// <summary>La Campanella: DeepEmber → InfernalOrange → BellGold → WhiteHot.</summary>
        public static readonly ThemeConfig LaCampanella = new ThemeConfig
        {
            Palette = new Color[]
            {
                new Color(180, 60, 0),       // DeepEmber (Pianissimo)
                new Color(255, 100, 0),      // InfernalOrange (Piano)
                new Color(255, 200, 50),     // FlameYellow (Mezzo)
                new Color(218, 165, 32),     // BellGold (Forte)
                new Color(255, 240, 180),    // ChimeShimmer (Fortissimo)
                new Color(255, 240, 200),    // WhiteHot (Sforzando)
            },
            GradientLUTPath = "MagnumOpus/Assets/VFX Asset Library/ColorGradients/LaCampanellaGradientLUTandRAMP",
            BodyTexPath = "MagnumOpus/Assets/VFX Asset Library/BeamTextures/SoundWaveBeam",
            DetailTex1Path = "MagnumOpus/Assets/VFX Asset Library/BeamTextures/EnergyMotion",
            DetailTex2Path = "MagnumOpus/Assets/VFX Asset Library/BeamTextures/EnergySurgeBeam",
            LightColor = new Vector3(0.7f, 0.4f, 0.05f),
            DustType = 6, // DustID.Torch
        };

        /// <summary>Enigma: VoidBlack → DeepPurple → GreenFlame → WhiteGreenFlash.</summary>
        public static readonly ThemeConfig Enigma = new ThemeConfig
        {
            Palette = new Color[]
            {
                new Color(15, 10, 20),       // VoidBlack (Pianissimo)
                new Color(80, 20, 120),      // DeepPurple (Piano)
                new Color(140, 60, 200),     // Purple (Mezzo)
                new Color(50, 220, 100),     // GreenFlame (Forte)
                new Color(120, 255, 160),    // BrightGreen (Fortissimo)
                new Color(220, 255, 230),    // WhiteGreenFlash (Sforzando)
            },
            GradientLUTPath = "MagnumOpus/Assets/VFX Asset Library/ColorGradients/EnigmaGradientLUTandRAMP",
            BodyTexPath = "MagnumOpus/Assets/VFX Asset Library/BeamTextures/SoundWaveBeam",
            DetailTex1Path = "MagnumOpus/Assets/VFX Asset Library/Theme Specific/Enigma/Beam Textures/EN Energy Motion Beam",
            DetailTex2Path = "MagnumOpus/Assets/VFX Asset Library/Theme Specific/Enigma/Beam Textures/EN Energy Surge Beam",
            LightColor = new Vector3(0.2f, 0.6f, 0.3f),
            DustType = 75, // DustID.GreenTorch
        };

        /// <summary>Swan Lake: ObsidianBlack → Silver → PureWhite → PrismaticShimmer.</summary>
        public static readonly ThemeConfig SwanLake = new ThemeConfig
        {
            Palette = new Color[]
            {
                new Color(20, 20, 30),       // ObsidianBlack (Pianissimo)
                new Color(80, 80, 100),      // DarkSilver (Piano)
                new Color(180, 185, 200),    // Silver (Mezzo)
                new Color(240, 240, 250),    // PureWhite (Forte)
                new Color(220, 230, 255),    // PrismaticShimmer (Fortissimo)
                new Color(255, 255, 255),    // RainbowFlash (Sforzando)
            },
            GradientLUTPath = "MagnumOpus/Assets/VFX Asset Library/ColorGradients/SwanLakeGradient",
            BodyTexPath = "MagnumOpus/Assets/VFX Asset Library/BeamTextures/SoundWaveBeam",
            DetailTex1Path = "MagnumOpus/Assets/VFX Asset Library/Theme Specific/Swan Lake/Beam Textures/SL Energy Motion Beam",
            DetailTex2Path = "MagnumOpus/Assets/VFX Asset Library/Theme Specific/Swan Lake/Beam Textures/SL Energy Surge Beam",
            LightColor = new Vector3(0.6f, 0.6f, 0.7f),
            DustType = 15, // DustID.WhiteTorch
        };

        /// <summary>Fate: CosmicVoid → DarkPink → BrightCrimson → StarGold → SupernovaWhite.</summary>
        public static readonly ThemeConfig Fate = new ThemeConfig
        {
            Palette = new Color[]
            {
                new Color(15, 5, 20),        // CosmicVoid (Pianissimo)
                new Color(180, 50, 100),     // DarkPink (Piano)
                new Color(255, 60, 80),      // BrightCrimson (Mezzo)
                new Color(255, 230, 180),    // StarGold (Forte)
                new Color(255, 255, 255),    // WhiteCelestial (Fortissimo)
                new Color(255, 255, 250),    // SupernovaWhite (Sforzando)
            },
            GradientLUTPath = "MagnumOpus/Assets/VFX Asset Library/ColorGradients/FateGradientLUTandRAMP",
            BodyTexPath = "MagnumOpus/Assets/VFX Asset Library/BeamTextures/SoundWaveBeam",
            DetailTex1Path = "MagnumOpus/Assets/VFX Asset Library/BeamTextures/EnergyMotion",
            DetailTex2Path = "MagnumOpus/Assets/VFX Asset Library/Theme Specific/Fate/Beam Textures/FA Energy Surge Beam",
            LightColor = new Vector3(0.6f, 0.2f, 0.4f),
            DustType = 72, // DustID.PinkTorch
        };

        /// <summary>Nachtmusik: MidnightBlue → DeepBlue → StarlitBlue → StarWhite → MoonlitSilver → TwinklingWhite.</summary>
        public static readonly ThemeConfig Nachtmusik = new ThemeConfig
        {
            Palette = new Color[]
            {
                new Color(15, 15, 45),       // MidnightBlue (Pianissimo)
                new Color(30, 50, 120),      // DeepBlue (Piano)
                new Color(80, 120, 200),     // StarlitBlue (Mezzo)
                new Color(200, 210, 240),    // StarWhite (Forte)
                new Color(230, 235, 245),    // MoonlitSilver (Fortissimo)
                new Color(248, 250, 255),    // TwinklingWhite (Sforzando)
            },
            GradientLUTPath = "MagnumOpus/Assets/VFX Asset Library/ColorGradients/NachtmusikGradientLUTandRAMP",
            BodyTexPath = "MagnumOpus/Assets/VFX Asset Library/BeamTextures/SoundWaveBeam",
            DetailTex1Path = "MagnumOpus/Assets/VFX Asset Library/Theme Specific/Nachtmusik/Beam Textures/NK Energy Motion Beam",
            DetailTex2Path = "MagnumOpus/Assets/VFX Asset Library/Theme Specific/Nachtmusik/Beam Textures/NK Energy Surge Beam",
            LightColor = new Vector3(0.3f, 0.4f, 0.7f),
            DustType = 135, // DustID.BlueTorch
        };

        /// <summary>Dies Irae: BloodBlack → DeepCrimson → WrathRed → EmberOrange → FireWhite.</summary>
        public static readonly ThemeConfig DiesIrae = new ThemeConfig
        {
            Palette = new Color[]
            {
                new Color(30, 5, 5),         // BloodBlack (Pianissimo)
                new Color(150, 20, 20),      // DeepCrimson (Piano)
                new Color(200, 50, 30),      // WrathRed (Mezzo)
                new Color(255, 140, 40),     // EmberOrange (Forte)
                new Color(255, 200, 120),    // FlameGold (Fortissimo)
                new Color(255, 240, 220),    // FireWhite (Sforzando)
            },
            GradientLUTPath = "MagnumOpus/Assets/VFX Asset Library/ColorGradients/DiesIraeGradientLUTandRAMP",
            BodyTexPath = "MagnumOpus/Assets/VFX Asset Library/BeamTextures/SoundWaveBeam",
            DetailTex1Path = "MagnumOpus/Assets/VFX Asset Library/BeamTextures/EnergyMotion",
            DetailTex2Path = "MagnumOpus/Assets/VFX Asset Library/BeamTextures/EnergySurgeBeam",
            LightColor = new Vector3(0.7f, 0.15f, 0.1f),
            DustType = 6, // DustID.Torch
        };

        /// <summary>Ode to Joy: ForestGreen → GoldenAmber → WarmGold → SunlightWhite.</summary>
        public static readonly ThemeConfig OdeToJoy = new ThemeConfig
        {
            Palette = new Color[]
            {
                new Color(30, 60, 20),       // ForestGreen (Pianissimo)
                new Color(80, 140, 40),      // LeafGreen (Piano)
                new Color(200, 170, 50),     // GoldenAmber (Mezzo)
                new Color(255, 200, 50),     // WarmGold (Forte)
                new Color(255, 230, 150),    // SunlightGold (Fortissimo)
                new Color(255, 250, 230),    // SunlightWhite (Sforzando)
            },
            GradientLUTPath = "MagnumOpus/Assets/VFX Asset Library/ColorGradients/OdeToJoyGradientLUTandRAMP",
            BodyTexPath = "MagnumOpus/Assets/VFX Asset Library/BeamTextures/SoundWaveBeam",
            DetailTex1Path = "MagnumOpus/Assets/VFX Asset Library/BeamTextures/EnergyMotion",
            DetailTex2Path = "MagnumOpus/Assets/VFX Asset Library/BeamTextures/EnergySurgeBeam",
            LightColor = new Vector3(0.6f, 0.5f, 0.1f),
            DustType = 169, // DustID.YellowTorch
        };

        /// <summary>Clair de Lune: NightMistBlue → SoftBlue → PearlBlue → MoonlitGlow → LunarWhite.</summary>
        public static readonly ThemeConfig ClairDeLune = new ThemeConfig
        {
            Palette = new Color[]
            {
                new Color(20, 30, 60),       // NightMistBlue (Pianissimo)
                new Color(60, 100, 170),     // SoftBlue (Piano)
                new Color(150, 200, 255),    // PearlBlue (Mezzo)
                new Color(200, 220, 255),    // MoonlitGlow (Forte)
                new Color(230, 240, 255),    // PearlWhite (Fortissimo)
                new Color(250, 252, 255),    // LunarWhite (Sforzando)
            },
            GradientLUTPath = "MagnumOpus/Assets/VFX Asset Library/ColorGradients/ClairDeLuneGradientLUTandRAMP",
            BodyTexPath = "MagnumOpus/Assets/VFX Asset Library/BeamTextures/SoundWaveBeam",
            DetailTex1Path = "MagnumOpus/Assets/VFX Asset Library/BeamTextures/EnergyMotion",
            DetailTex2Path = "MagnumOpus/Assets/VFX Asset Library/BeamTextures/EnergySurgeBeam",
            LightColor = new Vector3(0.4f, 0.5f, 0.7f),
            DustType = 135, // DustID.BlueTorch
        };

        // =====================================================================
        //  SEASONAL THEME CONFIGURATIONS (Spring, Summer, Autumn, Winter)
        // =====================================================================

        /// <summary>Spring: DeepRose → RosePink → BlossomPink → SpringGreen → PetalWhite.</summary>
        public static readonly ThemeConfig Spring = new ThemeConfig
        {
            Palette = new Color[]
            {
                new Color(80, 30, 50),       // DeepRose (Pianissimo)
                new Color(180, 80, 120),     // RosePink (Piano)
                new Color(255, 150, 170),    // BlossomPink (Mezzo)
                new Color(255, 183, 197),    // SpringPink (Forte)
                new Color(200, 255, 200),    // SpringGreen (Fortissimo)
                new Color(255, 245, 240),    // PetalWhite (Sforzando)
            },
            GradientLUTPath = "MagnumOpus/Assets/VFX Asset Library/ColorGradients/SpringGradientLUTandRAMP",
            BodyTexPath = "MagnumOpus/Assets/VFX Asset Library/BeamTextures/SoundWaveBeam",
            DetailTex1Path = "MagnumOpus/Assets/VFX Asset Library/BeamTextures/EnergyMotion",
            DetailTex2Path = "MagnumOpus/Assets/VFX Asset Library/BeamTextures/EnergySurgeBeam",
            LightColor = new Vector3(0.8f, 0.4f, 0.5f),
            DustType = 72, // DustID.PinkTorch
        };

        /// <summary>Summer: DeepAmber → WarmOrange → SunOrange → SunGold → BrightGold → SunWhite.</summary>
        public static readonly ThemeConfig Summer = new ThemeConfig
        {
            Palette = new Color[]
            {
                new Color(80, 40, 0),        // DeepAmber (Pianissimo)
                new Color(200, 100, 0),      // WarmOrange (Piano)
                new Color(255, 140, 0),      // SunOrange (Mezzo)
                new Color(255, 215, 0),      // SunGold (Forte)
                new Color(255, 240, 150),    // BrightGold (Fortissimo)
                new Color(255, 250, 240),    // SunWhite (Sforzando)
            },
            GradientLUTPath = "MagnumOpus/Assets/VFX Asset Library/ColorGradients/LaCampanellaGradientLUTandRAMP",
            BodyTexPath = "MagnumOpus/Assets/VFX Asset Library/BeamTextures/SoundWaveBeam",
            DetailTex1Path = "MagnumOpus/Assets/VFX Asset Library/BeamTextures/EnergyMotion",
            DetailTex2Path = "MagnumOpus/Assets/VFX Asset Library/BeamTextures/EnergySurgeBeam",
            LightColor = new Vector3(0.8f, 0.6f, 0.1f),
            DustType = 6, // DustID.Torch
        };

        /// <summary>Autumn: DarkBrown → DecayPurple → AutumnRed → AutumnOrange → AutumnGold → WarmWhite.</summary>
        public static readonly ThemeConfig Autumn = new ThemeConfig
        {
            Palette = new Color[]
            {
                new Color(50, 25, 15),       // DarkBrown (Pianissimo)
                new Color(100, 50, 120),     // DecayPurple (Piano)
                new Color(178, 34, 34),      // AutumnRed (Mezzo)
                new Color(255, 140, 50),     // AutumnOrange (Forte)
                new Color(218, 165, 32),     // AutumnGold (Fortissimo)
                new Color(255, 230, 200),    // WarmWhite (Sforzando)
            },
            GradientLUTPath = "MagnumOpus/Assets/VFX Asset Library/ColorGradients/EroicaGradientLUTandRAMP",
            BodyTexPath = "MagnumOpus/Assets/VFX Asset Library/BeamTextures/SoundWaveBeam",
            DetailTex1Path = "MagnumOpus/Assets/VFX Asset Library/BeamTextures/EnergyMotion",
            DetailTex2Path = "MagnumOpus/Assets/VFX Asset Library/BeamTextures/EnergySurgeBeam",
            LightColor = new Vector3(0.7f, 0.4f, 0.15f),
            DustType = 6, // DustID.Torch
        };

        /// <summary>Winter: DeepFrost → DeepBlue → IceBlue → CrystalCyan → FrostWhite → PureWhite.</summary>
        public static readonly ThemeConfig Winter = new ThemeConfig
        {
            Palette = new Color[]
            {
                new Color(20, 40, 60),       // DeepFrost (Pianissimo)
                new Color(60, 100, 180),     // DeepBlue (Piano)
                new Color(150, 220, 255),    // IceBlue (Mezzo)
                new Color(100, 255, 255),    // CrystalCyan (Forte)
                new Color(240, 250, 255),    // FrostWhite (Fortissimo)
                new Color(255, 255, 255),    // PureWhite (Sforzando)
            },
            GradientLUTPath = "MagnumOpus/Assets/VFX Asset Library/ColorGradients/ClairDeLuneGradientLUTandRAMP",
            BodyTexPath = "MagnumOpus/Assets/VFX Asset Library/BeamTextures/SoundWaveBeam",
            DetailTex1Path = "MagnumOpus/Assets/VFX Asset Library/BeamTextures/EnergyMotion",
            DetailTex2Path = "MagnumOpus/Assets/VFX Asset Library/BeamTextures/EnergySurgeBeam",
            LightColor = new Vector3(0.4f, 0.6f, 0.8f),
            DustType = 135, // DustID.BlueTorch
        };

        // =====================================================================
        //  SWAN LAKE RAINBOW SPARKLE HELPER
        // =====================================================================

        /// <summary>
        /// Spawns rainbow sparkle dust orbiting a Swan Lake projectile.
        /// Call from AI() each frame for Swan Lake orbs.
        /// </summary>
        public static void SpawnSwanLakeRainbowSparkles(Projectile proj, float orbitRadius = 16f)
        {
            if (Main.dedServ || !Main.rand.NextBool(3)) return;

            float time = (float)Main.timeForVisualEffects * 0.05f + proj.whoAmI * 1.3f;
            float angle = time + Main.rand.NextFloat(-0.3f, 0.3f);
            Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * orbitRadius;

            // Rainbow color cycling
            float hueT = (time * 0.4f + Main.rand.NextFloat(0.2f)) % 1f;
            Color rainbow = Main.hslToRgb(hueT, 1f, 0.7f);

            Dust sparkle = Dust.NewDustPerfect(
                proj.Center + offset,
                DustID.RainbowMk2,
                Vector2.Zero,
                newColor: rainbow,
                Scale: Main.rand.NextFloat(0.3f, 0.5f));
            sparkle.noGravity = true;
            sparkle.fadeIn = 0.5f;
            sparkle.velocity = offset.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2) * 0.5f;
        }
    }
}
