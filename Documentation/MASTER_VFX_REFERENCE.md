# MASTER VFX REFERENCE - MagnumOpus

> **THE SINGLE SOURCE OF TRUTH FOR ALL VFX IMPLEMENTATION**
> 
> This document consolidates TRUE_VFX_STANDARDS.md, Enhanced_VFX_System.md, HLSL Shader Reference, Bloom/Trail/Particle Systems, and Boss VFX patterns into ONE comprehensive reference.
> 
> **Last Updated:** 2025-01-27

---

## TABLE OF CONTENTS

1. [Core Philosophy](#core-philosophy)
2. [Cardinal Rules](#cardinal-rules)
3. [The Gold Standard: Iridescent Wingspan](#the-gold-standard)
4. [Particle Asset Catalog](#particle-assets)
5. [Theme Color Palettes](#theme-colors)
6. [VFX Implementation Patterns](#implementation-patterns)
7. [HLSL Shader Reference](#shader-reference)
8. [Bloom & Glow System](#bloom-system)
9. [Trail Rendering System](#trail-system)
10. [Particle System API](#particle-api)
11. [Boss VFX Patterns](#boss-vfx)
12. [Projectile VFX Checklist](#projectile-checklist)
13. [Melee VFX Patterns](#melee-vfx)
14. [Quick Reference Tables](#quick-reference)

---

## CORE PHILOSOPHY

> *"This mod is based around music. It's based around how it connects to your heart, and it's based around how it impacts the world."*

### The Soul of MagnumOpus
- Every weapon is a NOTE in the symphony of combat
- Every projectile should make players say *"Whoa, what was THAT?"*
- Every effect should GLOW, SHIMMER, SPARKLE with BRIGHTNESS
- Music notes must ORBIT projectiles, not spawn randomly
- Trails must CURVE and FLOW, not be rigid lines

---

## CARDINAL RULES

### Rule #1: EVERY WEAPON IS UNIQUE
If a theme has 3 swords, those 3 swords have **COMPLETELY DIFFERENT** effects:

| Sword | On-Swing | Trail | Impact | Special |
|-------|----------|-------|--------|---------|
| A | Spiraling orbs | Music note constellation | Harmonic shockwave | Orbs connect with beams |
| B | Burning afterimages | Ember + smoke wisps | Rising flame pillars | Charge summons phantom blade |
| C | Homing feathers | Prismatic rainbow arc | Crystalline explosion | 4th hit = gravity well |

### Rule #2: NO LAZY FLARES
❌ **FORBIDDEN:**
```csharp
CustomParticles.GenericFlare(target.Center, color, 0.5f, 15);
CustomParticles.HaloRing(target.Center, color, 0.3f, 12);
```

✅ **REQUIRED:** Layer 4+ effects with spinning, shimmer, bloom, music notes

### Rule #3: MUSIC NOTES MUST BE VISIBLE
- **Minimum scale: 0.7f** (NOT 0.25f!)
- **Must have:** Multi-layer bloom, shimmer animation, sparkle companions
- **Must:** ORBIT projectiles, not spawn randomly

### Rule #4: DENSE TRAILS
- **2+ dust particles per frame** (not 1-in-10)
- Scale **1.5f+** for visibility
- **Contrasting sparkles** every 1-in-2 frames
- **Color oscillation** using `Main.hslToRgb`

### Rule #5: USE ALL PARTICLE ASSETS
100+ custom PNGs available - use at least 3-4 per effect:
- 2+ custom particle types
- 1+ vanilla Dust type for density
- 1+ music-related particle
- Multi-layer bloom

---

## THE GOLD STANDARD

### Iridescent Wingspan Pattern

**STUDY THIS. COPY THIS. THIS IS WHAT GOOD LOOKS LIKE.**

#### PreDraw: 4+ Layered Spinning Flares
```csharp
public override bool PreDraw(ref Color lightColor)
{
    Texture2D flare1 = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare").Value;
    Texture2D flare2 = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare4").Value;
    Texture2D softGlow = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow2").Value;
    
    float time = Main.GameUpdateCount * 0.05f;
    float pulse = 1f + (float)Math.Sin(time * 2f) * 0.15f;
    
    Main.spriteBatch.End();
    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, ...);
    
    // Layer 1: Soft glow base
    Main.spriteBatch.Draw(softGlow, drawPos, null, themeColor * 0.3f, 0f, origin, 0.8f * pulse, ...);
    // Layer 2: Spinning clockwise
    Main.spriteBatch.Draw(flare1, drawPos, null, themeColor * 0.6f, time, origin, 0.5f * pulse, ...);
    // Layer 3: Spinning counter-clockwise
    Main.spriteBatch.Draw(flare2, drawPos, null, secondaryColor * 0.5f, -time * 0.7f, origin, 0.4f * pulse, ...);
    // Layer 4: White core
    Main.spriteBatch.Draw(flare1, drawPos, null, Color.White * 0.8f, 0f, origin, 0.2f, ...);
    
    Main.spriteBatch.End();
    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, ...);
    return false;
}
```

#### AI: Dense Trails + Orbiting Notes
```csharp
public override void AI()
{
    // DENSE DUST - 2+ every frame!
    for (int i = 0; i < 2; i++)
    {
        Dust d = Dust.NewDustPerfect(Projectile.Center, dustType, vel, 100, color, 1.8f);
        d.noGravity = true;
        d.fadeIn = 1.4f;
    }
    
    // CONTRASTING SPARKLES - 1-in-2
    if (Main.rand.NextBool(2))
    {
        Dust contrast = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch, vel, 0, Color.White, 1.0f);
        contrast.noGravity = true;
    }
    
    // FLARES - 1-in-2
    if (Main.rand.NextBool(2))
        CustomParticles.GenericFlare(Projectile.Center + offset, themeColor, 0.5f, 18);
    
    // COLOR OSCILLATION
    if (Main.rand.NextBool(3))
    {
        float hue = (Main.GameUpdateCount * 0.02f) % 1f;
        hue = themeHueMin + (hue * (themeHueMax - themeHueMin));
        Color shifted = Main.hslToRgb(hue, 0.9f, 0.75f);
        CustomParticles.GenericFlare(Projectile.Center, shifted, 0.35f, 12);
    }
    
    // ORBITING MUSIC NOTES
    float orbitAngle = Main.GameUpdateCount * 0.08f;
    if (Main.rand.NextBool(8))
    {
        for (int i = 0; i < 3; i++)
        {
            float noteAngle = orbitAngle + MathHelper.TwoPi * i / 3f;
            Vector2 notePos = Projectile.Center + noteAngle.ToRotationVector2() * 15f;
            ThemedParticles.MusicNote(notePos, Projectile.velocity * 0.8f, themeColor, 0.75f, 30);
        }
    }
}
```

#### OnKill: Glimmer Cascade
```csharp
public override void OnKill(int timeLeft)
{
    // CENTRAL GLIMMER - Multiple spinning flares
    for (int layer = 0; layer < 4; layer++)
    {
        float scale = 0.3f + layer * 0.15f;
        float alpha = 0.8f - layer * 0.15f;
        Color color = Color.Lerp(Color.White, themeColor, layer / 4f);
        CustomParticles.GenericFlare(Projectile.Center, color * alpha, scale, 18 - layer * 2);
    }
    
    // EXPANDING GLOW RINGS
    for (int ring = 0; ring < 3; ring++)
    {
        Color ringColor = Color.Lerp(themeColor, secondaryColor, ring / 3f);
        CustomParticles.HaloRing(Projectile.Center, ringColor, 0.3f + ring * 0.12f, 12 + ring * 3);
    }
    
    // RADIAL SPARKLE BURST
    for (int i = 0; i < 12; i++)
    {
        float angle = MathHelper.TwoPi * i / 12f;
        Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
        var sparkle = new SparkleParticle(Projectile.Center, vel, Color.Lerp(themeColor, Color.White, i / 12f), 0.4f, 25);
        MagnumParticleHandler.SpawnParticle(sparkle);
    }
    
    // MUSIC NOTE FINALE
    ThemedParticles.MusicNoteBurst(Projectile.Center, themeColor, 6, 4f);
    
    // BRIGHT LIGHTING
    Lighting.AddLight(Projectile.Center, themeColor.ToVector3() * 1.5f);
}
```

---

## PARTICLE ASSETS

### Complete Catalog (Assets/Particles/)

| Category | Files | Variants | Scale Range | Use For |
|----------|-------|----------|-------------|---------|
| **MusicNote** | MusicNote.png, CursiveMusicNote.png, MusicNoteWithSlashes.png, QuarterNote.png, TallMusicNote.png, WholeNote.png | 6 | **0.7f-1.2f** | EVERY trail, impact, aura |
| **EnergyFlare** | EnergyFlare.png, EnergyFlare4.png | 2 | 0.4f-1.2f | Projectile cores, layer 4+! |
| **SoftGlow** | SoftGlow2-4.png | 3 | 0.3f-0.8f | Bloom bases, layer under flares |
| **GlowingHalo** | GlowingHalo1.png, GlowingHalo2.png, GlowingHalo4-6.png | 5 | 0.3f-1.0f | Shockwaves, rings, impacts |
| **StarBurst** | StarBurst1-2.png | 2 | 0.5f-1.5f | Radial explosions |
| **MagicSparklField** | MagicSparklField4.png, MagicSparklField6-12.png | 8 | 0.3f-0.8f | Magic trails, enchantments |
| **PrismaticSparkle** | PrismaticSparkle11.png, PrismaticSparkle13.png, PrismaticSparkle14.png | 3 | 0.3f-0.7f | Rainbow sparkles |
| **ParticleTrail** | ParticleTrail1-4.png | 4 | 0.3f-0.8f | Movement trails |
| **SwordArc** | SwordArc1.png, SwordArc2.png, SwordArc3.png, SwordArc6.png, SwordArc8.png + 4 named variants | 9 | 0.5f-1.5f | **Melee swings - USE THESE!** |
| **SwanFeather** | SwanFeather1-10.png | 10 | 0.4f-1.0f | Swan Lake theme |
| **EnigmaEye** | EnigmaEye1.png + 7 named variants | 8 | 0.4f-0.8f | Enigma watching effects |
| **Glyphs** | Glyphs1-12.png | 12 | 0.3f-0.7f | Magic circles, Fate theme |
| **ShatteredStarlight** | ShatteredStarlight.png | 1 | 0.4f-1.0f | Shatter effects |

### Vanilla Dust Types (Combine for Density)
```csharp
DustID.MagicMirror       // scale 1.5f+ - magical shimmer
DustID.Enchanted_Gold    // scale 1.4f+ - golden sparkles
DustID.Enchanted_Pink    // scale 1.4f+ - pink magical
DustID.PurpleTorch       // scale 1.2f+ - purple flames
DustID.Electric          // scale 1.0f+ - electric sparks
DustID.WhiteTorch        // scale 1.0f+ - contrast sparkles
DustID.GemAmethyst       // scale 0.8f+ - purple gems
DustID.Pixie             // scale 1.0f+ - fairy dust
```

---

## THEME COLORS

### Gradient Palettes (Primary → Secondary → Accent)

| Theme | Primary | Secondary | Accent | Hue Range |
|-------|---------|-----------|--------|-----------|
| **La Campanella** | Black (20,15,20) | Orange (255,100,0) | Gold (255,200,50) | 0.08-0.12 |
| **Eroica** | Scarlet (139,0,0) | Crimson (220,50,50) | Gold (255,215,0) | 0.95-1.0 |
| **Swan Lake** | White (255,255,255) | Black (20,20,30) | Rainbow | 0.0-1.0 |
| **Moonlight Sonata** | Dark Purple (75,0,130) | Light Blue (135,206,250) | Silver (220,220,235) | 0.72-0.80 |
| **Enigma** | Black (15,10,20) | Purple (140,60,200) | Green (50,220,100) | 0.28-0.35 |
| **Fate** | Black (15,5,20) | Pink (180,50,100) | Red (255,60,80) | 0.90-0.98 |

### Gradient Usage
```csharp
// Gradient color lerping
float progress = (float)i / count;
Color gradientColor = Color.Lerp(primaryColor, secondaryColor, progress);

// Hue oscillation within theme range
float hue = (Main.GameUpdateCount * 0.02f) % 1f;
hue = themeHueMin + hue * (themeHueMax - themeHueMin);
Color shifted = Main.hslToRgb(hue, 0.9f, 0.75f);
```

---

## IMPLEMENTATION PATTERNS

### Pattern 1: Additive Bloom Stack (FargosSoulsDLC)
```csharp
// CRITICAL: Remove alpha for proper additive blending
Color bloomColor = baseColor.WithoutAlpha(); // or: baseColor with { A = 0 }

// 4-layer bloom stack
float[] scales = { 2.0f, 1.4f, 0.9f, 0.4f };
float[] opacities = { 0.3f, 0.5f, 0.7f, 0.85f };

for (int i = 0; i < 4; i++)
{
    spriteBatch.Draw(bloom, pos, null, bloomColor * opacities[i], 
        0, origin, scale * scales[i], SpriteEffects.None, 0f);
}
```

### Pattern 2: Pulsing Animation
```csharp
float pulse = 1f + MathF.Sin(Main.GameUpdateCount * 0.1f) * 0.15f;
float scale = baseScale * pulse;
```

### Pattern 3: Orbiting Elements
```csharp
float orbitAngle = Main.GameUpdateCount * 0.05f;
for (int i = 0; i < 3; i++)
{
    float angle = orbitAngle + MathHelper.TwoPi * i / 3f;
    Vector2 offset = angle.ToRotationVector2() * radius;
    // Draw at position + offset
}
```

### Pattern 4: Curved Trail (Ark of the Cosmos Style)
```csharp
private Vector2[] positionHistory = new Vector2[15];

public override void AI()
{
    // Sine-wave movement
    float waveOffset = MathF.Sin(Projectile.timeLeft * 0.15f) * 3f;
    Vector2 perpendicular = Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2);
    Projectile.Center += perpendicular * waveOffset * 0.1f;
    
    // Store position history for curved trail
    // Shift array, add current position
}
```

---

## SHADER REFERENCE

### File Locations
- **Shader Files:** `Assets/Shaders/*.fx`
- **Shader Loader:** `Common/Systems/Shaders/ShaderLoader.cs`
- **Shader Renderer:** `Common/Systems/Shaders/ShaderRenderer.cs`

### Core HLSL Functions

#### QuadraticBump (THE Universal Edge Fade)
```hlsl
float QuadraticBump(float x)
{
    return x * (4.0 - x * 4.0);
}
// Input 0.0 → 0.0, Input 0.5 → 1.0 (peak), Input 1.0 → 0.0
```

#### InverseLerp
```hlsl
float InverseLerp(float from, float to, float x)
{
    return saturate((x - from) / (to - from));
}
```

#### Convert01To010
```hlsl
float Convert01To010(float x)
{
    return x < 0.5 ? x * 2.0 : 2.0 - x * 2.0;
}
```

### Vertex Shader Structure
```hlsl
struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};
```

### Shader Application Pattern
```csharp
// Using custom shader
Effect shader = ShaderLoader.GetShader("TrailBloom");
shader.Parameters["uColor"]?.SetValue(themeColor.ToVector4());
shader.Parameters["uIntensity"]?.SetValue(1.5f);

spriteBatch.End();
spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, 
    SamplerState.LinearClamp, null, null, shader, Main.GameViewMatrix.TransformationMatrix);
// Draw with shader
spriteBatch.End();
spriteBatch.Begin(/* restore normal */);
```

---

## BLOOM SYSTEM

### File: `Common/Systems/VFX/BloomRenderer.cs`

### Key Methods

```csharp
// Standard 4-layer bloom
BloomRenderer.DrawBloomStack(spriteBatch, worldPosition, primaryColor, scale, opacity);

// Two-color gradient bloom
BloomRenderer.DrawBloomStack(spriteBatch, worldPosition, outerColor, innerColor, scale, opacity);

// Pulsing bloom
BloomRenderer.DrawPulsingBloom(spriteBatch, worldPosition, color, baseScale, pulseSpeed, pulseIntensity);

// Charge-up bloom (0-1 progress)
BloomRenderer.DrawChargeBloom(spriteBatch, worldPosition, color, chargeProgress, maxScale);

// Impact bloom (0-1 progress)
BloomRenderer.DrawImpactBloom(spriteBatch, worldPosition, color, progress, baseScale);

// Shine flare (4-point star)
BloomRenderer.DrawShineFlare(spriteBatch, worldPosition, color, scale, rotation, opacity);
```

### Bloom Layer Configuration
```csharp
// Outer to inner layers
float[] scales = { 2.0f, 1.4f, 0.9f, 0.4f };      // Decreasing size
float[] opacities = { 0.3f, 0.5f, 0.7f, 0.85f };  // Increasing brightness
```

---

## TRAIL SYSTEM

### File: `Common/Systems/VFX/EnhancedTrailRenderer.cs`

### Width Function Presets
```csharp
// Linear taper
EnhancedTrailRenderer.LinearTaper(startWidth)

// Thick in middle (projectile trails)
EnhancedTrailRenderer.QuadraticBumpWidth(maxWidth)

// FargosSoulsDLC style
EnhancedTrailRenderer.InverseLerpBumpWidth(minWidth, maxWidth, rampUpEnd, rampDownStart)

// Laser beams
EnhancedTrailRenderer.ConstantWithFade(width, fadeStart)
```

### Color Function Presets
```csharp
// Solid with fade
EnhancedTrailRenderer.SolidColorFade(color, opacity)

// Gradient along trail
EnhancedTrailRenderer.GradientColor(startColor, endColor, opacity)

// Theme palette
EnhancedTrailRenderer.PaletteColor(colorArray, opacity)

// Bright in middle
EnhancedTrailRenderer.BumpOpacityColor(color, rampUpStart, rampUpEnd, rampDownStart, rampDownEnd)
```

### Multi-Pass Trail Rendering
```csharp
// Pass 1: Outer bloom
RenderTrailWithSettings(positions, new PrimitiveSettings(
    BloomWidth(baseWidth, 2.7f),
    BloomColor(baseColor, 0.3f)
));

// Pass 2: Main trail
RenderTrailWithSettings(positions, new PrimitiveSettings(
    baseWidth,
    baseColor
));

// Pass 3: Bright core
RenderTrailWithSettings(positions, new PrimitiveSettings(
    c => baseWidth(c) * 0.4f,
    c => Color.White * baseColor(c).A / 255f
));
```

---

## PARTICLE API

### Spawning Particles
```csharp
using MagnumOpus.Common.Systems.Particles;

// Custom particles
CustomParticles.GenericFlare(position, color, scale, lifetime);
CustomParticles.HaloRing(position, color, scale, lifetime);
CustomParticles.PrismaticSparkle(position, color, scale, variant);

// Theme particles
ThemedParticles.[Theme]Impact(position, scale);
ThemedParticles.[Theme]Trail(position, velocity, scale);
ThemedParticles.MusicNote(position, velocity, color, scale, lifetime, variant);
ThemedParticles.MusicNoteBurst(position, color, count, speed);

// Enhanced particles with bloom
EnhancedThemedParticles.[Theme]ImpactEnhanced(position, intensity);
EnhancedThemedParticles.[Theme]BloomBurstEnhanced(position, intensity);

// Unified VFX API
UnifiedVFXBloom.[Theme].ImpactEnhanced(position, scale);
UnifiedVFXBloom.[Theme].TrailEnhanced(position, velocity, scale);
```

### Particle Classes
```csharp
// Generic glow particle
var glow = new GenericGlowParticle(position, velocity, color, scale, lifetime, noGravity);
MagnumParticleHandler.SpawnParticle(glow);

// Sparkle particle
var sparkle = new SparkleParticle(position, velocity, color, scale, lifetime);
MagnumParticleHandler.SpawnParticle(sparkle);

// Bloom particle (4-layer)
var bloom = new BloomParticle(position, velocity, color, scale, lifetime);
MagnumParticleHandler.SpawnParticle(bloom);
```

---

## BOSS VFX

### Warning Indicators
```csharp
// Warning types
WarningType.Safe     // Cyan - "Stand here"
WarningType.Caution  // Yellow - "Danger soon"
WarningType.Danger   // Red - "Projectiles incoming"
WarningType.Imminent // White - "Attack NOW"

// Warning methods
BossVFXOptimizer.WarningLine(start, direction, length, markers, type);
BossVFXOptimizer.SafeZoneRing(center, radius, markers);
BossVFXOptimizer.DangerZoneRing(center, radius, markers);
BossVFXOptimizer.ConvergingWarning(center, radius, progress, color, count);
BossVFXOptimizer.SafeArcIndicator(center, safeAngle, arcWidth, radius, markers);
```

### Attack Pattern VFX
```csharp
// Telegraph phase
if (SubPhase == 0)
{
    float progress = Timer / (float)chargeTime;
    BossVFXOptimizer.ConvergingWarning(NPC.Center, 150f, progress, themeColor, 8);
    BossVFXOptimizer.SafeZoneRing(target.Center, 100f, 12);
}

// Execute phase
if (SubPhase == 1 && Timer == 1)
{
    BossVFXOptimizer.AttackReleaseBurst(NPC.Center, primaryColor, secondaryColor, 1.2f);
}
```

---

## PROJECTILE CHECKLIST

Before considering a projectile complete, verify:

- [ ] PreDraw has **4+ layered flares** spinning at different speeds
- [ ] Trail has **dense dust** (2+ per frame, scale 1.5f+)
- [ ] Trail has **contrasting sparkles** (1-in-2)
- [ ] Trail has **flares** littering the air (1-in-2)
- [ ] Colors **oscillate** with `Main.hslToRgb`
- [ ] Music notes **orbit** projectile (scale 0.7f+)
- [ ] Impact is **glimmer cascade** (not puff)
- [ ] Lighting is **bright** (1.0f+ intensity)
- [ ] Uses **3+ particle types** combined
- [ ] Has **unique visual identity** vs other weapons

---

## MELEE VFX

### Use SwordArc Textures!
```csharp
public override void MeleeEffects(Player player, Rectangle hitbox)
{
    // Load SwordArc texture
    Texture2D arc = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SwordArc2").Value;
    
    // Draw layered arcs in PreDraw with additive blending
    // Layer 1: Main arc
    // Layer 2: Glow layer (larger, dimmer)
    // Layer 3: Trailing afterimage
    
    // Dense dust trail
    for (int i = 0; i < 3; i++)
    {
        Dust d = Dust.NewDustPerfect(hitbox.Center.ToVector2(), dustType, vel, 0, themeColor, 1.5f);
        d.noGravity = true;
    }
    
    // Music notes in swing (1-in-5)
    if (Main.rand.NextBool(5))
    {
        ThemedParticles.MusicNote(hitbox.Center.ToVector2(), swingDir, themeColor, 0.8f, 35);
    }
}
```

### Wave Projectile (NOT PNG Copy-Paste!)
```csharp
public override bool PreDraw(ref Color lightColor)
{
    Texture2D arc = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SwordArc2").Value;
    Texture2D glow = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow3").Value;
    
    Main.spriteBatch.End();
    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, ...);
    
    // Glow base
    Main.spriteBatch.Draw(glow, drawPos, null, themeColor * 0.4f, rotation, origin, 1.2f, ...);
    // Main arc
    Main.spriteBatch.Draw(arc, drawPos, null, themeColor * 0.9f, rotation, origin, 1f, ...);
    // Bright edge
    Main.spriteBatch.Draw(arc, drawPos, null, Color.White * 0.6f, rotation, origin, 0.8f, ...);
    
    Main.spriteBatch.End();
    Main.spriteBatch.Begin(/* restore */);
    return false;
}
```

---

## QUICK REFERENCE

### Scale Guidelines
| Element | Min Scale | Ideal Scale | Max Scale |
|---------|-----------|-------------|-----------|
| Music Notes | **0.7f** | 0.85f | 1.2f |
| Trail Dust | 1.2f | 1.5f | 2.0f |
| Flares | 0.3f | 0.5f | 0.8f |
| Sparkles | 0.25f | 0.4f | 0.6f |
| Halos | 0.3f | 0.5f | 1.0f |
| SwordArcs | 0.5f | 1.0f | 1.5f |

### Frequency Guidelines
| Effect | Frequency |
|--------|-----------|
| Trail Dust | Every frame, 2+ particles |
| Contrasting Sparkles | 1-in-2 |
| Flares | 1-in-2 |
| Color Oscillation | 1-in-3 |
| Music Notes | 1-in-6 to 1-in-8 |

### Opacity Guidelines (Additive Blending)
| Layer | Opacity |
|-------|---------|
| Outer bloom | 0.3f |
| Middle bloom | 0.5f |
| Inner bloom | 0.7f |
| Core | 0.85f |

### File Locations
| System | Location |
|--------|----------|
| VFX Utilities | `Common/Systems/VFX/VFXUtilities.cs` |
| Bloom Renderer | `Common/Systems/VFX/BloomRenderer.cs` |
| Trail Renderer | `Common/Systems/VFX/EnhancedTrailRenderer.cs` |
| Theme Palettes | `Common/Systems/VFX/MagnumThemePalettes.cs` |
| Particle Handler | `Common/Systems/Particles/MagnumParticleHandler.cs` |
| Themed Particles | `Common/Systems/ThemedParticles.cs` |
| Unified VFX | `Common/Systems/UnifiedVFX.cs` |
| Boss VFX | `Common/Systems/BossVFXOptimizer.cs` |
| Shader Loader | `Common/Systems/Shaders/ShaderLoader.cs` |
| Shader Renderer | `Common/Systems/Shaders/ShaderRenderer.cs` |
| Afterimage Trail | `Common/Systems/VFX/AfterimageTrail.cs` |
| Shaders | `Assets/Shaders/*.fx` |
| Particle Textures | `Assets/Particles/*.png` |

---

## CUSTOM SHADER SYSTEM

### Overview
MagnumOpus includes custom HLSL shaders for Calamity-style VFX:

| Shader File | Passes | Use For |
|-------------|--------|---------|
| `TrailShader.fx` | TrailPass, BloomPass | Primitive trails with gradient + glow |
| `BloomShader.fx` | BloomPass, GradientBloomPass, FlarePass | Multi-layer bloom, 4-point flares |
| `ScreenEffectsShader.fx` | 7 effects | Chromatic aberration, radial blur, reality crack |

### Loading Shaders
```csharp
using MagnumOpus.Common.Systems.Shaders;

// Get shader directly
Effect trailShader = ShaderLoader.Trail;
Effect bloomShader = ShaderLoader.Bloom;
Effect screenShader = ShaderLoader.Screen;

// Or by name
Effect shader = ShaderLoader.GetShader("TrailShader");
```

### Using ShaderRenderer
```csharp
using MagnumOpus.Common.Systems.Shaders;

// Disposable pattern (recommended)
using (ShaderRenderer.BeginShaderScope(spriteBatch, ShaderRenderer.ShaderType.Bloom, color, intensity))
{
    spriteBatch.Draw(texture, position, null, Color.White, ...);
}

// Direct drawing methods
ShaderRenderer.DrawWithBloom(spriteBatch, texture, worldPos, color, scale, intensity);
ShaderRenderer.DrawWithGradientBloom(spriteBatch, texture, worldPos, innerColor, outerColor, scale);
ShaderRenderer.DrawBloomStack(spriteBatch, texture, worldPos, color, baseScale, intensity);
ShaderRenderer.DrawFlare(spriteBatch, texture, worldPos, color, scale, rotation, intensity);

// Screen effects (call before drawing)
ShaderRenderer.ApplyChromaticAberration(spriteBatch, worldPosition, intensity);
ShaderRenderer.ApplyRadialBlur(spriteBatch, worldPosition, intensity);
ShaderRenderer.ApplyRealityCrack(spriteBatch, worldPosition, color, intensity);
```

### Shader Types
| ShaderType | Effect | BlendState |
|------------|--------|------------|
| `Bloom` | Standard radial bloom | Additive |
| `GradientBloom` | Two-color gradient bloom | Additive |
| `Flare` | 4-point star flare | Additive |
| `Trail` | Trail gradient rendering | Additive |
| `TrailBloom` | Trail with outer glow | Additive |
| `ChromaticAberration` | RGB channel separation | AlphaBlend |
| `RadialBlur` | Directional blur from point | AlphaBlend |
| `RealityCrack` | Fate theme reality distortion | AlphaBlend |

---

## AFTERIMAGE TRAIL SYSTEM

### Overview
Calamity-style afterimage trails store position history and draw with decreasing transparency.

### Basic Usage
```csharp
using MagnumOpus.Common.Systems.VFX;

private AfterimageTrail trail;

public override void AI()
{
    trail ??= new AfterimageTrail(12); // 12 afterimages
    trail.Update(Projectile.Center, Projectile.rotation);
}

public override bool PreDraw(ref Color lightColor)
{
    Texture2D texture = TextureAssets.Projectile[Type].Value;
    Vector2 origin = texture.Size() * 0.5f;
    
    // Standard draw (decreasing alpha)
    trail?.Draw(Main.spriteBatch, texture, origin, lightColor, Projectile.scale);
    
    return true; // Draw normal sprite on top
}
```

### Drawing Methods
```csharp
// Standard decreasing alpha
trail.Draw(spriteBatch, texture, origin, color, scale, alphaMultiplier);

// Two-color gradient (old→new)
trail.DrawGradient(spriteBatch, texture, origin, oldColor, newColor, scale, alphaMultiplier);

// Additive blending for glow (handles SpriteBatch state)
trail.DrawAdditive(spriteBatch, texture, origin, color, scale, alphaMultiplier);

// With custom shader
trail.DrawWithShader(spriteBatch, texture, origin, color, scale, ShaderRenderer.ShaderType.Bloom, intensity);

// Custom drawing action per afterimage
trail.DrawCustom(spriteBatch, (batch, position, rotation, progress, index) => {
    // Custom draw logic here
    // progress: 0=oldest, 1=newest
});
```

### Enhanced Afterimage Trail
For trails with velocity-based stretch effects:

```csharp
private EnhancedAfterimageTrail enhancedTrail;

public override void AI()
{
    enhancedTrail ??= new EnhancedAfterimageTrail(15);
    enhancedTrail.Update(Projectile.Center, Projectile.rotation, Projectile.scale, Projectile.velocity);
}

public override bool PreDraw(ref Color lightColor)
{
    // Velocity-based stretch effect
    enhancedTrail?.DrawStretched(Main.spriteBatch, texture, origin, color, 0.5f, 1f);
    return true;
}
```

### Trail Parameters
| Parameter | Recommended | Description |
|-----------|-------------|-------------|
| `length` | 8-20 | Number of afterimages stored |
| `everyNFrames` | 1-2 | Record frequency (1=every frame) |
| `alphaMultiplier` | 0.6-1.0 | Overall transparency |
| `stretchFactor` | 0.3-0.8 | Velocity stretch intensity |

---

## MULTI-PASS TRAIL RENDERING

### Using EnhancedTrailRenderer with Shaders
```csharp
using MagnumOpus.Common.Systems.VFX;

// Multi-pass rendering with bloom
EnhancedTrailRenderer.RenderMultiPassTrail(
    positions,           // Vector2[] of trail points
    EnhancedTrailRenderer.LinearTaper(20f),  // Width function
    primaryColor,        // Main color
    secondaryColor,      // Gradient end color
    baseWidth: 20f,
    bloomIntensity: 1f
);

// Custom shader trail
var settings = new EnhancedTrailRenderer.PrimitiveSettings(
    EnhancedTrailRenderer.QuadraticBumpWidth(15f),  // Thick in middle
    EnhancedTrailRenderer.GradientColor(color1, color2),
    null,  // No offset
    true,  // Smoothen positions
    null   // No vanilla shader
);

EnhancedTrailRenderer.RenderTrailWithShader(
    positions,
    settings,
    primaryColor,
    secondaryColor,
    intensity: 1.2f
);
```

### Multi-Pass Structure
| Pass | Width | Opacity | Purpose |
|------|-------|---------|---------|
| 1 (Bloom) | 2.0x | 30% | Outer glow halo |
| 2 (Main) | 1.0x | 100% | Full color trail |
| 3 (Core) | 0.4x | 80% white | Bright center highlight |

---

## IMPLEMENTATION PRIORITY

1. **Projectiles First:** Apply gold standard pattern to all projectiles
2. **Melee Second:** Use SwordArc textures for all melee weapons
3. **Impacts Third:** Convert all impacts to glimmer cascades
4. **Trails Fourth:** Dense trails with orbiting music notes
5. **Bosses Fifth:** Full telegraph → execute → recovery pattern

---

*This document supersedes all previous VFX documentation. When in doubt, refer to the Iridescent Wingspan pattern.*
