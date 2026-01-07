using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using MagnumOpus.Content.MoonlightSonata.Debuffs;
using MagnumOpus.Content.MoonlightSonata.Minions;

namespace MagnumOpus.Content.MoonlightSonata.Projectiles
{
    /// <summary>
    /// Goliath Devastating Beam - A Last Prism-style devastating beam.
    /// Fires as a continuous beam that creates ricochet explosions between enemies.
    /// </summary>
    public class GoliathDevastatingBeam : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.LastPrism;
        
        private const float MaxBeamLength = 2000f;
        private const int BeamDuration = 90; // 1.5 seconds of beam
        private const int ExplosionInterval = 10; // Ticks between ricochet explosions
        
        // Store the owning minion's whoAmI
        private int ownerProjectile = -1;
        
        public float BeamLength
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        
        public float BeamTimer
        {
            get => Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }
        
        private List<int> hitEnemies = new List<int>();
        private int explosionTimer = 0;
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionShot[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1;
            Projectile.timeLeft = BeamDuration;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
        }
        
        public override void AI()
        {
            BeamTimer++;
            
            // Find the owning Goliath minion to track position
            Player owner = Main.player[Projectile.owner];
            Projectile goliath = null;
            
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == Projectile.owner && p.type == ModContent.ProjectileType<GoliathOfMoonlight>())
                {
                    goliath = p;
                    break;
                }
            }
            
            // Update beam origin to follow Goliath
            if (goliath != null && goliath.active)
            {
                Projectile.Center = goliath.Center;
            }
            
            // Calculate beam length - starts small, grows rapidly
            float progress = Math.Min(BeamTimer / 20f, 1f);
            BeamLength = MaxBeamLength * progress;
            
            // Intense particle effects along the beam
            CreateBeamParticles();
            
            // Handle ricochet explosions
            explosionTimer++;
            if (explosionTimer >= ExplosionInterval)
            {
                explosionTimer = 0;
                CreateRicochetExplosions();
            }
            
            // Beam wobble for visual effect
            float wobble = (float)Math.Sin(BeamTimer * 0.3f) * 0.02f;
            Projectile.velocity = Projectile.velocity.RotatedBy(wobble);
            Projectile.velocity.Normalize();
            
            // Intense lighting along beam
            for (float i = 0; i < BeamLength; i += 50f)
            {
                Vector2 lightPos = Projectile.Center + Projectile.velocity * i;
                Lighting.AddLight(lightPos, 0.6f, 0.3f, 0.9f);
            }
            
            // Sound effects
            if (BeamTimer == 1)
            {
                SoundEngine.PlaySound(SoundID.Item122 with { Volume = 1.2f, Pitch = -0.3f }, Projectile.Center);
            }
            if (BeamTimer % 15 == 0)
            {
                SoundEngine.PlaySound(SoundID.Item15 with { Volume = 0.4f, Pitch = 0.5f }, Projectile.Center);
            }
        }
        
        private void CreateBeamParticles()
        {
            // Core beam particles
            for (float i = 0; i < BeamLength; i += 20f)
            {
                Vector2 dustPos = Projectile.Center + Projectile.velocity * i;
                dustPos += Main.rand.NextVector2Circular(8f, 8f);
                
                // Alternate between dark purple and light blue
                int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.IceTorch;
                Dust dust = Dust.NewDustPerfect(dustPos, dustType, 
                    Projectile.velocity.RotatedByRandom(0.5f) * Main.rand.NextFloat(1f, 3f), 0, default, 2f);
                dust.noGravity = true;
                dust.fadeIn = 1.5f;
            }
            
            // Edge particles - perpendicular to beam
            Vector2 perpendicular = Projectile.velocity.RotatedBy(MathHelper.PiOver2);
            for (float i = 0; i < BeamLength; i += 40f)
            {
                Vector2 basePos = Projectile.Center + Projectile.velocity * i;
                
                // Particles streaming off sides
                for (int side = -1; side <= 1; side += 2)
                {
                    Vector2 dustPos = basePos + perpendicular * side * Main.rand.NextFloat(10f, 30f);
                    int dustType = side == -1 ? DustID.PurpleTorch : DustID.IceTorch;
                    Vector2 vel = perpendicular * side * Main.rand.NextFloat(2f, 5f);
                    Dust dust = Dust.NewDustPerfect(dustPos, dustType, vel, 0, default, 1.5f);
                    dust.noGravity = true;
                }
            }
            
            // Intense origin particles
            for (int i = 0; i < 4; i++)
            {
                int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.IceTorch;
                Dust origin = Dust.NewDustPerfect(Projectile.Center, dustType, 
                    Main.rand.NextVector2Circular(4f, 4f), 0, default, 2.5f);
                origin.noGravity = true;
                origin.fadeIn = 1.5f;
            }
            
            // Electric sparks
            if (Main.rand.NextBool(3))
            {
                float pos = Main.rand.NextFloat(BeamLength);
                Vector2 sparkPos = Projectile.Center + Projectile.velocity * pos;
                Dust spark = Dust.NewDustPerfect(sparkPos, DustID.Electric, 
                    Main.rand.NextVector2Circular(3f, 3f), 100, Color.LightBlue, 1.5f);
                spark.noGravity = true;
            }
        }
        
        private void CreateRicochetExplosions()
        {
            // Find enemies along the beam path
            List<NPC> enemiesInBeam = new List<NPC>();
            
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (!npc.CanBeChasedBy(this))
                    continue;
                
                // Check if enemy is close to the beam line
                Vector2 toNPC = npc.Center - Projectile.Center;
                float distAlongBeam = Vector2.Dot(toNPC, Projectile.velocity);
                
                if (distAlongBeam > 0 && distAlongBeam < BeamLength)
                {
                    Vector2 closestPointOnBeam = Projectile.Center + Projectile.velocity * distAlongBeam;
                    float distToBeam = Vector2.Distance(npc.Center, closestPointOnBeam);
                    
                    if (distToBeam < 80f) // Hit detection width
                    {
                        enemiesInBeam.Add(npc);
                    }
                }
            }
            
            // Create ricochet explosions between enemies
            if (enemiesInBeam.Count >= 1)
            {
                // Create explosion at first enemy
                NPC firstEnemy = enemiesInBeam[0];
                CreateExplosionEffect(firstEnemy.Center);
                
                // Ricochet to other enemies
                for (int i = 1; i < enemiesInBeam.Count && i < 5; i++)
                {
                    NPC prevEnemy = enemiesInBeam[i - 1];
                    NPC currEnemy = enemiesInBeam[i];
                    
                    // Create ricochet line between enemies
                    CreateRicochetLine(prevEnemy.Center, currEnemy.Center);
                    CreateExplosionEffect(currEnemy.Center);
                }
                
                // Even if only one enemy, create secondary explosions around it
                if (enemiesInBeam.Count == 1)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        Vector2 randomOffset = Main.rand.NextVector2Circular(100f, 100f);
                        CreateExplosionEffect(firstEnemy.Center + randomOffset);
                    }
                }
            }
        }
        
        private void CreateExplosionEffect(Vector2 position)
        {
            // MASSIVE explosion burst
            // Outer ring - dark purple
            for (int i = 0; i < 25; i++)
            {
                float angle = MathHelper.TwoPi * i / 25f;
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(8f, 16f);
                Dust dust = Dust.NewDustPerfect(position, DustID.PurpleTorch, vel, 0, default, 3f);
                dust.noGravity = true;
                dust.fadeIn = 1.8f;
            }
            
            // Inner ring - light blue
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi * i / 20f;
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(5f, 12f);
                Dust dust = Dust.NewDustPerfect(position, DustID.IceTorch, vel, 0, default, 2.5f);
                dust.noGravity = true;
                dust.fadeIn = 1.5f;
            }
            
            // Random burst
            for (int i = 0; i < 20; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(10f, 10f);
                int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.IceTorch;
                Dust dust = Dust.NewDustPerfect(position, dustType, vel, 0, default, 2.2f);
                dust.noGravity = true;
            }
            
            // Electric sparks
            for (int i = 0; i < 12; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(8f, 8f);
                Dust spark = Dust.NewDustPerfect(position, DustID.Electric, vel, 100, Color.LightBlue, 1.5f);
                spark.noGravity = true;
            }
            
            // Shadowflame accent
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                Dust shadow = Dust.NewDustPerfect(position, DustID.Shadowflame, vel, 100, default, 1.8f);
                shadow.noGravity = true;
            }
            
            // Sound
            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.5f, Pitch = 0.6f }, position);
            
            // Intense lighting
            Lighting.AddLight(position, 1f, 0.5f, 1.2f);
        }
        
        private void CreateRicochetLine(Vector2 start, Vector2 end)
        {
            // Create particle line between two points
            Vector2 direction = (end - start).SafeNormalize(Vector2.Zero);
            float distance = Vector2.Distance(start, end);
            
            for (float i = 0; i < distance; i += 15f)
            {
                Vector2 pos = start + direction * i;
                int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.IceTorch;
                Dust dust = Dust.NewDustPerfect(pos, dustType, 
                    direction.RotatedByRandom(0.3f) * Main.rand.NextFloat(1f, 3f), 0, default, 1.5f);
                dust.noGravity = true;
                dust.fadeIn = 1.2f;
            }
        }
        
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // Line collision for the beam
            float point = 0f;
            Vector2 beamEnd = Projectile.Center + Projectile.velocity * BeamLength;
            
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), 
                Projectile.Center, beamEnd, 30f, ref point);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply debuff
            target.AddBuff(ModContent.BuffType<MusicsDissonance>(), 300);
            
            // Heal player
            Player owner = Main.player[Projectile.owner];
            if (owner != null && owner.active)
            {
                int healAmount = 5;
                owner.statLife = Math.Min(owner.statLife + healAmount, owner.statLifeMax2);
                owner.HealEffect(healAmount, true);
            }
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            // Don't draw any rectangle beam - just use particles created in AI()
            // Draw only a small gradient glow at the origin point
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;
            
            Vector2 beamStart = Projectile.Center - Main.screenPosition;
            
            // Draw only a soft glow at the source point - the actual beam is particles
            float glowSize = 20f + (float)Math.Sin(BeamTimer * 0.2f) * 5f;
            
            // Outer purple glow
            Color outerGlow = new Color(100, 40, 160) * 0.3f;
            spriteBatch.Draw(pixel, beamStart, null, outerGlow, 0f, new Vector2(0.5f, 0.5f), glowSize, SpriteEffects.None, 0f);
            
            // Inner white/blue glow
            Color innerGlow = new Color(200, 180, 255) * 0.5f;
            spriteBatch.Draw(pixel, beamStart, null, innerGlow, 0f, new Vector2(0.5f, 0.5f), glowSize * 0.5f, SpriteEffects.None, 0f);
            
            return false;
        }
    }
}
