using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.GameContent;

namespace MagnumOpus.Content.EnigmaVariations.ResonantOres
{
    public class EnigmaResonanceOreTile : ModTile
    {
        // Uses vanilla Emerald Gemstone Block texture

        public override void SetStaticDefaults()
        {
            TileID.Sets.Ore[Type] = true;
            Main.tileSpelunker[Type] = true;
            Main.tileOreFinderPriority[Type] = 920; // Higher than La Campanella
            Main.tileShine2[Type] = true;
            Main.tileShine[Type] = 1400; // High shimmer intensity
            Main.tileLighted[Type] = true;
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileMergeDirt[Type] = false;
            Main.tileFrameImportant[Type] = false;

            LocalizedText name = CreateMapEntryName();
            AddMapEntry(new Color(50, 205, 50), name); // Lime green color on map

            DustType = DustID.GreenTorch;
            HitSound = SoundID.Tink;

            MineResist = 5.5f;
            MinPick = 400; // Requires La Campanella pickaxe or better
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            // Brilliant green glow
            float pulse = 0.9f + (float)System.Math.Sin(Main.GameUpdateCount * 0.05f + i * 0.1f + j * 0.1f) * 0.2f;
            r = 0.2f * pulse;
            g = 0.8f * pulse;
            b = 0.2f * pulse;
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        public override bool CreateDust(int i, int j, ref int type)
        {
            type = DustID.GreenTorch;
            return true;
        }
    }
}
