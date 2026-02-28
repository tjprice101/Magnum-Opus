using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.MoonlightsCalling.Primitives
{
    /// <summary>
    /// Self-contained GPU primitive trail renderer for Moonlight's Calling beams.
    /// Adapted from the Eternal Moon's LunarTrailRenderer for beam-style trails.
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public sealed class SerenadeTrailRenderer : ModSystem
    {
        internal const float Epsilon = 0.0001f;

        private static DynamicVertexBuffer _vertexBuffer;
        private static DynamicIndexBuffer _indexBuffer;
        private static GraphicsDevice _device;

        private static SerenadeVertexType[] _vertices;
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
                _vertexBuffer = new DynamicVertexBuffer(_device, SerenadeVertexType.VertexDecl, MaxVertices, BufferUsage.WriteOnly);
                _indexBuffer = new DynamicIndexBuffer(_device, IndexElementSize.SixteenBits, MaxIndices, BufferUsage.WriteOnly);
                _vertices = new SerenadeVertexType[MaxVertices];
                _indices = new short[MaxIndices];
                _smoothedPoints = new Vector2[512];
                _completionRatios = new float[512];
                _tangents = new Vector2[512];
                _normals = new Vector2[512];
            });
        }

        public override void Unload()
        {
            Main.QueueMainThreadAction(() =>
            {
                _vertexBuffer?.Dispose();
                _indexBuffer?.Dispose();
                _vertexBuffer = null;
                _indexBuffer = null;
                _vertices = null;
                _indices = null;
            });
        }

        /// <summary>
        /// Renders a beam trail from world-space position points.
        /// </summary>
        public static void RenderTrail(IList<Vector2> positions, SerenadeTrailSettings settings, int pointsToRender = 0)
        {
            if (_device == null || _vertexBuffer == null || positions == null || positions.Count < 2)
                return;

            int count = pointsToRender > 0 ? Math.Min(pointsToRender, positions.Count) : positions.Count;
            if (count < 2) return;

            int smoothedCount = settings.Smoothen ? SmoothPoints(positions, count) : CopyPoints(positions, count);
            if (smoothedCount < 2) return;

            ComputeCompletionRatios(smoothedCount);
            ComputeFrames(smoothedCount);

            int vertexCount = BuildVertices(smoothedCount, settings);
            if (vertexCount < 4) return;

            int indexCount = BuildIndices(smoothedCount);
            RenderGeometry(vertexCount, indexCount, settings);
        }

        /// <summary>Array overload.</summary>
        public static void RenderTrail(Vector2[] positions, SerenadeTrailSettings settings, int pointsToRender = 0)
        {
            RenderTrail((IList<Vector2>)positions, settings, pointsToRender);
        }

        private static int SmoothPoints(IList<Vector2> source, int count)
        {
            const int SegmentsPerPair = 4;
            int totalOutput = (count - 1) * SegmentsPerPair + 1;
            totalOutput = Math.Min(totalOutput, _smoothedPoints.Length);

            int idx = 0;
            for (int i = 0; i < count - 1 && idx < totalOutput; i++)
            {
                Vector2 p0 = i > 0 ? source[i - 1] : source[i];
                Vector2 p1 = source[i];
                Vector2 p2 = source[i + 1];
                Vector2 p3 = i < count - 2 ? source[i + 2] : source[i + 1];

                for (int j = 0; j < SegmentsPerPair && idx < totalOutput; j++)
                {
                    float t = j / (float)SegmentsPerPair;
                    _smoothedPoints[idx++] = Vector2.CatmullRom(p0, p1, p2, p3, t);
                }
            }
            if (idx < totalOutput)
                _smoothedPoints[idx++] = source[count - 1];

            return idx;
        }

        private static int CopyPoints(IList<Vector2> source, int count)
        {
            int n = Math.Min(count, _smoothedPoints.Length);
            for (int i = 0; i < n; i++)
                _smoothedPoints[i] = source[i];
            return n;
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

        private static void ComputeFrames(int count)
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

                if (tangent.LengthSquared() > Epsilon)
                    tangent.Normalize();
                else
                    tangent = Vector2.UnitX;

                _tangents[i] = tangent;
            }

            _normals[0] = new Vector2(-_tangents[0].Y, _tangents[0].X);

            for (int i = 1; i < count; i++)
            {
                Vector2 prevNormal = _normals[i - 1];
                Vector2 currTangent = _tangents[i];

                float dot = Vector2.Dot(prevNormal, currTangent);
                Vector2 projected = prevNormal - currTangent * dot;

                if (projected.LengthSquared() > Epsilon)
                    projected.Normalize();
                else
                    projected = new Vector2(-currTangent.Y, currTangent.X);

                _normals[i] = projected;
            }
        }

        private static int BuildVertices(int count, SerenadeTrailSettings settings)
        {
            int vi = 0;
            for (int i = 0; i < count && vi + 1 < MaxVertices; i++)
            {
                float completion = _completionRatios[i];
                Vector2 pos = _smoothedPoints[i];

                if (settings.OffsetFunction != null)
                    pos += settings.OffsetFunction(completion, pos);

                float halfWidth = settings.WidthFunction(completion, pos) * 0.5f;
                Color color = settings.ColorFunction(completion, pos);
                Vector2 normal = _normals[i];

                Vector2 left = pos + normal * halfWidth - Main.screenPosition;
                Vector2 right = pos - normal * halfWidth - Main.screenPosition;

                float u = completion + settings.TextureScrollOffset;

                _vertices[vi++] = new SerenadeVertexType(left, color, new Vector2(u, 0f), halfWidth);
                _vertices[vi++] = new SerenadeVertexType(right, color, new Vector2(u, 1f), halfWidth);
            }
            return vi;
        }

        private static int BuildIndices(int count)
        {
            int ii = 0;
            int quadCount = count - 1;

            for (int i = 0; i < quadCount && ii + 5 < MaxIndices; i++)
            {
                short tl = (short)(i * 2);
                short bl = (short)(i * 2 + 1);
                short tr = (short)(i * 2 + 2);
                short br = (short)(i * 2 + 3);

                _indices[ii++] = tl;
                _indices[ii++] = bl;
                _indices[ii++] = tr;
                _indices[ii++] = tr;
                _indices[ii++] = bl;
                _indices[ii++] = br;
            }
            return ii;
        }

        private static void RenderGeometry(int vertexCount, int indexCount, SerenadeTrailSettings settings)
        {
            if (vertexCount < 4 || indexCount < 6) return;

            var (world, view, projection) = CalculatePerspectiveMatrices();

            if (settings.Shader != null)
            {
                Effect effect = settings.Shader.Shader;
                if (effect != null)
                {
                    effect.Parameters["uWorldViewProjection"]?.SetValue(world * view * projection);
                    settings.Shader.Apply();
                }
            }

            _vertexBuffer.SetData(_vertices, 0, vertexCount, SetDataOptions.Discard);
            _indexBuffer.SetData(_indices, 0, indexCount, SetDataOptions.Discard);

            _device.SetVertexBuffer(_vertexBuffer);
            _device.Indices = _indexBuffer;

            var prevRasterizer = _device.RasterizerState;
            _device.RasterizerState = RasterizerState.CullNone;

            int primitiveCount = indexCount / 3;
            _device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexCount, 0, primitiveCount);

            _device.RasterizerState = prevRasterizer;
        }

        public static (Matrix world, Matrix view, Matrix projection) CalculatePerspectiveMatrices()
        {
            Matrix world = Matrix.Identity;
            Matrix view = Matrix.Identity;

            Vector2 zoom = Main.GameViewMatrix.Zoom;
            int width = Main.screenWidth;
            int height = Main.screenHeight;

            Matrix projection = Matrix.CreateOrthographicOffCenter(0, width / zoom.X, height / zoom.Y, 0, -1, 1);

            if (Main.LocalPlayer.gravDir == -1f)
                view = Matrix.CreateScale(1f, -1f, 1f) * Matrix.CreateTranslation(0f, height, 0f);

            return (world, view, projection);
        }
    }
}
