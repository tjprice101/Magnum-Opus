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
        // Fallback to vanilla ore texture if custom texture fails to load
        public override string Texture => ModContent.HasAsset("MagnumOpus/Content/LaCampanella/ResonantOres/LaCampanellaResonanceOreTile") 
            ? "MagnumOpus/Content/LaCampanella/ResonantOres/LaCampanellaResonanceOreTile" 
            : "Terraria/Images/Tiles_" + TileID.Hellstone;

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
    }
}
