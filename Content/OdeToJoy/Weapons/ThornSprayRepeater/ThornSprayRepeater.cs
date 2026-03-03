using MagnumOpus.Common;
using MagnumOpus.Content.OdeToJoy.Weapons.ThornSprayRepeater.Projectiles;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using MagnumOpus.Content.Fate.CraftingStations;
using MagnumOpus.Content.OdeToJoy.HarmonicCores;
using MagnumOpus.Content.OdeToJoy.ResonanceEnergies;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ThornSprayRepeater
{
    public class ThornSprayRepeater : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 52;
            Item.height = 24;
            Item.damage = 2400;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 6;
            Item.useAnimation = 6;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 2f;
            Item.value = Item.sellPrice(platinum: 3, gold: 50);
            Item.rare = ModContent.RarityType<global::MagnumOpus.Common.OdeToJoyRarity>();
            Item.UseSound = SoundID.Item5;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.crit = 12;
            Item.shoot = ProjectileID.WoodenArrowFriendly;
            Item.shootSpeed = 16f;
            Item.useAmmo = AmmoID.Arrow;
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
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Converts any arrow into rapid-fire thorn bolts that embed in enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Embedded thorns detonate after 1 second in a chain explosion of splinters and poison"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Each additional thorn on the same enemy adds 10% explosion damage, up to 8 thorns"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Explosions scatter homing splinters and inflict Poisoned"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Let every barb be a verse of jubilation — a thousand thorns sing the hymn of triumphant bloom'")
            {
                OverrideColor = new Color(255, 200, 50)
            });
        }
    }
}