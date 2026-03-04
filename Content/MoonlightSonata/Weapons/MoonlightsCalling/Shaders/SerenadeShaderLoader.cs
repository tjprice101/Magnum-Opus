using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.MoonlightsCalling.Shaders
{
    /// <summary>
    /// Loads and registers all Moonlight's Calling weapon-specific shaders.
    /// Shader keys:
    ///   "MagnumOpus:SerenadePrismaticBeam"     窶・Prismatic beam trail body (spectral splitting)
    ///   "MagnumOpus:SerenadePrismaticGlow"      窶・Prismatic beam glow pass (bloom underlayer)
    ///   "MagnumOpus:SerenadeRefractionRipple"    窶・Refraction ripple at bounce points
    ///   "MagnumOpus:SerenadeRefractionSubtle"    窶・Subtle ambient ripple variant
    ///   "MagnumOpus:SerenadeLunarBeam"           窶・Shared LunarBeam for Serenade mega-beam
    ///   "MagnumOpus:SerenadeStandardPrimitive"   窶・Fallback primitive shader
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public sealed class SerenadeShaderLoader : ModSystem
    {
        internal static Asset<Effect> PrismaticBeamAsset;
        internal static Asset<Effect> RefractionRippleAsset;
        internal static Asset<Effect> LunarBeamAsset;

        public override void PostSetupContent()
        {
            var assets = Mod.Assets;
            Asset<Effect> Load(string path) => assets.Request<Effect>(path, AssetRequestMode.ImmediateLoad);

            // PrismaticBeam: Main beam trail + glow pass
            PrismaticBeamAsset = Load("Effects/MoonlightSonata/MoonlightsCalling/PrismaticBeam");
            GameShaders.Misc["MagnumOpus:SerenadePrismaticBeam"] = new MiscShaderData(PrismaticBeamAsset, "P0");
            GameShaders.Misc["MagnumOpus:SerenadePrismaticGlow"] = new MiscShaderData(PrismaticBeamAsset, "P0");

            // RefractionRipple: Bounce point ripple + subtle variant
            RefractionRippleAsset = Load("Effects/MoonlightSonata/MoonlightsCalling/RefractionRipple");
            GameShaders.Misc["MagnumOpus:SerenadeRefractionRipple"] = new MiscShaderData(RefractionRippleAsset, "P0");
            GameShaders.Misc["MagnumOpus:SerenadeRefractionSubtle"] = new MiscShaderData(RefractionRippleAsset, "P0");

            // LunarBeam: Shared Moonlight Sonata beam shader for Serenade mega-beam
            LunarBeamAsset = Load("Effects/MoonlightSonata/LunarBeam");
            GameShaders.Misc["MagnumOpus:SerenadeLunarBeam"] = new MiscShaderData(LunarBeamAsset, "P0");

            // Fallback standard primitive reusing prismatic beam main pass
            GameShaders.Misc["MagnumOpus:SerenadeStandardPrimitive"] = new MiscShaderData(PrismaticBeamAsset, "P0");
        }
    }
}
