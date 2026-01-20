using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using ReLogic.Content;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// Enhanced particle class using FargosSoulsDLC rendering patterns.
    /// Key improvements:
    /// - Multi-layer bloom stacking for professional glow effects
    /// - { A = 0 } pattern for proper additive blending
    /// - Theme palette gradient support
    /// - Shine flare overlays
    /// </summary>
    public class EnhancedParticle
    {
        // Position and movement
        public Vector2 Position;
        public Vector2 Velocity;
        
        // Visual properties
        public Color PrimaryColor;
        public Color SecondaryColor;
        public float Scale;
        public float Rotation;
        public float RotationSpeed;
        
        // Physics
        public float Gravity;
        public float Drag;
        public float ScaleVelocity;
        
        // Lifetime
        public int Lifetime;
        public int MaxLifetime;
        
        // Texture
        public Asset<Texture2D> Texture;
        
        // Behavior flags
        public bool FadeOut;
        public bool ScaleDown;
        public bool UseGradient;
        
        // NEW: Enhanced bloom settings
        public bool UseMultiLayerBloom;
        public int BloomLayers;
        public float BloomIntensity;
        public bool UseShineFlare;
        public float ShineFlareScale;
        public bool PulseScale;
        public float PulseSpeed;
        public float PulseAmplitude;
        
        // Theme support
        public string ThemeName;
        
        public bool IsDead => Lifetime <= 0;
        public float LifetimeProgress => 1f - (float)Lifetime / MaxLifetime;
        
        public EnhancedParticle()
        {
            Reset();
        }
        
        public void Reset()
        {
            Position = Vector2.Zero;
            Velocity = Vector2.Zero;
            PrimaryColor = Color.White;
            SecondaryColor = Color.White;
            Scale = 1f;
            Rotation = 0f;
            RotationSpeed = 0f;
            Gravity = 0f;
            Drag = 0.98f;
            ScaleVelocity = 0f;
            Lifetime = 30;
            MaxLifetime = 30;
            Texture = null;
            FadeOut = true;
            ScaleDown = false;
            UseGradient = false;
            
            // Enhanced defaults
            UseMultiLayerBloom = true;  // Enable by default for Fargos-style rendering
            BloomLayers = 4;
            BloomIntensity = 1f;
            UseShineFlare = false;
            ShineFlareScale = 0.5f;
            PulseScale = false;
            PulseSpeed = 0.1f;
            PulseAmplitude = 0.15f;
            ThemeName = null;
        }
        
        /// <summary>
        /// Setup basic particle properties
        /// </summary>
        public EnhancedParticle Setup(Asset<Texture2D> texture, Vector2 position, Vector2 velocity, 
            Color color, float scale, int lifetime, float rotationSpeed = 0f, bool fadeOut = true, bool scaleDown = false)
        {
            Texture = texture;
            Position = position;
            Velocity = velocity;
            PrimaryColor = color;
            SecondaryColor = color;
            Scale = scale;
            Lifetime = lifetime;
            MaxLifetime = lifetime;
            RotationSpeed = rotationSpeed;
            FadeOut = fadeOut;
            ScaleDown = scaleDown;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            
            return this;
        }
        
        /// <summary>
        /// Setup without explicit texture (uses RandomGlow as default)
        /// </summary>
        public EnhancedParticle Setup(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime,
            float rotationSpeed = 0f, bool fadeOut = true, bool scaleDown = false)
        {
            return Setup(CustomParticleSystem.RandomGlow(), position, velocity, color, scale, lifetime, 
                rotationSpeed, fadeOut, scaleDown);
        }
        
        // Fluent configuration methods
        public EnhancedParticle WithGradient(Color secondary)
        {
            SecondaryColor = secondary;
            UseGradient = true;
            return this;
        }
        
        public EnhancedParticle WithGravity(float gravity)
        {
            Gravity = gravity;
            return this;
        }
        
        public EnhancedParticle WithDrag(float drag)
        {
            Drag = drag;
            return this;
        }
        
        public EnhancedParticle WithScaleVelocity(float scaleVelocity)
        {
            ScaleVelocity = scaleVelocity;
            return this;
        }
        
        /// <summary>
        /// Set rotation speed
        /// </summary>
        public EnhancedParticle WithRotationSpeed(float rotSpeed)
        {
            RotationSpeed = rotSpeed;
            return this;
        }
        
        /// <summary>
        /// Enable multi-layer bloom rendering (Fargos pattern)
        /// </summary>
        public EnhancedParticle WithBloom(int layers = 4, float intensity = 1f)
        {
            UseMultiLayerBloom = true;
            BloomLayers = layers;
            BloomIntensity = intensity;
            return this;
        }
        
        /// <summary>
        /// Disable bloom for this particle
        /// </summary>
        public EnhancedParticle WithoutBloom()
        {
            UseMultiLayerBloom = false;
            return this;
        }
        
        /// <summary>
        /// Add shine flare overlay
        /// </summary>
        public EnhancedParticle WithShineFlare(float scale = 0.5f)
        {
            UseShineFlare = true;
            ShineFlareScale = scale;
            return this;
        }
        
        /// <summary>
        /// Add pulsing scale animation
        /// </summary>
        public EnhancedParticle WithPulse(float speed = 0.1f, float amplitude = 0.15f)
        {
            PulseScale = true;
            PulseSpeed = speed;
            PulseAmplitude = amplitude;
            return this;
        }
        
        /// <summary>
        /// Set theme for automatic palette gradient
        /// </summary>
        public EnhancedParticle WithTheme(string themeName)
        {
            ThemeName = themeName;
            UseGradient = true;
            return this;
        }
        
        public void Update()
        {
            Position += Velocity;
            Velocity *= Drag;
            Velocity.Y += Gravity;
            Rotation += RotationSpeed;
            Scale += ScaleVelocity;
            Lifetime--;
        }
        
        /// <summary>
        /// Draw particle with enhanced Fargos-style bloom rendering
        /// </summary>
        public void Draw(SpriteBatch spriteBatch)
        {
            if (Texture == null || !Texture.IsLoaded) return;
            if (IsDead) return;
            
            var tex = Texture.Value;
            var origin = tex.Size() / 2f;
            var drawPos = Position - Main.screenPosition;
            
            // Calculate lifetime-based effects
            float progress = LifetimeProgress;
            float alpha = FadeOut ? (1f - progress) : 1f;
            
            // Calculate scale with optional pulse
            float currentScale = Scale;
            if (ScaleDown)
                currentScale *= (1f - progress * 0.5f);
            if (PulseScale)
                currentScale *= 1f + (float)Math.Sin(Main.GameUpdateCount * PulseSpeed) * PulseAmplitude;
            
            // Get draw color - use gradient or theme palette
            Color drawColor = GetDrawColor(progress);
            
            if (UseMultiLayerBloom)
            {
                DrawWithBloom(spriteBatch, tex, drawPos, origin, drawColor, alpha, currentScale);
            }
            else
            {
                // Simple draw without bloom
                spriteBatch.Draw(tex, drawPos, null, drawColor * alpha, Rotation, origin, currentScale, SpriteEffects.None, 0f);
            }
            
            // Draw shine flare overlay if enabled
            if (UseShineFlare)
            {
                DrawShineFlare(spriteBatch, drawPos, drawColor, alpha, currentScale);
            }
        }
        
        /// <summary>
        /// Get the current draw color based on gradient or theme
        /// </summary>
        private Color GetDrawColor(float progress)
        {
            if (!string.IsNullOrEmpty(ThemeName))
            {
                return MagnumThemePalettes.GetThemeColor(ThemeName, progress);
            }
            
            if (UseGradient)
            {
                return Color.Lerp(PrimaryColor, SecondaryColor, progress);
            }
            
            return PrimaryColor;
        }
        
        /// <summary>
        /// Draw with multi-layer bloom using Fargos { A = 0 } pattern
        /// </summary>
        private void DrawWithBloom(SpriteBatch spriteBatch, Texture2D tex, Vector2 drawPos, 
            Vector2 origin, Color color, float alpha, float scale)
        {
            // CRITICAL: Remove alpha channel for additive blending
            // This is the key Fargos pattern
            Color bloomColor = color.WithoutAlpha();
            
            // Bloom layer scales and opacities (from FargosSoulsDLC patterns)
            float[] scales = { 2.0f, 1.4f, 0.9f, 0.4f };
            float[] opacities = { 0.3f, 0.5f, 0.7f, 0.85f };
            
            int layerCount = Math.Min(BloomLayers, scales.Length);
            
            // Draw bloom layers from largest (background) to smallest (core)
            for (int i = 0; i < layerCount; i++)
            {
                float layerScale = scale * scales[i];
                float layerAlpha = alpha * opacities[i] * BloomIntensity;
                
                // Apply slight color shift per layer for depth
                Color layerColor = bloomColor;
                if (i == 0) // Outermost layer - slightly dimmer
                    layerColor = Color.Lerp(bloomColor, Color.Black, 0.3f);
                else if (i == layerCount - 1) // Innermost layer - brighter core
                    layerColor = Color.Lerp(bloomColor, Color.White, 0.2f);
                
                spriteBatch.Draw(tex, drawPos, null, layerColor * layerAlpha, 
                    Rotation, origin, layerScale, SpriteEffects.None, 0f);
            }
        }
        
        /// <summary>
        /// Draw shine flare overlay for extra sparkle
        /// </summary>
        private void DrawShineFlare(SpriteBatch spriteBatch, Vector2 drawPos, Color color, float alpha, float scale)
        {
            // Try to get shine flare texture
            var shineTexture = MagnumTextureRegistry.GetShineFlare4Point();
            if (shineTexture == null) return;
            
            // Rotate shine flare for sparkle effect
            float shineRotation = Main.GameUpdateCount * 0.05f;
            
            // Draw with bloom color (no alpha)
            Color shineColor = color.WithoutAlpha() * alpha * 0.6f;
            var shineOrigin = shineTexture.Size() / 2f;
            
            spriteBatch.Draw(shineTexture, drawPos, null, shineColor, 
                shineRotation, shineOrigin, scale * ShineFlareScale, SpriteEffects.None, 0f);
        }
    }
    
    /// <summary>
    /// Pool and manager for EnhancedParticles
    /// </summary>
    public class EnhancedParticlePool : ModSystem
    {
        private static System.Collections.Generic.List<EnhancedParticle> activeParticles = new(500);
        private static System.Collections.Generic.Queue<EnhancedParticle> particlePool = new(200);
        
        public static int MaxParticles = 800;
        
        public override void Unload()
        {
            activeParticles?.Clear();
            particlePool?.Clear();
            activeParticles = null;
            particlePool = null;
        }
        
        public override void PostUpdateEverything()
        {
            // Update all particles
            for (int i = activeParticles.Count - 1; i >= 0; i--)
            {
                activeParticles[i].Update();
                if (activeParticles[i].IsDead)
                {
                    // Return to pool
                    activeParticles[i].Reset();
                    particlePool.Enqueue(activeParticles[i]);
                    activeParticles.RemoveAt(i);
                }
            }
        }
        
        /// <summary>
        /// Get a particle from the pool or create new
        /// </summary>
        public static EnhancedParticle GetParticle()
        {
            if (particlePool.Count > 0)
            {
                return particlePool.Dequeue();
            }
            return new EnhancedParticle();
        }
        
        /// <summary>
        /// Spawn a particle into the active list
        /// </summary>
        public static void SpawnParticle(EnhancedParticle particle)
        {
            if (activeParticles.Count < MaxParticles)
            {
                activeParticles.Add(particle);
            }
        }
        
        /// <summary>
        /// Draw all enhanced particles with bloom
        /// </summary>
        public static void DrawAllParticles(SpriteBatch spriteBatch)
        {
            if (activeParticles.Count == 0) return;
            
            // Switch to additive blending for bloom
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            foreach (var particle in activeParticles)
            {
                particle.Draw(spriteBatch);
            }
            
            // Restore normal blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        
        /// <summary>
        /// Get count of active particles
        /// </summary>
        public static int ActiveCount => activeParticles.Count;
    }
    
    /// <summary>
    /// Quick helpers for spawning enhanced bloom particles
    /// </summary>
    public static class EnhancedParticles
    {
        #region Generic Effects
        
        /// <summary>
        /// Spawn a bloom flare with multi-layer rendering
        /// </summary>
        public static void BloomFlare(Vector2 position, Color color, float scale = 0.5f, int lifetime = 20, 
            int bloomLayers = 4, float intensity = 1f)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            
            var particle = EnhancedParticlePool.GetParticle()
                .Setup(CustomParticleSystem.RandomFlare(), position, Vector2.Zero, color, scale, lifetime)
                .WithBloom(bloomLayers, intensity);
            
            EnhancedParticlePool.SpawnParticle(particle);
        }
        
        /// <summary>
        /// Spawn multiple bloom flares in a radial burst
        /// </summary>
        public static void BloomBurst(Vector2 position, Color primaryColor, Color secondaryColor, 
            int count = 8, float speed = 4f, float scale = 0.3f, int lifetime = 25)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            
            // Central bright core
            BloomFlare(position, Color.White, scale * 1.5f, lifetime, 4, 1.2f);
            
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count;
                Vector2 velocity = angle.ToRotationVector2() * (speed + Main.rand.NextFloat(speed * 0.5f));
                
                float progress = (float)i / count;
                Color particleColor = Color.Lerp(primaryColor, secondaryColor, progress);
                
                var particle = EnhancedParticlePool.GetParticle()
                    .Setup(CustomParticleSystem.RandomFlare(), position, velocity, particleColor, 
                        scale + Main.rand.NextFloat(scale * 0.3f), lifetime + Main.rand.Next(10),
                        Main.rand.NextFloat(-0.05f, 0.05f), true, true)
                    .WithBloom(3, 0.8f)
                    .WithDrag(0.96f);
                
                EnhancedParticlePool.SpawnParticle(particle);
            }
        }
        
        /// <summary>
        /// Spawn a shine flare (star sparkle)
        /// </summary>
        public static void ShineFlare(Vector2 position, Color color, float scale = 0.4f, int lifetime = 15)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            
            var particle = EnhancedParticlePool.GetParticle()
                .Setup(CustomParticleSystem.RandomFlare(), position, Vector2.Zero, color, scale, lifetime)
                .WithBloom(2, 0.7f)
                .WithShineFlare(0.6f);
            
            EnhancedParticlePool.SpawnParticle(particle);
        }
        
        /// <summary>
        /// Spawn pulsing aura particles
        /// </summary>
        public static void PulsingAura(Vector2 position, Color color, int count = 5, float radius = 30f)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count + Main.GameUpdateCount * 0.02f;
                Vector2 offset = angle.ToRotationVector2() * radius;
                
                var particle = EnhancedParticlePool.GetParticle()
                    .Setup(CustomParticleSystem.RandomGlow(), position + offset, Vector2.Zero, 
                        color, 0.3f, 20)
                    .WithBloom(3, 0.6f)
                    .WithPulse(0.15f, 0.2f);
                
                EnhancedParticlePool.SpawnParticle(particle);
            }
        }
        
        #endregion
        
        #region Theme-Specific Effects
        
        /// <summary>
        /// Spawn themed bloom burst using theme palettes
        /// </summary>
        public static void ThemedBloomBurst(Vector2 position, string themeName, int count = 8, 
            float speed = 4f, float scale = 0.3f, int lifetime = 25)
        {
            Color primary = MagnumThemePalettes.GetThemePrimary(themeName);
            Color secondary = MagnumThemePalettes.GetThemeSecondary(themeName);
            
            BloomBurst(position, primary, secondary, count, speed, scale, lifetime);
        }
        
        /// <summary>
        /// Eroica themed bloom impact
        /// </summary>
        public static void EroicaBloomImpact(Vector2 position, float scale = 1f)
        {
            ThemedBloomBurst(position, "Eroica", 10, 5f * scale, 0.35f * scale, 28);
            
            // Add sakura accent particles
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(20f, 20f);
                var particle = EnhancedParticlePool.GetParticle()
                    .Setup(CustomParticleSystem.RandomGlow(), position + offset, 
                        Main.rand.NextVector2Circular(2f, 2f) + new Vector2(0, -1f),
                        new Color(255, 180, 200), 0.25f, 35)
                    .WithBloom(2, 0.5f)
                    .WithGravity(0.02f)
                    .WithDrag(0.98f);
                
                EnhancedParticlePool.SpawnParticle(particle);
            }
        }
        
        /// <summary>
        /// Moonlight Sonata themed bloom impact
        /// </summary>
        public static void MoonlightBloomImpact(Vector2 position, float scale = 1f)
        {
            ThemedBloomBurst(position, "MoonlightSonata", 8, 3.5f * scale, 0.3f * scale, 32);
            
            // Add silver mist particles
            for (int i = 0; i < 5; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(25f, 25f);
                var particle = EnhancedParticlePool.GetParticle()
                    .Setup(CustomParticleSystem.RandomGlow(), position + offset,
                        Main.rand.NextVector2Circular(1f, 1f) + new Vector2(0, -0.5f),
                        new Color(220, 215, 255), 0.2f, 40)
                    .WithBloom(3, 0.4f)
                    .WithGravity(-0.01f)
                    .WithDrag(0.99f);
                
                EnhancedParticlePool.SpawnParticle(particle);
            }
        }
        
        /// <summary>
        /// La Campanella themed bloom impact (fiery bell)
        /// </summary>
        public static void LaCampanellaBloomImpact(Vector2 position, float scale = 1f)
        {
            ThemedBloomBurst(position, "LaCampanella", 12, 6f * scale, 0.4f * scale, 24);
            
            // Add smoke particles
            for (int i = 0; i < 6; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(15f, 15f);
                var particle = EnhancedParticlePool.GetParticle()
                    .Setup(CustomParticleSystem.RandomGlow(), position + offset,
                        Main.rand.NextVector2Circular(1.5f, 1.5f) + new Vector2(0, -2f),
                        new Color(40, 30, 30), 0.35f, 35)
                    .WithBloom(2, 0.3f)
                    .WithGravity(-0.03f)
                    .WithDrag(0.97f)
                    .WithScaleVelocity(0.01f);
                
                EnhancedParticlePool.SpawnParticle(particle);
            }
        }
        
        /// <summary>
        /// Fate themed bloom impact (cosmic/celestial)
        /// </summary>
        public static void FateBloomImpact(Vector2 position, float scale = 1f)
        {
            ThemedBloomBurst(position, "Fate", 10, 5f * scale, 0.35f * scale, 30);
            
            // Add cosmic star sparkles
            for (int i = 0; i < 6; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(30f, 30f);
                
                var particle = EnhancedParticlePool.GetParticle()
                    .Setup(CustomParticleSystem.RandomFlare(), position + offset,
                        Vector2.Zero, Color.White, 0.2f, 20)
                    .WithBloom(2, 0.9f)
                    .WithShineFlare(0.4f);
                
                EnhancedParticlePool.SpawnParticle(particle);
            }
        }
        
        /// <summary>
        /// Swan Lake themed bloom impact (elegant feathers)
        /// </summary>
        public static void SwanLakeBloomImpact(Vector2 position, float scale = 1f)
        {
            ThemedBloomBurst(position, "SwanLake", 8, 3f * scale, 0.3f * scale, 35);
            
            // Add rainbow shimmer
            for (int i = 0; i < 4; i++)
            {
                float hue = (Main.GameUpdateCount * 0.02f + i * 0.25f) % 1f;
                Color rainbowColor = Main.hslToRgb(hue, 0.8f, 0.8f);
                
                Vector2 offset = Main.rand.NextVector2Circular(20f, 20f);
                var particle = EnhancedParticlePool.GetParticle()
                    .Setup(CustomParticleSystem.RandomGlow(), position + offset,
                        Main.rand.NextVector2Circular(1f, 1f), rainbowColor, 0.2f, 30)
                    .WithBloom(2, 0.5f)
                    .WithPulse(0.2f, 0.1f);
                
                EnhancedParticlePool.SpawnParticle(particle);
            }
        }
        
        /// <summary>
        /// Enigma Variations themed bloom impact (mysterious)
        /// </summary>
        public static void EnigmaBloomImpact(Vector2 position, float scale = 1f)
        {
            ThemedBloomBurst(position, "EnigmaVariations", 10, 4f * scale, 0.35f * scale, 28);
            
            // Add green flame wisps
            for (int i = 0; i < 5; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(25f, 25f);
                var particle = EnhancedParticlePool.GetParticle()
                    .Setup(CustomParticleSystem.RandomGlow(), position + offset,
                        Main.rand.NextVector2Circular(1.5f, 1.5f) + new Vector2(0, -1.5f),
                        new Color(50, 220, 100), 0.25f, 32)
                    .WithBloom(3, 0.6f)
                    .WithGravity(-0.02f)
                    .WithDrag(0.98f);
                
                EnhancedParticlePool.SpawnParticle(particle);
            }
        }
        
        #endregion
    }
}
