# Exomech VFX Design Document
## Analysis of InfernumMode's Artemis, Apollo, Ares, and Thanatos Visual Effects

---

## Overview

The Exomechs (Artemis/Apollo twins, Ares, and Thanatos) feature some of the most sophisticated laser beam and primitive trail effects in the Calamity/Infernum modding ecosystem. This document details their implementation for adaptation into MagnumOpus.

---

## Core Architecture

### IPixelPrimitiveDrawer Interface

All Exomech laser projectiles implement `IPixelPrimitiveDrawer` for pixel-perfect primitive rendering:

```csharp
public class ArtemisSweepLaserbeam : ModProjectile, IPixelPrimitiveDrawer
{
    public PrimitiveTrailCopy BeamDrawer;
    
    public void DrawPixelPrimitives(SpriteBatch spriteBatch)
    {
        // Primitive drawing logic here
    }
}
```

**Source:** `Content/BehaviorOverrides/BossAIs/Draedon/ArtemisAndApollo/ArtemisSweepLaserbeam.cs`

### PrimitiveTrailCopy Pattern

The core primitive drawer uses width and color function delegates:

```csharp
BeamDrawer ??= new PrimitiveTrailCopy(
    WidthFunction,      // float WidthFunction(float completionRatio)
    ColorFunction,      // Color ColorFunction(float completionRatio)
    null,               // Offset function (optional)
    true,               // Pixel-perfect mode
    InfernumEffectsRegistry.ArtemisLaserVertexShader  // Shader
);
```

---

## Artemis Laser Effects

### ArtemisSweepLaserbeam

The signature sweeping laser uses the `ArtemisLaserVertexShader`:

```csharp
// File: ArtemisSweepLaserbeam.cs (Lines 50-70)
public void DrawPixelPrimitives(SpriteBatch spriteBatch)
{
    BeamDrawer ??= new PrimitiveTrailCopy(WidthFunction, ColorFunction, null, true, 
        InfernumEffectsRegistry.ArtemisLaserVertexShader);
    
    // Shader configuration
    InfernumEffectsRegistry.ArtemisLaserVertexShader.UseColor(Color.Cyan);
    InfernumEffectsRegistry.ArtemisLaserVertexShader.SetShaderTexture(
        InfernumTextureRegistry.StreakThickGlow);
    InfernumEffectsRegistry.ArtemisLaserVertexShader.UseImage2("Images/Misc/Perlin");
    
    // Generate draw points along laser length
    Vector2[] baseDrawPoints = new Vector2[8];
    for (int i = 0; i < baseDrawPoints.Length; i++)
        baseDrawPoints[i] = Vector2.Lerp(Projectile.Center, laserEnd, i / 7f);
    
    BeamDrawer.DrawPixelated(baseDrawPoints, -Main.screenPosition, 30);
}
```

**Key Parameters:**
- **Draw Points:** 8 points along the laser
- **Sample Count:** 30 for smooth rendering
- **Shader:** ArtemisLaserVertexShader with electricity effect
- **Textures:** StreakThickGlow (primary), Perlin noise (secondary)

### ArtemisLaserVertexShader.fx

The actual shader code for the electricity effect:

```hlsl
// File: Assets/Effects/Primitives/ArtemisLaserShader.fx (Lines 45-60)
float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float4 color = input.Color;
    float2 coords = input.TextureCoordinates;
    
    // Sample electricity noise
    float electricityNoise = tex2D(uImage1, coords * float2(3, 1) + float2(uTime * -4, 0)).r;
    
    // Edge fadeout for smooth laser edges
    float edgeFade = pow(sin(coords.y * 3.141), 2);
    
    // Apply color with electricity variation
    color.rgb *= 1 + electricityNoise * 0.5;
    color.a *= edgeFade;
    
    return color * uOpacity;
}
```

**Shader Features:**
- **Electricity Effect:** Animated noise texture scrolling
- **Edge Fade:** Sin^2 curve for soft edges
- **Color Multiplication:** 2.5x color boost for brightness
- **Opacity Control:** uOpacity uniform for fade effects

---

## Apollo Effects

### ApolloRocketInfernum

Apollo's rockets use flame trail primitives:

```csharp
// File: ApolloRocketInfernum.cs (Lines 100-120)
public void DrawPixelPrimitives(SpriteBatch spriteBatch)
{
    FlameTrailDrawer ??= new PrimitiveTrailCopy(
        FlameTrailWidthFunction, 
        FlameTrailColorFunction, 
        null, true, 
        GameShaders.Misc["CalamityMod:ImpFlameTrail"]);
    
    // Use StreakMagma texture for fire effect
    Utilities.SetTexture1(InfernumTextureRegistry.StreakMagma.Value);
    
    Vector2 trailOffset = Projectile.Size * 0.5f;
    trailOffset += (Projectile.rotation + PiOver2).ToRotationVector2() * 10f;
    
    FlameTrailDrawer.DrawPixelated(Projectile.oldPos, trailOffset - Main.screenPosition, 31);
}
```

**Key Features:**
- **Shader:** CalamityMod:ImpFlameTrail
- **Texture:** StreakMagma for molten fire look
- **Sample Count:** 31 points for smooth trails
- **Offset:** Positioned at projectile rear

### Plasma Flame Color Function

```csharp
public Color FlameTrailColorFunction(float completionRatio)
{
    // Gradient from green plasma to darker at edges
    Color inner = new Color(0, 255, 100);
    Color outer = new Color(0, 150, 50);
    float fade = Pow(1f - completionRatio, 2);
    return Color.Lerp(outer, inner, fade) * Projectile.Opacity;
}
```

---

## Ares Effects

### AresLaserDeathray

Ares uses the same shader system with different color parameters:

```csharp
// File: AresLaserDeathray.cs (Lines 116-135)
public void DrawPixelPrimitives(SpriteBatch spriteBatch)
{
    LaserDrawer ??= new(LaserWidthFunction, LaserColorFunction, null, true, 
        InfernumEffectsRegistry.ArtemisLaserVertexShader);

    Vector2 laserEnd = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitY) * LaserLength;
    Vector2[] baseDrawPoints = new Vector2[8];
    for (int i = 0; i < baseDrawPoints.Length; i++)
        baseDrawPoints[i] = Vector2.Lerp(Projectile.Center, laserEnd, i / 7f);

    // Red color configuration for Ares
    InfernumEffectsRegistry.ArtemisLaserVertexShader.UseColor(Color.Red);
    InfernumEffectsRegistry.ArtemisLaserVertexShader.SetShaderTexture(
        InfernumTextureRegistry.StreakThickGlow);
    
    LaserDrawer.DrawPixelated(baseDrawPoints, -Main.screenPosition, 54);
}
```

### AresPulseDeathray

Uses alternative texture for pulsing effect:

```csharp
// File: AresPulseDeathray.cs
// Uses Images/Extra_189 (vanilla Fuchsia texture)
// Color: Fuchsia for distinct visual identity
```

### AresSpinningDeathBeam

Uses the Bordernado shader for special spinning effects:

```csharp
// File: AresSpinningDeathBeam.cs
BeamDrawer ??= new PrimitiveTrailCopy(WidthFunction, ColorFunction, null, true, 
    GameShaders.Misc["Infernum:Bordernado"]);

// Uses VoronoiShapes texture for organic look
InfernumEffectsRegistry.SetShaderTexture(InfernumTextureRegistry.VoronoiShapes);
// Saturation: 1.4 for vibrant colors
```

---

## Thanatos Effects

### LightOverloadRay

The devastating mouth beam uses fire shaders with additive blending:

```csharp
// File: LightOverloadRay.cs (Lines 97-130)
public void DrawPixelPrimitives(SpriteBatch spriteBatch)
{
    spriteBatch.SetBlendState(BlendState.Additive);

    LaserDrawer ??= new PrimitiveTrailCopy(LaserWidthFunction, LaserColorFunction, 
        null, true, InfernumEffectsRegistry.FireVertexShader);

    InfernumEffectsRegistry.FireVertexShader.UseSaturation(0.14f);
    InfernumEffectsRegistry.FireVertexShader.SetShaderTexture(
        InfernumTextureRegistry.CultistRayMap);

    var oldBlendState = Main.instance.GraphicsDevice.BlendState;
    Main.instance.GraphicsDevice.BlendState = BlendState.Additive;
    
    // Multiple ray draws for volumetric effect
    for (int i = 0; i < 45; i++)
    {
        float offsetAngle = Lerp(-LaserSpread, LaserSpread, i / 44f);
        Vector2 rayDirection = (Thanatos.rotation - PiOver2 + offsetAngle).ToRotationVector2();
        
        List<Vector2> drawPoints = new();
        for (int j = 0; j < 20; j++)
            drawPoints.Add(StartingPosition + rayDirection * j * (LaserLength / 20f));
        
        LaserDrawer.Draw(drawPoints, -Main.screenPosition, 30);
    }
    
    Main.instance.GraphicsDevice.BlendState = oldBlendState;
}
```

**Key Features:**
- **Additive Blending:** For bright, glowing effect
- **Multiple Rays:** 45 individual rays for volumetric spread
- **FireVertexShader:** Animated fire texture
- **CultistRayMap:** High-contrast ray texture

### ExolaserBomb

Orb projectiles with fire shader rendering:

```csharp
// File: ExolaserBomb.cs (Lines 96-120)
FireDrawer ??= new PrimitiveTrailCopy(SunWidthFunction, SunColorFunction, 
    null, true, InfernumEffectsRegistry.FireVertexShader);

InfernumEffectsRegistry.FireVertexShader.UseSaturation(0.45f);
InfernumEffectsRegistry.FireVertexShader.SetShaderTexture(
    InfernumTextureRegistry.CultistRayMap);

// Draw radial segments for sun-like effect
for (float offsetAngle = -PiOver2; offsetAngle <= PiOver2; offsetAngle += Pi / 24f)
{
    drawPoints.Clear();
    float adjustedAngle = offsetAngle + LumUtils.PerlinNoise2D(offsetAngle, 
        Main.GlobalTimeWrappedHourly * 0.06f, 3, 185) * 3f;
    
    Vector2 offsetDirection = adjustedAngle.ToRotationVector2();
    for (int i = 0; i < 16; i++)
    {
        float radius = Projectile.scale * (i / 15f) * 100f;
        drawPoints.Add(Projectile.Center + offsetDirection * radius);
    }
    
    FireDrawer.Draw(drawPoints, -Main.screenPosition, 40);
}
```

---

## Twin Drawing System

### DrawExoTwin Function

Special drawing for the Artemis/Apollo twins with glow effects:

```csharp
// File: ApolloBehaviorOverride.cs (Lines 1800+)
public static void DrawExoTwin(NPC npc, float flashInterpolant)
{
    Main.spriteBatch.EnterShaderRegion();
    
    // Draw backglow
    Texture2D glowTexture = ModContent.Request<Texture2D>("...Glow").Value;
    Color glowColor = Color.White * flashInterpolant * 0.6f;
    Main.spriteBatch.Draw(glowTexture, drawPos, null, glowColor, rotation, 
        origin, scale * 1.2f, effects, 0f);
    
    // Final phase extra glow
    if (inFinalPhase)
    {
        Color finalPhaseGlow = Color.Lerp(Color.Cyan, Color.White, 0.3f) * 0.4f;
        for (int i = 0; i < 4; i++)
        {
            Vector2 offset = (TwoPi * i / 4f).ToRotationVector2() * 4f;
            Main.spriteBatch.Draw(texture, drawPos + offset, frame, 
                finalPhaseGlow, rotation, origin, scale, effects, 0f);
        }
    }
    
    Main.spriteBatch.ExitShaderRegion();
}
```

---

## Shader Registry

### Key Shaders Used by Exomechs

| Shader Name | Purpose | Key Parameters |
|-------------|---------|----------------|
| `ArtemisLaserVertexShader` | Electricity laser beams | Color, Opacity, Saturation, StretchReverseFactor |
| `FireVertexShader` | Fire/plasma effects | Saturation, Texture |
| `PrismaticRayVertexShader` | Rainbow/prismatic beams | Opacity, Image1, Image2 |
| `Bordernado` | Spinning beam effects | Saturation |
| `ImpFlameTrail` | Rocket flame trails | Texture |

### Texture Assets

| Texture Name | Use Case |
|--------------|----------|
| `StreakThickGlow` | Primary laser beam body |
| `StreakMagma` | Fire/magma trails |
| `CultistRayMap` | High-contrast ray effects |
| `VoronoiShapes` | Organic spinning beams |
| `Perlin` (vanilla) | Noise for electricity |

---

## Adaptation Guidelines for MagnumOpus

### 1. Create Theme-Specific Laser Shaders

For each MagnumOpus theme, adapt the Artemis shader pattern:

```csharp
// Example: Fate Theme Laser
FateLaserDrawer ??= new PrimitiveTrailCopy(WidthFunction, ColorFunction, 
    null, true, FateEffectsRegistry.RealityTearShader);

FateEffectsRegistry.RealityTearShader.UseColor(UnifiedVFX.Fate.DarkPink);
FateEffectsRegistry.RealityTearShader.SetShaderTexture(FateTextureRegistry.CosmicNoise);
```

### 2. Color Function Patterns

Use gradient color functions that match theme palettes:

```csharp
// La Campanella: Black to Orange gradient
public Color CampanellaLaserColor(float completionRatio)
{
    return Color.Lerp(
        ThemedParticles.CampanellaBlack, 
        ThemedParticles.CampanellaOrange, 
        Sin(completionRatio * Pi)
    );
}

// Fate: Dark prismatic gradient
public Color FateLaserColor(float completionRatio)
{
    Color c1 = Color.Lerp(UnifiedVFX.Fate.Black, UnifiedVFX.Fate.DarkPink, completionRatio * 2f);
    Color c2 = Color.Lerp(UnifiedVFX.Fate.DarkPink, UnifiedVFX.Fate.BrightRed, 
        Math.Max(0, completionRatio * 2f - 1f));
    return completionRatio < 0.5f ? c1 : c2;
}
```

### 3. Width Function Patterns

```csharp
// Standard laser tapering
public float LaserWidthFunction(float completionRatio)
{
    float baseSwell = Sin(Pi * completionRatio);
    float tipTaper = 1f - Pow(completionRatio, 4);
    return baseWidth * baseSwell * tipTaper;
}

// Pulsing width for dramatic effect
public float PulsingWidthFunction(float completionRatio)
{
    float pulse = Sin(Main.GlobalTimeWrappedHourly * 10f + completionRatio * 5f) * 0.2f + 1f;
    return baseWidth * pulse * Sin(Pi * completionRatio);
}
```

### 4. Multi-Pass Rendering

For volumetric effects like LightOverloadRay:

```csharp
// Draw multiple offset passes for glow
Main.instance.GraphicsDevice.BlendState = BlendState.Additive;
for (int pass = 0; pass < 3; pass++)
{
    float scale = 1f + pass * 0.15f;
    float alpha = 1f - pass * 0.3f;
    
    LaserDrawer.DrawPixelated(points, offset, 30);
}
Main.instance.GraphicsDevice.BlendState = BlendState.AlphaBlend;
```

---

## Key Takeaways

1. **Shader-Based Rendering:** All major effects use custom shaders via PrimitiveTrailCopy
2. **Delegate Pattern:** Width and Color functions provide dynamic appearance
3. **Additive Blending:** Essential for bright, glowing laser effects
4. **Texture Layering:** Multiple textures (base + noise) create depth
5. **Sample Count:** Higher sample counts (30-60) create smoother primitives
6. **Point Distribution:** 8-24 points along laser length is standard
7. **BlendState Management:** Remember to restore original blend state after rendering

---

## File References

- `Content/BehaviorOverrides/BossAIs/Draedon/ArtemisAndApollo/ArtemisSweepLaserbeam.cs`
- `Content/BehaviorOverrides/BossAIs/Draedon/ArtemisAndApollo/ApolloBehaviorOverride.cs`
- `Content/BehaviorOverrides/BossAIs/Draedon/ArtemisAndApollo/ApolloRocketInfernum.cs`
- `Content/BehaviorOverrides/BossAIs/Draedon/Ares/AresLaserDeathray.cs`
- `Content/BehaviorOverrides/BossAIs/Draedon/Ares/AresSpinningDeathBeam.cs`
- `Content/BehaviorOverrides/BossAIs/Draedon/Thanatos/LightOverloadRay.cs`
- `Content/BehaviorOverrides/BossAIs/Draedon/Thanatos/ExolaserBomb.cs`
- `Assets/Effects/Primitives/ArtemisLaserShader.fx`
- `Assets/Effects/InfernumEffectsRegistry.cs`
