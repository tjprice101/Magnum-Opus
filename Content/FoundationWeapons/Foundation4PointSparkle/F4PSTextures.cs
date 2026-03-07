using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;

namespace MagnumOpus.Content.FoundationWeapons.Foundation4PointSparkle
{
    /// <summary>
    /// Enumerates the two firing modes:
    /// - Normal: Ball projectile that explodes into dazzling 4-point sparkles on impact
    /// - SparkleTrail: Same ball but leaves a twinkling sparkle trail as it flies
    /// </summary>
    public enum SparkleFireMode
    {
        Normal = 0,
        SparkleTrail,
        COUNT
    }

    /// <summary>
    /// Self-contained texture registry for Foundation4PointSparkle.
    /// All textures sourced from the VFX Asset Library.
    /// 
    /// Star assets used:
    /// - 4PointStarShiningProjectile — Main projectile body and primary sparkle shape
    /// - 8-Point Starburst Flare — Large dramatic starburst for impact center flash
    /// - BrightStarProjectile1 — Medium sparkle for explosion field
    /// - BrightStarProjectile2 — Counter-rotated overlay sparkle for depth
    /// </summary>
    internal static class F4PSTextures
    {
        // ---- PATHS ----
        private static readonly string Projectiles = "MagnumOpus/Assets/VFX Asset Library/Projectiles/";
        private static readonly string Bloom = "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/";
        private static readonly string Impact = "MagnumOpus/Assets/VFX Asset Library/ImpactEffects/";
        private static readonly string Masks = "MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/";

        // ---- STAR / PROJECTILE TEXTURES (as specified by user) ----

        /// <summary>4-point shining star — main projectile body and primary sparkle shape.</summary>
        public static readonly Asset<Texture2D> Star4Point =
            ModContent.Request<Texture2D>(Projectiles + "4PointStarShiningProjectile", AssetRequestMode.ImmediateLoad);

        /// <summary>8-point starburst flare — large dramatic starburst for impact center flash.</summary>
        public static readonly Asset<Texture2D> StarburstFlare =
            ModContent.Request<Texture2D>(Projectiles + "8-Point Starburst Flare", AssetRequestMode.ImmediateLoad);

        /// <summary>Bright star 1 — medium sparkle for explosion field particles.</summary>
        public static readonly Asset<Texture2D> BrightStar1 =
            ModContent.Request<Texture2D>(Projectiles + "BrightStarProjectile1", AssetRequestMode.ImmediateLoad);

        /// <summary>Bright star 2 — counter-rotated overlay sparkle for depth layering.</summary>
        public static readonly Asset<Texture2D> BrightStar2 =
            ModContent.Request<Texture2D>(Projectiles + "BrightStarProjectile2", AssetRequestMode.ImmediateLoad);

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

        // ---- IMPACT TEXTURES ----

        public static readonly Asset<Texture2D> PowerEffectRing =
            ModContent.Request<Texture2D>(Impact + "PowerEffectRing", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> ImpactCross =
            ModContent.Request<Texture2D>(Impact + "X-ShapedImpactCross", AssetRequestMode.ImmediateLoad);

        // ---- MASK TEXTURES ----

        public static readonly Asset<Texture2D> SoftCircle =
            ModContent.Request<Texture2D>(Masks + "SoftCircle", AssetRequestMode.ImmediateLoad);

        /// <summary>
        /// Returns display name for the given fire mode.
        /// </summary>
        public static string GetModeName(SparkleFireMode mode) => mode switch
        {
            SparkleFireMode.Normal => "Impact Sparkle",
            SparkleFireMode.SparkleTrail => "Twinkling Trail",
            _ => "Unknown",
        };

        /// <summary>
        /// Returns theme colors for sparkle effects.
        /// A dazzling rainbow-prismatic palette since this is a foundation demo.
        /// Index 0 = primary, 1 = secondary, 2 = core/highlight, 3 = accent, 4 = warm accent
        /// </summary>
        public static Color[] GetSparkleColors() => new[]
        {
            new Color(180, 220, 255),  // Cool white-blue (primary)
            new Color(255, 200, 100),  // Warm gold (secondary)
            new Color(255, 255, 255),  // Pure white (core)
            new Color(160, 120, 255),  // Soft lavender (accent)
            new Color(255, 160, 200),  // Soft pink (warm accent)
        };
    }
}
