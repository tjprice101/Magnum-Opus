using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.ChromaticSwanSong.Shaders
{
    /// <summary>
    /// Loads and registers all shaders for Chromatic Swan Song.
    /// 
    /// Shader keys:
    ///   • MagnumOpus:ChromaticTrail    — Rainbow-shifting bolt trail
    ///   • MagnumOpus:AriaExplosion     — Chromatic aria detonation burst
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public class ChromaticShaderLoader : ModSystem
    {
        public static bool HasChromaticTrailShader { get; private set; }
        public static bool HasAriaExplosionShader { get; private set; }

        public override void PostSetupContent()
        {
            if (TryLoadMiscShader("Effects/SwanLake/ChromaticSwanSong/ChromaticTrail",
                "ChromaticTrailMain", "MagnumOpus:ChromaticTrail"))
                HasChromaticTrailShader = true;

            if (TryLoadMiscShader("Effects/SwanLake/ChromaticSwanSong/AriaExplosion",
                "AriaExplosionMain", "MagnumOpus:AriaExplosion"))
                HasAriaExplosionShader = true;
        }

        private static bool TryLoadMiscShader(string effectPath, string passName, string key)
        {
            try
            {
                var effect = ModContent.Request<Effect>(effectPath, AssetRequestMode.ImmediateLoad);
                if (effect?.Value != null)
                {
                    GameShaders.Misc[key] = new MiscShaderData(effect, passName);
                    return true;
                }
            }
            catch { }
            return false;
        }

        public override void Unload()
        {
            HasChromaticTrailShader = false;
            HasAriaExplosionShader = false;
        }
    }
}
