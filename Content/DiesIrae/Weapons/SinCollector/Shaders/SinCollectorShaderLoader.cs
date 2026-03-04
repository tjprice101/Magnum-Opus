using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.SinCollector.Shaders
{
    /// <summary>
    /// Loads shaders for Sin Collector (Ranged).
    /// No custom .fx exists — uses theme-wide Dies Irae + Foundation ranged patterns.
    /// 
    /// Registered shaders:
    ///   1. MagnumOpus:SinCollectorHellfireBloom  — Theme-wide HellfireBloom for muzzle flash / bullet bloom
    ///   2. MagnumOpus:SinCollectorJudgmentAura   — Theme-wide JudgmentAura for sin-charged aura
    ///   3. MagnumOpus:SinCollectorSparkleTrail   — Foundation SparkleTrailShader for bullet trails
    ///   4. MagnumOpus:SinCollectorRipple         — Foundation RippleShader for bullet impacts
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public sealed class SinCollectorShaderLoader : ModSystem
    {
        internal static Asset<Effect> HellfireBloomShader;
        internal static Asset<Effect> JudgmentAuraShader;
        internal static Asset<Effect> SparkleTrailShader;
        internal static Asset<Effect> RippleShader;

        public override void PostSetupContent()
        {
            if (Main.dedServ) return;

            HellfireBloomShader = Mod.Assets.Request<Effect>(
                "Effects/DiesIrae/HellfireBloom", AssetRequestMode.ImmediateLoad);
            JudgmentAuraShader = Mod.Assets.Request<Effect>(
                "Effects/DiesIrae/JudgmentAura", AssetRequestMode.ImmediateLoad);
            SparkleTrailShader = ModContent.Request<Effect>(
                "MagnumOpus/Content/FoundationWeapons/SparkleProjectileFoundation/Shaders/SparkleTrailShader",
                AssetRequestMode.ImmediateLoad);
            RippleShader = ModContent.Request<Effect>(
                "MagnumOpus/Content/FoundationWeapons/ImpactFoundation/Shaders/RippleShader",
                AssetRequestMode.ImmediateLoad);

            if (HellfireBloomShader?.Value != null)
                GameShaders.Misc["MagnumOpus:SinCollectorHellfireBloom"] = new MiscShaderData(HellfireBloomShader, "P0");
            if (JudgmentAuraShader?.Value != null)
                GameShaders.Misc["MagnumOpus:SinCollectorJudgmentAura"] = new MiscShaderData(JudgmentAuraShader, "P0");
            if (SparkleTrailShader?.Value != null)
                GameShaders.Misc["MagnumOpus:SinCollectorSparkleTrail"] = new MiscShaderData(SparkleTrailShader, "P0");
            if (RippleShader?.Value != null)
                GameShaders.Misc["MagnumOpus:SinCollectorRipple"] = new MiscShaderData(RippleShader, "P0");
        }

        public override void Unload()
        {
            HellfireBloomShader = null;
            JudgmentAuraShader = null;
            SparkleTrailShader = null;
            RippleShader = null;
        }
    }
}
