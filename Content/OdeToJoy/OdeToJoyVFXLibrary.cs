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

namespace MagnumOpus.Content.OdeToJoy
{
    /// <summary>
    /// Shared Ode to Joy VFX library — canonical palette, bloom stacking,
    /// shader setup, trail helpers, music notes, dust, petal spawns,
    /// garden effects, and impact VFX used by ALL Ode to Joy weapons,
    /// accessories, projectiles, minions, enemies, and bosses.
    ///
    /// Ode to Joy identity: Universal brotherhood, joyous celebration,
    /// garden triumph, roses, thorns, petals, pollen, verdant growth,
    /// warm golden radiance — Beethoven's Ninth made visual.
    /// </summary>
    public static class OdeToJoyVFXLibrary
    {
        // ─────────── CANONICAL PALETTE (forwarded from OdeToJoyPalette) ───────────

        // Greens and Foliage
        public static readonly Color MossShadow    = OdeToJoyPalette.MossShadow;
        public static readonly Color DeepForest     = OdeToJoyPalette.DeepForest;
        public static readonly Color LeafGreen      = OdeToJoyPalette.LeafGreen;
        public static readonly Color BudGreen       = OdeToJoyPalette.BudGreen;
        public static readonly Color VerdantGreen   = OdeToJoyPalette.VerdantGreen;

        // Pinks and Roses
        public static readonly Color RosePink       = OdeToJoyPalette.RosePink;
        public static readonly Color PetalPink      = OdeToJoyPalette.PetalPink;

        // Golds and Ambers
        public static readonly Color WarmAmber      = OdeToJoyPalette.WarmAmber;
        public static readonly Color GoldenPollen   = OdeToJoyPalette.GoldenPollen;
        public static readonly Color PollenGold     = OdeToJoyPalette.PollenGold;
        public static readonly Color SunlightYellow = OdeToJoyPalette.SunlightYellow;

        // Whites
        public static readonly Color WhiteBloom     = OdeToJoyPalette.WhiteBloom;

        // Master 6-stop palette array for indexed access (musical dynamics scale)
        private static readonly Color[] Palette = { MossShadow, DeepForest, LeafGreen, GoldenPollen, SunlightYellow, WhiteBloom };

        // Hue range for HueShiftingMusicNoteParticle (gold-green range)
        private const float HueMin = 0.08f;
        private const float HueMax = 0.18f;
        private const float NoteSaturation = 0.85f;
        private const float NoteLuminosity = 0.80f;

        // Ode to Joy glow profile for GlowRenderer
        public static readonly GlowRenderer.GlowLayer[] OdeToJoyGlowProfile = new[]
        {
            new GlowRenderer.GlowLayer(1.0f, 1.0f, WhiteBloom),                  // Core: warm white bloom
            new GlowRenderer.GlowLayer(1.6f, 0.65f, SunlightYellow),             // Inner: sunlight yellow
            new GlowRenderer.GlowLayer(2.5f, 0.4f, GoldenPollen),                // Mid: golden pollen
            new GlowRenderer.GlowLayer(4.0f, 0.2f, LeafGreen)                    // Outer: verdant green
        };

        // ─────────── PALETTE INTERPOLATION ───────────

        /// <summary>
        /// Lerp through the 6-colour Ode to Joy palette. t=0 -> MossShadow, t=1 -> WhiteBloom.
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
        /// Drop-in replacement for garden gradient calls in weapon files.
        /// MossShadow -> LeafGreen -> WhiteBloom over 0->1.
        /// </summary>
        public static Color GetOdeToJoyGradient(float progress)
            => OdeToJoyPalette.GetGardenGradient(progress);

        /// <summary>
        /// Get a cycling green-gold color for garden hue effects.
        /// </summary>
        public static Color GetGardenHue(float offset = 0f)
        {
            float hue = (HueMin + HueMax) * 0.5f + MathF.Sin(Main.GlobalTimeWrappedHourly * 2f + offset) * (HueMax - HueMin) * 0.5f;
            return Main.hslToRgb(hue, 0.85f, 0.75f);
        }

        /// <summary>
        /// Get a vivid garden color (higher saturation, brighter).
        /// </summary>
        public static Color GetVividGardenHue(float offset = 0f)
        {
            float hue = (HueMin + HueMax) * 0.5f + MathF.Sin(Main.GlobalTimeWrappedHourly * 2f + offset) * (HueMax - HueMin) * 0.5f;
            return Main.hslToRgb(hue, 1f, 0.85f);
        }

        // ─────────── SPRITEBATCH STATE HELPERS ───────────

        /// <summary>
        /// Switch SpriteBatch to additive blend for Ode to Joy VFX rendering.
        /// </summary>
        public static void BeginOdeToJoyAdditive(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
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
        /// Counter-rotating double flare — golden pollen and verdant green spinning in opposite directions.
        /// Creates the signature warm garden radiance at projectile centers.
        /// </summary>
        public static void DrawCounterRotatingFlares(SpriteBatch sb, Vector2 worldPos,
            float scale, float time, float opacity = 1f)
        {
            Texture2D flare = MagnumTextureRegistry.GetFlare();
            if (flare == null) return;

            // 1024px flare — cap so largest layer (scale*0.7) does not exceed 300px
            scale = MathHelper.Min(scale, 0.419f);

            Vector2 drawPos = worldPos - Main.screenPosition;
            Vector2 origin = flare.Size() * 0.5f;

            float rot1 = time * 2.5f;
            float rot2 = -time * 1.8f;

            sb.Draw(flare, drawPos, null,
                (GoldenPollen with { A = 0 }) * 0.6f * opacity, rot1, origin, scale * 0.7f, SpriteEffects.None, 0f);
            sb.Draw(flare, drawPos, null,
                (LeafGreen with { A = 0 }) * 0.5f * opacity, rot2, origin, scale * 0.5f, SpriteEffects.None, 0f);
        }


        // ─────────── GLOW RENDERER INTEGRATION ───────────

        /// <summary>
        /// Draw Ode to Joy-themed multi-layer glow via GlowRenderer.
        /// </summary>
        public static void DrawOdeToJoyGlow(SpriteBatch sb, Vector2 worldPos,
            float scale = 1f, float intensity = 1f, string rotationId = null)
        {
            GlowRenderer.DrawGlow(sb, worldPos, OdeToJoyGlowProfile, WhiteBloom, intensity * scale, rotationId);
        }

        /// <summary>
        /// Draw Ode to Joy glow with automatic SpriteBatch state management.
        /// </summary>
        public static void DrawOdeToJoyGlowManaged(SpriteBatch sb, Vector2 worldPos,
            float scale = 1f, float intensity = 1f, string rotationId = null)
        {
            GlowRenderer.DrawGlowManaged(sb, worldPos, OdeToJoyGlowProfile, WhiteBloom, intensity * scale, rotationId);
        }

        // ─────────── TRAIL WIDTH/COLOR FUNCTIONS ───────────

        /// <summary>
        /// Standard Ode to Joy trail width: warm, organic taper.
        /// </summary>
        public static float OdeToJoyTrailWidth(float completionRatio, float baseWidth = 18f)
        {
            float tipFade = MathF.Pow(1f - completionRatio, 0.6f);
            float headFade = MathF.Pow(completionRatio, 3f);
            return baseWidth * tipFade * (1f - headFade);
        }

        /// <summary>
        /// Thin precision trail for ranged weapons — golden pollen shot.
        /// </summary>
        public static float PrecisionTrailWidth(float completionRatio, float baseWidth = 6f)
        {
            float taper = 1f - completionRatio;
            return baseWidth * taper * taper;
        }

        /// <summary>
        /// Wide verdant trail for melee weapons — organic, sweeping arc.
        /// </summary>
        public static float VerdantTrailWidth(float completionRatio, float baseWidth = 16f)
        {
            float tipFade = MathF.Pow(1f - completionRatio, 0.4f);
            return baseWidth * tipFade;
        }

        /// <summary>
        /// Trail color function with {A=0} for additive rendering.
        /// Green-gold gradient with white push along trail.
        /// </summary>
        public static Color OdeToJoyTrailColor(float completionRatio, float whitePush = 0.45f)
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
                0 => (MossShadow.ToVector3(), DeepForest.ToVector3()),
                1 => (LeafGreen.ToVector3(), GoldenPollen.ToVector3()),
                2 => (SunlightYellow.ToVector3(), WhiteBloom.ToVector3()),
                _ => (GoldenPollen.ToVector3(), SunlightYellow.ToVector3()),
            };
        }

        // ─────────── MUSIC NOTES ───────────

        /// <summary>
        /// Spawn visible, hue-shifting Ode to Joy music notes at the given position.
        /// Notes cycle through the gold-green range (0.08-0.18) for garden warmth.
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
        /// Dense Ode to Joy swing dust trail at a blade tip during a swing.
        /// Green-gold torch dust for the signature garden warmth effect.
        /// </summary>
        public static void SpawnSwingDust(Vector2 pos, Vector2 awayDirection, int dustType = DustID.GreenTorch)
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
        /// Ode to Joy petal dust — alternating Flower and PinkSlime for botanical identity.
        /// Pink and green interleaved for the rose-garden duality.
        /// </summary>
        public static void SpawnPetalDust(Vector2 pos, Vector2 awayDirection)
        {
            for (int i = 0; i < 2; i++)
            {
                Vector2 vel = awayDirection * Main.rand.NextFloat(1f, 3f) + Main.rand.NextVector2Circular(0.5f, 0.5f);
                bool isPink = Main.rand.NextBool();
                int dustType = isPink ? DustID.PinkSlime : DustID.PinkTorch;
                Color col = isPink ? RosePink : LeafGreen;
                Dust d = Dust.NewDustPerfect(pos, dustType, vel, isPink ? 0 : 100, col, 1.5f);
                d.noGravity = true;
                d.fadeIn = 1.3f;
            }
        }

        /// <summary>
        /// Radial dust burst for on-hit / impact VFX.
        /// Alternating GreenTorch and Flower around the burst ring.
        /// </summary>
        public static void SpawnRadialDustBurst(Vector2 pos, int count = 12,
            float speed = 5f)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(speed * 0.6f, speed);
                bool isGreen = i % 2 == 0;
                int dustType = isGreen ? DustID.GreenTorch : DustID.PinkTorch;
                Color col = isGreen ? LeafGreen : RosePink;
                Dust d = Dust.NewDustPerfect(pos, dustType, vel, isGreen ? 0 : 100, col, 1.3f);
                d.noGravity = true;
            }
        }

        /// <summary>
        /// Golden pollen shimmer dust — gentle golden sparkle drift.
        /// </summary>
        public static void SpawnPollenShimmer(Vector2 pos, Vector2 awayDirection)
        {
            if (!Main.rand.NextBool(2)) return;
            float hue = Main.rand.NextFloat(HueMin, HueMax);
            Color golden = Main.hslToRgb(hue, 0.9f, 0.8f);
            Dust d = Dust.NewDustPerfect(pos, DustID.GoldFlame,
                awayDirection * 0.5f + Main.rand.NextVector2Circular(1.5f, 1.5f), 0, golden, 1.2f);
            d.noGravity = true;
        }

        /// <summary>
        /// Green-gold radial dust burst — verdant explosion ring.
        /// </summary>
        public static void SpawnGardenBurst(Vector2 pos, int count = 10, float speed = 5f)
        {
            for (int i = 0; i < count; i++)
            {
                float hue = MathHelper.Lerp(HueMin, HueMax, (float)i / count);
                float angle = MathHelper.TwoPi * i / count;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(speed * 0.6f, speed);
                Color col = Main.hslToRgb(hue, 0.9f, 0.75f);
                Dust d = Dust.NewDustPerfect(pos, DustID.GreenTorch, vel, 0, col, 1.4f);
                d.noGravity = true;
            }
        }

        // ─────────── ODE TO JOY-SPECIFIC VFX: PETALS ───────────

        /// <summary>
        /// Spawn floating rose petal particles around a position.
        /// The signature Ode to Joy visual identity — drifting petals of celebration.
        /// </summary>
        public static void SpawnPetalScatter(Vector2 pos, int count = 3, float radius = 30f, float scale = 0.25f)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(radius, radius);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(0.3f, 1.5f));
                Color petalColor = Color.Lerp(RosePink, PetalPink, Main.rand.NextFloat()) with { A = 0 };

                var glow = new GenericGlowParticle(pos + offset, vel, petalColor, scale, 30, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
        }

        /// <summary>
        /// Spawn gentle golden pollen floating particles.
        /// Warm, luminous specks drifting upward like sunlit pollen.
        /// </summary>
        public static void SpawnPollenDrift(Vector2 pos, int count = 3)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(25f, 25f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-1.2f, -0.3f));
                Color pollenColor = Color.Lerp(GoldenPollen, SunlightYellow, Main.rand.NextFloat()) with { A = 0 };

                var glow = new GenericGlowParticle(pos + offset, vel, pollenColor, 0.15f + Main.rand.NextFloat(0.1f), 25, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
        }

        /// <summary>
        /// Spawn green-gold thorny sparkle particles.
        /// Sharp botanical sparkles with verdant energy.
        /// </summary>
        public static void SpawnVineSparkle(Vector2 pos, int count = 3)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(20f, 20f);
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);
                Color sparkColor = Color.Lerp(LeafGreen, GoldenPollen, Main.rand.NextFloat()) with { A = 0 };

                Dust d = Dust.NewDustPerfect(pos + offset, DustID.GreenTorch, vel, 0, sparkColor, 1.1f);
                d.noGravity = true;
            }
        }

        /// <summary>
        /// Spawn a flower bloom burst explosion at a position.
        /// Radiating petals, pollen, and golden light — the garden in full bloom.
        /// </summary>
        public static void SpawnBloomBurst(Vector2 pos, int count = 8, float scale = 1f)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count;
                float speed = Main.rand.NextFloat(3f, 7f) * scale;
                Vector2 vel = angle.ToRotationVector2() * speed;

                Color bloomColor = (i % 3) switch
                {
                    0 => Color.Lerp(RosePink, PetalPink, Main.rand.NextFloat()),
                    1 => Color.Lerp(GoldenPollen, SunlightYellow, Main.rand.NextFloat()),
                    _ => Color.Lerp(LeafGreen, VerdantGreen, Main.rand.NextFloat())
                };
                bloomColor.A = 0;

                var glow = new GenericGlowParticle(pos, vel, bloomColor, 0.25f * scale, 22, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // Central bloom flash
            try { CustomParticles.GenericFlare(pos, WhiteBloom, 0.8f * scale, 15); } catch { }
            try { CustomParticles.GenericFlare(pos, GoldenPollen, 0.6f * scale, 20); } catch { }
        }

        // ─────────── ODE TO JOY-SPECIFIC VFX: JOYOUS SPARKLES ───────────

        /// <summary>
        /// Spawn joyous sparkle particles — green-gold-pink hue cycling points of light.
        /// Creates the jubilant shimmer effect at the boundary of nature and celebration.
        /// </summary>
        public static void SpawnJoyousSparkles(Vector2 pos, int count = 6, float radius = 25f)
        {
            for (int i = 0; i < count; i++)
            {
                // Cycle through green -> gold -> pink
                float hueT = (float)i / count;
                Color sparkColor = hueT < 0.33f
                    ? Color.Lerp(LeafGreen, GoldenPollen, hueT / 0.33f)
                    : hueT < 0.66f
                        ? Color.Lerp(GoldenPollen, RosePink, (hueT - 0.33f) / 0.33f)
                        : Color.Lerp(RosePink, LeafGreen, (hueT - 0.66f) / 0.34f);

                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);
                Dust d = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(radius, radius),
                    DustID.RainbowTorch, vel, 0, sparkColor, 1.1f);
                d.noGravity = true;
            }
        }

        /// <summary>
        /// Combined green-gold-pink sparkle + GreenTorch/Flower mixed explosion.
        /// Ode to Joy signature impact: botanical jubilance colliding with golden radiance.
        /// This is the canonical impact effect for ALL Ode to Joy weapon projectile hits.
        /// </summary>
        public static void SpawnMixedSparkleImpact(Vector2 pos, float intensity = 1f, int gardenCount = 6, int petalCount = 6)
        {
            // INNER: GreenTorch + Flower alternating burst — tight botanical duality
            for (int i = 0; i < petalCount; i++)
            {
                float angle = MathHelper.TwoPi * i / petalCount + Main.rand.NextFloat(-0.15f, 0.15f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(1.5f, 3.5f) * intensity;
                bool isGreen = i % 2 == 0;
                int dustType = isGreen ? DustID.GreenTorch : DustID.PinkTorch;
                Color col = isGreen ? LeafGreen : RosePink;
                Dust d = Dust.NewDustPerfect(pos, dustType, vel, isGreen ? 0 : 100, col, 1.3f * intensity);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            // OUTER: Green-gold RainbowTorch burst — wide verdant radiance
            for (int i = 0; i < gardenCount; i++)
            {
                float hue = MathHelper.Lerp(HueMin, HueMax, (float)i / gardenCount);
                float angle = MathHelper.TwoPi * i / gardenCount + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f) * intensity;
                Color col = Main.hslToRgb(hue, 0.9f, 0.75f);
                Dust d = Dust.NewDustPerfect(pos, DustID.RainbowTorch, vel, 0, col, 1.1f * intensity);
                d.noGravity = true;
            }

            // Pollen sparkle accents (dust-based, scattered between inner and outer)
            int accentCount = Math.Max(1, (int)(3 * intensity));
            for (int i = 0; i < accentCount; i++)
            {
                Color sparkColor = Color.Lerp(GoldenPollen, SunlightYellow, Main.rand.NextFloat());
                Vector2 accentVel = Main.rand.NextVector2Circular(2f, 2f) * intensity;
                Dust accent = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(15f * intensity, 15f * intensity),
                    DustID.GoldFlame, accentVel, 0, sparkColor, 0.9f * intensity);
                accent.noGravity = true;
            }
        }

        /// <summary>
        /// Spawn a garden bloom explosion — verdant burst of golden-green-pink particles.
        /// </summary>
        public static void SpawnGardenExplosion(Vector2 pos, float intensity = 1f)
        {
            SpawnBloomBurst(pos, (int)(12 * intensity), intensity);
            SpawnPetalScatter(pos, (int)(6 * intensity), 40f * intensity, 0.3f * intensity);
            SpawnPollenDrift(pos, (int)(4 * intensity));
        }

        /// <summary>
        /// Spawn golden glow particles swirling inward toward a center.
        /// Creates the jubilant convergence effect.
        /// </summary>
        public static void SpawnGoldenSwirl(Vector2 center, int count = 6, float radius = 60f, float opacity = 0.65f)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float dist = radius + Main.rand.NextFloat(30f);
                Vector2 particlePos = center + angle.ToRotationVector2() * dist;
                Vector2 vel = (center - particlePos).SafeNormalize(Vector2.Zero) * 3f;

                Color gardenCol = GetGardenHue((float)i / count);
                var glow = new GenericGlowParticle(particlePos, vel,
                    gardenCol * opacity,
                    0.28f, 22, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
        }

        // ─────────── GRADIENT HALO RINGS ───────────

        /// <summary>
        /// Cascading gradient halo rings — garden palette (MossShadow -> WhiteBloom).
        /// </summary>
        public static void SpawnGradientHaloRings(Vector2 pos, int count = 5, float baseScale = 0.3f)
        {
            for (int i = 0; i < count; i++)
            {
                float progress = (float)i / count;
                Color ringCol = Color.Lerp(MossShadow, WhiteBloom, progress);
                CustomParticles.HaloRing(pos, ringCol, baseScale + i * 0.12f, 14);
            }
        }

        /// <summary>
        /// Garden halo rings — green-gold hue cycling ring cascade.
        /// </summary>
        public static void SpawnGardenHaloRings(Vector2 pos, int count = 5, float baseScale = 0.3f)
        {
            for (int i = 0; i < count; i++)
            {
                float hue = MathHelper.Lerp(HueMin, HueMax, (float)i / count);
                Color ringCol = Main.hslToRgb(hue, 0.85f, 0.8f);
                CustomParticles.HaloRing(pos, ringCol, baseScale + i * 0.12f, 14);
            }
        }

        // ─────────── IMPACTS ───────────

        /// <summary>
        /// Full Ode to Joy melee impact VFX — petal scatter, halo cascade,
        /// thorn dust burst, music note burst, and garden sparkles.
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

            // Mixed green-gold + petal sparkle impact (Ode to Joy signature)
            float impactIntensity = 0.6f + comboStep * 0.1f;
            SpawnMixedSparkleImpact(pos, impactIntensity, 4 + comboStep, 4 + comboStep);

            // Petal scatter
            SpawnPetalScatter(pos, 2 + Math.Min(comboStep, 2), 20f, 0.2f);

            // Garden halo rings
            try { CustomParticles.HaloRing(pos, GoldenPollen, 0.35f, 14); } catch { }
            try { CustomParticles.HaloRing(pos, LeafGreen, 0.25f, 12); } catch { }

            Lighting.AddLight(pos, WhiteBloom.ToVector3() * (0.6f + comboStep * 0.1f));
        }

        /// <summary>
        /// Projectile death / on-kill VFX — bigger, flashier version of MeleeImpact.
        /// Includes bloom burst, pollen drift, and enhanced garden glow.
        /// </summary>
        public static void ProjectileImpact(Vector2 pos, float intensity = 1f)
        {
            SpawnGradientHaloRings(pos, 3, 0.2f * intensity);
            SpawnMusicNotes(pos, 3, 20f * intensity, 0.6f, 0.9f, 25);
            SpawnRadialDustBurst(pos, 8, 5f * intensity);
            SpawnMixedSparkleImpact(pos, intensity, 6, 6);
            SpawnPetalScatter(pos, 3, 25f * intensity, 0.2f);
            Lighting.AddLight(pos, WhiteBloom.ToVector3() * 0.8f * intensity);
        }

        // ─────────── SWING HELPERS ───────────

        /// <summary>
        /// Per-frame VFX to call from a swing projectile's AI().
        /// Handles petal dust trail, pollen shimmer, periodic music notes.
        /// </summary>
        public static void SwingFrameVFX(Vector2 tipPos, Vector2 swordDirection, int comboStep,
            int timer, int dustType = DustID.GreenTorch)
        {
            SpawnPetalDust(tipPos, -swordDirection);
            SpawnPollenShimmer(tipPos, -swordDirection);

            if (timer % 5 == 0)
                SpawnMusicNotes(tipPos, 1, 10f, 0.7f, 0.9f, 25);

            Lighting.AddLight(tipPos, GetPaletteColor(0.4f + comboStep * 0.15f).ToVector3() * 0.6f);
        }

        // ─────────── FINISHER EFFECTS ───────────

        /// <summary>
        /// Phase-3 / finisher slam VFX — screen shake, massive bloom burst, petal shower,
        /// garden explosion, golden swirl, music note scatter.
        /// </summary>
        public static void FinisherSlam(Vector2 pos, float intensity = 1f)
        {
            MagnumScreenEffects.AddScreenShake(6f * intensity);
            SpawnGradientHaloRings(pos, 4, 0.25f * intensity);
            SpawnGardenHaloRings(pos, 3, 0.2f * intensity);
            SpawnMusicNotes(pos, 4, 30f, 0.7f, 1.0f, 35);
            SpawnRadialDustBurst(pos, 12, 6f * intensity);
            SpawnGardenBurst(pos, 10, 6f * intensity);
            SpawnMixedSparkleImpact(pos, 1.2f * intensity, 8, 8);
            SpawnBloomBurst(pos, 10, intensity);
            SpawnPetalScatter(pos, 6, 40f * intensity, 0.3f);
            SpawnGoldenSwirl(pos, 6, 60f * intensity);
            Lighting.AddLight(pos, WhiteBloom.ToVector3() * 1.0f * intensity);
        }

        // ─────────── DYNAMIC LIGHTING ───────────

        /// <summary>
        /// Add standard Ode to Joy ambient light at a position.
        /// </summary>
        public static void AddOdeToJoyLight(Vector2 worldPos, float intensity = 0.6f)
        {
            Lighting.AddLight(worldPos, WhiteBloom.ToVector3() * intensity);
        }

        /// <summary>
        /// Add palette-interpolated dynamic light. Higher t = brighter, warmer.
        /// </summary>
        public static void AddPaletteLighting(Vector2 worldPos, float paletteT, float intensity = 0.8f)
        {
            Color col = GetPaletteColor(paletteT);
            Lighting.AddLight(worldPos, col.ToVector3() * intensity);
        }

        /// <summary>
        /// Add pulsing warm golden light.
        /// </summary>
        public static void AddPulsingLight(Vector2 worldPos, float time, float intensity = 0.6f)
        {
            Color golden = Color.Lerp(GoldenPollen, SunlightYellow, 0.3f);
            float pulse = (float)Math.Sin(time * 0.08f) * 0.15f + 0.85f;
            Lighting.AddLight(worldPos, golden.ToVector3() * pulse * intensity);
        }

        /// <summary>
        /// Add garden light — oscillates between LeafGreen and GoldenPollen.
        /// </summary>
        public static void AddGardenLight(Vector2 worldPos, float time, float intensity = 0.6f)
        {
            float shift = (float)Math.Sin(time * 0.06f) * 0.5f + 0.5f;
            Color lightColor = Color.Lerp(LeafGreen, GoldenPollen, shift);
            Lighting.AddLight(worldPos, lightColor.ToVector3() * intensity);
        }

        // ─────────── JOYOUS SPARKLE IMPACT (REPLACES NOISE ZONES) ───────────

        /// <summary>
        /// Draws a joyous sparkle impact burst — multiple rotating Star4Soft sparkles
        /// with green-gold-pink cycling, a small clamped bloom core, and optional HaloRing edge.
        /// This replaces the old DrawNoiseScrolledZone for impact effects.
        /// Must be called with an active SpriteBatch (TrueAdditive blend recommended).
        /// </summary>
        public static void DrawJoyousSparkleImpact(SpriteBatch sb, Vector2 worldPos, float radius,
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

                    // Cycle through green -> gold -> pink
                    float hueT = (time * 0.02f + i / (float)sparkleCount) % 1f;
                    Color starColor = hueT < 0.33f
                        ? Color.Lerp(LeafGreen, GoldenPollen, hueT / 0.33f)
                        : hueT < 0.66f
                            ? Color.Lerp(GoldenPollen, RosePink, (hueT - 0.33f) / 0.33f)
                            : Color.Lerp(RosePink, LeafGreen, (hueT - 0.66f) / 0.34f);

                    sb.Draw(star, drawPos + offset, null, (starColor with { A = 0 }) * 0.3f * opacity,
                        starRot, sOrigin, starScale, SpriteEffects.None, 0f);
                }
                // Center star — brighter, larger
                float centerRot = time * 0.035f;
                Color centerColor = Color.Lerp(GoldenPollen, WhiteBloom, 0.4f);
                sb.Draw(star, drawPos, null, (centerColor with { A = 0 }) * 0.45f * opacity,
                    centerRot, sOrigin, 0.35f, SpriteEffects.None, 0f);
            }

            // Garden edge ring
            Texture2D ring = MagnumTextureRegistry.GetHaloRing();
            if (ring != null)
            {
                Vector2 rOrigin = ring.Size() * 0.5f;
                float rScale = radius * 2f / ring.Width;
                Color rc = Color.Lerp(LeafGreen, GoldenPollen, (MathF.Sin(time * 0.03f) + 1f) * 0.5f);
                sb.Draw(ring, drawPos, null, (rc with { A = 0 }) * 0.2f * opacity,
                    time * 0.02f, rOrigin, rScale, SpriteEffects.None, 0f);
            }
        }

        // ─────────── THEME TEXTURE VFX ───────────
        // Uses OdeToJoyThemeTextures for theme-specific visuals.

        /// <summary>
        /// Draws a themed triumphant impact ring using Ode to Joy Power Effect Ring + Harmonic Impact.
        /// Must be called in Additive blend mode (or {A=0} pattern).
        /// </summary>
        public static void DrawThemeImpactRing(SpriteBatch sb, Vector2 worldPos,
            float scale, float intensity = 1f, float rotation = 0f)
        {
            Vector2 drawPos = worldPos - Main.screenPosition;

            Texture2D ring = OdeToJoyThemeTextures.OJPowerEffectRing?.Value;
            if (ring != null)
            {
                Vector2 origin = ring.Size() * 0.5f;
                sb.Draw(ring, drawPos, null,
                    (GoldenPollen with { A = 0 }) * 0.5f * intensity, rotation, origin,
                    scale * 0.15f, SpriteEffects.None, 0f);
                sb.Draw(ring, drawPos, null,
                    (LeafGreen with { A = 0 }) * 0.3f * intensity, -rotation * 0.7f, origin,
                    scale * 0.10f, SpriteEffects.None, 0f);
            }

            Texture2D impact = OdeToJoyThemeTextures.OJHarmonicImpact?.Value;
            if (impact != null)
            {
                Vector2 impOrigin = impact.Size() * 0.5f;
                sb.Draw(impact, drawPos, null,
                    (SunlightYellow with { A = 0 }) * 0.45f * intensity, rotation * 1.3f, impOrigin,
                    scale * 0.12f, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Draws a themed blossom sparkle particle accent at a position.
        /// Must be called in Additive blend mode.
        /// </summary>
        public static void DrawThemeBlossomAccent(SpriteBatch sb, Vector2 worldPos,
            float scale, float intensity = 1f)
        {
            Vector2 drawPos = worldPos - Main.screenPosition;

            Texture2D blossom = OdeToJoyThemeTextures.OJBlossomSparkle?.Value;
            if (blossom != null)
            {
                Vector2 origin = blossom.Size() * 0.5f;
                float rot = (float)Main.GameUpdateCount * 0.04f;
                sb.Draw(blossom, drawPos, null,
                    (GoldenPollen with { A = 0 }) * 0.45f * intensity, rot, origin,
                    scale * 0.07f, SpriteEffects.None, 0f);
                sb.Draw(blossom, drawPos, null,
                    (RosePink with { A = 0 }) * 0.3f * intensity, -rot * 0.6f, origin,
                    scale * 0.05f, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Combined theme impact: universal bloom + theme ring + blossom accents.
        /// </summary>
        public static void DrawThemeImpactFull(SpriteBatch sb, Vector2 worldPos,
            float scale, float intensity = 1f)
        {
            DrawThemeBlossomAccent(sb, worldPos, scale, intensity * 0.7f);
            float rot = (float)Main.GameUpdateCount * 0.02f;
            DrawThemeImpactRing(sb, worldPos, scale, intensity * 0.6f, rot);
        }
    }
}
