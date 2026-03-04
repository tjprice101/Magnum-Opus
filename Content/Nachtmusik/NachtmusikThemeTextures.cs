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
        /// <summary>Nachtmusik energy motion beam — stellar flowing beam.</summary>
        public static Asset<Texture2D> NKEnergyMotionBeam =>
            _nkEnergyMotionBeam ??= LoadTex($"{ThemePath}/Beam Textures/NK Energy Motion Beam");

        private static Asset<Texture2D> _nkEnergySurgeBeam;
        /// <summary>Nachtmusik energy surge beam — cosmic star-charged beam burst.</summary>
        public static Asset<Texture2D> NKEnergySurgeBeam =>
            _nkEnergySurgeBeam ??= LoadTex($"{ThemePath}/Beam Textures/NK Energy Surge Beam");

        // ══════════════════════════════════════════════════════
        //  IMPACT EFFECTS
        // ══════════════════════════════════════════════════════

        private static Asset<Texture2D> _nkComet;
        /// <summary>Nachtmusik comet — stellar impact comet burst.</summary>
        public static Asset<Texture2D> NKComet =>
            _nkComet ??= LoadTex($"{ThemePath}/Impact Effects/NK Comet");

        private static Asset<Texture2D> _nkRadialSlashStar;
        /// <summary>Nachtmusik radial slash star impact — stellar slash burst.</summary>
        public static Asset<Texture2D> NKRadialSlashStar =>
            _nkRadialSlashStar ??= LoadTex($"{ThemePath}/Impact Effects/NK Radial Slash Star Impact");

        // ══════════════════════════════════════════════════════
        //  PARTICLES
        // ══════════════════════════════════════════════════════

        private static Asset<Texture2D> _nkLensFlare;
        /// <summary>Nachtmusik lens flare — starlight point flare.</summary>
        public static Asset<Texture2D> NKLensFlare =>
            _nkLensFlare ??= LoadTex($"{ThemePath}/Particles/NK Lens Flare");

        private static Asset<Texture2D> _nkPowerEffectRing;
        /// <summary>Nachtmusik power effect ring — cosmic ring particle.</summary>
        public static Asset<Texture2D> NKPowerEffectRing =>
            _nkPowerEffectRing ??= LoadTex($"{ThemePath}/Particles/NK Power Effect Ring");

        // ══════════════════════════════════════════════════════
        //  NOISE TEXTURES
        // ══════════════════════════════════════════════════════

        private static Asset<Texture2D> _nkConstellationFragmentNoise;
        /// <summary>Nachtmusik constellation fragment noise — stellar shatter pattern.</summary>
        public static Asset<Texture2D> NKConstellationNoise =>
            _nkConstellationFragmentNoise ??= LoadTex($"{ThemePath}/Noise/NK Constellation Fragment Noise");

        private static Asset<Texture2D> _nkUniqueNoise;
        /// <summary>Nachtmusik unique theme noise — nocturnal star field pattern.</summary>
        public static Asset<Texture2D> NKUniqueNoise =>
            _nkUniqueNoise ??= LoadTex($"{ThemePath}/Noise/NK Unique Theme Noise");

        // ══════════════════════════════════════════════════════
        //  TRAILS AND RIBBONS
        // ══════════════════════════════════════════════════════

        private static Asset<Texture2D> _nkBasicTrail;
        /// <summary>Nachtmusik basic trail — cosmic indigo trail strip.</summary>
        public static Asset<Texture2D> NKBasicTrail =>
            _nkBasicTrail ??= LoadTex($"{ThemePath}/Trails and Ribbons/NK Basic Trail");

        private static Asset<Texture2D> _nkHarmonicRibbon;
        /// <summary>Nachtmusik harmonic standing wave ribbon — stellar flowing ribbon.</summary>
        public static Asset<Texture2D> NKHarmonicRibbon =>
            _nkHarmonicRibbon ??= LoadTex($"{ThemePath}/Trails and Ribbons/NK Harmonic Standing Wave Ribbon");

        // ══════════════════════════════════════════════════════
        //  COLOR GRADIENTS
        // ══════════════════════════════════════════════════════

        private static Asset<Texture2D> _nkGradientLUT;
        /// <summary>Nachtmusik gradient LUT — deep indigo to starlight silver to cosmic blue.</summary>
        public static Asset<Texture2D> NKGradientLUT =>
            _nkGradientLUT ??= LoadTex($"{GradientPath}/NachtmusikGradientLUTandRAMP");

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
