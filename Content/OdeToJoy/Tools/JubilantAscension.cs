using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.OdeToJoy.ResonanceEnergies;
using MagnumOpus.Content.OdeToJoy.HarmonicCores;
using MagnumOpus.Content.Fate.CraftingStations;

namespace MagnumOpus.Content.OdeToJoy.Tools
{
    /// <summary>
    /// Jubilant Ascension - Ode to Joy wings.
    /// K-key HP amplification: doubles effective HP for 50 seconds (5-minute cooldown).
    /// </summary>
    [AutoloadEquip(EquipType.Wings)]
    public class JubilantAscension : ModItem
    {
        private static readonly Color WarmGold = new Color(255, 200, 50);

        public override void SetStaticDefaults()
        {
            ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new WingStats(
                flyTime: 600,
                flySpeedOverride: 31f,
                accelerationMultiplier: 6.2f,
                hasHoldDownHoverFeatures: true
            );
        }

        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 34;
            Item.accessory = true;
            Item.value = Item.buyPrice(platinum: 20);
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FlightTime", "10 second flight time, 31 mph, hover enabled"));
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Press K to amplify your HP hearts with musical resonance, doubling your effective HP for 50 seconds (5 minute cooldown)")
            {
                OverrideColor = WarmGold
            });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Rise on wings of joy — for the hymn lifts all who hear it'")
            {
                OverrideColor = WarmGold
            });
        }

        public override void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising,
            ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend)
        {
            ascentWhenFalling = 2.0f;
            ascentWhenRising = 0.38f;
            maxCanAscendMultiplier = 2.2f;
            maxAscentMultiplier = 6.2f;
            constantAscend = 0.35f;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<WingAmplificationPlayer>().hasOdeToJoyWings = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfOdeToJoy>(), 30)
                .AddIngredient(ModContent.ItemType<OdeToJoyResonantEnergy>(), 20)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfOdeToJoy>(), 3)
                .AddIngredient(ItemID.SoulofFlight, 70)
                .AddIngredient(ItemID.LunarBar, 25)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }
}
