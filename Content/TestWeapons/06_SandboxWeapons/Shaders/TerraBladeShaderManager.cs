using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using MagnumOpus.Common.Systems.Shaders;

namespace MagnumOpus.Content.TestWeapons.SandboxWeapons.Shaders
{
    /// <summary>
    /// Compartmentalized shader manager for the Sandbox Terra Blade swing.
    /// Wraps ShaderLoader access to the TerraBladeSwingVFX.fxc shader and
    /// provides preset Apply* methods for each rendering pass.
    ///
    /// The shader has three techniques:
    ///   EnergyTrail  – Noise-distorted afterimage trail
    ///   BladeBloom   – Multi-layer radial bloom with directional bias
    ///   SlashSmear   – Arc-shaped swing smear overlay
    ///
    /// Usage (in PreDraw):
    ///   TerraBladeShaderManager.BindNoiseTexture(device);
    ///   TerraBladeShaderManager.ApplyEnergyTrail(shader, progress, swingSpeed, dir, time);
    ///   // ... draw afterimages ...
    ///   TerraBladeShaderManager.ApplyBladeBloom(shader, progress, swingSpeed, dir, time);
    ///   // ... draw bloom quads ...
    /// </summary>
    public static class TerraBladeShaderManager
    {
        // =====================================================================
        //  Terra Blade Color Palette (6-color scale: pianissimo → sforzando)
        // =====================================================================

        /// <summary>Dark shadow green — subtle underglow.</summary>
        public static readonly Color DarkGreen   = new Color(20, 60, 30);
        /// <summary>Forest green — outer glow edge.</summary>
        public static readonly Color ForestGreen = new Color(40, 140, 60);
        /// <summary>Primary energy green — main blade body.</summary>
        public static readonly Color EnergyGreen = new Color(100, 255, 120);
        /// <summary>Bright cyan-green — hot inner glow.</summary>
        public static readonly Color BrightCyan  = new Color(50, 200, 150);
        /// <summary>Pale mint — bloom highlight.</summary>
        public static readonly Color PaleMint    = new Color(180, 255, 210);
        /// <summary>White-hot — core/flare centre.</summary>
        public static readonly Color WhiteHot    = new Color(245, 255, 245);

        /// <summary>Full 6-color palette array for gradient lookups.</summary>
        public static readonly Color[] Palette = new Color[]
        {
            DarkGreen, ForestGreen, EnergyGreen, BrightCyan, PaleMint, WhiteHot
        };

        // =====================================================================
        //  Shader Constant
        // =====================================================================

        /// <summary>Shader name constant matching ShaderLoader registration.</summary>
        public const string ShaderName = "TerraBladeSwingVFX";

        // =====================================================================
        //  Palette Utility
        // =====================================================================

        /// <summary>
        /// Interpolates through the 6-color palette. t=0 → DarkGreen, t=1 → WhiteHot.
        /// </summary>
        public static Color GetPaletteColor(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            float scaled = t * (Palette.Length - 1);
            int idx = (int)scaled;
            int next = Math.Min(idx + 1, Palette.Length - 1);
            return Color.Lerp(Palette[idx], Palette[next], scaled - idx);
        }

        // =====================================================================
        //  Shader Access
        // =====================================================================

        /// <summary>
        /// Retrieves the loaded Effect from ShaderLoader. Returns null if unavailable.
        /// </summary>
        public static Effect GetShader()
        {
            return ShaderLoader.GetShader(ShaderName);
        }

        /// <summary>
        /// True if the shader .fxc was compiled and loaded successfully.
        /// </summary>
        public static bool IsAvailable => ShaderLoader.HasShader(ShaderName);

        // =====================================================================
        //  Noise Texture Binding
        // =====================================================================

        /// <summary>
        /// Binds the recommended noise texture (PerlinNoise) to sampler slot 1
        /// so the shader's uImage1 uniform can sample it. Call once per frame
        /// before any Apply* method.
        /// </summary>
        public static void BindNoiseTexture(GraphicsDevice device)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("PerlinNoise");
            if (noise != null)
            {
                device.Textures[1] = noise;
                device.SamplerStates[1] = SamplerState.LinearWrap;
            }
        }

        /// <summary>
        /// Binds SparklyNoiseTexture to sampler slot 1 for shimmer effects.
        /// Falls back to PerlinNoise if unavailable.
        /// </summary>
        public static void BindShimmerTexture(GraphicsDevice device)
        {
            Texture2D shimmerNoise = ShaderLoader.GetNoiseTexture("SparklyNoiseTexture");
            if (shimmerNoise == null)
                shimmerNoise = ShaderLoader.GetNoiseTexture("PerlinNoise");
            if (shimmerNoise != null)
            {
                device.Textures[1] = shimmerNoise;
                device.SamplerStates[1] = SamplerState.LinearWrap;
            }
        }

        // =====================================================================
        //  Common Uniform Helpers
        // =====================================================================

        /// <summary>
        /// Sets the base uniforms shared by all three techniques.
        /// </summary>
        private static void SetCommonUniforms(
            Effect shader, float time, float swingSpeed, float direction,
            float progress, float opacity = 1f)
        {
            shader.Parameters["uColor"]?.SetValue(EnergyGreen.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(BrightCyan.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(opacity);
            shader.Parameters["uProgress"]?.SetValue(progress);
            shader.Parameters["uSwingSpeed"]?.SetValue(swingSpeed);
            shader.Parameters["uDirection"]?.SetValue(direction);

            // Noise config
            Texture2D noise = ShaderLoader.GetNoiseTexture("PerlinNoise");
            shader.Parameters["uHasSecondaryTex"]?.SetValue(noise != null ? 1f : 0f);
            shader.Parameters["uSecondaryTexScale"]?.SetValue(1.2f);
            shader.Parameters["uSecondaryTexScroll"]?.SetValue(0.4f);
        }

        // =====================================================================
        //  TECHNIQUE 1: EnergyTrail
        // =====================================================================

        /// <summary>
        /// Configures the shader for the EnergyTrail technique and applies it.
        /// Call this inside a SpriteBatch.Begin(Immediate, Additive) block,
        /// then draw each afterimage blade sprite.
        /// </summary>
        /// <param name="shader">The loaded Effect.</param>
        /// <param name="afterimageProgress">0 = newest image, 1 = oldest (controls fade).</param>
        /// <param name="swingSpeed">Current angular velocity intensity (0-1).</param>
        /// <param name="direction">Swing direction (-1 or 1).</param>
        /// <param name="time">Main.GlobalTimeWrappedHourly.</param>
        /// <param name="intensity">Brightness multiplier (default 2.5).</param>
        public static void ApplyEnergyTrail(
            Effect shader, float afterimageProgress, float swingSpeed,
            float direction, float time, float intensity = 2.5f)
        {
            SetCommonUniforms(shader, time, swingSpeed, direction, afterimageProgress, 1f);
            shader.Parameters["uIntensity"]?.SetValue(intensity);
            shader.Parameters["uOverbrightMult"]?.SetValue(3.0f);

            shader.CurrentTechnique = shader.Techniques["EnergyTrail"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        // =====================================================================
        //  TECHNIQUE 2: BladeBloom
        // =====================================================================

        /// <summary>
        /// Configures the shader for the BladeBloom technique and applies it.
        /// Draw the glow/bloom texture (e.g. SoftGlow2) at the blade centre
        /// or tip position with additive blending.
        /// </summary>
        /// <param name="shader">The loaded Effect.</param>
        /// <param name="swingProgress">Current swing progress (0-1).</param>
        /// <param name="swingSpeed">Current angular velocity intensity (0-1).</param>
        /// <param name="direction">Swing direction (-1 or 1).</param>
        /// <param name="time">Main.GlobalTimeWrappedHourly.</param>
        /// <param name="intensity">Brightness multiplier (default 3.0).</param>
        /// <param name="overbright">HDR multiplier (default 4.0).</param>
        public static void ApplyBladeBloom(
            Effect shader, float swingProgress, float swingSpeed,
            float direction, float time, float intensity = 3.0f, float overbright = 4.0f)
        {
            SetCommonUniforms(shader, time, swingSpeed, direction, swingProgress, 1f);
            shader.Parameters["uIntensity"]?.SetValue(intensity);
            shader.Parameters["uOverbrightMult"]?.SetValue(overbright);

            shader.CurrentTechnique = shader.Techniques["BladeBloom"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        // =====================================================================
        //  TECHNIQUE 3: SlashSmear
        // =====================================================================

        /// <summary>
        /// Configures the shader for the SlashSmear technique and applies it.
        /// Draw the smear/arc texture centered on the player with additive blending.
        /// </summary>
        /// <param name="shader">The loaded Effect.</param>
        /// <param name="swingProgress">Current swing progress (0-1).</param>
        /// <param name="swingSpeed">Current angular velocity intensity (0-1).</param>
        /// <param name="direction">Swing direction (-1 or 1).</param>
        /// <param name="time">Main.GlobalTimeWrappedHourly.</param>
        /// <param name="intensity">Brightness multiplier (default 2.0).</param>
        /// <param name="overbright">HDR multiplier (default 3.0).</param>
        public static void ApplySlashSmear(
            Effect shader, float swingProgress, float swingSpeed,
            float direction, float time, float intensity = 2.0f, float overbright = 3.0f)
        {
            SetCommonUniforms(shader, time, swingSpeed, direction, swingProgress, 1f);
            shader.Parameters["uIntensity"]?.SetValue(intensity);
            shader.Parameters["uOverbrightMult"]?.SetValue(overbright);

            shader.CurrentTechnique = shader.Techniques["SlashSmear"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        // =====================================================================
        //  TECHNIQUE 4: ShimmerOverlay
        // =====================================================================

        /// <summary>
        /// Configures the shader for the ShimmerOverlay technique.
        /// Combined scrolling noise + iridescent color cycling overlay.
        ///
        /// Call BindShimmerTexture() before this method.
        /// The afterimageOffset parameter shifts uTime backward for shimmer
        /// trail copies, making sparkles appear to streak behind the blade.
        /// </summary>
        public static void ApplyShimmerOverlay(
            Effect shader, float swingProgress, float swingSpeed,
            float direction, float time,
            float afterimageOffset = 0f,
            float intensity = 2.5f, float overbright = 3.5f)
        {
            float effectiveTime = time - afterimageOffset;

            SetCommonUniforms(shader, effectiveTime, swingSpeed, direction, swingProgress, 1f);
            shader.Parameters["uIntensity"]?.SetValue(intensity);
            shader.Parameters["uOverbrightMult"]?.SetValue(overbright);

            // Override noise params for shimmer (higher scale, faster scroll)
            float speedScale = MathHelper.Lerp(0.8f, 1.4f, swingSpeed);
            shader.Parameters["uSecondaryTexScale"]?.SetValue(2.0f);
            shader.Parameters["uSecondaryTexScroll"]?.SetValue(0.5f * speedScale);

            shader.CurrentTechnique = shader.Techniques["ShimmerOverlay"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        // =====================================================================
        //  SpriteBatch State Helpers
        // =====================================================================

        /// <summary>
        /// Begins a SpriteBatch in Immediate + Additive mode for shader drawing.
        /// Immediate mode is required so that shader parameter changes take effect
        /// between draw calls.
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
        /// Begins a SpriteBatch in Deferred + Additive mode (no shader, for
        /// multi-layer manual bloom stacking).
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
