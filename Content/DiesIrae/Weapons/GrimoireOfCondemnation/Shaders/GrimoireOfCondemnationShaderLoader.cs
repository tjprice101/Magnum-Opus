using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.GrimoireOfCondemnation.Shaders
{
    /// <summary>
    /// Loads shaders for Grimoire of Condemnation (Magic).
    /// No custom .fx exists — uses theme-wide Dies Irae + Foundation magic patterns.
    /// 
    /// Registered shaders:
    ///   1. MagnumOpus:GrimoireHellfireBloom      — Theme-wide HellfireBloom for projectile bloom
    ///   2. MagnumOpus:GrimoireJudgmentAura       — Theme-wide JudgmentAura for blazing shard aura
    ///   3. MagnumOpus:GrimoireCrystalShimmer     — Foundation CrystalShimmerShader for shard glint
    ///   4. MagnumOpus:GrimoireSparkleTrail       — Foundation SparkleTrailShader for shard trails
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public sealed class GrimoireOfCondemnationShaderLoader : ModSystem
    {
        internal static Asset<Effect> HellfireBloomShader;
        internal static Asset<Effect> JudgmentAuraShader;
        internal static Asset<Effect> CrystalShimmerShader;
        internal static Asset<Effect> SparkleTrailShader;

        public override void PostSetupContent()
        {
            if (Main.dedServ) return;

            HellfireBloomShader = Mod.Assets.Request<Effect>(
                "Effects/DiesIrae/HellfireBloom", AssetRequestMode.ImmediateLoad);
            JudgmentAuraShader = Mod.Assets.Request<Effect>(
                "Effects/DiesIrae/JudgmentAura", AssetRequestMode.ImmediateLoad);
            CrystalShimmerShader = ModContent.Request<Effect>(
                "MagnumOpus/Content/FoundationWeapons/SparkleProjectileFoundation/Shaders/CrystalShimmerShader",
                AssetRequestMode.ImmediateLoad);
            SparkleTrailShader = ModContent.Request<Effect>(
                "MagnumOpus/Content/FoundationWeapons/SparkleProjectileFoundation/Shaders/SparkleTrailShader",
                AssetRequestMode.ImmediateLoad);

            if (HellfireBloomShader?.Value != null)
                GameShaders.Misc["MagnumOpus:GrimoireHellfireBloom"] = new MiscShaderData(HellfireBloomShader, "P0");
            if (JudgmentAuraShader?.Value != null)
                GameShaders.Misc["MagnumOpus:GrimoireJudgmentAura"] = new MiscShaderData(JudgmentAuraShader, "P0");
            if (CrystalShimmerShader?.Value != null)
                GameShaders.Misc["MagnumOpus:GrimoireCrystalShimmer"] = new MiscShaderData(CrystalShimmerShader, "P0");
            if (SparkleTrailShader?.Value != null)
                GameShaders.Misc["MagnumOpus:GrimoireSparkleTrail"] = new MiscShaderData(SparkleTrailShader, "P0");
        }

        public override void Unload()
        {
            HellfireBloomShader = null;
            JudgmentAuraShader = null;
            CrystalShimmerShader = null;
            SparkleTrailShader = null;
        }
    }
}
