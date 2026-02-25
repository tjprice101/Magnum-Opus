using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Bloom;
using MagnumOpus.Common.Systems.VFX.Screen;
using MagnumOpus.Common.Systems.Shaders;
using MagnumOpus.Common.Systems.Particles;
using static MagnumOpus.Common.Systems.VFX.GodRaySystem;

namespace MagnumOpus.Content.OdeToJoy
{
    /// <summary>
    /// Shared Ode to Joy VFX library — canonical palette, bloom stacking,
    /// shader setup, trail helpers, music notes, dust, and impact VFX used by
    /// ALL Ode to Joy weapons, accessories, projectiles, minions, and enemies.
    ///
    /// Theme identity: Blossoming nature, joyous celebration, golden radiance.
    /// Verdant green growth, rose pink petals, golden pollen, sunlit warmth.
    ///
    /// ALL Ode to Joy content should call these methods instead of
    /// OdeToJoyVFX (in OdeToJoyProjectiles.cs) or any deprecated VFX API.
    /// </summary>
    public static class OdeToJoyVFXLibrary
    {
        // ─────────── CANONICAL PALETTE (delegates to OdeToJoyPalette) ───────────
        // 6-colour musical dynamic scale (pianissimo → sforzando)
        public static readonly Color DeepForest     = OdeToJoyPalette.DeepForest;     // [0] Pianissimo
        public static readonly Color VerdantGreen   = OdeToJoyPalette.VerdantGreen;   // [1] Piano
        public static readonly Color RosePink       = OdeToJoyPalette.RosePink;       // [2] Mezzo
        public static readonly Color GoldenPollen   = OdeToJoyPalette.GoldenPollen;   // [3] Forte
        public static readonly Color SunlightYellow = OdeToJoyPalette.SunlightYellow; // [4] Fortissimo
        public static readonly Color WhiteBloom     = OdeToJoyPalette.WhiteBloom;     // [5] Sforzando

        // Extended convenience
        public static readonly Color LeafGreen  = OdeToJoyPalette.LeafGreen;
        public static readonly Color PetalPink  = OdeToJoyPalette.PetalPink;
        public static readonly Color BudGreen   = OdeToJoyPalette.BudGreen;
        public static readonly Color WarmAmber  = OdeToJoyPalette.WarmAmber;
        public static readonly Color PollenGold = OdeToJoyPalette.PollenGold;
        public static readonly Color MossShadow = OdeToJoyPalette.MossShadow;

        // Palette as array for indexed access
        private static readonly Color[] Palette = {
            OdeToJoyPalette.DeepForest, OdeToJoyPalette.VerdantGreen, OdeToJoyPalette.RosePink,
            OdeToJoyPalette.GoldenPollen, OdeToJoyPalette.SunlightYellow, OdeToJoyPalette.WhiteBloom
        };

        // Hue range for HueShiftingMusicNoteParticle (green→gold band)
        private const float HueMin = 0.22f;
        private const float HueMax = 0.18f; // wraps through gold
        private const float NoteSaturation = 0.85f;
        private const float NoteLuminosity = 0.55f;

        // Petal hue range for rose-themed notes
        private const float PetalHueMin = 0.90f;
        private const float PetalHueMax = 0.98f;

        // Glow profile for GlowRenderer
        public static readonly GlowRenderer.GlowLayer[] OdeToJoyGlowProfile = new[]
        {
            new GlowRenderer.GlowLayer(1.0f, 1.0f, Color.White),
            new GlowRenderer.GlowLayer(1.6f, 0.65f, OdeToJoyPalette.GoldenPollen),
            new GlowRenderer.GlowLayer(2.5f, 0.4f, OdeToJoyPalette.RosePink),
            new GlowRenderer.GlowLayer(4.0f, 0.2f, OdeToJoyPalette.VerdantGreen)
        };

        // ─────────── PALETTE INTERPOLATION ───────────

        /// <summary>
        /// Lerp through the 6-colour Ode to Joy palette. t=0 → DeepForest, t=1 → WhiteBloom.
        /// </summary>
        public static Color GetPaletteColor(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            float scaled = t * (Palette.Length - 1);
            int idx = (int)scaled;
            int next = Math.Min(idx + 1, Palette.Length - 1);
            return Color.Lerp(Palette[idx], Palette[next], scaled - idx);
        }

        /// <summary>
        /// Palette colour with white push for perceived brilliance.
        /// push=0 returns pure palette, push=1 returns full white.
        /// Typical usage: push 0.35-0.55 for trail/bloom cores.
        /// </summary>
        public static Color GetPaletteColorWithWhitePush(float t, float push)
        {
            Color baseCol = GetPaletteColor(t);
            return Color.Lerp(baseCol, Color.White, MathHelper.Clamp(push, 0f, 1f));
        }

        // ─────────── SPRITEBATCH STATE HELPERS ───────────

        /// <summary>
        /// Switch SpriteBatch to additive blend for Ode to Joy VFX rendering.
        /// Call EndOdeToJoyAdditive when done.
        /// </summary>
        public static void BeginOdeToJoyAdditive(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Restore SpriteBatch to standard AlphaBlend after additive rendering.
        /// </summary>
        public static void EndOdeToJoyAdditive(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);
        }

        // ─────────── BLOOM STACKING ({A=0} PATTERN) ───────────

        /// <summary>
        /// 4-layer bloom stack using {A=0} premultiplied alpha trick.
        /// Renders additively under AlphaBlend without SpriteBatch restart.
        /// paletteT: 0=deep forest, 0.3=verdant, 0.5=pink, 0.7=gold, 1=white
        /// </summary>
        public static void DrawOdeToJoyBloomStack(SpriteBatch sb, Vector2 worldPos,
            float scale, float paletteT = 0.3f, float opacity = 1f)
        {
            Texture2D bloom = MagnumTextureRegistry.GetBloom();
            if (bloom == null) return;

            Vector2 drawPos = worldPos - Main.screenPosition;
            Vector2 origin = bloom.Size() * 0.5f;

            Color outer = GetPaletteColor(MathHelper.Clamp(paletteT - 0.15f, 0f, 1f));
            Color inner = GetPaletteColor(MathHelper.Clamp(paletteT + 0.2f, 0f, 1f));

            // Layer 1: Outer halo (Verdant green)
            sb.Draw(bloom, drawPos, null,
                (outer with { A = 0 }) * 0.3f * opacity, 0f, origin, scale * 2.0f, SpriteEffects.None, 0f);

            // Layer 2: Mid glow (Rose pink)
            sb.Draw(bloom, drawPos, null,
                (outer with { A = 0 }) * 0.5f * opacity, 0f, origin, scale * 1.4f, SpriteEffects.None, 0f);

            // Layer 3: Inner bloom (Golden pollen)
            sb.Draw(bloom, drawPos, null,
                (inner with { A = 0 }) * 0.7f * opacity, 0f, origin, scale * 0.9f, SpriteEffects.None, 0f);

            // Layer 4: White-hot core
            sb.Draw(bloom, drawPos, null,
                (Color.White with { A = 0 }) * 0.85f * opacity, 0f, origin, scale * 0.4f, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Two-color bloom stack using {A=0} pattern.
        /// </summary>
        public static void DrawOdeToJoyBloomStack(SpriteBatch sb, Vector2 worldPos,
            Color outerColor, Color innerColor, float scale, float opacity = 1f)
        {
            Texture2D bloom = MagnumTextureRegistry.GetBloom();
            if (bloom == null) return;

            Vector2 drawPos = worldPos - Main.screenPosition;
            Vector2 origin = bloom.Size() * 0.5f;

            sb.Draw(bloom, drawPos, null,
                (outerColor with { A = 0 }) * 0.3f * opacity, 0f, origin, scale * 2.0f, SpriteEffects.None, 0f);
            sb.Draw(bloom, drawPos, null,
                (outerColor with { A = 0 }) * 0.5f * opacity, 0f, origin, scale * 1.4f, SpriteEffects.None, 0f);
            sb.Draw(bloom, drawPos, null,
                (innerColor with { A = 0 }) * 0.7f * opacity, 0f, origin, scale * 0.9f, SpriteEffects.None, 0f);
            sb.Draw(bloom, drawPos, null,
                (Color.White with { A = 0 }) * 0.85f * opacity, 0f, origin, scale * 0.4f, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Bloom sandwich layer — renders bloom BEHIND a projectile body for depth.
        /// Call before drawing the projectile sprite, then call again after for front glow.
        /// </summary>
        public static void DrawBloomSandwichLayer(SpriteBatch sb, Vector2 worldPos,
            float scale, float opacity, bool isFrontLayer)
        {
            Texture2D bloom = MagnumTextureRegistry.GetBloom();
            if (bloom == null) return;

            Vector2 drawPos = worldPos - Main.screenPosition;
            Vector2 origin = bloom.Size() * 0.5f;

            if (!isFrontLayer)
            {
                // Behind layer: larger, softer, verdant green
                sb.Draw(bloom, drawPos, null,
                    (VerdantGreen with { A = 0 }) * 0.25f * opacity, 0f, origin, scale * 2.5f, SpriteEffects.None, 0f);
                sb.Draw(bloom, drawPos, null,
                    (RosePink with { A = 0 }) * 0.35f * opacity, 0f, origin, scale * 1.6f, SpriteEffects.None, 0f);
            }
            else
            {
                // Front layer: smaller, brighter, Golden → White
                sb.Draw(bloom, drawPos, null,
                    (GoldenPollen with { A = 0 }) * 0.5f * opacity, 0f, origin, scale * 0.8f, SpriteEffects.None, 0f);
                sb.Draw(bloom, drawPos, null,
                    (Color.White with { A = 0 }) * 0.7f * opacity, 0f, origin, scale * 0.35f, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Counter-rotating double flare — two bloom textures spinning in opposite directions.
        /// Creates dynamic pollen energy appearance at projectile centers.
        /// </summary>
        public static void DrawCounterRotatingFlares(SpriteBatch sb, Vector2 worldPos,
            float scale, float time, float opacity = 1f)
        {
            Texture2D flare = MagnumTextureRegistry.GetFlare();
            if (flare == null) return;

            Vector2 drawPos = worldPos - Main.screenPosition;
            Vector2 origin = flare.Size() * 0.5f;

            float rot1 = time * 2.5f;
            float rot2 = -time * 1.8f;

            sb.Draw(flare, drawPos, null,
                (GoldenPollen with { A = 0 }) * 0.6f * opacity, rot1, origin, scale * 0.7f, SpriteEffects.None, 0f);
            sb.Draw(flare, drawPos, null,
                (RosePink with { A = 0 }) * 0.5f * opacity, rot2, origin, scale * 0.5f, SpriteEffects.None, 0f);
        }

        // ─────────── SELF-CONTAINED BLOOM ───────────

        /// <summary>
        /// Standard Ode to Joy bloom at a petal tip or projectile centre.
        /// Uses BloomRenderer for self-contained SpriteBatch management.
        /// </summary>
        public static void DrawBloom(Vector2 worldPos, float scale, float opacity = 1f)
        {
            BloomRenderer.DrawBloomStackAdditive(worldPos, VerdantGreen, GoldenPollen, scale, opacity);
        }

        /// <summary>
        /// Combo-step-aware bloom (bigger + brighter on later hits).
        /// </summary>
        public static void DrawComboBloom(Vector2 worldPos, int comboStep, float baseScale = 0.4f, float opacity = 1f)
        {
            float scale = baseScale + comboStep * 0.08f;
            DrawBloom(worldPos, scale, opacity);
        }

        // ─────────── GLOW RENDERER INTEGRATION ───────────

        /// <summary>
        /// Draw Ode to Joy-themed multi-layer glow via GlowRenderer.
        /// Caller must have SpriteBatch in additive blend.
        /// </summary>
        public static void DrawOdeToJoyGlow(SpriteBatch sb, Vector2 worldPos,
            float scale = 1f, float intensity = 1f, string rotationId = null)
        {
            GlowRenderer.DrawGlow(sb, worldPos, OdeToJoyGlowProfile, VerdantGreen, intensity * scale, rotationId);
        }

        /// <summary>
        /// Draw Ode to Joy glow with automatic SpriteBatch state management.
        /// </summary>
        public static void DrawOdeToJoyGlowManaged(SpriteBatch sb, Vector2 worldPos,
            float scale = 1f, float intensity = 1f, string rotationId = null)
        {
            GlowRenderer.DrawGlowManaged(sb, worldPos, OdeToJoyGlowProfile, VerdantGreen, intensity * scale, rotationId);
        }

        // ─────────── SHADER SETUP HELPERS ───────────

        /// <summary>
        /// Configure ScrollingTrailShader for garden-themed trail rendering.
        /// Uses flowing green-to-gold gradient with nature distortion.
        /// (Fallback to generic shader until Ode-specific shaders are authored.)
        /// </summary>
        public static void ApplyGardenTrailShader(float time, Color primary, Color secondary,
            float scrollSpeed = 1.0f, float distortionAmt = 0.06f, float overbrightMult = 2.5f)
        {
            Effect shader = ShaderLoader.ScrollingTrail;
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(1.3f);
            shader.Parameters["uOverbrightMult"]?.SetValue(overbrightMult);
            shader.Parameters["uScrollSpeed"]?.SetValue(scrollSpeed);
            shader.Parameters["uDistortionAmt"]?.SetValue(distortionAmt);
            shader.Parameters["uHasSecondaryTex"]?.SetValue(0f);
            shader.Parameters["uSecondaryTexScale"]?.SetValue(3f);
            shader.Parameters["uSecondaryTexScroll"]?.SetValue(0.5f);

            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>
        /// Configure ScrollingTrailShader with noise texture for richer garden distortion.
        /// </summary>
        public static void ApplyGardenTrailShaderWithNoise(float time, Color primary, Color secondary,
            float scrollSpeed = 1.0f, float distortionAmt = 0.06f, float overbrightMult = 2.5f,
            float noiseScale = 3f, float noiseScroll = 0.5f)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("TileableFBMNoise");
            if (noise == null)
                noise = ShaderLoader.GetNoiseTexture("PerlinNoise");
            if (noise != null)
            {
                Main.graphics.GraphicsDevice.Textures[1] = noise;
                Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;
            }

            Effect shader = ShaderLoader.ScrollingTrail;
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(1.3f);
            shader.Parameters["uOverbrightMult"]?.SetValue(overbrightMult);
            shader.Parameters["uScrollSpeed"]?.SetValue(scrollSpeed);
            shader.Parameters["uDistortionAmt"]?.SetValue(distortionAmt);
            shader.Parameters["uHasSecondaryTex"]?.SetValue(noise != null ? 1f : 0f);
            shader.Parameters["uSecondaryTexScale"]?.SetValue(noiseScale);
            shader.Parameters["uSecondaryTexScroll"]?.SetValue(noiseScroll);

            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>
        /// Configure CelestialValorTrail shader tuned for Ode to Joy garden effects.
        /// </summary>
        public static void ApplyGardenCelestialTrail(float time, Color primary, Color secondary,
            float scrollSpeed = 0.8f, float overbrightMult = 2.5f)
        {
            Effect shader = ShaderLoader.CelestialValorTrail;
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(1.3f);
            shader.Parameters["uOverbrightMult"]?.SetValue(overbrightMult);

            shader.CurrentTechnique.Passes[0].Apply();
        }

        // ─────────── TRAIL WIDTH/COLOR FUNCTIONS ───────────

        /// <summary>
        /// Standard garden trail width: wide at head, tapers to tail. 12f base.
        /// Slightly softer than Eroica's heroic width for a more organic feel.
        /// </summary>
        public static float GardenTrailWidth(float completionRatio, float baseWidth = 12f)
        {
            float tipFade = MathF.Pow(1f - completionRatio, 0.7f);
            float headFade = MathF.Pow(completionRatio, 3f);
            return baseWidth * tipFade * (1f - headFade);
        }

        /// <summary>
        /// Thorn trail width — sharp, thin, pointed. 8f base.
        /// </summary>
        public static float ThornTrailWidth(float completionRatio, float baseWidth = 8f)
        {
            float taper = 1f - completionRatio;
            return baseWidth * taper * taper;
        }

        /// <summary>
        /// Vine trail width — organic, slightly undulating. 14f base.
        /// </summary>
        public static float VineTrailWidth(float completionRatio, float baseWidth = 14f)
        {
            float tipFade = MathF.Pow(1f - completionRatio, 0.5f);
            float wave = 1f + MathF.Sin(completionRatio * MathHelper.TwoPi * 2f) * 0.1f;
            return baseWidth * tipFade * wave;
        }

        /// <summary>
        /// Garden trail color: VerdantGreen → GoldenPollen with {A=0} for additive rendering.
        /// </summary>
        public static Color GardenTrailColor(float completionRatio, float whitePush = 0.35f)
        {
            Color baseCol = Color.Lerp(VerdantGreen, GoldenPollen, completionRatio * 0.8f);
            baseCol = Color.Lerp(baseCol, Color.White, whitePush * (1f - completionRatio));
            float fade = 1f - MathF.Pow(completionRatio, 1.5f);
            return (baseCol * fade) with { A = 0 };
        }

        /// <summary>
        /// Petal trail color: RosePink → PollenGold with {A=0}.
        /// </summary>
        public static Color PetalTrailColor(float completionRatio, float whitePush = 0.35f)
        {
            Color baseCol = Color.Lerp(RosePink, PollenGold, completionRatio * 0.7f);
            baseCol = Color.Lerp(baseCol, Color.White, whitePush * (1f - completionRatio));
            float fade = 1f - MathF.Pow(completionRatio, 1.5f);
            return (baseCol * fade) with { A = 0 };
        }

        /// <summary>
        /// Vine trail color: LeafGreen → VerdantGreen → GoldenPollen with {A=0}.
        /// </summary>
        public static Color VineTrailColor(float completionRatio, float whitePush = 0.30f)
        {
            Color baseCol = Color.Lerp(LeafGreen, VerdantGreen, completionRatio * 0.5f);
            baseCol = Color.Lerp(baseCol, GoldenPollen, MathF.Max(0, completionRatio - 0.5f) * 1.5f);
            baseCol = Color.Lerp(baseCol, Color.White, whitePush * (1f - completionRatio));
            float fade = 1f - MathF.Pow(completionRatio, 1.8f);
            return (baseCol * fade) with { A = 0 };
        }

        /// <summary>
        /// Returns shader gradient pairs for multi-pass rendering.
        /// Pass 0: LeafGreen → VerdantGreen, Pass 1: VerdantGreen → GoldenPollen, Pass 2: GoldenPollen → WhiteBloom
        /// </summary>
        public static (Vector3 primary, Vector3 secondary) GetShaderGradient(int passIndex)
        {
            return passIndex switch
            {
                0 => (LeafGreen.ToVector3(), VerdantGreen.ToVector3()),
                1 => (VerdantGreen.ToVector3(), GoldenPollen.ToVector3()),
                2 => (GoldenPollen.ToVector3(), WhiteBloom.ToVector3()),
                _ => (VerdantGreen.ToVector3(), GoldenPollen.ToVector3()),
            };
        }

        // ─────────── MUSIC NOTES ───────────

        /// <summary>
        /// Spawn visible, hue-shifting Ode to Joy music notes at the given position.
        /// Notes use the canonical green→gold hue band and are spawned
        /// at scale 0.7f+ so they are clearly visible.
        /// </summary>
        public static void SpawnMusicNotes(Vector2 pos, int count = 3, float spread = 20f,
            float minScale = 0.7f, float maxScale = 1.0f, int lifetime = 35)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(spread, spread);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), -1.5f - Main.rand.NextFloat(1.5f));
                float scale = Main.rand.NextFloat(minScale, maxScale);

                var note = new HueShiftingMusicNoteParticle(
                    pos + offset, vel,
                    HueMin, HueMax,
                    NoteSaturation, NoteLuminosity,
                    scale, lifetime
                );
                MagnumParticleHandler.SpawnParticle(note);
            }
        }

        /// <summary>
        /// Spawn a single music note with precise control.
        /// </summary>
        public static void SpawnMusicNote(Vector2 pos, Vector2 vel, Color color,
            float scale = 0.22f, int lifetime = 45)
        {
            float hue = 0.25f; // Default green-gold midpoint
            if (color.G > 150 && color.R < 150) hue = 0.33f; // Green
            else if (color.G > 200 && color.R > 200) hue = 0.15f; // Gold
            else if (color.R > 200 && color.G < 200) hue = 0.95f; // Pink

            var note = new HueShiftingMusicNoteParticle(
                pos, vel,
                MathHelper.Clamp(hue - 0.03f, 0f, 1f),
                MathHelper.Clamp(hue + 0.03f, 0f, 1f),
                NoteSaturation, NoteLuminosity,
                Math.Max(scale, 0.7f), lifetime
            );
            MagnumParticleHandler.SpawnParticle(note);
        }

        /// <summary>
        /// Radial burst of music notes — notes fly outward in all directions.
        /// </summary>
        public static void MusicNoteBurst(Vector2 pos, Color color, int count = 12, float speed = 4f)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(speed * 0.6f, speed);
                SpawnMusicNote(pos, vel, color, Main.rand.NextFloat(0.7f, 1.0f), 35);
            }
        }

        /// <summary>
        /// Spawn orbiting music notes locked to a centre point.
        /// </summary>
        public static void SpawnOrbitingNotes(Vector2 centre, Vector2 hostVelocity,
            int noteCount = 3, float orbitRadius = 15f, float baseAngle = 0f)
        {
            for (int i = 0; i < noteCount; i++)
            {
                float angle = baseAngle + MathHelper.TwoPi * i / noteCount;
                Vector2 notePos = centre + angle.ToRotationVector2() * orbitRadius;
                Vector2 vel = hostVelocity * 0.8f;
                float scale = Main.rand.NextFloat(0.7f, 0.9f);

                var note = new HueShiftingMusicNoteParticle(
                    notePos, vel,
                    HueMin, HueMax,
                    NoteSaturation, NoteLuminosity,
                    scale, 30
                );
                MagnumParticleHandler.SpawnParticle(note);
            }
        }

        /// <summary>
        /// Spawn petal-hue music notes (pink/rose range instead of green/gold).
        /// </summary>
        public static void SpawnPetalMusicNotes(Vector2 pos, int count = 3, float spread = 20f)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(spread, spread);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-1f, 1f), -1.2f - Main.rand.NextFloat(1f));
                float scale = Main.rand.NextFloat(0.7f, 0.95f);

                var note = new HueShiftingMusicNoteParticle(
                    pos + offset, vel,
                    PetalHueMin, PetalHueMax,
                    0.80f, 0.65f,
                    scale, 30
                );
                MagnumParticleHandler.SpawnParticle(note);
            }
        }

        // ─────────── DUST HELPERS ───────────

        /// <summary>
        /// Dense Ode to Joy dust trail at a blade tip during a swing.
        /// Uses green fairy dust for nature-themed trails.
        /// </summary>
        public static void SpawnSwingDust(Vector2 pos, Vector2 awayDirection, int dustType = DustID.GreenFairy)
        {
            for (int i = 0; i < 2; i++)
            {
                Vector2 vel = awayDirection * Main.rand.NextFloat(1f, 3f) + Main.rand.NextVector2Circular(0.5f, 0.5f);
                Color col = GetPaletteColor(Main.rand.NextFloat(0.2f, 0.7f));
                Dust d = Dust.NewDustPerfect(pos, dustType, vel, 0, col, 1.5f);
                d.noGravity = true;
            }
        }

        /// <summary>
        /// Rose petal dust particles — pink/rose floating petals.
        /// </summary>
        public static void SpawnRosePetals(Vector2 pos, int count = 5, float spread = 40f)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(spread, spread);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), -0.5f - Main.rand.NextFloat(1f));
                Color col = Color.Lerp(RosePink, OdeToJoyPalette.RoseWhite, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos + offset, DustID.PinkFairy, vel, 0, col, Main.rand.NextFloat(1.2f, 1.8f));
                d.noGravity = true;
                d.fadeIn = Main.rand.NextFloat(0.8f, 1.2f);
            }
        }

        /// <summary>
        /// Garden aura — pulsing ring of rising leaf particles and golden dust.
        /// </summary>
        public static void SpawnGardenAura(Vector2 center, float radius = 40f)
        {
            for (int i = 0; i < 3; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 pos = center + angle.ToRotationVector2() * Main.rand.NextFloat(radius * 0.5f, radius);
                Vector2 vel = new Vector2(0, -Main.rand.NextFloat(0.5f, 1.5f));
                Color col = Color.Lerp(VerdantGreen, GoldenPollen, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.GreenFairy, vel, 0, col, Main.rand.NextFloat(1.0f, 1.6f));
                d.noGravity = true;
            }
        }

        /// <summary>
        /// Golden pollen sparkle particles.
        /// </summary>
        public static void SpawnPollenSparkles(Vector2 pos, int count = 8, float spread = 30f)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(spread, spread);
                Vector2 vel = Main.rand.NextVector2Circular(1f, 1f);
                Color col = Color.Lerp(GoldenPollen, SunlightYellow, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos + offset, DustID.Enchanted_Gold, vel, 0, col, Main.rand.NextFloat(0.8f, 1.4f));
                d.noGravity = true;
            }
        }

        /// <summary>
        /// Directional thorn sparks — fly in a specific direction with spread.
        /// </summary>
        public static void SpawnDirectionalSparks(Vector2 pos, Vector2 direction, int count = 6, float speed = 6f)
        {
            for (int i = 0; i < count; i++)
            {
                float spreadAngle = Main.rand.NextFloat(-0.5f, 0.5f);
                Vector2 vel = direction.RotatedBy(spreadAngle) * Main.rand.NextFloat(speed * 0.5f, speed);
                Color col = Color.Lerp(VerdantGreen, GoldenPollen, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.GreenFairy, vel, 0, col, Main.rand.NextFloat(1.0f, 1.6f));
                d.noGravity = true;
            }
        }

        /// <summary>
        /// Per-frame vine trail dust — single particle following a projectile.
        /// </summary>
        public static void SpawnVineTrailDust(Vector2 pos, Vector2 velocity)
        {
            Vector2 vel = -velocity * 0.2f + Main.rand.NextVector2Circular(0.5f, 0.5f);
            Color col = Color.Lerp(VerdantGreen, LeafGreen, Main.rand.NextFloat());
            Dust d = Dust.NewDustPerfect(pos, DustID.GreenFairy, vel, 0, col, Main.rand.NextFloat(1.2f, 1.6f));
            d.noGravity = true;
        }

        /// <summary>
        /// Radial dust burst for on-hit / impact VFX.
        /// Uses mixed green fairy and pink fairy dust for garden theme.
        /// </summary>
        public static void SpawnRadialDustBurst(Vector2 pos, int count = 12,
            float speed = 5f, int dustType = DustID.GreenFairy)
        {
            for (int i = 0; i < count; i++)
            {
                float progress = (float)i / count;
                float angle = MathHelper.TwoPi * i / count;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(speed * 0.6f, speed);
                Color col = Color.Lerp(VerdantGreen, GoldenPollen, progress);
                int type = i % 3 == 0 ? DustID.PinkFairy : dustType;
                Dust d = Dust.NewDustPerfect(pos, type, vel, 0, col, 1.3f);
                d.noGravity = true;
            }
        }

        /// <summary>
        /// Contrasting golden pollen sparkle dust — call every other frame for dual-colour trail.
        /// </summary>
        public static void SpawnContrastSparkle(Vector2 pos, Vector2 awayDirection)
        {
            if (!Main.rand.NextBool(2)) return;
            Dust d = Dust.NewDustPerfect(pos, DustID.Enchanted_Gold,
                awayDirection * 0.5f + Main.rand.NextVector2Circular(1.5f, 1.5f), 0, GoldenPollen, 1.0f);
            d.noGravity = true;
        }

        // ─────────── GRADIENT HALO RINGS ───────────

        /// <summary>
        /// Cascading gradient halo rings (VerdantGreen → GoldenPollen).
        /// </summary>
        public static void SpawnGradientHaloRings(Vector2 pos, int count = 5, float baseScale = 0.3f)
        {
            for (int i = 0; i < count; i++)
            {
                float progress = (float)i / count;
                Color ringCol = Color.Lerp(VerdantGreen, GoldenPollen, progress);
                float scale = baseScale + i * 0.12f;
                var ring = new BloomRingParticle(pos, Vector2.Zero, ringCol, scale, 25, 0.08f);
                MagnumParticleHandler.SpawnParticle(ring);
            }
        }

        /// <summary>
        /// Petal-tinted halo rings (RosePink → WhiteBloom).
        /// </summary>
        public static void SpawnPetalHaloRings(Vector2 pos, int count = 4, float baseScale = 0.25f)
        {
            for (int i = 0; i < count; i++)
            {
                float progress = (float)i / count;
                Color ringCol = Color.Lerp(RosePink, WhiteBloom, progress);
                float scale = baseScale + i * 0.10f;
                var ring = new BloomRingParticle(pos, Vector2.Zero, ringCol, scale, 22, 0.07f);
                MagnumParticleHandler.SpawnParticle(ring);
            }
        }

        // ─────────── IMPACTS ───────────

        /// <summary>
        /// Full garden impact VFX — bloom flash, halo cascade,
        /// radial dust burst, and music note scatter. Scales with intensity.
        /// </summary>
        public static void GardenImpact(Vector2 pos, float scale = 1f)
        {
            DrawBloom(pos, 0.5f * scale);

            int rings = (int)(3 + scale * 2);
            SpawnGradientHaloRings(pos, rings, 0.25f * scale);

            int dustCount = (int)(8 + scale * 6);
            SpawnRadialDustBurst(pos, dustCount, 5f * scale);

            int noteCount = (int)(2 + scale);
            SpawnMusicNotes(pos, noteCount, 25f * scale);

            CustomParticles.GenericFlare(pos, GoldenPollen, 0.4f * scale);

            Lighting.AddLight(pos, VerdantGreen.ToVector3() * (0.8f + scale * 0.3f));
        }

        /// <summary>
        /// Blossom impact — rose-tinted version of garden impact.
        /// </summary>
        public static void BlossomImpact(Vector2 pos, float scale = 1f)
        {
            DrawBloom(pos, 0.5f * scale);

            SpawnPetalHaloRings(pos, (int)(3 + scale * 2), 0.25f * scale);

            int dustCount = (int)(6 + scale * 4);
            for (int i = 0; i < dustCount; i++)
            {
                float angle = MathHelper.TwoPi * i / dustCount;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f) * scale;
                Dust d = Dust.NewDustPerfect(pos, DustID.PinkFairy, vel, 0, default, 1.4f);
                d.noGravity = true;
            }

            SpawnPetalMusicNotes(pos, (int)(2 + scale), 20f * scale);
            SpawnRosePetals(pos, (int)(3 * scale), 25f * scale);

            Lighting.AddLight(pos, RosePink.ToVector3() * (0.7f + scale * 0.3f));
        }

        /// <summary>
        /// Expanding shockwave ring with bloom.
        /// </summary>
        public static void Shockwave(Vector2 pos, float scale = 1f)
        {
            var ring = new BloomRingParticle(pos, Vector2.Zero, VerdantGreen, 0.5f * scale, 30, 0.15f * scale);
            MagnumParticleHandler.SpawnParticle(ring);

            var innerRing = new BloomRingParticle(pos, Vector2.Zero, GoldenPollen, 0.3f * scale, 25, 0.10f * scale);
            MagnumParticleHandler.SpawnParticle(innerRing);

            DrawBloom(pos, 0.3f * scale);
            Lighting.AddLight(pos, GoldenPollen.ToVector3() * scale);
        }

        /// <summary>
        /// Full Ode to Joy melee impact VFX — scales with combo step.
        /// </summary>
        public static void MeleeImpact(Vector2 pos, int comboStep = 0)
        {
            float bloomScale = 0.5f + comboStep * 0.1f;
            DrawBloom(pos, bloomScale);

            int rings = 3 + comboStep;
            SpawnGradientHaloRings(pos, rings);

            int dustCount = 8 + comboStep * 4;
            SpawnRadialDustBurst(pos, dustCount, 5f + comboStep);

            int noteCount = 2 + comboStep;
            SpawnMusicNotes(pos, noteCount, 25f);

            CustomParticles.GenericFlare(pos, GoldenPollen, 0.4f + comboStep * 0.08f);

            Lighting.AddLight(pos, VerdantGreen.ToVector3() * (0.8f + comboStep * 0.15f));
        }

        /// <summary>
        /// Musical burst impact with notes, sparkles, and petal accents.
        /// </summary>
        public static void MusicalImpact(Vector2 pos, float scale = 1f, bool enhanced = false)
        {
            int noteCount = enhanced ? 8 : 5;
            float noteSpread = enhanced ? 45f : 30f;
            SpawnMusicNotes(pos, noteCount, noteSpread, 0.8f, 1.2f, 40);
            SpawnPollenSparkles(pos, enhanced ? 6 : 3, 25f * scale);
            GardenImpact(pos, scale * 0.7f);
        }

        /// <summary>
        /// Projectile death / on-kill VFX — bigger, flashier version of GardenImpact.
        /// </summary>
        public static void ProjectileImpact(Vector2 pos, float intensity = 1f)
        {
            DrawBloom(pos, 0.6f * intensity);
            SpawnGradientHaloRings(pos, 6, 0.3f * intensity);
            SpawnMusicNotes(pos, 6, 30f * intensity, 0.75f, 1.1f, 30);
            SpawnRadialDustBurst(pos, 15, 7f * intensity);
            SpawnRosePetals(pos, 5, 30f * intensity);
            Lighting.AddLight(pos, GoldenPollen.ToVector3() * 1.2f * intensity);
        }

        /// <summary>
        /// Bloom burst — quick expanding bloom flash.
        /// </summary>
        public static void BloomBurst(Vector2 pos, float scale = 1f)
        {
            DrawBloom(pos, 0.6f * scale);
            SpawnGradientHaloRings(pos, 4, 0.3f * scale);
            SpawnRadialDustBurst(pos, 8, 4f * scale);
            Lighting.AddLight(pos, GoldenPollen.ToVector3() * scale);
        }

        /// <summary>
        /// Bloom flare at position.
        /// </summary>
        public static void BloomFlare(Vector2 pos, Color color, float scale = 0.55f,
            int lifetime = 18, int count = 3, float intensity = 0.85f)
        {
            CustomParticles.GenericFlare(pos, color, scale);
            DrawBloom(pos, scale * 0.5f);
            Lighting.AddLight(pos, color.ToVector3() * intensity);
        }

        /// <summary>
        /// Petal halo burst — expanding halos with rose petal accents.
        /// </summary>
        public static void PetalHaloBurst(Vector2 pos, float scale = 1f)
        {
            SpawnPetalHaloRings(pos, 5, 0.25f * scale);
            SpawnRosePetals(pos, 3, 30f * scale);
            DrawBloom(pos, 0.4f * scale);
            Lighting.AddLight(pos, RosePink.ToVector3() * 0.8f * scale);
        }

        /// <summary>
        /// Death garden flash — massive final bloom + dust + notes on enemy/boss death.
        /// </summary>
        public static void DeathGardenFlash(Vector2 pos, float scale = 1f)
        {
            DrawBloom(pos, 1.0f * scale);
            SpawnGradientHaloRings(pos, 8, 0.4f * scale);
            SpawnRadialDustBurst(pos, 25, 10f * scale);
            MusicNoteBurst(pos, GoldenPollen, 8, 5f * scale);
            SpawnRosePetals(pos, 10, 60f * scale);
            CustomParticles.GenericFlare(pos, WhiteBloom, 0.6f * scale);
            MagnumScreenEffects.AddScreenShake(4f * scale);
            GodRaySystem.CreateBurst(pos, VerdantGreen, 8, 80f * scale, 35, GodRaySystem.GodRayStyle.Explosion, GoldenPollen);
            Lighting.AddLight(pos, WhiteBloom.ToVector3() * 2f * scale);
        }

        // ─────────── SWING HELPERS ───────────

        /// <summary>
        /// Per-frame VFX to call from a swing projectile's AI().
        /// Handles dense dust trail, contrast sparkles, and periodic music notes.
        /// </summary>
        public static void SwingFrameVFX(Vector2 tipPos, Vector2 swordDirection, int comboStep,
            int timer, int dustType = DustID.GreenFairy)
        {
            SpawnSwingDust(tipPos, -swordDirection, dustType);
            SpawnContrastSparkle(tipPos, -swordDirection);

            if (timer % 5 == 0)
                SpawnMusicNotes(tipPos, 1, 10f, 0.7f, 0.9f, 25);

            Lighting.AddLight(tipPos, GetPaletteColor(0.3f + comboStep * 0.15f).ToVector3() * 0.6f);
        }

        // ─────────── FINISHER EFFECTS ───────────

        /// <summary>
        /// Phase-3 / finisher slam VFX — screen shake, massive bloom, music note cascade.
        /// The Ode to Joy version: a celebratory explosion of petals, pollen, and golden light.
        /// </summary>
        public static void FinisherSlam(Vector2 pos, float intensity = 1f)
        {
            MagnumScreenEffects.AddScreenShake(8f * intensity);
            DrawBloom(pos, 0.8f * intensity);
            SpawnGradientHaloRings(pos, 7, 0.35f * intensity);
            SpawnMusicNotes(pos, 6, 40f, 0.8f, 1.2f, 40);
            SpawnRadialDustBurst(pos, 20, 8f * intensity);
            SpawnRosePetals(pos, 8, 50f * intensity);
            GodRaySystem.CreateBurst(pos, VerdantGreen, 6, 100f * intensity, 40, GodRaySystem.GodRayStyle.Explosion, GoldenPollen);
            ScreenDistortionManager.TriggerRipple(pos, GoldenPollen, 0.8f * intensity, 25);
            Lighting.AddLight(pos, WhiteBloom.ToVector3() * 1.5f * intensity);
        }

        /// <summary>
        /// Triumphant celebration finale — the Ode to Joy signature.
        /// Ascending particles, golden radiance, nature's triumph.
        /// </summary>
        public static void TriumphantCelebration(Vector2 pos, float scale = 1f)
        {
            // Massive bloom burst
            DrawBloom(pos, 1.2f * scale);

            // Ascending petal storm - particles bias upward for "joy rises" effect
            for (int i = 0; i < (int)(15 * scale); i++)
            {
                float angle = MathHelper.TwoPi * i / 15f + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 10f) * scale;
                vel.Y -= 2f; // Upward bias
                Color col = OdeToJoyPalette.GetBlossomGradient((float)i / 15f);
                Dust d = Dust.NewDustPerfect(pos, i % 3 == 0 ? DustID.PinkFairy : DustID.GreenFairy, vel, 0, col, 1.8f);
                d.noGravity = true;
            }

            // Golden pollen explosion
            for (int i = 0; i < (int)(10 * scale); i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(8f, 8f) * scale;
                vel.Y -= 1.5f; // Upward bias
                Dust d = Dust.NewDustPerfect(pos, DustID.Enchanted_Gold, vel, 0, GoldenPollen, 1.5f);
                d.noGravity = true;
            }

            // Music note cascade
            MusicNoteBurst(pos, GoldenPollen, (int)(10 * scale), 6f * scale);

            // Halo rings
            SpawnGradientHaloRings(pos, 8, 0.35f * scale);
            SpawnPetalHaloRings(pos, 5, 0.3f * scale);

            // Rose petals
            SpawnRosePetals(pos, (int)(12 * scale), 60f * scale);

            // God rays
            GodRaySystem.CreateBurst(pos, GoldenPollen, 8, 120f * scale, 45, GodRaySystem.GodRayStyle.Explosion, WhiteBloom);

            // Screen effects
            MagnumScreenEffects.AddScreenShake(6f * scale);
            ScreenDistortionManager.TriggerRipple(pos, GoldenPollen, 0.6f * scale, 20);

            Lighting.AddLight(pos, WhiteBloom.ToVector3() * 2.5f * scale);
        }

        // ─────────── MISC ───────────

        /// <summary>
        /// Dodge afterimage trail — green/gold trailing dust.
        /// </summary>
        public static void DodgeTrail(Vector2 pos, Vector2 velocity)
        {
            for (int i = 0; i < 3; i++)
            {
                Vector2 vel = -velocity * 0.3f + Main.rand.NextVector2Circular(1f, 1f);
                Color col = Color.Lerp(VerdantGreen, GoldenPollen, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(8f, 8f),
                    DustID.GreenFairy, vel, 0, col, Main.rand.NextFloat(1.0f, 1.5f));
                d.noGravity = true;
            }
        }

        /// <summary>
        /// Teleport arrival burst — radial dust + bloom + notes.
        /// </summary>
        public static void TeleportBurst(Vector2 pos)
        {
            DrawBloom(pos, 0.5f);
            SpawnRadialDustBurst(pos, 12, 6f);
            SpawnMusicNotes(pos, 3, 20f);
            SpawnRosePetals(pos, 4, 30f);
            Lighting.AddLight(pos, GoldenPollen.ToVector3() * 1.2f);
        }

        /// <summary>
        /// Draw trail behind a projectile using {A=0} bloom pattern.
        /// </summary>
        public static void DrawProjectileTrail(SpriteBatch sb, Projectile proj, Color trailColor)
        {
            Texture2D bloom = MagnumTextureRegistry.GetBloom();
            if (bloom == null) return;

            Vector2 origin = bloom.Size() * 0.5f;

            for (int i = 0; i < proj.oldPos.Length; i++)
            {
                if (proj.oldPos[i] == Vector2.Zero) continue;

                float progress = (float)i / proj.oldPos.Length;
                float fade = 1f - progress;
                float scale = 0.3f * fade;

                Vector2 drawPos = proj.oldPos[i] + proj.Size * 0.5f - Main.screenPosition;
                Color col = Color.Lerp(trailColor, GoldenPollen, progress * 0.5f);

                sb.Draw(bloom, drawPos, null,
                    (col with { A = 0 }) * 0.5f * fade, 0f, origin, scale, SpriteEffects.None, 0f);
            }
        }

        // ─────────── DYNAMIC LIGHTING ───────────

        /// <summary>
        /// Add standard Ode to Joy ambient light at a position.
        /// </summary>
        public static void AddOdeToJoyLight(Vector2 worldPos, float intensity = 0.6f)
        {
            Lighting.AddLight(worldPos, VerdantGreen.ToVector3() * intensity);
        }

        /// <summary>
        /// Add palette-interpolated dynamic light. Higher t = brighter, more golden.
        /// </summary>
        public static void AddPaletteLighting(Vector2 worldPos, float paletteT, float intensity = 0.8f)
        {
            Color col = GetPaletteColor(paletteT);
            Lighting.AddLight(worldPos, col.ToVector3() * intensity);
        }
    }
}
