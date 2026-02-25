using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using MagnumOpus.Common.Systems.Shaders;

namespace MagnumOpus.Content.MoonlightSonata
{
    /// <summary>
    /// Compartmentalized shader manager for all Moonlight Sonata VFX.
    /// Wraps ShaderLoader access to MoonlightTrail.fx, LunarBeam.fx,
    /// and CrescentAura.fx — providing preset Apply* methods for each
    /// rendering technique.
    ///
    /// Techniques available:
    ///   MoonlightTrail  — MoonlightFlowTrail: flowing lunar trail with noise distortion
    ///   LunarBeam       — CrescentBeam: crescent-shaped beam body rendering
    ///   CrescentAura    — CrescentShape / CrescentPulse: procedural crescent overlays
    ///
    /// Usage (in PreDraw):
    ///   MoonlightSonataShaderManager.BindNoiseTexture(device);
    ///   MoonlightSonataShaderManager.ApplyMoonlightTrail(time, primary, secondary);
    ///   // ... draw trail geometry ...
    ///   MoonlightSonataShaderManager.RestoreSpriteBatch(sb);
    /// </summary>
    public static class MoonlightSonataShaderManager
    {
        // =====================================================================
        //  Shader Availability
        // =====================================================================

        /// <summary>True if the MoonlightTrail shader loaded successfully.</summary>
        public static bool HasMoonlightTrail => ShaderLoader.HasShader(ShaderLoader.MoonlightTrailShader);

        /// <summary>True if the LunarBeam shader loaded successfully.</summary>
        public static bool HasLunarBeam => ShaderLoader.HasShader(ShaderLoader.LunarBeamShader);

        /// <summary>True if the CrescentAura shader loaded successfully.</summary>
        public static bool HasCrescentAura => ShaderLoader.HasShader(ShaderLoader.CrescentAuraShader);

        /// <summary>True if any Moonlight shader is available.</summary>
        public static bool IsAvailable => HasMoonlightTrail || HasLunarBeam || HasCrescentAura;

        // =====================================================================
        //  Noise Texture Binding
        // =====================================================================

        /// <summary>
        /// Binds SoftCircularCaustics noise to sampler slot 1 for trail distortion.
        /// Call once per frame before any Apply* method.
        /// </summary>
        public static void BindNoiseTexture(GraphicsDevice device)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("SoftCircularCaustics");
            if (noise != null)
            {
                device.Textures[1] = noise;
                device.SamplerStates[1] = SamplerState.LinearWrap;
            }
        }

        /// <summary>
        /// Binds NebulaWispNoise to sampler slot 1 for cosmic/void effects.
        /// Falls back to SoftCircularCaustics if unavailable.
        /// </summary>
        public static void BindCosmicNoiseTexture(GraphicsDevice device)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("NebulaWispNoise");
            if (noise == null)
                noise = ShaderLoader.GetNoiseTexture("SoftCircularCaustics");
            if (noise != null)
            {
                device.Textures[1] = noise;
                device.SamplerStates[1] = SamplerState.LinearWrap;
            }
        }

        // =====================================================================
        //  TECHNIQUE: MoonlightFlowTrail (MoonlightTrail.fx)
        // =====================================================================

        /// <summary>
        /// Configure and apply MoonlightTrail.fx for standard lunar trail rendering.
        /// Uses flowing purple-to-blue gradient with optional noise distortion.
        /// </summary>
        public static void ApplyMoonlightTrail(float time, Color primary, Color secondary,
            float scrollSpeed = 1.0f, float distortionAmt = 0.06f, float overbrightMult = 2.5f)
        {
            Effect shader = ShaderLoader.MoonlightTrail;
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(1.5f);
            shader.Parameters["uOverbrightMult"]?.SetValue(overbrightMult);
            shader.Parameters["uScrollSpeed"]?.SetValue(scrollSpeed);
            shader.Parameters["uDistortionAmt"]?.SetValue(distortionAmt);
            shader.Parameters["uHasSecondaryTex"]?.SetValue(0f);
            shader.Parameters["uSecondaryTexScale"]?.SetValue(3f);
            shader.Parameters["uSecondaryTexScroll"]?.SetValue(0.5f);

            shader.CurrentTechnique = shader.Techniques["MoonlightFlowTrail"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>
        /// Configure MoonlightTrail.fx with noise texture on sampler 1
        /// for richer distortion effects.
        /// </summary>
        public static void ApplyMoonlightTrailWithNoise(float time, Color primary, Color secondary,
            float scrollSpeed = 1.0f, float distortionAmt = 0.06f, float overbrightMult = 2.5f,
            float noiseScale = 3f, float noiseScroll = 0.5f)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("SoftCircularCaustics");
            if (noise != null)
            {
                Main.graphics.GraphicsDevice.Textures[1] = noise;
                Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;
            }

            Effect shader = ShaderLoader.MoonlightTrail;
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(1.5f);
            shader.Parameters["uOverbrightMult"]?.SetValue(overbrightMult);
            shader.Parameters["uScrollSpeed"]?.SetValue(scrollSpeed);
            shader.Parameters["uDistortionAmt"]?.SetValue(distortionAmt);
            shader.Parameters["uHasSecondaryTex"]?.SetValue(noise != null ? 1f : 0f);
            shader.Parameters["uSecondaryTexScale"]?.SetValue(noiseScale);
            shader.Parameters["uSecondaryTexScroll"]?.SetValue(noiseScroll);

            shader.CurrentTechnique = shader.Techniques["MoonlightFlowTrail"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>
        /// Preset: EternalMoon trail — wide crescent sweep with moderate distortion.
        /// </summary>
        public static void ApplyEternalMoonTrail(float time)
        {
            ApplyMoonlightTrailWithNoise(time,
                MoonlightSonataPalette.DarkPurple, MoonlightSonataPalette.IceBlue,
                scrollSpeed: 1.2f, distortionAmt: 0.08f, overbrightMult: 2.8f);
        }

        /// <summary>
        /// Preset: Incisor trail — sharp, fast, minimal distortion for precision cuts.
        /// </summary>
        public static void ApplyIncisorTrail(float time)
        {
            ApplyMoonlightTrail(time,
                MoonlightSonataPalette.GravityWell, MoonlightSonataPalette.IceBlue,
                scrollSpeed: 2.0f, distortionAmt: 0.03f, overbrightMult: 3.0f);
        }

        /// <summary>
        /// Preset: Resurrection comet trail — heavy, impactful, strong distortion.
        /// </summary>
        public static void ApplyResurrectionTrail(float time)
        {
            ApplyMoonlightTrailWithNoise(time,
                MoonlightSonataPalette.NebulaPurple, MoonlightSonataPalette.CrescentGold,
                scrollSpeed: 0.8f, distortionAmt: 0.10f, overbrightMult: 3.5f,
                noiseScale: 2.5f, noiseScroll: 0.7f);
        }

        /// <summary>
        /// Preset: Goliath cosmic beam trail — massive, gravitational distortion.
        /// </summary>
        public static void ApplyGoliathTrail(float time)
        {
            ApplyMoonlightTrailWithNoise(time,
                MoonlightSonataPalette.CosmicVoid, MoonlightSonataPalette.StarCore,
                scrollSpeed: 0.6f, distortionAmt: 0.12f, overbrightMult: 4.0f,
                noiseScale: 2f, noiseScroll: 0.4f);
        }

        // =====================================================================
        //  TECHNIQUE: CrescentBeam (LunarBeam.fx)
        // =====================================================================

        /// <summary>
        /// Configure and apply LunarBeam.fx for crescent-shaped beam rendering.
        /// Phase controls crescent fullness (0 = new moon, 1 = full moon).
        /// </summary>
        public static void ApplyLunarBeam(float time, float phase,
            Color primary, Color secondary,
            float scrollSpeed = 1.5f, float overbrightMult = 3f)
        {
            Effect shader = ShaderLoader.LunarBeam;
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(1.5f);
            shader.Parameters["uOverbrightMult"]?.SetValue(overbrightMult);
            shader.Parameters["uPhase"]?.SetValue(MathHelper.Clamp(phase, 0f, 1f));
            shader.Parameters["uScrollSpeed"]?.SetValue(scrollSpeed);
            shader.Parameters["uHasSecondaryTex"]?.SetValue(0f);
            shader.Parameters["uSecondaryTexScale"]?.SetValue(2f);
            shader.Parameters["uSecondaryTexScroll"]?.SetValue(0.8f);

            shader.CurrentTechnique = shader.Techniques["CrescentBeam"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>
        /// Preset: MoonlightsCalling prismatic beam — bouncing refraction beam.
        /// </summary>
        public static void ApplyMoonlightsCallingBeam(float time, float bouncePhase)
        {
            ApplyLunarBeam(time, bouncePhase,
                MoonlightSonataPalette.PrismViolet, MoonlightSonataPalette.RefractedBlue,
                scrollSpeed: 2.0f, overbrightMult: 3.5f);
        }

        /// <summary>
        /// Preset: WaningDeer lunar beam sweep — enemy attack beam.
        /// </summary>
        public static void ApplyWaningDeerBeam(float time, float sweepProgress)
        {
            ApplyLunarBeam(time, sweepProgress,
                MoonlightSonataPalette.DarkPurple, MoonlightSonataPalette.IceBlue,
                scrollSpeed: 1.0f, overbrightMult: 2.5f);
        }

        // =====================================================================
        //  TECHNIQUE: CrescentShape / CrescentPulse (CrescentAura.fx)
        // =====================================================================

        /// <summary>
        /// Configure and apply CrescentAura.fx for crescent moon overlays.
        /// Set pulsing=true for rhythmic pulse variant.
        /// </summary>
        public static void ApplyCrescentAura(float time, float phase,
            Color primary, Color secondary,
            float overbrightMult = 2f, bool pulsing = false)
        {
            Effect shader = ShaderLoader.CrescentAura;
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(1.5f);
            shader.Parameters["uOverbrightMult"]?.SetValue(overbrightMult);
            shader.Parameters["uPhase"]?.SetValue(MathHelper.Clamp(phase, 0f, 1f));
            shader.Parameters["uSharpness"]?.SetValue(2.5f);

            string technique = pulsing ? "CrescentPulse" : "CrescentShape";
            shader.CurrentTechnique = shader.Techniques[technique];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>
        /// Preset: StaffOfTheLunarPhases crescent — phase-cycling aura for conductor's baton.
        /// </summary>
        public static void ApplyLunarPhaseAura(float time, float lunarPhase)
        {
            ApplyCrescentAura(time, lunarPhase,
                MoonlightSonataPalette.Violet, MoonlightSonataPalette.CrescentGold,
                overbrightMult: 2.5f, pulsing: true);
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
                BlendState.Additive,
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
                BlendState.Additive,
                SamplerState.LinearClamp,
                DepthStencilState.None,
                Main.Rasterizer,
                null,
                Main.GameViewMatrix.TransformationMatrix);
        }
    }
}
