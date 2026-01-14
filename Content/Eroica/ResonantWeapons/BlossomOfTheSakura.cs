using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using MagnumOpus.Content.Eroica.ResonanceEnergies;
using MagnumOpus.Content.Eroica.Projectiles;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.Eroica.ResonantWeapons
{
    /// <summary>
    /// Blossom of the Sakura - Assault rifle with explosive ammunition.
    /// Rainbow rarity, higher tier than Moonlight weapons.
    /// </summary>
    public class BlossomOfTheSakura : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 75; // Balanced: ~1125 DPS (75 × 60/4)
            Item.DamageType = DamageClass.Ranged;
            Item.width = 64;
            Item.height = 28;
            Item.useTime = 4; // Very fast fire rate
            Item.useAnimation = 4;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 3f;
            Item.value = Item.sellPrice(gold: 38);
            Item.rare = ModContent.RarityType<EroicaRainbowRarity>();
            Item.UseSound = SoundID.Item11;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<BlossomOfTheSakuraBulletProjectile>();
            Item.shootSpeed = 18f;
            Item.useAmmo = AmmoID.Bullet;
            Item.noMelee = true;
            Item.maxStack = 1;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-2f, 0f);
        }

        public override void HoldItem(Player player)
        {
            // === UnifiedVFX EROICA AURA ===
            UnifiedVFX.Eroica.Aura(player.Center, 30f, 0.25f);
            
            // === AMBIENT FRACTAL FLARES - Gun barrel geometric glow ===
            if (Main.rand.NextBool(7))
            {
                Vector2 gunOffset = new Vector2(35f * player.direction, -5f);
                for (int i = 0; i < 4; i++)
                {
                    float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                    float radius = Main.rand.NextFloat(15f, 32f);
                    Vector2 flarePos = player.Center + gunOffset + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                    float progress = (float)i / 4f;
                    Color fractalColor = Color.Lerp(UnifiedVFX.Eroica.Sakura, UnifiedVFX.Eroica.Crimson, progress);
                    CustomParticles.GenericFlare(flarePos, fractalColor, 0.28f, 15);
                }
            }
            
            // Sakura petal particles while holding
            if (Main.rand.NextBool(5))
                ThemedParticles.SakuraPetals(player.Center, 1, 30f);
            
            // Custom particle sakura glow with prismatic accents
            if (Main.rand.NextBool(6))
            {
                CustomParticles.GenericFlare(player.Center + Main.rand.NextVector2Circular(18f, 18f), UnifiedVFX.Eroica.Sakura, 0.22f, 14);
                CustomParticles.PrismaticSparkle(player.Center + Main.rand.NextVector2Circular(25f, 25f), UnifiedVFX.Eroica.Sakura, 0.2f);
            }
            
            // Subtle heroic glow with pulse and gradient
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.07f) * 0.08f + 0.92f;
            Color lightColor = Color.Lerp(UnifiedVFX.Eroica.Sakura, UnifiedVFX.Eroica.Gold, pulse * 0.5f);
            Lighting.AddLight(player.Center, lightColor.ToVector3() * 0.4f * pulse);
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            // Draw glowing backlight effect when dropped in world
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            // Calculate pulse - gentle like sakura petals falling
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.045f) * 0.08f + 1f;
            
            // Begin additive blending for glow
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Outer crimson aura - blood of heroes
            spriteBatch.Draw(texture, position, null, new Color(180, 50, 50) * 0.4f, rotation, origin, scale * pulse * 1.35f, SpriteEffects.None, 0f);
            
            // Middle pink/sakura glow - cherry blossom essence
            spriteBatch.Draw(texture, position, null, new Color(255, 180, 180) * 0.3f, rotation, origin, scale * pulse * 1.18f, SpriteEffects.None, 0f);
            
            // Inner golden-white glow - heroic valor
            spriteBatch.Draw(texture, position, null, new Color(255, 230, 200) * 0.22f, rotation, origin, scale * pulse * 1.06f, SpriteEffects.None, 0f);
            
            // Return to normal blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Add lighting
            Lighting.AddLight(Item.Center, 0.6f, 0.35f, 0.3f);
            
            return true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Always use our custom projectile (ignore ammo type)
            type = ModContent.ProjectileType<BlossomOfTheSakuraBulletProjectile>();

            // Add slight random spread
            Vector2 perturbedVelocity = velocity.RotatedByRandom(MathHelper.ToRadians(3));

            Projectile.NewProjectile(source, position, perturbedVelocity, type, damage, knockback, player.whoAmI);

            // === UnifiedVFX EROICA MUZZLE FLASH ===
            Vector2 muzzlePos = position + velocity.SafeNormalize(Vector2.Zero) * 25f;
            UnifiedVFX.Eroica.SwingAura(muzzlePos, velocity.SafeNormalize(Vector2.UnitX), 0.6f);
            
            // === FRACTAL MUZZLE FLASH - geometric burst pattern with gradient ===
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi * i / 5f + velocity.ToRotation();
                Vector2 flareOffset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 14f;
                float progress = (float)i / 5f;
                Color fractalColor = Color.Lerp(UnifiedVFX.Eroica.Sakura, UnifiedVFX.Eroica.Gold, progress);
                CustomParticles.GenericFlare(muzzlePos + flareOffset, fractalColor, 0.4f, 14);
            }
            CustomParticles.HaloRing(muzzlePos, UnifiedVFX.Eroica.Sakura * 0.8f, 0.28f, 12);
            
            // Sakura petals burst
            if (Main.rand.NextBool(3))
                ThemedParticles.SakuraPetals(muzzlePos, 2, 20f);
            
            // Music notes more frequently
            if (Main.rand.NextBool(4))
                ThemedParticles.EroicaMusicNotes(position, 2, 18f);

            // Bright muzzle light
            Lighting.AddLight(muzzlePos, UnifiedVFX.Eroica.Sakura.ToVector3() * 0.8f);

            return false;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            // Adjust spawn position to gun barrel
            position += velocity.SafeNormalize(Vector2.UnitX * player.direction) * 40f;
        }

        // Recipe removed - drops from Eroica, God of Valor
        // public override void AddRecipes()
        // {
        //     CreateRecipe()
        //         .AddIngredient(ModContent.ItemType<ResonantCoreOfEroica>(), 22)
        //         .AddIngredient(ModContent.ItemType<EroicasResonantEnergy>(), 18)
        //         .AddIngredient(ItemID.LunarBar, 14)
        //         .AddTile(TileID.LunarCraftingStation)
        //         .Register();
        // }
    }
}
