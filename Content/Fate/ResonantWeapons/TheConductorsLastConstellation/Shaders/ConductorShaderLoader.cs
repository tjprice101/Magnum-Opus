using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Fate.ResonantWeapons.TheConductorsLastConstellation.Shaders
{
    /// <summary>
    /// Self-contained shader loader for The Conductor's Last Constellation.
    /// Loads 4 unique shaders from Effects/Fate/TheConductorsLastConstellation/:
    ///
    ///   1. ConductorSwingTrail      窶・Main swing arc trail (2 techniques: main + glow)
    ///   2. ConductorBeamShader      窶・Beam rendering for homing sword beams
    ///   3. ConductorLightningShader 窶・Lightning cascade electric effect
    ///   4. ConductorConvergence     窶・Convergence explosion on 3rd combo
    ///
    /// Keys (5 total, because swing trail has 2 techniques):
    ///   "MagnumOpus:ConductorSwingTrail"
    ///   "MagnumOpus:ConductorSwingGlow"
    ///   "MagnumOpus:ConductorBeamShader"
    ///   "MagnumOpus:ConductorLightningShader"
    ///   "MagnumOpus:ConductorConvergence"
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public sealed class ConductorShaderLoader : ModSystem
    {
        internal static Asset<Effect> SwingTrailShader;
        internal static Asset<Effect> BeamShader;
        internal static Asset<Effect> LightningShader;
        internal static Asset<Effect> ConvergenceShader;

        public static bool HasSwingTrail { get; private set; }
        public static bool HasBeam { get; private set; }
        public static bool HasLightning { get; private set; }
        public static bool HasConvergence { get; private set; }

        private const string BasePath = "MagnumOpus/Effects/Fate/TheConductorsLastConstellation/";

        public override void PostSetupContent()
        {
            if (Main.dedServ) return;

            HasSwingTrail = TryLoad(BasePath + "ConductorSwingTrail", "ConductorSwingMain",
                "MagnumOpus:ConductorSwingTrail", out SwingTrailShader);

            HasBeam = TryLoad(BasePath + "ConductorBeamShader", "ConductorBeamMain",
                "MagnumOpus:ConductorBeamShader", out BeamShader);

            HasLightning = TryLoad(BasePath + "ConductorLightningShader", "ConductorLightningMain",
                "MagnumOpus:ConductorLightningShader", out LightningShader);

            HasConvergence = TryLoad(BasePath + "ConductorConvergence", "ConductorConvergenceMain",
                "MagnumOpus:ConductorConvergence", out ConvergenceShader);

            // Register alternate glow pass from the swing trail shader
            if (HasSwingTrail)
            {
                GameShaders.Misc["MagnumOpus:ConductorSwingGlow"] =
                    new MiscShaderData(SwingTrailShader, "P0");
            }
        }

        private static bool TryLoad(string path, string passName, string key, out Asset<Effect> asset)
        {
            asset = null;
            try
            {
                asset = ModContent.Request<Effect>(path, AssetRequestMode.ImmediateLoad);
                if (asset?.Value != null)
                {
                    GameShaders.Misc[key] = new MiscShaderData(asset, passName);
                    return true;
                }
            }
            catch { }

            // Fallback: try shared scrolling trail shader
            try
            {
                var fallback = ModContent.Request<Effect>("MagnumOpus/Effects/ScrollingTrailShader", AssetRequestMode.ImmediateLoad);
                if (fallback?.Value != null)
                {
                    GameShaders.Misc[key] = new MiscShaderData(fallback, "P0");
                    return true;
                }
            }
            catch { }

            return false;
        }

        /// <summary>Get the swing trail shader (main pass) or null.</summary>
        public static MiscShaderData GetSwingTrail()
        {
            if (!HasSwingTrail) return null;
            GameShaders.Misc.TryGetValue("MagnumOpus:ConductorSwingTrail", out var s);
            return s;
        }

        /// <summary>Get the swing glow underlayer shader or null.</summary>
        public static MiscShaderData GetSwingGlow()
        {
            if (!HasSwingTrail) return null;
            GameShaders.Misc.TryGetValue("MagnumOpus:ConductorSwingGlow", out var s);
            return s;
        }

        /// <summary>Get the beam shader or null.</summary>
        public static MiscShaderData GetBeamShader()
        {
            if (!HasBeam) return null;
            GameShaders.Misc.TryGetValue("MagnumOpus:ConductorBeamShader", out var s);
            return s;
        }

        /// <summary>Get the lightning shader or null.</summary>
        public static MiscShaderData GetLightningShader()
        {
            if (!HasLightning) return null;
            GameShaders.Misc.TryGetValue("MagnumOpus:ConductorLightningShader", out var s);
            return s;
        }

        /// <summary>Get the convergence explosion shader or null.</summary>
        public static MiscShaderData GetConvergence()
        {
            if (!HasConvergence) return null;
            GameShaders.Misc.TryGetValue("MagnumOpus:ConductorConvergence", out var s);
            return s;
        }
    }
}
