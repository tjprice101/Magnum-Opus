using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using MagnumOpus.Content.Eroica.ResonanceEnergies;
using MagnumOpus.Content.Eroica.Projectiles;
using MagnumOpus.Content.Eroica.ResonantWeapons;
using MagnumOpus.Content.Eroica.Pets;
using MagnumOpus.Content.Eroica.Enemies;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.Eroica.Bosses
{
    /// <summary>
    /// Eroica, God of Valor - Complete rework of the Eroica boss.
    /// Phase 1: 3 Flames of Valor orbit and attack, boss is invulnerable.
    /// Phase 2: Boss becomes enraged with charge attacks and Hand of Valor projectiles.
    /// </summary>
    public class EroicasRetribution : ModNPC
    {
        // Use the new sprite sheet
        public override string Texture => "MagnumOpus/Content/Eroica/Bosses/EroicaGodOfValor";
        
        // AI States
        private enum ActionState
        {
            // Phase 1 - Flames alive
            Phase1Hover,
            
            // Phase 2 - Flames dead
            Phase2Transition,
            Phase2Hover,
            ChargeWindup,
            ChargeAttack,
            ChargeRecovery,
            HandWindup,
            HandThrow,
            FistWindup,
            FistAttack
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

        private bool phase2Started = false;
        private bool flamesSpawned = false;
        private int chargeCount = 0;
        private const int MaxCharges = 3;
        private Vector2 chargeDirection = Vector2.Zero;
        
        // Speed boost mechanics
        private int flamesKilledCount = 0; // Track how many flames have died for Phase 1 speed boost
        private int previousFlameCount = 3; // Track previous flame count to detect deaths
        
        // Phase 2 aura pulsing
        private float auraPulse = 0f;
        
        // Fluid movement
        private float hoverWaveOffset = 0f;
        
        // Animation - adjust based on your sprite sheet
        private int frameCounter = 0;
        private int currentFrame = 0;
        private const int FrameTime = 4;
        private const int FrameColumns = 6; // Adjust to actual
        private const int FrameRows = 6; // Adjust to actual
        private const int TotalFrames = 36;
        
        // Death animation
        private bool isDying = false;
        private int deathTimer = 0;
        private const int DeathAnimationDuration = 180;
        private float screenFlashIntensity = 0f;
        
        // Health bar registration
        private bool hasRegisteredHealthBar = false;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = TotalFrames;
            
            NPCID.Sets.MPAllowedEnemies[Type] = true;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
            NPCID.Sets.TrailCacheLength[Type] = 10;
            NPCID.Sets.TrailingMode[Type] = 1;
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
            // Hitbox matches visual: ~166px frame × 0.65f scale = ~108px
            NPC.width = 100;
            NPC.height = 120;
            NPC.damage = 90;
            NPC.defense = 80;
            NPC.lifeMax = 406306; // Keep original health
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath14;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.value = Item.buyPrice(gold: 20);
            NPC.boss = true;
            NPC.npcSlots = 15f;
            NPC.aiStyle = -1;
            NPC.scale = 0.65f; // 45% increase from previous (larger boss)
            NPC.dontTakeDamage = true; // Invulnerable until flames die
            
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
                new FlavorTextBestiaryInfoElement("Eroica, God of Valor - a triumphant deity born from Beethoven's Third Symphony. " +
                    "Three Flames of Valor serve as guardians of its divine spirit.")
            });
        }

        public override void AI()
        {
            // Register with custom health bar system
            if (!hasRegisteredHealthBar)
            {
                BossHealthBarUI.RegisterBoss(NPC, BossColorTheme.Eroica);
                hasRegisteredHealthBar = true;
            }
            
            // Handle death animation
            if (isDying)
            {
                UpdateDeathAnimation();
                NPC.velocity = Main.rand.NextVector2Circular(2f, 2f);
                NPC.position += Main.rand.NextVector2Circular(1f, 1f);
                
                if (deathTimer >= DeathAnimationDuration)
                {
                    NPC.life = 0;
                    NPC.HitEffect();
                    NPC.checkDead();
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

            // Spawn flames on first frame
            if (!flamesSpawned && Main.netMode != NetmodeID.MultiplayerClient)
            {
                SpawnFlames();
                flamesSpawned = true;
            }

            // Update animation
            UpdateAnimation();
            
            // Update aura pulse
            auraPulse += 0.05f;

            // Check if all flames are dead for phase 2 transition
            if (!phase2Started)
            {
                int aliveFlames = CountAliveFlames();
                if (aliveFlames == 0 && flamesSpawned)
                {
                    State = ActionState.Phase2Transition;
                    Timer = 0;
                    phase2Started = true;
                    NPC.dontTakeDamage = false;
                    NPC.netUpdate = true;
                }
            }

            // Lighting
            float lightPulse = 0.8f + (float)Math.Sin(auraPulse) * 0.2f;
            if (phase2Started)
            {
                Lighting.AddLight(NPC.Center, 1f * lightPulse, 0.3f * lightPulse, 0.2f * lightPulse);
            }
            else
            {
                Lighting.AddLight(NPC.Center, 1f * lightPulse, 0.7f * lightPulse, 0.3f * lightPulse);
            }

            // Ambient particles
            SpawnAmbientParticles();

            Timer++;

            // State machine
            switch (State)
            {
                case ActionState.Phase1Hover:
                    Phase1Hover(target);
                    break;
                case ActionState.Phase2Transition:
                    Phase2Transition(target);
                    break;
                case ActionState.Phase2Hover:
                    Phase2Hover(target);
                    break;
                case ActionState.ChargeWindup:
                    ChargeWindup(target);
                    break;
                case ActionState.ChargeAttack:
                    ChargeAttack(target);
                    break;
                case ActionState.ChargeRecovery:
                    ChargeRecovery(target);
                    break;
                case ActionState.HandWindup:
                    HandWindup(target);
                    break;
                case ActionState.HandThrow:
                    HandThrow(target);
                    break;
                case ActionState.FistWindup:
                    FistWindup(target);
                    break;
                case ActionState.FistAttack:
                    FistAttack(target);
                    break;
            }

            // Face the player - sprite faces RIGHT by default, flip for left
            NPC.spriteDirection = NPC.direction = (target.Center.X > NPC.Center.X) ? 1 : -1;
        }

        private void SpawnFlames()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;

            // Spawn 3 Flames of Valor at different orbit positions
            float[] offsets = { 0f, MathHelper.TwoPi / 3f, MathHelper.TwoPi * 2f / 3f };
            
            for (int i = 0; i < 3; i++)
            {
                int flameIndex = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, 
                    ModContent.NPCType<FlamesOfValor>());
                    
                if (flameIndex < Main.maxNPCs && Main.npc[flameIndex].ModNPC is FlamesOfValor flame)
                {
                    flame.SetOrbitOffset(offsets[i]);
                }
            }

            Main.NewText("The Flames of Valor ignite...", 255, 200, 100);
        }

        private int CountAliveFlames()
        {
            int count = 0;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].active && Main.npc[i].type == ModContent.NPCType<FlamesOfValor>())
                {
                    count++;
                }
            }
            return count;
        }
        
        private void UpdateAnimation()
        {
            frameCounter++;
            int animSpeed = phase2Started ? 2 : FrameTime;
            if (frameCounter >= animSpeed)
            {
                frameCounter = 0;
                currentFrame++;
                if (currentFrame >= TotalFrames)
                    currentFrame = 0;
            }
        }

        private void SpawnAmbientParticles()
        {
            // Enhanced ambient particles using ThemedParticles
            if (phase2Started)
            {
                // Intense Phase 2 aura
                ThemedParticles.EroicaAura(NPC.Center, NPC.width * 0.7f);
                
                // Sakura petals during phase 2
                if (Main.rand.NextBool(2))
                {
                    ThemedParticles.SakuraPetals(NPC.Center, 8, NPC.width * 0.5f);
                }
            }
            else
            {
                // Gentle Phase 1 aura
                if (Main.rand.NextBool(3))
                {
                    ThemedParticles.EroicaSparkles(NPC.Center, 5, 35f);
                }
            }
            
            // Additional dust particles
            int spawnChance = phase2Started ? 2 : 5;
            if (Main.rand.NextBool(spawnChance))
            {
                int dustType = Main.rand.NextBool() ? DustID.GoldFlame : DustID.CrimsonTorch;
                float scale = phase2Started ? Main.rand.NextFloat(1.5f, 2.5f) : 1.3f;
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, dustType, 0f, 0f, 100, default, scale);
                dust.noGravity = true;
                dust.velocity = phase2Started ? Main.rand.NextVector2Circular(4f, 4f) : dust.velocity * 0.4f;
            }
        }

        #region Phase 1

        private void Phase1Hover(Player target)
        {
            // Check for flame deaths to increase speed
            int currentFlameCount = CountAliveFlames();
            if (currentFlameCount < previousFlameCount)
            {
                int flamesJustKilled = previousFlameCount - currentFlameCount;
                flamesKilledCount += flamesJustKilled;
                previousFlameCount = currentFlameCount;
            }
            
            // Calculate speed multiplier: 5% per flame killed (up to 15% at 3 flames)
            float phase1SpeedMultiplier = 1f + (flamesKilledCount * 0.05f);
            
            hoverWaveOffset += 0.04f; // Slightly faster wave
            
            // More aggressive wave motion - closer to player
            float waveX = (float)Math.Sin(hoverWaveOffset * 2.5f) * 60f;
            float waveY = (float)Math.Sin(hoverWaveOffset * 2f) * 30f;
            
            Vector2 hoverPosition = target.Center - new Vector2(-waveX, 280 + waveY); // Closer hover distance
            Vector2 direction = hoverPosition - NPC.Center;
            float distance = direction.Length();

            if (distance > 25f)
            {
                direction.Normalize();
                // Base speed 10f with multiplier, faster lerp for more aggressive following
                float baseSpeed = 10f * phase1SpeedMultiplier;
                NPC.velocity = Vector2.Lerp(NPC.velocity, direction * baseSpeed, 0.06f);
            }
            else
            {
                NPC.velocity *= 0.9f;
            }
        }

        #endregion

        #region Phase 2

        private void Phase2Transition(Player target)
        {
            NPC.velocity *= 0.9f;

            if (Timer == 1)
            {
                // Screen shake
                EroicaScreenShake.Phase2EnrageShake(NPC.Center);
                
                // Enhanced particle burst with ThemedParticles
                ThemedParticles.EroicaImpact(NPC.Center, 3f);
                ThemedParticles.EroicaShockwave(NPC.Center, 2.5f);
                
                // DRAMATIC MUSICAL BURST - heroic clef and notes!
                ThemedParticles.EroicaMusicalImpact(NPC.Center, 2.5f, true);
                ThemedParticles.EroicaMusicNotes(NPC.Center, 20, 80f);
                
                // Custom particles - dramatic phase transition
                CustomParticles.EroicaBossAttack(NPC.Center, 20);
                CustomParticles.GenericFlare(NPC.Center, new Color(200, 50, 50), 2.5f, 60);
                CustomParticles.GenericFlare(NPC.Center, new Color(255, 200, 100), 2f, 50);
                
                // Massive particle burst
                for (int i = 0; i < 60; i++)
                {
                    int dustType = Main.rand.NextBool() ? DustID.GoldFlame : DustID.CrimsonTorch;
                    Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, dustType, 0f, 0f, 100, default, 3f);
                    dust.noGravity = true;
                    dust.velocity = Main.rand.NextVector2Circular(20f, 20f);
                }
                
                SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
            }

            if (Timer == 60)
            {
                Main.NewText("All Flames have been extinguished...", 255, 150, 100);
            }

            if (Timer == 120)
            {
                Main.NewText("Eroica invokes a new crown for its melody...", 255, 200, 100);
                
                // Enhanced burst with ThemedParticles
                ThemedParticles.EroicaBloomBurst(NPC.Center, 2f);
                ThemedParticles.SakuraPetals(NPC.Center, 16, NPC.width * 0.8f);
                
                // Musical staff effect for dramatic transformation
                ThemedParticles.EroicaMusicStaff(NPC.Center, 1.5f);
                ThemedParticles.EroicaMusicNotes(NPC.Center, 12, 60f);
                
                for (int i = 0; i < 50; i++)
                {
                    int dustType = Main.rand.NextBool() ? DustID.GoldFlame : DustID.CrimsonTorch;
                    Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, dustType, 0f, 0f, 0, default, 2.5f);
                    dust.noGravity = true;
                    dust.velocity = Main.rand.NextVector2Circular(16f, 16f);
                }
                
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
            // Calculate Phase 2 speed multiplier based on health (0% health = +45% speed)
            float healthPercent = (float)NPC.life / NPC.lifeMax;
            float phase2SpeedMultiplier = 1f + (1f - healthPercent) * 0.45f; // Up to 45% speed increase at low health
            
            hoverWaveOffset += 0.05f * phase2SpeedMultiplier;
            
            // More aggressive hover
            float waveX = (float)Math.Sin(hoverWaveOffset * 3f) * 80f;
            float waveY = (float)Math.Sin(hoverWaveOffset * 2f) * 40f;
            
            Vector2 hoverPosition = target.Center - new Vector2(-waveX, 280 + waveY);
            Vector2 direction = hoverPosition - NPC.Center;
            float distance = direction.Length();

            if (distance > 25f)
            {
                direction.Normalize();
                // Base speed with health-based multiplier
                float baseSpeed = 12f + (float)Math.Sin(hoverWaveOffset * 4f) * 3f;
                float speed = baseSpeed * phase2SpeedMultiplier;
                NPC.velocity = Vector2.Lerp(NPC.velocity, direction * speed, 0.06f * phase2SpeedMultiplier);
            }
            else
            {
                NPC.velocity *= 0.9f;
            }
            
            // Pulsing red/gold aura effect
            if (Timer % 8 == 0)
            {
                float pulse = (float)Math.Sin(auraPulse * 2f) * 0.5f + 0.5f;
                for (int i = 0; i < 3; i++)
                {
                    int dustType = Main.rand.NextBool() ? DustID.GoldFlame : DustID.CrimsonTorch;
                    Vector2 offset = Main.rand.NextVector2Circular(NPC.width * 0.6f, NPC.height * 0.6f);
                    Dust aura = Dust.NewDustDirect(NPC.Center + offset, 1, 1, dustType, 0f, 0f, 100, default, 1.5f + pulse);
                    aura.noGravity = true;
                    aura.velocity = offset.SafeNormalize(Vector2.Zero) * 2f;
                }
            }

            // Attack selection
            int attackDelay = 90;
            
            if (Timer > attackDelay)
            {
                Timer = 0;
                chargeCount = 0;
                
                // 40% charge attack, 30% hand throw, 30% fist attack
                int attackChoice = Main.rand.Next(10);
                if (attackChoice < 4) // 0-3 = 40%
                {
                    State = ActionState.ChargeWindup;
                    chargeDirection = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY);
                }
                else if (attackChoice < 7) // 4-6 = 30%
                {
                    State = ActionState.HandWindup;
                }
                else // 7-9 = 30%
                {
                    State = ActionState.FistWindup;
                }
                
                NPC.netUpdate = true;
            }
        }

        private void ChargeWindup(Player target)
        {
            const int WindupTime = 35;
            
            NPC.velocity *= 0.88f;
            
            // Play sound at start
            if (Timer == 1)
            {
                SoundEngine.PlaySound(SoundID.Item15 with { Pitch = -0.2f, Volume = 0.7f }, NPC.Center);
            }
            
            // Continuously track player throughout windup - boss follows player until charge
            chargeDirection = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY);
            
            // Telegraph line that tracks player
            if (Timer >= 5)
            {
                float lineProgress = (Timer - 5f) / (WindupTime - 5f);
                float lineLength = 600f * lineProgress;
                
                for (float dist = 0; dist < lineLength; dist += 25f)
                {
                    Vector2 linePos = NPC.Center + chargeDirection * dist;
                    int dustType = Main.rand.NextBool() ? DustID.GoldFlame : DustID.CrimsonTorch;
                    Dust warning = Dust.NewDustPerfect(linePos, dustType, Vector2.Zero, 100, default, 1.5f);
                    warning.noGravity = true;
                    warning.fadeIn = 0.4f;
                }
            }
            
            // Gathering particles
            if (Timer % 3 == 0)
            {
                for (int i = 0; i < 6; i++)
                {
                    Vector2 dustOffset = Main.rand.NextVector2Circular(120f, 120f);
                    int dustType = Main.rand.NextBool() ? DustID.GoldFlame : DustID.CrimsonTorch;
                    Dust dust = Dust.NewDustPerfect(NPC.Center + dustOffset, dustType, -dustOffset * 0.06f, 100, default, 2f);
                    dust.noGravity = true;
                }
            }
            
            if (Timer >= WindupTime)
            {
                Timer = 0;
                State = ActionState.ChargeAttack;
                NPC.velocity = chargeDirection * 55f;
                
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.1f }, NPC.Center);
                EroicaScreenShake.MediumShake(NPC.Center);
                
                // Musical burst on charge!
                ThemedParticles.EroicaMusicNotes(NPC.Center, 12, 60f);
                ThemedParticles.EroicaClef(NPC.Center, Main.rand.NextBool(), 1.5f);
                
                // Launch burst
                for (int i = 0; i < 30; i++)
                {
                    int dustType = Main.rand.NextBool() ? DustID.GoldFlame : DustID.CrimsonTorch;
                    Dust dust = Dust.NewDustDirect(NPC.Center, 1, 1, dustType, 0f, 0f, 100, default, 2.5f);
                    dust.noGravity = true;
                    dust.velocity = Main.rand.NextVector2Circular(12f, 12f);
                }
                
                NPC.netUpdate = true;
            }
        }

        private void ChargeAttack(Player target)
        {
            const int ChargeTime = 15;
            
            // Trail particles
            if (Timer % 2 == 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    int dustType = Main.rand.NextBool() ? DustID.GoldFlame : DustID.CrimsonTorch;
                    Vector2 offset = Main.rand.NextVector2Circular(NPC.width / 3f, NPC.height / 3f);
                    Dust trail = Dust.NewDustPerfect(NPC.Center + offset, dustType, -NPC.velocity * 0.12f, 100, default, 2f);
                    trail.noGravity = true;
                }
                
                // Custom particles trail during charge
                CustomParticles.EroicaFlare(NPC.Center, 0.6f);
            }
            
            Lighting.AddLight(NPC.Center, 1.2f, 0.5f, 0.3f);
            
            if (Timer >= ChargeTime)
            {
                Timer = 0;
                State = ActionState.ChargeRecovery;
                
                // Custom particles burst at end of charge
                CustomParticles.EroicaImpactBurst(NPC.Center, 8);
                
                // Explosion effect at end of charge
                for (int i = 0; i < 25; i++)
                {
                    int dustType = Main.rand.NextBool() ? DustID.GoldFlame : DustID.CrimsonTorch;
                    Dust explosion = Dust.NewDustDirect(NPC.Center, 1, 1, dustType, 0f, 0f, 100, default, 2.5f);
                    explosion.noGravity = true;
                    explosion.velocity = Main.rand.NextVector2Circular(10f, 10f);
                }
                
                SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.6f }, NPC.Center);
                
                NPC.netUpdate = true;
            }
        }

        private void ChargeRecovery(Player target)
        {
            const int RecoveryTime = 8; // Reduced from 20 for faster chaining
            
            NPC.velocity *= 0.88f;
            
            if (Timer >= RecoveryTime)
            {
                chargeCount++;
                Timer = 0;
                
                if (chargeCount >= MaxCharges)
                {
                    State = ActionState.Phase2Hover;
                    chargeCount = 0;
                    SoundEngine.PlaySound(SoundID.Item4 with { Volume = 0.4f }, NPC.Center);
                }
                else
                {
                    // Immediately chain into next charge - no hovering
                    State = ActionState.ChargeWindup;
                    chargeDirection = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY);
                }
                
                NPC.netUpdate = true;
            }
        }

        private void HandWindup(Player target)
        {
            const int WindupTime = 50;
            
            NPC.velocity *= 0.9f;
            
            if (Timer == 1)
            {
                SoundEngine.PlaySound(SoundID.Item15 with { Pitch = 0.1f, Volume = 0.6f }, NPC.Center);
            }
            
            // Charging animation - hands gathering
            if (Timer % 4 == 0)
            {
                for (int i = 0; i < 4; i++)
                {
                    Vector2 dustOffset = Main.rand.NextVector2Circular(100f, 100f);
                    int dustType = Main.rand.NextBool() ? DustID.GoldFlame : DustID.CrimsonTorch;
                    Dust dust = Dust.NewDustPerfect(NPC.Center + dustOffset, dustType, -dustOffset * 0.05f, 100, default, 1.8f);
                    dust.noGravity = true;
                }
            }
            
            // Telegraph - show cone direction
            if (Timer > 25)
            {
                Vector2 toPlayer = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY);
                float baseAngle = toPlayer.ToRotation();
                
                for (int hand = 0; hand < 3; hand++)
                {
                    float angle = baseAngle + MathHelper.ToRadians(-45 + hand * 45); // 90 degree spread (reduced from 120)
                    Vector2 direction = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                    
                    for (float dist = 50; dist < 200; dist += 40f)
                    {
                        Vector2 pos = NPC.Center + direction * dist;
                        int dustType = Main.rand.NextBool() ? DustID.GoldFlame : DustID.CrimsonTorch;
                        Dust telegraph = Dust.NewDustPerfect(pos, dustType, Vector2.Zero, 150, default, 1f);
                        telegraph.noGravity = true;
                        telegraph.fadeIn = 0.3f;
                    }
                }
            }
            
            if (Timer >= WindupTime)
            {
                Timer = 0;
                State = ActionState.HandThrow;
                NPC.netUpdate = true;
            }
        }

        private void HandThrow(Player target)
        {
            if (Timer == 1 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                // Throw 3 Hand of Valor projectiles in a 90 degree cone (reduced from 120)
                Vector2 toPlayer = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY);
                float baseAngle = toPlayer.ToRotation();
                float handSpeed = 16f;
                
                for (int hand = 0; hand < 3; hand++)
                {
                    float angle = baseAngle + MathHelper.ToRadians(-45 + hand * 45); // 90 degree spread
                    Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * handSpeed;
                    
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity,
                        ModContent.ProjectileType<HandOfValor>(), 100, 3f, Main.myPlayer);
                }
                
                SoundEngine.PlaySound(SoundID.Item71 with { Pitch = -0.1f, Volume = 0.8f }, NPC.Center);
                EroicaScreenShake.SmallShake(NPC.Center);
                
                // Musical particle burst!
                ThemedParticles.EroicaMusicNotes(NPC.Center, 8, 50f);
                ThemedParticles.EroicaAccidentals(NPC.Center, 4, 30f);
                
                // Throw particles
                for (int i = 0; i < 20; i++)
                {
                    int dustType = Main.rand.NextBool() ? DustID.GoldFlame : DustID.CrimsonTorch;
                    Dust dust = Dust.NewDustDirect(NPC.Center, 1, 1, dustType, 0f, 0f, 100, default, 2f);
                    dust.noGravity = true;
                    dust.velocity = Main.rand.NextVector2Circular(8f, 8f);
                }
            }
            
            NPC.velocity *= 0.95f;
            
            if (Timer >= 45)
            {
                Timer = 0;
                State = ActionState.Phase2Hover;
                NPC.netUpdate = true;
            }
        }
        
        // Store the randomly chosen attack side for fist attack (0=top, 1=bottom, 2=left, 3=right)
        private int fistAttackSide = 0;

        private void FistWindup(Player target)
        {
            const int WindupTime = 30; // Shorter windup since fists have their own delay
            
            NPC.velocity *= 0.9f;
            
            if (Timer == 1)
            {
                // Randomly choose which side all 3 fists spawn on
                fistAttackSide = Main.rand.Next(4); // 0=top, 1=bottom, 2=left, 3=right
                SoundEngine.PlaySound(SoundID.Item73 with { Pitch = -0.3f, Volume = 0.8f }, NPC.Center);
            }
            
            // Boss raises hands dramatically - gather particles
            if (Timer % 4 == 0)
            {
                for (int i = 0; i < 4; i++)
                {
                    Vector2 offset = Main.rand.NextVector2Circular(150f, 150f);
                    int dustType = Main.rand.NextBool() ? DustID.GoldFlame : DustID.CrimsonTorch;
                    Dust dust = Dust.NewDustPerfect(NPC.Center + offset, dustType, -offset * 0.08f, 100, default, 2.2f);
                    dust.noGravity = true;
                }
            }
            
            // Warning indicators at the chosen spawn side
            if (Timer >= 10 && Timer % 4 == 0)
            {
                // Get spawn positions based on chosen side
                Vector2[] spawnPositions = GetFistSpawnPositions(target.Center, fistAttackSide);
                
                foreach (var spawnPos in spawnPositions)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        Vector2 offset = Main.rand.NextVector2Circular(25f, 25f);
                        int dustType = Main.rand.NextBool() ? DustID.GoldCoin : DustID.CrimsonTorch;
                        Dust warning = Dust.NewDustPerfect(spawnPos + offset, dustType, -offset * 0.06f, 100, default, 1.8f);
                        warning.noGravity = true;
                    }
                }
            }
            
            if (Timer >= WindupTime)
            {
                Timer = 0;
                State = ActionState.FistAttack;
                NPC.netUpdate = true;
            }
        }
        
        private Vector2[] GetFistSpawnPositions(Vector2 playerCenter, int side)
        {
            // Returns 3 spawn positions: middle, far left/top, far right/bottom of the chosen side
            float edgeDistance = 600f; // Distance from player to spawn edge
            float spread = 550f; // Spread between fists along the edge (wider spacing)
            
            Vector2[] positions = new Vector2[3];
            
            switch (side)
            {
                case 0: // Top - spawns at top, charges down
                    positions[0] = playerCenter + new Vector2(0, -edgeDistance); // Middle
                    positions[1] = playerCenter + new Vector2(-spread, -edgeDistance); // Left
                    positions[2] = playerCenter + new Vector2(spread, -edgeDistance); // Right
                    break;
                case 1: // Bottom - spawns at bottom, charges up
                    positions[0] = playerCenter + new Vector2(0, edgeDistance); // Middle
                    positions[1] = playerCenter + new Vector2(-spread, edgeDistance); // Left
                    positions[2] = playerCenter + new Vector2(spread, edgeDistance); // Right
                    break;
                case 2: // Left - spawns at left, charges right
                    positions[0] = playerCenter + new Vector2(-edgeDistance, 0); // Middle
                    positions[1] = playerCenter + new Vector2(-edgeDistance, -spread); // Top
                    positions[2] = playerCenter + new Vector2(-edgeDistance, spread); // Bottom
                    break;
                case 3: // Right - spawns at right, charges left
                    positions[0] = playerCenter + new Vector2(edgeDistance, 0); // Middle
                    positions[1] = playerCenter + new Vector2(edgeDistance, -spread); // Top
                    positions[2] = playerCenter + new Vector2(edgeDistance, spread); // Bottom
                    break;
            }
            
            return positions;
        }

        private void FistAttack(Player target)
        {
            // Spawn fists at Timer == 1
            if (Timer == 1 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                // Get spawn positions based on the chosen side
                Vector2[] spawnPositions = GetFistSpawnPositions(target.Center, fistAttackSide);
                
                // Charge direction encoded: 0=down, 1=up, 2=right, 3=left
                int chargeDirectionCode;
                switch (fistAttackSide)
                {
                    case 0: chargeDirectionCode = 0; break; // Top spawns -> charge down
                    case 1: chargeDirectionCode = 1; break; // Bottom spawns -> charge up
                    case 2: chargeDirectionCode = 2; break; // Left spawns -> charge right
                    case 3: chargeDirectionCode = 3; break; // Right spawns -> charge left
                    default: chargeDirectionCode = 0; break;
                }
                
                for (int i = 0; i < 3; i++)
                {
                    // Spawn with zero velocity - projectile handles the windup and charge
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPositions[i], Vector2.Zero,
                        ModContent.ProjectileType<FistOfEroica>(), 120, 5f, Main.myPlayer, 0f, chargeDirectionCode);
                }
                
                SoundEngine.PlaySound(SoundID.Item117 with { Pitch = 0.2f, Volume = 1f }, target.Center);
                EroicaScreenShake.MediumShake(target.Center);
                
                // Enhanced musical burst with fractal pattern
                UnifiedVFX.Eroica.Impact(NPC.Center, 1.2f);
                ThemedParticles.EroicaMusicNotes(NPC.Center, 12, 70f);
                ThemedParticles.EroicaClef(NPC.Center, true, 2f);
                
                // Fractal flare burst - signature geometric look
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f;
                    Vector2 flareOffset = angle.ToRotationVector2() * 35f;
                    float progress = (float)i / 8f;
                    Color fractalColor = Color.Lerp(UnifiedVFX.Eroica.Scarlet, UnifiedVFX.Eroica.Gold, progress);
                    CustomParticles.GenericFlare(NPC.Center + flareOffset, fractalColor, 0.5f, 20);
                }
            }
            
            NPC.velocity *= 0.95f;
            
            // Return to hover after fists have time to cross screen
            if (Timer >= 60)
            {
                Timer = 0;
                State = ActionState.Phase2Hover;
                NPC.netUpdate = true;
            }
        }

        #endregion

        #region Loot

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            // Expert/Master mode: Drop treasure bag
            npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<EroicaTreasureBag>()));
            
            // Normal mode drops (half of expert amounts, NO BELL)
            LeadingConditionRule notExpert = new LeadingConditionRule(new Conditions.NotExpert());
            
            // 10-12 Energy (half of 20-25)
            notExpert.OnSuccess(ItemDropRule.Common(ModContent.ItemType<EroicasResonantEnergy>(), 1, 10, 12));
            
            // 15-17 Remnant (half of 30-35)
            notExpert.OnSuccess(ItemDropRule.Common(ModContent.ItemType<RemnantOfEroicasTriumph>(), 1, 15, 17));
            
            // 5-10 Shard of Tempo (half of 10-20)
            notExpert.OnSuccess(ItemDropRule.Common(ModContent.ItemType<ShardOfTriumphsTempo>(), 1, 5, 10));
            
            // 1-2 random weapons (NO bell in normal mode)
            notExpert.OnSuccess(new EroicaNormalModeWeaponRule());
            
            npcLoot.Add(notExpert);
        }

        #endregion

        #region Death Animation

        public override bool CheckDead()
        {
            if (isDying && deathTimer >= DeathAnimationDuration)
            {
                return true;
            }
            
            if (!isDying)
            {
                isDying = true;
                deathTimer = 0;
                NPC.life = 1;
                NPC.dontTakeDamage = true;
                NPC.velocity = Vector2.Zero;
                
                SoundEngine.PlaySound(SoundID.Item105, NPC.Center);
            }
            
            return false;
        }

        private void UpdateDeathAnimation()
        {
            if (!isDying) return;
            
            deathTimer++;
            
            // Phase 1: Building intensity with gradient Scarlet → Gold effects
            if (deathTimer < 120)
            {
                float intensity = (float)deathTimer / 120f;
                
                // Fractal flare pattern with gradient colors
                if (deathTimer % 5 == 0)
                {
                    int points = 6 + (int)(intensity * 4);
                    for (int i = 0; i < points; i++)
                    {
                        float angle = MathHelper.TwoPi * i / points + deathTimer * 0.05f;
                        Vector2 offset = angle.ToRotationVector2() * (30f + intensity * 40f);
                        float progress = (float)i / points;
                        Color flareColor = Color.Lerp(UnifiedVFX.Eroica.Scarlet, UnifiedVFX.Eroica.Gold, progress);
                        CustomParticles.GenericFlare(NPC.Center + offset, flareColor, 0.4f + intensity * 0.4f, 15);
                    }
                    
                    // Central pulsing flare
                    CustomParticles.GenericFlare(NPC.Center, Color.Lerp(UnifiedVFX.Eroica.Crimson, UnifiedVFX.Eroica.Gold, intensity), 
                        0.6f + intensity * 0.6f, 20);
                }
                
                // Sakura petals intensifying
                if (deathTimer % 10 == 0)
                    ThemedParticles.SakuraPetals(NPC.Center, (int)(2 + intensity * 4), 60f + intensity * 40f);
                
                // Pulsing halo rings
                if (deathTimer % 15 == 0)
                {
                    CustomParticles.HaloRing(NPC.Center, Color.Lerp(UnifiedVFX.Eroica.Scarlet, UnifiedVFX.Eroica.Gold, intensity), 
                        0.4f + intensity * 0.4f, 25);
                }
                
                if (Main.LocalPlayer.Distance(NPC.Center) < 1500f)
                    MagnumScreenEffects.AddScreenShake(intensity * 3f);
                
                if (deathTimer % 20 == 0)
                    SoundEngine.PlaySound(SoundID.Item74, NPC.Center);
            }
            // Phase 2: Flash buildup with intense effects
            else if (deathTimer < 150)
            {
                float flashProgress = (deathTimer - 120f) / 30f;
                screenFlashIntensity = flashProgress;
                
                // Expanding gradient rings
                if (deathTimer % 4 == 0)
                {
                    UnifiedVFX.Eroica.Impact(NPC.Center, 0.8f + flashProgress * 0.5f);
                }
                
                // Music notes ascending
                ThemedParticles.EroicaMusicNotes(NPC.Center, 4, 50f + flashProgress * 30f);
                
                if (Main.LocalPlayer.Distance(NPC.Center) < 2000f)
                    MagnumScreenEffects.AddScreenShake(8f + flashProgress * 6f);
            }
            // Phase 3: CLIMAX - Full UnifiedVFX death explosion
            else if (deathTimer == 150)
            {
                screenFlashIntensity = 1f;
                SoundEngine.PlaySound(SoundID.Item122, NPC.Center);
                
                // Massive themed death explosion
                UnifiedVFX.Eroica.DeathExplosion(NPC.Center, 1.5f);
                
                // Extra spiral galaxy effect for heroic finale
                for (int arm = 0; arm < 6; arm++)
                {
                    float armAngle = MathHelper.TwoPi * arm / 6f;
                    for (int point = 0; point < 8; point++)
                    {
                        float spiralAngle = armAngle + point * 0.4f;
                        float spiralRadius = 25f + point * 18f;
                        Vector2 spiralPos = NPC.Center + spiralAngle.ToRotationVector2() * spiralRadius;
                        float progress = (arm * 8 + point) / 48f;
                        Color galaxyColor = Color.Lerp(UnifiedVFX.Eroica.Scarlet, UnifiedVFX.Eroica.Gold, progress);
                        CustomParticles.GenericFlare(spiralPos, galaxyColor, 0.5f + point * 0.05f, 25 + point * 2);
                    }
                }
                
                // Heroic music staff finale
                ThemedParticles.EroicaMusicStaff(NPC.Center, 2f);
            }
            // Phase 4: Fade out
            else if (deathTimer <= DeathAnimationDuration)
            {
                float fadeProgress = (deathTimer - 150f) / 30f;
                screenFlashIntensity = 1f - fadeProgress;
                
                // Lingering sakura petals
                if (deathTimer % 8 == 0)
                    ThemedParticles.SakuraPetals(NPC.Center, 3, 100f * (1f - fadeProgress));
            }
            
            // Pulsing light with gradient color
            if (screenFlashIntensity > 0 && Main.LocalPlayer.Distance(NPC.Center) < 2000f)
            {
                Color lightColor = Color.Lerp(UnifiedVFX.Eroica.Crimson, UnifiedVFX.Eroica.Gold, screenFlashIntensity);
                Lighting.AddLight(NPC.Center, lightColor.ToVector3() * screenFlashIntensity * 3f);
            }
        }

        public override void OnKill()
        {
            // Final burst with gradient colors
            UnifiedVFX.Generic.FractalBurst(NPC.Center, UnifiedVFX.Eroica.Scarlet, UnifiedVFX.Eroica.Gold, 12, 80f, 1.5f);
            
            // Layered halo cascade
            for (int ring = 0; ring < 6; ring++)
            {
                float progress = (float)ring / 6f;
                Color ringColor = Color.Lerp(UnifiedVFX.Eroica.Scarlet, UnifiedVFX.Eroica.Gold, progress);
                CustomParticles.HaloRing(NPC.Center, ringColor * (1f - progress * 0.3f), 0.5f + ring * 0.2f, 20 + ring * 5);
            }
            
            // Final sakura shower
            ThemedParticles.SakuraPetals(NPC.Center, 25, 120f);
            
            // Triumphant music burst
            ThemedParticles.EroicaMusicNotes(NPC.Center, 16, 100f);
            ThemedParticles.EroicaClef(NPC.Center, true, 2.5f);

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Main.NewText("Eroica's valor echoes through eternity...", 255, 200, 100);
            }
        }

        #endregion

        #region Targeting Prevention (Phase 1)

        public override bool? CanBeHitByProjectile(Projectile projectile)
        {
            if (!phase2Started && flamesSpawned && CountAliveFlames() > 0)
            {
                return false;
            }
            return null;
        }

        public override bool CanBeHitByNPC(NPC attacker)
        {
            if (!phase2Started && flamesSpawned && CountAliveFlames() > 0)
            {
                return false;
            }
            return true;
        }

        public override bool? CanBeHitByItem(Player player, Item item)
        {
            if (!phase2Started && flamesSpawned && CountAliveFlames() > 0)
            {
                return false;
            }
            return null;
        }

        #endregion

        #region Drawing

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Npc[Type].Value;
            
            int frameWidth = texture.Width / FrameColumns;
            int frameHeight = texture.Height / FrameRows;
            int column = currentFrame % FrameColumns;
            int row = currentFrame / FrameColumns;
            
            Rectangle sourceRect = new Rectangle(column * frameWidth, row * frameHeight, frameWidth, frameHeight);
            Vector2 drawOrigin = new Vector2(frameWidth / 2, frameHeight / 2);
            
            // Flip sprite when facing left (sprite faces RIGHT by default)
            SpriteEffects effects = NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            // Draw trail
            for (int k = 0; k < NPC.oldPos.Length; k++)
            {
                Vector2 drawPos = NPC.oldPos[k] - screenPos + new Vector2(NPC.width / 2, NPC.height / 2);
                Color trailColor;
                
                if (phase2Started)
                {
                    trailColor = new Color(255, 100, 50, 80) * ((float)(NPC.oldPos.Length - k) / NPC.oldPos.Length);
                }
                else
                {
                    trailColor = new Color(255, 200, 100, 80) * ((float)(NPC.oldPos.Length - k) / NPC.oldPos.Length);
                }
                
                float scale = NPC.scale * (1f - k * 0.08f);
                spriteBatch.Draw(texture, drawPos, sourceRect, trailColor, NPC.rotation, drawOrigin, scale, effects, 0f);
            }
            
            // Phase 2 pulsing aura glow
            if (phase2Started)
            {
                float pulse = (float)Math.Sin(auraPulse * 2f) * 0.3f + 0.7f;
                Color glowColor = new Color(255, 100, 50, 0) * pulse * 0.4f;
                
                for (int i = 0; i < 6; i++)
                {
                    Vector2 offset = new Vector2(6f, 0).RotatedBy(MathHelper.TwoPi * i / 6f);
                    spriteBatch.Draw(texture, NPC.Center - screenPos + offset, sourceRect, glowColor, NPC.rotation, drawOrigin, NPC.scale * 1.1f, effects, 0f);
                }
            }
            
            // Draw main sprite
            Vector2 mainDrawPos = NPC.Center - screenPos;
            Color mainColor = NPC.GetAlpha(drawColor);
            spriteBatch.Draw(texture, mainDrawPos, sourceRect, mainColor, NPC.rotation, drawOrigin, NPC.scale, effects, 0f);

            return false;
        }

        public override Color? GetAlpha(Color drawColor)
        {
            if (phase2Started)
            {
                // Red tint in phase 2
                return new Color(255, 200, 180, 230);
            }
            return new Color(255, 240, 220, 240); // Golden tint in phase 1
        }
        
        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            // The phase 2 overlay is now handled by EroicaSkyEffect for proper layering
            // Drawing fullscreen rectangles in NPC PostDraw can cause visual glitches
        }

        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            scale = 1.5f;
            return null;
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

        #endregion
    }
    
    /// <summary>
    /// Custom drop rule for normal mode that drops 1-2 random weapons (NO bell).
    /// </summary>
    public class EroicaNormalModeWeaponRule : IItemDropRule
    {
        public List<IItemDropRuleChainAttempt> ChainedRules => new List<IItemDropRuleChainAttempt>();

        public bool CanDrop(DropAttemptInfo info) => true;

        public void ReportDroprates(List<DropRateInfo> drops, DropRateInfoChainFeed ratesInfo)
        {
            int[] possibleDrops = GetPossibleDrops();
            float individualChance = 1.5f / possibleDrops.Length;
            
            foreach (int itemType in possibleDrops)
            {
                drops.Add(new DropRateInfo(itemType, 1, 1, individualChance, ratesInfo.conditions));
            }
        }

        public ItemDropAttemptResult TryDroppingItem(DropAttemptInfo info)
        {
            int[] possibleDrops = GetPossibleDrops();
            
            // Shuffle the array
            List<int> shuffled = new List<int>(possibleDrops);
            for (int i = shuffled.Count - 1; i > 0; i--)
            {
                int j = Main.rand.Next(i + 1);
                int temp = shuffled[i];
                shuffled[i] = shuffled[j];
                shuffled[j] = temp;
            }
            
            // Drop 1-2 items
            int dropCount = Main.rand.NextBool() ? 2 : 1;
            
            for (int i = 0; i < dropCount && i < shuffled.Count; i++)
            {
                CommonCode.DropItem(info, shuffled[i], 1);
            }
            
            return new ItemDropAttemptResult
            {
                State = ItemDropAttemptResultState.Success
            };
        }
        
        private int[] GetPossibleDrops()
        {
            // NO BELL in normal mode!
            return new int[]
            {
                ModContent.ItemType<FuneralPrayer>(),
                ModContent.ItemType<TriumphantFractal>(),
                ModContent.ItemType<SakurasBlossom>(),
                ModContent.ItemType<BlossomOfTheSakura>(),
                ModContent.ItemType<FinalityOfTheSakura>(),
                ModContent.ItemType<PiercingLightOfTheSakura>(),
                ModContent.ItemType<CelestialValor>()
            };
        }
    }
}
