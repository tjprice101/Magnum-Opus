using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace MagnumOpus.Content.Eroica.Weapons.BlossomOfTheSakura.Primitives
{
    public static class BlossomTrailRenderer
    {
        private const int MaxVertices = 2048;
        private const int MaxIndices = 4096;

        private static DynamicVertexBuffer _vertexBuffer;
        private static DynamicIndexBuffer _indexBuffer;

        public static void RenderTrail(Vector2[] positions, BlossomTrailSettings settings, int maxPointCount = 40)
        {
            if (positions == null || positions.Length < 2) return;
            if (Main.dedServ) return;

            GraphicsDevice device = Main.graphics.GraphicsDevice;

            if (_vertexBuffer == null || _vertexBuffer.IsDisposed)
                _vertexBuffer = new DynamicVertexBuffer(device, BlossomVertexType._vertexDeclaration, MaxVertices, BufferUsage.WriteOnly);
            if (_indexBuffer == null || _indexBuffer.IsDisposed)
                _indexBuffer = new DynamicIndexBuffer(device, IndexElementSize.SixteenBits, MaxIndices, BufferUsage.WriteOnly);

            Vector2[] smoothed = settings.Smoothen ? SmoothPositions(positions, maxPointCount) : positions;
            int pointCount = Math.Min(smoothed.Length, maxPointCount);
            if (pointCount < 2) return;

            int vertexCount = pointCount * 2;
            int indexCount = (pointCount - 1) * 6;
            if (vertexCount > MaxVertices || indexCount > MaxIndices) return;

            BlossomVertexType[] vertices = new BlossomVertexType[vertexCount];
            short[] indices = new short[indexCount];

            float totalLength = 0f;
            float[] lengths = new float[pointCount];
            lengths[0] = 0f;
            for (int i = 1; i < pointCount; i++)
            {
                totalLength += Vector2.Distance(smoothed[i], smoothed[i - 1]);
                lengths[i] = totalLength;
            }

            Matrix viewProjection = CalculateMatrices();

            for (int i = 0; i < pointCount; i++)
            {
                float completion = totalLength > 0 ? lengths[i] / totalLength : (float)i / (pointCount - 1);
                float width = settings.WidthFunction(completion);
                Color color = settings.ColorFunction(completion);

                Vector2 normal;
                if (i == 0)
                    normal = (smoothed[1] - smoothed[0]).SafeNormalize(Vector2.UnitY);
                else if (i == pointCount - 1)
                    normal = (smoothed[i] - smoothed[i - 1]).SafeNormalize(Vector2.UnitY);
                else
                    normal = (smoothed[i + 1] - smoothed[i - 1]).SafeNormalize(Vector2.UnitY);

                normal = new Vector2(-normal.Y, normal.X);

                Vector2 offset = settings.OffsetFunction?.Invoke(completion) ?? Vector2.Zero;
                Vector2 worldPos = smoothed[i] + offset - Main.screenPosition;

                vertices[i * 2] = new BlossomVertexType(worldPos + normal * width * 0.5f, color, new Vector3(completion, 0f, width));
                vertices[i * 2 + 1] = new BlossomVertexType(worldPos - normal * width * 0.5f, color, new Vector3(completion, 1f, width));
            }

            for (int i = 0; i < pointCount - 1; i++)
            {
                int startVert = i * 2;
                int startIdx = i * 6;
                indices[startIdx] = (short)startVert;
                indices[startIdx + 1] = (short)(startVert + 1);
                indices[startIdx + 2] = (short)(startVert + 2);
                indices[startIdx + 3] = (short)(startVert + 2);
                indices[startIdx + 4] = (short)(startVert + 1);
                indices[startIdx + 5] = (short)(startVert + 3);
            }

            _vertexBuffer.SetData(vertices, 0, vertexCount, SetDataOptions.Discard);
            _indexBuffer.SetData(indices, 0, indexCount, SetDataOptions.Discard);

            device.SetVertexBuffer(_vertexBuffer);
            device.Indices = _indexBuffer;

            device.RasterizerState = RasterizerState.CullNone;

            if (settings.Shader?.Shader != null)
            {
                settings.Shader.Shader.Parameters["uWorldViewProjection"]?.SetValue(viewProjection);
                settings.Shader.Apply();
            }

            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexCount, 0, (pointCount - 1) * 2);
        }

        private static Vector2[] SmoothPositions(Vector2[] positions, int outputCount)
        {
            if (positions.Length < 3) return positions;
            Vector2[] result = new Vector2[outputCount];
            for (int i = 0; i < outputCount; i++)
            {
                float t = (float)i / (outputCount - 1) * (positions.Length - 1);
                int idx = (int)t;
                float frac = t - idx;
                int p0 = Math.Max(idx - 1, 0);
                int p1 = idx;
                int p2 = Math.Min(idx + 1, positions.Length - 1);
                int p3 = Math.Min(idx + 2, positions.Length - 1);
                result[i] = Vector2.CatmullRom(positions[p0], positions[p1], positions[p2], positions[p3], frac);
            }
            return result;
        }

        private static Matrix CalculateMatrices()
        {
            var zoom = Main.GameViewMatrix.Zoom;
            var viewport = Main.graphics.GraphicsDevice.Viewport;
            return Matrix.CreateOrthographicOffCenter(0, viewport.Width / zoom.X, viewport.Height / zoom.Y, 0, -1, 1);
        }

        public static void Unload()
        {
            _vertexBuffer?.Dispose();
            _indexBuffer?.Dispose();
            _vertexBuffer = null;
            _indexBuffer = null;
        }
    }
}
