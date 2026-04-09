using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.DiesIrae.ResonanceEnergies;
using MagnumOpus.Content.DiesIrae.HarmonicCores;
using MagnumOpus.Content.Fate.CraftingStations;

namespace MagnumOpus.Content.DiesIrae.Tools
{
    /// <summary>
    /// Wings of Damnation - Dies Irae wings.
    /// K-key HP amplification: doubles effective HP for 45 seconds (5-minute cooldown).
    /// </summary>
    [AutoloadEquip(EquipType.Wings)]
    public class WingsOfDamnation : ModItem
    {
        private static readonly Color BloodRed = new Color(200, 50, 30);

        public override void SetStaticDefaults()
        {
            ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new WingStats(
                flyTime: 560,
                flySpeedOverride: 28f,
                accelerationMultiplier: 5.8f,
                hasHoldDownHoverFeatures: true
            );
        }

        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.buyPrice(platinum: 18);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FlightTime", "9.3 second flight time, 28 mph, hover enabled"));
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Press K to amplify your HP hearts with musical resonance, doubling your effective HP for 45 seconds (5 minute cooldown)")
            {
                OverrideColor = BloodRed
            });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Wings forged in the flames of final judgment — they do not carry you to heaven'")
            {
                OverrideColor = BloodRed
            });
        }

        public override void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising,
            ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend)
        {
            ascentWhenFalling = 1.8f;
            ascentWhenRising = 0.36f;
            maxCanAscendMultiplier = 2.0f;
            maxAscentMultiplier = 5.8f;
            constantAscend = 0.32f;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<WingAmplificationPlayer>().hasDiesIraeWings = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<DiesIraeResonantEnergy>(), 30)
                .AddIngredient(ModContent.ItemType<ResonantCoreOfDiesIrae>(), 15)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfDiesIrae>(), 3)
                .AddIngredient(ItemID.SoulofFlight, 80)
                .AddIngredient(ItemID.LunarBar, 25)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }
}