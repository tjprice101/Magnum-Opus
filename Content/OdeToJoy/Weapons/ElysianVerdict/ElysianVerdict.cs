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
using MagnumOpus.Content.OdeToJoy.Weapons.ElysianVerdict.Projectiles;
using MagnumOpus.Content.OdeToJoy.Weapons.ElysianVerdict.Particles;
using MagnumOpus.Content.OdeToJoy.Weapons.ElysianVerdict.Utilities;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ElysianVerdict
{
    /// <summary>
    /// Elysian Verdict — Ode to Joy magic staff.
    /// Deploys a cursor-tracking golden-green orb that fires homing vine missiles,
    /// then detonates in a massive jubilant explosion of music notes and leaves.
    /// Using the item while an orb exists detonates the old orb and fires a new one.
    /// Post-endgame Ode to Joy tier magic weapon.
    /// </summary>
    public class ElysianVerdict : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 42;
            Item.height = 42;
            Item.damage = 3200;
            Item.DamageType = DamageClass.Magic;
            Item.useTime = 35;
            Item.useAnimation = 35;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(platinum: 3, gold: 50);
            Item.rare = ModContent.RarityType<global::MagnumOpus.Common.OdeToJoyRarity>();
            Item.UseSound = SoundID.Item66;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.mana = 30;
            Item.crit = 18;
            Item.shoot = ModContent.ProjectileType<ElysianOrbProjectile>();
            Item.shootSpeed = 12f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Kill any existing ElysianOrb owned by this player (triggers detonation via OnKill)
            int orbType = ModContent.ProjectileType<ElysianOrbProjectile>();
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj.active && proj.owner == player.whoAmI && proj.type == orbType)
                {
                    // Signal the orb to detonate
                    proj.ai[1] = 1f;
                    proj.netUpdate = true;
                }
            }

            // Fire a new orb toward the cursor
            Vector2 dir = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
            Projectile.NewProjectile(source, player.Center + dir * 20f, dir * Item.shootSpeed,
                orbType, damage, knockback, player.whoAmI);

            return false; // We manually spawned the projectile
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
            spriteBatch.Draw(texture, position, null, ElysianUtils.Additive(ElysianUtils.GoldenVerdict, 0.35f * flicker),
                rotation, origin, scale * pulse * 1.3f, SpriteEffects.None, 0f);
            // Green inner glow
            spriteBatch.Draw(texture, position, null, ElysianUtils.Additive(ElysianUtils.VineGreen, 0.3f * flicker),
                rotation, origin, scale * pulse * 1.15f, SpriteEffects.None, 0f);

            float shimmer = (float)Math.Sin(time * 3f) * 0.5f + 0.5f;
            spriteBatch.Draw(texture, position, null, ElysianUtils.Additive(ElysianUtils.PureRadiance, 0.2f * shimmer),
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

            Color glowColor = Color.Lerp(ElysianUtils.VineGreen, ElysianUtils.GoldenVerdict,
                (float)Math.Sin(time * 0.8f) * 0.5f + 0.5f);
            spriteBatch.Draw(texture, position, frame, ElysianUtils.Additive(glowColor, 0.3f * flicker),
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
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Fires a golden-green orb that tracks your cursor"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "The orb fires homing vine missiles at nearby enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Using the staff again detonates the orb in a massive jubilant explosion"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Vine missiles apply Poisoned and home toward enemies"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Where the verdict of joy is passed, even thorns bloom into golden song'")
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
