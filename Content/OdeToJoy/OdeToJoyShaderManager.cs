using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using MagnumOpus.Common.Systems.Shaders;

namespace MagnumOpus.Content.OdeToJoy
{
    /// <summary>
    /// Compartmentalized shader manager for all Ode to Joy VFX.
    /// Wraps ShaderLoader access, providing preset Apply* methods
    /// for each weapon's rendering technique.
    ///
    /// Until Ode to Joy-specific shaders are authored (Phase 2),
    /// this manager falls back to existing generic shaders
    /// (ScrollingTrailShader, CelestialValorTrail) with garden-tuned parameters.
    ///
    /// Usage (in PreDraw):
    ///   OdeToJoyShaderManager.BindNoiseTexture(device);
    ///   OdeToJoyShaderManager.ApplyGardenTrail(time, primary, secondary);
    ///   // ... draw trail geometry ...
    ///   OdeToJoyShaderManager.RestoreSpriteBatch(sb);
    /// </summary>
    public static class OdeToJoyShaderManager
    {
        // =====================================================================
        //  Shader Availability
        // =====================================================================

        /// <summary>True if the ScrollingTrailShader loaded (used for garden trails).</summary>
        public static bool HasGardenTrail => ShaderLoader.HasShader(ShaderLoader.ScrollingTrailShader);

        /// <summary>True if the CelestialValorTrail shader loaded (used for celestial trails).</summary>
        public static bool HasCelestialTrail => ShaderLoader.HasShader(ShaderLoader.CelestialValorTrailShader);

        /// <summary>True if any usable shader is available for Ode to Joy VFX.</summary>
        public static bool IsAvailable => HasGardenTrail || HasCelestialTrail;

        // =====================================================================
        //  Noise Texture Binding
        // =====================================================================

        /// <summary>
        /// Binds TileableFBMNoise to sampler slot 1 for natural organic distortion.
        /// Call once per frame before any Apply* method.
        /// </summary>
        public static void BindNoiseTexture(GraphicsDevice device)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("TileableFBMNoise");
            if (noise == null)
                noise = ShaderLoader.GetNoiseTexture("PerlinNoise");
            if (noise != null)
            {
                device.Textures[1] = noise;
                device.SamplerStates[1] = SamplerState.LinearWrap;
            }
        }

        /// <summary>
        /// Binds MusicalWavePattern to sampler slot 1 for musical wave effects.
        /// Falls back to PerlinNoise if unavailable.
        /// </summary>
        public static void BindMusicalNoiseTexture(GraphicsDevice device)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("MusicalWavePattern");
            if (noise == null)
                noise = ShaderLoader.GetNoiseTexture("TileableFBMNoise");
            if (noise == null)
                noise = ShaderLoader.GetNoiseTexture("PerlinNoise");
            if (noise != null)
            {
                device.Textures[1] = noise;
                device.SamplerStates[1] = SamplerState.LinearWrap;
            }
        }

        /// <summary>
        /// Binds SparklyNoiseTexture to sampler slot 1 for golden pollen sparkle effects.
        /// Falls back to PerlinNoise if unavailable.
        /// </summary>
        public static void BindSparkleNoiseTexture(GraphicsDevice device)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("SparklyNoiseTexture");
            if (noise == null)
                noise = ShaderLoader.GetNoiseTexture("PerlinNoise");
            if (noise != null)
            {
                device.Textures[1] = noise;
                device.SamplerStates[1] = SamplerState.LinearWrap;
            }
        }

        // =====================================================================
        //  TECHNIQUE: GardenTrail (via ScrollingTrailShader)
        // =====================================================================

        /// <summary>
        /// Configure and apply ScrollingTrailShader for garden trail rendering.
        /// Uses flowing green-to-gold gradient with organic distortion.
        /// </summary>
        public static void ApplyGardenTrail(float time, Color primary, Color secondary,
            float scrollSpeed = 0.8f, float distortionAmt = 0.06f, float overbrightMult = 2.5f)
        {
            Effect shader = ShaderLoader.ScrollingTrail;
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(1.3f);
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
        /// for richer organic garden distortion effects.
        /// </summary>
        public static void ApplyGardenTrailWithNoise(float time, Color primary, Color secondary,
            float scrollSpeed = 0.8f, float distortionAmt = 0.06f, float overbrightMult = 2.5f,
            float noiseScale = 3f, float noiseScroll = 0.4f)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("TileableFBMNoise");
            if (noise == null)
                noise = ShaderLoader.GetNoiseTexture("PerlinNoise");
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
            shader.Parameters["uIntensity"]?.SetValue(1.3f);
            shader.Parameters["uOverbrightMult"]?.SetValue(overbrightMult);
            shader.Parameters["uScrollSpeed"]?.SetValue(scrollSpeed);
            shader.Parameters["uDistortionAmt"]?.SetValue(distortionAmt);
            shader.Parameters["uHasSecondaryTex"]?.SetValue(noise != null ? 1f : 0f);
            shader.Parameters["uSecondaryTexScale"]?.SetValue(noiseScale);
            shader.Parameters["uSecondaryTexScroll"]?.SetValue(noiseScroll);

            shader.CurrentTechnique.Passes[0].Apply();
        }

        // =====================================================================
        //  WEAPON PRESETS — Melee
        // =====================================================================

        /// <summary>
        /// Preset: TheGardenersFury trail — fierce vine slash, nature's wrath.
        /// </summary>
        public static void ApplyGardenersFuryTrail(float time)
        {
            ApplyGardenTrailWithNoise(time,
                OdeToJoyPalette.VerdantGreen, OdeToJoyPalette.GoldenPollen,
                scrollSpeed: 1.2f, distortionAmt: 0.10f, overbrightMult: 2.8f);
        }

        /// <summary>
        /// Preset: ThornboundReckoning trail — thorny aggressive nature energy.
        /// </summary>
        public static void ApplyThornboundReckoningTrail(float time)
        {
            ApplyGardenTrailWithNoise(time,
                OdeToJoyPalette.LeafGreen, OdeToJoyPalette.BudGreen,
                scrollSpeed: 1.4f, distortionAmt: 0.12f, overbrightMult: 3.0f,
                noiseScale: 4f, noiseScroll: 0.6f);
        }

        /// <summary>
        /// Preset: RoseThornChainsaw trail — rapid thorn chain, garden fury.
        /// </summary>
        public static void ApplyRoseThornChainsawTrail(float time)
        {
            ApplyGardenTrailWithNoise(time,
                OdeToJoyPalette.PetalPink, OdeToJoyPalette.VerdantGreen,
                scrollSpeed: 1.8f, distortionAmt: 0.08f, overbrightMult: 2.5f,
                noiseScale: 3f, noiseScroll: 0.7f);
        }

        // =====================================================================
        //  WEAPON PRESETS — Ranged
        // =====================================================================

        /// <summary>
        /// Preset: ThornSprayRepeater trail — rapid thorn spray, shrapnel nature.
        /// </summary>
        public static void ApplyThornSprayTrail(float time)
        {
            ApplyGardenTrail(time,
                OdeToJoyPalette.LeafGreen, OdeToJoyPalette.VerdantGreen,
                scrollSpeed: 1.6f, distortionAmt: 0.04f, overbrightMult: 2.2f);
        }

        /// <summary>
        /// Preset: PetalStormCannon trail — beautiful petal projectile storm.
        /// </summary>
        public static void ApplyPetalStormTrail(float time)
        {
            ApplyGardenTrailWithNoise(time,
                OdeToJoyPalette.RosePink, OdeToJoyPalette.GoldenPollen,
                scrollSpeed: 1.0f, distortionAmt: 0.06f, overbrightMult: 2.8f,
                noiseScale: 2.5f, noiseScroll: 0.5f);
        }

        /// <summary>
        /// Preset: ThePollinator trail — golden pollen spread, warm garden mist.
        /// </summary>
        public static void ApplyPollinatorTrail(float time)
        {
            ApplyGardenTrailWithNoise(time,
                OdeToJoyPalette.GoldenPollen, OdeToJoyPalette.SunlightYellow,
                scrollSpeed: 0.6f, distortionAmt: 0.05f, overbrightMult: 2.5f,
                noiseScale: 2f, noiseScroll: 0.3f);
        }

        // =====================================================================
        //  WEAPON PRESETS — Magic
        // =====================================================================

        /// <summary>
        /// Preset: AnthemOfGlory trail — triumphant golden beam of glory.
        /// </summary>
        public static void ApplyAnthemOfGloryTrail(float time)
        {
            ApplyGardenTrailWithNoise(time,
                OdeToJoyPalette.GoldenPollen, OdeToJoyPalette.WhiteBloom,
                scrollSpeed: 1.4f, distortionAmt: 0.08f, overbrightMult: 3.2f,
                noiseScale: 3f, noiseScroll: 0.5f);
        }

        /// <summary>
        /// Preset: HymnOfTheVictorious trail — sacred garden hymn energy.
        /// </summary>
        public static void ApplyHymnOfTheVictoriousTrail(float time)
        {
            ApplyGardenTrailWithNoise(time,
                OdeToJoyPalette.VerdantGreen, OdeToJoyPalette.WhiteBloom,
                scrollSpeed: 0.8f, distortionAmt: 0.06f, overbrightMult: 2.8f,
                noiseScale: 2.5f, noiseScroll: 0.4f);
        }

        /// <summary>
        /// Preset: ElysianVerdict trail — divine judgment bloom, supreme radiance.
        /// </summary>
        public static void ApplyElysianVerdictTrail(float time)
        {
            ApplyGardenTrailWithNoise(time,
                OdeToJoyPalette.WarmAmber, OdeToJoyPalette.WhiteBloom,
                scrollSpeed: 1.6f, distortionAmt: 0.10f, overbrightMult: 3.5f,
                noiseScale: 3f, noiseScroll: 0.6f);
        }

        // =====================================================================
        //  WEAPON PRESETS — Summon
        // =====================================================================

        /// <summary>
        /// Preset: TriumphantChorus summon trail — choral celebration energy.
        /// </summary>
        public static void ApplyTriumphantChorusTrail(float time)
        {
            ApplyGardenTrailWithNoise(time,
                OdeToJoyPalette.RosePink, OdeToJoyPalette.GoldenPollen,
                scrollSpeed: 0.6f, distortionAmt: 0.04f, overbrightMult: 2.2f,
                noiseScale: 2f, noiseScroll: 0.3f);
        }

        /// <summary>
        /// Preset: TheStandingOvation summon trail — encore performance energy.
        /// </summary>
        public static void ApplyStandingOvationTrail(float time)
        {
            ApplyGardenTrailWithNoise(time,
                OdeToJoyPalette.GoldenPollen, OdeToJoyPalette.SunlightYellow,
                scrollSpeed: 1.0f, distortionAmt: 0.06f, overbrightMult: 2.5f,
                noiseScale: 3f, noiseScroll: 0.4f);
        }

        /// <summary>
        /// Preset: FountainOfJoyousHarmony summon trail — flowing fountain of garden energy.
        /// </summary>
        public static void ApplyFountainOfHarmonyTrail(float time)
        {
            ApplyGardenTrailWithNoise(time,
                OdeToJoyPalette.VerdantGreen, OdeToJoyPalette.RosePink,
                scrollSpeed: 0.8f, distortionAmt: 0.05f, overbrightMult: 2.4f,
                noiseScale: 2.5f, noiseScroll: 0.35f);
        }

        // =====================================================================
        //  TECHNIQUE: CelestialValor (via CelestialValorTrail shader)
        // =====================================================================

        /// <summary>
        /// Apply the CelestialValorTrail shader tuned for Ode to Joy garden effects.
        /// </summary>
        public static void ApplyCelestialTrail(float time, Color primary, Color secondary,
            float scrollSpeed = 0.8f, float overbrightMult = 2.5f)
        {
            Effect shader = ShaderLoader.CelestialValorTrail;
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(1.3f);
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
