using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace MagnumOpus.Common.Systems.VFX.Core
{
    /// <summary>
    /// High-performance pooled particle system.
    /// Uses object pooling to avoid GC allocations for particle-heavy effects.
    /// 
    /// USAGE:
    /// var system = new ParticlePoolSystem(2000);
    /// system.Spawn(position, velocity, settings);
    /// system.SpawnBurst(position, 20, settings);
    /// system.Update();
    /// system.Draw(spriteBatch, texture);
    /// </summary>
    public class ParticlePoolSystem
    {
        #region Pooled Particle Class
        
        /// <summary>
        /// Poolable particle with all common properties.
        /// </summary>
        public class PooledParticle : IPoolable
        {
            public bool IsActive { get; set; }
            
            // Transform
            public Vector2 Position;
            public Vector2 Velocity;
            public Vector2 Acceleration;
            public float Rotation;
            public float RotationSpeed;
            public float Scale;
            public float ScaleVelocity;
            
            // Visual
            public Color Color;
            public Color EndColor;
            public float Alpha;
            public Rectangle? SourceRect;
            public SpriteEffects SpriteEffects;
            
            // Lifecycle
            public int Lifetime;
            public int Age;
            
            // Physics
            public float Drag;
            public bool AffectedByGravity;
            public float GravityScale;
            
            // Animation
            public float FadeInTime;
            public float FadeOutTime;
            public bool UseColorGradient;
            public bool PulseScale;
            public float PulseSpeed;
            public float PulseAmplitude;
            
            // Collision (optional)
            public bool CollidesWithTiles;
            public float Bounciness;
            
            public void OnPoolAcquire()
            {
                IsActive = true;
            }
            
            public void OnPoolRelease()
            {
                Reset();
            }
            
            public void Reset()
            {
                IsActive = false;
                Age = 0;
                Alpha = 1f;
                Scale = 1f;
                ScaleVelocity = 0f;
                Rotation = 0f;
                RotationSpeed = 0f;
                Velocity = Vector2.Zero;
                Acceleration = Vector2.Zero;
                Drag = 0f;
                AffectedByGravity = false;
                GravityScale = 1f;
                FadeInTime = 0f;
                FadeOutTime = 0f;
                UseColorGradient = false;
                PulseScale = false;
                CollidesWithTiles = false;
                SourceRect = null;
                SpriteEffects = SpriteEffects.None;
            }
            
            /// <summary>
            /// Get life progress (0 = just spawned, 1 = about to die).
            /// </summary>
            public float LifeProgress => Lifetime > 0 ? (float)Age / Lifetime : 0f;
            
            /// <summary>
            /// Get current draw color with alpha applied.
            /// </summary>
            public Color DrawColor
            {
                get
                {
                    Color baseColor = UseColorGradient 
                        ? Color.Lerp(Color, EndColor, LifeProgress) 
                        : Color;
                    return baseColor * Alpha;
                }
            }
        }
        
        #endregion
        
        #region Particle Settings
        
        /// <summary>
        /// Configuration for spawning particles.
        /// </summary>
        public class ParticleSettings
        {
            public Color Color = Color.White;
            public Color EndColor = Color.White;
            public bool UseColorGradient = false;
            
            public float MinSpeed = 1f;
            public float MaxSpeed = 5f;
            public float MinLifetime = 30f;
            public float MaxLifetime = 60f;
            public float MinScale = 0.5f;
            public float MaxScale = 1.5f;
            public float ScaleVelocity = 0f; // Scale change per frame
            
            public float MinRotationSpeed = -0.1f;
            public float MaxRotationSpeed = 0.1f;
            public bool RandomRotation = true;
            
            public Vector2 Gravity = Vector2.Zero;
            public float Drag = 0.02f;
            public float FadeInTime = 5f;
            public float FadeOutTime = 10f;
            
            public bool CollideWithTiles = false;
            public float Bounciness = 0.5f;
            
            public bool PulseScale = false;
            public float PulseSpeed = 0.1f;
            public float PulseAmplitude = 0.2f;
            
            #region Presets
            
            public static ParticleSettings Explosion() => new ParticleSettings
            {
                Color = Color.Orange,
                EndColor = new Color(50, 20, 10),
                UseColorGradient = true,
                MinSpeed = 5f,
                MaxSpeed = 15f,
                MinLifetime = 20f,
                MaxLifetime = 40f,
                Gravity = new Vector2(0, 0.3f),
                FadeOutTime = 15f
            };
            
            public static ParticleSettings Sparkle() => new ParticleSettings
            {
                Color = Color.White,
                MinSpeed = 0.5f,
                MaxSpeed = 2f,
                MinLifetime = 30f,
                MaxLifetime = 60f,
                MinScale = 0.2f,
                MaxScale = 0.8f,
                Gravity = Vector2.Zero,
                Drag = 0.05f,
                PulseScale = true,
                PulseSpeed = 0.15f,
                PulseAmplitude = 0.3f
            };
            
            public static ParticleSettings Smoke() => new ParticleSettings
            {
                Color = new Color(100, 100, 100),
                EndColor = new Color(50, 50, 50, 0),
                UseColorGradient = true,
                MinSpeed = 0.5f,
                MaxSpeed = 2f,
                MinLifetime = 60f,
                MaxLifetime = 120f,
                MinScale = 1f,
                MaxScale = 3f,
                ScaleVelocity = 0.02f,
                Gravity = new Vector2(0, -0.1f),
                Drag = 0.03f,
                FadeInTime = 10f,
                FadeOutTime = 30f
            };
            
            public static ParticleSettings Fire() => new ParticleSettings
            {
                Color = new Color(255, 200, 50),
                EndColor = new Color(200, 50, 20),
                UseColorGradient = true,
                MinSpeed = 2f,
                MaxSpeed = 6f,
                MinLifetime = 15f,
                MaxLifetime = 30f,
                MinScale = 0.5f,
                MaxScale = 1.2f,
                Gravity = new Vector2(0, -0.15f),
                Drag = 0.01f,
                FadeOutTime = 10f
            };
            
            public static ParticleSettings MusicNote() => new ParticleSettings
            {
                Color = Color.White,
                MinSpeed = 1f,
                MaxSpeed = 3f,
                MinLifetime = 40f,
                MaxLifetime = 80f,
                MinScale = 0.7f,
                MaxScale = 1.2f,
                MinRotationSpeed = -0.02f,
                MaxRotationSpeed = 0.02f,
                Gravity = new Vector2(0, -0.05f),
                Drag = 0.02f,
                FadeInTime = 5f,
                FadeOutTime = 20f,
                PulseScale = true,
                PulseSpeed = 0.1f,
                PulseAmplitude = 0.15f
            };
            
            public static ParticleSettings Beam() => new ParticleSettings
            {
                Color = Color.Cyan,
                EndColor = Color.White,
                UseColorGradient = true,
                MinSpeed = 0f,
                MaxSpeed = 0.5f,
                MinLifetime = 10f,
                MaxLifetime = 20f,
                MinScale = 0.3f,
                MaxScale = 0.8f,
                Drag = 0.1f,
                FadeOutTime = 8f
            };
            
            #endregion
        }
        
        #endregion
        
        #region Fields
        
        private ObjectPool<PooledParticle> particlePool;
        private List<PooledParticle> activeParticles;
        
        // World gravity (can be modified)
        public Vector2 WorldGravity { get; set; } = new Vector2(0, 0.2f);
        
        // Performance limits
        public int MaxActiveParticles { get; set; } = 2000;
        
        #endregion
        
        #region Statistics
        
        public int ActiveCount => activeParticles.Count;
        public int PoolSize => particlePool.TotalCreated;
        public int Available => particlePool.AvailableCount;
        
        #endregion
        
        #region Constructor
        
        public ParticlePoolSystem(int poolSize = 2000)
        {
            particlePool = new ObjectPool<PooledParticle>(poolSize, poolSize * 2);
            activeParticles = new List<PooledParticle>(poolSize);
            
            // Setup callbacks
            particlePool.OnAcquire = p => p.OnPoolAcquire();
            particlePool.OnRelease = p => p.OnPoolRelease();
        }
        
        #endregion
        
        #region Spawn Methods
        
        /// <summary>
        /// Spawn a single particle.
        /// </summary>
        public PooledParticle Spawn(Vector2 position, Vector2 velocity, ParticleSettings settings)
        {
            if (activeParticles.Count >= MaxActiveParticles)
                return null;
            
            if (!particlePool.TryGet(out var particle))
                return null;
            
            // Initialize from settings
            particle.Position = position;
            particle.Velocity = velocity;
            particle.Acceleration = settings.Gravity;
            particle.Color = settings.Color;
            particle.EndColor = settings.EndColor;
            particle.UseColorGradient = settings.UseColorGradient;
            particle.Scale = Main.rand.NextFloat(settings.MinScale, settings.MaxScale);
            particle.ScaleVelocity = settings.ScaleVelocity;
            particle.Rotation = settings.RandomRotation 
                ? Main.rand.NextFloat(0f, MathHelper.TwoPi) 
                : 0f;
            particle.RotationSpeed = Main.rand.NextFloat(
                settings.MinRotationSpeed, settings.MaxRotationSpeed);
            particle.Lifetime = Main.rand.Next(
                (int)settings.MinLifetime, (int)settings.MaxLifetime + 1);
            particle.Alpha = 1f;
            particle.Age = 0;
            particle.Drag = settings.Drag;
            particle.FadeInTime = settings.FadeInTime;
            particle.FadeOutTime = settings.FadeOutTime;
            particle.CollidesWithTiles = settings.CollideWithTiles;
            particle.Bounciness = settings.Bounciness;
            particle.PulseScale = settings.PulseScale;
            particle.PulseSpeed = settings.PulseSpeed;
            particle.PulseAmplitude = settings.PulseAmplitude;
            particle.AffectedByGravity = settings.Gravity != Vector2.Zero;
            
            activeParticles.Add(particle);
            return particle;
        }
        
        /// <summary>
        /// Spawn a burst of particles in random directions.
        /// </summary>
        public void SpawnBurst(Vector2 position, int count, ParticleSettings settings)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = Main.rand.NextFloat(0f, MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(settings.MinSpeed, settings.MaxSpeed);
                Vector2 velocity = angle.ToRotationVector2() * speed;
                
                Spawn(position, velocity, settings);
            }
        }
        
        /// <summary>
        /// Spawn particles in a cone direction.
        /// </summary>
        public void SpawnCone(Vector2 position, Vector2 direction, float coneAngle, 
                              int count, ParticleSettings settings)
        {
            float baseAngle = direction.ToRotation();
            float halfCone = MathHelper.ToRadians(coneAngle * 0.5f);
            
            for (int i = 0; i < count; i++)
            {
                float angle = baseAngle + Main.rand.NextFloat(-halfCone, halfCone);
                float speed = Main.rand.NextFloat(settings.MinSpeed, settings.MaxSpeed);
                Vector2 velocity = angle.ToRotationVector2() * speed;
                
                Spawn(position, velocity, settings);
            }
        }
        
        /// <summary>
        /// Spawn particles along a line.
        /// </summary>
        public void SpawnLine(Vector2 start, Vector2 end, int count, ParticleSettings settings)
        {
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)(count - 1);
                Vector2 position = Vector2.Lerp(start, end, t);
                
                float angle = Main.rand.NextFloat(0f, MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(settings.MinSpeed, settings.MaxSpeed);
                Vector2 velocity = angle.ToRotationVector2() * speed;
                
                Spawn(position, velocity, settings);
            }
        }
        
        /// <summary>
        /// Spawn particles in a ring pattern.
        /// </summary>
        public void SpawnRing(Vector2 center, float radius, int count, ParticleSettings settings)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = (i / (float)count) * MathHelper.TwoPi;
                Vector2 position = center + angle.ToRotationVector2() * radius;
                Vector2 velocity = angle.ToRotationVector2() * 
                    Main.rand.NextFloat(settings.MinSpeed, settings.MaxSpeed);
                
                Spawn(position, velocity, settings);
            }
        }
        
        #endregion
        
        #region Update
        
        public void Update()
        {
            for (int i = activeParticles.Count - 1; i >= 0; i--)
            {
                var p = activeParticles[i];
                p.Age++;
                
                // Check lifetime
                if (p.Age >= p.Lifetime)
                {
                    activeParticles.RemoveAt(i);
                    particlePool.Return(p);
                    continue;
                }
                
                // Physics
                if (p.AffectedByGravity)
                {
                    p.Velocity += WorldGravity * p.GravityScale;
                }
                p.Velocity += p.Acceleration;
                p.Velocity *= (1f - p.Drag);
                p.Position += p.Velocity;
                p.Rotation += p.RotationSpeed;
                p.Scale += p.ScaleVelocity;
                
                // Scale pulsing
                if (p.PulseScale)
                {
                    float pulse = (float)Math.Sin(p.Age * p.PulseSpeed) * p.PulseAmplitude;
                    // Apply pulse multiplicatively to base scale
                }
                
                // Tile collision
                if (p.CollidesWithTiles)
                {
                    Point tileCoords = p.Position.ToTileCoordinates();
                    if (Terraria.WorldGen.SolidTile(tileCoords.X, tileCoords.Y))
                    {
                        p.Velocity.Y *= -p.Bounciness;
                        p.Velocity.X *= 0.9f; // Friction
                    }
                }
                
                // Alpha animation
                float lifeProgress = p.LifeProgress;
                
                if (p.FadeInTime > 0 && p.Age < p.FadeInTime)
                {
                    p.Alpha = p.Age / p.FadeInTime;
                }
                else if (p.FadeOutTime > 0 && p.Lifetime - p.Age < p.FadeOutTime)
                {
                    p.Alpha = (p.Lifetime - p.Age) / p.FadeOutTime;
                }
                else
                {
                    p.Alpha = 1f;
                }
            }
        }
        
        #endregion
        
        #region Draw
        
        public void Draw(SpriteBatch spriteBatch, Texture2D texture)
        {
            if (texture == null || activeParticles.Count == 0)
                return;
            
            Vector2 origin = new Vector2(texture.Width, texture.Height) * 0.5f;
            
            foreach (var p in activeParticles)
            {
                // Skip off-screen particles
                Vector2 screenPos = p.Position - Main.screenPosition;
                if (screenPos.X < -100 || screenPos.X > Main.screenWidth + 100 ||
                    screenPos.Y < -100 || screenPos.Y > Main.screenHeight + 100)
                    continue;
                
                Color drawColor = p.DrawColor;
                
                // Apply scale pulse
                float drawScale = p.Scale;
                if (p.PulseScale)
                {
                    float pulse = 1f + (float)Math.Sin(p.Age * p.PulseSpeed) * p.PulseAmplitude;
                    drawScale *= pulse;
                }
                
                spriteBatch.Draw(
                    texture,
                    screenPos,
                    p.SourceRect,
                    drawColor,
                    p.Rotation,
                    origin,
                    drawScale,
                    p.SpriteEffects,
                    0f
                );
            }
        }
        
        /// <summary>
        /// Draw particles using additive blending (for glow effects).
        /// </summary>
        public void DrawAdditive(SpriteBatch spriteBatch, Texture2D texture)
        {
            if (texture == null || activeParticles.Count == 0)
                return;
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);
            
            Draw(spriteBatch, texture);
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Clear all active particles.
        /// </summary>
        public void Clear()
        {
            foreach (var p in activeParticles)
            {
                particlePool.Return(p);
            }
            activeParticles.Clear();
        }
        
        /// <summary>
        /// Get pool statistics.
        /// </summary>
        public string GetStats()
        {
            return $"Particles: Active={ActiveCount}, Pool={PoolSize}, Available={Available}";
        }
        
        #endregion
    }
}
