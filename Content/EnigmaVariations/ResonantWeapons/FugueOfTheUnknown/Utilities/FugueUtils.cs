using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using MagnumOpus.Content.EnigmaVariations;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.FugueOfTheUnknown.Utilities
{
    /// <summary>
    /// Self-contained utility class for FugueOfTheUnknown.
    /// Palette, easings, animation curves, SpriteBatch helpers.
    /// </summary>
    public static class FugueUtils
    {
        // =====================================================================
        //  PALETTE — Polyphonic voices converging through mystery:
        //  shadow void → deep chorus → voice purple → echo teal → fugue cyan → harmonic white
        // =====================================================================

        public static readonly Color[] FuguePalette = new Color[]
        {
            new Color(10, 5, 22),       // [0] Shadow Void — the silence before a fugue begins
            new Color(55, 15, 120),     // [1] Deep Chorus — overlapping voices in the dark
            new Color(155, 55, 205),    // [2] Voice Purple — a single melodic line emerging
            new Color(30, 190, 120),    // [3] Echo Teal — the answer phrase, reflected and inverted
            new Color(70, 230, 175),    // [4] Fugue Cyan — voices converging in counterpoint
            new Color(200, 250, 235),   // [5] Harmonic White — all voices resolving in unison
        };

        public static Color ShadowVoid => FuguePalette[0];
        public static Color DeepChorus => FuguePalette[1];
        public static Color VoicePurple => FuguePalette[2];
        public static Color EchoTeal => FuguePalette[3];
        public static Color FugueCyan => FuguePalette[4];
        public static Color HarmonicWhite => FuguePalette[5];

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

        /// <summary>Gets a color along the fugue gradient (0=shadow void → 1=harmonic white).</summary>
        public static Color GetFugueGradient(float t) => MulticolorLerp(t, FuguePalette);

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

        // ─────────── THEME TEXTURE ACCENTS ───────────

        public static void DrawThemeAccents(SpriteBatch sb, Vector2 worldPos, float scale, float intensity = 1f)
        {
            EnigmaVFXLibrary.DrawThemeStarFlare(sb, worldPos, scale, intensity * 0.5f);
            float rot = (float)Main.GameUpdateCount * 0.02f;
            EnigmaVFXLibrary.DrawThemeImpactRing(sb, worldPos, scale, intensity * 0.4f, rot);
        }
    }
}
