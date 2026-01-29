using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.GameContent;
using MagnumOpus.Content.LaCampanella.ResonanceEnergies;

namespace MagnumOpus.Content.LaCampanella.ResonantOres
{
    /// <summary>
    /// La Campanella Resonance Ore Tile - Uses a 3x3 sprite sheet (48x48 total) where each 16x16 frame is a unique ore variant.
    /// Each ore block randomly selects one of the 9 frames to display for visual variety.
    /// </summary>
    public class LaCampanellaResonanceOreTile : ModTile
    {
        // The texture is a 3x3 sprite sheet (48x48) of 16x16 tiles
        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantOres/LaCampanellaResonanceOreTile";

        public override void SetStaticDefaults()
        {
            TileID.Sets.Ore[Type] = true;
            Main.tileSpelunker[Type] = true;
            Main.tileOreFinderPriority[Type] = 915; // Higher than Swan Lake
            Main.tileShine2[Type] = true;
            Main.tileShine[Type] = 1350; // High shimmer intensity
            Main.tileLighted[Type] = true;
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileMergeDirt[Type] = false;
            Main.tileFrameImportant[Type] = false;

            LocalizedText name = CreateMapEntryName();
            AddMapEntry(new Color(255, 165, 0), name); // Orange color on map

            DustType = DustID.Torch;
            HitSound = SoundID.Tink;

            MineResist = 5f;
            MinPick = 350; // Requires Eroica's Pickaxe or better
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            // Golden orange glow
            float pulse = 0.9f + (float)System.Math.Sin(Main.GameUpdateCount * 0.05f + i * 0.1f + j * 0.1f) * 0.2f;
            r = 1.0f * pulse;
            g = 0.65f * pulse;
            b = 0.0f * pulse;
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 2 : 5;
        }

        public override void RandomUpdate(int i, int j)
        {
            // Smoky fire particles (La Campanella theme)
            if (Main.rand.NextBool(15))
            {
                Dust dust = Dust.NewDustDirect(new Vector2(i * 16, j * 16), 16, 16, DustID.Torch, 0f, -2f, 150, default, 1.2f);
                dust.noGravity = true;
                dust.velocity *= 0.8f;
            }

            if (Main.rand.NextBool(20))
            {
                Dust smoke = Dust.NewDustDirect(new Vector2(i * 16, j * 16), 16, 16, DustID.Smoke, Main.rand.NextFloat(-0.5f, 0.5f), -1f, 100, default, 0.8f);
                smoke.noGravity = true;
                smoke.velocity *= 0.5f;
            }
            
            // Golden flame sparks
            if (Main.rand.NextBool(30))
            {
                Dust spark = Dust.NewDustDirect(new Vector2(i * 16, j * 16), 16, 16, DustID.GoldFlame, Main.rand.NextFloat(-1f, 1f), -0.5f, 0, default, 0.8f);
                spark.noGravity = true;
            }
        }

        public override bool CreateDust(int i, int j, ref int type)
        {
            type = DustID.Torch;
            return true;
        }

        public override bool CanDrop(int i, int j)
        {
            return true;
        }

        public override IEnumerable<Item> GetItemDrops(int i, int j)
        {
            // Drop exactly 1 Remnant of the Bell's Harmony per ore block
            yield return new Item(ModContent.ItemType<RemnantOfTheBellsHarmony>(), 1);
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            // The texture is a 3x3 sprite sheet (48x48 total with 16x16 tiles)
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
            
            // Pick random frame from 3x3 grid (0-2 for X, 0-2 for Y)
            int frameX = tileRandom.Next(3);
            int frameY = tileRandom.Next(3);
            
            // Each frame is 16x16 pixels
            Rectangle sourceRect = new Rectangle(frameX * 16, frameY * 16, 16, 16);
            
            spriteBatch.Draw(texture, position, sourceRect, lightColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            
            return false; // Skip default drawing
        }
    }
}
