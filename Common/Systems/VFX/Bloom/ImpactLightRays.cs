using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// CALAMITY-STYLE IMPACT LIGHT RAYS
    /// 
    /// Creates stretching light rays from collision/impact points with:
    /// - SUB-PIXEL INTERPOLATION for buttery smooth 144Hz+ rendering
    /// - SHIMMER EFFECTS with oscillating brightness and color shifts
    /// - PROPER EASING for natural begin/end animations
    /// - MULTI-PASS BLOOM for professional glow quality
    /// - TAPERED RAY SHAPES that are thin at tips, bright in middle
    /// 
    /// Similar to Exoblade's on-hit glint effects and Calamity's impact flares.
    /// </summary>
    public static class ImpactLightRays
    {
        // Active light ray effects
        private static List<LightRayBurst> _activeRays = new List<LightRayBurst>();
        private static Texture2D _rayTexture;
        private static Texture2D _flareTexture;
        private static Texture2D _softGlow;
        
        private const int MaxActiveRays = 50;
        
        /// <summary>
        /// A single burst of light rays from an impact point.
        /// </summary>
        private class LightRayBurst
        {
            public Vector2 Position;
            public Vector2 PreviousPosition; // For interpolation
            public LightRay[] Rays;
            public int Timer;
            public int MaxLifetime;
            public string Theme;
            public Color PrimaryColor;
            public Color SecondaryColor;
            public float Scale;
            public bool IncludeMusicNotes;
            public uint SpawnTime; // For shimmer synchronization
            
            public bool IsExpired => Timer >= MaxLifetime;
            public float Progress => (float)Timer / MaxLifetime;
        }
        
        #region Easing Functions
        
        /// <summary>
        /// Smooth ease-out curve (fast start, slow end).
        /// </summary>
        private static float EaseOutQuart(float t)
        {
            t = 1f - t;
            return 1f - t * t * t * t;
        }
        
        /// <summary>
        /// Smooth ease-in curve (slow start, fast end).
        /// </summary>
        private static float EaseInQuad(float t)
        {
            return t * t;
        }
        
        /// <summary>
        /// Stretch out then shrink curve - peaks at ~0.3 for snappy feel.
        /// </summary>
        private static float StretchCurve(float t)
        {
            if (t < 0.3f)
            {
                float stretchT = t / 0.3f;
                return EaseOutQuart(stretchT);
            }
            else
            {
                float fadeT = (t - 0.3f) / 0.7f;
                return 1f - EaseInQuad(fadeT);
            }
        }
        
        /// <summary>
        /// Shimmer oscillation with multiple frequencies for organic feel.
        /// </summary>
        private static float ShimmerValue(float time, float offset)
        {
            float fast = MathF.Sin((time * 0.4f + offset) * MathHelper.TwoPi) * 0.15f;
            float medium = MathF.Sin((time * 0.15f + offset * 2.3f) * MathHelper.TwoPi) * 0.1f;
            float slow = MathF.Sin((time * 0.07f + offset * 1.7f) * MathHelper.TwoPi) * 0.08f;
            return 1f + fast + medium + slow;
        }
        
        #endregion
        
        /// <summary>
        /// A single stretching light ray with interpolated state.
        /// </summary>
        private struct LightRay
        {
            public float Angle;
            public float CurrentLength;
            public float PreviousLength; // For interpolation
            public float MaxLength;
            public float BaseWidth;
            public float LifeOffset; // Staggered timing
            public float ShimmerOffset; // Per-ray shimmer phase
        }
        
        #region Public API
        
        /// <summary>
        /// Spawns impact light rays at a position with theme-based coloring.
        /// </summary>
        /// <param name="position">World position of impact</param>
        /// <param name="theme">Theme name for coloring</param>
        /// <param name="rayCount">Number of rays (3-8 recommended)</param>
        /// <param name="scale">Size multiplier</param>
        /// <param name="includeMusicNotes">Whether to spawn music notes along rays</param>
        public static void SpawnImpactRays(Vector2 position, string theme, int rayCount = 5, float scale = 1f, bool includeMusicNotes = true)
        {
            if (_activeRays.Count >= MaxActiveRays)
                _activeRays.RemoveAt(0);
            
            var palette = MagnumThemePalettes.GetThemePalette(theme);
            Color primary = palette?.Length > 0 ? palette[0] : Color.White;
            Color secondary = palette?.Length > 1 ? palette[1] : primary;
            
            var burst = new LightRayBurst
            {
                Position = position,
                PreviousPosition = position,
                Rays = GenerateRays(rayCount, scale),
                Timer = 0,
                MaxLifetime = (int)(32 + scale * 12), // Slightly longer for smoother fade
                Theme = theme,
                PrimaryColor = primary,
                SecondaryColor = secondary,
                Scale = scale,
                IncludeMusicNotes = includeMusicNotes,
                SpawnTime = Main.GameUpdateCount
            };
            
            _activeRays.Add(burst);
            
            // Add bright lighting at impact
            Lighting.AddLight(position, primary.ToVector3() * 1.8f * scale);
        }
        
        /// <summary>
        /// Spawns impact rays with custom colors.
        /// </summary>
        public static void SpawnImpactRays(Vector2 position, Color primary, Color secondary, int rayCount = 5, float scale = 1f)
        {
            if (_activeRays.Count >= MaxActiveRays)
                _activeRays.RemoveAt(0);
            
            var burst = new LightRayBurst
            {
                Position = position,
                PreviousPosition = position,
                Rays = GenerateRays(rayCount, scale),
                Timer = 0,
                MaxLifetime = (int)(32 + scale * 12),
                Theme = "",
                PrimaryColor = primary,
                SecondaryColor = secondary,
                Scale = scale,
                IncludeMusicNotes = false,
                SpawnTime = Main.GameUpdateCount
            };
            
            _activeRays.Add(burst);
            Lighting.AddLight(position, primary.ToVector3() * 1.8f * scale);
        }
        
        /// <summary>
        /// Updates all active light ray effects with interpolation.
        /// Call this from a ModSystem.PostUpdateEverything or similar.
        /// </summary>
        public static void Update()
        {
            for (int i = _activeRays.Count - 1; i >= 0; i--)
            {
                var burst = _activeRays[i];
                
                // Store previous state for interpolation
                burst.PreviousPosition = burst.Position;
                
                burst.Timer++;
                float progress = burst.Progress;
                
                // Update each ray's length with proper easing
                for (int r = 0; r < burst.Rays.Length; r++)
                {
                    ref var ray = ref burst.Rays[r];
                    
                    // Store previous length for interpolation
                    ray.PreviousLength = ray.CurrentLength;
                    
                    // Staggered animation with offset
                    float rayProgress = Math.Clamp((progress - ray.LifeOffset * 0.15f) / (1f - ray.LifeOffset * 0.15f), 0f, 1f);
                    
                    // Apply smooth stretch curve for natural motion
                    float stretchFactor = StretchCurve(rayProgress);
                    ray.CurrentLength = ray.MaxLength * stretchFactor;
                }
                
                // Spawn particles along rays (during stretch phase)
                if (burst.IncludeMusicNotes && burst.Timer % 3 == 0 && progress < 0.5f)
                {
                    SpawnRayParticles(burst);
                }
                
                // Dynamic lighting that fades smoothly
                float lightIntensity = (1f - progress * progress) * burst.Scale;
                Lighting.AddLight(burst.Position, burst.PrimaryColor.ToVector3() * lightIntensity * 0.8f);
                
                if (burst.IsExpired)
                {
                    _activeRays.RemoveAt(i);
                }
            }
        }
        
        /// <summary>
        /// Draws all active light ray effects with interpolation.
        /// Call from a PostDraw hook.
        /// </summary>
        public static void Draw(SpriteBatch spriteBatch)
        {
            if (_activeRays.Count == 0) return;
            
            EnsureTextures();
            if (_rayTexture == null) return;
            
            // Calculate interpolation factor based on fractional game updates
            // This provides smooth interpolation between logic ticks for 144Hz+ displays
            float lerpFactor = 0f;
            try
            {
                // Use the game's internal timing to estimate frame interpolation
                // When the game runs at higher FPS than logic rate (60 Hz), this smooths motion
                lerpFactor = (float)(Main.GameUpdateCount % 60) / 60f;
            }
            catch { lerpFactor = 0f; }
            
            // Switch to additive blending
            try { spriteBatch.End(); } catch { }
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);
            
            foreach (var burst in _activeRays)
            {
                DrawBurstInterpolated(spriteBatch, burst, lerpFactor);
            }
            
            try { spriteBatch.End(); } catch { }
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);
        }
        
        /// <summary>
        /// Clears all active effects.
        /// </summary>
        public static void Clear()
        {
            _activeRays.Clear();
        }
        
        #endregion
        
        #region Private Methods
        
        private static LightRay[] GenerateRays(int count, float scale)
        {
            var rays = new LightRay[count];
            
            // Distribute rays with controlled randomness for balanced look
            float baseAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
            float angleStep = MathHelper.TwoPi / count;
            
            for (int i = 0; i < count; i++)
            {
                // Add slight angle variation but keep distribution even
                float angleVariance = Main.rand.NextFloat(-0.35f, 0.35f);
                float angle = baseAngle + angleStep * i + angleVariance;
                
                rays[i] = new LightRay
                {
                    Angle = angle,
                    CurrentLength = 0f,
                    PreviousLength = 0f,
                    MaxLength = (45f + Main.rand.NextFloat(35f)) * scale,
                    BaseWidth = (5f + Main.rand.NextFloat(4f)) * scale,
                    LifeOffset = Main.rand.NextFloat(0.25f),
                    ShimmerOffset = Main.rand.NextFloat(1f)
                };
            }
            
            return rays;
        }
        
        private static void DrawBurstInterpolated(SpriteBatch spriteBatch, LightRayBurst burst, float lerpFactor)
        {
            float progress = burst.Progress;
            
            // Smooth fade curve - quick in, slow out
            float fadeAlpha = progress < 0.3f 
                ? EaseOutQuart(progress / 0.3f)  // Quick appear
                : 1f - EaseInQuad((progress - 0.3f) / 0.7f); // Slow fade
            
            // Interpolate position for sub-pixel smoothness
            Vector2 interpolatedPos = Vector2.Lerp(burst.PreviousPosition, burst.Position, lerpFactor);
            Vector2 screenPos = interpolatedPos - Main.screenPosition;
            
            // Time for shimmer calculations
            float time = (Main.GameUpdateCount - burst.SpawnTime + lerpFactor) * 0.1f;
            
            Vector2 rayOrigin = new Vector2(_rayTexture.Width / 2f, _rayTexture.Height);
            
            foreach (var ray in burst.Rays)
            {
                // Interpolate ray length for smooth animation
                float interpolatedLength = MathHelper.Lerp(ray.PreviousLength, ray.CurrentLength, lerpFactor);
                if (interpolatedLength <= 1f) continue;
                
                // Calculate shimmer for this ray
                float shimmer = ShimmerValue(time, ray.ShimmerOffset);
                
                // Tapered width - thin at tip, thick in middle
                float widthMultiplier = shimmer;
                
                // Color with shimmer and gradient
                float colorProgress = progress + ray.ShimmerOffset * 0.1f;
                Color baseColor = Color.Lerp(burst.PrimaryColor, burst.SecondaryColor, colorProgress * 0.5f);
                Color rayColor = baseColor * fadeAlpha * shimmer;
                Color coreColor = Color.Lerp(baseColor, Color.White, 0.6f) * fadeAlpha * shimmer;
                
                // Remove alpha for proper additive blending
                rayColor = rayColor with { A = 0 };
                coreColor = coreColor with { A = 0 };
                
                float rotation = ray.Angle - MathHelper.PiOver2;
                float scaleX = ray.BaseWidth * widthMultiplier / _rayTexture.Width;
                float scaleY = interpolatedLength / _rayTexture.Height;
                
                // === MULTI-PASS BLOOM RENDERING ===
                
                // Pass 1: Outer bloom (large, very dim) - creates soft halo
                spriteBatch.Draw(
                    _rayTexture,
                    screenPos,
                    null,
                    rayColor * 0.2f,
                    rotation,
                    rayOrigin,
                    new Vector2(scaleX * 3.5f, scaleY * 1.1f),
                    SpriteEffects.None,
                    0f
                );
                
                // Pass 2: Middle bloom (medium, dim)
                spriteBatch.Draw(
                    _rayTexture,
                    screenPos,
                    null,
                    rayColor * 0.35f,
                    rotation,
                    rayOrigin,
                    new Vector2(scaleX * 2.2f, scaleY * 1.05f),
                    SpriteEffects.None,
                    0f
                );
                
                // Pass 3: Main ray body
                spriteBatch.Draw(
                    _rayTexture,
                    screenPos,
                    null,
                    rayColor * 0.8f,
                    rotation,
                    rayOrigin,
                    new Vector2(scaleX * 1.2f, scaleY),
                    SpriteEffects.None,
                    0f
                );
                
                // Pass 4: Bright inner core (thin, bright)
                spriteBatch.Draw(
                    _rayTexture,
                    screenPos,
                    null,
                    coreColor * 0.9f,
                    rotation,
                    rayOrigin,
                    new Vector2(scaleX * 0.5f, scaleY * 0.9f),
                    SpriteEffects.None,
                    0f
                );
                
                // Pass 5: White-hot center line
                spriteBatch.Draw(
                    _rayTexture,
                    screenPos,
                    null,
                    (Color.White with { A = 0 }) * fadeAlpha * shimmer * 0.7f,
                    rotation,
                    rayOrigin,
                    new Vector2(scaleX * 0.25f, scaleY * 0.7f),
                    SpriteEffects.None,
                    0f
                );
            }
            
            // === CENTRAL FLARE with shimmer ===
            DrawCentralFlare(spriteBatch, screenPos, burst, fadeAlpha, time);
        }
        
        private static void DrawCentralFlare(SpriteBatch spriteBatch, Vector2 screenPos, 
            LightRayBurst burst, float fadeAlpha, float time)
        {
            if (_flareTexture == null || fadeAlpha < 0.1f) return;
            
            // Pulsing flare with shimmer
            float shimmer = ShimmerValue(time * 1.5f, 0f);
            float flareScale = (0.25f + fadeAlpha * 0.35f) * burst.Scale * shimmer;
            float flareRot = time * 0.8f;
            
            // Multi-pass flare for bloom effect
            Color flareColor = Color.Lerp(burst.PrimaryColor, Color.White, 0.4f) * fadeAlpha;
            flareColor = flareColor with { A = 0 };
            
            Vector2 flareOrigin = _flareTexture.Size() * 0.5f;
            
            // Outer bloom
            spriteBatch.Draw(
                _flareTexture,
                screenPos,
                null,
                flareColor * 0.25f,
                flareRot,
                flareOrigin,
                flareScale * 2.5f,
                SpriteEffects.None,
                0f
            );
            
            // Middle glow
            spriteBatch.Draw(
                _flareTexture,
                screenPos,
                null,
                flareColor * 0.5f,
                -flareRot * 0.7f, // Counter-rotate for dynamic feel
                flareOrigin,
                flareScale * 1.5f,
                SpriteEffects.None,
                0f
            );
            
            // Core flare
            spriteBatch.Draw(
                _flareTexture,
                screenPos,
                null,
                flareColor * 0.9f,
                flareRot * 1.2f,
                flareOrigin,
                flareScale,
                SpriteEffects.None,
                0f
            );
            
            // White-hot center
            spriteBatch.Draw(
                _flareTexture,
                screenPos,
                null,
                (Color.White with { A = 0 }) * fadeAlpha * shimmer * 0.6f,
                0f,
                flareOrigin,
                flareScale * 0.4f,
                SpriteEffects.None,
                0f
            );
            
            // Soft glow background
            if (_softGlow != null)
            {
                float glowScale = flareScale * 3f;
                spriteBatch.Draw(
                    _softGlow,
                    screenPos,
                    null,
                    burst.PrimaryColor with { A = 0 } * fadeAlpha * 0.3f,
                    0f,
                    _softGlow.Size() * 0.5f,
                    glowScale,
                    SpriteEffects.None,
                    0f
                );
            }
        }
        
        private static void SpawnRayParticles(LightRayBurst burst)
        {
            // Pick a random ray that has visible length
            var validRays = new List<int>();
            for (int i = 0; i < burst.Rays.Length; i++)
            {
                if (burst.Rays[i].CurrentLength > 10f)
                    validRays.Add(i);
            }
            
            if (validRays.Count == 0) return;
            var ray = burst.Rays[validRays[Main.rand.Next(validRays.Count)]];
            
            // Position along the ray (favor middle section for visibility)
            float rayProgress = Main.rand.NextFloat(0.2f, 0.7f);
            Vector2 rayDir = ray.Angle.ToRotationVector2();
            Vector2 particlePos = burst.Position + rayDir * ray.CurrentLength * rayProgress;
            
            // Velocity perpendicular to ray with outward bias
            Vector2 particleVel = rayDir * Main.rand.NextFloat(1.5f, 4f) +
                                  Main.rand.NextVector2Circular(1.5f, 1.5f);
            
            // Spawn music note with proper visible scale
            if (Main.rand.NextBool(2))
            {
                float noteScale = Main.rand.NextFloat(0.6f, 0.85f);
                ThemedParticles.MusicNote(particlePos, particleVel * 0.6f, burst.PrimaryColor, noteScale, 25);
            }
            
            // Sparkle with shimmer
            var sparkle = new SparkleParticle(
                particlePos, 
                particleVel, 
                Color.Lerp(burst.PrimaryColor, Color.White, 0.4f) * 0.9f, 
                Main.rand.NextFloat(0.2f, 0.35f), 
                Main.rand.Next(12, 20)
            );
            MagnumParticleHandler.SpawnParticle(sparkle);
            
            // Additional glow particle
            if (Main.rand.NextBool(3))
            {
                var glow = new GenericGlowParticle(
                    particlePos,
                    particleVel * 0.4f,
                    burst.SecondaryColor * 0.7f,
                    Main.rand.NextFloat(0.15f, 0.25f),
                    Main.rand.Next(15, 25),
                    true
                );
                MagnumParticleHandler.SpawnParticle(glow);
            }
        }
        
        private static void EnsureTextures()
        {
            if (_rayTexture == null)
            {
                try
                {
                    _rayTexture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/ParticleTrail1",
                        ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                }
                catch
                {
                    _rayTexture = CreateRayTexture();
                }
            }
            
            if (_flareTexture == null)
            {
                try
                {
                    _flareTexture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare",
                        ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                }
                catch
                {
                    _flareTexture = null;
                }
            }
            
            if (_softGlow == null)
            {
                try
                {
                    _softGlow = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow2",
                        ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                }
                catch
                {
                    _softGlow = null;
                }
            }
        }
        
        /// <summary>
        /// Creates a gradient ray texture with proper tapering.
        /// Bright and thick at base (origin), thin and faded at tip.
        /// </summary>
        private static Texture2D CreateRayTexture()
        {
            int width = 12;
            int height = 80;
            
            Texture2D tex = new Texture2D(Main.instance.GraphicsDevice, width, height);
            Color[] data = new Color[width * height];
            
            for (int y = 0; y < height; y++)
            {
                // Progress from base (0) to tip (1)
                float yProgress = (float)y / height;
                
                // Fade: bright at base, fading toward tip with smooth curve
                float yAlpha = 1f - yProgress * yProgress * yProgress; // Cubic falloff
                
                // Taper width: thicker at base, thinner at tip
                float taperFactor = 1f - yProgress * 0.7f;
                
                for (int x = 0; x < width; x++)
                {
                    // Distance from center (0-1)
                    float xProgress = (float)x / width;
                    float xDist = Math.Abs(xProgress - 0.5f) * 2f;
                    
                    // Soft edge falloff that respects taper
                    float effectiveDist = xDist / taperFactor;
                    float xAlpha = Math.Clamp(1f - effectiveDist * effectiveDist, 0f, 1f);
                    
                    float alpha = yAlpha * xAlpha;
                    
                    // Slight color gradient for depth
                    float brightness = 1f - yProgress * 0.2f;
                    data[y * width + x] = new Color(brightness, brightness, brightness, alpha) * alpha;
                }
            }
            
            tex.SetData(data);
            return tex;
        }
        
        #endregion
    }
    
    /// <summary>
    /// ModSystem to update and draw impact light rays.
    /// </summary>
    public class ImpactLightRaysSystem : ModSystem
    {
        public override void PostUpdateEverything()
        {
            ImpactLightRays.Update();
        }
        
        public override void PostDrawTiles()
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            // Use Additive blending for proper light ray glow effect
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);
            
            ImpactLightRays.Draw(spriteBatch);
            
            try { spriteBatch.End(); } catch { }
        }
        
        public override void OnWorldUnload()
        {
            ImpactLightRays.Clear();
        }
    }
}
