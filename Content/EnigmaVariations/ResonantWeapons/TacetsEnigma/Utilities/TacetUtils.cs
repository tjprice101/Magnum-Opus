using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using MagnumOpus.Content.EnigmaVariations;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TacetsEnigma.Utilities
{
    /// <summary>
    /// Self-contained utility class for TacetsEnigma.
    /// Palette, easings, animation curves, SpriteBatch helpers.
    /// </summary>
    public static class TacetUtils
    {
        // =====================================================================
        //  PALETTE — Silence and paradox: dead void → muted depth → tacet purple → paradox green → unstable lime → flash white
        // =====================================================================

        public static readonly Color[] TacetPalette = new Color[]
        {
            new Color(5, 2, 15),         // [0] Dead Silence — the absence of sound
            new Color(30, 10, 80),       // [1] Muted Abyss — sound swallowed by void
            new Color(120, 40, 180),     // [2] Tacet Purple — the paradox's glow
            new Color(40, 200, 90),      // [3] Paradox Green — reality disagreeing with itself
            new Color(100, 255, 140),    // [4] Unstable Lime — paradox overload flash
            new Color(230, 255, 240),    // [5] Flash White — paradox detonation
        };

        public static Color DeadSilence => TacetPalette[0];
        public static Color MutedAbyss => TacetPalette[1];
        public static Color TacetPurple => TacetPalette[2];
        public static Color ParadoxGreen => TacetPalette[3];
        public static Color UnstableLime => TacetPalette[4];
        public static Color FlashWhite => TacetPalette[5];

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

        /// <summary>Gets a color along the tacet gradient (0=silence → 1=flash).</summary>
        public static Color GetTacetGradient(float t) => MulticolorLerp(t, TacetPalette);

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
