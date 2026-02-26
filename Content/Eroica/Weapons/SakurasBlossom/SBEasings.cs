using System;

namespace MagnumOpus.Content.Eroica.Weapons.SakurasBlossom
{
    /// <summary>
    /// Sakura-specific easing curves for dust behavior, trail rendering, and VFX timing.
    /// Complements the generic SLPEasings with curves tuned for cherry blossom motion.
    /// </summary>
    internal static class SBEasings
    {
        // ═══════════════════════════════════════════════════════
        //  PETAL PHYSICS CURVES
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// Sinusoidal lateral flutter for floating petals.
        /// Returns a value in [-1, 1] representing perpendicular drift.
        /// Amplitude decays over lifetime for natural settling.
        /// </summary>
        public static float PetalDriftCurve(float t, float frequency = 2.5f)
        {
            float decay = 1f - t * t;
            return MathF.Sin(t * MathF.PI * frequency) * decay;
        }

        /// <summary>
        /// Petal scatter deceleration — fast start, smooth coastal drift.
        /// Ideal for petals launched from swing impacts.
        /// </summary>
        public static float PetalScatterDecay(float t)
            => 1f - MathF.Pow(1f - t, 2.5f) * (1f - t);

        // ═══════════════════════════════════════════════════════
        //  BLOOM & EXPANSION CURVES
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// Fast bloom start, gentle settle — for expanding rings and bloom flares.
        /// Overshoots slightly past 1.0 then settles, like a flower opening.
        /// </summary>
        public static float BloomUnfurl(float t)
        {
            if (t < 0.6f)
                return MathF.Pow(t / 0.6f, 0.4f) * 1.08f;
            return 1.08f - 0.08f * ((t - 0.6f) / 0.4f);
        }

        /// <summary>
        /// Pollen rise curve — initial impulse, then gravity resistance plateau.
        /// Returns a [0, 1] upward velocity multiplier.
        /// </summary>
        public static float PollenRise(float t)
        {
            float upPhase = MathF.Pow(1f - t, 1.5f);
            float horizontalDrift = MathF.Sin(t * MathF.PI * 3f) * 0.15f * (1f - t);
            return upPhase + horizontalDrift;
        }

        // ═══════════════════════════════════════════════════════
        //  TRAIL & FADE CURVES
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// Swing trail opacity falloff — strong at blade tip (completionRatio=0),
        /// fades toward the base (completionRatio=1).
        /// </summary>
        public static float SwingTrailFade(float completionRatio)
            => MathF.Pow(1f - completionRatio, 1.8f);

        /// <summary>
        /// Ember cooling curve — fast bright start, slow crimson tail.
        /// t=0 → 1.0 (white-hot), t=1 → 0.0 (extinguished).
        /// </summary>
        public static float EmberCool(float t)
            => MathF.Pow(1f - t, 0.6f);

        // ═══════════════════════════════════════════════════════
        //  GENERIC HELPERS
        // ═══════════════════════════════════════════════════════

        /// <summary>Smooth step: 3t² - 2t³ for [0,1] range.</summary>
        public static float SmoothStep(float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            return t * t * (3f - 2f * t);
        }

        /// <summary>Inverse smooth step — fast start, slow end.</summary>
        public static float InverseSmoothStep(float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            return 1f - (1f - t) * (1f - t) * (3f - 2f * (1f - t));
        }

        /// <summary>Breathing pulse: sin-based oscillation around 1.0.</summary>
        public static float BreathingPulse(float time, float frequency = 0.1f, float amplitude = 0.08f)
            => 1f + MathF.Sin(time * frequency) * amplitude;

        /// <summary>Scale twinkling: faster irregular pulsation for sparkles.</summary>
        public static float Twinkle(float time, float phaseOffset = 0f)
            => 0.7f + 0.3f * MathF.Abs(MathF.Sin(time * 0.25f + phaseOffset));
    }
}
