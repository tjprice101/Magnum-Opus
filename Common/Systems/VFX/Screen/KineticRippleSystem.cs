using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// KINETIC IMPACT RIPPLES SYSTEM
    /// 
    /// Implements the "Kinetic Impact Ripples" concept:
    /// - Full-screen shockwave emanates from impact point
    /// - Normal-mapped distortion (simulated via UV displacement)
    /// - Radial strips for proper vertex geometry
    /// - Multiple ripple rings with decreasing intensity
    /// 
    /// Technical Approach:
    /// - Each ripple is a radial vertex strip (ring)
    /// - UV distortion simulates normal map warping
    /// - Multiple concurrent ripples for complex effects
    /// 
    /// Usage:
    ///   KineticRippleSystem.CreateRipple(impactPos, intensity, color);
    /// </summary>
    public class KineticRippleSystem : ModSystem
    {
        private static List<KineticRipple> _activeRipples = new();
        private static Texture2D _rippleTexture;
        private static Texture2D _distortionTexture;
        
        private const int MaxActiveRipples = 8;
        private const int RingSegments = 64; // Vertices per ring
        
        #region Ripple Styles
        
        public enum RippleStyle
        {
            Impact,         // Single expanding ring
            Shockwave,      // Multiple concentric rings
            Pulse,          // Pulsing in/out effect
            Spiral,         // Rotating spiral distortion
            Chromatic       // RGB separation effect
        }
        
        #endregion
        
        #region Ripple Data
        
        private class KineticRipple
        {
            public Vector2 Center;
            public float CurrentRadius;
            public float MaxRadius;
            public float Intensity;
            public float Width;
            public Color Color;
            public RippleStyle Style;
            public int Timer;
            public int Lifetime;
            public float DistortionStrength;
            public int RingCount;
            public float RotationOffset;
            
            public bool IsComplete => Timer >= Lifetime;
            public float Progress => (float)Timer / Lifetime;
        }
        
        #endregion
        
        #region Initialization
        
        public override void Load()
        {
            if (Main.dedServ) return;
        }
        
        public override void Unload()
        {
            _activeRipples?.Clear();
            
            // Cache references and null immediately (safe on any thread)
            var ripple = _rippleTexture;
            var distortion = _distortionTexture;
            _rippleTexture = null;
            _distortionTexture = null;
            
            // Queue texture disposal on main thread to avoid ThreadStateException
            Main.QueueMainThreadAction(() =>
            {
                try
                {
                    ripple?.Dispose();
                    distortion?.Dispose();
                }
                catch { }
            });
        }
        
        public override void PostUpdateEverything()
        {
            for (int i = _activeRipples.Count - 1; i >= 0; i--)
            {
                UpdateRipple(_activeRipples[i]);
                
                if (_activeRipples[i].IsComplete)
                    _activeRipples.RemoveAt(i);
            }
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Creates a kinetic ripple effect at the specified position.
        /// </summary>
        public static void CreateRipple(
            Vector2 center,
            float maxRadius = 400f,
            float intensity = 1f,
            Color? color = null,
            RippleStyle style = RippleStyle.Impact,
            int lifetime = 40)
        {
            if (_activeRipples.Count >= MaxActiveRipples) return;
            
            var ripple = new KineticRipple
            {
                Center = center,
                CurrentRadius = 0f,
                MaxRadius = maxRadius,
                Intensity = intensity,
                Width = maxRadius * 0.15f,
                Color = color ?? Color.White,
                Style = style,
                Timer = 0,
                Lifetime = lifetime,
                DistortionStrength = 15f * intensity,
                RingCount = style == RippleStyle.Shockwave ? 3 : 1,
                RotationOffset = 0f
            };
            
            _activeRipples.Add(ripple);
        }
        
        /// <summary>
        /// Creates an impact shockwave (expanding ring).
        /// </summary>
        public static void CreateImpact(Vector2 position, float intensity = 1f, Color? color = null)
        {
            CreateRipple(position, 300f * intensity, intensity, color, RippleStyle.Impact, 35);
        }
        
        /// <summary>
        /// Creates a multi-ring shockwave (Calamity boss style).
        /// </summary>
        public static void CreateShockwave(Vector2 position, float intensity = 1f, Color? color = null)
        {
            CreateRipple(position, 500f * intensity, intensity, color, RippleStyle.Shockwave, 50);
        }
        
        /// <summary>
        /// Creates a chromatic aberration ripple.
        /// </summary>
        public static void CreateChromaticRipple(Vector2 position, float intensity = 1f)
        {
            CreateRipple(position, 400f * intensity, intensity, Color.White, RippleStyle.Chromatic, 45);
        }
        
        /// <summary>
        /// Creates a pulsing ripple (expands and contracts).
        /// </summary>
        public static void CreatePulse(Vector2 position, int pulseCount = 3, Color? color = null)
        {
            var ripple = new KineticRipple
            {
                Center = position,
                CurrentRadius = 0f,
                MaxRadius = 200f,
                Intensity = 1f,
                Width = 30f,
                Color = color ?? new Color(100, 200, 255),
                Style = RippleStyle.Pulse,
                Timer = 0,
                Lifetime = 20 * pulseCount,
                DistortionStrength = 10f,
                RingCount = 1,
                RotationOffset = 0f
            };
            
            if (_activeRipples.Count < MaxActiveRipples)
                _activeRipples.Add(ripple);
        }
        
        #endregion
        
        #region Update Logic
        
        private static void UpdateRipple(KineticRipple ripple)
        {
            ripple.Timer++;
            
            switch (ripple.Style)
            {
                case RippleStyle.Impact:
                case RippleStyle.Shockwave:
                case RippleStyle.Chromatic:
                    // Smooth expansion with easing
                    float t = ripple.Progress;
                    float eased = EaseOutQuad(t);
                    ripple.CurrentRadius = ripple.MaxRadius * eased;
                    
                    // Width narrows as it expands
                    ripple.Width = MathHelper.Lerp(ripple.MaxRadius * 0.2f, ripple.MaxRadius * 0.05f, t);
                    
                    // Intensity fades
                    ripple.Intensity = MathHelper.Lerp(1f, 0f, t);
                    break;
                    
                case RippleStyle.Pulse:
                    // Oscillating radius
                    float pulseT = (ripple.Timer % 20) / 20f;
                    float pulse = MathF.Sin(pulseT * MathHelper.Pi);
                    ripple.CurrentRadius = ripple.MaxRadius * pulse;
                    ripple.Intensity = pulse;
                    break;
                    
                case RippleStyle.Spiral:
                    ripple.CurrentRadius = ripple.MaxRadius * EaseOutQuad(ripple.Progress);
                    ripple.RotationOffset += 0.1f;
                    ripple.Intensity = 1f - ripple.Progress;
                    break;
            }
        }
        
        private static float EaseOutQuad(float t)
        {
            return 1f - (1f - t) * (1f - t);
        }
        
        private static float EaseOutCubic(float t)
        {
            return 1f - MathF.Pow(1f - t, 3f);
        }
        
        #endregion
        
        #region Rendering
        
        /// <summary>
        /// Renders all active ripples.
        /// </summary>
        public static void RenderAll(SpriteBatch spriteBatch)
        {
            if (_activeRipples.Count == 0) return;
            
            EnsureTextures();
            
            foreach (var ripple in _activeRipples)
            {
                RenderRipple(spriteBatch, ripple);
            }
        }
        
        private static void RenderRipple(SpriteBatch spriteBatch, KineticRipple ripple)
        {
            if (ripple.CurrentRadius < 1f || ripple.Intensity < 0.01f) return;
            
            try { spriteBatch.End(); } catch { }
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearWrap,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Vector2 screenCenter = ripple.Center - Main.screenPosition;
            
            // Draw rings
            for (int ring = 0; ring < ripple.RingCount; ring++)
            {
                float ringDelay = ring * 0.15f;
                float ringProgress = MathHelper.Clamp(ripple.Progress - ringDelay, 0f, 1f);
                
                if (ringProgress <= 0f) continue;
                
                float ringRadius = ripple.CurrentRadius * (1f - ring * 0.2f);
                float ringIntensity = ripple.Intensity * (1f - ring * 0.3f);
                
                if (ripple.Style == RippleStyle.Chromatic)
                {
                    // Draw RGB-separated rings for chromatic aberration
                    DrawRing(spriteBatch, screenCenter, ringRadius - 3f, ripple.Width, 
                             new Color(255, 100, 100) * ringIntensity * 0.5f, ripple.RotationOffset);
                    DrawRing(spriteBatch, screenCenter, ringRadius, ripple.Width, 
                             new Color(100, 255, 100) * ringIntensity * 0.5f, ripple.RotationOffset);
                    DrawRing(spriteBatch, screenCenter, ringRadius + 3f, ripple.Width, 
                             new Color(100, 100, 255) * ringIntensity * 0.5f, ripple.RotationOffset);
                }
                else
                {
                    // Standard ring
                    DrawRing(spriteBatch, screenCenter, ringRadius, ripple.Width, 
                             ripple.Color * ringIntensity, ripple.RotationOffset);
                }
            }
            
            // Draw center flash for impact style
            if (ripple.Style == RippleStyle.Impact && ripple.Progress < 0.3f)
            {
                float flashIntensity = 1f - ripple.Progress / 0.3f;
                float flashScale = 1f + ripple.Progress * 3f;
                
                spriteBatch.Draw(
                    _rippleTexture,
                    screenCenter,
                    null,
                    Color.White * flashIntensity * 0.8f,
                    0f,
                    new Vector2(_rippleTexture.Width / 2f, _rippleTexture.Height / 2f),
                    flashScale,
                    SpriteEffects.None,
                    0f
                );
            }
            
            try { spriteBatch.End(); } catch { }
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        
        /// <summary>
        /// Draws a ring using radial segments.
        /// </summary>
        private static void DrawRing(SpriteBatch spriteBatch, Vector2 center, float radius, 
                                     float width, Color color, float rotationOffset = 0f)
        {
            if (radius <= 0f) return;
            
            Vector2 texOrigin = new Vector2(_rippleTexture.Width / 2f, _rippleTexture.Height / 2f);
            
            for (int i = 0; i < RingSegments; i++)
            {
                float angle = MathHelper.TwoPi * i / RingSegments + rotationOffset;
                Vector2 pos = center + angle.ToRotationVector2() * radius;
                
                // Calculate UV variation along ring for visual interest
                float uvOffset = (float)i / RingSegments + Main.GameUpdateCount * 0.01f;
                float variation = 0.8f + MathF.Sin(uvOffset * MathHelper.TwoPi * 4f) * 0.2f;
                
                // Draw segment
                float segmentAngle = angle + MathHelper.PiOver2;
                float segmentScale = width / _rippleTexture.Height;
                
                spriteBatch.Draw(
                    _rippleTexture,
                    pos,
                    null,
                    color * variation,
                    segmentAngle,
                    texOrigin,
                    new Vector2(0.2f, segmentScale),
                    SpriteEffects.None,
                    0f
                );
            }
        }
        
        #endregion
        
        #region Screen Distortion Integration
        
        /// <summary>
        /// Gets the current UV distortion amount at a screen position.
        /// Can be used by other systems for integrated distortion effects.
        /// </summary>
        public static Vector2 GetDistortionAtPosition(Vector2 worldPosition)
        {
            Vector2 totalDistortion = Vector2.Zero;
            
            foreach (var ripple in _activeRipples)
            {
                float distance = Vector2.Distance(worldPosition, ripple.Center);
                float ringDistance = MathF.Abs(distance - ripple.CurrentRadius);
                
                if (ringDistance < ripple.Width)
                {
                    float distortionMask = 1f - ringDistance / ripple.Width;
                    distortionMask *= ripple.Intensity;
                    
                    // Direction from center to position
                    Vector2 direction = (worldPosition - ripple.Center).SafeNormalize(Vector2.Zero);
                    
                    // Push outward from center
                    totalDistortion += direction * distortionMask * ripple.DistortionStrength;
                }
            }
            
            return totalDistortion;
        }
        
        #endregion
        
        #region Texture Generation
        
        private static void EnsureTextures()
        {
            if (_rippleTexture == null || _rippleTexture.IsDisposed)
                _rippleTexture = CreateRippleTexture(32, 32);
            
            if (_distortionTexture == null || _distortionTexture.IsDisposed)
                _distortionTexture = CreateDistortionTexture(64, 64);
        }
        
        /// <summary>
        /// Creates a soft gradient texture for rings.
        /// </summary>
        private static Texture2D CreateRippleTexture(int width, int height)
        {
            var texture = new Texture2D(Main.graphics.GraphicsDevice, width, height);
            var data = new Color[width * height];
            
            float centerX = width / 2f;
            float centerY = height / 2f;
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dx = (x - centerX) / centerX;
                    float dy = (y - centerY) / centerY;
                    float dist = MathF.Sqrt(dx * dx + dy * dy);
                    
                    // Soft circular gradient
                    float alpha = MathF.Max(0f, 1f - dist);
                    alpha = MathF.Pow(alpha, 0.5f); // Softer falloff
                    
                    data[y * width + x] = Color.White * alpha;
                }
            }
            
            texture.SetData(data);
            return texture;
        }
        
        /// <summary>
        /// Creates a normal-map style distortion texture.
        /// </summary>
        private static Texture2D CreateDistortionTexture(int width, int height)
        {
            var texture = new Texture2D(Main.graphics.GraphicsDevice, width, height);
            var data = new Color[width * height];
            
            float centerX = width / 2f;
            float centerY = height / 2f;
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dx = (x - centerX) / centerX;
                    float dy = (y - centerY) / centerY;
                    float dist = MathF.Sqrt(dx * dx + dy * dy);
                    
                    // Normal map encoding (direction in RG, intensity in B)
                    if (dist > 0.001f)
                    {
                        float nx = dx / dist * 0.5f + 0.5f;
                        float ny = dy / dist * 0.5f + 0.5f;
                        float intensity = MathF.Max(0f, 1f - dist);
                        
                        data[y * width + x] = new Color(nx, ny, intensity);
                    }
                    else
                    {
                        data[y * width + x] = new Color(0.5f, 0.5f, 0f);
                    }
                }
            }
            
            texture.SetData(data);
            return texture;
        }
        
        #endregion
    }
}
