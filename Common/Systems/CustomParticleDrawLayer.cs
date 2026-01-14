using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems
{
    /// <summary>
    /// Handles drawing custom particles at the appropriate layer.
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
            
            Main.spriteBatch.End();
        }
    }
}
