using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Common.Systems
{
    // ============================================================================
    // SCREEN EFFECT SYSTEM - Ported from InfernumMode patterns
    // Provides screen shake, flash, blur, and shockwave effects for dramatic moments.
    // FATE THEME: Includes reality distortion effects exclusive to endgame Fate weapons.
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
    // FATE REALITY DISTORTION SYSTEM - Endgame Exclusive Effects
    // These effects break reality itself - chromatic aberration, screen slices,
    // temporal echoes, and reality shattering. ONLY for Fate-themed weapons.
    // ============================================================================
    
    /// <summary>
    /// Fate-exclusive screen distortion effects that make reality bend and shatter.
    /// These are endgame-tier visual effects unique to the Fate theme.
    /// </summary>
    public static class FateRealityDistortion
    {
        // ==================== CHROMATIC ABERRATION ====================
        private static bool ChromaticActive;
        private static float ChromaticIntensity;
        private static int ChromaticLifetime;
        private static int ChromaticTime;
        private static Vector2 ChromaticCenter;
        
        // ==================== SCREEN SLICE ====================
        private static bool SliceActive;
        private static Vector2 SliceStart;
        private static Vector2 SliceEnd;
        private static float SliceIntensity;
        private static int SliceLifetime;
        private static int SliceTime;
        private static Color SliceColor;
        
        // ==================== REALITY SHATTER ====================
        private static bool ShatterActive;
        private static Vector2 ShatterCenter;
        private static float ShatterIntensity;
        private static int ShatterLifetime;
        private static int ShatterTime;
        private static List<ShatterFragment> ShatterFragments = new List<ShatterFragment>();
        
        // ==================== COLOR INVERSION PULSE ====================
        private static bool InversionActive;
        private static int InversionTime;
        private static int InversionLifetime;
        
        // Fate theme colors
        public static readonly Color FateBlack = new Color(15, 5, 20);
        public static readonly Color FateDarkPink = new Color(180, 50, 100);
        public static readonly Color FateBrightRed = new Color(255, 60, 80);
        public static readonly Color FatePurple = new Color(120, 30, 140);
        public static readonly Color FateWhite = Color.White;
        
        private struct ShatterFragment
        {
            public Vector2 Position;
            public Vector2 Offset;
            public float Rotation;
            public float Scale;
        }
        
        /// <summary>
        /// Triggers chromatic aberration (RGB color channel separation).
        /// Creates a reality-breaking visual where red and blue channels offset.
        /// </summary>
        /// <param name="center">World position center of the effect</param>
        /// <param name="intensity">Strength of the RGB separation (3-10 recommended)</param>
        /// <param name="lifetime">Duration in frames</param>
        public static void TriggerChromaticAberration(Vector2 center, float intensity, int lifetime)
        {
            ChromaticActive = true;
            ChromaticCenter = center;
            ChromaticIntensity = intensity;
            ChromaticLifetime = lifetime;
            ChromaticTime = 0;
        }
        
        /// <summary>
        /// Creates a visual "cut" across the screen as if reality itself is being sliced.
        /// </summary>
        /// <param name="start">Start position of the slice (world coordinates)</param>
        /// <param name="end">End position of the slice (world coordinates)</param>
        /// <param name="intensity">Width/brightness of the slice</param>
        /// <param name="lifetime">Duration in frames</param>
        public static void TriggerScreenSlice(Vector2 start, Vector2 end, float intensity, int lifetime)
        {
            SliceActive = true;
            SliceStart = start;
            SliceEnd = end;
            SliceIntensity = intensity;
            SliceLifetime = lifetime;
            SliceTime = 0;
            SliceColor = Color.Lerp(FateWhite, FateDarkPink, 0.3f);
        }
        
        /// <summary>
        /// Creates an effect where the screen appears to shatter into fragments briefly.
        /// Ultimate visual for Fate boss deaths or ultimate attacks.
        /// </summary>
        /// <param name="center">Center of the shatter effect (world coordinates)</param>
        /// <param name="fragmentCount">Number of shatter fragments (8-16 recommended)</param>
        /// <param name="intensity">How far fragments displace</param>
        /// <param name="lifetime">Duration in frames</param>
        public static void TriggerRealityShatter(Vector2 center, int fragmentCount, float intensity, int lifetime)
        {
            ShatterActive = true;
            ShatterCenter = center;
            ShatterIntensity = intensity;
            ShatterLifetime = lifetime;
            ShatterTime = 0;
            
            // Generate random shatter fragments
            ShatterFragments.Clear();
            for (int i = 0; i < fragmentCount; i++)
            {
                ShatterFragments.Add(new ShatterFragment
                {
                    Position = center + Main.rand.NextVector2Circular(200f, 200f),
                    Offset = Main.rand.NextVector2Unit() * Main.rand.NextFloat(5f, 15f) * intensity,
                    Rotation = Main.rand.NextFloat(-0.1f, 0.1f) * intensity,
                    Scale = 1f + Main.rand.NextFloat(-0.05f, 0.05f) * intensity
                });
            }
        }
        
        /// <summary>
        /// Brief color inversion flash that makes reality feel wrong.
        /// </summary>
        /// <param name="lifetime">Duration of the inversion pulse (5-15 frames recommended)</param>
        public static void TriggerInversionPulse(int lifetime)
        {
            InversionActive = true;
            InversionTime = 0;
            InversionLifetime = lifetime;
        }
        
        /// <summary>
        /// Triggers a full Fate reality-break combo: chromatic + slice + shake.
        /// Use for Fate Sever or other ultimate Fate attacks.
        /// </summary>
        public static void TriggerFullRealityBreak(Vector2 position, Vector2 sliceDirection, float scale = 1f)
        {
            // Chromatic aberration
            TriggerChromaticAberration(position, 6f * scale, 25);
            
            // Screen slice along the attack direction
            Vector2 perpendicular = sliceDirection.RotatedBy(MathHelper.PiOver2);
            perpendicular.Normalize();
            TriggerScreenSlice(
                position - perpendicular * 400f * scale,
                position + perpendicular * 400f * scale,
                2f * scale,
                18
            );
            
            // Brief inversion pulse
            TriggerInversionPulse(8);
            
            // Screen shake
            MagnumScreenEffects.AddScreenShake(12f * scale);
            
            // Flash
            MagnumScreenEffects.SetFlashEffect(position, 1.5f * scale, 20);
        }
        
        /// <summary>
        /// Ultimate reality shatter for boss deaths or climactic moments.
        /// </summary>
        public static void TriggerUltimateShatter(Vector2 position, float scale = 1f)
        {
            TriggerRealityShatter(position, 12, 1.5f * scale, 45);
            TriggerChromaticAberration(position, 8f * scale, 40);
            TriggerInversionPulse(12);
            MagnumScreenEffects.AddScreenShake(25f * scale);
            MagnumScreenEffects.SetFlashEffect(position, 2f * scale, 50);
        }
        
        /// <summary>
        /// Update all active Fate distortion effects. Called from ModSystem.
        /// </summary>
        public static void Update()
        {
            if (ChromaticActive)
            {
                ChromaticTime++;
                if (ChromaticTime >= ChromaticLifetime)
                    ChromaticActive = false;
            }
            
            if (SliceActive)
            {
                SliceTime++;
                if (SliceTime >= SliceLifetime)
                    SliceActive = false;
            }
            
            if (ShatterActive)
            {
                ShatterTime++;
                if (ShatterTime >= ShatterLifetime)
                {
                    ShatterActive = false;
                    ShatterFragments.Clear();
                }
            }
            
            if (InversionActive)
            {
                InversionTime++;
                if (InversionTime >= InversionLifetime)
                    InversionActive = false;
            }
        }
        
        /// <summary>
        /// Gets the current chromatic aberration offset for rendering.
        /// Returns the RGB offset vector based on current effect state.
        /// </summary>
        public static Vector2 GetChromaticOffset()
        {
            if (!ChromaticActive)
                return Vector2.Zero;
            
            float progress = (float)ChromaticTime / ChromaticLifetime;
            float easeOut = 1f - progress * progress; // Quadratic ease-out
            return new Vector2(ChromaticIntensity * easeOut, 0f);
        }
        
        /// <summary>
        /// Check if any Fate distortion effect is active.
        /// </summary>
        public static bool AnyDistortionActive() => 
            ChromaticActive || SliceActive || ShatterActive || InversionActive;
        
        /// <summary>
        /// Gets the current inversion intensity (0-1).
        /// </summary>
        public static float GetInversionIntensity()
        {
            if (!InversionActive)
                return 0f;
            
            float progress = (float)InversionTime / InversionLifetime;
            // Pulse in and out
            return (float)Math.Sin(progress * MathHelper.Pi) * 0.3f;
        }
        
        /// <summary>
        /// Draw Fate screen slice effect.
        /// Call during PostDrawTiles or similar.
        /// </summary>
        public static void DrawSliceEffect(SpriteBatch spriteBatch)
        {
            if (!SliceActive)
                return;
            
            float progress = (float)SliceTime / SliceLifetime;
            float alpha = 1f - progress;
            float width = SliceIntensity * (1f + progress * 0.5f);
            
            Vector2 screenStart = SliceStart - Main.screenPosition;
            Vector2 screenEnd = SliceEnd - Main.screenPosition;
            Vector2 direction = (screenEnd - screenStart).SafeNormalize(Vector2.UnitX);
            float length = Vector2.Distance(screenStart, screenEnd);
            float rotation = direction.ToRotation();
            
            // Draw the slice as overlapping lines with gradient
            Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;
            
            // Outer glow (dark pink)
            for (int i = 0; i < 3; i++)
            {
                float layerWidth = width * (3f - i);
                float layerAlpha = alpha * (0.3f - i * 0.08f);
                Color layerColor = Color.Lerp(FateDarkPink, FateBrightRed, (float)i / 3f) * layerAlpha;
                
                spriteBatch.Draw(
                    pixel,
                    screenStart,
                    null,
                    layerColor,
                    rotation,
                    new Vector2(0, 0.5f),
                    new Vector2(length, layerWidth),
                    SpriteEffects.None,
                    0f
                );
            }
            
            // Inner core (white)
            spriteBatch.Draw(
                pixel,
                screenStart,
                null,
                FateWhite * alpha * 0.8f,
                rotation,
                new Vector2(0, 0.5f),
                new Vector2(length, width * 0.3f),
                SpriteEffects.None,
                0f
            );
            
            // Add sparkling particles along the slice
            if (SliceTime % 3 == 0 && SliceTime < SliceLifetime - 5)
            {
                float t = Main.rand.NextFloat();
                Vector2 particlePos = Vector2.Lerp(SliceStart, SliceEnd, t);
                Vector2 particleVel = direction.RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * Main.rand.NextFloat(2f, 5f);
                Color particleColor = Color.Lerp(FateWhite, FateDarkPink, Main.rand.NextFloat());
                
                try
                {
                    var spark = new DirectionalSparkParticle(particlePos, particleVel, false, 20, 1.5f, particleColor);
                    MagnumParticleHandler.SpawnParticle(spark);
                }
                catch { }
            }
        }
        
        /// <summary>
        /// Get cosmic gradient color for Fate effects.
        /// </summary>
        public static Color GetFateGradient(float progress)
        {
            if (progress < 0.3f)
                return Color.Lerp(FateBlack, FateDarkPink, progress / 0.3f);
            else if (progress < 0.7f)
                return Color.Lerp(FateDarkPink, FatePurple, (progress - 0.3f) / 0.4f);
            else
                return Color.Lerp(FatePurple, FateBrightRed, (progress - 0.7f) / 0.3f);
        }
    }
    
    /// <summary>
    /// ModSystem to update Fate distortion effects each frame.
    /// </summary>
    public class FateDistortionUpdater : ModSystem
    {
        public override void PostUpdateEverything()
        {
            FateRealityDistortion.Update();
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
