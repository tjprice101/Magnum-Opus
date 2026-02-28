using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Shaders;

namespace MagnumOpus.Content.Eroica.Weapons.CelestialValor.Primitives
{
    public static class ValorTrailRenderer
    {
        private static DynamicVertexBuffer _vertexBuffer;
        private static DynamicIndexBuffer _indexBuffer;
        private const int MaxVertices = 2048;
        private const int MaxIndices = 4096;
        private static ValorVertexType[] _vertices;
        private static short[] _indices;
        private static bool _initialized;

        private static void EnsureInitialized()
        {
            if (_initialized && _vertexBuffer != null && !_vertexBuffer.IsDisposed) return;
            var device = Main.instance.GraphicsDevice;
            _vertices = new ValorVertexType[MaxVertices];
            _indices = new short[MaxIndices];
            _vertexBuffer = new DynamicVertexBuffer(device, ValorVertexType.VertexDecl, MaxVertices, BufferUsage.WriteOnly);
            _indexBuffer = new DynamicIndexBuffer(device, IndexElementSize.SixteenBits, MaxIndices, BufferUsage.WriteOnly);
            _initialized = true;
        }

        public static void RenderTrail(IList<Vector2> positions, ValorTrailSettings settings, int pointsToCreate = 0)
        {
            if (positions.Count < 3) return;
            Vector2[] arr = new Vector2[positions.Count];
            for (int i = 0; i < positions.Count; i++) arr[i] = positions[i];
            RenderTrail(arr, settings, pointsToCreate);
        }

        public static void RenderTrail(Vector2[] positions, ValorTrailSettings settings, int pointsToCreate = 0)
        {
            if (positions.Length < 3) return;
            EnsureInitialized();

            int validCount = 0;
            for (int i = 0; i < positions.Length; i++)
                if (positions[i] != Vector2.Zero) validCount++;
            if (validCount < 3) return;

            int desiredPoints = pointsToCreate > 0 ? pointsToCreate : validCount;
            desiredPoints = Math.Clamp(desiredPoints, 3, MaxVertices / 2 - 1);

            // Step 1: CatmullRom smoothing to generate smooth curve points
            Vector2[] smoothed = SmoothPoints(positions, desiredPoints, settings.Smoothen);
            int smoothedCount = smoothed.Length;
            if (smoothedCount < 3) return;

            // Step 2: Compute arc-length completion ratios
            float[] completionRatios = ComputeCompletionRatios(smoothed, smoothedCount);

            // Step 3: Build vertices with parallel transport normals
            int vertexCount = 0;
            int indexCount = 0;
            Vector2 previousNormal = Vector2.Zero;

            for (int i = 0; i < smoothedCount; i++)
            {
                if (vertexCount + 2 > MaxVertices) break;

                float completion = completionRatios[i];
                float width = Math.Max(settings.WidthFunction(completion), 0f);
                Color color = settings.ColorFunction(completion);

                // Compute tangent
                Vector2 tangent;
                if (i == 0) tangent = smoothed[1] - smoothed[0];
                else if (i == smoothedCount - 1) tangent = smoothed[i] - smoothed[i - 1];
                else tangent = (smoothed[i + 1] - smoothed[i - 1]);
                tangent = tangent.SafeNormalize(Vector2.UnitX);

                // Normal via parallel transport
                Vector2 baseNormal = new Vector2(-tangent.Y, tangent.X);
                Vector2 normal;
                if (i > 0 && previousNormal.LengthSquared() > 0.001f)
                {
                    Vector2 prevTangent = (i == 1)
                        ? (smoothed[1] - smoothed[0]).SafeNormalize(Vector2.UnitX)
                        : (smoothed[i] - smoothed[i - 2]).SafeNormalize(Vector2.UnitX);
                    float cosA = MathHelper.Clamp(Vector2.Dot(prevTangent, tangent), -1f, 1f);
                    float sinA = prevTangent.X * tangent.Y - prevTangent.Y * tangent.X;
                    normal = new Vector2(cosA * previousNormal.X - sinA * previousNormal.Y,
                                         sinA * previousNormal.X + cosA * previousNormal.Y);
                    normal = normal.SafeNormalize(baseNormal);
                }
                else
                    normal = baseNormal;
                previousNormal = normal;

                Vector2 screenPos = smoothed[i] - Main.screenPosition;
                if (settings.OffsetFunction != null)
                    screenPos += settings.OffsetFunction(completion);

                Vector2 offset = normal * width;
                Vector2 left = screenPos - offset;
                Vector2 right = screenPos + offset;
                float halfWidth = Math.Max(width, 0.001f);

                float texU = completion + settings.TextureScrollOffset;
                _vertices[vertexCount] = new ValorVertexType(left, color, new Vector2(texU, 0f), halfWidth);
                _vertices[vertexCount + 1] = new ValorVertexType(right, color, new Vector2(texU, 1f), halfWidth);
                vertexCount += 2;
            }

            if (vertexCount < 6) return;

            // Build triangle list indices
            int quads = vertexCount / 2 - 1;
            for (int i = 0; i < quads; i++)
            {
                if (indexCount + 6 > MaxIndices) break;
                short baseIdx = (short)(i * 2);
                _indices[indexCount++] = baseIdx;
                _indices[indexCount++] = (short)(baseIdx + 1);
                _indices[indexCount++] = (short)(baseIdx + 2);
                _indices[indexCount++] = (short)(baseIdx + 2);
                _indices[indexCount++] = (short)(baseIdx + 1);
                _indices[indexCount++] = (short)(baseIdx + 3);
            }

            // Step 4: Render
            var device = Main.instance.GraphicsDevice;
            device.RasterizerState = RasterizerState.CullNone;

            Matrix view, projection;
            CalculateMatrices(out view, out projection);

            var shader = settings.Shader ?? GameShaders.Misc["MagnumOpus:ValorStandardPrimitive"];
            shader.Shader.Parameters["uWorldViewProjection"]?.SetValue(view * projection);
            shader.Apply();

            _vertexBuffer.SetData(_vertices, 0, vertexCount, SetDataOptions.Discard);
            _indexBuffer.SetData(_indices, 0, indexCount, SetDataOptions.Discard);
            device.SetVertexBuffer(_vertexBuffer);
            device.Indices = _indexBuffer;
            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexCount, 0, indexCount / 3);
        }

        private static Vector2[] SmoothPoints(Vector2[] positions, int outputCount, bool smooth)
        {
            // Collect valid (non-zero) positions
            List<Vector2> valid = new();
            for (int i = 0; i < positions.Length; i++)
                if (positions[i] != Vector2.Zero) valid.Add(positions[i]);
            if (valid.Count < 3) return valid.ToArray();

            if (!smooth)
            {
                // Just linearly interpolate to desired count
                Vector2[] result = new Vector2[outputCount];
                for (int i = 0; i < outputCount; i++)
                {
                    float t = i / (float)(outputCount - 1);
                    float scaled = t * (valid.Count - 1);
                    int idx = (int)scaled;
                    float frac = scaled - idx;
                    result[i] = Vector2.Lerp(valid[idx], valid[Math.Min(idx + 1, valid.Count - 1)], frac);
                }
                return result;
            }

            // CatmullRom smoothing
            Vector2[] output = new Vector2[outputCount];
            int n = valid.Count;
            for (int i = 0; i < outputCount; i++)
            {
                float t = i / (float)(outputCount - 1);
                float scaled = t * (n - 1);
                int idx = (int)scaled;
                float localT = scaled - idx;
                Vector2 p0 = valid[Math.Max(idx - 1, 0)];
                Vector2 p1 = valid[idx];
                Vector2 p2 = valid[Math.Min(idx + 1, n - 1)];
                Vector2 p3 = valid[Math.Min(idx + 2, n - 1)];
                output[i] = Vector2.CatmullRom(p0, p1, p2, p3, localT);
            }
            return output;
        }

        private static float[] ComputeCompletionRatios(Vector2[] points, int count)
        {
            float[] ratios = new float[count];
            float totalLength = 0f;
            ratios[0] = 0f;
            for (int i = 1; i < count; i++)
            {
                totalLength += Vector2.Distance(points[i], points[i - 1]);
                ratios[i] = totalLength;
            }
            if (totalLength > 0.001f)
            {
                float inv = 1f / totalLength;
                for (int i = 1; i < count; i++)
                    ratios[i] *= inv;
                ratios[count - 1] = 1f;
            }
            return ratios;
        }

        public static void CalculateMatrices(out Matrix view, out Matrix projection)
        {
            Vector2 zoom = Main.GameViewMatrix.Zoom;
            Matrix zoomScale = Matrix.CreateScale(zoom.X, zoom.Y, 1f);
            int w = Main.instance.GraphicsDevice.Viewport.Width;
            int h = Main.instance.GraphicsDevice.Viewport.Height;
            view = Matrix.CreateLookAt(Vector3.Zero, Vector3.UnitZ, Vector3.Up);
            view *= Matrix.CreateTranslation(0f, -h, 0f);
            view *= Matrix.CreateRotationZ(MathHelper.Pi);
            if (Main.LocalPlayer.gravDir == -1f)
                view *= Matrix.CreateScale(1f, -1f, 1f) * Matrix.CreateTranslation(0f, h, 0f);
            view *= zoomScale;
            projection = Matrix.CreateOrthographicOffCenter(0f, w * zoom.X, 0f, h * zoom.Y, 0f, 1f) * zoomScale;
        }

        public static void Unload()
        {
            _vertexBuffer?.Dispose();
            _vertexBuffer = null;
            _indexBuffer?.Dispose();
            _indexBuffer = null;
            _vertices = null;
            _indices = null;
            _initialized = false;
        }
    }
}
