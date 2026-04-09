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
    /// Ember of the Moon - Magic accessory.
    /// 'Resonance Seared' Melodic Attunement.
    /// +10% increased Resonant Burn damage.
    /// Hitting an enemy 15 times with magic damage who's already inflicted with Resonant Burn heals 10% HP.
    /// </summary>
    public class EmberOfTheMoon : ModItem
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
            attunement.magicAttunement = true;
            attunement.resonantBurnDmgBonus += 0.10f;
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Attunement", "'Resonance Seared' Melodic Attunement")
            {
                OverrideColor = new Color(180, 120, 255)
            });
            tooltips.Add(new TooltipLine(Mod, "BurnDmg", "+10% increased Resonant Burn damage")
            {
                OverrideColor = new Color(255, 200, 100)
            });
            tooltips.Add(new TooltipLine(Mod, "Heal", "Hitting a burning enemy 15 times with magic heals 10% max HP")
            {
                OverrideColor = new Color(150, 100, 200)
            });
            tooltips.Add(new TooltipLine(Mod, "Flavor", "'A dying star's last breath of magic'")
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
                .AddIngredient(ItemID.SoulofLight, 5)
                .AddIngredient(ItemID.SoulofNight, 8)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
    }
}
