using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Bloom;
using MagnumOpus.Common.Systems.Shaders;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.DiesIrae
{
    /// <summary>
    /// Shared Dies Irae VFX library — canonical palette, bloom stacking,
    /// shader setup, trail helpers, music notes, dust, ember spawns,
    /// infernal effects, and impact VFX used by ALL Dies Irae weapons,
    /// accessories, projectiles, minions, enemies, and bosses.
    ///
    /// Dies Irae identity: Day of wrath, judgment, apocalyptic hellfire,
    /// divine fury, chains, blood, bone ash. Every effect burns with
    /// righteous fire and smolders with the weight of condemnation.
    /// </summary>
    public static class DiesIraeVFXLibrary
    {
        // ─────────── CANONICAL PALETTE (forwarded from DiesIraePalette) ───────────
        public static readonly Color CharcoalBlack = DiesIraePalette.CharcoalBlack;
        public static readonly Color BloodRed      = DiesIraePalette.BloodRed;
        public static readonly Color InfernalRed   = DiesIraePalette.InfernalRed;
        public static readonly Color JudgmentGold  = DiesIraePalette.JudgmentGold;
        public static readonly Color BoneWhite     = DiesIraePalette.BoneWhite;
        public static readonly Color WrathWhite    = DiesIraePalette.WrathWhite;

        // Convenience accessors
        public static readonly Color EmberOrange   = DiesIraePalette.EmberOrange;
        public static readonly Color HellfireGold  = DiesIraePalette.HellfireGold;

        // Palette as array for indexed access
        private static readonly Color[] Palette = { CharcoalBlack, BloodRed, InfernalRed, JudgmentGold, BoneWhite, WrathWhite };

        // Hue range for HueShiftingMusicNoteParticle (red hue range, wrapping around 0)
        private const float HueMin = 0.97f;
        private const float HueMax = 0.05f;
        private const float NoteSaturation = 0.95f;
        private const float NoteLuminosity = 0.65f;

        // Dies Irae glow profile for GlowRenderer
        public static readonly GlowRenderer.GlowLayer[] DiesIraeGlowProfile = new[]
        {
            new GlowRenderer.GlowLayer(1.0f, 1.0f, new Color(255, 250, 240)),    // WrathWhite core
            new GlowRenderer.GlowLayer(1.6f, 0.65f, new Color(200, 170, 50)),     // JudgmentGold inner
            new GlowRenderer.GlowLayer(2.5f, 0.4f, new Color(200, 30, 30)),       // InfernalRed mid
            new GlowRenderer.GlowLayer(4.0f, 0.2f, new Color(130, 0, 0))          // BloodRed outer
        };

        // ─────────── PALETTE INTERPOLATION ───────────

        /// <summary>
        /// Lerp through the 6-colour Dies Irae palette. t=0 -> CharcoalBlack, t=1 -> WrathWhite.
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
        /// Drop-in replacement for the GetGradient() in weapon files.
        /// BloodRed -> JudgmentGold over 0->1.
        /// </summary>
        public static Color GetDiesIraeGradient(float progress)
            => DiesIraePalette.GetGradient(progress);

        /// <summary>
        /// Get the fire gradient: CharcoalBlack -> InfernalRed -> WrathWhite.
        /// </summary>
        public static Color GetFireGradient(float progress)
            => DiesIraePalette.GetFireGradient(progress);

        /// <summary>
        /// Get the wrath gradient: BloodRed -> InfernalRed -> HellfireGold.
        /// </summary>
        public static Color GetWrathGradient(float progress)
            => DiesIraePalette.GetWrathGradient(progress);

        // ─────────── SPRITEBATCH STATE HELPERS ───────────

        /// <summary>
        /// Switch SpriteBatch to additive blend for Dies Irae VFX rendering.
        /// </summary>
        public static void BeginDiesIraeAdditive(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Restore SpriteBatch to standard AlphaBlend after additive rendering.
        /// </summary>
        public static void EndDiesIraeAdditive(SpriteBatch sb)
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
        /// Counter-rotating double flare — judgment gold and blood red spinning in opposite directions.
        /// Creates the signature wrathful dual-fire at projectile centers.
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
                (JudgmentGold with { A = 0 }) * 0.6f * opacity, rot1, origin, scale * 0.7f, SpriteEffects.None, 0f);
            sb.Draw(flare, drawPos, null,
                (BloodRed with { A = 0 }) * 0.5f * opacity, rot2, origin, scale * 0.5f, SpriteEffects.None, 0f);
        }


        // ─────────── GLOW RENDERER INTEGRATION ───────────

        /// <summary>
        /// Draw Dies Irae-themed multi-layer glow via GlowRenderer.
        /// </summary>
        public static void DrawDiesIraeGlow(SpriteBatch sb, Vector2 worldPos,
            float scale = 1f, float intensity = 1f, string rotationId = null)
        {
            GlowRenderer.DrawGlow(sb, worldPos, DiesIraeGlowProfile, WrathWhite, intensity * scale, rotationId);
        }

        /// <summary>
        /// Draw Dies Irae glow with automatic SpriteBatch state management.
        /// </summary>
        public static void DrawDiesIraeGlowManaged(SpriteBatch sb, Vector2 worldPos,
            float scale = 1f, float intensity = 1f, string rotationId = null)
        {
            GlowRenderer.DrawGlowManaged(sb, worldPos, DiesIraeGlowProfile, WrathWhite, intensity * scale, rotationId);
        }

        // ─────────── TRAIL WIDTH/COLOR FUNCTIONS ───────────

        /// <summary>
        /// Standard Dies Irae trail width: fiery, aggressive taper.
        /// </summary>
        public static float DiesIraeTrailWidth(float completionRatio, float baseWidth = 18f)
        {
            float tipFade = MathF.Pow(1f - completionRatio, 0.6f);
            float headFade = MathF.Pow(completionRatio, 3f);
            return baseWidth * tipFade * (1f - headFade);
        }

        /// <summary>
        /// Thin precision trail for infernal ranged weapons — searing shot.
        /// </summary>
        public static float PrecisionTrailWidth(float completionRatio, float baseWidth = 6f)
        {
            float taper = 1f - completionRatio;
            return baseWidth * taper * taper;
        }

        /// <summary>
        /// Wide wrathful trail for melee weapons — sweeping flame arc.
        /// </summary>
        public static float WrathfulTrailWidth(float completionRatio, float baseWidth = 16f)
        {
            float tipFade = MathF.Pow(1f - completionRatio, 0.4f);
            return baseWidth * tipFade;
        }

        /// <summary>
        /// Trail color function with {A=0} for additive rendering.
        /// Red-gold gradient with white push: blood red at edges, gold-white center along trail.
        /// </summary>
        public static Color DiesIraeTrailColor(float completionRatio, float whitePush = 0.45f)
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
                0 => (CharcoalBlack.ToVector3(), BloodRed.ToVector3()),
                1 => (InfernalRed.ToVector3(), JudgmentGold.ToVector3()),
                2 => (BoneWhite.ToVector3(), WrathWhite.ToVector3()),
                _ => (BloodRed.ToVector3(), JudgmentGold.ToVector3()),
            };
        }

        // ─────────── MUSIC NOTES ───────────

        /// <summary>
        /// Spawn visible, hue-shifting Dies Irae music notes at the given position.
        /// Notes cycle through the red hue range (0.97-0.05 wrapping) for infernal effect.
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
        /// Dense Dies Irae flame dust trail at a blade tip during a swing.
        /// Torch dust tinted with InfernalRed for the signature hellfire effect.
        /// </summary>
        public static void SpawnSwingDust(Vector2 pos, Vector2 awayDirection, int dustType = DustID.Torch)
        {
            for (int i = 0; i < 2; i++)
            {
                Vector2 vel = awayDirection * Main.rand.NextFloat(1f, 3f) + Main.rand.NextVector2Circular(0.5f, 0.5f);
                Color col = Color.Lerp(InfernalRed, JudgmentGold, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, dustType, vel, 0, col, 1.5f);
                d.noGravity = true;
            }
        }

        /// <summary>
        /// Dies Irae flame trail dust — alternating Torch and ShadowFlame in red/orange.
        /// </summary>
        public static void SpawnFlameTrailDust(Vector2 pos, Vector2 awayDirection)
        {
            for (int i = 0; i < 2; i++)
            {
                Vector2 vel = awayDirection * Main.rand.NextFloat(1f, 3f) + Main.rand.NextVector2Circular(0.5f, 0.5f);
                bool isTorch = Main.rand.NextBool();
                int dustType = isTorch ? DustID.Torch : DustID.Shadowflame;
                Color col = isTorch ? InfernalRed : EmberOrange;
                Dust d = Dust.NewDustPerfect(pos, dustType, vel, isTorch ? 0 : 100, col, 1.5f);
                d.noGravity = true;
                d.fadeIn = 1.3f;
            }
        }

        /// <summary>
        /// Radial dust burst for on-hit / impact VFX.
        /// Fire burst: alternating Torch and Lava around the burst ring.
        /// </summary>
        public static void SpawnRadialDustBurst(Vector2 pos, int count = 12,
            float speed = 5f)
        {
            for (int i = 0; i < count; i++)
            {
                float progress = (float)i / count;
                float angle = MathHelper.TwoPi * i / count;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(speed * 0.6f, speed);
                bool isTorch = i % 2 == 0;
                int dustType = isTorch ? DustID.Torch : DustID.Lava;
                Color col = isTorch ? InfernalRed : EmberOrange;
                Dust d = Dust.NewDustPerfect(pos, dustType, vel, isTorch ? 0 : 100, col, 1.3f);
                d.noGravity = true;
            }
        }

        /// <summary>
        /// Ember shimmer dust — ember orange drift trail.
        /// </summary>
        public static void SpawnEmberShimmer(Vector2 pos, Vector2 awayDirection)
        {
            if (!Main.rand.NextBool(2)) return;
            Color emberCol = Color.Lerp(EmberOrange, HellfireGold, Main.rand.NextFloat());
            Dust d = Dust.NewDustPerfect(pos, DustID.Torch,
                awayDirection * 0.5f + Main.rand.NextVector2Circular(1.5f, 1.5f), 0, emberCol, 1.2f);
            d.noGravity = true;
        }

        // ─────────── DIES IRAE-SPECIFIC VFX: EMBERS & ASH ───────────

        /// <summary>
        /// Spawn falling ember particles raining down around a position.
        /// The signature Dies Irae visual identity — hellfire raining from above.
        /// </summary>
        public static void SpawnEmberRain(Vector2 pos, int count = 3, float radius = 30f, float scale = 0.25f)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 spawnPos = pos + new Vector2(Main.rand.NextFloat(-radius, radius), -radius * 0.5f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.8f, 0.8f), Main.rand.NextFloat(1.5f, 3.5f));
                Color emberCol = Color.Lerp(InfernalRed, HellfireGold, Main.rand.NextFloat());
                float emberScale = scale * Main.rand.NextFloat(0.7f, 1.3f);

                Dust d = Dust.NewDustPerfect(spawnPos, DustID.Torch, vel, 0, emberCol, emberScale * 5f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }
        }

        /// <summary>
        /// Spawn drifting ash flake particles around a position.
        /// Light, slow, gray — the remnants of what was burned away.
        /// </summary>
        public static void SpawnAshDrift(Vector2 pos, int count = 3)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 spawnPos = pos + Main.rand.NextVector2Circular(25f, 25f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-1.5f, -0.3f));
                Color ashCol = Color.Lerp(DiesIraePalette.AshGray, BoneWhite, Main.rand.NextFloat(0f, 0.4f));

                var glow = new GenericGlowParticle(spawnPos, vel,
                    ashCol * 0.5f,
                    0.15f * Main.rand.NextFloat(0.7f, 1.3f), 40, false);
                MagnumParticleHandler.SpawnParticle(glow);
            }
        }

        /// <summary>
        /// Spawn lingering flame wisps that hover and slowly dissipate.
        /// The residual fire left behind after a wrathful attack.
        /// </summary>
        public static void SpawnFlameResidue(Vector2 pos, int count = 3)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 spawnPos = pos + Main.rand.NextVector2Circular(20f, 20f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(-1.0f, -0.2f));
                Color flameCol = Color.Lerp(BloodRed, EmberOrange, Main.rand.NextFloat());

                var glow = new GenericGlowParticle(spawnPos, vel,
                    flameCol * 0.6f,
                    0.22f * Main.rand.NextFloat(0.8f, 1.2f), 30, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
        }

        /// <summary>
        /// Spawn a fiery radial explosion burst — wrath detonation.
        /// </summary>
        public static void SpawnWrathBurst(Vector2 pos, int count = 8, float scale = 1f)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 7f) * scale;
                Color burstCol = Color.Lerp(InfernalRed, HellfireGold, Main.rand.NextFloat());

                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, burstCol, 1.6f * scale);
                d.noGravity = true;
            }
            // Extra central flash embers
            for (int i = 0; i < count / 2; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f) * scale;
                Dust d = Dust.NewDustPerfect(pos, DustID.Lava, vel, 0, WrathWhite, 1.0f * scale);
                d.noGravity = true;
            }
        }

        // ─────────── DIES IRAE-SPECIFIC VFX: INFERNAL SPARKLES ───────────

        /// <summary>
        /// Spawn infernal sparkle particles — red-gold hue cycling points of fire.
        /// Creates the wrathful shimmer effect where divine fire meets earthly ruin.
        /// </summary>
        public static void SpawnInfernalSparkles(Vector2 pos, int count = 6, float radius = 25f)
        {
            for (int i = 0; i < count; i++)
            {
                // Red-gold hue cycling: 0.97-0.05 wrapping through red, plus 0.08-0.12 for gold
                float hue;
                if (i % 2 == 0)
                    hue = 0.97f + Main.rand.NextFloat() * 0.08f; // red range (wraps)
                else
                    hue = 0.08f + Main.rand.NextFloat() * 0.04f; // gold range

                hue %= 1f;
                Color sparkColor = Main.hslToRgb(hue, 0.95f, 0.7f);
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);
                Dust d = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(radius, radius),
                    DustID.Torch, vel, 0, sparkColor, 1.1f);
                d.noGravity = true;
            }
        }

        /// <summary>
        /// Combined infernal sparkle impact burst — the canonical mixed-element explosion.
        /// Dies Irae signature impact: hellfire colliding with divine judgment.
        /// Inner: Torch + ShadowFlame alternating burst. Outer: red-gold RainbowTorch burst.
        /// Accents: ember sparkles scattered between inner and outer.
        /// </summary>
        public static void SpawnMixedSparkleImpact(Vector2 pos, float intensity = 1f, int innerCount = 6, int outerCount = 6)
        {
            // INNER: Torch & ShadowFlame alternating explosion — tight infernal burst
            for (int i = 0; i < innerCount; i++)
            {
                float angle = MathHelper.TwoPi * i / innerCount + Main.rand.NextFloat(-0.15f, 0.15f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(1.5f, 3.5f) * intensity;
                bool isTorch = i % 2 == 0;
                int dustType = isTorch ? DustID.Torch : DustID.Shadowflame;
                Color col = isTorch ? InfernalRed : BloodRed;
                Dust d = Dust.NewDustPerfect(pos, dustType, vel, isTorch ? 0 : 100, col, 1.3f * intensity);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            // OUTER: Red-gold RainbowTorch explosion — wide infernal burst
            for (int i = 0; i < outerCount; i++)
            {
                float hue;
                if (i % 2 == 0)
                    hue = 0.97f + Main.rand.NextFloat() * 0.06f; // red range
                else
                    hue = 0.08f + Main.rand.NextFloat() * 0.04f; // gold range
                hue %= 1f;

                float angle = MathHelper.TwoPi * i / outerCount + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f) * intensity;
                Color col = Main.hslToRgb(hue, 0.95f, 0.65f);
                Dust d = Dust.NewDustPerfect(pos, DustID.RainbowTorch, vel, 0, col, 1.1f * intensity);
                d.noGravity = true;
            }

            // Ember sparkle accents (dust-based, scattered between inner and outer)
            int accentCount = Math.Max(1, (int)(3 * intensity));
            for (int i = 0; i < accentCount; i++)
            {
                Color sparkColor = Color.Lerp(EmberOrange, HellfireGold, Main.rand.NextFloat());
                Vector2 accentVel = Main.rand.NextVector2Circular(2f, 2f) * intensity;
                Dust accent = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(15f * intensity, 15f * intensity),
                    DustID.Torch, accentVel, 0, sparkColor, 0.9f * intensity);
                accent.noGravity = true;
            }
        }

        /// <summary>
        /// Spawn an infernal wrath explosion — full spectrum fire detonation.
        /// </summary>
        public static void SpawnInfernalExplosion(Vector2 pos, float intensity = 1f)
        {
            SpawnWrathBurst(pos, (int)(12 * intensity), intensity);
            SpawnEmberRain(pos, (int)(6 * intensity), 40f * intensity);
            SpawnFlameResidue(pos, (int)(4 * intensity));
        }

        /// <summary>
        /// Spawn infernal glow particles swirling inward toward a center.
        /// Creates the wrathful convergence effect — fire drawn to judgment.
        /// </summary>
        public static void SpawnInfernalSwirl(Vector2 center, int count = 6, float radius = 60f, float opacity = 0.65f)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float dist = radius + Main.rand.NextFloat(30f);
                Vector2 particlePos = center + angle.ToRotationVector2() * dist;
                Vector2 vel = (center - particlePos).SafeNormalize(Vector2.Zero) * 3f;

                Color fireCol = Color.Lerp(InfernalRed, JudgmentGold, (float)i / count);
                var glow = new GenericGlowParticle(particlePos, vel,
                    fireCol * opacity,
                    0.28f, 22, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
        }

        // ─────────── GRADIENT HALO RINGS ───────────

        /// <summary>
        /// Cascading gradient halo rings — CharcoalBlack -> WrathWhite.
        /// </summary>
        public static void SpawnGradientHaloRings(Vector2 pos, int count = 5, float baseScale = 0.3f)
        {
            for (int i = 0; i < count; i++)
            {
                float progress = (float)i / count;
                Color ringCol = Color.Lerp(CharcoalBlack, WrathWhite, progress);
                CustomParticles.HaloRing(pos, ringCol, baseScale + i * 0.12f, 14);
            }
        }

        /// <summary>
        /// Fire halo rings — red-gold hue cycling ring cascade.
        /// </summary>
        public static void SpawnFireHaloRings(Vector2 pos, int count = 5, float baseScale = 0.3f)
        {
            for (int i = 0; i < count; i++)
            {
                float progress = (float)i / count;
                Color ringCol = Color.Lerp(BloodRed, HellfireGold, progress);
                CustomParticles.HaloRing(pos, ringCol, baseScale + i * 0.12f, 14);
            }
        }

        // ─────────── IMPACTS ───────────

        /// <summary>
        /// Full Dies Irae melee impact VFX — fire burst, halo cascade,
        /// flame/ash dust burst, music note burst, and infernal sparkles.
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

            // Mixed infernal sparkle impact (Dies Irae signature)
            float impactIntensity = 0.6f + comboStep * 0.1f;
            SpawnMixedSparkleImpact(pos, impactIntensity, 4 + comboStep, 4 + comboStep);

            // Ash drift on heavier hits
            if (comboStep >= 1)
                SpawnAshDrift(pos, 2 + comboStep);

            // Fire halo rings
            try { CustomParticles.HaloRing(pos, JudgmentGold, 0.35f, 14); } catch { }
            try { CustomParticles.HaloRing(pos, BloodRed, 0.25f, 12); } catch { }

            Lighting.AddLight(pos, WrathWhite.ToVector3() * (0.6f + comboStep * 0.1f));
        }

        /// <summary>
        /// Projectile death / on-kill VFX — bigger, flashier version of MeleeImpact.
        /// Includes infernal burst, ember rain, and enhanced bloom.
        /// </summary>
        public static void ProjectileImpact(Vector2 pos, float intensity = 1f)
        {
            SpawnGradientHaloRings(pos, 3, 0.2f * intensity);
            SpawnMusicNotes(pos, 3, 20f * intensity, 0.6f, 0.9f, 25);
            SpawnRadialDustBurst(pos, 8, 5f * intensity);
            SpawnMixedSparkleImpact(pos, intensity, 6, 6);
            SpawnFlameResidue(pos, (int)(3 * intensity));
            Lighting.AddLight(pos, WrathWhite.ToVector3() * 0.8f * intensity);
        }

        // ─────────── SWING HELPERS ───────────

        /// <summary>
        /// Per-frame VFX to call from a swing projectile's AI().
        /// Handles flame trail dust, ember shimmer, periodic music notes.
        /// </summary>
        public static void SwingFrameVFX(Vector2 tipPos, Vector2 swordDirection, int comboStep,
            int timer, int dustType = DustID.Torch)
        {
            SpawnFlameTrailDust(tipPos, -swordDirection);
            SpawnEmberShimmer(tipPos, -swordDirection);

            if (timer % 5 == 0)
                SpawnMusicNotes(tipPos, 1, 10f, 0.7f, 0.9f, 25);

            Lighting.AddLight(tipPos, GetPaletteColor(0.4f + comboStep * 0.15f).ToVector3() * 0.6f);
        }

        // ─────────── FINISHER EFFECTS ───────────

        /// <summary>
        /// Phase-3 / finisher slam VFX — screen shake, massive flame burst, ember rain,
        /// fire halo detonation, music note scatter.
        /// </summary>
        public static void FinisherSlam(Vector2 pos, float intensity = 1f)
        {
            MagnumScreenEffects.AddScreenShake(6f * intensity);
            SpawnGradientHaloRings(pos, 4, 0.25f * intensity);
            SpawnFireHaloRings(pos, 3, 0.2f * intensity);
            SpawnMusicNotes(pos, 4, 30f, 0.7f, 1.0f, 35);
            SpawnRadialDustBurst(pos, 12, 6f * intensity);
            SpawnWrathBurst(pos, 10, intensity);
            SpawnMixedSparkleImpact(pos, 1.2f * intensity, 8, 8);
            SpawnEmberRain(pos, (int)(8 * intensity), 50f * intensity);
            SpawnInfernalSwirl(pos, 6, 60f * intensity);
            Lighting.AddLight(pos, WrathWhite.ToVector3() * 1.0f * intensity);
        }

        // ─────────── DYNAMIC LIGHTING ───────────

        /// <summary>
        /// Add standard Dies Irae ambient light at a position.
        /// </summary>
        public static void AddDiesIraeLight(Vector2 worldPos, float intensity = 0.6f)
        {
            Lighting.AddLight(worldPos, InfernalRed.ToVector3() * intensity);
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
        /// Add pulsing fire light with infernal color shift.
        /// </summary>
        public static void AddPulsingLight(Vector2 worldPos, float time, float intensity = 0.6f)
        {
            Color fireShift = Color.Lerp(InfernalRed, HellfireGold,
                (MathF.Sin(time * 0.08f) + 1f) * 0.5f);
            float pulse = MathF.Sin(time * 0.12f) * 0.15f + 0.85f;
            Lighting.AddLight(worldPos, fireShift.ToVector3() * pulse * intensity);
        }

        /// <summary>
        /// Add wrath light — oscillates between InfernalRed and JudgmentGold.
        /// </summary>
        public static void AddWrathLight(Vector2 worldPos, float time, float intensity = 0.6f)
        {
            float shift = MathF.Sin(time * 0.06f) * 0.5f + 0.5f;
            Color lightColor = Color.Lerp(InfernalRed, JudgmentGold, shift);
            Lighting.AddLight(worldPos, lightColor.ToVector3() * intensity);
        }

        // ─────────── INFERNAL SPARKLE IMPACT (REPLACES NOISE ZONES) ───────────

        /// <summary>
        /// Draws an infernal sparkle impact burst — multiple rotating Star4Soft sparkles
        /// with red-gold cycling, a fiery bloom core, and HaloRing edge.
        /// This replaces noise-scrolled zone effects for Dies Irae impacts.
        /// Must be called with an active SpriteBatch (TrueAdditive blend recommended).
        /// </summary>
        public static void DrawInfernalSparkleImpact(SpriteBatch sb, Vector2 worldPos, float radius,
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

                    // Red-gold hue cycling within Dies Irae range
                    float hue = ((time * 0.02f + i / (float)sparkleCount) % 0.12f);
                    hue = hue < 0.05f ? 0.97f + hue : hue + 0.03f;
                    hue %= 1f;
                    Color starColor = Main.hslToRgb(hue, 0.95f, 0.65f);

                    sb.Draw(star, drawPos + offset, null, (starColor with { A = 0 }) * 0.3f * opacity,
                        starRot, sOrigin, starScale, SpriteEffects.None, 0f);
                }
                // Center star — brighter, larger, wrath-white
                float centerRot = time * 0.035f;
                sb.Draw(star, drawPos, null, (WrathWhite with { A = 0 }) * 0.45f * opacity,
                    centerRot, sOrigin, 0.35f, SpriteEffects.None, 0f);
            }

            // Infernal edge ring
            Texture2D ring = MagnumTextureRegistry.GetHaloRing();
            if (ring != null)
            {
                Vector2 rOrigin = ring.Size() * 0.5f;
                float rScale = radius * 2f / ring.Width;
                Color rc = Color.Lerp(BloodRed, JudgmentGold,
                    (MathF.Sin(time * 0.035f) + 1f) * 0.5f);
                sb.Draw(ring, drawPos, null, (rc with { A = 0 }) * 0.2f * opacity,
                    time * 0.02f, rOrigin, rScale, SpriteEffects.None, 0f);
            }
        }

        // ─────────── THEME TEXTURE VFX ───────────
        // Uses DiesIraeThemeTextures for theme-specific visuals.

        /// <summary>
        /// Draws a themed wrathful impact ring using Dies Irae Power Effect Ring + Harmonic Impact.
        /// Must be called in Additive blend mode (or {A=0} pattern).
        /// </summary>
        public static void DrawThemeImpactRing(SpriteBatch sb, Vector2 worldPos,
            float scale, float intensity = 1f, float rotation = 0f)
        {
            Vector2 drawPos = worldPos - Main.screenPosition;

            Texture2D ring = DiesIraeThemeTextures.DIPowerEffectRing?.Value;
            if (ring != null)
            {
                Vector2 origin = ring.Size() * 0.5f;
                sb.Draw(ring, drawPos, null,
                    (JudgmentGold with { A = 0 }) * 0.5f * intensity, rotation, origin,
                    scale * 0.15f, SpriteEffects.None, 0f);
                sb.Draw(ring, drawPos, null,
                    (InfernalRed with { A = 0 }) * 0.3f * intensity, -rotation * 0.7f, origin,
                    scale * 0.10f, SpriteEffects.None, 0f);
            }

            Texture2D impact = DiesIraeThemeTextures.DIHarmonicImpact?.Value;
            if (impact != null)
            {
                Vector2 impOrigin = impact.Size() * 0.5f;
                sb.Draw(impact, drawPos, null,
                    (BloodRed with { A = 0 }) * 0.45f * intensity, rotation * 1.3f, impOrigin,
                    scale * 0.12f, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Draws a themed ash flake particle accent at a position.
        /// Must be called in Additive blend mode.
        /// </summary>
        public static void DrawThemeAshAccent(SpriteBatch sb, Vector2 worldPos,
            float scale, float intensity = 1f)
        {
            Vector2 drawPos = worldPos - Main.screenPosition;

            Texture2D ash = DiesIraeThemeTextures.DIAshFlake?.Value;
            if (ash != null)
            {
                Vector2 origin = ash.Size() * 0.5f;
                float rot = (float)Main.GameUpdateCount * 0.04f;
                sb.Draw(ash, drawPos, null,
                    (BoneWhite with { A = 0 }) * 0.45f * intensity, rot, origin,
                    scale * 0.07f, SpriteEffects.None, 0f);
                sb.Draw(ash, drawPos, null,
                    (DiesIraePalette.AshGray with { A = 0 }) * 0.3f * intensity, -rot * 0.6f, origin,
                    scale * 0.05f, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Combined theme impact: infernal bloom + theme ring + ash accents.
        /// </summary>
        public static void DrawThemeImpactFull(SpriteBatch sb, Vector2 worldPos,
            float scale, float intensity = 1f)
        {
            DrawThemeAshAccent(sb, worldPos, scale, intensity * 0.7f);
            float rot = (float)Main.GameUpdateCount * 0.02f;
            DrawThemeImpactRing(sb, worldPos, scale, intensity * 0.6f, rot);
        }
    }
}
