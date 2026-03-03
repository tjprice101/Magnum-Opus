using MagnumOpus.Common;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using MagnumOpus.Content.DiesIrae.HarmonicCores;
using MagnumOpus.Content.DiesIrae.ResonanceEnergies;
using MagnumOpus.Content.Fate.CraftingStations;

namespace MagnumOpus.Content.DiesIrae.Weapons.GrimoireOfCondemnation
{
    public class GrimoireOfCondemnation : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.damage = 1500;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 12;
            Item.useTime = 15;
            Item.useAnimation = 15;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(platinum: 1, gold: 50);
            Item.rare = ModContent.RarityType<global::MagnumOpus.Common.DiesIraeRarity>();
            Item.UseSound = SoundID.Item103;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<Projectiles.BlazingShardProjectile>();
            Item.shootSpeed = 12f;
            Item.crit = 15;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
            .AddIngredient(ModContent.ItemType<Content.DiesIrae.ResonanceEnergies.ResonantCoreOfDiesIrae>(), 20)
            .AddIngredient(ModContent.ItemType<Content.DiesIrae.ResonanceEnergies.DiesIraeResonantEnergy>(), 15)
            .AddIngredient(ModContent.ItemType<Content.DiesIrae.HarmonicCores.HarmonicCoreOfDiesIrae>(), 2)
            .AddIngredient(ItemID.LunarBar, 15)
            .AddTile(ModContent.TileType<Content.Fate.CraftingStations.FatesCosmicAnvilTile>())
            .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Fires 3 spiraling cursed shards that chain lightning between them"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Shards embed in enemies, then detonate after a brief delay"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Each page condemns another soul to the pyre'")
            {
                OverrideColor = new Color(200, 50, 30)
            });
        }
    }
}