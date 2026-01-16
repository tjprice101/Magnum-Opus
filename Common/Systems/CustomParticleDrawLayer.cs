using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;

namespace MagnumOpus.Common.Systems
{
    /// <summary>
    /// Handles drawing custom particles and screen effects at the appropriate layer.
    /// Uses proper blending for consistent particle rendering.
    /// </summary>
    public class CustomParticleDrawLayer : ModSystem
    {
        public override void PostDrawTiles()
        {
            // Don't draw if no particles exist
            if (!CustomParticleSystem.TexturesLoaded) return;
            
            // Draw custom particles above tiles but below entities
            // DrawAllParticles handles its own blend state transitions internally
            // We start with AlphaBlend as the base state
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            CustomParticleSystem.DrawAllParticles(Main.spriteBatch);
            
            // Draw Fate reality distortion effects
            FateRealityDistortion.DrawSliceEffect(Main.spriteBatch);
            
            Main.spriteBatch.End();
        }
    }
    
    /// <summary>
    /// Handles drawing screen-wide distortion effects as a UI overlay.
    /// These are drawn last to affect the entire screen.
    /// </summary>
    public class ScreenDistortionDrawLayer : ModSystem
    {
        public override void ModifyInterfaceLayers(System.Collections.Generic.List<GameInterfaceLayer> layers)
        {
            // Find the layer index for Resource Bars (good spot to insert before)
            int index = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars"));
            if (index != -1)
            {
                layers.Insert(index, new LegacyGameInterfaceLayer(
                    "MagnumOpus: Screen Distortions",
                    delegate
                    {
                        DrawScreenDistortions();
                        return true;
                    },
                    InterfaceScaleType.UI));
            }
        }
        
        private void DrawScreenDistortions()
        {
            if (!FateRealityDistortion.AnyDistortionActive())
                return;
            
            SpriteBatch spriteBatch = Main.spriteBatch;
            
            // Draw chromatic aberration overlay
            Vector2 chromaOffset = FateRealityDistortion.GetChromaticOffset();
            if (chromaOffset != Vector2.Zero)
            {
                DrawChromaticAberrationOverlay(spriteBatch, chromaOffset);
            }
            
            // Draw inversion effect
            float inversionIntensity = FateRealityDistortion.GetInversionIntensity();
            if (inversionIntensity > 0f)
            {
                DrawInversionOverlay(spriteBatch, inversionIntensity);
            }
        }
        
        private void DrawChromaticAberrationOverlay(SpriteBatch spriteBatch, Vector2 offset)
        {
            // Draw subtle colored overlays offset from screen center to simulate chromatic aberration
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Rectangle screenRect = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);
            
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);
            
            // Red channel offset left
            spriteBatch.Draw(pixel, new Rectangle((int)-offset.X, 0, Main.screenWidth, Main.screenHeight), 
                Color.Red * 0.05f * (offset.X / 10f));
            
            // Cyan channel offset right
            spriteBatch.Draw(pixel, new Rectangle((int)offset.X, 0, Main.screenWidth, Main.screenHeight), 
                Color.Cyan * 0.05f * (offset.X / 10f));
            
            spriteBatch.End();
        }
        
        private void DrawInversionOverlay(SpriteBatch spriteBatch, float intensity)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            
            // Draw a white overlay that simulates color inversion
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);
            spriteBatch.Draw(pixel, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), 
                Color.White * intensity);
            spriteBatch.End();
        }
    }
}
