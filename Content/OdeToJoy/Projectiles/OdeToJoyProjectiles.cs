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
using MagnumOpus.Common.Systems.VFX;

// Dynamic particle effects for aesthetically pleasing animations
using static MagnumOpus.Common.Systems.DynamicParticleEffects;

namespace MagnumOpus.Content.OdeToJoy.Projectiles
{
    #region Theme Colors
    
    /// <summary>
    /// Ode to Joy theme color palette - Triumphant blossoming nature and joyous celebration
    /// </summary>
    public static class OdeToJoyColors
    {
        public static readonly Color VerdantGreen = new Color(76, 175, 80);      // #4CAF50 - Growth, Nature
        public static readonly Color RosePink = new Color(255, 182, 193);        // #FFB6C1 - Blossoms, Beauty
        public static readonly Color GoldenPollen = new Color(255, 215, 0);      // #FFD700 - Joy, Radiance
        public static readonly Color WhiteBloom = new Color(255, 255, 255);      // #FFFFFF - Triumph, Purity
        public static readonly Color LeafGreen = new Color(34, 139, 34);         // Forest green accent
        public static readonly Color SunlightYellow = new Color(255, 250, 205);  // Light warm yellow
        public static readonly Color PetalPink = new Color(255, 105, 180);       // Hot pink accent
        
        public static Color GetGradient(float progress)
        {
            if (progress < 0.25f)
                return Color.Lerp(VerdantGreen, RosePink, progress * 4f);
            else if (progress < 0.5f)
                return Color.Lerp(RosePink, GoldenPollen, (progress - 0.25f) * 4f);
            else if (progress < 0.75f)
                return Color.Lerp(GoldenPollen, WhiteBloom, (progress - 0.5f) * 4f);
            else
                return Color.Lerp(WhiteBloom, VerdantGreen, (progress - 0.75f) * 4f);
        }
        
        public static Color GetPetalGradient(float progress)
        {
            if (progress < 0.33f)
                return Color.Lerp(WhiteBloom, RosePink, progress * 3f);
            else if (progress < 0.66f)
                return Color.Lerp(RosePink, PetalPink, (progress - 0.33f) * 3f);
            else
                return Color.Lerp(PetalPink, WhiteBloom, (progress - 0.66f) * 3f);
        }
    }
    
    #endregion
    
    #region VFX Helpers
    
    /// <summary>
    /// Ode to Joy VFX System - BRIGHT BLOSSOMS + VERDANT NATURE + GOLDEN JOY
    /// Every effect uses multi-layered rendering for maximum visual impact
    /// Theme: Celebratory garden, triumphant blooming, joyous radiance
    /// </summary>
    public static class OdeToJoyVFX
    {
        // === CORE VISUAL IDENTITY ===
        // Layer 1: BRIGHT WHITE bloom core (radiant center)
        // Layer 2: ROSE PINK petals (beautiful blossoms)
        // Layer 3: VERDANT GREEN leaves/vines (natural growth)
        // Layer 4: GOLDEN POLLEN sparkles (joyous celebration)
        
        /// <summary>
        /// Spawn a glowing music note with proper-sized bloom
        /// Garden-themed with petal accents - OPTIMIZED for proper sizing
        /// </summary>
        public static void SpawnMusicNote(Vector2 position, Vector2 velocity, Color baseColor, float scale = 0.5f)
        {
            // Subtle shimmer animation
            float shimmer = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.1f;
            scale *= shimmer;
            
            // === SINGLE CORE BLOOM (BloomParticle already draws 4 internal layers) ===
            Color coreColor = Color.White * 0.7f;
            coreColor.A = 0;
            var coreBloom = new BloomParticle(
                position,
                velocity * 0.9f,
                coreColor,
                scale * 0.25f, // REDUCED from 0.4f-1.15f range
                25
            );
            MagnumParticleHandler.SpawnParticle(coreBloom);
            
            // === PINK ACCENT BLOOM (single layer) ===
            Color pinkColor = OdeToJoyColors.RosePink * 0.5f;
            pinkColor.A = 0;
            var pinkBloom = new BloomParticle(
                position + Main.rand.NextVector2Circular(2f, 2f),
                velocity * 0.7f,
                pinkColor,
                scale * 0.2f, // REDUCED from 0.6f-1.2f
                22
            );
            MagnumParticleHandler.SpawnParticle(pinkBloom);
            
            // === VERDANT GREEN ACCENT (single glow) ===
            var leaf = new GenericGlowParticle(
                position + Main.rand.NextVector2Circular(4f, 4f),
                velocity * 0.4f + Main.rand.NextVector2Circular(1f, 1f),
                OdeToJoyColors.VerdantGreen * 0.4f,
                scale * 0.2f, // REDUCED
                20,
                true
            );
            MagnumParticleHandler.SpawnParticle(leaf);
            
            // === GOLDEN POLLEN SPARKLE (single) ===
            var sparkle = new SparkleParticle(
                position + Main.rand.NextVector2Circular(6f, 6f),
                velocity * 0.5f + Main.rand.NextVector2Circular(1f, 1f),
                Color.Lerp(OdeToJoyColors.GoldenPollen, OdeToJoyColors.WhiteBloom, Main.rand.NextFloat(0.3f, 0.7f)),
                0.25f, // REDUCED from 0.45f
                20
            );
            MagnumParticleHandler.SpawnParticle(sparkle);
            
            // Vanilla dust for extra density - flower/nature themed
            for (int i = 0; i < 2; i++)
            {
                Dust dust = Dust.NewDustPerfect(position, DustID.GreenFairy, velocity + Main.rand.NextVector2Circular(2f, 2f), 0, default, 1.5f);
                dust.noGravity = true;
            }
        }
        
        /// <summary>
        /// Multi-layered blossom impact explosion - OPTIMIZED for proper sizing
        /// White core → Rose pink petals → Verdant green → Golden pollen burst
        /// </summary>
        public static void BlossomImpact(Vector2 position, float scale = 1f)
        {
            // === PHASE 1: WHITE BLOOM FLASH (Single bloom, not 5 layers) ===
            Color coreWhite = Color.White * 0.8f;
            coreWhite.A = 0;
            var coreFlare = new BloomParticle(position, Vector2.Zero, coreWhite, scale * 0.35f, 18);
            MagnumParticleHandler.SpawnParticle(coreFlare);
            
            // === PHASE 2: ROSE PINK PETAL ACCENT (Single bloom) ===
            Color petalColor = OdeToJoyColors.RosePink * 0.6f;
            petalColor.A = 0;
            var petalFlare = new BloomParticle(position, Vector2.Zero, petalColor, scale * 0.3f, 20);
            MagnumParticleHandler.SpawnParticle(petalFlare);
            
            // === PHASE 3: GOLDEN POLLEN GLOW (Single bloom) ===
            Color goldColor = OdeToJoyColors.GoldenPollen * 0.5f;
            goldColor.A = 0;
            var goldFlare = new BloomParticle(position, Vector2.Zero, goldColor, scale * 0.25f, 16);
            MagnumParticleHandler.SpawnParticle(goldFlare);
            
            // === EXPANDING HALO RINGS - 4 gradient rings (reduced from 8) ===
            for (int i = 0; i < 4; i++)
            {
                float progress = i / 4f;
                Color haloColor = Color.Lerp(OdeToJoyColors.RosePink, OdeToJoyColors.VerdantGreen, progress);
                haloColor.A = 0;
                var halo = new BloomRingParticle(position, Vector2.Zero, haloColor * 0.6f, (0.15f + i * 0.08f) * scale, 18 + i * 2);
                MagnumParticleHandler.SpawnParticle(halo);
            }
            
            // === RADIAL PETAL PARTICLE SPRAY (reduced count) ===
            int petalCount = (int)(10 * scale);
            for (int i = 0; i < petalCount; i++)
            {
                float angle = MathHelper.TwoPi * i / petalCount + Main.rand.NextFloat(-0.2f, 0.2f);
                float speed = Main.rand.NextFloat(4f, 10f) * scale;
                Vector2 vel = angle.ToRotationVector2() * speed;
                float colorProgress = (float)i / petalCount;
                Color particleColor = OdeToJoyColors.GetPetalGradient(colorProgress);
                var petal = new GenericGlowParticle(position, vel, particleColor, 0.25f * scale, 22, true);
                MagnumParticleHandler.SpawnParticle(petal);
            }
            
            // === VERDANT VINE WISPS (reduced from 12 to 6) ===
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 vineVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f) * scale;
                var vine = new GenericGlowParticle(
                    position + Main.rand.NextVector2Circular(6f, 6f), vineVel,
                    OdeToJoyColors.VerdantGreen * 0.5f, 0.3f * scale, 28, true);
                MagnumParticleHandler.SpawnParticle(vine);
            }
            
            // === VANILLA DUST DENSITY LAYER (reduced counts) ===
            for (int i = 0; i < (int)(8 * scale); i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(8f, 8f) * scale;
                Dust dust = Dust.NewDustPerfect(position, DustID.GreenFairy, vel, 0, default, 1.4f);
                dust.noGravity = true;
            }
            for (int i = 0; i < (int)(6 * scale); i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f) * scale;
                Dust dust = Dust.NewDustPerfect(position, DustID.PinkFairy, vel, 0, default, 1.2f);
                dust.noGravity = true;
            }
            for (int i = 0; i < (int)(4 * scale); i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f) * scale;
                Dust spark = Dust.NewDustPerfect(position, DustID.Enchanted_Gold, vel, 0, OdeToJoyColors.GoldenPollen, 1.0f);
                spark.noGravity = true;
            }
            
            // === MUSIC NOTES BURST (reduced from 6 to 3) ===
            for (int i = 0; i < (int)(3 * scale); i++)
            {
                float angle = MathHelper.TwoPi * i / 3f + Main.rand.NextFloat(-0.4f, 0.4f);
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                Color noteColor = Main.rand.NextBool() ? Color.White : OdeToJoyColors.GoldenPollen;
                SpawnMusicNote(position, noteVel, noteColor, 0.5f * scale);
            }
            
            // === ROSE BUD IMPACT CENTER (reduced count) ===
            int budCount = Math.Max(2, (int)(3 * scale));
            for (int i = 0; i < budCount; i++)
            {
                float angle = MathHelper.TwoPi * i / budCount;
                Vector2 budVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f) * scale;
                var roseBud = RoseBudParticle.CreateRandom(
                    position + Main.rand.NextVector2Circular(8f, 8f), budVel,
                    Color.Lerp(OdeToJoyColors.RosePink, OdeToJoyColors.WhiteBloom, Main.rand.NextFloat(0.3f)),
                    OdeToJoyColors.GoldenPollen, 0.3f * scale, Main.rand.Next(25, 40), Main.rand.NextBool());
                MagnumParticleHandler.SpawnParticle(roseBud);
            }
            
            // === PETAL PARTICLE BURST - reduced ===
            PetalParticleBurst(position, (int)(6 * scale), 5f * scale, 0.3f * scale);
            
            // Lighting flash
            Lighting.AddLight(position, Color.White.ToVector3() * scale * 0.8f);
        }
        
        /// <summary>
        /// Petal particle burst - Ode to Joy signature effect - OPTIMIZED
        /// </summary>
        public static void PetalParticleBurst(Vector2 position, int count, float speed, float scale)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 vel = angle.ToRotationVector2() * speed * Main.rand.NextFloat(0.7f, 1.3f);
                Color petalColor = OdeToJoyColors.GetPetalGradient(Main.rand.NextFloat());
                var petal = new GenericGlowParticle(
                    position + Main.rand.NextVector2Circular(3f, 3f), vel, petalColor,
                    scale * Main.rand.NextFloat(0.6f, 1f), Main.rand.Next(20, 32), true);
                MagnumParticleHandler.SpawnParticle(petal);
            }
            
            // Single rose bud accent for extra beauty
            if (count >= 4)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 budVel = angle.ToRotationVector2() * speed * 0.5f;
                var roseBud = RoseBudParticle.CreateRandom(
                    position + Main.rand.NextVector2Circular(4f, 4f), budVel,
                    OdeToJoyColors.RosePink, OdeToJoyColors.WhiteBloom,
                    scale * 0.5f, Main.rand.Next(20, 35));
                MagnumParticleHandler.SpawnParticle(roseBud);
            }
        }
        
        /// <summary>
        /// Petal trail effect - OPTIMIZED for proper sizing
        /// </summary>
        public static void PetalTrail(Vector2 position, Vector2 velocity, float intensity = 1f)
        {
            // === WHITE CORE GLOW (occasional) ===
            if (Main.rand.NextBool(3))
            {
                Color coreColor = Color.White * 0.5f;
                coreColor.A = 0;
                var core = new GenericGlowParticle(position, -velocity * 0.05f, coreColor, 0.15f * intensity, 10, true);
                MagnumParticleHandler.SpawnParticle(core);
            }
            
            // === ROSE PINK PETAL LAYER (occasional) ===
            if (Main.rand.NextBool(3))
            {
                Color petalColor = Color.Lerp(OdeToJoyColors.RosePink, OdeToJoyColors.PetalPink, Main.rand.NextFloat());
                var petal = new GenericGlowParticle(
                    position + Main.rand.NextVector2Circular(3f, 3f),
                    -velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f),
                    petalColor, 0.2f * intensity, 18, true);
                MagnumParticleHandler.SpawnParticle(petal);
            }
            
            // === GREEN ACCENTS (rare) ===
            if (Main.rand.NextBool(5))
            {
                var leaf = new GenericGlowParticle(
                    position + Main.rand.NextVector2Circular(4f, 4f),
                    -velocity * 0.06f + Main.rand.NextVector2Circular(1f, 1f),
                    OdeToJoyColors.VerdantGreen * 0.5f, 0.18f * intensity, 15, true);
                MagnumParticleHandler.SpawnParticle(leaf);
            }
            
            // === GOLDEN POLLEN SPARKLES ===
            if (Main.rand.NextBool(4))
            {
                var sparkle = new SparkleParticle(
                    position + Main.rand.NextVector2Circular(5f, 5f),
                    -velocity * 0.05f,
                    Color.Lerp(OdeToJoyColors.GoldenPollen, Color.White, Main.rand.NextFloat()),
                    0.35f * intensity,
                    18
                );
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // === VANILLA DUST - Fairy dusts ===
            if (Main.rand.NextBool(2))
            {
                Dust green = Dust.NewDustPerfect(
                    position + Main.rand.NextVector2Circular(8f, 8f),
                    DustID.GreenFairy,
                    -velocity * 0.1f + Main.rand.NextVector2Circular(3f, 3f),
                    0, default, 1.5f * intensity
                );
                green.noGravity = true;
            }
            
            if (Main.rand.NextBool(3))
            {
                Dust pink = Dust.NewDustPerfect(
                    position + Main.rand.NextVector2Circular(8f, 8f),
                    DustID.PinkFairy,
                    -velocity * 0.1f + Main.rand.NextVector2Circular(2f, 2f),
                    0, default, 1.3f * intensity
                );
                pink.noGravity = true;
            }
            
            // === MUSIC NOTE (Less frequent but visible) ===
            if (Main.rand.NextBool(10))
            {
                SpawnMusicNote(position, -velocity * 0.08f, OdeToJoyColors.GoldenPollen, 0.75f * intensity);
            }
        }
        
        /// <summary>
        /// DRAMATIC charge-up effect for attacks - Garden energy converging
        /// </summary>
        public static void ChargeUp(Vector2 position, float progress, float scale = 1f)
        {
            // Converging particles - Green → Pink → Gold → White
            int particleCount = (int)(8 + progress * 12);
            for (int i = 0; i < particleCount; i++)
            {
                float angle = MathHelper.TwoPi * i / particleCount + Main.GameUpdateCount * 0.03f;
                float radius = 150f * (1f - progress * 0.6f);
                Vector2 particlePos = position + angle.ToRotationVector2() * radius;
                
                Color chargeColor = OdeToJoyColors.GetGradient(progress);
                chargeColor.A = 0;
                
                var charge = new GenericGlowParticle(
                    particlePos,
                    (position - particlePos).SafeNormalize(Vector2.Zero) * (3f + progress * 5f),
                    chargeColor,
                    0.3f * scale * (0.5f + progress * 0.5f),
                    15,
                    true
                );
                MagnumParticleHandler.SpawnParticle(charge);
            }
            
            // Central pulsing glow
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.2f) * 0.2f;
            var centerGlow = new BloomParticle(position, Vector2.Zero, Color.White * (0.3f + progress * 0.4f), 0.5f * scale * pulse * (0.5f + progress), 5);
            centerGlow.Color = centerGlow.Color with { A = 0 };
            MagnumParticleHandler.SpawnParticle(centerGlow);
        }
        
        /// <summary>
        /// Vine/thorn whip trail effect
        /// </summary>
        public static void VineTrail(Vector2 position, Vector2 velocity, float intensity = 1f)
        {
            // Verdant green core
            if (Main.rand.NextBool(2))
            {
                var vine = new GenericGlowParticle(
                    position + Main.rand.NextVector2Circular(4f, 4f),
                    -velocity * 0.15f,
                    OdeToJoyColors.VerdantGreen * 0.9f,
                    0.35f * intensity,
                    20,
                    true
                );
                MagnumParticleHandler.SpawnParticle(vine);
            }
            
            // Leaf green accents
            if (Main.rand.NextBool(3))
            {
                var leaf = new GenericGlowParticle(
                    position + Main.rand.NextVector2Circular(6f, 6f),
                    -velocity * 0.1f + Main.rand.NextVector2Circular(2f, 2f),
                    OdeToJoyColors.LeafGreen * 0.7f,
                    0.3f * intensity,
                    18,
                    true
                );
                MagnumParticleHandler.SpawnParticle(leaf);
            }
            
            // Thorn sparkles
            if (Main.rand.NextBool(5))
            {
                var sparkle = new SparkleParticle(
                    position,
                    -velocity * 0.05f,
                    Color.White * 0.8f,
                    0.25f * intensity,
                    12
                );
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            Dust dust = Dust.NewDustPerfect(position, DustID.GreenFairy, -velocity * 0.1f, 0, default, 1.2f * intensity);
            dust.noGravity = true;
        }
        
        /// <summary>
        /// Golden pollen burst effect - now with rose bud accents
        /// </summary>
        public static void PollenBurst(Vector2 position, float scale = 1f)
        {
            for (int i = 0; i < (int)(12 * scale); i++)
            {
                float angle = MathHelper.TwoPi * i / 12f + Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f) * scale;
                
                var pollen = new SparkleParticle(
                    position + Main.rand.NextVector2Circular(10f, 10f),
                    vel,
                    Color.Lerp(OdeToJoyColors.GoldenPollen, OdeToJoyColors.SunlightYellow, Main.rand.NextFloat()),
                    0.4f * scale,
                    Main.rand.Next(20, 35)
                );
                MagnumParticleHandler.SpawnParticle(pollen);
            }
            
            // Rose bud accent at center - the pollen source!
            if (scale > 0.5f)
            {
                var centralRose = RoseBudParticle.CreateRandom(
                    position,
                    Vector2.Zero,
                    OdeToJoyColors.RosePink,
                    OdeToJoyColors.GoldenPollen,
                    0.45f * scale,
                    Main.rand.Next(25, 40),
                    true // Bloom phase
                );
                MagnumParticleHandler.SpawnParticle(centralRose);
            }
            
            // Gold dust
            for (int i = 0; i < (int)(8 * scale); i++)
            {
                Dust gold = Dust.NewDustPerfect(position, DustID.Enchanted_Gold, Main.rand.NextVector2Circular(6f, 6f) * scale, 0, default, 1.5f);
                gold.noGravity = true;
            }
        }
        
        /// <summary>
        /// 🌹 ROSE BUD EXPLOSION - Standalone rose bud burst effect
        /// Creates a beautiful burst of blooming rose buds
        /// USE THIS for projectile impacts, melee hits, and summoner effects!
        /// </summary>
        /// <param name="position">Center of the burst</param>
        /// <param name="count">Number of rose buds (default 6)</param>
        /// <param name="speed">Burst speed</param>
        /// <param name="scale">Overall scale</param>
        /// <param name="withBloomPhase">Use blooming animation (default true)</param>
        public static void RoseBudExplosion(Vector2 position, int count = 6, float speed = 6f, float scale = 1f, bool withBloomPhase = true)
        {
            // Central white flash
            var centerFlash = new BloomParticle(
                position,
                Vector2.Zero,
                Color.White with { A = 0 } * 0.8f,
                scale * 1.2f,
                20
            );
            MagnumParticleHandler.SpawnParticle(centerFlash);
            
            // Rose bud burst
            RoseBudParticle.SpawnBurst(
                position,
                count,
                speed * scale,
                OdeToJoyColors.RosePink,
                OdeToJoyColors.WhiteBloom,
                0.55f * scale,
                Main.rand.Next(35, 50)
            );
            
            // Secondary smaller burst
            RoseBudParticle.SpawnBurst(
                position,
                count / 2 + 1,
                speed * 1.5f * scale,
                OdeToJoyColors.PetalPink,
                OdeToJoyColors.GoldenPollen,
                0.35f * scale,
                Main.rand.Next(25, 40)
            );
            
            // Petal spray accompaniment
            for (int i = 0; i < count * 2; i++)
            {
                float angle = MathHelper.TwoPi * i / (count * 2) + Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 vel = angle.ToRotationVector2() * speed * Main.rand.NextFloat(0.8f, 1.4f);
                
                var petal = new GenericGlowParticle(
                    position + Main.rand.NextVector2Circular(6f, 6f),
                    vel,
                    OdeToJoyColors.GetPetalGradient(Main.rand.NextFloat()),
                    0.4f * scale,
                    Main.rand.Next(20, 35),
                    true
                );
                MagnumParticleHandler.SpawnParticle(petal);
            }
            
            // Golden pollen sparkles
            for (int i = 0; i < count; i++)
            {
                var pollen = new SparkleParticle(
                    position + Main.rand.NextVector2Circular(15f, 15f),
                    Main.rand.NextVector2Circular(speed * 0.5f, speed * 0.5f),
                    OdeToJoyColors.GoldenPollen,
                    0.35f * scale,
                    Main.rand.Next(20, 30)
                );
                MagnumParticleHandler.SpawnParticle(pollen);
            }
            
            // Lighting
            Lighting.AddLight(position, OdeToJoyColors.RosePink.ToVector3() * scale * 0.8f);
        }
        
        /// <summary>
        /// MASSIVE death explosion - Screen-filling blossom spectacle
        /// </summary>
        public static void DeathExplosion(Vector2 position, float scale = 1f)
        {
            // Phase 1: Blinding white nova
            for (int layer = 0; layer < 8; layer++)
            {
                float layerScale = scale * (2f - layer * 0.15f);
                Color novaColor = Color.Lerp(Color.White, OdeToJoyColors.GoldenPollen, layer / 8f);
                novaColor.A = 0;
                
                var nova = new BloomParticle(position, Vector2.Zero, novaColor * (0.9f / (layer + 1)), layerScale, 35 + layer * 3);
                MagnumParticleHandler.SpawnParticle(nova);
            }
            
            // Phase 2: Expanding petal rings
            for (int ring = 0; ring < 12; ring++)
            {
                float ringProgress = ring / 12f;
                Color ringColor = OdeToJoyColors.GetGradient(ringProgress);
                ringColor.A = 0;
                
                var petalRing = new BloomRingParticle(position, Vector2.Zero, ringColor * 0.7f, (0.4f + ring * 0.2f) * scale, 25 + ring * 3);
                MagnumParticleHandler.SpawnParticle(petalRing);
            }
            
            // Phase 3: Massive petal spray
            for (int i = 0; i < 60; i++)
            {
                float angle = MathHelper.TwoPi * i / 60f + Main.rand.NextFloat(-0.15f, 0.15f);
                float speed = Main.rand.NextFloat(8f, 20f) * scale;
                Vector2 vel = angle.ToRotationVector2() * speed;
                
                Color particleColor;
                float colorRoll = Main.rand.NextFloat();
                if (colorRoll < 0.25f)
                    particleColor = Color.White;
                else if (colorRoll < 0.5f)
                    particleColor = OdeToJoyColors.RosePink;
                else if (colorRoll < 0.75f)
                    particleColor = OdeToJoyColors.VerdantGreen;
                else
                    particleColor = OdeToJoyColors.GoldenPollen;
                
                var particle = new GenericGlowParticle(position, vel, particleColor, 0.5f * scale, 40, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }
            
            // Phase 4: Music note cascade
            for (int i = 0; i < 15; i++)
            {
                float angle = MathHelper.TwoPi * i / 15f;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 10f);
                SpawnMusicNote(position + Main.rand.NextVector2Circular(30f, 30f), noteVel, Color.White, scale);
            }
            
            // Phase 5: MASSIVE ROSE BUD EXPLOSION - The grand finale!
            // Large central rose buds blooming dramatically
            RoseBudParticle.SpawnBurst(
                position,
                (int)(12 * scale),
                10f * scale,
                OdeToJoyColors.RosePink,
                Color.White,
                0.8f * scale,
                Main.rand.Next(50, 70)
            );
            
            // Secondary wave of smaller rose buds
            RoseBudParticle.SpawnBurst(
                position,
                (int)(8 * scale),
                15f * scale,
                OdeToJoyColors.PetalPink,
                OdeToJoyColors.GoldenPollen,
                0.55f * scale,
                Main.rand.Next(40, 55)
            );
            
            // Tertiary wave - tiny fast rose buds
            RoseBudParticle.SpawnBurst(
                position,
                (int)(6 * scale),
                20f * scale,
                Color.Lerp(OdeToJoyColors.WhiteBloom, OdeToJoyColors.RosePink, 0.3f),
                OdeToJoyColors.SunlightYellow,
                0.35f * scale,
                Main.rand.Next(30, 45)
            );
            
            // Massive dust explosion
            for (int i = 0; i < 40; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(20f, 20f) * scale;
                int dustType = Main.rand.NextBool() ? DustID.GreenFairy : DustID.PinkFairy;
                Dust dust = Dust.NewDustPerfect(position, dustType, vel, 0, default, 2.5f);
                dust.noGravity = true;
            }
            
            // Golden pollen explosion
            for (int i = 0; i < 25; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(15f, 15f) * scale;
                Dust gold = Dust.NewDustPerfect(position, DustID.Enchanted_Gold, vel, 0, default, 2f);
                gold.noGravity = true;
            }
            
            // Screen shake
            MagnumScreenEffects.AddScreenShake(12f * scale);
            
            // Lighting
            Lighting.AddLight(position, Color.White.ToVector3() * 2f * scale);
        }
        
        /// <summary>
        /// Afterimage trail effect with garden colors
        /// </summary>
        public static void AfterimageTrail(Vector2 position, Vector2 velocity, float scale, Color baseColor, int count)
        {
            for (int i = 0; i < count; i++)
            {
                float progress = i / (float)count;
                Vector2 trailPos = position - velocity * (i * 2f);
                Color trailColor = Color.Lerp(baseColor, OdeToJoyColors.VerdantGreen, progress) * (1f - progress * 0.7f);
                
                var afterimage = new GenericGlowParticle(
                    trailPos,
                    Vector2.Zero,
                    trailColor,
                    scale * (1f - progress * 0.5f),
                    10,
                    true
                );
                MagnumParticleHandler.SpawnParticle(afterimage);
            }
        }
        
        /// <summary>
        /// Spiral trail effect
        /// </summary>
        public static void SpiralTrail(Vector2 position, Vector2 velocity, Color baseColor, float scale, float spiralAngle)
        {
            Vector2 perpendicular = velocity.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2);
            float offset = (float)Math.Sin(spiralAngle) * 8f;
            Vector2 spiralPos = position + perpendicular * offset;
            
            var spiral = new GenericGlowParticle(
                spiralPos,
                -velocity * 0.1f,
                baseColor * 0.7f,
                scale,
                15,
                true
            );
            MagnumParticleHandler.SpawnParticle(spiral);
        }
        
        #region === UNIQUE SIGNATURE PARTICLES (3 Types) ===
        
        /// <summary>
        /// 🌹 CHROMATIC ROSE PETAL BURST - Ode to Joy's signature petal explosion
        /// Spawns a spectacular burst of chromatic rose petals with multi-layer bloom
        /// and shimmering iridescent effects. USE THIS for all petal-based impacts!
        /// </summary>
        /// <param name="position">Center of the burst</param>
        /// <param name="petalCount">Number of petals (default 16 for full effect)</param>
        /// <param name="burstSpeed">How fast petals fly outward</param>
        /// <param name="scale">Overall scale multiplier</param>
        /// <param name="withMusicNotes">Include music note accents</param>
        public static void ChromaticRosePetalBurst(Vector2 position, int petalCount = 16, float burstSpeed = 8f, float scale = 1f, bool withMusicNotes = true)
        {
            // === PHASE 1: CENTRAL WHITE BLOOM FLASH ===
            for (int layer = 0; layer < 4; layer++)
            {
                float layerScale = scale * (1.5f - layer * 0.25f);
                Color bloomColor = Color.Lerp(Color.White, OdeToJoyColors.RosePink, layer / 4f);
                bloomColor.A = 0;
                
                var bloom = new BloomParticle(
                    position,
                    Vector2.Zero,
                    bloomColor * (0.8f / (layer + 1)),
                    layerScale,
                    20 + layer * 3
                );
                MagnumParticleHandler.SpawnParticle(bloom);
            }
            
            // === PHASE 2: CHROMATIC PETAL BURST (The Star of the Show!) ===
            for (int i = 0; i < petalCount; i++)
            {
                float angle = MathHelper.TwoPi * i / petalCount + Main.rand.NextFloat(-0.15f, 0.15f);
                float speed = burstSpeed * Main.rand.NextFloat(0.8f, 1.4f);
                Vector2 vel = angle.ToRotationVector2() * speed;
                
                // Chromatic color cycling - creates iridescent rainbow shimmer effect
                float hueShift = (float)i / petalCount + Main.rand.NextFloat(-0.1f, 0.1f);
                Color petalColor;
                
                // Create chromatic gradient: White → Pink → Magenta → Rose → Pink → White
                if (hueShift < 0.2f)
                    petalColor = Color.Lerp(Color.White, OdeToJoyColors.RosePink, hueShift * 5f);
                else if (hueShift < 0.4f)
                    petalColor = Color.Lerp(OdeToJoyColors.RosePink, new Color(255, 100, 180), (hueShift - 0.2f) * 5f);
                else if (hueShift < 0.6f)
                    petalColor = Color.Lerp(new Color(255, 100, 180), OdeToJoyColors.PetalPink, (hueShift - 0.4f) * 5f);
                else if (hueShift < 0.8f)
                    petalColor = Color.Lerp(OdeToJoyColors.PetalPink, OdeToJoyColors.RosePink, (hueShift - 0.6f) * 5f);
                else
                    petalColor = Color.Lerp(OdeToJoyColors.RosePink, Color.White, (hueShift - 0.8f) * 5f);
                
                // Main petal particle with glow
                var petal = new GenericGlowParticle(
                    position + Main.rand.NextVector2Circular(5f, 5f),
                    vel,
                    petalColor,
                    0.55f * scale * Main.rand.NextFloat(0.9f, 1.1f),
                    Main.rand.Next(30, 50),
                    true
                );
                MagnumParticleHandler.SpawnParticle(petal);
                
                // Add sparkle overlay for iridescence
                if (Main.rand.NextBool(2))
                {
                    var sparkle = new SparkleParticle(
                        position + Main.rand.NextVector2Circular(8f, 8f),
                        vel * 0.6f,
                        Color.White * 0.9f,
                        0.35f * scale,
                        Main.rand.Next(20, 35)
                    );
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
            }
            
            // === PHASE 3: SECONDARY PETAL LAYER (Smaller, faster, trailing) ===
            int secondaryCount = petalCount / 2;
            for (int i = 0; i < secondaryCount; i++)
            {
                float angle = MathHelper.TwoPi * i / secondaryCount + Main.rand.NextFloat(-0.3f, 0.3f);
                float speed = burstSpeed * Main.rand.NextFloat(1.2f, 1.8f);
                Vector2 vel = angle.ToRotationVector2() * speed;
                
                Color secondaryColor = Color.Lerp(OdeToJoyColors.RosePink, OdeToJoyColors.GoldenPollen, Main.rand.NextFloat(0.2f, 0.5f));
                
                var secondary = new GenericGlowParticle(
                    position,
                    vel,
                    secondaryColor * 0.7f,
                    0.35f * scale,
                    Main.rand.Next(25, 40),
                    true
                );
                MagnumParticleHandler.SpawnParticle(secondary);
            }
            
            // === PHASE 4: GOLDEN POLLEN CLOUD ===
            for (int i = 0; i < (int)(8 * scale); i++)
            {
                Vector2 pollenVel = Main.rand.NextVector2Circular(burstSpeed * 0.5f, burstSpeed * 0.5f);
                
                var pollen = new SparkleParticle(
                    position + Main.rand.NextVector2Circular(15f, 15f),
                    pollenVel,
                    Color.Lerp(OdeToJoyColors.GoldenPollen, OdeToJoyColors.SunlightYellow, Main.rand.NextFloat()),
                    0.4f * scale,
                    Main.rand.Next(25, 40)
                );
                MagnumParticleHandler.SpawnParticle(pollen);
            }
            
            // === PHASE 5: EXPANDING CHROMATIC RINGS ===
            for (int ring = 0; ring < 5; ring++)
            {
                float ringProgress = ring / 5f;
                Color ringColor = Color.Lerp(OdeToJoyColors.RosePink, OdeToJoyColors.VerdantGreen, ringProgress);
                ringColor.A = 0;
                
                var halo = new BloomRingParticle(
                    position,
                    Vector2.Zero,
                    ringColor * (0.6f - ringProgress * 0.3f),
                    (0.3f + ring * 0.15f) * scale,
                    22 + ring * 4
                );
                MagnumParticleHandler.SpawnParticle(halo);
            }
            
            // === PHASE 6: ROSE BUD CENTER BLOOM ===
            // Spawn beautiful rose bud particles at the center - blooming phase!
            int roseBudCount = Math.Max(2, (int)(4 * scale));
            for (int i = 0; i < roseBudCount; i++)
            {
                float angle = MathHelper.TwoPi * i / roseBudCount;
                Vector2 budVel = angle.ToRotationVector2() * burstSpeed * 0.4f;
                Vector2 budOffset = Main.rand.NextVector2Circular(8f, 8f);
                
                // Bloom phase rose buds that open dramatically
                var roseBud = RoseBudParticle.CreateRandom(
                    position + budOffset,
                    budVel,
                    OdeToJoyColors.RosePink,
                    OdeToJoyColors.PetalPink,
                    0.5f * scale,
                    Main.rand.Next(35, 50),
                    true // Bloom phase animation
                );
                MagnumParticleHandler.SpawnParticle(roseBud);
            }
            
            // === PHASE 7: MUSIC NOTE ACCENTS ===
            if (withMusicNotes)
            {
                for (int i = 0; i < (int)(5 * scale); i++)
                {
                    float angle = MathHelper.TwoPi * i / 5f + Main.rand.NextFloat(-0.4f, 0.4f);
                    Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                    Color noteColor = Main.rand.NextBool(3) ? Color.White : OdeToJoyColors.RosePink;
                    SpawnMusicNote(position + Main.rand.NextVector2Circular(10f, 10f), noteVel, noteColor, 0.9f * scale);
                }
            }
            
            // === VANILLA DUST FOR DENSITY ===
            for (int i = 0; i < (int)(12 * scale); i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(burstSpeed, burstSpeed);
                Dust pink = Dust.NewDustPerfect(position, DustID.PinkFairy, dustVel, 0, default, 1.6f);
                pink.noGravity = true;
            }
            
            for (int i = 0; i < (int)(8 * scale); i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(burstSpeed * 0.7f, burstSpeed * 0.7f);
                Dust gold = Dust.NewDustPerfect(position, DustID.Enchanted_Gold, dustVel, 0, OdeToJoyColors.GoldenPollen, 1.3f);
                gold.noGravity = true;
            }
            
            // Lighting
            Lighting.AddLight(position, OdeToJoyColors.RosePink.ToVector3() * scale * 1.2f);
        }
        
        /// <summary>
        /// 🎵 HARMONIC NOTE SPARKLE - Chromatic music note explosion with soft glow
        /// Creates a burst of shimmering musical notes with multi-layer bloom
        /// USE THIS for magic weapon impacts and hymn-related effects!
        /// </summary>
        /// <param name="position">Center of the burst</param>
        /// <param name="noteCount">Number of notes (default 12)</param>
        /// <param name="burstSpeed">How fast notes spread</param>
        /// <param name="scale">Overall scale multiplier</param>
        /// <param name="withVineAccents">Include small vine trail elements</param>
        public static void HarmonicNoteSparkle(Vector2 position, int noteCount = 12, float burstSpeed = 6f, float scale = 1f, bool withVineAccents = true)
        {
            // === PHASE 1: CENTRAL HARMONIC GLOW ===
            // Multi-layer white-gold-green gradient bloom
            for (int layer = 0; layer < 5; layer++)
            {
                float layerProgress = layer / 5f;
                Color glowColor;
                if (layerProgress < 0.33f)
                    glowColor = Color.Lerp(Color.White, OdeToJoyColors.GoldenPollen, layerProgress * 3f);
                else if (layerProgress < 0.66f)
                    glowColor = Color.Lerp(OdeToJoyColors.GoldenPollen, OdeToJoyColors.VerdantGreen, (layerProgress - 0.33f) * 3f);
                else
                    glowColor = Color.Lerp(OdeToJoyColors.VerdantGreen, Color.White, (layerProgress - 0.66f) * 3f);
                
                glowColor.A = 0;
                
                var glow = new BloomParticle(
                    position,
                    Vector2.Zero,
                    glowColor * (0.7f / (layer + 1)),
                    (1.2f - layer * 0.15f) * scale,
                    18 + layer * 3
                );
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // === PHASE 2: RADIANT MUSIC NOTES (Signature Effect!) ===
            for (int i = 0; i < noteCount; i++)
            {
                float angle = MathHelper.TwoPi * i / noteCount + Main.rand.NextFloat(-0.2f, 0.2f);
                float speed = burstSpeed * Main.rand.NextFloat(0.7f, 1.3f);
                Vector2 vel = angle.ToRotationVector2() * speed;
                
                // Chromatic note color - cycling through theme colors
                float colorPhase = (float)i / noteCount;
                Color noteColor;
                if (colorPhase < 0.25f)
                    noteColor = Color.Lerp(Color.White, OdeToJoyColors.GoldenPollen, colorPhase * 4f);
                else if (colorPhase < 0.5f)
                    noteColor = Color.Lerp(OdeToJoyColors.GoldenPollen, OdeToJoyColors.VerdantGreen, (colorPhase - 0.25f) * 4f);
                else if (colorPhase < 0.75f)
                    noteColor = Color.Lerp(OdeToJoyColors.VerdantGreen, OdeToJoyColors.RosePink, (colorPhase - 0.5f) * 4f);
                else
                    noteColor = Color.Lerp(OdeToJoyColors.RosePink, Color.White, (colorPhase - 0.75f) * 4f);
                
                // Spawn the music note with enhanced visibility (scale 0.8f+)
                SpawnMusicNote(
                    position + Main.rand.NextVector2Circular(8f, 8f),
                    vel,
                    noteColor,
                    Main.rand.NextFloat(0.8f, 1.1f) * scale
                );
                
                // Add sparkle companion for shimmer effect
                var sparkle = new SparkleParticle(
                    position + Main.rand.NextVector2Circular(10f, 10f),
                    vel * 0.5f,
                    Color.White * 0.85f,
                    0.3f * scale,
                    Main.rand.Next(15, 28)
                );
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // === PHASE 3: SECONDARY SPARKLE WAVE ===
            int sparkleCount = noteCount * 2;
            for (int i = 0; i < sparkleCount; i++)
            {
                float angle = MathHelper.TwoPi * i / sparkleCount + Main.rand.NextFloat(-0.4f, 0.4f);
                float speed = burstSpeed * Main.rand.NextFloat(1.1f, 1.6f);
                Vector2 vel = angle.ToRotationVector2() * speed;
                
                Color sparkColor = Color.Lerp(OdeToJoyColors.SunlightYellow, Color.White, Main.rand.NextFloat());
                
                var spark = new GenericGlowParticle(
                    position + Main.rand.NextVector2Circular(5f, 5f),
                    vel,
                    sparkColor * 0.65f,
                    0.25f * scale,
                    Main.rand.Next(18, 30),
                    true
                );
                MagnumParticleHandler.SpawnParticle(spark);
            }
            
            // === PHASE 4: VINE TRAIL ACCENTS ===
            if (withVineAccents)
            {
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi * i / 6f;
                    Vector2 vineVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                    
                    var vine = new GenericGlowParticle(
                        position + Main.rand.NextVector2Circular(12f, 12f),
                        vineVel,
                        OdeToJoyColors.VerdantGreen * 0.6f,
                        0.4f * scale,
                        Main.rand.Next(28, 42),
                        true
                    );
                    MagnumParticleHandler.SpawnParticle(vine);
                }
            }
            
            // === PHASE 5: HARMONIC RING EXPANSION ===
            for (int ring = 0; ring < 4; ring++)
            {
                Color ringColor = Color.Lerp(OdeToJoyColors.GoldenPollen, OdeToJoyColors.WhiteBloom, ring / 4f);
                ringColor.A = 0;
                
                var halo = new BloomRingParticle(
                    position,
                    Vector2.Zero,
                    ringColor * (0.5f - ring * 0.1f),
                    (0.25f + ring * 0.12f) * scale,
                    20 + ring * 4
                );
                MagnumParticleHandler.SpawnParticle(halo);
            }
            
            // === VANILLA DUST LAYER ===
            for (int i = 0; i < (int)(10 * scale); i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(burstSpeed, burstSpeed);
                Dust fairy = Dust.NewDustPerfect(position, DustID.GreenFairy, dustVel, 0, default, 1.4f);
                fairy.noGravity = true;
            }
            
            for (int i = 0; i < (int)(6 * scale); i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(burstSpeed * 0.8f, burstSpeed * 0.8f);
                Dust gold = Dust.NewDustPerfect(position, DustID.Enchanted_Gold, dustVel, 0, default, 1.2f);
                gold.noGravity = true;
            }
            
            // Lighting - warm golden glow
            Lighting.AddLight(position, OdeToJoyColors.GoldenPollen.ToVector3() * scale * 1.1f);
        }
        
        /// <summary>
        /// 🌿 CHROMATIC VINE GROWTH TENDRIL - Metal vine burst with roses and buds
        /// Creates an organic burst of metal vines with small roses sprouting
        /// NOW USES CUSTOM VINE TEXTURES: VineWithNoRoses, VineWithRoseOnTop, VineWithTwoRoses
        /// USE THIS for summon weapons, growth effects, and nature-themed impacts!
        /// </summary>
        /// <param name="position">Center of the burst</param>
        /// <param name="tendrilCount">Number of vine tendrils (default 8)</param>
        /// <param name="growthSpeed">How fast vines spread</param>
        /// <param name="scale">Overall scale multiplier</param>
        /// <param name="withRoseBloom">Include metal rose blooms at tips</param>
        public static void ChromaticVineGrowthBurst(Vector2 position, int tendrilCount = 8, float growthSpeed = 5f, float scale = 1f, bool withRoseBloom = true)
        {
            // === PHASE 1: CENTRAL GROWTH ENERGY PULSE ===
            for (int layer = 0; layer < 4; layer++)
            {
                float layerProgress = layer / 4f;
                Color energyColor = Color.Lerp(OdeToJoyColors.VerdantGreen, OdeToJoyColors.LeafGreen, layerProgress);
                energyColor.A = 0;
                
                var energy = new BloomParticle(
                    position,
                    Vector2.Zero,
                    energyColor * (0.75f / (layer + 1)),
                    (1.3f - layer * 0.2f) * scale,
                    20 + layer * 4
                );
                MagnumParticleHandler.SpawnParticle(energy);
            }
            
            // === PHASE 2: VINE TENDRIL BURST WITH CUSTOM TEXTURES! ===
            for (int i = 0; i < tendrilCount; i++)
            {
                float angle = MathHelper.TwoPi * i / tendrilCount + Main.rand.NextFloat(-0.1f, 0.1f);
                float speed = growthSpeed * Main.rand.NextFloat(0.8f, 1.3f);
                Vector2 vel = angle.ToRotationVector2() * speed;
                
                // Chromatic green gradient - creates iridescent vine effect
                float hueShift = (float)i / tendrilCount;
                Color vineColor;
                if (hueShift < 0.33f)
                    vineColor = Color.Lerp(OdeToJoyColors.VerdantGreen, OdeToJoyColors.LeafGreen, hueShift * 3f);
                else if (hueShift < 0.66f)
                    vineColor = Color.Lerp(OdeToJoyColors.LeafGreen, new Color(100, 200, 100), (hueShift - 0.33f) * 3f);
                else
                    vineColor = Color.Lerp(new Color(100, 200, 100), OdeToJoyColors.VerdantGreen, (hueShift - 0.66f) * 3f);
                
                // ★ NEW: Spawn VineRoseParticle with custom textures! ★
                // Alternate between vine types for variety
                VineRoseParticle.VineType vineType;
                if (withRoseBloom)
                {
                    // With roses - use all three types
                    vineType = (i % 3) switch
                    {
                        0 => VineRoseParticle.VineType.TwoRoses,
                        1 => VineRoseParticle.VineType.RoseOnTop,
                        _ => VineRoseParticle.VineType.NoRoses
                    };
                }
                else
                {
                    // Without roses - only NoRoses type
                    vineType = VineRoseParticle.VineType.NoRoses;
                }
                
                var customVine = new VineRoseParticle(
                    position + Main.rand.NextVector2Circular(4f, 4f),
                    vel,
                    vineType,
                    vineColor,
                    Color.Lerp(OdeToJoyColors.RosePink, OdeToJoyColors.GoldenPollen, Main.rand.NextFloat()),
                    0.5f * scale * Main.rand.NextFloat(0.9f, 1.2f),
                    Main.rand.Next(35, 55),
                    angle, // Rotation matches velocity direction
                    Main.rand.NextFloat(-0.02f, 0.02f) // Slight spin
                );
                MagnumParticleHandler.SpawnParticle(customVine);
                
                // Also spawn regular glow particles for extra trail density
                var vine = new GenericGlowParticle(
                    position + Main.rand.NextVector2Circular(4f, 4f),
                    vel * 0.9f,
                    vineColor,
                    0.35f * scale * Main.rand.NextFloat(0.9f, 1.2f),
                    Main.rand.Next(25, 45),
                    true
                );
                MagnumParticleHandler.SpawnParticle(vine);
                
                // Secondary vine trail particles (smaller, following the main one)
                for (int j = 0; j < 2; j++)
                {
                    Vector2 trailVel = vel * (0.6f - j * 0.15f);
                    trailVel += Main.rand.NextVector2Circular(1f, 1f);
                    
                    var trail = new GenericGlowParticle(
                        position + Main.rand.NextVector2Circular(8f, 8f),
                        trailVel,
                        vineColor * (0.5f - j * 0.1f),
                        0.3f * scale,
                        Main.rand.Next(20, 35),
                        true
                    );
                    MagnumParticleHandler.SpawnParticle(trail);
                }
            }
            
            // === PHASE 3: CHROMATIC BUD SPARKLES ===
            int budCount = tendrilCount * 2;
            for (int i = 0; i < budCount; i++)
            {
                float angle = MathHelper.TwoPi * i / budCount + Main.rand.NextFloat(-0.3f, 0.3f);
                float speed = growthSpeed * Main.rand.NextFloat(1.1f, 1.5f);
                Vector2 vel = angle.ToRotationVector2() * speed;
                
                // Bud colors - mix of green and pink/gold for chromatic effect
                Color budColor;
                if (Main.rand.NextBool(3))
                    budColor = Color.Lerp(OdeToJoyColors.RosePink, Color.White, Main.rand.NextFloat(0.3f, 0.6f));
                else if (Main.rand.NextBool(2))
                    budColor = Color.Lerp(OdeToJoyColors.GoldenPollen, OdeToJoyColors.SunlightYellow, Main.rand.NextFloat());
                else
                    budColor = OdeToJoyColors.VerdantGreen;
                
                var bud = new SparkleParticle(
                    position + Main.rand.NextVector2Circular(10f, 10f),
                    vel,
                    budColor * 0.85f,
                    0.35f * scale,
                    Main.rand.Next(22, 38)
                );
                MagnumParticleHandler.SpawnParticle(bud);
            }
            
            // === PHASE 4: METAL ROSE BLOOMS AT TENDRIL TIPS ===
            if (withRoseBloom)
            {
                for (int i = 0; i < (int)(tendrilCount / 2); i++)
                {
                    float angle = MathHelper.TwoPi * i / (tendrilCount / 2) + Main.rand.NextFloat(-0.2f, 0.2f);
                    float distance = growthSpeed * 2.5f;
                    Vector2 rosePos = position + angle.ToRotationVector2() * distance;
                    Vector2 roseVel = angle.ToRotationVector2() * Main.rand.NextFloat(1f, 2f);
                    
                    // ★ NEW: Spawn VineRoseParticle with TwoRoses for maximum roses! ★
                    var roseVine = new VineRoseParticle(
                        rosePos,
                        roseVel * 0.5f,
                        i % 2 == 0 ? VineRoseParticle.VineType.TwoRoses : VineRoseParticle.VineType.RoseOnTop,
                        OdeToJoyColors.VerdantGreen,
                        Color.Lerp(OdeToJoyColors.RosePink, Color.White, Main.rand.NextFloat(0.2f, 0.5f)),
                        0.55f * scale,
                        Main.rand.Next(35, 50),
                        angle + MathHelper.PiOver2, // Point outward
                        0f
                    );
                    MagnumParticleHandler.SpawnParticle(roseVine);
                    
                    // Rose bloom - pink/white particles for extra density
                    for (int p = 0; p < 4; p++)
                    {
                        float petalAngle = MathHelper.TwoPi * p / 4f + angle;
                        Vector2 petalOffset = petalAngle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                        
                        Color roseColor = Color.Lerp(OdeToJoyColors.RosePink, Color.White, Main.rand.NextFloat(0.2f, 0.5f));
                        
                        var petal = new GenericGlowParticle(
                            rosePos + petalOffset,
                            roseVel + Main.rand.NextVector2Circular(1f, 1f),
                            roseColor,
                            0.35f * scale,
                            Main.rand.Next(25, 40),
                            true
                        );
                        MagnumParticleHandler.SpawnParticle(petal);
                    }
                    
                    // Rose center sparkle
                    var center = new SparkleParticle(
                        rosePos,
                        roseVel,
                        OdeToJoyColors.GoldenPollen,
                        0.35f * scale,
                        Main.rand.Next(25, 40)
                    );
                    MagnumParticleHandler.SpawnParticle(center);
                }
            }
            
            // === PHASE 5: LEAF PARTICLES ===
            for (int i = 0; i < (int)(6 * scale); i++)
            {
                Vector2 leafVel = Main.rand.NextVector2Circular(growthSpeed, growthSpeed);
                
                var leaf = new GenericGlowParticle(
                    position + Main.rand.NextVector2Circular(15f, 15f),
                    leafVel,
                    OdeToJoyColors.LeafGreen * 0.7f,
                    0.3f * scale,
                    Main.rand.Next(30, 50),
                    true
                );
                MagnumParticleHandler.SpawnParticle(leaf);
            }
            
            // === PHASE 6: GROWTH ENERGY RINGS ===
            for (int ring = 0; ring < 4; ring++)
            {
                Color ringColor = Color.Lerp(OdeToJoyColors.VerdantGreen, OdeToJoyColors.LeafGreen, ring / 4f);
                ringColor.A = 0;
                
                var halo = new BloomRingParticle(
                    position,
                    Vector2.Zero,
                    ringColor * (0.55f - ring * 0.1f),
                    (0.3f + ring * 0.18f) * scale,
                    24 + ring * 5
                );
                MagnumParticleHandler.SpawnParticle(halo);
            }
            
            // === VANILLA DUST LAYER ===
            for (int i = 0; i < (int)(14 * scale); i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(growthSpeed * 1.2f, growthSpeed * 1.2f);
                Dust green = Dust.NewDustPerfect(position, DustID.GreenFairy, dustVel, 0, default, 1.6f);
                green.noGravity = true;
            }
            
            for (int i = 0; i < (int)(6 * scale); i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(growthSpeed * 0.8f, growthSpeed * 0.8f);
                Dust pink = Dust.NewDustPerfect(position, DustID.PinkFairy, dustVel, 0, default, 1.2f);
                pink.noGravity = true;
            }
            
            // Lighting - verdant glow
            Lighting.AddLight(position, OdeToJoyColors.VerdantGreen.ToVector3() * scale * 1.0f);
        }
        
        /// <summary>
        /// 🌸 COMBINED SIGNATURE EFFECT - Uses all three unique particles together!
        /// Ultimate Ode to Joy visual - petal burst + harmonic notes + vine growth
        /// USE THIS for ultimate attacks, boss deaths, and major impacts!
        /// </summary>
        public static void OdeToJoySignatureExplosion(Vector2 position, float scale = 1f)
        {
            // Stagger the three effects for maximum visual impact
            ChromaticRosePetalBurst(position, (int)(20 * scale), 10f * scale, scale, false);
            HarmonicNoteSparkle(position, (int)(15 * scale), 7f * scale, scale * 0.9f, false);
            ChromaticVineGrowthBurst(position, (int)(10 * scale), 6f * scale, scale * 0.85f, true);
            
            // Additional white nova flash
            for (int i = 0; i < 6; i++)
            {
                Color novaColor = Color.White;
                novaColor.A = 0;
                
                var nova = new BloomParticle(
                    position,
                    Vector2.Zero,
                    novaColor * (0.9f / (i + 1)),
                    (2f - i * 0.2f) * scale,
                    25 + i * 3
                );
                MagnumParticleHandler.SpawnParticle(nova);
            }
            
            // Music note cascade
            for (int i = 0; i < (int)(8 * scale); i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 9f);
                SpawnMusicNote(position, noteVel, Color.White, 1.0f * scale);
            }
            
            // === ROSE BUD BURST - The crown jewel! ===
            // Multiple rose buds blooming outward in a spectacular display
            RoseBudParticle.SpawnBurst(
                position,
                (int)(8 * scale),      // count
                7f * scale,             // speed
                OdeToJoyColors.RosePink,
                Color.White,
                0.65f * scale,          // scale
                Main.rand.Next(40, 55)  // lifetime
            );
            
            // Secondary rose bud layer - smaller, faster
            RoseBudParticle.SpawnBurst(
                position,
                (int)(5 * scale),
                12f * scale,
                OdeToJoyColors.PetalPink,
                OdeToJoyColors.GoldenPollen,
                0.4f * scale,
                Main.rand.Next(30, 45)
            );
            
            // Maximum dust explosion
            for (int i = 0; i < (int)(25 * scale); i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(15f, 15f);
                int dustType = Main.rand.Next(3) switch
                {
                    0 => DustID.GreenFairy,
                    1 => DustID.PinkFairy,
                    _ => DustID.Enchanted_Gold
                };
                Dust dust = Dust.NewDustPerfect(position, dustType, vel, 0, default, 2f);
                dust.noGravity = true;
            }
            
            // Screen shake for emphasis
            MagnumScreenEffects.AddScreenShake(8f * scale);
            
            // Bright lighting
            Lighting.AddLight(position, Color.White.ToVector3() * scale * 1.5f);
        }
        
        #endregion
    }
    
    #endregion
    
    #region Weapon Projectiles
    
    /// <summary>
    /// Blossom Wave Projectile - Sweeping arc of petal energy
    /// </summary>
    public class BlossomWaveProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SwordArc1";
        
        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 5;
            Projectile.timeLeft = 45;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }
        
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.alpha = (int)(255 * (1f - Projectile.timeLeft / 45f));
            
            // Petal trail
            OdeToJoyVFX.PetalTrail(Projectile.Center, Projectile.velocity, 0.8f);
            
            // Music notes
            if (Main.rand.NextBool(6))
            {
                OdeToJoyVFX.SpawnMusicNote(Projectile.Center, -Projectile.velocity * 0.1f, OdeToJoyColors.RosePink, 0.8f);
            }
            
            Lighting.AddLight(Projectile.Center, OdeToJoyColors.RosePink.ToVector3() * 0.6f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            OdeToJoyVFX.HarmonicNoteSparkle(target.Center, 7, 4f, 0.55f, true);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            ProceduralProjectileVFX.DrawOdeToJoyPetalProjectile(Main.spriteBatch, Projectile, 0.4f);
            return false;
        }
    }
    
    /// <summary>
    /// Thorn Projectile - Fast moving thorn with vine trail
    /// </summary>
    public class ThornProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/VineWithRoseOnTop";
        
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
        }
        
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            
            // Enhanced vine trail with chromatic effect
            if (Main.rand.NextBool(3))
                OdeToJoyVFX.ChromaticVineGrowthBurst(Projectile.Center, 1, 2f, 0.25f, false);
            
            Lighting.AddLight(Projectile.Center, OdeToJoyColors.VerdantGreen.ToVector3() * 0.4f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            OdeToJoyVFX.ChromaticRosePetalBurst(target.Center, 5, 3f, 0.4f, false);
        }
        
        public override void OnKill(int timeLeft)
        {
            OdeToJoyVFX.ChromaticRosePetalBurst(Projectile.Center, 8, 5f, 0.5f, true);
            SoundEngine.PlaySound(SoundID.Grass, Projectile.Center);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            ProceduralProjectileVFX.DrawOdeToJoyProjectile(Main.spriteBatch, Projectile, 0.18f);
            return false;
        }
    }
    
    /// <summary>
    /// Petal Storm Projectile - Homing petal that explodes into more petals
    /// </summary>
    public class PetalStormProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/FlareSparkle";
        
        private float homingStrength = 0.05f;
        
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }
        
        public override void AI()
        {
            // Gentle homing
            NPC target = FindClosestNPC(800f);
            if (target != null)
            {
                Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * Projectile.velocity.Length(), homingStrength);
            }
            
            Projectile.rotation += 0.15f;
            
            // Beautiful petal trail
            OdeToJoyVFX.PetalTrail(Projectile.Center, Projectile.velocity, 1f);
            
            // Orbiting sparkles
            if (Projectile.timeLeft % 8 == 0)
            {
                float orbitAngle = Main.GameUpdateCount * 0.1f;
                for (int i = 0; i < 3; i++)
                {
                    float angle = orbitAngle + MathHelper.TwoPi * i / 3f;
                    Vector2 sparklePos = Projectile.Center + angle.ToRotationVector2() * 15f;
                    var sparkle = new SparkleParticle(sparklePos, Projectile.velocity * 0.3f, OdeToJoyColors.GoldenPollen, 0.3f, 12);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
            }
            
            Lighting.AddLight(Projectile.Center, OdeToJoyColors.RosePink.ToVector3() * 0.6f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            OdeToJoyVFX.HarmonicNoteSparkle(target.Center, 8, 5f, 0.7f, true);
        }
        
        public override void OnKill(int timeLeft)
        {
            // Explode into smaller petals
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f + Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 10f);
                
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, vel,
                    ModContent.ProjectileType<SmallPetalProjectile>(), Projectile.damage / 3, 1f, Projectile.owner);
            }
            
            OdeToJoyVFX.OdeToJoySignatureExplosion(Projectile.Center, 0.7f);
            SoundEngine.PlaySound(SoundID.Item27, Projectile.Center);
        }
        
        private NPC FindClosestNPC(float range)
        {
            NPC closest = null;
            float closestDist = range;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.CanBeChasedBy(Projectile))
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = npc;
                    }
                }
            }
            return closest;
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            ProceduralProjectileVFX.DrawOdeToJoyPetalProjectile(Main.spriteBatch, Projectile, 0.25f);
            return false;
        }
    }
    
    /// <summary>
    /// Small Petal Projectile - Fragment from PetalStormProjectile
    /// </summary>
    public class SmallPetalProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/RosesBud";
        
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
        }
        
        public override void AI()
        {
            Projectile.rotation += 0.2f;
            Projectile.velocity.Y += 0.1f; // Light gravity
            
            if (Main.rand.NextBool(3))
            {
                Color trailColor = OdeToJoyColors.GetPetalGradient(Main.rand.NextFloat());
                var trail = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.1f, trailColor * 0.6f, 0.2f, 15, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }
            
            Lighting.AddLight(Projectile.Center, OdeToJoyColors.RosePink.ToVector3() * 0.3f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            OdeToJoyVFX.ChromaticRosePetalBurst(target.Center, 5, 3f, 0.35f, false);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            ProceduralProjectileVFX.DrawOdeToJoyPetalProjectile(Main.spriteBatch, Projectile, 0.15f);
            return false;
        }
    }
    
    /// <summary>
    /// Glory Beam Projectile - Powerful magic beam
    /// </summary>
    public class GloryBeamProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow3";
        
        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 4;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
        }
        
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // Intense golden trail
            for (int i = 0; i < 2; i++)
            {
                Color trailColor = Color.Lerp(OdeToJoyColors.GoldenPollen, Color.White, Main.rand.NextFloat(0.3f, 0.7f));
                var trail = new GenericGlowParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                    -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(2f, 2f),
                    trailColor,
                    0.4f,
                    20,
                    true
                );
                MagnumParticleHandler.SpawnParticle(trail);
            }
            
            // Golden sparkles
            if (Main.rand.NextBool(2))
            {
                var sparkle = new SparkleParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                    -Projectile.velocity * 0.1f,
                    OdeToJoyColors.GoldenPollen,
                    0.4f,
                    18
                );
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // Music notes
            if (Main.rand.NextBool(8))
            {
                OdeToJoyVFX.SpawnMusicNote(Projectile.Center, -Projectile.velocity * 0.1f, OdeToJoyColors.GoldenPollen, 0.85f);
            }
            
            Lighting.AddLight(Projectile.Center, OdeToJoyColors.GoldenPollen.ToVector3() * 0.8f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            OdeToJoyVFX.HarmonicNoteSparkle(target.Center, 8, 5f, 0.65f, true);
            OdeToJoyVFX.ChromaticRosePetalBurst(target.Center, 6, 4f, 0.5f, false);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            ProceduralProjectileVFX.DrawOdeToJoyGoldenProjectile(Main.spriteBatch, Projectile, 0.3f);
            return false;
        }
    }
    
    /// <summary>
    /// Chainsaw Thorn Segment - Part of the Rose Thorn Chainsaw's ripchain attack
    /// </summary>
    public class ChainsawThornSegment : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/VineWithRoseOnTop";
        
        public int ParentProjectile
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        
        public int SegmentIndex
        {
            get => (int)Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 6;
        }
        
        public override void AI()
        {
            Projectile.rotation += 0.3f;
            
            // VFX trail - enhanced vine growth
            if (Main.rand.NextBool(3))
            {
                OdeToJoyVFX.ChromaticVineGrowthBurst(Projectile.Center, 1, 2f, 0.25f, false);
            }
            
            Lighting.AddLight(Projectile.Center, OdeToJoyColors.VerdantGreen.ToVector3() * 0.4f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            OdeToJoyVFX.ChromaticRosePetalBurst(target.Center, 4, 3f, 0.35f, false);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            ProceduralProjectileVFX.DrawOdeToJoyProjectile(Main.spriteBatch, Projectile, 0.16f);
            return false;
        }
    }
    
    #endregion
}
