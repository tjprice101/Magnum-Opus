using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Eroica.ResonanceEnergies;
using MagnumOpus.Content.Eroica.Projectiles;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.Eroica.Bosses
{
    /// <summary>
    /// Eroica's Retribution - The main boss of the Eroica theme.
    /// Phase 1: Spawns with three Movement minions orbiting.
    /// Phase 2: After all minions die, gains pink hue and new attacks.
    /// </summary>
    public class EroicasRetribution : ModNPC
    {
        // AI States
        private enum ActionState
        {
            // Phase 1 - Minions alive
            Hover,
            CirclePlayer,
            
            // Phase 2 - Minions dead
            Phase2Transition,
            Phase2Hover,
            ChargeAttack,
            EnergyChase,
            BeamAttack
        }

        private ActionState State
        {
            get => (ActionState)NPC.ai[0];
            set => NPC.ai[0] = (float)value;
        }

        private float Timer
        {
            get => NPC.ai[1];
            set => NPC.ai[1] = value;
        }

        private float AttackCounter
        {
            get => NPC.ai[2];
            set => NPC.ai[2] = value;
        }

        private float BeamCountdown
        {
            get => NPC.ai[3];
            set => NPC.ai[3] = value;
        }

        private bool phase2Started = false;
        private bool minionsSpawned = false;
        private int chargeCount = 0;
        private const int MaxCharges = 3; // Reduced from 5 for better balance
        private Vector2 chargeDirection = Vector2.Zero; // Store charge direction for telegraph
        
        // Cherry blossom spawning
        private float blossomTimer = 0f;
        
        // Phase 2 speed multiplier - triple speed!
        private float Phase2SpeedMultiplier => phase2Started ? 3f : 1f;
        
        // Track if phase 2 shake has been triggered
        private bool phase2ShakeTriggered = false;
        
        // Death animation state
        private bool isDying = false;
        private int deathTimer = 0;
        private const int DeathAnimationDuration = 180; // 3 seconds at 60fps
        private float screenFlashIntensity = 0f;
        
        // Animation - 6 rows x 6 columns sprite sheet
        private int frameCounter = 0;
        private int currentFrame = 0;
        private const int FrameTime = 2; // Ticks per frame (fast animation)
        private const int FrameColumns = 6; // 6 frames per row
        private const int FrameRows = 6; // 6 rows
        private const int TotalFrames = 36; // 6 x 6 = 36 frames

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = TotalFrames;
            
            NPCID.Sets.MPAllowedEnemies[Type] = true;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
            NPCID.Sets.TrailCacheLength[Type] = 8;
            NPCID.Sets.TrailingMode[Type] = 1;
            
            // Boss music
            NPCID.Sets.MustAlwaysDraw[Type] = true;
            
            // Debuff immunities
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Frostburn] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Ichor] = true;
        }

        public override void SetDefaults()
        {
            NPC.width = 100;
            NPC.height = 100;
            NPC.damage = 90;
            NPC.defense = 80; // Increased armor
            NPC.lifeMax = 406306; // Endgame challenge (reduced 15% from original)
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath14;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.value = Item.buyPrice(gold: 20);
            NPC.boss = true;
            NPC.npcSlots = 15f;
            NPC.aiStyle = -1;
            NPC.dontTakeDamage = true; // Invulnerable until minions die
            
            // Assign boss music
            if (!Main.dedServ)
            {
                Music = MusicLoader.GetMusicSlot(Mod, "Assets/Music/CrownOfEroica");
            }
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.NightTime,
                new FlavorTextBestiaryInfoElement("A triumphant yet tragic entity born from Beethoven's Third Symphony. " +
                    "Its three movements - allegro con brio, marcia funebre, and scherzo - manifest as loyal guardians.")
            });
        }

        public override void AI()
        {
            // Handle death animation
            if (isDying)
            {
                UpdateDeathAnimation();
                
                // Jitter effect instead of spinning
                NPC.velocity = Main.rand.NextVector2Circular(2f, 2f);
                NPC.position += Main.rand.NextVector2Circular(1f, 1f);
                
                // When animation is complete, actually kill the NPC
                if (deathTimer >= DeathAnimationDuration)
                {
                    NPC.life = 0;
                    NPC.HitEffect();
                    NPC.checkDead(); // This will now return true and kill the NPC
                }
                return;
            }
            
            NPC.TargetClosest(true);
            Player target = Main.player[NPC.target];

            // Despawn check
            if (!target.active || target.dead)
            {
                NPC.velocity.Y -= 0.5f;
                NPC.EncourageDespawn(60);
                return;
            }

            // Spawn minions on first frame
            if (!minionsSpawned && Main.netMode != NetmodeID.MultiplayerClient)
            {
                SpawnMinions();
                minionsSpawned = true;
            }

            // Cherry blossom particles across the screen
            SpawnCherryBlossoms();
            
            // Update animation
            UpdateAnimation();

            // Check if all minions are dead for phase 2 transition
            if (!phase2Started)
            {
                int aliveMinions = CountAliveMinions();
                if (aliveMinions == 0 && minionsSpawned)
                {
                    // Transition to Phase 2 - boss becomes vulnerable!
                    State = ActionState.Phase2Transition;
                    Timer = 0;
                    phase2Started = true;
                    NPC.dontTakeDamage = false; // Now damageable
                    NPC.netUpdate = true;
                }
            }

            // Boss lighting (pink in phase 2)
            if (phase2Started)
            {
                Lighting.AddLight(NPC.Center, 1f, 0.4f, 0.6f);
            }
            else
            {
                Lighting.AddLight(NPC.Center, 0.9f, 0.6f, 0.3f); // Golden in phase 1
            }

            // Spawn ambient dust
            SpawnBossDust();

            Timer++;

            // State machine
            switch (State)
            {
                case ActionState.Hover:
                    Phase1Hover(target);
                    break;
                case ActionState.CirclePlayer:
                    Phase1Circle(target);
                    break;
                case ActionState.Phase2Transition:
                    Phase2TransitionBehavior(target);
                    break;
                case ActionState.Phase2Hover:
                    Phase2Hover(target);
                    break;
                case ActionState.ChargeAttack:
                    Phase2Charge(target);
                    break;
                case ActionState.EnergyChase:
                    Phase2EnergyChase(target);
                    break;
                case ActionState.BeamAttack:
                    Phase2Beam(target);
                    break;
            }

            // Face the player
            NPC.spriteDirection = NPC.direction = (target.Center.X > NPC.Center.X) ? 1 : -1;
        }

        private void SpawnMinions()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;

            // Spawn all three movements
            NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, 
                ModContent.NPCType<MovementI>());
            NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, 
                ModContent.NPCType<MovementII>());
            NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, 
                ModContent.NPCType<MovementIII>());

            Main.NewText("The three movements begin their symphony...", 255, 180, 200);
        }

        private int CountAliveMinions()
        {
            int count = 0;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].active && 
                    (Main.npc[i].type == ModContent.NPCType<MovementI>() ||
                     Main.npc[i].type == ModContent.NPCType<MovementII>() ||
                     Main.npc[i].type == ModContent.NPCType<MovementIII>()))
                {
                    count++;
                }
            }
            return count;
        }

        private void SpawnCherryBlossoms()
        {
            blossomTimer++;
            
            // In phase 2, spawn WAY more particles (every frame, multiple spawns)
            int spawnInterval = phase2Started ? 1 : 3;
            int particlesPerSpawn = phase2Started ? 8 : 1;
            
            // Spawn cherry blossoms across the screen
            if (blossomTimer % spawnInterval == 0)
            {
                Player target = Main.player[NPC.target];
                
                for (int p = 0; p < particlesPerSpawn; p++)
                {
                    // Random position across the visible area
                    float spawnX = target.Center.X + Main.rand.NextFloat(-1200, 1200);
                    float spawnY = target.Center.Y - 600 + Main.rand.NextFloat(-200, 200);
                    
                    // Pink cherry blossom dust - bigger in phase 2
                    float scale = phase2Started ? Main.rand.NextFloat(1.8f, 2.5f) : 1.5f;
                    Dust blossom = Dust.NewDustDirect(new Vector2(spawnX, spawnY), 1, 1, DustID.PinkFairy, 0f, 0f, 150, default, scale);
                    blossom.noGravity = false;
                    blossom.velocity = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(1f, 3f));
                    blossom.fadeIn = 1.2f;
                    
                    // Shimmer effect - much more frequent in phase 2
                    if (Main.rand.NextBool(phase2Started ? 1 : 3))
                    {
                        Dust shimmer = Dust.NewDustDirect(new Vector2(spawnX, spawnY), 1, 1, DustID.PinkTorch, 0f, 0f, 100, default, phase2Started ? 1.2f : 0.8f);
                        shimmer.noGravity = true;
                        shimmer.velocity = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(0.5f, 2f));
                    }
                }
                
                // Extra sparkles in phase 2
                if (phase2Started)
                {
                    for (int s = 0; s < 5; s++)
                    {
                        float sparkX = target.Center.X + Main.rand.NextFloat(-1000, 1000);
                        float sparkY = target.Center.Y + Main.rand.NextFloat(-600, 400);
                        Dust sparkle = Dust.NewDustDirect(new Vector2(sparkX, sparkY), 1, 1, DustID.PinkCrystalShard, 0f, 0f, 0, default, 1.5f);
                        sparkle.noGravity = true;
                        sparkle.velocity = Main.rand.NextVector2Circular(3f, 3f);
                    }
                }
            }
        }
        
        private void UpdateAnimation()
        {
            frameCounter++;
            // Animation speed - even faster in phase 2
            int animSpeed = phase2Started ? 1 : FrameTime;
            if (frameCounter >= animSpeed)
            {
                frameCounter = 0;
                currentFrame++;
                if (currentFrame >= TotalFrames)
                    currentFrame = 0;
            }
        }

        private void SpawnBossDust()
        {
            // Spawn much more dust in phase 2
            int spawnChance = phase2Started ? 1 : 5;
            int particleCount = phase2Started ? 5 : 1;
            
            if (Main.rand.NextBool(spawnChance))
            {
                for (int i = 0; i < particleCount; i++)
                {
                    int dustType = phase2Started ? DustID.PinkTorch : DustID.GoldFlame;
                    float scale = phase2Started ? Main.rand.NextFloat(1.8f, 2.5f) : 1.5f;
                    Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, dustType, 0f, 0f, 100, default, scale);
                    dust.noGravity = true;
                    dust.velocity = phase2Started ? Main.rand.NextVector2Circular(4f, 4f) : dust.velocity * 0.5f;
                }
                
                // Extra intense glow in phase 2
                if (phase2Started && Main.rand.NextBool(2))
                {
                    Dust glow = Dust.NewDustDirect(NPC.Center, 1, 1, DustID.PinkFairy, 0f, 0f, 0, default, 2f);
                    glow.noGravity = true;
                    glow.velocity = Main.rand.NextVector2Circular(6f, 6f);
                }
            }
        }

        #region Phase 1 Behaviors
        
        // Fluid movement variables for Phase 1
        private float phase1WaveOffset = 0f;
        private float phase1SwoopTimer = 0f;
        private bool phase1IsSwooping = false;

        private void Phase1Hover(Player target)
        {
            phase1WaveOffset += 0.03f;
            
            // Fluid hovering with gentle wave motion
            float waveX = (float)Math.Sin(phase1WaveOffset * 2f) * 40f;
            float waveY = (float)Math.Sin(phase1WaveOffset * 1.5f) * 20f;
            
            Vector2 hoverPosition = target.Center - new Vector2(-waveX, 300 + waveY);
            Vector2 direction = hoverPosition - NPC.Center;
            float distance = direction.Length();

            if (distance > 30f)
            {
                direction.Normalize();
                NPC.velocity = Vector2.Lerp(NPC.velocity, direction * 10f, 0.05f);
            }
            else
            {
                NPC.velocity *= 0.95f;
            }

            // Occasional gentle swoop toward player
            if (!phase1IsSwooping && Timer > 120 && Main.rand.NextBool(90))
            {
                phase1IsSwooping = true;
                phase1SwoopTimer = 0f;
            }

            if (phase1IsSwooping)
            {
                phase1SwoopTimer++;
                if (phase1SwoopTimer < 30)
                {
                    // Swoop down toward player
                    Vector2 swoopDir = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY);
                    NPC.velocity = Vector2.Lerp(NPC.velocity, swoopDir * 15f, 0.1f);
                }
                else if (phase1SwoopTimer < 60)
                {
                    // Pull back up
                    Vector2 upDir = (target.Center - new Vector2(0, 350) - NPC.Center).SafeNormalize(-Vector2.UnitY);
                    NPC.velocity = Vector2.Lerp(NPC.velocity, upDir * 12f, 0.08f);
                }
                else
                {
                    phase1IsSwooping = false;
                }
            }

            // Transition to circle after some time
            if (Timer > 180 && !phase1IsSwooping)
            {
                Timer = 0;
                State = ActionState.CirclePlayer;
                NPC.netUpdate = true;
            }
        }

        private void Phase1Circle(Player target)
        {
            phase1WaveOffset += 0.04f;
            
            // Fluid figure-8 pattern with varying speed
            float circleRadius = 350f + (float)Math.Sin(phase1WaveOffset * 0.5f) * 50f;
            float circleSpeed = 0.015f + (float)Math.Sin(phase1WaveOffset * 0.3f) * 0.005f;
            float angle = Timer * circleSpeed;
            
            // Figure-8 pattern instead of simple circle
            float figure8X = (float)Math.Sin(angle) * circleRadius;
            float figure8Y = (float)Math.Sin(angle * 2f) * circleRadius * 0.4f;
            
            Vector2 circlePosition = target.Center + new Vector2(figure8X, figure8Y - 150f);

            Vector2 direction = circlePosition - NPC.Center;
            if (direction.Length() > 10f)
            {
                direction.Normalize();
                float speed = 12f + (float)Math.Sin(phase1WaveOffset * 2f) * 3f;
                NPC.velocity = Vector2.Lerp(NPC.velocity, direction * speed, 0.08f);
            }

            // Return to hover after full pattern
            if (Timer > 420)
            {
                Timer = 0;
                State = ActionState.Hover;
                NPC.netUpdate = true;
            }
        }

        #endregion

        #region Phase 2 Behaviors

        private void Phase2TransitionBehavior(Player target)
        {
            // Stop and display message
            NPC.velocity *= 0.9f;

            if (Timer == 1)
            {
                // MASSIVE screen shake for phase 2 start!
                if (!phase2ShakeTriggered)
                {
                    EroicaScreenShake.Phase2EnrageShake(NPC.Center);
                    phase2ShakeTriggered = true;
                }
                
                // Dramatic effect - HUGE particle burst
                for (int i = 0; i < 100; i++)
                {
                    Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.PinkTorch, 0f, 0f, 100, default, 3f);
                    dust.noGravity = true;
                    dust.velocity = Main.rand.NextVector2Circular(18f, 18f);
                }
                
                // Extra fairy sparkles
                for (int i = 0; i < 60; i++)
                {
                    Dust sparkle = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.PinkFairy, 0f, 0f, 0, default, 2.5f);
                    sparkle.noGravity = true;
                    sparkle.velocity = Main.rand.NextVector2Circular(15f, 15f);
                }

                Terraria.Audio.SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
            }

            if (Timer == 60)
            {
                Main.NewText("All Movements are transposed...", 255, 100, 180);
            }

            if (Timer == 120)
            {
                Main.NewText("The real fight modulation begins now!!", 255, 50, 150);
                
                // Massive burst for enrage!
                for (int i = 0; i < 80; i++)
                {
                    Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.PinkFairy, 0f, 0f, 0, default, 2.5f);
                    dust.noGravity = true;
                    dust.velocity = Main.rand.NextVector2Circular(14f, 14f);
                }
                
                // Another shake!
                EroicaScreenShake.LargeShake(NPC.Center);
            }

            if (Timer > 180)
            {
                Timer = 0;
                State = ActionState.Phase2Hover;
                NPC.netUpdate = true;
            }
        }

        private void Phase2Hover(Player target)
        {
            phase1WaveOffset += 0.06f; // Faster wave in phase 2
            
            // FLUID aggressive hover in phase 2 with weaving!
            float waveX = (float)Math.Sin(phase1WaveOffset * 3f) * 80f * Phase2SpeedMultiplier;
            float waveY = (float)Math.Sin(phase1WaveOffset * 2f) * 40f;
            
            Vector2 hoverPosition = target.Center - new Vector2(-waveX, 250 + waveY);
            Vector2 direction = hoverPosition - NPC.Center;
            float distance = direction.Length();

            float baseSpeed = 12f * Phase2SpeedMultiplier;
            float lerpSpeed = 0.06f * Phase2SpeedMultiplier;

            if (distance > 20f)
            {
                direction.Normalize();
                // Add organic velocity variation
                float speedVariation = 1f + (float)Math.Sin(phase1WaveOffset * 4f) * 0.2f;
                NPC.velocity = Vector2.Lerp(NPC.velocity, direction * baseSpeed * speedVariation, lerpSpeed);
            }
            else
            {
                NPC.velocity *= 0.9f;
            }

            // EXTREME rage multiplier - attacks happen MUCH faster as health gets lower
            float healthPercent = (float)NPC.life / NPC.lifeMax;
            // Goes from 1x at full HP to 4x at 0 HP (stacks with phase 2 speed!)
            float rageMultiplier = 1f + (1f - healthPercent) * 3f;
            // Much shorter base delay, heavily affected by rage
            int attackDelay = (int)(45 / rageMultiplier); // Was 120, now starts at 45 and goes down to ~11 at low HP

            // Choose next attack
            if (Timer > attackDelay)
            {
                Timer = 0;
                chargeCount = 0;
                
                // Attack selection - HEAVILY weighted toward beam at low HP!
                int beamWeight = (int)(30 + (1f - healthPercent) * 40); // 30-70% chance for beam based on HP
                int chargeWeight = 35;
                int energyWeight = 35;
                
                int totalWeight = beamWeight + chargeWeight + energyWeight;
                int attackChoice = Main.rand.Next(totalWeight);
                
                if (attackChoice < beamWeight)
                {
                    State = ActionState.BeamAttack;
                    BeamCountdown = 0;
                }
                else if (attackChoice < beamWeight + chargeWeight)
                {
                    State = ActionState.ChargeAttack;
                }
                else
                {
                    State = ActionState.EnergyChase;
                }
                
                NPC.netUpdate = true;
            }
        }

        private void Phase2Charge(Player target)
        {
            // AGGRESSIVE TIMING - hard to dodge!
            const int WindupTime = 20;      // Quick windup - react fast!
            const int ChargeTime = 12;      // Short explosive dash
            const int RecoveryTime = 15;    // Minimal recovery
            
            // Phase timing
            int chargeStart = WindupTime;
            int chargeEnd = WindupTime + ChargeTime;
            int totalTime = chargeEnd + RecoveryTime;
            
            // === WINDUP PHASE (0-45 frames) ===
            if (Timer < WindupTime)
            {
                // Slow down and prepare
                NPC.velocity *= 0.85f;
                
                // Lock in charge direction at start of windup (aim at current position, no prediction!)
                if (Timer == 1)
                {
                    chargeDirection = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY);
                    
                    // Audio cue - charging up sound
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Item15 with { Pitch = -0.3f, Volume = 0.8f }, NPC.Center);
                }
                
                // Draw telegraph line showing EXACT charge path
                if (Timer >= 3) // Brief moment before showing line
                {
                    float lineProgress = (Timer - 3f) / (WindupTime - 3f); // 0 to 1
                    float lineLength = 500f * lineProgress; // Extends to full length
                    
                    // Particle line showing charge path
                    for (float dist = 0; dist < lineLength; dist += 20f)
                    {
                        Vector2 linePos = NPC.Center + chargeDirection * dist;
                        
                        // Warning particles along the path
                        Dust warning = Dust.NewDustPerfect(linePos, DustID.PinkTorch, Vector2.Zero, 100, default, 1.5f);
                        warning.noGravity = true;
                        warning.fadeIn = 0.5f;
                        
                        // Brighter particles at the end of the line
                        if (dist > lineLength - 50f)
                        {
                            Dust endDust = Dust.NewDustPerfect(linePos, DustID.PinkFairy, Main.rand.NextVector2Circular(1f, 1f), 0, default, 2f);
                            endDust.noGravity = true;
                        }
                    }
                }
                
                // Boss gathering energy particles
                if (Timer % 3 == 0)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        Vector2 dustOffset = Main.rand.NextVector2Circular(100f, 100f);
                        Dust dust = Dust.NewDustPerfect(NPC.Center + dustOffset, DustID.PinkTorch, -dustOffset * 0.05f, 100, default, 2f);
                        dust.noGravity = true;
                    }
                }
                
                // Pulsing glow effect
                float pulse = (float)Math.Sin(Timer * 0.3f) * 0.3f + 0.7f;
                Lighting.AddLight(NPC.Center, 1f * pulse, 0.3f * pulse, 0.5f * pulse);
            }
            // === LAUNCH FRAME ===
            else if (Timer == WindupTime)
            {
                // Launch charge - EXTREMELY fast!
                // Speed scales with boss health - faster as health drops!
                float healthPercent = (float)NPC.life / NPC.lifeMax;
                float baseChargeSpeed = 50f; // Very fast base speed
                float maxBonusSpeed = 40f; // Up to +40 speed at low health
                float chargeSpeed = baseChargeSpeed + (1f - healthPercent) * maxBonusSpeed; // 50 at full HP -> 90 at 0% HP!
                NPC.velocity = chargeDirection * chargeSpeed;
                
                // Launch sound - distinct and loud
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f }, NPC.Center);
                EroicaScreenShake.MediumShake(NPC.Center);
                
                // Burst particles at launch
                for (int i = 0; i < 25; i++)
                {
                    Dust dust = Dust.NewDustDirect(NPC.Center, 1, 1, DustID.PinkTorch, 0f, 0f, 100, default, 2.5f);
                    dust.noGravity = true;
                    dust.velocity = Main.rand.NextVector2Circular(10f, 10f);
                }
            }
            // === CHARGING PHASE (dash in progress) ===
            else if (Timer > WindupTime && Timer < chargeEnd)
            {
                // Committed to direction - NO homing, NO tracking
                // Player can dodge by moving perpendicular
                
                // Afterimage dust trail
                if (Timer % 2 == 0)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 offset = Main.rand.NextVector2Circular(NPC.width / 3f, NPC.height / 3f);
                        Dust trail = Dust.NewDustPerfect(NPC.Center + offset, DustID.PinkTorch, -NPC.velocity * 0.1f, 100, default, 2f);
                        trail.noGravity = true;
                    }
                }
                
                // Intense lighting during dash
                Lighting.AddLight(NPC.Center, 1.2f, 0.5f, 0.7f);
            }
            // === RECOVERY PHASE ===
            else if (Timer >= chargeEnd)
            {
                // Decelerate smoothly
                NPC.velocity *= 0.9f;
                
                // Recovery complete - next charge or return to hover
                if (Timer >= totalTime)
                {
                    chargeCount++;
                    Timer = 0;
                    chargeDirection = Vector2.Zero;
                    
                    if (chargeCount >= MaxCharges)
                    {
                        // Return to hover after 3 charges
                        State = ActionState.Phase2Hover;
                        chargeCount = 0;
                        
                        // Brief rest sound
                        Terraria.Audio.SoundEngine.PlaySound(SoundID.Item4 with { Volume = 0.5f }, NPC.Center);
                    }
                    NPC.netUpdate = true;
                }
            }
        }

        private void Phase2EnergyChase(Player target)
        {
            // Hover while spawning energy orbs - TRIPLE SPEED!
            Vector2 hoverPosition = target.Center - new Vector2(0, 300);
            Vector2 direction = hoverPosition - NPC.Center;
            
            float moveSpeed = 8f * Phase2SpeedMultiplier;
            float lerpSpeed = 0.04f * Phase2SpeedMultiplier;
            
            if (direction.Length() > 30f)
            {
                direction.Normalize();
                NPC.velocity = Vector2.Lerp(NPC.velocity, direction * moveSpeed, lerpSpeed);
            }
            else
            {
                NPC.velocity *= 0.9f;
            }

            // Health-based rage
            float healthPercent = (float)NPC.life / NPC.lifeMax;
            float energyRage = 1f + (1f - healthPercent) * 2f;
            int spawnTime = (int)(60 / (Phase2SpeedMultiplier * energyRage * 0.5f));

            // Spawn energy orbs - more orbs at low HP!
            if (Timer == spawnTime && Main.netMode != NetmodeID.MultiplayerClient)
            {
                Main.NewText("Feel the energy of heroism!", 255, 150, 200);
                
                // 5-9 orbs based on HP - LOTS of orbs!
                int orbCount = 5 + (int)((1f - healthPercent) * 4);
                float orbSpeed = 10f * (1f + (1f - healthPercent)); // Much faster orbs!
                
                for (int i = 0; i < orbCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / orbCount;
                    Vector2 spawnOffset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 50f;
                    Vector2 velocity = (target.Center - (NPC.Center + spawnOffset)).SafeNormalize(Vector2.UnitY) * orbSpeed;
                    
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + spawnOffset, velocity,
                        ModContent.ProjectileType<EnergyOfEroica>(), 85, 2f, Main.myPlayer, target.whoAmI);
                }
                
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item117, NPC.Center);
                EroicaScreenShake.MediumShake(NPC.Center);
            }

            // Return to hover faster
            int returnTime = (int)(180 / (Phase2SpeedMultiplier * energyRage * 0.5f));
            if (Timer > returnTime)
            {
                Timer = 0;
                State = ActionState.Phase2Hover;
                NPC.netUpdate = true;
            }
        }

        private void Phase2Beam(Player target)
        {
            // EXTREME rage multiplier for MUCH faster countdown at low HP
            float healthPercent = (float)NPC.life / NPC.lifeMax;
            float beamRage = 1f + (1f - healthPercent) * 2.5f; // Up to 3.5x faster at low HP!
            float totalSpeedMult = Phase2SpeedMultiplier * beamRage * 0.5f; // Combined multiplier
            
            int telegraphStart = (int)(30 / totalSpeedMult);
            int telegraphEnd = (int)(180 / totalSpeedMult);
            
            // Calculate when to stop moving (12 frames / 0.2 seconds before firing)
            int countdownSpeed = (int)(30 / totalSpeedMult);
            int countdownStart = (int)(60 / totalSpeedMult);
            int extraDelay = (int)(12 / totalSpeedMult); // Changed from 30 to 12 for 0.2 sec pause
            int stopMovingTime = countdownStart + countdownSpeed * 3; // Stop 0.2 sec before beam
            
            // Only chase player until we're about to fire
            if (Timer < stopMovingTime)
            {
                // Lock above player and countdown - TRIPLE SPEED positioning!
                Vector2 lockPosition = new Vector2(target.Center.X, target.Center.Y - 500);
                Vector2 direction = lockPosition - NPC.Center;
                
                float positionSpeed = 15f * Phase2SpeedMultiplier;
                float lerpSpeed = 0.1f * Phase2SpeedMultiplier;
                
                if (direction.Length() > 20f)
                {
                    direction.Normalize();
                    NPC.velocity = Vector2.Lerp(NPC.velocity, direction * positionSpeed, lerpSpeed);
                }
                else
                {
                    NPC.velocity *= 0.8f;
                }
            }
            else
            {
                // STOP MOVING - locked in position for beam
                NPC.velocity *= 0.85f;
            }

            // Draw faint telegraph line showing where beam will fire - MORE INTENSE
            if (Timer >= telegraphStart && Timer < telegraphEnd)
            {
                // Spawn dust particles in a vertical line below the boss
                float progress = (Timer - telegraphStart) / (float)(telegraphEnd - telegraphStart);
                int dustCount = (int)(8 * progress) + 2; // More dust
                
                for (int i = 0; i < dustCount; i++)
                {
                    float yOffset = Main.rand.NextFloat(0, 1500);
                    Vector2 dustPos = NPC.Center + new Vector2(Main.rand.NextFloat(-12, 12), yOffset);
                    
                    Dust telegraph = Dust.NewDustDirect(dustPos, 1, 1, DustID.PinkTorch, 0f, 0f, 150, default, 0.8f * progress + 0.3f);
                    telegraph.noGravity = true;
                    telegraph.velocity = Vector2.Zero;
                    telegraph.fadeIn = 0.5f;
                }
            }

            // Audio cue timing - sparkly sounds give the warning
            // Play sparkly audio cues instead of text countdown
            if (Timer == countdownStart)
            {
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item4, NPC.Center);
            }
            else if (Timer == countdownStart + countdownSpeed)
            {
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item4, NPC.Center);
            }
            else if (Timer == countdownStart + countdownSpeed * 2)
            {
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item4, NPC.Center);
            }
            else if (Timer == countdownStart + countdownSpeed * 3)
            {
                // Louder warning sound
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item105, NPC.Center);
            }
            else if (Timer == countdownStart + countdownSpeed * 3 + extraDelay)
            {
                // Final warning with intense sparkle
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item105, NPC.Center);
                
                // Visual warning - beam telegraph
                for (int i = 0; i < 30; i++)
                {
                    Dust dust = Dust.NewDustDirect(NPC.Center, 1, 1, DustID.PinkTorch, 0f, 0f, 100, default, 2.5f);
                    dust.noGravity = true;
                    dust.velocity = new Vector2(0, Main.rand.NextFloat(5f, 15f));
                }
            }
            else if (Timer == countdownStart + countdownSpeed * 4 + extraDelay && Main.netMode != NetmodeID.MultiplayerClient)
            {
                // Fire the beam!
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, 1),
                    ModContent.ProjectileType<EroicasBeam>(), 120, 0f, Main.myPlayer);
                
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item122, NPC.Center);
                EroicaScreenShake.BeamShake(NPC.Center);
                
                // MASSIVE particle burst
                for (int i = 0; i < 80; i++)
                {
                    Dust dust = Dust.NewDustDirect(NPC.Center, 1, 1, DustID.PinkTorch, 0f, 0f, 100, default, 3.5f);
                    dust.noGravity = true;
                    dust.velocity = new Vector2(Main.rand.NextFloat(-8f, 8f), Main.rand.NextFloat(15f, 30f));
                }
                
                // Extra sparkle burst
                for (int i = 0; i < 40; i++)
                {
                    Dust sparkle = Dust.NewDustDirect(NPC.Center, 1, 1, DustID.PinkFairy, 0f, 0f, 0, default, 2.5f);
                    sparkle.noGravity = true;
                    sparkle.velocity = Main.rand.NextVector2Circular(10f, 15f);
                }
            }

            // Return to hover FASTER after beam
            int returnDelay = (int)((80) / totalSpeedMult);
            if (Timer > countdownStart + countdownSpeed * 5 + extraDelay + returnDelay)
            {
                Timer = 0;
                State = ActionState.Phase2Hover;
                NPC.netUpdate = true;
            }
        }

        #endregion

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            // Drop 10-15 Eroica Resonant Energy
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<EroicasResonantEnergy>(), 1, 10, 15));
            
            // Also drop some Remnants
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<RemnantOfEroicasTriumph>(), 1, 20, 30));
        }

        public override bool CheckDead()
        {
            // If death animation is complete, actually die
            if (isDying && deathTimer >= DeathAnimationDuration)
            {
                return true; // Actually die now
            }
            
            // If not already dying, start death animation
            if (!isDying)
            {
                isDying = true;
                deathTimer = 0;
                NPC.life = 1;
                NPC.dontTakeDamage = true;
                NPC.velocity = Vector2.Zero;
                
                // Play dramatic sound
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item105, NPC.Center); // Celestial sound
            }
            
            return false; // Don't die yet, animation still playing
        }

        public override bool? CanBeHitByProjectile(Projectile projectile)
        {
            // Boss cannot be targeted by projectiles while minions are alive (phase 1)
            if (!phase2Started && minionsSpawned && CountAliveMinions() > 0)
            {
                return false;
            }
            return null; // Use default behavior otherwise
        }

        public override bool CanBeHitByNPC(NPC attacker)
        {
            // Boss cannot be hit by NPCs while minions are alive (phase 1)
            if (!phase2Started && minionsSpawned && CountAliveMinions() > 0)
            {
                return false;
            }
            return true;
        }

        public override bool? CanBeHitByItem(Player player, Item item)
        {
            // Boss cannot be hit by items while minions are alive (phase 1)
            if (!phase2Started && minionsSpawned && CountAliveMinions() > 0)
            {
                return false;
            }
            return null; // Use default behavior otherwise
        }

        private void UpdateDeathAnimation()
        {
            if (!isDying) return;
            
            deathTimer++;
            
            // Phase 1: Pink and red flares (0-120 frames / 0-2 seconds)
            if (deathTimer < 120)
            {
                float intensity = (float)deathTimer / 120f;
                
                // Spawn pink and red flares with increasing frequency
                int flareCount = (int)(1 + intensity * 5);
                for (int i = 0; i < flareCount; i++)
                {
                    // Pink flares
                    if (Main.rand.NextBool(2))
                    {
                        Vector2 velocity = Main.rand.NextVector2Circular(15f, 15f) * (0.5f + intensity);
                        Dust pink = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.PinkTorch, velocity.X, velocity.Y, 0, default, 2f + intensity * 2f);
                        pink.noGravity = true;
                        pink.fadeIn = 1.5f;
                    }
                    
                    // Red flares
                    if (Main.rand.NextBool(2))
                    {
                        Vector2 velocity = Main.rand.NextVector2Circular(12f, 12f) * (0.5f + intensity);
                        Dust red = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.RedTorch, velocity.X, velocity.Y, 0, default, 2f + intensity * 2f);
                        red.noGravity = true;
                        red.fadeIn = 1.5f;
                    }
                }
                
                // Sparkle effects
                if (Main.rand.NextBool(3))
                {
                    Dust sparkle = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.PinkFairy, 0f, -2f, 0, default, 1.5f);
                    sparkle.noGravity = true;
                }
                
                // Screen shake with increasing intensity
                if (Main.LocalPlayer.Distance(NPC.Center) < 1500f)
                {
                    // Use screenPosition offset for shake effect
                    float shakeAmount = intensity * 6f;
                    Main.LocalPlayer.velocity += Main.rand.NextVector2Circular(shakeAmount, shakeAmount) * 0.1f;
                }
                
                // Sound effects at intervals
                if (deathTimer % 20 == 0)
                {
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Item74, NPC.Center); // Energy sound
                }
            }
            // Phase 2: White flash buildup (120-150 frames)
            else if (deathTimer < 150)
            {
                float flashProgress = (deathTimer - 120f) / 30f;
                screenFlashIntensity = flashProgress;
                
                // Massive particle burst
                for (int i = 0; i < 10; i++)
                {
                    Vector2 velocity = Main.rand.NextVector2Circular(20f, 20f);
                    Dust burst = Dust.NewDustDirect(NPC.Center, 0, 0, DustID.PinkTorch, velocity.X, velocity.Y, 0, default, 3f);
                    burst.noGravity = true;
                }
                
                // Heavy screen shake
                if (Main.LocalPlayer.Distance(NPC.Center) < 2000f)
                {
                    float shakeAmount = 12f;
                    Main.LocalPlayer.velocity += Main.rand.NextVector2Circular(shakeAmount, shakeAmount) * 0.15f;
                }
            }
            // Phase 3: Peak white flash (frame 150)
            else if (deathTimer == 150)
            {
                screenFlashIntensity = 1f;
                
                // Play final death sound
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item122, NPC.Center); // Lunar explosion
                
                // Massive explosion of particles
                for (int i = 0; i < 100; i++)
                {
                    Vector2 velocity = Main.rand.NextVector2Circular(25f, 25f);
                    Dust explosion = Dust.NewDustDirect(NPC.Center, 0, 0, DustID.PinkTorch, velocity.X, velocity.Y, 0, Color.White, 4f);
                    explosion.noGravity = true;
                    explosion.fadeIn = 2f;
                }
                
                for (int i = 0; i < 50; i++)
                {
                    Vector2 velocity = Main.rand.NextVector2Circular(20f, 20f);
                    Dust whiteExplosion = Dust.NewDustDirect(NPC.Center, 0, 0, DustID.Cloud, velocity.X, velocity.Y, 200, Color.White, 3f);
                    whiteExplosion.noGravity = true;
                }
            }
            // Phase 4: Fade back to normal (150-180 frames)
            else if (deathTimer <= DeathAnimationDuration)
            {
                float fadeProgress = (deathTimer - 150f) / 30f;
                screenFlashIntensity = 1f - fadeProgress;
                
                // Lingering sparkles
                if (Main.rand.NextBool(3))
                {
                    Dust linger = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.PinkFairy, 0f, -1f, 0, default, 1f);
                    linger.noGravity = true;
                }
            }
            
            // Apply screen flash effect
            if (screenFlashIntensity > 0 && Main.LocalPlayer.Distance(NPC.Center) < 2000f)
            {
                Lighting.AddLight(NPC.Center, screenFlashIntensity * 3f, screenFlashIntensity * 3f, screenFlashIntensity * 3f);
            }
        }

        public override void OnKill()
        {
            // Death effects
            for (int i = 0; i < 60; i++)
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.PinkTorch, 0f, 0f, 100, default, 2.5f);
                dust.noGravity = true;
                dust.velocity = Main.rand.NextVector2Circular(12f, 12f);
            }

            for (int i = 0; i < 40; i++)
            {
                Dust sparkle = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.PinkFairy, 0f, 0f, 0, default, 2f);
                sparkle.noGravity = true;
                sparkle.velocity = Main.rand.NextVector2Circular(10f, 10f);
            }

            // First kill message
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Main.NewText("Eroica's heroism will be sung forever...", 255, 180, 200);
            }
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            cooldownSlot = ImmunityCooldownID.Bosses;
            return true;
        }

        [System.Obsolete]
        public override void BossLoot(ref string name, ref int potionType)
        {
            potionType = ItemID.GreaterHealingPotion;
        }

        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            scale = 1.5f;
            return null;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Npc[Type].Value;
            
            // Calculate frame dimensions for a 6 column x 6 row grid
            int frameWidth = texture.Width / FrameColumns;
            int frameHeight = texture.Height / FrameRows;
            
            // Calculate which column and row based on current frame
            // Left to right, top to bottom: frame 0-5 = row 0, frame 6-11 = row 1, etc.
            int column = currentFrame % FrameColumns;
            int row = currentFrame / FrameColumns;
            
            Rectangle sourceRect = new Rectangle(column * frameWidth, row * frameHeight, frameWidth, frameHeight);
            Vector2 drawOrigin = new Vector2(frameWidth / 2, frameHeight / 2);
            
            // Flip sprite based on movement direction
            SpriteEffects effects = NPC.velocity.X < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            // Draw trail
            for (int k = 0; k < NPC.oldPos.Length; k++)
            {
                Vector2 drawPos = NPC.oldPos[k] - screenPos + new Vector2(NPC.width / 2, NPC.height / 2);
                Color trailColor;
                
                if (phase2Started)
                {
                    trailColor = new Color(255, 100, 180, 80) * ((float)(NPC.oldPos.Length - k) / NPC.oldPos.Length);
                }
                else
                {
                    // Dark pink trail in Phase 1 as well
                    trailColor = new Color(180, 60, 100, 80) * ((float)(NPC.oldPos.Length - k) / NPC.oldPos.Length);
                }
                
                float scale = NPC.scale * (1f - k * 0.1f);
                spriteBatch.Draw(texture, drawPos, sourceRect, trailColor, NPC.rotation, drawOrigin, scale, effects, 0f);
            }
            
            // Draw main sprite
            Vector2 mainDrawPos = NPC.Center - screenPos;
            Color mainColor = NPC.GetAlpha(drawColor);
            spriteBatch.Draw(texture, mainDrawPos, sourceRect, mainColor, NPC.rotation, drawOrigin, NPC.scale, effects, 0f);

            return false; // We handled drawing ourselves
        }

        public override Color? GetAlpha(Color drawColor)
        {
            if (phase2Started)
            {
                // VERY DARK intense pink in phase 2 - gets darker as HP drops!
                float healthPercent = (float)NPC.life / NPC.lifeMax;
                int red = (int)(200 + (1f - healthPercent) * 55); // 200-255
                int green = (int)(60 - (1f - healthPercent) * 40); // 60-20 (gets darker)
                int blue = (int)(100 - (1f - healthPercent) * 50); // 100-50 (gets darker)
                return new Color(red, green, blue, 230);
            }
            return null; // Normal color in phase 1
        }
        
        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            // Draw a VERY DARK PINK screen overlay in phase 2
            if (phase2Started && !Main.dedServ)
            {
                float healthPercent = (float)NPC.life / NPC.lifeMax;
                // Overlay gets MORE intense (darker) as HP drops
                float overlayIntensity = 0.25f + (1f - healthPercent) * 0.35f; // 0.25 to 0.6 opacity
                
                // Very dark pink color - almost magenta/crimson
                Color overlayColor = new Color(80, 10, 40, (int)(overlayIntensity * 255));
                
                Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;
                Rectangle screenRect = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);
                
                spriteBatch.Draw(pixel, screenRect, overlayColor);
            }
        }
    }
}
