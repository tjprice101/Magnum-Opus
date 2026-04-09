using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Nachtmusik.ResonanceEnergies;
using MagnumOpus.Content.Nachtmusik.HarmonicCores;
using MagnumOpus.Content.Fate.CraftingStations;
using MagnumOpus.Common;

namespace MagnumOpus.Content.Nachtmusik.Tools
{
    /// <summary>
    /// Serenade of Stars - Nachtmusik wings.
    /// K-key HP amplification: doubles effective HP for 40 seconds (5-minute cooldown).
    /// </summary>
    [AutoloadEquip(EquipType.Wings)]
    public class SerenadeOfStars : ModItem
    {
        private static readonly Color StarIndigo = new Color(100, 120, 200);

        public override void SetStaticDefaults()
        {
            ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new WingStats(
                flyTime: 480,
                flySpeedOverride: 24f,
                accelerationMultiplier: 5.0f,
                hasHoldDownHoverFeatures: true
            );
        }

        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.buyPrice(platinum: 12);
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FlightTime", "8 second flight time, 24 mph, hover enabled"));
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Press K to amplify your HP hearts with musical resonance, doubling your effective HP for 40 seconds (5 minute cooldown)")
            {
                OverrideColor = new Color(200, 210, 230)
            });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The stars sing you upward — and the night holds you aloft'")
            {
                OverrideColor = StarIndigo
            });
        }

        public override void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising,
            ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend)
        {
            ascentWhenFalling = 1.6f;
            ascentWhenRising = 0.32f;
            maxCanAscendMultiplier = 1.8f;
            maxAscentMultiplier = 5.2f;
            constantAscend = 0.28f;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<WingAmplificationPlayer>().hasNachtmusikWings = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<NachtmusikResonantEnergy>(), 25)
                .AddIngredient(ModContent.ItemType<NachtmusikResonantCore>(), 12)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfNachtmusik>(), 2)
                .AddIngredient(ItemID.SoulofFlight, 60)
                .AddIngredient(ItemID.LunarBar, 20)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }
}