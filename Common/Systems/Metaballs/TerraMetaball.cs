using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems.Metaballs
{
    /// <summary>
    /// A green/Terra Blade themed metaball system for cosmic energy effects.
    /// Particles spawn along swing paths and merge together into smooth blobs.
    /// Uses TWO layer textures for rich, animated cosmic effect.
    /// 
    /// Pattern from Calamity's GalaxyMetaball:
    /// - Particles are drawn as soft circles to a render target
    /// - Layer textures are composited over the shapes
    /// - Edge color provides dark outline
    /// </summary>
    public class TerraMetaball : Metaball
    {
        /// <summary>
        /// Represents a single metaball particle that will merge with others.
        /// </summary>
        public class TerraParticle
        {
            public Vector2 Center;
            public Vector2 Velocity;
            public float Size;
            public float Rotation;
            public float OriginalSize;
            public float Age;

            public TerraParticle(Vector2 center, Vector2 velocity, float size)
            {
                Center = center;
                Velocity = velocity;
                Size = size;
                OriginalSize = size;
                Rotation = velocity.ToRotation();
                Age = 0f;
            }

            public void Update()
            {
                // Move particle
                Center += Velocity;

                // Decelerate (Galaxia uses 0.96f)
                Velocity *= 0.96f;

                // Shrink over time (Galaxia uses 0.91f)
                Size *= 0.93f;

                Age++;
            }

            public float LifeRatio => Math.Max(0f, Size / OriginalSize);
        }

        // Particle storage
        private static List<TerraParticle> particles = new();

        // TWO layer textures for rich effect!
        private static Asset<Texture2D> emeraldNebulaAsset;
        private static Asset<Texture2D> toxicVortexAsset;

        // Colors - Terra Blade green palette
        public static readonly Color TerraGreen = new Color(80, 255, 120);
        public static readonly Color TerraDarkGreen = new Color(30, 140, 60);
        public static readonly Color TerraEmerald = new Color(40, 200, 90);
        public static readonly Color TerraNeon = new Color(120, 255, 160);

        public override bool AnythingToDraw => particles.Count > 0;

        public override IEnumerable<Texture2D> Layers
        {
            get
            {
                // Return both textures for layered effect
                if (emeraldNebulaAsset?.Value != null)
                    yield return emeraldNebulaAsset.Value;
                if (toxicVortexAsset?.Value != null)
                    yield return toxicVortexAsset.Value;
            }
        }

        public override MetaballDrawLayer DrawLayer => MetaballDrawLayer.BeforeProjectiles;

        // Dark green/black edge for the blob outline - like Galaxia's dark blue edge
        public override Color EdgeColor => Color.Lerp(TerraDarkGreen, Color.Black, 0.7f);

        public override void SetStaticDefaults()
        {
            // Load both metaball layer textures
            if (ModContent.HasAsset("MagnumOpus/Assets/VFX/Metaball_EmeraldCosmicNebula"))
            {
                emeraldNebulaAsset = ModContent.Request<Texture2D>(
                    "MagnumOpus/Assets/VFX/Metaball_EmeraldCosmicNebula", 
                    AssetRequestMode.ImmediateLoad);
            }
            
            if (ModContent.HasAsset("MagnumOpus/Assets/VFX/Metaball_ToxicEnergyVortex"))
            {
                toxicVortexAsset = ModContent.Request<Texture2D>(
                    "MagnumOpus/Assets/VFX/Metaball_ToxicEnergyVortex", 
                    AssetRequestMode.ImmediateLoad);
            }
        }

        public override void Update()
        {
            // Update all particles
            for (int i = 0; i < particles.Count; i++)
            {
                particles[i].Update();
            }

            // Remove particles that have shrunk too small
            particles.RemoveAll(p => p.Size <= 4f);
        }

        public override void ClearInstances()
        {
            particles.Clear();
        }

        public override void DrawInstances()
        {
            Texture2D circleTex = MetaballManager.GetCircleTexture();
            if (circleTex == null)
                return;

            Vector2 origin = new Vector2(circleTex.Width, circleTex.Height) * 0.5f;

            foreach (var particle in particles)
            {
                Vector2 drawPos = particle.Center - Main.screenPosition;
                float scale = particle.Size / circleTex.Width;
                float lifeRatio = particle.LifeRatio;

                // Calculate color based on life - brighter when newer
                // Use white for the shape mask, color comes from layers
                Color drawColor = Color.White;
                
                // Fade out as particle dies
                float alpha = MathHelper.Clamp(lifeRatio * 1.5f, 0f, 1f);

                Main.spriteBatch.Draw(circleTex, drawPos, null, drawColor * alpha,
                    0f, origin, scale, SpriteEffects.None, 0f);

                // Draw a brighter, smaller core for more definition
                if (lifeRatio > 0.3f)
                {
                    float coreAlpha = (lifeRatio - 0.3f) / 0.7f;
                    Main.spriteBatch.Draw(circleTex, drawPos, null, Color.White * (coreAlpha * 0.8f),
                        0f, origin, scale * 0.5f, SpriteEffects.None, 0f);
                }
            }
        }

        public override Vector2 CalculateManualOffsetForLayer(int layerIndex)
        {
            // Scroll each layer in different directions for animated effect
            // This makes the cosmic texture appear to swirl
            float time = Main.GlobalTimeWrappedHourly;
            
            if (layerIndex == 0)
            {
                // First layer scrolls diagonally
                return new Vector2(time * 0.08f, time * 0.06f);
            }
            else
            {
                // Second layer scrolls in opposite direction for depth
                return new Vector2(-time * 0.05f, time * 0.09f);
            }
        }

        // ===== PUBLIC API FOR SPAWNING PARTICLES =====

        /// <summary>
        /// Spawns a Terra metaball particle at the given position.
        /// </summary>
        /// <param name="position">World position to spawn at</param>
        /// <param name="velocity">Initial velocity</param>
        /// <param name="size">Size of the particle (larger = more visible, ~40-150 typical)</param>
        public static TerraParticle SpawnParticle(Vector2 position, Vector2 velocity, float size)
        {
            var particle = new TerraParticle(position, velocity, size);
            particles.Add(particle);
            return particle;
        }

        /// <summary>
        /// Spawns multiple particles in a burst pattern.
        /// </summary>
        public static void SpawnBurst(Vector2 center, int count, float baseSize, float speed)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count;
                Vector2 velocity = angle.ToRotationVector2() * speed * Main.rand.NextFloat(0.7f, 1.3f);
                float size = baseSize * Main.rand.NextFloat(0.8f, 1.2f);
                SpawnParticle(center, velocity, size);
            }
        }

        /// <summary>
        /// Spawns particles along a line (for swing trails).
        /// </summary>
        public static void SpawnAlongLine(Vector2 start, Vector2 end, int count, float baseSize, float perpSpeed = 0f)
        {
            Vector2 direction = (end - start).SafeNormalize(Vector2.Zero);
            Vector2 perpendicular = direction.RotatedBy(MathHelper.PiOver2);

            for (int i = 0; i < count; i++)
            {
                float t = count > 1 ? (float)i / (count - 1) : 0.5f;
                Vector2 position = Vector2.Lerp(start, end, t);
                
                // Add some randomness
                position += Main.rand.NextVector2Circular(8f, 8f);
                
                // Velocity perpendicular to line for spreading effect
                Vector2 velocity = perpendicular * perpSpeed * Main.rand.NextFloat(-1f, 1f);
                velocity += Main.rand.NextVector2Circular(3f, 3f);

                float size = baseSize * Main.rand.NextFloat(0.7f, 1.3f);
                SpawnParticle(position, velocity, size);
            }
        }

        /// <summary>
        /// Spawns particles in an arc pattern (for swing effects).
        /// </summary>
        public static void SpawnArc(Vector2 center, float radius, float startAngle, float endAngle, 
            int count, float baseSize, float outwardSpeed = 0f)
        {
            float angleStep = count > 1 ? (endAngle - startAngle) / (count - 1) : 0f;

            for (int i = 0; i < count; i++)
            {
                float angle = startAngle + angleStep * i;
                Vector2 offset = angle.ToRotationVector2() * radius;
                Vector2 position = center + offset + Main.rand.NextVector2Circular(5f, 5f);

                // Velocity goes outward from center
                Vector2 velocity = offset.SafeNormalize(Vector2.Zero) * outwardSpeed * Main.rand.NextFloat(0.5f, 1.5f);
                velocity += Main.rand.NextVector2Circular(2f, 2f);

                float size = baseSize * Main.rand.NextFloat(0.8f, 1.2f);
                SpawnParticle(position, velocity, size);
            }
        }

        /// <summary>
        /// Spawns particles in a cloud around a point (for ambient effects).
        /// </summary>
        public static void SpawnCloud(Vector2 center, int count, float radius, float baseSize, float driftSpeed = 1f)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(radius, radius);
                Vector2 position = center + offset;
                
                Vector2 velocity = Main.rand.NextVector2Circular(driftSpeed, driftSpeed);
                float size = baseSize * Main.rand.NextFloat(0.6f, 1.4f);
                
                SpawnParticle(position, velocity, size);
            }
        }

        /// <summary>
        /// Gets the current particle count (for debugging).
        /// </summary>
        public static int ParticleCount => particles.Count;
    }
}
