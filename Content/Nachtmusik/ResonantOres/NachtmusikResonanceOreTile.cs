using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.GameContent;

namespace MagnumOpus.Content.Nachtmusik.ResonantOres
{
    /// <summary>
    /// Nachtmusik Resonance Ore Tile - Spawns in the Underground layer after Fate is defeated.
    /// Uses a 4x4 sprite sheet (64x64 total) where each 16x16 quadrant is a unique ore variant.
    /// Each ore block randomly selects one of the 16 quadrants to display for visual variety.
    /// Drops: Remnant of Nachtmusik's Harmony
    /// </summary>
    public class NachtmusikResonanceOreTile : ModTile
    {
        // The texture is a 4x4 sprite sheet (64x64) of 16x16 tiles
        public override string Texture => "MagnumOpus/Content/Nachtmusik/ResonantOres/NachtmusikResonanceOreTile";

        // Store random quadrant selection per tile (uses frame data)
        // TileFrameX and TileFrameY will store which quadrant to use

        public override void SetStaticDefaults()
        {
            TileID.Sets.Ore[Type] = true;
            Main.tileSpelunker[Type] = true;
            Main.tileOreFinderPriority[Type] = 950; // Higher than Fate (925) - post-Fate content
            Main.tileShine2[Type] = true;
            Main.tileShine[Type] = 1500; // High shimmer intensity - starlight glow
            Main.tileLighted[Type] = true;
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileMergeDirt[Type] = false;
            Main.tileFrameImportant[Type] = false;

            LocalizedText name = CreateMapEntryName();
            // Deep purple color on map to match Nachtmusik theme
            AddMapEntry(new Color(45, 27, 78), name); // #2D1B4E

            DustType = DustID.PurpleTorch;
            HitSound = SoundID.Tink;

            MineResist = 6.5f; // Slightly tougher than Fate ore
            MinPick = 520; // Requires Fate pickaxe or better
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            // Deep purple with golden starlight shimmer
            float pulse = 0.85f + (float)System.Math.Sin(Main.GameUpdateCount * 0.04f + i * 0.15f + j * 0.1f) * 0.2f;
            float goldenPulse = (float)System.Math.Sin(Main.GameUpdateCount * 0.08f + i * 0.2f) * 0.15f;
            
            // Base deep purple
            r = 0.25f * pulse + goldenPulse * 0.5f; // Gold accent
            g = 0.15f * pulse + goldenPulse * 0.4f;
            b = 0.45f * pulse; // Strong purple
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 2 : 6;
        }

        public override void RandomUpdate(int i, int j)
        {
            // Nocturnal stellar particles (Nachtmusik theme)
            if (Main.rand.NextBool(12))
            {
                Dust dust = Dust.NewDustDirect(new Vector2(i * 16, j * 16), 16, 16, DustID.PurpleTorch, 0f, -2.5f, 150, default, 1.3f);
                dust.noGravity = true;
                dust.velocity *= 0.7f;
            }

            // Golden starlight shimmer
            if (Main.rand.NextBool(18))
            {
                Dust gold = Dust.NewDustDirect(new Vector2(i * 16, j * 16), 16, 16, DustID.GoldFlame, 0f, -1.5f, 0, default, 1.0f);
                gold.noGravity = true;
                gold.velocity *= 0.4f;
            }
            
            // Occasional violet sparkle
            if (Main.rand.NextBool(25))
            {
                Dust sparkle = Dust.NewDustDirect(new Vector2(i * 16, j * 16), 16, 16, DustID.Enchanted_Gold, Main.rand.NextFloat(-0.8f, 0.8f), -0.6f, 0, default, 0.9f);
                sparkle.noGravity = true;
            }
        }

        public override bool CreateDust(int i, int j, ref int type)
        {
            // Randomly choose between purple and gold dust
            type = Main.rand.NextBool(3) ? DustID.GoldFlame : DustID.PurpleTorch;
            return true;
        }

        public override bool CanDrop(int i, int j)
        {
            return true;
        }

        public override IEnumerable<Item> GetItemDrops(int i, int j)
        {
            // Drop exactly 1 Remnant of Nachtmusik's Harmony per ore block
            yield return new Item(ModContent.ItemType<ResonanceEnergies.RemnantOfNachtmusiksHarmony>(), 1);
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            // The texture is a 4x4 sprite sheet (64x64 total with 16x16 tiles)
            // We use a seeded random based on tile position to pick a consistent quadrant
            Texture2D texture = TextureAssets.Tile[Type].Value;
            Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
            Vector2 position = new Vector2(i * 16, j * 16) - Main.screenPosition + zero;
            
            // Get lighting at this tile position
            Color lightColor = Lighting.GetColor(i, j);
            
            // Use tile coordinates as seed for consistent random per-tile
            // This ensures the same tile always shows the same variant
            int seed = i * 7919 + j * 6997; // Prime numbers for better distribution
            var tileRandom = new System.Random(seed);
            
            // Pick random quadrant from 4x4 grid (0-3 for X, 0-3 for Y)
            int quadrantX = tileRandom.Next(4);
            int quadrantY = tileRandom.Next(4);
            
            // Each quadrant is 16x16 pixels
            Rectangle sourceRect = new Rectangle(quadrantX * 16, quadrantY * 16, 16, 16);
            
            spriteBatch.Draw(texture, position, sourceRect, lightColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            
            return false; // Skip default drawing
        }
    }
}
