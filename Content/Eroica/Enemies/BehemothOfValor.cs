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
    /// Behemoth of Valor - A massive, slow Eroica enemy that rains down black and red flaming particles.
    /// Spawns in deserts during the day at 25% rate after Moon Lord is defeated.
    /// Sprite sheet faces LEFT - flip when moving right.
    /// </summary>
    public class BehemothOfValor : ModNPC
    {
        private enum AIState
        {
            Idle,
            Walking,
            Jumping,
            FlameRain
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

        private float FlameRainCooldown
        {
            get => NPC.ai[2];
            set => NPC.ai[2] = value;
        }

        private float JumpCooldown
        {
            get => NPC.ai[3];
            set => NPC.ai[3] = value;
        }

        // Animation - 6x6 sprite sheet (36 frames)
        private int frameCounter = 0;
        private int currentFrame = 0;
        private const int FrameTime = 6; // Slow animation for big creature
        private const int FrameColumns = 6;
        private const int FrameRows = 6;
        private const int TotalFrames = 36;

        // Movement tracking for idle detection
        private int lastSpriteDirection = -1; // Sprite faces LEFT by default
        private bool isMoving = false;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = TotalFrames;

            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Frostburn] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.CursedInferno] = true;

            NPCID.Sets.DangerDetectRange[Type] = 600;
        }

        public override void SetDefaults()
        {
            // Hitbox matches visual size: ~170px frame × 0.75f drawScale = ~127px
            NPC.width = 127;
            NPC.height = 127;
            NPC.damage = 135;
            NPC.defense = 75;
            NPC.lifeMax = 32000;
            NPC.HitSound = SoundID.NPCHit41;
            NPC.DeathSound = SoundID.NPCDeath43;
            NPC.knockBackResist = 0f; // Immune to knockback
            NPC.value = Item.buyPrice(gold: 12);
            NPC.aiStyle = -1;

            NPC.noGravity = false;
            NPC.noTileCollide = false;

            DrawOffsetY = -98f; // Raised 1.5 blocks (24 pixels) from -74
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.DayTime,
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Desert,
                new FlavorTextBestiaryInfoElement("A colossal manifestation of corrupted glory. Its mere presence scorches the earth with raining flames of black and crimson fury.")
            });
        }

        public override void AI()
        {
            Player target = Main.player[NPC.target];

            // Check if moving
            float movementThreshold = 0.5f;
            isMoving = Math.Abs(NPC.velocity.X) > movementThreshold || Math.Abs(NPC.velocity.Y) > movementThreshold;

            // Sprite faces LEFT by default - only update direction when moving
            if (isMoving)
            {
                if (target.Center.X > NPC.Center.X)
                    lastSpriteDirection = 1; // Moving right, flip the sprite
                else
                    lastSpriteDirection = -1; // Moving left, no flip
            }
            NPC.spriteDirection = lastSpriteDirection;

            // Dark red glow
            Lighting.AddLight(NPC.Center, 0.7f, 0.15f, 0.1f);

            // Themed ambient particles
            ThemedParticles.EroicaAura(NPC.Center, NPC.width * 0.6f);
            
            // Occasional sparkles
            if (Main.rand.NextBool(8))
            {
                ThemedParticles.EroicaSparkles(NPC.Center, 2, NPC.width * 0.5f);
            }

            // Retarget
            NPC.TargetClosest(true);
            target = Main.player[NPC.target];

            float distanceToTarget = Vector2.Distance(NPC.Center, target.Center);

            // Update timers
            StateTimer++;
            if (FlameRainCooldown > 0f)
                FlameRainCooldown--;
            if (JumpCooldown > 0f)
                JumpCooldown--;

            // Trigger flame rain when player is near
            if (distanceToTarget < 500f && FlameRainCooldown <= 0f && CurrentState != AIState.FlameRain)
            {
                CurrentState = AIState.FlameRain;
                StateTimer = 0f;
                FlameRainCooldown = 180f; // 3 second cooldown
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
            }

            // Animation update - only animate when moving
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
                // Idle - show first frame
                currentFrame = 0;
                frameCounter = 0;
            }
        }

        private void HandleWalking(Player target, float distance)
        {
            // Very slow movement
            float moveSpeed = 2.5f;
            float accel = 0.15f;

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

            // Jump when blocked or occasionally
            if (NPC.collideY && NPC.velocity.Y == 0f)
            {
                if (NPC.collideX && JumpCooldown <= 0f)
                {
                    CurrentState = AIState.Jumping;
                    NPC.velocity.Y = -14f; // Big jump
                    JumpCooldown = 60f;

                    // Ground stomp effect
                    CreateStompEffect();
                }
                else if (Main.rand.NextBool(120) && JumpCooldown <= 0f)
                {
                    CurrentState = AIState.Jumping;
                    NPC.velocity.Y = -10f;
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

                // Landing stomp
                CreateStompEffect();
            }
        }

        private void HandleFlameRain(Player target)
        {
            // Slow down during attack
            NPC.velocity.X *= 0.8f;

            // Create flame rain over 60 ticks
            if (StateTimer < 60f)
            {
                if (StateTimer % 3 == 0)
                {
                    CreateFlameRainParticle(target);
                }
            }
            else
            {
                CurrentState = AIState.Walking;
                StateTimer = 0f;
            }
        }

        private void CreateFlameRainParticle(Player target)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            // Spawn flames above the player area
            float offsetX = Main.rand.NextFloat(-200f, 200f);
            Vector2 spawnPos = new Vector2(target.Center.X + offsetX, target.Center.Y - 400f);

            // Black and red flame projectile raining down
            Vector2 velocity = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(8f, 14f));

            Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPos, velocity,
                ModContent.ProjectileType<BehemothFlameRain>(), 75, 1f, Main.myPlayer);
        }

        private void CreateStompEffect()
        {
            SoundEngine.PlaySound(SoundID.Item14, NPC.Center);

            // Big dust explosion
            for (int i = 0; i < 30; i++)
            {
                Dust stomp = Dust.NewDustDirect(NPC.BottomLeft - new Vector2(20f, 10f), NPC.width + 40, 20, DustID.Torch, 0f, 0f, 100, Color.DarkRed, 2f);
                stomp.velocity = new Vector2(Main.rand.NextFloat(-8f, 8f), Main.rand.NextFloat(-6f, -2f));
                stomp.noGravity = true;
            }

            for (int i = 0; i < 20; i++)
            {
                Dust smoke = Dust.NewDustDirect(NPC.BottomLeft - new Vector2(20f, 10f), NPC.width + 40, 20, DustID.Smoke, 0f, 0f, 150, Color.Black, 2.5f);
                smoke.velocity = new Vector2(Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-4f, -1f));
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

            float drawScale = 0.75f; // Scaled down 50%

            // Scarlet red glow effect
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.04f) * 0.2f + 0.8f;
            Color glowColor = new Color(180, 30, 30, 0) * 0.6f * pulse;

            for (int i = 0; i < 5; i++)
            {
                Vector2 glowOffset = new Vector2(5f, 0f).RotatedBy(i * MathHelper.TwoPi / 5f);
                spriteBatch.Draw(texture, drawPos + glowOffset, sourceRect, glowColor, NPC.rotation, origin, drawScale,
                    NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
            }

            // Main sprite
            spriteBatch.Draw(texture, drawPos, sourceRect, drawColor, NPC.rotation, origin, drawScale,
                NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);

            return false;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            // Hit sparks
            ThemedParticles.EroicaSparkles(NPC.Center, 4, NPC.width * 0.4f);
            ThemedParticles.EroicaSparks(NPC.Center, -hit.HitDirection * Vector2.UnitX, 4, 5f);

            if (NPC.life <= 0)
            {
                // Death explosion
                ThemedParticles.EroicaImpact(NPC.Center, 3f);
                ThemedParticles.EroicaShockwave(NPC.Center, 2f);
                ThemedParticles.SakuraPetals(NPC.Center, 20, NPC.width);

                SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
            }
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            // Second strongest - drops all 3 but less than Stolen Valor
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ShardOfTriumphsTempo>(), 1, 3, 6));
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<EroicasResonantEnergy>(), 1, 3, 6));
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ResonantCoreOfEroica>(), 1, 1, 3));
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (Main.dayTime &&
                NPC.downedMoonlord &&
                spawnInfo.Player.ZoneDesert &&
                !spawnInfo.PlayerSafe &&
                spawnInfo.Player.ZoneOverworldHeight)
            {
                return 0.25f; // 25% spawn rate
            }
            return 0f;
        }
    }

    /// <summary>
    /// Flame rain projectile spawned by Behemoth of Valor
    /// </summary>
    public class BehemothFlameRain : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Flames;

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
            // Gravity
            Projectile.velocity.Y += 0.15f;
            if (Projectile.velocity.Y > 16f)
                Projectile.velocity.Y = 16f;

            // Rotation
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Black and red flame trail
            if (Main.rand.NextBool(2))
            {
                Dust flame = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, Color.DarkRed, 1.5f);
                flame.noGravity = true;
                flame.velocity *= 0.3f;
            }

            if (Main.rand.NextBool(3))
            {
                Dust black = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 150, Color.Black, 1.2f);
                black.noGravity = true;
                black.velocity *= 0.2f;
            }

            Lighting.AddLight(Projectile.Center, 0.5f, 0.1f, 0.05f);
        }

        public override void OnKill(int timeLeft)
        {
            // Explosion on impact
            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.5f }, Projectile.Center);

            for (int i = 0; i < 15; i++)
            {
                Dust explode = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, Color.DarkRed, 2f);
                explode.noGravity = true;
                explode.velocity = Main.rand.NextVector2Circular(6f, 6f);
            }

            for (int i = 0; i < 10; i++)
            {
                Dust smoke = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 150, Color.Black, 1.5f);
                smoke.velocity = Main.rand.NextVector2Circular(4f, 4f);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Draw as flame particles instead of texture
            return false;
        }
    }
}
