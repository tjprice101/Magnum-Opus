using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;

namespace MagnumOpus.Content.FoundationWeapons.AttackFoundation
{
    /// <summary>
    /// Enumerates the five attack modes.
    /// Right-click on the weapon cycles through these.
    /// </summary>
    public enum AttackMode
    {
        ThrowSlam = 0,
        ComboSwing,
        Astralgraph,
        FlamingRing,
        RangerShot,
        COUNT
    }

    /// <summary>
    /// Self-contained texture registry for AttackFoundation.
    /// All textures sourced from the VFX Asset Library.
    /// </summary>
    internal static class AFTextures
    {
        // ---- PATHS ----
        private static readonly string Noise = "MagnumOpus/Assets/VFX Asset Library/NoiseTextures/";
        private static readonly string Bloom = "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/";
        private static readonly string Masks = "MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/";
        private static readonly string Impact = "MagnumOpus/Assets/VFX Asset Library/ImpactEffects/";
        private static readonly string GradientLib = "MagnumOpus/Assets/VFX Asset Library/ColorGradients/";

        // ---- NOISE TEXTURES ----
        public static readonly Asset<Texture2D> NoisePerlin =
            ModContent.Request<Texture2D>(Noise + "PerlinNoise", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> NoiseFBM =
            ModContent.Request<Texture2D>(Noise + "TileableFBMNoise", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> NoiseCosmicVortex =
            ModContent.Request<Texture2D>(Noise + "CosmicEnergyVortex", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> NoiseMusicalWave =
            ModContent.Request<Texture2D>(Noise + "MusicalWavePattern", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> NoiseStarField =
            ModContent.Request<Texture2D>(Noise + "StarFieldScatter", AssetRequestMode.ImmediateLoad);

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

        public static readonly Asset<Texture2D> LensFlare =
            ModContent.Request<Texture2D>(Bloom + "LensFlare", AssetRequestMode.ImmediateLoad);

        // ---- MASK TEXTURES ----
        public static readonly Asset<Texture2D> SoftCircle =
            ModContent.Request<Texture2D>(Masks + "SoftCircle", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> HardCircleMask =
            ModContent.Request<Texture2D>(Masks + "HardCircleMask", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> PowerEffectRing =
            ModContent.Request<Texture2D>(Impact + "PowerEffectRing", AssetRequestMode.ImmediateLoad);

        // ---- GRADIENT LUT TEXTURES ----
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

        /// <summary>
        /// Returns display name for the given attack mode.
        /// </summary>
        public static string GetModeName(AttackMode mode) => mode switch
        {
            AttackMode.ThrowSlam => "Throw Slam",
            AttackMode.ComboSwing => "Combo Swing",
            AttackMode.Astralgraph => "Astralgraph",
            AttackMode.FlamingRing => "Flaming Ring",
            AttackMode.RangerShot => "Ranger Shot",
            _ => "Unknown",
        };

        /// <summary>
        /// Returns theme-appropriate colors for the given attack mode.
        /// Index 0 = primary, 1 = secondary, 2 = core/highlight
        /// </summary>
        public static Color[] GetModeColors(AttackMode mode) => mode switch
        {
            AttackMode.ThrowSlam => new[] {
                new Color(60, 140, 255), new Color(120, 200, 255), new Color(220, 240, 255) },
            AttackMode.ComboSwing => new[] {
                new Color(255, 100, 50), new Color(255, 180, 60), new Color(255, 240, 180) },
            AttackMode.Astralgraph => new[] {
                new Color(100, 60, 220), new Color(160, 120, 255), new Color(220, 200, 255) },
            AttackMode.FlamingRing => new[] {
                new Color(255, 80, 30), new Color(255, 160, 40), new Color(255, 240, 120) },
            AttackMode.RangerShot => new[] {
                new Color(40, 220, 120), new Color(100, 255, 180), new Color(200, 255, 230) },
            _ => new[] { Color.White, Color.LightGray, Color.White },
        };

        /// <summary>
        /// Returns a short description of the attack mode for tooltips.
        /// </summary>
        public static string GetModeDescription(AttackMode mode) => mode switch
        {
            AttackMode.ThrowSlam => "Throws the sword skyward — it spins and slams the nearest enemy",
            AttackMode.ComboSwing => "Swing down, back up, then spin the blade toward the cursor",
            AttackMode.Astralgraph => "Summons an arcane astralgraph circle around the player",
            AttackMode.FlamingRing => "Creates a flaming ring that orbits the player",
            AttackMode.RangerShot => "Fires piercing bolts with special muzzle flash effects",
            _ => "Unknown attack mode",
        };
    }
}
