using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.GameContent;

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
            
            // Visual effects - black and scarlet red flame particles
            CreateAmbientEffects();

            // Lighting - deep red with some black tones
            Lighting.AddLight(Projectile.Center, 0.5f, 0.1f, 0.15f);
            
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
                
                // Teleport effect - black and red (NO purple)
                for (int i = 0; i < 25; i++)
                {
                    int dustType = Main.rand.NextBool() ? DustID.Smoke : DustID.CrimsonTorch;
                    Color dustColor = Main.rand.NextBool() ? Color.Black : default;
                    Dust dust = Dust.NewDustDirect(Projectile.Center, 1, 1, dustType, 
                        Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f), 100, dustColor, 1.8f);
                    dust.noGravity = true;
                }
                
                // Add black smoke burst
                for (int i = 0; i < 10; i++)
                {
                    Dust smoke = Dust.NewDustPerfect(Projectile.Center, DustID.Smoke,
                        Main.rand.NextVector2Circular(3f, 3f), 100, Color.Black, 1.5f);
                    smoke.noGravity = true;
                }
                
                SoundEngine.PlaySound(SoundID.Item8 with { Pitch = -0.3f }, Projectile.Center);
            }
        }

        private void AttackTarget(NPC target, Player owner)
        {
            Vector2 direction = target.Center - Projectile.Center;
            float distance = direction.Length();
            
            // Fire black/red flame projectiles
            attackCooldown--;
            if (attackCooldown <= 0 && distance < 500f && Main.myPlayer == Projectile.owner)
            {
                FireFlameProjectile(target);
                attackCooldown = 25; // Attack every ~0.4 seconds
            }
        }
        
        private void FireFlameProjectile(NPC target)
        {
            Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
            float speed = 16f;
            Vector2 velocity = toTarget * speed;
            
            // Muzzle flash - black and deep scarlet (no purple)
            for (int ring = 0; ring < 2; ring++)
            {
                for (int i = 0; i < 12; i++)
                {
                    float angle = MathHelper.TwoPi * i / 12f;
                    Vector2 dustVel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * (3f + ring * 2f);
                    int flashType = (i + ring) % 2 == 0 ? DustID.Smoke : DustID.CrimsonTorch;
                    Color flashColor = (i + ring) % 2 == 0 ? Color.Black : default;
                    Dust dust = Dust.NewDustPerfect(Projectile.Center + toTarget * 15f, flashType, dustVel, 0, flashColor, 1.8f - ring * 0.3f);
                    dust.noGravity = true;
                    dust.fadeIn = 1.3f;
                }
            }
            
            // Directional burst
            for (int i = 0; i < 10; i++)
            {
                Vector2 dustVel = toTarget.RotatedByRandom(0.5f) * Main.rand.NextFloat(3f, 7f);
                int flashType = Main.rand.NextBool() ? DustID.Smoke : DustID.CrimsonTorch;
                Color flashColor = Main.rand.NextBool() ? Color.Black : default;
                Dust dust = Dust.NewDustPerfect(Projectile.Center + toTarget * 18f, flashType, dustVel, 0, flashColor, 1.6f);
                dust.noGravity = true;
            }
            
            // Fire sound - dark, ominous
            SoundEngine.PlaySound(SoundID.Item20 with { Pitch = -0.4f, Volume = 0.7f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item73 with { Pitch = -0.5f, Volume = 0.4f }, Projectile.Center);
            
            // Spawn the flame projectile
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                Projectile.Center,
                velocity,
                ModContent.ProjectileType<SakuraFlameProjectile>(),
                Projectile.damage,
                Projectile.knockBack,
                Projectile.owner
            );
        }
        
        private void CreateAmbientEffects()
        {
            // Black and deep scarlet red flame particles - constantly aflame - NO PURPLE
            if (Main.rand.NextBool(2))
            {
                // Black smoke (not Shadowflame - that has purple)
                Dust shadow = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
                    DustID.Smoke, 0f, -2f, 150, Color.Black, 1.5f);
                shadow.noGravity = true;
                shadow.velocity *= 0.5f;
                shadow.fadeIn = 1f;
            }
            
            if (Main.rand.NextBool(2))
            {
                // Deep crimson/scarlet
                Dust crimson = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
                    DustID.CrimsonTorch, 0f, -2f, 100, default, 1.5f);
                crimson.noGravity = true;
                crimson.velocity *= 0.5f;
                crimson.fadeIn = 1f;
            }
            
            // Occasional black smoke wisps
            if (Main.rand.NextBool(4))
            {
                Dust smoke = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(Projectile.width / 2f, Projectile.height / 2f),
                    DustID.Smoke, new Vector2(Main.rand.NextFloat(-1f, 1f), -2f), 100, Color.Black, 1.3f);
                smoke.noGravity = true;
            }
            
            // Ember particles rising
            if (Main.rand.NextBool(3))
            {
                Dust ember = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(Projectile.width / 3f, Projectile.height / 3f),
                    DustID.Torch, new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -3f), 100, new Color(180, 30, 30), 1.2f);
                ember.noGravity = true;
            }
            
            // Orbiting dark flame particles - crimson instead of shadowflame
            if (Main.GameUpdateCount % 2 == 0)
            {
                float orbitAngle = Main.GameUpdateCount * 0.06f;
                Vector2 orbitOffset = new Vector2((float)Math.Cos(orbitAngle), (float)Math.Sin(orbitAngle)) * 25f;
                int orbitType = Main.GameUpdateCount % 4 < 2 ? DustID.Smoke : DustID.CrimsonTorch;
                Color orbitColor = Main.GameUpdateCount % 4 < 2 ? Color.Black : default;
                Dust orbit = Dust.NewDustPerfect(Projectile.Center + orbitOffset, orbitType, Vector2.Zero, 100, orbitColor, 1.3f);
                orbit.noGravity = true;
                orbit.fadeIn = 0.8f;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Impact effect - black and red explosion
            for (int ring = 0; ring < 2; ring++)
            {
                for (int i = 0; i < 15; i++)
                {
                    float angle = MathHelper.TwoPi * i / 15f;
                    Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * (3f + ring * 2f);
                    int dustType = (i + ring) % 2 == 0 ? DustID.Smoke : DustID.CrimsonTorch;
                    Color dustColor = (i + ring) % 2 == 0 ? Color.Black : default;
                    Dust dust = Dust.NewDustPerfect(target.Center, dustType, vel, 100, dustColor, 1.6f - ring * 0.3f);
                    dust.noGravity = true;
                }
            }
            
            // Add lighting on hit
            Lighting.AddLight(target.Center, 0.6f, 0.15f, 0.2f);
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
