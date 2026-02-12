using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// VERLET PHYSICS SYSTEM FOR ROPE/CHAIN DYNAMICS
    /// 
    /// Implements position-based Verlet integration for realistic physics simulation:
    /// - Rope/chain segments with distance constraints
    /// - Gravity, wind, and damping forces
    /// - Collision with world geometry (optional)
    /// - Smooth constraint solving with multiple iterations
    /// 
    /// USE CASES:
    /// - Whip weapon trails that flow naturally
    /// - Boss tentacles/tendrils with organic motion
    /// - Chain lightning that sags and sways
    /// - Hanging decorations and tethers
    /// - Hair/cape physics for NPCs
    /// - Rope bridges and swinging objects
    /// 
    /// VERLET INTEGRATION FORMULA:
    ///   x_new = x_current + (x_current - x_previous) * damping + acceleration * dt²
    ///   
    /// This is more stable than Euler integration because velocity is implicit
    /// in the position difference, and constraints can be applied directly.
    /// 
    /// EXAMPLE:
    ///   // Create a whip chain
    ///   var chain = new VerletChain(15, 12f); // 15 segments, 12px each
    ///   chain.SetAnchor(player.Center);
    ///   chain.Gravity = new Vector2(0, 0.3f);
    ///   
    ///   // Update each frame
    ///   chain.Update();
    ///   
    ///   // Get positions for rendering
    ///   Vector2[] positions = chain.GetPositions();
    /// </summary>
    public class VerletChain
    {
        #region Configuration
        
        /// <summary>
        /// Individual segment particles.
        /// </summary>
        private Vector2[] _currentPositions;
        private Vector2[] _previousPositions;
        
        /// <summary>
        /// Number of segments in the chain.
        /// </summary>
        public int SegmentCount { get; private set; }
        
        /// <summary>
        /// Rest length between adjacent segments.
        /// </summary>
        public float SegmentLength { get; set; }
        
        /// <summary>
        /// Gravity force applied each frame. Default (0, 0.3f) for natural droop.
        /// </summary>
        public Vector2 Gravity { get; set; } = new Vector2(0, 0.3f);
        
        /// <summary>
        /// Damping factor (0-1). Lower = more energy loss, smoother settling.
        /// 0.98f is good for rope, 0.9f for heavy chains.
        /// </summary>
        public float Damping { get; set; } = 0.98f;
        
        /// <summary>
        /// Number of constraint solving iterations. More = stiffer chain.
        /// 3-5 for ropes, 8-12 for rigid chains.
        /// </summary>
        public int ConstraintIterations { get; set; } = 5;
        
        /// <summary>
        /// Whether the first point is anchored (attached to something).
        /// </summary>
        public bool IsStartAnchored { get; set; } = true;
        
        /// <summary>
        /// Whether the last point is anchored.
        /// </summary>
        public bool IsEndAnchored { get; set; } = false;
        
        /// <summary>
        /// Optional wind force that varies over time.
        /// </summary>
        public Vector2 Wind { get; set; } = Vector2.Zero;
        
        /// <summary>
        /// Stiffness of the chain (0-1). Higher = less stretchy.
        /// </summary>
        public float Stiffness { get; set; } = 1f;
        
        #endregion
        
        #region Constructors
        
        /// <summary>
        /// Creates a new Verlet chain.
        /// </summary>
        /// <param name="segmentCount">Number of points in the chain</param>
        /// <param name="segmentLength">Distance between adjacent points</param>
        public VerletChain(int segmentCount, float segmentLength)
        {
            SegmentCount = Math.Max(2, segmentCount);
            SegmentLength = segmentLength;
            
            _currentPositions = new Vector2[SegmentCount];
            _previousPositions = new Vector2[SegmentCount];
        }
        
        /// <summary>
        /// Creates a chain starting at a position and extending in a direction.
        /// </summary>
        public VerletChain(int segmentCount, float segmentLength, Vector2 startPosition, Vector2 direction)
            : this(segmentCount, segmentLength)
        {
            Vector2 normalizedDir = direction.SafeNormalize(new Vector2(0, 1));
            
            for (int i = 0; i < SegmentCount; i++)
            {
                _currentPositions[i] = startPosition + normalizedDir * segmentLength * i;
                _previousPositions[i] = _currentPositions[i];
            }
        }
        
        #endregion
        
        #region Anchor Management
        
        /// <summary>
        /// Sets the position of the first (start) anchor point.
        /// </summary>
        public void SetStartAnchor(Vector2 position)
        {
            if (SegmentCount > 0)
            {
                _currentPositions[0] = position;
                _previousPositions[0] = position;
            }
        }
        
        /// <summary>
        /// Sets the position of the last (end) anchor point.
        /// </summary>
        public void SetEndAnchor(Vector2 position)
        {
            if (SegmentCount > 0)
            {
                int last = SegmentCount - 1;
                _currentPositions[last] = position;
                _previousPositions[last] = position;
            }
        }
        
        /// <summary>
        /// Initializes all points in a line from start to end.
        /// </summary>
        public void InitializeLine(Vector2 start, Vector2 end)
        {
            for (int i = 0; i < SegmentCount; i++)
            {
                float t = (float)i / (SegmentCount - 1);
                _currentPositions[i] = Vector2.Lerp(start, end, t);
                _previousPositions[i] = _currentPositions[i];
            }
        }
        
        /// <summary>
        /// Initializes all points at the same position (collapsed).
        /// Chain will unfold naturally due to gravity.
        /// </summary>
        public void InitializeCollapsed(Vector2 position)
        {
            for (int i = 0; i < SegmentCount; i++)
            {
                _currentPositions[i] = position;
                _previousPositions[i] = position;
            }
        }
        
        #endregion
        
        #region Physics Update
        
        /// <summary>
        /// Main update loop. Call once per frame.
        /// </summary>
        public void Update()
        {
            ApplyForces();
            SolveConstraints();
        }
        
        /// <summary>
        /// Applies gravity, wind, and Verlet integration.
        /// </summary>
        private void ApplyForces()
        {
            // Calculate wind variation
            Vector2 currentWind = Wind;
            if (Wind.LengthSquared() > 0.001f)
            {
                // Add some noise to wind
                float noise = (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.5f + 0.5f;
                currentWind *= noise;
            }
            
            // Total acceleration
            Vector2 acceleration = Gravity + currentWind;
            
            // Verlet integration for each non-anchored point
            for (int i = 0; i < SegmentCount; i++)
            {
                // Skip anchored points
                if (i == 0 && IsStartAnchored) continue;
                if (i == SegmentCount - 1 && IsEndAnchored) continue;
                
                Vector2 current = _currentPositions[i];
                Vector2 previous = _previousPositions[i];
                
                // Verlet integration: new_pos = current + (current - previous) * damping + acceleration
                Vector2 velocity = (current - previous) * Damping;
                Vector2 newPosition = current + velocity + acceleration;
                
                _previousPositions[i] = current;
                _currentPositions[i] = newPosition;
            }
        }
        
        /// <summary>
        /// Solves distance constraints between adjacent points.
        /// Multiple iterations make the chain stiffer.
        /// </summary>
        private void SolveConstraints()
        {
            for (int iteration = 0; iteration < ConstraintIterations; iteration++)
            {
                for (int i = 0; i < SegmentCount - 1; i++)
                {
                    Vector2 pointA = _currentPositions[i];
                    Vector2 pointB = _currentPositions[i + 1];
                    
                    Vector2 delta = pointB - pointA;
                    float distance = delta.Length();
                    
                    if (distance < 0.0001f) continue; // Avoid division by zero
                    
                    // Calculate how much to correct
                    float error = (distance - SegmentLength) / distance;
                    Vector2 correction = delta * error * 0.5f * Stiffness;
                    
                    // Apply correction to both points (unless anchored)
                    bool aAnchored = (i == 0 && IsStartAnchored);
                    bool bAnchored = (i + 1 == SegmentCount - 1 && IsEndAnchored);
                    
                    if (aAnchored && bAnchored)
                    {
                        // Both anchored, no correction possible
                    }
                    else if (aAnchored)
                    {
                        _currentPositions[i + 1] -= correction * 2f;
                    }
                    else if (bAnchored)
                    {
                        _currentPositions[i] += correction * 2f;
                    }
                    else
                    {
                        _currentPositions[i] += correction;
                        _currentPositions[i + 1] -= correction;
                    }
                }
            }
        }
        
        #endregion
        
        #region Position Access
        
        /// <summary>
        /// Returns a copy of all current positions.
        /// </summary>
        public Vector2[] GetPositions()
        {
            Vector2[] result = new Vector2[SegmentCount];
            Array.Copy(_currentPositions, result, SegmentCount);
            return result;
        }
        
        /// <summary>
        /// Gets position at specific index.
        /// </summary>
        public Vector2 GetPosition(int index)
        {
            if (index < 0 || index >= SegmentCount) return Vector2.Zero;
            return _currentPositions[index];
        }
        
        /// <summary>
        /// Gets the position at the tip (end) of the chain.
        /// </summary>
        public Vector2 GetTipPosition()
        {
            return _currentPositions[SegmentCount - 1];
        }
        
        /// <summary>
        /// Gets velocity (current - previous) at specific index.
        /// </summary>
        public Vector2 GetVelocity(int index)
        {
            if (index < 0 || index >= SegmentCount) return Vector2.Zero;
            return _currentPositions[index] - _previousPositions[index];
        }
        
        /// <summary>
        /// Gets the total length of the chain (sum of segment distances).
        /// May differ from SegmentCount * SegmentLength if chain is stretched.
        /// </summary>
        public float GetTotalLength()
        {
            float length = 0f;
            for (int i = 0; i < SegmentCount - 1; i++)
            {
                length += Vector2.Distance(_currentPositions[i], _currentPositions[i + 1]);
            }
            return length;
        }
        
        #endregion
        
        #region Force Application
        
        /// <summary>
        /// Applies an impulse force to a specific point.
        /// </summary>
        public void ApplyImpulse(int index, Vector2 impulse)
        {
            if (index < 0 || index >= SegmentCount) return;
            if (index == 0 && IsStartAnchored) return;
            if (index == SegmentCount - 1 && IsEndAnchored) return;
            
            // Modify previous position to add velocity
            _previousPositions[index] -= impulse;
        }
        
        /// <summary>
        /// Applies an impulse to all points in the chain.
        /// </summary>
        public void ApplyGlobalImpulse(Vector2 impulse)
        {
            for (int i = 0; i < SegmentCount; i++)
            {
                ApplyImpulse(i, impulse);
            }
        }
        
        /// <summary>
        /// Applies an explosion force from a point.
        /// Points closer to the explosion receive more force.
        /// </summary>
        public void ApplyExplosionForce(Vector2 explosionCenter, float force, float radius)
        {
            for (int i = 0; i < SegmentCount; i++)
            {
                Vector2 toPoint = _currentPositions[i] - explosionCenter;
                float distance = toPoint.Length();
                
                if (distance < radius && distance > 0.001f)
                {
                    float falloff = 1f - (distance / radius);
                    Vector2 impulse = toPoint.SafeNormalize(Vector2.Zero) * force * falloff;
                    ApplyImpulse(i, impulse);
                }
            }
        }
        
        #endregion
        
        #region Collision
        
        /// <summary>
        /// Simple tile collision - keeps points out of solid tiles.
        /// Call after Update() if collision is needed.
        /// </summary>
        public void ApplyTileCollision()
        {
            for (int i = 0; i < SegmentCount; i++)
            {
                // Skip anchored points
                if (i == 0 && IsStartAnchored) continue;
                if (i == SegmentCount - 1 && IsEndAnchored) continue;
                
                Vector2 pos = _currentPositions[i];
                Point tilePos = pos.ToTileCoordinates();
                
                if (WorldGen.InWorld(tilePos.X, tilePos.Y))
                {
                    Tile tile = Framing.GetTileSafely(tilePos.X, tilePos.Y);
                    if (tile.HasUnactuatedTile && Main.tileSolid[tile.TileType])
                    {
                        // Push out of tile
                        Vector2 tileCenter = tilePos.ToVector2() * 16f + new Vector2(8f);
                        Vector2 pushDir = (pos - tileCenter).SafeNormalize(Vector2.UnitY);
                        _currentPositions[i] = tileCenter + pushDir * 12f;
                    }
                }
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// Manages multiple Verlet chains for complex physics systems.
    /// </summary>
    public static class VerletChainManager
    {
        private static List<VerletChain> _activeChains = new List<VerletChain>();
        
        /// <summary>
        /// Creates a whip-style chain (anchored at start, free at end).
        /// </summary>
        public static VerletChain CreateWhipChain(Vector2 startPos, int segments = 15, float segmentLength = 10f)
        {
            var chain = new VerletChain(segments, segmentLength, startPos, new Vector2(1, 0.5f));
            chain.Damping = 0.95f;
            chain.Gravity = new Vector2(0, 0.25f);
            chain.ConstraintIterations = 6;
            chain.IsStartAnchored = true;
            chain.IsEndAnchored = false;
            return chain;
        }
        
        /// <summary>
        /// Creates a tentacle chain (high constraint iterations for organic feel).
        /// </summary>
        public static VerletChain CreateTentacleChain(Vector2 startPos, int segments = 20, float segmentLength = 8f)
        {
            var chain = new VerletChain(segments, segmentLength, startPos, new Vector2(0, 1));
            chain.Damping = 0.92f;
            chain.Gravity = new Vector2(0, 0.15f);
            chain.ConstraintIterations = 10;
            chain.Stiffness = 0.95f;
            return chain;
        }
        
        /// <summary>
        /// Creates a taut rope (both ends anchored).
        /// </summary>
        public static VerletChain CreateRope(Vector2 startPos, Vector2 endPos, int segments = 12)
        {
            float distance = Vector2.Distance(startPos, endPos);
            float segmentLength = distance / (segments - 1) * 1.05f; // Slightly slack
            
            var chain = new VerletChain(segments, segmentLength);
            chain.InitializeLine(startPos, endPos);
            chain.Damping = 0.97f;
            chain.Gravity = new Vector2(0, 0.4f);
            chain.ConstraintIterations = 8;
            chain.IsStartAnchored = true;
            chain.IsEndAnchored = true;
            return chain;
        }
        
        /// <summary>
        /// Creates a chain for visual lightning arcs.
        /// Uses lower gravity and higher stiffness for electric feel.
        /// </summary>
        public static VerletChain CreateLightningChain(Vector2 startPos, Vector2 endPos, int segments = 10)
        {
            float distance = Vector2.Distance(startPos, endPos);
            float segmentLength = distance / (segments - 1);
            
            var chain = new VerletChain(segments, segmentLength);
            chain.InitializeLine(startPos, endPos);
            chain.Damping = 0.99f;
            chain.Gravity = Vector2.Zero;
            chain.ConstraintIterations = 3;
            chain.Stiffness = 0.8f;
            chain.IsStartAnchored = true;
            chain.IsEndAnchored = true;
            return chain;
        }
    }
    
    /// <summary>
    /// Extension methods for drawing Verlet chains.
    /// </summary>
    public static class VerletChainDrawing
    {
        /// <summary>
        /// Generates trail positions from a Verlet chain for use with EnhancedTrailRenderer.
        /// </summary>
        public static Vector2[] ToTrailPositions(this VerletChain chain)
        {
            return chain.GetPositions();
        }
        
        /// <summary>
        /// Calculates rotation angles for each segment (for oriented sprites).
        /// </summary>
        public static float[] GetSegmentRotations(this VerletChain chain)
        {
            Vector2[] positions = chain.GetPositions();
            float[] rotations = new float[positions.Length];
            
            for (int i = 0; i < positions.Length; i++)
            {
                Vector2 direction;
                if (i == 0)
                    direction = positions[1] - positions[0];
                else if (i == positions.Length - 1)
                    direction = positions[i] - positions[i - 1];
                else
                    direction = positions[i + 1] - positions[i - 1];
                    
                rotations[i] = direction.ToRotation();
            }
            
            return rotations;
        }
        
        /// <summary>
        /// Draws the chain as simple lines for debugging.
        /// </summary>
        public static void DrawDebug(this VerletChain chain, SpriteBatch spriteBatch, Color color)
        {
            Vector2[] positions = chain.GetPositions();
            Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;
            
            for (int i = 0; i < positions.Length - 1; i++)
            {
                Vector2 start = positions[i] - Main.screenPosition;
                Vector2 end = positions[i + 1] - Main.screenPosition;
                Vector2 edge = end - start;
                float angle = edge.ToRotation();
                float length = edge.Length();
                
                spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), color, 
                    angle, Vector2.Zero, new Vector2(length, 2f), SpriteEffects.None, 0f);
            }
        }
    }
}
