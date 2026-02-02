using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.DataStructures;
using MagnumOpus.Content.MoonlightSonata.ResonantOres;
using MagnumOpus.Content.Eroica.ResonantOres;
using MagnumOpus.Content.SwanLake.ResonantOres;
using MagnumOpus.Content.LaCampanella.ResonantOres;
using MagnumOpus.Content.EnigmaVariations.ResonantOres;
using MagnumOpus.Content.Fate.ResonantOres;
using MagnumOpus.Content.Nachtmusik.ResonantOres;
using MagnumOpus.Content.DiesIrae.ResonantOres;
using MagnumOpus.Content.Common.GrandPiano;

namespace MagnumOpus.Common.Systems
{
    public class MoonlightSonataSystem : ModSystem
    {
        // Moon Lord kill tracking
        public static bool MoonLordKilledOnce { get; set; } = false;
        
        // Main boss kill tracking - These gate miniboss essence drops
        public static bool DownedEroica { get; set; } = false;
        public static bool DownedSwanLake { get; set; } = false;
        public static bool DownedEnigma { get; set; } = false;
        public static bool DownedLaCampanella { get; set; } = false;
        public static bool DownedMoonlitMaestro { get; set; } = false;
        
        // Future boss kill tracking
        public static bool FateBossKilledOnce { get; set; } = false;
        public static bool ClairDeLuneBossKilledOnce { get; set; } = false;
        
        // Post-Fate boss kill tracking (Phase 9 Secondary Theme Progression)
        public static bool DownedNachtmusik { get; set; } = false;
        public static bool DownedDiesIrae { get; set; } = false;
        public static bool DownedOdeToJoy { get; set; } = false;

        // Protected pedestal positions (indestructible)
        public static HashSet<Point> ProtectedPedestalTiles { get; private set; } = new HashSet<Point>();

        public override void ClearWorld()
        {
            MoonLordKilledOnce = false;
            DownedEroica = false;
            DownedSwanLake = false;
            DownedEnigma = false;
            DownedLaCampanella = false;
            DownedMoonlitMaestro = false;
            FateBossKilledOnce = false;
            ClairDeLuneBossKilledOnce = false;
            DownedNachtmusik = false;
            DownedDiesIrae = false;
            DownedOdeToJoy = false;
            PianoRoomCenter = Vector2.Zero;
            ProtectedPedestalTiles.Clear();
        }

        public override void SaveWorldData(TagCompound tag)
        {
            if (MoonLordKilledOnce)
                tag["MoonLordKilledOnce"] = true;
            if (DownedEroica)
                tag["DownedEroica"] = true;
            if (DownedSwanLake)
                tag["DownedSwanLake"] = true;
            if (DownedEnigma)
                tag["DownedEnigma"] = true;
            if (DownedLaCampanella)
                tag["DownedLaCampanella"] = true;
            if (DownedMoonlitMaestro)
                tag["DownedMoonlitMaestro"] = true;
            if (FateBossKilledOnce)
                tag["FateBossKilledOnce"] = true;
            if (ClairDeLuneBossKilledOnce)
                tag["ClairDeLuneBossKilledOnce"] = true;
            if (DownedNachtmusik)
                tag["DownedNachtmusik"] = true;
            if (DownedDiesIrae)
                tag["DownedDiesIrae"] = true;
            if (DownedOdeToJoy)
                tag["DownedOdeToJoy"] = true;
            if (PianoRoomCenter != Vector2.Zero)
            {
                tag["PianoRoomX"] = PianoRoomCenter.X;
                tag["PianoRoomY"] = PianoRoomCenter.Y;
            }
            // Save protected pedestal positions
            if (ProtectedPedestalTiles.Count > 0)
            {
                var positions = new List<int>();
                foreach (var point in ProtectedPedestalTiles)
                {
                    positions.Add(point.X);
                    positions.Add(point.Y);
                }
                tag["ProtectedPedestals"] = positions;
            }
        }

        public override void LoadWorldData(TagCompound tag)
        {
            MoonLordKilledOnce = tag.ContainsKey("MoonLordKilledOnce");
            DownedEroica = tag.ContainsKey("DownedEroica");
            DownedSwanLake = tag.ContainsKey("DownedSwanLake");
            DownedEnigma = tag.ContainsKey("DownedEnigma");
            DownedLaCampanella = tag.ContainsKey("DownedLaCampanella");
            DownedMoonlitMaestro = tag.ContainsKey("DownedMoonlitMaestro");
            FateBossKilledOnce = tag.ContainsKey("FateBossKilledOnce");
            ClairDeLuneBossKilledOnce = tag.ContainsKey("ClairDeLuneBossKilledOnce");
            DownedNachtmusik = tag.ContainsKey("DownedNachtmusik");
            DownedDiesIrae = tag.ContainsKey("DownedDiesIrae");
            DownedOdeToJoy = tag.ContainsKey("DownedOdeToJoy");
            
            if (tag.ContainsKey("PianoRoomX") && tag.ContainsKey("PianoRoomY"))
            {
                PianoRoomCenter = new Vector2(tag.GetFloat("PianoRoomX"), tag.GetFloat("PianoRoomY"));
            }
            
            // Load protected pedestal positions
            ProtectedPedestalTiles.Clear();
            if (tag.ContainsKey("ProtectedPedestals"))
            {
                var positions = tag.GetList<int>("ProtectedPedestals");
                for (int i = 0; i < positions.Count - 1; i += 2)
                {
                    ProtectedPedestalTiles.Add(new Point(positions[i], positions[i + 1]));
                }
            }
        }

        public override void NetSend(BinaryWriter writer)
        {
            BitsByte flags = new BitsByte();
            flags[0] = MoonLordKilledOnce;
            flags[1] = FateBossKilledOnce;
            flags[2] = ClairDeLuneBossKilledOnce;
            flags[3] = DownedEroica;
            flags[4] = DownedSwanLake;
            flags[5] = DownedEnigma;
            flags[6] = DownedLaCampanella;
            flags[7] = DownedMoonlitMaestro;
            writer.Write(flags);
            writer.Write(PianoRoomCenter.X);
            writer.Write(PianoRoomCenter.Y);
            writer.Write(ProtectedPedestalTiles.Count);
            foreach (var point in ProtectedPedestalTiles)
            {
                writer.Write(point.X);
                writer.Write(point.Y);
            }
        }

        public override void NetReceive(BinaryReader reader)
        {
            BitsByte flags = reader.ReadByte();
            MoonLordKilledOnce = flags[0];
            FateBossKilledOnce = flags[1];
            ClairDeLuneBossKilledOnce = flags[2];
            DownedEroica = flags[3];
            DownedSwanLake = flags[4];
            DownedEnigma = flags[5];
            DownedLaCampanella = flags[6];
            DownedMoonlitMaestro = flags[7];
            PianoRoomCenter = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            ProtectedPedestalTiles.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                ProtectedPedestalTiles.Add(new Point(reader.ReadInt32(), reader.ReadInt32()));
            }
        }

        public static void StorePedestalPositions(int startX, int startY, int width, int height)
        {
            for (int x = startX; x < startX + width; x++)
            {
                for (int y = startY; y < startY + height; y++)
                {
                    ProtectedPedestalTiles.Add(new Point(x, y));
                }
            }
        }

        public static void OnFirstMoonLordKill()
        {
            if (MoonLordKilledOnce)
                return;

            MoonLordKilledOnce = true;
            
            // Spawn ores and display messages for all Moon Lord triggered resonances
            SpawnMoonlitResonanceOre();
            SpawnEroicaResonanceOre();
            SpawnSwanLakeResonanceOre();
            SpawnLaCampanellaResonanceOre();
            SpawnEnigmaResonanceOre();
            SpawnFateResonanceOre();
            SpawnGrandPiano();
            DisplayMoonLordResonanceMessages();

            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendData(MessageID.WorldData);
            }
        }

        public static void OnFirstFateBossKill()
        {
            if (FateBossKilledOnce)
                return;

            FateBossKilledOnce = true;
            
            // Spawn Phase 9 theme ores (post-Fate content)
            SpawnNachtmusikResonanceOre();
            SpawnDiesIraeResonanceOre();
            // TODO: SpawnOdeToJoyResonanceOre(); - when implemented
            // TODO: SpawnClairDeLuneResonanceOre(); - when implemented
            
            DisplayFateShatteredMessages();

            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendData(MessageID.WorldData);
            }
        }

        public static void OnFirstClairDeLuneBossKill()
        {
            if (ClairDeLuneBossKilledOnce)
                return;

            ClairDeLuneBossKilledOnce = true;
            DisplayClairDeLuneMessages();

            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendData(MessageID.WorldData);
            }
        }

        private static void DisplayMessage(string message, Color color)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                Main.NewText(message, color);
            }
            else
            {
                Terraria.Chat.ChatHelper.BroadcastChatMessage(
                    Terraria.Localization.NetworkText.FromLiteral(message), color);
            }
        }

        private static void DisplayMoonLordResonanceMessages()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            // Grand Piano announcement
            DisplayMessage("A Grand Piano has materialized deep within the world...", 
                new Color(218, 165, 32));

            // Moonlight Sonata - Purple (world)
            DisplayMessage("Moonlit Resonance Energy spreads throughout the world...", 
                new Color(180, 100, 220));

            // Eroica - Scarlet (underground)
            DisplayMessage("Eroica's Resonance Energy spreads throughout the underground...", 
                new Color(220, 20, 60));

            // Swan Lake - White (sky bubbles)
            DisplayMessage("Swan Lake's Resonance Energy forms celestial bubbles in the skies...", 
                Color.White);

            // La Campanella - Gold/Orange (deserts)
            DisplayMessage("La Campanella's Resonance Energy spreads throughout the deserts...", 
                new Color(255, 165, 0));

            // Enigma - Green (deep jungle)
            DisplayMessage("Enigma's Resonance Energy forms mysterious pods in the depths of the jungle...", 
                new Color(50, 205, 50));

            // Fate - Dark Magenta (corruption/crimson)
            if (WorldGen.crimson)
            {
                DisplayMessage("Fate's Resonance Energy spreads throughout the crimson...", 
                    new Color(139, 0, 139));
            }
            else
            {
                DisplayMessage("Fate's Resonance Energy spreads throughout the corruption...", 
                    new Color(139, 0, 139));
            }
        }

        private static void DisplayFateShatteredMessages()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            // Nachtmusik - Deep Purple/Gold (Underground) - First
            DisplayMessage("Nachtmusik...it whispers among the stars...resonating the Underground with her starlit melody...", 
                new Color(100, 80, 150));

            // Dies Irae - Dark Red (Underworld)
            DisplayMessage("Dies Irae...it judges...resonating the Underworld with his wrathful melody...", 
                new Color(139, 0, 0));

            // Ode to Joy - Gold (Hallow)
            DisplayMessage("Ode to Joy...she sings...resonating the Hallow with her jubilous melody...", 
                new Color(255, 215, 0));

            // Clair de Lune - Soft Blue (Sky)
            DisplayMessage("Clair de Lune...she conducts time to her whim...resonating the skies with her chrono-melodic harmony...", 
                new Color(150, 200, 255));
        }

        private static void DisplayClairDeLuneMessages()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            // Clair de Lune - Soft Blue
            DisplayMessage("The melody of time has been shattered...the planets all shift melodically into a syzygy. Clair de Lune's Resonance Energy spreads throughout the world...", 
                new Color(176, 196, 222));

            // Mercury - Quicksilver/Metallic
            DisplayMessage("The melody of time has been shattered...the planets all shift melodically into a syzygy. The first of the planets awakens...Mercury's Resonance Energy spreads throughout the world...should you dare seek it...", 
                new Color(192, 192, 192));

            // Mars - Blood Red
            DisplayMessage("The melody of time has been shattered...the planets all shift melodically into a syzygy. The bringer of war stirs...Mars' Resonance Energy spreads throughout the world...prepare for battle...", 
                new Color(178, 34, 34));
        }

        // Store piano room location for map marker
        public static Vector2 PianoRoomCenter { get; private set; } = Vector2.Zero;

        private static void SpawnGrandPiano()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            // Single ornate room dimensions
            int roomWidth = 40;
            int roomHeight = 20;
            int pianoWidth = 6;

            // Position: directly below spawn X, right above underworld
            int spawnX = Main.spawnTileX;
            int underworldTop = Main.maxTilesY - 200;
            
            int roomCenterX = spawnX;
            int roomCenterY = underworldTop - roomHeight / 2 - 10;
            
            int roomLeft = roomCenterX - roomWidth / 2;
            int roomRight = roomCenterX + roomWidth / 2;
            int roomTop = roomCenterY - roomHeight / 2;
            int roomBottom = roomCenterY + roomHeight / 2;

            // Store for map marker
            PianoRoomCenter = new Vector2(roomCenterX * 16, roomCenterY * 16);

            // Clear the room area and surrounding vine area
            int vineSpread = 15;
            for (int x = roomLeft - vineSpread; x <= roomRight + vineSpread; x++)
            {
                for (int y = roomTop - vineSpread; y <= roomBottom + vineSpread; y++)
                {
                    if (x < 0 || x >= Main.maxTilesX || y < 0 || y >= Main.maxTilesY)
                        continue;
                    
                    Tile tile = Main.tile[x, y];
                    tile.HasTile = false;
                    tile.WallType = WallID.None;
                    tile.LiquidAmount = 0;
                    tile.LiquidType = LiquidID.Water;
                }
            }

            // Build silver brick vines spreading outward from room corners
            BuildSilverVines(roomLeft, roomTop, roomRight, roomBottom, vineSpread);

            // Build the main room structure (smooth marble exterior)
            BuildOrnateRoom(roomLeft, roomTop, roomWidth, roomHeight, roomCenterX);

            // Place the piano pedestal and piano in the very center
            int pianoX = roomCenterX - pianoWidth / 2;
            int pianoY = roomBottom - 3;

            // Store pedestal tile positions for protection
            StorePedestalPositions(pianoX - 2, pianoY, pianoWidth + 4, 3);

            // Create marble pedestal under piano
            for (int px = pianoX - 2; px <= pianoX + pianoWidth + 1; px++)
            {
                for (int py = pianoY; py <= pianoY + 2; py++)
                {
                    WorldGen.PlaceTile(px, py, TileID.MarbleBlock, forced: true);
                }
            }

            // Clear space above the pedestal for the piano
            for (int px = pianoX; px < pianoX + 6; px++)
            {
                for (int py = pianoY - 5; py < pianoY; py++)
                {
                    WorldGen.KillTile(px, py, noItem: true);
                }
            }

            // Place the Grand Piano
            int pianoTileType = ModContent.TileType<GrandPianoTile>();
            bool placed = WorldGen.PlaceObject(pianoX + 3, pianoY - 1, pianoTileType);
            
            if (!placed)
            {
                // Manual placement
                for (int tx = 0; tx < 6; tx++)
                {
                    for (int ty = 0; ty < 4; ty++)
                    {
                        int tileX = pianoX + tx;
                        int tileY = pianoY - 4 + ty;
                        
                        Tile tile = Main.tile[tileX, tileY];
                        tile.HasTile = true;
                        tile.TileType = (ushort)pianoTileType;
                        tile.TileFrameX = (short)(tx * 18);
                        tile.TileFrameY = (short)(ty * 18);
                    }
                }
            }

            // Add white torches for full lighting
            AddRoomLighting(roomLeft, roomTop, roomWidth, roomHeight, roomCenterX);

            // Sync all tiles
            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendTileSquare(-1, roomLeft - vineSpread, roomTop - vineSpread, roomWidth + vineSpread * 2, roomHeight + vineSpread * 2);
            }
        }

        private static void BuildSilverVines(int roomLeft, int roomTop, int roomRight, int roomBottom, int maxSpread)
        {
            // Create silver brick vine patterns with rainbow brick accents spreading from each corner
            
            // Corner positions
            int[][] corners = new int[][] {
                new int[] { roomLeft, roomTop, -1, -1 },      // Top-left, spread up-left
                new int[] { roomRight, roomTop, 1, -1 },     // Top-right, spread up-right
                new int[] { roomLeft, roomBottom, -1, 1 },   // Bottom-left, spread down-left
                new int[] { roomRight, roomBottom, 1, 1 }    // Bottom-right, spread down-right
            };

            int rainbowCounter = 0;
            foreach (var corner in corners)
            {
                int startX = corner[0];
                int startY = corner[1];
                int dirX = corner[2];
                int dirY = corner[3];
                
                // Main vine trunk
                for (int i = 1; i <= maxSpread; i++)
                {
                    int x = startX + dirX * i;
                    int y = startY + dirY * i;
                    if (x < 0 || x >= Main.maxTilesX || y < 0 || y >= Main.maxTilesY) continue;
                    
                    // Rainbow brick accent every 4th tile
                    bool isRainbow = (i % 4 == 0);
                    WorldGen.PlaceTile(x, y, isRainbow ? TileID.RainbowBrick : TileID.SilverBrick, forced: true);
                    
                    // Add slight waviness with silver brick
                    if (i % 3 == 0)
                    {
                        int waveX = x + dirX;
                        if (waveX >= 0 && waveX < Main.maxTilesX)
                            WorldGen.PlaceTile(waveX, y, TileID.SilverBrick, forced: true);
                    }
                }
                
                // Branch vines (horizontal spread)
                for (int branch = 3; branch <= maxSpread - 2; branch += 4)
                {
                    int branchX = startX + dirX * branch;
                    int branchY = startY + dirY * branch;
                    
                    for (int j = 1; j <= branch / 2; j++)
                    {
                        int x = branchX;
                        int y = branchY + dirY * j;
                        if (x < 0 || x >= Main.maxTilesX || y < 0 || y >= Main.maxTilesY) continue;
                        // Rainbow at branch tips
                        bool isRainbow = (j == branch / 2);
                        WorldGen.PlaceTile(x, y, isRainbow ? TileID.RainbowBrick : TileID.SilverBrick, forced: true);
                    }
                }
                
                // Branch vines (vertical spread) 
                for (int branch = 5; branch <= maxSpread - 2; branch += 5)
                {
                    int branchX = startX + dirX * branch;
                    int branchY = startY + dirY * branch;
                    
                    for (int j = 1; j <= branch / 3; j++)
                    {
                        int x = branchX + dirX * j;
                        int y = branchY;
                        if (x < 0 || x >= Main.maxTilesX || y < 0 || y >= Main.maxTilesY) continue;
                        // Rainbow at branch tips
                        bool isRainbow = (j == branch / 3);
                        WorldGen.PlaceTile(x, y, isRainbow ? TileID.RainbowBrick : TileID.SilverBrick, forced: true);
                    }
                }
                rainbowCounter++;
            }

            // Add silver brick border along room edges with rainbow accents
            int centerX = (roomLeft + roomRight) / 2;
            int centerY = (roomTop + roomBottom) / 2;
            
            // Top edge
            for (int x = roomLeft; x <= roomRight; x++)
            {
                bool isRainbow = (Math.Abs(x - centerX) % 6 == 0);
                WorldGen.PlaceTile(x, roomTop - 1, isRainbow ? TileID.RainbowBrick : TileID.SilverBrick, forced: true);
            }
            // Bottom edge
            for (int x = roomLeft; x <= roomRight; x++)
            {
                bool isRainbow = (Math.Abs(x - centerX) % 6 == 0);
                WorldGen.PlaceTile(x, roomBottom + 1, isRainbow ? TileID.RainbowBrick : TileID.SilverBrick, forced: true);
            }
            // Left edge
            for (int y = roomTop; y <= roomBottom; y++)
            {
                bool isRainbow = (Math.Abs(y - centerY) % 5 == 0);
                WorldGen.PlaceTile(roomLeft - 1, y, isRainbow ? TileID.RainbowBrick : TileID.SilverBrick, forced: true);
            }
            // Right edge  
            for (int y = roomTop; y <= roomBottom; y++)
            {
                bool isRainbow = (Math.Abs(y - centerY) % 5 == 0);
                WorldGen.PlaceTile(roomRight + 1, y, isRainbow ? TileID.RainbowBrick : TileID.SilverBrick, forced: true);
            }
        }

        private static void BuildOrnateRoom(int left, int top, int width, int height, int centerX)
        {
            int right = left + width;
            int bottom = top + height;

            // Floor - Smooth Marble Block
            for (int x = left; x <= right; x++)
            {
                for (int y = bottom - 2; y <= bottom; y++)
                {
                    WorldGen.PlaceTile(x, y, TileID.MarbleBlock, forced: true);
                }
            }

            // Ceiling - Smooth Marble Block
            for (int x = left; x <= right; x++)
            {
                for (int y = top; y <= top + 2; y++)
                {
                    WorldGen.PlaceTile(x, y, TileID.MarbleBlock, forced: true);
                }
            }

            // Side walls - Smooth Marble Block
            for (int y = top; y <= bottom; y++)
            {
                // Left wall
                WorldGen.PlaceTile(left, y, TileID.MarbleBlock, forced: true);
                WorldGen.PlaceTile(left + 1, y, TileID.MarbleBlock, forced: true);
                // Right wall (mirror)
                WorldGen.PlaceTile(right, y, TileID.MarbleBlock, forced: true);
                WorldGen.PlaceTile(right - 1, y, TileID.MarbleBlock, forced: true);
            }

            // Background walls - Marble block walls
            for (int x = left + 2; x < right - 1; x++)
            {
                for (int y = top + 2; y < bottom - 1; y++)
                {
                    Tile tile = Main.tile[x, y];
                    tile.WallType = WallID.MarbleBlock;
                }
            }

            // Create water pools on either side of the room (symmetrical)
            int poolWidth = 6;
            int poolDepth = 3;
            int poolOffset = 8; // Distance from center to pool edge
            
            // Left water pool
            int leftPoolX = centerX - poolOffset - poolWidth;
            int poolY = bottom - 2 - poolDepth;
            CreateWaterPool(leftPoolX, poolY, poolWidth, poolDepth);
            
            // Right water pool (mirror)
            int rightPoolX = centerX + poolOffset;
            CreateWaterPool(rightPoolX, poolY, poolWidth, poolDepth);
        }

        private static void CreateWaterPool(int poolX, int poolY, int poolWidth, int poolDepth)
        {
            // Create marble basin
            // Bottom of pool
            for (int x = poolX - 1; x <= poolX + poolWidth; x++)
            {
                WorldGen.PlaceTile(x, poolY + poolDepth, TileID.MarbleBlock, forced: true);
            }
            // Sides of pool
            for (int y = poolY; y < poolY + poolDepth; y++)
            {
                WorldGen.PlaceTile(poolX - 1, y, TileID.MarbleBlock, forced: true);
                WorldGen.PlaceTile(poolX + poolWidth, y, TileID.MarbleBlock, forced: true);
            }
            
            // Fill with water
            for (int x = poolX; x < poolX + poolWidth; x++)
            {
                for (int y = poolY; y < poolY + poolDepth; y++)
                {
                    Tile tile = Main.tile[x, y];
                    tile.HasTile = false;
                    tile.LiquidAmount = 255;
                    tile.LiquidType = LiquidID.Water;
                }
            }
        }

        private static void AddRoomLighting(int left, int top, int width, int height, int centerX)
        {
            int right = left + width;
            int bottom = top + height;

            // White torches on walls (symmetrical)
            for (int y = top + 5; y < bottom - 4; y += 4)
            {
                WorldGen.PlaceTile(left + 3, y, TileID.Torches, forced: true, style: 5); // White torch
                WorldGen.PlaceTile(right - 3, y, TileID.Torches, forced: true, style: 5); // White torch (mirror)
            }

            // White torches along ceiling (symmetrical)
            for (int offset = 6; offset < width / 2 - 2; offset += 6)
            {
                WorldGen.PlaceTile(centerX - offset, top + 3, TileID.Torches, forced: true, style: 5);
                WorldGen.PlaceTile(centerX + offset, top + 3, TileID.Torches, forced: true, style: 5);
            }

            // Marble chandelier above piano
            WorldGen.PlaceTile(centerX, top + 4, TileID.Chandeliers, forced: true, style: 28);
        }

        private static bool IsAreaClearForPiano(int startX, int startY, int width, int height)
        {
            for (int x = startX; x < startX + width; x++)
            {
                for (int y = startY; y < startY + height; y++)
                {
                    if (x < 0 || x >= Main.maxTilesX || y < 0 || y >= Main.maxTilesY)
                        return false;
                    
                    Tile tile = Main.tile[x, y];
                    // Check if there are any complex structures (chests, etc.) that we shouldn't destroy
                    if (tile.HasTile && TileID.Sets.BasicChest[tile.TileType])
                        return false;
                }
            }
            return true;
        }

        private static void ClearAreaForPiano(int startX, int startY, int width, int height)
        {
            for (int x = startX; x < startX + width; x++)
            {
                for (int y = startY; y < startY + height; y++)
                {
                    if (x < 0 || x >= Main.maxTilesX || y < 0 || y >= Main.maxTilesY)
                        continue;
                    
                    Tile tile = Main.tile[x, y];
                    tile.HasTile = false;
                    tile.WallType = WallID.None;
                    tile.LiquidAmount = 0;
                    
                    if (Main.netMode == NetmodeID.Server)
                    {
                        NetMessage.SendTileSquare(-1, x, y, 1);
                    }
                }
            }
        }

        private static void SpawnMoonlitResonanceOre()
        {
            int tileType = ModContent.TileType<MoonlitResonanceOreTile>();
            
            // Spawn MASSIVE amounts of veins throughout the world - blow up the underground!
            int veinsToSpawn = Main.rand.Next(192, 289); // 192-288 veins (reduced ~4%)
            int successfulVeins = 0;

            for (int attempt = 0; attempt < veinsToSpawn * 20 && successfulVeins < veinsToSpawn; attempt++)
            {
                // Random position in the world (avoiding edges and surface)
                int x = Main.rand.Next(50, Main.maxTilesX - 50);
                int y = Main.rand.Next((int)Main.worldSurface + 20, Main.maxTilesY - 150);

                // Check if the area is valid (solid tile)
                if (Main.tile[x, y].HasTile && Main.tileSolid[Main.tile[x, y].TileType])
                {
                    // Vein sizes of 13-32 tiles (reduced 45% from original)
                    int veinSize = Main.rand.Next(13, 33);
                    
                    if (SpawnOreVein(x, y, tileType, veinSize))
                    {
                        successfulVeins++;
                    }
                }
            }
        }

        private static void SpawnEroicaResonanceOre()
        {
            int tileType = ModContent.TileType<EroicaResonanceOreTile>();
            
            // Spawn MASSIVE amounts of veins in the underground/cavern - blow it up!
            int veinsToSpawn = Main.rand.Next(173, 250); // 173-249 veins (reduced ~4%)
            int successfulVeins = 0;

            for (int attempt = 0; attempt < veinsToSpawn * 20 && successfulVeins < veinsToSpawn; attempt++)
            {
                // Random position in underground/cavern only
                int x = Main.rand.Next(50, Main.maxTilesX - 50);
                // Underground starts at worldSurface, cavern at rockLayer
                int minY = (int)Main.worldSurface + 20;
                int maxY = Main.maxTilesY - 200; // Above underworld
                int y = Main.rand.Next(minY, maxY);

                // Check if the area is valid (solid tile)
                if (Main.tile[x, y].HasTile && Main.tileSolid[Main.tile[x, y].TileType])
                {
                    // Vein sizes of 10-26 tiles (reduced 45% from original)
                    int veinSize = Main.rand.Next(10, 27);
                    
                    if (SpawnOreVein(x, y, tileType, veinSize))
                    {
                        successfulVeins++;
                    }
                }
            }
        }

        /// <summary>
        /// Spawns a connected ore vein using vanilla's OreRunner algorithm.
        /// This creates proper clumped ore deposits like Calamity mod does.
        /// </summary>
        /// <param name="x">Center X position</param>
        /// <param name="y">Center Y position</param>
        /// <param name="tileType">The ore tile type to place</param>
        /// <param name="veinSize">Approximate number of tiles in the vein (used as strength)</param>
        /// <returns>True if ore was placed</returns>
        private static bool SpawnOreVein(int x, int y, int tileType, int veinSize)
        {
            // Use vanilla's OreRunner for proper connected ore veins
            // Strength controls how many tiles are placed (roughly)
            // Steps controls how far the vein "crawls" - more steps = longer veins
            double strength = veinSize * 0.4; // Scale down for OreRunner's algorithm
            int steps = Main.rand.Next(3, 8); // Random crawl length for organic shapes
            
            // OreRunner creates proper connected ore deposits
            WorldGen.OreRunner(x, y, strength, steps, (ushort)tileType);
            
            return true; // OreRunner always places something if position is valid
        }

        private static void SpawnSwanLakeResonanceOre()
        {
            int tileType = ModContent.TileType<SwanLakeResonanceOreTile>();
            
            // Spawn bubble formations on/near sky islands and floating islands
            int bubblesToSpawn = Main.rand.Next(120, 161); // 120-160 bubble formations
            int successfulBubbles = 0;
            int spawnX = Main.spawnTileX;

            for (int attempt = 0; attempt < bubblesToSpawn * 30 && successfulBubbles < bubblesToSpawn; attempt++)
            {
                // Sky island level (around 350-400 tiles above surface)
                int x = Main.rand.Next(50, Main.maxTilesX - 50);
                
                // Skip spawn if within 150 blocks of spawn point
                if (Math.Abs(x - spawnX) < 150)
                    continue;
                
                // Sky island level: between space layer and sky islands (200-400 tiles from surface)
                int minY = Math.Max(50, (int)(Main.worldSurface - 450));
                int maxY = (int)(Main.worldSurface - 150);
                if (minY >= maxY) minY = maxY - 100;
                
                int y = Main.rand.Next(minY, maxY);
                
                // Find a solid tile in this area - look for sky islands
                bool foundSolid = false;
                int searchRadius = 40;
                int solidX = x, solidY = y;
                
                for (int searchAttempt = 0; searchAttempt < 50 && !foundSolid; searchAttempt++)
                {
                    int testX = x + Main.rand.Next(-searchRadius, searchRadius + 1);
                    int testY = y + Main.rand.Next(-searchRadius, searchRadius + 1);
                    
                    if (testX < 0 || testX >= Main.maxTilesX || testY < 0 || testY >= Main.maxTilesY)
                        continue;
                    
                    Tile tile = Main.tile[testX, testY];
                    if (tile.HasTile && Main.tileSolid[tile.TileType])
                    {
                        // Found a sky island block!
                        foundSolid = true;
                        solidX = testX;
                        solidY = testY;
                    }
                }
                
                if (foundSolid)
                {
                    // Use OreRunner on the found solid location
                    int bubbleSize = Main.rand.Next(12, 22); // Larger bubbles for visibility
                    WorldGen.OreRunner(solidX, solidY, bubbleSize * 0.4, Main.rand.Next(3, 6), (ushort)tileType);
                    successfulBubbles++;
                }
            }
            
            // Also spawn some floating ore clusters manually if we didn't get enough
            // These create small floating ore platforms in the sky
            if (successfulBubbles < 30)
            {
                int extraClusters = 50 - successfulBubbles;
                for (int i = 0; i < extraClusters; i++)
                {
                    int x = Main.rand.Next(100, Main.maxTilesX - 100);
                    if (Math.Abs(x - spawnX) < 150) continue;
                    
                    int y = Main.rand.Next(Math.Max(50, (int)(Main.worldSurface - 400)), (int)(Main.worldSurface - 100));
                    
                    // Create a small floating ore cluster manually
                    int clusterSize = Main.rand.Next(3, 6);
                    for (int cx = 0; cx < clusterSize; cx++)
                    {
                        for (int cy = 0; cy < clusterSize; cy++)
                        {
                            int px = x + cx - clusterSize / 2;
                            int py = y + cy - clusterSize / 2;
                            
                            if (px > 0 && px < Main.maxTilesX && py > 0 && py < Main.maxTilesY)
                            {
                                // Only place if empty or replace dirt/stone
                                Tile tile = Main.tile[px, py];
                                if (!tile.HasTile || tile.TileType == TileID.Dirt || tile.TileType == TileID.Stone)
                                {
                                    WorldGen.PlaceTile(px, py, tileType, forced: true);
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void SpawnLaCampanellaResonanceOre()
        {
            int tileType = ModContent.TileType<LaCampanellaResonanceOreTile>();
            
            // Spawn in desert biomes (surface and underground)
            int veinsToSpawn = Main.rand.Next(120, 171); // 120-170 veins (was 180-240)
            int successfulVeins = 0;

            for (int attempt = 0; attempt < veinsToSpawn * 20 && successfulVeins < veinsToSpawn; attempt++)
            {
                // Find desert biomes
                int x = Main.rand.Next(50, Main.maxTilesX - 50);
                int y = Main.rand.Next((int)Main.worldSurface - 50, Main.maxTilesY - 200);

                // Check if we're in a desert biome (sand-heavy area)
                int sandCount = 0;
                for (int checkX = x - 20; checkX <= x + 20; checkX++)
                {
                    for (int checkY = y - 20; checkY <= y + 20; checkY++)
                    {
                        if (checkX < 0 || checkX >= Main.maxTilesX || checkY < 0 || checkY >= Main.maxTilesY)
                            continue;

                        Tile checkTile = Main.tile[checkX, checkY];
                        if (checkTile.HasTile && (checkTile.TileType == TileID.Sand || 
                            checkTile.TileType == TileID.Sandstone || 
                            checkTile.TileType == TileID.HardenedSand ||
                            checkTile.TileType == TileID.CorruptSandstone ||
                            checkTile.TileType == TileID.CrimsonSandstone ||
                            checkTile.TileType == TileID.HallowSandstone))
                        {
                            sandCount++;
                        }
                    }
                }

                // Need at least some sand blocks nearby to qualify as desert
                if (sandCount > 50 && Main.tile[x, y].HasTile && Main.tileSolid[Main.tile[x, y].TileType])
                {
                    int veinSize = Main.rand.Next(12, 27); // Veins (reduced 45%)
                    
                    if (SpawnOreVein(x, y, tileType, veinSize))
                    {
                        successfulVeins++;
                    }
                }
            }
        }

        private static void SpawnEnigmaResonanceOre()
        {
            int tileType = ModContent.TileType<EnigmaResonanceOreTile>();
            
            // Spawn small pod clusters in deep jungle (INCREASED for better availability)
            int podsToSpawn = Main.rand.Next(200, 281); // 200-280 pod clusters (increased for plentiful spawns)
            int successfulPods = 0;

            for (int attempt = 0; attempt < podsToSpawn * 20 && successfulPods < podsToSpawn; attempt++)
            {
                // Deep underground jungle
                int x = Main.rand.Next(50, Main.maxTilesX - 50);
                int y = Main.rand.Next((int)Main.rockLayer, Main.maxTilesY - 200);

                // Check if we're in jungle (mud-heavy area)
                int jungleCount = 0;
                for (int checkX = x - 25; checkX <= x + 25; checkX++)
                {
                    for (int checkY = y - 25; checkY <= y + 25; checkY++)
                    {
                        if (checkX < 0 || checkX >= Main.maxTilesX || checkY < 0 || checkY >= Main.maxTilesY)
                            continue;

                        Tile checkTile = Main.tile[checkX, checkY];
                        if (checkTile.HasTile && (checkTile.TileType == TileID.Mud || 
                            checkTile.TileType == TileID.JungleGrass ||
                            checkTile.TileType == TileID.Chlorophyte))
                        {
                            jungleCount++;
                        }
                    }
                }

                // Need significant jungle presence
                if (jungleCount > 80 && Main.tile[x, y].HasTile && Main.tileSolid[Main.tile[x, y].TileType])
                {
                    // Use OreRunner for proper connected pod veins (reduced 45%)
                    int podSize = Main.rand.Next(6, 10);
                    
                    if (SpawnOreVein(x, y, tileType, podSize))
                    {
                        successfulPods++;
                    }
                }
            }
        }

        private static void SpawnFateResonanceOre()
        {
            int tileType = ModContent.TileType<FateResonanceOreTile>();
            
            // Spawn in corruption or crimson biomes
            int veinsToSpawn = Main.rand.Next(110, 161); // 110-160 veins (was 170-230)
            int successfulVeins = 0;

            for (int attempt = 0; attempt < veinsToSpawn * 20 && successfulVeins < veinsToSpawn; attempt++)
            {
                int x = Main.rand.Next(50, Main.maxTilesX - 50);
                int y = Main.rand.Next((int)Main.worldSurface, Main.maxTilesY - 200);

                // Check if we're in corruption or crimson
                int evilCount = 0;
                for (int checkX = x - 20; checkX <= x + 20; checkX++)
                {
                    for (int checkY = y - 20; checkY <= y + 20; checkY++)
                    {
                        if (checkX < 0 || checkX >= Main.maxTilesX || checkY < 0 || checkY >= Main.maxTilesY)
                            continue;

                        Tile checkTile = Main.tile[checkX, checkY];
                        if (checkTile.HasTile && (checkTile.TileType == TileID.Ebonstone ||
                            checkTile.TileType == TileID.Crimstone ||
                            checkTile.TileType == TileID.CorruptGrass ||
                            checkTile.TileType == TileID.CrimsonGrass ||
                            checkTile.TileType == TileID.Ebonsand ||
                            checkTile.TileType == TileID.Crimsand))
                        {
                            evilCount++;
                        }
                    }
                }

                // Need significant evil biome presence
                if (evilCount > 60 && Main.tile[x, y].HasTile && Main.tileSolid[Main.tile[x, y].TileType])
                {
                    int veinSize = Main.rand.Next(11, 25); // Veins (reduced 45%)
                    
                    if (SpawnOreVein(x, y, tileType, veinSize))
                    {
                        successfulVeins++;
                    }
                }
            }
        }

        /// <summary>
        /// Spawns Nachtmusik Resonance Ore throughout the Underground layer.
        /// This ore spawns after the Fate boss is defeated (post-Fate content).
        /// Location: Underground layer (between surface and caverns)
        /// Vein sizes: 15-28 tiles for moderate visibility
        /// </summary>
        private static void SpawnNachtmusikResonanceOre()
        {
            int tileType = ModContent.TileType<NachtmusikResonanceOreTile>();
            
            // Spawn in Underground layer (somewhat uncommon but findable)
            // Less than Moonlight Sonata (which fills the whole underground) but still plentiful
            int veinsToSpawn = Main.rand.Next(100, 151); // 100-150 veins
            int successfulVeins = 0;

            for (int attempt = 0; attempt < veinsToSpawn * 20 && successfulVeins < veinsToSpawn; attempt++)
            {
                // Random position in the Underground layer (between surface and caverns)
                int x = Main.rand.Next(50, Main.maxTilesX - 50);
                
                // Underground layer: between worldSurface and rockLayer
                int minY = (int)Main.worldSurface + 20;
                int maxY = (int)Main.rockLayer; // Stop at cavern layer
                
                // Make sure we have a valid range
                if (maxY <= minY)
                    maxY = minY + 100;
                
                int y = Main.rand.Next(minY, maxY);

                // Check if the area is valid (solid tile)
                if (Main.tile[x, y].HasTile && Main.tileSolid[Main.tile[x, y].TileType])
                {
                    // Vein sizes of 15-28 tiles (slightly larger for post-Fate content visibility)
                    int veinSize = Main.rand.Next(15, 29);
                    
                    if (SpawnOreVein(x, y, tileType, veinSize))
                    {
                        successfulVeins++;
                    }
                }
            }
        }

        /// <summary>
        /// Spawns Dies Irae Resonance Ore in the Underworld.
        /// This ore spawns after the Fate boss is defeated (post-Fate content).
        /// Location: Underworld (bottom 200 tiles)
        /// Vein sizes: 12-25 tiles for fiery ore clusters
        /// </summary>
        private static void SpawnDiesIraeResonanceOre()
        {
            int tileType = ModContent.TileType<DiesIraeResonanceOreTile>();
            
            // Spawn in Underworld (fiery judgment ore in the depths of hell)
            // Moderate spawn rate - challenging to mine in hell
            int veinsToSpawn = Main.rand.Next(80, 121); // 80-120 veins
            int successfulVeins = 0;

            // Underworld bounds (bottom 200 tiles, avoiding lava lakes as much as possible)
            int underworldTop = Main.maxTilesY - 200;
            int underworldBottom = Main.maxTilesY - 50; // Stay above absolute bottom

            for (int attempt = 0; attempt < veinsToSpawn * 20 && successfulVeins < veinsToSpawn; attempt++)
            {
                // Random position in the Underworld
                int x = Main.rand.Next(100, Main.maxTilesX - 100);
                int y = Main.rand.Next(underworldTop, underworldBottom);

                // Check if the area is valid (solid tile, preferably ash/hellstone)
                Tile tile = Main.tile[x, y];
                if (tile.HasTile && Main.tileSolid[tile.TileType])
                {
                    // Don't replace dungeon bricks, lihzahrd, or containers
                    if (tile.TileType == TileID.LihzahrdBrick || 
                        tile.TileType == TileID.LihzahrdAltar ||
                        tile.TileType == TileID.Containers ||
                        tile.TileType == TileID.Containers2 ||
                        tile.TileType == TileID.BlueDungeonBrick ||
                        tile.TileType == TileID.GreenDungeonBrick ||
                        tile.TileType == TileID.PinkDungeonBrick)
                        continue;

                    // Vein sizes of 12-25 tiles (fiery clusters)
                    int veinSize = Main.rand.Next(12, 26);
                    
                    if (SpawnOreVein(x, y, tileType, veinSize))
                    {
                        successfulVeins++;
                    }
                }
            }
        }
    }
}
