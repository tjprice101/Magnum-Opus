using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.GameContent;

namespace MagnumOpus.Content.SwanLake.ResonantOres
{
    public class SwanLakeResonanceOreTile : ModTile
    {
        // Uses vanilla Diamond Gemstone Block texture

        public override void SetStaticDefaults()
        {
            TileID.Sets.Ore[Type] = true;
            Main.tileSpelunker[Type] = true;
            Main.tileOreFinderPriority[Type] = 910; // Higher than Eroica
            Main.tileShine2[Type] = true;
            Main.tileShine[Type] = 1300; // High shimmer intensity
            Main.tileLighted[Type] = true;
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileMergeDirt[Type] = false;
            Main.tileFrameImportant[Type] = false;

            LocalizedText name = CreateMapEntryName();
            AddMapEntry(new Color(255, 255, 255), name); // White color on map

            DustType = DustID.SilverCoin;
            HitSound = SoundID.Tink;

            MineResist = 4.5f;
            MinPick = 350; // Requires Eroica's Pickaxe or better
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            // Brilliant white glow like moonlight
            float pulse = 0.9f + (float)System.Math.Sin(Main.GameUpdateCount * 0.05f + i * 0.1f + j * 0.1f) * 0.2f;
            r = 1.0f * pulse;
            g = 1.0f * pulse;
            b = 1.0f * pulse;
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        public override bool CreateDust(int i, int j, ref int type)
        {
            type = DustID.SilverCoin;
            return true;
        }
    }
}
