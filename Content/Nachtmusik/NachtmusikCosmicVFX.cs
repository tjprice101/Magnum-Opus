using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.Nachtmusik
{
    /// <summary>
    /// Visual effects system for Nachtmusik, Queen of Radiance theme.
    /// Post-Fate tier weapons with celestial purple/gold/violet aesthetics.
    /// Theme: Nocturnal celestial radiance, starlit symphonies, the queen's grace.
    /// </summary>
    public static class NachtmusikCosmicVFX
    {
        #region Theme Colors - Celestial Radiance Palette
        
        // Primary colors
        public static readonly Color DeepPurple = new Color(45, 27, 78);       // #2D1B4E - Dark cosmic base
        public static readonly Color Gold = new Color(255, 215, 0);             // #FFD700 - Radiant gold
        public static readonly Color Violet = new Color(123, 104, 238);         // #7B68EE - Medium violet
        public static readonly Color StarWhite = new Color(255, 255, 255);      // Pure starlight
        public static readonly Color NightBlue = new Color(25, 25, 112);        // Midnight blue accent
        
        // Extended palette
        public static readonly Color CosmicPurple = new Color(75, 50, 130);     // Rich cosmic purple
        public static readonly Color NebulaPink = new Color(180, 120, 200);     // Nebula accent
        public static readonly Color StarGold = new Color(255, 230, 150);       // Warm star gold
        public static readonly Color MoonSilver = new Color(210, 220, 240);     // Moonlight silver
        public static readonly Color DuskViolet = new Color(100, 80, 180);      // Dusk transition
        
        #endregion
        
        #region Color Utilities
        
        /// <summary>
        /// Gets a gradient color along the Nachtmusik celestial spectrum.
        /// 0.0 = Deep Purple → 0.25 = Violet → 0.5 = Nebula Pink → 0.75 = Gold → 1.0 = Star White
        /// </summary>
        public static Color GetCelestialGradient(float progress)
        {
            progress = MathHelper.Clamp(progress, 0f, 1f);
            
            if (progress < 0.25f)
                return Color.Lerp(DeepPurple, Violet, progress / 0.25f);
            else if (progress < 0.5f)
                return Color.Lerp(Violet, NebulaPink, (progress - 0.25f) / 0.25f);
            else if (progress < 0.75f)
                return Color.Lerp(NebulaPink, Gold, (progress - 0.5f) / 0.25f);
            else
                return Color.Lerp(Gold, StarWhite, (progress - 0.75f) / 0.25f);
        }
        
        /// <summary>
        /// Gets a pulsing time-based gradient for ambient effects.
        /// </summary>
        public static Color GetPulsingGradient(float timeOffset = 0f)
        {
            float progress = ((float)Math.Sin(Main.GameUpdateCount * 0.03f + timeOffset) * 0.5f + 0.5f);
            return GetCelestialGradient(progress);
        }
        
        #endregion
        
        #region Core VFX Methods
        
        /// <summary>
        /// Spawns a celestial explosion with multiple layered effects.
        /// </summary>
        public static void SpawnCelestialExplosion(Vector2 position, float scale = 1f)
        {
            // Core white flash
            CustomParticles.GenericFlare(position, StarWhite, 1.2f * scale, 22);
            CustomParticles.GenericFlare(position, Gold, 0.9f * scale, 20);
            CustomParticles.GenericFlare(position, Violet, 0.7f * scale, 18);
            
            // Cascading starburst layers (replacing banned HaloRing)
            for (int i = 0; i < 6; i++)
            {
                float progress = (float)i / 6f;
                Color burstColor = GetCelestialGradient(progress);
                float burstScale = (0.4f + i * 0.08f) * scale;
                var starburst = new StarBurstParticle(position, Vector2.Zero, burstColor, burstScale, 16 + i * 3, i % 2);
                MagnumParticleHandler.SpawnParticle(starburst);
                
                // Add sparkle accents
                Vector2 offset = Main.rand.NextVector2Circular(12f * progress, 12f * progress);
                CustomParticles.GenericFlare(position + offset, burstColor, 0.25f * scale, 14 + i * 2);
            }
            
            // Star burst particles
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 12f) * scale;
                Color sparkColor = GetCelestialGradient((float)i / 16f);
                var spark = new GlowSparkParticle(position, vel, sparkColor, 0.35f * scale, 20);
                MagnumParticleHandler.SpawnParticle(spark);
            }
            
            // Music notes scattered
            SpawnMusicNoteBurst(position, 6, 4f * scale);
            
            // Glyphs for celestial feel
            SpawnGlyphBurst(position, 4, 5f * scale, 0.45f * scale);
        }
        
        /// <summary>
        /// Spawns an impact effect with celestial radiance.
        /// </summary>
        public static void SpawnCelestialImpact(Vector2 position, float scale = 1f)
        {
            // Core flash
            CustomParticles.GenericFlare(position, StarWhite, 0.8f * scale, 18);
            CustomParticles.GenericFlare(position, Gold, 0.6f * scale, 16);
            
            // Starburst layers (replacing banned HaloRing)
            var violetBurst = new StarBurstParticle(position, Vector2.Zero, Violet, 0.35f * scale, 14);
            MagnumParticleHandler.SpawnParticle(violetBurst);
            var goldBurst = new StarBurstParticle(position, Vector2.Zero, Gold * 0.8f, 0.28f * scale, 12, 1);
            MagnumParticleHandler.SpawnParticle(goldBurst);
            
            // Shattered starlight accents
            SpawnShatteredStarlightBurst(position, 4, 3f * scale, 0.3f * scale, false);
            
            // Sparks outward
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f) * scale;
                Color sparkColor = Main.rand.NextBool() ? Gold : Violet;
                var spark = new GlowSparkParticle(position, vel, sparkColor, 0.28f * scale, 16);
                MagnumParticleHandler.SpawnParticle(spark);
            }
            
            // Dynamic lighting
            Lighting.AddLight(position, Gold.ToVector3() * 1.2f * scale);
        }
        
        /// <summary>
        /// Spawns a cosmic cloud trail behind projectiles.
        /// </summary>
        public static void SpawnCelestialCloudTrail(Vector2 position, Vector2 velocity, float scale = 1f)
        {
            // Multiple layered cloud particles
            for (int layer = 0; layer < 3; layer++)
            {
                float layerProgress = layer / 3f;
                Color cloudColor = Color.Lerp(DeepPurple, Violet, layerProgress);
                float cloudScale = (0.35f + layer * 0.1f) * scale;
                
                Vector2 offset = Main.rand.NextVector2Circular(6f, 6f);
                Vector2 cloudVel = -velocity * (0.05f + layer * 0.02f) + Main.rand.NextVector2Circular(0.8f, 0.8f);
                
                var cloud = new GenericGlowParticle(position + offset, cloudVel, cloudColor * 0.55f, cloudScale, 22, true);
                MagnumParticleHandler.SpawnParticle(cloud);
            }
            
            // Star sparkle in cloud
            if (Main.rand.NextBool(4))
            {
                CustomParticles.GenericFlare(position + Main.rand.NextVector2Circular(10f, 10f), 
                    StarWhite, 0.2f * scale, 10);
            }
        }
        
        /// <summary>
        /// Spawns a burst of celestial glyphs.
        /// </summary>
        public static void SpawnGlyphBurst(Vector2 position, int count, float speed, float scale = 0.4f)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 vel = angle.ToRotationVector2() * speed * Main.rand.NextFloat(0.8f, 1.2f);
                Color glyphColor = Main.rand.NextBool() ? Violet : Gold;
                CustomParticles.Glyph(position + vel * 0.3f, glyphColor, scale, -1);
            }
        }
        
        /// <summary>
        /// Spawns floating music notes for the musical theme.
        /// </summary>
        public static void SpawnMusicNoteBurst(Vector2 position, int count, float spread)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.4f, 0.4f);
                Vector2 vel = angle.ToRotationVector2() * spread * Main.rand.NextFloat(0.7f, 1.3f);
                Color noteColor = GetCelestialGradient(Main.rand.NextFloat());
                ThemedParticles.MusicNote(position, vel * 0.5f, noteColor, 0.35f, 25);
            }
        }
        
        /// <summary>
        /// Spawns a celestial lightning strike effect.
        /// </summary>
        public static void SpawnCelestialLightningStrike(Vector2 position, float intensity = 1f)
        {
            // Central flash
            CustomParticles.GenericFlare(position, StarWhite, 1.5f * intensity, 12);
            CustomParticles.GenericFlare(position, Gold, 1.1f * intensity, 10);
            
            // Lightning tendrils (simulated with particles)
            for (int i = 0; i < 5; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float length = Main.rand.NextFloat(40f, 80f) * intensity;
                
                for (int j = 0; j < 6; j++)
                {
                    float dist = length * j / 6f;
                    Vector2 lightningPos = position + angle.ToRotationVector2() * dist;
                    lightningPos += Main.rand.NextVector2Circular(8f, 8f);
                    
                    var lightning = new GenericGlowParticle(lightningPos, Vector2.Zero, 
                        Color.Lerp(StarWhite, Gold, (float)j / 6f), 0.2f, 8, true);
                    MagnumParticleHandler.SpawnParticle(lightning);
                }
            }
            
            // Electric dust
            for (int i = 0; i < 12; i++)
            {
                Dust electric = Dust.NewDustPerfect(position, DustID.Electric, 
                    Main.rand.NextVector2Circular(8f, 8f), 0, Gold, 1.2f);
                electric.noGravity = true;
            }
            
            Lighting.AddLight(position, StarWhite.ToVector3() * 2f * intensity);
        }
        
        /// <summary>
        /// Creates the signature Nachtmusik star circle constellation effect.
        /// </summary>
        public static void SpawnConstellationCircle(Vector2 center, float radius, int stars, float rotation)
        {
            for (int i = 0; i < stars; i++)
            {
                float angle = rotation + MathHelper.TwoPi * i / stars;
                Vector2 starPos = center + angle.ToRotationVector2() * radius;
                
                // Star point
                CustomParticles.GenericFlare(starPos, StarWhite, 0.35f, 12);
                
                // Constellation line to next star (particle-based)
                if (i < stars - 1)
                {
                    float nextAngle = rotation + MathHelper.TwoPi * (i + 1) / stars;
                    Vector2 nextPos = center + nextAngle.ToRotationVector2() * radius;
                    
                    for (int j = 1; j < 4; j++)
                    {
                        Vector2 linePos = Vector2.Lerp(starPos, nextPos, j / 4f);
                        var linePart = new GenericGlowParticle(linePos, Vector2.Zero, 
                            Violet * 0.4f, 0.08f, 10, true);
                        MagnumParticleHandler.SpawnParticle(linePart);
                    }
                }
            }
        }
        
        /// <summary>
        /// Spawns orbiting celestial glyphs around a position.
        /// </summary>
        public static void SpawnOrbitingGlyphs(Vector2 center, int count, float radius, float baseAngle)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = baseAngle + MathHelper.TwoPi * i / count;
                Vector2 glyphPos = center + angle.ToRotationVector2() * radius;
                Color glyphColor = GetCelestialGradient((float)i / count);
                CustomParticles.Glyph(glyphPos, glyphColor, 0.4f, -1);
            }
        }
        
        /// <summary>
        /// Creates radiant beam particles for ranged weapon trails.
        /// </summary>
        public static void SpawnRadiantBeamTrail(Vector2 position, Vector2 velocity, float scale = 1f)
        {
            // Core trail
            var core = new GenericGlowParticle(position, -velocity * 0.05f, StarWhite * 0.9f, 0.25f * scale, 15, true);
            MagnumParticleHandler.SpawnParticle(core);
            
            // Outer glow
            var outer = new GenericGlowParticle(position, -velocity * 0.03f, Gold * 0.6f, 0.35f * scale, 18, true);
            MagnumParticleHandler.SpawnParticle(outer);
            
            // Star sparkle accent
            if (Main.rand.NextBool(3))
            {
                Vector2 sparkleOffset = Main.rand.NextVector2Circular(8f, 8f);
                var sparkle = new GenericGlowParticle(position + sparkleOffset, Main.rand.NextVector2Circular(1f, 1f),
                    Violet, 0.18f * scale, 12, true);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
        }
        
        /// <summary>
        /// Creates minion aura particles.
        /// </summary>
        public static void SpawnMinionAura(Vector2 position, float scale = 1f)
        {
            // Ambient glow particles
            if (Main.rand.NextBool(4))
            {
                Vector2 offset = Main.rand.NextVector2Circular(20f * scale, 20f * scale);
                Color auraColor = GetCelestialGradient(Main.rand.NextFloat());
                var aura = new GenericGlowParticle(position + offset, Main.rand.NextVector2Circular(0.5f, 0.5f),
                    auraColor * 0.5f, 0.2f * scale, 20, true);
                MagnumParticleHandler.SpawnParticle(aura);
            }
            
            // Star dust motes rising
            if (Main.rand.NextBool(6))
            {
                Vector2 motePos = position + new Vector2(Main.rand.NextFloat(-15f, 15f) * scale, 10f * scale);
                var mote = new GenericGlowParticle(motePos, new Vector2(0, -0.8f), 
                    StarWhite * 0.6f, 0.12f * scale, 25, true);
                MagnumParticleHandler.SpawnParticle(mote);
            }
        }
        
        #endregion
        
        #region Weapon-Specific Effects
        
        /// <summary>
        /// Swing trail effect for melee weapons.
        /// </summary>
        public static void SpawnMeleeSwingTrail(Vector2 position, float swingAngle, float scale = 1f)
        {
            // Celestial arc sparks
            for (int i = 0; i < 6; i++)
            {
                float angle = swingAngle + MathHelper.Lerp(-0.6f, 0.6f, (float)i / 5f);
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 10f) * scale;
                Color sparkColor = GetCelestialGradient((float)i / 5f);
                var spark = new GlowSparkParticle(position, sparkVel, sparkColor, 0.28f * scale, 14);
                MagnumParticleHandler.SpawnParticle(spark);
            }
            
            // Trailing glow
            var trail = new GenericGlowParticle(position, swingAngle.ToRotationVector2() * 2f,
                Gold * 0.6f, 0.3f * scale, 12, true);
            MagnumParticleHandler.SpawnParticle(trail);
        }
        
        /// <summary>
        /// Muzzle flash for ranged weapons.
        /// </summary>
        public static void SpawnRangedMuzzleFlash(Vector2 position, float direction, float scale = 1f)
        {
            // Core flash
            CustomParticles.GenericFlare(position, StarWhite, 0.7f * scale, 12);
            CustomParticles.GenericFlare(position, Gold, 0.5f * scale, 10);
            
            // Directional sparks
            for (int i = 0; i < 5; i++)
            {
                float angle = direction + Main.rand.NextFloat(-0.4f, 0.4f);
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f) * scale;
                var spark = new GlowSparkParticle(position, sparkVel, Violet, 0.22f * scale, 10);
                MagnumParticleHandler.SpawnParticle(spark);
            }
            
            // Sparkle burst (replacing banned HaloRing)
            var muzzleBurst = new StarBurstParticle(position, Vector2.Zero, Gold * 0.7f, 0.22f * scale, 10, 1);
            MagnumParticleHandler.SpawnParticle(muzzleBurst);
        }
        
        /// <summary>
        /// Magic cast burst effect.
        /// </summary>
        public static void SpawnMagicCastBurst(Vector2 position, float scale = 1f)
        {
            // Glyph circle
            SpawnGlyphBurst(position, 6, 3f * scale, 0.35f * scale);
            
            // Central radiance
            CustomParticles.GenericFlare(position, Violet, 0.8f * scale, 16);
            CustomParticles.GenericFlare(position, Gold * 0.8f, 0.6f * scale, 14);
            
            // Expanding magic starburst (replacing banned HaloRing)
            var magicBurst = new StarBurstParticle(position, Vector2.Zero, Violet, 0.45f * scale, 18);
            MagnumParticleHandler.SpawnParticle(magicBurst);
            var magicBurst2 = new StarBurstParticle(position, Vector2.Zero, NebulaPink * 0.7f, 0.35f * scale, 14, 1);
            MagnumParticleHandler.SpawnParticle(magicBurst2);
            
            // Star particles
            for (int i = 0; i < 8; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(25f * scale, 25f * scale);
                var star = new GenericGlowParticle(position + offset, Main.rand.NextVector2Circular(1f, 1f),
                    StarWhite, 0.2f * scale, 15, true);
                MagnumParticleHandler.SpawnParticle(star);
            }
        }
        
        #endregion
        
        #region Nachtmusik-Specific Signature Effects
        
        /// <summary>
        /// Spawns a dramatic 8-pointed star burst impact - signature Nachtmusik effect.
        /// Uses the new StarBurst textures for celestial impacts.
        /// </summary>
        public static void SpawnStarBurstImpact(Vector2 position, float scale = 1f, int burstCount = 3)
        {
            // Central star burst
            for (int i = 0; i < burstCount; i++)
            {
                float delay = i * 0.1f;
                float burstScale = (1f - i * 0.2f) * scale;
                Color burstColor = GetCelestialGradient((float)i / burstCount);
                
                Vector2 offset = Main.rand.NextVector2Circular(4f * i, 4f * i);
                var starBurst = new StarBurstParticle(position + offset, Vector2.Zero, burstColor, burstScale * 0.5f, 18 + i * 4, i % 2);
                MagnumParticleHandler.SpawnParticle(starBurst);
            }
            
            // Core white flash
            CustomParticles.GenericFlare(position, StarWhite, 0.9f * scale, 15);
            
            // Golden starburst (replacing banned HaloRing)
            var goldenBurst = new StarBurstParticle(position, Vector2.Zero, Gold, 0.4f * scale, 16, 0);
            MagnumParticleHandler.SpawnParticle(goldenBurst);
            
            // Outer violet sparkle spray
            for (int j = 0; j < 6; j++)
            {
                float sparkAngle = MathHelper.TwoPi * j / 6f;
                Vector2 sparkVel = sparkAngle.ToRotationVector2() * 4f * scale;
                var violetSparkle = new GlowSparkParticle(position, sparkVel, Violet * 0.7f, 0.22f * scale, 20);
                MagnumParticleHandler.SpawnParticle(violetSparkle);
            }
            
            Lighting.AddLight(position, StarWhite.ToVector3() * 1.5f * scale);
        }
        
        /// <summary>
        /// Spawns shattered starlight fragments that scatter like crystalline shards.
        /// Perfect for weapon hit effects and projectile deaths.
        /// </summary>
        public static void SpawnShatteredStarlightBurst(Vector2 position, int count, float speed, float scale = 1f, bool hasGravity = true)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.3f, 0.3f);
                float fragmentSpeed = speed * Main.rand.NextFloat(0.7f, 1.3f);
                Vector2 vel = angle.ToRotationVector2() * fragmentSpeed;
                
                Color fragmentColor = GetCelestialGradient(Main.rand.NextFloat());
                float fragmentScale = scale * Main.rand.NextFloat(0.3f, 0.5f);
                
                var fragment = new ShatteredStarlightParticle(position, vel, fragmentColor, fragmentScale, 
                    Main.rand.Next(20, 35), hasGravity, hasGravity ? 0.12f : 0f);
                MagnumParticleHandler.SpawnParticle(fragment);
            }
            
            // Central glow
            CustomParticles.GenericFlare(position, StarWhite * 0.8f, 0.5f * scale, 12);
        }
        
        /// <summary>
        /// Spawns a grand celestial weapon impact combining star bursts and shattered starlight.
        /// The signature hit effect for Nachtmusik melee weapons.
        /// </summary>
        public static void SpawnGrandCelestialImpact(Vector2 position, float scale = 1f)
        {
            // Layer 1: Central star burst explosion
            SpawnStarBurstImpact(position, scale, 4);
            
            // Layer 2: Shattered starlight fragments flying outward
            SpawnShatteredStarlightBurst(position, 12, 8f * scale, scale, true);
            
            // Layer 3: Celestial sparks
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 10f) * scale;
                Color sparkColor = Main.rand.NextBool() ? Gold : Violet;
                var spark = new GlowSparkParticle(position, vel, sparkColor, 0.3f * scale, 16);
                MagnumParticleHandler.SpawnParticle(spark);
            }
            
            // Layer 4: Music note accents
            SpawnMusicNoteBurst(position, 4, 3f * scale);
            
            // Layer 5: Cascading starburst cascade (replacing banned HaloRing)
            for (int i = 0; i < 5; i++)
            {
                Color cascadeColor = GetCelestialGradient((float)i / 5f);
                float cascadeScale = (0.35f + i * 0.08f) * scale;
                var cascadeBurst = new StarBurstParticle(position, Vector2.Zero, cascadeColor * 0.8f, cascadeScale, 14 + i * 3, i % 2);
                MagnumParticleHandler.SpawnParticle(cascadeBurst);
                
                // Shattered starlight accent per layer
                if (i % 2 == 0)
                {
                    float fragAngle = MathHelper.TwoPi * i / 5f;
                    Vector2 fragVel = fragAngle.ToRotationVector2() * 5f * scale;
                    var fragment = new ShatteredStarlightParticle(position, fragVel, cascadeColor, 0.25f * scale, 18, false, 0f);
                    MagnumParticleHandler.SpawnParticle(fragment);
                }
            }
            
            // Intense lighting burst
            Lighting.AddLight(position, Gold.ToVector3() * 2f * scale);
        }
        
        /// <summary>
        /// Spawns a projectile trail with star burst accents for ranged weapons.
        /// </summary>
        public static void SpawnStarTrailEffect(Vector2 position, Vector2 velocity, float scale = 1f)
        {
            // Base celestial cloud trail
            SpawnCelestialCloudTrail(position, velocity, scale);
            
            // Occasional star burst sparkle in trail
            if (Main.rand.NextBool(5))
            {
                var starBurst = new StarBurstParticle(position + Main.rand.NextVector2Circular(8f, 8f), 
                    -velocity * 0.08f, Gold, 0.2f * scale, 12, Main.rand.Next(2));
                MagnumParticleHandler.SpawnParticle(starBurst);
            }
            
            // Occasional shattered fragment
            if (Main.rand.NextBool(8))
            {
                Vector2 fragVel = -velocity * 0.1f + Main.rand.NextVector2Circular(1.5f, 1.5f);
                var fragment = new ShatteredStarlightParticle(position, fragVel, Violet, 0.18f * scale, 18, false, 0f);
                MagnumParticleHandler.SpawnParticle(fragment);
            }
        }
        
        /// <summary>
        /// Spawns a celestial projectile death explosion with maximum visual impact.
        /// </summary>
        public static void SpawnCelestialProjectileDeath(Vector2 position, float scale = 1f)
        {
            // Multi-layered star burst
            for (int i = 0; i < 3; i++)
            {
                Color burstColor = GetCelestialGradient((float)i / 3f);
                var burst = new StarBurstParticle(position, Vector2.Zero, burstColor, (0.6f - i * 0.12f) * scale, 20 + i * 5);
                MagnumParticleHandler.SpawnParticle(burst);
            }
            
            // Dramatic shattered starlight spray
            SpawnShatteredStarlightBurst(position, 16, 10f * scale, scale, true);
            
            // Core celestial explosion
            SpawnCelestialExplosion(position, scale * 0.8f);
        }
        
        /// <summary>
        /// Boss attack warning circle with star points.
        /// </summary>
        public static void SpawnBossWarningStarCircle(Vector2 center, float radius, float progress)
        {
            int starCount = 8;
            float rotation = Main.GameUpdateCount * 0.02f;
            
            // Star points around circle
            for (int i = 0; i < starCount; i++)
            {
                float angle = rotation + MathHelper.TwoPi * i / starCount;
                Vector2 starPos = center + angle.ToRotationVector2() * radius;
                
                Color starColor = Color.Lerp(Violet, Gold, progress);
                var burst = new StarBurstParticle(starPos, Vector2.Zero, starColor, 0.25f + progress * 0.15f, 8);
                MagnumParticleHandler.SpawnParticle(burst);
            }
            
            // Inner constellation glow connecting stars (replacing banned HaloRing)
            if (progress > 0.3f)
            {
                // Glow particles at star connection points
                for (int j = 0; j < starCount; j++)
                {
                    float connectAngle = rotation + MathHelper.TwoPi * j / starCount;
                    float nextAngle = rotation + MathHelper.TwoPi * ((j + 1) % starCount) / starCount;
                    Vector2 midPoint = center + ((connectAngle.ToRotationVector2() + nextAngle.ToRotationVector2()) * 0.5f).SafeNormalize(Vector2.Zero) * radius * 0.8f;
                    var connector = new GenericGlowParticle(midPoint, Vector2.Zero, Gold * (progress * 0.6f), 0.15f, 8, true);
                    MagnumParticleHandler.SpawnParticle(connector);
                }
            }
        }
        
        /// <summary>
        /// Boss phase transition celestial explosion.
        /// </summary>
        public static void SpawnBossPhaseTransition(Vector2 position, float scale = 1.5f)
        {
            // Massive central star bursts
            for (int layer = 0; layer < 5; layer++)
            {
                float delay = layer * 2f;
                Vector2 offset = Main.rand.NextVector2Circular(10f, 10f);
                Color layerColor = GetCelestialGradient((float)layer / 5f);
                var burst = new StarBurstParticle(position + offset, Vector2.Zero, layerColor, (0.8f - layer * 0.1f) * scale, 25 + layer * 5);
                MagnumParticleHandler.SpawnParticle(burst);
            }
            
            // Shattered starlight explosion
            SpawnShatteredStarlightBurst(position, 24, 15f * scale, scale * 1.2f, true);
            
            // Constellation circle
            SpawnConstellationCircle(position, 80f * scale, 12, Main.GameUpdateCount * 0.03f);
            
            // Glyph circle
            SpawnOrbitingGlyphs(position, 8, 60f * scale, Main.GameUpdateCount * 0.02f);
            
            // Massive starburst cascade (replacing banned HaloRing)
            for (int i = 0; i < 8; i++)
            {
                Color cascadeColor = GetCelestialGradient((float)i / 8f);
                float cascadeScale = (0.5f + i * 0.1f) * scale;
                var phaseBurst = new StarBurstParticle(position, Vector2.Zero, cascadeColor, cascadeScale, 20 + i * 4, i % 2);
                MagnumParticleHandler.SpawnParticle(phaseBurst);
                
                // Add shattered starlight fragments radiating outward
                float fragAngle = MathHelper.TwoPi * i / 8f;
                Vector2 fragVel = fragAngle.ToRotationVector2() * (8f + i * 2f) * scale;
                var fragment = new ShatteredStarlightParticle(position, fragVel, cascadeColor, 0.35f * scale, 25, true, 0.1f);
                MagnumParticleHandler.SpawnParticle(fragment);
            }
            
            // Music notes scattered
            SpawnMusicNoteBurst(position, 12, 8f * scale);
            
            // Intense lighting
            Lighting.AddLight(position, StarWhite.ToVector3() * 3f * scale);
        }
        
        #endregion
    }
}
