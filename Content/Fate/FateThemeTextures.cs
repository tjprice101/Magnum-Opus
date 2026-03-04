using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Fate
{
    /// <summary>
    /// Centralized texture registry for Fate theme-specific VFX assets.
    /// Lazy-loads from Assets/VFX Asset Library/Theme Specific/Fate/.
    /// All Fate weapons should reference these instead of universal or made-up textures.
    /// </summary>
    internal static class FateThemeTextures
    {
        private const string ThemePath = "MagnumOpus/Assets/VFX Asset Library/Theme Specific/Fate";
        private const string GradientPath = "MagnumOpus/Assets/VFX Asset Library/ColorGradients";

        // ══════════════════════════════════════════════════════
        //  BEAM TEXTURES
        // ══════════════════════════════════════════════════════

        private static Asset<Texture2D> _faEnergySurgeBeam;
        /// <summary>Fate energy surge beam — celestial cosmic beam burst.</summary>
        public static Asset<Texture2D> FAEnergySurgeBeam =>
            _faEnergySurgeBeam ??= LoadTex($"{ThemePath}/Beam Textures/FA Energy Surge Beam");

        private static Asset<Texture2D> _faPowerEffectRing;
        /// <summary>Fate power effect ring — cosmic expanding ring (beam category).</summary>
        public static Asset<Texture2D> FAPowerEffectRing =>
            _faPowerEffectRing ??= LoadTex($"{ThemePath}/Beam Textures/FA Power Effect Ring");

        // ══════════════════════════════════════════════════════
        //  IMPACT EFFECTS
        // ══════════════════════════════════════════════════════

        private static Asset<Texture2D> _faHarmonicImpact;
        /// <summary>Fate harmonic resonance wave impact — celestial shockwave burst.</summary>
        public static Asset<Texture2D> FAHarmonicImpact =>
            _faHarmonicImpact ??= LoadTex($"{ThemePath}/Impact Effects/FA Harmonic Resonance Wave Impact");

        // ══════════════════════════════════════════════════════
        //  PARTICLES
        // ══════════════════════════════════════════════════════

        private static Asset<Texture2D> _faCelestialGlyph;
        /// <summary>Fate celestial glyph — ancient cosmic symbol particle.</summary>
        public static Asset<Texture2D> FACelestialGlyph =>
            _faCelestialGlyph ??= LoadTex($"{ThemePath}/Particles/FA Celestial Glyph");

        private static Asset<Texture2D> _faSupernovaCore;
        /// <summary>Fate supernova core — star-death explosion core particle.</summary>
        public static Asset<Texture2D> FASupernovaCore =>
            _faSupernovaCore ??= LoadTex($"{ThemePath}/Particles/FA Supernova Core");

        // ══════════════════════════════════════════════════════
        //  TRAILS AND RIBBONS
        // ══════════════════════════════════════════════════════

        private static Asset<Texture2D> _faBasicTrail;
        /// <summary>Fate basic trail — cosmic destiny trail strip.</summary>
        public static Asset<Texture2D> FABasicTrail =>
            _faBasicTrail ??= LoadTex($"{ThemePath}/Trails and Ribbons/FA Basic Trail");

        // ══════════════════════════════════════════════════════
        //  COLOR GRADIENTS
        // ══════════════════════════════════════════════════════

        private static Asset<Texture2D> _faGradientLUT;
        /// <summary>Fate gradient LUT — black void to dark pink to crimson to celestial white.</summary>
        public static Asset<Texture2D> FAGradientLUT =>
            _faGradientLUT ??= LoadTex($"{GradientPath}/FateGradientLUTandRAMP");

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
