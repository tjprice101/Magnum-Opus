using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;

namespace MagnumOpus.Content.Items
{
    /// <summary>
    /// Debug/cheat item to manually trigger ore spawning.
    /// Useful for existing worlds or testing.
    /// Crafted at no cost for convenience.
    /// </summary>
    public class ResonanceAwakener : ModItem
    {
        // Use the Moonlight ore texture as a placeholder
        public override string Texture => "Terraria/Images/Item_" + ItemID.LunarBar;

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.rare = ItemRarityID.Purple;
            Item.value = 0;
            Item.UseSound = SoundID.Item4;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                // Force trigger ore spawning
                MoonlightSonataSystem.MoonLordKilledOnce = false;
                MoonlightSonataSystem.OnFirstMoonLordKill();
                
                Main.NewText("Resonance energies have been awakened throughout the world!", new Color(200, 100, 255));
                Main.NewText("Moonlit and Eroica ores have been spawned!", new Color(255, 150, 200));
            }
            return true;
        }

        public override void AddRecipes()
        {
            // Free recipe for testing/existing worlds
            CreateRecipe()
                .AddIngredient(ItemID.LunarBar, 1)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
    }
}
