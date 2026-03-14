using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Shaders;

namespace MagnumOpus.Common.Systems.Bosses
{
    /// <summary>
    /// Central boss shader management system. Loads and provides access to all 
    /// boss-specific shaders across the mod. Each boss gets 4-5 dedicated shaders
    /// that define their visual identity.
    ///
    /// Shader categories per boss:
    ///   1. Aura/Presence shader   — ambient glow, energy field around the boss
    ///   2. Trail/Dash shader      — movement trails during dashes/charges
    ///   3. Attack shader          — projectile/attack-specific VFX
    ///   4. Phase shader           — phase transition, awakening, or enrage visual
    ///   5. Death/Arena shader     — death dissolve or arena environment effect
    /// </summary>
    public class BossShaderManager : ModSystem
    {
        #region Shader Keys

        // ─── Eroica ────────────────────────────────────────────
        public const string EroicaValorAura = "Eroica/Boss/EroicaValorAura";
        public const string EroicaHeroicTrail = "Eroica/Boss/EroicaHeroicTrail";
        public const string EroicaPhoenixFlame = "Eroica/Boss/EroicaPhoenixFlame";
        public const string EroicaSakuraTransition = "Eroica/Boss/EroicaSakuraTransition";
        public const string EroicaDeathDissolveFx = "Eroica/Boss/EroicaDeathDissolve";

        // ─── La Campanella ─────────────────────────────────────
        public const string CampanellaBellAura = "LaCampanella/Boss/CampanellaBellAura";
        public const string CampanellaInfernalTrail = "LaCampanella/Boss/CampanellaInfernalTrail";
        public const string CampanellaResonanceWave = "LaCampanella/Boss/CampanellaResonanceWave";
        public const string CampanellaFirewall = "LaCampanella/Boss/CampanellaFirewall";
        public const string CampanellaChimeDissolve = "LaCampanella/Boss/CampanellaChimeDissolve";

        // ─── Swan Lake ─────────────────────────────────────────
        public const string SwanPrismaticAura = "SwanLake/Boss/SwanPrismaticAura";
        public const string SwanFeatherTrail = "SwanLake/Boss/SwanFeatherTrail";
        public const string SwanFractalBeam = "SwanLake/Boss/SwanFractalBeam";
        public const string SwanMoodTransition = "SwanLake/Boss/SwanMoodTransition";
        public const string SwanMonochromeDissolve = "SwanLake/Boss/SwanMonochromeDissolve";

        // ─── Enigma Variations ─────────────────────────────────
        public const string EnigmaVoidAura = "EnigmaVariations/Boss/EnigmaVoidAura";
        public const string EnigmaShadowTrail = "EnigmaVariations/Boss/EnigmaShadowTrail";
        public const string EnigmaParadoxRift = "EnigmaVariations/Boss/EnigmaParadoxRift";
        public const string EnigmaTeleportWarp = "EnigmaVariations/Boss/EnigmaTeleportWarp";
        public const string EnigmaUnveilingDissolve = "EnigmaVariations/Boss/EnigmaUnveilingDissolve";

        // ─── Fate ──────────────────────────────────────────────
        public const string FateCosmicAura = "Fate/Boss/FateCosmicAura";
        public const string FateConstellationTrail = "Fate/Boss/FateConstellationTrail";
        public const string FateTimeSlice = "Fate/Boss/FateTimeSlice";
        public const string FateAwakeningShatter = "Fate/Boss/FateAwakeningShatter";
        public const string FateCosmicDeathRift = "Fate/Boss/FateCosmicDeathRift";

        // ─── Nachtmusik ────────────────────────────────────────
        public const string NachtmusikStarfieldAura = "Nachtmusik/Boss/NachtmusikStarfieldAura";
        public const string NachtmusikNebulaDashTrail = "Nachtmusik/Boss/NachtmusikNebulaDashTrail";
        public const string NachtmusikSupernovaBlast = "Nachtmusik/Boss/NachtmusikSupernovaBlast";
        public const string NachtmusikPhase2Awakening = "Nachtmusik/Boss/NachtmusikPhase2Awakening";
        public const string NachtmusikStellarDissolve = "Nachtmusik/Boss/NachtmusikStellarDissolve";

        // ─── Ode to Joy ────────────────────────────────────────
        public const string OdeGardenAura = "OdeToJoy/Boss/OdeGardenAura";
        public const string OdeVineTrail = "OdeToJoy/Boss/OdeVineTrail";
        public const string OdePetalStorm = "OdeToJoy/Boss/OdePetalStorm";
        public const string OdeChromaticBloom = "OdeToJoy/Boss/OdeChromaticBloom";
        public const string OdeJubilantDissolve = "OdeToJoy/Boss/OdeJubilantDissolve";

        // ─── Dies Irae ─────────────────────────────────────────
        public const string DiesHellfireAura = "DiesIrae/Boss/DiesHellfireAura";
        public const string DiesJudgmentTrail = "DiesIrae/Boss/DiesJudgmentTrail";
        public const string DiesApocalypseRay = "DiesIrae/Boss/DiesApocalypseRay";
        public const string DiesWrathEscalation = "DiesIrae/Boss/DiesWrathEscalation";
        public const string DiesFinalJudgmentDissolve = "DiesIrae/Boss/DiesFinalJudgmentDissolve";

        // ─── Autunno ───────────────────────────────────────────
        public const string AutunnoDecayAura = "Seasons/Boss/AutunnoDecayAura";
        public const string AutunnoLeafTrail = "Seasons/Boss/AutunnoLeafTrail";
        public const string AutunnoWitheringWind = "Seasons/Boss/AutunnoWitheringWind";
        public const string AutunnoHarvestMoon = "Seasons/Boss/AutunnoHarvestMoon";
        public const string AutunnoFinalHarvest = "Seasons/Boss/AutunnoFinalHarvest";

        // ─── Primavera ─────────────────────────────────────────
        public const string PrimaveraBloomAura = "Seasons/Boss/PrimaveraBloomAura";
        public const string PrimaveraPetalTrail = "Seasons/Boss/PrimaveraPetalTrail";
        public const string PrimaveraGrowthPulse = "Seasons/Boss/PrimaveraGrowthPulse";
        public const string PrimaveraVernalStorm = "Seasons/Boss/PrimaveraVernalStorm";
        public const string PrimaveraRebirthDissolve = "Seasons/Boss/PrimaveraRebirthDissolve";

        // ─── L'Estate ──────────────────────────────────────────
        public const string EstateSolarAura = "Seasons/Boss/EstateSolarAura";
        public const string EstateHeatHazeTrail = "Seasons/Boss/EstateHeatHazeTrail";
        public const string EstateSolarFlare = "Seasons/Boss/EstateSolarFlare";
        public const string EstateZenithBeam = "Seasons/Boss/EstateZenithBeam";
        public const string EstateSupernovaDissolve = "Seasons/Boss/EstateSupernovaDissolve";

        // ─── L'Estate (non-boss-path phase shaders) ─────────
        public const string EstateHeatShimmer = "Seasons/EstateHeatShimmer";
        public const string EstateLightningTelegraph = "Seasons/EstateLightningTelegraph";
        public const string EstateCoronaFlame = "Seasons/EstateCoronaFlame";
        public const string EstateSolarEclipse = "Seasons/EstateSolarEclipse";
        public const string EstateAfterburn = "Seasons/EstateAfterburn";

        // ─── L'Inverno ─────────────────────────────────────────
        public const string InvernoFrostAura = "Seasons/Boss/InvernoFrostAura";
        public const string InvernoIceTrail = "Seasons/Boss/InvernoIceTrail";
        public const string InvernoBlizzardVortex = "Seasons/Boss/InvernoBlizzardVortex";
        public const string InvernoFreezeRay = "Seasons/Boss/InvernoFreezeRay";
        public const string InvernoAbsoluteZeroDissolve = "Seasons/Boss/InvernoAbsoluteZeroDissolve";

        // L'Inverno phase-aware screen/environment shaders
        public const string InvernoFrostFloor = "Seasons/Boss/InvernoFrostFloor";
        public const string InvernoFrostCreep = "Seasons/Boss/InvernoFrostCreep";
        public const string InvernoWhiteout = "Seasons/Boss/InvernoWhiteout";

        #endregion

        #region Shader Access

        /// <summary>
        /// Gets a boss shader by key. Returns null if shader not loaded or unavailable.
        /// Uses ShaderLoader's central infrastructure.
        /// </summary>
        public static Effect GetShader(string key)
        {
            if (!ShaderLoader.ShadersEnabled) return null;
            return ShaderLoader.GetShader(key);
        }

        /// <summary>Checks if a boss shader is available.</summary>
        public static bool HasShader(string key) => ShaderLoader.HasShader(key);

        #endregion

        #region Common Shader Parameter Helpers

        /// <summary>
        /// Pre-applies movement phase uniforms (uMovement, uHeroIntensity) to an Eroica shader.
        /// Call before the render helper so these values are present when the pass is applied.
        /// </summary>
        public static void ApplyMovementParams(Effect shader, float movement, float heroIntensity)
        {
            if (shader == null) return;
            var p = shader.Parameters;
            p["uMovement"]?.SetValue(movement);
            p["uHeroIntensity"]?.SetValue(heroIntensity);
        }

        /// <summary>
        /// Applies standard boss aura shader parameters.
        /// </summary>
        public static void ApplyAuraParams(Effect shader, Vector2 center, float radius, float intensity,
            Color primaryColor, Color secondaryColor, float time)
        {
            if (shader == null) return;
            var p = shader.Parameters;
            p["uCenter"]?.SetValue(center);
            p["uRadius"]?.SetValue(radius);
            p["uIntensity"]?.SetValue(intensity);
            p["uPrimaryColor"]?.SetValue(primaryColor.ToVector4());
            p["uSecondaryColor"]?.SetValue(secondaryColor.ToVector4());
            p["uTime"]?.SetValue(time);
        }

        /// <summary>
        /// Applies standard boss trail shader parameters.
        /// </summary>
        public static void ApplyTrailParams(Effect shader, Color trailColor, float width,
            float fadeRate, float time, Texture2D noiseTexture = null)
        {
            if (shader == null) return;
            var p = shader.Parameters;
            p["uColor"]?.SetValue(trailColor.ToVector4());
            p["uTrailWidth"]?.SetValue(width);
            p["uFadeRate"]?.SetValue(fadeRate);
            p["uTime"]?.SetValue(time);
            if (noiseTexture != null)
                Main.graphics.GraphicsDevice.Textures[1] = noiseTexture;
        }

        /// <summary>
        /// Applies dissolve/death shader parameters.
        /// </summary>
        public static void ApplyDissolveParams(Effect shader, float dissolveProgress, Color edgeColor,
            float edgeWidth, Texture2D dissolveTexture = null)
        {
            if (shader == null) return;
            var p = shader.Parameters;
            p["uDissolveProgress"]?.SetValue(dissolveProgress);
            p["uEdgeColor"]?.SetValue(edgeColor.ToVector4());
            p["uEdgeWidth"]?.SetValue(edgeWidth);
            if (dissolveTexture != null)
                Main.graphics.GraphicsDevice.Textures[1] = dissolveTexture;
        }

        /// <summary>
        /// Applies phase transition shader parameters.
        /// </summary>
        public static void ApplyPhaseTransitionParams(Effect shader, float transitionProgress,
            Color fromColor, Color toColor, float intensity, float time)
        {
            if (shader == null) return;
            var p = shader.Parameters;
            p["uTransitionProgress"]?.SetValue(transitionProgress);
            p["uFromColor"]?.SetValue(fromColor.ToVector4());
            p["uToColor"]?.SetValue(toColor.ToVector4());
            p["uIntensity"]?.SetValue(intensity);
            p["uTime"]?.SetValue(time);
        }

        #endregion
    }
}
