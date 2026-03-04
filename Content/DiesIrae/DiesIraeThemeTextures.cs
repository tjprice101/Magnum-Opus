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

        private static Asset<Texture2D> _diBraidedEnergyHelixBeam;
        /// <summary>Dies Irae braided energy helix beam (chain-link style) — judgment chain beam.</summary>
        public static Asset<Texture2D> DIBraidedHelixBeam =>
            _diBraidedEnergyHelixBeam ??= LoadTex($"{ThemePath}/Beam Textures/DI Braided Energy Helix Beam (Chain-link)");

        private static Asset<Texture2D> _diEnergyMotionBeam;
        /// <summary>Dies Irae energy motion beam — wrathful flowing beam.</summary>
        public static Asset<Texture2D> DIEnergyMotionBeam =>
            _diEnergyMotionBeam ??= LoadTex($"{ThemePath}/Beam Textures/DI Energy Motion Beam");

        private static Asset<Texture2D> _diEnergySurgeBeam;
        /// <summary>Dies Irae energy surge beam — hellfire surge burst.</summary>
        public static Asset<Texture2D> DIEnergySurgeBeam =>
            _diEnergySurgeBeam ??= LoadTex($"{ThemePath}/Beam Textures/DI Energy Surge Beam");

        // ══════════════════════════════════════════════════════
        //  IMPACT EFFECTS
        // ══════════════════════════════════════════════════════

        private static Asset<Texture2D> _diHarmonicImpact;
        /// <summary>Dies Irae harmonic resonance wave impact — shockwave of judgment.</summary>
        public static Asset<Texture2D> DIHarmonicImpact =>
            _diHarmonicImpact ??= LoadTex($"{ThemePath}/Impact Effects/DI Harmonic Resonance Wave Impact");

        private static Asset<Texture2D> _diPowerEffectRing;
        /// <summary>Dies Irae power effect ring — wrathful expanding ring.</summary>
        public static Asset<Texture2D> DIPowerEffectRing =>
            _diPowerEffectRing ??= LoadTex($"{ThemePath}/Impact Effects/DI Power Effect Ring");

        private static Asset<Texture2D> _diRadialSlashStar;
        /// <summary>Dies Irae radial slash star impact — hellfire slash burst.</summary>
        public static Asset<Texture2D> DIRadialSlashStar =>
            _diRadialSlashStar ??= LoadTex($"{ThemePath}/Impact Effects/DI Radial Slash Star Impact");

        // ══════════════════════════════════════════════════════
        //  NOISE TEXTURES
        // ══════════════════════════════════════════════════════

        private static Asset<Texture2D> _diCrackedEarthPattern;
        /// <summary>Dies Irae cracked earth pattern — unique judgment cracks noise.</summary>
        public static Asset<Texture2D> DICrackedEarthNoise =>
            _diCrackedEarthPattern ??= LoadTex($"{ThemePath}/Noise/DI Unique Theme Noise \u2014 Cracked Earth Pattern");

        // ══════════════════════════════════════════════════════
        //  PARTICLES
        // ══════════════════════════════════════════════════════

        private static Asset<Texture2D> _diAshFlake;
        /// <summary>Dies Irae ash flake — drifting judgment ash particle.</summary>
        public static Asset<Texture2D> DIAshFlake =>
            _diAshFlake ??= LoadTex($"{ThemePath}/Particles/DI Ash Flake");

        private static Asset<Texture2D> _diJudgmentChainLink;
        /// <summary>Dies Irae judgment chain link — chain link particle.</summary>
        public static Asset<Texture2D> DIJudgmentChainLink =>
            _diJudgmentChainLink ??= LoadTex($"{ThemePath}/Particles/DI Judgment Chain Link");

        // ══════════════════════════════════════════════════════
        //  PROJECTILES
        // ══════════════════════════════════════════════════════

        private static Asset<Texture2D> _diStarFlare;
        /// <summary>Dies Irae star flare — hellfire point flare.</summary>
        public static Asset<Texture2D> DIStarFlare =>
            _diStarFlare ??= LoadTex($"{ThemePath}/Projectiles/DI Star Flare");

        private static Asset<Texture2D> _diStarFlare2;
        /// <summary>Dies Irae star flare 2 — secondary hellfire burst flare.</summary>
        public static Asset<Texture2D> DIStarFlare2 =>
            _diStarFlare2 ??= LoadTex($"{ThemePath}/Projectiles/DI Star Flare 2");

        // ══════════════════════════════════════════════════════
        //  TRAILS AND RIBBONS
        // ══════════════════════════════════════════════════════

        private static Asset<Texture2D> _diRockyHellTrail;
        /// <summary>Dies Irae rocky hell trail — infernal cracked earth trail strip.</summary>
        public static Asset<Texture2D> DIRockyHellTrail =>
            _diRockyHellTrail ??= LoadTex($"{ThemePath}/Trails and Ribbons/DI Rocky Hell Trail");

        // ══════════════════════════════════════════════════════
        //  COLOR GRADIENTS
        // ══════════════════════════════════════════════════════

        private static Asset<Texture2D> _diGradientLUT;
        /// <summary>Dies Irae gradient LUT — blood red to dark crimson to ember orange.</summary>
        public static Asset<Texture2D> DIGradientLUT =>
            _diGradientLUT ??= LoadTex($"{GradientPath}/DiesIraeGradientLUTandRAMP");

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
