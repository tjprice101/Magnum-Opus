using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace MagnumOpus.Common.Systems.VFX.Bloom
{
    /// <summary>
    /// Full-screen bloom post-processing system.
    /// Implements bright pass extraction, Gaussian blur, and compositing.
    /// 
    /// Note: This is a framework for full-screen bloom. In tModLoader,
    /// you would typically integrate this with the game's render pipeline
    /// via ModSystem hooks.
    /// 
    /// USAGE:
    /// var bloom = new BloomPostProcess();
    /// bloom.Initialize(Main.instance.GraphicsDevice, screenWidth, screenHeight);
    /// bloom.Apply(spriteBatch, () => { DrawYourContent(); });
    /// </summary>
    public class BloomPostProcess : IDisposable
    {
        #region Fields
        
        private GraphicsDevice device;
        
        // Render targets
        private RenderTarget2D sceneRT;
        private RenderTarget2D brightPassRT;
        private RenderTarget2D blurH_RT;
        private RenderTarget2D blurV_RT;
        
        // Dimensions
        private int sceneWidth;
        private int sceneHeight;
        private int bloomWidth;
        private int bloomHeight;
        
        // State
        private bool isInitialized;
        private bool isDisposed;
        
        #endregion
        
        #region Properties
        
        /// <summary>
        /// Brightness threshold for bloom extraction (0-1).
        /// Pixels brighter than this contribute to bloom.
        /// </summary>
        public float Threshold { get; set; } = 0.8f;
        
        /// <summary>
        /// Bloom intensity multiplier.
        /// </summary>
        public float Intensity { get; set; } = 1.5f;
        
        /// <summary>
        /// Blur spread size.
        /// </summary>
        public float BlurSize { get; set; } = 2f;
        
        /// <summary>
        /// Number of blur passes (more = smoother but slower).
        /// </summary>
        public int BlurPasses { get; set; } = 2;
        
        /// <summary>
        /// Bloom resolution divisor (2 = half resolution, 4 = quarter).
        /// Lower = faster but blurrier.
        /// </summary>
        public int ResolutionDivisor { get; set; } = 2;
        
        /// <summary>
        /// Whether bloom is enabled.
        /// </summary>
        public bool Enabled { get; set; } = true;
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Initialize the bloom system with screen dimensions.
        /// </summary>
        public void Initialize(GraphicsDevice graphicsDevice, int width, int height)
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(BloomPostProcess));
            
            device = graphicsDevice;
            sceneWidth = width;
            sceneHeight = height;
            
            // Bloom at reduced resolution for performance
            bloomWidth = width / ResolutionDivisor;
            bloomHeight = height / ResolutionDivisor;
            
            // Create render targets
            CreateRenderTargets();
            
            isInitialized = true;
        }
        
        private void CreateRenderTargets()
        {
            // Dispose existing if resizing
            DisposeRenderTargets();
            
            // Scene capture (full resolution)
            sceneRT = new RenderTarget2D(
                device, sceneWidth, sceneHeight, false,
                SurfaceFormat.Color, DepthFormat.None, 0,
                RenderTargetUsage.PreserveContents
            );
            
            // Bloom chain (reduced resolution)
            brightPassRT = new RenderTarget2D(
                device, bloomWidth, bloomHeight, false,
                SurfaceFormat.Color, DepthFormat.None
            );
            
            blurH_RT = new RenderTarget2D(
                device, bloomWidth, bloomHeight, false,
                SurfaceFormat.Color, DepthFormat.None
            );
            
            blurV_RT = new RenderTarget2D(
                device, bloomWidth, bloomHeight, false,
                SurfaceFormat.Color, DepthFormat.None
            );
        }
        
        private void DisposeRenderTargets()
        {
            sceneRT?.Dispose();
            brightPassRT?.Dispose();
            blurH_RT?.Dispose();
            blurV_RT?.Dispose();
            
            sceneRT = null;
            brightPassRT = null;
            blurH_RT = null;
            blurV_RT = null;
        }
        
        /// <summary>
        /// Handle screen resize.
        /// </summary>
        public void Resize(int newWidth, int newHeight)
        {
            if (newWidth != sceneWidth || newHeight != sceneHeight)
            {
                sceneWidth = newWidth;
                sceneHeight = newHeight;
                bloomWidth = newWidth / ResolutionDivisor;
                bloomHeight = newHeight / ResolutionDivisor;
                
                CreateRenderTargets();
            }
        }
        
        #endregion
        
        #region Apply Bloom (Simplified - No Shader)
        
        /// <summary>
        /// Apply bloom post-processing using CPU-based bright pass.
        /// This is a simplified version that works without custom shaders.
        /// For full shader-based bloom, use ApplyWithShader().
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch to use</param>
        /// <param name="drawSceneAction">Action that draws the scene content</param>
        public void Apply(SpriteBatch spriteBatch, Action drawSceneAction)
        {
            if (!Enabled || !isInitialized || isDisposed)
            {
                drawSceneAction?.Invoke();
                return;
            }
            
            // Step 1: Render scene to texture
            device.SetRenderTarget(sceneRT);
            device.Clear(Color.Transparent);
            drawSceneAction?.Invoke();
            
            // Step 2: Extract bright pixels (simplified - just copy with threshold tint)
            device.SetRenderTarget(brightPassRT);
            device.Clear(Color.Transparent);
            
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend,
                SamplerState.LinearClamp, null, null);
            
            // Draw scene scaled down (acts as low-pass filter)
            spriteBatch.Draw(sceneRT, 
                new Rectangle(0, 0, bloomWidth, bloomHeight),
                Color.White);
            
            spriteBatch.End();
            
            // Step 3: Blur passes
            RenderTarget2D source = brightPassRT;
            
            for (int pass = 0; pass < BlurPasses; pass++)
            {
                // Horizontal blur (draw stretched)
                device.SetRenderTarget(blurH_RT);
                device.Clear(Color.Transparent);
                
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend,
                    SamplerState.LinearClamp, null, null);
                
                // Simulate blur by drawing multiple offset copies
                float offset = BlurSize * (pass + 1);
                spriteBatch.Draw(source, Vector2.Zero, Color.White * 0.4f);
                spriteBatch.Draw(source, new Vector2(-offset, 0), Color.White * 0.15f);
                spriteBatch.Draw(source, new Vector2(offset, 0), Color.White * 0.15f);
                spriteBatch.Draw(source, new Vector2(-offset * 2, 0), Color.White * 0.1f);
                spriteBatch.Draw(source, new Vector2(offset * 2, 0), Color.White * 0.1f);
                
                spriteBatch.End();
                
                // Vertical blur
                device.SetRenderTarget(blurV_RT);
                device.Clear(Color.Transparent);
                
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend,
                    SamplerState.LinearClamp, null, null);
                
                spriteBatch.Draw(blurH_RT, Vector2.Zero, Color.White * 0.4f);
                spriteBatch.Draw(blurH_RT, new Vector2(0, -offset), Color.White * 0.15f);
                spriteBatch.Draw(blurH_RT, new Vector2(0, offset), Color.White * 0.15f);
                spriteBatch.Draw(blurH_RT, new Vector2(0, -offset * 2), Color.White * 0.1f);
                spriteBatch.Draw(blurH_RT, new Vector2(0, offset * 2), Color.White * 0.1f);
                
                spriteBatch.End();
                
                source = blurV_RT;
            }
            
            // Step 4: Composite - draw original scene + bloom
            device.SetRenderTarget(null);
            
            // Draw original scene
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.LinearClamp, null, null);
            spriteBatch.Draw(sceneRT, Vector2.Zero, Color.White);
            spriteBatch.End();
            
            // Add bloom (additive blend, upscaled)
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, null, null);
            spriteBatch.Draw(blurV_RT, 
                new Rectangle(0, 0, sceneWidth, sceneHeight),
                Color.White * Intensity);
            spriteBatch.End();
        }
        
        #endregion
        
        #region Shader-Based Bloom (Framework)
        
        /// <summary>
        /// Apply bloom with custom shader.
        /// Requires compiled bloom shader Effect.
        /// </summary>
        public void ApplyWithShader(SpriteBatch spriteBatch, Effect bloomShader, Action drawSceneAction)
        {
            if (!Enabled || !isInitialized || isDisposed || bloomShader == null)
            {
                drawSceneAction?.Invoke();
                return;
            }
            
            // Step 1: Render scene
            device.SetRenderTarget(sceneRT);
            device.Clear(Color.Transparent);
            drawSceneAction?.Invoke();
            
            // Step 2: Bright pass extraction
            device.SetRenderTarget(brightPassRT);
            device.Clear(Color.Transparent);
            
            bloomShader.Parameters["Threshold"]?.SetValue(Threshold);
            
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque,
                SamplerState.LinearClamp, null, null, bloomShader);
            
            bloomShader.CurrentTechnique = bloomShader.Techniques["BrightPass"];
            spriteBatch.Draw(sceneRT, new Rectangle(0, 0, bloomWidth, bloomHeight), Color.White);
            
            spriteBatch.End();
            
            // Step 3: Blur passes
            Vector2 texelSize = new Vector2(1f / bloomWidth, 1f / bloomHeight);
            bloomShader.Parameters["TexelSize"]?.SetValue(texelSize);
            bloomShader.Parameters["BlurSize"]?.SetValue(BlurSize);
            
            RenderTarget2D source = brightPassRT;
            
            for (int pass = 0; pass < BlurPasses; pass++)
            {
                // Horizontal blur
                device.SetRenderTarget(blurH_RT);
                device.Clear(Color.Transparent);
                
                bloomShader.CurrentTechnique = bloomShader.Techniques["HorizontalBlur"];
                
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque,
                    SamplerState.LinearClamp, null, null, bloomShader);
                spriteBatch.Draw(source, Vector2.Zero, Color.White);
                spriteBatch.End();
                
                // Vertical blur
                device.SetRenderTarget(blurV_RT);
                device.Clear(Color.Transparent);
                
                bloomShader.CurrentTechnique = bloomShader.Techniques["VerticalBlur"];
                
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque,
                    SamplerState.LinearClamp, null, null, bloomShader);
                spriteBatch.Draw(blurH_RT, Vector2.Zero, Color.White);
                spriteBatch.End();
                
                source = blurV_RT;
            }
            
            // Step 4: Composite
            device.SetRenderTarget(null);
            
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.LinearClamp, null, null);
            spriteBatch.Draw(sceneRT, Vector2.Zero, Color.White);
            spriteBatch.End();
            
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, null, null);
            spriteBatch.Draw(blurV_RT,
                new Rectangle(0, 0, sceneWidth, sceneHeight),
                Color.White * Intensity);
            spriteBatch.End();
        }
        
        #endregion
        
        #region Kawase Blur (Optimized Alternative)
        
        /// <summary>
        /// Apply Kawase blur - more efficient single-pass blur.
        /// </summary>
        public void ApplyKawaseBlur(SpriteBatch spriteBatch, RenderTarget2D source, 
                                     RenderTarget2D destination, int iteration)
        {
            device.SetRenderTarget(destination);
            device.Clear(Color.Transparent);
            
            float offset = 0.5f + iteration * 0.5f;
            Vector2[] offsets = new Vector2[]
            {
                new Vector2(-offset, -offset),
                new Vector2( offset, -offset),
                new Vector2(-offset,  offset),
                new Vector2( offset,  offset)
            };
            
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend,
                SamplerState.LinearClamp, null, null);
            
            // Draw 4 offset samples and average
            foreach (var off in offsets)
            {
                spriteBatch.Draw(source, off, Color.White * 0.25f);
            }
            
            spriteBatch.End();
        }
        
        #endregion
        
        #region Disposal
        
        public void Dispose()
        {
            if (!isDisposed)
            {
                DisposeRenderTargets();
                isDisposed = true;
                isInitialized = false;
            }
        }
        
        #endregion
    }
}
