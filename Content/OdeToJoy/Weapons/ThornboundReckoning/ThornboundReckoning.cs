using MagnumOpus.Common;
using MagnumOpus.Content.OdeToJoy.Weapons.ThornboundReckoning.Projectiles;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using MagnumOpus.Content.Fate.CraftingStations;
using MagnumOpus.Content.OdeToJoy.HarmonicCores;
using MagnumOpus.Content.OdeToJoy.ResonanceEnergies;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ThornboundReckoning
{
    public class ThornboundReckoning : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 70;
            Item.height = 70;
            Item.damage = 4200;
            Item.DamageType = DamageClass.Melee;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 8f;
            Item.value = Item.sellPrice(platinum: 3, gold: 50);
            Item.rare = ModContent.RarityType<global::MagnumOpus.Common.OdeToJoyRarity>();
            Item.UseSound = SoundID.Item71;
            Item.autoReuse = true;
            Item.scale = 1.5f;
            Item.crit = 18;
            Item.shoot = ModContent.ProjectileType<VineWaveProjectile>();
            Item.shootSpeed = 14f;
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
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Every swing releases a rolling wave of thorny golden vines"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Every 4th swing creates a massive jubilant bloom explosion at the cursor"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Direct hits inflict Poisoned and Venom"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Where thorns take root, jubilant vine eruptions herald the triumph of spring'")
            {
                OverrideColor = new Color(255, 200, 50)
            });
        }
    }
}