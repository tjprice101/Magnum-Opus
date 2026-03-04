using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ThornboundReckoning
{
    /// <summary>
    /// Self-contained texture registry for Thornbound Reckoning.
    /// Loads all VFX assets from the shared library + Ode to Joy theme-specific assets.
    /// </summary>
    internal static class ThornboundTextures
    {
        // ---- PATHS ----
        private static readonly string SlashArcs = "MagnumOpus/Assets/VFX Asset Library/SlashArcs/";
        private static readonly string Bloom = "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/";
        private static readonly string GradientLib = "MagnumOpus/Assets/VFX Asset Library/ColorGradients/";
        private static readonly string NoiseLib = "MagnumOpus/Assets/VFX Asset Library/NoiseTextures/";
        private static readonly string Masks = "MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/";
        private static readonly string Impacts = "MagnumOpus/Assets/VFX Asset Library/ImpactEffects/";
        private static readonly string Trails = "MagnumOpus/Assets/VFX Asset Library/TrailsAndRibbons/";
        private static readonly string ThemeOJ = "MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/";

        // ---- SLASH ARC TEXTURES ----
        public static readonly Asset<Texture2D> FlamingSwordArc =
            ModContent.Request<Texture2D>(SlashArcs + "FlamingSwordArcSmear", AssetRequestMode.ImmediateLoad);
        public static readonly Asset<Texture2D> SwordArcSmear =
            ModContent.Request<Texture2D>(SlashArcs + "SwordArcSmear", AssetRequestMode.ImmediateLoad);

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

        // ---- GRADIENT LUT ----
        public static readonly Asset<Texture2D> GradOdeToJoy =
            ModContent.Request<Texture2D>(GradientLib + "OdeToJoyGradientLUTandRAMP", AssetRequestMode.ImmediateLoad);

        // ---- MASKS ----
        public static readonly Asset<Texture2D> SoftCircle =
            ModContent.Request<Texture2D>(Masks + "SoftCircle", AssetRequestMode.ImmediateLoad);

        // ---- TRAILS ----
        public static readonly Asset<Texture2D> BasicTrail =
            ModContent.Request<Texture2D>(Trails + "BasicTrail", AssetRequestMode.ImmediateLoad);

        // ---- IMPACTS ----
        public static readonly Asset<Texture2D> PowerRing =
            ModContent.Request<Texture2D>(Impacts + "PowerEffectRing", AssetRequestMode.ImmediateLoad);
        public static readonly Asset<Texture2D> ImpactEllipse =
            ModContent.Request<Texture2D>(Impacts + "ImpactEllipse", AssetRequestMode.ImmediateLoad);

        // ---- ODE TO JOY THEME-SPECIFIC ----
        public static readonly Asset<Texture2D> OJBasicTrail =
            ModContent.Request<Texture2D>(ThemeOJ + "Trails and Ribbons/OJ Basic Trail", AssetRequestMode.ImmediateLoad);
        public static readonly Asset<Texture2D> OJHarmonicImpact =
            ModContent.Request<Texture2D>(ThemeOJ + "Impact Effects/OJ Harmonic Resonance Wave Impact", AssetRequestMode.ImmediateLoad);
        public static readonly Asset<Texture2D> OJFloralImpact =
            ModContent.Request<Texture2D>(ThemeOJ + "Impact Effects/OJ Harmonic Resonance Wave Impact 2 (Floral)", AssetRequestMode.ImmediateLoad);
        public static readonly Asset<Texture2D> OJPowerRing =
            ModContent.Request<Texture2D>(ThemeOJ + "Impact Effects/OJ Power Effect Ring", AssetRequestMode.ImmediateLoad);
        public static readonly Asset<Texture2D> OJBlossomSparkle =
            ModContent.Request<Texture2D>(ThemeOJ + "Particles/OJ Blossom Sparkle", AssetRequestMode.ImmediateLoad);
        public static readonly Asset<Texture2D> OJThornFragment =
            ModContent.Request<Texture2D>(ThemeOJ + "Particles/OJ Thorn Fragment", AssetRequestMode.ImmediateLoad);
        public static readonly Asset<Texture2D> OJRosePetal =
            ModContent.Request<Texture2D>(ThemeOJ + "Projectiles/OJ Rose Petal", AssetRequestMode.ImmediateLoad);
        public static readonly Asset<Texture2D> OJPetalNoise =
            ModContent.Request<Texture2D>(ThemeOJ + "Noise/OJ Unique Theme Noise -- Petal Scatter Pattern", AssetRequestMode.ImmediateLoad);
        public static readonly Asset<Texture2D> OJPowerEffectRing =
            ModContent.Request<Texture2D>(ThemeOJ + "Impact Effects/OJ Power Effect Ring", AssetRequestMode.ImmediateLoad);
        public static readonly Asset<Texture2D> OJHarmonicWaveImpact =
            ModContent.Request<Texture2D>(ThemeOJ + "Impact Effects/OJ Harmonic Resonance Wave Impact", AssetRequestMode.ImmediateLoad);
        public static readonly Asset<Texture2D> OJBeamSurgeImpact =
            ModContent.Request<Texture2D>(ThemeOJ + "Impact Effects/OJ Harmonic Resonance Beam Surge Impact", AssetRequestMode.ImmediateLoad);

        // ---- DISTORTION SHADER (reuses SwordSmearFoundation's shader) ----
        private static Effect _smearShader;
        private static bool _shaderLoaded;

        public static Effect SmearDistortShader
        {
            get
            {
                if (!_shaderLoaded)
                {
                    _shaderLoaded = true;
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

        // ---- RADIAL NOISE MASK SHADER (reuses MaskFoundation's shader) ----
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

        // ---- IMPACT RIPPLE SHADER (reuses ImpactFoundation's shader) ----
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
        // Rose Shadow → Petal Pink → Bloom Gold → Radiant Amber → Jubilant Light → Pure Joy White
        public static readonly Color RoseShadow = new Color(100, 30, 50);
        public static readonly Color PetalPink = new Color(220, 100, 120);
        public static readonly Color BloomGold = new Color(255, 200, 50);
        public static readonly Color RadiantAmber = new Color(255, 170, 40);
        public static readonly Color JubilantLight = new Color(255, 250, 200);
        public static readonly Color PureJoyWhite = new Color(255, 255, 240);
        public static readonly Color LoreColor = new Color(255, 200, 50);

        /// <summary>Vine swing arc colors: outer, mid, core</summary>
        public static readonly Color[] VineSwingColors = new[]
        {
            RoseShadow,
            PetalPink,
            BloomGold,
        };

        /// <summary>Thorn lash colors: outer, mid, core</summary>
        public static readonly Color[] ThornLashColors = new[]
        {
            RoseShadow,
            RadiantAmber,
            JubilantLight,
        };

        /// <summary>Botanical burst colors: outer, mid, core</summary>
        public static readonly Color[] BotanicalBurstColors = new[]
        {
            PetalPink,
            BloomGold,
            PureJoyWhite,
        };

        /// <summary>Returns a gradient color from Rose Shadow → Bloom Gold → Jubilant Light</summary>
        public static Color GetBotanicalGradient(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            if (t < 0.33f)
                return Color.Lerp(RoseShadow, PetalPink, t / 0.33f);
            if (t < 0.66f)
                return Color.Lerp(PetalPink, BloomGold, (t - 0.33f) / 0.33f);
            return Color.Lerp(BloomGold, JubilantLight, (t - 0.66f) / 0.34f);
        }
    }
}
