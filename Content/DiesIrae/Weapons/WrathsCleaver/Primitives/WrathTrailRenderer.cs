using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.WrathsCleaver.Primitives
{
    /// <summary>
    /// GPU-based primitive trail renderer for Wrath's Cleaver.
    /// Generates triangle-strip meshes from trail points with CatmullRom smoothing.
    /// Self-contained — does not depend on any global primitive system.
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public sealed class WrathTrailRenderer : ModSystem
    {
        private static DynamicVertexBuffer vertexBuffer;
        private static DynamicIndexBuffer indexBuffer;
        private static WrathVertexType[] vertices;
        private static short[] indices;
        private const int MaxVertices = 2048;
        private const int MaxIndices = 4096;

        public override void Load()
        {
            vertices = new WrathVertexType[MaxVertices];
            indices = new short[MaxIndices];
            Main.QueueMainThreadAction(() =>
            {
                var device = Main.graphics.GraphicsDevice;
                vertexBuffer = new DynamicVertexBuffer(device, WrathVertexType.VertexDecl, MaxVertices, BufferUsage.WriteOnly);
                indexBuffer = new DynamicIndexBuffer(device, IndexElementSize.SixteenBits, MaxIndices, BufferUsage.WriteOnly);
            });
        }

        public override void Unload()
        {
            Main.QueueMainThreadAction(() =>
            {
                vertexBuffer?.Dispose();
                indexBuffer?.Dispose();
            });
            vertices = null;
            indices = null;
        }

        /// <summary>
        /// Render a trail from the given control points using the provided settings.
        /// Call within a shader region (SpriteBatch ended, shader applied).
        /// </summary>
        public static void RenderTrail(IList<Vector2> controlPoints, WrathTrailSettings settings)
        {
            if (controlPoints == null || controlPoints.Count < 2 || vertexBuffer == null)
                return;

            // Smooth via CatmullRom interpolation
            List<Vector2> smoothed = SmoothPoints(controlPoints, settings.SmoothingSteps);
            if (smoothed.Count < 2) return;

            int segCount = smoothed.Count;
            int vertCount = segCount * 2;
            int triCount = (segCount - 1) * 2;
            int idxCount = triCount * 3;

            if (vertCount > MaxVertices || idxCount > MaxIndices)
                return;

            // Build vertices
            for (int i = 0; i < segCount; i++)
            {
                float progress = (float)i / (segCount - 1);
                Vector2 pos = smoothed[i] + settings.OffsetFunction(progress);
                float width = settings.WidthFunction(progress);
                Color color = settings.ColorFunction(progress);

                // Compute normal via parallel transport
                Vector2 tangent;
                if (i == 0)
                    tangent = smoothed[1] - smoothed[0];
                else if (i == segCount - 1)
                    tangent = smoothed[segCount - 1] - smoothed[segCount - 2];
                else
                    tangent = smoothed[i + 1] - smoothed[i - 1];

                tangent = tangent.SafeNormalize(Vector2.UnitX);
                Vector2 normal = new Vector2(-tangent.Y, tangent.X);

                Vector2 screenPos = pos - Main.screenPosition;
                vertices[i * 2] = new WrathVertexType(screenPos + normal * width * 0.5f, color, new Vector3(progress, 0f, width));
                vertices[i * 2 + 1] = new WrathVertexType(screenPos - normal * width * 0.5f, color, new Vector3(progress, 1f, width));
            }

            // Build indices (triangle strip as triangle list)
            int idx = 0;
            for (int i = 0; i < segCount - 1; i++)
            {
                short tl = (short)(i * 2);
                short bl = (short)(i * 2 + 1);
                short tr = (short)((i + 1) * 2);
                short br = (short)((i + 1) * 2 + 1);

                indices[idx++] = tl;
                indices[idx++] = tr;
                indices[idx++] = bl;
                indices[idx++] = bl;
                indices[idx++] = tr;
                indices[idx++] = br;
            }

            // Submit to GPU
            var device = Main.graphics.GraphicsDevice;
            vertexBuffer.SetData(vertices, 0, vertCount, SetDataOptions.Discard);
            indexBuffer.SetData(indices, 0, idxCount, SetDataOptions.Discard);
            device.SetVertexBuffer(vertexBuffer);
            device.Indices = indexBuffer;

            settings.ShaderSetup?.Invoke();

            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertCount, 0, triCount);
        }

        /// <summary>
        /// CatmullRom smoothing of control points.
        /// </summary>
        private static List<Vector2> SmoothPoints(IList<Vector2> points, int subdivisions)
        {
            if (subdivisions <= 1 || points.Count < 3)
                return new List<Vector2>(points);

            var result = new List<Vector2>();
            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector2 p0 = points[Math.Max(i - 1, 0)];
                Vector2 p1 = points[i];
                Vector2 p2 = points[Math.Min(i + 1, points.Count - 1)];
                Vector2 p3 = points[Math.Min(i + 2, points.Count - 1)];

                for (int j = 0; j < subdivisions; j++)
                {
                    float t = (float)j / subdivisions;
                    result.Add(Vector2.CatmullRom(p0, p1, p2, p3, t));
                }
            }
            result.Add(points[points.Count - 1]);
            return result;
        }
    }
}
