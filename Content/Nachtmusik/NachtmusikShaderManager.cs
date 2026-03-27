using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using MagnumOpus.Common.Systems.Shaders;

namespace MagnumOpus.Content.Nachtmusik
{
    /// <summary>
    /// Compartmentalized shader manager for all Nachtmusik weapon VFX.
    /// Provides availability checks, noise texture binding, generic Apply methods,
    /// and weapon-specific presets for each Nachtmusik shader.
    ///
    /// Shared theme-wide shaders:
    ///   NachtmusikStarTrail, NachtmusikSerenade
    ///
    /// Fully-implemented weapons (presets NOT provided here — see weapon files):
    ///   NocturnalExecutioner:  ExecutionDecree
    ///   MidnightsCrescendo:   CrescendoRise
    ///   TwilightSeverance:    DimensionalRift
    ///   ConstellationPiercer: StarChainBeam
    ///
    /// Gutted weapons (presets provided here for VFX restoration):
    ///   NebulasWhisper:              NebulaScatter (trail)
    ///   SerenadeOfDistantStars:      StarHomingTrail (trail)
    ///   StarweaversGrimoire:         ConstellationWeave (trail)
    ///   RequiemOfTheCosmos:          CosmicRequiem (trail)
    ///   CelestialChorusBaton:        ChorusSummonAura (radial)
    ///   GalacticOverture:            OvertureAura (radial)
    ///
    /// Usage (in PreDraw):
    ///   NachtmusikShaderManager.BeginShaderAdditive(sb);
    ///   NachtmusikShaderManager.BindNoiseTexture(device);
    ///   NachtmusikShaderManager.ApplyNebulaScatterTrail(time, glow: false);
    ///   // ... draw trail geometry ...
    ///   NachtmusikShaderManager.RestoreSpriteBatch(sb);
    ///
    /// All Apply* methods gracefully return false if the shader is null,
    /// allowing VFX code to fall back to particle-based rendering.
    /// </summary>
    public static class NachtmusikShaderManager
    {
        // =====================================================================
        //  Shader Availability — Theme-Wide Shared
        // =====================================================================

        public static bool HasStarTrail => ShaderLoader.HasShader(ShaderLoader.NachtmusikStarTrailShader);
        public static bool HasSerenade => ShaderLoader.HasShader(ShaderLoader.NachtmusikSerenadeShader);

        // =====================================================================
        //  Shader Availability — Fully Implemented Weapons (no presets here)
        // =====================================================================

        // NocturnalExecutioner
        public static bool HasExecutionDecree => ShaderLoader.HasShader(ShaderLoader.ExecutionDecreeShader);

        // MidnightsCrescendo
        public static bool HasCrescendoRise => ShaderLoader.HasShader(ShaderLoader.CrescendoRiseShader);

        // TwilightSeverance
        public static bool HasDimensionalRift => ShaderLoader.HasShader(ShaderLoader.DimensionalRiftShader);

        // ConstellationPiercer
        public static bool HasStarChainBeam => ShaderLoader.HasShader(ShaderLoader.StarChainBeamShader);

        // =====================================================================
        //  Shader Availability — Gutted Weapons (presets below)
        // =====================================================================

        // NebulasWhisper
        public static bool HasNebulaScatter => ShaderLoader.HasShader(ShaderLoader.NebulaScatterShader);

        // SerenadeOfDistantStars
        public static bool HasStarHomingTrail => ShaderLoader.HasShader(ShaderLoader.StarHomingTrailShader);

        // StarweaversGrimoire
        public static bool HasConstellationWeave => ShaderLoader.HasShader(ShaderLoader.ConstellationWeaveShader);

        // RequiemOfTheCosmos
        public static bool HasCosmicRequiem => ShaderLoader.HasShader(ShaderLoader.CosmicRequiemShader);

        // CelestialChorusBaton
        public static bool HasChorusSummonAura => ShaderLoader.HasShader(ShaderLoader.ChorusSummonAuraShader);

        // GalacticOverture
        public static bool HasOvertureAura => ShaderLoader.HasShader(ShaderLoader.OvertureAuraShader);


        /// <summary>True if any Nachtmusik shader is available.</summary>
        public static bool IsAvailable =>
            HasStarTrail || HasSerenade ||
            HasExecutionDecree || HasCrescendoRise ||
            HasDimensionalRift || HasStarChainBeam ||
            HasNebulaScatter || HasStarHomingTrail ||
            HasConstellationWeave || HasCosmicRequiem ||
            HasChorusSummonAura || HasOvertureAura ;

        /// <summary>True if any trail shader is usable (dedicated or shared fallback).</summary>
        public static bool HasFallbackTrail => ShaderLoader.HasShader(ShaderLoader.ScrollingTrailShader);
        public static bool CanRenderTrails => HasNebulaScatter || HasStarHomingTrail || HasStarTrail || HasFallbackTrail;

        // =====================================================================
        //  Noise Texture Binding
        // =====================================================================

        /// <summary>
        /// Binds CosmicEnergyVortex to sampler slot 1 — Nachtmusik's primary noise.
        /// Swirling cosmic vortex ideal for stellar trails and nebula effects.
        /// </summary>
        public static void BindNoiseTexture(GraphicsDevice device)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("CosmicEnergyVortex");
            if (noise == null)
                noise = ShaderLoader.GetNoiseTexture("PerlinNoise");
            if (noise != null)
            {
                device.Textures[1] = noise;
                device.SamplerStates[1] = SamplerState.LinearWrap;
            }
        }

        /// <summary>
        /// Binds SimplexNoise to sampler slot 1 for weaving constellation patterns.
        /// Used by StarweaversGrimoire and intricate star-thread effects.
        /// </summary>
        public static void BindSimplexNoise(GraphicsDevice device)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("SimplexNoise");
            if (noise == null)
                noise = ShaderLoader.GetNoiseTexture("PerlinNoise");
            if (noise != null)
            {
                device.Textures[1] = noise;
                device.SamplerStates[1] = SamplerState.LinearWrap;
            }
        }

        /// <summary>
        /// Binds RealityCrackPattern to sampler slot 1 for dimensional rift shattering.
        /// Used by void-piercing and reality-tearing effects.
        /// </summary>
        public static void BindCrackNoiseTexture(GraphicsDevice device)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("RealityCrackPattern");
            if (noise == null)
                noise = ShaderLoader.GetNoiseTexture("PerlinNoise");
            if (noise != null)
            {
                device.Textures[1] = noise;
                device.SamplerStates[1] = SamplerState.LinearWrap;
            }
        }

        // =====================================================================
        //  Generic Apply Methods — Trail Shaders
        // =====================================================================

        /// <summary>
        /// Apply a trail shader with standard uniforms. Returns true if shader was applied.
        /// </summary>
        private static bool ApplyTrailShader(string shaderName, string technique,
            float time, Color primary, Color secondary,
            float scrollSpeed, float distortionAmt, float overbrightMult,
            float phase = 0f, float noiseScale = 3f, float noiseScroll = 0.5f,
            bool hasNoiseBound = false)
        {
            Effect shader = ShaderLoader.GetShader(shaderName);
            if (shader == null) return false;

            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(1.5f);
            shader.Parameters["uOverbrightMult"]?.SetValue(overbrightMult);
            shader.Parameters["uScrollSpeed"]?.SetValue(scrollSpeed);
            shader.Parameters["uDistortionAmt"]?.SetValue(distortionAmt);
            shader.Parameters["uNoiseScale"]?.SetValue(noiseScale);
            shader.Parameters["uPhase"]?.SetValue(phase);
            shader.Parameters["uHasSecondaryTex"]?.SetValue(hasNoiseBound ? 1f : 0f);
            shader.Parameters["uSecondaryTexScale"]?.SetValue(noiseScale);
            shader.Parameters["uSecondaryTexScroll"]?.SetValue(noiseScroll);

            shader.CurrentTechnique = shader.Techniques[technique];
            shader.CurrentTechnique.Passes[0].Apply();
            return true;
        }

        /// <summary>
        /// Apply a radial/aura shader with standard uniforms. Returns true if shader was applied.
        /// </summary>
        private static bool ApplyRadialShader(string shaderName, string technique,
            float time, Color primary, Color secondary,
            float explosionAge, float intensity, float overbrightMult,
            float noiseScale = 3f, float noiseScroll = 0.3f,
            bool hasNoiseBound = false)
        {
            Effect shader = ShaderLoader.GetShader(shaderName);
            if (shader == null) return false;

            shader.Parameters["uColor"]?.SetValue(new Vector4(primary.ToVector3(), 1f));
            shader.Parameters["uSecondaryColor"]?.SetValue(new Vector4(secondary.ToVector3(), 1f));
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(intensity);
            shader.Parameters["uOverbrightMult"]?.SetValue(overbrightMult);
            shader.Parameters["uScrollSpeed"]?.SetValue(1f);
            shader.Parameters["uNoiseScale"]?.SetValue(noiseScale);
            shader.Parameters["uDistortionAmt"]?.SetValue(0.1f);
            shader.Parameters["uPhase"]?.SetValue(explosionAge);
            shader.Parameters["uHasSecondaryTex"]?.SetValue(hasNoiseBound);
            shader.Parameters["uSecondaryTexScale"]?.SetValue(noiseScale);
            shader.Parameters["uSecondaryTexScroll"]?.SetValue(noiseScroll);

            shader.CurrentTechnique = shader.Techniques[technique];
            shader.CurrentTechnique.Passes[0].Apply();
            return true;
        }

        /// <summary>
        /// Fallback trail rendering using the shared ScrollingTrailShader.
        /// </summary>
        public static void ApplyFallbackTrail(float time, Color primary, Color secondary,
            float scrollSpeed = 1.0f, float overbrightMult = 2.5f)
        {
            Effect shader = ShaderLoader.ScrollingTrail;
            if (shader == null)
                shader = ShaderLoader.Trail;
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

        /// <summary>
        /// Begins a SpriteBatch in Immediate + Additive mode for shader drawing.
        /// </summary>
        public static void BeginShaderAdditive(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(
                SpriteSortMode.Immediate,
                MagnumBlendStates.ShaderAdditive,
                SamplerState.LinearClamp,
                DepthStencilState.None,
                Main.Rasterizer,
                null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Restores the SpriteBatch to normal deferred alpha-blend mode.
        /// </summary>
        public static void RestoreSpriteBatch(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                Main.Rasterizer,
                null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Begins a SpriteBatch in Deferred + Additive mode (no shader, for bloom stacking).
        /// </summary>
        public static void BeginAdditive(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(
                SpriteSortMode.Deferred,
                MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp,
                DepthStencilState.None,
                Main.Rasterizer,
                null,
                Main.GameViewMatrix.TransformationMatrix);
        }
    }
}
