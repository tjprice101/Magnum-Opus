using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Weapons.SakurasBlossom.Shaders
{
    public class SakuraShaderLoader : ModSystem
    {
        public const string SakuraTrailFlowKey = "MagnumOpus:SakuraTrailFlow";
        public const string SakuraTrailGlowKey = "MagnumOpus:SakuraTrailGlow";
        public const string PetalDissolveMainKey = "MagnumOpus:PetalDissolveMain";
        public const string PetalDissolveGlowKey = "MagnumOpus:PetalDissolveGlow";
        public const string SakuraPetalBloomKey = "MagnumOpus:SakuraPetalBloom";
        public const string SakuraGlowPassKey = "MagnumOpus:SakuraGlowPass";

        public override void PostSetupContent()
        {
            if (Main.dedServ) return;

            var swingTrail = ModContent.Request<Effect>("MagnumOpus/Effects/Eroica/SakurasBlossom/SakuraSwingTrail", AssetRequestMode.ImmediateLoad);
            GameShaders.Misc[SakuraTrailFlowKey] = new MiscShaderData(swingTrail, "SakuraTrailFlow");
            GameShaders.Misc[SakuraTrailGlowKey] = new MiscShaderData(swingTrail, "SakuraTrailGlow");

            var petalDissolve = ModContent.Request<Effect>("MagnumOpus/Effects/Eroica/SakurasBlossom/PetalDissolve", AssetRequestMode.ImmediateLoad);
            GameShaders.Misc[PetalDissolveMainKey] = new MiscShaderData(petalDissolve, "PetalDissolveMain");
            GameShaders.Misc[PetalDissolveGlowKey] = new MiscShaderData(petalDissolve, "PetalDissolveGlow");

            var sakuraBloom = ModContent.Request<Effect>("MagnumOpus/Effects/Eroica/SakuraBloom", AssetRequestMode.ImmediateLoad);
            GameShaders.Misc[SakuraPetalBloomKey] = new MiscShaderData(sakuraBloom, "SakuraPetalBloom");
            GameShaders.Misc[SakuraGlowPassKey] = new MiscShaderData(sakuraBloom, "SakuraGlowPass");
        }
    }
}
