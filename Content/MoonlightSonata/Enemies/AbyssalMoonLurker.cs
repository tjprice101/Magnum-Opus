using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.MoonlightSonata.ResonantOres;
using MagnumOpus.Content.MoonlightSonata.ResonanceEnergies;

namespace MagnumOpus.Content.MoonlightSonata.Enemies
{
    /// <summary>
    /// Abyssal Moon Lurker - A powerful lunar creature that spawns underground after Moon Lord is defeated.
    /// Predatory AI with stalking, ambush rushes, and fluid repositioning.
    /// </summary>
    public class AbyssalMoonLurker : ModNPC
    {
        // AI States - Predatory and stalking
        private enum AIState
        {
            Idle,
            Stalking,         // Slow approach, watching
            AmbushRush,       // Sudden burst toward player
            Flanking,         // Circle to side/behind
            Repositioning,    // Quick repositioning
            Jumping,
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

        private float JumpCooldown
        {
            get => NPC.ai[3];
            set => NPC.ai[3] = value;
        }

        // Fluid movement variables
        private float stalkOffset = 0f;
        private int flankDirection = 1;
        private float flankAngle = 0f;
        private Vector2 ambushDirection = Vector2.Zero;

        // Animation variables for 6x6 sprite sheet
        private int frameCounter = 0;
        private int currentFrame = 0;
        private const int FrameTime = 4;
        private const int FrameColumns = 6;
        private const int FrameRows = 6;
        private const int TotalFrames = 36;

        // Attack alternation
        private int attackPattern = 0;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = TotalFrames; // 36 frames for 6x6 sprite sheet
            
            // Immune to common debuffs
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Darkness] = true;

            NPCID.Sets.DangerDetectRange[Type] = 500;
        }

        public override void SetDefaults()
        {
            // Tougher than other Moonlight enemies
            NPC.width = 44;
            NPC.height = 44;
            NPC.damage = 110; // Higher than Lunus (90)
            NPC.defense = 60; // Higher defense
            NPC.lifeMax = 18000; // Double health for challenge
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath6;
            NPC.knockBackResist = 0.1f; // Very resistant to knockback
            NPC.value = Item.buyPrice(gold: 8); // Better coin drop
            NPC.aiStyle = -1;
            
            // Has gravity
            NPC.noGravity = false;
            NPC.noTileCollide = false;
            
            // Visual offset to align sprite with hitbox
            DrawOffsetY = -28f;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Underground,
                new FlavorTextBestiaryInfoElement("A terrifying predator that lurks in the deepest caverns, infused with abyssal moonlight. Its power rivals that of celestial beings.")
            });
        }

        public override void AI()
        {
            Player target = Main.player[NPC.target];
            
            // Target validation
            if (!target.active || target.dead)
            {
                NPC.TargetClosest(true);
                target = Main.player[NPC.target];
                
                if (!target.active || target.dead)
                {
                    NPC.velocity.Y += 0.1f;
                    NPC.timeLeft = Math.Min(NPC.timeLeft, 60);
                    return;
                }
            }

            // Add ambient lighting - eerie deep purple/white
            Lighting.AddLight(NPC.Center, 0.6f, 0.5f, 0.8f);

            // White shimmer particles
            if (Main.rand.NextBool(5))
            {
                Dust shimmer = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.SparksMech, 0f, 0f, 0, Color.White, 1.2f);
                shimmer.noGravity = true;
                shimmer.velocity *= 0.3f;
            }

            // Purple/blue ambient particles
            if (Main.rand.NextBool(8))
            {
                int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.IceTorch;
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, dustType, 0f, 0f, 100, default, 0.9f);
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }

            float distanceToTarget = Vector2.Distance(NPC.Center, target.Center);
            bool canSeeTarget = Collision.CanHitLine(NPC.Center, 1, 1, target.Center, 1, 1);

            if (AttackCooldown > 0) AttackCooldown--;
            if (JumpCooldown > 0) JumpCooldown--;

            switch (CurrentState)
            {
                case AIState.Idle:
                    HandleIdleState(target, distanceToTarget, canSeeTarget);
                    break;
                case AIState.Stalking:
                    HandleStalkingState(target, distanceToTarget, canSeeTarget);
                    break;
                case AIState.AmbushRush:
                    HandleAmbushRushState(target, distanceToTarget);
                    break;
                case AIState.Flanking:
                    HandleFlankingState(target, distanceToTarget, canSeeTarget);
                    break;
                case AIState.Repositioning:
                    HandleRepositioningState(target, distanceToTarget);
                    break;
                case AIState.Jumping:
                    HandleJumpingState(target);
                    break;
                case AIState.Attacking:
                    HandleAttackingState(target);
                    break;
            }

            if (NPC.velocity.X > 0.5f)
                NPC.spriteDirection = 1;
            else if (NPC.velocity.X < -0.5f)
                NPC.spriteDirection = -1;
        }

        private void HandleIdleState(Player target, float distance, bool canSee)
        {
            StateTimer++;
            stalkOffset += 0.03f;

            // Slow, menacing wander
            if (StateTimer % 80 == 0 && NPC.velocity.Y == 0)
            {
                NPC.velocity.X = Main.rand.NextFloat(-1.5f, 1.5f);
            }

            // Subtle swaying
            if (NPC.velocity.Y == 0)
            {
                NPC.velocity.X += (float)Math.Sin(stalkOffset) * 0.05f;
                NPC.velocity.X *= 0.97f;
            }

            // Spot prey - begin stalking
            if (canSee && distance < 700f)
            {
                CurrentState = AIState.Stalking;
                StateTimer = 0;
                flankDirection = Main.rand.NextBool() ? 1 : -1;
                
                // Menacing alert burst - white and purple
                for (int i = 0; i < 20; i++)
                {
                    int dustType = Main.rand.NextBool(3) ? DustID.SparksMech : (Main.rand.NextBool() ? DustID.PurpleTorch : DustID.IceTorch);
                    Dust alert = Dust.NewDustDirect(NPC.Center, 1, 1, dustType, 0f, 0f, 0, dustType == DustID.SparksMech ? Color.White : default, 1.5f);
                    alert.noGravity = true;
                    alert.velocity = Main.rand.NextVector2Circular(6f, 6f);
                }
            }
        }

        private void HandleStalkingState(Player target, float distance, bool canSee)
        {
            StateTimer++;
            stalkOffset += 0.06f;

            // Predatory slow approach with weaving
            float stalkSpeed = 3.5f;
            float waveInfluence = (float)Math.Sin(stalkOffset * 0.8f) * 1.5f;

            float dirToPlayer = target.Center.X > NPC.Center.X ? 1f : -1f;
            float targetVelX = dirToPlayer * stalkSpeed + waveInfluence;
            
            NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, targetVelX, 0.06f);

            // Suddenly rush at player (ambush)
            if (distance < 350f && distance > 100f && Main.rand.NextBool(40) && NPC.velocity.Y == 0)
            {
                CurrentState = AIState.AmbushRush;
                StateTimer = 0;
                ambushDirection = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
                
                // Ambush telegraph - quick flash
                for (int i = 0; i < 12; i++)
                {
                    Dust rush = Dust.NewDustDirect(NPC.Center, 1, 1, DustID.SparksMech, 0f, 0f, 0, Color.White, 2f);
                    rush.noGravity = true;
                    rush.velocity = Main.rand.NextVector2Circular(4f, 4f);
                }
            }

            // Start flanking to get around player
            if (distance < 300f && Main.rand.NextBool(80) && NPC.velocity.Y == 0)
            {
                CurrentState = AIState.Flanking;
                StateTimer = 0;
                flankAngle = (float)Math.Atan2(NPC.Center.Y - target.Center.Y, NPC.Center.X - target.Center.X);
                flankDirection = Main.rand.NextBool() ? 1 : -1;
            }

            // Only jump when target is significantly above
            bool targetAbove = target.Center.Y < NPC.Center.Y - 130f;

            if (NPC.velocity.Y == 0 && JumpCooldown <= 0 && targetAbove)
            {
                float jumpPower = -20f;
                NPC.velocity.Y = jumpPower;
                JumpCooldown = 45;
                CurrentState = AIState.Jumping;
                StateTimer = 0;

                for (int i = 0; i < 10; i++)
                {
                    Dust jumpDust = Dust.NewDustDirect(NPC.BottomLeft, NPC.width, 4, DustID.Smoke, 0f, 0f, 100, default, 1.5f);
                    jumpDust.velocity = new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-2f, 0f));
                }
            }

            // Attack from further range
            if (distance < 600f && canSee && AttackCooldown <= 0)
            {
                CurrentState = AIState.Attacking;
                StateTimer = 0;
            }

            if (distance > 1500f || !canSee)
            {
                StateTimer++;
                if (StateTimer > 240)
                {
                    CurrentState = AIState.Idle;
                    StateTimer = 0;
                }
            }
        }

        private void HandleAmbushRushState(Player target, float distance)
        {
            StateTimer++;

            if (StateTimer < 10)
            {
                // Brief crouch before rush
                NPC.velocity.X *= 0.75f;
                
                // Gathering dark energy
                for (int i = 0; i < 4; i++)
                {
                    Vector2 offset = Main.rand.NextVector2Circular(60f, 30f);
                    int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.SparksMech;
                    Dust charge = Dust.NewDustDirect(NPC.Center + offset, 1, 1, dustType, 0f, 0f, dustType == DustID.SparksMech ? 0 : 100, dustType == DustID.SparksMech ? Color.White : default, 1.4f);
                    charge.noGravity = true;
                    charge.velocity = (NPC.Center - charge.position) * 0.15f;
                }
            }
            else if (StateTimer == 10)
            {
                // AMBUSH! Explosive rush
                float rushDir = target.Center.X > NPC.Center.X ? 1f : -1f;
                NPC.velocity.X = rushDir * 20f;
                NPC.velocity.Y = -4f; // Slight hop for menacing effect
                
                // Explosive burst
                for (int i = 0; i < 25; i++)
                {
                    int dustType = Main.rand.NextBool(3) ? DustID.SparksMech : DustID.PurpleTorch;
                    Dust burst = Dust.NewDustDirect(NPC.Center, 1, 1, dustType, 0f, 0f, dustType == DustID.SparksMech ? 0 : 100, dustType == DustID.SparksMech ? Color.White : default, 2f);
                    burst.noGravity = true;
                    burst.velocity = new Vector2(-rushDir * Main.rand.NextFloat(2f, 6f), Main.rand.NextFloat(-3f, 3f));
                }
                
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item71, NPC.Center);
            }
            else if (StateTimer > 10 && StateTimer < 30)
            {
                // Maintain rush with slight deceleration
                NPC.velocity.X *= 0.96f;
                
                // Trail
                if (StateTimer % 2 == 0)
                {
                    Dust trail = Dust.NewDustDirect(NPC.Center, 1, 1, DustID.PurpleTorch, 0f, 0f, 100, default, 1.3f);
                    trail.noGravity = true;
                    trail.velocity = -NPC.velocity * 0.08f;
                }
            }
            else if (StateTimer >= 30)
            {
                // End rush - pick next action
                float newDist = Vector2.Distance(NPC.Center, target.Center);
                
                if (newDist < 150f)
                {
                    // Close - attack!
                    if (AttackCooldown <= 0)
                    {
                        CurrentState = AIState.Attacking;
                    }
                    else
                    {
                        CurrentState = AIState.Repositioning;
                    }
                }
                else if (Main.rand.NextBool())
                {
                    CurrentState = AIState.Flanking;
                    flankAngle = (float)Math.Atan2(NPC.Center.Y - target.Center.Y, NPC.Center.X - target.Center.X);
                }
                else
                {
                    CurrentState = AIState.Stalking;
                }
                StateTimer = 0;
            }
        }

        private void HandleFlankingState(Player target, float distance, bool canSee)
        {
            StateTimer++;
            flankAngle += 0.05f * flankDirection;
            
            // Circle around the player predatorily
            float flankRadius = 200f;
            
            Vector2 targetPos = target.Center + new Vector2(
                (float)Math.Cos(flankAngle) * flankRadius,
                0
            );

            float dirX = targetPos.X > NPC.Center.X ? 1f : -1f;
            NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, dirX * 5.5f, 0.09f);

            // Attack while flanking
            if (canSee && AttackCooldown <= 0 && Main.rand.NextBool(35))
            {
                CurrentState = AIState.Attacking;
                StateTimer = 0;
                return;
            }

            // Sudden ambush from flank
            if (StateTimer > 60 && Main.rand.NextBool(50))
            {
                CurrentState = AIState.AmbushRush;
                StateTimer = 0;
                return;
            }

            // Exit flanking
            if (StateTimer > 120 || Main.rand.NextBool(150))
            {
                int choice = Main.rand.Next(3);
                if (choice == 0)
                    CurrentState = AIState.AmbushRush;
                else if (choice == 1)
                    CurrentState = AIState.Repositioning;
                else
                    CurrentState = AIState.Stalking;
                StateTimer = 0;
            }
        }

        private void HandleRepositioningState(Player target, float distance)
        {
            StateTimer++;
            stalkOffset += 0.1f;

            // Quick repositioning away then back
            float dirAway = NPC.Center.X > target.Center.X ? 1f : -1f;
            float repoSpeed = 8f + (float)Math.Sin(stalkOffset * 1.5f) * 2f;
            
            NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, dirAway * repoSpeed, 0.15f);

            // Return to stalking/flanking
            if (StateTimer > 35 || distance > 450f)
            {
                if (Main.rand.NextBool())
                {
                    CurrentState = AIState.Flanking;
                    flankAngle = (float)Math.Atan2(NPC.Center.Y - target.Center.Y, NPC.Center.X - target.Center.X);
                }
                else
                {
                    CurrentState = AIState.Stalking;
                }
                StateTimer = 0;
            }
        }

        private void HandleJumpingState(Player target)
        {
            StateTimer++;
            stalkOffset += 0.1f;

            float airControl = 0.2f;
            float waveInfluence = (float)Math.Sin(stalkOffset) * 0.5f;
            
            if (target.Center.X > NPC.Center.X)
                NPC.velocity.X += airControl + waveInfluence * 0.1f;
            else
                NPC.velocity.X -= airControl + waveInfluence * 0.1f;

            NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -7.2f, 7.2f);

            if (NPC.velocity.Y == 0 && StateTimer > 10)
            {
                CurrentState = AIState.Stalking;
                StateTimer = 0;
            }
        }

        private void HandleAttackingState(Player target)
        {
            StateTimer++;
            stalkOffset += 0.04f;

            // Slow down but maintain predatory sway
            NPC.velocity.X *= 0.88f;
            NPC.velocity.X += (float)Math.Sin(stalkOffset * 2.5f) * 0.4f;

            if (StateTimer < 35)
            {
                if (StateTimer % 5 == 0)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        Vector2 offset = Main.rand.NextVector2Circular(50f, 50f);
                        int dustType = Main.rand.NextBool(3) ? DustID.SparksMech : (Main.rand.NextBool() ? DustID.PurpleTorch : DustID.IceTorch);
                        Dust charge = Dust.NewDustDirect(NPC.Center + offset, 1, 1, dustType, 0f, 0f, 0, dustType == DustID.SparksMech ? Color.White : default, 1.4f);
                        charge.noGravity = true;
                        charge.velocity = (NPC.Center - charge.position) * 0.12f;
                    }
                }
            }
            else if (StateTimer == 35)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Vector2 direction = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY);

                    // Alternate between two projectile types
                    if (attackPattern == 0)
                    {
                        // Projectile 1 - single powerful shot
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, direction * 14f,
                            ModContent.ProjectileType<AbyssalOrbProjectile>(), 60, 2.5f, Main.myPlayer);
                        attackPattern = 1;
                    }
                    else
                    {
                        // Projectile 2 - spread shot
                        for (int i = -1; i <= 1; i++)
                        {
                            Vector2 spreadDir = direction.RotatedBy(MathHelper.ToRadians(18f * i));
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, spreadDir * 12f,
                                ModContent.ProjectileType<AbyssalBoltProjectile>(), 55, 2f, Main.myPlayer);
                        }
                        attackPattern = 0;
                    }

                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Item125, NPC.Center);
                }
            }
            else if (StateTimer >= 70)
            {
                // Return to predatory behavior
                int choice = Main.rand.Next(4);
                if (choice == 0)
                    CurrentState = AIState.AmbushRush;
                else if (choice == 1)
                    CurrentState = AIState.Flanking;
                else if (choice == 2)
                    CurrentState = AIState.Repositioning;
                else
                    CurrentState = AIState.Stalking;
                    
                flankAngle = (float)Math.Atan2(NPC.Center.Y - target.Center.Y, NPC.Center.X - target.Center.X);
                StateTimer = 0;
                AttackCooldown = 70;
            }
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            // Spawn underground at any layer after Moon Lord
            if (NPC.downedMoonlord && 
                (spawnInfo.Player.ZoneDirtLayerHeight || 
                 spawnInfo.Player.ZoneRockLayerHeight || 
                 spawnInfo.Player.ZoneUnderworldHeight) &&
                !spawnInfo.PlayerSafe)
            {
                return 0.25f; // 25% spawn rate
            }
            return 0f;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            // AbyssalMoonLurker drops all 3: Resonant Core item, Energy, and Shards (not ore)
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ResonanceEnergies.ResonantCoreOfMoonlightSonata>(), 1, 2, 4));
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<MoonlightsResonantEnergy>(), 1, 4, 8));
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ShardsOfMoonlitTempo>(), 1, 4, 8));
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int i = 0; i < 12; i++)
            {
                int dustType = Main.rand.NextBool(3) ? DustID.SparksMech : (Main.rand.NextBool() ? DustID.PurpleTorch : DustID.IceTorch);
                Dust hurt = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, dustType, 0f, 0f, 100, dustType == DustID.SparksMech ? Color.White : default, 1.5f);
                hurt.noGravity = true;
                hurt.velocity = Main.rand.NextVector2Circular(6f, 6f);
            }

            if (NPC.life <= 0)
            {
                for (int i = 0; i < 40; i++)
                {
                    int dustType = Main.rand.NextBool(3) ? DustID.SparksMech : (Main.rand.NextBool() ? DustID.PurpleTorch : DustID.IceTorch);
                    Dust death = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, dustType, 0f, 0f, 100, dustType == DustID.SparksMech ? Color.White : default, 2f);
                    death.noGravity = true;
                    death.velocity = Main.rand.NextVector2Circular(12f, 12f);
                }
            }
        }
        
        public override void FindFrame(int frameHeight)
        {
            // Update animation
            frameCounter++;
            
            // Faster animation when ambushing or moving fast
            bool isAggressive = CurrentState == AIState.AmbushRush || Math.Abs(NPC.velocity.X) > 3f;
            int animSpeed = isAggressive ? 2 : FrameTime;
            
            if (frameCounter >= animSpeed)
            {
                frameCounter = 0;
                currentFrame++;
                if (currentFrame >= TotalFrames)
                    currentFrame = 0;
            }
            
            // Calculate frame position in sprite sheet
            int frameX = currentFrame % FrameColumns;
            int frameY = currentFrame / FrameColumns;
            
            // Set frame
            NPC.frame.Y = currentFrame * frameHeight;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Npc[Type].Value;
            
            // Calculate frame dimensions from 6x6 sprite sheet
            int frameWidth = texture.Width / FrameColumns;
            int frameHeight = texture.Height / FrameRows;
            
            // Get current frame position
            int frameX = currentFrame % FrameColumns;
            int frameY = currentFrame / FrameColumns;
            
            Rectangle sourceRect = new Rectangle(frameX * frameWidth, frameY * frameHeight, frameWidth, frameHeight);
            Vector2 drawPos = NPC.Center - screenPos;
            Vector2 origin = new Vector2(frameWidth / 2, frameHeight / 2);

            // White glow effect
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.25f + 0.75f;
            Color whiteGlow = Color.White * pulse * 0.4f;

            for (int i = 0; i < 5; i++)
            {
                Vector2 offset = new Vector2(5f, 0f).RotatedBy(MathHelper.TwoPi * i / 5);
                spriteBatch.Draw(texture, drawPos + offset, sourceRect, whiteGlow, NPC.rotation,
                    origin, NPC.scale, NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
            }

            // Draw main sprite
            spriteBatch.Draw(texture, drawPos, sourceRect, drawColor, NPC.rotation,
                origin, NPC.scale, NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
            
            return false;
        }
    }
}
