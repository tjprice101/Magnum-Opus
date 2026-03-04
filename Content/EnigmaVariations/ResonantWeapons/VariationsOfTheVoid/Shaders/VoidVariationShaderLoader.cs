using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.VariationsOfTheVoid.Shaders
{
    /// <summary>
    /// Loads VariationsOfTheVoid weapon-specific shaders into GameShaders.Misc.
    /// Each shader is self-contained 窶・no shared Enigma shader manager.
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public sealed class VoidVariationShaderLoader : ModSystem
    {
        internal static Asset<Effect> SwingTrailAsset;
        internal static Asset<Effect> BeamAsset;

        public override void PostSetupContent()
        {
            var assets = Mod.Assets;
            Asset<Effect> Load(string path) => assets.Request<Effect>(path, AssetRequestMode.ImmediateLoad);

            // Swing trail shader 窶・two techniques: VoidVariationSwingFlow (main trail) and VoidVariationSwingGlow (soft bloom)
            SwingTrailAsset = Load("Content/EnigmaVariations/ResonantWeapons/VariationsOfTheVoid/Shaders/VoidVariationSwingTrail");
            GameShaders.Misc["MagnumOpus:VoidVariationSwingFlow"] = new MiscShaderData(SwingTrailAsset, "P0");
            GameShaders.Misc["MagnumOpus:VoidVariationSwingGlow"] = new MiscShaderData(SwingTrailAsset, "P0");

            // Beam shader 窶・tri-beam convergence visual
            BeamAsset = Load("Content/EnigmaVariations/ResonantWeapons/VariationsOfTheVoid/Shaders/VoidVariationBeam");
            GameShaders.Misc["MagnumOpus:VoidVariationBeamFlow"] = new MiscShaderData(BeamAsset, "P0");
        }
    }
}
