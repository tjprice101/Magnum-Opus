using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.DamnationsCannon
{
    /// <summary>
    /// Loads shaders for Damnation's Cannon (Ranged).
    /// Theme-wide Dies Irae shaders plus Foundation ranged/explosion patterns.
    /// 
    /// Registered shaders:
    ///   1. MagnumOpus:DamnationHellfireBloom   — Theme-wide HellfireBloom for explosive muzzle flash
    ///   2. MagnumOpus:DamnationJudgmentAura    — Theme-wide JudgmentAura for hellfire zone
    ///   3. MagnumOpus:DamnationRadialNoiseMask — Foundation RadialNoiseMaskShader for hellfire zone
    ///   4. MagnumOpus:DamnationRipple          — Foundation RippleShader for explosive impacts
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public sealed class DamnationsCannonShaderLoader : ModSystem
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
                GameShaders.Misc["MagnumOpus:DamnationHellfireBloom"] = new MiscShaderData(HellfireBloomShader, "P0");
            if (JudgmentAuraShader?.Value != null)
                GameShaders.Misc["MagnumOpus:DamnationJudgmentAura"] = new MiscShaderData(JudgmentAuraShader, "P0");
            if (RadialNoiseMaskShader?.Value != null)
                GameShaders.Misc["MagnumOpus:DamnationRadialNoiseMask"] = new MiscShaderData(RadialNoiseMaskShader, "P0");
            if (RippleShader?.Value != null)
                GameShaders.Misc["MagnumOpus:DamnationRipple"] = new MiscShaderData(RippleShader, "P0");
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
