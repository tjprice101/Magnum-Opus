# Profaned Guardians VFX Design Document
## Analysis of InfernumMode's Profaned Guardians Visual Effects

---

## Overview

The Profaned Guardians feature holy fire and lava-based visual effects with unique shader systems. This document details the GuardiansLaserVertexShader, lava metaball effects, telegraph systems, and holy fire primitives for adaptation into MagnumOpus.

---

## Core Shader: GuardiansLaserVertexShader

### Lava Eruption Pillar

The signature lava pillar effect using dual-texture sampling:

```csharp
// File: LavaEruptionPillar.cs (Lines 70-130)
public class LavaEruptionPillar : ModProjectile, IPixelPrimitiveDrawer
{
    public PrimitiveTrailCopy FireDrawer;
    
    public const float MaxPillarHeight = 1800f;
    
    public float WidthFunction(float completionRatio)
    {
        // Wide at base, tapers toward top with flickering
        float baseFactor = Pow(1f - completionRatio, 0.4f);
        float flicker = Sin(Main.GlobalTimeWrappedHourly * 12f + completionRatio * 6f) * 0.1f + 1f;
        return 70f * baseFactor * flicker * Projectile.scale;
    }
    
    public Color ColorFunction(float completionRatio)
    {
        // Orange-yellow core, darker edges
        Color core = new Color(255, 200, 100);
        Color edge = new Color(200, 80, 30);
        float centerBrightness = Sin(completionRatio * Pi * 0.7f);
        return Color.Lerp(edge, core, centerBrightness) * Projectile.Opacity;
    }
    
    public void DrawPixelPrimitives(SpriteBatch spriteBatch)
    {
        FireDrawer ??= new PrimitiveTrailCopy(WidthFunction, ColorFunction, 
            null, true, InfernumEffectsRegistry.GuardiansLaserVertexShader);
        
        // Configure shader with LavaNoise texture
        InfernumEffectsRegistry.GuardiansLaserVertexShader.SetShaderTexture(
            InfernumTextureRegistry.LavaNoise);
        InfernumEffectsRegistry.GuardiansLaserVertexShader.UseColor(Color.Orange);
        InfernumEffectsRegistry.GuardiansLaserVertexShader.UseSecondaryColor(Color.Yellow);
        
        // Generate vertical pillar points
        Vector2[] pillarPoints = new Vector2[20];
        for (int i = 0; i < 20; i++)
        {
            float height = i / 19f * CurrentHeight;
            pillarPoints[i] = Projectile.Center - Vector2.UnitY * height;
        }
        
        FireDrawer.DrawPixelated(pillarPoints, -Main.screenPosition, 45);
    }
}
```

### Telegraph Side Streak Effect

```csharp
// File: LavaEruptionPillar.cs (Telegraph section)
public void DrawTelegraph(SpriteBatch spriteBatch)
{
    if (TelegraphIntensity <= 0f)
        return;
    
    TelegraphDrawer ??= new PrimitiveTrailCopy(TelegraphWidthFunction, TelegraphColorFunction, 
        null, true, InfernumEffectsRegistry.SideStreakVertexShader);
    
    // SideStreakVertexShader creates clean warning lines
    InfernumEffectsRegistry.SideStreakVertexShader.SetShaderTexture(
        InfernumTextureRegistry.StreakSolid);
    InfernumEffectsRegistry.SideStreakVertexShader.UseOpacity(TelegraphIntensity);
    
    // Vertical telegraph line
    Vector2[] telegraphPoints = new Vector2[8];
    for (int i = 0; i < 8; i++)
    {
        telegraphPoints[i] = Projectile.Center - Vector2.UnitY * (i / 7f * TelegraphHeight);
    }
    
    TelegraphDrawer.DrawPixelated(telegraphPoints, -Main.screenPosition, 20);
}

public float TelegraphWidthFunction(float completionRatio)
{
    return 8f * Sin(completionRatio * Pi) * TelegraphIntensity;
}

public Color TelegraphColorFunction(float completionRatio)
{
    return Color.Orange * TelegraphIntensity * 0.8f;
}
```

---

## Holy Spinning Fire Beam

### Multi-Point Laser with Rotation

```csharp
// File: HolySpinningFireBeam.cs (Lines 80-150)
public class HolySpinningFireBeam : ModProjectile, IPixelPrimitiveDrawer
{
    public PrimitiveTrailCopy LaserDrawer;
    
    public const int DrawPointCount = 54;
    public const float MaxLaserLength = 4200f;
    
    public float LaserWidth(float completionRatio)
    {
        // Sine-based width with tip fadeoff
        float tipFade = Pow(Sin(completionRatio * Pi), 0.5f);
        float pulse = Sin(Main.GlobalTimeWrappedHourly * 8f + completionRatio * 12f) * 0.15f + 1f;
        return 45f * tipFade * pulse * Projectile.scale;
    }
    
    public Color LaserColor(float completionRatio)
    {
        // Golden-orange holy fire gradient
        Color holy = new Color(255, 220, 150);
        Color fire = new Color(255, 130, 50);
        float gradient = Sin(completionRatio * Pi);
        return Color.Lerp(fire, holy, gradient) * Projectile.Opacity;
    }
    
    public void DrawPixelPrimitives(SpriteBatch spriteBatch)
    {
        LaserDrawer ??= new PrimitiveTrailCopy(LaserWidth, LaserColor, 
            null, true, InfernumEffectsRegistry.GuardiansLaserVertexShader);
        
        // Dual texture setup for complex lava effect
        InfernumEffectsRegistry.GuardiansLaserVertexShader.SetShaderTexture(
            InfernumTextureRegistry.LavaNoise);
        Main.instance.GraphicsDevice.Textures[2] = InfernumTextureRegistry.CultistRayMap.Value;
        
        // Color configuration
        InfernumEffectsRegistry.GuardiansLaserVertexShader.UseColor(Color.Orange);
        InfernumEffectsRegistry.GuardiansLaserVertexShader.UseSecondaryColor(Color.Gold);
        
        // Generate 54 points along rotating laser
        Vector2 laserDirection = Projectile.velocity.SafeNormalize(Vector2.UnitX);
        Vector2[] laserPoints = new Vector2[DrawPointCount];
        
        for (int i = 0; i < DrawPointCount; i++)
        {
            float progress = i / (float)(DrawPointCount - 1);
            float length = progress * MaxLaserLength * LaserLengthFactor;
            laserPoints[i] = Projectile.Center + laserDirection * length;
        }
        
        LaserDrawer.DrawPixelated(laserPoints, -Main.screenPosition, 60);
    }
}
```

---

## Holy Aimed Deathray

### Generic Laser with Harsh Noise

```csharp
// File: HolyAimedDeathray.cs (Lines 60-110)
public class HolyAimedDeathray : ModProjectile, IPixelPrimitiveDrawer
{
    public PrimitiveTrailCopy BeamDrawer;
    
    public const float MaxLength = 3600f;
    
    public float BeamWidthFunction(float completionRatio)
    {
        float tipFade = Sin(completionRatio * Pi);
        return 55f * Pow(tipFade, 0.6f) * Projectile.scale;
    }
    
    public Color BeamColorFunction(float completionRatio)
    {
        Color bright = new Color(255, 240, 200);
        Color warm = new Color(255, 150, 80);
        return Color.Lerp(warm, bright, Sin(completionRatio * Pi)) * Projectile.Opacity;
    }
    
    public void DrawPixelPrimitives(SpriteBatch spriteBatch)
    {
        BeamDrawer ??= new PrimitiveTrailCopy(BeamWidthFunction, BeamColorFunction, 
            null, true, InfernumEffectsRegistry.GenericLaserVertexShader);
        
        // HarshNoise texture for aggressive look
        InfernumEffectsRegistry.GenericLaserVertexShader.SetShaderTexture(
            InfernumTextureRegistry.HarshNoise);
        
        // "strongerFade" parameter for harder edge falloff
        InfernumEffectsRegistry.GenericLaserVertexShader.UseStrongerFade(true);
        
        Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
        Vector2[] points = new Vector2[16];
        
        for (int i = 0; i < 16; i++)
        {
            float length = i / 15f * MaxLength * LengthFactor;
            points[i] = Projectile.Center + direction * length;
        }
        
        BeamDrawer.DrawPixelated(points, -Main.screenPosition, 40);
    }
}
```

---

## Holy Fire Wall

### Gap-Based Laser Wall System

```csharp
// File: HolyFireWall.cs (Lines 50-100)
public class HolyFireWall : ModProjectile, IPixelPrimitiveDrawer
{
    public PrimitiveTrailCopy WallDrawer;
    
    public const int WallPointCount = 8;
    
    public float WallWidthFunction(float completionRatio)
    {
        return 40f * Sin(completionRatio * Pi) * Projectile.scale;
    }
    
    public Color WallColorFunction(float completionRatio)
    {
        Color holy = new Color(255, 200, 120);
        Color edge = new Color(200, 100, 50);
        return Color.Lerp(edge, holy, Sin(completionRatio * Pi)) * Projectile.Opacity;
    }
    
    public void DrawPixelPrimitives(SpriteBatch spriteBatch)
    {
        WallDrawer ??= new PrimitiveTrailCopy(WallWidthFunction, WallColorFunction, 
            null, true, InfernumEffectsRegistry.GenericLaserVertexShader);
        
        InfernumEffectsRegistry.GenericLaserVertexShader.SetShaderTexture(
            InfernumTextureRegistry.LavaNoise);
        
        // Wall with gap for player escape
        float gapStart = GapPosition - GapWidth * 0.5f;
        float gapEnd = GapPosition + GapWidth * 0.5f;
        
        // Draw segments before and after gap
        DrawWallSegment(WallStart, gapStart);
        DrawWallSegment(gapEnd, WallEnd);
    }
    
    private void DrawWallSegment(float start, float end)
    {
        Vector2[] points = new Vector2[WallPointCount];
        for (int i = 0; i < WallPointCount; i++)
        {
            float t = i / (float)(WallPointCount - 1);
            float position = MathHelper.Lerp(start, end, t);
            points[i] = GetWallPosition(position);
        }
        
        WallDrawer.DrawPixelated(points, -Main.screenPosition, 30);
    }
}
```

---

## Holy Sine Spear

### ImpFlameTrail with Honeycomb Noise

```csharp
// File: HolySineSpear.cs (Lines 70-110)
public class HolySineSpear : ModProjectile, IPixelPrimitiveDrawer
{
    public PrimitiveTrailCopy SpearDrawer;
    
    public float TrailWidthFunction(float completionRatio)
    {
        // Wider at origin, thin at tip
        return 25f * (1f - completionRatio) * Projectile.scale;
    }
    
    public Color TrailColorFunction(float completionRatio)
    {
        Color bright = new Color(255, 220, 150);
        Color dim = new Color(200, 100, 50);
        return Color.Lerp(bright, dim, completionRatio) * Projectile.Opacity;
    }
    
    public void DrawPixelPrimitives(SpriteBatch spriteBatch)
    {
        SpearDrawer ??= new PrimitiveTrailCopy(TrailWidthFunction, TrailColorFunction, 
            null, true, GameShaders.Misc["CalamityMod:ImpFlameTrail"]);
        
        // HoneycombNoise for organic fire texture
        GameShaders.Misc["CalamityMod:ImpFlameTrail"].SetShaderTexture(
            InfernumTextureRegistry.HoneycombNoise);
        
        // Use oldPos for trail rendering
        SpearDrawer.DrawPixelated(Projectile.oldPos, Projectile.Size * 0.5f - Main.screenPosition, 35);
    }
}
```

---

## Defender Guardian Fire Suckup

### Pulsating Laser for Energy Absorption

```csharp
// File: DefenderGuardianBehaviorOverride.cs (Fire Suckup section)
public static void DrawFireSuckup(NPC guardian, Vector2 target, float intensity)
{
    if (intensity <= 0f)
        return;
    
    FireSuckDrawer ??= new PrimitiveTrailCopy(SuckWidthFunction, SuckColorFunction, 
        null, true, InfernumEffectsRegistry.PulsatingLaserVertexShader);
    
    // Pulsating shader creates energy absorption effect
    InfernumEffectsRegistry.PulsatingLaserVertexShader.SetShaderTexture(
        InfernumTextureRegistry.StreakMagma);
    InfernumEffectsRegistry.PulsatingLaserVertexShader.UsePulseSpeed(4f);
    InfernumEffectsRegistry.PulsatingLaserVertexShader.UseIntensity(intensity);
    
    // Points from guardian to target (energy source)
    Vector2[] suckPoints = new Vector2[12];
    for (int i = 0; i < 12; i++)
    {
        suckPoints[i] = Vector2.Lerp(guardian.Center, target, i / 11f);
    }
    
    FireSuckDrawer.DrawPixelated(suckPoints, -Main.screenPosition, 30);
}

public static float SuckWidthFunction(float completionRatio)
{
    // Thin at guardian, wide at source
    return 20f * completionRatio;
}

public static Color SuckColorFunction(float completionRatio)
{
    Color sourceColor = new Color(255, 180, 100);
    Color drainColor = new Color(200, 80, 40);
    return Color.Lerp(drainColor, sourceColor, completionRatio);
}
```

---

## Attacker Guardian Fire Border

### Area Border Vertex Shader

```csharp
// File: AttackerGuardianBehaviorOverride.cs (Fire Border section)
public static void DrawFireBorder(Vector2 center, float radius, float intensity)
{
    BorderDrawer ??= new PrimitiveTrailCopy(BorderWidthFunction, BorderColorFunction, 
        null, true, InfernumEffectsRegistry.AreaBorderVertexShader);
    
    // AreaBorderVertexShader for arena boundary effects
    InfernumEffectsRegistry.AreaBorderVertexShader.SetShaderTexture(
        InfernumTextureRegistry.LavaNoise);
    InfernumEffectsRegistry.AreaBorderVertexShader.UseIntensity(intensity);
    
    // Circular border points
    int pointCount = 48;
    Vector2[] borderPoints = new Vector2[pointCount];
    
    for (int i = 0; i < pointCount; i++)
    {
        float angle = TwoPi * i / (pointCount - 1);
        borderPoints[i] = center + angle.ToRotationVector2() * radius;
    }
    
    BorderDrawer.DrawPixelated(borderPoints, -Main.screenPosition, 50);
}

public static float BorderWidthFunction(float completionRatio)
{
    return 35f;
}

public static Color BorderColorFunction(float completionRatio)
{
    // Gradient around the circle
    float hue = completionRatio * 0.1f + 0.05f; // Orange range
    return Main.hslToRgb(hue, 0.9f, 0.6f);
}
```

---

## ProfanedLava.fx Shader

### HLSL Shader Analysis

```hlsl
// File: Assets/Effects/Overlays/ProfanedLava.fx
sampler uImage0 : register(s0);
sampler uImage1 : register(s1); // LavaNoise
sampler uImage2 : register(s2); // Secondary noise

float globalTime;
float edgeOpacity;
float2 noiseScrollSpeed;

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    // Dual noise texture sampling
    float2 noiseCoords1 = coords + float2(globalTime * noiseScrollSpeed.x, 0);
    float2 noiseCoords2 = coords + float2(0, globalTime * noiseScrollSpeed.y);
    
    float noise1 = tex2D(uImage1, noiseCoords1).r;
    float noise2 = tex2D(uImage2, noiseCoords2).r;
    
    // Combine noise for lava flow effect
    float combinedNoise = (noise1 + noise2) * 0.5;
    
    // Edge opacity tapering
    float edgeFade = 1.0;
    if (coords.x < 0.1)
        edgeFade = coords.x / 0.1;
    if (coords.x > 0.9)
        edgeFade = (1.0 - coords.x) / 0.1;
    
    // Base color sampling with noise displacement
    float2 displacedCoords = coords + float2(combinedNoise * 0.05, 0);
    float4 baseColor = tex2D(uImage0, displacedCoords);
    
    // Apply edge opacity
    baseColor.a *= edgeFade * edgeOpacity;
    
    return baseColor;
}

technique Technique1
{
    pass ProfanedLavaPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
```

---

## Profaned Lava Metaball System

### Metaball Edge Rendering

```csharp
// File: ProfanedLavaMetaball.cs (Lines 50-100)
public class ProfanedLavaMetaball : BasePrimitiveGroupDrawer
{
    public override void DrawPrimitiveGroups(SpriteBatch spriteBatch)
    {
        // Render metaballs to target
        RenderToMetaballTarget();
        
        // Draw metaball result with edge shader
        DrawMetaballResult(spriteBatch);
    }
    
    private void RenderToMetaballTarget()
    {
        // Switch to metaball render target
        Main.instance.GraphicsDevice.SetRenderTarget(MetaballTarget);
        Main.instance.GraphicsDevice.Clear(Color.Transparent);
        
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, ...);
        
        // Draw each lava blob
        foreach (var blob in LavaBlobs)
        {
            Texture2D blobTex = InfernumTextureRegistry.BloomCircle.Value;
            float scale = blob.Radius / (blobTex.Width * 0.5f);
            Color blobColor = Color.White * blob.Intensity;
            
            Main.spriteBatch.Draw(blobTex, blob.Position - Main.screenPosition, 
                null, blobColor, 0f, blobTex.Size() * 0.5f, scale, 0, 0f);
        }
        
        Main.spriteBatch.End();
        
        // Switch back to main target
        Main.instance.GraphicsDevice.SetRenderTarget(Main.screenTarget);
    }
    
    private void DrawMetaballResult(SpriteBatch spriteBatch)
    {
        // Apply metaball edge shader
        Effect metaballEdge = InfernumEffectsRegistry.MetaballEdgeShader;
        metaballEdge.Parameters["threshold"].SetValue(0.5f);
        metaballEdge.Parameters["edgeColor"].SetValue(Color.Orange.ToVector4());
        metaballEdge.Parameters["innerColor"].SetValue(Color.Yellow.ToVector4());
        
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, ...);
        metaballEdge.CurrentTechnique.Passes[0].Apply();
        
        spriteBatch.Draw(MetaballTarget, Vector2.Zero, Color.White);
        
        spriteBatch.End();
    }
}
```

---

## Key Shaders and Textures

### Shaders Used

| Shader | Purpose | Key Parameters |
|--------|---------|----------------|
| `GuardiansLaserVertexShader` | Main lava beams | Color, SecondaryColor, Texture |
| `SideStreakVertexShader` | Telegraph lines | Opacity, Texture |
| `GenericLaserVertexShader` | Holy deathrays | Texture, StrongerFade |
| `PulsatingLaserVertexShader` | Energy absorption | PulseSpeed, Intensity |
| `AreaBorderVertexShader` | Arena boundaries | Intensity, Texture |
| `ImpFlameTrail` | Spear trails | Texture |
| `ProfanedLava` | HLSL lava overlay | EdgeOpacity, ScrollSpeed |

### Textures Used

| Texture | Purpose |
|---------|---------|
| `LavaNoise` | Primary lava flow texture |
| `CultistRayMap` | Secondary laser detail |
| `HarshNoise` | Aggressive beam edges |
| `HoneycombNoise` | Organic fire pattern |
| `StreakSolid` | Telegraph lines |
| `StreakMagma` | Pulsating beam body |
| `BloomCircle` | Metaball blobs |

---

## Holy Fire Color Palette

### Core Colors

```csharp
// Profaned Guardians Holy Fire Palette
public static class ProfanedColors
{
    public static Color HolyGold = new Color(255, 220, 150);
    public static Color HolyOrange = new Color(255, 150, 80);
    public static Color HolyFire = new Color(255, 130, 50);
    public static Color HolyEdge = new Color(200, 80, 30);
    public static Color HolyCore = new Color(255, 240, 200);
    public static Color LavaOrange = new Color(255, 100, 30);
    public static Color LavaYellow = new Color(255, 200, 100);
}
```

---

## Adaptation Guidelines for MagnumOpus

### 1. La Campanella Connection

Profaned fire shares orange-gold palette with La Campanella's infernal theme:

```csharp
// La Campanella lava effect adaptation
public Color CampanellaLavaColor(float completionRatio)
{
    Color black = ThemedParticles.CampanellaBlack;
    Color orange = ThemedParticles.CampanellaOrange;
    Color gold = ThemedParticles.CampanellaGold;
    
    // Three-stage gradient: Black → Orange → Gold
    if (completionRatio < 0.5f)
        return Color.Lerp(black, orange, completionRatio * 2f);
    else
        return Color.Lerp(orange, gold, (completionRatio - 0.5f) * 2f);
}
```

### 2. Telegraph System

Adapt SideStreakVertexShader pattern for attack warnings:

```csharp
public void DrawAttackTelegraph(Vector2 start, Vector2 end, float intensity, Color themeColor)
{
    // Clean warning line effect
    int pointCount = 8;
    Vector2[] points = new Vector2[pointCount];
    
    for (int i = 0; i < pointCount; i++)
    {
        points[i] = Vector2.Lerp(start, end, i / (float)(pointCount - 1));
    }
    
    // Width pulses to draw attention
    float pulse = Sin(Main.GlobalTimeWrappedHourly * 6f) * 0.3f + 0.7f;
    float width = 6f * intensity * pulse;
    
    // Draw with additive blend for glow
    Main.spriteBatch.SetBlendState(BlendState.Additive);
    DrawLinePrimitive(points, width, themeColor * intensity);
    Main.spriteBatch.SetBlendState(BlendState.AlphaBlend);
}
```

### 3. Fire Wall with Gap

Implement player-escapable walls:

```csharp
public void DrawFireWallWithGap(Vector2 wallStart, Vector2 wallEnd, Vector2 gapCenter, 
    float gapWidth, Color themeColor)
{
    Vector2 direction = (wallEnd - wallStart).SafeNormalize(Vector2.UnitX);
    float totalLength = Vector2.Distance(wallStart, wallEnd);
    float gapStartDist = Vector2.Distance(wallStart, gapCenter) - gapWidth * 0.5f;
    float gapEndDist = Vector2.Distance(wallStart, gapCenter) + gapWidth * 0.5f;
    
    // Segment before gap
    if (gapStartDist > 0)
    {
        DrawFireSegment(wallStart, wallStart + direction * gapStartDist, themeColor);
    }
    
    // Segment after gap
    if (gapEndDist < totalLength)
    {
        Vector2 afterGap = wallStart + direction * gapEndDist;
        DrawFireSegment(afterGap, wallEnd, themeColor);
    }
}
```

### 4. Circular Arena Boundary

For boss arena effects:

```csharp
public void DrawArenaBoundary(Vector2 center, float radius, Color themeColor, float intensity)
{
    int segments = 48;
    
    for (int i = 0; i < segments; i++)
    {
        float startAngle = TwoPi * i / segments;
        float endAngle = TwoPi * (i + 1) / segments;
        
        Vector2 start = center + startAngle.ToRotationVector2() * radius;
        Vector2 end = center + endAngle.ToRotationVector2() * radius;
        
        // Gradient around circle for visual interest
        float gradientT = i / (float)segments;
        Color segmentColor = Color.Lerp(themeColor, Color.White, gradientT * 0.3f) * intensity;
        
        // Draw fire particle at each segment point
        CustomParticles.GenericFlare(start, segmentColor, 0.4f * intensity, 8);
    }
    
    // Halo ring for boundary glow
    CustomParticles.HaloRing(center, themeColor * 0.5f, radius / 100f, 20);
}
```

### 5. Energy Absorption Effect

For charging attacks that pull energy:

```csharp
public void DrawEnergyAbsorption(Vector2 source, Vector2 target, float progress, 
    Color primaryColor, Color secondaryColor)
{
    int particleCount = (int)(progress * 12);
    
    for (int i = 0; i < particleCount; i++)
    {
        float t = i / (float)particleCount;
        float adjustedT = t + Main.GlobalTimeWrappedHourly * 2f;
        adjustedT %= 1f;
        
        Vector2 pos = Vector2.Lerp(source, target, adjustedT);
        
        // Particles move from source toward target
        Vector2 velocity = (target - source).SafeNormalize(Vector2.Zero) * 3f;
        
        Color color = Color.Lerp(secondaryColor, primaryColor, adjustedT) * (1f - adjustedT);
        CustomParticles.GenericGlow(pos, velocity, color, 0.3f, 15, true);
    }
    
    // Glow at target (collection point)
    CustomParticles.GenericFlare(target, primaryColor * progress, 0.5f * progress, 10);
}
```

---

## Key Takeaways

1. **Dual Texture Sampling:** GuardiansLaserVertexShader uses two textures for complex lava
2. **Telegraph System:** SideStreakVertexShader creates clean warning lines
3. **Edge Opacity:** Shader-based edge tapering for smooth falloff
4. **Gap Mechanics:** Fire walls with escapable gaps for fairness
5. **Pulsating Effects:** PulsatingLaserVertexShader for energy absorption
6. **Arena Borders:** Circular boundaries using AreaBorderVertexShader
7. **Metaball System:** Render target + threshold shader for organic shapes

---

## File References

- `Content/BehaviorOverrides/BossAIs/ProfanedGuardians/LavaEruptionPillar.cs`
- `Content/BehaviorOverrides/BossAIs/ProfanedGuardians/HolySpinningFireBeam.cs`
- `Content/BehaviorOverrides/BossAIs/ProfanedGuardians/HolyAimedDeathray.cs`
- `Content/BehaviorOverrides/BossAIs/ProfanedGuardians/HolyFireWall.cs`
- `Content/BehaviorOverrides/BossAIs/ProfanedGuardians/HolySineSpear.cs`
- `Content/BehaviorOverrides/BossAIs/ProfanedGuardians/DefenderGuardianBehaviorOverride.cs`
- `Content/BehaviorOverrides/BossAIs/ProfanedGuardians/AttackerGuardianBehaviorOverride.cs`
- `Content/BehaviorOverrides/BossAIs/ProfanedGuardians/ProfanedLavaMetaball.cs`
- `Assets/Effects/Overlays/ProfanedLava.fx`
