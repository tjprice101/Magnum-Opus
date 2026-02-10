using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// Pixelated primitive trail renderer following FargosSoulsDLC patterns.
    /// 
    /// This renderer draws trails to a low-resolution render target
    /// then upscales with nearest-neighbor filtering for a pixelated look
    /// that matches Terraria's aesthetic.
    /// 
    /// Key features:
    /// - IPixelatedPrimitiveRenderer interface support
    /// - Automatic render target management via RenderTargetPool
    /// - Multi-pass bloom with pixelation
    /// - Proper { A = 0 } alpha pattern for additive blending
    /// 
    /// Usage:
    ///   // In your projectile's PreDraw:
    ///   PixelatedTrailRenderer.DrawPixelatedTrail(
    ///       Projectile.oldPos,
    ///       pos => 20f * (1f - pos),  // Width function
    ///       pos => Color.Cyan * (1f - pos),  // Color function
    ///       pixelScale: 2  // 2x pixelation
    ///   );
    /// </summary>
    public static class PixelatedTrailRenderer
    {
        private static BasicEffect _basicEffect;
        private static VertexPositionColorTexture[] _vertices;
        private static short[] _indices;
        
        private const int MaxVertices = 256;
        private const int MaxIndices = 512;
        
        // Persistent render target name
        private const string PixelatedTrailTargetName = "PixelatedTrailBuffer";
        
        #region Initialization
        
        /// <summary>
        /// Initialize the renderer resources.
        /// </summary>
        public static void Initialize()
        {
            if (Main.dedServ || _basicEffect != null)
                return;
            
            _basicEffect = new BasicEffect(Main.instance.GraphicsDevice)
            {
                VertexColorEnabled = true,
                TextureEnabled = false
            };
            
            _vertices = new VertexPositionColorTexture[MaxVertices];
            _indices = new short[MaxIndices];
        }
        
        #endregion
        
        #region Public Drawing Methods
        
        /// <summary>
        /// Draws a pixelated trail using the given positions.
        /// </summary>
        /// <param name="positions">World positions for the trail</param>
        /// <param name="widthFunction">Function returning width at each position (0=start, 1=end)</param>
        /// <param name="colorFunction">Function returning color at each position</param>
        /// <param name="pixelScale">Pixelation scale (2 = half res, 4 = quarter res)</param>
        /// <param name="smoothen">Whether to apply Catmull-Rom smoothing</param>
        /// <param name="offset">Optional offset to add to all positions</param>
        public static void DrawPixelatedTrail(
            Vector2[] positions,
            Func<float, float> widthFunction,
            Func<float, Color> colorFunction,
            int pixelScale = 2,
            bool smoothen = true,
            Vector2 offset = default)
        {
            if (Main.dedServ || positions == null || positions.Length < 2)
                return;
            
            if (_basicEffect == null)
                Initialize();
            
            // Calculate pixelated dimensions
            int pixelWidth = Main.screenWidth / pixelScale;
            int pixelHeight = Main.screenHeight / pixelScale;
            
            // Get or create pixelated render target
            var pixelTarget = RenderTargetPool.GetPersistent(
                $"{PixelatedTrailTargetName}_{pixelScale}",
                pixelWidth, pixelHeight,
                preserveContents: false);
            
            if (pixelTarget == null)
            {
                // Fallback to non-pixelated
                DrawTrailDirect(positions, widthFunction, colorFunction, smoothen, offset);
                return;
            }
            
            var device = Main.instance.GraphicsDevice;
            var sb = Main.spriteBatch;
            
            // End current SpriteBatch
            try { sb.End(); } catch { }
            
            try
            {
                // Draw trail to low-res target
                RenderTargetPool.SetAndClear(pixelTarget, Color.Transparent);
                
                // Scale positions for low-res target
                Vector2 scale = new Vector2(
                    (float)pixelWidth / Main.screenWidth,
                    (float)pixelHeight / Main.screenHeight
                );
                
                DrawTrailToDevice(positions, widthFunction, colorFunction, smoothen, offset, scale, pixelWidth, pixelHeight);
                
                // Restore back buffer
                RenderTargetPool.RestoreBackBuffer();
                
                // Draw pixelated result to screen with nearest-neighbor sampling
                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                    SamplerState.PointClamp, // Nearest-neighbor for pixelated look
                    DepthStencilState.None, RasterizerState.CullNone,
                    null, Main.GameViewMatrix.TransformationMatrix);
                
                sb.Draw(pixelTarget, 
                    new Rectangle(0, 0, Main.screenWidth, Main.screenHeight),
                    Color.White);
                
                sb.End();
            }
            finally
            {
                // Restart SpriteBatch with default settings
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                    Main.DefaultSamplerState, DepthStencilState.None,
                    Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
        }
        
        /// <summary>
        /// Draws a pixelated trail with multi-pass bloom effect.
        /// </summary>
        public static void DrawPixelatedBloomTrail(
            Vector2[] positions,
            Func<float, float> widthFunction,
            Func<float, Color> colorFunction,
            int pixelScale = 2,
            bool smoothen = true,
            Vector2 offset = default,
            float bloomMultiplier = 2.5f,
            float coreMultiplier = 0.4f)
        {
            if (Main.dedServ || positions == null || positions.Length < 2)
                return;
            
            // Pass 1: Outer bloom (large, dim)
            DrawPixelatedTrail(
                positions,
                p => widthFunction(p) * bloomMultiplier,
                p => colorFunction(p).WithoutAlpha() * 0.3f,
                pixelScale,
                smoothen,
                offset
            );
            
            // Pass 2: Main trail
            DrawPixelatedTrail(
                positions,
                widthFunction,
                p => colorFunction(p).WithoutAlpha(),
                pixelScale,
                smoothen,
                offset
            );
            
            // Pass 3: Bright core
            DrawPixelatedTrail(
                positions,
                p => widthFunction(p) * coreMultiplier,
                p => Color.White.WithoutAlpha() * 0.8f * (1f - p),
                pixelScale,
                smoothen,
                offset
            );
        }
        
        /// <summary>
        /// Draws a projectile trail with pixelation and automatic interpolation.
        /// </summary>
        public static void DrawPixelatedProjectileTrail(
            Projectile projectile,
            Color startColor,
            Color endColor,
            float width,
            int pixelScale = 2)
        {
            if (projectile?.oldPos == null || projectile.oldPos.Length < 2)
                return;
            
            // Create interpolated positions for smooth 144Hz+ rendering
            var positions = CreateInterpolatedPositions(projectile);
            
            // Width: Linear taper
            Func<float, float> widthFunc = p => width * (1f - p);
            
            // Color: Gradient from start to end
            Func<float, Color> colorFunc = p => 
                Color.Lerp(startColor, endColor, p).WithoutAlpha() * projectile.Opacity;
            
            // Offset by projectile size center
            Vector2 offset = projectile.Size * 0.5f;
            
            DrawPixelatedBloomTrail(positions, widthFunc, colorFunc, pixelScale, 
                smoothen: true, offset: offset);
        }
        
        /// <summary>
        /// Draws a themed projectile trail with pixelation.
        /// </summary>
        public static void DrawThemedPixelatedTrail(
            Projectile projectile,
            string themeName,
            float width,
            int pixelScale = 2)
        {
            if (projectile?.oldPos == null || projectile.oldPos.Length < 2)
                return;
            
            var positions = CreateInterpolatedPositions(projectile);
            var palette = MagnumThemePalettes.GetPalette(themeName);
            
            Func<float, float> widthFunc = p => width * (1f - p);
            Func<float, Color> colorFunc = p => VFXUtilities.PaletteLerp(palette, p).WithoutAlpha() * projectile.Opacity;
            Vector2 offset = projectile.Size * 0.5f;
            
            DrawPixelatedBloomTrail(positions, widthFunc, colorFunc, pixelScale,
                smoothen: true, offset: offset);
        }
        
        #endregion
        
        #region Internal Rendering
        
        /// <summary>
        /// Draws a trail directly to the current render target (no pixelation).
        /// </summary>
        private static void DrawTrailDirect(
            Vector2[] positions,
            Func<float, float> widthFunction,
            Func<float, Color> colorFunction,
            bool smoothen,
            Vector2 offset)
        {
            var sb = Main.spriteBatch;
            
            try { sb.End(); } catch { }
            
            try
            {
                DrawTrailToDevice(positions, widthFunction, colorFunction, smoothen, offset,
                    Vector2.One, Main.screenWidth, Main.screenHeight);
            }
            finally
            {
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                    Main.DefaultSamplerState, DepthStencilState.None,
                    Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
        }
        
        /// <summary>
        /// Internal method to draw trail geometry to the graphics device.
        /// </summary>
        private static void DrawTrailToDevice(
            Vector2[] positions,
            Func<float, float> widthFunction,
            Func<float, Color> colorFunction,
            bool smoothen,
            Vector2 offset,
            Vector2 positionScale,
            int viewportWidth,
            int viewportHeight)
        {
            // Filter out zero positions
            var validPositions = new List<Vector2>();
            foreach (var pos in positions)
            {
                if (pos != Vector2.Zero)
                    validPositions.Add(pos);
            }
            
            if (validPositions.Count < 2)
                return;
            
            // Apply smoothing if requested
            List<Vector2> finalPositions = smoothen && validPositions.Count >= 4
                ? SmoothTrail(validPositions, Math.Min(50, validPositions.Count * 2))
                : validPositions;
            
            if (finalPositions.Count < 2)
                return;
            
            // Build vertex buffer
            int vertexCount = Math.Min(finalPositions.Count * 2, MaxVertices);
            int triangleCount = Math.Min((finalPositions.Count - 1) * 2, (MaxIndices / 3));
            
            for (int i = 0; i < finalPositions.Count && i * 2 + 1 < MaxVertices; i++)
            {
                float progress = (float)i / (finalPositions.Count - 1);
                float width = widthFunction?.Invoke(progress) ?? 10f;
                Color color = colorFunction?.Invoke(progress) ?? Color.White;
                
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
                
                // Convert to screen position and apply offset
                Vector2 worldPos = finalPositions[i] + offset;
                Vector2 screenPos = (worldPos - Main.screenPosition) * positionScale;
                
                // Scale width for low-res target
                float scaledWidth = width * positionScale.X;
                
                Vector2 topPos = screenPos + perpendicular * scaledWidth * 0.5f;
                Vector2 bottomPos = screenPos - perpendicular * scaledWidth * 0.5f;
                
                _vertices[i * 2] = new VertexPositionColorTexture(
                    new Vector3(topPos, 0),
                    color,
                    new Vector2(progress, 0));
                
                _vertices[i * 2 + 1] = new VertexPositionColorTexture(
                    new Vector3(bottomPos, 0),
                    color,
                    new Vector2(progress, 1));
            }
            
            // Build index buffer
            int idx = 0;
            for (int i = 0; i < finalPositions.Count - 1 && idx + 5 < MaxIndices; i++)
            {
                int baseVertex = i * 2;
                _indices[idx++] = (short)baseVertex;
                _indices[idx++] = (short)(baseVertex + 1);
                _indices[idx++] = (short)(baseVertex + 2);
                _indices[idx++] = (short)(baseVertex + 1);
                _indices[idx++] = (short)(baseVertex + 3);
                _indices[idx++] = (short)(baseVertex + 2);
            }
            
            // Draw primitives
            DrawPrimitives(vertexCount, triangleCount, viewportWidth, viewportHeight);
        }
        
        /// <summary>
        /// Draws the vertex buffer to the graphics device.
        /// </summary>
        private static void DrawPrimitives(int vertexCount, int triangleCount, int viewportWidth, int viewportHeight)
        {
            var device = Main.instance.GraphicsDevice;
            
            // Save states
            var prevBlend = device.BlendState;
            var prevRasterizer = device.RasterizerState;
            var prevDepthStencil = device.DepthStencilState;
            var prevSampler = device.SamplerStates[0];
            
            // Set up rendering states
            device.BlendState = BlendState.Additive;
            device.RasterizerState = RasterizerState.CullNone;
            device.DepthStencilState = DepthStencilState.None;
            device.SamplerStates[0] = SamplerState.LinearClamp;
            
            // Set up matrices for the viewport
            _basicEffect.View = Matrix.CreateLookAt(Vector3.Backward, Vector3.Zero, Vector3.Up);
            _basicEffect.Projection = Matrix.CreateOrthographicOffCenter(0, viewportWidth, viewportHeight, 0, -1, 1);
            _basicEffect.World = Matrix.Identity;
            
            try
            {
                foreach (var pass in _basicEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                }
                
                device.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    _vertices, 0, vertexCount,
                    _indices, 0, triangleCount);
            }
            finally
            {
                // Restore states
                device.BlendState = prevBlend;
                device.RasterizerState = prevRasterizer;
                device.DepthStencilState = prevDepthStencil;
                device.SamplerStates[0] = prevSampler;
            }
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Creates interpolated positions from a projectile for smooth 144Hz+ rendering.
        /// </summary>
        private static Vector2[] CreateInterpolatedPositions(Projectile projectile)
        {
            var result = new Vector2[projectile.oldPos.Length + 1];
            
            // First position: interpolated current position
            result[0] = InterpolatedRenderer.GetInterpolatedCenter(projectile) - projectile.Size * 0.5f;
            
            // Copy old positions with slight smoothing
            for (int i = 0; i < projectile.oldPos.Length; i++)
            {
                if (projectile.oldPos[i] == Vector2.Zero)
                {
                    result[i + 1] = Vector2.Zero;
                }
                else if (i > 0 && i < projectile.oldPos.Length - 1 &&
                         projectile.oldPos[i - 1] != Vector2.Zero &&
                         projectile.oldPos[i + 1] != Vector2.Zero)
                {
                    // Apply subtle smoothing
                    Vector2 smoothed = Vector2.Lerp(
                        projectile.oldPos[i],
                        (projectile.oldPos[i - 1] + projectile.oldPos[i + 1]) * 0.5f,
                        0.12f);
                    result[i + 1] = smoothed;
                }
                else
                {
                    result[i + 1] = projectile.oldPos[i];
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// Smooths a list of positions using Catmull-Rom interpolation.
        /// </summary>
        private static List<Vector2> SmoothTrail(List<Vector2> positions, int outputPoints)
        {
            if (positions.Count < 4)
                return positions;
            
            var result = new List<Vector2>(outputPoints);
            
            for (int i = 0; i < outputPoints; i++)
            {
                float t = (float)i / (outputPoints - 1) * (positions.Count - 1);
                int segment = (int)t;
                float localT = t - segment;
                
                int p0 = Math.Max(0, segment - 1);
                int p1 = segment;
                int p2 = Math.Min(positions.Count - 1, segment + 1);
                int p3 = Math.Min(positions.Count - 1, segment + 2);
                
                result.Add(CatmullRom(positions[p0], positions[p1], positions[p2], positions[p3], localT));
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
        
        #endregion
    }
}
