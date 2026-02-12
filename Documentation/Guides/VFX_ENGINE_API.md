# MagnumOpus VFX Engine API Reference

> **Complete API documentation for the Triple-A Visual Effects Engine**
> 
> This document covers all VFX systems available in `Common/Systems/VFX/`
> 
> **Last Updated:** 2026-02-04

---

## TABLE OF CONTENTS

1. [System Overview](#system-overview)
2. [Dynamic Skybox System](#dynamic-skybox-system)
3. [Bézier Projectile System](#bézier-projectile-system)
4. [Fluid Boss Movement](#fluid-boss-movement)
5. [Telegraph System](#telegraph-system)
6. [Rainbow Gradient System](#rainbow-gradient-system)
7. [Enhanced Trail Renderer](#enhanced-trail-renderer)
8. [Particle Handler](#particle-handler)
9. [Bloom Renderer](#bloom-renderer)
10. [Radial Scroll System](#radial-scroll-system)
11. [Integration Examples](#integration-examples)

---

## SYSTEM OVERVIEW

The MagnumOpus VFX Engine provides Calamity-level visual polish through integrated systems:

| System | File | Purpose |
|--------|------|---------|
| **DynamicSkyboxSystem** | `DynamicSkyboxSystem.cs` | Full-screen shader overlays for boss fights |
| **BezierProjectileSystem** | `BezierProjectileSystem.cs` | Curved/arcing projectile paths |
| **FluidBossMovement** | `FluidBossMovement.cs` | Smooth boss movement with predictive targeting |
| **TelegraphSystem** | `TelegraphSystem.cs` | Visual attack warnings and danger indicators |
| **RainbowGradientSystem** | `RainbowGradientSystem.cs` | Theme-specific color cycling |
| **EnhancedTrailRenderer** | `EnhancedTrailRenderer.cs` | Flowing energy trails |
| **MagnumParticleHandler** | `MagnumParticleHandler.cs` | High-performance particle engine |
| **BloomRenderer** | `BloomRenderer.cs` | Multi-layer bloom effects |
| **RadialScrollSystem** | `RadialScrollSystem.cs` | Radial scroll orbs, portals, auras |

---

## DYNAMIC SKYBOX SYSTEM

**File:** `Common/Systems/VFX/DynamicSkyboxSystem.cs`

Full-screen shader overlays that transform the atmosphere during boss fights.

### Available Effects

```csharp
public enum SkyboxEffect
{
    None,
    LaCampanella,    // Infernal orange/black smoke
    Eroica,          // Heroic scarlet/gold
    MoonlightSonata, // Lunar purple/blue
    SwanLake,        // Ethereal white/rainbow
    EnigmaVariations,// Void purple/green
    Fate,            // Cosmic pink/crimson with chromatic aberration
    Seasons,         // Four seasons cycling
    Custom           // User-defined colors
}
```

### Static API

```csharp
// Activate a themed skybox
DynamicSkyboxSystem.ActivateEffect(SkyboxEffect.Fate);

// Deactivate
DynamicSkyboxSystem.DeactivateEffect();

// Trigger screen flash (0f-1f intensity)
DynamicSkyboxSystem.TriggerFlash(0.8f);

// Set chromatic aberration (Fate theme uses this heavily)
DynamicSkyboxSystem.SetChromaticAberration(0.02f);

// Set vignette intensity (0f-1f)
DynamicSkyboxSystem.SetVignette(0.4f);

// Custom colors
DynamicSkyboxSystem.ActivateEffect(SkyboxEffect.Custom);
DynamicSkyboxSystem.SetCustomColors(primaryColor, secondaryColor, accentColor);
```

### Usage Example

```csharp
public override void AI()
{
    // Boss fight skybox
    if (NPC.life == NPC.lifeMax)
    {
        DynamicSkyboxSystem.ActivateEffect(SkyboxEffect.Fate);
    }
    
    // Phase transition flash
    if (phaseChanged)
    {
        DynamicSkyboxSystem.TriggerFlash(1.0f);
        DynamicSkyboxSystem.SetChromaticAberration(0.05f);
    }
}

public override void OnKill()
{
    DynamicSkyboxSystem.DeactivateEffect();
}
```

---

## BÉZIER PROJECTILE SYSTEM

**File:** `Common/Systems/VFX/BezierProjectileSystem.cs`

Create curved, arcing, and snaking projectile paths using Bézier curves.

### Core Math Functions

```csharp
// Quadratic Bézier: B(t) = (1-t)²P₀ + 2(1-t)tP₁ + t²P₂
Vector2 BezierProjectileSystem.QuadraticBezier(Vector2 p0, Vector2 p1, Vector2 p2, float t);

// Cubic Bézier: B(t) = (1-t)³P₀ + 3(1-t)²tP₁ + 3(1-t)t²P₂ + t³P₃
Vector2 BezierProjectileSystem.CubicBezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t);

// Get tangent for rotation
Vector2 BezierProjectileSystem.QuadraticBezierTangent(Vector2 p0, Vector2 p1, Vector2 p2, float t);
Vector2 BezierProjectileSystem.CubicBezierTangent(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t);
```

### Path Generators

```csharp
// Homing arc that curves toward target
(Vector2 P0, Vector2 P1, Vector2 P2, Vector2 P3) GenerateHomingArc(
    Vector2 start, 
    Vector2 target, 
    float arcHeight,    // How high the arc goes
    float sideOffset    // Lateral curve amount
);

// Snaking S-curve path
(Vector2 P0, Vector2 P1, Vector2 P2, Vector2 P3) GenerateSnakingPath(
    Vector2 start, 
    Vector2 direction, 
    float length, 
    float amplitude  // Snake wave size
);

// Spiral approach to target
(Vector2 P0, Vector2 P1, Vector2 P2, Vector2 P3) GenerateSpiralApproach(
    Vector2 start, 
    Vector2 target, 
    float spiralRadius, 
    bool clockwise
);
```

### BezierState for Projectiles

```csharp
public struct BezierState
{
    public Vector2 P0, P1, P2, P3;  // Control points
    public float Progress;          // 0-1 along curve
    public float Speed;             // Progress per frame
    public bool IsActive;
}

// Pack into projectile AI array
float[] packed = BezierState.Pack(state);
Projectile.ai[0] = packed[0];
// ...

// Unpack from AI array
BezierState state = BezierState.Unpack(Projectile.ai);
```

### Usage Example

```csharp
public override void AI()
{
    if (Projectile.ai[0] == 0) // Initialize
    {
        var path = BezierProjectileSystem.GenerateHomingArc(
            Projectile.Center, 
            Main.player[Projectile.owner].Center, 
            200f, 100f
        );
        bezierState = new BezierState 
        { 
            P0 = path.P0, P1 = path.P1, P2 = path.P2, P3 = path.P3,
            Progress = 0f, Speed = 0.02f, IsActive = true 
        };
        Projectile.ai[0] = 1;
    }
    
    // Update position along curve
    BezierProjectileSystem.UpdateBezierProjectile(Projectile, ref bezierState);
}
```

---

## FLUID BOSS MOVEMENT

**File:** `Common/Systems/VFX/FluidBossMovement.cs`

Smooth boss movement with acceleration, drag, and predictive targeting.

### Movement Presets

```csharp
// Heavy bosses (La Campanella style)
var settings = FluidBossMovement.HeavyPreset;
// Acceleration: 0.08f, MaxSpeed: 12f, Drag: 0.96f

// Agile bosses (Swan Lake style)  
var settings = FluidBossMovement.AgilePreset;
// Acceleration: 0.15f, MaxSpeed: 20f, Drag: 0.92f

// Floaty bosses (Moonlight style)
var settings = FluidBossMovement.FloatyPreset;
// Acceleration: 0.05f, MaxSpeed: 8f, Drag: 0.98f

// Erratic bosses (Enigma style)
var settings = FluidBossMovement.ErraticPreset;
// Acceleration: 0.2f, MaxSpeed: 25f, Drag: 0.85f
```

### Core Movement Functions

```csharp
// Apply acceleration toward target velocity
Vector2 FluidBossMovement.ApplyAcceleration(
    Vector2 currentVelocity, 
    Vector2 targetVelocity, 
    float acceleration
);

// Apply drag to slow down
Vector2 FluidBossMovement.ApplyDrag(Vector2 velocity, float drag);

// Combined update
Vector2 FluidBossMovement.UpdateMovement(
    Vector2 currentVelocity, 
    Vector2 targetVelocity, 
    MovementSettings settings
);
```

### Movement Patterns

```csharp
// Move toward a point
Vector2 FluidBossMovement.MoveToward(
    Vector2 currentPos, 
    Vector2 targetPos, 
    Vector2 currentVelocity, 
    MovementSettings settings
);

// Orbit around a point
Vector2 FluidBossMovement.OrbitAround(
    Vector2 currentPos, 
    Vector2 centerPoint, 
    float radius, 
    float angularSpeed, 
    Vector2 currentVelocity, 
    MovementSettings settings
);

// Dash toward target
Vector2 FluidBossMovement.Dash(
    Vector2 currentPos, 
    Vector2 targetPos, 
    float dashSpeed, 
    float dashAcceleration
);
```

### Predictive Targeting

```csharp
// Predict where player will be
Vector2 FluidBossMovement.PredictPlayerPosition(
    Player player, 
    int framesAhead
);

// Calculate interception point for projectiles
Vector2 FluidBossMovement.CalculateInterceptionPoint(
    Vector2 projectileStart, 
    float projectileSpeed, 
    Player player
);

// Lead target for moving enemies
Vector2 FluidBossMovement.LeadTarget(
    Vector2 shooterPos, 
    Vector2 targetPos, 
    Vector2 targetVelocity, 
    float projectileSpeed
);
```

### Phase State Machine

```csharp
public struct PhaseState
{
    public int Phase;        // Main phase (0, 1, 2...)
    public int SubPhase;     // Sub-state within phase
    public int AttackTimer;  // Frames in current attack
    public int StateTimer;   // Total frames in phase
    
    // Pack into AI array
    public void Pack(NPC npc)
    {
        npc.ai[0] = Phase + SubPhase * 100;
        npc.ai[1] = AttackTimer;
        npc.ai[2] = StateTimer;
    }
    
    // Unpack from AI array
    public static PhaseState Unpack(NPC npc);
}
```

### Usage Example

```csharp
public override void AI()
{
    var settings = FluidBossMovement.AgilePreset;
    Player target = Main.player[NPC.target];
    
    // Predict where player is going
    Vector2 predictedPos = FluidBossMovement.PredictPlayerPosition(target, 30);
    
    // Smoothly move toward predicted position
    NPC.velocity = FluidBossMovement.MoveToward(
        NPC.Center, 
        predictedPos, 
        NPC.velocity, 
        settings
    );
}
```

---

## TELEGRAPH SYSTEM

**File:** `Common/Systems/VFX/TelegraphSystem.cs`

Visual warning indicators for boss attacks.

### Telegraph Types

```csharp
public enum TelegraphType
{
    ThreatLine,      // Line showing projectile path
    DangerZone,      // Circular danger area
    SafeZone,        // Circular safe area
    ConvergingRing,  // Particles converging to center (charging)
    LaserPath,       // Wide beam path warning
    ImpactPoint,     // X marks the spot
    SectorCone       // Cone-shaped danger area
}
```

### Static API

```csharp
// Show projectile trajectory
TelegraphSystem.ThreatLine(
    Vector2 start, 
    Vector2 end, 
    Color color, 
    float width = 4f,
    int lifetime = 60
);

// Show danger circle
TelegraphSystem.DangerZone(
    Vector2 center, 
    float radius, 
    Color color, 
    int lifetime = 60
);

// Show safe area
TelegraphSystem.SafeZone(
    Vector2 center, 
    float radius, 
    Color color, 
    int lifetime = 60
);

// Show charge-up convergence
TelegraphSystem.ConvergingRing(
    Vector2 center, 
    float startRadius, 
    float endRadius, 
    Color color, 
    int lifetime = 90
);

// Show laser beam path
TelegraphSystem.LaserPath(
    Vector2 start, 
    float angle, 
    float length, 
    float width, 
    Color color, 
    int lifetime = 60
);

// Show impact point
TelegraphSystem.ImpactPoint(
    Vector2 position, 
    Color color, 
    float size = 32f,
    int lifetime = 45
);

// Show cone danger area
TelegraphSystem.SectorCone(
    Vector2 origin, 
    float angle, 
    float spread,   // Half-angle in radians
    float length, 
    Color color, 
    int lifetime = 60
);
```

### Usage Example

```csharp
// Boss attack with telegraph
private void Attack_LaserSweep()
{
    if (attackTimer == 0)
    {
        // Telegraph phase - show warning
        float sweepAngle = (Main.player[NPC.target].Center - NPC.Center).ToRotation();
        TelegraphSystem.LaserPath(NPC.Center, sweepAngle, 1500f, 40f, Color.Red * 0.5f, 60);
        TelegraphSystem.ConvergingRing(NPC.Center, 200f, 0f, Color.Orange, 60);
    }
    else if (attackTimer == 60)
    {
        // Execute phase - fire laser
        SpawnLaser();
    }
    
    attackTimer++;
}
```

---

## RAINBOW GRADIENT SYSTEM

**File:** `Common/Systems/VFX/RainbowGradientSystem.cs`

Exo-style prismatic color cycling for dynamic effects.

### Basic Color Functions

```csharp
// Full rainbow cycle (0-1 input)
Color RainbowGradientSystem.GetRainbowColor(float t);

// Sine-wave smoothed rainbow
Color RainbowGradientSystem.GetSineRainbow(float t, float saturation = 1f, float brightness = 1f);

// Rainbow constrained to hue range
Color RainbowGradientSystem.GetConstrainedRainbow(
    float t, 
    float minHue, 
    float maxHue,
    float saturation = 1f, 
    float brightness = 1f
);
```

### Theme-Specific Gradients

```csharp
// Swan Lake - white/black with rainbow shimmer
Color RainbowGradientSystem.GetSwanLakeShimmer(float t);

// Fate - cosmic gradient (black → pink → red → white highlights)
Color RainbowGradientSystem.GetFateCosmic(float t);

// Eroica - heroic gradient (scarlet → crimson → gold)
Color RainbowGradientSystem.GetEroicaFlame(float t);

// La Campanella - infernal gradient (black → orange → gold)
Color RainbowGradientSystem.GetCampanellaInferno(float t);

// Moonlight Sonata - lunar gradient (purple → blue → silver)
Color RainbowGradientSystem.GetMoonlightGlow(float t);

// Enigma - void gradient (black → purple → green)
Color RainbowGradientSystem.GetEnigmaVoid(float t);
```

### Position-Based Gradients

```csharp
// Color based on world position (creates flowing patterns)
Color RainbowGradientSystem.GetPositionalRainbow(Vector2 position, float scale, float timeOffset);

// Radial gradient from center
Color RainbowGradientSystem.GetRadialRainbow(Vector2 position, Vector2 center, float radius);

// Angular gradient around center
Color RainbowGradientSystem.GetAngularRainbow(Vector2 position, Vector2 center, float angleOffset);
```

### Trail Gradients

```csharp
// Trail gradient from head to tail
Color RainbowGradientSystem.GetTrailGradient(
    float progress,      // 0 = head, 1 = tail
    Color headColor, 
    Color tailColor, 
    float fadeStart = 0.7f
);

// Prismatic trail (full rainbow)
Color RainbowGradientSystem.GetPrismaticTrail(float progress, float timeOffset);
```

### Usage Example

```csharp
public override bool PreDraw(ref Color lightColor)
{
    // Animated rainbow trail
    for (int i = 0; i < trailPositions.Length; i++)
    {
        float progress = i / (float)trailPositions.Length;
        Color trailColor = RainbowGradientSystem.GetPrismaticTrail(progress, Main.GameUpdateCount * 0.02f);
        
        // Draw trail segment...
    }
    
    // Theme-specific projectile glow
    Color glowColor = RainbowGradientSystem.GetFateCosmic(Main.GameUpdateCount * 0.05f % 1f);
    // Draw glow...
}
```

---

## ENHANCED TRAIL RENDERER

**File:** `Common/Systems/VFX/EnhancedTrailRenderer.cs`

Flowing energy trails for projectiles and weapons.

### Trail Configuration

```csharp
public struct TrailConfig
{
    public int MaxLength;        // Max position history
    public float StartWidth;     // Width at head
    public float EndWidth;       // Width at tail
    public Color StartColor;     // Color at head
    public Color EndColor;       // Color at tail
    public BlendState Blend;     // Additive, AlphaBlend, etc.
    public Texture2D Texture;    // Trail texture (optional)
}
```

### Trail Management

```csharp
// Create a trail
int trailId = EnhancedTrailRenderer.CreateTrail(TrailConfig config);

// Update trail position
EnhancedTrailRenderer.UpdateTrail(int trailId, Vector2 position);

// Draw trail
EnhancedTrailRenderer.DrawTrail(int trailId, SpriteBatch spriteBatch);

// Remove trail
EnhancedTrailRenderer.RemoveTrail(int trailId);
```

---

## PARTICLE HANDLER

**File:** `Common/Systems/Particles/MagnumParticleHandler.cs`

High-performance particle engine supporting 10,000+ particles.

### Spawning Particles

```csharp
// Spawn a particle
MagnumParticleHandler.SpawnParticle(IParticle particle);

// Built-in particle types
MagnumParticleHandler.SpawnParticle(new BloomParticle(position, velocity, color, scale, lifetime));
MagnumParticleHandler.SpawnParticle(new SparkleParticle(position, velocity, color, scale, lifetime));
MagnumParticleHandler.SpawnParticle(new GenericGlowParticle(position, velocity, color, scale, lifetime, fade));
```

### Batch Spawning

```csharp
// Spawn multiple particles efficiently
MagnumParticleHandler.SpawnBurst(
    Vector2 center, 
    int count, 
    Func<int, IParticle> particleFactory
);
```

---

## BLOOM RENDERER

**File:** `Common/Systems/VFX/BloomRenderer.cs`

Multi-layer bloom effects using FargosSoulsDLC patterns.

### Key Pattern: Alpha Removal

```csharp
// CRITICAL: Remove alpha for proper additive blending
Color bloomColor = baseColor with { A = 0 };
```

### Multi-Layer Bloom

```csharp
// Draw 4-layer bloom stack
BloomRenderer.DrawBloomStack(
    SpriteBatch spriteBatch,
    Texture2D texture,
    Vector2 position,
    Color color,
    float scale,
    int layers = 4,
    float intensity = 1f,
    float rotation = 0f
);
```

---

## INTEGRATION EXAMPLES

### Complete Boss Example

```csharp
public class ExampleBoss : ModNPC
{
    private PhaseState phaseState;
    private MovementSettings movement;
    
    public override void SetDefaults()
    {
        movement = FluidBossMovement.AgilePreset;
    }
    
    public override void AI()
    {
        phaseState = PhaseState.Unpack(NPC);
        Player target = Main.player[NPC.target];
        
        // Skybox
        if (phaseState.Phase == 0 && phaseState.StateTimer == 0)
            DynamicSkyboxSystem.ActivateEffect(SkyboxEffect.Fate);
        
        // Movement
        Vector2 predictedPos = FluidBossMovement.PredictPlayerPosition(target, 20);
        NPC.velocity = FluidBossMovement.MoveToward(NPC.Center, predictedPos, NPC.velocity, movement);
        
        // Attacks with telegraphs
        if (phaseState.AttackTimer == 0)
            TelegraphSystem.DangerZone(target.Center, 200f, Color.Red * 0.3f, 60);
        else if (phaseState.AttackTimer == 60)
            SpawnAOEAttack(target.Center, 200f);
        
        phaseState.AttackTimer++;
        phaseState.StateTimer++;
        phaseState.Pack(NPC);
    }
}
```

### Complete Projectile Example

```csharp
public class ArcingProjectile : ModProjectile
{
    private BezierState bezierState;
    
    public override void AI()
    {
        if (!bezierState.IsActive)
        {
            // Initialize Bézier path
            Player target = Main.player[Projectile.owner];
            var path = BezierProjectileSystem.GenerateHomingArc(
                Projectile.Center, target.Center, 150f, 80f
            );
            bezierState = new BezierState
            {
                P0 = path.P0, P1 = path.P1, P2 = path.P2, P3 = path.P3,
                Progress = 0f, Speed = 0.015f, IsActive = true
            };
        }
        
        // Update position along curve
        BezierProjectileSystem.UpdateBezierProjectile(Projectile, ref bezierState);
        
        // Rainbow trail
        Color trailColor = RainbowGradientSystem.GetFateCosmic(Main.GameUpdateCount * 0.03f % 1f);
        // Spawn trail particles...
    }
}
```

---

## RADIAL SCROLL SYSTEM

**File:** `Common/Systems/VFX/Core/RadialScrollSystem.cs`

High-performance radial scrolling effects for orbs, portals, auras, and rotating energy patterns.

### Available Techniques

```csharp
public enum RadialScrollTechnique
{
    TECH_MULTI_LAYER,   // 4-layer outward scroll with vortex core
    TECH_DISTORTED,     // Perlin noise-based warped scrolling
    TECH_PULSING,       // Rhythmic expansion/contraction waves
    TECH_DUAL_PHASE,    // Inner/outer rings scrolling opposite directions
    TECH_VORTEX         // Spiral inward with converging energy
}
```

### Pre-Built Presets

| Preset | Technique | Description |
|--------|-----------|-------------|
| `EnergyStar` | MULTI_LAYER | Bright expanding star effect |
| `Portal` | VORTEX | Spiraling portal with inward pull |
| `AuraRing` | PULSING | Pulsing aura ring around entities |
| `Cosmic` | DISTORTED | Cosmic nebula distortion effect |
| `Singularity` | DUAL_PHASE | Black hole-style dual rotation |

### Static API

```csharp
// Draw a preset orb effect
RadialScrollSystem.DrawOrb(
    position: projectile.Center,
    size: 50f,
    time: Main.GlobalTimeWrappedHourly,
    preset: RadialScrollSystem.EnergyStar
);

// Draw a portal effect
RadialScrollSystem.DrawPortal(
    position: portalCenter,
    size: 80f,
    time: Main.GlobalTimeWrappedHourly
);

// Draw with custom colors
RadialScrollSystem.DrawOrb(
    position: center,
    size: 60f,
    time: gameTime,
    preset: RadialScrollSystem.Portal,
    overridePrimary: Color.Cyan,
    overrideSecondary: Color.White
);

// Create custom preset
var myPreset = new RadialScrollSystem.RadialScrollPreset
{
    Technique = RadialScrollTechnique.TECH_PULSING,
    PrimaryColor = new Color(255, 100, 50),
    SecondaryColor = new Color(255, 200, 100),
    CoreColor = Color.White,
    ScrollSpeed = 0.5f,
    Intensity = 1.2f,
    Layers = 4
};
RadialScrollSystem.DrawOrb(position, size, time, myPreset);
```

### Fallback Rendering

The system includes robust fallback rendering for shader-disabled environments:

| Method | Purpose |
|--------|---------|
| `DrawFallbackOrb()` | 5-layer bloom with pulsing animation |
| `DrawFallbackPortal()` | Swirling rings + bloom core |

Fallbacks automatically activate when shaders are unavailable, ensuring visual effects work on all hardware.

### Usage Example - VFXPlus Weapon

```csharp
public override bool PreDraw(ref Color lightColor)
{
    Vector2 drawPos = player.itemLocation;
    float time = Main.GlobalTimeWrappedHourly;
    
    // Draw glowing orb at weapon tip
    RadialScrollSystem.DrawOrb(
        drawPos,
        size: 40f * chargeProgress,
        time: time,
        preset: RadialScrollSystem.EnergyStar
    );
    
    return true;
}
```

---

## QUICK REFERENCE

### Common Patterns

```csharp
// Boss fight setup
DynamicSkyboxSystem.ActivateEffect(SkyboxEffect.ThemeName);

// Attack telegraph
TelegraphSystem.DangerZone(targetPos, radius, Color.Red * 0.4f, telegraphDuration);

// Smooth movement
NPC.velocity = FluidBossMovement.UpdateMovement(NPC.velocity, targetVelocity, settings);

// Predictive aim
Vector2 aimAt = FluidBossMovement.PredictPlayerPosition(player, 30);

// Curved projectile
var path = BezierProjectileSystem.GenerateHomingArc(start, target, arcHeight, sideOffset);

// Theme colors
Color color = RainbowGradientSystem.GetFateCosmic(animationProgress);

// Multi-layer bloom
BloomRenderer.DrawBloomStack(spriteBatch, texture, position, color with { A = 0 }, scale, 4, 1f);

// Radial scroll orb effects
RadialScrollSystem.DrawOrb(position, size, time, RadialScrollSystem.EnergyStar);

// Portal effects
RadialScrollSystem.DrawPortal(position, size, time);
```

---

*End of VFX Engine API Reference*
