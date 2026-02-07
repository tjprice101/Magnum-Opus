using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// WEAPON GLINT / SPECULAR MAPPING SYSTEM
    /// 
    /// Implements Calamity-style shiny weapon effects:
    /// - Glint Texture: Sharp white diagonal line mapped to weapon
    /// - UV Offset: Glint position tied to rotation angle
    /// - Formula: glintPos = sin(rotation + GlobalTime)
    /// - Result: Shine appears to travel across blade as it rotates
    /// 
    /// Technical Details:
    /// - Specular highlight simulation without shaders
    /// - Multi-layer glow for polish effect
    /// - Rotation-aware positioning
    /// 
    /// Usage:
    ///   // In weapon PreDraw:
    ///   WeaponGlintSystem.DrawWeaponWithGlint(item, position, rotation, color);
    /// </summary>
    public class WeaponGlintSystem : ModSystem
    {
        private static Texture2D _glintTexture;
        private static Texture2D _softGlowTexture;
        private static Texture2D _starSparkleTexture;
        
        #region Initialization
        
        public override void Load()
        {
            if (Main.dedServ) return;
        }
        
        public override void Unload()
        {
            // Cache references and null immediately (safe on any thread)
            var glint = _glintTexture;
            var softGlow = _softGlowTexture;
            var starSparkle = _starSparkleTexture;
            _glintTexture = null;
            _softGlowTexture = null;
            _starSparkleTexture = null;
            
            // Queue texture disposal on main thread to avoid ThreadStateException
            Main.QueueMainThreadAction(() =>
            {
                try
                {
                    glint?.Dispose();
                    softGlow?.Dispose();
                    starSparkle?.Dispose();
                }
                catch { }
            });
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Draws a glint effect on a weapon based on its rotation.
        /// Call this in addition to normal weapon drawing.
        /// </summary>
        public static void DrawWeaponGlint(
            SpriteBatch spriteBatch,
            Texture2D weaponTexture,
            Vector2 position,
            float rotation,
            Color tintColor,
            float scale = 1f,
            SpriteEffects effects = SpriteEffects.None)
        {
            EnsureTextures();
            
            // Calculate glint position along the blade
            float time = Main.GlobalTimeWrappedHourly;
            float glintProgress = MathF.Sin(rotation + time * 2f) * 0.5f + 0.5f; // 0 to 1
            
            // Blade length estimate (diagonal of texture)
            float bladeLength = MathF.Sqrt(weaponTexture.Width * weaponTexture.Width + 
                                           weaponTexture.Height * weaponTexture.Height) * scale;
            
            // Position glint along the blade
            Vector2 bladeDirection = rotation.ToRotationVector2();
            Vector2 glintPos = position + bladeDirection * (bladeLength * glintProgress * 0.5f);
            
            // Only draw if glint is in the "visible" part of the oscillation
            float glintIntensity = MathF.Pow(MathF.Sin(time * 3f + rotation), 4f);
            if (glintIntensity < 0.1f) return;
            
            // End current batch for additive blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // === LAYER 1: SOFT GLOW ===
            spriteBatch.Draw(
                _softGlowTexture,
                glintPos,
                null,
                tintColor * 0.3f * glintIntensity,
                0f,
                new Vector2(_softGlowTexture.Width / 2f, _softGlowTexture.Height / 2f),
                0.4f * scale,
                SpriteEffects.None,
                0f
            );
            
            // === LAYER 2: MAIN GLINT (diagonal line) ===
            float glintRotation = rotation + MathHelper.PiOver4; // Diagonal
            spriteBatch.Draw(
                _glintTexture,
                glintPos,
                null,
                Color.White * 0.8f * glintIntensity,
                glintRotation,
                new Vector2(_glintTexture.Width / 2f, _glintTexture.Height / 2f),
                0.3f * scale,
                SpriteEffects.None,
                0f
            );
            
            // === LAYER 3: STAR SPARKLE ===
            if (glintIntensity > 0.5f)
            {
                float sparkleRotation = time * 2f;
                spriteBatch.Draw(
                    _starSparkleTexture,
                    glintPos,
                    null,
                    Color.White * (glintIntensity - 0.5f) * 2f,
                    sparkleRotation,
                    new Vector2(_starSparkleTexture.Width / 2f, _starSparkleTexture.Height / 2f),
                    0.25f * scale,
                    SpriteEffects.None,
                    0f
                );
            }
            
            // Restore alpha blend
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        
        /// <summary>
        /// Draws a continuous glint strip along a blade edge.
        /// For held weapons that should always shine.
        /// </summary>
        public static void DrawBladeEdgeGlint(
            SpriteBatch spriteBatch,
            Vector2 hiltPosition,
            Vector2 tipPosition,
            Color glintColor,
            float width = 4f)
        {
            EnsureTextures();
            
            float time = Main.GlobalTimeWrappedHourly;
            float glintOffset = (time * 2f) % 1f; // Scrolling position
            
            Vector2 bladeVector = tipPosition - hiltPosition;
            float bladeLength = bladeVector.Length();
            float bladeRotation = bladeVector.ToRotation();
            
            // Calculate glint position along the blade
            Vector2 glintPos = Vector2.Lerp(hiltPosition, tipPosition, glintOffset);
            
            // Intensity based on position (brightest in middle of blade)
            float positionIntensity = MathF.Sin(glintOffset * MathHelper.Pi);
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Draw glint as a stretched sprite perpendicular to blade
            spriteBatch.Draw(
                _glintTexture,
                glintPos - Main.screenPosition,
                null,
                glintColor * positionIntensity,
                bladeRotation + MathHelper.PiOver2,
                new Vector2(_glintTexture.Width / 2f, _glintTexture.Height / 2f),
                new Vector2(0.2f, width / _glintTexture.Height),
                SpriteEffects.None,
                0f
            );
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        
        /// <summary>
        /// Creates a static specular highlight at a specific point.
        /// </summary>
        public static void DrawSpecularHighlight(
            SpriteBatch spriteBatch,
            Vector2 position,
            Color color,
            float intensity = 1f,
            float scale = 1f)
        {
            EnsureTextures();
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Soft glow base
            spriteBatch.Draw(
                _softGlowTexture,
                position,
                null,
                color * 0.5f * intensity,
                0f,
                new Vector2(_softGlowTexture.Width / 2f, _softGlowTexture.Height / 2f),
                0.5f * scale,
                SpriteEffects.None,
                0f
            );
            
            // Sharp center
            spriteBatch.Draw(
                _starSparkleTexture,
                position,
                null,
                Color.White * 0.9f * intensity,
                Main.GlobalTimeWrappedHourly,
                new Vector2(_starSparkleTexture.Width / 2f, _starSparkleTexture.Height / 2f),
                0.2f * scale,
                SpriteEffects.None,
                0f
            );
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        
        #endregion
        
        #region Texture Generation
        
        private static void EnsureTextures()
        {
            if (_glintTexture == null || _glintTexture.IsDisposed)
                _glintTexture = CreateGlintTexture(32, 64);
            
            if (_softGlowTexture == null || _softGlowTexture.IsDisposed)
                _softGlowTexture = CreateSoftGlowTexture(64);
            
            if (_starSparkleTexture == null || _starSparkleTexture.IsDisposed)
                _starSparkleTexture = CreateStarSparkleTexture(32);
        }
        
        /// <summary>
        /// Creates a diagonal glint texture (the "shine" line).
        /// </summary>
        private static Texture2D CreateGlintTexture(int width, int height)
        {
            var texture = new Texture2D(Main.graphics.GraphicsDevice, width, height);
            var data = new Color[width * height];
            
            float diagonalLength = MathF.Sqrt(width * width + height * height);
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Distance from center of texture
                    float cx = x - width / 2f;
                    float cy = y - height / 2f;
                    
                    // Distance from diagonal line (y = x scaled)
                    float diagonalDist = MathF.Abs(cy - cx * (float)height / width);
                    float normalizedDist = diagonalDist / (width * 0.3f);
                    
                    // Falloff from diagonal
                    float alpha = MathF.Max(0f, 1f - normalizedDist);
                    alpha = MathF.Pow(alpha, 2f);
                    
                    // Also fade at ends
                    float endFade = 1f - MathF.Abs(cy) / (height / 2f);
                    endFade = MathF.Pow(MathF.Max(0f, endFade), 0.5f);
                    
                    data[y * width + x] = Color.White * alpha * endFade;
                }
            }
            
            texture.SetData(data);
            return texture;
        }
        
        /// <summary>
        /// Creates a soft circular glow texture.
        /// </summary>
        private static Texture2D CreateSoftGlowTexture(int size)
        {
            var texture = new Texture2D(Main.graphics.GraphicsDevice, size, size);
            var data = new Color[size * size];
            
            float center = size / 2f;
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    float normalizedDist = dist / center;
                    
                    float alpha = MathF.Max(0f, 1f - normalizedDist);
                    alpha = MathF.Pow(alpha, 1.5f); // Soft falloff
                    
                    data[y * size + x] = Color.White * alpha;
                }
            }
            
            texture.SetData(data);
            return texture;
        }
        
        /// <summary>
        /// Creates a 4-pointed star sparkle texture.
        /// </summary>
        private static Texture2D CreateStarSparkleTexture(int size)
        {
            var texture = new Texture2D(Main.graphics.GraphicsDevice, size, size);
            var data = new Color[size * size];
            
            float center = size / 2f;
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float dist = MathF.Sqrt(dx * dx + dy * dy);
                    
                    // 4-pointed star: bright along axes
                    float axisX = MathF.Max(0f, 1f - MathF.Abs(dy) / 2f);
                    float axisY = MathF.Max(0f, 1f - MathF.Abs(dx) / 2f);
                    
                    float distFade = MathF.Max(0f, 1f - dist / center);
                    float alpha = MathF.Max(axisX, axisY) * distFade;
                    alpha = MathF.Pow(alpha, 1.2f);
                    
                    data[y * size + x] = Color.White * alpha;
                }
            }
            
            texture.SetData(data);
            return texture;
        }
        
        #endregion
    }
}
