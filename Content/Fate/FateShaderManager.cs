using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using MagnumOpus.Common.Systems.Shaders;

namespace MagnumOpus.Content.Fate
{
    /// <summary>
    /// Compartmentalized shader manager for all Fate VFX.
    /// Wraps ShaderLoader access to FateCosmicTrail.fx and FateDestinyBloom.fx  E
    /// providing preset Apply* methods for each rendering technique.
    ///
    /// Techniques available:
    ///   FateCosmicTrail   ECosmicSwirl: nebula-like cosmic trail with star particle noise
    ///   FateDestinyBloom  EDestinyFlare: supernova-intensity bloom with chromatic edge
    ///                     ECosmicFire: celestial flame for burning cosmic effects
    ///
    /// Until Fate-specific shaders are authored (Phase 2), this manager
    /// falls back to existing generic shaders (ScrollingTrailShader,
    /// CelestialValorTrail) with Fate-tuned parameters.
    ///
    /// Usage (in PreDraw):
    ///   FateShaderManager.BindNoiseTexture(device);
    ///   FateShaderManager.ApplyCosmicTrail(time, primary, secondary);
    ///   // ... draw trail geometry ...
    ///   FateShaderManager.RestoreSpriteBatch(sb);
    /// </summary>
    public static class FateShaderManager
    {
        // =====================================================================
        //  Shader Availability
        // =====================================================================

        /// <summary>True if the ScrollingTrailShader loaded (used for cosmic trails).</summary>
        public static bool HasCosmicTrail => ShaderLoader.HasShader(ShaderLoader.ScrollingTrailShader);

        /// <summary>True if the CelestialValorTrail shader loaded (used for celestial trails).</summary>
        public static bool HasCelestialTrail => ShaderLoader.HasShader(ShaderLoader.CelestialValorTrailShader);

        /// <summary>True if any usable shader is available for Fate VFX.</summary>
        public static bool IsAvailable => HasCosmicTrail || HasCelestialTrail;

        // =====================================================================
        //  Noise Texture Binding
        // =====================================================================

        /// <summary>
        /// Binds CosmicNebulaClouds to sampler slot 1 for nebula distortion.
        /// Call once per frame before any Apply* method.
        /// </summary>
        public static void BindNoiseTexture(GraphicsDevice device)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("CosmicNebulaClouds");
            if (noise == null)
                noise = ShaderLoader.GetNoiseTexture("CosmicEnergyVortex");
            if (noise == null)
                noise = ShaderLoader.GetNoiseTexture("PerlinNoise");
            if (noise != null)
            {
                device.Textures[1] = noise;
                device.SamplerStates[1] = SamplerState.LinearWrap;
            }
        }

        /// <summary>
        /// Binds CosmicEnergyVortex to sampler slot 1 for swirling vortex effects.
        /// Falls back to CosmicNebulaClouds if unavailable.
        /// </summary>
        public static void BindVortexNoiseTexture(GraphicsDevice device)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("CosmicEnergyVortex");
            if (noise == null)
                noise = ShaderLoader.GetNoiseTexture("CosmicNebulaClouds");
            if (noise == null)
                noise = ShaderLoader.GetNoiseTexture("PerlinNoise");
            if (noise != null)
            {
                device.Textures[1] = noise;
                device.SamplerStates[1] = SamplerState.LinearWrap;
            }
        }

        /// <summary>
        /// Binds StarFieldScatter to sampler slot 1 for constellation effects.
        /// Falls back to SimplexNoise if unavailable.
        /// </summary>
        public static void BindStarFieldTexture(GraphicsDevice device)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("StarFieldScatter");
            if (noise == null)
                noise = ShaderLoader.GetNoiseTexture("SimplexNoise");
            if (noise == null)
                noise = ShaderLoader.GetNoiseTexture("PerlinNoise");
            if (noise != null)
            {
                device.Textures[1] = noise;
                device.SamplerStates[1] = SamplerState.LinearWrap;
            }
        }

        // =====================================================================
        //  TECHNIQUE: CosmicSwirl (via ScrollingTrailShader)
        // =====================================================================

        /// <summary>
        /// Configure and apply ScrollingTrailShader for cosmic nebula trail rendering.
        /// Uses flowing crimson-to-gold gradient with cosmic distortion.
        /// </summary>
        public static void ApplyCosmicTrail(float time, Color primary, Color secondary,
            float scrollSpeed = 1.2f, float distortionAmt = 0.10f, float overbrightMult = 3.0f)
        {
            Effect shader = ShaderLoader.ScrollingTrail;
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

            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>
        /// Configure ScrollingTrailShader with noise texture on sampler 1
        /// for richer cosmic distortion effects.
        /// </summary>
        public static void ApplyCosmicTrailWithNoise(float time, Color primary, Color secondary,
            float scrollSpeed = 1.2f, float distortionAmt = 0.10f, float overbrightMult = 3.0f,
            float noiseScale = 3f, float noiseScroll = 0.5f)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("CosmicNebulaClouds");
            if (noise == null)
                noise = ShaderLoader.GetNoiseTexture("CosmicEnergyVortex");
            if (noise != null)
            {
                Main.graphics.GraphicsDevice.Textures[1] = noise;
                Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;
            }

            Effect shader = ShaderLoader.ScrollingTrail;
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

            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>
        /// Preset: CodaOfAnnihilation trail  Eannihilation incarnate, reality-ending sweep.
        /// </summary>
        public static void ApplyCodaTrail(float time)
        {
            ApplyCosmicTrailWithNoise(time,
                FatePalette.BrightCrimson, FatePalette.StarGold,
                scrollSpeed: 1.8f, distortionAmt: 0.14f, overbrightMult: 3.5f);
        }

        /// <summary>
        /// Preset: DestinysCrescendo trail  Ebuilding cosmic power, crescendo momentum.
        /// </summary>
        public static void ApplyCrescendoTrail(float time)
        {
            ApplyCosmicTrailWithNoise(time,
                FatePalette.DarkPink, FatePalette.BrightCrimson,
                scrollSpeed: 1.0f, distortionAmt: 0.08f, overbrightMult: 2.8f,
                noiseScale: 2.5f, noiseScroll: 0.6f);
        }

        /// <summary>
        /// Preset: FractalOfTheStars trail  Erecursive starlight, kaleidoscopic cosmos.
        /// </summary>
        public static void ApplyFractalTrail(float time)
        {
            ApplyCosmicTrailWithNoise(time,
                FatePalette.FateCyan, FatePalette.StarGold,
                scrollSpeed: 1.4f, distortionAmt: 0.12f, overbrightMult: 3.0f,
                noiseScale: 4f, noiseScroll: 0.8f);
        }

        /// <summary>
        /// Preset: LightOfTheFuture trail  Edawning golden light, destiny revealed.
        /// </summary>
        public static void ApplyFutureLightTrail(float time)
        {
            ApplyCosmicTrail(time,
                FatePalette.StarGold, FatePalette.SupernovaWhite,
                scrollSpeed: 2.0f, distortionAmt: 0.05f, overbrightMult: 3.5f);
        }

        /// <summary>
        /// Preset: OpusUltima trail  Ethe magnum opus, ultimate cosmic power.
        /// </summary>
        public static void ApplyOpusUltimaTrail(float time)
        {
            ApplyCosmicTrailWithNoise(time,
                FatePalette.BrightCrimson, FatePalette.SupernovaWhite,
                scrollSpeed: 1.6f, distortionAmt: 0.15f, overbrightMult: 4.0f,
                noiseScale: 3f, noiseScroll: 0.5f);
        }

        /// <summary>
        /// Preset: RequiemOfReality trail  Ereality's funeral, cosmic entropy.
        /// </summary>
        public static void ApplyRequiemTrail(float time)
        {
            ApplyCosmicTrailWithNoise(time,
                FatePalette.FatePurple, FatePalette.BrightCrimson,
                scrollSpeed: 0.8f, distortionAmt: 0.10f, overbrightMult: 2.5f,
                noiseScale: 2f, noiseScroll: 0.4f);
        }

        /// <summary>
        /// Preset: ResonanceOfABygoneReality trail  Eghost frequencies, phantom echoes.
        /// </summary>
        public static void ApplyBygoneResonanceTrail(float time)
        {
            ApplyCosmicTrailWithNoise(time,
                FatePalette.NebulaMist, FatePalette.CosmicRose,
                scrollSpeed: 0.6f, distortionAmt: 0.06f, overbrightMult: 2.2f,
                noiseScale: 2.5f, noiseScroll: 0.3f);
        }

        /// <summary>
        /// Preset: SymphonysEnd trail  Ethe final note, cosmic silence.
        /// </summary>
        public static void ApplySymphonyEndTrail(float time)
        {
            ApplyCosmicTrailWithNoise(time,
                FatePalette.DarkPink, FatePalette.StellarCore,
                scrollSpeed: 1.2f, distortionAmt: 0.10f, overbrightMult: 3.2f,
                noiseScale: 3f, noiseScroll: 0.5f);
        }

        /// <summary>
        /// Preset: TheConductorsLastConstellation trail  Estar map traced across the sky.
        /// </summary>
        public static void ApplyConstellationTrail(float time)
        {
            ApplyCosmicTrailWithNoise(time,
                FatePalette.ConstellationSilver, FatePalette.StarGold,
                scrollSpeed: 1.0f, distortionAmt: 0.08f, overbrightMult: 3.0f,
                noiseScale: 4f, noiseScroll: 0.4f);
        }

        /// <summary>
        /// Preset: TheFinalFermata trail  Esuspended time, the held note.
        /// </summary>
        public static void ApplyFermataTrail(float time)
        {
            ApplyCosmicTrail(time,
                FatePalette.CosmicRose, FatePalette.StarGold,
                scrollSpeed: 0.4f, distortionAmt: 0.04f, overbrightMult: 2.8f);
        }

        // =====================================================================
        //  TECHNIQUE: CelestialValor (via CelestialValorTrail shader)
        // =====================================================================

        /// <summary>
        /// Apply the CelestialValorTrail shader tuned for Fate cosmic effects.
        /// </summary>
        public static void ApplyCelestialTrail(float time, Color primary, Color secondary,
            float scrollSpeed = 1.0f, float overbrightMult = 3.0f)
        {
            Effect shader = ShaderLoader.CelestialValorTrail;
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
