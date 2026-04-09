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
        public static ModKeybind DashKeybind { get; private set; }
        public static ModKeybind TeleportKeybind { get; private set; }
        public static ModKeybind WingAmplifyKeybind { get; private set; }

        public override void Load()
        {
            // Initialize the interpolated renderer for buttery-smooth 144Hz+ animations
            InterpolatedRenderer.Initialize();

            DashKeybind = KeybindLoader.RegisterKeybind(this, "Momentum Dash", "Q");
            TeleportKeybind = KeybindLoader.RegisterKeybind(this, "Phase Shift", "F");
            WingAmplifyKeybind = KeybindLoader.RegisterKeybind(this, "Wing HP Amplification", "K");
        }
        
        public override void Unload()
        {
            // Shutdown the interpolated renderer
            InterpolatedRenderer.Shutdown();
            
            // Clean up smear textures
            SmearTextureGenerator.Unload();

            DashKeybind = null;
            TeleportKeybind = null;
            WingAmplifyKeybind = null;
        }
    }
}
