using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Weapons.CelestialValor.Shaders
{
    [Autoload(Side = ModSide.Client)]
    public sealed class ValorShaderLoader : ModSystem
    {
        // Shader keys for GameShaders.Misc
        public const string HeroicTrailKey = "MagnumOpus:ValorHeroicTrail";
        public const string ValorFlareKey = "MagnumOpus:ValorFlare";
        public const string ValorAuraMainKey = "MagnumOpus:ValorAuraMain";
        public const string ValorAuraGlowKey = "MagnumOpus:ValorAuraGlow";
        public const string ValorStandardPrimitiveKey = "MagnumOpus:ValorStandardPrimitive";

        public override void PostSetupContent()
        {
            if (Main.dedServ) return;

            // Load trail shader (2 techniques)
            var trailEffect = Mod.Assets.Request<Effect>("Effects/Eroica/CelestialValor/CelestialValorTrail",
                ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            if (trailEffect != null)
            {
                GameShaders.Misc[HeroicTrailKey] = new MiscShaderData(
                    Mod.Assets.Request<Effect>("Effects/Eroica/CelestialValor/CelestialValorTrail",
                        ReLogic.Content.AssetRequestMode.ImmediateLoad), "HeroicTrail");
                GameShaders.Misc[ValorFlareKey] = new MiscShaderData(
                    Mod.Assets.Request<Effect>("Effects/Eroica/CelestialValor/CelestialValorTrail",
                        ReLogic.Content.AssetRequestMode.ImmediateLoad), "ValorFlare");
                // Also register the main trail as the standard primitive for the trail renderer
                GameShaders.Misc[ValorStandardPrimitiveKey] = new MiscShaderData(
                    Mod.Assets.Request<Effect>("Effects/Eroica/CelestialValor/CelestialValorTrail",
                        ReLogic.Content.AssetRequestMode.ImmediateLoad), "HeroicTrail");
            }

            // Load aura shader (2 techniques)
            var auraEffect = Mod.Assets.Request<Effect>("Effects/Eroica/CelestialValor/ValorAura",
                ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            if (auraEffect != null)
            {
                GameShaders.Misc[ValorAuraMainKey] = new MiscShaderData(
                    Mod.Assets.Request<Effect>("Effects/Eroica/CelestialValor/ValorAura",
                        ReLogic.Content.AssetRequestMode.ImmediateLoad), "ValorAuraMain");
                GameShaders.Misc[ValorAuraGlowKey] = new MiscShaderData(
                    Mod.Assets.Request<Effect>("Effects/Eroica/CelestialValor/ValorAura",
                        ReLogic.Content.AssetRequestMode.ImmediateLoad), "ValorAuraGlow");
            }
        }
    }
}
