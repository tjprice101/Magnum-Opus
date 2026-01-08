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
            HandThrow
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
            NPC.width = 105;
            NPC.height = 105;
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
            int spawnChance = phase2Started ? 1 : 4;
            int particleCount = phase2Started ? 4 : 1;
            
            if (Main.rand.NextBool(spawnChance))
            {
                for (int i = 0; i < particleCount; i++)
                {
                    int dustType = Main.rand.NextBool() ? DustID.GoldFlame : DustID.CrimsonTorch;
                    float scale = phase2Started ? Main.rand.NextFloat(1.5f, 2.5f) : 1.3f;
                    Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, dustType, 0f, 0f, 100, default, scale);
                    dust.noGravity = true;
                    dust.velocity = phase2Started ? Main.rand.NextVector2Circular(4f, 4f) : dust.velocity * 0.4f;
                }
            }
        }

        #region Phase 1

        private void Phase1Hover(Player target)
        {
            hoverWaveOffset += 0.03f;
            
            // Gentle wave motion above player
            float waveX = (float)Math.Sin(hoverWaveOffset * 2f) * 50f;
            float waveY = (float)Math.Sin(hoverWaveOffset * 1.5f) * 25f;
            
            Vector2 hoverPosition = target.Center - new Vector2(-waveX, 320 + waveY);
            Vector2 direction = hoverPosition - NPC.Center;
            float distance = direction.Length();

            if (distance > 30f)
            {
                direction.Normalize();
                NPC.velocity = Vector2.Lerp(NPC.velocity, direction * 8f, 0.04f);
            }
            else
            {
                NPC.velocity *= 0.92f;
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
                
                // Massive particle burst
                for (int i = 0; i < 100; i++)
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
                
                for (int i = 0; i < 80; i++)
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
            hoverWaveOffset += 0.05f;
            
            // More aggressive hover
            float waveX = (float)Math.Sin(hoverWaveOffset * 3f) * 80f;
            float waveY = (float)Math.Sin(hoverWaveOffset * 2f) * 40f;
            
            Vector2 hoverPosition = target.Center - new Vector2(-waveX, 280 + waveY);
            Vector2 direction = hoverPosition - NPC.Center;
            float distance = direction.Length();

            if (distance > 25f)
            {
                direction.Normalize();
                float speed = 12f + (float)Math.Sin(hoverWaveOffset * 4f) * 3f;
                NPC.velocity = Vector2.Lerp(NPC.velocity, direction * speed, 0.06f);
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
                
                // 60% charge attack, 40% hand throw
                if (Main.rand.NextBool(3, 5))
                {
                    State = ActionState.ChargeWindup;
                    chargeDirection = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY);
                }
                else
                {
                    State = ActionState.HandWindup;
                }
                
                NPC.netUpdate = true;
            }
        }

        private void ChargeWindup(Player target)
        {
            const int WindupTime = 35;
            
            NPC.velocity *= 0.88f;
            
            // Lock in charge direction
            if (Timer == 1)
            {
                chargeDirection = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY);
                SoundEngine.PlaySound(SoundID.Item15 with { Pitch = -0.2f, Volume = 0.7f }, NPC.Center);
            }
            
            // Telegraph line
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
            }
            
            Lighting.AddLight(NPC.Center, 1.2f, 0.5f, 0.3f);
            
            if (Timer >= ChargeTime)
            {
                Timer = 0;
                State = ActionState.ChargeRecovery;
                
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
            const int RecoveryTime = 20;
            
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
                    // Continue with another charge
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
                    float angle = baseAngle + MathHelper.ToRadians(-60 + hand * 60); // 120 degree spread
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
                // Throw 3 Hand of Valor projectiles in a 120 degree cone
                Vector2 toPlayer = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY);
                float baseAngle = toPlayer.ToRotation();
                float handSpeed = 16f;
                
                for (int hand = 0; hand < 3; hand++)
                {
                    float angle = baseAngle + MathHelper.ToRadians(-60 + hand * 60); // 120 degree spread
                    Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * handSpeed;
                    
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity,
                        ModContent.ProjectileType<HandOfValor>(), 100, 3f, Main.myPlayer);
                }
                
                SoundEngine.PlaySound(SoundID.Item71 with { Pitch = -0.1f, Volume = 0.8f }, NPC.Center);
                EroicaScreenShake.SmallShake(NPC.Center);
                
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
            
            // Red and gold flares with increasing intensity
            if (deathTimer < 120)
            {
                float intensity = (float)deathTimer / 120f;
                int flareCount = (int)(1 + intensity * 6);
                
                for (int i = 0; i < flareCount; i++)
                {
                    int dustType = Main.rand.NextBool() ? DustID.GoldFlame : DustID.CrimsonTorch;
                    Vector2 velocity = Main.rand.NextVector2Circular(15f, 15f) * (0.5f + intensity);
                    Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, dustType, velocity.X, velocity.Y, 0, default, 2f + intensity * 2f);
                    dust.noGravity = true;
                    dust.fadeIn = 1.5f;
                }
                
                if (Main.LocalPlayer.Distance(NPC.Center) < 1500f)
                {
                    float shakeAmount = intensity * 6f;
                    Main.LocalPlayer.velocity += Main.rand.NextVector2Circular(shakeAmount, shakeAmount) * 0.1f;
                }
                
                if (deathTimer % 20 == 0)
                {
                    SoundEngine.PlaySound(SoundID.Item74, NPC.Center);
                }
            }
            else if (deathTimer < 150)
            {
                float flashProgress = (deathTimer - 120f) / 30f;
                screenFlashIntensity = flashProgress;
                
                for (int i = 0; i < 12; i++)
                {
                    int dustType = Main.rand.NextBool() ? DustID.GoldFlame : DustID.CrimsonTorch;
                    Vector2 velocity = Main.rand.NextVector2Circular(20f, 20f);
                    Dust burst = Dust.NewDustDirect(NPC.Center, 0, 0, dustType, velocity.X, velocity.Y, 0, default, 3f);
                    burst.noGravity = true;
                }
                
                if (Main.LocalPlayer.Distance(NPC.Center) < 2000f)
                {
                    float shakeAmount = 12f;
                    Main.LocalPlayer.velocity += Main.rand.NextVector2Circular(shakeAmount, shakeAmount) * 0.15f;
                }
            }
            else if (deathTimer == 150)
            {
                screenFlashIntensity = 1f;
                SoundEngine.PlaySound(SoundID.Item122, NPC.Center);
                
                for (int i = 0; i < 100; i++)
                {
                    int dustType = Main.rand.NextBool() ? DustID.GoldFlame : DustID.CrimsonTorch;
                    Vector2 velocity = Main.rand.NextVector2Circular(25f, 25f);
                    Dust explosion = Dust.NewDustDirect(NPC.Center, 0, 0, dustType, velocity.X, velocity.Y, 0, Color.White, 4f);
                    explosion.noGravity = true;
                    explosion.fadeIn = 2f;
                }
            }
            else if (deathTimer <= DeathAnimationDuration)
            {
                float fadeProgress = (deathTimer - 150f) / 30f;
                screenFlashIntensity = 1f - fadeProgress;
            }
            
            if (screenFlashIntensity > 0 && Main.LocalPlayer.Distance(NPC.Center) < 2000f)
            {
                Lighting.AddLight(NPC.Center, screenFlashIntensity * 3f, screenFlashIntensity * 2f, screenFlashIntensity);
            }
        }

        public override void OnKill()
        {
            for (int i = 0; i < 80; i++)
            {
                int dustType = Main.rand.NextBool() ? DustID.GoldFlame : DustID.CrimsonTorch;
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, dustType, 0f, 0f, 100, default, 2.5f);
                dust.noGravity = true;
                dust.velocity = Main.rand.NextVector2Circular(14f, 14f);
            }

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
            // Dark red overlay in phase 2
            if (phase2Started && !Main.dedServ)
            {
                float overlayIntensity = 0.15f + (float)Math.Sin(auraPulse) * 0.05f;
                Color overlayColor = new Color(80, 10, 20, (int)(overlayIntensity * 255));
                
                Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;
                Rectangle screenRect = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);
                
                spriteBatch.Draw(pixel, screenRect, overlayColor);
            }
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
