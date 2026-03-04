using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy.Weapons.FountainOfJoyousHarmony
{
    /// <summary>
    /// Texture registry for Fountain of Joyous Harmony.
    /// Stationary fountain minion — heals, fires golden droplets, Harmony Zone, Geyser.
    /// MaskFoundation zone + SparkleProjectileFoundation droplets + ImpactFoundation geyser.
    /// </summary>
    public static class FountainTextures
    {
        private static Asset<Texture2D> _softGlow;
        private static Asset<Texture2D> _noiseFBM;
        private static Asset<Texture2D> _noisePerlin;
        private static Asset<Texture2D> _ojBlossomSparkle;
        private static Asset<Texture2D> _ojPowerRing;
        private static Asset<Texture2D> _ojHarmonicImpact;
        private static Asset<Texture2D> _ojHarmonicWave2;
        private static Asset<Texture2D> _ojBasicTrail;
        private static Asset<Texture2D> _circularMask;
        private static Asset<Texture2D> _ojBeamEnergy;

        public static Texture2D SoftGlow => (_softGlow ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D NoiseFBM => (_noiseFBM ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/TileableFBMNoise", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D NoisePerlin => (_noisePerlin ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/TileablePerlinFlowNoise", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D OJBlossomSparkle => (_ojBlossomSparkle ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Particles/OJ Blossom Sparkle", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D OJPowerRing => (_ojPowerRing ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Impact Effects/OJ Power Effect Ring", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D OJHarmonicImpact => (_ojHarmonicImpact ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Impact Effects/OJ Harmonic Resonance Wave Impact", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D OJHarmonicWave2 => (_ojHarmonicWave2 ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Impact Effects/OJ Harmonic Resonance Wave Impact 2 (Floral)", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D OJBasicTrail => (_ojBasicTrail ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Trails and Ribbons/OJ Basic Trail", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D CircularMask => (_circularMask ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/CircularMask", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D OJBeamEnergy => (_ojBeamEnergy ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Beam Textures/OJ Energy Motion Beam", AssetRequestMode.ImmediateLoad)).Value;

        // OJ Palette
        public static readonly Color RoseShadow = new Color(100, 30, 50);
        public static readonly Color PetalPink = new Color(220, 100, 120);
        public static readonly Color BloomGold = new Color(255, 200, 50);
        public static readonly Color RadiantAmber = new Color(255, 170, 40);
        public static readonly Color JubilantLight = new Color(255, 250, 200);
        public static readonly Color PureJoyWhite = new Color(255, 255, 240);

        // Fountain-specific
        public static readonly Color FountainCore = new Color(255, 230, 120);
        public static readonly Color DropletGold = new Color(255, 215, 60);
        public static readonly Color HealGreen = new Color(120, 230, 100);
        public static readonly Color GeyserWhite = new Color(255, 250, 230);

        /// <summary>Gets droplet color with variance for visual interest.</summary>
        public static Color GetDropletColor(int index)
        {
            return (index % 3) switch
            {
                0 => BloomGold,
                1 => DropletGold,
                2 => RadiantAmber,
                _ => BloomGold,
            };
        }
    }
}
