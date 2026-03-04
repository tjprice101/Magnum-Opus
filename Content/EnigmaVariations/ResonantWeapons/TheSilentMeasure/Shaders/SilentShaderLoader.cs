using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheSilentMeasure.Shaders
{
    /// <summary>
    /// Loads TheSilentMeasure's weapon-specific shaders into GameShaders.Misc.
    /// Each shader is self-contained 窶・no shared Enigma shader manager.
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public sealed class SilentShaderLoader : ModSystem
    {
        internal static Asset<Effect> SeekerTrailAsset;
        internal static Asset<Effect> QuestionBurstAsset;

        public override void PostSetupContent()
        {
            var assets = Mod.Assets;
            Asset<Effect> Load(string path) => assets.Request<Effect>(path, AssetRequestMode.ImmediateLoad);

            // Seeker trail shader 窶・dotted/dashed energy trail for homing seekers
            SeekerTrailAsset = Load("Content/EnigmaVariations/ResonantWeapons/TheSilentMeasure/Shaders/SilentSeekerTrail");
            GameShaders.Misc["MagnumOpus:SilentSeekerFlow"] = new MiscShaderData(SeekerTrailAsset, "P0");

            // Question burst shader 窶・expanding burst with "?" motif + soft glow
            QuestionBurstAsset = Load("Content/EnigmaVariations/ResonantWeapons/TheSilentMeasure/Shaders/SilentQuestionBurst");
            GameShaders.Misc["MagnumOpus:SilentQuestionBlast"] = new MiscShaderData(QuestionBurstAsset, "P0");
            GameShaders.Misc["MagnumOpus:SilentQuestionGlow"] = new MiscShaderData(QuestionBurstAsset, "P0");
        }
    }
}
