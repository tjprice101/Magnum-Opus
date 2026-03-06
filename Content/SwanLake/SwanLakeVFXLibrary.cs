using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Bloom;
using MagnumOpus.Common.Systems.Shaders;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.SwanLake
{
    /// <summary>
    /// Shared Swan Lake VFX library — canonical palette, bloom stacking,
    /// shader setup, trail helpers, music notes, dust, feather spawns,
    /// prismatic effects, and impact VFX used by ALL Swan Lake weapons,
    /// accessories, projectiles, minions, enemies, and bosses.
    ///
    /// Swan Lake identity: Dual-polarity (Black/White), graceful destruction,
    /// dying beauty, prismatic iridescence at the boundary of opposites.
    /// </summary>
    public static class SwanLakeVFXLibrary
    {
        // ─────────── CANONICAL PALETTE (forwarded from SwanLakePalette) ───────────
        public static readonly Color ObsidianBlack    = SwanLakePalette.ObsidianBlack;
        public static readonly Color DarkSilver       = SwanLakePalette.DarkSilver;
        public static readonly Color Silver           = SwanLakePalette.Silver;
        public static readonly Color PureWhite        = SwanLakePalette.PureWhite;
        public static readonly Color PrismaticShimmer = SwanLakePalette.PrismaticShimmer;
        public static readonly Color RainbowFlash     = SwanLakePalette.RainbowFlash;

        // Convenience accessors
        public static readonly Color SwanBlack        = SwanLakePalette.SwanBlack;
        public static readonly Color SwanWhite        = SwanLakePalette.SwanWhite;
        public static readonly Color SwanSilver       = SwanLakePalette.SwanSilver;
        public static readonly Color FeatherWhite     = SwanLakePalette.FeatherWhite;
        public static readonly Color FeatherBlack     = SwanLakePalette.FeatherBlack;
        public static readonly Color Pearlescent      = SwanLakePalette.Pearlescent;

        // Palette as array for indexed access
        private static readonly Color[] Palette = { ObsidianBlack, DarkSilver, Silver, PureWhite, PrismaticShimmer, RainbowFlash };

        // Hue range for HueShiftingMusicNoteParticle (full rainbow spectrum)
        private const float HueMin = 0.0f;
        private const float HueMax = 1.0f;
        private const float NoteSaturation = 0.80f;
        private const float NoteLuminosity = 0.85f;

        // Swan Lake glow profile for GlowRenderer
        public static readonly GlowRenderer.GlowLayer[] SwanGlowProfile = new[]
        {
            new GlowRenderer.GlowLayer(1.0f, 1.0f, Color.White),
            new GlowRenderer.GlowLayer(1.6f, 0.65f, new Color(220, 225, 235)),   // SwanSilver
            new GlowRenderer.GlowLayer(2.5f, 0.4f, new Color(180, 185, 200)),    // Silver
            new GlowRenderer.GlowLayer(4.0f, 0.2f, new Color(80, 80, 100))       // DarkSilver
        };

        // ─────────── PALETTE INTERPOLATION ───────────

        /// <summary>
        /// Lerp through the 6-colour Swan Lake palette. t=0 -> ObsidianBlack, t=1 -> RainbowFlash.
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
        /// </summary>
        public static Color GetPaletteColorWithWhitePush(float t, float push)
        {
            Color baseCol = GetPaletteColor(t);
            return Color.Lerp(baseCol, Color.White, MathHelper.Clamp(push, 0f, 1f));
        }

        /// <summary>
        /// Drop-in replacement for the GetSwanLakeGradient() in weapon files.
        /// ObsidianBlack -> Silver -> PureWhite over 0->1.
        /// </summary>
        public static Color GetSwanLakeGradient(float progress)
            => SwanLakePalette.GetSwanLakeGradient(progress);

        /// <summary>
        /// Get a cycling rainbow color for prismatic effects.
        /// </summary>
        public static Color GetRainbow(float offset = 0f)
            => SwanLakePalette.GetRainbow(offset);

        /// <summary>
        /// Get a vivid rainbow color (higher saturation).
        /// </summary>
        public static Color GetVividRainbow(float offset = 0f)
            => SwanLakePalette.GetVividRainbow(offset);

        // ─────────── SPRITEBATCH STATE HELPERS ───────────

        /// <summary>
        /// Switch SpriteBatch to additive blend for Swan Lake VFX rendering.
        /// </summary>
        public static void BeginSwanAdditive(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Restore SpriteBatch to standard AlphaBlend after additive rendering.
        /// </summary>
        public static void EndSwanAdditive(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Switch SpriteBatch to Immediate + Additive for shader-driven drawing.
        /// </summary>
        public static void BeginShaderAdditive(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Immediate, MagnumBlendStates.ShaderAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
                Main.Rasterizer, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Restore SpriteBatch to normal deferred alpha-blend mode.
        /// </summary>
        public static void RestoreSpriteBatch(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                Main.Rasterizer, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        // ─────────── BLOOM STACKING (DIRECT {A=0} PATTERN) ───────────

        /// <summary>
        /// 4-layer bloom stack using {A=0} premultiplied alpha trick.
        /// Dual-polarity: DarkSilver outer -> Silver mid -> PureWhite inner -> White core.
        /// </summary>
        public static void DrawSwanBloomStack(SpriteBatch sb, Vector2 worldPos,
            float scale, float paletteT = 0.4f, float opacity = 1f)
        {
            Texture2D bloom = MagnumTextureRegistry.GetBloom();
            if (bloom == null) return;

            Vector2 drawPos = worldPos - Main.screenPosition;
            Vector2 origin = bloom.Size() * 0.5f;

            Color outer = GetPaletteColor(MathHelper.Clamp(paletteT - 0.2f, 0f, 1f));
            Color inner = GetPaletteColor(MathHelper.Clamp(paletteT + 0.2f, 0f, 1f));

            // Layer 1: Outer halo (DarkSilver)
            sb.Draw(bloom, drawPos, null,
                (outer with { A = 0 }) * 0.3f * opacity, 0f, origin, scale * 2.0f, SpriteEffects.None, 0f);

            // Layer 2: Mid glow (Silver)
            sb.Draw(bloom, drawPos, null,
                (outer with { A = 0 }) * 0.5f * opacity, 0f, origin, scale * 1.4f, SpriteEffects.None, 0f);

            // Layer 3: Inner bloom (PureWhite)
            sb.Draw(bloom, drawPos, null,
                (inner with { A = 0 }) * 0.7f * opacity, 0f, origin, scale * 0.9f, SpriteEffects.None, 0f);

            // Layer 4: White-hot core
            sb.Draw(bloom, drawPos, null,
                (RainbowFlash with { A = 0 }) * 0.85f * opacity, 0f, origin, scale * 0.4f, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Two-color bloom stack using {A=0} pattern.
        /// </summary>
        public static void DrawSwanBloomStack(SpriteBatch sb, Vector2 worldPos,
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
        /// Dual-polarity: dark behind, bright in front.
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
                // Behind layer: larger, softer, dark
                sb.Draw(bloom, drawPos, null,
                    (ObsidianBlack with { A = 0 }) * 0.25f * opacity, 0f, origin, scale * 2.5f, SpriteEffects.None, 0f);
                sb.Draw(bloom, drawPos, null,
                    (DarkSilver with { A = 0 }) * 0.35f * opacity, 0f, origin, scale * 1.6f, SpriteEffects.None, 0f);
            }
            else
            {
                // Front layer: smaller, brighter, white
                sb.Draw(bloom, drawPos, null,
                    (PureWhite with { A = 0 }) * 0.5f * opacity, 0f, origin, scale * 0.8f, SpriteEffects.None, 0f);
                sb.Draw(bloom, drawPos, null,
                    (Color.White with { A = 0 }) * 0.7f * opacity, 0f, origin, scale * 0.35f, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Counter-rotating double flare — dual-polarity black and white spinning in opposite directions.
        /// Creates the signature graceful dual-energy at projectile centers.
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
                (PureWhite with { A = 0 }) * 0.6f * opacity, rot1, origin, scale * 0.7f, SpriteEffects.None, 0f);
            sb.Draw(flare, drawPos, null,
                (DarkSilver with { A = 0 }) * 0.5f * opacity, rot2, origin, scale * 0.5f, SpriteEffects.None, 0f);
        }

        // ─────────── SELF-CONTAINED BLOOM (MANAGES OWN SPRITEBATCH) ───────────

        /// <summary>
        /// Standard Swan Lake bloom at a blade tip or projectile centre.
        /// Dual-polarity: ObsidianBlack outer -> PureWhite inner.
        /// Safe to call from PreDraw — handles SpriteBatch state internally.
        /// </summary>
        public static void DrawBloom(Vector2 worldPos, float scale, float opacity = 1f)
        {
            BloomRenderer.DrawBloomStackAdditive(worldPos, ObsidianBlack, PureWhite, scale, opacity);
        }

        /// <summary>
        /// Combo-step-aware bloom (bigger + brighter on later hits).
        /// </summary>
        public static void DrawComboBloom(Vector2 worldPos, int comboStep, float baseScale = 0.4f, float opacity = 1f)
        {
            float scale = baseScale + comboStep * 0.07f;
            DrawBloom(worldPos, scale, opacity);
        }

        // ─────────── GLOW RENDERER INTEGRATION ───────────

        /// <summary>
        /// Draw Swan Lake-themed multi-layer glow via GlowRenderer.
        /// </summary>
        public static void DrawSwanGlow(SpriteBatch sb, Vector2 worldPos,
            float scale = 1f, float intensity = 1f, string rotationId = null)
        {
            GlowRenderer.DrawGlow(sb, worldPos, SwanGlowProfile, PureWhite, intensity * scale, rotationId);
        }

        /// <summary>
        /// Draw Swan Lake glow with automatic SpriteBatch state management.
        /// </summary>
        public static void DrawSwanGlowManaged(SpriteBatch sb, Vector2 worldPos,
            float scale = 1f, float intensity = 1f, string rotationId = null)
        {
            GlowRenderer.DrawGlowManaged(sb, worldPos, SwanGlowProfile, PureWhite, intensity * scale, rotationId);
        }

        // ─────────── TRAIL WIDTH/COLOR FUNCTIONS ───────────

        /// <summary>
        /// Standard Swan Lake trail width: graceful, elegant taper.
        /// </summary>
        public static float SwanTrailWidth(float completionRatio, float baseWidth = 18f)
        {
            float tipFade = MathF.Pow(1f - completionRatio, 0.6f);
            float headFade = MathF.Pow(completionRatio, 3f);
            return baseWidth * tipFade * (1f - headFade);
        }

        /// <summary>
        /// Thin precision trail for elegant ranged weapons — pearlescent shot.
        /// </summary>
        public static float PrecisionTrailWidth(float completionRatio, float baseWidth = 6f)
        {
            float taper = 1f - completionRatio;
            return baseWidth * taper * taper;
        }

        /// <summary>
        /// Wide graceful trail for melee weapons — sweeping arc.
        /// </summary>
        public static float GracefulTrailWidth(float completionRatio, float baseWidth = 16f)
        {
            float tipFade = MathF.Pow(1f - completionRatio, 0.4f);
            return baseWidth * tipFade;
        }

        /// <summary>
        /// Trail color function with {A=0} for additive rendering.
        /// Dual-polarity gradient: dark at edges, white-pushed center along trail.
        /// </summary>
        public static Color SwanTrailColor(float completionRatio, float whitePush = 0.45f)
        {
            float t = 0.3f + completionRatio * 0.5f;
            Color baseCol = GetPaletteColorWithWhitePush(t, whitePush * (1f - completionRatio));
            float fade = 1f - MathF.Pow(completionRatio, 1.5f);
            return (baseCol * fade) with { A = 0 };
        }

        /// <summary>
        /// Returns a pair of Vector3 colours for shader gradient uniforms.
        /// </summary>
        public static (Vector3 primary, Vector3 secondary) GetShaderGradient(int passIndex)
        {
            return passIndex switch
            {
                0 => (ObsidianBlack.ToVector3(), DarkSilver.ToVector3()),
                1 => (Silver.ToVector3(), PureWhite.ToVector3()),
                2 => (PureWhite.ToVector3(), RainbowFlash.ToVector3()),
                _ => (DarkSilver.ToVector3(), PureWhite.ToVector3()),
            };
        }

        // ─────────── MUSIC NOTES ───────────

        /// <summary>
        /// Spawn visible, hue-shifting Swan Lake music notes at the given position.
        /// Notes cycle through the full rainbow spectrum (0.0-1.0) for prismatic effect.
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
                    scale, lifetime, hueSpeed: 0.03f
                );
                MagnumParticleHandler.SpawnParticle(note);
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
                    scale, 30, hueSpeed: 0.03f
                );
                MagnumParticleHandler.SpawnParticle(note);
            }
        }

        // ─────────── DUST HELPERS ───────────

        /// <summary>
        /// Dense Swan Lake dual-polarity dust trail at a blade tip during a swing.
        /// Alternating black and white torch dust for the signature monochrome effect.
        /// </summary>
        public static void SpawnSwingDust(Vector2 pos, Vector2 awayDirection, int dustType = DustID.WhiteTorch)
        {
            for (int i = 0; i < 2; i++)
            {
                Vector2 vel = awayDirection * Main.rand.NextFloat(1f, 3f) + Main.rand.NextVector2Circular(0.5f, 0.5f);
                Color col = GetPaletteColor(Main.rand.NextFloat(0.2f, 0.8f));
                Dust d = Dust.NewDustPerfect(pos, dustType, vel, 0, col, 1.5f);
                d.noGravity = true;
            }
        }

        /// <summary>
        /// Swan Lake dual-polarity swing dust — alternating white and shadowflame.
        /// </summary>
        public static void SpawnDualPolarityDust(Vector2 pos, Vector2 awayDirection)
        {
            for (int i = 0; i < 2; i++)
            {
                Vector2 vel = awayDirection * Main.rand.NextFloat(1f, 3f) + Main.rand.NextVector2Circular(0.5f, 0.5f);
                bool isWhite = Main.rand.NextBool();
                int dustType = isWhite ? DustID.WhiteTorch : DustID.Shadowflame;
                Color col = isWhite ? PureWhite : ObsidianBlack;
                Dust d = Dust.NewDustPerfect(pos, dustType, vel, isWhite ? 0 : 100, col, 1.5f);
                d.noGravity = true;
                d.fadeIn = 1.3f;
            }
        }

        /// <summary>
        /// Radial dust burst for on-hit / impact VFX.
        /// Dual-polarity: alternating black and white around the burst ring.
        /// </summary>
        public static void SpawnRadialDustBurst(Vector2 pos, int count = 12,
            float speed = 5f)
        {
            for (int i = 0; i < count; i++)
            {
                float progress = (float)i / count;
                float angle = MathHelper.TwoPi * i / count;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(speed * 0.6f, speed);
                bool isWhite = i % 2 == 0;
                int dustType = isWhite ? DustID.WhiteTorch : DustID.Shadowflame;
                Color col = isWhite ? PureWhite : ObsidianBlack;
                Dust d = Dust.NewDustPerfect(pos, dustType, vel, isWhite ? 0 : 100, col, 1.3f);
                d.noGravity = true;
            }
        }

        /// <summary>
        /// Rainbow shimmer dust — prismatic sparkle trail.
        /// </summary>
        public static void SpawnRainbowShimmer(Vector2 pos, Vector2 awayDirection)
        {
            if (!Main.rand.NextBool(2)) return;
            float hue = Main.rand.NextFloat();
            Color rainbow = Main.hslToRgb(hue, 1f, 0.8f);
            Dust d = Dust.NewDustPerfect(pos, DustID.RainbowTorch,
                awayDirection * 0.5f + Main.rand.NextVector2Circular(1.5f, 1.5f), 0, rainbow, 1.2f);
            d.noGravity = true;
        }

        /// <summary>
        /// Rainbow radial dust burst — prismatic explosion ring.
        /// </summary>
        public static void SpawnRainbowBurst(Vector2 pos, int count = 10, float speed = 5f)
        {
            for (int i = 0; i < count; i++)
            {
                float hue = (float)i / count;
                float angle = MathHelper.TwoPi * i / count;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(speed * 0.6f, speed);
                Color col = Main.hslToRgb(hue, 1f, 0.75f);
                Dust d = Dust.NewDustPerfect(pos, DustID.RainbowTorch, vel, 0, col, 1.4f);
                d.noGravity = true;
            }
        }

        // ─────────── SWAN LAKE-SPECIFIC VFX: FEATHERS ───────────

        /// <summary>
        /// Spawn drifting swan feather particles around a position.
        /// The signature Swan Lake visual identity — graceful floating feathers.
        /// </summary>
        public static void SpawnFeatherDrift(Vector2 pos, int count = 3, float radius = 30f, float scale = 0.25f)
        {
            for (int i = 0; i < count; i++)
            {
                Color featherCol = Main.rand.NextBool() ? FeatherWhite : FeatherBlack;
                try { CustomParticles.SwanFeatherDrift(pos + Main.rand.NextVector2Circular(radius, radius), featherCol, scale); } catch { }
            }
        }

        /// <summary>
        /// Spawn a burst of feathers exploding outward from impact point.
        /// </summary>
        public static void SpawnFeatherBurst(Vector2 pos, int count = 6, float scale = 0.3f)
        {
            try { CustomParticles.SwanFeatherBurst(pos, count, scale); } catch { }
        }

        /// <summary>
        /// Spawn dual-polarity feathers — black and white intertwined.
        /// </summary>
        public static void SpawnFeatherDuality(Vector2 pos, int count = 3, float scale = 0.3f)
        {
            try { CustomParticles.SwanFeatherDuality(pos, count, scale); } catch { }
        }

        // ─────────── SWAN LAKE-SPECIFIC VFX: PRISMATIC ───────────

        /// <summary>
        /// Spawn prismatic sparkle particles — rainbow-cycling points of light.
        /// Creates the iridescent effect at the boundary of black and white.
        /// </summary>
        public static void SpawnPrismaticSparkles(Vector2 pos, int count = 6, float radius = 25f)
        {
            for (int i = 0; i < count; i++)
            {
                float hue = (float)i / count;
                Color sparkColor = Main.hslToRgb(hue, 1f, 0.8f);
                try
                {
                    CustomParticles.GenericFlare(
                        pos + Main.rand.NextVector2Circular(radius, radius),
                        sparkColor, 0.45f, 18);
                }
                catch { }
            }
        }

        /// <summary>
        /// Spawn a prismatic rainbow explosion — full spectrum detonation.
        /// </summary>
        public static void SpawnRainbowExplosion(Vector2 pos, float intensity = 1f)
        {
            try { ThemedParticles.SwanLakeRainbowExplosion(pos, intensity); } catch { }
        }

        /// <summary>
        /// Spawn iridescent glow particles swirling inward toward a center.
        /// Creates the prismatic convergence effect.
        /// </summary>
        public static void SpawnPrismaticSwirl(Vector2 center, int count = 6, float radius = 60f, float opacity = 0.65f)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float dist = radius + Main.rand.NextFloat(30f);
                Vector2 particlePos = center + angle.ToRotationVector2() * dist;
                Vector2 vel = (center - particlePos).SafeNormalize(Vector2.Zero) * 3f;

                Color rainbow = GetRainbow((float)i / count);
                var glow = new GenericGlowParticle(particlePos, vel,
                    rainbow * opacity,
                    0.28f, 22, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
        }

        // ─────────── GRADIENT HALO RINGS ───────────

        /// <summary>
        /// Cascading gradient halo rings — dual-polarity (ObsidianBlack -> PureWhite).
        /// </summary>
        public static void SpawnGradientHaloRings(Vector2 pos, int count = 5, float baseScale = 0.3f)
        {
            for (int i = 0; i < count; i++)
            {
                float progress = (float)i / count;
                Color ringCol = Color.Lerp(ObsidianBlack, PureWhite, progress);
                CustomParticles.HaloRing(pos, ringCol, baseScale + i * 0.12f, 14);
            }
        }

        /// <summary>
        /// Rainbow halo rings — prismatic ring cascade.
        /// </summary>
        public static void SpawnRainbowHaloRings(Vector2 pos, int count = 5, float baseScale = 0.3f)
        {
            for (int i = 0; i < count; i++)
            {
                float hue = (float)i / count;
                Color ringCol = Main.hslToRgb(hue, 0.85f, 0.8f);
                CustomParticles.HaloRing(pos, ringCol, baseScale + i * 0.12f, 14);
            }
        }

        // ─────────── IMPACTS ───────────

        /// <summary>
        /// Full Swan Lake melee impact VFX — dual-polarity bloom flash, halo cascade,
        /// monochrome dust burst, prismatic sparkles, feather scatter, and music note burst.
        /// Scales with combo step.
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

            // Swan Lake signature: feather scatter at impact
            SpawnFeatherDrift(pos, 2 + comboStep, 20f);

            // Prismatic sparkles
            SpawnPrismaticSparkles(pos, 4 + comboStep * 2, 15f);

            // Rainbow flare ring
            for (int i = 0; i < 4 + comboStep * 2; i++)
            {
                float hue = (float)i / (4 + comboStep * 2);
                Color flareColor = Main.hslToRgb(hue, 1f, 0.75f);
                try { CustomParticles.GenericFlare(pos + Main.rand.NextVector2Circular(15f, 15f), flareColor, 0.45f, 18); } catch { }
            }

            // Dual-polarity halo rings
            try { CustomParticles.HaloRing(pos, PureWhite, 0.5f, 18); } catch { }
            try { CustomParticles.HaloRing(pos, ObsidianBlack, 0.35f, 15); } catch { }

            Lighting.AddLight(pos, PureWhite.ToVector3() * (0.8f + comboStep * 0.15f));
        }

        /// <summary>
        /// Projectile death / on-kill VFX — bigger, flashier version of MeleeImpact.
        /// Includes feather burst, rainbow explosion, and enhanced bloom.
        /// </summary>
        public static void ProjectileImpact(Vector2 pos, float intensity = 1f)
        {
            DrawBloom(pos, 0.6f * intensity);
            SpawnGradientHaloRings(pos, 6, 0.3f * intensity);
            SpawnMusicNotes(pos, 6, 30f * intensity, 0.75f, 1.1f, 30);
            SpawnRadialDustBurst(pos, 15, 7f * intensity);
            SpawnFeatherBurst(pos, (int)(6 * intensity), 0.35f);
            SpawnRainbowExplosion(pos, intensity);
            SpawnPrismaticSparkles(pos, (int)(8 * intensity), 25f);
            try { CustomParticles.GenericFlare(pos, PureWhite, 0.5f * intensity, 16); } catch { }
            Lighting.AddLight(pos, PureWhite.ToVector3() * 1.2f * intensity);
        }

        // ─────────── SWING HELPERS ───────────

        /// <summary>
        /// Per-frame VFX to call from a swing projectile's AI().
        /// Handles dual-polarity dust trail, rainbow shimmer, periodic feathers and music notes.
        /// </summary>
        public static void SwingFrameVFX(Vector2 tipPos, Vector2 swordDirection, int comboStep,
            int timer, int dustType = DustID.WhiteTorch)
        {
            SpawnDualPolarityDust(tipPos, -swordDirection);
            SpawnRainbowShimmer(tipPos, -swordDirection);

            if (timer % 5 == 0)
                SpawnMusicNotes(tipPos, 1, 10f, 0.7f, 0.9f, 25);

            // Swan Lake signature: periodic feather drift along swing arc
            if (timer % 8 == 0)
                SpawnFeatherDrift(tipPos, 1, 10f, 0.2f);

            Lighting.AddLight(tipPos, GetPaletteColor(0.4f + comboStep * 0.15f).ToVector3() * 0.6f);
        }

        // ─────────── FINISHER EFFECTS ───────────

        /// <summary>
        /// Phase-3 / finisher slam VFX — screen shake, massive bloom, monochrome cascade,
        /// feather explosion, rainbow detonation, music note scatter.
        /// </summary>
        public static void FinisherSlam(Vector2 pos, float intensity = 1f)
        {
            MagnumScreenEffects.AddScreenShake(8f * intensity);
            DrawBloom(pos, 0.8f * intensity);
            SpawnGradientHaloRings(pos, 7, 0.35f * intensity);
            SpawnRainbowHaloRings(pos, 5, 0.3f * intensity);
            SpawnMusicNotes(pos, 6, 40f, 0.8f, 1.2f, 40);
            SpawnRadialDustBurst(pos, 20, 8f * intensity);
            SpawnRainbowBurst(pos, 16, 7f * intensity);
            SpawnFeatherBurst(pos, (int)(10 * intensity), 0.4f);
            SpawnFeatherDuality(pos, (int)(6 * intensity), 0.35f);
            SpawnRainbowExplosion(pos, 1.5f * intensity);
            SpawnPrismaticSwirl(pos, 10, 80f * intensity);
            SpawnPrismaticSparkles(pos, 12, 30f * intensity);
            Lighting.AddLight(pos, RainbowFlash.ToVector3() * 1.5f * intensity);
        }

        // ─────────── DYNAMIC LIGHTING ───────────

        /// <summary>
        /// Add standard Swan Lake ambient light at a position.
        /// </summary>
        public static void AddSwanLight(Vector2 worldPos, float intensity = 0.6f)
        {
            Lighting.AddLight(worldPos, PureWhite.ToVector3() * intensity);
        }

        /// <summary>
        /// Add palette-interpolated dynamic light. Higher t = brighter, whiter.
        /// </summary>
        public static void AddPaletteLighting(Vector2 worldPos, float paletteT, float intensity = 0.8f)
        {
            Color col = GetPaletteColor(paletteT);
            Lighting.AddLight(worldPos, col.ToVector3() * intensity);
        }

        /// <summary>
        /// Add pulsing swan light with prismatic color shift.
        /// </summary>
        public static void AddPulsingLight(Vector2 worldPos, float time, float intensity = 0.6f)
        {
            Color rainbow = GetRainbow(time * 0.01f);
            Color lightColor = Color.Lerp(PureWhite, rainbow, 0.25f);
            float pulse = (float)Math.Sin(time * 0.08f) * 0.15f + 0.85f;
            Lighting.AddLight(worldPos, lightColor.ToVector3() * pulse * intensity);
        }

        /// <summary>
        /// Add dual-polarity flickering light — oscillates between warm white and cold black-blue.
        /// </summary>
        public static void AddDualPolarityLight(Vector2 worldPos, float time, float intensity = 0.6f)
        {
            float shift = (float)Math.Sin(time * 0.06f) * 0.5f + 0.5f;
            Color lightColor = Color.Lerp(DarkSilver, PureWhite, shift);
            Lighting.AddLight(worldPos, lightColor.ToVector3() * intensity);
        }

        // ─────────── THEME TEXTURE VFX ───────────
        // Uses SwanLakeThemeTextures for theme-specific visuals.

        /// <summary>
        /// Draws a themed graceful impact ring using Swan Lake Power Effect Ring + Harmonic Impact.
        /// Must be called in Additive blend mode (or {A=0} pattern).
        /// </summary>
        public static void DrawThemeImpactRing(SpriteBatch sb, Vector2 worldPos,
            float scale, float intensity = 1f, float rotation = 0f)
        {
            Vector2 drawPos = worldPos - Main.screenPosition;

            Texture2D ring = SwanLakeThemeTextures.SLPowerEffectRing?.Value;
            if (ring != null)
            {
                Vector2 origin = ring.Size() * 0.5f;
                sb.Draw(ring, drawPos, null,
                    (PureWhite with { A = 0 }) * 0.5f * intensity, rotation, origin,
                    scale * 0.15f, SpriteEffects.None, 0f);
                sb.Draw(ring, drawPos, null,
                    (PrismaticShimmer with { A = 0 }) * 0.3f * intensity, -rotation * 0.7f, origin,
                    scale * 0.10f, SpriteEffects.None, 0f);
            }

            Texture2D impact = SwanLakeThemeTextures.SLHarmonicImpact?.Value;
            if (impact != null)
            {
                Vector2 impOrigin = impact.Size() * 0.5f;
                sb.Draw(impact, drawPos, null,
                    (Silver with { A = 0 }) * 0.45f * intensity, rotation * 1.3f, impOrigin,
                    scale * 0.12f, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Draws a themed crystal shard particle accent at a position.
        /// Must be called in Additive blend mode.
        /// </summary>
        public static void DrawThemeCrystalAccent(SpriteBatch sb, Vector2 worldPos,
            float scale, float intensity = 1f)
        {
            Vector2 drawPos = worldPos - Main.screenPosition;

            Texture2D shard = SwanLakeThemeTextures.SLCrystalShard?.Value;
            if (shard != null)
            {
                Vector2 origin = shard.Size() * 0.5f;
                float rot = (float)Main.GameUpdateCount * 0.04f;
                sb.Draw(shard, drawPos, null,
                    (PureWhite with { A = 0 }) * 0.45f * intensity, rot, origin,
                    scale * 0.07f, SpriteEffects.None, 0f);
                sb.Draw(shard, drawPos, null,
                    (PrismaticShimmer with { A = 0 }) * 0.3f * intensity, -rot * 0.6f, origin,
                    scale * 0.05f, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Combined theme impact: universal bloom + theme ring + crystal accents.
        /// </summary>
        public static void DrawThemeImpactFull(SpriteBatch sb, Vector2 worldPos,
            float scale, float intensity = 1f)
        {
            DrawSwanBloomStack(sb, worldPos, scale, 0.3f, intensity);
            DrawThemeCrystalAccent(sb, worldPos, scale, intensity * 0.7f);
            float rot = (float)Main.GameUpdateCount * 0.02f;
            DrawThemeImpactRing(sb, worldPos, scale, intensity * 0.6f, rot);
        }
    }
}
