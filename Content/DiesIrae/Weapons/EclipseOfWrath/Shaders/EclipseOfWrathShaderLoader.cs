using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.EclipseOfWrath
{
    /// <summary>
    /// Loads and registers all shaders for Eclipse of Wrath.
    /// Custom .fx from Effects/DiesIrae/EclipseOfWrath/ plus Foundation shaders.
    /// 
    /// Registered shaders:
    ///   1. MagnumOpus:EclipseOrbShader          — Custom eclipse orb rendering
    ///   2. MagnumOpus:EclipseRadialNoiseMask    — Foundation RadialNoiseMaskShader for eclipse field
    ///   3. MagnumOpus:EclipseCrystalShimmer     — Foundation CrystalShimmerShader for shard accents
    ///   4. MagnumOpus:EclipseSparkleTrail       — Foundation SparkleTrailShader for projectile trails
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public sealed class EclipseOfWrathShaderLoader : ModSystem
    {
        internal static Asset<Effect> EclipseOrbShader;
        internal static Asset<Effect> RadialNoiseMaskShader;
        internal static Asset<Effect> CrystalShimmerShader;
        internal static Asset<Effect> SparkleTrailShader;

        public override void PostSetupContent()
        {
            if (Main.dedServ) return;

            EclipseOrbShader = Mod.Assets.Request<Effect>(
                "Effects/DiesIrae/EclipseOfWrath/EclipseOrb", AssetRequestMode.ImmediateLoad);

            if (EclipseOrbShader?.Value != null)
                GameShaders.Misc["MagnumOpus:EclipseOrbShader"] = new MiscShaderData(EclipseOrbShader, "P0");

            RadialNoiseMaskShader = ModContent.Request<Effect>(
                "MagnumOpus/Content/FoundationWeapons/MaskFoundation/Shaders/RadialNoiseMaskShader",
                AssetRequestMode.ImmediateLoad);
            CrystalShimmerShader = ModContent.Request<Effect>(
                "MagnumOpus/Content/FoundationWeapons/SparkleProjectileFoundation/Shaders/CrystalShimmerShader",
                AssetRequestMode.ImmediateLoad);
            SparkleTrailShader = ModContent.Request<Effect>(
                "MagnumOpus/Content/FoundationWeapons/SparkleProjectileFoundation/Shaders/SparkleTrailShader",
                AssetRequestMode.ImmediateLoad);

            if (RadialNoiseMaskShader?.Value != null)
                GameShaders.Misc["MagnumOpus:EclipseRadialNoiseMask"] = new MiscShaderData(RadialNoiseMaskShader, "P0");
            if (CrystalShimmerShader?.Value != null)
                GameShaders.Misc["MagnumOpus:EclipseCrystalShimmer"] = new MiscShaderData(CrystalShimmerShader, "P0");
            if (SparkleTrailShader?.Value != null)
                GameShaders.Misc["MagnumOpus:EclipseSparkleTrail"] = new MiscShaderData(SparkleTrailShader, "P0");
        }

        public override void Unload()
        {
            EclipseOrbShader = null;
            RadialNoiseMaskShader = null;
            CrystalShimmerShader = null;
            SparkleTrailShader = null;
        }
    }
}
