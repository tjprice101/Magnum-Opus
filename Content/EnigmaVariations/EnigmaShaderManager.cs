using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using MagnumOpus.Common.Systems.Shaders;

namespace MagnumOpus.Content.EnigmaVariations
{
    /// <summary>
    /// Compartmentalized shader manager for all Enigma Variations VFX.
    /// Wraps ShaderLoader access to EnigmaVoidTrail.fx and EnigmaFlame.fx —
    /// providing preset Apply* methods for each rendering technique.
    ///
    /// Techniques available:
    ///   EnigmaVoidTrail  — VoidSwirl: swirling void trail with purple-green distortion
    ///   EnigmaFlame      — EerieFlame: eerie green flame with flickering, unnatural movement
    ///                    — VoidFlicker: void-tinted flame for darker effects
    ///
    /// Usage (in PreDraw):
    ///   EnigmaShaderManager.BindNoiseTexture(device);
    ///   EnigmaShaderManager.ApplyVoidTrail(time, primary, secondary);
    ///   // ... draw trail geometry ...
    ///   EnigmaShaderManager.RestoreSpriteBatch(sb);
    /// </summary>
    public static class EnigmaShaderManager
    {
        // =====================================================================
        //  Shader Availability
        // =====================================================================

        /// <summary>True if the EnigmaVoidTrail shader loaded successfully.</summary>
        public static bool HasVoidTrail => ShaderLoader.HasShader(ShaderLoader.EnigmaVoidTrailShader);

        /// <summary>True if the EnigmaFlame shader loaded successfully.</summary>
        public static bool HasEnigmaFlame => ShaderLoader.HasShader(ShaderLoader.EnigmaFlameShader);

        /// <summary>True if any Enigma shader is available.</summary>
        public static bool IsAvailable => HasVoidTrail || HasEnigmaFlame;

        // =====================================================================
        //  Noise Texture Binding
        // =====================================================================

        /// <summary>
        /// Binds NebulaWispNoise to sampler slot 1 for void distortion.
        /// Call once per frame before any Apply* method.
        /// </summary>
        public static void BindNoiseTexture(GraphicsDevice device)
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

        /// <summary>
        /// Binds CosmicEnergyVortex to sampler slot 1 for swirling vortex effects.
        /// Falls back to NebulaWispNoise if unavailable.
        /// </summary>
        public static void BindVortexNoiseTexture(GraphicsDevice device)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("CosmicEnergyVortex");
            if (noise == null)
                noise = ShaderLoader.GetNoiseTexture("NebulaWispNoise");
            if (noise == null)
                noise = ShaderLoader.GetNoiseTexture("SoftCircularCaustics");
            if (noise != null)
            {
                device.Textures[1] = noise;
                device.SamplerStates[1] = SamplerState.LinearWrap;
            }
        }

        // =====================================================================
        //  TECHNIQUE: VoidSwirl (EnigmaVoidTrail.fx)
        // =====================================================================

        /// <summary>
        /// Configure and apply EnigmaVoidTrail.fx for standard void trail rendering.
        /// Uses swirling purple-to-green gradient with void distortion.
        /// </summary>
        public static void ApplyVoidTrail(float time, Color primary, Color secondary,
            float scrollSpeed = 1.0f, float distortionAmt = 0.08f, float overbrightMult = 2.5f)
        {
            Effect shader = ShaderLoader.EnigmaVoidTrail;
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

            shader.CurrentTechnique = shader.Techniques["VoidSwirl"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>
        /// Configure EnigmaVoidTrail.fx with noise texture on sampler 1
        /// for richer distortion effects.
        /// </summary>
        public static void ApplyVoidTrailWithNoise(float time, Color primary, Color secondary,
            float scrollSpeed = 1.0f, float distortionAmt = 0.08f, float overbrightMult = 2.5f,
            float noiseScale = 3f, float noiseScroll = 0.5f)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("NebulaWispNoise");
            if (noise == null)
                noise = ShaderLoader.GetNoiseTexture("SoftCircularCaustics");
            if (noise != null)
            {
                Main.graphics.GraphicsDevice.Textures[1] = noise;
                Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;
            }

            Effect shader = ShaderLoader.EnigmaVoidTrail;
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

            shader.CurrentTechnique = shader.Techniques["VoidSwirl"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>
        /// Preset: CipherNocturne beam trail — channeled reality-unraveling distortion.
        /// </summary>
        public static void ApplyCipherNocturneTrail(float time)
        {
            ApplyVoidTrailWithNoise(time,
                EnigmaPalette.DeepPurple, EnigmaPalette.GreenFlame,
                scrollSpeed: 1.5f, distortionAmt: 0.10f, overbrightMult: 3.0f);
        }

        /// <summary>
        /// Preset: DissonanceOfSecrets orb trail — growing mystery cascade.
        /// </summary>
        public static void ApplyDissonanceTrail(float time)
        {
            ApplyVoidTrailWithNoise(time,
                EnigmaPalette.Purple, EnigmaPalette.EyeGreen,
                scrollSpeed: 0.8f, distortionAmt: 0.12f, overbrightMult: 2.8f,
                noiseScale: 2.5f, noiseScroll: 0.7f);
        }

        /// <summary>
        /// Preset: FugueOfTheUnknown trail — contrapuntal layered void.
        /// </summary>
        public static void ApplyFugueTrail(float time)
        {
            ApplyVoidTrailWithNoise(time,
                EnigmaPalette.GlyphPurple, EnigmaPalette.RiddleShimmer,
                scrollSpeed: 1.2f, distortionAmt: 0.06f, overbrightMult: 2.5f);
        }

        /// <summary>
        /// Preset: TacetsEnigma trail — silent void, minimal distortion.
        /// </summary>
        public static void ApplyTacetTrail(float time)
        {
            ApplyVoidTrail(time,
                EnigmaPalette.CipherDark, EnigmaPalette.VoidFlame,
                scrollSpeed: 0.5f, distortionAmt: 0.04f, overbrightMult: 2.0f);
        }

        /// <summary>
        /// Preset: TheSilentMeasure trail — precise, surgical void cuts.
        /// </summary>
        public static void ApplySilentMeasureTrail(float time)
        {
            ApplyVoidTrail(time,
                EnigmaPalette.DeepPurple, EnigmaPalette.EyeGreen,
                scrollSpeed: 2.0f, distortionAmt: 0.03f, overbrightMult: 3.0f);
        }

        /// <summary>
        /// Preset: TheUnresolvedCadence trail — unstable, shifting tension.
        /// </summary>
        public static void ApplyUnresolvedCadenceTrail(float time)
        {
            ApplyVoidTrailWithNoise(time,
                EnigmaPalette.UnresolvedTension, EnigmaPalette.GreenFlame,
                scrollSpeed: 1.4f, distortionAmt: 0.09f, overbrightMult: 2.8f,
                noiseScale: 2f, noiseScroll: 0.6f);
        }

        /// <summary>
        /// Preset: TheWatchingRefrain trail — omniscient watchful energy.
        /// </summary>
        public static void ApplyWatchingRefrainTrail(float time)
        {
            ApplyVoidTrailWithNoise(time,
                EnigmaPalette.Purple, EnigmaPalette.BrightGreen,
                scrollSpeed: 1.0f, distortionAmt: 0.08f, overbrightMult: 3.2f,
                noiseScale: 3f, noiseScroll: 0.4f);
        }

        /// <summary>
        /// Preset: VariationsOfTheVoid trail — the void itself, deep and absolute.
        /// </summary>
        public static void ApplyVoidVariationsTrail(float time)
        {
            ApplyVoidTrailWithNoise(time,
                EnigmaPalette.VoidBlack, EnigmaPalette.Purple,
                scrollSpeed: 0.6f, distortionAmt: 0.14f, overbrightMult: 2.2f,
                noiseScale: 2f, noiseScroll: 0.3f);
        }

        // =====================================================================
        //  TECHNIQUE: EerieFlame / VoidFlicker (EnigmaFlame.fx)
        // =====================================================================

        /// <summary>
        /// Configure and apply EnigmaFlame.fx for eerie green flame rendering.
        /// </summary>
        public static void ApplyEnigmaFlame(float time, Color primary, Color secondary,
            float flickerSpeed = 1.5f, float overbrightMult = 3f)
        {
            Effect shader = ShaderLoader.EnigmaFlame;
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(1.5f);
            shader.Parameters["uOverbrightMult"]?.SetValue(overbrightMult);
            shader.Parameters["uScrollSpeed"]?.SetValue(flickerSpeed);
            shader.Parameters["uHasSecondaryTex"]?.SetValue(0f);
            shader.Parameters["uSecondaryTexScale"]?.SetValue(2f);
            shader.Parameters["uSecondaryTexScroll"]?.SetValue(0.8f);

            shader.CurrentTechnique = shader.Techniques["EerieFlame"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>
        /// Preset: Standard eerie green flame accent.
        /// </summary>
        public static void ApplyStandardFlame(float time)
        {
            ApplyEnigmaFlame(time,
                EnigmaPalette.GreenFlame, EnigmaPalette.BrightGreen,
                flickerSpeed: 1.5f, overbrightMult: 3f);
        }

        /// <summary>
        /// Preset: Void-tinted dark flame for shadow effects.
        /// </summary>
        public static void ApplyVoidFlame(float time)
        {
            ApplyEnigmaFlame(time,
                EnigmaPalette.VoidFlame, EnigmaPalette.DeepPurple,
                flickerSpeed: 0.8f, overbrightMult: 2f);
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
