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
    /// Behemoth of Valor - A massive desert mini-boss with 5 unique red and gold flaming attacks.
    /// Spawns in deserts during day at 5% rate after Moon Lord is defeated.
    /// 
    /// 5 ATTACKS:
    /// 1. Flame Rain - Rains black and red flames from the sky
    /// 2. Ground Slam - Massive stomp creating shockwaves
    /// 3. Infernal Breath - Cone of fire projectiles
    /// 4. Valor's Wrath - Summons fiery pillars around the arena
    /// 5. Apocalyptic Eruption - Massive explosion with debris
    /// </summary>
    public class BehemothOfValor : ModNPC
    {
        private enum AIState
        {
            Idle,
            Walking,
            Jumping,
            FlameRain,          // Attack 1
            GroundSlam,         // Attack 2
            InfernalBreath,     // Attack 3
            ValorsWrath,        // Attack 4
            ApocalypticEruption // Attack 5
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

        private float JumpCooldown = 0f;

        // Animation - 6x6 sprite sheet (36 frames)
        private int frameCounter = 0;
        private int currentFrame = 0;
        private const int FrameTime = 6;
        private const int FrameColumns = 6;
        private const int FrameRows = 6;
        private const int TotalFrames = 36;

        // Movement tracking
        private int lastSpriteDirection = -1;
        private bool isMoving = false;

        // Colors
        private static readonly Color EroicaRed = new Color(200, 40, 40);
        private static readonly Color EroicaGold = new Color(255, 200, 100);
        private static readonly Color EroicaDark = new Color(40, 15, 15);

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = TotalFrames;

            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Frostburn] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.CursedInferno] = true;

            NPCID.Sets.DangerDetectRange[Type] = 800;
        }

        public override void SetDefaults()
        {
            // MINI-BOSS STATS - Massive and tanky
            // Hitbox = (3780/6) × (2124/6) × 0.8 = 630 × 354 × 0.8 = 504 × 283
            NPC.width = 504;
            NPC.height = 283;
            NPC.damage = 200;
            NPC.defense = 100;
            NPC.lifeMax = 90000; // Highest HP mini-boss
            NPC.HitSound = SoundID.NPCHit41;
            NPC.DeathSound = SoundID.NPCDeath43;
            NPC.knockBackResist = 0f; // Immune to knockback
            NPC.value = Item.buyPrice(gold: 60);
            NPC.aiStyle = -1;
            NPC.npcSlots = 6f;

            NPC.noGravity = false;
            NPC.noTileCollide = false;

            DrawOffsetY = -98f;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.DayTime,
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Desert,
                new FlavorTextBestiaryInfoElement("A colossal manifestation of corrupted glory. Its five devastating attacks can reshape the battlefield. The very ground trembles at its approach, and its flames burn with the fury of a thousand warriors.")
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

            // Intense glow
            Lighting.AddLight(NPC.Center, 1f, 0.25f, 0.15f);

            // Ambient particles
            ThemedParticles.EroicaAura(NPC.Center, NPC.width * 0.7f);
            
            if (Main.rand.NextBool(6))
            {
                ThemedParticles.EroicaSparkles(NPC.Center, 3, NPC.width * 0.5f);
            }

            // Retarget
            NPC.TargetClosest(true);
            target = Main.player[NPC.target];

            float distanceToTarget = Vector2.Distance(NPC.Center, target.Center);

            // Update timers
            StateTimer++;
            if (AttackCooldown > 0f)
                AttackCooldown--;
            if (JumpCooldown > 0f)
                JumpCooldown--;

            // Select attack when cooldown done
            if (AttackCooldown <= 0f && distanceToTarget < 600f && 
                CurrentState != AIState.FlameRain && CurrentState != AIState.GroundSlam &&
                CurrentState != AIState.InfernalBreath && CurrentState != AIState.ValorsWrath &&
                CurrentState != AIState.ApocalypticEruption)
            {
                SelectNextAttack(target, distanceToTarget);
            }

            switch (CurrentState)
            {
                case AIState.Idle:
                case AIState.Walking:
                    HandleWalking(target, distanceToTarget);
                    break;
                case AIState.Jumping:
                    HandleJumping();
                    break;
                case AIState.FlameRain:
                    HandleFlameRain(target);
                    break;
                case AIState.GroundSlam:
                    HandleGroundSlam(target);
                    break;
                case AIState.InfernalBreath:
                    HandleInfernalBreath(target);
                    break;
                case AIState.ValorsWrath:
                    HandleValorsWrath(target);
                    break;
                case AIState.ApocalypticEruption:
                    HandleApocalypticEruption(target);
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
                case 0: // Flame Rain
                    CurrentState = AIState.FlameRain;
                    StateTimer = 0f;
                    SoundEngine.PlaySound(SoundID.Item88, NPC.Center);
                    break;

                case 1: // Ground Slam
                    CurrentState = AIState.GroundSlam;
                    StateTimer = 0f;
                    SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
                    break;

                case 2: // Infernal Breath
                    CurrentState = AIState.InfernalBreath;
                    StateTimer = 0f;
                    SoundEngine.PlaySound(SoundID.Item34, NPC.Center);
                    break;

                case 3: // Valor's Wrath
                    CurrentState = AIState.ValorsWrath;
                    StateTimer = 0f;
                    SoundEngine.PlaySound(SoundID.Item45, NPC.Center);
                    break;

                case 4: // Apocalyptic Eruption - when low health
                    if (NPC.life < NPC.lifeMax * 0.35f || Main.rand.NextBool(4))
                    {
                        CurrentState = AIState.ApocalypticEruption;
                        StateTimer = 0f;
                        SoundEngine.PlaySound(SoundID.Item119, NPC.Center);
                    }
                    else
                    {
                        CurrentState = AIState.FlameRain;
                        StateTimer = 0f;
                    }
                    break;
            }
        }

        private void HandleWalking(Player target, float distance)
        {
            float moveSpeed = 3f;
            float accel = 0.2f;

            if (distance > 80f)
            {
                if (target.Center.X > NPC.Center.X)
                    NPC.velocity.X = Math.Min(NPC.velocity.X + accel, moveSpeed);
                else
                    NPC.velocity.X = Math.Max(NPC.velocity.X - accel, -moveSpeed);
            }
            else
            {
                NPC.velocity.X *= 0.9f;
            }

            // Jump when blocked
            if (NPC.collideY && NPC.velocity.Y == 0f)
            {
                if (NPC.collideX && JumpCooldown <= 0f)
                {
                    CurrentState = AIState.Jumping;
                    NPC.velocity.Y = -16f;
                    JumpCooldown = 60f;
                    CreateStompEffect();
                }
                else if (Main.rand.NextBool(100) && JumpCooldown <= 0f)
                {
                    CurrentState = AIState.Jumping;
                    NPC.velocity.Y = -12f;
                    JumpCooldown = 60f;
                    CreateStompEffect();
                }
            }

            CurrentState = AIState.Walking;
        }

        private void HandleJumping()
        {
            if (NPC.collideY && NPC.velocity.Y == 0f)
            {
                CurrentState = AIState.Walking;
                StateTimer = 0f;
                CreateStompEffect();
            }
        }

        #region Attack 1: Flame Rain
        private void HandleFlameRain(Player target)
        {
            NPC.velocity.X *= 0.85f;

            // Rain flames for 90 frames
            if (StateTimer < 90f)
            {
                if (StateTimer % 4 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    float offsetX = Main.rand.NextFloat(-250f, 250f);
                    Vector2 spawnPos = new Vector2(target.Center.X + offsetX, target.Center.Y - 450f);
                    Vector2 velocity = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(10f, 16f));

                    Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPos, velocity,
                        ModContent.ProjectileType<BehemothFlameRain>(), 85, 1f, Main.myPlayer);
                }
            }
            else
            {
                EndAttack(100f);
            }
        }
        #endregion

        #region Attack 2: Ground Slam
        private void HandleGroundSlam(Player target)
        {
            if (StateTimer < 30f)
            {
                // Windup - slight jump
                if (StateTimer == 1f)
                {
                    NPC.velocity.Y = -8f;
                }
                NPC.velocity.X *= 0.9f;
            }
            else if (StateTimer == 30f)
            {
                // Slam down
                if (NPC.velocity.Y == 0f || NPC.collideY)
                {
                    SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
                    
                    // Create shockwaves
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        // Left and right shockwaves
                        for (int dir = -1; dir <= 1; dir += 2)
                        {
                            for (int i = 0; i < 8; i++)
                            {
                                float speed = 5f + i * 2f;
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Bottom, 
                                    new Vector2(dir * speed, -3f),
                                    ModContent.ProjectileType<BehemothShockwave>(), 95, 5f, Main.myPlayer);
                            }
                        }
                    }

                    // Massive stomp effect
                    CreateMassiveStompEffect();
                }
            }
            
            if (StateTimer > 60f)
            {
                EndAttack(120f);
            }
        }
        #endregion

        #region Attack 3: Infernal Breath
        private void HandleInfernalBreath(Player target)
        {
            NPC.velocity.X *= 0.9f;

            // Face target
            if (target.Center.X > NPC.Center.X)
                lastSpriteDirection = 1;
            else
                lastSpriteDirection = -1;
            NPC.spriteDirection = lastSpriteDirection;

            // Spray fire cone
            if (StateTimer >= 20f && StateTimer <= 80f && StateTimer % 3 == 0)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    float baseAngle = NPC.spriteDirection == 1 ? 0f : MathHelper.Pi;
                    float spread = (StateTimer - 50f) / 100f; // Oscillating spread
                    
                    for (int i = -2; i <= 2; i++)
                    {
                        float angle = baseAngle + i * 0.15f + spread * 0.3f;
                        Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 12f;

                        Projectile.NewProjectile(NPC.GetSource_FromAI(), 
                            NPC.Center + new Vector2(NPC.spriteDirection * 100f, 0),
                            velocity,
                            ModContent.ProjectileType<BehemothBreathFlame>(), 80, 2f, Main.myPlayer);
                    }
                }

                if (StateTimer % 9 == 0)
                    SoundEngine.PlaySound(SoundID.Item34 with { Volume = 0.6f }, NPC.Center);
            }

            if (StateTimer > 90f)
            {
                EndAttack(110f);
            }
        }
        #endregion

        #region Attack 4: Valor's Wrath
        private void HandleValorsWrath(Player target)
        {
            NPC.velocity.X *= 0.9f;

            // Summon pillars around the player
            if (StateTimer == 30f || StateTimer == 50f || StateTimer == 70f)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    int pillarCount = 6;
                    for (int i = 0; i < pillarCount; i++)
                    {
                        float angle = MathHelper.TwoPi / pillarCount * i + StateTimer / 60f;
                        float dist = 200f;
                        Vector2 pos = target.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * dist;

                        Projectile.NewProjectile(NPC.GetSource_FromAI(), pos, Vector2.Zero,
                            ModContent.ProjectileType<BehemothFirePillar>(), 90, 3f, Main.myPlayer);
                    }
                }

                SoundEngine.PlaySound(SoundID.Item45, target.Center);
            }

            if (StateTimer > 90f)
            {
                EndAttack(100f);
            }
        }
        #endregion

        #region Attack 5: Apocalyptic Eruption
        private void HandleApocalypticEruption(Player target)
        {
            // Windup
            if (StateTimer < 60f)
            {
                NPC.velocity *= 0.9f;

                if (StateTimer % 2 == 0)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                        float dist = Main.rand.NextFloat(100f, 250f);
                        Vector2 pos = NPC.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * dist;
                        Vector2 vel = (NPC.Center - pos).SafeNormalize(Vector2.Zero) * 10f;
                        
                        Dust gather = Dust.NewDustDirect(pos, 1, 1, DustID.Torch, vel.X, vel.Y, 100, EroicaRed, 2f);
                        gather.noGravity = true;
                    }
                }

                float pulse = (float)Math.Sin(StateTimer * 0.2f) * 0.5f + 1f;
                Lighting.AddLight(NPC.Center, 2.5f * pulse, 0.5f * pulse, 0.2f * pulse);
            }
            // Eruption
            else if (StateTimer == 60f)
            {
                SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
                SoundEngine.PlaySound(SoundID.Item119, NPC.Center);

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    // Massive explosion ring
                    int projectileCount = 32;
                    for (int i = 0; i < projectileCount; i++)
                    {
                        float angle = MathHelper.TwoPi / projectileCount * i;
                        Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 14f;
                        
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity,
                            ModContent.ProjectileType<BehemothEruptionWave>(), 100, 4f, Main.myPlayer);
                    }

                    // Rising debris
                    for (int i = 0; i < 12; i++)
                    {
                        float offsetX = Main.rand.NextFloat(-150f, 150f);
                        Vector2 pos = NPC.Bottom + new Vector2(offsetX, 0);
                        Vector2 vel = new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-18f, -12f));

                        Projectile.NewProjectile(NPC.GetSource_FromAI(), pos, vel,
                            ModContent.ProjectileType<BehemothDebris>(), 85, 3f, Main.myPlayer);
                    }
                }

                // Massive visual explosion
                for (int i = 0; i < 80; i++)
                {
                    Dust explode = Dust.NewDustDirect(NPC.Center, 1, 1, DustID.Torch, 0f, 0f, 100, EroicaRed, 4f);
                    explode.noGravity = true;
                    explode.velocity = Main.rand.NextVector2Circular(25f, 25f);
                }
                for (int i = 0; i < 50; i++)
                {
                    Dust gold = Dust.NewDustDirect(NPC.Center, 1, 1, DustID.GoldFlame, 0f, 0f, 50, default, 3.5f);
                    gold.noGravity = true;
                    gold.velocity = Main.rand.NextVector2Circular(22f, 22f);
                }

                ThemedParticles.EroicaShockwave(NPC.Center, 4f);
            }

            if (StateTimer > 100f)
            {
                EndAttack(180f); // Long cooldown after eruption
            }
        }
        #endregion

        private void EndAttack(float cooldown)
        {
            CurrentState = AIState.Walking;
            StateTimer = 0f;
            AttackCooldown = cooldown;
        }

        private void CreateStompEffect()
        {
            SoundEngine.PlaySound(SoundID.Item14, NPC.Center);

            for (int i = 0; i < 40; i++)
            {
                Dust stomp = Dust.NewDustDirect(NPC.BottomLeft - new Vector2(30f, 10f), NPC.width + 60, 20, DustID.Torch, 0f, 0f, 100, EroicaRed, 2.5f);
                stomp.velocity = new Vector2(Main.rand.NextFloat(-10f, 10f), Main.rand.NextFloat(-8f, -3f));
                stomp.noGravity = true;
            }

            for (int i = 0; i < 25; i++)
            {
                Dust smoke = Dust.NewDustDirect(NPC.BottomLeft - new Vector2(30f, 10f), NPC.width + 60, 20, DustID.Smoke, 0f, 0f, 150, Color.Black, 3f);
                smoke.velocity = new Vector2(Main.rand.NextFloat(-8f, 8f), Main.rand.NextFloat(-5f, -1f));
            }
        }

        private void CreateMassiveStompEffect()
        {
            SoundEngine.PlaySound(SoundID.Item14, NPC.Center);

            for (int i = 0; i < 60; i++)
            {
                Dust stomp = Dust.NewDustDirect(NPC.BottomLeft - new Vector2(50f, 10f), NPC.width + 100, 20, DustID.Torch, 0f, 0f, 100, EroicaRed, 3f);
                stomp.velocity = new Vector2(Main.rand.NextFloat(-15f, 15f), Main.rand.NextFloat(-12f, -5f));
                stomp.noGravity = true;
            }

            for (int i = 0; i < 40; i++)
            {
                Dust gold = Dust.NewDustDirect(NPC.BottomLeft - new Vector2(50f, 10f), NPC.width + 100, 20, DustID.GoldFlame, 0f, 0f, 50, default, 2.5f);
                gold.velocity = new Vector2(Main.rand.NextFloat(-12f, 12f), Main.rand.NextFloat(-10f, -4f));
                gold.noGravity = true;
            }

            ThemedParticles.EroicaShockwave(NPC.Bottom, 2.5f);
        }

        public override void FindFrame(int frameHeight)
        {
            int frameY = currentFrame / FrameColumns;
            NPC.frame.Y = frameY * frameHeight;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Npc[Type].Value;

            int frameWidth = texture.Width / FrameColumns;
            int frameHeight = texture.Height / FrameRows;
            int frameX = currentFrame % FrameColumns;
            int frameY = currentFrame / FrameColumns;

            Rectangle sourceRect = new Rectangle(frameX * frameWidth, frameY * frameHeight, frameWidth, frameHeight);
            Vector2 origin = new Vector2(frameWidth / 2f, frameHeight / 2f);
            Vector2 drawPos = NPC.Center - screenPos + new Vector2(0f, NPC.gfxOffY + DrawOffsetY);

            float drawScale = 0.75f;

            // Enhanced glow
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.04f) * 0.25f + 0.75f;
            Color glowColor = new Color(200, 40, 30, 0) * 0.65f * pulse;
            Color goldGlow = new Color(255, 200, 100, 0) * 0.35f * pulse;

            for (int i = 0; i < 5; i++)
            {
                Vector2 glowOffset = new Vector2(6f, 0f).RotatedBy(i * MathHelper.TwoPi / 5f);
                spriteBatch.Draw(texture, drawPos + glowOffset, sourceRect, glowColor, NPC.rotation, origin, drawScale,
                    NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
            }

            for (int i = 0; i < 5; i++)
            {
                Vector2 glowOffset = new Vector2(10f, 0f).RotatedBy(i * MathHelper.TwoPi / 5f + MathHelper.Pi / 5f);
                spriteBatch.Draw(texture, drawPos + glowOffset, sourceRect, goldGlow, NPC.rotation, origin, drawScale,
                    NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
            }

            spriteBatch.Draw(texture, drawPos, sourceRect, drawColor, NPC.rotation, origin, drawScale,
                NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);

            return false;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            ThemedParticles.EroicaSparkles(NPC.Center, 5, NPC.width * 0.5f);
            ThemedParticles.EroicaSparks(NPC.Center, -hit.HitDirection * Vector2.UnitX, 5, 6f);

            if (NPC.life <= 0)
            {
                ThemedParticles.EroicaImpact(NPC.Center, 5f);
                ThemedParticles.EroicaShockwave(NPC.Center, 4f);

                for (int i = 0; i < 70; i++)
                {
                    Dust death = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Torch, 0f, 0f, 100, EroicaRed, 4f);
                    death.noGravity = true;
                    death.velocity = Main.rand.NextVector2Circular(18f, 18f);
                }
                for (int i = 0; i < 50; i++)
                {
                    Dust gold = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.GoldFlame, 0f, 0f, 50, default, 3.5f);
                    gold.noGravity = true;
                    gold.velocity = Main.rand.NextVector2Circular(15f, 15f);
                }

                SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
                SoundEngine.PlaySound(SoundID.NPCDeath43, NPC.Center);
            }
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ShardOfTriumphsTempo>(), 1, 6, 12));
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<EroicasResonantEnergy>(), 1, 10, 18));
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ResonantCoreOfEroica>(), 1, 4, 7));
            
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

    public class BehemothFlameRain : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/ParticleTrail3";

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.alpha = 100;
        }

        public override void AI()
        {
            Projectile.velocity.Y += 0.15f;
            if (Projectile.velocity.Y > 16f)
                Projectile.velocity.Y = 16f;

            Projectile.rotation = Projectile.velocity.ToRotation();

            if (Main.rand.NextBool(2))
            {
                Dust flame = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, new Color(200, 40, 40), 1.5f);
                flame.noGravity = true;
                flame.velocity *= 0.3f;
            }

            if (Main.rand.NextBool(3))
            {
                Dust black = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 150, Color.Black, 1.2f);
                black.noGravity = true;
                black.velocity *= 0.2f;
            }
            
            // ☁EMUSICAL NOTATION - Subtle heroic melody trail (dimmer for enemy projectile)
            if (Main.rand.NextBool(10))
            {
                Color noteColor = Color.Lerp(new Color(200, 50, 50), new Color(255, 215, 0), Main.rand.NextFloat()) * 0.7f;
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -0.6f);
                ThemedParticles.MusicNote(Projectile.Center, noteVel, noteColor, 0.25f, 28);
            }

            Lighting.AddLight(Projectile.Center, 0.5f, 0.1f, 0.05f);
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.5f }, Projectile.Center);

            for (int i = 0; i < 15; i++)
            {
                Dust explode = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, new Color(200, 40, 40), 2f);
                explode.noGravity = true;
                explode.velocity = Main.rand.NextVector2Circular(6f, 6f);
            }

            for (int i = 0; i < 10; i++)
            {
                Dust smoke = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 150, Color.Black, 1.5f);
                smoke.velocity = Main.rand.NextVector2Circular(4f, 4f);
            }
            
            // ☁EMUSICAL FINALE - Hero's symphony (dimmer for enemy projectile)
            ThemedParticles.MusicNoteBurst(Projectile.Center, new Color(200, 50, 50) * 0.7f, 4, 3f);
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }

    public class BehemothShockwave : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/GlowingHalo4";

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = true;
            Projectile.alpha = 100;
        }

        public override void AI()
        {
            Projectile.velocity.Y += 0.5f;
            if (Projectile.velocity.Y > 10f)
                Projectile.velocity.Y = 10f;

            Projectile.alpha += 4;
            if (Projectile.alpha >= 255)
                Projectile.Kill();

            for (int i = 0; i < 3; i++)
            {
                Dust wave = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, -3f, 100, new Color(200, 40, 40), 2f);
                wave.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, 0.7f, 0.2f, 0.1f);
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }

    public class BehemothBreathFlame : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow4";

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = true;
            Projectile.alpha = 100;
        }

        public override void AI()
        {
            Projectile.velocity *= 0.98f;
            Projectile.alpha += 3;
            if (Projectile.alpha >= 255)
                Projectile.Kill();

            for (int i = 0; i < 2; i++)
            {
                Dust flame = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, new Color(200, 40, 40), 2f);
                flame.noGravity = true;
                flame.velocity *= 0.3f;
            }

            Lighting.AddLight(Projectile.Center, 0.6f, 0.15f, 0.08f);
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }

    public class BehemothFirePillar : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/GlowingHalo5";

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 120;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = false;
            Projectile.alpha = 50;
        }

        public override void AI()
        {
            if (Projectile.ai[0] == 0f)
            {
                // Warning phase
                Projectile.ai[0]++;
                if (Projectile.ai[0] < 20f)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Dust warning = Dust.NewDustDirect(Projectile.Center - new Vector2(10, 60), 20, 120, DustID.Torch, 0f, -2f, 100, new Color(200, 40, 40), 1.2f);
                        warning.noGravity = true;
                    }
                    return;
                }
            }

            // Active pillar
            for (int i = 0; i < 5; i++)
            {
                Dust pillar = Dust.NewDustDirect(Projectile.Center - new Vector2(20, 60), 40, 120, DustID.Torch, 0f, -5f, 100, new Color(200, 40, 40), 2.5f);
                pillar.noGravity = true;
            }
            if (Main.rand.NextBool(2))
            {
                Dust gold = Dust.NewDustDirect(Projectile.Center - new Vector2(20, 60), 40, 120, DustID.GoldFlame, 0f, -3f, 50, default, 2f);
                gold.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, 1f, 0.3f, 0.1f);

            Projectile.alpha += 2;
            if (Projectile.alpha >= 255)
                Projectile.Kill();
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }

    public class BehemothEruptionWave : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/StarBurst1";

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 100;
            Projectile.tileCollide = false;
            Projectile.alpha = 100;
        }

        public override void AI()
        {
            Projectile.alpha += 2;
            if (Projectile.alpha >= 255)
                Projectile.Kill();

            Projectile.rotation = Projectile.velocity.ToRotation();

            for (int i = 0; i < 4; i++)
            {
                Dust wave = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, new Color(200, 40, 40), 3f);
                wave.noGravity = true;
                wave.velocity = Projectile.velocity * 0.15f;
            }
            if (Main.rand.NextBool(2))
            {
                Dust gold = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GoldFlame, 0f, 0f, 50, default, 2.5f);
                gold.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, 1.2f, 0.4f, 0.15f);
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }

    public class BehemothDebris : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/StarBurst2";

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.alpha = 0;
        }

        public override void AI()
        {
            Projectile.velocity.Y += 0.35f;
            if (Projectile.velocity.Y > 18f)
                Projectile.velocity.Y = 18f;

            Projectile.rotation += Projectile.velocity.X * 0.04f;

            if (Main.rand.NextBool(2))
            {
                Dust trail = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, new Color(200, 40, 40), 1.5f);
                trail.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, 0.5f, 0.1f, 0.05f);
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.6f }, Projectile.Center);

            for (int i = 0; i < 15; i++)
            {
                Dust explode = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, new Color(200, 40, 40), 2f);
                explode.noGravity = true;
                explode.velocity = Main.rand.NextVector2Circular(8f, 8f);
            }
        }
    }

    #endregion
}
