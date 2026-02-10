using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems.Shaders
{
    /// <summary>
    /// MAGNUM OPUS SHADER SYSTEM
    /// 
    /// Properly implements shader loading following the 5 Calamity-standard requirements:
    /// 
    /// 1. COMPILATION BARRIER:
    ///    - Shaders in Assets/Shaders/ must be compiled .xnb files
    ///    - Use TMLShaderCompiler or EasyShader to compile .fx → .xnb
    ///    - OR tModLoader 1.4.4+ auto-compiles .fx files if properly configured
    /// 
    /// 2. LOADING INTO MEMORY:
    ///    - Use Asset&lt;Effect&gt; with ModContent.Request&lt;Effect&gt; (1.4.5+ pattern)
    ///    - Register with Filters.Scene for screen shaders
    ///    - Register with GameShaders.Misc for item/projectile shaders
    /// 
    /// 3. PARAMETER PASSING:
    ///    - Update uTime, uWorldPosition, uColor per-frame
    ///    - Use MiscShaderData.UseColor(), UseOpacity(), etc.
    /// 
    /// 4. COMMON PITFALLS:
    ///    - Coordinate system: (0,0) = top-left in Terraria
    ///    - Pass names must match exactly between C# and .fx
    ///    - Use SamplerState.LinearClamp for smooth visuals
    /// 
    /// 5. DRAW HOOKS:
    ///    - Use PostDraw/ModifyInterfaceLayers for complex effects
    ///    - Capture render targets for screen-wide effects
    /// </summary>
    public class MagnumShaderSystem : ModSystem
    {
        // Shader registration names
        public const string BossAuraShader = "MagnumOpus:BossAura";
        public const string TrailShader = "MagnumOpus:Trail";
        public const string BloomShader = "MagnumOpus:Bloom";
        public const string ScreenDistortionShader = "MagnumOpus:ScreenDistortion";
        public const string ChromaticAberrationShader = "MagnumOpus:ChromaticAberration";
        
        // Asset<Effect> holders - modern tModLoader 1.4.5+ pattern
        private static Asset<Effect> _trailShaderAsset;
        private static Asset<Effect> _bloomShaderAsset;
        private static Asset<Effect> _screenShaderAsset;
        
        // Cached MiscShaderData for per-frame updates
        private static MiscShaderData _trailShaderData;
        private static MiscShaderData _bloomShaderData;
        
        // Initialization state
        private static bool _initialized;
        private static bool _shadersAvailable;
        
        /// <summary>
        /// Whether shaders are available and properly loaded.
        /// </summary>
        public static bool ShadersAvailable => _initialized && _shadersAvailable;
        
        public override void Load()
        {
            if (Main.dedServ)
                return;
            
            _initialized = false;
            _shadersAvailable = false;
        }
        
        public override void PostSetupContent()
        {
            if (Main.dedServ)
                return;
            
            InitializeShaders();
        }
        
        public override void Unload()
        {
            _trailShaderAsset = null;
            _bloomShaderAsset = null;
            _screenShaderAsset = null;
            _trailShaderData = null;
            _bloomShaderData = null;
            _initialized = false;
            _shadersAvailable = false;
        }
        
        /// <summary>
        /// Initialize and register all shaders.
        /// Uses Asset&lt;Effect&gt; pattern for tModLoader 1.4.5+ compatibility.
        /// 
        /// NOTE: Shader loading is currently DISABLED because there are no compiled
        /// .xnb shader files. The VFX system will use particle-based fallback rendering
        /// which still looks great! To enable shaders, compile the .fx files in 
        /// ShaderSource/ folder using proper FNA-compatible tools.
        /// </summary>
        private void InitializeShaders()
        {
            if (_initialized)
                return;
            
            _initialized = true;
            _shadersAvailable = false;
            
            // DISABLED: Shader loading is disabled until properly compiled .xnb files exist
            // The VFX system uses particle-based fallback which looks great without shaders
            Mod.Logger.Info("MagnumShaderSystem: Using particle-based VFX (no compiled shaders).");
            
            // NOTE: To enable shaders:
            // 1. Compile .fx files from ShaderSource/ using FNA-compatible tools
            // 2. Place compiled .xnb files in Assets/Shaders/
            // 3. Uncomment the shader loading code below
            
            /*
            // Attempt to load shaders - tModLoader may auto-compile .fx files
            int loadedCount = 0;
            
            // Try loading Trail shader
            if (TryLoadShader("MagnumOpus/Assets/Shaders/TrailShader", out _trailShaderAsset))
            {
                try
                {
                    _trailShaderData = new MiscShaderData(_trailShaderAsset, "Pass1");
                    GameShaders.Misc[TrailShader] = _trailShaderData;
                    loadedCount++;
                }
                catch (Exception ex)
                {
                    Mod.Logger.Debug($"MagnumShaderSystem: Trail shader pass setup failed - {ex.Message}");
                }
            }
            
            // Try loading Bloom shader
            if (TryLoadShader("MagnumOpus/Assets/Shaders/SimpleBloomShader", out _bloomShaderAsset))
            {
                try
                {
                    _bloomShaderData = new MiscShaderData(_bloomShaderAsset, "Pass1");
                    GameShaders.Misc[BloomShader] = _bloomShaderData;
                    loadedCount++;
                }
                catch (Exception ex)
                {
                    Mod.Logger.Debug($"MagnumShaderSystem: Bloom shader pass setup failed - {ex.Message}");
                }
            }
            
            // Try loading Screen shader
            if (TryLoadShader("MagnumOpus/Assets/Shaders/ScreenEffectsShader", out _screenShaderAsset))
            {
                loadedCount++;
            }
            
            // Update availability based on loaded shaders
            _shadersAvailable = loadedCount > 0;
            
            if (_shadersAvailable)
            {
                Mod.Logger.Info($"MagnumShaderSystem: Loaded {loadedCount} shader(s) successfully.");
            }
            else
            {
                Mod.Logger.Info("MagnumShaderSystem: No shaders loaded - using particle-based VFX fallback.");
                Mod.Logger.Debug("MagnumShaderSystem: (Place compiled .xnb shaders in Assets/Shaders/ or ensure .fx files compile)");
            }
            */
        }
        
        /// <summary>
        /// Attempts to load a shader from the given path.
        /// Uses Asset&lt;Effect&gt; for tModLoader 1.4.5+ compatibility.
        /// Returns false if shader doesn't exist (no compiled .xnb file).
        /// </summary>
        private bool TryLoadShader(string path, out Asset<Effect> shaderAsset)
        {
            shaderAsset = null;
            
            try
            {
                // Use AsyncLoad mode which doesn't throw immediately if asset doesn't exist
                shaderAsset = ModContent.Request<Effect>(path, AssetRequestMode.AsyncLoad);
                
                // Check if asset actually exists and can be loaded
                if (shaderAsset != null && shaderAsset.State != AssetState.NotLoaded)
                {
                    // Wait for it to actually load
                    shaderAsset.Wait();
                    if (shaderAsset.Value != null)
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                // Shader not found or not compiled - this is expected if .fx files aren't compiled
                Mod.Logger.Debug($"MagnumShaderSystem: Could not load '{path}' - {ex.Message}");
            }
            
            shaderAsset = null;
            return false;
        }
        
        #region Per-Frame Shader Updates
        
        public override void PostUpdateEverything()
        {
            if (Main.dedServ || !_shadersAvailable)
                return;
            
            // Update shaders with current time for animations
            UpdateShaderTime();
        }
        
        /// <summary>
        /// Updates time-based shader parameters every frame.
        /// </summary>
        private void UpdateShaderTime()
        {
            float time = Main.GlobalTimeWrappedHourly;
            
            // Update Trail shader
            if (_trailShaderData != null && _trailShaderAsset?.Value != null)
            {
                _trailShaderAsset.Value.Parameters["uTime"]?.SetValue(time);
            }
            
            // Update Bloom shader
            if (_bloomShaderData != null && _bloomShaderAsset?.Value != null)
            {
                _bloomShaderAsset.Value.Parameters["uTime"]?.SetValue(time);
            }
            
            // Update Screen shader
            if (_screenShaderAsset?.Value != null)
            {
                _screenShaderAsset.Value.Parameters["uTime"]?.SetValue(time);
            }
        }
        
        #endregion
        
        #region Public API - Shader Access
        
        /// <summary>
        /// Gets the trail shader data for projectile/weapon trails.
        /// Updates color and applies the shader.
        /// </summary>
        /// <param name="primaryColor">Primary trail color</param>
        /// <param name="secondaryColor">Secondary/gradient color</param>
        /// <param name="intensity">Effect intensity (0-2)</param>
        /// <returns>The configured MiscShaderData, or null if unavailable</returns>
        public static MiscShaderData GetTrailShader(Color primaryColor, Color secondaryColor, float intensity = 1f)
        {
            if (!_shadersAvailable || _trailShaderData == null)
                return null;
            
            // Configure per Calamity pattern
            _trailShaderData.UseColor(primaryColor);
            _trailShaderData.UseSecondaryColor(secondaryColor);
            _trailShaderData.UseOpacity(intensity);
            
            // Set additional parameters directly
            if (_trailShaderAsset?.Value != null)
            {
                _trailShaderAsset.Value.Parameters["uIntensity"]?.SetValue(intensity);
            }
            
            return _trailShaderData;
        }
        
        /// <summary>
        /// Gets the bloom shader data for glow effects.
        /// </summary>
        public static MiscShaderData GetBloomShader(Color glowColor, float intensity = 1f, float opacity = 1f)
        {
            if (!_shadersAvailable || _bloomShaderData == null)
                return null;
            
            _bloomShaderData.UseColor(glowColor);
            _bloomShaderData.UseOpacity(opacity);
            
            if (_bloomShaderAsset?.Value != null)
            {
                _bloomShaderAsset.Value.Parameters["uIntensity"]?.SetValue(intensity);
            }
            
            return _bloomShaderData;
        }
        
        /// <summary>
        /// Activates the screen distortion filter.
        /// </summary>
        /// <param name="worldPosition">World position of the distortion center</param>
        /// <param name="intensity">Distortion intensity</param>
        /// <param name="duration">Duration in frames</param>
        public static void ActivateScreenDistortion(Vector2 worldPosition, float intensity, int duration)
        {
            if (!_shadersAvailable)
                return;
            
            if (!Filters.Scene[ScreenDistortionShader].IsActive())
            {
                Filters.Scene.Activate(ScreenDistortionShader, worldPosition);
            }
            
            // Convert world position to screen-relative (0,0 = top-left)
            Vector2 screenPos = worldPosition - Main.screenPosition;
            Vector2 normalizedPos = screenPos / new Vector2(Main.screenWidth, Main.screenHeight);
            
            if (_screenShaderAsset?.Value != null)
            {
                _screenShaderAsset.Value.Parameters["uIntensity"]?.SetValue(intensity);
                _screenShaderAsset.Value.Parameters["uTargetPosition"]?.SetValue(normalizedPos);
            }
        }
        
        /// <summary>
        /// Deactivates the screen distortion filter.
        /// </summary>
        public static void DeactivateScreenDistortion()
        {
            if (Filters.Scene[ScreenDistortionShader]?.IsActive() == true)
            {
                Filters.Scene.Deactivate(ScreenDistortionShader);
            }
        }
        
        #endregion
        
        #region Draw Helpers
        
        /// <summary>
        /// Begins a shader-enhanced drawing batch.
        /// Use this for complex effects that need custom blending.
        /// </summary>
        /// <param name="spriteBatch">The active SpriteBatch</param>
        /// <param name="shader">The shader to apply (null for no shader)</param>
        /// <param name="blendState">Blend state (default: Additive for bloom)</param>
        public static void BeginShaderBatch(
            SpriteBatch spriteBatch, 
            Effect shader = null,
            BlendState blendState = null)
        {
            spriteBatch.End();
            spriteBatch.Begin(
                SpriteSortMode.Immediate, // Required for shader effects
                blendState ?? BlendState.Additive,
                SamplerState.LinearClamp, // Calamity standard: smooth visuals
                DepthStencilState.None,
                RasterizerState.CullNone,
                shader,
                Main.GameViewMatrix.TransformationMatrix);
        }
        
        /// <summary>
        /// Ends a shader batch and restores normal drawing.
        /// </summary>
        public static void EndShaderBatch(SpriteBatch spriteBatch)
        {
            spriteBatch.End();
            spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.LinearClamp,
                DepthStencilState.None,
                RasterizerState.CullNone,
                null,
                Main.GameViewMatrix.TransformationMatrix);
        }
        
        /// <summary>
        /// Applies a shader to draw a texture with bloom effect.
        /// Falls back to additive blending if shader unavailable.
        /// </summary>
        public static void DrawWithBloom(
            SpriteBatch spriteBatch,
            Texture2D texture,
            Vector2 position,
            Rectangle? sourceRect,
            Color color,
            float rotation,
            Vector2 origin,
            float scale,
            float bloomIntensity = 1f)
        {
            // Get bloom shader if available
            var shaderData = GetBloomShader(color, bloomIntensity);
            Effect shader = shaderData != null ? _bloomShaderAsset?.Value : null;
            
            // Begin shader batch
            BeginShaderBatch(spriteBatch, shader, BlendState.Additive);
            
            // Apply shader if available
            shaderData?.Apply();
            
            // Draw bloom layers (Calamity pattern: multiple layers at different scales)
            float[] scales = { 1.4f, 1.2f, 1.05f };
            float[] opacities = { 0.15f, 0.25f, 0.4f };
            
            for (int i = 0; i < 3; i++)
            {
                Color layerColor = color with { A = 0 } * opacities[i] * bloomIntensity;
                spriteBatch.Draw(
                    texture, position, sourceRect, layerColor,
                    rotation, origin, scale * scales[i], SpriteEffects.None, 0f);
            }
            
            // End shader batch
            EndShaderBatch(spriteBatch);
        }
        
        #endregion
    }
}
