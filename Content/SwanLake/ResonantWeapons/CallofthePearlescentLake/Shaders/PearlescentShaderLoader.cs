using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.CallofthePearlescentLake.Shaders
{
    /// <summary>
    /// Loads and registers all shaders for Call of the Pearlescent Lake.
    /// Self-contained — no shared system crossover.
    /// 
    /// Shader keys:
    ///   • MagnumOpus:PearlescentRocketTrail — Opal-shimmer rocket trail (3-pass: bloom → core → halo)
    ///   • MagnumOpus:LakeExplosion         — Concentric water-ripple explosion
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public class PearlescentShaderLoader : ModSystem
    {
        public static bool HasRocketTrailShader { get; private set; }
        public static bool HasLakeExplosionShader { get; private set; }

        public override void PostSetupContent()
        {
            if (TryLoadMiscShader("MagnumOpus/Effects/SwanLake/CallofthePearlescentLake/PearlescentRocketTrail",
                "P0", "MagnumOpus:PearlescentRocketTrail"))
                HasRocketTrailShader = true;

            if (TryLoadMiscShader("MagnumOpus/Effects/SwanLake/CallofthePearlescentLake/LakeExplosion",
                "P0", "MagnumOpus:LakeExplosion"))
                HasLakeExplosionShader = true;
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
            HasRocketTrailShader = false;
            HasLakeExplosionShader = false;
        }
    }
}
