using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.GameContent;
using MagnumOpus.Common;
using MagnumOpus.Content.OdeToJoy.ResonanceEnergies;

namespace MagnumOpus.Content.OdeToJoy.ResonantOres
{
    /// <summary>
    /// Ode to Joy Resonance Ore Tile - Uses a 4x4 sprite sheet (64x64 total) where each 16x16 frame is a unique ore variant.
    /// Found in the Hallow after defeating the Fate boss.
    /// Theme: Joy and celebration - radiant nature's blessing
    /// </summary>
    public class OdeToJoyResonanceOreTile : ModTile
    {
        // The texture is a 4x4 sprite sheet (64x64) of 16x16 tiles
        public override string Texture => "MagnumOpus/Content/OdeToJoy/ResonantOres/OdeToJoyResonanceOreTile";

        public override void SetStaticDefaults()
        {
            TileID.Sets.Ore[Type] = true;
            Main.tileSpelunker[Type] = true;
            Main.tileOreFinderPriority[Type] = 940; // Similar to Nachtmusik/Dies Irae
            Main.tileShine2[Type] = true;
            Main.tileShine[Type] = 1600; // Very high shimmer intensity - radiant joy
            Main.tileLighted[Type] = true;
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileMergeDirt[Type] = false;
            Main.tileFrameImportant[Type] = false;

            LocalizedText name = CreateMapEntryName();
            AddMapEntry(new Color(255, 182, 193), name); // Rose pink on map

            DustType = DustID.PinkTorch; // Pink fairy dust
            HitSound = SoundID.Tink;

            MineResist = 6.5f; // Similar to other Phase 9 ores
            MinPick = 600; // Requires Fate pickaxe or better
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            // Rose pink and green glow with joyful pulse
            float pulse = 0.85f + (float)System.Math.Sin(Main.GameUpdateCount * 0.06f + i * 0.12f + j * 0.12f) * 0.2f;
            r = 0.95f * pulse;
            g = 0.65f * pulse;
            b = 0.7f * pulse;
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 3 : 6;
        }

        public override void RandomUpdate(int i, int j)
        {
            // Pink petal particles (Ode to Joy theme)
            if (Main.rand.NextBool(12))
            {
                Dust petal = Dust.NewDustDirect(new Vector2(i * 16, j * 16), 16, 16, DustID.PinkTorch, Main.rand.NextFloat(-0.5f, 0.5f), -1.5f, 100, default, 1.2f);
                petal.noGravity = true;
                petal.velocity *= 0.8f;
            }

            // Green nature sparkles
            if (Main.rand.NextBool(16))
            {
                Dust nature = Dust.NewDustDirect(new Vector2(i * 16, j * 16), 16, 16, DustID.JungleGrass, Main.rand.NextFloat(-0.3f, 0.3f), -1f, 0, default, 0.9f);
                nature.noGravity = true;
                nature.velocity *= 0.6f;
            }
            
            // Golden celebration sparks
            if (Main.rand.NextBool(22))
            {
                Dust gold = Dust.NewDustDirect(new Vector2(i * 16, j * 16), 16, 16, DustID.Enchanted_Gold, Main.rand.NextFloat(-0.6f, 0.6f), -1.2f, 0, default, 0.8f);
                gold.noGravity = true;
            }

            // Rainbow/prismatic sparkle (rare - joy theme)
            if (Main.rand.NextBool(30))
            {
                Dust rainbow = Dust.NewDustDirect(new Vector2(i * 16, j * 16), 16, 16, DustID.RainbowMk2, Main.rand.NextFloat(-0.4f, 0.4f), -0.8f, 0, default, 0.7f);
                rainbow.noGravity = true;
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
            // Drop exactly 1 Remnant of Ode to Joy's Bloom per ore block
            yield return new Item(ModContent.ItemType<RemnantOfOdeToJoysBloom>(), 1);
        }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            // Use the sprite sheet properly by setting frameX/frameY to random variants
            Tile tile = Main.tile[i, j];
            
            // Only set frame on first placement or when resetting
            if (tile.TileFrameX == 0 && tile.TileFrameY == 0)
            {
                // Pick a random variant from the 4x4 sheet (0-3 for each axis)
                int variantX = Main.rand.Next(4);
                int variantY = Main.rand.Next(4);
                tile.TileFrameX = (short)(variantX * 18); // 18 = 16 + 2 (tile size + padding)
                tile.TileFrameY = (short)(variantY * 18);
            }
            
            return true;
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            // Add a subtle glow overlay effect
            float pulse = 0.4f + (float)System.Math.Sin(Main.GameUpdateCount * 0.04f + i * 0.3f + j * 0.3f) * 0.2f;
            
            Tile tile = Main.tile[i, j];
            Texture2D texture = TextureAssets.Tile[Type].Value;
            Vector2 offset = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
            Vector2 drawPos = new Vector2(i * 16, j * 16) - Main.screenPosition + offset;
            
            Rectangle sourceRect = new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16);
            Color glowColor = new Color(255, 182, 193) * pulse * 0.3f; // Rose pink glow
            
            spriteBatch.Draw(texture, drawPos, sourceRect, glowColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Ode to Joy Resonance Ore Item - Placeable ore block
    /// </summary>
    public class OdeToJoyResonanceOre : ModItem
    {
        public override string Texture => "MagnumOpus/Assets/Particles/MediumCrystalShard";

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
        }

        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 16;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(gold: 2);
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 10;
            Item.useAnimation = 15;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<OdeToJoyResonanceOreTile>();
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Crystallized echoes of humanity's greatest celebration'") 
            { 
                OverrideColor = new Color(255, 182, 193) // Rose pink
            });
        }

        public override void PostUpdate()
        {
            // Soft pink/green glow
            float pulse = 0.7f + (float)System.Math.Sin(Main.GameUpdateCount * 0.05f) * 0.15f;
            Lighting.AddLight(Item.Center, 0.8f * pulse, 0.55f * pulse, 0.6f * pulse);
            
            // Pink petal sparkles
            if (Main.rand.NextBool(12))
            {
                Dust petal = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.PinkTorch, 0f, 0f, 100, default, 0.9f);
                petal.noGravity = true;
                petal.velocity *= 0.4f;
            }
        }
    }
}
