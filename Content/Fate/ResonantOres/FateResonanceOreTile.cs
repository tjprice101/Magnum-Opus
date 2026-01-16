using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.GameContent;
using MagnumOpus.Content.Fate.ResonanceEnergies;

namespace MagnumOpus.Content.Fate.ResonantOres
{
    public class FateResonanceOreTile : ModTile
    {
        // Uses FateResonanceOreTile.png - single 16x16 texture rendered for all ore blocks

        public override void SetStaticDefaults()
        {
            TileID.Sets.Ore[Type] = true;
            Main.tileSpelunker[Type] = true;
            Main.tileOreFinderPriority[Type] = 925; // Higher than Enigma
            Main.tileShine2[Type] = true;
            Main.tileShine[Type] = 1450; // High shimmer intensity
            Main.tileLighted[Type] = true;
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileMergeDirt[Type] = false;
            Main.tileFrameImportant[Type] = false;

            LocalizedText name = CreateMapEntryName();
            AddMapEntry(new Color(139, 0, 139), name); // Dark magenta/pink color on map

            DustType = DustID.PinkTorch;
            HitSound = SoundID.Tink;

            MineResist = 6f;
            MinPick = 500; // Requires Enigma pickaxe or better
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            // Dark pink/magenta mystical glow
            float pulse = 0.9f + (float)System.Math.Sin(Main.GameUpdateCount * 0.05f + i * 0.1f + j * 0.1f) * 0.2f;
            r = 0.85f * pulse;
            g = 0.0f * pulse;
            b = 0.85f * pulse;
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 2 : 5;
        }

        public override void RandomUpdate(int i, int j)
        {
            // Cosmic reality-bending particles (Fate theme)
            if (Main.rand.NextBool(15))
            {
                Dust dust = Dust.NewDustDirect(new Vector2(i * 16, j * 16), 16, 16, DustID.PinkTorch, 0f, -2f, 150, default, 1.2f);
                dust.noGravity = true;
                dust.velocity *= 0.8f;
            }

            if (Main.rand.NextBool(20))
            {
                Dust crimson = Dust.NewDustDirect(new Vector2(i * 16, j * 16), 16, 16, DustID.CrimsonTorch, 0f, -1f, 0, default, 1f);
                crimson.noGravity = true;
                crimson.velocity *= 0.5f;
            }
            
            // Dark cosmic shimmer
            if (Main.rand.NextBool(30))
            {
                Dust cosmic = Dust.NewDustDirect(new Vector2(i * 16, j * 16), 16, 16, DustID.Enchanted_Pink, Main.rand.NextFloat(-1f, 1f), -0.5f, 0, default, 0.8f);
                cosmic.noGravity = true;
            }
        }

        public override bool CreateDust(int i, int j, ref int type)
        {
            type = DustID.PinkTorch;
            return true;
        }

        public override bool CanDrop(int i, int j)
        {
            return true;
        }

        public override IEnumerable<Item> GetItemDrops(int i, int j)
        {
            yield return new Item(ModContent.ItemType<FateResonanceOre>());
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
