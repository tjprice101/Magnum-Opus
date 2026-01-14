using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using MagnumOpus.Content.MoonlightSonata.ResonantOres;
using MagnumOpus.Content.MoonlightSonata.ResonanceEnergies;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.MoonlightSonata.Enemies
{
    /// <summary>
    /// Waning Deer - A powerful mini-boss that spawns in snow biomes after Moon Lord.
    /// Features 5 devastating dark purple → light blue attacks with beams and explosions.
    /// The sole source of Shards of Moonlit Tempo.
    /// </summary>
    public class WaningDeer : ModNPC
    {
        // AI States - Mini-boss attack phases
        private enum AIState
        {
            Idle,
            Approaching,
            // Attack 1: Lunar Beam Sweep - A sweeping beam of dark purple to light blue energy
            LunarBeamSweepWindup,
            LunarBeamSweepAttack,
            // Attack 2: Frost Nova - Expanding ring of ice projectiles
            FrostNovaWindup,
            FrostNovaAttack,
            // Attack 3: Crescent Barrage - Multiple crescent moon projectiles
            CrescentBarrageWindup,
            CrescentBarrageAttack,
            // Attack 4: Abyssal Moon Orbs - Homing orbs that explode
            AbyssalOrbsWindup,
            AbyssalOrbsAttack,
            // Attack 5: Moonlight Apocalypse - Massive beam from above
            MoonlightApocalypseWindup,
            MoonlightApocalypseAttack,
            Recovering,
            Jumping
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

        // Attack tracking
        private int lastAttack = -1;
        private int attacksPerformed = 0;
        private float beamRotation = 0f;
        private float beamTargetRotation = 0f;
        private bool beamClockwise = true;
        
        // Animation variables for 6x6 sprite sheet
        private int frameCounter = 0;
        private int currentFrame = 0;
        private const int FrameTime = 4;
        private const int FrameColumns = 6;
        private const int FrameRows = 6;
        private const int TotalFrames = 36;

        // Orbiting chandelier projectiles (3 for mini-boss)
        private int[] orbitingChandeliers = new int[3] { -1, -1, -1 };

        // Movement tracking
        private int lastSpriteDirection = 1;
        private bool isActuallyMoving = false;
        
        // Visual effects
        private float pulseTimer = 0f;
        private float auraIntensity = 0f;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = TotalFrames;
            
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Frostburn] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Frozen] = true;

            NPCID.Sets.DangerDetectRange[Type] = 600;
            NPCID.Sets.TrailCacheLength[Type] = 8;
            NPCID.Sets.TrailingMode[Type] = 1;
        }

        public override void SetDefaults()
        {
            // Mini-boss stats - significantly stronger
            NPC.width = 205;
            NPC.height = 169;
            NPC.damage = 120;
            NPC.defense = 65;
            NPC.lifeMax = 85000; // Mini-boss HP
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath14;
            NPC.knockBackResist = 0.05f; // Nearly immune to knockback
            NPC.value = Item.buyPrice(gold: 15);
            NPC.aiStyle = -1;
            NPC.boss = false; // Not a full boss but mini-boss
            NPC.npcSlots = 10f; // Takes up significant spawn weight
            
            NPC.noGravity = false;
            NPC.noTileCollide = false;
            
            DrawOffsetY = -45f;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Snow,
                new FlavorTextBestiaryInfoElement("A spectral deer born from the frozen tears of the moon. " +
                    "Once a graceful creature, it has absorbed immense lunar power, " +
                    "commanding devastating beams and explosions of dark purple and icy blue energy.")
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
                    NPC.velocity.Y += 0.1f;
                    NPC.timeLeft = Math.Min(NPC.timeLeft, 60);
                    return;
                }
            }

            // Manage orbiting chandeliers
            ManageOrbitingChandeliers();

            // Update timers
            pulseTimer += 0.04f;
            
            // Ambient lighting - dark purple to light blue gradient
            float lightPulse = 0.6f + (float)Math.Sin(pulseTimer * 2f) * 0.2f;
            Lighting.AddLight(NPC.Center, 0.4f * lightPulse, 0.3f * lightPulse, 0.8f * lightPulse);
            
            // Enhanced aura during attacks
            bool isAttacking = CurrentState >= AIState.LunarBeamSweepWindup && CurrentState <= AIState.MoonlightApocalypseAttack;
            auraIntensity = MathHelper.Lerp(auraIntensity, isAttacking ? 1f : 0.3f, 0.05f);
            
            // Ambient particles
            SpawnAmbientParticles();

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
                case AIState.LunarBeamSweepWindup:
                case AIState.LunarBeamSweepAttack:
                    HandleLunarBeamSweep(target);
                    break;
                case AIState.FrostNovaWindup:
                case AIState.FrostNovaAttack:
                    HandleFrostNova(target);
                    break;
                case AIState.CrescentBarrageWindup:
                case AIState.CrescentBarrageAttack:
                    HandleCrescentBarrage(target);
                    break;
                case AIState.AbyssalOrbsWindup:
                case AIState.AbyssalOrbsAttack:
                    HandleAbyssalOrbs(target);
                    break;
                case AIState.MoonlightApocalypseWindup:
                case AIState.MoonlightApocalypseAttack:
                    HandleMoonlightApocalypse(target);
                    break;
                case AIState.Recovering:
                    HandleRecovering(target);
                    break;
                case AIState.Jumping:
                    HandleJumping(target);
                    break;
            }

            // Sprite direction
            if (Math.Abs(NPC.velocity.X) > 0.5f)
            {
                lastSpriteDirection = NPC.velocity.X > 0 ? 1 : -1;
            }
            NPC.spriteDirection = lastSpriteDirection;
        }

        private void SpawnAmbientParticles()
        {
            // Themed moonlight aura
            ThemedParticles.MoonlightAura(NPC.Center, 50f * auraIntensity);
            
            // Dark purple and light blue particles
            if (Main.rand.NextBool(8))
            {
                Vector2 offset = Main.rand.NextVector2Circular(40f, 40f);
                // Alternate between dark purple and light blue
                Color particleColor = Main.rand.NextBool() ? 
                    new Color(80, 40, 140) : // Dark purple
                    new Color(140, 200, 255); // Light blue
                    
                Dust dust = Dust.NewDustDirect(NPC.Center + offset, 1, 1, DustID.PurpleTorch, 0f, 0f, 100, particleColor, 1.2f);
                dust.noGravity = true;
                dust.velocity = (NPC.Center - dust.position) * 0.02f;
            }
            
            // Occasional sparkles
            if (Main.rand.NextBool(12))
            {
                CustomParticles.MoonlightFlare(NPC.Center + Main.rand.NextVector2Circular(35f, 35f), 0.4f);
            }
            
            // Enhanced particles during attacks
            if (auraIntensity > 0.5f && Main.rand.NextBool(6))
            {
                ThemedParticles.MoonlightSparkles(NPC.Center, 3, 45f);
            }
        }

        private void HandleIdleState(Player target, float distance, bool canSee)
        {
            StateTimer++;
            
            // Gentle wandering
            if (StateTimer % 60 == 0 && NPC.velocity.Y == 0)
            {
                NPC.velocity.X = Main.rand.NextFloat(-2f, 2f);
            }
            
            // Engage player
            if (canSee && distance < 800f)
            {
                CurrentState = AIState.Approaching;
                StateTimer = 0;
                
                // Alert burst
                ThemedParticles.MoonlightBloomBurst(NPC.Center, 1f);
                SoundEngine.PlaySound(SoundID.Item29 with { Pitch = -0.2f }, NPC.Center);
            }
        }

        private void HandleApproachingState(Player target, float distance, bool canSee)
        {
            StateTimer++;
            
            // Fast approach
            float moveSpeed = 8f;
            float accel = 0.4f;
            
            float dirToPlayer = target.Center.X > NPC.Center.X ? 1f : -1f;
            NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, dirToPlayer * moveSpeed, accel * 0.1f);
            
            // Jump when blocked
            if (NPC.collideX && NPC.velocity.Y == 0 && JumpCooldown <= 0)
            {
                NPC.velocity.Y = -12f;
                JumpCooldown = 40;
                CurrentState = AIState.Jumping;
                StateTimer = 0;
            }
            
            // Choose attack when in range
            if (distance < 600f && canSee && AttackCooldown <= 0)
            {
                ChooseNextAttack(target);
            }
            
            // Lose interest
            if (distance > 1500f)
            {
                CurrentState = AIState.Idle;
                StateTimer = 0;
            }
        }

        private void ChooseNextAttack(Player target)
        {
            // Cycle through attacks, avoiding repeats
            int nextAttack;
            do
            {
                nextAttack = Main.rand.Next(5);
            } while (nextAttack == lastAttack && attacksPerformed > 0);
            
            lastAttack = nextAttack;
            attacksPerformed++;
            
            switch (nextAttack)
            {
                case 0:
                    CurrentState = AIState.LunarBeamSweepWindup;
                    beamRotation = (target.Center - NPC.Center).ToRotation();
                    beamClockwise = Main.rand.NextBool();
                    beamTargetRotation = beamRotation + (beamClockwise ? MathHelper.Pi : -MathHelper.Pi);
                    break;
                case 1:
                    CurrentState = AIState.FrostNovaWindup;
                    break;
                case 2:
                    CurrentState = AIState.CrescentBarrageWindup;
                    break;
                case 3:
                    CurrentState = AIState.AbyssalOrbsWindup;
                    break;
                case 4:
                    CurrentState = AIState.MoonlightApocalypseWindup;
                    break;
            }
            StateTimer = 0;
        }

        // ===== ATTACK 1: Lunar Beam Sweep =====
        private void HandleLunarBeamSweep(Player target)
        {
            StateTimer++;
            NPC.velocity *= 0.9f;
            
            if (CurrentState == AIState.LunarBeamSweepWindup)
            {
                if (StateTimer < 45)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                        float dist = 80f - (StateTimer * 1.5f);
                        Vector2 pos = NPC.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * dist;
                        
                        Color col = Color.Lerp(new Color(80, 40, 140), new Color(140, 200, 255), Main.rand.NextFloat());
                        Dust d = Dust.NewDustDirect(pos, 1, 1, DustID.PurpleTorch, 0f, 0f, 0, col, 1.5f);
                        d.noGravity = true;
                        d.velocity = (NPC.Center - pos) * 0.1f;
                    }
                }
                else
                {
                    CurrentState = AIState.LunarBeamSweepAttack;
                    StateTimer = 0;
                    SoundEngine.PlaySound(SoundID.Item122, NPC.Center);
                }
            }
            else
            {
                int sweepDuration = 90;
                float progress = StateTimer / (float)sweepDuration;
                
                beamRotation = MathHelper.Lerp(beamRotation, beamTargetRotation, 0.04f);
                DrawLunarBeam(beamRotation, progress);
                
                if (StateTimer % 3 == 0)
                {
                    DealBeamDamage(beamRotation, 800f, 70);
                }
                
                if (StateTimer >= sweepDuration)
                {
                    CurrentState = AIState.Recovering;
                    StateTimer = 0;
                    AttackCooldown = 90;
                }
            }
        }

        private void DrawLunarBeam(float rotation, float progress)
        {
            float beamLength = 800f;
            Vector2 direction = rotation.ToRotationVector2();
            
            for (int i = 0; i < 40; i++)
            {
                float dist = i * (beamLength / 40f);
                Vector2 pos = NPC.Center + direction * dist;
                
                float colorProgress = i / 40f;
                Color beamColor = Color.Lerp(new Color(80, 40, 140), new Color(140, 200, 255), colorProgress);
                
                Dust core = Dust.NewDustDirect(pos, 1, 1, DustID.PurpleTorch, 0f, 0f, 0, beamColor, 2f);
                core.noGravity = true;
                core.velocity = Vector2.Zero;
                
                if (i % 2 == 0)
                {
                    Vector2 perpendicular = new Vector2(-direction.Y, direction.X);
                    for (int j = -1; j <= 1; j += 2)
                    {
                        Dust glow = Dust.NewDustDirect(pos + perpendicular * 8f * j, 1, 1, DustID.IceTorch, 0f, 0f, 100, default, 1.2f);
                        glow.noGravity = true;
                        glow.velocity = perpendicular * j * 0.5f;
                    }
                }
            }
            
            Vector2 endPos = NPC.Center + direction * beamLength;
            for (int i = 0; i < 3; i++)
            {
                Dust impact = Dust.NewDustDirect(endPos, 1, 1, DustID.IceTorch, 0f, 0f, 100, default, 1.8f);
                impact.noGravity = true;
                impact.velocity = Main.rand.NextVector2Circular(4f, 4f);
            }
        }

        private void DealBeamDamage(float rotation, float length, int damage)
        {
            Vector2 direction = rotation.ToRotationVector2();
            
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player p = Main.player[i];
                if (!p.active || p.dead) continue;
                
                Vector2 toPlayer = p.Center - NPC.Center;
                float dot = Vector2.Dot(toPlayer, direction);
                
                if (dot > 0 && dot < length)
                {
                    float perpDist = Math.Abs(toPlayer.X * direction.Y - toPlayer.Y * direction.X);
                    
                    if (perpDist < 30f + p.width / 2f)
                    {
                        p.Hurt(Terraria.DataStructures.PlayerDeathReason.ByNPC(NPC.whoAmI), damage, direction.X > 0 ? 1 : -1);
                    }
                }
            }
        }

        // ===== ATTACK 2: Frost Nova =====
        private void HandleFrostNova(Player target)
        {
            StateTimer++;
            NPC.velocity *= 0.85f;
            
            if (CurrentState == AIState.FrostNovaWindup)
            {
                if (StateTimer < 40)
                {
                    if (StateTimer % 3 == 0)
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            float angle = MathHelper.TwoPi * i / 8f + StateTimer * 0.1f;
                            Vector2 pos = NPC.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * (60f - StateTimer);
                            
                            Dust ice = Dust.NewDustDirect(pos, 1, 1, DustID.IceTorch, 0f, 0f, 100, default, 1.5f);
                            ice.noGravity = true;
                            ice.velocity = (NPC.Center - pos) * 0.08f;
                        }
                    }
                }
                else
                {
                    CurrentState = AIState.FrostNovaAttack;
                    StateTimer = 0;
                    SoundEngine.PlaySound(SoundID.Item28 with { Pitch = -0.3f }, NPC.Center);
                }
            }
            else
            {
                if (StateTimer == 1 || StateTimer == 20 || StateTimer == 40)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        int projectileCount = 12 + (int)(StateTimer / 10);
                        for (int i = 0; i < projectileCount; i++)
                        {
                            float angle = MathHelper.TwoPi * i / projectileCount + StateTimer * 0.05f;
                            Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 10f;
                            
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity,
                                ModContent.ProjectileType<SnowOfTheMoonProjectile>(), 55, 2f, Main.myPlayer);
                        }
                    }
                    
                    ThemedParticles.MoonlightShockwave(NPC.Center, 0.8f);
                    
                    for (int i = 0; i < 20; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 20f;
                        Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 6f;
                        
                        Dust burst = Dust.NewDustDirect(NPC.Center, 1, 1, DustID.IceTorch, vel.X, vel.Y, 100, default, 2f);
                        burst.noGravity = true;
                    }
                }
                
                if (StateTimer >= 60)
                {
                    CurrentState = AIState.Recovering;
                    StateTimer = 0;
                    AttackCooldown = 100;
                }
            }
        }

        // ===== ATTACK 3: Crescent Barrage =====
        private void HandleCrescentBarrage(Player target)
        {
            StateTimer++;
            
            float dirToTarget = target.Center.X > NPC.Center.X ? 1f : -1f;
            NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, dirToTarget * 3f, 0.05f);
            
            if (CurrentState == AIState.CrescentBarrageWindup)
            {
                if (StateTimer < 30)
                {
                    float arcAngle = StateTimer * 0.15f;
                    for (int i = -2; i <= 2; i++)
                    {
                        float offset = i * 0.3f;
                        Vector2 pos = NPC.Center + new Vector2((float)Math.Cos(arcAngle + offset) * 50f, (float)Math.Sin(arcAngle + offset) * 30f);
                        
                        Color col = Color.Lerp(new Color(80, 40, 140), new Color(140, 200, 255), (i + 2) / 4f);
                        Dust d = Dust.NewDustDirect(pos, 1, 1, DustID.PurpleTorch, 0f, 0f, 0, col, 1.3f);
                        d.noGravity = true;
                        d.velocity *= 0.1f;
                    }
                }
                else
                {
                    CurrentState = AIState.CrescentBarrageAttack;
                    StateTimer = 0;
                }
            }
            else
            {
                if (StateTimer % 8 == 0 && StateTimer <= 64)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Vector2 toTarget = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY);
                        int burstIndex = (int)(StateTimer / 8);
                        
                        if (burstIndex % 2 == 0)
                        {
                            for (int i = -2; i <= 2; i++)
                            {
                                Vector2 vel = toTarget.RotatedBy(MathHelper.ToRadians(15f * i)) * 12f;
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, vel,
                                    ModContent.ProjectileType<SnowOfTheMoonProjectile>(), 50, 2f, Main.myPlayer);
                            }
                        }
                        else
                        {
                            for (int i = -1; i <= 1; i++)
                            {
                                Vector2 vel = toTarget.RotatedBy(MathHelper.ToRadians(8f * i)) * 14f;
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, vel,
                                    ModContent.ProjectileType<SnowOfTheMoonProjectile>(), 50, 2f, Main.myPlayer);
                            }
                        }
                        
                        SoundEngine.PlaySound(SoundID.Item28, NPC.Center);
                    }
                    
                    CustomParticles.MoonlightFlare(NPC.Center, 0.6f);
                }
                
                if (StateTimer >= 80)
                {
                    CurrentState = AIState.Recovering;
                    StateTimer = 0;
                    AttackCooldown = 80;
                }
            }
        }

        // ===== ATTACK 4: Abyssal Moon Orbs =====
        private void HandleAbyssalOrbs(Player target)
        {
            StateTimer++;
            NPC.velocity *= 0.92f;
            
            if (CurrentState == AIState.AbyssalOrbsWindup)
            {
                if (StateTimer < 50)
                {
                    if (StateTimer % 4 == 0)
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            float angle = MathHelper.TwoPi * i / 6f + StateTimer * 0.08f;
                            float radius = 40f + (float)Math.Sin(StateTimer * 0.2f) * 15f;
                            Vector2 pos = NPC.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                            
                            Dust d = Dust.NewDustDirect(pos, 1, 1, DustID.Shadowflame, 0f, 0f, 100, new Color(60, 30, 100), 1.4f);
                            d.noGravity = true;
                            d.velocity = (NPC.Center - pos).SafeNormalize(Vector2.Zero) * 2f;
                        }
                    }
                }
                else
                {
                    CurrentState = AIState.AbyssalOrbsAttack;
                    StateTimer = 0;
                    SoundEngine.PlaySound(SoundID.Item103, NPC.Center);
                }
            }
            else
            {
                if (StateTimer == 1 || StateTimer == 25 || StateTimer == 50)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            float angle = MathHelper.TwoPi * i / 4f + StateTimer * 0.1f;
                            Vector2 spawnPos = NPC.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 60f;
                            Vector2 initialVel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 3f;
                            
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPos, initialVel,
                                ModContent.ProjectileType<WaningDeerHomingOrb>(), 65, 3f, Main.myPlayer, NPC.target);
                        }
                    }
                    
                    for (int i = 0; i < 15; i++)
                    {
                        Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                        Dust burst = Dust.NewDustDirect(NPC.Center, 1, 1, DustID.Shadowflame, vel.X, vel.Y, 100, new Color(80, 40, 140), 1.6f);
                        burst.noGravity = true;
                    }
                }
                
                if (StateTimer >= 75)
                {
                    CurrentState = AIState.Recovering;
                    StateTimer = 0;
                    AttackCooldown = 110;
                }
            }
        }

        // ===== ATTACK 5: Moonlight Apocalypse =====
        private void HandleMoonlightApocalypse(Player target)
        {
            StateTimer++;
            NPC.velocity *= 0.8f;
            
            if (CurrentState == AIState.MoonlightApocalypseWindup)
            {
                if (StateTimer < 75)
                {
                    float warningProgress = StateTimer / 75f;
                    
                    if (StateTimer % 3 == 0)
                    {
                        for (int i = 0; i < 16; i++)
                        {
                            float angle = MathHelper.TwoPi * i / 16f;
                            float radius = 100f * warningProgress;
                            Vector2 pos = new Vector2(target.Center.X, target.Center.Y + 100) + 
                                new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle) * 0.3f) * radius;
                            
                            Color warningColor = Color.Lerp(new Color(80, 40, 140), new Color(140, 200, 255), warningProgress);
                            Dust warning = Dust.NewDustDirect(pos, 1, 1, DustID.PurpleTorch, 0f, 0f, 0, warningColor, 1.5f);
                            warning.noGravity = true;
                            warning.velocity = Vector2.UnitY * -1f;
                        }
                    }
                    
                    if (StateTimer % 5 == 0)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            Vector2 risePos = NPC.Center + Main.rand.NextVector2Circular(30f, 30f);
                            Dust rise = Dust.NewDustDirect(risePos, 1, 1, DustID.IceTorch, 0f, -3f, 100, default, 1.8f);
                            rise.noGravity = true;
                        }
                    }
                    
                    if (StateTimer == 60)
                    {
                        SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f }, NPC.Center);
                    }
                }
                else
                {
                    CurrentState = AIState.MoonlightApocalypseAttack;
                    StateTimer = 0;
                    SoundEngine.PlaySound(SoundID.Item162, target.Center);
                }
            }
            else
            {
                Vector2 beamStart = new Vector2(target.Center.X, target.Center.Y - 1200f);
                Vector2 beamEnd = new Vector2(target.Center.X, target.Center.Y + 400f);
                
                if (StateTimer < 40)
                {
                    float beamWidth = 80f * Math.Min(1f, StateTimer / 10f);
                    
                    for (int i = 0; i < 60; i++)
                    {
                        float progress = i / 60f;
                        Vector2 pos = Vector2.Lerp(beamStart, beamEnd, progress);
                        
                        Color beamColor = Color.Lerp(new Color(140, 200, 255), new Color(80, 40, 140), progress);
                        
                        for (int j = 0; j < 3; j++)
                        {
                            Vector2 offset = Main.rand.NextVector2Circular(beamWidth * 0.3f, 1f);
                            Dust core = Dust.NewDustDirect(pos + offset, 1, 1, DustID.PurpleTorch, 0f, 0f, 0, beamColor, 2.5f);
                            core.noGravity = true;
                            core.velocity = Vector2.Zero;
                        }
                        
                        if (i % 3 == 0)
                        {
                            Vector2 sideOffset = new Vector2(Main.rand.NextFloat(-beamWidth, beamWidth), 0);
                            Dust outer = Dust.NewDustDirect(pos + sideOffset, 1, 1, DustID.IceTorch, sideOffset.X * 0.1f, 0f, 100, default, 1.5f);
                            outer.noGravity = true;
                        }
                    }
                    
                    if (StateTimer == 5)
                    {
                        for (int i = 0; i < 40; i++)
                        {
                            Vector2 vel = Main.rand.NextVector2Circular(12f, 8f);
                            vel.Y = -Math.Abs(vel.Y) - 2f;
                            
                            Color impactColor = Main.rand.NextBool() ? new Color(80, 40, 140) : new Color(140, 200, 255);
                            Dust impact = Dust.NewDustDirect(beamEnd, 1, 1, DustID.PurpleTorch, vel.X, vel.Y, 0, impactColor, 2f);
                            impact.noGravity = true;
                        }
                        
                        ThemedParticles.MoonlightShockwave(beamEnd, 1.2f);
                    }
                    
                    if (StateTimer % 5 == 0)
                    {
                        Rectangle damageArea = new Rectangle((int)(target.Center.X - 60), (int)(beamStart.Y), 120, (int)(beamEnd.Y - beamStart.Y));
                        
                        for (int i = 0; i < Main.maxPlayers; i++)
                        {
                            Player p = Main.player[i];
                            if (!p.active || p.dead) continue;
                            
                            if (p.Hitbox.Intersects(damageArea))
                            {
                                p.Hurt(Terraria.DataStructures.PlayerDeathReason.ByNPC(NPC.whoAmI), 85, 0);
                            }
                        }
                    }
                }
                
                if (StateTimer >= 50)
                {
                    CurrentState = AIState.Recovering;
                    StateTimer = 0;
                    AttackCooldown = 150;
                }
            }
        }

        private void HandleRecovering(Player target)
        {
            StateTimer++;
            NPC.velocity.X *= 0.95f;
            
            if (StateTimer >= 30)
            {
                CurrentState = AIState.Approaching;
                StateTimer = 0;
            }
        }

        private void HandleJumping(Player target)
        {
            StateTimer++;
            
            float airControl = 0.25f;
            float dirToTarget = target.Center.X > NPC.Center.X ? 1f : -1f;
            NPC.velocity.X += dirToTarget * airControl;
            NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -10f, 10f);
            
            if (StateTimer % 3 == 0)
            {
                Dust trail = Dust.NewDustDirect(NPC.Center, 1, 1, DustID.IceTorch, 0f, 0f, 100, default, 1f);
                trail.noGravity = true;
                trail.velocity = -NPC.velocity * 0.05f;
            }
            
            if (NPC.velocity.Y == 0 && StateTimer > 10)
            {
                CurrentState = AIState.Approaching;
                StateTimer = 0;
            }
        }

        private void ManageOrbitingChandeliers()
        {
            int chandelierCount = 3;
            for (int i = 0; i < chandelierCount; i++)
            {
                bool chandelierValid = orbitingChandeliers[i] >= 0 && 
                                       orbitingChandeliers[i] < Main.maxProjectiles &&
                                       Main.projectile[orbitingChandeliers[i]].active &&
                                       Main.projectile[orbitingChandeliers[i]].type == ModContent.ProjectileType<WaningDeerChandelierOrbiting>() &&
                                       (int)Main.projectile[orbitingChandeliers[i]].ai[0] == NPC.whoAmI;

                if (!chandelierValid && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    float angleOffset = i * (MathHelper.TwoPi / chandelierCount);
                    int proj = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero,
                        ModContent.ProjectileType<WaningDeerChandelierOrbiting>(), 0, 0f, Main.myPlayer, NPC.whoAmI, angleOffset);
                    orbitingChandeliers[i] = proj;
                }
            }
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            // Mini-boss spawn in snow biome after Moon Lord - rare
            if (NPC.downedMoonlord && 
                spawnInfo.Player.ZoneSnow &&
                spawnInfo.Player.ZoneOverworldHeight &&
                !spawnInfo.PlayerSafe &&
                !NPC.AnyNPCs(Type))
            {
                return 0.08f;
            }
            return 0f;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            // Primary source of Shards of Moonlit Tempo
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ShardsOfMoonlitTempo>(), 1, 8, 15));
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ResonanceEnergies.ResonantCoreOfMoonlightSonata>(), 2, 1, 3));
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int i = 0; i < 12; i++)
            {
                Color particleColor = Main.rand.NextBool() ? new Color(80, 40, 140) : new Color(140, 200, 255);
                int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.IceTorch;
                
                Dust hurt = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, dustType, 0f, 0f, 100, particleColor, 1.5f);
                hurt.noGravity = true;
                hurt.velocity = Main.rand.NextVector2Circular(6f, 6f);
            }

            if (NPC.life <= 0)
            {
                for (int i = 0; i < 50; i++)
                {
                    Color deathColor = Main.rand.NextBool() ? new Color(80, 40, 140) : new Color(140, 200, 255);
                    int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.IceTorch;
                    
                    Dust death = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, dustType, 0f, 0f, 100, deathColor, 2.5f);
                    death.noGravity = true;
                    death.velocity = Main.rand.NextVector2Circular(15f, 15f);
                }

                for (int i = 0; i < 30; i++)
                {
                    Dust snow = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Snow, 0f, 0f, 100, default, 2f);
                    snow.noGravity = false;
                    snow.velocity = Main.rand.NextVector2Circular(10f, 10f);
                }
                
                ThemedParticles.MoonlightShockwave(NPC.Center, 1.5f);
                ThemedParticles.MoonlightBloomBurst(NPC.Center, 1.5f);
            }
        }
        
        public override void FindFrame(int frameHeight)
        {
            float movementThreshold = 0.5f;
            isActuallyMoving = Math.Abs(NPC.velocity.X) > movementThreshold || Math.Abs(NPC.velocity.Y) > movementThreshold;

            if (isActuallyMoving)
            {
                if (NPC.velocity.X > 0.5f)
                    lastSpriteDirection = 1;
                else if (NPC.velocity.X < -0.5f)
                    lastSpriteDirection = -1;
            }
            NPC.spriteDirection = lastSpriteDirection;

            if (isActuallyMoving)
            {
                frameCounter++;
                int animSpeed = Math.Abs(NPC.velocity.X) > 3f ? 3 : FrameTime;
                
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
                currentFrame = 0;
                frameCounter = 0;
            }
            
            NPC.frame.Y = currentFrame * frameHeight;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Npc[Type].Value;
            
            int frameWidth = texture.Width / FrameColumns;
            int frameHeight = texture.Height / FrameRows;
            
            int frameX = currentFrame % FrameColumns;
            int frameY = currentFrame / FrameColumns;
            
            Rectangle sourceRect = new Rectangle(frameX * frameWidth, frameY * frameHeight, frameWidth, frameHeight);
            Vector2 drawPos = NPC.Center - screenPos + new Vector2(0f, DrawOffsetY);
            Vector2 origin = new Vector2(frameWidth / 2, frameHeight / 2);

            float drawScale = 1.8f;

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.3f + 0.7f;
            
            Color purpleGlow = new Color(80, 40, 140) * pulse * 0.4f * auraIntensity;
            Color blueGlow = new Color(140, 200, 255) * pulse * 0.4f * auraIntensity;

            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(6f, 0f).RotatedBy(MathHelper.TwoPi * i / 4);
                spriteBatch.Draw(texture, drawPos + offset, sourceRect, purpleGlow, NPC.rotation,
                    origin, drawScale, NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
            }
            
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(4f, 0f).RotatedBy(MathHelper.TwoPi * i / 4 + MathHelper.PiOver4);
                spriteBatch.Draw(texture, drawPos + offset, sourceRect, blueGlow, NPC.rotation,
                    origin, drawScale, NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
            }

            spriteBatch.Draw(texture, drawPos, sourceRect, drawColor, NPC.rotation,
                origin, drawScale, NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
            
            return false;
        }
    }
    
    /// <summary>
    /// Homing orb projectile for Waning Deer's Abyssal Orbs attack
    /// </summary>
    public class WaningDeerHomingOrb : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/MoonlightSonata/Enemies/SnowOfTheMoon";
        
        private int targetPlayer = -1;
        
        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.alpha = 0;
            Projectile.light = 0.5f;
        }
        
        public override void AI()
        {
            targetPlayer = (int)Projectile.ai[0];
            
            if (targetPlayer >= 0 && targetPlayer < Main.maxPlayers)
            {
                Player target = Main.player[targetPlayer];
                if (target.active && !target.dead)
                {
                    Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    float homingStrength = Projectile.timeLeft > 200 ? 0.02f : 0.04f;
                    
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * 8f, homingStrength);
                }
            }
            
            if (Projectile.velocity.Length() > 10f)
            {
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * 10f;
            }
            
            Projectile.rotation += 0.15f;
            
            if (Main.rand.NextBool(3))
            {
                Color particleColor = Color.Lerp(new Color(80, 40, 140), new Color(140, 200, 255), Main.rand.NextFloat());
                Dust trail = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
                    DustID.PurpleTorch, 0f, 0f, 100, particleColor, 1.2f);
                trail.noGravity = true;
                trail.velocity = -Projectile.velocity * 0.2f;
            }
            
            Lighting.AddLight(Projectile.Center, 0.4f, 0.2f, 0.6f);
        }
        
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
            
            for (int i = 0; i < 20; i++)
            {
                Color explosionColor = Main.rand.NextBool() ? new Color(80, 40, 140) : new Color(140, 200, 255);
                Dust explosion = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.PurpleTorch, 0f, 0f, 100, explosionColor, 1.8f);
                explosion.noGravity = true;
                explosion.velocity = Main.rand.NextVector2Circular(8f, 8f);
            }
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.2f + 0.8f;
            Color glowColor = Color.Lerp(new Color(80, 40, 140), new Color(140, 200, 255), 
                (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.5f + 0.5f) * pulse * 0.6f;
            
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(4f, 0f).RotatedBy(MathHelper.TwoPi * i / 4 + Main.GameUpdateCount * 0.1f);
                Main.EntitySpriteDraw(texture, drawPos + offset, null, glowColor, Projectile.rotation,
                    texture.Size() / 2f, Projectile.scale * 1.2f, SpriteEffects.None, 0);
            }
            
            Main.EntitySpriteDraw(texture, drawPos, null, lightColor, Projectile.rotation,
                texture.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);
            
            return false;
        }
    }
}
