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
    /// shader setup, trail helpers, music notes, dust, pearl shimmer, moonlit mist,
    /// and impact VFX used by ALL Clair de Lune weapons, accessories, projectiles,
    /// minions, and enemies.
    ///
    /// Theme identity: Debussy's moonlit reverie meeting clockwork temporal power.
    /// Dreamy blue-pearl shimmer, gentle moonlit mist, starlit sparkles,
    /// impressionistic haze — the supreme final boss tier.
    /// </summary>
    public static class ClairDeLuneVFXLibrary
    {
        // ─────────── CANONICAL PALETTE (forwarded from ClairDeLunePalette) ───────────
        // Convenience aliases so calling code can use either
        // ClairDeLuneVFXLibrary.SoftBlue or ClairDeLunePalette.SoftBlue interchangeably.
        public static readonly Color NightMist      = ClairDeLunePalette.NightMist;
        public static readonly Color MidnightBlue   = ClairDeLunePalette.MidnightBlue;
        public static readonly Color SoftBlue       = ClairDeLunePalette.SoftBlue;
        public static readonly Color PearlBlue      = ClairDeLunePalette.PearlBlue;
        public static readonly Color PearlWhite     = ClairDeLunePalette.PearlWhite;
        public static readonly Color WhiteHot       = ClairDeLunePalette.WhiteHot;

        // Convenience accessors
        public static readonly Color ClockworkBrass = ClairDeLunePalette.ClockworkBrass;
        public static readonly Color MoonbeamGold   = ClairDeLunePalette.MoonbeamGold;

        // Palette as array for indexed access
        private static readonly Color[] Palette = { NightMist, MidnightBlue, SoftBlue, PearlBlue, PearlWhite, WhiteHot };

        // Hue range for HueShiftingMusicNoteParticle (blue-pearl band)
        private const float HueMin = 0.55f;
        private const float HueMax = 0.68f;
        private const float NoteSaturation = 0.55f;
        private const float NoteLuminosity = 0.70f;

        // Clair de Lune glow profile for GlowRenderer
        public static readonly GlowRenderer.GlowLayer[] ClairDeLuneGlowProfile = new[]
        {
            new GlowRenderer.GlowLayer(1.0f, 1.0f, Color.White),
            new GlowRenderer.GlowLayer(1.6f, 0.65f, new Color(160, 195, 235)),  // PearlBlue
            new GlowRenderer.GlowLayer(2.5f, 0.4f, new Color(100, 140, 200)),   // SoftBlue
            new GlowRenderer.GlowLayer(4.0f, 0.2f, new Color(60, 80, 140))      // MidnightBlue
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
        /// Palette colour with Calamity-style white push for perceived brilliance.
        /// push=0 returns pure palette, push=1 returns full white.
        /// </summary>
        public static Color GetPaletteColorWithWhitePush(float t, float push)
        {
            Color baseCol = GetPaletteColor(t);
            return Color.Lerp(baseCol, Color.White, MathHelper.Clamp(push, 0f, 1f));
        }

        /// <summary>
        /// Drop-in replacement for the GetClairDeLuneGradient() duplicated in weapon files.
        /// NightMist -> SoftBlue -> PearlWhite over 0->1.
        /// </summary>
        public static Color GetClairDeLuneGradient(float progress)
            => ClairDeLunePalette.GetClairDeLuneGradient(progress);

        // ─────────── SPRITEBATCH STATE HELPERS ───────────

        /// <summary>
        /// Switch SpriteBatch to additive blend for Clair de Lune VFX rendering.
        /// Call EndClairDeLuneAdditive when done.
        /// </summary>
        public static void BeginClairDeLuneAdditive(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
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
            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive,
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
        /// Renders additively under AlphaBlend without SpriteBatch restart.
        /// Dreamy moonlit profile: MidnightBlue outer -> SoftBlue mid -> PearlBlue inner -> WhiteHot core.
        /// </summary>
        public static void DrawClairDeLuneBloomStack(SpriteBatch sb, Vector2 worldPos,
            float scale, float paletteT = 0.4f, float opacity = 1f)
        {
            Texture2D bloom = MagnumTextureRegistry.GetBloom();
            if (bloom == null) return;

            Vector2 drawPos = worldPos - Main.screenPosition;
            Vector2 origin = bloom.Size() * 0.5f;

            Color outer = GetPaletteColor(MathHelper.Clamp(paletteT - 0.2f, 0f, 1f));
            Color inner = GetPaletteColor(MathHelper.Clamp(paletteT + 0.2f, 0f, 1f));

            // Layer 1: Outer midnight blue halo
            sb.Draw(bloom, drawPos, null,
                (outer with { A = 0 }) * 0.3f * opacity, 0f, origin, scale * 2.0f, SpriteEffects.None, 0f);

            // Layer 2: Mid soft blue glow
            sb.Draw(bloom, drawPos, null,
                (outer with { A = 0 }) * 0.5f * opacity, 0f, origin, scale * 1.4f, SpriteEffects.None, 0f);

            // Layer 3: Inner pearl blue bloom
            sb.Draw(bloom, drawPos, null,
                (inner with { A = 0 }) * 0.7f * opacity, 0f, origin, scale * 0.9f, SpriteEffects.None, 0f);

            // Layer 4: Pearl white-hot core
            sb.Draw(bloom, drawPos, null,
                (WhiteHot with { A = 0 }) * 0.85f * opacity, 0f, origin, scale * 0.4f, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Two-color bloom stack using {A=0} pattern.
        /// </summary>
        public static void DrawClairDeLuneBloomStack(SpriteBatch sb, Vector2 worldPos,
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
                // Behind layer: larger, softer, midnight blue
                sb.Draw(bloom, drawPos, null,
                    (MidnightBlue with { A = 0 }) * 0.25f * opacity, 0f, origin, scale * 2.5f, SpriteEffects.None, 0f);
                sb.Draw(bloom, drawPos, null,
                    (SoftBlue with { A = 0 }) * 0.35f * opacity, 0f, origin, scale * 1.6f, SpriteEffects.None, 0f);
            }
            else
            {
                // Front layer: smaller, brighter, pearl -> white
                sb.Draw(bloom, drawPos, null,
                    (PearlBlue with { A = 0 }) * 0.5f * opacity, 0f, origin, scale * 0.8f, SpriteEffects.None, 0f);
                sb.Draw(bloom, drawPos, null,
                    (Color.White with { A = 0 }) * 0.7f * opacity, 0f, origin, scale * 0.35f, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Counter-rotating double flare — two bloom textures spinning in opposite directions.
        /// Creates dreamy moonlit energy appearance at projectile centers.
        /// </summary>
        public static void DrawCounterRotatingFlares(SpriteBatch sb, Vector2 worldPos,
            float scale, float time, float opacity = 1f)
        {
            Texture2D flare = MagnumTextureRegistry.GetFlare();
            if (flare == null) return;

            Vector2 drawPos = worldPos - Main.screenPosition;
            Vector2 origin = flare.Size() * 0.5f;

            float rot1 = time * 2.0f;
            float rot2 = -time * 1.5f;

            sb.Draw(flare, drawPos, null,
                (PearlBlue with { A = 0 }) * 0.6f * opacity, rot1, origin, scale * 0.7f, SpriteEffects.None, 0f);
            sb.Draw(flare, drawPos, null,
                (SoftBlue with { A = 0 }) * 0.5f * opacity, rot2, origin, scale * 0.5f, SpriteEffects.None, 0f);
        }

        // ─────────── SELF-CONTAINED BLOOM (MANAGES OWN SPRITEBATCH) ───────────

        /// <summary>
        /// Standard Clair de Lune bloom at a blade tip or projectile centre.
        /// Uses the two-colour overload (MidnightBlue outer -> PearlBlue inner).
        /// Safe to call from PreDraw — handles SpriteBatch state internally.
        /// </summary>
        public static void DrawBloom(Vector2 worldPos, float scale, float opacity = 1f)
        {
            BloomRenderer.DrawBloomStackAdditive(worldPos, MidnightBlue, PearlBlue, scale, opacity);
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
        /// Draw Clair de Lune-themed multi-layer glow via GlowRenderer.
        /// Caller must have SpriteBatch in additive blend.
        /// </summary>
        public static void DrawClairDeLuneGlow(SpriteBatch sb, Vector2 worldPos,
            float scale = 1f, float intensity = 1f, string rotationId = null)
        {
            GlowRenderer.DrawGlow(sb, worldPos, ClairDeLuneGlowProfile, PearlBlue, intensity * scale, rotationId);
        }

        /// <summary>
        /// Draw Clair de Lune glow with automatic SpriteBatch state management.
        /// Safe to call from any context.
        /// </summary>
        public static void DrawClairDeLuneGlowManaged(SpriteBatch sb, Vector2 worldPos,
            float scale = 1f, float intensity = 1f, string rotationId = null)
        {
            GlowRenderer.DrawGlowManaged(sb, worldPos, ClairDeLuneGlowProfile, PearlBlue, intensity * scale, rotationId);
        }

        // ─────────── TRAIL WIDTH/COLOR FUNCTIONS ───────────
        // These return values compatible with CalamityStyleTrailRenderer.

        /// <summary>
        /// Standard Clair de Lune trail width: wide at head, tapers to tail.
        /// Dreamy, flowing, gentle taper for moonlit trails.
        /// </summary>
        public static float ClairDeLuneTrailWidth(float completionRatio, float baseWidth = 18f)
        {
            float tipFade = MathF.Pow(1f - completionRatio, 0.6f);
            float headFade = MathF.Pow(completionRatio, 3f);
            return baseWidth * tipFade * (1f - headFade);
        }

        /// <summary>
        /// Thin precision trail for clockwork weapons — surgical temporal cut.
        /// </summary>
        public static float PrecisionTrailWidth(float completionRatio, float baseWidth = 6f)
        {
            float taper = 1f - completionRatio;
            return baseWidth * taper * taper;
        }

        /// <summary>
        /// Thick dreamy trail for heavy weapons — cascading moonlit sweep.
        /// </summary>
        public static float DreamyTrailWidth(float completionRatio, float baseWidth = 16f)
        {
            float tipFade = MathF.Pow(1f - completionRatio, 0.4f);
            return baseWidth * tipFade;
        }

        /// <summary>
        /// Trail color function with {A=0} for additive rendering under AlphaBlend.
        /// Gradient from outer palette colour at edges to white-pushed center along trail.
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
        /// Pass 0: NightMist -> MidnightBlue, Pass 1: SoftBlue -> PearlBlue, Pass 2: PearlWhite -> WhiteHot
        /// </summary>
        public static (Vector3 primary, Vector3 secondary) GetShaderGradient(int passIndex)
        {
            return passIndex switch
            {
                0 => (NightMist.ToVector3(), MidnightBlue.ToVector3()),
                1 => (SoftBlue.ToVector3(), PearlBlue.ToVector3()),
                2 => (PearlWhite.ToVector3(), WhiteHot.ToVector3()),
                _ => (SoftBlue.ToVector3(), PearlBlue.ToVector3()),
            };
        }

        // ─────────── MUSIC NOTES ───────────

        /// <summary>
        /// Spawn visible, hue-shifting Clair de Lune music notes at the given position.
        /// Notes use the blue-pearl hue band (0.55-0.68) and are spawned
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
        /// Spawn orbiting music notes locked to a centre point (e.g. projectile).
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

        // ─────────── DUST HELPERS ───────────

        /// <summary>
        /// Dreamy blue dust trail at a blade tip during a swing.
        /// Uses ice and diamond dust for the signature moonlit shimmer effect.
        /// </summary>
        public static void SpawnSwingDust(Vector2 pos, Vector2 awayDirection, int dustType = DustID.IceTorch)
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
        /// Clair de Lune dual-color swing dust — alternating blue and white shimmer.
        /// </summary>
        public static void SpawnMoonlitSwingDust(Vector2 pos, Vector2 awayDirection)
        {
            for (int i = 0; i < 2; i++)
            {
                Vector2 vel = awayDirection * Main.rand.NextFloat(1f, 3f) + Main.rand.NextVector2Circular(0.5f, 0.5f);
                int dustType = Main.rand.NextBool() ? DustID.IceTorch : DustID.GemDiamond;
                Color col = dustType == DustID.IceTorch ? SoftBlue : PearlWhite;
                Dust d = Dust.NewDustPerfect(pos, dustType, vel, 0, col, 1.5f);
                d.noGravity = true;
                d.fadeIn = 1.3f;
            }
        }

        /// <summary>
        /// Radial dust burst for on-hit / impact VFX.
        /// Gradient from MidnightBlue to PearlBlue around the burst ring.
        /// </summary>
        public static void SpawnRadialDustBurst(Vector2 pos, int count = 12,
            float speed = 5f, int dustType = DustID.IceTorch)
        {
            for (int i = 0; i < count; i++)
            {
                float progress = (float)i / count;
                float angle = MathHelper.TwoPi * i / count;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(speed * 0.6f, speed);
                Color col = Color.Lerp(MidnightBlue, PearlBlue, progress);
                Dust d = Dust.NewDustPerfect(pos, dustType, vel, 0, col, 1.3f);
                d.noGravity = true;
            }
        }

        /// <summary>
        /// Pearl shimmer sparkle dust — call every other frame for dreamy trail.
        /// </summary>
        public static void SpawnPearlSparkle(Vector2 pos, Vector2 awayDirection)
        {
            if (!Main.rand.NextBool(2)) return;
            Dust d = Dust.NewDustPerfect(pos, DustID.GemDiamond,
                awayDirection * 0.5f + Main.rand.NextVector2Circular(1.5f, 1.5f), 0, PearlWhite, 1.0f);
            d.noGravity = true;
        }

        // ─────────── CLAIR DE LUNE SPECIFIC VFX: PEARL SHIMMER ───────────

        /// <summary>
        /// Spawn pearl shimmer particles around a position.
        /// The signature Clair de Lune ambient sparkle — soft, dreamy, luminous.
        /// </summary>
        public static void SpawnPearlShimmer(Vector2 pos, int count = 3, float radius = 40f, float scale = 0.25f)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 sparklePos = pos + angle.ToRotationVector2() * Main.rand.NextFloat(radius * 0.3f, radius);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -0.3f - Main.rand.NextFloat(0.5f));

                Color shimmerCol = ClairDeLunePalette.GetPearlGradient(Main.rand.NextFloat());
                var sparkle = new SparkleParticle(
                    sparklePos, vel,
                    shimmerCol, scale,
                    Main.rand.Next(25, 40)
                );
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
        }

        /// <summary>
        /// Spawn a burst of pearl sparkles radiating outward from a point.
        /// Use for impacts, detonations, and reveal moments.
        /// </summary>
        public static void SpawnPearlBurst(Vector2 pos, int count = 8, float speed = 4f, float scale = 0.3f)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(speed * 0.6f, speed);

                Color shimmerCol = ClairDeLunePalette.GetPearlGradient(Main.rand.NextFloat());
                var sparkle = new SparkleParticle(
                    pos, vel,
                    shimmerCol, scale * Main.rand.NextFloat(0.8f, 1.2f),
                    Main.rand.Next(20, 35)
                );
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
        }

        // ─────────── CLAIR DE LUNE SPECIFIC VFX: MOONLIT MIST ───────────

        /// <summary>
        /// Spawn moonlit mist particles drifting gently around a position.
        /// Creates the impressionistic haze that defines the theme.
        /// </summary>
        public static void SpawnMoonlitMist(Vector2 center, int count = 4, float radius = 50f, float opacity = 0.5f)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float dist = Main.rand.NextFloat(radius * 0.3f, radius);
                Vector2 particlePos = center + angle.ToRotationVector2() * dist;
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.8f, 0.8f), Main.rand.NextFloat(-0.5f, 0.2f));

                Color mistCol = ClairDeLunePalette.GetMoonlitGradient(Main.rand.NextFloat()) * opacity;
                var glow = new GenericGlowParticle(particlePos, vel,
                    mistCol, 0.3f + Main.rand.NextFloat(0.15f),
                    Main.rand.Next(30, 50), true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
        }

        /// <summary>
        /// Spawn inward-spiraling mist particles converging on a center (channeling effect).
        /// </summary>
        public static void SpawnConvergingMist(Vector2 center, int count = 6, float radius = 60f, float opacity = 0.55f)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float dist = radius + Main.rand.NextFloat(30f);
                Vector2 particlePos = center + angle.ToRotationVector2() * dist;
                Vector2 vel = (center - particlePos).SafeNormalize(Vector2.Zero) * 2.5f;

                var glow = new GenericGlowParticle(particlePos, vel,
                    GetClairDeLuneGradient(Main.rand.NextFloat()) * opacity,
                    0.28f, 22, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
        }

        // ─────────── CLAIR DE LUNE SPECIFIC VFX: STARLIT SPARKLE ───────────

        /// <summary>
        /// Spawn starlit sparkle particles — tiny twinkling points of light.
        /// Creates the starlit cloud effect above moonlit water.
        /// </summary>
        public static void SpawnStarlitSparkles(Vector2 pos, int count = 5, float radius = 35f, float scale = 0.2f)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 sparklePos = pos + Main.rand.NextVector2Circular(radius, radius);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -0.2f - Main.rand.NextFloat(0.3f));

                Color starCol = Color.Lerp(ClairDeLunePalette.StarlightSilver, PearlWhite, Main.rand.NextFloat());
                var sparkle = new SparkleParticle(
                    sparklePos, vel,
                    starCol, scale * Main.rand.NextFloat(0.8f, 1.3f),
                    Main.rand.Next(15, 30)
                );
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
        }

        // ─────────── GRADIENT HALO RINGS ───────────

        /// <summary>
        /// Cascading gradient halo rings (MidnightBlue -> PearlBlue).
        /// Dreamy concentric moonlit ripples.
        /// </summary>
        public static void SpawnGradientHaloRings(Vector2 pos, int count = 5, float baseScale = 0.3f)
        {
            for (int i = 0; i < count; i++)
            {
                float progress = (float)i / count;
                Color ringCol = Color.Lerp(MidnightBlue, PearlBlue, progress);
                CustomParticles.HaloRing(pos, ringCol, baseScale + i * 0.12f, 14);
            }
        }

        // ─────────── IMPACTS ───────────

        /// <summary>
        /// Full Clair de Lune melee impact VFX — bloom flash, halo cascade,
        /// radial dust burst, pearl shimmer, and music note scatter.
        /// Scales with combo step. Dreamy yet powerful for the supreme final boss tier.
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

            // Clair de Lune signature: pearl shimmer burst at impact
            SpawnPearlShimmer(pos, 2 + comboStep, 25f, 0.25f);

            // Starlit sparkle accent
            if (comboStep >= 1 || Main.rand.NextBool(3))
                SpawnStarlitSparkles(pos, 3, 20f, 0.2f);

            CustomParticles.GenericFlare(pos, PearlBlue, 0.4f + comboStep * 0.08f, 14);

            Lighting.AddLight(pos, SoftBlue.ToVector3() * (0.8f + comboStep * 0.15f));
        }

        /// <summary>
        /// Projectile death / on-kill VFX — bigger, flashier version of MeleeImpact.
        /// Includes pearl burst, converging mist, and enhanced bloom.
        /// </summary>
        public static void ProjectileImpact(Vector2 pos, float intensity = 1f)
        {
            DrawBloom(pos, 0.6f * intensity);
            SpawnGradientHaloRings(pos, 6, 0.3f * intensity);
            SpawnMusicNotes(pos, 6, 30f * intensity, 0.75f, 1.1f, 30);
            SpawnRadialDustBurst(pos, 15, 7f * intensity);
            SpawnPearlBurst(pos, 8, 5f * intensity, 0.3f);
            SpawnMoonlitMist(pos, 4, 40f * intensity, 0.5f);
            CustomParticles.GenericFlare(pos, PearlBlue, 0.5f * intensity, 16);
            Lighting.AddLight(pos, PearlBlue.ToVector3() * 1.2f * intensity);
        }

        // ─────────── SWING HELPERS ───────────

        /// <summary>
        /// Per-frame VFX to call from a swing projectile's AI().
        /// Handles dreamy dual-color dust trail, pearl sparkles, and periodic music notes.
        /// </summary>
        public static void SwingFrameVFX(Vector2 tipPos, Vector2 swordDirection, int comboStep,
            int timer, int dustType = DustID.IceTorch)
        {
            SpawnMoonlitSwingDust(tipPos, -swordDirection);
            SpawnPearlSparkle(tipPos, -swordDirection);

            if (timer % 5 == 0)
                SpawnMusicNotes(tipPos, 1, 10f, 0.7f, 0.9f, 25);

            // Clair de Lune signature: periodic pearl shimmer along swing arc
            if (timer % 7 == 0)
                SpawnPearlShimmer(tipPos, 1, 12f, 0.2f);

            Lighting.AddLight(tipPos, GetPaletteColor(0.4f + comboStep * 0.15f).ToVector3() * 0.6f);
        }

        // ─────────── FINISHER EFFECTS ───────────

        /// <summary>
        /// Phase-3 / finisher slam VFX — screen shake, massive bloom, moonlit cascade,
        /// pearl burst, star scatter, music note shower.
        /// The ultimate Clair de Lune impact for the supreme final boss tier.
        /// </summary>
        public static void FinisherSlam(Vector2 pos, float intensity = 1f)
        {
            MagnumScreenEffects.AddScreenShake(8f * intensity);
            DrawBloom(pos, 0.8f * intensity);
            SpawnGradientHaloRings(pos, 7, 0.35f * intensity);
            SpawnMusicNotes(pos, 6, 40f, 0.8f, 1.2f, 40);
            SpawnRadialDustBurst(pos, 20, 8f * intensity);
            SpawnPearlBurst(pos, 12, 6f * intensity, 0.35f);
            SpawnMoonlitMist(pos, 8, 70f * intensity, 0.6f);
            SpawnConvergingMist(pos, 8, 80f * intensity);
            SpawnStarlitSparkles(pos, 10, 50f, 0.25f);
            Lighting.AddLight(pos, WhiteHot.ToVector3() * 1.5f * intensity);
        }

        // ─────────── DYNAMIC LIGHTING ───────────

        /// <summary>
        /// Add standard Clair de Lune ambient light at a position.
        /// </summary>
        public static void AddClairDeLuneLight(Vector2 worldPos, float intensity = 0.6f)
        {
            Lighting.AddLight(worldPos, SoftBlue.ToVector3() * intensity);
        }

        /// <summary>
        /// Add palette-interpolated dynamic light. Higher t = brighter, more pearl.
        /// </summary>
        public static void AddPaletteLighting(Vector2 worldPos, float paletteT, float intensity = 0.8f)
        {
            Color col = GetPaletteColor(paletteT);
            Lighting.AddLight(worldPos, col.ToVector3() * intensity);
        }

        /// <summary>
        /// Add pulsing moonlit light with gentle color shift between soft blue and pearl.
        /// </summary>
        public static void AddPulsingLight(Vector2 worldPos, float time, float intensity = 0.6f)
        {
            float shift = (float)Math.Sin(time * 0.05f) * 0.5f + 0.5f;
            Color lightColor = Color.Lerp(SoftBlue, PearlBlue, shift * 0.4f);
            float pulse = (float)Math.Sin(time * 0.07f) * 0.15f + 0.85f;
            Lighting.AddLight(worldPos, lightColor.ToVector3() * pulse * intensity);
        }

        // ─────────── AMBIENT VFX (for accessories, idle effects) ───────────

        /// <summary>
        /// Ambient dreamy aura — call every few frames for gentle passive VFX.
        /// Spawns sparse mist and occasional pearl sparkle around the player/entity.
        /// </summary>
        public static void AmbientDreamyAura(Vector2 center, float time, float radius = 40f)
        {
            // Sparse mist every 8 frames
            if ((int)time % 8 == 0)
                SpawnMoonlitMist(center, 1, radius, 0.35f);

            // Pearl sparkle every 12 frames
            if ((int)time % 12 == 0)
                SpawnPearlShimmer(center, 1, radius * 0.7f, 0.18f);

            // Gentle ambient light
            AddPulsingLight(center, time, 0.4f);
        }

        /// <summary>
        /// Clockwork ambient aura — call for gear-themed accessories and weapons.
        /// Adds subtle brass-gold shimmer to the dreamy base effect.
        /// </summary>
        public static void AmbientClockworkAura(Vector2 center, float time, float radius = 35f)
        {
            AmbientDreamyAura(center, time, radius);

            // Additional clockwork sparkle every 10 frames
            if ((int)time % 10 == 0)
            {
                Vector2 sparklePos = center + Main.rand.NextVector2Circular(radius * 0.6f, radius * 0.6f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -0.2f);
                Color brassCol = ClairDeLunePalette.GetClockworkGradient(Main.rand.NextFloat(0.3f, 0.7f));
                var sparkle = new SparkleParticle(sparklePos, vel, brassCol, 0.2f, 20);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
        }

        // ─────────── THEME TEXTURE VFX ───────────
        // Uses ClairDeLuneThemeTextures for dreamy, clockwork-themed visuals.

        /// <summary>
        /// Draws a themed power ring using Clair de Lune Power Effect Ring texture.
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
                    (SoftBlue with { A = 0 }) * 0.5f * intensity, rotation, origin,
                    scale * 0.14f, SpriteEffects.None, 0f);
                sb.Draw(ring, drawPos, null,
                    (PearlWhite with { A = 0 }) * 0.35f * intensity, -rotation * 0.6f, origin,
                    scale * 0.10f, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Draws a themed radial slash star using Clair de Lune texture.
        /// Useful for projectile impacts and melee hit effects.
        /// </summary>
        public static void DrawThemeRadialStar(SpriteBatch sb, Vector2 worldPos,
            float scale, float intensity = 1f)
        {
            Texture2D star = ClairDeLuneThemeTextures.CLRadialSlashStar?.Value;
            if (star == null) return;

            Vector2 drawPos = worldPos - Main.screenPosition;
            Vector2 origin = star.Size() * 0.5f;
            float rot = (float)Main.GameUpdateCount * 0.03f;

            sb.Draw(star, drawPos, null,
                (PearlBlue with { A = 0 }) * 0.5f * intensity, rot, origin,
                scale * 0.08f, SpriteEffects.None, 0f);
            sb.Draw(star, drawPos, null,
                (WhiteHot with { A = 0 }) * 0.4f * intensity, -rot * 0.5f, origin,
                scale * 0.05f, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Draws a themed clockwork gear fragment accent.
        /// Perfect for melee hit impacts and clockwork-themed effects.
        /// </summary>
        public static void DrawThemeClockworkAccent(SpriteBatch sb, Vector2 worldPos,
            float scale, float intensity = 1f)
        {
            Texture2D gear = ClairDeLuneThemeTextures.CLClockGearFragment?.Value;
            if (gear == null) return;

            Vector2 drawPos = worldPos - Main.screenPosition;
            Vector2 origin = gear.Size() * 0.5f;
            float rot = (float)Main.GameUpdateCount * 0.04f;

            sb.Draw(gear, drawPos, null,
                (ClockworkBrass with { A = 0 }) * 0.4f * intensity, rot, origin,
                scale * 0.07f, SpriteEffects.None, 0f);
            sb.Draw(gear, drawPos, null,
                (MoonbeamGold with { A = 0 }) * 0.25f * intensity, -rot * 0.3f, origin,
                scale * 0.05f, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Draws a themed clock face shard — large, ethereal clockwork accent.
        /// Best used as a background layer during special attacks.
        /// </summary>
        public static void DrawThemeClockFaceShard(SpriteBatch sb, Vector2 worldPos,
            float scale, float intensity = 1f)
        {
            Texture2D shard = ClairDeLuneThemeTextures.CLClockFaceShard?.Value;
            if (shard == null) return;

            Vector2 drawPos = worldPos - Main.screenPosition;
            Vector2 origin = shard.Size() * 0.5f;
            float rot = (float)Main.GameUpdateCount * 0.01f;

            sb.Draw(shard, drawPos, null,
                (NightMist with { A = 0 }) * 0.3f * intensity, rot, origin,
                scale * 0.10f, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Combined theme impact: bloom + radial star + impact ring + clockwork accent.
        /// </summary>
        public static void DrawThemeImpactFull(SpriteBatch sb, Vector2 worldPos,
            float scale, float intensity = 1f)
        {
            DrawClairDeLuneBloomStack(sb, worldPos, scale, 0.3f, intensity);
            float rot = (float)Main.GameUpdateCount * 0.02f;
            DrawThemeRadialStar(sb, worldPos, scale, intensity * 0.6f);
            DrawThemeImpactRing(sb, worldPos, scale, intensity * 0.5f, rot);
            DrawThemeClockworkAccent(sb, worldPos, scale * 0.7f, intensity * 0.4f);
        }

        /// <summary>
        /// Draws a subtle layered theme accent (radial star + impact ring + clockwork gear)
        /// around a projectile. Manages SpriteBatch state internally — safe to call from
        /// PreDraw at any blend-state point.
        /// </summary>
        public static void DrawThemeAccents(SpriteBatch sb, Vector2 worldPos,
            float scale, float intensity = 0.5f)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);

            float rot = (float)Main.GameUpdateCount * 0.02f;
            DrawThemeRadialStar(sb, worldPos, scale, intensity * 0.6f);
            DrawThemeImpactRing(sb, worldPos, scale * 0.8f, intensity * 0.4f, rot);
            DrawThemeClockworkAccent(sb, worldPos, scale * 0.6f, intensity * 0.3f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);
        }
    }
}
