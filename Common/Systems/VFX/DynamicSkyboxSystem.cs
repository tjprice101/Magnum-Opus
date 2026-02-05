using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// Dynamic Skybox System - Calamity-style full-screen shader overlays.
    /// 
    /// Instead of static sky swaps, this applies:
    /// - Gradient Mapping for color themes
    /// - Scrolling Noise Textures for energy clouds/voids
    /// - Dynamic Ambient Light bleeding for theme immersion
    /// 
    /// Hook into Main.OnPreDraw for rendering.
    /// </summary>
    public class DynamicSkyboxSystem : ModSystem
    {
        #region Static Instance
        
        private static DynamicSkyboxSystem _instance;
        public static DynamicSkyboxSystem Instance => _instance;
        
        #endregion

        #region State
        
        // Current active effect
        private SkyboxEffect _activeEffect = SkyboxEffect.None;
        private float _effectIntensity;
        private float _targetIntensity;
        private float _transitionSpeed = 0.02f;
        
        // Color theme
        private Color _primaryColor = Color.White;
        private Color _secondaryColor = Color.Black;
        private Color _ambientTint = Color.White;
        
        // Noise scrolling
        private float _noiseScrollX;
        private float _noiseScrollY;
        private float _noiseScale = 1f;
        private float _noiseSpeed = 0.5f;
        
        // Vignette
        private float _vignetteIntensity;
        private float _vignetteRadius = 0.8f;
        
        // Chromatic aberration
        private float _chromaticIntensity;
        
        // Flash effect
        private float _flashIntensity;
        private Color _flashColor = Color.White;
        
        // Render target for post-processing
        private RenderTarget2D _screenBuffer;
        private bool _initialized;
        
        #endregion

        #region Effect Types
        
        public enum SkyboxEffect
        {
            None,
            EroicaHeroic,        // Golden triumphant glow
            LaCampanellaInferno, // Black smoke with orange fire
            SwanLakeMonochrome,  // Black/white contrast with rainbow edges
            MoonlightLunar,      // Purple mist with silver moonlight
            EnigmaVoid,          // Swirling void with green flame accents
            FateCosmic,          // Reality distortion, star fields
            DiesIraeWrath,       // Blood red apocalyptic
            ClairDeLuneDream     // Soft blue ethereal
        }
        
        #endregion

        #region Lifecycle
        
        public override void Load()
        {
            _instance = this;
            
            if (!Main.dedServ)
            {
                Main.OnPreDraw += OnPreDraw;
                Main.OnResolutionChanged += OnResolutionChanged;
            }
        }
        
        public override void Unload()
        {
            if (!Main.dedServ)
            {
                Main.OnPreDraw -= OnPreDraw;
                Main.OnResolutionChanged -= OnResolutionChanged;
            }
            
            _screenBuffer?.Dispose();
            _screenBuffer = null;
            _instance = null;
        }
        
        private void OnResolutionChanged(Vector2 newSize)
        {
            _screenBuffer?.Dispose();
            _screenBuffer = null;
            _initialized = false;
        }
        
        private void Initialize()
        {
            if (_initialized || Main.dedServ)
                return;
                
            try
            {
                _screenBuffer = new RenderTarget2D(
                    Main.graphics.GraphicsDevice,
                    Main.screenWidth,
                    Main.screenHeight,
                    false,
                    SurfaceFormat.Color,
                    DepthFormat.None);
                    
                _initialized = true;
            }
            catch
            {
                _initialized = false;
            }
        }
        
        #endregion

        #region Public API
        
        /// <summary>
        /// Activates a themed skybox effect for boss fights.
        /// </summary>
        public static void ActivateEffect(SkyboxEffect effect, float transitionTime = 1f)
        {
            if (_instance == null) return;
            
            _instance._activeEffect = effect;
            _instance._targetIntensity = 1f;
            _instance._transitionSpeed = transitionTime > 0 ? 1f / (transitionTime * 60f) : 1f;
            _instance.ApplyThemeColors(effect);
        }
        
        /// <summary>
        /// Deactivates the current skybox effect.
        /// </summary>
        public static void DeactivateEffect(float transitionTime = 1f)
        {
            if (_instance == null) return;
            
            _instance._targetIntensity = 0f;
            _instance._transitionSpeed = transitionTime > 0 ? 1f / (transitionTime * 60f) : 1f;
        }
        
        /// <summary>
        /// Triggers a screen flash effect.
        /// </summary>
        public static void TriggerFlash(Color color, float intensity = 1f, float duration = 0.3f)
        {
            if (_instance == null) return;
            
            _instance._flashColor = color;
            _instance._flashIntensity = intensity;
        }
        
        /// <summary>
        /// Sets chromatic aberration intensity for reality distortion effects.
        /// </summary>
        public static void SetChromaticAberration(float intensity)
        {
            if (_instance != null)
                _instance._chromaticIntensity = MathHelper.Clamp(intensity, 0f, 1f);
        }
        
        /// <summary>
        /// Sets vignette parameters.
        /// </summary>
        public static void SetVignette(float intensity, float radius = 0.8f)
        {
            if (_instance == null) return;
            
            _instance._vignetteIntensity = MathHelper.Clamp(intensity, 0f, 1f);
            _instance._vignetteRadius = MathHelper.Clamp(radius, 0.3f, 1f);
        }
        
        /// <summary>
        /// Gets the current ambient light tint for world lighting modification.
        /// Apply this to Lighting.AddLight calls for theme bleeding.
        /// </summary>
        public static Color GetAmbientTint()
        {
            return _instance?._ambientTint ?? Color.White;
        }
        
        /// <summary>
        /// Gets the ambient tint as a multiplier for existing light.
        /// </summary>
        public static Vector3 GetAmbientMultiplier()
        {
            if (_instance == null || _instance._effectIntensity < 0.01f)
                return Vector3.One;
                
            Color tint = _instance._ambientTint;
            float intensity = _instance._effectIntensity;
            
            return Vector3.Lerp(Vector3.One, tint.ToVector3(), intensity * 0.5f);
        }
        
        #endregion

        #region Theme Configuration
        
        private void ApplyThemeColors(SkyboxEffect effect)
        {
            switch (effect)
            {
                case SkyboxEffect.EroicaHeroic:
                    _primaryColor = new Color(255, 200, 80);    // Gold
                    _secondaryColor = new Color(200, 50, 50);   // Scarlet
                    _ambientTint = new Color(255, 220, 150);    // Warm golden
                    _noiseSpeed = 0.3f;
                    _noiseScale = 0.8f;
                    break;
                    
                case SkyboxEffect.LaCampanellaInferno:
                    _primaryColor = new Color(255, 100, 0);     // Orange fire
                    _secondaryColor = new Color(20, 15, 20);    // Black smoke
                    _ambientTint = new Color(255, 150, 100);    // Infernal
                    _noiseSpeed = 0.6f;
                    _noiseScale = 1.2f;
                    break;
                    
                case SkyboxEffect.SwanLakeMonochrome:
                    _primaryColor = Color.White;
                    _secondaryColor = new Color(20, 20, 30);    // Deep black
                    _ambientTint = new Color(240, 240, 255);    // Cool white
                    _noiseSpeed = 0.2f;
                    _noiseScale = 0.6f;
                    break;
                    
                case SkyboxEffect.MoonlightLunar:
                    _primaryColor = new Color(135, 206, 250);   // Light blue
                    _secondaryColor = new Color(75, 0, 130);    // Dark purple
                    _ambientTint = new Color(180, 160, 220);    // Lunar purple
                    _noiseSpeed = 0.15f;
                    _noiseScale = 0.5f;
                    break;
                    
                case SkyboxEffect.EnigmaVoid:
                    _primaryColor = new Color(50, 220, 100);    // Green flame
                    _secondaryColor = new Color(15, 10, 20);    // Void black
                    _ambientTint = new Color(80, 120, 100);     // Eerie
                    _noiseSpeed = 0.8f;
                    _noiseScale = 1.5f;
                    break;
                    
                case SkyboxEffect.FateCosmic:
                    _primaryColor = new Color(255, 60, 80);     // Cosmic red
                    _secondaryColor = new Color(15, 5, 20);     // Void
                    _ambientTint = new Color(200, 150, 180);    // Cosmic pink
                    _noiseSpeed = 0.4f;
                    _noiseScale = 2f;
                    _chromaticIntensity = 0.1f;
                    break;
                    
                case SkyboxEffect.DiesIraeWrath:
                    _primaryColor = new Color(200, 30, 30);     // Blood red
                    _secondaryColor = new Color(40, 10, 10);    // Dark crimson
                    _ambientTint = new Color(255, 100, 100);    // Wrath
                    _noiseSpeed = 1f;
                    _noiseScale = 1.8f;
                    break;
                    
                case SkyboxEffect.ClairDeLuneDream:
                    _primaryColor = new Color(200, 220, 255);   // Dream blue
                    _secondaryColor = new Color(100, 120, 160); // Night mist
                    _ambientTint = new Color(180, 200, 240);    // Dreamy
                    _noiseSpeed = 0.1f;
                    _noiseScale = 0.4f;
                    break;
                    
                default:
                    _primaryColor = Color.White;
                    _secondaryColor = Color.Black;
                    _ambientTint = Color.White;
                    break;
            }
        }
        
        #endregion

        #region Update & Render
        
        public override void PostUpdateEverything()
        {
            // Smooth transition
            _effectIntensity = MathHelper.Lerp(_effectIntensity, _targetIntensity, _transitionSpeed);
            
            if (_effectIntensity < 0.001f && _targetIntensity == 0f)
            {
                _activeEffect = SkyboxEffect.None;
            }
            
            // Update noise scrolling
            _noiseScrollX += _noiseSpeed * 0.01f;
            _noiseScrollY += _noiseSpeed * 0.007f;
            if (_noiseScrollX > 1000f) _noiseScrollX -= 1000f;
            if (_noiseScrollY > 1000f) _noiseScrollY -= 1000f;
            
            // Decay flash
            _flashIntensity *= 0.9f;
            if (_flashIntensity < 0.01f) _flashIntensity = 0f;
            
            // Decay chromatic aberration
            if (_activeEffect != SkyboxEffect.FateCosmic)
                _chromaticIntensity *= 0.95f;
        }
        
        private void OnPreDraw(GameTime gameTime)
        {
            if (Main.dedServ || _activeEffect == SkyboxEffect.None && _flashIntensity < 0.01f)
                return;
                
            if (!_initialized)
                Initialize();
        }
        
        /// <summary>
        /// Call this from a ModSystem's PostDrawTiles or similar to apply overlay effects.
        /// </summary>
        public static void DrawOverlay(SpriteBatch spriteBatch)
        {
            if (_instance == null || Main.dedServ)
                return;
                
            _instance.DrawOverlayInternal(spriteBatch);
        }
        
        private void DrawOverlayInternal(SpriteBatch spriteBatch)
        {
            if (_effectIntensity < 0.01f && _flashIntensity < 0.01f)
                return;
                
            // Get a simple pixel texture for overlay drawing
            Texture2D pixel = MagnumTextureRegistry.GetPixelTexture();
            if (pixel == null) return;
            
            Rectangle screenRect = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);
            
            // Apply gradient overlay based on effect
            if (_effectIntensity > 0.01f)
            {
                // Draw gradient vignette with theme colors
                DrawGradientVignette(spriteBatch, pixel, screenRect);
            }
            
            // Apply flash effect
            if (_flashIntensity > 0.01f)
            {
                Color flashWithAlpha = _flashColor * (_flashIntensity * 0.6f);
                spriteBatch.Draw(pixel, screenRect, flashWithAlpha);
            }
        }
        
        private void DrawGradientVignette(SpriteBatch spriteBatch, Texture2D pixel, Rectangle screenRect)
        {
            // Simple vignette overlay - edges tinted with secondary color
            float alpha = _effectIntensity * 0.3f * _vignetteIntensity;
            
            if (alpha < 0.01f) return;
            
            // Draw edge darkening/tinting
            Color edgeColor = _secondaryColor * alpha;
            
            // Top edge
            spriteBatch.Draw(pixel, new Rectangle(0, 0, screenRect.Width, 100), edgeColor * 0.5f);
            // Bottom edge  
            spriteBatch.Draw(pixel, new Rectangle(0, screenRect.Height - 100, screenRect.Width, 100), edgeColor * 0.5f);
            // Left edge
            spriteBatch.Draw(pixel, new Rectangle(0, 0, 100, screenRect.Height), edgeColor * 0.3f);
            // Right edge
            spriteBatch.Draw(pixel, new Rectangle(screenRect.Width - 100, 0, 100, screenRect.Height), edgeColor * 0.3f);
        }
        
        #endregion
    }
}
