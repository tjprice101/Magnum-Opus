using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.Localization;
using MagnumOpus.Common;

namespace MagnumOpus.Content.MoonlightSonata.CraftingStations
{
    /// <summary>
    /// Moonlight Anvil - a crafting station for forging Moonlight Sonata equipment.
    /// Used to craft Moonlight tools and weapons.
    /// </summary>
    public class MoonlightAnvilTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = false;
            Main.tileSolidTop[Type] = true;
            Main.tileTable[Type] = true;
            Main.tileLighted[Type] = true;

            // 2 tiles wide x 1 tile tall (like a simple anvil)
            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x1);
            TileObjectData.newTile.CoordinateHeights = new[] { 16 };
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(148, 80, 200), Language.GetText("Mods.MagnumOpus.Tiles.MoonlightAnvilTile.MapEntry"));

            // Set dust type for when tile is broken
            DustType = DustID.PurpleCrystalShard;

            // Acts as an anvil for crafting
            AdjTiles = new int[] { TileID.Anvils, TileID.MythrilAnvil };
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            // Soft purple glow
            r = 0.4f;
            g = 0.2f;
            b = 0.6f;
        }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            // Prominent ambient purple sparkles
            if (closer && Main.rand.NextBool(20))
            {
                // Main purple crystal sparkle
                Dust dust = Dust.NewDustDirect(new Vector2(i * 16 - 4, j * 16 - 8), 24, 16, DustID.PurpleCrystalShard, 0f, -0.5f, 100, default, 1.2f);
                dust.noGravity = true;
                dust.velocity *= 0.5f;
                dust.fadeIn = 1.1f;
            }
            
            if (closer && Main.rand.NextBool(35))
            {
                // Floating purple sparkle that rises
                Dust sparkle = Dust.NewDustDirect(new Vector2(i * 16 + Main.rand.Next(-8, 24), j * 16 - 12), 1, 1, DustID.PurpleTorch, Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(-1.5f, -0.5f), 150, default, 0.9f);
                sparkle.noGravity = true;
                sparkle.fadeIn = 1.3f;
            }
            
            if (closer && Main.rand.NextBool(60))
            {
                // Occasional bright shimmer
                Dust shimmer = Dust.NewDustDirect(new Vector2(i * 16, j * 16 - 4), 16, 8, DustID.Enchanted_Pink, 0f, -0.2f, 0, default, 0.6f);
                shimmer.noGravity = true;
                shimmer.velocity *= 0.3f;
            }
        }
    }

    /// <summary>
    /// The item that places the Moonlight Anvil tile.
    /// </summary>
    public class MoonlightAnvil : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 18;
            Item.maxStack = 9999;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<MoonlightAnvilTile>();
            Item.value = Item.sellPrice(gold: 3);
            Item.rare = ModContent.RarityType<MoonlightSonataRarity>();
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonanceEnergies.ResonantCoreOfMoonlightSonata>(), 10)
                .AddTile(TileID.MythrilAnvil) // Mythril/Orichalcum Anvil
                .Register();
        }
    }
}
