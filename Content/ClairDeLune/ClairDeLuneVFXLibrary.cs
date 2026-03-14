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

namespace MagnumOpus.Content.ClairDeLune
{
    /// <summary>
    /// Shared Clair de Lune VFX library — canonical palette, bloom stacking,
    /// shader setup, trail helpers, music notes, dust, pearl/clockwork spawns,
    /// lunar effects, and impact VFX used by ALL Clair de Lune weapons,
    /// accessories, projectiles, minions, enemies, and bosses.
    ///
    /// Clair de Lune identity: Moonlit reverie, dreamy calm, gentle luminescence,
    /// clockwork precision, temporal power, pearl shimmer, impressionistic haze.
    /// Debussy's nocturnal dreamscape married with clockwork mechanism and
    /// the supreme final boss tier's temporal authority.
    /// </summary>
    public static class ClairDeLuneVFXLibrary
    {
        // ─────────── CANONICAL PALETTE (forwarded from ClairDeLunePalette) ───────────
        public static readonly Color NightMist     = ClairDeLunePalette.NightMist;
        public static readonly Color MidnightBlue  = ClairDeLunePalette.MidnightBlue;
        public static readonly Color SoftBlue      = ClairDeLunePalette.SoftBlue;
        public static readonly Color PearlBlue     = ClairDeLunePalette.PearlBlue;
        public static readonly Color PearlWhite    = ClairDeLunePalette.PearlWhite;
        public static readonly Color WhiteHot      = ClairDeLunePalette.WhiteHot;

        // Convenience accessors
        public static readonly Color ClockworkBrass   = ClairDeLunePalette.ClockworkBrass;
        public static readonly Color MoonbeamGold     = ClairDeLunePalette.MoonbeamGold;
        public static readonly Color StarlightSilver  = ClairDeLunePalette.StarlightSilver;
        public static readonly Color PearlShimmer     = ClairDeLunePalette.PearlShimmer;

        // Palette as array for indexed access
        private static readonly Color[] Palette = { NightMist, MidnightBlue, SoftBlue, PearlBlue, PearlWhite, WhiteHot };

        // Hue range for HueShiftingMusicNoteParticle (blue-pearl range)
        private const float HueMin = 0.55f;
        private const float HueMax = 0.68f;
        private const float NoteSaturation = 0.65f;
        private const float NoteLuminosity = 0.80f;

        // Clair de Lune glow profile for GlowRenderer
        public static readonly GlowRenderer.GlowLayer[] ClairDeLuneGlowProfile = new[]
        {
            new GlowRenderer.GlowLayer(1.0f, 1.0f, Color.White),                       // WhiteHot core
            new GlowRenderer.GlowLayer(1.6f, 0.60f, new Color(220, 230, 245)),          // PearlWhite inner
            new GlowRenderer.GlowLayer(2.5f, 0.38f, new Color(160, 195, 235)),          // PearlBlue mid
            new GlowRenderer.GlowLayer(4.0f, 0.18f, new Color(60, 80, 140))             // MidnightBlue outer
        };

        // ─────────── PALETTE INTERPOLATION ───────────

        /// <summary>
        /// Lerp through the 6-colour Clair de Lune palette. t=0 -> NightMist, t=1 -> WhiteHot.
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
        /// Drop-in replacement for the GetClairDeLuneGradient() in weapon files.
        /// NightMist -> SoftBlue -> PearlWhite over 0->1.
        /// </summary>
        public static Color GetClairDeLuneGradient(float progress)
            => ClairDeLunePalette.GetClairDeLuneGradient(progress);

        /// <summary>
        /// Get a cycling blue-pearl shimmer color for dreamy effects.
        /// </summary>
        public static Color GetShimmer(float offset = 0f)
            => ClairDeLunePalette.GetShimmer(offset);

        /// <summary>
        /// Get a pearl shimmer color (low saturation, high luminosity).
        /// </summary>
        public static Color GetPearlShimmerColor(float offset = 0f)
            => ClairDeLunePalette.GetPearlShimmer(offset);

        // ─────────── SPRITEBATCH STATE HELPERS ───────────

        /// <summary>
        /// Switch SpriteBatch to additive blend for Clair de Lune VFX rendering.
        /// </summary>
        public static void BeginClairDeLuneAdditive(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Restore SpriteBatch to standard AlphaBlend after additive rendering.
        /// </summary>
        public static void EndClairDeLuneAdditive(SpriteBatch sb)
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


        /// <summary>
        /// Counter-rotating double flare — pearl white and midnight blue spinning in opposite directions.
        /// Creates the signature dreamy moonlit luminescence at projectile centers.
        /// </summary>
        public static void DrawCounterRotatingFlares(SpriteBatch sb, Vector2 worldPos,
            float scale, float time, float opacity = 1f)
        {
            Texture2D flare = MagnumTextureRegistry.GetFlare();
            if (flare == null) return;

            // 1024px flare — cap so largest layer (scale*0.7) <= 0.293 => <=300px
            scale = MathHelper.Min(scale, 0.419f);

            Vector2 drawPos = worldPos - Main.screenPosition;
            Vector2 origin = flare.Size() * 0.5f;

            float rot1 = time * 2.5f;
            float rot2 = -time * 1.8f;

            sb.Draw(flare, drawPos, null,
                (PearlWhite with { A = 0 }) * 0.6f * opacity, rot1, origin, scale * 0.7f, SpriteEffects.None, 0f);
            sb.Draw(flare, drawPos, null,
                (MidnightBlue with { A = 0 }) * 0.5f * opacity, rot2, origin, scale * 0.5f, SpriteEffects.None, 0f);
        }


        // ─────────── GLOW RENDERER INTEGRATION ───────────

        /// <summary>
        /// Draw Clair de Lune-themed multi-layer glow via GlowRenderer.
        /// </summary>
        public static void DrawClairDeLuneGlow(SpriteBatch sb, Vector2 worldPos,
            float scale = 1f, float intensity = 1f, string rotationId = null)
        {
            GlowRenderer.DrawGlow(sb, worldPos, ClairDeLuneGlowProfile, PearlWhite, intensity * scale, rotationId);
        }

        /// <summary>
        /// Draw Clair de Lune glow with automatic SpriteBatch state management.
        /// </summary>
        public static void DrawClairDeLuneGlowManaged(SpriteBatch sb, Vector2 worldPos,
            float scale = 1f, float intensity = 1f, string rotationId = null)
        {
            GlowRenderer.DrawGlowManaged(sb, worldPos, ClairDeLuneGlowProfile, PearlWhite, intensity * scale, rotationId);
        }

        // ─────────── TRAIL WIDTH/COLOR FUNCTIONS ───────────

        /// <summary>
        /// Standard Clair de Lune trail width: flowing, dreamy taper with gentle luminescence.
        /// </summary>
        public static float ClairDeLuneTrailWidth(float completionRatio, float baseWidth = 18f)
        {
            float tipFade = MathF.Pow(1f - completionRatio, 0.6f);
            float headFade = MathF.Pow(completionRatio, 3f);
            return baseWidth * tipFade * (1f - headFade);
        }

        /// <summary>
        /// Thin precision trail for clockwork ranged weapons — pearl-blue shot.
        /// </summary>
        public static float PrecisionTrailWidth(float completionRatio, float baseWidth = 6f)
        {
            float taper = 1f - completionRatio;
            return baseWidth * taper * taper;
        }

        /// <summary>
        /// Wide dreamy trail for melee weapons — slower, wider taper for impressionistic sweep.
        /// </summary>
        public static float DreamyTrailWidth(float completionRatio, float baseWidth = 20f)
        {
            float tipFade = MathF.Pow(1f - completionRatio, 0.35f);
            return baseWidth * tipFade;
        }

        /// <summary>
        /// Trail color function with {A=0} for additive rendering.
        /// Blue-pearl gradient with white push: moonlit luminosity along trail.
        /// </summary>
        public static Color ClairDeLuneTrailColor(float completionRatio, float whitePush = 0.45f)
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
                0 => (NightMist.ToVector3(), MidnightBlue.ToVector3()),
                1 => (SoftBlue.ToVector3(), PearlBlue.ToVector3()),
                2 => (PearlWhite.ToVector3(), WhiteHot.ToVector3()),
                _ => (MidnightBlue.ToVector3(), PearlWhite.ToVector3()),
            };
        }

        // ─────────── MUSIC NOTES ───────────

        /// <summary>
        /// Spawn visible, hue-shifting Clair de Lune music notes at the given position.
        /// Notes cycle through the blue-pearl hue range (0.55-0.68) for dreamy moonlit effect.
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
                    scale, lifetime, hueSpeed: 0.02f
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
                    scale, 30, hueSpeed: 0.02f
                );
                MagnumParticleHandler.SpawnParticle(note);
            }
        }

        // ─────────── DUST HELPERS ───────────

        /// <summary>
        /// Dense Clair de Lune pearl-blue dust trail at a blade tip during a swing.
        /// WhiteTorch dust tinted with pearl-blue palette colours for moonlit luminescence.
        /// </summary>
        public static void SpawnSwingDust(Vector2 pos, Vector2 awayDirection, int dustType = DustID.WhiteTorch)
        {
            for (int i = 0; i < 2; i++)
            {
                Vector2 vel = awayDirection * Main.rand.NextFloat(1f, 3f) + Main.rand.NextVector2Circular(0.5f, 0.5f);
                Color col = GetPaletteColor(Main.rand.NextFloat(0.3f, 0.9f));
                Dust d = Dust.NewDustPerfect(pos, dustType, vel, 0, col, 1.5f);
                d.noGravity = true;
            }
        }

        /// <summary>
        /// Clair de Lune pearl dust — alternating WhiteTorch and BlueTorch
        /// for the signature moonlit pearl-blue shimmer.
        /// </summary>
        public static void SpawnPearlDust(Vector2 pos, Vector2 awayDirection)
        {
            for (int i = 0; i < 2; i++)
            {
                Vector2 vel = awayDirection * Main.rand.NextFloat(1f, 3f) + Main.rand.NextVector2Circular(0.5f, 0.5f);
                bool isWhite = Main.rand.NextBool();
                int dustType = isWhite ? DustID.WhiteTorch : DustID.BlueTorch;
                Color col = isWhite ? PearlWhite : PearlBlue;
                Dust d = Dust.NewDustPerfect(pos, dustType, vel, 0, col, 1.5f);
                d.noGravity = true;
                d.fadeIn = 1.3f;
            }
        }

        /// <summary>
        /// Radial dust burst for on-hit / impact VFX.
        /// Alternating WhiteTorch and BlueTorch around the burst ring for pearl-blue duality.
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
                int dustType = isWhite ? DustID.WhiteTorch : DustID.BlueTorch;
                Color col = isWhite ? PearlWhite : SoftBlue;
                Dust d = Dust.NewDustPerfect(pos, dustType, vel, 0, col, 1.3f);
                d.noGravity = true;
            }
        }

        /// <summary>
        /// Moonlit shimmer dust — gentle drifting mist-like particles for ambient atmosphere.
        /// </summary>
        public static void SpawnMoonlitShimmer(Vector2 pos, Vector2 awayDirection)
        {
            if (!Main.rand.NextBool(2)) return;
            float hue = 0.55f + Main.rand.NextFloat() * 0.13f;
            Color shimmer = Main.hslToRgb(hue, 0.45f, 0.80f);
            Dust d = Dust.NewDustPerfect(pos, DustID.WhiteTorch,
                awayDirection * 0.5f + Main.rand.NextVector2Circular(1.5f, 1.5f), 0, shimmer, 1.2f);
            d.noGravity = true;
        }

        /// <summary>
        /// Blue-pearl radial dust burst — lunar luminous explosion ring.
        /// </summary>
        public static void SpawnLunarBurst(Vector2 pos, int count = 10, float speed = 5f)
        {
            for (int i = 0; i < count; i++)
            {
                float hue = 0.55f + (float)i / count * 0.13f;
                float angle = MathHelper.TwoPi * i / count;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(speed * 0.6f, speed);
                Color col = Main.hslToRgb(hue, 0.50f, 0.78f);
                Dust d = Dust.NewDustPerfect(pos, DustID.WhiteTorch, vel, 0, col, 1.4f);
                d.noGravity = true;
            }
        }

        // ─────────── CLAIR DE LUNE-SPECIFIC VFX: PEARL SHIMMER ───────────

        /// <summary>
        /// Spawn pearl-like shimmering particles around a position.
        /// The signature Clair de Lune visual identity — soft luminous pearl motes.
        /// </summary>
        public static void SpawnPearlShimmerEffect(Vector2 pos, int count = 3, float radius = 30f, float scale = 0.25f)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(radius, radius);
                Vector2 vel = Main.rand.NextVector2Circular(0.8f, 0.8f) + new Vector2(0f, -0.3f);
                float hue = 0.57f + Main.rand.NextFloat() * 0.10f;
                Color pearlCol = Main.hslToRgb(hue, 0.30f, 0.85f);
                var glow = new GenericGlowParticle(pos + offset, vel,
                    pearlCol * 0.65f, scale, 28, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
        }

        /// <summary>
        /// Spawn tiny clockwork-brass sparkle dust — mechanical precision accents.
        /// </summary>
        public static void SpawnClockworkSparkle(Vector2 pos, int count = 3)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);
                Color brassCol = Color.Lerp(ClockworkBrass, MoonbeamGold, Main.rand.NextFloat(0.3f, 0.8f));
                Dust d = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(8f, 8f),
                    DustID.WhiteTorch, vel, 0, brassCol, 0.9f);
                d.noGravity = true;
            }
        }

        /// <summary>
        /// Spawn gentle drifting mist particles — impressionistic haze.
        /// Creates the dreamy atmospheric veil of Clair de Lune.
        /// </summary>
        public static void SpawnMoonlitMist(Vector2 pos, int count = 3)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.6f, 0.6f), -0.4f - Main.rand.NextFloat(0.3f));
                Color mistCol = Color.Lerp(NightMist, SoftBlue, Main.rand.NextFloat(0.3f, 0.7f));
                var mist = new GenericGlowParticle(
                    pos + Main.rand.NextVector2Circular(20f, 15f), vel,
                    mistCol * 0.40f, 0.35f + Main.rand.NextFloat(0.15f), 35, true);
                MagnumParticleHandler.SpawnParticle(mist);
            }
        }

        /// <summary>
        /// Spawn brief temporal echo afterimage dust — fleeting time distortion.
        /// </summary>
        public static void SpawnTemporalEcho(Vector2 pos, int count = 3)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(1.5f, 1.5f);
                Color echoCol = Color.Lerp(PearlBlue, WhiteHot, Main.rand.NextFloat(0.2f, 0.6f)) * 0.55f;
                Dust d = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(10f, 10f),
                    DustID.WhiteTorch, vel, 0, echoCol, 1.1f);
                d.noGravity = true;
                d.fadeIn = 1.1f;
            }
        }

        // ─────────── CLAIR DE LUNE-SPECIFIC VFX: LUNAR SPARKLES ───────────

        /// <summary>
        /// Spawn lunar sparkle particles — blue-pearl hue cycling points of light.
        /// Creates the dreamy moonlit iridescence of Clair de Lune.
        /// </summary>
        public static void SpawnLunarSparkles(Vector2 pos, int count = 6, float radius = 25f)
        {
            for (int i = 0; i < count; i++)
            {
                float hue = 0.55f + (float)i / count * 0.13f;
                Color sparkColor = Main.hslToRgb(hue, 0.55f, 0.82f);
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);
                Dust d = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(radius, radius),
                    DustID.WhiteTorch, vel, 0, sparkColor, 1.1f);
                d.noGravity = true;
            }
        }

        /// <summary>
        /// Combined pearl-blue sparkle + clockwork sparkle mixed impact explosion.
        /// Clair de Lune signature impact: moonlit pearl iridescence colliding with clockwork brass.
        /// This is the canonical impact effect for ALL Clair de Lune weapon projectile hits.
        /// </summary>
        public static void SpawnMixedSparkleImpact(Vector2 pos, float intensity = 1f, int pearlCount = 6, int brassCount = 6)
        {
            // INNER: White & blue sparkle explosion — pearl-blue duality burst
            for (int i = 0; i < brassCount; i++)
            {
                float angle = MathHelper.TwoPi * i / brassCount + Main.rand.NextFloat(-0.15f, 0.15f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(1.5f, 3.5f) * intensity;
                bool isWhite = i % 2 == 0;
                int dustType = isWhite ? DustID.WhiteTorch : DustID.BlueTorch;
                Color col = isWhite ? PearlWhite : SoftBlue;
                Dust d = Dust.NewDustPerfect(pos, dustType, vel, 0, col, 1.3f * intensity);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            // OUTER: Blue-pearl hue cycling burst — wide moonlit iridescent ring
            for (int i = 0; i < pearlCount; i++)
            {
                float hue = 0.55f + (float)i / pearlCount * 0.13f;
                float angle = MathHelper.TwoPi * i / pearlCount + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f) * intensity;
                Color col = Main.hslToRgb(hue, 0.55f, 0.78f);
                Dust d = Dust.NewDustPerfect(pos, DustID.WhiteTorch, vel, 0, col, 1.1f * intensity);
                d.noGravity = true;
            }

            // Moonlit sparkle accents (scattered between inner and outer)
            int accentCount = Math.Max(1, (int)(3 * intensity));
            for (int i = 0; i < accentCount; i++)
            {
                float hue = 0.55f + Main.rand.NextFloat() * 0.13f;
                Color sparkColor = Main.hslToRgb(hue, 0.45f, 0.82f);
                Vector2 accentVel = Main.rand.NextVector2Circular(2f, 2f) * intensity;
                Dust accent = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(15f * intensity, 15f * intensity),
                    DustID.WhiteTorch, accentVel, 0, sparkColor, 0.9f * intensity);
                accent.noGravity = true;
            }
        }

        /// <summary>
        /// Spawn a moonlit pearl explosion — full pearl-blue spectrum detonation.
        /// </summary>
        public static void SpawnPearlExplosion(Vector2 pos, float intensity = 1f)
        {
            SpawnLunarBurst(pos, (int)(12 * intensity), 6f * intensity);
            SpawnPearlShimmerEffect(pos, (int)(6 * intensity), 30f * intensity, 0.3f * intensity);
        }

        /// <summary>
        /// Spawn dreamy glow particles swirling inward toward a center.
        /// Creates the moonlit convergence effect.
        /// </summary>
        public static void SpawnLunarSwirl(Vector2 center, int count = 6, float radius = 60f, float opacity = 0.65f)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float dist = radius + Main.rand.NextFloat(30f);
                Vector2 particlePos = center + angle.ToRotationVector2() * dist;
                Vector2 vel = (center - particlePos).SafeNormalize(Vector2.Zero) * 3f;

                float hue = 0.55f + (float)i / count * 0.13f;
                Color moonlit = Main.hslToRgb(hue, 0.50f, 0.78f);
                var glow = new GenericGlowParticle(particlePos, vel,
                    moonlit * opacity,
                    0.28f, 22, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
        }

        // ─────────── GRADIENT HALO RINGS ───────────

        /// <summary>
        /// Cascading gradient halo rings — moonlit luminescence (NightMist -> WhiteHot).
        /// </summary>
        public static void SpawnGradientHaloRings(Vector2 pos, int count = 5, float baseScale = 0.3f)
        {
            for (int i = 0; i < count; i++)
            {
                float progress = (float)i / count;
                Color ringCol = Color.Lerp(NightMist, WhiteHot, progress);
                CustomParticles.HaloRing(pos, ringCol, baseScale + i * 0.12f, 14);
            }
        }

        /// <summary>
        /// Pearl halo rings — blue-pearl hue cycling ring cascade.
        /// </summary>
        public static void SpawnPearlHaloRings(Vector2 pos, int count = 5, float baseScale = 0.3f)
        {
            for (int i = 0; i < count; i++)
            {
                float hue = 0.55f + (float)i / count * 0.13f;
                Color ringCol = Main.hslToRgb(hue, 0.50f, 0.80f);
                CustomParticles.HaloRing(pos, ringCol, baseScale + i * 0.12f, 14);
            }
        }

        // ─────────── IMPACTS ───────────

        /// <summary>
        /// Full Clair de Lune melee impact VFX — pearl flash, halo cascade,
        /// pearl-blue dust burst, clockwork sparkle, and music note burst.
        /// Scales with combo step.
        /// </summary>
        public static void MeleeImpact(Vector2 pos, int comboStep = 0)
        {
            int rings = 2 + Math.Min(comboStep, 2);
            SpawnGradientHaloRings(pos, rings);

            int dustCount = 6 + comboStep * 2;
            SpawnRadialDustBurst(pos, dustCount, 4f + comboStep);

            int noteCount = 1 + Math.Min(comboStep, 2);
            SpawnMusicNotes(pos, noteCount, 18f);

            // Clockwork sparkle accent
            SpawnClockworkSparkle(pos, 2 + comboStep);

            // Mixed pearl-blue + clockwork impact (Clair de Lune signature)
            float impactIntensity = 0.6f + comboStep * 0.1f;
            SpawnMixedSparkleImpact(pos, impactIntensity, 4 + comboStep, 4 + comboStep);

            // Pearl-blue halo rings
            try { CustomParticles.HaloRing(pos, PearlWhite, 0.35f, 14); } catch { }
            try { CustomParticles.HaloRing(pos, MidnightBlue, 0.25f, 12); } catch { }

            Lighting.AddLight(pos, PearlWhite.ToVector3() * (0.6f + comboStep * 0.1f));
        }

        /// <summary>
        /// Projectile death / on-kill VFX — bigger, flashier version of MeleeImpact.
        /// Includes pearl shimmer burst, lunar explosion, and enhanced bloom.
        /// </summary>
        public static void ProjectileImpact(Vector2 pos, float intensity = 1f)
        {
            SpawnGradientHaloRings(pos, 3, 0.2f * intensity);
            SpawnMusicNotes(pos, 3, 20f * intensity, 0.6f, 0.9f, 25);
            SpawnRadialDustBurst(pos, 8, 5f * intensity);
            SpawnMixedSparkleImpact(pos, intensity, 6, 6);
            Lighting.AddLight(pos, PearlWhite.ToVector3() * 0.8f * intensity);
        }

        /// <summary>
        /// Phase-3 / finisher slam VFX — screen shake, massive bloom, pearl cascade,
        /// moonlit ring detonation, clockwork burst, music note scatter.
        /// </summary>
        public static void FinisherSlam(Vector2 pos, float intensity = 1f)
        {
            MagnumScreenEffects.AddScreenShake(6f * intensity);
            SpawnGradientHaloRings(pos, 4, 0.25f * intensity);
            SpawnPearlHaloRings(pos, 3, 0.2f * intensity);
            SpawnMusicNotes(pos, 4, 30f, 0.7f, 1.0f, 35);
            SpawnRadialDustBurst(pos, 12, 6f * intensity);
            SpawnLunarBurst(pos, 10, 6f * intensity);
            SpawnMixedSparkleImpact(pos, 1.2f * intensity, 8, 8);
            SpawnLunarSwirl(pos, 6, 60f * intensity);
            SpawnClockworkSparkle(pos, 4);
            Lighting.AddLight(pos, WhiteHot.ToVector3() * 1.0f * intensity);
        }

        // ─────────── SWING HELPERS ───────────

        /// <summary>
        /// Per-frame VFX to call from a swing projectile's AI().
        /// Handles pearl-blue dust trail, moonlit shimmer, periodic clockwork sparkle and music notes.
        /// </summary>
        public static void SwingFrameVFX(Vector2 tipPos, Vector2 swordDirection, int comboStep,
            int timer, int dustType = DustID.WhiteTorch)
        {
            SpawnPearlDust(tipPos, -swordDirection);
            SpawnMoonlitShimmer(tipPos, -swordDirection);

            if (timer % 5 == 0)
                SpawnMusicNotes(tipPos, 1, 10f, 0.7f, 0.9f, 25);

            Lighting.AddLight(tipPos, GetPaletteColor(0.4f + comboStep * 0.15f).ToVector3() * 0.6f);
        }

        // ─────────── DYNAMIC LIGHTING ───────────

        /// <summary>
        /// Add standard Clair de Lune ambient light at a position.
        /// </summary>
        public static void AddClairDeLuneLight(Vector2 worldPos, float intensity = 0.6f)
        {
            Lighting.AddLight(worldPos, PearlWhite.ToVector3() * intensity);
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
        /// Add gentle pulsing pearl light with blue-pearl color shift.
        /// </summary>
        public static void AddPulsingLight(Vector2 worldPos, float time, float intensity = 0.6f)
        {
            float hue = 0.55f + ((time * 0.01f) % 1f) * 0.13f;
            Color pearlLight = Main.hslToRgb(hue, 0.40f, 0.82f);
            Color lightColor = Color.Lerp(PearlWhite, pearlLight, 0.25f);
            float pulse = (float)Math.Sin(time * 0.08f) * 0.15f + 0.85f;
            Lighting.AddLight(worldPos, lightColor.ToVector3() * pulse * intensity);
        }

        /// <summary>
        /// Add moonbeam light — oscillates between SoftBlue and PearlWhite for dreamy ambience.
        /// </summary>
        public static void AddMoonbeamLight(Vector2 worldPos, float time, float intensity = 0.6f)
        {
            float shift = (float)Math.Sin(time * 0.06f) * 0.5f + 0.5f;
            Color lightColor = Color.Lerp(SoftBlue, PearlWhite, shift);
            Lighting.AddLight(worldPos, lightColor.ToVector3() * intensity);
        }

        // ─────────── LUNAR SPARKLE IMPACT (REPLACES NOISE ZONES) ───────────

        /// <summary>
        /// Draws a lunar sparkle impact burst — multiple rotating Star4Soft sparkles
        /// with blue-pearl cycling, a small clamped bloom core, and optional HaloRing edge.
        /// This replaces the old DrawNoiseScrolledZone for impact effects.
        /// Must be called with an active SpriteBatch (TrueAdditive blend recommended).
        /// </summary>
        public static void DrawLunarSparkleImpact(SpriteBatch sb, Vector2 worldPos, float radius,
            float time, float opacity = 1f, int sparkleCount = 8)
        {
            Vector2 drawPos = worldPos - Main.screenPosition;

            // Star4Soft sparkle ring — scattered at varied angles/distances
            Texture2D star = MagnumTextureRegistry.GetStar4Soft();
            if (star != null)
            {
                Vector2 sOrigin = star.Size() * 0.5f;
                for (int i = 0; i < sparkleCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / sparkleCount + time * 0.015f;
                    float dist = radius * (0.4f + 0.5f * MathF.Sin(i * 1.7f + time * 0.03f));
                    Vector2 offset = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * dist;
                    float starRot = time * (0.02f + i * 0.005f) + i * 0.8f;
                    float starScale = MathHelper.Lerp(0.15f, 0.4f, (MathF.Sin(i * 2.1f + time * 0.04f) + 1f) * 0.5f);
                    float hue = 0.55f + ((time * 0.02f + i / (float)sparkleCount) % 1f) * 0.13f;
                    Color starColor = Main.hslToRgb(hue, 0.50f, 0.78f);
                    sb.Draw(star, drawPos + offset, null, (starColor with { A = 0 }) * 0.3f * opacity,
                        starRot, sOrigin, starScale, SpriteEffects.None, 0f);
                }
                // Center star — brighter, larger
                float centerRot = time * 0.035f;
                float centerHue = 0.55f + ((time * 0.025f) % 1f) * 0.13f;
                Color centerColor = Main.hslToRgb(centerHue, 0.35f, 0.85f);
                sb.Draw(star, drawPos, null, (centerColor with { A = 0 }) * 0.45f * opacity,
                    centerRot, sOrigin, 0.35f, SpriteEffects.None, 0f);
            }

            // Moonlit edge ring
            Texture2D ring = MagnumTextureRegistry.GetHaloRing();
            if (ring != null)
            {
                Vector2 rOrigin = ring.Size() * 0.5f;
                float rScale = radius * 2f / ring.Width;
                float ringHue = 0.55f + ((time * 0.035f) % 1f) * 0.13f;
                Color rc = Main.hslToRgb(ringHue, 0.45f, 0.72f);
                sb.Draw(ring, drawPos, null, (rc with { A = 0 }) * 0.2f * opacity,
                    time * 0.02f, rOrigin, rScale, SpriteEffects.None, 0f);
            }
        }

        // ─────────── THEME TEXTURE VFX ───────────
        // Uses ClairDeLuneThemeTextures for theme-specific visuals.

        /// <summary>
        /// Draws a themed moonlit impact ring using Clair de Lune Power Effect Ring + Radial Slash Impact.
        /// Must be called in Additive blend mode (or {A=0} pattern).
        /// </summary>
        public static void DrawThemeImpactRing(SpriteBatch sb, Vector2 worldPos,
            float scale, float intensity = 1f, float rotation = 0f)
        {
            Vector2 drawPos = worldPos - Main.screenPosition;

            Texture2D ring = ClairDeLuneThemeTextures.CLPowerEffectRing?.Value;
            if (ring != null)
            {
                Vector2 origin = ring.Size() * 0.5f;
                sb.Draw(ring, drawPos, null,
                    (PearlWhite with { A = 0 }) * 0.5f * intensity, rotation, origin,
                    scale * 0.15f, SpriteEffects.None, 0f);
                sb.Draw(ring, drawPos, null,
                    (PearlBlue with { A = 0 }) * 0.3f * intensity, -rotation * 0.7f, origin,
                    scale * 0.10f, SpriteEffects.None, 0f);
            }

            Texture2D impact = ClairDeLuneThemeTextures.CLRadialSlashImpact?.Value;
            if (impact != null)
            {
                Vector2 impOrigin = impact.Size() * 0.5f;
                sb.Draw(impact, drawPos, null,
                    (SoftBlue with { A = 0 }) * 0.45f * intensity, rotation * 1.3f, impOrigin,
                    scale * 0.12f, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Draws a themed clock gear fragment particle accent at a position.
        /// Must be called in Additive blend mode.
        /// </summary>
        public static void DrawThemeGearAccent(SpriteBatch sb, Vector2 worldPos,
            float scale, float intensity = 1f)
        {
            Vector2 drawPos = worldPos - Main.screenPosition;

            Texture2D gear = ClairDeLuneThemeTextures.CLClockGearFragment?.Value;
            if (gear != null)
            {
                Vector2 origin = gear.Size() * 0.5f;
                float rot = (float)Main.GameUpdateCount * 0.04f;
                sb.Draw(gear, drawPos, null,
                    (PearlWhite with { A = 0 }) * 0.45f * intensity, rot, origin,
                    scale * 0.07f, SpriteEffects.None, 0f);
                sb.Draw(gear, drawPos, null,
                    (ClockworkBrass with { A = 0 }) * 0.3f * intensity, -rot * 0.6f, origin,
                    scale * 0.05f, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Combined theme impact: universal bloom + theme ring + gear accents.
        /// </summary>
        public static void DrawThemeImpactFull(SpriteBatch sb, Vector2 worldPos,
            float scale, float intensity = 1f)
        {
            DrawThemeGearAccent(sb, worldPos, scale, intensity * 0.7f);
            float rot = (float)Main.GameUpdateCount * 0.02f;
            DrawThemeImpactRing(sb, worldPos, scale, intensity * 0.6f, rot);
        }
    }
}
