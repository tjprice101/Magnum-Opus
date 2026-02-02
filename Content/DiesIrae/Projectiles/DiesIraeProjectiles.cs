using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.GameContent;
using MagnumOpus.Common.Systems.Particles;

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
            
            int variant = Main.rand.Next(1, 7);
            
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
        /// Bright white core → Dark red fire → Black smoke
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
            
            // === EXPANDING HALO RINGS - White → Red → Black gradient ===
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
            // Converging particles - White → Red
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
    }
    
    #endregion
    
    #region Wrath's Cleaver Projectiles
    
    /// <summary>
    /// Blazing cascading wave of wrathful energy from Wrath's Cleaver
    /// </summary>
    public class WrathWaveProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/TallFlamingWispProjectile";
        
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
            
            // === ENHANCED LAYERED VFX ===
            // Layer 1: Heavy fire trail with afterimages
            DiesIraeVFX.FireTrail(Projectile.Center, Projectile.velocity, 1.2f);
            DiesIraeVFX.AfterimageTrail(Projectile.Center, Projectile.velocity, Projectile.scale);
            
            // Layer 2: Orbiting spark points - 4 points rotating
            if (Projectile.timeLeft % 3 == 0)
            {
                DiesIraeVFX.OrbitingSparks(Projectile.Center, 25f * Projectile.scale, 4, 0.15f, 0.3f);
            }
            
            // Layer 3: Pulsing aura around the wave
            DiesIraeVFX.PulsingAura(Projectile.Center, 20f * Projectile.scale, 1f);
            
            // Layer 4: Spiral trail effect for dynamic rotation feel
            if (Projectile.timeLeft % 2 == 0)
            {
                DiesIraeVFX.SpiralTrail(Projectile.Center, Projectile.velocity, Projectile.rotation, Projectile.scale * 0.8f);
            }
            
            // Layer 5: Music notes scattered in wake
            if (Main.rand.NextBool(5))
            {
                DiesIraeVFX.SpawnMusicNote(Projectile.Center + Main.rand.NextVector2Circular(15f, 15f), 
                    -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(2f, 2f), 
                    Color.Lerp(Color.White, DiesIraeColors.HellfireGold, Main.rand.NextFloat()), 0.85f);
            }
            
            // Enhanced lighting with color gradient
            float lightPulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.2f) * 0.2f;
            Lighting.AddLight(Projectile.Center, DiesIraeColors.EmberOrange.ToVector3() * lightPulse * 0.9f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 180);
            DiesIraeVFX.FireImpact(target.Center, 0.8f);
        }
        
        public override void OnKill(int timeLeft)
        {
            DiesIraeVFX.FireImpact(Projectile.Center, 1f);
            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact, Projectile.Center);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = texture.Size() / 2f;
            
            // Draw trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                
                float progress = i / (float)Projectile.oldPos.Length;
                Color trailColor = DiesIraeColors.GetGradient(progress) * (1f - progress) * 0.6f;
                trailColor.A = 0;
                float trailScale = Projectile.scale * (1f - progress * 0.5f);
                
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Main.spriteBatch.Draw(texture, drawPos, null, trailColor, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0f);
            }
            
            // Draw main with bloom
            for (int bloom = 0; bloom < 3; bloom++)
            {
                float bloomScale = Projectile.scale * (1f + bloom * 0.2f);
                float bloomAlpha = 0.5f / (bloom + 1);
                Color bloomColor = DiesIraeColors.EmberOrange * bloomAlpha;
                bloomColor.A = 0;
                
                Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, bloomColor, Projectile.rotation, origin, bloomScale, SpriteEffects.None, 0f);
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
        
        private float homingStrength = 0f;
        
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
            
            // === ENHANCED LAYERED CRYSTAL FLAME VFX ===
            // Layer 1: Spiral trail effect - rotating crystal flames
            DiesIraeVFX.SpiralTrail(Projectile.Center, Projectile.velocity, Projectile.rotation * 2f, 0.8f);
            
            // Layer 2: Fire trail with afterimages
            DiesIraeVFX.FireTrail(Projectile.Center, Projectile.velocity, 0.9f);
            DiesIraeVFX.AfterimageTrail(Projectile.Center, Projectile.velocity, 0.8f);
            
            // Layer 3: Orbiting crystal sparks - 3 points, slow rotation
            if (Projectile.timeLeft % 4 == 0)
            {
                DiesIraeVFX.OrbitingSparks(Projectile.Center, 12f, 3, 0.1f, 0.2f);
            }
            
            // Layer 4: Pulsing crystal aura
            DiesIraeVFX.PulsingAura(Projectile.Center, 15f, 0.8f);
            
            // Layer 5: Crystal fire trail particles
            if (Main.rand.NextBool(2))
            {
                Color trailColor = Main.rand.NextBool() ? DiesIraeColors.Crimson : DiesIraeColors.HellfireGold;
                var crystal = new GenericGlowParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                    -Projectile.velocity * 0.1f,
                    trailColor,
                    0.35f,
                    20,
                    true
                );
                MagnumParticleHandler.SpawnParticle(crystal);
            }
            
            // Layer 6: Music note trail
            if (Main.rand.NextBool(8))
            {
                DiesIraeVFX.SpawnMusicNote(Projectile.Center, -Projectile.velocity * 0.05f, DiesIraeColors.EmberOrange, 0.8f);
            }
            
            // Layer 7: Vanilla dust for density
            Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, -Projectile.velocity * 0.2f, 0, default, 1.3f);
            dust.noGravity = true;
            
            // Electric sparks occasionally
            if (Main.rand.NextBool(6))
            {
                Dust spark = Dust.NewDustPerfect(Projectile.Center, DustID.Electric, Main.rand.NextVector2Circular(3f, 3f), 0, DiesIraeColors.HellfireGold, 0.8f);
                spark.noGravity = true;
            }
            
            // Dynamic lighting
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.18f) * 0.15f;
            Lighting.AddLight(Projectile.Center, DiesIraeColors.Crimson.ToVector3() * 0.7f * pulse);
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
        }
        
        public override void OnKill(int timeLeft)
        {
            DiesIraeVFX.FireImpact(Projectile.Center, 1.2f);
            SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode, Projectile.Center);
            
            // Explosion damage
            Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, Vector2.Zero,
                ProjectileID.DD2ExplosiveTrapT3Explosion, Projectile.damage / 2, 0f, Projectile.owner);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = texture.Size() / 2f;
            
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.2f) * 0.15f;
            
            // Bloom layers
            for (int i = 0; i < 4; i++)
            {
                float scale = pulse * (0.8f + i * 0.2f);
                float alpha = 0.4f / (i + 1);
                Color color = i < 2 ? DiesIraeColors.HellfireGold : DiesIraeColors.Crimson;
                color.A = 0;
                color *= alpha;
                
                Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, color, Projectile.rotation, origin, scale, SpriteEffects.None, 0f);
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
        
        private int targetNPC = -1;
        private bool hasExploded = false;
        
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
            
            // === ENHANCED LAYERED BOLT VFX ===
            // Layer 1: Fire trail with afterimages
            DiesIraeVFX.FireTrail(Projectile.Center, Projectile.velocity, 0.9f);
            DiesIraeVFX.AfterimageTrail(Projectile.Center, Projectile.velocity, 0.7f);
            
            // Layer 2: Orbiting sparks showing tracking energy
            if (Projectile.timeLeft % 5 == 0)
            {
                DiesIraeVFX.OrbitingSparks(Projectile.Center, 10f, 3, 0.12f, 0.2f);
            }
            
            // Layer 3: Pulsing aura when targeting
            if (targetNPC >= 0)
            {
                DiesIraeVFX.PulsingAura(Projectile.Center, 12f, 0.7f);
            }
            
            // Layer 4: Music notes occasionally
            if (Main.rand.NextBool(12))
            {
                DiesIraeVFX.SpawnMusicNote(Projectile.Center, -Projectile.velocity * 0.05f, DiesIraeColors.HellfireGold, 0.75f);
            }
            
            // Layer 5: Electric dust for tracking feel
            if (Main.rand.NextBool(4))
            {
                Dust spark = Dust.NewDustPerfect(Projectile.Center, DustID.Electric, Main.rand.NextVector2Circular(2f, 2f), 0, DiesIraeColors.HellfireGold, 0.7f);
                spark.noGravity = true;
            }
            
            // Dynamic pulsing light
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.2f) * 0.15f;
            Lighting.AddLight(Projectile.Center, DiesIraeColors.EmberOrange.ToVector3() * 0.6f * pulse);
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
            
            DiesIraeVFX.FireImpact(Projectile.Center, 1f);
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
            Vector2 origin = texture.Size() / 2f;
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.1f;
            
            // === LAYER 1: Outer dark/black glow ===
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = (MathHelper.TwoPi * i / 4f + Main.GameUpdateCount * 0.03f).ToRotationVector2() * 5f * pulse;
                Color blackGlow = DiesIraeColors.CharredBlack * 0.3f;
                blackGlow.A = 0;
                Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition + offset, null, blackGlow, Projectile.rotation, origin, 1.3f, SpriteEffects.None, 0f);
            }
            
            // === LAYER 2: Blood red middle glow ===
            for (int i = 0; i < 5; i++)
            {
                float scale = 1.1f + i * 0.08f;
                float alpha = 0.4f / (i + 1);
                Color redGlow = DiesIraeColors.BloodRed * alpha;
                redGlow.A = 0;
                Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, redGlow, Projectile.rotation, origin, scale * pulse, SpriteEffects.None, 0f);
            }
            
            // === LAYER 3: Ember orange accent ===
            for (int i = 0; i < 3; i++)
            {
                Vector2 offset = (MathHelper.TwoPi * i / 3f - Main.GameUpdateCount * 0.02f).ToRotationVector2() * 3f;
                Color emberGlow = DiesIraeColors.EmberOrange * 0.35f;
                emberGlow.A = 0;
                Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition + offset, null, emberGlow, Projectile.rotation, origin, 1.05f * pulse, SpriteEffects.None, 0f);
            }
            
            // === LAYER 4: Bright white core ===
            Color whiteCore = Color.White * 0.5f;
            whiteCore.A = 0;
            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, whiteCore, Projectile.rotation, origin, 0.9f * pulse, SpriteEffects.None, 0f);
            
            return false;
        }
    }
    
    /// <summary>
    /// Spectral sword that strikes the target after bolt explosion
    /// </summary>
    public class SpectralVerdictSword : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/DiesIrae/ResonantWeapons/ExecutionersVerdict";
        
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
            
            // Spectral fire trail
            for (int i = 0; i < 2; i++)
            {
                Color trailColor = Main.rand.NextBool() ? DiesIraeColors.BloodRed : DiesIraeColors.EmberOrange;
                trailColor *= 0.7f;
                
                var trail = new GenericGlowParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                    -Projectile.velocity * 0.2f,
                    trailColor,
                    0.4f,
                    15,
                    true
                );
                MagnumParticleHandler.SpawnParticle(trail);
            }
            
            // Music note
            if (Main.rand.NextBool(8))
            {
                DiesIraeVFX.SpawnMusicNote(Projectile.Center, Vector2.Zero, DiesIraeColors.HellfireGold, 0.8f);
            }
            
            Lighting.AddLight(Projectile.Center, DiesIraeColors.BloodRed.ToVector3() * 0.6f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 180);
        }
        
        public override void OnKill(int timeLeft)
        {
            DiesIraeVFX.FireImpact(Projectile.Center, 0.8f);
            SoundEngine.PlaySound(SoundID.Item60, Projectile.Center);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = texture.Size() / 2f;
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.18f) * 0.12f;
            
            // === SPECTRAL GHOST LAYERS - White hot core fading to crimson to black ===
            
            // Layer 1: Outer black ghost
            for (int i = 0; i < 3; i++)
            {
                float offset = (float)Math.Sin(Main.GameUpdateCount * 0.08f + i * 0.7f) * 6f;
                Vector2 drawPos = Projectile.Center - Main.screenPosition + new Vector2(offset, -offset * 0.7f);
                Color blackGhost = DiesIraeColors.CharredBlack * 0.25f;
                blackGhost.A = 0;
                Main.spriteBatch.Draw(texture, drawPos, null, blackGhost, Projectile.rotation, origin, Projectile.scale * 1.2f, SpriteEffects.None, 0f);
            }
            
            // Layer 2: Blood red spectral glow
            for (int i = 0; i < 4; i++)
            {
                float offset = (float)Math.Sin(Main.GameUpdateCount * 0.1f + i * 0.5f) * 4f;
                Vector2 drawPos = Projectile.Center - Main.screenPosition + new Vector2(offset, -offset);
                Color spectralColor = Color.Lerp(DiesIraeColors.BloodRed, DiesIraeColors.Crimson, i / 4f) * (0.35f - i * 0.05f);
                spectralColor.A = 0;
                Main.spriteBatch.Draw(texture, drawPos, null, spectralColor, Projectile.rotation, origin, Projectile.scale * (1.1f - i * 0.05f), SpriteEffects.None, 0f);
            }
            
            // Layer 3: Orange ember accent
            Color emberAccent = DiesIraeColors.EmberOrange * 0.4f;
            emberAccent.A = 0;
            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, emberAccent, Projectile.rotation, origin, Projectile.scale * pulse, SpriteEffects.None, 0f);
            
            // Layer 4: White-hot core
            Color whiteCore = Color.White * 0.55f;
            whiteCore.A = 0;
            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, whiteCore, Projectile.rotation, origin, Projectile.scale * 0.85f * pulse, SpriteEffects.None, 0f);
            
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
        
        private int bounceCount = 0;
        private const int MaxBounces = 4;
        private List<int> hitNPCs = new List<int>();
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
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
            
            // Fire trail with spinning effect
            float trailAngle = Projectile.rotation;
            for (int i = 0; i < 2; i++)
            {
                float angle = trailAngle + MathHelper.Pi * i;
                Vector2 trailPos = Projectile.Center + angle.ToRotationVector2() * 20f;
                
                var trail = new GenericGlowParticle(
                    trailPos,
                    -Projectile.velocity * 0.1f,
                    DiesIraeColors.GetGradient(Main.rand.NextFloat()),
                    0.35f,
                    15,
                    true
                );
                MagnumParticleHandler.SpawnParticle(trail);
            }
            
            // Central fire
            DiesIraeVFX.FireTrail(Projectile.Center, Projectile.velocity, 1f);
            
            // Music notes spinning around
            if (Main.rand.NextBool(6))
            {
                float noteAngle = Main.GameUpdateCount * 0.15f;
                Vector2 notePos = Projectile.Center + noteAngle.ToRotationVector2() * 30f;
                DiesIraeVFX.SpawnMusicNote(notePos, Vector2.Zero, DiesIraeColors.HellfireGold, 0.85f);
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
            
            Lighting.AddLight(Projectile.Center, DiesIraeColors.EmberOrange.ToVector3() * 0.7f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 180);
            
            // Explosion on each hit
            DiesIraeVFX.FireImpact(target.Center, 0.9f);
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
            DiesIraeVFX.FireImpact(Projectile.Center, 1f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = texture.Size() / 2f;
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 0.1f;
            
            // === ENHANCED TRAIL - White → Red → Black gradient ===
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                
                float progress = i / (float)Projectile.oldPos.Length;
                float trailAlpha = (1f - progress) * 0.65f;
                float trailScale = Projectile.scale * (1f - progress * 0.4f);
                
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                
                // Black outer trail
                Color blackTrail = DiesIraeColors.CharredBlack * trailAlpha * 0.4f;
                blackTrail.A = 0;
                Main.spriteBatch.Draw(texture, drawPos, null, blackTrail, Projectile.oldRot[i], origin, trailScale * 1.15f, SpriteEffects.None, 0f);
                
                // Red middle trail
                Color redTrail = Color.Lerp(DiesIraeColors.BloodRed, DiesIraeColors.EmberOrange, progress) * trailAlpha;
                redTrail.A = 0;
                Main.spriteBatch.Draw(texture, drawPos, null, redTrail, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0f);
                
                // White core (fades faster)
                if (i < Projectile.oldPos.Length / 2)
                {
                    Color whiteTrail = Color.White * trailAlpha * 0.5f;
                    whiteTrail.A = 0;
                    Main.spriteBatch.Draw(texture, drawPos, null, whiteTrail, Projectile.oldRot[i], origin, trailScale * 0.8f, SpriteEffects.None, 0f);
                }
            }
            
            // === MAIN PROJECTILE BLOOM ===
            // Layer 1: Black outer
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = (MathHelper.TwoPi * i / 4f + Main.GameUpdateCount * 0.025f).ToRotationVector2() * 4f;
                Color blackGlow = DiesIraeColors.CharredBlack * 0.3f;
                blackGlow.A = 0;
                Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition + offset, null, blackGlow, Projectile.rotation, origin, Projectile.scale * 1.25f, SpriteEffects.None, 0f);
            }
            
            // Layer 2: Blood red
            for (int i = 0; i < 4; i++)
            {
                float scale = Projectile.scale * (1.1f + i * 0.08f);
                Color glowColor = DiesIraeColors.BloodRed * (0.4f / (i + 1));
                glowColor.A = 0;
                Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, glowColor, Projectile.rotation, origin, scale * pulse, SpriteEffects.None, 0f);
            }
            
            // Layer 3: Ember orange
            Color emberGlow = DiesIraeColors.EmberOrange * 0.45f;
            emberGlow.A = 0;
            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, emberGlow, Projectile.rotation, origin, Projectile.scale * pulse, SpriteEffects.None, 0f);
            
            // Layer 4: White-hot core
            Color whiteCore = Color.White * 0.6f;
            whiteCore.A = 0;
            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, whiteCore, Projectile.rotation, origin, Projectile.scale * 0.7f * pulse, SpriteEffects.None, 0f);
            
            return false;
        }
    }
    
    #endregion
    
    #region Damnation's Cannon Projectiles
    
    /// <summary>
    /// Main projectile - ball of ignited wrath
    /// </summary>
    public class IgnitedWrathBall : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/TallFlamingWispProjectile";
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
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
            
            // === LAYER 6: Music notes (theme element) ===
            if (Main.rand.NextBool(6))
            {
                DiesIraeVFX.SpawnMusicNote(Projectile.Center, -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f), DiesIraeColors.HellfireGold, 0.85f);
            }
            
            // Dynamic lighting with pulse
            float lightPulse = 0.9f + (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 0.15f;
            Lighting.AddLight(Projectile.Center, DiesIraeColors.EmberOrange.ToVector3() * lightPulse);
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
            // Extra particles on death
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
            Vector2 origin = texture.Size() / 2f;
            
            // Trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                
                float progress = i / (float)Projectile.oldPos.Length;
                Color trailColor = DiesIraeColors.GetGradient(progress) * (1f - progress) * 0.7f;
                trailColor.A = 0;
                
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Main.spriteBatch.Draw(texture, drawPos, null, trailColor, Projectile.rotation, origin, Projectile.scale * (1f - progress * 0.4f), SpriteEffects.None, 0f);
            }
            
            // Bloom
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.1f;
            for (int i = 0; i < 4; i++)
            {
                float scale = pulse * (1f + i * 0.2f);
                Color bloomColor = DiesIraeColors.EmberOrange * (0.4f / (i + 1));
                bloomColor.A = 0;
                
                Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, bloomColor, Projectile.rotation, origin, scale, SpriteEffects.None, 0f);
            }
            
            return false;
        }
    }
    
    /// <summary>
    /// Orbiting shrapnel that seeks enemies
    /// </summary>
    public class OrbitingShrapnel : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/FlamingWispProjectileSmall";
        
        private float orbitAngle;
        private float orbitRadius = 80f;
        private int orbitTimer = 0;
        private const int OrbitDuration = 90;
        private bool seeking = false;
        private int targetNPC = -1;
        
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
            
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // === LAYER 1: Core fire trail ===
            DiesIraeVFX.FireTrail(Projectile.Center, Projectile.velocity.SafeNormalize(Vector2.Zero) * 5f, 0.6f);
            
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
            
            // === LAYER 5: Music note (theme element) ===
            if (Main.rand.NextBool(12))
            {
                DiesIraeVFX.SpawnMusicNote(Projectile.Center, Vector2.Zero, DiesIraeColors.Crimson, 0.7f);
            }
            
            // Dynamic lighting
            float lightIntensity = seeking ? 0.5f : 0.35f;
            Lighting.AddLight(Projectile.Center, DiesIraeColors.Crimson.ToVector3() * lightIntensity);
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
        }
        
        public override void OnKill(int timeLeft)
        {
            DiesIraeVFX.FireImpact(Projectile.Center, 0.7f);
            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact with { Volume = 0.5f }, Projectile.Center);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = texture.Size() / 2f;
            
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.2f + Projectile.ai[1]) * 0.15f;
            
            // === ENHANCED ORBITING SPARK BLOOM ===
            // Layer 1: Black smoke outer
            for (int i = 0; i < 3; i++)
            {
                Vector2 offset = (MathHelper.TwoPi * i / 3f + Main.GameUpdateCount * 0.03f).ToRotationVector2() * 3f;
                Color blackGlow = DiesIraeColors.CharredBlack * 0.25f;
                blackGlow.A = 0;
                Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition + offset, null, blackGlow, Projectile.rotation, origin, 0.9f * pulse, SpriteEffects.None, 0f);
            }
            
            // Layer 2: Dark red glow
            for (int i = 0; i < 3; i++)
            {
                float scale = pulse * (0.7f + i * 0.1f);
                Color redGlow = DiesIraeColors.Crimson * (0.4f / (i + 1));
                redGlow.A = 0;
                Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, redGlow, Projectile.rotation, origin, scale, SpriteEffects.None, 0f);
            }
            
            // Layer 3: Orange accent
            Color emberGlow = DiesIraeColors.EmberOrange * 0.35f;
            emberGlow.A = 0;
            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, emberGlow, Projectile.rotation, origin, 0.6f * pulse, SpriteEffects.None, 0f);
            
            // Layer 4: White-hot core
            Color whiteCore = Color.White * 0.55f;
            whiteCore.A = 0;
            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, whiteCore, Projectile.rotation, origin, 0.4f * pulse, SpriteEffects.None, 0f);
            
            return false;
        }
    }
    
    #endregion
    
    #region Arbiter's Sentence Projectiles
    
    /// <summary>
    /// Flamethrower stream projectile
    /// </summary>
    public class JudgmentFlame : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/TallFlamingWispProjectile";
        
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
            
            // === LAYER 1: Core fire particles (enhanced) ===
            for (int i = 0; i < 3; i++)
            {
                Color fireColor = DiesIraeColors.GetGradient(Main.rand.NextFloat());
                Vector2 particlePos = Projectile.Center + Main.rand.NextVector2Circular(Projectile.width / 2f, Projectile.height / 2f);
                
                var fire = new GenericGlowParticle(
                    particlePos,
                    Main.rand.NextVector2Circular(2f, 2f) + new Vector2(0, -1f),
                    fireColor,
                    0.4f * Projectile.scale,
                    15,
                    true
                );
                MagnumParticleHandler.SpawnParticle(fire);
                
                // Vanilla dust
                Dust dust = Dust.NewDustPerfect(particlePos, DustID.Torch, Main.rand.NextVector2Circular(3f, 3f), 0, default, 1.5f * Projectile.scale);
                dust.noGravity = true;
            }
            
            // === LAYER 2: Pulsing judgment aura ===
            DiesIraeVFX.PulsingAura(Projectile.Center, DiesIraeColors.BloodRed, 0.5f * Projectile.scale, Projectile.timeLeft);
            
            // === LAYER 3: Rising flame wisp burst (periodic) ===
            if (Main.rand.NextBool(4))
            {
                DiesIraeVFX.FlameWispBurst(Projectile.Center + Main.rand.NextVector2Circular(10f, 10f), 0.5f, 2);
            }
            
            // === LAYER 4: Spiral fire pattern ===
            if (Main.rand.NextBool(3))
            {
                float spiralAngle = progress * MathHelper.TwoPi * 3f;
                DiesIraeVFX.SpiralTrail(Projectile.Center, new Vector2(0, -2f), DiesIraeColors.EmberOrange, 0.4f * Projectile.scale, spiralAngle);
            }
            
            // === LAYER 5: Orbiting ember sparks (intensifies with time) ===
            if (Main.GameUpdateCount % 5 == 0)
            {
                int sparkCount = 2 + (int)(progress * 3);
                DiesIraeVFX.OrbitingSparks(Projectile.Center, DiesIraeColors.Crimson, 12f * Projectile.scale, sparkCount, Main.GameUpdateCount * 0.1f, 0.3f);
            }
            
            // === LAYER 6: Music notes (rising judgment theme) ===
            if (Main.rand.NextBool(8))
            {
                DiesIraeVFX.SpawnMusicNote(Projectile.Center, new Vector2(Main.rand.NextFloat(-1f, 1f), -2.5f), DiesIraeColors.HellfireGold, 0.8f);
            }
            
            // Dynamic lighting with intensity that grows
            float lightIntensity = 0.5f + progress * 0.3f;
            Lighting.AddLight(Projectile.Center, DiesIraeColors.EmberOrange.ToVector3() * Projectile.scale * lightIntensity);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 300);
            
            // Explosion on hit
            DiesIraeVFX.FireImpact(target.Center, 0.8f);
            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact with { Volume = 0.4f }, target.Center);
        }
        
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            DiesIraeVFX.FireImpact(Projectile.Center, 0.6f);
            return true;
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
            
            // === LAYER 1: Core fire trail (enhanced) ===
            if (Projectile.timeLeft % 2 == 0)
            {
                var trail = new GenericGlowParticle(
                    Projectile.Center,
                    Vector2.Zero,
                    DiesIraeColors.EmberOrange,
                    0.25f,
                    10,
                    true
                );
                MagnumParticleHandler.SpawnParticle(trail);
            }
            
            // === LAYER 2: Afterimage trail for bullet speed ===
            if (Main.GameUpdateCount % 2 == 0)
            {
                DiesIraeVFX.AfterimageTrail(Projectile.Center, Projectile.velocity, 0.25f, DiesIraeColors.Crimson, 3);
            }
            
            // === LAYER 3: Sharp spiral trail ===
            if (Main.rand.NextBool(2))
            {
                float spiralAngle = Main.GameUpdateCount * 0.3f;
                DiesIraeVFX.SpiralTrail(Projectile.Center, Projectile.velocity, DiesIraeColors.BloodRed, 0.2f, spiralAngle);
            }
            
            // === LAYER 4: Occasional ember sparks ===
            if (Main.rand.NextBool(5))
            {
                Vector2 sparkVel = -Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(1f, 1f);
                var spark = new SparkleParticle(Projectile.Center, sparkVel, DiesIraeColors.HellfireGold, 0.2f, 8);
                MagnumParticleHandler.SpawnParticle(spark);
            }
            
            // === LAYER 5: Music note trace (rare, for theming) ===
            if (Main.rand.NextBool(25))
            {
                DiesIraeVFX.SpawnMusicNote(Projectile.Center, -Projectile.velocity * 0.1f, DiesIraeColors.HellfireGold, 0.6f);
            }
            
            Lighting.AddLight(Projectile.Center, DiesIraeColors.Crimson.ToVector3() * 0.35f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 240);
            
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
            SoundEngine.PlaySound(SoundID.DD2_LightningBugZap, target.Center);
            
            // Music notes burst
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi * i / 5f;
                Vector2 noteVel = angle.ToRotationVector2() * 3f;
                DiesIraeVFX.SpawnMusicNote(target.Center, noteVel, DiesIraeColors.HellfireGold, 0.9f);
            }
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
        
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = texture.Size() / 2f;
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.1f;
            
            // === ENHANCED TRAIL - White → Red → Black ===
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                
                float progress = i / (float)Projectile.oldPos.Length;
                float trailAlpha = (1f - progress) * 0.7f;
                float trailScale = (1f - progress * 0.5f);
                
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                
                // Black outer layer
                Color blackTrail = DiesIraeColors.CharredBlack * trailAlpha * 0.35f;
                blackTrail.A = 0;
                Main.spriteBatch.Draw(texture, drawPos, null, blackTrail, Projectile.oldRot[i], origin, trailScale * 1.1f, SpriteEffects.None, 0f);
                
                // Red middle
                Color redTrail = Color.Lerp(DiesIraeColors.BloodRed, DiesIraeColors.EmberOrange, progress) * trailAlpha;
                redTrail.A = 0;
                Main.spriteBatch.Draw(texture, drawPos, null, redTrail, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0f);
                
                // White core (fades faster)
                if (i < Projectile.oldPos.Length / 3)
                {
                    Color whiteTrail = Color.White * trailAlpha * 0.5f;
                    whiteTrail.A = 0;
                    Main.spriteBatch.Draw(texture, drawPos, null, whiteTrail, Projectile.oldRot[i], origin, trailScale * 0.75f, SpriteEffects.None, 0f);
                }
            }
            
            // === MAIN PROJECTILE BLOOM ===
            // Black outer
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = (MathHelper.TwoPi * i / 4f + Main.GameUpdateCount * 0.02f).ToRotationVector2() * 4f;
                Color blackGlow = DiesIraeColors.CharredBlack * 0.25f;
                blackGlow.A = 0;
                Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition + offset, null, blackGlow, Projectile.rotation, origin, 1.2f, SpriteEffects.None, 0f);
            }
            
            // Red glow
            for (int i = 0; i < 3; i++)
            {
                Color glowColor = DiesIraeColors.BloodRed * (0.4f / (i + 1));
                glowColor.A = 0;
                Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, glowColor, Projectile.rotation, origin, (1f + i * 0.2f) * pulse, SpriteEffects.None, 0f);
            }
            
            // Orange accent
            Color emberGlow = DiesIraeColors.EmberOrange * 0.4f;
            emberGlow.A = 0;
            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, emberGlow, Projectile.rotation, origin, 0.9f * pulse, SpriteEffects.None, 0f);
            
            // White core
            Color whiteCore = Color.White * 0.5f;
            whiteCore.A = 0;
            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, whiteCore, Projectile.rotation, origin, 0.6f * pulse, SpriteEffects.None, 0f);
            
            return true;
        }
    }
    
    /// <summary>
    /// Spinning copy of Wrath's Cleaver that slices enemies
    /// </summary>
    public class SpinningCleaverCopy : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/DiesIrae/ResonantWeapons/WrathsCleaver";
        
        private int targetNPC = -1;
        
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
            
            // Fire trail
            DiesIraeVFX.FireTrail(Projectile.Center, Projectile.velocity, 0.8f);
            
            // Music note
            if (Main.rand.NextBool(8))
            {
                DiesIraeVFX.SpawnMusicNote(Projectile.Center, Vector2.Zero, DiesIraeColors.BloodRed, 0.75f);
            }
            
            Lighting.AddLight(Projectile.Center, DiesIraeColors.BloodRed.ToVector3() * 0.5f);
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
            DiesIraeVFX.FireImpact(target.Center, 0.6f);
        }
        
        public override void OnKill(int timeLeft)
        {
            DiesIraeVFX.FireImpact(Projectile.Center, 1f);
            SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with { Volume = 0.5f }, Projectile.Center);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = texture.Size() / 2f;
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.18f) * 0.12f;
            
            // === ENHANCED SPECTRAL SPINNING CLEAVER ===
            
            // Layer 1: Black outer trail
            for (int i = 0; i < 6; i++)
            {
                float rotOffset = -i * 0.12f;
                float alpha = 0.25f * (1f - i / 6f);
                Color blackTrail = DiesIraeColors.CharredBlack * alpha;
                blackTrail.A = 0;
                Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, blackTrail, Projectile.rotation + rotOffset, origin, Projectile.scale * 1.15f, SpriteEffects.None, 0f);
            }
            
            // Layer 2: Red spectral trail
            for (int i = 0; i < 5; i++)
            {
                float rotOffset = -i * 0.15f;
                float alpha = 0.35f * (1f - i / 5f);
                Color redTrail = Color.Lerp(DiesIraeColors.BloodRed, DiesIraeColors.Crimson, i / 5f) * alpha;
                redTrail.A = 0;
                Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, redTrail, Projectile.rotation + rotOffset, origin, Projectile.scale * (1.05f - i * 0.02f), SpriteEffects.None, 0f);
            }
            
            // Layer 3: Orange ember glow
            Color emberGlow = DiesIraeColors.EmberOrange * 0.4f;
            emberGlow.A = 0;
            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, emberGlow, Projectile.rotation, origin, Projectile.scale * pulse, SpriteEffects.None, 0f);
            
            // Layer 4: White-hot blade core
            Color whiteCore = Color.White * 0.65f;
            whiteCore.A = 0;
            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, whiteCore, Projectile.rotation, origin, Projectile.scale * 0.85f * pulse, SpriteEffects.None, 0f);
            
            return false;
        }
    }
    
    #endregion
    
    #region Grimoire of Condemnation Projectiles
    
    /// <summary>
    /// Blazing music shard that spirals and chains electricity
    /// </summary>
    public class BlazingMusicShard : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/FlamingWispProjectileSmall";
        
        private float spiralAngle;
        private float spiralRadius = 50f;
        private int spiralTimer = 0;
        private int targetNPC = -1;
        private bool hasChained = false;
        
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
            
            // === LAYER 1: Music note trail (this IS a music shard - EMPHASIZED!) ===
            if (Main.rand.NextBool(2))
            {
                DiesIraeVFX.SpawnMusicNote(Projectile.Center, -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f), DiesIraeColors.HellfireGold, 0.9f);
            }
            
            // === LAYER 2: Core fire trail ===
            DiesIraeVFX.FireTrail(Projectile.Center, Projectile.velocity, 0.8f);
            
            // === LAYER 3: Spiral musical trail ===
            DiesIraeVFX.SpiralTrail(Projectile.Center, Projectile.velocity, DiesIraeColors.EmberOrange, 0.4f, spiralAngle);
            
            // === LAYER 4: Afterimage with music theme ===
            if (Projectile.velocity.Length() > 5f)
            {
                DiesIraeVFX.AfterimageTrail(Projectile.Center, Projectile.velocity, 0.4f, DiesIraeColors.HellfireGold, 4);
            }
            
            // === LAYER 5: Orbiting ember sparks ===
            if (Main.GameUpdateCount % 4 == 0)
            {
                DiesIraeVFX.OrbitingSparks(Projectile.Center, DiesIraeColors.Crimson, 12f, 3, spiralAngle, 0.25f);
            }
            
            // === LAYER 6: Pulsing musical aura ===
            if (Main.rand.NextBool(3))
            {
                DiesIraeVFX.PulsingAura(Projectile.Center, DiesIraeColors.HellfireGold, 0.4f, (int)(spiralTimer * 10));
            }
            
            // Dynamic lighting with pulse
            float lightPulse = 0.5f + (float)Math.Sin(spiralTimer * 0.15f) * 0.15f;
            Lighting.AddLight(Projectile.Center, DiesIraeColors.HellfireGold.ToVector3() * lightPulse);
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
            DiesIraeVFX.FireImpact(Projectile.Center, 0.8f);
            SoundEngine.PlaySound(SoundID.DD2_LightningBugZap with { Volume = 0.5f }, Projectile.Center);
            
            // Music note burst
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f;
                DiesIraeVFX.SpawnMusicNote(Projectile.Center, angle.ToRotationVector2() * 3f, DiesIraeColors.EmberOrange, 0.85f);
            }
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = texture.Size() / 2f;
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.2f + Projectile.ai[0]) * 0.2f;
            
            // === ENHANCED BLAZING MUSIC SHARD ===
            
            // Layer 1: Black smoke outer
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = (MathHelper.TwoPi * i / 4f + Main.GameUpdateCount * 0.025f).ToRotationVector2() * 5f;
                Color blackGlow = DiesIraeColors.CharredBlack * 0.3f;
                blackGlow.A = 0;
                Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition + offset, null, blackGlow, Projectile.rotation, origin, 1.1f * pulse, SpriteEffects.None, 0f);
            }
            
            // Layer 2: Blood red pulsing
            for (int i = 0; i < 4; i++)
            {
                float scale = pulse * (0.8f + i * 0.12f);
                Color redGlow = DiesIraeColors.Crimson * (0.4f / (i + 1));
                redGlow.A = 0;
                Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, redGlow, Projectile.rotation, origin, scale, SpriteEffects.None, 0f);
            }
            
            // Layer 3: Gold/orange music energy
            Color goldGlow = DiesIraeColors.HellfireGold * 0.45f;
            goldGlow.A = 0;
            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, goldGlow, Projectile.rotation, origin, 0.7f * pulse, SpriteEffects.None, 0f);
            
            // Layer 4: White core
            Color whiteCore = Color.White * 0.6f;
            whiteCore.A = 0;
            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, whiteCore, Projectile.rotation, origin, 0.45f * pulse, SpriteEffects.None, 0f);
            
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
            
            // === LAYER 1: Shimmer and sparkle effect (core visual) ===
            if (Main.rand.NextBool(3))
            {
                Color sparkleColor = Main.rand.NextBool() ? DiesIraeColors.HellfireGold : DiesIraeColors.EmberOrange;
                var sparkle = new SparkleParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(15f, 15f),
                    Main.rand.NextVector2Circular(1f, 1f),
                    sparkleColor,
                    0.4f * pulse,
                    15
                );
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // === LAYER 2: Fire glow core ===
            var glow = new GenericGlowParticle(
                Projectile.Center,
                Vector2.Zero,
                DiesIraeColors.EmberOrange,
                0.3f * pulse,
                5,
                true
            );
            MagnumParticleHandler.SpawnParticle(glow);
            
            // === LAYER 3: Orbiting ember sparks (enhanced) ===
            if (Main.GameUpdateCount % 5 == 0)
            {
                int sparkCount = seeking ? 4 : 2;
                DiesIraeVFX.OrbitingSparks(Projectile.Center, DiesIraeColors.Crimson, 14f, sparkCount, orbitAngle, 0.3f * pulse);
            }
            
            // === LAYER 4: Spiral trail when seeking ===
            if (seeking && Projectile.velocity.Length() > 3f)
            {
                DiesIraeVFX.SpiralTrail(Projectile.Center, Projectile.velocity, DiesIraeColors.EmberOrange, 0.35f, Main.GameUpdateCount * 0.12f);
            }
            
            // === LAYER 5: Pulsing aura ===
            DiesIraeVFX.PulsingAura(Projectile.Center, DiesIraeColors.HellfireGold, 0.35f * pulse, orbitTimer);
            
            // === LAYER 6: Afterimage when moving fast ===
            if (seeking && Projectile.velocity.Length() > 8f)
            {
                DiesIraeVFX.AfterimageTrail(Projectile.Center, Projectile.velocity, 0.35f, DiesIraeColors.BloodRed, 3);
            }
            
            // === LAYER 7: Music note (theme element) ===
            if (Main.rand.NextBool(10))
            {
                DiesIraeVFX.SpawnMusicNote(Projectile.Center, Main.rand.NextVector2Circular(1f, 1f), DiesIraeColors.HellfireGold, 0.8f);
            }
            
            // Dynamic lighting with intensity based on state
            float lightIntensity = seeking ? 0.7f : 0.5f;
            Lighting.AddLight(Projectile.Center, DiesIraeColors.HellfireGold.ToVector3() * lightIntensity * pulse);
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
        }
        
        public override void OnKill(int timeLeft)
        {
            DiesIraeVFX.FireImpact(Projectile.Center, 1.2f);
            SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode, Projectile.Center);
            
            // Music note explosion
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                DiesIraeVFX.SpawnMusicNote(Projectile.Center, angle.ToRotationVector2() * 4f, DiesIraeColors.HellfireGold, 0.9f);
            }
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = texture.Size() / 2f;
            
            int index = (int)Projectile.ai[1];
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f + index * 0.5f) * 0.25f;
            
            // Outer glow layers
            for (int i = 0; i < 4; i++)
            {
                float scale = pulse * (0.8f + i * 0.25f);
                Color color = DiesIraeColors.GetGradient((i + index) % 4 / 4f) * (0.4f / (i + 1));
                color.A = 0;
                
                Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, color, 0f, origin, scale, SpriteEffects.None, 0f);
            }
            
            // Core
            Color coreColor = DiesIraeColors.InfernalWhite * 0.8f;
            coreColor.A = 0;
            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, coreColor, 0f, origin, pulse * 0.5f, SpriteEffects.None, 0f);
            
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
        
        private int shardTimer = 0;
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
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
            
            // === LAYER 1: Heavy fire trail (core effect) ===
            for (int i = 0; i < 2; i++)
            {
                DiesIraeVFX.FireTrail(Projectile.Center + Main.rand.NextVector2Circular(10f, 10f), Projectile.velocity, 1f);
            }
            
            // === LAYER 2: Afterimage trail for motion blur ===
            if (Projectile.velocity.Length() > 3f)
            {
                DiesIraeVFX.AfterimageTrail(Projectile.Center, Projectile.velocity, 0.5f, DiesIraeColors.BloodRed, 4);
            }
            
            // === LAYER 3: Orbiting ember sparks ===
            if (Main.GameUpdateCount % 4 == 0)
            {
                DiesIraeVFX.OrbitingSparks(Projectile.Center, DiesIraeColors.Crimson, 18f, 4, Projectile.rotation * 0.5f, 0.35f);
            }
            
            // === LAYER 4: Pulsing dark aura ===
            float eclipsePulse = 1f + (float)Math.Sin(shardTimer * 0.08f) * 0.25f;
            DiesIraeVFX.PulsingAura(Projectile.Center, DiesIraeColors.CharredBlack, 0.6f * eclipsePulse, shardTimer);
            
            // === LAYER 5: Spiral eclipse pattern ===
            if (Main.rand.NextBool(2))
            {
                DiesIraeVFX.SpiralTrail(Projectile.Center, Projectile.velocity, DiesIraeColors.EmberOrange, 0.45f, Projectile.rotation);
            }
            
            // === LAYER 6: Flame wisp bursts (periodic) ===
            if (shardTimer % 15 == 0)
            {
                DiesIraeVFX.FlameWispBurst(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f), 0.4f, 2);
            }
            
            // === LAYER 7: Music notes (theme element) ===
            if (Main.rand.NextBool(5))
            {
                DiesIraeVFX.SpawnMusicNote(Projectile.Center, -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f), DiesIraeColors.EmberOrange, 0.85f);
            }
            
            // Dynamic lighting with eclipse-style pulse
            float lightIntensity = 0.7f + (float)Math.Sin(shardTimer * 0.1f) * 0.15f;
            Lighting.AddLight(Projectile.Center, DiesIraeColors.EmberOrange.ToVector3() * lightIntensity);
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
            DiesIraeVFX.FireImpact(Projectile.Center, 1.5f);
            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact, Projectile.Center);
            
            // Big music note burst
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                DiesIraeVFX.SpawnMusicNote(Projectile.Center, angle.ToRotationVector2() * 5f, DiesIraeColors.GetGradient(i / 8f), 1f);
            }
            
            Projectile.Kill();
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = texture.Size() / 2f;
            
            // Trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                
                float progress = i / (float)Projectile.oldPos.Length;
                Color trailColor = DiesIraeColors.GetGradient(progress) * (1f - progress) * 0.6f;
                trailColor.A = 0;
                
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Main.spriteBatch.Draw(texture, drawPos, null, trailColor, Projectile.oldRot[i], origin, Projectile.scale * (1f - progress * 0.3f), SpriteEffects.None, 0f);
            }
            
            // Bloom
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.15f;
            for (int i = 0; i < 4; i++)
            {
                float scale = pulse * (1f + i * 0.2f);
                Color color = DiesIraeColors.GetGradient(i / 4f) * (0.5f / (i + 1));
                color.A = 0;
                
                Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, color, Projectile.rotation, origin, scale, SpriteEffects.None, 0f);
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
                // Hover in place briefly
                Projectile.velocity *= 0.9f;
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
            
            // === LAYER 1: Core fire trail ===
            DiesIraeVFX.FireTrail(Projectile.Center, Projectile.velocity, 0.5f);
            
            // === LAYER 2: Afterimage trail when seeking ===
            if (Projectile.velocity.Length() > 6f)
            {
                DiesIraeVFX.AfterimageTrail(Projectile.Center, Projectile.velocity, 0.3f, DiesIraeColors.Crimson, 3);
            }
            
            // === LAYER 3: Spiral trail ===
            if (Main.rand.NextBool(2))
            {
                DiesIraeVFX.SpiralTrail(Projectile.Center, Projectile.velocity, DiesIraeColors.EmberOrange, 0.25f, Main.GameUpdateCount * 0.15f);
            }
            
            // === LAYER 4: Small orbiting embers ===
            if (Main.GameUpdateCount % 6 == 0)
            {
                DiesIraeVFX.OrbitingSparks(Projectile.Center, DiesIraeColors.BloodRed, 8f, 2, Main.GameUpdateCount * 0.12f, 0.15f);
            }
            
            // === LAYER 5: Occasional music note ===
            if (Main.rand.NextBool(15))
            {
                DiesIraeVFX.SpawnMusicNote(Projectile.Center, -Projectile.velocity * 0.1f, DiesIraeColors.HellfireGold, 0.65f);
            }
            
            Lighting.AddLight(Projectile.Center, DiesIraeColors.Crimson.ToVector3() * 0.35f);
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
        }
        
        public override void OnKill(int timeLeft)
        {
            DiesIraeVFX.FireImpact(Projectile.Center, 0.5f);
            
            // Music note
            DiesIraeVFX.SpawnMusicNote(Projectile.Center, Vector2.Zero, DiesIraeColors.EmberOrange, 0.75f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = texture.Size() / 2f;
            
            for (int i = 0; i < 3; i++)
            {
                float scale = 0.5f + i * 0.1f;
                Color color = DiesIraeColors.GetGradient(i / 3f) * (0.6f / (i + 1));
                color.A = 0;
                
                Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, color, Projectile.rotation, origin, scale, SpriteEffects.None, 0f);
            }
            
            return false;
        }
    }
    
    #endregion
}
