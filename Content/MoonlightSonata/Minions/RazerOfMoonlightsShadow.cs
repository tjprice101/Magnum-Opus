using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.MoonlightSonata.Debuffs;
using MagnumOpus.Content.MoonlightSonata.Projectiles;

namespace MagnumOpus.Content.MoonlightSonata.Minions
{
    /// <summary>
    /// Razer of Moonlight's Shadow - A summoned minion that attacks enemies.
    /// Scales with summon accessories and buffs.
    /// Fires ricocheting moonlight beams.
    /// Applies Musical Dissonance debuff on hit.
    /// </summary>
    public class RazerOfMoonlightsShadow : ModProjectile
    {
        private enum AIState
        {
            Idle,
            Attacking
        }

        private AIState State
        {
            get => (AIState)Projectile.ai[0];
            set => Projectile.ai[0] = (float)value;
        }

        private float Timer
        {
            get => Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }
        
        private int attackCooldown = 0;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 1;
            Main.projPet[Type] = true;
            ProjectileID.Sets.MinionSacrificable[Type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 18000;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.minionSlots = 1f;
        }

        public override bool? CanCutTiles() => false;

        public override bool MinionContactDamage() => true;

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            
            // Check if player still has the buff
            if (!CheckActive(owner))
                return;

            // Find target
            NPC target = FindTarget(owner);
            
            if (target != null)
            {
                State = AIState.Attacking;
                AttackTarget(target);
            }
            else
            {
                State = AIState.Idle;
                IdleMovement(owner);
            }

            // Visual effects - trailing particles
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
                    DustID.PurpleTorch, 0f, 0f, 100, default, 0.8f);
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }

            // Lighting
            Lighting.AddLight(Projectile.Center, 0.3f, 0.1f, 0.5f);

            // Animation - bob up and down
            Projectile.rotation = Projectile.velocity.X * 0.05f;
        }

        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<RazerOfMoonlightsShadowBuff>());
                Projectile.Kill();
                return false;
            }

            if (owner.HasBuff(ModContent.BuffType<RazerOfMoonlightsShadowBuff>()))
            {
                Projectile.timeLeft = 2;
            }

            return true;
        }

        private NPC FindTarget(Player owner)
        {
            float maxDistance = 800f;
            NPC closestTarget = null;
            float closestDist = maxDistance;

            // Check if player has manually targeted an NPC
            if (owner.HasMinionAttackTargetNPC)
            {
                NPC target = Main.npc[owner.MinionAttackTargetNPC];
                if (target.CanBeChasedBy(this) && Vector2.Distance(Projectile.Center, target.Center) < maxDistance)
                {
                    return target;
                }
            }

            // Find closest enemy
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.CanBeChasedBy(this))
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closestTarget = npc;
                    }
                }
            }

            return closestTarget;
        }

        private void AttackTarget(NPC target)
        {
            Vector2 direction = target.Center - Projectile.Center;
            float distance = direction.Length();
            
            float speed = 14f;
            float inertia = 20f;

            if (distance > 40f)
            {
                direction.Normalize();
                direction *= speed;
                Projectile.velocity = (Projectile.velocity * (inertia - 1) + direction) / inertia;
            }

            // Attack particles when close
            if (distance < 100f && Main.rand.NextBool(5))
            {
                Vector2 dustVel = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * 3f;
                Dust dust = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.PurpleCrystalShard, dustVel.X, dustVel.Y, 100, default, 1.2f);
                dust.noGravity = true;
            }
            
            // Fire beam attack
            attackCooldown--;
            if (attackCooldown <= 0 && distance < 600f && Main.myPlayer == Projectile.owner)
            {
                FireBeam(target);
                attackCooldown = 90; // 1.5 second cooldown
            }
        }
        
        private void FireBeam(NPC target)
        {
            Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
            float speed = 16f;
            Vector2 velocity = toTarget * speed;
            
            // Create visual effect
            for (int i = 0; i < 10; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.PurpleCrystalShard, 0, 0, 100, default, 1.5f);
                dust.velocity = toTarget.RotatedByRandom(0.3f) * Main.rand.NextFloat(2f, 5f);
                dust.noGravity = true;
            }
            
            // Spawn beam projectile
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                Projectile.Center,
                velocity,
                ModContent.ProjectileType<GoliathMoonlightBeam>(),
                Projectile.damage,
                Projectile.knockBack,
                Projectile.owner
            );
        }

        private void IdleMovement(Player owner)
        {
            // Float near the player
            Vector2 targetPos = owner.Center + new Vector2(-50f * owner.direction, -60f);
            
            // Bobbing motion
            targetPos.Y += (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 10f;
            
            Vector2 direction = targetPos - Projectile.Center;
            float distance = direction.Length();
            
            float speed = 8f;
            float inertia = 30f;

            if (distance > 20f)
            {
                direction.Normalize();
                direction *= Math.Min(distance / 10f, speed);
                Projectile.velocity = (Projectile.velocity * (inertia - 1) + direction) / inertia;
            }
            else
            {
                Projectile.velocity *= 0.9f;
            }

            // Teleport if too far
            if (distance > 2000f)
            {
                Projectile.Center = owner.Center;
                Projectile.velocity = Vector2.Zero;
                
                // Teleport effect
                for (int i = 0; i < 15; i++)
                {
                    Dust dust = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.PurpleTorch, 
                        Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f), 100, default, 1.5f);
                    dust.noGravity = true;
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply Musical Dissonance debuff
            target.AddBuff(ModContent.BuffType<MusicsDissonance>(), 180);

            // Hit effect
            for (int i = 0; i < 6; i++)
            {
                Dust dust = Dust.NewDustDirect(target.Center, 1, 1, DustID.PurpleTorch, 
                    Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f), 100, default, 1.2f);
                dust.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Draw the minion with purple tint
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 drawOrigin = new Vector2(texture.Width / 2, texture.Height / 2);
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            // Purple glow effect
            Color glowColor = new Color(150, 80, 200, 100);
            
            Main.EntitySpriteDraw(texture, drawPos, null, glowColor, Projectile.rotation, 
                drawOrigin, Projectile.scale * 1.1f, SpriteEffects.None, 0);
            
            Main.EntitySpriteDraw(texture, drawPos, null, lightColor, Projectile.rotation, 
                drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            
            return false;
        }
    }
}
