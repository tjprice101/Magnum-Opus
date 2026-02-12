using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems.VFX.Core
{
    /// <summary>
    /// Advanced projectile arc system with anticipation lines, trajectory prediction, and visual telegraphing.
    /// Perfect for boss attacks, homing projectiles, and dramatic weapon effects.
    /// 
    /// Features:
    /// - Anticipation/telegraph lines that show where projectiles will go
    /// - Smooth trajectory prediction for homing projectiles
    /// - Arc rendering with fade effects
    /// - Integration with BezierProjectileSystem for curved paths
    /// </summary>
    public static class ProjectileArcSystem
    {
        #region Arc Data Structures
        
        /// <summary>
        /// Represents a telegraphed attack trajectory.
        /// </summary>
        public class TelegraphArc
        {
            public Vector2 Start;
            public Vector2 End;
            public Vector2 ControlPoint;  // For curved arcs
            public bool IsCurved;
            public float Duration;         // Total time to display
            public float Timer;            // Current time
            public Color BaseColor;
            public float Width;
            public float FadeInTime;       // Time to fade in (0-1 of duration)
            public float FadeOutTime;      // Time to start fade out (0-1 of duration)
            public bool PulseAnimation;
            public int Id;                 // Unique identifier
            
            public float Progress => Duration > 0 ? Timer / Duration : 1f;
            public bool IsActive => Timer < Duration;
            
            /// <summary>
            /// Sample a point along the arc (t in 0-1).
            /// </summary>
            public Vector2 Sample(float t)
            {
                if (!IsCurved)
                    return Vector2.Lerp(Start, End, t);
                
                return BezierProjectileSystem.QuadraticBezier(Start, ControlPoint, End, t);
            }
            
            /// <summary>
            /// Get current opacity based on fade settings.
            /// </summary>
            public float GetOpacity()
            {
                float p = Progress;
                
                // Fade in
                if (p < FadeInTime && FadeInTime > 0)
                    return p / FadeInTime;
                
                // Fade out
                if (p > FadeOutTime && FadeOutTime < 1f)
                    return 1f - ((p - FadeOutTime) / (1f - FadeOutTime));
                
                return 1f;
            }
            
            /// <summary>
            /// Get color with pulse animation applied.
            /// </summary>
            public Color GetColor()
            {
                Color c = BaseColor;
                float opacity = GetOpacity();
                
                if (PulseAnimation)
                {
                    float pulse = (float)Math.Sin(Timer * 10f) * 0.2f + 0.8f;
                    opacity *= pulse;
                }
                
                return c * opacity;
            }
        }
        
        /// <summary>
        /// Represents a predicted trajectory for a moving projectile.
        /// </summary>
        public class TrajectoryPrediction
        {
            public Vector2[] Points;
            public float[] TimeStamps;
            public int PointCount;
            public Color Color;
            public float Width;
            public float MaxLifetime;
            
            public TrajectoryPrediction(int maxPoints = 50)
            {
                Points = new Vector2[maxPoints];
                TimeStamps = new float[maxPoints];
                PointCount = 0;
            }
        }
        
        #endregion
        
        #region Active Arcs Management
        
        private static readonly List<TelegraphArc> ActiveArcs = new List<TelegraphArc>();
        private static int NextArcId = 0;
        
        /// <summary>
        /// Clear all active telegraph arcs.
        /// </summary>
        public static void ClearAll()
        {
            ActiveArcs.Clear();
        }
        
        /// <summary>
        /// Update all active arcs.
        /// </summary>
        public static void Update()
        {
            for (int i = ActiveArcs.Count - 1; i >= 0; i--)
            {
                ActiveArcs[i].Timer++;
                if (!ActiveArcs[i].IsActive)
                    ActiveArcs.RemoveAt(i);
            }
        }
        
        #endregion
        
        #region Telegraph Arc Creation
        
        /// <summary>
        /// Create a straight line telegraph.
        /// </summary>
        public static int CreateLineTelegraph(Vector2 start, Vector2 end, Color color, 
            float duration = 60f, float width = 4f, float fadeIn = 0.1f, float fadeOut = 0.8f, bool pulse = true)
        {
            var arc = new TelegraphArc
            {
                Start = start,
                End = end,
                IsCurved = false,
                BaseColor = color,
                Duration = duration,
                Timer = 0,
                Width = width,
                FadeInTime = fadeIn,
                FadeOutTime = fadeOut,
                PulseAnimation = pulse,
                Id = NextArcId++
            };
            ActiveArcs.Add(arc);
            return arc.Id;
        }
        
        /// <summary>
        /// Create a curved arc telegraph.
        /// </summary>
        public static int CreateCurvedTelegraph(Vector2 start, Vector2 control, Vector2 end, Color color,
            float duration = 60f, float width = 4f, float fadeIn = 0.1f, float fadeOut = 0.8f, bool pulse = true)
        {
            var arc = new TelegraphArc
            {
                Start = start,
                ControlPoint = control,
                End = end,
                IsCurved = true,
                BaseColor = color,
                Duration = duration,
                Timer = 0,
                Width = width,
                FadeInTime = fadeIn,
                FadeOutTime = fadeOut,
                PulseAnimation = pulse,
                Id = NextArcId++
            };
            ActiveArcs.Add(arc);
            return arc.Id;
        }
        
        /// <summary>
        /// Create a homing arc telegraph toward a target.
        /// </summary>
        public static int CreateHomingTelegraph(Vector2 start, Vector2 target, float arcHeight, Color color,
            float duration = 60f, float width = 3f)
        {
            var curve = BezierProjectileSystem.GenerateHomingArc(start, target, arcHeight);
            return CreateCurvedTelegraph(curve.P0, curve.P1, curve.P2, color, duration, width);
        }
        
        /// <summary>
        /// Create a converging warning pattern (common boss telegraph).
        /// </summary>
        public static void CreateConvergingTelegraph(Vector2 center, int lineCount, float startRadius, Color color,
            float duration = 60f, float width = 2f)
        {
            for (int i = 0; i < lineCount; i++)
            {
                float angle = MathHelper.TwoPi * i / lineCount;
                Vector2 start = center + angle.ToRotationVector2() * startRadius;
                CreateLineTelegraph(start, center, color, duration, width, 0.2f, 0.7f);
            }
        }
        
        /// <summary>
        /// Create a radial burst telegraph pattern.
        /// </summary>
        public static void CreateRadialTelegraph(Vector2 center, int lineCount, float radius, Color color,
            float duration = 60f, float width = 2f, float angleOffset = 0f)
        {
            for (int i = 0; i < lineCount; i++)
            {
                float angle = MathHelper.TwoPi * i / lineCount + angleOffset;
                Vector2 end = center + angle.ToRotationVector2() * radius;
                CreateLineTelegraph(center, end, color, duration, width, 0.1f, 0.7f);
            }
        }
        
        /// <summary>
        /// Create a safe zone indicator (ring of converging lines with gap).
        /// </summary>
        public static void CreateSafeZoneTelegraph(Vector2 center, float safeAngle, float safeArcWidth, 
            float radius, int lineCount, Color dangerColor, Color safeColor, float duration = 60f)
        {
            for (int i = 0; i < lineCount; i++)
            {
                float angle = MathHelper.TwoPi * i / lineCount;
                float angleDiff = Math.Abs(MathHelper.WrapAngle(angle - safeAngle));
                
                Vector2 end = center + angle.ToRotationVector2() * radius;
                
                if (angleDiff < safeArcWidth / 2f)
                {
                    // Safe zone - green/cyan indicator
                    CreateLineTelegraph(center, end, safeColor, duration, 3f, 0.1f, 0.8f, pulse: false);
                }
                else
                {
                    // Danger zone - red indicator
                    CreateLineTelegraph(center, end, dangerColor, duration, 2f, 0.1f, 0.7f);
                }
            }
        }
        
        /// <summary>
        /// Cancel a telegraph by ID.
        /// </summary>
        public static void CancelTelegraph(int id)
        {
            ActiveArcs.RemoveAll(a => a.Id == id);
        }
        
        #endregion
        
        #region Trajectory Prediction
        
        /// <summary>
        /// Predict trajectory for a projectile with gravity.
        /// </summary>
        public static TrajectoryPrediction PredictBallisticTrajectory(Vector2 start, Vector2 velocity, 
            float gravity, float timeSteps, int steps, Color color, float width = 2f)
        {
            var prediction = new TrajectoryPrediction(steps);
            prediction.Color = color;
            prediction.Width = width;
            prediction.MaxLifetime = timeSteps * steps;
            
            Vector2 pos = start;
            Vector2 vel = velocity;
            
            for (int i = 0; i < steps; i++)
            {
                prediction.Points[i] = pos;
                prediction.TimeStamps[i] = i * timeSteps;
                prediction.PointCount++;
                
                // Simulate physics
                vel.Y += gravity * timeSteps;
                pos += vel * timeSteps;
                
                // Stop if below screen
                if (pos.Y > Main.worldSurface * 16 + 1000)
                    break;
            }
            
            return prediction;
        }
        
        /// <summary>
        /// Predict trajectory for a homing projectile.
        /// </summary>
        public static TrajectoryPrediction PredictHomingTrajectory(Vector2 start, Vector2 initialVelocity,
            Vector2 target, float homingStrength, float maxSpeed, int steps, float stepTime, 
            Color color, float width = 2f)
        {
            var prediction = new TrajectoryPrediction(steps);
            prediction.Color = color;
            prediction.Width = width;
            prediction.MaxLifetime = stepTime * steps;
            
            Vector2 pos = start;
            Vector2 vel = initialVelocity;
            
            for (int i = 0; i < steps; i++)
            {
                prediction.Points[i] = pos;
                prediction.TimeStamps[i] = i * stepTime;
                prediction.PointCount++;
                
                // Simulate homing
                Vector2 toTarget = target - pos;
                float dist = toTarget.Length();
                if (dist > 10f)
                {
                    toTarget.Normalize();
                    vel += toTarget * homingStrength;
                    
                    // Clamp speed
                    if (vel.Length() > maxSpeed)
                        vel = Vector2.Normalize(vel) * maxSpeed;
                }
                
                pos += vel * stepTime;
                
                // Stop if reached target
                if (dist < 20f)
                    break;
            }
            
            return prediction;
        }
        
        /// <summary>
        /// Predict trajectory for a sine-wave projectile.
        /// </summary>
        public static TrajectoryPrediction PredictWaveTrajectory(Vector2 start, Vector2 direction,
            float speed, float amplitude, float frequency, int steps, float stepTime,
            Color color, float width = 2f)
        {
            var prediction = new TrajectoryPrediction(steps);
            prediction.Color = color;
            prediction.Width = width;
            prediction.MaxLifetime = stepTime * steps;
            
            direction.Normalize();
            Vector2 perpendicular = new Vector2(-direction.Y, direction.X);
            
            for (int i = 0; i < steps; i++)
            {
                float t = i * stepTime;
                float wave = (float)Math.Sin(t * frequency) * amplitude;
                
                Vector2 pos = start + direction * speed * t + perpendicular * wave;
                
                prediction.Points[i] = pos;
                prediction.TimeStamps[i] = t;
                prediction.PointCount++;
            }
            
            return prediction;
        }
        
        #endregion
        
        #region Drawing
        
        /// <summary>
        /// Draw all active telegraph arcs.
        /// </summary>
        public static void DrawAll(SpriteBatch spriteBatch)
        {
            if (ActiveArcs.Count == 0) return;
            
            Texture2D pixel = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow2", 
                ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            
            foreach (var arc in ActiveArcs)
            {
                DrawArc(spriteBatch, arc, pixel);
            }
        }
        
        /// <summary>
        /// Draw a single telegraph arc.
        /// </summary>
        private static void DrawArc(SpriteBatch spriteBatch, TelegraphArc arc, Texture2D texture)
        {
            Color color = arc.GetColor();
            if (color.A < 10) return;
            
            // Remove alpha for additive blending
            color = new Color(color.R, color.G, color.B, 0);
            
            int segments = arc.IsCurved ? 20 : 1;
            
            for (int i = 0; i < segments; i++)
            {
                float t0 = (float)i / segments;
                float t1 = (float)(i + 1) / segments;
                
                Vector2 p0 = arc.Sample(t0) - Main.screenPosition;
                Vector2 p1 = arc.Sample(t1) - Main.screenPosition;
                
                DrawLine(spriteBatch, texture, p0, p1, color, arc.Width);
            }
            
            // Draw endpoint indicators
            Vector2 startScreen = arc.Start - Main.screenPosition;
            Vector2 endScreen = arc.End - Main.screenPosition;
            
            // Start point glow
            spriteBatch.Draw(texture, startScreen, null, color * 0.5f, 0f, 
                texture.Size() / 2f, arc.Width * 0.1f, SpriteEffects.None, 0f);
            
            // End point (target) glow - brighter
            spriteBatch.Draw(texture, endScreen, null, color, 0f,
                texture.Size() / 2f, arc.Width * 0.15f, SpriteEffects.None, 0f);
        }
        
        /// <summary>
        /// Draw a trajectory prediction.
        /// </summary>
        public static void DrawPrediction(SpriteBatch spriteBatch, TrajectoryPrediction prediction)
        {
            if (prediction.PointCount < 2) return;
            
            Texture2D pixel = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow2",
                ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            
            for (int i = 0; i < prediction.PointCount - 1; i++)
            {
                float progress = (float)i / (prediction.PointCount - 1);
                float opacity = 1f - progress * 0.7f; // Fade out toward end
                
                Color color = prediction.Color * opacity;
                color = new Color(color.R, color.G, color.B, 0);
                
                Vector2 p0 = prediction.Points[i] - Main.screenPosition;
                Vector2 p1 = prediction.Points[i + 1] - Main.screenPosition;
                
                float width = prediction.Width * (1f - progress * 0.5f);
                
                DrawLine(spriteBatch, pixel, p0, p1, color, width);
            }
        }
        
        /// <summary>
        /// Draw a simple line between two points.
        /// </summary>
        private static void DrawLine(SpriteBatch spriteBatch, Texture2D texture, 
            Vector2 start, Vector2 end, Color color, float width)
        {
            Vector2 edge = end - start;
            float angle = (float)Math.Atan2(edge.Y, edge.X);
            float length = edge.Length();
            
            Rectangle sourceRect = new Rectangle(0, 0, 1, 1);
            Vector2 origin = new Vector2(0, 0.5f);
            Vector2 scale = new Vector2(length, width);
            
            spriteBatch.Draw(texture, start, sourceRect, color, angle, origin, scale, SpriteEffects.None, 0f);
        }
        
        /// <summary>
        /// Draw a dashed line for warning indicators.
        /// </summary>
        public static void DrawDashedLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, 
            Color color, float width, float dashLength = 8f, float gapLength = 4f)
        {
            Texture2D texture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow2",
                ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            
            Vector2 direction = end - start;
            float totalLength = direction.Length();
            direction.Normalize();
            
            float covered = 0f;
            bool drawing = true;
            
            while (covered < totalLength)
            {
                float segmentLength = drawing ? dashLength : gapLength;
                segmentLength = Math.Min(segmentLength, totalLength - covered);
                
                if (drawing)
                {
                    Vector2 segStart = start + direction * covered - Main.screenPosition;
                    Vector2 segEnd = start + direction * (covered + segmentLength) - Main.screenPosition;
                    DrawLine(spriteBatch, texture, segStart, segEnd, color, width);
                }
                
                covered += segmentLength;
                drawing = !drawing;
            }
        }
        
        /// <summary>
        /// Draw a warning ring indicator.
        /// </summary>
        public static void DrawWarningRing(SpriteBatch spriteBatch, Vector2 center, float radius,
            Color color, float width, int segments = 32, float progress = 1f)
        {
            Texture2D texture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow2",
                ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            
            int segmentsToDraw = (int)(segments * progress);
            
            for (int i = 0; i < segmentsToDraw; i++)
            {
                float angle0 = MathHelper.TwoPi * i / segments;
                float angle1 = MathHelper.TwoPi * (i + 1) / segments;
                
                Vector2 p0 = center + angle0.ToRotationVector2() * radius - Main.screenPosition;
                Vector2 p1 = center + angle1.ToRotationVector2() * radius - Main.screenPosition;
                
                DrawLine(spriteBatch, texture, p0, p1, color, width);
            }
        }
        
        #endregion
        
        #region Utility Functions
        
        /// <summary>
        /// Calculate intercept point for a moving target.
        /// Returns the position to aim at.
        /// </summary>
        public static Vector2 CalculateInterceptPoint(Vector2 shooterPos, float projectileSpeed,
            Vector2 targetPos, Vector2 targetVelocity)
        {
            Vector2 toTarget = targetPos - shooterPos;
            float a = Vector2.Dot(targetVelocity, targetVelocity) - projectileSpeed * projectileSpeed;
            float b = 2f * Vector2.Dot(targetVelocity, toTarget);
            float c = Vector2.Dot(toTarget, toTarget);
            
            float discriminant = b * b - 4f * a * c;
            
            if (discriminant < 0 || Math.Abs(a) < 0.0001f)
            {
                // No solution - just aim at current position
                return targetPos;
            }
            
            float t1 = (-b - (float)Math.Sqrt(discriminant)) / (2f * a);
            float t2 = (-b + (float)Math.Sqrt(discriminant)) / (2f * a);
            
            float t = (t1 > 0 && t1 < t2) ? t1 : t2;
            
            if (t < 0) return targetPos;
            
            return targetPos + targetVelocity * t;
        }
        
        /// <summary>
        /// Get arc height for a mortar-style lob shot.
        /// </summary>
        public static float CalculateMortarArcHeight(Vector2 start, Vector2 target, float flightTime, float gravity)
        {
            float horizontalDist = Vector2.Distance(start, target);
            float verticalDiff = target.Y - start.Y;
            
            // Peak is at midpoint of flight
            float peakTime = flightTime / 2f;
            float peakHeight = 0.5f * gravity * peakTime * peakTime;
            
            return peakHeight + Math.Abs(verticalDiff) * 0.5f;
        }
        
        #endregion
    }
}
