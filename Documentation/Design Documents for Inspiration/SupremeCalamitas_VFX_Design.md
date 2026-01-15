# Supreme Calamitas VFX Design Document
## Analysis of InfernumMode's Supreme Witch Calamitas Visual Effects

---

## Overview

Supreme Calamitas features brimstone-themed visual effects including dark flame shaders, pulsating laser beams, ritual symbols, and dramatic phase transition effects. This document details the implementation techniques for adaptation into MagnumOpus.

---

## Core Brimstone Systems

### BrimstoneLaserbeam

The primary laser attack using `FlameVertexShader`:

```csharp
// File: BrimstoneLaserbeam.cs (Lines 80-120)
public class BrimstoneLaserbeam : ModProjectile, IPixelPrimitiveDrawer
{
    public PrimitiveTrailCopy BeamDrawer;
    
    public const float MaxLaserLength = 3330f;
    public const int BasePointCount = 24;
    
    public float LaserWidthFunction(float completionRatio)
    {
        float tipFade = Pow(Sin(completionRatio * Pi), 0.6f);
        float pulsation = Sin(Main.GlobalTimeWrappedHourly * 6f + completionRatio * 10f) * 0.1f + 1f;
        return 60f * tipFade * pulsation * Projectile.scale;
    }
    
    public Color LaserColorFunction(float completionRatio)
    {
        // Dark red to orange gradient with black edges
        Color core = new Color(255, 100, 50);
        Color edge = new Color(150, 30, 30);
        float fade = Sin(completionRatio * Pi);
        return Color.Lerp(edge, core, fade) * Projectile.Opacity;
    }
    
    public void DrawPixelPrimitives(SpriteBatch spriteBatch)
    {
        BeamDrawer ??= new PrimitiveTrailCopy(LaserWidthFunction, LaserColorFunction, 
            null, true, InfernumEffectsRegistry.FlameVertexShader);
        
        // Configure flame shader
        InfernumEffectsRegistry.FlameVertexShader.UseSaturation(0.8f);
        InfernumEffectsRegistry.FlameVertexShader.SetShaderTexture(
            InfernumTextureRegistry.BlurryPerlinNoise);
        
        // Generate laser points
        Vector2[] basePoints = new Vector2[BasePointCount];
        for (int i = 0; i < BasePointCount; i++)
            basePoints[i] = Vector2.Lerp(Projectile.Center, 
                Projectile.Center + Projectile.velocity * MaxLaserLength, i / (float)(BasePointCount - 1));
        
        BeamDrawer.DrawPixelated(basePoints, -Main.screenPosition, 40);
    }
}
```

**Key Parameters:**
- **Max Length:** 3330 units
- **Point Count:** 24 base points
- **Shader:** FlameVertexShader with BlurryPerlinNoise texture
- **Sample Count:** 40 for smooth rendering

---

## Brimstone Flame Orb

### Radial Telegraph Lines

```csharp
// File: BrimstoneFlameOrb.cs (Lines 100-160)
public class BrimstoneFlameOrb : ModProjectile, IPixelPrimitiveDrawer
{
    public PrimitiveTrailCopy FireDrawer;
    
    public void DrawPixelPrimitives(SpriteBatch spriteBatch)
    {
        FireDrawer ??= new PrimitiveTrailCopy(OrbWidthFunction, OrbColorFunction, 
            null, true, InfernumEffectsRegistry.PrismaticRayVertexShader);
        
        // Configure prismatic ray shader
        InfernumEffectsRegistry.PrismaticRayVertexShader.UseOpacity(0.6f);
        InfernumEffectsRegistry.PrismaticRayVertexShader.UseImage1("Images/Misc/Perlin");
        Main.instance.GraphicsDevice.Textures[2] = InfernumTextureRegistry.StreakSolid.Value;
        
        float radius = Radius;
        
        // Draw radial telegraph lines
        for (int i = 0; i < 8; i++)
        {
            float angle = TwoPi * i / 8f + Main.GlobalTimeWrappedHourly * 0.5f;
            float adjustedAngle = angle + Time * 0.02f;
            Vector2 offsetDirection = adjustedAngle.ToRotationVector2();
            
            List<Vector2> drawPoints = new();
            List<float> rotationPoints = new();
            
            for (int j = 0; j < 8; j++)
            {
                rotationPoints.Add(adjustedAngle);
                drawPoints.Add(Vector2.Lerp(
                    Projectile.Center - offsetDirection * radius / 2f, 
                    Projectile.Center + offsetDirection * radius / 2f, 
                    j / 7f));
            }
            
            FireDrawer.DrawPixelated(drawPoints, -Main.screenPosition, 39);
        }
    }
    
    public float OrbWidthFunction(float completionRatio)
    {
        return 15f * Sin(completionRatio * Pi);
    }
    
    public Color OrbColorFunction(float completionRatio)
    {
        Color brimstone = new Color(255, 80, 60);
        Color dark = new Color(100, 20, 20);
        return Color.Lerp(dark, brimstone, Sin(completionRatio * Pi)) * Projectile.Opacity;
    }
}
```

---

## Flame Overload Beam

### Prismatic Ray Shader with Trypophobia Noise

```csharp
// File: FlameOverloadBeam.cs (Lines 60-100)
public class FlameOverloadBeam : ModProjectile, IPixelPrimitiveDrawer
{
    public PrimitiveTrailCopy LaserDrawer;
    
    public void DrawPixelPrimitives(SpriteBatch spriteBatch)
    {
        LaserDrawer ??= new PrimitiveTrailCopy(WidthFunction, ColorFunction, 
            null, true, InfernumEffectsRegistry.PrismaticRayVertexShader);
        
        // Use TrypophobiaNoise for organic look
        InfernumEffectsRegistry.PrismaticRayVertexShader.UseOpacity(0.8f);
        InfernumEffectsRegistry.PrismaticRayVertexShader.UseImage1(
            InfernumTextureRegistry.TrypophobiaNoise);
        Main.instance.GraphicsDevice.Textures[2] = InfernumTextureRegistry.StreakSolid.Value;
        
        // Orange-based colors instead of pure brimstone
        // Creates visual variety while staying on theme
        
        Vector2[] points = new Vector2[16];
        for (int i = 0; i < 16; i++)
            points[i] = Vector2.Lerp(Projectile.Center, LaserEnd, i / 15f);
        
        LaserDrawer.DrawPixelated(points, -Main.screenPosition, 35);
    }
    
    public Color ColorFunction(float completionRatio)
    {
        // Orange gradient for variety
        Color inner = new Color(255, 150, 50);
        Color outer = new Color(200, 80, 30);
        return Color.Lerp(outer, inner, Sin(completionRatio * Pi));
    }
}
```

---

## Brimstone Flame Pillar

### Dark Flame Pillar Shader

```csharp
// File: BrimstoneFlamePillar.cs (Lines 50-100)
public class BrimstoneFlamePillar : ModProjectile, IPixelPrimitiveDrawer
{
    public PrimitiveTrailCopy PillarDrawer;
    
    public void DrawPixelPrimitives(SpriteBatch spriteBatch)
    {
        PillarDrawer ??= new PrimitiveTrailCopy(PillarWidthFunction, PillarColorFunction, 
            null, true, InfernumEffectsRegistry.DarkFlamePillarVertexShader);
        
        // StreakFaded texture for smooth falloff
        InfernumEffectsRegistry.DarkFlamePillarVertexShader.SetShaderTexture(
            InfernumTextureRegistry.StreakFaded);
        
        // Vertical pillar points
        Vector2[] pillarPoints = new Vector2[12];
        for (int i = 0; i < 12; i++)
        {
            float height = i / 11f * PillarHeight;
            pillarPoints[i] = Projectile.Center - Vector2.UnitY * height;
        }
        
        PillarDrawer.DrawPixelated(pillarPoints, -Main.screenPosition, 30);
    }
    
    public float PillarWidthFunction(float completionRatio)
    {
        // Wide at base, narrow at top with flickering
        float baseTaper = Pow(1f - completionRatio, 0.5f);
        float flicker = Sin(Main.GlobalTimeWrappedHourly * 15f + completionRatio * 8f) * 0.15f + 1f;
        return 80f * baseTaper * flicker;
    }
    
    public Color PillarColorFunction(float completionRatio)
    {
        // Dark at edges, bright at center
        Color dark = new Color(50, 10, 10);
        Color bright = new Color(255, 100, 60);
        float centerBrightness = Sin(completionRatio * Pi);
        return Color.Lerp(dark, bright, centerBrightness) * Projectile.Opacity;
    }
}
```

---

## Ritual Brimstone Heart

### Upward Beam Effect

```csharp
// File: RitualBrimstoneHeart.cs (Lines 80-120)
public class RitualBrimstoneHeart : ModProjectile, IPixelPrimitiveDrawer
{
    public const float LaserLength = 2700f;
    
    public void DrawPixelPrimitives(SpriteBatch spriteBatch)
    {
        BeamDrawer ??= new PrimitiveTrailCopy(BeamWidthFunction, BeamColorFunction, 
            null, true, InfernumEffectsRegistry.PrismaticRayVertexShader);
        
        // Upward beam configuration
        Vector2 beamDirection = -Vector2.UnitY;
        Vector2 beamEnd = Projectile.Center + beamDirection * LaserLength;
        
        InfernumEffectsRegistry.PrismaticRayVertexShader.UseOpacity(0.7f);
        InfernumEffectsRegistry.PrismaticRayVertexShader.UseImage1("Images/Misc/Perlin");
        
        Vector2[] points = new Vector2[20];
        for (int i = 0; i < 20; i++)
            points[i] = Vector2.Lerp(Projectile.Center, beamEnd, i / 19f);
        
        BeamDrawer.DrawPixelated(points, -Main.screenPosition, 40);
    }
}
```

---

## SCal Symbol Effects

### Alchemical Symbol Rendering

```csharp
// File: SCalSymbol.cs (Lines 40-80)
public class SCalSymbol : ModProjectile
{
    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D symbolTexture = ModContent.Request<Texture2D>(Texture).Value;
        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        Vector2 origin = symbolTexture.Size() * 0.5f;
        
        // Additive bloom layers
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, ...);
        
        // Outer glow - pink/red
        Color outerGlow = new Color(255, 100, 150) * 0.4f * Projectile.Opacity;
        Main.spriteBatch.Draw(symbolTexture, drawPosition, null, outerGlow, 
            Projectile.rotation, origin, Projectile.scale * 1.3f, 0, 0f);
        
        // Middle glow - red
        Color middleGlow = new Color(255, 50, 80) * 0.5f * Projectile.Opacity;
        Main.spriteBatch.Draw(symbolTexture, drawPosition, null, middleGlow, 
            Projectile.rotation, origin, Projectile.scale * 1.15f, 0, 0f);
        
        // Core symbol
        Color coreColor = Color.White * Projectile.Opacity;
        Main.spriteBatch.Draw(symbolTexture, drawPosition, null, coreColor, 
            Projectile.rotation, origin, Projectile.scale, 0, 0f);
        
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, ...);
        
        return false;
    }
}
```

---

## Berserk Phase Effects

### Eye Gleam and Rotational Aura

```csharp
// File: SupremeCalamitasBehaviorOverride.cs (PreDraw section)
public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Color lightColor)
{
    // During berserk phase
    if (IsBerserk)
    {
        // Pulsating eye gleam
        float pulseIntensity = Sin(Main.GlobalTimeWrappedHourly * 8f) * 0.3f + 0.7f;
        Color eyeGleamColor = Color.Violet * pulseIntensity;
        
        Texture2D gleamTexture = InfernumTextureRegistry.BloomFlare.Value;
        Vector2 eyePosition = npc.Center + GetEyeOffset(npc);
        
        Main.spriteBatch.SetBlendState(BlendState.Additive);
        Main.spriteBatch.Draw(gleamTexture, eyePosition - Main.screenPosition, 
            null, eyeGleamColor, Main.GlobalTimeWrappedHourly * 2f, 
            gleamTexture.Size() * 0.5f, 0.5f * pulseIntensity, 0, 0f);
        Main.spriteBatch.SetBlendState(BlendState.AlphaBlend);
        
        // Rotational aura
        float rotationSpeed = Main.GlobalTimeWrappedHourly * 3f;
        for (int i = 0; i < 6; i++)
        {
            float angle = TwoPi * i / 6f + rotationSpeed;
            Vector2 auraOffset = angle.ToRotationVector2() * 50f;
            Color auraColor = Color.Lerp(Color.Red, Color.Violet, i / 5f) * 0.4f;
            
            Texture2D auraTex = InfernumTextureRegistry.BloomCircle.Value;
            Main.spriteBatch.Draw(auraTex, npc.Center + auraOffset - Main.screenPosition, 
                null, auraColor, 0f, auraTex.Size() * 0.5f, 0.3f, 0, 0f);
        }
    }
    
    return base.PreDraw(npc, spriteBatch, lightColor);
}
```

---

## Energy Chargeup Effect

### PreDraw Power Effect

```csharp
// File: SupremeCalamitasBehaviorOverride.cs (Energy chargeup)
public static void DrawEnergyChargeup(NPC npc, float chargeProgress)
{
    if (chargeProgress <= 0f)
        return;
    
    // PowerEffect texture for energy gathering
    Texture2D powerTexture = ModContent.Request<Texture2D>("CalamityMod/Particles/PowerEffect").Value;
    
    Main.spriteBatch.SetBlendState(BlendState.Additive);
    
    // Converging energy particles
    for (int i = 0; i < 12; i++)
    {
        float angle = TwoPi * i / 12f + Main.GlobalTimeWrappedHourly * 2f;
        float distance = (1f - chargeProgress) * 200f + 30f;
        Vector2 particlePos = npc.Center + angle.ToRotationVector2() * distance;
        
        float scale = chargeProgress * 0.6f;
        Color particleColor = Color.Lerp(Color.Red, Color.Violet, i / 11f) * chargeProgress;
        
        Main.spriteBatch.Draw(powerTexture, particlePos - Main.screenPosition, 
            null, particleColor, angle + PiOver2, powerTexture.Size() * 0.5f, scale, 0, 0f);
    }
    
    // Central gathering glow
    Texture2D bloomTex = InfernumTextureRegistry.BloomCircle.Value;
    Color centralColor = Color.Lerp(Color.DarkRed, Color.White, chargeProgress) * chargeProgress * 0.8f;
    float centralScale = chargeProgress * 0.8f;
    
    Main.spriteBatch.Draw(bloomTex, npc.Center - Main.screenPosition, 
        null, centralColor, 0f, bloomTex.Size() * 0.5f, centralScale, 0, 0f);
    
    Main.spriteBatch.SetBlendState(BlendState.AlphaBlend);
}
```

---

## Suicide Bomber Demon

### Flame Trail with Cyan Glow

```csharp
// File: SuicideBomberDemonHostile.cs (Lines 80-120)
public class SuicideBomberDemonHostile : ModProjectile, IPixelPrimitiveDrawer
{
    public void DrawPixelPrimitives(SpriteBatch spriteBatch)
    {
        TrailDrawer ??= new PrimitiveTrailCopy(TrailWidthFunction, TrailColorFunction, 
            null, true, GameShaders.Misc["CalamityMod:ImpFlameTrail"]);
        
        GameShaders.Misc["CalamityMod:ImpFlameTrail"].SetShaderTexture(
            InfernumTextureRegistry.StreakMagma);
        
        TrailDrawer.DrawPixelated(Projectile.oldPos, Projectile.Size * 0.5f - Main.screenPosition, 30);
    }
    
    public override bool PreDraw(ref Color lightColor)
    {
        // Flame orb glow with cyan accent
        Texture2D glowTexture = InfernumTextureRegistry.BloomCircle.Value;
        Vector2 drawPos = Projectile.Center - Main.screenPosition;
        
        Main.spriteBatch.SetBlendState(BlendState.Additive);
        
        // Outer cyan glow (distinctive feature)
        Color cyanGlow = Color.Cyan * 0.4f * Projectile.Opacity;
        Main.spriteBatch.Draw(glowTexture, drawPos, null, cyanGlow, 0f, 
            glowTexture.Size() * 0.5f, 0.8f, 0, 0f);
        
        // Inner brimstone glow
        Color brimstoneGlow = new Color(255, 80, 60) * 0.6f * Projectile.Opacity;
        Main.spriteBatch.Draw(glowTexture, drawPos, null, brimstoneGlow, 0f, 
            glowTexture.Size() * 0.5f, 0.5f, 0, 0f);
        
        Main.spriteBatch.SetBlendState(BlendState.AlphaBlend);
        
        return true;
    }
}
```

---

## Demonic Explosion

### Fire Vertex Shader Explosion

```csharp
// File: DemonicExplosion.cs (Lines 73-100)
public override bool PreDraw(ref Color lightColor)
{
    FireDrawer ??= new PrimitiveTrailCopy(SunWidthFunction, SunColorFunction, 
        null, true, InfernumEffectsRegistry.FireVertexShader);

    InfernumEffectsRegistry.FireVertexShader.UseSaturation(0.45f);
    InfernumEffectsRegistry.FireVertexShader.UseImage1("Images/Misc/Perlin");

    // Radial explosion pattern
    for (float offsetAngle = -PiOver2; offsetAngle <= PiOver2; offsetAngle += Pi / 10f)
    {
        List<float> rotationPoints = new();
        List<Vector2> drawPoints = new();

        float adjustedAngle = offsetAngle + Pi * -0.2f;
        Vector2 offsetDirection = adjustedAngle.ToRotationVector2();
        
        for (int i = 0; i < 16; i++)
        {
            float progress = i / 15f;
            float radius = Projectile.scale * progress * ExplosionRadius;
            
            rotationPoints.Add(adjustedAngle);
            drawPoints.Add(Projectile.Center + offsetDirection * radius);
        }
        
        FireDrawer.Draw(drawPoints, -Main.screenPosition, 25);
    }
    
    return false;
}

public float SunWidthFunction(float completionRatio)
{
    return 50f * Pow(1f - completionRatio, 2) * Projectile.scale;
}

public Color SunColorFunction(float completionRatio)
{
    Color core = new Color(255, 150, 100);
    Color edge = new Color(150, 30, 30);
    return Color.Lerp(core, edge, completionRatio) * Projectile.Opacity;
}
```

---

## Key Shaders and Textures

### Shaders Used

| Shader | Purpose | Key Parameters |
|--------|---------|----------------|
| `FlameVertexShader` | Brimstone lasers | Saturation, Texture (BlurryPerlinNoise) |
| `PrismaticRayVertexShader` | Orb telegraphs, beams | Opacity, Image1, Image2 |
| `DarkFlamePillarVertexShader` | Flame pillars | Texture (StreakFaded) |
| `FireVertexShader` | Explosions | Saturation, Image1 |
| `ImpFlameTrail` | Demon trails | Texture (StreakMagma) |

### Textures Used

| Texture | Purpose |
|---------|---------|
| `BlurryPerlinNoise` | Laser flame effect |
| `TrypophobiaNoise` | Organic beam texture |
| `StreakFaded` | Pillar falloff |
| `StreakSolid` | Prismatic ray body |
| `StreakMagma` | Fire trails |
| `BloomFlare` | Eye gleam |
| `BloomCircle` | Aura particles |
| `PowerEffect` | Energy chargeup |

---

## Brimstone Color Palette

### Core Colors

```csharp
// Supreme Calamitas Brimstone Palette
public static class SCalColors
{
    public static Color BrimstoneCore = new Color(255, 80, 60);
    public static Color BrimstoneDark = new Color(150, 30, 30);
    public static Color BrimstoneBlack = new Color(50, 10, 10);
    public static Color BrimstoneOrange = new Color(255, 150, 50);
    public static Color BrimstoneViolet = new Color(180, 50, 150);
    public static Color BrimstonePink = new Color(255, 100, 150);
    public static Color BrimstoneCyan = Color.Cyan; // Accent only
}
```

---

## Adaptation Guidelines for MagnumOpus

### 1. Enigma Theme Connection

SCal's dark brimstone shares visual DNA with Enigma's mysterious fire:

```csharp
// Enigma adaptation of brimstone effects
public Color EnigmaFlameColor(float completionRatio)
{
    // Black → Purple → Green flame gradient
    Color[] stages = new Color[]
    {
        ThemedParticles.EnigmaBlack,
        ThemedParticles.EnigmaPurple,
        ThemedParticles.EnigmaGreenFlame
    };
    
    return LerpThroughColors(stages, completionRatio);
}
```

### 2. Symbol Effects for Magic Themes

Adapt SCalSymbol for glyph rendering:

```csharp
// Universal glyph bloom effect
public void DrawGlyphWithBloom(Vector2 position, Texture2D glyph, Color themeColor)
{
    Main.spriteBatch.SetBlendState(BlendState.Additive);
    
    // Three-layer bloom
    Main.spriteBatch.Draw(glyph, position, null, themeColor * 0.3f, 
        0f, glyph.Size() * 0.5f, 1.4f, 0, 0f);
    Main.spriteBatch.Draw(glyph, position, null, themeColor * 0.5f, 
        0f, glyph.Size() * 0.5f, 1.2f, 0, 0f);
    Main.spriteBatch.Draw(glyph, position, null, Color.White, 
        0f, glyph.Size() * 0.5f, 1f, 0, 0f);
    
    Main.spriteBatch.SetBlendState(BlendState.AlphaBlend);
}
```

### 3. Energy Chargeup Pattern

For boss attack windups:

```csharp
public void DrawConvergingEnergy(Vector2 center, float progress, Color themeColor)
{
    int particleCount = 12;
    float maxDistance = 200f;
    
    Main.spriteBatch.SetBlendState(BlendState.Additive);
    
    for (int i = 0; i < particleCount; i++)
    {
        float angle = TwoPi * i / particleCount + Main.GlobalTimeWrappedHourly * 2f;
        float distance = (1f - progress) * maxDistance + 20f;
        Vector2 pos = center + angle.ToRotationVector2() * distance;
        
        Color color = Color.Lerp(themeColor, Color.White, progress) * progress;
        float scale = progress * 0.5f;
        
        CustomParticles.GenericFlare(pos, color, scale, 10);
    }
    
    // Central bloom
    CustomParticles.HaloRing(center, Color.Lerp(themeColor, Color.White, progress), 
        progress * 0.6f, 15);
    
    Main.spriteBatch.SetBlendState(BlendState.AlphaBlend);
}
```

### 4. Berserk Phase Aura

For intense phase transitions:

```csharp
public void DrawBerserkAura(NPC boss, Color primaryColor, Color secondaryColor)
{
    float time = Main.GlobalTimeWrappedHourly;
    float pulse = Sin(time * 8f) * 0.3f + 0.7f;
    
    // Rotating particle ring
    for (int i = 0; i < 8; i++)
    {
        float angle = TwoPi * i / 8f + time * 3f;
        Vector2 offset = angle.ToRotationVector2() * 60f;
        
        Color ringColor = Color.Lerp(primaryColor, secondaryColor, i / 7f) * 0.5f * pulse;
        CustomParticles.GenericFlare(boss.Center + offset, ringColor, 0.4f, 15);
    }
    
    // Eye gleam effect
    Vector2 eyePos = boss.Center + GetEyeOffset(boss);
    CustomParticles.GenericFlare(eyePos, secondaryColor * pulse, 0.6f * pulse, 8);
}
```

---

## Key Takeaways

1. **Layered Bloom:** Symbol effects use 3+ additive layers for glow
2. **Prismatic vs Flame Shaders:** Different shaders for different effect types
3. **Dark Color Base:** Brimstone starts dark and brightens toward core
4. **Cyan Accents:** Small cyan highlights create visual interest
5. **Energy Convergence:** Chargeup effects use converging particle rings
6. **Berserk Indicators:** Eye gleam + rotational aura signal phase changes
7. **Radial Explosions:** Sun-like bursts use multiple angle-offset primitive draws

---

## File References

- `Content/BehaviorOverrides/BossAIs/SupremeCalamitas/SupremeCalamitasBehaviorOverride.cs`
- `Content/BehaviorOverrides/BossAIs/SupremeCalamitas/BrimstoneLaserbeam.cs`
- `Content/BehaviorOverrides/BossAIs/SupremeCalamitas/BrimstoneFlameOrb.cs`
- `Content/BehaviorOverrides/BossAIs/SupremeCalamitas/BrimstoneFlamePillar.cs`
- `Content/BehaviorOverrides/BossAIs/SupremeCalamitas/FlameOverloadBeam.cs`
- `Content/BehaviorOverrides/BossAIs/SupremeCalamitas/RitualBrimstoneHeart.cs`
- `Content/BehaviorOverrides/BossAIs/SupremeCalamitas/SuicideBomberDemonHostile.cs`
- `Content/BehaviorOverrides/BossAIs/SupremeCalamitas/DemonicExplosion.cs`
- `Content/BehaviorOverrides/BossAIs/SupremeCalamitas/SCalSymbol.cs`
