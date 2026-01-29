using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.Localization;
using MagnumOpus.Common;

namespace MagnumOpus.Content.Fate.CraftingStations
{
    /// <summary>
    /// Fate's Cosmic Anvil - Post-Fate crafting station required for all Phase 9 content.
    /// Unlocks: Nachtmusik, Dies Irae, Ode to Joy, Clair de Lune equipment crafting.
    /// Sprite: 64x32 pixels (4x2 tiles)
    /// </summary>
    public class FatesCosmicAnvilTile : ModTile
    {
        // Use the item texture since tile texture doesn't exist yet
        public override string Texture => "MagnumOpus/Content/Fate/CraftingStations/FatesCosmicAnvil";
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = false;
            Main.tileSolidTop[Type] = true;
            Main.tileTable[Type] = true;
            Main.tileLighted[Type] = true;

            // 4 tiles wide x 2 tiles tall (larger cosmic anvil)
            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x1);
            TileObjectData.newTile.Width = 4;
            TileObjectData.newTile.Height = 2;
            TileObjectData.newTile.CoordinateHeights = new[] { 16, 16 };
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.Origin = new Terraria.DataStructures.Point16(1, 1);
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(180, 50, 100), Language.GetText("Mods.MagnumOpus.Tiles.FatesCosmicAnvilTile.MapEntry"));

            // Cosmic dust
            DustType = DustID.PinkTorch;

            // Acts as an anvil for crafting - includes all previous anvil tiers
            AdjTiles = new int[] { TileID.Anvils, TileID.MythrilAnvil };
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            // Dark pink cosmic glow with pulsing
            float pulse = 0.85f + (float)System.Math.Sin(Main.GameUpdateCount * 0.05f + i * 0.1f) * 0.15f;
            r = 0.7f * pulse;
            g = 0.2f * pulse;
            b = 0.5f * pulse;
        }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            if (!closer) return;
            
            // Cosmic pink/crimson sparkles
            if (Main.rand.NextBool(15))
            {
                Dust dust = Dust.NewDustDirect(
                    new Vector2(i * 16 - 8, j * 16 - 12), 
                    32, 24, 
                    DustID.PinkTorch, 
                    Main.rand.NextFloat(-0.3f, 0.3f), 
                    Main.rand.NextFloat(-1f, -0.3f), 
                    120, default, 1.1f);
                dust.noGravity = true;
                dust.fadeIn = 1.2f;
            }
            
            // Golden star sparkles
            if (Main.rand.NextBool(25))
            {
                Dust gold = Dust.NewDustDirect(
                    new Vector2(i * 16 + Main.rand.Next(-4, 20), j * 16 - 16), 
                    1, 1, 
                    DustID.GoldFlame, 
                    0f, -0.8f, 
                    0, default, 0.7f);
                gold.noGravity = true;
            }
            
            // Cosmic purple wisps
            if (Main.rand.NextBool(40))
            {
                Dust cosmic = Dust.NewDustDirect(
                    new Vector2(i * 16, j * 16 - 8), 
                    16, 8, 
                    DustID.PurpleTorch, 
                    Main.rand.NextFloat(-0.5f, 0.5f), 
                    -0.3f, 
                    150, default, 0.8f);
                cosmic.noGravity = true;
                cosmic.velocity *= 0.5f;
            }
        }
    }

    /// <summary>
    /// The item that places Fate's Cosmic Anvil tile.
    /// Crafted after defeating Fate boss.
    /// </summary>
    public class FatesCosmicAnvil : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 64;
            Item.height = 32;
            Item.maxStack = 9999;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<FatesCosmicAnvilTile>();
            Item.value = Item.sellPrice(gold: 15);
            Item.rare = ModContent.RarityType<FateRarity>();
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<MoonlightSonata.CraftingStations.MoonlightAnvil>(1)
                .AddIngredient<ResonanceEnergies.FateResonantEnergy>(20)
                .AddIngredient<HarmonicCores.HarmonicCoreOfFate>(10)
                .AddIngredient(ItemID.LunarBar, 15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
}
