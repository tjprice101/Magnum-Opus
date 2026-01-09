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
using MagnumOpus.Content.Eroica.Enemies;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.Eroica.Enemies
{
    /// <summary>
    /// Eroican Centurion - A fast, aggressive enemy with flaming charges.
    /// Has 3 floating lanterns that orbit and shoot gold/red sword projectiles.
    /// Spawns anywhere in desert at any time at 15% rate after Moon Lord.
    /// </summary>
    public class EroicanCenturion : ModNPC
    {
        private enum AIState
        {
            Idle,
            Chasing,
            FlamingCharge,
            Recovering
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

        private float ChargeCooldown
        {
            get => NPC.ai[2];
            set => NPC.ai[2] = value;
        }

        // Lantern tracking
        private int[] orbitingLanterns = new int[3] { -1, -1, -1 };
        private float orbitAngle = 0f;

        // Animation - 6x6 sprite sheet (36 frames)
        private int frameCounter = 0;
        private int currentFrame = 0;
        private const int FrameTime = 4;
        private const int FrameColumns = 6;
        private const int FrameRows = 6;
        private const int TotalFrames = 36;

        // Charge parameters
        private Vector2 chargeDirection = Vector2.Zero;
        private bool isCharging = false;

        // Movement tracking for idle detection
        private int lastSpriteDirection = 1;
        private bool isMoving = false;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = TotalFrames;

            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Frostburn] = true;

            NPCID.Sets.DangerDetectRange[Type] = 550;
        }

        public override void SetDefaults()
        {
            NPC.width = 27;
            NPC.height = 32;
            NPC.damage = 125;
            NPC.defense = 68;
            NPC.lifeMax = 25000;
            NPC.HitSound = SoundID.NPCHit41;
            NPC.DeathSound = SoundID.NPCDeath43;
            NPC.knockBackResist = 0.1f;
            NPC.value = Item.buyPrice(gold: 10);
            NPC.aiStyle = -1;

            NPC.noGravity = false;
            NPC.noTileCollide = false;

            DrawOffsetY = -52f; // Fix 2 blocks ground clipping
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Desert,
                new FlavorTextBestiaryInfoElement("An elite warrior consumed by corrupted valor. Its blazing charges leave trails of scorched earth, while spectral lanterns rain fiery judgment upon all who oppose it.")
            });
        }

        public override void AI()
        {
            Player target = Main.player[NPC.target];

            // Check if moving
            float movementThreshold = 0.5f;
            isMoving = Math.Abs(NPC.velocity.X) > movementThreshold || Math.Abs(NPC.velocity.Y) > movementThreshold;

            // Face direction of movement or target - only update when moving
            if (isCharging)
            {
                lastSpriteDirection = chargeDirection.X > 0 ? 1 : -1;
            }
            else if (isMoving)
            {
                if (target.Center.X > NPC.Center.X)
                    lastSpriteDirection = 1;
                else
                    lastSpriteDirection = -1;
            }
            NPC.spriteDirection = lastSpriteDirection;

            // Scarlet red glow
            Lighting.AddLight(NPC.Center, 0.8f, 0.2f, 0.15f);

            // Themed ambient particles
            ThemedParticles.EroicaAura(NPC.Center, NPC.width * 0.6f);
            
            if (Main.rand.NextBool(10))
            {
                ThemedParticles.EroicaSparkles(NPC.Center, 2, NPC.width * 0.5f);
            }

            // Spawn lanterns on first tick
            if (StateTimer == 0f && CurrentState == AIState.Idle)
            {
                SpawnLanterns();
                StateTimer = 1f;
            }

            // Manage orbiting lanterns
            ManageOrbitingLanterns();

            // Update orbit angle
            orbitAngle += 0.025f;
            if (orbitAngle > MathHelper.TwoPi)
                orbitAngle -= MathHelper.TwoPi;

            // Retarget
            NPC.TargetClosest(true);
            target = Main.player[NPC.target];

            float distanceToTarget = Vector2.Distance(NPC.Center, target.Center);

            // Update timers
            StateTimer++;
            if (ChargeCooldown > 0f)
                ChargeCooldown--;

            switch (CurrentState)
            {
                case AIState.Idle:
                case AIState.Chasing:
                    HandleChasing(target, distanceToTarget);
                    break;

                case AIState.FlamingCharge:
                    HandleFlamingCharge(target);
                    break;

                case AIState.Recovering:
                    HandleRecovering();
                    break;
            }

            // Animation update - only animate when moving or charging
            if (isMoving || isCharging)
            {
                frameCounter++;
                int animSpeed = isCharging ? 2 : FrameTime;
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
        }

        private void HandleChasing(Player target, float distance)
        {
            isCharging = false;

            // Fast aggressive movement
            float moveSpeed = 7f;
            float accel = 0.4f;

            if (distance > 60f)
            {
                if (target.Center.X > NPC.Center.X)
                    NPC.velocity.X = Math.Min(NPC.velocity.X + accel, moveSpeed);
                else
                    NPC.velocity.X = Math.Max(NPC.velocity.X - accel, -moveSpeed);
            }

            // Jump when blocked or to reach player
            if (NPC.collideY && NPC.velocity.Y == 0f)
            {
                if (NPC.collideX)
                {
                    NPC.velocity.Y = -12f;
                }
                else if (target.Center.Y < NPC.Center.Y - 100f && Main.rand.NextBool(30))
                {
                    NPC.velocity.Y = -14f;
                }
                else if (Main.rand.NextBool(60))
                {
                    NPC.velocity.Y = -8f;
                }
            }

            // Initiate flaming charge when in range
            if (distance < 400f && distance > 100f && ChargeCooldown <= 0f && NPC.velocity.Y == 0f)
            {
                CurrentState = AIState.FlamingCharge;
                StateTimer = 0f;
                chargeDirection = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
                isCharging = true;

                // Charge wind-up effect
                SoundEngine.PlaySound(SoundID.Item74, NPC.Center);
                for (int i = 0; i < 20; i++)
                {
                    Dust charge = Dust.NewDustDirect(NPC.Center, 1, 1, DustID.Torch, 0f, 0f, 100, Color.DarkRed, 2f);
                    charge.noGravity = true;
                    charge.velocity = Main.rand.NextVector2Circular(8f, 8f);
                }
            }

            CurrentState = AIState.Chasing;
        }

        private void HandleFlamingCharge(Player target)
        {
            isCharging = true;

            if (StateTimer < 45f)
            {
                // Fast charge
                float chargeSpeed = 16f;
                NPC.velocity = chargeDirection * chargeSpeed;
                NPC.velocity.Y += 0.3f; // Slight gravity

                // Flaming trail
                if (Main.rand.NextBool(2))
                {
                    Dust flame = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Torch, 0f, 0f, 100, Color.DarkRed, 2.5f);
                    flame.noGravity = true;
                    flame.velocity = -NPC.velocity * 0.2f + Main.rand.NextVector2Circular(2f, 2f);
                }

                if (Main.rand.NextBool(2))
                {
                    Dust gold = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.GoldFlame, 0f, 0f, 50, default, 2f);
                    gold.noGravity = true;
                    gold.velocity = -NPC.velocity * 0.15f;
                }

                // Stop if hit wall
                if (NPC.collideX || NPC.collideY)
                {
                    CurrentState = AIState.Recovering;
                    StateTimer = 0f;
                    ChargeCooldown = 90f;
                    isCharging = false;

                    // Impact effect
                    SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
                    for (int i = 0; i < 25; i++)
                    {
                        Dust impact = Dust.NewDustDirect(NPC.Center, 1, 1, DustID.Torch, 0f, 0f, 100, Color.DarkRed, 2f);
                        impact.noGravity = true;
                        impact.velocity = Main.rand.NextVector2Circular(10f, 10f);
                    }
                }
            }
            else
            {
                // End charge
                CurrentState = AIState.Recovering;
                StateTimer = 0f;
                ChargeCooldown = 90f;
                isCharging = false;
            }
        }

        private void HandleRecovering()
        {
            isCharging = false;
            NPC.velocity.X *= 0.9f;

            if (StateTimer > 30f)
            {
                CurrentState = AIState.Chasing;
                StateTimer = 0f;
            }
        }

        private void SpawnLanterns()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            for (int i = 0; i < 3; i++)
            {
                float angle = (MathHelper.TwoPi / 3f) * i;
                int lantern = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero,
                    ModContent.ProjectileType<CenturionLantern>(), 55, 2f, Main.myPlayer, NPC.whoAmI, angle);

                if (lantern < Main.maxProjectiles)
                {
                    orbitingLanterns[i] = lantern;
                }
            }
        }

        private void ManageOrbitingLanterns()
        {
            for (int i = 0; i < 3; i++)
            {
                int lanternIndex = orbitingLanterns[i];
                bool needsRespawn = false;

                if (lanternIndex < 0 || lanternIndex >= Main.maxProjectiles)
                {
                    needsRespawn = true;
                }
                else
                {
                    Projectile lantern = Main.projectile[lanternIndex];
                    if (!lantern.active || lantern.type != ModContent.ProjectileType<CenturionLantern>())
                    {
                        needsRespawn = true;
                    }
                }

                if (needsRespawn && Main.netMode != NetmodeID.MultiplayerClient && Main.rand.NextBool(150))
                {
                    float angle = (MathHelper.TwoPi / 3f) * i + orbitAngle;
                    int lantern = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero,
                        ModContent.ProjectileType<CenturionLantern>(), 55, 2f, Main.myPlayer, NPC.whoAmI, angle);

                    if (lantern < Main.maxProjectiles)
                    {
                        orbitingLanterns[i] = lantern;
                    }
                }
            }
        }

        public override void FindFrame(int frameHeight)
        {
            int frameX = currentFrame % FrameColumns;
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

            float drawScale = 0.45f; // Scaled down further (was 0.7)

            // Scarlet red glow effect
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.25f + 0.75f;
            Color glowColor = new Color(200, 40, 40, 0) * 0.5f * pulse;

            // Sprite faces LEFT by default - flip when spriteDirection is 1 (facing right)
            SpriteEffects effects = NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            for (int i = 0; i < 4; i++)
            {
                Vector2 glowOffset = new Vector2(4f, 0f).RotatedBy(i * MathHelper.PiOver2);
                spriteBatch.Draw(texture, drawPos + glowOffset, sourceRect, glowColor, NPC.rotation, origin, drawScale,
                    effects, 0f);
            }

            // Main sprite
            spriteBatch.Draw(texture, drawPos, sourceRect, drawColor, NPC.rotation, origin, drawScale,
                effects, 0f);

            return false;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            // Hit sparks
            ThemedParticles.EroicaSparkles(NPC.Center, 3, NPC.width * 0.4f);
            ThemedParticles.EroicaSparks(NPC.Center, -hit.HitDirection * Vector2.UnitX, 3, 4f);

            if (NPC.life <= 0)
            {
                // Death explosion
                ThemedParticles.EroicaImpact(NPC.Center, 2.5f);
                ThemedParticles.EroicaShockwave(NPC.Center, 1.8f);
                ThemedParticles.EroicaSparkles(NPC.Center, 15, NPC.width);

                SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
            }
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            // Third tier - only drops Shard and Core
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ShardOfTriumphsTempo>(), 1, 2, 5));
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ResonantCoreOfEroica>(), 1, 1, 2));
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            // Spawns anywhere in desert at any time
            if (NPC.downedMoonlord &&
                spawnInfo.Player.ZoneDesert &&
                !spawnInfo.PlayerSafe)
            {
                return 0.15f; // 15% spawn rate
            }
            return 0f;
        }
    }
}
