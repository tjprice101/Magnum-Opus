# VFX Core Concepts - Part 2: Advanced Techniques

> **COMPREHENSIVE DEEP DIVE INTO ADVANCED VFX SYSTEMS**
>
> This document covers bezier curves, particle architecture, billboarding, and performance optimization.
>
> ---
>
> ## 📖 Related Documentation
>
> | Document | Purpose |
> |----------|---------|
> | **[VFX_MASTERY_RESEARCH_COMPLETE.md](VFX_MASTERY_RESEARCH_COMPLETE.md)** | Part 1: MonoGame API, BlendStates, primitive trails, bloom stacking |
> | **[HLSL_GRAPHICS_DEEP_DIVE.md](HLSL_GRAPHICS_DEEP_DIVE.md)** | HLSL language reference, noise, SDFs, color grading |
> | **[Enhanced_VFX_System.md](Guides/Enhanced_VFX_System.md)** | MagnumOpus VFX API usage guide |

---

## Table of Contents

1. [Bezier Curves for Beam Paths](#1-bezier-curves-for-beam-paths)
2. [Particle System Architecture](#2-particle-system-architecture)
3. [Camera-Space Billboarding](#3-camera-space-billboarding)
4. [Performance Profiling & GC Avoidance](#4-performance-profiling--gc-avoidance)

---

## 1. Bezier Curves for Beam Paths

### 1.1 Theory

**Bezier Curve Types:**

| Order | Name | Control Points | Use Case |
|-------|------|----------------|----------|
| 1st | Linear | 2 points | Straight lines |
| 2nd | Quadratic | 3 points | Simple arcs, basic homing |
| 3rd | Cubic | 4 points | Complex curves, smooth paths |

### 1.2 Parametric Form (t ∈ [0, 1])

**Quadratic Bezier:**
$$B(t) = (1-t)^2 P_0 + 2(1-t)t P_1 + t^2 P_2$$

Where:
- $P_0$ = start point
- $P_1$ = control point
- $P_2$ = end point

**Cubic Bezier:**
$$B(t) = (1-t)^3 P_0 + 3(1-t)^2 t P_1 + 3(1-t)t^2 P_2 + t^3 P_3$$

Where:
- $P_0$ = start point
- $P_1, P_2$ = control points
- $P_3$ = end point

### 1.3 C# Implementation

```csharp
public static class BezierCurve
{
    /// <summary>
    /// Quadratic Bezier curve evaluation (3 control points).
    /// </summary>
    public static Vector2 QuadraticBezier(Vector2 p0, Vector2 p1, Vector2 p2, float t)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        
        Vector2 point = uu * p0;           // (1-t)² * P0
        point += 2 * u * t * p1;           // 2(1-t)t * P1
        point += tt * p2;                  // t² * P2
        
        return point;
    }
    
    /// <summary>
    /// Cubic Bezier curve evaluation (4 control points).
    /// </summary>
    public static Vector2 CubicBezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;
        
        Vector2 point = uuu * p0;          // (1-t)³ * P0
        point += 3 * uu * t * p1;          // 3(1-t)²t * P1
        point += 3 * u * tt * p2;          // 3(1-t)t² * P2
        point += ttt * p3;                 // t³ * P3
        
        return point;
    }
    
    /// <summary>
    /// Derivative (tangent direction) for cubic Bezier.
    /// Essential for orienting projectiles along the curve.
    /// </summary>
    public static Vector2 CubicBezierDerivative(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        
        Vector2 tangent = -3 * uu * p0;
        tangent += 3 * uu * p1 - 6 * u * t * p1;
        tangent += 6 * u * t * p2 - 3 * tt * p2;
        tangent += 3 * tt * p3;
        
        return tangent;
    }
    
    /// <summary>
    /// Generate evenly-spaced points along the curve.
    /// </summary>
    public static List<Vector2> GeneratePoints(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, int segments)
    {
        List<Vector2> points = new List<Vector2>(segments + 1);
        
        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            points.Add(CubicBezier(p0, p1, p2, p3, t));
        }
        
        return points;
    }
}
```

### 1.4 Adaptive Subdivision (Quality Optimization)

Subdivide curve based on curvature - more points where curve is tight:

```csharp
/// <summary>
/// Adaptively subdivide curve based on deviation from straight line.
/// Produces more points in curved sections, fewer in straight sections.
/// </summary>
public static List<Vector2> AdaptiveSubdivision(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, 
                                                  float tolerance = 0.5f)
{
    List<Vector2> points = new List<Vector2>();
    SubdivideRecursive(p0, p1, p2, p3, 0, 1, tolerance, points);
    return points;
}

private static void SubdivideRecursive(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3,
                                        float t0, float t1, float tolerance, List<Vector2> points)
{
    // Calculate midpoint on curve
    float tMid = (t0 + t1) * 0.5f;
    Vector2 mid = CubicBezier(p0, p1, p2, p3, tMid);
    
    // Calculate linear interpolation between endpoints
    Vector2 start = CubicBezier(p0, p1, p2, p3, t0);
    Vector2 end = CubicBezier(p0, p1, p2, p3, t1);
    Vector2 linearMid = (start + end) * 0.5f;
    
    // Check if curve deviates significantly from straight line
    float deviation = Vector2.Distance(mid, linearMid);
    
    if (deviation < tolerance)
    {
        // Curve is flat enough - add points
        if (points.Count == 0 || points[points.Count - 1] != start)
            points.Add(start);
        points.Add(end);
    }
    else
    {
        // Subdivide further
        SubdivideRecursive(p0, p1, p2, p3, t0, tMid, tolerance, points);
        SubdivideRecursive(p0, p1, p2, p3, tMid, t1, tolerance, points);
    }
}
```

### 1.5 Practical Applications

#### Homing Projectile with Curved Path

```csharp
public class HomingBeam : ModProjectile
{
    private Vector2 startPoint;
    private Vector2 controlPoint1;
    private Vector2 controlPoint2;
    private Vector2 targetPoint;
    private List<Vector2> pathPoints;
    
    public override void AI()
    {
        if (Projectile.ai[0] == 0) // First frame - setup curve
        {
            startPoint = Projectile.Center;
            NPC target = FindNearestEnemy();
            if (target == null) return;
            
            targetPoint = target.Center;
            
            // Create control points for curved path
            Vector2 toTarget = targetPoint - startPoint;
            Vector2 perpendicular = new Vector2(-toTarget.Y, toTarget.X);
            perpendicular.Normalize();
            
            // Offset control points to create arc
            float arcDirection = Main.rand.NextBool() ? 1f : -1f;
            controlPoint1 = startPoint + toTarget * 0.33f + perpendicular * 100f * arcDirection;
            controlPoint2 = startPoint + toTarget * 0.67f + perpendicular * 50f * arcDirection;
            
            // Generate path points for trail rendering
            pathPoints = BezierCurve.GeneratePoints(startPoint, controlPoint1, controlPoint2, targetPoint, 20);
        }
        
        // Move projectile along curve
        float duration = 60f; // 60 frames to complete
        float progress = Projectile.ai[0] / duration;
        
        if (progress < 1f)
        {
            // Smooth easing for acceleration/deceleration
            float easedProgress = EaseInOutQuad(progress);
            
            // Position on curve
            Projectile.Center = BezierCurve.CubicBezier(
                startPoint, controlPoint1, controlPoint2, targetPoint, easedProgress);
            
            // Rotation from derivative (tangent)
            Vector2 tangent = BezierCurve.CubicBezierDerivative(
                startPoint, controlPoint1, controlPoint2, targetPoint, easedProgress);
            Projectile.rotation = tangent.ToRotation();
            
            Projectile.ai[0]++;
        }
        else
        {
            Projectile.Kill();
        }
    }
    
    private float EaseInOutQuad(float t)
    {
        return t < 0.5f ? 2f * t * t : 1f - MathF.Pow(-2f * t + 2f, 2f) / 2f;
    }
    
    public override bool PreDraw(ref Color lightColor)
    {
        if (pathPoints == null || pathPoints.Count < 2) return false;
        
        // Draw beam along bezier path using trail renderer
        DrawBeamAlongPath(pathPoints);
        return false;
    }
}
```

### 1.6 Catmull-Rom Splines (Alternative)

**Key Difference:** Catmull-Rom passes through ALL control points (Bezier only passes through endpoints).

```csharp
/// <summary>
/// Catmull-Rom spline - curve passes through all 4 control points.
/// Use when you need the curve to hit specific waypoints.
/// </summary>
public static Vector2 CatmullRom(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
{
    float tt = t * t;
    float ttt = tt * t;
    
    // Catmull-Rom matrix multiplication
    Vector2 result = 0.5f * (
        (2f * p1) +
        (-p0 + p2) * t +
        (2f * p0 - 5f * p1 + 4f * p2 - p3) * tt +
        (-p0 + 3f * p1 - 3f * p2 + p3) * ttt
    );
    
    return result;
}

/// <summary>
/// Create smooth path through a list of waypoints.
/// </summary>
public static List<Vector2> SmoothPath(List<Vector2> waypoints, int subdivisions)
{
    if (waypoints.Count < 2) return waypoints;
    
    List<Vector2> smoothPath = new List<Vector2>();
    
    for (int i = 0; i < waypoints.Count - 1; i++)
    {
        // Get 4 control points (clamp at boundaries)
        Vector2 p0 = i > 0 ? waypoints[i - 1] : waypoints[i];
        Vector2 p1 = waypoints[i];
        Vector2 p2 = waypoints[i + 1];
        Vector2 p3 = i < waypoints.Count - 2 ? waypoints[i + 2] : waypoints[i + 1];
        
        for (int j = 0; j < subdivisions; j++)
        {
            float t = j / (float)subdivisions;
            smoothPath.Add(CatmullRom(p0, p1, p2, p3, t));
        }
    }
    
    smoothPath.Add(waypoints[waypoints.Count - 1]);
    return smoothPath;
}
```

### 1.7 When to Use Which Curve

| Curve Type | Use When |
|------------|----------|
| **Quadratic Bezier** | Simple arcs, basic homing, performance-critical |
| **Cubic Bezier** | Complex S-curves, precise control over shape |
| **Catmull-Rom** | Path must pass through specific waypoints |
| **Adaptive Subdivision** | Need variable detail based on curvature |

### 1.8 Resources

- **"A Primer on Bezier Curves"** by Pomax: https://pomax.github.io/bezierinfo/
- **Freya Holmér's Math for Game Devs**: https://www.youtube.com/c/Acegikmo
- **"Cubic Bezier: From Math to Motion"**: https://blog.maximeheckel.com/posts/cubic-bezier-from-math-to-motion/

---

## 2. Particle System Architecture

### 2.1 Design Goals

| Goal | Strategy |
|------|----------|
| **Performance** | Handle 1000+ particles at 60 FPS |
| **Memory** | Minimize GC allocations |
| **Flexibility** | Support gravity, collision, fading |
| **Quality** | Smooth interpolation, proper blending |

### 2.2 Object Pooling Concept

**Problem:** Creating/destroying particles causes GC spikes.

**Solution:** Pre-allocate particles and reuse "dead" ones.

```
┌─────────────────────────────────────────────────────────┐
│                    PARTICLE POOL                        │
├─────────────────────────────────────────────────────────┤
│  [Active] [Active] [Dead] [Active] [Dead] [Dead] ...   │
│     ↑        ↑       ↑                                  │
│   Drawing  Drawing  Reuse                               │
│                      when                               │
│                     spawning                            │
└─────────────────────────────────────────────────────────┘
```

### 2.3 Particle Structure (Use STRUCT, not class!)

```csharp
/// <summary>
/// Particle as struct for cache-friendly, allocation-free storage.
/// All particles stored inline in array (single allocation).
/// </summary>
public struct Particle
{
    // === STATE ===
    public bool Active;
    
    // === TRANSFORM ===
    public Vector2 Position;
    public Vector2 Velocity;
    public float Rotation;
    public float RotationSpeed;
    public Vector2 Scale;
    
    // === VISUAL ===
    public Color Color;
    public float Alpha;
    public Rectangle? SourceRect; // For sprite sheets
    
    // === LIFECYCLE ===
    public float LifeTime;        // Total lifetime in frames
    public float Age;             // Current age
    
    // === PHYSICS ===
    public Vector2 Acceleration;  // e.g., gravity
    public float Drag;            // Air resistance (0-1)
    
    // === ANIMATION ===
    public float FadeInTime;      // Fade in duration
    public float FadeOutTime;     // Fade out duration
    public AnimationCurve ScaleCurve;
    
    // === COLLISION (optional) ===
    public bool CollidesWithTiles;
    public float Bounciness;
}

public enum AnimationCurve
{
    Linear,
    EaseIn,
    EaseOut,
    EaseInOut
}
```

### 2.4 Particle Pool Manager

```csharp
public class ParticlePool
{
    private Particle[] particles;
    private int maxParticles;
    private int activeCount;
    
    public Texture2D Texture { get; set; }
    
    public ParticlePool(int maxParticles)
    {
        this.maxParticles = maxParticles;
        particles = new Particle[maxParticles]; // SINGLE ALLOCATION
        
        // Pre-initialize all as inactive
        for (int i = 0; i < maxParticles; i++)
        {
            particles[i].Active = false;
        }
    }
    
    /// <summary>
    /// Emit a single particle. Returns false if pool is exhausted.
    /// </summary>
    public bool EmitParticle(Vector2 position, Vector2 velocity, ParticleSettings settings)
    {
        // Find inactive particle slot
        for (int i = 0; i < maxParticles; i++)
        {
            if (!particles[i].Active)
            {
                InitializeParticle(ref particles[i], position, velocity, settings);
                activeCount++;
                return true;
            }
        }
        
        return false; // Pool exhausted - consider expanding or limiting emitters
    }
    
    /// <summary>
    /// Emit burst of particles in random directions.
    /// </summary>
    public void EmitBurst(Vector2 position, int count, ParticleSettings settings)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = Main.rand.NextFloat(MathHelper.TwoPi);
            float speed = Main.rand.NextFloat(settings.MinSpeed, settings.MaxSpeed);
            Vector2 velocity = angle.ToRotationVector2() * speed;
            
            EmitParticle(position, velocity, settings);
        }
    }
    
    private void InitializeParticle(ref Particle p, Vector2 position, Vector2 velocity, 
                                     ParticleSettings settings)
    {
        p.Active = true;
        p.Position = position;
        p.Velocity = velocity;
        p.Rotation = settings.RandomRotation ? Main.rand.NextFloat(MathHelper.TwoPi) : 0f;
        p.RotationSpeed = Main.rand.NextFloat(settings.MinRotationSpeed, settings.MaxRotationSpeed);
        p.Scale = Vector2.One * Main.rand.NextFloat(settings.MinScale, settings.MaxScale);
        p.Color = settings.Color;
        p.Alpha = 1f;
        p.LifeTime = Main.rand.NextFloat(settings.MinLifetime, settings.MaxLifetime);
        p.Age = 0f;
        p.Acceleration = settings.Gravity;
        p.Drag = settings.Drag;
        p.FadeInTime = settings.FadeInTime;
        p.FadeOutTime = settings.FadeOutTime;
        p.CollidesWithTiles = settings.CollideWithTiles;
        p.Bounciness = settings.Bounciness;
    }
    
    /// <summary>
    /// Update all active particles. Call once per frame.
    /// </summary>
    public void Update()
    {
        for (int i = 0; i < maxParticles; i++)
        {
            if (!particles[i].Active)
                continue;
            
            ref Particle p = ref particles[i]; // Use REF to modify in-place
            
            // Age check
            p.Age++;
            if (p.Age >= p.LifeTime)
            {
                p.Active = false;
                activeCount--;
                continue;
            }
            
            // Physics integration
            p.Velocity += p.Acceleration;
            p.Velocity *= (1f - p.Drag);
            p.Position += p.Velocity;
            p.Rotation += p.RotationSpeed;
            
            // Tile collision (optional)
            if (p.CollidesWithTiles)
            {
                Point tileCoords = p.Position.ToTileCoordinates();
                if (WorldGen.SolidTile(tileCoords.X, tileCoords.Y))
                {
                    p.Velocity.Y *= -p.Bounciness;
                    p.Velocity.X *= 0.9f; // Friction
                }
            }
            
            // Alpha animation (fade in/out)
            float lifeProgress = p.Age / p.LifeTime;
            
            if (p.Age < p.FadeInTime)
            {
                p.Alpha = p.Age / p.FadeInTime;
            }
            else if (p.LifeTime - p.Age < p.FadeOutTime)
            {
                p.Alpha = (p.LifeTime - p.Age) / p.FadeOutTime;
            }
            else
            {
                p.Alpha = 1f;
            }
            
            // Scale animation (grow then shrink)
            float scaleFactor = MathF.Sin(lifeProgress * MathF.PI); // 0→1→0
            p.Scale = Vector2.One * scaleFactor * 2f;
        }
    }
    
    /// <summary>
    /// Draw all active particles. Uses additive blending for glow.
    /// </summary>
    public void Draw(SpriteBatch spriteBatch)
    {
        if (Texture == null) return;
        
        spriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.Additive,
            SamplerState.LinearClamp,
            DepthStencilState.None,
            RasterizerState.CullNone
        );
        
        Vector2 origin = Texture.Bounds.Size.ToVector2() * 0.5f;
        
        for (int i = 0; i < maxParticles; i++)
        {
            if (!particles[i].Active)
                continue;
            
            ref Particle p = ref particles[i];
            
            // Skip off-screen particles
            Vector2 screenPos = p.Position - Main.screenPosition;
            if (screenPos.X < -100 || screenPos.X > Main.screenWidth + 100 ||
                screenPos.Y < -100 || screenPos.Y > Main.screenHeight + 100)
                continue;
            
            Color drawColor = p.Color * p.Alpha;
            
            spriteBatch.Draw(
                Texture,
                screenPos,
                p.SourceRect,
                drawColor,
                p.Rotation,
                origin,
                p.Scale,
                SpriteEffects.None,
                0f
            );
        }
        
        spriteBatch.End();
    }
    
    public int GetActiveCount() => activeCount;
    
    public void Clear()
    {
        for (int i = 0; i < maxParticles; i++)
        {
            particles[i].Active = false;
        }
        activeCount = 0;
    }
}
```

### 2.5 Particle Settings (Builder Pattern)

```csharp
public class ParticleSettings
{
    public Color Color = Color.White;
    public float MinSpeed = 1f;
    public float MaxSpeed = 5f;
    public float MinLifetime = 30f;
    public float MaxLifetime = 60f;
    public float MinScale = 0.5f;
    public float MaxScale = 1.5f;
    public float MinRotationSpeed = -0.1f;
    public float MaxRotationSpeed = 0.1f;
    public bool RandomRotation = true;
    public Vector2 Gravity = new Vector2(0, 0.2f);
    public float Drag = 0.02f;
    public float FadeInTime = 5f;
    public float FadeOutTime = 10f;
    public bool CollideWithTiles = false;
    public float Bounciness = 0.5f;
    
    // === PRESET FACTORIES ===
    
    public static ParticleSettings Explosion() => new ParticleSettings
    {
        Color = Color.Orange,
        MinSpeed = 5f,
        MaxSpeed = 15f,
        MinLifetime = 20f,
        MaxLifetime = 40f,
        Gravity = new Vector2(0, 0.3f),
        FadeOutTime = 15f
    };
    
    public static ParticleSettings Sparkle() => new ParticleSettings
    {
        Color = Color.White,
        MinSpeed = 0.5f,
        MaxSpeed = 2f,
        MinLifetime = 30f,
        MaxLifetime = 60f,
        MinScale = 0.2f,
        MaxScale = 0.8f,
        Gravity = Vector2.Zero,
        Drag = 0.05f
    };
    
    public static ParticleSettings Smoke() => new ParticleSettings
    {
        Color = Color.Gray,
        MinSpeed = 0.5f,
        MaxSpeed = 2f,
        MinLifetime = 60f,
        MaxLifetime = 120f,
        MinScale = 1f,
        MaxScale = 3f,
        Gravity = new Vector2(0, -0.1f), // Rise up
        Drag = 0.03f,
        FadeInTime = 10f,
        FadeOutTime = 30f
    };
    
    public static ParticleSettings Fire() => new ParticleSettings
    {
        Color = new Color(255, 150, 50),
        MinSpeed = 2f,
        MaxSpeed = 6f,
        MinLifetime = 15f,
        MaxLifetime = 30f,
        MinScale = 0.5f,
        MaxScale = 1.2f,
        Gravity = new Vector2(0, -0.15f), // Rise
        Drag = 0.01f,
        FadeOutTime = 10f
    };
}
```

### 2.6 Spatial Partitioning for Collision

For particle-particle or particle-entity collision, use grid-based spatial hashing:

```csharp
/// <summary>
/// Grid-based spatial hash for efficient nearby particle queries.
/// Use when particles need to interact with each other.
/// </summary>
public class ParticleGrid
{
    private Dictionary<Point, List<int>> grid = new Dictionary<Point, List<int>>();
    private int cellSize;
    
    public ParticleGrid(int cellSize = 64)
    {
        this.cellSize = cellSize;
    }
    
    public void Clear()
    {
        foreach (var cell in grid.Values)
            cell.Clear();
    }
    
    private Point GetCell(Vector2 position)
    {
        return new Point(
            (int)(position.X / cellSize),
            (int)(position.Y / cellSize)
        );
    }
    
    public void Insert(int particleIndex, Vector2 position)
    {
        Point cell = GetCell(position);
        if (!grid.ContainsKey(cell))
            grid[cell] = new List<int>();
        grid[cell].Add(particleIndex);
    }
    
    public List<int> GetNearby(Vector2 position)
    {
        List<int> nearby = new List<int>();
        Point center = GetCell(position);
        
        // Check 3x3 grid around particle
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Point cell = new Point(center.X + x, center.Y + y);
                if (grid.ContainsKey(cell))
                    nearby.AddRange(grid[cell]);
            }
        }
        
        return nearby;
    }
}
```

### 2.7 GPU-Accelerated Particles (HLSL Vertex Shader)

For 10,000+ particles, use GPU instancing:

```hlsl
// Vertex shader for instanced particle rendering
float4x4 ViewProjection;
float3 CameraRight;
float3 CameraUp;

struct VertexInput
{
    float3 Position : POSITION0;      // Particle center in world space
    float2 TexCoord : TEXCOORD0;      // Corner offset (-1 to 1)
    float2 Size : TEXCOORD1;          // Particle width/height
    float InstanceRotation : TEXCOORD2;
    float4 InstanceColor : COLOR0;
};

struct VertexOutput
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
    float4 Color : COLOR0;
};

VertexOutput VertexShaderFunction(VertexInput input)
{
    VertexOutput output;
    
    // Apply rotation
    float cosR = cos(input.InstanceRotation);
    float sinR = sin(input.InstanceRotation);
    float2x2 rotation = float2x2(cosR, -sinR, sinR, cosR);
    
    // Billboard: offset in camera space
    float2 rotatedOffset = mul(rotation, input.TexCoord * input.Size);
    float3 worldPos = input.Position;
    worldPos.xy += rotatedOffset;
    
    output.Position = mul(float4(worldPos, 1), ViewProjection);
    output.TexCoord = input.TexCoord * 0.5 + 0.5; // -1..1 → 0..1
    output.Color = input.InstanceColor;
    
    return output;
}
```

### 2.8 Performance Guidelines

| Particle Count | Expected FPS | Strategy |
|----------------|--------------|----------|
| 1,000 | 60 FPS (all GPUs) | Standard CPU pool |
| 5,000 | 60 FPS (mid+ GPU) | CPU pool + culling |
| 10,000+ | 60 FPS (high GPU) | GPU instancing required |

**Optimization Checklist:**
- ✅ Use `struct` for particles (value type, cache-friendly)
- ✅ Pre-allocate particle array (avoid runtime allocation)
- ✅ Use `ref` when updating particles in array
- ✅ Batch draw calls (single SpriteBatch.Begin/End)
- ✅ Cull off-screen particles before drawing
- ✅ Limit max particles per frame (e.g., 2000)

---

## 3. Camera-Space Billboarding

### 3.1 Theory

**Billboarding:** Orient 2D sprite/quad to always face the camera.

**Types:**
| Type | Axis Lock | Use Case |
|------|-----------|----------|
| Cylindrical | Y-axis | Trees, standing objects |
| Spherical | None | Particles, explosions |
| Axis-Aligned | Custom | Beams along direction |

**Why for Beams:**
- Beams are mathematically infinitely thin
- Need visible width perpendicular to view
- Creates convincing 3D effect in 2D game

### 3.2 2D Implementation (Terraria Context)

In 2D, "billboarding" means keeping beam width perpendicular to screen:

```csharp
public static class BeamBillboard
{
    /// <summary>
    /// Draw a beam as a billboarded quad between two points.
    /// </summary>
    public static void DrawBillboardedBeam(
        SpriteBatch spriteBatch,
        Vector2 start,
        Vector2 end,
        float width,
        Texture2D texture,
        Color color)
    {
        // Calculate beam direction
        Vector2 direction = end - start;
        float length = direction.Length();
        if (length < 0.1f) return;
        
        direction.Normalize();
        
        // Perpendicular vector (always perpendicular to screen in 2D)
        Vector2 perpendicular = new Vector2(-direction.Y, direction.X);
        
        // Beam corner positions
        Vector2 topLeft = start + perpendicular * width * 0.5f;
        Vector2 topRight = end + perpendicular * width * 0.5f;
        Vector2 bottomLeft = start - perpendicular * width * 0.5f;
        Vector2 bottomRight = end - perpendicular * width * 0.5f;
        
        // Draw as quad (using custom vertex rendering)
        DrawQuad(spriteBatch, topLeft, topRight, bottomLeft, bottomRight, texture, color);
    }
}
```

### 3.3 Shader-Based Billboarding (3D Context)

```hlsl
// Vertex shader for billboard quads
float4x4 ViewProjection;
float3 CameraRight;  // Camera's right vector in world space
float3 CameraUp;     // Camera's up vector in world space

struct VertexInput
{
    float3 Position : POSITION0;  // Particle center in world space
    float2 TexCoord : TEXCOORD0;  // Corner offset (-1 to 1)
    float2 Size : TEXCOORD1;      // Particle width/height
    float4 Color : COLOR0;
};

struct VertexOutput
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
    float4 Color : COLOR0;
};

VertexOutput BillboardVS(VertexInput input)
{
    VertexOutput output;
    
    // Offset particle center by corner position in camera space
    float3 worldPos = input.Position;
    worldPos += CameraRight * input.TexCoord.x * input.Size.x;
    worldPos += CameraUp * input.TexCoord.y * input.Size.y;
    
    // Project to screen
    output.Position = mul(float4(worldPos, 1), ViewProjection);
    output.TexCoord = input.TexCoord * 0.5 + 0.5; // -1..1 → 0..1
    output.Color = input.Color;
    
    return output;
}
```

### 3.4 Constrained Billboard (Beam Along Axis)

Billboard only around beam axis (for cylindrical beams):

```csharp
/// <summary>
/// Create billboard matrix constrained to beam axis.
/// Beam stays oriented along its direction, but faces camera.
/// </summary>
public static Matrix CreateAxisBillboard(Vector3 beamStart, Vector3 beamEnd, 
                                          Vector3 cameraPosition)
{
    // Beam direction is constrained axis
    Vector3 beamAxis = beamEnd - beamStart;
    beamAxis.Normalize();
    
    // Calculate vector from beam to camera
    Vector3 toCamera = cameraPosition - beamStart;
    
    // Project camera vector onto plane perpendicular to beam
    Vector3 perpendicular = toCamera - Vector3.Dot(toCamera, beamAxis) * beamAxis;
    if (perpendicular.LengthSquared() < 0.001f)
    {
        // Camera is directly along beam axis - use fallback
        perpendicular = Vector3.Up;
    }
    perpendicular.Normalize();
    
    // Up vector is perpendicular to both beam axis and camera direction
    Vector3 up = Vector3.Cross(beamAxis, perpendicular);
    
    Matrix billboard = Matrix.Identity;
    billboard.Right = perpendicular;
    billboard.Up = up;
    billboard.Forward = beamAxis;
    billboard.Translation = beamStart;
    
    return billboard;
}
```

### 3.5 Practical Terraria Beam Implementation

```csharp
public class BillboardedBeamProjectile : ModProjectile
{
    public override bool PreDraw(ref Color lightColor)
    {
        Vector2 start = Projectile.Center;
        Vector2 end = start + Projectile.velocity.SafeNormalize(Vector2.Zero) * Projectile.ai[0];
        
        Texture2D beamTexture = ModContent.Request<Texture2D>("MyMod/Projectiles/LaserBeam").Value;
        
        // Calculate perpendicular width vector
        Vector2 direction = end - start;
        Vector2 unit = direction.SafeNormalize(Vector2.Zero);
        Vector2 perpendicular = new Vector2(-unit.Y, unit.X);
        
        float beamWidth = 20f;
        
        // Draw multiple passes for glow effect
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);
        
        // Inner bright core
        DrawBeamSegment(start, end, perpendicular, beamWidth * 0.3f, beamTexture, Color.White);
        
        // Middle glow
        DrawBeamSegment(start, end, perpendicular, beamWidth * 0.6f, beamTexture, Color.White * 0.6f);
        
        // Outer soft glow
        DrawBeamSegment(start, end, perpendicular, beamWidth, beamTexture, Color.White * 0.3f);
        
        Main.spriteBatch.End();
        Main.spriteBatch.Begin();
        
        return false;
    }
    
    private void DrawBeamSegment(Vector2 start, Vector2 end, Vector2 perpendicular, 
                                  float width, Texture2D texture, Color color)
    {
        float length = Vector2.Distance(start, end);
        float rotation = perpendicular.ToRotation() + MathHelper.PiOver2;
        
        Main.spriteBatch.Draw(
            texture,
            start - Main.screenPosition,
            null,
            color,
            rotation,
            new Vector2(0, texture.Height * 0.5f),
            new Vector2(length / texture.Width, width / texture.Height),
            SpriteEffects.None,
            0f
        );
    }
}
```

---

## 4. Performance Profiling & GC Avoidance

### 4.1 Understanding Garbage Collection

**GC in C#:**
- Automatic memory management
- Pauses execution to clean up unused objects
- **GC Spike** = Frame drop when GC runs

**Allocation Sources:**
| Source | Example | Solution |
|--------|---------|----------|
| `new` keyword | `new List<int>()` | Pre-allocate, reuse |
| Boxing | `object x = 42;` | Use generics |
| String concat | `"A" + "B"` | StringBuilder |
| LINQ | `.Where().ToList()` | Manual loops |
| Array resize | `List.Add()` past capacity | Set initial capacity |
| Closures | Lambda capturing variables | Avoid in hot paths |

### 4.2 Custom Performance Monitor

```csharp
public class PerformanceMonitor
{
    private Stopwatch stopwatch = new Stopwatch();
    private Dictionary<string, TimeSpan> timings = new Dictionary<string, TimeSpan>();
    private Dictionary<string, int> callCounts = new Dictionary<string, int>();
    
    public void BeginSample(string name)
    {
        if (!timings.ContainsKey(name))
        {
            timings[name] = TimeSpan.Zero;
            callCounts[name] = 0;
        }
        stopwatch.Restart();
    }
    
    public void EndSample(string name)
    {
        stopwatch.Stop();
        timings[name] += stopwatch.Elapsed;
        callCounts[name]++;
    }
    
    public void DisplayResults()
    {
        foreach (var kvp in timings.OrderByDescending(x => x.Value))
        {
            double avgMs = kvp.Value.TotalMilliseconds / callCounts[kvp.Key];
            Main.NewText($"{kvp.Key}: {avgMs:F3}ms avg ({callCounts[kvp.Key]} calls)");
        }
    }
    
    public void Reset()
    {
        timings.Clear();
        callCounts.Clear();
    }
}
```

### 4.3 Common Allocation Mistakes & Fixes

#### 1. Creating Objects in Loops

```csharp
// ❌ BAD: Allocates every frame
void Update()
{
    List<Enemy> nearbyEnemies = new List<Enemy>(); // ALLOCATION!
    foreach (var enemy in AllEnemies)
    {
        if (Vector2.Distance(Position, enemy.Position) < 100f)
            nearbyEnemies.Add(enemy);
    }
}

// ✅ GOOD: Reuse list (allocate once as field)
private List<Enemy> nearbyEnemies = new List<Enemy>();

void Update()
{
    nearbyEnemies.Clear(); // Clear, don't recreate
    foreach (var enemy in AllEnemies)
    {
        if (Vector2.Distance(Position, enemy.Position) < 100f)
            nearbyEnemies.Add(enemy);
    }
}
```

#### 2. String Concatenation

```csharp
// ❌ BAD: Each + creates new string
string debugText = "FPS: " + fps + ", Particles: " + particleCount; // ALLOCATIONS!

// ✅ GOOD: Use StringBuilder (reusable)
private StringBuilder sb = new StringBuilder(256);

void Update()
{
    sb.Clear();
    sb.Append("FPS: ").Append(fps).Append(", Particles: ").Append(particleCount);
    string debugText = sb.ToString(); // Single allocation
}
```

#### 3. Boxing Value Types

```csharp
// ❌ BAD: Boxing int to object
object boxedValue = 42; // ALLOCATION!
Dictionary<string, object> data = new Dictionary<string, object>();
data["count"] = particleCount; // Boxing!

// ✅ GOOD: Use generics
Dictionary<string, int> data = new Dictionary<string, int>();
data["count"] = particleCount; // No boxing
```

#### 4. LINQ Allocations

```csharp
// ❌ BAD: LINQ creates enumerators (allocations)
var activeParticles = particles.Where(p => p.Active).ToList(); // ALLOCATIONS!

// ✅ GOOD: Manual loop
activeParticles.Clear();
for (int i = 0; i < particles.Length; i++)
{
    if (particles[i].Active)
        activeParticles.Add(particles[i]);
}
```

#### 5. Array/List Resizing

```csharp
// ❌ BAD: List grows dynamically (allocations when capacity exceeded)
List<Particle> particles = new List<Particle>(); // Default capacity: 4
for (int i = 0; i < 1000; i++)
{
    particles.Add(new Particle()); // Resizes multiple times!
}

// ✅ GOOD: Pre-allocate capacity
List<Particle> particles = new List<Particle>(1000); // No resizing
for (int i = 0; i < 1000; i++)
{
    particles.Add(new Particle());
}
```

#### 6. Closure Allocations

```csharp
// ❌ BAD: Lambda captures variable (allocates closure object)
void Update()
{
    int threshold = 100;
    particles.RemoveAll(p => p.Age > threshold); // ALLOCATION!
}

// ✅ GOOD: Avoid lambda in hot path
void Update()
{
    for (int i = particles.Count - 1; i >= 0; i--)
    {
        if (particles[i].Age > 100)
            particles.RemoveAt(i);
    }
}
```

### 4.4 Memory-Efficient Patterns

#### Generic Object Pool

```csharp
public class ObjectPool<T> where T : class, new()
{
    private Stack<T> pool;
    private int maxSize;
    
    public ObjectPool(int initialSize, int maxSize)
    {
        this.maxSize = maxSize;
        pool = new Stack<T>(initialSize);
        
        for (int i = 0; i < initialSize; i++)
        {
            pool.Push(new T());
        }
    }
    
    public T Get()
    {
        if (pool.Count > 0)
            return pool.Pop();
        return new T(); // Fallback if pool exhausted
    }
    
    public void Return(T obj)
    {
        if (pool.Count < maxSize)
            pool.Push(obj);
        // Else discard (prevent unlimited growth)
    }
}
```

#### Struct vs Class Decision

```csharp
// ❌ Class: Each element is a separate heap allocation
public class Particle { /* 100 bytes */ }
Particle[] particles = new Particle[1000]; // 1000 allocations + array!

// ✅ Struct: Single contiguous memory block
public struct Particle { /* 100 bytes */ }
Particle[] particles = new Particle[1000]; // 1 allocation of 100KB
```

**Struct Guidelines:**
- Use for small data (< 16 bytes ideal, < 64 bytes acceptable)
- Immutable or carefully managed mutability
- No inheritance needed
- Value semantics (copies on assignment)

#### ArrayPool for Temporary Buffers

```csharp
using System.Buffers;

// ❌ BAD: Allocate temporary array
void ProcessData()
{
    float[] tempBuffer = new float[1000]; // ALLOCATION!
    // ... use buffer ...
} // Garbage

// ✅ GOOD: Rent from pool
void ProcessData()
{
    float[] tempBuffer = ArrayPool<float>.Shared.Rent(1000);
    try
    {
        // ... use buffer ...
    }
    finally
    {
        ArrayPool<float>.Shared.Return(tempBuffer);
    }
}
```

### 4.5 Performance Targets

**60 FPS Budget (16.67ms per frame):**

| Phase | Target | Notes |
|-------|--------|-------|
| Update | < 8ms | AI, physics, particles |
| Draw | < 6ms | Rendering, effects |
| Other | < 2ms | Input, audio, misc |
| **Total** | **< 16.67ms** | Room for variance |

**What to Measure:**
- Frame time: Target < 16.67ms
- GC collections: Frequency and duration
- Allocations per frame: Target < 1KB
- Draw calls: Minimize state changes
- Vertex/index count: GPU load

### 4.6 Profiling Tools

| Tool | Best For |
|------|----------|
| **Visual Studio Profiler** | .NET allocation tracking |
| **dotTrace (JetBrains)** | Detailed timeline + GC spikes |
| **Main.NewText** | Quick in-game timing |
| **Stopwatch** | Custom performance sampling |

### 4.7 Resources

- **"Game Programming Patterns" - Object Pool**: https://gameprogrammingpatterns.com/object-pool.html
- **"Writing High-Performance .NET Code"** by Ben Watson
- **Visual Studio Profiler Guide**: https://learn.microsoft.com/en-us/visualstudio/profiling/

---

## Summary

This document covers the advanced VFX core concepts:

| Topic | Key Takeaway |
|-------|--------------|
| **Bezier Curves** | Use `CubicBezier` for smooth paths, derivative for rotation |
| **Particle Pools** | Pre-allocate structs, reuse dead particles |
| **Billboarding** | Perpendicular vector for 2D beam width |
| **Performance** | Avoid allocations in hot paths, use pools |

**Critical Patterns:**
```csharp
// Bezier evaluation
Vector2 pos = BezierCurve.CubicBezier(p0, p1, p2, p3, t);

// Particle pool usage
particles[i].Active = true; // Reuse, don't new

// Billboard perpendicular
Vector2 perp = new Vector2(-direction.Y, direction.X);

// Allocation-free list reuse
myList.Clear(); // Not: myList = new List<T>();
```

---

*Last Updated: February 2026 - VFX Core Concepts Part 2*
