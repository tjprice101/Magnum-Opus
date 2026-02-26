using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;

namespace MagnumOpus.Content.MoonlightSonata
{
    /// <summary>
    /// Centralized texture registry for all Moonlight Sonata VFX assets.
    /// Follows the SLPCommonTextures pattern — static readonly Asset fields
    /// loaded at startup via ImmediateLoad mode.
    ///
    /// Shared textures (Flare, Gradients, Orbs, Pixel) are used across all
    /// five Moonlight Sonata weapons. Weapon-specific textures live under
    /// their weapon's asset folder.
    /// </summary>
    class MoonlightSonataTextures
    {
        #region SharedAssets

        private static readonly string SharedFlare = "MagnumOpus/Assets/MoonlightSonata/Shared/Flare/";
        private static readonly string SharedGradients = "MagnumOpus/Assets/MoonlightSonata/Shared/Gradients/";
        private static readonly string SharedOrbs = "MagnumOpus/Assets/MoonlightSonata/Shared/Orbs/";
        private static readonly string SharedPixel = "MagnumOpus/Assets/MoonlightSonata/Shared/Pixel/";

        /// <summary>Multi-pointed crystalline star burst — impact flares, crescendo bursts, crit detonations.</summary>
        public static readonly Asset<Texture2D> ConstellationBurstFlare =
            ModContent.Request<Texture2D>(SharedFlare + "ConstellationNodeBurstFlare", AssetRequestMode.ImmediateLoad);

        /// <summary>Ice blue → lavender → violet horizontal gradient — trail color grading, beam gradients.</summary>
        public static readonly Asset<Texture2D> ResonanceTrailGradient =
            ModContent.Request<Texture2D>(SharedGradients + "PrimaryResonanceTrailGradient", AssetRequestMode.ImmediateLoad);

        /// <summary>Soft white circular gradient — bloom masking, metaball soft-edge, beam endpoints.</summary>
        public static readonly Asset<Texture2D> CircularMask =
            ModContent.Request<Texture2D>(SharedOrbs + "CircularMask", AssetRequestMode.ImmediateLoad);

        /// <summary>Glowing ring with dark center — aura rings, shockwave blooms, orbital accents.</summary>
        public static readonly Asset<Texture2D> BloomOrb =
            ModContent.Request<Texture2D>(SharedOrbs + "SoftCircularBloomOrb", AssetRequestMode.ImmediateLoad);

        /// <summary>Clean 4-pointed star sparkle — ambient constellation stars, impact sparks, node accents.</summary>
        public static readonly Asset<Texture2D> Star4Point =
            ModContent.Request<Texture2D>(SharedPixel + "Crisp4PointedStar", AssetRequestMode.ImmediateLoad);

        #endregion

        #region EternalMoon

        private static readonly string EternalMoonGradients = "MagnumOpus/Assets/MoonlightSonata/EternalMoon/Gradients/";
        private static readonly string EternalMoonTrails = "MagnumOpus/Assets/MoonlightSonata/EternalMoon/Trails/";
        private static readonly string EternalMoonTrailsClear = "MagnumOpus/Assets/MoonlightSonata/EternalMoon/Trails/Clear/";
        private static readonly string EternalMoonOrbs = "MagnumOpus/Assets/MoonlightSonata/EternalMoon/Orbs/";
        private static readonly string EternalMoonFlare = "MagnumOpus/Assets/MoonlightSonata/EternalMoon/Flare/";
        private static readonly string EternalMoonPixel = "MagnumOpus/Assets/MoonlightSonata/EternalMoon/Pixel/";

        /// <summary>Tidal color gradient for shader trail sampling — deep purple to ice blue to moon white.</summary>
        public static readonly Asset<Texture2D> TidalGradient =
            ModContent.Request<Texture2D>(EternalMoonGradients + "TidalGradient", AssetRequestMode.ImmediateLoad);

        /// <summary>Tidal trail body texture — thin glow line for shader-driven trail rendering.</summary>
        public static readonly Asset<Texture2D> TidalTrailBody =
            ModContent.Request<Texture2D>(EternalMoonTrails + "TidalTrailBody", AssetRequestMode.ImmediateLoad);

        /// <summary>Tidal energy texture — flowing energy pattern for trail overlay.</summary>
        public static readonly Asset<Texture2D> TidalEnergy =
            ModContent.Request<Texture2D>(EternalMoonTrails + "TidalEnergy", AssetRequestMode.ImmediateLoad);

        /// <summary>Tidal glow clear — additive-only trail glow for bloom stacking.</summary>
        public static readonly Asset<Texture2D> TidalGlow =
            ModContent.Request<Texture2D>(EternalMoonTrailsClear + "TidalGlow", AssetRequestMode.ImmediateLoad);

        /// <summary>Tidal bloom orb — soft glow for crescent bloom and impact effects.</summary>
        public static readonly Asset<Texture2D> TidalBloom =
            ModContent.Request<Texture2D>(EternalMoonOrbs + "TidalBloom", AssetRequestMode.ImmediateLoad);

        /// <summary>Crescent mask — feathered circle for aura rendering.</summary>
        public static readonly Asset<Texture2D> CrescentMask =
            ModContent.Request<Texture2D>(EternalMoonOrbs + "CrescentMask", AssetRequestMode.ImmediateLoad);

        /// <summary>Crescent flare — burst flare for impacts and phase transitions.</summary>
        public static readonly Asset<Texture2D> CrescentFlare =
            ModContent.Request<Texture2D>(EternalMoonFlare + "CrescentFlare", AssetRequestMode.ImmediateLoad);

        /// <summary>Tidal lens flare — elongated lens effect for blade tip.</summary>
        public static readonly Asset<Texture2D> TidalLensFlare =
            ModContent.Request<Texture2D>(EternalMoonFlare + "TidalLensFlare", AssetRequestMode.ImmediateLoad);

        /// <summary>Tidal spark — small bright pixel spark for finest detail particles.</summary>
        public static readonly Asset<Texture2D> TidalSpark =
            ModContent.Request<Texture2D>(EternalMoonPixel + "TidalSpark", AssetRequestMode.ImmediateLoad);

        #endregion

        #region IncisorOfMoonlight

        private static readonly string IncisorFlare = "MagnumOpus/Assets/MoonlightSonata/IncisorOfMoonlight/Flare/";
        private static readonly string IncisorTrails = "MagnumOpus/Assets/MoonlightSonata/IncisorOfMoonlight/Trails/";

        /// <summary>Crystalline tuning fork with violet/ice glow — Incisor weapon identity flare.</summary>
        public static readonly Asset<Texture2D> TuningForkFlare =
            ModContent.Request<Texture2D>(IncisorFlare + "TuningForkResonanceFlare", AssetRequestMode.ImmediateLoad);

        /// <summary>Horizontal stellar trail with scattered star particles — Incisor trail texture.</summary>
        public static readonly Asset<Texture2D> IncisorTrail =
            ModContent.Request<Texture2D>(IncisorTrails + "IncisorOfMoonlightTrail", AssetRequestMode.ImmediateLoad);

        #endregion
    }
}
