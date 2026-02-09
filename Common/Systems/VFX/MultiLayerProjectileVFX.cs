using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// CALAMITY-STYLE MULTI-LAYER PROJECTILE SYSTEM
    /// 
    /// Implements the 5-layer projectile rendering framework:
    /// 
    /// Layer 1: INVISIBLE LOGIC CORE
    ///   - The actual projectile hitbox (can be invisible)
    ///   - Handles collision, AI, homing logic
    ///   - extraUpdates = 2-3 for buttery 144Hz+ movement
    /// 
    /// Layer 2: PRIMITIVE LEAD TRAIL
    ///   - Vertex strip trail behind the projectile
    ///   - Uses PrimitiveTrailRenderer for Bézier smoothing
    ///   - Width tapering with QuadraticBump
    /// 
    /// Layer 3: BLOOM GLOW
    ///   - Multi-layer additive glow around projectile
    ///   - Uses { A = 0 } pattern for proper additive blending
    ///   - Pulsing scale animation
    /// 
    /// Layer 4: PARTICLE EXHAUST
    ///   - Dense dust/particle trail
    ///   - Vanilla Dust + Custom particles combined
    ///   - Contrasting sparkles for visual pop
    /// 
    /// Layer 5: HIGH-CONTRAST CORE
    ///   - Bright white/color center sprite
    ///   - Small scale, high opacity
    ///   - Spinning rotation for dynamism
    /// 
    /// Usage:
    /// 1. In SetDefaults: Projectile.extraUpdates = 2;
    /// 2. In AI: Call MultiLayerProjectileVFX.UpdateTrail()
    /// 3. In PreDraw: return MultiLayerProjectileVFX.RenderLayers()
    /// </summary>
    public static class MultiLayerProjectileVFX
    {
        #region Layer Configuration
        
        /// <summary>
        /// Configuration for multi-layer projectile rendering
        /// </summary>
        public class LayerConfig
        {
            // Colors
            public Color PrimaryColor = Color.White;
            public Color SecondaryColor = Color.White;
            public Color BloomColor = Color.White;
            
            // Trail settings
            public float TrailWidth = 20f;
            public int TrailLength = 15;
            public bool UseTrail = true;
            
            // Bloom settings
            public float BloomScale = 0.5f;
            public int BloomLayers = 4;
            public float BloomIntensity = 1f;
            public bool UseBloom = true;
            
            // Particle settings
            public int ParticleDustType = DustID.MagicMirror;
            public float ParticleScale = 1.2f;
            public int ParticlesPerFrame = 2;
            public bool UseParticles = true;
            
            // Core settings
            public float CoreScale = 0.3f;
            public float CoreRotationSpeed = 0.1f;
            public bool UseCore = true;
            
            // Homing settings (40:1 ratio)
            public float HomingStrength = 0.05f;
            public float MaxHomingAngle = 0.1f;
            
            // Theme
            public string Theme = "";
        }
        
        #endregion
        
        #region Preset Configurations
        
        /// <summary>
        /// Get preset configuration for each theme
        /// </summary>
        public static class Presets
        {
            public static LayerConfig LaCampanella() => new LayerConfig
            {
                PrimaryColor = new Color(255, 140, 40),
                SecondaryColor = new Color(30, 20, 25),
                BloomColor = new Color(255, 100, 0),
                TrailWidth = 25f,
                ParticleDustType = DustID.Torch,
                Theme = "LaCampanella"
            };
            
            public static LayerConfig Eroica() => new LayerConfig
            {
                PrimaryColor = new Color(200, 50, 50),
                SecondaryColor = new Color(255, 200, 80),
                BloomColor = new Color(255, 150, 50),
                TrailWidth = 22f,
                ParticleDustType = DustID.Enchanted_Gold,
                Theme = "Eroica"
            };
            
            public static LayerConfig SwanLake() => new LayerConfig
            {
                PrimaryColor = Color.White,
                SecondaryColor = new Color(30, 30, 40),
                BloomColor = new Color(220, 220, 255),
                TrailWidth = 20f,
                ParticleDustType = DustID.PinkFairy,
                Theme = "SwanLake"
            };
            
            public static LayerConfig MoonlightSonata() => new LayerConfig
            {
                PrimaryColor = new Color(75, 0, 130),
                SecondaryColor = new Color(135, 206, 250),
                BloomColor = new Color(140, 100, 200),
                TrailWidth = 18f,
                ParticleDustType = DustID.PurpleTorch,
                Theme = "MoonlightSonata"
            };
            
            public static LayerConfig EnigmaVariations() => new LayerConfig
            {
                PrimaryColor = new Color(140, 60, 200),
                SecondaryColor = new Color(50, 220, 100),
                BloomColor = new Color(100, 140, 150),
                TrailWidth = 24f,
                ParticleDustType = DustID.Vortex,
                Theme = "EnigmaVariations"
            };
            
            public static LayerConfig Fate() => new LayerConfig
            {
                PrimaryColor = new Color(180, 50, 100),
                SecondaryColor = new Color(255, 255, 255),
                BloomColor = new Color(200, 80, 120),
                TrailWidth = 28f,
                BloomLayers = 5,
                BloomIntensity = 1.2f,
                ParticleDustType = DustID.RainbowMk2,
                Theme = "Fate"
            };
        }
        
        #endregion
        
        #region Layer 1: Logic Core (AI Helper)
        
        /// <summary>
        /// Apply smooth homing with Calamity's 40:1 vector steering ratio
        /// Call in AI()
        /// </summary>
        public static void ApplySmoothHoming(Projectile projectile, Vector2 targetPosition, LayerConfig config)
        {
            if (projectile.velocity.Length() < 0.1f)
                return;
            
            Vector2 toTarget = targetPosition - projectile.Center;
            float targetAngle = toTarget.ToRotation();
            float currentAngle = projectile.velocity.ToRotation();
            
            // Calculate angle difference
            float angleDiff = MathHelper.WrapAngle(targetAngle - currentAngle);
            
            // Clamp to max homing angle
            angleDiff = MathHelper.Clamp(angleDiff, -config.MaxHomingAngle, config.MaxHomingAngle);
            
            // Apply with 40:1 ratio (0.025f lerp factor)
            float newAngle = currentAngle + angleDiff * config.HomingStrength;
            float speed = projectile.velocity.Length();
            
            projectile.velocity = new Vector2((float)Math.Cos(newAngle), (float)Math.Sin(newAngle)) * speed;
            projectile.rotation = projectile.velocity.ToRotation();
        }
        
        #endregion
        
        #region Layer 2: Primitive Trail
        
        /// <summary>
        /// Update trail tracking (call in AI every frame)
        /// </summary>
        public static void UpdateTrail(Projectile projectile, LayerConfig config)
        {
            if (!config.UseTrail)
                return;
            
            PrimitiveTrailRenderer.TrackPosition(
                projectile.whoAmI,
                projectile.Center,
                projectile.rotation,
                config.PrimaryColor,
                config.SecondaryColor,
                config.TrailWidth
            );
        }
        
        #endregion
        
        #region Layer 4: Particle Exhaust
        
        /// <summary>
        /// Spawn dense particle exhaust (call in AI every frame)
        /// </summary>
        public static void SpawnParticleExhaust(Projectile projectile, LayerConfig config)
        {
            if (!config.UseParticles)
                return;
            
            // Dense dust trail - 2+ per frame
            for (int i = 0; i < config.ParticlesPerFrame; i++)
            {
                Vector2 dustPos = projectile.Center + Main.rand.NextVector2Circular(6f, 6f);
                Vector2 dustVel = -projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1.5f, 1.5f);
                
                Dust dust = Dust.NewDustPerfect(dustPos, config.ParticleDustType, dustVel, 0, config.PrimaryColor, config.ParticleScale);
                dust.noGravity = true;
                dust.fadeIn = 1.2f;
            }
            
            // Contrasting sparkle (1 in 2)
            if (Main.rand.NextBool(2))
            {
                Dust sparkle = Dust.NewDustPerfect(
                    projectile.Center,
                    DustID.WhiteTorch,
                    -projectile.velocity * 0.1f,
                    0,
                    Color.White,
                    0.8f
                );
                sparkle.noGravity = true;
            }
            
            // Color oscillation (1 in 3)
            if (Main.rand.NextBool(3))
            {
                float hue = (Main.GlobalTimeWrappedHourly * 0.5f + Main.rand.NextFloat(0.1f)) % 1f;
                Color shiftedColor = Main.hslToRgb(hue, 0.9f, 0.75f);
                
                Dust colorDust = Dust.NewDustPerfect(
                    projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                    DustID.RainbowMk2,
                    -projectile.velocity * 0.05f,
                    0,
                    shiftedColor,
                    0.6f
                );
                colorDust.noGravity = true;
            }
        }
        
        #endregion
        
        #region Combined Render (PreDraw)
        
        /// <summary>
        /// Render all visual layers in PreDraw
        /// Returns false to skip default drawing
        /// </summary>
        public static bool RenderLayers(Projectile projectile, SpriteBatch spriteBatch, 
            Color lightColor, LayerConfig config)
        {
            // === LAYER 2: PRIMITIVE TRAIL ===
            if (config.UseTrail)
            {
                PrimitiveTrailRenderer.RenderTrailCustom(
                    projectile.whoAmI,
                    spriteBatch,
                    PrimitiveTrailRenderer.SwingWidthFunction(config.TrailWidth),
                    PrimitiveTrailRenderer.GradientColorFunction(config.PrimaryColor, config.SecondaryColor)
                );
            }
            
            // === LAYER 3: BLOOM GLOW ===
            if (config.UseBloom)
            {
                RenderBloomLayers(projectile, spriteBatch, config);
            }
            
            // === LAYER 5: HIGH-CONTRAST CORE ===
            if (config.UseCore)
            {
                RenderCore(projectile, spriteBatch, config, lightColor);
            }
            
            return false; // Skip default drawing
        }
        
        /// <summary>
        /// Render multi-layer bloom glow
        /// </summary>
        private static void RenderBloomLayers(Projectile projectile, SpriteBatch spriteBatch, LayerConfig config)
        {
            Texture2D glowTex = GetBloomTexture();
            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Vector2 origin = glowTex.Size() * 0.5f;
            
            // Pulsing animation
            float pulse = 1f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 5f) * 0.15f;
            
            // End current batch for additive blending
            try { spriteBatch.End(); } catch { }
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);
            
            // Multi-layer bloom (Fargos pattern)
            float[] scales = { 2.0f, 1.4f, 0.9f, 0.4f };
            float[] opacities = { 0.3f, 0.5f, 0.7f, 0.9f };
            
            int layers = Math.Min(config.BloomLayers, scales.Length);
            for (int i = 0; i < layers; i++)
            {
                float scale = config.BloomScale * scales[i] * pulse;
                float opacity = opacities[i] * config.BloomIntensity;
                
                // Use { A = 0 } pattern for proper additive
                Color bloomColor = config.BloomColor with { A = 0 } * opacity;
                
                spriteBatch.Draw(glowTex, drawPos, null, bloomColor, 0f, origin, scale, SpriteEffects.None, 0f);
            }
            
            // Restore normal blending
            try { spriteBatch.End(); } catch { }
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);
        }
        
        /// <summary>
        /// Render high-contrast core
        /// </summary>
        private static void RenderCore(Projectile projectile, SpriteBatch spriteBatch, 
            LayerConfig config, Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[projectile.type].Value;
            if (texture == null)
                return;
            
            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRect = new Rectangle(0, 0, texture.Width, texture.Height);
            Vector2 origin = texture.Size() * 0.5f;
            
            // Spinning rotation
            float rotation = projectile.rotation + Main.GlobalTimeWrappedHourly * config.CoreRotationSpeed;
            
            // Core with white highlight
            spriteBatch.Draw(texture, drawPos, sourceRect, Color.White, rotation, 
                origin, config.CoreScale * 1.2f, SpriteEffects.None, 0f);
            
            // Tinted overlay
            spriteBatch.Draw(texture, drawPos, sourceRect, config.PrimaryColor * 0.8f, rotation,
                origin, config.CoreScale, SpriteEffects.None, 0f);
        }
        
        #endregion
        
        #region Texture Helpers
        
        private static Texture2D _bloomTexture;
        private static Texture2D GetBloomTexture()
        {
            if (_bloomTexture != null && !_bloomTexture.IsDisposed)
                return _bloomTexture;
            
            // Create radial gradient texture
            int size = 64;
            _bloomTexture = new Texture2D(Main.graphics.GraphicsDevice, size, size);
            Color[] data = new Color[size * size];
            
            Vector2 center = new Vector2(size / 2f, size / 2f);
            float maxDist = center.Length();
            
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center) / maxDist;
                    
                    // Smooth radial falloff
                    float alpha = 1f - dist * dist;
                    alpha = MathHelper.Clamp(alpha, 0f, 1f);
                    
                    data[y * size + x] = Color.White * alpha;
                }
            }
            
            _bloomTexture.SetData(data);
            return _bloomTexture;
        }
        
        #endregion
        
        #region Integration Helpers
        
        /// <summary>
        /// Full AI update with all VFX (call this single method in AI)
        /// </summary>
        public static void FullAIUpdate(Projectile projectile, LayerConfig config, NPC target = null)
        {
            // Apply homing if target exists
            if (target != null && target.active)
            {
                ApplySmoothHoming(projectile, target.Center, config);
            }
            
            // Update trail
            UpdateTrail(projectile, config);
            
            // Spawn particles
            SpawnParticleExhaust(projectile, config);
            
            // Add dynamic lighting
            Lighting.AddLight(projectile.Center, config.BloomColor.ToVector3() * 0.8f);
        }
        
        /// <summary>
        /// Cleanup on projectile kill
        /// </summary>
        public static void OnKill(Projectile projectile)
        {
            PrimitiveTrailRenderer.ClearTrail(projectile.whoAmI);
        }
        
        #endregion
    }
}
