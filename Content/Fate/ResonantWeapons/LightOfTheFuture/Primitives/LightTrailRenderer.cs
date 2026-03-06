using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Fate.ResonantWeapons.LightOfTheFuture.Primitives
{
    /// <summary>
    /// GPU primitive trail renderer for Light of the Future.
    /// Self-contained — builds indexed triangle strips from point arrays
    /// with CatmullRom smoothing and parallel-transport normals.
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public class LightTrailRenderer : ModSystem
    {
        private static bool _initialized;
        private static GraphicsDevice _device;

        private static void EnsureInitialized()
        {
            if (_initialized) return;
            _device = Main.graphics.GraphicsDevice;
            _initialized = _device != null;
        }

        private static Matrix CalculateWVP()
        {
            Matrix world = Matrix.Identity;
            Matrix view = Matrix.Identity;
            Vector2 zoom = Main.GameViewMatrix.Zoom;
            int width = Main.screenWidth;
            int height = Main.screenHeight;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, width / zoom.X, height / zoom.Y, 0, -1, 1);
            if (Main.LocalPlayer.gravDir == -1f)
                view = Matrix.CreateScale(1f, -1f, 1f) * Matrix.CreateTranslation(0f, height, 0f);
            return world * view * projection;
        }

        public override void OnModUnload()
        {
            _initialized = false;
            _device = null;
        }

        /// <summary>
        /// Render a trail strip from an array of world positions.
        /// Points are ordered newest → oldest. The renderer generates
        /// smooth vertices with CatmullRom interpolation.
        /// </summary>
        public static void RenderTrail(Vector2[] points, LightTrailSettings settings, int pointCount, int smoothing = 2)
        {
            EnsureInitialized();
            if (_device == null || pointCount < 2) return;
            if (settings.Shader == null) return;

            // Count valid (non-zero) points
            int validCount = 0;
            for (int i = 0; i < pointCount && i < points.Length; i++)
            {
                if (points[i] != Vector2.Zero) validCount++;
                else break;
            }
            if (validCount < 2) return;

            // Generate smoothed points via CatmullRom
            int smoothedCount = (validCount - 1) * smoothing + 1;
            Vector2[] smoothed = new Vector2[smoothedCount];

            for (int i = 0; i < validCount - 1; i++)
            {
                Vector2 p0 = points[Math.Max(i - 1, 0)];
                Vector2 p1 = points[i];
                Vector2 p2 = points[Math.Min(i + 1, validCount - 1)];
                Vector2 p3 = points[Math.Min(i + 2, validCount - 1)];

                for (int s = 0; s < smoothing; s++)
                {
                    float t = (float)s / smoothing;
                    smoothed[i * smoothing + s] = Vector2.CatmullRom(p0, p1, p2, p3, t);
                }
            }
            smoothed[smoothedCount - 1] = points[validCount - 1];

            // Build triangle strip vertices
            int vertexCount = smoothedCount * 2;
            var vertices = new LightVertexType[vertexCount];

            for (int i = 0; i < smoothedCount; i++)
            {
                float progress = (float)i / (smoothedCount - 1);

                Vector2 tangent;
                if (i == 0)
                    tangent = smoothed[1] - smoothed[0];
                else if (i == smoothedCount - 1)
                    tangent = smoothed[i] - smoothed[i - 1];
                else
                    tangent = smoothed[i + 1] - smoothed[i - 1];

                if (tangent == Vector2.Zero) tangent = Vector2.UnitX;
                tangent.Normalize();
                Vector2 normal = new(-tangent.Y, tangent.X);

                float width = settings.WidthFunction?.Invoke(progress, i) ?? 20f;
                Color color = settings.ColorFunction?.Invoke(progress) ?? Color.White;
                Vector2 offset = settings.OffsetFunction?.Invoke(progress, i) ?? Vector2.Zero;

                Vector2 pos = smoothed[i] + offset - Main.screenPosition;

                vertices[i * 2] = new LightVertexType(
                    pos + normal * width * 0.5f,
                    color,
                    new Vector3(progress, 0f, width));

                vertices[i * 2 + 1] = new LightVertexType(
                    pos - normal * width * 0.5f,
                    color,
                    new Vector3(progress, 1f, width));
            }

            // Build index buffer for triangle strip
            int indexCount = (smoothedCount - 1) * 6;
            var indices = new short[indexCount];
            for (int i = 0; i < smoothedCount - 1; i++)
            {
                int bi = i * 6;
                int vi = i * 2;
                indices[bi + 0] = (short)vi;
                indices[bi + 1] = (short)(vi + 1);
                indices[bi + 2] = (short)(vi + 2);
                indices[bi + 3] = (short)(vi + 1);
                indices[bi + 4] = (short)(vi + 3);
                indices[bi + 5] = (short)(vi + 2);
            }

            // Shader is REQUIRED
            if (settings.Shader == null) return;

            Effect effect = settings.Shader.Shader;
            if (effect != null)
                effect.Parameters["uWorldViewProjection"]?.SetValue(CalculateWVP());

            var prevRasterizer = _device.RasterizerState;
            _device.RasterizerState = RasterizerState.CullNone;

            settings.Shader.Apply();

            _device.DrawUserIndexedPrimitives(
                PrimitiveType.TriangleList,
                vertices, 0, vertexCount,
                indices, 0, indexCount / 3);

            _device.RasterizerState = prevRasterizer;
        }
    }
}
