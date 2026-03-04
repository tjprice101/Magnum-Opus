using MagnumOpus.Common;
using MagnumOpus.Content.DiesIrae.Weapons.EclipseOfWrath.Projectiles;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using MagnumOpus.Content.DiesIrae.HarmonicCores;
using MagnumOpus.Content.DiesIrae.ResonanceEnergies;
using MagnumOpus.Content.Fate.CraftingStations;

namespace MagnumOpus.Content.DiesIrae.Weapons.EclipseOfWrath
{
    public class EclipseOfWrath : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 44;
            Item.damage = 1750;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 25;
            Item.useTime = 35;
            Item.useAnimation = 35;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 8f;
            Item.value = Item.sellPrice(platinum: 2, gold: 50);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
            Item.UseSound = SoundID.Item73 with { Pitch = -0.2f };
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.shoot = ModContent.ProjectileType<EclipseOrbProjectile>();
            Item.shootSpeed = 12f;
            Item.crit = 22;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
            .AddIngredient(ModContent.ItemType<ResonantCoreOfDiesIrae>(), 20)
            .AddIngredient(ModContent.ItemType<DiesIraeResonantEnergy>(), 15)
            .AddIngredient(ModContent.ItemType<HarmonicCoreOfDiesIrae>(), 2)
            .AddIngredient(ItemID.LunarBar, 15)
            .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
            .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Throws a slow-moving eclipse orb with a dark core and fire corona"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Orb tracks your cursor and splits into 6 wrath shards on impact"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Critical hits cause Corona Flare — 12 shards + fire nova"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Destroyed orbs leave Eclipse Fields that increase damage taken by 15%"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The sun that rises for judgment is not the sun that brings dawn.'")
            {
                OverrideColor = new Color(200, 50, 30) // Dies Irae blood red
            });
        }
    }
}