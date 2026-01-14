using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.GameContent;

namespace MagnumOpus.Content.LaCampanella.ResonantOres
{
    public class LaCampanellaResonanceOreTile : ModTile
    {
        // Uses vanilla Topaz Gemstone Block texture

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
            num = fail ? 1 : 3;
        }

        public override bool CreateDust(int i, int j, ref int type)
        {
            type = DustID.Torch;
            return true;
        }
    }
}
