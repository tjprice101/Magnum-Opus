using Microsoft.Xna.Framework;
using System;

namespace MagnumOpus.Common.Systems.VFX.Core
{
    /// <summary>
    /// Width tapering functions for beams and trails.
    /// 
    /// Taper types:
    /// - Linear: Constant rate of change
    /// - Quadratic: Smooth acceleration/deceleration
    /// - Exponential: Dramatic narrowing
    /// - Custom curves: Artist-controlled shape
    /// 
    /// Visual impact:
    /// - Creates depth perception
    /// - Suggests energy dissipation
    /// - Guides eye toward impact point
    /// </summary>
    public static class TaperCurves
    {
        /// <summary>
        /// Delegate for taper functions.
        /// </summary>
        /// <param name="t">Progress along beam (0 = start, 1 = end)</param>
        /// <param name="startWidth">Width at beam origin</param>
        /// <param name="endWidth">Width at beam end</param>
        /// <returns>Width at position t</returns>
        public delegate float TaperFunction(float t, float startWidth, float endWidth);

        #region Standard Taper Curves

        /// <summary>
        /// Linear taper - constant rate of width change.
        /// </summary>
        public static float Linear(float t, float startWidth, float endWidth)
        {
            return MathHelper.Lerp(startWidth, endWidth, t);
        }

        /// <summary>
        /// Ease out - fast taper at start, slow at end.
        /// Good for energy beams that start intense.
        /// </summary>
        public static float EaseOut(float t, float startWidth, float endWidth)
        {
            float factor = 1f - (float)Math.Pow(1f - t, 2);
            return MathHelper.Lerp(startWidth, endWidth, factor);
        }

        /// <summary>
        /// Ease in - slow taper at start, fast at end.
        /// Good for beams that maintain intensity then drop off.
        /// </summary>
        public static float EaseIn(float t, float startWidth, float endWidth)
        {
            float factor = (float)Math.Pow(t, 2);
            return MathHelper.Lerp(startWidth, endWidth, factor);
        }

        /// <summary>
        /// Smooth step - ease in-out, S-curve taper.
        /// Natural feeling transition.
        /// </summary>
        public static float SmoothStep(float t, float startWidth, float endWidth)
        {
            float factor = t * t * (3f - 2f * t);
            return MathHelper.Lerp(startWidth, endWidth, factor);
        }

        /// <summary>
        /// Exponential taper - dramatic narrowing.
        /// </summary>
        /// <param name="exponent">Higher = more dramatic (default 2)</param>
        public static float Exponential(float t, float startWidth, float endWidth, float exponent = 2f)
        {
            float factor = (float)Math.Pow(t, exponent);
            return MathHelper.Lerp(startWidth, endWidth, factor);
        }

        /// <summary>
        /// Bulge taper - thicker in the middle, thinner at ends.
        /// Good for energy pulses traveling along beams.
        /// </summary>
        /// <param name="bulgeFactor">How much wider the middle is (1.5 = 50% wider)</param>
        public static float Bulge(float t, float startWidth, float endWidth, float bulgeFactor = 1.5f)
        {
            // Parabola: peaks at t=0.5
            float bulge = 1f - 4f * (float)Math.Pow(t - 0.5f, 2);
            float baseWidth = MathHelper.Lerp(startWidth, endWidth, t);
            return baseWidth * (1f + bulge * (bulgeFactor - 1f));
        }

        /// <summary>
        /// Sinusoidal wave taper - oscillating width along beam.
        /// Good for energy fluctuations or plasma effects.
        /// </summary>
        /// <param name="frequency">Number of waves along beam</param>
        /// <param name="amplitude">Wave amplitude (0.2 = 20% variation)</param>
        public static float Wave(float t, float startWidth, float endWidth, 
            float frequency = 3f, float amplitude = 0.2f)
        {
            float baseWidth = MathHelper.Lerp(startWidth, endWidth, t);
            float wave = (float)Math.Sin(t * MathHelper.TwoPi * frequency) * amplitude;
            return baseWidth * (1f + wave);
        }

        /// <summary>
        /// Bezier-based taper - custom curve shape.
        /// Control points define the taper profile.
        /// </summary>
        /// <param name="controlPoint1">First control point (0-1, affects early curve)</param>
        /// <param name="controlPoint2">Second control point (0-1, affects late curve)</param>
        public static float BezierTaper(float t, float startWidth, float endWidth,
            float controlPoint1 = 0.8f, float controlPoint2 = 0.2f)
        {
            float u = 1f - t;
            float factor = u * u * u * 0f +
                           3f * u * u * t * controlPoint1 +
                           3f * u * t * t * controlPoint2 +
                           t * t * t * 1f;

            return MathHelper.Lerp(startWidth, endWidth, factor);
        }

        #endregion

        #region FargosSoulsDLC-Style Tapers

        /// <summary>
        /// InverseLerp with bump - ramps up early, holds, tapers at end.
        /// Classic FargosSoulsDLC trail pattern.
        /// </summary>
        public static float InverseLerpBump(float t, float startWidth, float endWidth,
            float rampStart = 0.06f, float rampEnd = 0.27f,
            float fadeStart = 0.72f, float fadeEnd = 0.9f)
        {
            float ramp = VFXUtilities.InverseLerp(rampStart, rampEnd, t);
            float fade = VFXUtilities.InverseLerp(fadeEnd, fadeStart, t);
            float factor = ramp * fade;
            return MathHelper.Lerp(endWidth, startWidth, factor);
        }

        /// <summary>
        /// Constant width with fade at end.
        /// Good for laser beams that maintain size until termination.
        /// </summary>
        public static float ConstantWithFade(float t, float width, float fadeStart = 0.8f)
        {
            if (t < fadeStart) return width;
            float fade = (t - fadeStart) / (1f - fadeStart);
            return width * (1f - fade);
        }

        /// <summary>
        /// Quadratic bump - thin at ends, thick in middle.
        /// Good for energy bursts and pulse effects.
        /// </summary>
        public static float QuadraticBump(float t, float minWidth, float maxWidth)
        {
            // Sin gives 0→1→0 over 0→π
            float factor = (float)Math.Sin(t * MathHelper.Pi);
            return MathHelper.Lerp(minWidth, maxWidth, factor);
        }

        #endregion

        #region Dynamic Width Modulation

        /// <summary>
        /// Create a pulsing width effect for animated beams.
        /// </summary>
        /// <param name="baseWidth">Static width from taper function</param>
        /// <param name="progress">Position along beam (0-1)</param>
        /// <param name="time">Animation time</param>
        /// <param name="pulseSpeed">Speed of pulse travel</param>
        /// <param name="pulseAmplitude">Pulse size (0.3 = 30% variation)</param>
        public static float ApplyPulse(float baseWidth, float progress, float time,
            float pulseSpeed = 5f, float pulseAmplitude = 0.3f)
        {
            // Traveling wave along beam
            float pulse = (float)Math.Sin(progress * MathHelper.TwoPi * 2f - time * pulseSpeed);
            pulse = pulse * 0.5f + 0.5f; // 0-1 range
            pulse = (float)Math.Pow(pulse, 2); // Sharpen peaks

            float dynamicScale = 1f + pulse * pulseAmplitude;
            return baseWidth * dynamicScale;
        }

        /// <summary>
        /// Create a jittering width effect for unstable energy.
        /// </summary>
        public static float ApplyJitter(float baseWidth, float jitterAmount = 0.1f)
        {
            float jitter = (float)(Terraria.Main.rand.NextDouble() * 2 - 1) * jitterAmount;
            return baseWidth * (1f + jitter);
        }

        /// <summary>
        /// Create a breathing effect for organic/magical beams.
        /// </summary>
        public static float ApplyBreathing(float baseWidth, float time, 
            float breathSpeed = 2f, float breathAmount = 0.15f)
        {
            float breath = (float)Math.Sin(time * breathSpeed) * 0.5f + 0.5f;
            breath = (float)Math.Pow(breath, 0.5f); // Softer curve
            return baseWidth * (1f - breathAmount + breath * breathAmount * 2f);
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Create a taper function with preset parameters.
        /// </summary>
        public static TaperFunction CreateTaper(TaperType type, float startWidth, float endWidth)
        {
            return type switch
            {
                TaperType.Linear => (t, s, e) => Linear(t, startWidth, endWidth),
                TaperType.EaseIn => (t, s, e) => EaseIn(t, startWidth, endWidth),
                TaperType.EaseOut => (t, s, e) => EaseOut(t, startWidth, endWidth),
                TaperType.SmoothStep => (t, s, e) => SmoothStep(t, startWidth, endWidth),
                TaperType.Exponential => (t, s, e) => Exponential(t, startWidth, endWidth),
                TaperType.Bulge => (t, s, e) => Bulge(t, startWidth, endWidth),
                TaperType.Wave => (t, s, e) => Wave(t, startWidth, endWidth),
                _ => (t, s, e) => Linear(t, startWidth, endWidth)
            };
        }

        /// <summary>
        /// Create a simple width function for trail rendering.
        /// </summary>
        public static Func<float, float> SimpleWidth(float startWidth, float endWidth, TaperType type = TaperType.Linear)
        {
            return type switch
            {
                TaperType.Linear => t => Linear(t, startWidth, endWidth),
                TaperType.EaseIn => t => EaseIn(t, startWidth, endWidth),
                TaperType.EaseOut => t => EaseOut(t, startWidth, endWidth),
                TaperType.SmoothStep => t => SmoothStep(t, startWidth, endWidth),
                TaperType.Exponential => t => Exponential(t, startWidth, endWidth),
                TaperType.Bulge => t => Bulge(t, startWidth, endWidth),
                TaperType.Wave => t => Wave(t, startWidth, endWidth),
                _ => t => Linear(t, startWidth, endWidth)
            };
        }

        #endregion
    }

    /// <summary>
    /// Predefined taper curve types.
    /// </summary>
    public enum TaperType
    {
        /// <summary>Constant rate of change.</summary>
        Linear,
        /// <summary>Slow start, fast end.</summary>
        EaseIn,
        /// <summary>Fast start, slow end.</summary>
        EaseOut,
        /// <summary>S-curve, smooth transitions.</summary>
        SmoothStep,
        /// <summary>Dramatic narrowing.</summary>
        Exponential,
        /// <summary>Thick in middle, thin at ends.</summary>
        Bulge,
        /// <summary>Oscillating width.</summary>
        Wave
    }
}
