using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Shaders;

namespace MagnumOpus.Common.Systems
{
    /// <summary>
    /// Primitive trail renderer for smooth, GPU-rendered trails on projectiles and NPCs.
    /// Inspired by Calamity Mod's PrimitiveRenderer.
    /// 
    /// Use this for weapon trails, boss flame trails, and projectile afterimages.
    /// </summary>
    public static class PrimitiveTrailRenderer
    {
        private static BasicEffect basicEffect;
        private static VertexPositionColorTexture[] vertices;
        private static short[] indices;
        
        private const int MaxTrailPositions = 256;
        private const int MaxVertices = 1024;
        private const int MaxIndices = 2048;

        /// <summary>
        /// Initializes the primitive renderer. Called automatically on first use.
        /// </summary>
        public static void Initialize()
        {
            if (Main.dedServ)
                return;

            basicEffect = new BasicEffect(Main.instance.GraphicsDevice)
            {
                VertexColorEnabled = true,
                TextureEnabled = false
            };

            vertices = new VertexPositionColorTexture[MaxVertices];
            indices = new short[MaxIndices];
        }

        /// <summary>
        /// Delegate for calculating trail width at a given completion ratio.
        /// </summary>
        /// <param name="completionRatio">Value from 0 (start) to 1 (end) of the trail.</param>
        /// <returns>The width of the trail at this point.</returns>
        public delegate float TrailWidthFunction(float completionRatio);

        /// <summary>
        /// Delegate for calculating trail color at a given completion ratio.
        /// </summary>
        /// <param name="completionRatio">Value from 0 (start) to 1 (end) of the trail.</param>
        /// <returns>The color of the trail at this point.</returns>
        public delegate Color TrailColorFunction(float completionRatio);

        /// <summary>
        /// Settings for rendering a primitive trail.
        /// </summary>
        public struct TrailSettings
        {
            public TrailWidthFunction WidthFunction;
            public TrailColorFunction ColorFunction;
            public Func<float, Vector2> OffsetFunction;
            public MiscShaderData Shader;
            public bool Smoothen;
            public bool Pixelate;

            public TrailSettings(TrailWidthFunction width, TrailColorFunction color, 
                Func<float, Vector2> offset = null, MiscShaderData shader = null, 
                bool smoothen = true, bool pixelate = false)
            {
                WidthFunction = width;
                ColorFunction = color;
                OffsetFunction = offset;
                Shader = shader;
                Smoothen = smoothen;
                Pixelate = pixelate;
            }
        }

        /// <summary>
        /// Renders a trail using the given positions and settings.
        /// Positions should be in world coordinates.
        /// </summary>
        /// <param name="positions">Array of world positions for the trail.</param>
        /// <param name="settings">Trail rendering settings.</param>
        /// <param name="pointsToCreate">Number of points to use (more = smoother but slower).</param>
        public static void RenderTrail(Vector2[] positions, TrailSettings settings, int pointsToCreate = 50)
        {
            if (Main.dedServ || positions == null || positions.Length < 2)
                return;

            if (basicEffect == null)
                Initialize();

            // Filter out zero positions
            List<Vector2> validPositions = new List<Vector2>();
            foreach (Vector2 pos in positions)
            {
                if (pos != Vector2.Zero)
                    validPositions.Add(pos);
            }

            if (validPositions.Count < 2)
                return;

            // Optionally smooth the trail using Catmull-Rom splines
            List<Vector2> finalPositions;
            if (settings.Smoothen && validPositions.Count >= 4)
            {
                finalPositions = SmoothTrail(validPositions, pointsToCreate);
            }
            else
            {
                finalPositions = validPositions;
            }

            if (finalPositions.Count < 2)
                return;

            // Build vertex data
            int vertexCount = finalPositions.Count * 2;
            int indexCount = (finalPositions.Count - 1) * 6;

            if (vertexCount > MaxVertices || indexCount > MaxIndices)
                return;

            for (int i = 0; i < finalPositions.Count; i++)
            {
                float completionRatio = (float)i / (finalPositions.Count - 1);
                float width = settings.WidthFunction?.Invoke(completionRatio) ?? 10f;
                Color color = settings.ColorFunction?.Invoke(completionRatio) ?? Color.White;

                // Calculate perpendicular direction for width
                Vector2 direction;
                if (i == 0)
                    direction = (finalPositions[1] - finalPositions[0]).SafeNormalize(Vector2.UnitY);
                else if (i == finalPositions.Count - 1)
                    direction = (finalPositions[i] - finalPositions[i - 1]).SafeNormalize(Vector2.UnitY);
                else
                    direction = (finalPositions[i + 1] - finalPositions[i - 1]).SafeNormalize(Vector2.UnitY);

                Vector2 perpendicular = new Vector2(-direction.Y, direction.X);
                Vector2 worldPos = finalPositions[i];

                // Apply offset if provided
                if (settings.OffsetFunction != null)
                    worldPos += settings.OffsetFunction(completionRatio);

                // Convert to screen position
                Vector2 screenPos = worldPos - Main.screenPosition;

                // Create two vertices for the quad strip
                vertices[i * 2] = new VertexPositionColorTexture(
                    new Vector3(screenPos + perpendicular * width * 0.5f, 0),
                    color,
                    new Vector2(completionRatio, 0));

                vertices[i * 2 + 1] = new VertexPositionColorTexture(
                    new Vector3(screenPos - perpendicular * width * 0.5f, 0),
                    color,
                    new Vector2(completionRatio, 1));
            }

            // Build indices for triangle strip
            int idx = 0;
            for (int i = 0; i < finalPositions.Count - 1; i++)
            {
                int baseVertex = i * 2;
                indices[idx++] = (short)baseVertex;
                indices[idx++] = (short)(baseVertex + 1);
                indices[idx++] = (short)(baseVertex + 2);
                indices[idx++] = (short)(baseVertex + 1);
                indices[idx++] = (short)(baseVertex + 3);
                indices[idx++] = (short)(baseVertex + 2);
            }

            // Render
            DrawPrimitives(vertexCount, indexCount / 3, settings.Shader);
        }

        /// <summary>
        /// Smooths a trail using Catmull-Rom spline interpolation.
        /// </summary>
        private static List<Vector2> SmoothTrail(List<Vector2> positions, int outputPoints)
        {
            List<Vector2> result = new List<Vector2>();
            
            for (int i = 0; i < outputPoints; i++)
            {
                float t = (float)i / (outputPoints - 1) * (positions.Count - 1);
                int segment = (int)t;
                float segmentT = t - segment;

                // Clamp segment indices
                int p0 = Math.Max(0, segment - 1);
                int p1 = segment;
                int p2 = Math.Min(positions.Count - 1, segment + 1);
                int p3 = Math.Min(positions.Count - 1, segment + 2);

                result.Add(CatmullRom(positions[p0], positions[p1], positions[p2], positions[p3], segmentT));
            }

            return result;
        }

        /// <summary>
        /// Catmull-Rom spline interpolation.
        /// </summary>
        private static Vector2 CatmullRom(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;

            return 0.5f * (
                2f * p1 +
                (-p0 + p2) * t +
                (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
                (-p0 + 3f * p1 - 3f * p2 + p3) * t3
            );
        }

        /// <summary>
        /// Draws the primitives to the screen.
        /// </summary>
        private static void DrawPrimitives(int vertexCount, int triangleCount, MiscShaderData shader)
        {
            GraphicsDevice device = Main.instance.GraphicsDevice;

            // Calculate view/projection matrices
            Matrix view = Matrix.CreateLookAt(Vector3.Backward, Vector3.Zero, Vector3.Up);
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            basicEffect.View = view;
            basicEffect.Projection = projection;
            basicEffect.World = Matrix.Identity;

            // Apply shader if provided
            if (shader != null)
            {
                shader.Apply();
            }
            else
            {
                foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                }
            }

            device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertexCount, indices, 0, triangleCount);
        }

        /// <summary>
        /// Renders a trail from a projectile's oldPos array with simple settings.
        /// </summary>
        public static void RenderProjectileTrail(Projectile projectile, Color startColor, Color endColor, 
            float startWidth, float endWidth, Vector2 offset = default)
        {
            RenderTrail(projectile.oldPos, new TrailSettings(
                completionRatio => MathHelper.Lerp(startWidth, endWidth, completionRatio),
                completionRatio => Color.Lerp(startColor, endColor, completionRatio),
                _ => projectile.Size * 0.5f + offset,
                smoothen: true
            ), Math.Min(projectile.oldPos.Length, 50));
        }

        /// <summary>
        /// Renders a glowing trail with multiple layers for enhanced effect.
        /// </summary>
        public static void RenderGlowingTrail(Vector2[] positions, Color coreColor, Color glowColor, 
            float width, float glowWidth, int points = 50)
        {
            // Draw outer glow layer
            RenderTrail(positions, new TrailSettings(
                c => MathHelper.Lerp(glowWidth, 0, c),
                c => glowColor * (1f - c) * 0.5f,
                smoothen: true
            ), points);

            // Draw core layer
            RenderTrail(positions, new TrailSettings(
                c => MathHelper.Lerp(width, 0, c),
                c => Color.Lerp(coreColor, Color.White, 0.3f) * (1f - c),
                smoothen: true
            ), points);
        }
    }
}
