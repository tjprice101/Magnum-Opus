# GALAXIA (Four Seasons Galaxia) - Complete Technical Deep Dive

> **Source:** Calamity Mod Public Repository
> **Branch:** 1.4.4
> **Analysis Date:** Comprehensive breakdown of the "flaming smoke" VFX system

---

## 📋 TABLE OF CONTENTS

1. [Overview & Architecture](#overview--architecture)
2. [Key Files Reference](#key-files-reference)
3. [The Attunement System](#the-attunement-system)
4. [The Flaming Smoke VFX System (Core Pattern)](#the-flaming-smoke-vfx-system-core-pattern)
5. [CircularSmearSmokeyVFX - The Nebula Arc](#circularsmearsmokeyevfx---the-nebula-arc)
6. [HeavySmokeParticle - Dual Layer System](#heavysmokeparticle---dual-layer-system)
7. [ImpFlameTrail Shader](#impflametrail-shader)
8. [Phoenix Attunement (Fire/Throw Mode)](#phoenix-attunement-firethrow-mode)
9. [Polaris Attunement (Ice/Shred Mode)](#polaris-attunement-iceshred-mode)
10. [Aries Attunement (Constellation Mode)](#aries-attunement-constellation-mode)
11. [Andromeda Attunement (Dash Mode)](#andromeda-attunement-dash-mode)
12. [Constellation Drawing System](#constellation-drawing-system)
13. [GalaxiaBolt Projectile](#galaxiabolt-projectile)
14. [Color Palettes & Gradient System](#color-palettes--gradient-system)
15. [Implementation Checklist for MagnumOpus](#implementation-checklist-for-magnumopus)

---

## Overview & Architecture

The **Four Seasons Galaxia** is Calamity's ultimate endgame broadsword with **4 distinct attunement modes**, each with unique attack patterns and visual effects. The defining visual characteristic is the **"flaming smoke" nebula effect** - a smoky, fire-like mesh that appears behind the sword during swings.

### The Flaming Smoke Effect - How It Works

The effect you see in the screenshot consists of **three layered systems**:

| Layer | Component | Purpose |
|-------|-----------|---------|
| **1. Smear Arc** | `CircularSmearSmokeyVFX` | Creates the curved nebula arc behind swings |
| **2. Base Smoke** | `HeavySmokeParticle` (non-glowing) | Volumetric smoke clouds |
| **3. Glowing Overlay** | `HeavySmokeParticle` (glowing=true) | Luminous flame-like overlay with additive blend |

### Visual Effect Breakdown

| Visual Element | Implementation |
|----------------|----------------|
| **Purple/indigo nebula arc** | `CircularSmearSmokeyVFX` with `CalamityMod/Particles/CircularSmearSmokey` texture |
| **Flame-like smoke clouds** | Dual-layer `HeavySmokeParticle` (base + glow overlay) |
| **Color shifting/animation** | `hueshift` parameter on HeavySmokeParticle |
| **Constellation star lines** | `BloomLineVFX` + `GenericSparkle` |
| **Blade swing trail** | Smear persistence pattern (reset `Time = 0` each frame) |

### Base Weapon Stats

```csharp
public class FourSeasonsGalaxia : ModItem
{
    public Attunement mainAttunement = null;
    
    // Base damage multipliers per attunement
    public static int BaseDamage = 250;
    public static int PhoenixAttunement_BaseDamage = 300;
    public static int PolarisAttunement_BaseDamage = 400;
    public static int AndromedaAttunement_BaseDamage = 1120;  // Highest!
    public static int AriesAttunement_BaseDamage = 375;
    
    // Phoenix specifics
    public static float PhoenixAttunement_BoltDamageReduction = 0.5f;
    public static float PhoenixAttunement_FullChargeDamageBoost = 2.1f;
    
    // Andromeda specifics
    public static float AndromedaAttunement_FullChargeMult = 3.5f;
    
    // Polaris specifics
    public static float PolarisAttunement_ShredDamageMultiplier = 0.50f;
    
    public override void SetDefaults()
    {
        Item.width = Item.height = 130;
        Item.damage = BaseDamage;
        Item.DamageType = TrueMeleeDamageClass.Instance;
        Item.useAnimation = Item.useTime = 16;
        Item.useTurn = true;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.knockBack = 9.5f;
        Item.UseSound = null;
        Item.autoReuse = true;
        Item.noUseGraphic = true;
        Item.noMelee = true;
        Item.channel = true;
        Item.shoot = ModContent.ProjectileType<PhoenixsPride>(); // Default mode
        Item.shootSpeed = 28f;
        Item.value = CalamityGlobalItem.RarityVioletBuyPrice;
        Item.rare = ModContent.RarityType<Violet>();
    }
}
```

---

## Key Files Reference

### Main Weapon & Attunement System

| File | Purpose |
|------|---------|
| `Items/Weapons/Melee/FourSeasonsGalaxia.cs` | Main weapon item, damage stats, attunement handling |
| `Items/Weapons/Melee/Attunement.cs` | Attunement base class + all 4 Galaxia attunement definitions |
| `Items/Weapons/Melee/AttunementSystem.cs` | Registry and initialization of attunements |
| `Projectiles/Melee/GalaxiaHoldout.cs` | Visual holdout projectile, constellation display |

### Projectile Files (Per Attunement)

| File | Attunement | Purpose |
|------|------------|---------|
| `Projectiles/Melee/Galaxia_PhoenixsPride.cs` | Phoenix | Main fire/throw projectile |
| `Projectiles/Melee/Galaxia_PhoenixsPrideFirewall.cs` | Phoenix | Fire pillar on charge release |
| `Projectiles/Melee/Galaxia_PolarisGaze.cs` | Polaris | Ice shred projectile |
| `Projectiles/Melee/Galaxia_AriesWrath.cs` | Aries | Constellation combo projectile |
| `Projectiles/Melee/Galaxia_AriesWrathConstellation.cs` | Aries | Constellation line effect |
| `Projectiles/Melee/Galaxia_AndromedasStride.cs` | Andromeda | Dash attack projectile |
| `Projectiles/Melee/Galaxia_AndromedasStrideBoltSpawner.cs` | Andromeda | Hit VFX spawner |
| `Projectiles/Melee/GalaxiaBolt.cs` | Shared | Cosmic bolt projectile (used by multiple modes) |

### VFX/Particle Systems

| File | Purpose |
|------|---------|
| `Particles/CircularSmearSmokeyVFX.cs` | **THE KEY FILE** - Nebula arc with smoky texture |
| `Particles/CircularSmearVFX.cs` | Base circular smear (no smoke) |
| `Particles/SemiCircularSmearVFX.cs` | Half-circle smear with squish |
| `Particles/TrientCircularSmearVFX.cs` | Third-circle smear |
| `Particles/HeavySmokeParticle.cs` | **Dense smoke with glow mode** |
| `Particles/MediumMistParticle.cs` | Lighter mist particle |
| `Particles/GenericSparkle.cs` | Star sparkle with bloom |
| `Particles/BloomLineVFX.cs` | Glowing line for constellations |
| `Particles/ConstellationRingVFX.cs` | Ring of constellation stars |

### Shaders

| File | Purpose |
|------|---------|
| `Effects/ImpFlameTrail.fx` | **Flame trail shader** - Scroll noise for fire effect |
| `Effects/TrailStreak.fx` | Basic trail rendering |
| `Effects/Bordernado.fx` | Used for fire pillar primitives |

---

## The Attunement System

Galaxia has **4 attunement modes** that cycle in order: Phoenix → Aries → Polaris → Andromeda

### AttunementID Enum

```csharp
public enum AttunementID
{
    // Non-Galaxia attunements (other weapons)
    Default,
    Healing,
    Hot,
    Cold,
    Supercritical,
    Whirlwind,
    Shockwave,
    Flawless,
    // ... more ...
    
    // Galaxia attunements
    Phoenix,    // Fire throw mode
    Aries,      // Constellation combo mode
    Polaris,    // Ice shred mode
    Andromeda   // Dash mode (highest damage)
}
```

### Attunement Class Definitions

```csharp
// PHOENIX - Fire/Throw Mode
public class PhoenixAttunement : Attunement
{
    public PhoenixAttunement()
    {
        id = AttunementID.Phoenix;
        tooltipColor = new Color(255, 87, 0);   // Orange
        tooltipColor2 = new Color(255, 143, 0); // Lighter orange
    }
    
    public override void ApplyStats(Item item)
    {
        item.channel = true;
        item.noUseGraphic = true;
        item.shoot = ModContent.ProjectileType<PhoenixsPride>();
    }
}

// ARIES - Constellation Combo Mode
public class AriesAttunement : Attunement
{
    public AriesAttunement()
    {
        id = AttunementID.Aries;
        tooltipColor = new Color(196, 89, 201);  // Purple
        tooltipColor2 = new Color(222, 116, 227); // Lighter purple
    }
    
    public override void ApplyStats(Item item)
    {
        item.channel = true;
        item.noUseGraphic = true;
        item.shoot = ModContent.ProjectileType<AriesWrath>();
    }
}

// POLARIS - Ice/Shred Mode
public class PolarisAttunement : Attunement
{
    public PolarisAttunement()
    {
        id = AttunementID.Polaris;
        tooltipColor = new Color(128, 189, 255);  // Ice blue
        tooltipColor2 = new Color(169, 207, 255); // Lighter blue
    }
    
    public override void ApplyStats(Item item)
    {
        item.channel = true;
        item.noUseGraphic = true;
        item.shoot = ModContent.ProjectileType<PolarisGaze>();
    }
}

// ANDROMEDA - Dash Mode (Highest Damage)
public class AndromedaAttunement : Attunement
{
    public AndromedaAttunement()
    {
        id = AttunementID.Andromeda;
        tooltipColor = new Color(132, 128, 255);  // Purple-blue
        tooltipColor2 = new Color(147, 144, 255); // Lighter
    }
    
    public override void ApplyStats(Item item)
    {
        item.channel = true;
        item.noUseGraphic = true;
        item.shoot = ModContent.ProjectileType<AndromedasStride>();
    }
}
```

---

## The Flaming Smoke VFX System (Core Pattern)

### ⭐ THE KEY INSIGHT ⭐

The "flaming smoke" effect is created through a **THREE-LAYER SYSTEM**:

```
┌─────────────────────────────────────────────────────────────┐
│  LAYER 3: HeavySmokeParticle (glowing=true, hueshift>0)    │  ← Luminous overlay
│    - Additive blending                                      │
│    - Color: Main.hslToRgb(0.85f, 1, 0.5f) or similar       │
│    - Creates the "flame" glow effect                        │
├─────────────────────────────────────────────────────────────┤
│  LAYER 2: HeavySmokeParticle (glowing=false)               │  ← Base smoke
│    - Normal blending                                        │
│    - Color: Color.Lerp(MidnightBlue, Indigo, i)            │
│    - Creates volumetric base                                │
├─────────────────────────────────────────────────────────────┤
│  LAYER 1: CircularSmearSmokeyVFX                           │  ← Nebula arc
│    - Additive blending (UseAdditiveBlend = true)           │
│    - Texture: CalamityMod/Particles/CircularSmearSmokey    │
│    - Creates the curved nebula behind swing                 │
└─────────────────────────────────────────────────────────────┘
```

### The Smear Persistence Pattern

**CRITICAL:** The smear particle stays alive by resetting its `Time` property to 0 every frame:

```csharp
// Store smear reference as class field
private CircularSmearSmokeyVFX smear = null;

// In AI() method:
if (smear == null)
{
    // First frame: Create and spawn the smear
    smear = new CircularSmearSmokeyVFX(
        Owner.Center,           // Position
        currentColor,           // Color
        direction.ToRotation(), // Rotation
        Projectile.scale * 1.5f // Scale
    );
    GeneralParticleHandler.SpawnParticle(smear);
}
else
{
    // Subsequent frames: UPDATE the smear (don't spawn new one!)
    smear.Rotation = direction.ToRotation() + MathHelper.PiOver2;
    smear.Time = 0;  // ⭐ CRITICAL: Reset lifetime to keep particle alive!
    smear.Position = Owner.Center;
    smear.Scale = Projectile.scale * 1.9f;
    smear.Color = currentColor;
}
```

---

## CircularSmearSmokeyVFX - The Nebula Arc

### Complete Class Definition

```csharp
public class CircularSmearSmokeyVFX : Particle
{
    // ⭐ This is the key texture that creates the nebula look
    public override string Texture => "CalamityMod/Particles/CircularSmearSmokey";
    
    // ⭐ Additive blending is CRITICAL for the glowing effect
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
        Lifetime = 2;  // Short lifetime, reset each frame to keep alive
    }

    public override void Update()
    {
        // Standard lifetime tick
        Lifetime--;
    }
}
```

### The Texture

The `CircularSmearSmokey` texture is a **pre-rendered noise-based arc texture** that looks like:
- A curved/circular arc shape
- Filled with smoky/cloudy noise pattern
- Grayscale (color is applied via the particle's Color property)
- The noise gives it the "nebula" or "flame" look when additively blended

### Related Smear Particles

```csharp
// Base smear (clean, no noise)
public class CircularSmearVFX : Particle
{
    public override string Texture => "CalamityMod/Particles/CircularSmear";
    public override bool UseAdditiveBlend => true;
    public override bool UseCustomDraw => true;
    
    public Asset<Texture2D> LoadedAsset;
    
    public CircularSmearVFX(Vector2 position, Color color, float rotation, float scale)
    {
        Position = position;
        Velocity = Vector2.Zero;
        Color = color;
        Scale = scale;
        Rotation = rotation;
        Lifetime = 2;
        LoadedAsset = ModContent.Request<Texture2D>(Texture);
    }
    
    public override void CustomDraw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(LoadedAsset.Value, Position - Main.screenPosition, null, 
            Color, Rotation, LoadedAsset.Value.Size() * 0.5f, Scale, SpriteEffects.None, 0);
    }
}

// Half-circle smear with squish support
public class SemiCircularSmearVFX : Particle
{
    public override string Texture => "CalamityMod/Particles/SemiCircularSmear";
    public override bool UseAdditiveBlend => true;
    public override bool UseCustomDraw => true;
    
    public Asset<Texture2D> LoadedAsset;
    public Vector2 Squish;  // Allows stretching/squishing
    
    public override void CustomDraw(SpriteBatch spriteBatch)
    {
        Texture2D tex = LoadedAsset.Value;
        spriteBatch.Draw(tex, Position - Main.screenPosition, null, Color, 
            Rotation, tex.Size() * 0.5f, Scale * Squish, SpriteEffects.None, 0);
    }
}
```

---

## HeavySmokeParticle - Dual Layer System

### Complete Class Definition

```csharp
public class HeavySmokeParticle : Particle
{
    // ⭐ 7 different smoke frame variants for variety
    public override string Texture => "CalamityMod/Particles/HeavySmoke";
    
    public float Spin;
    public float opacity;
    public float originalOpacity;
    public float originalScale;
    public float FadeRate;
    public bool glow;       // ⭐ KEY: When true, uses additive blending!
    public float hueShift;  // ⭐ Animates color over time
    
    // ⭐ CRITICAL: Glow mode uses additive blend
    public override bool UseAdditiveBlend => glow;

    public HeavySmokeParticle(
        Vector2 position, 
        Vector2 velocity, 
        Color color, 
        int lifetime, 
        float scale, 
        float opacity, 
        float rotationSpeed = 0f, 
        bool glowing = false,      // ← Set true for flame overlay
        float hueshift = 0f,       // ← Set > 0 for color animation
        bool NeedRandomDelay = false)
    {
        Position = position;
        Velocity = velocity;
        Color = color;
        Lifetime = lifetime;
        Scale = originalScale = scale;
        Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        Spin = rotationSpeed;
        glow = glowing;
        hueShift = hueshift;
        opacity = originalOpacity = opacity;
        
        // Random frame variant (7 options)
        Variant = Main.rand.Next(7);
        
        if (NeedRandomDelay)
            Time = -Main.rand.Next(10, 21);
        else
            Time = 0;
        
        FadeRate = 0.06f;
    }

    public override void Update()
    {
        // Standard physics
        Velocity *= 0.90f;
        Rotation += Spin * ((Velocity.X > 0f) ? 1f : -1f);
        
        // Hue shifting animation
        if (hueShift != 0)
        {
            float hue = (Main.rgbToHsl(Color).X + hueShift) % 1f;
            Color = Main.hslToRgb(hue, Main.rgbToHsl(Color).Y, Main.rgbToHsl(Color).Z);
        }
        
        // Lifetime management with delay support
        if (Time < 0)
            return;
            
        if (Time > Lifetime / 2f)
        {
            float fadeProgress = (Time - Lifetime / 2f) / (Lifetime / 2f);
            opacity = MathHelper.Lerp(originalOpacity, 0f, fadeProgress);
            Scale = MathHelper.Lerp(originalScale, originalScale * 0.5f, fadeProgress);
        }
        
        if (Time >= Lifetime)
            Kill();
    }
}
```

### The Dual-Layer Smoke Pattern

**THIS IS THE CORE OF THE FLAMING SMOKE EFFECT:**

```csharp
// Spawned along the blade during swing
for (int i = 0; i < 5; i++)
{
    float scaleFactor = 0.8f;
    Vector2 smokepos = Projectile.Center + (direction * (60 + 20 * i) * Projectile.scale);
    
    // ⭐ LAYER 1: Base smoke (non-glowing, normal blend)
    Particle smoke = new HeavySmokeParticle(
        smokepos, 
        direction.RotatedBy(-MathHelper.PiOver2) * 20f * scaleFactor + Owner.velocity, 
        Color.Lerp(Color.MidnightBlue, Color.Indigo, i),  // Purple gradient
        10 + Main.rand.Next(5),   // Lifetime
        scaleFactor * Main.rand.NextFloat(2.8f, 3.1f),  // Large scale
        Opacity + Main.rand.NextFloat(0f, 0.2f),        // Opacity
        0f,             // Spin
        false,          // ⭐ NOT glowing (base layer)
        0,              // No hue shift
        true            // Random delay
    );
    GeneralParticleHandler.SpawnParticle(smoke);
    
    // ⭐ LAYER 2: Glowing overlay (additive blend, hue shift)
    Particle smokeGlow = new HeavySmokeParticle(
        smokepos, 
        direction.RotatedBy(-MathHelper.PiOver2) * 20f * scaleFactor + Owner.velocity, 
        Main.hslToRgb(0.85f, 1, 0.5f),  // ⭐ Bright saturated color
        20,             // Longer lifetime
        scaleFactor * Main.rand.NextFloat(2.8f, 3.1f) * 0.8f,  // Slightly smaller
        Opacity + Main.rand.NextFloat(0f, 0.2f),
        0f,             // Spin
        true,           // ⭐ GLOWING (additive blend!)
        0.01f,          // ⭐ HUE SHIFT (color animation!)
        true            // Random delay
    );
    GeneralParticleHandler.SpawnParticle(smokeGlow);
}
```

### Why This Creates the "Flame" Effect

1. **Base Smoke (Non-Glowing)**
   - Uses normal alpha blending
   - Dark purple colors (MidnightBlue → Indigo)
   - Provides volumetric depth and mass

2. **Glowing Overlay (Additive)**
   - Uses additive blending → colors ADD together = brighter
   - Bright saturated colors via `Main.hslToRgb(0.85f, 1, 0.5f)`
   - `hueShift = 0.01f` causes color to animate over time → flame-like shifting
   - Appears as luminous "fire" within the smoke

---

## ImpFlameTrail Shader

### Complete Shader Code

```hlsl
sampler uImage0 : register(s0);      // Main texture
sampler uImage1 : register(s1);      // Noise texture for flame streaks
float uTime;                          // Animation time

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float4 color = input.Color;
    float2 coords = input.TextureCoordinates;
    
    // Account for texture distortion artifacts in primitives
    coords.y = (coords.y - 0.5) / input.TextureCoordinates.z + 0.5;
    
    // Sample noise texture with scrolling UV for fire animation
    // The noise scrolls LEFT (negative time) at 2.9x speed
    float flameStreakBrightness = tex2D(uImage1, float2(frac(coords.x - uTime * 2.9), coords.y)).r;
    
    // Edge fade using sine curve
    // Creates soft falloff at top/bottom edges
    // Power lerps from 3 (sharp) to 10 (very sharp) based on X position
    float bloomOpacity = lerp(
        pow(sin(coords.y * 3.141), lerp(3, 10, coords.x)), 
        0.7, 
        coords.x
    );
    
    // Final output: 
    // - Base opacity raised to 6th power (strong contrast)
    // - Multiplied by flame brightness (2x to 7x based on noise)
    return color * pow(bloomOpacity, 6.0) * lerp(2.0, 7, flameStreakBrightness);
}

technique Technique1
{
    pass TrailPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
```

### How the Shader Creates Flames

1. **Noise Scrolling**: `uTime * 2.9` scrolls a noise texture, creating animated "flame streaks"
2. **Edge Fade**: `pow(sin(coords.y * π), n)` fades edges smoothly
3. **Brightness Boost**: Final multiply by 2-7x creates the intense flame glow
4. **Used with Primitive Trails**: Applied to GPU-rendered trail meshes

---

## Phoenix Attunement (Fire/Throw Mode)

### Projectile: `Galaxia_PhoenixsPride.cs`

```csharp
public class PhoenixsPride : ModProjectile
{
    private CircularSmearSmokeyVFX smear = null;
    
    public Player Owner => Main.player[Projectile.owner];
    public ref float SwingProgress => ref Projectile.ai[0];
    public ref float ChargeTime => ref Projectile.ai[1];
    public ref float SwingMode => ref Projectile.ai[2];
    
    public override void AI()
    {
        // Calculate swing direction
        Vector2 direction = Projectile.rotation.ToRotationVector2();
        
        // ⭐ THE SMEAR PATTERN
        if (smear == null)
        {
            smear = new CircularSmearSmokeyVFX(
                Owner.Center, 
                currentColor, 
                direction.ToRotation(), 
                Projectile.scale * 1.5f
            );
            GeneralParticleHandler.SpawnParticle(smear);
        }
        else
        {
            smear.Rotation = direction.ToRotation() + MathHelper.PiOver2;
            smear.Time = 0;  // ⭐ Keep alive!
            smear.Position = Owner.Center;
            smear.Scale = Projectile.scale * 1.9f;
            smear.Color = currentColor;
        }
        
        // Spawn smoke along blade
        if (Main.rand.NextBool())
        {
            for (int i = 0; i < 5; i++)
            {
                float scaleFactor = 0.8f;
                Vector2 smokepos = Projectile.Center + (direction * (60 + 20 * i) * Projectile.scale);
                
                // Base smoke
                Particle smoke = new HeavySmokeParticle(
                    smokepos, 
                    direction.RotatedBy(-MathHelper.PiOver2) * 20f * scaleFactor + Owner.velocity, 
                    Color.Lerp(Color.MidnightBlue, Color.Indigo, i), 
                    10 + Main.rand.Next(5), 
                    scaleFactor * Main.rand.NextFloat(2.8f, 3.1f), 
                    Opacity + Main.rand.NextFloat(0f, 0.2f), 
                    0f, false, 0, true
                );
                GeneralParticleHandler.SpawnParticle(smoke);
            }
        }
        
        // Spawn bolts on full charge
        if (Charged && Main.rand.NextBool(3))
        {
            Vector2 boltPos = Projectile.Center + direction * Projectile.scale * 90f;
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(), 
                boltPos, 
                direction.RotatedByRandom(0.3f) * 15f,
                ModContent.ProjectileType<GalaxiaBolt>(), 
                (int)(Projectile.damage * FourSeasonsGalaxia.PhoenixAttunement_BoltDamageReduction),
                0f, Projectile.owner
            );
        }
    }
}
```

### Firewall Projectile

```csharp
public class PhoenixsPrideFirewall : ModProjectile
{
    // Uses MediumMistParticle for mist effects
    // Uses Bordernado shader for primitive flame rendering
    
    public override void AI()
    {
        // Spawn mist particles
        Particle mist = new MediumMistParticle(
            Projectile.Center + Main.rand.NextVector2Circular(40f, 40f),
            Vector2.UnitY * -Main.rand.NextFloat(1f, 3f),
            Color.OrangeRed,
            Color.Indigo,
            Main.rand.NextFloat(0.8f, 1.2f),
            Main.rand.NextFloat(180f, 220f),
            Main.rand.NextFloat(-0.1f, 0.1f)
        );
        GeneralParticleHandler.SpawnParticle(mist);
    }
    
    // Primitive rendering with Bordernado shader
    public override bool PreDraw(ref Color lightColor)
    {
        // Uses PrimitiveRenderer with Bordernado effect for flame pillar
    }
}
```

---

## Polaris Attunement (Ice/Shred Mode)

### Projectile: `Galaxia_PolarisGaze.cs`

```csharp
public class PolarisGaze : ModProjectile
{
    public override void AI()
    {
        // Ice-themed smoke (uses glowing layer with different hue)
        if (Main.rand.NextBool(3))
        {
            Vector2 smokepos = Projectile.Center + Main.rand.NextVector2Circular(30f, 30f);
            
            // Glowing ice smoke
            Particle smoke = new HeavySmokeParticle(
                smokepos,
                Projectile.velocity * 0.5f,
                Color.Lerp(Color.CornflowerBlue, Color.White, Main.rand.NextFloat(0.5f)),
                20,
                Main.rand.NextFloat(1.5f, 2.0f),
                0.8f,
                0f,
                true,      // ⭐ Glowing
                0.01f,     // ⭐ Hue shift
                true
            );
            GeneralParticleHandler.SpawnParticle(smoke);
        }
        
        // Constellation ring effect
        if (Timer % 30 == 0)
        {
            Particle ring = new ConstellationRingVFX(
                Projectile.Center,
                Color.CornflowerBlue,
                1.5f,
                40
            );
            GeneralParticleHandler.SpawnParticle(ring);
        }
    }
}
```

---

## Aries Attunement (Constellation Mode)

### Projectile: `Galaxia_AriesWrath.cs`

```csharp
public class AriesWrath : ModProjectile
{
    private CircularSmearSmokeyVFX smear = null;
    public ref float ChainSwapTimer => ref Projectile.ai[1];
    
    public override void AI()
    {
        // ⭐ Smear with alpha fade based on chain timer
        if (smear == null)
        {
            smear = new CircularSmearSmokeyVFX(
                Projectile.Center, 
                Color.MediumOrchid, 
                Projectile.rotation, 
                Projectile.scale
            );
            GeneralParticleHandler.SpawnParticle(smear);
        }
        if (smear != null)
        {
            smear.Position = Projectile.Center;
            smear.Rotation = Projectile.rotation + MathHelper.PiOver2 + MathHelper.PiOver4;
            smear.Time = 0;
            smear.Scale = Projectile.scale;
            // ⭐ Alpha fades based on chain swap progress
            smear.Color.A = (byte)(255 * MathHelper.Clamp(ChainSwapTimer / 50f, 0, 1));
        }
        
        // Dual-layer smoke
        if (Main.rand.NextBool())
        {
            Vector2 smokepos = Projectile.Center + Main.rand.NextVector2Circular(20f, 20f);
            
            // Base smoke
            Particle smoke = new HeavySmokeParticle(
                smokepos,
                Projectile.velocity * 0.3f,
                Color.Lerp(Color.Purple, Color.MediumOrchid, Main.rand.NextFloat()),
                15,
                Main.rand.NextFloat(2f, 2.5f),
                0.7f,
                0f,
                false,  // Base layer
                0,
                true
            );
            GeneralParticleHandler.SpawnParticle(smoke);
            
            // Glowing overlay
            Particle smokeGlow = new HeavySmokeParticle(
                smokepos,
                Projectile.velocity * 0.3f,
                Main.hslToRgb(0.85f, 1, 0.5f),
                20,
                Main.rand.NextFloat(2f, 2.5f) * 0.8f,
                0.7f,
                0f,
                true,   // ⭐ Glowing!
                0.01f,  // ⭐ Hue shift!
                true
            );
            GeneralParticleHandler.SpawnParticle(smokeGlow);
        }
    }
}
```

### Constellation Line Effect

```csharp
public class AriesWrathConstellation : ModProjectile
{
    private List<Vector2> starPositions = new List<Vector2>();
    
    public override void AI()
    {
        // Draw constellation lines between stored star positions
        for (int i = 1; i < starPositions.Count; i++)
        {
            Vector2 start = starPositions[i - 1];
            Vector2 end = starPositions[i];
            Vector2 delta = end - start;
            
            // BloomLineVFX connects stars with glowing lines
            Particle line = new BloomLineVFX(
                start,
                delta,
                0.8f,
                Color.MediumVioletRed * 0.75f,
                20,
                true,  // Bloom
                true   // Additive
            );
            GeneralParticleHandler.SpawnParticle(line);
            
            // Sparkle at each star
            Particle sparkle = new GenericSparkle(
                starPositions[i],
                Vector2.Zero,
                Color.White,
                Color.MediumOrchid,
                Main.rand.NextFloat(0.8f, 1.2f),
                30,
                Main.rand.NextFloat(-0.1f, 0.1f),
                3f
            );
            GeneralParticleHandler.SpawnParticle(sparkle);
        }
    }
}
```

---

## Andromeda Attunement (Dash Mode)

### Projectile: `Galaxia_AndromedasStride.cs`

**The highest damage mode (1120 base damage with 3.5x full charge multiplier = 3920 damage!)**

```csharp
public class AndromedasStride : ModProjectile
{
    public ref float ChargeProgress => ref Projectile.ai[0];
    public bool Dashing = false;
    
    public override void AI()
    {
        Vector2 direction = Projectile.rotation.ToRotationVector2();
        
        if (Dashing)
        {
            // Heavy smoke trail during dash
            if (Main.rand.NextBool())
            {
                Vector2 smokeSpeed = direction.RotatedByRandom(MathHelper.PiOver4 * 0.3f) 
                    * Main.rand.NextFloat(10f, 30f) * 0.9f;
                
                // ⭐ Pulsing color using sine wave
                Color smokeColor = Color.Lerp(
                    Color.Purple, 
                    Color.Indigo, 
                    (float)Math.Sin(Main.GlobalTimeWrappedHourly * 6f)
                );
                
                // Base smoke
                Particle smoke = new HeavySmokeParticle(
                    Projectile.Center + direction * 50f, 
                    smokeSpeed + Owner.velocity, 
                    smokeColor, 
                    30, 
                    Main.rand.NextFloat(0.6f, 1.2f), 
                    0.8f, 
                    0, 
                    false,  // Base layer
                    0, 
                    true
                );
                GeneralParticleHandler.SpawnParticle(smoke);
                
                // ⭐ 33% chance glowing overlay
                if (Main.rand.NextBool(3))
                {
                    Particle smokeGlow = new HeavySmokeParticle(
                        Projectile.Center + direction * 50f, 
                        smokeSpeed + Owner.velocity, 
                        Main.hslToRgb(0.85f, 1, 0.8f), 
                        20, 
                        Main.rand.NextFloat(0.6f, 1.2f) * 0.8f, 
                        0.8f, 
                        0, 
                        true,   // ⭐ Glowing!
                        0.01f,  // ⭐ Hue shift!
                        true
                    );
                    GeneralParticleHandler.SpawnParticle(smokeGlow);
                }
            }
            
            // Spawn bolts on hit
            if (HitEnemy)
            {
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    direction.RotatedByRandom(0.5f) * 12f,
                    ModContent.ProjectileType<GalaxiaBolt>(),
                    Projectile.damage / 2,
                    0f,
                    Projectile.owner
                );
            }
        }
    }
}
```

---

## Constellation Drawing System

### GalaxiaHoldout.cs - Star Positions

```csharp
public class GalaxiaHoldout : ModProjectile
{
    public Vector2[] StarPositions;
    public Color StarColor;
    private List<GenericSparkle> Stars = new List<GenericSparkle>();
    private List<BloomLineVFX> Lines = new List<BloomLineVFX>();
    
    public void SetupConstellation(AttunementID attunement)
    {
        switch (attunement)
        {
            case AttunementID.Phoenix:
                StarPositions = new Vector2[] 
                { 
                    new Vector2(-206, -99), 
                    new Vector2(-150, -43), 
                    new Vector2(-78, 1), 
                    new Vector2(-28, 11), 
                    new Vector2(64, -80), 
                    new Vector2(121, -69), 
                    new Vector2(200, -58), 
                    new Vector2(182, 43) 
                };
                StarColor = Color.OrangeRed;
                break;
                
            case AttunementID.Polaris: // Ursa Minor constellation
                StarPositions = new Vector2[] 
                { 
                    new Vector2(69, -188),   // Polaris (North Star)
                    new Vector2(18, -122), 
                    new Vector2(36, -84), 
                    new Vector2(-5, -48), 
                    new Vector2(-62, -11), 
                    new Vector2(-55, 72), 
                    new Vector2(-104, 75) 
                };
                StarColor = Color.CornflowerBlue;
                break;
                
            case AttunementID.Andromeda:
                StarPositions = new Vector2[] 
                { 
                    new Vector2(-210, -46), 
                    new Vector2(-150, -35), 
                    new Vector2(-89, -30), 
                    new Vector2(-16, 1), 
                    new Vector2(41, -15), 
                    new Vector2(71, -46), 
                    new Vector2(102, -11), 
                    new Vector2(148, 4), 
                    new Vector2(140, 60), 
                    new Vector2(200, 37) 
                };
                StarColor = Color.MediumSlateBlue;
                break;
                
            case AttunementID.Aries:
                StarPositions = new Vector2[] 
                { 
                    new Vector2(-180, 20), 
                    new Vector2(-100, -30), 
                    new Vector2(-20, -10), 
                    new Vector2(60, 40), 
                    new Vector2(140, 60) 
                };
                StarColor = Color.MediumOrchid;
                break;
        }
    }
    
    public override void AI()
    {
        // Spawn star particles
        for (int i = 0; i < StarPositions.Length; i++)
        {
            if (Stars.Count <= i || Stars[i] == null || Stars[i].Time >= Stars[i].Lifetime)
            {
                GenericSparkle star = new GenericSparkle(
                    Owner.Center + StarPositions[i],
                    Vector2.Zero,
                    Color.White,
                    StarColor,
                    Main.rand.NextFloat(1.2f, 1.6f),  // Scale
                    60,                                // Lifetime
                    Main.rand.NextFloat(-0.05f, 0.05f), // Rotation
                    5f                                  // Bloom strength
                );
                GeneralParticleHandler.SpawnParticle(star);
                
                if (Stars.Count <= i)
                    Stars.Add(star);
                else
                    Stars[i] = star;
            }
            else
            {
                // Update star position to follow player
                Stars[i].Position = Owner.Center + StarPositions[i];
                Stars[i].Time = 0;  // Keep alive
            }
        }
        
        // Draw constellation lines between stars
        for (int i = 1; i < StarPositions.Length; i++)
        {
            Vector2 start = Owner.Center + StarPositions[i - 1];
            Vector2 end = Owner.Center + StarPositions[i];
            
            if (Lines.Count <= i - 1 || Lines[i - 1] == null)
            {
                BloomLineVFX line = new BloomLineVFX(
                    start,
                    end - start,  // Direction vector
                    0.8f,         // Thickness
                    Color.MediumVioletRed * 0.75f,
                    20,           // Lifetime
                    true,         // Use bloom
                    true          // Additive blend
                );
                GeneralParticleHandler.SpawnParticle(line);
                
                if (Lines.Count <= i - 1)
                    Lines.Add(line);
                else
                    Lines[i - 1] = line;
            }
            else
            {
                // Update line endpoints
                Lines[i - 1].Position = start;
                Lines[i - 1].Velocity = end - start;
                Lines[i - 1].Time = 0;  // Keep alive
            }
        }
    }
}
```

---

## GalaxiaBolt Projectile

### Complete Implementation

```csharp
public class GalaxiaBolt : ModProjectile
{
    public ref float Time => ref Projectile.ai[0];
    
    public override void SetDefaults()
    {
        Projectile.width = 26;
        Projectile.height = 26;
        Projectile.friendly = true;
        Projectile.DamageType = TrueMeleeDamageClass.Instance;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 180;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
    }
    
    public override void AI()
    {
        Time++;
        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        
        // Homing after initial delay
        if (Time > 20)
        {
            NPC target = Projectile.FindTargetWithLineOfSight();
            if (target != null)
            {
                Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(
                    Projectile.velocity, 
                    toTarget * Projectile.velocity.Length(), 
                    0.08f
                );
            }
        }
        
        // ⭐ DUAL-LAYER SMOKE TRAIL (The key pattern!)
        if (Main.rand.NextBool())
        {
            Vector2 smokeVel = -Projectile.velocity * 0.3f + Main.rand.NextVector2Circular(2f, 2f);
            
            // Base smoke layer
            Particle smoke = new HeavySmokeParticle(
                Projectile.Center,
                smokeVel,
                Color.Lerp(Color.Purple, Color.Indigo, Main.rand.NextFloat()),
                15,
                Main.rand.NextFloat(0.8f, 1.2f),
                0.6f,
                Main.rand.NextFloat(-0.05f, 0.05f),
                false,  // Not glowing
                0,
                true
            );
            GeneralParticleHandler.SpawnParticle(smoke);
            
            // Glowing overlay (50% chance)
            if (Main.rand.NextBool())
            {
                Particle glow = new HeavySmokeParticle(
                    Projectile.Center,
                    smokeVel,
                    Main.hslToRgb(0.75f, 1, 0.6f),  // Purple-pink glow
                    20,
                    Main.rand.NextFloat(0.6f, 1.0f),
                    0.6f,
                    Main.rand.NextFloat(-0.05f, 0.05f),
                    true,   // ⭐ Glowing!
                    0.008f, // ⭐ Slight hue shift
                    true
                );
                GeneralParticleHandler.SpawnParticle(glow);
            }
        }
        
        // Sparkle accents
        if (Main.rand.NextBool(3))
        {
            Particle sparkle = new GenericSparkle(
                Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                -Projectile.velocity * 0.1f,
                Color.White,
                Color.MediumOrchid,
                Main.rand.NextFloat(0.4f, 0.8f),
                20,
                Main.rand.NextFloat(-0.1f, 0.1f),
                2f
            );
            GeneralParticleHandler.SpawnParticle(sparkle);
        }
        
        // Lighting
        Lighting.AddLight(Projectile.Center, Color.Purple.ToVector3() * 0.5f);
    }
    
    public override void OnKill(int timeLeft)
    {
        // Death explosion
        for (int i = 0; i < 8; i++)
        {
            Vector2 vel = Main.rand.NextVector2CircularEdge(5f, 5f);
            Particle smoke = new HeavySmokeParticle(
                Projectile.Center,
                vel,
                Color.Lerp(Color.Purple, Color.MediumOrchid, Main.rand.NextFloat()),
                20,
                Main.rand.NextFloat(1f, 1.5f),
                0.8f,
                0,
                true,   // Glowing death burst
                0.02f,
                false
            );
            GeneralParticleHandler.SpawnParticle(smoke);
        }
    }
}
```

---

## Color Palettes & Gradient System

### Phoenix (Fire) Colors

```csharp
// Tooltip colors
Color tooltipColor = new Color(255, 87, 0);   // Orange
Color tooltipColor2 = new Color(255, 143, 0); // Lighter orange

// Smoke colors
Color.Lerp(Color.MidnightBlue, Color.Indigo, progress);  // Purple base
Main.hslToRgb(0.85f, 1, 0.5f);  // Magenta glow overlay

// Fire pillar
Color.OrangeRed;
Color.Indigo;
```

### Polaris (Ice) Colors

```csharp
// Tooltip colors
Color tooltipColor = new Color(128, 189, 255);  // Ice blue
Color tooltipColor2 = new Color(169, 207, 255); // Lighter blue

// Smoke colors
Color.Lerp(Color.CornflowerBlue, Color.White, progress);
Color.CornflowerBlue;
```

### Aries (Constellation) Colors

```csharp
// Tooltip colors
Color tooltipColor = new Color(196, 89, 201);   // Purple
Color tooltipColor2 = new Color(222, 116, 227); // Lighter purple

// Smoke colors
Color.MediumOrchid;
Color.Purple;
Color.MediumVioletRed * 0.75f;  // Line color
```

### Andromeda (Cosmic Dash) Colors

```csharp
// Tooltip colors
Color tooltipColor = new Color(132, 128, 255);  // Purple-blue
Color tooltipColor2 = new Color(147, 144, 255); // Lighter

// Smoke colors - uses pulsing sine wave!
Color.Lerp(Color.Purple, Color.Indigo, (float)Math.Sin(Main.GlobalTimeWrappedHourly * 6f));
Main.hslToRgb(0.85f, 1, 0.8f);  // Bright glow overlay
```

### HSL Color Reference

```csharp
// Common hue values used:
// 0.75f = Purple/violet
// 0.80f = Purple-magenta
// 0.85f = Magenta/pink
// 0.90f = Pink-red

// The glow pattern:
Main.hslToRgb(hue, 1f, 0.5f);   // Saturated, medium brightness
Main.hslToRgb(hue, 1f, 0.6f);   // Saturated, brighter
Main.hslToRgb(hue, 1f, 0.8f);   // Saturated, very bright
```

---

## Implementation Checklist for MagnumOpus

### 1. Create CircularSmearSmokeyVFX Particle

```csharp
// Required texture: Assets/Particles/CircularSmearSmokey.png
// This should be a curved arc filled with cloudy noise

public class CircularSmearSmokeyVFX : Particle
{
    public override string Texture => "MagnumOpus/Assets/Particles/CircularSmearSmokey";
    public override bool UseAdditiveBlend => true;
    public override bool SetLifetime => true;
    
    public CircularSmearSmokeyVFX(Vector2 position, Color color, float rotation, float scale)
    {
        Position = position;
        Velocity = Vector2.Zero;
        Color = color;
        Scale = scale;
        Rotation = rotation;
        Lifetime = 2;
    }
}
```

### 2. Create HeavySmokeParticle with Glow Mode

```csharp
// Required texture: Assets/Particles/HeavySmoke.png (7 frame variants)

public class HeavySmokeParticle : Particle
{
    public override string Texture => "MagnumOpus/Assets/Particles/HeavySmoke";
    
    public bool glow;
    public float hueShift;
    
    public override bool UseAdditiveBlend => glow;  // KEY!
    
    public HeavySmokeParticle(
        Vector2 position, Vector2 velocity, Color color,
        int lifetime, float scale, float opacity,
        float spin = 0f, bool glowing = false, float hueshift = 0f, bool randomDelay = false)
    {
        Position = position;
        Velocity = velocity;
        Color = color;
        Lifetime = lifetime;
        Scale = scale;
        Opacity = opacity;
        Spin = spin;
        glow = glowing;
        hueShift = hueshift;
        Variant = Main.rand.Next(7);  // 7 frame variants
        
        if (randomDelay)
            Time = -Main.rand.Next(10, 21);
    }
    
    public override void Update()
    {
        Velocity *= 0.90f;
        
        // Hue shifting for flame effect
        if (hueShift != 0)
        {
            var hsl = Main.rgbToHsl(Color);
            float newHue = (hsl.X + hueShift) % 1f;
            Color = Main.hslToRgb(newHue, hsl.Y, hsl.Z);
        }
        
        // Fade out
        // ...
    }
}
```

### 3. Implement the Smear Persistence Pattern

```csharp
// In your projectile class:
private CircularSmearSmokeyVFX smear = null;

public override void AI()
{
    Vector2 direction = Projectile.rotation.ToRotationVector2();
    
    if (smear == null)
    {
        smear = new CircularSmearSmokeyVFX(
            Owner.Center, 
            themeColor, 
            direction.ToRotation(), 
            Projectile.scale * 1.5f
        );
        GeneralParticleHandler.SpawnParticle(smear);
    }
    else
    {
        smear.Rotation = direction.ToRotation() + MathHelper.PiOver2;
        smear.Time = 0;  // CRITICAL: Keep alive!
        smear.Position = Owner.Center;
        smear.Scale = Projectile.scale * 1.9f;
        smear.Color = themeColor;
    }
}

public override void OnKill(int timeLeft)
{
    if (smear != null)
        smear.Kill();  // Clean up on projectile death
}
```

### 4. Implement Dual-Layer Smoke

```csharp
// Spawn both layers together:
public void SpawnFlamingSmoke(Vector2 position, Vector2 velocity, Color baseColor)
{
    // LAYER 1: Base smoke (normal blend)
    Particle smoke = new HeavySmokeParticle(
        position,
        velocity,
        baseColor,
        15,
        Main.rand.NextFloat(2f, 2.5f),
        0.7f,
        0f,
        false,  // NOT glowing
        0,
        true
    );
    GeneralParticleHandler.SpawnParticle(smoke);
    
    // LAYER 2: Glowing overlay (additive blend + hue shift)
    Particle glow = new HeavySmokeParticle(
        position,
        velocity,
        Main.hslToRgb(0.85f, 1, 0.5f),  // Bright saturated color
        20,
        Main.rand.NextFloat(2f, 2.5f) * 0.8f,  // Slightly smaller
        0.7f,
        0f,
        true,   // GLOWING (additive)
        0.01f,  // HUE SHIFT (animates color)
        true
    );
    GeneralParticleHandler.SpawnParticle(glow);
}
```

### 5. Required Texture Assets

| Texture | Description | Size |
|---------|-------------|------|
| `CircularSmearSmokey.png` | Curved arc with cloudy noise fill | ~256x256 |
| `CircularSmear.png` | Clean curved arc (no noise) | ~256x256 |
| `SemiCircularSmear.png` | Half-circle arc | ~256x128 |
| `HeavySmoke.png` | 7-frame smoke variants | 7 frames horizontal |

### 6. Shader Integration (Optional)

For primitive trail rendering with the ImpFlameTrail effect:

```csharp
// Load effect
Effect impFlameTrail = ModContent.Request<Effect>("MagnumOpus/Assets/Shaders/ImpFlameTrail").Value;

// Set parameters
impFlameTrail.Parameters["uTime"].SetValue(Main.GlobalTimeWrappedHourly);
impFlameTrail.Parameters["uImage1"].SetValue(noiseTexture);

// Apply during primitive rendering
// ...
```

---

## Summary: The Complete Flaming Smoke Recipe

### Quick Reference Pattern

```csharp
// 1. Smear arc (persistent, updated each frame)
smear.Time = 0;  // Reset to keep alive
smear.Position = center;
smear.Rotation = direction.ToRotation() + MathHelper.PiOver2;

// 2. Base smoke (normal blend, dark colors)
new HeavySmokeParticle(..., glowing: false, hueshift: 0)

// 3. Glow overlay (additive blend, bright + shifting)
new HeavySmokeParticle(..., glowing: true, hueshift: 0.01f)
```

### Color Formula

```csharp
// Base layer: Dark, themed colors
Color.Lerp(Color.MidnightBlue, Color.Indigo, progress);
Color.Lerp(Color.Purple, Color.MediumOrchid, progress);

// Glow layer: Bright, saturated, HSL-based
Main.hslToRgb(0.85f, 1f, 0.5f);  // Magenta
Main.hslToRgb(0.75f, 1f, 0.6f);  // Purple
Main.hslToRgb(0.80f, 1f, 0.8f);  // Bright purple-magenta
```

### Why It Looks Like Flames

1. **Additive blending** makes overlapping areas BRIGHTER (like real fire)
2. **Hue shifting** animates colors subtly (like flickering flames)
3. **Noise texture** in smear creates organic, cloudy appearance
4. **Dual layers** create depth and richness
5. **Purple/magenta palette** gives cosmic fire appearance

---

## Additional Notes

### Performance Considerations

- Smear particles are reused (not spawned fresh each frame)
- Random delays on smoke prevent particle bursts
- Frame variants add visual variety without extra particles
- Additive blending is GPU-efficient

### Particle Lifetime Management

```csharp
// Smear: Lives forever (reset each frame)
smear.Time = 0;
smear.Lifetime = 2;

// Smoke: Natural decay
smoke.Lifetime = 15-30;  // Longer for trails
smoke.FadeRate = 0.06f;  // Gradual fade
```

### Integration with MagnumOpus Themes

| MagnumOpus Theme | Suggested Base Color | Suggested Glow HSL |
|------------------|---------------------|-------------------|
| Eroica | Scarlet/Crimson | (0.0f, 1f, 0.5f) |
| La Campanella | Orange/Black | (0.08f, 1f, 0.6f) |
| Moonlight Sonata | Dark Purple | (0.75f, 1f, 0.5f) |
| Fate | Pink/Crimson | (0.95f, 1f, 0.6f) |
| Enigma | Purple/Green | (0.35f, 1f, 0.5f) |
| Swan Lake | White/Black | (0.6f, 0.3f, 0.9f) |

---

*This documentation provides complete technical breakdown of the Galaxia "flaming smoke" VFX system for implementation in MagnumOpus.*
