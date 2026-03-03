using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;

namespace MagnumOpus.Content.FoundationWeapons.XSlashFoundation
{
    /// <summary>
    /// Enumerates the available X-slash color styles.
    /// Right-click on the weapon cycles through these.
    /// </summary>
    public enum XSlashStyle
    {
        MoonlightSonata = 0,
        Eroica,
        LaCampanella,
        Enigma,
        Fate,
        SwanLake,
        COUNT
    }

    /// <summary>
    /// Self-contained texture registry for XSlashFoundation.
    /// All textures sourced from the VFX Asset Library.
    /// </summary>
    internal static class XSFTextures
    {
        // ---- PATHS ----
        private static readonly string Bloom = "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/";
        private static readonly string Impacts = "MagnumOpus/Assets/VFX Asset Library/ImpactEffects/";
        private static readonly string Noise = "MagnumOpus/Assets/VFX Asset Library/NoiseTextures/";
        private static readonly string GradientLib = "MagnumOpus/Assets/VFX Asset Library/ColorGradients/";
        private static readonly string Masks = "MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/";

        // ---- IMPACT TEXTURES ----
        public static readonly Asset<Texture2D> XImpactCross =
            ModContent.Request<Texture2D>(Impacts + "X-ShapedImpactCross", AssetRequestMode.ImmediateLoad);

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

        // ---- MASK TEXTURES ----
        public static readonly Asset<Texture2D> SoftCircle =
            ModContent.Request<Texture2D>(Masks + "SoftCircle", AssetRequestMode.ImmediateLoad);

        // ---- NOISE TEXTURES ----
        public static readonly Asset<Texture2D> NoiseFBM =
            ModContent.Request<Texture2D>(Noise + "TileableFBMNoise", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> NoiseCosmicVortex =
            ModContent.Request<Texture2D>(Noise + "CosmicEnergyVortex", AssetRequestMode.ImmediateLoad);

        // ---- GRADIENT LUT TEXTURES ----
        public static readonly Asset<Texture2D> GradMoonlight =
            ModContent.Request<Texture2D>(GradientLib + "MoonlightSonataGradientLUTandRAMP", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> GradEroica =
            ModContent.Request<Texture2D>(GradientLib + "EroicaGradientLUTandRAMP", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> GradLaCampanella =
            ModContent.Request<Texture2D>(GradientLib + "LaCampanellaGradientLUTandRAMP", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> GradEnigma =
            ModContent.Request<Texture2D>(GradientLib + "EnigmaGradientLUTandRAMP", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> GradFate =
            ModContent.Request<Texture2D>(GradientLib + "FateGradientLUTandRAMP", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> GradSwanLake =
            ModContent.Request<Texture2D>(GradientLib + "SwanLakeGradient", AssetRequestMode.ImmediateLoad);

        /// <summary>
        /// Returns display name for the given X-slash style.
        /// </summary>
        public static string GetStyleName(XSlashStyle style) => style switch
        {
            XSlashStyle.MoonlightSonata => "Moonlight Sonata",
            XSlashStyle.Eroica => "Eroica",
            XSlashStyle.LaCampanella => "La Campanella",
            XSlashStyle.Enigma => "Enigma Variations",
            XSlashStyle.Fate => "Fate",
            XSlashStyle.SwanLake => "Swan Lake",
            _ => "Unknown",
        };

        /// <summary>
        /// Returns colors for the given X-slash style.
        /// Index 0 = outer glow, 1 = mid glow, 2 = core (brightest)
        /// </summary>
        public static Color[] GetStyleColors(XSlashStyle style) => style switch
        {
            XSlashStyle.MoonlightSonata => new[] {
                new Color(140, 100, 200), new Color(100, 150, 255), new Color(220, 200, 255) },
            XSlashStyle.Eroica => new[] {
                new Color(200, 50, 50), new Color(255, 120, 80), new Color(255, 230, 200) },
            XSlashStyle.LaCampanella => new[] {
                new Color(255, 140, 40), new Color(255, 200, 60), new Color(255, 245, 200) },
            XSlashStyle.Enigma => new[] {
                new Color(140, 60, 200), new Color(60, 200, 80), new Color(220, 255, 230) },
            XSlashStyle.Fate => new[] {
                new Color(180, 40, 80), new Color(220, 60, 100), new Color(255, 220, 240) },
            XSlashStyle.SwanLake => new[] {
                new Color(200, 200, 220), new Color(240, 240, 255), new Color(255, 255, 255) },
            _ => new[] { Color.White, Color.White, Color.White },
        };

        /// <summary>
        /// Returns gradient LUT for the given style.
        /// </summary>
        public static Texture2D GetGradientForStyle(XSlashStyle style) => style switch
        {
            XSlashStyle.MoonlightSonata => GradMoonlight.Value,
            XSlashStyle.Eroica => GradEroica.Value,
            XSlashStyle.LaCampanella => GradLaCampanella.Value,
            XSlashStyle.Enigma => GradEnigma.Value,
            XSlashStyle.Fate => GradFate.Value,
            XSlashStyle.SwanLake => GradSwanLake.Value,
            _ => GradMoonlight.Value,
        };
    }
}
