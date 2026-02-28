using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Weapons.BlossomOfTheSakura.Shaders
{
    public class BlossomShaderLoader : ModSystem
    {
        public const string TracerTrailMainKey = "MagnumOpus:TracerTrailMain";
        public const string TracerTrailGlowKey = "MagnumOpus:TracerTrailGlow";
        public const string HeatShimmerMainKey = "MagnumOpus:HeatShimmerMain";
        public const string SakuraPetalBloomKey = "MagnumOpus:BlossomSakuraPetalBloom";
        public const string SakuraGlowPassKey = "MagnumOpus:BlossomSakuraGlowPass";

        public override void PostSetupContent()
        {
            if (Main.dedServ) return;

            var tracerTrail = ModContent.Request<Effect>("MagnumOpus/Effects/Eroica/BlossomOfTheSakura/TracerTrail", AssetRequestMode.ImmediateLoad);
            GameShaders.Misc[TracerTrailMainKey] = new MiscShaderData(tracerTrail, "TracerTrailMain");
            GameShaders.Misc[TracerTrailGlowKey] = new MiscShaderData(tracerTrail, "TracerTrailGlow");

            var heatDistortion = ModContent.Request<Effect>("MagnumOpus/Effects/Eroica/BlossomOfTheSakura/HeatDistortion", AssetRequestMode.ImmediateLoad);
            GameShaders.Misc[HeatShimmerMainKey] = new MiscShaderData(heatDistortion, "HeatShimmerMain");

            var sakuraBloom = ModContent.Request<Effect>("MagnumOpus/Effects/Eroica/SakuraBloom", AssetRequestMode.ImmediateLoad);
            GameShaders.Misc[SakuraPetalBloomKey] = new MiscShaderData(sakuraBloom, "SakuraPetalBloom");
            GameShaders.Misc[SakuraGlowPassKey] = new MiscShaderData(sakuraBloom, "SakuraGlowPass");
        }
    }
}
