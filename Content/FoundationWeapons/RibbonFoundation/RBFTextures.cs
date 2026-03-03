using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;

namespace MagnumOpus.Content.FoundationWeapons.RibbonFoundation
{
    /// <summary>
    /// Enumerates the 10 ribbon trail modes cycled by right-click.
    /// </summary>
    public enum RibbonMode
    {
        /// <summary>Pure bloom sprites stacked along the trail — bright, hot, glowing.</summary>
        PureBloom = 0,

        /// <summary>Bloom sprites with noise texture erosion for organic fading.</summary>
        BloomNoiseFade,

        /// <summary>BasicTrail strip texture UV-stretched along position history.</summary>
        BasicTrailStrip,

        /// <summary>Harmonic Standing Wave Ribbon — sine wave pattern along the trail.</summary>
        HarmonicWave,

        /// <summary>Spiraling Vortex Energy Strip — helix pattern along the trail.</summary>
        SpiralingVortex,

        /// <summary>EnergySurgeBeam texture used as a flowing ribbon body.</summary>
        EnergySurge,

        /// <summary>Cosmic Nebula Clouds noise rendered as dense cloud ribbon.</summary>
        CosmicNebula,

        /// <summary>Musical Wave Pattern noise as a frequency-oscillation ribbon.</summary>
        MusicalWave,

        /// <summary>Tileable Marble Noise as a flowing veined ribbon.</summary>
        MarbleFlow,

        /// <summary>Lightning Surge texture + bloom hybrid for electric ribbon.</summary>
        LightningRibbon,

        COUNT
    }

    /// <summary>
    /// Self-contained texture registry for RibbonFoundation.
    /// All textures sourced from the VFX Asset Library.
    /// </summary>
    internal static class RBFTextures
    {
        // ---- PATHS ----
        private static readonly string Bloom = "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/";
        private static readonly string Trails = "MagnumOpus/Assets/VFX Asset Library/TrailsAndRibbons/";
        private static readonly string Noise = "MagnumOpus/Assets/VFX Asset Library/NoiseTextures/";
        private static readonly string Beams = "MagnumOpus/Assets/VFX Asset Library/BeamTextures/";
        private static readonly string Projectiles = "MagnumOpus/Assets/VFX Asset Library/Projectiles/";
        private static readonly string Masks = "MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/";
        private static readonly string Lightning = "MagnumOpus/Assets/VFX Asset Library/Lightning/";
        private static readonly string GradientLib = "MagnumOpus/Assets/VFX Asset Library/ColorGradients/";

        // ---- PROJECTILE BODY ----
        public static readonly Asset<Texture2D> MusicNoteOrb =
            ModContent.Request<Texture2D>(Projectiles + "Pulsating Music Note Orb", AssetRequestMode.ImmediateLoad);

        // ---- BLOOM / GLOW TEXTURES ----
        public static readonly Asset<Texture2D> SoftGlow =
            ModContent.Request<Texture2D>(Bloom + "SoftGlow", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> SoftGlowBright =
            ModContent.Request<Texture2D>(Bloom + "SoftGlowBrightAndLargerMiddle", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> GlowOrb =
            ModContent.Request<Texture2D>(Bloom + "GlowOrb", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> PointBloom =
            ModContent.Request<Texture2D>(Bloom + "PointBloom", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> SoftRadialBloom =
            ModContent.Request<Texture2D>(Bloom + "SoftRadialBloom", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> StarFlare =
            ModContent.Request<Texture2D>(Bloom + "StarFlare", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> LensFlare =
            ModContent.Request<Texture2D>(Bloom + "LensFlare", AssetRequestMode.ImmediateLoad);

        // ---- TRAIL STRIP TEXTURES ----
        public static readonly Asset<Texture2D> BasicTrail =
            ModContent.Request<Texture2D>(Trails + "BasicTrail", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> HarmonicWaveRibbon =
            ModContent.Request<Texture2D>(Trails + "Harmonic Standing Wave Ribbon", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> SpiralingVortexStrip =
            ModContent.Request<Texture2D>(Trails + "Spiraling Vortex Energy Strip", AssetRequestMode.ImmediateLoad);

        // ---- BEAM TEXTURES (used as ribbon fills) ----
        public static readonly Asset<Texture2D> EnergySurgeBeam =
            ModContent.Request<Texture2D>(Beams + "EnergySurgeBeam", AssetRequestMode.ImmediateLoad);

        // ---- NOISE TEXTURES (used for ribbon fills and erosion) ----
        public static readonly Asset<Texture2D> CosmicNebulaClouds =
            ModContent.Request<Texture2D>(Noise + "CosmicNebulaClouds", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> MusicalWavePattern =
            ModContent.Request<Texture2D>(Noise + "MusicalWavePattern", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> TileableMarbleNoise =
            ModContent.Request<Texture2D>(Noise + "TileableMarbleNoise", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> PerlinNoise =
            ModContent.Request<Texture2D>(Noise + "PerlinNoise", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> FBMNoise =
            ModContent.Request<Texture2D>(Noise + "TileableFBMNoise", AssetRequestMode.ImmediateLoad);

        // ---- LIGHTNING ----
        public static readonly Asset<Texture2D> LightningSurge =
            ModContent.Request<Texture2D>(Lightning + "LightningSurge", AssetRequestMode.ImmediateLoad);

        // ---- MASKS ----
        public static readonly Asset<Texture2D> SoftCircle =
            ModContent.Request<Texture2D>(Masks + "SoftCircle", AssetRequestMode.ImmediateLoad);

        // ---- GRADIENTS ----
        public static readonly Asset<Texture2D> GradMoonlight =
            ModContent.Request<Texture2D>(GradientLib + "MoonlightSonataGradientLUTandRAMP", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> GradEroica =
            ModContent.Request<Texture2D>(GradientLib + "EroicaGradientLUTandRAMP", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> GradLaCampanella =
            ModContent.Request<Texture2D>(GradientLib + "LaCampanellaGradientLUTandRAMP", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> GradFate =
            ModContent.Request<Texture2D>(GradientLib + "FateGradientLUTandRAMP", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> GradEnigma =
            ModContent.Request<Texture2D>(GradientLib + "EnigmaGradientLUTandRAMP", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> GradSwanLake =
            ModContent.Request<Texture2D>(GradientLib + "SwanLakeGradient", AssetRequestMode.ImmediateLoad);

        /// <summary>
        /// Returns the display name for a ribbon mode.
        /// </summary>
        public static string GetModeName(RibbonMode mode) => mode switch
        {
            RibbonMode.PureBloom => "Pure Bloom",
            RibbonMode.BloomNoiseFade => "Bloom + Noise Fade",
            RibbonMode.BasicTrailStrip => "Basic Trail",
            RibbonMode.HarmonicWave => "Harmonic Wave",
            RibbonMode.SpiralingVortex => "Spiraling Vortex",
            RibbonMode.EnergySurge => "Energy Surge",
            RibbonMode.CosmicNebula => "Cosmic Nebula",
            RibbonMode.MusicalWave => "Musical Wave",
            RibbonMode.MarbleFlow => "Marble Flow",
            RibbonMode.LightningRibbon => "Lightning Ribbon",
            _ => "Unknown",
        };

        /// <summary>
        /// Returns the primary display color for a ribbon mode (used in combat text and tooltips).
        /// </summary>
        public static Color GetModeColor(RibbonMode mode) => mode switch
        {
            RibbonMode.PureBloom => new Color(255, 230, 180),        // Warm white-gold
            RibbonMode.BloomNoiseFade => new Color(180, 140, 255),   // Soft purple
            RibbonMode.BasicTrailStrip => new Color(200, 220, 255),  // Cool white
            RibbonMode.HarmonicWave => new Color(100, 200, 255),     // Bright cyan
            RibbonMode.SpiralingVortex => new Color(160, 80, 220),   // Deep purple
            RibbonMode.EnergySurge => new Color(80, 255, 180),       // Electric green
            RibbonMode.CosmicNebula => new Color(120, 80, 200),      // Cosmic purple
            RibbonMode.MusicalWave => new Color(255, 160, 80),       // Warm orange
            RibbonMode.MarbleFlow => new Color(140, 180, 220),       // Marble blue-gray
            RibbonMode.LightningRibbon => new Color(180, 220, 255),  // Electric blue-white
            _ => Color.White,
        };

        /// <summary>
        /// Returns a 3-color palette for a ribbon mode: [primary, secondary, highlight].
        /// </summary>
        public static Color[] GetModeColors(RibbonMode mode) => mode switch
        {
            RibbonMode.PureBloom => new[] {
                new Color(255, 230, 180), new Color(255, 200, 100), Color.White },
            RibbonMode.BloomNoiseFade => new[] {
                new Color(180, 140, 255), new Color(120, 80, 200), new Color(220, 200, 255) },
            RibbonMode.BasicTrailStrip => new[] {
                new Color(200, 220, 255), new Color(150, 180, 230), Color.White },
            RibbonMode.HarmonicWave => new[] {
                new Color(100, 200, 255), new Color(60, 140, 220), new Color(180, 240, 255) },
            RibbonMode.SpiralingVortex => new[] {
                new Color(160, 80, 220), new Color(100, 40, 180), new Color(220, 160, 255) },
            RibbonMode.EnergySurge => new[] {
                new Color(80, 255, 180), new Color(40, 200, 120), new Color(180, 255, 220) },
            RibbonMode.CosmicNebula => new[] {
                new Color(120, 80, 200), new Color(80, 40, 160), new Color(200, 150, 255) },
            RibbonMode.MusicalWave => new[] {
                new Color(255, 160, 80), new Color(220, 100, 40), new Color(255, 220, 160) },
            RibbonMode.MarbleFlow => new[] {
                new Color(140, 180, 220), new Color(100, 140, 180), new Color(200, 220, 240) },
            RibbonMode.LightningRibbon => new[] {
                new Color(180, 220, 255), new Color(100, 160, 255), Color.White },
            _ => new[] { Color.White, Color.LightGray, Color.White },
        };
    }
}
