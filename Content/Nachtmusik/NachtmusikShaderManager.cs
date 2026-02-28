using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using MagnumOpus.Common.Systems.Shaders;

namespace MagnumOpus.Content.Nachtmusik
{
    /// <summary>
    /// Per-weapon shader manager for all Nachtmusik VFX.
    /// Each weapon has its own dedicated shader with unique visual identity.
    /// Wraps ShaderLoader for easy Apply* calls from weapon VFX files.
    ///
    /// Core shaders (shared):
    ///   NachtmusikStarTrail  — Twinkling star point field trail (melee base)
    ///   NachtmusikSerenade   — Harmonic wave aura/bloom (all weapons)
    ///
    /// Per-weapon shaders (unique):
    ///   ExecutionDecree       — NocturnalExecutioner: void-rip slash
    ///   CrescendoRise         — MidnightsCrescendo: intensity-building trail
    ///   DimensionalRift       — TwilightSeverance: dimensional tear slash
    ///   StarChainBeam         — ConstellationPiercer: precision bullet trail
    ///   NebulaScatter         — NebulasWhisper: gaseous nebula cloud trail
    ///   StarHomingTrail       — SerenadeOfDistantStars: arcing star ribbon
    ///   ConstellationWeave    — StarweaversGrimoire: star map charge orb
    ///   CosmicRequiem         — RequiemOfTheCosmos: channeled nebula beam
    ///   ChorusSummonAura      — CelestialChorusBaton: musical note aura
    ///   OvertureAura          — GalacticOverture: orchestral wave aura
    ///   StellarConductorAura  — ConductorOfConstellations: constellation ring
    /// </summary>
    public static class NachtmusikShaderManager
    {
        // =====================================================================
        //  Shader Availability — Per-Weapon
        // =====================================================================

        public static bool HasStarTrail => ShaderLoader.HasShader(ShaderLoader.NachtmusikStarTrailShader);
        public static bool HasSerenade => ShaderLoader.HasShader(ShaderLoader.NachtmusikSerenadeShader);
        public static bool HasExecutionDecree => ShaderLoader.HasShader(ShaderLoader.ExecutionDecreeShader);
        public static bool HasCrescendoRise => ShaderLoader.HasShader(ShaderLoader.CrescendoRiseShader);
        public static bool HasDimensionalRift => ShaderLoader.HasShader(ShaderLoader.DimensionalRiftShader);
        public static bool HasStarChainBeam => ShaderLoader.HasShader(ShaderLoader.StarChainBeamShader);
        public static bool HasNebulaScatter => ShaderLoader.HasShader(ShaderLoader.NebulaScatterShader);
        public static bool HasStarHomingTrail => ShaderLoader.HasShader(ShaderLoader.StarHomingTrailShader);
        public static bool HasConstellationWeave => ShaderLoader.HasShader(ShaderLoader.ConstellationWeaveShader);
        public static bool HasCosmicRequiem => ShaderLoader.HasShader(ShaderLoader.CosmicRequiemShader);
        public static bool HasChorusSummonAura => ShaderLoader.HasShader(ShaderLoader.ChorusSummonAuraShader);
        public static bool HasOvertureAura => ShaderLoader.HasShader(ShaderLoader.OvertureAuraShader);
        public static bool HasStellarConductorAura => ShaderLoader.HasShader(ShaderLoader.StellarConductorAuraShader);

        /// <summary>True if any Nachtmusik shader is available.</summary>
        public static bool IsAvailable => HasStarTrail || HasSerenade || HasExecutionDecree;

        /// <summary>True if the shared scrolling trail shader is available (fallback).</summary>
        public static bool HasFallbackTrail => ShaderLoader.HasShader(ShaderLoader.ScrollingTrailShader);

        // =====================================================================
        //  Noise Texture Binding
        // =====================================================================

        /// <summary>
        /// Binds StarFieldScatter noise to sampler slot 1 for twinkling star distortion.
        /// </summary>
        public static void BindStarfieldNoise(GraphicsDevice device)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("StarFieldScatter")
                           ?? ShaderLoader.GetNoiseTexture("SparklyNoiseTexture")
                           ?? ShaderLoader.GetNoiseTexture("PerlinNoise");
            if (noise != null)
            {
                device.Textures[1] = noise;
                device.SamplerStates[1] = SamplerState.LinearWrap;
            }
        }

        /// <summary>
        /// Binds CosmicNebulaClouds noise for nebula/cosmic weapon effects.
        /// </summary>
        public static void BindCosmicNoise(GraphicsDevice device)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("CosmicNebulaClouds")
                           ?? ShaderLoader.GetNoiseTexture("NebulaWispNoise")
                           ?? ShaderLoader.GetNoiseTexture("StarFieldScatter");
            if (noise != null)
            {
                device.Textures[1] = noise;
                device.SamplerStates[1] = SamplerState.LinearWrap;
            }
        }

        /// <summary>
        /// Binds CosmicEnergyVortex noise for high-energy effects (execution, requiem).
        /// </summary>
        public static void BindVortexNoise(GraphicsDevice device)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("CosmicEnergyVortex")
                           ?? ShaderLoader.GetNoiseTexture("CosmicNebulaClouds")
                           ?? ShaderLoader.GetNoiseTexture("PerlinNoise");
            if (noise != null)
            {
                device.Textures[1] = noise;
                device.SamplerStates[1] = SamplerState.LinearWrap;
            }
        }

        // =====================================================================
        //  Common Uniform Setter
        // =====================================================================

        private static void SetCommonUniforms(Effect shader, float time, Color primary, Color secondary,
            float opacity = 1f, float intensity = 1.5f, float overbrightMult = 2.5f,
            float scrollSpeed = 1f, float distortionAmt = 0.06f,
            float hasSecondaryTex = 1f, float secondaryTexScale = 3f, float secondaryTexScroll = 0.5f)
        {
            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(opacity);
            shader.Parameters["uIntensity"]?.SetValue(intensity);
            shader.Parameters["uOverbrightMult"]?.SetValue(overbrightMult);
            shader.Parameters["uScrollSpeed"]?.SetValue(scrollSpeed);
            shader.Parameters["uDistortionAmt"]?.SetValue(distortionAmt);
            shader.Parameters["uHasSecondaryTex"]?.SetValue(hasSecondaryTex);
            shader.Parameters["uSecondaryTexScale"]?.SetValue(secondaryTexScale);
            shader.Parameters["uSecondaryTexScroll"]?.SetValue(secondaryTexScroll);
        }

        private static void SetPhaseUniform(Effect shader, float phase)
        {
            shader.Parameters["uPhase"]?.SetValue(phase);
        }

        // =====================================================================
        //  CORE: NachtmusikStarTrail — shared twinkling star trail
        // =====================================================================

        /// <summary>Apply the core NachtmusikStarTrail shader (technique: NachtmusikStarFlow).</summary>
        public static void ApplyStarTrail(float time, Color primary, Color secondary,
            float scrollSpeed = 1f, float distortionAmt = 0.06f, float overbrightMult = 2.5f)
        {
            Effect shader = ShaderLoader.NachtmusikStarTrail;
            if (shader == null) return;

            BindStarfieldNoise(Main.graphics.GraphicsDevice);
            SetCommonUniforms(shader, time, primary, secondary,
                scrollSpeed: scrollSpeed, distortionAmt: distortionAmt, overbrightMult: overbrightMult);

            shader.CurrentTechnique = shader.Techniques["NachtmusikStarFlow"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>Apply the core NachtmusikStarTrail glow pass (technique: NachtmusikStarGlow).</summary>
        public static void ApplyStarTrailGlow(float time, Color primary, Color secondary,
            float scrollSpeed = 1f, float overbrightMult = 2.5f)
        {
            Effect shader = ShaderLoader.NachtmusikStarTrail;
            if (shader == null) return;

            SetCommonUniforms(shader, time, primary, secondary,
                scrollSpeed: scrollSpeed, overbrightMult: overbrightMult);

            shader.CurrentTechnique = shader.Techniques["NachtmusikStarGlow"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        // =====================================================================
        //  CORE: NachtmusikSerenade — shared aura/bloom
        // =====================================================================

        /// <summary>Apply the NachtmusikSerenade aura shader.</summary>
        public static void ApplySerenade(float time, Color primary, Color secondary,
            float phase = 1f, float overbrightMult = 2.5f)
        {
            Effect shader = ShaderLoader.NachtmusikSerenade;
            if (shader == null) return;

            BindStarfieldNoise(Main.graphics.GraphicsDevice);
            SetCommonUniforms(shader, time, primary, secondary, overbrightMult: overbrightMult);
            SetPhaseUniform(shader, phase);

            shader.CurrentTechnique = shader.Techniques["NachtmusikSerenadePass"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        // =====================================================================
        //  WEAPON: NocturnalExecutioner — ExecutionDecree
        // =====================================================================

        /// <summary>Heavy void-rip slash trail for NocturnalExecutioner.</summary>
        public static void ApplyExecutionDecree(float time, float intensity = 1.8f)
        {
            Effect shader = ShaderLoader.ExecutionDecree;
            if (shader == null) { ApplyStarTrail(time, NachtmusikPalette.MidnightBlue, NachtmusikPalette.Violet); return; }

            BindVortexNoise(Main.graphics.GraphicsDevice);
            SetCommonUniforms(shader, time, NachtmusikPalette.CosmicVoid, NachtmusikPalette.Violet,
                intensity: intensity, overbrightMult: 3.5f, scrollSpeed: 1.2f,
                distortionAmt: 0.12f, secondaryTexScale: 2.5f, secondaryTexScroll: 0.8f);

            shader.CurrentTechnique = shader.Techniques["ExecutionDecreeSlash"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>Glow pass for Execution Decree.</summary>
        public static void ApplyExecutionDecreeGlow(float time)
        {
            Effect shader = ShaderLoader.ExecutionDecree;
            if (shader == null) return;

            SetCommonUniforms(shader, time, NachtmusikPalette.CosmicVoid, NachtmusikPalette.Violet,
                overbrightMult: 3.0f);

            shader.CurrentTechnique = shader.Techniques["ExecutionDecreeGlow"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        // =====================================================================
        //  WEAPON: MidnightsCrescendo — CrescendoRise
        // =====================================================================

        /// <summary>Intensity-building trail for MidnightsCrescendo. crescendoLevel 0..1.</summary>
        public static void ApplyCrescendoRise(float time, float crescendoLevel)
        {
            Effect shader = ShaderLoader.CrescendoRise;
            if (shader == null) { ApplyStarTrail(time, NachtmusikPalette.DeepBlue, NachtmusikPalette.StarWhite); return; }

            BindStarfieldNoise(Main.graphics.GraphicsDevice);
            float intensity = 1.2f + crescendoLevel * 1.8f;
            float overbrightMult = 2.0f + crescendoLevel * 2.0f;
            SetCommonUniforms(shader, time, NachtmusikPalette.DeepBlue, NachtmusikPalette.StarWhite,
                intensity: intensity, overbrightMult: overbrightMult, scrollSpeed: 1.5f,
                distortionAmt: 0.05f, secondaryTexScale: 3f, secondaryTexScroll: 0.6f);
            SetPhaseUniform(shader, crescendoLevel);

            shader.CurrentTechnique = shader.Techniques["CrescendoRise"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>Glow pass for Crescendo Rise.</summary>
        public static void ApplyCrescendoRiseGlow(float time, float crescendoLevel)
        {
            Effect shader = ShaderLoader.CrescendoRise;
            if (shader == null) return;

            SetCommonUniforms(shader, time, NachtmusikPalette.DeepBlue, NachtmusikPalette.StarWhite,
                overbrightMult: 2.5f + crescendoLevel * 1.5f);
            SetPhaseUniform(shader, crescendoLevel);

            shader.CurrentTechnique = shader.Techniques["CrescendoRiseGlow"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        // =====================================================================
        //  WEAPON: TwilightSeverance — DimensionalRift
        // =====================================================================

        /// <summary>Ultra-sharp dimensional tear trail for TwilightSeverance.</summary>
        public static void ApplyDimensionalRift(float time)
        {
            Effect shader = ShaderLoader.DimensionalRift;
            if (shader == null) { ApplyStarTrail(time, NachtmusikPalette.DuskViolet, NachtmusikPalette.MoonlitSilver); return; }

            BindStarfieldNoise(Main.graphics.GraphicsDevice);
            SetCommonUniforms(shader, time, NachtmusikPalette.DuskViolet, NachtmusikPalette.MoonlitSilver,
                intensity: 2.0f, overbrightMult: 3.5f, scrollSpeed: 3.0f,
                distortionAmt: 0.04f, secondaryTexScale: 4f, secondaryTexScroll: 1.0f);

            shader.CurrentTechnique = shader.Techniques["DimensionalRiftSlash"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>Glow pass for Dimensional Rift.</summary>
        public static void ApplyDimensionalRiftGlow(float time)
        {
            Effect shader = ShaderLoader.DimensionalRift;
            if (shader == null) return;

            SetCommonUniforms(shader, time, NachtmusikPalette.DuskViolet, NachtmusikPalette.MoonlitSilver,
                overbrightMult: 3.0f);

            shader.CurrentTechnique = shader.Techniques["DimensionalRiftGlow"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        // =====================================================================
        //  WEAPON: ConstellationPiercer — StarChainBeam
        // =====================================================================

        /// <summary>Precision constellation bullet trail for ConstellationPiercer.</summary>
        public static void ApplyStarChainBeam(float time)
        {
            Effect shader = ShaderLoader.StarChainBeam;
            if (shader == null) { ApplyStarTrail(time, NachtmusikPalette.ConstellationBlue, NachtmusikPalette.StarGold); return; }

            BindStarfieldNoise(Main.graphics.GraphicsDevice);
            SetCommonUniforms(shader, time, NachtmusikPalette.ConstellationBlue, NachtmusikPalette.StarGold,
                intensity: 1.8f, overbrightMult: 3.0f, scrollSpeed: 2.5f,
                distortionAmt: 0.03f, secondaryTexScale: 3f, secondaryTexScroll: 0.5f);

            shader.CurrentTechnique = shader.Techniques["StarChainBeam"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>Glow pass for Star Chain Beam.</summary>
        public static void ApplyStarChainBeamGlow(float time)
        {
            Effect shader = ShaderLoader.StarChainBeam;
            if (shader == null) return;

            SetCommonUniforms(shader, time, NachtmusikPalette.ConstellationBlue, NachtmusikPalette.StarGold,
                overbrightMult: 2.5f);

            shader.CurrentTechnique = shader.Techniques["StarChainBeamGlow"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        // =====================================================================
        //  WEAPON: NebulasWhisper — NebulaScatter
        // =====================================================================

        /// <summary>Gaseous nebula cloud trail for NebulasWhisper.</summary>
        public static void ApplyNebulaScatter(float time)
        {
            Effect shader = ShaderLoader.NebulaScatter;
            if (shader == null) { ApplyStarTrail(time, NachtmusikPalette.CosmicPurple, NachtmusikPalette.NebulaPink); return; }

            BindCosmicNoise(Main.graphics.GraphicsDevice);
            SetCommonUniforms(shader, time, NachtmusikPalette.CosmicPurple, NachtmusikPalette.NebulaPink,
                intensity: 1.5f, overbrightMult: 2.5f, scrollSpeed: 0.8f,
                distortionAmt: 0.09f, secondaryTexScale: 2.5f, secondaryTexScroll: 0.4f);

            shader.CurrentTechnique = shader.Techniques["NebulaScatterTrail"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>Glow pass for Nebula Scatter.</summary>
        public static void ApplyNebulaScatterGlow(float time)
        {
            Effect shader = ShaderLoader.NebulaScatter;
            if (shader == null) return;

            SetCommonUniforms(shader, time, NachtmusikPalette.CosmicPurple, NachtmusikPalette.NebulaPink,
                overbrightMult: 2.0f);

            shader.CurrentTechnique = shader.Techniques["NebulaScatterGlow"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        // =====================================================================
        //  WEAPON: SerenadeOfDistantStars — StarHomingTrail
        // =====================================================================

        /// <summary>Graceful arcing star ribbon trail for SerenadeOfDistantStars.</summary>
        public static void ApplyStarHomingTrail(float time)
        {
            Effect shader = ShaderLoader.StarHomingTrail;
            if (shader == null) { ApplyStarTrail(time, NachtmusikPalette.DeepBlue, NachtmusikPalette.MoonlitSilver); return; }

            BindStarfieldNoise(Main.graphics.GraphicsDevice);
            SetCommonUniforms(shader, time, NachtmusikPalette.DeepBlue, NachtmusikPalette.StarGold,
                intensity: 1.6f, overbrightMult: 2.6f, scrollSpeed: 1.2f,
                distortionAmt: 0.05f, secondaryTexScale: 3f, secondaryTexScroll: 0.5f);

            shader.CurrentTechnique = shader.Techniques["StarHomingTrail"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>Glow pass for Star Homing Trail.</summary>
        public static void ApplyStarHomingTrailGlow(float time)
        {
            Effect shader = ShaderLoader.StarHomingTrail;
            if (shader == null) return;

            SetCommonUniforms(shader, time, NachtmusikPalette.DeepBlue, NachtmusikPalette.StarGold,
                overbrightMult: 2.2f);

            shader.CurrentTechnique = shader.Techniques["StarHomingGlow"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        // =====================================================================
        //  WEAPON: StarweaversGrimoire — ConstellationWeave
        // =====================================================================

        /// <summary>Constellation star map orb for StarweaversGrimoire. chargeLevel 0..1.</summary>
        public static void ApplyConstellationWeave(float time, float chargeLevel)
        {
            Effect shader = ShaderLoader.ConstellationWeave;
            if (shader == null) return;

            BindStarfieldNoise(Main.graphics.GraphicsDevice);
            SetCommonUniforms(shader, time, NachtmusikPalette.CosmicPurple, NachtmusikPalette.StarGold,
                intensity: 1.5f + chargeLevel * 1.0f, overbrightMult: 2.5f + chargeLevel * 1.5f,
                scrollSpeed: 1.3f, secondaryTexScale: 2f, secondaryTexScroll: 0.3f);
            SetPhaseUniform(shader, chargeLevel);

            shader.CurrentTechnique = shader.Techniques["ConstellationWeaveOrb"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>Glow pass for Constellation Weave.</summary>
        public static void ApplyConstellationWeaveGlow(float time, float chargeLevel)
        {
            Effect shader = ShaderLoader.ConstellationWeave;
            if (shader == null) return;

            SetCommonUniforms(shader, time, NachtmusikPalette.CosmicPurple, NachtmusikPalette.StarGold,
                overbrightMult: 2.0f);
            SetPhaseUniform(shader, chargeLevel);

            shader.CurrentTechnique = shader.Techniques["ConstellationWeaveGlow"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        // =====================================================================
        //  WEAPON: RequiemOfTheCosmos — CosmicRequiem
        // =====================================================================

        /// <summary>Channeled nebula-swirl beam for RequiemOfTheCosmos. phase 0..1 channel intensity.</summary>
        public static void ApplyCosmicRequiem(float time, float phase)
        {
            Effect shader = ShaderLoader.CosmicRequiem;
            if (shader == null) return;

            BindCosmicNoise(Main.graphics.GraphicsDevice);
            SetCommonUniforms(shader, time, NachtmusikPalette.CosmicVoid, NachtmusikPalette.RadianceGold,
                intensity: 1.5f + phase * 2.0f, overbrightMult: 3.0f + phase * 1.5f,
                scrollSpeed: 0.7f, distortionAmt: 0.12f,
                secondaryTexScale: 2f, secondaryTexScroll: 0.3f);
            SetPhaseUniform(shader, phase);

            shader.CurrentTechnique = shader.Techniques["CosmicRequiemBeam"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>Glow pass for Cosmic Requiem.</summary>
        public static void ApplyCosmicRequiemGlow(float time, float phase)
        {
            Effect shader = ShaderLoader.CosmicRequiem;
            if (shader == null) return;

            SetCommonUniforms(shader, time, NachtmusikPalette.CosmicVoid, NachtmusikPalette.RadianceGold,
                overbrightMult: 2.5f + phase * 1.0f);
            SetPhaseUniform(shader, phase);

            shader.CurrentTechnique = shader.Techniques["CosmicRequiemBeamGlow"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        // =====================================================================
        //  WEAPON: CelestialChorusBaton — ChorusSummonAura
        // =====================================================================

        /// <summary>Musical note constellation aura for CelestialChorusBaton minion.</summary>
        public static void ApplyChorusSummonAura(float time, float phase = 1f)
        {
            Effect shader = ShaderLoader.ChorusSummonAura;
            if (shader == null) return;

            BindStarfieldNoise(Main.graphics.GraphicsDevice);
            SetCommonUniforms(shader, time, NachtmusikPalette.ConstellationBlue, NachtmusikPalette.StarGold,
                intensity: 1.4f, overbrightMult: 2.5f, scrollSpeed: 1.0f,
                secondaryTexScale: 3f, secondaryTexScroll: 0.4f);
            SetPhaseUniform(shader, phase);

            shader.CurrentTechnique = shader.Techniques["ChorusSummonAura"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        // =====================================================================
        //  WEAPON: GalacticOverture — OvertureAura
        // =====================================================================

        /// <summary>Orchestral wave aura for GalacticOverture minion.</summary>
        public static void ApplyOvertureAura(float time, float phase = 1f)
        {
            Effect shader = ShaderLoader.OvertureAura;
            if (shader == null) return;

            BindCosmicNoise(Main.graphics.GraphicsDevice);
            SetCommonUniforms(shader, time, NachtmusikPalette.DeepBlue, NachtmusikPalette.StarGold,
                intensity: 1.5f, overbrightMult: 2.8f, scrollSpeed: 1.4f,
                secondaryTexScale: 2.5f, secondaryTexScroll: 0.5f);
            SetPhaseUniform(shader, phase);

            shader.CurrentTechnique = shader.Techniques["OvertureAura"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        // =====================================================================
        //  WEAPON: ConductorOfConstellations — StellarConductorAura
        // =====================================================================

        /// <summary>Orbiting constellation ring aura for ConductorOfConstellations minion.</summary>
        public static void ApplyStellarConductorAura(float time, float phase = 1f)
        {
            Effect shader = ShaderLoader.StellarConductorAura;
            if (shader == null) return;

            BindStarfieldNoise(Main.graphics.GraphicsDevice);
            SetCommonUniforms(shader, time, NachtmusikPalette.MidnightBlue, NachtmusikPalette.StarWhite,
                intensity: 1.6f, overbrightMult: 3.0f, scrollSpeed: 1.6f,
                secondaryTexScale: 3.5f, secondaryTexScroll: 0.5f);
            SetPhaseUniform(shader, phase);

            shader.CurrentTechnique = shader.Techniques["StellarConductorAura"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        // =====================================================================
        //  FALLBACK: Use shared ScrollingTrailShader
        // =====================================================================

        /// <summary>
        /// Fallback trail rendering using the shared ScrollingTrailShader.
        /// Used when weapon-specific shaders are unavailable.
        /// </summary>
        public static void ApplyFallbackTrail(float time, Color primary, Color secondary,
            float scrollSpeed = 1f, float overbrightMult = 2.5f)
        {
            Effect shader = ShaderLoader.ScrollingTrail;
            if (shader == null) shader = ShaderLoader.Trail;
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(1.5f);
            shader.Parameters["uOverbrightMult"]?.SetValue(overbrightMult);
            shader.Parameters["uScrollSpeed"]?.SetValue(scrollSpeed);

            shader.CurrentTechnique.Passes[0].Apply();
        }

        // =====================================================================
        //  SpriteBatch State Helpers
        // =====================================================================

        /// <summary>Begins SpriteBatch in Immediate + Additive mode for shader drawing.</summary>
        public static void BeginShaderAdditive(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None,
                Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>Restores SpriteBatch to normal deferred alpha-blend mode.</summary>
        public static void RestoreSpriteBatch(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>Begins SpriteBatch in Deferred + Additive mode (no shader, for bloom stacking).</summary>
        public static void BeginAdditive(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None,
                Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }
}
