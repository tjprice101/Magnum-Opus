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
    /// The Swan's Lament - Ranger rifle that fires a shotgun spread of black and white flaming bullets.
    /// Creates halos of mass destruction, rainbow sparkles, and black/white lightning on hit.
    /// Rainbow (Swan) rarity, no crafting recipe.
    /// </summary>
    public class TheSwansLament : ModItem
    {
        // Texture file renamed to avoid apostrophe for cross-platform compatibility
        public override string Texture => "MagnumOpus/Content/SwanLake/ResonantWeapons/TheSwansLament";

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 180; // Per bullet, with 8-12 bullets = massive damage
            Item.DamageType = DamageClass.Ranged;
            Item.width = 60;
            Item.height = 25;
            Item.useTime = 35;
            Item.useAnimation = 35;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 5f;
            Item.value = Item.sellPrice(gold: 55);
            Item.rare = ModContent.RarityType<SwanRarity>();
            Item.UseSound = SoundID.Item29 with { Pitch = 0.3f, Volume = 0.8f }; // Fractal crystal sound
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<SwansLamentBullet>();
            Item.shootSpeed = 16f;
            Item.useAmmo = AmmoID.Bullet;
            Item.noMelee = true;
            Item.scale = 0.9f; // 90% size
        }

        public override void HoldItem(Player player)
        {
            // === UnifiedVFX SWAN LAKE AMBIENT AURA ===
            UnifiedVFX.SwanLake.Aura(player.Center, 32f, 0.28f);
            
            // === AMBIENT FRACTAL FLARES - dual-polarity with rainbow shimmer ===
            if (Main.rand.NextBool(7))
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float radius = Main.rand.NextFloat(30f, 60f);
                Vector2 flarePos = player.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                Color baseColor = Main.rand.NextBool() ? UnifiedVFX.SwanLake.Black : UnifiedVFX.SwanLake.White;
                Color rainbow = UnifiedVFX.SwanLake.GetRainbow(Main.rand.NextFloat());
                Color fractalColor = Color.Lerp(baseColor, rainbow, 0.35f);
                CustomParticles.GenericFlare(flarePos, fractalColor, 0.32f, 20);
                ThemedParticles.SwanLakeFractalTrail(flarePos, 0.25f);
            }
            
            // Rainbow shimmer particles at muzzle
            if (Main.rand.NextBool(6))
            {
                Color rainbow = UnifiedVFX.SwanLake.GetRainbow(Main.rand.NextFloat());
                Vector2 muzzleOffset = new Vector2(30f * player.direction, 0) + Main.rand.NextVector2Circular(15f, 15f);
                CustomParticles.GenericFlare(player.Center + muzzleOffset, rainbow, 0.32f, 16);
            }
            
            // Floating feathers - dual-polarity
            if (Main.rand.NextBool(10))
            {
                Color featherColor = Main.rand.NextBool() ? UnifiedVFX.SwanLake.White : UnifiedVFX.SwanLake.Black;
                CustomParticles.SwanFeatherDrift(player.Center + Main.rand.NextVector2Circular(22f, 22f), featherColor, 0.28f);
            }
            
            // Pulsing rainbow light
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.1f + 0.9f;
            Vector3 lightColor = UnifiedVFX.SwanLake.GetRainbow(Main.GameUpdateCount * 0.01f).ToVector3();
            Lighting.AddLight(player.Center, lightColor * pulse * 0.5f);
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            // Override to our custom bullet
            type = ModContent.ProjectileType<SwansLamentBullet>();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Fire 8-12 bullets in DEVASTATING shotgun spread!
            int bulletCount = Main.rand.Next(10, 15); // Increased bullet count!
            float spreadAngle = MathHelper.ToRadians(28f);
            
            for (int i = 0; i < bulletCount; i++)
            {
                float angleOffset = Main.rand.NextFloat(-spreadAngle, spreadAngle);
                float speedVariation = Main.rand.NextFloat(0.9f, 1.15f);
                Vector2 bulletVel = velocity.RotatedBy(angleOffset) * speedVariation;
                
                int bulletType = i % 2;
                Projectile.NewProjectile(source, position, bulletVel, type, damage, knockback, player.whoAmI, bulletType);
            }
            
            // === UnifiedVFX SWAN LAKE DEVASTATING MUZZLE FLASH! ===
            Vector2 muzzlePos = position + velocity.SafeNormalize(Vector2.Zero) * 50f;
            Vector2 direction = velocity.SafeNormalize(Vector2.Zero);
            UnifiedVFX.SwanLake.Impact(muzzlePos, 1.4f);
            
            // HUGE spark effects!
            ThemedParticles.SwanLakeSparks(muzzlePos, direction, 28, 15f);
            ThemedParticles.SwanLakeSparkles(muzzlePos, 22, 52f);
            ThemedParticles.SwanLakeRainbowExplosion(muzzlePos, 1.1f);
            
            // Stacked halo rings (50% reduced)!
            for (int ring = 0; ring < 4; ring++)
            {
                float hue = (Main.GameUpdateCount * 0.02f + ring * 0.25f) % 1f;
                Color ringColor = Main.hslToRgb(hue, 1f, 0.8f);
                CustomParticles.HaloRing(muzzlePos, ringColor, 0.35f + ring * 0.1f, 12 + ring * 4);
            }
            CustomParticles.HaloRing(muzzlePos, Color.White, 0.5f, 20);
            CustomParticles.HaloRing(muzzlePos, Color.Black, 0.4f, 18);
            
            // Rainbow sparkle flares!
            ThemedParticles.SwanLakeSparkles(muzzlePos, 25, 40f);
            
            // HUGE Music note muzzle flash!
            ThemedParticles.SwanLakeMusicNotes(muzzlePos, 10, 45f);
            ThemedParticles.SwanLakeAccidentals(muzzlePos, 5, 35f);
            
            // MASSIVE rainbow flare burst!
            for (int i = 0; i < 14; i++)
            {
                float hue = i / 14f;
                Color flareColor = Main.hslToRgb(hue, 1f, 0.8f);
                CustomParticles.GenericFlare(muzzlePos + Main.rand.NextVector2Circular(18f, 18f), flareColor, 0.8f, 28);
            }
            
            // HEAVY recoil dust explosion!
            for (int i = 0; i < 24; i++)
            {
                Color col = i % 2 == 0 ? Color.White : new Color(20, 20, 30);
                int dustType = i % 2 == 0 ? DustID.WhiteTorch : DustID.Shadowflame;
                Vector2 vel = -direction * Main.rand.NextFloat(4f, 10f) + Main.rand.NextVector2Circular(5f, 5f);
                Dust d = Dust.NewDustPerfect(muzzlePos, dustType, vel, i % 2 == 0 ? 0 : 100, col, 2.0f);
                d.noGravity = true;
                d.fadeIn = 1.4f;
            }
            
            // Forward spark burst!
            for (int i = 0; i < 20; i++)
            {
                Color col = i % 2 == 0 ? Color.White : new Color(20, 20, 30);
                int dustType = i % 2 == 0 ? DustID.WhiteTorch : DustID.Shadowflame;
                Vector2 vel = direction.RotatedBy(Main.rand.NextFloat(-0.4f, 0.4f)) * Main.rand.NextFloat(8f, 16f);
                Dust d = Dust.NewDustPerfect(muzzlePos, dustType, vel, i % 2 == 0 ? 0 : 100, col, 1.8f);
                d.noGravity = true;
            }
            
            // HUGE rainbow dust burst!
            for (int i = 0; i < 16; i++)
            {
                float hue = i / 16f;
                Color rainbow = Main.hslToRgb(hue, 1f, 0.7f);
                Vector2 vel = direction.RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * Main.rand.NextFloat(6f, 14f);
                Dust r = Dust.NewDustPerfect(muzzlePos, DustID.RainbowTorch, vel, 0, rainbow, 2.0f);
                r.noGravity = true;
                r.fadeIn = 1.4f;
            }
            
            // MASSIVE light explosion!
            Lighting.AddLight(muzzlePos, 2.2f, 2.2f, 2.8f);
            
            return false;
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.04f) * 0.1f + 1f;
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Dark ominous glow
            spriteBatch.Draw(texture, position, null, Color.Black * 0.4f, rotation, origin, scale * 0.9f * pulse * 1.3f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, Color.White * 0.25f, rotation, origin, scale * 0.9f * pulse * 1.15f, SpriteEffects.None, 0f);
            
            // Rainbow shimmer
            float hue = (float)Main.GameUpdateCount * 0.005f % 1f;
            Color rainbow = Main.hslToRgb(hue, 0.5f, 0.7f);
            spriteBatch.Draw(texture, position, null, rainbow * 0.15f, rotation, origin, scale * 0.9f * pulse * 1.08f, SpriteEffects.None, 0f);
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            spriteBatch.Draw(texture, position, null, lightColor, rotation, origin, scale * 0.9f, SpriteEffects.None, 0f);
            
            Lighting.AddLight(Item.Center, 0.35f, 0.35f, 0.4f);
            
            return false;
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            spriteBatch.Draw(texture, position, frame, drawColor, 0f, origin, scale * 0.9f, SpriteEffects.None, 0f);
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Unleashes a devastating spread of black and white bullets"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Creates halos of destruction and rainbow lightning on hit"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Each shot is a verse in the swan's final song'") 
            { 
                OverrideColor = new Color(220, 225, 235) 
            });
        }
    }

    public class SwansLamentBullet : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SwanFeather1";

        private bool isBlack => Projectile.ai[0] == 0;

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.extraUpdates = 2;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // === MASSIVE Black and white FLAMING bullet trail! ===
            Color trailColor = isBlack ? new Color(20, 20, 30) : Color.White;
            Color oppositeColor = isBlack ? Color.White : new Color(20, 20, 30);
            
            // HEAVY main trail!
            for (int i = 0; i < 2; i++)
            {
                int dustType = isBlack ? DustID.Shadowflame : DustID.WhiteTorch;
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(3f, 3f), dustType, 
                    -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1.5f, 1.5f),
                    isBlack ? 100 : 0, trailColor, 1.6f);
                d.noGravity = true;
                d.fadeIn = 1.3f;
            }
            
            // Contrasting sparkles!
            if (Main.rand.NextBool(2))
            {
                int oppDustType = isBlack ? DustID.WhiteTorch : DustID.Shadowflame;
                Dust opp = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4f, 4f), oppDustType,
                    -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f),
                    isBlack ? 0 : 100, oppositeColor, 1.2f);
                opp.noGravity = true;
            }
            
            // Rainbow shimmer!
            if (Main.rand.NextBool(3))
            {
                float hue = Main.rand.NextFloat();
                Color rainbow = Main.hslToRgb(hue, 1f, 0.7f);
                Dust r = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowTorch,
                    -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(0.8f, 0.8f), 0, rainbow, 1.3f);
                r.noGravity = true;
            }
            
            // Occasional flare
            if (Main.rand.NextBool(5))
            {
                CustomParticles.GenericFlare(Projectile.Center, trailColor, 0.3f, 12);
            }
            
            // Ambient fractal gem sparkle
            if (Main.rand.NextBool(8))
            {
                ThemedParticles.SwanLakeFractalTrail(Projectile.Center, 0.35f);
            }
            
            // ☁EMUSICAL NOTATION - Swan Lake graceful melody
            if (Main.rand.NextBool(6))
            {
                float hue = (Main.GameUpdateCount * 0.01f + Main.rand.NextFloat()) % 1f;
                Color noteColor = Main.hslToRgb(hue, 0.8f, 0.9f);
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -1f);
                ThemedParticles.MusicNote(Projectile.Center, noteVel, noteColor, 0.35f, 35);
            }

            // BRIGHT pulsing light!
            float intensity = isBlack ? 0.35f : 0.7f;
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.2f) * 0.15f + 1f;
            Lighting.AddLight(Projectile.Center, intensity * pulse, intensity * pulse, (intensity + 0.1f) * pulse);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply debuff
            target.AddBuff(ModContent.BuffType<FlameOfTheSwan>(), 300); // 5 seconds

            // Create halo of mass destruction
            CreateDestructionHalo(target.Center);
            
            // === EXTRA CUSTOM FLARES ON HIT! ===
            // Rainbow flare burst!
            for (int i = 0; i < 6; i++)
            {
                float hue = i / 6f;
                Color flareColor = Main.hslToRgb(hue, 1f, 0.8f);
                CustomParticles.GenericFlare(target.Center + Main.rand.NextVector2Circular(15f, 15f), flareColor, 0.45f, 16);
            }
            
            // Pearlescent flare
            CustomParticles.SwanLakeFlare(target.Center, 0.4f);
            
            // ☁EMUSICAL IMPACT - Swan's graceful chord
            ThemedParticles.MusicNoteBurst(target.Center, Color.White, 5, 3.5f);
            
            // Fractal gem burst on hit!
            ThemedParticles.SwanLakeFractalGemBurst(target.Center, isBlack ? Color.Black : Color.White, 0.6f, 5, false);
        }

        private void CreateDestructionHalo(Vector2 position)
        {
            // === DEVASTATING HALO OF MASS DESTRUCTION! ===
            
            // MASSIVE Monochrome impact with rainbow explosion!
            ThemedParticles.SwanLakeImpact(position, 1.26f);
            ThemedParticles.SwanLakeRainbowExplosion(position, 1.08f);
            ThemedParticles.SwanLakeMusicalImpact(position, 0.81f, true);
            
            // HUGE Music notes on hit!
            ThemedParticles.SwanLakeMusicNotes(position, 8, 40f);
            ThemedParticles.SwanLakeAccidentals(position, 4, 30f);
            
            // Stacked halo rings (50% reduced)!
            for (int ring = 0; ring < 3; ring++)
            {
                float hue = (Main.GameUpdateCount * 0.02f + ring * 0.33f) % 1f;
                Color ringColor = Main.hslToRgb(hue, 1f, 0.75f);
                CustomParticles.HaloRing(position, ringColor, 0.25f + ring * 0.075f, 10 + ring * 3);
            }
            CustomParticles.HaloRing(position, Color.White, 0.35f, 15);
            CustomParticles.HaloRing(position, Color.Black, 0.25f, 12);
            
            // Rainbow sparkle flares!
            ThemedParticles.SwanLakeSparkles(position, 18, 30f);
            
            // Rainbow flare burst!
            for (int i = 0; i < 8; i++)
            {
                float hue = i / 8f;
                Color flareColor = Main.hslToRgb(hue, 1f, 0.75f);
                CustomParticles.GenericFlare(position + Main.rand.NextVector2Circular(12f, 12f), flareColor, 0.55f, 20);
            }
            
            // MASSIVE Black and white lightning effect - longer and brighter!
            for (int i = 0; i < 8; i++)
            {
                Vector2 lightningDir = Main.rand.NextVector2Unit();
                Color lightningCol = i % 2 == 0 ? Color.White : new Color(20, 20, 30);
                int dustType = i % 2 == 0 ? DustID.WhiteTorch : DustID.Shadowflame;
                
                for (int j = 0; j < 10; j++)
                {
                    Vector2 lightningPos = position + lightningDir * (j * 10f) + Main.rand.NextVector2Circular(5f, 5f);
                    Dust d = Dust.NewDustPerfect(lightningPos, dustType, lightningDir * 1.5f + Main.rand.NextVector2Circular(1f, 1f), 
                        i % 2 == 0 ? 0 : 100, lightningCol, 1.5f);
                    d.noGravity = true;
                    d.fadeIn = 1.2f;
                }
            }
            
            // Radial spark explosion!
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                float hue = i / 16f;
                Color sparkColor = Main.hslToRgb(hue, 1f, 0.7f);
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(5f, 10f);
                Dust spark = Dust.NewDustPerfect(position, DustID.RainbowTorch, vel, 0, sparkColor, 1.6f);
                spark.noGravity = true;
                spark.fadeIn = 1.3f;
            }
            
            // BRIGHT light burst!
            Lighting.AddLight(position, 1.4f, 1.4f, 1.8f);

            // Sound - more frequent
            if (Main.rand.NextBool(2))
            {
                SoundEngine.PlaySound(SoundID.Item93 with { Volume = 0.6f, Pitch = 0.3f }, position);
            }
        }

        public override void OnKill(int timeLeft)
        {
            // EXPLOSIVE death burst!
            for (int i = 0; i < 10; i++)
            {
                Color col = i % 2 == 0 ? Color.White : new Color(20, 20, 30);
                int dustType = i % 2 == 0 ? DustID.WhiteTorch : DustID.Shadowflame;
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, dustType, vel, i % 2 == 0 ? 0 : 100, col, 1.4f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }
            
            // Rainbow death sparks!
            for (int i = 0; i < 5; i++)
            {
                float hue = i / 5f;
                Color rainbow = Main.hslToRgb(hue, 1f, 0.7f);
                Dust r = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowTorch,
                    Main.rand.NextVector2Circular(3f, 3f), 0, rainbow, 1.2f);
                r.noGravity = true;
            }
            
            // Small halo
            CustomParticles.HaloRing(Projectile.Center, isBlack ? Color.Black : Color.White, 0.25f, 10);
            
            Lighting.AddLight(Projectile.Center, 0.6f, 0.6f, 0.8f);
        }
        
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // === EXPLOSIVE GROUND/WALL HIT EFFECT! ===
            CreateDestructionHalo(Projectile.Center);
            
            // Extra custom flares on tile hit!
            for (int i = 0; i < 6; i++)
            {
                float hue = i / 6f;
                Color flareColor = Main.hslToRgb(hue, 1f, 0.8f);
                CustomParticles.GenericFlare(Projectile.Center + Main.rand.NextVector2Circular(12f, 12f), flareColor, 0.4f, 16);
            }
            
            // Swan Lake flare
            CustomParticles.SwanLakeFlare(Projectile.Center, 0.35f);
            
            // Sparkles burst
            ThemedParticles.SwanLakeSparkles(Projectile.Center, 12, 25f);
            
            // Music notes on impact!
            ThemedParticles.SwanLakeMusicNotes(Projectile.Center, 5, 28f);
            ThemedParticles.SwanLakeAccidentals(Projectile.Center, 2, 20f);
            
            // Sound effect
            SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.4f, Pitch = 0.3f }, Projectile.Center);
            
            return true; // Destroy projectile
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.2f) * 0.15f + 1f;
            
            Color mainColor = isBlack ? new Color(25, 25, 30) : new Color(255, 255, 255);
            Color glowColor = isBlack ? new Color(40, 40, 50) * 0.9f : Color.White * 0.95f;
            Color oppositeGlow = isBlack ? Color.White * 0.4f : new Color(30, 30, 40) * 0.4f;
            
            // Rainbow outer glow - cycling!
            float hue = (Main.GameUpdateCount * 0.03f) % 1f;
            Color rainbow = Main.hslToRgb(hue, 1f, 0.7f);
            Main.EntitySpriteDraw(texture, drawPos, null, rainbow * 0.45f, Projectile.rotation, origin, 2.4f * pulse, SpriteEffects.None, 0);
            
            // Main glow layers
            Main.EntitySpriteDraw(texture, drawPos, null, glowColor * 0.6f, Projectile.rotation, origin, 2.0f * pulse, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(texture, drawPos, null, oppositeGlow, Projectile.rotation, origin, 1.6f * pulse, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(texture, drawPos, null, mainColor, Projectile.rotation, origin, 1.2f * pulse, SpriteEffects.None, 0);

            return false;
        }
    }
}
