using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.FeatheroftheIridescentFlock.Primitives
{
    public readonly struct FlockVertex : IVertexType
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

        public FlockVertex(Vector2 position, Color color, Vector3 texCoords)
        { Position = position; Color = color; TextureCoordinates = texCoords; }
    }

    public readonly struct FlockTrailSettings
    {
        public delegate float WidthFunc(float t);
        public delegate Color ColorFunc(float t);
        public readonly WidthFunc Width;
        public readonly ColorFunc TrailColor;
        public readonly Terraria.Graphics.Shaders.MiscShaderData Shader;

        public FlockTrailSettings(WidthFunc width, ColorFunc trailColor,
            Terraria.Graphics.Shaders.MiscShaderData shader = null)
        { Width = width; TrailColor = trailColor; Shader = shader; }
    }

    public class FlockPrimitiveRenderer : IDisposable
    {
        private const int MaxV = 2048, MaxI = 6144;
        private DynamicVertexBuffer _vb;
        private DynamicIndexBuffer _ib;
        private FlockVertex[] _v = new FlockVertex[MaxV];
        private short[] _ix = new short[MaxI];
        private bool _disposed;

        private void EnsureBuffers(GraphicsDevice gd)
        {
            if (_vb == null || _vb.IsDisposed) _vb = new DynamicVertexBuffer(gd, FlockVertex.Declaration, MaxV, BufferUsage.WriteOnly);
            if (_ib == null || _ib.IsDisposed) _ib = new DynamicIndexBuffer(gd, IndexElementSize.SixteenBits, MaxI, BufferUsage.WriteOnly);
        }

        public void RenderTrail(Vector2[] positions, FlockTrailSettings settings, int? pts = null)
        {
            if (positions == null || positions.Length < 2) return;
            var valid = new System.Collections.Generic.List<Vector2>();
            foreach (var p in positions) if (p != Vector2.Zero) valid.Add(p);
            if (valid.Count < 2) return;

            int count = Math.Min(pts ?? valid.Count, valid.Count);
            var arr = Resample(valid.ToArray(), count);
            if (arr.Length < 2) return;

            float totalDist = 0f;
            for (int i = 1; i < arr.Length; i++) totalDist += Vector2.Distance(arr[i - 1], arr[i]);

            int vc = 0, ic = 0; float acc = 0f;
            for (int i = 0; i < arr.Length; i++)
            {
                float t = totalDist > 0.001f ? acc / totalDist : (float)i / (arr.Length - 1);
                if (i > 0) acc += Vector2.Distance(arr[i - 1], arr[i]);
                float hw = settings.Width(t) * 0.5f;
                Color col = settings.TrailColor(t);
                Vector2 tan = i == 0 ? arr[1] - arr[0] : i == arr.Length - 1 ? arr[i] - arr[i - 1] : arr[i + 1] - arr[i - 1];
                if (tan.LengthSquared() < 0.0001f) tan = Vector2.UnitX;
                tan.Normalize();
                Vector2 n = new Vector2(-tan.Y, tan.X);
                Vector2 sp = arr[i] - Main.screenPosition;
                _v[vc] = new FlockVertex(sp + n * hw, col, new Vector3(t, 0f, hw));
                _v[vc + 1] = new FlockVertex(sp - n * hw, col, new Vector3(t, 1f, hw));
                if (i > 0) { int pv = vc - 2; _ix[ic++] = (short)pv; _ix[ic++] = (short)(pv+1); _ix[ic++] = (short)vc; _ix[ic++] = (short)(pv+1); _ix[ic++] = (short)(vc+1); _ix[ic++] = (short)vc; }
                vc += 2;
                if (vc >= MaxV - 2 || ic >= MaxI - 6) break;
            }
            if (vc < 4 || ic < 6) return;

            var dev = Main.graphics.GraphicsDevice;
            EnsureBuffers(dev);
            var oldR = dev.RasterizerState; dev.RasterizerState = RasterizerState.CullNone;
            Matrix xf = Matrix.CreateLookAt(Vector3.Zero, Vector3.UnitZ, Vector3.Up) * Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);
            if (settings.Shader != null) { settings.Shader.Shader.Parameters["uWorldViewProjection"]?.SetValue(xf); settings.Shader.Apply(); }
            _vb.SetData(_v, 0, vc, SetDataOptions.Discard);
            _ib.SetData(_ix, 0, ic, SetDataOptions.Discard);
            dev.SetVertexBuffer(_vb); dev.Indices = _ib;
            dev.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vc, 0, ic / 3);
            dev.RasterizerState = oldR;
        }

        private static Vector2[] Resample(Vector2[] s, int n)
        {
            if (s.Length == n) return s;
            var r = new Vector2[n];
            for (int i = 0; i < n; i++) { float t = (float)i / (n - 1); float idx = t * (s.Length - 1); int lo = (int)idx; r[i] = Vector2.Lerp(s[lo], s[Math.Min(lo + 1, s.Length - 1)], idx - lo); }
            return r;
        }

        public void Dispose() { if (!_disposed) { _vb?.Dispose(); _ib?.Dispose(); _disposed = true; } }
    }
}
