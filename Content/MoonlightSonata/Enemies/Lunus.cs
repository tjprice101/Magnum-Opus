using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.MoonlightSonata.ResonantOres;
using MagnumOpus.Content.MoonlightSonata.ResonanceEnergies;

namespace MagnumOpus.Content.MoonlightSonata.Enemies
{
    /// <summary>
    /// Lunus - A lunar beast that spawns at night after Moon Lord is defeated.
    /// Fast, agile creature with fluid, dynamic movement and multiple projectile attacks.
    /// </summary>
    public class Lunus : ModNPC
    {
        // AI States - More dynamic and fluid
        private enum AIState
        {
            Idle,
            Approaching,      // Moving toward player with weaving
            DashThrough,      // Dash past player and circle back
            Circling,         // Circle/strafe around player
            Retreating,       // Back off to reposition
            Jumping,
            Attacking
        }

        private AIState CurrentState
        {
            get => (AIState)NPC.ai[0];
            set => NPC.ai[0] = (float)value;
        }

        private float StateTimer
        {
            get => NPC.ai[1];
            set => NPC.ai[1] = value;
        }

        private float AttackCooldown
        {
            get => NPC.ai[2];
            set => NPC.ai[2] = value;
        }

        private float JumpCooldown
        {
            get => NPC.ai[3];
            set => NPC.ai[3] = value;
        }
        
        // Fluid movement variables
        private float waveOffset = 0f;
        private int dashDirection = 1; // 1 = right, -1 = left
        private float circleAngle = 0f;
        private Vector2 retreatTarget = Vector2.Zero;

        // Orbiting lantern projectiles
        private int[] orbitingLanterns = new int[2] { -1, -1 };
        
        // Animation - 6x6 sprite sheet (36 frames)
        private int frameCounter = 0;
        private int currentFrame = 0;
        private const int FrameTime = 4; // Ticks per frame
        private const int FrameColumns = 6;
        private const int FrameRows = 6;
        private const int TotalFrames = 36;

        // Movement tracking for idle detection
        private int lastSpriteDirection = 1;
        private bool isMoving = false;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = TotalFrames; // 36 frames for animation
            
            // Immune to common debuffs
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;

            // Make it count as a post-Moon Lord enemy
            NPCID.Sets.DangerDetectRange[Type] = 400;
        }

        public override void SetDefaults()
        {
            // Celestial enemies have around 4000-5000 HP, so much higher for post-Moon Lord
            NPC.width = 43;
            NPC.height = 43;
            NPC.damage = 90; // Celestial enemies deal ~80, slightly higher
            NPC.defense = 50; // Good defense
            NPC.lifeMax = 13000; // Double health for challenge
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath6;
            NPC.knockBackResist = 0.2f; // Slight knockback resistance
            NPC.value = Item.buyPrice(gold: 5); // Decent coin drop
            NPC.aiStyle = -1; // Custom AI
            
            // Has gravity, walks on ground
            NPC.noGravity = false;
            NPC.noTileCollide = false;
            
            // Visual offset to align sprite with hitbox (larger value = draw higher)
            DrawOffsetY = -45f;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.NightTime,
                new FlavorTextBestiaryInfoElement("A mystical lunar beast that emerged after the Moon Lord's defeat. Its movements are swift as moonbeams, and it commands the very essence of lunar energy.")
            });
        }

        public override void AI()
        {
            Player target = Main.player[NPC.target];
            
            // Target validation
            if (!target.active || target.dead)
            {
                NPC.TargetClosest(true);
                target = Main.player[NPC.target];
                
                if (!target.active || target.dead)
                {
                    // No valid target, despawn slowly
                    NPC.velocity.Y += 0.1f;
                    NPC.timeLeft = Math.Min(NPC.timeLeft, 60);
                    return;
                }
            }

            // Manage orbiting lanterns
            ManageOrbitingLanterns();

            // Add ambient lighting - white glow
            Lighting.AddLight(NPC.Center, 0.6f, 0.6f, 0.8f);

            // White shimmer particles
            if (Main.rand.NextBool(5))
            {
                Dust shimmer = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.SparksMech, 0f, 0f, 0, Color.White, 1.2f);
                shimmer.noGravity = true;
                shimmer.velocity *= 0.3f;
            }

            // Purple/blue ambient particles
            if (Main.rand.NextBool(8))
            {
                int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.IceTorch;
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, dustType, 0f, 0f, 100, default, 0.8f);
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }

            float distanceToTarget = Vector2.Distance(NPC.Center, target.Center);
            bool canSeeTarget = Collision.CanHitLine(NPC.Center, 1, 1, target.Center, 1, 1);

            // Decrement cooldowns
            if (AttackCooldown > 0) AttackCooldown--;
            if (JumpCooldown > 0) JumpCooldown--;

            // State machine
            switch (CurrentState)
            {
                case AIState.Idle:
                    HandleIdleState(target, distanceToTarget, canSeeTarget);
                    break;
                case AIState.Approaching:
                    HandleApproachingState(target, distanceToTarget, canSeeTarget);
                    break;
                case AIState.DashThrough:
                    HandleDashThroughState(target, distanceToTarget);
                    break;
                case AIState.Circling:
                    HandleCirclingState(target, distanceToTarget, canSeeTarget);
                    break;
                case AIState.Retreating:
                    HandleRetreatingState(target, distanceToTarget);
                    break;
                case AIState.Jumping:
                    HandleJumpingState(target);
                    break;
                case AIState.Attacking:
                    HandleAttackingState(target);
                    break;
            }

            // Sprite direction
            if (NPC.velocity.X > 0.5f)
                NPC.spriteDirection = 1;
            else if (NPC.velocity.X < -0.5f)
                NPC.spriteDirection = -1;
        }

        private void HandleIdleState(Player target, float distance, bool canSee)
        {
            StateTimer++;
            waveOffset += 0.05f;

            // Gentle wandering with slight wave motion
            if (StateTimer % 60 == 0 && NPC.velocity.Y == 0)
            {
                NPC.velocity.X = Main.rand.NextFloat(-3f, 3f);
            }

            // Add gentle wave to movement
            if (NPC.velocity.Y == 0)
            {
                NPC.velocity.X += (float)Math.Sin(waveOffset) * 0.1f;
                NPC.velocity.X *= 0.95f;
            }

            // Spot player and switch to approaching with fluid transition
            if (canSee && distance < 600f)
            {
                CurrentState = AIState.Approaching;
                StateTimer = 0;
                dashDirection = target.Center.X > NPC.Center.X ? 1 : -1;
                
                // Alert dust burst
                for (int i = 0; i < 15; i++)
                {
                    Dust alert = Dust.NewDustDirect(NPC.Center, 1, 1, DustID.PurpleTorch, 0f, 0f, 0, default, 1.5f);
                    alert.noGravity = true;
                    alert.velocity = Main.rand.NextVector2Circular(5f, 5f);
                }
            }
        }

        private void HandleApproachingState(Player target, float distance, bool canSee)
        {
            StateTimer++;
            waveOffset += 0.1f;

            // Fluid weaving approach - not just straight at player
            float baseSpeed = 6f;
            float waveAmplitude = 2.5f;
            float waveSpeed = (float)Math.Sin(waveOffset) * waveAmplitude;

            // Move toward player with wave pattern
            float dirToPlayer = target.Center.X > NPC.Center.X ? 1f : -1f;
            float targetVelX = dirToPlayer * baseSpeed + waveSpeed;
            
            NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, targetVelX, 0.08f);

            // Randomly decide to dash through player
            if (distance < 300f && distance > 100f && Main.rand.NextBool(60) && NPC.velocity.Y == 0)
            {
                CurrentState = AIState.DashThrough;
                StateTimer = 0;
                dashDirection = (int)dirToPlayer;
                
                // Telegraph
                for (int i = 0; i < 8; i++)
                {
                    Dust dash = Dust.NewDustDirect(NPC.Center, 1, 1, DustID.SparksMech, dashDirection * 3f, 0f, 0, Color.White, 1.5f);
                    dash.noGravity = true;
                }
            }

            // Randomly start circling
            if (distance < 250f && Main.rand.NextBool(90) && NPC.velocity.Y == 0)
            {
                CurrentState = AIState.Circling;
                StateTimer = 0;
                circleAngle = (float)Math.Atan2(NPC.Center.Y - target.Center.Y, NPC.Center.X - target.Center.X);
            }

            // Only jump when target is above (small hop)
            bool targetAbove = target.Center.Y < NPC.Center.Y - 60f;

            if (NPC.velocity.Y == 0 && JumpCooldown <= 0 && targetAbove)
            {
                float jumpPower = -8f; // Small hop
                NPC.velocity.Y = jumpPower;
                JumpCooldown = 60;
                CurrentState = AIState.Jumping;
                StateTimer = 0;

                for (int i = 0; i < 10; i++)
                {
                    Dust jumpDust = Dust.NewDustDirect(NPC.BottomLeft, NPC.width, 4, DustID.Smoke, 0f, 0f, 100, default, 1.5f);
                    jumpDust.velocity = new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-2f, 0f));
                }
            }

            // Attack when in range
            if (distance < 500f && canSee && AttackCooldown <= 0)
            {
                CurrentState = AIState.Attacking;
                StateTimer = 0;
            }

            // Lose interest if target too far
            if (distance > 1200f || !canSee)
            {
                StateTimer++;
                if (StateTimer > 180)
                {
                    CurrentState = AIState.Idle;
                    StateTimer = 0;
                }
            }
        }

        private void HandleDashThroughState(Player target, float distance)
        {
            StateTimer++;

            if (StateTimer < 15)
            {
                // Wind up - slow down
                NPC.velocity.X *= 0.85f;
                
                // Charging particles
                for (int i = 0; i < 3; i++)
                {
                    Dust charge = Dust.NewDustDirect(NPC.Center + new Vector2(dashDirection * 30f, 0), 1, 1, DustID.PurpleTorch, -dashDirection * 2f, 0f, 0, default, 1.3f);
                    charge.noGravity = true;
                }
            }
            else if (StateTimer == 15)
            {
                // DASH! Fast movement past the player
                NPC.velocity.X = dashDirection * 18f;
                
                // Burst particles
                for (int i = 0; i < 15; i++)
                {
                    Dust burst = Dust.NewDustDirect(NPC.Center, 1, 1, DustID.SparksMech, 0f, 0f, 0, Color.White, 1.8f);
                    burst.noGravity = true;
                    burst.velocity = new Vector2(-dashDirection * Main.rand.NextFloat(2f, 5f), Main.rand.NextFloat(-2f, 2f));
                }
                
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item24, NPC.Center);
            }
            else if (StateTimer > 15 && StateTimer < 35)
            {
                // Maintain dash speed with slight slowdown
                NPC.velocity.X *= 0.97f;
                
                // Trail particles
                if (StateTimer % 2 == 0)
                {
                    Dust trail = Dust.NewDustDirect(NPC.Center, 1, 1, DustID.PurpleTorch, 0f, 0f, 100, default, 1.2f);
                    trail.noGravity = true;
                    trail.velocity = -NPC.velocity * 0.1f;
                }
            }
            else if (StateTimer >= 35)
            {
                // Finished dash - decide next action
                float newDist = Vector2.Distance(NPC.Center, target.Center);
                
                if (newDist > 200f)
                {
                    // Too far, retreat and come back
                    CurrentState = AIState.Retreating;
                    StateTimer = 0;
                    retreatTarget = NPC.Center + new Vector2(-dashDirection * 100f, 0);
                }
                else
                {
                    // Close enough, start circling or approach again
                    if (Main.rand.NextBool())
                    {
                        CurrentState = AIState.Circling;
                        circleAngle = (float)Math.Atan2(NPC.Center.Y - target.Center.Y, NPC.Center.X - target.Center.X);
                    }
                    else
                    {
                        CurrentState = AIState.Approaching;
                    }
                    StateTimer = 0;
                }
            }
        }

        private void HandleCirclingState(Player target, float distance, bool canSee)
        {
            StateTimer++;
            
            // Circle around the player
            float circleRadius = 180f;
            float circleSpeed = 0.04f;
            circleAngle += circleSpeed * dashDirection;

            Vector2 targetPos = target.Center + new Vector2(
                (float)Math.Cos(circleAngle) * circleRadius,
                0 // Stay on ground level
            );

            // Move toward circle position
            float dirX = targetPos.X > NPC.Center.X ? 1f : -1f;
            NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, dirX * 5f, 0.1f);

            // Attack while circling
            if (canSee && AttackCooldown <= 0 && Main.rand.NextBool(30))
            {
                CurrentState = AIState.Attacking;
                StateTimer = 0;
                return;
            }

            // Exit circling after a while or randomly
            if (StateTimer > 120 || Main.rand.NextBool(120))
            {
                // Choose next behavior
                int choice = Main.rand.Next(3);
                if (choice == 0)
                {
                    CurrentState = AIState.DashThrough;
                    dashDirection = target.Center.X > NPC.Center.X ? 1 : -1;
                }
                else if (choice == 1)
                {
                    CurrentState = AIState.Retreating;
                    retreatTarget = NPC.Center + new Vector2(Main.rand.NextBool() ? -200f : 200f, 0);
                }
                else
                {
                    CurrentState = AIState.Approaching;
                }
                StateTimer = 0;
            }
        }

        private void HandleRetreatingState(Player target, float distance)
        {
            StateTimer++;

            // Move away from player briefly
            float dirAway = NPC.Center.X > target.Center.X ? 1f : -1f;
            NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, dirAway * 7f, 0.12f);

            // Add some wave motion even while retreating
            waveOffset += 0.08f;
            NPC.velocity.X += (float)Math.Sin(waveOffset * 2f) * 0.5f;

            // Return to approaching after brief retreat
            if (StateTimer > 45 || distance > 400f)
            {
                CurrentState = AIState.Approaching;
                StateTimer = 0;
            }
        }

        private void HandleJumpingState(Player target)
        {
            StateTimer++;

            // Fluid air control with wave motion
            waveOffset += 0.15f;
            float airControl = 0.24f;
            float waveInfluence = (float)Math.Sin(waveOffset) * 0.8f;
            
            if (target.Center.X > NPC.Center.X)
                NPC.velocity.X += airControl + waveInfluence * 0.1f;
            else
                NPC.velocity.X -= airControl + waveInfluence * 0.1f;

            // Clamp horizontal speed
            NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -8f, 8f);

            // Return to approaching when landed (not chasing - more fluid)
            if (NPC.velocity.Y == 0 && StateTimer > 10)
            {
                CurrentState = AIState.Approaching;
                StateTimer = 0;
            }
        }

        private void HandleAttackingState(Player target)
        {
            StateTimer++;

            // Slow down during attack but maintain slight movement for fluidity
            NPC.velocity.X *= 0.92f;
            waveOffset += 0.05f;
            NPC.velocity.X += (float)Math.Sin(waveOffset * 3f) * 0.3f; // Subtle sway

            // Attack telegraph
            if (StateTimer < 30)
            {
                // Charging particles
                if (StateTimer % 5 == 0)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        Vector2 offset = Main.rand.NextVector2Circular(40f, 40f);
                        Dust charge = Dust.NewDustDirect(NPC.Center + offset, 1, 1, DustID.PurpleTorch, 0f, 0f, 0, default, 1.2f);
                        charge.noGravity = true;
                        charge.velocity = (NPC.Center - charge.position) * 0.1f;
                    }
                }
            }
            else if (StateTimer == 30)
            {
                // Fire attack
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Vector2 direction = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY);

                    // Choose attack type
                    int attackType = Main.rand.Next(3);
                    
                    if (attackType == 0)
                    {
                        // Moonlight Flare (orb) - single accurate shot
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, direction * 12f,
                            ModContent.ProjectileType<MoonlightFlareProjectile>(), 50, 2f, Main.myPlayer);
                    }
                    else if (attackType == 1)
                    {
                        // Lunar Blaze (flaming ball) - spread shot
                        for (int i = -1; i <= 1; i++)
                        {
                            Vector2 spreadDir = direction.RotatedBy(MathHelper.ToRadians(15f * i));
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, spreadDir * 10f,
                                ModContent.ProjectileType<LunarBlazeProjectile>(), 45, 1.5f, Main.myPlayer);
                        }
                    }
                    else
                    {
                        // Combined attack - orb + blazes
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, direction * 11f,
                            ModContent.ProjectileType<MoonlightFlareProjectile>(), 50, 2f, Main.myPlayer);
                        
                        Vector2 spreadDir1 = direction.RotatedBy(MathHelper.ToRadians(25f));
                        Vector2 spreadDir2 = direction.RotatedBy(MathHelper.ToRadians(-25f));
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, spreadDir1 * 9f,
                            ModContent.ProjectileType<LunarBlazeProjectile>(), 40, 1f, Main.myPlayer);
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, spreadDir2 * 9f,
                            ModContent.ProjectileType<LunarBlazeProjectile>(), 40, 1f, Main.myPlayer);
                    }

                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Item117, NPC.Center);
                }
            }
            else if (StateTimer >= 60)
            {
                // Return to fluid behavior - pick randomly
                int choice = Main.rand.Next(4);
                if (choice == 0)
                    CurrentState = AIState.DashThrough;
                else if (choice == 1)
                    CurrentState = AIState.Circling;
                else if (choice == 2)
                    CurrentState = AIState.Retreating;
                else
                    CurrentState = AIState.Approaching;
                    
                dashDirection = target.Center.X > NPC.Center.X ? 1 : -1;
                circleAngle = (float)Math.Atan2(NPC.Center.Y - target.Center.Y, NPC.Center.X - target.Center.X);
                StateTimer = 0;
                AttackCooldown = 90; // 1.5 second cooldown
            }
        }

        private void ManageOrbitingLanterns()
        {
            // Spawn or maintain 2 orbiting lanterns
            for (int i = 0; i < 2; i++)
            {
                bool lanternValid = orbitingLanterns[i] >= 0 && 
                                   orbitingLanterns[i] < Main.maxProjectiles &&
                                   Main.projectile[orbitingLanterns[i]].active &&
                                   Main.projectile[orbitingLanterns[i]].type == ModContent.ProjectileType<DiatonicMoonLanternOrbiting>() &&
                                   (int)Main.projectile[orbitingLanterns[i]].ai[0] == NPC.whoAmI;

                if (!lanternValid && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    // Spawn a new orbiting lantern
                    float angleOffset = i * MathHelper.Pi; // 180 degrees apart
                    int proj = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero,
                        ModContent.ProjectileType<DiatonicMoonLanternOrbiting>(), 0, 0f, Main.myPlayer, NPC.whoAmI, angleOffset);
                    orbitingLanterns[i] = proj;
                }
            }
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            // Only spawn at night after Moon Lord is defeated, on the surface
            if (!Main.dayTime && 
                NPC.downedMoonlord && 
                spawnInfo.Player.ZoneOverworldHeight &&
                !spawnInfo.PlayerSafe)
            {
                return 0.25f; // 25% spawn rate
            }
            return 0f;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            // Lunus only drops Shards of Moonlit Tempo
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ShardsOfMoonlitTempo>(), 1, 2, 5));
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            // Hurt particles
            for (int i = 0; i < 10; i++)
            {
                Dust hurt = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.PurpleTorch, 0f, 0f, 100, default, 1.5f);
                hurt.noGravity = true;
                hurt.velocity = Main.rand.NextVector2Circular(5f, 5f);
            }

            // Death effect
            if (NPC.life <= 0)
            {
                for (int i = 0; i < 30; i++)
                {
                    Dust death = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.PurpleTorch, 0f, 0f, 100, default, 2f);
                    death.noGravity = true;
                    death.velocity = Main.rand.NextVector2Circular(10f, 10f);
                }

                // Additional blue particles
                for (int i = 0; i < 20; i++)
                {
                    Dust blue = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.BlueTorch, 0f, 0f, 100, default, 1.5f);
                    blue.noGravity = true;
                    blue.velocity = Main.rand.NextVector2Circular(8f, 8f);
                }
            }
        }
        
        public override void FindFrame(int frameHeight)
        {
            // Check if moving
            float movementThreshold = 0.5f;
            isMoving = Math.Abs(NPC.velocity.X) > movementThreshold || Math.Abs(NPC.velocity.Y) > movementThreshold;

            // Only update sprite direction when moving
            if (isMoving)
            {
                if (NPC.velocity.X > 0.5f)
                    lastSpriteDirection = 1;
                else if (NPC.velocity.X < -0.5f)
                    lastSpriteDirection = -1;
            }
            NPC.spriteDirection = lastSpriteDirection;

            // Animation update - only animate when moving
            if (isMoving)
            {
                frameCounter++;
                
                // Faster animation when moving fast
                int animSpeed = Math.Abs(NPC.velocity.X) > 1f ? 3 : FrameTime;
                
                if (frameCounter >= animSpeed)
                {
                    frameCounter = 0;
                    currentFrame++;
                    if (currentFrame >= TotalFrames)
                        currentFrame = 0;
                }
            }
            else
            {
                // Idle - show first frame
                currentFrame = 0;
                frameCounter = 0;
            }
            
            // Calculate frame position in sprite sheet
            int frameX = currentFrame % FrameColumns;
            int frameY = currentFrame / FrameColumns;
            
            // Set frame (NPC.frame is used by vanilla drawing)
            NPC.frame.Y = currentFrame * frameHeight;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Npc[Type].Value;
            
            // Calculate frame dimensions from 6x6 sprite sheet
            int frameWidth = texture.Width / FrameColumns;
            int frameHeight = texture.Height / FrameRows;
            
            // Get current frame position
            int frameX = currentFrame % FrameColumns;
            int frameY = currentFrame / FrameColumns;
            
            Rectangle sourceRect = new Rectangle(frameX * frameWidth, frameY * frameHeight, frameWidth, frameHeight);
            Vector2 drawPos = NPC.Center - screenPos + new Vector2(0f, DrawOffsetY);
            Vector2 origin = new Vector2(frameWidth / 2, frameHeight / 2);

            // Use explicit scale to ensure proper size - 1.8x for better visibility
            float drawScale = 1.8f;

            // White glow effect
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.2f + 0.8f;
            Color whiteGlow = Color.White * pulse * 0.4f;

            // Draw white glow layers
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(4f, 0f).RotatedBy(MathHelper.TwoPi * i / 4);
                spriteBatch.Draw(texture, drawPos + offset, sourceRect, whiteGlow, NPC.rotation,
                    origin, drawScale, NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
            }

            // Draw main sprite
            spriteBatch.Draw(texture, drawPos, sourceRect, drawColor, NPC.rotation,
                origin, drawScale, NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
            
            return false;
        }
    }
}
