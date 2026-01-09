using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.GameContent;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.Eroica.Minions
{
    /// <summary>
    /// Sakura of Fate - A spectral guardian minion that floats beside the player.
    /// Fires black and red flaming projectiles at nearby enemies.
    /// Has a mystical glow similar to Goliath but 25% dimmer.
    /// Uses a 6x6 spritesheet animation (36 frames).
    /// </summary>
    public class SakuraOfFate : ModProjectile
    {
        // Spritesheet configuration - 6x6 grid
        public const int FrameColumns = 6;
        public const int FrameRows = 6;
        public const int TotalFrames = 36;
        public const int FrameTime = 4; // Game ticks per animation frame
        
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
        
        private int frameCounter = 0;
        private int currentFrame = 0;
        private int attackCooldown = 0;
        private float hoverOffset = 0f;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 1; // We handle frames manually
            Main.projPet[Type] = true;
            ProjectileID.Sets.MinionSacrificable[Type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 48;
            Projectile.height = 48;
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

            // Update animation constantly
            UpdateAnimation();
            
            // Float beside player
            FloatBesidePlayer(owner);
            
            // Find target
            NPC target = FindTarget(owner);
            
            if (target != null)
            {
                State = AIState.Attacking;
                AttackTarget(target, owner);
            }
            else
            {
                State = AIState.Idle;
            }
            
            // Visual effects - gold and red flame particles
            CreateAmbientEffects();

            // Lighting - warm gold/orange glow
            Lighting.AddLight(Projectile.Center, 0.5f, 0.3f, 0.1f);
            
            // Update facing direction based on velocity or target
            if (target != null)
            {
                Projectile.spriteDirection = target.Center.X > Projectile.Center.X ? 1 : -1;
            }
            else if (Math.Abs(Projectile.velocity.X) > 0.5f)
            {
                Projectile.spriteDirection = Projectile.velocity.X > 0 ? 1 : -1;
            }
        }
        
        private void UpdateAnimation()
        {
            frameCounter++;
            if (frameCounter >= FrameTime)
            {
                frameCounter = 0;
                currentFrame++;
                if (currentFrame >= TotalFrames)
                    currentFrame = 0;
            }
        }

        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<SakuraOfFateBuff>());
                Projectile.Kill();
                return false;
            }

            if (owner.HasBuff(ModContent.BuffType<SakuraOfFateBuff>()))
            {
                Projectile.timeLeft = 2;
            }

            return true;
        }

        private NPC FindTarget(Player owner)
        {
            float maxDistance = 600f;
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
        
        private void FloatBesidePlayer(Player owner)
        {
            // Gentle hover animation
            hoverOffset += 0.05f;
            float hoverY = (float)Math.Sin(hoverOffset) * 8f;
            
            // Target position - float beside player
            Vector2 targetPos = owner.Center + new Vector2(-50f * owner.direction, -40f + hoverY);
            Vector2 direction = targetPos - Projectile.Center;
            float distance = direction.Length();
            
            float speed = 10f;
            float inertia = 20f;
            
            if (distance > 10f)
            {
                direction.Normalize();
                direction *= Math.Min(distance / 6f, speed);
                Projectile.velocity = (Projectile.velocity * (inertia - 1) + direction) / inertia;
            }
            else
            {
                Projectile.velocity *= 0.95f;
            }

            // Teleport if too far
            if (distance > 1200f)
            {
                Projectile.Center = owner.Center + new Vector2(-40f * owner.direction, -30f);
                Projectile.velocity = Vector2.Zero;
                
                // Teleport effect - black and red
                ThemedParticles.TeleportBurst(Projectile.Center, isMoonlight: false);
                ThemedParticles.EroicaBloomBurst(Projectile.Center, 1.5f);
                ThemedParticles.SakuraPetals(Projectile.Center, 8, 30f);
                
                SoundEngine.PlaySound(SoundID.Item8 with { Pitch = -0.3f }, Projectile.Center);
            }
        }

        private void AttackTarget(NPC target, Player owner)
        {
            Vector2 direction = target.Center - Projectile.Center;
            float distance = direction.Length();
            
            // Fire gold/red flame stream (flamethrower style - very fast)
            attackCooldown--;
            if (attackCooldown <= 0 && distance < 400f && Main.myPlayer == Projectile.owner)
            {
                FireFlameProjectile(target);
                attackCooldown = 4; // Rapid fire flamethrower - every ~0.07 seconds
            }
        }
        
        private void FireFlameProjectile(NPC target)
        {
            Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
            float speed = 14f + Main.rand.NextFloat(-2f, 2f);
            
            // Add spread for flamethrower feel
            Vector2 velocity = toTarget.RotatedByRandom(0.15f) * speed;
            
            // Small gold/red muzzle particles (only occasional)
            if (Main.rand.NextBool(3))
            {
                Color flameColor = Main.rand.NextBool() ? new Color(255, 200, 50) : new Color(255, 80, 30);
                Dust flame = Dust.NewDustPerfect(Projectile.Center + toTarget * 10f, DustID.Torch, 
                    toTarget * 2f + Main.rand.NextVector2Circular(1f, 1f), 100, flameColor, 1f);
                flame.noGravity = true;
            }
            
            // Occasional fire sound (not every shot)
            if (Main.rand.NextBool(8))
            {
                SoundEngine.PlaySound(SoundID.Item34 with { Pitch = 0.3f, Volume = 0.3f }, Projectile.Center);
            }
            
            // Spawn the flame projectile
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                Projectile.Center,
                velocity,
                ModContent.ProjectileType<SakuraFlameProjectile>(),
                Projectile.damage / 3, // Lower damage per hit since it fires rapidly
                Projectile.knockBack * 0.3f,
                Projectile.owner
            );
        }
        
        private void CreateAmbientEffects()
        {
            // Subtle black and crimson aura - minimal particles
            if (Main.rand.NextBool(5))
            {
                // Occasional black smoke wisp
                Dust shadow = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
                    DustID.Smoke, 0f, -1.5f, 150, Color.Black, 0.9f);
                shadow.noGravity = true;
                shadow.velocity *= 0.3f;
            }
            
            if (Main.rand.NextBool(6))
            {
                // Rare crimson spark
                Dust crimson = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
                    DustID.CrimsonTorch, 0f, -1.5f, 100, default, 0.8f);
                crimson.noGravity = true;
                crimson.velocity *= 0.3f;
            }
            
            // Orbiting dark flame particles - less frequent
            if (Main.GameUpdateCount % 4 == 0)
            {
                float orbitAngle = Main.GameUpdateCount * 0.06f;
                Vector2 orbitOffset = new Vector2((float)Math.Cos(orbitAngle), (float)Math.Sin(orbitAngle)) * 20f;
                int orbitType = Main.GameUpdateCount % 8 < 4 ? DustID.Smoke : DustID.CrimsonTorch;
                Color orbitColor = Main.GameUpdateCount % 8 < 4 ? Color.Black : default;
                Dust orbit = Dust.NewDustPerfect(Projectile.Center + orbitOffset, orbitType, Vector2.Zero, 100, orbitColor, 0.8f);
                orbit.noGravity = true;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Small gold/red impact burst
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 vel = new Vector2((float)System.Math.Cos(angle), (float)System.Math.Sin(angle)) * 2.5f;
                Color flameColor = i % 2 == 0 ? new Color(255, 200, 50) : new Color(255, 80, 30);
                Dust dust = Dust.NewDustPerfect(target.Center, DustID.Torch, vel, 100, flameColor, 1f);
                dust.noGravity = true;
            }
            
            // Warm lighting flash
            Lighting.AddLight(target.Center, 0.6f, 0.3f, 0.1f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Get the texture - render as simple still image
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            
            Vector2 origin = new Vector2(texture.Width / 2f, texture.Height / 2f);
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            // Flip sprite based on direction
            SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            
            // Glow effect - dark red/black outer glow (25% dimmer than Goliath)
            Color redGlow = new Color(130, 20, 30, 0) * 0.3f;
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(4f, 0f).RotatedBy(i * MathHelper.PiOver2 + Main.GameUpdateCount * 0.05f);
                Main.EntitySpriteDraw(texture, drawPos + offset, null, redGlow, 0f, origin, Projectile.scale * 1.1f, effects, 0);
            }
            
            // Black inner glow (no purple)
            Color blackGlow = new Color(20, 10, 10, 0) * 0.225f;
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(2f, 0f).RotatedBy(i * MathHelper.PiOver2 + Main.GameUpdateCount * 0.08f);
                Main.EntitySpriteDraw(texture, drawPos + offset, null, blackGlow, 0f, origin, Projectile.scale * 1.05f, effects, 0);
            }
            
            // Draw main sprite (still image, no animation frames)
            Main.EntitySpriteDraw(texture, drawPos, null, lightColor, 0f, origin, Projectile.scale, effects, 0);
            
            return false;
        }
    }
}
