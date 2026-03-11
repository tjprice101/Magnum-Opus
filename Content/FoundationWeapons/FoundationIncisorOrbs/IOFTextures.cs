using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;

namespace MagnumOpus.Content.FoundationWeapons.FoundationIncisorOrbs
{
    /// <summary>
    /// Self-contained texture registry for FoundationIncisorOrbs.
    /// All texture paths mirror exactly what LunarBeamProj uses.
    /// Sourced from the VFX Asset Library.
    /// </summary>
    internal static class IOFTextures
    {
        // ---- PATHS (identical to LunarBeamProj.LoadBeamTextures()) ----
        private static readonly string ThemeBeams = "MagnumOpus/Assets/VFX Asset Library/Theme Specific/Moonlight Sonata/Beam Textures/";
        private static readonly string Beams = "MagnumOpus/Assets/VFX Asset Library/BeamTextures/";
        private static readonly string Trails = "MagnumOpus/Assets/VFX Asset Library/TrailsAndRibbons/";
        private static readonly string Bloom = "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/";
        private static readonly string Noise = "MagnumOpus/Assets/VFX Asset Library/NoiseTextures/";
        private static readonly string Gradients = "MagnumOpus/Assets/VFX Asset Library/ColorGradients/";

        // ---- BEAM TRAIL TEXTURES (used by InfernalBeamBodyShader) ----

        /// <summary>Trail alpha mask — BasicTrail strip texture for UV alpha masking.</summary>
        public static readonly Asset<Texture2D> BasicTrail =
            ModContent.Request<Texture2D>(Trails + "BasicTrail", AssetRequestMode.ImmediateLoad);

        /// <summary>Gradient LUT — Moonlight Sonata color ramp for beam coloring.</summary>
        public static readonly Asset<Texture2D> MoonlightGradient =
            ModContent.Request<Texture2D>(Gradients + "MoonlightSonataGradientLUTandRAMP", AssetRequestMode.ImmediateLoad);

        /// <summary>Main body texture — SoundWaveBeam for the beam body layer.</summary>
        public static readonly Asset<Texture2D> SoundWaveBeam =
            ModContent.Request<Texture2D>(Beams + "SoundWaveBeam", AssetRequestMode.ImmediateLoad);

        /// <summary>Detail texture 1 — MS Energy Motion Beam for scrolling detail.</summary>
        public static readonly Asset<Texture2D> EnergyMotionBeam =
            ModContent.Request<Texture2D>(ThemeBeams + "MS Energy Motion Beam", AssetRequestMode.ImmediateLoad);

        /// <summary>Detail texture 2 — MS Energy Surge Beam for counter-scrolling detail.</summary>
        public static readonly Asset<Texture2D> EnergySurgeBeam =
            ModContent.Request<Texture2D>(ThemeBeams + "MS Energy Surge Beam", AssetRequestMode.ImmediateLoad);

        /// <summary>Noise texture — TileableFBMNoise for UV distortion in the shader.</summary>
        public static readonly Asset<Texture2D> FBMNoise =
            ModContent.Request<Texture2D>(Noise + "TileableFBMNoise", AssetRequestMode.ImmediateLoad);

        // ---- BLOOM TEXTURES (used by multi-layer bloom head) ----

        /// <summary>Soft glow — wide, gentle radial falloff for the outer glow layer.</summary>
        public static readonly Asset<Texture2D> SoftGlow =
            ModContent.Request<Texture2D>(Bloom + "SoftGlow", AssetRequestMode.ImmediateLoad);

        /// <summary>Point bloom — tighter, brighter point glow for mid + core layers.</summary>
        public static readonly Asset<Texture2D> PointBloom =
            ModContent.Request<Texture2D>(Bloom + "PointBloom", AssetRequestMode.ImmediateLoad);
    }
}
