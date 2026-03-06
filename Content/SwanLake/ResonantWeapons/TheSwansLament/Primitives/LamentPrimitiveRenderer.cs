using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Shaders;
using MagnumOpus.Content.SwanLake.ResonantWeapons.TheSwansLament.Utilities;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.TheSwansLament.Primitives
{
    public struct LamentTrailVertex : IVertexType
    {
        public Vector2 Position;
        public Color Color;
        public Vector3 TexCoord;

        public static readonly VertexDeclaration _vertexDeclaration = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
            new VertexElement(8, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0)
        );

        public VertexDeclaration VertexDeclaration => _vertexDeclaration;

        public LamentTrailVertex(Vector2 position, Color color, Vector3 texCoord)
        {
            Position = position;
            Color = color;
            TexCoord = texCoord;
        }
    }

    public struct LamentTrailSettings
    {
        public Func<float, float> WidthFunction;
        public Func<float, Color> ColorFunction;
        public string ShaderKey;
        public float TrailLength;

        public static LamentTrailSettings Default => new LamentTrailSettings
        {
            WidthFunction = p => 12f * (1f - p),
            ColorFunction = p => Color.Lerp(LamentUtils.GriefGrey, LamentUtils.MourningBlack, p) * (1f - p),
            ShaderKey = null,
            TrailLength = 0.9f
        };
    }

    public class LamentPrimitiveRenderer
    {
        private DynamicVertexBuffer _vertexBuffer;
        private DynamicIndexBuffer _indexBuffer;
        private LamentTrailVertex[] _vertices;
        private short[] _indices;
        private readonly GraphicsDevice _device;

        public LamentPrimitiveRenderer(GraphicsDevice device)
        {
            _device = device;
        }

        public void DrawTrail(List<Vector2> points, LamentTrailSettings settings)
        {
            if (points == null || points.Count < 2) return;

            var smoothed = CatmullRomSmooth(points, 3);
            if (smoothed.Count < 2) return;

            int segCount = smoothed.Count - 1;
            int vertCount = smoothed.Count * 2;
            int indexCount = segCount * 6;

            if (_vertices == null || _vertices.Length < vertCount)
                _vertices = new LamentTrailVertex[vertCount];
            if (_indices == null || _indices.Length < indexCount)
                _indices = new short[indexCount];

            for (int i = 0; i < smoothed.Count; i++)
            {
                float progress = (float)i / (smoothed.Count - 1);
                float width = settings.WidthFunction(progress);
                Color color = settings.ColorFunction(progress);

                Vector2 normal;
                if (i == 0)
                    normal = (smoothed[1] - smoothed[0]).SafeNormalize(Vector2.UnitY);
                else if (i == smoothed.Count - 1)
                    normal = (smoothed[i] - smoothed[i - 1]).SafeNormalize(Vector2.UnitY);
                else
                    normal = (smoothed[i + 1] - smoothed[i - 1]).SafeNormalize(Vector2.UnitY);

                Vector2 perp = new Vector2(-normal.Y, normal.X);
                Vector2 screenPos = smoothed[i] - Main.screenPosition;

                _vertices[i * 2] = new LamentTrailVertex(
                    screenPos + perp * width, color, new Vector3(progress, 0f, width));
                _vertices[i * 2 + 1] = new LamentTrailVertex(
                    screenPos - perp * width, color, new Vector3(progress, 1f, width));
            }

            for (int i = 0; i < segCount; i++)
            {
                int idx = i * 6;
                int vi = i * 2;
                _indices[idx + 0] = (short)vi;
                _indices[idx + 1] = (short)(vi + 1);
                _indices[idx + 2] = (short)(vi + 2);
                _indices[idx + 3] = (short)(vi + 1);
                _indices[idx + 4] = (short)(vi + 3);
                _indices[idx + 5] = (short)(vi + 2);
            }

            EnsureBuffers(vertCount, indexCount);
            _vertexBuffer.SetData(_vertices, 0, vertCount);
            _indexBuffer.SetData(_indices, 0, indexCount);

            _device.SetVertexBuffer(_vertexBuffer);
            _device.Indices = _indexBuffer;

            // Set render states for GPU primitive drawing
            var prevBlend = _device.BlendState;
            var prevDepth = _device.DepthStencilState;
            var prevRaster = _device.RasterizerState;
            _device.BlendState = BlendState.Additive;
            _device.DepthStencilState = DepthStencilState.None;
            _device.RasterizerState = RasterizerState.CullNone;

            // Apply shader if specified
            if (!string.IsNullOrEmpty(settings.ShaderKey) && GameShaders.Misc.ContainsKey(settings.ShaderKey))
            {
                var shader = GameShaders.Misc[settings.ShaderKey];
                var viewProjection = GetViewProjectionMatrix();
                shader.Shader.Parameters["uWorldViewProjection"]?.SetValue(viewProjection);
                shader.Shader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
                shader.Apply();
            }

            _device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertCount, 0, segCount * 2);

            _device.SetVertexBuffer(null);
            _device.Indices = null;
            _device.BlendState = prevBlend;
            _device.DepthStencilState = prevDepth;
            _device.RasterizerState = prevRaster;
        }

        private void EnsureBuffers(int vertCount, int indexCount)
        {
            if (_vertexBuffer == null || _vertexBuffer.IsDisposed || _vertexBuffer.VertexCount < vertCount)
            {
                _vertexBuffer?.Dispose();
                _vertexBuffer = new DynamicVertexBuffer(_device, LamentTrailVertex._vertexDeclaration,
                    Math.Max(vertCount, 64), BufferUsage.WriteOnly);
            }
            if (_indexBuffer == null || _indexBuffer.IsDisposed || _indexBuffer.IndexCount < indexCount)
            {
                _indexBuffer?.Dispose();
                _indexBuffer = new DynamicIndexBuffer(_device, IndexElementSize.SixteenBits,
                    Math.Max(indexCount, 192), BufferUsage.WriteOnly);
            }
        }

        private static Matrix GetViewProjectionMatrix()
        {
            var viewport = Main.graphics.GraphicsDevice.Viewport;
            var zoom = Main.GameViewMatrix.Zoom;
            Matrix projection = Matrix.CreateOrthographicOffCenter(
                0, viewport.Width / zoom.X, viewport.Height / zoom.Y, 0, -1, 1);
            return projection;
        }

        private static List<Vector2> CatmullRomSmooth(List<Vector2> points, int subdivisions)
        {
            if (points.Count < 3) return new List<Vector2>(points);
            var result = new List<Vector2>();

            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector2 p0 = points[Math.Max(i - 1, 0)];
                Vector2 p1 = points[i];
                Vector2 p2 = points[Math.Min(i + 1, points.Count - 1)];
                Vector2 p3 = points[Math.Min(i + 2, points.Count - 1)];

                for (int j = 0; j < subdivisions; j++)
                {
                    float t = (float)j / subdivisions;
                    result.Add(CatmullRom(p0, p1, p2, p3, t));
                }
            }
            result.Add(points[points.Count - 1]);
            return result;
        }

        private static Vector2 CatmullRom(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;
            return 0.5f * (
                (2f * p1) +
                (-p0 + p2) * t +
                (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
                (-p0 + 3f * p1 - 3f * p2 + p3) * t3);
        }

        public void Dispose()
        {
            _vertexBuffer?.Dispose();
            _indexBuffer?.Dispose();
        }
    }
}
