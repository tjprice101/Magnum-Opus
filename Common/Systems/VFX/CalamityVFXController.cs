using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using ReLogic.Content;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// CALAMITY VFX CONTROLLER - Master Orchestration System
    /// 
    /// This is the central hub for coordinating all high-end VFX components:
    /// - Ribbon trail mesh generation and rendering
    /// - Shader parameter management
    /// - LUT/Noise texture alignment
    /// - Multi-pass rendering coordination
    /// - Particle spawning synchronization
    /// - Screen-space distortion triggering
    /// 
    /// Based on the Ark of the Cosmos pipeline where multiple visual layers
    /// (core beam, flaming noise, rainbow shimmer) must be perfectly synchronized.
    /// </summary>
    public class CalamityVFXController : ModSystem
    {
        #region Static Instance
        
        private static CalamityVFXController _instance;
        public static CalamityVFXController Instance => _instance;
        
        #endregion
        
        #region VFX Effect Data
        
        /// <summary>
        /// Configuration for a complete VFX effect (weapon swing, beam, projectile trail).
        /// </summary>
        public class VFXEffectConfig
        {
            // Identification
            public int Id;
            public string Name;
            public string Theme;
            
            // Component IDs
            public int RibbonId = -1;           // From RibbonTrailSystem
            public int ArkTrailId = -1;         // From ArkSwingTrail (if melee)
            public int AdvancedTrailId = -1;    // From AdvancedTrailSystem
            
            // Colors
            public Color PrimaryColor;
            public Color SecondaryColor;
            public Color CoreColor;
            public Color GlowColor;
            
            // Shader Configuration
            public ShaderPreset ShaderMode;
            public float NoiseScale = 2.0f;
            public float ScrollSpeedA = 0.3f;
            public float ScrollSpeedB = 0.2f;
            public float DistortionStrength = 0.08f;
            public float ErosionThreshold = 0.3f;
            public float FlickerSpeed = 8.0f;
            public float LUTOffset = 0f;
            
            // Screen Effects
            public bool EnableHeatDistortion = false;
            public float HeatDistortionRadius = 100f;
            public float HeatDistortionStrength = 0.02f;
            
            // Particle Integration
            public bool EnableParticles = true;
            public int ParticleSpawnRate = 3;        // Particles per frame
            public float ParticleSpreadRadius = 10f;
            
            // Timing
            public float Age;
            public float Duration = -1f;             // -1 = infinite
            public bool IsFading;
            public float FadeProgress;
            public bool IsComplete;
        }
        
        /// <summary>
        /// Predefined shader configurations for different effect types.
        /// </summary>
        public enum ShaderPreset
        {
            FluidFire,           // Dual-scroll noise + erosion (Ark of the Cosmos flames)
            SpectralShimmer,     // Rainbow LUT sampling (Exo Blade prismatic)
            SmokeWisp,           // Slow-moving, soft smoke
            ElectricArc,         // High-frequency, sharp edges
            CosmicNebula,        // Deep space cloudy effect
            HolyLight,           // Bright, clean glow
            VoidDarkness,        // Dark with occasional bright cracks
            Custom               // User-defined parameters
        }
        
        #endregion
        
        #region Internal State
        
        private List<VFXEffectConfig> _activeEffects;
        private int _nextEffectId;
        
        // Shader instances
        private Effect _fireShader;
        private Effect _distortionShader;
        private bool _shadersLoaded;
        
        // Shared textures (from VFXTextureRegistry)
        private Texture2D _currentNoiseTex;
        private Texture2D _currentLUTTex;
        private Texture2D _currentMaskTex;
        
        #endregion
        
        #region Lifecycle
        
        public override void Load()
        {
            _instance = this;
            _activeEffects = new List<VFXEffectConfig>(64);
            _nextEffectId = 0;
            
            Main.QueueMainThreadAction(LoadShaders);
        }
        
        public override void Unload()
        {
            _instance = null;
            _activeEffects?.Clear();
            _fireShader?.Dispose();
            _distortionShader?.Dispose();
        }
        
        private void LoadShaders()
        {
            if (Main.dedServ) return;
            
            try
            {
                // Load CalamityFireShader
                if (ModContent.HasAsset("MagnumOpus/Assets/Shaders/CalamityFireShader"))
                {
                    _fireShader = ModContent.Request<Effect>(
                        "MagnumOpus/Assets/Shaders/CalamityFireShader",
                        AssetRequestMode.ImmediateLoad
                    ).Value;
                }
                
                // Load AdvancedDistortionShader
                if (ModContent.HasAsset("MagnumOpus/Assets/Shaders/AdvancedDistortionShader"))
                {
                    _distortionShader = ModContent.Request<Effect>(
                        "MagnumOpus/Assets/Shaders/AdvancedDistortionShader",
                        AssetRequestMode.ImmediateLoad
                    ).Value;
                }
                
                _shadersLoaded = _fireShader != null;
            }
            catch (Exception ex)
            {
                Main.NewText($"[CalamityVFX] Shader load error: {ex.Message}", Color.Red);
                _shadersLoaded = false;
            }
        }
        
        #endregion
        
        #region Public API - Effect Creation
        
        /// <summary>
        /// Creates a new VFX effect with the specified preset.
        /// Returns the effect ID for tracking and updating.
        /// </summary>
        public static int CreateEffect(
            string name,
            string theme,
            ShaderPreset preset,
            Color primaryColor,
            Color secondaryColor,
            float duration = -1f)
        {
            if (_instance == null) return -1;
            return _instance.CreateEffectInternal(name, theme, preset, primaryColor, secondaryColor, duration);
        }
        
        /// <summary>
        /// Creates a complete weapon swing effect with ribbon trail, particles, and shaders.
        /// </summary>
        public static int CreateSwingEffect(
            Player player,
            string theme,
            Color primaryColor,
            Color secondaryColor,
            float bladeLength,
            float trailWidth = 30f)
        {
            if (_instance == null) return -1;
            return _instance.CreateSwingEffectInternal(player, theme, primaryColor, secondaryColor, bladeLength, trailWidth);
        }
        
        /// <summary>
        /// Creates a projectile trail effect.
        /// </summary>
        public static int CreateProjectileTrail(
            Projectile projectile,
            string theme,
            Color primaryColor,
            Color secondaryColor,
            float width = 20f,
            ShaderPreset preset = ShaderPreset.FluidFire)
        {
            if (_instance == null) return -1;
            return _instance.CreateProjectileTrailInternal(projectile, theme, primaryColor, secondaryColor, width, preset);
        }
        
        /// <summary>
        /// Updates an effect with new position data.
        /// </summary>
        public static void UpdateEffect(int effectId, Vector2 position, Vector2 velocity, float rotation)
        {
            _instance?.UpdateEffectInternal(effectId, position, velocity, rotation);
        }
        
        /// <summary>
        /// Ends an effect (starts fade-out).
        /// </summary>
        public static void EndEffect(int effectId)
        {
            _instance?.EndEffectInternal(effectId);
        }
        
        /// <summary>
        /// Gets the configuration for an effect (for advanced customization).
        /// </summary>
        public static VFXEffectConfig GetEffect(int effectId)
        {
            return _instance?.GetEffectInternal(effectId);
        }
        
        #endregion
        
        #region Internal Implementation
        
        private int CreateEffectInternal(string name, string theme, ShaderPreset preset,
            Color primaryColor, Color secondaryColor, float duration)
        {
            var config = new VFXEffectConfig
            {
                Id = _nextEffectId++,
                Name = name,
                Theme = theme,
                ShaderMode = preset,
                PrimaryColor = primaryColor,
                SecondaryColor = secondaryColor,
                CoreColor = Color.Lerp(primaryColor, Color.White, 0.7f),
                GlowColor = Color.Lerp(primaryColor, secondaryColor, 0.5f),
                Duration = duration,
                Age = 0f
            };
            
            // Apply preset parameters
            ApplyPreset(config, preset);
            
            _activeEffects.Add(config);
            return config.Id;
        }
        
        private int CreateSwingEffectInternal(Player player, string theme, Color primaryColor,
            Color secondaryColor, float bladeLength, float trailWidth)
        {
            // Create the main effect config
            int effectId = CreateEffectInternal($"Swing_{player.whoAmI}", theme, ShaderPreset.FluidFire,
                primaryColor, secondaryColor, -1f);
            
            var config = GetEffectInternal(effectId);
            if (config == null) return -1;
            
            // Create ribbon trail
            config.RibbonId = RibbonTrailSystem.CreateRibbon(
                player.whoAmI,
                primaryColor,
                secondaryColor,
                trailWidth,
                25,
                theme
            );
            
            // Also use ArkSwingTrail for immediate visual
            // (ArkSwingTrail uses GlobalItem hook, so we just need to trigger it)
            
            config.EnableHeatDistortion = true;
            config.HeatDistortionRadius = bladeLength * 1.5f;
            
            return effectId;
        }
        
        private int CreateProjectileTrailInternal(Projectile projectile, string theme, 
            Color primaryColor, Color secondaryColor, float width, ShaderPreset preset)
        {
            int effectId = CreateEffectInternal($"Proj_{projectile.whoAmI}", theme, preset,
                primaryColor, secondaryColor, -1f);
            
            var config = GetEffectInternal(effectId);
            if (config == null) return -1;
            
            // Create ribbon trail for projectile
            config.RibbonId = RibbonTrailSystem.CreateRibbon(
                projectile.whoAmI + 10000, // Offset to avoid player ID collision
                primaryColor,
                secondaryColor,
                width,
                20,
                theme
            );
            
            // Use AdvancedTrailSystem for additional effects
            config.AdvancedTrailId = AdvancedTrailSystem.CreateThemeTrail(theme, width, 20, 1f);
            
            return effectId;
        }
        
        private void UpdateEffectInternal(int effectId, Vector2 position, Vector2 velocity, float rotation)
        {
            var config = GetEffectInternal(effectId);
            if (config == null || config.IsComplete) return;
            
            // Update ribbon trail
            if (config.RibbonId >= 0)
            {
                RibbonTrailSystem.UpdateRibbon(config.RibbonId, position, velocity, rotation);
            }
            
            // Update advanced trail
            if (config.AdvancedTrailId >= 0)
            {
                AdvancedTrailSystem.UpdateTrail(config.AdvancedTrailId, position, rotation);
            }
            
            // Spawn particles if enabled
            if (config.EnableParticles && Main.rand.Next(config.ParticleSpawnRate) == 0)
            {
                SpawnEffectParticles(config, position, velocity);
            }
            
            // Trigger heat distortion if enabled
            if (config.EnableHeatDistortion)
            {
                TriggerHeatDistortion(position, config.HeatDistortionRadius, config.HeatDistortionStrength);
            }
            
            config.Age += 1f / 60f;
        }
        
        private void EndEffectInternal(int effectId)
        {
            var config = GetEffectInternal(effectId);
            if (config == null) return;
            
            config.IsFading = true;
            
            // Fade child components
            if (config.RibbonId >= 0)
            {
                RibbonTrailSystem.FadeRibbon(config.RibbonId);
            }
            
            if (config.AdvancedTrailId >= 0)
            {
                AdvancedTrailSystem.DestroyTrail(config.AdvancedTrailId);
            }
        }
        
        private VFXEffectConfig GetEffectInternal(int effectId)
        {
            foreach (var effect in _activeEffects)
            {
                if (effect.Id == effectId)
                    return effect;
            }
            return null;
        }
        
        #endregion
        
        #region Shader Preset Configuration
        
        private void ApplyPreset(VFXEffectConfig config, ShaderPreset preset)
        {
            switch (preset)
            {
                case ShaderPreset.FluidFire:
                    config.NoiseScale = 2.0f;
                    config.ScrollSpeedA = 0.35f;
                    config.ScrollSpeedB = 0.25f;
                    config.DistortionStrength = 0.1f;
                    config.ErosionThreshold = 0.35f;
                    config.FlickerSpeed = 10f;
                    break;
                    
                case ShaderPreset.SpectralShimmer:
                    config.NoiseScale = 1.5f;
                    config.ScrollSpeedA = 0.15f;
                    config.ScrollSpeedB = 0.1f;
                    config.DistortionStrength = 0.02f;
                    config.ErosionThreshold = 0.1f;
                    config.FlickerSpeed = 4f;
                    break;
                    
                case ShaderPreset.SmokeWisp:
                    config.NoiseScale = 1.0f;
                    config.ScrollSpeedA = 0.08f;
                    config.ScrollSpeedB = 0.05f;
                    config.DistortionStrength = 0.15f;
                    config.ErosionThreshold = 0.5f;
                    config.FlickerSpeed = 2f;
                    break;
                    
                case ShaderPreset.ElectricArc:
                    config.NoiseScale = 4.0f;
                    config.ScrollSpeedA = 0.8f;
                    config.ScrollSpeedB = 0.6f;
                    config.DistortionStrength = 0.2f;
                    config.ErosionThreshold = 0.6f;
                    config.FlickerSpeed = 25f;
                    break;
                    
                case ShaderPreset.CosmicNebula:
                    config.NoiseScale = 0.8f;
                    config.ScrollSpeedA = 0.03f;
                    config.ScrollSpeedB = 0.02f;
                    config.DistortionStrength = 0.05f;
                    config.ErosionThreshold = 0.2f;
                    config.FlickerSpeed = 1f;
                    break;
                    
                case ShaderPreset.HolyLight:
                    config.NoiseScale = 1.2f;
                    config.ScrollSpeedA = 0.1f;
                    config.ScrollSpeedB = 0.08f;
                    config.DistortionStrength = 0.01f;
                    config.ErosionThreshold = 0.05f;
                    config.FlickerSpeed = 3f;
                    config.CoreColor = Color.White;
                    break;
                    
                case ShaderPreset.VoidDarkness:
                    config.NoiseScale = 2.5f;
                    config.ScrollSpeedA = 0.12f;
                    config.ScrollSpeedB = 0.08f;
                    config.DistortionStrength = 0.12f;
                    config.ErosionThreshold = 0.7f;
                    config.FlickerSpeed = 6f;
                    break;
            }
        }
        
        #endregion
        
        #region Particle Integration
        
        private void SpawnEffectParticles(VFXEffectConfig config, Vector2 position, Vector2 velocity)
        {
            // Use MagnumParticleHandler to spawn theme-appropriate particles
            Vector2 spawnOffset = Main.rand.NextVector2Circular(
                config.ParticleSpreadRadius, config.ParticleSpreadRadius);
            
            Vector2 particleVel = velocity * 0.2f + Main.rand.NextVector2Circular(1f, 1f);
            
            // Spawn glow particle
            var glow = new GenericGlowParticle(
                position + spawnOffset,
                particleVel,
                Color.Lerp(config.PrimaryColor, config.SecondaryColor, Main.rand.NextFloat()),
                Main.rand.NextFloat(0.2f, 0.4f),
                Main.rand.Next(15, 25),
                true
            );
            MagnumParticleHandler.SpawnParticle(glow);
            
            // Occasionally spawn sparkle
            if (Main.rand.NextBool(3))
            {
                var sparkle = new SparkleParticle(
                    position + spawnOffset * 0.5f,
                    particleVel * 1.5f,
                    config.CoreColor,
                    Main.rand.NextFloat(0.15f, 0.3f),
                    Main.rand.Next(10, 20)
                );
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
        }
        
        #endregion
        
        #region Screen Distortion
        
        private void TriggerHeatDistortion(Vector2 worldPosition, float radius, float strength)
        {
            // Convert to screen position
            Vector2 screenPos = worldPosition - Main.screenPosition;
            
            // Only apply if on screen
            if (screenPos.X < -radius || screenPos.X > Main.screenWidth + radius ||
                screenPos.Y < -radius || screenPos.Y > Main.screenHeight + radius)
                return;
            
            // Trigger through ScreenDistortionManager
            ScreenDistortionManager.TriggerRipple(worldPosition, strength, 15);
        }
        
        #endregion
        
        #region Update Loop
        
        public override void PostUpdateEverything()
        {
            UpdateAllEffects();
        }
        
        private void UpdateAllEffects()
        {
            for (int i = _activeEffects.Count - 1; i >= 0; i--)
            {
                var effect = _activeEffects[i];
                
                // Check duration
                if (effect.Duration > 0 && effect.Age >= effect.Duration && !effect.IsFading)
                {
                    EndEffectInternal(effect.Id);
                }
                
                // Update fading effects
                if (effect.IsFading)
                {
                    effect.FadeProgress += 0.05f;
                    if (effect.FadeProgress >= 1f)
                    {
                        effect.IsComplete = true;
                    }
                }
                
                // Remove complete effects
                if (effect.IsComplete)
                {
                    _activeEffects.RemoveAt(i);
                }
            }
        }
        
        #endregion
        
        #region Rendering Coordination
        
        /// <summary>
        /// Applies shader parameters for the active effect.
        /// Call this before rendering trails with the fire shader.
        /// </summary>
        public static void ApplyShaderParameters(int effectId, Effect shader)
        {
            _instance?.ApplyShaderParametersInternal(effectId, shader);
        }
        
        private void ApplyShaderParametersInternal(int effectId, Effect shader)
        {
            if (shader == null) return;
            
            var config = GetEffectInternal(effectId);
            if (config == null) return;
            
            float time = (float)Main.gameTimeCache.TotalGameTime.TotalSeconds;
            
            // Set standard parameters
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f - config.FadeProgress);
            shader.Parameters["uIntensity"]?.SetValue(1.2f);
            
            // Set colors
            shader.Parameters["uColor"]?.SetValue(config.PrimaryColor.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(config.SecondaryColor.ToVector3());
            shader.Parameters["uHotColor"]?.SetValue(config.CoreColor.ToVector3());
            
            // Set advection parameters
            shader.Parameters["uNoiseScale"]?.SetValue(config.NoiseScale);
            shader.Parameters["uScrollSpeedA"]?.SetValue(config.ScrollSpeedA);
            shader.Parameters["uScrollSpeedB"]?.SetValue(config.ScrollSpeedB);
            shader.Parameters["uDistortionStrength"]?.SetValue(config.DistortionStrength);
            shader.Parameters["uErosionThreshold"]?.SetValue(config.ErosionThreshold);
            shader.Parameters["uFlickerSpeed"]?.SetValue(config.FlickerSpeed);
            shader.Parameters["uLUTOffset"]?.SetValue(config.LUTOffset);
            
            // Set textures
            SetShaderTextures(shader, config.Theme);
        }
        
        private void SetShaderTextures(Effect shader, string theme)
        {
            // Get theme-appropriate textures from VFXTextureRegistry
            var noiseTex = VFXTextureRegistry.GetNoiseForTheme(theme);
            var lutTex = VFXTextureRegistry.LUT.Rainbow;
            var maskTex = VFXTextureRegistry.Mask.EclipseRing;
            
            shader.Parameters["uNoiseTex"]?.SetValue(noiseTex);
            shader.Parameters["uPaletteLUT"]?.SetValue(lutTex);
            shader.Parameters["uMaskTex"]?.SetValue(maskTex);
        }
        
        #endregion
    }
}
