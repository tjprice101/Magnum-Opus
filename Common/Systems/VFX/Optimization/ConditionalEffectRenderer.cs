using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

namespace MagnumOpus.Common.Systems.VFX.Optimization
{
    /// <summary>
    /// Conditional effect renderer that enables/disables effects based on
    /// adaptive quality settings and visibility.
    /// </summary>
    public class ConditionalEffectRenderer
    {
        private AdaptiveQualityManager qualityManager;
        private LODManager lodManager;

        // Effect toggles (can be overridden manually)
        public bool ForceGlowEnabled { get; set; } = false;
        public bool ForceGlowDisabled { get; set; } = false;
        public bool ForceParticlesEnabled { get; set; } = false;
        public bool ForceParticlesDisabled { get; set; } = false;
        public bool ForceBloomEnabled { get; set; } = false;
        public bool ForceBloomDisabled { get; set; } = false;

        public ConditionalEffectRenderer()
        {
            qualityManager = AdaptiveQualityManager.Instance;
            lodManager = LODManager.Instance;
        }

        /// <summary>
        /// Check if glow effects should be rendered.
        /// </summary>
        public bool ShouldRenderGlow(Vector2 worldPosition)
        {
            if (ForceGlowDisabled) return false;
            if (ForceGlowEnabled) return true;

            // Check quality
            if (qualityManager != null && !qualityManager.EnableGlow)
                return false;

            // Check LOD
            var lod = lodManager.GetLODLevel(worldPosition);
            return lodManager.ShouldEnableGlow(lod);
        }

        /// <summary>
        /// Check if particles should be spawned.
        /// </summary>
        public bool ShouldSpawnParticles(Vector2 worldPosition)
        {
            if (ForceParticlesDisabled) return false;
            if (ForceParticlesEnabled) return true;

            if (qualityManager != null && !qualityManager.EnableParticles)
                return false;

            var lod = lodManager.GetLODLevel(worldPosition);
            return lodManager.GetParticleCount(lod, 10) > 0;
        }

        /// <summary>
        /// Check if bloom should be applied.
        /// </summary>
        public bool ShouldApplyBloom()
        {
            if (ForceBloomDisabled) return false;
            if (ForceBloomEnabled) return true;

            return qualityManager != null && qualityManager.EnableBloom;
        }

        /// <summary>
        /// Get the number of bloom layers to use.
        /// </summary>
        public int GetBloomLayers(Vector2 worldPosition)
        {
            if (!ShouldApplyBloom())
                return 0;

            var lod = lodManager.GetLODLevel(worldPosition);
            int baseLayers = qualityManager?.BloomLayers ?? 4;

            // Reduce layers for distant objects
            return lod switch
            {
                LODManager.LODLevel.High => baseLayers,
                LODManager.LODLevel.Medium => Math.Max(1, baseLayers - 1),
                LODManager.LODLevel.Low => 1,
                _ => 0
            };
        }

        /// <summary>
        /// Get the number of particles to spawn based on quality and LOD.
        /// </summary>
        public int GetParticleCount(Vector2 worldPosition, int baseCount)
        {
            if (!ShouldSpawnParticles(worldPosition))
                return 0;

            var lod = lodManager.GetLODLevel(worldPosition);
            float lodMult = lodManager.GetQualityMultiplier(lod);
            float qualityMult = qualityManager?.ParticleQuality ?? 1f;

            return (int)(baseCount * lodMult * qualityMult);
        }

        /// <summary>
        /// Draw a beam with conditional effects based on quality and LOD.
        /// </summary>
        public void DrawBeamConditional(SpriteBatch spriteBatch, Vector2 start, Vector2 end,
            float width, Color color, Texture2D texture, Action<SpriteBatch> drawCore,
            Action<SpriteBatch, int> drawGlow = null, Action<Vector2, int> spawnParticles = null)
        {
            Vector2 midpoint = (start + end) * 0.5f;

            // Always draw core
            drawCore?.Invoke(spriteBatch);

            // Conditional glow
            if (ShouldRenderGlow(midpoint) && drawGlow != null)
            {
                int layers = GetBloomLayers(midpoint);
                if (layers > 0)
                {
                    drawGlow(spriteBatch, layers);
                }
            }

            // Conditional particles
            if (ShouldSpawnParticles(start) && spawnParticles != null)
            {
                int count = GetParticleCount(start, 10);
                if (count > 0)
                {
                    spawnParticles(start, count);
                }
            }
        }

        /// <summary>
        /// Apply post-processing conditionally.
        /// </summary>
        public void ApplyPostProcessingConditional(SpriteBatch spriteBatch,
            Action drawScene, Action<SpriteBatch> applyBloom)
        {
            if (ShouldApplyBloom() && applyBloom != null)
            {
                applyBloom(spriteBatch);
            }
            else
            {
                // Skip post-processing, draw directly
                drawScene?.Invoke();
            }
        }

        /// <summary>
        /// Get a quality-adjusted scale for effects.
        /// </summary>
        public float GetAdjustedScale(Vector2 worldPosition, float baseScale)
        {
            var lod = lodManager.GetLODLevel(worldPosition);
            float lodMult = lodManager.GetQualityMultiplier(lod);
            float qualityMult = qualityManager?.GetQualityMultiplier() ?? 1f;

            return baseScale * MathHelper.Lerp(0.5f, 1f, lodMult * qualityMult);
        }

        /// <summary>
        /// Get a quality-adjusted opacity for effects.
        /// </summary>
        public float GetAdjustedOpacity(Vector2 worldPosition, float baseOpacity)
        {
            var lod = lodManager.GetLODLevel(worldPosition);
            float lodMult = lodManager.GetQualityMultiplier(lod);

            return baseOpacity * lodMult;
        }
    }
}
