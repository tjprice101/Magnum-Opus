using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// Manages screen-wide distortion effects using the advanced shader system.
    /// Provides easy-to-use methods for triggering themed screen effects.
    /// </summary>
    public class ScreenDistortionManager : ModSystem
    {
        #region Active Effects Queue

        private static List<ActiveDistortion> _activeDistortions = new();
        private static RenderTarget2D _screenTarget;
        private static bool _effectsEnabled = true;

        private struct ActiveDistortion
        {
            public ShaderStyleRegistry.ScreenStyle Style;
            public Vector2 WorldPosition;
            public Vector2 SecondaryWorldPosition;
            public Color PrimaryColor;
            public Color SecondaryColor;
            public float BaseIntensity;
            public float Radius;
            public int Duration;
            public int Timer;
            public Func<float, float> EasingFunction;
        }

        #endregion

        #region Initialization

        public override void Load()
        {
            if (Main.dedServ) return;

            Main.OnResolutionChanged += OnResolutionChanged;
        }

        public override void Unload()
        {
            Main.OnResolutionChanged -= OnResolutionChanged;
            _screenTarget?.Dispose();
            _screenTarget = null;
            _activeDistortions?.Clear();
        }

        private void OnResolutionChanged(Vector2 newSize)
        {
            _screenTarget?.Dispose();
            _screenTarget = null;
        }

        private static void EnsureRenderTarget()
        {
            if (_screenTarget == null || _screenTarget.IsDisposed ||
                _screenTarget.Width != Main.screenWidth || _screenTarget.Height != Main.screenHeight)
            {
                _screenTarget?.Dispose();
                _screenTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);
            }
        }

        #endregion

        #region Public API - Trigger Effects

        /// <summary>
        /// Toggle screen effects on/off (for accessibility)
        /// </summary>
        public static void SetEffectsEnabled(bool enabled) => _effectsEnabled = enabled;

        /// <summary>
        /// Triggers a ripple distortion effect centered at a world position.
        /// Great for water impacts, sonic booms, shockwaves.
        /// </summary>
        public static void TriggerRipple(Vector2 worldPosition, Color color, float intensity = 1f, int duration = 30)
        {
            AddDistortion(new ActiveDistortion
            {
                Style = ShaderStyleRegistry.ScreenStyle.Ripple,
                WorldPosition = worldPosition,
                PrimaryColor = color,
                SecondaryColor = color,
                BaseIntensity = intensity,
                Radius = 0.5f,
                Duration = duration,
                Timer = 0,
                EasingFunction = EaseOutQuad
            });
        }

        /// <summary>
        /// Triggers a reality shatter effect. Great for powerful impacts, reality-breaking attacks.
        /// </summary>
        public static void TriggerShatter(Vector2 worldPosition, Color primaryColor, Color secondaryColor, float intensity = 1f, int duration = 25)
        {
            AddDistortion(new ActiveDistortion
            {
                Style = ShaderStyleRegistry.ScreenStyle.Shatter,
                WorldPosition = worldPosition,
                PrimaryColor = primaryColor,
                SecondaryColor = secondaryColor,
                BaseIntensity = intensity,
                Radius = 0.4f,
                Duration = duration,
                Timer = 0,
                EasingFunction = EaseOutExpo
            });
        }

        /// <summary>
        /// Triggers a gravitational warp effect. Great for void/gravity attacks, black hole effects.
        /// </summary>
        public static void TriggerWarp(Vector2 worldPosition, Color color, float intensity = 1f, int duration = 45)
        {
            AddDistortion(new ActiveDistortion
            {
                Style = ShaderStyleRegistry.ScreenStyle.Warp,
                WorldPosition = worldPosition,
                PrimaryColor = color,
                SecondaryColor = color,
                BaseIntensity = intensity,
                Radius = 0.35f,
                Duration = duration,
                Timer = 0,
                EasingFunction = EaseInOutQuad
            });
        }

        /// <summary>
        /// Triggers a heartbeat pulse effect. Great for life/death themes, rhythmic attacks.
        /// </summary>
        public static void TriggerPulse(Vector2 worldPosition, Color color, float intensity = 1f, int duration = 20)
        {
            AddDistortion(new ActiveDistortion
            {
                Style = ShaderStyleRegistry.ScreenStyle.Pulse,
                WorldPosition = worldPosition,
                PrimaryColor = color,
                SecondaryColor = color,
                BaseIntensity = intensity,
                Radius = 0.6f,
                Duration = duration,
                Timer = 0,
                EasingFunction = EasePulse
            });
        }

        /// <summary>
        /// Triggers a reality tear effect between two points. Great for dimensional attacks, teleportation.
        /// </summary>
        public static void TriggerTear(Vector2 startWorldPosition, Vector2 endWorldPosition, Color primaryColor, Color accentColor, float intensity = 1f, int duration = 40)
        {
            AddDistortion(new ActiveDistortion
            {
                Style = ShaderStyleRegistry.ScreenStyle.Tear,
                WorldPosition = startWorldPosition,
                SecondaryWorldPosition = endWorldPosition,
                PrimaryColor = primaryColor,
                SecondaryColor = accentColor,
                BaseIntensity = intensity,
                Radius = 0.15f,
                Duration = duration,
                Timer = 0,
                EasingFunction = EaseOutQuad
            });
        }

        /// <summary>
        /// Triggers the theme-appropriate screen effect for a given theme.
        /// </summary>
        public static void TriggerThemeEffect(string theme, Vector2 worldPosition, float intensity = 1f, int duration = 30, Vector2? secondaryPosition = null)
        {
            var styles = ShaderStyleRegistry.GetThemeStyles(theme);
            if (styles.Screen == ShaderStyleRegistry.ScreenStyle.None) return;

            var palette = MagnumThemePalettes.GetThemePalette(theme);
            Color primary = palette.Length > 0 ? palette[0] : Color.White;
            Color secondary = palette.Length > 1 ? palette[1] : primary;

            switch (styles.Screen)
            {
                case ShaderStyleRegistry.ScreenStyle.Ripple:
                    TriggerRipple(worldPosition, primary, intensity, duration);
                    break;
                case ShaderStyleRegistry.ScreenStyle.Shatter:
                    TriggerShatter(worldPosition, primary, secondary, intensity, duration);
                    break;
                case ShaderStyleRegistry.ScreenStyle.Warp:
                    TriggerWarp(worldPosition, primary, intensity, duration);
                    break;
                case ShaderStyleRegistry.ScreenStyle.Pulse:
                    TriggerPulse(worldPosition, primary, intensity, duration);
                    break;
                case ShaderStyleRegistry.ScreenStyle.Tear:
                    TriggerTear(worldPosition, secondaryPosition ?? worldPosition + new Vector2(100f, 0f), primary, secondary, intensity, duration);
                    break;
            }
        }

        /// <summary>
        /// Clears all active distortion effects immediately.
        /// </summary>
        public static void ClearAllEffects()
        {
            _activeDistortions.Clear();
        }

        #endregion

        #region Update Logic

        public override void PostUpdateEverything()
        {
            if (Main.dedServ || !_effectsEnabled) return;

            // Update all active distortions
            for (int i = _activeDistortions.Count - 1; i >= 0; i--)
            {
                var distortion = _activeDistortions[i];
                distortion.Timer++;

                if (distortion.Timer >= distortion.Duration)
                {
                    _activeDistortions.RemoveAt(i);
                }
                else
                {
                    _activeDistortions[i] = distortion;
                }
            }
        }

        #endregion

        #region Render Application

        /// <summary>
        /// Apply active screen effects. Call this when appropriate in your render pipeline.
        /// Typically called in a ModSystem.ModifyTransformMatrix or custom draw layer.
        /// </summary>
        public static void ApplyDistortions(SpriteBatch spriteBatch)
        {
            if (!_effectsEnabled || _activeDistortions.Count == 0) return;

            foreach (var distortion in _activeDistortions)
            {
                float progress = distortion.Timer / (float)distortion.Duration;
                float easedProgress = distortion.EasingFunction(progress);
                float intensity = distortion.BaseIntensity * (1f - easedProgress);

                // Only apply if intensity is significant
                if (intensity < 0.01f) continue;

                var shader = ShaderStyleRegistry.ApplyScreenStyle(
                    distortion.Style,
                    distortion.WorldPosition,
                    distortion.PrimaryColor,
                    distortion.SecondaryColor,
                    intensity,
                    easedProgress,
                    distortion.Radius,
                    distortion.SecondaryWorldPosition
                );

                // Shader is ready - actual screen rendering handled by draw layer
            }
        }

        /// <summary>
        /// Returns true if there are active distortion effects that need rendering.
        /// </summary>
        public static bool HasActiveEffects() => _effectsEnabled && _activeDistortions.Count > 0;

        /// <summary>
        /// Gets the current most intense distortion for single-effect rendering.
        /// </summary>
        public static (ShaderStyleRegistry.ScreenStyle style, float intensity, Vector2 position)? GetDominantEffect()
        {
            if (_activeDistortions.Count == 0) return null;

            ActiveDistortion dominant = default;
            float maxIntensity = 0f;

            foreach (var d in _activeDistortions)
            {
                float progress = d.Timer / (float)d.Duration;
                float intensity = d.BaseIntensity * (1f - d.EasingFunction(progress));
                if (intensity > maxIntensity)
                {
                    maxIntensity = intensity;
                    dominant = d;
                }
            }

            if (maxIntensity < 0.01f) return null;

            return (dominant.Style, maxIntensity, dominant.WorldPosition);
        }

        #endregion

        #region Easing Functions

        private static void AddDistortion(ActiveDistortion distortion)
        {
            // Limit active distortions for performance
            if (_activeDistortions.Count >= 5)
            {
                _activeDistortions.RemoveAt(0);
            }
            _activeDistortions.Add(distortion);
        }

        private static float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);
        private static float EaseOutExpo(float t) => t >= 1f ? 1f : 1f - (float)Math.Pow(2, -10 * t);
        private static float EaseInOutQuad(float t) => t < 0.5f ? 2f * t * t : 1f - (float)Math.Pow(-2 * t + 2, 2) / 2f;
        private static float EasePulse(float t)
        {
            // Double pulse pattern
            float phase1 = t < 0.3f ? (float)Math.Sin(t / 0.3f * MathHelper.Pi) : 0f;
            float phase2 = t > 0.4f && t < 0.7f ? (float)Math.Sin((t - 0.4f) / 0.3f * MathHelper.Pi) * 0.6f : 0f;
            return phase1 + phase2;
        }

        #endregion
    }
}
