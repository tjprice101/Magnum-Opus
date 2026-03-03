using System;

namespace MagnumOpus.Content.FoundationWeapons.LaserFoundation
{
    /// <summary>
    /// Self-contained easing functions for LaserFoundation.
    /// These are standard animation easing curves used for beam width transitions,
    /// opacity fades, and visual polish.
    /// 
    /// All methods take a float progress (0..1) and return the eased value (0..1).
    /// Reference: https://easings.net/
    /// </summary>
    internal static class LFEasings
    {
        // ---- SINE ----

        public static float EaseInSine(float t)
            => 1f - MathF.Cos(t * MathF.PI / 2f);

        public static float EaseOutSine(float t)
            => MathF.Sin(t * MathF.PI / 2f);

        public static float EaseInOutSine(float t)
            => -(MathF.Cos(MathF.PI * t) - 1f) / 2f;

        // ---- QUADRATIC ----

        public static float EaseInQuad(float t)
            => t * t;

        public static float EaseOutQuad(float t)
            => 1f - (1f - t) * (1f - t);

        public static float EaseInOutQuad(float t)
            => t < 0.5f ? 2f * t * t : 1f - MathF.Pow(-2f * t + 2f, 2f) / 2f;

        // ---- CUBIC ----

        public static float EaseInCubic(float t)
            => t * t * t;

        public static float EaseOutCubic(float t)
            => 1f - MathF.Pow(1f - t, 3f);

        public static float EaseInOutCubic(float t)
            => t < 0.5f ? 4f * t * t * t : 1f - MathF.Pow(-2f * t + 2f, 3f) / 2f;

        // ---- CIRCULAR ----

        public static float EaseOutCirc(float t)
            => MathF.Sqrt(1f - MathF.Pow(t - 1f, 2f));

        public static float EaseInCirc(float t)
            => 1f - MathF.Sqrt(1f - MathF.Pow(t, 2f));

        // ---- EXPONENTIAL ----

        public static float EaseOutExpo(float t)
            => t >= 1f ? 1f : 1f - MathF.Pow(2f, -10f * t);

        public static float EaseInExpo(float t)
            => t <= 0f ? 0f : MathF.Pow(2f, 10f * t - 10f);
    }
}
