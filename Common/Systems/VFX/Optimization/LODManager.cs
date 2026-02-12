using Microsoft.Xna.Framework;
using Terraria;

namespace MagnumOpus.Common.Systems.VFX.Optimization
{
    /// <summary>
    /// Level of Detail manager for distance-based rendering optimization.
    /// Reduces complexity for distant objects for massive performance gains.
    /// 
    /// LOD Distance Thresholds:
    /// - LOD 0 (High):     0-400 pixels   - Full detail
    /// - LOD 1 (Medium):   400-800 pixels - Reduced detail
    /// - LOD 2 (Low):      800-1200 pixels - Minimal detail
    /// - LOD 3 (VeryLow):  1200-1600 pixels - Very minimal
    /// - LOD 4 (Culled):   1600+ pixels   - Don't render
    /// </summary>
    public class LODManager
    {
        public enum LODLevel
        {
            High = 0,
            Medium = 1,
            Low = 2,
            VeryLow = 3,
            Culled = 4
        }

        /// <summary>
        /// LOD configuration settings for distance thresholds.
        /// </summary>
        public struct LODSettings
        {
            public float HighDetailDistance;    // 0-400
            public float MediumDetailDistance;  // 400-800
            public float LowDetailDistance;     // 800-1200
            public float CullDistance;          // 1200+

            public static LODSettings Default => new LODSettings
            {
                HighDetailDistance = 400f,
                MediumDetailDistance = 800f,
                LowDetailDistance = 1200f,
                CullDistance = 1600f
            };

            public static LODSettings Aggressive => new LODSettings
            {
                HighDetailDistance = 300f,
                MediumDetailDistance = 600f,
                LowDetailDistance = 900f,
                CullDistance = 1200f
            };

            public static LODSettings Relaxed => new LODSettings
            {
                HighDetailDistance = 600f,
                MediumDetailDistance = 1000f,
                LowDetailDistance = 1400f,
                CullDistance = 2000f
            };
        }

        public LODSettings Settings { get; set; }

        private static LODManager _instance;
        public static LODManager Instance => _instance ??= new LODManager();

        public LODManager()
        {
            Settings = LODSettings.Default;
        }

        /// <summary>
        /// Calculate LOD level based on distance from screen center.
        /// Objects near the center of the screen get highest detail.
        /// </summary>
        public LODLevel GetLODLevel(Vector2 worldPosition)
        {
            Vector2 screenCenter = Main.screenPosition + new Vector2(
                Main.screenWidth * 0.5f,
                Main.screenHeight * 0.5f
            );

            float distance = Vector2.Distance(worldPosition, screenCenter);

            if (distance < Settings.HighDetailDistance)
                return LODLevel.High;
            else if (distance < Settings.MediumDetailDistance)
                return LODLevel.Medium;
            else if (distance < Settings.LowDetailDistance)
                return LODLevel.Low;
            else if (distance < Settings.CullDistance)
                return LODLevel.VeryLow;
            else
                return LODLevel.Culled;
        }

        /// <summary>
        /// Calculate LOD level based on distance from a specific camera position.
        /// </summary>
        public LODLevel GetLODLevelFromCamera(Vector2 worldPosition, Vector2 cameraPosition)
        {
            float distance = Vector2.Distance(worldPosition, cameraPosition);

            if (distance < Settings.HighDetailDistance)
                return LODLevel.High;
            else if (distance < Settings.MediumDetailDistance)
                return LODLevel.Medium;
            else if (distance < Settings.LowDetailDistance)
                return LODLevel.Low;
            else if (distance < Settings.CullDistance)
                return LODLevel.VeryLow;
            else
                return LODLevel.Culled;
        }

        /// <summary>
        /// Get smooth LOD blend factor for gradual quality transitions.
        /// Returns 0-1 where 0 = highest quality, 1 = lowest/culled.
        /// </summary>
        public float GetLODBlendFactor(Vector2 worldPosition)
        {
            Vector2 screenCenter = Main.screenPosition + new Vector2(
                Main.screenWidth * 0.5f,
                Main.screenHeight * 0.5f
            );

            float distance = Vector2.Distance(worldPosition, screenCenter);
            float normalized = distance / Settings.CullDistance;
            return MathHelper.Clamp(normalized, 0f, 1f);
        }

        /// <summary>
        /// Get LOD level as a 0-1 quality multiplier.
        /// High = 1.0, Medium = 0.75, Low = 0.5, VeryLow = 0.25, Culled = 0
        /// </summary>
        public float GetQualityMultiplier(LODLevel level)
        {
            return level switch
            {
                LODLevel.High => 1.0f,
                LODLevel.Medium => 0.75f,
                LODLevel.Low => 0.5f,
                LODLevel.VeryLow => 0.25f,
                LODLevel.Culled => 0f,
                _ => 1.0f
            };
        }

        /// <summary>
        /// Get recommended update frequency based on LOD level.
        /// Higher values = less frequent updates.
        /// </summary>
        public int GetUpdateFrequency(LODLevel level)
        {
            return level switch
            {
                LODLevel.High => 1,     // Every frame
                LODLevel.Medium => 2,   // Every 2 frames
                LODLevel.Low => 3,      // Every 3 frames
                LODLevel.VeryLow => 5,  // Every 5 frames
                LODLevel.Culled => 0,   // Never update
                _ => 1
            };
        }

        /// <summary>
        /// Get recommended segment count for trails/beams based on LOD.
        /// </summary>
        public int GetSegmentCount(LODLevel level, int baseSegments = 50)
        {
            return level switch
            {
                LODLevel.High => baseSegments,
                LODLevel.Medium => baseSegments / 2,
                LODLevel.Low => baseSegments / 5,
                LODLevel.VeryLow => 4,
                LODLevel.Culled => 0,
                _ => baseSegments
            };
        }

        /// <summary>
        /// Get recommended particle count based on LOD.
        /// </summary>
        public int GetParticleCount(LODLevel level, int baseCount = 30)
        {
            return level switch
            {
                LODLevel.High => baseCount,
                LODLevel.Medium => baseCount / 2,
                LODLevel.Low => baseCount / 6,
                LODLevel.VeryLow => 0,
                LODLevel.Culled => 0,
                _ => baseCount
            };
        }

        /// <summary>
        /// Check if glow effects should be enabled at this LOD level.
        /// </summary>
        public bool ShouldEnableGlow(LODLevel level)
        {
            return level <= LODLevel.Medium;
        }

        /// <summary>
        /// Check if noise/distortion effects should be enabled at this LOD level.
        /// </summary>
        public bool ShouldEnableNoise(LODLevel level)
        {
            return level == LODLevel.High;
        }

        /// <summary>
        /// Get shader quality parameter (0-1) for LOD-aware shaders.
        /// </summary>
        public float GetShaderQuality(LODLevel level)
        {
            return level switch
            {
                LODLevel.High => 1.0f,
                LODLevel.Medium => 0.7f,
                LODLevel.Low => 0.4f,
                LODLevel.VeryLow => 0.2f,
                LODLevel.Culled => 0f,
                _ => 1.0f
            };
        }
    }
}
