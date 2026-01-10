using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.GameContent;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Content.SwanLake.Debuffs;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons
{
    /// <summary>
    /// Call of the Pearlescent Lake - Assault rifle that fires black and white flaming rockets.
    /// Creates rainbow pearlescent explosions on contact and ignites with Flame of the Swan.
    /// Rainbow (Swan) rarity, no crafting recipe.
    /// </summary>
    public class CallofthePearlescentLake : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 380; // Higher than Eroica weapons
            Item.DamageType = DamageClass.Ranged;
            Item.width = 60;
            Item.height = 30;
            Item.useTime = 8; // Fast assault rifle
            Item.useAnimation = 8;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(gold: 55);
            Item.rare = ModContent.RarityType<SwanRarity>();
            Item.UseSound = SoundID.Item30 with { Pitch = 0.2f, Volume = 0.75f }; // Fractal crystal sound
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<PearlescentRocket>();
            Item.shootSpeed = 18f;
            Item.useAmmo = AmmoID.Bullet; // Uses bullets but fires rockets
            Item.noMelee = true;
            Item.scale = 0.9f; // 90% size
        }

        public override void HoldItem(Player player)
        {
            // === EXPLOSIVE Black/white smoke effect while holding! ===
            for (int i = 0; i < 2; i++)
            {
                if (Main.rand.NextBool(3))
                {
                    Vector2 muzzleOffset = new Vector2(45f * player.direction, -5f + Main.rand.NextFloat(-3f, 3f));
                    Color col = Main.rand.NextBool() ? Color.White * 0.8f : new Color(30, 30, 40) * 0.9f;
                    int dustType = Main.rand.NextBool() ? DustID.WhiteTorch : DustID.Shadowflame;
                    Dust d = Dust.NewDustPerfect(player.Center + muzzleOffset, dustType, Main.rand.NextVector2Circular(2f, 2f) + new Vector2(0, -0.5f), 
                        col == Color.White ? 0 : 100, col, 1.2f);
                    d.noGravity = true;
                }
            }
            
            // Rainbow shimmer particles
            if (Main.rand.NextBool(6))
            {
                float hue = Main.rand.NextFloat();
                Color rainbow = Main.hslToRgb(hue, 1f, 0.7f);
                CustomParticles.GenericFlare(player.Center + new Vector2(30f * player.direction, 0) + Main.rand.NextVector2Circular(15f, 15f), rainbow, 0.35f, 15);
            }
            
            // Pearlescent shimmer
            if (Main.rand.NextBool(8))
            {
                CustomParticles.SwanLakeFlare(player.Center + Main.rand.NextVector2Circular(25f, 25f), 0.3f);
            }
            
            // Floating feather drift
            if (Main.rand.NextBool(12))
            {
                CustomParticles.SwanFeatherDrift(player.Center + new Vector2(35f * player.direction, 0), Color.White, 0.25f);
            }
            
            // Pulsing rainbow light
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 0.15f + 0.85f;
            float hueLight = (Main.GameUpdateCount * 0.012f) % 1f;
            Vector3 lightColor = Main.hslToRgb(hueLight, 0.5f, 0.6f).ToVector3();
            Lighting.AddLight(player.Center, lightColor * pulse * 0.7f);
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            // Override ammo type to always fire our rocket
            type = ModContent.ProjectileType<PearlescentRocket>();
            
            // Add slight spread for assault rifle feel
            velocity = velocity.RotatedByRandom(MathHelper.ToRadians(5f));
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // === MASSIVE Muzzle flash particles with EXPLOSIVE themed effects! ===
            Vector2 muzzlePos = position + velocity.SafeNormalize(Vector2.Zero) * 50f;
            Vector2 direction = velocity.SafeNormalize(Vector2.Zero);
            
            // HUGE rainbow spark burst in firing direction!
            ThemedParticles.SwanLakeSparks(muzzlePos, direction, 12, 10f);
            ThemedParticles.SwanLakeSparkles(muzzlePos, 8, 25f);
            ThemedParticles.SwanLakeBloomBurst(muzzlePos, 0.7f);
            
            // Multiple rainbow flares!
            for (int i = 0; i < 8; i++)
            {
                float hue = i / 8f;
                Color flareColor = Main.hslToRgb(hue, 1f, 0.75f);
                CustomParticles.GenericFlare(muzzlePos + Main.rand.NextVector2Circular(10f, 10f), flareColor, 0.6f, 20);
            }
            
            // Muzzle flash halo rings!
            CustomParticles.HaloRing(muzzlePos, Color.White, 0.6f, 20);
            CustomParticles.HaloRing(muzzlePos, Color.Black, 0.4f, 15);
            float hueRing = (Main.GameUpdateCount * 0.02f) % 1f;
            CustomParticles.HaloRing(muzzlePos, Main.hslToRgb(hueRing, 1f, 0.7f), 0.5f, 18);
            
            // Swan feather burst on shot!
            CustomParticles.SwanFeatherBurst(muzzlePos, 4, 0.25f);
            
            // HEAVY muzzle spark dust!
            for (int i = 0; i < 16; i++)
            {
                Color col = i % 2 == 0 ? Color.White : Color.Black;
                int dustType = i % 2 == 0 ? DustID.WhiteTorch : DustID.Shadowflame;
                Vector2 vel = direction.RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * Main.rand.NextFloat(5f, 12f);
                Dust d = Dust.NewDustPerfect(muzzlePos, dustType, vel, i % 2 == 0 ? 0 : 100, col, 1.8f);
                d.noGravity = true;
                d.fadeIn = 1.3f;
            }
            
            // Rainbow dust burst!
            for (int i = 0; i < 10; i++)
            {
                float hue = i / 10f;
                Color rainbow = Main.hslToRgb(hue, 1f, 0.7f);
                Vector2 vel = direction.RotatedBy(Main.rand.NextFloat(-0.4f, 0.4f)) * Main.rand.NextFloat(4f, 9f);
                Dust r = Dust.NewDustPerfect(muzzlePos, DustID.RainbowTorch, vel, 0, rainbow, 1.6f);
                r.noGravity = true;
            }
            
            // Bright muzzle light!
            Lighting.AddLight(muzzlePos, 1.5f, 1.5f, 1.8f);
            
            // Fire the rocket
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            
            return false;
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.1f + 1f;
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Alternating black/white glow
            Color glowCol = ((int)(Main.GameUpdateCount * 0.1f) % 2 == 0) ? Color.White * 0.4f : Color.Black * 0.3f;
            spriteBatch.Draw(texture, position, null, glowCol, rotation, origin, scale * 0.9f * pulse * 1.25f, SpriteEffects.None, 0f);
            
            // Pearlescent shimmer
            Color pearl = Color.Lerp(new Color(255, 200, 220), new Color(200, 220, 255), (float)Math.Sin(Main.GameUpdateCount * 0.07f) * 0.5f + 0.5f);
            spriteBatch.Draw(texture, position, null, pearl * 0.2f, rotation, origin, scale * 0.9f * pulse * 1.15f, SpriteEffects.None, 0f);
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Draw main sprite at 90% scale
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

    public class PearlescentRocket : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RocketI;

        private bool isBlackRocket => Projectile.localAI[0] == 0;

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.extraUpdates = 1;
        }

        public override void OnSpawn(IEntitySource source)
        {
            // Randomize black or white rocket
            Projectile.localAI[0] = Main.rand.NextBool() ? 0 : 1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // === MASSIVE BLACK AND WHITE BLAZING ROCKET TRAIL! ===
            Color trailColor = isBlackRocket ? new Color(20, 20, 30) : Color.White;
            Color oppositeColor = isBlackRocket ? Color.White : new Color(20, 20, 30);
            
            // HEAVY main flame trail - constant flow!
            for (int i = 0; i < 3; i++)
            {
                int dustType = isBlackRocket ? DustID.Shadowflame : DustID.WhiteTorch;
                Dust flame = Dust.NewDustPerfect(Projectile.Center - Projectile.velocity * 0.3f + Main.rand.NextVector2Circular(5f, 5f), 
                    dustType, 
                    -Projectile.velocity * 0.4f + Main.rand.NextVector2Circular(3f, 3f),
                    isBlackRocket ? 100 : 0, trailColor, 2.2f);
                flame.noGravity = true;
                flame.fadeIn = 1.5f;
            }
            
            // Contrasting sparkles - constant!
            for (int i = 0; i < 2; i++)
            {
                if (Main.rand.NextBool(2))
                {
                    int oppDustType = isBlackRocket ? DustID.WhiteTorch : DustID.Shadowflame;
                    Dust opp = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f), oppDustType,
                        -Projectile.velocity * 0.25f + Main.rand.NextVector2Circular(2f, 2f),
                        isBlackRocket ? 0 : 100, oppositeColor, 1.6f);
                    opp.noGravity = true;
                }
            }
            
            // Rainbow shimmer trail - HEAVY!
            for (int i = 0; i < 2; i++)
            {
                float hue = Main.rand.NextFloat();
                Color rainbow = Main.hslToRgb(hue, 1f, 0.75f);
                Dust r = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4f, 4f), DustID.RainbowTorch,
                    -Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(1.5f, 1.5f), 0, rainbow, 1.8f);
                r.noGravity = true;
                r.fadeIn = 1.4f;
            }
            
            // Frequent flare effects!
            if (Main.rand.NextBool(2))
            {
                CustomParticles.GenericFlare(Projectile.Center, oppositeColor, 0.45f, 18);
            }
            
            // Pearlescent shimmer particles
            if (Main.rand.NextBool(3))
            {
                Color pearl = Main.rand.NextBool() ? new Color(255, 230, 240) : new Color(230, 240, 255);
                Dust p = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(5f, 5f), DustID.WhiteTorch, 
                    -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1f, 1f), 30, pearl, 1.4f);
                p.noGravity = true;
            }
            
            // Rainbow flares along trail
            if (Main.rand.NextBool(4))
            {
                float hue = Main.rand.NextFloat();
                CustomParticles.GenericFlare(Projectile.Center, Main.hslToRgb(hue, 1f, 0.7f), 0.5f, 18);
            }
            
            // Mini halo rings along path occasionally
            if (Main.rand.NextBool(8))
            {
                CustomParticles.HaloRing(Projectile.Center, isBlackRocket ? Color.Black : Color.White, 0.25f, 10);
            }
            
            // Swan feather trail on rocket
            if (Main.rand.NextBool(5))
            {
                CustomParticles.SwanFeatherTrail(Projectile.Center, Projectile.velocity, 0.2f);
            }

            // BRIGHT pulsing light!
            float intensity = isBlackRocket ? 0.7f : 1.1f;
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.2f + 1f;
            float hueLight = (Main.GameUpdateCount * 0.02f) % 1f;
            Vector3 rainbowLight = Main.hslToRgb(hueLight, 0.6f, 0.6f).ToVector3();
            Lighting.AddLight(Projectile.Center, (rainbowLight + new Vector3(intensity)) * pulse * 0.5f);
        }

        public override void OnKill(int timeLeft)
        {
            CreatePearlescentExplosion();
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply Flame of the Swan debuff
            target.AddBuff(ModContent.BuffType<FlameOfTheSwan>(), 360); // 6 seconds
            
            // === EXPLOSIVE HIT EFFECT! ===
            CreateHitExplosion(target.Center);
        }
        
        private void CreateHitExplosion(Vector2 position)
        {
            // Rainbow impact burst!
            ThemedParticles.SwanLakeImpact(position, 0.9f);
            ThemedParticles.SwanLakeRainbowExplosion(position, 0.8f);
            
            // Rainbow sparkle flares!
            ThemedParticles.SwanLakeSparkles(position, 18, 35f);
            
            // Custom rainbow flares on hit!
            for (int i = 0; i < 8; i++)
            {
                float hue = i / 8f;
                Color flareColor = Main.hslToRgb(hue, 1f, 0.8f);
                CustomParticles.GenericFlare(position + Main.rand.NextVector2Circular(12f, 12f), flareColor, 0.5f, 18);
            }
            
            // B/W dust burst!
            for (int i = 0; i < 10; i++)
            {
                Color col = i % 2 == 0 ? Color.White : new Color(20, 20, 30);
                int dustType = i % 2 == 0 ? DustID.WhiteTorch : DustID.Shadowflame;
                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                Dust d = Dust.NewDustPerfect(position, dustType, vel, i % 2 == 0 ? 0 : 100, col, 1.4f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }
            
            // Small halo ring
            float haloHue = (Main.GameUpdateCount * 0.03f) % 1f;
            CustomParticles.HaloRing(position, Main.hslToRgb(haloHue, 1f, 0.7f), 0.3f, 12);
            
            // Swan feather burst on hit!
            CustomParticles.SwanFeatherBurst(position, 5, 0.3f);
            
            // Lighting!
            float lightHue = (Main.GameUpdateCount * 0.025f) % 1f;
            Vector3 lightColor = Main.hslToRgb(lightHue, 0.8f, 0.7f).ToVector3();
            Lighting.AddLight(position, lightColor * 0.9f);
        }

        private void CreatePearlescentExplosion()
        {
            // === RAINBOW PEARLESCENT EXPLOSION (50% reduced size, more sparkles)! ===
            SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.9f, Pitch = 0.3f }, Projectile.Center); // Glass shatter
            SoundEngine.PlaySound(SoundID.Item107 with { Volume = 0.75f, Pitch = 0.4f }, Projectile.Center); // Crystal shatter
            SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with { Volume = 0.4f, Pitch = 0.5f }, Projectile.Center);
            
            // Monochrome to rainbow explosion (reduced)
            ThemedParticles.SwanLakeRainbowExplosion(Projectile.Center, 1.25f);
            ThemedParticles.SwanLakeMusicalImpact(Projectile.Center, 1.0f, true);
            ThemedParticles.SwanLakeImpact(Projectile.Center, 0.9f);
            
            // Stacked halo rings (50% reduced)!
            for (int ring = 0; ring < 4; ring++)
            {
                float hue = (Main.GameUpdateCount * 0.02f + ring * 0.25f) % 1f;
                Color ringColor = Main.hslToRgb(hue, 1f, 0.8f);
                CustomParticles.HaloRing(Projectile.Center, ringColor, 0.5f + ring * 0.15f, 20 + ring * 5);
            }
            CustomParticles.HaloRing(Projectile.Center, Color.White, 0.9f, 30);
            CustomParticles.HaloRing(Projectile.Center, Color.Black, 0.75f, 28);
            
            // Music notes burst (reduced)
            ThemedParticles.SwanLakeMusicNotes(Projectile.Center, 8, 35f);
            ThemedParticles.SwanLakeAccidentals(Projectile.Center, 4, 28f);
            ThemedParticles.SwanLakeFeathers(Projectile.Center, 6, 30f);
            
            // Swan feather explosion on rocket death!
            CustomParticles.SwanFeatherExplosion(Projectile.Center, 10, 0.4f);
            
            // EXTRA SPARKLE FLARES for added rainbow flare!
            ThemedParticles.SwanLakeSparkles(Projectile.Center, 50, 55f);
            
            // Rainbow flare explosion!
            for (int i = 0; i < 16; i++)
            {
                float hue = i / 16f;
                Color flareColor = Main.hslToRgb(hue, 1f, 0.8f);
                CustomParticles.GenericFlare(Projectile.Center + Main.rand.NextVector2Circular(20f, 20f), flareColor, 0.65f, 25);
            }
            
            // Black flame burst (reduced)
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(5f, 10f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Shadowflame, vel, 100, new Color(20, 20, 30), 1.8f);
                d.noGravity = true;
                d.fadeIn = 1.3f;
            }
            
            // White flame burst (reduced)
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f + MathHelper.Pi / 12f;
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(4f, 9f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch, vel, 0, Color.White, 1.6f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }
            
            // Rainbow spark explosion (reduced)!
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi * i / 20f;
                float hue = i / 20f;
                Color sparkColor = Main.hslToRgb(hue, 1f, 0.7f);
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(6f, 12f);
                Dust spark = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowTorch, vel, 0, sparkColor, 1.6f);
                spark.noGravity = true;
                spark.fadeIn = 1.3f;
            }
            
            // Extra pearlescent shimmer burst!
            ThemedParticles.SwanLakeSparkles(Projectile.Center, 20, 45f);
            
            // Light explosion (reduced)
            Lighting.AddLight(Projectile.Center, 1.5f, 1.5f, 1.8f);
        }
        
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // === EXPLOSIVE GROUND/WALL HIT EFFECT! ===
            CreatePearlescentExplosion();
            
            // Extra custom flares on tile hit!
            for (int i = 0; i < 10; i++)
            {
                float hue = i / 10f;
                Color flareColor = Main.hslToRgb(hue, 1f, 0.8f);
                CustomParticles.GenericFlare(Projectile.Center + Main.rand.NextVector2Circular(18f, 18f), flareColor, 0.6f, 22);
            }
            
            // Swan Lake flare
            CustomParticles.SwanLakeFlare(Projectile.Center, 0.5f);
            
            // Sparkles burst
            ThemedParticles.SwanLakeSparkles(Projectile.Center, 25, 40f);
            
            // Music notes on impact!
            ThemedParticles.SwanLakeMusicNotes(Projectile.Center, 8, 40f);
            ThemedParticles.SwanLakeAccidentals(Projectile.Center, 4, 30f);
            
            // Sound effect
            SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.5f, Pitch = 0.1f }, Projectile.Center);
            
            return true; // Destroy projectile
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            Color mainColor = isBlackRocket ? new Color(50, 50, 60) : new Color(240, 240, 255);
            
            // Glow effect
            Color glowColor = isBlackRocket ? Color.Black * 0.6f : Color.White * 0.7f;
            Main.EntitySpriteDraw(texture, drawPos, null, glowColor, Projectile.rotation, origin, 1.3f, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(texture, drawPos, null, mainColor, Projectile.rotation, origin, 1f, SpriteEffects.None, 0);

            return false;
        }
    }
}
