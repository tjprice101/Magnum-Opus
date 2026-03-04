using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.DiesIrae;
using MagnumOpus.Content.DiesIrae.Weapons.HarmonyOfJudgement.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.HarmonyOfJudgement.Utilities
{
    /// <summary>
    /// Static VFX utility for Harmony of Judgement.
    /// Judgment sigil body rendering (RadialNoiseMaskShader with VoronoiCell + StarField),
    /// phase-driven beam rendering (ThinBeamShader), execute burst (RippleShader),
    /// harmonized verdict X (XSlashShader), and judgment aura.
    /// </summary>
    public static class HarmonyOfJudgementUtils
    {
        // ═══════════════════════════════════════════════════════
        //  PALETTE SHORTCUTS
        // ═══════════════════════════════════════════════════════
        public static Color ScanCrimson => DiesIraePalette.BloodRed;
        public static Color JudgeGold => DiesIraePalette.JudgmentGold;
        public static Color ExecuteWhite => DiesIraePalette.WrathWhite;
        public static Color SigilBody => DiesIraePalette.InfernalRed;
        public static Color SigilCore => DiesIraePalette.EmberOrange;
        public static Color AuraGold => new Color(200, 170, 50, 80);
        public static Color HarmonizedGold => new Color(255, 220, 100);

        // ═══════════════════════════════════════════════════════
        //  SHADER CACHE
        // ═══════════════════════════════════════════════════════
        private static Effect _maskShader;
        private static Effect _thinBeamShader;
        private static Effect _rippleShader;
        private static Effect _xSlashShader;

        // Textures loaded lazily
        private static Asset<Texture2D> _diGradient;
        private static Asset<Texture2D> _softCircle;
        private static Asset<Texture2D> _xImpactCross;

        public static Effect GetMaskShader()
        {
            if (_maskShader == null)
            {
                try
                {
                    _maskShader = ModContent.Request<Effect>(
                        "MagnumOpus/Content/FoundationWeapons/MaskFoundation/Shaders/RadialNoiseMaskShader",
                        AssetRequestMode.ImmediateLoad).Value;
                }
                catch { }
            }
            return _maskShader;
        }

        public static Effect GetThinBeamShader()
        {
            if (_thinBeamShader == null)
            {
                try
                {
                    _thinBeamShader = ModContent.Request<Effect>(
                        "MagnumOpus/Content/FoundationWeapons/ThinLaserFoundation/Shaders/ThinBeamShader",
                        AssetRequestMode.ImmediateLoad).Value;
                }
                catch { }
            }
            return _thinBeamShader;
        }

        public static Effect GetRippleShader()
        {
            if (_rippleShader == null)
            {
                try
                {
                    _rippleShader = ModContent.Request<Effect>(
                        "MagnumOpus/Content/FoundationWeapons/ImpactFoundation/Shaders/RippleShader",
                        AssetRequestMode.ImmediateLoad).Value;
                }
                catch { }
            }
            return _rippleShader;
        }

        public static Effect GetXSlashShader()
        {
            if (_xSlashShader == null)
            {
                try
                {
                    _xSlashShader = ModContent.Request<Effect>(
                        "MagnumOpus/Content/FoundationWeapons/XSlashFoundation/Shaders/XSlashShader",
                        AssetRequestMode.ImmediateLoad).Value;
                }
                catch { }
            }
            return _xSlashShader;
        }

        public static Texture2D GetDIGradient()
        {
            _diGradient ??= ModContent.Request<Texture2D>(
                "MagnumOpus/Assets/VFX Asset Library/ColorGradients/DiesIraeGradientLUTandRAMP",
                AssetRequestMode.AsyncLoad);
            return _diGradient?.IsLoaded == true ? _diGradient.Value : null;
        }

        public static Texture2D GetSoftCircle()
        {
            _softCircle ??= ModContent.Request<Texture2D>(
                "MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/SoftCircle",
                AssetRequestMode.AsyncLoad);
            return _softCircle?.IsLoaded == true ? _softCircle.Value : null;
        }

        public static Texture2D GetXImpactCross()
        {
            _xImpactCross ??= ModContent.Request<Texture2D>(
                "MagnumOpus/Assets/VFX Asset Library/ImpactEffects/X-ShapedImpactCross",
                AssetRequestMode.AsyncLoad);
            return _xImpactCross?.IsLoaded == true ? _xImpactCross.Value : null;
        }

        // ═══════════════════════════════════════════════════════
        //  EASING HELPERS
        // ═══════════════════════════════════════════════════════
        public static float EaseOutCubic(float t) => 1f - MathF.Pow(1f - t, 3f);
        public static float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);
        public static float SinePulse(float timer, float freq = 0.15f) => 0.85f + 0.15f * MathF.Sin(timer * freq);

        // ═══════════════════════════════════════════════════════
        //  SIGIL BODY RENDERING — RadialNoiseMaskShader
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// Sigil judgment phase: 0 = Idle, 1 = Scan, 2 = Judge, 3 = Execute, 4 = Harmonized
        /// </summary>
        public enum SigilPhase { Idle, Scan, Judge, Execute, Harmonized }

        /// <summary>
        /// Draws the sigil body using RadialNoiseMaskShader with VoronoiCell + StarField noise.
        /// Phase-driven rotation speed and intensity.
        /// </summary>
        public static void DrawSigilBody(SpriteBatch sb, Vector2 worldPos, float timer, SigilPhase phase, float seed = 0f)
        {
            Vector2 drawPos = worldPos - Main.screenPosition;
            Effect shader = GetMaskShader();
            Texture2D voronoi = MagnumTextureRegistry.GetVoronoiNoise();
            Texture2D starField = MagnumTextureRegistry.StarFieldScatter?.IsLoaded == true
                ? MagnumTextureRegistry.StarFieldScatter.Value : null;
            Texture2D gradient = GetDIGradient();
            Texture2D softCircle = GetSoftCircle();
            Texture2D softGlow = MagnumTextureRegistry.GetSoftGlow();
            Texture2D glowOrb = SigilTextures.GlowOrb;

            // Phase-driven parameters
            float rotSpeed, scrollSpeed, intensity;
            Color primary, core;
            GetPhaseParameters(phase, out rotSpeed, out scrollSpeed, out intensity, out primary, out core);

            float pulse = SinePulse(timer, 0.1f);

            // Layer 1: Outer bloom halo
            if (softGlow != null)
            {
                Vector2 origin = softGlow.Size() / 2f;
                sb.Draw(softGlow, drawPos, null, primary * 0.15f * pulse, 0f, origin, 0.35f, SpriteEffects.None, 0f);
                sb.Draw(softGlow, drawPos, null, core * 0.08f, 0f, origin, 0.55f, SpriteEffects.None, 0f);
            }

            // Layer 2: Shader-driven sigil body
            if (shader != null && voronoi != null && softCircle != null)
            {
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearWrap,
                    DepthStencilState.None, RasterizerState.CullNone, shader, Main.GameViewMatrix.ZoomMatrix);

                float time = (float)Main.timeForVisualEffects * 0.015f + seed;

                shader.Parameters["uTime"]?.SetValue(time);
                shader.Parameters["scrollSpeed"]?.SetValue(scrollSpeed);
                shader.Parameters["rotationSpeed"]?.SetValue(rotSpeed);
                shader.Parameters["circleRadius"]?.SetValue(0.32f);
                shader.Parameters["edgeSoftness"]?.SetValue(0.04f);
                shader.Parameters["intensity"]?.SetValue(intensity);
                shader.Parameters["primaryColor"]?.SetValue(primary.ToVector3());
                shader.Parameters["coreColor"]?.SetValue(core.ToVector3());
                shader.Parameters["noiseTex"]?.SetValue(voronoi);
                if (gradient != null)
                    shader.Parameters["gradientTex"]?.SetValue(gradient);

                Vector2 circOrigin = softCircle.Size() / 2f;
                sb.Draw(softCircle, drawPos, null, Color.White, 0f, circOrigin, 0.6f, SpriteEffects.None, 0f);

                sb.End();

                // Secondary StarField layer at 40% intensity
                if (starField != null)
                {
                    sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearWrap,
                        DepthStencilState.None, RasterizerState.CullNone, shader, Main.GameViewMatrix.ZoomMatrix);

                    shader.Parameters["intensity"]?.SetValue(intensity * 0.4f);
                    shader.Parameters["rotationSpeed"]?.SetValue(rotSpeed * -0.7f);
                    shader.Parameters["noiseTex"]?.SetValue(starField);

                    sb.Draw(softCircle, drawPos, null, Color.White, 0f, circOrigin, 0.55f, SpriteEffects.None, 0f);

                    sb.End();
                }

                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
            }

            // Layer 3: Core bloom
            if (glowOrb != null)
            {
                Vector2 origin = glowOrb.Size() / 2f;
                Color coreBloom = phase == SigilPhase.Harmonized ? HarmonizedGold : core;
                sb.Draw(glowOrb, drawPos, null, coreBloom * 0.4f * pulse, 0f, origin, 0.15f, SpriteEffects.None, 0f);
                sb.Draw(glowOrb, drawPos, null, Color.White * 0.15f, 0f, origin, 0.08f, SpriteEffects.None, 0f);
            }
        }

        private static void GetPhaseParameters(SigilPhase phase, out float rotSpeed, out float scrollSpeed,
            out float intensity, out Color primary, out Color core)
        {
            switch (phase)
            {
                case SigilPhase.Scan:
                    rotSpeed = 0.40f; scrollSpeed = 0.15f; intensity = 2.2f;
                    primary = ScanCrimson; core = SigilCore;
                    break;
                case SigilPhase.Judge:
                    rotSpeed = 0.35f; scrollSpeed = 0.20f; intensity = 2.5f;
                    primary = Color.Lerp(ScanCrimson, JudgeGold, 0.6f); core = JudgeGold;
                    break;
                case SigilPhase.Execute:
                    rotSpeed = 0.50f; scrollSpeed = 0.25f; intensity = 3.0f;
                    primary = JudgeGold; core = ExecuteWhite;
                    break;
                case SigilPhase.Harmonized:
                    rotSpeed = 0.50f; scrollSpeed = 0.20f; intensity = 2.8f;
                    primary = JudgeGold; core = HarmonizedGold;
                    break;
                default: // Idle
                    rotSpeed = 0.20f; scrollSpeed = 0.10f; intensity = 1.8f;
                    primary = ScanCrimson; core = SigilCore;
                    break;
            }
        }

        // ═══════════════════════════════════════════════════════
        //  JUDGMENT BEAM RENDERING — ThinBeamShader
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// Draws a judgment beam from source to target using ThinBeamShader.
        /// Width and color shift based on phase.
        /// </summary>
        public static void DrawJudgmentBeam(SpriteBatch sb, Vector2 start, Vector2 end,
            SigilPhase phase, float beamAlpha = 1f)
        {
            Texture2D softGlow = MagnumTextureRegistry.GetSoftGlow();
            Texture2D beamTex = SigilTextures.BeamStreak ?? SigilTextures.ThinGlow;

            if (beamTex == null) return;

            Vector2 drawStart = start - Main.screenPosition;
            Vector2 drawEnd = end - Main.screenPosition;
            Vector2 dir = drawEnd - drawStart;
            float dist = dir.Length();
            if (dist < 1f) return;
            float angle = dir.ToRotation();

            // Phase-driven beam params
            float width;
            Color beamColor;
            GetBeamParams(phase, out width, out beamColor);

            // Draw beam strip as stretched texture
            Vector2 beamOrigin = new Vector2(0, beamTex.Height / 2f);
            Vector2 scale = new Vector2(dist / beamTex.Width, width / beamTex.Height);

            sb.Draw(beamTex, drawStart, null, beamColor * beamAlpha * 0.7f,
                angle, beamOrigin, scale, SpriteEffects.None, 0f);

            // Inner core beam (brighter, thinner)
            Vector2 coreScale = new Vector2(dist / beamTex.Width, width * 0.5f / beamTex.Height);
            sb.Draw(beamTex, drawStart, null, Color.Lerp(beamColor, ExecuteWhite, 0.5f) * beamAlpha * 0.5f,
                angle, beamOrigin, coreScale, SpriteEffects.None, 0f);

            // Endpoint flares
            if (softGlow != null)
            {
                Vector2 flareOrigin = softGlow.Size() / 2f;
                float flareScale = width * 0.005f;
                sb.Draw(softGlow, drawStart, null, beamColor * beamAlpha * 0.4f, 0f, flareOrigin, flareScale, SpriteEffects.None, 0f);
                sb.Draw(softGlow, drawEnd, null, beamColor * beamAlpha * 0.6f, 0f, flareOrigin, flareScale * 1.3f, SpriteEffects.None, 0f);
            }

            // Beam sparkle particles
            if (Main.rand.NextBool(3))
            {
                Vector2 sparkPos = Vector2.Lerp(start, end, Main.rand.NextFloat());
                Vector2 sparkVel = Main.rand.NextVector2Circular(1f, 1f);
                SigilParticleHandler.Spawn(new JudgmentBeamSparkParticle(sparkPos, sparkVel, beamColor, 0.02f, 15));
            }
        }

        private static void GetBeamParams(SigilPhase phase, out float width, out Color color)
        {
            switch (phase)
            {
                case SigilPhase.Scan:
                    width = 4f; color = ScanCrimson;
                    break;
                case SigilPhase.Judge:
                    width = 6f; color = JudgeGold;
                    break;
                case SigilPhase.Execute:
                    width = 10f; color = ExecuteWhite;
                    break;
                case SigilPhase.Harmonized:
                    width = 10f; color = HarmonizedGold;
                    break;
                default:
                    width = 0f; color = Color.Transparent;
                    break;
            }
        }

        // ═══════════════════════════════════════════════════════
        //  EXECUTE BURST — RippleShader 4-ring burst
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// Spawns execute burst particles at target position — expanding crimson-gold rings.
        /// </summary>
        public static void DoExecuteBurst(Vector2 worldPos, bool harmonized = false)
        {
            Color burstColor = harmonized ? HarmonizedGold : JudgeGold;

            // 4 expanding ring particles with staggered timing
            for (int i = 0; i < 4; i++)
            {
                float startScale = 0.05f + i * 0.02f;
                float endScale = 0.35f + i * 0.12f;
                int delay = i * 3;
                Color ringColor = Color.Lerp(DiesIraePalette.InfernalRed, burstColor, i / 3f);

                // Delayed spawn via lifetime offset
                var particle = new ExecuteBurstParticle(worldPos, ringColor, startScale, endScale, 25 + delay);
                SigilParticleHandler.Spawn(particle);
            }

            // Central flash
            SigilParticleHandler.Spawn(new SigilGlowParticle(worldPos, ExecuteWhite, 0.15f, 12));

            // Scatter sparks
            for (int i = 0; i < 8; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(3f, 3f);
                Color sparkColor = Color.Lerp(DiesIraePalette.InfernalRed, burstColor, Main.rand.NextFloat());
                SigilParticleHandler.Spawn(new JudgmentBeamSparkParticle(
                    worldPos, sparkVel, sparkColor, 0.025f, 20));
            }

            // Harmonized X overlay
            if (harmonized)
            {
                SigilParticleHandler.Spawn(new HarmonizedXParticle(worldPos, 0.22f, 46));
            }
        }

        // ═══════════════════════════════════════════════════════
        //  JUDGMENT AURA — Faint passive gold ring
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// Draws the passive judgment aura around the sigil — faint gold ring.
        /// </summary>
        public static void DrawJudgmentAura(SpriteBatch sb, Vector2 worldPos, float timer)
        {
            Texture2D softGlow = MagnumTextureRegistry.GetSoftGlow();
            if (softGlow == null) return;

            Vector2 drawPos = worldPos - Main.screenPosition;
            Vector2 origin = softGlow.Size() / 2f;
            float pulse = 0.85f + 0.15f * MathF.Sin(timer * 0.08f);
            float radius = 80f * 0.01f; // 5-tile radius scaled to draw

            // Very faint gold ring
            sb.Draw(softGlow, drawPos, null, AuraGold * 0.08f * pulse, 0f, origin, radius, SpriteEffects.None, 0f);
            sb.Draw(softGlow, drawPos, null, JudgeGold * 0.03f, 0f, origin, radius * 1.3f, SpriteEffects.None, 0f);
        }

        // ═══════════════════════════════════════════════════════
        //  AMBIENT SIGIL PARTICLES
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// Spawns ambient glow particles around the sigil.
        /// </summary>
        public static void DoAmbientParticles(Vector2 worldPos, SigilPhase phase)
        {
            Color color = phase switch
            {
                SigilPhase.Scan => ScanCrimson,
                SigilPhase.Judge => JudgeGold,
                SigilPhase.Execute => ExecuteWhite,
                SigilPhase.Harmonized => HarmonizedGold,
                _ => ScanCrimson
            };

            if (Main.rand.NextBool(4))
            {
                Vector2 offset = Main.rand.NextVector2Circular(20f, 20f);
                SigilParticleHandler.Spawn(new SigilGlowParticle(
                    worldPos + offset, color * 0.6f, Main.rand.NextFloat(0.02f, 0.04f), 30));
            }
        }

        // ─────────── THEME TEXTURE ACCENTS ───────────

        /// <summary>
        /// Draws theme-textured sigil accents. Call under Additive blend.
        /// </summary>
        public static void DrawThemeAccents(SpriteBatch sb, Vector2 worldPos, float scale, float intensity = 1f)
        {
            DiesIraeVFXLibrary.DrawThemeStarFlare(sb, worldPos, scale, intensity * 0.5f);
            float rot = (float)Main.GameUpdateCount * 0.025f;
            DiesIraeVFXLibrary.DrawThemeImpactRing(sb, worldPos, scale, intensity * 0.45f, rot);
            DiesIraeVFXLibrary.DrawThemeRadialSlash(sb, worldPos, scale * 0.6f, intensity * 0.3f);
        }
    }
}
