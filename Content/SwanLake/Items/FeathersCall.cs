using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.GameContent;
using ReLogic.Content;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Content.SwanLake.Debuffs;

namespace MagnumOpus.Content.SwanLake.Items
{
    /// <summary>
    /// Feather's Call - A rare 1% drop from the Swan Lake boss.
    /// Transforms the player into a smaller version of Swan Lake, The Monochromatic Fractal.
    /// Drains all mana to activate.
    /// Left click: Rain pearlescent/black/white rainbow flares with explosions (tracking)
    /// Right click: Black/white lightning with massive AOE explosions (tracking)
    /// Grants infinite flight while transformed.
    /// </summary>
    public class FeathersCall : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.value = Item.sellPrice(gold: 100);
            Item.rare = ModContent.RarityType<SwanRarity>();
            Item.UseSound = SoundID.Item119 with { Pitch = -0.3f, Volume = 0.9f };
            Item.noMelee = true;
            Item.useTurn = true;
            Item.channel = false;
            Item.buffType = ModContent.BuffType<FeathersCallBuff>();
            Item.buffTime = int.MaxValue; // Infinite duration - lasts until death or manual cancellation
        }

        public override bool CanUseItem(Player player)
        {
            // Must have at least some mana
            return player.statMana > 0;
        }

        public override bool? UseItem(Player player)
        {
            // Drain ALL mana
            int manaDrained = player.statMana;
            player.statMana = 0;
            player.manaRegenDelay = 180; // Long mana regen delay

            // Display transformation message
            string message = $"{player.name} becomes one with the Swan's Melody...";
            if (Main.netMode == NetmodeID.Server)
            {
                Terraria.Chat.ChatHelper.BroadcastChatMessage(
                    Terraria.Localization.NetworkText.FromLiteral(message),
                    new Color(220, 220, 255));
            }
            else
            {
                Main.NewText(message, 220, 220, 255);
            }

            // Massive transformation visual effect
            for (int i = 0; i < 50; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(60f, 60f);
                Color col = Main.rand.NextBool() ? Color.White : Color.Black;
                int dustType = Main.rand.NextBool() ? DustID.WhiteTorch : DustID.Smoke;
                Dust d = Dust.NewDustPerfect(player.Center + offset, dustType,
                    offset * 0.1f, col == Color.White ? 0 : 150, col, 2.5f);
                d.noGravity = true;
                d.fadeIn = 1.5f;
            }

            // Rainbow ring explosion
            for (int i = 0; i < 24; i++)
            {
                float angle = MathHelper.TwoPi * i / 24f;
                float hue = i / 24f;
                Color rainbowColor = Main.hslToRgb(hue, 1f, 0.7f);
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 8f;
                Dust spark = Dust.NewDustPerfect(player.Center, DustID.RainbowTorch, vel, 0, rainbowColor, 2f);
                spark.noGravity = true;
            }

            // Feather burst
            CustomParticles.SwanFeatherExplosion(player.Center, 20, 0.8f);
            ThemedParticles.SwanLakeRainbowExplosion(player.Center, 2f);

            SoundEngine.PlaySound(SoundID.Roar with { Pitch = 0.5f, Volume = 0.7f }, player.Center);

            return true;
        }

        public override void AddRecipes()
        {
            // No recipe - 1% boss drop only
        }
    }

    /// <summary>
    /// Buff that handles the Swan transformation state.
    /// </summary>
    public class FeathersCallBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = false;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = false;
            Main.buffNoTimeDisplay[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<FeathersCallPlayer>().IsTransformed = true;
            player.GetModPlayer<FeathersCallPlayer>().TransformationTimer++;
        }
    }

    /// <summary>
    /// ModPlayer that handles the Swan transformation mechanics.
    /// </summary>
    public class FeathersCallPlayer : ModPlayer
    {
        public bool IsTransformed = false;
        public int TransformationTimer = 0;
        public int LeftClickCooldown = 0;
        public int RightClickCooldown = 0;
        
        // Animation - match boss animation system
        private int frameCounter = 0;
        private int currentFrame = 0;
        private const int TotalFrames = 36;
        private const int IdleFrameTime = 8;
        private const int AttackFrameTime = 5;
        private bool isAttacking = false;
        private int attackAnimTimer = 0;

        // Texture paths matching boss
        private const string IdleTexture = "MagnumOpus/Content/SwanLake/Bosses/SwanLakeTheMonochromaticFractal";
        private const string AttackTexture = "MagnumOpus/Content/SwanLake/Bosses/SwanLakeTheMonochromaticFractal_Attack";

        public override void ResetEffects()
        {
            if (!IsTransformed)
            {
                TransformationTimer = 0;
            }
            IsTransformed = false;
        }

        public override void PreUpdate()
        {
            if (!IsTransformed) return;

            // Cooldown timers
            if (LeftClickCooldown > 0) LeftClickCooldown--;
            if (RightClickCooldown > 0) RightClickCooldown--;

            // Attack animation timer
            if (attackAnimTimer > 0)
            {
                attackAnimTimer--;
                isAttacking = true;
            }
            else
            {
                isAttacking = false;
            }

            // Animation - faster during attacks like the boss
            int frameTime = isAttacking ? AttackFrameTime : IdleFrameTime;
            frameCounter++;
            if (frameCounter >= frameTime)
            {
                frameCounter = 0;
                currentFrame++;
                if (currentFrame >= TotalFrames)
                    currentFrame = 0;
            }

            // Infinite flight - grant wings and flight time
            Player.wingTime = Player.wingTimeMax;
            Player.rocketTime = Player.rocketTimeMax;
            Player.canRocket = true;
            Player.rocketBoots = 3;
            Player.noFallDmg = true;

            // Gravity reduction for more floaty movement
            Player.gravity *= 0.3f;
            Player.maxFallSpeed = 5f;

            // Faster movement
            Player.moveSpeed += 0.5f;
            Player.maxRunSpeed += 3f;
            Player.accRunSpeed += 2f;

            // Ambient particles
            SpawnAmbientParticles();

            // Handle attacks
            HandleAttacks();

            // Lighting
            float pulse = 0.8f + (float)Math.Sin(TransformationTimer * 0.05f) * 0.2f;
            Lighting.AddLight(Player.Center, 0.8f * pulse, 0.8f * pulse, 1f * pulse);
        }

        private void SpawnAmbientParticles()
        {
            // Feathers trailing
            if (Main.rand.NextBool(3))
            {
                Vector2 offset = Main.rand.NextVector2Circular(40f, 40f);
                Color featherColor = Main.rand.NextBool() ? Color.White : Color.Black;
                CustomParticles.SwanFeatherDrift(Player.Center + offset, featherColor, 0.25f);
            }

            // Rainbow shimmer
            if (Main.rand.NextBool(5))
            {
                float hue = Main.rand.NextFloat();
                Color sparkleColor = Main.hslToRgb(hue, 0.8f, 0.7f);
                CustomParticles.PrismaticSparkle(Player.Center + Main.rand.NextVector2Circular(50f, 50f), sparkleColor, 0.2f);
            }

            // Black/white particle flow
            if (Main.rand.NextBool(4))
            {
                Color col = Main.rand.NextBool() ? Color.White : Color.Black;
                int dustType = Main.rand.NextBool() ? DustID.WhiteTorch : DustID.Smoke;
                Dust d = Dust.NewDustPerfect(Player.Center + Main.rand.NextVector2Circular(30f, 30f),
                    dustType, new Vector2(0, -1f), col == Color.White ? 0 : 150, col, 1.2f);
                d.noGravity = true;
            }
        }

        private void HandleAttacks()
        {
            // Left click - Rain flares
            if (Main.mouseLeft && LeftClickCooldown <= 0 && Player.whoAmI == Main.myPlayer)
            {
                LeftClickCooldown = 8; // Fast attack rate
                PerformLeftClickAttack();
            }

            // Right click - Lightning storm
            if (Main.mouseRight && RightClickCooldown <= 0 && Player.whoAmI == Main.myPlayer)
            {
                RightClickCooldown = 25; // Medium attack rate
                PerformRightClickAttack();
            }
        }

        private void PerformLeftClickAttack()
        {
            // Switch to attack sprite for duration of attack
            attackAnimTimer = 60; // 1 second of attack animation
            
            // Find nearest enemy for tracking
            NPC target = FindNearestEnemy(800f);
            Vector2 targetPos = target != null ? target.Center : Main.MouseWorld;

            // Rain down pearlescent/black/white rainbow flares with FRACTAL effects
            SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.3f, Volume = 0.5f }, Player.Center);

            for (int i = 0; i < 6; i++)
            {
                // Spawn position above player
                Vector2 spawnPos = Player.Center + new Vector2(Main.rand.NextFloat(-120f, 120f), -140f + Main.rand.NextFloat(-50f, 50f));
                
                // Direction toward target with some spread
                Vector2 direction = (targetPos - spawnPos).SafeNormalize(Vector2.UnitY);
                direction = direction.RotatedBy(Main.rand.NextFloat(-0.2f, 0.2f));
                
                // Spawn projectile
                int projType = i % 3; // 0 = white, 1 = black, 2 = rainbow
                Projectile.NewProjectile(
                    Player.GetSource_FromThis(),
                    spawnPos,
                    direction * 20f,
                    ModContent.ProjectileType<FeathersCallFlare>(),
                    800, // INSANE damage for 1% drop rate transformation
                    6f,
                    Player.whoAmI,
                    projType,
                    target?.whoAmI ?? -1
                );
                
                // Fractal flare spawn effect at origin
                float hue = (Main.GameUpdateCount * 0.02f + i * 0.25f) % 1f;
                Color fractalColor = Main.hslToRgb(hue, 1f, 0.8f);
                CustomParticles.GenericFlare(spawnPos, fractalColor, 0.5f, 15);
                CustomParticles.PrismaticSparkleBurst(spawnPos, fractalColor, 4);
            }

            // Visual effects - more intense fractal sparkles
            ThemedParticles.SwanLakeSparkles(Player.Center, 8, 40f);
            
            // Fractal lightning from player to spawn positions occasionally
            if (Main.rand.NextBool(3))
            {
                Vector2 lightningEnd = Player.Center + new Vector2(Main.rand.NextFloat(-80f, 80f), -100f);
                MagnumVFX.DrawSwanLakeLightning(Player.Center, lightningEnd, 6, 15f, 2, 0.3f);
            }
        }

        private void PerformRightClickAttack()
        {
            // Switch to attack sprite for duration of attack
            attackAnimTimer = 90; // 1.5 seconds of attack animation (longer for lightning)
            
            // Black/white lightning with massive AOE
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f, Volume = 0.8f }, Player.Center);
            // Screen shake removed from weapons - reserved for bosses only

            // Find enemies for tracking - hit up to 8 enemies!
            List<NPC> targets = FindNearbyEnemies(800f, 8);
            
            if (targets.Count > 0)
            {
                foreach (NPC target in targets)
                {
                    // Draw lightning to each enemy
                    MagnumVFX.DrawSwanLakeLightning(Player.Center, target.Center, 12, 40f, 5, 0.6f);
                    
                    // Massive explosion at impact
                    SpawnLightningExplosion(target.Center);
                    
                    // Damage the enemy - INSANE damage for 1% drop rate
                    if (Main.myPlayer == Player.whoAmI)
                    {
                        Player.ApplyDamageToNPC(target, 1200, 12f, (target.Center.X > Player.Center.X) ? 1 : -1, false);
                    }
                }
            }
            else
            {
                // No enemies - strike ground/surfaces near cursor
                for (int i = 0; i < 5; i++)
                {
                    Vector2 strikePos = Main.MouseWorld + Main.rand.NextVector2Circular(120f, 120f);
                    MagnumVFX.DrawSwanLakeLightning(Player.Center, strikePos, 12, 40f, 5, 0.6f);
                    SpawnLightningExplosion(strikePos);
                }
            }
        }

        private void SpawnLightningExplosion(Vector2 position)
        {
            // Black and white core explosion
            CustomParticles.ExplosionBurst(position, Color.White, 18, 14f);
            CustomParticles.ExplosionBurst(position, Color.Black, 12, 10f);
            
            // Rainbow ring
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                float hue = i / 12f;
                Color rainbowColor = Main.hslToRgb(hue, 1f, 0.8f);
                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 50f;
                CustomParticles.PrismaticSparkleBurst(position + offset, rainbowColor, 5);
            }
            
            // Feather burst
            CustomParticles.SwanFeatherBurst(position, 8, 0.6f);
            
            // Dust explosion
            for (int i = 0; i < 30; i++)
            {
                Color col = i % 2 == 0 ? Color.White : Color.Black;
                int dustType = i % 2 == 0 ? DustID.WhiteTorch : DustID.Smoke;
                Vector2 vel = Main.rand.NextVector2Circular(12f, 12f);
                Dust d = Dust.NewDustPerfect(position, dustType, vel, col == Color.White ? 0 : 150, col, 2.5f);
                d.noGravity = true;
                d.fadeIn = 1.5f;
            }
            
            Lighting.AddLight(position, 2f, 2f, 2.5f);
        }

        private NPC FindNearestEnemy(float maxDistance)
        {
            NPC closest = null;
            float closestDist = maxDistance;
            
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                
                float dist = Vector2.Distance(Player.Center, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = npc;
                }
            }
            
            return closest;
        }

        private List<NPC> FindNearbyEnemies(float maxDistance, int maxCount)
        {
            List<NPC> enemies = new List<NPC>();
            
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                
                float dist = Vector2.Distance(Player.Center, npc.Center);
                if (dist < maxDistance)
                {
                    enemies.Add(npc);
                    if (enemies.Count >= maxCount) break;
                }
            }
            
            return enemies;
        }

        public override void HideDrawLayers(PlayerDrawSet drawInfo)
        {
            if (IsTransformed)
            {
                // Hide all VANILLA player layers - but NOT our custom draw layer!
                // This hides the player body/armor while letting our swan sprite render
                PlayerDrawLayers.Skin.Hide();
                PlayerDrawLayers.Leggings.Hide();
                PlayerDrawLayers.Torso.Hide();
                PlayerDrawLayers.Head.Hide();
                PlayerDrawLayers.ArmOverItem.Hide();
                PlayerDrawLayers.HeldItem.Hide();
                PlayerDrawLayers.FrontAccFront.Hide();
                PlayerDrawLayers.FrontAccBack.Hide();
                PlayerDrawLayers.BackAcc.Hide();
                PlayerDrawLayers.Wings.Hide();
                PlayerDrawLayers.HairBack.Hide();
                PlayerDrawLayers.FaceAcc.Hide();
                PlayerDrawLayers.NeckAcc.Hide();
                PlayerDrawLayers.Shield.Hide();
                PlayerDrawLayers.Shoes.Hide();
                PlayerDrawLayers.WaistAcc.Hide();
                PlayerDrawLayers.BalloonAcc.Hide();
                PlayerDrawLayers.HandOnAcc.Hide();
                PlayerDrawLayers.OffhandAcc.Hide();
            }
        }

        public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
        {
            if (IsTransformed)
            {
                // Make player invisible (we draw custom sprite in PostDraw)
                drawInfo.colorArmorBody = Color.Transparent;
                drawInfo.colorArmorHead = Color.Transparent;
                drawInfo.colorArmorLegs = Color.Transparent;
                drawInfo.colorBodySkin = Color.Transparent;
                drawInfo.colorHead = Color.Transparent;
                drawInfo.colorLegs = Color.Transparent;
            }
        }

        public void DrawTransformedSprite(SpriteBatch spriteBatch)
        {
            if (!IsTransformed) return;

            // Load the boss texture - use ATTACK texture when attacking, else idle
            string texturePath = isAttacking ? AttackTexture : IdleTexture;
            
            // Ensure texture is loaded
            var textureRequest = ModContent.Request<Texture2D>(texturePath, ReLogic.Content.AssetRequestMode.ImmediateLoad);
            if (!textureRequest.IsLoaded) return;
            
            Texture2D texture = textureRequest.Value;
            
            // Calculate frame rectangle for 6x6 sprite sheet (same as boss)
            int frameColumns = 6;
            int frameRows = 6;
            int frameWidth = texture.Width / frameColumns;
            int frameHeight = texture.Height / frameRows;
            int frameX = (currentFrame % frameColumns) * frameWidth;
            int frameY = (currentFrame / frameColumns) * frameHeight;
            Rectangle sourceRect = new Rectangle(frameX, frameY, frameWidth, frameHeight);

            Vector2 position = Player.Center - Main.screenPosition;
            Vector2 origin = new Vector2(frameWidth / 2f, frameHeight / 2f);
            
            // Scale - slightly smaller than boss but still HUGE (0.75f instead of 0.96f)
            float scale = 0.75f;
            
            // Flip based on player direction
            SpriteEffects effects = Player.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            // Pulse effect - faster pulse during attacks
            float pulseSpeed = isAttacking ? 0.12f : 0.08f;
            float pulse = 1f + (float)Math.Sin(TransformationTimer * pulseSpeed) * (isAttacking ? 0.08f : 0.05f);
            
            // Visual distortion effect during combat - more intense during attacks
            float distortion = isAttacking ? 0.25f : (Player.velocity.Length() > 5f ? 0.2f : 0.1f);
            float waveX = (float)Math.Sin(TransformationTimer * 0.15f + position.Y * 0.01f) * distortion * 3f;
            float waveY = (float)Math.Cos(TransformationTimer * 0.12f + position.X * 0.01f) * distortion * 2f;
            position += new Vector2(waveX, waveY);

            // First draw the main sprite in normal blend mode
            spriteBatch.Draw(texture, position, sourceRect, Color.White, 0f, origin, scale * pulse, effects, 0f);

            // Now restart for additive glow layers
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Rainbow pearlescent glow - cycling colors (brighter during attacks)
            float glowIntensity = isAttacking ? 0.7f : 0.5f;
            Color glowColor = Main.hslToRgb((Main.GameUpdateCount * 0.008f) % 1f, 0.6f, 0.6f) * glowIntensity;
            spriteBatch.Draw(texture, position, sourceRect, glowColor, 0f, origin, scale * pulse * 1.15f, effects, 0f);

            // White outer glow (more intense during attacks)
            float whiteIntensity = isAttacking ? 0.6f : 0.4f;
            spriteBatch.Draw(texture, position, sourceRect, Color.White * whiteIntensity, 0f, origin, scale * pulse * 1.25f, effects, 0f);
            
            // Additional rainbow ghost trails when moving
            if (Player.velocity.Length() > 3f || isAttacking)
            {
                int trailCount = isAttacking ? 5 : 3;
                for (int i = 0; i < trailCount; i++)
                {
                    float trailOffset = i * 8f;
                    Vector2 trailPos = position - Player.velocity.SafeNormalize(Vector2.Zero) * trailOffset;
                    float hue = (Main.GameUpdateCount * 0.01f + i * 0.15f) % 1f;
                    Color trailColor = Main.hslToRgb(hue, 0.8f, 0.6f) * (0.25f - i * 0.04f);
                    spriteBatch.Draw(texture, trailPos, sourceRect, trailColor, 0f, origin, scale * pulse * (1.1f - i * 0.04f), effects, 0f);
                }
            }

            // Return to normal AlphaBlend mode for other draw layers
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }

    /// <summary>
    /// Custom draw layer to render the transformed player as Swan Lake boss.
    /// </summary>
    public class FeathersCallDrawLayer : PlayerDrawLayer
    {
        public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.LastVanillaLayer);

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            return drawInfo.drawPlayer.GetModPlayer<FeathersCallPlayer>().IsTransformed;
        }

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            drawInfo.drawPlayer.GetModPlayer<FeathersCallPlayer>().DrawTransformedSprite(Main.spriteBatch);
        }
    }

    /// <summary>
    /// Flare projectile spawned by left click attack.
    /// </summary>
    public class FeathersCallFlare : ModProjectile
    {
        // Use vanilla texture as base
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RainbowRodBullet;

        private int FlareType => (int)Projectile.ai[0]; // 0 = white, 1 = black, 2 = rainbow
        private int TargetIndex => (int)Projectile.ai[1];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 12;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.light = 0.5f;
        }

        public override void AI()
        {
            // Rotate
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // Trail particles
            Color trailColor = FlareType switch
            {
                0 => Color.White,
                1 => Color.Black,
                _ => Main.hslToRgb((Main.GameUpdateCount * 0.02f) % 1f, 1f, 0.7f)
            };

            if (Main.rand.NextBool(2))
            {
                int dustType = FlareType == 1 ? DustID.Smoke : DustID.WhiteTorch;
                Dust d = Dust.NewDustPerfect(Projectile.Center, dustType,
                    -Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(1f, 1f),
                    FlareType == 1 ? 150 : 0, trailColor, 1.5f);
                d.noGravity = true;
            }

            // Homing toward target
            if (TargetIndex >= 0 && TargetIndex < Main.maxNPCs)
            {
                NPC target = Main.npc[TargetIndex];
                if (target.active && !target.friendly)
                {
                    Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 18f, 0.08f);
                }
            }

            // Light
            float lightIntensity = 0.6f;
            if (FlareType == 2)
            {
                Vector3 rainbowLight = Main.hslToRgb((Main.GameUpdateCount * 0.02f) % 1f, 1f, 0.6f).ToVector3();
                Lighting.AddLight(Projectile.Center, rainbowLight * lightIntensity);
            }
            else
            {
                Lighting.AddLight(Projectile.Center, lightIntensity, lightIntensity, lightIntensity);
            }
        }

        public override void OnKill(int timeLeft)
        {
            // Explosion effect - ENHANCED with fractal effects
            Color explosionColor = FlareType switch
            {
                0 => Color.White,
                1 => Color.Black,
                _ => Main.hslToRgb(Main.rand.NextFloat(), 1f, 0.7f)
            };

            // Core explosion
            CustomParticles.ExplosionBurst(Projectile.Center, explosionColor, 12, 10f);
            CustomParticles.SwanFeatherBurst(Projectile.Center, 6, 0.4f);
            
            // FRACTAL FLARE BURST - the signature look
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 flareOffset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 25f;
                float hue = (Main.GameUpdateCount * 0.02f + i * 0.16f) % 1f;
                Color fractalColor = FlareType == 2 ? Main.hslToRgb(hue, 1f, 0.85f) : explosionColor;
                CustomParticles.GenericFlare(Projectile.Center + flareOffset, fractalColor, 0.4f, 18);
            }

            // Rainbow sparkle ring for all types
            for (int i = 0; i < 10; i++)
            {
                float hue = i / 10f;
                Color rainbowColor = Main.hslToRgb(hue, 1f, 0.8f);
                CustomParticles.PrismaticSparkleBurst(Projectile.Center + Main.rand.NextVector2Circular(25f, 25f), rainbowColor, 4);
            }
            
            // Mini lightning fractals on rainbow type
            if (FlareType == 2 && Main.rand.NextBool(2))
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector2 lightningEnd = Projectile.Center + Main.rand.NextVector2Circular(60f, 60f);
                    MagnumVFX.DrawSwanLakeLightning(Projectile.Center, lightningEnd, 4, 12f, 1, 0.2f);
                }
            }

            // Dust explosion
            for (int i = 0; i < 20; i++)
            {
                int dustType = FlareType == 1 ? DustID.Smoke : DustID.WhiteTorch;
                Color col = FlareType == 1 ? Color.Black : Color.White;
                Vector2 vel = Main.rand.NextVector2Circular(8f, 8f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, dustType, vel, FlareType == 1 ? 150 : 0, col, 2f);
                d.noGravity = true;
                d.fadeIn = 1.5f;
            }

            // Apply debuff to nearby enemies
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && Vector2.Distance(Projectile.Center, npc.Center) < 80f)
                {
                    npc.AddBuff(ModContent.BuffType<FlameOfTheSwan>(), 300);
                }
            }

            SoundEngine.PlaySound(SoundID.Item10 with { Pitch = 0.5f, Volume = 0.5f }, Projectile.Center);
            
            // Screen shake removed from weapons - reserved for bosses only
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // Get color based on flare type
            Color coreColor = FlareType switch
            {
                0 => Color.White,
                1 => new Color(30, 30, 30),
                _ => Main.hslToRgb((Main.GameUpdateCount * 0.02f) % 1f, 1f, 0.7f)
            };
            Color glowColor = FlareType switch
            {
                0 => new Color(255, 255, 255, 100),
                1 => new Color(80, 80, 80, 100),
                _ => Main.hslToRgb((Main.GameUpdateCount * 0.02f + 0.1f) % 1f, 0.8f, 0.8f) * 0.7f
            };

            // Draw trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;

                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                float fade = 1f - (i / (float)Projectile.oldPos.Length);
                float trailScale = (1f - i * 0.06f) * 0.5f;
                Color trailCol = glowColor * fade * 0.5f;

                // Draw glowing orb trail
                Texture2D glowTex = TextureAssets.Extra[98].Value; // Glow texture
                spriteBatch.Draw(glowTex, trailPos, null, trailCol, 0f, glowTex.Size() / 2f, trailScale, SpriteEffects.None, 0f);
            }

            // Draw outer glow
            Texture2D glowTexture = TextureAssets.Extra[98].Value;
            spriteBatch.Draw(glowTexture, drawPos, null, glowColor * 0.6f, 0f, glowTexture.Size() / 2f, 0.8f, SpriteEffects.None, 0f);

            // Draw inner core
            spriteBatch.Draw(glowTexture, drawPos, null, coreColor, 0f, glowTexture.Size() / 2f, 0.4f, SpriteEffects.None, 0f);

            // Draw sparkle overlay for rainbow type
            if (FlareType == 2)
            {
                float sparkleRot = Main.GameUpdateCount * 0.1f;
                Color sparkleCol = Main.hslToRgb((Main.GameUpdateCount * 0.03f + 0.5f) % 1f, 1f, 0.9f) * 0.8f;
                spriteBatch.Draw(glowTexture, drawPos, null, sparkleCol, sparkleRot, glowTexture.Size() / 2f, 0.3f, SpriteEffects.None, 0f);
            }

            return false; // Don't draw default sprite
        }
    }
}
