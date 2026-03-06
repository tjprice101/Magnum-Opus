using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics.Shaders;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.Fate.ResonantWeapons.CodaOfAnnihilation.Utilities;

namespace MagnumOpus.Content.Fate.ResonantWeapons.CodaOfAnnihilation.Primitives
{
    /// <summary>
    /// Immutable configuration for a Coda primitive trail.
    /// Provides ZenithTrail and SwingTrail presets.
    /// </summary>
    public readonly struct CodaTrailSettings
    {
        public delegate float WidthFunc(float completionRatio);
        public delegate Color ColorFunc(float completionRatio);

        public readonly WidthFunc Width;
        public readonly ColorFunc TrailColor;
        public readonly Func<float, Vector2> Offset;
        public readonly MiscShaderData Shader;
        public readonly bool Smoothen;

        public CodaTrailSettings(
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

        #region Presets

        /// <summary>Zenith flying sword trail: wide at head, elegant taper, annihilation gradient.</summary>
        public static CodaTrailSettings ZenithTrail(Color weaponColor, MiscShaderData shader = null)
        {
            return new CodaTrailSettings(
                completionRatio =>
                {
                    float head = 1f - (float)Math.Pow(completionRatio, 0.6f);
                    return MathHelper.Lerp(2f, 18f, head);
                },
                completionRatio =>
                {
                    float fade = 1f - (float)Math.Pow(completionRatio, 1.3f);
                    Color c = Color.Lerp(weaponColor, CodaUtils.AnnihilationWhite, completionRatio * 0.3f);
                    return CodaUtils.Additive(c, fade);
                },
                shader: shader,
                smoothen: true
            );
        }

        /// <summary>Held swing arc trail: broad sweep tapering toward start, cosmic gradient.</summary>
        public static CodaTrailSettings SwingTrail(MiscShaderData shader = null)
        {
            return new CodaTrailSettings(
                completionRatio =>
                {
                    float head = 1f - (float)Math.Pow(completionRatio, 0.5f);
                    return MathHelper.Lerp(3f, 28f, head);
                },
                completionRatio =>
                {
                    float fade = 1f - (float)Math.Pow(completionRatio, 1.3f);
                    Color c = CodaUtils.GetAnnihilationGradient(0.2f + completionRatio * 0.6f);
                    return CodaUtils.Additive(c, fade);
                },
                shader: shader,
                smoothen: true
            );
        }

        #endregion
    }

    /// <summary>
    /// Self-contained GPU primitive trail renderer for Coda of Annihilation.
    /// Builds a triangle-strip ribbon mesh from position arrays and renders with shaders.
    /// Falls back to line rendering when shaders are unavailable.
    /// </summary>
    public class CodaTrailRenderer : IDisposable
    {
        private const int MaxVertices = 2048;
        private const int MaxIndices = 6144;

        private DynamicVertexBuffer _vertexBuffer;
        private DynamicIndexBuffer _indexBuffer;
        private CodaVertex[] _vertices;
        private short[] _indices;
        private bool _disposed;

        public CodaTrailRenderer()
        {
            _vertices = new CodaVertex[MaxVertices];
            _indices = new short[MaxIndices];
        }

        private void EnsureBuffers(GraphicsDevice device)
        {
            if (_vertexBuffer == null || _vertexBuffer.IsDisposed)
                _vertexBuffer = new DynamicVertexBuffer(device, CodaVertex.Declaration, MaxVertices, BufferUsage.WriteOnly);
            if (_indexBuffer == null || _indexBuffer.IsDisposed)
                _indexBuffer = new DynamicIndexBuffer(device, IndexElementSize.SixteenBits, MaxIndices, BufferUsage.WriteOnly);
        }

        /// <summary>
        /// Render a trail from positions using CatmullRom smoothing + GPU triangle strip.
        /// Falls back to simple line drawing if GPU buffers fail.
        /// </summary>
        public void RenderTrail(Vector2[] positions, CodaTrailSettings settings, int? renderPoints = null)
        {
            if (positions == null || positions.Length < 2)
                return;

            Vector2[] valid = FilterPositions(positions);
            if (valid.Length < 2)
                return;

            int numPoints = renderPoints ?? valid.Length;
            numPoints = Math.Min(numPoints, valid.Length);

            Vector2[] sampled = settings.Smoothen && numPoints > 2
                ? SmoothPositions(valid, numPoints)
                : ResamplePositions(valid, numPoints);

            if (sampled.Length < 2)
                return;

            float[] completionRatios = ComputeCompletionRatios(sampled);

            int vertexCount = 0;
            int indexCount = 0;

            for (int i = 0; i < sampled.Length; i++)
            {
                float t = completionRatios[i];
                float halfWidth = settings.Width(t) * 0.5f;
                Color color = settings.TrailColor(t);

                Vector2 tangent;
                if (i == 0) tangent = sampled[1] - sampled[0];
                else if (i == sampled.Length - 1) tangent = sampled[i] - sampled[i - 1];
                else tangent = sampled[i + 1] - sampled[i - 1];

                if (tangent.LengthSquared() < 0.0001f) tangent = Vector2.UnitX;
                tangent.Normalize();

                Vector2 normal = new Vector2(-tangent.Y, tangent.X);
                Vector2 screenPos = sampled[i] - Main.screenPosition;
                if (settings.Offset != null) screenPos += settings.Offset(t);

                Vector2 left = screenPos + normal * halfWidth;
                Vector2 right = screenPos - normal * halfWidth;

                float u = t;
                float wc = Math.Max(halfWidth, 0.01f);

                if (vertexCount + 2 > MaxVertices) break;
                _vertices[vertexCount] = new CodaVertex(left, color, new Vector3(u, 0f, wc));
                _vertices[vertexCount + 1] = new CodaVertex(right, color, new Vector3(u, 1f, wc));

                if (i > 0 && indexCount + 6 <= MaxIndices)
                {
                    short bl = (short)(vertexCount - 2);
                    short br = (short)(vertexCount - 1);
                    short tl = (short)vertexCount;
                    short tr = (short)(vertexCount + 1);
                    _indices[indexCount++] = bl;
                    _indices[indexCount++] = tl;
                    _indices[indexCount++] = br;
                    _indices[indexCount++] = br;
                    _indices[indexCount++] = tl;
                    _indices[indexCount++] = tr;
                }
                vertexCount += 2;
            }

            if (vertexCount < 4 || indexCount < 6)
                return;

            try
            {
                GraphicsDevice device = Main.graphics.GraphicsDevice;
                EnsureBuffers(device);

                _vertexBuffer.SetData(_vertices, 0, vertexCount, SetDataOptions.Discard);
                _indexBuffer.SetData(_indices, 0, indexCount, SetDataOptions.Discard);

                device.SetVertexBuffer(_vertexBuffer);
                device.Indices = _indexBuffer;

                // Set render states for GPU primitive drawing
                var prevBlend = device.BlendState;
                var prevDepth = device.DepthStencilState;
                var prevRaster = device.RasterizerState;
                device.BlendState = BlendState.Additive;
                device.DepthStencilState = DepthStencilState.None;
                device.RasterizerState = RasterizerState.CullNone;

                if (settings.Shader != null)
                {
                    // Set WVP matrix with zoom compensation on the underlying Effect
                    var zoom = Main.GameViewMatrix.Zoom;
                    Matrix projection = Matrix.CreateOrthographicOffCenter(
                        0, Main.screenWidth / zoom.X, Main.screenHeight / zoom.Y, 0, -1, 1);
                    settings.Shader.Shader?.Parameters["uWorldViewProjection"]?.SetValue(projection);

                    settings.Shader.Apply();
                }

                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexCount, 0, indexCount / 3);

                device.SetVertexBuffer(null);
                device.Indices = null;
                device.BlendState = prevBlend;
                device.DepthStencilState = prevDepth;
                device.RasterizerState = prevRaster;
            }
            catch
            {
                // Fallback: simple line draw (wrapped in try/catch — SpriteBatch may not be active)
                try { DrawLineFallback(sampled, settings, completionRatios); } catch { }
            }
        }

        /// <summary>Simple line fallback when GPU primitives fail.</summary>
        public void DrawLineFallback(Vector2[] positions, CodaTrailSettings settings, float[] completionRatios)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D pixel = MagnumTextureRegistry.GetSoftGlow();
            if (pixel == null) return;

            for (int i = 1; i < positions.Length; i++)
            {
                float t = completionRatios != null ? completionRatios[i] : (float)i / positions.Length;
                Color c = settings.TrailColor(t);
                float w = settings.Width(t);

                Vector2 a = positions[i - 1] - Main.screenPosition;
                Vector2 b = positions[i] - Main.screenPosition;
                Vector2 diff = b - a;
                float len = diff.Length();
                if (len < 0.01f) continue;
                float rot = (float)Math.Atan2(diff.Y, diff.X);

                sb.Draw(pixel, a, new Rectangle(0, 0, 1, 1), c, rot, new Vector2(0f, 0.5f), new Vector2(len, w), SpriteEffects.None, 0f);
            }
        }

        #region Position Processing

        private static Vector2[] FilterPositions(Vector2[] positions)
        {
            var result = new System.Collections.Generic.List<Vector2>();
            foreach (var p in positions)
            {
                if (p != Vector2.Zero)
                    result.Add(p);
            }
            return result.ToArray();
        }

        private static Vector2[] ResamplePositions(Vector2[] positions, int targetCount)
        {
            if (positions.Length <= targetCount) return positions;
            Vector2[] result = new Vector2[targetCount];
            for (int i = 0; i < targetCount; i++)
            {
                float t = (float)i / (targetCount - 1) * (positions.Length - 1);
                int index = (int)t;
                float frac = t - index;
                if (index >= positions.Length - 1) result[i] = positions[positions.Length - 1];
                else result[i] = Vector2.Lerp(positions[index], positions[index + 1], frac);
            }
            return result;
        }

        private static Vector2[] SmoothPositions(Vector2[] positions, int targetCount)
        {
            if (positions.Length < 3) return positions;
            int outCount = Math.Max(targetCount, positions.Length) * 2;
            Vector2[] result = new Vector2[outCount];
            for (int i = 0; i < outCount; i++)
            {
                float t = (float)i / (outCount - 1) * (positions.Length - 1);
                int idx = (int)t;
                float frac = t - idx;
                int i0 = Math.Max(idx - 1, 0);
                int i1 = idx;
                int i2 = Math.Min(idx + 1, positions.Length - 1);
                int i3 = Math.Min(idx + 2, positions.Length - 1);
                result[i] = CodaUtils.CatmullRom(positions[i0], positions[i1], positions[i2], positions[i3], frac);
            }
            return result;
        }

        private static float[] ComputeCompletionRatios(Vector2[] positions)
        {
            float[] ratios = new float[positions.Length];
            float totalLen = 0f;
            float[] cumDist = new float[positions.Length];
            cumDist[0] = 0f;
            for (int i = 1; i < positions.Length; i++)
            {
                totalLen += Vector2.Distance(positions[i - 1], positions[i]);
                cumDist[i] = totalLen;
            }
            if (totalLen < 0.01f)
            {
                for (int i = 0; i < ratios.Length; i++)
                    ratios[i] = (float)i / Math.Max(1, ratios.Length - 1);
                return ratios;
            }
            for (int i = 0; i < positions.Length; i++)
                ratios[i] = cumDist[i] / totalLen;
            return ratios;
        }

        #endregion

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _vertexBuffer?.Dispose();
            _indexBuffer?.Dispose();
        }
    }
}
