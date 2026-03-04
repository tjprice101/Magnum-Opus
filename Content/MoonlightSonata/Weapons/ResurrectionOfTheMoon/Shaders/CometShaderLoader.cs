using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.ResurrectionOfTheMoon.Shaders
{
    /// <summary>
    /// Loads and registers all Resurrection of the Moon weapon-specific shaders.
    /// Shader keys:
    ///   "MagnumOpus:CometTrailMain"       窶・Burning comet tail trail body (ember scrolling + cooling gradient)
    ///   "MagnumOpus:CometTrailGlow"        窶・Comet trail glow pass (bloom underlayer)
    ///   "MagnumOpus:SupernovaBlastMain"    窶・Radial crater explosion (full effect with lances)
    ///   "MagnumOpus:SupernovaBlastRing"    窶・Shockwave ring overlay for supernova
    ///   "MagnumOpus:CometLunarBeam"        窶・Shared LunarBeam shader for fallback trails
    ///   "MagnumOpus:CometStandardPrimitive"窶・Fallback primitive shader
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
            GameShaders.Misc["MagnumOpus:CometTrailMain"] = new MiscShaderData(CometTrailAsset, "P0");
            GameShaders.Misc["MagnumOpus:CometTrailGlow"] = new MiscShaderData(CometTrailAsset, "P0");

            // SupernovaBlast: Radial explosion + shockwave ring (2 techniques)
            SupernovaBlastAsset = Load("Effects/MoonlightSonata/ResurrectionOfTheMoon/SupernovaBlast");
            GameShaders.Misc["MagnumOpus:SupernovaBlastMain"] = new MiscShaderData(SupernovaBlastAsset, "P0");
            GameShaders.Misc["MagnumOpus:SupernovaBlastRing"] = new MiscShaderData(SupernovaBlastAsset, "P0");

            // LunarBeam: Shared Moonlight Sonata beam shader (fallback)
            LunarBeamAsset = Load("Effects/MoonlightSonata/LunarBeam");
            GameShaders.Misc["MagnumOpus:CometLunarBeam"] = new MiscShaderData(LunarBeamAsset, "P0");
            GameShaders.Misc["MagnumOpus:CometStandardPrimitive"] = new MiscShaderData(LunarBeamAsset, "P0");
        }
    }
}
