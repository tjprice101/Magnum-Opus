using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.Audio;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Common.Systems
{
    /// <summary>
    /// SEEKING CRYSTAL PROJECTILES - Elegant, Sparkly Homing Crystals
    /// ==============================================================
    /// These are small, elegant crystalline projectiles that:
    /// - Home toward enemies with fluid, organic movement
    /// - Leave subtle, sparkly prismatic trails
    /// - Have theme-specific color shifting
    /// - Are meant to be spawned in quantities (many small crystals)
    /// 
    /// Design Philosophy: "Small, elegant, sparkly - like tiny gemstones flying through the air"
    /// NOT massive projectiles with overwhelming VFX.
    /// 
    /// Use SeekingCrystalHelper.SpawnCrystals() from any weapon to spawn these.
    /// </summary>
    public static class SeekingCrystalHelper
    {
        /// <summary>
        /// Spawn seeking crystal projectiles from any weapon
        /// </summary>
        /// <param name="source">Entity source for the projectile</param>
        /// <param name="position">Spawn position</param>
        /// <param name="baseVelocity">Initial velocity direction</param>
        /// <param name="damage">Damage per crystal</param>
        /// <param name="knockback">Knockback</param>
        /// <param name="owner">Player whoAmI</param>
        /// <param name="primaryColor">Main crystal color</param>
        /// <param name="secondaryColor">Trail/accent color</param>
        /// <param name="count">Number of crystals to spawn (default 5)</param>
        /// <param name="spreadAngle">Spread angle in radians (default Pi/4)</param>
        /// <param name="homingStrength">How aggressively they home (0.02-0.10)</param>
        /// <param name="crystalSize">Visual scale (0.3-0.8 recommended for elegant look)</param>
        public static void SpawnCrystals(
            IEntitySource source,
            Vector2 position,
            Vector2 baseVelocity,
            int damage,
            float knockback,
            int owner,
            Color primaryColor,
            Color secondaryColor,
            int count = 5,
            float spreadAngle = MathHelper.PiOver4,
            float homingStrength = 0.05f,
            float crystalSize = 0.5f)
        {
            float baseAngle = baseVelocity.ToRotation();
            float speed = baseVelocity.Length();
            
            for (int i = 0; i < count; i++)
            {
                // Spread crystals in an arc
                float angleOffset = count > 1 
                    ? MathHelper.Lerp(-spreadAngle / 2f, spreadAngle / 2f, (float)i / (count - 1))
                    : 0f;
                
                // Add slight randomness for organic feel
                angleOffset += Main.rand.NextFloat(-0.08f, 0.08f);
                float finalSpeed = speed * Main.rand.NextFloat(0.92f, 1.08f);
                
                Vector2 velocity = (baseAngle + angleOffset).ToRotationVector2() * finalSpeed;
                
                int projIndex = Projectile.NewProjectile(
                    source,
                    position + Main.rand.NextVector2Circular(5f, 5f),
                    velocity,
                    ModContent.ProjectileType<SeekingCrystalProjectile>(),
                    damage,
                    knockback,
                    owner
                );
                
                if (projIndex >= 0 && projIndex < Main.maxProjectiles)
                {
                    var proj = Main.projectile[projIndex].ModProjectile as SeekingCrystalProjectile;
                    if (proj != null)
                    {
                        proj.SetColors(primaryColor, secondaryColor);
                        proj.HomingStrength = homingStrength;
                        proj.CrystalSize = crystalSize;
                        // Randomize sparkle variant for each crystal
                        proj.SparkleVariant = Main.rand.Next(1, 16);
                    }
                }
            }
            
            // Subtle spawn burst VFX - elegant, not overwhelming
            for (int i = 0; i < 4; i++)
            {
                var sparkle = new SparkleParticle(
                    position + Main.rand.NextVector2Circular(8f, 8f),
                    Main.rand.NextVector2Circular(2f, 2f),
                    primaryColor,
                    0.25f,
                    15
                );
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
        }
        
        /// <summary>
        /// Spawn a single crystal with custom parameters
        /// </summary>
        public static int SpawnSingleCrystal(
            IEntitySource source,
            Vector2 position,
            Vector2 velocity,
            int damage,
            float knockback,
            int owner,
            Color primaryColor,
            Color secondaryColor,
            float homingStrength = 0.05f,
            float crystalSize = 0.5f)
        {
            int projIndex = Projectile.NewProjectile(
                source,
                position,
                velocity,
                ModContent.ProjectileType<SeekingCrystalProjectile>(),
                damage,
                knockback,
                owner
            );
            
            if (projIndex >= 0 && projIndex < Main.maxProjectiles)
            {
                var proj = Main.projectile[projIndex].ModProjectile as SeekingCrystalProjectile;
                if (proj != null)
                {
                    proj.SetColors(primaryColor, secondaryColor);
                    proj.HomingStrength = homingStrength;
                    proj.CrystalSize = crystalSize;
                    proj.SparkleVariant = Main.rand.Next(1, 16);
                }
            }
            
            return projIndex;
        }
        
        // === THEME-SPECIFIC SPAWNERS - Elegant crystals in theme colors ===
        
        public static void SpawnEroicaCrystals(IEntitySource source, Vector2 pos, Vector2 vel, int damage, float kb, int owner, int count = 5)
        {
            SpawnCrystals(source, pos, vel, damage, kb, owner,
                new Color(255, 200, 80),   // Gold
                new Color(200, 50, 50),    // Scarlet
                count, MathHelper.PiOver4, 0.06f, 0.5f);
        }
        
        public static void SpawnLaCampanellaCrystals(IEntitySource source, Vector2 pos, Vector2 vel, int damage, float kb, int owner, int count = 5)
        {
            SpawnCrystals(source, pos, vel, damage, kb, owner,
                new Color(255, 140, 40),   // Orange
                new Color(255, 200, 80),   // Gold
                count, MathHelper.PiOver4, 0.055f, 0.5f);
        }
        
        public static void SpawnMoonlightCrystals(IEntitySource source, Vector2 pos, Vector2 vel, int damage, float kb, int owner, int count = 5)
        {
            SpawnCrystals(source, pos, vel, damage, kb, owner,
                new Color(140, 100, 220),  // Purple
                new Color(180, 200, 255),  // Light Blue
                count, MathHelper.PiOver4, 0.05f, 0.45f);
        }
        
        public static void SpawnEnigmaCrystals(IEntitySource source, Vector2 pos, Vector2 vel, int damage, float kb, int owner, int count = 5)
        {
            SpawnCrystals(source, pos, vel, damage, kb, owner,
                new Color(140, 60, 200),   // Purple
                new Color(50, 220, 100),   // Green
                count, MathHelper.PiOver4, 0.055f, 0.5f);
        }
        
        public static void SpawnFateCrystals(IEntitySource source, Vector2 pos, Vector2 vel, int damage, float kb, int owner, int count = 5)
        {
            SpawnCrystals(source, pos, vel, damage, kb, owner,
                new Color(200, 80, 120),   // Dark Pink
                new Color(255, 60, 80),    // Bright Red
                count, MathHelper.PiOver4, 0.07f, 0.55f);
        }
        
        public static void SpawnNachtmusikCrystals(IEntitySource source, Vector2 pos, Vector2 vel, int damage, float kb, int owner, int count = 5)
        {
            SpawnCrystals(source, pos, vel, damage, kb, owner,
                new Color(100, 60, 180),   // Deep Purple
                new Color(255, 215, 100),  // Gold
                count, MathHelper.PiOver4, 0.06f, 0.55f);
        }
        
        public static void SpawnSwanLakeCrystals(IEntitySource source, Vector2 pos, Vector2 vel, int damage, float kb, int owner, int count = 5)
        {
            // Rainbow shimmer for Swan Lake
            Color primary = Main.hslToRgb((Main.GameUpdateCount * 0.01f) % 1f, 0.8f, 0.85f);
            SpawnCrystals(source, pos, vel, damage, kb, owner,
                Color.White,
                primary,
                count, MathHelper.PiOver4, 0.055f, 0.5f);
        }
        
        public static void SpawnSpringCrystals(IEntitySource source, Vector2 pos, Vector2 vel, int damage, float kb, int owner, int count = 3)
        {
            SpawnCrystals(source, pos, vel, damage, kb, owner,
                new Color(255, 180, 200),  // Pink
                new Color(180, 255, 180),  // Light Green
                count, MathHelper.Pi / 6f, 0.045f, 0.4f);
        }
        
        public static void SpawnSummerCrystals(IEntitySource source, Vector2 pos, Vector2 vel, int damage, float kb, int owner, int count = 4)
        {
            SpawnCrystals(source, pos, vel, damage, kb, owner,
                new Color(255, 200, 80),   // Sun Gold
                new Color(255, 120, 40),   // Orange
                count, MathHelper.Pi / 5f, 0.05f, 0.45f);
        }
        
        public static void SpawnAutumnCrystals(IEntitySource source, Vector2 pos, Vector2 vel, int damage, float kb, int owner, int count = 4)
        {
            SpawnCrystals(source, pos, vel, damage, kb, owner,
                new Color(255, 140, 50),   // Orange
                new Color(100, 50, 120),   // Decay Purple
                count, MathHelper.Pi / 5f, 0.048f, 0.42f);
        }
        
        public static void SpawnWinterCrystals(IEntitySource source, Vector2 pos, Vector2 vel, int damage, float kb, int owner, int count = 4)
        {
            SpawnCrystals(source, pos, vel, damage, kb, owner,
                new Color(150, 220, 255),  // Ice Blue
                new Color(240, 250, 255),  // Frost White
                count, MathHelper.Pi / 5f, 0.052f, 0.45f);
        }
    }
    
    /// <summary>
    /// THE SEEKING CRYSTAL PROJECTILE
    /// ==============================
    /// A small, elegant, sparkly homing projectile.
    /// 
    /// Design: Like a tiny prismatic gemstone flying through the air
    /// - Small hitbox (8x8)
    /// - Subtle sparkle trail
    /// - Gentle color shifting
    /// - Smooth homing behavior
    /// - Visible but not overwhelming
    /// </summary>
    public class SeekingCrystalProjectile : ModProjectile
    {
        // Colors set by spawner
        private Color _primaryColor = Color.White;
        private Color _secondaryColor = Color.Cyan;
        
        // Customizable properties
        public float HomingStrength = 0.05f;
        public float CrystalSize = 0.5f;
        public float BaseSpeed = 14f;
        public int SparkleVariant = 1;
        
        // Trail tracking - shorter, subtler
        private const int TrailLength = 8;
        private Vector2[] trailPositions = new Vector2[TrailLength];
        private bool trailInitialized = false;
        
        // Animation - subtle pulsing
        private float pulseTimer = 0f;
        private float shimmerPhase = 0f;
        
        public override string Texture => "MagnumOpus/Assets/Particles/PrismaticSparkle1";
        
        public void SetColors(Color primary, Color secondary)
        {
            _primaryColor = primary;
            _secondaryColor = secondary;
        }
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = TrailLength;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override void SetDefaults()
        {
            // Small, elegant hitbox
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = 2; // Can hit 2 enemies
            Projectile.timeLeft = 300; // 5 seconds
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1; // Smooth movement
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
            Projectile.alpha = 0;
        }
        
        public override void AI()
        {
            // Initialize trail
            if (!trailInitialized)
            {
                for (int i = 0; i < TrailLength; i++)
                {
                    trailPositions[i] = Projectile.Center;
                }
                trailInitialized = true;
                shimmerPhase = Main.rand.NextFloat(MathHelper.TwoPi);
            }
            
            // Update trail positions
            for (int i = TrailLength - 1; i > 0; i--)
            {
                trailPositions[i] = trailPositions[i - 1];
            }
            trailPositions[0] = Projectile.Center;
            
            // === SMOOTH FLUID HOMING BEHAVIOR ===
            NPC target = FindBestTarget(800f);
            if (target != null)
            {
                Vector2 toTarget = target.Center - Projectile.Center;
                float distance = toTarget.Length();
                float targetAngle = toTarget.ToRotation();
                float currentAngle = Projectile.velocity.ToRotation();
                
                // Smooth angular interpolation
                float angleDiff = MathHelper.WrapAngle(targetAngle - currentAngle);
                float turnAmount = MathHelper.Clamp(angleDiff, -HomingStrength, HomingStrength);
                
                // Apply turn
                float newAngle = currentAngle + turnAmount;
                float speed = Projectile.velocity.Length();
                
                // Gentle speed adjustment
                float targetSpeed = BaseSpeed * (1f + CrystalSize * 0.2f);
                speed = MathHelper.Lerp(speed, targetSpeed, 0.04f);
                
                Projectile.velocity = newAngle.ToRotationVector2() * speed;
            }
            else
            {
                // No target - maintain speed
                float speed = Projectile.velocity.Length();
                if (speed < BaseSpeed * 0.4f)
                    Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * BaseSpeed * 0.4f;
            }
            
            // === SUBTLE ANIMATION ===
            pulseTimer += 0.08f;
            shimmerPhase += 0.1f;
            
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // === SUBTLE TRAIL PARTICLES ===
            SpawnTrailParticles();
            
            // === GENTLE LIGHTING ===
            float pulse = (float)Math.Sin(pulseTimer) * 0.15f + 0.5f;
            Lighting.AddLight(Projectile.Center, _primaryColor.ToVector3() * pulse * 0.4f);
        }
        
        private void SpawnTrailParticles()
        {
            // Subtle sparkle trail - only occasionally
            if (Main.rand.NextBool(4))
            {
                Color trailColor = Color.Lerp(_primaryColor, _secondaryColor, Main.rand.NextFloat());
                
                var sparkle = new SparkleParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(3f, 3f),
                    -Projectile.velocity * 0.08f,
                    trailColor * 0.7f,
                    0.15f * CrystalSize,
                    12
                );
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // Very subtle glow trail
            if (Main.rand.NextBool(6))
            {
                Color glowColor = Color.Lerp(_primaryColor, _secondaryColor, Main.rand.NextFloat()) * 0.5f;
                
                var glow = new GenericGlowParticle(
                    Projectile.Center,
                    -Projectile.velocity * 0.1f,
                    glowColor,
                    0.12f * CrystalSize,
                    10,
                    true
                );
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Vanilla dust for subtle density
            if (Main.rand.NextBool(5))
            {
                Dust dust = Dust.NewDustPerfect(
                    Projectile.Center,
                    DustID.MagicMirror,
                    -Projectile.velocity * 0.15f,
                    0,
                    _primaryColor,
                    0.5f
                );
                dust.noGravity = true;
            }
        }
        
        private NPC FindBestTarget(float maxRange)
        {
            NPC bestTarget = null;
            float bestScore = float.MaxValue;
            
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.immortal)
                    continue;
                
                float distance = Vector2.Distance(Projectile.Center, npc.Center);
                if (distance > maxRange)
                    continue;
                
                // Score based on distance and angle
                float angleToTarget = (npc.Center - Projectile.Center).ToRotation();
                float currentAngle = Projectile.velocity.ToRotation();
                float angleDiff = Math.Abs(MathHelper.WrapAngle(angleToTarget - currentAngle));
                
                // Lower score = better target
                float score = distance + angleDiff * 150f;
                
                if (score < bestScore)
                {
                    bestScore = score;
                    bestTarget = npc;
                }
            }
            
            return bestTarget;
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            
            // Use the sparkle variant assigned to this crystal
            string texturePath = $"MagnumOpus/Assets/Particles/PrismaticSparkle{SparkleVariant}";
            Texture2D texture = ModContent.Request<Texture2D>(texturePath).Value;
            Vector2 origin = texture.Size() / 2f;
            
            // === DRAW SUBTLE TRAIL ===
            for (int i = TrailLength - 1; i >= 1; i--)
            {
                if (trailPositions[i] == Vector2.Zero)
                    continue;
                
                float progress = (float)i / TrailLength;
                float trailAlpha = (1f - progress) * 0.35f;
                float trailScale = CrystalSize * (1f - progress * 0.5f) * 0.35f;
                
                // Gentle color gradient along trail
                Color trailColor = Color.Lerp(_primaryColor, _secondaryColor, progress);
                trailColor *= trailAlpha;
                trailColor.A = 0; // Additive blending
                
                Vector2 drawPos = trailPositions[i] - Main.screenPosition;
                
                spriteBatch.Draw(
                    texture,
                    drawPos,
                    null,
                    trailColor,
                    Projectile.rotation + progress * 0.5f,
                    origin,
                    trailScale,
                    SpriteEffects.None,
                    0f
                );
            }
            
            // === DRAW MAIN CRYSTAL ===
            float shimmer = (float)Math.Sin(shimmerPhase) * 0.15f + 1f;
            float mainScale = CrystalSize * 0.4f * shimmer;
            Vector2 mainPos = Projectile.Center - Main.screenPosition;
            
            // Outer glow - subtle, 2 layers
            for (int i = 1; i >= 0; i--)
            {
                float glowScale = mainScale * (1f + i * 0.4f);
                float glowAlpha = 0.35f / (i + 1);
                
                // Color shifts based on shimmer
                float colorShift = (float)Math.Sin(shimmerPhase * 0.7f) * 0.5f + 0.5f;
                Color glowColor = Color.Lerp(_primaryColor, _secondaryColor, colorShift);
                glowColor.A = 0;
                
                spriteBatch.Draw(
                    texture,
                    mainPos,
                    null,
                    glowColor * glowAlpha,
                    shimmerPhase * 0.3f,
                    origin,
                    glowScale,
                    SpriteEffects.None,
                    0f
                );
            }
            
            // Bright core
            Color coreColor = Color.White;
            coreColor.A = 0;
            spriteBatch.Draw(
                texture,
                mainPos,
                null,
                coreColor * 0.8f,
                shimmerPhase * 0.5f,
                origin,
                mainScale * 0.5f,
                SpriteEffects.None,
                0f
            );
            
            return false;
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Subtle impact burst - elegant, not overwhelming
            CustomParticles.GenericFlare(target.Center, Color.White * 0.6f, 0.35f, 12);
            CustomParticles.GenericFlare(target.Center, _primaryColor * 0.7f, 0.3f, 10);
            
            // Small sparkle burst
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f;
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                Color burstColor = Color.Lerp(_primaryColor, _secondaryColor, (float)i / 4f);
                
                var sparkle = new SparkleParticle(
                    target.Center,
                    burstVel,
                    burstColor * 0.8f,
                    0.18f * CrystalSize,
                    14
                );
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // Subtle sound
            SoundEngine.PlaySound(SoundID.Item27 with { Pitch = 0.5f, Volume = 0.3f }, target.Center);
        }
        
        public override void OnKill(int timeLeft)
        {
            // Subtle dissipation - just a few sparkles
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi * i / 5f;
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(1.5f, 3f);
                Color burstColor = Color.Lerp(_primaryColor, _secondaryColor, (float)i / 5f);
                
                var sparkle = new SparkleParticle(
                    Projectile.Center,
                    burstVel,
                    burstColor * 0.6f,
                    0.15f * CrystalSize,
                    15
                );
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
        }
    }
}
