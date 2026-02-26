using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight.Shaders
{
    /// <summary>
    /// Loads the Incisor's three self-contained shaders into GameShaders.Misc and Filters.Scene.
    /// Completely independent of any shared mod shader infrastructure.
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public sealed class IncisorShaderLoader : ModSystem
    {
        internal static Asset<Effect> SlashShaderAsset;
        internal static Asset<Effect> PierceShaderAsset;
        internal static Asset<Effect> SwingSpriteAsset;

        public override void PostSetupContent()
        {
            var assets = Mod.Assets;
            Asset<Effect> Load(string path) => assets.Request<Effect>(path, AssetRequestMode.ImmediateLoad);

            // Resonance slash arc trail
            SlashShaderAsset = Load("Content/MoonlightSonata/Weapons/IncisorOfMoonlight/Shaders/IncisorSlashShader");
            MiscShaderData slashPass = new(SlashShaderAsset, "IncisorSlashPass");
            GameShaders.Misc["MagnumOpus:IncisorSlash"] = slashPass;

            // Constellation pierce trail (dash)
            PierceShaderAsset = Load("Content/MoonlightSonata/Weapons/IncisorOfMoonlight/Shaders/IncisorPierceShader");
            MiscShaderData piercePass = new(PierceShaderAsset, "IncisorPiercePass");
            GameShaders.Misc["MagnumOpus:IncisorPierce"] = piercePass;

            // Swing sprite rotation
            SwingSpriteAsset = Load("Content/MoonlightSonata/Weapons/IncisorOfMoonlight/Shaders/IncisorSwingSprite");
            ScreenShaderData swingPass = new(SwingSpriteAsset, "IncisorSwingPass");
            Filters.Scene["MagnumOpus:IncisorSwingSprite"] = new Filter(swingPass, EffectPriority.High);
            Filters.Scene["MagnumOpus:IncisorSwingSprite"].Load();

            // Fallback standard primitive
            GameShaders.Misc["MagnumOpus:IncisorStandardPrimitive"] = slashPass;
        }
    }
}
