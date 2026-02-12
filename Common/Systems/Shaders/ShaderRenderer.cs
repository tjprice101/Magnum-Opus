using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;

namespace MagnumOpus.Common.Systems.Shaders
{
    /// <summary>
    /// High-level shader rendering utilities for MagnumOpus.
    /// Provides easy SpriteBatch management and shader parameter configuration.
    /// 
    /// Usage:
    ///   using (ShaderRenderer.BeginShaderMode(ShaderRenderer.ShaderType.Bloom, color, intensity))
    ///   {
    ///       spriteBatch.Draw(...);
    ///   }
    /// </summary>
    public static class ShaderRenderer
    {
        private static SpriteBatch _spriteBatch;
        private static bool _inShaderMode;
        private static SpriteSortMode _previousSortMode;
        private static BlendState _previousBlendState;

        /// <summary>
        /// Available shader types.
        /// </summary>
        public enum ShaderType
        {
            None,
            Trail,
            TrailBloom,
            Bloom,
            GradientBloom,
            Flare,
            ChromaticAberration,
            RadialBlur,
            Vignette,
            ColorFlash,
            WaveDistortion,
            HeatDistortion,
            RealityCrack
        }

        #region SpriteBatch Helpers

        /// <summary>
        /// Begins a shader rendering block. Must be followed by EndShaderMode().
        /// Use the disposable pattern with BeginShaderScope() instead for safer usage.
        /// </summary>
        public static void BeginShaderMode(SpriteBatch spriteBatch, ShaderType type, Color color, 
            float intensity = 1f, Color? secondaryColor = null, Vector2? targetPosition = null)
        {
            if (_inShaderMode || spriteBatch == null)
                return;

            _spriteBatch = spriteBatch;
            _inShaderMode = true;

            Effect shader = GetShaderForType(type);
            ConfigureShader(shader, type, color, intensity, secondaryColor, targetPosition);

            // Determine blend state based on shader type
            BlendState blendState = IsAdditiveType(type) ? BlendState.Additive : BlendState.AlphaBlend;

            // End current spritebatch and restart with shader
            spriteBatch.End();
            spriteBatch.Begin(
                SpriteSortMode.Immediate, 
                blendState,
                SamplerState.LinearClamp,
                DepthStencilState.None,
                RasterizerState.CullNone,
                shader,
                Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Ends shader rendering mode and restores normal SpriteBatch state.
        /// </summary>
        public static void EndShaderMode()
        {
            if (!_inShaderMode || _spriteBatch == null)
                return;

            _spriteBatch.End();
            _spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.LinearClamp,
                DepthStencilState.None,
                RasterizerState.CullNone,
                null,
                Main.GameViewMatrix.TransformationMatrix);

            _inShaderMode = false;
            _spriteBatch = null;
        }

        /// <summary>
        /// Returns a disposable scope that automatically manages shader begin/end.
        /// 
        /// Usage:
        ///   using (ShaderRenderer.BeginShaderScope(spriteBatch, ShaderType.Bloom, color, intensity))
        ///   {
        ///       spriteBatch.Draw(...);
        ///   }
        /// </summary>
        public static ShaderScope BeginShaderScope(SpriteBatch spriteBatch, ShaderType type, Color color,
            float intensity = 1f, Color? secondaryColor = null, Vector2? targetPosition = null)
        {
            BeginShaderMode(spriteBatch, type, color, intensity, secondaryColor, targetPosition);
            return new ShaderScope();
        }

        #endregion

        #region Direct Shader Methods

        /// <summary>
        /// Draws a texture with bloom shader applied.
        /// </summary>
        public static void DrawWithBloom(SpriteBatch spriteBatch, Texture2D texture, Vector2 worldPosition,
            Color color, float scale, float intensity = 1f, float rotation = 0f)
        {
            if (spriteBatch == null || texture == null)
                return;

            Vector2 drawPos = worldPosition - Main.screenPosition;
            Vector2 origin = texture.Size() * 0.5f;

            using (BeginShaderScope(spriteBatch, ShaderType.Bloom, color, intensity))
            {
                spriteBatch.Draw(texture, drawPos, null, Color.White, rotation, origin, scale, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Draws a texture with gradient bloom (two-color blend).
        /// </summary>
        public static void DrawWithGradientBloom(SpriteBatch spriteBatch, Texture2D texture, Vector2 worldPosition,
            Color innerColor, Color outerColor, float scale, float intensity = 1f, float rotation = 0f)
        {
            if (spriteBatch == null || texture == null)
                return;

            Vector2 drawPos = worldPosition - Main.screenPosition;
            Vector2 origin = texture.Size() * 0.5f;

            using (BeginShaderScope(spriteBatch, ShaderType.GradientBloom, innerColor, intensity, outerColor))
            {
                spriteBatch.Draw(texture, drawPos, null, Color.White, rotation, origin, scale, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Draws a multi-layer bloom stack with shader (4 layers).
        /// </summary>
        public static void DrawBloomStack(SpriteBatch spriteBatch, Texture2D texture, Vector2 worldPosition,
            Color color, float baseScale, float intensity = 1f)
        {
            if (spriteBatch == null || texture == null)
                return;

            Vector2 drawPos = worldPosition - Main.screenPosition;
            Vector2 origin = texture.Size() * 0.5f;

            // Layer scales and opacities (from FargosSoulsDLC pattern)
            float[] scales = { 2.0f, 1.4f, 0.9f, 0.4f };
            float[] opacities = { 0.3f, 0.5f, 0.7f, 0.85f };

            using (BeginShaderScope(spriteBatch, ShaderType.Bloom, color, intensity))
            {
                for (int i = 0; i < 4; i++)
                {
                    Color layerColor = Color.White * opacities[i];
                    spriteBatch.Draw(texture, drawPos, null, layerColor, 0f, origin, 
                        baseScale * scales[i], SpriteEffects.None, 0f);
                }
            }
        }

        /// <summary>
        /// Draws a 4-point flare with shader.
        /// </summary>
        public static void DrawFlare(SpriteBatch spriteBatch, Texture2D texture, Vector2 worldPosition,
            Color color, float scale, float rotation, float intensity = 1f)
        {
            if (spriteBatch == null || texture == null)
                return;

            Vector2 drawPos = worldPosition - Main.screenPosition;
            Vector2 origin = texture.Size() * 0.5f;

            using (BeginShaderScope(spriteBatch, ShaderType.Flare, color, intensity))
            {
                spriteBatch.Draw(texture, drawPos, null, Color.White, rotation, origin, scale, SpriteEffects.None, 0f);
            }
        }

        #endregion

        #region Screen Effects

        /// <summary>
        /// Triggers chromatic aberration effect at a world position.
        /// Call this before drawing the scene for full-screen effect.
        /// </summary>
        public static void ApplyChromaticAberration(SpriteBatch spriteBatch, Vector2 worldPosition, float intensity)
        {
            if (spriteBatch == null || intensity <= 0)
                return;

            Vector2 screenPos = (worldPosition - Main.screenPosition) / new Vector2(Main.screenWidth, Main.screenHeight);
            BeginShaderMode(spriteBatch, ShaderType.ChromaticAberration, Color.White, intensity, null, screenPos);
        }

        /// <summary>
        /// Applies radial blur from a world position.
        /// </summary>
        public static void ApplyRadialBlur(SpriteBatch spriteBatch, Vector2 worldPosition, float intensity)
        {
            if (spriteBatch == null || intensity <= 0)
                return;

            Vector2 screenPos = (worldPosition - Main.screenPosition) / new Vector2(Main.screenWidth, Main.screenHeight);
            BeginShaderMode(spriteBatch, ShaderType.RadialBlur, Color.White, intensity, null, screenPos);
        }

        /// <summary>
        /// Applies color flash overlay.
        /// </summary>
        public static void ApplyColorFlash(SpriteBatch spriteBatch, Color flashColor, float intensity)
        {
            if (spriteBatch == null || intensity <= 0)
                return;

            BeginShaderMode(spriteBatch, ShaderType.ColorFlash, flashColor, intensity);
        }

        /// <summary>
        /// Applies Fate theme reality crack effect.
        /// </summary>
        public static void ApplyRealityCrack(SpriteBatch spriteBatch, Vector2 worldPosition, Color color, float intensity)
        {
            if (spriteBatch == null || intensity <= 0)
                return;

            Vector2 screenPos = (worldPosition - Main.screenPosition) / new Vector2(Main.screenWidth, Main.screenHeight);
            BeginShaderMode(spriteBatch, ShaderType.RealityCrack, color, intensity, null, screenPos);
        }

        #endregion

        #region Private Helpers

        /// <summary>
        /// Gets the shader Effect for a given type.
        /// Uses ShaderLoader for direct Effect references (no GameShaders.Misc indirection).
        /// </summary>
        private static Effect GetShaderForType(ShaderType type)
        {
            if (!ShaderLoader.ShadersEnabled)
                return null;
            
            return type switch
            {
                ShaderType.Trail or ShaderType.TrailBloom => ShaderLoader.Trail,
                ShaderType.Bloom or ShaderType.GradientBloom or ShaderType.Flare => ShaderLoader.Bloom,
                // Screen distortion shaders are not yet compiled — return null gracefully
                ShaderType.ChromaticAberration or ShaderType.RadialBlur or ShaderType.Vignette 
                    or ShaderType.ColorFlash or ShaderType.WaveDistortion 
                    or ShaderType.HeatDistortion or ShaderType.RealityCrack => null,
                _ => null
            };
        }

        private static void ConfigureShader(Effect shader, ShaderType type, Color color, 
            float intensity, Color? secondaryColor, Vector2? targetPosition)
        {
            if (shader == null)
                return;

            // Common parameters
            shader.Parameters["uColor"]?.SetValue(color.ToVector3());
            shader.Parameters["uIntensity"]?.SetValue(intensity);
            shader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            shader.Parameters["uOpacity"]?.SetValue(1f);

            // Secondary color for gradients
            if (secondaryColor.HasValue)
                shader.Parameters["uSecondaryColor"]?.SetValue(secondaryColor.Value.ToVector3());
            else
                shader.Parameters["uSecondaryColor"]?.SetValue(color.ToVector3() * 0.5f);

            // Target position for directional effects
            if (targetPosition.HasValue)
                shader.Parameters["uTargetPosition"]?.SetValue(targetPosition.Value);
            else
                shader.Parameters["uTargetPosition"]?.SetValue(new Vector2(0.5f, 0.5f));

            // Set technique/pass based on type
            string passName = type switch
            {
                ShaderType.TrailBloom => "BloomPass",
                ShaderType.GradientBloom => "GradientBloomPass",
                ShaderType.Flare => "FlarePass",
                ShaderType.ChromaticAberration => "ChromaticAberration",
                ShaderType.RadialBlur => "RadialBlur",
                ShaderType.Vignette => "Vignette",
                ShaderType.ColorFlash => "ColorFlash",
                ShaderType.WaveDistortion => "WaveDistortion",
                ShaderType.HeatDistortion => "HeatDistortion",
                ShaderType.RealityCrack => "RealityCrack",
                _ => null
            };

            // Try to select the specific pass
            if (!string.IsNullOrEmpty(passName))
            {
                try
                {
                    shader.CurrentTechnique = shader.Techniques["Technique1"];
                    // Note: XNA/FNA doesn't support selecting passes directly in SpriteBatch
                    // The shader will use the first pass by default
                }
                catch { }
            }
        }

        private static bool IsAdditiveType(ShaderType type)
        {
            return type switch
            {
                ShaderType.Bloom or ShaderType.GradientBloom or ShaderType.Flare 
                    or ShaderType.Trail or ShaderType.TrailBloom => true,
                _ => false
            };
        }

        #endregion

        /// <summary>
        /// Disposable shader scope for using() pattern.
        /// </summary>
        public struct ShaderScope : IDisposable
        {
            public void Dispose()
            {
                EndShaderMode();
            }
        }
    }
}
