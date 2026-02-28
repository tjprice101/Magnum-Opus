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

            HasBeamShader = TryLoadShader(ShaderBasePath + "GrandioseBeamShader", "GrandioseChimeBeam", FallbackBeam);
            HasBarrageShader = TryLoadShader(ShaderBasePath + "BarrageShader", "GrandioseChimeBarrage", FallbackBloom);
            HasMineShader = TryLoadShader(ShaderBasePath + "MineShader", "GrandioseChimeMine", FallbackBloom);
        }

        private bool TryLoadShader(string path, string key, string fallbackPath)
        {
            try
            {
                var effect = ModContent.Request<Microsoft.Xna.Framework.Graphics.Effect>(path, AssetRequestMode.ImmediateLoad).Value;
                GameShaders.Misc[key] = new MiscShaderData(new Terraria.Ref<Microsoft.Xna.Framework.Graphics.Effect>(effect), "AutoPass");
                return true;
            }
            catch
            {
                try
                {
                    var fallback = ModContent.Request<Microsoft.Xna.Framework.Graphics.Effect>(fallbackPath, AssetRequestMode.ImmediateLoad).Value;
                    GameShaders.Misc[key] = new MiscShaderData(new Terraria.Ref<Microsoft.Xna.Framework.Graphics.Effect>(fallback), "AutoPass");
                }
                catch { }
                return false;
            }
        }
    }
}
