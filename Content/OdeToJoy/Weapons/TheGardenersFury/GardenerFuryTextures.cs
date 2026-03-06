using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy.Weapons.TheGardenersFury
{
    /// <summary>
    /// Self-contained texture registry for The Gardener's Fury.
    /// Loads shared VFX + OJ theme-specific assets.
    /// </summary>
    internal static class GardenerFuryTextures
    {
        // ---- PATHS ----
        private static readonly string SlashArcs = "MagnumOpus/Assets/VFX Asset Library/SlashArcs/";
        private static readonly string Bloom = "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/";
        private static readonly string GradientLib = "MagnumOpus/Assets/VFX Asset Library/ColorGradients/";
        private static readonly string NoiseLib = "MagnumOpus/Assets/VFX Asset Library/NoiseTextures/";
        private static readonly string Masks = "MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/";
        private static readonly string Impacts = "MagnumOpus/Assets/VFX Asset Library/ImpactEffects/";
        private static readonly string ThemeOJ = "MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/";

        // ---- SLASH ARC TEXTURES ----
        public static readonly Asset<Texture2D> SwordArcSmear =
            ModContent.Request<Texture2D>(SlashArcs + "SwordArcSmear", AssetRequestMode.ImmediateLoad);
        public static readonly Asset<Texture2D> FlamingSwordArc =
            ModContent.Request<Texture2D>(SlashArcs + "FlamingSwordArcSmear", AssetRequestMode.ImmediateLoad);

        // ---- BLOOM / GLOW ----
        public static readonly Asset<Texture2D> SoftGlow =
            ModContent.Request<Texture2D>(Bloom + "SoftGlow", AssetRequestMode.ImmediateLoad);
        public static readonly Asset<Texture2D> GlowOrb =
            ModContent.Request<Texture2D>(Bloom + "GlowOrb", AssetRequestMode.ImmediateLoad);
        public static readonly Asset<Texture2D> StarFlare =
            ModContent.Request<Texture2D>(Bloom + "StarFlare", AssetRequestMode.ImmediateLoad);
        public static readonly Asset<Texture2D> SoftRadialBloom =
            ModContent.Request<Texture2D>(Bloom + "SoftRadialBloom", AssetRequestMode.ImmediateLoad);

        // ---- NOISE TEXTURES ----
        public static readonly Asset<Texture2D> FBMNoise =
            ModContent.Request<Texture2D>(NoiseLib + "TileableFBMNoise", AssetRequestMode.ImmediateLoad);
        public static readonly Asset<Texture2D> VoronoiCell =
            ModContent.Request<Texture2D>(NoiseLib + "VoronoiCellNoise", AssetRequestMode.ImmediateLoad);

        // ---- GRADIENT ----
        public static readonly Asset<Texture2D> GradOdeToJoy =
            ModContent.Request<Texture2D>(GradientLib + "OdeToJoyGradientLUTandRAMP", AssetRequestMode.ImmediateLoad);

        // ---- MASKS ----
        public static readonly Asset<Texture2D> SoftCircle =
            ModContent.Request<Texture2D>(Masks + "SoftCircle", AssetRequestMode.ImmediateLoad);

        // ---- IMPACTS ----
        public static readonly Asset<Texture2D> PowerRing =
            ModContent.Request<Texture2D>(Impacts + "PowerEffectRing", AssetRequestMode.ImmediateLoad);
        public static readonly Asset<Texture2D> ImpactEllipse =
            ModContent.Request<Texture2D>(Impacts + "ImpactEllipse", AssetRequestMode.ImmediateLoad);

        // ---- ODE TO JOY THEME-SPECIFIC ----
        public static readonly Asset<Texture2D> OJHarmonicImpact =
            ModContent.Request<Texture2D>(ThemeOJ + "Impact Effects/OJ Harmonic Resonance Wave Impact", AssetRequestMode.ImmediateLoad);
        public static readonly Asset<Texture2D> OJFloralImpact =
            ModContent.Request<Texture2D>(ThemeOJ + "Impact Effects/OJ Harmonic Resonance Wave Impact 2 (Floral)", AssetRequestMode.ImmediateLoad);
        public static readonly Asset<Texture2D> OJPowerEffectRing =
            ModContent.Request<Texture2D>(ThemeOJ + "Impact Effects/OJ Power Effect Ring", AssetRequestMode.ImmediateLoad);
        public static readonly Asset<Texture2D> OJBeamSurgeImpact =
            ModContent.Request<Texture2D>(ThemeOJ + "Impact Effects/OJ Harmonic Resonance Beam Surge Impact", AssetRequestMode.ImmediateLoad);
        public static readonly Asset<Texture2D> OJBlossomSparkle =
            ModContent.Request<Texture2D>(ThemeOJ + "Particles/OJ Blossom Sparkle", AssetRequestMode.ImmediateLoad);
        public static readonly Asset<Texture2D> OJRosePetal =
            ModContent.Request<Texture2D>(ThemeOJ + "Projectiles/OJ Rose Petal", AssetRequestMode.ImmediateLoad);
        public static readonly Asset<Texture2D> OJPetalNoise =
            ModContent.Request<Texture2D>(ThemeOJ + "Noise/OJ Unique Theme Noise \u2014 Petal Scatter Pattern", AssetRequestMode.ImmediateLoad);

        // ---- SMEAR SHADER (SwordSmearFoundation) ----
        private static Effect _smearShader;
        private static bool _smearLoaded;

        public static Effect SmearDistortShader
        {
            get
            {
                if (!_smearLoaded)
                {
                    _smearLoaded = true;
                    try
                    {
                        _smearShader = ModContent.Request<Effect>(
                            "MagnumOpus/Content/FoundationWeapons/SwordSmearFoundation/Shaders/SmearDistortShader",
                            AssetRequestMode.ImmediateLoad).Value;
                    }
                    catch { _smearShader = null; }
                }
                return _smearShader;
            }
        }

        // ---- RADIAL NOISE MASK SHADER (MaskFoundation) ----
        private static Effect _radialShader;
        private static bool _radialLoaded;

        public static Effect RadialNoiseMaskShader
        {
            get
            {
                if (!_radialLoaded)
                {
                    _radialLoaded = true;
                    try
                    {
                        _radialShader = ModContent.Request<Effect>(
                            "MagnumOpus/Content/FoundationWeapons/MaskFoundation/Shaders/RadialNoiseMaskShader",
                            AssetRequestMode.ImmediateLoad).Value;
                    }
                    catch { _radialShader = null; }
                }
                return _radialShader;
            }
        }

        // ---- RIPPLE SHADER (ImpactFoundation) ----
        private static Effect _rippleShader;
        private static bool _rippleLoaded;

        public static Effect RippleShader
        {
            get
            {
                if (!_rippleLoaded)
                {
                    _rippleLoaded = true;
                    try
                    {
                        _rippleShader = ModContent.Request<Effect>(
                            "MagnumOpus/Content/FoundationWeapons/ImpactFoundation/Shaders/RippleShader",
                            AssetRequestMode.ImmediateLoad).Value;
                    }
                    catch { _rippleShader = null; }
                }
                return _rippleShader;
            }
        }

        // ---- ODE TO JOY COLOR PALETTE ----
        public static readonly Color RoseShadow = new Color(100, 30, 50);
        public static readonly Color PetalPink = new Color(220, 100, 120);
        public static readonly Color BloomGold = new Color(255, 200, 50);
        public static readonly Color RadiantAmber = new Color(255, 170, 40);
        public static readonly Color JubilantLight = new Color(255, 250, 200);
        public static readonly Color PureJoyWhite = new Color(255, 255, 240);
        public static readonly Color LoreColor = new Color(255, 200, 50);

        // Pod type colors
        public static readonly Color BloomPodCore = new Color(220, 100, 120);     // Petal Pink
        public static readonly Color ThornPodCore = new Color(100, 30, 50);       // Rose Shadow 
        public static readonly Color PollenPodCore = new Color(255, 200, 50);     // Bloom Gold

        /// <summary>Botanical gradient: RoseShadow → PetalPink → BloomGold → JubilantLight</summary>
        public static Color GetBotanicalGradient(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            if (t < 0.33f)
                return Color.Lerp(RoseShadow, PetalPink, t / 0.33f);
            if (t < 0.66f)
                return Color.Lerp(PetalPink, BloomGold, (t - 0.33f) / 0.33f);
            return Color.Lerp(BloomGold, JubilantLight, (t - 0.66f) / 0.34f);
        }

        /// <summary>Gets the core color for a seed pod type</summary>
        public static Color GetPodColor(int podType)
        {
            return podType switch
            {
                0 => BloomPodCore,  // Bloom Pod
                1 => ThornPodCore,  // Thorn Pod
                2 => PollenPodCore, // Pollen Pod
                _ => BloomGold
            };
        }
    }
}
