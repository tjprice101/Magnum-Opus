using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// CALAMITY-STYLE SKYBOX/SCREEN EFFECT SYSTEM
    /// 
    /// Implements screen-space texturing for boss fights:
    /// - Perlin Noise: Procedural noise for distortion/clouds
    /// - Gradient Maps: Theme-based color gradients
    /// - UV Shifting: Time-based scroll for dynamic visuals
    /// 
    /// Usage:
    /// 1. Call ActivateBossSky() when boss spawns
    /// 2. Call DeactivateBossSky() when boss dies
    /// 3. Shader handles the rest via Main.OnPreDraw
    /// </summary>
    public class CalamitySkyboxRenderer : ModSystem
    {
        // Active sky effects
        private static bool _skyActive;
        private static string _activeTheme;
        private static float _skyIntensity;
        private static float _targetIntensity;
        private static Color _primaryColor;
        private static Color _secondaryColor;
        private static float _noiseScale;
        private static float _scrollSpeed;
        
        // Perlin noise texture
        private static Texture2D _noiseTexture;
        private static Texture2D _gradientTexture;
        
        // Flash effect for big attacks
        private static float _flashIntensity;
        private static Color _flashColor;
        
        public override void Load()
        {
            if (Main.dedServ)
                return;
            
            _skyActive = false;
            _skyIntensity = 0f;
            _targetIntensity = 0f;
            
            // Hook into pre-draw for screen effects
            Main.OnPostDraw += DrawScreenEffects;
        }
        
        public override void Unload()
        {
            Main.OnPostDraw -= DrawScreenEffects;
            
            // Cache references and null immediately (safe on any thread)
            var noise = _noiseTexture;
            var gradient = _gradientTexture;
            _noiseTexture = null;
            _gradientTexture = null;
            
            // Queue texture disposal on main thread to avoid ThreadStateException
            Main.QueueMainThreadAction(() =>
            {
                try
                {
                    noise?.Dispose();
                    gradient?.Dispose();
                }
                catch { }
            });
        }
        
        public override void PostUpdateEverything()
        {
            // Smooth intensity transitions
            _skyIntensity = MathHelper.Lerp(_skyIntensity, _targetIntensity, 0.05f);
            
            // Fade out flash
            if (_flashIntensity > 0f)
                _flashIntensity *= 0.9f;
        }
        
        #region Public API
        
        /// <summary>
        /// Activate boss sky effect with theme colors
        /// </summary>
        public static void ActivateBossSky(string theme, Color primary, Color secondary, 
            float noiseScale = 0.5f, float scrollSpeed = 0.02f)
        {
            _skyActive = true;
            _activeTheme = theme;
            _targetIntensity = 1f;
            _primaryColor = primary;
            _secondaryColor = secondary;
            _noiseScale = noiseScale;
            _scrollSpeed = scrollSpeed;
            
            GenerateNoiseTexture();
            GenerateGradientTexture(primary, secondary);
        }
        
        /// <summary>
        /// Deactivate boss sky with smooth fade
        /// </summary>
        public static void DeactivateBossSky()
        {
            _targetIntensity = 0f;
        }
        
        /// <summary>
        /// Force immediate deactivation
        /// </summary>
        public static void ForceClearSky()
        {
            _skyActive = false;
            _skyIntensity = 0f;
            _targetIntensity = 0f;
        }
        
        /// <summary>
        /// Trigger a screen flash (for big attacks)
        /// </summary>
        public static void TriggerFlash(Color color, float intensity = 1f)
        {
            _flashColor = color;
            _flashIntensity = intensity;
        }
        
        /// <summary>
        /// Update intensity dynamically (e.g., based on boss HP)
        /// </summary>
        public static void SetIntensity(float intensity)
        {
            _targetIntensity = MathHelper.Clamp(intensity, 0f, 1f);
        }
        
        /// <summary>
        /// Update colors dynamically (e.g., for phase transitions)
        /// </summary>
        public static void SetColors(Color primary, Color secondary)
        {
            _primaryColor = primary;
            _secondaryColor = secondary;
            GenerateGradientTexture(primary, secondary);
        }
        
        #endregion
        
        #region Screen Effect Drawing
        
        private static void DrawScreenEffects(GameTime gameTime)
        {
            if (Main.dedServ)
                return;
            
            // Skip if intensity is negligible
            if (_skyIntensity < 0.01f && _flashIntensity < 0.01f)
                return;
            
            // Draw overlay effects
            SpriteBatch spriteBatch = Main.spriteBatch;
            
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone);
            
            // Draw noise overlay
            if (_skyIntensity > 0.01f && _noiseTexture != null)
            {
                DrawNoiseOverlay(spriteBatch);
            }
            
            // Draw flash
            if (_flashIntensity > 0.01f)
            {
                DrawFlash(spriteBatch);
            }
            
            spriteBatch.End();
        }
        
        /// <summary>
        /// Draw scrolling noise overlay for boss atmospherics
        /// </summary>
        private static void DrawNoiseOverlay(SpriteBatch spriteBatch)
        {
            float time = Main.GlobalTimeWrappedHourly;
            
            // UV shifting (scroll the noise)
            Vector2 screenSize = new Vector2(Main.screenWidth, Main.screenHeight);
            float uvOffsetX = time * _scrollSpeed;
            float uvOffsetY = time * _scrollSpeed * 0.7f;
            
            // Source rectangle with UV shift (ensure positive modulo)
            int tileX = (int)(uvOffsetX * _noiseTexture.Width) % _noiseTexture.Width;
            int tileY = (int)(uvOffsetY * _noiseTexture.Height) % _noiseTexture.Height;
            if (tileX < 0) tileX += _noiseTexture.Width;
            if (tileY < 0) tileY += _noiseTexture.Height;
            Rectangle sourceRect = new Rectangle(tileX, tileY, 
                (int)(Main.screenWidth * _noiseScale), (int)(Main.screenHeight * _noiseScale));
            
            // Draw noise with theme color tint
            Color tintColor = Color.Lerp(_primaryColor, _secondaryColor, 
                (float)Math.Sin(time * 2f) * 0.5f + 0.5f) * _skyIntensity * 0.3f;
            
            spriteBatch.Draw(_noiseTexture, Vector2.Zero, sourceRect, tintColor, 0f,
                Vector2.Zero, 1f / _noiseScale, SpriteEffects.None, 0f);
            
            // Draw gradient overlay
            if (_gradientTexture != null)
            {
                // Vignette-style gradient (bright center, themed edges)
                Color gradientColor = _primaryColor * _skyIntensity * 0.15f;
                spriteBatch.Draw(_gradientTexture, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight),
                    gradientColor);
            }
        }
        
        /// <summary>
        /// Draw screen flash
        /// </summary>
        private static void DrawFlash(SpriteBatch spriteBatch)
        {
            Texture2D pixel = GetPixelTexture();
            Color flashColor = _flashColor * _flashIntensity;
            
            spriteBatch.Draw(pixel, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), flashColor);
        }
        
        #endregion
        
        #region Texture Generation
        
        /// <summary>
        /// Generate Perlin noise texture for cloud/distortion effects
        /// </summary>
        private static void GenerateNoiseTexture()
        {
            if (_noiseTexture != null && !_noiseTexture.IsDisposed)
                return;
            
            int size = 256;
            _noiseTexture = new Texture2D(Main.graphics.GraphicsDevice, size, size);
            Color[] data = new Color[size * size];
            
            // Simple multi-octave noise
            Random rand = new Random(42); // Fixed seed for consistency
            float[,] noise = new float[size, size];
            
            // Generate base noise
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    noise[x, y] = (float)rand.NextDouble();
                }
            }
            
            // Smooth with multiple octaves
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float value = 0f;
                    float amplitude = 1f;
                    float frequency = 1f;
                    float maxValue = 0f;
                    
                    for (int octave = 0; octave < 4; octave++)
                    {
                        int sampleX = (int)(x * frequency) % size;
                        int sampleY = (int)(y * frequency) % size;
                        if (sampleX < 0) sampleX += size;
                        if (sampleY < 0) sampleY += size;
                        
                        value += SampleSmoothed(noise, size, sampleX, sampleY) * amplitude;
                        maxValue += amplitude;
                        
                        amplitude *= 0.5f;
                        frequency *= 2f;
                    }
                    
                    value /= maxValue;
                    data[y * size + x] = new Color(value, value, value, value);
                }
            }
            
            _noiseTexture.SetData(data);
        }
        
        private static float SampleSmoothed(float[,] noise, int size, int x, int y)
        {
            // Bilinear sampling
            int x0 = x % size;
            int x1 = (x + 1) % size;
            int y0 = y % size;
            int y1 = (y + 1) % size;
            
            float corners = (noise[x0, y0] + noise[x1, y0] + noise[x0, y1] + noise[x1, y1]) / 16f;
            float sides = (noise[x0, y0] + noise[x1, y0] + noise[x0, y1] + noise[x1, y1]) / 8f;
            float center = noise[x, y] / 4f;
            
            return corners + sides + center;
        }
        
        /// <summary>
        /// Generate radial gradient texture for vignette effects
        /// </summary>
        private static void GenerateGradientTexture(Color primary, Color secondary)
        {
            if (_gradientTexture != null && !_gradientTexture.IsDisposed)
                _gradientTexture.Dispose();
            
            int size = 128;
            _gradientTexture = new Texture2D(Main.graphics.GraphicsDevice, size, size);
            Color[] data = new Color[size * size];
            
            Vector2 center = new Vector2(size / 2f, size / 2f);
            float maxDist = center.Length();
            
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center) / maxDist;
                    
                    // Radial gradient from secondary (edges) to transparent (center)
                    float alpha = dist * dist; // Quadratic falloff
                    Color color = Color.Lerp(primary, secondary, dist) * alpha;
                    
                    data[y * size + x] = color;
                }
            }
            
            _gradientTexture.SetData(data);
        }
        
        private static Texture2D _pixelTexture;
        private static Texture2D GetPixelTexture()
        {
            if (_pixelTexture != null && !_pixelTexture.IsDisposed)
                return _pixelTexture;
            
            _pixelTexture = new Texture2D(Main.graphics.GraphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
            return _pixelTexture;
        }
        
        #endregion
        
        #region Theme Presets
        
        /// <summary>
        /// Preset sky effects for each theme
        /// </summary>
        public static class Presets
        {
            public static void LaCampanella()
            {
                ActivateBossSky("LaCampanella", 
                    new Color(255, 100, 0),   // Infernal orange
                    new Color(30, 20, 25),    // Black smoke
                    noiseScale: 0.3f, scrollSpeed: 0.03f);
            }
            
            public static void Eroica()
            {
                ActivateBossSky("Eroica",
                    new Color(200, 50, 50),   // Scarlet
                    new Color(255, 200, 80),  // Gold
                    noiseScale: 0.4f, scrollSpeed: 0.02f);
            }
            
            public static void SwanLake()
            {
                ActivateBossSky("SwanLake",
                    new Color(255, 255, 255), // White
                    new Color(30, 30, 40),    // Black
                    noiseScale: 0.6f, scrollSpeed: 0.01f);
            }
            
            public static void MoonlightSonata()
            {
                ActivateBossSky("MoonlightSonata",
                    new Color(75, 0, 130),    // Dark purple
                    new Color(135, 206, 250), // Light blue
                    noiseScale: 0.5f, scrollSpeed: 0.015f);
            }
            
            public static void EnigmaVariations()
            {
                ActivateBossSky("EnigmaVariations",
                    new Color(140, 60, 200),  // Purple
                    new Color(50, 220, 100),  // Green flame
                    noiseScale: 0.4f, scrollSpeed: 0.025f);
            }
            
            public static void Fate()
            {
                ActivateBossSky("Fate",
                    new Color(180, 50, 100),  // Dark pink
                    new Color(15, 5, 20),     // Cosmic black
                    noiseScale: 0.3f, scrollSpeed: 0.02f);
            }
        }
        
        #endregion
    }
}
