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

namespace MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight
{
    /// <summary>
    /// Unique VFX for Incisor of Moonlight — "The Stellar Scalpel".
    /// Theme: Surgical precision, resonant frequency pulses, constellation starpoints.
    /// Unlike EternalMoon's flowing crescent arcs, Incisor is sharp, tight, and precise.
    /// Each combo phase builds resonance — visual intensity escalates through the 4-phase combo.
    ///
    /// All palette references go through MoonlightVFXLibrary.
    /// </summary>
    public static class IncisorOfMoonlightVFX
    {
        // === UNIQUE ACCENT COLORS (layered on top of canonical palette) ===
        public static readonly Color ResonantSilver = new Color(230, 235, 255);
        public static readonly Color FrequencyPulse = new Color(170, 140, 255);
        public static readonly Color HarmonicWhite = new Color(255, 250, 245);
        public static readonly Color DeepResonance = new Color(90, 50, 160);

        // === RESONANCE MAPPING ===
        // ComboStep → resonance level: 0 = cool/dim, 1 = warming, 2 = hot, 3 = blazing
        private static float GetResonanceLevel(int comboStep)
        {
            return comboStep switch
            {
                0 => 0.3f,
                1 => 0.5f,
                2 => 0.75f,
                3 => 1.0f,
                _ => 0.5f
            };
        }

        /// <summary>
        /// Resonant frequency color — shifts with combo step to show building resonance.
        /// Phase 0: Cool silver. Phase 1: Warming purple. Phase 2: Hot violet. Phase 3: Blazing white.
        /// Unlike EternalMoon's lunar phase cycling, this follows resonant frequency escalation.
        /// </summary>
        public static Color GetResonanceColor(float progress, int comboStep)
        {
            float resonance = MathHelper.Clamp(comboStep / 3f, 0f, 1f);

            Color cold = Color.Lerp(ResonantSilver, MoonlightVFXLibrary.IceBlue, progress);
            Color hot = Color.Lerp(FrequencyPulse, HarmonicWhite, progress);

            return Color.Lerp(cold, hot, resonance);
        }

        // ----------- RESONANT EDGE BLOOM -----------

        /// <summary>
        /// Sharp, precise constellation-node bloom along the blade edge.
        /// Unlike EternalMoon's crescent offset bloom, this places tight starburst points
        /// at regular intervals — like constellation stars along the blade.
        /// Uses {A=0} premultiplied trick — no SpriteBatch restart needed.
        /// </summary>
        public static void DrawResonantEdgeBloom(SpriteBatch sb, Vector2 pommelPos,
            Vector2 tipPos, int comboStep, float progression)
        {
            if (sb == null) return;

            float intensityRamp = MathHelper.Clamp(progression * 4f, 0f, 1f)
                                * MathHelper.Clamp((1f - progression) * 3f, 0f, 1f);
            if (intensityRamp < 0.05f) return;

            var bloomTex = MagnumTextureRegistry.GetSoftGlow();
            if (bloomTex == null) return;
            Vector2 origin = bloomTex.Size() * 0.5f;

            float resonance = GetResonanceLevel(comboStep);
            float pulse = 1f + MathF.Sin(Main.GlobalTimeWrappedHourly * 12f) * 0.08f;

            // Constellation nodes along blade — every (pointCount) positions
            int pointCount = 4 + comboStep;
            for (int i = 0; i < pointCount; i++)
            {
                float t = (float)(i + 1) / (pointCount + 1);
                Vector2 drawPos = Vector2.Lerp(pommelPos, tipPos, t) - Main.screenPosition;
                float pointScale = (0.12f + t * 0.18f) * resonance * pulse * intensityRamp;

                // 4-layer {A=0} bloom stack — DeepResonance → FrequencyPulse → IceBlue → White
                sb.Draw(bloomTex, drawPos, null,
                    (DeepResonance with { A = 0 }) * 0.25f * intensityRamp,
                    0f, origin, pointScale * 2.0f, SpriteEffects.None, 0f);
                sb.Draw(bloomTex, drawPos, null,
                    (FrequencyPulse with { A = 0 }) * 0.45f * intensityRamp,
                    0f, origin, pointScale * 1.3f, SpriteEffects.None, 0f);
                sb.Draw(bloomTex, drawPos, null,
                    (MoonlightVFXLibrary.IceBlue with { A = 0 }) * 0.6f * intensityRamp,
                    0f, origin, pointScale * 0.8f, SpriteEffects.None, 0f);
                sb.Draw(bloomTex, drawPos, null,
                    (HarmonicWhite with { A = 0 }) * 0.75f * intensityRamp,
                    0f, origin, pointScale * 0.35f, SpriteEffects.None, 0f);
            }

            // Brightest bloom at blade tip
            Vector2 tipScreen = tipPos - Main.screenPosition;
            float tipScale = 0.3f * resonance * pulse * intensityRamp;
            sb.Draw(bloomTex, tipScreen, null,
                (DeepResonance with { A = 0 }) * 0.3f * intensityRamp,
                0f, origin, tipScale * 2.2f, SpriteEffects.None, 0f);
            sb.Draw(bloomTex, tipScreen, null,
                (FrequencyPulse with { A = 0 }) * 0.5f * intensityRamp,
                0f, origin, tipScale * 1.5f, SpriteEffects.None, 0f);
            sb.Draw(bloomTex, tipScreen, null,
                (ResonantSilver with { A = 0 }) * 0.65f * intensityRamp,
                0f, origin, tipScale * 0.9f, SpriteEffects.None, 0f);
            sb.Draw(bloomTex, tipScreen, null,
                (HarmonicWhite with { A = 0 }) * 0.8f * intensityRamp,
                0f, origin, tipScale * 0.4f, SpriteEffects.None, 0f);
        }

        // ----------- SWING FRAME VFX -----------

        /// <summary>
        /// Per-frame swing effects — precision sparks along the blade edge.
        /// Unlike EternalMoon's broad crescent dust, these are tight silver sparks
        /// in a "tuning fork" vibration pattern. Sparse but precise.
        /// </summary>
        public static void SwingFrameEffects(Vector2 ownerCenter, Vector2 tipPos,
            Vector2 swordDirection, int comboStep, int timer)
        {
            // Sharp silver sparks along blade (1-2 per frame, tight perpendicular)
            int sparkCount = 1 + (comboStep > 2 ? 1 : 0);
            for (int i = 0; i < sparkCount; i++)
            {
                float bladeT = Main.rand.NextFloat(0.5f, 1f);
                Vector2 sparkPos = Vector2.Lerp(ownerCenter, tipPos, bladeT);
                Vector2 perp = new Vector2(-swordDirection.Y, swordDirection.X);
                sparkPos += perp * Main.rand.NextFloat(-3f, 3f);

                Color sparkColor = GetResonanceColor(bladeT, comboStep);
                Dust d = Dust.NewDustPerfect(sparkPos, DustID.MagicMirror,
                    -swordDirection * Main.rand.NextFloat(2f, 4f), 0, sparkColor, 1.3f);
                d.noGravity = true;
            }

            // Contrasting silver sparkle every 3 frames
            if (timer % 3 == 0)
            {
                Vector2 sparklePos = Vector2.Lerp(ownerCenter, tipPos, Main.rand.NextFloat(0.6f, 1f));
                MoonlightVFXLibrary.SpawnContrastSparkle(sparklePos, -swordDirection);
            }

            // Music notes from tip every 5 frames (smaller, faster — precision feel)
            if (timer % 5 == 0)
            {
                MoonlightVFXLibrary.SpawnMusicNotes(tipPos, 1, 4f, 0.65f, 0.85f, 25);
            }
        }

        // ----------- IMPACT VFX -----------

        /// <summary>
        /// On-hit impact — resonant shockwave with tuning-fork vibration pattern.
        /// Unlike EternalMoon's crescent shockwave, this creates perpendicular spark lines.
        /// </summary>
        public static void OnHitImpact(Vector2 hitPos, int comboStep, bool isCrit)
        {
            // Base moonlight impact (shared library)
            MoonlightVFXLibrary.MeleeImpact(hitPos, comboStep);

            // UNIQUE: Resonant ripple rings (tight tuning-fork pattern)
            for (int i = 0; i < 2; i++)
            {
                Color rippleColor = Color.Lerp(ResonantSilver, FrequencyPulse, i * 0.5f);
                CustomParticles.HaloRing(hitPos, rippleColor,
                    0.25f + i * 0.12f + comboStep * 0.05f, 15 + i * 5);
            }

            // UNIQUE: Frequency burst — perpendicular spark lines (vibration visual)
            float hitAngle = Main.rand.NextFloat(MathHelper.TwoPi);
            for (int side = -1; side <= 1; side += 2)
            {
                Vector2 lineDir = (hitAngle + side * MathHelper.PiOver2).ToRotationVector2();
                for (int j = 0; j < 3; j++)
                {
                    Vector2 sparkPos = hitPos + lineDir * (10f + j * 8f);
                    Color sparkCol = GetResonanceColor((float)j / 3f, comboStep);
                    Dust d = Dust.NewDustPerfect(sparkPos, DustID.Enchanted_Gold,
                        lineDir * (2f + j), 0, sparkCol, 1.1f);
                    d.noGravity = true;
                }
            }

            // Crit: Harmonic resonance burst with god rays
            if (isCrit)
            {
                // 4-layer bloom flare cascade
                CustomParticles.GenericFlare(hitPos, DeepResonance, 0.8f, 22);
                CustomParticles.GenericFlare(hitPos, FrequencyPulse, 0.6f, 20);
                CustomParticles.GenericFlare(hitPos, ResonantSilver, 0.45f, 18);
                CustomParticles.GenericFlare(hitPos, HarmonicWhite, 0.3f, 15);

                // God ray burst on crits — starburst pattern
                GodRaySystem.CreateBurst(hitPos, MoonlightVFXLibrary.IceBlue,
                    rayCount: 5, radius: 35f, duration: 18, GodRaySystem.GodRayStyle.Explosion,
                    secondaryColor: FrequencyPulse);
            }
        }

        // ----------- CRESCENDO FINISHER -----------

        /// <summary>
        /// Phase 3 finisher — Moonlit Crescendo slam with resonant frequency cascade.
        /// God rays, screen distortion, expanding resonance rings.
        /// </summary>
        public static void CrescendoFinisherVFX(Vector2 pos)
        {
            // Use shared finisher as base
            MoonlightVFXLibrary.FinisherSlam(pos, 1.3f);

            // UNIQUE: Resonant frequency cascade — 5 expanding concentric rings
            for (int i = 0; i < 5; i++)
            {
                Color ringColor = Color.Lerp(DeepResonance, HarmonicWhite, i / 5f);
                CustomParticles.HaloRing(pos, ringColor, 0.3f + i * 0.15f, 12 + i * 4);
            }

            // UNIQUE: Music note starburst (6 notes spiraling outward)
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 noteOffset = angle.ToRotationVector2() * 6f;
                MoonlightVFXLibrary.SpawnMusicNotes(pos + noteOffset, 1, 5f, 0.85f, 1.05f, 35);
            }

            // God ray burst — 6 rays, starburst pattern
            GodRaySystem.CreateBurst(pos, MoonlightVFXLibrary.IceBlue,
                rayCount: 6, radius: 50f, duration: 25, GodRaySystem.GodRayStyle.Explosion,
                secondaryColor: MoonlightVFXLibrary.Violet);

            // Screen distortion ripple
            if (AdaptiveQualityManager.Instance?.CurrentQuality >= AdaptiveQualityManager.QualityLevel.Medium)
            {
                ScreenDistortionManager.TriggerRipple(pos, FrequencyPulse, 0.7f, 20);
                MagnumScreenEffects.AddScreenShake(5f);
            }
        }

        // ----------- TRAIL FUNCTIONS -----------

        /// <summary>
        /// Wave projectile trail color — silver-dominant with purple shimmer.
        /// Sharp and precise, unlike EternalMoon's flowing moonlight.
        /// </summary>
        public static Color WaveTrailColor(float progress)
        {
            Color c = Color.Lerp(ResonantSilver, FrequencyPulse,
                progress * 0.6f + MathF.Sin(progress * MathHelper.TwoPi * 2f) * 0.2f);
            return (c * (1f - progress * 0.65f)) with { A = 0 };
        }

        /// <summary>
        /// Wave trail width — sharp leading edge with clean quadratic taper.
        /// Precision feel: thin and surgical, opposite of EternalMoon's flowing crescent.
        /// </summary>
        public static float WaveTrailWidth(float progress)
        {
            float sharp = 1f - progress;
            return sharp * sharp * 18f;
        }

        /// <summary>
        /// Resonant lighting — pulsing at higher frequency than EternalMoon's slow moonlight.
        /// </summary>
        public static void AddResonantLight(Vector2 worldPos, float intensity = 0.7f)
        {
            float pulse = 0.9f + MathF.Sin(Main.GlobalTimeWrappedHourly * 10f) * 0.1f;
            Lighting.AddLight(worldPos, MoonlightVFXLibrary.IceBlue.ToVector3() * intensity * pulse);
        }
    }
}
