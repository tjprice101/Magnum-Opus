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
    /// Iridescent Wingspan - Magic weapon that fires 3 black and white flares in a 60 degree cone.
    /// On hit, creates rings and flares of pearlescent rainbow explosions.
    /// Rainbow (Swan) rarity, no crafting recipe.
    /// </summary>
    public class IridescentWingspan : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
            Item.staff[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = 420; // Higher than Eroica weapons
            Item.DamageType = DamageClass.Magic;
            Item.width = 50;
            Item.height = 50;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 6f;
            Item.value = Item.sellPrice(gold: 55);
            Item.rare = ModContent.RarityType<SwanRarity>();
            Item.UseSound = SoundID.Item30 with { Pitch = 0.4f, Volume = 0.7f }; // Fractal crystal sound
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<IridescentWingspanProjectile>();
            Item.shootSpeed = 16f;
            Item.mana = 16;
            Item.noMelee = true;
            Item.scale = 0.9f; // 90% size
        }

        public override void HoldItem(Player player)
        {
            // === Elegant black and white pulsing with prismatic sparkles ===
            
            // Magic sparkle field aura - graceful enchantment glow
            if (Main.rand.NextBool(10))
            {
                Vector2 offset = Main.rand.NextVector2Circular(30f, 30f);
                Color fieldColor = Main.rand.NextBool() ? Color.White * 0.6f : CustomParticleSystem.SwanLakeColors.IcyBlue * 0.5f;
                CustomParticles.MagicSparkleFieldAura(player.Center + offset, fieldColor, 0.3f, 28);
            }
            
            // Ambient prismatic sparkle dust
            if (Main.rand.NextBool(8))
            {
                CustomParticles.PrismaticSparkleAmbient(player.Center, CustomParticleSystem.SwanLakeColors.Silver, 28f, 2);
            }
            
            // Elegant floating feathers
            if (Main.rand.NextBool(10))
            {
                CustomParticles.SwanFeatherAura(player.Center, 30f, 2);
            }
            
            // Occasional black/white particles - reduced
            if (Main.rand.NextBool(5))
            {
                Vector2 offset = Main.rand.NextVector2Circular(25f, 25f);
                Color col = Main.rand.NextBool() ? Color.White : new Color(20, 20, 30);
                int dustType = Main.rand.NextBool() ? DustID.WhiteTorch : DustID.Shadowflame;
                Dust d = Dust.NewDustPerfect(player.Center + offset, dustType, Main.rand.NextVector2Circular(1f, 1f),
                    col == Color.White ? 0 : 100, col, 1.1f);
                d.noGravity = true;
            }
            
            // Pulsing rainbow light - softer
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 0.12f + 0.85f;
            float hueLight = (Main.GameUpdateCount * 0.012f) % 1f;
            Vector3 lightColor = Main.hslToRgb(hueLight, 0.5f, 0.6f).ToVector3();
            Lighting.AddLight(player.Center, lightColor * pulse * 0.6f);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Fire 3 projectiles in a 60 degree cone
            Vector2 towardsMouse = velocity.SafeNormalize(Vector2.UnitX);
            float spreadAngle = MathHelper.ToRadians(60f);
            
            for (int i = 0; i < 3; i++)
            {
                float angleOffset = spreadAngle * ((float)i / 2f - 0.5f);
                Vector2 projectileVelocity = towardsMouse.RotatedBy(angleOffset) * Item.shootSpeed;
                
                Projectile.NewProjectile(source, player.Center, projectileVelocity, type, damage, knockback, player.whoAmI, i);
            }
            
            // === Elegant casting particles with sword arcs and prismatic effects ===
            Vector2 castPos = player.Center + towardsMouse * 35f;
            
            // Sword arc double helix - black and white intertwined slashes
            CustomParticles.SwordArcDoubleHelix(castPos, towardsMouse * 5f, Color.White, Color.Black, 0.5f);
            
            // Magic sparkle field burst - enchanted cast
            CustomParticles.MagicSparkleFieldBurst(castPos, CustomParticleSystem.SwanLakeColors.PureWhite, 5, 25f);
            
            // Prismatic rainbow sparkle burst
            CustomParticles.PrismaticSparkleRainbow(castPos, 8);
            
            // Themed effects - reduced counts
            ThemedParticles.SwanLakeSparks(castPos, towardsMouse, 10, 8f);
            ThemedParticles.SwanLakeBloomBurst(castPos, 0.7f);
            
            // Elegant halo rings - fewer but impactful
            CustomParticles.HaloRing(castPos, Color.White, 0.7f, 25);
            CustomParticles.HaloRing(castPos, Color.Black, 0.5f, 20);
            
            // Swan feather spiral on cast!
            CustomParticles.SwanFeatherSpiral(castPos, Color.White, 6);
            
            // Music notes - reduced
            ThemedParticles.SwanLakeMusicNotes(castPos, 5, 28f);
            
            // Black/white spark accent - reduced
            for (int i = 0; i < 12; i++)
            {
                Color col = i % 2 == 0 ? Color.White : new Color(20, 20, 30);
                int dustType = i % 2 == 0 ? DustID.WhiteTorch : DustID.Shadowflame;
                Vector2 vel = towardsMouse.RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * Main.rand.NextFloat(4f, 10f);
                Dust d = Dust.NewDustPerfect(castPos, dustType, vel, i % 2 == 0 ? 0 : 100, col, 1.5f);
                d.noGravity = true;
            }
            
            // Elegant casting light
            Lighting.AddLight(castPos, 1.4f, 1.4f, 1.7f);
            
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
            
            // White outer glow
            spriteBatch.Draw(texture, position, null, Color.White * 0.4f, rotation, origin, scale * 0.9f * pulse * 1.3f, SpriteEffects.None, 0f);
            // Black inner shadow
            spriteBatch.Draw(texture, position, null, Color.Black * 0.3f, rotation, origin, scale * 0.9f * pulse * 1.15f, SpriteEffects.None, 0f);
            // Pearlescent shimmer
            Color pearlColor = Color.Lerp(new Color(255, 240, 245), new Color(240, 245, 255), (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 0.5f + 0.5f);
            spriteBatch.Draw(texture, position, null, pearlColor * 0.25f, rotation, origin, scale * 0.9f * pulse * 1.1f, SpriteEffects.None, 0f);
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Draw main sprite at 90% scale
            spriteBatch.Draw(texture, position, null, lightColor, rotation, origin, scale * 0.9f, SpriteEffects.None, 0f);
            
            Lighting.AddLight(Item.Center, 0.5f, 0.5f, 0.6f);
            
            return false;
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            // Draw at 90% scale in inventory
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            spriteBatch.Draw(texture, position, frame, drawColor, 0f, origin, scale * 0.9f, SpriteEffects.None, 0f);
            return false;
        }
    }

    public class IridescentWingspanProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/SwanLake/ResonantWeapons/IridescentWingspan";

        private bool isBlack => Projectile.ai[0] % 2 == 0;

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            // Rotation
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // === MASSIVE Black and white trailing particles! ===
            Color trailColor = isBlack ? new Color(20, 20, 30) : Color.White;
            Color oppositeColor = isBlack ? Color.White : new Color(20, 20, 30);
            
            // HEAVY main trail - constant flow!
            for (int i = 0; i < 2; i++)
            {
                int dustType = isBlack ? DustID.Shadowflame : DustID.WhiteTorch;
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(5f, 5f), dustType,
                    -Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(2f, 2f),
                    isBlack ? 100 : 0, trailColor, 1.8f);
                d.noGravity = true;
                d.fadeIn = 1.4f;
            }
            
            // Contrasting sparkles!
            if (Main.rand.NextBool(2))
            {
                int oppDustType = isBlack ? DustID.WhiteTorch : DustID.Shadowflame;
                Dust opp = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f), oppDustType,
                    -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1.5f, 1.5f),
                    isBlack ? 0 : 100, oppositeColor, 1.4f);
                opp.noGravity = true;
            }
            
            // Frequent flare effects!
            if (Main.rand.NextBool(2))
            {
                CustomParticles.GenericFlare(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f), trailColor, 0.5f, 18);
            }
            
            // Rainbow shimmer trail!
            if (Main.rand.NextBool(3))
            {
                float hue = Main.rand.NextFloat();
                Color rainbow = Main.hslToRgb(hue, 1f, 0.7f);
                Dust r = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowTorch,
                    -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1f, 1f), 0, rainbow, 1.5f);
                r.noGravity = true;
            }
            
            // Pearlescent shimmer trail
            if (Main.rand.NextBool(4))
            {
                Color pearl = Main.rand.NextBool() ? new Color(255, 240, 250) : new Color(240, 250, 255);
                CustomParticles.GenericFlare(Projectile.Center, pearl, 0.35f, 15);
            }
            
            // Rainbow flares
            if (Main.rand.NextBool(5))
            {
                float hue = Main.rand.NextFloat();
                CustomParticles.GenericFlare(Projectile.Center, Main.hslToRgb(hue, 1f, 0.7f), 0.4f, 16);
            }
            
            // Swan feather trail
            if (Main.rand.NextBool(4))
            {
                CustomParticles.SwanFeatherTrail(Projectile.Center, Projectile.velocity, 0.25f);
            }

            // BRIGHT pulsing light!
            float lightIntensity = isBlack ? 0.5f : 1.0f;
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.2f + 1f;
            Lighting.AddLight(Projectile.Center, lightIntensity * pulse, lightIntensity * pulse, (lightIntensity + 0.15f) * pulse);
        }

        public override void OnKill(int timeLeft)
        {
            // Create pearlescent rainbow explosion
            CreateRainbowExplosion();
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply Flame of the Swan debuff
            target.AddBuff(ModContent.BuffType<FlameOfTheSwan>(), 300); // 5 seconds

            // Create rings and flares on hit
            CreateRainbowExplosion();
            
            // === EXTRA CUSTOM FLARES ON HIT! ===
            // Rainbow flare burst at impact!
            for (int i = 0; i < 8; i++)
            {
                float hue = i / 8f;
                Color flareColor = Main.hslToRgb(hue, 1f, 0.8f);
                CustomParticles.GenericFlare(target.Center + Main.rand.NextVector2Circular(18f, 18f), flareColor, 0.55f, 20);
            }
            
            // Swan Lake flare
            CustomParticles.SwanLakeFlare(target.Center, 0.5f);
            
            // Feather burst on hit
            CustomParticles.SwanFeatherBurst(target.Center, 5, 0.3f);
            
            // Extra sparkles
            ThemedParticles.SwanLakeSparkles(target.Center, 15, 30f);
        }

        private void CreateRainbowExplosion()
        {
            // === MASSIVE PEARLESCENT RAINBOW EXPLOSION! ===
            SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.9f, Pitch = 0.4f }, Projectile.Center); // Glass shatter
            SoundEngine.PlaySound(SoundID.Item107 with { Volume = 0.7f, Pitch = 0.5f }, Projectile.Center); // Crystal shatter
            SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with { Volume = 0.4f, Pitch = 0.6f }, Projectile.Center);
            
            // MASSIVE monochrome impact that explodes into rainbow!
            ThemedParticles.SwanLakeImpact(Projectile.Center, 1.6f);
            ThemedParticles.SwanLakeRainbowExplosion(Projectile.Center, 1.8f);
            ThemedParticles.SwanLakeMusicalImpact(Projectile.Center, 1.4f, true);
            
            // HUGE Music notes burst!
            ThemedParticles.SwanLakeMusicNotes(Projectile.Center, 12, 55f);
            ThemedParticles.SwanLakeAccidentals(Projectile.Center, 6, 45f);
            ThemedParticles.SwanLakeFeathers(Projectile.Center, 10, 50f);
            
            // Swan feather explosion!
            CustomParticles.SwanFeatherExplosion(Projectile.Center, 10, 0.4f);
            
            // Stacked halo rings (50% reduced)!
            for (int ring = 0; ring < 4; ring++)
            {
                float hue = (Main.GameUpdateCount * 0.02f + ring * 0.25f) % 1f;
                Color ringColor = Main.hslToRgb(hue, 1f, 0.8f);
                CustomParticles.HaloRing(Projectile.Center, ringColor, 0.4f + ring * 0.125f, 18 + ring * 4);
            }
            CustomParticles.HaloRing(Projectile.Center, Color.White, 0.65f, 25);
            CustomParticles.HaloRing(Projectile.Center, Color.Black, 0.5f, 22);
            
            // Rainbow sparkle flares!
            ThemedParticles.SwanLakeSparkles(Projectile.Center, 30, 45f);
            
            // HUGE rainbow flare burst!
            for (int i = 0; i < 14; i++)
            {
                float hue = i / 14f;
                Color flareColor = Main.hslToRgb(hue, 1f, 0.8f);
                CustomParticles.GenericFlare(Projectile.Center + Main.rand.NextVector2Circular(20f, 20f), flareColor, 0.85f, 30);
            }
            
            // Black and white explosion particles - MASSIVE!
            for (int i = 0; i < 24; i++)
            {
                float angle = MathHelper.TwoPi * i / 24f;
                Color bwColor = i % 2 == 0 ? Color.White : new Color(20, 20, 30);
                int dustType = i % 2 == 0 ? DustID.WhiteTorch : DustID.Shadowflame;
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(7f, 14f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, dustType, vel, i % 2 == 0 ? 0 : 100, bwColor, 2.3f);
                d.noGravity = true;
                d.fadeIn = 1.5f;
            }
            
            // HUGE rainbow spark explosion!
            for (int i = 0; i < 32; i++)
            {
                float angle = MathHelper.TwoPi * i / 32f;
                float hue = i / 32f;
                Color sparkColor = Main.hslToRgb(hue, 1f, 0.7f);
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(8f, 16f);
                Dust spark = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowTorch, vel, 0, sparkColor, 2.2f);
                spark.noGravity = true;
                spark.fadeIn = 1.5f;
            }
            
            // Pearlescent shimmer burst!
            ThemedParticles.SwanLakeSparkles(Projectile.Center, 25, 60f);
            
            // MASSIVE light burst!
            Lighting.AddLight(Projectile.Center, 2f, 2f, 2.5f);
        }
        
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // === EXPLOSIVE GROUND/WALL HIT EFFECT! ===
            CreateRainbowExplosion();
            
            // Extra custom flares on tile hit!
            for (int i = 0; i < 10; i++)
            {
                float hue = i / 10f;
                Color flareColor = Main.hslToRgb(hue, 1f, 0.8f);
                CustomParticles.GenericFlare(Projectile.Center + Main.rand.NextVector2Circular(18f, 18f), flareColor, 0.55f, 20);
            }
            
            // Swan Lake flare
            CustomParticles.SwanLakeFlare(Projectile.Center, 0.5f);
            
            // Sparkles burst
            ThemedParticles.SwanLakeSparkles(Projectile.Center, 20, 40f);
            
            // Music notes on impact!
            ThemedParticles.SwanLakeMusicNotes(Projectile.Center, 8, 40f);
            ThemedParticles.SwanLakeAccidentals(Projectile.Center, 4, 32f);
            
            // Sound effect
            SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.5f, Pitch = 0.1f }, Projectile.Center);
            
            return true; // Destroy projectile
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Custom draw with glow
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            Color glowColor = isBlack ? Color.Black * 0.8f : Color.White * 0.8f;
            float scale = 0.4f; // Small projectile
            
            // Glow layers
            Main.EntitySpriteDraw(texture, drawPos, null, glowColor * 0.5f, Projectile.rotation, origin, scale * 1.4f, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(texture, drawPos, null, glowColor * 0.3f, Projectile.rotation, origin, scale * 1.2f, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(texture, drawPos, null, Color.White, Projectile.rotation, origin, scale, SpriteEffects.None, 0);

            return false;
        }
    }
}
