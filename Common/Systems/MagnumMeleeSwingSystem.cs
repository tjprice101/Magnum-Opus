using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.LaCampanella.Debuffs;

namespace MagnumOpus.Common.Systems
{
    /// <summary>
    /// Configuration for MagnumOpus's unique melee swing pattern.
    /// Swing 1: Downward slash
    /// Swing 2: Upward slash  
    /// Swing 3: 360° spin with particle burst
    /// </summary>
    public class MagnumSwingConfig
    {
        // Theme colors for particles
        public Color PrimaryColor { get; set; } = Color.White;
        public Color SecondaryColor { get; set; } = Color.Gray;
        
        // Particle burst settings for the 360 spin
        public int BurstParticleCount { get; set; } = 12;
        public float BurstRadius { get; set; } = 60f;
        public float BurstScale { get; set; } = 0.5f;
        
        // Optional custom effects
        public Action<Vector2, int, float> SpawnThemeMusicNotes { get; set; }
        public Action<Vector2, float> SpawnThemeImpact { get; set; }
        public Action<Vector2, float> SpawnThemeBurst { get; set; }
        
        // Sound effects
        public SoundStyle? SwingSound { get; set; }
        public SoundStyle? BurstSound { get; set; }
        
        // Trail settings
        public bool EnableTrail { get; set; } = true;
        public float TrailScale { get; set; } = 1f;
    }
    
    /// <summary>
    /// Tracks the current swing state for the player.
    /// Handles the Down → Up → 360 Burst combo pattern.
    /// </summary>
    public class MagnumMeleePlayer : ModPlayer
    {
        // Current swing in the combo (0 = down, 1 = up, 2 = 360 spin)
        public int CurrentSwingIndex { get; private set; } = 0;
        
        // Timer for combo reset
        private int comboResetTimer = 0;
        private const int ComboResetTime = 45; // Frames before combo resets (0.75 seconds)
        
        // Track if we're mid-swing
        public bool IsSwinging { get; private set; } = false;
        private float swingProgress = 0f;
        private float swingDuration = 0f;
        
        // Current swing config
        private MagnumSwingConfig currentConfig;
        private Item swingingWeapon;
        
        // Track last held item type to reset combo when switching weapons
        private int lastHeldItemType = -1;
        
        // Swing angle tracking for rotation
        private float startAngle;
        private float endAngle;
        private float currentAngle;
        
        // For 360 burst tracking
        private bool hasFiredBurst = false;
        
        /// <summary>
        /// Gets the current swing direction: 0 = down, 1 = up, 2 = spin
        /// </summary>
        public int GetSwingDirection() => CurrentSwingIndex;
        
        /// <summary>
        /// Gets the current swing angle for custom drawing
        /// </summary>
        public float GetCurrentSwingAngle() => currentAngle;
        
        /// <summary>
        /// Gets swing progress from 0 to 1
        /// </summary>
        public float GetSwingProgress() => swingProgress;
        
        /// <summary>
        /// Call this when starting a swing. Returns the swing type (0=down, 1=up, 2=spin)
        /// </summary>
        public int StartSwing(Item weapon, MagnumSwingConfig config, int useAnimationFrames)
        {
            if (weapon == null || config == null) return 0;
            
            // Reset combo if switching weapons
            if (weapon.type != lastHeldItemType)
            {
                CurrentSwingIndex = 0;
                lastHeldItemType = weapon.type;
            }
            
            currentConfig = config;
            swingingWeapon = weapon;
            IsSwinging = true;
            swingProgress = 0f;
            swingDuration = useAnimationFrames;
            hasFiredBurst = false;
            
            // Calculate swing angles based on direction player is facing
            int dir = Player.direction;
            float baseAngle = dir == 1 ? 0f : MathHelper.Pi;
            
            switch (CurrentSwingIndex)
            {
                case 0: // Downward slash
                    startAngle = baseAngle - MathHelper.PiOver2 * 0.8f * dir;
                    endAngle = baseAngle + MathHelper.PiOver2 * 1.2f * dir;
                    break;
                    
                case 1: // Upward slash
                    startAngle = baseAngle + MathHelper.PiOver2 * 1.2f * dir;
                    endAngle = baseAngle - MathHelper.PiOver2 * 0.8f * dir;
                    break;
                    
                case 2: // 360 spin
                    startAngle = baseAngle - MathHelper.PiOver4 * dir;
                    endAngle = startAngle + MathHelper.TwoPi * dir;
                    break;
            }
            
            currentAngle = startAngle;
            
            // Play swing sound
            if (config.SwingSound.HasValue)
            {
                SoundEngine.PlaySound(config.SwingSound.Value, Player.Center);
            }
            
            // Reset combo timer
            comboResetTimer = ComboResetTime + useAnimationFrames;
            
            return CurrentSwingIndex;
        }
        
        /// <summary>
        /// Call this every frame during the swing animation
        /// </summary>
        public void UpdateSwing()
        {
            if (!IsSwinging || currentConfig == null) return;
            
            swingProgress += 1f / swingDuration;
            
            // Ease the swing (faster in middle, slower at ends)
            float easedProgress = EaseSwing(swingProgress);
            
            // Calculate current angle
            currentAngle = MathHelper.Lerp(startAngle, endAngle, easedProgress);
            
            // Spawn trail particles
            if (currentConfig.EnableTrail)
            {
                SpawnSwingTrail(easedProgress);
            }
            
            // For 360 spin, spawn burst at midpoint
            if (CurrentSwingIndex == 2 && !hasFiredBurst && swingProgress >= 0.5f)
            {
                SpawnParticleBurst();
                hasFiredBurst = true;
            }
            
            // End swing
            if (swingProgress >= 1f)
            {
                EndSwing();
            }
        }
        
        private float EaseSwing(float t)
        {
            // Smooth ease in/out
            return t < 0.5f 
                ? 4f * t * t * t 
                : 1f - (float)Math.Pow(-2f * t + 2f, 3) / 2f;
        }
        
        private void SpawnSwingTrail(float progress)
        {
            if (currentConfig == null) return;
            
            // Calculate weapon tip position
            float weaponLength = (swingingWeapon?.width ?? 50) * 0.8f;
            Vector2 tipPos = Player.Center + currentAngle.ToRotationVector2() * weaponLength;
            
            // Gradient color based on progress
            Color trailColor = Color.Lerp(currentConfig.PrimaryColor, currentConfig.SecondaryColor, progress);
            
            // Spawn trail flare
            CustomParticles.GenericFlare(tipPos, trailColor, 0.3f * currentConfig.TrailScale, 12);
            
            // Spawn occasional glow particles
            if (Main.rand.NextBool(3))
            {
                Vector2 velocity = currentAngle.ToRotationVector2().RotatedBy(MathHelper.PiOver2) * 2f;
                var glow = new GenericGlowParticle(tipPos, velocity + Main.rand.NextVector2Circular(1f, 1f),
                    trailColor, 0.25f * currentConfig.TrailScale, 15, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Spawn music notes occasionally
            if (Main.rand.NextBool(8) && currentConfig.SpawnThemeMusicNotes != null)
            {
                currentConfig.SpawnThemeMusicNotes(tipPos, 1, 20f);
            }
            
            // For spin attack, more intense trail
            if (CurrentSwingIndex == 2)
            {
                // Extra particles for the spin
                if (Main.rand.NextBool(2))
                {
                    float angle = currentAngle + Main.rand.NextFloat(-0.3f, 0.3f);
                    Vector2 sparkPos = Player.Center + angle.ToRotationVector2() * weaponLength * Main.rand.NextFloat(0.5f, 1f);
                    Color sparkColor = Color.Lerp(currentConfig.PrimaryColor, currentConfig.SecondaryColor, Main.rand.NextFloat());
                    
                    var spark = new GlowSparkParticle(sparkPos, 
                        angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f),
                        sparkColor, 0.3f * currentConfig.TrailScale, 18);
                    MagnumParticleHandler.SpawnParticle(spark);
                }
            }
        }
        
        private void SpawnParticleBurst()
        {
            if (currentConfig == null) return;
            
            // === 360 PARTICLE BURST ===
            int count = currentConfig.BurstParticleCount;
            float radius = currentConfig.BurstRadius;
            float scale = currentConfig.BurstScale;
            
            // Central flash
            CustomParticles.GenericFlare(Player.Center, Color.White, 0.8f * scale, 20);
            
            // Radial flare burst with gradient
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count;
                Vector2 offset = angle.ToRotationVector2() * radius;
                Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                
                // Gradient color
                float progress = (float)i / count;
                Color burstColor = Color.Lerp(currentConfig.PrimaryColor, currentConfig.SecondaryColor, progress);
                
                // Flare at burst position
                CustomParticles.GenericFlare(Player.Center + offset * 0.5f, burstColor, 0.5f * scale, 18);
                
                // Outward spark
                var spark = new GlowSparkParticle(Player.Center + offset * 0.3f, velocity, burstColor, 0.4f * scale, 22);
                MagnumParticleHandler.SpawnParticle(spark);
            }
            
            // Inner ring of secondary flares
            for (int i = 0; i < count / 2; i++)
            {
                float angle = MathHelper.TwoPi * i / (count / 2) + MathHelper.Pi / count;
                Vector2 offset = angle.ToRotationVector2() * radius * 0.5f;
                
                float progress = (float)i / (count / 2);
                Color innerColor = Color.Lerp(currentConfig.SecondaryColor, currentConfig.PrimaryColor, progress);
                
                CustomParticles.GenericFlare(Player.Center + offset, innerColor, 0.35f * scale, 15);
            }
            
            // Halo ring
            CustomParticles.HaloRing(Player.Center, currentConfig.PrimaryColor, 0.6f * scale, 25);
            CustomParticles.HaloRing(Player.Center, currentConfig.SecondaryColor, 0.4f * scale, 20);
            
            // Theme-specific burst
            currentConfig.SpawnThemeBurst?.Invoke(Player.Center, scale);
            
            // Music notes burst
            if (currentConfig.SpawnThemeMusicNotes != null)
            {
                currentConfig.SpawnThemeMusicNotes(Player.Center, 6, radius);
            }
            
            // Burst sound
            if (currentConfig.BurstSound.HasValue)
            {
                SoundEngine.PlaySound(currentConfig.BurstSound.Value, Player.Center);
            }
            else
            {
                // Default burst sound
                SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.2f, Volume = 0.5f }, Player.Center);
            }
            
            // Subtle screen shake
            Player.GetModPlayer<ScreenShakePlayer>()?.AddShake(3f, 8);
            
            // Light burst
            Lighting.AddLight(Player.Center, currentConfig.PrimaryColor.ToVector3() * 1.5f);
        }
        
        private void EndSwing()
        {
            IsSwinging = false;
            swingProgress = 0f;
            
            // Advance to next swing in combo
            CurrentSwingIndex = (CurrentSwingIndex + 1) % 3;
            
            // Reset combo timer
            comboResetTimer = ComboResetTime;
        }
        
        public override void PreUpdate()
        {
            // Decay combo timer
            if (comboResetTimer > 0 && !IsSwinging)
            {
                comboResetTimer--;
                
                if (comboResetTimer <= 0)
                {
                    // Reset combo
                    CurrentSwingIndex = 0;
                }
            }
        }
        
        /// <summary>
        /// Call this to manually reset the combo (e.g., when switching weapons)
        /// </summary>
        public void ResetCombo()
        {
            CurrentSwingIndex = 0;
            comboResetTimer = 0;
            IsSwinging = false;
            swingProgress = 0f;
            currentConfig = null;
            swingingWeapon = null;
        }
    }
    
    /// <summary>
    /// Helper class with pre-configured swing configs for each theme
    /// </summary>
    public static class MagnumSwingConfigs
    {
        public static MagnumSwingConfig SwanLake => new MagnumSwingConfig
        {
            PrimaryColor = UnifiedVFX.SwanLake.White,
            SecondaryColor = UnifiedVFX.SwanLake.Black,
            BurstParticleCount = 14,
            BurstRadius = 65f,
            BurstScale = 0.55f,
            SpawnThemeMusicNotes = (pos, count, radius) => ThemedParticles.SwanLakeMusicNotes(pos, count, radius),
            SpawnThemeBurst = (pos, scale) => ThemedParticles.SwanLakeRainbowExplosion(pos, scale),
            SwingSound = SoundID.Item1 with { Pitch = 0.1f },
            BurstSound = SoundID.Item29 with { Pitch = 0.3f, Volume = 0.6f }
        };
        
        public static MagnumSwingConfig LaCampanella => new MagnumSwingConfig
        {
            PrimaryColor = UnifiedVFX.LaCampanella.Orange,
            SecondaryColor = UnifiedVFX.LaCampanella.Black,
            BurstParticleCount = 12,
            BurstRadius = 60f,
            BurstScale = 0.5f,
            SpawnThemeMusicNotes = (pos, count, radius) => ThemedParticles.LaCampanellaMusicNotes(pos, count, radius),
            SpawnThemeBurst = (pos, scale) => UnifiedVFX.LaCampanella.Explosion(pos, scale),
            SwingSound = SoundID.Item1 with { Pitch = -0.1f },
            BurstSound = SoundID.DD2_BetsyFireballImpact with { Volume = 0.5f }
        };
        
        public static MagnumSwingConfig Eroica => new MagnumSwingConfig
        {
            PrimaryColor = UnifiedVFX.Eroica.Scarlet,
            SecondaryColor = UnifiedVFX.Eroica.Gold,
            BurstParticleCount = 12,
            BurstRadius = 60f,
            BurstScale = 0.5f,
            SpawnThemeMusicNotes = (pos, count, radius) => ThemedParticles.EroicaMusicNotes(pos, count, radius),
            SpawnThemeBurst = (pos, scale) => UnifiedVFX.Eroica.Explosion(pos, scale),
            SwingSound = SoundID.Item1,
            BurstSound = SoundID.Item45 with { Volume = 0.5f }
        };
        
        public static MagnumSwingConfig MoonlightSonata => new MagnumSwingConfig
        {
            PrimaryColor = UnifiedVFX.MoonlightSonata.DarkPurple,
            SecondaryColor = UnifiedVFX.MoonlightSonata.LightBlue,
            BurstParticleCount = 10,
            BurstRadius = 55f,
            BurstScale = 0.45f,
            SpawnThemeMusicNotes = (pos, count, radius) => ThemedParticles.MoonlightMusicNotes(pos, count, radius),
            SpawnThemeBurst = (pos, scale) => UnifiedVFX.MoonlightSonata.Explosion(pos, scale),
            SwingSound = SoundID.Item1 with { Pitch = 0.2f },
            BurstSound = SoundID.Item29 with { Pitch = 0.5f, Volume = 0.4f }
        };
        
        public static MagnumSwingConfig EnigmaVariations => new MagnumSwingConfig
        {
            PrimaryColor = UnifiedVFX.EnigmaVariations.Purple,
            SecondaryColor = UnifiedVFX.EnigmaVariations.GreenFlame,
            BurstParticleCount = 12,
            BurstRadius = 60f,
            BurstScale = 0.5f,
            SpawnThemeMusicNotes = (pos, count, radius) => ThemedParticles.EnigmaMusicNotes(pos, count, radius),
            SpawnThemeBurst = (pos, scale) => UnifiedVFX.EnigmaVariations.Explosion(pos, scale),
            SwingSound = SoundID.Item1 with { Pitch = -0.2f },
            BurstSound = SoundID.Item104 with { Volume = 0.5f }
        };
        
        public static MagnumSwingConfig Fate => new MagnumSwingConfig
        {
            PrimaryColor = UnifiedVFX.Fate.White,
            SecondaryColor = UnifiedVFX.Fate.Crimson,
            BurstParticleCount = 16,
            BurstRadius = 70f,
            BurstScale = 0.6f,
            SpawnThemeMusicNotes = (pos, count, radius) => ThemedParticles.FateMusicNotes(pos, count, radius),
            SpawnThemeBurst = (pos, scale) => UnifiedVFX.Fate.Explosion(pos, scale),
            SwingSound = SoundID.Item1 with { Pitch = -0.3f },
            BurstSound = SoundID.Item122 with { Volume = 0.6f }
        };
        
        /// <summary>
        /// Creates a custom swing config with specified colors
        /// </summary>
        public static MagnumSwingConfig Custom(Color primary, Color secondary, 
            Action<Vector2, int, float> musicNotes = null, 
            Action<Vector2, float> burst = null)
        {
            return new MagnumSwingConfig
            {
                PrimaryColor = primary,
                SecondaryColor = secondary,
                SpawnThemeMusicNotes = musicNotes,
                SpawnThemeBurst = burst
            };
        }
    }
}
