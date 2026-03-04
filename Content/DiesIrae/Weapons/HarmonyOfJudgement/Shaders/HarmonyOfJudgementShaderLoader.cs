using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.HarmonyOfJudgement.Shaders
{
    /// <summary>
    /// Formal ShaderLoader for Harmony of Judgement (Summon).
    /// Consolidates the 4 Foundation shaders loaded in Utils + adds theme-wide shaders.
    /// 
    /// Registered shaders:
    ///   1. MagnumOpus:HarmonyRadialNoiseMask   — Foundation RadialNoiseMaskShader for sigil rendering
    ///   2. MagnumOpus:HarmonyThinBeam          — Foundation ThinBeamShader for summoner beam attacks
    ///   3. MagnumOpus:HarmonyRipple            — Foundation RippleShader for impact waves
    ///   4. MagnumOpus:HarmonyXSlash            — Foundation XSlashShader for cross-slash attack
    ///   5. MagnumOpus:HarmonyHellfireBloom     — Theme-wide HellfireBloom for sigil glow
    ///   6. MagnumOpus:HarmonyJudgmentAura      — Theme-wide JudgmentAura for judgment aura overlay
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public sealed class HarmonyOfJudgementShaderLoader : ModSystem
    {
        internal static Asset<Effect> RadialNoiseMaskShader;
        internal static Asset<Effect> ThinBeamShader;
        internal static Asset<Effect> RippleShader;
        internal static Asset<Effect> XSlashShader;
        internal static Asset<Effect> HellfireBloomShader;
        internal static Asset<Effect> JudgmentAuraShader;

        public override void PostSetupContent()
        {
            if (Main.dedServ) return;

            RadialNoiseMaskShader = ModContent.Request<Effect>(
                "MagnumOpus/Content/FoundationWeapons/MaskFoundation/Shaders/RadialNoiseMaskShader",
                AssetRequestMode.ImmediateLoad);
            ThinBeamShader = ModContent.Request<Effect>(
                "MagnumOpus/Content/FoundationWeapons/ThinLaserFoundation/Shaders/ThinBeamShader",
                AssetRequestMode.ImmediateLoad);
            RippleShader = ModContent.Request<Effect>(
                "MagnumOpus/Content/FoundationWeapons/ImpactFoundation/Shaders/RippleShader",
                AssetRequestMode.ImmediateLoad);
            XSlashShader = ModContent.Request<Effect>(
                "MagnumOpus/Content/FoundationWeapons/XSlashFoundation/Shaders/XSlashShader",
                AssetRequestMode.ImmediateLoad);
            HellfireBloomShader = Mod.Assets.Request<Effect>(
                "Effects/DiesIrae/HellfireBloom", AssetRequestMode.ImmediateLoad);
            JudgmentAuraShader = Mod.Assets.Request<Effect>(
                "Effects/DiesIrae/JudgmentAura", AssetRequestMode.ImmediateLoad);

            if (RadialNoiseMaskShader?.Value != null)
                GameShaders.Misc["MagnumOpus:HarmonyRadialNoiseMask"] = new MiscShaderData(RadialNoiseMaskShader, "P0");
            if (ThinBeamShader?.Value != null)
                GameShaders.Misc["MagnumOpus:HarmonyThinBeam"] = new MiscShaderData(ThinBeamShader, "P0");
            if (RippleShader?.Value != null)
                GameShaders.Misc["MagnumOpus:HarmonyRipple"] = new MiscShaderData(RippleShader, "P0");
            if (XSlashShader?.Value != null)
                GameShaders.Misc["MagnumOpus:HarmonyXSlash"] = new MiscShaderData(XSlashShader, "P0");
            if (HellfireBloomShader?.Value != null)
                GameShaders.Misc["MagnumOpus:HarmonyHellfireBloom"] = new MiscShaderData(HellfireBloomShader, "P0");
            if (JudgmentAuraShader?.Value != null)
                GameShaders.Misc["MagnumOpus:HarmonyJudgmentAura"] = new MiscShaderData(JudgmentAuraShader, "P0");
        }

        public override void Unload()
        {
            RadialNoiseMaskShader = null;
            ThinBeamShader = null;
            RippleShader = null;
            XSlashShader = null;
            HellfireBloomShader = null;
            JudgmentAuraShader = null;
        }
    }
}
