using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.ChainOfJudgment
{
    /// <summary>
    /// Loads shaders for Chain of Judgment (Melee).
    /// Theme-wide Dies Irae shaders plus Foundation melee patterns.
    /// 
    /// Registered shaders:
    ///   1. MagnumOpus:ChainHellfireBloom       — Theme-wide HellfireBloom for chain fire impacts
    ///   2. MagnumOpus:ChainJudgmentAura        — Theme-wide JudgmentAura for chain glow
    ///   3. MagnumOpus:ChainSmearDistort         — Foundation SmearDistortShader for chain swing arc
    ///   4. MagnumOpus:ChainInfernalBeam         — Foundation InfernalBeamBodyShader for lightning arcs
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public sealed class ChainOfJudgmentShaderLoader : ModSystem
    {
        internal static Asset<Effect> HellfireBloomShader;
        internal static Asset<Effect> JudgmentAuraShader;
        internal static Asset<Effect> SmearDistortShader;
        internal static Asset<Effect> InfernalBeamShader;

        public override void PostSetupContent()
        {
            if (Main.dedServ) return;

            HellfireBloomShader = Mod.Assets.Request<Effect>(
                "Effects/DiesIrae/HellfireBloom", AssetRequestMode.ImmediateLoad);
            JudgmentAuraShader = Mod.Assets.Request<Effect>(
                "Effects/DiesIrae/JudgmentAura", AssetRequestMode.ImmediateLoad);
            SmearDistortShader = ModContent.Request<Effect>(
                "MagnumOpus/Content/FoundationWeapons/SwordSmearFoundation/Shaders/SmearDistortShader",
                AssetRequestMode.ImmediateLoad);
            InfernalBeamShader = ModContent.Request<Effect>(
                "MagnumOpus/Content/FoundationWeapons/InfernalBeamFoundation/Shaders/InfernalBeamBodyShader",
                AssetRequestMode.ImmediateLoad);

            if (HellfireBloomShader?.Value != null)
                GameShaders.Misc["MagnumOpus:ChainHellfireBloom"] = new MiscShaderData(HellfireBloomShader, "P0");
            if (JudgmentAuraShader?.Value != null)
                GameShaders.Misc["MagnumOpus:ChainJudgmentAura"] = new MiscShaderData(JudgmentAuraShader, "P0");
            if (SmearDistortShader?.Value != null)
                GameShaders.Misc["MagnumOpus:ChainSmearDistort"] = new MiscShaderData(SmearDistortShader, "P0");
            if (InfernalBeamShader?.Value != null)
                GameShaders.Misc["MagnumOpus:ChainInfernalBeam"] = new MiscShaderData(InfernalBeamShader, "P0");
        }

        public override void Unload()
        {
            HellfireBloomShader = null;
            JudgmentAuraShader = null;
            SmearDistortShader = null;
            InfernalBeamShader = null;
        }
    }
}
