using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.EnigmaVariations.Debuffs;
using MagnumOpus.Content.EnigmaVariations.ResonanceEnergies;
using MagnumOpus.Content.EnigmaVariations.HarmonicCores;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons;

namespace MagnumOpus.Content.EnigmaVariations.Bosses
{
    /// <summary>
    /// Enigma, The Hollow Mystery - A nightmarish spider-like entity of the unknown.
    /// Features fast, aggressive ground-based combat with jumps and charges.
    /// Does NOT fly - relies on rapid movement, web attacks, and reality-warping abilities.
    /// Third boss before the final boss in the Enigma theme progression.
    /// </summary>
    public class EnigmaTheHollowMystery : ModNPC
    {
        // Boss texture
        public override string Texture => "MagnumOpus/Content/EnigmaVariations/Bosses/EnigmaTheHollowMystery";
        
        // Enigma theme colors
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        private static readonly Color EnigmaDeepPurple = new Color(80, 20, 120);
        
        // AI States - Spider-like ground combat
        private enum ActionState
        {
            Spawn,
            Idle,
            // Movement
            Crawling,
            JumpWindup,
            Jumping,
            Landing,
            ChargeWindup,
            Charging,
            // Attack 1 - Web Shot: Fast projectiles
            WebShotWindup,
            WebShot,
            // Attack 2 - Paradox Pounce: Rapid leap attack
            PounceWindup,
            Pouncing,
            // Attack 3 - Glyph Eruption: Ground-based AOE
            GlyphEruptionWindup,
            GlyphEruption,
            // Attack 4 - Eye Barrage: Multiple homing eyes
            EyeBarrageWindup,
            EyeBarrage,
            // Attack 5 - Void Web: Creates damaging web patterns
            VoidWebWindup,
            VoidWeb,
            // Attack 6 - Reality Quake: Ground slam with shockwaves
            RealityQuakeWindup,
            RealityQuake,
            // Attack 7 - Enigma Swarm: Spawns mini spiders
            SwarmWindup,
            Swarm,
            // Attack 8 - Mystery Vortex: Pulls player while attacking
            VortexWindup,
            Vortex,
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
        private float crawlDirection = 1f;
        private float crawlSpeed = 6f; // Fast crawler
        private int jumpCooldown = 0;
        private const int JumpCooldownMax = 120; // 2 seconds between jumps
        private int attacksSinceLastJump = 0;
        private Vector2 chargeTarget = Vector2.Zero;
        private float chargeSpeed = 25f;
        
        // Visual effects
        private float auraPulse = 0f;
        private float distortionIntensity = 0f;
        private float eyeGlowIntensity = 0f;
        private bool hasActivatedSky = false;
        
        // Animation - Single PNG (no spritesheet)
        private int frameCounter = 0;
        private int currentFrame = 0;
        private const int TotalFrames = 1;
        private const int FrameColumns = 1;
        private const int FrameRows = 1;
        private const int FrameSpeed = 5;
        
        // Health bar registration
        private bool hasRegisteredHealthBar = false;
        
        // Death animation
        private int deathTimer = 0;
        private const int DeathAnimationDuration = 480; // 8 seconds death
        private float deathFlashIntensity = 0f;
        
        // Attack tracking
        private int consecutiveAttacks = 0;
        private int lastAttackType = -1;
        private bool isEnraged = false;
        private int enrageTimer = 0;
        
        // Pounce tracking
        private Vector2 pounceTarget = Vector2.Zero;
        private int pounceCount = 0;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 1; // Handle 6x6 manually
            
            NPCID.Sets.MPAllowedEnemies[Type] = true;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
            NPCID.Sets.TrailCacheLength[Type] = 10;
            NPCID.Sets.TrailingMode[Type] = 1;
            NPCID.Sets.MustAlwaysDraw[Type] = true;
            
            // Debuff immunities
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Venom] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Webbed] = true;
        }

        public override void SetDefaults()
        {
            NPC.width = 200;
            NPC.height = 140;
            NPC.damage = 160; // High damage for aggressive boss
            NPC.defense = 90;
            NPC.lifeMax = 800000; // 800k HP - Third tier before final
            NPC.HitSound = SoundID.NPCHit8 with { Pitch = -0.4f };
            NPC.DeathSound = SoundID.NPCDeath10;
            NPC.knockBackResist = 0f;
            NPC.noGravity = false; // Ground-based!
            NPC.noTileCollide = false; // Collides with tiles!
            NPC.value = Item.buyPrice(gold: 35);
            NPC.boss = true;
            NPC.npcSlots = 16f;
            NPC.aiStyle = -1;
            NPC.scale = 1.1f;
            
            // Visual offset - raised significantly to prevent sinking into ground
            DrawOffsetY = -130f;
            
            Music = MusicID.Boss3; // Fallback until custom music
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Underground,
                new FlavorTextBestiaryInfoElement("Enigma, The Hollow Mystery - " +
                    "A spider-like abomination that embodies the unknowable void. " +
                    "Its many eyes see through dimensions, and its legs pierce reality itself.")
            });
        }

        public override void AI()
        {
            // Register with health bar system
            if (!hasRegisteredHealthBar)
            {
                BossHealthBarUI.RegisterBoss(NPC, BossColorTheme.Enigma);
                hasRegisteredHealthBar = true;
            }
            
            NPC.TargetClosest(true);
            Player target = Main.player[NPC.target];
            
            // Ground check
            CheckGrounded();
            
            // Handle dying
            if (State == ActionState.Dying)
            {
                UpdateDeathAnimation(target);
                return;
            }
            
            // Despawn check
            if (!target.active || target.dead)
            {
                NPC.velocity.Y += 0.3f;
                NPC.EncourageDespawn(60);
                return;
            }
            
            // Update timers
            Timer++;
            auraPulse += 0.05f;
            jumpCooldown = Math.Max(0, jumpCooldown - 1);
            AttackCooldown = Math.Max(0, AttackCooldown - 1);
            
            // Enrage at 30% health
            if (!isEnraged && NPC.life < NPC.lifeMax * 0.3f)
            {
                isEnraged = true;
                EnrageTransition();
            }
            
            // Update enrage effects
            if (isEnraged)
            {
                enrageTimer++;
                crawlSpeed = 9f;
                chargeSpeed = 35f;
            }
            
            // Eye glow intensity
            eyeGlowIntensity = 0.6f + (float)Math.Sin(auraPulse * 2f) * 0.4f;
            if (isEnraged) eyeGlowIntensity *= 1.5f;
            
            // Ambient lighting
            float pulse = 0.5f + (float)Math.Sin(auraPulse) * 0.3f;
            Lighting.AddLight(NPC.Center, EnigmaPurple.ToVector3() * pulse);
            Lighting.AddLight(NPC.Center + new Vector2(0, -30), EnigmaGreen.ToVector3() * eyeGlowIntensity * 0.5f);
            
            // Spawn ambient particles
            SpawnAmbientParticles();
            
            // State machine
            switch (State)
            {
                case ActionState.Spawn:
                    SpawnSequence(target);
                    break;
                case ActionState.Idle:
                    IdleBehavior(target);
                    break;
                case ActionState.Crawling:
                    CrawlingBehavior(target);
                    break;
                case ActionState.JumpWindup:
                    JumpWindupBehavior(target);
                    break;
                case ActionState.Jumping:
                    JumpingBehavior(target);
                    break;
                case ActionState.Landing:
                    LandingBehavior(target);
                    break;
                case ActionState.ChargeWindup:
                    ChargeWindupBehavior(target);
                    break;
                case ActionState.Charging:
                    ChargingBehavior(target);
                    break;
                    
                // Attacks
                case ActionState.WebShotWindup:
                    WebShotWindup(target);
                    break;
                case ActionState.WebShot:
                    WebShotAttack(target);
                    break;
                case ActionState.PounceWindup:
                    PounceWindup(target);
                    break;
                case ActionState.Pouncing:
                    PounceAttack(target);
                    break;
                case ActionState.GlyphEruptionWindup:
                    GlyphEruptionWindup(target);
                    break;
                case ActionState.GlyphEruption:
                    GlyphEruptionAttack(target);
                    break;
                case ActionState.EyeBarrageWindup:
                    EyeBarrageWindup(target);
                    break;
                case ActionState.EyeBarrage:
                    EyeBarrageAttack(target);
                    break;
                case ActionState.VoidWebWindup:
                    VoidWebWindup(target);
                    break;
                case ActionState.VoidWeb:
                    VoidWebAttack(target);
                    break;
                case ActionState.RealityQuakeWindup:
                    RealityQuakeWindup(target);
                    break;
                case ActionState.RealityQuake:
                    RealityQuakeAttack(target);
                    break;
                case ActionState.SwarmWindup:
                    SwarmWindup(target);
                    break;
                case ActionState.Swarm:
                    SwarmAttack(target);
                    break;
                case ActionState.VortexWindup:
                    VortexWindup(target);
                    break;
                case ActionState.Vortex:
                    VortexAttack(target);
                    break;
            }
            
            // Face target during movement
            if (State == ActionState.Crawling || State == ActionState.Idle)
            {
                NPC.spriteDirection = NPC.direction = (target.Center.X > NPC.Center.X) ? 1 : -1;
            }
        }
        
        private void CheckGrounded()
        {
            Vector2 bottomLeft = new Vector2(NPC.position.X + 20, NPC.position.Y + NPC.height + 4);
            Vector2 bottomRight = new Vector2(NPC.position.X + NPC.width - 20, NPC.position.Y + NPC.height + 4);
            
            Point tileLeft = bottomLeft.ToTileCoordinates();
            Point tileRight = bottomRight.ToTileCoordinates();
            
            bool leftSolid = WorldGen.SolidTile(tileLeft.X, tileLeft.Y);
            bool rightSolid = WorldGen.SolidTile(tileRight.X, tileRight.Y);
            
            isGrounded = (leftSolid || rightSolid) && NPC.velocity.Y >= 0;
        }
        
        private void SpawnAmbientParticles()
        {
            // Void particles
            if (Main.rand.NextBool(8))
            {
                Vector2 particlePos = NPC.Center + Main.rand.NextVector2Circular(100, 60);
                Color particleColor = Main.rand.NextBool() ? EnigmaPurple : EnigmaGreen;
                CustomParticles.GenericGlow(particlePos, particleColor * 0.6f, 0.3f, 30);
            }
            
            // Watching eye particles around boss
            if (Main.rand.NextBool(25))
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 eyePos = NPC.Center + angle.ToRotationVector2() * Main.rand.NextFloat(60, 120);
                CustomParticles.EnigmaEyeGaze(eyePos, EnigmaGreen * 0.7f, 0.25f, (-angle).ToRotationVector2());
            }
            
            // Glyph particles
            if (Main.rand.NextBool(30))
            {
                CustomParticles.Glyph(NPC.Center + Main.rand.NextVector2Circular(80, 50), EnigmaPurple * 0.5f, 0.25f);
            }
            
            // Leg trail particles during movement
            if ((State == ActionState.Crawling || State == ActionState.Charging) && Main.rand.NextBool(3))
            {
                for (int leg = 0; leg < 4; leg++)
                {
                    float legAngle = MathHelper.PiOver4 + leg * MathHelper.PiOver2;
                    Vector2 legPos = NPC.Center + new Vector2((leg < 2 ? -1 : 1) * 80, 40);
                    Color legColor = Color.Lerp(EnigmaBlack, EnigmaPurple, 0.3f);
                    var smoke = new HeavySmokeParticle(legPos, Vector2.UnitY * -1f, legColor, Main.rand.Next(15, 25), 0.2f, 0.4f, 0.02f, false);
                    MagnumParticleHandler.SpawnParticle(smoke);
                }
            }
        }
        
        private void EnrageTransition()
        {
            // Dramatic enrage announcement
            Main.NewText("The Hollow Mystery reveals its true form!", EnigmaGreen);
            SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
            
            // Screen shake for boss
            EroicaScreenShake.LargeShake(NPC.Center);
            
            // Massive particle burst
            UnifiedVFX.EnigmaVariations.Explosion(NPC.Center, 2.5f);
            
            // Eye explosion
            CustomParticles.EnigmaEyeExplosion(NPC.Center, EnigmaGreen, 12, 8f);
            CustomParticles.GlyphBurst(NPC.Center, EnigmaPurple, 10, 6f);
            
            // Reality distortion
            FateRealityDistortion.TriggerChromaticAberration(NPC.Center, 8f, 30);
        }
        
        #region Movement States
        
        private void SpawnSequence(Player target)
        {
            if (Timer == 1)
            {
                Main.NewText("Something watches from the shadows...", EnigmaPurple);
                SoundEngine.PlaySound(SoundID.Zombie105 with { Pitch = -0.5f, Volume = 1.2f }, NPC.Center);
                
                // Spawn VFX
                CustomParticles.EnigmaEyeFormation(NPC.Center, EnigmaGreen, 8, 100f);
                UnifiedVFX.EnigmaVariations.Impact(NPC.Center, 1.5f);
            }
            
            // Build-up particles
            if (Timer % 8 == 0)
            {
                ThemedParticles.EnigmaImpact(NPC.Center + Main.rand.NextVector2Circular(60, 40), 1f);
            }
            
            NPC.velocity.X = 0;
            
            if (Timer >= 90)
            {
                Main.NewText("Enigma, The Hollow Mystery emerges!", EnigmaGreen);
                EroicaScreenShake.MediumShake(NPC.Center);
                State = ActionState.Crawling;
                Timer = 0;
            }
        }
        
        private void IdleBehavior(Player target)
        {
            // Brief idle before selecting next action - AGGRESSIVE, minimal wait
            if (isGrounded)
                NPC.velocity.X *= 0.9f;
            
            // Enraged = immediate action, Normal = quick transition
            if (Timer >= (isEnraged ? 10 : 20))
            {
                SelectNextAction(target);
            }
        }
        
        private void CrawlingBehavior(Player target)
        {
            float direction = Math.Sign(target.Center.X - NPC.Center.X);
            crawlDirection = direction;
            
            if (isGrounded)
            {
                // Faster crawl speed for more aggression
                float currentCrawlSpeed = isEnraged ? crawlSpeed * 1.3f : crawlSpeed;
                NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, direction * currentCrawlSpeed, 0.15f);
            }
            
            // Check for jump opportunity
            float heightDiff = target.Center.Y - NPC.Center.Y;
            float distanceX = Math.Abs(target.Center.X - NPC.Center.X);
            
            // Jump if player is above or far away - more aggressive thresholds
            if (jumpCooldown <= 0 && isGrounded && (heightDiff < -100 || distanceX > 400))
            {
                State = ActionState.JumpWindup;
                Timer = 0;
                return;
            }
            
            // Select attack much sooner - AGGRESSIVE boss
            int attackWindow = isEnraged ? 40 : 60;
            if (Timer >= attackWindow && AttackCooldown <= 0)
            {
                SelectNextAction(target);
            }
        }
        
        private void JumpWindupBehavior(Player target)
        {
            // Crouch before jump
            NPC.velocity.X *= 0.95f;
            
            // Build-up particles
            if (Timer % 5 == 0)
            {
                CustomParticles.GenericFlare(NPC.Bottom, EnigmaPurple, 0.4f + Timer * 0.01f, 15);
                CustomParticles.GlyphBurst(NPC.Bottom, EnigmaGreen, 2, 2f);
            }
            
            if (Timer >= 25)
            {
                // Execute jump
                Vector2 toTarget = target.Center - NPC.Center;
                float jumpPower = MathHelper.Clamp(Math.Abs(toTarget.Y) * 0.02f + 15f, 15f, 28f);
                float horizontalPower = MathHelper.Clamp(toTarget.X * 0.015f, -12f, 12f);
                
                NPC.velocity = new Vector2(horizontalPower, -jumpPower);
                
                // Jump VFX
                EroicaScreenShake.SmallShake(NPC.Center);
                UnifiedVFX.EnigmaVariations.Impact(NPC.Bottom, 1f);
                SoundEngine.PlaySound(SoundID.Item24 with { Pitch = -0.3f }, NPC.Center);
                
                jumpCooldown = JumpCooldownMax;
                State = ActionState.Jumping;
                Timer = 0;
            }
        }
        
        private void JumpingBehavior(Player target)
        {
            // Air control
            float direction = Math.Sign(target.Center.X - NPC.Center.X);
            NPC.velocity.X += direction * 0.2f;
            NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -15f, 15f);
            
            // Jump trail
            if (Timer % 3 == 0)
            {
                Color trailColor = Color.Lerp(EnigmaPurple, EnigmaGreen, Main.rand.NextFloat());
                CustomParticles.GenericGlow(NPC.Center, trailColor * 0.6f, 0.4f, 20);
            }
            
            // Check for landing
            if (isGrounded && Timer > 10)
            {
                State = ActionState.Landing;
                Timer = 0;
            }
        }
        
        private void LandingBehavior(Player target)
        {
            NPC.velocity.X *= 0.85f;
            
            if (Timer == 1)
            {
                // Landing impact
                EroicaScreenShake.SmallShake(NPC.Center);
                UnifiedVFX.EnigmaVariations.Impact(NPC.Bottom, 0.8f);
                SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.2f, Volume = 0.7f }, NPC.Center);
                
                // Ground crack particles
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.Pi + MathHelper.PiOver4 * (i - 3.5f) * 0.3f;
                    Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                    CustomParticles.GenericGlow(NPC.Bottom + vel, EnigmaPurple, 0.3f, 25);
                }
            }
            
            if (Timer >= 15)
            {
                attacksSinceLastJump = 0;
                State = ActionState.Crawling;
                Timer = 0;
            }
        }
        
        private void ChargeWindupBehavior(Player target)
        {
            NPC.velocity.X *= 0.9f;
            
            // Update charge target to track player - BETTER TRACKING
            chargeTarget = target.Center;
            
            // Charge telegraph - FASTER
            if (Timer % 3 == 0)
            {
                Vector2 telegraphDir = (chargeTarget - NPC.Center).SafeNormalize(Vector2.UnitX);
                for (int i = 0; i < 4; i++)
                {
                    Vector2 pos = NPC.Center + telegraphDir * (i * 35);
                    CustomParticles.GenericFlare(pos, EnigmaGreen * 0.7f, 0.35f, 12);
                }
                // Add eye telegraph
                CustomParticles.EnigmaEyeGaze(NPC.Center + telegraphDir * 60f, EnigmaGreen, 0.5f, telegraphDir);
            }
            
            // SHORTER windup
            if (Timer >= 20)
            {
                // Execute charge - update target one more time
                chargeTarget = target.Center;
                Vector2 chargeDir = (chargeTarget - NPC.Center).SafeNormalize(Vector2.UnitX);
                float currentChargeSpeed = isEnraged ? chargeSpeed * 1.4f : chargeSpeed;
                NPC.velocity = chargeDir * currentChargeSpeed;
                
                SoundEngine.PlaySound(SoundID.Roar with { Pitch = 0.5f, Volume = 0.8f }, NPC.Center);
                State = ActionState.Charging;
                Timer = 0;
            }
        }
        
        private void ChargingBehavior(Player target)
        {
            // Charge trail - MORE particles
            if (Timer % 2 == 0)
            {
                CustomParticles.GenericFlare(NPC.Center, EnigmaPurple, 0.6f, 15);
                CustomParticles.GenericGlow(NPC.Center, EnigmaGreen, 0.5f, 20);
                CustomParticles.EnigmaEyeGaze(NPC.Center + Main.rand.NextVector2Circular(30, 30), 
                    EnigmaPurple * 0.7f, 0.3f, NPC.velocity.SafeNormalize(Vector2.UnitX));
            }
            
            // Light course correction during charge - BETTER TRACKING
            if (Timer % 8 == 0 && isEnraged)
            {
                Vector2 toTarget = (target.Center - NPC.Center).SafeNormalize(NPC.velocity.SafeNormalize(Vector2.UnitX));
                NPC.velocity = Vector2.Lerp(NPC.velocity.SafeNormalize(Vector2.UnitX), toTarget, 0.15f) * NPC.velocity.Length();
            }
            
            // LONGER charge duration
            if (Timer >= 55 || NPC.velocity.Length() < 4f)
            {
                NPC.velocity *= 0.3f;
                State = ActionState.Crawling;
                Timer = 0;
                AttackCooldown = isEnraged ? 20 : 30;
            }
        }
        
        #endregion
        
        #region Attack Selection
        
        private void SelectNextAction(Player target)
        {
            attacksSinceLastJump++;
            
            // Force jump more frequently for mobility - AGGRESSIVE
            if (attacksSinceLastJump >= 2 && jumpCooldown <= 0 && isGrounded)
            {
                State = ActionState.JumpWindup;
                Timer = 0;
                attacksSinceLastJump = 0;
                return;
            }
            
            // Weighted attack selection based on distance and health
            float distance = Vector2.Distance(NPC.Center, target.Center);
            float healthPercent = (float)NPC.life / NPC.lifeMax;
            
            List<ActionState> availableAttacks = new List<ActionState>();
            
            // Close range attacks - PRIORITIZE these when close
            if (distance < 350)
            {
                availableAttacks.Add(ActionState.PounceWindup);
                availableAttacks.Add(ActionState.PounceWindup); // Double weight for pounce
                availableAttacks.Add(ActionState.RealityQuakeWindup);
                availableAttacks.Add(ActionState.GlyphEruptionWindup);
                availableAttacks.Add(ActionState.ChargeWindup); // Can charge at close range too
                chargeTarget = target.Center;
            }
            
            // Medium range - AGGRESSIVE mix of approaches
            if (distance >= 150 && distance < 600)
            {
                availableAttacks.Add(ActionState.WebShotWindup);
                availableAttacks.Add(ActionState.EyeBarrageWindup);
                availableAttacks.Add(ActionState.ChargeWindup);
                availableAttacks.Add(ActionState.ChargeWindup); // Double weight for charge
                chargeTarget = target.Center;
            }
            
            // Long range - CLOSE THE GAP AGGRESSIVELY
            if (distance >= 400)
            {
                availableAttacks.Add(ActionState.VoidWebWindup);
                availableAttacks.Add(ActionState.SwarmWindup);
                availableAttacks.Add(ActionState.ChargeWindup); // Always include charge at range
                availableAttacks.Add(ActionState.ChargeWindup);
                chargeTarget = target.Center;
            }
            
            // Enrage dramatically increases vortex frequency
            if (isEnraged)
            {
                availableAttacks.Add(ActionState.VortexWindup);
                if (Main.rand.NextBool(2))
                    availableAttacks.Add(ActionState.VortexWindup); // More vortex when enraged
            }
            
            // LOW HEALTH PHASE (below 50%) - DESPERATE AGGRESSION
            if (healthPercent < 0.5f)
            {
                // Add more charge and pounce attacks
                availableAttacks.Add(ActionState.PounceWindup);
                availableAttacks.Add(ActionState.ChargeWindup);
                chargeTarget = target.Center;
            }
            
            // CRITICAL PHASE (below 25%) - MAXIMUM AGGRESSION
            if (healthPercent < 0.25f)
            {
                availableAttacks.Add(ActionState.VortexWindup);
                availableAttacks.Add(ActionState.RealityQuakeWindup);
                availableAttacks.Add(ActionState.GlyphEruptionWindup);
            }
            
            // Default if no attacks available
            if (availableAttacks.Count == 0)
            {
                availableAttacks.Add(ActionState.WebShotWindup);
                availableAttacks.Add(ActionState.EyeBarrageWindup);
                availableAttacks.Add(ActionState.ChargeWindup);
                chargeTarget = target.Center;
            }
            
            // Pick random attack, avoiding repeats
            ActionState selectedAttack;
            int attempts = 0;
            do
            {
                selectedAttack = availableAttacks[Main.rand.Next(availableAttacks.Count)];
                attempts++;
            } while ((int)selectedAttack == lastAttackType && attempts < 5 && availableAttacks.Count > 1);
            
            lastAttackType = (int)selectedAttack;
            State = selectedAttack;
            Timer = 0;
            
            // REDUCED COOLDOWNS for aggressive pacing
            // Enraged: 15 ticks (0.25 seconds)
            // Critical (below 25%): 10 ticks
            // Normal: 35 ticks (0.58 seconds)
            if (healthPercent < 0.25f)
                AttackCooldown = 10;
            else if (isEnraged)
                AttackCooldown = 15;
            else
                AttackCooldown = 35;
        }
        
        #endregion
        
        #region Attacks
        
        // Attack 1: Web Shot - Fast projectiles
        private void WebShotWindup(Player target)
        {
            NPC.velocity.X *= 0.9f;
            
            if (Timer % 5 == 0)
            {
                CustomParticles.GenericFlare(NPC.Center, EnigmaPurple, 0.4f + Timer * 0.015f, 15);
            }
            
            // FASTER windup
            if (Timer >= 18)
            {
                State = ActionState.WebShot;
                Timer = 0;
                AttackPhase = 0;
            }
        }
        
        private void WebShotAttack(Player target)
        {
            // MORE shots, FASTER rate
            int shotsPerBurst = isEnraged ? 7 : 5;
            int shotDelay = isEnraged ? 5 : 8;
            
            if (Timer % shotDelay == 0 && AttackPhase < shotsPerBurst)
            {
                Vector2 shootDir = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
                Vector2 spread = shootDir.RotatedBy(Main.rand.NextFloat(-0.15f, 0.15f));
                
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, spread * 14f,
                        ModContent.ProjectileType<EnigmaWebShot>(), NPC.damage / 2, 3f, Main.myPlayer);
                }
                
                // Shot VFX
                CustomParticles.GenericFlare(NPC.Center, EnigmaGreen, 0.6f, 15);
                SoundEngine.PlaySound(SoundID.Item17 with { Pitch = 0.3f }, NPC.Center);
                
                AttackPhase++;
            }
            
            if (AttackPhase >= shotsPerBurst)
            {
                State = ActionState.Crawling;
                Timer = 0;
            }
        }
        
        // Attack 2: Paradox Pounce - Rapid leap attacks
        private void PounceWindup(Player target)
        {
            NPC.velocity.X *= 0.85f;
            
            pounceTarget = target.Center;
            pounceCount = 0;
            
            // Telegraph - FASTER
            if (Timer % 4 == 0)
            {
                Vector2 telegraphPos = NPC.Center + (pounceTarget - NPC.Center).SafeNormalize(Vector2.Zero) * 50f;
                CustomParticles.EnigmaEyeGaze(telegraphPos, EnigmaGreen, 0.4f, (pounceTarget - NPC.Center).SafeNormalize(Vector2.UnitX));
            }
            
            // SHORTER windup
            if (Timer >= 15)
            {
                State = ActionState.Pouncing;
                Timer = 0;
            }
        }
        
        private void PounceAttack(Player target)
        {
            // MORE pounces, FASTER speed
            int maxPounces = isEnraged ? 5 : 4;
            
            if (Timer == 1 && pounceCount < maxPounces)
            {
                // Execute pounce - MORE AGGRESSIVE
                Vector2 toPounce = (target.Center - NPC.Center);
                float pounceSpeed = isEnraged ? 28f : 22f;
                
                // Arc trajectory - better tracking
                NPC.velocity.X = toPounce.X * 0.06f;
                NPC.velocity.Y = -14f;
                NPC.velocity = NPC.velocity.SafeNormalize(Vector2.UnitX) * pounceSpeed;
                NPC.velocity.Y = Math.Min(NPC.velocity.Y, -10f);
                
                SoundEngine.PlaySound(SoundID.Item24 with { Pitch = 0.2f }, NPC.Center);
                pounceCount++;
            }
            
            // Trail
            if (Timer % 2 == 0)
            {
                CustomParticles.GenericFlare(NPC.Center, EnigmaPurple, 0.4f, 12);
            }
            
            // Check landing for next pounce
            if (isGrounded && Timer > 15)
            {
                // Landing impact
                EroicaScreenShake.SmallShake(NPC.Center);
                UnifiedVFX.EnigmaVariations.Impact(NPC.Bottom, 0.7f);
                
                if (pounceCount < maxPounces)
                {
                    Timer = 0; // Reset for next pounce
                }
                else
                {
                    State = ActionState.Crawling;
                    Timer = 0;
                }
            }
            
            // Timeout
            if (Timer > 90)
            {
                State = ActionState.Crawling;
                Timer = 0;
            }
        }
        
        // Attack 3: Glyph Eruption - Ground AOE - MORE DANGEROUS
        private void GlyphEruptionWindup(Player target)
        {
            NPC.velocity.X *= 0.9f;
            
            // Ground glyph telegraph - FASTER
            if (Timer % 6 == 0)
            {
                float radius = 150f + Timer * 3f;
                CustomParticles.GlyphCircle(NPC.Bottom, EnigmaPurple * 0.6f, 6, radius, 0.07f);
            }
            
            // SHORTER windup
            if (Timer >= 30)
            {
                State = ActionState.GlyphEruption;
                Timer = 0;
            }
        }
        
        private void GlyphEruptionAttack(Player target)
        {
            if (Timer == 1)
            {
                // Ground eruption - MORE WAVES, MORE PROJECTILES
                EroicaScreenShake.MediumShake(NPC.Center);
                
                int waves = isEnraged ? 6 : 4;
                for (int wave = 0; wave < waves; wave++)
                {
                    float radius = 80f + wave * 70f;
                    int projectiles = 10 + wave * 3;
                    
                    for (int i = 0; i < projectiles; i++)
                    {
                        float angle = MathHelper.TwoPi * i / projectiles;
                        Vector2 pos = NPC.Bottom + angle.ToRotationVector2() * radius;
                        // Faster projectiles, some aimed at player
                        Vector2 vel = new Vector2(0, -10f - wave * 2.5f);
                        if (wave % 2 == 1) // Every other wave aims at player
                        {
                            vel = (target.Center - pos).SafeNormalize(Vector2.UnitY * -1) * (8f + wave * 2f);
                        }
                        
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), pos, vel,
                                ModContent.ProjectileType<EnigmaGlyphProjectile>(), NPC.damage / 3, 2f, Main.myPlayer);
                        }
                    }
                }
                
                // VFX
                UnifiedVFX.EnigmaVariations.Explosion(NPC.Bottom, 1.8f);
                CustomParticles.GlyphBurst(NPC.Bottom, EnigmaGreen, 20, 10f);
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.2f }, NPC.Center);
            }
            
            // SHORTER recovery
            if (Timer >= 20)
            {
                State = ActionState.Crawling;
                Timer = 0;
            }
        }
        
        // Attack 4: Eye Barrage - Multiple homing eyes
        private void EyeBarrageWindup(Player target)
        {
            NPC.velocity.X *= 0.9f;
            
            // Eyes gathering
            if (Timer % 8 == 0)
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 eyePos = NPC.Center + angle.ToRotationVector2() * 100f;
                CustomParticles.EnigmaEyeGaze(eyePos, EnigmaGreen, 0.3f + Timer * 0.005f, (NPC.Center - eyePos).SafeNormalize(Vector2.UnitX));
            }
            
            if (Timer >= 40)
            {
                State = ActionState.EyeBarrage;
                Timer = 0;
                AttackPhase = 0;
            }
        }
        
        private void EyeBarrageAttack(Player target)
        {
            int totalEyes = isEnraged ? 8 : 5;
            int eyeDelay = isEnraged ? 6 : 10;
            
            if (Timer % eyeDelay == 0 && AttackPhase < totalEyes)
            {
                float angle = MathHelper.TwoPi * AttackPhase / totalEyes + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 spawnPos = NPC.Center + angle.ToRotationVector2() * 60f;
                Vector2 vel = (target.Center - spawnPos).SafeNormalize(Vector2.UnitX) * 8f;
                
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPos, vel,
                        ModContent.ProjectileType<EnigmaHomingEye>(), NPC.damage / 3, 2f, Main.myPlayer, target.whoAmI);
                }
                
                // Spawn VFX
                CustomParticles.EnigmaEyeGaze(spawnPos, EnigmaGreen, 0.5f, vel.SafeNormalize(Vector2.UnitX));
                SoundEngine.PlaySound(SoundID.Item8 with { Pitch = 0.4f, Volume = 0.6f }, spawnPos);
                
                AttackPhase++;
            }
            
            if (AttackPhase >= totalEyes && Timer > totalEyes * eyeDelay + 20)
            {
                State = ActionState.Crawling;
                Timer = 0;
            }
        }
        
        // Attack 5: Void Web - Creates damaging web pattern
        private void VoidWebWindup(Player target)
        {
            NPC.velocity.X *= 0.9f;
            
            if (Timer % 15 == 0)
            {
                CustomParticles.GlyphCircle(NPC.Center, EnigmaPurple, 4, 80f + Timer, 0.08f);
            }
            
            if (Timer >= 45)
            {
                State = ActionState.VoidWeb;
                Timer = 0;
            }
        }
        
        private void VoidWebAttack(Player target)
        {
            if (Timer == 1)
            {
                // Create web pattern projectiles
                int webLines = isEnraged ? 12 : 8;
                float webRadius = 400f;
                
                for (int i = 0; i < webLines; i++)
                {
                    float angle = MathHelper.TwoPi * i / webLines;
                    Vector2 endPos = NPC.Center + angle.ToRotationVector2() * webRadius;
                    
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero,
                            ModContent.ProjectileType<EnigmaVoidWeb>(), NPC.damage / 4, 0f, Main.myPlayer, 
                            angle, webRadius);
                    }
                }
                
                // VFX
                UnifiedVFX.EnigmaVariations.Impact(NPC.Center, 1.2f);
                SoundEngine.PlaySound(SoundID.Item17 with { Pitch = -0.3f }, NPC.Center);
            }
            
            if (Timer >= 60)
            {
                State = ActionState.Crawling;
                Timer = 0;
            }
        }
        
        // Attack 6: Reality Quake - Ground slam
        private void RealityQuakeWindup(Player target)
        {
            NPC.velocity.X *= 0.85f;
            
            // Charging particles
            float chargeProgress = Timer / 40f;
            if (Timer % 5 == 0)
            {
                CustomParticles.GenericFlare(NPC.Bottom, EnigmaGreen, 0.3f + chargeProgress * 0.5f, 15);
                CustomParticles.GlyphBurst(NPC.Bottom, EnigmaPurple, 2, 1f + chargeProgress * 2f);
            }
            
            if (Timer >= 40)
            {
                State = ActionState.RealityQuake;
                Timer = 0;
            }
        }
        
        private void RealityQuakeAttack(Player target)
        {
            if (Timer == 1)
            {
                // Ground slam!
                EroicaScreenShake.LargeShake(NPC.Center);
                
                // Shockwaves
                int waves = isEnraged ? 5 : 3;
                for (int wave = 0; wave < waves; wave++)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        // Left shockwave
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Bottom - new Vector2(20, 0), new Vector2(-10f - wave * 2f, 0),
                            ModContent.ProjectileType<EnigmaShockwave>(), NPC.damage / 2, 5f, Main.myPlayer, wave * 8);
                        // Right shockwave
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Bottom + new Vector2(20, 0), new Vector2(10f + wave * 2f, 0),
                            ModContent.ProjectileType<EnigmaShockwave>(), NPC.damage / 2, 5f, Main.myPlayer, wave * 8);
                    }
                }
                
                // Massive impact VFX
                UnifiedVFX.EnigmaVariations.Explosion(NPC.Bottom, 2f);
                CustomParticles.EnigmaEyeExplosion(NPC.Center, EnigmaGreen, 6, 5f);
                FateRealityDistortion.TriggerInversionPulse(5);
                
                SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.5f, Volume = 1.2f }, NPC.Center);
            }
            
            if (Timer >= 40)
            {
                State = ActionState.Crawling;
                Timer = 0;
            }
        }
        
        // Attack 7: Enigma Swarm - Spawn mini spiders
        private void SwarmWindup(Player target)
        {
            NPC.velocity.X *= 0.9f;
            
            if (Timer % 10 == 0)
            {
                CustomParticles.EnigmaEyeFormation(NPC.Center, EnigmaPurple * 0.5f, 3, 50f + Timer);
            }
            
            if (Timer >= 35)
            {
                State = ActionState.Swarm;
                Timer = 0;
            }
        }
        
        private void SwarmAttack(Player target)
        {
            if (Timer == 1)
            {
                int spawnCount = isEnraged ? 6 : 4;
                
                for (int i = 0; i < spawnCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / spawnCount;
                    Vector2 spawnPos = NPC.Center + angle.ToRotationVector2() * 80f;
                    
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        int npcIndex = NPC.NewNPC(NPC.GetSource_FromAI(), (int)spawnPos.X, (int)spawnPos.Y,
                            ModContent.NPCType<EnigmaMiniSpider>(), 0, NPC.whoAmI);
                    }
                    
                    // Spawn VFX
                    UnifiedVFX.EnigmaVariations.Impact(spawnPos, 0.6f);
                }
                
                SoundEngine.PlaySound(SoundID.Zombie103 with { Pitch = 0.3f }, NPC.Center);
            }
            
            if (Timer >= 30)
            {
                State = ActionState.Crawling;
                Timer = 0;
            }
        }
        
        // Attack 8: Mystery Vortex - Pull player while attacking
        private void VortexWindup(Player target)
        {
            NPC.velocity.X *= 0.85f;
            
            // Vortex building - KEEP SPINNING throughout windup
            float chargeProgress = Timer / 50f;
            
            // Continuous spinning vortex effect
            if (Timer % 3 == 0)
            {
                float innerRadius = 60f - chargeProgress * 20f;
                float outerRadius = 100f - chargeProgress * 30f;
                
                // Inner ring - spins faster
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi * i / 6f + Timer * 0.15f;
                    Vector2 pos = NPC.Center + angle.ToRotationVector2() * innerRadius;
                    Color innerColor = Color.Lerp(EnigmaPurple, EnigmaGreen, chargeProgress);
                    CustomParticles.GenericFlare(pos, innerColor, 0.35f + chargeProgress * 0.2f, 12);
                }
                
                // Outer ring - spins opposite direction
                for (int i = 0; i < 4; i++)
                {
                    float angle = MathHelper.TwoPi * i / 4f - Timer * 0.1f;
                    Vector2 pos = NPC.Center + angle.ToRotationVector2() * outerRadius;
                    CustomParticles.GenericFlare(pos, EnigmaPurple, 0.4f, 15);
                }
            }
            
            // Glyph circle spins during charge
            if (Timer % 10 == 0)
            {
                CustomParticles.GlyphCircle(NPC.Center, EnigmaPurple * (0.5f + chargeProgress * 0.5f), 4, 80f - chargeProgress * 30f, 0.1f + chargeProgress * 0.1f);
            }
            
            // Central pulsing core
            if (Timer % 6 == 0)
            {
                CustomParticles.GenericFlare(NPC.Center, EnigmaGreen * chargeProgress, 0.5f + chargeProgress * 0.3f, 15);
            }
            
            if (Timer >= 50)
            {
                State = ActionState.Vortex;
                Timer = 0;
            }
        }
        
        private void VortexAttack(Player target)
        {
            NPC.velocity.X *= 0.95f;
            
            // Pull player toward boss
            float pullStrength = 0.5f + (isEnraged ? 0.3f : 0f);
            Vector2 pullDir = (NPC.Center - target.Center).SafeNormalize(Vector2.Zero);
            target.velocity += pullDir * pullStrength;
            
            // Vortex visuals - CONTINUOUS SPINNING throughout attack
            // Multi-layer spinning vortex
            if (Timer % 2 == 0)
            {
                // Inner layer - fast spin
                for (int i = 0; i < 4; i++)
                {
                    float innerAngle = Timer * 0.2f + MathHelper.TwoPi * i / 4f;
                    float innerRadius = 40f + (float)Math.Sin(Timer * 0.1f) * 10f;
                    Vector2 innerPos = NPC.Center + innerAngle.ToRotationVector2() * innerRadius;
                    Color innerColor = Color.Lerp(EnigmaGreen, EnigmaPurple, (float)i / 4f);
                    CustomParticles.GenericFlare(innerPos, innerColor, 0.4f, 10);
                }
                
                // Middle layer - medium spin opposite direction
                for (int i = 0; i < 6; i++)
                {
                    float midAngle = -Timer * 0.12f + MathHelper.TwoPi * i / 6f;
                    float midRadius = 80f + (float)Math.Sin(Timer * 0.08f + i) * 15f;
                    Vector2 midPos = NPC.Center + midAngle.ToRotationVector2() * midRadius;
                    Color midColor = Color.Lerp(EnigmaPurple, EnigmaGreen, (Timer % 30) / 30f);
                    CustomParticles.GenericGlow(midPos, midColor, 0.35f, 15);
                }
                
                // Outer layer - slow spin
                float outerAngle = Timer * 0.08f;
                float outerRadius = 120f - (Timer % 60) * 1.5f;
                Vector2 outerPos = NPC.Center + outerAngle.ToRotationVector2() * outerRadius;
                CustomParticles.GenericGlow(outerPos, EnigmaPurple * 0.7f, 0.3f, 20);
            }
            
            // Glyph circle continuously spinning
            if (Timer % 8 == 0)
            {
                CustomParticles.GlyphCircle(NPC.Center, EnigmaPurple, 6, 100f - (Timer % 40), 0.12f);
            }
            
            // Shoot during vortex
            if (Timer % 20 == 10)
            {
                Vector2 shootDir = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
                
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, shootDir * 12f,
                        ModContent.ProjectileType<EnigmaWebShot>(), NPC.damage / 3, 2f, Main.myPlayer);
                }
                
                CustomParticles.GenericFlare(NPC.Center, EnigmaGreen, 0.5f, 12);
            }
            
            if (Timer >= 120)
            {
                State = ActionState.Crawling;
                Timer = 0;
            }
        }
        
        #endregion
        
        #region Death Animation
        
        public override bool CheckDead()
        {
            if (State != ActionState.Dying)
            {
                State = ActionState.Dying;
                Timer = 0;
                deathTimer = 0;
                NPC.life = 1;
                NPC.dontTakeDamage = true;
                NPC.velocity = Vector2.Zero;
                return false;
            }
            return true;
        }
        
        private void UpdateDeathAnimation(Player target)
        {
            deathTimer++;
            NPC.velocity *= 0.95f;
            
            float progress = (float)deathTimer / DeathAnimationDuration;
            
            // Phase 1: Convulsions
            if (deathTimer < 180)
            {
                float intensity = deathTimer / 180f;
                
                if (deathTimer % 10 == 0)
                {
                    // Screen shake removed during death animation
                    UnifiedVFX.EnigmaVariations.Impact(NPC.Center + Main.rand.NextVector2Circular(50, 30), 0.5f + intensity * 0.5f);
                }
                
                if (deathTimer % 15 == 0)
                {
                    CustomParticles.EnigmaEyeExplosion(NPC.Center, EnigmaGreen, 3, 3f);
                }
            }
            // Phase 2: Reality fracturing
            else if (deathTimer < 360)
            {
                float intensity = (deathTimer - 180) / 180f;
                
                if (deathTimer % 8 == 0)
                {
                    // Screen shake removed during death animation - only chromatic effect
                    FateRealityDistortion.TriggerChromaticAberration(NPC.Center, 4f + intensity * 6f, 15);
                }
                
                if (deathTimer % 20 == 0)
                {
                    UnifiedVFX.EnigmaVariations.Explosion(NPC.Center + Main.rand.NextVector2Circular(80, 50), 1f + intensity);
                    CustomParticles.GlyphBurst(NPC.Center, EnigmaPurple, 8, 5f);
                }
                
                deathFlashIntensity = intensity * 0.5f;
            }
            // Phase 3: Final collapse
            else if (deathTimer == 360)
            {
                Main.NewText("The Hollow Mystery fades into the unknown...", EnigmaGreen);
                
                // Massive final explosion - single shake at climax moment only
                UnifiedVFX.EnigmaVariations.DeathExplosion(NPC.Center, 3f);
                
                // Eye explosion
                CustomParticles.EnigmaEyeExplosion(NPC.Center, EnigmaGreen, 20, 12f);
                CustomParticles.EnigmaEyeFormation(NPC.Center, EnigmaPurple, 12, 150f);
                
                // Glyph explosion
                CustomParticles.GlyphBurst(NPC.Center, EnigmaPurple, 20, 10f);
                CustomParticles.GlyphCircle(NPC.Center, EnigmaGreen, 16, 200f, 0f);
                
                FateRealityDistortion.TriggerInversionPulse(15);
                FateRealityDistortion.TriggerChromaticAberration(NPC.Center, 12f, 40);
                
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.5f, Volume = 1.5f }, NPC.Center);
                SoundEngine.PlaySound(SoundID.NPCDeath10 with { Volume = 1.3f }, NPC.Center);
            }
            
            // Actually die
            if (deathTimer >= DeathAnimationDuration)
            {
                NPC.life = 0;
                NPC.HitEffect(0, 9999);
                NPC.checkDead();
            }
        }
        
        #endregion
        
        #region Drops
        
        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            // Resonance Energy (always drops)
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<EnigmaResonantEnergy>(), 1, 15, 25));
            
            // Harmonic Core (always drops)
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<HarmonicCoreOfEnigma>(), 1, 1, 1));
            
            // Weapons - first drop guaranteed (various types)
            npcLoot.Add(ItemDropRule.OneFromOptions(1,
                ModContent.ItemType<VariationsOfTheVoid>(),      // Melee Sword
                ModContent.ItemType<TheUnresolvedCadence>(),     // Melee Broadsword
                ModContent.ItemType<DissonanceOfSecrets>(),      // Magic Staff
                ModContent.ItemType<CipherNocturne>(),           // Magic Beam
                ModContent.ItemType<FugueOfTheUnknown>()));      // Magic Tome
            
            // Second weapon drop 50% chance (remaining types)
            npcLoot.Add(ItemDropRule.OneFromOptions(2,
                ModContent.ItemType<TheWatchingRefrain>(),       // Summon
                ModContent.ItemType<TheSilentMeasure>(),         // Ranged Gun
                ModContent.ItemType<TacetsEnigma>()));
            
            // Treasure bag in expert
            npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<EnigmaTreasureBag>()));
        }
        
        #endregion
        
        #region Drawing
        
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = TextureAssets.Npc[Type].Value;
            
            // Single PNG - use full texture
            Rectangle sourceRect = new Rectangle(0, 0, texture.Width, texture.Height);
            
            Vector2 drawPos = NPC.Center - screenPos;
            Vector2 origin = new Vector2(texture.Width / 2, texture.Height / 2);
            SpriteEffects effects = NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            
            // Glow effect
            float glowIntensity = eyeGlowIntensity;
            if (State == ActionState.Dying)
                glowIntensity = deathFlashIntensity;
            
            // Draw glow underlay
            if (glowIntensity > 0.1f)
            {
                Color glowColor = EnigmaGreen * glowIntensity * 0.3f;
                spriteBatch.Draw(texture, drawPos, sourceRect, glowColor, 0f, origin, NPC.scale * 1.1f, effects, 0f);
            }
            
            // Main draw
            spriteBatch.Draw(texture, drawPos, sourceRect, drawColor, 0f, origin, NPC.scale, effects, 0f);
            
            // Overlay glow
            if (glowIntensity > 0.2f)
            {
                Color overlayColor = Color.Lerp(EnigmaPurple, EnigmaGreen, (float)Math.Sin(auraPulse) * 0.5f + 0.5f) * glowIntensity * 0.2f;
                spriteBatch.Draw(texture, drawPos, sourceRect, overlayColor, 0f, origin, NPC.scale, effects, 0f);
            }
            
            return false;
        }
        
        public override void FindFrame(int frameHeight)
        {
            frameCounter++;
            
            int speed = FrameSpeed;
            if (State == ActionState.Charging || State == ActionState.Pouncing)
                speed = 3; // Faster animation during attacks
            
            if (frameCounter >= speed)
            {
                frameCounter = 0;
                currentFrame++;
                if (currentFrame >= TotalFrames)
                    currentFrame = 0;
            }
        }
        
        #endregion
    }
}
