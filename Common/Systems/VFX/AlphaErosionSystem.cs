using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// ALPHA EROSION / DISSOLVE EFFECT SYSTEM
    /// 
    /// Implements Calamity-style dissolve effects where:
    /// - Noise texture is sampled at each pixel
    /// - Pixels with noise value below threshold are discarded
    /// - Creates "burning away" or "crumbling" effect
    /// 
    /// Technical Implementation:
    /// - Progress uniform controls erosion threshold
    /// - Noise texture determines which pixels dissolve first
    /// - Edge glow at the dissolve boundary
    /// 
    /// Usage:
    ///   AlphaErosionSystem.CreateDissolve(position, texture, color, duration);
    /// </summary>
    public class AlphaErosionSystem : ModSystem
    {
        private static List<DissolveEffect> _activeDissolves = new();
        private static Texture2D _noiseTexture;
        private static Effect _dissolveEffect;
        
        private const int MaxActiveDissolves = 50;
        
        #region Dissolve Effect Data
        
        private class DissolveEffect
        {
            public Vector2 Position;
            public Texture2D Texture;
            public Color PrimaryColor;
            public Color EdgeColor;
            public float Scale;
            public float Rotation;
            public int Timer;
            public int MaxLifetime;
            public DissolveStyle Style;
            public float EdgeWidth;
            
            public bool IsExpired => Timer >= MaxLifetime;
            public float Progress => (float)Timer / MaxLifetime;
        }
        
        public enum DissolveStyle
        {
            BurnAway,       // Burns from edges inward (high noise dissolves first)
            Crumble,        // Random crumbling pattern
            Implode,        // Center dissolves first
            Explode,        // Edges dissolve first
            Directional     // Dissolves in a direction
        }
        
        #endregion
        
        #region Initialization
        
        public override void Load()
        {
            if (Main.dedServ) return;
        }
        
        public override void Unload()
        {
            _activeDissolves?.Clear();
            
            // Cache reference and null immediately (safe on any thread)
            var noise = _noiseTexture;
            _noiseTexture = null;
            _dissolveEffect = null;
            
            // Queue texture disposal on main thread to avoid ThreadStateException
            if (noise != null)
            {
                Main.QueueMainThreadAction(() =>
                {
                    try { noise.Dispose(); } catch { }
                });
            }
        }
        
        public override void PostUpdateEverything()
        {
            for (int i = _activeDissolves.Count - 1; i >= 0; i--)
            {
                _activeDissolves[i].Timer++;
                if (_activeDissolves[i].IsExpired)
                    _activeDissolves.RemoveAt(i);
            }
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Creates a dissolve effect at the specified position.
        /// </summary>
        public static void CreateDissolve(
            Vector2 position,
            Texture2D texture,
            Color primaryColor,
            int duration = 30,
            float scale = 1f,
            DissolveStyle style = DissolveStyle.BurnAway,
            Color? edgeColor = null)
        {
            if (_activeDissolves.Count >= MaxActiveDissolves)
                _activeDissolves.RemoveAt(0);
            
            _activeDissolves.Add(new DissolveEffect
            {
                Position = position,
                Texture = texture,
                PrimaryColor = primaryColor,
                EdgeColor = edgeColor ?? Color.White,
                Scale = scale,
                Rotation = 0f,
                Timer = 0,
                MaxLifetime = duration,
                Style = style,
                EdgeWidth = 0.15f
            });
        }
        
        /// <summary>
        /// Creates a beam impact dissolve (radial strips that burn away).
        /// </summary>
        public static void CreateBeamImpactDissolve(
            Vector2 impactPoint,
            Color primaryColor,
            int rayCount = 8,
            float rayLength = 60f,
            int duration = 25)
        {
            EnsureNoiseTexture();
            
            for (int i = 0; i < rayCount; i++)
            {
                float angle = MathHelper.TwoPi * i / rayCount + Main.rand.NextFloat(-0.2f, 0.2f);
                float length = rayLength * Main.rand.NextFloat(0.7f, 1.0f);
                
                // Create a ray dissolve effect
                CreateRayDissolve(impactPoint, angle, length, primaryColor, duration);
            }
        }
        
        /// <summary>
        /// Creates a single ray dissolve effect (for beam impacts).
        /// </summary>
        public static void CreateRayDissolve(
            Vector2 origin,
            float angle,
            float length,
            Color color,
            int duration)
        {
            // This would typically spawn particles along the ray that dissolve
            // For now, spawn dissolving particles along the path
            int segments = (int)(length / 10f);
            
            for (int i = 0; i < segments; i++)
            {
                float t = (float)i / segments;
                Vector2 pos = origin + angle.ToRotationVector2() * (length * t);
                
                // Stagger the dissolve timing
                int staggeredDuration = (int)(duration * (0.5f + t * 0.5f));
                
                // Spawn a small dissolving particle
                SpawnDissolvingDust(pos, color, staggeredDuration);
            }
        }
        
        private static void SpawnDissolvingDust(Vector2 position, Color color, int lifetime)
        {
            // Use dust with alpha fade simulating dissolve
            Dust dust = Dust.NewDustPerfect(position, Terraria.ID.DustID.MagicMirror, 
                Main.rand.NextVector2Circular(2f, 2f), 0, color, 1.2f);
            dust.noGravity = true;
            dust.fadeIn = 1f;
        }
        
        #endregion
        
        #region Rendering
        
        /// <summary>
        /// Renders all active dissolve effects.
        /// </summary>
        public static void RenderAll(SpriteBatch spriteBatch)
        {
            if (_activeDissolves.Count == 0) return;
            
            EnsureNoiseTexture();
            
            foreach (var dissolve in _activeDissolves)
            {
                RenderDissolve(spriteBatch, dissolve);
            }
        }
        
        private static void RenderDissolve(SpriteBatch spriteBatch, DissolveEffect effect)
        {
            // Without a shader, we simulate dissolve with alpha and scale
            float progress = effect.Progress;
            float easedProgress = EaseInQuad(progress);
            
            // Calculate alpha based on style
            float alpha = 1f - easedProgress;
            
            // Add fragmentation simulation - draw multiple offset copies fading
            int fragments = 3 + (int)(easedProgress * 5);
            
            for (int i = 0; i < fragments; i++)
            {
                float fragmentProgress = easedProgress + i * 0.1f;
                if (fragmentProgress > 1f) continue;
                
                float fragmentAlpha = (1f - fragmentProgress) * 0.5f;
                Vector2 offset = GetDissolveOffset(effect.Style, fragmentProgress, i);
                
                Color fragmentColor = Color.Lerp(effect.PrimaryColor, effect.EdgeColor, fragmentProgress);
                
                // Draw fragment
                spriteBatch.Draw(
                    effect.Texture,
                    effect.Position + offset - Main.screenPosition,
                    null,
                    fragmentColor * fragmentAlpha,
                    effect.Rotation + i * 0.1f * fragmentProgress,
                    new Vector2(effect.Texture.Width / 2f, effect.Texture.Height / 2f),
                    effect.Scale * (1f - fragmentProgress * 0.3f),
                    SpriteEffects.None,
                    0f
                );
            }
            
            // Draw edge glow
            if (progress > 0.2f && progress < 0.9f)
            {
                float edgeIntensity = MathF.Sin((progress - 0.2f) / 0.7f * MathHelper.Pi);
                
                spriteBatch.Draw(
                    effect.Texture,
                    effect.Position - Main.screenPosition,
                    null,
                    effect.EdgeColor * edgeIntensity * 0.6f,
                    effect.Rotation,
                    new Vector2(effect.Texture.Width / 2f, effect.Texture.Height / 2f),
                    effect.Scale * 1.1f,
                    SpriteEffects.None,
                    0f
                );
            }
        }
        
        private static Vector2 GetDissolveOffset(DissolveStyle style, float progress, int fragmentIndex)
        {
            switch (style)
            {
                case DissolveStyle.Explode:
                    float explodeAngle = fragmentIndex * MathHelper.TwoPi / 8f;
                    return explodeAngle.ToRotationVector2() * (progress * 30f);
                    
                case DissolveStyle.Implode:
                    float implodeAngle = fragmentIndex * MathHelper.TwoPi / 8f;
                    return implodeAngle.ToRotationVector2() * ((1f - progress) * 20f);
                    
                case DissolveStyle.Directional:
                    return new Vector2(progress * 40f, 0f);
                    
                case DissolveStyle.Crumble:
                    return new Vector2(
                        MathF.Sin(fragmentIndex * 1.7f) * progress * 15f,
                        progress * 20f + MathF.Cos(fragmentIndex * 2.3f) * progress * 10f
                    );
                    
                case DissolveStyle.BurnAway:
                default:
                    return Main.rand.NextVector2Circular(progress * 5f, progress * 5f);
            }
        }
        
        #endregion
        
        #region Noise Texture Generation
        
        private static void EnsureNoiseTexture()
        {
            if (_noiseTexture == null || _noiseTexture.IsDisposed)
            {
                _noiseTexture = GeneratePerlinNoiseTexture(128, 128);
            }
        }
        
        /// <summary>
        /// Generates a Perlin-like noise texture for dissolve effects.
        /// </summary>
        private static Texture2D GeneratePerlinNoiseTexture(int width, int height)
        {
            var texture = new Texture2D(Main.graphics.GraphicsDevice, width, height);
            var data = new Color[width * height];
            
            // Simple layered noise (pseudo-Perlin)
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float noise = 0f;
                    float amplitude = 1f;
                    float frequency = 1f;
                    float maxValue = 0f;
                    
                    // 4 octaves of noise
                    for (int octave = 0; octave < 4; octave++)
                    {
                        float sampleX = x * frequency / 16f;
                        float sampleY = y * frequency / 16f;
                        
                        // Simple hash-based pseudo-noise
                        float value = HashNoise(sampleX, sampleY);
                        noise += value * amplitude;
                        
                        maxValue += amplitude;
                        amplitude *= 0.5f;
                        frequency *= 2f;
                    }
                    
                    noise /= maxValue;
                    byte noiseValue = (byte)(noise * 255);
                    data[y * width + x] = new Color(noiseValue, noiseValue, noiseValue, 255);
                }
            }
            
            texture.SetData(data);
            return texture;
        }
        
        /// <summary>
        /// Simple hash-based noise function.
        /// </summary>
        private static float HashNoise(float x, float y)
        {
            int xi = (int)MathF.Floor(x);
            int yi = (int)MathF.Floor(y);
            float xf = x - xi;
            float yf = y - yi;
            
            // Smoothstep interpolation
            float u = xf * xf * (3f - 2f * xf);
            float v = yf * yf * (3f - 2f * yf);
            
            // Hash corners
            float n00 = Hash2D(xi, yi);
            float n10 = Hash2D(xi + 1, yi);
            float n01 = Hash2D(xi, yi + 1);
            float n11 = Hash2D(xi + 1, yi + 1);
            
            // Bilinear interpolation
            float nx0 = MathHelper.Lerp(n00, n10, u);
            float nx1 = MathHelper.Lerp(n01, n11, u);
            return MathHelper.Lerp(nx0, nx1, v);
        }
        
        private static float Hash2D(int x, int y)
        {
            int n = x + y * 57;
            n = (n << 13) ^ n;
            return (1.0f - ((n * (n * n * 15731 + 789221) + 1376312589) & 0x7fffffff) / 1073741824.0f) * 0.5f + 0.5f;
        }
        
        private static float EaseInQuad(float t) => t * t;
        
        #endregion
    }
}
