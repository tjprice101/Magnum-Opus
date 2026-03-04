using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ThePollinator
{
    /// <summary>
    /// Self-contained texture registry for The Pollinator.
    /// SmokeFoundation primary — pollen clouds, golden fields, mass bloom.
    /// </summary>
    public static class PollinatorTextures
    {
        private static Asset<Texture2D> _softGlow;
        private static Asset<Texture2D> _noiseFBM;
        private static Asset<Texture2D> _noisePerlin;
        private static Asset<Texture2D> _ojGradient;
        private static Asset<Texture2D> _ojBlossomSparkle;
        private static Asset<Texture2D> _ojRosePetal;
        private static Asset<Texture2D> _ojPowerRing;
        private static Asset<Texture2D> _ojFloralImpact;
        private static Asset<Texture2D> _ojBeamSurge;
        private static Asset<Texture2D> _circularMask;

        public static Texture2D SoftGlow => (_softGlow ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX/Blooms/SoftGlow", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D NoiseFBM => (_noiseFBM ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX/Noise/TileableFBMNoise", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D NoisePerlin => (_noisePerlin ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX/Noise/TileablePerlinFlowNoise", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D OJGradient => (_ojGradient ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/ColorGradients/OdeToJoyGradientLUTandRAMP", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D OJBlossomSparkle => (_ojBlossomSparkle ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Particles/OJ Blossom Sparkle", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D OJRosePetal => (_ojRosePetal ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Projectiles/OJ Rose Petal", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D OJPowerRing => (_ojPowerRing ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Impact Effects/OJ Power Effect Ring", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D OJFloralImpact => (_ojFloralImpact ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Impact Effects/OJ Harmonic Resonance Wave Impact 2 (Floral)", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D OJBeamSurge => (_ojBeamSurge ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Impact Effects/OJ Harmonic Resonance Beam Surge Impact", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D CircularMask => (_circularMask ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX/Masks/CircularMask", AssetRequestMode.ImmediateLoad)).Value;

        // === OJ Color Palette ===
        public static readonly Color RoseShadow = new Color(100, 30, 50);
        public static readonly Color PetalPink = new Color(220, 100, 120);
        public static readonly Color BloomGold = new Color(255, 200, 50);
        public static readonly Color RadiantAmber = new Color(255, 170, 40);
        public static readonly Color JubilantLight = new Color(255, 250, 200);
        public static readonly Color PureJoyWhite = new Color(255, 255, 240);

        public static readonly Color[] PollenColors = new[] { BloomGold, RadiantAmber, JubilantLight, PureJoyWhite };
        public static readonly Color[] MassBloomColors = new[] { PetalPink, BloomGold, RadiantAmber, JubilantLight };
    }
}
