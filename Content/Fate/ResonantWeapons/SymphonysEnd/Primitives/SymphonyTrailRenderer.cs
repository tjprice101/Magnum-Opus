using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.Fate.ResonantWeapons.SymphonysEnd
{
    /// <summary>
    /// Self-contained trail renderer for Symphony's End.
    /// Supports shader-driven primitive rendering with a SpriteBatch line-segment fallback.
    /// 
    /// IMPORTANT: Caller must End() the active SpriteBatch BEFORE calling DrawTrail,
    /// and Begin() it again AFTER. The PreDraw methods in the projectiles handle this.
    /// </summary>
    public static class SymphonyTrailRenderer
    {
        /// <summary>
        /// Render a trail strip from an array of world-space positions.
        /// </summary>
        /// <param name="positions">Projectile.oldPos (top-left corner); offset to center is applied internally.</param>
        /// <param name="halfSize">Half the projectile hitbox size, used to offset oldPos to center.</param>
        /// <param name="settings">Width/color curves.</param>
        /// <param name="shader">HLSL effect, or null for SpriteBatch fallback.</param>
        public static void DrawTrail(Vector2[] positions, Vector2 halfSize,
            SymphonyTrailSettings settings, Effect shader)
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

        // ─── Primitive path (GPU shader) ──────────────────────────

        private static void DrawPrimitive(Vector2[] positions, Vector2 half,
            int count, SymphonyTrailSettings settings, Effect shader)
        {
            var verts = new SymphonyVertexType[count * 2];

            for (int i = 0; i < count; i++)
            {
                float progress = (float)i / (count - 1);
                Vector2 worldCenter = positions[i] + half;
                Vector2 screen = worldCenter - Main.screenPosition;
                float width = settings.WidthFunction(progress);
                Color color = settings.ColorFunction(progress);

                // Direction / normal
                Vector2 dir;
                if (i == 0 && count > 1)
                    dir = (positions[1] - positions[0]).SafeNormalize(Vector2.UnitY);
                else if (i == count - 1)
                    dir = (positions[i] - positions[i - 1]).SafeNormalize(Vector2.UnitY);
                else if (settings.SmoothNormals)
                    dir = ((positions[i + 1] - positions[i - 1]) * 0.5f).SafeNormalize(Vector2.UnitY);
                else
                    dir = (positions[i] - positions[i - 1]).SafeNormalize(Vector2.UnitY);

                Vector2 perp = new Vector2(-dir.Y, dir.X);

                verts[i * 2]     = new SymphonyVertexType(screen + perp * width, new Vector2(progress, 0f), color);
                verts[i * 2 + 1] = new SymphonyVertexType(screen - perp * width, new Vector2(progress, 1f), color);
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
                gd.DrawUserPrimitives(PrimitiveType.TriangleStrip, verts, 0, count * 2 - 2);
            }
        }

        // ─── SpriteBatch fallback (no shader) ─────────────────────

        private static void DrawFallback(Vector2[] positions, Vector2 half,
            int count, SymphonyTrailSettings settings)
        {
            Texture2D pixel = MagnumTextureRegistry.GetSoftGlow();
            if (pixel == null) return;

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
