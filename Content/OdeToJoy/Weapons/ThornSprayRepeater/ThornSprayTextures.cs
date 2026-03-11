using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ThornSprayRepeater
{
    /// <summary>
    /// Self-contained texture registry for Thorn Spray Repeater.
    /// Loads VFX assets, shaders, and OJ palette colors.
    /// SparkleProjectileFoundation primary.
    /// </summary>
    public static class ThornSprayTextures
    {
        // === VFX Textures ===
        private static Asset<Texture2D> _softGlow;
        private static Asset<Texture2D> _noisePerlin;
        private static Asset<Texture2D> _noiseCellular;
        private static Asset<Texture2D> _ojGradient;
        private static Asset<Texture2D> _ojThornFrag;
        private static Asset<Texture2D> _ojBlossomSparkle;
        private static Asset<Texture2D> _ojPowerRing;
        private static Asset<Texture2D> _ojBeamSurge;
        private static Asset<Texture2D> _ojTrail;
        private static Asset<Texture2D> _SoftCircle;

        public static Texture2D SoftGlow => (_softGlow ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D NoisePerlin => (_noisePerlin ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/PerlinNoise", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D NoiseCellular => (_noiseCellular ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/VoronoiCellNoise", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D OJGradient => (_ojGradient ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/ColorGradients/OdeToJoyGradientLUTandRAMP", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D OJThornFragment => (_ojThornFrag ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Particles/OJ Thorn Fragment", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D OJBlossomSparkle => (_ojBlossomSparkle ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Particles/OJ Blossom Sparkle", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D OJPowerRing => (_ojPowerRing ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Impact Effects/OJ Power Effect Ring", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D OJBeamSurge => (_ojBeamSurge ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Impact Effects/OJ Harmonic Resonance Beam Surge Impact", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D OJTrail => (_ojTrail ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Trails and Ribbons/OJ Basic Trail", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D SoftCircle => (_SoftCircle ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/SoftCircle", AssetRequestMode.ImmediateLoad)).Value;

        // === OJ Color Palette ===
        public static readonly Color RoseShadow = new Color(100, 30, 50);
        public static readonly Color PetalPink = new Color(220, 100, 120);
        public static readonly Color BloomGold = new Color(255, 200, 50);
        public static readonly Color RadiantAmber = new Color(255, 170, 40);
        public static readonly Color JubilantLight = new Color(255, 250, 200);
        public static readonly Color PureJoyWhite = new Color(255, 255, 240);

        // === Thorn Spray Colors ===
        public static readonly Color[] CrystallineThornColors = new[]
        {
            RoseShadow, PetalPink, BloomGold, RadiantAmber, JubilantLight
        };

        public static readonly Color[] BloomThornColors = new[]
        {
            BloomGold, RadiantAmber, JubilantLight, PureJoyWhite, BloomGold
        };

        public static readonly Color[] DetonationColors = new[]
        {
            RoseShadow, PetalPink, RadiantAmber, JubilantLight
        };

        /// <summary>
        /// Gets a gradient color for thorn projectile based on lifetime progress.
        /// </summary>
        public static Color GetThornGradient(float progress, bool isBloomThorn)
        {
            Color[] palette = isBloomThorn ? BloomThornColors : CrystallineThornColors;
            float scaled = progress * (palette.Length - 1);
            int idx = (int)scaled;
            float frac = scaled - idx;
            if (idx >= palette.Length - 1) return palette[palette.Length - 1];
            return Color.Lerp(palette[idx], palette[idx + 1], frac);
        }
    }
}
