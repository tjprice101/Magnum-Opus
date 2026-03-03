using MagnumOpus.Common;
using MagnumOpus.Content.DiesIrae.Weapons.WrathsCleaver.Projectiles;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using MagnumOpus.Content.DiesIrae.HarmonicCores;
using MagnumOpus.Content.DiesIrae.ResonanceEnergies;
using MagnumOpus.Content.Fate.CraftingStations;

namespace MagnumOpus.Content.DiesIrae.Weapons.WrathsCleaver
{
    public class WrathsCleaver : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 70;
            Item.height = 70;
            Item.damage = 2800;
            Item.DamageType = DamageClass.Melee;
            Item.useTime = 16;
            Item.useAnimation = 16;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 9f;
            Item.value = Item.sellPrice(platinum: 2, gold: 50);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
            Item.UseSound = SoundID.Item71;
            Item.autoReuse = true;
            Item.useTurn = false;
            Item.scale = 1.6f;
            Item.crit = 20;
            Item.shoot = ModContent.ProjectileType<WrathsCleaverSwing>();
            Item.shootSpeed = 12f;
            Item.noMelee = true;
            Item.noUseGraphic = true;
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
            tooltips.Add(new TooltipLine(Mod, "Effect1", "5-phase combo with escalating intensity"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Every 3rd swing spawns 5 homing crystallized flame projectiles"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Hits build Wrath — at maximum, triggers Infernal Eruption"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Eruption marks all nearby enemies, increasing damage taken by 25%"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Forged in the flames of final judgment'")
            {
                OverrideColor = new Color(200, 50, 30)
            });
        }
    }
}