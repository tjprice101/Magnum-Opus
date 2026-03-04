using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.DeathTollingBell.Shaders
{
    /// <summary>
    /// Formal ShaderLoader for Death Tolling Bell (Summon).
    /// Consolidates the shader loading currently scattered in Utils into a proper ModSystem.
    /// Adds theme-wide shaders alongside the existing BellToll.fx + RippleShader.
    /// 
    /// Registered shaders:
    ///   1. MagnumOpus:BellTollWave             — Custom BellToll.fx for bell-toll shockwave rendering
    ///   2. MagnumOpus:BellTollRipple           — Foundation RippleShader for toll impact rings
    ///   3. MagnumOpus:BellTollHellfireBloom    — Theme-wide HellfireBloom for bell glow overlay
    ///   4. MagnumOpus:BellTollJudgmentAura     — Theme-wide JudgmentAura for minion aura
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public sealed class DeathTollingBellShaderLoader : ModSystem
    {
        internal static Asset<Effect> BellTollShader;
        internal static Asset<Effect> RippleShader;
        internal static Asset<Effect> HellfireBloomShader;
        internal static Asset<Effect> JudgmentAuraShader;

        public override void PostSetupContent()
        {
            if (Main.dedServ) return;

            BellTollShader = ModContent.Request<Effect>(
                "MagnumOpus/Effects/DiesIrae/DeathTollingBell/BellToll", AssetRequestMode.ImmediateLoad);
            RippleShader = ModContent.Request<Effect>(
                "MagnumOpus/Content/FoundationWeapons/ImpactFoundation/Shaders/RippleShader",
                AssetRequestMode.ImmediateLoad);
            HellfireBloomShader = Mod.Assets.Request<Effect>(
                "Effects/DiesIrae/HellfireBloom", AssetRequestMode.ImmediateLoad);
            JudgmentAuraShader = Mod.Assets.Request<Effect>(
                "Effects/DiesIrae/JudgmentAura", AssetRequestMode.ImmediateLoad);

            if (BellTollShader?.Value != null)
                GameShaders.Misc["MagnumOpus:BellTollWave"] = new MiscShaderData(BellTollShader, "P0");
            if (RippleShader?.Value != null)
                GameShaders.Misc["MagnumOpus:BellTollRipple"] = new MiscShaderData(RippleShader, "P0");
            if (HellfireBloomShader?.Value != null)
                GameShaders.Misc["MagnumOpus:BellTollHellfireBloom"] = new MiscShaderData(HellfireBloomShader, "P0");
            if (JudgmentAuraShader?.Value != null)
                GameShaders.Misc["MagnumOpus:BellTollJudgmentAura"] = new MiscShaderData(JudgmentAuraShader, "P0");
        }

        public override void Unload()
        {
            BellTollShader = null;
            RippleShader = null;
            HellfireBloomShader = null;
            JudgmentAuraShader = null;
        }
    }
}
