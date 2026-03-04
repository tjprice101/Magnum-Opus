using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;

namespace MagnumOpus.Content.SwanLake
{
    /// <summary>
    /// Centralized texture registry for Swan Lake theme-specific VFX assets.
    /// Lazy-loads from Assets/VFX Asset Library/Theme Specific/Swan Lake/.
    /// All Swan Lake weapons should reference these instead of universal or made-up textures.
    /// </summary>
    internal static class SwanLakeThemeTextures
    {
        private const string ThemePath = "MagnumOpus/Assets/VFX Asset Library/Theme Specific/Swan Lake";
        private const string GradientPath = "MagnumOpus/Assets/VFX Asset Library/ColorGradients";

        // ══════════════════════════════════════════════════════
        //  BEAM TEXTURES
        // ══════════════════════════════════════════════════════

        private static Asset<Texture2D> _slEnergyMotionBeam;
        /// <summary>Swan Lake energy motion beam — flowing graceful beam body.</summary>
        public static Asset<Texture2D> SLEnergyMotionBeam =>
            _slEnergyMotionBeam ??= LoadTex($"{ThemePath}/Beam Textures/SL Energy Motion Beam");

        private static Asset<Texture2D> _slEnergySurgeBeam;
        /// <summary>Swan Lake energy surge beam — intense beam burst.</summary>
        public static Asset<Texture2D> SLEnergySurgeBeam =>
            _slEnergySurgeBeam ??= LoadTex($"{ThemePath}/Beam Textures/SL Energy Surge Beam");

        // ══════════════════════════════════════════════════════
        //  IMPACT EFFECTS
        // ══════════════════════════════════════════════════════

        private static Asset<Texture2D> _slHarmonicResonanceWaveImpact;
        /// <summary>Swan Lake harmonic resonance wave impact — elegant ripple burst.</summary>
        public static Asset<Texture2D> SLHarmonicImpact =>
            _slHarmonicResonanceWaveImpact ??= LoadTex($"{ThemePath}/Impact Effects/SL Harmonic Resonance Wave Impact");

        private static Asset<Texture2D> _slPowerEffectRing;
        /// <summary>Swan Lake power effect ring — prismatic expanding ring.</summary>
        public static Asset<Texture2D> SLPowerEffectRing =>
            _slPowerEffectRing ??= LoadTex($"{ThemePath}/Impact Effects/SL Power Effect Ring");

        // ══════════════════════════════════════════════════════
        //  PARTICLES
        // ══════════════════════════════════════════════════════

        private static Asset<Texture2D> _slCrystalShard;
        /// <summary>Swan Lake crystal shard — unique crystalline particle.</summary>
        public static Asset<Texture2D> SLCrystalShard =>
            _slCrystalShard ??= LoadTex($"{ThemePath}/Particles/SL Crystal Shard");

        // ══════════════════════════════════════════════════════
        //  TRAILS AND RIBBONS
        // ══════════════════════════════════════════════════════

        private static Asset<Texture2D> _slBasicTrail;
        /// <summary>Swan Lake basic trail — monochrome-to-prismatic trail strip.</summary>
        public static Asset<Texture2D> SLBasicTrail =>
            _slBasicTrail ??= LoadTex($"{ThemePath}/Trails and Ribbons/SL Basic Trail");

        private static Asset<Texture2D> _slHarmonicStandingWaveRibbon;
        /// <summary>Swan Lake harmonic standing wave ribbon — graceful flowing ribbon.</summary>
        public static Asset<Texture2D> SLHarmonicRibbon =>
            _slHarmonicStandingWaveRibbon ??= LoadTex($"{ThemePath}/Trails and Ribbons/SL Harmonic Standing Wave Ribbon");

        // ══════════════════════════════════════════════════════
        //  COLOR GRADIENTS
        // ══════════════════════════════════════════════════════

        private static Asset<Texture2D> _slGradient;
        /// <summary>Swan Lake gradient LUT — pure white to prismatic rainbow edges.</summary>
        public static Asset<Texture2D> SLGradient =>
            _slGradient ??= LoadTex($"{GradientPath}/SwanLakeGradient");

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
