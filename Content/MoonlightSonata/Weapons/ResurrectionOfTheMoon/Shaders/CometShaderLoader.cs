using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.ResurrectionOfTheMoon.Shaders
{
    /// <summary>
    /// Loads and registers all Resurrection of the Moon weapon-specific shaders.
    /// Shader keys:
    ///   "MagnumOpus:CometTrailMain"       — Burning comet tail trail body (ember scrolling + cooling gradient)
    ///   "MagnumOpus:CometTrailGlow"        — Comet trail glow pass (bloom underlayer)
    ///   "MagnumOpus:SupernovaBlastMain"    — Radial crater explosion (full effect with lances)
    ///   "MagnumOpus:SupernovaBlastRing"    — Shockwave ring overlay for supernova
    ///   "MagnumOpus:CometLunarBeam"        — Shared LunarBeam shader for fallback trails
    ///   "MagnumOpus:CometStandardPrimitive"— Fallback primitive shader
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public sealed class CometShaderLoader : ModSystem
    {
        internal static Asset<Effect> CometTrailAsset;
        internal static Asset<Effect> SupernovaBlastAsset;
        internal static Asset<Effect> LunarBeamAsset;

        public override void PostSetupContent()
        {
            var assets = Mod.Assets;
            Asset<Effect> Load(string path) => assets.Request<Effect>(path, AssetRequestMode.ImmediateLoad);

            // CometTrail: Burning comet tail + glow pass (2 techniques)
            CometTrailAsset = Load("Effects/MoonlightSonata/ResurrectionOfTheMoon/CometTrail");
            GameShaders.Misc["MagnumOpus:CometTrailMain"] = new MiscShaderData(CometTrailAsset, "CometTrailMain");
            GameShaders.Misc["MagnumOpus:CometTrailGlow"] = new MiscShaderData(CometTrailAsset, "CometTrailGlow");

            // SupernovaBlast: Radial explosion + shockwave ring (2 techniques)
            SupernovaBlastAsset = Load("Effects/MoonlightSonata/ResurrectionOfTheMoon/SupernovaBlast");
            GameShaders.Misc["MagnumOpus:SupernovaBlastMain"] = new MiscShaderData(SupernovaBlastAsset, "SupernovaBlastMain");
            GameShaders.Misc["MagnumOpus:SupernovaBlastRing"] = new MiscShaderData(SupernovaBlastAsset, "SupernovaBlastRing");

            // LunarBeam: Shared Moonlight Sonata beam shader (fallback)
            LunarBeamAsset = Load("Effects/MoonlightSonata/LunarBeam");
            GameShaders.Misc["MagnumOpus:CometLunarBeam"] = new MiscShaderData(LunarBeamAsset, "LunarBeamMain");
            GameShaders.Misc["MagnumOpus:CometStandardPrimitive"] = new MiscShaderData(LunarBeamAsset, "LunarBeamGlow");
        }
    }
}
