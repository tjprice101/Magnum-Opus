using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Shaders;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// A simplified primitive trail renderer for swing effects.
    /// This handles vertex buffer construction and rendering for ribbon-style trails.
    /// 
    /// USAGE:
    /// 1. Call SetupTrailShader() before Begin() to apply custom shaders
    /// 2. Use DrawTrail() to render a list of positions as a ribbon
    /// 3. Use fallback methods if shaders aren't available
    /// 
    /// Based on Calamity's PrimitiveRenderer patterns.
    /// </summary>
    public static class SimplePrimitiveTrail
    {
        /// <summary>
        /// Delegate for calculating trail width at a given progress point.
        /// </summary>
        /// <param name="progress">0 = start, 1 = end of trail</param>
        /// <returns>Width in pixels</returns>
        public delegate float WidthFunction(float progress);

        /// <summary>
        /// Delegate for calculating trail color at a given progress point.
        /// </summary>
        /// <param name="progress">0 = start, 1 = end of trail</param>
        /// <returns>Color at that point</returns>
        public delegate Color ColorFunction(float progress);

        /// <summary>
        /// Delegate for calculating trail offset at a given progress point.
        /// </summary>
        /// <param name="progress">0 = start, 1 = end of trail</param>
        /// <returns>Offset vector to apply</returns>
        public delegate Vector2 OffsetFunction(float progress);

        private static Effect _currentShader;
        private static bool _useShader;

        /// <summary>
        /// Sets up a custom shader for trail rendering.
        /// Call this before DrawTrail() if you want to use a shader.
        /// </summary>
        public static void SetupTrailShader(Effect shader)
        {
            _currentShader = shader;
            _useShader = shader != null;
        }

        /// <summary>
        /// Clears the current shader, reverting to basic rendering.
        /// </summary>
        public static void ClearShader()
        {
            _currentShader = null;
            _useShader = false;
        }

        /// <summary>
        /// Draws a ribbon trail through the given positions.
        /// </summary>
        /// <param name="positions">List of positions defining the trail spine</param>
        /// <param name="widthFunc">Function to calculate width at each point</param>
        /// <param name="colorFunc">Function to calculate color at each point</param>
        /// <param name="offsetFunc">Optional function to offset positions (can be null)</param>
        /// <param name="smoothen">Whether to add interpolated points for smoother curves</param>
        public static void DrawTrail(
            List<Vector2> positions,
            WidthFunction widthFunc,
            ColorFunction colorFunc,
            OffsetFunction offsetFunc = null,
            bool smoothen = true)
        {
            if (positions == null || positions.Count < 2)
                return;

            // Apply smoothing if requested
            List<Vector2> trailPositions = smoothen ? SmoothPositions(positions) : positions;

            // Build vertices
            var vertices = new List<VertexPositionColorTexture>();

            for (int i = 0; i < trailPositions.Count; i++)
            {
                float progress = (float)i / (trailPositions.Count - 1);
                float width = widthFunc(progress);
                Color color = colorFunc(progress);

                Vector2 pos = trailPositions[i];
                if (offsetFunc != null)
                    pos += offsetFunc(progress);

                // Calculate perpendicular direction for ribbon width
                Vector2 perpendicular;
                if (i == 0)
                {
                    perpendicular = (trailPositions[1] - trailPositions[0]).SafeNormalize(Vector2.UnitY);
                }
                else if (i == trailPositions.Count - 1)
                {
                    perpendicular = (trailPositions[i] - trailPositions[i - 1]).SafeNormalize(Vector2.UnitY);
                }
                else
                {
                    // Average of incoming and outgoing directions for smooth corners
                    Vector2 dir1 = (trailPositions[i] - trailPositions[i - 1]).SafeNormalize(Vector2.UnitY);
                    Vector2 dir2 = (trailPositions[i + 1] - trailPositions[i]).SafeNormalize(Vector2.UnitY);
                    perpendicular = ((dir1 + dir2) / 2f).SafeNormalize(Vector2.UnitY);
                }

                // Rotate 90 degrees to get perpendicular
                perpendicular = new Vector2(-perpendicular.Y, perpendicular.X);

                // Create two vertices per position (top and bottom of ribbon)
                Vector2 topPos = pos + perpendicular * (width / 2f);
                Vector2 bottomPos = pos - perpendicular * (width / 2f);

                // Convert to screen coordinates
                topPos -= Main.screenPosition;
                bottomPos -= Main.screenPosition;

                // U = progress along trail, V = 0 for top, 1 for bottom
                // Z coordinate stores width factor for shader correction
                float widthFactor = width / 50f; // Normalize width

                vertices.Add(new VertexPositionColorTexture(
                    new Vector3(topPos, 0),
                    color,
                    new Vector3(progress, 0, widthFactor)));

                vertices.Add(new VertexPositionColorTexture(
                    new Vector3(bottomPos, 0),
                    color,
                    new Vector3(progress, 1, widthFactor)));
            }

            if (vertices.Count < 4)
                return;

            // Build triangle strip indices
            var indices = new List<short>();
            for (int i = 0; i < vertices.Count - 2; i++)
            {
                indices.Add((short)i);
                indices.Add((short)(i + 1));
                indices.Add((short)(i + 2));
            }

            // Render
            RenderPrimitives(vertices.ToArray(), indices.ToArray());
        }

        /// <summary>
        /// Smooths a list of positions using Catmull-Rom interpolation.
        /// </summary>
        private static List<Vector2> SmoothPositions(List<Vector2> positions)
        {
            if (positions.Count < 3)
                return positions;

            var smoothed = new List<Vector2>();
            int subdivisions = 3; // Add this many points between each pair

            for (int i = 0; i < positions.Count - 1; i++)
            {
                Vector2 p0 = positions[Math.Max(0, i - 1)];
                Vector2 p1 = positions[i];
                Vector2 p2 = positions[i + 1];
                Vector2 p3 = positions[Math.Min(positions.Count - 1, i + 2)];

                for (int j = 0; j < subdivisions; j++)
                {
                    float t = (float)j / subdivisions;
                    smoothed.Add(CatmullRom(p0, p1, p2, p3, t));
                }
            }

            // Add the final position
            smoothed.Add(positions[positions.Count - 1]);

            return smoothed;
        }

        /// <summary>
        /// Catmull-Rom spline interpolation.
        /// </summary>
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

        /// <summary>
        /// Renders primitive vertices with optional shader.
        /// </summary>
        private static void RenderPrimitives(VertexPositionColorTexture[] vertices, short[] indices)
        {
            GraphicsDevice device = Main.graphics.GraphicsDevice;

            // Save current state
            RasterizerState oldRasterizer = device.RasterizerState;
            BlendState oldBlend = device.BlendState;
            SamplerState oldSampler = device.SamplerStates[0];

            try
            {
                // Set up rendering state
                device.RasterizerState = RasterizerState.CullNone;
                device.BlendState = BlendState.Additive;
                device.SamplerStates[0] = SamplerState.LinearWrap;

                if (_useShader && _currentShader != null)
                {
                    // Apply shader
                    _currentShader.CurrentTechnique.Passes[0].Apply();
                }
                else
                {
                    // Use basic effect for fallback
                    var basicEffect = new BasicEffect(device)
                    {
                        VertexColorEnabled = true,
                        TextureEnabled = false,
                        World = Matrix.Identity,
                        View = Matrix.Identity,
                        Projection = Matrix.CreateOrthographicOffCenter(
                            0, Main.screenWidth, Main.screenHeight, 0, 0, 1)
                    };

                    basicEffect.CurrentTechnique.Passes[0].Apply();
                }

                // Draw the primitives
                device.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    vertices,
                    0,
                    vertices.Length,
                    indices,
                    0,
                    indices.Length / 3);
            }
            finally
            {
                // Restore state
                device.RasterizerState = oldRasterizer;
                device.BlendState = oldBlend;
                device.SamplerStates[0] = oldSampler;
            }
        }

        /// <summary>
        /// Creates a simple linear width function.
        /// </summary>
        /// <param name="startWidth">Width at start of trail</param>
        /// <param name="endWidth">Width at end of trail (usually 0)</param>
        public static WidthFunction LinearWidth(float startWidth, float endWidth = 0f)
        {
            return progress => MathHelper.Lerp(startWidth, endWidth, progress);
        }

        /// <summary>
        /// Creates a smooth width function using sine curve.
        /// </summary>
        /// <param name="maxWidth">Maximum width at center</param>
        public static WidthFunction SineWidth(float maxWidth)
        {
            return progress => (float)Math.Sin(progress * MathHelper.Pi) * maxWidth;
        }

        /// <summary>
        /// Creates a Calamity-style "sword slash" width function.
        /// Wide at the start, tapering quickly, then a thin tail.
        /// </summary>
        /// <param name="maxWidth">Maximum width near start</param>
        public static WidthFunction SlashWidth(float maxWidth)
        {
            return progress =>
            {
                // Quick taper from wide to narrow
                float taper = (float)Math.Pow(1 - progress, 2);
                // Slight bulge at the start
                float bulge = 1f + (float)Math.Sin(progress * MathHelper.Pi * 0.5f) * 0.3f;
                return maxWidth * taper * bulge;
            };
        }

        /// <summary>
        /// Creates a simple gradient color function.
        /// </summary>
        public static ColorFunction GradientColor(Color start, Color end)
        {
            return progress => Color.Lerp(start, end, progress);
        }

        /// <summary>
        /// Creates a multi-stop gradient color function.
        /// </summary>
        public static ColorFunction MultiGradient(params Color[] colors)
        {
            return progress =>
            {
                if (colors.Length == 0) return Color.White;
                if (colors.Length == 1) return colors[0];

                float scaledProgress = progress * (colors.Length - 1);
                int index = (int)scaledProgress;
                float localProgress = scaledProgress - index;

                if (index >= colors.Length - 1)
                    return colors[colors.Length - 1];

                return Color.Lerp(colors[index], colors[index + 1], localProgress);
            };
        }

        /// <summary>
        /// Creates a color function with alpha fade.
        /// </summary>
        public static ColorFunction ColorWithFade(Color baseColor, float startAlpha = 1f, float endAlpha = 0f)
        {
            return progress =>
            {
                float alpha = MathHelper.Lerp(startAlpha, endAlpha, progress);
                return baseColor * alpha;
            };
        }
    }

    /// <summary>
    /// Custom vertex structure that includes a 3-component texture coordinate.
    /// The Z component is used to pass additional data to the shader (like width scaling).
    /// </summary>
    public struct VertexPositionColorTexture : IVertexType
    {
        public Vector3 Position;
        public Color Color;
        public Vector3 TextureCoordinate;

        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new VertexElement(16, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0)
        );

        VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;

        public VertexPositionColorTexture(Vector3 position, Color color, Vector3 textureCoordinate)
        {
            Position = position;
            Color = color;
            TextureCoordinate = textureCoordinate;
        }
    }
}
