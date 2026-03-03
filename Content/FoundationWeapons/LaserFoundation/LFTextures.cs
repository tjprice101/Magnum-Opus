using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;

namespace MagnumOpus.Content.FoundationWeapons.LaserFoundation
{
    /// <summary>
    /// Enumerates the available beam color themes.
    /// Each corresponds to a gradient LUT texture in the VFX Asset Library.
    /// Right-click on the weapon cycles through these.
    /// </summary>
    public enum BeamTheme
    {
        MoonlightSonata = 0,
        Eroica,
        EroicaPale,
        LaCampanella,
        Enigma,
        Fate,
        SwanLake,
        COUNT // Must be last — used for wrapping the cycle
    }

    /// <summary>
    /// Self-contained texture registry for LaserFoundation.
    /// All textures are loaded ImmediateLoad so they're available the instant we need them.
    /// 
    /// All assets sourced exclusively from the VFX Asset Library — no SLP dependencies.
    /// </summary>
    internal static class LFTextures
    {
        // ---- PATHS ----
        private static readonly string Trails = "MagnumOpus/Assets/VFX Asset Library/TrailsAndRibbons/";
        private static readonly string Beams = "MagnumOpus/Assets/VFX Asset Library/BeamTextures/";
        private static readonly string Lightning = "MagnumOpus/Assets/VFX Asset Library/Lightning/";
        private static readonly string Bloom = "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/";
        private static readonly string GradientLib = "MagnumOpus/Assets/VFX Asset Library/ColorGradients/";

        // ---- BEAM BODY TEXTURES ----

        /// <summary>Alpha mask for the beam cross-section. Bright center, transparent edges.</summary>
        public static readonly Asset<Texture2D> BeamAlphaMask =
            ModContent.Request<Texture2D>(Trails + "BasicTrail", AssetRequestMode.ImmediateLoad);

        /// <summary>Detail texture 1: thin linear glow strip.</summary>
        public static readonly Asset<Texture2D> DetailThinGlowLine =
            ModContent.Request<Texture2D>(Beams + "ThinLinearGlow", AssetRequestMode.ImmediateLoad);

        /// <summary>Detail texture 2: lightning surge / spark energy pattern.</summary>
        public static readonly Asset<Texture2D> DetailSpark =
            ModContent.Request<Texture2D>(Lightning + "LightningSurge", AssetRequestMode.ImmediateLoad);

        /// <summary>Detail texture 3: flowing energy motion pattern.</summary>
        public static readonly Asset<Texture2D> DetailExtra =
            ModContent.Request<Texture2D>(Beams + "EnergyMotion", AssetRequestMode.ImmediateLoad);

        /// <summary>Detail texture 4: energy surge beam strip.</summary>
        public static readonly Asset<Texture2D> DetailTrailLoop =
            ModContent.Request<Texture2D>(Beams + "EnergySurgeBeam", AssetRequestMode.ImmediateLoad);

        // ---- THEME GRADIENT LUT TEXTURES ----
        // Each score/theme has its own color gradient ramp that the beam shader samples.

        public static readonly Asset<Texture2D> GradMoonlightSonata =
            ModContent.Request<Texture2D>(GradientLib + "MoonlightSonataGradientLUTandRAMP", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> GradEroica =
            ModContent.Request<Texture2D>(GradientLib + "EroicaGradientLUTandRAMP", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> GradEroicaPale =
            ModContent.Request<Texture2D>(GradientLib + "EroicaGradientPALELUTandRAMP", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> GradLaCampanella =
            ModContent.Request<Texture2D>(GradientLib + "LaCampanellaGradientLUTandRAMP", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> GradEnigma =
            ModContent.Request<Texture2D>(GradientLib + "EnigmaGradientLUTandRAMP", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> GradFate =
            ModContent.Request<Texture2D>(GradientLib + "FateGradientLUTandRAMP", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> GradSwanLake =
            ModContent.Request<Texture2D>(GradientLib + "SwanLakeGradient", AssetRequestMode.ImmediateLoad);

        /// <summary>
        /// Returns the gradient LUT texture for the given beam theme.
        /// </summary>
        public static Texture2D GetGradientForTheme(BeamTheme theme) => theme switch
        {
            BeamTheme.MoonlightSonata => GradMoonlightSonata.Value,
            BeamTheme.Eroica => GradEroica.Value,
            BeamTheme.EroicaPale => GradEroicaPale.Value,
            BeamTheme.LaCampanella => GradLaCampanella.Value,
            BeamTheme.Enigma => GradEnigma.Value,
            BeamTheme.Fate => GradFate.Value,
            BeamTheme.SwanLake => GradSwanLake.Value,
            _ => GradMoonlightSonata.Value,
        };

        /// <summary>
        /// Returns the display name for the given beam theme (shown in combat text on cycle).
        /// </summary>
        public static string GetThemeName(BeamTheme theme) => theme switch
        {
            BeamTheme.MoonlightSonata => "Moonlight Sonata",
            BeamTheme.Eroica => "Eroica",
            BeamTheme.EroicaPale => "Eroica (Pale)",
            BeamTheme.LaCampanella => "La Campanella",
            BeamTheme.Enigma => "Enigma Variations",
            BeamTheme.Fate => "Fate",
            BeamTheme.SwanLake => "Swan Lake",
            _ => "Unknown",
        };

        /// <summary>
        /// Returns theme-appropriate dust colors for particle accents.
        /// </summary>
        public static Color[] GetDustColorsForTheme(BeamTheme theme) => theme switch
        {
            BeamTheme.MoonlightSonata => new[] {
                new Color(140, 100, 200), new Color(80, 60, 160), new Color(100, 150, 255),
                new Color(180, 130, 255), new Color(60, 80, 200) },
            BeamTheme.Eroica => new[] {
                new Color(200, 50, 50), new Color(220, 80, 30), new Color(255, 180, 50),
                new Color(180, 30, 30), new Color(255, 120, 80) },
            BeamTheme.EroicaPale => new[] {
                new Color(255, 180, 180), new Color(255, 200, 150), new Color(255, 220, 200),
                new Color(220, 150, 150), new Color(255, 160, 130) },
            BeamTheme.LaCampanella => new[] {
                new Color(255, 140, 40), new Color(200, 80, 20), new Color(255, 200, 60),
                new Color(180, 60, 10), new Color(255, 160, 80) },
            BeamTheme.Enigma => new[] {
                new Color(140, 60, 200), new Color(60, 200, 80), new Color(80, 40, 160),
                new Color(40, 180, 60), new Color(100, 80, 220) },
            BeamTheme.Fate => new[] {
                new Color(180, 40, 80), new Color(220, 60, 100), new Color(255, 255, 255),
                new Color(140, 20, 60), new Color(200, 80, 120) },
            BeamTheme.SwanLake => new[] {
                new Color(240, 240, 255), new Color(200, 200, 220), new Color(180, 180, 200),
                new Color(255, 255, 255), new Color(220, 210, 240) },
            _ => new[] { Color.White },
        };

        // ---- ENDPOINT FLARE TEXTURES ----

        /// <summary>Lens flare for beam origin and endpoint.</summary>
        public static readonly Asset<Texture2D> LensFlare =
            ModContent.Request<Texture2D>(Bloom + "LensFlare", AssetRequestMode.ImmediateLoad);

        /// <summary>Star flare for beam origin and endpoint.</summary>
        public static readonly Asset<Texture2D> StarFlare =
            ModContent.Request<Texture2D>(Bloom + "StarFlare", AssetRequestMode.ImmediateLoad);

        /// <summary>Soft glow orb for beam endpoint.</summary>
        public static readonly Asset<Texture2D> GlowOrb =
            ModContent.Request<Texture2D>(Bloom + "GlowOrb", AssetRequestMode.ImmediateLoad);

        /// <summary>Soft glow for bloom effects.</summary>
        public static readonly Asset<Texture2D> SoftGlow =
            ModContent.Request<Texture2D>(Bloom + "SoftGlow", AssetRequestMode.ImmediateLoad);
    }
}
