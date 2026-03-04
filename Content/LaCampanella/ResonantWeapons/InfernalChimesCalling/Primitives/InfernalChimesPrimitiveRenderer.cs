using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Shaders;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.InfernalChimesCalling.Primitives
{
    public class InfernalChimesPrimitiveRenderer : IDisposable
    {
        private const int MaxVertices = 2048;
        private const int MaxIndices = 6144;
        private DynamicVertexBuffer _vb;
        private DynamicIndexBuffer _ib;
        private bool _disposed;
        private GraphicsDevice _device;

        public class ChimesTrailSettings
        {
            public Func<float, float> WidthFunc;
            public Func<float, Color> ColorFunc;
            public MiscShaderData Shader;
            public bool Smoothen;
            public ChimesTrailSettings(Func<float, float> w = null, Func<float, Color> c = null, MiscShaderData s = null, bool smooth = true)
            { WidthFunc = w ?? (t => 10f); ColorFunc = c ?? (t => Color.White); Shader = s; Smoothen = smooth; }
        }

        public void RenderTrail(Vector2[] positions, ChimesTrailSettings settings, int maxPts = 50)
        {
            try
            {
                _device = Main.graphics.GraphicsDevice;
                if (_device == null || _device.IsDisposed) return;
                EnsureBuffers();
                var filtered = Filter(positions);
                if (filtered.Length < 2) return;
                if (settings.Smoothen && filtered.Length >= 4) filtered = Smooth(filtered, maxPts);
                float[] ratios = Ratios(filtered);
                Render(filtered, ratios, settings);
            }
            catch { }
        }

        private Vector2[] Filter(Vector2[] p) { var v = new List<Vector2>(); foreach (var pt in p) if (pt != Vector2.Zero) v.Add(pt); return v.ToArray(); }

        private Vector2[] Smooth(Vector2[] input, int count)
        {
            if (input.Length < 4) return input;
            var r = new Vector2[count];
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)(count - 1) * (input.Length - 1);
                int idx = Math.Min((int)t, input.Length - 2); float lt = t - idx;
                r[i] = Vector2.CatmullRom(input[Math.Max(idx - 1, 0)], input[idx], input[Math.Min(idx + 1, input.Length - 1)], input[Math.Min(idx + 2, input.Length - 1)], lt);
            }
            return r;
        }

        private float[] Ratios(Vector2[] p)
        {
            float[] r = new float[p.Length]; float total = 0; float[] d = new float[p.Length];
            for (int i = 1; i < p.Length; i++) { total += Vector2.Distance(p[i], p[i - 1]); d[i] = total; }
            if (total > 0) for (int i = 0; i < p.Length; i++) r[i] = d[i] / total;
            return r;
        }

        private void Render(Vector2[] pos, float[] ratios, ChimesTrailSettings s)
        {
            int vc = pos.Length * 2;
            if (vc > MaxVertices || vc < 4) return;
            var verts = new InfernalChimesVertex[vc];
            var indices = new short[(pos.Length - 1) * 6];
            Vector2 scr = Main.screenPosition;
            for (int i = 0; i < pos.Length; i++)
            {
                float r = ratios[i]; float w = s.WidthFunc(r); Color c = s.ColorFunc(r);
                Vector2 n = i < pos.Length - 1 ? pos[i + 1] - pos[i] : pos[i] - pos[i - 1];
                n = new Vector2(-n.Y, n.X); if (n.Length() > 0) n.Normalize();
                verts[i * 2] = new InfernalChimesVertex(pos[i] + n * w * 0.5f - scr, c, new Vector3(r, 0, r));
                verts[i * 2 + 1] = new InfernalChimesVertex(pos[i] - n * w * 0.5f - scr, c, new Vector3(r, 1, r));
            }
            int idx = 0;
            for (int i = 0; i < pos.Length - 1; i++)
            {
                short tl = (short)(i * 2), bl = (short)(i * 2 + 1), tr = (short)((i + 1) * 2), br = (short)((i + 1) * 2 + 1);
                indices[idx++] = tl; indices[idx++] = tr; indices[idx++] = bl;
                indices[idx++] = bl; indices[idx++] = tr; indices[idx++] = br;
            }
            BlendState oldBlend = _device.BlendState;
            DepthStencilState oldDepth = _device.DepthStencilState;
            RasterizerState oldRaster = _device.RasterizerState;
            try
            {
                _vb.SetData(verts, 0, vc, SetDataOptions.Discard);
                _ib.SetData(indices, 0, (pos.Length - 1) * 6, SetDataOptions.Discard);
                _device.SetVertexBuffer(_vb); _device.Indices = _ib;
                _device.BlendState = BlendState.Additive;
                _device.DepthStencilState = DepthStencilState.None;
                _device.RasterizerState = RasterizerState.CullNone;
                if (s.Shader != null)
                {
                    try { s.Shader.Apply(); } catch { }
                }
                else
                {
                    var basicEffect = new BasicEffect(_device)
                    {
                        VertexColorEnabled = true,
                        TextureEnabled = false,
                        World = Matrix.Identity,
                        View = Matrix.Identity,
                        Projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1)
                    };
                    foreach (var pass in basicEffect.CurrentTechnique.Passes) pass.Apply();
                }
                _device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vc, 0, (pos.Length - 1) * 2);
            }
            catch { }
            finally
            {
                _device.BlendState = oldBlend;
                _device.DepthStencilState = oldDepth;
                _device.RasterizerState = oldRaster;
            }
        }

        private void EnsureBuffers()
        {
            if (_vb == null || _vb.IsDisposed) _vb = new DynamicVertexBuffer(_device, typeof(InfernalChimesVertex), MaxVertices, BufferUsage.WriteOnly);
            if (_ib == null || _ib.IsDisposed) _ib = new DynamicIndexBuffer(_device, IndexElementSize.SixteenBits, MaxIndices, BufferUsage.WriteOnly);
        }

        public void Dispose() { if (_disposed) return; _disposed = true; _vb?.Dispose(); _ib?.Dispose(); }
    }
}
