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
using MagnumOpus.Common;

namespace MagnumOpus.Content.Eroica.Enemies
{
    /// <summary>
    /// Stolen Valor - A slow, heavy Eroica enemy that spawns in deserts during the day after Moon Lord is defeated.
    /// Has 3 orbiting minions that can shoot at the player and be sent charging.
    /// </summary>
    public class StolenValor : ModNPC
    {
        // AI States
        private enum AIState
        {
            Idle,
            Walking,
            SmallHop,
            SendingMinion,
            Attacking
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

        // Minion tracking - now using projectile indices
        private int[] orbitingMinions = new int[3] { -1, -1, -1 };
        private float orbitAngle = 0f;

        // Animation - 6x6 sprite sheet (36 frames)
        private int frameCounter = 0;
        private int currentFrame = 0;
        private const int FrameTime = 5; // Ticks per frame (slower animation)
        private const int FrameColumns = 6;
        private const int FrameRows = 6;
        private const int TotalFrames = 36;

        // Movement tracking for idle detection
        private Vector2 lastPosition = Vector2.Zero;
        private int lastSpriteDirection = 1;
        private bool isMoving = false;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = TotalFrames; // 36 frames for animation

            // Immune to most debuffs
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Frostburn] = true;

            // Make it count as a post-Moon Lord enemy
            NPCID.Sets.DangerDetectRange[Type] = 500;
        }

        public override void SetDefaults()
        {
            // Much tougher than Moonlight enemies
            NPC.width = 65;
            NPC.height = 86;
            NPC.damage = 140; // Higher than Lunus (90) and AbyssalMoonLurker (110)
            NPC.defense = 80; // Very tanky
            NPC.lifeMax = 28000; // More health than Moonlight enemies
            NPC.HitSound = SoundID.NPCHit41; // Metallic sound
            NPC.DeathSound = SoundID.NPCDeath43;
            NPC.knockBackResist = 0.02f; // Almost immune to knockback
            NPC.value = Item.buyPrice(gold: 10); // Good coin drop
            NPC.aiStyle = -1; // Custom AI
            NPC.scale = 1.15f; // 15% size increase

            // Has gravity, walks on ground
            NPC.noGravity = false;
            NPC.noTileCollide = false;

            // Visual offset to align sprite with hitbox (larger value = draw higher to prevent clipping)
            DrawOffsetY = -30f;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.DayTime,
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Desert,
                new FlavorTextBestiaryInfoElement("A twisted manifestation that wears the guise of heroism, but lacks its true spirit. Its hollow triumph is guarded by three dark echoes.")
            });
        }

        public override void AI()
        {
            Player target = Main.player[NPC.target];

            // Check if moving
            float movementThreshold = 0.5f;
            isMoving = Math.Abs(NPC.velocity.X) > movementThreshold || Math.Abs(NPC.velocity.Y) > movementThreshold;

            // Only update sprite direction when moving
            if (isMoving)
            {
                if (target.Center.X > NPC.Center.X)
                    lastSpriteDirection = 1;
                else
                    lastSpriteDirection = -1;
            }
            NPC.spriteDirection = lastSpriteDirection;

            lastPosition = NPC.Center;

            // Dark red/black glow
            Lighting.AddLight(NPC.Center, 0.5f, 0.1f, 0.1f);

            // Dark particles
            if (Main.rand.NextBool(8))
            {
                Dust dark = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Torch, 0f, 0f, 100, Color.DarkRed, 1.0f);
                dark.noGravity = true;
                dark.velocity *= 0.3f;
            }

            // Spawn minions on first tick
            if (StateTimer == 0f && CurrentState == AIState.Idle)
            {
                SpawnMinions();
                StateTimer = 1f;
            }

            // Manage orbiting minions (like Moonlight enemies)
            ManageOrbitingMinions();

            // Update orbit angle for minions
            orbitAngle += 0.015f; // Slower orbit (like the minions)
            if (orbitAngle > MathHelper.TwoPi)
                orbitAngle -= MathHelper.TwoPi;

            float distanceToTarget = Vector2.Distance(NPC.Center, target.Center);

            // Update timers
            StateTimer++;
            if (AttackCooldown > 0f)
                AttackCooldown--;

            // Retarget if needed
            NPC.TargetClosest(true);
            target = Main.player[NPC.target];

            // Fast aggressive AI - faster than Moonlight enemies
            if (CurrentState == AIState.Idle || CurrentState == AIState.Walking)
            {
                // Fast aggressive movement toward player
                float moveSpeed = 5.5f; // Much faster than Moonlight enemies (Lunus is ~3-4)
                float accel = 0.35f;
                
                if (distanceToTarget > 60f)
                {
                    if (target.Center.X > NPC.Center.X)
                        NPC.velocity.X = Math.Min(NPC.velocity.X + accel, moveSpeed);
                    else
                        NPC.velocity.X = Math.Max(NPC.velocity.X - accel, -moveSpeed);
                }
                else
                {
                    NPC.velocity.X *= 0.85f; // Slight slowdown when close
                }

                // Aggressive hop frequently or when blocked
                if (NPC.collideY && NPC.velocity.Y == 0f)
                {
                    if (Main.rand.NextBool(40) && distanceToTarget < 600f)
                    {
                        CurrentState = AIState.SmallHop;
                        NPC.velocity.Y = -10f; // Higher hop
                        StateTimer = 0f;
                    }
                    // Jump over obstacles quickly
                    else if (NPC.collideX)
                    {
                        NPC.velocity.Y = -12f;
                    }
                }

                CurrentState = AIState.Walking;
            }
            else if (CurrentState == AIState.SmallHop)
            {
                // Just wait for landing
                if (NPC.collideY && NPC.velocity.Y == 0f)
                {
                    CurrentState = AIState.Walking;
                    StateTimer = 0f;
                }
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

        private void SpawnMinions()
        {
            // Only spawn on server/single player
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            // Spawn 3 minion projectiles orbiting this enemy
            for (int i = 0; i < 3; i++)
            {
                float angle = (MathHelper.TwoPi / 3f) * i;
                int minion = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero,
                    ModContent.ProjectileType<StolenValorMinion>(), 60, 2f, Main.myPlayer, NPC.whoAmI, angle);

                if (minion < Main.maxProjectiles)
                {
                    orbitingMinions[i] = minion;
                }
            }
        }

        private void ManageOrbitingMinions()
        {
            // Check if minions are still alive and respawn if needed
            for (int i = 0; i < 3; i++)
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

                // Respawn missing minions (less frequently for balance)
                if (needsRespawn && Main.netMode != NetmodeID.MultiplayerClient && Main.rand.NextBool(180))
                {
                    float angle = (MathHelper.TwoPi / 3f) * i + orbitAngle;
                    int minion = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero,
                        ModContent.ProjectileType<StolenValorMinion>(), 60, 2f, Main.myPlayer, NPC.whoAmI, angle);

                    if (minion < Main.maxProjectiles)
                    {
                        orbitingMinions[i] = minion;
                    }
                }
            }
        }

        public override void FindFrame(int frameHeight)
        {
            // Calculate frame from 6x6 sprite sheet
            int frameX = currentFrame % FrameColumns;
            int frameY = currentFrame / FrameColumns;
            NPC.frame.Y = frameY * frameHeight;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

            // Calculate frame dimensions
            int frameWidth = texture.Width / FrameColumns;
            int frameHeight = texture.Height / FrameRows;
            int frameX = currentFrame % FrameColumns;
            int frameY = currentFrame / FrameColumns;

            Rectangle sourceRect = new Rectangle(frameX * frameWidth, frameY * frameHeight, frameWidth, frameHeight);
            Vector2 origin = new Vector2(frameWidth / 2f, frameHeight / 2f);

            // Dark red glow effect
            Color glowColor = new Color(100, 0, 0, 0) * 0.5f;
            for (int i = 0; i < 4; i++)
            {
                Vector2 glowOffset = new Vector2(3f, 0f).RotatedBy(i * MathHelper.PiOver2);
                spriteBatch.Draw(texture, NPC.Center - screenPos + glowOffset + new Vector2(0f, NPC.gfxOffY + DrawOffsetY),
                    sourceRect, glowColor, NPC.rotation, origin, NPC.scale, NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
            }

            // Draw main sprite
            spriteBatch.Draw(texture, NPC.Center - screenPos + new Vector2(0f, NPC.gfxOffY + DrawOffsetY),
                sourceRect, drawColor, NPC.rotation, origin, NPC.scale, NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);

            return false;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            // Hurt particles
            for (int i = 0; i < 8; i++)
            {
                Dust hurt = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Torch, 0f, 0f, 100, Color.DarkRed, 1.5f);
                hurt.noGravity = true;
                hurt.velocity = Main.rand.NextVector2Circular(4f, 4f);
            }

            // Death effect
            if (NPC.life <= 0)
            {
                // Dramatic dark red explosion
                for (int i = 0; i < 40; i++)
                {
                    Dust death = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Torch, 0f, 0f, 100, Color.DarkRed, 2.5f);
                    death.noGravity = true;
                    death.velocity = Main.rand.NextVector2Circular(12f, 12f);
                }

                // Black smoke
                for (int i = 0; i < 25; i++)
                {
                    Dust smoke = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Smoke, 0f, 0f, 150, Color.Black, 2.0f);
                    smoke.velocity = Main.rand.NextVector2Circular(8f, 8f);
                }
            }
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            // Drops Shard of Triumph's Tempo (new drop)
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ShardOfTriumphsTempo>(), 1, 3, 6));

            // Drops Eroica's Resonant Energy
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<EroicasResonantEnergy>(), 1, 4, 8));

            // Drops Resonant Cores of Eroica
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ResonantCoreOfEroica>(), 1, 2, 4));
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            // Only spawn in deserts during day after Moon Lord is defeated
            if (Main.dayTime &&
                NPC.downedMoonlord &&
                spawnInfo.Player.ZoneDesert &&
                !spawnInfo.PlayerSafe &&
                spawnInfo.Player.ZoneOverworldHeight)
            {
                return 0.08f; // Pretty uncommon (8% spawn rate)
            }
            return 0f;
        }
    }
}
