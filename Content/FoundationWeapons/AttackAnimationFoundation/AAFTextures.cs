using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;

namespace MagnumOpus.Content.FoundationWeapons.AttackAnimationFoundation
{
    /// <summary>
    /// Self-contained texture registry for AttackAnimationFoundation.
    /// All textures sourced from the VFX Asset Library.
    /// </summary>
    internal static class AAFTextures
    {
        // ---- PATHS ----
        private static readonly string Noise = "MagnumOpus/Assets/VFX Asset Library/NoiseTextures/";
        private static readonly string Bloom = "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/";
        private static readonly string Masks = "MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/";

        // ---- NOISE TEXTURES ----
        public static readonly Asset<Texture2D> NoiseFBM =
            ModContent.Request<Texture2D>(Noise + "TileableFBMNoise", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> NoisePerlin =
            ModContent.Request<Texture2D>(Noise + "PerlinNoise", AssetRequestMode.ImmediateLoad);

        // ---- BLOOM / GLOW TEXTURES ----
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

        public static readonly Asset<Texture2D> LensFlare =
            ModContent.Request<Texture2D>(Bloom + "LensFlare", AssetRequestMode.ImmediateLoad);

        // ---- MASK TEXTURES ----
        public static readonly Asset<Texture2D> SoftCircle =
            ModContent.Request<Texture2D>(Masks + "SoftCircle", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> HardCircleMask =
            ModContent.Request<Texture2D>(Masks + "HardCircleMask", AssetRequestMode.ImmediateLoad);

        // ---- SLASH COLORS ----
        /// <summary>
        /// Colors used for the slash effects.
        /// Index 0 = outer trail, 1 = mid slash, 2 = core flash
        /// </summary>
        public static readonly Color[] SlashColors = new[]
        {
            new Color(180, 200, 255),   // Ice-blue outer
            new Color(220, 230, 255),   // Bright blue-white mid
            new Color(255, 255, 255),   // Pure white core
        };

        /// <summary>
        /// Colors for the noise zone that builds on hit.
        /// </summary>
        public static readonly Color[] ZoneColors = new[]
        {
            new Color(80, 60, 180),     // Deep indigo
            new Color(150, 120, 255),   // Violet
            new Color(255, 220, 255),   // Hot white-pink core
        };
    }
}
