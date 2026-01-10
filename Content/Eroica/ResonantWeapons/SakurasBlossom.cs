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
    /// Sakura's Blossom - Melee weapon that creates spectral copies seeking enemies.
    /// Rainbow rarity, higher tier than Moonlight weapons.
    /// </summary>
    public class SakurasBlossom : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 350; // Balanced: ~1050 DPS (350 × 60/20)
            Item.DamageType = DamageClass.Melee;
            Item.width = 70;
            Item.height = 70;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 8f;
            Item.value = Item.sellPrice(gold: 40);
            Item.rare = ModContent.RarityType<EroicaRainbowRarity>();
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.useTurn = false;
            Item.shoot = ModContent.ProjectileType<SakurasBlossomSpectral>();
            Item.shootSpeed = 10f;
            Item.maxStack = 1;
        }

        public override void HoldItem(Player player)
        {
            // Sakura flame aura while holding
            if (Main.rand.NextBool(4))
            {
                Vector2 offset = Main.rand.NextVector2Circular(25f, 25f);
                ThemedParticles.EroicaAura(player.Center + offset, 18f);
            }
            
            // Custom particle sakura spirit glow
            if (Main.rand.NextBool(5))
            {
                CustomParticles.EroicaTrailFlare(player.Center + Main.rand.NextVector2Circular(22f, 22f), player.velocity);
            }
            
            // Heroic scarlet glow
            Lighting.AddLight(player.Center, 0.5f, 0.25f, 0.15f);
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            // Draw glowing backlight effect when dropped in world
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            // Calculate pulse - powerful and blossoming
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.055f) * 0.12f + 1f;
            
            // Begin additive blending for glow
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Outer deep scarlet aura - sakura's spirit
            spriteBatch.Draw(texture, position, null, new Color(180, 40, 50) * 0.45f, rotation, origin, scale * pulse * 1.38f, SpriteEffects.None, 0f);
            
            // Middle crimson/pink glow - cherry blossom
            spriteBatch.Draw(texture, position, null, new Color(255, 100, 120) * 0.35f, rotation, origin, scale * pulse * 1.2f, SpriteEffects.None, 0f);
            
            // Inner golden/white glow - valor's light
            spriteBatch.Draw(texture, position, null, new Color(255, 230, 180) * 0.25f, rotation, origin, scale * pulse * 1.08f, SpriteEffects.None, 0f);
            
            // Return to normal blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Add lighting
            Lighting.AddLight(Item.Center, 0.65f, 0.35f, 0.3f);
            
            return true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Get cursor direction
            Vector2 cursorDirection = velocity;
            cursorDirection.Normalize();

            // Create 3 spectral swords in a 100 degree cone towards cursor
            float spreadAngle = MathHelper.ToRadians(100f); // 100 degree total spread
            float startAngle = -spreadAngle / 2f; // Start at -50 degrees from center

            for (int i = 0; i < 3; i++)
            {
                // Evenly distribute the 3 swords across the 100 degree cone
                float angle = startAngle + (spreadAngle / 2f) * i; // -50°, 0°, +50°
                Vector2 spectralVelocity = cursorDirection.RotatedBy(angle) * 15f;

                Projectile.NewProjectile(source, player.Center, spectralVelocity,
                    ModContent.ProjectileType<SakurasBlossomSpectral>(), damage, knockback, player.whoAmI);
            }
            
            // Musical burst on swing!
            ThemedParticles.EroicaMusicNotes(player.Center + cursorDirection * 30f, 4, 25f);

            return false;
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            // Intense scarlet and black particles
            if (Main.rand.NextBool(2))
            {
                Dust flame = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height,
                    DustID.RedTorch, player.velocity.X * 0.2f, player.velocity.Y * 0.2f, 150, default, 1.8f);
                flame.noGravity = true;
                flame.velocity *= 2f;
            }

            if (Main.rand.NextBool(3))
            {
                Dust smoke = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height,
                    DustID.Smoke, 0f, 0f, 100, Color.Black, 1.3f);
                smoke.noGravity = true;
                smoke.velocity = Main.rand.NextVector2Circular(3f, 3f);
            }
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Create massive scarlet and black explosion on hit
            for (int i = 0; i < 30; i++)
            {
                Dust explosion = Dust.NewDustDirect(target.position, target.width, target.height,
                    DustID.RedTorch, 0f, 0f, 100, default, 2.5f);
                explosion.noGravity = true;
                explosion.velocity = Main.rand.NextVector2Circular(8f, 8f);
            }

            for (int i = 0; i < 20; i++)
            {
                Dust smoke = Dust.NewDustDirect(target.position, target.width, target.height,
                    DustID.Smoke, 0f, 0f, 100, Color.Black, 2.0f);
                smoke.noGravity = true;
                smoke.velocity = Main.rand.NextVector2Circular(6f, 6f);
            }
        }

        // Recipe removed - drops from Eroica, God of Valor
        // public override void AddRecipes()
        // {
        //     CreateRecipe()
        //         .AddIngredient(ModContent.ItemType<ResonantCoreOfEroica>(), 30)
        //         .AddIngredient(ModContent.ItemType<EroicasResonantEnergy>(), 25)
        //         .AddIngredient(ItemID.LunarBar, 18)
        //         .AddTile(TileID.LunarCraftingStation)
        //         .Register();
        // }
    }
}
