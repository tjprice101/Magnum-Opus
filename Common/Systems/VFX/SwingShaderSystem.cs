using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// SWING SHADER SYSTEM
    /// 
    /// Manages shaders for Exoblade-style swing effects:
    /// - SwingSpriteShader: Blade deformation during swing
    /// - ExobladeSlashShader: Trail slash effect with noise
    /// 
    /// Provides fallback rendering when shaders aren't available.
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public sealed class SwingShaderSystem : ModSystem
    {
        private const string ShaderPath = "Effects/";
        private const string ShaderPrefix = "MagnumOpus:";

        /// <summary>
        /// SwingSprite shader for blade deformation.
        /// </summary>
        internal static Asset<Effect> SwingSpriteShader;

        /// <summary>
        /// ExobladeSlash shader for trail rendering.
        /// </summary>
        internal static Asset<Effect> ExobladeSlashShader;

        /// <summary>
        /// Whether shaders loaded successfully.
        /// </summary>
        public static bool ShadersAvailable { get; private set; }

        /// <summary>
        /// Noise texture for slash shader.
        /// </summary>
        public static Texture2D NoiseTexture { get; private set; }

        public override void Load()
        {
            // Shaders are loaded in PostSetupContent after all content is available
        }

        public override void PostSetupContent()
        {
            if (Main.dedServ)
                return;

            // Try to load noise texture for shaders
            try
            {
                if (ModContent.HasAsset("MagnumOpus/Assets/VFX/Noise/VoronoiNoise"))
                {
                    var noiseTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX/Noise/VoronoiNoise", AssetRequestMode.ImmediateLoad);
                    if (noiseTex?.Value != null)
                        NoiseTexture = noiseTex.Value;
                }
            }
            catch
            {
                // Use fallback noise
            }

            // Attempt to load swing shader from Effects/ (tModLoader auto-compiles .fx)
            // Check existence BEFORE requesting to avoid fatal tracked-asset failure.
            try
            {
                if (ModContent.HasAsset("MagnumOpus/Effects/TerraBladeSwingVFX"))
                {
                    SwingSpriteShader = ModContent.Request<Effect>("MagnumOpus/Effects/TerraBladeSwingVFX", AssetRequestMode.ImmediateLoad);
                    ShadersAvailable = SwingSpriteShader?.Value != null;
                }
                else
                {
                    ShadersAvailable = false;
                }
            }
            catch
            {
                ShadersAvailable = false;
            }

            if (ShadersAvailable)
                Mod.Logger.Info("SwingShaderSystem: Swing shader loaded successfully.");
            else
                Mod.Logger.Info("SwingShaderSystem: Swing shader not available — using fallback rendering.");
        }

        public override void Unload()
        {
            SwingSpriteShader = null;
            ExobladeSlashShader = null;
            NoiseTexture = null;
        }

        /// <summary>
        /// Apply swing sprite shader for blade deformation.
        /// Returns true if shader was applied, false if using fallback.
        /// </summary>
        public static bool ApplySwingShader(SpriteBatch spriteBatch, float rotation, float pommelPercent = 0.05f, Color? color = null)
        {
            if (!ShadersAvailable || SwingSpriteShader?.Value == null)
                return false;

            try
            {
                Effect fx = SwingSpriteShader.Value;
                fx.Parameters["rotation"]?.SetValue(rotation);
                fx.Parameters["pommelToOriginPercent"]?.SetValue(pommelPercent);
                fx.Parameters["color"]?.SetValue((color ?? Color.White).ToVector4());

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, Main.Rasterizer, fx, Main.GameViewMatrix.TransformationMatrix);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Restore spritebatch after shader use.
        /// </summary>
        public static void RestoreSpriteBatch(SpriteBatch spriteBatch)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Begin additive blending for glow effects.
        /// </summary>
        public static void BeginAdditive(SpriteBatch spriteBatch)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Get Exoblade-style color palette for rainbow cycling.
        /// Based on Calamity's MulticolorLerp: Cyan→Lime→GreenYellow→Goldenrod→Orange
        /// </summary>
        public static Color GetExobladeColor(float progress)
        {
            Color[] palette = new Color[]
            {
                Color.Cyan,
                Color.Lime,
                Color.GreenYellow,
                Color.Goldenrod,
                Color.Orange
            };

            float scaledProgress = progress * (palette.Length - 1);
            int startIndex = (int)scaledProgress;
            int endIndex = System.Math.Min(startIndex + 1, palette.Length - 1);
            float lerpAmount = scaledProgress - startIndex;

            return Color.Lerp(palette[startIndex], palette[endIndex], lerpAmount);
        }
    }
}
