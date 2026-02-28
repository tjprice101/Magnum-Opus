using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.OdeToJoy.ResonanceEnergies;
using MagnumOpus.Content.OdeToJoy.HarmonicCores;
using MagnumOpus.Content.Fate.CraftingStations;
using MagnumOpus.Content.OdeToJoy.Weapons.AnthemOfGlory.Projectiles;
using MagnumOpus.Content.OdeToJoy.Weapons.AnthemOfGlory.Particles;
using MagnumOpus.Content.OdeToJoy.Weapons.AnthemOfGlory.Utilities;

namespace MagnumOpus.Content.OdeToJoy.Weapons.AnthemOfGlory
{
    /// <summary>
    /// Anthem of Glory — Ode to Joy rapid magic weapon.
    /// Fires 3 chain-lightning shards per cast in a spread.
    /// Every 3rd shot also fires a massive golden glory beam.
    /// Post-endgame Ode to Joy tier magic weapon.
    /// </summary>
    public class AnthemOfGlory : ModItem
    {
        private int shotCounter = 0;

        public override void SetDefaults()
        {
            Item.width = 42;
            Item.height = 42;
            Item.damage = 2800;
            Item.DamageType = DamageClass.Magic;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 3f;
            Item.value = Item.sellPrice(platinum: 3, gold: 50);
            Item.rare = ModContent.RarityType<global::MagnumOpus.Common.OdeToJoyRarity>();
            Item.UseSound = SoundID.Item21;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.mana = 15;
            Item.crit = 12;
            Item.shoot = ModContent.ProjectileType<GloryShardProjectile>();
            Item.shootSpeed = 18f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            shotCounter++;
            Vector2 dir = velocity.SafeNormalize(Vector2.UnitX);

            // Fire 3 shards in a spread: -15, 0, +15 degrees
            float[] spreadAngles = { -MathHelper.ToRadians(15f), 0f, MathHelper.ToRadians(15f) };
            int shardType = ModContent.ProjectileType<GloryShardProjectile>();

            foreach (float angle in spreadAngles)
            {
                Vector2 spreadVel = velocity.RotatedBy(angle);
                Projectile.NewProjectile(source, player.Center + dir * 20f, spreadVel,
                    shardType, damage, knockback, player.whoAmI);
            }

            // Every 3rd shot: fire a glory beam toward the cursor
            if (shotCounter % 3 == 0)
            {
                int beamType = ModContent.ProjectileType<GloryBeamProjectile>();
                Projectile.NewProjectile(source, player.Center + dir * 30f, velocity,
                    beamType, damage, knockback, player.whoAmI);
            }

            return false; // We manually spawned projectiles
        }

        // ── WORLD DROP RENDERING ──

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            float time = Main.GameUpdateCount * 0.06f;
            float pulse = 1f + (float)Math.Sin(time * 2f) * 0.1f;
            float flicker = Main.rand.NextFloat(0.9f, 1f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Golden outer glow
            spriteBatch.Draw(texture, position, null, AnthemUtils.Additive(AnthemUtils.BrilliantAmber, 0.35f * flicker),
                rotation, origin, scale * pulse * 1.3f, SpriteEffects.None, 0f);
            // Rose inner glow
            spriteBatch.Draw(texture, position, null, AnthemUtils.Additive(AnthemUtils.RoseTint, 0.25f * flicker),
                rotation, origin, scale * pulse * 1.15f, SpriteEffects.None, 0f);

            float shimmer = (float)Math.Sin(time * 3f) * 0.5f + 0.5f;
            spriteBatch.Draw(texture, position, null, AnthemUtils.Additive(AnthemUtils.GloryWhite, 0.2f * shimmer),
                rotation, origin, scale * pulse * 1.05f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Lighting.AddLight(Item.Center, 0.5f, 0.45f, 0.12f);
            return true;
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            float time = Main.GameUpdateCount * 0.05f;
            float pulse = 1f + (float)Math.Sin(time * 2.2f) * 0.08f;
            float flicker = Main.rand.NextFloat(0.9f, 1f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

            Color glowColor = Color.Lerp(AnthemUtils.BrilliantAmber, AnthemUtils.RichGold,
                (float)Math.Sin(time * 0.8f) * 0.5f + 0.5f);
            spriteBatch.Draw(texture, position, frame, AnthemUtils.Additive(glowColor, 0.3f * flicker),
                0f, origin, scale * pulse * 1.12f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

            spriteBatch.Draw(texture, position, frame, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
            return false;
        }

        // ── TOOLTIPS ──

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Fires 3 golden shards per cast in a wide spread"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Shards chain lightning to nearby enemies on hit"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Every 3rd cast unleashes a massive glory beam that pierces infinitely"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Chain lightning can arc up to 2 times between enemies"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Let every note ring with triumph — for this is the anthem that crowns the victorious'")
            {
                OverrideColor = new Color(255, 200, 50)
            });
        }

        // ── RECIPE ──

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfOdeToJoy>(), 20)
                .AddIngredient(ModContent.ItemType<OdeToJoyResonantEnergy>(), 15)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfOdeToJoy>(), 2)
                .AddIngredient(ItemID.LunarBar, 15)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }
}
