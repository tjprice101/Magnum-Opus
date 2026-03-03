using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;

namespace MagnumOpus.Content.FoundationWeapons.SwordSmearFoundation
{
    /// <summary>
    /// The 4 sword arc smear visual styles available in the VFX Asset Library.
    /// Each creates a distinctly different swing arc visual.
    /// </summary>
    public enum SmearStyle
    {
        /// <summary>Fiery curved arc smear with flame styling — intense, aggressive.</summary>
        FlamingSwordArc = 0,

        /// <summary>Clean directional sword arc smear — sharp, precise, versatile.</summary>
        SwordArcSmear,

        COUNT
    }

    /// <summary>
    /// Self-contained texture registry for SwordSmearFoundation.
    /// Loads all 4 SlashArc textures + bloom/glow assets + shader.
    /// </summary>
    internal static class SMFTextures
    {
        // ---- PATHS ----
        private static readonly string SlashArcs = "MagnumOpus/Assets/VFX Asset Library/SlashArcs/";
        private static readonly string Bloom = "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/";
        private static readonly string GradientLib = "MagnumOpus/Assets/VFX Asset Library/ColorGradients/";
        private static readonly string NoiseLib = "MagnumOpus/Assets/VFX Asset Library/NoiseTextures/";

        // ---- SLASH ARC TEXTURES ----
        public static readonly Asset<Texture2D> FlamingSwordArc =
            ModContent.Request<Texture2D>(SlashArcs + "FlamingSwordArcSmear", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> SwordArcSmear =
            ModContent.Request<Texture2D>(SlashArcs + "SwordArcSmear", AssetRequestMode.ImmediateLoad);

        // ---- BLOOM / GLOW ----
        public static readonly Asset<Texture2D> SoftGlow =
            ModContent.Request<Texture2D>(Bloom + "SoftGlow", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> GlowOrb =
            ModContent.Request<Texture2D>(Bloom + "GlowOrb", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> StarFlare =
            ModContent.Request<Texture2D>(Bloom + "StarFlare", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> PointBloom =
            ModContent.Request<Texture2D>(Bloom + "PointBloom", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> SoftRadialBloom =
            ModContent.Request<Texture2D>(Bloom + "SoftRadialBloom", AssetRequestMode.ImmediateLoad);

        // ---- GRADIENT LUTs (for optional theme coloring) ----
        public static readonly Asset<Texture2D> GradMoonlight =
            ModContent.Request<Texture2D>(GradientLib + "MoonlightSonataGradientLUTandRAMP", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> GradEroica =
            ModContent.Request<Texture2D>(GradientLib + "EroicaGradientLUTandRAMP", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> GradLaCampanella =
            ModContent.Request<Texture2D>(GradientLib + "LaCampanellaGradientLUTandRAMP", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> GradSwanLake =
            ModContent.Request<Texture2D>(GradientLib + "SwanLakeGradient", AssetRequestMode.ImmediateLoad);

        // ---- NOISE TEXTURE (for shader distortion) ----
        public static readonly Asset<Texture2D> FBMNoise =
            ModContent.Request<Texture2D>(NoiseLib + "TileableFBMNoise", AssetRequestMode.ImmediateLoad);

        // ---- DISTORTION SHADER ----
        private static Effect _smearShader;
        private static bool _shaderLoaded;

        /// <summary>
        /// Lazy-loaded SmearDistortShader. Returns null if shader failed to load.
        /// </summary>
        public static Effect SmearDistortShader
        {
            get
            {
                if (!_shaderLoaded)
                {
                    _shaderLoaded = true;
                    try
                    {
                        _smearShader = ModContent.Request<Effect>(
                            "MagnumOpus/Content/FoundationWeapons/SwordSmearFoundation/Shaders/SmearDistortShader",
                            AssetRequestMode.ImmediateLoad).Value;
                    }
                    catch { _smearShader = null; }
                }
                return _smearShader;
            }
        }

        // ---- HELPERS ----

        public static Texture2D GetSmearTexture(SmearStyle style) => style switch
        {
            SmearStyle.FlamingSwordArc => FlamingSwordArc.Value,
            SmearStyle.SwordArcSmear => SwordArcSmear.Value,
            _ => FlamingSwordArc.Value,
        };

        public static string GetStyleName(SmearStyle style) => style switch
        {
            SmearStyle.FlamingSwordArc => "Flaming Arc",
            SmearStyle.SwordArcSmear => "Sword Arc Smear",
            _ => "Unknown",
        };

        /// <summary>
        /// Each smear style has its own 3-color palette (outer, mid, core)
        /// to visually distinguish them even without per-style shaders.
        /// </summary>
        public static Color[] GetStyleColors(SmearStyle style) => style switch
        {
            SmearStyle.FlamingSwordArc => new[] {
                new Color(255, 100, 20),   // Deep orange outer
                new Color(255, 180, 60),   // Bright amber mid
                new Color(255, 240, 180),  // White-hot core
            },
            SmearStyle.SwordArcSmear => new[] {
                new Color(60, 160, 255),   // Blue outer
                new Color(140, 200, 255),  // Sky-blue mid
                new Color(230, 240, 255),  // White-blue core
            },
            _ => new[] { Color.White, Color.White, Color.White },
        };

        /// <summary>
        /// Maps each smear style to a matching gradient LUT for shader coloring.
        /// </summary>
        public static Asset<Texture2D> GetGradientForStyle(SmearStyle style) => style switch
        {
            SmearStyle.FlamingSwordArc => GradLaCampanella,
            SmearStyle.SwordArcSmear => GradSwanLake,
            _ => GradLaCampanella,
        };
    }
}
