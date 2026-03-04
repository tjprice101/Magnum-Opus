using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy.Weapons.TheStandingOvation
{
    /// <summary>
    /// Texture registry for The Standing Ovation.
    /// Phantom spectator minions — applause waves, thrown roses, standing rush.
    /// Ovation Meter → Standing Ovation Event (shockwave + rose rain).
    /// </summary>
    public static class OvationTextures
    {
        private static Asset<Texture2D> _softGlow;
        private static Asset<Texture2D> _noiseFBM;
        private static Asset<Texture2D> _noisePerlin;
        private static Asset<Texture2D> _ojBlossomSparkle;
        private static Asset<Texture2D> _ojRosePetal;
        private static Asset<Texture2D> _ojPowerRing;
        private static Asset<Texture2D> _ojHarmonicImpact;
        private static Asset<Texture2D> _ojHarmonicWave2;
        private static Asset<Texture2D> _ojBasicTrail;
        private static Asset<Texture2D> _circularMask;
        private static Asset<Texture2D> _ojThornFragment;

        public static Texture2D SoftGlow => (_softGlow ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D NoiseFBM => (_noiseFBM ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/TileableFBMNoise", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D NoisePerlin => (_noisePerlin ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/TileablePerlinNoise", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D OJBlossomSparkle => (_ojBlossomSparkle ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Particles/OJ Blossom Sparkle", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D OJRosePetal => (_ojRosePetal ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Projectiles/OJ Rose Petal", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D OJPowerRing => (_ojPowerRing ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Impact Effects/OJ Power Effect Ring", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D OJHarmonicImpact => (_ojHarmonicImpact ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Impact Effects/OJ Harmonic Resonance Wave Impact", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D OJHarmonicWave2 => (_ojHarmonicWave2 ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Impact Effects/OJ Harmonic Resonance Wave Impact 2 (Floral)", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D OJBasicTrail => (_ojBasicTrail ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Trails and Ribbons/OJ Basic Trail", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D CircularMask => (_circularMask ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/CircularMask", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D OJThornFragment => (_ojThornFragment ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Particles/OJ Thorn Fragment", AssetRequestMode.ImmediateLoad)).Value;

        // OJ Palette
        public static readonly Color RoseShadow = new Color(100, 30, 50);
        public static readonly Color PetalPink = new Color(220, 100, 120);
        public static readonly Color BloomGold = new Color(255, 200, 50);
        public static readonly Color RadiantAmber = new Color(255, 170, 40);
        public static readonly Color JubilantLight = new Color(255, 250, 200);
        public static readonly Color PureJoyWhite = new Color(255, 255, 240);

        // Spectator-specific tints
        public static readonly Color SpectatorGold = new Color(255, 210, 80);
        public static readonly Color ApplauseFlash = new Color(255, 240, 180);
        public static readonly Color RoseRed = new Color(200, 40, 60);
        public static readonly Color RosePetalPink = new Color(230, 120, 140);

        /// <summary>Gets spectator tint based on crowd index for visual variety.</summary>
        public static Color GetSpectatorColor(int index)
        {
            return (index % 4) switch
            {
                0 => BloomGold,
                1 => RadiantAmber,
                2 => SpectatorGold,
                3 => JubilantLight,
                _ => BloomGold,
            };
        }
    }
}
