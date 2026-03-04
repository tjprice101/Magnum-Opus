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

            HasRocketTrailShader = TryLoadShader(ShaderBasePath + "RocketTrailShader", "SymphonicRocketTrail", FallbackTrail);
            HasCrescendoShader = TryLoadShader(ShaderBasePath + "CrescendoShader", "SymphonicCrescendo", FallbackBloom);
            HasExplosionShader = TryLoadShader(ShaderBasePath + "ExplosionShader", "SymphonicExplosion", FallbackBloom);
        }

        private bool TryLoadShader(string path, string key, string fallbackPath)
        {
            try
            {
                var effect = ModContent.Request<Microsoft.Xna.Framework.Graphics.Effect>(path, AssetRequestMode.ImmediateLoad).Value;
                GameShaders.Misc[key] = new MiscShaderData(new Terraria.Ref<Microsoft.Xna.Framework.Graphics.Effect>(effect), "P0");
                return true;
            }
            catch
            {
                try
                {
                    var fallback = ModContent.Request<Microsoft.Xna.Framework.Graphics.Effect>(fallbackPath, AssetRequestMode.ImmediateLoad).Value;
                    GameShaders.Misc[key] = new MiscShaderData(new Terraria.Ref<Microsoft.Xna.Framework.Graphics.Effect>(fallback), "P0");
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
