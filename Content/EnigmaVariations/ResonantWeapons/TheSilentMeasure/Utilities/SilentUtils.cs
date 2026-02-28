using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheSilentMeasure.Utilities
{
    /// <summary>
    /// Self-contained utility class for TheSilentMeasure.
    /// Palette, easings, animation curves, SpriteBatch helpers.
    /// Themed around silence, questions, and measurement.
    /// </summary>
    public static class SilentUtils
    {
        // =====================================================================
        //  PALETTE — The silence between questions: void → depth → violet → emerald → bright → answer
        // =====================================================================

        public static readonly Color[] SilentPalette = new Color[]
        {
            new Color(3, 1, 12),        // [0] Absolute Silence — the void before the question
            new Color(25, 12, 75),       // [1] Hushed Depth — deep shrouded murmur
            new Color(110, 35, 170),     // [2] Question Violet — the question forming
            new Color(25, 185, 80),      // [3] Enigma Emerald — the measured unknown
            new Color(80, 240, 130),     // [4] Bright Question — revelation flash
            new Color(215, 255, 230),    // [5] Answer White — truth measured and found
        };

        public static Color AbsoluteSilence => SilentPalette[0];
        public static Color HushedDepth => SilentPalette[1];
        public static Color QuestionViolet => SilentPalette[2];
        public static Color EnigmaEmerald => SilentPalette[3];
        public static Color BrightQuestion => SilentPalette[4];
        public static Color AnswerWhite => SilentPalette[5];

        // =====================================================================
        //  EASINGS
        // =====================================================================

        public static float SineIn(float t) => 1f - MathF.Cos(t * MathHelper.PiOver2);
        public static float SineOut(float t) => MathF.Sin(t * MathHelper.PiOver2);
        public static float SineInOut(float t) => -(MathF.Cos(MathHelper.Pi * t) - 1f) / 2f;
        public static float SineBump(float t) => MathF.Sin(MathHelper.Pi * t);
        public static float PolyIn(float t) => t * t * t;
        public static float PolyOut(float t) { float inv = 1f - t; return 1f - inv * inv * inv; }
        public static float PolyInOut(float t) => t < 0.5f ? 4f * t * t * t : 1f - MathF.Pow(-2f * t + 2f, 3f) / 2f;
        public static float ExpIn(float t) => t <= 0f ? 0f : MathF.Pow(2f, 10f * t - 10f);
        public static float ExpOut(float t) => t >= 1f ? 1f : 1f - MathF.Pow(2f, -10f * t);

        // =====================================================================
        //  CURVE SEGMENT & PIECEWISE ANIMATION
        // =====================================================================

        public readonly struct CurveSegment
        {
            public readonly Func<float, float> Easing;
            public readonly float AnimationStart;
            public readonly float Height;
            public readonly float Elevation;

            public CurveSegment(Func<float, float> easing, float animationStart, float height, float elevation = 0f)
            {
                Easing = easing;
                AnimationStart = animationStart;
                Height = height;
                Elevation = elevation;
            }
        }

        public static float PiecewiseAnimation(float progress, params CurveSegment[] segments)
        {
            float value = 0f;
            for (int i = 0; i < segments.Length; i++)
            {
                float start = segments[i].AnimationStart;
                float end = i + 1 < segments.Length ? segments[i + 1].AnimationStart : 1f;
                if (progress < start) break;
                float segProgress = MathHelper.Clamp((progress - start) / (end - start), 0f, 1f);
                value = segments[i].Elevation + segments[i].Height * segments[i].Easing(segProgress);
            }
            return value;
        }

        // =====================================================================
        //  COLOR HELPERS
        // =====================================================================

        public static Color MulticolorLerp(float t, params Color[] colors)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            float scaled = t * (colors.Length - 1);
            int index = (int)scaled;
            if (index >= colors.Length - 1) return colors[^1];
            float frac = scaled - index;
            return Color.Lerp(colors[index], colors[index + 1], frac);
        }

        /// <summary>Gets a color along the silent gradient (0=void → 1=answer).</summary>
        public static Color GetSilentGradient(float t) => MulticolorLerp(t, SilentPalette);

        // =====================================================================
        //  SPRITEBATCH HELPERS
        // =====================================================================

        public static void EnterShaderRegion(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public static void EnterAdditiveShaderRegion(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearWrap,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public static void ExitShaderRegion(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }
}
