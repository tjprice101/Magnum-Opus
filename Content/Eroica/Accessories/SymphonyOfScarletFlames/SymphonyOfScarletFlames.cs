using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.Eroica.ResonanceEnergies;
using MagnumOpus.Content.Eroica.Enemies;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using MagnumOpus.Content.Common.Accessories;

namespace MagnumOpus.Content.Eroica.Accessories.SymphonyOfScarletFlames
{
    /// <summary>
    /// Symphony of Scarlet Flames - Ranged accessory.
    /// 'Resonance Pierced' Melodic Attunement.
    /// +25% increased Resonant Burn damage.
    /// Hitting a burning enemy 25 times with ranged heals 10% HP.
    /// +2.5% crit damage on Resonant Burn enemies.
    /// </summary>
    public class SymphonyOfScarletFlames : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.buyPrice(platinum: 3);
            Item.rare = ModContent.RarityType<EroicaRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var attunement = player.GetModPlayer<MelodicAttunementPlayer>();
            attunement.rangedAttunement = true;
            attunement.resonantBurnDmgBonus += 0.25f;
            attunement.critDmgBonusOnBurn += 0.025f;
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Attunement", "'Resonance Pierced' Melodic Attunement")
            {
                OverrideColor = new Color(255, 100, 100)
            });
            tooltips.Add(new TooltipLine(Mod, "BurnDmg", "+25% increased Resonant Burn damage")
            {
                OverrideColor = new Color(255, 180, 100)
            });
            tooltips.Add(new TooltipLine(Mod, "Heal", "Hitting a burning enemy 25 times with ranged heals 10% max HP")
            {
                OverrideColor = new Color(255, 150, 150)
            });
            tooltips.Add(new TooltipLine(Mod, "CritDmg", "+2.5% critical strike damage on Resonant Burn enemies")
            {
                OverrideColor = new Color(255, 200, 100)
            });
            tooltips.Add(new TooltipLine(Mod, "Flavor", "'Each note builds to the symphony's triumphant crescendo'")
            {
                OverrideColor = new Color(200, 50, 50)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<EroicasResonantEnergy>(), 5)
                .AddIngredient(ModContent.ItemType<ResonantCoreOfEroica>(), 5)
                .AddIngredient(ModContent.ItemType<ShardOfTriumphsTempo>(), 5)
                .AddIngredient(ItemID.SoulofSight, 5)
                .AddIngredient(ItemID.SoulofMight, 10)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
    }
}
