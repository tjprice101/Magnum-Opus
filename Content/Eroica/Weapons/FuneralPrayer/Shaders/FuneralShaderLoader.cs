using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Weapons.FuneralPrayer.Shaders
{
    public class FuneralShaderLoader : ModSystem
    {
        // EroicaFuneralTrail.fx passes
        public const string FuneralFlameFlowKey = "MagnumOpus:FuneralFlameFlow";
        public const string FuneralGlowPassKey = "MagnumOpus:FuneralGlowPass";

        // PrayerConvergence.fx passes
        public const string ConvergenceMainKey = "MagnumOpus:ConvergenceMain";
        public const string ConvergenceGlowKey = "MagnumOpus:ConvergenceGlow";

        // RequiemBeam.fx passes
        public const string RequiemBeamMainKey = "MagnumOpus:RequiemBeamMain";
        public const string RequiemBeamGlowKey = "MagnumOpus:RequiemBeamGlow";

        // Shared HeroicFlameTrail.fx passes (scoped to Funeral to avoid key collisions)
        public const string HeroicFlameFlowKey = "MagnumOpus:FuneralHeroicFlameFlow";
        public const string HeroicFlameGlowKey = "MagnumOpus:FuneralHeroicFlameGlow";

        public override void PostSetupContent()
        {
            if (Main.dedServ) return;

            // EroicaFuneralTrail — funeral flame trail with glow
            var funeralTrail = ModContent.Request<Effect>("MagnumOpus/Effects/Eroica/FuneralPrayer/EroicaFuneralTrail", AssetRequestMode.ImmediateLoad);
            GameShaders.Misc[FuneralFlameFlowKey] = new MiscShaderData(funeralTrail, "FuneralFlameFlow");
            GameShaders.Misc[FuneralGlowPassKey] = new MiscShaderData(funeralTrail, "FuneralGlowPass");

            // PrayerConvergence — convergence point shader for when beams meet
            var convergence = ModContent.Request<Effect>("MagnumOpus/Effects/Eroica/FuneralPrayer/PrayerConvergence", AssetRequestMode.ImmediateLoad);
            GameShaders.Misc[ConvergenceMainKey] = new MiscShaderData(convergence, "ConvergenceMain");
            GameShaders.Misc[ConvergenceGlowKey] = new MiscShaderData(convergence, "ConvergenceGlow");

            // RequiemBeam — ricochet beam shader
            var requiemBeam = ModContent.Request<Effect>("MagnumOpus/Effects/Eroica/FuneralPrayer/RequiemBeam", AssetRequestMode.ImmediateLoad);
            GameShaders.Misc[RequiemBeamMainKey] = new MiscShaderData(requiemBeam, "RequiemBeamMain");
            GameShaders.Misc[RequiemBeamGlowKey] = new MiscShaderData(requiemBeam, "RequiemBeamGlow");

            // Shared HeroicFlameTrail — reused across Eroica weapons
            var heroicFlame = ModContent.Request<Effect>("MagnumOpus/Effects/Eroica/HeroicFlameTrail", AssetRequestMode.ImmediateLoad);
            GameShaders.Misc[HeroicFlameFlowKey] = new MiscShaderData(heroicFlame, "HeroicFlameFlow");
            GameShaders.Misc[HeroicFlameGlowKey] = new MiscShaderData(heroicFlame, "HeroicFlameGlow");
        }
    }
}
