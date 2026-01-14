using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.LaCampanella.ResonanceEnergies;
using MagnumOpus.Content.LaCampanella.ResonantWeapons;
using MagnumOpus.Content.LaCampanella.Accessories;
using MagnumOpus.Content.LaCampanella.HarmonicCores;

namespace MagnumOpus.Content.LaCampanella.Bosses
{
    /// <summary>
    /// La Campanella, Chime of Life - A colossal infernal bell deity boss.
    /// Features gravity-based movement with slow walking and occasional powerful jumps.
    /// ENHANCED with spectacular particle effects, lightning, beams, and cinematic combat.
    /// Attacks: Massive infernal lasers, explosive bell projectiles, fire waves, 
    ///          infernal vortex, sonic resonance, flame geysers, bell lightning storm.
    /// Aesthetic: Black and orange heat distortion, cinematic infernal atmosphere.
    /// A significant step up from Eroica in difficulty and spectacle.
    /// </summary>
    public class LaCampanellaChimeOfLife : ModNPC
    {
        public override string Texture => "MagnumOpus/Content/LaCampanella/Bosses/LaCampanellaChimeOfLife";
        
        // AI States - Expanded with new spectacular attacks
        private enum ActionState
        {
            Spawn,
            Walking,
            JumpWindup,
            Jumping,
            Landing,
            // Basic Attacks
            LaserWindup,
            LaserFiring,
            BellBarrageWindup,
            BellBarrage,
            InfernalWaveWindup,
            InfernalWave,
            MassiveLaserWindup,
            MassiveLaser,
            BellStormWindup,
            BellStorm,
            // NEW Advanced Attacks
            InfernalVortexWindup,
            InfernalVortex,
            SonicResonanceWindup,
            SonicResonance,
            FlameGeyserWindup,
            FlameGeyser,
            BellLightningWindup,
            BellLightning,
            ChimeCascadeWindup,
            ChimeCascade,
            InfernalBeamWindup,
            InfernalBeam,
            TollOfDoomWindup,
            TollOfDoom,
            // Death
            Dying
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

        private float AttackPhase
        {
            get => NPC.ai[2];
            set => NPC.ai[2] = value;
        }

        private float AttackCooldown
        {
            get => NPC.ai[3];
            set => NPC.ai[3] = value;
        }

        // Movement
        private bool isGrounded = false;
        private float walkDirection = 1f;
        private float walkSpeed = 3.5f; // Faster walking
        private int jumpCooldown = 0;
        private int attacksSinceLastJump = 0;
        
        // Visual effects - ENHANCED
        private float auraPulse = 0f;
        private float screenShakeIntensity = 0f;
        private bool hasActivatedSky = false;
        private float bellRingTimer = 0f;
        private float distortionIntensity = 0f; // NEW: Heat distortion effect
        private float distortionTimer = 0f;
        private float backgroundDarknessAlpha = 0f;
        
        // NEW: Beam attack tracking
        private float beamRotationAngle = 0f;
        private float beamLength = 0f;
        
        // NEW: Lightning storm tracking
        private Vector2[] telegraphedLightningPositions = new Vector2[10];
        private int lightningStrikeIndex = 0;
        
        // NEW: Vortex attack tracking
        private float[] vortexBeamAngles = new float[8];
        private float vortexRotationSpeed = 0f;
        
        // Animation
        private int frameCounter = 0;
        private int currentFrame = 0;
        private const int TotalFrames = 1; // Single frame sprite
        
        // Health bar registration
        private bool hasRegisteredHealthBar = false;
        
        // Death animation
        private int deathTimer = 0;
        private const int DeathAnimationDuration = 420; // 7 seconds epic death
        private float deathFlashIntensity = 0f;
        
        // Attack tracking - ENHANCED
        private int consecutiveAttacks = 0;
        private int lastAttackType = -1;
        private bool isEnraged = false; // Enrage at low health
        private int enrageTimer = 0;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = TotalFrames;
            
            NPCID.Sets.MPAllowedEnemies[Type] = true;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
            NPCID.Sets.TrailCacheLength[Type] = 8;
            NPCID.Sets.TrailingMode[Type] = 1;
            NPCID.Sets.MustAlwaysDraw[Type] = true;
            
            // Debuff immunities
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire3] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Frostburn] = true;
        }

        public override void SetDefaults()
        {
            NPC.width = 180;
            NPC.height = 240;
            NPC.damage = 130; // Tier 2 - between Eroica (90) and Swan Lake (170)
            NPC.defense = 70; // Tier 2 - between Eroica (80) and Swan Lake (110)
            NPC.lifeMax = 650000; // 650k HP - Tier 2 (Eroica ~400k, Swan Lake ~950k)
            NPC.HitSound = SoundID.NPCHit4 with { Pitch = -0.3f };
            NPC.DeathSound = SoundID.NPCDeath14;
            NPC.knockBackResist = 0f;
            NPC.noGravity = false; // Has gravity!
            NPC.noTileCollide = false; // Collides with tiles!
            NPC.value = Item.buyPrice(gold: 30);
            NPC.boss = true;
            NPC.npcSlots = 16f;
            NPC.aiStyle = -1;
            NPC.scale = 1f;
            NPC.lavaImmune = true;
            
            // Music - uses Underworld boss theme as fallback until custom music is added
            // TODO: Add custom music at Assets/Music/LaCampanellaTheme.ogg
            Music = MusicID.Boss2; // Wall of Flesh theme fits the infernal atmosphere
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheUnderworld,
                new FlavorTextBestiaryInfoElement("La Campanella, Chime of Life - " +
                    "An ancient infernal bell deity awakened from the depths. " +
                    "Its tolling brings both creation and destruction, " +
                    "wreathed in black flames and scorching orange fire.")
            });
        }

        public override void AI()
        {
            // Register with custom health bar system
            if (!hasRegisteredHealthBar)
            {
                BossHealthBarUI.RegisterBoss(NPC, BossColorTheme.LaCampanella);
                hasRegisteredHealthBar = true;
            }
            
            // Activate sky effect
            if (!hasActivatedSky && !Main.dedServ)
            {
                SkyManager.Instance.Activate("MagnumOpus:LaCampanellaSky");
                hasActivatedSky = true;
            }
            
            NPC.TargetClosest(true);
            Player target = Main.player[NPC.target];

            // Death animation
            if (State == ActionState.Dying)
            {
                UpdateDeathAnimation(target);
                return;
            }

            // Despawn check
            if (!target.active || target.dead)
            {
                NPC.velocity.Y += 0.5f;
                NPC.EncourageDespawn(60);
                DeactivateSky();
                return;
            }

            // Update timers
            Timer++;
            auraPulse += 0.05f;
            bellRingTimer += 0.03f;
            distortionTimer += 0.02f;
            
            if (jumpCooldown > 0) jumpCooldown--;
            if (AttackCooldown > 0) AttackCooldown--;
            
            // Check for enrage (below 30% health)
            float healthPercent = (float)NPC.life / NPC.lifeMax;
            if (healthPercent < 0.3f && !isEnraged)
            {
                TriggerEnrage();
            }
            
            // Update enrage effects
            if (isEnraged)
            {
                enrageTimer++;
                distortionIntensity = MathHelper.Lerp(distortionIntensity, 0.5f, 0.05f);
            }
            else
            {
                distortionIntensity = MathHelper.Lerp(distortionIntensity, 0.15f, 0.03f);
            }
            
            // Ground detection
            CheckGrounded();
            
            // Apply gravity (only when not in special states)
            if (State != ActionState.Jumping && State != ActionState.JumpWindup)
            {
                if (!isGrounded)
                {
                    NPC.velocity.Y += 0.4f; // Gravity
                    if (NPC.velocity.Y > 15f)
                        NPC.velocity.Y = 15f;
                }
            }
            
            // Ambient effects - ENHANCED
            SpawnAmbientParticles();
            UpdateScreenShake();
            
            // Lighting - orange/black infernal glow - ENHANCED
            float lightPulse = 0.7f + (float)Math.Sin(auraPulse) * 0.3f;
            float enrageBoost = isEnraged ? 1.4f : 1f;
            Lighting.AddLight(NPC.Center, 1.2f * lightPulse * enrageBoost, 0.5f * lightPulse * enrageBoost, 0.1f * lightPulse);

            // State machine
            switch (State)
            {
                case ActionState.Spawn:
                    SpawnSequence(target);
                    break;
                case ActionState.Walking:
                    WalkingBehavior(target);
                    break;
                case ActionState.JumpWindup:
                    JumpWindup(target);
                    break;
                case ActionState.Jumping:
                    JumpingBehavior(target);
                    break;
                case ActionState.Landing:
                    LandingBehavior(target);
                    break;
                    
                // Attacks
                case ActionState.LaserWindup:
                    LaserWindup(target);
                    break;
                case ActionState.LaserFiring:
                    LaserFiring(target);
                    break;
                case ActionState.BellBarrageWindup:
                    BellBarrageWindup(target);
                    break;
                case ActionState.BellBarrage:
                    BellBarrageFiring(target);
                    break;
                case ActionState.InfernalWaveWindup:
                    InfernalWaveWindup(target);
                    break;
                case ActionState.InfernalWave:
                    InfernalWaveFiring(target);
                    break;
                case ActionState.MassiveLaserWindup:
                    MassiveLaserWindup(target);
                    break;
                case ActionState.MassiveLaser:
                    MassiveLaserFiring(target);
                    break;
                case ActionState.BellStormWindup:
                    BellStormWindup(target);
                    break;
                case ActionState.BellStorm:
                    BellStormFiring(target);
                    break;
                    
                // NEW Advanced Attacks
                case ActionState.InfernalVortexWindup:
                    InfernalVortexWindup(target);
                    break;
                case ActionState.InfernalVortex:
                    InfernalVortexAttack(target);
                    break;
                case ActionState.SonicResonanceWindup:
                    SonicResonanceWindup(target);
                    break;
                case ActionState.SonicResonance:
                    SonicResonanceAttack(target);
                    break;
                case ActionState.FlameGeyserWindup:
                    FlameGeyserWindup(target);
                    break;
                case ActionState.FlameGeyser:
                    FlameGeyserAttack(target);
                    break;
                case ActionState.BellLightningWindup:
                    BellLightningWindup(target);
                    break;
                case ActionState.BellLightning:
                    BellLightningAttack(target);
                    break;
                case ActionState.ChimeCascadeWindup:
                    ChimeCascadeWindup(target);
                    break;
                case ActionState.ChimeCascade:
                    ChimeCascadeAttack(target);
                    break;
                case ActionState.InfernalBeamWindup:
                    InfernalBeamWindup(target);
                    break;
                case ActionState.InfernalBeam:
                    InfernalBeamAttack(target);
                    break;
                case ActionState.TollOfDoomWindup:
                    TollOfDoomWindup(target);
                    break;
                case ActionState.TollOfDoom:
                    TollOfDoomAttack(target);
                    break;
            }

            // Face the player
            if (State == ActionState.Walking || State == ActionState.Spawn)
            {
                NPC.spriteDirection = NPC.direction = (target.Center.X > NPC.Center.X) ? 1 : -1;
            }
        }
        
        private void CheckGrounded()
        {
            // Check if standing on solid ground
            Vector2 bottomLeft = new Vector2(NPC.position.X + 10, NPC.position.Y + NPC.height + 4);
            Vector2 bottomRight = new Vector2(NPC.position.X + NPC.width - 10, NPC.position.Y + NPC.height + 4);
            
            Point tileLeft = bottomLeft.ToTileCoordinates();
            Point tileRight = bottomRight.ToTileCoordinates();
            
            bool leftSolid = WorldGen.SolidTile(tileLeft.X, tileLeft.Y);
            bool rightSolid = WorldGen.SolidTile(tileRight.X, tileRight.Y);
            
            isGrounded = (leftSolid || rightSolid) && NPC.velocity.Y >= 0;
        }
        
        private void SpawnSequence(Player target)
        {
            if (Timer == 1)
            {
                // Dramatic spawn announcement
                Main.NewText("The earth trembles as an ancient bell awakens...", 255, 100, 0);
                SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.5f, Volume = 1.5f }, NPC.Center);
                
                // Massive shockwave
                ThemedParticles.LaCampanellaShockwave(NPC.Center, 3f);
                screenShakeIntensity = 15f;
            }
            
            // Rising dramatic particles
            if (Timer % 5 == 0)
            {
                ThemedParticles.LaCampanellaBloomBurst(NPC.Center + Main.rand.NextVector2Circular(100, 100), 1.5f);
            }
            
            NPC.velocity.X = 0;
            
            if (Timer >= 120)
            {
                Main.NewText("La Campanella, Chime of Life has awoken!", 255, 150, 0);
                State = ActionState.Walking;
                Timer = 0;
            }
        }
        
        private void WalkingBehavior(Player target)
        {
            // Slow walking towards player
            float direction = Math.Sign(target.Center.X - NPC.Center.X);
            walkDirection = direction;
            
            if (isGrounded)
            {
                NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, direction * walkSpeed, 0.08f);
                
                // Walking dust
                if (Timer % 10 == 0)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 dustPos = NPC.Bottom + new Vector2(Main.rand.NextFloat(-40, 40), 0);
                        ThemedParticles.LaCampanellaSparks(dustPos, new Vector2(0, -1), 2, 2f);
                    }
                }
            }
            
            // Jump if player is significantly above or far away
            float verticalDiff = target.Center.Y - NPC.Center.Y;
            float horizontalDiff = Math.Abs(target.Center.X - NPC.Center.X);
            
            if (jumpCooldown <= 0 && isGrounded)
            {
                bool shouldJump = verticalDiff < -200 || horizontalDiff > 600 || attacksSinceLastJump >= 3;
                
                if (shouldJump)
                {
                    State = ActionState.JumpWindup;
                    Timer = 0;
                    attacksSinceLastJump = 0;
                    return;
                }
            }
            
            // Attack selection
            if (AttackCooldown <= 0 && Timer >= 90)
            {
                SelectAttack(target);
            }
        }
        
        private void SelectAttack(Player target)
        {
            float healthPercent = (float)NPC.life / NPC.lifeMax;
            int attackType;
            int maxAttacks;
            
            // More aggressive at lower health with access to more powerful attacks
            if (healthPercent < 0.2f || isEnraged)
            {
                // Critical health - FULL attack arsenal, most powerful attacks
                maxAttacks = 12;
                attackType = Main.rand.Next(maxAttacks);
                if (attackType == lastAttackType) attackType = (attackType + 1) % maxAttacks;
            }
            else if (healthPercent < 0.4f)
            {
                // Low health - powerful attacks unlocked
                maxAttacks = 10;
                attackType = Main.rand.Next(maxAttacks);
                if (attackType == lastAttackType) attackType = (attackType + 1) % maxAttacks;
            }
            else if (healthPercent < 0.65f)
            {
                // Medium health - mixed attacks
                maxAttacks = 8;
                attackType = Main.rand.Next(maxAttacks);
                if (attackType == lastAttackType) attackType = (attackType + 1) % maxAttacks;
            }
            else
            {
                // High health - basic attacks
                maxAttacks = 5;
                attackType = Main.rand.Next(maxAttacks);
                if (attackType == lastAttackType) attackType = (attackType + 1) % maxAttacks;
            }
            
            lastAttackType = attackType;
            attacksSinceLastJump++;
            consecutiveAttacks++;
            
            switch (attackType)
            {
                case 0:
                    State = ActionState.LaserWindup;
                    break;
                case 1:
                    State = ActionState.BellBarrageWindup;
                    break;
                case 2:
                    State = ActionState.InfernalWaveWindup;
                    break;
                case 3:
                    State = ActionState.MassiveLaserWindup;
                    break;
                case 4:
                    State = ActionState.BellStormWindup;
                    break;
                // Medium health unlocks
                case 5:
                    State = ActionState.SonicResonanceWindup;
                    break;
                case 6:
                    State = ActionState.FlameGeyserWindup;
                    break;
                case 7:
                    State = ActionState.ChimeCascadeWindup;
                    break;
                // Low health unlocks
                case 8:
                    State = ActionState.BellLightningWindup;
                    break;
                case 9:
                    State = ActionState.InfernalVortexWindup;
                    break;
                // Critical health unlocks
                case 10:
                    State = ActionState.InfernalBeamWindup;
                    break;
                case 11:
                    State = ActionState.TollOfDoomWindup;
                    break;
            }
            
            Timer = 0;
            AttackPhase = 0;
        }
        
        private void JumpWindup(Player target)
        {
            NPC.velocity.X *= 0.9f;
            
            // Crouch animation / charging particles
            if (Timer % 5 == 0)
            {
                Vector2 dustPos = NPC.Bottom + new Vector2(Main.rand.NextFloat(-60, 60), -10);
                ThemedParticles.LaCampanellaSparkles(dustPos, 3, 20f);
            }
            
            screenShakeIntensity = Math.Max(screenShakeIntensity, Timer / 30f * 3f);
            
            if (Timer >= 45)
            {
                // Launch!
                float horizontalDir = Math.Sign(target.Center.X - NPC.Center.X);
                float verticalPower = -18f;
                float horizontalPower = horizontalDir * 12f;
                
                // Adjust jump based on player position
                if (target.Center.Y < NPC.Center.Y - 300)
                    verticalPower = -22f;
                
                NPC.velocity = new Vector2(horizontalPower, verticalPower);
                
                // Jump effects
                SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.2f }, NPC.Center);
                ThemedParticles.LaCampanellaShockwave(NPC.Bottom, 2f);
                screenShakeIntensity = 12f;
                
                // Spawn dust burst
                for (int i = 0; i < 20; i++)
                {
                    ThemedParticles.LaCampanellaSparks(NPC.Bottom, new Vector2(Main.rand.NextFloat(-1, 1), -1), 3, 8f);
                }
                
                State = ActionState.Jumping;
                Timer = 0;
                jumpCooldown = 180;
            }
        }
        
        private void JumpingBehavior(Player target)
        {
            // Air control
            float direction = Math.Sign(target.Center.X - NPC.Center.X);
            NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, direction * 8f, 0.02f);
            
            // Falling particles
            if (Timer % 3 == 0)
            {
                ThemedParticles.LaCampanellaTrail(NPC.Center, NPC.velocity);
            }
            
            // Check for landing
            if (isGrounded && NPC.velocity.Y >= 0 && Timer > 10)
            {
                State = ActionState.Landing;
                Timer = 0;
            }
        }
        
        private void LandingBehavior(Player target)
        {
            if (Timer == 1)
            {
                // Landing impact!
                SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.4f, Volume = 1.3f }, NPC.Center);
                screenShakeIntensity = 20f;
                
                // Massive shockwave
                ThemedParticles.LaCampanellaShockwave(NPC.Bottom, 2.5f);
                ThemedParticles.LaCampanellaImpact(NPC.Bottom, 2f);
                
                // Spawn ground fire
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    for (int i = -3; i <= 3; i++)
                    {
                        Vector2 firePos = NPC.Bottom + new Vector2(i * 80, -20);
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), firePos, new Vector2(i * 2, -8),
                            ModContent.ProjectileType<InfernalGroundFire>(), NPC.damage / 2, 2f, Main.myPlayer);
                    }
                }
                
                // Dust burst
                for (int i = 0; i < 30; i++)
                {
                    float angle = MathHelper.TwoPi * i / 30f;
                    Vector2 dir = angle.ToRotationVector2();
                    ThemedParticles.LaCampanellaSparks(NPC.Bottom, dir, 3, 10f);
                }
            }
            
            NPC.velocity.X *= 0.85f;
            
            if (Timer >= 30)
            {
                State = ActionState.Walking;
                Timer = 0;
                AttackCooldown = 60;
            }
        }
        
        #region Attack Implementations
        
        private void LaserWindup(Player target)
        {
            NPC.velocity.X *= 0.9f;
            distortionIntensity = MathHelper.Lerp(distortionIntensity, 0.35f, 0.08f);
            
            // Charging effect - FULL VFX SUITE
            if (Timer % 2 == 0)
            {
                Vector2 chargePos = NPC.Center + new Vector2(0, -30);
                ThemedParticles.LaCampanellaSparkles(chargePos, 6, 50f);
                
                // Converging particles - MORE INTENSE
                for (int i = 0; i < 5; i++)
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 offset = angle.ToRotationVector2() * 100f;
                    var glow = new GenericGlowParticle(chargePos + offset, -offset * 0.04f, 
                        Main.rand.NextBool() ? ThemedParticles.CampanellaOrange : ThemedParticles.CampanellaYellow, 
                        0.5f, 25, true);
                    MagnumParticleHandler.SpawnParticle(glow);
                }
                
                // Central flare buildup
                float chargeProgress = Timer / 50f;
                CustomParticles.GenericFlare(chargePos, ThemedParticles.CampanellaYellow, 0.2f + chargeProgress * 0.5f, 18);
                CustomParticles.GenericFlare(chargePos, ThemedParticles.CampanellaOrange, 0.15f + chargeProgress * 0.4f, 15);
            }
            
            // Pulsing halo effect during charge
            if (Timer % 10 == 0)
            {
                Vector2 chargePos = NPC.Center + new Vector2(0, -30);
                float chargeProgress = Timer / 50f;
                CustomParticles.HaloRing(chargePos, ThemedParticles.CampanellaOrange * (0.5f + chargeProgress * 0.5f), 0.3f + chargeProgress * 0.3f, 20);
            }
            
            screenShakeIntensity = Math.Max(screenShakeIntensity, Timer / 60f * 6f);
            
            if (Timer >= 50) // Slightly faster
            {
                State = ActionState.LaserFiring;
                Timer = 0;
                AttackPhase = 0;
            }
        }
        
        private void LaserFiring(Player target)
        {
            NPC.velocity.X *= 0.95f;
            distortionIntensity = MathHelper.Lerp(distortionIntensity, 0.4f, 0.1f);
            
            // Fire lasers in bursts - MORE LASERS
            if (Timer % 12 == 0 && AttackPhase < 6)
            {
                SoundEngine.PlaySound(SoundID.Item33 with { Pitch = -0.3f }, NPC.Center);
                
                Vector2 laserStart = NPC.Center + new Vector2(0, -30);
                Vector2 toPlayer = target.Center - laserStart;
                toPlayer.Normalize();
                
                // Wider spread pattern
                int laserCount = isEnraged ? 5 : 3;
                float spreadAngle = isEnraged ? 0.12f : 0.15f;
                for (int i = -laserCount / 2; i <= laserCount / 2; i++)
                {
                    Vector2 dir = toPlayer.RotatedBy(i * spreadAngle);
                    
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), laserStart, dir * 16f,
                            ModContent.ProjectileType<InfernalBellLaser>(), NPC.damage / 2, 3f, Main.myPlayer);
                    }
                }
                
                // Muzzle flash - FULL VFX SUITE
                ThemedParticles.LaCampanellaBloomBurst(laserStart, 2f);
                ThemedParticles.LaCampanellaBellChime(laserStart, 0.8f);
                ThemedParticles.LaCampanellaHaloBurst(laserStart, 1f);
                CustomParticles.GenericFlare(laserStart, ThemedParticles.CampanellaYellow, 0.7f, 22);
                CustomParticles.GenericFlare(laserStart, ThemedParticles.CampanellaOrange, 0.5f, 18);
                CustomParticles.HaloRing(laserStart, ThemedParticles.CampanellaOrange, 0.4f, 18);
                screenShakeIntensity = 10f;
                
                AttackPhase++;
            }
            
            if (Timer >= 85)
            {
                State = ActionState.Walking;
                Timer = 0;
                AttackCooldown = 70;
            }
        }
        
        private void BellBarrageWindup(Player target)
        {
            NPC.velocity.X *= 0.9f;
            
            // Bell ringing buildup - ENHANCED with halos and flares
            bellRingTimer += 0.05f;
            if (Timer % 10 == 0)
            {
                SoundEngine.PlaySound(SoundID.Item35 with { Pitch = 0.3f + Timer / 100f, Volume = 0.6f }, NPC.Center);
                ThemedParticles.LaCampanellaShockwave(NPC.Center, 0.5f + Timer / 120f);
                CustomParticles.HaloRing(NPC.Center, ThemedParticles.CampanellaGold * 0.7f, 0.3f + Timer / 150f, 20);
            }
            
            // Continuous flare during charge
            if (Timer % 4 == 0)
            {
                float chargeProgress = Timer / 50f;
                CustomParticles.GenericFlare(NPC.Center + new Vector2(0, -40), ThemedParticles.CampanellaOrange, 0.2f + chargeProgress * 0.3f, 15);
            }
            
            if (Timer >= 50)
            {
                State = ActionState.BellBarrage;
                Timer = 0;
                AttackPhase = 0;
            }
        }
        
        private void BellBarrageFiring(Player target)
        {
            NPC.velocity.X *= 0.95f;
            
            // Fire explosive bells
            if (Timer % 12 == 0 && AttackPhase < 8)
            {
                SoundEngine.PlaySound(SoundID.Item35 with { Pitch = -0.2f }, NPC.Center);
                
                Vector2 spawnPos = NPC.Center + new Vector2(Main.rand.NextFloat(-50, 50), -60);
                Vector2 toPlayer = target.Center - spawnPos;
                toPlayer.Normalize();
                
                // Add some randomness
                float randomAngle = Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 velocity = toPlayer.RotatedBy(randomAngle) * Main.rand.NextFloat(8f, 12f);
                
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPos, velocity,
                        ModContent.ProjectileType<ExplosiveBellProjectile>(), NPC.damage / 2, 4f, Main.myPlayer);
                }
                
                // ENHANCED spawn VFX
                ThemedParticles.LaCampanellaSparkles(spawnPos, 5, 25f);
                CustomParticles.GenericFlare(spawnPos, ThemedParticles.CampanellaOrange, 0.4f, 15);
                CustomParticles.HaloRing(spawnPos, ThemedParticles.CampanellaGold * 0.6f, 0.25f, 12);
                AttackPhase++;
            }
            
            if (Timer >= 120)
            {
                State = ActionState.Walking;
                Timer = 0;
                AttackCooldown = 80;
            }
        }
        
        private void InfernalWaveWindup(Player target)
        {
            NPC.velocity.X *= 0.85f;
            
            // Ground charging effect - ENHANCED
            if (Timer % 4 == 0)
            {
                Vector2 groundPos = NPC.Bottom + new Vector2(Main.rand.NextFloat(-100, 100), 0);
                ThemedParticles.LaCampanellaSparks(groundPos, new Vector2(0, -1), 4, 5f);
                
                // Ground flares
                if (Main.rand.NextBool(2))
                {
                    CustomParticles.GenericFlare(groundPos + new Vector2(0, -5), ThemedParticles.CampanellaOrange, 0.25f, 12);
                }
            }
            
            // Pulsing halo at feet
            if (Timer % 12 == 0)
            {
                float chargeProgress = Timer / 40f;
                CustomParticles.HaloRing(NPC.Bottom, ThemedParticles.CampanellaOrange * (0.4f + chargeProgress * 0.4f), 0.3f + chargeProgress * 0.2f, 18);
            }
            
            screenShakeIntensity = Math.Max(screenShakeIntensity, Timer / 40f * 6f);
            
            if (Timer >= 40)
            {
                State = ActionState.InfernalWave;
                Timer = 0;
            }
        }
        
        private void InfernalWaveFiring(Player target)
        {
            if (Timer == 1)
            {
                SoundEngine.PlaySound(SoundID.Item74 with { Pitch = -0.5f, Volume = 1.2f }, NPC.Center);
                
                // Spawn fire waves in both directions
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    for (int dir = -1; dir <= 1; dir += 2)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Bottom, new Vector2(dir * 10f, 0),
                            ModContent.ProjectileType<InfernalFireWave>(), NPC.damage / 2, 5f, Main.myPlayer);
                    }
                }
                
                // FULL VFX on wave launch
                ThemedParticles.LaCampanellaShockwave(NPC.Bottom, 2f);
                ThemedParticles.LaCampanellaHaloBurst(NPC.Bottom, 1.2f);
                CustomParticles.GenericFlare(NPC.Bottom, ThemedParticles.CampanellaYellow, 0.7f, 22);
                CustomParticles.ExplosionBurst(NPC.Bottom, ThemedParticles.CampanellaOrange, 10, 6f);
                screenShakeIntensity = 15f;
            }
            
            NPC.velocity.X *= 0.9f;
            
            if (Timer >= 60)
            {
                State = ActionState.Walking;
                Timer = 0;
                AttackCooldown = 100;
            }
        }
        
        private void MassiveLaserWindup(Player target)
        {
            NPC.velocity.X *= 0.8f;
            
            // Dramatic buildup - ENHANCED
            if (Timer % 2 == 0)
            {
                // Converging energy from all directions
                for (int i = 0; i < 5; i++)
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 offset = angle.ToRotationVector2() * Main.rand.NextFloat(100f, 200f);
                    Vector2 targetPos = NPC.Center + new Vector2(0, -40);
                    
                    var glow = new GenericGlowParticle(targetPos + offset, -offset * 0.04f, 
                        Main.rand.NextBool() ? ThemedParticles.CampanellaOrange : ThemedParticles.CampanellaYellow, 
                        0.5f, 30, true);
                    MagnumParticleHandler.SpawnParticle(glow);
                }
                
                // Core flare buildup
                float chargeProgress = Timer / 90f;
                Vector2 corePos = NPC.Center + new Vector2(0, -40);
                CustomParticles.GenericFlare(corePos, ThemedParticles.CampanellaYellow, 0.3f + chargeProgress * 0.5f, 20);
            }
            
            // Pulsing halos during massive laser charge
            if (Timer % 15 == 0)
            {
                Vector2 corePos = NPC.Center + new Vector2(0, -40);
                float chargeProgress = Timer / 90f;
                CustomParticles.HaloRing(corePos, ThemedParticles.CampanellaOrange, 0.35f + chargeProgress * 0.3f, 22);
                ThemedParticles.LaCampanellaBellChime(corePos, 0.5f + chargeProgress * 0.5f);
            }
            
            if (Timer % 20 == 0)
            {
                SoundEngine.PlaySound(SoundID.Item29 with { Pitch = -0.5f + Timer / 200f }, NPC.Center);
            }
            
            screenShakeIntensity = Math.Max(screenShakeIntensity, Timer / 80f * 10f);
            
            // Flash sky effect
            if (!Main.dedServ && Timer >= 60)
            {
                if (SkyManager.Instance["MagnumOpus:LaCampanellaSky"] is LaCampanellaSkyEffect sky)
                {
                    sky.TriggerFlash(0.3f);
                }
            }
            
            if (Timer >= 90)
            {
                State = ActionState.MassiveLaser;
                Timer = 0;
            }
        }
        
        private void MassiveLaserFiring(Player target)
        {
            if (Timer == 1)
            {
                SoundEngine.PlaySound(SoundID.Zombie104 with { Pitch = -0.3f, Volume = 1.5f }, NPC.Center);
                
                Vector2 laserStart = NPC.Center + new Vector2(0, -40);
                Vector2 toPlayer = target.Center - laserStart;
                toPlayer.Normalize();
                
                // Massive laser beam
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), laserStart, toPlayer * 2f,
                        ModContent.ProjectileType<MassiveInfernalLaser>(), (int)(NPC.damage * 0.8f), 6f, Main.myPlayer);
                }
                
                // EPIC muzzle flash - FULL VFX SUITE
                ThemedParticles.LaCampanellaImpact(laserStart, 3f);
                ThemedParticles.LaCampanellaHaloBurst(laserStart, 2f);
                CustomParticles.GenericFlare(laserStart, ThemedParticles.CampanellaYellow, 1.0f, 30);
                CustomParticles.GenericFlare(laserStart, Color.White, 0.8f, 25);
                CustomParticles.HaloRing(laserStart, ThemedParticles.CampanellaOrange, 0.7f, 25);
                CustomParticles.HaloRing(laserStart, ThemedParticles.CampanellaYellow, 0.5f, 20);
                CustomParticles.ExplosionBurst(laserStart, ThemedParticles.CampanellaOrange, 16, 10f);
                screenShakeIntensity = 25f;
                
                // Flash sky
                if (!Main.dedServ)
                {
                    if (SkyManager.Instance["MagnumOpus:LaCampanellaSky"] is LaCampanellaSkyEffect sky)
                    {
                        sky.TriggerFlash(0.8f, Color.Orange);
                    }
                }
            }
            
            // Continuous flares while laser active
            if (Timer % 5 == 0)
            {
                Vector2 laserStart = NPC.Center + new Vector2(0, -40);
                CustomParticles.GenericFlare(laserStart + Main.rand.NextVector2Circular(20f, 20f), ThemedParticles.CampanellaOrange, 0.4f, 15);
            }
            
            NPC.velocity.X *= 0.95f;
            
            if (Timer >= 90)
            {
                State = ActionState.Walking;
                Timer = 0;
                AttackCooldown = 150;
            }
        }
        
        private void BellStormWindup(Player target)
        {
            NPC.velocity.X *= 0.85f;
            
            // Bells orbiting around boss - ENHANCED
            if (Timer % 8 == 0)
            {
                float angle = Timer * 0.15f;
                Vector2 orbitPos = NPC.Center + angle.ToRotationVector2() * 120f;
                ThemedParticles.LaCampanellaBloomBurst(orbitPos, 0.8f);
                CustomParticles.GenericFlare(orbitPos, ThemedParticles.CampanellaGold, 0.4f, 15);
                CustomParticles.HaloRing(orbitPos, ThemedParticles.CampanellaOrange * 0.6f, 0.25f, 12);
                
                SoundEngine.PlaySound(SoundID.Item35 with { Pitch = 0.5f, Volume = 0.4f }, orbitPos);
            }
            
            // Central buildup flares
            if (Timer % 6 == 0)
            {
                float chargeProgress = Timer / 70f;
                CustomParticles.GenericFlare(NPC.Center, ThemedParticles.CampanellaOrange, 0.2f + chargeProgress * 0.3f, 18);
            }
            
            screenShakeIntensity = Math.Max(screenShakeIntensity, Timer / 70f * 8f);
            
            if (Timer >= 70)
            {
                State = ActionState.BellStorm;
                Timer = 0;
                AttackPhase = 0;
            }
        }
        
        private void BellStormFiring(Player target)
        {
            // Spawn bells in circular patterns
            if (Timer % 8 == 0 && AttackPhase < 16)
            {
                float angle = AttackPhase * (MathHelper.TwoPi / 8f) + Timer * 0.02f;
                Vector2 spawnPos = NPC.Center + angle.ToRotationVector2() * 80f;
                Vector2 velocity = (target.Center - spawnPos).SafeNormalize(Vector2.UnitY) * 10f;
                
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPos, velocity,
                        ModContent.ProjectileType<ExplosiveBellProjectile>(), NPC.damage / 3, 3f, Main.myPlayer);
                }
                
                SoundEngine.PlaySound(SoundID.Item35 with { Pitch = -0.1f, Volume = 0.5f }, spawnPos);
                ThemedParticles.LaCampanellaSparkles(spawnPos, 4, 20f);
                
                // Bell spawn VFX - flare and halo per bell
                CustomParticles.GenericFlare(spawnPos, ThemedParticles.CampanellaGold, 0.5f, 18);
                CustomParticles.HaloRing(spawnPos, ThemedParticles.CampanellaOrange, 0.3f, 15);
                
                AttackPhase++;
            }
            
            // Central storm effect pulsing
            if (Timer % 10 == 0)
            {
                CustomParticles.GenericFlare(NPC.Center, ThemedParticles.CampanellaYellow * 0.6f, 0.3f, 12);
            }
            
            NPC.velocity.X *= 0.95f;
            
            if (Timer >= 150)
            {
                State = ActionState.Walking;
                Timer = 0;
                AttackCooldown = 120;
            }
        }
        
        #endregion
        
        #region NEW Advanced Attacks
        
        // =====================================================
        // ATTACK: SONIC RESONANCE - Expanding sound wave rings
        // =====================================================
        
        private void SonicResonanceWindup(Player target)
        {
            const int WindupTime = 40;
            NPC.velocity.X *= 0.85f;
            distortionIntensity = MathHelper.Lerp(distortionIntensity, 0.4f, 0.08f);
            
            if (Timer == 1)
            {
                SoundEngine.PlaySound(SoundID.Item35 with { Pitch = -0.8f, Volume = 1.3f }, NPC.Center);
            }
            
            // Building resonance - converging sound waves
            if (Timer % 3 == 0)
            {
                float chargeProgress = Timer / (float)WindupTime;
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f + Timer * 0.1f;
                    float radius = 150f * (1f - chargeProgress * 0.6f);
                    Vector2 ringPos = NPC.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                    
                    ThemedParticles.LaCampanellaSparkles(ringPos, 3, 15f);
                    CustomParticles.GenericFlare(ringPos, ThemedParticles.CampanellaOrange * 0.7f, 0.3f * chargeProgress, 12);
                }
                
                // Core vibration effect
                ThemedParticles.LaCampanellaBellChime(NPC.Center, 0.4f * chargeProgress);
            }
            
            screenShakeIntensity = Math.Max(screenShakeIntensity, Timer / WindupTime * 6f);
            
            if (Timer >= WindupTime)
            {
                State = ActionState.SonicResonance;
                Timer = 0;
                AttackPhase = 0;
            }
        }
        
        private void SonicResonanceAttack(Player target)
        {
            const int AttackDuration = 120;
            distortionIntensity = MathHelper.Lerp(distortionIntensity, 0.5f, 0.1f);
            
            NPC.velocity.X *= 0.9f;
            
            // Fire expanding sonic rings
            if (Timer % 20 == 0)
            {
                SoundEngine.PlaySound(SoundID.Item35 with { Pitch = 0.2f + Timer / 200f, Volume = 1f }, NPC.Center);
                
                // Create massive shockwave ring
                ThemedParticles.LaCampanellaShockwave(NPC.Center, 2.5f);
                
                // Spawn ring of projectiles expanding outward
                int bellCount = isEnraged ? 16 : 12;
                for (int i = 0; i < bellCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / bellCount;
                    Vector2 velocity = angle.ToRotationVector2() * 8f;
                    Vector2 spawnPos = NPC.Center + velocity * 3f;
                    
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPos, velocity,
                            ModContent.ProjectileType<ExplosiveBellProjectile>(), NPC.damage / 3, 2f, Main.myPlayer);
                    }
                    
                    // Visual ring burst
                    CustomParticles.GenericFlare(spawnPos, ThemedParticles.CampanellaOrange, 0.5f, 15);
                    ThemedParticles.LaCampanellaSparks(spawnPos, velocity, 4, 6f);
                }
                
                screenShakeIntensity = 12f;
                
                // Sky flash
                if (!Main.dedServ && SkyManager.Instance["MagnumOpus:LaCampanellaSky"] is LaCampanellaSkyEffect sky)
                {
                    sky.TriggerFlash(0.4f, ThemedParticles.CampanellaOrange);
                }
            }
            
            // Continuous particle aura
            if (Timer % 3 == 0)
            {
                ThemedParticles.LaCampanellaAura(NPC.Center, 80f);
            }
            
            if (Timer >= AttackDuration)
            {
                State = ActionState.Walking;
                Timer = 0;
                AttackCooldown = 80;
            }
        }
        
        // =====================================================
        // ATTACK: FLAME GEYSER - Erupting fire columns from ground
        // =====================================================
        
        private void FlameGeyserWindup(Player target)
        {
            const int WindupTime = 35;
            NPC.velocity.X *= 0.85f;
            
            if (Timer == 1)
            {
                SoundEngine.PlaySound(SoundID.Item45 with { Pitch = -0.6f }, NPC.Center);
                EroicaScreenShake.MediumShake(NPC.Center);
            }
            
            // Ground rumbling effect - telegraph where geysers will appear
            if (Timer > 15)
            {
                float telegraphIntensity = (Timer - 15f) / (WindupTime - 15f);
                
                // Show warning particles at upcoming geyser locations
                for (int i = -4; i <= 4; i++)
                {
                    if (i == 0) continue;
                    Vector2 geyserPos = target.Bottom + new Vector2(i * 120f, 0);
                    
                    if (Main.rand.NextFloat() < telegraphIntensity * 0.4f)
                    {
                        CustomParticles.GenericFlare(geyserPos, ThemedParticles.CampanellaYellow * telegraphIntensity * 0.5f, 0.2f * telegraphIntensity, 8);
                        
                        // Rising warning embers
                        var ember = new GenericGlowParticle(geyserPos + Main.rand.NextVector2Circular(20f, 10f), 
                            new Vector2(0, -2f), ThemedParticles.CampanellaOrange, 0.25f * telegraphIntensity, 20, true);
                        MagnumParticleHandler.SpawnParticle(ember);
                    }
                }
            }
            
            screenShakeIntensity = Math.Max(screenShakeIntensity, Timer / WindupTime * 8f);
            
            if (Timer >= WindupTime)
            {
                State = ActionState.FlameGeyser;
                Timer = 0;
                AttackPhase = 0;
            }
        }
        
        private void FlameGeyserAttack(Player target)
        {
            const int AttackDuration = 150;
            distortionIntensity = MathHelper.Lerp(distortionIntensity, 0.45f, 0.1f);
            
            NPC.velocity.X *= 0.9f;
            
            // Spawn geysers in sequence
            if (Timer % 15 == 0 && AttackPhase < 10)
            {
                SoundEngine.PlaySound(SoundID.Item74 with { Pitch = 0.3f, Volume = 1.1f }, target.Center);
                
                // Alternate sides with some randomness
                int geyserIndex = (int)AttackPhase - 5;
                if (geyserIndex == 0) geyserIndex = Main.rand.NextBool() ? -1 : 1;
                
                Vector2 geyserPos = target.Bottom + new Vector2(geyserIndex * 100f + Main.rand.NextFloat(-30f, 30f), 0);
                
                // Spawn upward fire projectiles
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    int fireCount = isEnraged ? 5 : 3;
                    for (int i = 0; i < fireCount; i++)
                    {
                        float spread = (i - fireCount / 2) * 0.15f;
                        Vector2 velocity = new Vector2(spread * 3f, -12f - i * 2f);
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), geyserPos, velocity,
                            ModContent.ProjectileType<InfernalGroundFire>(), NPC.damage / 3, 2f, Main.myPlayer);
                    }
                }
                
                // MASSIVE explosion effect at geyser point - FULL VFX SUITE
                ThemedParticles.LaCampanellaImpact(geyserPos, 2f);
                ThemedParticles.LaCampanellaShockwave(geyserPos, 1.5f);
                ThemedParticles.LaCampanellaHaloBurst(geyserPos, 1.5f);
                CustomParticles.GenericFlare(geyserPos, ThemedParticles.CampanellaOrange, 0.8f, 25);
                CustomParticles.GenericFlare(geyserPos, ThemedParticles.CampanellaYellow, 0.6f, 20);
                CustomParticles.HaloRing(geyserPos, Color.White * 0.8f, 0.4f, 18);
                CustomParticles.ExplosionBurst(geyserPos, ThemedParticles.CampanellaOrange, 14, 12f);
                
                // Fire pillar particles
                for (int i = 0; i < 20; i++)
                {
                    Vector2 vel = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-15f, -8f));
                    var glow = new GenericGlowParticle(geyserPos + Main.rand.NextVector2Circular(30f, 10f), 
                        vel, Main.rand.NextBool() ? ThemedParticles.CampanellaOrange : ThemedParticles.CampanellaYellow, 
                        Main.rand.NextFloat(0.4f, 0.7f), Main.rand.Next(30, 50), true);
                    MagnumParticleHandler.SpawnParticle(glow);
                }
                
                // Black smoke column
                for (int i = 0; i < 8; i++)
                {
                    var smoke = new HeavySmokeParticle(geyserPos + Main.rand.NextVector2Circular(25f, 5f), 
                        new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-6f, -3f)), 
                        ThemedParticles.CampanellaBlack, Main.rand.Next(50, 80), Main.rand.NextFloat(0.5f, 0.9f), 
                        0.6f, 0.02f, false);
                    MagnumParticleHandler.SpawnParticle(smoke);
                }
                
                screenShakeIntensity = 10f;
                AttackPhase++;
            }
            
            if (Timer >= AttackDuration)
            {
                State = ActionState.Walking;
                Timer = 0;
                AttackCooldown = 90;
            }
        }
        
        // =====================================================
        // ATTACK: BELL LIGHTNING - Infernal fractal lightning storm
        // =====================================================
        
        private void BellLightningWindup(Player target)
        {
            const int WindupTime = 30;
            NPC.velocity.X *= 0.85f;
            distortionIntensity = MathHelper.Lerp(distortionIntensity, 0.5f, 0.1f);
            
            if (Timer == 1)
            {
                SoundEngine.PlaySound(SoundID.Thunder with { Volume = 1.2f }, NPC.Center);
                EroicaScreenShake.MediumShake(NPC.Center);
                
                // Pre-calculate lightning strike positions around player
                lightningStrikeIndex = 0;
                int strikeCount = isEnraged ? 10 : 8;
                for (int i = 0; i < strikeCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / strikeCount + Main.rand.NextFloat(-0.2f, 0.2f);
                    float distance = 120f + Main.rand.NextFloat(60f);
                    telegraphedLightningPositions[i] = target.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * distance;
                }
            }
            
            // Crackling energy buildup
            if (Timer % 2 == 0)
            {
                Vector2 offset = Main.rand.NextVector2Circular(120f, 120f);
                CustomParticles.GenericFlare(NPC.Center + offset, ThemedParticles.CampanellaOrange * 0.7f, 0.3f, 12);
                ThemedParticles.LaCampanellaSparkles(NPC.Center + offset, 2, 15f);
            }
            
            // Telegraph: Show warning at strike positions
            if (Timer > 15)
            {
                float telegraphIntensity = (Timer - 15f) / 15f;
                int strikeCount = isEnraged ? 10 : 8;
                for (int i = 0; i < strikeCount; i++)
                {
                    Vector2 pos = telegraphedLightningPositions[i];
                    if (Main.rand.NextFloat() < telegraphIntensity * 0.5f)
                    {
                        CustomParticles.GenericFlare(pos, ThemedParticles.CampanellaYellow * telegraphIntensity * 0.6f, 0.25f * telegraphIntensity, 10);
                    }
                }
            }
            
            screenShakeIntensity = Math.Max(screenShakeIntensity, Timer / WindupTime * 8f);
            
            if (Timer >= WindupTime)
            {
                State = ActionState.BellLightning;
                Timer = 0;
                AttackPhase = 0;
                lightningStrikeIndex = 0;
            }
        }
        
        private void BellLightningAttack(Player target)
        {
            const int AttackDuration = 80;
            distortionIntensity = MathHelper.Lerp(distortionIntensity, 0.55f, 0.1f);
            
            // Aggressive movement during lightning
            Vector2 toPlayer = (target.Center - NPC.Center).SafeNormalize(Vector2.Zero);
            NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, toPlayer.X * 8f, 0.05f);
            
            // Strike lightning at telegraphed positions
            int strikeCount = isEnraged ? 10 : 8;
            if (Timer % 8 == 0 && lightningStrikeIndex < strikeCount)
            {
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.5f, Volume = 0.8f }, NPC.Center);
                
                Vector2 strikePos = telegraphedLightningPositions[lightningStrikeIndex];
                lightningStrikeIndex++;
                
                // Draw infernal lightning from boss to strike point
                MagnumVFX.DrawLaCampanellaLightning(NPC.Center, strikePos, 10, 35f, 4, 0.5f);
                
                // Impact explosion - FULL VFX SUITE
                ThemedParticles.LaCampanellaImpact(strikePos, 1.5f);
                ThemedParticles.LaCampanellaHaloBurst(strikePos, 1.2f);
                CustomParticles.GenericFlare(strikePos, ThemedParticles.CampanellaYellow, 0.7f, 20);
                CustomParticles.GenericFlare(strikePos, Color.White, 0.5f, 15);
                CustomParticles.HaloRing(strikePos, ThemedParticles.CampanellaOrange, 0.4f, 18);
                CustomParticles.ExplosionBurst(strikePos, ThemedParticles.CampanellaOrange, 10, 8f);
                CustomParticles.ExplosionBurst(strikePos, ThemedParticles.CampanellaBlack, 6, 5f);
                
                // Fire burst at strike point
                for (int i = 0; i < 12; i++)
                {
                    float angle = MathHelper.TwoPi * i / 12f;
                    Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                    var spark = new GlowSparkParticle(strikePos, vel, true, 
                        Main.rand.Next(20, 35), Main.rand.NextFloat(0.4f, 0.7f), 
                        ThemedParticles.CampanellaOrange, new Vector2(0.4f, 1.6f), false, true);
                    MagnumParticleHandler.SpawnParticle(spark);
                }
                
                screenShakeIntensity = 8f;
            }
            
            if (Timer >= AttackDuration)
            {
                State = ActionState.Walking;
                Timer = 0;
                AttackCooldown = 100;
            }
        }
        
        // =====================================================
        // ATTACK: INFERNAL VORTEX - Spiraling inward fire beams
        // =====================================================
        
        private void InfernalVortexWindup(Player target)
        {
            const int WindupTime = 50;
            NPC.velocity.X *= 0.8f;
            distortionIntensity = MathHelper.Lerp(distortionIntensity, 0.55f, 0.08f);
            
            if (Timer == 1)
            {
                SoundEngine.PlaySound(SoundID.Roar with { Pitch = -0.3f }, NPC.Center);
                EroicaScreenShake.LargeShake(NPC.Center);
                Main.NewText("The bell channels infernal fury!", 255, 100, 0);
                
                // Initialize vortex beam angles
                for (int i = 0; i < 8; i++)
                {
                    vortexBeamAngles[i] = MathHelper.TwoPi * i / 8f;
                }
                vortexRotationSpeed = 0f;
            }
            
            // Spiraling energy gathering
            if (Timer % 2 == 0)
            {
                float chargeProgress = Timer / (float)WindupTime;
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi * i / 6f + Timer * 0.15f;
                    float radius = 180f * (1f - chargeProgress * 0.4f);
                    Vector2 ringPos = NPC.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                    
                    CustomParticles.GenericFlare(ringPos, ThemedParticles.CampanellaOrange, 0.4f * chargeProgress, 15);
                    ThemedParticles.LaCampanellaSparks(ringPos, (NPC.Center - ringPos).SafeNormalize(Vector2.UnitY), 3, 4f);
                }
                
                // Core buildup
                CustomParticles.GenericFlare(NPC.Center, ThemedParticles.CampanellaYellow, 0.3f + chargeProgress * 0.4f, 20);
            }
            
            screenShakeIntensity = Math.Max(screenShakeIntensity, Timer / WindupTime * 12f);
            
            if (Timer >= WindupTime)
            {
                State = ActionState.InfernalVortex;
                Timer = 0;
                vortexRotationSpeed = 0.02f;
            }
        }
        
        private void InfernalVortexAttack(Player target)
        {
            const int AttackDuration = 180;
            distortionIntensity = 0.6f;
            
            // Slow chase during vortex
            Vector2 toPlayer = (target.Center - NPC.Center).SafeNormalize(Vector2.Zero);
            NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, toPlayer.X * 4f, 0.03f);
            
            // Accelerating rotation
            vortexRotationSpeed = MathHelper.Lerp(vortexRotationSpeed, isEnraged ? 0.07f : 0.05f, 0.01f);
            
            // Update and draw vortex beams
            for (int i = 0; i < 8; i++)
            {
                vortexBeamAngles[i] += vortexRotationSpeed;
                
                // Spiraling toward player
                float spiralFactor = 1f - (Timer / (float)AttackDuration) * 0.5f;
                float beamLength = 1500f * spiralFactor;
                
                Vector2 beamDir = vortexBeamAngles[i].ToRotationVector2();
                Vector2 beamEnd = NPC.Center + beamDir * beamLength;
                
                // Draw the fire beam
                DrawInfernalBeam(NPC.Center, vortexBeamAngles[i], beamLength * 0.8f);
            }
            
            // Spawn fire projectiles along beams periodically
            if (Timer % 25 == 0)
            {
                SoundEngine.PlaySound(SoundID.Item45 with { Pitch = 0.3f, Volume = 0.7f }, NPC.Center);
                
                // Burst VFX from core
                ThemedParticles.LaCampanellaHaloBurst(NPC.Center, 1.2f);
                CustomParticles.GenericFlare(NPC.Center, ThemedParticles.CampanellaOrange, 0.7f, 20);
                
                for (int i = 0; i < 8; i++)
                {
                    Vector2 beamDir = vortexBeamAngles[i].ToRotationVector2();
                    Vector2 spawnPos = NPC.Center + beamDir * 200f;
                    
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPos, beamDir * 10f,
                            ModContent.ProjectileType<InfernalBellLaser>(), NPC.damage / 4, 2f, Main.myPlayer);
                    }
                    
                    // Spawn VFX per projectile
                    CustomParticles.GenericFlare(spawnPos, ThemedParticles.CampanellaYellow, 0.4f, 15);
                    CustomParticles.HaloRing(spawnPos, ThemedParticles.CampanellaOrange * 0.6f, 0.25f, 12);
                }
            }
            
            // Continuous particles
            if (Timer % 2 == 0)
            {
                for (int i = 0; i < 4; i++)
                {
                    float spiralAngle = Timer * 0.1f + i * MathHelper.PiOver2;
                    float spiralRadius = 50f + (float)Math.Sin(Timer * 0.08f + i) * 20f;
                    Vector2 spiralPos = NPC.Center + new Vector2((float)Math.Cos(spiralAngle), (float)Math.Sin(spiralAngle)) * spiralRadius;
                    
                    ThemedParticles.LaCampanellaSparks(spiralPos, spiralAngle.ToRotationVector2(), 3, 5f);
                }
            }
            
            // Screen shake
            if (Timer % 10 == 0)
            {
                screenShakeIntensity = 6f;
            }
            
            if (Timer >= AttackDuration)
            {
                State = ActionState.Walking;
                Timer = 0;
                AttackCooldown = 150;
            }
        }
        
        // =====================================================
        // ATTACK: CHIME CASCADE - Falling bell projectiles from above
        // =====================================================
        
        private void ChimeCascadeWindup(Player target)
        {
            const int WindupTime = 40;
            NPC.velocity.X *= 0.85f;
            
            if (Timer == 1)
            {
                SoundEngine.PlaySound(SoundID.Item35 with { Pitch = 0.8f, Volume = 1.2f }, NPC.Center);
            }
            
            // Rising energy to sky
            if (Timer % 3 == 0)
            {
                Vector2 upwardPos = NPC.Center + new Vector2(Main.rand.NextFloat(-80f, 80f), -Timer * 3f);
                ThemedParticles.LaCampanellaSparkles(upwardPos, 4, 20f);
                CustomParticles.GenericFlare(upwardPos, ThemedParticles.CampanellaOrange * 0.6f, 0.3f, 10);
            }
            
            // Telegraph - show warning particles above player
            if (Timer > 20)
            {
                float telegraphIntensity = (Timer - 20f) / 20f;
                for (int i = -3; i <= 3; i++)
                {
                    Vector2 warningPos = target.Center + new Vector2(i * 100f, -400f);
                    if (Main.rand.NextFloat() < telegraphIntensity * 0.3f)
                    {
                        CustomParticles.GenericFlare(warningPos, ThemedParticles.CampanellaYellow * telegraphIntensity * 0.4f, 0.2f, 8);
                    }
                }
            }
            
            screenShakeIntensity = Math.Max(screenShakeIntensity, Timer / WindupTime * 5f);
            
            if (Timer >= WindupTime)
            {
                State = ActionState.ChimeCascade;
                Timer = 0;
                AttackPhase = 0;
            }
        }
        
        private void ChimeCascadeAttack(Player target)
        {
            const int AttackDuration = 120;
            distortionIntensity = MathHelper.Lerp(distortionIntensity, 0.35f, 0.08f);
            
            NPC.velocity.X *= 0.9f;
            
            // Spawn falling bells from above
            if (Timer % 8 == 0)
            {
                SoundEngine.PlaySound(SoundID.Item35 with { Pitch = Main.rand.NextFloat(-0.2f, 0.3f), Volume = 0.6f }, target.Center);
                
                int bellCount = isEnraged ? 5 : 3;
                for (int i = 0; i < bellCount; i++)
                {
                    float xOffset = Main.rand.NextFloat(-350f, 350f);
                    Vector2 spawnPos = target.Center + new Vector2(xOffset, -500f);
                    Vector2 velocity = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(6f, 10f));
                    
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPos, velocity,
                            ModContent.ProjectileType<ExplosiveBellProjectile>(), NPC.damage / 3, 3f, Main.myPlayer);
                    }
                    
                    // Spawn particle at origin - FULL VFX
                    ThemedParticles.LaCampanellaBloomBurst(spawnPos, 0.8f);
                    CustomParticles.GenericFlare(spawnPos, ThemedParticles.CampanellaGold, 0.4f, 15);
                    CustomParticles.HaloRing(spawnPos, ThemedParticles.CampanellaOrange, 0.3f, 12);
                }
            }
            
            // Ambient falling sparks
            if (Timer % 3 == 0)
            {
                Vector2 sparkPos = target.Center + new Vector2(Main.rand.NextFloat(-400f, 400f), -400f + Main.rand.NextFloat(-100f, 100f));
                var spark = new GlowSparkParticle(sparkPos, new Vector2(Main.rand.NextFloat(-1f, 1f), 8f), true, 
                    Main.rand.Next(30, 50), Main.rand.NextFloat(0.3f, 0.5f), 
                    ThemedParticles.CampanellaOrange, new Vector2(0.3f, 1.5f), false, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }
            
            if (Timer >= AttackDuration)
            {
                State = ActionState.Walking;
                Timer = 0;
                AttackCooldown = 70;
            }
        }
        
        // =====================================================
        // ATTACK: INFERNAL BEAM - Rotating massive fire laser (like Swan Lake's Apocalypse)
        // =====================================================
        
        private void InfernalBeamWindup(Player target)
        {
            const int WindupTime = 55;
            NPC.velocity.X *= 0.75f;
            distortionIntensity = MathHelper.Lerp(distortionIntensity, 0.65f, 0.08f);
            
            if (Timer == 1)
            {
                SoundEngine.PlaySound(SoundID.Roar with { Pitch = -0.5f, Volume = 1.3f }, NPC.Center);
                EroicaScreenShake.LargeShake(NPC.Center);
                Main.NewText("La Campanella channels the Infernal Radiance!", 255, 100, 0);
                beamRotationAngle = 0f;
                beamLength = 0f;
            }
            
            // Intense energy buildup - rings of fire spiraling inward
            if (Timer % 2 == 0)
            {
                float chargeProgress = Timer / (float)WindupTime;
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f + Timer * 0.12f;
                    float radius = 220f * (1f - chargeProgress * 0.6f);
                    Vector2 ringPos = NPC.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                    
                    CustomParticles.GenericFlare(ringPos, ThemedParticles.CampanellaOrange, 0.5f * chargeProgress, 18);
                    ThemedParticles.LaCampanellaSparks(ringPos, (NPC.Center - ringPos).SafeNormalize(Vector2.UnitY), 4, 5f);
                }
                
                // Core energy buildup
                CustomParticles.GenericFlare(NPC.Center, ThemedParticles.CampanellaYellow, 0.4f + chargeProgress * 0.6f, 25);
                ThemedParticles.LaCampanellaBellChime(NPC.Center, chargeProgress);
            }
            
            // Screenshake building up
            if (Timer % 8 == 0)
            {
                screenShakeIntensity = Math.Max(screenShakeIntensity, Timer / WindupTime * 15f);
            }
            
            if (Timer >= WindupTime)
            {
                State = ActionState.InfernalBeam;
                Timer = 0;
                beamRotationAngle = 0f;
                beamLength = 3500f; // MASSIVE beam
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.6f, Volume = 1.4f }, NPC.Center);
                EroicaScreenShake.Phase2EnrageShake(NPC.Center);
            }
        }
        
        private void InfernalBeamAttack(Player target)
        {
            const int AttackDuration = 180;
            distortionIntensity = 0.7f;
            
            // Slow movement during beam
            Vector2 toPlayer = (target.Center - NPC.Center).SafeNormalize(Vector2.Zero);
            NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, toPlayer.X * 3f, 0.03f);
            
            // Rotate the beam - CLOCKWISE
            float rotationSpeed = isEnraged ? 0.045f : 0.035f;
            beamRotationAngle += rotationSpeed;
            
            // Draw the infernal beam
            DrawInfernalBeam(NPC.Center, beamRotationAngle, beamLength);
            
            // Secondary beam at 180 degrees for extra danger
            if (Timer > 80 && isEnraged)
            {
                DrawInfernalBeam(NPC.Center, beamRotationAngle + MathHelper.Pi, beamLength * 0.6f);
            }
            
            // Particles spiraling around boss
            if (Timer % 2 == 0)
            {
                for (int i = 0; i < 4; i++)
                {
                    float spiralAngle = beamRotationAngle * 2f + i * MathHelper.PiOver2;
                    float spiralRadius = 60f + (float)Math.Sin(Timer * 0.1f + i) * 25f;
                    Vector2 spiralPos = NPC.Center + new Vector2((float)Math.Cos(spiralAngle), (float)Math.Sin(spiralAngle)) * spiralRadius;
                    
                    ThemedParticles.LaCampanellaSparks(spiralPos, spiralAngle.ToRotationVector2(), 4, 6f);
                }
            }
            
            // Bell chime particles along beam
            if (Timer % 10 == 0)
            {
                ThemedParticles.LaCampanellaBellChime(NPC.Center, 1.2f);
                
                // Flares along beam direction
                Vector2 beamDir = beamRotationAngle.ToRotationVector2();
                for (int f = 0; f < 5; f++)
                {
                    Vector2 flarePos = NPC.Center + beamDir * (150f + f * 150f);
                    CustomParticles.GenericFlare(flarePos, ThemedParticles.CampanellaOrange, 0.7f, 18);
                }
            }
            
            // Screenshake
            if (Timer % 6 == 0)
            {
                screenShakeIntensity = 8f;
            }
            
            // Sound effects
            if (Timer % 18 == 0)
            {
                SoundEngine.PlaySound(SoundID.Item15 with { Pitch = 0.2f, Volume = 0.7f }, NPC.Center);
            }
            
            if (Timer >= AttackDuration)
            {
                State = ActionState.Walking;
                Timer = 0;
                beamLength = 0f;
                AttackCooldown = 180;
            }
        }
        
        /// <summary>
        /// Draws a massive infernal fire beam like Swan Lake's prismatic beam
        /// </summary>
        private void DrawInfernalBeam(Vector2 origin, float angle, float length)
        {
            Vector2 direction = angle.ToRotationVector2();
            Vector2 perpendicular = direction.RotatedBy(MathHelper.PiOver2);
            
            int segments = 45;
            float segmentLength = length / segments;
            
            for (int i = 0; i < segments; i++)
            {
                Vector2 segmentPos = origin + direction * (i * segmentLength);
                float segmentProgress = i / (float)segments;
                
                // Orange to yellow gradient along beam
                Color beamColor = Color.Lerp(ThemedParticles.CampanellaOrange, ThemedParticles.CampanellaYellow, segmentProgress * 0.6f);
                
                // Core flares
                float coreSize = 0.85f - segmentProgress * 0.35f;
                CustomParticles.GenericFlare(segmentPos, ThemedParticles.CampanellaYellow, coreSize * 0.7f, 18);
                CustomParticles.GenericFlare(segmentPos, beamColor, coreSize, 14);
                
                // Wide dust beam
                if (i % 2 == 0)
                {
                    float dustScale = 3.5f - segmentProgress * 1.2f;
                    
                    for (int layer = 0; layer < 3; layer++)
                    {
                        float layerOffset = 10f + layer * 15f;
                        
                        // Fire dust on both sides
                        Dust fireTop = Dust.NewDustPerfect(segmentPos + perpendicular * layerOffset, DustID.Torch,
                            perpendicular * Main.rand.NextFloat(0.5f, 2f), 0, beamColor, dustScale - layer * 0.6f);
                        fireTop.noGravity = true;
                        
                        Dust fireBottom = Dust.NewDustPerfect(segmentPos - perpendicular * layerOffset, DustID.Torch,
                            -perpendicular * Main.rand.NextFloat(0.5f, 2f), 0, beamColor, dustScale - layer * 0.6f);
                        fireBottom.noGravity = true;
                    }
                }
                
                // Black smoke outline
                if (i % 3 == 0)
                {
                    float blackOffset = 40f + Main.rand.NextFloat(10f);
                    Dust blackTop = Dust.NewDustPerfect(segmentPos + perpendicular * blackOffset,
                        DustID.Smoke, new Vector2(0, -0.5f), 200, Color.Black, 2.2f);
                    blackTop.noGravity = true;
                    
                    Dust blackBottom = Dust.NewDustPerfect(segmentPos - perpendicular * blackOffset,
                        DustID.Smoke, new Vector2(0, -0.5f), 200, Color.Black, 2.2f);
                    blackBottom.noGravity = true;
                }
                
                // Edge sparkles
                if (i % 4 == 0)
                {
                    float perpOffset = 30f - segmentProgress * 10f;
                    ThemedParticles.LaCampanellaSparks(segmentPos + perpendicular * perpOffset, perpendicular, 2, 3f);
                    ThemedParticles.LaCampanellaSparks(segmentPos - perpendicular * perpOffset, -perpendicular, 2, 3f);
                }
                
                // Intense lighting
                float lightIntensity = 1.8f - segmentProgress * 0.7f;
                Lighting.AddLight(segmentPos, ThemedParticles.CampanellaOrange.ToVector3() * lightIntensity);
            }
            
            // Impact at beam end
            Vector2 beamEnd = origin + direction * length;
            ThemedParticles.LaCampanellaImpact(beamEnd, 1.5f);
            
            // Lightning from beam end occasionally
            if (Main.rand.NextBool(3))
            {
                Vector2 lightningEnd = beamEnd + Main.rand.NextVector2Circular(100f, 100f);
                MagnumVFX.DrawLaCampanellaLightning(beamEnd, lightningEnd, 6, 20f, 2, 0.3f);
            }
        }
        
        // =====================================================
        // ATTACK: TOLL OF DOOM - Ultimate devastating attack
        // =====================================================
        
        private void TollOfDoomWindup(Player target)
        {
            const int WindupTime = 75;
            NPC.velocity.X *= 0.7f;
            distortionIntensity = MathHelper.Lerp(distortionIntensity, 0.75f, 0.06f);
            
            if (Timer == 1)
            {
                SoundEngine.PlaySound(SoundID.Roar with { Pitch = -0.8f, Volume = 1.5f }, NPC.Center);
                EroicaScreenShake.Phase2EnrageShake(NPC.Center);
                Main.NewText("THE BELL TOLLS FOR THEE!", 255, 50, 0);
            }
            
            // Dramatic bell tolling sounds
            if (Timer % 25 == 0)
            {
                SoundEngine.PlaySound(SoundID.Item35 with { Pitch = -0.5f, Volume = 1.4f }, NPC.Center);
                ThemedParticles.LaCampanellaShockwave(NPC.Center, 2f + Timer / 50f);
                
                // Sky flash
                if (!Main.dedServ && SkyManager.Instance["MagnumOpus:LaCampanellaSky"] is LaCampanellaSkyEffect sky)
                {
                    sky.TriggerFlash(0.5f + Timer / 150f, ThemedParticles.CampanellaOrange);
                }
            }
            
            // Converging energy from everywhere
            if (Timer % 2 == 0)
            {
                float chargeProgress = Timer / (float)WindupTime;
                
                for (int i = 0; i < 10; i++)
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    float distance = Main.rand.NextFloat(150f, 300f);
                    Vector2 offset = angle.ToRotationVector2() * distance;
                    
                    var glow = new GenericGlowParticle(NPC.Center + offset, -offset * 0.05f, 
                        Main.rand.NextBool() ? ThemedParticles.CampanellaOrange : ThemedParticles.CampanellaYellow, 
                        0.5f * chargeProgress, 25, true);
                    MagnumParticleHandler.SpawnParticle(glow);
                }
                
                // Core growing more intense
                CustomParticles.GenericFlare(NPC.Center, ThemedParticles.CampanellaYellow, 0.5f + chargeProgress * 0.8f, 30);
                ThemedParticles.LaCampanellaBellChime(NPC.Center, chargeProgress * 1.5f);
            }
            
            screenShakeIntensity = Math.Max(screenShakeIntensity, Timer / WindupTime * 20f);
            
            if (Timer >= WindupTime)
            {
                State = ActionState.TollOfDoom;
                Timer = 0;
                AttackPhase = 0;
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.8f, Volume = 1.6f }, NPC.Center);
            }
        }
        
        private void TollOfDoomAttack(Player target)
        {
            const int AttackDuration = 200;
            distortionIntensity = 0.8f;
            
            NPC.velocity.X *= 0.85f;
            
            // Phase 1: Massive omni-directional bell barrage (0-60)
            if (Timer <= 60 && Timer % 4 == 0)
            {
                SoundEngine.PlaySound(SoundID.Item35 with { Pitch = Main.rand.NextFloat(-0.3f, 0.3f), Volume = 0.7f }, NPC.Center);
                
                // Central burst VFX
                ThemedParticles.LaCampanellaShockwave(NPC.Center, 1.5f);
                ThemedParticles.LaCampanellaHaloBurst(NPC.Center, 1.2f);
                CustomParticles.GenericFlare(NPC.Center, ThemedParticles.CampanellaOrange, 0.8f, 25);
                
                int bellCount = 16;
                for (int i = 0; i < bellCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / bellCount + Timer * 0.05f;
                    Vector2 velocity = angle.ToRotationVector2() * 12f;
                    Vector2 spawnPos = NPC.Center + velocity * 2f;
                    
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPos, velocity,
                            ModContent.ProjectileType<ExplosiveBellProjectile>(), NPC.damage / 4, 2f, Main.myPlayer);
                    }
                    
                    ThemedParticles.LaCampanellaSparks(spawnPos, velocity, 3, 6f);
                    CustomParticles.GenericFlare(spawnPos, ThemedParticles.CampanellaYellow, 0.3f, 12);
                }
            }
            
            // Phase 2: Lightning storm (60-120)
            if (Timer > 60 && Timer <= 120 && Timer % 6 == 0)
            {
                Vector2 strikePos = target.Center + Main.rand.NextVector2Circular(200f, 200f);
                MagnumVFX.DrawLaCampanellaLightning(NPC.Center, strikePos, 12, 40f, 5, 0.6f);
                ThemedParticles.LaCampanellaImpact(strikePos, 2f);
                ThemedParticles.LaCampanellaHaloBurst(strikePos, 1.5f);
                CustomParticles.GenericFlare(strikePos, ThemedParticles.CampanellaYellow, 0.6f, 18);
                CustomParticles.HaloRing(strikePos, ThemedParticles.CampanellaOrange, 0.4f, 15);
                screenShakeIntensity = 10f;
                
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.3f, Volume = 0.6f }, strikePos);
            }
            
            // Phase 3: Fire geysers all around (120-180)
            if (Timer > 120 && Timer <= 180 && Timer % 10 == 0)
            {
                for (int i = -2; i <= 2; i++)
                {
                    Vector2 geyserPos = target.Bottom + new Vector2(i * 150f + Main.rand.NextFloat(-30f, 30f), 0);
                    
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            Vector2 velocity = new Vector2(Main.rand.NextFloat(-2f, 2f), -10f - j * 3f);
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), geyserPos, velocity,
                                ModContent.ProjectileType<InfernalGroundFire>(), NPC.damage / 4, 2f, Main.myPlayer);
                        }
                    }
                    
                    // Geyser burst VFX - FULL SUITE
                    ThemedParticles.LaCampanellaImpact(geyserPos, 1.5f);
                    ThemedParticles.LaCampanellaHaloBurst(geyserPos, 1.2f);
                    CustomParticles.GenericFlare(geyserPos, ThemedParticles.CampanellaOrange, 0.6f, 20);
                    CustomParticles.HaloRing(geyserPos, ThemedParticles.CampanellaYellow * 0.8f, 0.3f, 15);
                }
                
                SoundEngine.PlaySound(SoundID.Item74 with { Pitch = 0.2f, Volume = 1f }, target.Center);
            }
            
            // Continuous intense particles
            if (Timer % 2 == 0)
            {
                ThemedParticles.LaCampanellaAura(NPC.Center, 100f);
                CustomParticles.GenericFlare(NPC.Center, ThemedParticles.CampanellaOrange, 0.6f, 20);
            }
            
            // Constant screen shake
            screenShakeIntensity = Math.Max(screenShakeIntensity, 8f);
            
            // Sky flashing
            if (Timer % 15 == 0 && !Main.dedServ && SkyManager.Instance["MagnumOpus:LaCampanellaSky"] is LaCampanellaSkyEffect sky)
            {
                sky.TriggerFlash(0.4f, ThemedParticles.CampanellaOrange);
            }
            
            if (Timer >= AttackDuration)
            {
                State = ActionState.Walking;
                Timer = 0;
                AttackCooldown = 200;
            }
        }
        
        #endregion
        
        #region Enrage and Special Effects
        
        private void TriggerEnrage()
        {
            isEnraged = true;
            enrageTimer = 0;
            
            // Dramatic enrage effects
            SoundEngine.PlaySound(SoundID.Roar with { Pitch = -0.6f, Volume = 1.5f }, NPC.Center);
            Main.NewText("La Campanella's fury intensifies!", 255, 50, 0);
            
            // Massive explosion - FULL VFX SUITE
            ThemedParticles.LaCampanellaImpact(NPC.Center, 4f);
            ThemedParticles.LaCampanellaShockwave(NPC.Center, 3f);
            ThemedParticles.LaCampanellaHaloBurst(NPC.Center, 3f);
            CustomParticles.GenericFlare(NPC.Center, ThemedParticles.CampanellaOrange, 1.5f, 45);
            CustomParticles.GenericFlare(NPC.Center, Color.White, 1.2f, 40);
            CustomParticles.GenericFlare(NPC.Center, ThemedParticles.CampanellaYellow, 1.0f, 35);
            CustomParticles.HaloRing(NPC.Center, Color.Red, 0.8f, 30);
            CustomParticles.HaloRing(NPC.Center, ThemedParticles.CampanellaOrange, 0.6f, 25);
            CustomParticles.ExplosionBurst(NPC.Center, ThemedParticles.CampanellaOrange, 20, 15f);
            CustomParticles.ExplosionBurst(NPC.Center, ThemedParticles.CampanellaYellow, 16, 12f);
            EroicaScreenShake.Phase2EnrageShake(NPC.Center);
            
            // Sky effect
            if (!Main.dedServ && SkyManager.Instance["MagnumOpus:LaCampanellaSky"] is LaCampanellaSkyEffect sky)
            {
                sky.TriggerFlash(1f, Color.Red);
            }
            
            // Boost stats
            NPC.damage = (int)(NPC.damage * 1.2f);
            walkSpeed = 4.5f;
        }
        
        #endregion
        
        private void SpawnAmbientParticles()
        {
            // Rising embers and flames - ENHANCED
            if (Main.rand.NextBool(2))
            {
                Vector2 pos = NPC.Center + Main.rand.NextVector2Circular(NPC.width * 0.7f, NPC.height * 0.6f);
                ThemedParticles.LaCampanellaAura(pos, 40f);
            }
            
            // Occasional sparkles - MORE FREQUENT
            if (Main.rand.NextBool(5))
            {
                ThemedParticles.LaCampanellaSparkles(NPC.Center, 5, NPC.width * 0.6f);
            }
            
            // Black smoke wisps - MORE INTENSE
            if (Main.rand.NextBool(6))
            {
                Vector2 pos = NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height));
                var smoke = new HeavySmokeParticle(pos, new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -2.5f),
                    ThemedParticles.CampanellaBlack, Main.rand.Next(50, 80), Main.rand.NextFloat(0.4f, 0.7f),
                    0.5f, 0.015f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // NEW: Periodic fire flares
            if (Main.rand.NextBool(10))
            {
                Vector2 flarePos = NPC.Center + Main.rand.NextVector2Circular(60f, 80f);
                CustomParticles.GenericFlare(flarePos, ThemedParticles.CampanellaOrange * 0.5f, 0.3f, 15);
            }
            
            // NEW: Bell chime particles when enraged
            if (isEnraged && Main.rand.NextBool(12))
            {
                ThemedParticles.LaCampanellaBellChime(NPC.Center + Main.rand.NextVector2Circular(50f, 50f), 0.4f);
            }
            
            // NEW: Glowing embers rising from bottom
            if (Main.rand.NextBool(4))
            {
                Vector2 emberPos = NPC.Bottom + new Vector2(Main.rand.NextFloat(-NPC.width * 0.4f, NPC.width * 0.4f), 0);
                var ember = new GenericGlowParticle(emberPos, new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-3f, -1.5f)),
                    Main.rand.NextBool() ? ThemedParticles.CampanellaOrange : ThemedParticles.CampanellaYellow,
                    Main.rand.NextFloat(0.2f, 0.4f), Main.rand.Next(30, 50), true);
                MagnumParticleHandler.SpawnParticle(ember);
            }
            
            // NEW: Heat distortion sparkles
            if (distortionIntensity > 0.3f && Main.rand.NextBool(8))
            {
                Vector2 distortPos = NPC.Center + Main.rand.NextVector2Circular(100f, 100f);
                var distortSpark = new GlowSparkParticle(distortPos, Main.rand.NextVector2Circular(2f, 2f) + new Vector2(0, -1f),
                    true, Main.rand.Next(20, 35), Main.rand.NextFloat(0.25f, 0.45f),
                    ThemedParticles.CampanellaYellow, new Vector2(0.4f, 1.3f), true, true);
                MagnumParticleHandler.SpawnParticle(distortSpark);
            }
        }
        
        private void UpdateScreenShake()
        {
            if (screenShakeIntensity > 0.1f)
            {
                Main.LocalPlayer.position += Main.rand.NextVector2Circular(screenShakeIntensity, screenShakeIntensity);
                screenShakeIntensity *= 0.92f;
            }
            else
            {
                screenShakeIntensity = 0f;
            }
        }
        
        private void DeactivateSky()
        {
            if (hasActivatedSky && !Main.dedServ)
            {
                SkyManager.Instance.Deactivate("MagnumOpus:LaCampanellaSky");
                hasActivatedSky = false;
            }
        }
        
        private void UpdateDeathAnimation(Player target)
        {
            deathTimer++;
            
            NPC.velocity.X *= 0.95f;
            NPC.velocity.Y += 0.15f; // Slight gravity during death
            
            // Escalating effects
            float progress = (float)deathTimer / DeathAnimationDuration;
            
            // Increasing shake - MORE INTENSE
            screenShakeIntensity = 8f + progress * 30f;
            
            // Phase 1: Cracking and fire (0-140 ticks)
            if (deathTimer <= 140)
            {
                if (deathTimer % (int)MathHelper.Lerp(12, 4, progress * 2f) == 0)
                {
                    ThemedParticles.LaCampanellaImpact(NPC.Center + Main.rand.NextVector2Circular(100, 100), 1.2f + progress * 2f);
                    SoundEngine.PlaySound(SoundID.Item14 with { Pitch = progress - 0.5f, Volume = 0.6f + progress * 0.5f }, NPC.Center);
                    
                    // Lightning crackles
                    Vector2 lightningEnd = NPC.Center + Main.rand.NextVector2Circular(200f, 200f);
                    MagnumVFX.DrawLaCampanellaLightning(NPC.Center, lightningEnd, 8, 25f, 3, 0.4f);
                }
            }
            
            // Phase 2: Bell tolling and fire geysers (140-280 ticks)
            if (deathTimer > 140 && deathTimer <= 280)
            {
                float phase2Progress = (deathTimer - 140f) / 140f;
                
                if (deathTimer % 25 == 0)
                {
                    SoundEngine.PlaySound(SoundID.Item35 with { Pitch = -0.6f + phase2Progress * 0.3f, Volume = 1.2f }, NPC.Center);
                    ThemedParticles.LaCampanellaShockwave(NPC.Center, 2f + phase2Progress);
                    ThemedParticles.LaCampanellaBellChime(NPC.Center, 1.5f + phase2Progress);
                }
                
                if (deathTimer % 10 == 0)
                {
                    // Fire geysers erupting from boss
                    float geyserAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 geyserDir = geyserAngle.ToRotationVector2();
                    for (int i = 0; i < 5; i++)
                    {
                        var geyser = new GlowSparkParticle(NPC.Center, geyserDir * (10f + i * 3f) + new Vector2(0, -2f),
                            true, Main.rand.Next(40, 60), Main.rand.NextFloat(0.5f, 0.8f),
                            Main.rand.NextBool() ? ThemedParticles.CampanellaOrange : ThemedParticles.CampanellaYellow,
                            new Vector2(0.5f, 1.8f), false, true);
                        MagnumParticleHandler.SpawnParticle(geyser);
                    }
                }
            }
            
            // Phase 3: Final crescendo (280-420 ticks)
            if (deathTimer > 280)
            {
                float phase3Progress = (deathTimer - 280f) / 140f;
                
                // Continuous intense particles
                if (deathTimer % 3 == 0)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        ThemedParticles.LaCampanellaImpact(NPC.Center + Main.rand.NextVector2Circular(80, 80), 0.8f + phase3Progress);
                    }
                }
                
                // Rapid lightning
                if (deathTimer % 5 == 0)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 lightningEnd = NPC.Center + Main.rand.NextVector2Circular(300f, 300f);
                        MagnumVFX.DrawLaCampanellaLightning(NPC.Center, lightningEnd, 10, 35f, 4, 0.5f);
                    }
                }
            }
            
            // Flash sky effect throughout
            if (deathTimer % (int)MathHelper.Lerp(25, 8, progress) == 0)
            {
                if (!Main.dedServ && SkyManager.Instance["MagnumOpus:LaCampanellaSky"] is LaCampanellaSkyEffect sky)
                {
                    sky.TriggerFlash(0.4f + progress * 0.6f, Color.Lerp(ThemedParticles.CampanellaOrange, Color.White, progress));
                }
            }
            
            // Final explosion
            if (deathTimer >= DeathAnimationDuration)
            {
                // MASSIVE final explosion with UnifiedVFX
                SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.8f, Volume = 2f }, NPC.Center);
                SoundEngine.PlaySound(SoundID.Roar with { Pitch = -0.5f, Volume = 1.5f }, NPC.Center);
                
                // UnifiedVFX themed death explosion
                UnifiedVFX.LaCampanella.DeathExplosion(NPC.Center, 2f);
                
                // Radial fire burst with gradient
                for (int i = 0; i < 80; i++)
                {
                    float angle = MathHelper.TwoPi * i / 80f;
                    Vector2 dir = angle.ToRotationVector2();
                    float burstProgress = (float)i / 80f;
                    Color sparkColor = Color.Lerp(UnifiedVFX.LaCampanella.Black, UnifiedVFX.LaCampanella.Orange, burstProgress);
                    ThemedParticles.LaCampanellaSparks(NPC.Center, dir, 10, 18f);
                    CustomParticles.GenericFlare(NPC.Center + dir * 50f, sparkColor, 0.5f, 20);
                }
                
                // Spiral galaxy effect for unique finale
                for (int arm = 0; arm < 8; arm++)
                {
                    float armAngle = MathHelper.TwoPi * arm / 8f;
                    for (int point = 0; point < 10; point++)
                    {
                        float spiralAngle = armAngle + point * 0.35f;
                        float spiralRadius = 30f + point * 20f;
                        Vector2 spiralPos = NPC.Center + spiralAngle.ToRotationVector2() * spiralRadius;
                        float spiralProgress = (arm * 10 + point) / 80f;
                        Color galaxyColor = Color.Lerp(UnifiedVFX.LaCampanella.Orange, UnifiedVFX.LaCampanella.Gold, spiralProgress);
                        CustomParticles.GenericFlare(spiralPos, galaxyColor, 0.6f + point * 0.05f, 30 + point * 2);
                    }
                }
                
                // Multiple impact explosions
                for (int i = 0; i < 12; i++)
                {
                    Vector2 explosionPos = NPC.Center + Main.rand.NextVector2Circular(100f, 100f);
                    UnifiedVFX.LaCampanella.Impact(explosionPos, 2f);
                }
                
                // Giant shockwaves
                for (int i = 0; i < 4; i++)
                {
                    ThemedParticles.LaCampanellaShockwave(NPC.Center, 4f - i * 0.5f);
                }
                
                // Lightning storm with gradient colors
                for (int i = 0; i < 10; i++)
                {
                    float angle = MathHelper.TwoPi * i / 10f;
                    Vector2 lightningEnd = NPC.Center + angle.ToRotationVector2() * 500f;
                    MagnumVFX.DrawLaCampanellaLightning(NPC.Center, lightningEnd, 18, 60f, 7, 0.8f);
                }
                
                // Massive smoke cloud with bell chime
                UnifiedVFX.LaCampanella.BellChime(NPC.Center, 3f);
                for (int i = 0; i < 40; i++)
                {
                    var smoke = new HeavySmokeParticle(NPC.Center + Main.rand.NextVector2Circular(60f, 60f),
                        Main.rand.NextVector2Circular(6f, 6f) + new Vector2(0, -4f),
                        ThemedParticles.CampanellaBlack, Main.rand.Next(100, 150), Main.rand.NextFloat(1f, 1.5f),
                        0.7f, 0.012f, false);
                    MagnumParticleHandler.SpawnParticle(smoke);
                }
                
                // Layered halo cascade
                for (int ring = 0; ring < 8; ring++)
                {
                    float ringProgress = (float)ring / 8f;
                    Color ringColor = Color.Lerp(UnifiedVFX.LaCampanella.Orange, UnifiedVFX.LaCampanella.Gold, ringProgress);
                    CustomParticles.HaloRing(NPC.Center, ringColor * (1f - ringProgress * 0.3f), 0.5f + ring * 0.25f, 25 + ring * 5);
                }
                
                screenShakeIntensity = 60f;
                
                if (!Main.dedServ && SkyManager.Instance["MagnumOpus:LaCampanellaSky"] is LaCampanellaSkyEffect sky)
                {
                    sky.TriggerFlash(1f, Color.White);
                }
                
                DeactivateSky();
                
                NPC.life = 0;
                NPC.HitEffect();
                NPC.checkDead();
            }
        }
        
        public override bool CheckDead()
        {
            if (State != ActionState.Dying && deathTimer < DeathAnimationDuration)
            {
                State = ActionState.Dying;
                deathTimer = 0;
                NPC.life = 1;
                NPC.dontTakeDamage = true;
                NPC.netUpdate = true;
                return false;
            }
            
            return true;
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frame.Y = currentFrame * frameHeight;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            // Energy drops
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<LaCampanellaResonantEnergy>(), 1, 25, 35));
            
            // Weapons - guaranteed 2-3 different weapons
            int[] weaponTypes = new int[]
            {
                ModContent.ItemType<IgnitionOfTheBell>(),
                ModContent.ItemType<InfernalChimesCalling>(),
                ModContent.ItemType<FangOfTheInfiniteBell>(),
                ModContent.ItemType<DualFatedChime>()
            };
            
            foreach (int weaponType in weaponTypes)
            {
                npcLoot.Add(ItemDropRule.Common(weaponType, 3));
            }
            
            // Harmonic Core
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<HarmonicCoreOfLaCampanella>(), 1));
            
            // Expert mode treasure bag
            npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<LaCampanellaTreasureBag>()));
        }

        public override void OnKill()
        {
            DeactivateSky();
            BossHealthBarUI.UnregisterBoss(NPC.whoAmI);
        }

        public override void BossLoot(ref string name, ref int potionType)
        {
            potionType = ItemID.GreaterHealingPotion;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = TextureAssets.Npc[Type].Value;
            Vector2 drawPos = NPC.Center - screenPos;
            Vector2 origin = new Vector2(texture.Width / 2, texture.Height / 2);
            
            // Pulsing glow effect - ENHANCED
            float glowPulse = (float)Math.Sin(auraPulse * 2f) * 0.25f + 0.85f;
            float enragePulse = isEnraged ? (float)Math.Sin(auraPulse * 4f) * 0.15f + 1.1f : 1f;
            
            // Draw outer fire aura when enraged
            if (isEnraged)
            {
                for (int i = 0; i < 4; i++)
                {
                    float auraAngle = MathHelper.TwoPi * i / 4f + auraPulse;
                    Vector2 auraOffset = auraAngle.ToRotationVector2() * (18f + (float)Math.Sin(auraPulse * 5f + i) * 6f);
                    Color auraColor = ThemedParticles.CampanellaYellow * 0.25f * enragePulse;
                    
                    spriteBatch.Draw(texture, drawPos + auraOffset, NPC.frame, auraColor, NPC.rotation, 
                        origin, NPC.scale * 1.1f, NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
                }
            }
            
            // Draw orange glow behind - MORE LAYERS
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f + auraPulse * 0.5f;
                float pulseOffset = 10f + (float)Math.Sin(auraPulse * 3f + i) * 4f;
                Vector2 offset = angle.ToRotationVector2() * pulseOffset;
                Color glowColor = ThemedParticles.CampanellaOrange * 0.35f * glowPulse * enragePulse;
                
                spriteBatch.Draw(texture, drawPos + offset, NPC.frame, glowColor, NPC.rotation, 
                    origin, NPC.scale, NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
            }
            
            // Draw black shadow outline
            for (int i = 0; i < 4; i++)
            {
                float shadowAngle = MathHelper.TwoPi * i / 4f + auraPulse * 0.3f;
                Vector2 shadowOffset = shadowAngle.ToRotationVector2() * 5f;
                Color shadowColor = ThemedParticles.CampanellaBlack * 0.4f;
                
                spriteBatch.Draw(texture, drawPos + shadowOffset, NPC.frame, shadowColor, NPC.rotation, 
                    origin, NPC.scale, NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
            }
            
            // Draw main sprite
            Color mainColor = Color.Lerp(drawColor, Color.White, 0.25f * enragePulse);
            spriteBatch.Draw(texture, drawPos, NPC.frame, mainColor, NPC.rotation, 
                origin, NPC.scale, NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
            
            // Draw inner glow highlight
            Color innerGlow = ThemedParticles.CampanellaYellow * 0.15f * glowPulse;
            spriteBatch.Draw(texture, drawPos, NPC.frame, innerGlow, NPC.rotation, 
                origin, NPC.scale * 0.95f, NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
            
            return false;
        }
    }
}
