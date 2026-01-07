using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.GameContent;
using MagnumOpus.Content.Eroica.ResonanceEnergies;

namespace MagnumOpus.Content.Eroica.ResonantOres
{
    public class EroicaResonanceOreTile : ModTile
    {
        // Uses EroicaResonanceOreTile.png - single 16x16 texture rendered for all ore blocks

        public override void SetStaticDefaults()
        {
            TileID.Sets.Ore[Type] = true;
            Main.tileSpelunker[Type] = true;
            Main.tileOreFinderPriority[Type] = 905; // Slightly higher than Moonlight
            Main.tileShine2[Type] = true;
            Main.tileShine[Type] = 1100; // Shimmer intensity
            Main.tileLighted[Type] = true;
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileMergeDirt[Type] = false;
            Main.tileFrameImportant[Type] = false;

            LocalizedText name = CreateMapEntryName();
            AddMapEntry(new Color(255, 105, 180), name); // Pink/rose color on map

            DustType = DustID.PinkTorch;
            HitSound = SoundID.Tink;

            MineResist = 4f;
            MinPick = 225; // Luminite-tier pickaxes required
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            // INTENSE pink/rose heroic glow - very visible!
            float pulse = 0.9f + (float)System.Math.Sin(Main.GameUpdateCount * 0.05f + i * 0.1f + j * 0.1f) * 0.2f;
            r = 1.0f * pulse;
            g = 0.3f * pulse;
            b = 0.5f * pulse;
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 2 : 5;
        }

        public override void RandomUpdate(int i, int j)
        {
            // MUCH more frequent sparkle particles (cherry blossom themed) - very visible!
            if (Main.rand.NextBool(15)) // Was 50
            {
                Dust dust = Dust.NewDustDirect(new Vector2(i * 16, j * 16), 16, 16, DustID.PinkTorch, 0f, -2f, 150, default, 1.2f);
                dust.noGravity = true;
                dust.velocity *= 0.8f;
            }

            if (Main.rand.NextBool(20)) // Was 80
            {
                Dust sparkle = Dust.NewDustDirect(new Vector2(i * 16, j * 16), 16, 16, DustID.PinkFairy, 0f, -1f, 0, default, 1f);
                sparkle.noGravity = true;
                sparkle.velocity *= 0.5f;
            }
            
            // Extra cherry blossom petals
            if (Main.rand.NextBool(30))
            {
                Dust petal = Dust.NewDustDirect(new Vector2(i * 16, j * 16), 16, 16, DustID.PinkCrystalShard, Main.rand.NextFloat(-1f, 1f), -0.5f, 0, default, 0.8f);
                petal.noGravity = true;
            }
        }

        public override bool CreateDust(int i, int j, ref int type)
        {
            type = DustID.PinkTorch;
            
            // Extra sparkle effect when mined
            for (int k = 0; k < 3; k++)
            {
                Dust dust = Dust.NewDustDirect(new Vector2(i * 16, j * 16), 16, 16, DustID.PinkFairy, 0f, 0f, 100, default, 0.8f);
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
            // Drop exactly 1 Remnant of Eroica's Triumph per ore block
            yield return new Item(ModContent.ItemType<RemnantOfEroicasTriumph>(), 1);
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            // Draw the single 16x16 texture for every ore block, ignoring frame variations
            Texture2D texture = TextureAssets.Tile[Type].Value;
            Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
            Vector2 position = new Vector2(i * 16, j * 16) - Main.screenPosition + zero;
            
            // Get lighting at this tile position
            Color lightColor = Lighting.GetColor(i, j);
            
            // Draw the entire texture as a single 16x16 tile
            Rectangle sourceRect = new Rectangle(0, 0, texture.Width, texture.Height);
            spriteBatch.Draw(texture, position, sourceRect, lightColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            
            return false; // Skip default drawing
        }
    }
}
