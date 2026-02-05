using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// Procedural skeletal animation system for multi-segment entities (worm bosses, tentacles, chains).
    /// 
    /// Instead of frame-by-frame animation, this uses:
    /// 1. Inverse Kinematics (IK) - segments follow constraints
    /// 2. Segment Lag Logic - each segment smoothly follows the previous
    /// 3. Velocity-Based Stretching - segments stretch along movement direction
    /// 4. Smooth LookAt Rotation - gradual rotation toward target
    /// 
    /// Inspired by Devourer of Gods and other Calamity worm bosses.
    /// </summary>
    public class SegmentAnimator
    {
        #region Segment Data

        /// <summary>
        /// Data for a single segment in the chain.
        /// </summary>
        public class Segment
        {
            public Vector2 Position;
            public Vector2 OldPosition;
            public Vector2 Velocity;
            public float Rotation;
            public float TargetRotation;
            public float Scale = 1f;
            public float BaseScale = 1f;

            /// <summary>
            /// Segment length (distance to next segment).
            /// </summary>
            public float Length;

            /// <summary>
            /// How quickly this segment rotates toward its target (0-1, higher = faster).
            /// </summary>
            public float RotationSpeed = 0.15f;

            /// <summary>
            /// How closely this segment follows the previous one (0-1, higher = tighter).
            /// </summary>
            public float FollowTightness = 0.25f;

            /// <summary>
            /// Velocity stretch factor for this segment.
            /// </summary>
            public float StretchFactor = 0.015f;
        }

        #endregion

        #region Fields

        private Segment[] segments;
        private int segmentCount;
        
        /// <summary>
        /// Maximum rotation change per frame (in radians).
        /// Prevents "snapping" rotation.
        /// </summary>
        public float MaxRotationPerFrame = 0.15f;

        /// <summary>
        /// Global velocity stretch intensity.
        /// </summary>
        public float StretchIntensity = 1f;

        /// <summary>
        /// Maximum stretch scale multiplier.
        /// </summary>
        public float MaxStretch = 1.5f;

        /// <summary>
        /// Minimum squash scale multiplier (for volume preservation).
        /// </summary>
        public float MinSquash = 0.7f;

        /// <summary>
        /// Smoothing factor for position interpolation (0-1).
        /// </summary>
        public float PositionSmoothing = 0.2f;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new segment animator.
        /// </summary>
        /// <param name="segmentCount">Number of segments in the chain</param>
        /// <param name="segmentLength">Default distance between segments</param>
        public SegmentAnimator(int segmentCount, float segmentLength = 30f)
        {
            this.segmentCount = segmentCount;
            segments = new Segment[segmentCount];

            for (int i = 0; i < segmentCount; i++)
            {
                segments[i] = new Segment
                {
                    Position = Vector2.Zero,
                    OldPosition = Vector2.Zero,
                    Velocity = Vector2.Zero,
                    Rotation = 0f,
                    TargetRotation = 0f,
                    Length = segmentLength,
                    BaseScale = 1f,
                    Scale = 1f
                };
            }
        }

        /// <summary>
        /// Creates a segment animator with varying segment lengths (like a worm with different body parts).
        /// </summary>
        public SegmentAnimator(float[] segmentLengths)
        {
            segmentCount = segmentLengths.Length;
            segments = new Segment[segmentCount];

            for (int i = 0; i < segmentCount; i++)
            {
                segments[i] = new Segment
                {
                    Position = Vector2.Zero,
                    OldPosition = Vector2.Zero,
                    Velocity = Vector2.Zero,
                    Rotation = 0f,
                    TargetRotation = 0f,
                    Length = segmentLengths[i],
                    BaseScale = 1f,
                    Scale = 1f
                };
            }
        }

        #endregion

        #region Core Update Logic

        /// <summary>
        /// Updates all segments based on the head position.
        /// Call this in AI() after moving the head.
        /// </summary>
        /// <param name="headPosition">Position of the head (segment 0 anchor point)</param>
        /// <param name="headRotation">Rotation of the head</param>
        public void Update(Vector2 headPosition, float headRotation)
        {
            if (segments == null || segmentCount == 0) return;

            // Update head segment
            segments[0].OldPosition = segments[0].Position;
            segments[0].Velocity = headPosition - segments[0].Position;
            segments[0].Position = headPosition;
            segments[0].TargetRotation = headRotation;
            UpdateSegmentRotation(0);
            UpdateSegmentStretch(0);

            // Update following segments using distance constraint
            for (int i = 1; i < segmentCount; i++)
            {
                Segment current = segments[i];
                Segment previous = segments[i - 1];

                current.OldPosition = current.Position;

                // Calculate direction and distance to previous segment
                Vector2 toPrevious = previous.Position - current.Position;
                float distance = toPrevious.Length();

                if (distance > 0.001f)
                {
                    Vector2 direction = toPrevious / distance;

                    // Target position maintains fixed distance from previous segment
                    Vector2 targetPosition = previous.Position - direction * previous.Length;

                    // Smooth follow with tightness factor
                    current.Position = Vector2.Lerp(current.Position, targetPosition, current.FollowTightness);

                    // Target rotation points toward previous segment
                    current.TargetRotation = toPrevious.ToRotation();
                }

                // Update velocity for stretch calculation
                current.Velocity = current.Position - current.OldPosition;

                // Update rotation and stretch
                UpdateSegmentRotation(i);
                UpdateSegmentStretch(i);
            }
        }

        /// <summary>
        /// Updates segment rotation with smooth interpolation.
        /// </summary>
        private void UpdateSegmentRotation(int index)
        {
            Segment seg = segments[index];

            // Calculate shortest rotation path
            float diff = MathHelper.WrapAngle(seg.TargetRotation - seg.Rotation);

            // Clamp rotation change per frame
            diff = MathHelper.Clamp(diff, -MaxRotationPerFrame, MaxRotationPerFrame);

            // Apply rotation with speed factor
            seg.Rotation = MathHelper.WrapAngle(seg.Rotation + diff * seg.RotationSpeed * 10f);
        }

        /// <summary>
        /// Updates segment stretch based on velocity.
        /// </summary>
        private void UpdateSegmentStretch(int index)
        {
            Segment seg = segments[index];

            float speed = seg.Velocity.Length();
            float stretchAmount = 1f + speed * seg.StretchFactor * StretchIntensity;

            // Clamp stretch
            stretchAmount = MathHelper.Clamp(stretchAmount, MinSquash, MaxStretch);

            // Smooth transition to new scale
            seg.Scale = MathHelper.Lerp(seg.Scale, seg.BaseScale * stretchAmount, 0.2f);
        }

        #endregion

        #region Segment Access

        /// <summary>
        /// Gets the position of a segment, interpolated for smooth rendering.
        /// </summary>
        public Vector2 GetInterpolatedPosition(int index)
        {
            if (index < 0 || index >= segmentCount) return Vector2.Zero;
            Segment seg = segments[index];
            return Vector2.Lerp(seg.OldPosition, seg.Position, InterpolatedRenderer.PartialTicks);
        }

        /// <summary>
        /// Gets the rotation of a segment.
        /// </summary>
        public float GetRotation(int index)
        {
            if (index < 0 || index >= segmentCount) return 0f;
            return segments[index].Rotation;
        }

        /// <summary>
        /// Gets the stretch scale of a segment.
        /// Returns (stretchX, squashY) for proper stretching along movement direction.
        /// </summary>
        public Vector2 GetStretchScale(int index)
        {
            if (index < 0 || index >= segmentCount) return Vector2.One;
            float stretch = segments[index].Scale;
            float squash = 1f / stretch; // Volume preservation
            return new Vector2(stretch, squash);
        }

        /// <summary>
        /// Gets raw segment data for advanced manipulation.
        /// </summary>
        public Segment GetSegment(int index)
        {
            if (index < 0 || index >= segmentCount) return null;
            return segments[index];
        }

        /// <summary>
        /// Number of segments in the chain.
        /// </summary>
        public int Count => segmentCount;

        #endregion

        #region Configuration

        /// <summary>
        /// Sets follow tightness for all segments.
        /// </summary>
        public void SetFollowTightness(float tightness)
        {
            for (int i = 0; i < segmentCount; i++)
                segments[i].FollowTightness = tightness;
        }

        /// <summary>
        /// Sets follow tightness with gradient (head tighter, tail looser).
        /// </summary>
        public void SetFollowTightnessGradient(float headTightness, float tailTightness)
        {
            for (int i = 0; i < segmentCount; i++)
            {
                float t = (float)i / (segmentCount - 1);
                segments[i].FollowTightness = MathHelper.Lerp(headTightness, tailTightness, t);
            }
        }

        /// <summary>
        /// Sets rotation speed for all segments.
        /// </summary>
        public void SetRotationSpeed(float speed)
        {
            for (int i = 0; i < segmentCount; i++)
                segments[i].RotationSpeed = speed;
        }

        /// <summary>
        /// Sets segment base scale with gradient (for tapered bodies).
        /// </summary>
        public void SetScaleGradient(float headScale, float tailScale)
        {
            for (int i = 0; i < segmentCount; i++)
            {
                float t = (float)i / (segmentCount - 1);
                segments[i].BaseScale = MathHelper.Lerp(headScale, tailScale, t);
                segments[i].Scale = segments[i].BaseScale;
            }
        }

        /// <summary>
        /// Initializes all segment positions in a line from head.
        /// Call this when spawning the entity.
        /// </summary>
        public void InitializePositions(Vector2 headPosition, float initialRotation)
        {
            Vector2 direction = new Vector2(MathF.Cos(initialRotation + MathHelper.Pi), 
                MathF.Sin(initialRotation + MathHelper.Pi));

            Vector2 currentPos = headPosition;
            for (int i = 0; i < segmentCount; i++)
            {
                segments[i].Position = currentPos;
                segments[i].OldPosition = currentPos;
                segments[i].Rotation = initialRotation;
                segments[i].TargetRotation = initialRotation;

                if (i < segmentCount - 1)
                    currentPos += direction * segments[i].Length;
            }
        }

        #endregion

        #region Drawing Helpers

        /// <summary>
        /// Draws all segments with a single texture.
        /// </summary>
        public void Draw(SpriteBatch spriteBatch, Texture2D texture, Color color, 
            Vector2 origin = default, SpriteEffects effects = SpriteEffects.None)
        {
            if (origin == default)
                origin = new Vector2(texture.Width / 2f, texture.Height / 2f);

            for (int i = 0; i < segmentCount; i++)
            {
                Vector2 drawPos = GetInterpolatedPosition(i) - Main.screenPosition;
                float rotation = GetRotation(i);
                Vector2 scale = GetStretchScale(i) * segments[i].BaseScale;

                spriteBatch.Draw(texture, drawPos, null, color, rotation, origin, scale, effects, 0f);
            }
        }

        /// <summary>
        /// Draws segments with different textures for head, body, and tail.
        /// </summary>
        public void Draw(SpriteBatch spriteBatch, Texture2D headTexture, Texture2D bodyTexture, 
            Texture2D tailTexture, Color color, Vector2? headOrigin = null, Vector2? bodyOrigin = null, 
            Vector2? tailOrigin = null, SpriteEffects effects = SpriteEffects.None)
        {
            for (int i = 0; i < segmentCount; i++)
            {
                Texture2D tex;
                Vector2 origin;

                if (i == 0)
                {
                    tex = headTexture;
                    origin = headOrigin ?? new Vector2(tex.Width / 2f, tex.Height / 2f);
                }
                else if (i == segmentCount - 1)
                {
                    tex = tailTexture;
                    origin = tailOrigin ?? new Vector2(tex.Width / 2f, tex.Height / 2f);
                }
                else
                {
                    tex = bodyTexture;
                    origin = bodyOrigin ?? new Vector2(tex.Width / 2f, tex.Height / 2f);
                }

                Vector2 drawPos = GetInterpolatedPosition(i) - Main.screenPosition;
                float rotation = GetRotation(i);
                Vector2 scale = GetStretchScale(i) * segments[i].BaseScale;

                spriteBatch.Draw(tex, drawPos, null, color, rotation, origin, scale, effects, 0f);
            }
        }

        /// <summary>
        /// Draws segments with custom draw callback for full control.
        /// </summary>
        public void Draw(SpriteBatch spriteBatch, Action<SpriteBatch, int, Vector2, float, Vector2> drawCallback)
        {
            for (int i = 0; i < segmentCount; i++)
            {
                Vector2 drawPos = GetInterpolatedPosition(i) - Main.screenPosition;
                float rotation = GetRotation(i);
                Vector2 scale = GetStretchScale(i) * segments[i].BaseScale;

                drawCallback(spriteBatch, i, drawPos, rotation, scale);
            }
        }

        #endregion

        #region Advanced: FABRIK IK Solver

        /// <summary>
        /// Forward And Backward Reaching Inverse Kinematics solver.
        /// Use when you need segments to reach toward a target while maintaining constraints.
        /// </summary>
        /// <param name="targetPosition">The position the end effector should try to reach</param>
        /// <param name="anchorPosition">The fixed base position (head position)</param>
        /// <param name="iterations">Number of solver iterations (2-10 recommended)</param>
        public void SolveFABRIK(Vector2 targetPosition, Vector2 anchorPosition, int iterations = 5)
        {
            if (segmentCount < 2) return;

            for (int iter = 0; iter < iterations; iter++)
            {
                // Backward pass: end to start
                segments[segmentCount - 1].Position = targetPosition;
                for (int i = segmentCount - 2; i >= 0; i--)
                {
                    Vector2 toNext = segments[i].Position - segments[i + 1].Position;
                    float dist = toNext.Length();
                    if (dist > 0.001f)
                    {
                        toNext = toNext / dist * segments[i].Length;
                        segments[i].Position = segments[i + 1].Position + toNext;
                    }
                }

                // Forward pass: start to end
                segments[0].Position = anchorPosition;
                for (int i = 0; i < segmentCount - 1; i++)
                {
                    Vector2 toNext = segments[i + 1].Position - segments[i].Position;
                    float dist = toNext.Length();
                    if (dist > 0.001f)
                    {
                        toNext = toNext / dist * segments[i].Length;
                        segments[i + 1].Position = segments[i].Position + toNext;
                    }
                }
            }

            // Update rotations to face along chain
            for (int i = 0; i < segmentCount - 1; i++)
            {
                Vector2 toNext = segments[i + 1].Position - segments[i].Position;
                segments[i].TargetRotation = toNext.ToRotation();
                UpdateSegmentRotation(i);
            }
            segments[segmentCount - 1].TargetRotation = segments[segmentCount - 2].Rotation;
            UpdateSegmentRotation(segmentCount - 1);
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Gets the total length of the segment chain.
        /// </summary>
        public float GetTotalLength()
        {
            float total = 0f;
            for (int i = 0; i < segmentCount; i++)
                total += segments[i].Length;
            return total;
        }

        /// <summary>
        /// Gets positions as an array for trail rendering.
        /// </summary>
        public Vector2[] GetPositionsArray(bool interpolated = true)
        {
            Vector2[] positions = new Vector2[segmentCount];
            for (int i = 0; i < segmentCount; i++)
            {
                positions[i] = interpolated ? GetInterpolatedPosition(i) : segments[i].Position;
            }
            return positions;
        }

        /// <summary>
        /// Checks if a segment is within the camera view (for culling).
        /// </summary>
        public bool IsSegmentVisible(int index, float margin = 100f)
        {
            if (index < 0 || index >= segmentCount) return false;
            Vector2 pos = segments[index].Position;
            return pos.X > Main.screenPosition.X - margin && 
                   pos.X < Main.screenPosition.X + Main.screenWidth + margin &&
                   pos.Y > Main.screenPosition.Y - margin && 
                   pos.Y < Main.screenPosition.Y + Main.screenHeight + margin;
        }

        #endregion
    }
}
