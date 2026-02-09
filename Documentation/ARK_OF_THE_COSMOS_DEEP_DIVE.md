# ARK OF THE COSMOS - Complete Technical Deep Dive

> **Source:** Calamity Mod Public Repository
> **Branch:** 1.4.4
> **Analysis Date:** February 7, 2026

---

## 📋 TABLE OF CONTENTS

1. [Overview & Architecture](#overview--architecture)
2. [Key Files Reference](#key-files-reference)
3. [Swing Animation System (CurveSegment)](#swing-animation-system-curvesegment)
4. [Circular Smear VFX System](#circular-smear-vfx-system)
5. [Primitive Trail Rendering](#primitive-trail-rendering)
6. [EonBolt Homing Projectile](#eonbolt-homing-projectile)
7. [Constellation System](#constellation-system)
8. [Particle Effects](#particle-effects)
9. [Color & Gradient System](#color--gradient-system)
10. [Implementation Checklist](#implementation-checklist)

---

## Overview & Architecture

The **Ark of the Cosmos** is Calamity's premier endgame melee weapon. It creates stunning cosmic visual effects through a layered system of:

1. **Piecewise Animation** - Complex swing curves using `CurveSegment`
2. **Circular Smear Particles** - Nebula/smoke arc effects behind swings
3. **Primitive Trail Rendering** - GPU-rendered trails with shaders
4. **Constellation Lines** - Star connection effects between hit points
5. **Homing Projectiles (EonBolt)** - Pink/purple homing bolts with trails

### Visual Effect Breakdown (From Your Screenshots)

| Effect | Implementation |
|--------|----------------|
| **Constellation beam lines** | `ArkoftheCosmosConstellation.cs` - connects star particles with lines |
| **Smoky nebula swing arc** | `CircularSmearSmokeyVFX` particle with noise texture |
| **Pink/purple homing bolts** | `EonBolt.cs` with `HeavySmokeParticle` + primitive trails |
| **Star sparkles at nodes** | `Sparkle` texture + `BloomCircle` layered with additive blend |
| **Swing afterimages** | `oldPos`/`oldRot` arrays with HSL color gradient |

---

## Key Files Reference

### Main Weapon & Projectiles

| File | Purpose |
|------|---------|
| `Items/Weapons/Melee/ArkoftheCosmos.cs` | Main weapon item definition |
| `Projectiles/Melee/ArkOfTheCosmos_SwungBlade.cs` | **Core swing projectile** with all swing VFX |
| `Projectiles/Melee/ArkOfTheCosmos_BlastAttack.cs` | Scissor snap attack |
| `Projectiles/Melee/ArkoftheCosmos_Constellation.cs` | Constellation line effects |
| `Projectiles/Melee/EonBolt.cs` | Homing cosmic projectiles |

### VFX/Particle Systems

| File | Purpose |
|------|---------|
| `Particles/CircularSmearVFX.cs` | Base circular arc smear |
| `Particles/CircularSmearSmokeyVFX.cs` | **Nebula/smoky version** (key for cosmic look) |
| `Particles/TrientCircularSmearVFX.cs` | Third-circle smear variant |
| `Particles/SemiCircularSmearVFX.cs` | Half-circle smear |
| `Particles/ConstellationRing.cs` | Ring of stars particle |
| `Particles/HeavySmokeParticle.cs` | Dense smoke with glow |
| `Particles/GenericSparkle.cs` | Star sparkle with bloom |

### Core Utilities

| File | Purpose |
|------|---------|
| `Utilities/MathematicalUtils.cs` | `CurveSegment`, `PiecewiseAnimation`, `EasingType` |
| `Graphics/Primitives/PrimitiveRenderer.cs` | GPU trail rendering |
| `Utilities/DrawingUtils.cs` | `DrawAfterimagesCentered`, bloom helpers |

---

## Swing Animation System (CurveSegment)

### The `CurveSegment` Structure

```csharp
public struct CurveSegment
{
    public EasingFunction easing;    // The easing curve type
    public float startingX;          // When this segment starts (0-1)
    public float startingHeight;     // Starting value at this segment
    public float elevationShift;     // How much to change by end
    public int degree;               // Polynomial degree (for Poly types)
}
```

### Available Easing Types

```csharp
public enum EasingType
{
    Linear,
    SineIn, SineOut, SineInOut, SineBump,  // Smooth sine curves
    PolyIn, PolyOut, PolyInOut,            // Polynomial (uses degree)
    ExpIn, ExpOut, ExpInOut,               // Exponential
    CircIn, CircOut, CircInOut             // Circular
}
```

### Key Easing Functions

```csharp
// SineBump - Perfect for "overshoot and return" effects
// Creates a 0 → 1 → 0 curve over 0 → 1 input
public static float SineBumpEasing(float amount, int degree) 
    => (float)Math.Sin(amount * MathHelper.Pi);

// PolyOut - Fast start, slow end (great for swing acceleration)
public static float PolyOutEasing(float amount, int degree) 
    => 1f - (float)Math.Pow(1f - amount, degree);

// PolyIn - Slow start, fast end (anticipation)
public static float PolyInEasing(float amount, int degree) 
    => (float)Math.Pow(amount, degree);
```

### Ark of the Cosmos Swing Curves

From `ArkOfTheCosmos_SwungBlade.cs`:

```csharp
// Standard swing animation
public CurveSegment anticipation = new CurveSegment(EasingType.ExpOut, 0f, 0f, 0.15f);
public CurveSegment thrust = new CurveSegment(EasingType.PolyInOut, 0.1f, 0.15f, 0.85f, 3);
public CurveSegment hold = new CurveSegment(EasingType.Linear, 0.5f, 1f, 0.2f);
internal float SwingRatio() => PiecewiseAnimation(SwingCompletion, new CurveSegment[] { anticipation, thrust, hold });

// Wide "swirl" swing animation
public CurveSegment startup = new CurveSegment(EasingType.SineIn, 0f, 0f, 0.25f);
public CurveSegment swing = new CurveSegment(EasingType.SineOut, 0.1f, 0.25f, 0.75f);
internal float SwirlRatio() => PiecewiseAnimation(SwingCompletion, new CurveSegment[] { startup, swing });

// Throw/return animation
public CurveSegment shoot = new CurveSegment(EasingType.PolyIn, 0f, 1f, -0.2f, 3);
public CurveSegment remain = new CurveSegment(EasingType.Linear, SnapWindowStart, 0.8f, 0f);
public CurveSegment retract = new CurveSegment(EasingType.SineIn, SnapWindowEnd, 1f, -1f);
internal float ThrowRatio() => PiecewiseAnimation(ThrowCompletion, new CurveSegment[] { shoot, remain, retract });

// Size pulse during throw
public CurveSegment sizeCurve = new CurveSegment(EasingType.SineBump, 0f, 0f, 1f);
internal float ThrowScaleRatio() => PiecewiseAnimation(ThrowCompletion, new CurveSegment[] { sizeCurve });
```

### Using PiecewiseAnimation

```csharp
public static float PiecewiseAnimation(float progress, params CurveSegment[] segments)
{
    progress = MathHelper.Clamp(progress, 0f, 1f);
    float ratio = 0f;
    
    for (int i = 0; i <= segments.Length - 1; i++)
    {
        CurveSegment segment = segments[i];
        float startPoint = segment.startingX;
        float endPoint = (i < segments.Length - 1) ? segments[i + 1].startingX : 1f;
        
        if (progress < segment.startingX) continue;
        
        float segmentLength = endPoint - startPoint;
        float segmentProgress = (progress - segment.startingX) / segmentLength;
        ratio = segment.startingHeight + segment.easing(segmentProgress, segment.degree) * segment.elevationShift;
        break;
    }
    return ratio;
}
```

### Applying Swing Rotation

```csharp
// In AI()
public override void AI()
{
    // Baby swing
    if (!SwirlSwing)
    {
        Projectile.rotation = Projectile.velocity.ToRotation() + 
            MathHelper.Lerp(SwingWidth / 2 * SwingDirection, -SwingWidth / 2 * SwingDirection, SwingRatio());
    }
    // Chungal (wide) swing
    else
    {
        float startRot = (MathHelper.Pi - MathHelper.PiOver4) * SwingDirection;
        float endRot = -(MathHelper.TwoPi + MathHelper.PiOver4 * 1.5f) * SwingDirection;
        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.Lerp(startRot, endRot, SwirlRatio());
        DoParticleEffects(true);
    }
}
```

---

## Circular Smear VFX System

### The Core Smear Effect (The Nebula Cloud)

The large cosmic cloud you see during swings uses **`CircularSmearSmokeyVFX`** - a specialized particle that uses a **noise texture** to create the nebula effect.

```csharp
// CircularSmearSmokeyVFX.cs
public class CircularSmearSmokeyVFX : Particle
{
    public override string Texture => "CalamityMod/Particles/CircularSmearSmokey"; // Noise-based texture
    public override bool UseAdditiveBlend => true;
    public override bool SetLifetime => true;
    public float opacity;

    public CircularSmearSmokeyVFX(Vector2 position, Color color, float rotation, float scale)
    {
        Position = position;
        Velocity = Vector2.Zero;
        Color = color;
        Scale = scale;
        Rotation = rotation;
        Lifetime = 2; // Very short - updated each frame
    }
}
```

### Spawning & Updating the Smear

From `ArkOfTheCosmos_SwungBlade.DoParticleEffects()`:

```csharp
public void DoParticleEffects(bool swirlSwing)
{
    // During wide swings, create the smoky smear effect
    if (swirlSwing)
    {
        Color currentColor = Color.Lerp(Color.HotPink, Color.DarkViolet, 
            (float)Math.Sin(Main.GlobalTimeWrappedHourly * 2f));
        
        if (smear == null)
        {
            // First frame - spawn new smear
            smear = new CircularSmearSmokeyVFX(Owner.Center, currentColor, Projectile.rotation, Projectile.scale * 2.4f);
            GeneralParticleHandler.SpawnParticle(smear);
        }
        else
        {
            // Update existing smear each frame
            smear.Rotation = Projectile.rotation + MathHelper.PiOver4 + (Owner.direction < 0 ? MathHelper.PiOver4 * 4f : 0f);
            smear.Time = 0; // Reset lifetime to keep it alive
            smear.Position = Owner.Center;
            smear.Scale = MathHelper.Lerp(2.6f, 3.5f, (Projectile.scale - 1.6f) / 1f);
            smear.Color = currentColor;
        }
    }
}
```

### Smear Variants

| Class | Texture | Use Case |
|-------|---------|----------|
| `CircularSmearVFX` | `CircularSmear` | Base full-circle smear |
| `CircularSmearSmokeyVFX` | `CircularSmearSmokey` | **Nebula/noise version** |
| `TrientCircularSmear` | `TrientCircularSmear` | 1/3 circle arc |
| `SemiCircularSmearVFX` | `SemiCircularSmear` | Half-circle arc |
| `SemiCircularSmearFade` | `SemiCircularSmearVerticalBlank` | Fading half-circle |

### Drawing Smears in PreDraw

For static smear drawing in `PreDraw`:

```csharp
public override bool PreDraw(ref Color lightColor)
{
    if (SwingCompletion > 0.5f)
    {
        Texture2D smear = Request<Texture2D>("CalamityMod/Particles/TrientCircularSmear").Value;
        
        // Switch to additive blending
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, 
            Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, 
            Main.GameViewMatrix.TransformationMatrix);
        
        float opacity = (float)Math.Sin(SwingCompletion * MathHelper.Pi);
        float rotation = (-MathHelper.PiOver4 * 0.5f + MathHelper.PiOver4 * 0.5f * SwingCompletion) * SwingDirection;
        Color smearColor = Main.hslToRgb(
            ((SwingTimer - MaxSwingTime * 0.5f) / (MaxSwingTime * 0.5f)) * 0.15f + ((Combo == 1f) ? 0.85f : 0f), 
            1, 0.6f);
        
        Main.EntitySpriteDraw(smear, Owner.Center - Main.screenPosition, null, 
            smearColor * 0.5f * opacity, 
            Projectile.velocity.ToRotation() + MathHelper.Pi + rotation, 
            smear.Size() / 2f, 
            Projectile.scale * 2.3f, 
            SpriteEffects.None, 0);
        
        // Return to normal blending
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, 
            Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, 
            Main.GameViewMatrix.TransformationMatrix);
    }
}
```

---

## Primitive Trail Rendering

### Overview

Calamity uses a **GPU-based primitive renderer** for smooth trails. This is different from simple afterimages - it creates a continuous triangle strip mesh along the trail path.

### Key Components

```csharp
// PrimitiveSettings - defines trail appearance
public record struct PrimitiveSettings(
    WidthFunction WidthFunc,      // float(float completionRatio, Vector2 vertexPos)
    ColorFunction ColorFunc,       // Color(float completionRatio, Vector2 vertexPos)
    OffsetFunction? OffsetFunc,   // Vector2(float completionRatio, Vector2 vertexPos)
    bool smoothen = false,
    bool pixelate = false,
    MiscShaderData? shader = null
);
```

### EonBolt Trail Implementation

From `EonBolt.cs`:

```csharp
// Width function - cubic taper from base to tip
internal float WidthFunction(float completionRatio, Vector2 vertexPos)
{
    float expansionCompletion = (float)Math.Pow(1 - completionRatio, 3);
    return MathHelper.Lerp(0f, 22 * Projectile.scale * Projectile.Opacity, expansionCompletion);
}

// Color function - dynamic hue cycling with turquoise blend
internal Color ColorFunction(float completionRatio, Vector2 vertexPos)
{
    float fadeToEnd = MathHelper.Lerp(0.65f, 1f, 
        (float)Math.Cos(-Main.GlobalTimeWrappedHourly * 3f) * 0.5f + 0.5f);
    float fadeOpacity = Utils.GetLerpValue(1f, 0.64f, completionRatio, true) * Projectile.Opacity;
    Color colorHue = Main.hslToRgb(Hue, 1, 0.8f);
    
    Color endColor = Color.Lerp(colorHue, Color.PaleTurquoise, 
        (float)Math.Sin(completionRatio * MathHelper.Pi * 1.6f - Main.GlobalTimeWrappedHourly * 4f) * 0.5f + 0.5f);
    
    return endColor * fadeOpacity;
}

// In PreDraw - render the trail
public override bool PreDraw(ref Color lightColor)
{
    // Set shader texture
    GameShaders.Misc["CalamityMod:TrailStreak"].SetShaderTexture(
        Request<Texture2D>("CalamityMod/ExtraTextures/Trails/ScarletDevilStreak"));
    
    // Render trail with settings
    PrimitiveRenderer.RenderTrail(
        Projectile.oldPos, 
        new PrimitiveSettings(
            WidthFunction, 
            ColorFunction, 
            (_, _) => Projectile.Size * 0.5f, 
            shader: GameShaders.Misc["CalamityMod:TrailStreak"]
        ), 
        30 // Number of points
    );
    
    // Draw the bolt sprite on top
    Texture2D texture = Request<Texture2D>("CalamityMod/Projectiles/Melee/GalaxiaBolt").Value;
    Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, 
        Color.Lerp(lightColor, Color.White, 0.5f), 
        Projectile.rotation, texture.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);
    
    return false;
}
```

### Common Trail Shaders

| Shader | Texture | Effect |
|--------|---------|--------|
| `TrailStreak` | `ScarletDevilStreak` | Soft gradient streak |
| `ImpFlameTrail` | Various | Fire/energy trail |
| `SideStreakTrail` | `Perlin` | Side-fading streak |
| `FadingSolidTrail` | N/A | Solid fading color |

---

## EonBolt Homing Projectile

The pink/purple homing projectiles in your screenshots.

### Core Implementation

```csharp
public class EonBolt : ModProjectile, ILocalizedModType
{
    public NPC target;
    public Player Owner => Main.player[Projectile.owner];
    
    public ref float Hue => ref Projectile.ai[0];              // Color hue (0-1)
    public ref float HomingStrenght => ref Projectile.ai[1];   // How strongly it homes
    
    Particle Head; // Glowing particle at the bolt's head

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Type] = 40;  // Long trail
        ProjectileID.Sets.TrailingMode[Type] = 2;       // Store rotation too
    }

    public override void AI()
    {
        // Find target
        target = Projectile.Center.ClosestNPCAt(2000f);
        
        // Homing logic
        if (target != null && CalamityUtils.AngleBetween(Projectile.velocity, target.Center - Projectile.Center) < MathHelper.Pi)
        {
            float idealDirection = Projectile.AngleTo(target.Center);
            float updatedDirection = Projectile.velocity.ToRotation().AngleTowards(idealDirection, HomingStrenght);
            Projectile.velocity = updatedDirection.ToRotationVector2() * Projectile.velocity.Length() * 0.995f;
        }
        
        // Lighting
        Lighting.AddLight(Projectile.Center, 0.75f, 1f, 0.24f);
        
        // Dense smoke trail
        if (Main.rand.NextBool(2))
        {
            Particle smoke = new HeavySmokeParticle(
                Projectile.Center, 
                Projectile.velocity * 0.5f, 
                Color.Lerp(Color.DodgerBlue, Color.MediumVioletRed, 
                    (float)Math.Sin(Main.GlobalTimeWrappedHourly * 6f)), 
                20, 
                Main.rand.NextFloat(0.6f, 1.2f) * Projectile.scale, 
                0.28f, 
                0, 
                false, 
                0, 
                true
            );
            GeneralParticleHandler.SpawnParticle(smoke);
            
            // Occasional glowing smoke
            if (Main.rand.NextBool(3))
            {
                Particle smokeGlow = new HeavySmokeParticle(
                    Projectile.Center, 
                    Projectile.velocity * 0.5f, 
                    Main.hslToRgb(Hue, 1, 0.7f), 
                    15, 
                    Main.rand.NextFloat(0.4f, 0.7f) * Projectile.scale, 
                    0.8f, 
                    0, 
                    true,  // Glow mode
                    0.05f, 
                    true
                );
                GeneralParticleHandler.SpawnParticle(smokeGlow);
            }
        }
    }
}
```

---

## Constellation System

The star-connected line effects.

### ArkoftheCosmosConstellation.cs

```csharp
public class ArkoftheCosmosConstellation : ModProjectile, ILocalizedModType
{
    public List<Particle> Particles; // Stars and bloom effects
    
    public override void AI()
    {
        // Create star particles at constellation points
        if (Particles == null)
        {
            Particles = new List<Particle>();
            // Spawn stars at impact points...
        }
        
        // Update particles
        foreach (Particle particle in Particles)
            particle.Update();
    }
    
    public override bool PreDraw(ref Color lightColor)
    {
        if (Particles != null)
        {
            Main.spriteBatch.EnterShaderRegion(BlendState.Additive);
            
            foreach (Particle particle in Particles)
                particle.CustomDraw(Main.spriteBatch);
            
            Main.spriteBatch.ExitShaderRegion();
        }
        return false;
    }
}
```

### Drawing Constellation Lines (From LyraConstellation example)

```csharp
// Draw line between two star points
CalamityUtils.DrawLineBetter(
    Main.spriteBatch, 
    SiriusPos + point1.RotatedBy(Projectile.rotation) * Projectile.scale - (Owner.oldVelocity * Math.Clamp(point1.Length() * 0.001f, 0, 1)), 
    SiriusPos + point2.RotatedBy(Projectile.rotation) * Projectile.scale - (Owner.oldVelocity * Math.Clamp(point2.Length() * 0.001f, 0, 1)), 
    color, 
    3  // Line width
);
```

### ConstellationRingVFX (Star Ring)

```csharp
public class ConstellationRingVFX : Particle
{
    public override string Texture => "CalamityMod/Particles/HollowCircleSoftEdge";
    public override bool UseAdditiveBlend => true;
    
    public Vector2 Squish;
    public int StarAmount;
    public float StarScale;
    public float SpinSpeed;
    
    public override void CustomDraw(SpriteBatch spriteBatch)
    {
        Texture2D ringTexture = ModContent.Request<Texture2D>(Texture).Value;
        Texture2D starTexture = ModContent.Request<Texture2D>("CalamityMod/Particles/Sparkle").Value;
        Texture2D bloomTexture = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle").Value;
        
        // Draw the ring
        spriteBatch.Draw(ringTexture, Position - Main.screenPosition, null, 
            Color * Opacity, Rotation, ringTexture.Size() / 2f, Squish * Scale, SpriteEffects.None, 0);
        
        float time = Main.GlobalTimeWrappedHourly * SpinSpeed;
        float properBloomSize = (float)starTexture.Height / (float)bloomTexture.Height;
        
        // Draw orbiting stars
        for (int i = 0; i < StarAmount; i++)
        {
            float starHeight = (float)Math.Sin(Offset + time + i * MathHelper.TwoPi / (float)StarAmount);
            float starWidth = (float)Math.Cos(Offset + time + i * MathHelper.TwoPi / (float)StarAmount);
            Vector2 starPos = Position + 
                Rotation.ToRotationVector2() * starWidth * starPosOffsetX + 
                (Rotation + MathHelper.PiOver2).ToRotationVector2() * starHeight * starPosOffsetY;
            
            // Draw bloom behind star
            spriteBatch.Draw(bloomTexture, starPos - Main.screenPosition, null, 
                starColor, 0, bloomOrigin, bloomScale, SpriteEffects.None, 0);
            // Draw star
            spriteBatch.Draw(starTexture, starPos - Main.screenPosition, null, 
                starColor, Rotation + MathHelper.PiOver4 + MathHelper.PiOver4 * i, 
                starOrigin, starScale, SpriteEffects.None, 0);
        }
    }
}
```

---

## Particle Effects

### HeavySmokeParticle (Core Nebula Effect)

Used extensively for the cosmic cloud trail:

```csharp
Particle smoke = new HeavySmokeParticle(
    position,           // Spawn position
    velocity,           // Movement
    color,              // Base color
    lifetime,           // How long it lasts
    scale,              // Size
    opacity,            // Starting opacity
    rotation,           // Initial rotation
    glowMode,           // true = additive glow
    fadeFactor,         // How fast it fades
    noGravity           // true = floats
);
GeneralParticleHandler.SpawnParticle(smoke);
```

### GenericSparkle (Star Points)

```csharp
public class GenericSparkle : Particle
{
    public override string Texture => "CalamityMod/Particles/Sparkle";
    public override bool UseAdditiveBlend => true;
    
    private Color Bloom;      // Bloom color (usually same as Color but can differ)
    private float BloomScale; // How large the bloom is
    
    public override void CustomDraw(SpriteBatch spriteBatch)
    {
        Texture2D sparkTexture = ModContent.Request<Texture2D>(Texture).Value;
        Texture2D bloomTexture = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle").Value;
        
        float properBloomSize = (float)sparkTexture.Height / (float)bloomTexture.Height;
        
        // Draw bloom first (behind)
        spriteBatch.Draw(bloomTexture, Position - Main.screenPosition, null, 
            Bloom * opacity * 0.5f, 0, bloomTexture.Size() / 2f, 
            Scale * BloomScale * properBloomSize, SpriteEffects.None, 0);
        
        // Draw sparkle on top
        spriteBatch.Draw(sparkTexture, Position - Main.screenPosition, null, 
            Color * opacity, Rotation, sparkTexture.Size() / 2f, Scale, SpriteEffects.None, 0);
    }
}
```

### FlareShine (Bright Star Flare)

```csharp
new FlareShine(
    position, velocity,
    color,              // Main color
    bloom,              // Bloom color
    angle,              // Initial angle
    scale,              // Starting scale (Vector2)
    finalScale,         // Ending scale (Vector2)
    lifeTime,
    rotationSpeed,
    bloomScale,
    hueShift,           // Shift color over time
    spawnDelay
);
```

---

## Color & Gradient System

### Dynamic HSL Color Cycling

```csharp
// Cycle through hues over time
Color dynamicColor = Main.hslToRgb(
    (Main.GlobalTimeWrappedHourly * 0.5f) % 1f,  // Hue (0-1, cycling)
    1f,                                           // Saturation
    0.7f                                          // Lightness
);

// Lerp between two colors based on time
Color oscillatingColor = Color.Lerp(
    Color.HotPink, 
    Color.DarkViolet, 
    (float)Math.Sin(Main.GlobalTimeWrappedHourly * 2f) * 0.5f + 0.5f
);
```

### Trail Color Gradients

```csharp
// In ColorFunction for trails
internal Color ColorFunction(float completionRatio, Vector2 vertexPos)
{
    // Fade from start color to end color along the trail
    Color startColor = Main.hslToRgb(Hue, 1, 0.8f);
    Color endColor = Color.PaleTurquoise;
    
    // Oscillate blend based on position and time
    float blend = (float)Math.Sin(completionRatio * MathHelper.Pi * 1.6f - Main.GlobalTimeWrappedHourly * 4f) * 0.5f + 0.5f;
    
    // Fade opacity along trail
    float fadeOpacity = Utils.GetLerpValue(1f, 0.64f, completionRatio, true) * Projectile.Opacity;
    
    return Color.Lerp(startColor, endColor, blend) * fadeOpacity;
}
```

### Afterimage Color Gradient

```csharp
// In PreDraw - rainbow afterimages
for (int i = 0; i < Projectile.oldRot.Length; ++i)
{
    // Cycle through hues based on afterimage index
    Color color = Main.hslToRgb(
        (i / (float)Projectile.oldRot.Length) * 0.15f,  // Hue range
        1,                                               // Full saturation
        0.6f + (Charge > 0 ? 0.3f : 0f)                 // Brightness boost when charged
    );
    
    float afterimageRotation = Projectile.oldRot[i] + angleShift + extraAngle;
    Main.spriteBatch.Draw(glowmask, drawOffset, null, 
        color * 0.05f,  // Very transparent
        afterimageRotation, 
        drawOrigin, 
        Projectile.scale - 0.2f * (i / (float)Projectile.oldRot.Length), // Shrink with distance
        flip, 0f);
}
```

---

## Implementation Checklist

### Core Systems Needed

- [ ] **CurveSegment struct** with all easing types
- [ ] **PiecewiseAnimation function** for composing curves
- [ ] **CircularSmearVFX particle** (base version)
- [ ] **CircularSmearSmokeyVFX particle** (noise/nebula version)
- [ ] **Primitive trail renderer** or equivalent system
- [ ] **HeavySmokeParticle** for dense cosmic clouds
- [ ] **GenericSparkle** with bloom for star effects

### Textures Required

| Texture | Purpose |
|---------|---------|
| `CircularSmear.png` | Base arc smear |
| `CircularSmearSmokey.png` | Noise-based nebula smear |
| `TrientCircularSmear.png` | 1/3 circle variant |
| `Sparkle.png` | 4-pointed star |
| `BloomCircle.png` | Soft glow for bloom |
| `ScarletDevilStreak.png` | Trail shader texture |
| `HollowCircleSoftEdge.png` | Ring particle |

### Key Implementation Steps

1. **Implement `CurveSegment` and `PiecewiseAnimation`** in your utility class
2. **Create swing projectile** with `oldPos`/`oldRot` tracking
3. **Add smear particle system** with the smokey variant
4. **Implement primitive trails** or use afterimage-based approach
5. **Create homing projectile** with dense smoke particles
6. **Add constellation line drawing** for multi-hit effects
7. **Layer bloom effects** on all star particles

### Blending Setup

```csharp
// For additive glow effects
Main.spriteBatch.End();
Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, 
    Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, 
    Main.GameViewMatrix.TransformationMatrix);

// Draw additive stuff...

// Return to normal
Main.spriteBatch.End();
Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, 
    Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, 
    Main.GameViewMatrix.TransformationMatrix);
```

---

## Summary

The Ark of the Cosmos achieves its stunning visuals through:

1. **Complex swing curves** using `CurveSegment` piecewise animation
2. **Smoky circular smear particles** that update each frame to follow the swing
3. **Primitive trail rendering** with GPU shaders for smooth projectile trails
4. **Dense HeavySmokeParticle spawning** for cosmic cloud effects
5. **Layered bloom + sparkle particles** for star/constellation effects
6. **Dynamic HSL color cycling** for the signature pink/purple/turquoise palette
7. **Constellation line drawing** between hit/star points
8. **Additive blending** for all glow effects
