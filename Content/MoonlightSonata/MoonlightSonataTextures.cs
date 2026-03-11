using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;

namespace MagnumOpus.Content.MoonlightSonata
{
    /// <summary>
    /// Centralized texture registry for all Moonlight Sonata VFX assets.
    /// Follows the SLPCommonTextures pattern  Estatic readonly Asset fields
    /// loaded at startup via ImmediateLoad mode.
    ///
    /// Shared textures (Flare, Gradients, Orbs, Pixel) are used across all
    /// five Moonlight Sonata weapons. Weapon-specific textures live under
    /// their weapon's asset folder.
    /// </summary>
    class MoonlightSonataTextures
    {
        #region SharedAssets

        // Shared texture paths  Eremapped to existing VFX Asset Library + SandboxLastPrism assets
        private static readonly string SLPFlare = "MagnumOpus/Assets/SandboxLastPrism/Flare/";
        private static readonly string SLPOrbs = "MagnumOpus/Assets/SandboxLastPrism/Orbs/";
        private static readonly string SLPPixel = "MagnumOpus/Assets/SandboxLastPrism/Pixel/";
        private static readonly string SLPTrails = "MagnumOpus/Assets/SandboxLastPrism/Trails/";
        private static readonly string SLPTrailsClear = "MagnumOpus/Assets/SandboxLastPrism/Trails/Clear/";
        private static readonly string VFXGradients = "MagnumOpus/Assets/VFX Asset Library/ColorGradients/";
        private static readonly string VFXMasks = "MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/";
        private static readonly string VFXGlow = "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/";
        private static readonly string ParticleStars = "MagnumOpus/Assets/Particles Asset Library/Stars/";

        /// <summary>Multi-pointed crystalline star burst  Eimpact flares, crescendo bursts, crit detonations.
        /// Remapped: flare_16 provides a clean multi-pointed burst shape ideal for impact VFX.</summary>
        public static readonly Asset<Texture2D> ConstellationBurstFlare =
            ModContent.Request<Texture2D>(SLPFlare + "flare_16", AssetRequestMode.ImmediateLoad);

        /// <summary>Ice blue ↁElavender ↁEviolet horizontal gradient  Etrail color grading, beam gradients.
        /// Remapped: MoonlightSonataGradientLUTandRAMP is the canonical Moonlight theme gradient ramp.</summary>
        public static readonly Asset<Texture2D> ResonanceTrailGradient =
            ModContent.Request<Texture2D>(VFXGradients + "MoonlightSonataGradientLUTandRAMP", AssetRequestMode.ImmediateLoad);

        /// <summary>Soft white circular gradient  Ebloom masking, metaball soft-edge, beam endpoints.
        /// Remapped: SoftCircle provides clean feathered circle with smooth falloff.</summary>
        public static readonly Asset<Texture2D> SoftCircle =
            ModContent.Request<Texture2D>(VFXMasks + "SoftCircle", AssetRequestMode.ImmediateLoad);

        /// <summary>Glowing ring with dark center  Eaura rings, shockwave blooms, orbital accents.
        /// Remapped: circle_05 has ring-like quality with hollow center for aura effects.</summary>
        public static readonly Asset<Texture2D> BloomOrb =
            ModContent.Request<Texture2D>(SLPOrbs + "circle_05", AssetRequestMode.ImmediateLoad);

        /// <summary>Clean 4-pointed star sparkle  Eambient constellation stars, impact sparks, node accents.
        /// Remapped: 4PointedStarHard is a crisp 4-pointed star matching the original intent.</summary>
        public static readonly Asset<Texture2D> Star4Point =
            ModContent.Request<Texture2D>(ParticleStars + "4PointedStarHard", AssetRequestMode.ImmediateLoad);

        #endregion

        #region EternalMoon

        // EternalMoon textures  Eremapped to shared VFX library assets.
        // The shader-driven trail effects use gradient LUTs, energy overlays,
        // and glow masks from existing assets  Eachieving the tidal moonlight
        // aesthetic through UV scrolling and palette-driven color grading.

        /// <summary>Tidal color gradient for shader trail sampling  Edeep purple to ice blue to moon white.
        /// Remapped: Moonlight Sonata theme gradient LUT  Eshader samples across this ramp for tidal color flow.</summary>
        public static readonly Asset<Texture2D> TidalGradient =
            ModContent.Request<Texture2D>(VFXGradients + "MoonlightSonataGradientLUTandRAMP", AssetRequestMode.ImmediateLoad);

        /// <summary>Tidal trail body texture  Ethin glow line for shader-driven trail rendering.
        /// Remapped: ThinGlowLine is a clean thin trail body ideal for UV-scrolled shader trails.</summary>
        public static readonly Asset<Texture2D> TidalTrailBody =
            ModContent.Request<Texture2D>(SLPTrails + "ThinGlowLine", AssetRequestMode.ImmediateLoad);

        /// <summary>Tidal energy texture  Eflowing energy pattern for trail overlay.
        /// Remapped: EnergyTex provides flowing energy patterns for secondary trail layer UV scrolling.</summary>
        public static readonly Asset<Texture2D> TidalEnergy =
            ModContent.Request<Texture2D>(SLPTrails + "EnergyTex", AssetRequestMode.ImmediateLoad);

        /// <summary>Tidal glow clear  Eadditive-only trail glow for bloom stacking.
        /// Remapped: ThinLineGlowClear is transparent-background glow perfect for additive bloom stacking.</summary>
        public static readonly Asset<Texture2D> TidalGlow =
            ModContent.Request<Texture2D>(SLPTrailsClear + "ThinLineGlowClear", AssetRequestMode.ImmediateLoad);

        /// <summary>Tidal bloom orb  Esoft glow for crescent bloom and impact effects.
        /// Remapped: SoftGlow is a clean radial bloom orb for impact glow and bloom sandwich layers.</summary>
        public static readonly Asset<Texture2D> TidalBloom =
            ModContent.Request<Texture2D>(SLPOrbs + "SoftGlow", AssetRequestMode.ImmediateLoad);

        /// <summary>Crescent mask  Efeathered circle for aura rendering.
        /// Remapped: feather_circle128PMA is a feathered soft-edge circle in premultiplied alpha  E
        /// perfect for crescent masking when combined with shader-driven UV offset.</summary>
        public static readonly Asset<Texture2D> CrescentMask =
            ModContent.Request<Texture2D>(SLPOrbs + "feather_circle128PMA", AssetRequestMode.ImmediateLoad);

        /// <summary>Crescent flare  Eburst flare for impacts and phase transitions.
        /// Remapped: flare_16 multi-pointed burst works as a crescent flare accent.</summary>
        public static readonly Asset<Texture2D> CrescentFlare =
            ModContent.Request<Texture2D>(SLPFlare + "flare_16", AssetRequestMode.ImmediateLoad);

        /// <summary>Tidal lens flare  Eelongated lens effect for blade tip.
        /// Remapped: Simple Lens Flare_11 provides an elongated anamorphic lens streak.</summary>
        public static readonly Asset<Texture2D> TidalLensFlare =
            ModContent.Request<Texture2D>(SLPFlare + "Simple Lens Flare_11", AssetRequestMode.ImmediateLoad);

        /// <summary>Tidal spark  Esmall bright pixel spark for finest detail particles.
        /// Remapped: PartiGlow is a tiny bright pixel perfect for fine sparks and detail particles.</summary>
        public static readonly Asset<Texture2D> TidalSpark =
            ModContent.Request<Texture2D>(SLPPixel + "PartiGlow", AssetRequestMode.ImmediateLoad);

        #endregion

        #region IncisorOfMoonlight

        // Incisor textures  Eremapped to shared assets.
        // The precision blade aesthetic uses sharp flares and thin trails.

        /// <summary>Crystalline tuning fork with violet/ice glow  EIncisor weapon identity flare.
        /// Remapped: flare_16 provides sharp crystalline burst for precision blade identity.</summary>
        public static readonly Asset<Texture2D> TuningForkFlare =
            ModContent.Request<Texture2D>(SLPFlare + "flare_16", AssetRequestMode.ImmediateLoad);

        /// <summary>Horizontal stellar trail with scattered star particles  EIncisor trail texture.
        /// Remapped: ThinGlowLine gives a clean precision trail body for the Incisor's laser-sharp aesthetic.</summary>
        public static readonly Asset<Texture2D> IncisorTrail =
            ModContent.Request<Texture2D>(SLPTrails + "ThinGlowLine", AssetRequestMode.ImmediateLoad);

        #endregion
    }
}
