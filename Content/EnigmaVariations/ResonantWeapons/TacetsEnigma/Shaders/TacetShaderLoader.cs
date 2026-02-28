using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TacetsEnigma.Shaders
{
    /// <summary>
    /// Loads TacetsEnigma's weapon-specific shaders into GameShaders.Misc.
    /// Each shader is self-contained — no shared Enigma shader manager.
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public sealed class TacetShaderLoader : ModSystem
    {
        internal static Asset<Effect> BulletTrailAsset;
        internal static Asset<Effect> ParadoxExplosionAsset;

        public override void PostSetupContent()
        {
            var assets = Mod.Assets;
            Asset<Effect> Load(string path) => assets.Request<Effect>(path, AssetRequestMode.ImmediateLoad);

            // Bullet trail shader — technique: TacetBulletFlow (fast-scrolling energy trail for bullets and paradox bolts)
            BulletTrailAsset = Load("Content/EnigmaVariations/ResonantWeapons/TacetsEnigma/Shaders/TacetBulletTrail");
            GameShaders.Misc["MagnumOpus:TacetBulletFlow"] = new MiscShaderData(BulletTrailAsset, "TacetBulletFlow");

            // Paradox explosion shader — two techniques: TacetParadoxBlast (shockwave) and TacetParadoxRing (edge ring)
            ParadoxExplosionAsset = Load("Content/EnigmaVariations/ResonantWeapons/TacetsEnigma/Shaders/TacetParadoxExplosion");
            GameShaders.Misc["MagnumOpus:TacetParadoxBlast"] = new MiscShaderData(ParadoxExplosionAsset, "TacetParadoxBlast");
            GameShaders.Misc["MagnumOpus:TacetParadoxRing"] = new MiscShaderData(ParadoxExplosionAsset, "TacetParadoxRing");
        }
    }
}
