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
    /// Eroican Centurion - A desert mini-boss with 5 unique red and gold flaming attacks.
    /// Spawns in deserts at 5% rate after Moon Lord is defeated.
    /// 
    /// 5 ATTACKS:
    /// 1. Blazing Charge - Fast dash with fire trail
    /// 2. Inferno Ring - Spawns ring of fire projectiles outward
    /// 3. Crimson Meteor Shower - Rains fire meteors from sky
    /// 4. Golden Sword Storm - Fires rotating sword projectiles
    /// 5. Triumphant Nova - Massive AoE explosion with shockwave
    /// </summary>
    public class EroicanCenturion : ModNPC
    {
        private enum AIState
        {
            Idle,
            Chasing,
            BlazingCharge,      // Attack 1
            InfernoRing,        // Attack 2
            CrimsonMeteorShower,// Attack 3
            GoldenSwordStorm,   // Attack 4
            TriumphantNova,     // Attack 5
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

        // Attack parameters
        private Vector2 chargeDirection = Vector2.Zero;
        private bool isCharging = false;

        // Movement tracking for idle detection
        private int lastSpriteDirection = 1;
        private bool isMoving = false;

        // Colors
        private static readonly Color EroicaRed = new Color(200, 40, 40);
        private static readonly Color EroicaGold = new Color(255, 200, 100);
        private static readonly Color EroicaCrimson = new Color(180, 20, 20);

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
            // MINI-BOSS STATS - Significantly boosted
            NPC.width = 287;
            NPC.height = 146;
            NPC.damage = 180;
            NPC.defense = 90;
            NPC.lifeMax = 75000; // Mini-boss HP
            NPC.HitSound = SoundID.NPCHit41;
            NPC.DeathSound = SoundID.NPCDeath43;
            NPC.knockBackResist = 0.02f; // Near immune
            NPC.value = Item.buyPrice(gold: 50);
            NPC.aiStyle = -1;
            NPC.boss = false; // Not a true boss, but powerful
            NPC.npcSlots = 5f;

            NPC.noGravity = false;
            NPC.noTileCollide = false;

            DrawOffsetY = -52f;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Desert,
                new FlavorTextBestiaryInfoElement("A legendary warrior blazing with corrupted valor. Its mastery of five devastating fire techniques makes it a fearsome desert guardian. The golden flames of its attacks burn with ancient heroic fury.")
            });
        }

        public override void AI()
        {
            Player target = Main.player[NPC.target];

            // Check if moving
            float movementThreshold = 0.5f;
            isMoving = Math.Abs(NPC.velocity.X) > movementThreshold || Math.Abs(NPC.velocity.Y) > movementThreshold;

            // Face direction of movement or target
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

            // Intense scarlet and gold glow
            Lighting.AddLight(NPC.Center, 1.2f, 0.4f, 0.2f);

            // Themed ambient particles
            ThemedParticles.EroicaAura(NPC.Center, NPC.width * 0.8f);
            
            if (Main.rand.NextBool(6))
            {
                ThemedParticles.EroicaSparkles(NPC.Center, 3, NPC.width * 0.6f);
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
            orbitAngle += 0.03f;
            if (orbitAngle > MathHelper.TwoPi)
                orbitAngle -= MathHelper.TwoPi;

            // Retarget
            NPC.TargetClosest(true);
            target = Main.player[NPC.target];

            float distanceToTarget = Vector2.Distance(NPC.Center, target.Center);

            // Update timers
            StateTimer++;
            if (AttackCooldown > 0f)
                AttackCooldown--;

            switch (CurrentState)
            {
                case AIState.Idle:
                case AIState.Chasing:
                    HandleChasing(target, distanceToTarget);
                    break;
                case AIState.BlazingCharge:
                    HandleBlazingCharge(target);
                    break;
                case AIState.InfernoRing:
                    HandleInfernoRing(target);
                    break;
                case AIState.CrimsonMeteorShower:
                    HandleCrimsonMeteorShower(target);
                    break;
                case AIState.GoldenSwordStorm:
                    HandleGoldenSwordStorm(target);
                    break;
                case AIState.TriumphantNova:
                    HandleTriumphantNova(target);
                    break;
                case AIState.Recovering:
                    HandleRecovering();
                    break;
            }

            // Animation update
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
                currentFrame = 0;
                frameCounter = 0;
            }
        }

        private void HandleChasing(Player target, float distance)
        {
            isCharging = false;

            // Fast aggressive movement
            float moveSpeed = 8f;
            float accel = 0.45f;

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
                    NPC.velocity.Y = -14f;
                }
                else if (target.Center.Y < NPC.Center.Y - 100f && Main.rand.NextBool(20))
                {
                    NPC.velocity.Y = -16f;
                }
            }

            // Select next attack when cooldown is done
            if (AttackCooldown <= 0f && distance < 600f)
            {
                SelectNextAttack(target, distance);
            }

            CurrentState = AIState.Chasing;
        }

        private void SelectNextAttack(Player target, float distance)
        {
            // Cycle through attacks for variety
            AttackCounter++;
            int attackChoice = AttackCounter % 5;

            switch (attackChoice)
            {
                case 0: // Blazing Charge - when in medium range
                    if (distance > 150f && distance < 500f && NPC.velocity.Y == 0f)
                    {
                        CurrentState = AIState.BlazingCharge;
                        StateTimer = 0f;
                        chargeDirection = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
                        isCharging = true;
                        SoundEngine.PlaySound(SoundID.Item74, NPC.Center);
                        SpawnWindupEffect();
                    }
                    break;

                case 1: // Inferno Ring - when close
                    if (distance < 400f)
                    {
                        CurrentState = AIState.InfernoRing;
                        StateTimer = 0f;
                        SoundEngine.PlaySound(SoundID.Item45, NPC.Center);
                    }
                    break;

                case 2: // Crimson Meteor Shower - any range
                    CurrentState = AIState.CrimsonMeteorShower;
                    StateTimer = 0f;
                    SoundEngine.PlaySound(SoundID.Item88, NPC.Center);
                    break;

                case 3: // Golden Sword Storm - medium range
                    if (distance < 500f)
                    {
                        CurrentState = AIState.GoldenSwordStorm;
                        StateTimer = 0f;
                        SoundEngine.PlaySound(SoundID.Item71, NPC.Center);
                    }
                    break;

                case 4: // Triumphant Nova - when low on health or randomly
                    if (NPC.life < NPC.lifeMax * 0.5f || Main.rand.NextBool(3))
                    {
                        CurrentState = AIState.TriumphantNova;
                        StateTimer = 0f;
                        SoundEngine.PlaySound(SoundID.Item119, NPC.Center);
                    }
                    break;
            }
        }

        private void SpawnWindupEffect()
        {
            for (int i = 0; i < 25; i++)
            {
                Dust charge = Dust.NewDustDirect(NPC.Center, 1, 1, DustID.Torch, 0f, 0f, 100, EroicaCrimson, 2.5f);
                charge.noGravity = true;
                charge.velocity = Main.rand.NextVector2Circular(10f, 10f);
            }
            for (int i = 0; i < 15; i++)
            {
                Dust gold = Dust.NewDustDirect(NPC.Center, 1, 1, DustID.GoldFlame, 0f, 0f, 50, default, 2f);
                gold.noGravity = true;
                gold.velocity = Main.rand.NextVector2Circular(8f, 8f);
            }
        }

        #region Attack 1: Blazing Charge
        private void HandleBlazingCharge(Player target)
        {
            isCharging = true;

            if (StateTimer < 50f)
            {
                // Fast charge
                float chargeSpeed = 20f;
                NPC.velocity = chargeDirection * chargeSpeed;
                NPC.velocity.Y += 0.3f;

                // Flaming trail
                if (Main.rand.NextBool(2))
                {
                    Dust flame = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Torch, 0f, 0f, 100, EroicaCrimson, 3f);
                    flame.noGravity = true;
                    flame.velocity = -NPC.velocity * 0.25f + Main.rand.NextVector2Circular(3f, 3f);
                }
                if (Main.rand.NextBool(2))
                {
                    Dust gold = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.GoldFlame, 0f, 0f, 50, default, 2.5f);
                    gold.noGravity = true;
                    gold.velocity = -NPC.velocity * 0.2f;
                }

                // Spawn fire trail projectiles
                if (StateTimer % 5 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero,
                        ModContent.ProjectileType<CenturionFireTrail>(), 80, 1f, Main.myPlayer);
                }

                // Stop if hit wall
                if (NPC.collideX || NPC.collideY)
                {
                    EndAttack(120f);
                    SpawnImpactEffect();
                }
            }
            else
            {
                EndAttack(120f);
            }
        }
        #endregion

        #region Attack 2: Inferno Ring
        private void HandleInfernoRing(Player target)
        {
            isCharging = false;
            NPC.velocity.X *= 0.9f;

            if (StateTimer == 30f)
            {
                // Fire ring of projectiles
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    int projectileCount = 16;
                    for (int i = 0; i < projectileCount; i++)
                    {
                        float angle = MathHelper.TwoPi / projectileCount * i;
                        Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 8f;
                        
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity,
                            ModContent.ProjectileType<CenturionInfernoOrb>(), 75, 2f, Main.myPlayer);
                    }
                }
                
                SoundEngine.PlaySound(SoundID.Item45, NPC.Center);
                SpawnRingEffect();
            }
            else if (StateTimer == 60f)
            {
                // Second wave - offset
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    int projectileCount = 16;
                    for (int i = 0; i < projectileCount; i++)
                    {
                        float angle = MathHelper.TwoPi / projectileCount * i + MathHelper.PiOver4 / 2f;
                        Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 10f;
                        
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity,
                            ModContent.ProjectileType<CenturionInfernoOrb>(), 75, 2f, Main.myPlayer);
                    }
                }
                SpawnRingEffect();
            }
            else if (StateTimer > 80f)
            {
                EndAttack(90f);
            }
        }

        private void SpawnRingEffect()
        {
            for (int i = 0; i < 30; i++)
            {
                float angle = MathHelper.TwoPi / 30f * i;
                Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 6f;
                Dust ring = Dust.NewDustDirect(NPC.Center, 1, 1, DustID.Torch, velocity.X, velocity.Y, 100, EroicaRed, 2f);
                ring.noGravity = true;
            }
        }
        #endregion

        #region Attack 3: Crimson Meteor Shower
        private void HandleCrimsonMeteorShower(Player target)
        {
            isCharging = false;
            NPC.velocity.X *= 0.95f;

            // Rain meteors for 90 frames
            if (StateTimer < 90f)
            {
                if (StateTimer % 8 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    // Spawn meteor above player
                    float offsetX = Main.rand.NextFloat(-300f, 300f);
                    Vector2 spawnPos = new Vector2(target.Center.X + offsetX, target.Center.Y - 500f);
                    Vector2 velocity = new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(10f, 16f));

                    Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPos, velocity,
                        ModContent.ProjectileType<CenturionCrimsonMeteor>(), 85, 3f, Main.myPlayer);
                }
            }
            else
            {
                EndAttack(100f);
            }
        }
        #endregion

        #region Attack 4: Golden Sword Storm
        private void HandleGoldenSwordStorm(Player target)
        {
            isCharging = false;
            NPC.velocity.X *= 0.9f;

            // Fire rotating swords in bursts
            if (StateTimer == 20f || StateTimer == 40f || StateTimer == 60f)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    int swordCount = 8;
                    for (int i = 0; i < swordCount; i++)
                    {
                        float angle = MathHelper.TwoPi / swordCount * i + (StateTimer / 20f) * 0.3f;
                        Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 9f;

                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity,
                            ModContent.ProjectileType<CenturionGoldenSword>(), 90, 2f, Main.myPlayer, angle);
                    }
                }
                
                SoundEngine.PlaySound(SoundID.Item71, NPC.Center);
                
                for (int i = 0; i < 20; i++)
                {
                    Dust sword = Dust.NewDustDirect(NPC.Center, 1, 1, DustID.GoldFlame, 0f, 0f, 50, default, 2f);
                    sword.noGravity = true;
                    sword.velocity = Main.rand.NextVector2Circular(8f, 8f);
                }
            }
            else if (StateTimer > 80f)
            {
                EndAttack(80f);
            }
        }
        #endregion

        #region Attack 5: Triumphant Nova
        private void HandleTriumphantNova(Player target)
        {
            isCharging = false;

            // Windup phase
            if (StateTimer < 60f)
            {
                NPC.velocity *= 0.9f;
                
                // Gathering energy effect
                if (StateTimer % 3 == 0)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                        float dist = Main.rand.NextFloat(100f, 200f);
                        Vector2 pos = NPC.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * dist;
                        Vector2 vel = (NPC.Center - pos).SafeNormalize(Vector2.Zero) * 8f;
                        
                        Dust gather = Dust.NewDustDirect(pos, 1, 1, DustID.GoldFlame, vel.X, vel.Y, 50, default, 2f);
                        gather.noGravity = true;
                    }
                }

                // Pulsing glow
                float pulse = (float)Math.Sin(StateTimer * 0.2f) * 0.5f + 1f;
                Lighting.AddLight(NPC.Center, 2f * pulse, 0.8f * pulse, 0.2f * pulse);
            }
            // Explosion phase
            else if (StateTimer == 60f)
            {
                SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
                SoundEngine.PlaySound(SoundID.Item119, NPC.Center);

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    // Massive explosion ring
                    int projectileCount = 24;
                    for (int i = 0; i < projectileCount; i++)
                    {
                        float angle = MathHelper.TwoPi / projectileCount * i;
                        Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 12f;
                        
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity,
                            ModContent.ProjectileType<CenturionNovaWave>(), 100, 4f, Main.myPlayer);
                    }

                    // Inner ring - faster
                    for (int i = 0; i < projectileCount; i++)
                    {
                        float angle = MathHelper.TwoPi / projectileCount * i + MathHelper.Pi / projectileCount;
                        Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 18f;
                        
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity,
                            ModContent.ProjectileType<CenturionInfernoOrb>(), 80, 3f, Main.myPlayer);
                    }
                }

                // Massive visual explosion
                for (int i = 0; i < 60; i++)
                {
                    Dust explode = Dust.NewDustDirect(NPC.Center, 1, 1, DustID.Torch, 0f, 0f, 100, EroicaCrimson, 3.5f);
                    explode.noGravity = true;
                    explode.velocity = Main.rand.NextVector2Circular(20f, 20f);
                }
                for (int i = 0; i < 40; i++)
                {
                    Dust gold = Dust.NewDustDirect(NPC.Center, 1, 1, DustID.GoldFlame, 0f, 0f, 50, default, 3f);
                    gold.noGravity = true;
                    gold.velocity = Main.rand.NextVector2Circular(18f, 18f);
                }

                ThemedParticles.EroicaShockwave(NPC.Center, 3f);
            }
            else if (StateTimer > 90f)
            {
                EndAttack(150f); // Long cooldown after nova
            }
        }
        #endregion

        private void HandleRecovering()
        {
            isCharging = false;
            NPC.velocity.X *= 0.85f;

            if (StateTimer > 40f)
            {
                CurrentState = AIState.Chasing;
                StateTimer = 0f;
            }
        }

        private void EndAttack(float cooldown)
        {
            CurrentState = AIState.Recovering;
            StateTimer = 0f;
            AttackCooldown = cooldown;
            isCharging = false;
        }

        private void SpawnImpactEffect()
        {
            SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
            for (int i = 0; i < 30; i++)
            {
                Dust impact = Dust.NewDustDirect(NPC.Center, 1, 1, DustID.Torch, 0f, 0f, 100, EroicaCrimson, 2.5f);
                impact.noGravity = true;
                impact.velocity = Main.rand.NextVector2Circular(12f, 12f);
            }
            ThemedParticles.EroicaImpact(NPC.Center, 2f);
        }

        private void SpawnLanterns()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            for (int i = 0; i < 3; i++)
            {
                float angle = (MathHelper.TwoPi / 3f) * i;
                int lantern = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero,
                    ModContent.ProjectileType<CenturionLantern>(), 70, 2f, Main.myPlayer, NPC.whoAmI, angle);

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

                if (needsRespawn && Main.netMode != NetmodeID.MultiplayerClient && Main.rand.NextBool(120))
                {
                    float angle = (MathHelper.TwoPi / 3f) * i + orbitAngle;
                    int lantern = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero,
                        ModContent.ProjectileType<CenturionLantern>(), 70, 2f, Main.myPlayer, NPC.whoAmI, angle);

                    if (lantern < Main.maxProjectiles)
                    {
                        orbitingLanterns[i] = lantern;
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
            Texture2D texture = Terraria.GameContent.TextureAssets.Npc[Type].Value;

            int frameWidth = texture.Width / FrameColumns;
            int frameHeight = texture.Height / FrameRows;
            int frameX = currentFrame % FrameColumns;
            int frameY = currentFrame / FrameColumns;

            Rectangle sourceRect = new Rectangle(frameX * frameWidth, frameY * frameHeight, frameWidth, frameHeight);
            Vector2 origin = new Vector2(frameWidth / 2f, frameHeight / 2f);
            Vector2 drawPos = NPC.Center - screenPos + new Vector2(0f, NPC.gfxOffY + DrawOffsetY);

            float drawScale = 0.45f;

            // Enhanced glow effect for mini-boss
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.3f + 0.7f;
            Color glowColor = new Color(220, 50, 30, 0) * 0.6f * pulse;
            Color goldGlow = new Color(255, 200, 100, 0) * 0.3f * pulse;

            SpriteEffects effects = NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            // Red glow
            for (int i = 0; i < 4; i++)
            {
                Vector2 glowOffset = new Vector2(5f, 0f).RotatedBy(i * MathHelper.PiOver2);
                spriteBatch.Draw(texture, drawPos + glowOffset, sourceRect, glowColor, NPC.rotation, origin, drawScale,
                    effects, 0f);
            }

            // Gold outer glow
            for (int i = 0; i < 4; i++)
            {
                Vector2 glowOffset = new Vector2(8f, 0f).RotatedBy(i * MathHelper.PiOver2 + MathHelper.PiOver4);
                spriteBatch.Draw(texture, drawPos + glowOffset, sourceRect, goldGlow, NPC.rotation, origin, drawScale,
                    effects, 0f);
            }

            // Main sprite
            spriteBatch.Draw(texture, drawPos, sourceRect, drawColor, NPC.rotation, origin, drawScale,
                effects, 0f);

            return false;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            ThemedParticles.EroicaSparkles(NPC.Center, 4, NPC.width * 0.5f);
            ThemedParticles.EroicaSparks(NPC.Center, -hit.HitDirection * Vector2.UnitX, 4, 5f);

            if (NPC.life <= 0)
            {
                // Dramatic death explosion
                ThemedParticles.EroicaImpact(NPC.Center, 4f);
                ThemedParticles.EroicaShockwave(NPC.Center, 3f);
                ThemedParticles.EroicaSparkles(NPC.Center, 25, NPC.width * 1.5f);

                for (int i = 0; i < 50; i++)
                {
                    Dust death = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.GoldFlame, 0f, 0f, 50, default, 3f);
                    death.noGravity = true;
                    death.velocity = Main.rand.NextVector2Circular(15f, 15f);
                }

                SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
                SoundEngine.PlaySound(SoundID.NPCDeath43, NPC.Center);
            }
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            // Mini-boss tier drops
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ShardOfTriumphsTempo>(), 1, 5, 10));
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<EroicasResonantEnergy>(), 1, 8, 15));
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ResonantCoreOfEroica>(), 1, 3, 6));
            
            // Valor Essence - theme essence drop (15%)
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ValorEssence>(), 7));
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            // Mini-boss - 5% spawn rate in desert after Moon Lord
            if (NPC.downedMoonlord &&
                spawnInfo.Player.ZoneDesert &&
                !spawnInfo.PlayerSafe)
            {
                return 0.05f; // 5% spawn rate
            }
            return 0f;
        }
    }

    #region Projectiles

    /// <summary>
    /// Fire trail left behind during Blazing Charge
    /// </summary>
    public class CenturionFireTrail : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
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

            for (int i = 0; i < 2; i++)
            {
                Dust flame = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, -2f, 100, new Color(200, 40, 40), 2f);
                flame.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, 0.8f, 0.2f, 0.1f);
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }

    /// <summary>
    /// Inferno orb projectile for ring attack
    /// </summary>
    public class CenturionInfernoOrb : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/EnergyFlare";

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.alpha = 100;
        }

        public override void AI()
        {
            Projectile.rotation += 0.2f;

            if (Main.rand.NextBool(2))
            {
                Dust flame = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, new Color(200, 40, 40), 1.5f);
                flame.noGravity = true;
                flame.velocity *= 0.3f;
            }
            if (Main.rand.NextBool(3))
            {
                Dust gold = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GoldFlame, 0f, 0f, 50, default, 1.2f);
                gold.noGravity = true;
                gold.velocity *= 0.2f;
            }

            Lighting.AddLight(Projectile.Center, 0.7f, 0.3f, 0.1f);
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
        }
    }

    /// <summary>
    /// Crimson meteor for meteor shower attack
    /// </summary>
    public class CenturionCrimsonMeteor : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.alpha = 0;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // Fire trail
            for (int i = 0; i < 2; i++)
            {
                Dust trail = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, new Color(180, 20, 20), 2f);
                trail.noGravity = true;
                trail.velocity = -Projectile.velocity * 0.2f;
            }
            if (Main.rand.NextBool(2))
            {
                Dust smoke = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 150, Color.Black, 1.5f);
                smoke.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, 0.9f, 0.2f, 0.1f);
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
            
            for (int i = 0; i < 20; i++)
            {
                Dust explode = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, new Color(180, 20, 20), 2.5f);
                explode.noGravity = true;
                explode.velocity = Main.rand.NextVector2Circular(10f, 10f);
            }
            for (int i = 0; i < 10; i++)
            {
                Dust smoke = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 150, Color.Black, 2f);
                smoke.velocity = Main.rand.NextVector2Circular(5f, 5f);
            }
        }
    }

    /// <summary>
    /// Golden rotating sword projectile
    /// </summary>
    public class CenturionGoldenSword : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/EnergyFlare";

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.alpha = 50;
        }

        public override void AI()
        {
            Projectile.rotation += 0.3f;

            // Golden trail
            if (Main.rand.NextBool(2))
            {
                Dust gold = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GoldFlame, 0f, 0f, 50, default, 1.5f);
                gold.noGravity = true;
                gold.velocity *= 0.2f;
            }

            Lighting.AddLight(Projectile.Center, 0.8f, 0.6f, 0.2f);
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 10; i++)
            {
                Dust gold = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GoldFlame, 0f, 0f, 50, default, 1.5f);
                gold.noGravity = true;
                gold.velocity = Main.rand.NextVector2Circular(5f, 5f);
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255, 220, 100, 150);
        }
    }

    /// <summary>
    /// Nova shockwave projectile
    /// </summary>
    public class CenturionNovaWave : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = false;
            Projectile.alpha = 100;
        }

        public override void AI()
        {
            Projectile.alpha += 2;
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

            Lighting.AddLight(Projectile.Center, 1f, 0.4f, 0.1f);
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }

    #endregion
}
