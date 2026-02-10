using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// NEBULA FOG VERTEX MESH SYSTEM
    /// 
    /// Creates circular/blob vertex meshes for fog cloud rendering with:
    /// - Edge softening via alpha falloff at vertices
    /// - Radial masking for soft circular boundaries
    /// - Custom vertex structure for shader time-based effects
    /// 
    /// Used by NebulaFogSystem for GPU-accelerated fog rendering.
    /// </summary>
    public static class NebulaFogMesh
    {
        #region Custom Vertex Structure
        
        /// <summary>
        /// Custom vertex for fog rendering with time-based UV animation.
        /// </summary>
        public struct FogVertex : IVertexType
        {
            public Vector3 Position;
            public Color Color;
            public Vector2 TexCoord;
            public float RadialDistance; // Distance from center (0 = center, 1 = edge)
            public float TimeOffset; // For per-vertex animation offset
            
            public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(
                new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                new VertexElement(12, VertexElementFormat.Color, VertexElementUsage.Color, 0),
                new VertexElement(16, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
                new VertexElement(24, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 1),
                new VertexElement(28, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 2)
            );
            
            VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;
            
            public FogVertex(Vector2 screenPos, Color color, Vector2 texCoord, float radialDist, float timeOffset)
            {
                Position = new Vector3(screenPos, 0);
                Color = color;
                TexCoord = texCoord;
                RadialDistance = radialDist;
                TimeOffset = timeOffset;
            }
        }
        
        #endregion
        
        #region Mesh Generation
        
        /// <summary>
        /// Generates a circular fog mesh with radial alpha falloff.
        /// </summary>
        /// <param name="center">World position center</param>
        /// <param name="radius">Fog radius</param>
        /// <param name="segments">Number of circle segments (more = smoother)</param>
        /// <param name="primaryColor">Center color</param>
        /// <param name="edgeColor">Edge color (typically more transparent)</param>
        /// <param name="rotation">Rotation angle</param>
        /// <returns>Vertex and index arrays for drawing</returns>
        public static (FogVertex[] vertices, short[] indices) GenerateCircularFogMesh(
            Vector2 center,
            float radius,
            int segments,
            Color primaryColor,
            Color edgeColor,
            float rotation = 0f)
        {
            // Vertex count: center + ring vertices
            int vertexCount = 1 + segments;
            FogVertex[] vertices = new FogVertex[vertexCount];
            
            // Index count: triangles from center to each edge segment
            int triangleCount = segments;
            short[] indices = new short[triangleCount * 3];
            
            Vector2 screenCenter = center - Main.screenPosition;
            
            // Center vertex - full opacity
            vertices[0] = new FogVertex(
                screenCenter,
                primaryColor,
                new Vector2(0.5f, 0.5f),
                0f, // Radial distance 0 at center
                0f  // No time offset at center
            );
            
            // Edge vertices - faded opacity
            for (int i = 0; i < segments; i++)
            {
                float angle = rotation + MathHelper.TwoPi * i / segments;
                float x = (float)Math.Cos(angle);
                float y = (float)Math.Sin(angle);
                
                Vector2 edgePos = screenCenter + new Vector2(x, y) * radius;
                Vector2 texCoord = new Vector2(0.5f + x * 0.5f, 0.5f + y * 0.5f);
                
                // Edge alpha falloff
                Color vertexColor = edgeColor;
                vertexColor.A = 0; // Fully transparent at edge for smooth blend
                
                vertices[i + 1] = new FogVertex(
                    edgePos,
                    vertexColor,
                    texCoord,
                    1f, // Radial distance 1 at edge
                    i / (float)segments // Time offset varies around circle
                );
            }
            
            // Triangle indices (fan from center)
            for (int i = 0; i < segments; i++)
            {
                int baseIndex = i * 3;
                indices[baseIndex] = 0; // Center
                indices[baseIndex + 1] = (short)(i + 1);
                indices[baseIndex + 2] = (short)(((i + 1) % segments) + 1);
            }
            
            return (vertices, indices);
        }
        
        /// <summary>
        /// Generates an organic "blob" fog mesh with irregular edges.
        /// </summary>
        public static (FogVertex[] vertices, short[] indices) GenerateBlobFogMesh(
            Vector2 center,
            float baseRadius,
            int segments,
            Color primaryColor,
            Color edgeColor,
            float rotation = 0f,
            float blobiness = 0.2f,
            int seed = 0)
        {
            int vertexCount = 1 + segments;
            FogVertex[] vertices = new FogVertex[vertexCount];
            
            int triangleCount = segments;
            short[] indices = new short[triangleCount * 3];
            
            Vector2 screenCenter = center - Main.screenPosition;
            Random rand = new Random(seed);
            
            // Center vertex
            vertices[0] = new FogVertex(
                screenCenter,
                primaryColor,
                new Vector2(0.5f, 0.5f),
                0f,
                0f
            );
            
            // Pre-calculate blob radius variations
            float[] radiusVariations = new float[segments];
            for (int i = 0; i < segments; i++)
            {
                // Smooth noise-like variation using multiple sine waves
                float t = i / (float)segments;
                float variation = 0f;
                variation += (float)Math.Sin(t * MathHelper.TwoPi * 2 + seed) * 0.3f;
                variation += (float)Math.Sin(t * MathHelper.TwoPi * 3 + seed * 2.7f) * 0.2f;
                variation += (float)Math.Sin(t * MathHelper.TwoPi * 5 + seed * 4.3f) * 0.15f;
                radiusVariations[i] = 1f + variation * blobiness;
            }
            
            // Edge vertices with blob variation
            for (int i = 0; i < segments; i++)
            {
                float angle = rotation + MathHelper.TwoPi * i / segments;
                float x = (float)Math.Cos(angle);
                float y = (float)Math.Sin(angle);
                
                float blobRadius = baseRadius * radiusVariations[i];
                Vector2 edgePos = screenCenter + new Vector2(x, y) * blobRadius;
                Vector2 texCoord = new Vector2(0.5f + x * 0.5f, 0.5f + y * 0.5f);
                
                // Calculate alpha based on radius variation (thinner parts more transparent)
                byte alphaValue = (byte)(255 * (1f - radiusVariations[i] * 0.3f) * 0.2f);
                Color vertexColor = new Color(edgeColor.R, edgeColor.G, edgeColor.B, alphaValue);
                
                vertices[i + 1] = new FogVertex(
                    edgePos,
                    vertexColor,
                    texCoord,
                    1f,
                    i / (float)segments
                );
            }
            
            // Triangle indices
            for (int i = 0; i < segments; i++)
            {
                int baseIndex = i * 3;
                indices[baseIndex] = 0;
                indices[baseIndex + 1] = (short)(i + 1);
                indices[baseIndex + 2] = (short)(((i + 1) % segments) + 1);
            }
            
            return (vertices, indices);
        }
        
        /// <summary>
        /// Generates a trail-shaped fog mesh with smooth width falloff.
        /// </summary>
        public static (FogVertex[] vertices, short[] indices) GenerateTrailFogMesh(
            List<Vector2> trailPoints,
            float baseWidth,
            Color startColor,
            Color endColor)
        {
            if (trailPoints.Count < 2)
                return (new FogVertex[0], new short[0]);
            
            int pointCount = trailPoints.Count;
            int vertexCount = pointCount * 2; // Two vertices per point (top and bottom)
            FogVertex[] vertices = new FogVertex[vertexCount];
            
            // Quad strip: (pointCount - 1) quads, 2 triangles each
            int indexCount = (pointCount - 1) * 6;
            short[] indices = new short[indexCount];
            
            for (int i = 0; i < pointCount; i++)
            {
                float progress = i / (float)(pointCount - 1);
                Vector2 point = trailPoints[i] - Main.screenPosition;
                
                // Calculate perpendicular direction
                Vector2 direction;
                if (i == 0)
                    direction = (trailPoints[1] - trailPoints[0]).SafeNormalize(Vector2.UnitX);
                else if (i == pointCount - 1)
                    direction = (trailPoints[i] - trailPoints[i - 1]).SafeNormalize(Vector2.UnitX);
                else
                    direction = (trailPoints[i + 1] - trailPoints[i - 1]).SafeNormalize(Vector2.UnitX);
                
                Vector2 perpendicular = new Vector2(-direction.Y, direction.X);
                
                // Width tapers toward end
                float width = baseWidth * (1f - progress * 0.6f);
                
                // Color interpolation
                Color color = Color.Lerp(startColor, endColor, progress);
                
                // Top vertex
                Vector2 topPos = point + perpendicular * width * 0.5f;
                vertices[i * 2] = new FogVertex(
                    topPos,
                    new Color(color.R, color.G, color.B, (byte)(color.A * 0.3f)), // Edge alpha
                    new Vector2(progress, 0f),
                    1f, // Edge
                    progress
                );
                
                // Bottom vertex
                Vector2 bottomPos = point - perpendicular * width * 0.5f;
                vertices[i * 2 + 1] = new FogVertex(
                    bottomPos,
                    new Color(color.R, color.G, color.B, (byte)(color.A * 0.3f)),
                    new Vector2(progress, 1f),
                    1f,
                    progress
                );
            }
            
            // Generate quad indices
            for (int i = 0; i < pointCount - 1; i++)
            {
                int baseIndex = i * 6;
                int baseVertex = i * 2;
                
                // First triangle
                indices[baseIndex] = (short)baseVertex;
                indices[baseIndex + 1] = (short)(baseVertex + 1);
                indices[baseIndex + 2] = (short)(baseVertex + 2);
                
                // Second triangle
                indices[baseIndex + 3] = (short)(baseVertex + 1);
                indices[baseIndex + 4] = (short)(baseVertex + 3);
                indices[baseIndex + 5] = (short)(baseVertex + 2);
            }
            
            return (vertices, indices);
        }
        
        #endregion
        
        #region Mesh Drawing
        
        private static BasicEffect _basicEffect;
        
        /// <summary>
        /// Draws a fog mesh using the graphics device.
        /// </summary>
        public static void DrawFogMesh(GraphicsDevice device, FogVertex[] vertices, short[] indices, Effect shader = null)
        {
            if (vertices.Length == 0 || indices.Length == 0)
                return;
            
            // Create basic effect if needed (for fallback rendering)
            if (_basicEffect == null)
            {
                _basicEffect = new BasicEffect(device)
                {
                    VertexColorEnabled = true,
                    TextureEnabled = false,
                    View = Matrix.Identity,
                    World = Matrix.Identity
                };
            }
            
            // Set projection matrix
            _basicEffect.Projection = Matrix.CreateOrthographicOffCenter(
                0, device.Viewport.Width,
                device.Viewport.Height, 0,
                0, 1
            );
            
            // Use provided shader or basic effect
            Effect effectToUse = shader ?? _basicEffect;
            
            foreach (EffectPass pass in effectToUse.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    vertices,
                    0,
                    vertices.Length,
                    indices,
                    0,
                    indices.Length / 3
                );
            }
        }
        
        /// <summary>
        /// Draws a circular fog cloud with radial falloff.
        /// </summary>
        public static void DrawCircularFog(
            GraphicsDevice device,
            Vector2 center,
            float radius,
            Color primaryColor,
            Color edgeColor,
            float rotation = 0f,
            int segments = 24,
            Effect shader = null)
        {
            var (vertices, indices) = GenerateCircularFogMesh(center, radius, segments, primaryColor, edgeColor, rotation);
            DrawFogMesh(device, vertices, indices, shader);
        }
        
        /// <summary>
        /// Draws a blob-shaped fog cloud.
        /// </summary>
        public static void DrawBlobFog(
            GraphicsDevice device,
            Vector2 center,
            float radius,
            Color primaryColor,
            Color edgeColor,
            float rotation = 0f,
            float blobiness = 0.2f,
            int seed = 0,
            int segments = 32,
            Effect shader = null)
        {
            var (vertices, indices) = GenerateBlobFogMesh(center, radius, segments, primaryColor, edgeColor, rotation, blobiness, seed);
            DrawFogMesh(device, vertices, indices, shader);
        }
        
        /// <summary>
        /// Draws a trail-shaped fog overlay.
        /// </summary>
        public static void DrawTrailFog(
            GraphicsDevice device,
            List<Vector2> trailPoints,
            float width,
            Color startColor,
            Color endColor,
            Effect shader = null)
        {
            var (vertices, indices) = GenerateTrailFogMesh(trailPoints, width, startColor, endColor);
            DrawFogMesh(device, vertices, indices, shader);
        }
        
        #endregion
        
        #region Animated Fog Mesh
        
        /// <summary>
        /// Generates an animated fog mesh with time-based vertex wobble.
        /// </summary>
        public static (FogVertex[] vertices, short[] indices) GenerateAnimatedFogMesh(
            Vector2 center,
            float radius,
            int segments,
            Color primaryColor,
            Color edgeColor,
            float time,
            float wobbleIntensity = 0.15f,
            float wobbleSpeed = 2f)
        {
            int vertexCount = 1 + segments;
            FogVertex[] vertices = new FogVertex[vertexCount];
            
            int triangleCount = segments;
            short[] indices = new short[triangleCount * 3];
            
            Vector2 screenCenter = center - Main.screenPosition;
            
            // Center vertex with subtle pulsing
            float centerPulse = 1f + (float)Math.Sin(time * wobbleSpeed) * 0.05f;
            Color centerColor = new Color(
                (int)(primaryColor.R * centerPulse),
                (int)(primaryColor.G * centerPulse),
                (int)(primaryColor.B * centerPulse),
                primaryColor.A
            );
            
            vertices[0] = new FogVertex(
                screenCenter,
                centerColor,
                new Vector2(0.5f, 0.5f),
                0f,
                time
            );
            
            // Edge vertices with animated wobble
            for (int i = 0; i < segments; i++)
            {
                float baseAngle = MathHelper.TwoPi * i / segments;
                float angle = baseAngle;
                
                // Multi-frequency wobble
                float wobble = 0f;
                wobble += (float)Math.Sin(baseAngle * 3 + time * wobbleSpeed) * wobbleIntensity;
                wobble += (float)Math.Sin(baseAngle * 5 + time * wobbleSpeed * 1.3f) * wobbleIntensity * 0.5f;
                wobble += (float)Math.Sin(baseAngle * 7 + time * wobbleSpeed * 0.7f) * wobbleIntensity * 0.3f;
                
                float animatedRadius = radius * (1f + wobble);
                
                float x = (float)Math.Cos(angle);
                float y = (float)Math.Sin(angle);
                
                Vector2 edgePos = screenCenter + new Vector2(x, y) * animatedRadius;
                Vector2 texCoord = new Vector2(0.5f + x * 0.5f, 0.5f + y * 0.5f);
                
                // Edge color with distance-based alpha
                float alphaFactor = 1f - Math.Abs(wobble) * 2f; // Thinner parts more transparent
                alphaFactor = MathHelper.Clamp(alphaFactor, 0.1f, 1f);
                Color vertexColor = new Color(
                    edgeColor.R,
                    edgeColor.G,
                    edgeColor.B,
                    (byte)(edgeColor.A * alphaFactor * 0.2f)
                );
                
                vertices[i + 1] = new FogVertex(
                    edgePos,
                    vertexColor,
                    texCoord,
                    1f,
                    time + i / (float)segments
                );
            }
            
            // Triangle indices
            for (int i = 0; i < segments; i++)
            {
                int baseIndex = i * 3;
                indices[baseIndex] = 0;
                indices[baseIndex + 1] = (short)(i + 1);
                indices[baseIndex + 2] = (short)(((i + 1) % segments) + 1);
            }
            
            return (vertices, indices);
        }
        
        /// <summary>
        /// Draws an animated fog cloud with real-time vertex animation.
        /// </summary>
        public static void DrawAnimatedFog(
            GraphicsDevice device,
            Vector2 center,
            float radius,
            Color primaryColor,
            Color edgeColor,
            float wobbleIntensity = 0.15f,
            float wobbleSpeed = 2f,
            int segments = 32,
            Effect shader = null)
        {
            var (vertices, indices) = GenerateAnimatedFogMesh(
                center, radius, segments, primaryColor, edgeColor,
                Main.GlobalTimeWrappedHourly, wobbleIntensity, wobbleSpeed
            );
            DrawFogMesh(device, vertices, indices, shader);
        }
        
        #endregion
    }
}
