using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Common.Systems
{
    // ============================================================================
    // SCREEN EFFECT SYSTEM - Ported from InfernumMode patterns
    // Provides screen shake, flash, blur, and shockwave effects for dramatic moments.
    // ============================================================================

    /// <summary>
    /// Central system for managing screen effects like flash, blur, and shockwaves.
    /// Inspired by Infernum's ScreenEffectSystem.
    /// </summary>
    public class MagnumScreenEffects : ModSystem
    {
        // ==================== FLASH EFFECT ====================
        private static Vector2 FlashPosition;
        private static float FlashIntensity;
        private static int FlashLifetime;
        private static int FlashTime;
        private static bool FlashActive;
        
        // ==================== BLUR EFFECT ====================
        private static Vector2 BlurPosition;
        private static float BlurIntensity;
        private static int BlurLifetime;
        private static int BlurTime;
        private static bool BlurActive;
        
        // ==================== SCREEN SHAKE ====================
        private static float CurrentScreenShakePower;
        private static float ScreenShakeDecay = 0.2f;
        
        /// <summary>
        /// Creates a flash effect at the specified position.
        /// </summary>
        /// <param name="position">World position for the flash center</param>
        /// <param name="intensity">Flash brightness (0-2 recommended)</param>
        /// <param name="lifetime">Duration in frames</param>
        public static void SetFlashEffect(Vector2 position, float intensity, int lifetime)
        {
            FlashPosition = position;
            FlashIntensity = intensity;
            FlashLifetime = lifetime;
            FlashTime = 0;
            FlashActive = true;
        }
        
        /// <summary>
        /// Creates a blur effect emanating from the specified position.
        /// </summary>
        /// <param name="position">World position for the blur center</param>
        /// <param name="intensity">Blur strength (0-2 recommended)</param>
        /// <param name="lifetime">Duration in frames</param>
        public static void SetBlurEffect(Vector2 position, float intensity, int lifetime)
        {
            BlurPosition = position;
            BlurIntensity = intensity;
            BlurLifetime = lifetime;
            BlurTime = 0;
            BlurActive = true;
        }
        
        /// <summary>
        /// Adds screen shake that decays over time.
        /// </summary>
        /// <param name="power">Initial shake power</param>
        public static void AddScreenShake(float power)
        {
            CurrentScreenShakePower = Math.Max(CurrentScreenShakePower, power);
        }
        
        /// <summary>
        /// Sets screen shake to a specific value (won't decrease if already higher).
        /// </summary>
        public static void SetScreenShake(float power)
        {
            CurrentScreenShakePower = Math.Max(CurrentScreenShakePower, power);
        }
        
        /// <summary>
        /// Gets the current screen shake power.
        /// </summary>
        public static float GetScreenShakePower() => CurrentScreenShakePower;
        
        /// <summary>
        /// Checks if any blur or flash effect is currently active.
        /// </summary>
        public static bool AnyEffectActive() => FlashActive || BlurActive;
        
        public override void PostUpdateEverything()
        {
            // Update flash
            if (FlashActive)
            {
                FlashTime++;
                if (FlashTime >= FlashLifetime)
                    FlashActive = false;
            }
            
            // Update blur
            if (BlurActive)
            {
                BlurTime++;
                if (BlurTime >= BlurLifetime)
                    BlurActive = false;
            }
            
            // Decay screen shake
            if (CurrentScreenShakePower > 0f)
                CurrentScreenShakePower = Math.Max(0f, CurrentScreenShakePower - ScreenShakeDecay);
        }
        
        public override void ModifyScreenPosition()
        {
            // Apply screen shake
            if (CurrentScreenShakePower > 0f)
            {
                float shakeX = Main.rand.NextFloat(-CurrentScreenShakePower, CurrentScreenShakePower);
                float shakeY = Main.rand.NextFloat(-CurrentScreenShakePower, CurrentScreenShakePower);
                Main.screenPosition += new Vector2(shakeX, shakeY);
            }
        }
        
        /// <summary>
        /// Call this in your ModSystem's PostDrawTiles or similar to render effects.
        /// </summary>
        public static void DrawEffects(SpriteBatch spriteBatch)
        {
            if (!FlashActive && !BlurActive)
                return;
            
            // Draw flash effect
            if (FlashActive)
            {
                float progress = (float)FlashTime / FlashLifetime;
                float opacity = (1f - progress) * FlashIntensity;
                
                // Simple white flash overlay centered on position
                Vector2 screenPos = FlashPosition - Main.screenPosition;
                float radius = 500f * (1f + progress);
                
                // Draw radial gradient flash
                for (int i = 0; i < 3; i++)
                {
                    float layerOpacity = opacity * (1f - i * 0.25f);
                    float layerRadius = radius * (1f + i * 0.3f);
                    
                    // We'd need a proper texture for this - for now, add light
                    Lighting.AddLight(FlashPosition, layerOpacity, layerOpacity, layerOpacity);
                }
            }
        }
    }
    
    // ============================================================================
    // SHOCKWAVE UTILITY - Creates visual shockwave effects
    // ============================================================================
    
    /// <summary>
    /// Utility class for creating shockwave effects.
    /// Call CreateShockwave to spawn a visual shockwave at a position.
    /// </summary>
    public static class ShockwaveUtility
    {
        /// <summary>
        /// Creates a visual shockwave effect using dust and particles.
        /// </summary>
        /// <param name="position">Center position of the shockwave</param>
        /// <param name="rippleCount">Number of dust rings</param>
        /// <param name="rippleSize">Size of dust particles</param>
        /// <param name="rippleSpeed">Expansion speed</param>
        /// <param name="color">Optional color tint</param>
        public static void CreateShockwave(Vector2 position, int rippleCount = 2, int rippleSize = 8, float rippleSpeed = 75f, Color? color = null)
        {
            Color dustColor = color ?? Color.White;
            
            // Create expanding dust rings
            for (int ring = 0; ring < rippleCount; ring++)
            {
                float ringRadius = (ring + 1) * rippleSpeed * 0.3f;
                int dustCount = 12 + ring * 6;
                
                for (int i = 0; i < dustCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / dustCount;
                    Vector2 dustPos = position + angle.ToRotationVector2() * ringRadius;
                    Vector2 dustVel = angle.ToRotationVector2() * rippleSpeed * 0.1f;
                    
                    Dust dust = Dust.NewDustPerfect(dustPos, DustID.WhiteTorch, dustVel, 0, dustColor, rippleSize * 0.2f);
                    dust.noGravity = true;
                    dust.fadeIn = 1.2f;
                }
            }
            
            // Add screen shake
            MagnumScreenEffects.AddScreenShake(rippleCount * 2f);
            
            // Spawn pulse ring particle if available
            try
            {
                var pulseRing = new PulseRingParticle(position, Vector2.Zero, dustColor, 0.5f, 3f, 30);
                MagnumParticleHandler.SpawnParticle(pulseRing);
            }
            catch { }
        }
        
        /// <summary>
        /// Creates a themed shockwave with specific color palette.
        /// </summary>
        public static void CreateThemedShockwave(Vector2 position, Color primaryColor, Color secondaryColor, float scale = 1f)
        {
            // Primary pulse ring
            try
            {
                var ring1 = new PulseRingParticle(position, Vector2.Zero, primaryColor, 0.3f * scale, 2.5f * scale, 25);
                var ring2 = new PulseRingParticle(position, Vector2.Zero, secondaryColor, 0.5f * scale, 3f * scale, 35);
                var bloom = new StrongBloomParticle(position, Vector2.Zero, primaryColor * 0.6f, 2f * scale, 20);
                
                MagnumParticleHandler.SpawnParticle(ring1);
                MagnumParticleHandler.SpawnParticle(ring2);
                MagnumParticleHandler.SpawnParticle(bloom);
            }
            catch { }
            
            // Dust burst
            for (int i = 0; i < 24; i++)
            {
                float angle = MathHelper.TwoPi * i / 24f;
                Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 8f) * scale;
                Color dustColor = Main.rand.NextBool() ? primaryColor : secondaryColor;
                
                Dust dust = Dust.NewDustPerfect(position, DustID.RainbowMk2, velocity, 0, dustColor, 1.5f * scale);
                dust.noGravity = true;
            }
            
            MagnumScreenEffects.AddScreenShake(5f * scale);
        }
    }
    
    // ============================================================================
    // EXPLOSION UTILITY - Creates layered explosion effects
    // ============================================================================
    
    /// <summary>
    /// Utility class for creating explosion effects with multiple particle layers.
    /// Inspired by Infernum's explosion patterns.
    /// </summary>
    public static class ExplosionUtility
    {
        /// <summary>
        /// Creates a fire explosion with smoke, sparks, and bloom.
        /// </summary>
        public static void CreateFireExplosion(Vector2 position, Color primaryColor, Color secondaryColor, float scale = 1f)
        {
            // Light particles
            for (int i = 0; i < 20; i++)
            {
                Vector2 pos = position + Main.rand.NextVector2Circular(20f * scale, 20f * scale);
                Vector2 velocity = position.DirectionTo(pos) * Main.rand.NextFloat(3f, 5f);
                Color lightColor = Main.rand.NextBool() ? primaryColor : secondaryColor;
                
                try
                {
                    var light = new Particles.GenericGlowParticle(pos, velocity, lightColor, Main.rand.NextFloat(0.5f, 0.8f) * scale, 60);
                    Particles.MagnumParticleHandler.SpawnParticle(light);
                }
                catch { }
            }
            
            // Smoke clouds
            for (int i = 0; i < 15; i++)
            {
                Vector2 pos = position + Main.rand.NextVector2Circular(40f * scale, 40f * scale);
                Vector2 velocity = Main.rand.NextVector2Circular(2f, 2f);
                
                try
                {
                    var smoke = new CloudSmokeParticle(pos, velocity, primaryColor, Color.DarkGray, 45, Main.rand.NextFloat(1f, 1.5f) * scale);
                    Particles.MagnumParticleHandler.SpawnParticle(smoke);
                }
                catch { }
            }
            
            // Sparks
            for (int i = 0; i < 30; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(5f, 15f) * scale;
                Color sparkColor = Main.rand.NextBool() ? primaryColor : secondaryColor;
                
                try
                {
                    var spark = new DirectionalSparkParticle(position, velocity, false, 30, Main.rand.NextFloat(1.5f, 2.5f) * scale, sparkColor);
                    Particles.MagnumParticleHandler.SpawnParticle(spark);
                }
                catch { }
            }
            
            // Central bloom
            try
            {
                var bloom = new StrongBloomParticle(position, Vector2.Zero, primaryColor * 0.6f, 3f * scale, 25);
                Particles.MagnumParticleHandler.SpawnParticle(bloom);
            }
            catch { }
            
            // Dust fallback
            for (int i = 0; i < 40; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(3f, 10f) * scale;
                Dust dust = Dust.NewDustPerfect(position, DustID.Torch, velocity, 100, primaryColor, 2f * scale);
                dust.noGravity = true;
            }
            
            MagnumScreenEffects.AddScreenShake(8f * scale);
            MagnumScreenEffects.SetFlashEffect(position, 1f * scale, 20);
        }
        
        /// <summary>
        /// Creates an energy explosion with electric arcs and pulse rings.
        /// </summary>
        public static void CreateEnergyExplosion(Vector2 position, Color primaryColor, float scale = 1f)
        {
            // Electric explosion ring
            try
            {
                var ring = new PulseRingParticle(position, Vector2.Zero, primaryColor, 0f, 4f * scale, 40);
                Particles.MagnumParticleHandler.SpawnParticle(ring);
            }
            catch { }
            
            // Electric arcs
            for (int i = 0; i < 12; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(8f, 20f) * scale;
                try
                {
                    var arc = new ElectricArcParticle(position, velocity, primaryColor, 0.8f, 40);
                    Particles.MagnumParticleHandler.SpawnParticle(arc);
                }
                catch { }
            }
            
            // Sparks
            for (int i = 0; i < 40; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(5f, 25f) * scale;
                try
                {
                    var spark = new DirectionalSparkParticle(position, velocity, false, 45, 2f * scale, primaryColor);
                    Particles.MagnumParticleHandler.SpawnParticle(spark);
                }
                catch { }
            }
            
            // Strong bloom at center
            try
            {
                var bloom = new StrongBloomParticle(position, Vector2.Zero, primaryColor, 4f * scale, 30);
                Particles.MagnumParticleHandler.SpawnParticle(bloom);
            }
            catch { }
            
            MagnumScreenEffects.AddScreenShake(12f * scale);
            MagnumScreenEffects.SetFlashEffect(position, 1.5f * scale, 30);
        }
        
        /// <summary>
        /// Creates a death explosion with all the dramatic effects.
        /// Use for boss deaths or major enemy deaths.
        /// </summary>
        public static void CreateDeathExplosion(Vector2 position, Color primaryColor, Color secondaryColor, float scale = 1f)
        {
            // Multiple pulse rings with staggered timing
            for (int ring = 0; ring < 5; ring++)
            {
                float ringScale = (1f + ring * 0.3f) * scale;
                int delay = ring * 5;
                Color ringColor = Color.Lerp(primaryColor, secondaryColor, ring / 5f);
                
                try
                {
                    var pulseRing = new PulseRingParticle(position, Vector2.Zero, ringColor, 0f, 4f * ringScale, 40 + delay);
                    Particles.MagnumParticleHandler.SpawnParticle(pulseRing);
                }
                catch { }
            }
            
            // Electric arcs bursting outward
            for (int i = 0; i < 20; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(10f, 30f) * scale;
                Color arcColor = Color.Lerp(primaryColor, secondaryColor, Main.rand.NextFloat());
                try
                {
                    var arc = new ElectricArcParticle(position, velocity, arcColor, 1f, 50);
                    Particles.MagnumParticleHandler.SpawnParticle(arc);
                }
                catch { }
            }
            
            // Massive spark burst
            for (int i = 0; i < 60; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(5f, 35f) * scale;
                Color sparkColor = Main.rand.NextBool() ? primaryColor : secondaryColor;
                try
                {
                    var spark = new DirectionalSparkParticle(position, velocity, Main.rand.NextBool(4), 60, Main.rand.NextFloat(1.5f, 3f) * scale, sparkColor);
                    Particles.MagnumParticleHandler.SpawnParticle(spark);
                }
                catch { }
            }
            
            // Smoke ring expanding outward
            for (int i = 0; i < 32; i++)
            {
                float angle = MathHelper.TwoPi * i / 32f;
                Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(7f, 12f) * scale;
                Color smokeColor = Color.Lerp(primaryColor, secondaryColor, Main.rand.NextFloat());
                try
                {
                    var smoke = new DenseSmokeParticle(position, velocity, smokeColor, 56, 2.4f * scale, 1f);
                    Particles.MagnumParticleHandler.SpawnParticle(smoke);
                    
                    velocity *= 2f;
                    var smoke2 = new DenseSmokeParticle(position, velocity, smokeColor, 56, 3f * scale, 1f);
                    Particles.MagnumParticleHandler.SpawnParticle(smoke2);
                }
                catch { }
            }
            
            // Flare shine at center
            try
            {
                var flare = new FlareShineParticle(position, Vector2.Zero, primaryColor, secondaryColor, 0f, new Vector2(10f * scale), 60);
                Particles.MagnumParticleHandler.SpawnParticle(flare);
            }
            catch { }
            
            // Strong bloom
            try
            {
                var bloom = new StrongBloomParticle(position, Vector2.Zero, primaryColor, 5f * scale, 35);
                Particles.MagnumParticleHandler.SpawnParticle(bloom);
            }
            catch { }
            
            // Maximum visual impact
            MagnumScreenEffects.AddScreenShake(20f * scale);
            MagnumScreenEffects.SetFlashEffect(position, 2f * scale, 45);
            ShockwaveUtility.CreateShockwave(position, 3, 10, 100f * scale, primaryColor);
        }
        
        /// <summary>
        /// Creates a generic dust explosion for simple effects.
        /// </summary>
        public static void CreateDustExplosion(Vector2 position, int dustType, int count, float speed, float scale)
        {
            for (int burst = 0; burst < 3; burst++)
            {
                float burstSpeed = speed + burst * 3f;
                for (int i = 0; i < count; i++)
                {
                    Vector2 velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(0.8f, 1.2f) * burstSpeed;
                    Dust dust = Dust.NewDustPerfect(position + Main.rand.NextVector2Circular(10f, 10f), dustType, velocity);
                    dust.scale = scale * Main.rand.NextFloat(0.8f, 1.2f);
                    dust.noGravity = true;
                }
            }
        }
    }
}
