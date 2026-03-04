using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Shaders;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.IgnitionOfTheBell.Primitives
{
    /// <summary>
    /// Per-instance GPU triangle-strip trail renderer for IgnitionOfTheBell.
    /// Builds triangle meshes from position arrays with Catmull-Rom smoothing.
    /// IDisposable — each projectile instance owns one.
    /// </summary>
    public class IgnitionOfTheBellPrimitiveRenderer : IDisposable
    {
        private const int MaxVertices = 2048;
        private const int MaxIndices = 6144;

        private DynamicVertexBuffer _vertexBuffer;
        private DynamicIndexBuffer _indexBuffer;
        private bool _disposed;
        private GraphicsDevice _device;

        #region Trail Settings

        public class IgnitionOfTheBellTrailSettings
        {
            public Func<float, float> WidthFunc;
            public Func<float, Color> ColorFunc;
            public Vector2 Offset;
            public MiscShaderData Shader;
            public bool Smoothen;

            public IgnitionOfTheBellTrailSettings(
                Func<float, float> width = null,
                Func<float, Color> trailColor = null,
                MiscShaderData shader = null,
                bool smoothen = true,
                Vector2? offset = null)
            {
                WidthFunc = width ?? ((t) => 10f);
                ColorFunc = trailColor ?? ((t) => Color.White);
                Shader = shader;
                Smoothen = smoothen;
                Offset = offset ?? Vector2.Zero;
            }
        }

        #endregion

        public void RenderTrail(Vector2[] positions, IgnitionOfTheBellTrailSettings settings, int maxPoints = 50)
        {
            try
            {
                _device = Main.graphics.GraphicsDevice;
                if (_device == null || _device.IsDisposed) return;

                EnsureBuffers();

                Vector2[] filtered = FilterPositions(positions);
                if (filtered.Length < 2) return;

                if (settings.Smoothen && filtered.Length >= 4)
                    filtered = SmoothPositions(filtered, maxPoints);

                float[] completionRatios = ComputeCompletionRatios(filtered);
                BuildAndRenderMesh(filtered, completionRatios, settings);
            }
            catch { }
        }

        #region Pipeline

        private Vector2[] FilterPositions(Vector2[] positions)
        {
            var valid = new List<Vector2>();
            for (int i = 0; i < positions.Length; i++)
            {
                if (positions[i] != Vector2.Zero)
                    valid.Add(positions[i]);
            }
            return valid.ToArray();
        }

        private Vector2[] SmoothPositions(Vector2[] input, int outputCount)
        {
            if (input.Length < 4) return input;
            var result = new Vector2[outputCount];
            for (int i = 0; i < outputCount; i++)
            {
                float t = i / (float)(outputCount - 1) * (input.Length - 1);
                int idx = Math.Min((int)t, input.Length - 2);
                float localT = t - idx;

                Vector2 p0 = input[Math.Max(idx - 1, 0)];
                Vector2 p1 = input[idx];
                Vector2 p2 = input[Math.Min(idx + 1, input.Length - 1)];
                Vector2 p3 = input[Math.Min(idx + 2, input.Length - 1)];

                result[i] = Vector2.CatmullRom(p0, p1, p2, p3, localT);
            }
            return result;
        }

        private float[] ComputeCompletionRatios(Vector2[] positions)
        {
            float[] ratios = new float[positions.Length];
            float totalLength = 0f;
            float[] distances = new float[positions.Length];

            for (int i = 1; i < positions.Length; i++)
            {
                float segLen = Vector2.Distance(positions[i], positions[i - 1]);
                totalLength += segLen;
                distances[i] = totalLength;
            }

            if (totalLength > 0)
            {
                for (int i = 0; i < positions.Length; i++)
                    ratios[i] = distances[i] / totalLength;
            }

            return ratios;
        }

        private void BuildAndRenderMesh(Vector2[] positions, float[] completionRatios, IgnitionOfTheBellTrailSettings settings)
        {
            int vertCount = positions.Length * 2;
            if (vertCount > MaxVertices || vertCount < 4) return;

            var vertices = new IgnitionOfTheBellVertex[vertCount];
            var indices = new short[(positions.Length - 1) * 6];

            Vector2 screenCenter = Main.screenPosition;

            for (int i = 0; i < positions.Length; i++)
            {
                float ratio = completionRatios[i];
                float width = settings.WidthFunc(ratio);
                Color color = settings.ColorFunc(ratio);

                Vector2 normal;
                if (i < positions.Length - 1)
                    normal = (positions[i + 1] - positions[i]);
                else
                    normal = (positions[i] - positions[i - 1]);

                normal = new Vector2(-normal.Y, normal.X);
                if (normal.Length() > 0) normal.Normalize();

                Vector2 worldPos = positions[i] + settings.Offset;
                Vector2 top = worldPos + normal * width * 0.5f - screenCenter;
                Vector2 bottom = worldPos - normal * width * 0.5f - screenCenter;

                vertices[i * 2] = new IgnitionOfTheBellVertex(top, color, new Vector3(ratio, 0f, ratio));
                vertices[i * 2 + 1] = new IgnitionOfTheBellVertex(bottom, color, new Vector3(ratio, 1f, ratio));
            }

            int idx = 0;
            for (int i = 0; i < positions.Length - 1; i++)
            {
                short tl = (short)(i * 2);
                short bl = (short)(i * 2 + 1);
                short tr = (short)((i + 1) * 2);
                short br = (short)((i + 1) * 2 + 1);

                indices[idx++] = tl; indices[idx++] = tr; indices[idx++] = bl;
                indices[idx++] = bl; indices[idx++] = tr; indices[idx++] = br;
            }

            RenderToGPU(vertices, indices, vertCount, (positions.Length - 1) * 2, settings);
        }

        private void RenderToGPU(IgnitionOfTheBellVertex[] vertices, short[] indices, int vertCount, int triCount, IgnitionOfTheBellTrailSettings settings)
        {
            var oldBlendState = _device.BlendState;
            var oldRasterizer = _device.RasterizerState;
            var oldDepthStencil = _device.DepthStencilState;

            try
            {
                _vertexBuffer.SetData(vertices, 0, vertCount, SetDataOptions.Discard);
                _indexBuffer.SetData(indices, 0, triCount * 3, SetDataOptions.Discard);

                _device.SetVertexBuffer(_vertexBuffer);
                _device.Indices = _indexBuffer;

                _device.RasterizerState = RasterizerState.CullNone;
                _device.BlendState = BlendState.Additive;
                _device.DepthStencilState = DepthStencilState.None;

                if (settings.Shader != null)
                {
                    Matrix view = Matrix.CreateLookAt(Vector3.Zero, Vector3.UnitZ, Vector3.Up);
                    Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);
                    settings.Shader.Shader.Parameters["uWorldViewProjection"]?.SetValue(view * projection);
                    try { settings.Shader.Apply(); }
                    catch { }
                }
                else
                {
                    // Fallback: BasicEffect so GPU has a valid shader pipeline
                    var basicEffect = new BasicEffect(_device)
                    {
                        VertexColorEnabled = true,
                        TextureEnabled = false,
                        World = Matrix.Identity,
                        View = Matrix.CreateLookAt(Vector3.Zero, Vector3.UnitZ, Vector3.Up),
                        Projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1)
                    };
                    basicEffect.CurrentTechnique.Passes[0].Apply();
                }

                _device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertCount, 0, triCount);
            }
            catch { }
            finally
            {
                _device.BlendState = oldBlendState;
                _device.RasterizerState = oldRasterizer;
                _device.DepthStencilState = oldDepthStencil;
            }
        }

        #endregion

        #region Buffer Management

        private void EnsureBuffers()
        {
            if (_vertexBuffer == null || _vertexBuffer.IsDisposed)
                _vertexBuffer = new DynamicVertexBuffer(_device, typeof(IgnitionOfTheBellVertex), MaxVertices, BufferUsage.WriteOnly);

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

        #endregion
    }
}
