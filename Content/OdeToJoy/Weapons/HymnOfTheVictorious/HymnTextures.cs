using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy.Weapons.HymnOfTheVictorious
{
    /// <summary>
    /// Self-contained texture registry for Hymn of the Victorious.
    /// 4-verse cycle system (Exordium/Rising/Apex/Gloria).
    /// MagicOrbFoundation primary with per-verse color/noise variants.
    /// </summary>
    public static class HymnTextures
    {
        // --- VFX Assets ---
        private static Asset<Texture2D> _softGlow;
        private static Asset<Texture2D> _noiseFBM;
        private static Asset<Texture2D> _noisePerlin;
        private static Asset<Texture2D> _noiseVoronoi;

        // --- OJ Theme ---
        private static Asset<Texture2D> _ojGradient;
        private static Asset<Texture2D> _ojBlossomSparkle;
        private static Asset<Texture2D> _ojPowerRing;
        private static Asset<Texture2D> _ojHarmonicImpact;
        private static Asset<Texture2D> _ojFloralImpact;
        private static Asset<Texture2D> _ojBeamSurge;

        // --- Library ---
        public static Texture2D SoftGlow => (_softGlow ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D NoiseFBM => (_noiseFBM ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/TileableFBMNoise", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D NoisePerlin => (_noisePerlin ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/PerlinNoise", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D NoiseVoronoi => (_noiseVoronoi ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/VoronoiCellNoise", AssetRequestMode.ImmediateLoad)).Value;

        // --- OJ Theme ---
        public static Texture2D OJGradient => (_ojGradient ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/ColorGradients/OdeToJoyGradientLUTandRAMP", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D OJBlossomSparkle => (_ojBlossomSparkle ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Particles/OJ Blossom Sparkle", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D OJPowerRing => (_ojPowerRing ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Impact Effects/OJ Power Effect Ring", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D OJHarmonicImpact => (_ojHarmonicImpact ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Impact Effects/OJ Harmonic Resonance Wave Impact", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D OJFloralImpact => (_ojFloralImpact ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Impact Effects/OJ Harmonic Resonance Wave Impact 2 (Floral)", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D OJBeamSurge => (_ojBeamSurge ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Beam Textures/OJ Energy Surge Beam", AssetRequestMode.ImmediateLoad)).Value;

        // --- OJ Palette ---
        public static readonly Color RoseShadow = new Color(100, 30, 50);
        public static readonly Color PetalPink = new Color(220, 100, 120);
        public static readonly Color BloomGold = new Color(255, 200, 50);
        public static readonly Color RadiantAmber = new Color(255, 170, 40);
        public static readonly Color JubilantLight = new Color(255, 250, 200);
        public static readonly Color PureJoyWhite = new Color(255, 255, 240);

        // --- Per-Verse Color Palettes ---
        // V1 Exordium: Pure Gold ↁERadiant Amber (clean, piercing)
        public static readonly Color[] ExordiumColors = { new Color(255, 200, 50), new Color(255, 170, 40), new Color(255, 250, 200) };
        // V2 Rising: Petal Pink ↁEBloom Gold (warmer, spreading)
        public static readonly Color[] RisingColors = { new Color(220, 100, 120), new Color(255, 200, 50), new Color(255, 170, 40) };
        // V3 Apex: Bloom Gold ↁEJubilant Light (largest, brightest)
        public static readonly Color[] ApexColors = { new Color(255, 200, 50), new Color(255, 250, 200), new Color(255, 255, 240) };
        // V4 Gloria: Radiant Amber ↁEPure Joy White (fracturing, splitting)
        public static readonly Color[] GloriaColors = { new Color(255, 170, 40), new Color(255, 250, 200), new Color(255, 255, 240) };

        /// <summary>
        /// Returns the primary color for a verse type (0-3).
        /// </summary>
        public static Color GetVerseColor(int verse)
        {
            return (verse % 4) switch
            {
                0 => BloomGold,       // Exordium
                1 => PetalPink,       // Rising
                2 => JubilantLight,   // Apex
                3 => RadiantAmber,    // Gloria
                _ => BloomGold,
            };
        }

        /// <summary>
        /// Returns the verse color array for a verse type (0-3).
        /// </summary>
        public static Color[] GetVerseColorArray(int verse)
        {
            return (verse % 4) switch
            {
                0 => ExordiumColors,
                1 => RisingColors,
                2 => ApexColors,
                3 => GloriaColors,
                _ => ExordiumColors,
            };
        }
    }
}
