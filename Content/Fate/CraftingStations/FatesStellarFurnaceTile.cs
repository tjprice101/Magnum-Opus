using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.Localization;
using System.Collections.Generic;
using MagnumOpus.Common;

namespace MagnumOpus.Content.Fate.CraftingStations
{
    /// <summary>
    /// Fate's Stellar Furnace - Post-Fate smelting station required for all Phase 9 content.
    /// Unlocks: Nachtmusik, Dies Irae, Ode to Joy, Clair de Lune bar smelting.
    /// Sprite: 48x48 pixels (3x3 tiles)
    /// </summary>
    public class FatesStellarFurnaceTile : ModTile
    {
        // Use the item texture since tile texture doesn't exist yet
        public override string Texture => "MagnumOpus/Content/Fate/CraftingStations/FatesStellarFurnace";
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = false;
            Main.tileLighted[Type] = true;

            // 3 tiles wide x 3 tiles tall (taller furnace)
            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
            TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 16 };
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(200, 80, 120), Language.GetText("Mods.MagnumOpus.Tiles.FatesStellarFurnaceTile.MapEntry"));

            // Cosmic fire dust
            DustType = DustID.PinkTorch;

            // Acts as a furnace for smelting - includes all previous furnace tiers
            AdjTiles = new int[] { TileID.Furnaces, TileID.Hellforge, TileID.AdamantiteForge };
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            // Cosmic fire glow - brighter pink/crimson with golden accent
            float pulse = 0.8f + (float)System.Math.Sin(Main.GameUpdateCount * 0.06f + i * 0.15f + j * 0.1f) * 0.2f;
            float goldPulse = (float)System.Math.Sin(Main.GameUpdateCount * 0.1f) * 0.1f;
            r = (0.9f + goldPulse) * pulse;
            g = (0.3f + goldPulse * 0.5f) * pulse;
            b = 0.5f * pulse;
        }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            if (!closer) return;
            
            // Rising cosmic flames
            if (Main.rand.NextBool(8))
            {
                Dust flame = Dust.NewDustDirect(
                    new Vector2(i * 16 - 4, j * 16 - 8), 
                    24, 16, 
                    DustID.PinkTorch, 
                    Main.rand.NextFloat(-0.5f, 0.5f), 
                    Main.rand.NextFloat(-2f, -0.8f), 
                    100, default, 1.3f);
                flame.noGravity = true;
                flame.fadeIn = 1.4f;
            }
            
            // Golden star sparks rising from furnace
            if (Main.rand.NextBool(12))
            {
                Dust spark = Dust.NewDustDirect(
                    new Vector2(i * 16 + Main.rand.Next(-8, 24), j * 16 - 20), 
                    1, 1, 
                    DustID.GoldFlame, 
                    Main.rand.NextFloat(-0.3f, 0.3f), 
                    Main.rand.NextFloat(-1.5f, -0.5f), 
                    0, default, 0.9f);
                spark.noGravity = true;
            }
            
            // Cosmic smoke wisps
            if (Main.rand.NextBool(20))
            {
                Dust smoke = Dust.NewDustDirect(
                    new Vector2(i * 16, j * 16 - 24), 
                    16, 8, 
                    DustID.Smoke, 
                    Main.rand.NextFloat(-0.3f, 0.3f), 
                    -0.5f, 
                    180, new Color(180, 80, 120), 1.0f);
                smoke.noGravity = true;
            }
            
            // Crimson ember particles
            if (Main.rand.NextBool(15))
            {
                Dust ember = Dust.NewDustDirect(
                    new Vector2(i * 16 - 8, j * 16), 
                    32, 16, 
                    DustID.Torch, 
                    0f, 
                    Main.rand.NextFloat(-1f, -0.3f), 
                    100, default, 0.7f);
                ember.noGravity = true;
                ember.color = new Color(255, 80, 100);
            }
        }
    }

    /// <summary>
    /// The item that places Fate's Stellar Furnace tile.
    /// Crafted after defeating Fate boss.
    /// </summary>
    public class FatesStellarFurnace : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 48;
            Item.height = 48;
            Item.maxStack = 9999;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<FatesStellarFurnaceTile>();
            Item.value = Item.sellPrice(gold: 15);
            Item.rare = ModContent.RarityType<FateRarity>();
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Advanced crafting station for Fate-tier equipment"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'A forge fueled by dying stars, where cosmic materials are shaped'")
            {
                OverrideColor = new Color(180, 40, 80)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<MoonlightSonata.CraftingStations.MoonlightFurnace>(1)
                .AddIngredient<ResonanceEnergies.FateResonantEnergy>(20)
                .AddIngredient<HarmonicCores.HarmonicCoreOfFate>(10)
                .AddIngredient(ItemID.LunarBar, 15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
}
