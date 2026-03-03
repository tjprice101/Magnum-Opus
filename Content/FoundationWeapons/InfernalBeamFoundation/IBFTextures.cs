using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;

namespace MagnumOpus.Content.FoundationWeapons.InfernalBeamFoundation
{
    /// <summary>
    /// Beam theme enum — reuses the same score themes as other Foundation weapons.
    /// </summary>
    public enum InfernalBeamTheme
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
    /// Self-contained texture registry for InfernalBeamFoundation.
    /// All asset references live here — nothing depends on external VFX systems.
    /// </summary>
    internal static class IBFTextures
    {
        // ---- PATHS ----
        private static readonly string Beams = "MagnumOpus/Assets/VFX Asset Library/BeamTextures/";
        private static readonly string Trails = "MagnumOpus/Assets/VFX Asset Library/TrailsAndRibbons/";
        private static readonly string Bloom = "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/";
        private static readonly string Noise = "MagnumOpus/Assets/VFX Asset Library/NoiseTextures/";
        private static readonly string GradientLib = "MagnumOpus/Assets/VFX Asset Library/ColorGradients/";

        // ---- BEAM BODY TEXTURES ----
        /// <summary>The SoundWaveBeam texture — used as the primary beam body UV strip.</summary>
        public static readonly Asset<Texture2D> SoundWaveBeam =
            ModContent.Request<Texture2D>(Beams + "SoundWaveBeam", AssetRequestMode.ImmediateLoad);

        /// <summary>InfernalBeamRing — spinning ring sprite rendered at beam origin.</summary>
        public static readonly Asset<Texture2D> InfernalBeamRing =
            ModContent.Request<Texture2D>(Beams + "InfernalBeamRing", AssetRequestMode.ImmediateLoad);

        /// <summary>Basic trail mask for the vertex strip beam body alpha.</summary>
        public static readonly Asset<Texture2D> BeamAlphaMask =
            ModContent.Request<Texture2D>(Trails + "BasicTrail", AssetRequestMode.ImmediateLoad);

        /// <summary>Secondary detail: energy motion flowing along the beam.</summary>
        public static readonly Asset<Texture2D> EnergyMotion =
            ModContent.Request<Texture2D>(Beams + "EnergyMotion", AssetRequestMode.ImmediateLoad);

        /// <summary>Another detail layer: fine energy surge for inner beam detail.</summary>
        public static readonly Asset<Texture2D> EnergySurge =
            ModContent.Request<Texture2D>(Beams + "EnergySurgeBeam", AssetRequestMode.ImmediateLoad);

        /// <summary>Thin linear glow — adds central brightness strip to beam.</summary>
        public static readonly Asset<Texture2D> ThinGlowLine =
            ModContent.Request<Texture2D>(Beams + "ThinLinearGlow", AssetRequestMode.ImmediateLoad);

        // ---- NOISE ----
        public static readonly Asset<Texture2D> NoiseFBM =
            ModContent.Request<Texture2D>(Noise + "TileableFBMNoise", AssetRequestMode.ImmediateLoad);

        // ---- BLOOM / FLARE ----
        public static readonly Asset<Texture2D> SoftGlow =
            ModContent.Request<Texture2D>(Bloom + "SoftGlow", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> GlowOrb =
            ModContent.Request<Texture2D>(Bloom + "GlowOrb", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> StarFlare =
            ModContent.Request<Texture2D>(Bloom + "StarFlare", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> LensFlare =
            ModContent.Request<Texture2D>(Bloom + "LensFlare", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> PointBloom =
            ModContent.Request<Texture2D>(Bloom + "PointBloom", AssetRequestMode.ImmediateLoad);

        // ---- GRADIENT LUTs ----
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

        // ---- HELPERS ----

        public static Texture2D GetGradientForTheme(InfernalBeamTheme theme) => theme switch
        {
            InfernalBeamTheme.MoonlightSonata => GradMoonlight.Value,
            InfernalBeamTheme.Eroica => GradEroica.Value,
            InfernalBeamTheme.LaCampanella => GradLaCampanella.Value,
            InfernalBeamTheme.Enigma => GradEnigma.Value,
            InfernalBeamTheme.Fate => GradFate.Value,
            InfernalBeamTheme.SwanLake => GradSwanLake.Value,
            _ => GradMoonlight.Value,
        };

        public static string GetThemeName(InfernalBeamTheme theme) => theme switch
        {
            InfernalBeamTheme.MoonlightSonata => "Moonlight Sonata",
            InfernalBeamTheme.Eroica => "Eroica",
            InfernalBeamTheme.LaCampanella => "La Campanella",
            InfernalBeamTheme.Enigma => "Enigma Variations",
            InfernalBeamTheme.Fate => "Fate",
            InfernalBeamTheme.SwanLake => "Swan Lake",
            _ => "Unknown",
        };

        public static Color[] GetDustColorsForTheme(InfernalBeamTheme theme) => theme switch
        {
            InfernalBeamTheme.MoonlightSonata => new[] {
                new Color(140, 100, 200), new Color(80, 60, 160), new Color(100, 150, 255),
                new Color(180, 130, 255), new Color(60, 80, 200) },
            InfernalBeamTheme.Eroica => new[] {
                new Color(200, 50, 50), new Color(220, 80, 30), new Color(255, 180, 50),
                new Color(180, 30, 30), new Color(255, 120, 80) },
            InfernalBeamTheme.LaCampanella => new[] {
                new Color(255, 140, 40), new Color(200, 80, 20), new Color(255, 200, 60),
                new Color(180, 60, 10), new Color(255, 160, 80) },
            InfernalBeamTheme.Enigma => new[] {
                new Color(140, 60, 200), new Color(60, 200, 80), new Color(80, 40, 160),
                new Color(40, 180, 60), new Color(100, 80, 220) },
            InfernalBeamTheme.Fate => new[] {
                new Color(180, 40, 80), new Color(220, 60, 100), new Color(255, 255, 255),
                new Color(140, 20, 60), new Color(200, 80, 120) },
            InfernalBeamTheme.SwanLake => new[] {
                new Color(240, 240, 255), new Color(200, 200, 220), new Color(180, 180, 200),
                new Color(255, 255, 255), new Color(220, 210, 240) },
            _ => new[] { Color.White },
        };
    }
}
