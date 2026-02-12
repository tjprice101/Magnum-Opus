using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;

namespace MagnumOpus.Common.Systems.VFX.Core
{
    /// <summary>
    /// Frustum (screen bounds) culling utilities for VFX optimization.
    /// Prevents rendering and updating off-screen effects.
    /// 
    /// USAGE:
    /// if (FrustumCuller.IsPointVisible(position))
    ///     DrawEffect(position);
    /// 
    /// if (FrustumCuller.IsLineVisible(start, end))
    ///     DrawBeam(start, end);
    /// </summary>
    public static class FrustumCuller
    {
        #region Screen Bounds
        
        /// <summary>
        /// Get current screen bounds in world coordinates.
        /// </summary>
        public static Rectangle GetScreenBounds()
        {
            return new Rectangle(
                (int)Main.screenPosition.X,
                (int)Main.screenPosition.Y,
                Main.screenWidth,
                Main.screenHeight
            );
        }
        
        /// <summary>
        /// Get expanded screen bounds with buffer zone.
        /// </summary>
        /// <param name="buffer">Extra pixels around screen</param>
        public static Rectangle GetExpandedScreenBounds(float buffer = 100f)
        {
            return new Rectangle(
                (int)(Main.screenPosition.X - buffer),
                (int)(Main.screenPosition.Y - buffer),
                (int)(Main.screenWidth + buffer * 2),
                (int)(Main.screenHeight + buffer * 2)
            );
        }
        
        #endregion
        
        #region Point Culling
        
        /// <summary>
        /// Check if a world position is visible on screen.
        /// </summary>
        public static bool IsPointVisible(Vector2 worldPosition)
        {
            Rectangle screen = GetScreenBounds();
            return screen.Contains(worldPosition.ToPoint());
        }
        
        /// <summary>
        /// Check if a world position is visible with buffer zone.
        /// </summary>
        public static bool IsPointVisible(Vector2 worldPosition, float buffer)
        {
            Rectangle screen = GetExpandedScreenBounds(buffer);
            return screen.Contains(worldPosition.ToPoint());
        }
        
        #endregion
        
        #region Rectangle Culling
        
        /// <summary>
        /// Check if a rectangle intersects the screen.
        /// </summary>
        public static bool IsRectangleVisible(Rectangle worldBounds)
        {
            Rectangle screen = GetScreenBounds();
            return screen.Intersects(worldBounds);
        }
        
        /// <summary>
        /// Check if a rectangle intersects the screen with buffer.
        /// </summary>
        public static bool IsRectangleVisible(Rectangle worldBounds, float buffer)
        {
            Rectangle screen = GetExpandedScreenBounds(buffer);
            return screen.Intersects(worldBounds);
        }
        
        #endregion
        
        #region Circle Culling
        
        /// <summary>
        /// Check if a circle is visible on screen.
        /// </summary>
        public static bool IsCircleVisible(Vector2 center, float radius)
        {
            Rectangle screen = GetScreenBounds();
            
            // Expand screen bounds by radius for conservative test
            Rectangle expandedScreen = new Rectangle(
                screen.X - (int)radius,
                screen.Y - (int)radius,
                screen.Width + (int)(radius * 2),
                screen.Height + (int)(radius * 2)
            );
            
            return expandedScreen.Contains(center.ToPoint());
        }
        
        #endregion
        
        #region Line Culling
        
        /// <summary>
        /// Check if a line segment is visible on screen.
        /// Uses Cohen-Sutherland-style quick rejection then full test.
        /// </summary>
        public static bool IsLineVisible(Vector2 start, Vector2 end)
        {
            Rectangle screen = GetScreenBounds();
            
            // Quick reject: both endpoints outside same edge
            if (start.X < screen.Left && end.X < screen.Left) return false;
            if (start.X > screen.Right && end.X > screen.Right) return false;
            if (start.Y < screen.Top && end.Y < screen.Top) return false;
            if (start.Y > screen.Bottom && end.Y > screen.Bottom) return false;
            
            // One or both endpoints visible
            if (screen.Contains(start.ToPoint()) || screen.Contains(end.ToPoint()))
                return true;
            
            // Line might cross screen - do proper line-rect intersection
            return LineIntersectsRect(start, end, screen);
        }
        
        /// <summary>
        /// Check if a line segment is visible with buffer zone.
        /// </summary>
        public static bool IsLineVisible(Vector2 start, Vector2 end, float buffer)
        {
            Rectangle screen = GetExpandedScreenBounds(buffer);
            
            if (start.X < screen.Left && end.X < screen.Left) return false;
            if (start.X > screen.Right && end.X > screen.Right) return false;
            if (start.Y < screen.Top && end.Y < screen.Top) return false;
            if (start.Y > screen.Bottom && end.Y > screen.Bottom) return false;
            
            if (screen.Contains(start.ToPoint()) || screen.Contains(end.ToPoint()))
                return true;
            
            return LineIntersectsRect(start, end, screen);
        }
        
        private static bool LineIntersectsRect(Vector2 start, Vector2 end, Rectangle rect)
        {
            // Check intersection with each edge of rectangle
            Vector2 topLeft = new Vector2(rect.Left, rect.Top);
            Vector2 topRight = new Vector2(rect.Right, rect.Top);
            Vector2 bottomLeft = new Vector2(rect.Left, rect.Bottom);
            Vector2 bottomRight = new Vector2(rect.Right, rect.Bottom);
            
            if (LineSegmentIntersection(start, end, topLeft, topRight)) return true;
            if (LineSegmentIntersection(start, end, topRight, bottomRight)) return true;
            if (LineSegmentIntersection(start, end, bottomRight, bottomLeft)) return true;
            if (LineSegmentIntersection(start, end, bottomLeft, topLeft)) return true;
            
            return false;
        }
        
        private static bool LineSegmentIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
        {
            float d = (p2.X - p1.X) * (p4.Y - p3.Y) - (p2.Y - p1.Y) * (p4.X - p3.X);
            
            if (Math.Abs(d) < 0.0001f)
                return false; // Parallel
            
            float t = ((p3.X - p1.X) * (p4.Y - p3.Y) - (p3.Y - p1.Y) * (p4.X - p3.X)) / d;
            float u = ((p3.X - p1.X) * (p2.Y - p1.Y) - (p3.Y - p1.Y) * (p2.X - p1.X)) / d;
            
            return t >= 0 && t <= 1 && u >= 0 && u <= 1;
        }
        
        #endregion
        
        #region Beam Culling
        
        /// <summary>
        /// Get bounding rectangle for a beam.
        /// </summary>
        public static Rectangle GetBeamBounds(Vector2 origin, Vector2 direction, float length, float width)
        {
            Vector2 end = origin + direction * length;
            
            float minX = Math.Min(origin.X, end.X);
            float minY = Math.Min(origin.Y, end.Y);
            float maxX = Math.Max(origin.X, end.X);
            float maxY = Math.Max(origin.Y, end.Y);
            
            // Add width padding
            float padding = width * 0.5f + 10f;
            
            return new Rectangle(
                (int)(minX - padding),
                (int)(minY - padding),
                (int)(maxX - minX + padding * 2),
                (int)(maxY - minY + padding * 2)
            );
        }
        
        /// <summary>
        /// Check if a beam is visible on screen.
        /// </summary>
        public static bool IsBeamVisible(Vector2 origin, Vector2 direction, float length, float width)
        {
            Vector2 end = origin + direction * length;
            
            // First do quick line test
            if (!IsLineVisible(origin, end, width * 0.5f))
                return false;
            
            return true;
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Get distance from point to nearest screen edge.
        /// Negative if inside screen.
        /// </summary>
        public static float DistanceToScreenEdge(Vector2 worldPosition)
        {
            Rectangle screen = GetScreenBounds();
            
            float distLeft = worldPosition.X - screen.Left;
            float distRight = screen.Right - worldPosition.X;
            float distTop = worldPosition.Y - screen.Top;
            float distBottom = screen.Bottom - worldPosition.Y;
            
            // If inside, return negative of minimum distance to edge
            if (distLeft > 0 && distRight > 0 && distTop > 0 && distBottom > 0)
            {
                return -Math.Min(Math.Min(distLeft, distRight), Math.Min(distTop, distBottom));
            }
            
            // If outside, calculate actual distance
            float dx = 0;
            float dy = 0;
            
            if (worldPosition.X < screen.Left) dx = screen.Left - worldPosition.X;
            else if (worldPosition.X > screen.Right) dx = worldPosition.X - screen.Right;
            
            if (worldPosition.Y < screen.Top) dy = screen.Top - worldPosition.Y;
            else if (worldPosition.Y > screen.Bottom) dy = worldPosition.Y - screen.Bottom;
            
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }
        
        /// <summary>
        /// Get normalized position on screen (0,0 = top-left, 1,1 = bottom-right).
        /// Values outside 0-1 are off-screen.
        /// </summary>
        public static Vector2 GetNormalizedScreenPosition(Vector2 worldPosition)
        {
            Vector2 screenPos = worldPosition - Main.screenPosition;
            return new Vector2(
                screenPos.X / Main.screenWidth,
                screenPos.Y / Main.screenHeight
            );
        }
        
        #endregion
    }
    
    #region Culled Beam Renderer
    
    /// <summary>
    /// Beam renderer with automatic frustum culling.
    /// Only updates and renders beams that are visible on screen.
    /// </summary>
    public class CulledBeamRenderer
    {
        #region Cullable Beam
        
        public class CullableBeam
        {
            public Vector2 Origin;
            public Vector2 Direction;
            public float Length;
            public float Width;
            public Color Color;
            public float Intensity;
            
            public bool IsVisible;
            public bool WasVisible;
            public Rectangle Bounds;
            
            public void UpdateBounds()
            {
                Bounds = FrustumCuller.GetBeamBounds(Origin, Direction, Length, Width);
            }
            
            public Vector2 EndPoint => Origin + Direction * Length;
        }
        
        #endregion
        
        #region Fields
        
        private List<CullableBeam> beams;
        
        #endregion
        
        #region Statistics
        
        public int TotalBeams => beams.Count;
        public int VisibleBeams { get; private set; }
        public int CulledBeams => TotalBeams - VisibleBeams;
        public float CullRatio => TotalBeams > 0 ? (float)CulledBeams / TotalBeams : 0f;
        
        #endregion
        
        #region Constructor
        
        public CulledBeamRenderer()
        {
            beams = new List<CullableBeam>();
        }
        
        #endregion
        
        #region Beam Management
        
        public CullableBeam AddBeam(Vector2 origin, Vector2 direction, float length, float width, Color color)
        {
            var beam = new CullableBeam
            {
                Origin = origin,
                Direction = Vector2.Normalize(direction),
                Length = length,
                Width = width,
                Color = color,
                Intensity = 1f,
                IsVisible = true,
                WasVisible = true
            };
            
            beams.Add(beam);
            return beam;
        }
        
        public bool RemoveBeam(CullableBeam beam)
        {
            return beams.Remove(beam);
        }
        
        public void Clear()
        {
            beams.Clear();
            VisibleBeams = 0;
        }
        
        #endregion
        
        #region Culling
        
        /// <summary>
        /// Update culling status for all beams.
        /// Call once per frame before Update/Draw.
        /// </summary>
        public void UpdateCulling()
        {
            VisibleBeams = 0;
            
            foreach (var beam in beams)
            {
                beam.WasVisible = beam.IsVisible;
                beam.UpdateBounds();
                
                beam.IsVisible = FrustumCuller.IsRectangleVisible(beam.Bounds);
                
                if (beam.IsVisible)
                    VisibleBeams++;
            }
        }
        
        #endregion
        
        #region Update & Draw
        
        /// <summary>
        /// Update all beams (only full update for visible beams).
        /// </summary>
        public void Update(Action<CullableBeam> fullUpdate, Action<CullableBeam> minimalUpdate = null)
        {
            foreach (var beam in beams)
            {
                if (beam.IsVisible)
                {
                    fullUpdate?.Invoke(beam);
                }
                else
                {
                    minimalUpdate?.Invoke(beam);
                }
            }
        }
        
        /// <summary>
        /// Draw only visible beams.
        /// </summary>
        public void Draw(Action<CullableBeam> drawBeam)
        {
            foreach (var beam in beams)
            {
                if (beam.IsVisible)
                {
                    drawBeam?.Invoke(beam);
                }
            }
        }
        
        #endregion
        
        #region Statistics
        
        public string GetCullingStats()
        {
            return $"Beams: Visible={VisibleBeams}/{TotalBeams} ({CulledBeams} culled, {CullRatio:P0})";
        }
        
        #endregion
    }
    
    #endregion
    
    #region Hierarchical Culler
    
    /// <summary>
    /// Spatial grid-based hierarchical culling for many objects.
    /// First culls grid cells, then only tests objects in visible cells.
    /// </summary>
    public class HierarchicalCuller<T> where T : class
    {
        #region Spatial Cell
        
        private class SpatialCell
        {
            public Rectangle Bounds;
            public List<T> Objects;
            public bool IsVisible;
            
            public SpatialCell(Rectangle bounds)
            {
                Bounds = bounds;
                Objects = new List<T>();
            }
        }
        
        #endregion
        
        #region Fields
        
        private SpatialCell[,] grid;
        private int cellSize;
        private int gridWidth, gridHeight;
        private Func<T, Rectangle> getBounds;
        
        #endregion
        
        #region Constructor
        
        /// <summary>
        /// Create hierarchical culler.
        /// </summary>
        /// <param name="worldWidth">World width in pixels</param>
        /// <param name="worldHeight">World height in pixels</param>
        /// <param name="cellSize">Grid cell size in pixels</param>
        /// <param name="getBounds">Function to get bounds of an object</param>
        public HierarchicalCuller(int worldWidth, int worldHeight, int cellSize, Func<T, Rectangle> getBounds)
        {
            this.cellSize = cellSize;
            this.getBounds = getBounds;
            
            gridWidth = (worldWidth / cellSize) + 1;
            gridHeight = (worldHeight / cellSize) + 1;
            
            grid = new SpatialCell[gridWidth, gridHeight];
            
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    grid[x, y] = new SpatialCell(new Rectangle(
                        x * cellSize,
                        y * cellSize,
                        cellSize,
                        cellSize
                    ));
                }
            }
        }
        
        #endregion
        
        #region Registration
        
        /// <summary>
        /// Register an object in the spatial grid.
        /// </summary>
        public void Register(T obj)
        {
            Rectangle bounds = getBounds(obj);
            
            int minX = Math.Max(0, Math.Min(bounds.Left / cellSize, gridWidth - 1));
            int minY = Math.Max(0, Math.Min(bounds.Top / cellSize, gridHeight - 1));
            int maxX = Math.Max(0, Math.Min(bounds.Right / cellSize, gridWidth - 1));
            int maxY = Math.Max(0, Math.Min(bounds.Bottom / cellSize, gridHeight - 1));
            
            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    if (!grid[x, y].Objects.Contains(obj))
                        grid[x, y].Objects.Add(obj);
                }
            }
        }
        
        /// <summary>
        /// Unregister an object from all cells.
        /// </summary>
        public void Unregister(T obj)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    grid[x, y].Objects.Remove(obj);
                }
            }
        }
        
        /// <summary>
        /// Clear all objects from the grid.
        /// </summary>
        public void Clear()
        {
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    grid[x, y].Objects.Clear();
                }
            }
        }
        
        #endregion
        
        #region Culling
        
        /// <summary>
        /// Get all visible objects.
        /// </summary>
        public List<T> GetVisibleObjects()
        {
            Rectangle screenBounds = FrustumCuller.GetScreenBounds();
            HashSet<T> visible = new HashSet<T>();
            
            // First pass: Mark visible cells
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    var cell = grid[x, y];
                    cell.IsVisible = screenBounds.Intersects(cell.Bounds);
                }
            }
            
            // Second pass: Collect objects from visible cells
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    var cell = grid[x, y];
                    
                    if (!cell.IsVisible)
                        continue;
                    
                    foreach (var obj in cell.Objects)
                    {
                        if (!visible.Contains(obj))
                        {
                            // Fine-grained test
                            Rectangle objBounds = getBounds(obj);
                            if (screenBounds.Intersects(objBounds))
                            {
                                visible.Add(obj);
                            }
                        }
                    }
                }
            }
            
            return new List<T>(visible);
        }
        
        #endregion
    }
    
    #endregion
}
