using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems.VFX.Optimization
{
    /// <summary>
    /// Adaptive quality manager that adjusts VFX quality based on frame rate.
    /// Automatically scales down effects when performance drops, and scales up
    /// when there's headroom.
    /// </summary>
    public class AdaptiveQualityManager : ModSystem
    {
        // Performance targets in milliseconds
        private const float TargetFrameTime = 16.67f;  // 60 FPS
        private const float MinFrameTime = 13.33f;     // 75 FPS (headroom for upgrade)
        private const float MaxFrameTime = 20.0f;      // 50 FPS (trigger downgrade)

        /// <summary>
        /// Quality levels from highest to lowest.
        /// </summary>
        public enum QualityLevel
        {
            Ultra = 4,
            High = 3,
            Medium = 2,
            Low = 1,
            Potato = 0
        }

        private QualityLevel currentQuality;
        private Queue<float> frameTimeHistory;
        private const int HistorySize = 60; // 1 second at 60fps
        private int framesAtCurrentQuality;
        private const int MinFramesBeforeChange = 60; // Wait 1 second before changing

        private static AdaptiveQualityManager _instance;
        public static AdaptiveQualityManager Instance => _instance;

        // Quality-based settings
        public QualityLevel CurrentQuality => currentQuality;
        public bool EnableGlow => currentQuality >= QualityLevel.Medium;
        public bool EnableParticles => currentQuality >= QualityLevel.Low;
        public bool EnablePostProcess => currentQuality >= QualityLevel.High;
        public bool EnableShaders => currentQuality >= QualityLevel.Medium;
        public bool EnableNoise => currentQuality >= QualityLevel.High;
        public bool EnableBloom => currentQuality >= QualityLevel.Medium;
        public bool EnableMotionBlurBloom => currentQuality >= QualityLevel.High;

        public int MaxParticles => currentQuality switch
        {
            QualityLevel.Ultra => 5000,
            QualityLevel.High => 3000,
            QualityLevel.Medium => 1500,
            QualityLevel.Low => 500,
            QualityLevel.Potato => 100,
            _ => 1000
        };

        public int BloomLayers => currentQuality switch
        {
            QualityLevel.Ultra => 4,
            QualityLevel.High => 3,
            QualityLevel.Medium => 2,
            QualityLevel.Low => 1,
            QualityLevel.Potato => 0,
            _ => 2
        };

        public float ParticleQuality => currentQuality switch
        {
            QualityLevel.Ultra => 1.0f,
            QualityLevel.High => 0.8f,
            QualityLevel.Medium => 0.5f,
            QualityLevel.Low => 0.25f,
            QualityLevel.Potato => 0.1f,
            _ => 0.5f
        };

        public override void Load()
        {
            _instance = this;
            currentQuality = QualityLevel.High;
            frameTimeHistory = new Queue<float>(HistorySize);
            framesAtCurrentQuality = 0;
        }

        public override void Unload()
        {
            _instance = null;
            frameTimeHistory = null;
        }

        public override void PostUpdateEverything()
        {
            // Approximate frame time from game update count
            // In real implementation, use Stopwatch for accurate timing
            float frameTime = 1000f / 60f; // Assume 60fps for now

            // Use Main.frameRate if available
            if (Main.frameRate > 0)
            {
                frameTime = 1000f / Main.frameRate;
            }

            UpdateQuality(frameTime);
        }

        private void UpdateQuality(float frameTime)
        {
            // Add to history
            frameTimeHistory.Enqueue(frameTime);
            if (frameTimeHistory.Count > HistorySize)
                frameTimeHistory.Dequeue();

            framesAtCurrentQuality++;

            // Don't change quality too quickly
            if (framesAtCurrentQuality < MinFramesBeforeChange)
                return;

            // Need enough history
            if (frameTimeHistory.Count < HistorySize / 2)
                return;

            // Calculate average frame time
            float avgFrameTime = frameTimeHistory.Average();

            // Decrease quality if struggling
            if (avgFrameTime > MaxFrameTime && currentQuality > QualityLevel.Potato)
            {
                currentQuality--;
                OnQualityChanged();
            }
            // Increase quality if running smoothly
            else if (avgFrameTime < MinFrameTime && currentQuality < QualityLevel.Ultra)
            {
                // Be more conservative about upgrading - need longer stable period
                if (framesAtCurrentQuality > MinFramesBeforeChange * 2)
                {
                    currentQuality++;
                    OnQualityChanged();
                }
            }
        }

        private void OnQualityChanged()
        {
            framesAtCurrentQuality = 0;
            frameTimeHistory.Clear();
        }

        /// <summary>
        /// Manually set quality level (bypasses adaptive adjustment temporarily).
        /// </summary>
        public void SetQuality(QualityLevel quality)
        {
            currentQuality = quality;
            OnQualityChanged();
        }

        /// <summary>
        /// Get current FPS estimate.
        /// </summary>
        public float GetCurrentFPS()
        {
            if (frameTimeHistory.Count == 0)
                return 60f;

            float avgFrameTime = frameTimeHistory.Average();
            return avgFrameTime > 0 ? 1000f / avgFrameTime : 60f;
        }

        /// <summary>
        /// Get quality as a 0-1 multiplier.
        /// </summary>
        public float GetQualityMultiplier()
        {
            return (int)currentQuality / (float)(int)QualityLevel.Ultra;
        }

        /// <summary>
        /// Check if a particular effect should be enabled at current quality.
        /// </summary>
        public bool ShouldEnable(QualityLevel requiredQuality)
        {
            return currentQuality >= requiredQuality;
        }

        /// <summary>
        /// Get recommended segment count for trails based on quality.
        /// </summary>
        public int GetTrailSegments(int baseSegments = 50)
        {
            return (int)(baseSegments * ParticleQuality);
        }

        /// <summary>
        /// Get recommended particle count based on quality.
        /// </summary>
        public int GetParticleCount(int baseCount = 30)
        {
            return (int)(baseCount * ParticleQuality);
        }
    }
}
