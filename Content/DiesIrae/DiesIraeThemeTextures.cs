using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae
{
    /// <summary>
    /// Centralized texture registry for Dies Irae theme-specific VFX assets.
    /// Lazy-loads from Assets/VFX Asset Library/Theme Specific/Dies Irae/.
    /// All Dies Irae weapons should reference these instead of universal or made-up textures.
    /// </summary>
    internal static class DiesIraeThemeTextures
    {
        private const string ThemePath = "MagnumOpus/Assets/VFX Asset Library/Theme Specific/Dies Irae";
        private const string GradientPath = "MagnumOpus/Assets/VFX Asset Library/ColorGradients";

        // ══════════════════════════════════════════════════════
        //  BEAM TEXTURES
        // ══════════════════════════════════════════════════════

        private static Asset<Texture2D> _diEnergyMotionBeam;
        /// <summary>Dies Irae energy motion beam — wrathful flowing beam body.</summary>
        public static Asset<Texture2D> DIEnergyMotionBeam =>
            _diEnergyMotionBeam ??= LoadTex($"{ThemePath}/Beam Textures/DI Energy Motion Beam");

        private static Asset<Texture2D> _diEnergySurgeBeam;
        /// <summary>Dies Irae energy surge beam — apocalyptic beam burst.</summary>
        public static Asset<Texture2D> DIEnergySurgeBeam =>
            _diEnergySurgeBeam ??= LoadTex($"{ThemePath}/Beam Textures/DI Energy Surge Beam");

        private static Asset<Texture2D> _diBraidedHelixBeam;
        /// <summary>Dies Irae braided energy helix beam — chain-link intertwined beam.</summary>
        public static Asset<Texture2D> DIBraidedHelixBeam =>
            _diBraidedHelixBeam ??= LoadTex($"{ThemePath}/Beam Textures/DI Braided Energy Helix Beam (Chain-link)");

        // ══════════════════════════════════════════════════════
        //  IMPACT EFFECTS
        // ══════════════════════════════════════════════════════

        private static Asset<Texture2D> _diPowerEffectRing;
        /// <summary>Dies Irae power effect ring — crimson expanding ring.</summary>
        public static Asset<Texture2D> DIPowerEffectRing =>
            _diPowerEffectRing ??= LoadTex($"{ThemePath}/Impact Effects/DI Power Effect Ring");

        private static Asset<Texture2D> _diHarmonicImpact;
        /// <summary>Dies Irae harmonic resonance wave impact — wrathful ripple burst.</summary>
        public static Asset<Texture2D> DIHarmonicImpact =>
            _diHarmonicImpact ??= LoadTex($"{ThemePath}/Impact Effects/DI Harmonic Resonance Wave Impact");

        private static Asset<Texture2D> _diRadialSlashImpact;
        /// <summary>Dies Irae radial slash star impact — judgment slash burst.</summary>
        public static Asset<Texture2D> DIRadialSlashImpact =>
            _diRadialSlashImpact ??= LoadTex($"{ThemePath}/Impact Effects/DI Radial Slash Star Impact");

        // ══════════════════════════════════════════════════════
        //  PARTICLES
        // ══════════════════════════════════════════════════════

        private static Asset<Texture2D> _diAshFlake;
        /// <summary>Dies Irae ash flake — drifting ember ash particle.</summary>
        public static Asset<Texture2D> DIAshFlake =>
            _diAshFlake ??= LoadTex($"{ThemePath}/Particles/DI Ash Flake");

        private static Asset<Texture2D> _diJudgmentChainLink;
        /// <summary>Dies Irae judgment chain link — heavy chain fragment particle.</summary>
        public static Asset<Texture2D> DIJudgmentChainLink =>
            _diJudgmentChainLink ??= LoadTex($"{ThemePath}/Particles/DI Judgment Chain Link");

        // ══════════════════════════════════════════════════════
        //  PROJECTILES
        // ══════════════════════════════════════════════════════

        private static Asset<Texture2D> _diStarFlare;
        /// <summary>Dies Irae star flare — burning judgment projectile sprite.</summary>
        public static Asset<Texture2D> DIStarFlare =>
            _diStarFlare ??= LoadTex($"{ThemePath}/Projectiles/DI Star Flare");

        private static Asset<Texture2D> _diStarFlare2;
        /// <summary>Dies Irae star flare variant — alternate wrath projectile sprite.</summary>
        public static Asset<Texture2D> DIStarFlare2 =>
            _diStarFlare2 ??= LoadTex($"{ThemePath}/Projectiles/DI Star Flare 2");

        // ══════════════════════════════════════════════════════
        //  TRAILS AND RIBBONS
        // ══════════════════════════════════════════════════════

        private static Asset<Texture2D> _diRockyHellTrail;
        /// <summary>Dies Irae rocky hell trail — scorched infernal trail strip.</summary>
        public static Asset<Texture2D> DIRockyHellTrail =>
            _diRockyHellTrail ??= LoadTex($"{ThemePath}/Trails and Ribbons/DI Rocky Hell Trail");

        // ══════════════════════════════════════════════════════
        //  NOISE TEXTURES
        // ══════════════════════════════════════════════════════

        private static Asset<Texture2D> _diCrackedEarthNoise;
        /// <summary>Dies Irae unique cracked earth noise — fractured ground distortion pattern.</summary>
        public static Asset<Texture2D> DICrackedEarthNoise =>
            _diCrackedEarthNoise ??= LoadTex($"{ThemePath}/Noise/DI Unique Theme Noise \u2014 Cracked Earth Pattern");

        // ══════════════════════════════════════════════════════
        //  COLOR GRADIENTS
        // ══════════════════════════════════════════════════════

        private static Asset<Texture2D> _diGradient;
        /// <summary>Dies Irae gradient LUT — blood red to ember orange ramp.</summary>
        public static Asset<Texture2D> DIGradient =>
            _diGradient ??= LoadTex($"{GradientPath}/DiesIraeGradientLUTandRAMP");

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
