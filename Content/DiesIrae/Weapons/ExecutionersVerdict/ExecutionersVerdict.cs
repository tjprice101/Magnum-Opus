using MagnumOpus.Common;
using MagnumOpus.Content.DiesIrae.Weapons.ExecutionersVerdict.Projectiles;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using MagnumOpus.Content.DiesIrae.HarmonicCores;
using MagnumOpus.Content.DiesIrae.ResonanceEnergies;
using MagnumOpus.Content.Fate.CraftingStations;

namespace MagnumOpus.Content.DiesIrae.Weapons.ExecutionersVerdict
{
    public class ExecutionersVerdict : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 80;
            Item.height = 80;
            Item.damage = 4200;
            Item.DamageType = DamageClass.Melee;
            Item.useTime = 32;
            Item.useAnimation = 32;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 12f;
            Item.value = Item.sellPrice(platinum: 2, gold: 50);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
            Item.UseSound = SoundID.Item71 with { Pitch = -0.3f };
            Item.autoReuse = true;
            Item.useTurn = false;
            Item.scale = 1.8f;
            Item.crit = 25;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.shoot = ModContent.ProjectileType<ExecutionersVerdictSwing>();
            Item.shootSpeed = 1f;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
            .AddIngredient(ModContent.ItemType<ResonantCoreOfDiesIrae>(), 25)
            .AddIngredient(ModContent.ItemType<DiesIraeResonantEnergy>(), 20)
            .AddIngredient(ModContent.ItemType<HarmonicCoreOfDiesIrae>(), 3)
            .AddIngredient(ItemID.LunarBar, 20)
            .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
            .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Each swing launches 3 judgment bolts that track enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Bolts explode into spectral sword strikes on impact"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Executes non-boss enemies below 15% health instantly"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Deals 50% more damage to enemies below 30% health"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "3-phase combo builds to a devastating guillotine drop"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The final sentence is always death'")
            {
                OverrideColor = new Color(200, 50, 30)
            });
        }
    }
}