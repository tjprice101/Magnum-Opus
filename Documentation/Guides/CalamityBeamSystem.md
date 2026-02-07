# Calamity-Style Beam Effect System

> **Massive, shiny, buttery-smooth beam effects with interpolation and tick management.**

This document describes the MagnumOpus beam effect system, which provides Calamity Mod-style massive beam rendering with:
- **Primitive mesh rendering** with vertex strips
- **Multi-pass bloom stacking** (4+ layers)
- **Sub-pixel interpolation** for 144Hz+ smoothness
- **Tick-based lifetime management** for frame-independent timing
- **Dynamic width/color functions** for varied beam profiles
- **Automatic particle emission** along beams

---

## System Files

| File | Purpose |
|------|---------|
| [CalamityBeamSystem.cs](../Common/Systems/VFX/CalamityBeamSystem.cs) | Core beam rendering library with all the rendering logic |
| [BaseBeamProjectile.cs](../Common/Systems/VFX/BaseBeamProjectile.cs) | Base class for beam projectiles - inherit from this |
| [ExampleBeamProjectiles.cs](../Common/Systems/VFX/ExampleBeamProjectiles.cs) | Example implementations for each theme |
| [MassiveBeamSystem.cs](../Common/Systems/VFX/MassiveBeamSystem.cs) | Lower-level beam utilities (used internally) |
| [BeamVertexStrip.cs](../Common/Systems/VFX/BeamVertexStrip.cs) | Managed vertex strip state with tick aging |
| [InterpolatedRenderer.cs](../Common/Systems/VFX/InterpolatedRenderer.cs) | Sub-pixel interpolation system |

---

## Quick Start

### Option 1: Inherit from BaseBeamProjectile (Recommended)

The easiest way to create a beam projectile:

```csharp
using MagnumOpus.Common.Systems.VFX;

public class MyDeathRay : BaseBeamProjectile
{
    public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.LastPrismLaser;
    
    // Required: Theme for color palette
    public override string ThemeName => "Eroica";
    
    // Basic settings
    public override float BeamWidth => 50f;
    public override float MaxBeamLength => 1800f;
    
    public override void SetDefaults()
    {
        base.SetDefaults();
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.timeLeft = 180;
    }
    
    protected override Vector2 GetBeamEndPoint()
    {
        // Define where the beam ends
        return Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitX) * MaxBeamLength;
    }
}
```

### Option 2: Direct CalamityBeamSystem API

For custom rendering or non-projectile beams:

```csharp
using MagnumOpus.Common.Systems.VFX;

// Simple beam between two points
CalamityBeamSystem.RenderBeam(
    Main.spriteBatch,
    startPosition,
    endPosition,
    "Eroica",  // Theme name
    40f,       // Width
    1f         // Opacity
);

// Wave-style beam
CalamityBeamSystem.RenderWaveBeam(
    Main.spriteBatch,
    start, end,
    "Fate",
    35f,
    waveAmplitude: 10f,
    waveFrequency: 3f
);

// Pulsing beam
CalamityBeamSystem.RenderPulsingBeam(
    Main.spriteBatch,
    start, end,
    "LaCampanella",
    60f,
    pulseSpeed: 5f,
    pulseAmount: 0.3f
);
```

---

## Width Styles

The system supports multiple beam width profiles:

| Style | Description | Visual |
|-------|-------------|--------|
| `QuadraticBump` | Thin→Thick→Thin (default) | Classic beam profile |
| `SourceTaper` | Thick at source, thin at end | Laser/ray style |
| `Constant` | Same width throughout | Energy wall |
| `PulsingWidth` | QuadraticBump with pulsing | Alive/organic feel |

```csharp
// In BaseBeamProjectile subclass:
public override CalamityBeamSystem.WidthStyle WidthStyle => CalamityBeamSystem.WidthStyle.SourceTaper;

// Or with CalamityBeamSystem directly:
var profile = new CalamityBeamSystem.BeamProfile
{
    ThemeName = "Fate",
    BaseWidth = 40f,
    WidthType = CalamityBeamSystem.WidthStyle.PulsingWidth,
    PulseSpeed = 5f,
    PulseAmount = 0.3f
};
CalamityBeamSystem.RenderBeam(spriteBatch, start, end, profile, 1f);
```

---

## Multi-Pass Bloom Rendering

The system automatically renders beams in 5 passes:

1. **Outer Bloom** (×2.5 width, 15% opacity) - Large additive glow
2. **Middle Glow** (×1.5 width, 35% opacity) - Secondary glow layer  
3. **Main Beam** (×1.0 width, 85% opacity) - The core visible beam
4. **Hot Core** (×0.3 width, 100% opacity, white) - Bright center
5. **Particles** - Theme-specific particle emission

This creates the characteristic "massive shiny" Calamity look.

---

## Theme Examples

### Eroica (Heroic Death Ray)
```csharp
public class EroicaDeathRay : BaseBeamProjectile
{
    public override string ThemeName => "Eroica";
    public override float BeamWidth => 50f;
    public override CalamityBeamSystem.WidthStyle WidthStyle => CalamityBeamSystem.WidthStyle.QuadraticBump;
    
    protected override void OnBeamAI()
    {
        // Track to mouse
        Player owner = Main.player[Projectile.owner];
        Vector2 toMouse = Main.MouseWorld - owner.Center;
        Projectile.velocity = toMouse.SafeNormalize(Vector2.UnitX) * 10f;
        Projectile.Center = owner.Center;
        
        // Sakura petals (theme-specific)
        if (Main.rand.NextBool(8))
            ThemedParticles.SakuraPetals(Vector2.Lerp(BeamStart, BeamEnd, Main.rand.NextFloat()), 1, 15f);
    }
    
    protected override Vector2 GetBeamEndPoint()
        => Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitX) * MaxBeamLength;
}
```

### Fate (Cosmic Homing Laser)
```csharp
public class FateCosmicLaser : BaseBeamProjectile
{
    public override string ThemeName => "Fate";
    public override float BeamWidth => 35f;
    public override CalamityBeamSystem.WidthStyle WidthStyle => CalamityBeamSystem.WidthStyle.PulsingWidth;
    public override float BloomMultiplier => 3.5f;
    
    private NPC _target;
    
    protected override Vector2 GetBeamEndPoint()
    {
        if (_target != null && _target.active)
            return _target.Center;
        return Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitX) * MaxBeamLength;
    }
    
    protected override void OnBeamAI()
    {
        // Find/track target
        UpdateTarget();
        
        // Glyphs and stars (Fate theme)
        if (Main.rand.NextBool(6))
            CustomParticles.GlyphBurst(Vector2.Lerp(BeamStart, BeamEnd, Main.rand.NextFloat()), 
                MagnumThemePalettes.FatePink, 2, 3f);
    }
}
```

### La Campanella (Infernal Sweep)
```csharp
public class LaCampanellaInferno : BaseBeamProjectile
{
    public override string ThemeName => "LaCampanella";
    public override float BeamWidth => 60f;
    public override CalamityBeamSystem.WidthStyle WidthStyle => CalamityBeamSystem.WidthStyle.SourceTaper;
    
    private float _rotation;
    private float _sweepAngle = MathHelper.PiOver2;
    
    protected override Vector2 GetBeamEndPoint()
    {
        float baseAngle = Projectile.velocity.ToRotation();
        float currentAngle = baseAngle + MathF.Sin(_rotation) * _sweepAngle;
        return Projectile.Center + currentAngle.ToRotationVector2() * MaxBeamLength;
    }
    
    protected override void OnBeamAI()
    {
        _rotation += 0.02f; // Sweep speed
        
        // Heavy smoke (La Campanella theme)
        if (Main.rand.NextBool(3))
        {
            Dust smoke = Dust.NewDustPerfect(Vector2.Lerp(BeamStart, BeamEnd, Main.rand.NextFloat()),
                DustID.Smoke, Main.rand.NextVector2Circular(2f, 2f), 150, Color.Black, 2f);
            smoke.noGravity = true;
        }
    }
}
```

---

## Boss Attack Beams

For boss attacks, use `RenderBossBeam`:

```csharp
// In boss AI
public override void AI()
{
    if (IsAttacking && AttackType == AttackType.DeathRay)
    {
        // Calculate beam direction from boss to player
        Player target = Main.player[NPC.target];
        beamDirection = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
        
        // Render the beam
        CalamityBeamSystem.RenderBossBeam(
            Main.spriteBatch,
            NPC.Center,
            NPC.Center + beamDirection * 2000f,
            "LaCampanella",
            80f,  // Width
            1f,   // Opacity
            5     // Extra particle density
        );
        
        // Deal damage to players in beam
        DamagePlayersInBeam();
    }
}
```

---

## Interpolation System

The beam system uses `InterpolatedRenderer` for 144Hz+ smoothness:

```csharp
// Automatic in BaseBeamProjectile via:
protected Vector2 BeamStart => InterpolatedRenderer.GetInterpolatedCenter(Projectile);

// Manual usage:
float partialTicks = InterpolatedRenderer.PartialTicks; // 0-1 between updates
Vector2 smoothPos = InterpolatedRenderer.GetInterpolatedCenter(myProjectile);
Vector2 smoothNPCPos = InterpolatedRenderer.GetInterpolatedCenter(myNPC);
```

This prevents the "stuttery" look at high refresh rates.

---

## Managed Beam Registry

For persistent beams (e.g., channeled attacks), use the managed system:

```csharp
// Register a beam (returns ID)
int beamId = CalamityBeamSystem.RegisterManagedBeam(
    startPosition,
    endPosition,
    "Fate",
    40f,
    lifetime: 120  // Ticks
);

// Update beam position each frame
CalamityBeamSystem.UpdateManagedBeam(beamId, newStart, newEnd);

// System automatically removes expired beams
// Or manually remove:
CalamityBeamSystem.RemoveManagedBeam(beamId);

// Draw all managed beams in PostDraw:
CalamityBeamSystem.DrawManagedBeams(Main.spriteBatch);
```

---

## Impact Effects

Create impact effects at beam endpoints:

```csharp
// Standard impact
CalamityBeamSystem.CreateImpactEffect(beamEndPosition, "Eroica", 1.5f);

// Startup burst (when beam begins)
CalamityBeamSystem.CreateStartupEffect(beamStartPosition, "LaCampanella", 1.2f);
```

---

## Performance Notes

1. **Segment Count**: Default 50 segments is good balance. Use 70+ for very long beams with curves.
2. **Particle Density**: Reduce `ParticleDensity` (0.5f or lower) for performance-critical situations.
3. **Bloom Multiplier**: Lower values (1.5-2.0) are faster; higher (3.0+) are more dramatic.
4. **Managed Beams**: Clean up old beams to prevent memory accumulation.

---

## Available Themes

| Theme | Gradient | Best For |
|-------|----------|----------|
| `Eroica` | Scarlet → Gold | Heroic weapons, triumphant attacks |
| `Fate` | Black → Pink → Red | Cosmic endgame, celestial weapons |
| `LaCampanella` | Black → Orange → Gold | Infernal/fire attacks, boss lasers |
| `MoonlightSonata` | Purple → Ice Blue | Ethereal, lunar weapons |
| `SwanLake` | White → Black + Rainbow | Elegant, prismatic effects |
| `EnigmaVariations` | Purple → Green | Mysterious, arcane attacks |
| `ClairDeLune` | Night Mist → Pearl | Dreamy, soft effects |
| `DiesIrae` | Blood → Hellfire | Aggressive, wrath attacks |

---

## See Also

- [TRUE_VFX_STANDARDS.md](TRUE_VFX_STANDARDS.md) - General VFX guidelines
- [Enhanced_VFX_System.md](Enhanced_VFX_System.md) - Bloom and particle systems
- [MASTER_VFX_REFERENCE.md](../MASTER_VFX_REFERENCE.md) - Complete VFX reference
