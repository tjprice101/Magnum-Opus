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
        /// Spawn MASSIVE glowing music note with clockwork-temporal theme
        /// Multi-layer bloom with gear and lightning accents
        /// </summary>
        public static void SpawnMusicNote(Vector2 position, Vector2 velocity, Color baseColor, float scale = 0.85f)
        {
            // SCALE UP - Final boss tier notes must be BOLDLY VISIBLE
            scale = Math.Max(scale, 0.75f);
            float shimmer = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.18f) * 0.25f;
            scale *= shimmer;
            
            int variant = Main.rand.Next(1, 7);
            
            // === LAYER 1: BLINDING WHITE CORE BLOOM ===
            for (int bloom = 0; bloom < 5; bloom++)
            {
                float bloomScale = scale * (0.35f + bloom * 0.22f);
                float bloomAlpha = 0.9f / (bloom + 1);
                Color coreColor = ClairDeLuneColors.BrightWhite * bloomAlpha;
                coreColor.A = 0;
                
                var coreBloom = new BloomParticle(
                    position + Main.rand.NextVector2Circular(2f, 2f),
                    velocity * 0.92f,
                    coreColor,
                    bloomScale,
                    32 + bloom * 5
                );
                MagnumParticleHandler.SpawnParticle(coreBloom);
            }
            
            // === LAYER 2: CRIMSON TEMPORAL ENERGY ===
            for (int bloom = 0; bloom < 4; bloom++)
            {
                float bloomScale = scale * (0.5f + bloom * 0.28f);
                float bloomAlpha = 0.7f / (bloom + 1);
                Color crimsonColor = ClairDeLuneColors.Crimson * bloomAlpha;
                crimsonColor.A = 0;
                
                var crimsonBloom = new BloomParticle(
                    position + Main.rand.NextVector2Circular(4f, 4f),
                    velocity * 0.75f,
                    crimsonColor,
                    bloomScale,
                    30 + bloom * 4
                );
                MagnumParticleHandler.SpawnParticle(crimsonBloom);
            }
            
            // === LAYER 3: BRASS CLOCKWORK ACCENT ===
            for (int i = 0; i < 3; i++)
            {
                var gear = new GenericGlowParticle(
                    position + Main.rand.NextVector2Circular(10f, 10f),
                    velocity * 0.5f + Main.rand.NextVector2Circular(2f, 2f),
                    ClairDeLuneColors.Brass * 0.6f,
                    scale * 0.35f,
                    25,
                    true
                );
                MagnumParticleHandler.SpawnParticle(gear);
            }
            
            // === LAYER 4: CRYSTAL SPARKLES ===
            for (int i = 0; i < 4; i++)
            {
                Vector2 sparkleOffset = Main.rand.NextVector2Circular(14f, 14f);
                var sparkle = new SparkleParticle(
                    position + sparkleOffset,
                    velocity * 0.6f + Main.rand.NextVector2Circular(2.5f, 2.5f),
                    Color.Lerp(ClairDeLuneColors.Crystal, ClairDeLuneColors.MoonlightSilver, Main.rand.NextFloat(0.3f, 0.8f)),
                    0.5f,
                    28
                );
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // === LAYER 5: LIGHTNING MICRO-ARCS ===
            if (Main.rand.NextBool(3))
            {
                var lightning = new GenericGlowParticle(
                    position + Main.rand.NextVector2Circular(8f, 8f),
                    velocity * 0.8f + Main.rand.NextVector2Circular(4f, 4f),
                    ClairDeLuneColors.GetLightningGradient(Main.rand.NextFloat()),
                    0.3f,
                    12,
                    true
                );
                MagnumParticleHandler.SpawnParticle(lightning);
            }
            
            // Vanilla dust for extra density - mechanical/electric themed
            for (int i = 0; i < 3; i++)
            {
                Dust dust = Dust.NewDustPerfect(
                    position + Main.rand.NextVector2Circular(5f, 5f),
                    DustID.Electric,
                    velocity + Main.rand.NextVector2Circular(3f, 3f),
                    0, ClairDeLuneColors.ElectricBlue, 1.6f
                );
                dust.noGravity = true;
            }
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
        /// Uses the custom gear texture with rotation animation
        /// </summary>
        public static void SpawnClockworkGear(Vector2 position, Vector2 velocity, bool large = false, float scale = 1f)
        {
            // Multi-layer gear bloom for epic visibility
            for (int layer = 0; layer < 4; layer++)
            {
                float layerScale = scale * (0.8f + layer * 0.15f);
                float alpha = 0.8f / (layer + 1);
                Color gearColor = Color.Lerp(ClairDeLuneColors.Brass, ClairDeLuneColors.GearGold, layer / 4f) * alpha;
                gearColor.A = 0;
                
                var gearBloom = new BloomParticle(
                    position + Main.rand.NextVector2Circular(3f, 3f),
                    velocity * (0.95f - layer * 0.05f),
                    gearColor,
                    layerScale * (large ? 1.4f : 0.9f),
                    30 + layer * 5
                );
                MagnumParticleHandler.SpawnParticle(gearBloom);
            }
            
            // Central gear accent
            var centerGear = new GenericGlowParticle(
                position,
                velocity,
                ClairDeLuneColors.Brass,
                scale * (large ? 0.7f : 0.45f),
                35,
                true
            );
            MagnumParticleHandler.SpawnParticle(centerGear);
            
            // Gear sparkles
            for (int i = 0; i < (large ? 4 : 2); i++)
            {
                var sparkle = new SparkleParticle(
                    position + Main.rand.NextVector2Circular(8f, 8f),
                    velocity * 0.5f + Main.rand.NextVector2Circular(2f, 2f),
                    ClairDeLuneColors.GearGold,
                    0.35f,
                    22
                );
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
        }
        
        /// <summary>
        /// Clockwork gear cascade - Multiple gears erupting in a mechanical burst
        /// </summary>
        public static void ClockworkGearCascade(Vector2 position, int count, float speed, float scale = 1f)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.25f, 0.25f);
                Vector2 vel = angle.ToRotationVector2() * speed * Main.rand.NextFloat(0.7f, 1.3f);
                bool isLarge = i % 3 == 0;
                float gearScale = scale * Main.rand.NextFloat(0.8f, 1.2f);
                
                SpawnClockworkGear(position + Main.rand.NextVector2Circular(10f, 10f), vel, isLarge, gearScale);
            }
            
            // Central mechanism flash
            for (int i = 0; i < 3; i++)
            {
                float flashScale = scale * (1.2f - i * 0.2f);
                Color flashColor = Color.Lerp(ClairDeLuneColors.BrightWhite, ClairDeLuneColors.Brass, i / 3f);
                flashColor.A = 0;
                
                var flash = new BloomParticle(position, Vector2.Zero, flashColor * (0.9f / (i + 1)), flashScale, 15 + i * 3);
                MagnumParticleHandler.SpawnParticle(flash);
            }
            
            // Gear dust
            for (int i = 0; i < count; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(speed * 1.2f, speed * 1.2f);
                Dust dust = Dust.NewDustPerfect(position, DustID.Enchanted_Gold, dustVel, 0, ClairDeLuneColors.Brass, 1.4f);
                dust.noGravity = true;
            }
        }
        
        /// <summary>
        /// Orbiting clockwork gears around a point
        /// </summary>
        public static void OrbitingGears(Vector2 center, float radius, int gearCount, float rotationOffset = 0f, float scale = 0.8f)
        {
            float baseAngle = Main.GameUpdateCount * 0.03f + rotationOffset;
            
            for (int i = 0; i < gearCount; i++)
            {
                float angle = baseAngle + MathHelper.TwoPi * i / gearCount;
                Vector2 gearPos = center + angle.ToRotationVector2() * radius;
                Vector2 tangentVel = (angle + MathHelper.PiOver2).ToRotationVector2() * 0.5f;
                
                // Alternating large and small gears
                bool isLarge = i % 2 == 0;
                
                // Gear glow
                Color gearColor = ClairDeLuneColors.GetGearGradient((float)i / gearCount);
                gearColor.A = 0;
                
                var gearGlow = new BloomParticle(gearPos, tangentVel, gearColor * 0.6f, scale * (isLarge ? 0.5f : 0.35f), 8);
                MagnumParticleHandler.SpawnParticle(gearGlow);
                
                // Gear sparkle trail
                if (Main.rand.NextBool(3))
                {
                    var trail = new GenericGlowParticle(
                        gearPos + Main.rand.NextVector2Circular(5f, 5f),
                        tangentVel * 2f,
                        ClairDeLuneColors.Brass * 0.5f,
                        0.25f * scale,
                        15,
                        true
                    );
                    MagnumParticleHandler.SpawnParticle(trail);
                }
            }
        }
        
        #endregion
        
        #region Lightning Effects
        
        /// <summary>
        /// Spawn a LIGHTNING BURST effect - Crackling temporal energy
        /// </summary>
        public static void SpawnLightningBurst(Vector2 position, Vector2 velocity, bool thick = false, float scale = 1f)
        {
            // Multi-layer electric bloom
            for (int layer = 0; layer < 5; layer++)
            {
                float layerScale = scale * (0.6f + layer * 0.2f);
                float alpha = 0.85f / (layer + 1);
                Color lightningColor = ClairDeLuneColors.GetLightningGradient(layer / 5f) * alpha;
                lightningColor.A = 0;
                
                var lightningBloom = new BloomParticle(
                    position + Main.rand.NextVector2Circular(5f, 5f),
                    velocity * (0.9f - layer * 0.1f),
                    lightningColor,
                    layerScale * (thick ? 1.3f : 0.9f),
                    18 + layer * 3
                );
                MagnumParticleHandler.SpawnParticle(lightningBloom);
            }
            
            // Core electric particle
            var core = new GenericGlowParticle(
                position,
                velocity,
                ClairDeLuneColors.ElectricBlue,
                scale * (thick ? 0.6f : 0.4f),
                20,
                true
            );
            MagnumParticleHandler.SpawnParticle(core);
            
            // Branching mini-arcs
            int arcCount = thick ? 5 : 3;
            for (int i = 0; i < arcCount; i++)
            {
                float arcAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 arcVel = arcAngle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                
                var arc = new GenericGlowParticle(
                    position + Main.rand.NextVector2Circular(6f, 6f),
                    velocity * 0.5f + arcVel,
                    ClairDeLuneColors.LightningPurple,
                    0.25f * scale,
                    10,
                    true
                );
                MagnumParticleHandler.SpawnParticle(arc);
            }
            
            // Electric dust
            for (int i = 0; i < (thick ? 6 : 3); i++)
            {
                Dust dust = Dust.NewDustPerfect(
                    position + Main.rand.NextVector2Circular(8f, 8f),
                    DustID.Electric,
                    velocity * 0.7f + Main.rand.NextVector2Circular(5f, 5f),
                    0, ClairDeLuneColors.ElectricBlue, 1.5f
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
        /// MASSIVE lightning strike explosion
        /// </summary>
        public static void LightningStrikeExplosion(Vector2 position, float scale = 1f)
        {
            // === BLINDING WHITE CORE ===
            for (int i = 0; i < 6; i++)
            {
                float coreScale = scale * (1.5f - i * 0.15f);
                Color coreColor = ClairDeLuneColors.BrightWhite * (0.95f / (i + 1));
                coreColor.A = 0;
                
                var coreFlare = new BloomParticle(position, Vector2.Zero, coreColor, coreScale, 20 + i * 2);
                MagnumParticleHandler.SpawnParticle(coreFlare);
            }
            
            // === LIGHTNING GRADIENT LAYERS ===
            for (int i = 0; i < 5; i++)
            {
                float layerScale = scale * (1.2f - i * 0.12f);
                Color lightningColor = ClairDeLuneColors.GetLightningGradient(i / 5f);
                lightningColor.A = 0;
                
                var lightningFlare = new BloomParticle(position, Vector2.Zero, lightningColor * (0.85f / (i + 1)), layerScale, 18 + i * 3);
                MagnumParticleHandler.SpawnParticle(lightningFlare);
            }
            
            // === RADIAL LIGHTNING ARCS ===
            int arcCount = (int)(12 * scale);
            for (int i = 0; i < arcCount; i++)
            {
                float angle = MathHelper.TwoPi * i / arcCount;
                float arcLength = Main.rand.NextFloat(60f, 120f) * scale;
                Vector2 arcEnd = position + angle.ToRotationVector2() * arcLength;
                
                LightningArc(position, arcEnd, 5, 8f * scale, scale * 0.6f);
            }
            
            // === EXPANDING ELECTRIC RINGS ===
            for (int i = 0; i < 6; i++)
            {
                Color ringColor = ClairDeLuneColors.GetLightningGradient(i / 6f);
                ringColor.A = 0;
                
                var ring = new BloomRingParticle(position, Vector2.Zero, ringColor * 0.75f, (0.3f + i * 0.15f) * scale, 22 + i * 3);
                MagnumParticleHandler.SpawnParticle(ring);
            }
            
            // === ELECTRIC DUST STORM ===
            for (int i = 0; i < (int)(25 * scale); i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(15f, 15f) * scale;
                Dust dust = Dust.NewDustPerfect(position, DustID.Electric, vel, 0, ClairDeLuneColors.ElectricBlue, 1.8f);
                dust.noGravity = true;
            }
            
            // Lighting
            Lighting.AddLight(position, ClairDeLuneColors.ElectricBlue.ToVector3() * scale * 2f);
        }
        
        #endregion
        
        #region Crystal Effects
        
        /// <summary>
        /// Spawn a CRYSTAL SHARD particle
        /// </summary>
        public static void SpawnCrystalShard(Vector2 position, Vector2 velocity, bool medium = false, float scale = 1f)
        {
            // Crystal bloom layers
            for (int layer = 0; layer < 4; layer++)
            {
                float layerScale = scale * (0.5f + layer * 0.18f);
                float alpha = 0.85f / (layer + 1);
                Color crystalColor = ClairDeLuneColors.GetCrystalGradient(layer / 4f) * alpha;
                crystalColor.A = 0;
                
                var crystalBloom = new BloomParticle(
                    position + Main.rand.NextVector2Circular(3f, 3f),
                    velocity * (0.95f - layer * 0.08f),
                    crystalColor,
                    layerScale * (medium ? 1.2f : 0.8f),
                    28 + layer * 4
                );
                MagnumParticleHandler.SpawnParticle(crystalBloom);
            }
            
            // Core shard
            var core = new GenericGlowParticle(
                position,
                velocity,
                ClairDeLuneColors.Crystal,
                scale * (medium ? 0.5f : 0.35f),
                30,
                true
            );
            MagnumParticleHandler.SpawnParticle(core);
            
            // Prismatic sparkles
            for (int i = 0; i < (medium ? 4 : 2); i++)
            {
                float hue = Main.rand.NextFloat();
                Color prismColor = Main.hslToRgb(hue, 0.6f, 0.85f);
                
                var sparkle = new SparkleParticle(
                    position + Main.rand.NextVector2Circular(8f, 8f),
                    velocity * 0.6f + Main.rand.NextVector2Circular(2f, 2f),
                    prismColor,
                    0.35f,
                    20
                );
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
        }
        
        /// <summary>
        /// Crystal shatter burst - Time shattering into fragments
        /// </summary>
        public static void CrystalShatterBurst(Vector2 position, int count, float speed, float scale = 1f)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 vel = angle.ToRotationVector2() * speed * Main.rand.NextFloat(0.6f, 1.4f);
                bool isMedium = i % 4 == 0;
                float shardScale = scale * Main.rand.NextFloat(0.7f, 1.3f);
                
                SpawnCrystalShard(position + Main.rand.NextVector2Circular(8f, 8f), vel, isMedium, shardScale);
            }
            
            // Central crystal flash
            for (int i = 0; i < 4; i++)
            {
                float flashScale = scale * (1.4f - i * 0.2f);
                Color flashColor = Color.Lerp(ClairDeLuneColors.BrightWhite, ClairDeLuneColors.Crystal, i / 4f);
                flashColor.A = 0;
                
                var flash = new BloomParticle(position, Vector2.Zero, flashColor * (0.9f / (i + 1)), flashScale, 16 + i * 3);
                MagnumParticleHandler.SpawnParticle(flash);
            }
            
            // Crystal dust
            for (int i = 0; i < count; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(speed * 1.1f, speed * 1.1f);
                Dust dust = Dust.NewDustPerfect(position, DustID.GemDiamond, dustVel, 0, ClairDeLuneColors.Crystal, 1.3f);
                dust.noGravity = true;
            }
        }
        
        /// <summary>
        /// Prismatic crystal refraction effect
        /// </summary>
        public static void CrystalRefraction(Vector2 position, float scale = 1f)
        {
            // Rainbow prismatic burst
            for (int i = 0; i < 12; i++)
            {
                float hue = (float)i / 12f;
                Color prismColor = Main.hslToRgb(hue, 0.8f, 0.8f);
                prismColor.A = 0;
                
                float angle = MathHelper.TwoPi * i / 12f + Main.rand.NextFloat(-0.1f, 0.1f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                
                var prism = new BloomParticle(position, vel, prismColor * 0.7f, 0.4f * scale, 25);
                MagnumParticleHandler.SpawnParticle(prism);
            }
            
            // White core flash
            for (int i = 0; i < 3; i++)
            {
                Color whiteCore = ClairDeLuneColors.BrightWhite * (0.9f / (i + 1));
                whiteCore.A = 0;
                
                var core = new BloomParticle(position, Vector2.Zero, whiteCore, (0.8f - i * 0.15f) * scale, 12 + i * 2);
                MagnumParticleHandler.SpawnParticle(core);
            }
        }
        
        #endregion
        
        #region Major Impact Effects
        
        /// <summary>
        /// TEMPORAL IMPACT - THE SIGNATURE CLAIR DE LUNE EFFECT
        /// Clockwork gears + Lightning + Crystal shards + Crimson energy
        /// MUST be the most spectacular impact in the entire mod!
        /// </summary>
        public static void TemporalImpact(Vector2 position, float scale = 1f)
        {
            // === PHASE 1: BLINDING WHITE TEMPORAL CORE ===
            for (int i = 0; i < 6; i++)
            {
                float coreScale = scale * (1.6f - i * 0.18f);
                float alpha = 0.95f / (i + 1);
                Color coreColor = ClairDeLuneColors.BrightWhite * alpha;
                coreColor.A = 0;
                
                var coreFlare = new BloomParticle(position, Vector2.Zero, coreColor, coreScale, 22 + i * 3);
                MagnumParticleHandler.SpawnParticle(coreFlare);
            }
            
            // === PHASE 2: CRIMSON TEMPORAL ENERGY BURST ===
            for (int i = 0; i < 5; i++)
            {
                float crimsonScale = scale * (1.3f - i * 0.12f);
                Color crimsonColor = Color.Lerp(ClairDeLuneColors.Crimson, ClairDeLuneColors.DeepCrimson, i / 5f);
                crimsonColor.A = 0;
                
                var crimsonFlare = new BloomParticle(position, Vector2.Zero, crimsonColor * (0.85f / (i + 1)), crimsonScale, 20 + i * 3);
                MagnumParticleHandler.SpawnParticle(crimsonFlare);
            }
            
            // === PHASE 3: CLOCKWORK GEAR CASCADE ===
            ClockworkGearCascade(position, (int)(10 * scale), 10f * scale, scale);
            
            // === PHASE 4: CRYSTAL SHATTER BURST ===
            CrystalShatterBurst(position, (int)(14 * scale), 12f * scale, scale);
            
            // === PHASE 5: LIGHTNING DISCHARGE ===
            int lightningArcs = (int)(8 * scale);
            for (int i = 0; i < lightningArcs; i++)
            {
                float angle = MathHelper.TwoPi * i / lightningArcs + Main.rand.NextFloat(-0.2f, 0.2f);
                float arcLength = Main.rand.NextFloat(50f, 100f) * scale;
                Vector2 arcEnd = position + angle.ToRotationVector2() * arcLength;
                
                LightningArc(position, arcEnd, 4, 10f * scale, scale * 0.5f);
            }
            
            // === EXPANDING TEMPORAL RINGS - Full gradient cycle ===
            for (int i = 0; i < 10; i++)
            {
                float progress = i / 10f;
                Color ringColor = ClairDeLuneColors.GetGradient(progress);
                ringColor.A = 0;
                
                var ring = new BloomRingParticle(
                    position, Vector2.Zero,
                    ringColor * 0.85f,
                    (0.25f + i * 0.14f) * scale,
                    24 + i * 3
                );
                MagnumParticleHandler.SpawnParticle(ring);
            }
            
            // === RADIAL PARTICLE SPRAY ===
            int particleCount = (int)(30 * scale);
            for (int i = 0; i < particleCount; i++)
            {
                float angle = MathHelper.TwoPi * i / particleCount + Main.rand.NextFloat(-0.2f, 0.2f);
                float speed = Main.rand.NextFloat(8f, 18f) * scale;
                Vector2 vel = angle.ToRotationVector2() * speed;
                
                float colorProgress = (float)i / particleCount;
                Color particleColor = ClairDeLuneColors.GetGradient(colorProgress);
                
                var particle = new GenericGlowParticle(position, vel, particleColor, 0.5f * scale, 32, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }
            
            // === VANILLA DUST DENSITY LAYER ===
            // Electric dust
            for (int i = 0; i < (int)(20 * scale); i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(14f, 14f) * scale;
                Dust dust = Dust.NewDustPerfect(position, DustID.Electric, vel, 0, ClairDeLuneColors.ElectricBlue, 1.7f);
                dust.noGravity = true;
            }
            
            // Gold sparkle dust
            for (int i = 0; i < (int)(15 * scale); i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(12f, 12f) * scale;
                Dust dust = Dust.NewDustPerfect(position, DustID.Enchanted_Gold, vel, 0, ClairDeLuneColors.Brass, 1.5f);
                dust.noGravity = true;
            }
            
            // Diamond dust
            for (int i = 0; i < (int)(12 * scale); i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(10f, 10f) * scale;
                Dust dust = Dust.NewDustPerfect(position, DustID.GemDiamond, vel, 0, ClairDeLuneColors.Crystal, 1.4f);
                dust.noGravity = true;
            }
            
            // === MUSIC NOTES BURST ===
            MusicNoteBurst(position, (int)(8 * scale), 6f * scale, 0.9f);
            
            // === PRISMATIC REFRACTION ===
            CrystalRefraction(position, scale);
            
            // INTENSE LIGHTING
            Lighting.AddLight(position, ClairDeLuneColors.BrightWhite.ToVector3() * scale * 2.5f);
        }
        
        /// <summary>
        /// Death explosion - MAXIMUM SPECTACLE for kill effects
        /// </summary>
        public static void TemporalDeathExplosion(Vector2 position, float scale = 1.5f)
        {
            // Use the full impact but even more intense
            TemporalImpact(position, scale * 1.3f);
            
            // Additional massive lightning strike
            LightningStrikeExplosion(position, scale);
            
            // Extra clockwork cascade
            ClockworkGearCascade(position, 16, 14f * scale, scale);
            
            // Extra crystal shatter
            CrystalShatterBurst(position, 20, 16f * scale, scale);
            
            // Massive music note spiral
            for (int ring = 0; ring < 3; ring++)
            {
                float ringRadius = (40f + ring * 30f) * scale;
                int notesInRing = 6 + ring * 2;
                
                for (int i = 0; i < notesInRing; i++)
                {
                    float angle = MathHelper.TwoPi * i / notesInRing + ring * 0.3f;
                    Vector2 notePos = position + angle.ToRotationVector2() * ringRadius;
                    Vector2 noteVel = angle.ToRotationVector2() * (4f + ring * 2f);
                    
                    SpawnMusicNote(notePos, noteVel, ClairDeLuneColors.GetGradient(Main.rand.NextFloat()), 0.95f * scale);
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
