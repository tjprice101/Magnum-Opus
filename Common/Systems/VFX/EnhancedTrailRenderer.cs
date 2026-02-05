using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Shaders;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// Enhanced primitive trail renderer implementing FargosSoulsDLC patterns.
    /// Provides multi-pass rendering, width/color functions, and proper bloom support.
    /// 
    /// Key patterns implemented:
    /// - PrimitiveSettings struct with delegates
    /// - Multi-pass rendering (bloom pass, main pass, core pass)
    /// - Width functions: Linear taper, QuadraticBump, InverseLerpBump
    /// - Color functions: Gradient, bump opacity, palette lerp
    /// </summary>
    public static class EnhancedTrailRenderer
    {
        private static BasicEffect _basicEffect;
        private static VertexPositionColorTexture[] _vertices;
        private static short[] _indices;
        
        private const int MaxVertices = 2048;
        private const int MaxIndices = 4096;
        
        /// <summary>
        /// Delegate for calculating trail width at a given point.
        /// </summary>
        /// <param name="completionRatio">0 at trail start, 1 at trail end</param>
        public delegate float WidthFunction(float completionRatio);
        
        /// <summary>
        /// Delegate for calculating trail color at a given point.
        /// </summary>
        /// <param name="completionRatio">0 at trail start, 1 at trail end</param>
        public delegate Color ColorFunction(float completionRatio);
        
        /// <summary>
        /// Settings for primitive trail rendering.
        /// Mirrors FargosSoulsDLC's PrimitiveSettings pattern.
        /// </summary>
        public struct PrimitiveSettings
        {
            public WidthFunction WidthFunc;
            public ColorFunction ColorFunc;
            public Func<float, Vector2> OffsetFunc;
            public bool Smoothen;
            public MiscShaderData Shader;
            
            public PrimitiveSettings(
                WidthFunction width,
                ColorFunction color,
                Func<float, Vector2> offset = null,
                bool smoothen = true,
                MiscShaderData shader = null)
            {
                WidthFunc = width;
                ColorFunc = color;
                OffsetFunc = offset;
                Smoothen = smoothen;
                Shader = shader;
            }
        }
        
        #region Initialization
        
        /// <summary>
        /// Initializes the renderer. Called automatically on first use.
        /// </summary>
        public static void Initialize()
        {
            if (Main.dedServ) return;
            
            _basicEffect = new BasicEffect(Main.instance.GraphicsDevice)
            {
                VertexColorEnabled = true,
                TextureEnabled = false
            };
            
            _vertices = new VertexPositionColorTexture[MaxVertices];
            _indices = new short[MaxIndices];
        }
        
        #endregion
        
        #region Width Function Presets
        
        /// <summary>
        /// Linear taper from start width to 0. The most common pattern.
        /// </summary>
        public static WidthFunction LinearTaper(float startWidth)
        {
            return completionRatio => startWidth * (1f - completionRatio);
        }
        
        /// <summary>
        /// Quadratic bump - thin at both ends, thick in middle.
        /// Perfect for projectile trails and energy beams.
        /// </summary>
        public static WidthFunction QuadraticBumpWidth(float maxWidth)
        {
            return completionRatio => 
                maxWidth * MathF.Sin(completionRatio * MathHelper.Pi);
        }
        
        /// <summary>
        /// FargosSoulsDLC-style InverseLerp width with bump in the middle section.
        /// Creates a profile that's thin at start, full in middle, tapers at end.
        /// </summary>
        public static WidthFunction InverseLerpBumpWidth(float minWidth, float maxWidth,
            float rampUpEnd = 0.27f, float rampDownStart = 0.72f)
        {
            return completionRatio =>
            {
                float widthInterpolant = VFXUtilities.InverseLerp(0.06f, rampUpEnd, completionRatio);
                widthInterpolant *= VFXUtilities.InverseLerp(0.9f, rampDownStart, completionRatio);
                return MathHelper.Lerp(minWidth, maxWidth, widthInterpolant);
            };
        }
        
        /// <summary>
        /// Constant width that fades only at the very end.
        /// Good for laser beams.
        /// </summary>
        public static WidthFunction ConstantWithFade(float width, float fadeStart = 0.8f)
        {
            return completionRatio =>
            {
                if (completionRatio < fadeStart)
                    return width;
                float fadeProgress = (completionRatio - fadeStart) / (1f - fadeStart);
                return width * (1f - fadeProgress);
            };
        }
        
        /// <summary>
        /// Creates a bloom width function (multiplied by a factor).
        /// Used for the outer glow pass.
        /// </summary>
        public static WidthFunction BloomWidth(WidthFunction baseWidth, float multiplier = 2.7f)
        {
            return completionRatio => baseWidth(completionRatio) * multiplier;
        }
        
        #endregion
        
        #region Color Function Presets
        
        /// <summary>
        /// Simple solid color with opacity fade along the trail.
        /// </summary>
        public static ColorFunction SolidColorFade(Color color, float opacity = 1f)
        {
            return completionRatio => color * opacity * (1f - completionRatio);
        }
        
        /// <summary>
        /// Gradient from start color to end color along the trail.
        /// </summary>
        public static ColorFunction GradientColor(Color startColor, Color endColor, float opacity = 1f)
        {
            return completionRatio =>
                Color.Lerp(startColor, endColor, completionRatio) * opacity;
        }
        
        /// <summary>
        /// Theme palette gradient along the trail.
        /// </summary>
        public static ColorFunction PaletteColor(Color[] palette, float opacity = 1f)
        {
            return completionRatio =>
                VFXUtilities.PaletteLerp(palette, completionRatio) * opacity;
        }
        
        /// <summary>
        /// Color with bump opacity (bright in middle section).
        /// </summary>
        public static ColorFunction BumpOpacityColor(Color color, 
            float rampUpStart = 0.02f, float rampUpEnd = 0.05f,
            float rampDownStart = 0.81f, float rampDownEnd = 0.95f)
        {
            return completionRatio =>
            {
                float opacity = VFXUtilities.InverseLerpBump(
                    rampUpStart, rampUpEnd, rampDownStart, rampDownEnd, completionRatio);
                return color * opacity;
            };
        }
        
        /// <summary>
        /// Creates a bloom color function (dimmer version for outer glow).
        /// </summary>
        public static ColorFunction BloomColor(ColorFunction baseColor, float dimFactor = 0.3f)
        {
            return completionRatio => baseColor(completionRatio) * dimFactor;
        }
        
        /// <summary>
        /// Theme-based color function.
        /// </summary>
        public static ColorFunction ThemeColor(string themeName, float opacity = 1f)
        {
            Color[] palette = themeName.ToLowerInvariant() switch
            {
                "lacampanella" or "campanella" => MagnumThemePalettes.LaCampanella,
                "eroica" => MagnumThemePalettes.Eroica,
                "moonlight" or "moonlightsonata" => MagnumThemePalettes.MoonlightSonata,
                "swanlake" or "swan" => MagnumThemePalettes.SwanLake,
                "enigma" or "enigmavariations" => MagnumThemePalettes.EnigmaVariations,
                "fate" => MagnumThemePalettes.Fate,
                "clair" or "clairdelune" => MagnumThemePalettes.ClairDeLune,
                "dies" or "diesirae" => MagnumThemePalettes.DiesIrae,
                _ => new[] { Color.White }
            };
            return PaletteColor(palette, opacity);
        }
        
        #endregion
        
        #region Rendering Methods
        
        /// <summary>
        /// Renders a trail using the given positions and settings.
        /// </summary>
        /// <param name="positions">Array of world positions</param>
        /// <param name="settings">Trail rendering settings</param>
        /// <param name="segmentCount">Number of segments to render</param>
        public static void RenderTrail(Vector2[] positions, PrimitiveSettings settings, int segmentCount = 50)
        {
            if (Main.dedServ || positions == null || positions.Length < 2)
                return;
            
            if (_basicEffect == null)
                Initialize();
            
            // Filter out zero positions
            List<Vector2> validPositions = new List<Vector2>();
            foreach (var pos in positions)
            {
                if (pos != Vector2.Zero)
                    validPositions.Add(pos);
            }
            
            if (validPositions.Count < 2) return;
            
            // Optionally smooth the trail
            List<Vector2> finalPositions;
            if (settings.Smoothen && validPositions.Count >= 4)
                finalPositions = SmoothTrail(validPositions, segmentCount);
            else
                finalPositions = validPositions;
            
            if (finalPositions.Count < 2) return;
            
            // Build vertices
            int vertexCount = finalPositions.Count * 2;
            int indexCount = (finalPositions.Count - 1) * 6;
            
            if (vertexCount > MaxVertices || indexCount > MaxIndices)
                return;
            
            for (int i = 0; i < finalPositions.Count; i++)
            {
                float completionRatio = (float)i / (finalPositions.Count - 1);
                float width = settings.WidthFunc?.Invoke(completionRatio) ?? 10f;
                Color color = settings.ColorFunc?.Invoke(completionRatio) ?? Color.White;
                
                // CRITICAL: Remove alpha for proper additive blending
                color = color.WithoutAlpha();
                
                // Calculate perpendicular direction
                Vector2 direction;
                if (i == 0)
                    direction = (finalPositions[1] - finalPositions[0]).SafeNormalize(Vector2.UnitY);
                else if (i == finalPositions.Count - 1)
                    direction = (finalPositions[i] - finalPositions[i - 1]).SafeNormalize(Vector2.UnitY);
                else
                    direction = (finalPositions[i + 1] - finalPositions[i - 1]).SafeNormalize(Vector2.UnitY);
                
                Vector2 perpendicular = new Vector2(-direction.Y, direction.X);
                Vector2 worldPos = finalPositions[i];
                
                // Apply offset
                if (settings.OffsetFunc != null)
                    worldPos += settings.OffsetFunc(completionRatio);
                
                // Convert to screen position
                Vector2 screenPos = worldPos - Main.screenPosition;
                
                // Create vertices
                _vertices[i * 2] = new VertexPositionColorTexture(
                    new Vector3(screenPos + perpendicular * width * 0.5f, 0),
                    color,
                    new Vector2(completionRatio, 0));
                
                _vertices[i * 2 + 1] = new VertexPositionColorTexture(
                    new Vector3(screenPos - perpendicular * width * 0.5f, 0),
                    color,
                    new Vector2(completionRatio, 1));
            }
            
            // Build indices
            int idx = 0;
            for (int i = 0; i < finalPositions.Count - 1; i++)
            {
                int baseVertex = i * 2;
                _indices[idx++] = (short)baseVertex;
                _indices[idx++] = (short)(baseVertex + 1);
                _indices[idx++] = (short)(baseVertex + 2);
                _indices[idx++] = (short)(baseVertex + 1);
                _indices[idx++] = (short)(baseVertex + 3);
                _indices[idx++] = (short)(baseVertex + 2);
            }
            
            // Render
            DrawPrimitives(vertexCount, indexCount / 3, settings.Shader);
        }
        
        /// <summary>
        /// Renders a multi-pass trail with bloom, main, and core layers.
        /// This is the FargosSoulsDLC standard for high-quality trails.
        /// </summary>
        public static void RenderMultiPassTrail(Vector2[] positions, 
            WidthFunction widthFunc, ColorFunction colorFunc,
            float bloomMultiplier = 2.7f, float coreMultiplier = 0.4f,
            Func<float, Vector2> offset = null, int segmentCount = 50)
        {
            // Pass 1: Outer bloom (behind, large, dim)
            RenderTrail(positions, new PrimitiveSettings(
                BloomWidth(widthFunc, bloomMultiplier),
                BloomColor(colorFunc, 0.3f),
                offset,
                smoothen: true
            ), segmentCount);
            
            // Pass 2: Main trail
            RenderTrail(positions, new PrimitiveSettings(
                widthFunc,
                colorFunc,
                offset,
                smoothen: true
            ), segmentCount);
            
            // Pass 3: Inner bright core
            RenderTrail(positions, new PrimitiveSettings(
                completionRatio => widthFunc(completionRatio) * coreMultiplier,
                completionRatio => Color.White * 0.8f * (1f - completionRatio),
                offset,
                smoothen: true
            ), segmentCount);
        }
        
        /// <summary>
        /// Renders a themed trail using the theme's color palette.
        /// </summary>
        public static void RenderThemedTrail(Vector2[] positions, string themeName,
            float width, float opacity = 1f, int segmentCount = 50)
        {
            var widthFunc = LinearTaper(width);
            var colorFunc = ThemeColor(themeName, opacity);
            
            RenderMultiPassTrail(positions, widthFunc, colorFunc, 
                segmentCount: segmentCount);
        }
        
        /// <summary>
        /// Renders a projectile trail using its oldPos array.
        /// </summary>
        public static void RenderProjectileTrail(Projectile projectile, 
            Color startColor, Color endColor, float width,
            bool multiPass = true, int segmentCount = 50)
        {
            var widthFunc = LinearTaper(width);
            var colorFunc = GradientColor(startColor, endColor, projectile.Opacity);
            Func<float, Vector2> offset = _ => projectile.Size * 0.5f;
            
            if (multiPass)
            {
                RenderMultiPassTrail(projectile.oldPos, widthFunc, colorFunc, 
                    offset: offset, segmentCount: segmentCount);
            }
            else
            {
                RenderTrail(projectile.oldPos, new PrimitiveSettings(
                    widthFunc, colorFunc, offset), segmentCount);
            }
        }
        
        /// <summary>
        /// Renders a laser beam using control points.
        /// </summary>
        public static void RenderLaserBeam(Vector2 start, Vector2 end, Color color, 
            float width, float opacity = 1f, int segmentCount = 30)
        {
            // Generate points along the laser
            Vector2[] points = new Vector2[segmentCount];
            for (int i = 0; i < segmentCount; i++)
            {
                float t = (float)i / (segmentCount - 1);
                points[i] = Vector2.Lerp(start, end, t);
            }
            
            var widthFunc = QuadraticBumpWidth(width);
            var colorFunc = SolidColorFade(color, opacity);
            
            RenderMultiPassTrail(points, widthFunc, colorFunc, 
                segmentCount: segmentCount);
        }
        
        #endregion
        
        #region Helper Methods
        
        /// <summary>
        /// Smooths trail positions using Catmull-Rom splines.
        /// </summary>
        private static List<Vector2> SmoothTrail(List<Vector2> positions, int outputPoints)
        {
            List<Vector2> result = new List<Vector2>();
            
            for (int i = 0; i < outputPoints; i++)
            {
                float t = (float)i / (outputPoints - 1) * (positions.Count - 1);
                int segment = (int)t;
                float segmentT = t - segment;
                
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
        /// Draws primitives to the screen.
        /// </summary>
        private static void DrawPrimitives(int vertexCount, int triangleCount, MiscShaderData shader)
        {
            GraphicsDevice device = Main.instance.GraphicsDevice;
            
            Matrix view = Matrix.CreateLookAt(Vector3.Backward, Vector3.Zero, Vector3.Up);
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);
            
            _basicEffect.View = view;
            _basicEffect.Projection = projection;
            _basicEffect.World = Matrix.Identity;
            
            if (shader != null)
            {
                shader.Apply();
            }
            else
            {
                foreach (EffectPass pass in _basicEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                }
            }
            
            device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, 
                _vertices, 0, vertexCount, _indices, 0, triangleCount);
        }
        
        /// <summary>
        /// Draws primitives with a custom MagnumOpus shader effect.
        /// </summary>
        private static void DrawPrimitivesWithCustomShader(int vertexCount, int triangleCount, Effect customShader, Color primaryColor, Color secondaryColor, float intensity)
        {
            GraphicsDevice device = Main.instance.GraphicsDevice;
            
            Matrix view = Matrix.CreateLookAt(Vector3.Backward, Vector3.Zero, Vector3.Up);
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);
            
            _basicEffect.View = view;
            _basicEffect.Projection = projection;
            _basicEffect.World = Matrix.Identity;
            
            if (customShader != null)
            {
                // Configure custom shader parameters
                customShader.Parameters["uColor"]?.SetValue(primaryColor.ToVector3());
                customShader.Parameters["uSecondaryColor"]?.SetValue(secondaryColor.ToVector3());
                customShader.Parameters["uIntensity"]?.SetValue(intensity);
                customShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
                customShader.Parameters["uOpacity"]?.SetValue(1f);
                
                foreach (EffectPass pass in customShader.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList,
                        _vertices, 0, vertexCount, _indices, 0, triangleCount);
                }
            }
            else
            {
                foreach (EffectPass pass in _basicEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                }
                device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList,
                    _vertices, 0, vertexCount, _indices, 0, triangleCount);
            }
        }
        
        #endregion
        
        #region Custom Shader Trail Rendering
        
        /// <summary>
        /// Renders a trail using MagnumOpus custom shaders.
        /// Provides enhanced bloom and gradient effects.
        /// </summary>
        /// <param name="positions">Array of world positions for the trail</param>
        /// <param name="settings">Primitive settings for width/color</param>
        /// <param name="useCustomShader">Whether to use custom HLSL shaders</param>
        /// <param name="primaryColor">Primary trail color</param>
        /// <param name="secondaryColor">Secondary/gradient color</param>
        /// <param name="intensity">Shader intensity multiplier</param>
        public static void RenderTrailWithShader(
            Vector2[] positions,
            PrimitiveSettings settings,
            Color primaryColor,
            Color secondaryColor,
            float intensity = 1f)
        {
            if (_basicEffect == null) Initialize();
            if (positions == null || positions.Length < 2) return;
            
            // Get custom trail shader
            Effect trailShader = Shaders.ShaderLoader.Trail;
            
            // Build the trail mesh
            List<Vector2> smoothedPositions = settings.Smoothen ? 
                SmoothTrail(new List<Vector2>(positions), positions.Length * 2) : new List<Vector2>(positions);
            
            int vertexCount = 0;
            int indexCount = 0;
            
            for (int i = 0; i < smoothedPositions.Count; i++)
            {
                float completionRatio = (float)i / (smoothedPositions.Count - 1);
                
                float width = settings.WidthFunc?.Invoke(completionRatio) ?? 10f;
                Color color = settings.ColorFunc?.Invoke(completionRatio) ?? Color.White;
                Vector2 offset = settings.OffsetFunc?.Invoke(completionRatio) ?? Vector2.Zero;
                
                Vector2 position = smoothedPositions[i] + offset - Main.screenPosition;
                
                // Calculate perpendicular direction for width
                Vector2 direction = Vector2.Zero;
                if (i < smoothedPositions.Count - 1)
                    direction = smoothedPositions[i + 1] - smoothedPositions[i];
                else if (i > 0)
                    direction = smoothedPositions[i] - smoothedPositions[i - 1];
                
                direction.Normalize();
                Vector2 perpendicular = new Vector2(-direction.Y, direction.X);
                
                // Add vertices
                Vector2 leftPos = position - perpendicular * width * 0.5f;
                Vector2 rightPos = position + perpendicular * width * 0.5f;
                
                _vertices[vertexCount] = new VertexPositionColorTexture(
                    new Vector3(leftPos, 0), color, new Vector2(0, completionRatio));
                _vertices[vertexCount + 1] = new VertexPositionColorTexture(
                    new Vector3(rightPos, 0), color, new Vector2(1, completionRatio));
                
                // Add indices for triangles
                if (i < smoothedPositions.Count - 1)
                {
                    int baseIndex = vertexCount;
                    _indices[indexCount++] = (short)baseIndex;
                    _indices[indexCount++] = (short)(baseIndex + 1);
                    _indices[indexCount++] = (short)(baseIndex + 2);
                    
                    _indices[indexCount++] = (short)(baseIndex + 1);
                    _indices[indexCount++] = (short)(baseIndex + 3);
                    _indices[indexCount++] = (short)(baseIndex + 2);
                }
                
                vertexCount += 2;
            }
            
            int triangleCount = indexCount / 3;
            
            // Draw with custom shader
            DrawPrimitivesWithCustomShader(vertexCount, triangleCount, trailShader, primaryColor, secondaryColor, intensity);
        }
        
        /// <summary>
        /// Renders a multi-pass trail with bloom effect.
        /// Pass 1: Outer bloom (large, dim)
        /// Pass 2: Main trail (full color)
        /// Pass 3: Core glow (small, bright)
        /// </summary>
        public static void RenderMultiPassTrail(
            Vector2[] positions,
            WidthFunction widthFunc,
            Color primaryColor,
            Color secondaryColor,
            float baseWidth = 20f,
            float bloomIntensity = 1f)
        {
            if (positions == null || positions.Length < 2) return;
            
            // Pass 1: Outer bloom (2x width, 30% opacity)
            var bloomSettings = new PrimitiveSettings(
                ratio => widthFunc(ratio) * 2f,
                ratio => Color.Lerp(primaryColor, secondaryColor, ratio) * 0.3f,
                null, true, null
            );
            RenderTrailWithShader(positions, bloomSettings, primaryColor * 0.3f, secondaryColor * 0.3f, bloomIntensity * 0.5f);
            
            // Pass 2: Main trail (full width, full color)
            var mainSettings = new PrimitiveSettings(
                widthFunc,
                ratio => Color.Lerp(primaryColor, secondaryColor, ratio),
                null, true, null
            );
            RenderTrailWithShader(positions, mainSettings, primaryColor, secondaryColor, bloomIntensity);
            
            // Pass 3: Core glow (0.4x width, white core)
            var coreSettings = new PrimitiveSettings(
                ratio => widthFunc(ratio) * 0.4f,
                ratio => Color.White * 0.8f,
                null, true, null
            );
            RenderTrailWithShader(positions, coreSettings, Color.White * 0.8f, primaryColor * 0.6f, bloomIntensity * 1.2f);
        }
        
        #endregion
        
        #region Noise-Scrolling UV Trails (Flowing Energy Ribbons)
        
        /// <summary>
        /// UV scroll speed for noise texture. Creates flowing energy ribbon effect.
        /// </summary>
        private static float _noiseScrollOffset = 0f;
        
        /// <summary>
        /// Renders a trail with noise-scrolling UV coordinates for flowing energy effects.
        /// This creates the signature Calamity-style "flowing ribbon" look.
        /// </summary>
        /// <param name="positions">World positions for the trail</param>
        /// <param name="settings">Base primitive settings</param>
        /// <param name="noiseTexture">Noise texture for UV scrolling (or null for procedural)</param>
        /// <param name="scrollSpeed">How fast the UV scrolls along the trail</param>
        /// <param name="waveAmplitude">How much the trail waves side-to-side</param>
        /// <param name="waveFrequency">Frequency of the wave oscillation</param>
        public static void RenderFlowingTrail(
            Vector2[] positions,
            PrimitiveSettings settings,
            Texture2D noiseTexture = null,
            float scrollSpeed = 3f,
            float waveAmplitude = 0f,
            float waveFrequency = 4f)
        {
            if (Main.dedServ || positions == null || positions.Length < 2)
                return;
            
            if (_basicEffect == null)
                Initialize();
            
            // Update scroll offset (1/60 = 0.0167 seconds per frame at 60 FPS)
            const float frameTime = 1f / 60f;
            _noiseScrollOffset += frameTime * scrollSpeed;
            if (_noiseScrollOffset > 100f) _noiseScrollOffset -= 100f;
            
            // Filter out zero positions
            List<Vector2> validPositions = new List<Vector2>();
            foreach (var pos in positions)
            {
                if (pos != Vector2.Zero)
                    validPositions.Add(pos);
            }
            
            if (validPositions.Count < 2) return;
            
            // Smooth the trail
            List<Vector2> finalPositions;
            if (settings.Smoothen && validPositions.Count >= 4)
                finalPositions = SmoothTrail(validPositions, validPositions.Count * 2);
            else
                finalPositions = validPositions;
            
            if (finalPositions.Count < 2) return;
            
            // Build vertices with scrolling UVs and wave offset
            int vertexCount = finalPositions.Count * 2;
            int indexCount = (finalPositions.Count - 1) * 6;
            
            if (vertexCount > MaxVertices || indexCount > MaxIndices)
                return;
            
            for (int i = 0; i < finalPositions.Count; i++)
            {
                float completionRatio = (float)i / (finalPositions.Count - 1);
                float width = settings.WidthFunc?.Invoke(completionRatio) ?? 10f;
                Color color = settings.ColorFunc?.Invoke(completionRatio) ?? Color.White;
                
                // CRITICAL: Remove alpha for proper additive blending
                color = color.WithoutAlpha();
                
                // Calculate perpendicular direction
                Vector2 direction;
                if (i == 0)
                    direction = (finalPositions[1] - finalPositions[0]).SafeNormalize(Vector2.UnitY);
                else if (i == finalPositions.Count - 1)
                    direction = (finalPositions[i] - finalPositions[i - 1]).SafeNormalize(Vector2.UnitY);
                else
                    direction = (finalPositions[i + 1] - finalPositions[i - 1]).SafeNormalize(Vector2.UnitY);
                
                Vector2 perpendicular = new Vector2(-direction.Y, direction.X);
                Vector2 worldPos = finalPositions[i];
                
                // Apply wave offset for flowing ribbon effect
                if (waveAmplitude > 0)
                {
                    float wave = MathF.Sin((completionRatio * waveFrequency + _noiseScrollOffset) * MathHelper.TwoPi);
                    worldPos += perpendicular * wave * waveAmplitude;
                }
                
                // Apply offset
                if (settings.OffsetFunc != null)
                    worldPos += settings.OffsetFunc(completionRatio);
                
                // Convert to screen position
                Vector2 screenPos = worldPos - Main.screenPosition;
                
                // SCROLLING UV - the key to flowing energy effect
                float scrolledU = completionRatio + _noiseScrollOffset;
                
                // Create vertices with scrolling UV
                _vertices[i * 2] = new VertexPositionColorTexture(
                    new Vector3(screenPos + perpendicular * width * 0.5f, 0),
                    color,
                    new Vector2(scrolledU, 0));
                
                _vertices[i * 2 + 1] = new VertexPositionColorTexture(
                    new Vector3(screenPos - perpendicular * width * 0.5f, 0),
                    color,
                    new Vector2(scrolledU, 1));
            }
            
            // Build indices
            int idx = 0;
            for (int i = 0; i < finalPositions.Count - 1; i++)
            {
                int baseVertex = i * 2;
                _indices[idx++] = (short)baseVertex;
                _indices[idx++] = (short)(baseVertex + 1);
                _indices[idx++] = (short)(baseVertex + 2);
                _indices[idx++] = (short)(baseVertex + 1);
                _indices[idx++] = (short)(baseVertex + 3);
                _indices[idx++] = (short)(baseVertex + 2);
            }
            
            // Render with noise texture if available
            DrawPrimitives(vertexCount, indexCount / 3, settings.Shader);
        }
        
        /// <summary>
        /// Renders a multi-pass flowing energy ribbon trail.
        /// Perfect for cosmic weapons, laser beams, and magical effects.
        /// </summary>
        public static void RenderFlowingEnergyRibbon(
            Vector2[] positions,
            Color primaryColor,
            Color secondaryColor,
            float width,
            float scrollSpeed = 3f,
            float waveAmplitude = 5f)
        {
            // Pass 1: Outer flowing bloom
            var bloomSettings = new PrimitiveSettings(
                BloomWidth(LinearTaper(width), 2.5f),
                completionRatio => 
                    Color.Lerp(primaryColor, secondaryColor, completionRatio) * 0.25f * (1f - completionRatio),
                null, true, null
            );
            RenderFlowingTrail(positions, bloomSettings, null, scrollSpeed, waveAmplitude * 1.5f);
            
            // Pass 2: Main flowing trail
            var mainSettings = new PrimitiveSettings(
                LinearTaper(width),
                completionRatio => 
                    Color.Lerp(primaryColor, secondaryColor, completionRatio) * (1f - completionRatio),
                null, true, null
            );
            RenderFlowingTrail(positions, mainSettings, null, scrollSpeed * 1.2f, waveAmplitude);
            
            // Pass 3: Core glow (faster scroll for energy effect)
            var coreSettings = new PrimitiveSettings(
                completionRatio => LinearTaper(width)(completionRatio) * 0.35f,
                completionRatio => Color.White * 0.85f * (1f - completionRatio * 0.8f),
                null, true, null
            );
            RenderFlowingTrail(positions, coreSettings, null, scrollSpeed * 2f, waveAmplitude * 0.3f);
        }
        
        /// <summary>
        /// Renders a constellation-style trail with connected star points.
        /// Perfect for Fate theme cosmic effects.
        /// </summary>
        public static void RenderConstellationTrail(
            Vector2[] positions,
            Color starColor,
            Color lineColor,
            float lineWidth,
            float starScale = 0.3f)
        {
            // Draw connecting lines first (behind stars)
            var lineSettings = new PrimitiveSettings(
                completionRatio => lineWidth * (1f - completionRatio * 0.5f),
                completionRatio => lineColor * 0.6f * (1f - completionRatio),
                null, true, null
            );
            RenderFlowingTrail(positions, lineSettings, null, 1f, 0f);
            
            // Draw star points at intervals
            Texture2D bloomTex = MagnumTextureRegistry.GetBloom();
            if (bloomTex == null) return;
            
            int starInterval = Math.Max(1, positions.Length / 8);
            for (int i = 0; i < positions.Length; i += starInterval)
            {
                if (positions[i] == Vector2.Zero) continue;
                
                float completion = (float)i / positions.Length;
                float scale = starScale * (1f - completion * 0.5f);
                Color color = Color.Lerp(Color.White, starColor, completion);
                
                Vector2 drawPos = positions[i] - Main.screenPosition;
                Vector2 origin = bloomTex.Size() * 0.5f;
                
                // Bloom behind star
                Main.spriteBatch.Draw(bloomTex, drawPos, null, 
                    starColor.WithoutAlpha() * 0.4f, 0f, origin, scale * 2f, SpriteEffects.None, 0f);
                
                // Star core
                Main.spriteBatch.Draw(bloomTex, drawPos, null, 
                    color.WithoutAlpha() * 0.8f, 0f, origin, scale, SpriteEffects.None, 0f);
            }
        }
        
        #endregion
    }
}
