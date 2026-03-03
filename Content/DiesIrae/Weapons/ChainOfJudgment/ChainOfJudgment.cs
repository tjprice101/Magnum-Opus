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

namespace MagnumOpus.Content.DiesIrae.Weapons.ChainOfJudgment
{
    public class ChainOfJudgment : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 50;
            Item.height = 50;
            Item.damage = 2400;
            Item.DamageType = DamageClass.Melee;
            Item.useTime = 22;
            Item.useAnimation = 22;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 6f;
            Item.value = Item.sellPrice(platinum: 2, gold: 50);
            Item.rare = ModContent.RarityType<global::MagnumOpus.Common.DiesIraeRarity>();
            Item.UseSound = SoundID.Item153;
            Item.autoReuse = true;
            Item.useTurn = false;
            Item.scale = 1.3f;
            Item.crit = 15;
            Item.shoot = ModContent.ProjectileType<Projectiles.JudgmentChainProjectile>();
            Item.shootSpeed = 16f;
            Item.noMelee = true;
            Item.noUseGraphic = true;
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
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Throws a blazing spectral chain that spins and ricochets"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Bounces up to 4 times between enemies, exploding on each hit"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Returns to you after bouncing, ignites struck enemies"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The chains that bind the damned to their fate'")
            {
                OverrideColor = new Color(200, 50, 30)
            });
        }
    }
}