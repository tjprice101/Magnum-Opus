using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.StaffOfTheLunarPhases.Shaders
{
    /// <summary>
    /// Loads and registers all Staff of the Lunar Phases weapon-specific shaders.
    /// Shader keys:
    ///   "MagnumOpus:GoliathBeamMain"       — Moonlight beam trail body (cosmic gradient + energy flow)
    ///   "MagnumOpus:GoliathBeamGlow"        — Beam trail glow pass (bloom underlayer)
    ///   "MagnumOpus:GoliathDevastatingMain" — Devastating beam trail (intense, wider variant)
    ///   "MagnumOpus:GoliathDevastatingGlow" — Devastating beam glow pass
    ///   "MagnumOpus:GoliathLunarBeam"       — LunarBeam shader for fallback trail rendering
    ///   "MagnumOpus:GoliathStandardPrimitive" — Fallback primitive shader
    ///
    /// The GravitationalRift and SummonCircle shaders are loaded by the central ShaderLoader
    /// and accessed via MoonlightSonataShaderManager's Apply methods.
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public sealed class GoliathShaderLoader : ModSystem
    {
        internal static Asset<Effect> GravitationalRiftAsset;
        internal static Asset<Effect> SummonCircleAsset;
        internal static Asset<Effect> LunarBeamAsset;

        public override void PostSetupContent()
        {
            var assets = Mod.Assets;
            Asset<Effect> Load(string path) => assets.Request<Effect>(path, AssetRequestMode.ImmediateLoad);

            // GravitationalRift: Spiral gravity well effect (2 techniques: Main + Glow)
            // Used for Goliath ambient aura and beam charge-up
            GravitationalRiftAsset = Load("Effects/MoonlightSonata/StaffOfTheLunarPhases/GravitationalRift");
            GameShaders.Misc["MagnumOpus:GoliathBeamMain"] = new MiscShaderData(GravitationalRiftAsset, "GravitationalRiftMain");
            GameShaders.Misc["MagnumOpus:GoliathBeamGlow"] = new MiscShaderData(GravitationalRiftAsset, "GravitationalRiftGlow");

            // SummonCircle: Rotating sigil with lunar phase nodes (2 techniques: Main + Glow)
            // Used for summoning ritual VFX
            SummonCircleAsset = Load("Effects/MoonlightSonata/StaffOfTheLunarPhases/SummonCircle");
            GameShaders.Misc["MagnumOpus:GoliathDevastatingMain"] = new MiscShaderData(SummonCircleAsset, "SummonCircleMain");
            GameShaders.Misc["MagnumOpus:GoliathDevastatingGlow"] = new MiscShaderData(SummonCircleAsset, "SummonCircleGlow");

            // LunarBeam: Shared Moonlight Sonata beam shader (fallback for trail rendering)
            LunarBeamAsset = Load("Effects/MoonlightSonata/LunarBeam");
            GameShaders.Misc["MagnumOpus:GoliathLunarBeam"] = new MiscShaderData(LunarBeamAsset, "LunarBeamMain");
            GameShaders.Misc["MagnumOpus:GoliathStandardPrimitive"] = new MiscShaderData(LunarBeamAsset, "LunarBeamGlow");
        }
    }
}
