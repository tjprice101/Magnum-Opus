using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Weapons.PiercingLightOfTheSakura.Shaders
{
    public class PiercingShaderLoader : ModSystem
    {
        public const string LightningTrailMainKey = "MagnumOpus:LightningTrailMain";
        public const string LightningTrailGlowKey = "MagnumOpus:LightningTrailGlow";
        public const string CrescendoChargeMainKey = "MagnumOpus:CrescendoChargeMain";
        public const string CrescendoChargeGlowKey = "MagnumOpus:CrescendoChargeGlow";
        public const string SakuraPetalBloomKey = "MagnumOpus:PiercingSakuraPetalBloom";
        public const string SakuraGlowPassKey = "MagnumOpus:PiercingSakuraGlowPass";

        public override void PostSetupContent()
        {
            if (Main.dedServ) return;

            var lightningTrail = ModContent.Request<Effect>(
                "MagnumOpus/Effects/Eroica/PiercingLightOfTheSakura/SakuraLightningTrail",
                AssetRequestMode.ImmediateLoad);
            GameShaders.Misc[LightningTrailMainKey] = new MiscShaderData(lightningTrail, "LightningTrailMain");
            GameShaders.Misc[LightningTrailGlowKey] = new MiscShaderData(lightningTrail, "LightningTrailGlow");

            var crescendoCharge = ModContent.Request<Effect>(
                "MagnumOpus/Effects/Eroica/PiercingLightOfTheSakura/CrescendoCharge",
                AssetRequestMode.ImmediateLoad);
            GameShaders.Misc[CrescendoChargeMainKey] = new MiscShaderData(crescendoCharge, "CrescendoChargeMain");
            GameShaders.Misc[CrescendoChargeGlowKey] = new MiscShaderData(crescendoCharge, "CrescendoChargeGlow");

            var sakuraBloom = ModContent.Request<Effect>(
                "MagnumOpus/Effects/Eroica/SakuraBloom",
                AssetRequestMode.ImmediateLoad);
            GameShaders.Misc[SakuraPetalBloomKey] = new MiscShaderData(sakuraBloom, "SakuraPetalBloom");
            GameShaders.Misc[SakuraGlowPassKey] = new MiscShaderData(sakuraBloom, "SakuraGlowPass");
        }
    }
}
