using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// Sub-pixel interpolation system for buttery-smooth rendering on high refresh rate monitors.
    /// 
    /// Vanilla Terraria runs game logic at 60 FPS but can render at higher rates.
    /// This system tracks how far we are between game update ticks (0-1) so we can
    /// smoothly interpolate positions/rotations for 144Hz+ displays.
    /// 
    /// The Math: Instead of drawing at entity.position, we draw at:
    /// Vector2 drawPos = Vector2.Lerp(previousPosition, currentPosition, partialTicks)
    /// 
    /// Where partialTicks is 0 right after a game update and approaches 1 right before the next.
    /// </summary>
    public static class InterpolatedRenderer
    {
        #region Partial Ticks System
        
        // Frame timing for interpolation calculation
        private static Stopwatch _frameTimer = new Stopwatch();
        private static double _lastGameUpdateTime;
        private static uint _lastGameUpdateCount;
        private static float _partialTicks = 1f;
        
        /// <summary>
        /// The partial tick value (0-1) representing how far we are between game updates.
        /// 0 = just after a game tick, 1 = right before the next tick.
        /// This is recalculated each render frame in UpdatePartialTicks().
        /// </summary>
        public static float PartialTicks => _partialTicks;
        
        /// <summary>
        /// Initializes the frame timer. Call once at mod load.
        /// </summary>
        public static void Initialize()
        {
            _frameTimer.Start();
            _lastGameUpdateTime = _frameTimer.Elapsed.TotalMilliseconds;
            _lastGameUpdateCount = Main.GameUpdateCount;
        }
        
        /// <summary>
        /// Updates the partial tick interpolation factor.
        /// Call this at the start of draw operations for accurate interpolation.
        /// 
        /// The calculation works by:
        /// 1. Detecting when a new game update happened (Main.GameUpdateCount changed)
        /// 2. Recording the time of that game update
        /// 3. Calculating how much time has passed since then as a fraction of 16.666ms
        /// </summary>
        public static void UpdatePartialTicks()
        {
            if (!_frameTimer.IsRunning)
            {
                _frameTimer.Start();
                _lastGameUpdateTime = _frameTimer.Elapsed.TotalMilliseconds;
                _lastGameUpdateCount = Main.GameUpdateCount;
            }
            
            double currentTime = _frameTimer.Elapsed.TotalMilliseconds;
            
            // Check if a new game update has occurred
            if (Main.GameUpdateCount != _lastGameUpdateCount)
            {
                // A new game tick happened - reset the timer
                _lastGameUpdateTime = currentTime;
                _lastGameUpdateCount = Main.GameUpdateCount;
            }
            
            // 16.666... ms per game update at 60 ticks per second
            const double gameTickInterval = 1000.0 / 60.0;
            
            // Calculate how far we are into the current tick (0-1)
            double timeSinceUpdate = currentTime - _lastGameUpdateTime;
            _partialTicks = MathHelper.Clamp((float)(timeSinceUpdate / gameTickInterval), 0f, 1f);
        }
        
        /// <summary>
        /// Resets the frame timer. Call on mod unload.
        /// </summary>
        public static void Shutdown()
        {
            _frameTimer.Stop();
            _frameTimer.Reset();
        }
        
        #endregion

        #region Core Interpolation

        /// <summary>
        /// Gets the interpolated world position for an NPC using partialTicks.
        /// Call this in PreDraw/PostDraw for smooth rendering.
        /// </summary>
        public static Vector2 GetInterpolatedPosition(NPC npc)
        {
            if (npc == null) return Vector2.Zero;
            return Vector2.Lerp(npc.oldPosition, npc.position, _partialTicks);
        }

        /// <summary>
        /// Gets the interpolated world center for an NPC.
        /// </summary>
        public static Vector2 GetInterpolatedCenter(NPC npc)
        {
            if (npc == null) return Vector2.Zero;
            Vector2 interpolatedPos = Vector2.Lerp(npc.oldPosition, npc.position, _partialTicks);
            return interpolatedPos + new Vector2(npc.width / 2f, npc.height / 2f);
        }

        /// <summary>
        /// Gets the interpolated world position for a Projectile.
        /// </summary>
        public static Vector2 GetInterpolatedPosition(Projectile proj)
        {
            if (proj == null) return Vector2.Zero;
            return Vector2.Lerp(proj.oldPosition, proj.position, _partialTicks);
        }

        /// <summary>
        /// Gets the interpolated world center for a Projectile.
        /// </summary>
        public static Vector2 GetInterpolatedCenter(Projectile proj)
        {
            if (proj == null) return Vector2.Zero;
            Vector2 interpolatedPos = Vector2.Lerp(proj.oldPosition, proj.position, _partialTicks);
            return interpolatedPos + new Vector2(proj.width / 2f, proj.height / 2f);
        }

        /// <summary>
        /// Gets the interpolated screen draw position for an NPC.
        /// Subtracts screenPosition for direct use in SpriteBatch.Draw.
        /// </summary>
        public static Vector2 GetInterpolatedDrawPos(NPC npc)
        {
            return GetInterpolatedCenter(npc) - Main.screenPosition;
        }

        /// <summary>
        /// Gets the interpolated screen draw position for a Projectile.
        /// </summary>
        public static Vector2 GetInterpolatedDrawPos(Projectile proj)
        {
            return GetInterpolatedCenter(proj) - Main.screenPosition;
        }

        #endregion

        #region Rotation Interpolation

        /// <summary>
        /// Interpolates rotation smoothly, handling the wrap-around at 2*PI.
        /// Essential for smooth rotation on high refresh displays.
        /// </summary>
        public static float InterpolateRotation(float oldRotation, float newRotation)
        {
            // Handle wrap-around at PI/-PI boundary
            float diff = MathHelper.WrapAngle(newRotation - oldRotation);
            return MathHelper.WrapAngle(oldRotation + diff * _partialTicks);
        }

        /// <summary>
        /// Smooth rotation interpolation for projectile-like entities with oldRot array.
        /// </summary>
        public static float GetInterpolatedRotation(Projectile proj)
        {
            if (proj == null || proj.oldRot == null || proj.oldRot.Length == 0)
                return proj?.rotation ?? 0f;

            return InterpolateRotation(proj.oldRot[0], proj.rotation);
        }

        /// <summary>
        /// Smooth rotation interpolation for NPC entities.
        /// Uses NPC.rotation directly since NPCs don't have oldRot arrays.
        /// </summary>
        public static float GetInterpolatedRotation(NPC npc)
        {
            if (npc == null)
                return 0f;
            
            // NPCs don't have oldRot arrays, so just return current rotation
            // For bosses with rotation tracking, consider caching oldRotation in BossVFXState
            return npc.rotation;
        }

        #endregion

        #region Advanced Interpolation

        /// <summary>
        /// Spherical linear interpolation for smoother rotation blending.
        /// Use when standard lerp creates "popping" at large angle differences.
        /// </summary>
        public static float Slerp(float from, float to, float t)
        {
            // Normalize angles to [0, 2PI]
            from = MathHelper.WrapAngle(from);
            to = MathHelper.WrapAngle(to);

            float diff = to - from;

            // Take the shorter path
            if (Math.Abs(diff) > MathHelper.Pi)
            {
                if (diff > 0)
                    diff -= MathHelper.TwoPi;
                else
                    diff += MathHelper.TwoPi;
            }

            return MathHelper.WrapAngle(from + diff * t);
        }

        /// <summary>
        /// Catmull-Rom spline interpolation for extra-smooth position curves.
        /// Requires 4 control points: p0 (before old), p1 (old), p2 (new), p3 (predicted).
        /// </summary>
        public static Vector2 CatmullRomInterpolate(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;

            return 0.5f * (
                (2f * p1) +
                (-p0 + p2) * t +
                (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
                (-p0 + 3f * p1 - 3f * p2 + p3) * t3
            );
        }

        /// <summary>
        /// Gets a predicted future position based on current velocity.
        /// Useful for Catmull-Rom splines and motion blur.
        /// </summary>
        public static Vector2 PredictPosition(Vector2 position, Vector2 velocity, float frames = 1f)
        {
            return position + velocity * frames;
        }

        #endregion

        #region Velocity-Based Effects

        /// <summary>
        /// Calculates squash and stretch scale based on velocity magnitude.
        /// Higher velocity = more stretch along movement direction.
        /// 
        /// Returns a Vector2 where X is stretch along velocity, Y is squash perpendicular.
        /// </summary>
        /// <param name="velocity">Entity velocity</param>
        /// <param name="stretchFactor">How much to stretch (0.1 = 10% stretch per velocity unit)</param>
        /// <param name="maxStretch">Maximum stretch multiplier (e.g., 1.5 = 50% max stretch)</param>
        public static Vector2 GetSquashStretch(Vector2 velocity, float stretchFactor = 0.02f, float maxStretch = 1.4f)
        {
            float speed = velocity.Length();
            float stretch = MathHelper.Clamp(1f + speed * stretchFactor, 1f, maxStretch);
            float squash = 1f / stretch; // Volume preservation

            return new Vector2(stretch, squash);
        }

        /// <summary>
        /// Gets the rotation angle for velocity-based stretching.
        /// The stretch should be aligned with the movement direction.
        /// </summary>
        public static float GetStretchRotation(Vector2 velocity)
        {
            if (velocity.LengthSquared() < 0.01f)
                return 0f;
            return velocity.ToRotation();
        }

        /// <summary>
        /// Calculates motion blur offset positions based on velocity.
        /// Returns an array of positions trailing behind the entity.
        /// </summary>
        /// <param name="position">Current position</param>
        /// <param name="velocity">Current velocity</param>
        /// <param name="blurSteps">Number of blur samples (4-8 recommended)</param>
        /// <param name="blurLength">How far back to sample (in frames)</param>
        public static Vector2[] GetMotionBlurPositions(Vector2 position, Vector2 velocity, 
            int blurSteps = 5, float blurLength = 3f)
        {
            Vector2[] positions = new Vector2[blurSteps];
            
            for (int i = 0; i < blurSteps; i++)
            {
                float t = (float)i / (blurSteps - 1);
                positions[i] = position - velocity * t * blurLength;
            }

            return positions;
        }

        #endregion

        #region Helper Draw Methods

        /// <summary>
        /// Draws a sprite with interpolated position for smooth movement.
        /// Drop-in replacement for standard SpriteBatch.Draw calls.
        /// </summary>
        public static void DrawInterpolated(this SpriteBatch spriteBatch, Texture2D texture,
            NPC npc, Rectangle? sourceRect, Color color, Vector2 origin, float scale, SpriteEffects effects)
        {
            Vector2 drawPos = GetInterpolatedDrawPos(npc);
            spriteBatch.Draw(texture, drawPos, sourceRect, color, npc.rotation, origin, scale, effects, 0f);
        }

        /// <summary>
        /// Draws a projectile with interpolated position.
        /// </summary>
        public static void DrawInterpolated(this SpriteBatch spriteBatch, Texture2D texture,
            Projectile proj, Rectangle? sourceRect, Color color, Vector2 origin, float scale, SpriteEffects effects)
        {
            Vector2 drawPos = GetInterpolatedDrawPos(proj);
            float rotation = GetInterpolatedRotation(proj);
            spriteBatch.Draw(texture, drawPos, sourceRect, color, rotation, origin, scale, effects, 0f);
        }

        /// <summary>
        /// Draws a sprite with velocity-based squash and stretch.
        /// Creates dynamic, juicy movement feel.
        /// </summary>
        public static void DrawWithStretch(this SpriteBatch spriteBatch, Texture2D texture,
            Vector2 position, Vector2 velocity, Rectangle? sourceRect, Color color, 
            Vector2 origin, float baseScale, SpriteEffects effects, 
            float stretchFactor = 0.02f, float maxStretch = 1.4f)
        {
            Vector2 stretchScale = GetSquashStretch(velocity, stretchFactor, maxStretch);
            float stretchRotation = GetStretchRotation(velocity);
            
            // Apply stretch as scale modification along velocity direction
            Vector2 finalScale = new Vector2(baseScale * stretchScale.X, baseScale * stretchScale.Y);
            
            spriteBatch.Draw(texture, position, sourceRect, color, stretchRotation, origin, finalScale, effects, 0f);
        }

        /// <summary>
        /// Draws motion blur for a fast-moving entity.
        /// Draws multiple fading copies trailing behind.
        /// </summary>
        public static void DrawMotionBlur(this SpriteBatch spriteBatch, Texture2D texture,
            Vector2 position, Vector2 velocity, Rectangle? sourceRect, Color color,
            float rotation, Vector2 origin, float scale, SpriteEffects effects,
            int blurSteps = 5, float blurLength = 3f, float opacityFalloff = 0.7f)
        {
            Vector2[] blurPositions = GetMotionBlurPositions(position, velocity, blurSteps, blurLength);
            
            for (int i = blurSteps - 1; i >= 0; i--)
            {
                float t = (float)i / (blurSteps - 1);
                float opacity = MathF.Pow(1f - t, opacityFalloff);
                float blurScale = scale * (1f - t * 0.1f); // Slight scale reduction for older frames
                
                Color blurColor = color * opacity;
                Vector2 drawPos = blurPositions[i] - Main.screenPosition;
                
                spriteBatch.Draw(texture, drawPos, sourceRect, blurColor, rotation, origin, blurScale, effects, 0f);
            }
        }

        #endregion

        #region Trail Position Smoothing

        /// <summary>
        /// Smooths a trail of old positions using Catmull-Rom interpolation.
        /// Creates much smoother trails than the raw position history.
        /// </summary>
        /// <param name="positions">Raw position array (oldest first or newest first)</param>
        /// <param name="subdivisions">Number of intermediate points per segment</param>
        /// <returns>Smoothed position array with more points</returns>
        public static Vector2[] SmoothTrailPositions(Vector2[] positions, int subdivisions = 3)
        {
            if (positions == null || positions.Length < 2)
                return positions;

            int resultCount = (positions.Length - 1) * subdivisions + 1;
            Vector2[] result = new Vector2[resultCount];
            
            int resultIndex = 0;
            for (int i = 0; i < positions.Length - 1; i++)
            {
                // Get 4 control points for Catmull-Rom
                Vector2 p0 = positions[Math.Max(0, i - 1)];
                Vector2 p1 = positions[i];
                Vector2 p2 = positions[i + 1];
                Vector2 p3 = positions[Math.Min(positions.Length - 1, i + 2)];

                for (int j = 0; j < subdivisions; j++)
                {
                    float t = (float)j / subdivisions;
                    result[resultIndex++] = CatmullRomInterpolate(p0, p1, p2, p3, t);
                }
            }

            // Add final point
            result[resultIndex] = positions[positions.Length - 1];
            
            return result;
        }

        #endregion
    }

    /// <summary>
    /// Tracks old position/rotation for entities that don't have built-in history.
    /// Attach to NPCs or projectiles via ModNPC/ModProjectile.
    /// </summary>
    public class InterpolationTracker
    {
        public Vector2 OldPosition { get; private set; }
        public Vector2 CurrentPosition { get; private set; }
        public float OldRotation { get; private set; }
        public float CurrentRotation { get; private set; }

        private Vector2[] positionHistory;
        private float[] rotationHistory;
        private int historyLength;
        private int historyIndex;

        /// <summary>
        /// Creates a new interpolation tracker.
        /// </summary>
        /// <param name="historyLength">Number of frames to track (4-16 recommended)</param>
        public InterpolationTracker(int historyLength = 8)
        {
            this.historyLength = historyLength;
            positionHistory = new Vector2[historyLength];
            rotationHistory = new float[historyLength];
            historyIndex = 0;
        }

        /// <summary>
        /// Updates the tracker. Call once per frame in AI().
        /// </summary>
        public void Update(Vector2 position, float rotation)
        {
            OldPosition = CurrentPosition;
            OldRotation = CurrentRotation;
            CurrentPosition = position;
            CurrentRotation = rotation;

            positionHistory[historyIndex] = position;
            rotationHistory[historyIndex] = rotation;
            historyIndex = (historyIndex + 1) % historyLength;
        }

        /// <summary>
        /// Gets the interpolated position using partialTicks.
        /// </summary>
        public Vector2 GetInterpolatedPosition()
        {
            return Vector2.Lerp(OldPosition, CurrentPosition, InterpolatedRenderer.PartialTicks);
        }

        /// <summary>
        /// Gets the interpolated rotation using partialTicks.
        /// </summary>
        public float GetInterpolatedRotation()
        {
            return InterpolatedRenderer.InterpolateRotation(OldRotation, CurrentRotation);
        }

        /// <summary>
        /// Gets the position history array for trail effects.
        /// Returns positions from oldest to newest.
        /// </summary>
        public Vector2[] GetPositionHistory()
        {
            Vector2[] result = new Vector2[historyLength];
            for (int i = 0; i < historyLength; i++)
            {
                int index = (historyIndex + i) % historyLength;
                result[i] = positionHistory[index];
            }
            return result;
        }

        /// <summary>
        /// Gets Catmull-Rom smoothed position history.
        /// </summary>
        public Vector2[] GetSmoothedPositionHistory(int subdivisions = 3)
        {
            return InterpolatedRenderer.SmoothTrailPositions(GetPositionHistory(), subdivisions);
        }
    }
}
