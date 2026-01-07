using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.UI;

namespace MagnumOpus.Common.Systems
{
    /// <summary>
    /// Handles the shimmering music note map marker for the Grand Piano room.
    /// Also spawns ambient particles around the piano room location.
    /// </summary>
    public class PianoRoomMapMarker : ModSystem
    {
        private float shimmerTimer = 0f;

        public override void PostUpdateWorld()
        {
            shimmerTimer += 0.02f;
            
            // Spawn ambient particles near the piano room if player is close
            if (MoonlightSonataSystem.PianoRoomCenter != Vector2.Zero && MoonlightSonataSystem.MoonLordKilledOnce)
            {
                Vector2 roomCenter = MoonlightSonataSystem.PianoRoomCenter;
                
                // Only spawn particles if a player is within range
                foreach (Player player in Main.player)
                {
                    if (player.active && !player.dead)
                    {
                        float distance = Vector2.Distance(player.Center, roomCenter);
                        
                        // Spawn golden sparkles near the room
                        if (distance < 1500f && Main.rand.NextBool(distance < 500f ? 3 : 10))
                        {
                            Vector2 dustPos = roomCenter + Main.rand.NextVector2Circular(200f, 150f);
                            Dust sparkle = Dust.NewDustDirect(dustPos, 1, 1, DustID.GoldCoin, 0f, -0.5f, 150, default, 1.2f);
                            sparkle.noGravity = true;
                            sparkle.velocity *= 0.3f;
                        }
                        
                        // Musical note particles floating up
                        if (distance < 800f && Main.rand.NextBool(20))
                        {
                            Vector2 notePos = roomCenter + Main.rand.NextVector2Circular(100f, 50f);
                            Dust note = Dust.NewDustDirect(notePos, 1, 1, DustID.Enchanted_Gold, 0f, -1f, 0, default, 1.5f);
                            note.noGravity = true;
                            note.velocity.Y = -2f;
                            note.velocity.X = Main.rand.NextFloat(-0.5f, 0.5f);
                        }
                        
                        break; // Only need to check once
                    }
                }
            }
        }

        public override void PostDrawFullscreenMap(ref string mouseText)
        {
            // Draw the shimmering music note on the fullscreen map
            if (MoonlightSonataSystem.PianoRoomCenter == Vector2.Zero || !MoonlightSonataSystem.MoonLordKilledOnce)
                return;
            
            // Get map position
            Vector2 worldPos = MoonlightSonataSystem.PianoRoomCenter;
            
            // Convert world position to map position
            float mapScale = Main.mapFullscreenScale;
            Vector2 mapOffset = new Vector2(Main.mapFullscreenPos.X, Main.mapFullscreenPos.Y);
            
            Vector2 mapPos = (worldPos / 16f - mapOffset) * mapScale;
            mapPos += new Vector2(Main.screenWidth / 2f, Main.screenHeight / 2f);

            // Check if on screen
            if (mapPos.X < 0 || mapPos.X > Main.screenWidth || mapPos.Y < 0 || mapPos.Y > Main.screenHeight)
                return;

            // Shimmer effect
            float shimmer = 0.7f + (float)Math.Sin(shimmerTimer * 3f) * 0.3f;
            float pulse = 1f + (float)Math.Sin(shimmerTimer * 2f) * 0.15f;
            
            // Draw glowing background
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;
            
            // Outer glow
            Color glowColor = new Color(255, 215, 0) * (shimmer * 0.3f);
            float glowSize = 12f * pulse;
            spriteBatch.Draw(pixel, mapPos, new Rectangle(0, 0, 1, 1), glowColor, 0f, 
                new Vector2(0.5f, 0.5f), new Vector2(glowSize, glowSize), SpriteEffects.None, 0f);
            
            // Inner glow
            Color innerColor = new Color(255, 230, 100) * (shimmer * 0.5f);
            float innerSize = 6f * pulse;
            spriteBatch.Draw(pixel, mapPos, new Rectangle(0, 0, 1, 1), innerColor, 0f,
                new Vector2(0.5f, 0.5f), new Vector2(innerSize, innerSize), SpriteEffects.None, 0f);

            // Draw music note symbol using simple shapes
            DrawMusicNote(spriteBatch, mapPos, shimmer, pulse);

            // Show tooltip when hovering
            Rectangle markerRect = new Rectangle((int)(mapPos.X - 10), (int)(mapPos.Y - 10), 20, 20);
            if (markerRect.Contains(Main.mouseX, Main.mouseY))
            {
                mouseText = "Grand Piano Chamber";
            }
        }

        private void DrawMusicNote(SpriteBatch spriteBatch, Vector2 pos, float shimmer, float pulse)
        {
            Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;
            Color noteColor = new Color(255, 215, 0) * shimmer;
            
            // Draw a simple music note shape using rectangles
            // Note head (filled oval approximation)
            float noteScale = pulse * 0.8f;
            Vector2 headPos = pos + new Vector2(-2f, 2f) * noteScale;
            spriteBatch.Draw(pixel, headPos, new Rectangle(0, 0, 1, 1), noteColor, 0.5f,
                new Vector2(0.5f, 0.5f), new Vector2(4f, 3f) * noteScale, SpriteEffects.None, 0f);
            
            // Note stem (vertical line)
            Vector2 stemBottom = pos + new Vector2(0f, 0f) * noteScale;
            spriteBatch.Draw(pixel, stemBottom, new Rectangle(0, 0, 1, 1), noteColor, 0f,
                new Vector2(0.5f, 1f), new Vector2(1.5f, 8f) * noteScale, SpriteEffects.None, 0f);
            
            // Note flag (curved line approximation)
            Vector2 flagPos = pos + new Vector2(1f, -6f) * noteScale;
            spriteBatch.Draw(pixel, flagPos, new Rectangle(0, 0, 1, 1), noteColor, 0.3f,
                new Vector2(0f, 0.5f), new Vector2(3f, 1.5f) * noteScale, SpriteEffects.None, 0f);
        }
    }
}
