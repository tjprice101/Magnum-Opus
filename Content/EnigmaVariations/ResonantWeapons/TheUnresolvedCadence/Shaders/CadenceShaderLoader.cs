using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheUnresolvedCadence.Shaders
{
    /// <summary>
    /// Loads TheUnresolvedCadence weapon-specific shaders into GameShaders.Misc.
    /// Each shader is self-contained 窶・no shared Enigma shader manager.
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public sealed class CadenceShaderLoader : ModSystem
    {
        internal static Asset<Effect> SwingTrailAsset;
        internal static Asset<Effect> CollapseAsset;

        public override void PostSetupContent()
        {
            var assets = Mod.Assets;
            Asset<Effect> Load(string path) => assets.Request<Effect>(path, AssetRequestMode.ImmediateLoad);

            // Swing trail shader 窶・two techniques: CadenceSwingFlow (main trail) and CadenceSwingGlow (soft bloom)
            SwingTrailAsset = Load("Content/EnigmaVariations/ResonantWeapons/TheUnresolvedCadence/Shaders/CadenceSwingTrail");
            GameShaders.Misc["MagnumOpus:CadenceSwingFlow"] = new MiscShaderData(SwingTrailAsset, "P0");
            GameShaders.Misc["MagnumOpus:CadenceSwingGlow"] = new MiscShaderData(SwingTrailAsset, "P0");

            // Collapse shader 窶・Paradox Collapse warp visual
            CollapseAsset = Load("Content/EnigmaVariations/ResonantWeapons/TheUnresolvedCadence/Shaders/CadenceCollapse");
            GameShaders.Misc["MagnumOpus:CadenceCollapseWarp"] = new MiscShaderData(CollapseAsset, "P0");
        }
    }
}
