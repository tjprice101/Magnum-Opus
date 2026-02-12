using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// RADIAL SCROLL SYSTEM
    /// ====================
    /// 
    /// High-performance radial scrolling effect system for:
    /// - Magic orbs and energy spheres
    /// - Portal/vortex effects
    /// - Weapon auras and enchantments
    /// - Boss arena ambient effects
    /// - Projectile energy cores
    /// 
    /// Based on VFX+ mod patterns with MagnumOpus theme integration.
    /// 
    /// USAGE:
    /// 1. Call RadialScrollSystem.DrawOrb() for simple usage
    /// 2. Use RadialScrollSystem.ApplyEffect() for manual control
    /// 3. Use themed presets for consistent visual identity
    /// </summary>
    public class RadialScrollSystem : ModSystem
    {
        #region Singleton
        
        public static RadialScrollSystem Instance { get; private set; }
        
        #endregion
        
        #region Shader Assets
        
        private static Effect _radialScrollEffect;
        private static bool _shaderLoaded = false;
        
        // Technique names matching RadialScrollShader.fx
        public const string TECH_BASIC = "RadialBasic";
        public const string TECH_DUAL_PHASE = "DualPhase";
        public const string TECH_DISTORTED = "Distorted";
        public const string TECH_MULTI_LAYER = "MultiLayer";
        public const string TECH_GRADIENT = "GradientMapped";
        public const string TECH_PULSING = "PulsingOrb";
        public const string TECH_VORTEX = "Vortex";
        
        #endregion
        
        #region Configuration Presets
        
        /// <summary>
        /// Predefined radial scroll configurations for different use cases.
        /// </summary>
        public static class Presets
        {
            // === GENERIC PRESETS ===
            
            public static RadialScrollConfig DefaultOrb => new RadialScrollConfig
            {
                FlowSpeed = 0.5f,
                RadialSpeed = 0.2f,
                DistortStrength = 0.02f,
                Zoom = 1.0f,
                Repeat = 1.0f,
                VignetteSize = 0.5f,
                VignetteBlend = 0.2f,
                Technique = TECH_DUAL_PHASE
            };
            
            public static RadialScrollConfig EnergyOrb => new RadialScrollConfig
            {
                FlowSpeed = 0.8f,
                RadialSpeed = 0.3f,
                DistortStrength = 0.03f,
                Zoom = 1.2f,
                Repeat = 1.0f,
                VignetteSize = 0.45f,
                VignetteBlend = 0.15f,
                PulseSpeed = 3.0f,
                PulseAmount = 0.15f,
                Technique = TECH_PULSING
            };
            
            public static RadialScrollConfig Portal => new RadialScrollConfig
            {
                FlowSpeed = 1.2f,
                RadialSpeed = 0.5f,
                DistortStrength = 0.15f,
                Zoom = 0.8f,
                Repeat = 1.0f,
                VignetteSize = 0.6f,
                VignetteBlend = 0.1f,
                Technique = TECH_VORTEX
            };
            
            public static RadialScrollConfig WeaponAura => new RadialScrollConfig
            {
                FlowSpeed = 0.4f,
                RadialSpeed = 0.1f,
                DistortStrength = 0.01f,
                Zoom = 2.0f,
                Repeat = 2.0f,
                VignetteSize = 0.7f,
                VignetteBlend = 0.3f,
                Technique = TECH_BASIC
            };
            
            public static RadialScrollConfig AmbientField => new RadialScrollConfig
            {
                FlowSpeed = 0.2f,
                RadialSpeed = 0.05f,
                DistortStrength = 0.02f,
                Zoom = 0.5f,
                Repeat = 1.0f,
                VignetteSize = 0.8f,
                VignetteBlend = 0.4f,
                Technique = TECH_MULTI_LAYER
            };
            
            // === THEME-SPECIFIC PRESETS ===
            
            public static RadialScrollConfig LaCampanella => new RadialScrollConfig
            {
                FlowSpeed = 0.6f,
                RadialSpeed = 0.25f,
                DistortStrength = 0.04f,
                Zoom = 1.0f,
                Repeat = 1.0f,
                VignetteSize = 0.5f,
                VignetteBlend = 0.2f,
                PulseSpeed = 2.0f,
                PulseAmount = 0.1f,
                PrimaryColor = MagnumThemePalettes.CampanellaOrange,
                SecondaryColor = MagnumThemePalettes.CampanellaGold,
                Technique = TECH_PULSING
            };
            
            public static RadialScrollConfig Eroica => new RadialScrollConfig
            {
                FlowSpeed = 0.7f,
                RadialSpeed = 0.3f,
                DistortStrength = 0.03f,
                Zoom = 1.1f,
                Repeat = 1.0f,
                VignetteSize = 0.5f,
                VignetteBlend = 0.15f,
                PulseSpeed = 2.5f,
                PulseAmount = 0.12f,
                PrimaryColor = MagnumThemePalettes.EroicaScarlet,
                SecondaryColor = MagnumThemePalettes.EroicaGold,
                Technique = TECH_PULSING
            };
            
            public static RadialScrollConfig MoonlightSonata => new RadialScrollConfig
            {
                FlowSpeed = 0.3f,
                RadialSpeed = 0.1f,
                DistortStrength = 0.02f,
                Zoom = 0.9f,
                Repeat = 1.0f,
                VignetteSize = 0.55f,
                VignetteBlend = 0.25f,
                PulseSpeed = 1.5f,
                PulseAmount = 0.08f,
                PrimaryColor = MagnumThemePalettes.MoonlightPurple,
                SecondaryColor = MagnumThemePalettes.MoonlightIceBlue,
                Technique = TECH_DUAL_PHASE
            };
            
            public static RadialScrollConfig SwanLake => new RadialScrollConfig
            {
                FlowSpeed = 0.25f,
                RadialSpeed = 0.08f,
                DistortStrength = 0.015f,
                Zoom = 1.0f,
                Repeat = 1.0f,
                VignetteSize = 0.5f,
                VignetteBlend = 0.2f,
                PulseSpeed = 1.0f,
                PulseAmount = 0.05f,
                PrimaryColor = MagnumThemePalettes.SwanWhite,
                SecondaryColor = MagnumThemePalettes.SwanIcyBlue,
                Technique = TECH_DUAL_PHASE
            };
            
            public static RadialScrollConfig EnigmaVariations => new RadialScrollConfig
            {
                FlowSpeed = 0.5f,
                RadialSpeed = 0.2f,
                DistortStrength = 0.05f,
                Zoom = 1.2f,
                Repeat = 1.0f,
                VignetteSize = 0.45f,
                VignetteBlend = 0.15f,
                PulseSpeed = 2.0f,
                PulseAmount = 0.1f,
                PrimaryColor = MagnumThemePalettes.EnigmaPurple,
                SecondaryColor = MagnumThemePalettes.EnigmaGreen,
                Technique = TECH_DISTORTED
            };
            
            public static RadialScrollConfig Fate => new RadialScrollConfig
            {
                FlowSpeed = 0.8f,
                RadialSpeed = 0.35f,
                DistortStrength = 0.06f,
                Zoom = 1.3f,
                Repeat = 1.0f,
                VignetteSize = 0.5f,
                VignetteBlend = 0.18f,
                PulseSpeed = 3.0f,
                PulseAmount = 0.15f,
                PrimaryColor = MagnumThemePalettes.FateBrightRed,
                SecondaryColor = MagnumThemePalettes.FateWhite,
                Technique = TECH_PULSING
            };
            
            public static RadialScrollConfig FatePortal => new RadialScrollConfig
            {
                FlowSpeed = 1.5f,
                RadialSpeed = 0.6f,
                DistortStrength = 0.2f,
                Zoom = 0.7f,
                Repeat = 1.0f,
                VignetteSize = 0.6f,
                VignetteBlend = 0.1f,
                PrimaryColor = MagnumThemePalettes.FatePurple,
                SecondaryColor = MagnumThemePalettes.FatePink,
                Technique = TECH_VORTEX
            };
        }
        
        #endregion
        
        #region Lifecycle
        
        public override void Load()
        {
            Instance = this;
            
            // Shader will be loaded on first use (lazy loading)
            _shaderLoaded = false;
        }
        
        public override void Unload()
        {
            Instance = null;
            _radialScrollEffect = null;
            _shaderLoaded = false;
        }
        
        private static bool EnsureShaderLoaded()
        {
            if (_shaderLoaded) return _radialScrollEffect != null;
            
            _shaderLoaded = true;
            
            try
            {
                // Try to load compiled shader
                // Note: You'll need to compile RadialScrollShader.fx to .xnb
                if (ModContent.HasAsset("MagnumOpus/Assets/Shaders/RadialScrollShader"))
                {
                    _radialScrollEffect = ModContent.Request<Effect>(
                        "MagnumOpus/Assets/Shaders/RadialScrollShader",
                        AssetRequestMode.ImmediateLoad
                    ).Value;
                    return true;
                }
            }
            catch (Exception ex)
            {
                // Shader not available - use fallback
                Mod mod = ModLoader.GetMod("MagnumOpus");
                mod?.Logger.Warn($"RadialScrollShader not loaded: {ex.Message}");
            }
            
            return false;
        }
        
        #endregion
        
        #region Public API - Easy Usage
        
        /// <summary>
        /// Draw a radial scroll orb effect at the specified position.
        /// This is the easiest way to use the system.
        /// </summary>
        /// <param name="position">World position (will be converted to screen space)</param>
        /// <param name="size">Orb diameter in pixels</param>
        /// <param name="config">Configuration preset (use Presets.* or custom)</param>
        /// <param name="noiseTexture">Noise texture to use (null = default)</param>
        public static void DrawOrb(
            Vector2 position,
            float size,
            RadialScrollConfig config,
            Texture2D noiseTexture = null)
        {
            if (!EnsureShaderLoaded())
            {
                // Enhanced fallback rendering based on technique type
                // Portal/Vortex techniques get the swirling ring effect
                if (config.Technique == TECH_VORTEX || config.FlowSpeed > 1.5f)
                {
                    DrawFallbackPortal(position, size, config.PrimaryColor, config.SecondaryColor, config.Opacity);
                }
                else
                {
                    DrawFallbackOrb(position, size, config.PrimaryColor, config.Opacity);
                }
                return;
            }
            
            Vector2 screenPos = position - Main.screenPosition;
            
            // Use default noise if not specified
            Texture2D noise = noiseTexture ?? VFXTextureRegistry.Noise.TileableFBM 
                                           ?? VFXTextureRegistry.Noise.Smoke;
            
            if (noise == null) return;
            
            SpriteBatch sb = Main.spriteBatch;
            
            // End current batch and start with shader
            sb.End();
            
            ApplyEffect(config, noise, null, null);
            
            sb.Begin(
                SpriteSortMode.Immediate,
                BlendState.Additive,
                SamplerState.LinearWrap,
                DepthStencilState.None,
                RasterizerState.CullNone,
                _radialScrollEffect,
                Main.GameViewMatrix.TransformationMatrix
            );
            
            // Draw quad
            Rectangle destRect = new Rectangle(
                (int)(screenPos.X - size / 2),
                (int)(screenPos.Y - size / 2),
                (int)size,
                (int)size
            );
            
            sb.Draw(noise, destRect, Color.White);
            
            // Restore normal batch
            sb.End();
            sb.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.LinearClamp,
                DepthStencilState.None,
                RasterizerState.CullNone,
                null,
                Main.GameViewMatrix.TransformationMatrix
            );
        }
        
        /// <summary>
        /// Draw a themed radial orb using MagnumOpus theme presets.
        /// </summary>
        public static void DrawThemedOrb(
            Vector2 position,
            float size,
            string themeName,
            float opacity = 1f,
            Texture2D noiseTexture = null)
        {
            RadialScrollConfig config = GetThemeConfig(themeName);
            config.Opacity = opacity;
            DrawOrb(position, size, config, noiseTexture);
        }
        
        /// <summary>
        /// Draw a portal/vortex effect.
        /// </summary>
        public static void DrawPortal(
            Vector2 position,
            float size,
            Color primaryColor,
            Color secondaryColor,
            float intensity = 1f,
            Texture2D noiseTexture = null)
        {
            var config = Presets.Portal;
            config.PrimaryColor = primaryColor;
            config.SecondaryColor = secondaryColor;
            config.Opacity = intensity;
            config.FlowSpeed *= intensity;
            
            DrawOrb(position, size, config, noiseTexture);
        }
        
        /// <summary>
        /// Draw a weapon enchantment aura.
        /// </summary>
        public static void DrawWeaponAura(
            Vector2 position,
            float size,
            Color color,
            float intensity = 0.5f,
            Texture2D noiseTexture = null)
        {
            var config = Presets.WeaponAura;
            config.PrimaryColor = color;
            config.SecondaryColor = Color.White;
            config.Opacity = intensity;
            
            DrawOrb(position, size, config, noiseTexture);
        }
        
        #endregion
        
        #region Public API - Manual Control
        
        /// <summary>
        /// Apply radial scroll effect parameters to shader.
        /// Use this for manual SpriteBatch control.
        /// </summary>
        public static void ApplyEffect(
            RadialScrollConfig config,
            Texture2D primaryNoise,
            Texture2D secondaryNoise = null,
            Texture2D gradientLUT = null)
        {
            if (_radialScrollEffect == null) return;
            
            // Select technique
            if (_radialScrollEffect.Techniques[config.Technique] != null)
            {
                _radialScrollEffect.CurrentTechnique = _radialScrollEffect.Techniques[config.Technique];
            }
            
            // Set parameters
            _radialScrollEffect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            _radialScrollEffect.Parameters["uOpacity"]?.SetValue(config.Opacity);
            _radialScrollEffect.Parameters["uColor"]?.SetValue(config.PrimaryColor.ToVector4());
            _radialScrollEffect.Parameters["uSecondaryColor"]?.SetValue(config.SecondaryColor.ToVector4());
            
            _radialScrollEffect.Parameters["uFlowSpeed"]?.SetValue(config.FlowSpeed);
            _radialScrollEffect.Parameters["uRadialSpeed"]?.SetValue(config.RadialSpeed);
            _radialScrollEffect.Parameters["uDistortStrength"]?.SetValue(config.DistortStrength);
            _radialScrollEffect.Parameters["uZoom"]?.SetValue(config.Zoom);
            _radialScrollEffect.Parameters["uRepeat"]?.SetValue(config.Repeat);
            
            _radialScrollEffect.Parameters["uVignetteSize"]?.SetValue(config.VignetteSize);
            _radialScrollEffect.Parameters["uVignetteBlend"]?.SetValue(config.VignetteBlend);
            
            _radialScrollEffect.Parameters["uPulseSpeed"]?.SetValue(config.PulseSpeed);
            _radialScrollEffect.Parameters["uPulseAmount"]?.SetValue(config.PulseAmount);
            
            // Set textures
            Main.graphics.GraphicsDevice.Textures[0] = primaryNoise;
            
            if (secondaryNoise != null)
                Main.graphics.GraphicsDevice.Textures[1] = secondaryNoise;
            
            if (gradientLUT != null)
                Main.graphics.GraphicsDevice.Textures[2] = gradientLUT;
        }
        
        /// <summary>
        /// Get a theme configuration by name.
        /// </summary>
        public static RadialScrollConfig GetThemeConfig(string themeName)
        {
            return themeName?.ToLowerInvariant() switch
            {
                "lacampanella" or "campanella" => Presets.LaCampanella,
                "eroica" => Presets.Eroica,
                "moonlight" or "moonlightsonata" => Presets.MoonlightSonata,
                "swan" or "swanlake" => Presets.SwanLake,
                "enigma" or "enigmavariations" => Presets.EnigmaVariations,
                "fate" => Presets.Fate,
                "fateportal" => Presets.FatePortal,
                "portal" => Presets.Portal,
                "energy" or "energyorb" => Presets.EnergyOrb,
                "aura" or "weaponaura" => Presets.WeaponAura,
                "ambient" or "field" => Presets.AmbientField,
                _ => Presets.DefaultOrb
            };
        }
        
        #endregion
        
        #region Fallback Rendering
        
        /// <summary>
        /// Enhanced fallback orb rendering when shader isn't available.
        /// Uses multi-layer bloom, pulsing animation, and color gradients.
        /// </summary>
        private static void DrawFallbackOrb(Vector2 position, float size, Color color, float opacity)
        {
            Vector2 screenPos = position - Main.screenPosition;
            SpriteBatch sb = Main.spriteBatch;
            
            // Get bloom texture - much better than MagicPixel
            Texture2D bloomTex = MagnumTextureRegistry.GetBloom();
            if (bloomTex == null)
            {
                // Ultimate fallback to MagicPixel
                bloomTex = Terraria.GameContent.TextureAssets.MagicPixel.Value;
                if (bloomTex == null) return;
            }
            
            Vector2 origin = bloomTex.Size() * 0.5f;
            
            // Animated pulsing
            float time = Main.GlobalTimeWrappedHourly;
            float pulse = 1f + MathF.Sin(time * 4f) * 0.1f;
            float rotation = time * 0.5f;
            
            // Remove alpha for additive blending
            Color baseColor = color with { A = 0 };
            
            // === LAYER 1: Outer soft glow (largest, most transparent) ===
            float outerSize = size * 2.0f * pulse;
            Color outerColor = baseColor * (opacity * 0.2f);
            sb.Draw(bloomTex, screenPos, null, outerColor, rotation * 0.3f, origin,
                outerSize / bloomTex.Width, SpriteEffects.None, 0f);
            
            // === LAYER 2: Middle glow ===
            float midSize = size * 1.4f * pulse;
            Color midColor = baseColor * (opacity * 0.35f);
            sb.Draw(bloomTex, screenPos, null, midColor, -rotation * 0.5f, origin,
                midSize / bloomTex.Width, SpriteEffects.None, 0f);
            
            // === LAYER 3: Inner glow ===
            float innerSize = size * 1.0f * pulse;
            Color innerColor = baseColor * (opacity * 0.5f);
            sb.Draw(bloomTex, screenPos, null, innerColor, rotation * 0.7f, origin,
                innerSize / bloomTex.Width, SpriteEffects.None, 0f);
            
            // === LAYER 4: Bright core ===
            float coreSize = size * 0.6f * pulse;
            Color coreColor = Color.Lerp(baseColor, (Color.White with { A = 0 }), 0.5f) * (opacity * 0.7f);
            sb.Draw(bloomTex, screenPos, null, coreColor, -rotation, origin,
                coreSize / bloomTex.Width, SpriteEffects.None, 0f);
            
            // === LAYER 5: White-hot center ===
            float hotSize = size * 0.3f * pulse;
            Color hotColor = (Color.White with { A = 0 }) * (opacity * 0.8f);
            sb.Draw(bloomTex, screenPos, null, hotColor, rotation * 1.2f, origin,
                hotSize / bloomTex.Width, SpriteEffects.None, 0f);
        }
        
        /// <summary>
        /// Draw a fallback portal effect with animated swirl.
        /// Uses screen-space internally, expects world position as input.
        /// </summary>
        private static void DrawFallbackPortal(Vector2 position, float size, Color primary, Color secondary, float opacity)
        {
            Vector2 screenPos = position - Main.screenPosition;
            SpriteBatch sb = Main.spriteBatch;
            
            Texture2D bloomTex = MagnumTextureRegistry.GetBloom();
            Texture2D haloTex = MagnumTextureRegistry.GetHaloRing();
            if (bloomTex == null) return;
            if (haloTex == null) haloTex = bloomTex;
            
            Vector2 bloomOrigin = bloomTex.Size() * 0.5f;
            Vector2 haloOrigin = haloTex.Size() * 0.5f;
            
            float time = Main.GlobalTimeWrappedHourly;
            float pulse = 1f + MathF.Sin(time * 5f) * 0.15f;
            float rotation = time * 1.5f;
            
            Color primaryNoAlpha = primary with { A = 0 };
            Color secondaryNoAlpha = secondary with { A = 0 };
            
            // === OUTER SWIRLING RINGS ===
            for (int ring = 0; ring < 3; ring++)
            {
                float ringSize = size * (1.8f - ring * 0.3f) * pulse;
                float ringRotation = rotation * (1f + ring * 0.4f) * (ring % 2 == 0 ? 1 : -1);
                Color ringColor = Color.Lerp(primaryNoAlpha, secondaryNoAlpha, ring / 3f) * (opacity * 0.3f);
                
                sb.Draw(haloTex, screenPos, null, ringColor, ringRotation, haloOrigin,
                    ringSize / haloTex.Width, SpriteEffects.None, 0f);
            }
            
            // === ENERGY CORE (use internal method to avoid double screen-conversion) ===
            float coreSize = size * 0.6f;
            float corePulse = 1f + MathF.Sin(time * 4f) * 0.1f;
            Color coreColor = primaryNoAlpha * (opacity * 0.6f);
            Color coreWhite = (Color.White with { A = 0 }) * (opacity * 0.8f);
            
            // Multi-layer core bloom
            sb.Draw(bloomTex, screenPos, null, coreColor, rotation * 0.3f, bloomOrigin,
                coreSize * 1.4f * corePulse / bloomTex.Width, SpriteEffects.None, 0f);
            sb.Draw(bloomTex, screenPos, null, coreColor, -rotation * 0.5f, bloomOrigin,
                coreSize * 1.0f * corePulse / bloomTex.Width, SpriteEffects.None, 0f);
            sb.Draw(bloomTex, screenPos, null, coreWhite, rotation * 0.7f, bloomOrigin,
                coreSize * 0.5f * corePulse / bloomTex.Width, SpriteEffects.None, 0f);
        }
        
        #endregion
    }
    
    /// <summary>
    /// Configuration for radial scroll effects.
    /// </summary>
    public struct RadialScrollConfig
    {
        /// <summary>Angular scroll speed (radians/second). Default: 0.5</summary>
        public float FlowSpeed;
        
        /// <summary>Radial scroll speed (UV/second). Default: 0.2</summary>
        public float RadialSpeed;
        
        /// <summary>Noise distortion strength (0-0.2). Default: 0.02</summary>
        public float DistortStrength;
        
        /// <summary>UV zoom factor. Higher = more detail. Default: 1.0</summary>
        public float Zoom;
        
        /// <summary>Angular repeat count. Higher = more pattern copies. Default: 1.0</summary>
        public float Repeat;
        
        /// <summary>Vignette start radius (0-1). Default: 0.5</summary>
        public float VignetteSize;
        
        /// <summary>Vignette blend width (0-1). Default: 0.2</summary>
        public float VignetteBlend;
        
        /// <summary>Pulsing animation speed. Default: 0</summary>
        public float PulseSpeed;
        
        /// <summary>Pulsing intensity (0-0.5). Default: 0</summary>
        public float PulseAmount;
        
        /// <summary>Primary tint color.</summary>
        public Color PrimaryColor;
        
        /// <summary>Secondary color for edge glow/gradient.</summary>
        public Color SecondaryColor;
        
        /// <summary>Master opacity (0-1). Default: 1.0</summary>
        public float Opacity;
        
        /// <summary>Shader technique name. Default: "DualPhase"</summary>
        public string Technique;
        
        /// <summary>
        /// Create with defaults.
        /// </summary>
        public static RadialScrollConfig Default => new RadialScrollConfig
        {
            FlowSpeed = 0.5f,
            RadialSpeed = 0.2f,
            DistortStrength = 0.02f,
            Zoom = 1.0f,
            Repeat = 1.0f,
            VignetteSize = 0.5f,
            VignetteBlend = 0.2f,
            PulseSpeed = 0f,
            PulseAmount = 0f,
            PrimaryColor = Color.White,
            SecondaryColor = Color.White,
            Opacity = 1.0f,
            Technique = RadialScrollSystem.TECH_DUAL_PHASE
        };
    }
}
