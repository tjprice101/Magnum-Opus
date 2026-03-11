using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy.Weapons.PetalStormCannon
{
    /// <summary>
    /// Self-contained texture registry for Petal Storm Cannon.
    /// MaskFoundation primary  Epersistent vortex zones with CosmicVortex noise.
    /// </summary>
    public static class PetalStormTextures
    {
        private static Asset<Texture2D> _softGlow;
        private static Asset<Texture2D> _noiseVortex;
        private static Asset<Texture2D> _noiseFBM;
        private static Asset<Texture2D> _ojGradient;
        private static Asset<Texture2D> _ojRosePetal;
        private static Asset<Texture2D> _ojBlossomSparkle;
        private static Asset<Texture2D> _ojPowerRing;
        private static Asset<Texture2D> _ojFloralImpact;
        private static Asset<Texture2D> _ojHarmonicImpact;
        private static Asset<Texture2D> _ojTrail;
        private static Asset<Texture2D> _SoftCircle;

        public static Texture2D SoftGlow => (_softGlow ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D NoiseVortex => (_noiseVortex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/CosmicEnergyVortex", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D NoiseFBM => (_noiseFBM ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/TileableFBMNoise", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D OJGradient => (_ojGradient ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/ColorGradients/OdeToJoyGradientLUTandRAMP", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D OJRosePetal => (_ojRosePetal ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Projectiles/OJ Rose Petal", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D OJBlossomSparkle => (_ojBlossomSparkle ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Particles/OJ Blossom Sparkle", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D OJPowerRing => (_ojPowerRing ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Impact Effects/OJ Power Effect Ring", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D OJFloralImpact => (_ojFloralImpact ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Impact Effects/OJ Harmonic Resonance Wave Impact 2 (Floral)", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D OJHarmonicImpact => (_ojHarmonicImpact ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Impact Effects/OJ Harmonic Resonance Wave Impact", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D OJTrail => (_ojTrail ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Trails and Ribbons/OJ Basic Trail", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D SoftCircle => (_SoftCircle ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/SoftCircle", AssetRequestMode.ImmediateLoad)).Value;

        // === OJ Color Palette ===
        public static readonly Color RoseShadow = new Color(100, 30, 50);
        public static readonly Color PetalPink = new Color(220, 100, 120);
        public static readonly Color BloomGold = new Color(255, 200, 50);
        public static readonly Color RadiantAmber = new Color(255, 170, 40);
        public static readonly Color JubilantLight = new Color(255, 250, 200);
        public static readonly Color PureJoyWhite = new Color(255, 255, 240);

        // Seasonal petal colors: pink ↁEgold ↁEwhite cycle
        public static Color GetSeasonalColor(int shotIndex)
        {
            return (shotIndex % 3) switch
            {
                0 => PetalPink,
                1 => BloomGold,
                2 => PureJoyWhite,
                _ => BloomGold
            };
        }

        public static readonly Color[] VortexColors = new[] { PetalPink, BloomGold, RadiantAmber, JubilantLight, PureJoyWhite };
    }
}
