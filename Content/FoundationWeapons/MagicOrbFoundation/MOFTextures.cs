using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;

namespace MagnumOpus.Content.FoundationWeapons.MagicOrbFoundation
{
    /// <summary>
    /// Enumerates the available noise texture modes for the orb visual.
    /// Right-click cycling is NOT used here since right-click fires burst orbs.
    /// Instead, the orb randomly picks from a few curated noise textures.
    /// </summary>
    public enum OrbNoiseStyle
    {
        CosmicVortex = 0,
        NebulaWisp,
        FBMNoise,
        CosmicNebula,
        COUNT
    }

    /// <summary>
    /// Self-contained texture registry for MagicOrbFoundation.
    /// All textures sourced from the VFX Asset Library.
    /// </summary>
    internal static class MOFTextures
    {
        // ---- PATHS ----
        private static readonly string Noise = "MagnumOpus/Assets/VFX Asset Library/NoiseTextures/";
        private static readonly string Bloom = "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/";
        private static readonly string GradientLib = "MagnumOpus/Assets/VFX Asset Library/ColorGradients/";
        private static readonly string Masks = "MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/";

        // ---- NOISE TEXTURES ----
        public static readonly Asset<Texture2D> NoiseCosmicVortex =
            ModContent.Request<Texture2D>(Noise + "CosmicEnergyVortex", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> NoiseNebulaWisp =
            ModContent.Request<Texture2D>(Noise + "NebulaWispNoise", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> NoiseFBM =
            ModContent.Request<Texture2D>(Noise + "TileableFBMNoise", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> NoiseCosmicNebula =
            ModContent.Request<Texture2D>(Noise + "CosmicNebulaClouds", AssetRequestMode.ImmediateLoad);

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

        // ---- MASK TEXTURES ----
        public static readonly Asset<Texture2D> SoftCircle =
            ModContent.Request<Texture2D>(Masks + "SoftCircle", AssetRequestMode.ImmediateLoad);

        // ---- GRADIENT LUT ----
        public static readonly Asset<Texture2D> GradFate =
            ModContent.Request<Texture2D>(GradientLib + "FateGradientLUTandRAMP", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> GradEnigma =
            ModContent.Request<Texture2D>(GradientLib + "EnigmaGradientLUTandRAMP", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> GradLaCampanella =
            ModContent.Request<Texture2D>(GradientLib + "LaCampanellaGradientLUTandRAMP", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> GradMoonlight =
            ModContent.Request<Texture2D>(GradientLib + "MoonlightSonataGradientLUTandRAMP", AssetRequestMode.ImmediateLoad);

        /// <summary>
        /// Returns the noise texture for a given orb style.
        /// </summary>
        public static Texture2D GetNoiseForStyle(OrbNoiseStyle style) => style switch
        {
            OrbNoiseStyle.CosmicVortex => NoiseCosmicVortex.Value,
            OrbNoiseStyle.NebulaWisp => NoiseNebulaWisp.Value,
            OrbNoiseStyle.FBMNoise => NoiseFBM.Value,
            OrbNoiseStyle.CosmicNebula => NoiseCosmicNebula.Value,
            _ => NoiseCosmicVortex.Value,
        };

        /// <summary>
        /// Returns the gradient LUT for a given orb style.
        /// </summary>
        public static Texture2D GetGradientForStyle(OrbNoiseStyle style) => style switch
        {
            OrbNoiseStyle.CosmicVortex => GradFate.Value,
            OrbNoiseStyle.NebulaWisp => GradEnigma.Value,
            OrbNoiseStyle.FBMNoise => GradLaCampanella.Value,
            OrbNoiseStyle.CosmicNebula => GradMoonlight.Value,
            _ => GradFate.Value,
        };

        /// <summary>
        /// Returns theme colors for each orb style.
        /// Index 0 = primary/outer, 1 = secondary/mid, 2 = core/highlight
        /// </summary>
        public static Color[] GetStyleColors(OrbNoiseStyle style) => style switch
        {
            OrbNoiseStyle.CosmicVortex => new[] {
                new Color(180, 40, 80), new Color(220, 60, 100), new Color(255, 200, 220) },
            OrbNoiseStyle.NebulaWisp => new[] {
                new Color(140, 60, 200), new Color(60, 200, 80), new Color(180, 140, 255) },
            OrbNoiseStyle.FBMNoise => new[] {
                new Color(255, 140, 40), new Color(200, 80, 20), new Color(255, 240, 180) },
            OrbNoiseStyle.CosmicNebula => new[] {
                new Color(100, 60, 180), new Color(160, 80, 220), new Color(220, 180, 255) },
            _ => new[] { Color.White, Color.LightGray, Color.White },
        };

        /// <summary>
        /// Returns a style name for displayed text.
        /// </summary>
        public static string GetStyleName(OrbNoiseStyle style) => style switch
        {
            OrbNoiseStyle.CosmicVortex => "Cosmic Vortex",
            OrbNoiseStyle.NebulaWisp => "Nebula Wisp",
            OrbNoiseStyle.FBMNoise => "Fractal Fire",
            OrbNoiseStyle.CosmicNebula => "Cosmic Nebula",
            _ => "Unknown",
        };
    }
}
