using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;

namespace MagnumOpus.Content.FoundationWeapons.ExplosionParticlesFoundation
{
    /// <summary>
    /// Enumerates the three explosion spark modes.
    /// Right-click on the weapon cycles through these.
    /// </summary>
    public enum SparkMode
    {
        RadialScatter = 0,
        FountainCascade,
        SpiralShrapnel,
        COUNT
    }

    /// <summary>
    /// Self-contained texture registry for ExplosionParticlesFoundation.
    /// All textures sourced from the VFX Asset Library.
    /// </summary>
    internal static class EPFTextures
    {
        // ---- PATHS ----
        private static readonly string Bloom = "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/";
        private static readonly string Masks = "MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/";
        private static readonly string Impact = "MagnumOpus/Assets/VFX Asset Library/ImpactEffects/";
        private static readonly string Particles = "MagnumOpus/Assets/Particles Asset Library/Stars/";
        private static readonly string Root = "MagnumOpus/Assets/VFX Asset Library/";

        // ---- BLOOM / GLOW TEXTURES ----
        public static readonly Asset<Texture2D> SoftGlow =
            ModContent.Request<Texture2D>(Bloom + "SoftGlow", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> SoftRadialBloom =
            ModContent.Request<Texture2D>(Bloom + "SoftRadialBloom", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> GlowOrb =
            ModContent.Request<Texture2D>(Bloom + "GlowOrb", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> StarFlare =
            ModContent.Request<Texture2D>(Bloom + "StarFlare", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> PointBloom =
            ModContent.Request<Texture2D>(Bloom + "PointBloom", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> LensFlare =
            ModContent.Request<Texture2D>(Bloom + "LensFlare", AssetRequestMode.ImmediateLoad);

        // ---- MASK TEXTURES ----
        public static readonly Asset<Texture2D> SoftCircle =
            ModContent.Request<Texture2D>(Masks + "SoftCircle", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> WideSoftEllipse =
            ModContent.Request<Texture2D>(Masks + "WideSoftEllipse", AssetRequestMode.ImmediateLoad);

        // ---- SPARK TEXTURES (used as elongated spark shapes) ----
        /// <summary>
        /// SolidWhiteLine — a simple solid white line sprite, perfect for stretching
        /// into elongated spark/debris shapes.
        /// </summary>
        public static readonly Asset<Texture2D> SolidWhiteLine =
            ModContent.Request<Texture2D>(Root + "SolidWhiteLine", AssetRequestMode.ImmediateLoad);

        /// <summary>
        /// 4-pointed star — used for bright spark heads and flash accents.
        /// </summary>
        public static readonly Asset<Texture2D> Star4Hard =
            ModContent.Request<Texture2D>(Particles + "4PointedStarHard", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> Star4Soft =
            ModContent.Request<Texture2D>(Particles + "4PointedStarSoft", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> ThinTallStar =
            ModContent.Request<Texture2D>(Particles + "ThinTall4PointedStar", AssetRequestMode.ImmediateLoad);

        // ---- IMPACT TEXTURES ----
        public static readonly Asset<Texture2D> ImpactEllipse =
            ModContent.Request<Texture2D>(Impact + "ImpactEllipse", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> ImpactCross =
            ModContent.Request<Texture2D>(Impact + "X-ShapedImpactCross", AssetRequestMode.ImmediateLoad);

        /// <summary>
        /// Returns display name for the given spark mode.
        /// </summary>
        public static string GetModeName(SparkMode mode) => mode switch
        {
            SparkMode.RadialScatter => "Radial Scatter",
            SparkMode.FountainCascade => "Fountain Cascade",
            SparkMode.SpiralShrapnel => "Spiral Shrapnel",
            _ => "Unknown",
        };

        /// <summary>
        /// Returns theme-appropriate colors for the given spark mode.
        /// Index 0 = primary outer, 1 = secondary mid, 2 = core/highlight, 3 = dark accent
        /// </summary>
        public static Color[] GetModeColors(SparkMode mode) => mode switch
        {
            SparkMode.RadialScatter => new[] {
                new Color(255, 160, 40),   // Hot orange
                new Color(255, 220, 80),   // Bright gold
                new Color(255, 250, 200),  // White-hot core
                new Color(180, 80, 20),    // Dark ember
            },
            SparkMode.FountainCascade => new[] {
                new Color(60, 180, 255),   // Electric blue
                new Color(140, 220, 255),  // Bright cyan
                new Color(230, 245, 255),  // White-blue core
                new Color(30, 80, 160),    // Deep blue
            },
            SparkMode.SpiralShrapnel => new[] {
                new Color(220, 60, 255),   // Vivid purple
                new Color(255, 120, 220),  // Hot pink
                new Color(255, 230, 255),  // White-magenta core
                new Color(100, 20, 140),   // Dark violet
            },
            _ => new[] { Color.White, Color.LightGray, Color.White, Color.Gray },
        };
    }
}
