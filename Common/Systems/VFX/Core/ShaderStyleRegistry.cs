using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// Central registry for all shader styles in MagnumOpus.
    /// Provides 5 unique styles for each shader category:
    /// - Bloom: Ethereal, Infernal, Celestial, Chromatic, Void
    /// - Screen: Ripple, Shatter, Warp, Pulse, Tear
    /// - Trail: Flame, Ice, Lightning, Nature, Cosmic
    /// </summary>
    public class ShaderStyleRegistry : ModSystem
    {
        #region Shader Style Enums

        /// <summary>
        /// Bloom shader visual styles
        /// </summary>
        public enum BloomStyle
        {
            /// <summary>Standard multi-layer bloom</summary>
            Standard,
            /// <summary>Soft, dreamy, gossamer glow with breathing animation</summary>
            Ethereal,
            /// <summary>Harsh, flickering, fire-like bloom with embers</summary>
            Infernal,
            /// <summary>Star-like rays with orbital shimmer</summary>
            Celestial,
            /// <summary>Rainbow prismatic with spectrum cycling</summary>
            Chromatic,
            /// <summary>Dark inner glow with event horizon effect</summary>
            Void
        }

        /// <summary>
        /// Screen distortion effect styles
        /// </summary>
        public enum ScreenStyle
        {
            /// <summary>No screen effect</summary>
            None,
            /// <summary>Water ripple waves from impact point</summary>
            Ripple,
            /// <summary>Reality fracture with crack patterns</summary>
            Shatter,
            /// <summary>Gravitational lens warping</summary>
            Warp,
            /// <summary>Rhythmic heartbeat pulse</summary>
            Pulse,
            /// <summary>Reality tear between two points</summary>
            Tear
        }

        /// <summary>
        /// Trail shader visual styles
        /// </summary>
        public enum TrailStyle
        {
            /// <summary>Standard gradient trail</summary>
            Standard,
            /// <summary>Fire trail with rising embers</summary>
            Flame,
            /// <summary>Crystalline ice with frost particles</summary>
            Ice,
            /// <summary>Electric crackling with arcs</summary>
            Lightning,
            /// <summary>Organic vine/petal with growth</summary>
            Nature,
            /// <summary>Starfield/nebula with constellations</summary>
            Cosmic
        }

        #endregion

        #region Shader References

        private static Effect _advancedBloomShader;
        private static Effect _advancedScreenShader;
        private static Effect _advancedTrailShader;

        private static Asset<Effect> _advancedBloomAsset;
        private static Asset<Effect> _advancedScreenAsset;
        private static Asset<Effect> _advancedTrailAsset;

        #endregion

        #region Theme Mappings

        /// <summary>
        /// Maps each MagnumOpus theme to its recommended shader styles
        /// </summary>
        public static readonly Dictionary<string, ThemeShaderSet> ThemeStyles = new()
        {
            ["LaCampanella"] = new ThemeShaderSet(BloomStyle.Infernal, ScreenStyle.Ripple, TrailStyle.Flame),
            ["Eroica"] = new ThemeShaderSet(BloomStyle.Celestial, ScreenStyle.Pulse, TrailStyle.Flame),
            ["SwanLake"] = new ThemeShaderSet(BloomStyle.Chromatic, ScreenStyle.Shatter, TrailStyle.Ice),
            ["MoonlightSonata"] = new ThemeShaderSet(BloomStyle.Ethereal, ScreenStyle.Ripple, TrailStyle.Cosmic),
            ["Enigma"] = new ThemeShaderSet(BloomStyle.Void, ScreenStyle.Warp, TrailStyle.Lightning),
            ["Fate"] = new ThemeShaderSet(BloomStyle.Void, ScreenStyle.Tear, TrailStyle.Cosmic),
            ["Spring"] = new ThemeShaderSet(BloomStyle.Ethereal, ScreenStyle.Pulse, TrailStyle.Nature),
            ["Summer"] = new ThemeShaderSet(BloomStyle.Infernal, ScreenStyle.Ripple, TrailStyle.Flame),
            ["Autumn"] = new ThemeShaderSet(BloomStyle.Ethereal, ScreenStyle.Shatter, TrailStyle.Nature),
            ["Winter"] = new ThemeShaderSet(BloomStyle.Celestial, ScreenStyle.Shatter, TrailStyle.Ice),
            ["DiesIrae"] = new ThemeShaderSet(BloomStyle.Infernal, ScreenStyle.Shatter, TrailStyle.Flame),
            ["Nachtmusik"] = new ThemeShaderSet(BloomStyle.Celestial, ScreenStyle.Warp, TrailStyle.Cosmic),
            ["OdeToJoy"] = new ThemeShaderSet(BloomStyle.Chromatic, ScreenStyle.Pulse, TrailStyle.Nature),
            ["ClairDeLune"] = new ThemeShaderSet(BloomStyle.Ethereal, ScreenStyle.Ripple, TrailStyle.Ice)
        };

        public struct ThemeShaderSet
        {
            public BloomStyle Bloom;
            public ScreenStyle Screen;
            public TrailStyle Trail;

            public ThemeShaderSet(BloomStyle bloom, ScreenStyle screen, TrailStyle trail)
            {
                Bloom = bloom;
                Screen = screen;
                Trail = trail;
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Shader loading path prefix (no trailing slash - Calamity pattern)
        /// </summary>
        private const string ShaderPath = "Assets/Shaders/";
        
        /// <summary>
        /// Prefix for GameShaders.Misc registration
        /// </summary>
        internal const string ShaderPrefix = "MagnumOpus:";

        /// <summary>
        /// Tracks whether shaders loaded successfully
        /// </summary>
        public static bool ShadersLoaded { get; private set; } = false;

        public override void Load()
        {
            if (Main.dedServ) return;

            // DISABLED: Shader loading is currently disabled because tModLoader/FNA requires
            // shaders compiled with MojoShader, which uses a different bytecode format than
            // standard MonoGame. The VFX system uses particle-based fallbacks instead.
            // 
            // When proper FNA-compatible shaders are available, re-enable LoadShaders().
            ShadersLoaded = false;
            _advancedBloomShader = null;
            _advancedScreenShader = null;
            _advancedTrailShader = null;
            Mod.Logger.Info("ShaderStyleRegistry: Using particle-based VFX (shaders disabled for FNA compatibility)");
        }

        /// <summary>
        /// Loads and registers all shaders (Calamity pattern)
        /// Requires pre-compiled .xnb shader files - NOT raw .fx source
        /// </summary>
        private void LoadShaders()
        {
            // Helper to load shader asset by path (tModLoader 1.4.5+ pattern)
            Asset<Effect> LoadShaderAsset(string path) => 
                Mod.Assets.Request<Effect>($"{ShaderPath}{path}", AssetRequestMode.ImmediateLoad);

            // Helper to register a shader with GameShaders.Misc (using Asset<Effect> to avoid obsolete warning)
            void RegisterMiscShader(Asset<Effect> shaderAsset, string passName, string registrationName)
            {
                MiscShaderData shaderData = new(shaderAsset, passName);
                GameShaders.Misc[$"{ShaderPrefix}{registrationName}"] = shaderData;
            }

            // Load Simple Bloom Shader (PS 2.0 compatible)
            var bloomAsset = LoadShaderAsset("SimpleBloomShader");
            _advancedBloomShader = bloomAsset.Value;
            _advancedBloomAsset = bloomAsset;
            RegisterMiscShader(bloomAsset, "DefaultPass", "DefaultBloom");
            RegisterMiscShader(bloomAsset, "EtherealPass", "EtherealBloom");
            RegisterMiscShader(bloomAsset, "InfernalPass", "InfernalBloom");
            // Note: Celestial, Chromatic, Void bloom styles not available in simplified shader
            // They will fallback to Default or particle-based effects

            // Load Simple Screen Shader (PS 2.0 compatible)
            var screenAsset = LoadShaderAsset("SimpleScreenShader");
            _advancedScreenShader = screenAsset.Value;
            _advancedScreenAsset = screenAsset;
            RegisterMiscShader(screenAsset, "RipplePass", "RippleScreen");
            RegisterMiscShader(screenAsset, "ShatterPass", "ShatterScreen");
            RegisterMiscShader(screenAsset, "WarpPass", "WarpScreen");
            RegisterMiscShader(screenAsset, "PulsePass", "PulseScreen");
            RegisterMiscShader(screenAsset, "TearPass", "TearScreen");

            // Load Simple Trail Shader (PS 2.0 compatible)
            var trailAsset = LoadShaderAsset("SimpleTrailShader");
            _advancedTrailShader = trailAsset.Value;
            _advancedTrailAsset = trailAsset;
            RegisterMiscShader(trailAsset, "FlamePass", "FlameTrail");
            RegisterMiscShader(trailAsset, "IcePass", "IceTrail");
            RegisterMiscShader(trailAsset, "LightningPass", "LightningTrail");
            RegisterMiscShader(trailAsset, "NaturePass", "NatureTrail");
            RegisterMiscShader(trailAsset, "CosmicPass", "CosmicTrail");

            Mod.Logger.Info($"ShaderStyleRegistry: Registered shader passes with GameShaders.Misc (PS 2.0 compatible shaders)");
        }

        public override void Unload()
        {
            _advancedBloomShader = null;
            _advancedScreenShader = null;
            _advancedTrailShader = null;
            _advancedBloomAsset = null;
            _advancedScreenAsset = null;
            _advancedTrailAsset = null;
            ShadersLoaded = false;
        }

        /// <summary>
        /// Gets a registered MiscShaderData by name (Calamity pattern)
        /// </summary>
        public static MiscShaderData GetShader(string name)
        {
            string fullName = $"{ShaderPrefix}{name}";
            if (GameShaders.Misc.TryGetValue(fullName, out var shader))
                return shader;
            return null;
        }

        /// <summary>
        /// Gets the bloom shader Effect directly
        /// </summary>
        private static Effect GetBloomShader() => _advancedBloomShader;

        /// <summary>
        /// Gets the screen shader Effect directly
        /// </summary>
        private static Effect GetScreenShader() => _advancedScreenShader;

        /// <summary>
        /// Gets the trail shader Effect directly
        /// </summary>
        private static Effect GetTrailShader() => _advancedTrailShader;

        #endregion

        #region Bloom Shader Application

        /// <summary>
        /// Gets the shader styles for a theme, with fallback to standard if not found
        /// </summary>
        public static ThemeShaderSet GetThemeStyles(string theme)
        {
            if (ThemeStyles.TryGetValue(theme, out var styles))
                return styles;
            return new ThemeShaderSet(BloomStyle.Standard, ScreenStyle.None, TrailStyle.Standard);
        }

        /// <summary>
        /// Applies a bloom shader style and returns the effect for drawing.
        /// </summary>
        public static Effect ApplyBloomStyle(BloomStyle style, Color primaryColor, Color secondaryColor,
            float intensity = 1f, float time = -1f, float pulseSpeed = 0f, float pulseIntensity = 0.1f)
        {
            Effect shader = GetBloomShader();
            if (shader == null) return null;

            if (time < 0) time = (float)Main.gameTimeCache.TotalGameTime.TotalSeconds;

            try
            {
                // Set common parameters
                shader.Parameters["uColor"]?.SetValue(primaryColor.ToVector3());
                shader.Parameters["uSecondaryColor"]?.SetValue(secondaryColor.ToVector3());
                shader.Parameters["uOpacity"]?.SetValue(1f);
                shader.Parameters["uTime"]?.SetValue(time);
                shader.Parameters["uIntensity"]?.SetValue(intensity);
                shader.Parameters["uPulseSpeed"]?.SetValue(pulseSpeed);
                shader.Parameters["uPulseIntensity"]?.SetValue(pulseIntensity);

                // Select technique based on style
                string techniqueName = style switch
                {
                    BloomStyle.Ethereal => "EtherealTechnique",
                    BloomStyle.Infernal => "InfernalTechnique",
                    BloomStyle.Celestial => "CelestialTechnique",
                    BloomStyle.Chromatic => "ChromaticTechnique",
                    BloomStyle.Void => "VoidTechnique",
                    _ => "EtherealTechnique" // Default to Ethereal for Standard
                };

                shader.CurrentTechnique = shader.Techniques[techniqueName];
            }
            catch (Exception)
            {
                return null;
            }

            return shader;
        }

        /// <summary>
        /// Applies bloom style based on theme name
        /// </summary>
        public static Effect ApplyThemeBloom(string theme, Color? primaryOverride = null, Color? secondaryOverride = null, float intensity = 1f)
        {
            var styles = GetThemeStyles(theme);
            var colors = MagnumThemePalettes.GetThemePalette(theme);
            
            Color primary = primaryOverride ?? (colors.Length > 0 ? colors[0] : Color.White);
            Color secondary = secondaryOverride ?? (colors.Length > 1 ? colors[1] : primary);

            return ApplyBloomStyle(styles.Bloom, primary, secondary, intensity);
        }

        #endregion

        #region Screen Shader Application

        /// <summary>
        /// Applies a screen distortion effect.
        /// </summary>
        public static Effect ApplyScreenStyle(ScreenStyle style, Vector2 targetPosition, 
            Color primaryColor, Color secondaryColor, float intensity = 1f, float progress = 1f, 
            float radius = 0.3f, Vector2? secondaryPosition = null)
        {
            Effect shader = GetScreenShader();
            if (shader == null) return null;

            float time = (float)Main.gameTimeCache.TotalGameTime.TotalSeconds;

            // Convert world position to screen-space (0-1)
            Vector2 screenPos = (targetPosition - Main.screenPosition) / new Vector2(Main.screenWidth, Main.screenHeight);

            try
            {
                shader.Parameters["uScreenResolution"]?.SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
                shader.Parameters["uTargetPosition"]?.SetValue(screenPos);
                shader.Parameters["uSecondaryPosition"]?.SetValue(secondaryPosition.HasValue 
                    ? (secondaryPosition.Value - Main.screenPosition) / new Vector2(Main.screenWidth, Main.screenHeight)
                    : screenPos + new Vector2(0.2f, 0f));
                shader.Parameters["uIntensity"]?.SetValue(intensity);
                shader.Parameters["uTime"]?.SetValue(time);
                shader.Parameters["uProgress"]?.SetValue(progress);
                shader.Parameters["uColor"]?.SetValue(primaryColor.ToVector3());
                shader.Parameters["uSecondaryColor"]?.SetValue(secondaryColor.ToVector3());
                shader.Parameters["uRadius"]?.SetValue(radius);
                
                // ============================================
                // VFXTextureRegistry Integration for Screen FX
                // ============================================
                // Set mask textures for proper distortion effects
                // - RippleRing for Ripple and Pulse styles
                // - EclipseRing for Warp and Tear styles
                // - Noise for Shatter style
                switch (style)
                {
                    case ScreenStyle.Ripple:
                    case ScreenStyle.Pulse:
                        shader.Parameters["uMaskTexture"]?.SetValue(VFXTextureRegistry.Mask.RippleRing);
                        break;
                    case ScreenStyle.Warp:
                    case ScreenStyle.Tear:
                        shader.Parameters["uMaskTexture"]?.SetValue(VFXTextureRegistry.Mask.EclipseRing);
                        break;
                    case ScreenStyle.Shatter:
                        shader.Parameters["uMaskTexture"]?.SetValue(VFXTextureRegistry.Noise.Smoke);
                        break;
                }
                shader.Parameters["uNoiseTexture"]?.SetValue(VFXTextureRegistry.Noise.Smoke);

                string techniqueName = style switch
                {
                    ScreenStyle.Ripple => "RippleTechnique",
                    ScreenStyle.Shatter => "ShatterTechnique",
                    ScreenStyle.Warp => "WarpTechnique",
                    ScreenStyle.Pulse => "PulseTechnique",
                    ScreenStyle.Tear => "TearTechnique",
                    _ => null
                };

                if (techniqueName != null)
                    shader.CurrentTechnique = shader.Techniques[techniqueName];
            }
            catch (Exception)
            {
                return null;
            }

            return shader;
        }

        /// <summary>
        /// Applies screen style based on theme name
        /// </summary>
        public static Effect ApplyThemeScreen(string theme, Vector2 targetPosition, float intensity = 1f, float progress = 1f)
        {
            var styles = GetThemeStyles(theme);
            if (styles.Screen == ScreenStyle.None) return null;

            var colors = MagnumThemePalettes.GetThemePalette(theme);
            Color primary = colors.Length > 0 ? colors[0] : Color.White;
            Color secondary = colors.Length > 1 ? colors[1] : primary;

            return ApplyScreenStyle(styles.Screen, targetPosition, primary, secondary, intensity, progress);
        }

        #endregion

        #region Trail Shader Application

        /// <summary>
        /// Applies a trail shader style.
        /// </summary>
        public static Effect ApplyTrailStyle(TrailStyle style, Color primaryColor, Color secondaryColor,
            Color? tertiaryColor = null, float intensity = 1f, float trailLength = 1f)
        {
            Effect shader = GetTrailShader();
            if (shader == null) return null;

            float time = (float)Main.gameTimeCache.TotalGameTime.TotalSeconds;

            try
            {
                shader.Parameters["uColor"]?.SetValue(primaryColor.ToVector3());
                shader.Parameters["uSecondaryColor"]?.SetValue(secondaryColor.ToVector3());
                shader.Parameters["uTertiaryColor"]?.SetValue((tertiaryColor ?? secondaryColor).ToVector3());
                shader.Parameters["uOpacity"]?.SetValue(1f);
                shader.Parameters["uTime"]?.SetValue(time);
                shader.Parameters["uIntensity"]?.SetValue(intensity);
                shader.Parameters["uTrailLength"]?.SetValue(trailLength);

                string techniqueName = style switch
                {
                    TrailStyle.Flame => "FlameTechnique",
                    TrailStyle.Ice => "IceTechnique",
                    TrailStyle.Lightning => "LightningTechnique",
                    TrailStyle.Nature => "NatureTechnique",
                    TrailStyle.Cosmic => "CosmicTechnique",
                    _ => "FlameTechnique" // Default
                };

                shader.CurrentTechnique = shader.Techniques[techniqueName];
            }
            catch (Exception)
            {
                return null;
            }

            return shader;
        }

        /// <summary>
        /// Applies trail style based on theme name
        /// </summary>
        public static Effect ApplyThemeTrail(string theme, float intensity = 1f)
        {
            var styles = GetThemeStyles(theme);
            var colors = MagnumThemePalettes.GetThemePalette(theme);

            Color primary = colors.Length > 0 ? colors[0] : Color.White;
            Color secondary = colors.Length > 1 ? colors[1] : primary;
            Color tertiary = colors.Length > 2 ? colors[2] : secondary;

            return ApplyTrailStyle(styles.Trail, primary, secondary, tertiary, intensity);
        }

        #endregion
    }
}
