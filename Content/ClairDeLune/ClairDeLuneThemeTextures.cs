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

        private static Asset<Texture2D> _clBraidedEnergyHelixBeam;
        /// <summary>Clair de Lune braided energy helix beam — clockwork intertwined beam.</summary>
        public static Asset<Texture2D> CLBraidedHelixBeam =>
            _clBraidedEnergyHelixBeam ??= LoadTex($"{ThemePath}/Beam Textures/CL Braided Energy Helix Beam");

        private static Asset<Texture2D> _clEnergyMotionBeam;
        /// <summary>Clair de Lune energy motion beam — moonlit clockwork beam.</summary>
        public static Asset<Texture2D> CLEnergyMotionBeam =>
            _clEnergyMotionBeam ??= LoadTex($"{ThemePath}/Beam Textures/CL Energy Motion Beam");

        private static Asset<Texture2D> _clEnergySurgeBeam;
        /// <summary>Clair de Lune energy surge beam — temporal burst beam.</summary>
        public static Asset<Texture2D> CLEnergySurgeBeam =>
            _clEnergySurgeBeam ??= LoadTex($"{ThemePath}/Beam Textures/CL Energy Surge Beam");

        // ══════════════════════════════════════════════════════
        //  IMPACT EFFECTS
        // ══════════════════════════════════════════════════════

        private static Asset<Texture2D> _clPowerEffectRing;
        /// <summary>Clair de Lune power effect ring — clockwork expanding ring.</summary>
        public static Asset<Texture2D> CLPowerEffectRing =>
            _clPowerEffectRing ??= LoadTex($"{ThemePath}/Impact Effects/CL Power Effect Ring");

        private static Asset<Texture2D> _clRadialSlashStar;
        /// <summary>Clair de Lune radial slash star impact — temporal slash burst.</summary>
        public static Asset<Texture2D> CLRadialSlashStar =>
            _clRadialSlashStar ??= LoadTex($"{ThemePath}/Impact Effects/CL Radial Slash Star Impact");

        // ══════════════════════════════════════════════════════
        //  NOISE TEXTURES
        // ══════════════════════════════════════════════════════

        private static Asset<Texture2D> _clShatteredTimepieceNoise;
        /// <summary>Clair de Lune shattered timepiece dapple pattern — clockwork fracture noise.</summary>
        public static Asset<Texture2D> CLShatteredTimepieceNoise =>
            _clShatteredTimepieceNoise ??= LoadTex($"{ThemePath}/Noise/CL Unique Theme Noise \u2014 Shattered Timepiece Dapple Pattern");

        // ══════════════════════════════════════════════════════
        //  PARTICLES
        // ══════════════════════════════════════════════════════

        private static Asset<Texture2D> _clClockFaceShard;
        /// <summary>Clair de Lune clock face shard — broken clock fragment particle.</summary>
        public static Asset<Texture2D> CLClockFaceShard =>
            _clClockFaceShard ??= LoadTex($"{ThemePath}/Particles/CL Clock Face Shard");

        private static Asset<Texture2D> _clClockGearFragment;
        /// <summary>Clair de Lune clock gear fragment — mechanical gear debris particle.</summary>
        public static Asset<Texture2D> CLClockGearFragment =>
            _clClockGearFragment ??= LoadTex($"{ThemePath}/Particles/CL Clock Gear Fragment");

        // ══════════════════════════════════════════════════════
        //  PROJECTILES
        // ══════════════════════════════════════════════════════

        private static Asset<Texture2D> _clClockGearProj;
        /// <summary>Clair de Lune clock gear fragment projectile sprite.</summary>
        public static Asset<Texture2D> CLClockGearProj =>
            _clClockGearProj ??= LoadTex($"{ThemePath}/Projectiles/CL Clock Gear Fragment");

        private static Asset<Texture2D> _clRadialSlashStarProj;
        /// <summary>Clair de Lune radial slash star impact projectile sprite.</summary>
        public static Asset<Texture2D> CLRadialSlashStarProj =>
            _clRadialSlashStarProj ??= LoadTex($"{ThemePath}/Projectiles/CL Radial Slash Star Impact");

        // ══════════════════════════════════════════════════════
        //  TRAILS AND RIBBONS
        // ══════════════════════════════════════════════════════

        private static Asset<Texture2D> _clBasicTrail;
        /// <summary>Clair de Lune basic trail — moonlit clockwork trail strip.</summary>
        public static Asset<Texture2D> CLBasicTrail =>
            _clBasicTrail ??= LoadTex($"{ThemePath}/Trails and Ribbons/CL Basic Trail");

        // ══════════════════════════════════════════════════════
        //  COLOR GRADIENTS
        // ══════════════════════════════════════════════════════

        private static Asset<Texture2D> _clGradientLUT;
        /// <summary>Clair de Lune gradient LUT — night mist blue to pearl white.</summary>
        public static Asset<Texture2D> CLGradientLUT =>
            _clGradientLUT ??= LoadTex($"{GradientPath}/ClairDeLuneGradientLUTandRAMP");

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
