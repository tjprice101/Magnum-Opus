using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;

namespace MagnumOpus.Content.Eroica.Weapons.SakurasBlossom
{
    /// <summary>
    /// Centralized texture loading for Sakura's Blossom.
    /// Follows SLPCommonTextures pattern — every dust, shader, and VFX class
    /// reads textures from HERE, not from hardcoded paths.
    ///
    /// Currently uses existing mod textures as placeholders.
    /// Replace with custom sakura-specific PNGs when art is ready.
    /// </summary>
    internal static class SBTextures
    {
        // ═══════════════════════════════════════════════════════
        //  PATH CONSTANTS
        // ═══════════════════════════════════════════════════════

        private static readonly string SLP_Orbs = "MagnumOpus/Assets/SandboxLastPrism/Orbs/";
        private static readonly string SLP_Trails = "MagnumOpus/Assets/SandboxLastPrism/Trails/";
        private static readonly string SLP_Pixel = "MagnumOpus/Assets/SandboxLastPrism/Pixel/";
        private static readonly string Particles = "MagnumOpus/Assets/Particles/";

        // ═══════════════════════════════════════════════════════
        //  GLOW & ORB TEXTURES (soft circles, feathered spheres)
        // ═══════════════════════════════════════════════════════

        /// <summary>Soft feathered circle 128px — bloom orbs, impact flares.</summary>
        public static readonly Asset<Texture2D> BloomOrb =
            ModContent.Request<Texture2D>(SLP_Orbs + "feather_circle128PMA", AssetRequestMode.ImmediateLoad);

        /// <summary>Soft glow 64px — mid-sized glow particles.</summary>
        public static readonly Asset<Texture2D> SoftGlow64 =
            ModContent.Request<Texture2D>(SLP_Orbs + "SoftGlow64", AssetRequestMode.ImmediateLoad);

        /// <summary>Soft glow small — tiny glow particles, pollen motes.</summary>
        public static readonly Asset<Texture2D> SoftGlow =
            ModContent.Request<Texture2D>(SLP_Orbs + "SoftGlow", AssetRequestMode.ImmediateLoad);

        /// <summary>Circle with soft edges — rings, halos.</summary>
        public static readonly Asset<Texture2D> CircleGlow =
            ModContent.Request<Texture2D>(SLP_Orbs + "circle_05", AssetRequestMode.ImmediateLoad);

        // ═══════════════════════════════════════════════════════
        //  FLARE & SPARK TEXTURES (sharp points, stars)
        // ═══════════════════════════════════════════════════════

        /// <summary>Pixel flare — directional wind lines, speed streaks.</summary>
        public static readonly Asset<Texture2D> PixelFlare =
            ModContent.Request<Texture2D>(SLP_Pixel + "Flare", AssetRequestMode.ImmediateLoad);

        /// <summary>Sparkle particle — 4-point star sparkles.</summary>
        public static readonly Asset<Texture2D> Sparkle =
            ModContent.Request<Texture2D>(Particles + "SoftSparkle", AssetRequestMode.ImmediateLoad);

        // ═══════════════════════════════════════════════════════
        //  TRAIL TEXTURES (strip UVs, energy lines)
        // ═══════════════════════════════════════════════════════

        /// <summary>Thin glow line — trail body texture for vertex trails.</summary>
        public static readonly Asset<Texture2D> ThinGlowLine =
            ModContent.Request<Texture2D>(SLP_Trails + "ThinGlowLine", AssetRequestMode.ImmediateLoad);

        /// <summary>Energy texture — noise-based trail overdraw.</summary>
        public static readonly Asset<Texture2D> EnergyTex =
            ModContent.Request<Texture2D>(SLP_Trails + "EnergyTex", AssetRequestMode.ImmediateLoad);

        /// <summary>Spark texture — trail highlights.</summary>
        public static readonly Asset<Texture2D> SparkTex =
            ModContent.Request<Texture2D>(SLP_Trails + "spark_06", AssetRequestMode.ImmediateLoad);

        // ═══════════════════════════════════════════════════════
        //  PARTICLE TEXTURES (existing shared particles)
        // ═══════════════════════════════════════════════════════

        /// <summary>Glowing halo — ring dust, bloom rings.</summary>
        public static readonly Asset<Texture2D> GlowingHalo =
            ModContent.Request<Texture2D>(Particles + "GlowingHalo2", AssetRequestMode.ImmediateLoad);

        /// <summary>Sword arc smear — used in swing overlays.</summary>
        public static readonly Asset<Texture2D> SwordArc =
            ModContent.Request<Texture2D>(Particles + "SwordArc2", AssetRequestMode.ImmediateLoad);
    }
}
