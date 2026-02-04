using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.GameContent;
using MagnumOpus.Content.ClairDeLune.Projectiles;
using MagnumOpus.Content.ClairDeLune.ResonanceEnergies;

namespace MagnumOpus.Content.ClairDeLune.ResonantOres
{
    /// <summary>
    /// Clair de Lune Resonance Ore Tile - FINAL BOSS TIER
    /// Spawns in the sky within cloud pods (5-8 ore surrounded by 1-2 layers of cloud blocks).
    /// Uses a 4x4 sprite sheet (64x64 total) where each 16x16 quadrant is a unique ore variant.
    /// Drops: Remnant of Clair de Lune's Harmony
    /// Theme: Temporal clockwork, crystallized time, crimson energy
    /// </summary>
    public class ClairDeLuneResonanceOreTile : ModTile
    {
        // The texture is a 4x4 sprite sheet (64x64) of 16x16 tiles
        public override string Texture => "MagnumOpus/Content/ClairDeLune/ResonantOres/ClairDeLuneResonanceOreTile";

        public override void SetStaticDefaults()
        {
            TileID.Sets.Ore[Type] = true;
            Main.tileSpelunker[Type] = true;
            Main.tileOreFinderPriority[Type] = 1000; // Highest priority - FINAL BOSS tier
            Main.tileShine2[Type] = true;
            Main.tileShine[Type] = 2000; // Maximum shimmer intensity - temporal crystalline glow
            Main.tileLighted[Type] = true;
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileMergeDirt[Type] = false;
            Main.tileFrameImportant[Type] = false;

            LocalizedText name = CreateMapEntryName();
            // Dark gray with crimson tint on map - clockwork temporal theme
            AddMapEntry(new Color(82, 50, 60), name); // Dark crimson-gray

            DustType = DustID.GemRuby;
            HitSound = SoundID.Tink;

            MineResist = 7.5f; // Toughest ore in the mod
            MinPick = 550; // Requires post-Ode to Joy pickaxe or better
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            // Temporal clockwork glow - dark gray base with crimson pulse and crystal sparkle
            float mechanicalPulse = 0.85f + (float)System.Math.Sin(Main.GameUpdateCount * 0.06f + i * 0.15f + j * 0.1f) * 0.2f;
            float crystalFlicker = (float)System.Math.Sin(Main.GameUpdateCount * 0.1f + i * 0.25f) * 0.12f;
            
            // Dark gray base with crimson energy
            r = 0.35f * mechanicalPulse + crystalFlicker * 0.3f; // Crimson tint
            g = 0.2f * mechanicalPulse;
            b = 0.25f * mechanicalPulse; // Slight purple undertone
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 3 : 8; // More dust for dramatic effect
        }

        public override void RandomUpdate(int i, int j)
        {
            // Clockwork gear dust particles
            if (Main.rand.NextBool(10))
            {
                Dust gear = Dust.NewDustDirect(new Vector2(i * 16, j * 16), 16, 16, DustID.Silver, 0f, -2f, 100, ClairDeLuneColors.DarkGray, 1.2f);
                gear.noGravity = true;
                gear.velocity *= 0.6f;
            }

            // Crimson energy sparks
            if (Main.rand.NextBool(14))
            {
                Dust crimson = Dust.NewDustDirect(new Vector2(i * 16, j * 16), 16, 16, DustID.GemRuby, 0f, -2f, 0, ClairDeLuneColors.Crimson, 1.1f);
                crimson.noGravity = true;
                crimson.velocity *= 0.5f;
            }
            
            // Crystal shimmer
            if (Main.rand.NextBool(20))
            {
                Dust crystal = Dust.NewDustDirect(new Vector2(i * 16, j * 16), 16, 16, DustID.GemDiamond, Main.rand.NextFloat(-0.6f, 0.6f), -0.5f, 0, ClairDeLuneColors.Crystal, 0.9f);
                crystal.noGravity = true;
            }
            
            // Brass accent sparks
            if (Main.rand.NextBool(25))
            {
                Dust brass = Dust.NewDustDirect(new Vector2(i * 16, j * 16), 16, 16, DustID.Enchanted_Gold, Main.rand.NextFloat(-0.8f, 0.8f), -0.6f, 0, ClairDeLuneColors.Brass, 0.8f);
                brass.noGravity = true;
            }
            
            // Occasional lightning spark
            if (Main.rand.NextBool(35))
            {
                Dust lightning = Dust.NewDustDirect(new Vector2(i * 16, j * 16), 16, 16, DustID.Electric, Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-1.5f, 1.5f), 50, ClairDeLuneColors.ElectricBlue, 0.7f);
                lightning.noGravity = true;
            }
        }

        public override bool CreateDust(int i, int j, ref int type)
        {
            // Randomly choose between theme dust types
            int rand = Main.rand.Next(5);
            type = rand switch
            {
                0 => DustID.Silver,        // Clockwork gray
                1 => DustID.GemRuby,       // Crimson energy
                2 => DustID.GemDiamond,    // Crystal
                3 => DustID.Enchanted_Gold, // Brass
                _ => DustID.Electric        // Lightning
            };
            return true;
        }

        public override bool CanDrop(int i, int j)
        {
            return true;
        }

        public override IEnumerable<Item> GetItemDrops(int i, int j)
        {
            // Drop exactly 1 Remnant of Clair de Lune's Harmony per ore block
            yield return new Item(ModContent.ItemType<RemnantOfClairDeLunesHarmony>(), 1);
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
