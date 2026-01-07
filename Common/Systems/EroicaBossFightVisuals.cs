using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.Eroica.Bosses;

namespace MagnumOpus.Common.Systems
{
    /// <summary>
    /// Handles visual effects during the Eroica boss fight.
    /// Adds a dark pink hue overlay and cherry blossom effects.
    /// </summary>
    public class EroicaBossFightVisuals : ModSystem
    {
        private static bool eroicaBossActive = false;
        private static float overlayIntensity = 0f;
        private const float MaxIntensity = 0.55f; // How strong the pink overlay is (increased)
        private const float FadeSpeed = 0.02f;

        public override void PostUpdateNPCs()
        {
            // Check if Eroica's Retribution is active
            bool bossFound = false;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].active && Main.npc[i].type == ModContent.NPCType<EroicasRetribution>())
                {
                    bossFound = true;
                    break;
                }
            }

            eroicaBossActive = bossFound;

            // Fade overlay in/out
            if (eroicaBossActive)
            {
                if (overlayIntensity < MaxIntensity)
                    overlayIntensity += FadeSpeed;
                if (overlayIntensity > MaxIntensity)
                    overlayIntensity = MaxIntensity;
            }
            else
            {
                if (overlayIntensity > 0f)
                    overlayIntensity -= FadeSpeed;
                if (overlayIntensity < 0f)
                    overlayIntensity = 0f;
            }
        }

        public override void ModifyLightingBrightness(ref float scale)
        {
            // Slightly dim the world during the fight
            if (overlayIntensity > 0f)
            {
                scale *= 1f - (overlayIntensity * 0.3f);
            }
        }

        public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)
        {
            if (overlayIntensity > 0f)
            {
                // Tint the world with a dark pink hue
                Color pinkTint = new Color(200, 80, 120); // Deeper dark pink
                
                tileColor = Color.Lerp(tileColor, pinkTint, overlayIntensity * 0.6f);
                backgroundColor = Color.Lerp(backgroundColor, pinkTint, overlayIntensity * 0.85f); // Much stronger on background
            }
        }

        public override void PostDrawTiles()
        {
            if (overlayIntensity <= 0f)
                return;

            // Only draw on client
            if (Main.dedServ)
                return;

            // Draw the pink overlay on top of tiles but before entities
            SpriteBatch spriteBatch = Main.spriteBatch;
            
            // Null check for texture
            if (Terraria.GameContent.TextureAssets.MagicPixel == null || !Terraria.GameContent.TextureAssets.MagicPixel.IsLoaded)
                return;

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

            // Dark pink overlay color
            Color overlayColor = new Color(139, 58, 98, (int)(overlayIntensity * 180)); // Stronger transparency
            
            // Draw a full-screen rectangle
            Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;
            Rectangle screenRect = new Rectangle(
                (int)Main.screenPosition.X - 100,
                (int)Main.screenPosition.Y - 100,
                Main.screenWidth + 200,
                Main.screenHeight + 200
            );

            spriteBatch.Draw(pixel, screenRect, overlayColor);

            spriteBatch.End();
        }

        /// <summary>
        /// Static method to check if the boss fight visuals are active.
        /// Can be used by other systems to sync effects.
        /// </summary>
        public static bool IsEroicaFightActive() => eroicaBossActive;
        
        /// <summary>
        /// Gets the current overlay intensity for other effects to sync with.
        /// </summary>
        public static float GetOverlayIntensity() => overlayIntensity;
    }
}
