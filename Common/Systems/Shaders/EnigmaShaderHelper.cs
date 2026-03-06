using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace MagnumOpus.Common.Systems.Shaders
{
    /// <summary>
    /// Helper for applying Enigma Variations custom shaders via SpriteBatch rendering.
    /// Computes the correct transformation matrix and binds shader parameters.
    /// All methods are safe to call when shaders are unavailable — they gracefully no-op.
    /// </summary>
    public static class EnigmaShaderHelper
    {
        /// <summary>
        /// Draws a shader-processed overlay quad at the given position.
        /// Ends the current SpriteBatch, renders the shader pass, then restores SpriteBatch
        /// to its default AlphaBlend state. Safe to call at the start of PreDraw.
        /// </summary>
        /// <param name="sb">The active SpriteBatch.</param>
        /// <param name="shader">The shader Effect to apply.</param>
        /// <param name="drawTexture">Texture to draw (typically bloom — provides UV space for shader).</param>
        /// <param name="position">Screen-space draw position (already minus Main.screenPosition).</param>
        /// <param name="origin">Texture origin for centering.</param>
        /// <param name="scale">Draw scale.</param>
        /// <param name="primaryColor">Primary color (uColor) as Vector3.</param>
        /// <param name="secondaryColor">Secondary color (uSecondaryColor) as Vector3.</param>
        /// <param name="opacity">Shader opacity (uOpacity). Default 0.5.</param>
        /// <param name="intensity">Shader intensity (uIntensity). Default 1.0.</param>
        /// <param name="rotation">Draw rotation in radians. Default 0.</param>
        /// <param name="noiseTexture">Optional noise texture for sampler slot 1 (uImage1).</param>
        /// <param name="techniqueName">Optional technique name to select. Null = use current.</param>
        public static void DrawShaderOverlay(SpriteBatch sb, Effect shader,
            Texture2D drawTexture, Vector2 position, Vector2 origin, float scale,
            Vector3 primaryColor, Vector3 secondaryColor,
            float opacity = 0.5f, float intensity = 1f, float rotation = 0f,
            Texture2D noiseTexture = null, string techniqueName = null)
        {
            if (shader == null || !ShaderLoader.ShadersEnabled || drawTexture == null)
                return;

            try
            {
                sb.End();

                // Select technique if specified
                if (techniqueName != null)
                {
                    var technique = shader.Techniques[techniqueName];
                    if (technique != null)
                        shader.CurrentTechnique = technique;
                }

                sb.Begin(SpriteSortMode.Immediate, MagnumBlendStates.ShaderAdditive, SamplerState.LinearWrap,
                    DepthStencilState.None, Main.Rasterizer, shader,
                    Main.GameViewMatrix.TransformationMatrix);

                // Compute SpriteBatch-compatible WVP matrix
                var vp = Main.graphics.GraphicsDevice.Viewport;
                Matrix proj;
                Matrix.CreateOrthographicOffCenter(0, vp.Width, vp.Height, 0, 0, 1, out proj);
                shader.Parameters["uWorldViewProjection"]?.SetValue(
                    Main.GameViewMatrix.TransformationMatrix * proj);

                // Set standard shader parameters
                shader.Parameters["uTime"]?.SetValue((float)Main.timeForVisualEffects * 0.016f);
                shader.Parameters["uColor"]?.SetValue(primaryColor);
                shader.Parameters["uSecondaryColor"]?.SetValue(secondaryColor);
                shader.Parameters["uOpacity"]?.SetValue(opacity);
                shader.Parameters["uIntensity"]?.SetValue(intensity);

                // Bind noise texture to sampler slot 1
                if (noiseTexture != null)
                {
                    Main.graphics.GraphicsDevice.Textures[1] = noiseTexture;
                    Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;
                }

                sb.Draw(drawTexture, position, null, Color.White, rotation,
                    origin, scale, SpriteEffects.None, 0f);

                // Restore SpriteBatch to default state
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                    Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer,
                    null, Main.GameViewMatrix.TransformationMatrix);
            }
            catch
            {
                // Recovery: ensure SpriteBatch is in a usable state
                try { sb.End(); } catch { }
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                    Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer,
                    null, Main.GameViewMatrix.TransformationMatrix);
            }
        }

        /// <summary>
        /// Draws two overlapping shader layers for richer visual effects.
        /// First draws the primary technique (larger, lower opacity), then the glow technique (smaller, brighter).
        /// </summary>
        public static void DrawDualShaderOverlay(SpriteBatch sb, Effect shader,
            Texture2D drawTexture, Vector2 position, Vector2 origin,
            float outerScale, float innerScale,
            Vector3 primaryColor, Vector3 secondaryColor,
            string primaryTechnique, string glowTechnique,
            float opacity = 0.5f, float intensity = 1f,
            Texture2D noiseTexture = null)
        {
            // Outer: wider, softer
            DrawShaderOverlay(sb, shader, drawTexture, position, origin, outerScale,
                primaryColor, secondaryColor,
                opacity * 0.6f, intensity, 0f, noiseTexture, primaryTechnique);

            // Inner: tighter, brighter
            DrawShaderOverlay(sb, shader, drawTexture, position, origin, innerScale,
                primaryColor, secondaryColor,
                opacity, intensity * 1.3f, 0f, noiseTexture, glowTechnique);
        }
    }
}
