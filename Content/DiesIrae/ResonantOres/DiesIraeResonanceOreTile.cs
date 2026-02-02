using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.GameContent;
using MagnumOpus.Content.DiesIrae.ResonanceEnergies;

namespace MagnumOpus.Content.DiesIrae.ResonantOres
{
    /// <summary>
    /// Dies Irae Resonance Ore Tile - Uses a 4x4 sprite sheet (64x64 total) where each 16x16 frame is a unique ore variant.
    /// Found in the Underworld after defeating the Fate boss.
    /// Theme: Day of Wrath - infernal hellfire embedded in the depths
    /// </summary>
    public class DiesIraeResonanceOreTile : ModTile
    {
        // The texture is a 4x4 sprite sheet (64x64) of 16x16 tiles
        public override string Texture => "MagnumOpus/Content/DiesIrae/ResonantOres/DiesIraeResonanceOreTile";

        public override void SetStaticDefaults()
        {
            TileID.Sets.Ore[Type] = true;
            Main.tileSpelunker[Type] = true;
            Main.tileOreFinderPriority[Type] = 950; // Higher than Fate ore (925)
            Main.tileShine2[Type] = true;
            Main.tileShine[Type] = 1500; // Very high shimmer intensity - infernal glow
            Main.tileLighted[Type] = true;
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileMergeDirt[Type] = false;
            Main.tileFrameImportant[Type] = false;

            LocalizedText name = CreateMapEntryName();
            AddMapEntry(new Color(139, 0, 0), name); // Blood red on map

            DustType = DustID.Torch; // Fire dust
            HitSound = SoundID.Tink;

            MineResist = 7f; // Harder to mine than Fate ore
            MinPick = 600; // Requires Fate pickaxe or better
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            // Infernal ember glow with ominous pulse
            float pulse = 0.85f + (float)System.Math.Sin(Main.GameUpdateCount * 0.04f + i * 0.15f + j * 0.15f) * 0.25f;
            r = 1.0f * pulse;
            g = 0.25f * pulse;
            b = 0.0f * pulse;
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 3 : 6;
        }

        public override void RandomUpdate(int i, int j)
        {
            // Infernal fire particles (Dies Irae theme)
            if (Main.rand.NextBool(12))
            {
                Dust fire = Dust.NewDustDirect(new Vector2(i * 16, j * 16), 16, 16, DustID.Torch, 0f, -2f, 150, default, 1.3f);
                fire.noGravity = true;
                fire.velocity *= 0.9f;
            }

            // Blood red embers
            if (Main.rand.NextBool(18))
            {
                Dust ember = Dust.NewDustDirect(new Vector2(i * 16, j * 16), 16, 16, DustID.CrimsonTorch, Main.rand.NextFloat(-0.5f, 0.5f), -1.5f, 0, default, 1.1f);
                ember.noGravity = true;
                ember.velocity *= 0.7f;
            }
            
            // Hellfire sparks
            if (Main.rand.NextBool(25))
            {
                Dust spark = Dust.NewDustDirect(new Vector2(i * 16, j * 16), 16, 16, DustID.FlameBurst, Main.rand.NextFloat(-1f, 1f), -0.8f, 0, default, 0.9f);
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
            // Drop exactly 1 Remnant of Dies Irae's Wrath per ore block
            yield return new Item(ModContent.ItemType<RemnantOfDiesIraesWrath>(), 1);
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            // The texture is a 4x4 sprite sheet (64x64 total with 16x16 tiles)
            // We use a seeded random based on tile position to pick a consistent frame
            Texture2D texture = TextureAssets.Tile[Type].Value;
            Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
            Vector2 position = new Vector2(i * 16, j * 16) - Main.screenPosition + zero;
            
            // Get lighting at this tile position
            Color lightColor = Lighting.GetColor(i, j);
            
            // Use tile coordinates as seed for consistent random per-tile
            int seed = i * 7919 + j * 6997;
            var tileRandom = new System.Random(seed);
            
            // Pick random frame from 4x4 grid (0-3 for X, 0-3 for Y)
            int frameX = tileRandom.Next(4);
            int frameY = tileRandom.Next(4);
            
            // Each frame is 16x16 pixels
            Rectangle sourceRect = new Rectangle(frameX * 16, frameY * 16, 16, 16);
            
            spriteBatch.Draw(texture, position, sourceRect, lightColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            
            return false;
        }
    }
}
