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
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.Eroica.Enemies
{
    /// <summary>
    /// Funeral Blitzer - A desert mini-boss with 5 unique red and gold explosive attacks.
    /// Spawns in deserts at night at 5% rate after Moon Lord is defeated.
    /// 
    /// 5 ATTACKS:
    /// 1. Funeral Salvo - Rapid-fire explosive projectiles
    /// 2. Lightning Storm - Chain lightning strikes
    /// 3. Sorrow Bombs - Lobbed bombs that explode into fireballs
    /// 4. Dark Requiem - Homing soul projectiles
    /// 5. Final Ceremony - Massive barrage of all types
    /// </summary>
    public class FuneralBlitzer : ModNPC
    {
        private enum AIState
        {
            Idle,
            Chasing,
            FuneralSalvo,       // Attack 1
            LightningStorm,     // Attack 2
            SorrowBombs,        // Attack 3
            DarkRequiem,        // Attack 4
            FinalCeremony,      // Attack 5
            Retreating
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

        // Animation - 6x6 sprite sheet (36 frames)
        private int frameCounter = 0;
        private int currentFrame = 0;
        private const int FrameTime = 4;
        private const int FrameColumns = 6;
        private const int FrameRows = 6;
        private const int TotalFrames = 36;

        // Movement tracking
        private int lastSpriteDirection = 1;
        private bool isMoving = false;
        private float JumpCooldown = 0f;

        // Colors
        private static readonly Color EroicaRed = new Color(200, 40, 40);
        private static readonly Color EroicaGold = new Color(255, 200, 100);
        private static readonly Color EroicaDark = new Color(60, 20, 20);

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = TotalFrames;

            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Electrified] = true;

            NPCID.Sets.DangerDetectRange[Type] = 700;
        }

        public override void SetDefaults()
        {
            // MINI-BOSS STATS
            NPC.width = 237;
            NPC.height = 175;
            NPC.damage = 160;
            NPC.defense = 80;
            NPC.lifeMax = 65000; // Mini-boss HP
            NPC.HitSound = SoundID.NPCHit41;
            NPC.DeathSound = SoundID.NPCDeath43;
            NPC.knockBackResist = 0.05f;
            NPC.value = Item.buyPrice(gold: 40);
            NPC.aiStyle = -1;
            NPC.npcSlots = 5f;

            NPC.noGravity = false;
            NPC.noTileCollide = false;

            DrawOffsetY = -23f;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.NightTime,
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Desert,
                new FlavorTextBestiaryInfoElement("A harbinger of dark ceremonies who commands five devastating attacks. Its explosive projectiles and chain lightning make it a fearsome foe. The night desert echoes with its funeral dirge.")
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

            // Enhanced glow
            Lighting.AddLight(NPC.Center, 0.9f, 0.25f, 0.2f);

            // Ambient particles
            if (Main.rand.NextBool(8))
            {
                Dust dark = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Torch, 0f, 0f, 100, EroicaRed, 1.3f);
                dark.noGravity = true;
                dark.velocity *= 0.3f;
            }

            if (Main.rand.NextBool(12))
            {
                Dust electric = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Electric, 0f, 0f, 100, default, 0.8f);
                electric.noGravity = true;
                electric.velocity = Main.rand.NextVector2Circular(1.5f, 1.5f);
            }
            
            ThemedParticles.EroicaAura(NPC.Center, 25f);

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

            switch (CurrentState)
            {
                case AIState.Idle:
                case AIState.Chasing:
                    HandleChasing(target, distanceToTarget);
                    break;
                case AIState.FuneralSalvo:
                    HandleFuneralSalvo(target);
                    break;
                case AIState.LightningStorm:
                    HandleLightningStorm(target);
                    break;
                case AIState.SorrowBombs:
                    HandleSorrowBombs(target);
                    break;
                case AIState.DarkRequiem:
                    HandleDarkRequiem(target);
                    break;
                case AIState.FinalCeremony:
                    HandleFinalCeremony(target);
                    break;
                case AIState.Retreating:
                    HandleRetreating(target);
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

        private void HandleChasing(Player target, float distance)
        {
            float moveSpeed = 6f;
            float accel = 0.35f;

            if (distance > 80f)
            {
                if (target.Center.X > NPC.Center.X)
                    NPC.velocity.X = Math.Min(NPC.velocity.X + accel, moveSpeed);
                else
                    NPC.velocity.X = Math.Max(NPC.velocity.X - accel, -moveSpeed);
            }

            // Jump when blocked
            if (NPC.collideY && NPC.velocity.Y == 0f)
            {
                if (NPC.collideX && JumpCooldown <= 0f)
                {
                    NPC.velocity.Y = -12f;
                    JumpCooldown = 30f;
                }
                else if (Main.rand.NextBool(60) && JumpCooldown <= 0f)
                {
                    NPC.velocity.Y = -10f;
                    JumpCooldown = 30f;
                }
            }

            // Select attack
            if (AttackCooldown <= 0f && distance < 550f)
            {
                SelectNextAttack(target, distance);
            }

            CurrentState = AIState.Chasing;
        }

        private void SelectNextAttack(Player target, float distance)
        {
            AttackCounter++;
            int attackChoice = AttackCounter % 5;

            switch (attackChoice)
            {
                case 0: // Funeral Salvo
                    CurrentState = AIState.FuneralSalvo;
                    StateTimer = 0f;
                    SoundEngine.PlaySound(SoundID.Item73, NPC.Center);
                    break;

                case 1: // Lightning Storm
                    CurrentState = AIState.LightningStorm;
                    StateTimer = 0f;
                    SoundEngine.PlaySound(SoundID.Item122, NPC.Center);
                    break;

                case 2: // Sorrow Bombs
                    CurrentState = AIState.SorrowBombs;
                    StateTimer = 0f;
                    SoundEngine.PlaySound(SoundID.Item61, NPC.Center);
                    break;

                case 3: // Dark Requiem
                    CurrentState = AIState.DarkRequiem;
                    StateTimer = 0f;
                    SoundEngine.PlaySound(SoundID.Item8, NPC.Center);
                    break;

                case 4: // Final Ceremony - when low on health
                    if (NPC.life < NPC.lifeMax * 0.4f || Main.rand.NextBool(4))
                    {
                        CurrentState = AIState.FinalCeremony;
                        StateTimer = 0f;
                        SoundEngine.PlaySound(SoundID.Item119, NPC.Center);
                    }
                    else
                    {
                        CurrentState = AIState.FuneralSalvo;
                        StateTimer = 0f;
                    }
                    break;
            }
        }

        #region Attack 1: Funeral Salvo
        private void HandleFuneralSalvo(Player target)
        {
            NPC.velocity.X *= 0.9f;

            // Rapid fire 8 projectiles
            if (StateTimer >= 10f && StateTimer <= 70f && StateTimer % 8 == 0)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Vector2 shootDir = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY);
                    Vector2 velocity = shootDir * 13f + Main.rand.NextVector2Circular(2f, 2f);

                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity,
                        ModContent.ProjectileType<BlitzerSalvoProjectile>(), 75, 2f, Main.myPlayer);

                    SoundEngine.PlaySound(SoundID.Item73 with { Volume = 0.6f }, NPC.Center);

                    // Muzzle flash
                    for (int i = 0; i < 8; i++)
                    {
                        Dust flash = Dust.NewDustDirect(NPC.Center, 1, 1, DustID.Torch, 0f, 0f, 100, EroicaRed, 1.8f);
                        flash.noGravity = true;
                        flash.velocity = shootDir * 5f + Main.rand.NextVector2Circular(3f, 3f);
                    }
                }
            }

            if (StateTimer > 80f)
            {
                EndAttack(70f);
            }
        }
        #endregion

        #region Attack 2: Lightning Storm
        private void HandleLightningStorm(Player target)
        {
            NPC.velocity.X *= 0.85f;

            // Strike lightning at player location
            if (StateTimer == 20f || StateTimer == 40f || StateTimer == 60f || StateTimer == 80f)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    // Warning dust at target location
                    Vector2 strikePos = target.Center + new Vector2(Main.rand.NextFloat(-50f, 50f), 0);
                    
                    for (int i = 0; i < 20; i++)
                    {
                        Dust warning = Dust.NewDustDirect(strikePos - new Vector2(10, 10), 20, 20, DustID.Electric, 0f, -5f, 100, default, 1.5f);
                        warning.noGravity = true;
                    }

                    // Spawn lightning bolt from above
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), 
                        new Vector2(strikePos.X, target.Center.Y - 400f), 
                        new Vector2(0, 25f),
                        ModContent.ProjectileType<BlitzerLightningBolt>(), 90, 3f, Main.myPlayer);
                }
                
                SoundEngine.PlaySound(SoundID.Item122, target.Center);
            }

            if (StateTimer > 100f)
            {
                EndAttack(90f);
            }
        }
        #endregion

        #region Attack 3: Sorrow Bombs
        private void HandleSorrowBombs(Player target)
        {
            NPC.velocity.X *= 0.9f;

            // Lob bombs in arc
            if (StateTimer == 25f || StateTimer == 50f || StateTimer == 75f)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Vector2 toTarget = target.Center - NPC.Center;
                    float distance = toTarget.Length();
                    
                    // Calculate arc velocity
                    Vector2 velocity = new Vector2(
                        toTarget.X * 0.02f + (target.Center.X > NPC.Center.X ? 3f : -3f),
                        -12f
                    );

                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + new Vector2(0, -30), velocity,
                        ModContent.ProjectileType<BlitzerSorrowBomb>(), 85, 4f, Main.myPlayer);
                }
                
                SoundEngine.PlaySound(SoundID.Item61, NPC.Center);
                
                for (int i = 0; i < 15; i++)
                {
                    Dust launch = Dust.NewDustDirect(NPC.Center, 1, 1, DustID.Torch, 0f, -5f, 100, EroicaDark, 2f);
                    launch.noGravity = true;
                }
            }

            if (StateTimer > 90f)
            {
                EndAttack(80f);
            }
        }
        #endregion

        #region Attack 4: Dark Requiem
        private void HandleDarkRequiem(Player target)
        {
            NPC.velocity.X *= 0.9f;

            // Spawn homing soul projectiles
            if (StateTimer == 30f)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    int soulCount = 6;
                    for (int i = 0; i < soulCount; i++)
                    {
                        float angle = MathHelper.TwoPi / soulCount * i;
                        Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 50f;
                        Vector2 velocity = offset.SafeNormalize(Vector2.Zero) * 3f;

                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + offset, velocity,
                            ModContent.ProjectileType<BlitzerHomingSoul>(), 70, 2f, Main.myPlayer);
                    }
                }
                
                SoundEngine.PlaySound(SoundID.Item8, NPC.Center);

                // Soul spawn effect
                for (int i = 0; i < 30; i++)
                {
                    Dust soul = Dust.NewDustDirect(NPC.Center, 1, 1, DustID.Wraith, 0f, 0f, 100, EroicaDark, 2f);
                    soul.noGravity = true;
                    soul.velocity = Main.rand.NextVector2Circular(10f, 10f);
                }
            }

            if (StateTimer > 60f)
            {
                EndAttack(100f);
            }
        }
        #endregion

        #region Attack 5: Final Ceremony
        private void HandleFinalCeremony(Player target)
        {
            NPC.velocity *= 0.9f;

            // Windup
            if (StateTimer < 40f)
            {
                // Gathering dark energy
                if (StateTimer % 2 == 0)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                        float dist = Main.rand.NextFloat(80f, 150f);
                        Vector2 pos = NPC.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * dist;
                        Vector2 vel = (NPC.Center - pos).SafeNormalize(Vector2.Zero) * 6f;
                        
                        Dust gather = Dust.NewDustDirect(pos, 1, 1, DustID.Torch, vel.X, vel.Y, 100, EroicaRed, 1.5f);
                        gather.noGravity = true;
                    }
                }

                float pulse = (float)Math.Sin(StateTimer * 0.25f) * 0.5f + 1f;
                Lighting.AddLight(NPC.Center, 1.5f * pulse, 0.3f * pulse, 0.2f * pulse);
            }
            // Massive barrage
            else if (StateTimer >= 40f && StateTimer <= 120f)
            {
                if (StateTimer % 4 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Vector2 shootDir = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY);
                    
                    // Random spread
                    float spread = Main.rand.NextFloat(-0.4f, 0.4f);
                    Vector2 velocity = shootDir.RotatedBy(spread) * 14f;

                    // Alternate projectile types
                    int projType = Main.rand.Next(3) switch
                    {
                        0 => ModContent.ProjectileType<BlitzerSalvoProjectile>(),
                        1 => ModContent.ProjectileType<BlitzerHomingSoul>(),
                        _ => ModContent.ProjectileType<CenturionInfernoOrb>()
                    };

                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity,
                        projType, 80, 2f, Main.myPlayer);

                    if (Main.rand.NextBool(3))
                        SoundEngine.PlaySound(SoundID.Item73 with { Volume = 0.4f }, NPC.Center);
                }
            }

            if (StateTimer > 130f)
            {
                EndAttack(150f);
            }
        }
        #endregion

        private void HandleRetreating(Player target)
        {
            float retreatDir = target.Center.X > NPC.Center.X ? -1f : 1f;
            NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, retreatDir * 5f, 0.1f);

            if (StateTimer > 40f)
            {
                CurrentState = AIState.Chasing;
                StateTimer = 0f;
            }
        }

        private void EndAttack(float cooldown)
        {
            CurrentState = AIState.Retreating;
            StateTimer = 0f;
            AttackCooldown = cooldown;
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

            float drawScale = 0.42f;

            // Enhanced glow
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.25f + 0.75f;
            Color glowColor = new Color(200, 50, 40, 0) * 0.55f * pulse;

            SpriteEffects baseEffect = SpriteEffects.FlipHorizontally;

            for (int i = 0; i < 4; i++)
            {
                Vector2 glowOffset = new Vector2(5f, 0f).RotatedBy(i * MathHelper.PiOver2);
                spriteBatch.Draw(texture, drawPos + glowOffset, sourceRect, glowColor, NPC.rotation, origin, drawScale,
                    NPC.spriteDirection == 1 ? SpriteEffects.None : baseEffect, 0f);
            }

            spriteBatch.Draw(texture, drawPos, sourceRect, drawColor, NPC.rotation, origin, drawScale,
                NPC.spriteDirection == 1 ? SpriteEffects.None : baseEffect, 0f);

            return false;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int i = 0; i < 10; i++)
            {
                Dust hurt = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Torch, 0f, 0f, 100, EroicaRed, 1.8f);
                hurt.noGravity = true;
                hurt.velocity = Main.rand.NextVector2Circular(5f, 5f);
            }

            if (NPC.life <= 0)
            {
                ThemedParticles.EroicaImpact(NPC.Center, 3.5f);
                ThemedParticles.EroicaShockwave(NPC.Center, 2.5f);

                for (int i = 0; i < 50; i++)
                {
                    Dust death = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Torch, 0f, 0f, 100, EroicaRed, 3f);
                    death.noGravity = true;
                    death.velocity = Main.rand.NextVector2Circular(14f, 14f);
                }

                for (int i = 0; i < 25; i++)
                {
                    Dust lightning = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Electric, 0f, 0f, 100, default, 1.8f);
                    lightning.noGravity = true;
                    lightning.velocity = Main.rand.NextVector2Circular(12f, 12f);
                }

                SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
            }
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ShardOfTriumphsTempo>(), 1, 4, 8));
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<EroicasResonantEnergy>(), 1, 6, 12));
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ResonantCoreOfEroica>(), 1, 2, 4));
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            // Mini-boss - 5% spawn rate at night in desert
            if (!Main.dayTime &&
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

    public class BlitzerSalvoProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/EnergyFlare";

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
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
            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.5f }, Projectile.Center);
            for (int i = 0; i < 12; i++)
            {
                Dust explode = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, new Color(200, 40, 40), 2f);
                explode.noGravity = true;
                explode.velocity = Main.rand.NextVector2Circular(6f, 6f);
            }
        }
    }

    public class BlitzerLightningBolt : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/EnergyFlare";

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 80;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = false;
            Projectile.alpha = 50;
        }

        public override void AI()
        {
            for (int i = 0; i < 4; i++)
            {
                Dust lightning = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Electric, 0f, 0f, 100, default, 1.5f);
                lightning.noGravity = true;
                lightning.velocity *= 0.5f;
            }

            if (Main.rand.NextBool(2))
            {
                Dust red = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, new Color(200, 40, 40), 1.5f);
                red.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, 0.8f, 0.8f, 1f);
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }

    public class BlitzerSorrowBomb : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.alpha = 0;
        }

        public override void AI()
        {
            Projectile.velocity.Y += 0.25f;
            if (Projectile.velocity.Y > 16f)
                Projectile.velocity.Y = 16f;

            Projectile.rotation += Projectile.velocity.X * 0.05f;

            if (Main.rand.NextBool(3))
            {
                Dust smoke = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 150, Color.Black, 1.2f);
                smoke.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, 0.4f, 0.1f, 0.05f);
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);

            // Spawn fireballs on explosion
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                int fireballCount = 6;
                for (int i = 0; i < fireballCount; i++)
                {
                    float angle = MathHelper.TwoPi / fireballCount * i;
                    Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 6f;

                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity,
                        ModContent.ProjectileType<CenturionInfernoOrb>(), 60, 2f, Main.myPlayer);
                }
            }

            for (int i = 0; i < 25; i++)
            {
                Dust explode = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, new Color(60, 20, 20), 2.5f);
                explode.noGravity = true;
                explode.velocity = Main.rand.NextVector2Circular(10f, 10f);
            }
        }
    }

    public class BlitzerHomingSoul : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/EnergyFlare";

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
            Projectile.alpha = 100;
        }

        public override void AI()
        {
            // Home toward nearest player
            if (Projectile.ai[0] < 30f)
            {
                Projectile.ai[0]++;
                return;
            }

            Player target = Main.player[Player.FindClosest(Projectile.position, Projectile.width, Projectile.height)];
            if (target.active && !target.dead)
            {
                Vector2 toTarget = target.Center - Projectile.Center;
                toTarget = toTarget.SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 10f, 0.04f);
            }

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (Main.rand.NextBool(2))
            {
                Dust soul = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Wraith, 0f, 0f, 100, new Color(60, 20, 20), 1.3f);
                soul.noGravity = true;
                soul.velocity *= 0.2f;
            }

            Lighting.AddLight(Projectile.Center, 0.3f, 0.05f, 0.05f);
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 10; i++)
            {
                Dust death = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Wraith, 0f, 0f, 100, new Color(60, 20, 20), 1.5f);
                death.noGravity = true;
                death.velocity = Main.rand.NextVector2Circular(4f, 4f);
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(180, 40, 40, 100);
        }
    }

    #endregion
}
