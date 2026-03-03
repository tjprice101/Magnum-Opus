using MagnumOpus.Common;
using MagnumOpus.Content.OdeToJoy.Weapons.TheGardenersFury.Projectiles;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using MagnumOpus.Content.Fate.CraftingStations;
using MagnumOpus.Content.OdeToJoy.HarmonicCores;
using MagnumOpus.Content.OdeToJoy.ResonanceEnergies;

namespace MagnumOpus.Content.OdeToJoy.Weapons.TheGardenersFury
{
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
    }
}