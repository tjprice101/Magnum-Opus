using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using ReLogic.Content;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// SCREEN HEAT DISTORTION SYSTEM
    /// 
    /// Implements screen-space post-processing for heat haze/shimmer effects.
    /// Uses noise-based UV displacement on the render target to create
    /// the "heat wave" distortion seen in Calamity boss attacks.
    /// 
    /// Technical Approach:
    /// - Render game to render target
    /// - Apply UV displacement based on noise texture
    /// - Optional chromatic aberration (RGB channel separation)
    /// - Intensity falloff from source points
    /// - Additive with normal rendering (not replacing)
    /// </summary>
    public class ScreenHeatDistortionSystem : ModSystem
    {
        #region Static Instance
        
        private static ScreenHeatDistortionSystem _instance;
        public static ScreenHeatDistortionSystem Instance => _instance;
        
        #endregion
        
        #region Distortion Sources
        
        /// <summary>
        /// A source of heat distortion in the world.
        /// </summary>
        public class HeatSource
        {
            public int Id;
            public Vector2 WorldPosition;
            public float Radius;
            public float Intensity;
            public float ChromaticStrength;
            public float NoiseScale;
            public float ScrollSpeed;
            public float Lifetime;
            public float Age;
            public bool Persistent;
            public bool IsComplete;
            
            // Animation
            public float PulseFrequency;
            public float PulseAmount;
        }
        
        private List<HeatSource> _sources;
        private int _nextSourceId;
        
        #endregion
        
        #region Render State
        
        private Effect _distortionShader;
        private Texture2D _noiseTexture;
        private RenderTarget2D _screenCapture;
        private bool _isRendering;
        private bool _needsRender;
        
        // Shader parameters
        private float _globalDistortionScale = 1f;
        private bool _chromaticAberrationEnabled = true;
        
        #endregion
        
        #region Lifecycle
        
        public override void Load()
        {
            _instance = this;
            _sources = new List<HeatSource>(32);
            _nextSourceId = 0;
            
            Main.QueueMainThreadAction(LoadResources);
            
            // Hook into rendering
            On_Main.DoDraw += HookRenderPass;
        }
        
        public override void Unload()
        {
            _instance = null;
            _sources?.Clear();
            
            _screenCapture?.Dispose();
            
            On_Main.DoDraw -= HookRenderPass;
        }
        
        private void LoadResources()
        {
            if (Main.dedServ) return;
            
            try
            {
                // Load distortion shader
                if (ModContent.HasAsset("MagnumOpus/Assets/Shaders/AdvancedDistortionShader"))
                {
                    _distortionShader = ModContent.Request<Effect>(
                        "MagnumOpus/Assets/Shaders/AdvancedDistortionShader",
                        AssetRequestMode.ImmediateLoad
                    ).Value;
                }
                
                // Get noise texture from registry
                _noiseTexture = VFXTextureRegistry.Noise.Worley128;
                if (_noiseTexture == null)
                {
                    _noiseTexture = VFXTextureRegistry.GetGenericNoise();
                }
            }
            catch (Exception ex)
            {
                Main.NewText($"[HeatDistortion] Load error: {ex.Message}", Color.Red);
            }
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Creates a heat distortion source at a world position.
        /// </summary>
        public static int CreateHeatSource(
            Vector2 worldPosition,
            float radius,
            float intensity = 0.03f,
            float lifetime = -1f,
            float chromaticStrength = 0.01f)
        {
            if (_instance == null) return -1;
            return _instance.CreateHeatSourceInternal(worldPosition, radius, intensity, lifetime, chromaticStrength);
        }
        
        /// <summary>
        /// Creates a pulsing heat distortion (for boss auras).
        /// </summary>
        public static int CreatePulsingHeatSource(
            Vector2 worldPosition,
            float radius,
            float baseIntensity,
            float pulseFrequency = 2f,
            float pulseAmount = 0.5f)
        {
            if (_instance == null) return -1;
            
            int id = _instance.CreateHeatSourceInternal(worldPosition, radius, baseIntensity, -1f, 0.005f);
            var source = _instance.GetSourceById(id);
            if (source != null)
            {
                source.PulseFrequency = pulseFrequency;
                source.PulseAmount = pulseAmount;
                source.Persistent = true;
            }
            return id;
        }
        
        /// <summary>
        /// Updates the position of a heat source.
        /// </summary>
        public static void UpdateHeatSourcePosition(int sourceId, Vector2 worldPosition)
        {
            var source = _instance?.GetSourceById(sourceId);
            if (source != null)
            {
                source.WorldPosition = worldPosition;
            }
        }
        
        /// <summary>
        /// Removes a heat source.
        /// </summary>
        public static void RemoveHeatSource(int sourceId)
        {
            var source = _instance?.GetSourceById(sourceId);
            if (source != null)
            {
                source.IsComplete = true;
            }
        }
        
        /// <summary>
        /// Creates a brief flash of distortion (for impacts).
        /// </summary>
        public static void FlashDistortion(Vector2 worldPosition, float radius, float intensity = 0.08f)
        {
            CreateHeatSource(worldPosition, radius, intensity, 0.3f, 0.02f);
        }
        
        /// <summary>
        /// Creates an expanding ring of distortion.
        /// </summary>
        public static void ExpandingRingDistortion(Vector2 worldPosition, float startRadius, float endRadius, float duration, float intensity = 0.04f)
        {
            // This creates an animated ring effect
            int id = CreateHeatSource(worldPosition, startRadius, intensity, duration, 0.015f);
            // The shader handles the ring effect based on radius/age
            // For now, we'll update radius over time in the update loop
        }
        
        /// <summary>
        /// Sets the global distortion scale (0 = disabled, 1 = normal, >1 = intense).
        /// </summary>
        public static void SetGlobalDistortionScale(float scale)
        {
            if (_instance != null)
            {
                _instance._globalDistortionScale = scale;
            }
        }
        
        /// <summary>
        /// Enables/disables chromatic aberration on distortion.
        /// </summary>
        public static void SetChromaticAberration(bool enabled)
        {
            if (_instance != null)
            {
                _instance._chromaticAberrationEnabled = enabled;
            }
        }
        
        #endregion
        
        #region Internal Implementation
        
        private int CreateHeatSourceInternal(Vector2 worldPosition, float radius, float intensity, 
            float lifetime, float chromaticStrength)
        {
            var source = new HeatSource
            {
                Id = _nextSourceId++,
                WorldPosition = worldPosition,
                Radius = radius,
                Intensity = intensity,
                ChromaticStrength = chromaticStrength,
                NoiseScale = 4f,
                ScrollSpeed = 0.3f,
                Lifetime = lifetime,
                Age = 0f,
                Persistent = lifetime < 0f
            };
            
            _sources.Add(source);
            _needsRender = true;
            
            return source.Id;
        }
        
        private HeatSource GetSourceById(int id)
        {
            foreach (var source in _sources)
            {
                if (source.Id == id)
                    return source;
            }
            return null;
        }
        
        #endregion
        
        #region Update Loop
        
        public override void PostUpdateEverything()
        {
            UpdateAllSources();
        }
        
        private void UpdateAllSources()
        {
            float deltaTime = 1f / 60f;
            _needsRender = false;
            
            for (int i = _sources.Count - 1; i >= 0; i--)
            {
                var source = _sources[i];
                
                source.Age += deltaTime;
                
                // Check lifetime
                if (!source.Persistent && source.Age >= source.Lifetime)
                {
                    source.IsComplete = true;
                }
                
                // Apply pulsing
                if (source.PulseFrequency > 0)
                {
                    float pulse = (float)Math.Sin(source.Age * source.PulseFrequency * MathHelper.TwoPi);
                    source.Intensity *= (1f + pulse * source.PulseAmount);
                }
                
                // Remove complete sources
                if (source.IsComplete)
                {
                    _sources.RemoveAt(i);
                    continue;
                }
                
                // Check if source is on screen
                Vector2 screenPos = source.WorldPosition - Main.screenPosition;
                if (screenPos.X > -source.Radius && screenPos.X < Main.screenWidth + source.Radius &&
                    screenPos.Y > -source.Radius && screenPos.Y < Main.screenHeight + source.Radius)
                {
                    _needsRender = true;
                }
            }
        }
        
        #endregion
        
        #region Render Pipeline
        
        private void HookRenderPass(On_Main.orig_DoDraw orig, Main self, GameTime gameTime)
        {
            // Call original drawing
            orig(self, gameTime);
            
            // Apply our distortion effect on top
            if (_needsRender && _sources.Count > 0 && !Main.gameMenu)
            {
                ApplyDistortionPass();
            }
        }
        
        private void ApplyDistortionPass()
        {
            if (_distortionShader == null || _noiseTexture == null) return;
            if (_isRendering) return; // Prevent recursion
            
            _isRendering = true;
            
            try
            {
                GraphicsDevice device = Main.graphics.GraphicsDevice;
                SpriteBatch spriteBatch = Main.spriteBatch;
                
                // Ensure render target exists
                EnsureRenderTarget(device);
                
                // Calculate combined distortion parameters
                Vector4[] sourceData = new Vector4[Math.Min(_sources.Count, 8)];
                Vector4[] sourceParams = new Vector4[Math.Min(_sources.Count, 8)];
                
                for (int i = 0; i < sourceData.Length; i++)
                {
                    var source = _sources[i];
                    Vector2 screenPos = source.WorldPosition - Main.screenPosition;
                    
                    // Normalize to screen UV space
                    Vector2 uvPos = new Vector2(
                        screenPos.X / Main.screenWidth,
                        screenPos.Y / Main.screenHeight
                    );
                    
                    // Calculate fade based on lifetime
                    float fade = 1f;
                    if (!source.Persistent && source.Lifetime > 0)
                    {
                        fade = 1f - (source.Age / source.Lifetime);
                        fade = fade * fade; // Quadratic falloff
                    }
                    
                    sourceData[i] = new Vector4(
                        uvPos.X,
                        uvPos.Y,
                        source.Radius / Math.Max(Main.screenWidth, Main.screenHeight),
                        source.Intensity * fade * _globalDistortionScale
                    );
                    
                    sourceParams[i] = new Vector4(
                        source.NoiseScale,
                        source.ScrollSpeed,
                        _chromaticAberrationEnabled ? source.ChromaticStrength : 0f,
                        source.Age
                    );
                }
                
                // Set shader parameters
                float time = (float)Main.gameTimeCache.TotalGameTime.TotalSeconds;
                _distortionShader.Parameters["uTime"]?.SetValue(time);
                _distortionShader.Parameters["uSourceCount"]?.SetValue(_sources.Count);
                _distortionShader.Parameters["uScreenSize"]?.SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
                _distortionShader.Parameters["uNoiseScale"]?.SetValue(4f);
                
                // Set noise texture
                device.Textures[1] = _noiseTexture;
                device.SamplerStates[1] = SamplerState.LinearWrap;
                
                // Apply shader
                spriteBatch.Begin(
                    SpriteSortMode.Immediate,
                    BlendState.AlphaBlend,
                    SamplerState.LinearClamp,
                    DepthStencilState.None,
                    RasterizerState.CullNone,
                    _distortionShader,
                    Main.GameViewMatrix.TransformationMatrix
                );
                
                // Draw per-source distortion
                foreach (var source in _sources)
                {
                    DrawSourceDistortion(spriteBatch, source);
                }
                
                spriteBatch.End();
            }
            catch (Exception ex)
            {
                Main.NewText($"[HeatDistortion] Render error: {ex.Message}", Color.Red);
            }
            finally
            {
                _isRendering = false;
            }
        }
        
        private void DrawSourceDistortion(SpriteBatch spriteBatch, HeatSource source)
        {
            Vector2 screenPos = source.WorldPosition - Main.screenPosition;
            
            // Calculate fade
            float fade = 1f;
            if (!source.Persistent && source.Lifetime > 0)
            {
                fade = 1f - (source.Age / source.Lifetime);
                fade = MathHelper.SmoothStep(0f, 1f, fade);
            }
            
            // Update shader parameters for this source
            _distortionShader.Parameters["uIntensity"]?.SetValue(source.Intensity * fade * _globalDistortionScale);
            _distortionShader.Parameters["uRadius"]?.SetValue(source.Radius);
            _distortionShader.Parameters["uCenter"]?.SetValue(screenPos);
            _distortionShader.Parameters["uChromaticStrength"]?.SetValue(
                _chromaticAberrationEnabled ? source.ChromaticStrength * fade : 0f);
            
            // Draw a screen-aligned quad at the source position
            // The shader handles the distortion
            Rectangle destRect = new Rectangle(
                (int)(screenPos.X - source.Radius),
                (int)(screenPos.Y - source.Radius),
                (int)(source.Radius * 2),
                (int)(source.Radius * 2)
            );
            
            // Use a blank white texture as the base (shader does the work)
            spriteBatch.Draw(
                VFXTextureRegistry.GetWhitePixel(),
                destRect,
                Color.White * fade * 0.3f
            );
        }
        
        private void EnsureRenderTarget(GraphicsDevice device)
        {
            if (_screenCapture == null || 
                _screenCapture.Width != device.Viewport.Width ||
                _screenCapture.Height != device.Viewport.Height)
            {
                _screenCapture?.Dispose();
                _screenCapture = new RenderTarget2D(
                    device,
                    device.Viewport.Width,
                    device.Viewport.Height,
                    false,
                    SurfaceFormat.Color,
                    DepthFormat.None
                );
            }
        }
        
        #endregion
    }
}
