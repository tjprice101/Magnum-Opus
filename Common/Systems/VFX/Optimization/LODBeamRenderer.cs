using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;

namespace MagnumOpus.Common.Systems.VFX.Optimization
{
    /// <summary>
    /// LOD-aware beam renderer that adjusts detail based on distance from screen center.
    /// Reduces segment count, particles, and effects for distant beams.
    /// </summary>
    public class LODBeamRenderer
    {
        private LODManager lodManager;

        /// <summary>
        /// Per-LOD rendering configuration.
        /// </summary>
        public struct BeamLODConfig
        {
            public int SegmentCount;      // Geometric detail
            public int ParticleCount;     // Muzzle/impact particles
            public bool EnableGlow;       // Glow layers
            public bool EnableNoise;      // Noise distortion
            public int UpdateFrequency;   // Frames between updates
            public float ShaderQuality;   // 0-1, affects shader complexity
            public int BloomLayers;       // Number of bloom passes

            public static BeamLODConfig High => new BeamLODConfig
            {
                SegmentCount = 50,
                ParticleCount = 30,
                EnableGlow = true,
                EnableNoise = true,
                UpdateFrequency = 1,
                ShaderQuality = 1.0f,
                BloomLayers = 4
            };

            public static BeamLODConfig Medium => new BeamLODConfig
            {
                SegmentCount = 25,
                ParticleCount = 15,
                EnableGlow = true,
                EnableNoise = false,
                UpdateFrequency = 2,
                ShaderQuality = 0.7f,
                BloomLayers = 2
            };

            public static BeamLODConfig Low => new BeamLODConfig
            {
                SegmentCount = 10,
                ParticleCount = 5,
                EnableGlow = false,
                EnableNoise = false,
                UpdateFrequency = 3,
                ShaderQuality = 0.4f,
                BloomLayers = 1
            };

            public static BeamLODConfig VeryLow => new BeamLODConfig
            {
                SegmentCount = 4,
                ParticleCount = 0,
                EnableGlow = false,
                EnableNoise = false,
                UpdateFrequency = 5,
                ShaderQuality = 0.2f,
                BloomLayers = 0
            };
        }

        private Dictionary<LODManager.LODLevel, BeamLODConfig> lodConfigs;

        public LODBeamRenderer()
        {
            lodManager = LODManager.Instance;

            lodConfigs = new Dictionary<LODManager.LODLevel, BeamLODConfig>
            {
                { LODManager.LODLevel.High, BeamLODConfig.High },
                { LODManager.LODLevel.Medium, BeamLODConfig.Medium },
                { LODManager.LODLevel.Low, BeamLODConfig.Low },
                { LODManager.LODLevel.VeryLow, BeamLODConfig.VeryLow }
            };
        }

        /// <summary>
        /// Draw a beam with automatic LOD selection based on position.
        /// </summary>
        public void DrawBeam(SpriteBatch spriteBatch, Vector2 start, Vector2 end, 
            float width, Color color, Texture2D texture)
        {
            // Use midpoint for LOD calculation
            Vector2 midpoint = (start + end) * 0.5f;
            LODManager.LODLevel lod = lodManager.GetLODLevel(midpoint);

            if (lod == LODManager.LODLevel.Culled)
                return; // Don't render at all

            BeamLODConfig config = lodConfigs[lod];
            DrawBeamWithLOD(spriteBatch, start, end, width, color, texture, config);
        }

        /// <summary>
        /// Draw a beam with specific LOD configuration.
        /// </summary>
        public void DrawBeamWithLOD(SpriteBatch spriteBatch, Vector2 start, Vector2 end,
            float width, Color color, Texture2D texture, BeamLODConfig config)
        {
            // Generate beam points with LOD-appropriate segment count
            List<Vector2> points = GenerateBeamPoints(start, end, config.SegmentCount);

            // Draw core beam (always)
            DrawBeamCore(spriteBatch, points, width, color, texture);

            // Conditional glow based on LOD
            if (config.EnableGlow && config.BloomLayers > 0)
            {
                DrawBeamGlow(spriteBatch, points, width, color, texture, config.BloomLayers);
            }

            // Muzzle particles
            if (config.ParticleCount > 0)
            {
                SpawnMuzzleParticles(start, color, config.ParticleCount);
            }
        }

        /// <summary>
        /// Get the current LOD configuration for a world position.
        /// </summary>
        public BeamLODConfig GetLODConfig(Vector2 worldPosition)
        {
            LODManager.LODLevel lod = lodManager.GetLODLevel(worldPosition);
            
            if (lod == LODManager.LODLevel.Culled)
                return BeamLODConfig.VeryLow;
                
            return lodConfigs[lod];
        }

        /// <summary>
        /// Check if a beam should be rendered at all based on position.
        /// </summary>
        public bool ShouldRender(Vector2 worldPosition)
        {
            return lodManager.GetLODLevel(worldPosition) != LODManager.LODLevel.Culled;
        }

        private List<Vector2> GenerateBeamPoints(Vector2 start, Vector2 end, int segments)
        {
            List<Vector2> points = new List<Vector2>(segments + 1);

            for (int i = 0; i <= segments; i++)
            {
                float t = i / (float)segments;
                points.Add(Vector2.Lerp(start, end, t));
            }

            return points;
        }

        private void DrawBeamCore(SpriteBatch spriteBatch, List<Vector2> points, 
            float width, Color color, Texture2D texture)
        {
            if (points.Count < 2 || texture == null)
                return;

            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector2 start = points[i];
                Vector2 end = points[i + 1];
                Vector2 direction = end - start;
                float length = direction.Length();
                float rotation = direction.ToRotation();

                float progress = i / (float)(points.Count - 1);
                float segmentWidth = width * (1f - progress * 0.3f); // Taper

                spriteBatch.Draw(
                    texture,
                    start - Main.screenPosition,
                    null,
                    color,
                    rotation,
                    new Vector2(0, texture.Height * 0.5f),
                    new Vector2(length / texture.Width, segmentWidth / texture.Height),
                    SpriteEffects.None,
                    0f
                );
            }
        }

        private void DrawBeamGlow(SpriteBatch spriteBatch, List<Vector2> points,
            float width, Color color, Texture2D texture, int layers)
        {
            Color glowColor = color with { A = 0 };

            for (int layer = 0; layer < layers; layer++)
            {
                float layerScale = 1f + (layer + 1) * 0.3f;
                float layerOpacity = 0.3f / (layer + 1);

                for (int i = 0; i < points.Count - 1; i++)
                {
                    Vector2 start = points[i];
                    Vector2 end = points[i + 1];
                    Vector2 direction = end - start;
                    float length = direction.Length();
                    float rotation = direction.ToRotation();

                    float progress = i / (float)(points.Count - 1);
                    float segmentWidth = width * layerScale * (1f - progress * 0.3f);

                    spriteBatch.Draw(
                        texture,
                        start - Main.screenPosition,
                        null,
                        glowColor * layerOpacity,
                        rotation,
                        new Vector2(0, texture.Height * 0.5f),
                        new Vector2(length / texture.Width, segmentWidth / texture.Height),
                        SpriteEffects.None,
                        0f
                    );
                }
            }
        }

        private void SpawnMuzzleParticles(Vector2 position, Color color, int count)
        {
            // Spawn dust particles at muzzle
            for (int i = 0; i < count; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(3f, 3f);
                Dust dust = Dust.NewDustPerfect(position, Terraria.ID.DustID.MagicMirror, velocity, 0, color, 0.8f);
                dust.noGravity = true;
                dust.fadeIn = 0.5f;
            }
        }

        /// <summary>
        /// Set custom LOD configuration for a specific level.
        /// </summary>
        public void SetLODConfig(LODManager.LODLevel level, BeamLODConfig config)
        {
            lodConfigs[level] = config;
        }
    }
}
