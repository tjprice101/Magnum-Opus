using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using MagnumOpus.Content.MoonlightSonata.Debuffs;
using MagnumOpus.Common.Systems;
using System.Collections.Generic;
using System;

namespace MagnumOpus.Content.Eroica.Projectiles
{
    /// <summary>
    /// Large ricochet beam that bounces between enemies 5-6 times
    /// </summary>
    public class FuneralPrayerRicochetBeam : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0"; // Invisible - particle-based

        private int currentTarget = -1;
        private List<int> hitEnemies = new List<int>();
        private int ricochetsRemaining = 5;
        private bool isRicocheting = false;
        private const float MaxRicochetRange = 500f;
        private Vector2 lastHitPosition;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 30;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300; // 5 seconds max
            Projectile.alpha = 255;
            Projectile.light = 1.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            // Initialize ricochet count from ai parameter
            if (Projectile.ai[0] == 0)
            {
                Projectile.ai[0] = 1;
                ricochetsRemaining = Main.rand.Next(5, 7); // 5-6 ricochets
            }

            // Find next target if we don't have one or need to ricochet
            if (currentTarget < 0 || !Main.npc[currentTarget].active || isRicocheting)
            {
                FindNextTarget();
                isRicocheting = false;
            }

            // Track towards current target
            if (currentTarget >= 0 && Main.npc[currentTarget].active)
            {
                Vector2 direction = (Main.npc[currentTarget].Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                Projectile.velocity = direction * 20f;
                
                // Check if we're close enough to hit
                if (Vector2.Distance(Projectile.Center, Main.npc[currentTarget].Center) < 50f)
                {
                    HitCurrentTarget();
                }
            }
            else if (ricochetsRemaining <= 0)
            {
                // No more ricochets, fade out
                Projectile.Kill();
            }

            // Intense glowing red with pink highlights
            Lighting.AddLight(Projectile.Center, 1.2f, 0.2f, 0.5f);
            
            // Custom particle trail effect
            CustomParticles.EroicaTrail(Projectile.Center, Projectile.velocity, 0.4f);

            // Large red torch particles for massive beam
            if (Main.rand.NextBool(1)) // Every frame
            {
                Dust beam = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.RedTorch, 0f, 0f, 100, default, 3.0f);
                beam.noGravity = true;
                beam.velocity = Projectile.velocity * 0.3f;
            }

            // Pink highlight particles
            if (Main.rand.NextBool(2))
            {
                Dust energy = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.PinkTorch, 0f, 0f, 100, default, 2.5f);
                energy.noGravity = true;
                energy.velocity = Main.rand.NextVector2Circular(3f, 3f);
            }

            // Orange fire particles for emphasis
            if (Main.rand.NextBool(3))
            {
                Dust fire = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.Torch, 0f, 0f, 100, new Color(255, 100, 50), 2.0f);
                fire.noGravity = true;
            }

            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        private void FindNextTarget()
        {
            if (ricochetsRemaining <= 0)
            {
                currentTarget = -1;
                return;
            }

            // Find nearest enemy we haven't hit yet
            int nextTarget = -1;
            float minDistance = MaxRicochetRange;
            Vector2 searchFrom = lastHitPosition != Vector2.Zero ? lastHitPosition : Projectile.Center;

            // Prioritize bosses
            bool foundBoss = false;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.boss && !npc.dontTakeDamage && !hitEnemies.Contains(i))
                {
                    float distance = Vector2.Distance(searchFrom, npc.Center);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nextTarget = i;
                        foundBoss = true;
                    }
                }
            }

            // If no boss, find any enemy
            if (!foundBoss)
            {
                minDistance = MaxRicochetRange;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && npc.lifeMax > 5 && !npc.dontTakeDamage && !hitEnemies.Contains(i))
                    {
                        float distance = Vector2.Distance(searchFrom, npc.Center);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            nextTarget = i;
                        }
                    }
                }
            }

            currentTarget = nextTarget;
        }

        private void HitCurrentTarget()
        {
            if (currentTarget < 0 || !Main.npc[currentTarget].active)
                return;

            NPC target = Main.npc[currentTarget];
            
            // Deal damage
            target.SimpleStrikeNPC(Projectile.damage, 0, false, 0f, null, false, 0f, true);
            target.AddBuff(ModContent.BuffType<MusicsDissonance>(), 360); // 6 seconds for ricochet

            // Sharp ricochet flash using EnergyFlares[3] (sharp burst)
            CustomParticles.EroicaFlare(target.Center, 0.6f);
            var ricochetFlare = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.EnergyFlares[3], target.Center, Vector2.Zero,
                new Color(255, 100, 50), 0.5f, 15, 0.02f, true, true);
            CustomParticleSystem.SpawnParticle(ricochetFlare);
            CustomParticles.ExplosionBurst(target.Center, new Color(255, 120, 80), 6, 4f);

            // Massive explosion particles
            for (int i = 0; i < 30; i++)
            {
                Dust shock = Dust.NewDustDirect(target.position, target.width, target.height,
                    DustID.RedTorch, 0f, 0f, 100, default, 3.5f);
                shock.noGravity = true;
                shock.velocity = Main.rand.NextVector2Circular(8f, 8f);
            }

            for (int i = 0; i < 20; i++)
            {
                Dust energy = Dust.NewDustDirect(target.position, target.width, target.height,
                    DustID.PinkTorch, 0f, 0f, 100, default, 2.5f);
                energy.noGravity = true;
                energy.velocity = Main.rand.NextVector2Circular(6f, 6f);
            }

            for (int i = 0; i < 15; i++)
            {
                Dust fire = Dust.NewDustDirect(target.position, target.width, target.height,
                    DustID.Torch, 0f, 0f, 100, new Color(255, 80, 30), 2.0f);
                fire.noGravity = true;
                fire.velocity = Main.rand.NextVector2Circular(5f, 5f);
            }

            // Sound effect
            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact with { Volume = 0.8f, Pitch = -0.2f }, target.position);

            // Mark this enemy as hit
            hitEnemies.Add(currentTarget);
            lastHitPosition = target.Center;
            
            // Decrement ricochets
            ricochetsRemaining--;
            
            // Flag for ricochet
            isRicocheting = true;
            currentTarget = -1;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Draw large particle trail
            Vector2 beamDirection = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            
            // Create thick particle trail
            for (int i = 0; i < 3; i++)
            {
                Vector2 offset = new Vector2(0, (i - 1) * 5).RotatedBy(Projectile.rotation);
                Vector2 trailPos = Projectile.Center + offset;
                
                Dust trail = Dust.NewDustPerfect(trailPos, DustID.RedTorch, Vector2.Zero, 0, default, 2.5f);
                trail.noGravity = true;
                trail.velocity = -Projectile.velocity * 0.1f;
            }

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            // Final explosion
            for (int i = 0; i < 20; i++)
            {
                Dust explosion = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.RedTorch, 0f, 0f, 100, default, 2.5f);
                explosion.noGravity = true;
                explosion.velocity = Main.rand.NextVector2Circular(5f, 5f);
            }
        }
    }
}
