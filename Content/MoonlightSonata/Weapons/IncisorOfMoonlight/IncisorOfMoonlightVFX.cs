using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Content.MoonlightSonata;

namespace MagnumOpus.Content.MoonlightSonata.VFX.IncisorOfMoonlight
{
    /// <summary>
    /// Unique VFX for Incisor of Moonlight — the Resonant melee weapon.
    /// Theme: Surgical precision, resonant frequency pulses, silver-edge cuts.
    /// Each phase builds resonance — visual intensity escalates dramatically.
    /// </summary>
    public static class IncisorOfMoonlightVFX
    {
        // === UNIQUE COLOR ACCENTS (public for wave projectile access) ===
        public static readonly Color ResonantSilver = new Color(230, 235, 255);
        public static readonly Color FrequencyPulse = new Color(170, 140, 255);
        public static readonly Color HarmonicWhite = new Color(255, 250, 245);
        public static readonly Color DeepResonance = new Color(90, 50, 160);

        /// <summary>
        /// Resonant edge bloom — sharp, clean line of light along the blade.
        /// Unlike EternalMoon's crescent bloom, this is a tight, precise edge.
        /// </summary>
        public static void DrawResonantEdgeBloom(SpriteBatch sb, Vector2 pommelPos, Vector2 tipPos, int comboStep, float progression)
        {
            if (sb == null) return;

            float intensityRamp = MathHelper.Clamp(progression * 4f, 0f, 1f) * MathHelper.Clamp((1f - progression) * 3f, 0f, 1f);
            if (intensityRamp < 0.05f) return;

            var bloomTex = MagnumTextureRegistry.GetSoftGlow();
            if (bloomTex == null) return;
            Vector2 origin = bloomTex.Size() * 0.5f;

            // Resonance intensifies with combo step
            float resonanceIntensity = 0.6f + comboStep * 0.15f;
            float pulse = 1f + MathF.Sin(Main.GlobalTimeWrappedHourly * 12f) * 0.08f; // Faster pulse = higher frequency

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // 4-layer Calamity bloom stack along the blade edge
            int pointCount = 4 + comboStep;
            for (int i = 0; i < pointCount; i++)
            {
                float t = (float)(i + 1) / (pointCount + 1);
                Vector2 bladePos = Vector2.Lerp(pommelPos, tipPos, t) - Main.screenPosition;
                float pointScale = (0.15f + t * 0.2f) * resonanceIntensity * pulse * intensityRamp;

                // Layer 1 (Outer): Deep resonance glow
                sb.Draw(bloomTex, bladePos, null, DeepResonance with { A = 0 } * 0.30f * intensityRamp, 0f, origin, pointScale * 2.0f, SpriteEffects.None, 0f);
                // Layer 2 (Mid): Frequency pulse
                sb.Draw(bloomTex, bladePos, null, FrequencyPulse with { A = 0 } * 0.50f * intensityRamp, 0f, origin, pointScale * 1.4f, SpriteEffects.None, 0f);
                // Layer 3 (Inner): Resonant silver
                sb.Draw(bloomTex, bladePos, null, ResonantSilver with { A = 0 } * 0.70f * intensityRamp, 0f, origin, pointScale * 0.9f, SpriteEffects.None, 0f);
                // Layer 4 (Core): Harmonic white-hot center
                sb.Draw(bloomTex, bladePos, null, HarmonicWhite with { A = 0 } * 0.85f * intensityRamp, 0f, origin, pointScale * 0.4f, SpriteEffects.None, 0f);
            }

            // 4-layer tip bloom (brightest point on the blade)
            Vector2 tipScreen = tipPos - Main.screenPosition;
            float tipScale = 0.35f * resonanceIntensity * pulse * intensityRamp;
            sb.Draw(bloomTex, tipScreen, null, DeepResonance with { A = 0 } * 0.30f * intensityRamp, 0f, origin, tipScale * 2.0f, SpriteEffects.None, 0f);
            sb.Draw(bloomTex, tipScreen, null, FrequencyPulse with { A = 0 } * 0.50f * intensityRamp, 0f, origin, tipScale * 1.4f, SpriteEffects.None, 0f);
            sb.Draw(bloomTex, tipScreen, null, ResonantSilver with { A = 0 } * 0.70f * intensityRamp, 0f, origin, tipScale * 0.9f, SpriteEffects.None, 0f);
            sb.Draw(bloomTex, tipScreen, null, HarmonicWhite with { A = 0 } * 0.85f * intensityRamp, 0f, origin, tipScale * 0.4f, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Resonant frequency color — shifts with combo step to show building resonance.
        /// Phase 1: Cool silver. Phase 2: Warming purple. Phase 3: Hot violet. Phase 4: Blazing white.
        /// </summary>
        public static Color GetResonanceColor(float progress, int comboStep)
        {
            float resonance = MathHelper.Clamp(comboStep / 3f, 0f, 1f);

            Color cold = Color.Lerp(ResonantSilver, MoonlightVFXLibrary.IceBlue, progress);
            Color hot = Color.Lerp(FrequencyPulse, HarmonicWhite, progress);

            return Color.Lerp(cold, hot, resonance);
        }

        /// <summary>
        /// Per-frame swing effects — precision sparks along the blade edge.
        /// Unlike EternalMoon's broad crescent dust, these are tight silver sparks.
        /// </summary>
        public static void SwingFrameEffects(Vector2 ownerCenter, Vector2 tipPos, Vector2 swordDirection, int comboStep, int timer)
        {
            // Sharp silver sparks along blade (1-2 per frame, tight)
            for (int i = 0; i < 1 + (comboStep > 2 ? 1 : 0); i++)
            {
                float bladeT = Main.rand.NextFloat(0.5f, 1f);
                Vector2 sparkPos = Vector2.Lerp(ownerCenter, tipPos, bladeT);
                // Tight perpendicular offset (precision feel)
                Vector2 perp = new Vector2(-swordDirection.Y, swordDirection.X);
                sparkPos += perp * Main.rand.NextFloat(-3f, 3f);

                Color sparkColor = GetResonanceColor(bladeT, comboStep);
                Dust d = Dust.NewDustPerfect(sparkPos, DustID.MagicMirror, -swordDirection * Main.rand.NextFloat(2f, 4f), 0, sparkColor, 1.3f);
                d.noGravity = true;
            }

            // Resonance pulse ring at tip every 6 frames (frequency visualization)
            if (timer % 6 == 0)
            {
                CustomParticles.MoonlightHalo(tipPos, 0.2f + comboStep * 0.05f);
            }

            // Music notes from tip every 5 frames (smaller, faster — precision feel)
            if (timer % 5 == 0)
            {
                MoonlightVFXLibrary.SpawnMusicNotes(tipPos, 1, 4f, 0.65f, 0.85f, 25);
            }
        }

        /// <summary>
        /// Impact VFX — resonant shockwave that ripples outward.
        /// Unlike EternalMoon's constellation burst, this is a tuning-fork vibration pattern.
        /// </summary>
        public static void OnHitImpact(Vector2 hitPos, int comboStep, bool isCrit)
        {
            // Base impact
            MoonlightVFXLibrary.MeleeImpact(hitPos, comboStep);

            // UNIQUE: Resonant ripple rings (tuning fork pattern — 2 tight rings)
            for (int i = 0; i < 2; i++)
            {
                Color rippleColor = Color.Lerp(ResonantSilver, FrequencyPulse, i * 0.5f);
                CustomParticles.HaloRing(hitPos, rippleColor, 0.25f + i * 0.12f + comboStep * 0.05f, 15 + i * 5);
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
                    Dust d = Dust.NewDustPerfect(sparkPos, DustID.Enchanted_Gold, lineDir * (2f + j), 0, sparkCol, 1.1f);
                    d.noGravity = true;
                }
            }

            // Crit: Harmonic resonance burst — layered bloom cascade
            if (isCrit)
            {
                // 4-layer bloom flare cascade (outer → core)
                CustomParticles.GenericFlare(hitPos, DeepResonance, 0.8f, 22);
                CustomParticles.GenericFlare(hitPos, FrequencyPulse, 0.6f, 20);
                CustomParticles.GenericFlare(hitPos, ResonantSilver, 0.45f, 18);
                CustomParticles.GenericFlare(hitPos, HarmonicWhite, 0.3f, 15);
                CustomParticles.MoonlightCrescendo(hitPos, 0.8f + comboStep * 0.2f);
            }
        }

        /// <summary>
        /// Phase 4 finisher — MoonlitCrescendo slam with resonant frequency cascade.
        /// </summary>
        public static void CrescendoFinisherVFX(Vector2 pos)
        {
            MoonlightVFXLibrary.FinisherSlam(pos, 1.3f);

            // Central resonance bloom flash — layered flare cascade
            CustomParticles.GenericFlare(pos, DeepResonance, 0.9f, 25);
            CustomParticles.GenericFlare(pos, FrequencyPulse, 0.7f, 22);
            CustomParticles.GenericFlare(pos, ResonantSilver, 0.5f, 18);
            CustomParticles.GenericFlare(pos, HarmonicWhite, 0.35f, 14);

            // UNIQUE: Resonant frequency cascade — concentric rings in rapid succession
            for (int i = 0; i < 5; i++)
            {
                Color ringColor = Color.Lerp(DeepResonance, HarmonicWhite, i / 5f);
                CustomParticles.HaloRing(pos, ringColor, 0.3f + i * 0.15f, 12 + i * 4);
            }

            // Music note burst (6 notes, escalated scale)
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                MoonlightVFXLibrary.SpawnMusicNotes(pos + noteVel * 3f, 1, 5f, 0.85f, 1.05f, 35);
            }
        }

        /// <summary>
        /// Wave projectile trail color — silver-dominant with purple shimmer.
        /// </summary>
        public static Color WaveTrailColor(float progress)
        {
            Color c = Color.Lerp(ResonantSilver, FrequencyPulse, progress * 0.6f + MathF.Sin(progress * MathHelper.TwoPi * 2f) * 0.2f);
            return c with { A = 0 } * (1f - progress * 0.65f);
        }

        /// <summary>
        /// Wave trail width — sharp leading edge, clean taper.
        /// </summary>
        public static float WaveTrailWidth(float progress)
        {
            float sharp = 1f - progress;
            return sharp * sharp * 18f; // Quadratic falloff for precision feel
        }

        public static void AddResonantLight(Vector2 worldPos, float intensity = 0.7f)
        {
            float pulse = 0.9f + MathF.Sin(Main.GlobalTimeWrappedHourly * 10f) * 0.1f;
            Lighting.AddLight(worldPos, MoonlightVFXLibrary.IceBlue.ToVector3() * intensity * pulse);
        }
    }
}
