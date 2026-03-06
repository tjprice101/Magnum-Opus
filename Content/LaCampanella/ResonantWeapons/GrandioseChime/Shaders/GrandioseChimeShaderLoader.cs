using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;
using ReLogic.Content;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.GrandioseChime.Shaders
{
    [Autoload(Side = ModSide.Client)]
    public class GrandioseChimeShaderLoader : ModSystem
    {
        public static bool HasBeamShader { get; private set; }
        public static bool HasBarrageShader { get; private set; }
        public static bool HasMineShader { get; private set; }

        private const string ShaderBasePath = "MagnumOpus/Effects/LaCampanella/GrandioseChime/";
        private const string FallbackBeam = "MagnumOpus/Effects/ScrollingTrailShader";
        private const string FallbackBloom = "MagnumOpus/Effects/SimpleBloomShader";

        public override void PostSetupContent()
        {
            if (Main.dedServ) return;

            HasBeamShader = TryLoadShader(ShaderBasePath + "GrandioseBeamShader", "GrandioseChimeBeam", "P0", FallbackBeam, "Pass0");
            HasBarrageShader = TryLoadShader(ShaderBasePath + "BarrageShader", "GrandioseChimeBarrage", "P0", FallbackBloom, "DefaultPass");
            HasMineShader = TryLoadShader(ShaderBasePath + "MineShader", "GrandioseChimeMine", "P0", FallbackBloom, "DefaultPass");
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

        public static MiscShaderData GetBeamShader()
        {
            if (!HasBeamShader) return null;
            try
            {
                return GameShaders.Misc["GrandioseChimeBeam"];
            }
            catch
            {
                HasBeamShader = false;
                return null;
            }
        }

        public static MiscShaderData GetBarrageShader()
        {
            if (!HasBarrageShader) return null;
            try
            {
                return GameShaders.Misc["GrandioseChimeBarrage"];
            }
            catch
            {
                HasBarrageShader = false;
                return null;
            }
        }

        public static MiscShaderData GetMineShader()
        {
            if (!HasMineShader) return null;
            try
            {
                return GameShaders.Misc["GrandioseChimeMine"];
            }
            catch
            {
                HasMineShader = false;
                return null;
            }
        }
    }
}
