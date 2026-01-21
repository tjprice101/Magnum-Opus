using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace MagnumOpus.Common.Systems
{
    /// <summary>
    /// Draws music note icons on the minimap to show miniboss locations.
    /// Each theme has its own color scheme for visual identification.
    /// </summary>
    public class MinibossMinimapSystem : ModSystem
    {
        // Registered miniboss types and their theme colors
        private static Dictionary<int, MinibossIconData> registeredMinibosses = new Dictionary<int, MinibossIconData>();
        
        private static Asset<Texture2D> musicNoteTexture;
        
        public struct MinibossIconData
        {
            public Color PrimaryColor;
            public Color SecondaryColor;
            public string ThemeName;
            
            public MinibossIconData(Color primary, Color secondary, string theme)
            {
                PrimaryColor = primary;
                SecondaryColor = secondary;
                ThemeName = theme;
            }
        }
        
        // Theme color definitions
        public static class ThemeColors
        {
            // Eroica - Heroic Gold/Scarlet
            public static readonly Color EroicaPrimary = new Color(255, 200, 80);
            public static readonly Color EroicaSecondary = new Color(200, 50, 50);
            
            // La Campanella - Infernal Orange/Black
            public static readonly Color LaCampanellaPrimary = new Color(255, 140, 40);
            public static readonly Color LaCampanellaSecondary = new Color(40, 25, 20);
            
            // Swan Lake - White/Rainbow shimmer
            public static readonly Color SwanLakePrimary = Color.White;
            public static readonly Color SwanLakeSecondary = new Color(200, 200, 255);
            
            // Moonlight Sonata - Purple/Blue
            public static readonly Color MoonlightPrimary = new Color(138, 43, 226);
            public static readonly Color MoonlightSecondary = new Color(135, 206, 250);
            
            // Enigma Variations - Purple/Green
            public static readonly Color EnigmaPrimary = new Color(140, 60, 200);
            public static readonly Color EnigmaSecondary = new Color(50, 220, 100);
            
            // Fate - Pink/Crimson
            public static readonly Color FatePrimary = new Color(200, 80, 120);
            public static readonly Color FateSecondary = new Color(180, 30, 60);
        }
        
        public override void Load()
        {
            // Load music note texture
            musicNoteTexture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/MusicNote", AssetRequestMode.AsyncLoad);
        }
        
        public override void Unload()
        {
            registeredMinibosses?.Clear();
            registeredMinibosses = null;
            musicNoteTexture = null;
        }
        
        /// <summary>
        /// Register a miniboss NPC type to show on the minimap with a music note icon.
        /// Call this in your NPC's SetStaticDefaults.
        /// </summary>
        public static void RegisterMiniboss(int npcType, Color primaryColor, Color secondaryColor, string themeName)
        {
            if (registeredMinibosses == null)
                registeredMinibosses = new Dictionary<int, MinibossIconData>();
                
            registeredMinibosses[npcType] = new MinibossIconData(primaryColor, secondaryColor, themeName);
        }
        
        /// <summary>
        /// Helper to register Eroica theme miniboss
        /// </summary>
        public static void RegisterEroicaMiniboss(int npcType)
        {
            RegisterMiniboss(npcType, ThemeColors.EroicaPrimary, ThemeColors.EroicaSecondary, "Eroica");
        }
        
        /// <summary>
        /// Helper to register La Campanella theme miniboss
        /// </summary>
        public static void RegisterLaCampanellaMiniboss(int npcType)
        {
            RegisterMiniboss(npcType, ThemeColors.LaCampanellaPrimary, ThemeColors.LaCampanellaSecondary, "LaCampanella");
        }
        
        /// <summary>
        /// Helper to register Swan Lake theme miniboss
        /// </summary>
        public static void RegisterSwanLakeMiniboss(int npcType)
        {
            RegisterMiniboss(npcType, ThemeColors.SwanLakePrimary, ThemeColors.SwanLakeSecondary, "SwanLake");
        }
        
        /// <summary>
        /// Helper to register Enigma theme miniboss
        /// </summary>
        public static void RegisterEnigmaMiniboss(int npcType)
        {
            RegisterMiniboss(npcType, ThemeColors.EnigmaPrimary, ThemeColors.EnigmaSecondary, "Enigma");
        }
        
        /// <summary>
        /// Helper to register Fate theme miniboss
        /// </summary>
        public static void RegisterFateMiniboss(int npcType)
        {
            RegisterMiniboss(npcType, ThemeColors.FatePrimary, ThemeColors.FateSecondary, "Fate");
        }
        
        /// <summary>
        /// Helper to register Moonlight Sonata theme miniboss
        /// </summary>
        public static void RegisterMoonlightMiniboss(int npcType)
        {
            RegisterMiniboss(npcType, ThemeColors.MoonlightPrimary, ThemeColors.MoonlightSecondary, "MoonlightSonata");
        }
        
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            // Find the map layer and insert our drawing after it
            int mapIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Map / Minimap"));
            if (mapIndex != -1)
            {
                layers.Insert(mapIndex + 1, new LegacyGameInterfaceLayer(
                    "MagnumOpus: Miniboss Minimap Icons",
                    delegate
                    {
                        DrawMinibossIcons();
                        return true;
                    },
                    InterfaceScaleType.UI
                ));
            }
        }
        
        private void DrawMinibossIcons()
        {
            // Only draw when minimap is visible
            if (!Main.mapEnabled || Main.mapStyle == 0)
                return;
                
            if (musicNoteTexture == null || !musicNoteTexture.IsLoaded)
                return;
                
            if (registeredMinibosses == null || registeredMinibosses.Count == 0)
                return;
            
            Player player = Main.LocalPlayer;
            Texture2D texture = musicNoteTexture.Value;
            Vector2 origin = new Vector2(texture.Width / 2f, texture.Height / 2f);
            
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active)
                    continue;
                    
                if (!registeredMinibosses.TryGetValue(npc.type, out MinibossIconData iconData))
                    continue;
                
                // Calculate position on minimap
                Vector2 mapPosition = GetMinimapPosition(npc.Center, player.Center);
                
                if (mapPosition == Vector2.Zero)
                    continue;
                
                // Pulse animation
                float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.1f + npc.whoAmI * 0.5f) * 0.15f;
                float scale = 0.5f * pulse;
                
                // Color gradient animation
                float colorLerp = (float)Math.Sin(Main.GameUpdateCount * 0.08f + npc.whoAmI * 0.3f) * 0.5f + 0.5f;
                Color drawColor = Color.Lerp(iconData.PrimaryColor, iconData.SecondaryColor, colorLerp);
                
                // Draw glow behind
                Main.spriteBatch.Draw(
                    texture,
                    mapPosition,
                    null,
                    drawColor * 0.4f,
                    0f,
                    origin,
                    scale * 1.3f,
                    SpriteEffects.None,
                    0f
                );
                
                // Draw main icon
                Main.spriteBatch.Draw(
                    texture,
                    mapPosition,
                    null,
                    drawColor,
                    0f,
                    origin,
                    scale,
                    SpriteEffects.None,
                    0f
                );
            }
        }
        
        private Vector2 GetMinimapPosition(Vector2 worldPosition, Vector2 playerCenter)
        {
            // Handle different map modes
            if (Main.mapStyle == 1) // Minimap (corner map)
            {
                return GetCornerMinimapPosition(worldPosition, playerCenter);
            }
            else if (Main.mapStyle == 2) // Full map overlay
            {
                return GetFullMapPosition(worldPosition);
            }
            
            return Vector2.Zero;
        }
        
        private Vector2 GetCornerMinimapPosition(Vector2 worldPosition, Vector2 playerCenter)
        {
            // Minimap dimensions and position (top right corner)
            float mapScale = Main.mapMinimapScale;
            int mapWidth = (int)(240 * mapScale);
            int mapHeight = (int)(240 * mapScale);
            
            // Minimap is positioned in top-right corner with some padding
            float mapX = Main.screenWidth - mapWidth - 10;
            float mapY = 10;
            
            // Calculate relative position from player
            Vector2 relativePos = worldPosition - playerCenter;
            
            // Scale down to map coordinates (minimap shows a certain radius around player)
            float minimapRange = 2000f / mapScale; // Approximate range shown on minimap
            Vector2 mapOffset = relativePos / minimapRange * (mapWidth / 2f);
            
            // Center on minimap
            Vector2 mapCenter = new Vector2(mapX + mapWidth / 2f, mapY + mapHeight / 2f);
            Vector2 iconPos = mapCenter + mapOffset;
            
            // Clamp to minimap bounds
            iconPos.X = MathHelper.Clamp(iconPos.X, mapX, mapX + mapWidth);
            iconPos.Y = MathHelper.Clamp(iconPos.Y, mapY, mapY + mapHeight);
            
            // Only return if the miniboss is within reasonable range to show
            if (Math.Abs(mapOffset.X) > mapWidth / 2f * 1.5f || Math.Abs(mapOffset.Y) > mapHeight / 2f * 1.5f)
            {
                // Show on edge pointing toward miniboss direction
                Vector2 dir = relativePos.SafeNormalize(Vector2.UnitX);
                iconPos = mapCenter + dir * Math.Min(mapWidth, mapHeight) * 0.4f;
            }
            
            return iconPos;
        }
        
        private Vector2 GetFullMapPosition(Vector2 worldPosition)
        {
            // Full map overlay uses screen-space coordinates based on world position
            float mapScale = Main.mapFullscreenScale;
            
            // Convert world position to map position
            float mapX = (worldPosition.X / 16f - Main.mapFullscreenPos.X) * mapScale + Main.screenWidth / 2f;
            float mapY = (worldPosition.Y / 16f - Main.mapFullscreenPos.Y) * mapScale + Main.screenHeight / 2f;
            
            // Only show if on screen
            if (mapX < 0 || mapX > Main.screenWidth || mapY < 0 || mapY > Main.screenHeight)
            {
                // Edge indicator - find intersection with screen edge
                Vector2 screenCenter = new Vector2(Main.screenWidth / 2f, Main.screenHeight / 2f);
                Vector2 direction = new Vector2(mapX, mapY) - screenCenter;
                direction.Normalize();
                
                float edgeX = direction.X > 0 ? Main.screenWidth - 30 : 30;
                float edgeY = direction.Y > 0 ? Main.screenHeight - 30 : 30;
                
                float tX = (edgeX - screenCenter.X) / direction.X;
                float tY = (edgeY - screenCenter.Y) / direction.Y;
                float t = Math.Min(Math.Abs(tX), Math.Abs(tY));
                
                return screenCenter + direction * t;
            }
            
            return new Vector2(mapX, mapY);
        }
    }
}
