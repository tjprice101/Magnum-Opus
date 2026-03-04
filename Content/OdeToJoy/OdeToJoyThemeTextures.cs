using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy
{
    /// <summary>
    /// Centralized texture registry for Ode to Joy theme-specific VFX assets.
    /// Lazy-loads from Assets/VFX Asset Library/Theme Specific/Ode to Joy/.
    /// All Ode to Joy weapons should reference these instead of universal or made-up textures.
    /// </summary>
    internal static class OdeToJoyThemeTextures
    {
        private const string ThemePath = "MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy";
        private const string GradientPath = "MagnumOpus/Assets/VFX Asset Library/ColorGradients";

        // ══════════════════════════════════════════════════════
        //  BEAM TEXTURES
        // ══════════════════════════════════════════════════════

        private static Asset<Texture2D> _ojEnergyMotionBeam;
        /// <summary>Ode to Joy energy motion beam — jubilant flowing beam.</summary>
        public static Asset<Texture2D> OJEnergyMotionBeam =>
            _ojEnergyMotionBeam ??= LoadTex($"{ThemePath}/Beam Textures/OJ Energy Motion Beam");

        private static Asset<Texture2D> _ojEnergySurgeBeam;
        /// <summary>Ode to Joy energy surge beam — triumphant beam burst.</summary>
        public static Asset<Texture2D> OJEnergySurgeBeam =>
            _ojEnergySurgeBeam ??= LoadTex($"{ThemePath}/Beam Textures/OJ Energy Surge Beam");

        // ══════════════════════════════════════════════════════
        //  IMPACT EFFECTS
        // ══════════════════════════════════════════════════════

        private static Asset<Texture2D> _ojHarmonicBeamSurge;
        /// <summary>Ode to Joy harmonic resonance beam surge impact — radiant burst.</summary>
        public static Asset<Texture2D> OJHarmonicBeamSurge =>
            _ojHarmonicBeamSurge ??= LoadTex($"{ThemePath}/Impact Effects/OJ Harmonic Resonance Beam Surge Impact");

        private static Asset<Texture2D> _ojHarmonicWaveFloral;
        /// <summary>Ode to Joy harmonic resonance wave impact (floral variant) — petal-shaped burst.</summary>
        public static Asset<Texture2D> OJHarmonicWaveFloral =>
            _ojHarmonicWaveFloral ??= LoadTex($"{ThemePath}/Impact Effects/OJ Harmonic Resonance Wave Impact 2 (Floral)");

        private static Asset<Texture2D> _ojHarmonicWaveImpact;
        /// <summary>Ode to Joy harmonic resonance wave impact — joyful ripple burst.</summary>
        public static Asset<Texture2D> OJHarmonicImpact =>
            _ojHarmonicWaveImpact ??= LoadTex($"{ThemePath}/Impact Effects/OJ Harmonic Resonance Wave Impact");

        private static Asset<Texture2D> _ojPowerEffectRing;
        /// <summary>Ode to Joy power effect ring — golden expanding ring.</summary>
        public static Asset<Texture2D> OJPowerEffectRing =>
            _ojPowerEffectRing ??= LoadTex($"{ThemePath}/Impact Effects/OJ Power Effect Ring");

        // ══════════════════════════════════════════════════════
        //  NOISE TEXTURES
        // ══════════════════════════════════════════════════════

        private static Asset<Texture2D> _ojPetalScatterNoise;
        /// <summary>Ode to Joy petal scatter pattern — unique floral noise.</summary>
        public static Asset<Texture2D> OJPetalScatterNoise =>
            _ojPetalScatterNoise ??= LoadTex($"{ThemePath}/Noise/OJ Unique Theme Noise \u2014 Petal Scatter Pattern");

        // ══════════════════════════════════════════════════════
        //  PARTICLES
        // ══════════════════════════════════════════════════════

        private static Asset<Texture2D> _ojBlossomSparkle;
        /// <summary>Ode to Joy blossom sparkle — radiant flower sparkle particle.</summary>
        public static Asset<Texture2D> OJBlossomSparkle =>
            _ojBlossomSparkle ??= LoadTex($"{ThemePath}/Particles/OJ Blossom Sparkle");

        private static Asset<Texture2D> _ojThornFragment;
        /// <summary>Ode to Joy thorn fragment — sharp thorn debris particle.</summary>
        public static Asset<Texture2D> OJThornFragment =>
            _ojThornFragment ??= LoadTex($"{ThemePath}/Particles/OJ Thorn Fragment");

        // ══════════════════════════════════════════════════════
        //  PROJECTILES
        // ══════════════════════════════════════════════════════

        private static Asset<Texture2D> _ojBlossomSparkleProj;
        /// <summary>Ode to Joy blossom sparkle projectile — radiant floral projectile sprite.</summary>
        public static Asset<Texture2D> OJBlossomSparkleProj =>
            _ojBlossomSparkleProj ??= LoadTex($"{ThemePath}/Projectiles/OJ Blossom Sparkle");

        private static Asset<Texture2D> _ojRosePetal;
        /// <summary>Ode to Joy rose petal projectile — graceful petal projectile sprite.</summary>
        public static Asset<Texture2D> OJRosePetal =>
            _ojRosePetal ??= LoadTex($"{ThemePath}/Projectiles/OJ Rose Petal");

        // ══════════════════════════════════════════════════════
        //  TRAILS AND RIBBONS
        // ══════════════════════════════════════════════════════

        private static Asset<Texture2D> _ojBasicTrail;
        /// <summary>Ode to Joy basic trail — golden jubilant trail strip.</summary>
        public static Asset<Texture2D> OJBasicTrail =>
            _ojBasicTrail ??= LoadTex($"{ThemePath}/Trails and Ribbons/OJ Basic Trail");

        // ══════════════════════════════════════════════════════
        //  COLOR GRADIENTS
        // ══════════════════════════════════════════════════════

        private static Asset<Texture2D> _ojGradientLUT;
        /// <summary>Ode to Joy gradient LUT — warm gold to radiant amber to jubilant light.</summary>
        public static Asset<Texture2D> OJGradientLUT =>
            _ojGradientLUT ??= LoadTex($"{GradientPath}/OdeToJoyGradientLUTandRAMP");

        // ══════════════════════════════════════════════════════
        //  LOADER
        // ══════════════════════════════════════════════════════

        private static Asset<Texture2D> LoadTex(string path)
        {
            if (!ModContent.HasAsset(path)) return null;
            return ModContent.Request<Texture2D>(path, AssetRequestMode.ImmediateLoad);
        }
    }
}
