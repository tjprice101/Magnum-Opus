using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.ExecutionersVerdict
{
    /// <summary>
    /// Loads and registers all shaders for Executioner's Verdict.
    /// Custom .fx shader from Effects/DiesIrae/ExecutionersVerdict/ plus Foundation shaders.
    /// 
    /// Registered shaders:
    ///   1. MagnumOpus:GuillotineBladeSlash     — Custom guillotine blade slash arc
    ///   2. MagnumOpus:VerdictSmearDistort       — Foundation SmearDistortShader for swing arc
    ///   3. MagnumOpus:VerdictRipple             — Foundation RippleShader for impact ripples
    ///   4. MagnumOpus:VerdictThinSlash          — Foundation ThinSlashShader for executioner cuts
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public sealed class ExecutionersVerdictShaderLoader : ModSystem
    {
        internal static Asset<Effect> GuillotineBladeShader;
        internal static Asset<Effect> SmearDistortShader;
        internal static Asset<Effect> RippleShader;
        internal static Asset<Effect> ThinSlashShader;

        public override void PostSetupContent()
        {
            if (Main.dedServ) return;

            GuillotineBladeShader = Mod.Assets.Request<Effect>(
                "Effects/DiesIrae/ExecutionersVerdict/GuillotineBlade", AssetRequestMode.ImmediateLoad);

            if (GuillotineBladeShader?.Value != null)
            {
                GameShaders.Misc["MagnumOpus:GuillotineBladeSlash"] =
                    new MiscShaderData(GuillotineBladeShader, "P0");
            }

            SmearDistortShader = ModContent.Request<Effect>(
                "MagnumOpus/Content/FoundationWeapons/SwordSmearFoundation/Shaders/SmearDistortShader",
                AssetRequestMode.ImmediateLoad);
            RippleShader = ModContent.Request<Effect>(
                "MagnumOpus/Content/FoundationWeapons/ImpactFoundation/Shaders/RippleShader",
                AssetRequestMode.ImmediateLoad);
            ThinSlashShader = ModContent.Request<Effect>(
                "MagnumOpus/Content/FoundationWeapons/ThinSlashFoundation/Shaders/ThinSlashShader",
                AssetRequestMode.ImmediateLoad);

            if (SmearDistortShader?.Value != null)
                GameShaders.Misc["MagnumOpus:VerdictSmearDistort"] = new MiscShaderData(SmearDistortShader, "P0");
            if (RippleShader?.Value != null)
                GameShaders.Misc["MagnumOpus:VerdictRipple"] = new MiscShaderData(RippleShader, "P0");
            if (ThinSlashShader?.Value != null)
                GameShaders.Misc["MagnumOpus:VerdictThinSlash"] = new MiscShaderData(ThinSlashShader, "P0");
        }

        public override void Unload()
        {
            GuillotineBladeShader = null;
            SmearDistortShader = null;
            RippleShader = null;
            ThinSlashShader = null;
        }
    }
}
