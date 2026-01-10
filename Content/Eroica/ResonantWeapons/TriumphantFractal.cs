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
    /// Triumphant Fractal - Magic staff that fires three fractal projectiles with massive explosions.
    /// Rainbow rarity, higher tier than Moonlight weapons.
    /// Features Calamity-inspired visual effects with chromatic aberration and pulsing glow.
    /// </summary>
    public class TriumphantFractal : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
            Item.staff[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = 518; // Buffed: ~1244 DPS (518 × 60/25), 15% increase
            Item.DamageType = DamageClass.Magic;
            Item.width = 56;
            Item.height = 56;
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 8f;
            Item.value = Item.sellPrice(gold: 45);
            Item.rare = ModContent.RarityType<EroicaRainbowRarity>();
            Item.UseSound = SoundID.Item43;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<TriumphantFractalProjectile>();
            Item.shootSpeed = 14f;
            Item.mana = 19; // Reduced from 20 (5% reduction)
            Item.noMelee = true;            Item.maxStack = 1;        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Fire 3 fractals in a tight spread
            int numberOfProjectiles = 3;
            float spreadAngle = MathHelper.ToRadians(15);

            for (int i = 0; i < numberOfProjectiles; i++)
            {
                float angle = spreadAngle * ((float)i / (numberOfProjectiles - 1) - 0.5f);
                Vector2 perturbedVelocity = velocity.RotatedBy(angle);

                Projectile.NewProjectile(source, position, perturbedVelocity, type, (int)(damage * 1.15f), knockback, player.whoAmI);
            }

            // Spawn dramatic red and black particles at cast location
            for (int i = 0; i < 15; i++)
            {
                Dust cast = Dust.NewDustDirect(position, 20, 20,
                    DustID.RedTorch, 0f, 0f, 100, default, 1.5f);
                cast.noGravity = true;
                cast.velocity = Main.rand.NextVector2Circular(3f, 3f);
            }

            for (int i = 0; i < 8; i++)
            {
                Dust smoke = Dust.NewDustDirect(position, 20, 20,
                    DustID.Smoke, 0f, 0f, 100, Color.Black, 1.2f);
                smoke.noGravity = true;
                smoke.velocity = Main.rand.NextVector2Circular(2f, 2f);
            }
            
            // Musical burst on cast!
            ThemedParticles.EroicaMusicNotes(position, 5, 25f);

            return false;
        }

        public override void HoldItem(Player player)
        {
            // Intense ambient particles while holding - crimson flames
            if (Main.rand.NextBool(3))
            {
                Vector2 offset = Main.rand.NextVector2Circular(20f, 20f);
                Dust flame = Dust.NewDustPerfect(player.Center + offset, DustID.CrimsonTorch, 
                    new Vector2(0, -2f) + Main.rand.NextVector2Circular(1f, 1f), 100, default, 1.3f);
                flame.noGravity = true;
            }
            
            // Golden sparkles
            if (Main.rand.NextBool(5))
            {
                ThemedParticles.EroicaSparkles(player.Center + Main.rand.NextVector2Circular(25f, 25f), 1, 5f);
            }
            
            // Lighting aura
            Lighting.AddLight(player.Center, 0.6f, 0.3f, 0.2f);
        }
        
        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            // Draw glowing backlight effect when dropped in world
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            // Calculate pulse - faster for magic staff
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 0.2f + 1f;
            
            // Begin additive blending for glow
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Outer crimson/scarlet glow
            spriteBatch.Draw(texture, position, null, new Color(200, 50, 50) * 0.5f, rotation, origin, scale * pulse * 1.4f, SpriteEffects.None, 0f);
            
            // Middle orange glow
            spriteBatch.Draw(texture, position, null, new Color(255, 120, 60) * 0.4f, rotation, origin, scale * pulse * 1.2f, SpriteEffects.None, 0f);
            
            // Inner golden-white core
            spriteBatch.Draw(texture, position, null, new Color(255, 220, 150) * 0.3f, rotation, origin, scale * pulse * 1.05f, SpriteEffects.None, 0f);
            
            // Return to normal blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Add lighting
            Lighting.AddLight(Item.Center, 1f, 0.5f, 0.3f);
            
            return true; // Draw the normal sprite too
        }

        // Recipe removed - drops from Eroica, God of Valor
        // public override void AddRecipes()
        // {
        //     CreateRecipe()
        //         .AddIngredient(ModContent.ItemType<ResonantCoreOfEroica>(), 28)
        //         .AddIngredient(ModContent.ItemType<EroicasResonantEnergy>(), 22)
        //         .AddIngredient(ItemID.LunarBar, 16)
        //         .AddTile(TileID.LunarCraftingStation)
        //         .Register();
        // }
    }
}
