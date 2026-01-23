using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using MagnumOpus.Content.Eroica.Bosses;
using MagnumOpus.Content.Eroica.SummonItems;
using MagnumOpus.Content.SwanLake.Bosses;
using MagnumOpus.Content.SwanLake.SummonItems;
using MagnumOpus.Content.LaCampanella.Bosses;
using MagnumOpus.Content.LaCampanella.SummonItems;
using MagnumOpus.Content.EnigmaVariations.Bosses;
using MagnumOpus.Content.EnigmaVariations.SummonItems;
using MagnumOpus.Content.Fate.Bosses;
using MagnumOpus.Content.Fate.SummonItems;

namespace MagnumOpus.Content.Common.GrandPiano
{
    /// <summary>
    /// The Grand Piano - A mystical instrument that appears after Moon Lord's defeat.
    /// Used to summon the musical bosses of Magnum Opus.
    /// Can be mined with a pickaxe and placed anywhere in the world.
    /// </summary>
    public class GrandPianoTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = false;
            Main.tileWaterDeath[Type] = false;
            Main.tileLighted[Type] = true;
            Main.tileSpelunker[Type] = true; // Shows up with spelunker potion

            // 6 tiles wide, 4 tiles tall
            TileObjectData.newTile.CopyFrom(TileObjectData.Style6x3);
            TileObjectData.newTile.Width = 6;
            TileObjectData.newTile.Height = 4;
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16, 16 };
            TileObjectData.newTile.Origin = new Point16(3, 3);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.addTile(Type);

            LocalizedText name = CreateMapEntryName();
            AddMapEntry(new Color(218, 165, 32), name); // Golden color on map

            DustType = DustID.Gold;
            
            // Mineable with a post-Moon Lord pickaxe (Luminite tier)
            MinPick = 225;
            
            // Register the item that drops when mined
            RegisterItemDrop(ModContent.ItemType<GrandPianoItem>());
        }

        public override bool CanExplode(int i, int j)
        {
            return false; // Cannot be blown up by explosives (must be mined properly)
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3; // Golden dust when mined
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            // Warm golden glow
            r = 0.8f;
            g = 0.6f;
            b = 0.2f;
        }

        // Enable smart cursor interaction (yellow highlight)
        public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings)
        {
            return true;
        }

        // Draw yellow highlight outline when hovering
        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            player.noThrow = 2;
            player.cursorItemIconEnabled = true;
            
            // Show the item the player is holding, or a music note icon
            if (player.HeldItem != null && !player.HeldItem.IsAir)
            {
                player.cursorItemIconID = player.HeldItem.type;
            }
            else
            {
                player.cursorItemIconID = ItemID.MusicBox;
            }
        }

        public override void MouseOverFar(int i, int j)
        {
            // Show interaction even from far away
            MouseOver(i, j);
        }

        // Draw yellow selection box around the piano
        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
        {
            // Check if this is the origin tile and player is hovering
            Tile tile = Main.tile[i, j];
            if (tile.TileFrameX == 0 && tile.TileFrameY == 0)
            {
                // Add a subtle yellow glow effect when nearby
                float distance = Vector2.Distance(Main.LocalPlayer.Center, new Vector2(i * 16 + 48, j * 16 + 32));
                if (distance < 200f)
                {
                    float glowIntensity = 1f - (distance / 200f);
                    Lighting.AddLight(new Vector2(i * 16 + 48, j * 16 + 32), 0.5f * glowIntensity, 0.4f * glowIntensity, 0.1f * glowIntensity);
                }
            }
        }

        public override bool RightClick(int i, int j)
        {
            Player player = Main.LocalPlayer;
            Item heldItem = player.HeldItem;

            // Check if holding Score of Eroica
            if (heldItem != null && !heldItem.IsAir && heldItem.type == ModContent.ItemType<ScoreOfEroica>())
            {
                // Check if Eroica boss is already alive
                if (NPC.AnyNPCs(ModContent.NPCType<EroicasRetribution>()))
                {
                    Main.NewText("Eroica, God of Valor is already playing...", 255, 100, 100);
                    return true;
                }

                // Consume 1 item
                heldItem.stack--;
                if (heldItem.stack <= 0)
                {
                    heldItem.TurnToAir();
                }

                // Play piano sound
                SoundEngine.PlaySound(SoundID.Item4, player.Center);

                // Spawn the boss
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<EroicasRetribution>());
                }

                Main.NewText("The Grand Piano plays Eroica's triumphant melody...", 255, 180, 200);
                return true;
            }
            
            // Check if holding Score of Swan Lake
            if (heldItem != null && !heldItem.IsAir && heldItem.type == ModContent.ItemType<ScoreOfSwanLake>())
            {
                // Check if Swan Lake boss is already alive
                if (NPC.AnyNPCs(ModContent.NPCType<SwanLakeTheMonochromaticFractal>()))
                {
                    Main.NewText("Swan Lake is already dancing...", 255, 255, 255);
                    return true;
                }

                // Consume 1 item
                heldItem.stack--;
                if (heldItem.stack <= 0)
                {
                    heldItem.TurnToAir();
                }

                // Play piano sound
                SoundEngine.PlaySound(SoundID.Item4, player.Center);

                // Spawn the boss
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<SwanLakeTheMonochromaticFractal>());
                }

                Main.NewText("The Grand Piano plays Swan Lake's elegant yet ominous melody...", 255, 255, 255);
                return true;
            }

            // Check if holding Score of La Campanella
            if (heldItem != null && !heldItem.IsAir && heldItem.type == ModContent.ItemType<ScoreOfLaCampanella>())
            {
                // Check if La Campanella boss is already alive
                if (NPC.AnyNPCs(ModContent.NPCType<LaCampanellaChimeOfLife>()))
                {
                    Main.NewText("La Campanella, Chime of Life is already ringing...", 255, 140, 40);
                    return true;
                }

                // Consume 1 item
                heldItem.stack--;
                if (heldItem.stack <= 0)
                {
                    heldItem.TurnToAir();
                }

                // Play piano sound
                SoundEngine.PlaySound(SoundID.Item4, player.Center);

                // Spawn the boss
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<LaCampanellaChimeOfLife>());
                }

                Main.NewText("The Grand Piano plays La Campanella's fiery virtuoso melody...", 255, 140, 40);
                return true;
            }
            
            // Check if holding Score of Enigma
            if (heldItem != null && !heldItem.IsAir && heldItem.type == ModContent.ItemType<ScoreOfEnigma>())
            {
                // Check if Enigma boss is already alive
                if (NPC.AnyNPCs(ModContent.NPCType<EnigmaTheHollowMystery>()))
                {
                    Main.NewText("Enigma, The Hollow Mystery is already watching...", 140, 60, 200);
                    return true;
                }

                // Consume 1 item
                heldItem.stack--;
                if (heldItem.stack <= 0)
                {
                    heldItem.TurnToAir();
                }

                // Play piano sound
                SoundEngine.PlaySound(SoundID.Item4, player.Center);

                // Spawn the boss
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<EnigmaTheHollowMystery>());
                }

                Main.NewText("The Grand Piano plays Enigma's mysterious, unanswerable melody...", 140, 60, 200);
                return true;
            }
            
            // Check if holding Score of Fate
            if (heldItem != null && !heldItem.IsAir && heldItem.type == ModContent.ItemType<ScoreOfFate>())
            {
                // Check if Fate boss is already alive
                if (NPC.AnyNPCs(ModContent.NPCType<FateWardenOfMelodies>()))
                {
                    Main.NewText("The Warden of Melodies already conducts the cosmos...", 180, 50, 100);
                    return true;
                }

                // Consume 1 item
                heldItem.stack--;
                if (heldItem.stack <= 0)
                {
                    heldItem.TurnToAir();
                }

                // Play piano sound
                SoundEngine.PlaySound(SoundID.Item4, player.Center);

                // Spawn the boss
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<FateWardenOfMelodies>());
                }

                Main.NewText("The Grand Piano plays Fate's cosmic symphony of inevitability...", 180, 50, 100);
                return true;
            }

            // If holding anything else (or nothing), reject
            Main.NewText("The piano rejects your music...", 180, 180, 180);
            SoundEngine.PlaySound(SoundID.Item11, player.Center); // Reject sound
            
            return true;
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            // Add sparkle effect occasionally
            if (Main.rand.NextBool(120))
            {
                Tile tile = Main.tile[i, j];
                if (tile.TileFrameX == 0 && tile.TileFrameY == 0) // Only from origin tile
                {
                    Vector2 worldPos = new Vector2(i * 16, j * 16);
                    Dust sparkle = Dust.NewDustDirect(worldPos, 96, 64, DustID.GoldCoin, 0f, -1f, 150, default, 0.8f);
                    sparkle.noGravity = true;
                    sparkle.velocity *= 0.3f;
                }
            }
        }
    }
}
