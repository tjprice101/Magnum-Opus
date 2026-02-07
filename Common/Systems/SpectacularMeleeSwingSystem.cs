using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.Fate;
using MagnumOpus.Content.Nachtmusik;

namespace MagnumOpus.Common.Systems
{
    /// <summary>
    /// SPECTACULAR MELEE SWING SYSTEM - Ark of the Cosmos Inspired
    /// 
    /// This system provides tiered visual effects for melee weapons:
    /// - Basic Tier: 1-2 sword arcs (early game)
    /// - Mid Tier: 3-4 sword arcs with trailing glow (mid game)
    /// - High Tier: 5-6 layered sword arcs with ambient particles (late game)
    /// - Endgame Tier: Full spectacle with cosmic clouds, seeking particles, fog, music notes
    /// 
    /// Each theme gets unique variations while maintaining consistent quality standards.
    /// </summary>
    public static class SpectacularMeleeSwing
    {
        // ========== WEAPON TIERS ==========
        public enum SwingTier
        {
            Basic,      // Pre-hardmode: 1-2 arcs
            Mid,        // Early hardmode: 3-4 arcs with glow
            High,       // Post-Plantera: 5-6 layered arcs, ambient particles
            Endgame,    // Post-Moon Lord: Full spectacle - clouds, seeking, fog, everything
            Ultimate    // Nachtmusik/Final: Maximum visual chaos
        }

        // ========== UNIVERSAL SWING EFFECT ==========
        
        /// <summary>
        /// Main entry point for spectacular melee swing effects.
        /// Call this in MeleeEffects for any weapon.
        /// </summary>
        public static void OnSwing(Player player, Rectangle hitbox, Color primaryColor, Color secondaryColor, 
            SwingTier tier, WeaponTheme theme = WeaponTheme.Default)
        {
            Vector2 hitCenter = hitbox.Center.ToVector2();
            Vector2 swingDir = (player.direction * Vector2.UnitX).RotatedBy(player.itemRotation);
            
            switch (tier)
            {
                case SwingTier.Basic:
                    BasicSwing(hitCenter, swingDir, primaryColor, secondaryColor, player.direction);
                    break;
                case SwingTier.Mid:
                    MidTierSwing(hitCenter, swingDir, primaryColor, secondaryColor, player.direction, theme);
                    break;
                case SwingTier.High:
                    HighTierSwing(hitCenter, swingDir, primaryColor, secondaryColor, player.direction, theme);
                    break;
                case SwingTier.Endgame:
                    EndgameSwing(player, hitCenter, swingDir, primaryColor, secondaryColor, theme);
                    break;
                case SwingTier.Ultimate:
                    UltimateSwing(player, hitCenter, swingDir, primaryColor, secondaryColor, theme);
                    break;
            }
        }

        // ========== BASIC TIER (1-2 arcs) ==========
        
        private static void BasicSwing(Vector2 center, Vector2 direction, Color primary, Color secondary, int playerDir)
        {
            // Simple 1-2 sword arc with basic trail
            if (Main.rand.NextBool(2))
            {
                Vector2 arcVel = direction * Main.rand.NextFloat(2f, 4f);
                CustomParticles.SwordArcSlash(center, arcVel, primary * 0.9f, 0.4f, direction.ToRotation());
            }
            
            // Basic glow trail
            if (Main.rand.NextBool(3))
            {
                Vector2 trailPos = center + Main.rand.NextVector2Circular(10f, 10f);
                var trail = new GenericGlowParticle(trailPos, direction * 0.5f, primary * 0.6f, 0.25f, 15, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }
        }

        // ========== MID TIER (3-4 arcs with glow) ==========
        
        private static void MidTierSwing(Vector2 center, Vector2 direction, Color primary, Color secondary, 
            int playerDir, WeaponTheme theme)
        {
            // 3-4 layered sword arcs
            int arcCount = Main.rand.Next(3, 5);
            for (int i = 0; i < arcCount; i++)
            {
                if (!Main.rand.NextBool(2)) continue;
                
                float angleOffset = Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 arcDir = direction.RotatedBy(angleOffset);
                float scale = 0.35f + i * 0.08f;
                Color arcColor = Color.Lerp(primary, secondary, (float)i / arcCount) * (0.8f - i * 0.1f);
                
                CustomParticles.SwordArcSlash(center + arcDir * i * 3f, arcDir * 3f, arcColor, scale, arcDir.ToRotation());
            }
            
            // Glowing trail particles
            for (int i = 0; i < 2; i++)
            {
                Vector2 trailPos = center + Main.rand.NextVector2Circular(15f, 15f);
                Vector2 trailVel = direction * Main.rand.NextFloat(1f, 3f) + Main.rand.NextVector2Circular(1f, 1f);
                Color trailColor = Color.Lerp(primary, secondary, Main.rand.NextFloat());
                var trail = new GenericGlowParticle(trailPos, trailVel, trailColor * 0.7f, 0.3f, 18, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }
            
            // Music notes (subtle)
            if (Main.rand.NextBool(5))
            {
                Vector2 notePos = center + Main.rand.NextVector2Circular(20f, 20f);
                ThemedParticles.MusicNote(notePos, direction * 0.5f, primary * 0.6f, 0.6f, 25);
            }
        }

        // ========== HIGH TIER (5-6 layered arcs, ambient particles) ==========
        
        private static void HighTierSwing(Vector2 center, Vector2 direction, Color primary, Color secondary, 
            int playerDir, WeaponTheme theme)
        {
            // 5-6 heavily layered sword arcs with staggered timing feel
            int arcCount = Main.rand.Next(5, 7);
            for (int i = 0; i < arcCount; i++)
            {
                float progress = (float)i / arcCount;
                float angleOffset = (progress - 0.5f) * 0.8f + Main.rand.NextFloat(-0.15f, 0.15f);
                Vector2 arcDir = direction.RotatedBy(angleOffset);
                
                // Scale increases for outer arcs
                float scale = 0.4f + progress * 0.25f;
                // Color gradient through the arc layers
                Color arcColor = Color.Lerp(primary, secondary, progress) * (0.9f - progress * 0.2f);
                
                // Spawn arc with slight position offset for depth
                Vector2 arcPos = center + arcDir * (progress * 8f);
                CustomParticles.SwordArcSlash(arcPos, arcDir * (3f + progress * 2f), arcColor, scale, arcDir.ToRotation());
            }
            
            // Dense trailing particles
            for (int i = 0; i < 4; i++)
            {
                Vector2 trailPos = center + Main.rand.NextVector2Circular(20f, 20f);
                Vector2 trailVel = direction * Main.rand.NextFloat(2f, 5f) + Main.rand.NextVector2Circular(2f, 2f);
                Color trailColor = Color.Lerp(primary, secondary, Main.rand.NextFloat()) * 0.8f;
                var trail = new GenericGlowParticle(trailPos, trailVel, trailColor, 0.35f, 22, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }
            
            // Sparkle accents
            if (Main.rand.NextBool(2))
            {
                Vector2 sparklePos = center + direction * 20f + Main.rand.NextVector2Circular(10f, 10f);
                var sparkle = new SparkleParticle(sparklePos, direction * 2f, Color.White * 0.6f, 0.25f, 18);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // Music notes (more prominent)
            if (Main.rand.NextBool(3))
            {
                Vector2 notePos = center + Main.rand.NextVector2Circular(25f, 25f);
                Vector2 noteVel = direction * Main.rand.NextFloat(0.5f, 1.5f);
                ThemedParticles.MusicNote(notePos, noteVel, primary * 0.75f, 0.75f, 30);
                
                // Sparkle companion
                var sparkle = new SparkleParticle(notePos, noteVel * 0.5f, secondary * 0.5f, 0.2f, 20);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // Theme-specific ambient effects
            SpawnThemeAmbient(center, direction, primary, secondary, theme, 0.5f);
        }

        // ========== ENDGAME TIER (Full Ark of the Cosmos spectacle) ==========
        
        private static void EndgameSwing(Player player, Vector2 center, Vector2 direction, Color primary, Color secondary, WeaponTheme theme)
        {
            // === PHASE 1: MASSIVE LAYERED SWORD ARCS (7-9 layers) ===
            int arcCount = Main.rand.Next(7, 10);
            for (int i = 0; i < arcCount; i++)
            {
                float progress = (float)i / arcCount;
                
                // Multiple arc patterns: main, upper sweep, lower sweep
                float[] angleOffsets = { 
                    (progress - 0.5f) * 1.0f,           // Main arc
                    (progress - 0.5f) * 0.6f + 0.2f,   // Upper sweep
                    (progress - 0.5f) * 0.6f - 0.2f    // Lower sweep
                };
                
                foreach (float baseOffset in angleOffsets)
                {
                    if (Main.rand.NextBool(3)) continue; // Stochastic spawning for natural feel
                    
                    float angleOffset = baseOffset + Main.rand.NextFloat(-0.1f, 0.1f);
                    Vector2 arcDir = direction.RotatedBy(angleOffset);
                    
                    float scale = 0.5f + progress * 0.35f;
                    Color arcColor = Color.Lerp(primary, secondary, progress) * (0.95f - progress * 0.15f);
                    
                    // Bloom glow behind arc
                    Vector2 arcPos = center + arcDir * (progress * 12f);
                    CustomParticles.GenericFlare(arcPos, arcColor * 0.4f, 0.2f, 8);
                    CustomParticles.SwordArcSlash(arcPos, arcDir * (4f + progress * 3f), arcColor, scale, arcDir.ToRotation());
                }
            }
            
            // === PHASE 2: COSMIC CLOUD BURST (Ark of the Cosmos style) ===
            if (Main.rand.NextBool(2))
            {
                SpawnCosmicCloudBurst(center, direction, primary, secondary, theme, 0.8f);
            }
            
            // === PHASE 3: DENSE PARTICLE TRAIL ===
            for (int i = 0; i < 6; i++)
            {
                Vector2 trailPos = center + Main.rand.NextVector2Circular(25f, 25f);
                Vector2 trailVel = direction * Main.rand.NextFloat(3f, 7f) + Main.rand.NextVector2Circular(3f, 3f);
                Color trailColor = Color.Lerp(primary, secondary, Main.rand.NextFloat());
                var trail = new GenericGlowParticle(trailPos, trailVel, trailColor * 0.85f, 0.4f, 25, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }
            
            // === PHASE 4: VISIBLE MUSIC NOTES (LARGE SCALE) ===
            if (Main.rand.NextBool(2))
            {
                for (int i = 0; i < 2; i++)
                {
                    Vector2 notePos = center + Main.rand.NextVector2Circular(30f, 30f);
                    Vector2 noteVel = direction * Main.rand.NextFloat(1f, 2.5f);
                    Color noteColor = Color.Lerp(primary, secondary, Main.rand.NextFloat());
                    
                    // Multi-layer bloom on music notes
                    CustomParticles.GenericFlare(notePos, noteColor * 0.3f, 0.3f, 10);
                    ThemedParticles.MusicNote(notePos, noteVel, noteColor * 0.9f, Main.rand.NextFloat(0.8f, 1.1f), 35);
                    
                    // Sparkle companions
                    for (int s = 0; s < 2; s++)
                    {
                        var sparkle = new SparkleParticle(notePos + Main.rand.NextVector2Circular(8f, 8f),
                            noteVel * 0.3f + Main.rand.NextVector2Circular(1f, 1f), secondary * 0.5f, 0.22f, 22);
                        MagnumParticleHandler.SpawnParticle(sparkle);
                    }
                }
            }
            
            // === PHASE 5: SEEKING SHIMMER PARTICLES ===
            if (Main.rand.NextBool(4))
            {
                SpawnSeekingShimmerParticles(player, center, direction, primary, secondary, 3);
            }
            
            // === PHASE 6: FOG/MIST ELEMENT ===
            if (Main.rand.NextBool(3))
            {
                SpawnSwingFog(center, direction, primary, theme);
            }
            
            // === PHASE 7: THEME-SPECIFIC SPECTACULAR EFFECTS ===
            SpawnThemeSpectacular(player, center, direction, primary, secondary, theme);
            
            // Dynamic lighting
            Lighting.AddLight(center, primary.ToVector3() * 0.7f);
        }

        // ========== ULTIMATE TIER (Nachtmusik/Final - Maximum everything) ==========
        
        private static void UltimateSwing(Player player, Vector2 center, Vector2 direction, Color primary, Color secondary, WeaponTheme theme)
        {
            // Everything from Endgame PLUS:
            EndgameSwing(player, center, direction, primary, secondary, theme);
            
            // === ADDITIONAL: DOUBLE ARC CASCADE ===
            for (int wave = 0; wave < 2; wave++)
            {
                float waveOffset = wave * 0.15f;
                int arcCount = Main.rand.Next(5, 7);
                
                for (int i = 0; i < arcCount; i++)
                {
                    float progress = (float)i / arcCount;
                    float angleOffset = (progress - 0.5f) * 1.2f + waveOffset;
                    Vector2 arcDir = direction.RotatedBy(angleOffset);
                    
                    float scale = 0.55f + progress * 0.4f + wave * 0.1f;
                    Color arcColor = Color.Lerp(primary, secondary, progress + wave * 0.2f) * (0.85f - wave * 0.1f);
                    
                    Vector2 arcPos = center + arcDir * (progress * 15f + wave * 8f);
                    CustomParticles.SwordArcSlash(arcPos, arcDir * (5f + progress * 4f), arcColor, scale, arcDir.ToRotation());
                }
            }
            
            // === ADDITIONAL: COSMIC CONSTELLATION EFFECT ===
            if (Main.rand.NextBool(3))
            {
                SpawnConstellationEffect(center, direction, primary, secondary, 5);
            }
            
            // === ADDITIONAL: ENHANCED SEEKING PARTICLES ===
            if (Main.rand.NextBool(3))
            {
                SpawnSeekingShimmerParticles(player, center, direction, primary, secondary, 5);
            }
            
            // === ADDITIONAL: GLYPH ACCENTS ===
            if (Main.rand.NextBool(4))
            {
                float glyphAngle = direction.ToRotation() + Main.rand.NextFloat(-0.5f, 0.5f);
                Vector2 glyphPos = center + glyphAngle.ToRotationVector2() * 35f;
                CustomParticles.Glyph(glyphPos, secondary, 0.45f, -1);
            }
            
            // Enhanced lighting
            Lighting.AddLight(center, primary.ToVector3() * 1.0f);
            Lighting.AddLight(center + direction * 30f, secondary.ToVector3() * 0.6f);
        }

        // ========== HELPER METHODS ==========

        /// <summary>
        /// Spawn cosmic cloud burst effect (Ark of the Cosmos nebula style)
        /// </summary>
        private static void SpawnCosmicCloudBurst(Vector2 center, Vector2 direction, Color primary, Color secondary, 
            WeaponTheme theme, float intensity)
        {
            int cloudCount = (int)(8 * intensity);
            
            for (int i = 0; i < cloudCount; i++)
            {
                float angle = direction.ToRotation() + Main.rand.NextFloat(-0.8f, 0.8f);
                float speed = Main.rand.NextFloat(2f, 5f) * intensity;
                Vector2 cloudVel = angle.ToRotationVector2() * speed;
                
                // Layered cloud particles for nebula effect
                for (int layer = 0; layer < 3; layer++)
                {
                    float layerProgress = layer / 3f;
                    Color cloudColor = Color.Lerp(Color.Black, Color.Lerp(primary, secondary, layerProgress), 0.4f + layerProgress * 0.3f);
                    cloudColor *= (0.5f - layer * 0.1f);
                    
                    float particleScale = (0.25f + layer * 0.1f) * intensity;
                    Vector2 offset = Main.rand.NextVector2Circular(6f, 6f);
                    Vector2 layerVel = cloudVel * (0.6f + layer * 0.15f) + Main.rand.NextVector2Circular(1f, 1f);
                    
                    var cloud = new GenericGlowParticle(center + offset, layerVel, cloudColor, particleScale, 28, true);
                    MagnumParticleHandler.SpawnParticle(cloud);
                }
            }
            
            // Star points in the cloud
            for (int i = 0; i < (int)(4 * intensity); i++)
            {
                Vector2 starPos = center + Main.rand.NextVector2Circular(20f, 20f);
                Vector2 starVel = direction * Main.rand.NextFloat(1f, 3f);
                var star = new GenericGlowParticle(starPos, starVel, Color.White * 0.7f, 0.18f * intensity, 18, true);
                MagnumParticleHandler.SpawnParticle(star);
            }
        }

        /// <summary>
        /// Spawn seeking shimmer particles that home toward nearby enemies
        /// </summary>
        private static void SpawnSeekingShimmerParticles(Player player, Vector2 center, Vector2 direction, 
            Color primary, Color secondary, int count)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 spawnOffset = Main.rand.NextVector2Circular(25f, 25f);
                Vector2 spawnPos = center + spawnOffset;
                Vector2 initialVel = direction * Main.rand.NextFloat(2f, 4f) + Main.rand.NextVector2Circular(2f, 2f);
                
                // Find nearest enemy to home toward
                NPC target = FindNearestEnemy(spawnPos, 400f);
                
                Color shimmerColor = Color.Lerp(primary, secondary, Main.rand.NextFloat());
                
                // Create seeking particle (using GlowSpark with homing behavior simulated via updates)
                var seeker = new SeekingShimmerParticle(spawnPos, initialVel, shimmerColor, 0.3f, 60, target);
                MagnumParticleHandler.SpawnParticle(seeker);
            }
        }

        /// <summary>
        /// Spawn swing fog/mist effect
        /// </summary>
        private static void SpawnSwingFog(Vector2 center, Vector2 direction, Color primary, WeaponTheme theme)
        {
            int fogCount = Main.rand.Next(3, 6);
            
            for (int i = 0; i < fogCount; i++)
            {
                float angle = direction.ToRotation() + Main.rand.NextFloat(-1f, 1f);
                Vector2 fogPos = center + angle.ToRotationVector2() * Main.rand.NextFloat(15f, 35f);
                Vector2 fogVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-0.3f, 0f));
                
                // Fog color based on theme
                Color fogColor = GetThemeFogColor(primary, theme) * Main.rand.NextFloat(0.15f, 0.3f);
                
                var fog = new GenericGlowParticle(fogPos, fogVel, fogColor, Main.rand.NextFloat(0.4f, 0.7f), 
                    Main.rand.Next(35, 55), true);
                MagnumParticleHandler.SpawnParticle(fog);
            }
        }

        /// <summary>
        /// Spawn constellation effect with connecting star points
        /// </summary>
        private static void SpawnConstellationEffect(Vector2 center, Vector2 direction, Color primary, Color secondary, int starCount)
        {
            List<Vector2> starPositions = new List<Vector2>();
            
            for (int i = 0; i < starCount; i++)
            {
                float angle = direction.ToRotation() + (float)(i - starCount / 2) * 0.4f + Main.rand.NextFloat(-0.2f, 0.2f);
                float dist = Main.rand.NextFloat(20f, 50f);
                Vector2 starPos = center + angle.ToRotationVector2() * dist;
                starPositions.Add(starPos);
                
                // Star flare
                Color starColor = Main.rand.NextBool() ? Color.White : Color.Lerp(primary, secondary, Main.rand.NextFloat());
                CustomParticles.GenericFlare(starPos, starColor, 0.35f, 20);
                
                // Glow around star
                var glow = new GenericGlowParticle(starPos, Vector2.Zero, starColor * 0.5f, 0.2f, 18, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Draw connecting lines via particles (create line of small particles between stars)
            for (int i = 0; i < starPositions.Count - 1; i++)
            {
                Vector2 start = starPositions[i];
                Vector2 end = starPositions[i + 1];
                int linePoints = 4;
                
                for (int p = 0; p < linePoints; p++)
                {
                    float t = (float)p / linePoints;
                    Vector2 linePos = Vector2.Lerp(start, end, t);
                    var lineParticle = new GenericGlowParticle(linePos, Vector2.Zero, secondary * 0.3f, 0.1f, 15, true);
                    MagnumParticleHandler.SpawnParticle(lineParticle);
                }
            }
        }

        /// <summary>
        /// Spawn theme-specific ambient effects
        /// </summary>
        private static void SpawnThemeAmbient(Vector2 center, Vector2 direction, Color primary, Color secondary, 
            WeaponTheme theme, float intensity)
        {
            switch (theme)
            {
                case WeaponTheme.Spring:
                    // Cherry blossom petals
                    if (Main.rand.NextBool((int)(4 / intensity)))
                    {
                        Vector2 petalPos = center + Main.rand.NextVector2Circular(25f, 25f);
                        var petal = new GenericGlowParticle(petalPos, direction * 0.5f + new Vector2(0, 0.3f),
                            new Color(255, 183, 197) * 0.7f, 0.25f, 30, true);
                        MagnumParticleHandler.SpawnParticle(petal);
                    }
                    break;
                    
                case WeaponTheme.Summer:
                    // Heat shimmer / embers
                    if (Main.rand.NextBool((int)(3 / intensity)))
                    {
                        Vector2 emberPos = center + Main.rand.NextVector2Circular(20f, 20f);
                        Vector2 emberVel = new Vector2(0, -Main.rand.NextFloat(1f, 2f));
                        var ember = new GenericGlowParticle(emberPos, emberVel, new Color(255, 140, 0) * 0.6f, 0.2f, 25, true);
                        MagnumParticleHandler.SpawnParticle(ember);
                    }
                    break;
                    
                case WeaponTheme.Autumn:
                    // Falling leaves / decay particles
                    if (Main.rand.NextBool((int)(4 / intensity)))
                    {
                        Vector2 leafPos = center + Main.rand.NextVector2Circular(25f, 25f);
                        Vector2 leafVel = direction * 0.3f + new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), 0.5f);
                        Color leafColor = Color.Lerp(new Color(200, 100, 50), new Color(150, 80, 30), Main.rand.NextFloat());
                        var leaf = new GenericGlowParticle(leafPos, leafVel, leafColor * 0.7f, 0.22f, 35, true);
                        MagnumParticleHandler.SpawnParticle(leaf);
                    }
                    break;
                    
                case WeaponTheme.Winter:
                    // Frost crystals / snowflakes
                    if (Main.rand.NextBool((int)(3 / intensity)))
                    {
                        Vector2 frostPos = center + Main.rand.NextVector2Circular(20f, 20f);
                        var frost = new GenericGlowParticle(frostPos, Main.rand.NextVector2Circular(1f, 1f),
                            new Color(180, 220, 255) * 0.6f, 0.18f, 22, true);
                        MagnumParticleHandler.SpawnParticle(frost);
                        
                        Dust.NewDustPerfect(frostPos, DustID.Frost, Main.rand.NextVector2Circular(2f, 2f), 0, default, 0.8f);
                    }
                    break;
                
                case WeaponTheme.Seasons:
                    // Combined seasonal effects - cycling through all four
                    int seasonPhase = (int)(Main.GameUpdateCount / 30) % 4;
                    Color[] seasonColors = { new Color(255, 183, 197), new Color(255, 215, 0), new Color(255, 140, 50), new Color(150, 220, 255) };
                    if (Main.rand.NextBool((int)(4 / intensity)))
                    {
                        Vector2 seasonPos = center + Main.rand.NextVector2Circular(22f, 22f);
                        var seasonParticle = new GenericGlowParticle(seasonPos, direction * 0.3f,
                            seasonColors[seasonPhase] * 0.6f, 0.22f, 25, true);
                        MagnumParticleHandler.SpawnParticle(seasonParticle);
                    }
                    break;
                    
                case WeaponTheme.Eroica:
                    // Sakura petals + golden embers
                    if (Main.rand.NextBool((int)(4 / intensity)))
                    {
                        ThemedParticles.SakuraPetals(center, 1, 30f);
                    }
                    break;
                    
                case WeaponTheme.LaCampanella:
                    // Heavy smoke wisps
                    if (Main.rand.NextBool((int)(3 / intensity)))
                    {
                        Vector2 smokePos = center + Main.rand.NextVector2Circular(20f, 20f);
                        var smoke = new HeavySmokeParticle(smokePos, direction * 0.5f + Main.rand.NextVector2Circular(1f, 1f),
                            Color.Black, Main.rand.Next(25, 40), 0.3f, 0.5f, 0.02f, false);
                        MagnumParticleHandler.SpawnParticle(smoke);
                    }
                    break;
                    
                case WeaponTheme.Enigma:
                    // Void wisps + occasional eye
                    if (Main.rand.NextBool((int)(5 / intensity)))
                    {
                        Vector2 voidPos = center + Main.rand.NextVector2Circular(25f, 25f);
                        var voidWisp = new GenericGlowParticle(voidPos, Main.rand.NextVector2Circular(1f, 1f),
                            new Color(140, 60, 200) * 0.5f, 0.25f, 20, true);
                        MagnumParticleHandler.SpawnParticle(voidWisp);
                    }
                    break;
                    
                case WeaponTheme.Fate:
                    // Glyphs + star sparkles
                    if (Main.rand.NextBool((int)(5 / intensity)))
                    {
                        Vector2 glyphPos = center + Main.rand.NextVector2Circular(30f, 30f);
                        CustomParticles.Glyph(glyphPos, FateCosmicVFX.FateDarkPink, 0.3f, -1);
                    }
                    if (Main.rand.NextBool((int)(4 / intensity)))
                    {
                        var star = new GenericGlowParticle(center + Main.rand.NextVector2Circular(25f, 25f),
                            Main.rand.NextVector2Circular(0.5f, 0.5f), FateCosmicVFX.FateWhite, 0.15f, 15, true);
                        MagnumParticleHandler.SpawnParticle(star);
                    }
                    break;
                    
                case WeaponTheme.Nachtmusik:
                    // Celestial particles + constellation points
                    if (Main.rand.NextBool((int)(4 / intensity)))
                    {
                        Vector2 celestialPos = center + Main.rand.NextVector2Circular(30f, 30f);
                        Color celestialColor = NachtmusikCosmicVFX.GetCelestialGradient(Main.rand.NextFloat());
                        var celestial = new GenericGlowParticle(celestialPos, Main.rand.NextVector2Circular(1f, 1f),
                            celestialColor * 0.6f, 0.2f, 18, true);
                        MagnumParticleHandler.SpawnParticle(celestial);
                    }
                    break;
            }
        }

        /// <summary>
        /// Spawn theme-specific spectacular effects (for endgame tier)
        /// </summary>
        private static void SpawnThemeSpectacular(Player player, Vector2 center, Vector2 direction, 
            Color primary, Color secondary, WeaponTheme theme)
        {
            switch (theme)
            {
                case WeaponTheme.Fate:
                    // Chromatic aberration particles
                    if (Main.rand.NextBool(4))
                    {
                        Vector2 aberrationPos = center + direction * 30f;
                        CustomParticles.GenericFlare(aberrationPos + new Vector2(-2, 0), new Color(255, 0, 0, 0) * 0.4f, 0.25f, 10);
                        CustomParticles.GenericFlare(aberrationPos + new Vector2(2, 0), new Color(0, 0, 255, 0) * 0.4f, 0.25f, 10);
                    }
                    
                    // Cosmic cloud trail
                    if (Main.rand.NextBool(2))
                    {
                        FateCosmicVFX.SpawnCosmicCloudTrail(center + direction * 15f, direction * 2f, 0.6f);
                    }
                    
                    // Glyph circle accent
                    if (Main.rand.NextBool(6))
                    {
                        float circleAngle = Main.GameUpdateCount * 0.05f;
                        for (int g = 0; g < 3; g++)
                        {
                            Vector2 glyphPos = center + (circleAngle + MathHelper.TwoPi * g / 3f).ToRotationVector2() * 40f;
                            CustomParticles.Glyph(glyphPos, FateCosmicVFX.FatePurple, 0.35f, -1);
                        }
                    }
                    break;
                    
                case WeaponTheme.Eroica:
                    // Sakura storm
                    if (Main.rand.NextBool(2))
                    {
                        ThemedParticles.SakuraPetals(center, Main.rand.Next(2, 4), 40f);
                    }
                    
                    // Rising golden embers
                    if (Main.rand.NextBool(3))
                    {
                        for (int e = 0; e < 3; e++)
                        {
                            Vector2 emberPos = center + Main.rand.NextVector2Circular(25f, 25f);
                            Vector2 emberVel = new Vector2(Main.rand.NextFloat(-1f, 1f), -Main.rand.NextFloat(2f, 4f));
                            var ember = new GlowSparkParticle(emberPos, emberVel, UnifiedVFX.Eroica.Gold, 0.25f, 25);
                            MagnumParticleHandler.SpawnParticle(ember);
                        }
                    }
                    break;
                    
                case WeaponTheme.LaCampanella:
                    // Heavy smoke eruption
                    if (Main.rand.NextBool(2))
                    {
                        for (int s = 0; s < 3; s++)
                        {
                            Vector2 smokePos = center + Main.rand.NextVector2Circular(15f, 15f);
                            var smoke = new HeavySmokeParticle(smokePos, direction * 2f + Main.rand.NextVector2Circular(2f, 2f),
                                Color.Black, Main.rand.Next(30, 50), 0.4f, 0.6f, 0.025f, false);
                            MagnumParticleHandler.SpawnParticle(smoke);
                        }
                    }
                    
                    // Bell chime flare
                    if (Main.rand.NextBool(4))
                    {
                        Vector2 chimePos = center + direction * 25f;
                        CustomParticles.GenericFlare(chimePos, UnifiedVFX.LaCampanella.Orange, 0.5f, 15);
                        CustomParticles.GenericFlare(chimePos, UnifiedVFX.LaCampanella.Gold, 0.35f, 12);
                    }
                    break;
                    
                case WeaponTheme.Enigma:
                    // Void tendrils
                    if (Main.rand.NextBool(3))
                    {
                        for (int t = 0; t < 2; t++)
                        {
                            float tendrilAngle = direction.ToRotation() + Main.rand.NextFloat(-0.6f, 0.6f);
                            Vector2 tendrilPos = center;
                            for (int seg = 0; seg < 5; seg++)
                            {
                                tendrilPos += tendrilAngle.ToRotationVector2() * 8f;
                                tendrilAngle += Main.rand.NextFloat(-0.2f, 0.2f);
                                var tendril = new GenericGlowParticle(tendrilPos, Vector2.Zero,
                                    new Color(80, 20, 120) * (0.6f - seg * 0.1f), 0.2f - seg * 0.02f, 15, true);
                                MagnumParticleHandler.SpawnParticle(tendril);
                            }
                        }
                    }
                    
                    // Watching eye accent
                    if (Main.rand.NextBool(8))
                    {
                        Vector2 eyePos = center + Main.rand.NextVector2Circular(40f, 40f);
                        CustomParticles.EnigmaEyeGaze(eyePos, new Color(140, 60, 200), 0.4f, player.Center);
                    }
                    break;
                    
                case WeaponTheme.Nachtmusik:
                    // Full celestial explosion
                    if (Main.rand.NextBool(3))
                    {
                        NachtmusikCosmicVFX.SpawnCelestialImpact(center + direction * 20f, 0.8f);
                    }
                    
                    // Star burst trail
                    if (Main.rand.NextBool(2))
                    {
                        for (int star = 0; star < 4; star++)
                        {
                            Vector2 starPos = center + Main.rand.NextVector2Circular(30f, 30f);
                            Vector2 starVel = direction * Main.rand.NextFloat(2f, 4f);
                            var starParticle = new GlowSparkParticle(starPos, starVel, 
                                NachtmusikCosmicVFX.GetCelestialGradient(Main.rand.NextFloat()), 0.25f, 20);
                            MagnumParticleHandler.SpawnParticle(starParticle);
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Get fog color based on theme
        /// </summary>
        private static Color GetThemeFogColor(Color primary, WeaponTheme theme)
        {
            return theme switch
            {
                WeaponTheme.Spring => new Color(255, 220, 230), // Pink mist
                WeaponTheme.Summer => new Color(255, 200, 150), // Golden haze
                WeaponTheme.Autumn => new Color(180, 120, 80),  // Brown mist
                WeaponTheme.Winter => new Color(200, 220, 255), // Frost fog
                WeaponTheme.Eroica => new Color(255, 180, 180), // Sakura mist
                WeaponTheme.LaCampanella => Color.Black,        // Dark smoke
                WeaponTheme.Enigma => new Color(60, 20, 80),    // Void mist
                WeaponTheme.Fate => new Color(80, 30, 60),      // Cosmic fog
                WeaponTheme.Nachtmusik => new Color(40, 30, 80), // Celestial mist
                _ => primary * 0.3f
            };
        }

        /// <summary>
        /// Find nearest enemy within range
        /// </summary>
        private static NPC FindNearestEnemy(Vector2 position, float range)
        {
            NPC nearest = null;
            float nearestDist = range;
            
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                
                float dist = Vector2.Distance(position, npc.Center);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = npc;
                }
            }
            
            return nearest;
        }

        // ========== WEAPON THEMES ==========
        
        public enum WeaponTheme
        {
            Default,
            Spring,
            Summer,
            Autumn,
            Winter,
            Seasons, // Combined Four Seasons
            Eroica,
            LaCampanella,
            MoonlightSonata,
            SwanLake, // Don't touch!
            Enigma,
            Fate,
            Nachtmusik
        }
    }

    /// <summary>
    /// Seeking shimmer particle that homes toward enemies (Ark of the Cosmos style)
    /// </summary>
    public class SeekingShimmerParticle : Particle
    {
        private NPC _target;
        private float _homingStrength = 0.08f;
        private float _maxSpeed = 12f;
        private int _maxLifetime;
        private bool _fadeOut = true;
        
        public override string Texture => "SoftGlow";
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;
        public override bool SetLifetime => true;
        
        public SeekingShimmerParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime, NPC target)
        {
            Position = position;
            Velocity = velocity;
            Color = color;
            Scale = scale;
            Lifetime = lifetime;
            _maxLifetime = lifetime;
            _target = target;
        }
        
        public override void Update()
        {
            // Home toward target if it exists and is valid
            if (_target != null && _target.active && !_target.friendly)
            {
                Vector2 toTarget = (_target.Center - Position).SafeNormalize(Vector2.Zero);
                Velocity = Vector2.Lerp(Velocity, toTarget * _maxSpeed, _homingStrength);
                
                // Check if we hit the target
                if (Vector2.Distance(Position, _target.Center) < 20f)
                {
                    // Spawn impact particles
                    CustomParticles.GenericFlare(Position, Color, 0.3f, 10);
                    for (int i = 0; i < 3; i++)
                    {
                        var spark = new GenericGlowParticle(Position, Main.rand.NextVector2Circular(3f, 3f),
                            Color * 0.6f, 0.15f, 10, true);
                        MagnumParticleHandler.SpawnParticle(spark);
                    }
                    
                    Kill(); // Kill particle
                    return;
                }
            }
            else
            {
                // No target - just float with slight deceleration
                Velocity *= 0.98f;
            }
            
            // Leave sparkle trail
            if (Main.rand.NextBool(3))
            {
                var trail = new GenericGlowParticle(Position, -Velocity * 0.1f, Color * 0.4f, Scale * 0.6f, 10, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }
        }
        
        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ParticleTextureHelper.GetTexture(Texture);
            Vector2 drawPos = Position - Main.screenPosition;
            Vector2 origin = tex.Size() * 0.5f;
            
            float progress = (float)Time / _maxLifetime;
            float alpha = _fadeOut ? (1f - progress) : 1f;
            
            // Multi-layer bloom
            Color bloomColor = Color with { A = 0 };
            spriteBatch.Draw(tex, drawPos, null, bloomColor * alpha * 0.3f, Rotation, origin, Scale * 1.8f, SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, drawPos, null, bloomColor * alpha * 0.5f, Rotation, origin, Scale * 1.3f, SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, drawPos, null, Color * alpha * 0.8f, Rotation, origin, Scale, SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, drawPos, null, Microsoft.Xna.Framework.Color.White * alpha * 0.6f, Rotation, origin, Scale * 0.5f, SpriteEffects.None, 0f);
        }
    }
}
