using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Shaders;
using MagnumOpus.Common.Systems.VFX.Optimization;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// Kernel shape for the motion blur bloom effect.
    /// Each shape maps to a different HLSL technique in MotionBlurBloom.fx.
    /// 
    /// Directional: Linear streak along velocity vector (projectiles, dashes).
    /// Radial: Burst outward from center (explosions, impacts).
    /// ArcSweep: Tangential rotational blur (melee swings, spinning attacks).
    /// </summary>
    public enum BlurKernelShape
    {
        Directional,
        Radial,
        ArcSweep
    }

    /// <summary>
    /// Configuration for a single motion blur bloom draw call.
    /// 
    /// Pre-built factory methods are provided for common use cases:
    ///   MotionBlurConfig.ForProjectile(velocity)
    ///   MotionBlurConfig.ForMeleeSwing(swingRotation)
    ///   MotionBlurConfig.ForExplosion()
    /// </summary>
    public struct MotionBlurConfig
    {
        /// <summary>Blur kernel shape (Directional, Radial, ArcSweep).</summary>
        public BlurKernelShape KernelShape;

        /// <summary>Normalized velocity/direction vector for Directional and ArcSweep kernels.</summary>
        public Vector2 VelocityDirection;

        /// <summary>Blur spread in UV space. Range: 0.005 (subtle) to 0.15 (extreme). Default: 0.04.</summary>
        public float BlurStrength;

        /// <summary>Bloom intensity multiplier applied to luminance. Range: 0.5 to 5.0. Default: 1.5.</summary>
        public float Intensity;

        /// <summary>Overall opacity of the effect. Range: 0.0 to 1.0. Default: 1.0.</summary>
        public float Opacity;

        /// <summary>Primary tint color (RGB, no alpha). Default: White.</summary>
        public Color PrimaryColor;

        /// <summary>Secondary gradient tint color (RGB, no alpha). Default: same as Primary.</summary>
        public Color SecondaryColor;

        /// <summary>
        /// Creates a default config with sensible values.
        /// </summary>
        public static MotionBlurConfig Default => new MotionBlurConfig
        {
            KernelShape = BlurKernelShape.Directional,
            VelocityDirection = Vector2.UnitX,
            BlurStrength = 0.04f,
            Intensity = 1.5f,
            Opacity = 1.0f,
            PrimaryColor = Color.White,
            SecondaryColor = Color.White
        };

        /// <summary>
        /// Factory: Motion blur for a moving projectile.
        /// Velocity is automatically normalized; speed scales blur strength.
        /// </summary>
        public static MotionBlurConfig ForProjectile(Vector2 velocity, Color primaryColor, Color? secondaryColor = null, float intensityMult = 1f)
        {
            float speed = velocity.Length();
            Vector2 dir = speed > 0.01f ? velocity / speed : Vector2.UnitX;

            return new MotionBlurConfig
            {
                KernelShape = BlurKernelShape.Directional,
                VelocityDirection = dir,
                BlurStrength = MathHelper.Clamp(speed * 0.003f, 0.01f, 0.12f),
                Intensity = MathHelper.Clamp(1.0f + speed * 0.03f, 1.0f, 4.0f) * intensityMult,
                Opacity = 1.0f,
                PrimaryColor = primaryColor,
                SecondaryColor = secondaryColor ?? primaryColor
            };
        }

        /// <summary>
        /// Factory: Arc sweep blur for melee swing effects.
        /// swingDirection should be the blade's current direction vector.
        /// </summary>
        public static MotionBlurConfig ForMeleeSwing(Vector2 swingDirection, Color primaryColor, Color? secondaryColor = null, float blurStrength = 0.06f, float intensityMult = 1f)
        {
            Vector2 dir = swingDirection;
            if (dir.LengthSquared() > 0.001f)
                dir.Normalize();
            else
                dir = Vector2.UnitX;

            return new MotionBlurConfig
            {
                KernelShape = BlurKernelShape.ArcSweep,
                VelocityDirection = dir,
                BlurStrength = blurStrength,
                Intensity = 2.0f * intensityMult,
                Opacity = 1.0f,
                PrimaryColor = primaryColor,
                SecondaryColor = secondaryColor ?? primaryColor
            };
        }

        /// <summary>
        /// Factory: Radial burst blur for explosions and impacts.
        /// </summary>
        public static MotionBlurConfig ForExplosion(Color primaryColor, Color? secondaryColor = null, float blurStrength = 0.05f, float intensityMult = 1f)
        {
            return new MotionBlurConfig
            {
                KernelShape = BlurKernelShape.Radial,
                VelocityDirection = Vector2.Zero,
                BlurStrength = blurStrength,
                Intensity = 2.5f * intensityMult,
                Opacity = 1.0f,
                PrimaryColor = primaryColor,
                SecondaryColor = secondaryColor ?? primaryColor
            };
        }
    }

    /// <summary>
    /// Renders motion blur bloom effects using the MotionBlurBloom.fx shader.
    /// 
    /// This is a LIBRARY class for per-weapon VFX — call it explicitly from
    /// individual weapon/projectile PreDraw methods. It does NOT auto-hook.
    /// 
    /// GPU Path (High+ quality): Uses HLSL shader with render-target ping-pong.
    ///   - Ultra: 13-tap ps_3_0 techniques
    ///   - High:  9-tap ps_2_0 techniques
    /// 
    /// CPU Fallback (Medium-): Multi-layer additive SpriteBatch overdraw.
    ///   - Medium: 4-layer bloom with directional offset
    ///   - Low/Potato: 2-layer simplified bloom
    /// 
    /// Usage in a projectile's PreDraw:
    ///   var config = MotionBlurConfig.ForProjectile(Projectile.velocity, themeColor);
    ///   MotionBlurBloomRenderer.Draw(Main.spriteBatch, myTexture, drawPos, sourceRect, 
    ///       config, drawScale, drawRotation, origin);
    /// </summary>
    public static class MotionBlurBloomRenderer
    {
        // Technique name lookup tables indexed by BlurKernelShape enum value
        private static readonly string[] TechniqueNamesUltra = 
        {
            "DirectionalBlurUltraTechnique",
            "RadialBlurUltraTechnique",
            "ArcSweepBlurUltraTechnique"
        };

        private static readonly string[] TechniqueNamesHigh = 
        {
            "DirectionalBlurHQTechnique",
            "RadialBlurHQTechnique",
            "ArcSweepBlurHQTechnique"
        };

        private static readonly string[] TechniqueNamesMedium = 
        {
            "DirectionalBlurTechnique",
            "RadialBlurTechnique",
            "ArcSweepBlurTechnique"
        };

        /// <summary>
        /// Returns true if the GPU shader path is available and enabled.
        /// </summary>
        public static bool IsGPUPathAvailable
        {
            get
            {
                try
                {
                    var quality = AdaptiveQualityManager.Instance;
                    if (quality == null || !quality.EnableMotionBlurBloom)
                        return false;

                    Effect shader = ShaderLoader.MotionBlurBloom;
                    return shader != null;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Primary draw method. Automatically selects GPU or CPU path based on quality settings.
        /// 
        /// Call this from a weapon/projectile's PreDraw method.
        /// The SpriteBatch should already be in a valid Begin state.
        /// This method will safely End/Begin as needed and restore the original state.
        /// </summary>
        /// <param name="sb">The active SpriteBatch (typically Main.spriteBatch).</param>
        /// <param name="texture">The texture to draw with motion blur bloom.</param>
        /// <param name="position">Screen-space draw position (world pos minus Main.screenPosition).</param>
        /// <param name="sourceRect">Source rectangle on the texture (null for full texture).</param>
        /// <param name="config">Motion blur configuration.</param>
        /// <param name="scale">Draw scale.</param>
        /// <param name="rotation">Draw rotation in radians.</param>
        /// <param name="origin">Origin point for rotation/scaling.</param>
        /// <param name="effects">Sprite flip effects.</param>
        public static void Draw(
            SpriteBatch sb,
            Texture2D texture,
            Vector2 position,
            Rectangle? sourceRect,
            MotionBlurConfig config,
            float scale = 1f,
            float rotation = 0f,
            Vector2? origin = null,
            SpriteEffects effects = SpriteEffects.None)
        {
            if (texture == null || config.Opacity <= 0f)
                return;

            Vector2 drawOrigin = origin ?? texture.Size() * 0.5f;

            // Attempt GPU path first
            if (IsGPUPathAvailable)
            {
                DrawGPU(sb, texture, position, sourceRect, config, scale, rotation, drawOrigin, effects);
            }
            else
            {
                DrawCPUFallback(sb, texture, position, sourceRect, config, scale, rotation, drawOrigin, effects);
            }
        }

        /// <summary>
        /// Overload accepting Vector2 scale for non-uniform scaling.
        /// </summary>
        public static void Draw(
            SpriteBatch sb,
            Texture2D texture,
            Vector2 position,
            Rectangle? sourceRect,
            MotionBlurConfig config,
            Vector2 scale,
            float rotation = 0f,
            Vector2? origin = null,
            SpriteEffects effects = SpriteEffects.None)
        {
            if (texture == null || config.Opacity <= 0f)
                return;

            Vector2 drawOrigin = origin ?? texture.Size() * 0.5f;

            // GPU path doesn't support non-uniform scale directly via render target approach,
            // so we use the CPU fallback which handles Vector2 scale natively.
            if (IsGPUPathAvailable)
            {
                // Use the larger axis as uniform scale for the GPU path
                float uniformScale = Math.Max(scale.X, scale.Y);
                DrawGPU(sb, texture, position, sourceRect, config, uniformScale, rotation, drawOrigin, effects);
            }
            else
            {
                DrawCPUFallbackV2(sb, texture, position, sourceRect, config, scale, rotation, drawOrigin, effects);
            }
        }

        #region GPU Shader Path

        /// <summary>
        /// GPU shader rendering path. Uses render target for full-screen-quality blur.
        /// 
        /// Pipeline:
        ///   1. Capture current SpriteBatch state via SpriteBatchScope
        ///   2. Draw the source texture to a transient render target
        ///   3. Apply the motion blur bloom shader, reading from the render target
        ///   4. Composite the result back to the screen with additive blending
        ///   5. Restore original SpriteBatch state
        /// </summary>
        private static void DrawGPU(
            SpriteBatch sb,
            Texture2D texture,
            Vector2 position,
            Rectangle? sourceRect,
            MotionBlurConfig config,
            float scale,
            float rotation,
            Vector2 origin,
            SpriteEffects effects)
        {
            Effect shader = ShaderLoader.MotionBlurBloom;
            if (shader == null)
            {
                DrawCPUFallback(sb, texture, position, sourceRect, config, scale, rotation, origin, effects);
                return;
            }

            // Determine padded render target size from the drawn sprite dimensions
            Rectangle srcRect = sourceRect ?? texture.Bounds;
            float maxBlurExpand = config.BlurStrength * Math.Max(srcRect.Width, srcRect.Height) * scale * 2f;
            int rtWidth = (int)(srcRect.Width * scale + maxBlurExpand + 16);
            int rtHeight = (int)(srcRect.Height * scale + maxBlurExpand + 16);
            rtWidth = Math.Max(rtWidth, 32);
            rtHeight = Math.Max(rtHeight, 32);

            // Clamp to reasonable sizes to prevent massive allocations
            rtWidth = Math.Min(rtWidth, 512);
            rtHeight = Math.Min(rtHeight, 512);

            GraphicsDevice device = Main.instance.GraphicsDevice;

            try
            {
                // Save the current SpriteBatch state and end it
                using var scope = sb.Scope();

                // --- PASS 1: Render source sprite to a temporary render target ---
                using var sourceScope = RenderTargetPool.GetTransient(out RenderTarget2D sourceRT, rtWidth, rtHeight);
                RenderTargetPool.SetAndClear(sourceRT, Color.Transparent);

                // Draw the sprite centered in the RT
                Vector2 rtCenter = new Vector2(rtWidth * 0.5f, rtHeight * 0.5f);

                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone);
                sb.Draw(texture, rtCenter, sourceRect, Color.White, rotation, origin, scale, effects, 0f);
                sb.End();

                // --- PASS 2: Apply motion blur bloom shader ---
                using var blurScope = RenderTargetPool.GetTransient(out RenderTarget2D blurRT, rtWidth, rtHeight);
                RenderTargetPool.SetAndClear(blurRT, Color.Transparent);

                // Select technique based on quality
                string techniqueName = SelectTechnique(config.KernelShape);
                shader.CurrentTechnique = shader.Techniques[techniqueName];

                // Set shader parameters
                SetShaderParameters(shader, config);

                // Draw source RT through the shader
                sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, shader);
                sb.Draw(sourceRT, Vector2.Zero, Color.White);
                sb.End();

                // --- PASS 3: Composite to screen with additive blending ---
                RenderTargetPool.RestoreBackBuffer();

                // Calculate screen position offset (RT center maps to the draw position)
                Vector2 screenPos = position - rtCenter;

                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null,
                    Main.GameViewMatrix.TransformationMatrix);
                sb.Draw(blurRT, screenPos, null, Color.White * config.Opacity, 0f,
                    Vector2.Zero, 1f, SpriteEffects.None, 0f);
                sb.End();

                // SpriteBatchScope dispose will restore the original state
            }
            catch
            {
                // On any failure, fall back to CPU path
                // SpriteBatchScope ensures state is restored even on exception
                try
                {
                    RenderTargetPool.RestoreBackBuffer();
                }
                catch { }

                DrawCPUFallback(sb, texture, position, sourceRect, config, scale, rotation, origin, effects);
            }
        }

        /// <summary>
        /// Selects the HLSL technique name based on kernel shape and current quality level.
        /// Ultra → 13-tap ps_3_0, High → 9-tap ps_2_0, Medium → 5-tap ps_2_0.
        /// </summary>
        private static string SelectTechnique(BlurKernelShape kernel)
        {
            int idx = (int)kernel;
            if (idx < 0 || idx > 2) idx = 0;

            var quality = AdaptiveQualityManager.Instance;
            if (quality == null)
                return TechniqueNamesHigh[idx];

            AdaptiveQualityManager.QualityLevel level = quality.CurrentQuality;

            if (level >= AdaptiveQualityManager.QualityLevel.Ultra)
                return TechniqueNamesUltra[idx];
            else if (level >= AdaptiveQualityManager.QualityLevel.High)
                return TechniqueNamesHigh[idx];
            else
                return TechniqueNamesMedium[idx];
        }

        /// <summary>
        /// Sets all shader uniform parameters from the config.
        /// </summary>
        private static void SetShaderParameters(Effect shader, MotionBlurConfig config)
        {
            shader.Parameters["uVelocityDir"]?.SetValue(config.VelocityDirection);
            shader.Parameters["uBlurStrength"]?.SetValue(config.BlurStrength);
            shader.Parameters["uIntensity"]?.SetValue(config.Intensity);
            shader.Parameters["uOpacity"]?.SetValue(config.Opacity);
            shader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);

            // Convert colors to Vector3 (RGB, no alpha)
            Vector3 primary = config.PrimaryColor.ToVector3();
            Vector3 secondary = config.SecondaryColor.ToVector3();
            shader.Parameters["uColor"]?.SetValue(primary);
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary);
        }

        #endregion

        #region CPU Fallback Path

        /// <summary>
        /// CPU fallback: Multi-layer directional bloom using SpriteBatch overdraw.
        /// Used when quality is below High or shader is unavailable.
        /// 
        /// Simulates motion blur by drawing offset copies along the velocity direction
        /// with decreasing opacity, plus standard Calamity-style bloom layers.
        /// </summary>
        private static void DrawCPUFallback(
            SpriteBatch sb,
            Texture2D texture,
            Vector2 position,
            Rectangle? sourceRect,
            MotionBlurConfig config,
            float scale,
            float rotation,
            Vector2 origin,
            SpriteEffects effects)
        {
            var quality = AdaptiveQualityManager.Instance;
            bool isMedium = quality != null && quality.CurrentQuality >= AdaptiveQualityManager.QualityLevel.Medium;

            // Keep full alpha for additive blending (SourceAlpha blend needs alpha > 0)
            Color baseColor = config.PrimaryColor;
            Color secondColor = config.SecondaryColor;

            // Use scope to safely manage SpriteBatch state
            using var scope = sb.Scope();

            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            if (config.KernelShape == BlurKernelShape.Directional || 
                config.KernelShape == BlurKernelShape.ArcSweep)
            {
                DrawDirectionalCPU(sb, texture, position, sourceRect, config, scale, rotation,
                    origin, effects, baseColor, secondColor, isMedium);
            }
            else // Radial
            {
                DrawRadialCPU(sb, texture, position, sourceRect, config, scale, rotation,
                    origin, effects, baseColor, isMedium);
            }

            // Standard bloom layers on top (always)
            DrawBloomLayers(sb, texture, position, sourceRect, config, scale, rotation,
                origin, effects, baseColor, isMedium);

            sb.End();
            // SpriteBatchScope dispose restores original state
        }

        /// <summary>
        /// CPU fallback with Vector2 scale support.
        /// </summary>
        private static void DrawCPUFallbackV2(
            SpriteBatch sb,
            Texture2D texture,
            Vector2 position,
            Rectangle? sourceRect,
            MotionBlurConfig config,
            Vector2 scale,
            float rotation,
            Vector2 origin,
            SpriteEffects effects)
        {
            var quality = AdaptiveQualityManager.Instance;
            bool isMedium = quality != null && quality.CurrentQuality >= AdaptiveQualityManager.QualityLevel.Medium;

            Color baseColor = config.PrimaryColor;
            Color secondColor = config.SecondaryColor;

            using var scope = sb.Scope();

            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            // Directional motion blur ghost copies
            int ghostCount = isMedium ? 4 : 2;
            Vector2 blurDir = config.VelocityDirection;
            float pixelSpread = config.BlurStrength * 400f;

            for (int i = 1; i <= ghostCount; i++)
            {
                float t = (float)i / (ghostCount + 1);
                float alpha = (1f - t) * 0.4f * config.Opacity;
                Vector2 offset = blurDir * pixelSpread * t;
                Color ghostColor = Color.Lerp(baseColor, secondColor, t) * alpha;

                sb.Draw(texture, position + offset, sourceRect, ghostColor, rotation, origin, scale, effects, 0f);
                sb.Draw(texture, position - offset, sourceRect, ghostColor * 0.7f, rotation, origin, scale, effects, 0f);
            }

            // Bloom layers
            float[] scales = isMedium ? new[] { 1.3f, 1.15f, 1.0f, 0.85f } : new[] { 1.2f, 0.9f };
            float[] opacities = isMedium ? new[] { 0.20f, 0.35f, 0.50f, 0.65f } : new[] { 0.30f, 0.55f };

            for (int i = 0; i < scales.Length; i++)
            {
                Color layerColor = (i == scales.Length - 1 ? Color.White : baseColor)
                    * opacities[i] * config.Opacity * config.Intensity * 0.4f;
                sb.Draw(texture, position, sourceRect, layerColor, rotation, origin, scale * scales[i], effects, 0f);
            }

            sb.End();
        }

        /// <summary>
        /// Draw directional ghost copies along the velocity direction.
        /// </summary>
        private static void DrawDirectionalCPU(
            SpriteBatch sb,
            Texture2D texture,
            Vector2 position,
            Rectangle? sourceRect,
            MotionBlurConfig config,
            float scale,
            float rotation,
            Vector2 origin,
            SpriteEffects effects,
            Color baseColor,
            Color secondColor,
            bool highQuality)
        {
            int ghostCount = highQuality ? 4 : 2;
            Vector2 blurDir = config.VelocityDirection;

            // Convert UV-space blur strength to pixel-space offset
            float pixelSpread = config.BlurStrength * 400f;

            for (int i = 1; i <= ghostCount; i++)
            {
                float t = (float)i / (ghostCount + 1);
                float alpha = (1f - t) * 0.4f * config.Opacity;
                Vector2 offset = blurDir * pixelSpread * t;

                Color ghostColor = Color.Lerp(baseColor, secondColor, t) * alpha;

                // Forward ghost
                sb.Draw(texture, position + offset, sourceRect, ghostColor, rotation,
                    origin, scale, effects, 0f);

                // Backward ghost (dimmer)
                sb.Draw(texture, position - offset, sourceRect, ghostColor * 0.7f, rotation,
                    origin, scale, effects, 0f);
            }
        }

        /// <summary>
        /// Draw radial ghost copies expanding from center.
        /// </summary>
        private static void DrawRadialCPU(
            SpriteBatch sb,
            Texture2D texture,
            Vector2 position,
            Rectangle? sourceRect,
            MotionBlurConfig config,
            float scale,
            float rotation,
            Vector2 origin,
            SpriteEffects effects,
            Color baseColor,
            bool highQuality)
        {
            int ringCount = highQuality ? 3 : 2;

            for (int ring = 1; ring <= ringCount; ring++)
            {
                float t = (float)ring / (ringCount + 1);
                float alpha = (1f - t) * 0.3f * config.Opacity;
                float ringScale = scale * (1f + t * config.BlurStrength * 3f);

                Color ghostColor = baseColor * alpha;
                sb.Draw(texture, position, sourceRect, ghostColor, rotation,
                    origin, ringScale, effects, 0f);
            }
        }

        /// <summary>
        /// Draw standard Calamity-style multi-layer bloom on top.
        /// </summary>
        private static void DrawBloomLayers(
            SpriteBatch sb,
            Texture2D texture,
            Vector2 position,
            Rectangle? sourceRect,
            MotionBlurConfig config,
            float scale,
            float rotation,
            Vector2 origin,
            SpriteEffects effects,
            Color baseColor,
            bool highQuality)
        {
            float[] bloomScales;
            float[] bloomOpacities;

            if (highQuality)
            {
                bloomScales = new[] { 1.3f, 1.15f, 1.0f, 0.85f };
                bloomOpacities = new[] { 0.20f, 0.35f, 0.50f, 0.65f };
            }
            else
            {
                bloomScales = new[] { 1.2f, 0.9f };
                bloomOpacities = new[] { 0.30f, 0.55f };
            }

            float intensityMod = config.Intensity * 0.4f;

            for (int i = 0; i < bloomScales.Length; i++)
            {
                bool isCore = i == bloomScales.Length - 1;
                Color layerColor = isCore
                    ? Color.White * bloomOpacities[i] * config.Opacity * intensityMod
                    : baseColor * bloomOpacities[i] * config.Opacity * intensityMod;

                sb.Draw(texture, position, sourceRect, layerColor, rotation,
                    origin, scale * bloomScales[i], effects, 0f);
            }
        }

        #endregion

        #region Convenience Methods

        /// <summary>
        /// Quick draw for projectiles: pass the projectile directly.
        /// Automatically computes draw position, rotation, and velocity-based config.
        /// </summary>
        public static void DrawProjectile(
            SpriteBatch sb,
            Texture2D texture,
            Projectile projectile,
            Color primaryColor,
            Color? secondaryColor = null,
            float intensityMult = 1f,
            Rectangle? sourceRect = null,
            Vector2? origin = null)
        {
            Vector2 drawPos = projectile.Center - Main.screenPosition;
            var config = MotionBlurConfig.ForProjectile(projectile.velocity, primaryColor, secondaryColor, intensityMult);
            Vector2 drawOrigin = origin ?? texture.Size() * 0.5f;

            Draw(sb, texture, drawPos, sourceRect, config, projectile.scale, projectile.rotation, drawOrigin);
        }

        /// <summary>
        /// Quick draw for melee swings: pass the swing direction directly.
        /// </summary>
        public static void DrawMeleeSwing(
            SpriteBatch sb,
            Texture2D texture,
            Vector2 position,
            Vector2 swingDirection,
            Color primaryColor,
            Color? secondaryColor = null,
            float scale = 1f,
            float rotation = 0f,
            float blurStrength = 0.06f,
            float intensityMult = 1f,
            Rectangle? sourceRect = null,
            Vector2? origin = null)
        {
            var config = MotionBlurConfig.ForMeleeSwing(swingDirection, primaryColor, secondaryColor, blurStrength, intensityMult);
            Draw(sb, texture, position, sourceRect, config, scale, rotation, origin);
        }

        /// <summary>
        /// Quick draw for explosions/impacts with radial blur.
        /// </summary>
        public static void DrawExplosion(
            SpriteBatch sb,
            Texture2D texture,
            Vector2 position,
            Color primaryColor,
            Color? secondaryColor = null,
            float scale = 1f,
            float blurStrength = 0.05f,
            float intensityMult = 1f,
            Rectangle? sourceRect = null,
            Vector2? origin = null)
        {
            var config = MotionBlurConfig.ForExplosion(primaryColor, secondaryColor, blurStrength, intensityMult);
            Draw(sb, texture, position, sourceRect, config, scale, 0f, origin);
        }

        #endregion
    }
}
