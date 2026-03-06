using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;
using ReLogic.Content;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.SymphonicBellfireAnnihilator.Shaders
{
    [Autoload(Side = ModSide.Client)]
    public class SymphonicBellfireShaderLoader : ModSystem
    {
        public static bool HasRocketTrailShader { get; private set; }
        public static bool HasCrescendoShader { get; private set; }
        public static bool HasExplosionShader { get; private set; }

        private const string ShaderBasePath = "MagnumOpus/Effects/LaCampanella/SymphonicBellfireAnnihilator/";
        private const string FallbackTrail = "MagnumOpus/Effects/ScrollingTrailShader";
        private const string FallbackBloom = "MagnumOpus/Effects/SimpleBloomShader";

        public override void PostSetupContent()
        {
            if (Main.dedServ) return;

            HasRocketTrailShader = TryLoadShader(ShaderBasePath + "RocketTrailShader", "SymphonicRocketTrail", "P0", FallbackTrail, "Pass0");
            HasCrescendoShader = TryLoadShader(ShaderBasePath + "CrescendoShader", "SymphonicCrescendo", "P0", FallbackBloom, "DefaultPass");
            HasExplosionShader = TryLoadShader(ShaderBasePath + "ExplosionShader", "SymphonicExplosion", "P0", FallbackBloom, "DefaultPass");
        }

        private bool TryLoadShader(string path, string key, string technique, string fallbackPath, string fallbackTechnique)
        {
            try
            {
                var effect = ModContent.Request<Microsoft.Xna.Framework.Graphics.Effect>(path, AssetRequestMode.ImmediateLoad);
                GameShaders.Misc[key] = new MiscShaderData(effect, technique);
                return true;
            }
            catch
            {
                try
                {
                    var fallback = ModContent.Request<Microsoft.Xna.Framework.Graphics.Effect>(fallbackPath, AssetRequestMode.ImmediateLoad);
                    GameShaders.Misc[key] = new MiscShaderData(fallback, fallbackTechnique);
                    return true;
                }
                catch { }
                return false;
            }
        }

        public static MiscShaderData GetRocketTrailShader()
        {
            if (!HasRocketTrailShader) return null;
            try
            {
                return GameShaders.Misc["SymphonicRocketTrail"];
            }
            catch
            {
                HasRocketTrailShader = false;
                return null;
            }
        }

        public static MiscShaderData GetCrescendoShader()
        {
            if (!HasCrescendoShader) return null;
            try
            {
                return GameShaders.Misc["SymphonicCrescendo"];
            }
            catch
            {
                HasCrescendoShader = false;
                return null;
            }
        }

        public static MiscShaderData GetExplosionShader()
        {
            if (!HasExplosionShader) return null;
            try
            {
                return GameShaders.Misc["SymphonicExplosion"];
            }
            catch
            {
                HasExplosionShader = false;
                return null;
            }
        }
    }
}
