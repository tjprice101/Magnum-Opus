using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.GameContent;
using MagnumOpus.Content.SwanLake.ResonanceEnergies;

namespace MagnumOpus.Content.SwanLake.ResonantOres
{
    public class SwanLakeResonanceOreTile : ModTile
    {
        // Uses the same texture as SwanLakeResonanceOre.png for visual consistency
        public override string Texture => "MagnumOpus/Content/SwanLake/ResonantOres/SwanLakeResonanceOre";

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
            AddMapEntry(new Color(220, 240, 255), name); // Icy white/blue color on map

            DustType = DustID.IceTorch;
            HitSound = SoundID.Tink;

            MineResist = 4.5f;
            MinPick = 450; // Requires Enigma's Pickaxe or better (Swan Lake is just before Fate tier)
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            // Brilliant pearlescent white/blue glow with rainbow shimmer
            float pulse = 0.9f + (float)System.Math.Sin(Main.GameUpdateCount * 0.05f + i * 0.1f + j * 0.1f) * 0.2f;
            float rainbow = Main.GameUpdateCount * 0.02f + i * 0.3f + j * 0.2f;
            
            // Base white with subtle rainbow cycling
            r = (0.9f + 0.1f * (float)System.Math.Sin(rainbow)) * pulse;
            g = (0.95f + 0.05f * (float)System.Math.Sin(rainbow + 2.1f)) * pulse;
            b = (1.0f + 0.0f * (float)System.Math.Sin(rainbow + 4.2f)) * pulse;
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 2 : 5;
        }

        public override void RandomUpdate(int i, int j)
        {
            // Frequent icy sparkle particles - very visible!
            if (Main.rand.NextBool(15))
            {
                Dust dust = Dust.NewDustDirect(new Vector2(i * 16, j * 16), 16, 16, DustID.IceTorch, 0f, -2f, 150, default, 1.2f);
                dust.noGravity = true;
                dust.velocity *= 0.8f;
            }

            // Rainbow shimmer sparkles
            if (Main.rand.NextBool(20))
            {
                // Cycle through rainbow colors
                int dustType = Main.rand.Next(6) switch
                {
                    0 => DustID.PinkTorch,
                    1 => DustID.PurpleTorch,
                    2 => DustID.BlueTorch,
                    3 => DustID.IceTorch,
                    4 => DustID.WhiteTorch,
                    _ => DustID.SilverCoin
                };
                Dust sparkle = Dust.NewDustDirect(new Vector2(i * 16, j * 16), 16, 16, dustType, 0f, -1f, 0, default, 1f);
                sparkle.noGravity = true;
                sparkle.velocity = Main.rand.NextVector2Circular(1f, 1f);
            }
            
            // Extra feathery white particles (swan theme)
            if (Main.rand.NextBool(25))
            {
                Dust feather = Dust.NewDustDirect(new Vector2(i * 16, j * 16), 16, 16, DustID.Cloud, Main.rand.NextFloat(-0.5f, 0.5f), -1f, 100, default, 0.9f);
                feather.noGravity = true;
            }
        }

        public override bool CreateDust(int i, int j, ref int type)
        {
            type = DustID.IceTorch;
            
            // Extra rainbow sparkle effect when mined
            for (int k = 0; k < 4; k++)
            {
                int dustType = k switch
                {
                    0 => DustID.IceTorch,
                    1 => DustID.PurpleTorch,
                    2 => DustID.BlueTorch,
                    _ => DustID.SilverCoin
                };
                Dust dust = Dust.NewDustDirect(new Vector2(i * 16, j * 16), 16, 16, dustType, 0f, 0f, 100, default, 0.9f);
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
            // Drop exactly 1 Remnant of Swan's Harmony per ore block
            yield return new Item(ModContent.ItemType<RemnantOfSwansHarmony>(), 1);
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

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            // Add rainbow shimmer overlay effect when on screen
            if (!Main.gamePaused && Main.instance.IsActive)
            {
                // Only spawn particles occasionally to avoid performance issues
                if (Main.rand.NextBool(180))
                {
                    Vector2 worldPos = new Vector2(i * 16 + Main.rand.Next(16), j * 16 + Main.rand.Next(16));
                    
                    // Check if on screen
                    if (worldPos.X > Main.screenPosition.X - 16 && worldPos.X < Main.screenPosition.X + Main.screenWidth + 16 &&
                        worldPos.Y > Main.screenPosition.Y - 16 && worldPos.Y < Main.screenPosition.Y + Main.screenHeight + 16)
                    {
                        // Rainbow prismatic sparkle
                        int dustType = Main.rand.Next(5) switch
                        {
                            0 => DustID.PinkTorch,
                            1 => DustID.PurpleTorch,
                            2 => DustID.BlueTorch,
                            3 => DustID.IceTorch,
                            _ => DustID.WhiteTorch
                        };
                        
                        Dust shimmer = Dust.NewDustDirect(worldPos, 1, 1, dustType, 0f, -0.5f, 0, default, 0.7f);
                        shimmer.noGravity = true;
                        shimmer.fadeIn = 0.8f;
                        shimmer.velocity = Main.rand.NextVector2Circular(0.3f, 0.3f);
                    }
                }
            }
        }
    }
}
