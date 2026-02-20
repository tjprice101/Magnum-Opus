using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Shaders;
using MagnumOpus.Common.Systems.VFX.Core;

namespace MagnumOpus.Common.Systems.VFX.Trails
{
    /// <summary>
    /// Calamity-tier trail renderer with shader-based effects.
    /// Provides 5 trail styles: Flame, Ice, Lightning, Nature, Cosmic.
    /// 
    /// Usage:
    ///   CalamityStyleTrailRenderer.DrawTrail(
    ///       projectile.oldPos, 
    ///       projectile.oldRot, 
    ///       TrailStyle.Flame, 
    ///       baseWidth: 20f,
    ///       primaryColor: Color.Orange,
    ///       secondaryColor: Color.Yellow
    ///   );
    /// 
    /// Features:
    /// - Shader-based rendering when available
    /// - Automatic particle fallback when shaders disabled
    /// - Multi-pass bloom integration
    /// - FargosSoulsDLC-style width/color functions
    /// - 144Hz+ interpolation support
    /// - Catmull-Rom trail smoothing
    /// </summary>
    public static class CalamityStyleTrailRenderer
    {
        #region Constants

        private const int MaxVertices = 512;
        private const int MaxIndices = 768;
        private const int DefaultSegmentCount = 50;
        private const float DefaultWidth = 20f;

        #endregion

        #region Enums

        /// <summary>
        /// Trail rendering styles, each with unique visual characteristics.
        /// </summary>
        public enum TrailStyle
        {
            /// <summary>Hot, fiery trail with ember particles and heat distortion.</summary>
            Flame,
            /// <summary>Cold, crystalline trail with frost particles and shimmer.</summary>
            Ice,
            /// <summary>Electric, jagged trail with spark particles and branching.</summary>
            Lightning,
            /// <summary>Organic, flowing trail with leaf/petal particles.</summary>
            Nature,
            /// <summary>Celestial, starry trail with nebula effects and star particles.</summary>
            Cosmic
        }

        #endregion

        #region Static Fields

        private static BasicEffect _basicEffect;
        private static VertexPositionColorTexture[] _vertexBuffer;
        private static short[] _indexBuffer;
        private static bool _initialized;

        #endregion

        #region Initialization

        /// <summary>
        /// Initializes the trail renderer. Called automatically on first use.
        /// </summary>
        public static void Initialize()
        {
            if (_initialized || Main.dedServ)
                return;

            try
            {
                _basicEffect = new BasicEffect(Main.graphics.GraphicsDevice)
                {
                    VertexColorEnabled = true,
                    TextureEnabled = true
                };

                _vertexBuffer = new VertexPositionColorTexture[MaxVertices];
                _indexBuffer = new short[MaxIndices];

                // Pre-generate index buffer for triangle strip pattern
                for (int i = 0; i < MaxIndices / 6; i++)
                {
                    int startIndex = i * 6;
                    int startVertex = i * 2;

                    if (startIndex + 5 < MaxIndices)
                    {
                        _indexBuffer[startIndex] = (short)startVertex;
                        _indexBuffer[startIndex + 1] = (short)(startVertex + 1);
                        _indexBuffer[startIndex + 2] = (short)(startVertex + 2);
                        _indexBuffer[startIndex + 3] = (short)(startVertex + 2);
                        _indexBuffer[startIndex + 4] = (short)(startVertex + 1);
                        _indexBuffer[startIndex + 5] = (short)(startVertex + 3);
                    }
                }

                _initialized = true;
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<MagnumOpus>()?.Logger.Warn(
                    $"CalamityStyleTrailRenderer: Initialization failed - {ex.Message}");
            }
        }

        #endregion

        #region Main Drawing Methods

        /// <summary>
        /// Draws a trail with the specified style.
        /// Automatically uses shaders when available, falls back to particles otherwise.
        /// </summary>
        /// <param name="positions">World positions of the trail (from projectile.oldPos)</param>
        /// <param name="rotations">Rotations at each position (from projectile.oldRot)</param>
        /// <param name="style">Visual style of the trail</param>
        /// <param name="baseWidth">Base width of the trail in pixels</param>
        /// <param name="primaryColor">Primary color of the trail</param>
        /// <param name="secondaryColor">Secondary color for gradient effects</param>
        /// <param name="intensity">Overall intensity multiplier (0-1)</param>
        /// <param name="segmentCount">Number of segments for smoothing</param>
        public static void DrawTrail(
            Vector2[] positions,
            float[] rotations,
            TrailStyle style,
            float baseWidth = DefaultWidth,
            Color? primaryColor = null,
            Color? secondaryColor = null,
            float intensity = 1f,
            int segmentCount = DefaultSegmentCount)
        {
            if (Main.dedServ || positions == null || positions.Length < 2)
                return;

            if (!_initialized)
                Initialize();

            // Default colors based on style
            Color primary = primaryColor ?? GetDefaultPrimaryColor(style);
            Color secondary = secondaryColor ?? GetDefaultSecondaryColor(style);

            // Check if shaders are available
            if (ShaderLoader.ShadersEnabled)
            {
                DrawShaderTrail(positions, rotations, style, baseWidth, primary, secondary, intensity, segmentCount);
            }
            else
            {
                DrawParticleFallbackTrail(positions, style, baseWidth, primary, secondary, intensity);
            }
        }

        /// <summary>
        /// Draws a simple trail with just positions (no rotations).
        /// </summary>
        public static void DrawTrail(
            Vector2[] positions,
            TrailStyle style,
            float baseWidth = DefaultWidth,
            Color? primaryColor = null,
            Color? secondaryColor = null,
            float intensity = 1f)
        {
            DrawTrail(positions, null, style, baseWidth, primaryColor, secondaryColor, intensity);
        }

        /// <summary>
        /// Draws a trail for a projectile using its oldPos and oldRot arrays.
        /// </summary>
        public static void DrawProjectileTrail(
            Projectile projectile,
            TrailStyle style,
            float baseWidth = DefaultWidth,
            Color? primaryColor = null,
            Color? secondaryColor = null,
            float intensity = 1f)
        {
            DrawTrail(projectile.oldPos, projectile.oldRot, style, baseWidth, primaryColor, secondaryColor, intensity);
        }

        #endregion

        #region Shader-Based Rendering

        // Pre-allocated scratch arrays to avoid GC allocations per frame
        private static Vector2[] _filteredPositions = new Vector2[MaxVertices];
        private static Vector2[] _smoothedPositions = new Vector2[MaxVertices];

        private static void DrawShaderTrail(
            Vector2[] positions,
            float[] rotations,
            TrailStyle style,
            float baseWidth,
            Color primaryColor,
            Color secondaryColor,
            float intensity,
            int segmentCount)
        {
            // Filter out zero positions into pre-allocated array (zero-GC)
            int validCount = 0;
            foreach (var pos in positions)
            {
                if (pos != Vector2.Zero && validCount < _filteredPositions.Length)
                    _filteredPositions[validCount++] = pos;
            }

            if (validCount < 2)
                return;

            // Smooth the trail using Catmull-Rom interpolation (zero-GC)
            int smoothedCount = SmoothTrailInPlace(_filteredPositions, validCount, _smoothedPositions, segmentCount);
            if (smoothedCount < 2)
                return;

            // Get style-specific width and color functions
            Func<float, float> widthFunc = GetWidthFunction(style, baseWidth);
            Func<float, Color> colorFunc = GetColorFunction(style, primaryColor, secondaryColor, intensity);

            // Build vertex buffer from smoothed positions
            int vertexCount = BuildVertexBufferFromArray(_smoothedPositions, smoothedCount, widthFunc, colorFunc);
            if (vertexCount < 4)
                return;

            // Draw with shader
            DrawWithShader(style, vertexCount, primaryColor, secondaryColor, intensity);
        }

        /// <summary>
        /// Draws the trail using the SimpleTrailShader with full Gap #1-#7 support:
        /// - Binds noise texture to sampler slot 1 (uImage1) for texture-driven effects
        /// - Sets overbright multiplier and conditional glow uniforms
        /// - Cleans up device.Textures[1] after draw to avoid leaking into subsequent draws
        /// </summary>
        /// <param name="overbrightMult">Overbright color multiplier (1.0 = normal, 2-7 = Calamity-tier glow). Default 3.0.</param>
        /// <param name="glowThreshold">Luminance threshold for conditional glow (0-1). Default 0.5.</param>
        /// <param name="glowIntensity">Intensity of conditional glow effect. Default 1.5.</param>
        /// <param name="noiseTextureOverride">Optional noise texture override. Null = use default for style.</param>
        private static void DrawWithShader(
            TrailStyle style,
            int vertexCount,
            Color primaryColor,
            Color secondaryColor,
            float intensity,
            float overbrightMult = 3f,
            float glowThreshold = 0.5f,
            float glowIntensity = 1.5f,
            Texture2D noiseTextureOverride = null)
        {
            var device = Main.graphics.GraphicsDevice;
            Effect shader = ShaderLoader.Trail;

            // Safely end SpriteBatch before primitive drawing.
            // SpriteBatch.End() is idempotent in tModLoader — it won't throw if not begun,
            // but we guard anyway since other mods may alter state.
            try { Main.spriteBatch.End(); } catch { }

            try
            {
                // Set render states
                device.BlendState = BlendState.Additive;
                device.DepthStencilState = DepthStencilState.None;
                device.RasterizerState = RasterizerState.CullNone;
                device.SamplerStates[0] = SamplerState.LinearWrap;

                if (shader != null)
                {
                    // Select the correct technique for this trail style
                    string techniqueName = style switch
                    {
                        TrailStyle.Flame => "FlameTechnique",
                        TrailStyle.Ice => "IceTechnique",
                        TrailStyle.Lightning => "LightningTechnique",
                        TrailStyle.Nature => "NatureTechnique",
                        TrailStyle.Cosmic => "CosmicTechnique",
                        _ => "FlameTechnique"
                    };
                    
                    if (shader.Techniques[techniqueName] != null)
                        shader.CurrentTechnique = shader.Techniques[techniqueName];

                    // --- Core params (existing) ---
                    shader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
                    shader.Parameters["uColor"]?.SetValue(primaryColor.ToVector3());
                    shader.Parameters["uSecondaryColor"]?.SetValue(secondaryColor.ToVector3());
                    shader.Parameters["uIntensity"]?.SetValue(intensity);
                    shader.Parameters["uOpacity"]?.SetValue(1f);
                    shader.Parameters["uProgress"]?.SetValue(0f);

                    // --- Gap #2: Overbright multiplier ---
                    shader.Parameters["uOverbrightMult"]?.SetValue(Math.Max(1f, overbrightMult));

                    // --- Gap #3: Conditional glow ---
                    shader.Parameters["uGlowThreshold"]?.SetValue(MathHelper.Clamp(glowThreshold, 0f, 1f));
                    shader.Parameters["uGlowIntensity"]?.SetValue(Math.Max(0f, glowIntensity));

                    // --- Gap #1: Bind noise texture to sampler slot 1 (uImage1) ---
                    Texture2D noiseTex = noiseTextureOverride ?? ShaderLoader.GetDefaultTrailStyleTexture((int)style);
                    if (noiseTex != null)
                    {
                        device.Textures[1] = noiseTex;
                        device.SamplerStates[1] = SamplerState.LinearWrap;
                        shader.Parameters["uHasSecondaryTex"]?.SetValue(1f);
                        shader.Parameters["uSecondaryTexScale"]?.SetValue(1f);
                        shader.Parameters["uSecondaryTexScroll"]?.SetValue(0.5f);
                    }
                    else
                    {
                        shader.Parameters["uHasSecondaryTex"]?.SetValue(0f);
                    }

                    shader.CurrentTechnique.Passes[0].Apply();
                }
                else
                {
                    // Fallback to BasicEffect
                    _basicEffect.World = Matrix.Identity;
                    _basicEffect.View = Matrix.Identity;
                    _basicEffect.Projection = Matrix.CreateOrthographicOffCenter(
                        0, Main.screenWidth, Main.screenHeight, 0, 0, 1);
                    _basicEffect.TextureEnabled = false;

                    _basicEffect.CurrentTechnique.Passes[0].Apply();
                }

                // Draw primitives
                int primitiveCount = (vertexCount / 2) - 1;
                device.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    _vertexBuffer, 0, vertexCount,
                    _indexBuffer, 0, primitiveCount * 2);
            }
            finally
            {
                // --- Gap #7: Clean up secondary texture to prevent leaking ---
                device.Textures[1] = null;

                // Restore SpriteBatch
                Main.spriteBatch.Begin(
                    SpriteSortMode.Deferred,
                    BlendState.AlphaBlend,
                    SamplerState.LinearClamp,
                    DepthStencilState.None,
                    RasterizerState.CullNone,
                    null,
                    Main.GameViewMatrix.TransformationMatrix);
            }
        }

        #endregion

        #region Particle Fallback

        private static void DrawParticleFallbackTrail(
            Vector2[] positions,
            TrailStyle style,
            float baseWidth,
            Color primaryColor,
            Color secondaryColor,
            float intensity)
        {
            // When shaders aren't available, use dense particle effects
            // This ensures effects still look good on lower-end systems

            int particleCount = Math.Max(2, positions.Length / 3);
            float stepSize = 1f / particleCount;

            for (int i = 0; i < particleCount; i++)
            {
                float progress = i * stepSize;
                int index = Math.Min((int)(progress * positions.Length), positions.Length - 1);

                if (positions[index] == Vector2.Zero)
                    continue;

                Vector2 pos = positions[index];
                Color color = Color.Lerp(primaryColor, secondaryColor, progress);
                float scale = baseWidth * 0.05f * (1f - progress) * intensity;

                SpawnStyleParticle(pos, style, color, scale);
            }
        }

        private static void SpawnStyleParticle(Vector2 position, TrailStyle style, Color color, float scale)
        {
            // Spawn particles based on style
            int dustType = style switch
            {
                TrailStyle.Flame => DustID.Torch,
                TrailStyle.Ice => DustID.Frost,
                TrailStyle.Lightning => DustID.Electric,
                TrailStyle.Nature => DustID.JungleGrass,
                TrailStyle.Cosmic => DustID.Enchanted_Gold,
                _ => DustID.MagicMirror
            };

            Dust dust = Dust.NewDustPerfect(
                position,
                dustType,
                Vector2.Zero,
                0,
                color,
                scale * 1.5f);

            dust.noGravity = true;
            dust.noLight = false;
        }

        #endregion

        #region Vertex Buffer Building

        /// <summary>
        /// Builds vertex buffer from a pre-allocated array (zero-allocation hot path).
        /// </summary>
        private static int BuildVertexBufferFromArray(
            Vector2[] positions,
            int positionCount,
            Func<float, float> widthFunc,
            Func<float, Color> colorFunc)
        {
            int vertexIndex = 0;

            for (int i = 0; i < positionCount && vertexIndex < MaxVertices - 2; i++)
            {
                float progress = (float)i / (positionCount - 1);
                float width = widthFunc(progress);
                Color color = colorFunc(progress);

                // Calculate perpendicular direction
                Vector2 direction;
                if (i == 0 && positionCount > 1)
                    direction = (positions[1] - positions[0]).SafeNormalize(Vector2.UnitY);
                else if (i == positionCount - 1 && positionCount > 1)
                    direction = (positions[i] - positions[i - 1]).SafeNormalize(Vector2.UnitY);
                else if (positionCount > 2)
                    direction = (positions[i + 1] - positions[i - 1]).SafeNormalize(Vector2.UnitY);
                else
                    direction = Vector2.UnitY;

                Vector2 perpendicular = new Vector2(-direction.Y, direction.X);
                Vector2 screenPos = positions[i] - Main.screenPosition;

                // Top vertex
                Vector2 topPos = screenPos + perpendicular * width * 0.5f;
                _vertexBuffer[vertexIndex++] = new VertexPositionColorTexture(
                    new Vector3(topPos, 0),
                    color,
                    new Vector2(progress, 0));

                // Bottom vertex
                Vector2 bottomPos = screenPos - perpendicular * width * 0.5f;
                _vertexBuffer[vertexIndex++] = new VertexPositionColorTexture(
                    new Vector3(bottomPos, 0),
                    color,
                    new Vector2(progress, 1));
            }

            return vertexIndex;
        }

        /// <summary>
        /// Builds vertex buffer from a List (backward-compatible, used by non-hot-path callers).
        /// </summary>
        private static int BuildVertexBuffer(
            List<Vector2> positions,
            Func<float, float> widthFunc,
            Func<float, Color> colorFunc)
        {
            int vertexIndex = 0;

            for (int i = 0; i < positions.Count && vertexIndex < MaxVertices - 2; i++)
            {
                float progress = (float)i / (positions.Count - 1);
                float width = widthFunc(progress);
                Color color = colorFunc(progress);

                // Calculate perpendicular direction
                Vector2 direction;
                if (i == 0)
                    direction = (positions[1] - positions[0]).SafeNormalize(Vector2.UnitY);
                else if (i == positions.Count - 1)
                    direction = (positions[i] - positions[i - 1]).SafeNormalize(Vector2.UnitY);
                else
                    direction = (positions[i + 1] - positions[i - 1]).SafeNormalize(Vector2.UnitY);

                Vector2 perpendicular = new Vector2(-direction.Y, direction.X);
                Vector2 screenPos = positions[i] - Main.screenPosition;

                // Top vertex
                Vector2 topPos = screenPos + perpendicular * width * 0.5f;
                _vertexBuffer[vertexIndex++] = new VertexPositionColorTexture(
                    new Vector3(topPos, 0),
                    color,
                    new Vector2(progress, 0));

                // Bottom vertex
                Vector2 bottomPos = screenPos - perpendicular * width * 0.5f;
                _vertexBuffer[vertexIndex++] = new VertexPositionColorTexture(
                    new Vector3(bottomPos, 0),
                    color,
                    new Vector2(progress, 1));
            }

            return vertexIndex;
        }

        #endregion

        #region Trail Smoothing

        /// <summary>
        /// Smooths a trail using Catmull-Rom spline interpolation (zero-allocation version).
        /// Writes results into a pre-allocated output array.
        /// </summary>
        private static int SmoothTrailInPlace(Vector2[] inputPositions, int inputCount, Vector2[] output, int outputCount)
        {
            if (inputCount < 4)
            {
                int copyCount = Math.Min(inputCount, output.Length);
                Array.Copy(inputPositions, output, copyCount);
                return copyCount;
            }

            int actualOutput = Math.Min(outputCount, output.Length);

            for (int i = 0; i < actualOutput; i++)
            {
                float t = (float)i / (actualOutput - 1) * (inputCount - 1);
                int segment = (int)t;
                float localT = t - segment;

                // Clamp indices for Catmull-Rom (needs 4 points)
                int p0 = Math.Max(0, segment - 1);
                int p1 = segment;
                int p2 = Math.Min(inputCount - 1, segment + 1);
                int p3 = Math.Min(inputCount - 1, segment + 2);

                output[i] = CatmullRom(
                    inputPositions[p0],
                    inputPositions[p1],
                    inputPositions[p2],
                    inputPositions[p3],
                    localT);
            }

            return actualOutput;
        }

        /// <summary>
        /// Smooths a trail using Catmull-Rom spline interpolation (List version, allocates).
        /// Kept for backward compatibility with non-hot-path callers.
        /// </summary>
        private static List<Vector2> SmoothTrail(List<Vector2> positions, int outputCount)
        {
            if (positions.Count < 4)
                return new List<Vector2>(positions);

            List<Vector2> result = new List<Vector2>(outputCount);

            for (int i = 0; i < outputCount; i++)
            {
                float t = (float)i / (outputCount - 1) * (positions.Count - 1);
                int segment = (int)t;
                float localT = t - segment;

                // Clamp indices for Catmull-Rom (needs 4 points)
                int p0 = Math.Max(0, segment - 1);
                int p1 = segment;
                int p2 = Math.Min(positions.Count - 1, segment + 1);
                int p3 = Math.Min(positions.Count - 1, segment + 2);

                result.Add(CatmullRom(
                    positions[p0],
                    positions[p1],
                    positions[p2],
                    positions[p3],
                    localT));
            }

            return result;
        }

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

        #endregion

        #region Width Functions

        private static Func<float, float> GetWidthFunction(TrailStyle style, float baseWidth)
        {
            return style switch
            {
                TrailStyle.Flame => progress =>
                {
                    // Flame: Wide at start, tapers with flicker
                    float flicker = 1f + MathF.Sin(Main.GlobalTimeWrappedHourly * 20f + progress * 10f) * 0.15f;
                    return baseWidth * (1f - progress * 0.8f) * flicker;
                },

                TrailStyle.Ice => progress =>
                {
                    // Ice: Crystalline faceted width with subtle variation
                    float crystal = 1f + MathF.Sin(progress * MathHelper.Pi * 4f) * 0.1f;
                    return baseWidth * MathF.Pow(1f - progress, 0.7f) * crystal;
                },

                TrailStyle.Lightning => progress =>
                {
                    // Lightning: Jagged, spiky width
                    float spike = Main.rand.NextFloat(0.7f, 1.3f);
                    float taper = 1f - MathF.Pow(progress, 1.5f);
                    return baseWidth * taper * spike;
                },

                TrailStyle.Nature => progress =>
                {
                    // Nature: Organic flowing width
                    float wave = MathF.Sin(progress * MathHelper.Pi * 2f + Main.GlobalTimeWrappedHourly * 3f) * 0.2f;
                    return baseWidth * (1f - progress * 0.6f) * (1f + wave);
                },

                TrailStyle.Cosmic => progress =>
                {
                    // Cosmic: Nebula-like expanding and contracting
                    float nebula = MathF.Sin(progress * MathHelper.Pi) * 1.3f;
                    float shimmer = 1f + MathF.Sin(Main.GlobalTimeWrappedHourly * 5f + progress * 8f) * 0.1f;
                    return baseWidth * nebula * shimmer;
                },

                _ => progress => baseWidth * (1f - progress) // Linear fallback
            };
        }

        #endregion

        #region Color Functions

        private static Func<float, Color> GetColorFunction(
            TrailStyle style,
            Color primaryColor,
            Color secondaryColor,
            float intensity)
        {
            return style switch
            {
                TrailStyle.Flame => progress =>
                {
                    // Flame: Hot core to cool edge gradient
                    Color core = Color.White;
                    Color mid = primaryColor;
                    Color outer = secondaryColor;

                    float heat = 1f - progress;
                    Color gradient = progress < 0.3f
                        ? Color.Lerp(core, mid, progress / 0.3f)
                        : Color.Lerp(mid, outer, (progress - 0.3f) / 0.7f);

                    return gradient * intensity * heat;
                },

                TrailStyle.Ice => progress =>
                {
                    // Ice: Blue-white crystalline shimmer
                    float shimmer = 0.8f + MathF.Sin(Main.GlobalTimeWrappedHourly * 10f + progress * 15f) * 0.2f;
                    Color gradient = Color.Lerp(primaryColor, secondaryColor, progress);
                    return gradient * intensity * (1f - progress * 0.5f) * shimmer;
                },

                TrailStyle.Lightning => progress =>
                {
                    // Lightning: Bright white-blue with flashes
                    float flash = Main.rand.NextFloat(0.8f, 1.2f);
                    Color gradient = Color.Lerp(Color.White, primaryColor, progress * 0.7f);
                    return gradient * intensity * (1f - progress * 0.4f) * flash;
                },

                TrailStyle.Nature => progress =>
                {
                    // Nature: Organic gradient with soft fade
                    float pulse = 0.9f + MathF.Sin(Main.GlobalTimeWrappedHourly * 4f + progress * 6f) * 0.1f;
                    Color gradient = Color.Lerp(primaryColor, secondaryColor, MathF.Pow(progress, 0.8f));
                    return gradient * intensity * (1f - progress * 0.7f) * pulse;
                },

                TrailStyle.Cosmic => progress =>
                {
                    // Cosmic: Nebula color cycling with star sparkles
                    float hueShift = Main.GlobalTimeWrappedHourly * 0.3f + progress;
                    float hueCycle = (hueShift % 1f + 1f) % 1f;

                    Color nebula = Main.hslToRgb(hueCycle * 0.3f + 0.6f, 0.8f, 0.6f);
                    Color blended = Color.Lerp(primaryColor, nebula, progress * 0.5f);

                    float sparkle = 0.85f + MathF.Sin(Main.GlobalTimeWrappedHourly * 15f + progress * 20f) * 0.15f;
                    return blended * intensity * (1f - progress * 0.3f) * sparkle;
                },

                _ => progress => Color.Lerp(primaryColor, secondaryColor, progress) * intensity * (1f - progress)
            };
        }

        #endregion

        #region Default Colors

        private static Color GetDefaultPrimaryColor(TrailStyle style)
        {
            return style switch
            {
                TrailStyle.Flame => new Color(255, 140, 50),      // Orange fire
                TrailStyle.Ice => new Color(100, 200, 255),       // Cyan ice
                TrailStyle.Lightning => new Color(200, 180, 255), // Purple-white
                TrailStyle.Nature => new Color(100, 200, 80),     // Green
                TrailStyle.Cosmic => new Color(180, 100, 255),    // Purple nebula
                _ => Color.White
            };
        }

        private static Color GetDefaultSecondaryColor(TrailStyle style)
        {
            return style switch
            {
                TrailStyle.Flame => new Color(255, 60, 20),       // Deep red
                TrailStyle.Ice => new Color(200, 230, 255),       // White-blue
                TrailStyle.Lightning => new Color(100, 150, 255), // Blue
                TrailStyle.Nature => new Color(180, 220, 100),    // Yellow-green
                TrailStyle.Cosmic => new Color(100, 50, 200),     // Deep purple
                _ => Color.Gray
            };
        }

        #endregion

        #region Multi-Pass Rendering

        /// <summary>
        /// Draws a trail with multi-pass rendering for enhanced bloom effect.
        /// Includes: Core pass, Inner glow pass, Outer glow pass.
        /// </summary>
        public static void DrawTrailWithBloom(
            Vector2[] positions,
            float[] rotations,
            TrailStyle style,
            float baseWidth = DefaultWidth,
            Color? primaryColor = null,
            Color? secondaryColor = null,
            float intensity = 1f,
            float bloomMultiplier = 2.5f)
        {
            if (Main.dedServ || positions == null || positions.Length < 2)
                return;

            Color primary = primaryColor ?? GetDefaultPrimaryColor(style);
            Color secondary = secondaryColor ?? GetDefaultSecondaryColor(style);

            // Pass 1: Outer bloom (widest, dimmest)
            DrawTrail(positions, rotations, style, baseWidth * bloomMultiplier, 
                primary * 0.25f, secondary * 0.25f, intensity * 0.4f);

            // Pass 2: Middle glow
            DrawTrail(positions, rotations, style, baseWidth * (bloomMultiplier * 0.6f), 
                primary * 0.5f, secondary * 0.5f, intensity * 0.6f);

            // Pass 3: Inner glow
            DrawTrail(positions, rotations, style, baseWidth * (bloomMultiplier * 0.3f), 
                primary * 0.7f, secondary * 0.7f, intensity * 0.8f);

            // Pass 4: Core (brightest, thinnest)
            DrawTrail(positions, rotations, style, baseWidth, 
                Color.Lerp(primary, Color.White, 0.3f), primary, intensity);
        }

        /// <summary>
        /// Draws a projectile trail with multi-pass bloom.
        /// </summary>
        public static void DrawProjectileTrailWithBloom(
            Projectile projectile,
            TrailStyle style,
            float baseWidth = DefaultWidth,
            Color? primaryColor = null,
            Color? secondaryColor = null,
            float intensity = 1f,
            float bloomMultiplier = 2.5f)
        {
            DrawTrailWithBloom(
                projectile.oldPos,
                projectile.oldRot,
                style,
                baseWidth,
                primaryColor,
                secondaryColor,
                intensity,
                bloomMultiplier);
        }

        /// <summary>
        /// Gap #4: Two-layer body+core trail rendering.
        /// Draws a body pass at full width with standard overbright, then a narrow
        /// core pass at 0.4x width with higher overbright lerped toward white.
        /// This creates the characteristic Calamity-style "hot core inside soft body" look.
        /// </summary>
        public static void DrawDualLayerTrail(
            Vector2[] positions,
            float[] rotations,
            TrailStyle style,
            float baseWidth = DefaultWidth,
            Color? primaryColor = null,
            Color? secondaryColor = null,
            float intensity = 1f,
            float bodyOverbright = 3f,
            float coreOverbright = 5f,
            float coreWidthRatio = 0.4f,
            Texture2D noiseTextureOverride = null)
        {
            if (!_initialized)
                Initialize();

            if (positions == null || positions.Length < 2)
                return;

            Color primary = primaryColor ?? GetDefaultPrimaryColor(style);
            Color secondary = secondaryColor ?? GetDefaultSecondaryColor(style);

            // --- Body pass: full width, standard overbright ---
            DrawTrailInternal(positions, rotations, style, baseWidth,
                primary, secondary, intensity,
                bodyOverbright, 0.5f, 1.5f, noiseTextureOverride);

            // --- Core pass: narrow, high overbright, lerped toward white ---
            Color coreColor = Color.Lerp(primary, Color.White, 0.4f);
            Color coreSecondary = Color.Lerp(secondary, Color.White, 0.3f);

            DrawTrailInternal(positions, rotations, style, baseWidth * coreWidthRatio,
                coreColor, coreSecondary, intensity * 1.2f,
                coreOverbright, 0.4f, 2f, noiseTextureOverride);
        }

        /// <summary>
        /// Internal trail draw method that builds vertices and calls DrawWithShader with full Gap support.
        /// Used by DrawDualLayerTrail to avoid duplicating vertex generation.
        /// </summary>
        private static void DrawTrailInternal(
            Vector2[] positions,
            float[] rotations,
            TrailStyle style,
            float baseWidth,
            Color primaryColor,
            Color secondaryColor,
            float intensity,
            float overbrightMult,
            float glowThreshold,
            float glowIntensity,
            Texture2D noiseTextureOverride)
        {
            if (Main.dedServ || positions == null || positions.Length < 2)
                return;

            // Filter out zero positions into pre-allocated array (zero-GC)
            int validCount = 0;
            foreach (var pos in positions)
            {
                if (pos != Vector2.Zero && validCount < _filteredPositions.Length)
                    _filteredPositions[validCount++] = pos;
            }
            if (validCount < 2) return;

            // Smooth trail using Catmull-Rom interpolation
            int smoothedCount = SmoothTrailInPlace(_filteredPositions, validCount, _smoothedPositions, DefaultSegmentCount);
            if (smoothedCount < 2) return;

            // Get style-specific width and color functions
            Func<float, float> widthFunc = GetWidthFunction(style, baseWidth);
            Func<float, Color> colorFunc = GetColorFunction(style, primaryColor, secondaryColor, intensity);

            // Build vertex buffer
            int vertexCount = BuildVertexBufferFromArray(_smoothedPositions, smoothedCount, widthFunc, colorFunc);
            if (vertexCount < 4) return;

            // Draw with full gap support
            DrawWithShader(style, vertexCount, primaryColor, secondaryColor, intensity,
                overbrightMult, glowThreshold, glowIntensity, noiseTextureOverride);
        }

        #endregion

        #region Scrolling Shader Trail

        /// <summary>
        /// Scrolling trail shader styles. Each maps to a technique in ScrollingTrailShader.fx.
        /// </summary>
        public enum ScrollStyle
        {
            Flame,
            Cosmic,
            Energy,
            Void,
            Holy
        }

        /// <summary>
        /// Draws a trail using the UV-scrolling shader with animated effects.
        /// Zero-GC hot path: uses pre-allocated arrays for position filtering and smoothing.
        /// Falls back to standard trail rendering if the scrolling shader is unavailable.
        /// </summary>
        public static void DrawScrollingTrail(
            Vector2[] positions,
            float[] rotations,
            ScrollStyle scrollStyle = ScrollStyle.Flame,
            float baseWidth = DefaultWidth,
            Color? primaryColor = null,
            Color? secondaryColor = null,
            float intensity = 1f,
            float scrollSpeed = 1f,
            float noiseScale = 4f)
        {
            if (!_initialized)
                Initialize();

            if (positions == null || positions.Length < 2)
                return;

            // Map ScrollStyle to a matching TrailStyle for width/color functions
            TrailStyle trailStyle = scrollStyle switch
            {
                ScrollStyle.Flame => TrailStyle.Flame,
                ScrollStyle.Cosmic => TrailStyle.Cosmic,
                ScrollStyle.Energy => TrailStyle.Lightning,
                ScrollStyle.Void => TrailStyle.Cosmic,
                ScrollStyle.Holy => TrailStyle.Nature,
                _ => TrailStyle.Flame
            };

            Color primary = primaryColor ?? GetDefaultPrimaryColor(trailStyle);
            Color secondary = secondaryColor ?? GetDefaultSecondaryColor(trailStyle);

            var widthFunc = GetWidthFunction(trailStyle, baseWidth);
            var colorFunc = GetColorFunction(trailStyle, primary, secondary, intensity);

            // Filter zero-positions into pre-allocated array (zero-GC)
            int validCount = 0;
            for (int i = 0; i < positions.Length && validCount < MaxVertices; i++)
            {
                if (positions[i] != Vector2.Zero)
                {
                    _filteredPositions[validCount] = positions[i];
                    validCount++;
                }
            }

            if (validCount < 2)
                return;

            // Smooth the trail via Catmull-Rom (zero-GC)
            int segmentCount = Math.Min(validCount * 3, MaxVertices);
            int smoothedCount = SmoothTrailInPlace(_filteredPositions, validCount, _smoothedPositions, segmentCount);

            // Build vertex buffer (zero-GC)
            int vertexCount = BuildVertexBufferFromArray(_smoothedPositions, smoothedCount, widthFunc, colorFunc);

            if (vertexCount < 4)
                return;

            // Draw with scrolling shader
            DrawWithScrollingShader(scrollStyle, vertexCount, primary, secondary, intensity, scrollSpeed, noiseScale);
        }

        /// <summary>
        /// Convenience: draw scrolling trail from a Projectile's oldPos/oldRot arrays.
        /// </summary>
        public static void DrawProjectileScrollingTrail(
            Projectile projectile,
            ScrollStyle scrollStyle = ScrollStyle.Flame,
            float baseWidth = DefaultWidth,
            Color? primaryColor = null,
            Color? secondaryColor = null,
            float intensity = 1f,
            float scrollSpeed = 1f,
            float noiseScale = 4f)
        {
            DrawScrollingTrail(
                projectile.oldPos,
                projectile.oldRot,
                scrollStyle,
                baseWidth,
                primaryColor,
                secondaryColor,
                intensity,
                scrollSpeed,
                noiseScale);
        }

        /// <summary>
        /// Draws the trail using the ScrollingTrailShader with full Gap #1-#7 support.
        /// Same enhancements as DrawWithShader: noise texture binding, overbright, glow, cleanup.
        /// </summary>
        private static void DrawWithScrollingShader(
            ScrollStyle scrollStyle,
            int vertexCount,
            Color primaryColor,
            Color secondaryColor,
            float intensity,
            float scrollSpeed,
            float noiseScale,
            float overbrightMult = 3f,
            float glowThreshold = 0.5f,
            float glowIntensity = 1.5f,
            Texture2D noiseTextureOverride = null)
        {
            var device = Main.graphics.GraphicsDevice;
            Effect shader = ShaderLoader.ScrollingTrail;

            // If scrolling shader didn't load, delegate entirely to DrawWithShader
            // (which manages its own SpriteBatch End/Begin cycle)
            if (shader == null)
            {
                TrailStyle fallbackStyle = scrollStyle switch
                {
                    ScrollStyle.Flame => TrailStyle.Flame,
                    ScrollStyle.Cosmic => TrailStyle.Cosmic,
                    ScrollStyle.Energy => TrailStyle.Lightning,
                    ScrollStyle.Void => TrailStyle.Cosmic,
                    ScrollStyle.Holy => TrailStyle.Nature,
                    _ => TrailStyle.Flame
                };
                DrawWithShader(fallbackStyle, vertexCount, primaryColor, secondaryColor, intensity,
                    overbrightMult, glowThreshold, glowIntensity);
                return;
            }

            // Safely end SpriteBatch before primitive drawing
            try { Main.spriteBatch.End(); } catch { }

            try
            {
                // Set render states
                device.BlendState = BlendState.Additive;
                device.DepthStencilState = DepthStencilState.None;
                device.RasterizerState = RasterizerState.CullNone;
                device.SamplerStates[0] = SamplerState.LinearWrap;

                // Select technique for this scroll style
                string techniqueName = scrollStyle switch
                {
                    ScrollStyle.Flame => "ScrollFlameTechnique",
                    ScrollStyle.Cosmic => "ScrollCosmicTechnique",
                    ScrollStyle.Energy => "ScrollEnergyTechnique",
                    ScrollStyle.Void => "ScrollVoidTechnique",
                    ScrollStyle.Holy => "ScrollHolyTechnique",
                    _ => "ScrollFlameTechnique"
                };

                if (shader.Techniques[techniqueName] != null)
                    shader.CurrentTechnique = shader.Techniques[techniqueName];

                // --- Core params ---
                shader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
                shader.Parameters["uColor"]?.SetValue(primaryColor.ToVector3());
                shader.Parameters["uSecondaryColor"]?.SetValue(secondaryColor.ToVector3());
                shader.Parameters["uIntensity"]?.SetValue(intensity);
                shader.Parameters["uOpacity"]?.SetValue(1f);
                shader.Parameters["uProgress"]?.SetValue(0f);

                // --- Scrolling-specific params ---
                shader.Parameters["uScrollSpeed"]?.SetValue(scrollSpeed);
                shader.Parameters["uNoiseScale"]?.SetValue(noiseScale);

                // --- Gap #2: Overbright multiplier ---
                shader.Parameters["uOverbrightMult"]?.SetValue(Math.Max(1f, overbrightMult));

                // --- Gap #3: Conditional glow ---
                shader.Parameters["uGlowThreshold"]?.SetValue(MathHelper.Clamp(glowThreshold, 0f, 1f));
                shader.Parameters["uGlowIntensity"]?.SetValue(Math.Max(0f, glowIntensity));

                // --- Gap #1: Bind noise texture to sampler slot 1 (uImage1) ---
                Texture2D noiseTex = noiseTextureOverride ?? ShaderLoader.GetDefaultScrollStyleTexture((int)scrollStyle);
                if (noiseTex != null)
                {
                    device.Textures[1] = noiseTex;
                    device.SamplerStates[1] = SamplerState.LinearWrap;
                    shader.Parameters["uHasSecondaryTex"]?.SetValue(1f);
                    shader.Parameters["uSecondaryTexScale"]?.SetValue(1f);
                    shader.Parameters["uSecondaryTexScroll"]?.SetValue(0.5f);
                }
                else
                {
                    shader.Parameters["uHasSecondaryTex"]?.SetValue(0f);
                }

                shader.CurrentTechnique.Passes[0].Apply();

                // Draw primitives
                int primitiveCount = (vertexCount / 2) - 1;
                device.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    _vertexBuffer, 0, vertexCount,
                    _indexBuffer, 0, primitiveCount * 2);
            }
            finally
            {
                // --- Gap #7: Clean up secondary texture to prevent leaking ---
                device.Textures[1] = null;

                // Restore SpriteBatch
                Main.spriteBatch.Begin(
                    SpriteSortMode.Deferred,
                    BlendState.AlphaBlend,
                    SamplerState.LinearClamp,
                    DepthStencilState.None,
                    RasterizerState.CullNone,
                    null,
                    Main.GameViewMatrix.TransformationMatrix);
            }
        }

        #endregion

        #region Theme Integration

        /// <summary>
        /// Draws a trail using MagnumOpus theme colors.
        /// </summary>
        public static void DrawThemedTrail(
            Vector2[] positions,
            string themeName,
            TrailStyle style = TrailStyle.Flame,
            float baseWidth = DefaultWidth,
            float intensity = 1f)
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
                _ => new[] { Color.White, Color.Gray }
            };

            Color primary = palette.Length > 0 ? palette[0] : Color.White;
            Color secondary = palette.Length > 1 ? palette[1] : palette[0];

            DrawTrail(positions, null, style, baseWidth, primary, secondary, intensity);
        }

        /// <summary>
        /// Draws a projectile trail using MagnumOpus theme colors with bloom.
        /// </summary>
        public static void DrawThemedProjectileTrail(
            Projectile projectile,
            string themeName,
            TrailStyle style = TrailStyle.Flame,
            float baseWidth = DefaultWidth,
            float intensity = 1f,
            bool withBloom = true)
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
                _ => new[] { Color.White, Color.Gray }
            };

            Color primary = palette.Length > 0 ? palette[0] : Color.White;
            Color secondary = palette.Length > 1 ? palette[1] : palette[0];

            if (withBloom)
            {
                DrawTrailWithBloom(projectile.oldPos, projectile.oldRot, style, 
                    baseWidth, primary, secondary, intensity);
            }
            else
            {
                DrawTrail(projectile.oldPos, projectile.oldRot, style, 
                    baseWidth, primary, secondary, intensity);
            }
        }

        #endregion
    }
}
