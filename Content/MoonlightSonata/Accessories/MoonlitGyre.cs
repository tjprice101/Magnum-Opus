using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.MoonlightSonata.ResonanceEnergies;
using MagnumOpus.Content.MoonlightSonata.Enemies;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using MagnumOpus.Content.Common.Accessories;

namespace MagnumOpus.Content.MoonlightSonata.Accessories
{
    /// <summary>
    /// Moonlit Gyre - Ranged accessory.
    /// 'Resonance Pierced' Melodic Attunement.
    /// +10% increased Resonant Burn damage.
    /// Hitting an enemy 25 times with ranged damage who's already inflicted with Resonant Burn heals 10% HP.
    /// </summary>
    public class MoonlitGyre : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.buyPrice(platinum: 2);
            Item.rare = ModContent.RarityType<MoonlightSonataRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var attunement = player.GetModPlayer<MelodicAttunementPlayer>();
            attunement.rangedAttunement = true;
            attunement.resonantBurnDmgBonus += 0.10f;

            var moonlightPlayer = player.GetModPlayer<MoonlightAccessoryPlayer>();
            moonlightPlayer.hasMoonlitGyre = true;
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Attunement", "'Resonance Pierced' Melodic Attunement")
            {
                OverrideColor = new Color(180, 120, 255)
            });
            tooltips.Add(new TooltipLine(Mod, "BurnDmg", "+10% increased Resonant Burn damage")
            {
                OverrideColor = new Color(255, 200, 100)
            });
            tooltips.Add(new TooltipLine(Mod, "Heal", "Hitting a burning enemy 25 times with ranged heals 10% max HP")
            {
                OverrideColor = new Color(150, 100, 200)
            });
            tooltips.Add(new TooltipLine(Mod, "Flavor", "'The gyre empowers those who embrace the moon'")
            {
                OverrideColor = new Color(140, 100, 200)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<MoonlightsResonantEnergy>(), 5)
                .AddIngredient(ModContent.ItemType<ResonantCoreOfMoonlightSonata>(), 5)
                .AddIngredient(ModContent.ItemType<ShardsOfMoonlitTempo>(), 5)
                .AddIngredient(ItemID.SoulofNight, 12)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
    }
}
