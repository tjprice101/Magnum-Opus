using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems
{
    /// <summary>
    /// Handles drawing custom particles at the appropriate layer.
    /// </summary>
    public class CustomParticleDrawLayer : ModSystem
    {
        public override void PostDrawTiles()
        {
            // Draw custom particles above tiles but below entities
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            CustomParticleSystem.DrawAllParticles(Main.spriteBatch);
            
            Main.spriteBatch.End();
        }
    }
}
