using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;

namespace MagnumOpus.Content.Eroica.Weapons.SakurasBlossom
{
    /// <summary>
    /// Centralized texture loading for Sakura's Blossom.
    /// Follows SLPCommonTextures pattern  Eevery dust, shader, and VFX class
    /// reads textures from HERE, not from hardcoded paths.
    ///
    /// Currently uses existing mod textures as placeholders.
    /// Replace with custom sakura-specific PNGs when art is ready.
    /// </summary>
    internal static class SBTextures
    {
        // ══════════════════════════════════════════════════════╁E
        //  PATH CONSTANTS
        // ══════════════════════════════════════════════════════╁E

        private static readonly string SLP_Orbs = "MagnumOpus/Assets/SandboxLastPrism/Orbs/";
        private static readonly string SLP_Trails = "MagnumOpus/Assets/SandboxLastPrism/Trails/";
        private static readonly string SLP_Pixel = "MagnumOpus/Assets/SandboxLastPrism/Pixel/";
        private static readonly string Particles = "MagnumOpus/Assets/Particles Asset Library/";

        // ══════════════════════════════════════════════════════╁E
        //  GLOW & ORB TEXTURES (soft circles, feathered spheres)
        // ══════════════════════════════════════════════════════╁E

        /// <summary>Soft feathered circle 128px  Ebloom orbs, impact flares.</summary>
        public static readonly Asset<Texture2D> BloomOrb =
            ModContent.Request<Texture2D>(SLP_Orbs + "feather_circle128PMA", AssetRequestMode.ImmediateLoad);

        /// <summary>Soft glow 64px  Emid-sized glow particles.</summary>
        public static readonly Asset<Texture2D> SoftGlow64 =
            ModContent.Request<Texture2D>(SLP_Orbs + "SoftGlow64", AssetRequestMode.ImmediateLoad);

        /// <summary>Soft glow small  Etiny glow particles, pollen motes.</summary>
        public static readonly Asset<Texture2D> SoftGlow =
            ModContent.Request<Texture2D>(SLP_Orbs + "SoftGlow", AssetRequestMode.ImmediateLoad);

        /// <summary>Circle with soft edges  Erings, halos.</summary>
        public static readonly Asset<Texture2D> CircleGlow =
            ModContent.Request<Texture2D>(SLP_Orbs + "circle_05", AssetRequestMode.ImmediateLoad);

        // ══════════════════════════════════════════════════════╁E
        //  FLARE & SPARK TEXTURES (sharp points, stars)
        // ══════════════════════════════════════════════════════╁E

        /// <summary>Pixel flare  Edirectional wind lines, speed streaks.</summary>
        public static readonly Asset<Texture2D> PixelFlare =
            ModContent.Request<Texture2D>(SLP_Pixel + "Flare", AssetRequestMode.ImmediateLoad);

        /// <summary>Sparkle particle  E4-point star sparkles.</summary>
        public static readonly Asset<Texture2D> Sparkle =
            ModContent.Request<Texture2D>(Particles + "SoftSparkle", AssetRequestMode.ImmediateLoad);

        // ══════════════════════════════════════════════════════╁E
        //  TRAIL TEXTURES (strip UVs, energy lines)
        // ══════════════════════════════════════════════════════╁E

        /// <summary>Thin glow line  Etrail body texture for vertex trails.</summary>
        public static readonly Asset<Texture2D> ThinGlowLine =
            ModContent.Request<Texture2D>(SLP_Trails + "ThinGlowLine", AssetRequestMode.ImmediateLoad);

        /// <summary>Energy texture  Enoise-based trail overdraw.</summary>
        public static readonly Asset<Texture2D> EnergyTex =
            ModContent.Request<Texture2D>(SLP_Trails + "EnergyTex", AssetRequestMode.ImmediateLoad);

        /// <summary>Spark texture  Etrail highlights.</summary>
        public static readonly Asset<Texture2D> SparkTex =
            ModContent.Request<Texture2D>(SLP_Trails + "spark_06", AssetRequestMode.ImmediateLoad);

        // ══════════════════════════════════════════════════════╁E
        //  PARTICLE TEXTURES (existing shared particles)
        // ══════════════════════════════════════════════════════╁E

        /// <summary>Glowing halo  Ering dust, bloom rings.</summary>
        public static readonly Asset<Texture2D> GlowingHalo =
            ModContent.Request<Texture2D>(Particles + "GlowingHalo2", AssetRequestMode.ImmediateLoad);

        /// <summary>Sword arc smear  Eused in swing overlays.</summary>
        public static readonly Asset<Texture2D> SwordArc =
            ModContent.Request<Texture2D>(Particles + "SwordArc2", AssetRequestMode.ImmediateLoad);
    }
}
