using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.StaffOfTheLunarPhases.Primitives
{
    /// <summary>
    /// Self-contained GPU primitive trail renderer for Goliath moonlight beam projectiles.
    /// Adapted from the Comet trail renderer for beam-style trails with wider, smoother profiles.
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public sealed class GoliathTrailRenderer : ModSystem
    {
        internal const float Epsilon = 0.0001f;

        private static DynamicVertexBuffer _vertexBuffer;
        private static DynamicIndexBuffer _indexBuffer;
        private static GraphicsDevice _device;

        private static GoliathVertexType[] _vertices;
        private static short[] _indices;

        private static Vector2[] _smoothedPoints;
        private static float[] _completionRatios;
        private static Vector2[] _tangents;
        private static Vector2[] _normals;

        private const int MaxVertices = 2048;
        private const int MaxIndices = 4096;

        public override void Load()
        {
            Main.QueueMainThreadAction(() =>
            {
                _device = Main.graphics.GraphicsDevice;
                _vertexBuffer = new DynamicVertexBuffer(_device, GoliathVertexType.VertexDecl, MaxVertices, BufferUsage.WriteOnly);
                _indexBuffer = new DynamicIndexBuffer(_device, IndexElementSize.SixteenBits, MaxIndices, BufferUsage.WriteOnly);
            });

            _vertices = new GoliathVertexType[MaxVertices];
            _indices = new short[MaxIndices];
            _smoothedPoints = new Vector2[512];
            _completionRatios = new float[512];
            _tangents = new Vector2[512];
            _normals = new Vector2[512];
        }

        public override void Unload()
        {
            Main.QueueMainThreadAction(() =>
            {
                _vertexBuffer?.Dispose();
                _indexBuffer?.Dispose();
            });
            _vertices = null;
            _indices = null;
        }

        /// <summary>
        /// Render a trail from an array of positions using the given settings.
        /// </summary>
        public static void RenderTrail(Vector2[] positions, GoliathTrailSettings settings, int pointCount = 0)
        {
            if (_device == null || _vertexBuffer == null || positions == null || positions.Length < 2) return;

            int count = pointCount > 0 ? Math.Min(pointCount, positions.Length) : positions.Length;

            // Filter out zero/duplicate positions
            List<Vector2> validPoints = new(count);
            for (int i = 0; i < count; i++)
            {
                if (positions[i] == Vector2.Zero) continue;
                if (validPoints.Count > 0 && Vector2.DistanceSquared(validPoints[^1], positions[i]) < Epsilon)
                    continue;
                validPoints.Add(positions[i]);
            }

            if (validPoints.Count < 2) return;

            // Smooth via CatmullRom if enabled
            int smoothedCount;
            if (settings.Smoothen && validPoints.Count >= 4)
                smoothedCount = SmoothPoints(validPoints);
            else
            {
                smoothedCount = validPoints.Count;
                for (int i = 0; i < smoothedCount && i < _smoothedPoints.Length; i++)
                    _smoothedPoints[i] = validPoints[i];
            }

            if (smoothedCount < 2) return;

            ComputeCompletionRatios(smoothedCount);
            ComputeTangentsAndNormals(smoothedCount);

            int vertCount = 0, idxCount = 0;
            BuildMesh(smoothedCount, settings, ref vertCount, ref idxCount);

            if (vertCount < 3 || idxCount < 3) return;

            _vertexBuffer.SetData(_vertices, 0, vertCount, SetDataOptions.Discard);
            _indexBuffer.SetData(_indices, 0, idxCount, SetDataOptions.Discard);

            _device.SetVertexBuffer(_vertexBuffer);
            _device.Indices = _indexBuffer;

            var viewport = _device.Viewport;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, viewport.Width, viewport.Height, 0, -1, 1);
            Matrix zoom = Main.GameViewMatrix.ZoomMatrix;
            Matrix gravityMatrix = Main.GameViewMatrix.EffectMatrix;

            settings.Shader?.Shader?.Parameters["uWorldViewProjection"]?.SetValue(gravityMatrix * zoom * projection);
            settings.Shader?.Apply(null);

            _device.RasterizerState = RasterizerState.CullNone;
            _device.BlendState = BlendState.Additive;
            _device.SamplerStates[0] = SamplerState.LinearWrap;

            _device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertCount, 0, idxCount / 3);
        }

        private static int SmoothPoints(List<Vector2> points)
        {
            int segmentsPerPair = 4;
            int count = 0;

            for (int i = 0; i < points.Count - 1 && count < _smoothedPoints.Length - segmentsPerPair; i++)
            {
                Vector2 p0 = points[Math.Max(i - 1, 0)];
                Vector2 p1 = points[i];
                Vector2 p2 = points[Math.Min(i + 1, points.Count - 1)];
                Vector2 p3 = points[Math.Min(i + 2, points.Count - 1)];

                for (int s = 0; s < segmentsPerPair; s++)
                {
                    float t = s / (float)segmentsPerPair;
                    _smoothedPoints[count++] = Vector2.CatmullRom(p0, p1, p2, p3, t);
                }
            }

            if (count < _smoothedPoints.Length)
                _smoothedPoints[count++] = points[^1];

            return count;
        }

        private static void ComputeCompletionRatios(int count)
        {
            float totalLength = 0f;
            _completionRatios[0] = 0f;

            for (int i = 1; i < count; i++)
            {
                totalLength += Vector2.Distance(_smoothedPoints[i - 1], _smoothedPoints[i]);
                _completionRatios[i] = totalLength;
            }

            if (totalLength > Epsilon)
            {
                for (int i = 1; i < count; i++)
                    _completionRatios[i] /= totalLength;
            }
        }

        private static void ComputeTangentsAndNormals(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 tangent;
                if (i == 0)
                    tangent = _smoothedPoints[1] - _smoothedPoints[0];
                else if (i == count - 1)
                    tangent = _smoothedPoints[i] - _smoothedPoints[i - 1];
                else
                    tangent = _smoothedPoints[i + 1] - _smoothedPoints[i - 1];

                float len = tangent.Length();
                if (len > Epsilon)
                    tangent /= len;
                else
                    tangent = Vector2.UnitX;

                _tangents[i] = tangent;
                _normals[i] = new Vector2(-tangent.Y, tangent.X);
            }
        }

        private static void BuildMesh(int count, GoliathTrailSettings settings, ref int vertCount, ref int idxCount)
        {
            vertCount = 0;
            idxCount = 0;

            for (int i = 0; i < count; i++)
            {
                float completion = _completionRatios[i];
                Vector2 pos = _smoothedPoints[i] - Main.screenPosition;

                float width = settings.WidthFunction?.Invoke(completion, _smoothedPoints[i]) ?? 10f;
                Color color = settings.ColorFunction?.Invoke(completion, _smoothedPoints[i]) ?? Color.White;
                Vector2 offset = settings.OffsetFunction?.Invoke(completion, _smoothedPoints[i]) ?? Vector2.Zero;

                Vector2 normal = _normals[i];
                float halfWidth = width * 0.5f;

                float u = completion + settings.TextureScrollOffset;

                _vertices[vertCount] = new GoliathVertexType(
                    pos + offset + normal * halfWidth,
                    color, new Vector2(u, 0f));

                _vertices[vertCount + 1] = new GoliathVertexType(
                    pos + offset - normal * halfWidth,
                    color, new Vector2(u, 1f));

                if (i > 0)
                {
                    short tl = (short)(vertCount - 2);
                    short bl = (short)(vertCount - 1);
                    short tr = (short)vertCount;
                    short br = (short)(vertCount + 1);

                    _indices[idxCount++] = tl;
                    _indices[idxCount++] = tr;
                    _indices[idxCount++] = bl;

                    _indices[idxCount++] = bl;
                    _indices[idxCount++] = tr;
                    _indices[idxCount++] = br;
                }

                vertCount += 2;

                if (vertCount >= MaxVertices - 2 || idxCount >= MaxIndices - 6)
                    break;
            }
        }
    }
}
