using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.GameContent;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;

// Dynamic particle effects for aesthetically pleasing animations
using static MagnumOpus.Common.Systems.DynamicParticleEffects;

namespace MagnumOpus.Content.ClairDeLune.Projectiles
{
    #region Theme Colors
    
    /// <summary>
    /// Clair de Lune theme color palette - FINAL BOSS TIER
    /// Clockwork precision meets moonlit dreams and temporal power
    /// Theme: Shattered time, brass mechanisms, crystal light, crimson energy
    /// </summary>
    public static class ClairDeLuneColors
    {
        // === PRIMARY PALETTE ===
        public static readonly Color DarkGray = new Color(58, 58, 58);           // #3A3A3A - Clockwork steel
        public static readonly Color Crimson = new Color(220, 20, 60);           // #DC143C - Temporal energy
        public static readonly Color Crystal = new Color(224, 224, 224);         // #E0E0E0 - Shattered time crystal
        public static readonly Color Brass = new Color(205, 127, 50);            // #CD7F32 - Clockwork gears
        
        // === ACCENT COLORS ===
        public static readonly Color MoonlightSilver = new Color(192, 192, 220); // Lunar reflection
        public static readonly Color DeepCrimson = new Color(139, 0, 0);         // Blood of time
        public static readonly Color ElectricBlue = new Color(70, 130, 180);     // Lightning core
        public static readonly Color BrightWhite = new Color(255, 255, 255);     // Crystal core brilliance
        public static readonly Color GearGold = new Color(218, 165, 32);         // Polished mechanisms
        public static readonly Color VoidBlack = new Color(15, 15, 20);          // Temporal void
        public static readonly Color LightningPurple = new Color(147, 112, 219); // Arcane discharge
        
        /// <summary>
        /// Get gradient color cycling through the temporal palette
        /// </summary>
        public static Color GetGradient(float progress)
        {
            // Full clockwork-temporal cycle: Brass → Crystal → Crimson → Moonlight → Brass
            if (progress < 0.25f)
                return Color.Lerp(Brass, Crystal, progress * 4f);
            else if (progress < 0.5f)
                return Color.Lerp(Crystal, Crimson, (progress - 0.25f) * 4f);
            else if (progress < 0.75f)
                return Color.Lerp(Crimson, MoonlightSilver, (progress - 0.5f) * 4f);
            else
                return Color.Lerp(MoonlightSilver, Brass, (progress - 0.75f) * 4f);
        }
        
        /// <summary>
        /// Get lightning gradient - Electric blue → Purple → Crimson
        /// </summary>
        public static Color GetLightningGradient(float progress)
        {
            if (progress < 0.33f)
                return Color.Lerp(ElectricBlue, LightningPurple, progress * 3f);
            else if (progress < 0.66f)
                return Color.Lerp(LightningPurple, Crimson, (progress - 0.33f) * 3f);
            else
                return Color.Lerp(Crimson, BrightWhite, (progress - 0.66f) * 3f);
        }
        
        /// <summary>
        /// Get crystal gradient - Pure white → Crystal → Moonlight Silver
        /// </summary>
        public static Color GetCrystalGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(BrightWhite, Crystal, progress * 2f);
            else
                return Color.Lerp(Crystal, MoonlightSilver, (progress - 0.5f) * 2f);
        }
        
        /// <summary>
        /// Get gear gradient - Brass → Gold → Crystal (polished mechanism shine)
        /// </summary>
        public static Color GetGearGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(Brass, GearGold, progress * 2f);
            else
                return Color.Lerp(GearGold, Crystal, (progress - 0.5f) * 2f);
        }
    }
    
    #endregion
    
    #region VFX Helpers
    
    /// <summary>
    /// Clair de Lune VFX System - FINAL BOSS TIER - MUST BE THE MOST SPECTACULAR
    /// Every effect uses multi-layered rendering with:
    /// - CLOCKWORK GEARS (spinning, orbiting, cascading)
    /// - LIGHTNING BURSTS (crackling, arcing, branching)
    /// - CRYSTAL SHARDS (shattering, glowing, reflecting)
    /// - TEMPORAL DISTORTION (time echoes, phase shifts)
    /// </summary>
    public static class ClairDeLuneVFX
    {
        // === TEXTURE PATHS ===
        private const string ClockworkGearLarge = "MagnumOpus/Assets/Particles/ClockworkGearLarge";
        private const string ClockworkGearSmall = "MagnumOpus/Assets/Particles/ClockworkGearSmall";
        private const string LightningBurst = "MagnumOpus/Assets/Particles/LightningBurst";
        private const string LightningBurstThick = "MagnumOpus/Assets/Particles/LightningBurstThick";
        private const string LightningStreak = "MagnumOpus/Assets/Particles/LightningStreak";
        private const string MediumCrystalShard = "MagnumOpus/Assets/Particles/MediumCrystalShard";
        private const string SmallCrystalShard = "MagnumOpus/Assets/Particles/SmallCrystalShard";
        
        // === CORE VISUAL IDENTITY ===
        // Layer 1: BRILLIANT WHITE core (temporal power center)
        // Layer 2: CRIMSON energy (time's blood)
        // Layer 3: BRASS clockwork (spinning gears)
        // Layer 4: CRYSTAL shards (shattered time)
        // Layer 5: LIGHTNING arcs (temporal discharge)
        
        #region Music Notes
        
        /// <summary>
        /// Spawn glowing music note with clockwork-temporal theme - OPTIMIZED
        /// Reduced bloom layers for proper sizing
        /// </summary>
        public static void SpawnMusicNote(Vector2 position, Vector2 velocity, Color baseColor, float scale = 0.5f)
        {
            // Subtle shimmer animation
            float shimmer = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.18f) * 0.12f;
            scale *= shimmer;
            
            // === SINGLE WHITE CORE BLOOM (BloomParticle already draws 4 internal layers) ===
            Color coreColor = ClairDeLuneColors.BrightWhite * 0.7f;
            coreColor.A = 0;
            var coreBloom = new BloomParticle(
                position, velocity * 0.9f, coreColor,
                scale * 0.25f, // REDUCED from multi-layer madness
                25
            );
            MagnumParticleHandler.SpawnParticle(coreBloom);
            
            // === SINGLE CRIMSON ACCENT BLOOM ===
            Color crimsonColor = ClairDeLuneColors.Crimson * 0.5f;
            crimsonColor.A = 0;
            var crimsonBloom = new BloomParticle(
                position + Main.rand.NextVector2Circular(2f, 2f),
                velocity * 0.75f, crimsonColor,
                scale * 0.2f, // REDUCED
                22
            );
            MagnumParticleHandler.SpawnParticle(crimsonBloom);
            
            // === SINGLE BRASS CLOCKWORK ACCENT ===
            var gear = new GenericGlowParticle(
                position + Main.rand.NextVector2Circular(5f, 5f),
                velocity * 0.5f + Main.rand.NextVector2Circular(1f, 1f),
                ClairDeLuneColors.Brass * 0.5f,
                scale * 0.2f, // REDUCED
                20,
                true
            );
            MagnumParticleHandler.SpawnParticle(gear);
            
            // === CRYSTAL SPARKLE (single) ===
            var sparkle = new SparkleParticle(
                position + Main.rand.NextVector2Circular(6f, 6f),
                velocity * 0.6f + Main.rand.NextVector2Circular(1.5f, 1.5f),
                Color.Lerp(ClairDeLuneColors.Crystal, ClairDeLuneColors.MoonlightSilver, Main.rand.NextFloat(0.3f, 0.8f)),
                0.3f, // REDUCED
                22
            );
            MagnumParticleHandler.SpawnParticle(sparkle);
            
            // === LIGHTNING MICRO-ARC (occasional) ===
            if (Main.rand.NextBool(4))
            {
                var lightning = new GenericGlowParticle(
                    position + Main.rand.NextVector2Circular(4f, 4f),
                    velocity * 0.8f + Main.rand.NextVector2Circular(2f, 2f),
                    ClairDeLuneColors.GetLightningGradient(Main.rand.NextFloat()),
                    0.18f, // REDUCED
                    10,
                    true
                );
                MagnumParticleHandler.SpawnParticle(lightning);
            }
            
            // Vanilla dust (reduced count)
            Dust dust = Dust.NewDustPerfect(
                position + Main.rand.NextVector2Circular(3f, 3f),
                DustID.Electric,
                velocity + Main.rand.NextVector2Circular(2f, 2f),
                0, ClairDeLuneColors.ElectricBlue, 1.2f
            );
            dust.noGravity = true;
        }
        
        /// <summary>
        /// Music note burst - FINAL BOSS VERSION
        /// </summary>
        public static void MusicNoteBurst(Vector2 position, int count, float speed, float scale = 0.9f)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 noteVel = angle.ToRotationVector2() * speed * Main.rand.NextFloat(0.8f, 1.2f);
                Color noteColor = ClairDeLuneColors.GetCrystalGradient(Main.rand.NextFloat());
                SpawnMusicNote(position, noteVel, noteColor, scale);
            }
        }
        
        #endregion
        
        #region Clockwork Effects
        
        /// <summary>
        /// Spawn a SPINNING CLOCKWORK GEAR particle
        /// NOW ACTUALLY USES the custom gear texture with rotation animation!
        /// </summary>
        public static void SpawnClockworkGear(Vector2 position, Vector2 velocity, bool large = false, float scale = 1f)
        {
            // === LOAD AND USE ACTUAL GEAR TEXTURES! ===
            string texturePath = large ? ClockworkGearLarge : ClockworkGearSmall;
            Texture2D gearTexture = ModContent.Request<Texture2D>(texturePath, ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            
            // Calculate spin speed based on velocity (faster movement = faster spin)
            float spin = (large ? 0.04f : 0.06f) * (velocity.Length() > 0 ? Math.Sign(velocity.X + velocity.Y) : 1);
            
            // Create the ACTUAL gear particle with the gear texture
            var gearParticle = new TexturedParticle(
                position,
                velocity * 0.9f,
                gearTexture,
                ClairDeLuneColors.Brass,
                scale * (large ? 0.35f : 0.25f), // PROPER sizing
                35,
                Main.rand.NextFloat(MathHelper.TwoPi), // Random initial rotation
                spin
            );
            MagnumParticleHandler.SpawnParticle(gearParticle);
            
            // Single subtle bloom behind the gear
            Color gearColor = ClairDeLuneColors.GearGold * 0.4f;
            gearColor.A = 0;
            var gearBloom = new BloomParticle(
                position,
                velocity * 0.85f,
                gearColor,
                scale * (large ? 0.2f : 0.15f), // Subtle glow
                30
            );
            MagnumParticleHandler.SpawnParticle(gearBloom);
            
            // Occasional sparkle accent
            if (Main.rand.NextBool(3))
            {
                var sparkle = new SparkleParticle(
                    position + Main.rand.NextVector2Circular(4f, 4f),
                    velocity * 0.5f + Main.rand.NextVector2Circular(1f, 1f),
                    ClairDeLuneColors.GearGold,
                    0.2f,
                    18
                );
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
        }
        
        /// <summary>
        /// Clockwork gear cascade - Multiple ACTUAL gears erupting in a mechanical burst
        /// </summary>
        public static void ClockworkGearCascade(Vector2 position, int count, float speed, float scale = 1f)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.25f, 0.25f);
                Vector2 vel = angle.ToRotationVector2() * speed * Main.rand.NextFloat(0.7f, 1.3f);
                bool isLarge = i % 3 == 0;
                float gearScale = scale * Main.rand.NextFloat(0.8f, 1.2f);
                
                SpawnClockworkGear(position + Main.rand.NextVector2Circular(6f, 6f), vel, isLarge, gearScale);
            }
            
            // Central mechanism flash (single bloom, not 3)
            Color flashColor = ClairDeLuneColors.BrightWhite * 0.7f;
            flashColor.A = 0;
            var flash = new BloomParticle(position, Vector2.Zero, flashColor, scale * 0.3f, 15);
            MagnumParticleHandler.SpawnParticle(flash);
            
            // Gear dust (reduced)
            for (int i = 0; i < Math.Min(count, 4); i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(speed * 0.8f, speed * 0.8f);
                Dust dust = Dust.NewDustPerfect(position, DustID.Enchanted_Gold, dustVel, 0, ClairDeLuneColors.Brass, 1.1f);
                dust.noGravity = true;
            }
        }
        
        /// <summary>
        /// Orbiting clockwork gears around a point - NOW WITH ACTUAL GEAR TEXTURES!
        /// </summary>
        public static void OrbitingGears(Vector2 center, float radius, int gearCount, float rotationOffset = 0f, float scale = 0.6f)
        {
            float baseAngle = Main.GameUpdateCount * 0.03f + rotationOffset;
            
            for (int i = 0; i < gearCount; i++)
            {
                float angle = baseAngle + MathHelper.TwoPi * i / gearCount;
                Vector2 gearPos = center + angle.ToRotationVector2() * radius;
                Vector2 tangentVel = (angle + MathHelper.PiOver2).ToRotationVector2() * 0.3f;
                
                // Alternating large and small gears - NOW USING ACTUAL TEXTURES
                bool isLarge = i % 2 == 0;
                string texturePath = isLarge ? ClockworkGearLarge : ClockworkGearSmall;
                Texture2D gearTexture = ModContent.Request<Texture2D>(texturePath, ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                
                // Spin direction alternates for meshed gear effect
                float spin = (i % 2 == 0 ? 0.05f : -0.05f);
                
                var gearParticle = new TexturedParticle(
                    gearPos,
                    tangentVel,
                    gearTexture,
                    ClairDeLuneColors.GetGearGradient((float)i / gearCount),
                    scale * (isLarge ? 0.25f : 0.18f),
                    6, // Short lifetime for orbiting effect
                    baseAngle + i * 0.5f, // Staggered rotation
                    spin
                );
                MagnumParticleHandler.SpawnParticle(gearParticle);
            }
        }
        
        #endregion
        
        #region Lightning Effects
        
        /// <summary>
        /// Spawn a LIGHTNING BURST effect - Crackling temporal energy
        /// REDUCED: BloomParticle already has 4 internal layers, so we only need 1-2 blooms
        /// </summary>
        public static void SpawnLightningBurst(Vector2 position, Vector2 velocity, bool thick = false, float scale = 1f)
        {
            // === SINGLE LIGHTNING BLOOM (BloomParticle draws 4 internal layers) ===
            Color lightningColor = ClairDeLuneColors.ElectricBlue * 0.8f;
            lightningColor.A = 0;
            
            var lightningBloom = new BloomParticle(
                position,
                velocity * 0.9f,
                lightningColor,
                scale * (thick ? 0.3f : 0.2f), // REDUCED scales
                20
            );
            MagnumParticleHandler.SpawnParticle(lightningBloom);
            
            // Core electric sparkle
            var core = new SparkleParticle(
                position,
                velocity,
                ClairDeLuneColors.BrightWhite,
                scale * (thick ? 0.35f : 0.25f),
                18
            );
            MagnumParticleHandler.SpawnParticle(core);
            
            // Branching mini-arcs (reduced count)
            int arcCount = thick ? 3 : 2;
            for (int i = 0; i < arcCount; i++)
            {
                float arcAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 arcVel = arcAngle.ToRotationVector2() * Main.rand.NextFloat(3f, 5f);
                
                var arc = new GenericGlowParticle(
                    position + Main.rand.NextVector2Circular(4f, 4f),
                    velocity * 0.4f + arcVel,
                    ClairDeLuneColors.LightningPurple,
                    0.15f * scale,
                    10,
                    true
                );
                MagnumParticleHandler.SpawnParticle(arc);
            }
            
            // Electric dust (reduced)
            for (int i = 0; i < (thick ? 3 : 2); i++)
            {
                Dust dust = Dust.NewDustPerfect(
                    position + Main.rand.NextVector2Circular(5f, 5f),
                    DustID.Electric,
                    velocity * 0.5f + Main.rand.NextVector2Circular(3f, 3f),
                    0, ClairDeLuneColors.ElectricBlue, 1.1f
                );
                dust.noGravity = true;
            }
        }
        
        /// <summary>
        /// Lightning arc chain between two points
        /// </summary>
        public static void LightningArc(Vector2 start, Vector2 end, int segments = 8, float amplitude = 15f, float scale = 1f)
        {
            Vector2 direction = end - start;
            float length = direction.Length();
            direction.Normalize();
            Vector2 perpendicular = new Vector2(-direction.Y, direction.X);
            
            Vector2 lastPos = start;
            
            for (int i = 0; i <= segments; i++)
            {
                float progress = (float)i / segments;
                Vector2 basePos = Vector2.Lerp(start, end, progress);
                
                // Add zigzag offset
                float offset = (float)Math.Sin(progress * MathHelper.Pi * 3f + Main.GameUpdateCount * 0.3f) * amplitude;
                offset *= (1f - Math.Abs(progress - 0.5f) * 2f); // Reduce at endpoints
                Vector2 pos = basePos + perpendicular * offset;
                
                // Segment glow
                Color segmentColor = ClairDeLuneColors.GetLightningGradient(progress);
                segmentColor.A = 0;
                
                var segmentGlow = new BloomParticle(pos, Vector2.Zero, segmentColor * 0.7f, 0.3f * scale, 8);
                MagnumParticleHandler.SpawnParticle(segmentGlow);
                
                // Mini sparks between segments
                if (i > 0 && Main.rand.NextBool(2))
                {
                    Vector2 sparkPos = Vector2.Lerp(lastPos, pos, Main.rand.NextFloat());
                    var spark = new SparkleParticle(sparkPos, Main.rand.NextVector2Circular(2f, 2f), ClairDeLuneColors.BrightWhite, 0.3f, 6);
                    MagnumParticleHandler.SpawnParticle(spark);
                }
                
                lastPos = pos;
            }
            
            // Start and end flares
            SpawnLightningBurst(start, direction * 2f, false, scale * 0.7f);
            SpawnLightningBurst(end, -direction * 2f, false, scale * 0.7f);
        }
        
        /// <summary>
        /// MASSIVE lightning strike explosion - REDUCED for proper sizing
        /// </summary>
        public static void LightningStrikeExplosion(Vector2 position, float scale = 1f)
        {
            // === WHITE CORE (single bloom, not 6!) ===
            Color coreColor = ClairDeLuneColors.BrightWhite * 0.9f;
            coreColor.A = 0;
            var coreFlare = new BloomParticle(position, Vector2.Zero, coreColor, scale * 0.4f, 20);
            MagnumParticleHandler.SpawnParticle(coreFlare);
            
            // === LIGHTNING ACCENT (single bloom) ===
            Color lightningColor = ClairDeLuneColors.ElectricBlue * 0.7f;
            lightningColor.A = 0;
            var lightningFlare = new BloomParticle(position, Vector2.Zero, lightningColor, scale * 0.35f, 18);
            MagnumParticleHandler.SpawnParticle(lightningFlare);
            
            // === RADIAL LIGHTNING ARCS (reduced count) ===
            int arcCount = (int)(6 * scale);
            for (int i = 0; i < arcCount; i++)
            {
                float angle = MathHelper.TwoPi * i / arcCount;
                float arcLength = Main.rand.NextFloat(40f, 70f) * scale;
                Vector2 arcEnd = position + angle.ToRotationVector2() * arcLength;
                
                LightningArc(position, arcEnd, 4, 6f * scale, scale * 0.4f);
            }
            
            // === EXPANDING ELECTRIC RINGS (reduced from 6 to 3) ===
            for (int i = 0; i < 3; i++)
            {
                Color ringColor = ClairDeLuneColors.GetLightningGradient(i / 3f);
                ringColor.A = 0;
                
                var ring = new BloomRingParticle(position, Vector2.Zero, ringColor * 0.6f, (0.2f + i * 0.1f) * scale, 18 + i * 3);
                MagnumParticleHandler.SpawnParticle(ring);
            }
            
            // === ELECTRIC DUST (reduced) ===
            for (int i = 0; i < (int)(10 * scale); i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(10f, 10f) * scale;
                Dust dust = Dust.NewDustPerfect(position, DustID.Electric, vel, 0, ClairDeLuneColors.ElectricBlue, 1.3f);
                dust.noGravity = true;
            }
            
            // Lighting
            Lighting.AddLight(position, ClairDeLuneColors.ElectricBlue.ToVector3() * scale * 1.5f);
        }
        
        #endregion
        
        #region Crystal Effects
        
        /// <summary>
        /// Spawn a CRYSTAL SHARD particle - REDUCED bloom layers
        /// </summary>
        public static void SpawnCrystalShard(Vector2 position, Vector2 velocity, bool medium = false, float scale = 1f)
        {
            // === SINGLE CRYSTAL BLOOM (already draws 4 internal layers) ===
            Color crystalColor = ClairDeLuneColors.Crystal * 0.75f;
            crystalColor.A = 0;
            
            var crystalBloom = new BloomParticle(
                position,
                velocity * 0.9f,
                crystalColor,
                scale * (medium ? 0.25f : 0.18f), // REDUCED
                28
            );
            MagnumParticleHandler.SpawnParticle(crystalBloom);
            
            // Core shard sparkle
            var core = new SparkleParticle(
                position,
                velocity,
                ClairDeLuneColors.BrightWhite,
                scale * (medium ? 0.3f : 0.22f),
                25
            );
            MagnumParticleHandler.SpawnParticle(core);
            
            // Prismatic sparkles (reduced count)
            for (int i = 0; i < (medium ? 2 : 1); i++)
            {
                float hue = Main.rand.NextFloat();
                Color prismColor = Main.hslToRgb(hue, 0.6f, 0.85f);
                
                var sparkle = new SparkleParticle(
                    position + Main.rand.NextVector2Circular(5f, 5f),
                    velocity * 0.5f + Main.rand.NextVector2Circular(1.5f, 1.5f),
                    prismColor,
                    0.2f,
                    18
                );
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
        }
        
        /// <summary>
        /// Crystal shatter burst - Time shattering into fragments - REDUCED
        /// </summary>
        public static void CrystalShatterBurst(Vector2 position, int count, float speed, float scale = 1f)
        {
            // Reduce count for performance
            int actualCount = Math.Min(count, 8);
            
            for (int i = 0; i < actualCount; i++)
            {
                float angle = MathHelper.TwoPi * i / actualCount + Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 vel = angle.ToRotationVector2() * speed * Main.rand.NextFloat(0.6f, 1.2f);
                bool isMedium = i % 3 == 0;
                float shardScale = scale * Main.rand.NextFloat(0.7f, 1.1f);
                
                SpawnCrystalShard(position + Main.rand.NextVector2Circular(5f, 5f), vel, isMedium, shardScale);
            }
            
            // Central crystal flash (single bloom, not 4!)
            Color flashColor = ClairDeLuneColors.BrightWhite * 0.8f;
            flashColor.A = 0;
            var flash = new BloomParticle(position, Vector2.Zero, flashColor, scale * 0.3f, 18);
            MagnumParticleHandler.SpawnParticle(flash);
            
            // Crystal dust (reduced)
            for (int i = 0; i < Math.Min(count, 6); i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(speed * 0.8f, speed * 0.8f);
                Dust dust = Dust.NewDustPerfect(position, DustID.GemDiamond, dustVel, 0, ClairDeLuneColors.Crystal, 1.1f);
                dust.noGravity = true;
            }
        }
        
        /// <summary>
        /// Prismatic crystal refraction effect - REDUCED
        /// </summary>
        public static void CrystalRefraction(Vector2 position, float scale = 1f)
        {
            // Rainbow prismatic burst (reduced from 12 to 6)
            for (int i = 0; i < 6; i++)
            {
                float hue = (float)i / 6f;
                Color prismColor = Main.hslToRgb(hue, 0.7f, 0.8f);
                prismColor.A = 0;
                
                float angle = MathHelper.TwoPi * i / 6f + Main.rand.NextFloat(-0.1f, 0.1f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 5f);
                
                // Use sparkle instead of bloom for prismatic effect
                var prism = new SparkleParticle(position, vel, prismColor, 0.25f * scale, 20);
                MagnumParticleHandler.SpawnParticle(prism);
            }
            
            // White core flash (single, not 3!)
            Color whiteCore = ClairDeLuneColors.BrightWhite * 0.8f;
            whiteCore.A = 0;
            var core = new BloomParticle(position, Vector2.Zero, whiteCore, 0.25f * scale, 15);
            MagnumParticleHandler.SpawnParticle(core);
        }
        
        #endregion
        
        #region Major Impact Effects
        
        /// <summary>
        /// TEMPORAL IMPACT - THE SIGNATURE CLAIR DE LUNE EFFECT - REDUCED VERSION
        /// Clockwork gears + Lightning + Crystal shards + Crimson energy
        /// Now properly sized and not overwhelming the screen!
        /// </summary>
        public static void TemporalImpact(Vector2 position, float scale = 1f)
        {
            // === SINGLE WHITE CORE (not 6!) ===
            Color coreColor = ClairDeLuneColors.BrightWhite * 0.9f;
            coreColor.A = 0;
            var coreFlare = new BloomParticle(position, Vector2.Zero, coreColor, scale * 0.4f, 22);
            MagnumParticleHandler.SpawnParticle(coreFlare);
            
            // === SINGLE CRIMSON ACCENT (not 5!) ===
            Color crimsonColor = ClairDeLuneColors.Crimson * 0.8f;
            crimsonColor.A = 0;
            var crimsonFlare = new BloomParticle(position, Vector2.Zero, crimsonColor, scale * 0.35f, 20);
            MagnumParticleHandler.SpawnParticle(crimsonFlare);
            
            // === CLOCKWORK GEAR CASCADE (reduced) ===
            ClockworkGearCascade(position, (int)(6 * scale), 7f * scale, scale * 0.8f);
            
            // === CRYSTAL SHATTER BURST (reduced) ===
            CrystalShatterBurst(position, (int)(6 * scale), 8f * scale, scale * 0.8f);
            
            // === LIGHTNING DISCHARGE (reduced) ===
            int lightningArcs = (int)(4 * scale);
            for (int i = 0; i < lightningArcs; i++)
            {
                float angle = MathHelper.TwoPi * i / lightningArcs + Main.rand.NextFloat(-0.2f, 0.2f);
                float arcLength = Main.rand.NextFloat(30f, 60f) * scale;
                Vector2 arcEnd = position + angle.ToRotationVector2() * arcLength;
                
                LightningArc(position, arcEnd, 3, 6f * scale, scale * 0.4f);
            }
            
            // === EXPANDING TEMPORAL RINGS (reduced from 10 to 4) ===
            for (int i = 0; i < 4; i++)
            {
                float progress = i / 4f;
                Color ringColor = ClairDeLuneColors.GetGradient(progress);
                ringColor.A = 0;
                
                var ring = new BloomRingParticle(
                    position, Vector2.Zero,
                    ringColor * 0.6f,
                    (0.15f + i * 0.08f) * scale, // REDUCED scales
                    20 + i * 3
                );
                MagnumParticleHandler.SpawnParticle(ring);
            }
            
            // === RADIAL PARTICLE SPRAY (reduced from 30 to 12) ===
            int particleCount = (int)(12 * scale);
            for (int i = 0; i < particleCount; i++)
            {
                float angle = MathHelper.TwoPi * i / particleCount;
                float speed = Main.rand.NextFloat(5f, 10f) * scale;
                Vector2 vel = angle.ToRotationVector2() * speed;
                
                float colorProgress = (float)i / particleCount;
                Color particleColor = ClairDeLuneColors.GetGradient(colorProgress);
                
                // Use sparkle for radial spray instead of glow
                var particle = new SparkleParticle(position, vel, particleColor, 0.25f * scale, 25);
                MagnumParticleHandler.SpawnParticle(particle);
            }
            
            // === VANILLA DUST DENSITY LAYER (reduced) ===
            // Electric dust
            for (int i = 0; i < (int)(8 * scale); i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(8f, 8f) * scale;
                Dust dust = Dust.NewDustPerfect(position, DustID.Electric, vel, 0, ClairDeLuneColors.ElectricBlue, 1.2f);
                dust.noGravity = true;
            }
            
            // Gold sparkle dust
            for (int i = 0; i < (int)(6 * scale); i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f) * scale;
                Dust dust = Dust.NewDustPerfect(position, DustID.Enchanted_Gold, vel, 0, ClairDeLuneColors.Brass, 1.1f);
                dust.noGravity = true;
            }
            
            // === MUSIC NOTES (reduced) ===
            MusicNoteBurst(position, (int)(4 * scale), 4f * scale, 0.7f);
            
            // Lighting (reduced)
            Lighting.AddLight(position, ClairDeLuneColors.BrightWhite.ToVector3() * scale * 1.2f);
        }
        
        /// <summary>
        /// Death explosion - Spectacle for kill effects - REDUCED
        /// </summary>
        public static void TemporalDeathExplosion(Vector2 position, float scale = 1.5f)
        {
            // Use the full impact (already reduced)
            TemporalImpact(position, scale);
            
            // Additional lightning (uses reduced version)
            LightningStrikeExplosion(position, scale * 0.8f);
            
            // Extra gear cascade (reduced)
            ClockworkGearCascade(position, 8, 8f * scale, scale * 0.8f);
            
            // Extra crystal shatter (reduced)
            CrystalShatterBurst(position, 8, 10f * scale, scale * 0.8f);
            
            // Music note spiral (reduced from 3 rings to 2)
            for (int ring = 0; ring < 2; ring++)
            {
                float ringRadius = (25f + ring * 20f) * scale;
                int notesInRing = 4 + ring * 2;
                
                for (int i = 0; i < notesInRing; i++)
                {
                    float angle = MathHelper.TwoPi * i / notesInRing + ring * 0.3f;
                    Vector2 notePos = position + angle.ToRotationVector2() * ringRadius;
                    Vector2 noteVel = angle.ToRotationVector2() * (3f + ring * 1.5f);
                    
                    SpawnMusicNote(notePos, noteVel, ClairDeLuneColors.GetGradient(Main.rand.NextFloat()), 0.7f * scale);
                }
            }
        }
        
        #endregion
        
        #region Trail Effects
        
        /// <summary>
        /// TEMPORAL TRAIL - Multi-layered with gears, lightning, crystals
        /// </summary>
        public static void TemporalTrail(Vector2 position, Vector2 velocity, float intensity = 1f)
        {
            // === BRIGHT WHITE CORE GLOW ===
            if (Main.rand.NextBool(2))
            {
                Color coreColor = ClairDeLuneColors.BrightWhite * 0.8f;
                coreColor.A = 0;
                var core = new GenericGlowParticle(
                    position,
                    -velocity * 0.06f,
                    coreColor,
                    0.28f * intensity,
                    14,
                    true
                );
                MagnumParticleHandler.SpawnParticle(core);
            }
            
            // === CRIMSON TEMPORAL ENERGY ===
            if (Main.rand.NextBool(2))
            {
                Color crimsonColor = Color.Lerp(ClairDeLuneColors.Crimson, ClairDeLuneColors.DeepCrimson, Main.rand.NextFloat());
                var crimson = new GenericGlowParticle(
                    position + Main.rand.NextVector2Circular(7f, 7f),
                    -velocity * 0.12f + Main.rand.NextVector2Circular(1.8f, 1.8f),
                    crimsonColor,
                    0.38f * intensity,
                    24,
                    true
                );
                MagnumParticleHandler.SpawnParticle(crimson);
            }
            
            // === CLOCKWORK GEAR PARTICLES ===
            if (Main.rand.NextBool(3))
            {
                Color gearColor = ClairDeLuneColors.GetGearGradient(Main.rand.NextFloat());
                var gear = new GenericGlowParticle(
                    position + Main.rand.NextVector2Circular(10f, 10f),
                    -velocity * 0.08f + Main.rand.NextVector2Circular(2.5f, 2.5f),
                    gearColor * 0.75f,
                    0.32f * intensity,
                    20,
                    true
                );
                MagnumParticleHandler.SpawnParticle(gear);
            }
            
            // === CRYSTAL SHARD SPARKLES ===
            if (Main.rand.NextBool(3))
            {
                var sparkle = new SparkleParticle(
                    position + Main.rand.NextVector2Circular(6f, 6f),
                    -velocity * 0.05f,
                    Color.Lerp(ClairDeLuneColors.Crystal, ClairDeLuneColors.MoonlightSilver, Main.rand.NextFloat()),
                    0.38f * intensity,
                    20
                );
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // === LIGHTNING MICRO-ARCS ===
            if (Main.rand.NextBool(5))
            {
                var lightning = new GenericGlowParticle(
                    position + Main.rand.NextVector2Circular(8f, 8f),
                    -velocity * 0.15f + Main.rand.NextVector2Circular(4f, 4f),
                    ClairDeLuneColors.GetLightningGradient(Main.rand.NextFloat()),
                    0.25f * intensity,
                    10,
                    true
                );
                MagnumParticleHandler.SpawnParticle(lightning);
            }
            
            // === VANILLA DUST ===
            // Electric dust
            if (Main.rand.NextBool(2))
            {
                Dust electric = Dust.NewDustPerfect(
                    position + Main.rand.NextVector2Circular(8f, 8f),
                    DustID.Electric,
                    -velocity * 0.1f + Main.rand.NextVector2Circular(3.5f, 3.5f),
                    0, ClairDeLuneColors.ElectricBlue, 1.5f * intensity
                );
                electric.noGravity = true;
            }
            
            // Gold dust
            if (Main.rand.NextBool(3))
            {
                Dust gold = Dust.NewDustPerfect(
                    position + Main.rand.NextVector2Circular(8f, 8f),
                    DustID.Enchanted_Gold,
                    -velocity * 0.1f + Main.rand.NextVector2Circular(2.5f, 2.5f),
                    0, ClairDeLuneColors.Brass, 1.3f * intensity
                );
                gold.noGravity = true;
            }
            
            // === MUSIC NOTE (Less frequent but visible) ===
            if (Main.rand.NextBool(12))
            {
                SpawnMusicNote(position, -velocity * 0.1f, ClairDeLuneColors.Crystal, 0.78f * intensity);
            }
        }
        
        /// <summary>
        /// Heavy temporal trail for melee weapons
        /// </summary>
        public static void HeavyTemporalTrail(Vector2 position, Vector2 velocity, float intensity = 1f)
        {
            // Call regular trail twice for density
            TemporalTrail(position, velocity, intensity);
            TemporalTrail(position + Main.rand.NextVector2Circular(5f, 5f), velocity * 0.9f, intensity * 0.8f);
            
            // Extra gears
            if (Main.rand.NextBool(4))
            {
                SpawnClockworkGear(position, -velocity * 0.3f, Main.rand.NextBool(), 0.6f * intensity);
            }
            
            // Extra crystals
            if (Main.rand.NextBool(4))
            {
                SpawnCrystalShard(position, -velocity * 0.25f, Main.rand.NextBool(), 0.55f * intensity);
            }
            
            // Extra lightning burst
            if (Main.rand.NextBool(8))
            {
                SpawnLightningBurst(position, -velocity * 0.2f, false, 0.5f * intensity);
            }
        }
        
        #endregion
        
        #region Charge-Up Effects
        
        /// <summary>
        /// Temporal charge-up effect - Converging gears, lightning, crystals
        /// </summary>
        public static void TemporalChargeUp(Vector2 position, float progress, float scale = 1f)
        {
            float intensity = progress * scale;
            float radius = 120f * (1f - progress * 0.7f); // Converges inward
            
            // Converging clockwork gears
            int gearCount = (int)(8 + progress * 8);
            float gearRotation = Main.GameUpdateCount * 0.05f;
            
            for (int i = 0; i < gearCount; i++)
            {
                float angle = MathHelper.TwoPi * i / gearCount + gearRotation;
                Vector2 gearPos = position + angle.ToRotationVector2() * radius;
                Vector2 gearVel = (position - gearPos).SafeNormalize(Vector2.Zero) * 2f;
                
                Color gearColor = ClairDeLuneColors.GetGearGradient((float)i / gearCount);
                gearColor.A = 0;
                
                var gearGlow = new BloomParticle(gearPos, gearVel, gearColor * intensity, 0.3f * intensity, 10);
                MagnumParticleHandler.SpawnParticle(gearGlow);
            }
            
            // Converging lightning sparks
            if (progress > 0.3f && Main.rand.NextBool(3))
            {
                float sparkAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 sparkPos = position + sparkAngle.ToRotationVector2() * radius * 0.8f;
                
                SpawnLightningBurst(sparkPos, (position - sparkPos).SafeNormalize(Vector2.Zero) * 3f, false, 0.5f * intensity);
            }
            
            // Converging crystal shards
            if (progress > 0.5f && Main.rand.NextBool(4))
            {
                float crystalAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 crystalPos = position + crystalAngle.ToRotationVector2() * radius * 0.6f;
                
                SpawnCrystalShard(crystalPos, (position - crystalPos).SafeNormalize(Vector2.Zero) * 2.5f, false, 0.4f * intensity);
            }
            
            // Central building energy
            if (Main.rand.NextBool(3))
            {
                Color coreColor = ClairDeLuneColors.GetGradient(Main.GameUpdateCount * 0.02f % 1f);
                coreColor.A = 0;
                
                var core = new BloomParticle(position + Main.rand.NextVector2Circular(10f, 10f), Vector2.Zero, coreColor * intensity * 0.8f, 0.4f * intensity, 15);
                MagnumParticleHandler.SpawnParticle(core);
            }
            
            // Orbiting gears at higher charge
            if (progress > 0.6f)
            {
                OrbitingGears(position, 40f * (1f - progress * 0.5f), 4, Main.GameUpdateCount * 0.04f, intensity * 0.7f);
            }
            
            // Screen-wide effect at near-full charge
            if (progress > 0.9f)
            {
                Lighting.AddLight(position, ClairDeLuneColors.Crimson.ToVector3() * intensity * 1.5f);
            }
        }
        
        /// <summary>
        /// Charge release burst - When charge is complete
        /// </summary>
        public static void TemporalChargeRelease(Vector2 position, float scale = 1f)
        {
            // Massive central flash
            for (int i = 0; i < 8; i++)
            {
                float flashScale = scale * (2f - i * 0.15f);
                Color flashColor = Color.Lerp(ClairDeLuneColors.BrightWhite, ClairDeLuneColors.Crimson, i / 8f);
                flashColor.A = 0;
                
                var flash = new BloomParticle(position, Vector2.Zero, flashColor * (0.95f / (i + 1)), flashScale, 18 + i * 2);
                MagnumParticleHandler.SpawnParticle(flash);
            }
            
            // Radial gear explosion
            ClockworkGearCascade(position, 14, 15f * scale, scale * 1.2f);
            
            // Lightning discharge burst
            LightningStrikeExplosion(position, scale * 0.8f);
            
            // Crystal shatter wave
            CrystalShatterBurst(position, 18, 14f * scale, scale);
            
            // Massive music note burst
            MusicNoteBurst(position, 10, 8f * scale, 1f);
            
            // Screen shake worthy lighting
            Lighting.AddLight(position, ClairDeLuneColors.BrightWhite.ToVector3() * scale * 3f);
        }
        
        #endregion
        
        #region Aura Effects
        
        /// <summary>
        /// Ambient temporal aura - For held weapons and accessories
        /// </summary>
        public static void TemporalAura(Vector2 center, float radius, float scale = 0.5f)
        {
            // Orbiting gears at various radii
            if (Main.rand.NextBool(5))
            {
                OrbitingGears(center, radius * 0.8f, 4, Main.rand.NextFloat(MathHelper.TwoPi), scale);
            }
            
            // Ambient lightning crackles
            if (Main.rand.NextBool(12))
            {
                float sparkAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 sparkPos = center + sparkAngle.ToRotationVector2() * radius * Main.rand.NextFloat(0.5f, 1f);
                
                var lightning = new GenericGlowParticle(
                    sparkPos,
                    Main.rand.NextVector2Circular(2f, 2f),
                    ClairDeLuneColors.GetLightningGradient(Main.rand.NextFloat()),
                    0.2f * scale,
                    10,
                    true
                );
                MagnumParticleHandler.SpawnParticle(lightning);
            }
            
            // Floating crystal sparkles
            if (Main.rand.NextBool(8))
            {
                Vector2 crystalPos = center + Main.rand.NextVector2Circular(radius, radius);
                
                var sparkle = new SparkleParticle(
                    crystalPos,
                    Main.rand.NextVector2Circular(1f, 1f),
                    ClairDeLuneColors.Crystal,
                    0.3f * scale,
                    20
                );
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // Occasional music note
            if (Main.rand.NextBool(20))
            {
                Vector2 notePos = center + Main.rand.NextVector2Circular(radius * 0.6f, radius * 0.6f);
                SpawnMusicNote(notePos, Main.rand.NextVector2Circular(1f, 1f), ClairDeLuneColors.GetGradient(Main.rand.NextFloat()), 0.6f * scale);
            }
        }
        
        #endregion
    }
    
    #endregion
}
