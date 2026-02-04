using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Eroica.ResonanceEnergies;
using MagnumOpus.Content.Materials.EnemyDrops;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.Eroica.Enemies
{
    /// <summary>
    /// Stolen Valor - A desert mini-boss with 5 unique red and gold attacks.
    /// Commands orbiting minions and has devastating coordinated attacks.
    /// Spawns in deserts during day at 5% rate after Moon Lord is defeated.
    /// 
    /// 5 ATTACKS:
    /// 1. Minion Barrage - All minions fire at player simultaneously
    /// 2. Charging Formation - Minions charge in formation at player
    /// 3. Orbital Bombardment - Minions spiral outward firing projectiles
    /// 4. False Glory - Teleport minions around player creating cage
    /// 5. Stolen Triumph - Ultimate attack with massive minion explosion
    /// </summary>
    public class StolenValor : ModNPC
    {
        private enum AIState
        {
            Idle,
            Walking,
            SmallHop,
            MinionBarrage,      // Attack 1
            ChargingFormation,  // Attack 2
            OrbitalBombardment, // Attack 3
            FalseGlory,         // Attack 4
            StolenTriumph       // Attack 5
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

        private int AttackCounter
        {
            get => (int)NPC.ai[3];
            set => NPC.ai[3] = value;
        }

        // Minion tracking
        private int[] orbitingMinions = new int[5] { -1, -1, -1, -1, -1 }; // 5 minions now
        private float orbitAngle = 0f;

        // Animation - 6x6 sprite sheet (36 frames)
        private int frameCounter = 0;
        private int currentFrame = 0;
        private const int FrameTime = 5;
        private const int FrameColumns = 6;
        private const int FrameRows = 6;
        private const int TotalFrames = 36;

        // Movement tracking
        private int lastSpriteDirection = 1;
        private bool isMoving = false;

        // Colors
        private static readonly Color EroicaRed = new Color(200, 40, 40);
        private static readonly Color EroicaGold = new Color(255, 200, 100);
        private static readonly Color EroicaDark = new Color(80, 20, 20);

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = TotalFrames;

            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Frostburn] = true;

            NPCID.Sets.DangerDetectRange[Type] = 700;
        }

        public override void SetDefaults()
        {
            // MINI-BOSS STATS
            // Hitbox = (1660/6) × (868/6) × 1.15 × 0.8 = 276.6 × 144.6 × 1.15 × 0.8 = 254 × 133
            NPC.width = 254;
            NPC.height = 133;
            NPC.damage = 170;
            NPC.defense = 85;
            NPC.lifeMax = 70000;
            NPC.HitSound = SoundID.NPCHit41;
            NPC.DeathSound = SoundID.NPCDeath43;
            NPC.knockBackResist = 0.02f;
            NPC.value = Item.buyPrice(gold: 45);
            NPC.aiStyle = -1;
            NPC.scale = 1.15f;
            NPC.npcSlots = 5f;

            NPC.noGravity = false;
            NPC.noTileCollide = false;

            DrawOffsetY = -6f;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.DayTime,
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Desert,
                new FlavorTextBestiaryInfoElement("A twisted manifestation commanding five dark echoes. Its coordinated attacks with orbiting minions make it a formidable foe. It wears the guise of heroism, but its glory is hollow and stolen.")
            });
        }

        public override void AI()
        {
            Player target = Main.player[NPC.target];

            // Check if moving
            float movementThreshold = 0.5f;
            isMoving = Math.Abs(NPC.velocity.X) > movementThreshold || Math.Abs(NPC.velocity.Y) > movementThreshold;

            if (isMoving)
            {
                if (target.Center.X > NPC.Center.X)
                    lastSpriteDirection = 1;
                else
                    lastSpriteDirection = -1;
            }
            NPC.spriteDirection = lastSpriteDirection;

            // Glow
            Lighting.AddLight(NPC.Center, 0.7f, 0.15f, 0.15f);

            // Ambient particles
            ThemedParticles.EroicaAura(NPC.Center, NPC.width * 0.6f);
            
            if (Main.rand.NextBool(10))
            {
                ThemedParticles.EroicaSparkles(NPC.Center, 3, NPC.width * 0.5f);
            }

            // Spawn minions on first tick
            if (StateTimer == 0f && CurrentState == AIState.Idle)
            {
                SpawnMinions();
                StateTimer = 1f;
            }

            // Manage orbiting minions
            ManageOrbitingMinions();

            // Update orbit angle
            orbitAngle += 0.02f;
            if (orbitAngle > MathHelper.TwoPi)
                orbitAngle -= MathHelper.TwoPi;

            float distanceToTarget = Vector2.Distance(NPC.Center, target.Center);

            // Update timers
            StateTimer++;
            if (AttackCooldown > 0f)
                AttackCooldown--;

            // Retarget
            NPC.TargetClosest(true);
            target = Main.player[NPC.target];

            // Select attack
            if (AttackCooldown <= 0f && distanceToTarget < 600f &&
                CurrentState != AIState.MinionBarrage && CurrentState != AIState.ChargingFormation &&
                CurrentState != AIState.OrbitalBombardment && CurrentState != AIState.FalseGlory &&
                CurrentState != AIState.StolenTriumph)
            {
                SelectNextAttack(target, distanceToTarget);
            }

            switch (CurrentState)
            {
                case AIState.Idle:
                case AIState.Walking:
                    HandleWalking(target, distanceToTarget);
                    break;
                case AIState.SmallHop:
                    HandleSmallHop();
                    break;
                case AIState.MinionBarrage:
                    HandleMinionBarrage(target);
                    break;
                case AIState.ChargingFormation:
                    HandleChargingFormation(target);
                    break;
                case AIState.OrbitalBombardment:
                    HandleOrbitalBombardment(target);
                    break;
                case AIState.FalseGlory:
                    HandleFalseGlory(target);
                    break;
                case AIState.StolenTriumph:
                    HandleStolenTriumph(target);
                    break;
            }

            // Animation update
            if (isMoving)
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
            else
            {
                currentFrame = 0;
                frameCounter = 0;
            }
        }

        private void SelectNextAttack(Player target, float distance)
        {
            AttackCounter++;
            int attackChoice = AttackCounter % 5;

            switch (attackChoice)
            {
                case 0: // Minion Barrage
                    CurrentState = AIState.MinionBarrage;
                    StateTimer = 0f;
                    SoundEngine.PlaySound(SoundID.Item73, NPC.Center);
                    break;

                case 1: // Charging Formation
                    CurrentState = AIState.ChargingFormation;
                    StateTimer = 0f;
                    SoundEngine.PlaySound(SoundID.Item74, NPC.Center);
                    break;

                case 2: // Orbital Bombardment
                    CurrentState = AIState.OrbitalBombardment;
                    StateTimer = 0f;
                    SoundEngine.PlaySound(SoundID.Item45, NPC.Center);
                    break;

                case 3: // False Glory
                    CurrentState = AIState.FalseGlory;
                    StateTimer = 0f;
                    SoundEngine.PlaySound(SoundID.Item8, NPC.Center);
                    break;

                case 4: // Stolen Triumph - when low health
                    if (NPC.life < NPC.lifeMax * 0.4f || Main.rand.NextBool(4))
                    {
                        CurrentState = AIState.StolenTriumph;
                        StateTimer = 0f;
                        SoundEngine.PlaySound(SoundID.Item119, NPC.Center);
                    }
                    else
                    {
                        CurrentState = AIState.MinionBarrage;
                        StateTimer = 0f;
                    }
                    break;
            }
        }

        private void HandleWalking(Player target, float distance)
        {
            float moveSpeed = 6f;
            float accel = 0.4f;
            
            if (distance > 60f)
            {
                if (target.Center.X > NPC.Center.X)
                    NPC.velocity.X = Math.Min(NPC.velocity.X + accel, moveSpeed);
                else
                    NPC.velocity.X = Math.Max(NPC.velocity.X - accel, -moveSpeed);
            }
            else
            {
                NPC.velocity.X *= 0.85f;
            }

            // Jump
            if (NPC.collideY && NPC.velocity.Y == 0f)
            {
                if (Main.rand.NextBool(40) && distance < 600f)
                {
                    CurrentState = AIState.SmallHop;
                    NPC.velocity.Y = -12f;
                    StateTimer = 0f;
                }
                else if (NPC.collideX)
                {
                    NPC.velocity.Y = -14f;
                }
            }

            CurrentState = AIState.Walking;
        }

        private void HandleSmallHop()
        {
            if (NPC.collideY && NPC.velocity.Y == 0f)
            {
                CurrentState = AIState.Walking;
                StateTimer = 0f;
            }
        }

        #region Attack 1: Minion Barrage
        private void HandleMinionBarrage(Player target)
        {
            NPC.velocity.X *= 0.9f;

            // All minions fire simultaneously in bursts
            if (StateTimer == 20f || StateTimer == 40f || StateTimer == 60f)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        int minionIndex = orbitingMinions[i];
                        if (minionIndex >= 0 && minionIndex < Main.maxProjectiles && Main.projectile[minionIndex].active)
                        {
                            Projectile minion = Main.projectile[minionIndex];
                            Vector2 shootDir = (target.Center - minion.Center).SafeNormalize(Vector2.UnitY);
                            
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), minion.Center, shootDir * 12f,
                                ModContent.ProjectileType<StolenValorMinionShot>(), 70, 2f, Main.myPlayer);
                        }
                    }
                }
                
                SoundEngine.PlaySound(SoundID.Item73 with { Volume = 0.7f }, NPC.Center);
            }

            if (StateTimer > 80f)
            {
                EndAttack(80f);
            }
        }
        #endregion

        #region Attack 2: Charging Formation
        private void HandleChargingFormation(Player target)
        {
            NPC.velocity.X *= 0.85f;

            // Minions charge in formation
            if (StateTimer == 30f)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        int minionIndex = orbitingMinions[i];
                        if (minionIndex >= 0 && minionIndex < Main.maxProjectiles && Main.projectile[minionIndex].active)
                        {
                            Projectile minion = Main.projectile[minionIndex];
                            
                            // Signal minion to charge
                            minion.ai[1] = 1f; // Charge mode
                            
                            // Spawn charging projectile from minion
                            Vector2 chargeDir = (target.Center - minion.Center).SafeNormalize(Vector2.UnitX);
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), minion.Center, chargeDir * 18f,
                                ModContent.ProjectileType<StolenValorChargeWave>(), 85, 4f, Main.myPlayer);
                        }
                    }
                }
                
                SoundEngine.PlaySound(SoundID.Item74, NPC.Center);

                for (int i = 0; i < 30; i++)
                {
                    Dust charge = Dust.NewDustDirect(NPC.Center, 1, 1, DustID.Torch, 0f, 0f, 100, EroicaRed, 2f);
                    charge.noGravity = true;
                    charge.velocity = Main.rand.NextVector2Circular(12f, 12f);
                }
            }

            if (StateTimer > 70f)
            {
                EndAttack(90f);
            }
        }
        #endregion

        #region Attack 3: Orbital Bombardment
        private void HandleOrbitalBombardment(Player target)
        {
            NPC.velocity.X *= 0.9f;

            // Minions spiral outward firing projectiles
            if (StateTimer >= 20f && StateTimer <= 80f && StateTimer % 6 == 0)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        int minionIndex = orbitingMinions[i];
                        if (minionIndex >= 0 && minionIndex < Main.maxProjectiles && Main.projectile[minionIndex].active)
                        {
                            Projectile minion = Main.projectile[minionIndex];
                            
                            // Fire outward from spiral position
                            Vector2 outward = (minion.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), minion.Center, outward * 10f,
                                ModContent.ProjectileType<StolenValorOrbitalShot>(), 65, 2f, Main.myPlayer);
                        }
                    }
                }

                if (StateTimer % 18 == 0)
                    SoundEngine.PlaySound(SoundID.Item45 with { Volume = 0.5f }, NPC.Center);
            }

            if (StateTimer > 90f)
            {
                EndAttack(100f);
            }
        }
        #endregion

        #region Attack 4: False Glory
        private void HandleFalseGlory(Player target)
        {
            NPC.velocity.X *= 0.9f;

            // Teleport minions around player creating cage
            if (StateTimer == 30f)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    // Spawn ring of projectiles around player
                    int projectileCount = 12;
                    for (int i = 0; i < projectileCount; i++)
                    {
                        float angle = MathHelper.TwoPi / projectileCount * i;
                        Vector2 pos = target.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 200f;
                        
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), pos, Vector2.Zero,
                            ModContent.ProjectileType<StolenValorCageOrb>(), 80, 2f, Main.myPlayer, target.whoAmI);
                    }
                }

                SoundEngine.PlaySound(SoundID.Item8, target.Center);

                // Teleport effect
                for (int i = 0; i < 40; i++)
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 pos = target.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 200f;
                    
                    Dust teleport = Dust.NewDustDirect(pos, 1, 1, DustID.Torch, 0f, 0f, 100, EroicaDark, 2f);
                    teleport.noGravity = true;
                }
            }

            if (StateTimer > 70f)
            {
                EndAttack(110f);
            }
        }
        #endregion

        #region Attack 5: Stolen Triumph
        private void HandleStolenTriumph(Player target)
        {
            // Windup
            if (StateTimer < 50f)
            {
                NPC.velocity *= 0.9f;

                if (StateTimer % 2 == 0)
                {
                    // All minions glow and gather energy
                    for (int i = 0; i < 5; i++)
                    {
                        int minionIndex = orbitingMinions[i];
                        if (minionIndex >= 0 && minionIndex < Main.maxProjectiles && Main.projectile[minionIndex].active)
                        {
                            Projectile minion = Main.projectile[minionIndex];
                            
                            for (int j = 0; j < 3; j++)
                            {
                                Dust gather = Dust.NewDustDirect(minion.Center + Main.rand.NextVector2Circular(30f, 30f), 1, 1, DustID.GoldFlame, 0f, 0f, 50, default, 1.5f);
                                gather.noGravity = true;
                                gather.velocity = (minion.Center - gather.position).SafeNormalize(Vector2.Zero) * 4f;
                            }
                        }
                    }
                }

                float pulse = (float)Math.Sin(StateTimer * 0.2f) * 0.5f + 1f;
                Lighting.AddLight(NPC.Center, 1.5f * pulse, 0.4f * pulse, 0.2f * pulse);
            }
            // Ultimate attack
            else if (StateTimer == 50f)
            {
                SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
                SoundEngine.PlaySound(SoundID.Item119, NPC.Center);

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    // Each minion creates an explosion
                    for (int i = 0; i < 5; i++)
                    {
                        int minionIndex = orbitingMinions[i];
                        if (minionIndex >= 0 && minionIndex < Main.maxProjectiles && Main.projectile[minionIndex].active)
                        {
                            Projectile minion = Main.projectile[minionIndex];
                            
                            // Explosion ring from each minion
                            int projCount = 12;
                            for (int j = 0; j < projCount; j++)
                            {
                                float angle = MathHelper.TwoPi / projCount * j;
                                Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 10f;
                                
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), minion.Center, velocity,
                                    ModContent.ProjectileType<StolenValorTriumphWave>(), 90, 3f, Main.myPlayer);
                            }

                            // Explosion effect at minion
                            for (int k = 0; k < 20; k++)
                            {
                                Dust explode = Dust.NewDustDirect(minion.Center, 1, 1, DustID.Torch, 0f, 0f, 100, EroicaRed, 2.5f);
                                explode.noGravity = true;
                                explode.velocity = Main.rand.NextVector2Circular(12f, 12f);
                            }
                        }
                    }

                    // Main boss also explodes
                    int mainCount = 24;
                    for (int i = 0; i < mainCount; i++)
                    {
                        float angle = MathHelper.TwoPi / mainCount * i;
                        Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 14f;
                        
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity,
                            ModContent.ProjectileType<StolenValorTriumphWave>(), 95, 4f, Main.myPlayer);
                    }
                }

                // Massive visual effect
                for (int i = 0; i < 60; i++)
                {
                    Dust explode = Dust.NewDustDirect(NPC.Center, 1, 1, DustID.Torch, 0f, 0f, 100, EroicaRed, 3.5f);
                    explode.noGravity = true;
                    explode.velocity = Main.rand.NextVector2Circular(20f, 20f);
                }
                for (int i = 0; i < 40; i++)
                {
                    Dust gold = Dust.NewDustDirect(NPC.Center, 1, 1, DustID.GoldFlame, 0f, 0f, 50, default, 3f);
                    gold.noGravity = true;
                    gold.velocity = Main.rand.NextVector2Circular(18f, 18f);
                }

                ThemedParticles.EroicaShockwave(NPC.Center, 3.5f);
            }

            if (StateTimer > 90f)
            {
                EndAttack(160f);
            }
        }
        #endregion

        private void EndAttack(float cooldown)
        {
            CurrentState = AIState.Walking;
            StateTimer = 0f;
            AttackCooldown = cooldown;
        }

        private void SpawnMinions()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            for (int i = 0; i < 5; i++)
            {
                float angle = (MathHelper.TwoPi / 5f) * i;
                int minion = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero,
                    ModContent.ProjectileType<StolenValorMinion>(), 70, 2f, Main.myPlayer, NPC.whoAmI, angle);

                if (minion < Main.maxProjectiles)
                {
                    orbitingMinions[i] = minion;
                }
            }
        }

        private void ManageOrbitingMinions()
        {
            for (int i = 0; i < 5; i++)
            {
                int minionIndex = orbitingMinions[i];
                bool needsRespawn = false;

                if (minionIndex < 0 || minionIndex >= Main.maxProjectiles)
                {
                    needsRespawn = true;
                }
                else
                {
                    Projectile minion = Main.projectile[minionIndex];
                    if (!minion.active || minion.type != ModContent.ProjectileType<StolenValorMinion>())
                    {
                        needsRespawn = true;
                    }
                }

                if (needsRespawn && Main.netMode != NetmodeID.MultiplayerClient && Main.rand.NextBool(150))
                {
                    float angle = (MathHelper.TwoPi / 5f) * i + orbitAngle;
                    int minion = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero,
                        ModContent.ProjectileType<StolenValorMinion>(), 70, 2f, Main.myPlayer, NPC.whoAmI, angle);

                    if (minion < Main.maxProjectiles)
                    {
                        orbitingMinions[i] = minion;
                    }
                }
            }
        }

        public override void FindFrame(int frameHeight)
        {
            int frameY = currentFrame / FrameColumns;
            NPC.frame.Y = frameY * frameHeight;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

            int frameWidth = texture.Width / FrameColumns;
            int frameHeight = texture.Height / FrameRows;
            int frameX = currentFrame % FrameColumns;
            int frameY = currentFrame / FrameColumns;

            Rectangle sourceRect = new Rectangle(frameX * frameWidth, frameY * frameHeight, frameWidth, frameHeight);
            Vector2 origin = new Vector2(frameWidth / 2f, frameHeight / 2f);

            // Enhanced glow
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.25f + 0.75f;
            Color glowColor = new Color(120, 20, 20, 0) * 0.55f * pulse;
            Color goldGlow = new Color(255, 200, 100, 0) * 0.3f * pulse;

            for (int i = 0; i < 4; i++)
            {
                Vector2 glowOffset = new Vector2(4f, 0f).RotatedBy(i * MathHelper.PiOver2);
                spriteBatch.Draw(texture, NPC.Center - screenPos + glowOffset + new Vector2(0f, NPC.gfxOffY + DrawOffsetY),
                    sourceRect, glowColor, NPC.rotation, origin, NPC.scale, NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
            }

            for (int i = 0; i < 4; i++)
            {
                Vector2 glowOffset = new Vector2(6f, 0f).RotatedBy(i * MathHelper.PiOver2 + MathHelper.PiOver4);
                spriteBatch.Draw(texture, NPC.Center - screenPos + glowOffset + new Vector2(0f, NPC.gfxOffY + DrawOffsetY),
                    sourceRect, goldGlow, NPC.rotation, origin, NPC.scale, NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
            }

            spriteBatch.Draw(texture, NPC.Center - screenPos + new Vector2(0f, NPC.gfxOffY + DrawOffsetY),
                sourceRect, drawColor, NPC.rotation, origin, NPC.scale, NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);

            return false;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            ThemedParticles.EroicaSparkles(NPC.Center, 4, NPC.width * 0.5f);
            ThemedParticles.EroicaSparks(NPC.Center, -hit.HitDirection * Vector2.UnitX, 4, 5f);

            if (NPC.life <= 0)
            {
                ThemedParticles.EroicaImpact(NPC.Center, 4f);
                ThemedParticles.EroicaShockwave(NPC.Center, 3f);

                for (int i = 0; i < 60; i++)
                {
                    Dust death = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Torch, 0f, 0f, 100, EroicaRed, 3.5f);
                    death.noGravity = true;
                    death.velocity = Main.rand.NextVector2Circular(16f, 16f);
                }
                for (int i = 0; i < 40; i++)
                {
                    Dust gold = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.GoldFlame, 0f, 0f, 50, default, 3f);
                    gold.noGravity = true;
                    gold.velocity = Main.rand.NextVector2Circular(14f, 14f);
                }

                SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
                SoundEngine.PlaySound(SoundID.NPCDeath43, NPC.Center);
            }
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ShardOfTriumphsTempo>(), 1, 5, 10));
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<EroicasResonantEnergy>(), 1, 8, 14));
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ResonantCoreOfEroica>(), 1, 3, 5));
            
            // Valor Essence - theme essence drop (15%)
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ValorEssence>(), 7));
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            // Mini-boss - 5% spawn rate during day in desert
            if (Main.dayTime &&
                NPC.downedMoonlord &&
                spawnInfo.Player.ZoneDesert &&
                !spawnInfo.PlayerSafe)
            {
                return 0.05f;
            }
            return 0f;
        }
    }

    #region Projectiles

    public class StolenValorMinionShot : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/MusicNote";

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.alpha = 100;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            if (Main.rand.NextBool(2))
            {
                Dust trail = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, new Color(200, 40, 40), 1.5f);
                trail.noGravity = true;
                trail.velocity *= 0.2f;
            }

            Lighting.AddLight(Projectile.Center, 0.6f, 0.15f, 0.1f);
        }

        public override void OnKill(int timeLeft)
        {
            // Small enemy projectile death
            DynamicParticleEffects.EroicaDeathHeroicFlash(Projectile.Center, 0.4f);
        }
    }

    public class StolenValorChargeWave : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/WholeNote";

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = false;
            Projectile.alpha = 100;
        }

        public override void AI()
        {
            Projectile.alpha += 4;
            if (Projectile.alpha >= 255)
                Projectile.Kill();

            Projectile.rotation = Projectile.velocity.ToRotation();

            for (int i = 0; i < 3; i++)
            {
                Dust wave = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, new Color(200, 40, 40), 2.5f);
                wave.noGravity = true;
                wave.velocity = Projectile.velocity * 0.1f;
            }

            Lighting.AddLight(Projectile.Center, 0.8f, 0.2f, 0.1f);
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }

    public class StolenValorOrbitalShot : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/CursiveMusicNote";

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 150;
            Projectile.tileCollide = true;
            Projectile.alpha = 100;
        }

        public override void AI()
        {
            Projectile.rotation += 0.2f;

            if (Main.rand.NextBool(2))
            {
                Dust trail = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GoldFlame, 0f, 0f, 50, default, 1.2f);
                trail.noGravity = true;
                trail.velocity *= 0.2f;
            }

            Lighting.AddLight(Projectile.Center, 0.6f, 0.4f, 0.1f);
        }

        public override void OnKill(int timeLeft)
        {
            // Small enemy orbital shot death
            DynamicParticleEffects.EroicaDeathGoldenGlow(Projectile.Center, 0.4f);
        }
    }

    public class StolenValorCageOrb : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/QuarterNote";

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = false;
            Projectile.alpha = 50;
        }

        public override void AI()
        {
            // Slowly close in on target
            if (Projectile.ai[0] >= 0 && Projectile.ai[0] < Main.maxPlayers)
            {
                Player target = Main.player[(int)Projectile.ai[0]];
                if (target.active && !target.dead)
                {
                    Vector2 toTarget = target.Center - Projectile.Center;
                    Projectile.velocity = toTarget.SafeNormalize(Vector2.Zero) * 2f;
                }
            }

            Projectile.rotation += 0.1f;

            for (int i = 0; i < 2; i++)
            {
                Dust cage = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, new Color(80, 20, 20), 1.5f);
                cage.noGravity = true;
                cage.velocity *= 0.2f;
            }

            Lighting.AddLight(Projectile.Center, 0.5f, 0.1f, 0.1f);

            Projectile.alpha += 2;
            if (Projectile.alpha >= 255)
                Projectile.Kill();
        }

        public override void OnKill(int timeLeft)
        {
            // Small enemy cage orb death
            DynamicParticleEffects.EroicaDeathCrimsonSpark(Projectile.Center, 0.4f);
        }
    }

    public class StolenValorTriumphWave : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/TallMusicNote";

        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 28;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = false;
            Projectile.alpha = 100;
        }

        public override void AI()
        {
            Projectile.alpha += 3;
            if (Projectile.alpha >= 255)
                Projectile.Kill();

            Projectile.rotation = Projectile.velocity.ToRotation();

            for (int i = 0; i < 3; i++)
            {
                Dust wave = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, new Color(200, 40, 40), 2.5f);
                wave.noGravity = true;
                wave.velocity = Projectile.velocity * 0.1f;
            }
            if (Main.rand.NextBool(2))
            {
                Dust gold = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GoldFlame, 0f, 0f, 50, default, 2f);
                gold.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, 1f, 0.35f, 0.12f);
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }

    #endregion
}
