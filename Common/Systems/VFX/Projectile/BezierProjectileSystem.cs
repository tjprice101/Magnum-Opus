using Microsoft.Xna.Framework;
using System;
using Terraria;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// Bézier Curve Projectile System for Exo-style "miracle" weapon effects.
    /// 
    /// Instead of straight-line bullets, projectiles can arc and snake toward targets
    /// using quadratic and cubic Bézier curves.
    /// 
    /// The Math:
    /// - Quadratic: B(t) = (1-t)²P₀ + 2(1-t)tP₁ + t²P₂
    /// - Cubic: B(t) = (1-t)³P₀ + 3(1-t)²tP₁ + 3(1-t)t²P₂ + t³P₃
    /// </summary>
    public static class BezierProjectileSystem
    {
        #region Core Bézier Math
        
        /// <summary>
        /// Evaluates a quadratic Bézier curve at parameter t.
        /// </summary>
        /// <param name="p0">Start point</param>
        /// <param name="p1">Control point</param>
        /// <param name="p2">End point</param>
        /// <param name="t">Parameter 0-1</param>
        public static Vector2 QuadraticBezier(Vector2 p0, Vector2 p1, Vector2 p2, float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            float oneMinusT = 1f - t;
            
            return oneMinusT * oneMinusT * p0 +
                   2f * oneMinusT * t * p1 +
                   t * t * p2;
        }
        
        /// <summary>
        /// Evaluates a cubic Bézier curve at parameter t.
        /// Used for complex arcing and snaking projectile paths.
        /// </summary>
        /// <param name="p0">Start point</param>
        /// <param name="p1">First control point</param>
        /// <param name="p2">Second control point</param>
        /// <param name="p3">End point</param>
        /// <param name="t">Parameter 0-1</param>
        public static Vector2 CubicBezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            float oneMinusT = 1f - t;
            float oneMinusT2 = oneMinusT * oneMinusT;
            float oneMinusT3 = oneMinusT2 * oneMinusT;
            float t2 = t * t;
            float t3 = t2 * t;
            
            return oneMinusT3 * p0 +
                   3f * oneMinusT2 * t * p1 +
                   3f * oneMinusT * t2 * p2 +
                   t3 * p3;
        }
        
        /// <summary>
        /// Gets the tangent (derivative) of a quadratic Bézier at parameter t.
        /// Used for projectile rotation along the curve.
        /// </summary>
        public static Vector2 QuadraticBezierTangent(Vector2 p0, Vector2 p1, Vector2 p2, float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            float oneMinusT = 1f - t;
            
            Vector2 tangent = 2f * oneMinusT * (p1 - p0) + 2f * t * (p2 - p1);
            return tangent.SafeNormalize(Vector2.UnitX);
        }
        
        /// <summary>
        /// Gets the tangent of a cubic Bézier at parameter t.
        /// </summary>
        public static Vector2 CubicBezierTangent(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            float oneMinusT = 1f - t;
            float oneMinusT2 = oneMinusT * oneMinusT;
            float t2 = t * t;
            
            Vector2 tangent = 3f * oneMinusT2 * (p1 - p0) +
                              6f * oneMinusT * t * (p2 - p1) +
                              3f * t2 * (p3 - p2);
            return tangent.SafeNormalize(Vector2.UnitX);
        }
        
        #endregion

        #region Projectile Path Generation
        
        /// <summary>
        /// Generates control points for a homing arc from start to target.
        /// Creates a smooth curved path that arcs upward then down to target.
        /// </summary>
        /// <param name="start">Projectile spawn position</param>
        /// <param name="target">Target position</param>
        /// <param name="arcHeight">How high the arc goes (relative to distance)</param>
        /// <param name="curveDirection">-1 for left curve, 1 for right curve, 0 for upward</param>
        /// <returns>Array of 4 control points for cubic Bézier</returns>
        public static Vector2[] GenerateHomingArc(Vector2 start, Vector2 target, float arcHeight = 0.5f, float curveDirection = 0f)
        {
            Vector2 toTarget = target - start;
            float distance = toTarget.Length();
            Vector2 direction = toTarget.SafeNormalize(Vector2.UnitX);
            Vector2 perpendicular = new Vector2(-direction.Y, direction.X);
            
            // Base arc height scales with distance
            float height = distance * arcHeight;
            
            // Determine arc offset direction
            Vector2 arcOffset;
            if (Math.Abs(curveDirection) < 0.1f)
            {
                // Default upward arc
                arcOffset = -Vector2.UnitY * height;
            }
            else
            {
                // Curve left or right
                arcOffset = perpendicular * height * Math.Sign(curveDirection);
            }
            
            Vector2[] points = new Vector2[4];
            points[0] = start;
            points[1] = start + direction * (distance * 0.33f) + arcOffset;
            points[2] = start + direction * (distance * 0.66f) + arcOffset * 0.5f;
            points[3] = target;
            
            return points;
        }
        
        /// <summary>
        /// Generates a snaking S-curve path between two points.
        /// Perfect for beam weapons that "snake" toward targets.
        /// </summary>
        public static Vector2[] GenerateSnakingPath(Vector2 start, Vector2 target, float amplitude = 100f)
        {
            Vector2 toTarget = target - start;
            float distance = toTarget.Length();
            Vector2 direction = toTarget.SafeNormalize(Vector2.UnitX);
            Vector2 perpendicular = new Vector2(-direction.Y, direction.X);
            
            Vector2[] points = new Vector2[4];
            points[0] = start;
            points[1] = start + direction * (distance * 0.33f) + perpendicular * amplitude;
            points[2] = start + direction * (distance * 0.66f) - perpendicular * amplitude;
            points[3] = target;
            
            return points;
        }
        
        /// <summary>
        /// Generates a spiral approach path (for dramatic boss attacks).
        /// </summary>
        public static Vector2[] GenerateSpiralApproach(Vector2 start, Vector2 target, float spiralRadius = 150f, bool clockwise = true)
        {
            Vector2 toTarget = target - start;
            float distance = toTarget.Length();
            Vector2 direction = toTarget.SafeNormalize(Vector2.UnitX);
            Vector2 perpendicular = new Vector2(-direction.Y, direction.X);
            
            float sign = clockwise ? 1f : -1f;
            
            Vector2[] points = new Vector2[4];
            points[0] = start;
            points[1] = start + direction * (distance * 0.25f) + perpendicular * spiralRadius * sign;
            points[2] = start + direction * (distance * 0.75f) - perpendicular * spiralRadius * sign * 0.5f;
            points[3] = target;
            
            return points;
        }
        
        #endregion

        #region Real-Time Projectile Tracking
        
        /// <summary>
        /// State container for a Bézier-following projectile.
        /// Store this in Projectile.localAI or a ModProjectile field.
        /// </summary>
        public struct BezierState
        {
            public Vector2 P0, P1, P2, P3;
            public float T;
            public float Speed;
            public bool Initialized;
            public int TargetNPC;
            public bool UpdateTarget;
            
            /// <summary>
            /// Creates a new Bézier state with the given control points.
            /// </summary>
            public static BezierState Create(Vector2[] points, float speed, int targetNPC = -1, bool updateTarget = false)
            {
                return new BezierState
                {
                    P0 = points[0],
                    P1 = points[1],
                    P2 = points[2],
                    P3 = points[3],
                    T = 0f,
                    Speed = speed,
                    Initialized = true,
                    TargetNPC = targetNPC,
                    UpdateTarget = updateTarget
                };
            }
        }
        
        /// <summary>
        /// Updates a projectile following a Bézier curve.
        /// Call this in ModProjectile.AI().
        /// </summary>
        /// <param name="projectile">The projectile to update</param>
        /// <param name="state">The Bézier state (will be modified)</param>
        /// <returns>True if the curve is complete</returns>
        public static bool UpdateBezierProjectile(Projectile projectile, ref BezierState state)
        {
            if (!state.Initialized)
                return true;
                
            // Update target position if tracking
            if (state.UpdateTarget && state.TargetNPC >= 0 && state.TargetNPC < Main.maxNPCs)
            {
                NPC target = Main.npc[state.TargetNPC];
                if (target.active && !target.friendly)
                {
                    // Smoothly adjust P3 toward the current target position
                    state.P3 = Vector2.Lerp(state.P3, target.Center, 0.1f);
                    
                    // Also adjust P2 to maintain smooth approach
                    Vector2 toTarget = state.P3 - state.P0;
                    Vector2 direction = toTarget.SafeNormalize(Vector2.UnitX);
                    state.P2 = Vector2.Lerp(state.P2, state.P0 + direction * toTarget.Length() * 0.66f, 0.05f);
                }
            }
            
            // Calculate approximate curve length for consistent speed
            float approxLength = Vector2.Distance(state.P0, state.P1) +
                                 Vector2.Distance(state.P1, state.P2) +
                                 Vector2.Distance(state.P2, state.P3);
            
            // Advance t based on speed
            float tStep = state.Speed / Math.Max(approxLength, 1f);
            state.T += tStep;
            
            if (state.T >= 1f)
            {
                projectile.Center = state.P3;
                return true;
            }
            
            // Update position
            projectile.Center = CubicBezier(state.P0, state.P1, state.P2, state.P3, state.T);
            
            // Update rotation to face along curve
            Vector2 tangent = CubicBezierTangent(state.P0, state.P1, state.P2, state.P3, state.T);
            projectile.rotation = tangent.ToRotation();
            
            // Update velocity for particle effects (normalized tangent * speed)
            projectile.velocity = tangent * state.Speed;
            
            return false;
        }
        
        #endregion

        #region Utility Methods
        
        /// <summary>
        /// Samples multiple points along a Bézier curve for trail rendering.
        /// </summary>
        public static Vector2[] SampleCurve(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, int sampleCount)
        {
            Vector2[] samples = new Vector2[sampleCount];
            
            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)(sampleCount - 1);
                samples[i] = CubicBezier(p0, p1, p2, p3, t);
            }
            
            return samples;
        }
        
        /// <summary>
        /// Calculates the approximate length of a cubic Bézier curve.
        /// </summary>
        public static float ApproximateCurveLength(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, int segments = 10)
        {
            float length = 0f;
            Vector2 prev = p0;
            
            for (int i = 1; i <= segments; i++)
            {
                float t = i / (float)segments;
                Vector2 current = CubicBezier(p0, p1, p2, p3, t);
                length += Vector2.Distance(prev, current);
                prev = current;
            }
            
            return length;
        }
        
        /// <summary>
        /// Creates a smooth easing function for Bézier traversal.
        /// </summary>
        public static float EaseInOut(float t)
        {
            // Smooth step: 3t² - 2t³
            return t * t * (3f - 2f * t);
        }
        
        /// <summary>
        /// Creates an aggressive ease-in for fast finishes.
        /// </summary>
        public static float EaseIn(float t)
        {
            return t * t;
        }
        
        /// <summary>
        /// Creates a gentle ease-out for slow starts.
        /// </summary>
        public static float EaseOut(float t)
        {
            return 1f - (1f - t) * (1f - t);
        }
        
        #endregion
    }
}
