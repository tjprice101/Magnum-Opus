using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;

namespace MagnumOpus.Content.FoundationWeapons.ImpactFoundation
{
    /// <summary>
    /// Enumerates the three impact modes.
    /// Right-click on the weapon cycles through these.
    /// </summary>
    public enum ImpactMode
    {
        Ripple = 0,
        DamageZone,
        SlashMark,
        COUNT
    }

    /// <summary>
    /// Self-contained texture registry for ImpactFoundation.
    /// All textures sourced from the VFX Asset Library.
    /// </summary>
    internal static class IFTextures
    {
        // ---- PATHS ----
        private static readonly string Noise = "MagnumOpus/Assets/VFX Asset Library/NoiseTextures/";
        private static readonly string Bloom = "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/";
        private static readonly string GradientLib = "MagnumOpus/Assets/VFX Asset Library/ColorGradients/";
        private static readonly string Masks = "MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/";
        private static readonly string Impact = "MagnumOpus/Assets/VFX Asset Library/ImpactEffects/";

        // ---- NOISE TEXTURES ----
        public static readonly Asset<Texture2D> NoisePerlin =
            ModContent.Request<Texture2D>(Noise + "PerlinNoise", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> NoiseFBM =
            ModContent.Request<Texture2D>(Noise + "TileableFBMNoise", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> NoiseCosmicVortex =
            ModContent.Request<Texture2D>(Noise + "CosmicEnergyVortex", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> NoiseSmoke =
            ModContent.Request<Texture2D>(Noise + "NoiseSmoke", AssetRequestMode.ImmediateLoad);

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

        public static readonly Asset<Texture2D> HardCircleMask =
            ModContent.Request<Texture2D>(Masks + "HardCircleMask", AssetRequestMode.ImmediateLoad);

        // ---- IMPACT TEXTURES ----
        public static readonly Asset<Texture2D> ImpactEllipse =
            ModContent.Request<Texture2D>(Impact + "ImpactEllipse", AssetRequestMode.ImmediateLoad);

        // ---- GRADIENT LUT TEXTURES ----
        public static readonly Asset<Texture2D> GradMoonlightSonata =
            ModContent.Request<Texture2D>(GradientLib + "MoonlightSonataGradientLUTandRAMP", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> GradEroica =
            ModContent.Request<Texture2D>(GradientLib + "EroicaGradientLUTandRAMP", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> GradLaCampanella =
            ModContent.Request<Texture2D>(GradientLib + "LaCampanellaGradientLUTandRAMP", AssetRequestMode.ImmediateLoad);

        /// <summary>
        /// Returns display name for the given impact mode.
        /// </summary>
        public static string GetModeName(ImpactMode mode) => mode switch
        {
            ImpactMode.Ripple => "Ripple Impact",
            ImpactMode.DamageZone => "Damage Zone",
            ImpactMode.SlashMark => "Slash Mark",
            _ => "Unknown",
        };

        /// <summary>
        /// Returns theme-appropriate colors for the given impact mode.
        /// Index 0 = primary, 1 = secondary, 2 = core/highlight
        /// </summary>
        public static Color[] GetModeColors(ImpactMode mode) => mode switch
        {
            ImpactMode.Ripple => new[] {
                new Color(60, 180, 255), new Color(100, 220, 255), new Color(220, 240, 255) },
            ImpactMode.DamageZone => new[] {
                new Color(255, 80, 50), new Color(255, 160, 40), new Color(255, 240, 180) },
            ImpactMode.SlashMark => new[] {
                new Color(180, 60, 220), new Color(220, 120, 255), new Color(255, 200, 255) },
            _ => new[] { Color.White, Color.LightGray, Color.White },
        };

        /// <summary>
        /// Returns a gradient LUT for the given impact mode.
        /// </summary>
        public static Texture2D GetGradientForMode(ImpactMode mode) => mode switch
        {
            ImpactMode.Ripple => GradMoonlightSonata.Value,
            ImpactMode.DamageZone => GradLaCampanella.Value,
            ImpactMode.SlashMark => GradEroica.Value,
            _ => GradMoonlightSonata.Value,
        };

        /// <summary>
        /// Returns a noise texture appropriate for the given impact mode.
        /// </summary>
        public static Texture2D GetNoiseForMode(ImpactMode mode) => mode switch
        {
            ImpactMode.Ripple => NoisePerlin.Value,
            ImpactMode.DamageZone => NoiseFBM.Value,
            ImpactMode.SlashMark => NoiseCosmicVortex.Value,
            _ => NoisePerlin.Value,
        };
    }
}
