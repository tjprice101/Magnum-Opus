using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems.VFX.Effects
{
    /// <summary>
    /// Screen shake management system with multiple shake types.
    /// 
    /// Shake Components:
    /// - Amplitude: Maximum displacement
    /// - Frequency: Speed of oscillation
    /// - Decay: How quickly it fades
    /// - Direction: Random, directional, or circular
    /// 
    /// Best Practices:
    /// - Short duration (&lt; 0.5s for most effects)
    /// - Exponential decay feels natural
    /// - Multiple shakes stack additively
    /// - Camera-relative (not world-space)
    /// </summary>
    public class ScreenShakeManager : ModSystem
    {
        public static ScreenShakeManager Instance { get; private set; }

        private List<Shake> activeShakes;
        private Vector2 currentOffset;
        private float trauma; // 0-1, drives shake intensity (GDC-style)

        /// <summary>
        /// Current screen shake offset. Apply to camera position.
        /// </summary>
        public Vector2 ShakeOffset => currentOffset;

        /// <summary>
        /// Current trauma level (0-1).
        /// </summary>
        public float Trauma => trauma;

        public override void Load()
        {
            Instance = this;
            activeShakes = new List<Shake>();
        }

        public override void Unload()
        {
            Instance = null;
            activeShakes?.Clear();
            activeShakes = null;
        }

        #region Shake Methods

        /// <summary>
        /// Add a discrete shake effect.
        /// </summary>
        /// <param name="amplitude">Maximum pixel displacement</param>
        /// <param name="duration">Duration in seconds</param>
        /// <param name="frequency">Oscillation speed (Hz)</param>
        /// <param name="direction">Specific direction (null for omni)</param>
        /// <param name="decay">Fade curve type</param>
        public void AddShake(float amplitude, float duration, float frequency = 15f,
            Vector2? direction = null, DecayCurve decay = DecayCurve.Exponential)
        {
            activeShakes.Add(new Shake
            {
                Amplitude = amplitude,
                Frequency = frequency,
                Duration = duration,
                Age = 0f,
                Direction = direction ?? Vector2.Zero,
                Decay = decay
            });
        }

        /// <summary>
        /// Add impact-style shake (short, sharp).
        /// </summary>
        public void AddImpactShake(float intensity)
        {
            AddShake(intensity * 10f, 0.15f, 25f, null, DecayCurve.Exponential);
        }

        /// <summary>
        /// Add explosion-style shake (builds quickly, fades slowly).
        /// </summary>
        public void AddExplosionShake(float intensity, Vector2 explosionPos)
        {
            float distance = Vector2.Distance(explosionPos, Main.LocalPlayer.Center);
            float distanceFactor = MathHelper.Clamp(500f / (distance + 1f), 0f, 1f);

            AddShake(intensity * 15f * distanceFactor, 0.4f, 18f, null, DecayCurve.Exponential);
        }

        /// <summary>
        /// Add directional shake (pushes camera away from source).
        /// </summary>
        public void AddDirectionalShake(float intensity, Vector2 sourcePosition)
        {
            Vector2 direction = Main.LocalPlayer.Center - sourcePosition;
            if (direction.LengthSquared() > 0)
                direction.Normalize();

            AddShake(intensity * 8f, 0.25f, 20f, direction, DecayCurve.Exponential);
        }

        /// <summary>
        /// Add rumble-style shake (continuous, low frequency).
        /// </summary>
        public void AddRumbleShake(float intensity, float duration)
        {
            AddShake(intensity * 5f, duration, 8f, null, DecayCurve.Linear);
        }

        /// <summary>
        /// Add trauma for GDC-style cumulative shake.
        /// Trauma decays over time and drives shake intensity.
        /// </summary>
        public void AddTrauma(float amount)
        {
            trauma = MathHelper.Clamp(trauma + amount, 0f, 1f);
        }

        /// <summary>
        /// Clear all active shakes.
        /// </summary>
        public void Clear()
        {
            activeShakes.Clear();
            trauma = 0f;
            currentOffset = Vector2.Zero;
        }

        #endregion

        public override void PostUpdateEverything()
        {
            float deltaTime = 1f / 60f; // Assume 60 FPS for consistent behavior
            Update(deltaTime);
        }

        private void Update(float deltaTime)
        {
            currentOffset = Vector2.Zero;

            // Trauma-based shake (Perlin-style, cumulative)
            if (trauma > 0.01f)
            {
                trauma = MathHelper.Max(0f, trauma - deltaTime * 2f); // Decay

                float shake = trauma * trauma; // Square for smoother feel
                float time = (float)Main.GameUpdateCount * 0.1f;

                // Use sine waves at different frequencies for pseudo-noise
                currentOffset.X += (float)Math.Sin(time * 1.7f + 0.3f) * shake * 20f;
                currentOffset.Y += (float)Math.Sin(time * 2.3f + 1.7f) * shake * 20f;
            }

            // Discrete shake effects
            for (int i = activeShakes.Count - 1; i >= 0; i--)
            {
                var shake = activeShakes[i];
                shake.Age += deltaTime;

                if (shake.Age >= shake.Duration)
                {
                    activeShakes.RemoveAt(i);
                    continue;
                }

                float intensity = shake.GetIntensity();
                float phase = shake.Age * shake.Frequency * MathHelper.TwoPi;

                if (shake.Direction == Vector2.Zero)
                {
                    // Omni-directional (pseudo-random)
                    currentOffset.X += (float)Math.Sin(phase * 1.7f) * shake.Amplitude * intensity;
                    currentOffset.Y += (float)Math.Cos(phase * 2.3f) * shake.Amplitude * intensity;
                }
                else
                {
                    // Directional (oscillates along axis)
                    Vector2 dir = shake.Direction;
                    if (dir.LengthSquared() > 0)
                        dir.Normalize();

                    float amount = (float)Math.Sin(phase) * shake.Amplitude * intensity;
                    currentOffset += dir * amount;
                }
            }
        }

        /// <summary>
        /// Get camera transform matrix with shake applied.
        /// Use this instead of Main.GameViewMatrix when drawing.
        /// </summary>
        public Matrix GetShakeTransformMatrix()
        {
            return Matrix.CreateTranslation(currentOffset.X, currentOffset.Y, 0f);
        }

        /// <summary>
        /// Apply shake offset to a position.
        /// </summary>
        public Vector2 ApplyShake(Vector2 position)
        {
            return position + currentOffset;
        }

        private class Shake
        {
            public float Amplitude;
            public float Frequency;
            public float Duration;
            public float Age;
            public Vector2 Direction;
            public DecayCurve Decay;

            public float GetIntensity()
            {
                float progress = Age / Duration;

                return Decay switch
                {
                    DecayCurve.Linear => 1f - progress,
                    DecayCurve.Exponential => (float)Math.Pow(1f - progress, 3),
                    DecayCurve.EaseOut => 1f - (float)Math.Pow(progress, 2),
                    DecayCurve.EaseIn => (float)Math.Pow(1f - progress, 2),
                    DecayCurve.Bounce => (float)Math.Abs(Math.Sin(progress * MathHelper.Pi * 3f)) * (1f - progress),
                    _ => 1f - progress
                };
            }
        }
    }

    /// <summary>
    /// Shake decay curve types.
    /// </summary>
    public enum DecayCurve
    {
        /// <summary>Constant rate of fade.</summary>
        Linear,
        /// <summary>Starts fast, slows down (natural feeling).</summary>
        Exponential,
        /// <summary>Starts slow, speeds up.</summary>
        EaseIn,
        /// <summary>Starts fast, slows down.</summary>
        EaseOut,
        /// <summary>Bouncing oscillation fade.</summary>
        Bounce
    }

    /// <summary>
    /// Extension methods for easy screen shake access.
    /// </summary>
    public static class ScreenShakeExtensions
    {
        /// <summary>
        /// Add screen shake at position with auto-distance falloff.
        /// </summary>
        public static void ShakeScreen(this Entity entity, float intensity)
        {
            if (ScreenShakeManager.Instance == null) return;

            float distance = Vector2.Distance(entity.Center, Main.LocalPlayer.Center);
            float falloff = MathHelper.Clamp(300f / (distance + 1f), 0f, 1f);

            ScreenShakeManager.Instance.AddImpactShake(intensity * falloff);
        }

        /// <summary>
        /// Add directional shake pushing away from entity.
        /// </summary>
        public static void ShakeScreenDirectional(this Entity entity, float intensity)
        {
            ScreenShakeManager.Instance?.AddDirectionalShake(intensity, entity.Center);
        }

        /// <summary>
        /// Add trauma for cumulative shake effect.
        /// </summary>
        public static void AddTrauma(this Entity entity, float amount)
        {
            if (ScreenShakeManager.Instance == null) return;

            float distance = Vector2.Distance(entity.Center, Main.LocalPlayer.Center);
            float falloff = MathHelper.Clamp(400f / (distance + 1f), 0f, 1f);

            ScreenShakeManager.Instance.AddTrauma(amount * falloff);
        }
    }
}
