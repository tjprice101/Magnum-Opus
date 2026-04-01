using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheWatchingRefrain.Shaders
{
    /// <summary>
    /// Loads TheWatchingRefrain's weapon-specific shaders into GameShaders.Misc.
    /// Each shader is self-contained  -- no shared Enigma shader manager.
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public sealed class WatchingShaderLoader : ModSystem
    {
        internal static Asset<Effect> PhantomAuraAsset;
        internal static Asset<Effect> MysteryZoneAsset;

        public override void PostSetupContent()
        {
            var assets = Mod.Assets;
            Asset<Effect> Load(string path) => assets.Request<Effect>(path, AssetRequestMode.ImmediateLoad);

            // Phantom aura shader  -- two techniques: WatchingPhantomGhost (spectral trail) and WatchingPhantomGlow (bloom)
            PhantomAuraAsset = Load("Content/EnigmaVariations/ResonantWeapons/TheWatchingRefrain/Shaders/WatchingPhantomAura");
            GameShaders.Misc["MagnumOpus:WatchingPhantomGhost"] = new MiscShaderData(PhantomAuraAsset, "P0");
            GameShaders.Misc["MagnumOpus:WatchingPhantomGlow"] = new MiscShaderData(PhantomAuraAsset, "P0");

            // Mystery zone area shader
            MysteryZoneAsset = Load("Content/EnigmaVariations/ResonantWeapons/TheWatchingRefrain/Shaders/WatchingMysteryZone");
            GameShaders.Misc["MagnumOpus:WatchingMysteryField"] = new MiscShaderData(MysteryZoneAsset, "P0");
        }
    }
}
