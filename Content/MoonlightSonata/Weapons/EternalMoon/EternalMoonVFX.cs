using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Common.Systems.VFX.Bloom;
using MagnumOpus.Common.Systems.VFX.Optimization;
using MagnumOpus.Content.MoonlightSonata;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.EternalMoon
{
    /// <summary>
    /// Unique VFX for Eternal Moon — "The Eternal Tide".
    /// Crescent moon arcs that wax/wane through the combo.
    /// Trail flows like moonlight on water, building from thin crescent (new moon)
    /// through half moon to full moon crescendo.
    ///
    /// All palette references go through MoonlightVFXLibrary.
    /// </summary>
    public static class EternalMoonVFX
    {
        // === UNIQUE ACCENT COLORS (layered on top of canonical palette) ===
        private static readonly Color CrescentGold = new Color(255, 240, 180);
        private static readonly Color LunarEclipse = new Color(60, 20, 80);

        // === LUNAR PHASE MAPPING ===
        // ComboStep → moon phase: 0 = new moon (thin, dim), 1 = half (mid), 2 = full (massive)
        private static float GetMoonPhase(int comboStep)
        {
            return comboStep switch
            {
                0 => 0.2f,  // New moon — thin crescent
                1 => 0.5f,  // Half moon — wider
                2 => 1.0f,  // Full moon — maximum intensity
                _ => 0.5f
            };
        }

        /// <summary>
        /// Lunar phase color cycling unique to EternalMoon.
        /// Each combo step shifts the hue band for visual variety across the combo.
        /// </summary>
        public static Color GetLunarPhaseColor(float progress, int comboStep)
        {
            float phaseOffset = comboStep * 0.08f;
            float t = MathHelper.Clamp(progress + phaseOffset, 0f, 1f);

            if (t < 0.3f)
                return Color.Lerp(MoonlightVFXLibrary.DarkPurple, MoonlightVFXLibrary.Violet, t / 0.3f);
            else if (t < 0.6f)
                return Color.Lerp(MoonlightVFXLibrary.Violet, MoonlightVFXLibrary.IceBlue, (t - 0.3f) / 0.3f);
            else
                return Color.Lerp(MoonlightVFXLibrary.IceBlue, MoonlightVFXLibrary.MoonWhite, (t - 0.6f) / 0.4f);
        }

        // ─────────── CRESCENT BLOOM ───────────

        /// <summary>
        /// Crescent-shaped multi-layer bloom at the blade tip.
        /// The crescent offset creates an asymmetric glow that looks like a waxing moon.
        /// Phase-dependent: dims at combo start, brilliant at finale.
        /// </summary>
        public static void DrawCrescentTipBloom(SpriteBatch sb, Vector2 tipWorldPos,
            float swordRotation, int comboStep, float progression)
        {
            if (sb == null) return;

            float phase = GetMoonPhase(comboStep);
            float pulse = 1f + MathF.Sin(Main.GlobalTimeWrappedHourly * 8f + comboStep) * 0.12f;
            float intensityRamp = MathHelper.Clamp(progression * 3f, 0f, 1f)
                                * MathHelper.Clamp((1f - progression) * 4f, 0f, 1f);
            float baseScale = (0.25f + phase * 0.25f) * pulse * intensityRamp;

            if (baseScale < 0.05f) return;

            // Crescent offset perpendicular to blade — creates asymmetric glow
            Vector2 crescentOffset = new Vector2(
                MathF.Cos(swordRotation + MathHelper.PiOver2),
                MathF.Sin(swordRotation + MathHelper.PiOver2)) * (4f * phase);

            Vector2 drawPos = tipWorldPos - Main.screenPosition;

            // Use bloom stack with crescent offset for outer layers
            var bloomTex = MagnumTextureRegistry.GetSoftGlow();
            if (bloomTex == null) return;
            Vector2 origin = bloomTex.Size() * 0.5f;

            // Layer 1: Outer crescent halo (DarkPurple, large, offset)
            sb.Draw(bloomTex, drawPos + crescentOffset, null,
                (MoonlightVFXLibrary.DarkPurple with { A = 0 }) * 0.25f * intensityRamp,
                0f, origin, baseScale * 2.2f, SpriteEffects.None, 0f);

            // Layer 2: Mid crescent (Violet, medium offset)
            sb.Draw(bloomTex, drawPos + crescentOffset * 0.5f, null,
                (MoonlightVFXLibrary.Violet with { A = 0 }) * 0.4f * intensityRamp,
                0f, origin, baseScale * 1.5f, SpriteEffects.None, 0f);

            // Layer 3: Inner glow (IceBlue, centered)
            sb.Draw(bloomTex, drawPos, null,
                (MoonlightVFXLibrary.IceBlue with { A = 0 }) * 0.55f * intensityRamp,
                0f, origin, baseScale * 1.0f, SpriteEffects.None, 0f);

            // Layer 4: White-hot core with gold accent
            sb.Draw(bloomTex, drawPos, null,
                (CrescentGold with { A = 0 }) * 0.7f * intensityRamp,
                0f, origin, baseScale * 0.45f, SpriteEffects.None, 0f);
        }

        // ─────────── SWING FRAME VFX ───────────

        /// <summary>
        /// Per-frame swing VFX unique to EternalMoon.
        /// Spawns crescent dust arcs, orbiting lunar motes, and phase-cycling music notes.
        /// </summary>
        public static void SwingFrameEffects(Vector2 ownerCenter, Vector2 tipPos,
            Vector2 swordDirection, int comboStep, int timer)
        {
            // Dense crescent dust along blade (2/frame, palette-cycled)
            for (int i = 0; i < 2; i++)
            {
                float bladeProgress = Main.rand.NextFloat(0.4f, 1f);
                Vector2 dustPos = Vector2.Lerp(ownerCenter, tipPos, bladeProgress);
                Vector2 perpendicular = new Vector2(-swordDirection.Y, swordDirection.X);
                dustPos += perpendicular * Main.rand.NextFloat(-6f, 6f);

                Color dustColor = GetLunarPhaseColor(bladeProgress, comboStep);
                Dust d = Dust.NewDustPerfect(dustPos, DustID.PurpleTorch,
                    -swordDirection * Main.rand.NextFloat(1f, 3f), 0, dustColor, 1.6f);
                d.noGravity = true;
                d.fadeIn = 1.1f;
            }

            // Contrasting silver sparkle every other frame
            if (timer % 2 == 0)
            {
                Vector2 sparkPos = Vector2.Lerp(ownerCenter, tipPos, Main.rand.NextFloat(0.6f, 1f));
                MoonlightVFXLibrary.SpawnContrastSparkle(sparkPos, -swordDirection);
            }

            // Orbiting lunar mote at blade midpoint every 4 frames
            if (timer % 4 == 0)
            {
                Vector2 midBlade = Vector2.Lerp(ownerCenter, tipPos, 0.6f);
                MoonlightVFXLibrary.SpawnMusicNotes(midBlade, 1, 5f, 0.7f, 0.9f, 30);
            }
        }

        // ─────────── IMPACT VFX ───────────

        /// <summary>
        /// On-hit impact VFX unique to EternalMoon — crescent shockwave + phase-colored burst.
        /// Builds on top of MoonlightVFXLibrary.MeleeImpact with crescent-specific additions.
        /// </summary>
        public static void OnHitImpact(Vector2 hitPos, int comboStep, bool isCrit)
        {
            // Base moonlight impact (shared library)
            MoonlightVFXLibrary.MeleeImpact(hitPos, comboStep);

            // UNIQUE: Crescent arc ring — offset halo
            float arcScale = 0.4f + comboStep * 0.1f;
            CustomParticles.MoonlightHalo(hitPos + Main.rand.NextVector2Circular(5f, 5f), arcScale);

            // UNIQUE: Phase-colored radial sparkles
            int sparkCount = 4 + comboStep * 2;
            for (int i = 0; i < sparkCount; i++)
            {
                float angle = MathHelper.TwoPi * i / sparkCount;
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                Color sparkColor = GetLunarPhaseColor((float)i / sparkCount, comboStep) with { A = 0 };

                Dust d = Dust.NewDustPerfect(hitPos, DustID.MagicMirror, sparkVel, 0, sparkColor, 1.4f);
                d.noGravity = true;
            }

            // Crit bonus: constellation starburst — star points with bright center
            if (isCrit)
            {
                int starCount = 5;
                for (int i = 0; i < starCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / starCount + Main.rand.NextFloat(-0.2f, 0.2f);
                    float dist = Main.rand.NextFloat(20f, 40f);
                    Vector2 starPos = hitPos + angle.ToRotationVector2() * dist;
                    CustomParticles.GenericFlare(starPos, MoonlightVFXLibrary.Lavender, 0.4f, 20);
                }
                CustomParticles.GenericFlare(hitPos, CrescentGold, 0.7f, 15);

                // God ray burst on crits for extra impact
                GodRaySystem.CreateBurst(hitPos, MoonlightVFXLibrary.Violet,
                    rayCount: 4, radius: 40f, duration: 20, GodRaySystem.GodRayStyle.Explosion);
            }
        }

        // ─────────── CRESCENDO FINALE ───────────

        /// <summary>
        /// Phase 2 finisher — massive crescent explosion with expanding lunar rings,
        /// screen shake, god rays, ripple distortion, and music note cascade.
        /// </summary>
        public static void CrescendoFinaleVFX(Vector2 pos)
        {
            // Use shared finisher as base
            MoonlightVFXLibrary.FinisherSlam(pos, 1.5f);

            // UNIQUE: Triple expanding crescent rings
            for (int ring = 0; ring < 3; ring++)
            {
                Color ringColor = Color.Lerp(MoonlightVFXLibrary.IceBlue, CrescentGold, ring / 3f);
                CustomParticles.HaloRing(pos, ringColor, 0.5f + ring * 0.2f, 20 + ring * 5);
            }

            // UNIQUE: Lunar music note cascade (8 notes spiraling outward)
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 noteOffset = angle.ToRotationVector2() * 8f;
                MoonlightVFXLibrary.SpawnMusicNotes(pos + noteOffset, 1, 8f, 0.8f, 1.1f, 40);
            }

            // God ray burst — 6 rays, expansion style
            GodRaySystem.CreateBurst(pos, MoonlightVFXLibrary.IceBlue,
                rayCount: 6, radius: 60f, duration: 30, GodRaySystem.GodRayStyle.Explosion,
                secondaryColor: MoonlightVFXLibrary.Violet);

            // Screen ripple distortion
            if (AdaptiveQualityManager.Instance?.CurrentQuality >= AdaptiveQualityManager.QualityLevel.Medium)
            {
                ScreenDistortionManager.TriggerRipple(pos, MoonlightVFXLibrary.Violet, 0.8f, 25);
                MagnumScreenEffects.AddScreenShake(6f);
            }
        }

        // ─────────── TRAIL FUNCTIONS ───────────

        /// <summary>
        /// Trail color function for EternalMoon projectiles.
        /// Returns lunar-phase cycling color with {A=0} for additive rendering.
        /// </summary>
        public static Color WaveTrailColor(float progress)
        {
            Color c = GetLunarPhaseColor(progress, 0);
            return (c * (1f - progress * 0.7f)) with { A = 0 };
        }

        /// <summary>
        /// Trail width function for EternalMoon sub-projectiles.
        /// Crescent-shaped taper: rises quickly, holds wide, then tapers to a point.
        /// </summary>
        public static float WaveTrailWidth(float progress)
        {
            float ramp = MathHelper.Clamp(progress * 5f, 0f, 1f);
            float fade = MathHelper.Clamp((1f - progress) * 3f, 0f, 1f);
            return ramp * fade * 22f;
        }

        /// <summary>
        /// Dynamic lighting for EternalMoon effects — pulsing violet with phase cycling.
        /// </summary>
        public static void AddCrescentLight(Vector2 worldPos, float intensity = 0.8f)
        {
            float pulse = 0.85f + MathF.Sin(Main.GlobalTimeWrappedHourly * 6f) * 0.15f;
            Vector3 lightColor = MoonlightVFXLibrary.Violet.ToVector3() * intensity * pulse;
            Lighting.AddLight(worldPos, lightColor);
        }
    }
}
