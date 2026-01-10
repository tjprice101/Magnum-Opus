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
            // Sakura petal particles while holding
            if (Main.rand.NextBool(4))
            {
                Vector2 offset = Main.rand.NextVector2Circular(25f, 25f);
                ThemedParticles.EroicaAura(player.Center + offset, 15f);
            }
            
            // Custom particle sakura glow
            if (Main.rand.NextBool(6))
            {
                CustomParticles.EroicaFlare(player.Center + Main.rand.NextVector2Circular(18f, 18f), 0.2f);
            }
            
            // Subtle heroic glow
            Lighting.AddLight(player.Center, 0.4f, 0.25f, 0.15f);
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

            // Muzzle flash particles
            for (int i = 0; i < 5; i++)
            {
                Dust flash = Dust.NewDustDirect(position, 10, 10,
                    DustID.RedTorch, velocity.X * 0.3f, velocity.Y * 0.3f, 100, default, 1.2f);
                flash.noGravity = true;
            }
            
            // Occasional music notes
            if (Main.rand.NextBool(6))
            {
                ThemedParticles.EroicaMusicNotes(position, 2, 15f);
            }

            if (Main.rand.NextBool(2))
            {
                Dust smoke = Dust.NewDustDirect(position, 10, 10,
                    DustID.Smoke, velocity.X * 0.2f, velocity.Y * 0.2f, 100, Color.Black, 0.8f);
                smoke.noGravity = true;
            }

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
