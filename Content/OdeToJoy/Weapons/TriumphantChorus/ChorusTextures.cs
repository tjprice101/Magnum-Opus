using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy.Weapons.TriumphantChorus
{
    /// <summary>
    /// Texture registry for Triumphant Chorus.
    /// 4 vocal parts (Soprano/Alto/Tenor/Bass) with distinct colors.
    /// MagicOrbFoundation bodies + AttackFoundation sound waves.
    /// </summary>
    public static class ChorusTextures
    {
        private static Asset<Texture2D> _softGlow;
        private static Asset<Texture2D> _noiseFBM;
        private static Asset<Texture2D> _ojBlossomSparkle;
        private static Asset<Texture2D> _ojPowerRing;
        private static Asset<Texture2D> _ojHarmonicImpact;
        private static Asset<Texture2D> _circularMask;

        public static Texture2D SoftGlow => (_softGlow ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D NoiseFBM => (_noiseFBM ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/TileableFBMNoise", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D OJBlossomSparkle => (_ojBlossomSparkle ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Particles/OJ Blossom Sparkle", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D OJPowerRing => (_ojPowerRing ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Impact Effects/OJ Power Effect Ring", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D OJHarmonicImpact => (_ojHarmonicImpact ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Impact Effects/OJ Harmonic Resonance Wave Impact", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D CircularMask => (_circularMask ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/CircularMask", AssetRequestMode.ImmediateLoad)).Value;

        // OJ Palette
        public static readonly Color RoseShadow = new Color(100, 30, 50);
        public static readonly Color PetalPink = new Color(220, 100, 120);
        public static readonly Color BloomGold = new Color(255, 200, 50);
        public static readonly Color RadiantAmber = new Color(255, 170, 40);
        public static readonly Color JubilantLight = new Color(255, 250, 200);
        public static readonly Color PureJoyWhite = new Color(255, 255, 240);

        // Per-voice colors
        public static readonly Color SopranoColor = new Color(255, 250, 200); // Jubilant Light — highest
        public static readonly Color AltoColor = new Color(255, 200, 50);     // Bloom Gold — warm mid
        public static readonly Color TenorColor = new Color(255, 170, 40);   // Radiant Amber — deeper
        public static readonly Color BassColor = new Color(220, 100, 120);   // Petal Pink — deepest/warmest

        /// <summary>Returns color for voice type (0=Soprano, 1=Alto, 2=Tenor, 3=Bass).</summary>
        public static Color GetVoiceColor(int voice)
        {
            return (voice % 4) switch
            {
                0 => SopranoColor,
                1 => AltoColor,
                2 => TenorColor,
                3 => BassColor,
                _ => BloomGold,
            };
        }
    }
}
