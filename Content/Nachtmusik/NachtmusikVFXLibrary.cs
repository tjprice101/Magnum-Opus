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
    /// Shared Nachtmusik VFX library  Ecanonical palette, bloom stacking,
    /// shader setup, trail helpers, music notes, dust, star spawns,
    /// constellation effects, and impact VFX used by ALL Nachtmusik weapons,
    /// accessories, projectiles, minions, enemies, and bosses.
    ///
    /// Nachtmusik identity: Mozart's serenade  Eplayful night music, starlit
    /// elegance, nocturnal charm, twinkling stars, constellation patterns,
    /// the Queen of Radiance's celestial grace.
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
        public static readonly Color Violet          = NachtmusikPalette.Violet;
        public static readonly Color CosmicPurple    = NachtmusikPalette.CosmicPurple;
        public static readonly Color NebulaPink      = NachtmusikPalette.NebulaPink;
        public static readonly Color Silver          = NachtmusikPalette.Silver;

        // Palette as array for indexed access
        private static readonly Color[] Palette = { MidnightBlue, DeepBlue, StarlitBlue, StarWhite, MoonlitSilver, TwinklingWhite };

        // Hue range for HueShiftingMusicNoteParticle (blue-indigo-violet spectrum)
        private const float HueMin = 0.55f;
        private const float HueMax = 0.75f;
        private const float NoteSaturation = 0.70f;
        private const float NoteLuminosity = 0.80f;

        // Nachtmusik glow profile for GlowRenderer
        public static readonly GlowRenderer.GlowLayer[] NachtmusikGlowProfile = new[]
        {
            new GlowRenderer.GlowLayer(1.0f, 1.0f, Color.White),
            new GlowRenderer.GlowLayer(1.6f, 0.65f, new Color(200, 210, 240)),   // StarWhite
            new GlowRenderer.GlowLayer(2.5f, 0.4f, new Color(80, 120, 200)),     // StarlitBlue
            new GlowRenderer.GlowLayer(4.0f, 0.2f, new Color(30, 50, 120))       // DeepBlue
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
        /// Drop-in replacement for NachtmusikCosmicVFX.GetCelestialGradient().
        /// MidnightBlue -> StarlitBlue -> TwinklingWhite over 0->1.
        /// </summary>
        public static Color GetCelestialGradient(float progress)
            => NachtmusikPalette.GetCelestialGradient(progress);

        /// <summary>
        /// Get a cycling nocturnal shimmer color.
        /// </summary>
        public static Color GetShimmer(float offset = 0f)
            => NachtmusikPalette.GetShimmer(Main.GameUpdateCount * 0.02f + offset);

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

        // ─────────── BLOOM STACKING (DIRECT {A=0} PATTERN) ───────────

        /// <summary>
        /// 4-layer bloom stack using {A=0} premultiplied alpha trick.
        /// Nocturnal gradient: DeepBlue outer -> StarlitBlue mid -> StarWhite inner -> TwinklingWhite core.
        /// </summary>
        public static void DrawNachtmusikBloomStack(SpriteBatch sb, Vector2 worldPos,
            float scale, float paletteT = 0.4f, float opacity = 1f)
        {
            Texture2D bloom = MagnumTextureRegistry.GetBloom();
            if (bloom == null) return;

            // 2160px bloom — cap so largest layer (scale*0.115) ≤ 0.139 → ≤300px
            scale = MathHelper.Min(scale, 1.209f);

            Vector2 drawPos = worldPos - Main.screenPosition;
            Vector2 origin = bloom.Size() * 0.5f;

            Color outer = GetPaletteColor(MathHelper.Clamp(paletteT - 0.2f, 0f, 1f));
            Color inner = GetPaletteColor(MathHelper.Clamp(paletteT + 0.2f, 0f, 1f));

            // Layer 1: Outer halo (DeepBlue)
            sb.Draw(bloom, drawPos, null,
                (outer with { A = 0 }) * 0.3f * opacity, 0f, origin, scale * 0.115f, SpriteEffects.None, 0f);

            // Layer 2: Mid glow (StarlitBlue)
            sb.Draw(bloom, drawPos, null,
                (outer with { A = 0 }) * 0.5f * opacity, 0f, origin, scale * 0.08f, SpriteEffects.None, 0f);

            // Layer 3: Inner bloom (StarWhite)
            sb.Draw(bloom, drawPos, null,
                (inner with { A = 0 }) * 0.7f * opacity, 0f, origin, scale * 0.052f, SpriteEffects.None, 0f);

            // Layer 4: White-hot core
            sb.Draw(bloom, drawPos, null,
                (TwinklingWhite with { A = 0 }) * 0.85f * opacity, 0f, origin, scale * 0.023f, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Two-color bloom stack using {A=0} pattern.
        /// </summary>
        public static void DrawNachtmusikBloomStack(SpriteBatch sb, Vector2 worldPos,
            Color outerColor, Color innerColor, float scale, float opacity = 1f)
        {
            Texture2D bloom = MagnumTextureRegistry.GetBloom();
            if (bloom == null) return;

            // 2160px bloom — cap so largest layer (scale*0.115) ≤ 0.139 → ≤300px
            scale = MathHelper.Min(scale, 1.209f);

            Vector2 drawPos = worldPos - Main.screenPosition;
            Vector2 origin = bloom.Size() * 0.5f;

            sb.Draw(bloom, drawPos, null,
                (outerColor with { A = 0 }) * 0.3f * opacity, 0f, origin, scale * 0.115f, SpriteEffects.None, 0f);
            sb.Draw(bloom, drawPos, null,
                (outerColor with { A = 0 }) * 0.5f * opacity, 0f, origin, scale * 0.08f, SpriteEffects.None, 0f);
            sb.Draw(bloom, drawPos, null,
                (innerColor with { A = 0 }) * 0.7f * opacity, 0f, origin, scale * 0.052f, SpriteEffects.None, 0f);
            sb.Draw(bloom, drawPos, null,
                (Color.White with { A = 0 }) * 0.85f * opacity, 0f, origin, scale * 0.023f, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Counter-rotating constellation flares  Etwin starlit points spinning in opposite directions.
        /// Creates the signature nocturnal twinkling at projectile centers.
        /// </summary>
        public static void DrawCounterRotatingFlares(SpriteBatch sb, Vector2 worldPos,
            float scale, float time, float opacity = 1f)
        {
            Texture2D flare = MagnumTextureRegistry.GetFlare();
            if (flare == null) return;

            // 1024px flare — cap so largest layer (scale*0.7) ≤ 0.293 → ≤300px
            scale = MathHelper.Min(scale, 0.419f);

            Vector2 drawPos = worldPos - Main.screenPosition;
            Vector2 origin = flare.Size() * 0.5f;

            float rot1 = time * 2.5f;
            float rot2 = -time * 1.8f;

            sb.Draw(flare, drawPos, null,
                (StarWhite with { A = 0 }) * 0.6f * opacity, rot1, origin, scale * 0.7f, SpriteEffects.None, 0f);
            sb.Draw(flare, drawPos, null,
                (StarlitBlue with { A = 0 }) * 0.5f * opacity, rot2, origin, scale * 0.5f, SpriteEffects.None, 0f);
        }

        // ─────────── SELF-CONTAINED BLOOM (MANAGES OWN SPRITEBATCH) ───────────

        /// <summary>
        /// Standard Nachtmusik bloom at a blade tip or projectile centre.
        /// Nocturnal gradient: DeepBlue outer -> StarWhite inner.
        /// Safe to call from PreDraw  Ehandles SpriteBatch state internally.
        /// </summary>
        public static void DrawBloom(Vector2 worldPos, float scale, float opacity = 1f)
        {
            BloomRenderer.DrawBloomStackAdditive(worldPos, DeepBlue, StarWhite, scale, opacity);
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
        /// Draw Nachtmusik-themed multi-layer glow via GlowRenderer.
        /// </summary>
        public static void DrawNachtmusikGlow(SpriteBatch sb, Vector2 worldPos,
            float scale = 1f, float intensity = 1f, string rotationId = null)
        {
            GlowRenderer.DrawGlow(sb, worldPos, NachtmusikGlowProfile, StarWhite, intensity * scale, rotationId);
        }

        /// <summary>
        /// Draw Nachtmusik glow with automatic SpriteBatch state management.
        /// </summary>
        public static void DrawNachtmusikGlowManaged(SpriteBatch sb, Vector2 worldPos,
            float scale = 1f, float intensity = 1f, string rotationId = null)
        {
            GlowRenderer.DrawGlowManaged(sb, worldPos, NachtmusikGlowProfile, StarWhite, intensity * scale, rotationId);
        }

        // ─────────── TRAIL WIDTH/COLOR FUNCTIONS ───────────

        /// <summary>
        /// Standard Nachtmusik trail width: elegant, starlit taper.
        /// </summary>
        public static float NachtmusikTrailWidth(float completionRatio, float baseWidth = 18f)
        {
            float tipFade = MathF.Pow(1f - completionRatio, 0.6f);
            float headFade = MathF.Pow(completionRatio, 3f);
            return baseWidth * tipFade * (1f - headFade);
        }

        /// <summary>
        /// Thin precision trail for ranged and constellation weapons.
        /// </summary>
        public static float PrecisionTrailWidth(float completionRatio, float baseWidth = 6f)
        {
            float taper = 1f - completionRatio;
            return baseWidth * taper * taper;
        }

        /// <summary>
        /// Wide sweeping trail for melee weapons  Enocturnal arc.
        /// </summary>
        public static float NocturnalTrailWidth(float completionRatio, float baseWidth = 16f)
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
        /// Notes cycle through the blue-indigo-violet spectrum for nocturnal feel.
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
                    scale, lifetime, hueSpeed: 0.025f
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
                    scale, 30, hueSpeed: 0.025f
                );
                MagnumParticleHandler.SpawnParticle(note);
            }
        }

        // ─────────── DUST HELPERS ───────────

        /// <summary>
        /// Starlit dust trail at a blade tip during a swing.
        /// Blue-silver nocturnal sparkle dust.
        /// </summary>
        public static void SpawnSwingDust(Vector2 pos, Vector2 awayDirection, int dustType = DustID.BlueTorch)
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
        /// Nachtmusik starlit swing dust  Ealternating blue torch and starlit shimmer.
        /// </summary>
        public static void SpawnStarlitDust(Vector2 pos, Vector2 awayDirection)
        {
            for (int i = 0; i < 2; i++)
            {
                Vector2 vel = awayDirection * Main.rand.NextFloat(1f, 3f) + Main.rand.NextVector2Circular(0.5f, 0.5f);
                bool isBright = Main.rand.NextBool();
                int dustType = isBright ? DustID.BlueTorch : DustID.SparksMech;
                Color col = isBright ? StarWhite : StarlitBlue;
                Dust d = Dust.NewDustPerfect(pos, dustType, vel, 0, col, 1.5f);
                d.noGravity = true;
                d.fadeIn = 1.3f;
            }
        }

        /// <summary>
        /// Radial dust burst for on-hit / impact VFX.
        /// Nocturnal: blue-silver starlit burst ring.
        /// </summary>
        public static void SpawnRadialDustBurst(Vector2 pos, int count = 12, float speed = 5f)
        {
            for (int i = 0; i < count; i++)
            {
                float progress = (float)i / count;
                float angle = MathHelper.TwoPi * i / count;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(speed * 0.6f, speed);
                Color col = GetCelestialGradient(progress);
                Dust d = Dust.NewDustPerfect(pos, DustID.BlueTorch, vel, 0, col, 1.3f);
                d.noGravity = true;
            }
        }

        /// <summary>
        /// Golden radiance shimmer dust  Ethe Queen's golden accent.
        /// </summary>
        public static void SpawnRadianceShimmer(Vector2 pos, Vector2 awayDirection)
        {
            if (!Main.rand.NextBool(2)) return;
            Dust d = Dust.NewDustPerfect(pos, DustID.GoldCoin,
                awayDirection * 0.5f + Main.rand.NextVector2Circular(1.5f, 1.5f), 0, RadianceGold, 1.2f);
            d.noGravity = true;
        }

        /// <summary>
        /// Radiant gold dust burst  Ethe Queen's radiance explosion.
        /// </summary>
        public static void SpawnRadianceBurst(Vector2 pos, int count = 10, float speed = 5f)
        {
            for (int i = 0; i < count; i++)
            {
                float progress = (float)i / count;
                float angle = MathHelper.TwoPi * i / count;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(speed * 0.6f, speed);
                Color col = Color.Lerp(StarGold, RadianceGold, progress);
                Dust d = Dust.NewDustPerfect(pos, DustID.GoldCoin, vel, 0, col, 1.4f);
                d.noGravity = true;
            }
        }

        // ─────────── NACHTMUSIK-SPECIFIC VFX: TWINKLING STARS ───────────

        /// <summary>
        /// Spawn twinkling star particles around a position.
        /// The signature Nachtmusik visual identity  Eplayful starlit sparkles.
        /// </summary>
        public static void SpawnTwinklingStars(Vector2 pos, int count = 3, float radius = 30f)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(radius, radius);
                Color starCol = GetCelestialGradient(Main.rand.NextFloat());
                float starScale = Main.rand.NextFloat(0.2f, 0.45f);
                try { CustomParticles.GenericFlare(pos + offset, starCol, starScale, Main.rand.Next(12, 22)); } catch { }
            }
        }

        /// <summary>
        /// Spawn a burst of stars exploding outward from impact point.
        /// </summary>
        public static void SpawnStarBurst(Vector2 pos, int count = 8, float scale = 0.4f)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                Color starColor = GetCelestialGradient((float)i / count);
                var spark = new GlowSparkParticle(pos, vel, starColor, scale * Main.rand.NextFloat(0.8f, 1.2f), 18);
                MagnumParticleHandler.SpawnParticle(spark);
            }
        }

        /// <summary>
        /// Spawn constellation line particles connecting two points.
        /// Creates the signature starlit constellation web effect.
        /// </summary>
        public static void SpawnConstellationLine(Vector2 start, Vector2 end, int segments = 4)
        {
            for (int i = 0; i <= segments; i++)
            {
                float t = (float)i / segments;
                Vector2 linePos = Vector2.Lerp(start, end, t);
                Color lineCol = Color.Lerp(StarlitBlue, StarWhite, t) * 0.5f;
                var glow = new GenericGlowParticle(linePos, Vector2.Zero, lineCol, 0.08f, 12, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            // Star points at endpoints
            try { CustomParticles.GenericFlare(start, StarWhite, 0.3f, 14); } catch { }
            try { CustomParticles.GenericFlare(end, StarWhite, 0.3f, 14); } catch { }
        }

        /// <summary>
        /// Spawn a constellation circle pattern  Econnected star points in a ring.
        /// </summary>
        public static void SpawnConstellationCircle(Vector2 center, float radius, int stars, float rotation)
        {
            for (int i = 0; i < stars; i++)
            {
                float angle = rotation + MathHelper.TwoPi * i / stars;
                Vector2 starPos = center + angle.ToRotationVector2() * radius;
                try { CustomParticles.GenericFlare(starPos, StarWhite, 0.35f, 12); } catch { }

                // Constellation line to next star
                if (i < stars - 1)
                {
                    float nextAngle = rotation + MathHelper.TwoPi * (i + 1) / stars;
                    Vector2 nextPos = center + nextAngle.ToRotationVector2() * radius;
                    for (int j = 1; j < 4; j++)
                    {
                        Vector2 linePos = Vector2.Lerp(starPos, nextPos, j / 4f);
                        var linePart = new GenericGlowParticle(linePos, Vector2.Zero,
                            StarlitBlue * 0.4f, 0.08f, 10, true);
                        MagnumParticleHandler.SpawnParticle(linePart);
                    }
                }
            }
        }

        // ─────────── NACHTMUSIK-SPECIFIC VFX: STARBURST ───────────

        /// <summary>
        /// Spawn cascading starburst layers  Ethe signature celestial impact.
        /// </summary>
        public static void SpawnStarburstCascade(Vector2 pos, int layers = 5, float scale = 1f, float opacity = 1f)
        {
            for (int i = 0; i < layers; i++)
            {
                float progress = (float)i / layers;
                Color burstColor = GetCelestialGradient(progress);
                float burstScale = (0.4f + i * 0.08f) * scale;
                var starburst = new StarBurstParticle(pos, Vector2.Zero, burstColor, burstScale, 16 + i * 3, i % 2);
                MagnumParticleHandler.SpawnParticle(starburst);

                // Sparkle accent
                Vector2 offset = Main.rand.NextVector2Circular(12f * progress, 12f * progress);
                try { CustomParticles.GenericFlare(pos + offset, burstColor, 0.25f * scale, 14 + i * 2); } catch { }
            }
        }

        /// <summary>
        /// Spawn shattered starlight fragments scattering outward.
        /// Crystal-like nocturnal shards for weapon hit effects and projectile deaths.
        /// </summary>
        public static void SpawnShatteredStarlight(Vector2 pos, int count = 6, float speed = 5f, float scale = 1f, bool hasGravity = true)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.3f, 0.3f);
                float fragmentSpeed = speed * Main.rand.NextFloat(0.7f, 1.3f);
                Vector2 vel = angle.ToRotationVector2() * fragmentSpeed;
                Color fragmentColor = GetCelestialGradient(Main.rand.NextFloat());
                float fragmentScale = scale * Main.rand.NextFloat(0.3f, 0.5f);
                var fragment = new ShatteredStarlightParticle(pos, vel, fragmentColor, fragmentScale,
                    Main.rand.Next(20, 35), hasGravity, hasGravity ? 0.12f : 0f);
                MagnumParticleHandler.SpawnParticle(fragment);
            }
            try { CustomParticles.GenericFlare(pos, StarWhite * 0.8f, 0.5f * scale, 12); } catch { }
        }

        // ─────────── GRADIENT HALO RINGS ───────────

        /// <summary>
        /// Cascading gradient halo rings  Enocturnal (DeepBlue -> StarWhite).
        /// </summary>
        public static void SpawnGradientHaloRings(Vector2 pos, int count = 5, float baseScale = 0.3f)
        {
            for (int i = 0; i < count; i++)
            {
                float progress = (float)i / count;
                Color ringCol = GetCelestialGradient(progress);
                try { CustomParticles.HaloRing(pos, ringCol, baseScale + i * 0.12f, 14); } catch { }
            }
        }

        /// <summary>
        /// Celestial halo rings  Egolden radiance ring cascade.
        /// </summary>
        public static void SpawnRadianceHaloRings(Vector2 pos, int count = 5, float baseScale = 0.3f)
        {
            for (int i = 0; i < count; i++)
            {
                float progress = (float)i / count;
                Color ringCol = Color.Lerp(StarlitBlue, RadianceGold, progress);
                try { CustomParticles.HaloRing(pos, ringCol, baseScale + i * 0.12f, 14); } catch { }
            }
        }

        // ─────────── GLYPH HELPERS ───────────

        /// <summary>
        /// Spawn orbiting celestial glyphs around a position.
        /// </summary>
        public static void SpawnOrbitingGlyphs(Vector2 center, int count = 4, float radius = 40f, float baseAngle = 0f)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = baseAngle + MathHelper.TwoPi * i / count;
                Vector2 glyphPos = center + angle.ToRotationVector2() * radius;
                Color glyphColor = GetCelestialGradient((float)i / count);
                try { CustomParticles.Glyph(glyphPos, glyphColor, 0.4f, -1); } catch { }
            }
        }

        /// <summary>
        /// Spawn a burst of celestial glyphs outward from a position.
        /// </summary>
        public static void SpawnGlyphBurst(Vector2 pos, int count = 4, float speed = 5f, float scale = 0.4f)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 vel = angle.ToRotationVector2() * speed * Main.rand.NextFloat(0.8f, 1.2f);
                Color glyphColor = Main.rand.NextBool() ? StarlitBlue : RadianceGold;
                try { CustomParticles.Glyph(pos + vel * 0.3f, glyphColor, scale, -1); } catch { }
            }
        }


        // ─────────── COLOR-RAMPED SPARKLE EXPLOSIONS ───────────

        /// <summary>
        /// Spawn a starburst of color-ramped GlowSparkParticles using Nachtmusik's
        /// deep-blue → starlit-blue → star-white → moonlit-silver gradient.
        /// </summary>
        public static void SpawnStarlitSparkleExplosion(Vector2 pos, int count = 10, float speed = 6f, float scale = 0.35f)
        {
            try
            {
                for (int i = 0; i < count; i++)
                {
                    float progress = (float)i / count;
                    Color sparkColor;
                    if (progress < 0.33f)
                        sparkColor = Color.Lerp(DeepBlue, StarlitBlue, progress / 0.33f);
                    else if (progress < 0.66f)
                        sparkColor = Color.Lerp(StarlitBlue, StarWhite, (progress - 0.33f) / 0.33f);
                    else
                        sparkColor = Color.Lerp(StarWhite, MoonlitSilver, (progress - 0.66f) / 0.34f);

                    float angle = MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.2f, 0.2f);
                    Vector2 vel = angle.ToRotationVector2() * speed * Main.rand.NextFloat(0.7f, 1.3f);
                    float sparkScale = scale * Main.rand.NextFloat(0.8f, 1.3f);

                    var spark = new GlowSparkParticle(pos, vel, sparkColor with { A = 0 }, sparkScale, 18);
                    MagnumParticleHandler.SpawnParticle(spark);
                }
            }
            catch
            {
                // Fallback: vanilla dust burst
                for (int i = 0; i < count; i++)
                {
                    float angle = MathHelper.TwoPi * i / count;
                    Vector2 vel = angle.ToRotationVector2() * speed * 0.5f;
                    Dust.NewDustPerfect(pos, DustID.BlueTorch, vel, 0, default, 1.2f);
                }
            }
        }

        /// <summary>
        /// Multi-layered constellation starburst — 3 concentric rings of sparks:
        /// inner white-hot core, mid starlit-blue radial burst, outer deep-blue halo.
        /// </summary>
        public static void SpawnConstellationStarburst(Vector2 pos, float intensityMul = 1f)
        {
            try
            {
                // Inner ring — twinkling white core sparks
                int innerCount = (int)(6 * intensityMul);
                for (int i = 0; i < innerCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / innerCount + Main.rand.NextFloat(-0.3f, 0.3f);
                    Vector2 vel = angle.ToRotationVector2() * 3f * Main.rand.NextFloat(0.8f, 1.2f);
                    var spark = new GlowSparkParticle(pos, vel, TwinklingWhite with { A = 0 }, 0.25f * intensityMul, 14);
                    MagnumParticleHandler.SpawnParticle(spark);
                }

                // Mid ring — starlit blue radial sparks
                int midCount = (int)(10 * intensityMul);
                for (int i = 0; i < midCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / midCount + Main.rand.NextFloat(-0.15f, 0.15f);
                    Vector2 vel = angle.ToRotationVector2() * 6f * Main.rand.NextFloat(0.7f, 1.3f);
                    Color midColor = Color.Lerp(StarlitBlue, StarWhite, Main.rand.NextFloat(0.3f)) with { A = 0 };
                    var spark = new GlowSparkParticle(pos, vel, midColor, 0.32f * intensityMul, 20);
                    MagnumParticleHandler.SpawnParticle(spark);
                }

                // Outer ring — deep blue constellation halo
                int outerCount = (int)(8 * intensityMul);
                for (int i = 0; i < outerCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / outerCount + Main.rand.NextFloat(-0.25f, 0.25f);
                    Vector2 vel = angle.ToRotationVector2() * 9f * Main.rand.NextFloat(0.6f, 1.4f);
                    Color outerColor = Color.Lerp(DeepBlue, StarlitBlue, Main.rand.NextFloat()) with { A = 0 };
                    var spark = new GlowSparkParticle(pos, vel, outerColor, 0.4f * intensityMul, 24);
                    MagnumParticleHandler.SpawnParticle(spark);
                }

                // Gold accent sparks — radiance gold highlights scattered among the burst
                int goldCount = (int)(4 * intensityMul);
                for (int i = 0; i < goldCount; i++)
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 vel = angle.ToRotationVector2() * 5f * Main.rand.NextFloat(0.8f, 1.2f);
                    var spark = new GlowSparkParticle(pos, vel, RadianceGold with { A = 0 }, 0.28f * intensityMul, 16);
                    MagnumParticleHandler.SpawnParticle(spark);
                }
            }
            catch
            {
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8;
                    Dust.NewDustPerfect(pos, DustID.BlueTorch, angle.ToRotationVector2() * 4f, 0, default, 1.5f);
                }
            }
        }
        // ─────────── IMPACTS ───────────

        /// <summary>
        /// Full Nachtmusik melee impact VFX  Estarlit bloom flash, halo cascade,
        /// nocturnal dust burst, twinkling stars, constellation sparks, and music note burst.
        /// Scales with combo step.
        /// </summary>
        public static void MeleeImpact(Vector2 pos, int comboStep = 0)
        {
            float bloomScale = 0.5f + comboStep * 0.1f;
            DrawBloom(pos, bloomScale);

            // Color-ramped starlit sparkle explosion
            SpawnStarlitSparkleExplosion(pos, 8 + comboStep * 2, 5f + comboStep, 0.3f);

            // Constellation starburst on higher combo steps
            if (comboStep >= 2)
                SpawnConstellationStarburst(pos, 0.6f + comboStep * 0.15f);

            int rings = 3 + comboStep;
            SpawnGradientHaloRings(pos, rings);

            int dustCount = 8 + comboStep * 4;
            SpawnRadialDustBurst(pos, dustCount, 5f + comboStep);

            int noteCount = 2 + comboStep;
            SpawnMusicNotes(pos, noteCount, 25f);

            // Nachtmusik signature: twinkling star scatter at impact
            SpawnTwinklingStars(pos, 2 + comboStep, 20f);

            // Star burst sparks
            SpawnStarBurst(pos, 4 + comboStep * 2, 0.3f);

            // Starlit halo ring
            try { CustomParticles.HaloRing(pos, StarWhite, 0.5f, 18); } catch { }
            try { CustomParticles.HaloRing(pos, DeepBlue, 0.35f, 15); } catch { }

            Lighting.AddLight(pos, StarWhite.ToVector3() * (0.8f + comboStep * 0.15f));
        }

        /// <summary>
        /// Projectile death / on-kill VFX  Ebigger, flashier version of MeleeImpact.
        /// Includes star burst, constellation lines, and enhanced bloom.
        /// </summary>
        public static void ProjectileImpact(Vector2 pos, float intensity = 1f)
        {
            DrawBloom(pos, 0.6f * intensity);

            // Color-ramped starlit sparkle explosion
            SpawnStarlitSparkleExplosion(pos, 12, 7f * intensity, 0.35f);
            SpawnConstellationStarburst(pos, intensity);

            SpawnGradientHaloRings(pos, 6, 0.3f * intensity);
            SpawnMusicNotes(pos, 6, 30f * intensity, 0.75f, 1.1f, 30);
            SpawnRadialDustBurst(pos, 15, 7f * intensity);
            SpawnStarBurst(pos, (int)(8 * intensity), 0.4f);
            SpawnStarburstCascade(pos, 3, intensity);
            SpawnShatteredStarlight(pos, (int)(8 * intensity), 6f * intensity, intensity);
            SpawnTwinklingStars(pos, (int)(6 * intensity), 25f);
            try { CustomParticles.GenericFlare(pos, TwinklingWhite, 0.5f * intensity, 16); } catch { }
            Lighting.AddLight(pos, StarWhite.ToVector3() * 1.2f * intensity);
        }

        // ─────────── SWING HELPERS ───────────

        /// <summary>
        /// Per-frame VFX to call from a swing projectile's AI().
        /// Handles starlit dust trail, radiance shimmer, periodic twinkling stars and music notes.
        /// </summary>
        public static void SwingFrameVFX(Vector2 tipPos, Vector2 swordDirection, int comboStep,
            int timer, int dustType = DustID.BlueTorch)
        {
            SpawnStarlitDust(tipPos, -swordDirection);
            SpawnRadianceShimmer(tipPos, -swordDirection);

            if (timer % 5 == 0)
                SpawnMusicNotes(tipPos, 1, 10f, 0.7f, 0.9f, 25);

            // Nachtmusik signature: periodic twinkling along swing arc
            if (timer % 8 == 0)
                SpawnTwinklingStars(tipPos, 1, 10f);

            Lighting.AddLight(tipPos, GetPaletteColor(0.4f + comboStep * 0.15f).ToVector3() * 0.6f);
        }

        // ─────────── FINISHER EFFECTS ───────────

        /// <summary>
        /// Phase-3 / finisher slam VFX  Escreen shake, massive bloom, constellation cascade,
        /// star explosion, radiance burst, music note scatter.
        /// </summary>
        public static void FinisherSlam(Vector2 pos, float intensity = 1f)
        {
            MagnumScreenEffects.AddScreenShake(8f * intensity);
            DrawBloom(pos, 0.8f * intensity);

            // Grand constellation starburst at 1.5x intensity
            SpawnConstellationStarburst(pos, intensity * 1.5f);
            SpawnStarlitSparkleExplosion(pos, 16, 8f * intensity, 0.4f);

            SpawnGradientHaloRings(pos, 7, 0.35f * intensity);
            SpawnRadianceHaloRings(pos, 5, 0.3f * intensity);
            SpawnMusicNotes(pos, 6, 40f, 0.8f, 1.2f, 40);
            SpawnRadialDustBurst(pos, 20, 8f * intensity);
            SpawnRadianceBurst(pos, 16, 7f * intensity);
            SpawnStarBurst(pos, (int)(12 * intensity), 0.45f);
            SpawnStarburstCascade(pos, 5, intensity * 1.2f);
            SpawnShatteredStarlight(pos, (int)(12 * intensity), 8f * intensity, intensity, true);
            SpawnTwinklingStars(pos, (int)(10 * intensity), 40f);
            SpawnConstellationCircle(pos, 60f * intensity, 8, Main.GameUpdateCount * 0.03f);
            SpawnGlyphBurst(pos, 6, 5f * intensity, 0.4f);
            Lighting.AddLight(pos, TwinklingWhite.ToVector3() * 1.5f * intensity);
        }

        // ─────────── CLOUD TRAIL ───────────

        /// <summary>
        /// Spawn a nocturnal cloud trail behind projectiles.
        /// Layered cosmic mist with starlit sparkle accents.
        /// </summary>
        public static void SpawnCloudTrail(Vector2 pos, Vector2 velocity, float scale = 1f)
        {
            for (int layer = 0; layer < 3; layer++)
            {
                float layerProgress = layer / 3f;
                Color cloudColor = Color.Lerp(MidnightBlue, StarlitBlue, layerProgress);
                float cloudScale = (0.35f + layer * 0.1f) * scale;

                Vector2 offset = Main.rand.NextVector2Circular(6f, 6f);
                Vector2 cloudVel = -velocity * (0.05f + layer * 0.02f) + Main.rand.NextVector2Circular(0.8f, 0.8f);
                var cloud = new GenericGlowParticle(pos + offset, cloudVel, cloudColor * 0.55f, cloudScale, 22, true);
                MagnumParticleHandler.SpawnParticle(cloud);
            }

            // Twinkling star sparkle in cloud
            if (Main.rand.NextBool(4))
            {
                try { CustomParticles.GenericFlare(pos + Main.rand.NextVector2Circular(10f, 10f), StarWhite, 0.2f * scale, 10); } catch { }
            }
        }

        /// <summary>
        /// Spawn a radiant beam trail for ranged weapons.
        /// Core trail with outer starlit glow and optional star sparkle.
        /// </summary>
        public static void SpawnRadiantBeamTrail(Vector2 pos, Vector2 velocity, float scale = 1f)
        {
            var core = new GenericGlowParticle(pos, -velocity * 0.05f, StarWhite * 0.9f, 0.25f * scale, 15, true);
            MagnumParticleHandler.SpawnParticle(core);

            var outer = new GenericGlowParticle(pos, -velocity * 0.03f, StarlitBlue * 0.6f, 0.35f * scale, 18, true);
            MagnumParticleHandler.SpawnParticle(outer);

            if (Main.rand.NextBool(3))
            {
                Vector2 sparkleOffset = Main.rand.NextVector2Circular(8f, 8f);
                var sparkle = new GenericGlowParticle(pos + sparkleOffset, Main.rand.NextVector2Circular(1f, 1f),
                    Violet, 0.18f * scale, 12, true);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
        }

        // ─────────── DYNAMIC LIGHTING ───────────

        /// <summary>
        /// Add standard Nachtmusik ambient light at a position.
        /// </summary>
        public static void AddNachtmusikLight(Vector2 worldPos, float intensity = 0.6f)
        {
            Lighting.AddLight(worldPos, StarWhite.ToVector3() * intensity);
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
        /// Add pulsing starlit light  Ethe signature nocturnal twinkle.
        /// </summary>
        public static void AddTwinklingLight(Vector2 worldPos, float time, float intensity = 0.6f)
        {
            float twinkle = (float)Math.Sin(time * 0.12f) * 0.25f + 0.75f;
            Color lightColor = Color.Lerp(StarlitBlue, StarWhite, twinkle);
            Lighting.AddLight(worldPos, lightColor.ToVector3() * twinkle * intensity);
        }

        /// <summary>
        /// Add golden radiance light  Ethe Queen's warm celestial glow.
        /// </summary>
        public static void AddRadianceLight(Vector2 worldPos, float time, float intensity = 0.6f)
        {
            float pulse = (float)Math.Sin(time * 0.06f) * 0.15f + 0.85f;
            Color lightColor = Color.Lerp(StarGold, RadianceGold, pulse);
            Lighting.AddLight(worldPos, lightColor.ToVector3() * pulse * intensity);
        }

        // ─────────── MINION AURA ───────────

        /// <summary>
        /// Spawn ambient minion aura particles  Estarlit shimmer around summons.
        /// </summary>
        public static void SpawnMinionAura(Vector2 pos, float scale = 1f)
        {
            if (Main.rand.NextBool(4))
            {
                Vector2 offset = Main.rand.NextVector2Circular(20f * scale, 20f * scale);
                Color auraColor = GetCelestialGradient(Main.rand.NextFloat());
                var aura = new GenericGlowParticle(pos + offset, Main.rand.NextVector2Circular(0.5f, 0.5f),
                    auraColor * 0.5f, 0.2f * scale, 20, true);
                MagnumParticleHandler.SpawnParticle(aura);
            }

            // Star dust motes rising  Ethe playful twinkling
            if (Main.rand.NextBool(6))
            {
                Vector2 motePos = pos + new Vector2(Main.rand.NextFloat(-15f, 15f) * scale, 10f * scale);
                var mote = new GenericGlowParticle(motePos, new Vector2(0, -0.8f),
                    StarWhite * 0.6f, 0.12f * scale, 25, true);
                MagnumParticleHandler.SpawnParticle(mote);
            }
        }

        // ─────────── BOSS-SPECIFIC EFFECTS ───────────

        /// <summary>
        /// Boss phase transition  Emassive celestial explosion with constellation circle.
        /// </summary>
        public static void SpawnBossPhaseTransition(Vector2 pos, float scale = 1.5f)
        {
            // Massive star bursts
            for (int layer = 0; layer < 5; layer++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(10f, 10f);
                Color layerColor = GetCelestialGradient((float)layer / 5f);
                var burst = new StarBurstParticle(pos + offset, Vector2.Zero, layerColor,
                    (0.8f - layer * 0.1f) * scale, 25 + layer * 5);
                MagnumParticleHandler.SpawnParticle(burst);
            }

            SpawnShatteredStarlight(pos, 24, 15f * scale, scale * 1.2f, true);
            SpawnConstellationCircle(pos, 80f * scale, 12, Main.GameUpdateCount * 0.03f);
            SpawnOrbitingGlyphs(pos, 8, 60f * scale, Main.GameUpdateCount * 0.02f);
            SpawnStarburstCascade(pos, 8, scale);
            SpawnMusicNotes(pos, 12, 60f, 0.8f, 1.2f, 40);
            SpawnRadianceBurst(pos, 20, 10f * scale);
            MagnumScreenEffects.AddScreenShake(12f * scale);
            Lighting.AddLight(pos, TwinklingWhite.ToVector3() * 3f * scale);
        }

        /// <summary>
        /// Boss warning circle with star points  Etelegraph effect.
        /// </summary>
        public static void SpawnBossWarningCircle(Vector2 center, float radius, float progress)
        {
            int starCount = 8;
            float rotation = Main.GameUpdateCount * 0.02f;

            for (int i = 0; i < starCount; i++)
            {
                float angle = rotation + MathHelper.TwoPi * i / starCount;
                Vector2 starPos = center + angle.ToRotationVector2() * radius;
                Color starColor = Color.Lerp(StarlitBlue, RadianceGold, progress);
                var burst = new StarBurstParticle(starPos, Vector2.Zero, starColor, 0.25f + progress * 0.15f, 8);
                MagnumParticleHandler.SpawnParticle(burst);
            }

            if (progress > 0.3f)
            {
                for (int j = 0; j < starCount; j++)
                {
                    float connectAngle = rotation + MathHelper.TwoPi * j / starCount;
                    float nextAngle = rotation + MathHelper.TwoPi * ((j + 1) % starCount) / starCount;
                    Vector2 midPoint = center + ((connectAngle.ToRotationVector2() + nextAngle.ToRotationVector2()) * 0.5f).SafeNormalize(Vector2.Zero) * radius * 0.8f;
                    var connector = new GenericGlowParticle(midPoint, Vector2.Zero, StarlitBlue * (progress * 0.6f), 0.15f, 8, true);
                    MagnumParticleHandler.SpawnParticle(connector);
                }
            }
        }

        // ─────────── THEME TEXTURE VFX ───────────
        // Uses NachtmusikThemeTextures for theme-specific stellar visuals.

        /// <summary>
        /// Draws a themed cosmic impact ring using Nachtmusik Power Effect Ring.
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
                    (StarlitBlue with { A = 0 }) * 0.5f * intensity, rotation, origin,
                    scale * 0.15f, SpriteEffects.None, 0f);
                sb.Draw(ring, drawPos, null,
                    (CosmicPurple with { A = 0 }) * 0.3f * intensity, -rotation * 0.7f, origin,
                    scale * 0.10f, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Draws themed stellar lens flare at a position using Nachtmusik star textures.
        /// Must be called in Additive blend mode.
        /// </summary>
        public static void DrawThemeStarFlare(SpriteBatch sb, Vector2 worldPos,
            float scale, float intensity = 1f)
        {
            Vector2 drawPos = worldPos - Main.screenPosition;

            Texture2D flare = NachtmusikThemeTextures.NKLensFlare?.Value;
            if (flare != null)
            {
                Vector2 origin = flare.Size() * 0.5f;
                float rot = (float)Main.GameUpdateCount * 0.035f;
                sb.Draw(flare, drawPos, null,
                    (StarWhite with { A = 0 }) * 0.5f * intensity, rot, origin,
                    scale * 0.08f, SpriteEffects.None, 0f);
                sb.Draw(flare, drawPos, null,
                    (RadianceGold with { A = 0 }) * 0.3f * intensity, -rot * 0.5f, origin,
                    scale * 0.06f, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Draws themed radial slash star for melee impacts using NK Radial Slash Star.
        /// Must be called in Additive blend mode.
        /// </summary>
        public static void DrawThemeRadialSlash(SpriteBatch sb, Vector2 worldPos,
            float scale, float intensity = 1f, float rotation = 0f)
        {
            Texture2D slash = NachtmusikThemeTextures.NKRadialSlashStar?.Value;
            if (slash == null) return;

            Vector2 drawPos = worldPos - Main.screenPosition;
            Vector2 origin = slash.Size() * 0.5f;

            sb.Draw(slash, drawPos, null,
                (DeepBlue with { A = 0 }) * 0.4f * intensity, rotation, origin,
                scale * 0.14f, SpriteEffects.None, 0f);
            sb.Draw(slash, drawPos, null,
                (StarlitBlue with { A = 0 }) * 0.6f * intensity, -rotation * 0.5f, origin,
                scale * 0.08f, SpriteEffects.None, 0f);
            sb.Draw(slash, drawPos, null,
                (StarWhite with { A = 0 }) * 0.7f * intensity, rotation * 1.5f, origin,
                scale * 0.04f, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Draws a themed comet burst at a position using NK Comet texture.
        /// Must be called in Additive blend mode.
        /// </summary>
        public static void DrawThemeComet(SpriteBatch sb, Vector2 worldPos,
            float scale, float intensity = 1f, float rotation = 0f)
        {
            Texture2D comet = NachtmusikThemeTextures.NKComet?.Value;
            if (comet == null) return;

            Vector2 drawPos = worldPos - Main.screenPosition;
            Vector2 origin = comet.Size() * 0.5f;

            sb.Draw(comet, drawPos, null,
                (StarlitBlue with { A = 0 }) * 0.55f * intensity, rotation, origin,
                scale * 0.10f, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Combined theme impact: bloom + star flare + impact ring.
        /// </summary>
        public static void DrawThemeImpactFull(SpriteBatch sb, Vector2 worldPos,
            float scale, float intensity = 1f)
        {
            DrawNachtmusikBloomStack(sb, worldPos, scale, 0.3f, intensity);
            DrawThemeStarFlare(sb, worldPos, scale, intensity * 0.7f);
            float rot = (float)Main.GameUpdateCount * 0.02f;
            DrawThemeImpactRing(sb, worldPos, scale, intensity * 0.6f, rot);
        }

        // ─────────── LUT TEXTURE SAMPLING (CPU-SIDE) ───────────

        private static Color[] _lutPixelCache;
        private static int _lutWidth;

        /// <summary>
        /// Sample the NachtmusikGradientLUTandRAMP texture on CPU.
        /// t=0 → left edge, t=1 → right edge. Caches pixel data on first call.
        /// Returns the actual LUT colour (dark purples / yellows per the texture).
        /// Falls back to GetPaletteColor if the texture isn't available.
        /// </summary>
        public static Color SampleLUT(float t)
        {
            if (_lutPixelCache == null)
            {
                var lutAsset = NachtmusikThemeTextures.NKGradientLUT;
                if (lutAsset?.Value == null)
                    return GetPaletteColor(t);

                Texture2D tex = lutAsset.Value;
                _lutWidth = tex.Width;
                _lutPixelCache = new Color[tex.Width * tex.Height];
                tex.GetData(_lutPixelCache);
            }

            t = MathHelper.Clamp(t, 0f, 1f);
            int x = (int)(t * (_lutWidth - 1));
            return _lutPixelCache[x]; // sample first row
        }

        // ─────────── LUT-RAMPED SPARKLE PARTICLES ───────────

        /// <summary>
        /// Spawn LUT-colour-ramped SparkleParticle / TwinklingSparkleParticle along a swing arc
        /// or projectile trail. Colors are sampled through the NachtmusikGradientLUTandRAMP
        /// texture (dark purples → yellows), matching the actual gradient ramp asset.
        /// </summary>
        public static void SpawnGradientSparkles(Vector2 pos, Vector2 velocity, int count = 3,
            float scale = 0.3f, int lifetime = 18, float spread = 8f, bool twinkling = true)
        {
            for (int i = 0; i < count; i++)
            {
                float t = (count <= 1) ? Main.rand.NextFloat() : (float)i / (count - 1);
                Color sparkleColor = SampleLUT(t);

                Vector2 offset = Main.rand.NextVector2Circular(spread, spread);
                Vector2 vel = -velocity * 0.05f + Main.rand.NextVector2Circular(0.8f, 0.8f);
                float sparkScale = scale * Main.rand.NextFloat(0.7f, 1.3f);

                if (twinkling && Main.rand.NextBool())
                {
                    var twinkle = new TwinklingSparkleParticle(pos + offset, vel, sparkleColor,
                        sparkleColor * 0.7f, sparkScale * 0.6f, sparkScale, lifetime,
                        twinkleSpeed: Main.rand.NextFloat(0.15f, 0.3f), bloomScale: 1.3f);
                    MagnumParticleHandler.SpawnParticle(twinkle);
                }
                else
                {
                    var sparkle = new SparkleParticle(pos + offset, vel, sparkleColor, sparkScale, lifetime);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
            }
        }

        /// <summary>
        /// Spawn a radial burst of LUT-colour-ramped SparkleParticle for impact/explosion VFX.
        /// Each sparkle's colour is sampled progressively through the NachtmusikGradientLUTandRAMP
        /// texture, creating a colour-ramped starburst with the actual LUT gradient.
        /// </summary>
        public static void SpawnGradientSparkleExplosion(Vector2 pos, int count = 10,
            float speed = 6f, float scale = 0.35f, int lifetime = 22)
        {
            for (int i = 0; i < count; i++)
            {
                float t = (float)i / count;
                Color sparkleColor = SampleLUT(t);

                float angle = MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 vel = angle.ToRotationVector2() * speed * Main.rand.NextFloat(0.6f, 1.3f);
                float sparkScale = scale * Main.rand.NextFloat(0.7f, 1.3f);

                if (Main.rand.NextBool(3))
                {
                    var twinkle = new TwinklingSparkleParticle(pos, vel, sparkleColor,
                        sparkleColor * 0.7f, sparkScale * 0.5f, sparkScale, lifetime,
                        twinkleSpeed: Main.rand.NextFloat(0.12f, 0.25f), bloomScale: 1.4f);
                    MagnumParticleHandler.SpawnParticle(twinkle);
                }
                else
                {
                    var sparkle = new SparkleParticle(pos, vel, sparkleColor,
                        sparkleColor * 0.6f, sparkScale, lifetime,
                        rotationSpeed: Main.rand.NextFloat(0.5f, 1.5f), bloomScale: 1.2f);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
            }
        }
    }
}
