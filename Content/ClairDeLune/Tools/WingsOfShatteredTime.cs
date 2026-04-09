using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.ClairDeLune.ResonanceEnergies;
using MagnumOpus.Content.ClairDeLune.HarmonicCores;
using MagnumOpus.Content.Fate.CraftingStations;

namespace MagnumOpus.Content.ClairDeLune.Tools
{
    /// <summary>
    /// Wings of Shattered Time - Clair de Lune wings (supreme tier).
    /// K-key HP amplification: doubles effective HP for 55 seconds (5-minute cooldown).
    /// </summary>
    [AutoloadEquip(EquipType.Wings)]
    public class WingsOfShatteredTime : ModItem
    {
        private static readonly Color IceBlue = new Color(150, 200, 255);

        public override void SetStaticDefaults()
        {
            ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new WingStats(
                flyTime: 650,
                flySpeedOverride: 34f,
                accelerationMultiplier: 6.8f,
                hasHoldDownHoverFeatures: true
            );
        }

        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 36;
            Item.accessory = true;
            Item.value = Item.buyPrice(platinum: 25);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FlightTime", "10.8 second flight time, 34 mph, supreme acceleration, hover enabled"));
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Press K to amplify your HP hearts with musical resonance, doubling your effective HP for 55 seconds (5 minute cooldown)")
            {
                OverrideColor = IceBlue
            });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Wings spun from the first moonbeam of evening — they carry you not through the sky, but through a dream'")
            {
                OverrideColor = IceBlue
            });
        }

        public override void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising,
            ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend)
        {
            ascentWhenFalling = 2.2f;
            ascentWhenRising = 0.42f;
            maxCanAscendMultiplier = 2.4f;
            maxAscentMultiplier = 6.8f;
            constantAscend = 0.38f;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<WingAmplificationPlayer>().hasClairDeLuneWings = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfClairDeLune>(), 35)
                .AddIngredient(ModContent.ItemType<ClairDeLuneResonantEnergy>(), 25)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfClairDeLune>(), 4)
                .AddIngredient(ItemID.LunarBar, 25)
                .AddIngredient(ItemID.SoulofFlight, 50)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }
}