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

namespace MagnumOpus.Content.Nachtmusik
{
    /// <summary>
    /// Shared Nachtmusik VFX library — canonical palette, bloom stacking,
    /// shader setup, trail helpers, music notes, dust, star spawns,
    /// celestial effects, and impact VFX used by ALL Nachtmusik weapons,
    /// accessories, projectiles, minions, enemies, and bosses.
    ///
    /// Nachtmusik identity: Nocturnal wonder, starlit elegance,
    /// twinkling stars, serenade charm, the Queen of Radiance's celestial grace.
    /// </summary>
    public static class NachtmusikVFXLibrary
    {
        // ─────────── CANONICAL PALETTE (forwarded from NachtmusikPalette) ───────────
        public static readonly Color MidnightBlue    = NachtmusikPalette.MidnightBlue;
        public static readonly Color DeepBlue        = NachtmusikPalette.DeepBlue;
        public static readonly Color StarlitBlue     = NachtmusikPalette.StarlitBlue;
        public static readonly Color StarWhite       = NachtmusikPalette.StarWhite;
        public static readonly Color MoonlitSilver   = NachtmusikPalette.MoonlitSilver;
        public static readonly Color TwinklingWhite  = NachtmusikPalette.TwinklingWhite;

        // Convenience accessors
        public static readonly Color RadianceGold    = NachtmusikPalette.RadianceGold;
        public static readonly Color StarGold        = NachtmusikPalette.StarGold;
        public static readonly Color Silver          = NachtmusikPalette.Silver;

        // Palette as array for indexed access
        private static readonly Color[] Palette = { MidnightBlue, DeepBlue, StarlitBlue, StarWhite, MoonlitSilver, TwinklingWhite };

        // Hue range for HueShiftingMusicNoteParticle (blue-indigo spectrum)
        private const float HueMin = 0.58f;
        private const float HueMax = 0.72f;
        private const float NoteSaturation = 0.80f;
        private const float NoteLuminosity = 0.85f;

        // Nachtmusik glow profile for GlowRenderer
        public static readonly GlowRenderer.GlowLayer[] NachtmusikGlowProfile = new[]
        {
            new GlowRenderer.GlowLayer(1.0f, 1.0f, new Color(248, 250, 255)),    // TwinklingWhite core
            new GlowRenderer.GlowLayer(1.6f, 0.65f, new Color(200, 210, 240)),   // StarWhite inner
            new GlowRenderer.GlowLayer(2.5f, 0.4f, new Color(80, 120, 200)),     // StarlitBlue mid
            new GlowRenderer.GlowLayer(4.0f, 0.2f, new Color(30, 50, 120))       // DeepBlue outer
        };

        // ─────────── PALETTE INTERPOLATION ───────────

        /// <summary>
        /// Lerp through the 6-colour Nachtmusik palette. t=0 -> MidnightBlue, t=1 -> TwinklingWhite.
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
        /// Drop-in replacement for the GetNachtmusikGradient() in weapon files.
        /// MidnightBlue -> StarlitBlue -> TwinklingWhite over 0->1.
        /// </summary>
        public static Color GetNachtmusikGradient(float progress)
            => NachtmusikPalette.GetCelestialGradient(progress);

        /// <summary>
        /// Get a cycling starlit color within the blue-indigo hue range (0.58-0.72).
        /// </summary>
        public static Color GetStarlitCycle(float offset = 0f)
        {
            float hue = HueMin + ((float)(Main.timeForVisualEffects * 0.02 + offset) % 1f) * (HueMax - HueMin);
            return Main.hslToRgb(hue, 0.7f, 0.7f);
        }

        /// <summary>
        /// Get a vivid starlit color (higher saturation) within the blue-indigo range.
        /// </summary>
        public static Color GetVividStarlight(float offset = 0f)
        {
            float hue = HueMin + ((float)(Main.timeForVisualEffects * 0.025 + offset) % 1f) * (HueMax - HueMin);
            return Main.hslToRgb(hue, 0.9f, 0.75f);
        }

        // ─────────── SPRITEBATCH STATE HELPERS ───────────

        /// <summary>
        /// Switch SpriteBatch to additive blend for Nachtmusik VFX rendering.
        /// </summary>
        public static void BeginNachtmusikAdditive(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Restore SpriteBatch to standard AlphaBlend after additive rendering.
        /// </summary>
        public static void EndNachtmusikAdditive(SpriteBatch sb)
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
        /// Counter-rotating double flare — starlight and deep blue spinning in opposite directions.
        /// Creates the signature celestial dual-energy at projectile centers.
        /// </summary>
        public static void DrawCounterRotatingFlares(SpriteBatch sb, Vector2 worldPos,
            float scale, float time, float opacity = 1f)
        {
            Texture2D flare = MagnumTextureRegistry.GetFlare();
            if (flare == null) return;

            // 1024px flare — cap so largest layer (scale*0.7) <= 0.293 -> <=300px
            scale = MathHelper.Min(scale, 0.419f);

            Vector2 drawPos = worldPos - Main.screenPosition;
            Vector2 origin = flare.Size() * 0.5f;

            float rot1 = time * 2.5f;
            float rot2 = -time * 1.8f;

            sb.Draw(flare, drawPos, null,
                (StarWhite with { A = 0 }) * 0.6f * opacity, rot1, origin, scale * 0.7f, SpriteEffects.None, 0f);
            sb.Draw(flare, drawPos, null,
                (DeepBlue with { A = 0 }) * 0.5f * opacity, rot2, origin, scale * 0.5f, SpriteEffects.None, 0f);
        }


        // ─────────── GLOW RENDERER INTEGRATION ───────────

        /// <summary>
        /// Draw Nachtmusik-themed multi-layer glow via GlowRenderer.
        /// </summary>
        public static void DrawNachtmusikGlow(SpriteBatch sb, Vector2 worldPos,
            float scale = 1f, float intensity = 1f, string rotationId = null)
        {
            GlowRenderer.DrawGlow(sb, worldPos, NachtmusikGlowProfile, TwinklingWhite, intensity * scale, rotationId);
        }

        /// <summary>
        /// Draw Nachtmusik glow with automatic SpriteBatch state management.
        /// </summary>
        public static void DrawNachtmusikGlowManaged(SpriteBatch sb, Vector2 worldPos,
            float scale = 1f, float intensity = 1f, string rotationId = null)
        {
            GlowRenderer.DrawGlowManaged(sb, worldPos, NachtmusikGlowProfile, TwinklingWhite, intensity * scale, rotationId);
        }

        // ─────────── TRAIL WIDTH/COLOR FUNCTIONS ───────────

        /// <summary>
        /// Standard Nachtmusik trail width: elegant celestial taper.
        /// </summary>
        public static float NachtmusikTrailWidth(float completionRatio, float baseWidth = 18f)
        {
            float tipFade = MathF.Pow(1f - completionRatio, 0.6f);
            float headFade = MathF.Pow(completionRatio, 3f);
            return baseWidth * tipFade * (1f - headFade);
        }

        /// <summary>
        /// Thin precision trail for elegant ranged weapons — starlit shot.
        /// </summary>
        public static float PrecisionTrailWidth(float completionRatio, float baseWidth = 6f)
        {
            float taper = 1f - completionRatio;
            return baseWidth * taper * taper;
        }

        /// <summary>
        /// Wide graceful trail for melee weapons — sweeping celestial arc.
        /// </summary>
        public static float GracefulTrailWidth(float completionRatio, float baseWidth = 16f)
        {
            float tipFade = MathF.Pow(1f - completionRatio, 0.4f);
            return baseWidth * tipFade;
        }

        /// <summary>
        /// Trail color function with {A=0} for additive rendering.
        /// Nocturnal gradient: deep at edges, starlit center along trail.
        /// </summary>
        public static Color NachtmusikTrailColor(float completionRatio, float whitePush = 0.45f)
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
                0 => (MidnightBlue.ToVector3(), DeepBlue.ToVector3()),
                1 => (StarlitBlue.ToVector3(), StarWhite.ToVector3()),
                2 => (StarWhite.ToVector3(), TwinklingWhite.ToVector3()),
                _ => (DeepBlue.ToVector3(), StarWhite.ToVector3()),
            };
        }

        // ─────────── MUSIC NOTES ───────────

        /// <summary>
        /// Spawn visible, hue-shifting Nachtmusik music notes at the given position.
        /// Notes cycle through the blue-indigo spectrum (0.58-0.72) for nocturnal effect.
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
        /// Dense Nachtmusik starlit dust trail at a blade tip during a swing.
        /// Palette-tinted white torch dust for the signature nocturnal shimmer.
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
        /// Nachtmusik star dust — alternating white torch and rainbow torch.
        /// Creates the signature starlit duality of cool silver and warm celestial blue.
        /// </summary>
        public static void SpawnStarDust(Vector2 pos, Vector2 awayDirection)
        {
            for (int i = 0; i < 2; i++)
            {
                Vector2 vel = awayDirection * Main.rand.NextFloat(1f, 3f) + Main.rand.NextVector2Circular(0.5f, 0.5f);
                bool isWhite = Main.rand.NextBool();
                int dustType = isWhite ? DustID.WhiteTorch : DustID.RainbowTorch;
                Color col = isWhite ? StarWhite : StarlitBlue;
                Dust d = Dust.NewDustPerfect(pos, dustType, vel, 0, col, 1.5f);
                d.noGravity = true;
                d.fadeIn = 1.3f;
            }
        }

        /// <summary>
        /// Radial dust burst for on-hit / impact VFX.
        /// Starlit pattern: alternating white torch and blue-tinted rainbow torch around the burst ring.
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
                int dustType = isWhite ? DustID.WhiteTorch : DustID.RainbowTorch;
                Color col = isWhite ? StarWhite : StarlitBlue;
                Dust d = Dust.NewDustPerfect(pos, dustType, vel, 0, col, 1.3f);
                d.noGravity = true;
            }
        }

        /// <summary>
        /// Star shimmer dust — blue-indigo sparkle trail.
        /// </summary>
        public static void SpawnStarShimmer(Vector2 pos, Vector2 awayDirection)
        {
            if (!Main.rand.NextBool(2)) return;
            float hue = HueMin + Main.rand.NextFloat() * (HueMax - HueMin);
            Color starlit = Main.hslToRgb(hue, 0.8f, 0.8f);
            Dust d = Dust.NewDustPerfect(pos, DustID.RainbowTorch,
                awayDirection * 0.5f + Main.rand.NextVector2Circular(1.5f, 1.5f), 0, starlit, 1.2f);
            d.noGravity = true;
        }

        /// <summary>
        /// Celestial radial dust burst — blue-indigo explosion ring.
        /// </summary>
        public static void SpawnCelestialBurst(Vector2 pos, int count = 10, float speed = 5f)
        {
            for (int i = 0; i < count; i++)
            {
                float hue = HueMin + (float)i / count * (HueMax - HueMin);
                float angle = MathHelper.TwoPi * i / count;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(speed * 0.6f, speed);
                Color col = Main.hslToRgb(hue, 0.85f, 0.75f);
                Dust d = Dust.NewDustPerfect(pos, DustID.RainbowTorch, vel, 0, col, 1.4f);
                d.noGravity = true;
            }
        }

        // ─────────── NACHTMUSIK-SPECIFIC VFX: STARS & CONSTELLATIONS ───────────

        /// <summary>
        /// Spawn twinkling star dust particles around a position.
        /// The signature Nachtmusik visual identity — delicate points of starlight.
        /// </summary>
        public static void SpawnStarTwinkle(Vector2 pos, int count = 3, float radius = 30f, float scale = 0.25f)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(radius, radius);
                float hue = HueMin + Main.rand.NextFloat() * (HueMax - HueMin);
                Color starColor = Main.hslToRgb(hue, 0.7f, 0.85f);
                Vector2 vel = Main.rand.NextVector2Circular(0.5f, 0.5f) + new Vector2(0f, -0.3f);
                float dustScale = scale * 4f + Main.rand.NextFloat(0.5f);
                Dust d = Dust.NewDustPerfect(pos + offset, DustID.WhiteTorch, vel, 0, starColor, dustScale);
                d.noGravity = true;
                d.fadeIn = 1.1f;
            }
        }

        /// <summary>
        /// Spawn a dotted star trail between two points — a dust-based constellation line.
        /// Creates the signature Nachtmusik constellation connection effect.
        /// </summary>
        public static void SpawnConstellationLine(Vector2 from, Vector2 to, int stars = 5)
        {
            Vector2 direction = to - from;
            float length = direction.Length();
            if (length < 1f) return;
            Vector2 step = direction / stars;

            for (int i = 0; i <= stars; i++)
            {
                Vector2 starPos = from + step * i + Main.rand.NextVector2Circular(2f, 2f);
                float hue = HueMin + (float)i / stars * (HueMax - HueMin);
                Color starColor = Main.hslToRgb(hue, 0.75f, 0.85f);
                float starScale = (i == 0 || i == stars) ? 1.4f : 0.9f + Main.rand.NextFloat(0.3f);
                Dust d = Dust.NewDustPerfect(starPos, DustID.WhiteTorch, Vector2.Zero, 0, starColor, starScale);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }
        }

        /// <summary>
        /// Spawn a burst of stars exploding outward from impact point.
        /// Radial star explosion — the Nachtmusik signature detonation.
        /// </summary>
        public static void SpawnStarBurst(Vector2 pos, int count = 6, float scale = 0.3f)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                float hue = HueMin + (float)i / count * (HueMax - HueMin);
                Color starColor = Main.hslToRgb(hue, 0.8f, 0.85f);
                float dustScale = scale * 5f + Main.rand.NextFloat(0.5f);
                Dust d = Dust.NewDustPerfect(pos, DustID.WhiteTorch, vel, 0, starColor, dustScale);
                d.noGravity = true;
                d.fadeIn = 1.3f;
            }
        }

        // ─────────── NACHTMUSIK-SPECIFIC VFX: CELESTIAL ───────────

        /// <summary>
        /// Spawn celestial sparkle particles — blue-indigo hue cycling points of light.
        /// Creates the nocturnal shimmer effect characteristic of Nachtmusik weapons.
        /// </summary>
        public static void SpawnCelestialSparkles(Vector2 pos, int count = 6, float radius = 25f)
        {
            for (int i = 0; i < count; i++)
            {
                float hue = HueMin + (float)i / count * (HueMax - HueMin);
                Color sparkColor = Main.hslToRgb(hue, 0.85f, 0.8f);
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);
                Dust d = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(radius, radius),
                    DustID.RainbowTorch, vel, 0, sparkColor, 1.1f);
                d.noGravity = true;
            }
        }

        /// <summary>
        /// Combined starlit sparkle + star dust mixed explosion.
        /// Nachtmusik signature impact: blue-indigo celestial fire colliding with starlit duality.
        /// This is the canonical impact effect for ALL Nachtmusik weapon projectile hits.
        /// </summary>
        public static void SpawnMixedSparkleImpact(Vector2 pos, float intensity = 1f, int celestialCount = 6, int starCount = 6)
        {
            // INNER: White torch & rainbow torch sparkle explosion — tight starlit duality burst
            for (int i = 0; i < starCount; i++)
            {
                float angle = MathHelper.TwoPi * i / starCount + Main.rand.NextFloat(-0.15f, 0.15f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(1.5f, 3.5f) * intensity;
                bool isWhite = i % 2 == 0;
                int dustType = isWhite ? DustID.WhiteTorch : DustID.RainbowTorch;
                Color col = isWhite ? StarWhite : StarlitBlue;
                Dust d = Dust.NewDustPerfect(pos, dustType, vel, 0, col, 1.3f * intensity);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            // OUTER: Blue-hue cycling sparkle explosion — wide celestial burst
            for (int i = 0; i < celestialCount; i++)
            {
                float hue = HueMin + (float)i / celestialCount * (HueMax - HueMin);
                float angle = MathHelper.TwoPi * i / celestialCount + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f) * intensity;
                Color col = Main.hslToRgb(hue, 0.85f, 0.75f);
                Dust d = Dust.NewDustPerfect(pos, DustID.RainbowTorch, vel, 0, col, 1.1f * intensity);
                d.noGravity = true;
            }

            // Celestial sparkle accents (dust-based, scattered between inner and outer)
            int accentCount = Math.Max(1, (int)(3 * intensity));
            for (int i = 0; i < accentCount; i++)
            {
                float hue = HueMin + Main.rand.NextFloat() * (HueMax - HueMin);
                Color sparkColor = Main.hslToRgb(hue, 0.8f, 0.8f);
                Vector2 accentVel = Main.rand.NextVector2Circular(2f, 2f) * intensity;
                Dust accent = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(15f * intensity, 15f * intensity),
                    DustID.RainbowTorch, accentVel, 0, sparkColor, 0.9f * intensity);
                accent.noGravity = true;
            }
        }

        /// <summary>
        /// Spawn a celestial starburst explosion — blue-indigo spectrum detonation.
        /// </summary>
        public static void SpawnStarburstExplosion(Vector2 pos, float intensity = 1f)
        {
            SpawnStarBurst(pos, (int)(8 * intensity), 0.35f * intensity);
            SpawnCelestialBurst(pos, (int)(10 * intensity), 6f * intensity);
        }

        /// <summary>
        /// Spawn celestial glow particles swirling inward toward a center.
        /// Creates the nocturnal convergence effect.
        /// </summary>
        public static void SpawnCelestialSwirl(Vector2 center, int count = 6, float radius = 60f, float opacity = 0.65f)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float dist = radius + Main.rand.NextFloat(30f);
                Vector2 particlePos = center + angle.ToRotationVector2() * dist;
                Vector2 vel = (center - particlePos).SafeNormalize(Vector2.Zero) * 3f;

                Color starlit = GetStarlitCycle((float)i / count);
                var glow = new GenericGlowParticle(particlePos, vel,
                    starlit * opacity,
                    0.28f, 22, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
        }

        // ─────────── GRADIENT HALO RINGS ───────────

        /// <summary>
        /// Cascading gradient halo rings — nocturnal (MidnightBlue -> TwinklingWhite).
        /// </summary>
        public static void SpawnGradientHaloRings(Vector2 pos, int count = 5, float baseScale = 0.3f)
        {
            for (int i = 0; i < count; i++)
            {
                float progress = (float)i / count;
                Color ringCol = Color.Lerp(MidnightBlue, TwinklingWhite, progress);
                CustomParticles.HaloRing(pos, ringCol, baseScale + i * 0.12f, 14);
            }
        }

        /// <summary>
        /// Star halo rings — blue-gold hue cycling ring cascade.
        /// </summary>
        public static void SpawnStarHaloRings(Vector2 pos, int count = 5, float baseScale = 0.3f)
        {
            for (int i = 0; i < count; i++)
            {
                float progress = (float)i / count;
                Color ringCol = Color.Lerp(StarlitBlue, RadianceGold, progress);
                CustomParticles.HaloRing(pos, ringCol, baseScale + i * 0.12f, 14);
            }
        }

        // ─────────── IMPACTS ───────────

        /// <summary>
        /// Full Nachtmusik melee impact VFX — starlit bloom flash, halo cascade,
        /// celestial dust burst, star sparkles, star twinkle scatter, and music note burst.
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

            // Mixed celestial + starlit sparkle impact (Nachtmusik signature)
            float impactIntensity = 0.6f + comboStep * 0.1f;
            SpawnMixedSparkleImpact(pos, impactIntensity, 4 + comboStep, 4 + comboStep);

            // Starlit halo rings
            try { CustomParticles.HaloRing(pos, TwinklingWhite, 0.35f, 14); } catch { }
            try { CustomParticles.HaloRing(pos, DeepBlue, 0.25f, 12); } catch { }

            Lighting.AddLight(pos, TwinklingWhite.ToVector3() * (0.6f + comboStep * 0.1f));
        }

        /// <summary>
        /// Projectile death / on-kill VFX — bigger, flashier version of MeleeImpact.
        /// Includes star burst, celestial explosion, and enhanced bloom.
        /// </summary>
        public static void ProjectileImpact(Vector2 pos, float intensity = 1f)
        {
            SpawnGradientHaloRings(pos, 3, 0.2f * intensity);
            SpawnMusicNotes(pos, 3, 20f * intensity, 0.6f, 0.9f, 25);
            SpawnRadialDustBurst(pos, 8, 5f * intensity);
            SpawnMixedSparkleImpact(pos, intensity, 6, 6);
            Lighting.AddLight(pos, TwinklingWhite.ToVector3() * 0.8f * intensity);
        }

        // ─────────── SWING HELPERS ───────────

        /// <summary>
        /// Per-frame VFX to call from a swing projectile's AI().
        /// Handles starlit dust trail, star shimmer, periodic star twinkles and music notes.
        /// </summary>
        public static void SwingFrameVFX(Vector2 tipPos, Vector2 swordDirection, int comboStep,
            int timer, int dustType = DustID.WhiteTorch)
        {
            SpawnStarDust(tipPos, -swordDirection);
            SpawnStarShimmer(tipPos, -swordDirection);

            if (timer % 5 == 0)
                SpawnMusicNotes(tipPos, 1, 10f, 0.7f, 0.9f, 25);

            Lighting.AddLight(tipPos, GetPaletteColor(0.4f + comboStep * 0.15f).ToVector3() * 0.6f);
        }

        // ─────────── FINISHER EFFECTS ───────────

        /// <summary>
        /// Phase-3 / finisher slam VFX — screen shake, massive bloom, starlit cascade,
        /// star burst explosion, celestial detonation, music note scatter.
        /// </summary>
        public static void FinisherSlam(Vector2 pos, float intensity = 1f)
        {
            MagnumScreenEffects.AddScreenShake(6f * intensity);
            SpawnGradientHaloRings(pos, 4, 0.25f * intensity);
            SpawnStarHaloRings(pos, 3, 0.2f * intensity);
            SpawnMusicNotes(pos, 4, 30f, 0.7f, 1.0f, 35);
            SpawnRadialDustBurst(pos, 12, 6f * intensity);
            SpawnCelestialBurst(pos, 10, 6f * intensity);
            SpawnMixedSparkleImpact(pos, 1.2f * intensity, 8, 8);
            SpawnCelestialSwirl(pos, 6, 60f * intensity);
            Lighting.AddLight(pos, TwinklingWhite.ToVector3() * 1.0f * intensity);
        }

        // ─────────── DYNAMIC LIGHTING ───────────

        /// <summary>
        /// Add standard Nachtmusik ambient light at a position.
        /// </summary>
        public static void AddNachtmusikLight(Vector2 worldPos, float intensity = 0.6f)
        {
            Lighting.AddLight(worldPos, TwinklingWhite.ToVector3() * intensity);
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
        /// Add pulsing Nachtmusik light with blue-indigo color shift.
        /// </summary>
        public static void AddPulsingLight(Vector2 worldPos, float time, float intensity = 0.6f)
        {
            Color starlit = GetStarlitCycle(time * 0.01f);
            Color lightColor = Color.Lerp(TwinklingWhite, starlit, 0.25f);
            float pulse = (float)Math.Sin(time * 0.08f) * 0.15f + 0.85f;
            Lighting.AddLight(worldPos, lightColor.ToVector3() * pulse * intensity);
        }

        /// <summary>
        /// Add starlit flickering light — oscillates between StarlitBlue and RadianceGold.
        /// The Queen of Radiance's signature celestial glow.
        /// </summary>
        public static void AddStarlitLight(Vector2 worldPos, float time, float intensity = 0.6f)
        {
            float shift = (float)Math.Sin(time * 0.06f) * 0.5f + 0.5f;
            Color lightColor = Color.Lerp(StarlitBlue, RadianceGold, shift);
            Lighting.AddLight(worldPos, lightColor.ToVector3() * intensity);
        }

        // ─────────── CELESTIAL SPARKLE IMPACT (REPLACES NOISE ZONES) ───────────

        /// <summary>
        /// Draws a celestial sparkle impact burst — multiple rotating Star4Soft sparkles
        /// with blue-indigo cycling, a small clamped bloom core, and optional HaloRing edge.
        /// This replaces the old DrawNoiseScrolledZone for impact effects.
        /// Must be called with an active SpriteBatch (TrueAdditive blend recommended).
        /// </summary>
        public static void DrawCelestialSparkleImpact(SpriteBatch sb, Vector2 worldPos, float radius,
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
                    float hue = HueMin + ((time * 0.02f + i / (float)sparkleCount) % 1f) * (HueMax - HueMin);
                    Color starColor = Main.hslToRgb(hue, 0.85f, 0.7f);
                    sb.Draw(star, drawPos + offset, null, (starColor with { A = 0 }) * 0.3f * opacity,
                        starRot, sOrigin, starScale, SpriteEffects.None, 0f);
                }
                // Center star — brighter, larger
                float centerRot = time * 0.035f;
                float centerHue = HueMin + ((time * 0.025f) % 1f) * (HueMax - HueMin);
                Color centerColor = Main.hslToRgb(centerHue, 0.6f, 0.8f);
                sb.Draw(star, drawPos, null, (centerColor with { A = 0 }) * 0.45f * opacity,
                    centerRot, sOrigin, 0.35f, SpriteEffects.None, 0f);
            }

            // Celestial edge ring
            Texture2D ring = MagnumTextureRegistry.GetHaloRing();
            if (ring != null)
            {
                Vector2 rOrigin = ring.Size() * 0.5f;
                float rScale = radius * 2f / ring.Width;
                float ringHue = HueMin + ((time * 0.035f) % 1f) * (HueMax - HueMin);
                Color rc = Main.hslToRgb(ringHue, 0.85f, 0.65f);
                sb.Draw(ring, drawPos, null, (rc with { A = 0 }) * 0.2f * opacity,
                    time * 0.02f, rOrigin, rScale, SpriteEffects.None, 0f);
            }
        }

        // ─────────── THEME TEXTURE VFX ───────────
        // Uses NachtmusikThemeTextures for theme-specific visuals.

        /// <summary>
        /// Draws a themed celestial impact ring using Nachtmusik Power Effect Ring + Radial Slash Impact.
        /// Must be called in Additive blend mode (or {A=0} pattern).
        /// </summary>
        public static void DrawThemeImpactRing(SpriteBatch sb, Vector2 worldPos,
            float scale, float intensity = 1f, float rotation = 0f)
        {
            Vector2 drawPos = worldPos - Main.screenPosition;

            Texture2D ring = NachtmusikThemeTextures.NKPowerEffectRing?.Value;
            if (ring != null)
            {
                Vector2 origin = ring.Size() * 0.5f;
                sb.Draw(ring, drawPos, null,
                    (TwinklingWhite with { A = 0 }) * 0.5f * intensity, rotation, origin,
                    scale * 0.15f, SpriteEffects.None, 0f);
                sb.Draw(ring, drawPos, null,
                    (StarlitBlue with { A = 0 }) * 0.3f * intensity, -rotation * 0.7f, origin,
                    scale * 0.10f, SpriteEffects.None, 0f);
            }

            Texture2D impact = NachtmusikThemeTextures.NKRadialSlashImpact?.Value;
            if (impact != null)
            {
                Vector2 impOrigin = impact.Size() * 0.5f;
                sb.Draw(impact, drawPos, null,
                    (StarWhite with { A = 0 }) * 0.45f * intensity, rotation * 1.3f, impOrigin,
                    scale * 0.12f, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Draws a themed star accent using the Nachtmusik Lens Flare texture.
        /// Must be called in Additive blend mode.
        /// </summary>
        public static void DrawThemeStarAccent(SpriteBatch sb, Vector2 worldPos,
            float scale, float intensity = 1f)
        {
            Vector2 drawPos = worldPos - Main.screenPosition;

            Texture2D flare = NachtmusikThemeTextures.NKLensFlare?.Value;
            if (flare != null)
            {
                Vector2 origin = flare.Size() * 0.5f;
                float rot = (float)Main.GameUpdateCount * 0.04f;
                sb.Draw(flare, drawPos, null,
                    (TwinklingWhite with { A = 0 }) * 0.45f * intensity, rot, origin,
                    scale * 0.07f, SpriteEffects.None, 0f);
                sb.Draw(flare, drawPos, null,
                    (StarlitBlue with { A = 0 }) * 0.3f * intensity, -rot * 0.6f, origin,
                    scale * 0.05f, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Combined theme impact: star accent + theme ring.
        /// </summary>
        public static void DrawThemeImpactFull(SpriteBatch sb, Vector2 worldPos,
            float scale, float intensity = 1f)
        {
            DrawThemeStarAccent(sb, worldPos, scale, intensity * 0.7f);
            float rot = (float)Main.GameUpdateCount * 0.02f;
            DrawThemeImpactRing(sb, worldPos, scale, intensity * 0.6f, rot);
        }
    }
}
