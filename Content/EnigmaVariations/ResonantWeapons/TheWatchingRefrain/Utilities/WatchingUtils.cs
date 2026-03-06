using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using MagnumOpus.Content.EnigmaVariations;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheWatchingRefrain.Utilities
{
    /// <summary>
    /// Self-contained utility class for TheWatchingRefrain.
    /// Palette, easings, animation curves, SpriteBatch helpers.
    /// Themed around watching eyes, phantoms, and mystery.
    /// </summary>
    public static class WatchingUtils
    {
        // =====================================================================
        //  PALETTE — The Watcher's gaze: phantom black → deep void → refrain purple → eerie gaze green → spectral mint → phantom white
        // =====================================================================

        public static readonly Color[] WatchingPalette = new Color[]
        {
            new Color(6, 3, 16),         // [0] Phantom Black — the unseen watcher
            new Color(35, 12, 95),        // [1] Watcher Deep — lurking in void depths
            new Color(125, 45, 185),      // [2] Refrain Purple — the repeating motif
            new Color(35, 200, 85),       // [3] Gaze Green — the watcher's piercing eye
            new Color(75, 235, 155),      // [4] Spectral Mint — phantom afterglow
            new Color(205, 250, 230),     // [5] Phantom White — ghostly revelation
        };

        public static Color PhantomBlack => WatchingPalette[0];
        public static Color WatcherDeep => WatchingPalette[1];
        public static Color RefrainPurple => WatchingPalette[2];
        public static Color GazeGreen => WatchingPalette[3];
        public static Color SpectralMint => WatchingPalette[4];
        public static Color PhantomWhite => WatchingPalette[5];

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

        /// <summary>Gets a color along the watching gradient (0=phantom black → 1=phantom white).</summary>
        public static Color GetWatchingGradient(float t) => MulticolorLerp(t, WatchingPalette);

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
            sb.Begin(SpriteSortMode.Immediate, MagnumBlendStates.ShaderAdditive, SamplerState.LinearWrap,
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
