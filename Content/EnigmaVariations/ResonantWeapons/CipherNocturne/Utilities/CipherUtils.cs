using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using MagnumOpus.Content.EnigmaVariations;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.CipherNocturne.Utilities
{
    /// <summary>
    /// Self-contained utility class for CipherNocturne.
    /// Palette, easings, animation curves, SpriteBatch helpers.
    /// </summary>
    public static class CipherUtils
    {
        // =====================================================================
        //  PALETTE — Cipher's mystery unraveling: void black → deep purple → arcane green → white flash
        // =====================================================================
        
        public static readonly Color[] CipherPalette = new Color[]
        {
            new Color(10, 5, 15),       // [0] Abyss Black — the unknowable void
            new Color(60, 15, 100),      // [1] Deep Enigma — shrouded purple depth
            new Color(120, 50, 180),     // [2] Arcane Violet — the cipher's glow
            new Color(40, 200, 90),      // [3] Unravel Green — reality fraying
            new Color(100, 240, 150),    // [4] Cipher Bright — revelation flash
            new Color(200, 255, 220),    // [5] White Revelation — truth exposed
        };

        public static Color AbyssBlack => CipherPalette[0];
        public static Color DeepEnigma => CipherPalette[1];
        public static Color ArcaneViolet => CipherPalette[2];
        public static Color UnravelGreen => CipherPalette[3];
        public static Color CipherBright => CipherPalette[4];
        public static Color WhiteRevelation => CipherPalette[5];

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

        /// <summary>Gets a color along the cipher gradient (0=void → 1=revelation).</summary>
        public static Color GetCipherGradient(float t) => MulticolorLerp(t, CipherPalette);

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
