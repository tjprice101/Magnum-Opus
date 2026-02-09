using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;

namespace MagnumOpus.Common.Systems.Particles
{
    /// <summary>
    /// Central handler for all custom particles in MagnumOpus.
    /// Manages particle lifecycle, updates, and batched rendering for optimal performance.
    /// Inspired by Calamity Mod's particle system.
    /// </summary>
    public static class MagnumParticleHandler
    {
        private static List<Particle> particles;
        private static List<Particle> particlesToKill;
        internal static Dictionary<Type, int> particleTypes;
        internal static Dictionary<int, Texture2D> particleTextures;
        private static List<Particle> particleInstances;

        // Batched particle lists for efficient rendering
        private static List<Particle> batchedAlphaBlendParticles;
        private static List<Particle> batchedNonPremultipliedParticles;
        private static List<Particle> batchedAdditiveBlendParticles;
        
        // High-performance particle pools for each blend mode
        private static Queue<Particle> particlePool;
        private const int PoolInitialSize = 500;
        private const int PoolMaxSize = 2000;

        /// <summary>
        /// Maximum number of particles that can exist at once.
        /// Calamity uses up to 10,000 particles - we match this for dense visual effects.
        /// </summary>
        public const int MaxParticles = 10000;
        
        /// <summary>
        /// Gets the current number of active particles. Used by BossVFXOptimizer for quality scaling.
        /// </summary>
        public static int ActiveParticleCount => particles?.Count ?? 0;
        
        /// <summary>
        /// Gets the number of particles in the pool for reuse.
        /// </summary>
        public static int PooledParticleCount => particlePool?.Count ?? 0;
        
        /// <summary>
        /// Gets the particle capacity utilization (0-1).
        /// </summary>
        public static float CapacityUtilization => (float)ActiveParticleCount / MaxParticles;
        
        /// <summary>
        /// Distance threshold for culling particles outside the visible screen area.
        /// Particles beyond this distance from the screen center won't render but still update.
        /// </summary>
        private const float CullDistance = 3000f;
        
        /// <summary>
        /// Whether to use aggressive culling when particle count is high.
        /// </summary>
        private static bool useAggressiveCulling = false;

        internal static void Load()
        {
            // Pre-allocate lists with generous initial capacity for performance
            particles = new List<Particle>(MaxParticles / 2);
            particlesToKill = new List<Particle>(256);
            particleTypes = new Dictionary<Type, int>();
            particleTextures = new Dictionary<int, Texture2D>();
            particleInstances = new List<Particle>();

            // Batched rendering lists - pre-allocate for typical usage
            batchedAlphaBlendParticles = new List<Particle>(1000);
            batchedNonPremultipliedParticles = new List<Particle>(500);
            batchedAdditiveBlendParticles = new List<Particle>(3000);
            
            // Initialize particle pool for object reuse (reduces GC pressure)
            particlePool = new Queue<Particle>(PoolInitialSize);
        }

        internal static void Unload()
        {
            particles = null;
            particlesToKill = null;
            particleTypes = null;
            particleTextures = null;
            particleInstances = null;
            batchedAlphaBlendParticles = null;
            batchedNonPremultipliedParticles = null;
            batchedAdditiveBlendParticles = null;
            particlePool = null;
        }

        /// <summary>
        /// Loads all particle types from the mod assembly.
        /// </summary>
        public static void LoadModParticleInstances()
        {
            if (particleInstances == null)
                particleInstances = new List<Particle>();

            Type baseType = typeof(Particle);
            var mod = ModContent.GetInstance<MagnumOpus>();

            foreach (Type type in AssemblyManager.GetLoadableTypes(mod.Code))
            {
                if (type.IsSubclassOf(baseType) && !type.IsAbstract && type != baseType)
                {
                    // Check if type has a parameterless constructor
                    var parameterlessConstructor = type.GetConstructor(Type.EmptyTypes);
                    if (parameterlessConstructor == null)
                    {
                        // No parameterless constructor - just register the type ID
                        // The type will still work when spawned via SpawnParticle with proper constructor
                        int typeId = particleTypes.Count;
                        particleTypes[type] = typeId;
                        particleInstances.Add(null); // Placeholder to keep indices aligned
                        continue;
                    }

                    int id = particleTypes.Count;
                    particleTypes[type] = id;

                    try
                    {
                        Particle instance = (Particle)Activator.CreateInstance(type, true);
                        particleInstances.Add(instance);

                        if (!string.IsNullOrEmpty(instance.Texture))
                        {
                            // Use ParticleTextureHelper to get generated textures or load from assets
                            Texture2D tex = ParticleTextureHelper.GetTexture(instance.Texture);
                            if (tex != null)
                                particleTextures[id] = tex;
                        }
                    }
                    catch
                    {
                        // If instantiation fails for any reason, still register the type
                        particleInstances.Add(null);
                    }
                }
            }
        }

        /// <summary>
        /// Spawns a particle into the world.
        /// If the particle limit is reached but the particle is marked as Important,
        /// it will try to replace a non-important particle.
        /// </summary>
        public static void SpawnParticle(Particle particle)
        {
            if (Main.dedServ)
                return;

            if (particles == null)
                Load();

            // Register particle type if not already done
            Type particleType = particle.GetType();
            if (!particleTypes.ContainsKey(particleType))
            {
                int id = particleTypes.Count;
                particleTypes[particleType] = id;

                if (!string.IsNullOrEmpty(particle.Texture))
                {
                    // Use ParticleTextureHelper to get generated textures or load from assets
                    Texture2D tex = ParticleTextureHelper.GetTexture(particle.Texture);
                    if (tex != null)
                        particleTextures[id] = tex;
                }
            }

            particle.Type = particleTypes[particleType];

            if (particles.Count >= MaxParticles)
            {
                if (particle.Important)
                {
                    // Try to remove a non-important particle
                    Particle toRemove = particles.FirstOrDefault(p => !p.Important);
                    if (toRemove != null)
                    {
                        particles.Remove(toRemove);
                    }
                    else
                    {
                        return; // All particles are important, can't spawn more
                    }
                }
                else
                {
                    return; // Particle limit reached
                }
            }

            particle.ID = particles.Count;
            particles.Add(particle);
        }

        /// <summary>
        /// Updates all active particles.
        /// Uses aggressive culling when particle count exceeds 70% capacity.
        /// </summary>
        public static void Update()
        {
            if (Main.dedServ || particles == null)
                return;

            // Enable aggressive culling when near capacity
            useAggressiveCulling = particles.Count > MaxParticles * 0.7f;
            
            // If severely overloaded, cull oldest non-important particles
            if (particles.Count > MaxParticles * 0.9f)
            {
                int cullTarget = particles.Count - (int)(MaxParticles * 0.8f);
                int culled = 0;
                for (int i = particles.Count - 1; i >= 0 && culled < cullTarget; i--)
                {
                    if (!particles[i].Important && particles[i].Time > 5)
                    {
                        particlesToKill.Add(particles[i]);
                        culled++;
                    }
                }
            }

            foreach (Particle particle in particles)
            {
                if (particle == null)
                    continue;

                particle.Update();
                particle.Position += particle.Velocity;
                particle.Time++;

                if (particle.SetLifetime && particle.Time >= particle.Lifetime)
                {
                    particlesToKill.Add(particle);
                }
                
                // Aggressive culling: kill off-screen particles faster if overloaded
                if (useAggressiveCulling && !particle.Important)
                {
                    Vector2 screenCenter = Main.screenPosition + new Vector2(Main.screenWidth, Main.screenHeight) * 0.5f;
                    float distSq = Vector2.DistanceSquared(particle.Position, screenCenter);
                    if (distSq > CullDistance * CullDistance * 0.5f) // Tighter culling
                    {
                        particlesToKill.Add(particle);
                    }
                }
            }

            // Remove dead particles and return to pool
            foreach (Particle particle in particlesToKill)
            {
                particles.Remove(particle);
                ReturnToPool(particle);
            }
            particlesToKill.Clear();
        }

        /// <summary>
        /// Removes a specific particle from the handler.
        /// </summary>
        public static void RemoveParticle(Particle particle)
        {
            if (particles != null && !particlesToKill.Contains(particle))
            {
                particlesToKill.Add(particle);
            }
        }

        /// <summary>
        /// Draws all particles as a standalone operation - manages its own spritebatch state.
        /// Use this when calling from an IL hook where the spritebatch state is unknown.
        /// </summary>
        public static void DrawAllParticlesStandalone(SpriteBatch sb)
        {
            if (Main.dedServ || particles == null || particles.Count == 0)
                return;

            // Calculate screen bounds for culling
            Vector2 screenCenter = Main.screenPosition + new Vector2(Main.screenWidth, Main.screenHeight) * 0.5f;
            float cullDistSq = CullDistance * CullDistance;

            // Batch particles by blend mode, culling off-screen particles
            foreach (Particle particle in particles)
            {
                if (particle == null)
                    continue;
                
                // Cull particles far outside the screen for performance
                float distSq = Vector2.DistanceSquared(particle.Position, screenCenter);
                if (distSq > cullDistSq && !particle.Important)
                    continue;

                if (particle.UseAdditiveBlend)
                    batchedAdditiveBlendParticles.Add(particle);
                else if (particle.UseHalfTransparency)
                    batchedNonPremultipliedParticles.Add(particle);
                else
                    batchedAlphaBlendParticles.Add(particle);
            }

            // Draw alpha blend particles
            if (batchedAlphaBlendParticles.Count > 0)
            {
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                foreach (Particle particle in batchedAlphaBlendParticles)
                {
                    DrawParticle(sb, particle);
                }
                sb.End();
            }

            // Draw non-premultiplied (half transparency) particles
            if (batchedNonPremultipliedParticles.Count > 0)
            {
                sb.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, Main.DefaultSamplerState,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                foreach (Particle particle in batchedNonPremultipliedParticles)
                {
                    DrawParticle(sb, particle);
                }
                sb.End();
            }

            // Draw additive blend particles (most important for glows!)
            if (batchedAdditiveBlendParticles.Count > 0)
            {
                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                foreach (Particle particle in batchedAdditiveBlendParticles)
                {
                    DrawParticle(sb, particle);
                }
                sb.End();
            }

            // Clear batches
            batchedAlphaBlendParticles.Clear();
            batchedNonPremultipliedParticles.Clear();
            batchedAdditiveBlendParticles.Clear();
        }

        /// <summary>
        /// Draws all active particles with proper batching for performance.
        /// Particles outside the visible screen area are culled for optimization.
        /// </summary>
        public static void DrawAllParticles(SpriteBatch sb)
        {
            if (Main.dedServ || particles == null || particles.Count == 0)
                return;

            sb.End();

            var rasterizer = Main.Rasterizer;
            rasterizer.ScissorTestEnable = true;
            Main.instance.GraphicsDevice.RasterizerState.ScissorTestEnable = true;
            Main.instance.GraphicsDevice.ScissorRectangle = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);

            // Calculate screen bounds for culling
            Vector2 screenCenter = Main.screenPosition + new Vector2(Main.screenWidth, Main.screenHeight) * 0.5f;
            float cullDistSq = CullDistance * CullDistance;

            // Batch particles by blend mode, culling off-screen particles
            foreach (Particle particle in particles)
            {
                if (particle == null)
                    continue;
                
                // Cull particles far outside the screen for performance
                float distSq = Vector2.DistanceSquared(particle.Position, screenCenter);
                if (distSq > cullDistSq && !particle.Important)
                    continue;

                if (particle.UseAdditiveBlend)
                    batchedAdditiveBlendParticles.Add(particle);
                else if (particle.UseHalfTransparency)
                    batchedNonPremultipliedParticles.Add(particle);
                else
                    batchedAlphaBlendParticles.Add(particle);
            }

            // Draw alpha blend particles
            if (batchedAlphaBlendParticles.Count > 0)
            {
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

                foreach (Particle particle in batchedAlphaBlendParticles)
                {
                    DrawParticle(sb, particle);
                }
                sb.End();
            }

            // Draw non-premultiplied (half transparency) particles
            if (batchedNonPremultipliedParticles.Count > 0)
            {
                sb.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, Main.DefaultSamplerState,
                    DepthStencilState.None, rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

                foreach (Particle particle in batchedNonPremultipliedParticles)
                {
                    DrawParticle(sb, particle);
                }
                sb.End();
            }

            // Draw additive blend particles
            if (batchedAdditiveBlendParticles.Count > 0)
            {
                rasterizer = RasterizerState.CullNone;
                rasterizer.ScissorTestEnable = true;
                Main.instance.GraphicsDevice.RasterizerState.ScissorTestEnable = true;
                Main.instance.GraphicsDevice.ScissorRectangle = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);

                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp,
                    DepthStencilState.Default, rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

                foreach (Particle particle in batchedAdditiveBlendParticles)
                {
                    DrawParticle(sb, particle);
                }
                sb.End();
            }

            // Clear batches
            batchedAlphaBlendParticles.Clear();
            batchedNonPremultipliedParticles.Clear();
            batchedAdditiveBlendParticles.Clear();

            // Restart the spritebatch in default state
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
        }

        private static void DrawParticle(SpriteBatch sb, Particle particle)
        {
            if (particle.UseCustomDraw)
            {
                particle.CustomDraw(sb);
            }
            else if (particleTextures.TryGetValue(particle.Type, out Texture2D texture))
            {
                Rectangle frame = texture.Frame(1, particle.FrameVariants, 0, particle.Variant);
                Vector2 origin = frame.Size() * 0.5f;
                Vector2 drawPos = particle.Position - Main.screenPosition;
                
                // FARGOS PATTERN: For additive blending, use { A = 0 } to remove alpha channel
                // This creates proper glow effects without darkening
                Color drawColor = particle.UseAdditiveBlend 
                    ? particle.Color with { A = 0 }  // Key Fargos pattern!
                    : particle.Color;
                
                sb.Draw(texture, drawPos, frame, drawColor, particle.Rotation, origin, particle.Scale, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Returns the number of free particle slots available.
        /// </summary>
        public static int FreeSpacesAvailable()
        {
            if (particles == null)
                return MaxParticles;
            return MaxParticles - particles.Count;
        }

        /// <summary>
        /// Clears all active particles.
        /// </summary>
        public static void ClearAllParticles()
        {
            particles?.Clear();
        }
        
        /// <summary>
        /// Clears particles and returns them to the pool for reuse.
        /// </summary>
        public static void ClearAllParticlesToPool()
        {
            if (particles == null) return;
            
            foreach (var particle in particles)
            {
                ReturnToPool(particle);
            }
            particles.Clear();
        }
        
        /// <summary>
        /// Returns a particle to the pool for reuse.
        /// </summary>
        private static void ReturnToPool(Particle particle)
        {
            if (particlePool == null || particlePool.Count >= PoolMaxSize)
                return;
                
            // Reset particle for reuse
            particle.Reset();
            particlePool.Enqueue(particle);
        }
        
        /// <summary>
        /// Gets a particle from the pool if available.
        /// </summary>
        /// <typeparam name="T">The particle type to get.</typeparam>
        /// <returns>A pooled particle or null if none available of that type.</returns>
        public static T GetPooledParticle<T>() where T : Particle
        {
            if (particlePool == null || particlePool.Count == 0)
                return null;
                
            // Try to find a matching type in the pool
            int count = particlePool.Count;
            for (int i = 0; i < count; i++)
            {
                var particle = particlePool.Dequeue();
                if (particle is T typedParticle)
                    return typedParticle;
                    
                // Put it back if not the right type
                particlePool.Enqueue(particle);
            }
            
            return null;
        }
        
        /// <summary>
        /// Pre-warms the particle pool with instances of common particle types.
        /// Call this during loading to reduce runtime allocations.
        /// </summary>
        public static void PreWarmPool<T>(int count) where T : Particle, new()
        {
            if (particlePool == null) return;
            
            for (int i = 0; i < count && particlePool.Count < PoolMaxSize; i++)
            {
                particlePool.Enqueue(new T());
            }
        }
        
        /// <summary>
        /// Gets statistics about the particle system for debugging/profiling.
        /// </summary>
        public static (int active, int pooled, float utilization, bool aggressiveCulling) GetStats()
        {
            return (ActiveParticleCount, PooledParticleCount, CapacityUtilization, useAggressiveCulling);
        }

        /// <summary>
        /// Gets the texture for a particle type.
        /// </summary>
        public static Texture2D GetTexture(int type)
        {
            if (particleTextures != null && particleTextures.TryGetValue(type, out Texture2D texture))
                return texture;
            return null;
        }
    }
}
