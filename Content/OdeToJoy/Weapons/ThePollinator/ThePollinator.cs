using MagnumOpus.Common;
using MagnumOpus.Content.OdeToJoy.Weapons.ThePollinator.Projectiles;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using MagnumOpus.Content.Fate.CraftingStations;
using MagnumOpus.Content.OdeToJoy.HarmonicCores;
using MagnumOpus.Content.OdeToJoy.ResonanceEnergies;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ThePollinator
{
    public class ThePollinator : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 52;
            Item.height = 28;
            Item.damage = 3200;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 5f;
            Item.value = Item.sellPrice(platinum: 3, gold: 50);
            Item.rare = ModContent.RarityType<global::MagnumOpus.Common.OdeToJoyRarity>();
            Item.UseSound = SoundID.Item11;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.crit = 15;
            Item.shoot = ProjectileID.Bullet;
            Item.shootSpeed = 14f;
            Item.useAmmo = AmmoID.Bullet;
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
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Converts bullets into pollen seeds that home toward enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Pollen seeds burst into homing rose petals on hit"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Every 4th shot fires a larger pollen burst that explodes into a radial shower of petals"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "All projectiles inflict Poisoned"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Where pollen drifts, the garden of joy blooms eternal — each seed a note in nature's jubilant hymn'")
            {
                OverrideColor = new Color(255, 200, 50)
            });
        }
    }
}