using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ElysianVerdict
{
    /// <summary>
    /// Self-contained texture registry for Elysian Verdict.
    /// Golden orb with prismatic edges. 3-tier Judgment Marks.
    /// Paradise Lost mode at low HP. MagicOrbFoundation primary.
    /// </summary>
    public static class ElysianTextures
    {
        private static Asset<Texture2D> _softGlow;
        private static Asset<Texture2D> _noisePerlin;
        private static Asset<Texture2D> _noiseVoronoi;
        private static Asset<Texture2D> _ojGradient;
        private static Asset<Texture2D> _ojBlossomSparkle;
        private static Asset<Texture2D> _ojPowerRing;
        private static Asset<Texture2D> _ojHarmonicImpact;
        private static Asset<Texture2D> _ojFloralImpact;
        private static Asset<Texture2D> _circularMask;

        public static Texture2D SoftGlow => (_softGlow ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D NoisePerlin => (_noisePerlin ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/TileablePerlinFlowNoise", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D NoiseVoronoi => (_noiseVoronoi ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/TileableVoronoiCellNoise", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D OJGradient => (_ojGradient ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/ColorGradients/OdeToJoyGradientLUTandRAMP", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D OJBlossomSparkle => (_ojBlossomSparkle ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Particles/OJ Blossom Sparkle", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D OJPowerRing => (_ojPowerRing ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Impact Effects/OJ Power Effect Ring", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D OJHarmonicImpact => (_ojHarmonicImpact ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Impact Effects/OJ Harmonic Resonance Wave Impact", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D OJFloralImpact => (_ojFloralImpact ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Impact Effects/OJ Harmonic Resonance Wave Impact 2 (Floral)", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D CircularMask => (_circularMask ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/CircularMask", AssetRequestMode.ImmediateLoad)).Value;

        // --- OJ Palette ---
        public static readonly Color RoseShadow = new Color(100, 30, 50);
        public static readonly Color PetalPink = new Color(220, 100, 120);
        public static readonly Color BloomGold = new Color(255, 200, 50);
        public static readonly Color RadiantAmber = new Color(255, 170, 40);
        public static readonly Color JubilantLight = new Color(255, 250, 200);
        public static readonly Color PureJoyWhite = new Color(255, 255, 240);

        // --- Paradise Lost corrupted palette ---
        public static readonly Color CorruptedGold = new Color(180, 130, 30);
        public static readonly Color CrimsonEdge = new Color(180, 40, 40);

        /// <summary>
        /// Gets orb color based on Paradise Lost state.
        /// </summary>
        public static Color GetOrbColor(bool paradiseLost, float intensity)
        {
            if (paradiseLost)
                return Color.Lerp(CorruptedGold, CrimsonEdge, intensity);
            return Color.Lerp(BloomGold, JubilantLight, intensity);
        }
    }
}
