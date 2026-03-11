using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.Eroica.ResonanceEnergies;
using MagnumOpus.Content.Eroica.Enemies;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.Eroica.Accessories.Shared;

namespace MagnumOpus.Content.Eroica.Accessories.SymphonyOfScarletFlames
{
    /// <summary>
    /// Symphony of Scarlet Flames - Ranger accessory.
    /// Triumphant Precision: Hitting same enemy 3 times marks them - 4th hit deals 300% damage with petal explosion.
    /// +15% ranged damage, +10% ranged crit.
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
            var modPlayer = player.GetModPlayer<EroicaAccessoryPlayer>();
            modPlayer.hasSymphonyOfScarletFlames = true;
            
            // Base stat boosts
            player.GetDamage(DamageClass.Ranged) += 0.15f; // +15% ranged damage
            player.GetCritChance(DamageClass.Ranged) += 10f; // +10% ranged crit
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "StatBoosts", "+15% ranged damage, +10% ranged critical strike chance")
            {
                OverrideColor = new Color(255, 200, 100)
            });
            
            tooltips.Add(new TooltipLine(Mod, "PrecisionHeader", "Triumphant Precision:")
            {
                OverrideColor = new Color(255, 100, 100)
            });
            
            tooltips.Add(new TooltipLine(Mod, "MarkEffect", "Hitting the same enemy 2 times marks them as 'Heroic Target'")
            {
                OverrideColor = new Color(255, 180, 100)
            });
            
            tooltips.Add(new TooltipLine(Mod, "ExplosionEffect", "The 3rd hit deals 300% damage and creates a petal explosion")
            {
                OverrideColor = new Color(255, 150, 150)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Flavor", "'Each note builds to the symphony's triumphant crescendo'")
            {
                OverrideColor = new Color(180, 80, 80)
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
