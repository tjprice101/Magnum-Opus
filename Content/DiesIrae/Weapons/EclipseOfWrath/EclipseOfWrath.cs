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
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Throws a dark eclipse orb that tracks your cursor"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "While airborne, spawns blazing wrath shards that seek enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Explodes on impact with enemies or tiles"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The sun's wrath made manifest'")
            {
                OverrideColor = new Color(200, 50, 30) // Dies Irae blood red
            });
        }
    }
}