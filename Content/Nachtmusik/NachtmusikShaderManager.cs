using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using MagnumOpus.Common.Systems.Shaders;

namespace MagnumOpus.Content.Nachtmusik
{
    /// <summary>
    /// Compartmentalized shader manager for all Nachtmusik VFX.
    /// Wraps ShaderLoader access to NachtmusikStarTrail.fx and
    /// NachtmusikSerenade.fx — providing preset Apply* methods for
    /// each weapon's rendering technique.
    ///
    /// Techniques available (when .fx files are created):
    ///   NachtmusikStarTrail  — StarTrail: twinkling star trail with playful bounce and night sky noise
    ///   NachtmusikSerenade   — SerenadeBloom: soft starlit bloom with twinkling point-light sparkles
    ///
    /// Usage (in PreDraw):
    ///   NachtmusikShaderManager.BindNoiseTexture(device);
    ///   NachtmusikShaderManager.ApplyStarTrail(time, primary, secondary);
    ///   // ... draw trail geometry ...
    ///   NachtmusikShaderManager.RestoreSpriteBatch(sb);
    ///
    /// NOTE: Nachtmusik shaders (.fx files) do not yet exist. This manager is
    /// scaffolded in advance so weapon VFX files can reference it. All Apply*
    /// methods gracefully return if the shader is null. When the .fx files are
    /// created, they will work immediately via ShaderLoader auto-discovery.
    /// For now, Nachtmusik weapons use the existing shared trail shaders
    /// (SimpleTrailShader, ScrollingTrailShader) via fallback methods.
    /// </summary>
    public static class NachtmusikShaderManager
    {
        // =====================================================================
        //  Shader Constants (future .fx file names)
        // =====================================================================

        /// <summary>Name for the Nachtmusik star trail shader (when created).</summary>
        public const string NachtmusikStarTrailShaderName = "NachtmusikStarTrail";

        /// <summary>Name for the Nachtmusik serenade bloom shader (when created).</summary>
        public const string NachtmusikSerenadeShaderName = "NachtmusikSerenade";

        // =====================================================================
        //  Shader Availability
        // =====================================================================

        /// <summary>True if the NachtmusikStarTrail shader loaded successfully.</summary>
        public static bool HasStarTrail => ShaderLoader.HasShader(NachtmusikStarTrailShaderName);

        /// <summary>True if the NachtmusikSerenade shader loaded successfully.</summary>
        public static bool HasSerenade => ShaderLoader.HasShader(NachtmusikSerenadeShaderName);

        /// <summary>True if any Nachtmusik shader is available.</summary>
        public static bool IsAvailable => HasStarTrail || HasSerenade;

        /// <summary>True if the shared scrolling trail shader is available (fallback).</summary>
        public static bool HasFallbackTrail => ShaderLoader.HasShader(ShaderLoader.ScrollingTrailShader);

        /// <summary>True if any trail shader is usable (dedicated or fallback).</summary>
        public static bool CanRenderTrails => HasStarTrail || HasFallbackTrail;

        // =====================================================================
        //  Noise Texture Binding
        // =====================================================================

        /// <summary>
        /// Binds StarFieldScatter noise to sampler slot 1 for twinkling star distortion.
        /// The signature Nachtmusik noise: scattered starlight points.
        /// Falls back to SparklyNoiseTexture then PerlinNoise if unavailable.
        /// </summary>
        public static void BindNoiseTexture(GraphicsDevice device)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("StarFieldScatter");
            if (noise == null)
                noise = ShaderLoader.GetNoiseTexture("SparklyNoiseTexture");
            if (noise == null)
                noise = ShaderLoader.GetNoiseTexture("PerlinNoise");
            if (noise != null)
            {
                device.Textures[1] = noise;
                device.SamplerStates[1] = SamplerState.LinearWrap;
            }
        }

        /// <summary>
        /// Binds CosmicNebulaClouds to sampler slot 1 for nebula/cosmic effects.
        /// Falls back to NebulaWispNoise then StarFieldScatter if unavailable.
        /// </summary>
        public static void BindCosmicNoiseTexture(GraphicsDevice device)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("CosmicNebulaClouds");
            if (noise == null)
                noise = ShaderLoader.GetNoiseTexture("NebulaWispNoise");
            if (noise == null)
                noise = ShaderLoader.GetNoiseTexture("StarFieldScatter");
            if (noise != null)
            {
                device.Textures[1] = noise;
                device.SamplerStates[1] = SamplerState.LinearWrap;
            }
        }

        // =====================================================================
        //  TECHNIQUE: StarTrail (NachtmusikStarTrail.fx — future)
        // =====================================================================

        /// <summary>
        /// Configure and apply NachtmusikStarTrail.fx for twinkling star trail rendering.
        /// Uses nocturnal blue-to-silver gradient with star-sparkle noise distortion.
        /// </summary>
        public static void ApplyStarTrail(float time, Color primary, Color secondary,
            float scrollSpeed = 1.0f, float distortionAmt = 0.06f, float overbrightMult = 2.5f)
        {
            Effect shader = ShaderLoader.GetShader(NachtmusikStarTrailShaderName);
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

            shader.CurrentTechnique = shader.Techniques["StarTrail"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>
        /// Configure NachtmusikStarTrail.fx with noise texture for richer effects.
        /// </summary>
        public static void ApplyStarTrailWithNoise(float time, Color primary, Color secondary,
            float scrollSpeed = 1.0f, float distortionAmt = 0.06f, float overbrightMult = 2.5f,
            float noiseScale = 3f, float noiseScroll = 0.5f)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("StarFieldScatter");
            if (noise == null)
                noise = ShaderLoader.GetNoiseTexture("SparklyNoiseTexture");
            if (noise != null)
            {
                Main.graphics.GraphicsDevice.Textures[1] = noise;
                Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;
            }

            Effect shader = ShaderLoader.GetShader(NachtmusikStarTrailShaderName);
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

            shader.CurrentTechnique = shader.Techniques["StarTrail"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        // =====================================================================
        //  FALLBACK: Use shared ScrollingTrailShader for trail rendering
        // =====================================================================

        /// <summary>
        /// Fallback trail rendering using the shared ScrollingTrailShader.
        /// Used when NachtmusikStarTrail.fx has not been created yet.
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
        //  WEAPON-SPECIFIC PRESETS
        // =====================================================================

        /// <summary>
        /// Preset: NocturnalExecutioner trail — heavy midnight sweep with violet flash.
        /// </summary>
        public static void ApplyNocturnalExecutionerTrail(float time)
        {
            if (HasStarTrail)
            {
                ApplyStarTrailWithNoise(time,
                    NachtmusikPalette.MidnightBlue, NachtmusikPalette.Violet,
                    scrollSpeed: 1.0f, distortionAmt: 0.08f, overbrightMult: 2.8f);
            }
            else
            {
                ApplyFallbackTrail(time,
                    NachtmusikPalette.MidnightBlue, NachtmusikPalette.Violet,
                    scrollSpeed: 1.0f, overbrightMult: 2.8f);
            }
        }

        /// <summary>
        /// Preset: MidnightsCrescendo trail — building intensity from deep blue to starlit white.
        /// </summary>
        public static void ApplyMidnightsCrescendoTrail(float time)
        {
            if (HasStarTrail)
            {
                ApplyStarTrailWithNoise(time,
                    NachtmusikPalette.DeepBlue, NachtmusikPalette.StarWhite,
                    scrollSpeed: 1.5f, distortionAmt: 0.06f, overbrightMult: 3.0f);
            }
            else
            {
                ApplyFallbackTrail(time,
                    NachtmusikPalette.DeepBlue, NachtmusikPalette.StarWhite,
                    scrollSpeed: 1.5f, overbrightMult: 3.0f);
            }
        }

        /// <summary>
        /// Preset: TwilightSeverance trail — dusk violet cutting to silver.
        /// </summary>
        public static void ApplyTwilightSeveranceTrail(float time)
        {
            if (HasStarTrail)
            {
                ApplyStarTrailWithNoise(time,
                    NachtmusikPalette.DuskViolet, NachtmusikPalette.MoonlitSilver,
                    scrollSpeed: 2.0f, distortionAmt: 0.04f, overbrightMult: 3.2f);
            }
            else
            {
                ApplyFallbackTrail(time,
                    NachtmusikPalette.DuskViolet, NachtmusikPalette.MoonlitSilver,
                    scrollSpeed: 2.0f, overbrightMult: 3.2f);
            }
        }

        /// <summary>
        /// Preset: ConstellationPiercer trail — starfield precision shot.
        /// </summary>
        public static void ApplyConstellationPiercerTrail(float time)
        {
            if (HasStarTrail)
            {
                ApplyStarTrailWithNoise(time,
                    NachtmusikPalette.ConstellationBlue, NachtmusikPalette.StarGold,
                    scrollSpeed: 2.5f, distortionAmt: 0.03f, overbrightMult: 3.0f);
            }
            else
            {
                ApplyFallbackTrail(time,
                    NachtmusikPalette.ConstellationBlue, NachtmusikPalette.StarGold,
                    scrollSpeed: 2.5f, overbrightMult: 3.0f);
            }
        }

        /// <summary>
        /// Preset: NebulasWhisper trail — soft nebula mist with purple-pink flow.
        /// </summary>
        public static void ApplyNebulasWhisperTrail(float time)
        {
            if (HasStarTrail)
            {
                ApplyStarTrailWithNoise(time,
                    NachtmusikPalette.CosmicPurple, NachtmusikPalette.NebulaPink,
                    scrollSpeed: 0.8f, distortionAmt: 0.09f, overbrightMult: 2.5f,
                    noiseScale: 2.5f, noiseScroll: 0.4f);
            }
            else
            {
                ApplyFallbackTrail(time,
                    NachtmusikPalette.CosmicPurple, NachtmusikPalette.NebulaPink,
                    scrollSpeed: 0.8f, overbrightMult: 2.5f);
            }
        }

        /// <summary>
        /// Preset: SerenadeOfDistantStars trail — warm starlit melody flow.
        /// </summary>
        public static void ApplySerenadeTrail(float time)
        {
            if (HasStarTrail)
            {
                ApplyStarTrailWithNoise(time,
                    NachtmusikPalette.DeepBlue, NachtmusikPalette.MoonlitSilver,
                    scrollSpeed: 1.2f, distortionAmt: 0.05f, overbrightMult: 2.6f);
            }
            else
            {
                ApplyFallbackTrail(time,
                    NachtmusikPalette.DeepBlue, NachtmusikPalette.MoonlitSilver,
                    scrollSpeed: 1.2f, overbrightMult: 2.6f);
            }
        }

        /// <summary>
        /// Preset: StarweaversGrimoire trail — arcane star-weaving pattern.
        /// </summary>
        public static void ApplyStarweaverTrail(float time)
        {
            if (HasStarTrail)
            {
                ApplyStarTrailWithNoise(time,
                    NachtmusikPalette.CosmicPurple, NachtmusikPalette.SerenadeGlow,
                    scrollSpeed: 1.3f, distortionAmt: 0.07f, overbrightMult: 2.8f,
                    noiseScale: 2f, noiseScroll: 0.6f);
            }
            else
            {
                ApplyFallbackTrail(time,
                    NachtmusikPalette.CosmicPurple, NachtmusikPalette.SerenadeGlow,
                    scrollSpeed: 1.3f, overbrightMult: 2.8f);
            }
        }

        /// <summary>
        /// Preset: RequiemOfTheCosmos trail — grand cosmic finale with gold accents.
        /// </summary>
        public static void ApplyRequiemTrail(float time)
        {
            if (HasStarTrail)
            {
                ApplyStarTrailWithNoise(time,
                    NachtmusikPalette.CosmicVoid, NachtmusikPalette.RadianceGold,
                    scrollSpeed: 0.7f, distortionAmt: 0.12f, overbrightMult: 3.5f,
                    noiseScale: 2f, noiseScroll: 0.3f);
            }
            else
            {
                ApplyFallbackTrail(time,
                    NachtmusikPalette.CosmicVoid, NachtmusikPalette.RadianceGold,
                    scrollSpeed: 0.7f, overbrightMult: 3.5f);
            }
        }

        /// <summary>
        /// Preset: CelestialChorusBaton minion trail — harmonic celestial choir.
        /// </summary>
        public static void ApplyCelestialChorusTrail(float time)
        {
            if (HasStarTrail)
            {
                ApplyStarTrailWithNoise(time,
                    NachtmusikPalette.ConstellationBlue, NachtmusikPalette.StarWhite,
                    scrollSpeed: 1.0f, distortionAmt: 0.05f, overbrightMult: 2.5f);
            }
            else
            {
                ApplyFallbackTrail(time,
                    NachtmusikPalette.ConstellationBlue, NachtmusikPalette.StarWhite,
                    scrollSpeed: 1.0f, overbrightMult: 2.5f);
            }
        }

        /// <summary>
        /// Preset: GalacticOverture minion trail — grand galactic entrance with gold.
        /// </summary>
        public static void ApplyGalacticOvertureTrail(float time)
        {
            if (HasStarTrail)
            {
                ApplyStarTrailWithNoise(time,
                    NachtmusikPalette.DeepBlue, NachtmusikPalette.StarGold,
                    scrollSpeed: 1.4f, distortionAmt: 0.08f, overbrightMult: 3.0f);
            }
            else
            {
                ApplyFallbackTrail(time,
                    NachtmusikPalette.DeepBlue, NachtmusikPalette.StarGold,
                    scrollSpeed: 1.4f, overbrightMult: 3.0f);
            }
        }

        /// <summary>
        /// Preset: ConductorOfConstellations minion trail — commanding star authority.
        /// </summary>
        public static void ApplyConductorTrail(float time)
        {
            if (HasStarTrail)
            {
                ApplyStarTrailWithNoise(time,
                    NachtmusikPalette.MidnightBlue, NachtmusikPalette.StarWhite,
                    scrollSpeed: 1.6f, distortionAmt: 0.06f, overbrightMult: 2.8f,
                    noiseScale: 3.5f, noiseScroll: 0.5f);
            }
            else
            {
                ApplyFallbackTrail(time,
                    NachtmusikPalette.MidnightBlue, NachtmusikPalette.StarWhite,
                    scrollSpeed: 1.6f, overbrightMult: 2.8f);
            }
        }

        /// <summary>
        /// Preset: Boss (Queen of Radiance) trail — full celestial radiance.
        /// </summary>
        public static void ApplyQueenOfRadianceTrail(float time)
        {
            if (HasStarTrail)
            {
                ApplyStarTrailWithNoise(time,
                    NachtmusikPalette.CosmicVoid, NachtmusikPalette.TwinklingWhite,
                    scrollSpeed: 2.0f, distortionAmt: 0.14f, overbrightMult: 4.0f,
                    noiseScale: 2f, noiseScroll: 0.3f);
            }
            else
            {
                ApplyFallbackTrail(time,
                    NachtmusikPalette.CosmicVoid, NachtmusikPalette.TwinklingWhite,
                    scrollSpeed: 2.0f, overbrightMult: 4.0f);
            }
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
