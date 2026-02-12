# MagnumOpus VFX Design Document
## Inspired by Calamity Mod's Visual Systems

> **Purpose**: This document outlines the design and implementation strategy for bringing Calamity-tier visual effects into MagnumOpus, specifically focusing on massive laser beams, cosmic constellation effects, melee smear trails, and chromatic aberration distortions.

---
## 🚨🚨🚨 CRITICAL: READ TRUE_VFX_STANDARDS.md FIRST 🚨🚨🚨

> **Before implementing ANY effects from this document, read [../Guides/TRUE_VFX_STANDARDS.md](../Guides/TRUE_VFX_STANDARDS.md).**
>
> **Gold Standard = Calamity Mod Source Code** (Exoblade, Ark of the Cosmos, Galaxia, Photoviscerator, The Oracle, Scarlet Devil)
> - **Multi-Layer Bloom Stack**: `{ A = 0 }` pattern with 4 layers (Outer 0.30, Mid 0.50, Inner 0.70, Core 0.85)
> - **3-Pass Trail Rendering**: `EnhancedTrailRenderer` or `CalamityStyleTrailRenderer` with width taper and color gradient
> - **CurveSegment Piecewise Animation**: Multi-phase swing arcs with PolyIn/PolyOut easing
> - **Sub-Pixel Interpolation**: `InterpolatedRenderer.PartialTicks` for 144Hz+ smoothness
> - **Velocity-Based VFX**: Stretch + spin scaling with speed
>
> The techniques in this document should SUPPLEMENT those Calamity-grounded standards, not replace them.

---
## Table of Contents

1. [Massive Sweeping Laser Beams](#1-massive-sweeping-laser-beams)
2. [Cosmic Constellation System](#2-cosmic-constellation-system)
3. [Melee Smear Trail Effects](#3-melee-smear-trail-effects)
4. [Chromatic Aberration & Visual Distortions](#4-chromatic-aberration--visual-distortions)
5. [Particle System Extensions](#5-particle-system-extensions)
6. [Implementation Priority](#6-implementation-priority)

---

## 1. Massive Sweeping Laser Beams

### Reference: `CalamityMod/Projectiles/Boss/ProvidenceHolyRay.cs`

The Profaned Guardian's massive golden laser beam is one of Calamity's most iconic boss attacks. It features:

### 1.1 Core Architecture

**Base Class Pattern:**
```
ProvidenceHolyRay : BaseLaserbeamProjectile
```

The laser inherits from a reusable base class that handles:
- Collision detection via `Collision.LaserScan()`
- Length calculation with tile collision
- Basic projectile lifecycle

**Key Properties:**
| Property | Value | Purpose |
|----------|-------|---------|
| `MaxTime` | 90 frames | Total laser duration |
| `MaxLaserLength` | 2400 pixels | Maximum beam reach |
| `RotationalSpeed` | Stored in `ai[0]` | Sweeping rotation per frame |
| `LaserOverlayColor` | `new Color(255, 255, 150, 100)` | Golden transparency overlay |

### 1.2 Three-Part Texture System

The laser uses THREE separate textures drawn in sequence:

```
[Start Cap] ─────────── [Mid Segment × N] ─────────── [End Cap]
     ↓                        ↓                           ↓
ProvidenceHolyRay.png   ProvidenceHolyRayMid.png   ProvidenceHolyRayEnd.png
```

**Drawing Logic (from PreDraw):**
```csharp
// 1. Calculate laser length via tile collision
float determinedLength = Collision.LaserScan(
    startPos, 
    direction, 
    bodyWidth * scale, 
    maxLength, 
    ignoreTiles);

// 2. Draw body segments in a loop
for (float i = 0; i < determinedLength; i += bodyTexture.Height)
{
    Vector2 segmentPos = startPos + direction * i;
    spriteBatch.Draw(bodyTexture, segmentPos, sourceRect, color, rotation, origin, scale, effects, 0);
}

// 3. Draw end cap at terminus
Vector2 endPos = startPos + direction * determinedLength;
spriteBatch.Draw(endTexture, endPos, null, color, rotation, endOrigin, scale, effects, 0);
```

### 1.3 Animation & Scale Pulsing

The beam "breathes" with a sine-based scale animation:

```csharp
// Scale oscillates over time creating a pulsing effect
float scaleAddition = (float)Math.Sin(Projectile.localAI[0] * MathHelper.Pi / 180f) * 10f;
Projectile.scale = 1f + scaleAddition * 0.01f;

// Local timer increments for animation
Projectile.localAI[0]++;
```

### 1.4 Rotational Sweeping

The laser sweeps across an arc via incremental rotation:

```csharp
// In AI():
Projectile.velocity = Projectile.velocity.RotatedBy(Projectile.ai[0]);
// Where ai[0] is the rotation speed (e.g., 0.008f for slow sweep, 0.02f for fast)
```

**Boss spawns laser with rotation parameters:**
```csharp
// From ProfanedGuardianCommander.cs
Projectile.NewProjectile(
    source, 
    position, 
    direction, 
    ModContent.ProjectileType<ProvidenceHolyRay>(),
    damage,
    knockback,
    Main.myPlayer,
    rotationSpeed,  // ai[0] - rotation increment per frame
    npcIndex        // ai[1] - parent NPC to follow
);
```

### 1.5 Burn Intensity Afterimage Effect

The boss draws multiple offset copies for a "heat distortion" look:

```csharp
// From ProfanedGuardianCommander.cs PreDraw
for (int i = 0; i < offsetCount; i++)
{
    float angle = MathHelper.TwoPi * i / offsetCount;
    Vector2 offset = angle.ToRotationVector2() * burnIntensity;
    Color afterimageColor = NPC.GetAlpha(drawColor) * MathHelper.Lerp(0.5f, 0.2f, burnIntensity / 30f);
    
    spriteBatch.Draw(texture, drawPos + offset, frame, afterimageColor, rotation, origin, scale, effects, 0);
}
```

### 1.6 MagnumOpus Implementation: Fate Cosmic Laser

**Theme Adaptation:**
- **Color Palette**: Dark pink → Crimson → White core (Fate's dark prismatic)
- **Texture Style**: Cosmic energy with star speckles, not holy fire
- **Distortion**: Chromatic aberration along edges instead of burn glow
- **Sound**: Deep resonant hum, not crackling fire

**Proposed Classes:**
```
Common/Systems/Projectiles/
├── BaseLaserBeamProjectile.cs    (Reusable base class)
└── Fate/
    ├── CosmicJudgmentRay.cs      (Boss laser)
    ├── CosmicJudgmentRay.png     (Start texture)
    ├── CosmicJudgmentRayMid.png  (Body texture - 4 frame animation)
    └── CosmicJudgmentRayEnd.png  (End cap texture)
```

**Key Differences from Providence:**
1. **Chromatic aberration** on the beam edges (RGB separation)
2. **Constellation sparkles** along the beam length
3. **Reality tear** visual at the origin point
4. **Screen distortion pulse** on firing

---

## 2. Cosmic Constellation System

### Reference: `CalamityMod/Projectiles/Melee/ArkoftheCosmos_Constellation.cs`

This is the signature effect of Ark of the Cosmos - a procedurally generated constellation that connects stars with glowing lines.

### 2.1 Core Architecture

**The Constellation Projectile:**
- Invisible projectile (`InvisibleProj` texture)
- Manages its own `List<Particle>` for stars and lines
- Regenerates constellation pattern every N frames
- Draws in `PreDraw` with additive blending

```csharp
public class ArkoftheCosmosConstellation : ModProjectile
{
    public List<Particle> Particles;
    const float ConstellationSwapTime = 15; // Regenerate every 15 frames
    
    Vector2 AnchorStart => Owner.Center;
    Vector2 AnchorEnd => Owner.Calamity().mouseWorld;
    public Vector2 SizeVector => Utils.SafeNormalize(AnchorEnd - AnchorStart, Vector2.Zero) 
        * MathHelper.Clamp((AnchorEnd - AnchorStart).Length(), 0, MaxThrowReach);
}
```

### 2.2 Particle Types Used

**GenericSparkle - Star Nodes:**
```csharp
// Creates a 4-pointed star with bloom backing
Particle Star = new GenericSparkle(
    position,           // Star location
    Vector2.Zero,       // No velocity (stationary)
    Color.White,        // Core color
    Color.Plum,         // Bloom color
    Main.rand.NextFloat(1f, 1.5f),  // Scale variation
    20,                 // Lifetime
    0f,                 // Rotation speed
    3f                  // Bloom scale
);
```

**BloomLineVFX - Connecting Lines:**
```csharp
// Creates a glowing line between two points
Particle Line = new BloomLineVFX(
    startPoint,         // Line start
    endPoint - startPoint,  // Line vector (direction + length)
    0.8f,               // Thickness
    constellationColor * 0.75f,  // Slightly transparent
    20,                 // Lifetime
    true,               // Capped ends (rounded)
    false               // Not a telegraph
);
```

### 2.3 Constellation Generation Algorithm

```csharp
// From AI() - generates random constellation pattern
if (Timer % ConstellationSwapTime == 0 && Projectile.timeLeft >= 20)
{
    Particles.Clear();
    
    float constellationColorHue = Main.rand.NextFloat();
    Color constellationColor = Main.hslToRgb(constellationColorHue, 1, 0.8f);
    Vector2 previousStar = AnchorStart;
    
    // First star at origin
    Particle Star = new GenericSparkle(previousStar, Vector2.Zero, Color.White, Color.Plum, ...);
    BootlegSpawnParticle(Star);
    
    // Generate intermediate stars along the path
    for (float i = 0 + Main.rand.NextFloat(0.2f, 0.5f); i < 1; i += Main.rand.NextFloat(0.2f, 0.5f))
    {
        // Shift hue for rainbow effect
        constellationColorHue = (constellationColorHue + 0.16f) % 1;
        constellationColor = Main.hslToRgb(constellationColorHue, 1, 0.8f);
        
        // Random perpendicular offset for organic look
        Vector2 offset = Main.rand.NextFloat(-50f, 50f) 
            * Utils.SafeNormalize(SizeVector.RotatedBy(MathHelper.PiOver2), Vector2.Zero);
        
        // Create star
        Star = new GenericSparkle(AnchorStart + SizeVector * i + offset, ...);
        BootlegSpawnParticle(Star);
        
        // Create connecting line
        Line = new BloomLineVFX(previousStar, currentPos - previousStar, 0.8f, color, ...);
        BootlegSpawnParticle(Line);
        
        // Optional: Branch lines (30% chance)
        if (Main.rand.NextBool(3))
        {
            // Create branch with shifted color
            ...
        }
        
        previousStar = currentPos;
    }
    
    // Final star at destination
    Star = new GenericSparkle(AnchorStart + SizeVector, ...);
    BootlegSpawnParticle(Star);
    
    // Final connecting line
    Line = new BloomLineVFX(previousStar, AnchorStart + SizeVector - previousStar, ...);
    BootlegSpawnParticle(Line);
}
```

### 2.4 Hue Cycling Effect

The constellation cycles through colors over time:

```csharp
// In particle update loop
foreach (Particle particle in Particles)
{
    // Gradually shift hue for prismatic effect
    particle.Color = Main.hslToRgb(
        Main.rgbToHsl(particle.Color).X + 0.02f,  // Shift hue
        Main.rgbToHsl(particle.Color).Y,           // Keep saturation
        Main.rgbToHsl(particle.Color).Z            // Keep lightness
    );
}
```

### 2.5 Custom Drawing with Additive Blending

```csharp
public override bool PreDraw(ref Color lightColor)
{
    if (Particles != null)
    {
        // Switch to additive blend for glow effect
        Main.spriteBatch.EnterShaderRegion(BlendState.Additive);
        
        foreach (Particle particle in Particles)
            particle.CustomDraw(Main.spriteBatch);
        
        Main.spriteBatch.ExitShaderRegion();
    }
    return false; // Don't draw default sprite
}
```

### 2.6 ConstellationRingVFX - Orbiting Stars

```csharp
// Creates a ring of stars orbiting a point (used by Galaxia weapons)
ConstellationRingVFX ring = new ConstellationRingVFX(
    position,           // Center point
    Color.DarkOrchid,   // Star color
    direction.ToRotation(),  // Ring orientation
    scale * 0.25f,      // Ring size
    new Vector2(0.5f, 1f),   // Squish (ellipse ratio)
    spinSpeed: 7,       // Rotation speed
    starAmount: 3 + i,  // Number of stars
    important: true     // Bypass particle limit
);
```

### 2.7 MagnumOpus Implementation: Fate Constellation System

**Proposed Particles:**
```
Common/Systems/Particles/
├── FateStarSparkle.cs      (4-pointed star with cosmic bloom)
├── FateBloomLine.cs        (Glowing line connector)
├── FateConstellationRing.cs (Orbiting star formation)
└── Textures/
    ├── Sparkle.png          (White 4-pointed star)
    ├── BloomCircle.png      (Soft circular glow)
    ├── BloomLine.png        (Vertical gradient line)
    └── BloomLineCap.png     (Rounded line end)
```

**Color Scheme:**
```csharp
// Fate cosmic gradient for constellations
Color.Lerp(FateWhite, FateDarkPink, progress * 0.5f)
// Then cycle hue for rainbow shimmer at edges
```

**Integration Points:**
- Fate melee weapon charge-up effects
- Fate projectile trails
- Fate boss attack telegraphs
- Fate accessory ambient auras

---

## 3. Melee Smear Trail Effects

### Reference: `CalamityMod/Projectiles/Melee/ArkOfTheCosmos_SwungBlade.cs`

### 3.1 CircularSmearVFX System

Calamity uses specialized "smear" particles that create the iconic sword trail:

**CircularSmearSmokeyVFX:**
```csharp
// Creates a smoky circular arc trail
CircularSmearSmokeyVFX smear = new CircularSmearSmokeyVFX(
    Owner.Center,           // Pivot point
    currentColor,           // Trail color
    Projectile.rotation,    // Current angle
    Projectile.scale * 2.4f // Trail size
);
GeneralParticleHandler.SpawnParticle(smear);
```

**Smear Update Logic:**
```csharp
// Update smear to follow swing
smear.Rotation = Projectile.rotation + MathHelper.PiOver4 + (Owner.direction < 0 ? MathHelper.PiOver4 * 4f : 0f);
smear.Time = 0;  // Reset lifetime to keep it visible
smear.Position = Owner.Center;
smear.Scale = MathHelper.Lerp(2.6f, 3.5f, (Projectile.scale - 1.6f) / 1f);
smear.Color = currentColor;
```

### 3.2 Texture Reference

The smear uses `TrientCircularSmear.png` - a semicircular gradient texture:
```
CalamityMod/Particles/TrientCircularSmear.png
```

### 3.3 HSL Color Cycling

```csharp
// Dynamic color based on swing progress
Color smearColor = Main.hslToRgb(
    ((SwingTimer - MaxSwingTime * 0.5f) / (MaxSwingTime * 0.5f)) * 0.15f, // Hue shifts during swing
    1,      // Full saturation
    0.8f    // Bright
);
```

### 3.4 Glitter/Sparkle Particles During Swing

```csharp
// CritSpark particles orbit around the swing
float maxDistance = Projectile.scale * 78f;
Vector2 distance = Main.rand.NextVector2Circular(maxDistance, maxDistance);
Vector2 angularVelocity = Utils.SafeNormalize(
    distance.RotatedBy(MathHelper.PiOver2 * Owner.direction), 
    Vector2.Zero
) * 2 * (1f + distance.Length() / 15f);

Particle glitter = new CritSpark(
    Owner.Center + distance,
    Owner.velocity + angularVelocity,
    Main.rand.NextBool(3) ? Color.Turquoise : Color.Coral,
    currentColor,
    1f + 1 * (distance.Length() / maxDistance),
    10,     // Lifetime
    0.05f,  // Fade rate
    3f      // Bloom scale
);
GeneralParticleHandler.SpawnParticle(glitter);
```

### 3.5 Heavy Smoke for Atmosphere

```csharp
// Smoky trails during powerful swings
HeavySmokeParticle smokeGlow = new HeavySmokeParticle(
    smokepos,
    smokespeed,
    Main.rand.NextBool(5) ? Color.Gold : Color.Chocolate,
    5,                              // Lifetime
    scaleFactor * Main.rand.NextFloat(2f, 2.4f),
    Opacity * 2.5f,                 // Opacity
    0f,                             // Rotation
    true,                           // Glow
    0.004f,                         // Rotation speed
    true                            // Required (bypass limit)
);
GeneralParticleHandler.SpawnParticle(smokeGlow);
```

### 3.6 MagnumOpus Implementation: Fate Cosmic Smear

**Proposed Structure:**
```
Common/Systems/Particles/
├── FateCosmicSmear.cs       (Circular smear with chromatic edges)
├── FateSwingGlitter.cs      (Orbiting sparkle particles)
└── Textures/
    ├── CosmicSmear.png      (Semicircular gradient with star speckles)
    └── CosmicSmearSmokey.png (Softer version with nebula texture)
```

**Visual Characteristics:**
- Base smear in dark pink/crimson
- Chromatic aberration at the edges (RGB split)
- Constellation sparkles trailing behind
- Reality tear effect at swing apex
- Screen shake ONLY on charged release

---

## 4. Chromatic Aberration & Visual Distortions

### Reference: `CalamityMod/Utilities/DrawingUtils.cs`

### 4.1 DrawChromaticAberration Utility

This is the core function for RGB color separation effects:

```csharp
public delegate void ChromaAberrationDelegate(Vector2 offset, Color colorMult);

public static void DrawChromaticAberration(Vector2 direction, float strength, ChromaAberrationDelegate drawCall)
{
    for (int i = -1; i <= 1; i++)
    {
        Color aberrationColor = Color.White;
        switch (i)
        {
            case -1:
                aberrationColor = new Color(255, 0, 0, 0);   // Red channel only
                break;
            case 0:
                aberrationColor = new Color(0, 255, 0, 0);   // Green channel only
                break;
            case 1:
                aberrationColor = new Color(0, 0, 255, 0);   // Blue channel only
                break;
        }

        // Offset perpendicular to direction
        Vector2 offset = direction.RotatedBy(MathHelper.PiOver2) * i;
        offset *= strength;

        drawCall.Invoke(offset, aberrationColor);
    }
}
```

### 4.2 Usage Example (Wulfrum Screw)

```csharp
// From WulfrumScrew.cs PreDraw
CalamityUtils.DrawChromaticAberration(Vector2.UnitX, 3f, delegate (Vector2 offset, Color colorMod)
{
    Main.EntitySpriteDraw(
        tex, 
        Projectile.Center - Main.screenPosition + offset, 
        null, 
        Color.GreenYellow.MultiplyRGB(colorMod) * opacity, 
        Projectile.rotation, 
        tex.Size() / 2f, 
        Projectile.scale, 
        SpriteEffects.None, 
        0
    );
});
```

### 4.3 Combining with Primitive Trails

```csharp
// From WulfrumDroid.cs - chromatic trail
CalamityUtils.DrawChromaticAberration(Vector2.UnitX, 1.8f, delegate (Vector2 offset, Color colorMod)
{
    PrimColorMult = colorMod;
    PrimitiveRenderer.RenderTrail(
        drawPos, 
        new PrimitiveSettings(WidthFunction, ColorFunction, (_) => offset, shader: GameShaders.Misc["CalamityMod:TrailStreak"]), 
        30
    );
});
```

### 4.4 Screen Shader Effects

**ScreenShaderData Pattern:**
```csharp
// From various screen shaders
public class FateScreenShaderData : ScreenShaderData
{
    public FateScreenShaderData(string passName) : base(passName) { }

    public override void Apply()
    {
        UseTargetPosition(bossCenter);  // Effect centered on boss
        base.Apply();
    }

    public override void Update(GameTime gameTime)
    {
        // Deactivate when boss dies
        if (!bossActive)
            Filters.Scene["MagnumOpus:Fate"].Deactivate();
    }
}
```

**Registering Screen Filters:**
```csharp
// In Mod.Load()
Filters.Scene["MagnumOpus:Fate"] = new Filter(
    new FateScreenShaderData("FilterMiniTower")
        .UseColor(0.9f, 0.3f, 0.5f)  // Pink tint
        .UseOpacity(0.6f),
    EffectPriority.VeryHigh
);
```

### 4.5 Heat Distortion Shader

```csharp
// From DrunkCrabulon filter setup
Texture2D DistortionTexture = ModContent.Request<Texture2D>(
    "CalamityMod/ExtraTextures/GreyscaleGradients/BlobbyNoise", 
    AssetRequestMode.ImmediateLoad
).Value;

Filters.Scene["CalamityMod:DrunkCrabulon"] = new Filter(
    new DrunkCrabScreenShaderData("FilterHeatDistortion")
        .UseImage(DistortionTexture, 0, null)
        .UseIntensity(20f),
    EffectPriority.VeryHigh
);
```

### 4.6 MagnumOpus Implementation: Reality Distortion Effects

**Proposed Utility Class:**
```csharp
// Common/Systems/MagnumDistortionUtils.cs
public static class MagnumDistortionUtils
{
    // Chromatic aberration for Fate theme
    public static void DrawWithChromaticAberration(
        Vector2 direction, 
        float strength, 
        Action<Vector2, Color> drawCall)
    {
        // Red channel (-1)
        drawCall(direction.RotatedBy(MathHelper.PiOver2) * -strength, new Color(255, 0, 0, 0));
        // Green channel (0)
        drawCall(Vector2.Zero, new Color(0, 255, 0, 0));
        // Blue channel (+1)
        drawCall(direction.RotatedBy(MathHelper.PiOver2) * strength, new Color(0, 0, 255, 0));
    }
    
    // Reality slice effect
    public static void DrawRealitySlice(Vector2 start, Vector2 end, float intensity)
    {
        // Draw white core line
        // Draw chromatic offset lines
        // Add sparkle particles at endpoints
    }
    
    // Screen fragment effect
    public static void TriggerRealityShatter(Vector2 center, float duration)
    {
        // Activate screen shader
        // Spawn fragment particles
        // Add screen shake
    }
}
```

---

## 5. Particle System Extensions

### 5.1 Required New Particles

| Particle | Purpose | Texture |
|----------|---------|---------|
| `FateStarSparkle` | Constellation nodes | 4-pointed star + bloom |
| `FateBloomLine` | Star connections | Vertical gradient |
| `FateCosmicSmear` | Melee swing trail | Semicircular gradient |
| `FateNebulaMist` | Ambient cosmic fog | Soft cloud |
| `FateRealityTear` | Dimension crack | Jagged line |
| `FateCritSpark` | Impact glitter | Stretched spark |

### 5.2 Texture Requirements

**New Textures Needed (in `Assets/Particles/`):**

```
Sparkle.png          - 64x64 white 4-pointed star, soft edges
BloomCircle.png      - 64x64 white radial gradient, very soft
BloomLine.png        - 8x64 white vertical gradient (bright at center)
BloomLineCap.png     - 16x16 white semicircle
CosmicSmear.png      - 256x128 semicircular arc gradient
HollowCircleSoftEdge.png - 64x64 ring with soft edges
Light.png            - 32x32 soft white glow
ThinSparkle.png      - 64x16 stretched diamond shape
```

### 5.3 Drawing Utilities

**Additive Blend Helper:**
```csharp
public static void EnterAdditiveBlend(this SpriteBatch spriteBatch)
{
    spriteBatch.End();
    spriteBatch.Begin(
        SpriteSortMode.Deferred, 
        BlendState.Additive, 
        Main.DefaultSamplerState, 
        DepthStencilState.None, 
        Main.Rasterizer, 
        null, 
        Main.GameViewMatrix.TransformationMatrix
    );
}

public static void ExitAdditiveBlend(this SpriteBatch spriteBatch)
{
    spriteBatch.End();
    spriteBatch.Begin(
        SpriteSortMode.Deferred, 
        BlendState.AlphaBlend, 
        Main.DefaultSamplerState, 
        DepthStencilState.None, 
        Main.Rasterizer, 
        null, 
        Main.GameViewMatrix.TransformationMatrix
    );
}
```

---

## 6. Implementation Priority

### Phase 1: Core Infrastructure (Required First)
1. **BaseLaserBeamProjectile.cs** - Reusable laser base class
2. **FateBloomLine.cs** - Line particle for constellations
3. **FateStarSparkle.cs** - Star particle for constellations
4. **MagnumDistortionUtils.cs** - Chromatic aberration utility
5. Required textures (Sparkle, BloomCircle, BloomLine, BloomLineCap)

### Phase 2: Constellation System
1. **FateConstellation.cs** - Procedural constellation projectile
2. **FateConstellationRing.cs** - Orbiting star formation
3. Integration with existing Fate weapons

### Phase 3: Laser Beam System
1. **CosmicJudgmentRay.cs** - Fate boss laser
2. Laser textures (Start, Mid, End)
3. Integration with Fate boss attacks

### Phase 4: Melee Smear System
1. **FateCosmicSmear.cs** - Circular smear particle
2. **FateSwingGlitter.cs** - Orbiting swing sparkles
3. Smear textures (CosmicSmear.png)
4. Integration with Fate melee weapons

### Phase 5: Advanced Effects
1. **FateRealityTear.cs** - Dimension crack particle
2. Screen shader for Fate boss fights
3. Heat distortion/reality warp effects

---

## Summary: Key Takeaways from Calamity

### Laser Beams
- Use 3-part textures (start, mid, end)
- Sine-based scale pulsing for "breathing" effect
- Store rotation speed in `ai[0]`, parent NPC in `ai[1]`
- Draw multiple offset copies for burn/heat distortion

### Constellations
- Invisible projectile manages particle list
- Generate random star positions along a vector
- Connect with BloomLineVFX particles
- Shift hue over time for rainbow effect
- Draw with additive blending

### Melee Smears
- Use CircularSmear textures drawn at swing rotation
- Reset particle lifetime each frame to maintain visibility
- Add glitter particles with angular velocity around pivot
- HSL color shifts based on swing progress

### Chromatic Aberration
- Draw same thing 3 times with RGB color masks
- Offset each draw perpendicular to main direction
- Combine with primitive trails for extra flair

### Screen Effects
- Extend ScreenShaderData for custom filters
- Use noise textures for distortion
- Center effects on boss position
- Deactivate when boss dies

---

*This document serves as the technical foundation for implementing Calamity-tier visual effects in MagnumOpus. Each section provides the exact code patterns used by Calamity that can be adapted for the Fate theme's unique identity.*
