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
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.MoonlightSonata.Enemies
{
    /// <summary>
    /// Waning Deer - A graceful lunar deer that spawns in snow biomes after Moon Lord is defeated.
    /// Fast, agile creature with fluid, dance-like movements that shoots Snow of the Moon.
    /// </summary>
    public class WaningDeer : ModNPC
    {
        // AI States - Graceful and fluid like a deer
        private enum AIState
        {
            Idle,
            Approaching,      // Graceful approach with prancing
            LeapPast,         // Graceful leap past player
            Prancing,         // Side-to-side prancing
            GracefulRetreat,  // Elegant backward movement
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
        private float graceOffset = 0f;
        private int pranceDirection = 1;
        private float pranceAngle = 0f;
        private Vector2 leapTarget = Vector2.Zero;

        // Animation variables for 6x6 sprite sheet
        private int frameCounter = 0;
        private int currentFrame = 0;
        private const int FrameTime = 4;
        private const int FrameColumns = 6;
        private const int FrameRows = 6;
        private const int TotalFrames = 36;

        // Orbiting chandelier projectiles (2-3)
        private int[] orbitingChandeliers = new int[3] { -1, -1, -1 };

        // Movement tracking for idle detection
        private int lastSpriteDirection = 1;
        private bool isActuallyMoving = false;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = TotalFrames; // 36 frames for 6x6 sprite sheet
            
            // Immune to common debuffs
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Frostburn] = true;

            // Make it count as a post-Moon Lord enemy
            NPCID.Sets.DangerDetectRange[Type] = 400;
        }

        public override void SetDefaults()
        {
            // Similar to Lunus stats
            // Hitbox matches visual size: ~170px frame × 1.8f drawScale = ~306px
            NPC.width = 280;
            NPC.height = 280;
            NPC.damage = 85; // Slightly less than Lunus
            NPC.defense = 45; // Good defense
            NPC.lifeMax = 12000; // Double health for challenge
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath6;
            NPC.knockBackResist = 0.25f; // Slight knockback resistance
            NPC.value = Item.buyPrice(gold: 4, silver: 50); // Decent coin drop
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
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Snow,
                new FlavorTextBestiaryInfoElement("A spectral deer born from the frozen tears of the moon. It glides through snow with ethereal grace, accompanied by floating chandeliers of lunar ice.")
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

            // Manage orbiting chandeliers (2-3 of them)
            ManageOrbitingChandeliers();

            // Add ambient lighting - icy blue/purple
            Lighting.AddLight(NPC.Center, 0.3f, 0.5f, 0.7f);
            
            // Moonlight halo aura pulse for graceful deer
            if (Main.GameUpdateCount % 30 == 0)
            {
                CustomParticles.MoonlightHalo(NPC.Center, 0.35f);
            }
            
            // Trail effect while moving
            if (NPC.velocity.Length() > 2f)
            {
                CustomParticles.MoonlightTrail(NPC.Center, NPC.velocity, 0.2f);
            }

            // Enhanced ambient particles using ThemedParticles
            ThemedParticles.MoonlightAura(NPC.Center, 35f);
            
            // Custom particle icy moonlight glow (blends swan lake icy blue with moonlight purple)
            if (Main.rand.NextBool(10))
            {
                CustomParticles.MoonlightFlare(NPC.Center + Main.rand.NextVector2Circular(28f, 28f), 0.3f);
            }
            if (Main.rand.NextBool(15))
            {
                CustomParticles.SwanLakeFlare(NPC.Center + Main.rand.NextVector2Circular(25f, 25f), 0.22f);
            }
            
            // Occasional shimmer and snow particles
            if (Main.rand.NextBool(10))
            {
                ThemedParticles.MoonlightSparkles(NPC.Center, 4, 30f);
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
                case AIState.LeapPast:
                    HandleLeapPastState(target, distanceToTarget);
                    break;
                case AIState.Prancing:
                    HandlePrancingState(target, distanceToTarget, canSeeTarget);
                    break;
                case AIState.GracefulRetreat:
                    HandleGracefulRetreatState(target, distanceToTarget);
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
            graceOffset += 0.04f;

            // Graceful idle wandering with bobbing motion
            if (StateTimer % 50 == 0 && NPC.velocity.Y == 0)
            {
                NPC.velocity.X = Main.rand.NextFloat(-2.5f, 2.5f);
                // Small hop while idle
                if (Main.rand.NextBool(3))
                {
                    NPC.velocity.Y = -5f;
                }
            }

            // Graceful swaying motion
            if (NPC.velocity.Y == 0)
            {
                NPC.velocity.X += (float)Math.Sin(graceOffset) * 0.08f;
                NPC.velocity.X *= 0.96f;
            }

            // Spot player and switch to graceful approach
            if (canSee && distance < 600f)
            {
                CurrentState = AIState.Approaching;
                StateTimer = 0;
                pranceDirection = target.Center.X > NPC.Center.X ? 1 : -1;
                
                // Enhanced alert burst with ThemedParticles
                ThemedParticles.MoonlightBloomBurst(NPC.Center, 0.8f);
                ThemedParticles.MoonlightShockwave(NPC.Center, 0.5f);
                
                // Musical alert - floating notes when deer spots player
                ThemedParticles.MoonlightMusicNotes(NPC.Center, 5, 30f);
            }
        }

        private void HandleApproachingState(Player target, float distance, bool canSee)
        {
            StateTimer++;
            graceOffset += 0.12f;

            // Graceful prancing approach - deer-like bounding motion
            float baseSpeed = 7f;
            float pranceWave = (float)Math.Sin(graceOffset * 1.5f) * 2f;

            float dirToPlayer = target.Center.X > NPC.Center.X ? 1f : -1f;
            float targetVelX = dirToPlayer * baseSpeed + pranceWave;
            
            NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, targetVelX, 0.1f);

            // Small hops while approaching (prancing)
            if (NPC.velocity.Y == 0 && Main.rand.NextBool(25))
            {
                NPC.velocity.Y = -6f;
                
                // Snow puff on hop
                for (int i = 0; i < 4; i++)
                {
                    Dust snow = Dust.NewDustDirect(NPC.BottomLeft, NPC.width, 4, DustID.Snow, 0f, 0f, 100, default, 1f);
                    snow.velocity = new Vector2(Main.rand.NextFloat(-2f, 2f), -1f);
                }
            }

            // Graceful leap past player
            if (distance < 280f && distance > 80f && Main.rand.NextBool(50) && NPC.velocity.Y == 0)
            {
                CurrentState = AIState.LeapPast;
                StateTimer = 0;
                pranceDirection = (int)dirToPlayer;
                leapTarget = target.Center + new Vector2(pranceDirection * 200f, 0);
            }

            // Start prancing side-to-side
            if (distance < 220f && Main.rand.NextBool(70) && NPC.velocity.Y == 0)
            {
                CurrentState = AIState.Prancing;
                StateTimer = 0;
                pranceAngle = 0f;
            }

            // Only jump when target is above (small hop)
            bool targetAbove = target.Center.Y < NPC.Center.Y - 60f;

            if (NPC.velocity.Y == 0 && JumpCooldown <= 0 && targetAbove)
            {
                float jumpPower = -9f; // Small hop
                NPC.velocity.Y = jumpPower;
                JumpCooldown = 55;
                CurrentState = AIState.Jumping;
                StateTimer = 0;

                // Jump particles - snow and ice
                for (int i = 0; i < 12; i++)
                {
                    int dustType = Main.rand.NextBool() ? DustID.Snow : DustID.IceTorch;
                    Dust jumpDust = Dust.NewDustDirect(NPC.BottomLeft, NPC.width, 4, dustType, 0f, 0f, 100, default, 1.5f);
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

        private void HandleLeapPastState(Player target, float distance)
        {
            StateTimer++;

            if (StateTimer < 12)
            {
                // Crouch before leap
                NPC.velocity.X *= 0.8f;
                
                // Gathering energy particles
                for (int i = 0; i < 2; i++)
                {
                    int dustType = Main.rand.NextBool() ? DustID.IceTorch : DustID.Snow;
                    Dust charge = Dust.NewDustDirect(NPC.Center + new Vector2(pranceDirection * 25f, 10f), 1, 1, dustType, 0f, 0f, 100, default, 1.2f);
                    charge.noGravity = true;
                    charge.velocity = new Vector2(-pranceDirection, -1f);
                }
            }
            else if (StateTimer == 12)
            {
                // Graceful LEAP! High arc past the player
                NPC.velocity.X = pranceDirection * 16f;
                NPC.velocity.Y = -14f; // High graceful arc
                
                // Burst of snow and sparkles
                for (int i = 0; i < 20; i++)
                {
                    int dustType = Main.rand.NextBool(3) ? DustID.SparksMech : (Main.rand.NextBool() ? DustID.Snow : DustID.IceTorch);
                    Dust burst = Dust.NewDustDirect(NPC.Center, 1, 1, dustType, 0f, 0f, dustType == DustID.SparksMech ? 0 : 100, dustType == DustID.SparksMech ? Color.White : default, 1.5f);
                    burst.noGravity = true;
                    burst.velocity = new Vector2(-pranceDirection * Main.rand.NextFloat(1f, 4f), Main.rand.NextFloat(-3f, 1f));
                }
                
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item24 with { Pitch = 0.3f }, NPC.Center);
            }
            else if (StateTimer > 12 && NPC.velocity.Y != 0)
            {
                // In the air - graceful arc
                NPC.velocity.X *= 0.99f;
                
                // Trail sparkles
                if (StateTimer % 3 == 0)
                {
                    Dust trail = Dust.NewDustDirect(NPC.Center, 1, 1, DustID.IceTorch, 0f, 0f, 100, default, 1f);
                    trail.noGravity = true;
                    trail.velocity = -NPC.velocity * 0.05f;
                }
            }
            else if (NPC.velocity.Y == 0 && StateTimer > 15)
            {
                // Landed - choose next graceful action
                float newDist = Vector2.Distance(NPC.Center, target.Center);
                
                if (Main.rand.NextBool())
                {
                    CurrentState = AIState.Prancing;
                    pranceAngle = 0f;
                }
                else if (newDist > 250f)
                {
                    CurrentState = AIState.GracefulRetreat;
                }
                else
                {
                    CurrentState = AIState.Approaching;
                }
                StateTimer = 0;
            }
        }

        private void HandlePrancingState(Player target, float distance, bool canSee)
        {
            StateTimer++;
            pranceAngle += 0.06f;
            
            // Elegant side-to-side prancing around the player
            float pranceRadius = 160f;
            float pranceSpeed = 0.05f;
            
            // Move in a figure-8 or weaving pattern
            float waveX = (float)Math.Sin(pranceAngle) * pranceRadius * 0.5f;
            float targetX = target.Center.X + waveX;
            
            float dirX = targetX > NPC.Center.X ? 1f : -1f;
            NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, dirX * 6f, 0.08f);

            // Occasional small hops during prancing
            if (NPC.velocity.Y == 0 && Main.rand.NextBool(20))
            {
                NPC.velocity.Y = -4f;
            }

            // Attack while prancing
            if (canSee && AttackCooldown <= 0 && Main.rand.NextBool(25))
            {
                CurrentState = AIState.Attacking;
                StateTimer = 0;
                return;
            }

            // Exit prancing after a while
            if (StateTimer > 100 || Main.rand.NextBool(100))
            {
                int choice = Main.rand.Next(3);
                if (choice == 0)
                {
                    CurrentState = AIState.LeapPast;
                    pranceDirection = target.Center.X > NPC.Center.X ? 1 : -1;
                }
                else if (choice == 1)
                {
                    CurrentState = AIState.GracefulRetreat;
                }
                else
                {
                    CurrentState = AIState.Approaching;
                }
                StateTimer = 0;
            }
        }

        private void HandleGracefulRetreatState(Player target, float distance)
        {
            StateTimer++;
            graceOffset += 0.1f;

            // Elegant backward prancing
            float dirAway = NPC.Center.X > target.Center.X ? 1f : -1f;
            float retreatSpeed = 6f + (float)Math.Sin(graceOffset * 2f) * 1.5f;
            
            NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, dirAway * retreatSpeed, 0.1f);

            // Small backward hops
            if (NPC.velocity.Y == 0 && Main.rand.NextBool(15))
            {
                NPC.velocity.Y = -5f;
                
                // Snow puff
                for (int i = 0; i < 3; i++)
                {
                    Dust snow = Dust.NewDustDirect(NPC.BottomLeft, NPC.width, 4, DustID.Snow, 0f, 0f, 100, default, 0.8f);
                    snow.velocity = new Vector2(Main.rand.NextFloat(-1f, 1f), -0.5f);
                }
            }

            // Return to approaching
            if (StateTimer > 50 || distance > 350f)
            {
                CurrentState = AIState.Approaching;
                StateTimer = 0;
            }
        }

        private void HandleJumpingState(Player target)
        {
            StateTimer++;
            graceOffset += 0.12f;

            // Graceful air control with flowing motion
            float airControl = 0.28f;
            float graceInfluence = (float)Math.Sin(graceOffset) * 0.6f;
            
            if (target.Center.X > NPC.Center.X)
                NPC.velocity.X += airControl + graceInfluence * 0.1f;
            else
                NPC.velocity.X -= airControl + graceInfluence * 0.1f;

            // Clamp horizontal speed
            NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -8.8f, 8.8f);

            // Return to approaching when landed
            if (NPC.velocity.Y == 0 && StateTimer > 10)
            {
                CurrentState = AIState.Approaching;
                StateTimer = 0;
            }
        }

        private void HandleAttackingState(Player target)
        {
            StateTimer++;
            graceOffset += 0.06f;

            // Slow down during attack but maintain graceful sway
            NPC.velocity.X *= 0.92f;
            NPC.velocity.X += (float)Math.Sin(graceOffset * 2f) * 0.25f;

            // Attack telegraph - purple and light blue
            if (StateTimer < 25)
            {
                // Charging particles - purple and light blue
                if (StateTimer % 4 == 0)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        Vector2 offset = Main.rand.NextVector2Circular(45f, 45f);
                        int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.IceTorch;
                        Dust charge = Dust.NewDustDirect(NPC.Center + offset, 1, 1, dustType, 0f, 0f, 0, default, 1.3f);
                        charge.noGravity = true;
                        charge.velocity = (NPC.Center - charge.position) * 0.12f;
                    }
                }
            }
            else if (StateTimer == 25)
            {
                // Fire Snow of the Moon
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Vector2 direction = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY);

                    // Choose attack pattern
                    int attackType = Main.rand.Next(3);
                    
                    if (attackType == 0)
                    {
                        // Single accurate shot
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, direction * 13f,
                            ModContent.ProjectileType<SnowOfTheMoonProjectile>(), 48, 2f, Main.myPlayer);
                    }
                    else if (attackType == 1)
                    {
                        // Triple spread shot
                        for (int i = -1; i <= 1; i++)
                        {
                            Vector2 spreadDir = direction.RotatedBy(MathHelper.ToRadians(12f * i));
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, spreadDir * 11f,
                                ModContent.ProjectileType<SnowOfTheMoonProjectile>(), 42, 1.5f, Main.myPlayer);
                        }
                    }
                    else
                    {
                        // Burst of 5
                        for (int i = -2; i <= 2; i++)
                        {
                            Vector2 spreadDir = direction.RotatedBy(MathHelper.ToRadians(10f * i));
                            float speed = 10f + Math.Abs(i) * 0.5f;
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, spreadDir * speed,
                                ModContent.ProjectileType<SnowOfTheMoonProjectile>(), 40, 1f, Main.myPlayer);
                        }
                    }

                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Item28, NPC.Center); // Ice sound
                }
            }
            else if (StateTimer >= 55)
            {
                // Return to fluid graceful movement
                int choice = Main.rand.Next(4);
                if (choice == 0)
                    CurrentState = AIState.LeapPast;
                else if (choice == 1)
                    CurrentState = AIState.Prancing;
                else if (choice == 2)
                    CurrentState = AIState.GracefulRetreat;
                else
                    CurrentState = AIState.Approaching;
                    
                pranceDirection = target.Center.X > NPC.Center.X ? 1 : -1;
                pranceAngle = 0f;
                StateTimer = 0;
                AttackCooldown = 80;
            }
        }

        private void ManageOrbitingChandeliers()
        {
            // Spawn or maintain 2-3 orbiting chandeliers
            int chandelierCount = 3; // 3 chandeliers
            for (int i = 0; i < chandelierCount; i++)
            {
                bool chandelierValid = orbitingChandeliers[i] >= 0 && 
                                       orbitingChandeliers[i] < Main.maxProjectiles &&
                                       Main.projectile[orbitingChandeliers[i]].active &&
                                       Main.projectile[orbitingChandeliers[i]].type == ModContent.ProjectileType<WaningDeerChandelierOrbiting>() &&
                                       (int)Main.projectile[orbitingChandeliers[i]].ai[0] == NPC.whoAmI;

                if (!chandelierValid && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    // Spawn a new orbiting chandelier - evenly spaced
                    float angleOffset = i * (MathHelper.TwoPi / chandelierCount);
                    int proj = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero,
                        ModContent.ProjectileType<WaningDeerChandelierOrbiting>(), 0, 0f, Main.myPlayer, NPC.whoAmI, angleOffset);
                    orbitingChandeliers[i] = proj;
                }
            }
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            // Only spawn in snow biome surface after Moon Lord is defeated, any time of day
            if (NPC.downedMoonlord && 
                spawnInfo.Player.ZoneSnow &&
                spawnInfo.Player.ZoneOverworldHeight &&
                !spawnInfo.PlayerSafe)
            {
                return 0.25f; // 25% spawn rate
            }
            return 0f;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            // WaningDeer drops Resonant Cores and Shards
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ResonanceEnergies.ResonantCoreOfMoonlightSonata>(), 2, 1, 2));
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ShardsOfMoonlitTempo>(), 1, 2, 4));
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            // Hurt particles - purple and light blue
            for (int i = 0; i < 10; i++)
            {
                int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.IceTorch;
                Dust hurt = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, dustType, 0f, 0f, 100, default, 1.5f);
                hurt.noGravity = true;
                hurt.velocity = Main.rand.NextVector2Circular(5f, 5f);
            }

            // Death effect
            if (NPC.life <= 0)
            {
                for (int i = 0; i < 30; i++)
                {
                    int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.IceTorch;
                    Dust death = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, dustType, 0f, 0f, 100, default, 2f);
                    death.noGravity = true;
                    death.velocity = Main.rand.NextVector2Circular(10f, 10f);
                }

                // Snow particles
                for (int i = 0; i < 20; i++)
                {
                    Dust snow = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Snow, 0f, 0f, 100, default, 1.5f);
                    snow.noGravity = false;
                    snow.velocity = Main.rand.NextVector2Circular(8f, 8f);
                }
            }
        }
        
        public override void FindFrame(int frameHeight)
        {
            // Check if moving
            float movementThreshold = 0.5f;
            isActuallyMoving = Math.Abs(NPC.velocity.X) > movementThreshold || Math.Abs(NPC.velocity.Y) > movementThreshold || CurrentState == AIState.LeapPast;

            // Only update sprite direction when moving
            if (isActuallyMoving)
            {
                if (NPC.velocity.X > 0.5f)
                    lastSpriteDirection = 1;
                else if (NPC.velocity.X < -0.5f)
                    lastSpriteDirection = -1;
            }
            NPC.spriteDirection = lastSpriteDirection;

            // Animation update - only animate when moving
            if (isActuallyMoving)
            {
                frameCounter++;
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
            
            // Set frame
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

            // Pulsing glow
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.2f + 0.8f;
            Color glowColor = new Color(220, 230, 255) * pulse * 0.5f;

            // Draw glow layers
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(4f, 0f).RotatedBy(MathHelper.TwoPi * i / 4);
                spriteBatch.Draw(texture, drawPos + offset, sourceRect, glowColor, NPC.rotation,
                    origin, drawScale, NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
            }

            // Draw main sprite
            spriteBatch.Draw(texture, drawPos, sourceRect, drawColor, NPC.rotation,
                origin, drawScale, NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
            
            return false;
        }
    }
}
