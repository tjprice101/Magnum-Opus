using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.ArbitersSentence
{
    /// <summary>
    /// Loads shaders for Arbiter's Sentence (Ranged).
    /// Uses theme-wide Dies Irae shaders plus Foundation ranged weapon patterns.
    /// 
    /// Registered shaders:
    ///   1. MagnumOpus:ArbiterHellfireBloom     — Theme-wide HellfireBloom for muzzle/impact flare
    ///   2. MagnumOpus:ArbiterJudgmentAura      — Theme-wide JudgmentAura for charged shots
    ///   3. MagnumOpus:ArbiterSparkleTrail      — Foundation SparkleTrailShader for bullet trails
    ///   4. MagnumOpus:ArbiterRipple            — Foundation RippleShader for impact ripples
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public sealed class ArbitersSentenceShaderLoader : ModSystem
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
                GameShaders.Misc["MagnumOpus:ArbiterHellfireBloom"] = new MiscShaderData(HellfireBloomShader, "P0");
            if (JudgmentAuraShader?.Value != null)
                GameShaders.Misc["MagnumOpus:ArbiterJudgmentAura"] = new MiscShaderData(JudgmentAuraShader, "P0");
            if (SparkleTrailShader?.Value != null)
                GameShaders.Misc["MagnumOpus:ArbiterSparkleTrail"] = new MiscShaderData(SparkleTrailShader, "P0");
            if (RippleShader?.Value != null)
                GameShaders.Misc["MagnumOpus:ArbiterRipple"] = new MiscShaderData(RippleShader, "P0");
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
