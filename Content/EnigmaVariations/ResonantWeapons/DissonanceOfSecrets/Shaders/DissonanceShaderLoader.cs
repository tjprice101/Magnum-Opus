using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.DissonanceOfSecrets.Shaders
{
    /// <summary>
    /// Loads DissonanceOfSecrets weapon-specific shaders into GameShaders.Misc.
    /// Each shader is self-contained 窶・no shared Enigma shader manager.
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public sealed class DissonanceShaderLoader : ModSystem
    {
        internal static Asset<Effect> OrbAuraAsset;
        internal static Asset<Effect> RiddleTrailAsset;

        public override void PostSetupContent()
        {
            var assets = Mod.Assets;
            Asset<Effect> Load(string path) => assets.Request<Effect>(path, AssetRequestMode.ImmediateLoad);

            // Orb aura shader 窶・two techniques: DissonanceOrbAuraMain (radial rings) and DissonanceOrbAuraGlow (soft bloom)
            OrbAuraAsset = Load("Content/EnigmaVariations/ResonantWeapons/DissonanceOfSecrets/Shaders/DissonanceOrbAura");
            GameShaders.Misc["MagnumOpus:DissonanceOrbAuraMain"] = new MiscShaderData(OrbAuraAsset, "P0");
            GameShaders.Misc["MagnumOpus:DissonanceOrbAuraGlow"] = new MiscShaderData(OrbAuraAsset, "P0");

            // Riddlebolt trail shader
            RiddleTrailAsset = Load("Content/EnigmaVariations/ResonantWeapons/DissonanceOfSecrets/Shaders/DissonanceRiddleTrail");
            GameShaders.Misc["MagnumOpus:DissonanceRiddleTrail"] = new MiscShaderData(RiddleTrailAsset, "P0");
        }
    }
}
