using System;
using Microsoft.Xna.Framework;

namespace MagnumOpus.Content.FoundationWeapons.FoundationIncisorOrbs
{
    /// <summary>
    /// Self-contained utility helpers for FoundationIncisorOrbs.
    /// 1-to-1 copy of IncisorUtils — easing functions, piecewise animation curves,
    /// color palette, and MulticolorLerp.
    ///
    /// These are the exact same systems that drive the Incisor of Moonlight's
    /// swing animation (Grave → Allegro → Diminuendo curve segments).
    /// </summary>
    public static class IOFUtils
    {
        // =====================================================================
        // FOUNDATION PALETTE — mirrors IncisorUtils.IncisorPalette
        // Deep Resonance through Harmonic White
        // Musical dynamics: Pianissimo → Sforzando
        // =====================================================================

        public static readonly Color[] FoundationPalette = new Color[]
        {
            new Color(90, 50, 160),      // Deep Resonance (Pianissimo)
            new Color(170, 140, 255),    // Frequency Pulse (Piano)
            new Color(230, 235, 255),    // Resonant Silver (Mezzo)
            new Color(135, 206, 250),    // Ice Blue Clarity (Forte)
            new Color(220, 230, 255),    // Crystal Edge (Fortissimo)
            new Color(235, 240, 255),    // Harmonic White (Sforzando)
        };

        // =====================================================================
        // EASING FUNCTIONS — identical to IncisorUtils
        // =====================================================================

        public delegate float EasingFunction(float amount, int degree);

        public static float LinearEasing(float amount, int degree) => amount;

        public static float SineInEasing(float amount, int degree)
            => 1f - (float)Math.Cos(amount * MathHelper.Pi / 2f);

        public static float SineOutEasing(float amount, int degree)
            => (float)Math.Sin(amount * MathHelper.Pi / 2f);

        public static float SineInOutEasing(float amount, int degree)
            => -((float)Math.Cos(amount * MathHelper.Pi) - 1) / 2f;

        public static float SineBumpEasing(float amount, int degree)
            => (float)Math.Sin(amount * MathHelper.Pi);

        public static float PolyInEasing(float amount, int degree)
            => (float)Math.Pow(amount, degree);

        public static float PolyOutEasing(float amount, int degree)
            => 1f - (float)Math.Pow(1f - amount, degree);

        public static float PolyInOutEasing(float amount, int degree) => amount < 0.5f
            ? (float)Math.Pow(2, degree - 1) * (float)Math.Pow(amount, degree)
            : 1f - (float)Math.Pow(-2 * amount + 2, degree) / 2f;

        public static float ExpInEasing(float amount, int degree)
            => amount == 0f ? 0f : (float)Math.Pow(2, 10f * amount - 10f);

        public static float ExpOutEasing(float amount, int degree)
            => amount == 1f ? 1f : 1f - (float)Math.Pow(2, -10f * amount);

        public static float CircInEasing(float amount, int degree)
            => 1f - (float)Math.Sqrt(1 - Math.Pow(amount, 2f));

        public static float CircOutEasing(float amount, int degree)
            => (float)Math.Sqrt(1 - Math.Pow(amount - 1f, 2f));

        // =====================================================================
        // CURVE SEGMENT — piecewise animation building block
        // Identical to IncisorUtils.CurveSegment
        // =====================================================================

        public struct CurveSegment
        {
            public EasingFunction easing;
            public float startingX;
            public float startingHeight;
            public float elevationShift;
            public int degree;

            public float EndingHeight => startingHeight + elevationShift;

            public CurveSegment(EasingFunction mode, float startX, float startHeight,
                float elevationShift, int degree = 1)
            {
                easing = mode;
                startingX = startX;
                startingHeight = startHeight;
                this.elevationShift = elevationShift;
                this.degree = degree;
            }
        }

        /// <summary>
        /// Evaluates a piecewise animation curve at the given progress (0–1).
        /// Each segment uses its own easing function, start height, and elevation shift.
        /// Identical to IncisorUtils.PiecewiseAnimation.
        /// </summary>
        public static float PiecewiseAnimation(float progress, params CurveSegment[] segments)
        {
            if (segments.Length == 0)
                return 0f;
            if (segments[0].startingX != 0)
                segments[0].startingX = 0;

            progress = MathHelper.Clamp(progress, 0f, 1f);
            float ratio = 0f;

            for (int i = 0; i <= segments.Length - 1; i++)
            {
                CurveSegment segment = segments[i];
                float startPoint = segment.startingX;
                float endPoint = 1f;

                if (progress < segment.startingX) continue;
                if (i < segments.Length - 1)
                {
                    if (segments[i + 1].startingX <= progress) continue;
                    endPoint = segments[i + 1].startingX;
                }

                float segmentLength = endPoint - startPoint;
                float segmentProgress = (progress - segment.startingX) / segmentLength;

                ratio = segment.startingHeight;
                if (segment.easing != null)
                    ratio += segment.easing(segmentProgress, segment.degree) * segment.elevationShift;
                else
                    ratio += LinearEasing(segmentProgress, segment.degree) * segment.elevationShift;
                break;
            }
            return ratio;
        }

        // =====================================================================
        // COLOR UTILITIES — identical to IncisorUtils
        // =====================================================================

        /// <summary>
        /// Smoothly lerps through an array of colors based on a 0–1 increment.
        /// Identical to IncisorUtils.MulticolorLerp.
        /// </summary>
        public static Color MulticolorLerp(float increment, params Color[] colors)
        {
            increment %= 0.999f;
            int currentColorIndex = (int)(increment * colors.Length);
            Color currentColor = colors[currentColorIndex];
            Color nextColor = colors[(currentColorIndex + 1) % colors.Length];
            return Color.Lerp(currentColor, nextColor, increment * colors.Length % 1f);
        }
    }
}
