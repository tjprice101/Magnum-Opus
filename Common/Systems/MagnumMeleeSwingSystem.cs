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

namespace MagnumOpus.Common.Systems
{
    /// <summary>
    /// Configuration for MagnumOpus's full rotation melee swing.
    /// Every swing is a full 360° rotation with a large particle trail.
    /// </summary>
    public class MagnumSwingConfig
    {
        // Theme colors for particles
        public Color PrimaryColor { get; set; } = Color.White;
        public Color SecondaryColor { get; set; } = Color.Gray;
        
        // Trail settings
        public float TrailScale { get; set; } = 1.5f;
        public int TrailDensity { get; set; } = 3; // Particles per frame
        
        // Optional custom effects
        public Action<Vector2, int, float> SpawnThemeMusicNotes { get; set; }
        
        // Custom trail effect (for theme-specific visuals like nebular clouds)
        public Action<Vector2, float, float> SpawnCustomTrailEffect { get; set; }
        
        // Sound effects
        public SoundStyle? SwingSound { get; set; }
    }
    
    /// <summary>
    /// Tracks the current swing state for the player.
    /// Every swing is a full 360° rotation with large particle trail.
    /// </summary>
    public class MagnumMeleePlayer : ModPlayer
    {
        // Track if we're mid-swing
        public bool IsSwinging { get; private set; } = false;
        private float swingProgress = 0f;
        private float swingDuration = 0f;
        
        // Current swing config
        private MagnumSwingConfig currentConfig;
        private Item swingingWeapon;
        
        // Track last held item type
        private int lastHeldItemType = -1;
        
        // Track itemAnimation to detect new swing cycles
        private int lastItemAnimation = 0;
        private bool wasSwingingLastFrame = false;
        
        // Swing angle tracking for rotation
        private float startAngle;
        private float endAngle;
        private float currentAngle;
        
        // Cached config for current held weapon
        private MagnumSwingConfig cachedHeldConfig = null;
        private int cachedHeldItemType = -1;
        
        /// <summary>
        /// Gets the current swing angle for custom drawing
        /// </summary>
        public float GetCurrentSwingAngle() => currentAngle;
        
        /// <summary>
        /// Gets swing progress from 0 to 1
        /// </summary>
        public float GetSwingProgress() => swingProgress;
        
        /// <summary>
        /// Starts a full 360° rotation swing
        /// </summary>
        public void StartSwing(Item weapon, MagnumSwingConfig config, int useAnimationFrames)
        {
            if (weapon == null || config == null) return;
            
            lastHeldItemType = weapon.type;
            currentConfig = config;
            swingingWeapon = weapon;
            IsSwinging = true;
            swingProgress = 0f;
            swingDuration = useAnimationFrames;
            
            // Calculate full 360° rotation based on direction player is facing
            int dir = Player.direction;
            
            // Start from above-forward, rotate full circle
            startAngle = -MathHelper.PiOver2; // Start pointing up
            endAngle = startAngle + MathHelper.TwoPi * dir; // Full rotation
            
            currentAngle = startAngle;
            
            // Play swing sound
            if (config.SwingSound.HasValue)
            {
                SoundEngine.PlaySound(config.SwingSound.Value, Player.Center);
            }
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
            
            // Spawn large trail particles
            SpawnSwingTrail(easedProgress);
            
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
            if (currentConfig == null || swingingWeapon == null) return;
            
            // Calculate weapon tip position - use a larger radius for visual impact
            float weaponLength = Math.Max(swingingWeapon.width, swingingWeapon.height) * 1.2f;
            Vector2 tipPos = Player.Center + currentAngle.ToRotationVector2() * weaponLength;
            
            // Gradient color based on progress
            Color trailColor = Color.Lerp(currentConfig.PrimaryColor, currentConfig.SecondaryColor, progress);
            
            // === LARGE PARTICLE TRAIL ===
            float scale = currentConfig.TrailScale;
            
            // Main bright flare at weapon tip
            CustomParticles.GenericFlare(tipPos, trailColor, 0.6f * scale, 15);
            CustomParticles.GenericFlare(tipPos, Color.White * 0.8f, 0.35f * scale, 10);
            
            // Multiple trailing glow particles
            for (int i = 0; i < currentConfig.TrailDensity; i++)
            {
                // Velocity perpendicular to swing direction (trailing behind)
                Vector2 trailVel = -currentAngle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                trailVel += Main.rand.NextVector2Circular(1.5f, 1.5f);
                
                // Gradient across particles
                float particleProgress = (progress + i * 0.1f) % 1f;
                Color particleColor = Color.Lerp(currentConfig.PrimaryColor, currentConfig.SecondaryColor, particleProgress);
                
                var glow = new GenericGlowParticle(
                    tipPos + Main.rand.NextVector2Circular(5f, 5f), 
                    trailVel,
                    particleColor, 
                    0.4f * scale, 
                    20, 
                    true
                );
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Sparks flying off
            if (Main.rand.NextBool(2))
            {
                Vector2 sparkVel = currentAngle.ToRotationVector2().RotatedBy(MathHelper.PiOver2 * Player.direction) * Main.rand.NextFloat(3f, 7f);
                sparkVel += Main.rand.NextVector2Circular(2f, 2f);
                
                Color sparkColor = Color.Lerp(currentConfig.PrimaryColor, currentConfig.SecondaryColor, Main.rand.NextFloat());
                var spark = new GlowSparkParticle(tipPos, sparkVel, sparkColor, 0.35f * scale, 18);
                MagnumParticleHandler.SpawnParticle(spark);
            }
            
            // Music notes occasionally
            if (Main.rand.NextBool(6) && currentConfig.SpawnThemeMusicNotes != null)
            {
                currentConfig.SpawnThemeMusicNotes(tipPos, 1, 25f);
            }
            
            // Halo ring periodically for extra flair
            if (Main.rand.NextBool(8))
            {
                CustomParticles.HaloRing(tipPos, trailColor * 0.6f, 0.25f * scale, 12);
            }
            
            // Custom theme-specific trail effect (e.g., Fate nebular clouds)
            if (currentConfig.SpawnCustomTrailEffect != null)
            {
                currentConfig.SpawnCustomTrailEffect(tipPos, progress, scale);
            }
            
            // Dynamic lighting
            Lighting.AddLight(tipPos, trailColor.ToVector3() * 0.8f);
        }
        
        private void EndSwing()
        {
            IsSwinging = false;
            swingProgress = 0f;
        }
        
        /// <summary>
        /// Sets the cached config for the current held weapon.
        /// Call this from the GlobalItem when a qualifying weapon is held.
        /// </summary>
        public void SetHeldWeaponConfig(Item item, MagnumSwingConfig config)
        {
            if (item == null || config == null) return;
            cachedHeldItemType = item.type;
            cachedHeldConfig = config;
        }
        
        public override void PreUpdate()
        {
            Item heldItem = Player.HeldItem;
            
            // Check if we have a valid config for this weapon
            bool hasValidConfig = heldItem != null && 
                                  cachedHeldConfig != null &&
                                  heldItem.type == cachedHeldItemType;
            
            // Detect if player is currently in a swing animation
            bool isCurrentlySwinging = Player.itemAnimation > 0 && hasValidConfig;
            
            // Detect the START of a new swing cycle
            // Case 1: We weren't swinging last frame but now we are (first click)
            // Case 2: itemAnimation jumped back up (held click started new swing)
            // Case 3: itemAnimation reset while we were swinging (continuous hold)
            bool newSwingStarted = false;
            
            if (isCurrentlySwinging)
            {
                if (!wasSwingingLastFrame)
                {
                    // Just started swinging
                    newSwingStarted = true;
                }
                else if (Player.itemAnimation > lastItemAnimation)
                {
                    // itemAnimation increased - new swing cycle while holding
                    newSwingStarted = true;
                }
                else if (lastItemAnimation <= 2 && Player.itemAnimation > 2)
                {
                    // Was about to end, but jumped back up (continuous swing)
                    newSwingStarted = true;
                }
            }
            
            if (newSwingStarted && cachedHeldConfig != null)
            {
                // Start the new swing
                StartSwing(heldItem, cachedHeldConfig, Player.itemAnimationMax);
            }
            
            // Update swing if in progress
            if (IsSwinging && isCurrentlySwinging)
            {
                UpdateSwing();
            }
            else if (IsSwinging && !isCurrentlySwinging)
            {
                // Swing ended naturally (itemAnimation reached 0)
                EndSwing();
            }
            
            // Track for next frame
            lastItemAnimation = Player.itemAnimation;
            wasSwingingLastFrame = isCurrentlySwinging;
        }
        
        /// <summary>
        /// Call this to manually reset the swing state
        /// </summary>
        public void ResetSwing()
        {
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
            TrailScale = 1.6f,
            TrailDensity = 4,
            SpawnThemeMusicNotes = (pos, count, radius) => ThemedParticles.SwanLakeMusicNotes(pos, count, radius),
            SwingSound = SoundID.Item1 with { Pitch = 0.1f }
        };
        
        public static MagnumSwingConfig LaCampanella => new MagnumSwingConfig
        {
            PrimaryColor = UnifiedVFX.LaCampanella.Orange,
            SecondaryColor = UnifiedVFX.LaCampanella.Black,
            TrailScale = 1.5f,
            TrailDensity = 3,
            SpawnThemeMusicNotes = (pos, count, radius) => ThemedParticles.LaCampanellaMusicNotes(pos, count, radius),
            SwingSound = SoundID.Item1 with { Pitch = -0.1f }
        };
        
        public static MagnumSwingConfig Eroica => new MagnumSwingConfig
        {
            PrimaryColor = UnifiedVFX.Eroica.Scarlet,
            SecondaryColor = UnifiedVFX.Eroica.Gold,
            TrailScale = 1.5f,
            TrailDensity = 3,
            SpawnThemeMusicNotes = (pos, count, radius) => ThemedParticles.EroicaMusicNotes(pos, count, radius),
            SwingSound = SoundID.Item1
        };
        
        public static MagnumSwingConfig MoonlightSonata => new MagnumSwingConfig
        {
            PrimaryColor = UnifiedVFX.MoonlightSonata.DarkPurple,
            SecondaryColor = UnifiedVFX.MoonlightSonata.LightBlue,
            TrailScale = 1.4f,
            TrailDensity = 3,
            SpawnThemeMusicNotes = (pos, count, radius) => ThemedParticles.MoonlightMusicNotes(pos, count, radius),
            SwingSound = SoundID.Item1 with { Pitch = 0.2f }
        };
        
        public static MagnumSwingConfig EnigmaVariations => new MagnumSwingConfig
        {
            PrimaryColor = UnifiedVFX.EnigmaVariations.Purple,
            SecondaryColor = UnifiedVFX.EnigmaVariations.GreenFlame,
            TrailScale = 1.5f,
            TrailDensity = 3,
            SpawnThemeMusicNotes = (pos, count, radius) => ThemedParticles.EnigmaMusicNotes(pos, count, radius),
            SwingSound = SoundID.Item1 with { Pitch = -0.2f }
        };
        
        // Dark colors for Fate nebular clouds
        private static readonly Color FateDarkBlack = new Color(15, 5, 20);
        private static readonly Color FateDarkPink = new Color(120, 30, 80);
        
        public static MagnumSwingConfig Fate => new MagnumSwingConfig
        {
            PrimaryColor = FateDarkPink,
            SecondaryColor = FateDarkBlack,
            TrailScale = 1.8f,
            TrailDensity = 5,
            SpawnThemeMusicNotes = (pos, count, radius) => ThemedParticles.FateMusicNotes(pos, count, radius),
            SwingSound = SoundID.Item1 with { Pitch = -0.3f },
            SpawnCustomTrailEffect = SpawnFateNebularClouds
        };
        
        /// <summary>
        /// Spawns dark black and dark pink nebular clouds around Fate weapon swings
        /// </summary>
        private static void SpawnFateNebularClouds(Vector2 position, float progress, float scale)
        {
            // Spawn heavy dark smoke particles as nebular clouds
            // Dark black clouds
            if (Main.rand.NextBool(2))
            {
                Vector2 cloudVel = Main.rand.NextVector2Circular(3f, 3f);
                cloudVel.Y -= 1f; // Slight upward drift
                
                var blackCloud = new HeavySmokeParticle(
                    position + Main.rand.NextVector2Circular(15f, 15f),
                    cloudVel,
                    FateDarkBlack,
                    Main.rand.Next(35, 55),
                    0.5f * scale,
                    0.9f * scale,
                    0.015f,
                    true
                );
                MagnumParticleHandler.SpawnParticle(blackCloud);
            }
            
            // Dark pink clouds
            if (Main.rand.NextBool(2))
            {
                Vector2 cloudVel = Main.rand.NextVector2Circular(2.5f, 2.5f);
                cloudVel.Y -= 0.8f;
                
                var pinkCloud = new HeavySmokeParticle(
                    position + Main.rand.NextVector2Circular(12f, 12f),
                    cloudVel,
                    FateDarkPink,
                    Main.rand.Next(30, 50),
                    0.4f * scale,
                    0.8f * scale,
                    0.018f,
                    true
                );
                MagnumParticleHandler.SpawnParticle(pinkCloud);
            }
            
            // Occasional brighter pink accent
            if (Main.rand.NextBool(4))
            {
                Color brightPink = new Color(180, 50, 100);
                var glow = new GenericGlowParticle(
                    position + Main.rand.NextVector2Circular(8f, 8f),
                    Main.rand.NextVector2Circular(2f, 2f),
                    brightPink * 0.7f,
                    0.3f * scale,
                    25,
                    true
                );
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Dark ambient glow
            if (Main.rand.NextBool(3))
            {
                Color mixedColor = Color.Lerp(FateDarkBlack, FateDarkPink, progress);
                CustomParticles.GenericFlare(position, mixedColor * 0.5f, 0.4f * scale, 18);
            }
        }
        
        /// <summary>
        /// Creates a custom swing config with specified colors
        /// </summary>
        public static MagnumSwingConfig Custom(Color primary, Color secondary, 
            Action<Vector2, int, float> musicNotes = null)
        {
            return new MagnumSwingConfig
            {
                PrimaryColor = primary,
                SecondaryColor = secondary,
                SpawnThemeMusicNotes = musicNotes
            };
        }
    }
}
