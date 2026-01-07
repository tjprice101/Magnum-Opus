using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.DataStructures;
using Terraria.Localization;
using MagnumOpus.Common;

namespace MagnumOpus.Content.MoonlightSonata.CraftingStations
{
    /// <summary>
    /// Moonlight Furnace - a crafting station for smelting Moonlight Sonata materials.
    /// Used to craft Resonant Core of Moonlight Sonata from Remnants.
    /// </summary>
    public class MoonlightFurnaceTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = false;
            Main.tileLighted[Type] = true;

            // 3 tiles wide x 2 tiles tall (54x36 pixels)
            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
            TileObjectData.newTile.CoordinateHeights = new[] { 16, 18 };
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(148, 80, 200), Language.GetText("Mods.MagnumOpus.Tiles.MoonlightFurnaceTile.MapEntry"));

            // Set dust type for when tile is broken
            DustType = DustID.PurpleTorch;

            // Acts as a furnace for crafting
            AdjTiles = new int[] { TileID.Furnaces, TileID.AdamantiteForge };
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            // Purple-ish glow
            r = 0.5f;
            g = 0.25f;
            b = 0.75f;
        }

        public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset)
        {
            // Optional: Add animated flames/glow effect here if you have multiple frames
        }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            // Prominent ambient purple sparkles
            if (closer && Main.rand.NextBool(15))
            {
                // Rising purple flame particles
                Dust dust = Dust.NewDustDirect(new Vector2(i * 16 + Main.rand.Next(-4, 20), j * 16 - 8), 8, 8, DustID.PurpleTorch, Main.rand.NextFloat(-0.2f, 0.2f), Main.rand.NextFloat(-2f, -0.8f), 150, default, 1.3f);
                dust.noGravity = true;
                dust.fadeIn = 1.2f;
            }
            
            if (closer && Main.rand.NextBool(25))
            {
                // Purple crystal sparkle
                Dust sparkle = Dust.NewDustDirect(new Vector2(i * 16 - 4, j * 16 - 12), 24, 8, DustID.PurpleCrystalShard, 0f, -0.3f, 100, default, 1.0f);
                sparkle.noGravity = true;
                sparkle.velocity *= 0.4f;
                sparkle.fadeIn = 1.1f;
            }
            
            if (closer && Main.rand.NextBool(40))
            {
                // Bright shimmer effect
                Dust shimmer = Dust.NewDustDirect(new Vector2(i * 16 + Main.rand.Next(0, 16), j * 16 - 6), 1, 1, DustID.Enchanted_Pink, 0f, -0.5f, 0, default, 0.7f);
                shimmer.noGravity = true;
            }
            
            if (closer && Main.rand.NextBool(50))
            {
                // Occasional ember
                Dust ember = Dust.NewDustDirect(new Vector2(i * 16 + Main.rand.Next(4, 12), j * 16 - 4), 1, 1, DustID.Firework_Pink, Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-1f, -0.3f), 100, default, 0.5f);
                ember.noGravity = true;
            }
        }
    }

    /// <summary>
    /// The item that places the Moonlight Furnace tile.
    /// </summary>
    public class MoonlightFurnace : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = 9999;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<MoonlightFurnaceTile>();
            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ModContent.RarityType<MoonlightSonataRarity>();
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonanceEnergies.RemnantOfMoonlightsHarmony>(), 30)
                .AddIngredient(ItemID.TitaniumForge, 1)
                .AddTile(TileID.MythrilAnvil) // Mythril/Orichalcum Anvil
                .Register();

            // Alternative recipe with Adamantite Forge
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonanceEnergies.RemnantOfMoonlightsHarmony>(), 30)
                .AddIngredient(ItemID.AdamantiteForge, 1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
