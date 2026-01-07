using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.GameContent;
using MagnumOpus.Content.MoonlightSonata.Debuffs;
using MagnumOpus.Content.MoonlightSonata.Projectiles;
using MagnumOpus.Content.MoonlightSonata.Accessories;

namespace MagnumOpus.Content.MoonlightSonata.Minions
{
    /// <summary>
    /// Goliath of Moonlight - A massive lunar guardian minion.
    /// Has gravity, floats toward player when can't reach them.
    /// Fires devastating Last Prism-style beams after a 2 second charge.
    /// Uses a 6x6 spritesheet animation (36 frames).
    /// </summary>
    public class GoliathOfMoonlight : ModProjectile
    {
        // Spritesheet configuration - 6x6 grid
        public const int FrameColumns = 6;
        public const int FrameRows = 6;
        public const int TotalFrames = 36;
        public const int FrameTime = 4; // Game ticks per animation frame
        
        // Charge time - 2 seconds = 120 ticks
        private const int ChargeUpTime = 120;
        
        private enum AIState
        {
            Idle,
            Attacking,
            Floating,
            ChargingBeam // New state for 2-second charge
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
        private int chargeTimer = 0;
        private bool isCharging = false;
        private NPC chargeTarget = null;
        private bool wasOnGround = false;
        private Vector2 frozenPosition = Vector2.Zero; // Position when charging starts

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
            Projectile.width = 64;
            Projectile.height = 64;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 18000;
            Projectile.tileCollide = true; // Has gravity, collides with tiles
            Projectile.ignoreWater = true;
            Projectile.minionSlots = 1f;
        }

        public override bool? CanCutTiles() => false;

        public override bool MinionContactDamage() => true;
        
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // Don't die on tile collision, just stop
            return false;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            
            // Check if player still has the buff
            if (!CheckActive(owner))
                return;

            // Update animation constantly
            UpdateAnimation();
            
            // Apply gravity
            ApplyGravity();
            
            // Check if we're grounded
            bool onGround = IsOnGround();
            
            // Find target
            NPC target = FindTarget(owner);
            
            // Distance to player
            float distToPlayer = Vector2.Distance(Projectile.Center, owner.Center);
            
            // If too far from player or can't physically reach them, float toward them
            if (distToPlayer > 600f || !CanReachPlayer(owner))
            {
                State = AIState.Floating;
                FloatTowardPlayer(owner);
            }
            else if (target != null)
            {
                State = AIState.Attacking;
                AttackTarget(target, owner, onGround);
            }
            else
            {
                State = AIState.Idle;
                IdleMovement(owner, onGround);
            }
            
            // Visual effects - trailing particles
            CreateAmbientEffects();

            // Lighting
            Lighting.AddLight(Projectile.Center, 0.4f, 0.2f, 0.6f);
            
            // Update facing direction based on velocity or target
            if (Math.Abs(Projectile.velocity.X) > 0.5f)
            {
                Projectile.spriteDirection = Projectile.velocity.X > 0 ? 1 : -1;
            }
            
            wasOnGround = onGround;
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
        
        private void ApplyGravity()
        {
            // Apply gravity when not floating
            if (State != AIState.Floating)
            {
                Projectile.velocity.Y += 0.35f; // Gravity
                
                // Terminal velocity
                if (Projectile.velocity.Y > 16f)
                    Projectile.velocity.Y = 16f;
            }
        }
        
        private bool IsOnGround()
        {
            // Check if standing on tiles
            Vector2 checkPos = Projectile.BottomLeft;
            for (int x = 0; x < Projectile.width / 16 + 1; x++)
            {
                int tileX = (int)((checkPos.X + x * 16) / 16);
                int tileY = (int)((checkPos.Y + 4) / 16);
                
                Tile tile = Framing.GetTileSafely(tileX, tileY);
                if (tile.HasTile && Main.tileSolid[tile.TileType])
                    return true;
            }
            return false;
        }
        
        private bool CanReachPlayer(Player owner)
        {
            // Simple line-of-sight check
            Vector2 direction = owner.Center - Projectile.Center;
            float distance = direction.Length();
            direction.Normalize();
            
            // Check every 16 pixels along the path
            for (float i = 0; i < distance; i += 16f)
            {
                Vector2 checkPos = Projectile.Center + direction * i;
                int tileX = (int)(checkPos.X / 16);
                int tileY = (int)(checkPos.Y / 16);
                
                Tile tile = Framing.GetTileSafely(tileX, tileY);
                if (tile.HasTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType])
                {
                    // Hit a solid block - can't reach player easily
                    if (i > 200f) // Only trigger float if wall is far enough
                        return false;
                }
            }
            return true;
        }

        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<GoliathOfMoonlightBuff>());
                Projectile.Kill();
                return false;
            }

            if (owner.HasBuff(ModContent.BuffType<GoliathOfMoonlightBuff>()))
            {
                Projectile.timeLeft = 2;
            }

            return true;
        }

        private NPC FindTarget(Player owner)
        {
            float maxDistance = 700f;
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
        
        private void FloatTowardPlayer(Player owner)
        {
            // Disable tile collision while floating
            Projectile.tileCollide = false;
            
            Vector2 targetPos = owner.Center + new Vector2(-40f * owner.direction, -60f);
            Vector2 direction = targetPos - Projectile.Center;
            float distance = direction.Length();
            
            float speed = 12f;
            float inertia = 15f;
            
            if (distance > 20f)
            {
                direction.Normalize();
                direction *= Math.Min(distance / 8f, speed);
                Projectile.velocity = (Projectile.velocity * (inertia - 1) + direction) / inertia;
            }
            else
            {
                Projectile.velocity *= 0.9f;
            }
            
            // Re-enable tile collision once close enough
            if (distance < 100f)
            {
                Projectile.tileCollide = true;
            }
            
            // Floating particles
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.IceTorch, 0f, -2f, 100, default, 1.2f);
                dust.noGravity = true;
            }
        }

        private void AttackTarget(NPC target, Player owner, bool onGround)
        {
            Projectile.tileCollide = true;
            
            Vector2 direction = target.Center - Projectile.Center;
            float distance = direction.Length();
            
            // Handle charging state - FREEZE during 2 second charge
            if (isCharging)
            {
                // FREEZE in place during charge
                Projectile.velocity = Vector2.Zero;
                Projectile.Center = frozenPosition;
                State = AIState.ChargingBeam;
                
                chargeTimer++;
                float chargeProgress = (float)chargeTimer / ChargeUpTime;
                
                // MASSIVE swirling particle effect - dark purple and light blue
                if (chargeTimer % 2 == 0)
                {
                    // Inward spiral of particles - multiple rings
                    int particleCount = (int)(8 + chargeProgress * 12); // More particles as charge builds
                    float maxDist = 120f - chargeProgress * 60f; // Shrinks toward center
                    
                    for (int i = 0; i < particleCount; i++)
                    {
                        // Spiral pattern
                        float spiralAngle = (chargeTimer * 0.15f) + (MathHelper.TwoPi * i / particleCount);
                        float dist = maxDist * (0.5f + Main.rand.NextFloat(0.5f));
                        
                        Vector2 dustPos = Projectile.Center + new Vector2((float)Math.Cos(spiralAngle), (float)Math.Sin(spiralAngle)) * dist;
                        Vector2 dustVel = (Projectile.Center - dustPos).SafeNormalize(Vector2.Zero) * (4f + chargeProgress * 6f);
                        
                        // Alternate dark purple and light blue
                        int dustType = i % 2 == 0 ? DustID.PurpleTorch : DustID.IceTorch;
                        Dust dust = Dust.NewDustPerfect(dustPos, dustType, dustVel, 0, default, 2.2f + chargeProgress);
                        dust.noGravity = true;
                        dust.fadeIn = 1.5f;
                    }
                    
                    // Inner vortex particles
                    for (int i = 0; i < 4; i++)
                    {
                        float innerAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                        Vector2 innerPos = Projectile.Center + new Vector2((float)Math.Cos(innerAngle), (float)Math.Sin(innerAngle)) * (20f + Main.rand.NextFloat(30f));
                        Vector2 innerVel = (Projectile.Center - innerPos).SafeNormalize(Vector2.Zero) * 8f;
                        int innerType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.IceTorch;
                        Dust inner = Dust.NewDustPerfect(innerPos, innerType, innerVel, 0, default, 2.5f);
                        inner.noGravity = true;
                        inner.fadeIn = 1.8f;
                    }
                }
                
                // Sparkles converging
                if (chargeTimer % 3 == 0)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                        float sparkDist = 80f - chargeProgress * 40f;
                        Vector2 sparkPos = Projectile.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * sparkDist;
                        Vector2 sparkVel = (Projectile.Center - sparkPos).SafeNormalize(Vector2.Zero) * 5f;
                        Dust sparkle = Dust.NewDustPerfect(sparkPos, DustID.SparksMech, sparkVel, 100, Color.White, 1.5f);
                        sparkle.noGravity = true;
                    }
                }
                
                // Growing glow at center - intensifies over time
                float glowIntensity = 0.3f + chargeProgress * 0.8f;
                Lighting.AddLight(Projectile.Center, 0.4f * glowIntensity, 0.2f * glowIntensity, 0.8f * glowIntensity);
                
                // Electric buildup - more frequent as charge builds
                if (Main.rand.NextFloat() < 0.1f + chargeProgress * 0.4f)
                {
                    Vector2 electricVel = Main.rand.NextVector2Circular(4f, 4f);
                    Dust electric = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(30f, 30f), 
                        DustID.Electric, electricVel, 100, Color.LightBlue, 1.2f + chargeProgress);
                    electric.noGravity = true;
                }
                
                // Shadowflame wisps
                if (chargeTimer % 4 == 0)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 wispPos = Projectile.Center + Main.rand.NextVector2Circular(40f, 40f);
                        Dust wisp = Dust.NewDustPerfect(wispPos, DustID.Shadowflame, 
                            (Projectile.Center - wispPos).SafeNormalize(Vector2.Zero) * 3f, 100, default, 1.5f);
                        wisp.noGravity = true;
                    }
                }
                
                // Sound cues during charge
                if (chargeTimer == 1)
                {
                    SoundEngine.PlaySound(SoundID.Item15 with { Pitch = 0.3f, Volume = 0.6f }, Projectile.Center);
                }
                if (chargeTimer == 40)
                {
                    SoundEngine.PlaySound(SoundID.Item15 with { Pitch = 0.5f, Volume = 0.7f }, Projectile.Center);
                }
                if (chargeTimer == 80)
                {
                    SoundEngine.PlaySound(SoundID.Item15 with { Pitch = 0.7f, Volume = 0.8f }, Projectile.Center);
                }
                if (chargeTimer == 110)
                {
                    SoundEngine.PlaySound(SoundID.Item105 with { Volume = 0.9f }, Projectile.Center);
                }
                
                // Fire devastating beam after 2 second charge (120 ticks)
                if (chargeTimer >= ChargeUpTime)
                {
                    if (chargeTarget != null && chargeTarget.active && Main.myPlayer == Projectile.owner)
                    {
                        FireDevastatingBeam(chargeTarget, owner);
                    }
                    isCharging = false;
                    chargeTimer = 0;
                    chargeTarget = null;
                    
                    // Fractal of Moonlight - 25% faster attack speed
                    var modPlayer = owner.GetModPlayer<MoonlightAccessoryPlayer>();
                    attackCooldown = modPlayer.hasFractalOfMoonlight ? 135 : 180; // Faster cooldown with accessory
                    State = AIState.Attacking;
                }
                return;
            }
            
            // Normal movement when not charging
            // Move toward target on ground
            if (onGround)
            {
                float moveSpeed = 6f;
                float targetX = Math.Sign(direction.X) * moveSpeed;
                
                Projectile.velocity.X = MathHelper.Lerp(Projectile.velocity.X, targetX, 0.1f);
                
                // Jump if target is above
                if (target.Center.Y < Projectile.Center.Y - 80f && Math.Abs(direction.X) < 200f)
                {
                    Projectile.velocity.Y = -10f;
                    
                    // Jump particles
                    for (int i = 0; i < 8; i++)
                    {
                        Dust dust = Dust.NewDustDirect(Projectile.BottomLeft, Projectile.width, 4,
                            DustID.PurpleTorch, Main.rand.NextFloat(-3f, 3f), -2f, 100, default, 1.2f);
                        dust.noGravity = true;
                    }
                }
            }
            else
            {
                // In air - slight horizontal control
                float airControl = 0.08f;
                Projectile.velocity.X += Math.Sign(direction.X) * airControl;
                Projectile.velocity.X = MathHelper.Clamp(Projectile.velocity.X, -8f, 8f);
            }
            
            // Start charging attack
            attackCooldown--;
            if (attackCooldown <= 0 && distance < 600f && !isCharging)
            {
                isCharging = true;
                chargeTimer = 0;
                chargeTarget = target;
                frozenPosition = Projectile.Center; // Lock position
                
                // Initial charge-up sound
                SoundEngine.PlaySound(SoundID.Item15 with { Pitch = 0.2f, Volume = 0.5f }, Projectile.Center);
            }
        }
        
        private void FireDevastatingBeam(NPC target, Player owner)
        {
            Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
            
            // MASSIVE muzzle flash - devastating beam release!
            for (int ring = 0; ring < 4; ring++)
            {
                for (int i = 0; i < 20; i++)
                {
                    float angle = MathHelper.TwoPi * i / 20f;
                    Vector2 dustVel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * (5f + ring * 3f);
                    int flashType = (i + ring) % 2 == 0 ? DustID.IceTorch : DustID.PurpleTorch;
                    Dust dust = Dust.NewDustPerfect(Projectile.Center + toTarget * 20f, flashType, dustVel, 0, default, 2.8f - ring * 0.3f);
                    dust.noGravity = true;
                    dust.fadeIn = 1.8f;
                }
            }
            
            // Massive directional burst toward target
            for (int i = 0; i < 25; i++)
            {
                Vector2 dustVel = toTarget.RotatedByRandom(0.8f) * Main.rand.NextFloat(6f, 15f);
                int flashType = Main.rand.NextBool() ? DustID.IceTorch : DustID.PurpleTorch;
                Dust dust = Dust.NewDustPerfect(Projectile.Center + toTarget * 25f, flashType, dustVel, 0, default, 2.5f);
                dust.noGravity = true;
                dust.fadeIn = 1.5f;
            }
            
            // Electric explosion
            for (int i = 0; i < 15; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(10f, 10f);
                Dust electric = Dust.NewDustPerfect(Projectile.Center + toTarget * 15f, DustID.Electric, dustVel, 100, Color.LightBlue, 1.8f);
                electric.noGravity = true;
            }
            
            // Shadowflame burst
            for (int i = 0; i < 12; i++)
            {
                Vector2 dustVel = toTarget.RotatedByRandom(0.6f) * Main.rand.NextFloat(3f, 8f);
                Dust shadow = Dust.NewDustPerfect(Projectile.Center + toTarget * 18f, DustID.Shadowflame, dustVel, 100, default, 2f);
                shadow.noGravity = true;
            }
            
            // Fire sound - LOUD devastating beam
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.2f, Volume = 1.2f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item125 with { Pitch = 0.3f, Volume = 0.8f }, Projectile.Center);
            
            // Add intense lighting
            Lighting.AddLight(Projectile.Center, 1.2f, 0.6f, 1.5f);
            
            // Calculate damage - 50% bonus for the devastating beam
            int beamDamage = (int)(Projectile.damage * 1.5f);
            
            // Spawn the devastating beam projectile
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                Projectile.Center,
                toTarget,
                ModContent.ProjectileType<GoliathDevastatingBeam>(),
                beamDamage,
                Projectile.knockBack * 2f,
                Projectile.owner
            );
        }

        private void IdleMovement(Player owner, bool onGround)
        {
            Projectile.tileCollide = true;
            
            // Stay near player
            Vector2 targetPos = owner.Center + new Vector2(-60f * owner.direction, 0f);
            Vector2 direction = targetPos - Projectile.Center;
            float distance = direction.Length();
            
            if (onGround)
            {
                // Walk toward target position
                if (Math.Abs(direction.X) > 40f)
                {
                    float walkSpeed = 4f;
                    Projectile.velocity.X = MathHelper.Lerp(Projectile.velocity.X, Math.Sign(direction.X) * walkSpeed, 0.08f);
                }
                else
                {
                    // Slow down when near target
                    Projectile.velocity.X *= 0.9f;
                }
                
                // Jump to follow player if they're above
                if (owner.Center.Y < Projectile.Center.Y - 100f && Math.Abs(direction.X) < 150f)
                {
                    Projectile.velocity.Y = -12f;
                }
            }
            else
            {
                // In air - slight control
                Projectile.velocity.X += Math.Sign(direction.X) * 0.05f;
                Projectile.velocity.X = MathHelper.Clamp(Projectile.velocity.X, -6f, 6f);
            }

            // Teleport if too far
            if (distance > 1500f)
            {
                Projectile.Center = owner.Center + new Vector2(-40f * owner.direction, 0f);
                Projectile.velocity = Vector2.Zero;
                
                // Teleport effect
                for (int i = 0; i < 25; i++)
                {
                    int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.IceTorch;
                    Dust dust = Dust.NewDustDirect(Projectile.Center, 1, 1, dustType, 
                        Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f), 100, default, 1.8f);
                    dust.noGravity = true;
                }
                
                SoundEngine.PlaySound(SoundID.Item8, Projectile.Center);
            }
        }
        
        private void CreateAmbientEffects()
        {
            // PRONOUNCED ambient purple/blue particles
            if (Main.rand.NextBool(2))
            {
                int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.IceTorch;
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
                    dustType, 0f, -2f, 0, default, 1.8f);
                dust.noGravity = true;
                dust.velocity *= 0.5f;
                dust.fadeIn = 1.2f;
            }
            
            // Constant sparkles
            if (Main.rand.NextBool(3))
            {
                Dust sparkle = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(Projectile.width / 2f, Projectile.height / 2f),
                    DustID.SparksMech, new Vector2(0, -1f), 100, Color.White, 1.2f);
                sparkle.noGravity = true;
            }
            
            // Orbiting particles
            if (Main.GameUpdateCount % 2 == 0)
            {
                float orbitAngle = Main.GameUpdateCount * 0.08f;
                Vector2 orbitOffset = new Vector2((float)Math.Cos(orbitAngle), (float)Math.Sin(orbitAngle)) * 35f;
                int orbitType = Main.GameUpdateCount % 4 < 2 ? DustID.PurpleTorch : DustID.IceTorch;
                Dust orbit = Dust.NewDustPerfect(Projectile.Center + orbitOffset, orbitType, Vector2.Zero, 0, default, 1.5f);
                orbit.noGravity = true;
                orbit.fadeIn = 1f;
            }
            
            // Shadowflame wisps rising
            if (Main.rand.NextBool(6))
            {
                Vector2 wispPos = Projectile.Center + Main.rand.NextVector2Circular(Projectile.width / 3f, Projectile.height / 3f);
                Dust wisp = Dust.NewDustPerfect(wispPos, DustID.Shadowflame, new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -2f), 100, default, 1.3f);
                wisp.noGravity = true;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply Musical Dissonance debuff
            target.AddBuff(ModContent.BuffType<MusicsDissonance>(), 180);

            // MASSIVE hit effect - very visible!
            // Ring explosion
            for (int ring = 0; ring < 2; ring++)
            {
                for (int i = 0; i < 20; i++)
                {
                    float angle = MathHelper.TwoPi * i / 20f;
                    Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * (4f + ring * 3f);
                    int dustType = (i + ring) % 2 == 0 ? DustID.PurpleTorch : DustID.IceTorch;
                    Dust dust = Dust.NewDustPerfect(target.Center, dustType, vel, 0, default, 2f - ring * 0.3f);
                    dust.noGravity = true;
                    dust.fadeIn = 1.3f;
                }
            }
            
            // Random burst
            for (int i = 0; i < 15; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.IceTorch;
                Dust dust = Dust.NewDustPerfect(target.Center, dustType, vel, 0, default, 1.8f);
                dust.noGravity = true;
            }
            
            // Sparkles
            for (int i = 0; i < 10; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Dust sparkle = Dust.NewDustPerfect(target.Center, DustID.SparksMech, vel, 100, Color.White, 1.3f);
                sparkle.noGravity = true;
            }
            
            // Add lighting on hit
            Lighting.AddLight(target.Center, 0.6f, 0.3f, 0.8f);
        }
        
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // Fractal of Moonlight - +50% damage boost for Moonlight minions
            Player owner = Main.player[Projectile.owner];
            var modPlayer = owner.GetModPlayer<MoonlightAccessoryPlayer>();
            
            if (modPlayer.hasFractalOfMoonlight)
            {
                modifiers.FinalDamage *= 1.5f;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            
            // Get the spritesheet texture
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;
            
            // Calculate frame dimensions
            int frameWidth = texture.Width / FrameColumns;
            int frameHeight = texture.Height / FrameRows;
            
            // Get current frame position in grid
            int col = currentFrame % FrameColumns;
            int row = currentFrame / FrameColumns;
            
            Rectangle sourceRect = new Rectangle(col * frameWidth, row * frameHeight, frameWidth, frameHeight);
            // Origin at bottom-center so sprite sits ON the ground, not in it
            Vector2 origin = new Vector2(frameWidth / 2f, frameHeight);
            // Draw from bottom of projectile hitbox
            Vector2 drawPos = new Vector2(Projectile.Center.X, Projectile.position.Y + Projectile.height) - Main.screenPosition;
            
            // Flip sprite based on direction
            SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            
            // === CHARGING VFX - Dramatic additive blending effects ===
            if (isCharging && chargeTimer > 0)
            {
                float chargeProgress = (float)chargeTimer / ChargeUpTime;
                
                // Switch to additive blending for glows
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                
                // Outer pulsing energy rings
                for (int ring = 0; ring < 3; ring++)
                {
                    float ringProgress = (chargeTimer * 0.05f + ring * 0.3f) % 1f;
                    float ringSize = 80f + (1f - ringProgress) * 60f;
                    float ringAlpha = ringProgress * (1f - ringProgress) * 4f * chargeProgress;
                    
                    Color ringColor = ring % 2 == 0 ? new Color(100, 50, 180) : new Color(100, 180, 255);
                    ringColor *= ringAlpha * 0.5f;
                    
                    // Draw ring as circle of points
                    for (int i = 0; i < 24; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 24f;
                        Vector2 ringPos = Projectile.Center - Main.screenPosition + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * ringSize;
                        spriteBatch.Draw(pixel, ringPos, new Rectangle(0, 0, 1, 1), ringColor, 0f, new Vector2(0.5f), 4f, SpriteEffects.None, 0f);
                    }
                }
                
                // Central energy buildup glow
                float coreSize = 30f + chargeProgress * 50f;
                float corePulse = (float)Math.Sin(chargeTimer * 0.3f) * 0.2f + 0.8f;
                
                // Outer purple aura
                Color outerGlow = new Color(120, 60, 180) * chargeProgress * corePulse * 0.6f;
                spriteBatch.Draw(pixel, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 1, 1), outerGlow, 0f, new Vector2(0.5f), coreSize * 1.5f, SpriteEffects.None, 0f);
                
                // Mid blue glow
                Color midGlow = new Color(100, 150, 255) * chargeProgress * corePulse * 0.7f;
                spriteBatch.Draw(pixel, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 1, 1), midGlow, 0f, new Vector2(0.5f), coreSize, SpriteEffects.None, 0f);
                
                // Inner white-hot core
                Color coreGlow = new Color(220, 200, 255) * chargeProgress * corePulse * 0.8f;
                spriteBatch.Draw(pixel, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 1, 1), coreGlow, 0f, new Vector2(0.5f), coreSize * 0.5f, SpriteEffects.None, 0f);
                
                // Energy tendrils spiraling inward (drawn as line segments)
                int tendrilCount = 6;
                for (int t = 0; t < tendrilCount; t++)
                {
                    float baseAngle = chargeTimer * 0.08f + (MathHelper.TwoPi * t / tendrilCount);
                    float tendrilLength = 100f - chargeProgress * 40f;
                    
                    for (int seg = 0; seg < 8; seg++)
                    {
                        float segProgress = seg / 8f;
                        float dist = tendrilLength * (1f - segProgress);
                        float angle = baseAngle + segProgress * 0.5f;
                        
                        Vector2 segPos = Projectile.Center - Main.screenPosition + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * dist;
                        float segAlpha = segProgress * chargeProgress;
                        
                        Color tendrilColor = t % 2 == 0 ? new Color(150, 80, 200) : new Color(120, 180, 255);
                        tendrilColor *= segAlpha * 0.6f;
                        
                        spriteBatch.Draw(pixel, segPos, new Rectangle(0, 0, 1, 1), tendrilColor, 0f, new Vector2(0.5f), 6f * segAlpha, SpriteEffects.None, 0f);
                    }
                }
                
                // Restore normal blending
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            
            // Draw purple glow behind (normal glow, enhanced during charge)
            float glowMult = isCharging ? 1f + (float)chargeTimer / ChargeUpTime * 0.5f : 1f;
            Color purpleGlow = new Color(150, 80, 200, 0) * 0.4f * glowMult;
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(4f, 0f).RotatedBy(i * MathHelper.PiOver2 + Main.GameUpdateCount * 0.05f);
                Main.EntitySpriteDraw(texture, drawPos + offset, sourceRect, purpleGlow, 0f, origin, Projectile.scale * 1.1f, effects, 0);
            }
            
            // Draw light blue inner glow
            Color blueGlow = new Color(150, 200, 255, 0) * 0.3f * glowMult;
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(2f, 0f).RotatedBy(i * MathHelper.PiOver2 + Main.GameUpdateCount * 0.08f);
                Main.EntitySpriteDraw(texture, drawPos + offset, sourceRect, blueGlow, 0f, origin, Projectile.scale * 1.05f, effects, 0);
            }
            
            // Draw main sprite
            Main.EntitySpriteDraw(texture, drawPos, sourceRect, lightColor, 0f, origin, Projectile.scale, effects, 0);
            
            return false;
        }
    }
}
