using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune
{
    /// <summary>
    /// Centralized texture registry for Clair de Lune theme-specific VFX assets.
    /// Lazy-loads from Assets/VFX Asset Library/Theme Specific/Clair de Lune/.
    /// All Clair de Lune weapons should reference these instead of universal or made-up textures.
    /// </summary>
    internal static class ClairDeLuneThemeTextures
    {
        private const string ThemePath = "MagnumOpus/Assets/VFX Asset Library/Theme Specific/Clair de Lune";
        private const string GradientPath = "MagnumOpus/Assets/VFX Asset Library/ColorGradients";

        // ══════════════════════════════════════════════════════
        //  BEAM TEXTURES
        // ══════════════════════════════════════════════════════

        private static Asset<Texture2D> _clEnergyMotionBeam;
        /// <summary>Clair de Lune energy motion beam — moonlit luminous beam body.</summary>
        public static Asset<Texture2D> CLEnergyMotionBeam =>
            _clEnergyMotionBeam ??= LoadTex($"{ThemePath}/Beam Textures/CL Energy Motion Beam");

        // ══════════════════════════════════════════════════════
        //  IMPACT EFFECTS
        // ══════════════════════════════════════════════════════

        private static Asset<Texture2D> _clPowerEffectRing;
        /// <summary>Clair de Lune power effect ring — soft pearlescent expanding ring.</summary>
        public static Asset<Texture2D> CLPowerEffectRing =>
            _clPowerEffectRing ??= LoadTex($"{ThemePath}/Impact Effects/CL Power Effect Ring");

        private static Asset<Texture2D> _clRadialSlashImpact;
        /// <summary>Clair de Lune radial slash star impact — clockwork slash burst.</summary>
        public static Asset<Texture2D> CLRadialSlashImpact =>
            _clRadialSlashImpact ??= LoadTex($"{ThemePath}/Impact Effects/CL Radial Slash Star Impact");

        // ══════════════════════════════════════════════════════
        //  PARTICLES
        // ══════════════════════════════════════════════════════

        private static Asset<Texture2D> _clClockGearFragment;
        /// <summary>Clair de Lune clock gear fragment — mechanical gear shard particle.</summary>
        public static Asset<Texture2D> CLClockGearFragment =>
            _clClockGearFragment ??= LoadTex($"{ThemePath}/Particles/CL Clock Gear Fragment");

        // ══════════════════════════════════════════════════════
        //  TRAILS AND RIBBONS
        // ══════════════════════════════════════════════════════

        private static Asset<Texture2D> _clBasicTrail;
        /// <summary>Clair de Lune basic trail — soft blue luminescent trail strip.</summary>
        public static Asset<Texture2D> CLBasicTrail =>
            _clBasicTrail ??= LoadTex($"{ThemePath}/Trails and Ribbons/CL Basic Trail");

        // ══════════════════════════════════════════════════════
        //  NOISE TEXTURES
        // ══════════════════════════════════════════════════════

        private static Asset<Texture2D> _clTimepieceNoise;
        /// <summary>Clair de Lune unique shattered timepiece noise — fractured clock dapple distortion pattern.</summary>
        public static Asset<Texture2D> CLTimepieceNoise =>
            _clTimepieceNoise ??= LoadTex($"{ThemePath}/Noise/CL Unique Theme Noise \u2014 Shattered Timepiece Dapple Pattern");

        // ══════════════════════════════════════════════════════
        //  COLOR GRADIENTS
        // ══════════════════════════════════════════════════════

        private static Asset<Texture2D> _clGradient;
        /// <summary>Clair de Lune gradient LUT — night mist blue to pearl white ramp.</summary>
        public static Asset<Texture2D> CLGradient =>
            _clGradient ??= LoadTex($"{GradientPath}/ClairDeLuneGradientLUTandRAMP");

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
