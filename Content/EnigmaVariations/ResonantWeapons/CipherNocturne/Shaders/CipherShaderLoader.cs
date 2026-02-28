using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.CipherNocturne.Shaders
{
    /// <summary>
    /// Loads CipherNocturne's weapon-specific shaders into GameShaders.Misc.
    /// Each shader is self-contained — no shared Enigma shader manager.
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public sealed class CipherShaderLoader : ModSystem
    {
        internal static Asset<Effect> BeamTrailAsset;
        internal static Asset<Effect> SnapBackAsset;

        public override void PostSetupContent()
        {
            var assets = Mod.Assets;
            Asset<Effect> Load(string path) => assets.Request<Effect>(path, AssetRequestMode.ImmediateLoad);

            // Beam trail shader — two techniques: CipherBeamFlow (main beam) and CipherBeamGlow (glow overlay)
            BeamTrailAsset = Load("Content/EnigmaVariations/ResonantWeapons/CipherNocturne/Shaders/CipherBeamTrail");
            GameShaders.Misc["MagnumOpus:CipherBeamFlow"] = new MiscShaderData(BeamTrailAsset, "CipherBeamFlow");
            GameShaders.Misc["MagnumOpus:CipherBeamGlow"] = new MiscShaderData(BeamTrailAsset, "CipherBeamGlow");

            // Snap-back implosion shader
            SnapBackAsset = Load("Content/EnigmaVariations/ResonantWeapons/CipherNocturne/Shaders/CipherSnapBack");
            GameShaders.Misc["MagnumOpus:CipherSnapBack"] = new MiscShaderData(SnapBackAsset, "CipherSnapBackMain");
        }
    }
}
