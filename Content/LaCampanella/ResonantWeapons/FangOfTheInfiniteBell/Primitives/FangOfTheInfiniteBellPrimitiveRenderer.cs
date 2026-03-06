using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Shaders;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.FangOfTheInfiniteBell.Primitives
{
    public class FangOfTheInfiniteBellPrimitiveRenderer : IDisposable
    {
        private const int MaxVertices = 2048;
        private const int MaxIndices = 6144;
        private DynamicVertexBuffer _vertexBuffer;
        private DynamicIndexBuffer _indexBuffer;
        private bool _disposed;
        private GraphicsDevice _device;

        public class FangTrailSettings
        {
            public Func<float, float> WidthFunc;
            public Func<float, Color> ColorFunc;
            public MiscShaderData Shader;
            public bool Smoothen;

            public FangTrailSettings(Func<float, float> width = null, Func<float, Color> color = null,
                MiscShaderData shader = null, bool smoothen = true)
            {
                WidthFunc = width ?? (t => 10f);
                ColorFunc = color ?? (t => Color.White);
                Shader = shader;
                Smoothen = smoothen;
            }
        }

        public void RenderTrail(Vector2[] positions, FangTrailSettings settings, int maxPoints = 50)
        {
            try
            {
                _device = Main.graphics.GraphicsDevice;
                if (_device == null || _device.IsDisposed) return;
                EnsureBuffers();

                var filtered = FilterPositions(positions);
                if (filtered.Length < 2) return;

                if (settings.Smoothen && filtered.Length >= 4)
                    filtered = SmoothPositions(filtered, maxPoints);

                float[] ratios = ComputeRatios(filtered);
                BuildAndRender(filtered, ratios, settings);
            }
            catch { }
        }

        private Vector2[] FilterPositions(Vector2[] positions)
        {
            var valid = new List<Vector2>();
            foreach (var p in positions) if (p != Vector2.Zero) valid.Add(p);
            return valid.ToArray();
        }

        private Vector2[] SmoothPositions(Vector2[] input, int count)
        {
            if (input.Length < 4) return input;
            var result = new Vector2[count];
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)(count - 1) * (input.Length - 1);
                int idx = Math.Min((int)t, input.Length - 2);
                float lt = t - idx;
                result[i] = Vector2.CatmullRom(
                    input[Math.Max(idx - 1, 0)], input[idx],
                    input[Math.Min(idx + 1, input.Length - 1)],
                    input[Math.Min(idx + 2, input.Length - 1)], lt);
            }
            return result;
        }

        private float[] ComputeRatios(Vector2[] positions)
        {
            float[] ratios = new float[positions.Length];
            float total = 0f;
            float[] dists = new float[positions.Length];
            for (int i = 1; i < positions.Length; i++)
            {
                total += Vector2.Distance(positions[i], positions[i - 1]);
                dists[i] = total;
            }
            if (total > 0) for (int i = 0; i < positions.Length; i++) ratios[i] = dists[i] / total;
            return ratios;
        }

        private void BuildAndRender(Vector2[] positions, float[] ratios, FangTrailSettings settings)
        {
            int vertCount = positions.Length * 2;
            if (vertCount > MaxVertices || vertCount < 4) return;

            var vertices = new FangOfTheInfiniteBellVertex[vertCount];
            var indices = new short[(positions.Length - 1) * 6];
            Vector2 screen = Main.screenPosition;

            for (int i = 0; i < positions.Length; i++)
            {
                float r = ratios[i];
                float w = settings.WidthFunc(r);
                Color c = settings.ColorFunc(r);

                Vector2 normal = i < positions.Length - 1
                    ? positions[i + 1] - positions[i]
                    : positions[i] - positions[i - 1];
                normal = new Vector2(-normal.Y, normal.X);
                if (normal.Length() > 0) normal.Normalize();

                Vector2 top = positions[i] + normal * w * 0.5f - screen;
                Vector2 bot = positions[i] - normal * w * 0.5f - screen;
                vertices[i * 2] = new FangOfTheInfiniteBellVertex(top, c, new Vector3(r, 0f, r));
                vertices[i * 2 + 1] = new FangOfTheInfiniteBellVertex(bot, c, new Vector3(r, 1f, r));
            }

            int idx = 0;
            for (int i = 0; i < positions.Length - 1; i++)
            {
                short tl = (short)(i * 2), bl = (short)(i * 2 + 1);
                short tr = (short)((i + 1) * 2), br = (short)((i + 1) * 2 + 1);
                indices[idx++] = tl; indices[idx++] = tr; indices[idx++] = bl;
                indices[idx++] = bl; indices[idx++] = tr; indices[idx++] = br;
            }

            BlendState oldBlend = _device.BlendState;
            DepthStencilState oldDepth = _device.DepthStencilState;
            RasterizerState oldRaster = _device.RasterizerState;
            try
            {
                _vertexBuffer.SetData(vertices, 0, vertCount, SetDataOptions.Discard);
                _indexBuffer.SetData(indices, 0, (positions.Length - 1) * 6, SetDataOptions.Discard);
                _device.SetVertexBuffer(_vertexBuffer);
                _device.Indices = _indexBuffer;
                _device.BlendState = MagnumBlendStates.TrueAdditive;
                _device.DepthStencilState = DepthStencilState.None;
                _device.RasterizerState = RasterizerState.CullNone;
                if (settings.Shader != null && settings.Shader.Shader != null)
                {
                    Matrix view = Matrix.CreateLookAt(Vector3.Zero, Vector3.UnitZ, Vector3.Up);
                    Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);
                    Matrix wvp = view * projection;
                    settings.Shader.Shader.Parameters["uWorldViewProjection"]?.SetValue(wvp);
                    try { settings.Shader.Apply(); } catch { }
                }
                else
                {
                    return;
                }
                _device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertCount, 0, (positions.Length - 1) * 2);
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
            if (_vertexBuffer == null || _vertexBuffer.IsDisposed)
                _vertexBuffer = new DynamicVertexBuffer(_device, typeof(FangOfTheInfiniteBellVertex), MaxVertices, BufferUsage.WriteOnly);
            if (_indexBuffer == null || _indexBuffer.IsDisposed)
                _indexBuffer = new DynamicIndexBuffer(_device, IndexElementSize.SixteenBits, MaxIndices, BufferUsage.WriteOnly);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _vertexBuffer?.Dispose();
            _indexBuffer?.Dispose();
        }
    }
}
