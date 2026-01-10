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
    /// Funeral Blitzer - Medium speed enemy that shoots explosive projectiles.
    /// Projectiles explode into black/red flames and lightning.
    /// Spawns at night on desert surface at 20% rate after Moon Lord.
    /// </summary>
    public class FuneralBlitzer : ModNPC
    {
        private enum AIState
        {
            Idle,
            Chasing,
            Shooting,
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

        private float ShootCooldown
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
        private const int FrameTime = 4;
        private const int FrameColumns = 6;
        private const int FrameRows = 6;
        private const int TotalFrames = 36;

        // Shooting alternation
        private bool useProjectile1 = true;

        // Movement tracking for idle detection
        private int lastSpriteDirection = 1;
        private bool isMoving = false;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = TotalFrames;

            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Electrified] = true;

            NPCID.Sets.DangerDetectRange[Type] = 500;
        }

        public override void SetDefaults()
        {
            // Hitbox matches visual size: ~170px frame × 0.42f drawScale = ~71px
            NPC.width = 71;
            NPC.height = 71;
            NPC.damage = 115; // Higher than AbyssalMoonLurker (110)
            NPC.defense = 62;
            NPC.lifeMax = 20000; // Higher than AbyssalMoonLurker (18000)
            NPC.HitSound = SoundID.NPCHit41;
            NPC.DeathSound = SoundID.NPCDeath43;
            NPC.knockBackResist = 0.2f;
            NPC.value = Item.buyPrice(gold: 8);
            NPC.aiStyle = -1;

            NPC.noGravity = false;
            NPC.noTileCollide = false;

            DrawOffsetY = -23f; // Raised 0.5 blocks (8 pixels)
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.NightTime,
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Desert,
                new FlavorTextBestiaryInfoElement("A harbinger of dark ceremonies, the Funeral Blitzer rains explosive sorrow upon its foes. Its projectiles burst with flames of mourning and crackling despair.")
            });
        }

        public override void AI()
        {
            Player target = Main.player[NPC.target];

            // Check if moving
            float movementThreshold = 0.5f;
            isMoving = Math.Abs(NPC.velocity.X) > movementThreshold || Math.Abs(NPC.velocity.Y) > movementThreshold;

            // Face target - only update direction when moving
            if (isMoving)
            {
                if (target.Center.X > NPC.Center.X)
                    lastSpriteDirection = 1;
                else
                    lastSpriteDirection = -1;
            }
            NPC.spriteDirection = lastSpriteDirection;

            // Scarlet red glow\n            Lighting.AddLight(NPC.Center, 0.6f, 0.15f, 0.15f);\n\n            // Ambient particles - enhanced with themed and custom particles\n            if (Main.rand.NextBool(8))\n            {\n                Dust dark = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Torch, 0f, 0f, 100, Color.DarkRed, 1.1f);\n                dark.noGravity = true;\n                dark.velocity *= 0.3f;\n            }\n\n            if (Main.rand.NextBool(15))\n            {\n                Dust electric = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Electric, 0f, 0f, 100, default, 0.6f);\n                electric.noGravity = true;\n                electric.velocity = Main.rand.NextVector2Circular(1f, 1f);\n            }\n            \n            // Themed particle aura\n            if (Main.rand.NextBool(10))\n            {\n                ThemedParticles.EroicaAura(NPC.Center, 20f);\n            }\n            \n            // Custom particle glow\n            if (Main.rand.NextBool(12))\n            {\n                CustomParticles.EroicaFlare(NPC.Center + Main.rand.NextVector2Circular(15f, 15f), 0.2f);\n            }

            // Retarget
            NPC.TargetClosest(true);
            target = Main.player[NPC.target];

            float distanceToTarget = Vector2.Distance(NPC.Center, target.Center);

            // Update timers
            StateTimer++;
            if (ShootCooldown > 0f)
                ShootCooldown--;
            if (JumpCooldown > 0f)
                JumpCooldown--;

            switch (CurrentState)
            {
                case AIState.Idle:
                case AIState.Chasing:
                    HandleChasing(target, distanceToTarget);
                    break;

                case AIState.Shooting:
                    HandleShooting(target);
                    break;

                case AIState.Retreating:
                    HandleRetreating(target);
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

        private void HandleChasing(Player target, float distance)
        {
            // Medium speed
            float moveSpeed = 5f;
            float accel = 0.3f;

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
                    NPC.velocity.Y = -10f;
                    JumpCooldown = 30f;
                }
                else if (Main.rand.NextBool(80) && JumpCooldown <= 0f)
                {
                    NPC.velocity.Y = -8f;
                    JumpCooldown = 30f;
                }
            }

            // Enter shooting state when in range
            if (distance < 450f && distance > 150f && ShootCooldown <= 0f)
            {
                CurrentState = AIState.Shooting;
                StateTimer = 0f;
            }
            else
            {
                CurrentState = AIState.Chasing;
            }
        }

        private void HandleShooting(Player target)
        {
            // Slow down to shoot
            NPC.velocity.X *= 0.85f;

            if (StateTimer == 15f)
            {
                ShootProjectile(target);
            }
            else if (StateTimer == 35f)
            {
                ShootProjectile(target);
            }
            else if (StateTimer > 50f)
            {
                CurrentState = AIState.Retreating;
                StateTimer = 0f;
                ShootCooldown = 60f;
            }
        }

        private void HandleRetreating(Player target)
        {
            // Back away briefly
            float retreatDir = target.Center.X > NPC.Center.X ? -1f : 1f;
            NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, retreatDir * 4f, 0.1f);

            if (StateTimer > 30f)
            {
                CurrentState = AIState.Chasing;
                StateTimer = 0f;
            }
        }

        private void ShootProjectile(Player target)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            Vector2 shootDirection = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY);
            Vector2 velocity = shootDirection * 11f;

            // Alternate between projectile types
            int projType = useProjectile1 ?
                ModContent.ProjectileType<BlitzerProjectile1>() :
                ModContent.ProjectileType<BlitzerProjectile2>();

            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity,
                projType, 70, 2f, Main.myPlayer);

            useProjectile1 = !useProjectile1;

            SoundEngine.PlaySound(SoundID.Item73, NPC.Center);

            // Muzzle flash
            for (int i = 0; i < 10; i++)
            {
                Dust flash = Dust.NewDustDirect(NPC.Center, 1, 1, DustID.Torch, 0f, 0f, 100, Color.DarkRed, 1.5f);
                flash.noGravity = true;
                flash.velocity = shootDirection * 6f + Main.rand.NextVector2Circular(3f, 3f);
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

            float drawScale = 0.42f; // Scaled down further (was 0.65)

            // Flip sprite horizontally (frames are mirrored)
            SpriteEffects baseEffect = SpriteEffects.FlipHorizontally;

            // Scarlet red glow effect
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.2f + 0.8f;
            Color glowColor = new Color(180, 40, 40, 0) * 0.5f * pulse;

            for (int i = 0; i < 4; i++)
            {
                Vector2 glowOffset = new Vector2(4f, 0f).RotatedBy(i * MathHelper.PiOver2);
                spriteBatch.Draw(texture, drawPos + glowOffset, sourceRect, glowColor, NPC.rotation, origin, drawScale,
                    NPC.spriteDirection == 1 ? SpriteEffects.None : baseEffect, 0f);
            }

            // Main sprite
            spriteBatch.Draw(texture, drawPos, sourceRect, drawColor, NPC.rotation, origin, drawScale,
                NPC.spriteDirection == 1 ? SpriteEffects.None : baseEffect, 0f);

            return false;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int i = 0; i < 8; i++)
            {
                Dust hurt = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Torch, 0f, 0f, 100, Color.DarkRed, 1.5f);
                hurt.noGravity = true;
                hurt.velocity = Main.rand.NextVector2Circular(4f, 4f);
            }

            if (NPC.life <= 0)
            {
                for (int i = 0; i < 40; i++)
                {
                    Dust death = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Torch, 0f, 0f, 100, Color.DarkRed, 2.5f);
                    death.noGravity = true;
                    death.velocity = Main.rand.NextVector2Circular(12f, 12f);
                }

                for (int i = 0; i < 20; i++)
                {
                    Dust lightning = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Electric, 0f, 0f, 100, default, 1.5f);
                    lightning.noGravity = true;
                    lightning.velocity = Main.rand.NextVector2Circular(10f, 10f);
                }

                SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
            }
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            // Weakest tier - only drops Shard of Triumph's Tempo
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ShardOfTriumphsTempo>(), 1, 2, 4));
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            // Spawns at night on desert surface
            if (!Main.dayTime &&
                NPC.downedMoonlord &&
                spawnInfo.Player.ZoneDesert &&
                !spawnInfo.PlayerSafe &&
                spawnInfo.Player.ZoneOverworldHeight)
            {
                return 0.20f; // 20% spawn rate
            }
            return 0f;
        }
    }
}
