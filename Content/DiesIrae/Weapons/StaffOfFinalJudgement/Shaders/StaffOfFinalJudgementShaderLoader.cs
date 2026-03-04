using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.StaffOfFinalJudgement.Shaders
{
    /// <summary>
    /// Loads shaders for Staff of Final Judgement (Magic).
    /// No custom .fx exists — uses theme-wide Dies Irae + Foundation magic patterns.
    /// 
    /// Registered shaders:
    ///   1. MagnumOpus:FinalJudgementHellfireBloom   — Theme-wide HellfireBloom for ignition bloom
    ///   2. MagnumOpus:FinalJudgementJudgmentAura    — Theme-wide JudgmentAura for floating ignition aura
    ///   3. MagnumOpus:FinalJudgementRadialNoiseMask — Foundation RadialNoiseMaskShader for ignition zone
    ///   4. MagnumOpus:FinalJudgementRipple          — Foundation RippleShader for ignition detonation
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public sealed class StaffOfFinalJudgementShaderLoader : ModSystem
    {
        internal static Asset<Effect> HellfireBloomShader;
        internal static Asset<Effect> JudgmentAuraShader;
        internal static Asset<Effect> RadialNoiseMaskShader;
        internal static Asset<Effect> RippleShader;

        public override void PostSetupContent()
        {
            if (Main.dedServ) return;

            HellfireBloomShader = Mod.Assets.Request<Effect>(
                "Effects/DiesIrae/HellfireBloom", AssetRequestMode.ImmediateLoad);
            JudgmentAuraShader = Mod.Assets.Request<Effect>(
                "Effects/DiesIrae/JudgmentAura", AssetRequestMode.ImmediateLoad);
            RadialNoiseMaskShader = ModContent.Request<Effect>(
                "MagnumOpus/Content/FoundationWeapons/MaskFoundation/Shaders/RadialNoiseMaskShader",
                AssetRequestMode.ImmediateLoad);
            RippleShader = ModContent.Request<Effect>(
                "MagnumOpus/Content/FoundationWeapons/ImpactFoundation/Shaders/RippleShader",
                AssetRequestMode.ImmediateLoad);

            if (HellfireBloomShader?.Value != null)
                GameShaders.Misc["MagnumOpus:FinalJudgementHellfireBloom"] = new MiscShaderData(HellfireBloomShader, "P0");
            if (JudgmentAuraShader?.Value != null)
                GameShaders.Misc["MagnumOpus:FinalJudgementJudgmentAura"] = new MiscShaderData(JudgmentAuraShader, "P0");
            if (RadialNoiseMaskShader?.Value != null)
                GameShaders.Misc["MagnumOpus:FinalJudgementRadialNoiseMask"] = new MiscShaderData(RadialNoiseMaskShader, "P0");
            if (RippleShader?.Value != null)
                GameShaders.Misc["MagnumOpus:FinalJudgementRipple"] = new MiscShaderData(RippleShader, "P0");
        }

        public override void Unload()
        {
            HellfireBloomShader = null;
            JudgmentAuraShader = null;
            RadialNoiseMaskShader = null;
            RippleShader = null;
        }
    }
}
