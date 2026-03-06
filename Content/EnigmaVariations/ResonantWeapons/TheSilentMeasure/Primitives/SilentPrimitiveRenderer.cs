using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheSilentMeasure.Primitives
{
    /// <summary>
    /// GPU primitive trail renderer for TheSilentMeasure's seeker trails.
    /// Builds and renders a triangle-list from a list of positions using SilentVertex.
    /// Supports CatmullRom spline smoothing and arc-length parameterization.
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public sealed class SilentPrimitiveRenderer : ModSystem
    {
        private static DynamicVertexBuffer _vertexBuffer;
        private static DynamicIndexBuffer _indexBuffer;
        private static SilentVertex[] _vertices;
        private static short[] _indices;

        private const int MaxVertices = 2048;
        private const int MaxIndices = 4096;

        public override void OnModLoad()
        {
            Main.QueueMainThreadAction(() =>
            {
                var device = Main.graphics.GraphicsDevice;
                _vertexBuffer = new DynamicVertexBuffer(device, SilentVertex.VertexDecl, MaxVertices, BufferUsage.WriteOnly);
                _indexBuffer = new DynamicIndexBuffer(device, IndexElementSize.SixteenBits, MaxIndices, BufferUsage.WriteOnly);
                _vertices = new SilentVertex[MaxVertices];
                _indices = new short[MaxIndices];
            });
        }

        public override void OnModUnload()
        {
            Main.QueueMainThreadAction(() =>
            {
                _vertexBuffer?.Dispose();
                _indexBuffer?.Dispose();
                _vertexBuffer = null;
                _indexBuffer = null;
                _vertices = null;
                _indices = null;
            });
        }

        /// <summary>
        /// Renders a trail from the given positions using the specified settings.
        /// </summary>
        public static void RenderTrail(List<Vector2> positions, SilentPrimitiveSettings settings)
        {
            if (_vertexBuffer == null || positions == null || positions.Count < 2) return;

            // Resample with smoothing if enabled
            List<Vector2> points = settings.Smoothing ? SmoothPoints(positions, settings.MaxPoints) : positions;
            if (points.Count < 2) return;

            // Compute arc-length completion values
            float[] completions = ComputeCompletions(points);

            // Build vertices (triangle list via paired strip)
            int vertCount = 0;
            int idxCount = 0;

            for (int i = 0; i < points.Count; i++)
            {
                if (vertCount + 2 >= MaxVertices) break;

                float completion = completions[i];
                float width = settings.WidthFunction(completion);
                Color color = settings.ColorFunction(completion);
                Vector2 offset = settings.OffsetFunction?.Invoke(completion) ?? Vector2.Zero;

                // Compute normal (perpendicular to tangent)
                Vector2 tangent;
                if (i == 0)
                    tangent = (points[1] - points[0]).SafeNormalize(Vector2.UnitX);
                else if (i == points.Count - 1)
                    tangent = (points[i] - points[i - 1]).SafeNormalize(Vector2.UnitX);
                else
                    tangent = (points[i + 1] - points[i - 1]).SafeNormalize(Vector2.UnitX);

                Vector2 normal = new(-tangent.Y, tangent.X);
                Vector2 worldPos = points[i] + offset;

                // Screen-space transform
                Vector2 screenPos = worldPos - Main.screenPosition;
                float widthCorrection = width > 0 ? 1f : 0.001f;

                float u = completion;
                _vertices[vertCount] = new SilentVertex(screenPos + normal * width * 0.5f, color, new Vector2(u, 0f), widthCorrection);
                _vertices[vertCount + 1] = new SilentVertex(screenPos - normal * width * 0.5f, color, new Vector2(u, 1f), widthCorrection);

                // Build triangle list indices from strip pairs
                if (i > 0)
                {
                    short baseIdx = (short)(vertCount - 2);
                    _indices[idxCount++] = baseIdx;
                    _indices[idxCount++] = (short)(baseIdx + 1);
                    _indices[idxCount++] = (short)(baseIdx + 2);
                    _indices[idxCount++] = (short)(baseIdx + 1);
                    _indices[idxCount++] = (short)(baseIdx + 3);
                    _indices[idxCount++] = (short)(baseIdx + 2);
                }

                vertCount += 2;
            }

            if (vertCount < 4 || idxCount < 6) return;

            // Upload and render
            var device = Main.graphics.GraphicsDevice;

            _vertexBuffer.SetData(_vertices, 0, vertCount, SetDataOptions.Discard);
            _indexBuffer.SetData(_indices, 0, idxCount, SetDataOptions.Discard);

            device.SetVertexBuffer(_vertexBuffer);
            device.Indices = _indexBuffer;

            // Set render states for GPU primitive drawing
            var prevBlend = device.BlendState;
            var prevDepth = device.DepthStencilState;
            var prevRaster = device.RasterizerState;
            device.BlendState = BlendState.Additive;
            device.DepthStencilState = DepthStencilState.None;
            device.RasterizerState = RasterizerState.CullNone;

            // Set world-view-projection matrix on shader with zoom compensation
            if (settings.Shader != null)
            {
                var zoom = Main.GameViewMatrix.Zoom;
                Matrix projection = Matrix.CreateOrthographicOffCenter(
                    0, Main.screenWidth / zoom.X, Main.screenHeight / zoom.Y, 0, -1, 1);
                Matrix wvp = projection;

                var wvpParam = settings.Shader.Parameters["uWorldViewProjection"];
                wvpParam?.SetValue(wvp);

                foreach (var pass in settings.Shader.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertCount, 0, idxCount / 3);
                }
            }

            device.SetVertexBuffer(null);
            device.Indices = null;
            device.BlendState = prevBlend;
            device.DepthStencilState = prevDepth;
            device.RasterizerState = prevRaster;
        }

        private static float[] ComputeCompletions(List<Vector2> points)
        {
            float[] completions = new float[points.Count];
            float totalLength = 0f;

            for (int i = 1; i < points.Count; i++)
                totalLength += Vector2.Distance(points[i - 1], points[i]);

            if (totalLength < 0.001f)
            {
                for (int i = 0; i < points.Count; i++)
                    completions[i] = (float)i / Math.Max(1, points.Count - 1);
                return completions;
            }

            float runLength = 0f;
            completions[0] = 0f;
            for (int i = 1; i < points.Count; i++)
            {
                runLength += Vector2.Distance(points[i - 1], points[i]);
                completions[i] = runLength / totalLength;
            }
            return completions;
        }

        private static List<Vector2> SmoothPoints(List<Vector2> raw, int maxPoints)
        {
            if (raw.Count < 3) return new List<Vector2>(raw);

            int outCount = Math.Min(maxPoints, raw.Count * 3);
            var smoothed = new List<Vector2>(outCount);

            for (int i = 0; i < outCount; i++)
            {
                float t = (float)i / (outCount - 1) * (raw.Count - 1);
                int idx = (int)t;
                float frac = t - idx;

                // CatmullRom spline
                Vector2 p0 = raw[Math.Max(0, idx - 1)];
                Vector2 p1 = raw[idx];
                Vector2 p2 = raw[Math.Min(raw.Count - 1, idx + 1)];
                Vector2 p3 = raw[Math.Min(raw.Count - 1, idx + 2)];

                smoothed.Add(Vector2.CatmullRom(p0, p1, p2, p3, frac));
            }
            return smoothed;
        }
    }
}
