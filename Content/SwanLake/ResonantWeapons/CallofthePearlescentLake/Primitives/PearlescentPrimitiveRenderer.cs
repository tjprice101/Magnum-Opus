using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.CallofthePearlescentLake.Primitives
{
    public readonly struct PearlescentVertex : IVertexType
    {
        public readonly Vector2 Position;
        public readonly Color Color;
        public readonly Vector3 TextureCoordinates;

        public static readonly VertexDeclaration Declaration = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
            new VertexElement(8, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0)
        );

        VertexDeclaration IVertexType.VertexDeclaration => Declaration;

        public PearlescentVertex(Vector2 position, Color color, Vector3 texCoords)
        {
            Position = position;
            Color = color;
            TextureCoordinates = texCoords;
        }
    }

    public readonly struct PearlescentTrailSettings
    {
        public delegate float WidthFunc(float completionRatio);
        public delegate Color ColorFunc(float completionRatio);

        public readonly WidthFunc Width;
        public readonly ColorFunc TrailColor;
        public readonly Terraria.Graphics.Shaders.MiscShaderData Shader;
        public readonly bool Smoothen;

        public PearlescentTrailSettings(WidthFunc width, ColorFunc trailColor,
            Terraria.Graphics.Shaders.MiscShaderData shader = null, bool smoothen = true)
        {
            Width = width;
            TrailColor = trailColor;
            Shader = shader;
            Smoothen = smoothen;
        }
    }

    public class PearlescentPrimitiveRenderer : IDisposable
    {
        private const int MaxVertices = 2048;
        private const int MaxIndices = 6144;
        private DynamicVertexBuffer _vertexBuffer;
        private DynamicIndexBuffer _indexBuffer;
        private PearlescentVertex[] _vertices;
        private short[] _indices;
        private bool _disposed;

        public PearlescentPrimitiveRenderer()
        {
            _vertices = new PearlescentVertex[MaxVertices];
            _indices = new short[MaxIndices];
        }

        private void EnsureBuffers(GraphicsDevice device)
        {
            if (_vertexBuffer == null || _vertexBuffer.IsDisposed)
                _vertexBuffer = new DynamicVertexBuffer(device, PearlescentVertex.Declaration, MaxVertices, BufferUsage.WriteOnly);
            if (_indexBuffer == null || _indexBuffer.IsDisposed)
                _indexBuffer = new DynamicIndexBuffer(device, IndexElementSize.SixteenBits, MaxIndices, BufferUsage.WriteOnly);
        }

        public void RenderTrail(Vector2[] positions, PearlescentTrailSettings settings, int? renderPoints = null)
        {
            if (positions == null || positions.Length < 2) return;

            var valid = new System.Collections.Generic.List<Vector2>();
            foreach (var pos in positions)
                if (pos != Vector2.Zero) valid.Add(pos);
            if (valid.Count < 2) return;

            int numPoints = renderPoints ?? valid.Count;
            numPoints = Math.Min(numPoints, valid.Count);

            Vector2[] sampled = Resample(valid.ToArray(), numPoints);
            if (sampled.Length < 2) return;

            float totalDist = 0f;
            for (int i = 1; i < sampled.Length; i++)
                totalDist += Vector2.Distance(sampled[i - 1], sampled[i]);

            int vertexCount = 0, indexCount = 0;
            float accumulated = 0f;

            for (int i = 0; i < sampled.Length; i++)
            {
                float t = totalDist > 0.001f ? accumulated / totalDist : (float)i / (sampled.Length - 1);
                if (i > 0) accumulated += Vector2.Distance(sampled[i - 1], sampled[i]);

                float halfWidth = settings.Width(t) * 0.5f;
                Color color = settings.TrailColor(t);

                Vector2 tangent = i == 0 ? sampled[1] - sampled[0]
                    : i == sampled.Length - 1 ? sampled[i] - sampled[i - 1]
                    : sampled[i + 1] - sampled[i - 1];
                if (tangent.LengthSquared() < 0.0001f) tangent = Vector2.UnitX;
                tangent.Normalize();
                Vector2 normal = new Vector2(-tangent.Y, tangent.X);

                Vector2 screenPos = sampled[i] - Main.screenPosition;
                int vi = vertexCount;
                _vertices[vi] = new PearlescentVertex(screenPos + normal * halfWidth, color, new Vector3(t, 0f, halfWidth));
                _vertices[vi + 1] = new PearlescentVertex(screenPos - normal * halfWidth, color, new Vector3(t, 1f, halfWidth));
                vertexCount += 2;

                if (i > 0)
                {
                    int prev = vi - 2;
                    _indices[indexCount++] = (short)prev;
                    _indices[indexCount++] = (short)(prev + 1);
                    _indices[indexCount++] = (short)vi;
                    _indices[indexCount++] = (short)(prev + 1);
                    _indices[indexCount++] = (short)(vi + 1);
                    _indices[indexCount++] = (short)vi;
                }

                if (vertexCount >= MaxVertices - 2 || indexCount >= MaxIndices - 6) break;
            }

            if (vertexCount < 4 || indexCount < 6) return;

            GraphicsDevice device = Main.graphics.GraphicsDevice;
            EnsureBuffers(device);
            var oldRasterizer = device.RasterizerState;
            device.RasterizerState = RasterizerState.CullNone;

            Matrix transform = Matrix.CreateLookAt(Vector3.Zero, Vector3.UnitZ, Vector3.Up) *
                Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            if (settings.Shader != null)
            {
                settings.Shader.Shader.Parameters["uWorldViewProjection"]?.SetValue(transform);
                settings.Shader.Apply();
            }

            _vertexBuffer.SetData(_vertices, 0, vertexCount, SetDataOptions.Discard);
            _indexBuffer.SetData(_indices, 0, indexCount, SetDataOptions.Discard);
            device.SetVertexBuffer(_vertexBuffer);
            device.Indices = _indexBuffer;
            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexCount, 0, indexCount / 3);
            device.RasterizerState = oldRasterizer;
        }

        private static Vector2[] Resample(Vector2[] positions, int targetCount)
        {
            if (positions.Length == targetCount) return positions;
            Vector2[] result = new Vector2[targetCount];
            for (int i = 0; i < targetCount; i++)
            {
                float t = (float)i / (targetCount - 1);
                float idx = t * (positions.Length - 1);
                int lo = (int)idx;
                int hi = Math.Min(lo + 1, positions.Length - 1);
                result[i] = Vector2.Lerp(positions[lo], positions[hi], idx - lo);
            }
            return result;
        }

        public void Dispose()
        {
            if (!_disposed) { _vertexBuffer?.Dispose(); _indexBuffer?.Dispose(); _disposed = true; }
        }
    }
}
