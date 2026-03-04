using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;

namespace MagnumOpus.Content.EnigmaVariations
{
    /// <summary>
    /// Centralized texture registry for Enigma Variations theme-specific VFX assets.
    /// Lazy-loads from Assets/VFX Asset Library/Theme Specific/Enigma/.
    /// All Enigma weapons should reference these instead of universal or made-up textures.
    /// </summary>
    internal static class EnigmaThemeTextures
    {
        private const string ThemePath = "MagnumOpus/Assets/VFX Asset Library/Theme Specific/Enigma";
        private const string GradientPath = "MagnumOpus/Assets/VFX Asset Library/ColorGradients";

        // ══════════════════════════════════════════════════════
        //  BEAM TEXTURES
        // ══════════════════════════════════════════════════════

        private static Asset<Texture2D> _enEnergyMotionBeam;
        /// <summary>Enigma energy motion beam — mysterious flowing energy.</summary>
        public static Asset<Texture2D> ENEnergyMotionBeam =>
            _enEnergyMotionBeam ??= LoadTex($"{ThemePath}/Beam Textures/EN Energy Motion Beam");

        private static Asset<Texture2D> _enEnergySurgeBeam;
        /// <summary>Enigma energy surge beam — void-charged beam burst.</summary>
        public static Asset<Texture2D> ENEnergySurgeBeam =>
            _enEnergySurgeBeam ??= LoadTex($"{ThemePath}/Beam Textures/EN Energy Surge Beam");

        // ══════════════════════════════════════════════════════
        //  IMPACT EFFECTS
        // ══════════════════════════════════════════════════════

        private static Asset<Texture2D> _enHarmonicImpact;
        /// <summary>Enigma harmonic resonance wave impact — arcane ripple burst.</summary>
        public static Asset<Texture2D> ENHarmonicImpact =>
            _enHarmonicImpact ??= LoadTex($"{ThemePath}/Impact Effects/EN Harmonic Resonance Wave Impact");

        private static Asset<Texture2D> _enPowerEffectRing;
        /// <summary>Enigma power effect ring — eerie expanding void ring.</summary>
        public static Asset<Texture2D> ENPowerEffectRing =>
            _enPowerEffectRing ??= LoadTex($"{ThemePath}/Impact Effects/EN Power Effect Ring");

        private static Asset<Texture2D> _enStarFlare;
        /// <summary>Enigma star flare — mysterious point flare.</summary>
        public static Asset<Texture2D> ENStarFlare =>
            _enStarFlare ??= LoadTex($"{ThemePath}/Impact Effects/EN Star Flare");

        // ══════════════════════════════════════════════════════
        //  PARTICLES
        // ══════════════════════════════════════════════════════

        private static Asset<Texture2D> _enEnigmaEye;
        /// <summary>Enigma eye particle — the watching, unknowable eye.</summary>
        public static Asset<Texture2D> ENEnigmaEye =>
            _enEnigmaEye ??= LoadTex($"{ThemePath}/Particles/EN Enigma Eye");

        // ══════════════════════════════════════════════════════
        //  TRAILS AND RIBBONS
        // ══════════════════════════════════════════════════════

        private static Asset<Texture2D> _enBasicTrail;
        /// <summary>Enigma basic trail — void-touched trail strip.</summary>
        public static Asset<Texture2D> ENBasicTrail =>
            _enBasicTrail ??= LoadTex($"{ThemePath}/Trails and Ribbons/EN Basic Trail");

        private static Asset<Texture2D> _enHarmonicRibbon;
        /// <summary>Enigma harmonic standing wave ribbon — arcane flowing ribbon.</summary>
        public static Asset<Texture2D> ENHarmonicRibbon =>
            _enHarmonicRibbon ??= LoadTex($"{ThemePath}/Trails and Ribbons/EN Harmonic Standing Wave Ribbon");

        // ══════════════════════════════════════════════════════
        //  COLOR GRADIENTS
        // ══════════════════════════════════════════════════════

        private static Asset<Texture2D> _enGradientLUT;
        /// <summary>Enigma gradient LUT — void black to eerie green to deep purple.</summary>
        public static Asset<Texture2D> ENGradientLUT =>
            _enGradientLUT ??= LoadTex($"{GradientPath}/EnigmaGradientLUTandRAMP");

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
