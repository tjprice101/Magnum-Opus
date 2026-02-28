using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;

namespace MagnumOpus.Content.Fate.ResonantWeapons.DestinysCrescendo
{
    /// <summary>
    /// Self-contained trail renderer for Destiny's Crescendo.
    /// Supports shader-driven primitive rendering with CatmullRom smoothing
    /// and a SpriteBatch line-segment fallback.
    /// 
    /// IMPORTANT: Caller must End() the active SpriteBatch BEFORE calling DrawTrail,
    /// and Begin() it again AFTER. The PreDraw methods in the projectiles handle this.
    /// </summary>
    public static class CrescendoTrailRenderer
    {
        /// <summary>
        /// Render a trail strip from an array of world-space positions.
        /// </summary>
        /// <param name="positions">Projectile.oldPos (top-left corner); offset to center is applied internally.</param>
        /// <param name="halfSize">Half the projectile hitbox size, used to offset oldPos to center.</param>
        /// <param name="settings">Width/color curves.</param>
        /// <param name="shader">HLSL effect, or null for SpriteBatch fallback.</param>
        public static void DrawTrail(Vector2[] positions, Vector2 halfSize,
            CrescendoTrailSettings settings, Effect shader)
        {
            int count = CountValid(positions);
            if (count < 2) return;

            if (shader != null)
                DrawPrimitive(positions, halfSize, count, settings, shader);
            else
                DrawFallback(positions, halfSize, count, settings);
        }

        // ─── Helpers ──────────────────────────────────────────────

        private static int CountValid(Vector2[] pos)
        {
            if (pos == null) return 0;
            for (int i = 0; i < pos.Length; i++)
                if (pos[i] == Vector2.Zero) return i;
            return pos.Length;
        }

        // ─── Primitive path (GPU shader) with CatmullRom smoothing ──

        private static void DrawPrimitive(Vector2[] positions, Vector2 half,
            int count, CrescendoTrailSettings settings, Effect shader)
        {
            // Generate smoothed positions using CatmullRom
            int smoothCount = count * 2 - 1;
            var smoothed = new Vector2[smoothCount];

            for (int i = 0; i < count; i++)
            {
                smoothed[i * 2] = positions[i] + half;

                if (i < count - 1)
                {
                    Vector2 p0 = (i > 0) ? positions[i - 1] + half : positions[i] + half;
                    Vector2 p1 = positions[i] + half;
                    Vector2 p2 = positions[i + 1] + half;
                    Vector2 p3 = (i + 2 < count) ? positions[i + 2] + half : positions[i + 1] + half;
                    smoothed[i * 2 + 1] = CrescendoUtils.CatmullRom(p0, p1, p2, p3, 0.5f);
                }
            }

            var verts = new CrescendoVertexType[smoothCount * 2];

            for (int i = 0; i < smoothCount; i++)
            {
                float progress = (float)i / (smoothCount - 1);
                Vector2 screen = smoothed[i] - Main.screenPosition;
                float width = settings.WidthFunction(progress);
                Color color = settings.ColorFunction(progress);

                // Direction / normal
                Vector2 dir;
                if (i == 0 && smoothCount > 1)
                    dir = (smoothed[1] - smoothed[0]).SafeNormalize(Vector2.UnitY);
                else if (i == smoothCount - 1)
                    dir = (smoothed[i] - smoothed[i - 1]).SafeNormalize(Vector2.UnitY);
                else if (settings.SmoothNormals)
                    dir = ((smoothed[i + 1] - smoothed[i - 1]) * 0.5f).SafeNormalize(Vector2.UnitY);
                else
                    dir = (smoothed[i] - smoothed[i - 1]).SafeNormalize(Vector2.UnitY);

                Vector2 perp = new Vector2(-dir.Y, dir.X);

                verts[i * 2]     = new CrescendoVertexType(screen + perp * width, new Vector2(progress, 0f), color);
                verts[i * 2 + 1] = new CrescendoVertexType(screen - perp * width, new Vector2(progress, 1f), color);
            }

            GraphicsDevice gd = Main.graphics.GraphicsDevice;
            gd.BlendState        = BlendState.Additive;
            gd.DepthStencilState = DepthStencilState.None;
            gd.RasterizerState   = RasterizerState.CullNone;

            Matrix proj = Matrix.CreateOrthographicOffCenter(
                0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            shader.Parameters["uTransformMatrix"]?.SetValue(
                Main.GameViewMatrix.TransformationMatrix * proj);
            shader.Parameters["uTime"]?.SetValue((float)Main.timeForVisualEffects * 0.01f);
            shader.Parameters["uOpacity"]?.SetValue(1f);

            foreach (EffectPass pass in shader.CurrentTechnique.Passes)
            {
                pass.Apply();
                gd.DrawUserPrimitives(PrimitiveType.TriangleStrip, verts, 0, smoothCount * 2 - 2);
            }
        }

        // ─── SpriteBatch fallback (no shader) ─────────────────────

        private static void DrawFallback(Vector2[] positions, Vector2 half,
            int count, CrescendoTrailSettings settings)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;

            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);

            for (int i = 0; i < count - 1; i++)
            {
                float progress = (float)i / (count - 1);
                Vector2 a = positions[i]     + half - Main.screenPosition;
                Vector2 b = positions[i + 1] + half - Main.screenPosition;
                Vector2 diff = b - a;
                float length = diff.Length();
                if (length < 0.5f) continue;

                float angle = diff.ToRotation();
                float width = settings.WidthFunction(progress);
                Color color = settings.ColorFunction(progress);

                Main.spriteBatch.Draw(pixel, a, new Rectangle(0, 0, 1, 1), color,
                    angle, new Vector2(0f, 0.5f), new Vector2(length, width),
                    SpriteEffects.None, 0f);
            }

            Main.spriteBatch.End();
        }
    }
}
