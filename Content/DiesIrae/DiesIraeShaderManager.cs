using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using MagnumOpus.Common.Systems.Shaders;

namespace MagnumOpus.Content.DiesIrae
{
    /// <summary>
    /// Compartmentalized shader manager for all Dies Irae VFX.
    /// Wraps ShaderLoader access to HeroicFlameTrail.fx, RadialScrollShader.fx,
    /// and ScrollingTrailShader.fx — providing preset Apply* methods for each
    /// weapon's hellfire / judgment wrath rendering technique.
    ///
    /// Techniques available:
    ///   HeroicFlameTrail  — HeroicFlameFlow: hellfire trail with smoke distortion
    ///   RadialScrollShader — wrath pulse shockwave / expanding concentric rings
    ///   ScrollingTrailShader — blood-gold scrolling trail for general weapon use
    ///
    /// Usage (in PreDraw):
    ///   DiesIraeShaderManager.BindSmokeNoiseTexture(device);
    ///   DiesIraeShaderManager.ApplyHellfireTrail(time, primary, secondary);
    ///   // ... draw trail geometry ...
    ///   DiesIraeShaderManager.RestoreSpriteBatch(sb);
    /// </summary>
    public static class DiesIraeShaderManager
    {
        // =====================================================================
        //  Shader Availability
        // =====================================================================

        /// <summary>True if the HeroicFlameTrail shader loaded successfully.</summary>
        public static bool HasFlameTrail => ShaderLoader.HasShader(ShaderLoader.HeroicFlameTrailShader);

        /// <summary>True if the RadialScroll shader loaded successfully.</summary>
        public static bool HasRadialScroll => ShaderLoader.HasShader(ShaderLoader.RadialScrollShaderName);

        /// <summary>True if the ScrollingTrail shader loaded successfully.</summary>
        public static bool HasScrollingTrail => ShaderLoader.HasShader(ShaderLoader.ScrollingTrailShader);

        /// <summary>True if any Dies Irae shader is available.</summary>
        public static bool IsAvailable => HasFlameTrail || HasRadialScroll || HasScrollingTrail;

        // =====================================================================
        //  Noise Texture Binding
        // =====================================================================

        /// <summary>
        /// Binds NoiseSmoke texture to sampler slot 1 for smoky hellfire distortion.
        /// Call once per frame before any Apply* method.
        /// Falls back to SoftCircularCaustics if NoiseSmoke unavailable.
        /// </summary>
        public static void BindSmokeNoiseTexture(GraphicsDevice device)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("NoiseSmoke");
            if (noise == null)
                noise = ShaderLoader.GetNoiseTexture("SoftCircularCaustics");
            if (noise != null)
            {
                device.Textures[1] = noise;
                device.SamplerStates[1] = SamplerState.LinearWrap;
            }
        }

        /// <summary>
        /// Binds CosmicEnergyVortex to sampler slot 1 for intense hellfire vortex effects.
        /// Falls back to NoiseSmoke if unavailable.
        /// </summary>
        public static void BindVortexNoiseTexture(GraphicsDevice device)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("CosmicEnergyVortex");
            if (noise == null)
                noise = ShaderLoader.GetNoiseTexture("NoiseSmoke");
            if (noise == null)
                noise = ShaderLoader.GetNoiseTexture("SoftCircularCaustics");
            if (noise != null)
            {
                device.Textures[1] = noise;
                device.SamplerStates[1] = SamplerState.LinearWrap;
            }
        }

        // =====================================================================
        //  TECHNIQUE: HeroicFlameFlow (HeroicFlameTrail.fx)
        // =====================================================================

        /// <summary>
        /// Configure and apply HeroicFlameTrail.fx for hellfire trail rendering.
        /// Uses flowing blood-red to judgment-gold gradient with smoke noise distortion.
        /// </summary>
        public static void ApplyHellfireTrail(float time, Color primary, Color secondary,
            float scrollSpeed = 1.2f, float distortionAmt = 0.10f, float overbrightMult = 3f)
        {
            Effect shader = ShaderLoader.HeroicFlameTrail;
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

            shader.CurrentTechnique = shader.Techniques["HeroicFlameFlow"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>
        /// Configure HeroicFlameTrail.fx with smoke noise texture on sampler 1
        /// for richer hellfire distortion effects.
        /// </summary>
        public static void ApplyHellfireTrailWithNoise(float time, Color primary, Color secondary,
            float scrollSpeed = 1.2f, float distortionAmt = 0.10f, float overbrightMult = 3f,
            float noiseScale = 3f, float noiseScroll = 0.5f)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("NoiseSmoke");
            if (noise == null)
                noise = ShaderLoader.GetNoiseTexture("SoftCircularCaustics");
            if (noise != null)
            {
                Main.graphics.GraphicsDevice.Textures[1] = noise;
                Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;
            }

            Effect shader = ShaderLoader.HeroicFlameTrail;
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

            shader.CurrentTechnique = shader.Techniques["HeroicFlameFlow"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        // =====================================================================
        //  WEAPON PRESETS
        // =====================================================================

        /// <summary>
        /// Preset: WrathsCleaver trail — brutal blood-fire slash with heavy distortion.
        /// </summary>
        public static void ApplyWrathsCleaverTrail(float time)
        {
            ApplyHellfireTrailWithNoise(time,
                DiesIraePalette.BloodRed, DiesIraePalette.HellfireGold,
                scrollSpeed: 1.5f, distortionAmt: 0.12f, overbrightMult: 3.2f);
        }

        /// <summary>
        /// Preset: ChainOfJudgment trail — precise, methodical judgment links.
        /// </summary>
        public static void ApplyChainOfJudgmentTrail(float time)
        {
            ApplyHellfireTrail(time,
                DiesIraePalette.InfernalRed, DiesIraePalette.JudgmentGold,
                scrollSpeed: 2.0f, distortionAmt: 0.06f, overbrightMult: 3.0f);
        }

        /// <summary>
        /// Preset: ExecutionersVerdict trail — absolute, guillotine descent.
        /// </summary>
        public static void ApplyExecutionersVerdictTrail(float time)
        {
            ApplyHellfireTrailWithNoise(time,
                DiesIraePalette.DarkBlood, DiesIraePalette.InfernalRed,
                scrollSpeed: 0.8f, distortionAmt: 0.14f, overbrightMult: 4.0f,
                noiseScale: 2.5f, noiseScroll: 0.7f);
        }

        /// <summary>
        /// Preset: SinCollector trail — sharp, precise sin-seeking tracer.
        /// </summary>
        public static void ApplySinCollectorTrail(float time)
        {
            ApplyHellfireTrail(time,
                DiesIraePalette.BloodRed, DiesIraePalette.JudgmentGold,
                scrollSpeed: 3.0f, distortionAmt: 0.03f, overbrightMult: 3.5f);
        }

        /// <summary>
        /// Preset: DamnationsCannon trail — explosive, volcanic damnation shell.
        /// </summary>
        public static void ApplyDamnationsCannonTrail(float time)
        {
            ApplyHellfireTrailWithNoise(time,
                DiesIraePalette.SmolderingEmber, DiesIraePalette.HellfireGold,
                scrollSpeed: 1.0f, distortionAmt: 0.12f, overbrightMult: 3.8f,
                noiseScale: 2f, noiseScroll: 0.5f);
        }

        /// <summary>
        /// Preset: ArbitersSentence trail — sweeping judgment flame stream.
        /// </summary>
        public static void ApplyArbitersSentenceTrail(float time)
        {
            ApplyHellfireTrailWithNoise(time,
                DiesIraePalette.InfernalRed, DiesIraePalette.WrathWhite,
                scrollSpeed: 1.8f, distortionAmt: 0.09f, overbrightMult: 3.0f,
                noiseScale: 3f, noiseScroll: 0.6f);
        }

        /// <summary>
        /// Preset: StaffOfFinalJudgement trail — divine condemnation orbs.
        /// </summary>
        public static void ApplyStaffOfFinalJudgementTrail(float time)
        {
            ApplyHellfireTrailWithNoise(time,
                DiesIraePalette.DoomPurple, DiesIraePalette.JudgmentGold,
                scrollSpeed: 1.2f, distortionAmt: 0.08f, overbrightMult: 3.5f);
        }

        /// <summary>
        /// Preset: EclipseOfWrath trail — orbiting wrath orb tracker.
        /// </summary>
        public static void ApplyEclipseOfWrathTrail(float time)
        {
            ApplyHellfireTrailWithNoise(time,
                DiesIraePalette.BloodRed, DiesIraePalette.InfernalRed,
                scrollSpeed: 1.5f, distortionAmt: 0.10f, overbrightMult: 3.2f,
                noiseScale: 2.5f, noiseScroll: 0.5f);
        }

        /// <summary>
        /// Preset: GrimoireOfCondemnation trail — spiraling condemnation shards.
        /// </summary>
        public static void ApplyGrimoireTrail(float time)
        {
            ApplyHellfireTrail(time,
                DiesIraePalette.InfernalRed, DiesIraePalette.JudgmentGold,
                scrollSpeed: 2.2f, distortionAmt: 0.05f, overbrightMult: 3.0f);
        }

        /// <summary>
        /// Preset: DeathTollingBell minion trail — solemn funeral toll aura.
        /// </summary>
        public static void ApplyDeathTollingBellTrail(float time)
        {
            ApplyHellfireTrailWithNoise(time,
                DiesIraePalette.CharcoalBlack, DiesIraePalette.BloodRed,
                scrollSpeed: 0.8f, distortionAmt: 0.08f, overbrightMult: 2.5f,
                noiseScale: 3f, noiseScroll: 0.4f);
        }

        /// <summary>
        /// Preset: HarmonyOfJudgement minion trail — angelic judgment ray.
        /// </summary>
        public static void ApplyHarmonyOfJudgementTrail(float time)
        {
            ApplyHellfireTrail(time,
                DiesIraePalette.JudgmentGold, DiesIraePalette.WrathWhite,
                scrollSpeed: 2.5f, distortionAmt: 0.04f, overbrightMult: 4.0f);
        }

        /// <summary>
        /// Preset: WrathfulContract minion trail — contracted wrath aura.
        /// </summary>
        public static void ApplyWrathfulContractTrail(float time)
        {
            ApplyHellfireTrailWithNoise(time,
                DiesIraePalette.BloodRed, DiesIraePalette.HellfireGold,
                scrollSpeed: 1.3f, distortionAmt: 0.10f, overbrightMult: 3.0f);
        }

        /// <summary>
        /// Preset: Boss wrath shockwave — massive expanding ring with intense bloom.
        /// </summary>
        public static void ApplyBossWrathShockwave(float time)
        {
            Effect shader = ShaderLoader.RadialScroll;
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(DiesIraePalette.InfernalRed.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(DiesIraePalette.WrathWhite.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(2f);
            shader.Parameters["uOverbrightMult"]?.SetValue(4f);
            shader.Parameters["uScrollSpeed"]?.SetValue(3f);

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
