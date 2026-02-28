using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.IridescentWingspan.Shaders
{
    [Autoload(Side = ModSide.Client)]
    public class WingspanShaderLoader : ModSystem
    {
        public static bool HasEtherealWingShader { get; private set; }
        public static bool HasWingspanFlareTrailShader { get; private set; }

        public override void PostSetupContent()
        {
            if (TryLoadMiscShader("Effects/SwanLake/IridescentWingspan/EtherealWing",
                "EtherealWingMain", "MagnumOpus:EtherealWing"))
                HasEtherealWingShader = true;

            if (TryLoadMiscShader("Effects/SwanLake/IridescentWingspan/WingspanFlareTrail",
                "WingspanFlareMain", "MagnumOpus:WingspanFlareTrail"))
                HasWingspanFlareTrailShader = true;
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
            HasEtherealWingShader = false;
            HasWingspanFlareTrailShader = false;
        }
    }
}
