using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
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

        /// <summary>
        /// Maximum number of particles that can exist at once.
        /// Increased from 500 to 3000 to support dense visual effects during boss fights.
        /// </summary>
        public const int MaxParticles = 3000;
        
        /// <summary>
        /// Distance threshold for culling particles outside the visible screen area.
        /// Particles beyond this distance from the screen center won't render but still update.
        /// </summary>
        private const float CullDistance = 2500f;

        internal static void Load()
        {
            particles = new List<Particle>();
            particlesToKill = new List<Particle>();
            particleTypes = new Dictionary<Type, int>();
            particleTextures = new Dictionary<int, Texture2D>();
            particleInstances = new List<Particle>();

            batchedAlphaBlendParticles = new List<Particle>();
            batchedNonPremultipliedParticles = new List<Particle>();
            batchedAdditiveBlendParticles = new List<Particle>();
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
                    int id = particleTypes.Count;
                    particleTypes[type] = id;

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
        /// </summary>
        public static void Update()
        {
            if (Main.dedServ || particles == null)
                return;

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
            }

            // Remove dead particles
            foreach (Particle particle in particlesToKill)
            {
                particles.Remove(particle);
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
                sb.Draw(texture, particle.Position - Main.screenPosition, frame,
                    particle.Color, particle.Rotation, frame.Size() * 0.5f, particle.Scale, SpriteEffects.None, 0f);
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
