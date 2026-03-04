using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.WrathfulContract.Shaders
{
    /// <summary>
    /// Formal ShaderLoader for Wrathful Contract (Summon).
    /// Consolidates the 2 Foundation shaders loaded in Utils + adds theme-wide shaders + additional Foundation.
    /// 
    /// Registered shaders:
    ///   1. MagnumOpus:WrathContractRadialNoiseMask — Foundation RadialNoiseMaskShader for demon sigil
    ///   2. MagnumOpus:WrathContractRipple          — Foundation RippleShader for demon slam impacts
    ///   3. MagnumOpus:WrathContractHellfireBloom   — Theme-wide HellfireBloom for demon fire bloom
    ///   4. MagnumOpus:WrathContractSmearDistort    — Foundation SmearDistortShader for demon claw swipe
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public sealed class WrathfulContractShaderLoader : ModSystem
    {
        internal static Asset<Effect> RadialNoiseMaskShader;
        internal static Asset<Effect> RippleShader;
        internal static Asset<Effect> HellfireBloomShader;
        internal static Asset<Effect> SmearDistortShader;

        public override void PostSetupContent()
        {
            if (Main.dedServ) return;

            RadialNoiseMaskShader = ModContent.Request<Effect>(
                "MagnumOpus/Content/FoundationWeapons/MaskFoundation/Shaders/RadialNoiseMaskShader",
                AssetRequestMode.ImmediateLoad);
            RippleShader = ModContent.Request<Effect>(
                "MagnumOpus/Content/FoundationWeapons/ImpactFoundation/Shaders/RippleShader",
                AssetRequestMode.ImmediateLoad);
            HellfireBloomShader = Mod.Assets.Request<Effect>(
                "Effects/DiesIrae/HellfireBloom", AssetRequestMode.ImmediateLoad);
            SmearDistortShader = ModContent.Request<Effect>(
                "MagnumOpus/Content/FoundationWeapons/SwordSmearFoundation/Shaders/SmearDistortShader",
                AssetRequestMode.ImmediateLoad);

            if (RadialNoiseMaskShader?.Value != null)
                GameShaders.Misc["MagnumOpus:WrathContractRadialNoiseMask"] = new MiscShaderData(RadialNoiseMaskShader, "P0");
            if (RippleShader?.Value != null)
                GameShaders.Misc["MagnumOpus:WrathContractRipple"] = new MiscShaderData(RippleShader, "P0");
            if (HellfireBloomShader?.Value != null)
                GameShaders.Misc["MagnumOpus:WrathContractHellfireBloom"] = new MiscShaderData(HellfireBloomShader, "P0");
            if (SmearDistortShader?.Value != null)
                GameShaders.Misc["MagnumOpus:WrathContractSmearDistort"] = new MiscShaderData(SmearDistortShader, "P0");
        }

        public override void Unload()
        {
            RadialNoiseMaskShader = null;
            RippleShader = null;
            HellfireBloomShader = null;
            SmearDistortShader = null;
        }
    }
}
