using MagnumOpus.Common;
using MagnumOpus.Content.OdeToJoy.Weapons.ElysianVerdict.Projectiles;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using MagnumOpus.Content.Fate.CraftingStations;
using MagnumOpus.Content.OdeToJoy.HarmonicCores;
using MagnumOpus.Content.OdeToJoy.ResonanceEnergies;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ElysianVerdict
{
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
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Fires a golden-green orb that tracks your cursor"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "The orb fires homing vine missiles at nearby enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Using the staff again detonates the orb in a massive jubilant explosion"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Vine missiles apply Poisoned and home toward enemies"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Where the verdict of joy is passed, even thorns bloom into golden song'")
            {
                OverrideColor = new Color(255, 200, 50)
            });
        }
    }
}