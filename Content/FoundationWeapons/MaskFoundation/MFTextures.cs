using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;

namespace MagnumOpus.Content.FoundationWeapons.MaskFoundation
{
    /// <summary>
    /// Enumerates the available noise texture modes.
    /// Right-click on the weapon cycles through these.
    /// Each mode maps a different noise texture onto the radial mask orb.
    /// </summary>
    public enum NoiseMode
    {
        PerlinNoise = 0,
        VoronoiCell,
        CosmicVortex,
        FBMNoise,
        MarbleNoise,
        NebulaWisp,
        VoronoiEdge,
        SimplexNoise,
        CosmicNebula,
        StarField,
        MusicalWave,
        COUNT
    }

    /// <summary>
    /// Self-contained texture registry for MaskFoundation.
    /// All textures sourced exclusively from the VFX Asset Library.
    /// </summary>
    internal static class MFTextures
    {
        // ---- PATHS ----
        private static readonly string Noise = "MagnumOpus/Assets/VFX Asset Library/NoiseTextures/";
        private static readonly string Bloom = "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/";
        private static readonly string GradientLib = "MagnumOpus/Assets/VFX Asset Library/ColorGradients/";
        private static readonly string Masks = "MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/";

        // ---- NOISE TEXTURES ----

        public static readonly Asset<Texture2D> NoisePerlin =
            ModContent.Request<Texture2D>(Noise + "PerlinNoise", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> NoiseVoronoiCell =
            ModContent.Request<Texture2D>(Noise + "VoronoiCellNoise", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> NoiseCosmicVortex =
            ModContent.Request<Texture2D>(Noise + "CosmicEnergyVortex", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> NoiseFBM =
            ModContent.Request<Texture2D>(Noise + "TileableFBMNoise", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> NoiseMarble =
            ModContent.Request<Texture2D>(Noise + "TileableMarbleNoise", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> NoiseNebulaWisp =
            ModContent.Request<Texture2D>(Noise + "NebulaWispNoise", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> NoiseVoronoiEdge =
            ModContent.Request<Texture2D>(Noise + "VornoiEdgeNoise", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> NoiseSimplex =
            ModContent.Request<Texture2D>(Noise + "SimplexNoise", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> NoiseCosmicNebula =
            ModContent.Request<Texture2D>(Noise + "CosmicNebulaClouds", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> NoiseStarField =
            ModContent.Request<Texture2D>(Noise + "StarFieldScatter", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> NoiseMusicalWave =
            ModContent.Request<Texture2D>(Noise + "MusicalWavePattern", AssetRequestMode.ImmediateLoad);

        // ---- BLOOM / GLOW TEXTURES ----

        public static readonly Asset<Texture2D> SoftGlow =
            ModContent.Request<Texture2D>(Bloom + "SoftGlow", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> SoftRadialBloom =
            ModContent.Request<Texture2D>(Bloom + "SoftRadialBloom", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> GlowOrb =
            ModContent.Request<Texture2D>(Bloom + "GlowOrb", AssetRequestMode.ImmediateLoad);

        // ---- MASK TEXTURES ----

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
        /// Returns the noise texture for the given noise mode.
        /// </summary>
        public static Texture2D GetNoiseForMode(NoiseMode mode) => mode switch
        {
            NoiseMode.PerlinNoise => NoisePerlin.Value,
            NoiseMode.VoronoiCell => NoiseVoronoiCell.Value,
            NoiseMode.CosmicVortex => NoiseCosmicVortex.Value,
            NoiseMode.FBMNoise => NoiseFBM.Value,
            NoiseMode.MarbleNoise => NoiseMarble.Value,
            NoiseMode.NebulaWisp => NoiseNebulaWisp.Value,
            NoiseMode.VoronoiEdge => NoiseVoronoiEdge.Value,
            NoiseMode.SimplexNoise => NoiseSimplex.Value,
            NoiseMode.CosmicNebula => NoiseCosmicNebula.Value,
            NoiseMode.StarField => NoiseStarField.Value,
            NoiseMode.MusicalWave => NoiseMusicalWave.Value,
            _ => NoisePerlin.Value,
        };

        /// <summary>
        /// Returns the display name for the given noise mode.
        /// </summary>
        public static string GetModeName(NoiseMode mode) => mode switch
        {
            NoiseMode.PerlinNoise => "Perlin Noise",
            NoiseMode.VoronoiCell => "Voronoi Cell",
            NoiseMode.CosmicVortex => "Cosmic Vortex",
            NoiseMode.FBMNoise => "Fractal Brownian Motion",
            NoiseMode.MarbleNoise => "Marble Veins",
            NoiseMode.NebulaWisp => "Nebula Wisp",
            NoiseMode.VoronoiEdge => "Voronoi Edge",
            NoiseMode.SimplexNoise => "Simplex Noise",
            NoiseMode.CosmicNebula => "Cosmic Nebula",
            NoiseMode.StarField => "Star Field",
            NoiseMode.MusicalWave => "Musical Wave",
            _ => "Unknown",
        };

        /// <summary>
        /// Returns theme-appropriate colors for the given noise mode.
        /// Index 0 = primary, 1 = secondary, 2 = core/highlight
        /// </summary>
        public static Color[] GetModeColors(NoiseMode mode) => mode switch
        {
            NoiseMode.PerlinNoise => new[] {
                new Color(140, 100, 200), new Color(80, 60, 160), new Color(200, 180, 255) },
            NoiseMode.VoronoiCell => new[] {
                new Color(200, 50, 50), new Color(220, 80, 30), new Color(255, 220, 150) },
            NoiseMode.CosmicVortex => new[] {
                new Color(180, 40, 80), new Color(220, 60, 100), new Color(255, 200, 220) },
            NoiseMode.FBMNoise => new[] {
                new Color(255, 140, 40), new Color(200, 80, 20), new Color(255, 240, 180) },
            NoiseMode.MarbleNoise => new[] {
                new Color(220, 220, 255), new Color(180, 180, 220), new Color(255, 255, 255) },
            NoiseMode.NebulaWisp => new[] {
                new Color(140, 60, 200), new Color(60, 200, 80), new Color(180, 140, 255) },
            NoiseMode.VoronoiEdge => new[] {
                new Color(60, 200, 180), new Color(40, 160, 140), new Color(200, 255, 240) },
            NoiseMode.SimplexNoise => new[] {
                new Color(255, 200, 50), new Color(220, 160, 30), new Color(255, 255, 200) },
            NoiseMode.CosmicNebula => new[] {
                new Color(100, 60, 180), new Color(160, 80, 220), new Color(220, 180, 255) },
            NoiseMode.StarField => new[] {
                new Color(100, 120, 200), new Color(60, 80, 160), new Color(200, 220, 255) },
            NoiseMode.MusicalWave => new[] {
                new Color(200, 100, 180), new Color(160, 60, 140), new Color(255, 200, 240) },
            _ => new[] { Color.White, Color.LightGray, Color.White },
        };

        /// <summary>
        /// Returns a gradient LUT appropriate for the given noise mode.
        /// </summary>
        public static Texture2D GetGradientForMode(NoiseMode mode) => mode switch
        {
            NoiseMode.PerlinNoise => GradMoonlightSonata.Value,
            NoiseMode.VoronoiCell => GradEroica.Value,
            NoiseMode.CosmicVortex => GradFate.Value,
            NoiseMode.FBMNoise => GradLaCampanella.Value,
            NoiseMode.MarbleNoise => GradSwanLake.Value,
            NoiseMode.NebulaWisp => GradEnigma.Value,
            NoiseMode.VoronoiEdge => GradEnigma.Value,
            NoiseMode.SimplexNoise => GradLaCampanella.Value,
            NoiseMode.CosmicNebula => GradFate.Value,
            NoiseMode.StarField => GradMoonlightSonata.Value,
            NoiseMode.MusicalWave => GradEroica.Value,
            _ => GradMoonlightSonata.Value,
        };
    }
}
