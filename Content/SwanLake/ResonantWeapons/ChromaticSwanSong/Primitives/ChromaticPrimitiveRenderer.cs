using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.ChromaticSwanSong.Primitives
{
    public readonly struct ChromaticVertex : IVertexType
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

        public ChromaticVertex(Vector2 position, Color color, Vector3 texCoords)
        {
            Position = position; Color = color; TextureCoordinates = texCoords;
        }
    }

    public readonly struct ChromaticTrailSettings
    {
        public delegate float WidthFunc(float completionRatio);
        public delegate Color ColorFunc(float completionRatio);

        public readonly WidthFunc Width;
        public readonly ColorFunc TrailColor;
        public readonly Terraria.Graphics.Shaders.MiscShaderData Shader;

        public ChromaticTrailSettings(WidthFunc width, ColorFunc trailColor,
            Terraria.Graphics.Shaders.MiscShaderData shader = null)
        {
            Width = width; TrailColor = trailColor; Shader = shader;
        }
    }

    public class ChromaticPrimitiveRenderer : IDisposable
    {
        private const int MaxVertices = 2048;
        private const int MaxIndices = 6144;
        private DynamicVertexBuffer _vertexBuffer;
        private DynamicIndexBuffer _indexBuffer;
        private ChromaticVertex[] _vertices;
        private short[] _indices;
        private bool _disposed;

        public ChromaticPrimitiveRenderer()
        {
            _vertices = new ChromaticVertex[MaxVertices];
            _indices = new short[MaxIndices];
        }

        private void EnsureBuffers(GraphicsDevice device)
        {
            if (_vertexBuffer == null || _vertexBuffer.IsDisposed)
                _vertexBuffer = new DynamicVertexBuffer(device, ChromaticVertex.Declaration, MaxVertices, BufferUsage.WriteOnly);
            if (_indexBuffer == null || _indexBuffer.IsDisposed)
                _indexBuffer = new DynamicIndexBuffer(device, IndexElementSize.SixteenBits, MaxIndices, BufferUsage.WriteOnly);
        }

        public void RenderTrail(Vector2[] positions, ChromaticTrailSettings settings, int? pointCount = null)
        {
            if (positions == null || positions.Length < 2) return;

            var valid = new System.Collections.Generic.List<Vector2>();
            foreach (var p in positions) if (p != Vector2.Zero) valid.Add(p);
            if (valid.Count < 2) return;

            int count = Math.Min(pointCount ?? valid.Count, valid.Count);
            Vector2[] pts = Resample(valid.ToArray(), count);
            if (pts.Length < 2) return;

            float totalDist = 0f;
            for (int i = 1; i < pts.Length; i++) totalDist += Vector2.Distance(pts[i - 1], pts[i]);

            int vc = 0, ic = 0;
            float acc = 0f;

            for (int i = 0; i < pts.Length; i++)
            {
                float t = totalDist > 0.001f ? acc / totalDist : (float)i / (pts.Length - 1);
                if (i > 0) acc += Vector2.Distance(pts[i - 1], pts[i]);

                float hw = settings.Width(t) * 0.5f;
                Color col = settings.TrailColor(t);

                Vector2 tan = i == 0 ? pts[1] - pts[0]
                    : i == pts.Length - 1 ? pts[i] - pts[i - 1]
                    : pts[i + 1] - pts[i - 1];
                if (tan.LengthSquared() < 0.0001f) tan = Vector2.UnitX;
                tan.Normalize();
                Vector2 norm = new Vector2(-tan.Y, tan.X);

                Vector2 sp = pts[i] - Main.screenPosition;
                _vertices[vc] = new ChromaticVertex(sp + norm * hw, col, new Vector3(t, 0f, hw));
                _vertices[vc + 1] = new ChromaticVertex(sp - norm * hw, col, new Vector3(t, 1f, hw));
                if (i > 0)
                {
                    int pv = vc - 2;
                    _indices[ic++] = (short)pv; _indices[ic++] = (short)(pv + 1); _indices[ic++] = (short)vc;
                    _indices[ic++] = (short)(pv + 1); _indices[ic++] = (short)(vc + 1); _indices[ic++] = (short)vc;
                }
                vc += 2;
                if (vc >= MaxVertices - 2 || ic >= MaxIndices - 6) break;
            }

            if (vc < 4 || ic < 6) return;

            GraphicsDevice dev = Main.graphics.GraphicsDevice;
            EnsureBuffers(dev);
            var oldRast = dev.RasterizerState;
            dev.RasterizerState = RasterizerState.CullNone;

            Matrix xform = Matrix.CreateLookAt(Vector3.Zero, Vector3.UnitZ, Vector3.Up) *
                Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            if (settings.Shader != null)
            {
                settings.Shader.Shader.Parameters["uWorldViewProjection"]?.SetValue(xform);
                settings.Shader.Apply();
            }

            _vertexBuffer.SetData(_vertices, 0, vc, SetDataOptions.Discard);
            _indexBuffer.SetData(_indices, 0, ic, SetDataOptions.Discard);
            dev.SetVertexBuffer(_vertexBuffer);
            dev.Indices = _indexBuffer;
            dev.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vc, 0, ic / 3);
            dev.RasterizerState = oldRast;
        }

        private static Vector2[] Resample(Vector2[] src, int n)
        {
            if (src.Length == n) return src;
            var res = new Vector2[n];
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / (n - 1);
                float idx = t * (src.Length - 1);
                int lo = (int)idx;
                int hi = Math.Min(lo + 1, src.Length - 1);
                res[i] = Vector2.Lerp(src[lo], src[hi], idx - lo);
            }
            return res;
        }

        public void Dispose()
        {
            if (!_disposed) { _vertexBuffer?.Dispose(); _indexBuffer?.Dispose(); _disposed = true; }
        }
    }
}
