using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;

namespace MagnumOpus.Content.FoundationWeapons.ThinSlashFoundation
{
    /// <summary>
    /// Enumerates the available slash style modes.
    /// Right-click on the weapon cycles through these.
    /// </summary>
    public enum SlashStyle
    {
        PureWhite = 0,
        IceCyan,
        GoldenEdge,
        VioletCut,
        CrimsonSlice,
        COUNT
    }

    /// <summary>
    /// Self-contained texture registry for ThinSlashFoundation.
    /// All textures sourced from the VFX Asset Library.
    /// </summary>
    internal static class TSFTextures
    {
        // ---- PATHS ----
        private static readonly string Bloom = "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/";
        private static readonly string Masks = "MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/";
        private static readonly string Noise = "MagnumOpus/Assets/VFX Asset Library/NoiseTextures/";

        // ---- BLOOM / GLOW TEXTURES ----
        public static readonly Asset<Texture2D> SoftGlow =
            ModContent.Request<Texture2D>(Bloom + "SoftGlow", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> StarFlare =
            ModContent.Request<Texture2D>(Bloom + "StarFlare", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> PointBloom =
            ModContent.Request<Texture2D>(Bloom + "PointBloom", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> GlowOrb =
            ModContent.Request<Texture2D>(Bloom + "GlowOrb", AssetRequestMode.ImmediateLoad);

        // ---- MASK TEXTURES ----
        public static readonly Asset<Texture2D> SoftCircle =
            ModContent.Request<Texture2D>(Masks + "SoftCircle", AssetRequestMode.ImmediateLoad);

        // ---- NOISE ----
        public static readonly Asset<Texture2D> NoisePerlin =
            ModContent.Request<Texture2D>(Noise + "PerlinNoise", AssetRequestMode.ImmediateLoad);

        /// <summary>
        /// Returns display name for the given slash style.
        /// </summary>
        public static string GetStyleName(SlashStyle style) => style switch
        {
            SlashStyle.PureWhite => "Pure White",
            SlashStyle.IceCyan => "Ice Cyan",
            SlashStyle.GoldenEdge => "Golden Edge",
            SlashStyle.VioletCut => "Violet Cut",
            SlashStyle.CrimsonSlice => "Crimson Slice",
            _ => "Unknown",
        };

        /// <summary>
        /// Returns colors for the given slash style.
        /// Index 0 = edge glow, 1 = mid glow, 2 = core (brightest)
        /// </summary>
        public static Color[] GetStyleColors(SlashStyle style) => style switch
        {
            SlashStyle.PureWhite => new[] {
                new Color(180, 200, 220), new Color(220, 230, 240), new Color(255, 255, 255) },
            SlashStyle.IceCyan => new[] {
                new Color(40, 140, 220), new Color(100, 200, 255), new Color(220, 245, 255) },
            SlashStyle.GoldenEdge => new[] {
                new Color(200, 150, 40), new Color(255, 220, 100), new Color(255, 255, 220) },
            SlashStyle.VioletCut => new[] {
                new Color(120, 40, 200), new Color(180, 100, 255), new Color(230, 200, 255) },
            SlashStyle.CrimsonSlice => new[] {
                new Color(200, 30, 30), new Color(255, 80, 60), new Color(255, 220, 200) },
            _ => new[] { Color.White, Color.White, Color.White },
        };
    }
}
