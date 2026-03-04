using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Shaders;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.DualFatedChime.Primitives
{
    /// <summary>
    /// Immutable configuration for a Dual Fated Chime primitive trail.
    /// </summary>
    public readonly struct DualFatedChimeTrailSettings
    {
        public delegate float WidthFunc(float completionRatio);
        public delegate Color ColorFunc(float completionRatio);

        public readonly WidthFunc Width;
        public readonly ColorFunc TrailColor;
        public readonly Func<float, Vector2> Offset;
        public readonly MiscShaderData Shader;
        public readonly bool Smoothen;

        public DualFatedChimeTrailSettings(
            WidthFunc width,
            ColorFunc trailColor,
            Func<float, Vector2> offset = null,
            MiscShaderData shader = null,
            bool smoothen = true)
        {
            Width = width;
            TrailColor = trailColor;
            Offset = offset;
            Shader = shader;
            Smoothen = smoothen;
        }
    }

    /// <summary>
    /// Self-contained GPU primitive trail renderer for Dual Fated Chime.
    /// Builds a triangle-strip ribbon mesh from position arrays and renders with shaders.
    /// </summary>
    public class DualFatedChimePrimitiveRenderer : IDisposable
    {
        private const int MaxVertices = 2048;
        private const int MaxIndices = 6144;

        private DynamicVertexBuffer _vertexBuffer;
        private DynamicIndexBuffer _indexBuffer;
        private DualFatedChimeVertex[] _vertices;
        private short[] _indices;
        private bool _disposed;

        public DualFatedChimePrimitiveRenderer()
        {
            _vertices = new DualFatedChimeVertex[MaxVertices];
            _indices = new short[MaxIndices];
        }

        private void EnsureBuffers(GraphicsDevice device)
        {
            if (_vertexBuffer == null || _vertexBuffer.IsDisposed)
                _vertexBuffer = new DynamicVertexBuffer(device, DualFatedChimeVertex.Declaration, MaxVertices, BufferUsage.WriteOnly);
            if (_indexBuffer == null || _indexBuffer.IsDisposed)
                _indexBuffer = new DynamicIndexBuffer(device, IndexElementSize.SixteenBits, MaxIndices, BufferUsage.WriteOnly);
        }

        public void RenderTrail(Vector2[] positions, DualFatedChimeTrailSettings settings, int? renderPoints = null)
        {
            if (positions == null || positions.Length < 2)
                return;

            Vector2[] validPositions = FilterPositions(positions);
            if (validPositions.Length < 2)
                return;

            int numPoints = renderPoints ?? validPositions.Length;
            numPoints = Math.Min(numPoints, validPositions.Length);

            Vector2[] sampledPositions = settings.Smoothen && numPoints > 2
                ? SmoothPositions(validPositions, numPoints)
                : ResamplePositions(validPositions, numPoints);

            if (sampledPositions.Length < 2)
                return;

            float[] completionRatios = ComputeCompletionRatios(sampledPositions);

            int vertexCount = 0;
            int indexCount = 0;

            for (int i = 0; i < sampledPositions.Length; i++)
            {
                float t = completionRatios[i];
                float halfWidth = settings.Width(t) * 0.5f;
                Color color = settings.TrailColor(t);

                Vector2 tangent;
                if (i == 0)
                    tangent = sampledPositions[1] - sampledPositions[0];
                else if (i == sampledPositions.Length - 1)
                    tangent = sampledPositions[i] - sampledPositions[i - 1];
                else
                    tangent = sampledPositions[i + 1] - sampledPositions[i - 1];

                if (tangent.LengthSquared() < 0.0001f)
                    tangent = Vector2.UnitX;
                tangent.Normalize();

                Vector2 normal = new Vector2(-tangent.Y, tangent.X);
                Vector2 screenPos = sampledPositions[i] - Main.screenPosition;
                if (settings.Offset != null)
                    screenPos += settings.Offset(t);

                Vector2 left = screenPos + normal * halfWidth;
                Vector2 right = screenPos - normal * halfWidth;

                float u = t;
                float widthCorrection = Math.Max(halfWidth, 0.01f);

                int vi = vertexCount;
                _vertices[vi] = new DualFatedChimeVertex(left, color, new Vector3(u, 0f, widthCorrection));
                _vertices[vi + 1] = new DualFatedChimeVertex(right, color, new Vector3(u, 1f, widthCorrection));
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

                if (vertexCount >= MaxVertices - 2 || indexCount >= MaxIndices - 6)
                    break;
            }

            if (vertexCount < 4 || indexCount < 6)
                return;

            GraphicsDevice device = Main.graphics.GraphicsDevice;
            if (device == null || device.IsDisposed)
                return;

            EnsureBuffers(device);

            // Save all graphics states for proper restoration
            var oldRasterizer = device.RasterizerState;
            var oldBlendState = device.BlendState;
            var oldDepthStencil = device.DepthStencilState;
            var oldSampler0 = device.SamplerStates[0];

            device.RasterizerState = RasterizerState.CullNone;
            device.BlendState = BlendState.Additive;
            device.DepthStencilState = DepthStencilState.None;
            device.SamplerStates[0] = SamplerState.LinearWrap;

            Matrix view = Matrix.CreateLookAt(Vector3.Zero, Vector3.UnitZ, Vector3.Up);
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);
            Matrix transform = view * projection;

            try
            {
                if (settings.Shader != null)
                {
                    settings.Shader.Shader.Parameters["uWorldViewProjection"]?.SetValue(transform);
                    settings.Shader.Apply();
                }
                else
                {
                    // Fallback: use a BasicEffect so the GPU has a valid shader pipeline
                    var basicEffect = new BasicEffect(device)
                    {
                        VertexColorEnabled = true,
                        TextureEnabled = false,
                        World = Matrix.Identity,
                        View = view,
                        Projection = projection
                    };
                    basicEffect.CurrentTechnique.Passes[0].Apply();
                }

                _vertexBuffer.SetData(_vertices, 0, vertexCount, SetDataOptions.Discard);
                _indexBuffer.SetData(_indices, 0, indexCount, SetDataOptions.Discard);

                device.SetVertexBuffer(_vertexBuffer);
                device.Indices = _indexBuffer;
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexCount, 0, indexCount / 3);
            }
            finally
            {
                // Restore all graphics states
                device.RasterizerState = oldRasterizer;
                device.BlendState = oldBlendState;
                device.DepthStencilState = oldDepthStencil;
                device.SamplerStates[0] = oldSampler0;
            }
        }

        #region Position Processing

        private static Vector2[] FilterPositions(Vector2[] positions)
        {
            var result = new List<Vector2>(positions.Length);
            foreach (var pos in positions)
            {
                if (pos != Vector2.Zero)
                    result.Add(pos);
            }
            return result.ToArray();
        }

        private static float[] ComputeCompletionRatios(Vector2[] positions)
        {
            float[] ratios = new float[positions.Length];
            if (positions.Length <= 1)
            {
                ratios[0] = 0f;
                return ratios;
            }

            float totalLength = 0f;
            for (int i = 1; i < positions.Length; i++)
                totalLength += Vector2.Distance(positions[i - 1], positions[i]);

            if (totalLength < 0.001f)
            {
                for (int i = 0; i < positions.Length; i++)
                    ratios[i] = (float)i / (positions.Length - 1);
                return ratios;
            }

            float accumulated = 0f;
            ratios[0] = 0f;
            for (int i = 1; i < positions.Length; i++)
            {
                accumulated += Vector2.Distance(positions[i - 1], positions[i]);
                ratios[i] = accumulated / totalLength;
            }

            return ratios;
        }

        private static Vector2[] ResamplePositions(Vector2[] positions, int targetCount)
        {
            if (positions.Length == targetCount)
                return positions;

            Vector2[] result = new Vector2[targetCount];
            for (int i = 0; i < targetCount; i++)
            {
                float t = (float)i / (targetCount - 1);
                float idx = t * (positions.Length - 1);
                int lo = (int)idx;
                int hi = Math.Min(lo + 1, positions.Length - 1);
                float frac = idx - lo;
                result[i] = Vector2.Lerp(positions[lo], positions[hi], frac);
            }
            return result;
        }

        private static Vector2[] SmoothPositions(Vector2[] positions, int outputCount)
        {
            if (positions.Length < 3)
                return ResamplePositions(positions, outputCount);

            Vector2[] result = new Vector2[outputCount];
            for (int i = 0; i < outputCount; i++)
            {
                float t = (float)i / (outputCount - 1);
                float scaled = t * (positions.Length - 1);
                int p1 = (int)scaled;
                int p0 = Math.Max(p1 - 1, 0);
                int p2 = Math.Min(p1 + 1, positions.Length - 1);
                int p3 = Math.Min(p1 + 2, positions.Length - 1);
                float frac = scaled - p1;

                result[i] = CatmullRom(positions[p0], positions[p1], positions[p2], positions[p3], frac);
            }
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
                (-p0 + 3f * p1 - 3f * p2 + p3) * t3
            );
        }

        #endregion

        public void Dispose()
        {
            if (!_disposed)
            {
                _vertexBuffer?.Dispose();
                _indexBuffer?.Dispose();
                _disposed = true;
            }
        }
    }
}
