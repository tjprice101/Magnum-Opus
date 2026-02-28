using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

namespace MagnumOpus.Content.Fate.ResonantWeapons.ResonanceOfABygoneReality
{
    /// <summary>
    /// GPU primitive trail renderer for Resonance weapon effects.
    /// Builds CatmullRom-smoothed triangle strip geometry with line fallback.
    /// </summary>
    public static class ResonanceTrailRenderer
    {
        /// <summary>
        /// Draws a trail strip from a position history array.
        /// </summary>
        public static void DrawTrail(SpriteBatch sb, Vector2[] positions, int count,
            ResonanceTrailSettings settings, Effect shader = null)
        {
            if (count < 2 || positions == null) return;

            // Build smoothed points via CatmullRom subdivision
            int smoothCount = Math.Min(count, 20);
            Vector2[] smoothed = new Vector2[smoothCount * 3];
            int totalPts = 0;

            for (int i = 0; i < smoothCount - 1 && i < positions.Length - 1; i++)
            {
                if (positions[i] == Vector2.Zero || positions[i + 1] == Vector2.Zero)
                    continue;

                Vector2 p0 = i > 0 ? positions[i - 1] : positions[i];
                Vector2 p1 = positions[i];
                Vector2 p2 = positions[i + 1];
                Vector2 p3 = i + 2 < count ? positions[i + 2] : positions[i + 1];

                for (int s = 0; s < 3; s++)
                {
                    float t = s / 3f;
                    if (totalPts < smoothed.Length)
                    {
                        smoothed[totalPts++] = Vector2.CatmullRom(p0, p1, p2, p3, t);
                    }
                }
            }

            if (totalPts < 2) return;

            // Line-based fallback rendering using SpriteBatch
            DrawLineFallback(sb, smoothed, totalPts, settings);
        }

        /// <summary>
        /// Line-segment fallback using SpriteBatch.Draw with MagicPixel.
        /// </summary>
        private static void DrawLineFallback(SpriteBatch sb, Vector2[] points, int count,
            ResonanceTrailSettings settings)
        {
            Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;
            if (pixel == null) return;

            for (int i = 0; i < count - 1; i++)
            {
                float progress = (float)i / (count - 1);
                float width = settings.WidthFunction(progress) * settings.MaxWidth;
                Color color = settings.ColorFunction(progress);

                Vector2 start = points[i] - Main.screenPosition;
                Vector2 end = points[i + 1] - Main.screenPosition;
                Vector2 diff = end - start;
                float len = diff.Length();
                if (len < 0.5f) continue;

                float rot = MathF.Atan2(diff.Y, diff.X);
                sb.Draw(pixel, start, new Rectangle(0, 0, 1, 1), color, rot,
                    new Vector2(0, 0.5f), new Vector2(len, Math.Max(width, 1f)),
                    SpriteEffects.None, 0f);
            }
        }
    }
}
