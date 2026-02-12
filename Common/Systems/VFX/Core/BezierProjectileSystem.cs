using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// BEZIER CURVE SYSTEM FOR PROJECTILE PATHING
    /// 
    /// Implements the advanced Bezier curve techniques from the VFX learning resources:
    /// - Quadratic Bezier (3 control points) - Simple curves, homing arcs
    /// - Cubic Bezier (4 control points) - S-curves, complex paths
    /// - Rational Bezier - Weighted control points for tighter/looser curves
    /// - Arc length parameterization - Uniform speed along curve
    /// 
    /// USE CASES:
    /// - Homing projectiles with graceful arc paths
    /// - Magic missiles that curve toward targets
    /// - Boss attack patterns with predictable curved paths
    /// - Chain lightning visual arcs
    /// - Whip/tendril motion curves
    /// 
    /// EXAMPLE:
    ///   // Create a homing arc from projectile to target
    ///   var curve = BezierProjectileSystem.GenerateHomingArc(
    ///       Projectile.Center, target.Center, Projectile.velocity, 0.5f);
    ///   
    ///   // Sample the curve for current position (t = 0 to 1)
    ///   float t = 1f - (Projectile.timeLeft / (float)maxTimeLeft);
    ///   Projectile.Center = curve.Evaluate(t);
    /// </summary>
    public static class BezierProjectileSystem
    {
        #region Core Bezier Functions
        
        /// <summary>
        /// Quadratic Bezier curve (3 control points).
        /// P(t) = (1-t)²P₀ + 2(1-t)tP₁ + t²P₂
        /// </summary>
        /// <param name="p0">Start point</param>
        /// <param name="p1">Control point (curve bends toward this)</param>
        /// <param name="p2">End point</param>
        /// <param name="t">Parameter from 0 to 1</param>
        public static Vector2 QuadraticBezier(Vector2 p0, Vector2 p1, Vector2 p2, float t)
        {
            float oneMinusT = 1f - t;
            return oneMinusT * oneMinusT * p0 + 
                   2f * oneMinusT * t * p1 + 
                   t * t * p2;
        }
        
        /// <summary>
        /// Cubic Bezier curve (4 control points).
        /// P(t) = (1-t)³P₀ + 3(1-t)²tP₁ + 3(1-t)t²P₂ + t³P₃
        /// </summary>
        /// <param name="p0">Start point</param>
        /// <param name="p1">First control point (affects start curvature)</param>
        /// <param name="p2">Second control point (affects end curvature)</param>
        /// <param name="p3">End point</param>
        /// <param name="t">Parameter from 0 to 1</param>
        public static Vector2 CubicBezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
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
        /// Derivative of quadratic Bezier (velocity/tangent at point t).
        /// P'(t) = 2(1-t)(P₁-P₀) + 2t(P₂-P₁)
        /// </summary>
        public static Vector2 QuadraticBezierDerivative(Vector2 p0, Vector2 p1, Vector2 p2, float t)
        {
            float oneMinusT = 1f - t;
            return 2f * oneMinusT * (p1 - p0) + 2f * t * (p2 - p1);
        }
        
        /// <summary>
        /// Derivative of cubic Bezier (velocity/tangent at point t).
        /// P'(t) = 3(1-t)²(P₁-P₀) + 6(1-t)t(P₂-P₁) + 3t²(P₃-P₂)
        /// </summary>
        public static Vector2 CubicBezierDerivative(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            float oneMinusT = 1f - t;
            float oneMinusT2 = oneMinusT * oneMinusT;
            float t2 = t * t;
            
            return 3f * oneMinusT2 * (p1 - p0) + 
                   6f * oneMinusT * t * (p2 - p1) + 
                   3f * t2 * (p3 - p2);
        }
        
        #endregion
        
        #region Homing Arc Generation
        
        /// <summary>
        /// Generates a quadratic Bezier arc for homing projectiles.
        /// The projectile will curve gracefully from its current position/velocity toward the target.
        /// </summary>
        /// <param name="start">Current projectile position</param>
        /// <param name="end">Target position</param>
        /// <param name="startVelocity">Current velocity (determines initial arc direction)</param>
        /// <param name="curveIntensity">How much the path curves (0 = straight, 1 = aggressive curve)</param>
        /// <returns>BezierCurve struct for sampling</returns>
        public static QuadraticCurve GenerateHomingArc(Vector2 start, Vector2 end, Vector2 startVelocity, float curveIntensity = 0.5f)
        {
            // Project velocity forward to create control point
            float distance = Vector2.Distance(start, end);
            Vector2 velocityDirection = startVelocity.SafeNormalize(Vector2.UnitX);
            
            // Control point is projected along initial velocity, scaled by curve intensity
            Vector2 controlPoint = start + velocityDirection * distance * curveIntensity;
            
            return new QuadraticCurve(start, controlPoint, end);
        }
        
        /// <summary>
        /// Generates a quadratic Bezier arc for homing telegraph effects.
        /// Creates an arc that curves upward (or to the side) between start and target.
        /// </summary>
        /// <param name="start">Start position</param>
        /// <param name="target">Target position</param>
        /// <param name="arcHeight">Height of the arc (perpendicular offset for the control point)</param>
        /// <returns>QuadraticCurve struct for sampling</returns>
        public static QuadraticCurve GenerateHomingArc(Vector2 start, Vector2 target, float arcHeight)
        {
            Vector2 toTarget = target - start;
            float distance = toTarget.Length();
            Vector2 direction = toTarget.SafeNormalize(Vector2.UnitX);
            Vector2 perpendicular = new Vector2(-direction.Y, direction.X);
            
            // Control point is at the midpoint, offset perpendicular by arcHeight
            Vector2 midpoint = (start + target) * 0.5f;
            Vector2 controlPoint = midpoint + perpendicular * arcHeight * distance;
            
            return new QuadraticCurve(start, controlPoint, target);
        }
        
        /// <summary>
        /// Generates a cubic S-curve Bezier for weaving projectile paths.
        /// Creates an elegant S-shape between start and end.
        /// </summary>
        /// <param name="start">Start position</param>
        /// <param name="end">End position</param>
        /// <param name="waveAmplitude">How far the S-curve extends perpendicular to travel</param>
        /// <param name="startTangentLength">Length of start tangent (affects sharpness)</param>
        /// <param name="endTangentLength">Length of end tangent</param>
        public static CubicCurve GenerateSCurve(Vector2 start, Vector2 end, float waveAmplitude, 
            float startTangentLength = 0.33f, float endTangentLength = 0.33f)
        {
            Vector2 toEnd = end - start;
            float distance = toEnd.Length();
            Vector2 direction = toEnd / distance;
            Vector2 perpendicular = new Vector2(-direction.Y, direction.X);
            
            // Control points create the S-shape
            Vector2 p1 = start + direction * distance * startTangentLength + perpendicular * waveAmplitude;
            Vector2 p2 = end - direction * distance * endTangentLength - perpendicular * waveAmplitude;
            
            return new CubicCurve(start, p1, p2, end);
        }
        
        /// <summary>
        /// Generates an overhead arc (like a mortar/lob trajectory).
        /// </summary>
        /// <param name="start">Launch position</param>
        /// <param name="end">Landing position</param>
        /// <param name="arcHeight">How high the arc peaks above the start/end line</param>
        public static QuadraticCurve GenerateOverheadArc(Vector2 start, Vector2 end, float arcHeight)
        {
            // Control point is at midpoint, elevated by arc height
            Vector2 midpoint = (start + end) * 0.5f;
            Vector2 controlPoint = midpoint - new Vector2(0, arcHeight); // Subtract Y (up in Terraria)
            
            return new QuadraticCurve(start, controlPoint, end);
        }
        
        /// <summary>
        /// Generates a curved path that avoids an obstacle by arcing around it.
        /// </summary>
        /// <param name="start">Start position</param>
        /// <param name="end">End position</param>
        /// <param name="avoidPoint">Point to curve away from</param>
        /// <param name="avoidDistance">How far to curve away</param>
        public static QuadraticCurve GenerateAvoidanceCurve(Vector2 start, Vector2 end, Vector2 avoidPoint, float avoidDistance)
        {
            // Calculate perpendicular direction from avoid point to line
            Vector2 midpoint = (start + end) * 0.5f;
            Vector2 toAvoid = avoidPoint - midpoint;
            Vector2 awayFromAvoid = -toAvoid.SafeNormalize(Vector2.UnitY);
            
            // Control point is pushed away from avoid point
            Vector2 controlPoint = midpoint + awayFromAvoid * avoidDistance;
            
            return new QuadraticCurve(start, controlPoint, end);
        }
        
        #endregion
        
        #region Arc Length Parameterization
        
        /// <summary>
        /// Calculates approximate arc length of a quadratic Bezier using subdivision.
        /// </summary>
        /// <param name="p0">Start point</param>
        /// <param name="p1">Control point</param>
        /// <param name="p2">End point</param>
        /// <param name="subdivisions">Number of line segments to approximate with</param>
        public static float QuadraticBezierArcLength(Vector2 p0, Vector2 p1, Vector2 p2, int subdivisions = 20)
        {
            float length = 0f;
            Vector2 prevPoint = p0;
            
            for (int i = 1; i <= subdivisions; i++)
            {
                float t = (float)i / subdivisions;
                Vector2 point = QuadraticBezier(p0, p1, p2, t);
                length += Vector2.Distance(prevPoint, point);
                prevPoint = point;
            }
            
            return length;
        }
        
        /// <summary>
        /// Calculates approximate arc length of a cubic Bezier.
        /// </summary>
        public static float CubicBezierArcLength(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, int subdivisions = 25)
        {
            float length = 0f;
            Vector2 prevPoint = p0;
            
            for (int i = 1; i <= subdivisions; i++)
            {
                float t = (float)i / subdivisions;
                Vector2 point = CubicBezier(p0, p1, p2, p3, t);
                length += Vector2.Distance(prevPoint, point);
                prevPoint = point;
            }
            
            return length;
        }
        
        /// <summary>
        /// Creates a lookup table for arc-length parameterization.
        /// This allows uniform-speed traversal of the curve.
        /// </summary>
        /// <param name="curve">The curve to parameterize</param>
        /// <param name="samples">Number of samples in the LUT</param>
        /// <returns>Array mapping uniform distance to t parameter</returns>
        public static float[] CreateArcLengthLUT(QuadraticCurve curve, int samples = 50)
        {
            float[] lut = new float[samples];
            float totalLength = 0f;
            float[] segmentLengths = new float[samples];
            Vector2 prevPoint = curve.Evaluate(0f);
            
            // First pass: calculate segment lengths
            for (int i = 1; i < samples; i++)
            {
                float t = (float)i / (samples - 1);
                Vector2 point = curve.Evaluate(t);
                segmentLengths[i] = Vector2.Distance(prevPoint, point);
                totalLength += segmentLengths[i];
                prevPoint = point;
            }
            
            // Second pass: create normalized distance LUT
            float cumulativeLength = 0f;
            lut[0] = 0f;
            for (int i = 1; i < samples; i++)
            {
                cumulativeLength += segmentLengths[i];
                lut[i] = cumulativeLength / totalLength;
            }
            
            return lut;
        }
        
        /// <summary>
        /// Sample the curve at uniform arc-length distance.
        /// </summary>
        /// <param name="curve">The curve</param>
        /// <param name="normalizedDistance">0-1 distance along curve (uniform speed)</param>
        /// <param name="lut">Pre-computed arc length lookup table</param>
        public static Vector2 SampleAtArcLength(QuadraticCurve curve, float normalizedDistance, float[] lut)
        {
            normalizedDistance = MathHelper.Clamp(normalizedDistance, 0f, 1f);
            
            // Binary search for the t parameter
            int lower = 0;
            int upper = lut.Length - 1;
            
            while (lower < upper - 1)
            {
                int mid = (lower + upper) / 2;
                if (lut[mid] < normalizedDistance)
                    lower = mid;
                else
                    upper = mid;
            }
            
            // Interpolate between samples
            float segmentStart = lut[lower];
            float segmentEnd = lut[upper];
            float segmentT = (normalizedDistance - segmentStart) / (segmentEnd - segmentStart);
            
            float t = ((float)lower + segmentT) / (lut.Length - 1);
            return curve.Evaluate(t);
        }
        
        #endregion
        
        #region Curve Structs
        
        /// <summary>
        /// Represents a quadratic Bezier curve with cached control points.
        /// </summary>
        public struct QuadraticCurve
        {
            public Vector2 P0, P1, P2;
            
            public QuadraticCurve(Vector2 p0, Vector2 p1, Vector2 p2)
            {
                P0 = p0; P1 = p1; P2 = p2;
            }
            
            public Vector2 Evaluate(float t) => QuadraticBezier(P0, P1, P2, t);
            public Vector2 EvaluateDerivative(float t) => QuadraticBezierDerivative(P0, P1, P2, t);
            public float GetRotationAt(float t) => EvaluateDerivative(t).ToRotation();
            public float ArcLength(int subdivisions = 20) => QuadraticBezierArcLength(P0, P1, P2, subdivisions);
            
            /// <summary>
            /// Samples the curve at regular intervals for trail rendering.
            /// </summary>
            public Vector2[] SamplePoints(int count)
            {
                Vector2[] points = new Vector2[count];
                for (int i = 0; i < count; i++)
                {
                    float t = (float)i / (count - 1);
                    points[i] = Evaluate(t);
                }
                return points;
            }
        }
        
        /// <summary>
        /// Represents a cubic Bezier curve with cached control points.
        /// </summary>
        public struct CubicCurve
        {
            public Vector2 P0, P1, P2, P3;
            
            public CubicCurve(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
            {
                P0 = p0; P1 = p1; P2 = p2; P3 = p3;
            }
            
            public Vector2 Evaluate(float t) => CubicBezier(P0, P1, P2, P3, t);
            public Vector2 EvaluateDerivative(float t) => CubicBezierDerivative(P0, P1, P2, P3, t);
            public float GetRotationAt(float t) => EvaluateDerivative(t).ToRotation();
            public float ArcLength(int subdivisions = 25) => CubicBezierArcLength(P0, P1, P2, P3, subdivisions);
            
            /// <summary>
            /// Samples the curve at regular intervals for trail rendering.
            /// </summary>
            public Vector2[] SamplePoints(int count)
            {
                Vector2[] points = new Vector2[count];
                for (int i = 0; i < count; i++)
                {
                    float t = (float)i / (count - 1);
                    points[i] = Evaluate(t);
                }
                return points;
            }
            
            /// <summary>
            /// Splits the curve at parameter t into two curves.
            /// Uses de Casteljau's algorithm.
            /// </summary>
            public (CubicCurve left, CubicCurve right) SplitAt(float t)
            {
                // De Casteljau subdivision
                Vector2 q0 = Vector2.Lerp(P0, P1, t);
                Vector2 q1 = Vector2.Lerp(P1, P2, t);
                Vector2 q2 = Vector2.Lerp(P2, P3, t);
                
                Vector2 r0 = Vector2.Lerp(q0, q1, t);
                Vector2 r1 = Vector2.Lerp(q1, q2, t);
                
                Vector2 s = Vector2.Lerp(r0, r1, t);
                
                return (
                    new CubicCurve(P0, q0, r0, s),
                    new CubicCurve(s, r1, q2, P3)
                );
            }
        }
        
        #endregion
        
        #region Projectile Path Helpers
        
        /// <summary>
        /// Creates anticipation line points for telegraphing a curved projectile path.
        /// Returns fewer, evenly-spaced points for drawing warning lines.
        /// </summary>
        public static Vector2[] GetAnticipationPath(QuadraticCurve curve, int markerCount = 8)
        {
            // Use arc-length parameterization for even spacing
            float[] lut = CreateArcLengthLUT(curve, 50);
            Vector2[] markers = new Vector2[markerCount];
            
            for (int i = 0; i < markerCount; i++)
            {
                float dist = (float)i / (markerCount - 1);
                markers[i] = SampleAtArcLength(curve, dist, lut);
            }
            
            return markers;
        }
        
        /// <summary>
        /// Calculates where a projectile following this curve will be after N frames.
        /// Useful for AI prediction.
        /// </summary>
        /// <param name="curve">The path curve</param>
        /// <param name="currentT">Current position on curve (0-1)</param>
        /// <param name="speed">Speed in world units per frame</param>
        /// <param name="frames">How many frames to predict ahead</param>
        public static Vector2 PredictPositionAlongCurve(QuadraticCurve curve, float currentT, float speed, int frames)
        {
            float arcLength = curve.ArcLength();
            float currentDistance = currentT * arcLength;
            float futureDistance = currentDistance + speed * frames;
            float futureT = MathHelper.Clamp(futureDistance / arcLength, 0f, 1f);
            
            return curve.Evaluate(futureT);
        }
        
        /// <summary>
        /// Generates a trail of positions along a curve, with older positions fading back.
        /// Perfect for rendering curved projectile trails.
        /// </summary>
        /// <param name="curve">The curve path</param>
        /// <param name="headT">Current position on curve (the "head" of the trail)</param>
        /// <param name="trailLength">How far back the trail extends (0-1 on curve)</param>
        /// <param name="pointCount">Number of trail points</param>
        public static Vector2[] GenerateTrailPositions(QuadraticCurve curve, float headT, float trailLength, int pointCount)
        {
            Vector2[] trail = new Vector2[pointCount];
            float startT = Math.Max(0f, headT - trailLength);
            float step = (headT - startT) / (pointCount - 1);
            
            for (int i = 0; i < pointCount; i++)
            {
                float t = startT + step * i;
                trail[i] = curve.Evaluate(t);
            }
            
            return trail;
        }
        
        #endregion
        
        #region Advanced Curve Operations
        
        /// <summary>
        /// Converts a series of points into a smooth Bezier spline.
        /// Creates a composite curve that passes through all points.
        /// </summary>
        /// <param name="points">Points the curve should pass through</param>
        /// <param name="tension">How tight the curve is (0 = smooth, 1 = sharp corners)</param>
        public static List<CubicCurve> FitCurveThroughPoints(Vector2[] points, float tension = 0.5f)
        {
            if (points.Length < 2)
                return new List<CubicCurve>();
                
            List<CubicCurve> curves = new List<CubicCurve>();
            
            for (int i = 0; i < points.Length - 1; i++)
            {
                Vector2 p0 = points[i];
                Vector2 p3 = points[i + 1];
                
                // Calculate tangents using Catmull-Rom style
                Vector2 tangentStart, tangentEnd;
                
                if (i == 0)
                    tangentStart = (p3 - p0) * 0.5f;
                else
                    tangentStart = (points[i + 1] - points[i - 1]) * 0.5f * (1f - tension);
                    
                if (i == points.Length - 2)
                    tangentEnd = (p3 - p0) * 0.5f;
                else
                    tangentEnd = (points[i + 2] - points[i]) * 0.5f * (1f - tension);
                
                // Convert tangents to control points
                Vector2 p1 = p0 + tangentStart / 3f;
                Vector2 p2 = p3 - tangentEnd / 3f;
                
                curves.Add(new CubicCurve(p0, p1, p2, p3));
            }
            
            return curves;
        }
        
        /// <summary>
        /// Evaluates a composite Bezier spline at parameter t (0-1 across all segments).
        /// </summary>
        public static Vector2 EvaluateSpline(List<CubicCurve> curves, float t)
        {
            if (curves.Count == 0)
                return Vector2.Zero;
                
            t = MathHelper.Clamp(t, 0f, 1f);
            float scaledT = t * curves.Count;
            int segmentIndex = (int)MathF.Floor(scaledT);
            
            if (segmentIndex >= curves.Count)
                segmentIndex = curves.Count - 1;
                
            float localT = scaledT - segmentIndex;
            return curves[segmentIndex].Evaluate(localT);
        }
        
        #endregion
        
        #region BezierState for Projectile AI
        
        /// <summary>
        /// State struct for managing bezier-path projectile movement.
        /// Used by CalamityStyleVFX for homing and snaking projectile paths.
        /// </summary>
        public struct BezierState
        {
            public bool Initialized;
            public Vector2[] ControlPoints;
            public float T;              // Current position on curve (0-1)
            public float Speed;          // How fast T increases per frame
            public int TargetWhoAmI;     // Target NPC index for homing updates
            public bool UpdateTarget;    // Whether to update control points when target moves
            
            public static BezierState Create(Vector2[] controlPoints, float speed, int targetWhoAmI = -1, bool updateTarget = false)
            {
                return new BezierState
                {
                    Initialized = true,
                    ControlPoints = controlPoints,
                    T = 0f,
                    Speed = speed,
                    TargetWhoAmI = targetWhoAmI,
                    UpdateTarget = updateTarget
                };
            }
        }
        
        /// <summary>
        /// Generates a homing arc path using cubic bezier.
        /// Overload that takes arcHeight and curveDirection instead of velocity.
        /// </summary>
        public static Vector2[] GenerateHomingArc(Vector2 start, Vector2 end, float arcHeight, float curveDirection)
        {
            Vector2 toEnd = end - start;
            float distance = toEnd.Length();
            Vector2 direction = toEnd.SafeNormalize(Vector2.UnitX);
            Vector2 perpendicular = new Vector2(-direction.Y, direction.X) * curveDirection;
            
            // Create control points for a curved arc
            Vector2 p1 = start + direction * distance * 0.33f + perpendicular * arcHeight * distance;
            Vector2 p2 = start + direction * distance * 0.66f + perpendicular * arcHeight * distance * 0.5f;
            
            return new Vector2[] { start, p1, p2, end };
        }
        
        /// <summary>
        /// Generates a snaking S-curve path.
        /// </summary>
        public static Vector2[] GenerateSnakingPath(Vector2 start, Vector2 end, float amplitude)
        {
            Vector2 toEnd = end - start;
            float distance = toEnd.Length();
            Vector2 direction = toEnd.SafeNormalize(Vector2.UnitX);
            Vector2 perpendicular = new Vector2(-direction.Y, direction.X);
            
            // S-curve: first curve one way, then the other
            Vector2 p1 = start + direction * distance * 0.33f + perpendicular * amplitude;
            Vector2 p2 = start + direction * distance * 0.66f - perpendicular * amplitude;
            
            return new Vector2[] { start, p1, p2, end };
        }
        
        /// <summary>
        /// Updates a projectile following a bezier path.
        /// </summary>
        public static void UpdateBezierProjectile(Projectile projectile, ref BezierState state)
        {
            if (!state.Initialized || state.ControlPoints == null || state.ControlPoints.Length < 3)
                return;
            
            // Update target position if tracking
            if (state.UpdateTarget && state.TargetWhoAmI >= 0 && state.TargetWhoAmI < Main.maxNPCs)
            {
                NPC target = Main.npc[state.TargetWhoAmI];
                if (target.active)
                {
                    // Update end point to track target
                    state.ControlPoints[state.ControlPoints.Length - 1] = target.Center;
                }
            }
            
            // Advance along curve
            state.T += state.Speed;
            state.T = MathHelper.Clamp(state.T, 0f, 1f);
            
            // Evaluate position based on number of control points
            Vector2 newPos;
            Vector2 tangent;
            
            if (state.ControlPoints.Length == 3)
            {
                // Quadratic bezier
                newPos = QuadraticBezier(state.ControlPoints[0], state.ControlPoints[1], state.ControlPoints[2], state.T);
                tangent = QuadraticBezierDerivative(state.ControlPoints[0], state.ControlPoints[1], state.ControlPoints[2], state.T);
            }
            else if (state.ControlPoints.Length >= 4)
            {
                // Cubic bezier
                newPos = CubicBezier(state.ControlPoints[0], state.ControlPoints[1], state.ControlPoints[2], state.ControlPoints[3], state.T);
                tangent = CubicBezierDerivative(state.ControlPoints[0], state.ControlPoints[1], state.ControlPoints[2], state.ControlPoints[3], state.T);
            }
            else
            {
                return;
            }
            
            // Update projectile
            projectile.Center = newPos;
            projectile.velocity = tangent.SafeNormalize(Vector2.UnitX) * tangent.Length() * 0.1f;
            projectile.rotation = tangent.ToRotation();
            
            // Kill projectile when path complete
            if (state.T >= 1f)
            {
                projectile.Kill();
            }
        }
        
        #endregion
    }
}
