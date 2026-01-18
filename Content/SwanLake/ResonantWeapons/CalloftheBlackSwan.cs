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
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.SwanLake.Debuffs;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons
{
    /// <summary>
    /// Call of the Black Swan - Greatsword that sends 3 black/white flares on swing.
    /// Landing all 3 flares empowers the sword, making the next swing fire 8 smaller projectiles at double damage.
    /// Rainbow (Swan) rarity, no crafting recipe.
    /// Hold right-click to charge a devastating prismatic swan storm attack!
    /// </summary>
    public class CalloftheBlackSwan : ModItem
    {
        // Track empowerment state per player
        private static Dictionary<int, int> flareHitCounts = new Dictionary<int, int>();
        private static Dictionary<int, bool> empoweredState = new Dictionary<int, bool>();
        private static Dictionary<int, int> empowermentTimer = new Dictionary<int, int>();

        public static void RegisterFlareHit(int playerIndex)
        {
            if (!flareHitCounts.ContainsKey(playerIndex))
                flareHitCounts[playerIndex] = 0;
            
            flareHitCounts[playerIndex]++;
            
            if (flareHitCounts[playerIndex] >= 3)
            {
                empoweredState[playerIndex] = true;
                empowermentTimer[playerIndex] = 300; // 5 seconds to use empowered swing
                flareHitCounts[playerIndex] = 0;
                
                // Visual feedback for empowerment
                Player player = Main.player[playerIndex];
                CustomParticles.SwanLakeImpactBurst(player.Center, 15);
                CustomParticles.HaloRing(player.Center, Color.Black, 1f, 40);
                CustomParticles.HaloRing(player.Center, Color.White, 0.8f, 30);
                SoundEngine.PlaySound(SoundID.Item119 with { Volume = 0.8f }, player.Center);
            }
        }

        public static bool IsEmpowered(int playerIndex)
        {
            return empoweredState.ContainsKey(playerIndex) && empoweredState[playerIndex];
        }

        public static void ConsumeEmpowerment(int playerIndex)
        {
            if (empoweredState.ContainsKey(playerIndex))
            {
                empoweredState[playerIndex] = false;
                empowermentTimer[playerIndex] = 0;
            }
        }

        public static void ResetFlareCount(int playerIndex)
        {
            flareHitCounts[playerIndex] = 0;
        }

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 400;
            Item.DamageType = DamageClass.Melee;
            Item.width = 70;
            Item.height = 70;
            Item.useTime = 28;
            Item.useAnimation = 28;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 7f;
            Item.value = Item.sellPrice(gold: 60);
            Item.rare = ModContent.RarityType<SwanRarity>();
            Item.UseSound = SoundID.Item29 with { Pitch = -0.1f, Volume = 0.85f }; // Fractal crystal sound
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<BlackSwanFlare>();
            Item.shootSpeed = 14f;
            Item.noMelee = false;
            Item.scale = 0.9f; // 90% size
        }

        public override void HoldItem(Player player)
        {
            // Update empowerment timer
            if (empowermentTimer.ContainsKey(player.whoAmI) && empowermentTimer[player.whoAmI] > 0)
            {
                empowermentTimer[player.whoAmI]--;
                if (empowermentTimer[player.whoAmI] <= 0)
                {
                    empoweredState[player.whoAmI] = false;
                }
            }

            // EXPLOSIVE visual effects when empowered!
            if (IsEmpowered(player.whoAmI))
            {
                // Intense pulsing black/white aura with rainbow shimmer!
                float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.2f);
                
                // Magic sparkle field rising - elegant empowerment aura
                if (Main.rand.NextBool(4))
                {
                    CustomParticles.MagicSparkleFieldRising(player.Center, Color.White * 0.7f, 3);
                }
                
                // Prismatic rainbow sparkles - floating iridescent dust
                if (Main.rand.NextBool(5))
                {
                    CustomParticles.PrismaticSparkleRainbow(player.Center, 5);
                }
                
                // Swan feather aura - elegant floating feathers
                if (Main.rand.NextBool(6))
                {
                    CustomParticles.SwanFeatherAura(player.Center, 35f, 2);
                }
                
                // Black/white particle flow - reduced for elegance
                for (int i = 0; i < 2; i++)
                {
                    Vector2 offset = Main.rand.NextVector2Circular(25f, 25f);
                    Color col = Main.rand.NextBool() ? Color.White : Color.Black;
                    int dustType = Main.rand.NextBool() ? DustID.WhiteTorch : DustID.Shadowflame;
                    Dust d = Dust.NewDustPerfect(player.Center + offset, dustType,
                        new Vector2(0, -1.8f) + Main.rand.NextVector2Circular(1f, 1f), col == Color.White ? 0 : 100, col, 1.5f);
                    d.noGravity = true;
                    d.fadeIn = 1.2f;
                }
                
                // BRIGHT pulsing rainbow light!
                float hueLight = (Main.GameUpdateCount * 0.02f) % 1f;
                Vector3 rainbowLight = Main.hslToRgb(hueLight, 0.8f, 0.6f).ToVector3();
                Lighting.AddLight(player.Center, (0.7f + pulse * 0.25f) * rainbowLight);
            }
            else
            {
                // === SUBTLE AMBIENT EFFECTS - Reduced for elegance ===
                // Only occasional fractal flares
                if (Main.rand.NextBool(25))
                {
                    float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                    float radius = Main.rand.NextFloat(30f, 60f);
                    Vector2 flarePos = player.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                    Color baseColor = Main.rand.NextBool() ? UnifiedVFX.SwanLake.Black : UnifiedVFX.SwanLake.White;
                    Color rainbow = UnifiedVFX.SwanLake.GetRainbow(Main.rand.NextFloat());
                    Color fractalColor = Color.Lerp(baseColor, rainbow, 0.35f);
                    CustomParticles.GenericFlare(flarePos, fractalColor, 0.28f, 18);
                }
                
                // Rare floating feathers - much less frequent
                if (Main.rand.NextBool(40))
                {
                    Color featherColor = Main.rand.NextBool() ? UnifiedVFX.SwanLake.White : UnifiedVFX.SwanLake.Black;
                    CustomParticles.SwanFeatherDrift(player.Center + Main.rand.NextVector2Circular(22f, 22f), featherColor, 0.25f);
                }
                
                // Subtle light
                float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 0.1f + 0.9f;
                Vector3 lightColor = UnifiedVFX.SwanLake.GetRainbow(Main.GameUpdateCount * 0.01f).ToVector3();
                Lighting.AddLight(player.Center, lightColor * pulse * 0.25f);
            }
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            // === REDUCED: BLACK SWAN FEATHER TRAIL ===
            // The greatsword leaves a subtle trail of black and white feathers
            
            Vector2 hitboxCenter = hitbox.Center.ToVector2();
            float swingProgress = player.itemAnimation / (float)player.itemAnimationMax;
            float trailIntensity = (float)Math.Sin(swingProgress * MathHelper.Pi);
            
            // === VERY SUBTLE DUAL-POLARITY FEATHER WAKE ===
            // Rare feather spawns - accent only, not primary effect
            if (Main.rand.NextBool(15))
            {
                Vector2 featherPos = hitboxCenter + Main.rand.NextVector2Circular(hitbox.Width * 0.3f, hitbox.Height * 0.3f);
                Color featherColor = Main.rand.NextBool() ? UnifiedVFX.SwanLake.Black : UnifiedVFX.SwanLake.White;
                CustomParticles.SwanFeatherDrift(featherPos, featherColor, 0.25f);
            }
            
            // === SUBTLE RAINBOW EDGE SHIMMER ===
            if (trailIntensity > 0.4f && Main.rand.NextBool(8))
            {
                Vector2 shimmerPos = hitboxCenter + Main.rand.NextVector2Circular(hitbox.Width * 0.25f, hitbox.Height * 0.25f);
                float hue = Main.rand.NextFloat();
                Color rainbowColor = Main.hslToRgb(hue, 0.8f, 0.75f);
                CustomParticles.PrismaticSparkle(shimmerPos, rainbowColor, 0.2f);
            }
            
            // === SUBTLE GLOW TRAIL ===
            if (Main.rand.NextBool(10))
            {
                Vector2 glowPos = hitboxCenter + Main.rand.NextVector2Circular(12f, 12f);
                Color glowColor = Color.Lerp(UnifiedVFX.SwanLake.White, UnifiedVFX.SwanLake.Silver, Main.rand.NextFloat());
                CustomParticles.GenericFlare(glowPos, glowColor, 0.15f + trailIntensity * 0.1f, 8);
            }
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<FlameOfTheSwan>(), 360); // 6 seconds
            
            // === UnifiedVFX SWAN LAKE IMPACT! ===
            UnifiedVFX.SwanLake.Impact(target.Center, 1.4f);
            
            // Enhanced monochrome burst into rainbow explosion!
            ThemedParticles.SwanLakeRainbowExplosion(target.Center, 1.1f);
            
                // HEAVY black/white spark explosion with GRADIENT to rainbow!
            for (int i = 0; i < 20; i++)
            {
                // GRADIENT: Black → White with rainbow shimmer overlay
                float progress = (float)i / 20f;
                Color baseColor = Color.Lerp(Color.Black, Color.White, progress);
                // Add rainbow shimmer overlay
                float hue = (progress + Main.GameUpdateCount * 0.01f) % 1f;
                Color rainbowShimmer = Main.hslToRgb(hue, 0.5f, 0.8f);
                Color finalColor = Color.Lerp(baseColor, rainbowShimmer, 0.3f);
                Color col = i % 2 == 0 ? Color.White : Color.Black;
                int dustType = i % 2 == 0 ? DustID.WhiteTorch : DustID.Shadowflame;
                Vector2 vel = Main.rand.NextVector2Circular(8f, 8f);
                Dust d = Dust.NewDustPerfect(target.Center, dustType, vel, i % 2 == 0 ? 0 : 100, col, 2.0f);
                d.noGravity = true;
                d.fadeIn = 1.4f;
            }
            
            // Multiple music notes on hit!
            ThemedParticles.SwanLakeMusicNotes(target.Center, 8, 40f);
            ThemedParticles.SwanLakeAccidentals(target.Center, 4, 30f);
            
            // Halo rings!
            CustomParticles.HaloRing(target.Center, Color.White, 0.8f, 30);
            CustomParticles.HaloRing(target.Center, Color.Black, 0.6f, 25);
            
            // Swan feather burst on impact!
            CustomParticles.SwanFeatherBurst(target.Center, 6, 0.35f);
            
            // Rainbow explosion on critical hits - DEVASTATING!
            if (hit.Crit)
            {
                ThemedParticles.SwanLakeRainbowExplosion(target.Center, 1.98f);
                ThemedParticles.SwanLakeMusicalImpact(target.Center, 1.35f, true);
                
                // MASSIVE rainbow flare burst!
                for (int i = 0; i < 16; i++)
                {
                    float hue = i / 16f;
                    Color flareColor = Main.hslToRgb(hue, 1f, 0.8f);
                    CustomParticles.GenericFlare(target.Center + Main.rand.NextVector2Circular(20f, 20f), flareColor, 0.9f, 30);
                }
                
                // Multiple stacked halo rings!
                for (int ring = 0; ring < 4; ring++)
                {
                    float hue = (Main.GameUpdateCount * 0.02f + ring * 0.25f) % 1f;
                    Color ringColor = Main.hslToRgb(hue, 1f, 0.75f);
                    CustomParticles.HaloRing(target.Center, ringColor, 0.7f + ring * 0.2f, 25 + ring * 6);
                }
                
                // Huge radial spark explosion!
                for (int i = 0; i < 32; i++)
                {
                    float angle = MathHelper.TwoPi * i / 32f;
                    float hue = i / 32f;
                    Color sparkColor = Main.hslToRgb(hue, 1f, 0.7f);
                    Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(6f, 14f);
                    Dust spark = Dust.NewDustPerfect(target.Center, DustID.RainbowTorch, vel, 0, sparkColor, 2.3f);
                    spark.noGravity = true;
                    spark.fadeIn = 1.5f;
                }
                
                // Crit sound and light
                SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with { Volume = 0.6f, Pitch = 0.4f }, target.Center);
                Lighting.AddLight(target.Center, 2f, 2f, 2.5f);
            }
            else
            {
                Lighting.AddLight(target.Center, 1.2f, 1.2f, 1.5f);
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            bool empowered = IsEmpowered(player.whoAmI);
            
            if (empowered)
            {
                // === ABSOLUTELY DEVASTATING EMPOWERED SWING! ===
                ConsumeEmpowerment(player.whoAmI);
                
                // Fire 8 flares at double damage in a spread!
                float spreadAngle = MathHelper.ToRadians(50f);
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.Lerp(-spreadAngle, spreadAngle, i / 7f);
                    Vector2 flareVel = velocity.RotatedBy(angle) * Main.rand.NextFloat(0.9f, 1.15f);
                    int flareType = i % 2;
                    
                    Projectile.NewProjectile(source, position, flareVel, type, damage * 2, knockback, player.whoAmI, flareType, 1);
                }
                
                // MASSIVE rainbow explosion from monochrome burst!
                ThemedParticles.SwanLakeRainbowExplosion(position, 2.7f);
                ThemedParticles.SwanLakeMusicalImpact(position, 2.25f, true);
                
                // Stacked shockwave rings!
                for (int ring = 0; ring < 6; ring++)
                {
                    float hue = (Main.GameUpdateCount * 0.02f + ring * 0.16f) % 1f;
                    Color ringColor = Main.hslToRgb(hue, 1f, 0.8f);
                    CustomParticles.HaloRing(position, ringColor, 0.5f + ring * 0.125f, 20 + ring * 5);
                }
                CustomParticles.HaloRing(position, Color.Black, 0.9f, 28);
                CustomParticles.HaloRing(position, Color.White, 0.75f, 25);
                
                // Rainbow sparkle flares!
                ThemedParticles.SwanLakeSparkles(position, 30, 55f);
                for (int i = 0; i < 12; i++)
                {
                    float flareHue = i / 12f;
                    Color flareColor = Main.hslToRgb(flareHue, 1f, 0.8f);
                    CustomParticles.GenericFlare(position + Main.rand.NextVector2Circular(20f, 20f), flareColor, 0.6f, 22);
                }
                
                // HUGE music notes burst!
                ThemedParticles.SwanLakeMusicNotes(position, 20, 70f);
                ThemedParticles.SwanLakeAccidentals(position, 10, 55f);
                ThemedParticles.SwanLakeFeathers(position, 15, 60f);
                
                // DEVASTATING feather explosion!
                CustomParticles.SwanFeatherExplosion(position, 12, 0.5f);
                
                // MASSIVE rainbow spark explosion!
                for (int i = 0; i < 48; i++)
                {
                    float angle = MathHelper.TwoPi * i / 48f;
                    float hue = i / 48f;
                    Color sparkColor = Main.hslToRgb(hue, 1f, 0.75f);
                    Vector2 sparkVel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(10f, 20f);
                    Dust spark = Dust.NewDustPerfect(position, DustID.RainbowTorch, sparkVel, 0, sparkColor, 2.8f);
                    spark.noGravity = true;
                    spark.fadeIn = 1.6f;
                }
                
                // Explosive black/white core!
                for (int i = 0; i < 24; i++)
                {
                    Color col = i % 2 == 0 ? Color.White : Color.Black;
                    int dustType = i % 2 == 0 ? DustID.WhiteTorch : DustID.Shadowflame;
                    Vector2 vel = velocity.SafeNormalize(Vector2.Zero).RotatedBy(Main.rand.NextFloat(-1f, 1f)) * Main.rand.NextFloat(8f, 16f);
                    Dust d = Dust.NewDustPerfect(position, dustType, vel, i % 2 == 0 ? 0 : 100, col, 2.5f);
                    d.noGravity = true;
                    d.fadeIn = 1.5f;
                }
                
                // MASSIVE light explosion!
                Lighting.AddLight(position, 3f, 3f, 3.5f);
                
                SoundEngine.PlaySound(SoundID.Item122 with { Volume = 1.1f, Pitch = -0.1f }, position);
                SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with { Volume = 0.9f, Pitch = 0.2f }, position);
            }
            else
            {
                // === ENHANCED NORMAL SWING - Still flashy! ===
                ResetFlareCount(player.whoAmI);
                
                float spreadAngle = MathHelper.ToRadians(30f);
                for (int i = 0; i < 3; i++)
                {
                    float angle = MathHelper.Lerp(-spreadAngle, spreadAngle, i / 2f);
                    Vector2 flareVel = velocity.RotatedBy(angle);
                    int flareType = i % 2;
                    
                    Projectile.NewProjectile(source, position, flareVel, type, damage, knockback, player.whoAmI, flareType, 0);
                }
                
                // Enhanced swing effect with rainbow accents!
                ThemedParticles.SwanLakeSparks(position, velocity.SafeNormalize(Vector2.Zero), 15, 10f);
                ThemedParticles.SwanLakeSparkles(position, 12, 40f);
                ThemedParticles.SwanLakeBloomBurst(position, 0.8f);
                
                // Rainbow flares on swing!
                for (int i = 0; i < 8; i++)
                {
                    float hue = i / 8f;
                    Color flareColor = Main.hslToRgb(hue, 1f, 0.7f);
                    CustomParticles.GenericFlare(position + Main.rand.NextVector2Circular(15f, 15f), flareColor, 0.55f, 20);
                }
                
                // Halo rings (reduced size)
                CustomParticles.HaloRing(position, Color.White, 0.3f, 11);
                CustomParticles.HaloRing(position, Color.Black, 0.2f, 9);
                
                // Rainbow sparkle flares!
                ThemedParticles.SwanLakeSparkles(position, 15, 35f);
                
                // Swan feather duality - black and white feathers
                CustomParticles.SwanFeatherDuality(position, 4, 0.3f);
                
                // Spark burst
                for (int i = 0; i < 16; i++)
                {
                    Color col = i % 2 == 0 ? Color.White : Color.Black;
                    int dustType = i % 2 == 0 ? DustID.WhiteTorch : DustID.Shadowflame;
                    Vector2 vel = velocity.SafeNormalize(Vector2.Zero).RotatedBy(Main.rand.NextFloat(-0.6f, 0.6f)) * Main.rand.NextFloat(5f, 10f);
                    Dust d = Dust.NewDustPerfect(position, dustType, vel, i % 2 == 0 ? 0 : 100, col, 1.6f);
                    d.noGravity = true;
                }
                
                Lighting.AddLight(position, 1.2f, 1.2f, 1.5f);
            }
            
            return false;
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.15f + 1f;
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Black ominous glow
            spriteBatch.Draw(texture, position, null, Color.Black * 0.5f, rotation, origin, scale * 0.9f * pulse * 1.4f, SpriteEffects.None, 0f);
            // White inner glow
            spriteBatch.Draw(texture, position, null, Color.White * 0.3f, rotation, origin, scale * 0.9f * pulse * 1.2f, SpriteEffects.None, 0f);
            
            // Rainbow shimmer
            float hue = (float)Main.GameUpdateCount * 0.004f % 1f;
            Color rainbow = Main.hslToRgb(hue, 0.7f, 0.6f);
            spriteBatch.Draw(texture, position, null, rainbow * 0.2f, rotation, origin, scale * 0.9f * pulse * 1.1f, SpriteEffects.None, 0f);
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            spriteBatch.Draw(texture, position, null, lightColor, rotation, origin, scale * 0.9f, SpriteEffects.None, 0f);
            
            Lighting.AddLight(Item.Center, 0.4f, 0.4f, 0.5f);
            
            return false;
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            spriteBatch.Draw(texture, position, frame, drawColor, 0f, origin, scale * 0.9f, SpriteEffects.None, 0f);
            return false;
        }
    }

    public class BlackSwanFlare : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RainbowRodBullet;

        private bool isBlack => Projectile.ai[0] == 0;
        private bool isEmpowered => Projectile.ai[1] == 1;

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // === HOMING/TRACKING BEHAVIOR ===
            float homingRange = 300f;
            float homingStrength = 0.08f;
            NPC closestNPC = null;
            float closestDist = homingRange;
            
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.CanBeChasedBy() && !npc.friendly)
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closestNPC = npc;
                    }
                }
            }
            
            if (closestNPC != null)
            {
                // Use varied target point so projectiles don't all converge on the same spot
                Vector2 targetPoint = TargetingUtilities.GetVariedTargetPoint(closestNPC, Projectile.whoAmI);
                Vector2 targetDir = (targetPoint - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetDir * Projectile.velocity.Length(), homingStrength);
            }

            // === MASSIVE BLACK AND WHITE BLAZING TRAIL! ===
            Color trailColor = isBlack ? new Color(20, 20, 30) : new Color(255, 255, 255);
            
            // HEAVY main trail - constant flow of particles!
            for (int i = 0; i < 3; i++)
            {
                int dustType = isBlack ? DustID.Shadowflame : DustID.WhiteTorch;
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4f, 4f), dustType, 
                    -Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(2f, 2f),
                    isBlack ? 100 : 0, trailColor, isEmpowered ? 1.4f : 2.0f);
                d.noGravity = true;
                d.fadeIn = 1.4f;
            }

            // CONSTANT swirling contrasting spiral!
            float angle = Main.GameUpdateCount * 0.4f;
            for (int i = 0; i < 2; i++)
            {
                float spiralAngle = angle + i * MathHelper.Pi;
                Vector2 spiralOffset = new Vector2((float)Math.Cos(spiralAngle), (float)Math.Sin(spiralAngle)) * 6f;
                
                int spiralDustType = isBlack ? DustID.WhiteTorch : DustID.Shadowflame;
                Color spiralColor = isBlack ? Color.White : Color.Black;
                Dust spiral = Dust.NewDustPerfect(Projectile.Center + spiralOffset, spiralDustType, 
                    -Projectile.velocity * 0.1f, isBlack ? 0 : 100, spiralColor, isEmpowered ? 1.0f : 1.5f);
                spiral.noGravity = true;
            }
            
            // Rainbow shimmer particles along trail!
            if (Main.rand.NextBool(2))
            {
                float hue = Main.rand.NextFloat();
                Color rainbow = Main.hslToRgb(hue, 1f, 0.7f);
                Dust r = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowTorch,
                    -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1.5f, 1.5f), 0, rainbow, 1.5f);
                r.noGravity = true;
            }
            
            // Frequent flare particles!
            if (Main.rand.NextBool(3))
            {
                Color flareCol = isBlack ? Color.Black : Color.White;
                CustomParticles.GenericFlare(Projectile.Center, flareCol, 0.4f, 15);
            }
            
            // Rainbow flares occasionally
            if (Main.rand.NextBool(5))
            {
                float hue = Main.rand.NextFloat();
                CustomParticles.GenericFlare(Projectile.Center, Main.hslToRgb(hue, 1f, 0.7f), 0.35f, 15);
            }
            
            // Ambient fractal gem sparkle
            if (Main.rand.NextBool(8))
            {
                ThemedParticles.SwanLakeFractalTrail(Projectile.Center, 0.4f);
            }

            // BRIGHT pulsing light!
            float intensity = isBlack ? 0.5f : 0.9f;
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.2f + 1f;
            Lighting.AddLight(Projectile.Center, intensity * pulse, intensity * pulse, (intensity + 0.1f) * pulse);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply debuff
            target.AddBuff(ModContent.BuffType<FlameOfTheSwan>(), 240); // 4 seconds

            // Register hit for empowerment (only non-empowered flares count)
            if (!isEmpowered)
            {
                CalloftheBlackSwan.RegisterFlareHit(Projectile.owner);
            }

            // Explosion effect
            CreateFlareExplosion(target.Center);
        }

        private void CreateFlareExplosion(Vector2 position)
        {
            float scale = isEmpowered ? 0.8f : 1.3f;
            
            // === EXPLOSIVE IMPACT! ===
            
            // Monochrome impact that transitions to MASSIVE rainbow!
            ThemedParticles.SwanLakeImpact(position, scale * 1.5f);
            ThemedParticles.SwanLakeRainbowExplosion(position, scale * 1.2f);
            
            // HUGE rainbow sparkles burst!
            ThemedParticles.SwanLakeSparkles(position, (int)(20 * scale), 50f * scale);
            
            // Multiple music notes on impact!
            ThemedParticles.SwanLakeMusicNotes(position, (int)(8 * scale), 35f * scale);
            ThemedParticles.SwanLakeAccidentals(position, (int)(4 * scale), 25f * scale);
            
            // Stacked halo rings (reduced size)!
            for (int ring = 0; ring < 3; ring++)
            {
                float hue = (Main.GameUpdateCount * 0.02f + ring * 0.33f) % 1f;
                Color ringColor = Main.hslToRgb(hue, 1f, 0.75f);
                CustomParticles.HaloRing(position, ringColor, 0.25f * scale + ring * 0.075f, (int)(10 * scale) + ring * 3);
            }
            CustomParticles.HaloRing(position, Color.White, 0.35f * scale, (int)(12 * scale));
            CustomParticles.HaloRing(position, Color.Black, 0.25f * scale, (int)(10 * scale));
            
            // Rainbow sparkle flares!
            ThemedParticles.SwanLakeSparkles(position, (int)(12 * scale), 30f * scale);
            
            // Rainbow flare burst!
            for (int i = 0; i < (int)(10 * scale); i++)
            {
                float hue = i / (10f * scale);
                Color flareColor = Main.hslToRgb(hue, 1f, 0.75f);
                CustomParticles.GenericFlare(position + Main.rand.NextVector2Circular(15f, 15f), flareColor, 0.65f * scale, 22);
            }
            
            // Radial spark explosion!
            for (int i = 0; i < (int)(24 * scale); i++)
            {
                float angle = MathHelper.TwoPi * i / (24f * scale);
                float hue = i / (24f * scale);
                Color sparkColor = Main.hslToRgb(hue, 1f, 0.7f);
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(5f, 10f) * scale;
                Dust spark = Dust.NewDustPerfect(position, DustID.RainbowTorch, vel, 0, sparkColor, 1.8f * scale);
                spark.noGravity = true;
                spark.fadeIn = 1.4f;
            }
            
            // Black/white contrast burst!
            for (int i = 0; i < (int)(12 * scale); i++)
            {
                Color col = i % 2 == 0 ? Color.White : Color.Black;
                int dustType = i % 2 == 0 ? DustID.WhiteTorch : DustID.Shadowflame;
                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f) * scale;
                Dust d = Dust.NewDustPerfect(position, dustType, vel, i % 2 == 0 ? 0 : 100, col, 1.8f * scale);
                d.noGravity = true;
                d.fadeIn = 1.3f;
            }
            
            // BRIGHT light burst!
            float lightIntensity = isEmpowered ? 1.2f : 1.8f;
            Lighting.AddLight(position, lightIntensity, lightIntensity, lightIntensity + 0.3f);

            SoundEngine.PlaySound(SoundID.Item93 with { Volume = 0.7f * scale, Pitch = isEmpowered ? 0.6f : 0.1f }, position);
        }

        public override void OnKill(int timeLeft)
        {
            // EXPLOSIVE death burst!
            for (int i = 0; i < 12; i++)
            {
                Color col = i % 2 == 0 ? Color.White : Color.Black;
                int dustType = i % 2 == 0 ? DustID.WhiteTorch : DustID.Shadowflame;
                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, dustType, vel, i % 2 == 0 ? 0 : 100, col, isEmpowered ? 1.2f : 1.8f);
                d.noGravity = true;
                d.fadeIn = 1.3f;
            }
            
            // Rainbow death sparks!
            for (int i = 0; i < 6; i++)
            {
                float hue = i / 6f;
                Color rainbow = Main.hslToRgb(hue, 1f, 0.7f);
                Dust r = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowTorch,
                    Main.rand.NextVector2Circular(4f, 4f), 0, rainbow, 1.4f);
                r.noGravity = true;
            }
            
            // Small halo
            CustomParticles.HaloRing(Projectile.Center, isBlack ? Color.Black : Color.White, 0.35f, 12);
            
            // Fractal gem burst on death!
            ThemedParticles.SwanLakeFractalGemBurst(Projectile.Center, isBlack ? Color.Black : Color.White, 0.7f, 5, false);
            
            // Music notes on death!
            ThemedParticles.SwanLakeMusicNotes(Projectile.Center, 4, 25f);
            
            Lighting.AddLight(Projectile.Center, 0.8f, 0.8f, 1f);
        }
        
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // === EXPLOSIVE GROUND/WALL HIT EFFECT! ===
            CreateFlareExplosion(Projectile.Center);
            
            // Extra custom flares on tile hit!
            for (int i = 0; i < 8; i++)
            {
                float hue = i / 8f;
                Color flareColor = Main.hslToRgb(hue, 1f, 0.8f);
                CustomParticles.GenericFlare(Projectile.Center + Main.rand.NextVector2Circular(15f, 15f), flareColor, 0.5f, 18);
            }
            
            // Swan Lake flare
            CustomParticles.SwanLakeFlare(Projectile.Center, 0.45f);
            
            // Sparkles burst
            ThemedParticles.SwanLakeSparkles(Projectile.Center, 15, 30f);
            
            // Music notes on impact!
            ThemedParticles.SwanLakeMusicNotes(Projectile.Center, 6, 35f);
            ThemedParticles.SwanLakeAccidentals(Projectile.Center, 3, 25f);
            
            // Sound effect
            SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.5f, Pitch = 0.2f }, Projectile.Center);
            
            return true; // Destroy projectile
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            float scale = isEmpowered ? 0.6f : 0.975f; // 25% smaller
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.2f) * 0.2f + 1f;
            
            Color mainColor = isBlack ? new Color(15, 15, 20) : new Color(255, 255, 255);
            Color glowColor = isBlack ? new Color(30, 30, 40) * 0.9f : Color.White * 0.95f;
            Color oppositeGlow = isBlack ? Color.White * 0.5f : new Color(20, 20, 30) * 0.5f;
            
            // Rainbow outer aura - cycling!
            float hue = (Main.GameUpdateCount * 0.025f) % 1f;
            Color rainbow = Main.hslToRgb(hue, 1f, 0.7f);
            Main.EntitySpriteDraw(texture, drawPos, null, rainbow * 0.5f, Projectile.rotation, origin, scale * pulse * 2.25f, SpriteEffects.None, 0);
            
            // Second rainbow layer (offset hue)
            Color rainbow2 = Main.hslToRgb((hue + 0.5f) % 1f, 0.9f, 0.65f);
            Main.EntitySpriteDraw(texture, drawPos, null, rainbow2 * 0.4f, Projectile.rotation, origin, scale * pulse * 1.875f, SpriteEffects.None, 0);
            
            // Main glow layers
            Main.EntitySpriteDraw(texture, drawPos, null, glowColor * 0.6f, Projectile.rotation, origin, scale * pulse * 1.65f, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(texture, drawPos, null, oppositeGlow, Projectile.rotation, origin, scale * pulse * 1.35f, SpriteEffects.None, 0);
            
            // Core glow
            Main.EntitySpriteDraw(texture, drawPos, null, glowColor * 0.8f, Projectile.rotation, origin, scale * pulse * 1.05f, SpriteEffects.None, 0);
            
            // Main sprite
            Main.EntitySpriteDraw(texture, drawPos, null, mainColor, Projectile.rotation, origin, scale * pulse * 0.75f, SpriteEffects.None, 0);

            return false;
        }
    }
}
