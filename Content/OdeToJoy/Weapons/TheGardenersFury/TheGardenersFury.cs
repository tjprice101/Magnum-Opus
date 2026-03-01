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
using MagnumOpus.Content.OdeToJoy.Weapons.TheGardenersFury.Projectiles;
using MagnumOpus.Content.OdeToJoy.Weapons.TheGardenersFury.Particles;
using MagnumOpus.Content.OdeToJoy.Weapons.TheGardenersFury.Utilities;

namespace MagnumOpus.Content.OdeToJoy.Weapons.TheGardenersFury
{
    /// <summary>
    /// The Gardener's Fury — Lightning-fast rapier of Ode to Joy.
    /// Builds combo stacks with each hit (max 10), granting +5% melee attack speed per stack.
    /// At 5+ stacks: spawns homing petal projectiles on hit.
    /// At 10 stacks + crit: Triumphant Celebration — radial burst of jubilant petals, massive bloom, screen shake.
    /// Post-endgame Ode to Joy tier melee weapon.
    /// </summary>
    public class TheGardenersFury : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;
            Item.damage = 3200;
            Item.DamageType = DamageClass.Melee;
            Item.useTime = 8;
            Item.useAnimation = 8;
            Item.useStyle = ItemUseStyleID.Rapier;
            Item.knockBack = 3f;
            Item.value = Item.sellPrice(platinum: 3, gold: 50);
            Item.rare = ModContent.RarityType<global::MagnumOpus.Common.OdeToJoyRarity>();
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.crit = 25;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.shoot = ModContent.ProjectileType<GardenerFuryProjectile>();
            Item.shootSpeed = 5f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Pass current combo stacks to projectile via ai[0]
            var comboPlayer = player.GetModPlayer<ComboStackPlayer>();
            Projectile.NewProjectile(source, player.MountedCenter, velocity,
                ModContent.ProjectileType<GardenerFuryProjectile>(),
                damage, knockback, player.whoAmI, ai0: comboPlayer.ComboStacks);

            return false; // We manually spawned the projectile
        }

        // ── WORLD DROP RENDERING ──

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            // Rapier rhythm: rapid staccato thrust-pulse with directional extension
            float time = Main.GameUpdateCount * 0.06f;
            float thrustCycle = (float)Math.Sin(time * 5f); // Fast rhythmic thrust
            float thrustPulse = 1f + Math.Max(0f, thrustCycle) * 0.18f; // Only extends on positive phase
            float restPulse = 1f + (float)Math.Sin(time * 1.2f) * 0.04f; // Gentle idle breathe
            float pulse = MathHelper.Lerp(restPulse, thrustPulse, 0.7f);
            float colorShift = (float)Math.Sin(time * 4f) * 0.5f + 0.5f; // Rapid green↔gold

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Directional thrust glow — extends along blade axis
            Vector2 thrustDir = (rotation + MathHelper.PiOver4).ToRotationVector2();
            Vector2 thrustOffset = thrustDir * Math.Max(0f, thrustCycle) * 4f;
            Color outerColor = Color.Lerp(GardenersUtils.StemGreen, GardenersUtils.JubilantGold, colorShift);
            spriteBatch.Draw(texture, position + thrustOffset, null, GardenersUtils.Additive(outerColor, 0.4f),
                rotation, origin, new Vector2(scale * pulse * 1.4f, scale * pulse * 1.15f), SpriteEffects.None, 0f);

            // Rose accent — counter-phase for visual depth
            float rosePhase = (float)Math.Sin(time * 5f + 1.5f) * 0.5f + 0.5f;
            spriteBatch.Draw(texture, position, null, GardenersUtils.Additive(GardenersUtils.RoseBlush, 0.3f * rosePhase),
                rotation, origin, scale * (1f + rosePhase * 0.12f) * 1.1f, SpriteEffects.None, 0f);

            // White flash on thrust peak — bright staccato accent
            float flashIntensity = (float)Math.Pow(Math.Max(0f, thrustCycle), 3f);
            spriteBatch.Draw(texture, position + thrustOffset * 0.5f, null, GardenersUtils.Additive(GardenersUtils.SunlightWhite, 0.35f * flashIntensity),
                rotation, origin, scale * (1f + flashIntensity * 0.1f), SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Lighting.AddLight(Item.Center, 0.5f + flashIntensity * 0.3f, 0.45f + flashIntensity * 0.2f, 0.1f);
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

            Color glowColor = Color.Lerp(GardenersUtils.GoldenPetal, GardenersUtils.JubilantGold,
                (float)Math.Sin(time * 0.8f) * 0.5f + 0.5f);
            spriteBatch.Draw(texture, position, frame, GardenersUtils.Additive(glowColor, 0.3f * flicker),
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
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Lightning-fast rapier thrusts build combo stacks on hit (max 10)"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Each stack grants 5% increased melee attack speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "At 5+ stacks, hits release homing petal projectiles"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "At max stacks, a critical strike triggers Triumphant Celebration"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Let every thorn become a blossom — let fury bloom into jubilation'")
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
