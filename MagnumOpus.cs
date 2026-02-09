using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus
{
    /// <summary>
    /// MagnumOpus - A music-themed Terraria mod featuring symphonic weapons,
    /// thematic bosses, and spectacular visual effects.
    /// </summary>
    public class MagnumOpus : Mod
    {
        public override void Load()
        {
            // Initialize the interpolated renderer for buttery-smooth 144Hz+ animations
            InterpolatedRenderer.Initialize();
        }
        
        public override void Unload()
        {
            // Shutdown the interpolated renderer
            InterpolatedRenderer.Shutdown();
            
            // Clean up smear textures
            SmearTextureGenerator.Unload();
        }
    }
}
