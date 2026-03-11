using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy.Weapons.AnthemOfGlory
{
    /// <summary>
    /// Self-contained texture registry for Anthem of Glory.
    /// Channeled golden beam + Glory Notes + Victory Fanfare.
    /// LaserFoundation primary, SparkleProjectileFoundation notes.
    /// </summary>
    public static class AnthemTextures
    {
        // --- VFX Asset Library Textures ---
        private static Asset<Texture2D> _softGlow;
        private static Asset<Texture2D> _noiseFBM;
        private static Asset<Texture2D> _noisePerlin;
        private static Asset<Texture2D> _noiseVoronoi;

        // --- OJ Theme-Specific ---
        private static Asset<Texture2D> _ojGradient;
        private static Asset<Texture2D> _ojBeamEnergy;
        private static Asset<Texture2D> _ojBeamSurge;
        private static Asset<Texture2D> _ojBlossomSparkle;
        private static Asset<Texture2D> _ojPowerRing;
        private static Asset<Texture2D> _ojHarmonicImpact;
        private static Asset<Texture2D> _ojFloralImpact;
        private static Asset<Texture2D> _ojTrail;

        // --- Library Textures ---
        public static Texture2D SoftGlow => (_softGlow ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D NoiseFBM => (_noiseFBM ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/TileableFBMNoise", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D NoisePerlin => (_noisePerlin ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/PerlinNoise", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D NoiseVoronoi => (_noiseVoronoi ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/VoronoiCellNoise", AssetRequestMode.ImmediateLoad)).Value;

        // --- OJ Theme Assets ---
        public static Texture2D OJGradient => (_ojGradient ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/ColorGradients/OdeToJoyGradientLUTandRAMP", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D OJBeamEnergy => (_ojBeamEnergy ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Beam Textures/OJ Energy Motion Beam", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D OJBeamSurge => (_ojBeamSurge ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Beam Textures/OJ Energy Surge Beam", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D OJBlossomSparkle => (_ojBlossomSparkle ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Particles/OJ Blossom Sparkle", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D OJPowerRing => (_ojPowerRing ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Impact Effects/OJ Power Effect Ring", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D OJHarmonicImpact => (_ojHarmonicImpact ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Impact Effects/OJ Harmonic Resonance Wave Impact", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D OJFloralImpact => (_ojFloralImpact ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Impact Effects/OJ Harmonic Resonance Wave Impact 2 (Floral)", AssetRequestMode.ImmediateLoad)).Value;
        public static Texture2D OJTrail => (_ojTrail ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Trails and Ribbons/OJ Basic Trail", AssetRequestMode.ImmediateLoad)).Value;

        // --- OJ Color Palette ---
        public static readonly Color RoseShadow = new Color(100, 30, 50);
        public static readonly Color PetalPink = new Color(220, 100, 120);
        public static readonly Color BloomGold = new Color(255, 200, 50);
        public static readonly Color RadiantAmber = new Color(255, 170, 40);
        public static readonly Color JubilantLight = new Color(255, 250, 200);
        public static readonly Color PureJoyWhite = new Color(255, 255, 240);

        // --- Beam gradient colors (edge ↁEcore) ---
        public static readonly Color[] BeamGradient = new Color[]
        {
            new Color(100, 30, 50),   // Rose Shadow edge
            new Color(220, 100, 120), // Petal Pink
            new Color(255, 200, 50),  // Bloom Gold
            new Color(255, 170, 40),  // Radiant Amber core
            new Color(255, 250, 200), // Jubilant Light hot center
        };

        // --- Glory Note colors cycle ---
        public static readonly Color[] NoteColors = new Color[]
        {
            new Color(255, 200, 50),  // Bloom Gold
            new Color(255, 170, 40),  // Radiant Amber
            new Color(255, 250, 200), // Jubilant Light
            new Color(255, 255, 240), // Pure Joy White
        };

        /// <summary>
        /// Gets beam color interpolated across width (0 = edge, 1 = center).
        /// </summary>
        public static Color GetBeamColor(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            float idx = t * (BeamGradient.Length - 1);
            int lo = (int)idx;
            int hi = Math.Min(lo + 1, BeamGradient.Length - 1);
            float frac = idx - lo;
            return Color.Lerp(BeamGradient[lo], BeamGradient[hi], frac);
        }

        /// <summary>
        /// Lazy-loaded shader for OdeToJoy beam effects.
        /// </summary>
        private static Effect _triumphantTrail;
        public static Effect TriumphantTrailShader
        {
            get
            {
                if (_triumphantTrail != null) return _triumphantTrail;
                try
                {
                    _triumphantTrail = ModContent.Request<Effect>("MagnumOpus/Effects/OdeToJoy/TriumphantTrail", AssetRequestMode.ImmediateLoad).Value;
                }
                catch { _triumphantTrail = null; }
                return _triumphantTrail;
            }
        }

        private static Effect _jubilantHarmony;
        public static Effect JubilantHarmonyShader
        {
            get
            {
                if (_jubilantHarmony != null) return _jubilantHarmony;
                try
                {
                    _jubilantHarmony = ModContent.Request<Effect>("MagnumOpus/Effects/OdeToJoy/JubilantHarmony", AssetRequestMode.ImmediateLoad).Value;
                }
                catch { _jubilantHarmony = null; }
                return _jubilantHarmony;
            }
        }
    }
}
