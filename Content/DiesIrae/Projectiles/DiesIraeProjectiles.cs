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

namespace MagnumOpus.Content.DiesIrae.Projectiles
{
    #region Theme Colors
    
    /// <summary>
    /// Dies Irae theme color palette - Fiery judgment and wrath
    /// </summary>
    public static class DiesIraeColors
    {
        public static readonly Color BloodRed = new Color(139, 0, 0);
        public static readonly Color EmberOrange = new Color(255, 69, 0);
        public static readonly Color CharredBlack = new Color(25, 20, 15);
        public static readonly Color Crimson = new Color(200, 30, 30);
        public static readonly Color HellfireGold = new Color(255, 180, 50);
        public static readonly Color InfernalWhite = new Color(255, 240, 220);
        
        public static Color GetGradient(float progress)
        {
            if (progress < 0.33f)
                return Color.Lerp(CharredBlack, BloodRed, progress * 3f);
            else if (progress < 0.66f)
                return Color.Lerp(BloodRed, EmberOrange, (progress - 0.33f) * 3f);
            else
                return Color.Lerp(EmberOrange, HellfireGold, (progress - 0.66f) * 3f);
        }
    }
    
    #endregion
    
    #region VFX Helpers
    
    /// <summary>
    /// OVERHAULED Dies Irae VFX System - BRIGHT WHITE + DARK RED + BLACK FLAMES
    /// Every effect uses multi-layered rendering for maximum visual impact
    /// </summary>
    public static class DiesIraeVFX
    {
        // === CORE VISUAL IDENTITY ===
        // Layer 1: BRIGHT WHITE core (blazing center)
        // Layer 2: DARK RED energy (crimson fire)
        // Layer 3: BLACK flames/smoke (charred edges)
        
        /// <summary>
        /// Spawn a MASSIVE glowing music note with intense multi-layer bloom
        /// </summary>
        public static void SpawnMusicNote(Vector2 position, Vector2 velocity, Color baseColor, float scale = 0.8f)
        {
            // SCALE UP - Music notes must be VISIBLE
            scale = Math.Max(scale, 0.7f);
            float shimmer = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.2f;
            scale *= shimmer;
            
            // === LAYER 1: BRIGHT WHITE CORE BLOOM ===
            for (int bloom = 0; bloom < 4; bloom++)
            {
                float bloomScale = scale * (0.4f + bloom * 0.25f);
                float bloomAlpha = 0.8f / (bloom + 1);
                Color coreColor = Color.White * bloomAlpha;
                coreColor.A = 0;
                
                var coreBloom = new BloomParticle(
                    position + Main.rand.NextVector2Circular(2f, 2f),
                    velocity * 0.9f,
                    coreColor,
                    bloomScale,
                    30 + bloom * 5
                );
                MagnumParticleHandler.SpawnParticle(coreBloom);
            }
            
            // === LAYER 2: DARK RED ENERGY ===
            for (int bloom = 0; bloom < 3; bloom++)
            {
                float bloomScale = scale * (0.6f + bloom * 0.3f);
                float bloomAlpha = 0.6f / (bloom + 1);
                Color redColor = DiesIraeColors.Crimson * bloomAlpha;
                redColor.A = 0;
                
                var redBloom = new BloomParticle(
                    position + Main.rand.NextVector2Circular(4f, 4f),
                    velocity * 0.7f,
                    redColor,
                    bloomScale,
                    28 + bloom * 4
                );
                MagnumParticleHandler.SpawnParticle(redBloom);
            }
            
            // === LAYER 3: BLACK SMOKE EDGE ===
            for (int i = 0; i < 2; i++)
            {
                var smoke = new GenericGlowParticle(
                    position + Main.rand.NextVector2Circular(8f, 8f),
                    velocity * 0.4f + Main.rand.NextVector2Circular(1.5f, 1.5f),
                    DiesIraeColors.CharredBlack * 0.6f,
                    scale * 0.5f,
                    25,
                    true
                );
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // Sparkle companions - BRIGHT
            for (int i = 0; i < 3; i++)
            {
                Vector2 sparkleOffset = Main.rand.NextVector2Circular(12f, 12f);
                var sparkle = new SparkleParticle(
                    position + sparkleOffset,
                    velocity * 0.5f + Main.rand.NextVector2Circular(2f, 2f),
                    Color.Lerp(Color.White, DiesIraeColors.HellfireGold, Main.rand.NextFloat(0.3f, 0.7f)),
                    0.45f,
                    25
                );
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // Vanilla dust for extra density
            for (int i = 0; i < 2; i++)
            {
                Dust dust = Dust.NewDustPerfect(position, DustID.Torch, velocity + Main.rand.NextVector2Circular(2f, 2f), 0, default, 1.5f);
                dust.noGravity = true;
            }
        }
        
        /// <summary>
        /// MASSIVE multi-layered fire impact explosion - THE SIGNATURE EFFECT
        /// Bright white core ↁEDark red fire ↁEBlack smoke
        /// </summary>
        public static void FireImpact(Vector2 position, float scale = 1f)
        {
            // === PHASE 1: BLINDING WHITE CORE FLASH ===
            for (int i = 0; i < 5; i++)
            {
                float layerScale = scale * (1.2f - i * 0.15f);
                float alpha = 0.9f / (i + 1);
                Color coreWhite = Color.White * alpha;
                coreWhite.A = 0;
                
                var coreFlare = new BloomParticle(position, Vector2.Zero, coreWhite, layerScale, 18 + i * 2);
                MagnumParticleHandler.SpawnParticle(coreFlare);
            }
            
            // === PHASE 2: DARK RED FIRE BURST ===
            for (int i = 0; i < 4; i++)
            {
                float layerScale = scale * (1.0f - i * 0.1f);
                Color fireColor = Color.Lerp(DiesIraeColors.Crimson, DiesIraeColors.BloodRed, i / 4f);
                fireColor.A = 0;
                
                var fireFlare = new BloomParticle(position, Vector2.Zero, fireColor * (0.8f / (i + 1)), layerScale * 0.9f, 20 + i * 3);
                MagnumParticleHandler.SpawnParticle(fireFlare);
            }
            
            // === PHASE 3: HELLFIRE GOLD ACCENTS ===
            for (int i = 0; i < 3; i++)
            {
                float layerScale = scale * (0.8f - i * 0.15f);
                Color goldColor = DiesIraeColors.HellfireGold * (0.7f / (i + 1));
                goldColor.A = 0;
                
                var goldFlare = new BloomParticle(position, Vector2.Zero, goldColor, layerScale, 16 + i * 2);
                MagnumParticleHandler.SpawnParticle(goldFlare);
            }
            
            // === EXPANDING HALO RINGS - White ↁERed ↁEBlack gradient ===
            for (int i = 0; i < 8; i++)
            {
                float progress = i / 8f;
                Color haloColor;
                if (progress < 0.33f)
                    haloColor = Color.Lerp(Color.White, DiesIraeColors.Crimson, progress * 3f);
                else if (progress < 0.66f)
                    haloColor = Color.Lerp(DiesIraeColors.Crimson, DiesIraeColors.BloodRed, (progress - 0.33f) * 3f);
                else
                    haloColor = Color.Lerp(DiesIraeColors.BloodRed, DiesIraeColors.CharredBlack, (progress - 0.66f) * 3f);
                
                haloColor.A = 0;
                var halo = new BloomRingParticle(position, Vector2.Zero, haloColor * 0.8f, (0.25f + i * 0.12f) * scale, 20 + i * 2);
                MagnumParticleHandler.SpawnParticle(halo);
            }
            
            // === RADIAL FIRE PARTICLE SPRAY ===
            int fireCount = (int)(20 * scale);
            for (int i = 0; i < fireCount; i++)
            {
                float angle = MathHelper.TwoPi * i / fireCount + Main.rand.NextFloat(-0.2f, 0.2f);
                float speed = Main.rand.NextFloat(6f, 14f) * scale;
                Vector2 vel = angle.ToRotationVector2() * speed;
                
                // Gradient color based on angle
                float colorProgress = (float)i / fireCount;
                Color fireColor;
                if (colorProgress < 0.5f)
                    fireColor = Color.Lerp(Color.White, DiesIraeColors.Crimson, colorProgress * 2f);
                else
                    fireColor = Color.Lerp(DiesIraeColors.Crimson, DiesIraeColors.BloodRed, (colorProgress - 0.5f) * 2f);
                
                var fire = new GenericGlowParticle(position, vel, fireColor, 0.45f * scale, 28, true);
                MagnumParticleHandler.SpawnParticle(fire);
            }
            
            // === BLACK SMOKE RING ===
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 smokeVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 7f) * scale;
                smokeVel.Y -= 2f; // Smoke rises
                
                var smoke = new GenericGlowParticle(
                    position + Main.rand.NextVector2Circular(10f, 10f),
                    smokeVel,
                    DiesIraeColors.CharredBlack * 0.7f,
                    0.5f * scale,
                    35,
                    true
                );
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // === VANILLA DUST DENSITY LAYER ===
            // Torch dust
            for (int i = 0; i < (int)(20 * scale); i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(12f, 12f) * scale;
                Dust dust = Dust.NewDustPerfect(position, DustID.Torch, vel, 0, default, 2f);
                dust.noGravity = true;
            }
            
            // Electric sparks
            for (int i = 0; i < (int)(8 * scale); i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(8f, 8f) * scale;
                Dust spark = Dust.NewDustPerfect(position, DustID.Electric, vel, 0, DiesIraeColors.HellfireGold, 1.2f);
                spark.noGravity = true;
            }
            
            // Smoke dust
            for (int i = 0; i < (int)(10 * scale); i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f) * scale;
                vel.Y -= 3f;
                Dust smoke = Dust.NewDustPerfect(position, DustID.Smoke, vel, 100, DiesIraeColors.CharredBlack, 1.8f);
                smoke.noGravity = false;
            }
            
            // === MUSIC NOTES BURST ===
            for (int i = 0; i < (int)(6 * scale); i++)
            {
                float angle = MathHelper.TwoPi * i / 6f + Main.rand.NextFloat(-0.4f, 0.4f);
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 7f);
                Color noteColor = Main.rand.NextBool() ? Color.White : DiesIraeColors.HellfireGold;
                SpawnMusicNote(position, noteVel, noteColor, 0.85f * scale);
            }
            
            // === CROSS PARTICLE BURST - Dies Irae Signature! ===
            CrossParticleBurst(position, (int)(8 * scale), 6f * scale, 0.55f * scale);
            
            // Lighting flash
            Lighting.AddLight(position, Color.White.ToVector3() * scale * 1.5f);
        }
        
        /// <summary>
        /// ENHANCED fire trail - Multi-layered white/red/black
        /// </summary>
        public static void FireTrail(Vector2 position, Vector2 velocity, float intensity = 1f)
        {
            // === BRIGHT WHITE CORE GLOW ===
            if (Main.rand.NextBool(2))
            {
                Color coreColor = Color.White * 0.8f;
                coreColor.A = 0;
                var core = new GenericGlowParticle(
                    position,
                    -velocity * 0.05f,
                    coreColor,
                    0.25f * intensity,
                    12,
                    true
                );
                MagnumParticleHandler.SpawnParticle(core);
            }
            
            // === DARK RED FIRE LAYER ===
            if (Main.rand.NextBool(2))
            {
                Color fireColor = Color.Lerp(DiesIraeColors.Crimson, DiesIraeColors.BloodRed, Main.rand.NextFloat());
                var fire = new GenericGlowParticle(
                    position + Main.rand.NextVector2Circular(6f, 6f),
                    -velocity * 0.12f + Main.rand.NextVector2Circular(1.5f, 1.5f),
                    fireColor,
                    0.35f * intensity,
                    22,
                    true
                );
                MagnumParticleHandler.SpawnParticle(fire);
            }
            
            // === EMBER ORANGE ACCENTS ===
            if (Main.rand.NextBool(3))
            {
                Color emberColor = DiesIraeColors.EmberOrange;
                var ember = new GenericGlowParticle(
                    position + Main.rand.NextVector2Circular(8f, 8f),
                    -velocity * 0.08f + Main.rand.NextVector2Circular(2f, 2f),
                    emberColor * 0.9f,
                    0.3f * intensity,
                    18,
                    true
                );
                MagnumParticleHandler.SpawnParticle(ember);
            }
            
            // === BLACK SMOKE WISPS ===
            if (Main.rand.NextBool(4))
            {
                Vector2 smokeVel = -velocity * 0.05f + new Vector2(Main.rand.NextFloat(-1f, 1f), -Main.rand.NextFloat(1f, 3f));
                var smoke = new GenericGlowParticle(
                    position + Main.rand.NextVector2Circular(10f, 10f),
                    smokeVel,
                    DiesIraeColors.CharredBlack * 0.5f,
                    0.4f * intensity,
                    30,
                    true
                );
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // === VANILLA DUST - Torch + Smoke ===
            if (Main.rand.NextBool(2))
            {
                Dust torch = Dust.NewDustPerfect(
                    position + Main.rand.NextVector2Circular(8f, 8f),
                    DustID.Torch,
                    -velocity * 0.1f + Main.rand.NextVector2Circular(3f, 3f),
                    0, default, 1.5f * intensity
                );
                torch.noGravity = true;
            }
            
            if (Main.rand.NextBool(5))
            {
                Dust smoke = Dust.NewDustPerfect(
                    position + Main.rand.NextVector2Circular(12f, 12f),
                    DustID.Smoke,
                    new Vector2(Main.rand.NextFloat(-1f, 1f), -2f),
                    80, DiesIraeColors.CharredBlack, 1.2f * intensity
                );
                smoke.noGravity = false;
            }
            
            // === OCCASIONAL SPARKLES ===
            if (Main.rand.NextBool(6))
            {
                var sparkle = new SparkleParticle(
                    position + Main.rand.NextVector2Circular(5f, 5f),
                    -velocity * 0.05f,
                    Color.Lerp(Color.White, DiesIraeColors.HellfireGold, Main.rand.NextFloat()),
                    0.3f * intensity,
                    15
                );
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // === CROSS PARTICLE PNGS - The Dies Irae signature! ===
            CrossParticleTrail(position, velocity, 0.5f * intensity);
            
            // === MUSIC NOTE (Less frequent but visible) ===
            if (Main.rand.NextBool(10))
            {
                SpawnMusicNote(position, -velocity * 0.08f, DiesIraeColors.HellfireGold, 0.75f * intensity);
            }
        }
        
        /// <summary>
        /// DRAMATIC charge-up effect for attacks
        /// </summary>
        public static void ChargeUp(Vector2 position, float progress, float scale = 1f)
        {
            // Converging particles - White ↁERed
            int particleCount = (int)(8 + progress * 12);
            for (int i = 0; i < particleCount; i++)
            {
                float angle = MathHelper.TwoPi * i / particleCount + Main.GameUpdateCount * 0.03f;
                float radius = 150f * (1f - progress * 0.6f);
                Vector2 particlePos = position + angle.ToRotationVector2() * radius;
                
                Color chargeColor = Color.Lerp(Color.White, DiesIraeColors.Crimson, progress);
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
        /// WARNING indicator - Red danger zone
        /// </summary>
        public static void WarningFlare(Vector2 position, float scale = 1f)
        {
            // Red warning pulse
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.3f) * 0.5f + 0.5f;
            
            var warning = new BloomParticle(position, Vector2.Zero, DiesIraeColors.Crimson * pulse, 0.4f * scale, 8);
            warning.Color = warning.Color with { A = 0 };
            MagnumParticleHandler.SpawnParticle(warning);
            
            // Ground marker dust
            for (int i = 0; i < 3; i++)
            {
                Dust marker = Dust.NewDustPerfect(position + Main.rand.NextVector2Circular(20f * scale, 20f * scale), 
                    DustID.Torch, new Vector2(0, -1f), 0, default, 1f);
                marker.noGravity = true;
            }
        }
        
        /// <summary>
        /// MASSIVE death explosion - Screen-filling spectacle
        /// </summary>
        public static void DeathExplosion(Vector2 position, float scale = 1f)
        {
            // Phase 1: Blinding white nova
            for (int layer = 0; layer < 8; layer++)
            {
                float layerScale = scale * (2f - layer * 0.15f);
                Color novaColor = Color.Lerp(Color.White, DiesIraeColors.HellfireGold, layer / 8f);
                novaColor.A = 0;
                
                var nova = new BloomParticle(position, Vector2.Zero, novaColor * (0.9f / (layer + 1)), layerScale, 35 + layer * 3);
                MagnumParticleHandler.SpawnParticle(nova);
            }
            
            // Phase 2: Expanding fire rings
            for (int ring = 0; ring < 12; ring++)
            {
                float ringProgress = ring / 12f;
                Color ringColor = Color.Lerp(DiesIraeColors.Crimson, DiesIraeColors.CharredBlack, ringProgress);
                ringColor.A = 0;
                
                var fireRing = new BloomRingParticle(position, Vector2.Zero, ringColor * 0.7f, (0.4f + ring * 0.2f) * scale, 25 + ring * 3);
                MagnumParticleHandler.SpawnParticle(fireRing);
            }
            
            // Phase 3: Massive particle spray
            for (int i = 0; i < 60; i++)
            {
                float angle = MathHelper.TwoPi * i / 60f + Main.rand.NextFloat(-0.15f, 0.15f);
                float speed = Main.rand.NextFloat(8f, 20f) * scale;
                Vector2 vel = angle.ToRotationVector2() * speed;
                
                Color particleColor;
                float colorRoll = Main.rand.NextFloat();
                if (colorRoll < 0.3f)
                    particleColor = Color.White;
                else if (colorRoll < 0.6f)
                    particleColor = DiesIraeColors.Crimson;
                else if (colorRoll < 0.85f)
                    particleColor = DiesIraeColors.EmberOrange;
                else
                    particleColor = DiesIraeColors.CharredBlack;
                
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
            
            // Phase 4.5: CROSS PARTICLE EXPLOSION - Dies Irae Signature!
            CrossParticleBurst(position, (int)(16 * scale), 12f * scale, 0.7f * scale);
            
            // Phase 5: Vanilla dust storm
            for (int i = 0; i < 80; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(18f, 18f) * scale;
                int dustType = Main.rand.NextBool(3) ? DustID.Smoke : DustID.Torch;
                Dust dust = Dust.NewDustPerfect(position, dustType, vel, Main.rand.Next(50), default, Main.rand.NextFloat(1.5f, 2.5f));
                dust.noGravity = dustType == DustID.Torch;
            }
            
            // Lighting
            Lighting.AddLight(position, Color.White.ToVector3() * 2f * scale);
        }
        
        /// <summary>
        /// Chain lightning effect between two points - ENHANCED
        /// </summary>
        public static void ChainLightning(Vector2 start, Vector2 end, Color color, int segments = 8)
        {
            Vector2 direction = end - start;
            float length = direction.Length();
            direction.Normalize();
            
            Vector2 perpendicular = new Vector2(-direction.Y, direction.X);
            Vector2 lastPoint = start;
            
            for (int i = 1; i <= segments; i++)
            {
                float progress = i / (float)segments;
                Vector2 basePoint = start + direction * length * progress;
                
                // Add randomness except for endpoints
                if (i < segments)
                {
                    float amplitude = 20f * (1f - Math.Abs(progress - 0.5f) * 2f);
                    basePoint += perpendicular * Main.rand.NextFloat(-amplitude, amplitude);
                }
                
                // Draw segment with particles
                Vector2 segmentDir = basePoint - lastPoint;
                int particleCount = (int)(segmentDir.Length() / 8f);
                
                for (int p = 0; p < particleCount; p++)
                {
                    float t = p / (float)particleCount;
                    Vector2 particlePos = Vector2.Lerp(lastPoint, basePoint, t);
                    
                    var lightning = new GenericGlowParticle(
                        particlePos,
                        Main.rand.NextVector2Circular(1f, 1f),
                        color,
                        0.25f,
                        8,
                        true
                    );
                    MagnumParticleHandler.SpawnParticle(lightning);
                }
                
                // Spark at bend points
                if (i < segments && Main.rand.NextBool(2))
                {
                    var spark = new SparkleParticle(basePoint, Vector2.Zero, color, 0.4f, 10);
                    MagnumParticleHandler.SpawnParticle(spark);
                }
                
                lastPoint = basePoint;
            }
            
            // Dust for density
            for (int i = 0; i < 5; i++)
            {
                Vector2 dustPos = Vector2.Lerp(start, end, Main.rand.NextFloat());
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.Electric, Main.rand.NextVector2Circular(2f, 2f), 0, color, 0.8f);
                dust.noGravity = true;
            }
        }
        
        /// <summary>
        /// ENHANCED orbiting spark points for projectiles - Creates dynamic visual energy
        /// Overload with color parameter (center, color, radius, count, rotationAngle, scale)
        /// </summary>
        public static void OrbitingSparks(Vector2 center, Color color, float radius, int count, float rotationAngle, float scale)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = rotationAngle + MathHelper.TwoPi * i / count;
                Vector2 sparkPos = center + angle.ToRotationVector2() * radius;
                
                // White core spark
                var whiteSpark = new SparkleParticle(sparkPos, Vector2.Zero, Color.White, scale * 0.8f, 6);
                MagnumParticleHandler.SpawnParticle(whiteSpark);
                
                // Colored outer glow with custom color
                var coloredGlow = new GenericGlowParticle(sparkPos, Vector2.Zero, color * 0.7f, scale, 8, true);
                MagnumParticleHandler.SpawnParticle(coloredGlow);
            }
        }
        
        /// <summary>
        /// ENHANCED orbiting spark points for projectiles - Creates dynamic visual energy
        /// </summary>
        public static void OrbitingSparks(Vector2 center, float radius, int count, float rotationSpeed, float scale = 0.25f)
        {
            float baseAngle = Main.GameUpdateCount * rotationSpeed;
            
            for (int i = 0; i < count; i++)
            {
                float angle = baseAngle + MathHelper.TwoPi * i / count;
                Vector2 sparkPos = center + angle.ToRotationVector2() * radius;
                
                // White core spark
                var whiteSpark = new SparkleParticle(sparkPos, Vector2.Zero, Color.White, scale * 0.8f, 6);
                MagnumParticleHandler.SpawnParticle(whiteSpark);
                
                // Colored outer glow with theme gradient
                Color sparkColor = Color.Lerp(DiesIraeColors.Crimson, DiesIraeColors.HellfireGold, i / (float)count);
                var coloredGlow = new GenericGlowParticle(sparkPos, Vector2.Zero, sparkColor * 0.7f, scale, 8, true);
                MagnumParticleHandler.SpawnParticle(coloredGlow);
            }
        }
        
        /// <summary>
        /// ENHANCED flame wisp burst - Multi-layered explosion with texture sampling
        /// </summary>
        public static void FlameWispBurst(Vector2 position, float scale, int count)
        {
            float speed = 3f;
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.2f, 0.2f);
                float burstSpeed = speed * Main.rand.NextFloat(0.7f, 1.3f);
                Vector2 vel = angle.ToRotationVector2() * burstSpeed;
                
                // Progress-based color gradient
                float colorProgress = (float)i / count;
                Color wispColor;
                if (colorProgress < 0.3f)
                    wispColor = Color.Lerp(Color.White, DiesIraeColors.HellfireGold, colorProgress / 0.3f);
                else if (colorProgress < 0.6f)
                    wispColor = Color.Lerp(DiesIraeColors.HellfireGold, DiesIraeColors.Crimson, (colorProgress - 0.3f) / 0.3f);
                else
                    wispColor = Color.Lerp(DiesIraeColors.Crimson, DiesIraeColors.CharredBlack, (colorProgress - 0.6f) / 0.4f);
                
                // Main flame wisp
                var wisp = new GenericGlowParticle(position, vel, wispColor, 0.4f * scale, 25, true);
                MagnumParticleHandler.SpawnParticle(wisp);
                
                // Trailing ember
                var ember = new GenericGlowParticle(position, vel * 0.6f, DiesIraeColors.EmberOrange * 0.6f, 0.25f * scale, 30, true);
                MagnumParticleHandler.SpawnParticle(ember);
            }
            
            // Central flash
            var flash = new BloomParticle(position, Vector2.Zero, Color.White * 0.8f, 0.6f * scale, 12);
            flash.Color = flash.Color with { A = 0 };
            MagnumParticleHandler.SpawnParticle(flash);
        }
        
        /// <summary>
        /// PULSING aura effect with custom color - Creates breathing glow around projectiles
        /// </summary>
        public static void PulsingAura(Vector2 position, Color color, float baseRadius, int timer)
        {
            float pulse = 1f + (float)Math.Sin(timer * 0.15f) * 0.25f;
            float radius = baseRadius * pulse;
            
            // Outer black smoke ring
            if (Main.rand.NextBool(4))
            {
                float smokeAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 smokePos = position + smokeAngle.ToRotationVector2() * radius * 1.2f;
                var smoke = new GenericGlowParticle(smokePos, smokeAngle.ToRotationVector2() * 0.5f, DiesIraeColors.CharredBlack * 0.4f, 0.3f, 20, true);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // Colored fire ring
            if (Main.rand.NextBool(3))
            {
                float fireAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 firePos = position + fireAngle.ToRotationVector2() * radius;
                var fire = new GenericGlowParticle(firePos, fireAngle.ToRotationVector2() * 1f, color * 0.6f, 0.25f, 15, true);
                MagnumParticleHandler.SpawnParticle(fire);
            }
            
            // Inner ember sparks
            if (Main.rand.NextBool(5))
            {
                float emberAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 emberPos = position + emberAngle.ToRotationVector2() * radius * 0.6f;
                var emberSpark = new SparkleParticle(emberPos, Vector2.Zero, DiesIraeColors.EmberOrange, 0.3f, 10);
                MagnumParticleHandler.SpawnParticle(emberSpark);
            }
        }
        
        /// <summary>
        /// PULSING aura effect - Creates breathing glow around projectiles
        /// </summary>
        public static void PulsingAura(Vector2 position, float baseRadius, float intensity = 1f)
        {
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.25f;
            float radius = baseRadius * pulse;
            
            // Outer black smoke ring
            if (Main.rand.NextBool(4))
            {
                float smokeAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 smokePos = position + smokeAngle.ToRotationVector2() * radius * 1.2f;
                var smoke = new GenericGlowParticle(smokePos, smokeAngle.ToRotationVector2() * 0.5f, DiesIraeColors.CharredBlack * 0.4f, 0.3f * intensity, 20, true);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // Red fire ring
            if (Main.rand.NextBool(3))
            {
                float fireAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 firePos = position + fireAngle.ToRotationVector2() * radius;
                var fire = new GenericGlowParticle(firePos, fireAngle.ToRotationVector2() * 1f, DiesIraeColors.Crimson * 0.6f, 0.25f * intensity, 15, true);
                MagnumParticleHandler.SpawnParticle(fire);
            }
            
            // Inner ember sparks
            if (Main.rand.NextBool(5))
            {
                float emberAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 emberPos = position + emberAngle.ToRotationVector2() * radius * 0.6f;
                var emberSpark = new SparkleParticle(emberPos, Vector2.Zero, DiesIraeColors.EmberOrange, 0.3f * intensity, 10);
                MagnumParticleHandler.SpawnParticle(emberSpark);
            }
        }
        
        /// <summary>
        /// AFTERIMAGE trail effect with custom color - Creates sharp ghostly echoes
        /// </summary>
        public static void AfterimageTrail(Vector2 position, Vector2 velocity, float scale, Color color, int count)
        {
            for (int i = 0; i < count; i++)
            {
                float progress = (float)i / count;
                Vector2 offset = Main.rand.NextVector2Circular(3f, 3f) * progress;
                
                // Colored afterimage
                float alpha = 0.5f - progress * 0.3f;
                float afterScale = scale * (1f - progress * 0.3f);
                var afterimage = new GenericGlowParticle(position + offset, -velocity * 0.05f * progress, color * alpha, afterScale, 12, true);
                MagnumParticleHandler.SpawnParticle(afterimage);
            }
            
            // Black smoke afterimage
            if (Main.rand.NextBool(2))
            {
                Vector2 smokeVel = -velocity * 0.03f + new Vector2(0, -1f);
                var smokeAfter = new GenericGlowParticle(position + Main.rand.NextVector2Circular(5f, 5f), smokeVel, DiesIraeColors.CharredBlack * 0.3f, 0.35f * scale, 18, true);
                MagnumParticleHandler.SpawnParticle(smokeAfter);
            }
        }
        
        /// <summary>
        /// AFTERIMAGE trail effect - Creates sharp ghostly echoes
        /// </summary>
        public static void AfterimageTrail(Vector2 position, Vector2 velocity, float scale = 1f)
        {
            // White core afterimage
            var whiteAfter = new GenericGlowParticle(position, -velocity * 0.02f, Color.White * 0.4f, 0.2f * scale, 8, true);
            MagnumParticleHandler.SpawnParticle(whiteAfter);
            
            // Red flame afterimage
            var redAfter = new GenericGlowParticle(position + Main.rand.NextVector2Circular(3f, 3f), -velocity * 0.05f, DiesIraeColors.Crimson * 0.5f, 0.3f * scale, 12, true);
            MagnumParticleHandler.SpawnParticle(redAfter);
            
            // Black smoke afterimage
            if (Main.rand.NextBool(2))
            {
                Vector2 smokeVel = -velocity * 0.03f + new Vector2(0, -1f);
                var smokeAfter = new GenericGlowParticle(position + Main.rand.NextVector2Circular(5f, 5f), smokeVel, DiesIraeColors.CharredBlack * 0.3f, 0.35f * scale, 18, true);
                MagnumParticleHandler.SpawnParticle(smokeAfter);
            }
        }
        
        /// <summary>
        /// ENHANCED spiral trail with custom color - Creates rotating fire spiral pattern
        /// </summary>
        public static void SpiralTrail(Vector2 position, Vector2 velocity, Color color, float scale, float rotation)
        {
            int armCount = 3;
            float armRadius = 15f * scale;
            
            for (int arm = 0; arm < armCount; arm++)
            {
                float angle = rotation + MathHelper.TwoPi * arm / armCount;
                Vector2 armPos = position + angle.ToRotationVector2() * armRadius;
                
                // Flame at arm tip
                var flame = new GenericGlowParticle(armPos, -velocity * 0.08f + angle.ToRotationVector2() * 0.5f, 
                    Color.Lerp(DiesIraeColors.HellfireGold, color, arm / (float)armCount), 0.25f * scale, 12, true);
                MagnumParticleHandler.SpawnParticle(flame);
                
                // White spark
                if (Main.rand.NextBool(3))
                {
                    var spark = new SparkleParticle(armPos, angle.ToRotationVector2() * 1f, Color.White * 0.6f, 0.2f * scale, 8);
                    MagnumParticleHandler.SpawnParticle(spark);
                }
            }
            
            // Central glow
            var center = new GenericGlowParticle(position, Vector2.Zero, DiesIraeColors.EmberOrange * 0.5f, 0.2f * scale, 6, true);
            MagnumParticleHandler.SpawnParticle(center);
        }
        
        /// <summary>
        /// ENHANCED spiral trail - Creates rotating fire spiral pattern
        /// </summary>
        public static void SpiralTrail(Vector2 position, Vector2 velocity, float rotation, float scale = 1f)
        {
            int armCount = 3;
            float armRadius = 15f * scale;
            
            for (int arm = 0; arm < armCount; arm++)
            {
                float angle = rotation + MathHelper.TwoPi * arm / armCount;
                Vector2 armPos = position + angle.ToRotationVector2() * armRadius;
                
                // Flame at arm tip
                var flame = new GenericGlowParticle(armPos, -velocity * 0.08f + angle.ToRotationVector2() * 0.5f, 
                    Color.Lerp(DiesIraeColors.HellfireGold, DiesIraeColors.Crimson, arm / (float)armCount), 0.25f * scale, 12, true);
                MagnumParticleHandler.SpawnParticle(flame);
                
                // White spark
                if (Main.rand.NextBool(3))
                {
                    var spark = new SparkleParticle(armPos, angle.ToRotationVector2() * 1f, Color.White * 0.6f, 0.2f * scale, 8);
                    MagnumParticleHandler.SpawnParticle(spark);
                }
            }
            
            // Central glow
            var center = new GenericGlowParticle(position, Vector2.Zero, DiesIraeColors.EmberOrange * 0.5f, 0.2f * scale, 6, true);
            MagnumParticleHandler.SpawnParticle(center);
        }
        
        #region CrossParticle PNG Effects
        
        /// <summary>
        /// Spawns a CrossParticle texture-based particle - THE CORE CROSS EFFECT
        /// Uses actual PNG textures from Assets/Particles/CrossParticleBlack.png and CrossParticleWhite.png
        /// </summary>
        /// <param name="position">Spawn position</param>
        /// <param name="velocity">Movement velocity</param>
        /// <param name="useBlack">True for black cross, false for white cross</param>
        /// <param name="scale">Particle scale</param>
        /// <param name="rotation">Initial rotation</param>
        public static void SpawnCrossParticle(Vector2 position, Vector2 velocity, bool useBlack, float scale = 0.6f, float rotation = 0f)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            
            int texIndex = useBlack ? 0 : 1;
            Texture2D crossTex = CustomParticleSystem.CrossParticles[texIndex]?.Value;
            if (crossTex == null) return;
            
            // The cross particle is a texture-based particle with rotation
            var cross = new TexturedParticle(
                position,
                velocity,
                crossTex,
                useBlack ? DiesIraeColors.CharredBlack : Color.White,
                scale,
                35,
                rotation,
                Main.rand.NextFloat(-0.02f, 0.02f) // Slight spin
            );
            MagnumParticleHandler.SpawnParticle(cross);
            
            // Add glow layers around the cross
            Color glowColor = useBlack ? DiesIraeColors.BloodRed : DiesIraeColors.HellfireGold;
            for (int i = 0; i < 2; i++)
            {
                var glow = new GenericGlowParticle(
                    position + Main.rand.NextVector2Circular(4f, 4f),
                    velocity * 0.8f,
                    glowColor * (0.4f - i * 0.1f),
                    scale * (0.5f + i * 0.2f),
                    30,
                    true
                );
                MagnumParticleHandler.SpawnParticle(glow);
            }
        }
        
        /// <summary>
        /// Spawns a burst of cross particles - For impacts and explosions
        /// </summary>
        public static void CrossParticleBurst(Vector2 position, int count, float speed, float scale = 0.6f)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(speed * 0.7f, speed * 1.3f);
                bool useBlack = i % 2 == 0; // Alternate black/white
                float rotation = angle; // Orient outward
                
                SpawnCrossParticle(position, vel, useBlack, scale, rotation);
            }
            
            // Central flash
            CustomParticles.GenericFlare(position, DiesIraeColors.HellfireGold, 0.5f, 15);
        }
        
        /// <summary>
        /// Spawns cross particles as a trail - For projectile trails
        /// </summary>
        public static void CrossParticleTrail(Vector2 position, Vector2 velocity, float scale = 0.5f)
        {
            if (Main.rand.NextBool(3))
            {
                bool useBlack = Main.rand.NextBool();
                Vector2 trailVel = -velocity * 0.1f + Main.rand.NextVector2Circular(1.5f, 1.5f);
                float rotation = velocity.ToRotation();
                
                SpawnCrossParticle(position + Main.rand.NextVector2Circular(6f, 6f), trailVel, useBlack, scale, rotation);
            }
        }
        
        /// <summary>
        /// Spawns orbiting cross particles around a center point
        /// </summary>
        public static void OrbitingCrossParticles(Vector2 center, float radius, int count, float orbitAngle)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = orbitAngle + MathHelper.TwoPi * i / count;
                Vector2 crossPos = center + angle.ToRotationVector2() * radius;
                bool useBlack = i % 2 == 0;
                
                // Small, slow-moving orbiting crosses
                Vector2 tangentVel = (angle + MathHelper.PiOver2).ToRotationVector2() * 0.3f;
                SpawnCrossParticle(crossPos, tangentVel, useBlack, 0.4f, angle);
            }
        }
        
        #endregion
        
        #region Music Note PNG Variants
        
        /// <summary>
        /// Spawns a specific music note variant using actual PNG textures
        /// Variants: 0=MusicNote, 1=CursiveMusicNote, 2=MusicNoteWithSlashes, 3=QuarterNote, 4=TallMusicNote, 5=WholeNote
        /// </summary>
        public static void SpawnMusicNoteVariant(Vector2 position, Vector2 velocity, Color color, float scale, int variant)
        {
            if (!CustomParticleSystem.TexturesLoaded) return;
            
            variant = Math.Clamp(variant, 0, 5);
            Texture2D noteTex = CustomParticleSystem.MusicNotes[variant]?.Value;
            if (noteTex == null) return;
            
            // Scale must be visible (0.6f minimum per TRUE_VFX_STANDARDS)
            scale = Math.Max(scale, 0.6f);
            float shimmer = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.15f;
            scale *= shimmer;
            
            // The music note with bloom layers
            for (int bloom = 0; bloom < 3; bloom++)
            {
                float bloomScale = scale * (1f + bloom * 0.3f);
                float bloomAlpha = 0.6f / (bloom + 1);
                Color bloomColor = color * bloomAlpha;
                bloomColor.A = 0;
                
                var noteBloom = new TexturedParticle(
                    position + Main.rand.NextVector2Circular(bloom * 2f, bloom * 2f),
                    velocity * (1f - bloom * 0.15f),
                    noteTex,
                    bloomColor,
                    bloomScale,
                    30 + bloom * 5,
                    velocity.X * 0.01f, // Slight tilt based on velocity
                    0f
                );
                MagnumParticleHandler.SpawnParticle(noteBloom);
            }
            
            // Core note (brightest)
            var coreNote = new TexturedParticle(
                position,
                velocity,
                noteTex,
                color,
                scale,
                35,
                0f,
                0f
            );
            MagnumParticleHandler.SpawnParticle(coreNote);
            
            // Sparkle companions
            for (int i = 0; i < 2; i++)
            {
                Vector2 sparkleOffset = Main.rand.NextVector2Circular(10f, 10f);
                var sparkle = new SparkleParticle(
                    position + sparkleOffset,
                    velocity * 0.5f + Main.rand.NextVector2Circular(1f, 1f),
                    Color.Lerp(Color.White, color, 0.4f),
                    0.35f,
                    22
                );
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
        }
        
        /// <summary>
        /// Spawns music notes with weapon-specific variants for unique identity
        /// </summary>
        public static void SpawnWeaponSpecificMusicNotes(Vector2 position, Vector2 velocity, Color color, float scale, string weaponType)
        {
            // Each weapon type gets a unique combination of music note variants
            switch (weaponType.ToLower())
            {
                case "wrath": // Wrath's Cleaver - TallMusicNote + WholeNote (imposing)
                    SpawnMusicNoteVariant(position, velocity, color, scale, 4); // TallMusicNote
                    if (Main.rand.NextBool(2))
                        SpawnMusicNoteVariant(position + Main.rand.NextVector2Circular(8f, 8f), velocity * 0.7f, color, scale * 0.8f, 5); // WholeNote
                    break;
                    
                case "judgment": // Arbiter's Sentence - CursiveMusicNote + MusicNoteWithSlashes (elegant judgment)
                    SpawnMusicNoteVariant(position, velocity, color, scale, 1); // CursiveMusicNote
                    if (Main.rand.NextBool(2))
                        SpawnMusicNoteVariant(position + Main.rand.NextVector2Circular(8f, 8f), velocity * 0.7f, color, scale * 0.8f, 2); // MusicNoteWithSlashes
                    break;
                    
                case "chain": // Chain of Judgment - QuarterNote pairs (rhythmic chains)
                    for (int i = 0; i < 2; i++)
                    {
                        Vector2 offset = Main.rand.NextVector2Circular(6f, 6f);
                        SpawnMusicNoteVariant(position + offset, velocity + Main.rand.NextVector2Circular(1f, 1f), color, scale * 0.9f, 3); // QuarterNote
                    }
                    break;
                    
                case "sin": // Sin Collector - WholeNote + MusicNote (heavy, full notes)
                    SpawnMusicNoteVariant(position, velocity, color, scale, 5); // WholeNote
                    if (Main.rand.NextBool(2))
                        SpawnMusicNoteVariant(position + Main.rand.NextVector2Circular(10f, 10f), velocity * 0.6f, color, scale * 0.7f, 0); // Standard MusicNote
                    break;
                    
                case "damnation": // Damnation's Cannon - All variants cascade (chaotic)
                    int randomVariant = Main.rand.Next(6);
                    SpawnMusicNoteVariant(position, velocity, color, scale, randomVariant);
                    break;
                    
                case "grimoire": // Grimoire of Condemnation - CursiveMusicNote (scholarly)
                    SpawnMusicNoteVariant(position, velocity, color, scale, 1); // CursiveMusicNote
                    break;
                    
                case "staff": // Staff of Final Judgement - TallMusicNote + CursiveMusicNote (commanding)
                    SpawnMusicNoteVariant(position, velocity, color, scale, 4); // TallMusicNote
                    if (Main.rand.NextBool(3))
                        SpawnMusicNoteVariant(position + Main.rand.NextVector2Circular(6f, 6f), velocity * 0.8f, DiesIraeColors.HellfireGold, scale * 0.7f, 1); // CursiveMusicNote
                    break;
                    
                case "eclipse": // Eclipse of Wrath - MusicNoteWithSlashes (slashing)
                    SpawnMusicNoteVariant(position, velocity, color, scale, 2); // MusicNoteWithSlashes
                    break;
                    
                case "bell": // Death Tolling Bell - WholeNote (bell resonance)
                    SpawnMusicNoteVariant(position, velocity, color, scale * 1.1f, 5); // WholeNote (larger for bell effect)
                    break;
                    
                case "contract": // Wrathful Contract - All variants in burst (demonic chaos)
                    for (int i = 0; i < 3; i++)
                    {
                        int variant = Main.rand.Next(6);
                        Vector2 burstVel = velocity + Main.rand.NextVector2Circular(3f, 3f);
                        SpawnMusicNoteVariant(position + Main.rand.NextVector2Circular(15f, 15f), burstVel, color, scale * Main.rand.NextFloat(0.7f, 1f), variant);
                    }
                    break;
                    
                case "harmony": // Harmony of Judgement - QuarterNote + CursiveMusicNote (harmonic)
                    SpawnMusicNoteVariant(position, velocity, color, scale, 3); // QuarterNote
                    SpawnMusicNoteVariant(position + new Vector2(8f, 0f), velocity, DiesIraeColors.HellfireGold, scale * 0.85f, 1); // CursiveMusicNote
                    break;
                    
                default:
                    // Default: Standard music note
                    SpawnMusicNoteVariant(position, velocity, color, scale, 0);
                    break;
            }
        }
        
        #endregion
    }
    
    #endregion
    
    #region Wrath's Cleaver Projectiles
    
    /// <summary>
    /// Blazing cascading wave of wrathful energy from Wrath's Cleaver
    /// </summary>
    public class WrathWaveProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/TallFlamingWispProjectile";
        
        // TRUE_VFX_STANDARDS: Dies Irae blood red to orange hue range
        private const float HueMin = 0.0f;    // Blood red
        private const float HueMax = 0.08f;   // Orange-red
        
        // Color palette for this wave
        private static readonly Color WaveCore = new Color(255, 230, 200);      // White-gold core
        private static readonly Color WaveFlame = new Color(255, 150, 50);      // Gold-orange flame
        private static readonly Color WaveBlood = new Color(200, 50, 40);       // Blood red
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 5;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 50;
            Projectile.extraUpdates = 1;
        }
        
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // Expand over time
            float progress = 1f - Projectile.timeLeft / 60f;
            Projectile.scale = 1f + progress * 0.5f;
            
            // === TRUE_VFX_STANDARDS: DENSE DUST TRAIL (3+ per frame for wave) ===
            for (int i = 0; i < 3; i++)
            {
                Vector2 dustOffset = Main.rand.NextVector2Circular(15f * Projectile.scale, 15f * Projectile.scale);
                Vector2 dustVel = -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(2f, 2f);
                Dust fire = Dust.NewDustPerfect(Projectile.Center + dustOffset, DustID.Torch, dustVel, 0, default, 1.6f);
                fire.noGravity = true;
                fire.fadeIn = 1.3f;
            }
            
            // === CONTRASTING SPARKLES (1-in-2) ===
            if (Main.rand.NextBool(2))
            {
                Vector2 sparklePos = Projectile.Center + Main.rand.NextVector2Circular(12f * Projectile.scale, 12f * Projectile.scale);
                Dust spark = Dust.NewDustPerfect(sparklePos, DustID.GoldCoin, 
                    -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(2f, 2f), 0, default, 1.1f);
                spark.noGravity = true;
            }
            
            // === TRUE_VFX_STANDARDS: ORBITING MUSIC NOTES (3 notes locked to projectile) ===
            float orbitAngle = Main.GameUpdateCount * 0.1f;
            float orbitRadius = 20f * Projectile.scale;
            
            if (Main.rand.NextBool(3))
            {
                for (int i = 0; i < 3; i++)
                {
                    float noteAngle = orbitAngle + MathHelper.TwoPi * i / 3f;
                    Vector2 noteOffset = noteAngle.ToRotationVector2() * orbitRadius;
                    Vector2 notePos = Projectile.Center + noteOffset;
                    Vector2 noteVel = Projectile.velocity * 0.7f + noteAngle.ToRotationVector2() * 0.5f;
                    
                    // hslToRgb color oscillation within theme range
                    float hue = HueMin + ((float)i / 3f * (HueMax - HueMin));
                    hue += (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.02f;
                    Color noteColor = Main.hslToRgb(hue, 0.95f, 0.7f);
                    
                    DiesIraeVFX.SpawnMusicNote(notePos, noteVel, noteColor, 0.85f);
                }
            }
            
            // === FLARES LITTERING THE AIR (1-in-2) ===
            if (Main.rand.NextBool(2))
            {
                Vector2 flareOffset = Main.rand.NextVector2Circular(15f * Projectile.scale, 15f * Projectile.scale);
                float hue = HueMin + (Main.rand.NextFloat() * (HueMax - HueMin));
                Color flareColor = Main.hslToRgb(hue, 0.9f, 0.75f);
                CustomParticles.GenericFlare(Projectile.Center + flareOffset, flareColor, 0.4f * Projectile.scale, 12);
            }
            
            // Fire trail with afterimages
            DiesIraeVFX.FireTrail(Projectile.Center, Projectile.velocity, 1.2f * Projectile.scale);
            DiesIraeVFX.AfterimageTrail(Projectile.Center, Projectile.velocity, Projectile.scale);
            
            // Orbiting spark points
            if (Projectile.timeLeft % 3 == 0)
            {
                DiesIraeVFX.OrbitingSparks(Projectile.Center, 25f * Projectile.scale, 4, 0.15f, 0.3f);
            }
            
            // === DYNAMIC PARTICLE EFFECTS - Pulsing glow and concentric orbits ===
            if (Main.GameUpdateCount % 5 == 0)
            {
                PulsingGlow(Projectile.Center, Vector2.Zero, DiesIraeColors.Crimson, DiesIraeColors.HellfireGold, 0.35f * Projectile.scale, 20, 0.18f, 0.28f);
            }
            if (Main.GameUpdateCount % 30 == 0)
            {
                ConcentricOrbits(Projectile.Center, DiesIraeColors.EmberOrange, DiesIraeColors.BloodRed, 3, 4, 18f * Projectile.scale, 8f, 0.02f, 0.22f, 40);
            }
            
            // Pulsing aura
            DiesIraeVFX.PulsingAura(Projectile.Center, 20f * Projectile.scale, 1f);
            
            // Enhanced lighting with color gradient
            float lightPulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.2f) * 0.2f;
            Lighting.AddLight(Projectile.Center, WaveFlame.ToVector3() * lightPulse * 1.0f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 180);
            
            // === TRUE_VFX_STANDARDS: MULTI-LAYER FLASH CASCADE ===
            CustomParticles.GenericFlare(target.Center, Color.White, 0.9f, 20);
            CustomParticles.GenericFlare(target.Center, WaveCore, 0.7f, 18);
            CustomParticles.GenericFlare(target.Center, WaveFlame, 0.55f, 15);
            
            // 4 gradient music notes
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                float hue = HueMin + ((float)i / 4f * (HueMax - HueMin));
                Color noteColor = Main.hslToRgb(hue, 0.95f, 0.75f);
                DiesIraeVFX.SpawnMusicNote(target.Center, noteVel, noteColor, 0.8f);
            }
            
            // Halo ring
            CustomParticles.HaloRing(target.Center, WaveFlame, 0.5f, 16);
            
            // === DYNAMIC PARTICLE EFFECTS - Dies Hellfire Eruption (volcanic slam) ===
            DiesHellfireEruption(target.Center, 1.2f);
            DramaticImpact(target.Center, DiesIraeColors.InfernalWhite, DiesIraeColors.Crimson, 0.6f, 22);
            
            DiesIraeVFX.FireImpact(target.Center, 0.8f);
        }
        
        public override void OnKill(int timeLeft)
        {
            // === TRUE_VFX_STANDARDS: GLIMMER CASCADE (not puff) ===
            
            // 4-layer flash cascade
            CustomParticles.GenericFlare(Projectile.Center, Color.White, 1.0f * Projectile.scale, 22);
            CustomParticles.GenericFlare(Projectile.Center, WaveCore, 0.8f * Projectile.scale, 20);
            CustomParticles.GenericFlare(Projectile.Center, WaveFlame, 0.6f * Projectile.scale, 18);
            CustomParticles.GenericFlare(Projectile.Center, WaveBlood, 0.45f * Projectile.scale, 15);
            
            // 3 halo rings
            for (int i = 0; i < 3; i++)
            {
                float ringProgress = (float)i / 3f;
                Color ringColor = Color.Lerp(WaveFlame, WaveBlood, ringProgress);
                CustomParticles.HaloRing(Projectile.Center, ringColor, (0.4f + i * 0.15f) * Projectile.scale, 15 + i * 3);
            }
            
            // 6 gradient music notes finale
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(2.5f, 5f);
                float hue = HueMin + ((float)i / 6f * (HueMax - HueMin));
                Color noteColor = Main.hslToRgb(hue, 0.9f, 0.72f);
                DiesIraeVFX.SpawnMusicNote(Projectile.Center, noteVel, noteColor, 0.85f);
            }
            
            // 8 sparkle burst
            for (int i = 0; i < 8; i++)
            {
                Vector2 sparkleVel = Main.rand.NextVector2Circular(6f, 6f);
                float hue = HueMin + (Main.rand.NextFloat() * (HueMax - HueMin));
                Color sparkleColor = Main.hslToRgb(hue, 0.9f, 0.78f);
                var sparkle = new SparkleParticle(Projectile.Center, sparkleVel, sparkleColor, 0.4f, 20);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // 12 dust burst for density
            for (int i = 0; i < 12; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(6f, 6f);
                Dust fire = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, dustVel, 0, default, 1.4f);
                fire.noGravity = true;
            }
            
            Lighting.AddLight(Projectile.Center, WaveFlame.ToVector3() * 1.0f);
            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact, Projectile.Center);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Texture2D flareTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare").Value;
            Texture2D flareTex2 = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare4").Value;
            Texture2D glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow2").Value;
            
            Vector2 origin = texture.Size() / 2f;
            Vector2 flareOrigin = flareTex.Size() / 2f;
            Vector2 flareOrigin2 = flareTex2.Size() / 2f;
            Vector2 glowOrigin = glowTex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            float time = Main.GameUpdateCount * 0.06f;
            float pulse = 1f + (float)Math.Sin(time * 2f) * 0.15f;
            
            // === TRUE_VFX_STANDARDS: hslToRgb TRAIL GRADIENT ===
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                
                float progress = i / (float)Projectile.oldPos.Length;
                float trailAlpha = (1f - progress) * 0.6f;
                float trailScale = Projectile.scale * (1f - progress * 0.4f);
                
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                
                // hslToRgb gradient from gold to blood red
                float hue = HueMin + (progress * (HueMax - HueMin));
                Color trailColor = Main.hslToRgb(hue, 0.9f, 0.6f - progress * 0.25f) * trailAlpha;
                trailColor.A = 0;
                
                Main.spriteBatch.Draw(texture, trailPos, null, trailColor, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0f);
            }
            
            // === TRUE_VFX_STANDARDS: 5+ SPINNING FLARE LAYERS ===
            
            // Layer 1: Soft glow base (large, dim)
            Color glowColor1 = WaveFlame * 0.35f;
            glowColor1.A = 0;
            Main.spriteBatch.Draw(glowTex, drawPos, null, glowColor1, 0f, glowOrigin, 0.7f * Projectile.scale * pulse, SpriteEffects.None, 0f);
            
            // Layer 2: First flare spinning clockwise
            Color flareColor1 = WaveFlame * 0.6f;
            flareColor1.A = 0;
            Main.spriteBatch.Draw(flareTex, drawPos, null, flareColor1, time, flareOrigin, 0.45f * Projectile.scale * pulse, SpriteEffects.None, 0f);
            
            // Layer 3: Second flare spinning counter-clockwise
            Color flareColor2 = WaveBlood * 0.55f;
            flareColor2.A = 0;
            Main.spriteBatch.Draw(flareTex2, drawPos, null, flareColor2, -time * 0.75f, flareOrigin2, 0.38f * Projectile.scale * pulse, SpriteEffects.None, 0f);
            
            // Layer 4: Third flare different speed
            Color flareColor3 = WaveCore * 0.65f;
            flareColor3.A = 0;
            Main.spriteBatch.Draw(flareTex, drawPos, null, flareColor3, time * 1.3f, flareOrigin, 0.3f * Projectile.scale * pulse, SpriteEffects.None, 0f);
            
            // Layer 5: Fourth flare accent
            Color flareColor4 = Color.Lerp(WaveFlame, Color.White, 0.3f) * 0.5f;
            flareColor4.A = 0;
            Main.spriteBatch.Draw(flareTex2, drawPos, null, flareColor4, -time * 0.5f, flareOrigin2, 0.22f * Projectile.scale * pulse, SpriteEffects.None, 0f);
            
            // Layer 6: White hot center
            Color whiteCore = Color.White * 0.7f;
            whiteCore.A = 0;
            Main.spriteBatch.Draw(flareTex, drawPos, null, whiteCore, 0f, flareOrigin, 0.15f * Projectile.scale, SpriteEffects.None, 0f);
            
            // === 4 ORBITING SPARK POINTS ===
            float orbitAngle = Main.GameUpdateCount * 0.08f;
            for (int i = 0; i < 4; i++)
            {
                float sparkAngle = orbitAngle + MathHelper.TwoPi * i / 4f;
                Vector2 sparkPos = drawPos + sparkAngle.ToRotationVector2() * (15f * Projectile.scale);
                Color sparkColor = Color.Lerp(WaveFlame, Color.White, 0.4f) * 0.6f;
                sparkColor.A = 0;
                Main.spriteBatch.Draw(flareTex, sparkPos, null, sparkColor, sparkAngle, flareOrigin, 0.1f * pulse, SpriteEffects.None, 0f);
            }
            
            return false;
        }
    }
    
    /// <summary>
    /// Crystallized flame projectile that homes and explodes (every 3rd swing)
    /// </summary>
    public class CrystallizedFlameProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/FlamingWispProjectileSmall";
        
        // TRUE_VFX_STANDARDS: Dies Irae blood red to orange hue range
        private const float HueMin = 0.0f;    // Blood red
        private const float HueMax = 0.08f;   // Orange-red
        
        // Color palette for this crystal
        private static readonly Color CrystalCore = new Color(255, 240, 200);    // White-gold core
        private static readonly Color CrystalFlame = new Color(255, 160, 60);    // Crystal gold-orange
        private static readonly Color CrystalBlood = new Color(220, 60, 40);     // Crystal blood red
        
        private float homingStrength = 0f;
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }
        
        public override void AI()
        {
            // Increase homing over time
            homingStrength = Math.Min(0.15f, homingStrength + 0.003f);
            
            // Find target
            NPC target = FindClosestNPC(800f);
            if (target != null)
            {
                Vector2 direction = target.Center - Projectile.Center;
                direction.Normalize();
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * 14f, homingStrength);
            }
            
            Projectile.rotation += 0.2f;
            
            // === TRUE_VFX_STANDARDS: DENSE DUST TRAIL (2+ per frame guaranteed) ===
            for (int i = 0; i < 2; i++)
            {
                Vector2 dustOffset = Main.rand.NextVector2Circular(6f, 6f);
                Vector2 dustVel = -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1.5f, 1.5f);
                Dust fire = Dust.NewDustPerfect(Projectile.Center + dustOffset, DustID.Torch, dustVel, 0, default, 1.4f);
                fire.noGravity = true;
                fire.fadeIn = 1.2f;
            }
            
            // === CONTRASTING SPARKLES (1-in-2) ===
            if (Main.rand.NextBool(2))
            {
                Dust spark = Dust.NewDustPerfect(Projectile.Center, DustID.GoldCoin, 
                    -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1.5f, 1.5f), 0, default, 1.0f);
                spark.noGravity = true;
            }
            
            // Electric sparks for crystal feel
            if (Main.rand.NextBool(4))
            {
                Dust electric = Dust.NewDustPerfect(Projectile.Center, DustID.Electric, 
                    Main.rand.NextVector2Circular(3f, 3f), 0, CrystalFlame, 0.9f);
                electric.noGravity = true;
            }
            
            // === TRUE_VFX_STANDARDS: ORBITING MUSIC NOTES (3 notes locked to projectile) ===
            float orbitAngle = Main.GameUpdateCount * 0.09f;
            float orbitRadius = 12f;
            
            if (Main.rand.NextBool(4))
            {
                for (int i = 0; i < 3; i++)
                {
                    float noteAngle = orbitAngle + MathHelper.TwoPi * i / 3f;
                    Vector2 noteOffset = noteAngle.ToRotationVector2() * orbitRadius;
                    Vector2 notePos = Projectile.Center + noteOffset;
                    Vector2 noteVel = Projectile.velocity * 0.6f + noteAngle.ToRotationVector2() * 0.4f;
                    
                    // hslToRgb color oscillation within theme range
                    float hue = HueMin + ((float)i / 3f * (HueMax - HueMin));
                    hue += (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.015f;
                    Color noteColor = Main.hslToRgb(hue, 0.95f, 0.72f);
                    
                    DiesIraeVFX.SpawnMusicNote(notePos, noteVel, noteColor, 0.8f);
                }
            }
            
            // === FLARES LITTERING THE AIR (1-in-2) ===
            if (Main.rand.NextBool(2))
            {
                Vector2 flareOffset = Main.rand.NextVector2Circular(8f, 8f);
                float hue = HueMin + (Main.rand.NextFloat() * (HueMax - HueMin));
                Color flareColor = Main.hslToRgb(hue, 0.9f, 0.75f);
                CustomParticles.GenericFlare(Projectile.Center + flareOffset, flareColor, 0.4f, 12);
            }
            
            // Crystal glow particles
            if (Main.rand.NextBool(2))
            {
                Color trailColor = Main.rand.NextBool() ? CrystalBlood : CrystalFlame;
                var crystal = new GenericGlowParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                    -Projectile.velocity * 0.1f,
                    trailColor,
                    0.35f,
                    18,
                    true
                );
                MagnumParticleHandler.SpawnParticle(crystal);
            }
            
            // Fire trail with afterimages
            DiesIraeVFX.SpiralTrail(Projectile.Center, Projectile.velocity, Projectile.rotation * 2f, 0.8f);
            DiesIraeVFX.FireTrail(Projectile.Center, Projectile.velocity, 0.9f);
            
            // === DYNAMIC PARTICLE EFFECTS - Twinkling sparks and pulsing glow ===
            if (Main.rand.NextBool(3))
            {
                TwinklingSparks(Projectile.Center, DiesIraeColors.HellfireGold, 2, 20f, 0.25f, 28);
            }
            if (Main.GameUpdateCount % 6 == 0)
            {
                PulsingGlow(Projectile.Center, Vector2.Zero, DiesIraeColors.Crimson, DiesIraeColors.EmberOrange, 0.32f, 18, 0.15f, 0.22f);
            }
            
            // Dynamic pulsing lighting
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.18f) * 0.2f;
            Lighting.AddLight(Projectile.Center, CrystalFlame.ToVector3() * 0.75f * pulse);
        }
        
        private NPC FindClosestNPC(float maxDistance)
        {
            NPC closest = null;
            float closestDist = maxDistance;
            
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.CanBeChasedBy())
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
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 300);
            
            // === TRUE_VFX_STANDARDS: MULTI-LAYER FLASH CASCADE ===
            CustomParticles.GenericFlare(target.Center, Color.White, 0.9f, 20);
            CustomParticles.GenericFlare(target.Center, CrystalCore, 0.7f, 18);
            CustomParticles.GenericFlare(target.Center, CrystalFlame, 0.55f, 16);
            
            // 4 gradient music notes
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(2.5f, 4f);
                float hue = HueMin + ((float)i / 4f * (HueMax - HueMin));
                Color noteColor = Main.hslToRgb(hue, 0.95f, 0.75f);
                DiesIraeVFX.SpawnMusicNote(target.Center, noteVel, noteColor, 0.85f);
            }
            
            // Halo ring
            CustomParticles.HaloRing(target.Center, CrystalFlame, 0.5f, 16);
            
            // === DYNAMIC PARTICLE EFFECTS - Dies Judgment Vortex (crystallized magic) ===
            DiesJudgmentVortex(target.Center, 1f);
            SpiralBurst(target.Center, DiesIraeColors.Crimson, DiesIraeColors.HellfireGold, 6, 0.15f, 4f, 0.35f, 24);
        }
        
        public override void OnKill(int timeLeft)
        {
            // === TRUE_VFX_STANDARDS: GLIMMER CASCADE (not puff) ===
            
            // 4-layer flash cascade
            CustomParticles.GenericFlare(Projectile.Center, Color.White, 1.1f, 24);
            CustomParticles.GenericFlare(Projectile.Center, CrystalCore, 0.85f, 22);
            CustomParticles.GenericFlare(Projectile.Center, CrystalFlame, 0.65f, 20);
            CustomParticles.GenericFlare(Projectile.Center, CrystalBlood, 0.5f, 18);
            
            // 3 halo rings
            for (int i = 0; i < 3; i++)
            {
                float ringProgress = (float)i / 3f;
                Color ringColor = Color.Lerp(CrystalFlame, CrystalBlood, ringProgress);
                CustomParticles.HaloRing(Projectile.Center, ringColor, 0.45f + i * 0.12f, 16 + i * 3);
            }
            
            // 6 gradient music notes finale
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 5f);
                float hue = HueMin + ((float)i / 6f * (HueMax - HueMin));
                Color noteColor = Main.hslToRgb(hue, 0.9f, 0.72f);
                DiesIraeVFX.SpawnMusicNote(Projectile.Center, noteVel, noteColor, 0.85f);
            }
            
            // 8 sparkle burst
            for (int i = 0; i < 8; i++)
            {
                Vector2 sparkleVel = Main.rand.NextVector2Circular(5f, 5f);
                float hue = HueMin + (Main.rand.NextFloat() * (HueMax - HueMin));
                Color sparkleColor = Main.hslToRgb(hue, 0.9f, 0.78f);
                var sparkle = new SparkleParticle(Projectile.Center, sparkleVel, sparkleColor, 0.4f, 20);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // 10 dust burst for density
            for (int i = 0; i < 10; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(5f, 5f);
                Dust fire = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, dustVel, 0, default, 1.3f);
                fire.noGravity = true;
            }
            
            // Explosion damage
            Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, Vector2.Zero,
                ProjectileID.DD2ExplosiveTrapT3Explosion, Projectile.damage / 2, 0f, Projectile.owner);
            
            Lighting.AddLight(Projectile.Center, CrystalFlame.ToVector3() * 1.0f);
            SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode, Projectile.Center);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Texture2D flareTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare").Value;
            Texture2D flareTex2 = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare4").Value;
            Texture2D glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow2").Value;
            
            Vector2 origin = texture.Size() / 2f;
            Vector2 flareOrigin = flareTex.Size() / 2f;
            Vector2 flareOrigin2 = flareTex2.Size() / 2f;
            Vector2 glowOrigin = glowTex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            float time = Main.GameUpdateCount * 0.07f;
            float pulse = 1f + (float)Math.Sin(time * 2f) * 0.15f;
            
            // === TRUE_VFX_STANDARDS: 5+ SPINNING FLARE LAYERS ===
            
            // Layer 1: Soft glow base (large, dim)
            Color glowColor1 = CrystalFlame * 0.35f;
            glowColor1.A = 0;
            Main.spriteBatch.Draw(glowTex, drawPos, null, glowColor1, 0f, glowOrigin, 0.6f * pulse, SpriteEffects.None, 0f);
            
            // Layer 2: First flare spinning clockwise
            Color flareColor1 = CrystalFlame * 0.6f;
            flareColor1.A = 0;
            Main.spriteBatch.Draw(flareTex, drawPos, null, flareColor1, time, flareOrigin, 0.4f * pulse, SpriteEffects.None, 0f);
            
            // Layer 3: Second flare spinning counter-clockwise
            Color flareColor2 = CrystalBlood * 0.55f;
            flareColor2.A = 0;
            Main.spriteBatch.Draw(flareTex2, drawPos, null, flareColor2, -time * 0.75f, flareOrigin2, 0.32f * pulse, SpriteEffects.None, 0f);
            
            // Layer 4: Third flare different speed
            Color flareColor3 = CrystalCore * 0.65f;
            flareColor3.A = 0;
            Main.spriteBatch.Draw(flareTex, drawPos, null, flareColor3, time * 1.4f, flareOrigin, 0.25f * pulse, SpriteEffects.None, 0f);
            
            // Layer 5: Fourth flare accent
            float hue = HueMin + ((float)Math.Sin(time) * 0.5f + 0.5f) * (HueMax - HueMin);
            Color hslColor = Main.hslToRgb(hue, 0.9f, 0.7f) * 0.5f;
            hslColor.A = 0;
            Main.spriteBatch.Draw(flareTex2, drawPos, null, hslColor, -time * 0.5f, flareOrigin2, 0.2f * pulse, SpriteEffects.None, 0f);
            
            // Layer 6: White hot center
            Color whiteCore = Color.White * 0.7f;
            whiteCore.A = 0;
            Main.spriteBatch.Draw(flareTex, drawPos, null, whiteCore, 0f, flareOrigin, 0.12f, SpriteEffects.None, 0f);
            
            // === 3 ORBITING SPARK POINTS ===
            float orbitAngle = Main.GameUpdateCount * 0.08f;
            for (int i = 0; i < 3; i++)
            {
                float sparkAngle = orbitAngle + MathHelper.TwoPi * i / 3f;
                Vector2 sparkPos = drawPos + sparkAngle.ToRotationVector2() * 10f;
                Color sparkColor = Color.Lerp(CrystalFlame, Color.White, 0.4f) * 0.6f;
                sparkColor.A = 0;
                Main.spriteBatch.Draw(flareTex, sparkPos, null, sparkColor, sparkAngle, flareOrigin, 0.08f * pulse, SpriteEffects.None, 0f);
            }
            
            return false;
        }
    }
    
    #endregion
    
    #region Executioner's Verdict Projectiles
    
    /// <summary>
    /// Ignited bolt that tracks enemies then spawns spectral swords
    /// </summary>
    public class IgnitedWrathBolt : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/FlamingWispProjectileSmall";
        
        // TRUE_VFX_STANDARDS: Dies Irae blood red to orange hue range
        private const float HueMin = 0.0f;    // Blood red
        private const float HueMax = 0.08f;   // Orange-red
        
        // Color palette for this bolt
        private static readonly Color BoltCore = new Color(255, 235, 190);     // White-gold core
        private static readonly Color BoltFlame = new Color(255, 145, 55);     // Gold-orange flame
        private static readonly Color BoltBlood = new Color(210, 55, 35);      // Blood red
        
        private int targetNPC = -1;
        private bool hasExploded = false;
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }
        
        public override void AI()
        {
            // Find target on first frame
            if (Projectile.ai[0] == 0)
            {
                targetNPC = FindClosestNPCIndex(1000f);
                Projectile.ai[0] = 1;
            }
            
            // Track target
            if (targetNPC >= 0 && targetNPC < Main.maxNPCs && Main.npc[targetNPC].active)
            {
                NPC target = Main.npc[targetNPC];
                Vector2 direction = target.Center - Projectile.Center;
                direction.Normalize();
                
                float speed = Projectile.velocity.Length();
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * speed, 0.08f);
                
                // Check for proximity explosion
                if (Vector2.Distance(Projectile.Center, target.Center) < 50f && !hasExploded)
                {
                    ExplodeAndSpawnSwords(target);
                }
            }
            
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            
            // === TRUE_VFX_STANDARDS: DENSE DUST TRAIL (2+ per frame guaranteed) ===
            for (int i = 0; i < 2; i++)
            {
                Vector2 dustOffset = Main.rand.NextVector2Circular(5f, 5f);
                Vector2 dustVel = -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1.5f, 1.5f);
                Dust fire = Dust.NewDustPerfect(Projectile.Center + dustOffset, DustID.Torch, dustVel, 0, default, 1.4f);
                fire.noGravity = true;
                fire.fadeIn = 1.2f;
            }
            
            // === CONTRASTING SPARKLES (1-in-2) ===
            if (Main.rand.NextBool(2))
            {
                Dust spark = Dust.NewDustPerfect(Projectile.Center, DustID.GoldCoin, 
                    -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1.5f, 1.5f), 0, default, 1.0f);
                spark.noGravity = true;
            }
            
            // Electric dust for tracking feel
            if (Main.rand.NextBool(3))
            {
                Dust electric = Dust.NewDustPerfect(Projectile.Center, DustID.Electric, 
                    Main.rand.NextVector2Circular(2f, 2f), 0, BoltFlame, 0.8f);
                electric.noGravity = true;
            }
            
            // === TRUE_VFX_STANDARDS: ORBITING MUSIC NOTES (3 notes locked to projectile) ===
            float orbitAngle = Main.GameUpdateCount * 0.1f;
            float orbitRadius = 10f;
            
            if (Main.rand.NextBool(4))
            {
                for (int i = 0; i < 3; i++)
                {
                    float noteAngle = orbitAngle + MathHelper.TwoPi * i / 3f;
                    Vector2 noteOffset = noteAngle.ToRotationVector2() * orbitRadius;
                    Vector2 notePos = Projectile.Center + noteOffset;
                    Vector2 noteVel = Projectile.velocity * 0.6f + noteAngle.ToRotationVector2() * 0.4f;
                    
                    // hslToRgb color oscillation within theme range
                    float hue = HueMin + ((float)i / 3f * (HueMax - HueMin));
                    hue += (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.015f;
                    Color noteColor = Main.hslToRgb(hue, 0.95f, 0.72f);
                    
                    DiesIraeVFX.SpawnMusicNote(notePos, noteVel, noteColor, 0.75f);
                }
            }
            
            // === FLARES LITTERING THE AIR (1-in-2) ===
            if (Main.rand.NextBool(2))
            {
                Vector2 flareOffset = Main.rand.NextVector2Circular(6f, 6f);
                float hue = HueMin + (Main.rand.NextFloat() * (HueMax - HueMin));
                Color flareColor = Main.hslToRgb(hue, 0.9f, 0.75f);
                CustomParticles.GenericFlare(Projectile.Center + flareOffset, flareColor, 0.35f, 12);
            }
            
            // Fire trail with afterimages
            DiesIraeVFX.FireTrail(Projectile.Center, Projectile.velocity, 0.9f);
            DiesIraeVFX.AfterimageTrail(Projectile.Center, Projectile.velocity, 0.7f);
            
            // Pulsing aura when targeting
            if (targetNPC >= 0)
            {
                DiesIraeVFX.PulsingAura(Projectile.Center, 12f, 0.7f);
            }
            
            // Dynamic pulsing light
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.2f) * 0.2f;
            Lighting.AddLight(Projectile.Center, BoltFlame.ToVector3() * 0.7f * pulse);
        }
        
        private int FindClosestNPCIndex(float maxDistance)
        {
            int closest = -1;
            float closestDist = maxDistance;
            
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.CanBeChasedBy())
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = i;
                    }
                }
            }
            
            return closest;
        }
        
        private void ExplodeAndSpawnSwords(NPC target)
        {
            hasExploded = true;
            
            // === TRUE_VFX_STANDARDS: GLIMMER CASCADE ON EXPLOSION ===
            CustomParticles.GenericFlare(Projectile.Center, Color.White, 1.0f, 22);
            CustomParticles.GenericFlare(Projectile.Center, BoltCore, 0.8f, 20);
            CustomParticles.GenericFlare(Projectile.Center, BoltFlame, 0.6f, 18);
            
            // 3 halo rings
            for (int i = 0; i < 3; i++)
            {
                float ringProgress = (float)i / 3f;
                Color ringColor = Color.Lerp(BoltFlame, BoltBlood, ringProgress);
                CustomParticles.HaloRing(Projectile.Center, ringColor, 0.4f + i * 0.12f, 15 + i * 3);
            }
            
            // 6 gradient music notes
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(2.5f, 4.5f);
                float hue = HueMin + ((float)i / 6f * (HueMax - HueMin));
                Color noteColor = Main.hslToRgb(hue, 0.9f, 0.72f);
                DiesIraeVFX.SpawnMusicNote(Projectile.Center, noteVel, noteColor, 0.85f);
            }
            
            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact, Projectile.Center);
            
            // Spawn 3 spectral swords
            if (Main.myPlayer == Projectile.owner)
            {
                for (int i = 0; i < 3; i++)
                {
                    float angle = MathHelper.TwoPi * i / 3f;
                    Vector2 spawnPos = target.Center + angle.ToRotationVector2() * 100f;
                    Vector2 velocity = (target.Center - spawnPos).SafeNormalize(Vector2.Zero) * 12f;
                    
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPos, velocity,
                        ModContent.ProjectileType<SpectralVerdictSword>(), Projectile.damage, Projectile.knockBack, Projectile.owner, target.whoAmI);
                }
            }
            
            Projectile.Kill();
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 240);
            if (!hasExploded)
            {
                ExplodeAndSpawnSwords(target);
            }
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Texture2D flareTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare").Value;
            Texture2D flareTex2 = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare4").Value;
            Texture2D glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow2").Value;
            
            Vector2 origin = texture.Size() / 2f;
            Vector2 flareOrigin = flareTex.Size() / 2f;
            Vector2 flareOrigin2 = flareTex2.Size() / 2f;
            Vector2 glowOrigin = glowTex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            float time = Main.GameUpdateCount * 0.07f;
            float pulse = 1f + (float)Math.Sin(time * 2f) * 0.12f;
            
            // === TRUE_VFX_STANDARDS: 5+ SPINNING FLARE LAYERS ===
            
            // Layer 1: Soft glow base (large, dim)
            Color glowColor1 = BoltFlame * 0.35f;
            glowColor1.A = 0;
            Main.spriteBatch.Draw(glowTex, drawPos, null, glowColor1, 0f, glowOrigin, 0.5f * pulse, SpriteEffects.None, 0f);
            
            // Layer 2: First flare spinning clockwise
            Color flareColor1 = BoltFlame * 0.6f;
            flareColor1.A = 0;
            Main.spriteBatch.Draw(flareTex, drawPos, null, flareColor1, time, flareOrigin, 0.35f * pulse, SpriteEffects.None, 0f);
            
            // Layer 3: Second flare spinning counter-clockwise
            Color flareColor2 = BoltBlood * 0.55f;
            flareColor2.A = 0;
            Main.spriteBatch.Draw(flareTex2, drawPos, null, flareColor2, -time * 0.75f, flareOrigin2, 0.28f * pulse, SpriteEffects.None, 0f);
            
            // Layer 4: Third flare different speed
            Color flareColor3 = BoltCore * 0.65f;
            flareColor3.A = 0;
            Main.spriteBatch.Draw(flareTex, drawPos, null, flareColor3, time * 1.35f, flareOrigin, 0.22f * pulse, SpriteEffects.None, 0f);
            
            // Layer 5: Fourth flare hslToRgb accent
            float hue = HueMin + ((float)Math.Sin(time) * 0.5f + 0.5f) * (HueMax - HueMin);
            Color hslColor = Main.hslToRgb(hue, 0.9f, 0.7f) * 0.5f;
            hslColor.A = 0;
            Main.spriteBatch.Draw(flareTex2, drawPos, null, hslColor, -time * 0.55f, flareOrigin2, 0.18f * pulse, SpriteEffects.None, 0f);
            
            // Layer 6: White hot center
            Color whiteCore = Color.White * 0.7f;
            whiteCore.A = 0;
            Main.spriteBatch.Draw(flareTex, drawPos, null, whiteCore, 0f, flareOrigin, 0.1f, SpriteEffects.None, 0f);
            
            // === 3 ORBITING SPARK POINTS ===
            float orbitAngle = Main.GameUpdateCount * 0.08f;
            for (int i = 0; i < 3; i++)
            {
                float sparkAngle = orbitAngle + MathHelper.TwoPi * i / 3f;
                Vector2 sparkPos = drawPos + sparkAngle.ToRotationVector2() * 8f;
                Color sparkColor = Color.Lerp(BoltFlame, Color.White, 0.4f) * 0.6f;
                sparkColor.A = 0;
                Main.spriteBatch.Draw(flareTex, sparkPos, null, sparkColor, sparkAngle, flareOrigin, 0.07f * pulse, SpriteEffects.None, 0f);
            }
            
            return false;
        }
    }
    
    /// <summary>
    /// Spectral sword that strikes the target after bolt explosion
    /// </summary>
    public class SpectralVerdictSword : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/DiesIrae/ResonantWeapons/ExecutionersVerdict";
        
        // TRUE_VFX_STANDARDS: Dies Irae blood red to orange hue range
        private const float HueMin = 0.0f;    // Blood red
        private const float HueMax = 0.08f;   // Orange-red
        
        // Color palette for this spectral sword
        private static readonly Color SwordCore = new Color(255, 240, 210);    // White-gold core
        private static readonly Color SwordFlame = new Color(255, 150, 55);    // Gold-orange flame
        private static readonly Color SwordBlood = new Color(215, 50, 35);     // Blood red
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 100;
        }
        
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            
            // === TRUE_VFX_STANDARDS: DENSE DUST TRAIL (2+ per frame guaranteed) ===
            for (int i = 0; i < 2; i++)
            {
                Vector2 dustOffset = Main.rand.NextVector2Circular(8f, 8f);
                Vector2 dustVel = -Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(1.8f, 1.8f);
                Dust fire = Dust.NewDustPerfect(Projectile.Center + dustOffset, DustID.Torch, dustVel, 0, default, 1.5f);
                fire.noGravity = true;
                fire.fadeIn = 1.3f;
            }
            
            // === CONTRASTING SPARKLES (1-in-2) ===
            if (Main.rand.NextBool(2))
            {
                Dust spark = Dust.NewDustPerfect(Projectile.Center, DustID.GoldCoin, 
                    -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(2f, 2f), 0, default, 1.1f);
                spark.noGravity = true;
            }
            
            // === SPECTRAL GLOW TRAIL ===
            for (int i = 0; i < 2; i++)
            {
                Color trailColor = Color.Lerp(SwordBlood, SwordFlame, Main.rand.NextFloat()) * 0.7f;
                
                var trail = new GenericGlowParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                    -Projectile.velocity * 0.2f,
                    trailColor,
                    0.45f,
                    15,
                    true
                );
                MagnumParticleHandler.SpawnParticle(trail);
            }
            
            // === TRUE_VFX_STANDARDS: ORBITING MUSIC NOTES (2 notes locked to projectile) ===
            float orbitAngle = Main.GameUpdateCount * 0.12f;
            float orbitRadius = 18f;
            
            if (Main.rand.NextBool(3))
            {
                for (int i = 0; i < 2; i++)
                {
                    float noteAngle = orbitAngle + MathHelper.TwoPi * i / 2f;
                    Vector2 noteOffset = noteAngle.ToRotationVector2() * orbitRadius;
                    Vector2 notePos = Projectile.Center + noteOffset;
                    Vector2 noteVel = Projectile.velocity * 0.5f + noteAngle.ToRotationVector2() * 0.5f;
                    
                    // hslToRgb color oscillation within theme range
                    float hue = HueMin + ((float)i / 2f * (HueMax - HueMin));
                    hue += (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.015f;
                    Color noteColor = Main.hslToRgb(hue, 0.95f, 0.72f);
                    
                    DiesIraeVFX.SpawnMusicNote(notePos, noteVel, noteColor, 0.8f);
                }
            }
            
            // === FLARES LITTERING THE AIR (1-in-2) ===
            if (Main.rand.NextBool(2))
            {
                Vector2 flareOffset = Main.rand.NextVector2Circular(10f, 10f);
                float hue = HueMin + (Main.rand.NextFloat() * (HueMax - HueMin));
                Color flareColor = Main.hslToRgb(hue, 0.9f, 0.75f);
                CustomParticles.GenericFlare(Projectile.Center + flareOffset, flareColor, 0.4f, 14);
            }
            
            // Dynamic pulsing light
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.15f;
            Lighting.AddLight(Projectile.Center, SwordFlame.ToVector3() * 0.7f * pulse);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 180);
            
            // === TRUE_VFX_STANDARDS: MULTI-LAYER FLASH CASCADE ===
            CustomParticles.GenericFlare(target.Center, Color.White, 0.9f, 18);
            CustomParticles.GenericFlare(target.Center, SwordCore, 0.7f, 16);
            CustomParticles.GenericFlare(target.Center, SwordFlame, 0.55f, 14);
            
            // 4 gradient music notes
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                float hue = HueMin + ((float)i / 4f * (HueMax - HueMin));
                Color noteColor = Main.hslToRgb(hue, 0.9f, 0.72f);
                DiesIraeVFX.SpawnMusicNote(target.Center, noteVel, noteColor, 0.8f);
            }
            
            CustomParticles.HaloRing(target.Center, SwordFlame, 0.4f, 15);
            
            // === DYNAMIC PARTICLE EFFECTS - Dies Hellfire Eruption (sword strike) ===
            DiesHellfireEruption(target.Center, 0.9f);
        }
        
        public override void OnKill(int timeLeft)
        {
            // === TRUE_VFX_STANDARDS: GLIMMER CASCADE (not puff) ===
            CustomParticles.GenericFlare(Projectile.Center, Color.White, 0.9f, 20);
            CustomParticles.GenericFlare(Projectile.Center, SwordCore, 0.7f, 18);
            CustomParticles.GenericFlare(Projectile.Center, SwordFlame, 0.55f, 16);
            
            // 2 halo rings
            CustomParticles.HaloRing(Projectile.Center, SwordFlame, 0.4f, 14);
            CustomParticles.HaloRing(Projectile.Center, SwordBlood, 0.32f, 12);
            
            // 4 gradient music notes
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                float hue = HueMin + ((float)i / 4f * (HueMax - HueMin));
                Color noteColor = Main.hslToRgb(hue, 0.9f, 0.72f);
                DiesIraeVFX.SpawnMusicNote(Projectile.Center, noteVel, noteColor, 0.8f);
            }
            
            // 6 sparkle burst
            for (int i = 0; i < 6; i++)
            {
                Vector2 sparkleVel = Main.rand.NextVector2Circular(5f, 5f);
                Color sparkleColor = Color.Lerp(SwordFlame, SwordCore, Main.rand.NextFloat());
                var sparkle = new SparkleParticle(Projectile.Center, sparkleVel, sparkleColor, 0.4f, 20);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // 8 dust burst
            for (int i = 0; i < 8; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(5f, 5f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, dustVel, 0, default, 1.3f);
                d.noGravity = true;
            }
            
            SoundEngine.PlaySound(SoundID.Item60, Projectile.Center);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Texture2D flareTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare").Value;
            Texture2D flareTex2 = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare4").Value;
            
            Vector2 origin = texture.Size() / 2f;
            Vector2 flareOrigin = flareTex.Size() / 2f;
            Vector2 flareOrigin2 = flareTex2.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            float time = Main.GameUpdateCount * 0.06f;
            float pulse = 1f + (float)Math.Sin(time * 2f) * 0.12f;
            
            // === TRUE_VFX_STANDARDS: TRAIL WITH hslToRgb GRADIENT ===
            if (ProjectileID.Sets.TrailCacheLength[Type] > 0)
            {
                for (int i = 0; i < Projectile.oldPos.Length; i++)
                {
                    if (Projectile.oldPos[i] == Vector2.Zero) continue;
                    
                    float progress = (float)i / Projectile.oldPos.Length;
                    float trailHue = HueMin + progress * (HueMax - HueMin);
                    Color trailColor = Main.hslToRgb(trailHue, 0.85f, 0.65f) * (1f - progress) * 0.5f;
                    trailColor.A = 0;
                    
                    Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                    float trailScale = Projectile.scale * (1f - progress * 0.4f);
                    
                    Main.spriteBatch.Draw(texture, trailPos, null, trailColor, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0f);
                }
            }
            
            // === SPECTRAL GHOST LAYERS - White hot core fading to crimson to black ===
            
            // Layer 1: Outer black ghost
            for (int i = 0; i < 3; i++)
            {
                float offset = (float)Math.Sin(Main.GameUpdateCount * 0.08f + i * 0.7f) * 6f;
                Vector2 ghostPos = drawPos + new Vector2(offset, -offset * 0.7f);
                Color blackGhost = DiesIraeColors.CharredBlack * 0.25f;
                blackGhost.A = 0;
                Main.spriteBatch.Draw(texture, ghostPos, null, blackGhost, Projectile.rotation, origin, Projectile.scale * 1.2f, SpriteEffects.None, 0f);
            }
            
            // Layer 2: Blood red spectral glow
            for (int i = 0; i < 4; i++)
            {
                float offset = (float)Math.Sin(Main.GameUpdateCount * 0.1f + i * 0.5f) * 4f;
                Vector2 spectralPos = drawPos + new Vector2(offset, -offset);
                Color spectralColor = Color.Lerp(SwordBlood, DiesIraeColors.Crimson, i / 4f) * (0.35f - i * 0.05f);
                spectralColor.A = 0;
                Main.spriteBatch.Draw(texture, spectralPos, null, spectralColor, Projectile.rotation, origin, Projectile.scale * (1.1f - i * 0.05f), SpriteEffects.None, 0f);
            }
            
            // Layer 3: Orange ember accent
            Color emberAccent = SwordFlame * 0.4f;
            emberAccent.A = 0;
            Main.spriteBatch.Draw(texture, drawPos, null, emberAccent, Projectile.rotation, origin, Projectile.scale * pulse, SpriteEffects.None, 0f);
            
            // Layer 4: White-hot core
            Color whiteCore = Color.White * 0.55f;
            whiteCore.A = 0;
            Main.spriteBatch.Draw(texture, drawPos, null, whiteCore, Projectile.rotation, origin, Projectile.scale * 0.85f * pulse, SpriteEffects.None, 0f);
            
            // === 4 SPINNING FLARE LAYERS ===
            
            // Flare 1: Spinning clockwise
            Color flareColor1 = SwordFlame * 0.5f;
            flareColor1.A = 0;
            Main.spriteBatch.Draw(flareTex, drawPos, null, flareColor1, time, flareOrigin, 0.4f * pulse, SpriteEffects.None, 0f);
            
            // Flare 2: Counter-clockwise
            Color flareColor2 = SwordBlood * 0.45f;
            flareColor2.A = 0;
            Main.spriteBatch.Draw(flareTex2, drawPos, null, flareColor2, -time * 0.7f, flareOrigin2, 0.32f * pulse, SpriteEffects.None, 0f);
            
            // Flare 3: hslToRgb accent
            float hue = HueMin + ((float)Math.Sin(time) * 0.5f + 0.5f) * (HueMax - HueMin);
            Color hslColor = Main.hslToRgb(hue, 0.9f, 0.7f) * 0.45f;
            hslColor.A = 0;
            Main.spriteBatch.Draw(flareTex, drawPos, null, hslColor, time * 1.3f, flareOrigin, 0.25f * pulse, SpriteEffects.None, 0f);
            
            // Flare 4: White center
            Color whiteCoreFlare = Color.White * 0.5f;
            whiteCoreFlare.A = 0;
            Main.spriteBatch.Draw(flareTex2, drawPos, null, whiteCoreFlare, 0f, flareOrigin2, 0.12f, SpriteEffects.None, 0f);
            
            return false;
        }
    }
    
    #endregion
    
    #region Chain of Judgment Projectiles
    
    /// <summary>
    /// Spinning spectral blade that ricochets between enemies
    /// </summary>
    public class JudgmentChainProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/DiesIrae/ResonantWeapons/ChainOfJudgment";
        
        // TRUE_VFX_STANDARDS: Dies Irae blood red to orange hue range
        private const float HueMin = 0.0f;    // Blood red
        private const float HueMax = 0.08f;   // Orange-red
        
        // Color palette for this blade
        private static readonly Color ChainCore = new Color(255, 235, 200);     // White-gold core
        private static readonly Color ChainFlame = new Color(255, 150, 55);     // Gold-orange flame
        private static readonly Color ChainBlood = new Color(210, 50, 35);      // Blood red
        
        private int bounceCount = 0;
        private const int MaxBounces = 4;
        private List<int> hitNPCs = new List<int>();
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }
        
        public override void AI()
        {
            // Spinning
            Projectile.rotation += 0.3f;
            
            // === TRUE_VFX_STANDARDS: DENSE DUST TRAIL (2+ per frame guaranteed) ===
            for (int i = 0; i < 2; i++)
            {
                Vector2 dustOffset = Main.rand.NextVector2Circular(12f, 12f);
                Vector2 dustVel = -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(2f, 2f);
                Dust fire = Dust.NewDustPerfect(Projectile.Center + dustOffset, DustID.Torch, dustVel, 0, default, 1.6f);
                fire.noGravity = true;
                fire.fadeIn = 1.3f;
            }
            
            // === CONTRASTING SPARKLES (1-in-2) ===
            if (Main.rand.NextBool(2))
            {
                Dust spark = Dust.NewDustPerfect(Projectile.Center, DustID.GoldCoin, 
                    -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(2f, 2f), 0, default, 1.1f);
                spark.noGravity = true;
            }
            
            // === SPINNING FIRE TRAIL at blade tips ===
            float trailAngle = Projectile.rotation;
            for (int i = 0; i < 2; i++)
            {
                float angle = trailAngle + MathHelper.Pi * i;
                Vector2 trailPos = Projectile.Center + angle.ToRotationVector2() * 20f;
                
                Color trailColor = Color.Lerp(ChainFlame, ChainBlood, Main.rand.NextFloat());
                var trail = new GenericGlowParticle(trailPos, -Projectile.velocity * 0.12f, trailColor, 0.4f, 18, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }
            
            // Central fire trail
            DiesIraeVFX.FireTrail(Projectile.Center, Projectile.velocity, 1f);
            
            // === DYNAMIC PARTICLE EFFECTS - Spinning orbits and pulsing glow ===
            if (Main.GameUpdateCount % 4 == 0)
            {
                PulsingGlow(Projectile.Center, Vector2.Zero, DiesIraeColors.Crimson, DiesIraeColors.HellfireGold, 0.38f, 22, 0.16f, 0.26f);
            }
            if (Main.GameUpdateCount % 25 == 0)
            {
                OrbitingRing(Projectile.Center, DiesIraeColors.EmberOrange, 4, 22f, 0.05f, 0.28f, 36);
            }
            
            // === TRUE_VFX_STANDARDS: ORBITING MUSIC NOTES (4 notes locked to projectile, spinning with blade) ===
            float orbitAngle = Main.GameUpdateCount * 0.08f;
            float orbitRadius = 25f;
            
            if (Main.rand.NextBool(3))
            {
                for (int i = 0; i < 4; i++)
                {
                    float noteAngle = Projectile.rotation + MathHelper.TwoPi * i / 4f;
                    Vector2 noteOffset = noteAngle.ToRotationVector2() * orbitRadius;
                    Vector2 notePos = Projectile.Center + noteOffset;
                    Vector2 noteVel = noteAngle.ToRotationVector2() * 1.5f; // Spin outward
                    
                    float hue = HueMin + ((float)i / 4f * (HueMax - HueMin));
                    hue += (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.015f;
                    Color noteColor = Main.hslToRgb(hue, 0.95f, 0.72f);
                    
                    DiesIraeVFX.SpawnMusicNote(notePos, noteVel, noteColor, 0.85f);
                }
            }
            
            // === FLARES LITTERING THE AIR (1-in-2) ===
            if (Main.rand.NextBool(2))
            {
                Vector2 flareOffset = Main.rand.NextVector2Circular(15f, 15f);
                float hue = HueMin + (Main.rand.NextFloat() * (HueMax - HueMin));
                Color flareColor = Main.hslToRgb(hue, 0.9f, 0.75f);
                CustomParticles.GenericFlare(Projectile.Center + flareOffset, flareColor, 0.45f, 14);
            }
            
            // Return to player after max bounces
            if (bounceCount >= MaxBounces)
            {
                Player player = Main.player[Projectile.owner];
                Vector2 toPlayer = player.Center - Projectile.Center;
                toPlayer.Normalize();
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toPlayer * 18f, 0.1f);
                
                if (Vector2.Distance(Projectile.Center, player.Center) < 30f)
                {
                    Projectile.Kill();
                }
            }
            
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.15f;
            Lighting.AddLight(Projectile.Center, ChainFlame.ToVector3() * 0.8f * pulse);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 180);
            
            // === TRUE_VFX_STANDARDS: MULTI-LAYER FLASH CASCADE ===
            CustomParticles.GenericFlare(target.Center, Color.White, 1.1f, 22);
            CustomParticles.GenericFlare(target.Center, ChainCore, 0.85f, 20);
            CustomParticles.GenericFlare(target.Center, ChainFlame, 0.65f, 18);
            
            // 5 gradient music notes
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi * i / 5f;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(2.5f, 5f);
                float hue = HueMin + ((float)i / 5f * (HueMax - HueMin));
                Color noteColor = Main.hslToRgb(hue, 0.9f, 0.72f);
                DiesIraeVFX.SpawnMusicNote(target.Center, noteVel, noteColor, 0.85f);
            }
            
            CustomParticles.HaloRing(target.Center, ChainFlame, 0.5f, 16);
            CustomParticles.HaloRing(target.Center, ChainBlood, 0.35f, 14);
            
            // === DYNAMIC PARTICLE EFFECTS - Dies Wrath Chain Lightning (chain projectile) ===
            DiesWrathChainLightning(target.Center, Projectile.velocity.SafeNormalize(Vector2.UnitX), 1.3f);
            DramaticImpact(target.Center, DiesIraeColors.InfernalWhite, DiesIraeColors.BloodRed, 0.7f, 26);
            SpiralBurst(target.Center, DiesIraeColors.Crimson, DiesIraeColors.HellfireGold, 8, 0.15f, 5f, 0.4f, 28);
            
            SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with { Volume = 0.6f }, Projectile.Center);
            
            // Bounce to next target
            if (bounceCount < MaxBounces)
            {
                hitNPCs.Add(target.whoAmI);
                NPC nextTarget = FindNextTarget(target.Center, 500f);
                
                if (nextTarget != null)
                {
                    Vector2 direction = nextTarget.Center - Projectile.Center;
                    direction.Normalize();
                    Projectile.velocity = direction * Projectile.velocity.Length();
                    bounceCount++;
                }
                else
                {
                    bounceCount = MaxBounces; // No more targets, return
                }
            }
        }
        
        private NPC FindNextTarget(Vector2 position, float maxDistance)
        {
            NPC closest = null;
            float closestDist = maxDistance;
            
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (hitNPCs.Contains(i)) continue;
                
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.CanBeChasedBy())
                {
                    float dist = Vector2.Distance(position, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = npc;
                    }
                }
            }
            
            return closest;
        }
        
        public override void OnKill(int timeLeft)
        {
            // === TRUE_VFX_STANDARDS: GLIMMER CASCADE (not puff) ===
            CustomParticles.GenericFlare(Projectile.Center, Color.White, 1.0f, 22);
            CustomParticles.GenericFlare(Projectile.Center, ChainCore, 0.8f, 20);
            CustomParticles.GenericFlare(Projectile.Center, ChainFlame, 0.6f, 18);
            
            // 3 halo rings
            for (int i = 0; i < 3; i++)
            {
                float ringProgress = (float)i / 3f;
                Color ringColor = Color.Lerp(ChainFlame, ChainBlood, ringProgress);
                CustomParticles.HaloRing(Projectile.Center, ringColor, 0.45f + i * 0.12f, 16 + i * 3);
            }
            
            // 6 gradient music notes finale
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 5f);
                float hue = HueMin + ((float)i / 6f * (HueMax - HueMin));
                Color noteColor = Main.hslToRgb(hue, 0.9f, 0.72f);
                DiesIraeVFX.SpawnMusicNote(Projectile.Center, noteVel, noteColor, 0.9f);
            }
            
            // 8 sparkle burst
            for (int i = 0; i < 8; i++)
            {
                Vector2 sparkleVel = Main.rand.NextVector2Circular(6f, 6f);
                Color sparkleColor = Color.Lerp(ChainFlame, ChainCore, Main.rand.NextFloat());
                var sparkle = new SparkleParticle(Projectile.Center, sparkleVel, sparkleColor, 0.45f, 22);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // 10 dust burst
            for (int i = 0; i < 10; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(6f, 6f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, dustVel, 0, default, 1.4f);
                d.noGravity = true;
            }
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Texture2D flareTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare").Value;
            Texture2D flareTex2 = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare4").Value;
            Texture2D glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow2").Value;
            
            Vector2 origin = texture.Size() / 2f;
            Vector2 flareOrigin = flareTex.Size() / 2f;
            Vector2 flareOrigin2 = flareTex2.Size() / 2f;
            Vector2 glowOrigin = glowTex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            float time = Main.GameUpdateCount * 0.06f;
            float pulse = 1f + (float)Math.Sin(time * 2f) * 0.1f;
            
            // === TRUE_VFX_STANDARDS: TRAIL WITH hslToRgb GRADIENT ===
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                
                float progress = i / (float)Projectile.oldPos.Length;
                float hue = HueMin + progress * (HueMax - HueMin);
                Color trailColor = Main.hslToRgb(hue, 0.85f, 0.65f) * (1f - progress) * 0.6f;
                trailColor.A = 0;
                
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                float trailScale = Projectile.scale * (1f - progress * 0.35f);
                
                Main.spriteBatch.Draw(texture, trailPos, null, trailColor, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0f);
            }
            
            // === MAIN PROJECTILE BLOOM ===
            // Layer 1: Black outer
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = (MathHelper.TwoPi * i / 4f + Main.GameUpdateCount * 0.025f).ToRotationVector2() * 4f;
                Color blackGlow = DiesIraeColors.CharredBlack * 0.3f;
                blackGlow.A = 0;
                Main.spriteBatch.Draw(texture, drawPos + offset, null, blackGlow, Projectile.rotation, origin, Projectile.scale * 1.25f, SpriteEffects.None, 0f);
            }
            
            // Layer 2: Blood red
            for (int i = 0; i < 4; i++)
            {
                float scale = Projectile.scale * (1.1f + i * 0.08f);
                Color glowColor = ChainBlood * (0.4f / (i + 1));
                glowColor.A = 0;
                Main.spriteBatch.Draw(texture, drawPos, null, glowColor, Projectile.rotation, origin, scale * pulse, SpriteEffects.None, 0f);
            }
            
            // Layer 3: Ember orange
            Color emberGlow = ChainFlame * 0.45f;
            emberGlow.A = 0;
            Main.spriteBatch.Draw(texture, drawPos, null, emberGlow, Projectile.rotation, origin, Projectile.scale * pulse, SpriteEffects.None, 0f);
            
            // Layer 4: White-hot core
            Color whiteCore = Color.White * 0.6f;
            whiteCore.A = 0;
            Main.spriteBatch.Draw(texture, drawPos, null, whiteCore, Projectile.rotation, origin, Projectile.scale * 0.7f * pulse, SpriteEffects.None, 0f);
            
            // === 5 SPINNING FLARE LAYERS ===
            
            // Flare 1: Soft glow base
            Color glowColor1 = ChainFlame * 0.35f;
            glowColor1.A = 0;
            Main.spriteBatch.Draw(glowTex, drawPos, null, glowColor1, 0f, glowOrigin, 0.55f * pulse, SpriteEffects.None, 0f);
            
            // Flare 2: Spinning clockwise
            Color flareColor1 = ChainFlame * 0.55f;
            flareColor1.A = 0;
            Main.spriteBatch.Draw(flareTex, drawPos, null, flareColor1, time, flareOrigin, 0.4f * pulse, SpriteEffects.None, 0f);
            
            // Flare 3: Counter-clockwise
            Color flareColor2 = ChainBlood * 0.5f;
            flareColor2.A = 0;
            Main.spriteBatch.Draw(flareTex2, drawPos, null, flareColor2, -time * 0.75f, flareOrigin2, 0.32f * pulse, SpriteEffects.None, 0f);
            
            // Flare 4: hslToRgb accent
            float hslHue = HueMin + ((float)Math.Sin(time) * 0.5f + 0.5f) * (HueMax - HueMin);
            Color hslColor = Main.hslToRgb(hslHue, 0.9f, 0.7f) * 0.45f;
            hslColor.A = 0;
            Main.spriteBatch.Draw(flareTex, drawPos, null, hslColor, time * 1.3f, flareOrigin, 0.28f * pulse, SpriteEffects.None, 0f);
            
            // Flare 5: White center
            Color whiteCoreFlare = Color.White * 0.55f;
            whiteCoreFlare.A = 0;
            Main.spriteBatch.Draw(flareTex2, drawPos, null, whiteCoreFlare, 0f, flareOrigin2, 0.12f, SpriteEffects.None, 0f);
            
            // === 4 ORBITING SPARK POINTS ===
            float orbitAngle = Projectile.rotation; // Sync with blade spin
            for (int i = 0; i < 4; i++)
            {
                float sparkAngle = orbitAngle + MathHelper.TwoPi * i / 4f;
                Vector2 sparkPos = drawPos + sparkAngle.ToRotationVector2() * 20f;
                Color sparkColor = Color.Lerp(ChainFlame, Color.White, 0.4f) * 0.6f;
                sparkColor.A = 0;
                Main.spriteBatch.Draw(flareTex, sparkPos, null, sparkColor, sparkAngle, flareOrigin, 0.1f * pulse, SpriteEffects.None, 0f);
            }
            
            return false;
        }
    }
    
    #endregion
    
    #region Damnation's Cannon Projectiles
    
    /// <summary>
    /// Main projectile - ball of ignited wrath
    /// TRUE_VFX_STANDARDS: Dense dust, orbiting music notes, spinning flares, hslToRgb color oscillation
    /// </summary>
    public class IgnitedWrathBall : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/TallFlamingWispProjectile";
        
        // TRUE_VFX_STANDARDS: Dies Irae blood red to orange hue range
        private const float HueMin = 0.0f;    // Blood red
        private const float HueMax = 0.08f;   // Orange-red
        
        // Color palette for this fiery ball
        private static readonly Color BallCore = new Color(255, 230, 190);     // White-gold core
        private static readonly Color BallFlame = new Color(255, 150, 50);     // Gold-orange flame
        private static readonly Color BallBlood = new Color(210, 50, 35);      // Blood red
        
        public override void SetStaticDefaults()
        {
            // TRUE_VFX_STANDARDS: Extended trail cache for smoother rendering
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
        }
        
        public override void AI()
        {
            Projectile.rotation += 0.15f;
            
            // === TRUE_VFX_STANDARDS: DENSE DUST TRAIL (3+ per frame for ball) ===
            for (int i = 0; i < 3; i++)
            {
                Vector2 dustOffset = Main.rand.NextVector2Circular(12f, 12f);
                Vector2 dustVel = -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(2f, 2f);
                Dust fire = Dust.NewDustPerfect(Projectile.Center + dustOffset, DustID.Torch, dustVel, 0, default, 1.8f);
                fire.noGravity = true;
                fire.fadeIn = 1.4f;
            }
            
            // === CONTRASTING SPARKLES (1-in-2) ===
            if (Main.rand.NextBool(2))
            {
                Dust spark = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f), DustID.GoldCoin, 
                    -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1.5f, 1.5f), 0, default, 1.2f);
                spark.noGravity = true;
            }
            
            // === ELECTRIC SPARKS (1-in-3) ===
            if (Main.rand.NextBool(3))
            {
                Dust electric = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(10f, 10f), DustID.Electric, 
                    Main.rand.NextVector2Circular(2f, 2f), 0, default, 0.8f);
                electric.noGravity = true;
            }
            
            // === TRUE_VFX_STANDARDS: 4 ORBITING MUSIC NOTES LOCKED TO PROJECTILE ===
            float orbitAngle = Main.GameUpdateCount * 0.1f;
            float orbitRadius = 18f + (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 4f;
            
            if (Main.rand.NextBool(3))
            {
                for (int i = 0; i < 4; i++)
                {
                    float noteAngle = orbitAngle + MathHelper.TwoPi * i / 4f;
                    Vector2 noteOffset = noteAngle.ToRotationVector2() * orbitRadius;
                    Vector2 notePos = Projectile.Center + noteOffset;
                    Vector2 noteVel = Projectile.velocity * 0.75f + noteAngle.ToRotationVector2() * 0.5f;
                    
                    // hslToRgb color oscillation within theme range
                    float hue = HueMin + (((float)i / 4f + Main.GameUpdateCount * 0.01f) % 1f) * (HueMax - HueMin);
                    Color noteColor = Main.hslToRgb(hue, 0.95f, 0.72f);
                    
                    DiesIraeVFX.SpawnMusicNote(notePos, noteVel, noteColor, 0.85f);
                }
            }
            
            // === FLARES LITTERING THE AIR (1-in-2) ===
            if (Main.rand.NextBool(2))
            {
                Vector2 flareOffset = Main.rand.NextVector2Circular(10f, 10f);
                float hue = HueMin + (Main.rand.NextFloat() * (HueMax - HueMin));
                Color flareColor = Main.hslToRgb(hue, 0.9f, 0.75f);
                CustomParticles.GenericFlare(Projectile.Center + flareOffset, flareColor, 0.45f, 14);
            }
            
            // === LAYER 1: Heavy fire trail (core effect) ===
            for (int i = 0; i < 2; i++)
            {
                DiesIraeVFX.FireTrail(Projectile.Center + Main.rand.NextVector2Circular(10f, 10f), Projectile.velocity, 1.2f);
            }
            
            // === LAYER 2: Afterimage trail for motion blur ===
            if (Projectile.velocity.Length() > 2f)
            {
                DiesIraeVFX.AfterimageTrail(Projectile.Center, Projectile.velocity, 0.5f, DiesIraeColors.BloodRed, 4);
            }
            
            // === LAYER 3: Pulsing ember aura ===
            DiesIraeVFX.PulsingAura(Projectile.Center, DiesIraeColors.EmberOrange, 0.7f, Projectile.timeLeft);
            
            // === LAYER 4: Orbiting ember sparks ===
            if (Main.GameUpdateCount % 4 == 0)
            {
                DiesIraeVFX.OrbitingSparks(Projectile.Center, DiesIraeColors.EmberOrange, 16f, 3, Main.GameUpdateCount * 0.08f, 0.4f);
            }
            
            // === LAYER 5: Spiral fire pattern ===
            if (Main.rand.NextBool(3))
            {
                DiesIraeVFX.SpiralTrail(Projectile.Center, Projectile.velocity, DiesIraeColors.Crimson, 0.5f, Main.GameUpdateCount * 0.15f);
            }
            
            // Dynamic lighting with pulse
            float lightPulse = 1.0f + (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 0.15f;
            Lighting.AddLight(Projectile.Center, BallFlame.ToVector3() * lightPulse);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 300);
            
            // === TRUE_VFX_STANDARDS: MULTI-LAYER FLASH CASCADE ===
            CustomParticles.GenericFlare(target.Center, Color.White, 1.0f, 20);
            CustomParticles.GenericFlare(target.Center, BallCore, 0.8f, 18);
            CustomParticles.GenericFlare(target.Center, BallFlame, 0.65f, 16);
            
            // Gradient music notes burst
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi * i / 5f;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                float hue = HueMin + ((float)i / 5f * (HueMax - HueMin));
                Color noteColor = Main.hslToRgb(hue, 0.95f, 0.75f);
                DiesIraeVFX.SpawnMusicNote(target.Center, noteVel, noteColor, 0.8f);
            }
            
            // Halo
            CustomParticles.HaloRing(target.Center, BallFlame, 0.5f, 16);
            
            // === DYNAMIC PARTICLE EFFECTS - Dies Judgment Vortex (exploding orb) ===
            DiesJudgmentVortex(target.Center, 1.1f);
            
            Explode();
        }
        
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Explode();
            return true;
        }
        
        private void Explode()
        {
            // === TRUE_VFX_STANDARDS: GLIMMER CASCADE (not puff) ===
            // Layer 1: White core flash
            CustomParticles.GenericFlare(Projectile.Center, Color.White, 1.4f, 25);
            // Layer 2: Core flame
            CustomParticles.GenericFlare(Projectile.Center, BallCore, 1.1f, 22);
            // Layer 3: Flame
            CustomParticles.GenericFlare(Projectile.Center, BallFlame, 0.9f, 20);
            // Layer 4: Blood
            CustomParticles.GenericFlare(Projectile.Center, BallBlood, 0.7f, 18);
            
            // 4 gradient halo rings
            for (int i = 0; i < 4; i++)
            {
                Color ringColor = Color.Lerp(BallFlame, BallBlood, i / 4f);
                CustomParticles.HaloRing(Projectile.Center, ringColor, 0.4f + i * 0.15f, 14 + i * 3);
            }
            
            // 8 gradient music notes finale
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                float hue = HueMin + ((float)i / 8f * (HueMax - HueMin));
                Color noteColor = Main.hslToRgb(hue, 0.95f, 0.75f);
                DiesIraeVFX.SpawnMusicNote(Projectile.Center, noteVel, noteColor, 0.9f);
            }
            
            DiesIraeVFX.FireImpact(Projectile.Center, 1.5f);
            SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode, Projectile.Center);
            
            // Spawn 5 orbiting shrapnel
            if (Main.myPlayer == Projectile.owner)
            {
                for (int i = 0; i < 5; i++)
                {
                    float angle = MathHelper.TwoPi * i / 5f;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), 
                        Main.player[Projectile.owner].Center, Vector2.Zero,
                        ModContent.ProjectileType<OrbitingShrapnel>(), Projectile.damage / 2, 0f, Projectile.owner, angle, i * 0.5f);
                }
            }
        }
        
        public override void OnKill(int timeLeft)
        {
            // === TRUE_VFX_STANDARDS: ENHANCED GLIMMER CASCADE ===
            // 3-layer glimmer
            CustomParticles.GenericFlare(Projectile.Center, Color.White, 1.2f, 22);
            CustomParticles.GenericFlare(Projectile.Center, BallCore, 0.9f, 20);
            CustomParticles.GenericFlare(Projectile.Center, BallFlame, 0.7f, 18);
            
            // 3 halo rings
            for (int i = 0; i < 3; i++)
            {
                Color ringColor = Color.Lerp(BallFlame, BallBlood, i / 3f);
                CustomParticles.HaloRing(Projectile.Center, ringColor, 0.35f + i * 0.12f, 14 + i * 2);
            }
            
            // 6 gradient music notes
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(2.5f, 5f);
                float hue = HueMin + ((float)i / 6f * (HueMax - HueMin));
                Color noteColor = Main.hslToRgb(hue, 0.95f, 0.72f);
                DiesIraeVFX.SpawnMusicNote(Projectile.Center, noteVel, noteColor, 0.85f);
            }
            
            // 10 sparkle burst
            for (int i = 0; i < 10; i++)
            {
                Vector2 sparkleVel = Main.rand.NextVector2Circular(6f, 6f);
                Color sparkleColor = Color.Lerp(BallFlame, BallCore, Main.rand.NextFloat());
                var sparkle = new SparkleParticle(Projectile.Center, sparkleVel, sparkleColor, 0.45f, 22);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // 15 dust burst
            for (int i = 0; i < 15; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(8f, 8f);
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, vel, 0, default, 2f);
                dust.noGravity = true;
            }
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Texture2D flareTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare").Value;
            Texture2D flareTex2 = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare4").Value;
            Texture2D glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow2").Value;
            
            Vector2 origin = texture.Size() / 2f;
            Vector2 flareOrigin = flareTex.Size() / 2f;
            Vector2 flareOrigin2 = flareTex2.Size() / 2f;
            Vector2 glowOrigin = glowTex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            float time = Main.GameUpdateCount * 0.06f;
            float pulse = 1f + (float)Math.Sin(time * 2f) * 0.12f;
            
            // === TRUE_VFX_STANDARDS: TRAIL WITH hslToRgb GRADIENT ===
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                
                float progress = i / (float)Projectile.oldPos.Length;
                float hue = HueMin + progress * (HueMax - HueMin);
                Color trailColor = Main.hslToRgb(hue, 0.85f, 0.65f) * (1f - progress) * 0.65f;
                trailColor.A = 0;
                
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                float trailScale = Projectile.scale * (1f - progress * 0.4f);
                
                Main.spriteBatch.Draw(texture, trailPos, null, trailColor, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0f);
            }
            
            // === MAIN BALL BLOOM ===
            // Layer 1: Black smoke outer
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = (MathHelper.TwoPi * i / 4f + Main.GameUpdateCount * 0.02f).ToRotationVector2() * 5f;
                Color blackGlow = DiesIraeColors.CharredBlack * 0.3f;
                blackGlow.A = 0;
                Main.spriteBatch.Draw(texture, drawPos + offset, null, blackGlow, Projectile.rotation, origin, Projectile.scale * 1.35f, SpriteEffects.None, 0f);
            }
            
            // Layer 2: Blood red glow
            for (int i = 0; i < 4; i++)
            {
                float scale = Projectile.scale * (1.2f + i * 0.08f);
                Color glowColor = BallBlood * (0.4f / (i + 1));
                glowColor.A = 0;
                Main.spriteBatch.Draw(texture, drawPos, null, glowColor, Projectile.rotation, origin, scale * pulse, SpriteEffects.None, 0f);
            }
            
            // Layer 3: Ember orange
            Color emberGlow = BallFlame * 0.5f;
            emberGlow.A = 0;
            Main.spriteBatch.Draw(texture, drawPos, null, emberGlow, Projectile.rotation, origin, Projectile.scale * pulse, SpriteEffects.None, 0f);
            
            // Layer 4: White-hot core
            Color whiteCore = Color.White * 0.7f;
            whiteCore.A = 0;
            Main.spriteBatch.Draw(texture, drawPos, null, whiteCore, Projectile.rotation, origin, Projectile.scale * 0.7f * pulse, SpriteEffects.None, 0f);
            
            // === 6 SPINNING FLARE LAYERS ===
            
            // Flare 1: Soft glow base (large)
            Color glowColor1 = BallFlame * 0.4f;
            glowColor1.A = 0;
            Main.spriteBatch.Draw(glowTex, drawPos, null, glowColor1, 0f, glowOrigin, 0.7f * pulse, SpriteEffects.None, 0f);
            
            // Flare 2: Spinning clockwise
            Color flareColor1 = BallFlame * 0.6f;
            flareColor1.A = 0;
            Main.spriteBatch.Draw(flareTex, drawPos, null, flareColor1, time, flareOrigin, 0.5f * pulse, SpriteEffects.None, 0f);
            
            // Flare 3: Counter-clockwise
            Color flareColor2 = BallBlood * 0.55f;
            flareColor2.A = 0;
            Main.spriteBatch.Draw(flareTex2, drawPos, null, flareColor2, -time * 0.75f, flareOrigin2, 0.4f * pulse, SpriteEffects.None, 0f);
            
            // Flare 4: Different speed
            Color flareColor3 = BallFlame * 0.5f;
            flareColor3.A = 0;
            Main.spriteBatch.Draw(flareTex, drawPos, null, flareColor3, time * 1.4f, flareOrigin, 0.35f * pulse, SpriteEffects.None, 0f);
            
            // Flare 5: hslToRgb accent
            float hslHue = HueMin + ((float)Math.Sin(time) * 0.5f + 0.5f) * (HueMax - HueMin);
            Color hslColor = Main.hslToRgb(hslHue, 0.92f, 0.72f) * 0.5f;
            hslColor.A = 0;
            Main.spriteBatch.Draw(flareTex2, drawPos, null, hslColor, time * 1.8f, flareOrigin2, 0.28f * pulse, SpriteEffects.None, 0f);
            
            // Flare 6: White center
            Color whiteCoreFlare = Color.White * 0.65f;
            whiteCoreFlare.A = 0;
            Main.spriteBatch.Draw(flareTex, drawPos, null, whiteCoreFlare, 0f, flareOrigin, 0.15f, SpriteEffects.None, 0f);
            
            // === 4 ORBITING SPARK POINTS ===
            float orbitAngle = Main.GameUpdateCount * 0.1f;
            for (int i = 0; i < 4; i++)
            {
                float sparkAngle = orbitAngle + MathHelper.TwoPi * i / 4f;
                Vector2 sparkPos = drawPos + sparkAngle.ToRotationVector2() * 22f;
                Color sparkColor = Color.Lerp(BallFlame, Color.White, 0.4f) * 0.65f;
                sparkColor.A = 0;
                Main.spriteBatch.Draw(flareTex, sparkPos, null, sparkColor, sparkAngle, flareOrigin, 0.12f * pulse, SpriteEffects.None, 0f);
            }
            
            return false;
        }
    }
    
    /// <summary>
    /// Orbiting shrapnel that seeks enemies
    /// </summary>
    /// <summary>
    /// Orbiting shrapnel that seeks enemies
    /// TRUE_VFX_STANDARDS: Dense dust, orbiting music notes, spinning flares, hslToRgb color oscillation
    /// </summary>
    public class OrbitingShrapnel : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/FlamingWispProjectileSmall";
        
        // TRUE_VFX_STANDARDS: Dies Irae blood red to orange hue range
        private const float HueMin = 0.0f;    // Blood red
        private const float HueMax = 0.08f;   // Orange-red
        
        // Color palette for this shrapnel
        private static readonly Color ShrapnelCore = new Color(255, 225, 180);    // White-gold core
        private static readonly Color ShrapnelFlame = new Color(255, 145, 55);    // Gold-orange flame
        private static readonly Color ShrapnelBlood = new Color(205, 45, 35);     // Blood red
        
        private float orbitAngle;
        private float orbitRadius = 80f;
        private int orbitTimer = 0;
        private const int OrbitDuration = 90;
        private bool seeking = false;
        private int targetNPC = -1;
        
        public override void SetStaticDefaults()
        {
            // TRUE_VFX_STANDARDS: Trail cache for smooth rendering
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }
        
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            orbitAngle = Projectile.ai[0];
            
            orbitTimer++;
            
            if (!seeking && orbitTimer < OrbitDuration)
            {
                // Orbit around player
                orbitAngle += 0.08f;
                Projectile.ai[0] = orbitAngle;
                
                Vector2 targetPos = owner.Center + orbitAngle.ToRotationVector2() * orbitRadius;
                Projectile.Center = Vector2.Lerp(Projectile.Center, targetPos, 0.2f);
                Projectile.velocity = Vector2.Zero;
                
                // Find target during orbit
                if (orbitTimer > 30)
                {
                    targetNPC = FindClosestNPCIndex(600f);
                    if (targetNPC >= 0)
                    {
                        seeking = true;
                    }
                }
            }
            else
            {
                // Seek and destroy
                seeking = true;
                
                if (targetNPC >= 0 && targetNPC < Main.maxNPCs && Main.npc[targetNPC].active)
                {
                    NPC target = Main.npc[targetNPC];
                    Vector2 direction = target.Center - Projectile.Center;
                    direction.Normalize();
                    
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * 16f, 0.1f);
                }
                else
                {
                    // Find new target or fade
                    targetNPC = FindClosestNPCIndex(600f);
                    if (targetNPC < 0)
                    {
                        Projectile.velocity *= 0.95f;
                        if (Projectile.velocity.Length() < 1f)
                            Projectile.Kill();
                    }
                }
            }
            
            Projectile.rotation = Projectile.velocity != Vector2.Zero ? Projectile.velocity.ToRotation() : orbitAngle;
            
            // === TRUE_VFX_STANDARDS: DENSE DUST TRAIL (2+ per frame guaranteed) ===
            for (int i = 0; i < 2; i++)
            {
                Vector2 dustOffset = Main.rand.NextVector2Circular(5f, 5f);
                Vector2 dustVel = seeking ? -Projectile.velocity * 0.12f : -orbitAngle.ToRotationVector2() * 1.5f;
                dustVel += Main.rand.NextVector2Circular(1f, 1f);
                Dust fire = Dust.NewDustPerfect(Projectile.Center + dustOffset, DustID.Torch, dustVel, 0, default, 1.3f);
                fire.noGravity = true;
                fire.fadeIn = 1.1f;
            }
            
            // === CONTRASTING SPARKLES (1-in-2) ===
            if (Main.rand.NextBool(2))
            {
                Dust spark = Dust.NewDustPerfect(Projectile.Center, DustID.GoldCoin, 
                    Main.rand.NextVector2Circular(1.5f, 1.5f), 0, default, 0.85f);
                spark.noGravity = true;
            }
            
            // === TRUE_VFX_STANDARDS: 3 ORBITING MUSIC NOTES LOCKED TO PROJECTILE ===
            float noteOrbitAngle = Main.GameUpdateCount * 0.1f;
            float noteOrbitRadius = 10f;
            
            if (Main.rand.NextBool(4))
            {
                for (int i = 0; i < 3; i++)
                {
                    float noteAngle = noteOrbitAngle + MathHelper.TwoPi * i / 3f;
                    Vector2 noteOffset = noteAngle.ToRotationVector2() * noteOrbitRadius;
                    Vector2 notePos = Projectile.Center + noteOffset;
                    Vector2 noteVel = seeking ? Projectile.velocity * 0.6f : orbitAngle.ToRotationVector2() * 2f;
                    noteVel += noteAngle.ToRotationVector2() * 0.4f;
                    
                    // hslToRgb color oscillation within theme range
                    float hue = HueMin + (((float)i / 3f + Main.GameUpdateCount * 0.01f) % 1f) * (HueMax - HueMin);
                    Color noteColor = Main.hslToRgb(hue, 0.95f, 0.72f);
                    
                    DiesIraeVFX.SpawnMusicNote(notePos, noteVel, noteColor, 0.75f);
                }
            }
            
            // === FLARES LITTERING THE AIR (1-in-3) ===
            if (Main.rand.NextBool(3))
            {
                Vector2 flareOffset = Main.rand.NextVector2Circular(6f, 6f);
                float hue = HueMin + (Main.rand.NextFloat() * (HueMax - HueMin));
                Color flareColor = Main.hslToRgb(hue, 0.9f, 0.72f);
                CustomParticles.GenericFlare(Projectile.Center + flareOffset, flareColor, 0.35f, 12);
            }
            
            // === LAYER 1: Core fire trail ===
            DiesIraeVFX.FireTrail(Projectile.Center, seeking ? Projectile.velocity.SafeNormalize(Vector2.Zero) * 5f : orbitAngle.ToRotationVector2() * 3f, 0.6f);
            
            // === LAYER 2: Afterimage trail when moving fast ===
            if (seeking && Projectile.velocity.Length() > 4f)
            {
                DiesIraeVFX.AfterimageTrail(Projectile.Center, Projectile.velocity, 0.35f, DiesIraeColors.Crimson, 3);
            }
            
            // === LAYER 3: Spiral embers while orbiting ===
            if (!seeking)
            {
                DiesIraeVFX.SpiralTrail(Projectile.Center, orbitAngle.ToRotationVector2() * 3f, DiesIraeColors.EmberOrange, 0.35f, orbitAngle);
            }
            
            // === LAYER 4: Small orbiting sparks ===
            if (Main.GameUpdateCount % 6 == 0)
            {
                DiesIraeVFX.OrbitingSparks(Projectile.Center, DiesIraeColors.EmberOrange, 10f, 2, Main.GameUpdateCount * 0.12f, 0.2f);
            }
            
            // Dynamic lighting
            float lightIntensity = seeking ? 0.6f : 0.4f;
            Lighting.AddLight(Projectile.Center, ShrapnelFlame.ToVector3() * lightIntensity);
        }
        
        private int FindClosestNPCIndex(float maxDistance)
        {
            int closest = -1;
            float closestDist = maxDistance;
            
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.CanBeChasedBy())
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = i;
                    }
                }
            }
            
            return closest;
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 180);
            
            // === TRUE_VFX_STANDARDS: MULTI-LAYER FLASH CASCADE ===
            CustomParticles.GenericFlare(target.Center, Color.White, 0.75f, 16);
            CustomParticles.GenericFlare(target.Center, ShrapnelCore, 0.6f, 14);
            CustomParticles.GenericFlare(target.Center, ShrapnelFlame, 0.45f, 12);
            
            // 4 gradient music notes burst
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 3.5f);
                float hue = HueMin + ((float)i / 4f * (HueMax - HueMin));
                Color noteColor = Main.hslToRgb(hue, 0.95f, 0.72f);
                DiesIraeVFX.SpawnMusicNote(target.Center, noteVel, noteColor, 0.75f);
            }
            
            // Halo
            CustomParticles.HaloRing(target.Center, ShrapnelFlame, 0.35f, 12);
            
            // === DYNAMIC PARTICLE EFFECTS - Dies Wrath Chain Lightning (shrapnel impact) ===
            DiesWrathChainLightning(target.Center, Projectile.velocity.SafeNormalize(Vector2.UnitX), 0.7f);
        }
        
        public override void OnKill(int timeLeft)
        {
            // === TRUE_VFX_STANDARDS: GLIMMER CASCADE (not puff) ===
            // 3-layer glimmer
            CustomParticles.GenericFlare(Projectile.Center, Color.White, 0.85f, 18);
            CustomParticles.GenericFlare(Projectile.Center, ShrapnelCore, 0.65f, 16);
            CustomParticles.GenericFlare(Projectile.Center, ShrapnelFlame, 0.5f, 14);
            
            // 2 halo rings
            CustomParticles.HaloRing(Projectile.Center, ShrapnelFlame, 0.3f, 12);
            CustomParticles.HaloRing(Projectile.Center, ShrapnelBlood, 0.4f, 14);
            
            // 4 gradient music notes
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                float hue = HueMin + ((float)i / 4f * (HueMax - HueMin));
                Color noteColor = Main.hslToRgb(hue, 0.95f, 0.72f);
                DiesIraeVFX.SpawnMusicNote(Projectile.Center, noteVel, noteColor, 0.8f);
            }
            
            // 6 sparkle burst
            for (int i = 0; i < 6; i++)
            {
                Vector2 sparkleVel = Main.rand.NextVector2Circular(4f, 4f);
                Color sparkleColor = Color.Lerp(ShrapnelFlame, ShrapnelCore, Main.rand.NextFloat());
                var sparkle = new SparkleParticle(Projectile.Center, sparkleVel, sparkleColor, 0.35f, 18);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // 8 dust burst
            for (int i = 0; i < 8; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(5f, 5f);
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, dustVel, 0, default, 1.3f);
                dust.noGravity = true;
            }
            
            DiesIraeVFX.FireImpact(Projectile.Center, 0.7f);
            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact with { Volume = 0.5f }, Projectile.Center);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Texture2D flareTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare").Value;
            Texture2D flareTex2 = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare4").Value;
            Texture2D glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow2").Value;
            
            Vector2 origin = texture.Size() / 2f;
            Vector2 flareOrigin = flareTex.Size() / 2f;
            Vector2 flareOrigin2 = flareTex2.Size() / 2f;
            Vector2 glowOrigin = glowTex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            float time = Main.GameUpdateCount * 0.07f;
            float pulse = 1f + (float)Math.Sin(time * 2f + Projectile.ai[1]) * 0.15f;
            
            // === TRUE_VFX_STANDARDS: TRAIL WITH hslToRgb GRADIENT ===
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                
                float progress = i / (float)Projectile.oldPos.Length;
                float hue = HueMin + progress * (HueMax - HueMin);
                Color trailColor = Main.hslToRgb(hue, 0.85f, 0.65f) * (1f - progress) * 0.55f;
                trailColor.A = 0;
                
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                float trailScale = Projectile.scale * (1f - progress * 0.35f);
                
                Main.spriteBatch.Draw(texture, trailPos, null, trailColor, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0f);
            }
            
            // === MAIN SHRAPNEL BLOOM ===
            // Layer 1: Black smoke outer
            for (int i = 0; i < 3; i++)
            {
                Vector2 offset = (MathHelper.TwoPi * i / 3f + Main.GameUpdateCount * 0.03f).ToRotationVector2() * 3f;
                Color blackGlow = DiesIraeColors.CharredBlack * 0.25f;
                blackGlow.A = 0;
                Main.spriteBatch.Draw(texture, drawPos + offset, null, blackGlow, Projectile.rotation, origin, 0.95f * pulse, SpriteEffects.None, 0f);
            }
            
            // Layer 2: Dark red glow
            for (int i = 0; i < 3; i++)
            {
                float scale = pulse * (0.75f + i * 0.1f);
                Color redGlow = ShrapnelBlood * (0.4f / (i + 1));
                redGlow.A = 0;
                Main.spriteBatch.Draw(texture, drawPos, null, redGlow, Projectile.rotation, origin, scale, SpriteEffects.None, 0f);
            }
            
            // Layer 3: Orange accent
            Color emberGlow = ShrapnelFlame * 0.4f;
            emberGlow.A = 0;
            Main.spriteBatch.Draw(texture, drawPos, null, emberGlow, Projectile.rotation, origin, 0.65f * pulse, SpriteEffects.None, 0f);
            
            // Layer 4: White-hot core
            Color whiteCore = Color.White * 0.6f;
            whiteCore.A = 0;
            Main.spriteBatch.Draw(texture, drawPos, null, whiteCore, Projectile.rotation, origin, 0.45f * pulse, SpriteEffects.None, 0f);
            
            // === 4 SPINNING FLARE LAYERS ===
            
            // Flare 1: Soft glow base
            Color glowColor1 = ShrapnelFlame * 0.35f;
            glowColor1.A = 0;
            Main.spriteBatch.Draw(glowTex, drawPos, null, glowColor1, 0f, glowOrigin, 0.4f * pulse, SpriteEffects.None, 0f);
            
            // Flare 2: Spinning clockwise
            Color flareColor1 = ShrapnelFlame * 0.5f;
            flareColor1.A = 0;
            Main.spriteBatch.Draw(flareTex, drawPos, null, flareColor1, time, flareOrigin, 0.3f * pulse, SpriteEffects.None, 0f);
            
            // Flare 3: Counter-clockwise
            Color flareColor2 = ShrapnelBlood * 0.45f;
            flareColor2.A = 0;
            Main.spriteBatch.Draw(flareTex2, drawPos, null, flareColor2, -time * 0.8f, flareOrigin2, 0.25f * pulse, SpriteEffects.None, 0f);
            
            // Flare 4: hslToRgb accent
            float hslHue = HueMin + ((float)Math.Sin(time) * 0.5f + 0.5f) * (HueMax - HueMin);
            Color hslColor = Main.hslToRgb(hslHue, 0.9f, 0.7f) * 0.45f;
            hslColor.A = 0;
            Main.spriteBatch.Draw(flareTex, drawPos, null, hslColor, time * 1.3f, flareOrigin, 0.18f * pulse, SpriteEffects.None, 0f);
            
            // === 3 ORBITING SPARK POINTS ===
            float orbitSparkAngle = Main.GameUpdateCount * 0.1f;
            for (int i = 0; i < 3; i++)
            {
                float sparkAngle = orbitSparkAngle + MathHelper.TwoPi * i / 3f;
                Vector2 sparkPos = drawPos + sparkAngle.ToRotationVector2() * 12f;
                Color sparkColor = Color.Lerp(ShrapnelFlame, Color.White, 0.35f) * 0.55f;
                sparkColor.A = 0;
                Main.spriteBatch.Draw(flareTex, sparkPos, null, sparkColor, sparkAngle, flareOrigin, 0.08f * pulse, SpriteEffects.None, 0f);
            }
            
            return false;
        }
    }
    
    #endregion
    
    #region Arbiter's Sentence Projectiles
    
    /// <summary>
    /// Flamethrower stream projectile - TRUE_VFX_STANDARDS compliant
    /// </summary>
    public class JudgmentFlame : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/TallFlamingWispProjectile";
        
        // TRUE_VFX_STANDARDS: Dies Irae blood red to orange hue range
        private const float HueMin = 0.0f;    // Blood red
        private const float HueMax = 0.08f;   // Orange-red
        
        // Color palette for this flame
        private static readonly Color FlameCore = new Color(255, 235, 200);      // White-gold core
        private static readonly Color FlameEmber = new Color(255, 150, 60);      // Gold-orange ember
        private static readonly Color FlameBlood = new Color(210, 50, 35);       // Blood red
        
        public override void SetStaticDefaults()
        {
            // TRUE_VFX_STANDARDS: Enable trail cache for gradient trail
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.alpha = 255;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }
        
        public override void AI()
        {
            // Grow hitbox over time
            float progress = 1f - Projectile.timeLeft / 60f;
            Projectile.scale = 1f + progress * 0.5f;
            
            // Slow down
            Projectile.velocity *= 0.97f;
            
            // === TRUE_VFX_STANDARDS: DENSE DUST TRAIL (3+ per frame for flamethrower) ===
            for (int i = 0; i < 3; i++)
            {
                Vector2 dustOffset = Main.rand.NextVector2Circular(Projectile.width / 2f, Projectile.height / 2f);
                Vector2 dustVel = Main.rand.NextVector2Circular(2f, 2f) + new Vector2(0, -1.5f);
                
                Dust fire = Dust.NewDustPerfect(Projectile.Center + dustOffset, DustID.Torch, dustVel, 0, default, 1.6f * Projectile.scale);
                fire.noGravity = true;
                fire.fadeIn = 1.3f;
            }
            
            // === CONTRASTING SPARKLES (1-in-2) - GoldCoin for flame contrast ===
            if (Main.rand.NextBool(2))
            {
                Vector2 sparkOffset = Main.rand.NextVector2Circular(8f, 8f);
                Dust spark = Dust.NewDustPerfect(Projectile.Center + sparkOffset, DustID.GoldCoin, 
                    new Vector2(0, -2f) + Main.rand.NextVector2Circular(1f, 1f), 0, default, 1.1f * Projectile.scale);
                spark.noGravity = true;
            }
            
            // === SMOKE ACCENTS (1-in-3) ===
            if (Main.rand.NextBool(3))
            {
                Vector2 smokeOffset = Main.rand.NextVector2Circular(10f, 10f);
                Dust smoke = Dust.NewDustPerfect(Projectile.Center + smokeOffset, DustID.Smoke, 
                    new Vector2(0, -1.5f), 0, DiesIraeColors.CharredBlack, 1.2f * Projectile.scale);
                smoke.noGravity = true;
            }
            
            // === TRUE_VFX_STANDARDS: 3 ORBITING MUSIC NOTES LOCKED TO PROJECTILE ===
            float orbitAngle = Main.GameUpdateCount * 0.1f;
            float orbitRadius = 15f * Projectile.scale + (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 5f;
            
            if (Main.rand.NextBool(3))
            {
                for (int i = 0; i < 3; i++)
                {
                    float noteAngle = orbitAngle + MathHelper.TwoPi * i / 3f;
                    Vector2 noteOffset = noteAngle.ToRotationVector2() * orbitRadius;
                    Vector2 notePos = Projectile.Center + noteOffset;
                    
                    // Rising velocity with outward drift
                    Vector2 noteVel = new Vector2(0, -2f) + noteAngle.ToRotationVector2() * 0.5f;
                    
                    // hslToRgb color oscillation within Dies Irae range
                    float hue = HueMin + ((float)Math.Sin(Main.GameUpdateCount * 0.05f + i) * 0.5f + 0.5f) * (HueMax - HueMin);
                    Color noteColor = Main.hslToRgb(hue, 0.95f, 0.7f);
                    
                    DiesIraeVFX.SpawnMusicNote(notePos, noteVel, noteColor, 0.75f);
                    
                    // Sparkle companion
                    CustomParticles.GenericFlare(notePos, noteColor * 0.6f, 0.2f, 8);
                }
            }
            
            // === FLARES LITTERING THE AIR (1-in-2) ===
            if (Main.rand.NextBool(2))
            {
                Vector2 flareOffset = Main.rand.NextVector2Circular(12f * Projectile.scale, 12f * Projectile.scale);
                float hue = HueMin + (Main.rand.NextFloat() * (HueMax - HueMin));
                Color flareColor = Main.hslToRgb(hue, 0.9f, 0.75f);
                CustomParticles.GenericFlare(Projectile.Center + flareOffset, flareColor, 0.35f * Projectile.scale, 12);
            }
            
            // === PRESERVED EXISTING EFFECTS ===
            // Pulsing judgment aura
            DiesIraeVFX.PulsingAura(Projectile.Center, DiesIraeColors.BloodRed, 0.5f * Projectile.scale, Projectile.timeLeft);
            
            // Rising flame wisp burst (periodic)
            if (Main.rand.NextBool(4))
            {
                DiesIraeVFX.FlameWispBurst(Projectile.Center + Main.rand.NextVector2Circular(10f, 10f), 0.5f, 2);
            }
            
            // Spiral fire pattern
            if (Main.rand.NextBool(3))
            {
                float spiralAngle = progress * MathHelper.TwoPi * 3f;
                DiesIraeVFX.SpiralTrail(Projectile.Center, new Vector2(0, -2f), DiesIraeColors.EmberOrange, 0.4f * Projectile.scale, spiralAngle);
            }
            
            // Orbiting ember sparks (intensifies with time)
            if (Main.GameUpdateCount % 5 == 0)
            {
                int sparkCount = 2 + (int)(progress * 3);
                DiesIraeVFX.OrbitingSparks(Projectile.Center, DiesIraeColors.Crimson, 12f * Projectile.scale, sparkCount, Main.GameUpdateCount * 0.1f, 0.3f);
            }
            
            // Dynamic lighting with intensity that grows
            float lightPulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 0.15f;
            float lightIntensity = (0.6f + progress * 0.35f) * lightPulse;
            Lighting.AddLight(Projectile.Center, FlameEmber.ToVector3() * Projectile.scale * lightIntensity);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 300);
            
            // === TRUE_VFX_STANDARDS: MULTI-LAYER FLASH CASCADE ===
            // Layer 1: White-hot core flash
            CustomParticles.GenericFlare(target.Center, Color.White, 0.9f, 18);
            // Layer 2: Gold-orange flash
            CustomParticles.GenericFlare(target.Center, FlameCore, 0.7f, 15);
            // Layer 3: Ember flash
            CustomParticles.GenericFlare(target.Center, FlameEmber, 0.55f, 13);
            
            // === GRADIENT MUSIC NOTES ON HIT ===
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f + Main.rand.NextFloat(0.2f);
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                float hue = HueMin + ((float)i / 4f) * (HueMax - HueMin);
                Color noteColor = Main.hslToRgb(hue, 0.9f, 0.7f);
                DiesIraeVFX.SpawnMusicNote(target.Center, noteVel, noteColor, 0.8f);
            }
            
            // === HALO RING ===
            CustomParticles.HaloRing(target.Center, FlameEmber, 0.45f, 12);
            
            // === DYNAMIC PARTICLE EFFECTS - Dies Judgment Vortex (flame spiraling) ===
            DiesJudgmentVortex(target.Center, 0.85f);
            
            // Preserved existing impact
            DiesIraeVFX.FireImpact(target.Center, 0.8f);
            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact with { Volume = 0.4f }, target.Center);
        }
        
        public override void OnKill(int timeLeft)
        {
            // === TRUE_VFX_STANDARDS: GLIMMER CASCADE (not puff) ===
            // Layer 1: White-hot core glimmer
            CustomParticles.GenericFlare(Projectile.Center, Color.White, 0.8f, 20);
            // Layer 2: Core glimmer
            CustomParticles.GenericFlare(Projectile.Center, FlameCore, 0.65f, 18);
            // Layer 3: Ember glimmer
            CustomParticles.GenericFlare(Projectile.Center, FlameEmber, 0.5f, 15);
            
            // === EXPANDING HALO RINGS ===
            for (int ring = 0; ring < 2; ring++)
            {
                Color ringColor = Color.Lerp(FlameEmber, FlameBlood, ring / 2f);
                CustomParticles.HaloRing(Projectile.Center, ringColor, 0.35f + ring * 0.12f, 12 + ring * 3);
            }
            
            // === GRADIENT MUSIC NOTE FINALE ===
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi * i / 5f;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f) + new Vector2(0, -1.5f);
                float hue = HueMin + ((float)i / 5f) * (HueMax - HueMin);
                Color noteColor = Main.hslToRgb(hue, 0.9f, 0.7f);
                DiesIraeVFX.SpawnMusicNote(Projectile.Center, noteVel, noteColor, 0.75f);
            }
            
            // === SPARKLE BURST ===
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                Dust spark = Dust.NewDustPerfect(Projectile.Center, DustID.GoldCoin, sparkVel, 0, default, 1.1f);
                spark.noGravity = true;
            }
            
            // === DUST BURST FOR DENSITY ===
            for (int i = 0; i < 12; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(5f, 5f) + new Vector2(0, -2f);
                Dust fire = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, dustVel, 0, default, 1.4f);
                fire.noGravity = true;
            }
            
            // Preserved fire impact
            DiesIraeVFX.FireImpact(Projectile.Center, 0.65f);
        }
        
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // Call OnKill VFX before destruction
            OnKill(Projectile.timeLeft);
            return true;
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            // Flare textures for spinning layers
            Texture2D flareTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare").Value;
            Texture2D flareTex2 = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare4").Value;
            Texture2D glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow2").Value;
            Vector2 flareOrigin = flareTex.Size() / 2f;
            Vector2 flareOrigin2 = flareTex2.Size() / 2f;
            Vector2 glowOrigin = glowTex.Size() / 2f;
            
            float time = Main.GameUpdateCount * 0.06f;
            float pulse = 1f + (float)Math.Sin(time * 2f) * 0.12f;
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // === TRUE_VFX_STANDARDS: hslToRgb TRAIL GRADIENT ===
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                
                float progress = (float)i / Projectile.oldPos.Length;
                float hue = HueMin + progress * (HueMax - HueMin);
                Color trailColor = Main.hslToRgb(hue, 0.85f, 0.65f) * (1f - progress) * 0.5f;
                trailColor.A = 0;
                
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                float trailScale = Projectile.scale * (1f - progress * 0.4f);
                
                spriteBatch.Draw(texture, trailPos, null, trailColor, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0f);
            }
            
            // === MAIN FLAME BLOOM LAYERS ===
            // Layer 1: Black smoke outer (billowing effect)
            for (int i = 0; i < 4; i++)
            {
                Vector2 smokeOffset = (MathHelper.TwoPi * i / 4f + time * 0.3f).ToRotationVector2() * (6f * Projectile.scale);
                Color smokeGlow = DiesIraeColors.CharredBlack * 0.22f;
                smokeGlow.A = 0;
                spriteBatch.Draw(glowTex, drawPos + smokeOffset, null, smokeGlow, 0f, glowOrigin, 0.65f * Projectile.scale * pulse, SpriteEffects.None, 0f);
            }
            
            // Layer 2: Blood red glow
            for (int i = 0; i < 3; i++)
            {
                float scale = pulse * Projectile.scale * (0.55f + i * 0.08f);
                Color redGlow = FlameBlood * (0.35f / (i + 1));
                redGlow.A = 0;
                spriteBatch.Draw(texture, drawPos, null, redGlow, 0f, origin, scale, SpriteEffects.None, 0f);
            }
            
            // Layer 3: Ember orange
            Color emberGlow = FlameEmber * 0.4f;
            emberGlow.A = 0;
            spriteBatch.Draw(texture, drawPos, null, emberGlow, 0f, origin, 0.45f * Projectile.scale * pulse, SpriteEffects.None, 0f);
            
            // Layer 4: White-hot core
            Color whiteCore = Color.White * 0.55f;
            whiteCore.A = 0;
            spriteBatch.Draw(texture, drawPos, null, whiteCore, 0f, origin, 0.3f * Projectile.scale * pulse, SpriteEffects.None, 0f);
            
            // === TRUE_VFX_STANDARDS: 5 SPINNING FLARE LAYERS ===
            
            // Flare 1: Soft glow base (static)
            Color glowColor1 = FlameEmber * 0.35f;
            glowColor1.A = 0;
            spriteBatch.Draw(glowTex, drawPos, null, glowColor1, 0f, glowOrigin, 0.5f * Projectile.scale * pulse, SpriteEffects.None, 0f);
            
            // Flare 2: Spinning clockwise (fast)
            Color flareColor1 = FlameEmber * 0.5f;
            flareColor1.A = 0;
            spriteBatch.Draw(flareTex, drawPos, null, flareColor1, time, flareOrigin, 0.35f * Projectile.scale * pulse, SpriteEffects.None, 0f);
            
            // Flare 3: Counter-clockwise
            Color flareColor2 = FlameBlood * 0.45f;
            flareColor2.A = 0;
            spriteBatch.Draw(flareTex2, drawPos, null, flareColor2, -time * 0.75f, flareOrigin2, 0.3f * Projectile.scale * pulse, SpriteEffects.None, 0f);
            
            // Flare 4: hslToRgb accent (slow rotation)
            float hslHue = HueMin + ((float)Math.Sin(time * 0.7f) * 0.5f + 0.5f) * (HueMax - HueMin);
            Color hslColor = Main.hslToRgb(hslHue, 0.9f, 0.7f) * 0.45f;
            hslColor.A = 0;
            spriteBatch.Draw(flareTex, drawPos, null, hslColor, time * 1.2f, flareOrigin, 0.22f * Projectile.scale * pulse, SpriteEffects.None, 0f);
            
            // Flare 5: Core white accent
            Color coreFlare = FlameCore * 0.4f;
            coreFlare.A = 0;
            spriteBatch.Draw(flareTex2, drawPos, null, coreFlare, -time * 0.5f, flareOrigin2, 0.15f * Projectile.scale * pulse, SpriteEffects.None, 0f);
            
            // === 4 ORBITING SPARK POINTS ===
            float orbitSparkAngle = Main.GameUpdateCount * 0.08f;
            float sparkRadius = 14f * Projectile.scale;
            for (int i = 0; i < 4; i++)
            {
                float sparkAngle = orbitSparkAngle + MathHelper.TwoPi * i / 4f;
                Vector2 sparkPos = drawPos + sparkAngle.ToRotationVector2() * sparkRadius;
                Color sparkColor = Color.Lerp(FlameEmber, Color.White, 0.3f) * 0.5f;
                sparkColor.A = 0;
                spriteBatch.Draw(flareTex, sparkPos, null, sparkColor, sparkAngle, flareOrigin, 0.1f * Projectile.scale * pulse, SpriteEffects.None, 0f);
            }
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            return false;
        }
    }
    
    #endregion
    
    #region Sin Collector Projectiles
    
    /// <summary>
    /// Precise sniper bullet that chains lightning and spawns cleaver copies
    /// </summary>
    public class SinBullet : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/FlamingWispProjectileSmall";
        
        // TRUE_VFX_STANDARDS: Dies Irae blood red to orange hue range
        private const float HueMin = 0.0f;    // Blood red
        private const float HueMax = 0.08f;   // Orange-red
        
        // Color palette for this bullet
        private static readonly Color BulletCore = new Color(255, 220, 180);     // White-gold core
        private static readonly Color BulletFlame = new Color(255, 140, 50);     // Gold-orange flame
        private static readonly Color BulletBlood = new Color(200, 40, 30);      // Blood red
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 3;
        }
        
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // === TRUE_VFX_STANDARDS: DENSE DUST TRAIL (2+ per frame guaranteed) ===
            for (int i = 0; i < 2; i++)
            {
                Vector2 dustOffset = Main.rand.NextVector2Circular(3f, 3f);
                Vector2 dustVel = -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1f, 1f);
                Dust fire = Dust.NewDustPerfect(Projectile.Center + dustOffset, DustID.Torch, dustVel, 0, default, 1.3f);
                fire.noGravity = true;
                fire.fadeIn = 1.1f;
            }
            
            // === CONTRASTING SPARKLES (1-in-2) ===
            if (Main.rand.NextBool(2))
            {
                Dust spark = Dust.NewDustPerfect(Projectile.Center, DustID.GoldCoin, 
                    -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f), 0, default, 0.9f);
                spark.noGravity = true;
            }
            
            // === TRUE_VFX_STANDARDS: ORBITING MUSIC NOTES (2 notes locked to projectile) ===
            float orbitAngle = Main.GameUpdateCount * 0.12f;
            float orbitRadius = 8f;
            
            if (Main.rand.NextBool(4))
            {
                for (int i = 0; i < 2; i++)
                {
                    float noteAngle = orbitAngle + MathHelper.Pi * i;
                    Vector2 noteOffset = noteAngle.ToRotationVector2() * orbitRadius;
                    Vector2 notePos = Projectile.Center + noteOffset;
                    Vector2 noteVel = Projectile.velocity * 0.7f + noteAngle.ToRotationVector2() * 0.3f;
                    
                    // hslToRgb color oscillation within theme range
                    float hue = HueMin + (Main.rand.NextFloat() * (HueMax - HueMin));
                    Color noteColor = Main.hslToRgb(hue, 0.95f, 0.7f);
                    
                    DiesIraeVFX.SpawnMusicNote(notePos, noteVel, noteColor, 0.7f);
                }
            }
            
            // === FLARES LITTERING THE AIR (1-in-3) ===
            if (Main.rand.NextBool(3))
            {
                Vector2 flareOffset = Main.rand.NextVector2Circular(5f, 5f);
                float hue = HueMin + (Main.rand.NextFloat() * (HueMax - HueMin));
                Color flareColor = Main.hslToRgb(hue, 0.9f, 0.75f);
                CustomParticles.GenericFlare(Projectile.Center + flareOffset, flareColor, 0.3f, 10);
            }
            
            // === AFTERIMAGE TRAIL FOR SPEED ===
            if (Main.GameUpdateCount % 2 == 0)
            {
                DiesIraeVFX.AfterimageTrail(Projectile.Center, Projectile.velocity, 0.25f, DiesIraeColors.Crimson, 3);
            }
            
            // === SPIRAL TRAIL (1-in-2) ===
            if (Main.rand.NextBool(2))
            {
                float spiralAngle = Main.GameUpdateCount * 0.3f;
                DiesIraeVFX.SpiralTrail(Projectile.Center, Projectile.velocity, DiesIraeColors.BloodRed, 0.2f, spiralAngle);
            }
            
            // Enhanced pulsing lighting
            float lightPulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.2f;
            Lighting.AddLight(Projectile.Center, BulletFlame.ToVector3() * 0.5f * lightPulse);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 240);
            
            // === TRUE_VFX_STANDARDS: MULTI-LAYER FLASH CASCADE ===
            CustomParticles.GenericFlare(target.Center, Color.White, 0.8f, 18);
            CustomParticles.GenericFlare(target.Center, BulletCore, 0.65f, 16);
            CustomParticles.GenericFlare(target.Center, BulletFlame, 0.5f, 14);
            
            // Chain lightning to nearby enemies
            List<NPC> chainTargets = new List<NPC>();
            chainTargets.Add(target);
            
            NPC lastTarget = target;
            for (int chain = 0; chain < 4; chain++)
            {
                NPC nextTarget = FindChainTarget(lastTarget.Center, 300f, chainTargets);
                if (nextTarget == null) break;
                
                // Draw lightning
                DiesIraeVFX.ChainLightning(lastTarget.Center, nextTarget.Center, DiesIraeColors.HellfireGold);
                
                // Damage
                if (Main.myPlayer == Projectile.owner)
                {
                    nextTarget.SimpleStrikeNPC(Projectile.damage / 2, 0, false, 0f, DamageClass.Ranged);
                }
                nextTarget.AddBuff(BuffID.OnFire3, 180);
                
                chainTargets.Add(nextTarget);
                lastTarget = nextTarget;
            }
            
            // Spawn spinning cleaver copies
            if (Main.myPlayer == Projectile.owner)
            {
                for (int i = 0; i < 2; i++)
                {
                    float angle = MathHelper.TwoPi * i / 2f + Main.rand.NextFloat(-0.3f, 0.3f);
                    Vector2 spawnPos = target.Center + angle.ToRotationVector2() * 80f;
                    
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPos, Vector2.Zero,
                        ModContent.ProjectileType<SpinningCleaverCopy>(), Projectile.damage / 2, 0f, Projectile.owner, target.whoAmI);
                }
            }
            
            DiesIraeVFX.FireImpact(target.Center, 1f);
            
            // === DYNAMIC PARTICLE EFFECTS - Dies Wrath Chain Lightning (sin bullet) ===
            DiesWrathChainLightning(target.Center, Projectile.velocity.SafeNormalize(Vector2.UnitX), 1.1f);
            
            SoundEngine.PlaySound(SoundID.DD2_LightningBugZap, target.Center);
            
            // === TRUE_VFX_STANDARDS: GRADIENT MUSIC NOTES BURST ===
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(2.5f, 4f);
                
                // Gradient color across the burst
                float hue = HueMin + ((float)i / 6f * (HueMax - HueMin));
                Color noteColor = Main.hslToRgb(hue, 0.95f, 0.75f);
                
                DiesIraeVFX.SpawnMusicNote(target.Center, noteVel, noteColor, 0.85f);
            }
            
            // Halo ring
            CustomParticles.HaloRing(target.Center, BulletFlame, 0.4f, 15);
        }
        
        private NPC FindChainTarget(Vector2 position, float maxDistance, List<NPC> exclude)
        {
            NPC closest = null;
            float closestDist = maxDistance;
            
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || !npc.CanBeChasedBy()) continue;
                if (exclude.Contains(npc)) continue;
                
                float dist = Vector2.Distance(position, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = npc;
                }
            }
            
            return closest;
        }
        
        public override void OnKill(int timeLeft)
        {
            // === TRUE_VFX_STANDARDS: GLIMMER CASCADE (not puff) ===
            
            // 3-layer flash cascade
            CustomParticles.GenericFlare(Projectile.Center, Color.White, 0.7f, 20);
            CustomParticles.GenericFlare(Projectile.Center, BulletCore, 0.55f, 18);
            CustomParticles.GenericFlare(Projectile.Center, BulletFlame, 0.4f, 15);
            
            // 2 halo rings
            CustomParticles.HaloRing(Projectile.Center, BulletFlame, 0.35f, 14);
            CustomParticles.HaloRing(Projectile.Center, BulletBlood, 0.25f, 12);
            
            // 4 gradient music notes finale
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 3.5f);
                float hue = HueMin + ((float)i / 4f * (HueMax - HueMin));
                Color noteColor = Main.hslToRgb(hue, 0.9f, 0.7f);
                DiesIraeVFX.SpawnMusicNote(Projectile.Center, noteVel, noteColor, 0.75f);
            }
            
            // 6 sparkle burst
            for (int i = 0; i < 6; i++)
            {
                Vector2 sparkleVel = Main.rand.NextVector2Circular(4f, 4f);
                float hue = HueMin + (Main.rand.NextFloat() * (HueMax - HueMin));
                Color sparkleColor = Main.hslToRgb(hue, 0.9f, 0.8f);
                var sparkle = new SparkleParticle(Projectile.Center, sparkleVel, sparkleColor, 0.35f, 18);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // 8 dust burst for density
            for (int i = 0; i < 8; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(4f, 4f);
                Dust fire = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, dustVel, 0, default, 1.2f);
                fire.noGravity = true;
            }
            
            Lighting.AddLight(Projectile.Center, BulletFlame.ToVector3() * 0.8f);
            SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.5f }, Projectile.Center);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Texture2D flareTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare").Value;
            Texture2D flareTex2 = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare4").Value;
            Texture2D glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow2").Value;
            
            Vector2 origin = texture.Size() / 2f;
            Vector2 flareOrigin = flareTex.Size() / 2f;
            Vector2 flareOrigin2 = flareTex2.Size() / 2f;
            Vector2 glowOrigin = glowTex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            float time = Main.GameUpdateCount * 0.08f;
            float pulse = 1f + (float)Math.Sin(time * 2f) * 0.12f;
            
            // === TRUE_VFX_STANDARDS: hslToRgb TRAIL GRADIENT ===
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                
                float progress = i / (float)Projectile.oldPos.Length;
                float trailAlpha = (1f - progress) * 0.65f;
                float trailScale = (1f - progress * 0.5f);
                
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                
                // hslToRgb gradient from orange to dark red
                float hue = HueMin + (progress * (HueMax - HueMin));
                Color trailColor = Main.hslToRgb(hue, 0.9f, 0.6f - progress * 0.3f) * trailAlpha;
                trailColor.A = 0;
                
                Main.spriteBatch.Draw(texture, trailPos, null, trailColor, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0f);
                
                // White core for first third
                if (i < Projectile.oldPos.Length / 3)
                {
                    Color whiteTrail = Color.White * trailAlpha * 0.4f;
                    whiteTrail.A = 0;
                    Main.spriteBatch.Draw(texture, trailPos, null, whiteTrail, Projectile.oldRot[i], origin, trailScale * 0.7f, SpriteEffects.None, 0f);
                }
            }
            
            // === TRUE_VFX_STANDARDS: 4+ SPINNING FLARE LAYERS ===
            
            // Layer 1: Soft glow base (large, dim)
            Color glowColor1 = BulletFlame * 0.3f;
            glowColor1.A = 0;
            Main.spriteBatch.Draw(glowTex, drawPos, null, glowColor1, 0f, glowOrigin, 0.35f * pulse, SpriteEffects.None, 0f);
            
            // Layer 2: First flare spinning clockwise
            Color flareColor1 = BulletFlame * 0.55f;
            flareColor1.A = 0;
            Main.spriteBatch.Draw(flareTex, drawPos, null, flareColor1, time, flareOrigin, 0.25f * pulse, SpriteEffects.None, 0f);
            
            // Layer 3: Second flare spinning counter-clockwise
            Color flareColor2 = BulletBlood * 0.5f;
            flareColor2.A = 0;
            Main.spriteBatch.Draw(flareTex2, drawPos, null, flareColor2, -time * 0.7f, flareOrigin2, 0.2f * pulse, SpriteEffects.None, 0f);
            
            // Layer 4: Third flare different speed
            Color flareColor3 = BulletCore * 0.6f;
            flareColor3.A = 0;
            Main.spriteBatch.Draw(flareTex, drawPos, null, flareColor3, time * 1.4f, flareOrigin, 0.15f * pulse, SpriteEffects.None, 0f);
            
            // Layer 5: White hot center
            Color whiteCore = Color.White * 0.7f;
            whiteCore.A = 0;
            Main.spriteBatch.Draw(flareTex, drawPos, null, whiteCore, 0f, flareOrigin, 0.08f, SpriteEffects.None, 0f);
            
            // === 2 ORBITING SPARK POINTS ===
            float orbitAngle = Main.GameUpdateCount * 0.1f;
            for (int i = 0; i < 2; i++)
            {
                float sparkAngle = orbitAngle + MathHelper.Pi * i;
                Vector2 sparkPos = drawPos + sparkAngle.ToRotationVector2() * 6f;
                Color sparkColor = Color.Lerp(BulletFlame, Color.White, 0.5f) * 0.65f;
                sparkColor.A = 0;
                Main.spriteBatch.Draw(flareTex, sparkPos, null, sparkColor, sparkAngle, flareOrigin, 0.06f * pulse, SpriteEffects.None, 0f);
            }
            
            return false;
        }
    }
    
    /// <summary>
    /// Spinning copy of Wrath's Cleaver that slices enemies - TRUE_VFX_STANDARDS compliant
    /// </summary>
    public class SpinningCleaverCopy : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/DiesIrae/ResonantWeapons/WrathsCleaver";
        
        // TRUE_VFX_STANDARDS: Dies Irae blood red to orange hue range
        private const float HueMin = 0.0f;    // Blood red
        private const float HueMax = 0.08f;   // Orange-red
        
        // Color palette for this cleaver
        private static readonly Color CleaverCore = new Color(255, 230, 195);     // White-gold core
        private static readonly Color CleaverFlame = new Color(255, 145, 55);     // Gold-orange flame
        private static readonly Color CleaverBlood = new Color(215, 45, 35);      // Blood red
        
        private int targetNPC = -1;
        
        public override void SetStaticDefaults()
        {
            // TRUE_VFX_STANDARDS: Enable trail cache
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 5;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }
        
        public override void AI()
        {
            // Get target from ai[0]
            if (Projectile.ai[0] >= 0 && Projectile.ai[0] < Main.maxNPCs)
            {
                targetNPC = (int)Projectile.ai[0];
            }
            
            // Spinning
            Projectile.rotation += 0.25f;
            
            // Move toward target or nearest enemy
            NPC target = null;
            if (targetNPC >= 0 && Main.npc[targetNPC].active)
            {
                target = Main.npc[targetNPC];
            }
            else
            {
                target = FindClosestNPC(500f);
            }
            
            if (target != null)
            {
                Vector2 direction = target.Center - Projectile.Center;
                direction.Normalize();
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * 12f, 0.08f);
            }
            
            // === TRUE_VFX_STANDARDS: DENSE DUST TRAIL (2+ per frame guaranteed) ===
            for (int i = 0; i < 2; i++)
            {
                // Spawn dust along the blade edge
                float edgeAngle = Projectile.rotation + MathHelper.PiOver2 * (i == 0 ? 1 : -1);
                Vector2 edgeOffset = edgeAngle.ToRotationVector2() * 25f;
                Vector2 dustVel = -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1.5f, 1.5f);
                
                Dust fire = Dust.NewDustPerfect(Projectile.Center + edgeOffset, DustID.Torch, dustVel, 0, default, 1.4f);
                fire.noGravity = true;
                fire.fadeIn = 1.2f;
            }
            
            // === CONTRASTING SPARKLES (1-in-2) ===
            if (Main.rand.NextBool(2))
            {
                float sparkAngle = Projectile.rotation + Main.rand.NextFloat(-0.5f, 0.5f);
                Vector2 sparkOffset = sparkAngle.ToRotationVector2() * Main.rand.NextFloat(15f, 30f);
                Dust spark = Dust.NewDustPerfect(Projectile.Center + sparkOffset, DustID.GoldCoin, 
                    -Projectile.velocity * 0.1f, 0, default, 1.0f);
                spark.noGravity = true;
            }
            
            // === TRUE_VFX_STANDARDS: 3 ORBITING MUSIC NOTES LOCKED TO CLEAVER ===
            float orbitAngle = Main.GameUpdateCount * 0.1f;
            float orbitRadius = 35f + (float)Math.Sin(Main.GameUpdateCount * 0.07f) * 8f;
            
            if (Main.rand.NextBool(3))
            {
                for (int i = 0; i < 3; i++)
                {
                    float noteAngle = orbitAngle + MathHelper.TwoPi * i / 3f;
                    Vector2 noteOffset = noteAngle.ToRotationVector2() * orbitRadius;
                    Vector2 notePos = Projectile.Center + noteOffset;
                    
                    // Note velocity matches cleaver + outward spiral
                    Vector2 noteVel = Projectile.velocity * 0.6f + noteAngle.ToRotationVector2() * 0.5f;
                    
                    // hslToRgb color oscillation
                    float hue = HueMin + ((float)Math.Sin(Main.GameUpdateCount * 0.04f + i) * 0.5f + 0.5f) * (HueMax - HueMin);
                    Color noteColor = Main.hslToRgb(hue, 0.95f, 0.7f);
                    
                    DiesIraeVFX.SpawnMusicNote(notePos, noteVel, noteColor, 0.75f);
                    
                    // Sparkle companion
                    CustomParticles.GenericFlare(notePos, noteColor * 0.5f, 0.2f, 8);
                }
            }
            
            // === FLARES LITTERING THE AIR (1-in-2) ===
            if (Main.rand.NextBool(2))
            {
                Vector2 flareOffset = Main.rand.NextVector2Circular(20f, 20f);
                float hue = HueMin + (Main.rand.NextFloat() * (HueMax - HueMin));
                Color flareColor = Main.hslToRgb(hue, 0.9f, 0.75f);
                CustomParticles.GenericFlare(Projectile.Center + flareOffset, flareColor, 0.35f, 12);
            }
            
            // === PRESERVED EXISTING EFFECTS ===
            // Fire trail
            DiesIraeVFX.FireTrail(Projectile.Center, Projectile.velocity, 0.8f);
            
            // Afterimage trail (add for spinning effect)
            if (Main.GameUpdateCount % 2 == 0)
            {
                DiesIraeVFX.AfterimageTrail(Projectile.Center, Projectile.velocity, 0.35f, DiesIraeColors.Crimson, 3);
            }
            
            // Spiral trail for extra flair (1-in-3)
            if (Main.rand.NextBool(3))
            {
                DiesIraeVFX.SpiralTrail(Projectile.Center, Projectile.velocity, DiesIraeColors.EmberOrange, 0.35f, Projectile.rotation);
            }
            
            // Enhanced pulsing lighting
            float lightPulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.2f;
            Lighting.AddLight(Projectile.Center, CleaverFlame.ToVector3() * 0.6f * lightPulse);
        }
        
        private NPC FindClosestNPC(float maxDistance)
        {
            NPC closest = null;
            float closestDist = maxDistance;
            
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.CanBeChasedBy())
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
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 180);
            
            // === TRUE_VFX_STANDARDS: MULTI-LAYER FLASH CASCADE ===
            // Layer 1: White-hot core flash
            CustomParticles.GenericFlare(target.Center, Color.White, 0.85f, 18);
            // Layer 2: Core color flash
            CustomParticles.GenericFlare(target.Center, CleaverCore, 0.65f, 15);
            // Layer 3: Flame color flash
            CustomParticles.GenericFlare(target.Center, CleaverFlame, 0.5f, 12);
            
            // === GRADIENT MUSIC NOTES ON HIT ===
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f + Main.rand.NextFloat(0.2f);
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                float hue = HueMin + ((float)i / 4f) * (HueMax - HueMin);
                Color noteColor = Main.hslToRgb(hue, 0.9f, 0.7f);
                DiesIraeVFX.SpawnMusicNote(target.Center, noteVel, noteColor, 0.8f);
            }
            
            // === HALO RING ===
            CustomParticles.HaloRing(target.Center, CleaverFlame, 0.4f, 12);
            
            // === DYNAMIC PARTICLE EFFECTS - Dies Hellfire Eruption (cleaver strike) ===
            DiesHellfireEruption(target.Center, 0.7f);
            
            // Preserved fire impact
            DiesIraeVFX.FireImpact(target.Center, 0.6f);
        }
        
        public override void OnKill(int timeLeft)
        {
            // === TRUE_VFX_STANDARDS: GLIMMER CASCADE (not puff) ===
            // Layer 1: White-hot core glimmer
            CustomParticles.GenericFlare(Projectile.Center, Color.White, 1.0f, 22);
            // Layer 2: Core glimmer
            CustomParticles.GenericFlare(Projectile.Center, CleaverCore, 0.8f, 18);
            // Layer 3: Flame glimmer
            CustomParticles.GenericFlare(Projectile.Center, CleaverFlame, 0.65f, 15);
            // Layer 4: Blood glimmer
            CustomParticles.GenericFlare(Projectile.Center, CleaverBlood, 0.5f, 12);
            
            // === EXPANDING HALO RINGS ===
            for (int ring = 0; ring < 3; ring++)
            {
                Color ringColor = Color.Lerp(CleaverFlame, CleaverBlood, ring / 3f);
                CustomParticles.HaloRing(Projectile.Center, ringColor, 0.4f + ring * 0.15f, 14 + ring * 3);
            }
            
            // === GRADIENT MUSIC NOTE FINALE ===
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                float hue = HueMin + ((float)i / 6f) * (HueMax - HueMin);
                Color noteColor = Main.hslToRgb(hue, 0.9f, 0.7f);
                DiesIraeVFX.SpawnMusicNote(Projectile.Center, noteVel, noteColor, 0.8f);
            }
            
            // === SPARKLE BURST ===
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                Dust spark = Dust.NewDustPerfect(Projectile.Center, DustID.GoldCoin, sparkVel, 0, default, 1.2f);
                spark.noGravity = true;
            }
            
            // === DUST BURST FOR DENSITY ===
            for (int i = 0; i < 14; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(6f, 6f);
                Dust fire = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, dustVel, 0, default, 1.5f);
                fire.noGravity = true;
            }
            
            // Preserved effects
            DiesIraeVFX.FireImpact(Projectile.Center, 1f);
            SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with { Volume = 0.5f }, Projectile.Center);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            // Flare textures for spinning layers
            Texture2D flareTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare").Value;
            Texture2D flareTex2 = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare4").Value;
            Texture2D glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow2").Value;
            Vector2 flareOrigin = flareTex.Size() / 2f;
            Vector2 flareOrigin2 = flareTex2.Size() / 2f;
            Vector2 glowOrigin = glowTex.Size() / 2f;
            
            float time = Main.GameUpdateCount * 0.06f;
            float pulse = 1f + (float)Math.Sin(time * 2.5f) * 0.12f;
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // === TRUE_VFX_STANDARDS: hslToRgb SPIN TRAIL ===
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                
                float progress = (float)i / Projectile.oldPos.Length;
                float hue = HueMin + progress * (HueMax - HueMin);
                Color trailColor = Main.hslToRgb(hue, 0.85f, 0.6f) * (1f - progress) * 0.4f;
                trailColor.A = 0;
                
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                float trailScale = Projectile.scale * (1f - progress * 0.25f);
                
                spriteBatch.Draw(texture, trailPos, null, trailColor, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0f);
            }
            
            // === ENHANCED SPECTRAL SPINNING CLEAVER LAYERS ===
            
            // Layer 1: Black outer trail (smoky)
            for (int i = 0; i < 6; i++)
            {
                float rotOffset = -i * 0.12f;
                float alpha = 0.25f * (1f - i / 6f);
                Color blackTrail = DiesIraeColors.CharredBlack * alpha;
                blackTrail.A = 0;
                spriteBatch.Draw(texture, drawPos, null, blackTrail, Projectile.rotation + rotOffset, origin, Projectile.scale * 1.15f, SpriteEffects.None, 0f);
            }
            
            // Layer 2: Red spectral trail
            for (int i = 0; i < 5; i++)
            {
                float rotOffset = -i * 0.15f;
                float alpha = 0.35f * (1f - i / 5f);
                Color redTrail = Color.Lerp(CleaverBlood, DiesIraeColors.Crimson, i / 5f) * alpha;
                redTrail.A = 0;
                spriteBatch.Draw(texture, drawPos, null, redTrail, Projectile.rotation + rotOffset, origin, Projectile.scale * (1.05f - i * 0.02f), SpriteEffects.None, 0f);
            }
            
            // Layer 3: Orange ember glow
            Color emberGlow = CleaverFlame * 0.4f;
            emberGlow.A = 0;
            spriteBatch.Draw(texture, drawPos, null, emberGlow, Projectile.rotation, origin, Projectile.scale * pulse, SpriteEffects.None, 0f);
            
            // Layer 4: White-hot blade core
            Color whiteCore = Color.White * 0.65f;
            whiteCore.A = 0;
            spriteBatch.Draw(texture, drawPos, null, whiteCore, Projectile.rotation, origin, Projectile.scale * 0.85f * pulse, SpriteEffects.None, 0f);
            
            // === TRUE_VFX_STANDARDS: 4 SPINNING FLARE LAYERS ===
            
            // Flare 1: Soft glow base
            Color glowColor1 = CleaverFlame * 0.3f;
            glowColor1.A = 0;
            spriteBatch.Draw(glowTex, drawPos, null, glowColor1, 0f, glowOrigin, 0.45f * pulse, SpriteEffects.None, 0f);
            
            // Flare 2: Spinning clockwise
            Color flareColor1 = CleaverFlame * 0.45f;
            flareColor1.A = 0;
            spriteBatch.Draw(flareTex, drawPos, null, flareColor1, time, flareOrigin, 0.32f * pulse, SpriteEffects.None, 0f);
            
            // Flare 3: Counter-clockwise
            Color flareColor2 = CleaverBlood * 0.4f;
            flareColor2.A = 0;
            spriteBatch.Draw(flareTex2, drawPos, null, flareColor2, -time * 0.8f, flareOrigin2, 0.28f * pulse, SpriteEffects.None, 0f);
            
            // Flare 4: hslToRgb accent
            float hslHue = HueMin + ((float)Math.Sin(time * 0.8f) * 0.5f + 0.5f) * (HueMax - HueMin);
            Color hslColor = Main.hslToRgb(hslHue, 0.9f, 0.7f) * 0.4f;
            hslColor.A = 0;
            spriteBatch.Draw(flareTex, drawPos, null, hslColor, time * 1.2f, flareOrigin, 0.2f * pulse, SpriteEffects.None, 0f);
            
            // === 4 ORBITING SPARK POINTS around the cleaver ===
            float orbitSparkAngle = Main.GameUpdateCount * 0.12f;
            for (int i = 0; i < 4; i++)
            {
                float sparkAngle = orbitSparkAngle + MathHelper.TwoPi * i / 4f;
                Vector2 sparkPos = drawPos + sparkAngle.ToRotationVector2() * 35f;
                Color sparkColor = Color.Lerp(CleaverFlame, Color.White, 0.3f) * 0.5f;
                sparkColor.A = 0;
                spriteBatch.Draw(flareTex, sparkPos, null, sparkColor, sparkAngle, flareOrigin, 0.12f * pulse, SpriteEffects.None, 0f);
            }
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            return false;
        }
    }
    
    #endregion
    
    #region Grimoire of Condemnation Projectiles
    
    /// <summary>
    /// Blazing music shard that spirals and chains electricity
    /// TRUE_VFX_STANDARDS: Orbiting music notes, dense dust, spinning flares, hslToRgb color oscillation
    /// </summary>
    public class BlazingMusicShard : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/FlamingWispProjectileSmall";
        
        // TRUE_VFX_STANDARDS: Dies Irae red hue range for color oscillation
        private const float HueMin = 0.0f;   // Blood red
        private const float HueMax = 0.08f;  // Orange-red
        
        // Dies Irae music shard color palette
        private static readonly Color ShardCore = new Color(255, 200, 150);    // Bright hellfire core
        private static readonly Color ShardFlame = new Color(255, 100, 30);    // Ember orange
        private static readonly Color ShardBlood = new Color(180, 30, 30);     // Blood red accent
        
        private float spiralAngle;
        private float spiralRadius = 50f;
        private int spiralTimer = 0;
        private int targetNPC = -1;
        private bool hasChained = false;
        
        public override void SetStaticDefaults()
        {
            // TRUE_VFX_STANDARDS: Trail cache for smooth rendering
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }
        
        public override void AI()
        {
            spiralAngle = Projectile.ai[0];
            spiralTimer++;
            
            // Find target
            if (targetNPC < 0 || !Main.npc[targetNPC].active)
            {
                targetNPC = FindClosestNPCIndex(800f);
            }
            
            // Spiral toward target
            if (targetNPC >= 0 && Main.npc[targetNPC].active)
            {
                NPC target = Main.npc[targetNPC];
                
                // Spiral motion
                spiralAngle += 0.15f;
                spiralRadius = Math.Max(10f, spiralRadius - 1f);
                Projectile.ai[0] = spiralAngle;
                
                Vector2 spiralOffset = spiralAngle.ToRotationVector2() * spiralRadius;
                Vector2 targetPos = target.Center + spiralOffset;
                
                Vector2 direction = targetPos - Projectile.Center;
                if (direction.Length() > 5f)
                {
                    direction.Normalize();
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * 14f, 0.1f);
                }
            }
            
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // ========================================
            // TRUE_VFX_STANDARDS IMPLEMENTATION
            // ========================================
            
            // === DENSE DUST TRAIL (GUARANTEED 2+ per frame) ===
            for (int i = 0; i < 2; i++)
            {
                Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(8f, 8f);
                Vector2 dustVel = -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(2f, 2f);
                
                // Primary fire dust
                Dust fire = Dust.NewDustPerfect(dustPos, DustID.Torch, dustVel, 0, default, 1.6f);
                fire.noGravity = true;
                fire.fadeIn = 1.3f;
            }
            
            // Contrasting gold sparkle dust (1-in-2)
            if (Main.rand.NextBool(2))
            {
                Vector2 sparkleVel = -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1.5f, 1.5f);
                Dust gold = Dust.NewDustPerfect(Projectile.Center, DustID.GoldCoin, sparkleVel, 0, default, 1.2f);
                gold.noGravity = true;
            }
            
            // === ORBITING MUSIC NOTES LOCKED TO PROJECTILE ===
            // This IS a MUSIC SHARD - music notes MUST orbit it!
            float noteOrbitAngle = Main.GameUpdateCount * 0.1f;
            float noteRadius = 18f + (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 4f;
            
            if (spiralTimer % 6 == 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    float individualAngle = noteOrbitAngle + MathHelper.TwoPi * i / 3f;
                    Vector2 noteOffset = individualAngle.ToRotationVector2() * noteRadius;
                    Vector2 notePos = Projectile.Center + noteOffset;
                    Vector2 noteVel = Projectile.velocity * 0.7f + individualAngle.ToRotationVector2() * 0.8f;
                    
                    // Color oscillates with hslToRgb
                    float hue = HueMin + ((Main.GameUpdateCount * 0.015f + i * 0.33f) % 1f) * (HueMax - HueMin);
                    Color noteColor = Main.hslToRgb(hue, 0.95f, 0.7f);
                    
                    // VISIBLE music note (scale 0.8f+)
                    DiesIraeVFX.SpawnMusicNote(notePos, noteVel, noteColor, 0.85f);
                    
                    // Sparkle companion
                    var sparkle = new SparkleParticle(notePos, noteVel * 0.5f, Color.White * 0.7f, 0.3f, 18);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
            }
            
            // === 5-POINT MUSICAL CONSTELLATION ORBIT ===
            float constellationAngle = Main.GameUpdateCount * 0.06f;
            if (spiralTimer % 4 == 0)
            {
                for (int i = 0; i < 5; i++)
                {
                    float starAngle = constellationAngle + MathHelper.TwoPi * i / 5f;
                    Vector2 starPos = Projectile.Center + starAngle.ToRotationVector2() * 14f;
                    
                    float colorProgress = (float)i / 5f;
                    Color starColor = Color.Lerp(ShardCore, ShardFlame, colorProgress);
                    
                    var star = new SparkleParticle(starPos, Vector2.Zero, starColor, 0.25f, 8);
                    MagnumParticleHandler.SpawnParticle(star);
                }
            }
            
            // === FLARES LITTERING THE AIR (1-in-2) ===
            if (Main.rand.NextBool(2))
            {
                Vector2 flarePos = Projectile.Center + Main.rand.NextVector2Circular(12f, 12f);
                
                // hslToRgb color oscillation for flare
                float flareHue = HueMin + (Main.GameUpdateCount * 0.02f % 1f) * (HueMax - HueMin);
                Color flareColor = Main.hslToRgb(flareHue, 0.9f, 0.75f);
                
                var flare = new BloomParticle(flarePos, -Projectile.velocity * 0.08f, flareColor * 0.7f, 0.4f, 15);
                MagnumParticleHandler.SpawnParticle(flare);
            }
            
            // === CORE FIRE TRAIL ===
            DiesIraeVFX.FireTrail(Projectile.Center, Projectile.velocity, 0.8f);
            
            // === SPIRAL MUSICAL TRAIL ===
            DiesIraeVFX.SpiralTrail(Projectile.Center, Projectile.velocity, DiesIraeColors.EmberOrange, 0.4f, spiralAngle);
            
            // === AFTERIMAGE with music theme ===
            if (Projectile.velocity.Length() > 5f)
            {
                DiesIraeVFX.AfterimageTrail(Projectile.Center, Projectile.velocity, 0.4f, DiesIraeColors.HellfireGold, 4);
            }
            
            // === PULSING MUSICAL AURA ===
            if (Main.rand.NextBool(3))
            {
                DiesIraeVFX.PulsingAura(Projectile.Center, DiesIraeColors.HellfireGold, 0.4f, (int)(spiralTimer * 10));
            }
            
            // Dynamic lighting with pulse and hslToRgb
            float lightHue = HueMin + ((Main.GameUpdateCount * 0.01f) % 1f) * (HueMax - HueMin);
            Color lightColor = Main.hslToRgb(lightHue, 0.9f, 0.6f);
            float lightPulse = 0.6f + (float)Math.Sin(spiralTimer * 0.15f) * 0.2f;
            Lighting.AddLight(Projectile.Center, lightColor.ToVector3() * lightPulse);
        }
        
        private int FindClosestNPCIndex(float maxDistance)
        {
            int closest = -1;
            float closestDist = maxDistance;
            
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.CanBeChasedBy())
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = i;
                    }
                }
            }
            
            return closest;
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 240);
            target.AddBuff(BuffID.Electrified, 120);
            
            // Chain electricity between enemies
            if (!hasChained)
            {
                hasChained = true;
                List<NPC> chainedNPCs = new List<NPC> { target };
                NPC lastNPC = target;
                
                for (int i = 0; i < 3; i++)
                {
                    NPC nextNPC = FindChainTarget(lastNPC.Center, 250f, chainedNPCs);
                    if (nextNPC == null) break;
                    
                    DiesIraeVFX.ChainLightning(lastNPC.Center, nextNPC.Center, DiesIraeColors.HellfireGold);
                    
                    if (Main.myPlayer == Projectile.owner)
                    {
                        nextNPC.SimpleStrikeNPC(Projectile.damage / 3, 0, false, 0f, DamageClass.Magic);
                    }
                    nextNPC.AddBuff(BuffID.Electrified, 90);
                    
                    chainedNPCs.Add(nextNPC);
                    lastNPC = nextNPC;
                }
            }
            
            // === TRUE_VFX_STANDARDS: 3-LAYER FLASH CASCADE ON HIT ===
            // Layer 1: Blinding white core
            var whiteFlash = new BloomParticle(target.Center, Vector2.Zero, Color.White * 0.9f, 0.8f, 18);
            MagnumParticleHandler.SpawnParticle(whiteFlash);
            
            // Layer 2: Hellfire gold mid
            var goldFlash = new BloomParticle(target.Center, Vector2.Zero, ShardFlame * 0.8f, 0.65f, 20);
            MagnumParticleHandler.SpawnParticle(goldFlash);
            
            // Layer 3: Blood red outer
            var redFlash = new BloomParticle(target.Center, Vector2.Zero, ShardBlood * 0.7f, 0.5f, 22);
            MagnumParticleHandler.SpawnParticle(redFlash);
            
            // Gradient music notes burst
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 5f);
                float hue = HueMin + ((float)i / 6f) * (HueMax - HueMin);
                Color noteColor = Main.hslToRgb(hue, 0.95f, 0.7f);
                DiesIraeVFX.SpawnMusicNote(target.Center, noteVel, noteColor, 0.85f);
            }
            
            // 2 expanding halo rings
            var halo1 = new BloomRingParticle(target.Center, Vector2.Zero, ShardFlame * 0.7f, 0.4f, 18);
            MagnumParticleHandler.SpawnParticle(halo1);
            var halo2 = new BloomRingParticle(target.Center, Vector2.Zero, ShardBlood * 0.6f, 0.55f, 22);
            MagnumParticleHandler.SpawnParticle(halo2);
            
            // Sparkle burst
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 7f);
                Color sparkColor = Color.Lerp(ShardCore, ShardFlame, (float)i / 8f);
                var sparkle = new SparkleParticle(target.Center, sparkVel, sparkColor, 0.4f, 22);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            DiesIraeVFX.FireImpact(target.Center, 0.9f);
        }
        
        private NPC FindChainTarget(Vector2 position, float maxDistance, List<NPC> exclude)
        {
            NPC closest = null;
            float closestDist = maxDistance;
            
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || !npc.CanBeChasedBy()) continue;
                if (exclude.Contains(npc)) continue;
                
                float dist = Vector2.Distance(position, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = npc;
                }
            }
            
            return closest;
        }
        
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.DD2_LightningBugZap with { Volume = 0.5f }, Projectile.Center);
            
            // === TRUE_VFX_STANDARDS: GLIMMER CASCADE (not puff!) ===
            
            // 4-layer glimmer cascade
            for (int layer = 0; layer < 4; layer++)
            {
                float layerScale = 0.35f + layer * 0.18f;
                float layerAlpha = 0.85f - layer * 0.15f;
                Color layerColor = Color.Lerp(Color.White, ShardFlame, layer / 4f);
                layerColor.A = 0;
                
                var glimmer = new BloomParticle(Projectile.Center, Vector2.Zero, layerColor * layerAlpha, layerScale, 20 - layer * 2);
                MagnumParticleHandler.SpawnParticle(glimmer);
            }
            
            // 3 expanding halo rings with gradient
            for (int ring = 0; ring < 3; ring++)
            {
                Color ringColor = Color.Lerp(ShardFlame, ShardBlood, ring / 3f);
                ringColor.A = 0;
                var halo = new BloomRingParticle(Projectile.Center, Vector2.Zero, ringColor * 0.7f, 0.35f + ring * 0.15f, 18 + ring * 4);
                MagnumParticleHandler.SpawnParticle(halo);
            }
            
            // 10 music notes finale - THIS IS A MUSIC SHARD!
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                float hue = HueMin + ((float)i / 10f) * (HueMax - HueMin);
                Color noteColor = Main.hslToRgb(hue, 0.95f, 0.7f);
                DiesIraeVFX.SpawnMusicNote(Projectile.Center, noteVel, noteColor, 0.9f);
            }
            
            // Sparkle + glow particle burst
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                Color burstColor = Color.Lerp(ShardCore, ShardFlame, (float)i / 10f);
                
                var sparkle = new SparkleParticle(Projectile.Center, burstVel, burstColor, 0.45f, 25);
                MagnumParticleHandler.SpawnParticle(sparkle);
                
                var glow = new GenericGlowParticle(Projectile.Center, burstVel * 0.8f, burstColor * 0.8f, 0.35f, 22, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Dust explosion for density
            for (int i = 0; i < 15; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(6f, 6f);
                Dust fire = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, dustVel, 0, default, 1.5f);
                fire.noGravity = true;
            }
            
            // Fire impact base
            DiesIraeVFX.FireImpact(Projectile.Center, 0.8f);
            
            // Bright lighting flash
            Lighting.AddLight(Projectile.Center, ShardFlame.ToVector3() * 1.5f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Texture2D flare1 = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare").Value;
            Texture2D flare2 = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare4").Value;
            Texture2D softGlow = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow2").Value;
            
            Vector2 origin = texture.Size() / 2f;
            Vector2 flareOrigin1 = flare1.Size() / 2f;
            Vector2 flareOrigin2 = flare2.Size() / 2f;
            Vector2 glowOrigin = softGlow.Size() / 2f;
            
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float time = Main.GameUpdateCount * 0.05f;
            float pulse = 1f + (float)Math.Sin(time * 2f) * 0.18f;
            
            // ========================================
            // TRUE_VFX_STANDARDS: 6-LAYER SPINNING FLARES
            // ========================================
            
            // Layer 1: Soft glow base (large, dim, charred black)
            Color glowBlack = DiesIraeColors.CharredBlack * 0.35f;
            glowBlack.A = 0;
            Main.spriteBatch.Draw(softGlow, drawPos, null, glowBlack, 0f, glowOrigin, 0.9f * pulse, SpriteEffects.None, 0f);
            
            // Layer 2: Blood red flare spinning clockwise
            Color flareRed = ShardBlood * 0.6f;
            flareRed.A = 0;
            Main.spriteBatch.Draw(flare1, drawPos, null, flareRed, time * 0.8f, flareOrigin1, 0.55f * pulse, SpriteEffects.None, 0f);
            
            // Layer 3: Ember orange flare spinning counter-clockwise
            Color flareOrange = ShardFlame * 0.65f;
            flareOrange.A = 0;
            Main.spriteBatch.Draw(flare2, drawPos, null, flareOrange, -time * 0.65f, flareOrigin2, 0.48f * pulse, SpriteEffects.None, 0f);
            
            // Layer 4: Hellfire gold flare spinning faster
            Color flareGold = DiesIraeColors.HellfireGold * 0.7f;
            flareGold.A = 0;
            Main.spriteBatch.Draw(flare1, drawPos, null, flareGold, time * 1.2f, flareOrigin1, 0.4f * pulse, SpriteEffects.None, 0f);
            
            // Layer 5: Crimson core spinning opposite
            Color flareCrimson = DiesIraeColors.Crimson * 0.75f;
            flareCrimson.A = 0;
            Main.spriteBatch.Draw(flare2, drawPos, null, flareCrimson, -time * 0.9f, flareOrigin2, 0.32f * pulse, SpriteEffects.None, 0f);
            
            // Layer 6: Bright white hot core
            Color coreWhite = Color.White * 0.85f;
            coreWhite.A = 0;
            Main.spriteBatch.Draw(flare1, drawPos, null, coreWhite, 0f, flareOrigin1, 0.22f * pulse, SpriteEffects.None, 0f);
            
            // === TRAIL RENDERING with hslToRgb gradient ===
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                
                float progress = (float)i / Projectile.oldPos.Length;
                float hue = HueMin + progress * (HueMax - HueMin);
                Color trailColor = Main.hslToRgb(hue, 0.9f, 0.65f) * (1f - progress) * 0.6f;
                trailColor.A = 0;
                float trailScale = (1f - progress * 0.5f) * 0.4f;
                
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Main.spriteBatch.Draw(texture, trailPos, null, trailColor, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0f);
            }
            
            // === 5 ORBITING SPARK POINTS ===
            float orbitAngle = Main.GameUpdateCount * 0.08f;
            for (int i = 0; i < 5; i++)
            {
                float sparkAngle = orbitAngle + MathHelper.TwoPi * i / 5f;
                Vector2 sparkPos = drawPos + sparkAngle.ToRotationVector2() * 16f;
                float sparkProgress = (float)i / 5f;
                Color sparkColor = Color.Lerp(ShardCore, ShardFlame, sparkProgress) * 0.8f;
                sparkColor.A = 0;
                Main.spriteBatch.Draw(flare1, sparkPos, null, sparkColor, 0f, flareOrigin1, 0.12f * pulse, SpriteEffects.None, 0f);
            }
            
            return false;
        }
    }
    
    #endregion
    
    #region Staff of Final Judgment Projectiles
    
    /// <summary>
    /// Floating ignition orb that targets enemies
    /// </summary>
    public class FloatingIgnition : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/FlamingWispProjectileSmall";
        
        // === TRUE_VFX_STANDARDS: Hue range for Dies Irae (blood red to orange) ===
        private const float HueMin = 0.0f;
        private const float HueMax = 0.08f;
        
        // Color palette for this projectile
        private static readonly Color IgniteCore = new Color(255, 255, 220);
        private static readonly Color IgniteFlame = DiesIraeColors.HellfireGold;
        private static readonly Color IgniteBlood = DiesIraeColors.BloodRed;
        
        private float orbitAngle;
        private int orbitTimer = 0;
        private const int OrbitDuration = 60;
        private int targetNPC = -1;
        private bool seeking = false;
        
        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }
        
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            orbitAngle = Projectile.ai[0];
            int index = (int)Projectile.ai[1];
            
            orbitTimer++;
            
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.1f + index) * 0.2f;
            
            if (!seeking && orbitTimer < OrbitDuration)
            {
                // Orbit around player
                orbitAngle += 0.06f;
                Projectile.ai[0] = orbitAngle;
                
                float radius = 60f + index * 15f;
                Vector2 targetPos = owner.Center + orbitAngle.ToRotationVector2() * radius;
                Projectile.Center = Vector2.Lerp(Projectile.Center, targetPos, 0.15f);
                Projectile.velocity = Vector2.Zero;
                
                // Find target
                targetNPC = FindClosestNPCIndex(600f);
            }
            else
            {
                seeking = true;
                
                // Seek target
                if (targetNPC >= 0 && targetNPC < Main.maxNPCs && Main.npc[targetNPC].active)
                {
                    NPC target = Main.npc[targetNPC];
                    Vector2 direction = target.Center - Projectile.Center;
                    direction.Normalize();
                    
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * 16f, 0.12f);
                }
                else
                {
                    targetNPC = FindClosestNPCIndex(600f);
                    if (targetNPC < 0)
                    {
                        Projectile.velocity *= 0.95f;
                    }
                }
            }
            
            // ========================================
            // TRUE_VFX_STANDARDS: DENSE DUST TRAIL
            // 2+ particles per frame GUARANTEED
            // ========================================
            for (int i = 0; i < 2; i++)
            {
                Vector2 dustOffset = Main.rand.NextVector2Circular(8f, 8f);
                Vector2 dustVel = -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1.5f, 1.5f);
                Dust fire = Dust.NewDustPerfect(Projectile.Center + dustOffset, DustID.Torch, dustVel, 0, default, 1.6f);
                fire.noGravity = true;
                fire.fadeIn = 1.3f;
            }
            
            // Contrasting gold sparkle dust
            if (Main.rand.NextBool(2))
            {
                Dust gold = Dust.NewDustPerfect(Projectile.Center, DustID.GoldCoin, 
                    Main.rand.NextVector2Circular(2f, 2f), 0, default, 1.1f);
                gold.noGravity = true;
            }
            
            // ========================================
            // TRUE_VFX_STANDARDS: 3 ORBITING MUSIC NOTES
            // Locked to projectile, not random spawn!
            // ========================================
            float noteOrbitAngle = Main.GameUpdateCount * 0.08f;
            float noteRadius = 16f + (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 4f;
            
            if (Main.rand.NextBool(6))
            {
                for (int i = 0; i < 3; i++)
                {
                    float angle = noteOrbitAngle + MathHelper.TwoPi * i / 3f;
                    Vector2 noteOffset = angle.ToRotationVector2() * noteRadius;
                    Vector2 notePos = Projectile.Center + noteOffset;
                    Vector2 noteVel = Projectile.velocity * 0.7f + angle.ToRotationVector2() * 0.5f;
                    
                    // hslToRgb for color oscillation
                    float hue = HueMin + ((float)i / 3f) * (HueMax - HueMin);
                    Color noteColor = Main.hslToRgb(hue, 0.95f, 0.7f);
                    DiesIraeVFX.SpawnMusicNote(notePos, noteVel, noteColor, 0.8f);
                }
            }
            
            // ========================================
            // TRUE_VFX_STANDARDS: COLOR OSCILLATION
            // hslToRgb for dynamic hue shifting
            // ========================================
            if (Main.rand.NextBool(3))
            {
                float hue = HueMin + ((Main.GameUpdateCount * 0.02f) % 1f) * (HueMax - HueMin);
                Color shiftColor = Main.hslToRgb(hue, 0.9f, 0.7f);
                var sparkle = new SparkleParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(12f, 12f),
                    Main.rand.NextVector2Circular(1.5f, 1.5f),
                    shiftColor,
                    0.45f * pulse,
                    18
                );
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // ========================================
            // TRUE_VFX_STANDARDS: FLARES LITTERING THE AIR
            // ========================================
            if (Main.rand.NextBool(2))
            {
                Vector2 flarePos = Projectile.Center + Main.rand.NextVector2Circular(10f, 10f);
                float hue = HueMin + Main.rand.NextFloat() * (HueMax - HueMin);
                Color flareColor = Main.hslToRgb(hue, 0.95f, 0.75f);
                var flare = new BloomParticle(flarePos, Main.rand.NextVector2Circular(0.5f, 0.5f), flareColor * 0.7f, 0.35f, 15);
                MagnumParticleHandler.SpawnParticle(flare);
            }
            
            // Fire glow core
            var glow = new GenericGlowParticle(
                Projectile.Center,
                Vector2.Zero,
                IgniteFlame,
                0.35f * pulse,
                5,
                true
            );
            MagnumParticleHandler.SpawnParticle(glow);
            
            // Orbiting ember sparks (enhanced)
            if (Main.GameUpdateCount % 5 == 0)
            {
                int sparkCount = seeking ? 4 : 3;
                DiesIraeVFX.OrbitingSparks(Projectile.Center, DiesIraeColors.Crimson, 14f, sparkCount, orbitAngle, 0.35f * pulse);
            }
            
            // Spiral trail when seeking
            if (seeking && Projectile.velocity.Length() > 3f)
            {
                DiesIraeVFX.SpiralTrail(Projectile.Center, Projectile.velocity, DiesIraeColors.EmberOrange, 0.4f, Main.GameUpdateCount * 0.12f);
            }
            
            // Pulsing aura
            DiesIraeVFX.PulsingAura(Projectile.Center, IgniteFlame, 0.4f * pulse, orbitTimer);
            
            // Afterimage when moving fast
            if (seeking && Projectile.velocity.Length() > 8f)
            {
                DiesIraeVFX.AfterimageTrail(Projectile.Center, Projectile.velocity, 0.4f, IgniteBlood, 4);
            }
            
            // Dynamic lighting with intensity based on state
            float lightIntensity = seeking ? 0.8f : 0.6f;
            Lighting.AddLight(Projectile.Center, IgniteFlame.ToVector3() * lightIntensity * pulse);
        }
        
        private int FindClosestNPCIndex(float maxDistance)
        {
            int closest = -1;
            float closestDist = maxDistance;
            
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.CanBeChasedBy())
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = i;
                    }
                }
            }
            
            return closest;
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 300);
            
            // === TRUE_VFX_STANDARDS: 3-LAYER FLASH CASCADE ON HIT ===
            var whiteFlash = new BloomParticle(target.Center, Vector2.Zero, Color.White * 0.85f, 0.7f, 16);
            MagnumParticleHandler.SpawnParticle(whiteFlash);
            
            var goldFlash = new BloomParticle(target.Center, Vector2.Zero, IgniteFlame * 0.75f, 0.55f, 18);
            MagnumParticleHandler.SpawnParticle(goldFlash);
            
            var redFlash = new BloomParticle(target.Center, Vector2.Zero, IgniteBlood * 0.65f, 0.45f, 20);
            MagnumParticleHandler.SpawnParticle(redFlash);
            
            // Gradient music notes
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi * i / 5f;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 5f);
                float hue = HueMin + ((float)i / 5f) * (HueMax - HueMin);
                Color noteColor = Main.hslToRgb(hue, 0.95f, 0.7f);
                DiesIraeVFX.SpawnMusicNote(target.Center, noteVel, noteColor, 0.85f);
            }
            
            // Halo ring
            var halo = new BloomRingParticle(target.Center, Vector2.Zero, IgniteFlame * 0.6f, 0.4f, 18);
            MagnumParticleHandler.SpawnParticle(halo);
            
            // === DYNAMIC PARTICLE EFFECTS - Dies Hellfire Eruption (floating ignition) ===
            DiesHellfireEruption(target.Center, 0.95f);
            
            DiesIraeVFX.FireImpact(target.Center, 0.8f);
        }
        
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode, Projectile.Center);
            
            // === TRUE_VFX_STANDARDS: GLIMMER CASCADE (not puff!) ===
            
            // 4-layer glimmer cascade
            for (int layer = 0; layer < 4; layer++)
            {
                float layerScale = 0.35f + layer * 0.15f;
                float layerAlpha = 0.85f - layer * 0.15f;
                Color layerColor = Color.Lerp(Color.White, IgniteFlame, layer / 4f);
                layerColor.A = 0;
                
                var glimmer = new BloomParticle(Projectile.Center, Vector2.Zero, layerColor * layerAlpha, layerScale, 18 - layer * 2);
                MagnumParticleHandler.SpawnParticle(glimmer);
            }
            
            // 3 expanding halo rings
            for (int ring = 0; ring < 3; ring++)
            {
                Color ringColor = Color.Lerp(IgniteFlame, IgniteBlood, ring / 3f);
                ringColor.A = 0;
                var halo = new BloomRingParticle(Projectile.Center, Vector2.Zero, ringColor * 0.65f, 0.35f + ring * 0.12f, 16 + ring * 3);
                MagnumParticleHandler.SpawnParticle(halo);
            }
            
            // 8 music notes finale with hslToRgb gradient
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 5f);
                float hue = HueMin + ((float)i / 8f) * (HueMax - HueMin);
                Color noteColor = Main.hslToRgb(hue, 0.95f, 0.7f);
                DiesIraeVFX.SpawnMusicNote(Projectile.Center, noteVel, noteColor, 0.9f);
            }
            
            // Sparkle burst
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 7f);
                Color burstColor = Color.Lerp(IgniteCore, IgniteFlame, (float)i / 8f);
                var sparkle = new SparkleParticle(Projectile.Center, burstVel, burstColor, 0.4f, 22);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // Dust explosion
            for (int i = 0; i < 12; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(5f, 5f);
                Dust fire = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, dustVel, 0, default, 1.4f);
                fire.noGravity = true;
            }
            
            DiesIraeVFX.FireImpact(Projectile.Center, 1.0f);
            Lighting.AddLight(Projectile.Center, IgniteFlame.ToVector3() * 1.3f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Texture2D flare1 = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare").Value;
            Texture2D flare2 = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare4").Value;
            Texture2D softGlow = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow2").Value;
            
            Vector2 origin = texture.Size() / 2f;
            Vector2 flareOrigin1 = flare1.Size() / 2f;
            Vector2 flareOrigin2 = flare2.Size() / 2f;
            Vector2 glowOrigin = softGlow.Size() / 2f;
            
            int index = (int)Projectile.ai[1];
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float time = Main.GameUpdateCount * 0.05f;
            float pulse = 1f + (float)Math.Sin(time * 2f + index * 0.5f) * 0.2f;
            
            // ========================================
            // TRUE_VFX_STANDARDS: 6-LAYER SPINNING FLARES
            // ========================================
            
            // Layer 1: Soft glow base (large, charred)
            Color glowBlack = DiesIraeColors.CharredBlack * 0.35f;
            glowBlack.A = 0;
            Main.spriteBatch.Draw(softGlow, drawPos, null, glowBlack, 0f, glowOrigin, 0.8f * pulse, SpriteEffects.None, 0f);
            
            // Layer 2: Blood red flare spinning clockwise
            Color flareRed = IgniteBlood * 0.55f;
            flareRed.A = 0;
            Main.spriteBatch.Draw(flare1, drawPos, null, flareRed, time * 0.9f, flareOrigin1, 0.5f * pulse, SpriteEffects.None, 0f);
            
            // Layer 3: Ember orange flare spinning counter-clockwise
            Color flareOrange = DiesIraeColors.EmberOrange * 0.6f;
            flareOrange.A = 0;
            Main.spriteBatch.Draw(flare2, drawPos, null, flareOrange, -time * 0.7f, flareOrigin2, 0.45f * pulse, SpriteEffects.None, 0f);
            
            // Layer 4: Hellfire gold flare spinning faster
            Color flareGold = IgniteFlame * 0.65f;
            flareGold.A = 0;
            Main.spriteBatch.Draw(flare1, drawPos, null, flareGold, time * 1.1f, flareOrigin1, 0.38f * pulse, SpriteEffects.None, 0f);
            
            // Layer 5: Crimson core spinning opposite
            Color flareCrimson = DiesIraeColors.Crimson * 0.7f;
            flareCrimson.A = 0;
            Main.spriteBatch.Draw(flare2, drawPos, null, flareCrimson, -time * 0.85f, flareOrigin2, 0.3f * pulse, SpriteEffects.None, 0f);
            
            // Layer 6: White hot core
            Color coreWhite = Color.White * 0.8f;
            coreWhite.A = 0;
            Main.spriteBatch.Draw(flare1, drawPos, null, coreWhite, 0f, flareOrigin1, 0.2f * pulse, SpriteEffects.None, 0f);
            
            // === 4 ORBITING SPARK POINTS with hslToRgb ===
            float sparkOrbit = time * 1.5f;
            for (int i = 0; i < 4; i++)
            {
                float sparkAngle = sparkOrbit + MathHelper.TwoPi * i / 4f;
                Vector2 sparkOffset = sparkAngle.ToRotationVector2() * 12f * pulse;
                
                float hue = HueMin + ((float)i / 4f) * (HueMax - HueMin);
                Color sparkColor = Main.hslToRgb(hue, 0.95f, 0.75f) * 0.65f;
                sparkColor.A = 0;
                
                Main.spriteBatch.Draw(flare1, drawPos + sparkOffset, null, sparkColor, -sparkOrbit, flareOrigin1, 0.12f * pulse, SpriteEffects.None, 0f);
            }
            
            return false;
        }
    }
    
    #endregion
    
    #region Eclipse of Wrath Projectiles
    
    /// <summary>
    /// Throwable magic orb that tracks cursor and spawns shards
    /// </summary>
    public class EclipseOrb : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/TallFlamingWispProjectile";
        
        // === TRUE_VFX_STANDARDS: Hue range for Dies Irae ===
        private const float HueMin = 0.0f;
        private const float HueMax = 0.08f;
        
        private static readonly Color EclipseCore = new Color(255, 255, 220);
        private static readonly Color EclipseFlame = DiesIraeColors.EmberOrange;
        private static readonly Color EclipseBlood = DiesIraeColors.BloodRed;
        
        private int shardTimer = 0;
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
        }
        
        public override void AI()
        {
            // Track cursor
            if (Main.myPlayer == Projectile.owner)
            {
                Vector2 cursorPos = Main.MouseWorld;
                Vector2 direction = cursorPos - Projectile.Center;
                
                if (direction.Length() > 20f)
                {
                    direction.Normalize();
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * 14f, 0.08f);
                }
            }
            
            Projectile.rotation += 0.1f;
            
            shardTimer++;
            
            // Spawn tracking shards while airborne
            if (shardTimer % 20 == 0 && Main.myPlayer == Projectile.owner)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 spawnPos = Projectile.Center + angle.ToRotationVector2() * 20f;
                
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPos, Vector2.Zero,
                    ModContent.ProjectileType<WrathShard>(), Projectile.damage / 2, 0f, Projectile.owner);
            }
            
            // ========================================
            // TRUE_VFX_STANDARDS: DENSE DUST TRAIL
            // 2+ particles per frame GUARANTEED
            // ========================================
            for (int i = 0; i < 3; i++)
            {
                Vector2 dustOffset = Main.rand.NextVector2Circular(12f, 12f);
                Vector2 dustVel = -Projectile.velocity * 0.18f + Main.rand.NextVector2Circular(2f, 2f);
                Dust fire = Dust.NewDustPerfect(Projectile.Center + dustOffset, DustID.Torch, dustVel, 0, default, 1.8f);
                fire.noGravity = true;
                fire.fadeIn = 1.4f;
            }
            
            // Contrasting gold sparkle dust
            if (Main.rand.NextBool(2))
            {
                Dust gold = Dust.NewDustPerfect(Projectile.Center, DustID.GoldCoin, 
                    Main.rand.NextVector2Circular(2.5f, 2.5f), 0, default, 1.2f);
                gold.noGravity = true;
            }
            
            // ========================================
            // TRUE_VFX_STANDARDS: 4 ORBITING MUSIC NOTES
            // Locked to projectile in eclipse formation
            // ========================================
            float noteOrbitAngle = Main.GameUpdateCount * 0.07f;
            float noteRadius = 22f + (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 5f;
            
            if (Main.rand.NextBool(5))
            {
                for (int i = 0; i < 4; i++)
                {
                    float angle = noteOrbitAngle + MathHelper.TwoPi * i / 4f;
                    Vector2 noteOffset = angle.ToRotationVector2() * noteRadius;
                    Vector2 notePos = Projectile.Center + noteOffset;
                    Vector2 noteVel = Projectile.velocity * 0.7f + angle.ToRotationVector2() * 0.6f;
                    
                    float hue = HueMin + ((float)i / 4f) * (HueMax - HueMin);
                    Color noteColor = Main.hslToRgb(hue, 0.95f, 0.7f);
                    DiesIraeVFX.SpawnMusicNote(notePos, noteVel, noteColor, 0.9f);
                }
            }
            
            // ========================================
            // TRUE_VFX_STANDARDS: 5-POINT CONSTELLATION ORBIT
            // ========================================
            float constOrbit = Main.GameUpdateCount * 0.06f;
            if (Main.GameUpdateCount % 8 == 0)
            {
                for (int i = 0; i < 5; i++)
                {
                    float angle = constOrbit + MathHelper.TwoPi * i / 5f;
                    Vector2 constPos = Projectile.Center + angle.ToRotationVector2() * 18f;
                    float hue = HueMin + ((float)i / 5f) * (HueMax - HueMin);
                    Color constColor = Main.hslToRgb(hue, 0.9f, 0.8f);
                    var sparkle = new SparkleParticle(constPos, Vector2.Zero, constColor, 0.35f, 15);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
            }
            
            // ========================================
            // TRUE_VFX_STANDARDS: FLARES LITTERING THE AIR
            // ========================================
            if (Main.rand.NextBool(2))
            {
                Vector2 flarePos = Projectile.Center + Main.rand.NextVector2Circular(15f, 15f);
                float hue = HueMin + Main.rand.NextFloat() * (HueMax - HueMin);
                Color flareColor = Main.hslToRgb(hue, 0.95f, 0.75f);
                var flare = new BloomParticle(flarePos, -Projectile.velocity * 0.08f, flareColor * 0.7f, 0.4f, 16);
                MagnumParticleHandler.SpawnParticle(flare);
            }
            
            // Heavy fire trail (core effect)
            for (int i = 0; i < 2; i++)
            {
                DiesIraeVFX.FireTrail(Projectile.Center + Main.rand.NextVector2Circular(10f, 10f), Projectile.velocity, 1f);
            }
            
            // Afterimage trail for motion blur
            if (Projectile.velocity.Length() > 3f)
            {
                DiesIraeVFX.AfterimageTrail(Projectile.Center, Projectile.velocity, 0.55f, EclipseBlood, 5);
            }
            
            // Orbiting ember sparks
            if (Main.GameUpdateCount % 4 == 0)
            {
                DiesIraeVFX.OrbitingSparks(Projectile.Center, DiesIraeColors.Crimson, 18f, 5, Projectile.rotation * 0.5f, 0.4f);
            }
            
            // Pulsing dark aura
            float eclipsePulse = 1f + (float)Math.Sin(shardTimer * 0.08f) * 0.25f;
            DiesIraeVFX.PulsingAura(Projectile.Center, DiesIraeColors.CharredBlack, 0.65f * eclipsePulse, shardTimer);
            
            // Spiral eclipse pattern
            if (Main.rand.NextBool(2))
            {
                DiesIraeVFX.SpiralTrail(Projectile.Center, Projectile.velocity, EclipseFlame, 0.5f, Projectile.rotation);
            }
            
            // Flame wisp bursts (periodic)
            if (shardTimer % 15 == 0)
            {
                DiesIraeVFX.FlameWispBurst(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f), 0.45f, 3);
            }
            
            // Dynamic lighting with eclipse-style pulse
            float lightIntensity = 0.8f + (float)Math.Sin(shardTimer * 0.1f) * 0.2f;
            Lighting.AddLight(Projectile.Center, EclipseFlame.ToVector3() * lightIntensity);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 300);
            Explode();
        }
        
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Explode();
            return true;
        }
        
        private void Explode()
        {
            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact, Projectile.Center);
            
            // === TRUE_VFX_STANDARDS: MASSIVE GLIMMER CASCADE ===
            
            // 5-layer glimmer cascade
            for (int layer = 0; layer < 5; layer++)
            {
                float layerScale = 0.5f + layer * 0.2f;
                float layerAlpha = 0.9f - layer * 0.15f;
                Color layerColor = Color.Lerp(Color.White, EclipseFlame, layer / 5f);
                layerColor.A = 0;
                
                var glimmer = new BloomParticle(Projectile.Center, Vector2.Zero, layerColor * layerAlpha, layerScale, 22 - layer * 2);
                MagnumParticleHandler.SpawnParticle(glimmer);
            }
            
            // 4 expanding halo rings with gradient
            for (int ring = 0; ring < 4; ring++)
            {
                Color ringColor = Color.Lerp(EclipseFlame, EclipseBlood, ring / 4f);
                ringColor.A = 0;
                var halo = new BloomRingParticle(Projectile.Center, Vector2.Zero, ringColor * 0.7f, 0.4f + ring * 0.18f, 18 + ring * 4);
                MagnumParticleHandler.SpawnParticle(halo);
            }
            
            // 10 music notes finale with hslToRgb gradient
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 7f);
                float hue = HueMin + ((float)i / 10f) * (HueMax - HueMin);
                Color noteColor = Main.hslToRgb(hue, 0.95f, 0.7f);
                DiesIraeVFX.SpawnMusicNote(Projectile.Center, noteVel, noteColor, 1f);
            }
            
            // Sparkle + glow burst
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 9f);
                Color burstColor = Color.Lerp(EclipseCore, EclipseFlame, (float)i / 12f);
                
                var sparkle = new SparkleParticle(Projectile.Center, burstVel, burstColor, 0.5f, 25);
                MagnumParticleHandler.SpawnParticle(sparkle);
                
                var glow = new GenericGlowParticle(Projectile.Center, burstVel * 0.7f, burstColor * 0.75f, 0.4f, 22, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Heavy dust explosion
            for (int i = 0; i < 20; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(8f, 8f);
                Dust fire = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, dustVel, 0, default, 1.6f);
                fire.noGravity = true;
            }
            
            DiesIraeVFX.FireImpact(Projectile.Center, 1.5f);
            Lighting.AddLight(Projectile.Center, EclipseFlame.ToVector3() * 1.8f);
            
            Projectile.Kill();
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Texture2D flare1 = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare").Value;
            Texture2D flare2 = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare4").Value;
            Texture2D softGlow = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow2").Value;
            
            Vector2 origin = texture.Size() / 2f;
            Vector2 flareOrigin1 = flare1.Size() / 2f;
            Vector2 flareOrigin2 = flare2.Size() / 2f;
            Vector2 glowOrigin = softGlow.Size() / 2f;
            
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float time = Main.GameUpdateCount * 0.05f;
            float pulse = 1f + (float)Math.Sin(time * 2f) * 0.18f;
            
            // ========================================
            // TRUE_VFX_STANDARDS: TRAIL RENDERING with hslToRgb
            // ========================================
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                
                float progress = i / (float)Projectile.oldPos.Length;
                float hue = HueMin + progress * (HueMax - HueMin);
                Color trailColor = Main.hslToRgb(hue, 0.9f, 0.65f) * (1f - progress) * 0.65f;
                trailColor.A = 0;
                
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Main.spriteBatch.Draw(texture, trailPos, null, trailColor, Projectile.oldRot[i], origin, Projectile.scale * (1f - progress * 0.35f), SpriteEffects.None, 0f);
            }
            
            // ========================================
            // TRUE_VFX_STANDARDS: 6-LAYER SPINNING FLARES
            // ========================================
            
            // Layer 1: Soft glow base (large, charred black)
            Color glowBlack = DiesIraeColors.CharredBlack * 0.4f;
            glowBlack.A = 0;
            Main.spriteBatch.Draw(softGlow, drawPos, null, glowBlack, 0f, glowOrigin, 1.0f * pulse, SpriteEffects.None, 0f);
            
            // Layer 2: Blood red flare spinning clockwise
            Color flareRed = EclipseBlood * 0.6f;
            flareRed.A = 0;
            Main.spriteBatch.Draw(flare1, drawPos, null, flareRed, time * 0.85f + Projectile.rotation, flareOrigin1, 0.6f * pulse, SpriteEffects.None, 0f);
            
            // Layer 3: Ember orange flare spinning counter-clockwise
            Color flareOrange = EclipseFlame * 0.65f;
            flareOrange.A = 0;
            Main.spriteBatch.Draw(flare2, drawPos, null, flareOrange, -time * 0.7f + Projectile.rotation, flareOrigin2, 0.52f * pulse, SpriteEffects.None, 0f);
            
            // Layer 4: Hellfire gold flare spinning faster
            Color flareGold = DiesIraeColors.HellfireGold * 0.7f;
            flareGold.A = 0;
            Main.spriteBatch.Draw(flare1, drawPos, null, flareGold, time * 1.15f + Projectile.rotation, flareOrigin1, 0.45f * pulse, SpriteEffects.None, 0f);
            
            // Layer 5: Crimson core spinning opposite
            Color flareCrimson = DiesIraeColors.Crimson * 0.75f;
            flareCrimson.A = 0;
            Main.spriteBatch.Draw(flare2, drawPos, null, flareCrimson, -time * 0.9f + Projectile.rotation, flareOrigin2, 0.38f * pulse, SpriteEffects.None, 0f);
            
            // Layer 6: Bright white hot core
            Color coreWhite = Color.White * 0.85f;
            coreWhite.A = 0;
            Main.spriteBatch.Draw(flare1, drawPos, null, coreWhite, Projectile.rotation, flareOrigin1, 0.25f * pulse, SpriteEffects.None, 0f);
            
            // === 5 ORBITING SPARK POINTS with hslToRgb ===
            float sparkOrbit = time * 1.4f;
            for (int i = 0; i < 5; i++)
            {
                float sparkAngle = sparkOrbit + MathHelper.TwoPi * i / 5f;
                Vector2 sparkOffset = sparkAngle.ToRotationVector2() * 15f * pulse;
                
                float hue = HueMin + ((float)i / 5f) * (HueMax - HueMin);
                Color sparkColor = Main.hslToRgb(hue, 0.95f, 0.75f) * 0.65f;
                sparkColor.A = 0;
                
                Main.spriteBatch.Draw(flare1, drawPos + sparkOffset, null, sparkColor, -sparkOrbit, flareOrigin1, 0.14f * pulse, SpriteEffects.None, 0f);
            }
            
            return false;
        }
    }
    
    /// <summary>
    /// Small tracking shard spawned by Eclipse Orb
    /// </summary>
    public class WrathShard : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/FlamingWispProjectileSmall";
        
        // === TRUE_VFX_STANDARDS: Hue range for Dies Irae ===
        private const float HueMin = 0.0f;
        private const float HueMax = 0.08f;
        
        private static readonly Color ShardCore = new Color(255, 255, 220);
        private static readonly Color ShardFlame = DiesIraeColors.EmberOrange;
        private static readonly Color ShardBlood = DiesIraeColors.Crimson;
        
        private int targetNPC = -1;
        private int delay = 15;
        
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }
        
        public override void AI()
        {
            delay--;
            
            if (delay > 0)
            {
                // Hover in place briefly - still spawn visuals
                Projectile.velocity *= 0.9f;
                
                // Charging visuals during delay
                if (Main.rand.NextBool(2))
                {
                    float hue = HueMin + Main.rand.NextFloat() * (HueMax - HueMin);
                    Color chargeColor = Main.hslToRgb(hue, 0.9f, 0.7f);
                    var glow = new GenericGlowParticle(Projectile.Center, Main.rand.NextVector2Circular(1f, 1f), chargeColor * 0.6f, 0.25f, 12, true);
                    MagnumParticleHandler.SpawnParticle(glow);
                }
                return;
            }
            
            // Find and track target
            if (targetNPC < 0 || !Main.npc[targetNPC].active)
            {
                targetNPC = FindClosestNPCIndex(500f);
            }
            
            if (targetNPC >= 0 && Main.npc[targetNPC].active)
            {
                NPC target = Main.npc[targetNPC];
                Vector2 direction = target.Center - Projectile.Center;
                direction.Normalize();
                
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * 18f, 0.15f);
            }
            
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // ========================================
            // TRUE_VFX_STANDARDS: DENSE DUST TRAIL
            // 2+ particles per frame GUARANTEED
            // ========================================
            for (int i = 0; i < 2; i++)
            {
                Vector2 dustOffset = Main.rand.NextVector2Circular(4f, 4f);
                Vector2 dustVel = -Projectile.velocity * 0.12f + Main.rand.NextVector2Circular(1f, 1f);
                Dust fire = Dust.NewDustPerfect(Projectile.Center + dustOffset, DustID.Torch, dustVel, 0, default, 1.3f);
                fire.noGravity = true;
                fire.fadeIn = 1.1f;
            }
            
            // Gold sparkle dust
            if (Main.rand.NextBool(2))
            {
                Dust gold = Dust.NewDustPerfect(Projectile.Center, DustID.GoldCoin, 
                    -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f), 0, default, 0.9f);
                gold.noGravity = true;
            }
            
            // ========================================
            // TRUE_VFX_STANDARDS: 2 ORBITING MUSIC NOTES
            // Locked to projectile, compact for small shard
            // ========================================
            float noteOrbitAngle = Main.GameUpdateCount * 0.1f;
            float noteRadius = 8f + (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 2f;
            
            if (Main.rand.NextBool(8))
            {
                for (int i = 0; i < 2; i++)
                {
                    float angle = noteOrbitAngle + MathHelper.Pi * i;
                    Vector2 noteOffset = angle.ToRotationVector2() * noteRadius;
                    Vector2 notePos = Projectile.Center + noteOffset;
                    Vector2 noteVel = Projectile.velocity * 0.6f + angle.ToRotationVector2() * 0.3f;
                    
                    float hue = HueMin + ((float)i / 2f) * (HueMax - HueMin);
                    Color noteColor = Main.hslToRgb(hue, 0.95f, 0.7f);
                    DiesIraeVFX.SpawnMusicNote(notePos, noteVel, noteColor, 0.7f);
                }
            }
            
            // ========================================
            // TRUE_VFX_STANDARDS: FLARES LITTERING THE AIR
            // ========================================
            if (Main.rand.NextBool(2))
            {
                Vector2 flarePos = Projectile.Center + Main.rand.NextVector2Circular(6f, 6f);
                float hue = HueMin + Main.rand.NextFloat() * (HueMax - HueMin);
                Color flareColor = Main.hslToRgb(hue, 0.95f, 0.75f);
                var flare = new BloomParticle(flarePos, -Projectile.velocity * 0.05f, flareColor * 0.6f, 0.25f, 12);
                MagnumParticleHandler.SpawnParticle(flare);
            }
            
            // Core fire trail
            DiesIraeVFX.FireTrail(Projectile.Center, Projectile.velocity, 0.55f);
            
            // Afterimage trail when seeking
            if (Projectile.velocity.Length() > 6f)
            {
                DiesIraeVFX.AfterimageTrail(Projectile.Center, Projectile.velocity, 0.35f, ShardBlood, 3);
            }
            
            // Spiral trail
            if (Main.rand.NextBool(2))
            {
                DiesIraeVFX.SpiralTrail(Projectile.Center, Projectile.velocity, ShardFlame, 0.3f, Main.GameUpdateCount * 0.15f);
            }
            
            // Small orbiting embers
            if (Main.GameUpdateCount % 6 == 0)
            {
                DiesIraeVFX.OrbitingSparks(Projectile.Center, DiesIraeColors.BloodRed, 8f, 2, Main.GameUpdateCount * 0.12f, 0.18f);
            }
            
            Lighting.AddLight(Projectile.Center, ShardFlame.ToVector3() * 0.4f);
        }
        
        private int FindClosestNPCIndex(float maxDistance)
        {
            int closest = -1;
            float closestDist = maxDistance;
            
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.CanBeChasedBy())
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = i;
                    }
                }
            }
            
            return closest;
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 180);
            
            // === TRUE_VFX_STANDARDS: 2-LAYER FLASH ON HIT (compact for small shard) ===
            var whiteFlash = new BloomParticle(target.Center, Vector2.Zero, Color.White * 0.75f, 0.5f, 14);
            MagnumParticleHandler.SpawnParticle(whiteFlash);
            
            var flameFlash = new BloomParticle(target.Center, Vector2.Zero, ShardFlame * 0.65f, 0.4f, 16);
            MagnumParticleHandler.SpawnParticle(flameFlash);
            
            // 3 music notes
            for (int i = 0; i < 3; i++)
            {
                float angle = MathHelper.TwoPi * i / 3f;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                float hue = HueMin + ((float)i / 3f) * (HueMax - HueMin);
                Color noteColor = Main.hslToRgb(hue, 0.95f, 0.7f);
                DiesIraeVFX.SpawnMusicNote(target.Center, noteVel, noteColor, 0.75f);
            }
            
            // === DYNAMIC PARTICLE EFFECTS - Dies Wrath Chain Lightning (wrath shard) ===
            DiesWrathChainLightning(target.Center, Projectile.velocity.SafeNormalize(Vector2.UnitX), 0.6f);
            
            DiesIraeVFX.FireImpact(target.Center, 0.5f);
        }
        
        public override void OnKill(int timeLeft)
        {
            // === TRUE_VFX_STANDARDS: COMPACT GLIMMER CASCADE ===
            
            // 3-layer glimmer
            for (int layer = 0; layer < 3; layer++)
            {
                float layerScale = 0.25f + layer * 0.1f;
                float layerAlpha = 0.8f - layer * 0.2f;
                Color layerColor = Color.Lerp(Color.White, ShardFlame, layer / 3f);
                layerColor.A = 0;
                
                var glimmer = new BloomParticle(Projectile.Center, Vector2.Zero, layerColor * layerAlpha, layerScale, 15 - layer * 2);
                MagnumParticleHandler.SpawnParticle(glimmer);
            }
            
            // 2 halo rings
            var halo1 = new BloomRingParticle(Projectile.Center, Vector2.Zero, ShardFlame * 0.6f, 0.3f, 14);
            MagnumParticleHandler.SpawnParticle(halo1);
            var halo2 = new BloomRingParticle(Projectile.Center, Vector2.Zero, ShardBlood * 0.5f, 0.4f, 18);
            MagnumParticleHandler.SpawnParticle(halo2);
            
            // 4 music notes finale
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                float hue = HueMin + ((float)i / 4f) * (HueMax - HueMin);
                Color noteColor = Main.hslToRgb(hue, 0.95f, 0.7f);
                DiesIraeVFX.SpawnMusicNote(Projectile.Center, noteVel, noteColor, 0.8f);
            }
            
            // Sparkle burst
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 5f);
                Color burstColor = Color.Lerp(ShardCore, ShardFlame, (float)i / 6f);
                var sparkle = new SparkleParticle(Projectile.Center, burstVel, burstColor, 0.3f, 18);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // Dust burst
            for (int i = 0; i < 8; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(4f, 4f);
                Dust fire = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, dustVel, 0, default, 1.2f);
                fire.noGravity = true;
            }
            
            DiesIraeVFX.FireImpact(Projectile.Center, 0.55f);
            Lighting.AddLight(Projectile.Center, ShardFlame.ToVector3() * 1.0f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Texture2D flare1 = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare").Value;
            Texture2D flare2 = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare4").Value;
            
            Vector2 origin = texture.Size() / 2f;
            Vector2 flareOrigin1 = flare1.Size() / 2f;
            Vector2 flareOrigin2 = flare2.Size() / 2f;
            
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float time = Main.GameUpdateCount * 0.06f;
            float pulse = 1f + (float)Math.Sin(time * 2f) * 0.15f;
            
            // ========================================
            // TRUE_VFX_STANDARDS: 4-LAYER SPINNING FLARES (compact)
            // ========================================
            
            // Layer 1: Blood red flare spinning
            Color flareRed = ShardBlood * 0.5f;
            flareRed.A = 0;
            Main.spriteBatch.Draw(flare1, drawPos, null, flareRed, time * 0.9f, flareOrigin1, 0.35f * pulse, SpriteEffects.None, 0f);
            
            // Layer 2: Orange flare counter-spin
            Color flareOrange = ShardFlame * 0.55f;
            flareOrange.A = 0;
            Main.spriteBatch.Draw(flare2, drawPos, null, flareOrange, -time * 0.75f, flareOrigin2, 0.3f * pulse, SpriteEffects.None, 0f);
            
            // Layer 3: Crimson mid
            Color flareCrimson = DiesIraeColors.Crimson * 0.6f;
            flareCrimson.A = 0;
            Main.spriteBatch.Draw(flare1, drawPos, null, flareCrimson, time * 1.1f, flareOrigin1, 0.25f * pulse, SpriteEffects.None, 0f);
            
            // Layer 4: White core
            Color coreWhite = Color.White * 0.75f;
            coreWhite.A = 0;
            Main.spriteBatch.Draw(flare2, drawPos, null, coreWhite, 0f, flareOrigin2, 0.15f * pulse, SpriteEffects.None, 0f);
            
            // === 2 ORBITING SPARK POINTS ===
            float sparkOrbit = time * 1.6f;
            for (int i = 0; i < 2; i++)
            {
                float sparkAngle = sparkOrbit + MathHelper.Pi * i;
                Vector2 sparkOffset = sparkAngle.ToRotationVector2() * 7f * pulse;
                
                float hue = HueMin + ((float)i / 2f) * (HueMax - HueMin);
                Color sparkColor = Main.hslToRgb(hue, 0.95f, 0.75f) * 0.6f;
                sparkColor.A = 0;
                
                Main.spriteBatch.Draw(flare1, drawPos + sparkOffset, null, sparkColor, -sparkOrbit, flareOrigin1, 0.08f * pulse, SpriteEffects.None, 0f);
            }
            
            return false;
        }
    }
    
    #endregion
}
