using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.Audio;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.Fate
{
    /// <summary>
    /// Comprehensive VFX helper system for Fate weapons featuring:
    /// - Celestial cosmic aesthetic (Ark of the Cosmos inspired)
    /// - Ancient glyphs, star particles, cosmic cloud energy
    /// - Music note integration
    /// - Cosmic lightning effects
    /// - Constellation patterns
    /// - Cinematic star formation attack effect
    /// </summary>
    public static class FateCosmicVFX
    {
        // ========== FATE CELESTIAL COLOR PALETTE ==========
        public static readonly Color CosmicBlack = new Color(15, 5, 20);
        public static readonly Color FateDarkPink = new Color(180, 50, 100);
        public static readonly Color FateBrightRed = new Color(255, 60, 80);
        public static readonly Color FatePurple = new Color(120, 30, 140);
        public static readonly Color FateWhite = new Color(255, 255, 255);
        public static readonly Color FateStarGold = new Color(255, 230, 180);
        public static readonly Color FateCyan = new Color(100, 200, 255);
        public static readonly Color FateNebulaPurple = new Color(160, 80, 200);

        /// <summary>
        /// Get the Fate celestial gradient color
        /// </summary>
        public static Color GetCosmicGradient(float progress)
        {
            if (progress < 0.3f)
                return Color.Lerp(CosmicBlack, FateDarkPink, progress / 0.3f);
            else if (progress < 0.6f)
                return Color.Lerp(FateDarkPink, FateBrightRed, (progress - 0.3f) / 0.3f);
            else if (progress < 0.85f)
                return Color.Lerp(FateBrightRed, FatePurple, (progress - 0.6f) / 0.25f);
            else
                return Color.Lerp(FatePurple, FateWhite, (progress - 0.85f) / 0.15f);
        }

        /// <summary>
        /// Get prismatic cosmic color cycling through the palette
        /// </summary>
        public static Color GetPrismaticColor(float time, float offset = 0f)
        {
            float cycle = (time * 0.5f + offset) % 1f;
            return GetCosmicGradient(cycle);
        }

        // ========== COSMIC CLOUD EFFECTS (Ark of the Cosmos Style) ==========

        /// <summary>
        /// Spawn billowing cosmic cloud trail particles
        /// </summary>
        public static void SpawnCosmicCloudTrail(Vector2 position, Vector2 velocity, float scale = 1f)
        {
            // Multiple layered cloud particles for nebula effect
            for (int layer = 0; layer < 4; layer++)
            {
                float layerProgress = layer / 4f;
                Color cloudColor = Color.Lerp(CosmicBlack, FatePurple, layerProgress) * 0.6f;
                float particleScale = (0.3f + layer * 0.12f) * scale;

                Vector2 offset = Main.rand.NextVector2Circular(8f * scale, 8f * scale);
                Vector2 cloudVel = -velocity * (0.05f + layer * 0.02f) + Main.rand.NextVector2Circular(1.5f, 1.5f);

                var cloud = new GenericGlowParticle(position + offset, cloudVel, cloudColor, particleScale, 28, true);
                MagnumParticleHandler.SpawnParticle(cloud);
            }

            // Star points scattered in the cloud
            if (Main.rand.NextBool(3))
            {
                Vector2 starOffset = Main.rand.NextVector2Circular(15f * scale, 15f * scale);
                var star = new GenericGlowParticle(position + starOffset, Main.rand.NextVector2Circular(0.5f, 0.5f), 
                    FateWhite, 0.2f * scale, 15, true);
                MagnumParticleHandler.SpawnParticle(star);
            }
        }

        /// <summary>
        /// Spawn cosmic cloud burst (explosion of nebula energy) with enhanced multi-layer bloom
        /// </summary>
        public static void SpawnCosmicCloudBurst(Vector2 position, float scale = 1f, int cloudCount = 16)
        {
            // === ENHANCED CENTRAL FLASH WITH MULTI-LAYER BLOOM ===
            EnhancedParticles.BloomFlare(position, FateWhite, 0.8f * scale, 20, 4, 1.2f);
            EnhancedParticles.BloomFlare(position, FateDarkPink, 0.6f * scale, 18, 3, 1.0f);
            
            for (int i = 0; i < cloudCount; i++)
            {
                float angle = MathHelper.TwoPi * i / cloudCount + Main.rand.NextFloat(-0.2f, 0.2f);
                float speed = Main.rand.NextFloat(3f, 8f) * scale;
                Vector2 cloudVel = angle.ToRotationVector2() * speed;
                
                Color cloudColor = Color.Lerp(CosmicBlack, FatePurple, Main.rand.NextFloat()) * 0.5f;
                float particleScale = Main.rand.NextFloat(0.3f, 0.6f) * scale;
                
                // Enhanced cloud particle with bloom
                var cloud = EnhancedParticlePool.GetParticle()
                    .Setup(CustomParticleSystem.RandomGlow(), position, cloudVel, cloudColor, particleScale, 35)
                    .WithBloom(2, 0.6f)
                    .WithDrag(0.96f);
                EnhancedParticlePool.SpawnParticle(cloud);
            }

            // Enhanced central star burst with bloom
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 starVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f) * scale;
                
                var star = EnhancedParticlePool.GetParticle()
                    .Setup(CustomParticleSystem.RandomFlare(), position, starVel, FateWhite * 0.8f, 0.25f * scale, 20)
                    .WithBloom(3, 0.8f)
                    .WithShineFlare(0.5f);
                EnhancedParticlePool.SpawnParticle(star);
            }
        }

        // ========== GLYPH EFFECTS ==========

        /// <summary>
        /// Spawn orbiting glyphs around a position
        /// </summary>
        public static void SpawnOrbitingGlyphs(Vector2 center, int count, float radius, float rotationOffset = 0f, float scale = 0.4f)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = rotationOffset + MathHelper.TwoPi * i / count;
                Vector2 glyphPos = center + angle.ToRotationVector2() * radius;
                Color glyphColor = GetCosmicGradient((float)i / count);
                
                var glyph = new GenericGlowParticle(glyphPos, Vector2.Zero, glyphColor, scale, 12, true);
                MagnumParticleHandler.SpawnParticle(glyph);
            }
        }

        /// <summary>
        /// Spawn a glyph burst explosion
        /// </summary>
        public static void SpawnGlyphBurst(Vector2 position, int count, float speed, float scale = 0.35f)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.1f, 0.1f);
                Vector2 vel = angle.ToRotationVector2() * speed * Main.rand.NextFloat(0.8f, 1.2f);
                Color glyphColor = GetCosmicGradient((float)i / count);
                
                var glyph = new GenericGlowParticle(position, vel, glyphColor, scale, 25, true);
                MagnumParticleHandler.SpawnParticle(glyph);
            }
        }

        // ========== STAR PARTICLE EFFECTS ==========

        /// <summary>
        /// Spawn twinkling star particles
        /// </summary>
        public static void SpawnStarSparkles(Vector2 position, int count, float radius, float scale = 0.25f)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(radius, radius);
                Vector2 vel = Main.rand.NextVector2Circular(0.8f, 0.8f);
                Color starColor = Main.rand.NextBool(3) ? FateStarGold : FateWhite;
                float starScale = scale * Main.rand.NextFloat(0.6f, 1.2f);
                
                var star = new GenericGlowParticle(position + offset, vel, starColor, starScale, 20, true);
                MagnumParticleHandler.SpawnParticle(star);
            }
        }

        /// <summary>
        /// Spawn a constellation pattern of stars with connecting lines and enhanced bloom
        /// </summary>
        public static void SpawnConstellationBurst(Vector2 center, int starCount, float radius, float scale = 1f)
        {
            List<Vector2> starPositions = new List<Vector2>();

            // Place stars in a pattern with enhanced bloom
            for (int i = 0; i < starCount; i++)
            {
                float angle = MathHelper.TwoPi * i / starCount + Main.rand.NextFloat(-0.3f, 0.3f);
                float dist = radius * Main.rand.NextFloat(0.5f, 1f);
                Vector2 starPos = center + angle.ToRotationVector2() * dist;
                starPositions.Add(starPos);

                // Enhanced main star flare with multi-layer bloom
                EnhancedParticles.BloomFlare(starPos, FateWhite, 0.4f * scale, 30, 3, 0.9f);
                
                // Glyph at star position with bloom
                var glyph = EnhancedParticlePool.GetParticle()
                    .Setup(CustomParticleSystem.RandomGlow(), starPos, Vector2.Zero, FateDarkPink * 0.8f, 0.25f * scale, 25)
                    .WithBloom(2, 0.6f);
                EnhancedParticlePool.SpawnParticle(glyph);
            }

            // Draw connecting line particles between stars
            for (int i = 0; i < starPositions.Count; i++)
            {
                int next = (i + 1) % starPositions.Count;
                SpawnConstellationLine(starPositions[i], starPositions[next], FatePurple * 0.5f);
            }
        }

        /// <summary>
        /// Spawn particles along a line to create constellation connection
        /// </summary>
        public static void SpawnConstellationLine(Vector2 start, Vector2 end, Color color)
        {
            float dist = Vector2.Distance(start, end);
            int segments = (int)(dist / 6f);
            
            for (int i = 0; i < segments; i++)
            {
                float progress = (float)i / segments;
                Vector2 pos = Vector2.Lerp(start, end, progress);
                var line = new GenericGlowParticle(pos, Main.rand.NextVector2Circular(0.3f, 0.3f), color * 0.6f, 0.08f, 20, true);
                MagnumParticleHandler.SpawnParticle(line);
            }
        }

        // ========== MUSIC NOTE EFFECTS ==========

        /// <summary>
        /// Spawn floating cosmic music notes
        /// </summary>
        public static void SpawnCosmicMusicNotes(Vector2 position, int count, float radius, float scale = 0.3f)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(radius, radius);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-2f, -0.5f));
                Color noteColor = GetCosmicGradient(Main.rand.NextFloat());
                
                var note = new GenericGlowParticle(position + offset, vel, noteColor, scale, 35, true);
                MagnumParticleHandler.SpawnParticle(note);
            }
        }

        /// <summary>
        /// Spawn music note explosion with cosmic colors
        /// </summary>
        public static void SpawnMusicNoteExplosion(Vector2 position, int count, float speed = 5f)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 vel = angle.ToRotationVector2() * speed * Main.rand.NextFloat(0.7f, 1.3f);
                Color noteColor = GetCosmicGradient((float)i / count);
                
                var note = new GenericGlowParticle(position, vel, noteColor, 0.35f, 30, true);
                MagnumParticleHandler.SpawnParticle(note);
            }
        }

        // ========== COSMIC LIGHTNING EFFECTS ==========

        /// <summary>
        /// Draw cosmic lightning between two points
        /// </summary>
        public static void DrawCosmicLightning(Vector2 start, Vector2 end, int segments = 12, float amplitude = 40f, 
            int branches = 3, float branchChance = 0.4f, Color? primaryColor = null, Color? secondaryColor = null)
        {
            Color primary = primaryColor ?? FateDarkPink;
            Color secondary = secondaryColor ?? FateWhite;
            
            // Calculate lightning path
            List<Vector2> points = new List<Vector2> { start };
            Vector2 direction = (end - start).SafeNormalize(Vector2.Zero);
            Vector2 perpendicular = direction.RotatedBy(MathHelper.PiOver2);
            float totalDist = Vector2.Distance(start, end);
            float segmentLength = totalDist / segments;

            Vector2 currentPos = start;
            for (int i = 1; i < segments; i++)
            {
                float progress = (float)i / segments;
                Vector2 basePos = start + direction * (segmentLength * i);
                float offset = Main.rand.NextFloat(-amplitude, amplitude) * (1f - Math.Abs(progress - 0.5f) * 2f);
                currentPos = basePos + perpendicular * offset;
                points.Add(currentPos);

                // Spawn particles along the lightning
                Color particleColor = Color.Lerp(primary, secondary, progress);
                var spark = new GlowSparkParticle(currentPos, Main.rand.NextVector2Circular(2f, 2f), particleColor, 0.2f, 8);
                MagnumParticleHandler.SpawnParticle(spark);
                
                // Branch lightning
                if (Main.rand.NextFloat() < branchChance && i > 2 && i < segments - 2)
                {
                    float branchAngle = Main.rand.NextFloat(-0.8f, 0.8f);
                    Vector2 branchDir = direction.RotatedBy(branchAngle);
                    float branchLength = Main.rand.NextFloat(30f, 60f);
                    
                    for (int b = 0; b < 4; b++)
                    {
                        Vector2 branchPos = currentPos + branchDir * (branchLength * b / 4f) + 
                            perpendicular.RotatedBy(branchAngle) * Main.rand.NextFloat(-10f, 10f);
                        var branchSpark = new GlowSparkParticle(branchPos, branchDir * 2f, primary * 0.6f, 0.12f, 6);
                        MagnumParticleHandler.SpawnParticle(branchSpark);
                    }
                }
            }
            points.Add(end);

            // Impact flare at end
            var endFlare = new GenericGlowParticle(end, Vector2.Zero, secondary, 0.5f, 10, true);
            MagnumParticleHandler.SpawnParticle(endFlare);
            
            Lighting.AddLight(end, primary.ToVector3() * 0.8f);
        }

        /// <summary>
        /// Spawn cosmic lightning strike from above
        /// </summary>
        public static void SpawnCosmicLightningStrike(Vector2 targetPos, float scale = 1f)
        {
            Vector2 startPos = targetPos + new Vector2(Main.rand.NextFloat(-30f, 30f), -400f);
            DrawCosmicLightning(startPos, targetPos, 16, 50f * scale, 4, 0.5f);
            
            // Ground impact effects
            SpawnCosmicCloudBurst(targetPos, scale * 0.6f, 12);
            SpawnGlyphBurst(targetPos, 6, 4f * scale, 0.3f);
            SpawnStarSparkles(targetPos, 8, 30f * scale, 0.3f);
            
            // Sound
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f, Volume = 0.8f }, targetPos);
        }

        // ========== CINEMATIC STAR CIRCLE EFFECT ==========

        /// <summary>
        /// Create the cinematic star circle formation that appears when attacking
        /// This spawns a projectile that handles the animation
        /// </summary>
        public static void TriggerStarCircleEffect(Player player)
        {
            // Only trigger occasionally to avoid spam
            if (Main.rand.NextBool(8)) // 1 in 8 chance per attack
            {
                Projectile.NewProjectile(
                    player.GetSource_FromThis(),
                    player.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<CosmicStarCircleEffect>(),
                    0,
                    0f,
                    player.whoAmI
                );
            }
        }

        // ========== SPECTRAL WEAPON EFFECTS ==========

        /// <summary>
        /// Spawn spectral sword trail effect (white core with pink energy)
        /// </summary>
        public static void SpawnSpectralSwordTrail(Vector2 position, Vector2 velocity, float scale = 1f)
        {
            // White core trail
            var core = new GenericGlowParticle(position, -velocity * 0.1f, FateWhite * 0.9f, 0.25f * scale, 15, true);
            MagnumParticleHandler.SpawnParticle(core);
            
            // Pink energy emanation
            for (int i = 0; i < 2; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(6f, 6f);
                Vector2 vel = -velocity * 0.05f + Main.rand.NextVector2Circular(1f, 1f);
                Color energyColor = Main.rand.NextBool() ? FateDarkPink : FateBrightRed;
                var energy = new GenericGlowParticle(position + offset, vel, energyColor * 0.7f, 0.2f * scale, 20, true);
                MagnumParticleHandler.SpawnParticle(energy);
            }
            
            // Cosmic cloud wisps
            if (Main.rand.NextBool(3))
            {
                SpawnCosmicCloudTrail(position, velocity, scale * 0.5f);
            }
        }

        /// <summary>
        /// Spawn spectral weapon aura around player
        /// </summary>
        public static void SpawnSpectralAura(Vector2 center, float scale = 1f)
        {
            float angle = Main.GameUpdateCount * 0.03f;
            float radius = 35f * scale;
            
            for (int i = 0; i < 3; i++)
            {
                float particleAngle = angle + MathHelper.TwoPi * i / 3f;
                Vector2 pos = center + particleAngle.ToRotationVector2() * radius;
                Color auraColor = GetCosmicGradient((float)i / 3f + Main.GameUpdateCount * 0.01f % 1f);
                
                var aura = new GenericGlowParticle(pos, particleAngle.ToRotationVector2() * 0.5f, auraColor * 0.5f, 0.2f * scale, 10, true);
                MagnumParticleHandler.SpawnParticle(aura);
            }
        }

        // ========== COSMIC BEAM EFFECTS ==========

        /// <summary>
        /// Spawn particles for a cosmic beam
        /// </summary>
        public static void SpawnCosmicBeamParticles(Vector2 start, Vector2 end, float intensity = 1f)
        {
            float dist = Vector2.Distance(start, end);
            int particleCount = (int)(dist / 10f);
            Vector2 direction = (end - start).SafeNormalize(Vector2.Zero);
            
            for (int i = 0; i < particleCount; i++)
            {
                float progress = (float)i / particleCount;
                Vector2 pos = Vector2.Lerp(start, end, progress) + Main.rand.NextVector2Circular(5f * intensity, 5f * intensity);
                Vector2 vel = direction.RotatedByRandom(0.3f) * Main.rand.NextFloat(1f, 3f);
                Color beamColor = Color.Lerp(FateWhite, FateDarkPink, progress) * intensity;
                
                var particle = new GenericGlowParticle(pos, vel, beamColor, 0.2f * intensity, 15, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }
            
            // Star sparkles along beam
            if (Main.rand.NextBool(2))
            {
                float randomProgress = Main.rand.NextFloat();
                Vector2 starPos = Vector2.Lerp(start, end, randomProgress);
                var star = new GenericGlowParticle(starPos, Vector2.Zero, FateWhite, 0.3f * intensity, 12, true);
                MagnumParticleHandler.SpawnParticle(star);
            }
        }

        // ========== COSMIC EXPLOSION EFFECTS ==========

        /// <summary>
        /// Spawn a massive cosmic explosion with all the celestial elements and ENHANCED MULTI-LAYER BLOOM
        /// </summary>
        public static void SpawnCosmicExplosion(Vector2 position, float scale = 1f)
        {
            // === ENHANCED CORE FLASH WITH MULTI-LAYER BLOOM ===
            EnhancedParticles.BloomFlare(position, FateWhite, 1.0f * scale, 20, 4, 1.5f);
            EnhancedParticles.BloomFlare(position, FateDarkPink, 0.8f * scale, 18, 3, 1.2f);
            EnhancedParticles.BloomFlare(position, FatePurple, 0.6f * scale, 16, 2, 1.0f);
            
            // Enhanced cosmic cloud burst with bloom
            SpawnCosmicCloudBurst(position, scale, 20);
            
            // Enhanced glyph burst
            SpawnGlyphBurst(position, 8, 6f * scale, 0.4f);
            
            // Enhanced star sparkles with bloom
            for (int i = 0; i < 15; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(50f * scale, 50f * scale);
                EnhancedParticles.ShineFlare(position + offset, FateWhite, 0.3f * scale, 25);
            }
            
            // Music notes
            SpawnMusicNoteExplosion(position, 8, 5f * scale);
            
            // Constellation burst
            SpawnConstellationBurst(position, 6, 60f * scale, scale);
            
            // Enhanced halo rings with gradient
            for (int ring = 0; ring < 4; ring++)
            {
                float ringProgress = ring / 4f;
                Color ringColor = GetCosmicGradient(ringProgress);
                float ringScale = (0.3f + ring * 0.2f) * scale;
                
                for (int i = 0; i < 12; i++)
                {
                    float angle = MathHelper.TwoPi * i / 12f;
                    Vector2 ringPos = position + angle.ToRotationVector2() * (30f + ring * 20f) * scale;
                    var ringParticle = new GenericGlowParticle(ringPos, angle.ToRotationVector2() * 2f, ringColor, ringScale, 20 + ring * 5, true);
                    MagnumParticleHandler.SpawnParticle(ringParticle);
                }
            }
            
            Lighting.AddLight(position, FateDarkPink.ToVector3() * 2f * scale);
        }

        // ========== COSMIC FLAME EFFECTS ==========

        /// <summary>
        /// Spawn cosmic flames that burn with celestial energy
        /// </summary>
        public static void SpawnCosmicFlames(Vector2 position, int count, float radius, float scale = 1f)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(radius, radius);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-3f, -1f));
                
                // Multi-layer flame
                Color flameColor = GetCosmicGradient(Main.rand.NextFloat());
                var flame = new GenericGlowParticle(position + offset, vel, flameColor, 0.3f * scale, 25, true);
                MagnumParticleHandler.SpawnParticle(flame);
                
                // Inner white-hot core
                if (Main.rand.NextBool(3))
                {
                    var core = new GenericGlowParticle(position + offset, vel * 0.8f, FateWhite * 0.8f, 0.15f * scale, 18, true);
                    MagnumParticleHandler.SpawnParticle(core);
                }
            }
        }

        /// <summary>
        /// Spawn cosmic electricity sparks
        /// </summary>
        public static void SpawnCosmicElectricity(Vector2 position, int count, float range, float scale = 1f)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 startOffset = Main.rand.NextVector2Circular(range * 0.3f, range * 0.3f);
                Vector2 endOffset = Main.rand.NextVector2Circular(range, range);
                
                DrawCosmicLightning(
                    position + startOffset, 
                    position + endOffset, 
                    6, 
                    15f * scale, 
                    1, 
                    0.2f,
                    FateCyan,
                    FateWhite
                );
            }
        }
    }

    /// <summary>
    /// Cinematic projectile effect: Stars appear in a circle, connect, glow, then explode
    /// </summary>
    public class CosmicStarCircleEffect : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";

        private const int FormationTime = 40;
        private const int ConnectionTime = 20;
        private const int GlowTime = 20;
        private const int ExplosionTime = 15;
        private const int TotalDuration = FormationTime + ConnectionTime + GlowTime + ExplosionTime;
        
        private const int StarCount = 12;
        private const float StarRadius = 80f; // ~5 blocks

        private List<Vector2> starPositions = new List<Vector2>();
        private bool initialized = false;

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = TotalDuration;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            Projectile.Center = owner.Center;

            if (!initialized)
            {
                // Initialize star positions in a circle
                for (int i = 0; i < StarCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / StarCount;
                    starPositions.Add(angle.ToRotationVector2() * StarRadius);
                }
                initialized = true;
            }

            int timeElapsed = TotalDuration - Projectile.timeLeft;
            float rotationOffset = Main.GameUpdateCount * 0.02f;

            // Phase 1: Formation - stars appear one by one
            if (timeElapsed < FormationTime)
            {
                float formationProgress = (float)timeElapsed / FormationTime;
                int starsToShow = (int)(StarCount * formationProgress);

                for (int i = 0; i < starsToShow; i++)
                {
                    float angle = MathHelper.TwoPi * i / StarCount + rotationOffset;
                    Vector2 starPos = owner.Center + angle.ToRotationVector2() * StarRadius;
                    
                    // Star particle
                    float starAlpha = Math.Min(1f, (formationProgress - (float)i / StarCount) * StarCount);
                    Color starColor = FateCosmicVFX.FateWhite * starAlpha;
                    var star = new GenericGlowParticle(starPos, Vector2.Zero, starColor, 0.35f, 5, true);
                    MagnumParticleHandler.SpawnParticle(star);
                    
                    // Glyph at each star
                    if (Main.rand.NextBool(4))
                    {
                        Color glyphColor = FateCosmicVFX.FateDarkPink * starAlpha * 0.7f;
                        var glyph = new GenericGlowParticle(starPos, Vector2.Zero, glyphColor, 0.25f, 8, true);
                        MagnumParticleHandler.SpawnParticle(glyph);
                    }
                }
            }
            // Phase 2: Connection - lines form between stars
            else if (timeElapsed < FormationTime + ConnectionTime)
            {
                float connectionProgress = (float)(timeElapsed - FormationTime) / ConnectionTime;
                
                for (int i = 0; i < StarCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / StarCount + rotationOffset;
                    Vector2 starPos = owner.Center + angle.ToRotationVector2() * StarRadius;
                    
                    // Draw all stars
                    var star = new GenericGlowParticle(starPos, Vector2.Zero, FateCosmicVFX.FateWhite, 0.4f, 5, true);
                    MagnumParticleHandler.SpawnParticle(star);
                    
                    // Draw connecting lines progressively
                    int next = (i + 1) % StarCount;
                    float nextAngle = MathHelper.TwoPi * next / StarCount + rotationOffset;
                    Vector2 nextPos = owner.Center + nextAngle.ToRotationVector2() * StarRadius;
                    
                    float lineProgress = Math.Min(1f, connectionProgress * StarCount - i);
                    if (lineProgress > 0)
                    {
                        Vector2 lineEnd = Vector2.Lerp(starPos, nextPos, lineProgress);
                        int segments = (int)(Vector2.Distance(starPos, lineEnd) / 8f);
                        for (int s = 0; s < segments; s++)
                        {
                            Vector2 segPos = Vector2.Lerp(starPos, lineEnd, (float)s / segments);
                            Color lineColor = FateCosmicVFX.FatePurple * 0.6f;
                            var linePart = new GenericGlowParticle(segPos, Vector2.Zero, lineColor, 0.1f, 5, true);
                            MagnumParticleHandler.SpawnParticle(linePart);
                        }
                    }
                }
            }
            // Phase 3: Glow - entire formation glows brighter
            else if (timeElapsed < FormationTime + ConnectionTime + GlowTime)
            {
                float glowProgress = (float)(timeElapsed - FormationTime - ConnectionTime) / GlowTime;
                float glowIntensity = 1f + glowProgress * 2f;
                
                for (int i = 0; i < StarCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / StarCount + rotationOffset;
                    Vector2 starPos = owner.Center + angle.ToRotationVector2() * StarRadius;
                    
                    // Intensifying stars
                    Color starColor = Color.Lerp(FateCosmicVFX.FateWhite, FateCosmicVFX.FateStarGold, glowProgress);
                    var star = new GenericGlowParticle(starPos, Vector2.Zero, starColor, 0.5f * glowIntensity, 5, true);
                    MagnumParticleHandler.SpawnParticle(star);
                    
                    // All connection lines
                    int next = (i + 1) % StarCount;
                    float nextAngle = MathHelper.TwoPi * next / StarCount + rotationOffset;
                    Vector2 nextPos = owner.Center + nextAngle.ToRotationVector2() * StarRadius;
                    
                    int segments = (int)(Vector2.Distance(starPos, nextPos) / 6f);
                    for (int s = 0; s < segments; s++)
                    {
                        Vector2 segPos = Vector2.Lerp(starPos, nextPos, (float)s / segments);
                        Color lineColor = Color.Lerp(FateCosmicVFX.FateDarkPink, FateCosmicVFX.FateBrightRed, glowProgress) * glowIntensity * 0.5f;
                        var linePart = new GenericGlowParticle(segPos, Vector2.Zero, lineColor, 0.12f * glowIntensity, 5, true);
                        MagnumParticleHandler.SpawnParticle(linePart);
                    }
                }
                
                // Center glow building up
                float centerGlow = 0.3f + glowProgress * 0.5f;
                var centerFlare = new GenericGlowParticle(owner.Center, Vector2.Zero, FateCosmicVFX.FateWhite * glowProgress, centerGlow, 5, true);
                MagnumParticleHandler.SpawnParticle(centerFlare);
                
                Lighting.AddLight(owner.Center, FateCosmicVFX.FateDarkPink.ToVector3() * glowIntensity);
            }
            // Phase 4: Explosion - cosmic gradient explosion
            else
            {
                float explosionProgress = (float)(timeElapsed - FormationTime - ConnectionTime - GlowTime) / ExplosionTime;
                float explosionRadius = StarRadius + explosionProgress * 100f;
                float explosionAlpha = 1f - explosionProgress;
                
                // Expanding star ring
                for (int i = 0; i < StarCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / StarCount + rotationOffset;
                    Vector2 starPos = owner.Center + angle.ToRotationVector2() * explosionRadius;
                    
                    Color explosionColor = FateCosmicVFX.GetCosmicGradient((float)i / StarCount + explosionProgress * 0.5f) * explosionAlpha;
                    var star = new GenericGlowParticle(starPos, angle.ToRotationVector2() * 5f, explosionColor, 0.4f * explosionAlpha, 8, true);
                    MagnumParticleHandler.SpawnParticle(star);
                }
                
                // Cosmic cloud burst on first frame of explosion
                if (timeElapsed == FormationTime + ConnectionTime + GlowTime)
                {
                    FateCosmicVFX.SpawnCosmicCloudBurst(owner.Center, 1.2f, 24);
                    FateCosmicVFX.SpawnMusicNoteExplosion(owner.Center, 12, 8f);
                    SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.3f, Volume = 0.9f }, owner.Center);
                }
                
                Lighting.AddLight(owner.Center, FateCosmicVFX.FateBrightRed.ToVector3() * explosionAlpha * 1.5f);
            }
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }
}
