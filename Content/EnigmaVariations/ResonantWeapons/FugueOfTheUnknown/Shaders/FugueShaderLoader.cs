using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.FugueOfTheUnknown.Shaders
{
    /// <summary>
    /// Loads FugueOfTheUnknown weapon-specific shaders into GameShaders.Misc.
    /// Each shader is self-contained — no shared Enigma shader manager.
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public sealed class FugueShaderLoader : ModSystem
    {
        internal static Asset<Effect> VoiceTrailAsset;
        internal static Asset<Effect> ConvergenceAsset;

        public override void PostSetupContent()
        {
            var assets = Mod.Assets;
            Asset<Effect> Load(string path) => assets.Request<Effect>(path, AssetRequestMode.ImmediateLoad);

            // Voice trail shader — spectral waveform trail for voice projectiles
            VoiceTrailAsset = Load("Content/EnigmaVariations/ResonantWeapons/FugueOfTheUnknown/Shaders/FugueVoiceTrail");
            GameShaders.Misc["MagnumOpus:FugueVoiceFlow"] = new MiscShaderData(VoiceTrailAsset, "FugueVoiceFlow");

            // Convergence shader — two techniques: wave (contracting rings) and glow (soft bloom)
            ConvergenceAsset = Load("Content/EnigmaVariations/ResonantWeapons/FugueOfTheUnknown/Shaders/FugueConvergence");
            GameShaders.Misc["MagnumOpus:FugueConvergenceWave"] = new MiscShaderData(ConvergenceAsset, "FugueConvergenceWave");
            GameShaders.Misc["MagnumOpus:FugueConvergenceGlow"] = new MiscShaderData(ConvergenceAsset, "FugueConvergenceGlow");
        }
    }
}
