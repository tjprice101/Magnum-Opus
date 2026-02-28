using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Weapons.FinalityOfTheSakura.Shaders
{
    [Autoload(Side = ModSide.Client)]
    public class FinalityShaderLoader : ModSystem
    {
        // DarkFlameAura.fx passes
        public const string DarkFlameTrailKey = "MagnumOpus:FinalityDarkFlameTrail";
        public const string DarkFlameGlowKey = "MagnumOpus:FinalityDarkFlameGlow";

        // FateSummonCircle.fx pass
        public const string SummonCircleKey = "MagnumOpus:FinalitySummonCircle";

        // Shared HeroicFlameTrail.fx passes (scoped to Finality to avoid key collisions)
        public const string HeroicFlameFlowKey = "MagnumOpus:FinalityHeroicFlameFlow";
        public const string HeroicFlameGlowKey = "MagnumOpus:FinalityHeroicFlameGlow";

        private static MiscShaderData _darkFlameTrail;
        private static MiscShaderData _darkFlameGlow;
        private static MiscShaderData _summonCircle;

        public static MiscShaderData DarkFlameTrail => _darkFlameTrail;
        public static MiscShaderData DarkFlameGlow => _darkFlameGlow;
        public static MiscShaderData SummonCircleShader => _summonCircle;

        public override void PostSetupContent()
        {
            if (Main.dedServ) return;

            // DarkFlameAura — dark flame trail and glow for summoned minion
            try
            {
                var darkFlameAura = ModContent.Request<Effect>("MagnumOpus/Effects/Eroica/FinalityOfTheSakura/DarkFlameAura", AssetRequestMode.ImmediateLoad);
                _darkFlameTrail = new MiscShaderData(darkFlameAura, "DarkFlameAuraMain");
                _darkFlameGlow = new MiscShaderData(darkFlameAura, "DarkFlameAuraGlow");
            }
            catch
            {
                // Fall back to shared Eroica HeroicFlameTrail
                var heroicFlame = ModContent.Request<Effect>("MagnumOpus/Effects/Eroica/HeroicFlameTrail", AssetRequestMode.ImmediateLoad);
                _darkFlameTrail = new MiscShaderData(heroicFlame, "HeroicFlameFlow");
                _darkFlameGlow = new MiscShaderData(heroicFlame, "HeroicFlameGlow");
            }

            // FateSummonCircle — summoning circle shader for minion spawn
            try
            {
                var summonCircle = ModContent.Request<Effect>("MagnumOpus/Effects/Eroica/FinalityOfTheSakura/FateSummonCircle", AssetRequestMode.ImmediateLoad);
                _summonCircle = new MiscShaderData(summonCircle, "FateSummonMain");
            }
            catch
            {
                // Fall back to shared Eroica HeroicFlameTrail
                var heroicFlame = ModContent.Request<Effect>("MagnumOpus/Effects/Eroica/HeroicFlameTrail", AssetRequestMode.ImmediateLoad);
                _summonCircle = new MiscShaderData(heroicFlame, "HeroicFlameFlow");
            }

            // Register all shaders in GameShaders.Misc for global access
            GameShaders.Misc[DarkFlameTrailKey] = _darkFlameTrail;
            GameShaders.Misc[DarkFlameGlowKey] = _darkFlameGlow;
            GameShaders.Misc[SummonCircleKey] = _summonCircle;

            // Shared HeroicFlameTrail — scoped keys for Finality usage
            try
            {
                var heroicFlame = ModContent.Request<Effect>("MagnumOpus/Effects/Eroica/HeroicFlameTrail", AssetRequestMode.ImmediateLoad);
                GameShaders.Misc[HeroicFlameFlowKey] = new MiscShaderData(heroicFlame, "HeroicFlameFlow");
                GameShaders.Misc[HeroicFlameGlowKey] = new MiscShaderData(heroicFlame, "HeroicFlameGlow");
            }
            catch
            {
                // HeroicFlameTrail is a shared Eroica asset; if missing, shaders simply won't render
            }
        }
    }
}
