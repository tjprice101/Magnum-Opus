using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using MagnumOpus.Common.Systems.Shaders;

namespace MagnumOpus.Content.SwanLake
{
    /// <summary>
    /// Compartmentalized shader manager for all Swan Lake VFX.
    /// Wraps ShaderLoader access to SwanLakeTrail.fx, SwanLakeBloom.fx,
    /// and SwanLakePrismatic.fx — providing preset Apply* methods for
    /// each weapon's rendering technique.
    ///
    /// Techniques available (when .fx files are created):
    ///   SwanLakeTrail    — GracefulArc: flowing trail with iridescent edge shimmer and feather dissolve
    ///   SwanLakeBloom    — DualPolarity: bloom with white core and black outer halo (or inverse)
    ///   SwanLakePrismatic — RainbowShimmer: rainbow iridescence overlay using hue rotation
    ///
    /// Usage (in PreDraw):
    ///   SwanLakeShaderManager.BindNoiseTexture(device);
    ///   SwanLakeShaderManager.ApplyGracefulTrail(time, primary, secondary);
    ///   // ... draw trail geometry ...
    ///   SwanLakeShaderManager.RestoreSpriteBatch(sb);
    ///
    /// NOTE: Swan Lake shaders (.fx files) do not yet exist. This manager is
    /// scaffolded in advance so weapon VFX files can reference it. All Apply*
    /// methods gracefully return if the shader is null. When the .fx files are
    /// created, they will work immediately via ShaderLoader auto-discovery.
    /// For now, Swan Lake weapons use the existing shared trail shaders
    /// (SimpleTrailShader, ScrollingTrailShader) via fallback methods.
    /// </summary>
    public static class SwanLakeShaderManager
    {
        // =====================================================================
        //  Shader Constants (future .fx file names)
        // =====================================================================

        /// <summary>Name for the Swan Lake trail shader (when created).</summary>
        public const string SwanLakeTrailShaderName = "SwanLakeTrail";

        /// <summary>Name for the Swan Lake bloom shader (when created).</summary>
        public const string SwanLakeBloomShaderName = "SwanLakeBloom";

        /// <summary>Name for the Swan Lake prismatic shader (when created).</summary>
        public const string SwanLakePrismaticShaderName = "SwanLakePrismatic";

        // =====================================================================
        //  Shader Availability
        // =====================================================================

        /// <summary>True if the SwanLakeTrail shader loaded successfully.</summary>
        public static bool HasSwanTrail => ShaderLoader.HasShader(SwanLakeTrailShaderName);

        /// <summary>True if the SwanLakeBloom shader loaded successfully.</summary>
        public static bool HasSwanBloom => ShaderLoader.HasShader(SwanLakeBloomShaderName);

        /// <summary>True if the SwanLakePrismatic shader loaded successfully.</summary>
        public static bool HasSwanPrismatic => ShaderLoader.HasShader(SwanLakePrismaticShaderName);

        /// <summary>True if any Swan Lake shader is available.</summary>
        public static bool IsAvailable => HasSwanTrail || HasSwanBloom || HasSwanPrismatic;

        /// <summary>True if the shared scrolling trail shader is available (fallback).</summary>
        public static bool HasFallbackTrail => ShaderLoader.HasShader(ShaderLoader.ScrollingTrailShader);

        /// <summary>True if any trail shader is usable (dedicated or fallback).</summary>
        public static bool CanRenderTrails => HasSwanTrail || HasFallbackTrail;

        // =====================================================================
        //  Noise Texture Binding
        // =====================================================================

        /// <summary>
        /// Binds SoftCircularCaustics to sampler slot 1 for graceful flow distortion.
        /// Swan Lake's signature noise: soft, circular, elegant.
        /// </summary>
        public static void BindNoiseTexture(GraphicsDevice device)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("SoftCircularCaustics");
            if (noise == null)
                noise = ShaderLoader.GetNoiseTexture("PerlinNoise");
            if (noise != null)
            {
                device.Textures[1] = noise;
                device.SamplerStates[1] = SamplerState.LinearWrap;
            }
        }

        /// <summary>
        /// Binds CosmicEnergyVortex to sampler slot 1 for prismatic swirl effects.
        /// Falls back to SoftCircularCaustics if unavailable.
        /// </summary>
        public static void BindPrismaticNoiseTexture(GraphicsDevice device)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("CosmicEnergyVortex");
            if (noise == null)
                noise = ShaderLoader.GetNoiseTexture("SoftCircularCaustics");
            if (noise == null)
                noise = ShaderLoader.GetNoiseTexture("PerlinNoise");
            if (noise != null)
            {
                device.Textures[1] = noise;
                device.SamplerStates[1] = SamplerState.LinearWrap;
            }
        }

        // =====================================================================
        //  TECHNIQUE: GracefulArc (SwanLakeTrail.fx — future)
        // =====================================================================

        /// <summary>
        /// Configure and apply SwanLakeTrail.fx for graceful flowing trail rendering.
        /// Uses dual-polarity gradient with iridescent edge shimmer.
        /// </summary>
        public static void ApplyGracefulTrail(float time, Color primary, Color secondary,
            float scrollSpeed = 1.0f, float distortionAmt = 0.06f, float overbrightMult = 2.5f)
        {
            Effect shader = ShaderLoader.GetShader(SwanLakeTrailShaderName);
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

            shader.CurrentTechnique = shader.Techniques["GracefulArc"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>
        /// Configure SwanLakeTrail.fx with noise texture for richer effects.
        /// </summary>
        public static void ApplyGracefulTrailWithNoise(float time, Color primary, Color secondary,
            float scrollSpeed = 1.0f, float distortionAmt = 0.06f, float overbrightMult = 2.5f,
            float noiseScale = 3f, float noiseScroll = 0.5f)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("SoftCircularCaustics");
            if (noise == null)
                noise = ShaderLoader.GetNoiseTexture("PerlinNoise");
            if (noise != null)
            {
                Main.graphics.GraphicsDevice.Textures[1] = noise;
                Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;
            }

            Effect shader = ShaderLoader.GetShader(SwanLakeTrailShaderName);
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

            shader.CurrentTechnique = shader.Techniques["GracefulArc"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        // =====================================================================
        //  FALLBACK: Use shared ScrollingTrailShader for trail rendering
        // =====================================================================

        /// <summary>
        /// Fallback trail rendering using the shared ScrollingTrailShader.
        /// Used when SwanLakeTrail.fx has not been created yet.
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
        /// Preset: CalloftheBlackSwan trail — dark-to-light monochrome sweep.
        /// </summary>
        public static void ApplyBlackSwanTrail(float time)
        {
            if (HasSwanTrail)
            {
                ApplyGracefulTrailWithNoise(time,
                    SwanLakePalette.ObsidianBlack, SwanLakePalette.PureWhite,
                    scrollSpeed: 1.2f, distortionAmt: 0.08f, overbrightMult: 2.8f);
            }
            else
            {
                ApplyFallbackTrail(time,
                    SwanLakePalette.ObsidianBlack, SwanLakePalette.PureWhite,
                    scrollSpeed: 1.2f, overbrightMult: 2.8f);
            }
        }

        /// <summary>
        /// Preset: ChromaticSwanSong trail — prismatic magic cascade.
        /// </summary>
        public static void ApplySwanSongTrail(float time)
        {
            Color rainbow = SwanLakePalette.GetRainbow(time * 0.1f);
            if (HasSwanTrail)
            {
                ApplyGracefulTrailWithNoise(time,
                    SwanLakePalette.Silver, rainbow,
                    scrollSpeed: 1.5f, distortionAmt: 0.10f, overbrightMult: 3.0f);
            }
            else
            {
                ApplyFallbackTrail(time,
                    SwanLakePalette.Silver, rainbow,
                    scrollSpeed: 1.5f, overbrightMult: 3.0f);
            }
        }

        /// <summary>
        /// Preset: TheSwansLament trail — sorrowful monochrome to dying beauty.
        /// </summary>
        public static void ApplySwansLamentTrail(float time)
        {
            if (HasSwanTrail)
            {
                ApplyGracefulTrailWithNoise(time,
                    SwanLakePalette.SwanDarkGray, SwanLakePalette.FeatherWhite,
                    scrollSpeed: 0.8f, distortionAmt: 0.05f, overbrightMult: 2.5f);
            }
            else
            {
                ApplyFallbackTrail(time,
                    SwanLakePalette.SwanDarkGray, SwanLakePalette.FeatherWhite,
                    scrollSpeed: 0.8f, overbrightMult: 2.5f);
            }
        }

        /// <summary>
        /// Preset: FeatheroftheIridescentFlock trail — prismatic crystal orbit.
        /// </summary>
        public static void ApplyIridescentFlockTrail(float time)
        {
            Color rainbow = SwanLakePalette.GetVividRainbow(time * 0.15f);
            if (HasSwanTrail)
            {
                ApplyGracefulTrailWithNoise(time,
                    SwanLakePalette.Pearlescent, rainbow,
                    scrollSpeed: 1.8f, distortionAmt: 0.12f, overbrightMult: 3.2f,
                    noiseScale: 2.5f, noiseScroll: 0.7f);
            }
            else
            {
                ApplyFallbackTrail(time,
                    SwanLakePalette.Pearlescent, rainbow,
                    scrollSpeed: 1.8f, overbrightMult: 3.2f);
            }
        }

        /// <summary>
        /// Preset: IridescentWingspan trail — ethereal wing magic.
        /// </summary>
        public static void ApplyWingspanTrail(float time)
        {
            Color rainbow = SwanLakePalette.GetRainbow(time * 0.12f);
            if (HasSwanTrail)
            {
                ApplyGracefulTrailWithNoise(time,
                    SwanLakePalette.PureWhite, rainbow,
                    scrollSpeed: 1.0f, distortionAmt: 0.07f, overbrightMult: 3.0f);
            }
            else
            {
                ApplyFallbackTrail(time,
                    SwanLakePalette.PureWhite, rainbow,
                    scrollSpeed: 1.0f, overbrightMult: 3.0f);
            }
        }

        /// <summary>
        /// Preset: CallofthePearlescentLake trail — pearlescent projectile shimmer.
        /// </summary>
        public static void ApplyPearlescentLakeTrail(float time)
        {
            if (HasSwanTrail)
            {
                ApplyGracefulTrailWithNoise(time,
                    SwanLakePalette.LakeSurface, SwanLakePalette.Pearlescent,
                    scrollSpeed: 1.3f, distortionAmt: 0.06f, overbrightMult: 2.6f);
            }
            else
            {
                ApplyFallbackTrail(time,
                    SwanLakePalette.LakeSurface, SwanLakePalette.Pearlescent,
                    scrollSpeed: 1.3f, overbrightMult: 2.6f);
            }
        }

        /// <summary>
        /// Preset: Boss (The Monochromatic Fractal) trail — dual-polarity fractal pattern.
        /// </summary>
        public static void ApplyMonochromaticFractalTrail(float time)
        {
            if (HasSwanTrail)
            {
                ApplyGracefulTrailWithNoise(time,
                    SwanLakePalette.ShadowCore, SwanLakePalette.MonochromaticFlash,
                    scrollSpeed: 2.0f, distortionAmt: 0.14f, overbrightMult: 3.5f,
                    noiseScale: 2f, noiseScroll: 0.3f);
            }
            else
            {
                ApplyFallbackTrail(time,
                    SwanLakePalette.ShadowCore, SwanLakePalette.MonochromaticFlash,
                    scrollSpeed: 2.0f, overbrightMult: 3.5f);
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
