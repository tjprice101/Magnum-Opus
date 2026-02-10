using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.Graphics;
using MagnumOpus.Content.Eroica.Bosses;

namespace MagnumOpus.Common.Systems
{
    /// <summary>
    /// Handles visual effects during the Eroica boss fight.
    /// Adds scattered black, gold, and scarlet red particles.
    /// </summary>
    public class EroicaBossFightVisuals : ModSystem
    {
        private static bool eroicaBossActive = false;
        private static float overlayIntensity = 0f;
        private const float MaxIntensity = 0.25f; // Reduced overlay intensity
        private const float FadeSpeed = 0.02f;

        public override void PostUpdateNPCs()
        {
            // Check if Eroica, God of Valor is active
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
                    
                // Spawn scattered black, gold, and scarlet particles across the screen
                if (!Main.dedServ && Main.rand.NextBool(5))
                {
                    // Spawn across the visible screen area
                    Vector2 spawnPos = Main.screenPosition + new Vector2(
                        Main.rand.NextFloat(0f, Main.screenWidth),
                        Main.rand.NextFloat(-50f, Main.screenHeight * 0.4f));
                    
                    // Randomly choose between black, gold, and scarlet red particles
                    int dustChoice = Main.rand.Next(3);
                    int dustType;
                    Color dustColor = default;
                    
                    if (dustChoice == 0)
                    {
                        // Black/dark smoke
                        dustType = Terraria.ID.DustID.Smoke;
                        dustColor = Color.Black;
                    }
                    else if (dustChoice == 1)
                    {
                        // Gold flame
                        dustType = Terraria.ID.DustID.GoldFlame;
                    }
                    else
                    {
                        // Scarlet red/deep pink - use torch and tint it
                        dustType = Terraria.ID.DustID.Torch;
                        dustColor = new Color(220, 20, 60); // Crimson/scarlet
                    }
                    
                    Dust dust = Dust.NewDustPerfect(spawnPos, dustType, 
                        new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(0.5f, 2f)), 
                        100, dustColor, Main.rand.NextFloat(1.0f, 1.6f));
                    dust.noGravity = true;
                    dust.fadeIn = 1.2f;
                }
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
                scale *= 1f - (overlayIntensity * 0.2f);
            }
        }

        public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)
        {
            if (overlayIntensity > 0f)
            {
                // Tint the world with a dark scarlet/gold hue instead of pink
                Color scarletTint = new Color(80, 30, 30); // Dark scarlet
                
                tileColor = Color.Lerp(tileColor, scarletTint, overlayIntensity * 0.4f);
                backgroundColor = Color.Lerp(backgroundColor, scarletTint, overlayIntensity * 0.5f);
            }
        }

        public override void PreDrawMapIconOverlay(IReadOnlyList<IMapLayer> layers, MapOverlayDrawContext mapOverlayDrawContext)
        {
            // Empty - required override
        }

        // Removed ModifyTransformMatrix overlay - fullscreen overlays in this hook can interfere with player rendering
        // The EroicaSkyEffect handles the atmospheric visuals properly through CustomSky

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
