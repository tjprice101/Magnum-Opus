using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using MagnumOpus.Common.Systems.Shaders;

namespace MagnumOpus.Content.LaCampanella
{
    /// <summary>
    /// Compartmentalized shader manager for all La Campanella VFX.
    /// Wraps ShaderLoader access to HeroicFlameTrail.fx, RadialScrollShader.fx,
    /// and ScrollingTrailShader.fx — providing preset Apply* methods for each
    /// weapon's infernal fire / bell chime rendering technique.
    ///
    /// Techniques available:
    ///   HeroicFlameTrail  — HeroicFlameFlow: infernal fire trail with smoke distortion
    ///   RadialScrollShader — bell ring shockwave / expanding concentric rings
    ///   ScrollingTrailShader — amber-gold scrolling trail for general weapon use
    ///
    /// Usage (in PreDraw):
    ///   LaCampanellaShaderManager.BindSmokeNoiseTexture(device);
    ///   LaCampanellaShaderManager.ApplyInfernalFlameTrail(time, primary, secondary);
    ///   // ... draw trail geometry ...
    ///   LaCampanellaShaderManager.RestoreSpriteBatch(sb);
    /// </summary>
    public static class LaCampanellaShaderManager
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

        /// <summary>True if any La Campanella shader is available.</summary>
        public static bool IsAvailable => HasFlameTrail || HasRadialScroll || HasScrollingTrail;

        // =====================================================================
        //  Noise Texture Binding
        // =====================================================================

        /// <summary>
        /// Binds NoiseSmoke texture to sampler slot 1 for smoky fire distortion.
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
        /// Binds CosmicEnergyVortex to sampler slot 1 for intense fire vortex effects.
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
        /// Configure and apply HeroicFlameTrail.fx for infernal fire trail rendering.
        /// Uses flowing orange-to-gold gradient with smoke noise distortion.
        /// </summary>
        public static void ApplyInfernalFlameTrail(float time, Color primary, Color secondary,
            float scrollSpeed = 1.5f, float distortionAmt = 0.08f, float overbrightMult = 3f)
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
        /// for richer fire distortion effects.
        /// </summary>
        public static void ApplyInfernalFlameTrailWithNoise(float time, Color primary, Color secondary,
            float scrollSpeed = 1.5f, float distortionAmt = 0.08f, float overbrightMult = 3f,
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
        /// Preset: DualFatedChime trail — infernal waltz fire dance with strong distortion.
        /// </summary>
        public static void ApplyDualFatedChimeTrail(float time)
        {
            ApplyInfernalFlameTrailWithNoise(time,
                LaCampanellaPalette.DeepEmber, LaCampanellaPalette.FlameYellow,
                scrollSpeed: 1.8f, distortionAmt: 0.10f, overbrightMult: 3.2f);
        }

        /// <summary>
        /// Preset: FangOfTheInfiniteBell trail — sharp, fast, bronze-bell metallic cut.
        /// </summary>
        public static void ApplyFangTrail(float time)
        {
            ApplyInfernalFlameTrail(time,
                LaCampanellaPalette.BellBronze, LaCampanellaPalette.BellGold,
                scrollSpeed: 2.5f, distortionAmt: 0.04f, overbrightMult: 3.5f);
        }

        /// <summary>
        /// Preset: IgnitionOfTheBell trail — explosive ignition with volcanic distortion.
        /// </summary>
        public static void ApplyIgnitionTrail(float time)
        {
            ApplyInfernalFlameTrailWithNoise(time,
                LaCampanellaPalette.EmberRed, LaCampanellaPalette.WhiteHot,
                scrollSpeed: 1.0f, distortionAmt: 0.12f, overbrightMult: 4.0f,
                noiseScale: 2.5f, noiseScroll: 0.7f);
        }

        /// <summary>
        /// Preset: InfernalBellMinion trail — smoldering fire spirit with moderate smoke.
        /// </summary>
        public static void ApplyInfernalMinionTrail(float time)
        {
            ApplyInfernalFlameTrailWithNoise(time,
                LaCampanellaPalette.SootBlack, LaCampanellaPalette.InfernalOrange,
                scrollSpeed: 1.2f, distortionAmt: 0.09f, overbrightMult: 2.8f,
                noiseScale: 3f, noiseScroll: 0.6f);
        }

        /// <summary>
        /// Preset: InfernalChimesCalling beam — chiming fire projectile stream.
        /// </summary>
        public static void ApplyInfernalChimesTrail(float time)
        {
            ApplyInfernalFlameTrailWithNoise(time,
                LaCampanellaPalette.DeepEmber, LaCampanellaPalette.ChimeShimmer,
                scrollSpeed: 2.0f, distortionAmt: 0.06f, overbrightMult: 3.0f);
        }

        /// <summary>
        /// Preset: LaCampanellaRanger comet trail — fast fire comet with heavy smoky tail.
        /// </summary>
        public static void ApplyRangerCometTrail(float time)
        {
            ApplyInfernalFlameTrailWithNoise(time,
                LaCampanellaPalette.EmberRed, LaCampanellaPalette.MoltenCore,
                scrollSpeed: 0.8f, distortionAmt: 0.10f, overbrightMult: 3.5f,
                noiseScale: 2f, noiseScroll: 0.4f);
        }

        /// <summary>
        /// Preset: Boss bell chime shockwave — massive expanding ring with intense bloom.
        /// </summary>
        public static void ApplyBossChimeShockwave(float time)
        {
            Effect shader = ShaderLoader.RadialScroll;
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(LaCampanellaPalette.BellGold.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(LaCampanellaPalette.WhiteHot.ToVector3());
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
