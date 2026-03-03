using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;

namespace MagnumOpus.Content.FoundationWeapons.SparkleProjectileFoundation
{
    /// <summary>
    /// Enumerates the available crystal color themes.
    /// Right-click on the weapon cycles through these.
    /// </summary>
    public enum CrystalTheme
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
    /// Self-contained texture registry for SparkleProjectileFoundation.
    /// All textures sourced exclusively from the VFX Asset Library.
    /// </summary>
    internal static class SPFTextures
    {
        // ---- PATHS ----
        private static readonly string Bloom = "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/";
        private static readonly string Trails = "MagnumOpus/Assets/VFX Asset Library/TrailsAndRibbons/";
        private static readonly string Projectiles = "MagnumOpus/Assets/VFX Asset Library/Projectiles/";
        private static readonly string Stars = "MagnumOpus/Assets/Particles Asset Library/Stars/";
        private static readonly string GradientLib = "MagnumOpus/Assets/VFX Asset Library/ColorGradients/";
        private static readonly string Masks = "MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/";

        // ---- CRYSTAL BODY TEXTURES ----

        /// <summary>Bright star projectile used as the crystal body.</summary>
        public static readonly Asset<Texture2D> CrystalBody =
            ModContent.Request<Texture2D>(Projectiles + "BrightStarProjectile1", AssetRequestMode.ImmediateLoad);

        /// <summary>Second bright star projectile for overlay variation.</summary>
        public static readonly Asset<Texture2D> CrystalOverlay =
            ModContent.Request<Texture2D>(Projectiles + "BrightStarProjectile2", AssetRequestMode.ImmediateLoad);

        // ---- SPARKLE/GLITTER TEXTURES ----

        /// <summary>Hard 4-pointed star for sharp sparkle accents.</summary>
        public static readonly Asset<Texture2D> SparkleHard =
            ModContent.Request<Texture2D>(Stars + "4PointedStarHard", AssetRequestMode.ImmediateLoad);

        /// <summary>Soft 4-pointed star for diffuse glow sparkles.</summary>
        public static readonly Asset<Texture2D> SparkleSoft =
            ModContent.Request<Texture2D>(Stars + "4PointedStarSoft", AssetRequestMode.ImmediateLoad);

        /// <summary>Thin tall 4-pointed star for elongated twinkle effect.</summary>
        public static readonly Asset<Texture2D> SparkleThin =
            ModContent.Request<Texture2D>(Stars + "ThinTall4PointedStar", AssetRequestMode.ImmediateLoad);

        // ---- GLOW/BLOOM TEXTURES ----

        /// <summary>Soft glow orb for trail glow points.</summary>
        public static readonly Asset<Texture2D> GlowOrb =
            ModContent.Request<Texture2D>(Bloom + "GlowOrb", AssetRequestMode.ImmediateLoad);

        /// <summary>Soft glow for bloom halos around crystals.</summary>
        public static readonly Asset<Texture2D> SoftGlow =
            ModContent.Request<Texture2D>(Bloom + "SoftGlow", AssetRequestMode.ImmediateLoad);

        /// <summary>Point bloom for tiny bright trail points.</summary>
        public static readonly Asset<Texture2D> PointBloom =
            ModContent.Request<Texture2D>(Bloom + "PointBloom", AssetRequestMode.ImmediateLoad);

        /// <summary>Star flare for special flare accents.</summary>
        public static readonly Asset<Texture2D> StarFlare =
            ModContent.Request<Texture2D>(Bloom + "StarFlare", AssetRequestMode.ImmediateLoad);

        /// <summary>Soft radial bloom for ambient glow.</summary>
        public static readonly Asset<Texture2D> SoftRadialBloom =
            ModContent.Request<Texture2D>(Bloom + "SoftRadialBloom", AssetRequestMode.ImmediateLoad);

        // ---- TRAIL TEXTURES ----

        /// <summary>Basic trail strip texture.</summary>
        public static readonly Asset<Texture2D> BasicTrail =
            ModContent.Request<Texture2D>(Trails + "BasicTrail", AssetRequestMode.ImmediateLoad);

        // ---- MASK TEXTURES ----

        /// <summary>Soft circle mask for smooth falloff effects.</summary>
        public static readonly Asset<Texture2D> SoftCircle =
            ModContent.Request<Texture2D>(Masks + "SoftCircle", AssetRequestMode.ImmediateLoad);

        // ---- GRADIENT LUT TEXTURES ----

        public static readonly Asset<Texture2D> GradMoonlightSonata =
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
        /// Returns the gradient LUT texture for the given crystal theme.
        /// </summary>
        public static Texture2D GetGradientForTheme(CrystalTheme theme) => theme switch
        {
            CrystalTheme.MoonlightSonata => GradMoonlightSonata.Value,
            CrystalTheme.Eroica => GradEroica.Value,
            CrystalTheme.LaCampanella => GradLaCampanella.Value,
            CrystalTheme.Enigma => GradEnigma.Value,
            CrystalTheme.Fate => GradFate.Value,
            CrystalTheme.SwanLake => GradSwanLake.Value,
            _ => GradMoonlightSonata.Value,
        };

        /// <summary>
        /// Returns the display name for the given crystal theme.
        /// </summary>
        public static string GetThemeName(CrystalTheme theme) => theme switch
        {
            CrystalTheme.MoonlightSonata => "Moonlight Sonata",
            CrystalTheme.Eroica => "Eroica",
            CrystalTheme.LaCampanella => "La Campanella",
            CrystalTheme.Enigma => "Enigma Variations",
            CrystalTheme.Fate => "Fate",
            CrystalTheme.SwanLake => "Swan Lake",
            _ => "Unknown",
        };

        /// <summary>
        /// Returns theme-appropriate color sets for crystal and sparkle tinting.
        /// Index 0 = primary, 1 = secondary, 2 = accent, 3 = dark, 4 = highlight
        /// </summary>
        public static Color[] GetThemeColors(CrystalTheme theme) => theme switch
        {
            CrystalTheme.MoonlightSonata => new[] {
                new Color(140, 100, 200), new Color(80, 60, 160), new Color(100, 150, 255),
                new Color(60, 40, 120), new Color(200, 180, 255) },
            CrystalTheme.Eroica => new[] {
                new Color(200, 50, 50), new Color(220, 80, 30), new Color(255, 180, 50),
                new Color(140, 20, 20), new Color(255, 220, 150) },
            CrystalTheme.LaCampanella => new[] {
                new Color(255, 140, 40), new Color(200, 80, 20), new Color(255, 200, 60),
                new Color(150, 50, 10), new Color(255, 240, 180) },
            CrystalTheme.Enigma => new[] {
                new Color(140, 60, 200), new Color(60, 200, 80), new Color(80, 40, 160),
                new Color(40, 30, 100), new Color(180, 140, 255) },
            CrystalTheme.Fate => new[] {
                new Color(180, 40, 80), new Color(220, 60, 100), new Color(255, 200, 220),
                new Color(100, 20, 50), new Color(255, 180, 200) },
            CrystalTheme.SwanLake => new[] {
                new Color(220, 220, 255), new Color(200, 200, 230), new Color(180, 180, 220),
                new Color(150, 150, 180), new Color(255, 255, 255) },
            _ => new[] { Color.White, Color.LightGray, Color.White, Color.Gray, Color.White },
        };
    }
}
