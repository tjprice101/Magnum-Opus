using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// LAYERED MAGIC PROJECTILE SYSTEM
    /// 
    /// Implements the "Magic Projectile Layered Composition" concept:
    /// - Core: Solid pulsing center
    /// - Pulse Layer: Oscillating glow that breathes
    /// - Afterimage Ribbon: Trailing positions with color gradient
    /// - Fresnel Rim: Edge glow that catches light
    /// - Particle Envelope: Orbiting sparkles
    /// 
    /// Usage (in projectile PreDraw):
    ///   LayeredProjectileRenderer.Draw(spriteBatch, projectile, "Fate");
    /// </summary>
    public static class LayeredProjectileRenderer
    {
        private static Texture2D _coreTexture;
        private static Texture2D _glowTexture;
        private static Texture2D _sparkleTexture;
        
        #region Layer Configuration
        
        /// <summary>
        /// Configuration for each layer of the projectile.
        /// </summary>
        public class LayerConfig
        {
            public bool EnableCore = true;
            public bool EnablePulse = true;
            public bool EnableAfterimage = true;
            public bool EnableFresnel = true;
            public bool EnableParticles = true;
            
            public float CoreScale = 0.3f;
            public float PulseScaleMin = 0.5f;
            public float PulseScaleMax = 0.7f;
            public float PulseSpeed = 0.15f;
            
            public int AfterimageCount = 8;
            public float AfterimageSpacing = 0.1f;
            
            public float FresnelIntensity = 0.6f;
            public float FresnelPower = 2f;
            
            public int ParticleCount = 4;
            public float ParticleOrbitRadius = 15f;
            public float ParticleOrbitSpeed = 0.08f;
        }
        
        private static readonly LayerConfig DefaultConfig = new LayerConfig();
        
        #endregion
        
        #region Theme Colors
        
        public static (Color core, Color pulse, Color fresnel, Color[] gradient) GetThemeColors(string theme)
        {
            return theme?.ToLower() switch
            {
                "fate" => (
                    new Color(255, 200, 220),   // Pink-white core
                    new Color(200, 80, 120),    // Dark pink pulse
                    new Color(255, 150, 180),   // Light pink fresnel
                    new Color[] { new Color(20, 5, 20), new Color(180, 50, 100), new Color(255, 80, 120), new Color(255, 200, 220) }
                ),
                "eroica" => (
                    new Color(255, 255, 200),   // Gold-white core
                    new Color(200, 50, 50),     // Scarlet pulse
                    new Color(255, 200, 80),    // Gold fresnel
                    new Color[] { new Color(139, 0, 0), new Color(220, 50, 50), new Color(255, 150, 50), new Color(255, 215, 0) }
                ),
                "moonlightsonata" => (
                    new Color(220, 220, 255),   // Silver-white core
                    new Color(100, 50, 150),    // Purple pulse
                    new Color(150, 180, 255),   // Light blue fresnel
                    new Color[] { new Color(75, 0, 130), new Color(138, 43, 226), new Color(135, 206, 250), new Color(220, 220, 235) }
                ),
                "lacampanella" => (
                    new Color(255, 230, 200),   // Warm white core
                    new Color(255, 100, 0),     // Orange pulse
                    new Color(255, 180, 80),    // Gold-orange fresnel
                    new Color[] { new Color(30, 20, 25), new Color(200, 50, 30), new Color(255, 140, 40), new Color(255, 200, 80) }
                ),
                "swanlake" => (
                    Color.White,                // Pure white core
                    new Color(30, 30, 40),      // Black pulse
                    new Color(200, 200, 255),   // Iridescent fresnel
                    new Color[] { new Color(30, 30, 40), new Color(150, 150, 180), new Color(230, 230, 245), Color.White }
                ),
                "enigma" => (
                    new Color(200, 255, 200),   // Green-white core
                    new Color(100, 50, 150),    // Purple pulse
                    new Color(100, 255, 150),   // Green fresnel
                    new Color[] { new Color(15, 10, 20), new Color(80, 20, 120), new Color(140, 60, 200), new Color(50, 220, 100) }
                ),
                _ => (
                    Color.White,
                    new Color(100, 150, 255),
                    new Color(200, 220, 255),
                    new Color[] { Color.DarkBlue, Color.Blue, Color.Cyan, Color.White }
                )
            };
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Draws a fully layered magic projectile with all effects.
        /// </summary>
        public static void Draw(
            SpriteBatch spriteBatch,
            Projectile projectile,
            string theme,
            LayerConfig config = null)
        {
            config ??= DefaultConfig;
            EnsureTextures();
            
            var (coreColor, pulseColor, fresnelColor, gradient) = GetThemeColors(theme);
            Vector2 drawPos = projectile.Center - Main.screenPosition;
            float rotation = projectile.rotation;
            float time = Main.GameUpdateCount * 0.1f;
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Layer 1: Afterimage ribbon (drawn first, behind everything)
            if (config.EnableAfterimage)
            {
                DrawAfterimageRibbon(spriteBatch, projectile, gradient, config);
            }
            
            // Layer 2: Pulse (breathing glow)
            if (config.EnablePulse)
            {
                float pulsePhase = MathF.Sin(time * config.PulseSpeed * MathHelper.TwoPi);
                float pulseScale = MathHelper.Lerp(config.PulseScaleMin, config.PulseScaleMax, (pulsePhase + 1f) / 2f);
                
                // Multiple bloom passes
                for (int i = 0; i < 3; i++)
                {
                    float layerScale = pulseScale * (1.5f - i * 0.3f);
                    float layerAlpha = 0.3f / (i + 1);
                    
                    spriteBatch.Draw(
                        _glowTexture,
                        drawPos,
                        null,
                        pulseColor * layerAlpha,
                        rotation + time * 0.5f,
                        new Vector2(_glowTexture.Width / 2f, _glowTexture.Height / 2f),
                        layerScale,
                        SpriteEffects.None,
                        0f
                    );
                }
            }
            
            // Layer 3: Fresnel rim (edge glow)
            if (config.EnableFresnel)
            {
                DrawFresnelRim(spriteBatch, drawPos, rotation, fresnelColor, config, time);
            }
            
            // Layer 4: Core (central solid)
            if (config.EnableCore)
            {
                // Core with slight pulse
                float corePulse = 1f + MathF.Sin(time * 0.2f) * 0.1f;
                
                spriteBatch.Draw(
                    _glowTexture,
                    drawPos,
                    null,
                    coreColor * 0.9f,
                    rotation,
                    new Vector2(_glowTexture.Width / 2f, _glowTexture.Height / 2f),
                    config.CoreScale * corePulse,
                    SpriteEffects.None,
                    0f
                );
                
                // White-hot center
                spriteBatch.Draw(
                    _glowTexture,
                    drawPos,
                    null,
                    Color.White * 0.8f,
                    rotation,
                    new Vector2(_glowTexture.Width / 2f, _glowTexture.Height / 2f),
                    config.CoreScale * 0.4f * corePulse,
                    SpriteEffects.None,
                    0f
                );
            }
            
            // Layer 5: Orbiting particles
            if (config.EnableParticles)
            {
                DrawOrbitingParticles(spriteBatch, drawPos, fresnelColor, config, time);
            }
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        
        /// <summary>
        /// Draws just the afterimage ribbon for a projectile.
        /// </summary>
        public static void DrawAfterimageOnly(
            SpriteBatch spriteBatch,
            Projectile projectile,
            Color[] gradient)
        {
            EnsureTextures();
            DrawAfterimageRibbon(spriteBatch, projectile, gradient, DefaultConfig);
        }
        
        #endregion
        
        #region Layer Drawing
        
        private static void DrawAfterimageRibbon(
            SpriteBatch spriteBatch,
            Projectile projectile,
            Color[] gradient,
            LayerConfig config)
        {
            int count = Math.Min(config.AfterimageCount, projectile.oldPos.Length);
            
            for (int i = 0; i < count; i++)
            {
                if (projectile.oldPos[i] == Vector2.Zero) continue;
                
                float progress = (float)i / count;
                float alpha = 1f - progress;
                alpha = MathF.Pow(alpha, 1.5f); // Faster falloff
                
                // Get color from gradient
                Color trailColor = GetGradientColor(gradient, progress);
                
                // Scale decreases along trail
                float scale = MathHelper.Lerp(0.4f, 0.1f, progress);
                
                Vector2 trailPos = projectile.oldPos[i] + projectile.Size / 2f - Main.screenPosition;
                float trailRot = projectile.oldRot[i];
                
                spriteBatch.Draw(
                    _glowTexture,
                    trailPos,
                    null,
                    trailColor * alpha * 0.5f,
                    trailRot,
                    new Vector2(_glowTexture.Width / 2f, _glowTexture.Height / 2f),
                    scale,
                    SpriteEffects.None,
                    0f
                );
            }
        }
        
        private static void DrawFresnelRim(
            SpriteBatch spriteBatch,
            Vector2 drawPos,
            float rotation,
            Color fresnelColor,
            LayerConfig config,
            float time)
        {
            // Draw rim highlights at edges
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f + time * 0.02f;
                Vector2 rimOffset = angle.ToRotationVector2() * 8f;
                
                // Simulate fresnel by varying intensity based on angle
                float fresnel = MathF.Abs(MathF.Cos(angle + rotation));
                fresnel = MathF.Pow(fresnel, config.FresnelPower);
                
                spriteBatch.Draw(
                    _sparkleTexture,
                    drawPos + rimOffset,
                    null,
                    fresnelColor * config.FresnelIntensity * fresnel,
                    angle + MathHelper.PiOver2,
                    new Vector2(_sparkleTexture.Width / 2f, _sparkleTexture.Height / 2f),
                    0.2f,
                    SpriteEffects.None,
                    0f
                );
            }
        }
        
        private static void DrawOrbitingParticles(
            SpriteBatch spriteBatch,
            Vector2 center,
            Color particleColor,
            LayerConfig config,
            float time)
        {
            for (int i = 0; i < config.ParticleCount; i++)
            {
                float angle = MathHelper.TwoPi * i / config.ParticleCount + time * config.ParticleOrbitSpeed;
                
                // Elliptical orbit with slight wobble
                float wobble = MathF.Sin(time * 0.3f + i * 1.2f) * 0.3f;
                float radiusX = config.ParticleOrbitRadius * (1f + wobble * 0.5f);
                float radiusY = config.ParticleOrbitRadius * 0.6f * (1f - wobble * 0.3f);
                
                Vector2 particlePos = center + new Vector2(
                    MathF.Cos(angle) * radiusX,
                    MathF.Sin(angle) * radiusY
                );
                
                // Twinkle effect
                float twinkle = (MathF.Sin(time * 0.5f + i * 2.1f) + 1f) / 2f;
                twinkle = 0.4f + twinkle * 0.6f;
                
                spriteBatch.Draw(
                    _sparkleTexture,
                    particlePos,
                    null,
                    particleColor * twinkle * 0.7f,
                    time * 0.1f + i,
                    new Vector2(_sparkleTexture.Width / 2f, _sparkleTexture.Height / 2f),
                    0.15f * (0.8f + twinkle * 0.4f),
                    SpriteEffects.None,
                    0f
                );
            }
        }
        
        #endregion
        
        #region Utility
        
        private static Color GetGradientColor(Color[] gradient, float t)
        {
            if (gradient == null || gradient.Length == 0)
                return Color.White;
            
            if (gradient.Length == 1)
                return gradient[0];
            
            t = MathHelper.Clamp(t, 0f, 1f);
            float scaledT = t * (gradient.Length - 1);
            int index = (int)scaledT;
            int nextIndex = Math.Min(index + 1, gradient.Length - 1);
            float localT = scaledT - index;
            
            return Color.Lerp(gradient[index], gradient[nextIndex], localT);
        }
        
        private static void EnsureTextures()
        {
            if (_coreTexture == null || _coreTexture.IsDisposed)
                _coreTexture = CreateCoreTexture(16);
            
            if (_glowTexture == null || _glowTexture.IsDisposed)
                _glowTexture = CreateGlowTexture(32);
            
            if (_sparkleTexture == null || _sparkleTexture.IsDisposed)
                _sparkleTexture = CreateSparkleTexture(16);
        }
        
        #endregion
        
        #region Texture Generation
        
        private static Texture2D CreateCoreTexture(int size)
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
                    float dist = MathF.Sqrt(dx * dx + dy * dy) / center;
                    
                    // Hard-edged core
                    float alpha = MathF.Max(0f, 1f - dist);
                    alpha = alpha > 0.5f ? 1f : alpha * 2f;
                    
                    data[y * size + x] = Color.White * alpha;
                }
            }
            
            texture.SetData(data);
            return texture;
        }
        
        private static Texture2D CreateGlowTexture(int size)
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
                    float dist = MathF.Sqrt(dx * dx + dy * dy) / center;
                    
                    // Soft glow falloff
                    float alpha = MathF.Max(0f, 1f - dist);
                    alpha = MathF.Pow(alpha, 0.7f);
                    
                    data[y * size + x] = Color.White * alpha;
                }
            }
            
            texture.SetData(data);
            return texture;
        }
        
        private static Texture2D CreateSparkleTexture(int size)
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
                    
                    // 4-point star shape
                    float cross = MathF.Exp(-MathF.Abs(dx) * 0.5f) + MathF.Exp(-MathF.Abs(dy) * 0.5f);
                    cross *= 0.5f;
                    
                    // Combined with circular falloff
                    float dist = MathF.Sqrt(dx * dx + dy * dy) / center;
                    float circular = MathF.Max(0f, 1f - dist);
                    
                    float alpha = cross * circular;
                    
                    data[y * size + x] = Color.White * alpha;
                }
            }
            
            texture.SetData(data);
            return texture;
        }
        
        #endregion
        
        #region Cleanup
        
        public static void Unload()
        {
            _coreTexture?.Dispose();
            _glowTexture?.Dispose();
            _sparkleTexture?.Dispose();
            _coreTexture = null;
            _glowTexture = null;
            _sparkleTexture = null;
        }
        
        #endregion
    }
}
