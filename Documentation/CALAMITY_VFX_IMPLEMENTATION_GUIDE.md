# Calamity VFX Implementation Guide for MagnumOpus

> **This document bridges the Ark of the Cosmos and Galaxia deep dives with MagnumOpus's existing systems.**
> 
> After extensive analysis, MagnumOpus ALREADY HAS the core systems to replicate Calamity's visual effects.
> This guide shows you exactly how to use them correctly.

---

## 📊 SYSTEM MAPPING: Calamity → MagnumOpus

| Calamity System | MagnumOpus Equivalent | Location |
|-----------------|----------------------|----------|
| `CircularSmearSmokeyVFX` | `CircularSmearSmokeyVFX` | `Common/Systems/Particles/SmearParticles.cs` |
| `SemiCircularSmear` | `SemiCircularSmearVFX` | `Common/Systems/Particles/SmearParticles.cs` |
| `TrientCircularSmear` | `TrientCircularSmear` | `Common/Systems/Particles/SmearParticles.cs` |
| `HeavySmokeParticle` | `HeavySmokeParticle` | `Common/Systems/Particles/CommonParticles.cs` |
| `CritSpark` | `CritSpark` | `Common/Systems/Particles/SmearParticles.cs` |
| `GenericSparkle` | `SparkleParticle` | `Common/Systems/Particles/CommonParticles.cs` |
| `CurveSegment` + `PiecewiseAnimation` | `CurveSegment` + `PiecewiseAnimation` | `Common/Systems/Particles/Particle.cs` |
| `PrimitiveSettings` | `EnhancedTrailRenderer.PrimitiveSettings` | `Common/Systems/VFX/EnhancedTrailRenderer.cs` |
| GPU Primitive Trails | `EnhancedTrailRenderer` | `Common/Systems/VFX/EnhancedTrailRenderer.cs` |
| Bloom Multi-Layer Stack | `BloomRenderer` | `Common/Systems/VFX/BloomRenderer.cs` |
| Constellation Lines | `VerletConstellationSystem` | `Common/Systems/VFX/VerletConstellationSystem.cs` |
| 5-Layer Nebula Trail | `CalamityNebulaTrail` | `Common/Systems/VFX/CalamityNebulaTrail.cs` |
| Nebula Fog Clouds | `LayeredNebulaFog` + `NebulaFogSystem` | `Common/Systems/VFX/` |
| Ark Swing Trails | `ArkSwingTrail` | `Common/Systems/VFX/ArkSwingTrail.cs` |
| Segment Animation (Worms) | `SegmentAnimator` | `Common/Systems/VFX/SegmentAnimator.cs` |

---

## 🔥 PATTERN 1: The Smear Persistence Pattern

### Calamity Pattern (from Ark of the Cosmos Deep Dive)
```csharp
// Store reference, reset Time=0 each frame to keep alive
if (smear == null) {
    smear = new CircularSmearSmokeyVFX(Owner.Center, color, direction.ToRotation(), scale);
    GeneralParticleHandler.SpawnParticle(smear);
} else {
    smear.Time = 0;  // CRITICAL: Keep alive
    smear.Position = Owner.Center;
    smear.Rotation = direction.ToRotation() + MathHelper.PiOver2;
}
```

### MagnumOpus Implementation
```csharp
using MagnumOpus.Common.Systems.Particles;

// In your weapon/projectile class:
private CircularSmearSmokeyVFX _smear;

public void UpdateSwingSmear(Player owner, Vector2 direction, Color color, float scale)
{
    if (_smear == null)
    {
        _smear = new CircularSmearSmokeyVFX(
            owner.Center, 
            color, 
            direction.ToRotation(), 
            scale
        );
        MagnumParticleHandler.SpawnParticle(_smear);
    }
    else
    {
        _smear.Time = 0;  // Reset to keep alive
        _smear.Position = owner.Center;
        _smear.Rotation = direction.ToRotation() + MathHelper.PiOver2;
        _smear.Color = color; // Update color if cycling
    }
}

// When swing ends:
public void EndSwingSmear()
{
    // Stop resetting Time - particle will die naturally
    _smear = null;
}
```

### Available Smear Variants

| Class | Arc Size | Best For |
|-------|----------|----------|
| `CircularSmearVFX` | 360° | Spinning attacks, full rotations |
| `CircularSmearSmokeyVFX` | 360° (smokey) | Nebula effects, cosmic weapons |
| `SemiCircularSmearVFX` | 180° | Standard sword swings |
| `SemiCircularSmearFade` | 180° (fading) | Transitional swings |
| `TrientCircularSmear` | 120° | Charged attacks, cosmic slashes |

---

## 🌫️ PATTERN 2: Dual-Layer Smoke (Galaxia Style)

### Calamity Pattern (from Galaxia Deep Dive)
```csharp
// Layer 1: Base smoke (normal blend, no glow)
new HeavySmokeParticle(pos, vel, color, lifetime, scale, opacity, rot, false);

// Layer 2: Glowing overlay (additive blend, hue shifted)
new HeavySmokeParticle(pos, vel, glowColor, lifetime, scale*0.8f, opacity*0.5f, rot, true);
```

### MagnumOpus Implementation

**EASIEST WAY: Use the `DualLayerSmoke` helper class:**
```csharp
using MagnumOpus.Common.Systems.Particles;

// One-line dual-layer smoke with proper Galaxia-style effect
DualLayerSmoke.Spawn(
    position, velocity, 
    baseColor: new Color(255, 100, 50),   // Base layer color
    glowColor: new Color(255, 180, 80),   // Glow overlay color
    lifetime: 40, scale: 0.5f,
    baseOpacity: 0.75f, glowOpacity: 0.4f,
    hueShift: 0.01f  // For flame-like color animation
);

// Theme-based variant (auto-selects colors):
DualLayerSmoke.SpawnThemed(position, velocity, "phoenix", lifetime: 40, scale: 0.5f, intensity: 1f);
DualLayerSmoke.SpawnThemed(position, velocity, "fate", lifetime: 35, scale: 0.6f, intensity: 1.2f);
```

**MANUAL WAY (for full control):**
```csharp
public void SpawnDualLayerSmoke(Vector2 position, Vector2 velocity, Color baseColor)
{
    // Calculate hue-shifted glow color
    Vector3 hsl = Main.rgbToHsl(baseColor);
    hsl.X = (hsl.X + 0.01f) % 1f; // Shift hue slightly
    Color glowColor = Main.hslToRgb(hsl.X, hsl.Y, hsl.Z);
    
    // Layer 1: Base smoke (non-glowing)
    var baseSmoke = new HeavySmokeParticle(
        position, 
        velocity,
        baseColor,
        lifetime: 35,
        scale: 1.2f,
        opacity: 0.8f,
        rotationSpeed: 0.02f,
        glowing: false,   // Normal blend
        hueShift: 0f      // No color animation on base layer
    );
    MagnumParticleHandler.SpawnParticle(baseSmoke);
    
    // Layer 2: Glowing overlay (additive blend with hue shift)
    var glowSmoke = new HeavySmokeParticle(
        position,
        velocity * 1.1f,
        glowColor,
        lifetime: 30,
        scale: 1.0f,
        opacity: 0.4f,
        rotationSpeed: 0.015f,
        glowing: true,    // Additive blend!
        hueShift: 0.01f   // Galaxia-style flame animation!
    );
    MagnumParticleHandler.SpawnParticle(glowSmoke);
}
```

---

## 🎨 PATTERN 3: Multi-Layer Bloom Stack

### Calamity/Fargos Pattern
```csharp
// 4 layers from largest (faint) to smallest (bright)
float[] scales = { 2.0f, 1.4f, 0.9f, 0.4f };
float[] opacities = { 0.3f, 0.5f, 0.7f, 0.85f };

// CRITICAL: Remove alpha channel for proper additive blending
Color bloomColor = baseColor with { A = 0 };

for (int i = 0; i < 4; i++)
{
    spriteBatch.Draw(tex, pos, null, bloomColor * opacities[i], 
        0, origin, scale * scales[i], SpriteEffects.None, 0);
}
```

### MagnumOpus Implementation
```csharp
using MagnumOpus.Common.Systems.VFX;

// Easy way - use BloomRenderer:
BloomRenderer.DrawBloomStack(
    Main.spriteBatch,
    worldPosition,
    primaryColor,
    scale: 1.5f,
    opacity: 1f
);

// Two-color gradient:
BloomRenderer.DrawBloomStack(
    Main.spriteBatch,
    worldPosition,
    outerColor: cosmicPurple,
    innerColor: Color.White,
    scale: 1.5f,
    opacity: 1f
);

// Pulsing bloom:
BloomRenderer.DrawPulsingBloom(
    Main.spriteBatch,
    worldPosition,
    baseColor,
    scale: 1f,
    opacity: 1f,
    pulseSpeed: 3f,
    pulseIntensity: 0.3f
);
```

---

## 📈 PATTERN 4: CurveSegment Animation System

### The CurveSegment System (Already in MagnumOpus!)

Both Ark of the Cosmos and Galaxia use `CurveSegment` + `PiecewiseAnimation` for smooth, non-linear animations. **MagnumOpus already has this in `Particle.cs`!**

```csharp
// From Particle.cs - already available!
public struct CurveSegment
{
    public EasingType easing;
    public float startingX;      // Progress at segment start (0-1)
    public float startingHeight; // Value at segment start
    public float elevationShift; // Change in value over segment
    public int degree;           // Power for polynomial easing
}

public enum EasingType
{
    Linear,
    SineIn, SineOut, SineInOut, SineBump,
    PolyIn, PolyOut, PolyInOut,
    ExpIn, ExpOut, ExpInOut,
    CircIn, CircOut, CircInOut
}
```

### Using PiecewiseAnimation
```csharp
using MagnumOpus.Common.Systems.Particles;
using static MagnumOpus.Common.Systems.Particles.Particle;

// Example: Slow start → Fast middle → Ease out (whip effect)
public float GetSwingVelocity(float progress)
{
    return PiecewiseAnimation(progress, new CurveSegment[]
    {
        // Phase 1: Slow anticipation (0 → 0.2) - ease in
        new CurveSegment(EasingType.PolyIn, 0f, 0f, 0.15f, 3),
        
        // Phase 2: Fast swing (0.2 → 0.75) - fast and powerful
        new CurveSegment(EasingType.PolyOut, 0.2f, 0.15f, 0.75f, 2),
        
        // Phase 3: Follow through (0.75 → 1.0) - ease out
        new CurveSegment(EasingType.SineOut, 0.75f, 0.9f, 0.1f)
    });
}

// Example: Pulse effect (0 → 1 → 0)
public float GetPulseIntensity(float progress)
{
    return PiecewiseAnimation(progress, new CurveSegment[]
    {
        new CurveSegment(EasingType.SineBump, 0f, 0f, 1f)
    });
}
```

### Predefined Curves in CalamityStyleVFX
```csharp
using MagnumOpus.Common.Systems.VFX;

// Get swing velocity curve with whip feel
float velocity = CalamityStyleVFX.GetSwingVelocityVaried(progress, variation);

// MeleeSwingVariation presets:
CalamityStyleVFX.MeleeSwingVariation.Default   // Balanced
CalamityStyleVFX.MeleeSwingVariation.Heavy     // Greatswords, hammers
CalamityStyleVFX.MeleeSwingVariation.Swift     // Rapiers, daggers
CalamityStyleVFX.MeleeSwingVariation.Ethereal  // Magical blades
CalamityStyleVFX.MeleeSwingVariation.Legendary // Endgame weapons
```

---

## 🛤️ PATTERN 5: Primitive Trail Rendering

### Calamity's PrimitiveSettings
```csharp
new PrimitiveSettings(
    WidthFunction,   // Func<float, float> - completion ratio → width
    ColorFunction,   // Func<float, Color> - completion ratio → color
    ...
);
```

### MagnumOpus Implementation
```csharp
using MagnumOpus.Common.Systems.VFX;

// Create trail settings
var settings = new EnhancedTrailRenderer.PrimitiveSettings(
    width: EnhancedTrailRenderer.LinearTaper(24f),           // 24 → 0
    color: EnhancedTrailRenderer.GradientColor(startColor, endColor),
    smoothen: true
);

// Render multi-pass trail (bloom, main, core)
EnhancedTrailRenderer.RenderMultiPassTrail(
    positions,      // Vector2[] - trail positions
    rotations,      // float[] - rotations at each point (optional)
    settings,
    passes: 3
);
```

### Width Function Presets
```csharp
// Linear taper (most common)
EnhancedTrailRenderer.LinearTaper(startWidth)

// Quadratic bump (thicker in middle)
EnhancedTrailRenderer.QuadraticBumpWidth(maxWidth)

// Inverse lerp bump (custom falloff)
EnhancedTrailRenderer.InverseLerpBumpWidth(maxWidth)

// Custom:
EnhancedTrailRenderer.WidthFunction myWidth = (completionRatio) => {
    return 20f * (1f - completionRatio * completionRatio);
};
```

### Color Function Presets
```csharp
// Two-color gradient
EnhancedTrailRenderer.GradientColor(startColor, endColor)

// Gradient with opacity curve
EnhancedTrailRenderer.GradientColor(startColor, endColor, maxOpacity: 0.8f)

// Palette lerp (through color array)
EnhancedTrailRenderer.PaletteLerpColor(Color[] palette)

// Custom:
EnhancedTrailRenderer.ColorFunction myColor = (completionRatio) => {
    return Color.Lerp(Color.White, Color.Blue, completionRatio);
};
```

---

## 🌌 PATTERN 6: Nebula Fog Trails (5-Layer System)

### Calamity's 5-Layer Technique
From the Galaxia deep dive:
1. **Base Shape** - Nebula Wisp Noise
2. **Detail Variation** - FBM Noise  
3. **Core Definition** - Horizontal Energy Gradient
4. **Sparkle Overlay** - Sparkly Noise
5. **Distortion Pass** - Marble Noise

### MagnumOpus Implementation
```csharp
using MagnumOpus.Common.Systems.VFX;

// Create a nebula trail for your projectile
int trailId = CalamityNebulaTrail.CreateTrail(
    ownerId: Projectile.whoAmI,
    theme: "Fate",  // Gets cosmic palette automatically
    baseWidth: 25f
);

// Update each frame
CalamityNebulaTrail.UpdateTrail(
    trailId,
    Projectile.Center,
    Projectile.velocity.ToRotation()
);

// When projectile dies
CalamityNebulaTrail.FadeOutTrail(trailId);
```

### Theme Palettes Available
```csharp
// All themes have pre-defined cosmic palettes:
CalamityNebulaTrail.GetCosmicPalette("Fate");       // White → Pink → Magenta
CalamityNebulaTrail.GetCosmicPalette("LaCampanella"); // Warm White → Orange → Dark Ember
CalamityNebulaTrail.GetCosmicPalette("Eroica");     // White → Gold → Scarlet
CalamityNebulaTrail.GetCosmicPalette("SwanLake");   // Pure White → Icy Blue → Deep Blue
CalamityNebulaTrail.GetCosmicPalette("MoonlightSonata"); // Silver → Purple → Deep Violet
CalamityNebulaTrail.GetCosmicPalette("EnigmaVariations"); // Green Flame → Purple → Black
```

---

## ⚔️ PATTERN 7: Ark-Style Melee Swings

### Using ArkSwingTrail
```csharp
using MagnumOpus.Common.Systems.VFX;

// In your weapon's UseItemFrame or similar:
public override void UseItemFrame(Player player)
{
    if (player.itemAnimation > 0)
    {
        // Update trail each frame during swing
        ArkSwingTrail.UpdateSwingTrail(
            player,
            bladeLength: 80f,
            primaryColor: EroicaGold,
            secondaryColor: EroicaScarlet,
            width: 35f,
            theme: "Eroica"
        );
    }
}

// When item use ends
public override void HoldItem(Player player)
{
    if (player.itemAnimation == 0)
    {
        ArkSwingTrail.EndSwingTrail(player);
    }
}
```

### Instant Arc (for projectile-based swings)
```csharp
ArkSwingTrail.SpawnSwingArc(
    player,
    startAngle: -MathHelper.PiOver4,
    endAngle: MathHelper.PiOver4,
    bladeLength: 80f,
    primaryColor: Color.Gold,
    secondaryColor: Color.Orange,
    width: 30f,
    pointCount: 20,
    theme: "LaCampanella"
);
```

---

## 🌟 PATTERN 8: Constellation Line Effects

### MagnumOpus Constellation System
```csharp
using MagnumOpus.Common.Systems.VFX;

// Draw constellation lines between points
VerletConstellationSystem.DrawConstellationLine(
    startPos: projectilePos,
    endPos: targetPos,
    color: cosmicPurple,
    width: 3f,
    bloomIntensity: 0.6f
);

// Create a star pattern connecting multiple points
Vector2[] starPoints = new Vector2[] { point1, point2, point3, point4, point5 };
VerletConstellationSystem.DrawConstellationPattern(
    points: starPoints,
    color: Color.White,
    lineWidth: 2f,
    sparkleAtVertices: true
);
```

---

## 🎆 PATTERN 9: Layered Nebula Fog (Swing Fog)

### For Weapon Swing Fog Effects
```csharp
using MagnumOpus.Common.Systems.VFX;

// Spawn fog clouds along swing arc
LayeredNebulaFog.SpawnSwingNebula(
    player,
    swingProgress,
    primaryColor: cosmicPurple,
    secondaryColor: nebulaPink,
    scale: 1.2f
);

// Or spawn individual cloud
LayeredNebulaFog.SpawnNebulaCloud(
    position: bladeTipPos,
    primaryColor: cosmicPurple,
    secondaryColor: nebulaPink,
    scale: 0.8f,
    velocity: -swingVelocity * 0.1f,
    lifetime: 45
);
```

### For Projectile Trail Fog
```csharp
NebulaFogSystem.AttachTrailFog(
    projectile: Projectile,
    color: themeColor,
    density: 0.8f,
    scale: 1f
);
```

---

## 🔧 COMPLETE WEAPON EXAMPLE

Here's how to create a weapon with full Calamity-style VFX:

```csharp
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;

public class CosmicBlade : ModItem
{
    // Smear particle reference for persistence
    private CircularSmearSmokeyVFX _swingSmear;
    
    public override void SetDefaults()
    {
        Item.width = 60;
        Item.height = 60;
        Item.damage = 150;
        Item.DamageType = DamageClass.Melee;
        Item.useTime = 25;
        Item.useAnimation = 25;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.knockBack = 6f;
        Item.rare = ModContent.RarityType<FateRarity>();
    }
    
    public override void UseItemFrame(Player player)
    {
        if (player.itemAnimation <= 0) return;
        
        float progress = 1f - (player.itemAnimation / (float)player.itemAnimationMax);
        Vector2 swingDir = player.itemRotation.ToRotationVector2();
        
        // === SMEAR PERSISTENCE ===
        Color smearColor = GetCosmicColor(progress);
        if (_swingSmear == null)
        {
            _swingSmear = new CircularSmearSmokeyVFX(
                player.Center, smearColor, swingDir.ToRotation(), 1.5f);
            MagnumParticleHandler.SpawnParticle(_swingSmear);
        }
        else
        {
            _swingSmear.Time = 0;
            _swingSmear.Position = player.Center;
            _swingSmear.Rotation = swingDir.ToRotation() + MathHelper.PiOver2;
            _swingSmear.Color = smearColor;
        }
        
        // === ARK SWING TRAIL ===
        ArkSwingTrail.UpdateSwingTrail(
            player, 
            bladeLength: 80f,
            primaryColor: new Color(255, 180, 200),
            secondaryColor: new Color(200, 80, 120),
            width: 30f,
            theme: "Fate"
        );
        
        // === DUAL-LAYER SMOKE ===
        if (progress > 0.2f && progress < 0.8f && Main.rand.NextBool(2))
        {
            Vector2 tipPos = player.Center + swingDir * 80f;
            SpawnDualLayerSmoke(tipPos, -swingDir * 2f, smearColor);
        }
        
        // === SPARKLES ===
        float velocity = CalamityStyleVFX.GetSwingVelocityVaried(progress, 
            CalamityStyleVFX.MeleeSwingVariation.Legendary);
        if (velocity > 0.5f && Main.rand.NextBool(3))
        {
            Vector2 tipPos = player.Center + swingDir * 80f;
            var spark = new CritSpark(
                tipPos,
                Main.rand.NextVector2Circular(4f, 4f),
                Color.White,
                new Color(255, 180, 200),
                scale: 0.6f,
                lifetime: 20,
                angularVelocity: 0.1f,
                bloomScale: 3f
            );
            MagnumParticleHandler.SpawnParticle(spark);
        }
    }
    
    public override void HoldItem(Player player)
    {
        if (player.itemAnimation == 0)
        {
            // End swing effects
            _swingSmear = null;
            ArkSwingTrail.EndSwingTrail(player);
        }
    }
    
    private Color GetCosmicColor(float progress)
    {
        // Fate palette gradient
        Color[] palette = MagnumThemePalettes.Fate;
        return MagnumThemePalettes.GetThemeColor("Fate", progress);
    }
    
    private void SpawnDualLayerSmoke(Vector2 pos, Vector2 vel, Color color)
    {
        // Base layer
        var baseSmoke = new HeavySmokeParticle(pos, vel, color, 35, 1.2f, 0.7f, 0.02f, false);
        MagnumParticleHandler.SpawnParticle(baseSmoke);
        
        // Glow layer
        var glowSmoke = new HeavySmokeParticle(pos, vel * 1.1f, 
            Color.Lerp(color, Color.White, 0.3f), 30, 1.0f, 0.35f, 0.015f, true);
        MagnumParticleHandler.SpawnParticle(glowSmoke);
    }
}
```

---

## 📋 QUICK REFERENCE CHECKLIST

### For Any Weapon VFX:
- [ ] Use smear persistence pattern (store reference, reset `Time = 0` each frame)
- [ ] Use `{ A = 0 }` pattern for all additive blending
- [ ] Layer bloom effects (4 layers: 2.0x, 1.4x, 0.9x, 0.4x scale)
- [ ] Use `CurveSegment` for non-linear animations
- [ ] Dual-layer smoke (base + glow with hue shift)
- [ ] Use `MagnumThemePalettes` for consistent colors

### For Projectile Trails:
- [ ] Use `EnhancedTrailRenderer` with proper `WidthFunction`/`ColorFunction`
- [ ] Consider `CalamityNebulaTrail` for cosmic effects
- [ ] Add sparkle overlay with `CritSpark`
- [ ] Use `LayeredNebulaFog` for volumetric fog

### For Boss Effects:
- [ ] Use `SegmentAnimator` for worm/multi-segment bosses
- [ ] Use `VerletConstellationSystem` for laser/beam connections
- [ ] Scale effects with `BossVFXOptimizer` for performance
- [ ] Layer multiple VFX systems together

---

## 📂 FILE LOCATIONS

| System | File Path |
|--------|-----------|
| Smear Particles | `Common/Systems/Particles/SmearParticles.cs` |
| HeavySmokeParticle | `Common/Systems/Particles/CommonParticles.cs` |
| Particle Base + CurveSegment | `Common/Systems/Particles/Particle.cs` |
| Particle Handler | `Common/Systems/Particles/MagnumParticleHandler.cs` |
| Bloom Renderer | `Common/Systems/VFX/BloomRenderer.cs` |
| Enhanced Trail Renderer | `Common/Systems/VFX/EnhancedTrailRenderer.cs` |
| Ark Swing Trail | `Common/Systems/VFX/ArkSwingTrail.cs` |
| Calamity Nebula Trail | `Common/Systems/VFX/CalamityNebulaTrail.cs` |
| Layered Nebula Fog | `Common/Systems/VFX/LayeredNebulaFog.cs` |
| Nebula Fog System | `Common/Systems/VFX/NebulaFogSystem.cs` |
| Segment Animator | `Common/Systems/VFX/SegmentAnimator.cs` |
| Constellation System | `Common/Systems/VFX/VerletConstellationSystem.cs` |
| Calamity Style VFX | `Common/Systems/VFX/CalamityStyleVFX.cs` |
| Theme Palettes | `Common/Systems/VFX/MagnumThemePalettes.cs` |

---

## ⚠️ IMPORTANT NOTES

### 1. The `{ A = 0 }` Pattern is CRITICAL
```csharp
// ALWAYS remove alpha for additive blending
Color drawColor = myColor with { A = 0 };
spriteBatch.Draw(tex, pos, null, drawColor * opacity, ...);
```

### 2. Smear Lifetime is SHORT by Design
Smear particles have `Lifetime = 2` or `Lifetime = 3`. This is intentional - you keep them alive by resetting `Time = 0` each frame. When you stop resetting, they die quickly.

### 3. Layer Order Matters
For bloom stacking, draw LARGEST/FAINTEST first, then work toward SMALLEST/BRIGHTEST.

### 4. Performance Considerations
- Use `BossVFXOptimizer` for boss fights
- Check `MagnumParticleHandler.ActiveParticleCount` before spawning
- Consider `frameInterval` parameters for expensive effects
- Pool particles when possible

### 5. Shader Compatibility
MagnumOpus uses FNA (MojoShader), not DirectX. Custom shaders need special compilation. The systems are designed to work with `BasicEffect` fallbacks when shaders aren't available.

---

*This guide was created by analyzing both the Ark of the Cosmos and Galaxia deep dive documents, then mapping their patterns to MagnumOpus's existing infrastructure.*
