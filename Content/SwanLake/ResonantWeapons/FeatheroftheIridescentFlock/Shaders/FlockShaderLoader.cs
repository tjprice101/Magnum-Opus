using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.FeatheroftheIridescentFlock.Shaders
{
    /// <summary>
    /// Loads shaders for Feather of the Iridescent Flock.
    /// 
    /// Shader keys:
    ///   • MagnumOpus:CrystalOrbitTrail — Oil-sheen trail for orbiting crystal minions
    ///   • MagnumOpus:FlockAura        — Flock formation aura when 3+ crystals active
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public class FlockShaderLoader : ModSystem
    {
        public static bool HasCrystalOrbitTrailShader { get; private set; }
        public static bool HasFlockAuraShader { get; private set; }

        public override void PostSetupContent()
        {
            if (TryLoadMiscShader("MagnumOpus/Effects/SwanLake/FeatheroftheIridescentFlock/CrystalOrbitTrail",
                "P0", "MagnumOpus:CrystalOrbitTrail"))
                HasCrystalOrbitTrailShader = true;

            if (TryLoadMiscShader("MagnumOpus/Effects/SwanLake/FeatheroftheIridescentFlock/FlockAura",
                "P0", "MagnumOpus:FlockAura"))
                HasFlockAuraShader = true;
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
            HasCrystalOrbitTrailShader = false;
            HasFlockAuraShader = false;
        }
    }
}
