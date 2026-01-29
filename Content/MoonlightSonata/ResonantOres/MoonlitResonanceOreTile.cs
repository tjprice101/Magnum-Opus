using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.GameContent;
using MagnumOpus.Content.MoonlightSonata.ResonanceEnergies;

namespace MagnumOpus.Content.MoonlightSonata.ResonantOres
{
    /// <summary>
    /// Moonlit Resonance Ore Tile - Uses a 6x6 sprite sheet (96x96 total) where each 16x16 frame is a unique ore variant.
    /// Each ore block randomly selects one of the 36 frames to display for visual variety.
    /// </summary>
    public class MoonlitResonanceOreTile : ModTile
    {
        // The texture is a 6x6 sprite sheet (96x96) of 16x16 tiles
        public override string Texture => "MagnumOpus/Content/MoonlightSonata/ResonantOres/MoonlitResonanceOreTile";

        public override void SetStaticDefaults()
        {
            TileID.Sets.Ore[Type] = true;
            Main.tileSpelunker[Type] = true;
            Main.tileOreFinderPriority[Type] = 900; // High priority for metal detector
            Main.tileShine2[Type] = true;
            Main.tileShine[Type] = 1200; // Shimmer intensity
            Main.tileLighted[Type] = true;
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileMergeDirt[Type] = false;
            Main.tileFrameImportant[Type] = false;

            LocalizedText name = CreateMapEntryName();
            AddMapEntry(new Color(180, 100, 220), name); // Purple-ish blue color on map

            DustType = DustID.PurpleTorch;
            HitSound = SoundID.Tink;

            MineResist = 4f;
            MinPick = 225; // Luminite-tier pickaxes required (Solar/Vortex/Nebula/Stardust Pickaxe or better)
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            // INTENSE purple glow - very visible!
            float pulse = 0.9f + (float)System.Math.Sin(Main.GameUpdateCount * 0.05f + i * 0.1f + j * 0.1f) * 0.2f;
            r = 0.6f * pulse;
            g = 0.2f * pulse;
            b = 1.0f * pulse;
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 2 : 5;
        }

        public override void RandomUpdate(int i, int j)
        {
            // MUCH more frequent sparkle particles - very visible glow!
            if (Main.rand.NextBool(15)) // Was 60
            {
                Dust dust = Dust.NewDustDirect(new Vector2(i * 16, j * 16), 16, 16, DustID.PurpleTorch, 0f, -2f, 150, default, 1.2f);
                dust.noGravity = true;
                dust.velocity *= 0.8f;
            }
            
            // Extra sparkles
            if (Main.rand.NextBool(25))
            {
                Dust sparkle = Dust.NewDustDirect(new Vector2(i * 16, j * 16), 16, 16, DustID.PurpleCrystalShard, 0f, -1f, 0, default, 1f);
                sparkle.noGravity = true;
                sparkle.velocity = Main.rand.NextVector2Circular(1f, 1f);
            }
        }

        public override bool CreateDust(int i, int j, ref int type)
        {
            // Create sparkly dust when mined
            type = DustID.PurpleTorch;
            
            // Extra sparkle effect
            for (int k = 0; k < 3; k++)
            {
                Dust dust = Dust.NewDustDirect(new Vector2(i * 16, j * 16), 16, 16, DustID.PurpleCrystalShard, 0f, 0f, 100, default, 0.8f);
                dust.noGravity = true;
                dust.velocity = Main.rand.NextVector2Circular(2f, 2f);
            }
            
            return true;
        }

        public override bool CanDrop(int i, int j)
        {
            return true;
        }

        public override IEnumerable<Item> GetItemDrops(int i, int j)
        {
            // Drop exactly 1 Remnant of Moonlight's Harmony per ore block
            yield return new Item(ModContent.ItemType<RemnantOfMoonlightsHarmony>(), 1);
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            // The texture is a 6x6 sprite sheet (96x96 total with 16x16 tiles)
            // We use a seeded random based on tile position to pick a consistent frame
            Texture2D texture = TextureAssets.Tile[Type].Value;
            Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
            Vector2 position = new Vector2(i * 16, j * 16) - Main.screenPosition + zero;
            
            // Get lighting at this tile position
            Color lightColor = Lighting.GetColor(i, j);
            
            // Use tile coordinates as seed for consistent random per-tile
            // This ensures the same tile always shows the same variant
            int seed = i * 7919 + j * 6997; // Prime numbers for better distribution
            var tileRandom = new System.Random(seed);
            
            // Pick random frame from 6x6 grid (0-5 for X, 0-5 for Y)
            int frameX = tileRandom.Next(6);
            int frameY = tileRandom.Next(6);
            
            // Each frame is 16x16 pixels
            Rectangle sourceRect = new Rectangle(frameX * 16, frameY * 16, 16, 16);
            
            spriteBatch.Draw(texture, position, sourceRect, lightColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            
            return false; // Skip default drawing
        }
    }
}
