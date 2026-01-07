using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.MoonlightSonata.ResonanceEnergies;
using MagnumOpus.Content.MoonlightSonata.Enemies;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;

namespace MagnumOpus.Content.MoonlightSonata.Accessories
{
    /// <summary>
    /// Fractal of Moonlight - Summoner accessory.
    /// +3 minion slots.
    /// Minions deal 30% more damage.
    /// Every 10 seconds, all minions synchronize for a "Crescendo Attack" (500% combined damage).
    /// Minion attacks have 2% lifesteal.
    /// </summary>
    public class FractalOfMoonlight : ModItem
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
            var modPlayer = player.GetModPlayer<MoonlightAccessoryPlayer>();
            modPlayer.hasFractalOfMoonlight = true;
            
            // +3 minion slots
            player.maxMinions += 3;
            
            // Note: 30% damage bonus and 2% lifesteal handled in MoonlightAccessoryPlayer
            
            // Ambient particles when equipped
            if (!hideVisual && Main.rand.NextBool(6))
            {
                int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.IceTorch;
                
                // Fractal-like orbiting particles
                float angle = Main.GameUpdateCount * 0.05f + Main.rand.NextFloat(MathHelper.TwoPi);
                float radius = 25f + Main.rand.NextFloat(15f);
                Vector2 offset = new Vector2((float)System.Math.Cos(angle), (float)System.Math.Sin(angle)) * radius;
                
                Dust dust = Dust.NewDustPerfect(player.Center + offset, dustType, 
                    offset.SafeNormalize(Vector2.Zero) * -1f, 100, default, 1.2f);
                dust.noGravity = true;
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<MoonlightsResonantEnergy>(), 5)
                .AddIngredient(ModContent.ItemType<ResonantCoreOfMoonlightSonata>(), 5)
                .AddIngredient(ModContent.ItemType<ShardsOfMoonlitTempo>(), 5)
                .AddIngredient(ItemID.SoulofSight, 5)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
    }
}
