using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using MagnumOpus.Content.SwanLake;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.CallofthePearlescentLake.Utilities
{
    /// <summary>
    /// Self-contained utility library for Call of the Pearlescent Lake.
    /// </summary>
    public static class PearlescentUtils
    {
        public static readonly Color PearlWhite = new Color(245, 240, 250);
        public static readonly Color LakeSilver = new Color(180, 195, 220);
        public static readonly Color DeepLake = new Color(40, 50, 80);
        public static readonly Color MistBlue = new Color(160, 180, 210);
        public static readonly Color LoreColor = new Color(240, 240, 255);

        public static readonly Color[] RipplePalette = new Color[]
        {
            new Color(20, 25, 45),
            new Color(80, 100, 150),
            new Color(160, 180, 220),
            new Color(220, 230, 250),
            new Color(245, 240, 255),
            new Color(255, 255, 255),
        };

        public static Color GetRainbow(float offset = 0f)
        {
            float hue = (Main.GameUpdateCount * 0.012f + offset) % 1f;
            return Main.hslToRgb(hue, 0.85f, 0.8f);
        }

        public static Color MulticolorLerp(float t, params Color[] colors)
        {
            t = MathHelper.Clamp(t, 0f, 0.999f);
            int count = colors.Length;
            float scaled = t * (count - 1);
            int index = (int)scaled;
            float frac = scaled - index;
            if (index >= count - 1) return colors[count - 1];
            return Color.Lerp(colors[index], colors[index + 1], frac);
        }

        public static Color Additive(Color c, float opacity = 1f)
            => new Color(c.R, c.G, c.B, 0) * opacity;

        public static Vector2 SafeDirectionTo(this Vector2 from, Vector2 to, Vector2 fallback = default)
        {
            Vector2 diff = to - from;
            float length = diff.Length();
            if (length < 0.0001f) return fallback;
            return diff / length;
        }

        public static void EnterShaderRegion(this SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public static void RestoreSpriteBatch(this SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        // ─────────── THEME TEXTURE ACCENTS ───────────

        public static void DrawThemeAccents(SpriteBatch sb, Vector2 worldPos, float scale, float intensity = 1f)
        {
            SwanLakeVFXLibrary.DrawThemeCrystalAccent(sb, worldPos, scale, intensity * 0.5f);
            float rot = (float)Main.GameUpdateCount * 0.02f;
            SwanLakeVFXLibrary.DrawThemeImpactRing(sb, worldPos, scale, intensity * 0.4f, rot);
        }
    }
}
