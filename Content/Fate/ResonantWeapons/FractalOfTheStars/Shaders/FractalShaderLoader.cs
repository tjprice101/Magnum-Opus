using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Fate.ResonantWeapons.FractalOfTheStars.Shaders
{
    /// <summary>
    /// Self-contained shader loader for Fractal of the Stars.
    /// Loads 4 unique shaders from Effects/Fate/FractalOfTheStars/:
    ///
    ///   1. FractalSwingTrail        窶・Main swing arc trail (2 techniques: main + glow)
    ///   2. FractalConstellationTrail 窶・Constellation-line connecting trail
    ///   3. FractalStarFracture      窶・Geometric fractal explosion pattern
    ///   4. FractalOrbitGlow         窶・Glow for orbiting spectral blades
    ///
    /// Keys: "MagnumOpus:Fractal<Purpose>"
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public sealed class FractalShaderLoader : ModSystem
    {
        internal static Asset<Effect> SwingTrailShader;
        internal static Asset<Effect> ConstellationTrailShader;
        internal static Asset<Effect> StarFractureShader;
        internal static Asset<Effect> OrbitGlowShader;

        public static bool HasSwingTrail { get; private set; }
        public static bool HasConstellationTrail { get; private set; }
        public static bool HasStarFracture { get; private set; }
        public static bool HasOrbitGlow { get; private set; }

        private const string BasePath = "MagnumOpus/Effects/Fate/FractalOfTheStars/";

        public override void PostSetupContent()
        {
            if (Main.dedServ) return;

            HasSwingTrail = TryLoad(BasePath + "FractalSwingTrail", "FractalSwingMain",
                "MagnumOpus:FractalSwingTrail", out SwingTrailShader);

            HasConstellationTrail = TryLoad(BasePath + "FractalConstellationTrail", "ConstellationMain",
                "MagnumOpus:FractalConstellationTrail", out ConstellationTrailShader);

            HasStarFracture = TryLoad(BasePath + "FractalStarFracture", "StarFractureMain",
                "MagnumOpus:FractalStarFracture", out StarFractureShader);

            HasOrbitGlow = TryLoad(BasePath + "FractalOrbitGlow", "OrbitGlowMain",
                "MagnumOpus:FractalOrbitGlow", out OrbitGlowShader);

            // Register alternate glow pass from the swing trail shader
            if (HasSwingTrail)
            {
                GameShaders.Misc["MagnumOpus:FractalSwingGlow"] =
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
            GameShaders.Misc.TryGetValue("MagnumOpus:FractalSwingTrail", out var s);
            return s;
        }

        /// <summary>Get the swing glow underlayer shader or null.</summary>
        public static MiscShaderData GetSwingGlow()
        {
            if (!HasSwingTrail) return null;
            GameShaders.Misc.TryGetValue("MagnumOpus:FractalSwingGlow", out var s);
            return s;
        }

        /// <summary>Get the constellation trail shader or null.</summary>
        public static MiscShaderData GetConstellationTrail()
        {
            if (!HasConstellationTrail) return null;
            GameShaders.Misc.TryGetValue("MagnumOpus:FractalConstellationTrail", out var s);
            return s;
        }

        /// <summary>Get the star fracture shader or null.</summary>
        public static MiscShaderData GetStarFracture()
        {
            if (!HasStarFracture) return null;
            GameShaders.Misc.TryGetValue("MagnumOpus:FractalStarFracture", out var s);
            return s;
        }

        /// <summary>Get the orbit glow shader or null.</summary>
        public static MiscShaderData GetOrbitGlow()
        {
            if (!HasOrbitGlow) return null;
            GameShaders.Misc.TryGetValue("MagnumOpus:FractalOrbitGlow", out var s);
            return s;
        }
    }
}
