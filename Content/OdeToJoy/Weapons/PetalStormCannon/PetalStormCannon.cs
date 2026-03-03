using MagnumOpus.Common;
using MagnumOpus.Content.OdeToJoy.Weapons.PetalStormCannon.Projectiles;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using MagnumOpus.Content.Fate.CraftingStations;
using MagnumOpus.Content.OdeToJoy.HarmonicCores;
using MagnumOpus.Content.OdeToJoy.ResonanceEnergies;

namespace MagnumOpus.Content.OdeToJoy.Weapons.PetalStormCannon
{
    public class PetalStormCannon : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 62;
            Item.height = 32;
            Item.damage = 4800;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 50;
            Item.useAnimation = 50;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 12f;
            Item.value = Item.sellPrice(platinum: 3, gold: 50);
            Item.rare = ModContent.RarityType<global::MagnumOpus.Common.OdeToJoyRarity>();
            Item.UseSound = SoundID.Item62;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.crit = 20;
            Item.shoot = ProjectileID.RocketI;
            Item.shootSpeed = 8f;
            Item.useAmmo = AmmoID.Rocket;
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
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Converts any rocket into explosive petal bombs that arc through the air"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Petal bombs detonate into 8 homing shrapnel petals and a lingering petal storm vortex"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "The petal storm lasts 5 seconds, damaging all enemies caught in the whirling bloom"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "All impacts inflict Poisoned and Venom"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Where the cannon roars, a garden erupts — every detonation a verse in the jubilant anthem of creation'")
            {
                OverrideColor = new Color(255, 200, 50)
            });
        }
    }
}