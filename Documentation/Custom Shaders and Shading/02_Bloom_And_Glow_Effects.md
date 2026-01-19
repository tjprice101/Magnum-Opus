# Bloom and Glow Effects

> **Comprehensive guide to bloom rendering, shine flares, and glow effects from FargosSoulsDLC.**

---

## Core Bloom Textures

FargosSoulsDLC uses textures from `MiscTexturesRegistry` (provided by Luminance library):

| Texture | Purpose | Typical Use |
|---------|---------|-------------|
| `BloomCircleSmall` | Soft circular glow | General bloom, auras, core glows |
| `ShineFlareTexture` | Directional cross/star flare | Impact flashes, charge-up gleams |
| `Pixel` | 1x1 white pixel | Scaled up for shader targets |
| `InvisiblePixel` | Transparent pixel | Placeholder textures for shader-only rendering |
| `WavyBlotchNoise` | Wavy noise pattern | Noise scrolling in shaders |
| `DendriticNoise` | Branching noise | Lightning, cracks, organic patterns |
| `DendriticNoiseZoomedOut` | Larger scale dendritic | Portal effects, large-scale patterns |

---

## The Essential Bloom Pattern

**CRITICAL: Always set alpha to 0 for additive bloom!**

```csharp
// Standard bloom draw call
Texture2D bloom = MiscTexturesRegistry.BloomCircleSmall.Value;
Main.spriteBatch.Draw(
    bloom, 
    position - Main.screenPosition,    // World to screen coords
    null,                               // Source rectangle (full texture)
    color with { A = 0 } * opacity,     // COLOR WITH A = 0!
    0f,                                 // Rotation
    bloom.Size() * 0.5f,                // Origin at center
    scale,                              // Scale factor
    0,                                  // SpriteEffects
    0f                                  // Layer depth
);
```

**Why `{ A = 0 }`?** Additive blending works by adding RGB values. If alpha > 0, it will blend incorrectly and create muddy colors instead of bright glows.

---

## Multi-Layer Bloom Stack

The key to vibrant glows is **multiple layers at different scales**:

```csharp
Vector2 drawPosition = Projectile.Center - Main.screenPosition;
Texture2D bloom = MiscTexturesRegistry.BloomCircleSmall.Value;
Vector2 origin = bloom.Size() * 0.5f;

// Layer 1: Outer glow (largest, most transparent)
Main.spriteBatch.Draw(bloom, drawPosition, null, 
    primaryColor with { A = 0 } * 0.3f, 0f, origin, scale * 2.0f, 0, 0f);

// Layer 2: Middle glow
Main.spriteBatch.Draw(bloom, drawPosition, null, 
    primaryColor with { A = 0 } * 0.5f, 0f, origin, scale * 1.4f, 0, 0f);

// Layer 3: Inner bloom
Main.spriteBatch.Draw(bloom, drawPosition, null, 
    secondaryColor with { A = 0 } * 0.7f, 0f, origin, scale * 0.9f, 0, 0f);

// Layer 4: Bright core (smallest, white/near-white)
Main.spriteBatch.Draw(bloom, drawPosition, null, 
    Color.White with { A = 0 } * 0.85f, 0f, origin, scale * 0.4f, 0, 0f);
```

---

## Shine Flare Effects

The `ShineFlareTexture` creates a directional cross/star pattern:

```csharp
Texture2D flare = MiscTexturesRegistry.ShineFlareTexture.Value;
Texture2D bloom = MiscTexturesRegistry.BloomCircleSmall.Value;

// Calculate animated values
float flareOpacity = Utilities.InverseLerp(1f, 0.75f, glimmerInterpolant);
float flareScale = MathF.Pow(Utilities.Convert01To010(glimmerInterpolant), 1.4f) * 1.9f + 0.1f;
float flareRotation = Main.GlobalTimeWrappedHourly * 2f;  // Slow rotation

// Color scheme (orange to cyan gradient)
Color flareColorA = Color.Orange;
Color flareColorB = new Color(1f, 0.6f, 0.1f);
Color flareColorC = Color.Cyan;

// Draw bloom background
Main.spriteBatch.Draw(bloom, drawPosition, null, 
    flareColorA with { A = 0 } * flareOpacity * 0.3f, 0f, 
    bloom.Size() * 0.5f, flareScale * 1.9f, 0, 0f);

Main.spriteBatch.Draw(bloom, drawPosition, null, 
    flareColorB with { A = 0 } * flareOpacity * 0.54f, 0f, 
    bloom.Size() * 0.5f, flareScale, 0, 0f);

// Draw cross flare on top
Main.spriteBatch.Draw(flare, drawPosition, null, 
    flareColorC with { A = 0 } * flareOpacity, flareRotation, 
    flare.Size() * 0.5f, flareScale, 0, 0f);
```

---

## Pulsing/Animated Bloom

### Cosine-Based Pulsing
```csharp
// Smooth pulse using cosine
float bloomScaleFactor = MathF.Cos(Main.GlobalTimeWrappedHourly * 48f) * 0.1f + 1f;
// Result: Oscillates between 0.9 and 1.1

// Slower, larger pulse
float pulse = Utilities.Cos01(MathHelper.TwoPi * Time / 6f);
float bloomScale = MathHelper.Lerp(0.85f, 1.3f, pulse);
```

### Interpolant-Based (Charge-up/Fade-out)
```csharp
// Charge-up effect
float chargeProgress = Time / MaxChargeTime;
float bloomScale = 0.2f + chargeProgress * 1.5f;  // Grows as charging
float bloomOpacity = 0.3f + chargeProgress * 0.6f;  // Gets brighter

// Fade-out effect
float lifeProgress = (float)Projectile.timeLeft / MaxLifetime;
float fadeOpacity = lifeProgress;  // Fades as lifetime decreases
```

### Convert01To010 Pattern
```csharp
// Creates 0 → 1 → 0 over the range
// Perfect for things that appear, peak, then disappear
float peakInterpolant = Utilities.Convert01To010(progress);
float flareScale = MathF.Pow(peakInterpolant, 1.4f) * 1.9f + 0.1f;
```

---

## Bloom in PreDraw

```csharp
public override bool PreDraw(ref Color lightColor)
{
    Vector2 drawPosition = Projectile.Center - Main.screenPosition;
    Texture2D bloom = MiscTexturesRegistry.BloomCircleSmall.Value;
    
    // Animated scale
    float bloomScaleFactor = MathF.Cos(Main.GlobalTimeWrappedHourly * 48f) * 0.1f + 1f;
    
    // Color with removed alpha for additive blending
    Color bloomColor = new Color(255, 255, 255, 0);  // Explicit zero alpha
    // OR
    Color bloomColor = Color.White with { A = 0 };    // C# 9 syntax
    
    // Draw outer glow
    Main.spriteBatch.Draw(bloom, drawPosition, null, 
        LaserColorFunction(0.5f) with { A = 0 }, 0f, 
        bloom.Size() * 0.5f, bloomScaleFactor * 1.5f, 0, 0f);
    
    // Draw inner white core
    Main.spriteBatch.Draw(bloom, drawPosition, null, 
        bloomColor * 0.6f, 0f, 
        bloom.Size() * 0.5f, bloomScaleFactor * 0.6f, 0, 0f);
    
    // Continue to default drawing
    return true;
    
    // OR return false to skip default drawing entirely
}
```

---

## Thruster Glow Example (Exo Twins)

```csharp
public static void DrawThrusters(NPC twin, IExoTwin twinInterface)
{
    Vector2 thrusterBloomPosition = twin.Center - Main.screenPosition 
        - twin.rotation.ToRotationVector2() * twin.scale * 12f;
    
    Texture2D thrusterBloom = MiscTexturesRegistry.BloomCircleSmall.Value;
    
    // Color based on thruster boost level
    Color thrusterBloomColor = Color.SkyBlue * (twinInterface.ThrusterBoost * 0.6f + 0.33f) * twin.Opacity;
    thrusterBloomColor.A = 0;  // Critical for additive!
    
    // Scale based on boost
    float thrusterScale = 0.3f + twinInterface.ThrusterBoost * 0.4f;
    
    Main.spriteBatch.Draw(thrusterBloom, thrusterBloomPosition, null, 
        thrusterBloomColor, 0f, 
        thrusterBloom.Size() * 0.5f, thrusterScale, 0, 0f);
}
```

---

## StrongBloom Particle Class

FargosSoulsDLC uses Calamity's `StrongBloom` particle for major bloom effects:

```csharp
// Impact bloom
StrongBloom impactBloom = new(
    position: Projectile.Center, 
    velocity: Vector2.Zero, 
    color: new Color(0.34f, 0.5f, 1f),  // Cyan-blue
    scale: 3f, 
    lifetime: 16
);
GeneralParticleHandler.SpawnParticle(impactBloom);

// Double bloom for extra impact
StrongBloom bloom2 = new(Projectile.Center, Vector2.Zero, Color.White, 3f, 5);
GeneralParticleHandler.SpawnParticle(bloom2);
```

---

## Electric Gleam Pattern

Used for telegraphs and charge-ups:

```csharp
public static void RenderElectricGleam(Vector2 drawPosition, float glimmerInterpolant)
{
    Texture2D flare = MiscTexturesRegistry.ShineFlareTexture.Value;
    Texture2D bloom = MiscTexturesRegistry.BloomCircleSmall.Value;
    
    // Calculate animation values
    float flareOpacity = LumUtils.InverseLerp(1f, 0.75f, glimmerInterpolant);
    float flareScale = MathF.Pow(LumUtils.Convert01To010(glimmerInterpolant), 1.4f) * 1.9f + 0.1f;
    float flareRotation = Main.GlobalTimeWrappedHourly * 2f;
    
    // Blue-cyan color scheme
    Color flareColorA = new Color(0.3f, 0.5f, 1f);
    Color flareColorB = new Color(0.1f, 0.8f, 1f);
    Color flareColorC = Color.Cyan;
    
    // Layer 1: Large soft bloom
    Main.spriteBatch.Draw(bloom, drawPosition, null, 
        flareColorA with { A = 0 } * flareOpacity * 0.3f, 0f, 
        bloom.Size() * 0.5f, flareScale * 1.9f, 0, 0f);
    
    // Layer 2: Medium bloom
    Main.spriteBatch.Draw(bloom, drawPosition, null, 
        flareColorB with { A = 0 } * flareOpacity * 0.54f, 0f, 
        bloom.Size() * 0.5f, flareScale, 0, 0f);
    
    // Layer 3: Cross flare
    Main.spriteBatch.Draw(flare, drawPosition, null, 
        flareColorC with { A = 0 } * flareOpacity, flareRotation, 
        flare.Size() * 0.5f, flareScale, 0, 0f);
}
```

---

## Segment Bloom (Worm Bodies)

For Hades body segments:

```csharp
public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color lightColor)
{
    Texture2D bloom = MiscTexturesRegistry.BloomCircleSmall.Value;
    
    // Bloom intensity based on segment state
    float bloomOpacity = SegmentOpenInterpolant.Squared() * glowmaskOpacity * 0.56f;
    Vector2 bloomDrawPosition = TurretPosition - screenPos;
    
    // Red glow for turret
    Main.spriteBatch.Draw(bloom, bloomDrawPosition * positionScale, null, 
        NPC.GetAlpha(Color.Red with { A = 0 }) * bloomOpacity, 0f, 
        bloom.Size() * 0.5f, NPC.scale * 0.4f, 0, 0f);
    
    // White inner core
    Main.spriteBatch.Draw(bloom, bloomDrawPosition * positionScale, null, 
        NPC.GetAlpha(Color.Wheat with { A = 0 }) * bloomOpacity * 0.5f, 0f, 
        bloom.Size() * 0.5f, NPC.scale * 0.2f, 0, 0f);
    
    return false;  // Skip default drawing
}
```

---

## DrawBackBloom Helper Pattern

```csharp
public void DrawBackBloom()
{
    Vector2 drawPosition = Projectile.Center - Main.screenPosition;
    Texture2D bloom = MiscTexturesRegistry.BloomCircleSmall.Value;
    
    float bloomScale = Projectile.scale * 2f;
    Color outerBloomColor = new Color(0.2f, 0.1f, 0.4f);  // Deep purple
    
    // Outer large bloom
    Main.spriteBatch.Draw(bloom, drawPosition, null, 
        Projectile.GetAlpha(outerBloomColor) * 0.4f, 0f, 
        bloom.Size() * 0.5f, bloomScale * 5f, 0, 0f);
    
    // Inner white bloom
    Main.spriteBatch.Draw(bloom, drawPosition, null, 
        Projectile.GetAlpha(new Color(1f, 1f, 1f, 0f)) * 0.5f, 0f, 
        bloom.Size() * 0.5f, bloomScale * 2f, 0, 0f);
}
```

---

## MagnumOpus Adaptation Checklist

### For Each Theme, Create:

1. **Primary Bloom Color** - Main glow color
2. **Secondary Bloom Color** - Inner/accent glow
3. **Flare Color** - For shine effects
4. **Scale Factors** - Theme-appropriate sizes

### Example Theme Bloom Colors:

```csharp
// Fate Theme
Color fateOuterBloom = new Color(180, 50, 100) with { A = 0 };  // Dark pink
Color fateInnerBloom = Color.White with { A = 0 };               // White core
Color fateFlare = new Color(255, 60, 80) with { A = 0 };        // Bright red accent

// La Campanella Theme  
Color campOuterBloom = new Color(255, 100, 0) with { A = 0 };   // Orange
Color campInnerBloom = new Color(255, 200, 50) with { A = 0 };  // Yellow
Color campFlare = Color.White with { A = 0 };                    // White hot

// Moonlight Sonata Theme
Color moonOuterBloom = new Color(75, 0, 130) with { A = 0 };    // Deep purple
Color moonInnerBloom = new Color(135, 206, 250) with { A = 0 }; // Light blue
Color moonFlare = new Color(220, 220, 235) with { A = 0 };      // Silver
```

---

*Extracted from FargosSoulsDLC for MagnumOpus VFX development reference.*
