# Yharon VFX Design Document
## Analysis of InfernumMode's Yharon Dragon of Rebirth Visual Effects

---

## Overview

Yharon, the Dragon of Rebirth, features extensive fire-based visual effects including burn shaders, tornado primitives, flamethrower rendering, and explosive sun-like bursts. This document details the implementation techniques for adaptation into MagnumOpus.

---

## Core Fire Systems

### YharonBurnShader

The signature molten burn effect applied to Yharon's body:

```csharp
// File: YharonBehaviorOverride.cs (Lines ~1800)
// DrawInstance method with burn shader application

public static void DrawInstance(NPC npc, SpriteBatch spriteBatch, Color lightColor)
{
    var burnShader = InfernumEffectsRegistry.YharonBurnShader;
    
    // Configure burn parameters
    burnShader.Shader.Parameters["uTimeFactor"].SetValue(Main.GlobalTimeWrappedHourly * 1.5f);
    burnShader.Shader.Parameters["uZoomFactor"].SetValue(2.5f);
    burnShader.Shader.Parameters["uNoiseTexture"].SetValue(InfernumTextureRegistry.LavaNoise.Value);
    
    Main.spriteBatch.EnterShaderRegion();
    burnShader.Apply();
    
    // Draw Yharon with burn effect
    Main.spriteBatch.Draw(texture, drawPosition, frame, Color.White, 
        npc.rotation, origin, scale, effects, 0f);
    
    Main.spriteBatch.ExitShaderRegion();
}
```

**Shader Parameters:**
- `uTimeFactor`: Animation speed multiplier
- `uZoomFactor`: Noise texture zoom level
- `uNoiseTexture`: LavaNoise texture for molten effect

### Fire Intensity Afterimages

During enraged/high-intensity phases, afterimages are drawn:

```csharp
// Fire intensity system with afterimage rendering
if (fireIntensity > 0.5f)
{
    int afterimageCount = (int)(fireIntensity * 8);
    for (int i = 1; i <= afterimageCount; i++)
    {
        float progress = i / (float)afterimageCount;
        Color afterimageColor = Color.Lerp(Color.Yellow, Color.Red, progress) * (1f - progress);
        afterimageColor *= fireIntensity;
        
        Vector2 afterimagePos = npc.oldPos[i] + npc.Size * 0.5f - Main.screenPosition;
        Main.spriteBatch.Draw(texture, afterimagePos, frame, afterimageColor, 
            npc.oldRot[i], origin, scale, effects, 0f);
    }
}
```

---

## Draconic Infernado (Tornado)

### Primitive Trail Tornado

The fire tornado uses `PrimitiveTrailCopy` for smooth spiral rendering:

```csharp
// File: DraconicInfernado.cs (Full implementation)
public class DraconicInfernado : ModProjectile, IPixelPrimitiveDrawer
{
    public PrimitiveTrailCopy TornadoDrawer;
    
    public float TornadoWidthFunction(float completionRatio)
    {
        // Wide at base, narrow at top
        float baseWidth = 150f;
        float topWidth = 30f;
        float taperCurve = Pow(1f - completionRatio, 0.7f);
        return Lerp(topWidth, baseWidth, taperCurve) * Projectile.scale;
    }
    
    public Color TornadoColorFunction(float completionRatio)
    {
        // Yellow-orange at base, red-black at top
        Color baseColor = new Color(255, 200, 50);
        Color topColor = new Color(200, 50, 30);
        float fade = Pow(completionRatio, 1.5f);
        return Color.Lerp(baseColor, topColor, fade) * Projectile.Opacity;
    }
    
    public void DrawPixelPrimitives(SpriteBatch spriteBatch)
    {
        TornadoDrawer ??= new PrimitiveTrailCopy(TornadoWidthFunction, TornadoColorFunction, 
            null, true, InfernumEffectsRegistry.FireVertexShader);
        
        InfernumEffectsRegistry.FireVertexShader.UseSaturation(0.8f);
        InfernumEffectsRegistry.FireVertexShader.SetShaderTexture(
            InfernumTextureRegistry.HarshNoise);
        
        // Generate spiral points
        List<Vector2> tornadoPoints = new();
        for (int i = 0; i < 30; i++)
        {
            float height = i / 29f * TornadoHeight;
            float spiralAngle = i * 0.5f + Main.GlobalTimeWrappedHourly * 3f;
            float radius = TornadoWidthFunction(i / 29f) * 0.3f;
            
            Vector2 offset = spiralAngle.ToRotationVector2() * radius;
            tornadoPoints.Add(Projectile.Center - Vector2.UnitY * height + offset);
        }
        
        TornadoDrawer.DrawPixelated(tornadoPoints, -Main.screenPosition, 40);
    }
}
```

**Key Features:**
- **Spiral Points:** 30 points with rotating offset
- **Width Taper:** Wide base (150) to narrow top (30)
- **Color Gradient:** Yellow → Orange → Red → Black
- **Animation:** GlobalTimeWrappedHourly for spinning

---

## Yharon Flamethrower

### Multi-Stage Flame Gradient

The flamethrower uses a sophisticated color progression:

```csharp
// File: YharonFlamethrower.cs (Lines 40-80)
public const float MaxFlameLength = 1450f;

public Color FlameColorFunction(float completionRatio)
{
    // 5-stage color gradient: White → Yellow → Orange → Red → Black
    Color[] stages = new Color[]
    {
        Color.White,
        new Color(255, 255, 150),  // Pale yellow
        new Color(255, 200, 50),   // Golden
        new Color(255, 100, 30),   // Orange
        new Color(200, 50, 20),    // Red
        new Color(30, 10, 5)       // Near black
    };
    
    float stageProgress = completionRatio * (stages.Length - 1);
    int stageIndex = (int)stageProgress;
    float stageLerp = stageProgress - stageIndex;
    
    if (stageIndex >= stages.Length - 1)
        return stages[stages.Length - 1] * Projectile.Opacity;
    
    return Color.Lerp(stages[stageIndex], stages[stageIndex + 1], stageLerp) * Projectile.Opacity;
}

public float FlameWidthFunction(float completionRatio)
{
    // Bell curve with slight taper at end
    float bellCurve = Sin(completionRatio * Pi);
    float endTaper = 1f - Pow(completionRatio, 3);
    return 80f * bellCurve * endTaper * Projectile.scale;
}
```

### Flamethrower Drawing

```csharp
public void DrawPixelPrimitives(SpriteBatch spriteBatch)
{
    FlameDrawer ??= new PrimitiveTrailCopy(FlameWidthFunction, FlameColorFunction, 
        null, true, InfernumEffectsRegistry.FireVertexShader);
    
    InfernumEffectsRegistry.FireVertexShader.UseSaturation(1.2f);
    InfernumEffectsRegistry.FireVertexShader.SetShaderTexture(
        InfernumTextureRegistry.StreakMagma);
    
    // Calculate flame points with slight wavering
    List<Vector2> flamePoints = new();
    for (int i = 0; i < 24; i++)
    {
        float progress = i / 23f;
        float waver = Sin(progress * 10f + Time * 0.5f) * 15f * progress;
        Vector2 perpendicular = Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(PiOver2);
        
        Vector2 point = Projectile.Center + Projectile.velocity * progress * MaxFlameLength;
        point += perpendicular * waver;
        flamePoints.Add(point);
    }
    
    FlameDrawer.DrawPixelated(flamePoints, -Main.screenPosition, 50);
}
```

---

## Yharon Boom (Explosion)

### ForceField Shader Effect

```csharp
// File: YharonBoom.cs (Lines 50-90)
public override bool PreDraw(ref Color lightColor)
{
    var forceFieldShader = GameShaders.Misc["CalamityMod:ForceField"];
    
    // Pulsing scale animation
    float pulseScale = 1f + Sin(Time * 0.3f) * 0.15f;
    float explosionProgress = Time / Lifetime;
    float currentRadius = MaxRadius * Easing.OutQuad(explosionProgress) * pulseScale;
    
    // Color transition: Yellow → Orange → Red
    Color innerColor = Color.Lerp(Color.Yellow, Color.Orange, explosionProgress);
    Color outerColor = Color.Lerp(Color.Orange, Color.Red, explosionProgress);
    
    forceFieldShader.UseColor(innerColor);
    forceFieldShader.UseSecondaryColor(outerColor);
    forceFieldShader.UseOpacity(1f - explosionProgress);
    
    Main.spriteBatch.EnterShaderRegion();
    forceFieldShader.Apply();
    
    // Draw expanding circle
    Texture2D circleTexture = InfernumTextureRegistry.BloomCircle.Value;
    Vector2 scale = new Vector2(currentRadius) / circleTexture.Size();
    Main.spriteBatch.Draw(circleTexture, Projectile.Center - Main.screenPosition, 
        null, Color.White, 0f, circleTexture.Size() * 0.5f, scale, 0, 0f);
    
    Main.spriteBatch.ExitShaderRegion();
    return false;
}
```

---

## Yharon Flame Explosion

### Radial Fire Burst

```csharp
// File: YharonFlameExplosion.cs (Lines 70-100)
public override bool PreDraw(ref Color lightColor)
{
    FireDrawer ??= new PrimitiveTrailCopy(SunWidthFunction, SunColorFunction, 
        null, true, InfernumEffectsRegistry.FireVertexShader);

    InfernumEffectsRegistry.FireVertexShader.UseSaturation(0.45f);
    InfernumEffectsRegistry.FireVertexShader.UseImage1("Images/Misc/Perlin");

    // Draw radial fire segments
    for (float offsetAngle = -PiOver2; offsetAngle <= PiOver2; offsetAngle += Pi / 10f)
    {
        List<Vector2> drawPoints = new();
        
        float adjustedAngle = offsetAngle + Pi * -0.2f;
        Vector2 offsetDirection = adjustedAngle.ToRotationVector2();
        
        for (int i = 0; i < 16; i++)
        {
            float progress = i / 15f;
            float radius = Projectile.scale * progress * 150f;
            drawPoints.Add(Projectile.Center + offsetDirection * radius);
        }
        
        FireDrawer.Draw(drawPoints, -Main.screenPosition, 25);
    }
    
    return false;
}

public float SunWidthFunction(float completionRatio)
{
    float tipCurve = Pow(1f - completionRatio, 2);
    return 40f * tipCurve * Projectile.scale;
}

public Color SunColorFunction(float completionRatio)
{
    Color inner = Color.White;
    Color outer = new Color(255, 100, 30);
    return Color.Lerp(inner, outer, completionRatio) * Projectile.Opacity;
}
```

### Additive Blend Drawing

```csharp
// Uses additive blending for intense glow
Main.spriteBatch.End();
Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, ...);

// XerocLight texture for central glow
Texture2D glowTexture = ModContent.Request<Texture2D>("CalamityMod/Particles/XerocLight").Value;
float scaleProgress = Easing.OutQuad(Time / Lifetime);
float scale = MaxScale * scaleProgress;

Main.spriteBatch.Draw(glowTexture, Projectile.Center - Main.screenPosition, 
    null, Color.Yellow * (1f - scaleProgress), 0f, glowTexture.Size() * 0.5f, scale, 0, 0f);

Main.spriteBatch.End();
Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, ...);
```

---

## Homing Fireball

### Fire Particle Burst on Impact

```csharp
// File: HomingFireball.cs (Impact effects)
public override void OnKill(int timeLeft)
{
    // FireballParticle burst
    for (int i = 0; i < 15; i++)
    {
        Vector2 velocity = Main.rand.NextVector2Circular(8f, 8f);
        float scale = Main.rand.NextFloat(0.8f, 1.4f);
        int lifetime = Main.rand.Next(30, 50);
        
        FireballParticle fire = new(Projectile.Center, velocity, 
            Color.Yellow, Color.Red, scale, lifetime);
        GeneralParticleHandler.SpawnParticle(fire);
    }
    
    // HeavySmokeParticle for smoke
    for (int i = 0; i < 8; i++)
    {
        Vector2 velocity = -Vector2.UnitY.RotatedByRandom(0.5f) * Main.rand.NextFloat(2f, 5f);
        var smoke = new HeavySmokeParticle(Projectile.Center, velocity, 
            Color.DarkGray, Main.rand.Next(40, 70), 0.6f, 1.2f, 0.02f, true);
        GeneralParticleHandler.SpawnParticle(smoke);
    }
}
```

### Trail Rendering

```csharp
public void DrawPixelPrimitives(SpriteBatch spriteBatch)
{
    TrailDrawer ??= new PrimitiveTrailCopy(TrailWidthFunction, TrailColorFunction, 
        null, true, GameShaders.Misc["CalamityMod:ImpFlameTrail"]);
    
    GameShaders.Misc["CalamityMod:ImpFlameTrail"].SetShaderTexture(
        InfernumTextureRegistry.StreakMagma);
    
    TrailDrawer.DrawPixelated(Projectile.oldPos, Projectile.Size * 0.5f - Main.screenPosition, 35);
}
```

---

## Vortex Telegraph Beam

### Fire Shader Telegraph

```csharp
// File: VortexTelegraphBeam.cs
public void DrawPixelPrimitives(SpriteBatch spriteBatch)
{
    TelegraphDrawer ??= new PrimitiveTrailCopy(TelegraphWidthFunction, TelegraphColorFunction, 
        null, true, InfernumEffectsRegistry.FireVertexShader);
    
    InfernumEffectsRegistry.FireVertexShader.UseSaturation(0.8f);
    InfernumEffectsRegistry.FireVertexShader.SetShaderTexture(
        InfernumTextureRegistry.CultistRayMap);
    
    // Pulsing width for telegraph warning
    float pulse = Sin(Time * 0.5f) * 0.3f + 1f;
    
    Vector2[] points = new Vector2[12];
    for (int i = 0; i < 12; i++)
        points[i] = Vector2.Lerp(StartPosition, EndPosition, i / 11f);
    
    TelegraphDrawer.DrawPixelated(points, -Main.screenPosition, 20);
}

public float TelegraphWidthFunction(float completionRatio)
{
    float pulse = Sin(Main.GlobalTimeWrappedHourly * 8f) * 0.2f + 1f;
    return 20f * pulse * Sin(completionRatio * Pi);
}

public Color TelegraphColorFunction(float completionRatio)
{
    return Color.Lerp(Color.Yellow, Color.OrangeRed, completionRatio) * 0.7f;
}
```

---

## Yharon Sky System

### Custom Sky with Smoke Particles

```csharp
// File: YharonSky.cs
public class YharonSky : CustomSky
{
    private List<BackgroundSmoke> backgroundSmoke = new();
    
    public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
    {
        // Draw ambient background smoke
        foreach (var smoke in backgroundSmoke)
        {
            if (smoke.Depth >= minDepth && smoke.Depth < maxDepth)
            {
                Texture2D smokeTex = InfernumTextureRegistry.Smoke.Value;
                Color smokeColor = Color.Lerp(Color.DarkGray, Color.Black, smoke.Depth / 10f);
                smokeColor *= smoke.Opacity;
                
                spriteBatch.Draw(smokeTex, smoke.Position, null, smokeColor, 
                    smoke.Rotation, smokeTex.Size() * 0.5f, smoke.Scale, 0, 0f);
            }
        }
        
        // Orange ambient glow at horizon
        if (maxDepth >= 9f)
        {
            Texture2D glowTex = InfernumTextureRegistry.BloomFlare.Value;
            Vector2 horizonPos = new Vector2(Main.screenWidth * 0.5f, Main.screenHeight * 0.9f);
            Color glowColor = new Color(255, 100, 30) * 0.3f * Intensity;
            
            spriteBatch.Draw(glowTex, horizonPos, null, glowColor, 0f, 
                glowTex.Size() * 0.5f, 3f, 0, 0f);
        }
    }
    
    public override void Update(GameTime gameTime)
    {
        // Update smoke positions
        foreach (var smoke in backgroundSmoke)
        {
            smoke.Position += smoke.Velocity;
            smoke.Rotation += smoke.RotationSpeed;
            smoke.Opacity *= 0.995f;
        }
        
        // Spawn new smoke
        if (Main.rand.NextBool(5))
        {
            backgroundSmoke.Add(new BackgroundSmoke
            {
                Position = new Vector2(Main.rand.Next(Main.screenWidth), Main.screenHeight + 50),
                Velocity = new Vector2(Main.rand.NextFloat(-1f, 1f), -Main.rand.NextFloat(0.5f, 2f)),
                Scale = Main.rand.NextFloat(0.5f, 2f),
                Opacity = Main.rand.NextFloat(0.3f, 0.7f),
                Depth = Main.rand.NextFloat(1f, 9f)
            });
        }
    }
}
```

---

## Key Shaders and Textures

### Shaders Used

| Shader | Purpose | Key Parameters |
|--------|---------|----------------|
| `YharonBurnShader` | Body burn effect | uTimeFactor, uZoomFactor, uNoiseTexture |
| `FireVertexShader` | All fire primitives | Saturation, Texture |
| `ForceField` | Explosion sphere | Color, SecondaryColor, Opacity |
| `ImpFlameTrail` | Fireball trails | Texture |

### Textures Used

| Texture | Purpose |
|---------|---------|
| `LavaNoise` | Burn shader noise |
| `StreakMagma` | Fire trail map |
| `HarshNoise` | Tornado fire texture |
| `CultistRayMap` | Telegraph beams |
| `XerocLight` | Explosion glow |
| `BloomCircle` | Shockwave base |

---

## Adaptation Guidelines for MagnumOpus

### 1. La Campanella Fire Effects

The closest theme match - adapt Yharon's fire system:

```csharp
// La Campanella Flamethrower Color
public Color CampanellaFlameColor(float completionRatio)
{
    Color[] stages = new Color[]
    {
        ThemedParticles.CampanellaYellow,
        ThemedParticles.CampanellaOrange,
        ThemedParticles.CampanellaRed,
        ThemedParticles.CampanellaBlack
    };
    
    return LerpThroughColors(stages, completionRatio);
}
```

### 2. Smoke Integration

Always pair fire with smoke for La Campanella:

```csharp
// HeavySmokeParticle on all fire impacts
foreach (var impact in fireImpacts)
{
    var smoke = new HeavySmokeParticle(
        impact.Position,
        -Vector2.UnitY * Main.rand.NextFloat(1f, 3f),
        ThemedParticles.CampanellaBlack,
        Main.rand.Next(30, 50),
        0.4f, 0.8f, 0.02f, true);
    MagnumParticleHandler.SpawnParticle(smoke);
}
```

### 3. Tornado Spiral Pattern

For themed tornado/vortex effects:

```csharp
public List<Vector2> GenerateSpiralPoints(Vector2 center, float height, int pointCount)
{
    List<Vector2> points = new();
    for (int i = 0; i < pointCount; i++)
    {
        float progress = i / (float)(pointCount - 1);
        float heightOffset = progress * height;
        float angle = progress * TwoPi * 3f + Main.GlobalTimeWrappedHourly * 4f;
        float radius = (1f - progress) * 50f;
        
        Vector2 offset = angle.ToRotationVector2() * radius;
        points.Add(center - Vector2.UnitY * heightOffset + offset);
    }
    return points;
}
```

### 4. Radial Sun Burst Pattern

For explosion effects:

```csharp
public void DrawSunBurst(Vector2 center, float radius, int rayCount)
{
    for (int ray = 0; ray < rayCount; ray++)
    {
        float angle = TwoPi * ray / rayCount;
        Vector2 direction = angle.ToRotationVector2();
        
        List<Vector2> rayPoints = new();
        for (int i = 0; i < 16; i++)
        {
            float progress = i / 15f;
            rayPoints.Add(center + direction * progress * radius);
        }
        
        FireDrawer.Draw(rayPoints, -Main.screenPosition, 20);
    }
}
```

---

## Key Takeaways

1. **Multi-Stage Color Gradients:** Fire effects use 4-6 color stages for realism
2. **Width Functions:** Bell curves with end taper create natural flame shapes
3. **Smoke Pairing:** Every fire effect should have accompanying smoke
4. **Radial Patterns:** Sun/explosion effects use multiple angle-offset draws
5. **Spiral Generation:** Tornados use time-animated spiral point generation
6. **Burn Shaders:** Body effects use dedicated shaders with noise textures
7. **Background Integration:** Custom sky systems enhance atmospheric effects

---

## File References

- `Content/BehaviorOverrides/BossAIs/Yharon/YharonBehaviorOverride.cs`
- `Content/BehaviorOverrides/BossAIs/Yharon/DraconicInfernado.cs`
- `Content/BehaviorOverrides/BossAIs/Yharon/YharonFlamethrower.cs`
- `Content/BehaviorOverrides/BossAIs/Yharon/YharonBoom.cs`
- `Content/BehaviorOverrides/BossAIs/Yharon/YharonFlameExplosion.cs`
- `Content/BehaviorOverrides/BossAIs/Yharon/HomingFireball.cs`
- `Content/BehaviorOverrides/BossAIs/Yharon/VortexTelegraphBeam.cs`
- `Content/Skies/YharonSky.cs`
