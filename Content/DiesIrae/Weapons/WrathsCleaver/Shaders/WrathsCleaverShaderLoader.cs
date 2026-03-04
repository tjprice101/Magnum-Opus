using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.WrathsCleaver
{
    /// <summary>
    /// Loads and registers all shaders for Wrath's Cleaver.
    /// Custom .fx shaders from Effects/DiesIrae/WrathsCleaver/ plus Foundation shaders.
    /// 
    /// Registered shaders:
    ///   1. MagnumOpus:WrathInfernoTrail      — Dual-noise fire trail with heat-mapped color
    ///   2. MagnumOpus:WrathInfernoEmbers      — Sporadic ember points along trail
    ///   3. MagnumOpus:WrathCleaverSlash        — Noise-distorted fire slash arc
    ///   4. MagnumOpus:WrathCleaverSlashGlow    — Softer bloom layer for slash overlay
    ///   5. MagnumOpus:WrathSmearDistort        — Foundation SmearDistortShader for swing arc
    ///   6. MagnumOpus:WrathRippleEffect        — Foundation RippleShader for impact ripples
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public sealed class WrathsCleaverShaderLoader : ModSystem
    {
        // Custom per-weapon shaders
        internal static Asset<Effect> InfernoTrailShader;
        internal static Asset<Effect> WrathSlashShader;

        // Foundation shaders (reused)
        internal static Asset<Effect> SmearDistortShader;
        internal static Asset<Effect> RippleShader;

        public override void PostSetupContent()
        {
            if (Main.dedServ) return;

            // --- Custom weapon shaders ---
            InfernoTrailShader = Mod.Assets.Request<Effect>(
                "Effects/DiesIrae/WrathsCleaver/InfernoTrail", AssetRequestMode.ImmediateLoad);

            WrathSlashShader = Mod.Assets.Request<Effect>(
                "Effects/DiesIrae/WrathsCleaver/WrathCleaverSlash", AssetRequestMode.ImmediateLoad);

            // Register into GameShaders.Misc with namespaced keys
            if (InfernoTrailShader?.Value != null)
            {
                GameShaders.Misc["MagnumOpus:WrathInfernoTrail"] =
                    new MiscShaderData(InfernoTrailShader, "InfernoMain");
                GameShaders.Misc["MagnumOpus:WrathInfernoEmbers"] =
                    new MiscShaderData(InfernoTrailShader, "InfernoEmbers");
            }

            if (WrathSlashShader?.Value != null)
            {
                GameShaders.Misc["MagnumOpus:WrathCleaverSlash"] =
                    new MiscShaderData(WrathSlashShader, "WrathSlashMain");
                GameShaders.Misc["MagnumOpus:WrathCleaverSlashGlow"] =
                    new MiscShaderData(WrathSlashShader, "WrathSlashGlow");
            }

            // --- Foundation shaders (scaffolded rendering techniques) ---
            SmearDistortShader = ModContent.Request<Effect>(
                "MagnumOpus/Content/FoundationWeapons/SwordSmearFoundation/Shaders/SmearDistortShader",
                AssetRequestMode.ImmediateLoad);

            RippleShader = ModContent.Request<Effect>(
                "MagnumOpus/Content/FoundationWeapons/ImpactFoundation/Shaders/RippleShader",
                AssetRequestMode.ImmediateLoad);

            if (SmearDistortShader?.Value != null)
            {
                GameShaders.Misc["MagnumOpus:WrathSmearDistort"] =
                    new MiscShaderData(SmearDistortShader, "P0");
            }

            if (RippleShader?.Value != null)
            {
                GameShaders.Misc["MagnumOpus:WrathRippleEffect"] =
                    new MiscShaderData(RippleShader, "P0");
            }
        }

        public override void Unload()
        {
            InfernoTrailShader = null;
            WrathSlashShader = null;
            SmearDistortShader = null;
            RippleShader = null;
        }
    }
}
