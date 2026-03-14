using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Nachtmusik
{
    /// <summary>
    /// Centralized texture registry for Nachtmusik theme-specific VFX assets.
    /// Lazy-loads from Assets/VFX Asset Library/Theme Specific/Nachtmusik/.
    /// All Nachtmusik weapons should reference these instead of universal or made-up textures.
    /// </summary>
    internal static class NachtmusikThemeTextures
    {
        private const string ThemePath = "MagnumOpus/Assets/VFX Asset Library/Theme Specific/Nachtmusik";
        private const string GradientPath = "MagnumOpus/Assets/VFX Asset Library/ColorGradients";

        // ══════════════════════════════════════════════════════
        //  BEAM TEXTURES
        // ══════════════════════════════════════════════════════

        private static Asset<Texture2D> _nkEnergyMotionBeam;
        /// <summary>Nachtmusik energy motion beam — flowing cosmic beam body.</summary>
        public static Asset<Texture2D> NKEnergyMotionBeam =>
            _nkEnergyMotionBeam ??= LoadTex($"{ThemePath}/Beam Textures/NK Energy Motion Beam");

        private static Asset<Texture2D> _nkEnergySurgeBeam;
        /// <summary>Nachtmusik energy surge beam — intense stellar beam burst.</summary>
        public static Asset<Texture2D> NKEnergySurgeBeam =>
            _nkEnergySurgeBeam ??= LoadTex($"{ThemePath}/Beam Textures/NK Energy Surge Beam");

        // ══════════════════════════════════════════════════════
        //  IMPACT EFFECTS
        // ══════════════════════════════════════════════════════

        private static Asset<Texture2D> _nkPowerEffectRing;
        /// <summary>Nachtmusik power effect ring — stellar expanding ring.</summary>
        public static Asset<Texture2D> NKPowerEffectRing =>
            _nkPowerEffectRing ??= LoadTex($"{ThemePath}/Particles/NK Power Effect Ring");

        private static Asset<Texture2D> _nkRadialSlashImpact;
        /// <summary>Nachtmusik radial slash star impact — nocturnal slash burst.</summary>
        public static Asset<Texture2D> NKRadialSlashImpact =>
            _nkRadialSlashImpact ??= LoadTex($"{ThemePath}/Impact Effects/NK Radial Slash Star Impact");

        private static Asset<Texture2D> _nkComet;
        /// <summary>Nachtmusik comet — streaking celestial impact projectile.</summary>
        public static Asset<Texture2D> NKComet =>
            _nkComet ??= LoadTex($"{ThemePath}/Impact Effects/NK Comet");

        // ══════════════════════════════════════════════════════
        //  PARTICLES
        // ══════════════════════════════════════════════════════

        private static Asset<Texture2D> _nkLensFlare;
        /// <summary>Nachtmusik lens flare — starlight bloom particle.</summary>
        public static Asset<Texture2D> NKLensFlare =>
            _nkLensFlare ??= LoadTex($"{ThemePath}/Particles/NK Lens Flare");

        // ══════════════════════════════════════════════════════
        //  TRAILS AND RIBBONS
        // ══════════════════════════════════════════════════════

        private static Asset<Texture2D> _nkBasicTrail;
        /// <summary>Nachtmusik basic trail — indigo-to-silver trail strip.</summary>
        public static Asset<Texture2D> NKBasicTrail =>
            _nkBasicTrail ??= LoadTex($"{ThemePath}/Trails and Ribbons/NK Basic Trail");

        private static Asset<Texture2D> _nkHarmonicStandingWaveRibbon;
        /// <summary>Nachtmusik harmonic standing wave ribbon — flowing nocturnal ribbon.</summary>
        public static Asset<Texture2D> NKHarmonicRibbon =>
            _nkHarmonicStandingWaveRibbon ??= LoadTex($"{ThemePath}/Trails and Ribbons/NK Harmonic Standing Wave Ribbon");

        // ══════════════════════════════════════════════════════
        //  NOISE TEXTURES
        // ══════════════════════════════════════════════════════

        private static Asset<Texture2D> _nkConstellationNoise;
        /// <summary>Nachtmusik constellation fragment noise — scattered star cluster pattern.</summary>
        public static Asset<Texture2D> NKConstellationNoise =>
            _nkConstellationNoise ??= LoadTex($"{ThemePath}/Noise/NK Constellation Fragment Noise");

        private static Asset<Texture2D> _nkUniqueNoise;
        /// <summary>Nachtmusik unique theme noise — nocturnal distortion pattern.</summary>
        public static Asset<Texture2D> NKUniqueNoise =>
            _nkUniqueNoise ??= LoadTex($"{ThemePath}/Noise/NK Unique Theme Noise");

        // ══════════════════════════════════════════════════════
        //  COLOR GRADIENTS
        // ══════════════════════════════════════════════════════

        private static Asset<Texture2D> _nkGradient;
        /// <summary>Nachtmusik gradient LUT — deep indigo to starlight silver ramp.</summary>
        public static Asset<Texture2D> NKGradient =>
            _nkGradient ??= LoadTex($"{GradientPath}/NachtmusikGradientLUTandRAMP");

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
