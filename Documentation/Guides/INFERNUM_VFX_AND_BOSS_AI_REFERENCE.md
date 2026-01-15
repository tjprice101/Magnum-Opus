# Infernum Mode VFX and Boss AI Reference Guide

**Repository:** https://github.com/InfernumTeam/InfernumMode

This document provides a comprehensive reference of visual effects, boss AI patterns, particles, lighting, and screen effects from InfernumMode to incorporate into MagnumOpus for better boss fights, attacks, and visual effects.

---

## Table of Contents

1. [Particle System Overview](#particle-system-overview)
2. [Particle Types and Usage](#particle-types-and-usage)
3. [Primitive Trail System](#primitive-trail-system)
4. [Shader Effects](#shader-effects)
5. [Screen Effects](#screen-effects)
6. [Boss AI Patterns](#boss-ai-patterns)
7. [Explosion and Impact Effects](#explosion-and-impact-effects)
8. [Utility Methods](#utility-methods)
9. [Implementation Examples](#implementation-examples)

---

## Particle System Overview

Infernum uses `GeneralParticleHandler.SpawnParticle()` to spawn custom particles. This is similar to MagnumOpus's `MagnumParticleHandler.SpawnParticle()`.

### Basic Spawn Pattern
```csharp
// Spawn a particle
GeneralParticleHandler.SpawnParticle(new ParticleType(position, velocity, color, scale, lifetime));

// Example with PulseRing
GeneralParticleHandler.SpawnParticle(new PulseRing(position, Vector2.Zero, Color.Red, 0f, 3.5f, 50));
```

---

## Particle Types and Usage

### PulseRing - Expanding Ring Effects
**Use for:** Shockwaves, explosions, phase transitions, energy bursts

```csharp
// Basic expanding ring
PulseRing ring = new(position, Vector2.Zero, Color.MediumPurple, 3.6f, 0f, 60);
GeneralParticleHandler.SpawnParticle(ring);

// Parameters: position, velocity, color, startScale, endScale, lifetime
PulseRing ring = new(npc.Center, Vector2.Zero, energyColor, 0f, 16f, 20);
```

### StrongBloom - Bright Bloom/Glow Effects
**Use for:** Impact highlights, energy bursts, phase transitions

```csharp
// Bright bloom at center
StrongBloom bloom = new(position, Vector2.Zero, Color.Orange * 0.56f, 2f, 35);
GeneralParticleHandler.SpawnParticle(bloom);

// Fire bloom
StrongBloom fire = new(position, Vector2.Zero, Color.Orange * 0.56f, 2f, 35);
```

### CloudParticle - Smoke/Cloud Effects
**Use for:** Fire explosions, teleport effects, impact clouds

```csharp
// Fire cloud
CloudParticle fireCloud = new(
    position, 
    (TwoPi * i / 6f).ToRotationVector2() * 2f + Main.rand.NextVector2Circular(0.3f, 0.3f), 
    Color.HotPink,       // Start color
    Color.DarkGray,      // End color
    33,                  // Lifetime
    Main.rand.NextFloat(1.8f, 2f)  // Scale
) {
    Rotation = Main.rand.NextFloat(TwoPi)
};
GeneralParticleHandler.SpawnParticle(fireCloud);

// Teleport cloud
CloudParticle teleportCloud = new(teleportPosition, Main.rand.NextVector2Circular(6f, 6f), fireColor, Color.DarkGray, 36, Main.rand.NextFloat(1.4f, 1.6f));
```

### HeavySmokeParticle - Dense Smoke
**Use for:** Explosions, death effects, trail smoke

```csharp
// Death explosion smoke
for (int i = 0; i < 32; i++)
{
    Color smokeColor = Color.Lerp(Color.Yellow, Color.Cyan, Main.rand.NextFloat());
    Vector2 smokeVelocity = (TwoPi * i / 32f).ToRotationVector2() * Main.rand.NextFloat(7f, 11.5f) + Main.rand.NextVector2Circular(4f, 4f);
    GeneralParticleHandler.SpawnParticle(new HeavySmokeParticle(npc.Center, smokeVelocity, smokeColor, 56, 2.4f, 1f));
    
    // Second layer with doubled velocity
    smokeVelocity *= 2f;
    GeneralParticleHandler.SpawnParticle(new HeavySmokeParticle(npc.Center, smokeVelocity, smokeColor, 56, 3f, 1f));
}

// Fire smoke with glow
var particle = new HeavySmokeParticle(
    position, Vector2.Zero, 
    fireColor, 
    lifeTime: 90, 
    scale: fireScale, 
    opacity: 1, 
    rotationSpeed: fireRotationSpeed, 
    glowing: true,
    screenDepthOffset: 0f, 
    useSmokyParticleEffect: true
);
```

### SparkParticle - Directional Sparks
**Use for:** Impacts, explosions, electrical effects

```csharp
// Explosion sparks
for (int i = 0; i < 40; i++)
{
    Vector2 sparkVelocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(5f, 24f);
    GeneralParticleHandler.SpawnParticle(new SparkParticle(npc.Center, sparkVelocity, Main.rand.NextBool(4), 60, 2f, Color.Gold));
}

// Impact sparks
Vector2 sparkVelocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(3f, 16f);
Color sparkColor = Color.Lerp(Color.Cyan, Color.IndianRed, Main.rand.NextFloat(0.6f));
GeneralParticleHandler.SpawnParticle(new SparkParticle(impactPoint, sparkVelocity, false, 45, 2f, sparkColor));
```

### ElectricArc - Lightning Arc Effects
**Use for:** Electrical attacks, death animations, energy effects

```csharp
// Electric death explosion
for (int i = 0; i < 20; i++)
{
    Vector2 sparkVelocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(5f, 24f);
    Color arcColor = Color.Lerp(Color.Yellow, Color.Cyan, Main.rand.NextFloat());
    GeneralParticleHandler.SpawnParticle(new ElectricArc(npc.Center, sparkVelocity, arcColor, 0.84f, 60));
}
```

### ElectricExplosionRing - Electric Ring Explosion
**Use for:** Major explosions, death effects, phase transitions

```csharp
// Exo death explosion
GeneralParticleHandler.SpawnParticle(new ElectricExplosionRing(npc.Center, Vector2.Zero, CalamityUtils.ExoPalette, 3f, 90));
```

### SquishyLightParticle - Soft Glowing Particles
**Use for:** Magic effects, energy gathering, ambient particles

```csharp
// Energy gathering effect
for (int i = 0; i < 8; i++)
{
    Vector2 sparkVelocity = -Vector2.UnitY.RotatedByRandom(1.23f) * Main.rand.NextFloat(6f, 14f);
    GeneralParticleHandler.SpawnParticle(new SquishyLightParticle(npc.Center, sparkVelocity, 1f, Color.Red, 55, 1f, 5f));
}
```

### GlowyLightParticle - Glowing Light Particles
**Use for:** Fire effects, energy bursts, impact effects

```csharp
// Fire impact light
var light = new GlowyLightParticle(
    position, 
    impactCenter.DirectionTo(position) * Main.rand.NextFloat(3f, 5f),
    Main.rand.NextBool() ? WayfinderSymbol.Colors[1] : Color.OrangeRed, 
    60, 
    Main.rand.NextFloat(0.85f, 1.15f) * scaleModifier, 
    Main.rand.NextFloat(0.95f, 1.05f), 
    false
);
GeneralParticleHandler.SpawnParticle(light);
```

### MediumMistParticle - Mist/Fire Effects
**Use for:** Fire explosions, ash effects, atmospheric particles

```csharp
// Fire explosion mist
MediumMistParticle fireExplosion = new(
    impactCenter + Main.rand.NextVector2Circular(80f, 80f), 
    Vector2.Zero,
    Main.rand.NextBool() ? WayfinderSymbol.Colors[0] : WayfinderSymbol.Colors[1],
    Color.Gray, 
    Main.rand.NextFloat(0.85f, 1.15f) * scaleModifier, 
    Main.rand.NextFloat(220f, 250f)
);
GeneralParticleHandler.SpawnParticle(fireExplosion);

// Plasma effect
MediumMistParticle plasma = new(endOfFlamethrower, plasmaVelocity, Color.Lime, Color.YellowGreen, 1.3f, 255f);
```

### SmallSmokeParticle - Small Smoke Puffs
**Use for:** Sand effects, small explosions, trail effects

```csharp
// Sand burst
SmallSmokeParticle sand = new(
    particleSpawnPosition + Main.rand.NextVector2Circular(10f, 10f), 
    Main.rand.NextVector2Circular(4f, 8f) - Vector2.UnitY * 9f, 
    sandColor, 
    Color.Tan, 
    Main.rand.NextFloat(0.32f, 0.67f), 
    255f, 
    Main.rand.NextFloatDirection() * 0.015f
);
GeneralParticleHandler.SpawnParticle(sand);
```

### ExplosionRing - Circular Explosion Ring
**Use for:** Explosions, impacts, shockwaves

```csharp
ExplosionRing ring = new(position, velocity, color, scale, lifeTime, rotationSpeed: 0.2f);
GeneralParticleHandler.SpawnParticle(ring);
```

### CritSpark - Critical Hit Sparks
**Use for:** Impacts, charge-up effects, energy convergence

```csharp
// Energy convergence sparks
CritSpark spark = new(npc.Center, Main.rand.NextVector2Circular(8f, 8f), Color.LightCyan, Color.Cyan, 5f, 6, 0.01f, 7.5f);
GeneralParticleHandler.SpawnParticle(spark);

// Fire charge sparks
CritSpark spark = new(npc.Center, Main.rand.NextVector2Circular(8f, 8f), Color.OrangeRed, Color.Gold, 5f, 6, 0.01f, 7.5f);
```

### FlareShine - Bright Flare Effects
**Use for:** Major impacts, death effects, dramatic moments

```csharp
FlareShine strike = new(
    Projectile.Center, 
    Vector2.Zero, 
    Color.MediumPurple,    // Main color
    bloomColor,             // Bloom color
    0f,                     // Start scale
    Vector2.One * 9f,       // End scale
    Vector2.Zero,           // Velocity
    40,                     // Lifetime
    0f,                     // Initial rotation
    8f                      // Scale multiplier
);
GeneralParticleHandler.SpawnParticle(strike);
```

### DirectionalPulseRing - Directional Ring Effects
**Use for:** Beam impacts, directional explosions

```csharp
var pulse = new DirectionalPulseRing(
    Projectile.Center, 
    Vector2.Zero, 
    pulseColor, 
    Vector2.One * 1.35f,    // Scale
    Projectile.velocity.ToRotation(),  // Direction
    0.05f,                  // Start opacity
    0.42f,                  // End opacity
    30                      // Lifetime
);
GeneralParticleHandler.SpawnParticle(pulse);
```

### BurstParticle - Custom Burst Effects
**Use for:** Major explosions, special effects

```csharp
public class BurstParticle : Particle
{
    public readonly bool UseExtraBloom;
    public readonly Color ExtraBloomColor;
    public float Opacity;
    public Vector2 DrawScale;
    
    // Important flags
    public override bool Important => true;  // Bypasses particle limit
    public override bool SetLifetime => true;
    public override bool UseCustomDraw => true;
}
```

---

## Primitive Trail System

Infernum uses `PrimitiveTrailCopy` for smooth trail rendering with shaders.

### Basic Setup
```csharp
public class MyProjectile : ModProjectile, IPixelPrimitiveDrawer
{
    internal PrimitiveTrailCopy TrailDrawer;
    
    // Width function - controls trail width along its length
    public float WidthFunction(float completionRatio)
    {
        return Lerp(0f, 50f, completionRatio) * Projectile.scale;
    }
    
    // Color function - controls trail color along its length
    public Color ColorFunction(float completionRatio)
    {
        return Color.Lerp(Color.White, Color.Red, completionRatio) * Projectile.Opacity;
    }
    
    public void DrawPixelPrimitives(SpriteBatch spriteBatch)
    {
        // Initialize drawer with shader
        TrailDrawer ??= new PrimitiveTrailCopy(
            WidthFunction, 
            ColorFunction, 
            null,           // Offset function (optional)
            true,           // Use smoothening
            InfernumEffectsRegistry.FireVertexShader  // Shader
        );
        
        // Configure shader
        InfernumEffectsRegistry.FireVertexShader.UseSaturation(0.4f);
        InfernumEffectsRegistry.FireVertexShader.UseImage1("Images/Misc/Perlin");
        
        // Draw the trail
        TrailDrawer.DrawPixelated(Projectile.oldPos, Projectile.Size * 0.5f - Main.screenPosition, 38);
    }
}
```

### Using CalamityMod's PrimitiveRenderer
```csharp
// Simpler approach using PrimitiveRenderer
PrimitiveRenderer.RenderTrail(
    Projectile.oldPos, 
    new PrimitiveSettings(
        PrimitiveWidthFunction, 
        PrimitiveColorFunction, 
        _ => Projectile.Size * 0.5f,  // Offset
        false,                         // Pixelated
        Shader: InfernumEffectsRegistry.GaleLightningShader
    ), 
    pointCount: 18
);
```

### Laser/Beam Drawing
```csharp
public void DrawPixelPrimitives(SpriteBatch spriteBatch)
{
    BeamDrawer ??= new PrimitiveTrailCopy(WidthFunction, ColorFunction, null, true, InfernumEffectsRegistry.ArtemisLaserVertexShader);
    
    // Configure laser shader
    var oldBlendState = Main.instance.GraphicsDevice.BlendState;
    Main.instance.GraphicsDevice.BlendState = BlendState.Additive;
    
    InfernumEffectsRegistry.ArtemisLaserVertexShader.UseSaturation(1.4f);
    InfernumEffectsRegistry.ArtemisLaserVertexShader.UseOpacity(-0.1f);
    InfernumEffectsRegistry.ArtemisLaserVertexShader.SetShaderTexture(InfernumTextureRegistry.HarshNoise);
    
    // Create points along the laser
    List<Vector2> points = new();
    for (int i = 0; i <= 8; i++)
        points.Add(Vector2.Lerp(Projectile.Center, Projectile.Center + Projectile.velocity * LaserLength, i / 8f));
    
    BeamDrawer.DrawPixelated(points, -Main.screenPosition, 47);
    
    Main.instance.GraphicsDevice.BlendState = oldBlendState;
}
```

### Lightning Drawing
```csharp
public override bool PreDraw(ref Color lightColor)
{
    var lightning = InfernumEffectsRegistry.GaleLightningShader;
    Main.instance.GraphicsDevice.Textures[1] = ModContent.Request<Texture2D>("Terraria/Images/Misc/Perlin").Value;
    
    PrimitiveRenderer.RenderTrail(
        Projectile.oldPos, 
        new PrimitiveSettings(
            PrimitiveWidthFunction, 
            PrimitiveColorFunction, 
            _ => Projectile.Size * 0.5f, 
            false, 
            Shader: lightning
        ), 
        18
    );
    return false;
}
```

---

## Shader Effects

### Available Primitive Shaders

| Shader Name | Use Case |
|-------------|----------|
| `FireVertexShader` | Fire/flame trails |
| `ArtemisLaserVertexShader` | Energy lasers |
| `GaleLightningShader` | Lightning bolts |
| `AresLightningVertexShader` | Arc lightning |
| `PrismaticRayVertexShader` | Rainbow/prismatic beams |
| `GenericLaserVertexShader` | Generic laser effects |
| `TwinsFlameTrailVertexShader` | Twins boss flame trails |
| `ProfanedLavaVertexShader` | Lava effects |
| `PolterghastEctoplasmVertexShader` | Ectoplasm effects |
| `RealityTearVertexShader` | Reality slice effects |
| `DarkFlamePillarVertexShader` | Dark flame pillars |
| `GuardiansLaserVertexShader` | Guardian boss lasers |
| `SideStreakVertexShader` | Side streak telegraphs |
| `FlameVertexShader` | General flame effects |

### Shader Configuration Examples

```csharp
// Fire shader
InfernumEffectsRegistry.FireVertexShader.UseSaturation(0.4f);
InfernumEffectsRegistry.FireVertexShader.SetShaderTexture(InfernumTextureRegistry.HarshNoise);
InfernumEffectsRegistry.FireVertexShader.UseImage1("Images/Misc/Perlin");

// Laser shader with multiple textures
InfernumEffectsRegistry.GuardiansLaserVertexShader.SetShaderTexture(InfernumTextureRegistry.LavaNoise);
InfernumEffectsRegistry.GuardiansLaserVertexShader.SetShaderTexture2(InfernumTextureRegistry.CultistRayMap);
InfernumEffectsRegistry.GuardiansLaserVertexShader.UseColor(Color.LightGoldenrodYellow);
InfernumEffectsRegistry.GuardiansLaserVertexShader.Shader.Parameters["flipY"].SetValue(false);

// Prismatic shader
InfernumEffectsRegistry.PrismaticRayVertexShader.UseOpacity(0.2f);
InfernumEffectsRegistry.PrismaticRayVertexShader.UseImage1("Images/Misc/Perlin");
Main.instance.GraphicsDevice.Textures[2] = InfernumTextureRegistry.StreakSolid.Value;
```

---

## Screen Effects

### Screen Shake
```csharp
// Basic screen shake
Main.LocalPlayer.Infernum_Camera().CurrentScreenShakePower = 12f;

// Or through Calamity
Main.LocalPlayer.Calamity().GeneralScreenShakePower = screenShakePower;
```

### Screen Flash
```csharp
// Trigger a screen flash at a position
ScreenEffectSystem.SetFlashEffect(npc.Center, 1f, 45);  // position, intensity (0-1), lifetime in frames

// Strong flash
ScreenEffectSystem.SetFlashEffect(target.Center, 2f, 45);
```

### Screen Blur
```csharp
// Create blur effect at position
ScreenEffectSystem.SetBlurEffect(npc.Center, 1f, 25);  // position, intensity (0-1), lifetime

// Stronger blur
ScreenEffectSystem.SetBlurEffect(npc.Center, 2f, 45);
```

### Shockwave Effect
```csharp
// Create visual shockwave
Utilities.CreateShockwave(
    npc.Center,     // Position
    4,              // Ripple count
    15,             // Ripple size
    192f,           // Ripple speed
    false           // Play sound
);

// With secondary variant
Utilities.CreateShockwave(npc.Center, 2, 8, 75f, false, true);
```

### Screen Distortion
```csharp
// Activate screen distortion shader
Filters.Scene.Activate("InfernumMode:ScreenDistortion", Main.LocalPlayer.Center);
InfernumEffectsRegistry.ScreenDistortionScreenShader.GetShader().UseImage("Images/Extra_193");
InfernumEffectsRegistry.ScreenDistortionScreenShader.GetShader().Shader.Parameters["distortionAmount"].SetValue(distortionInterpolant * 25f);
InfernumEffectsRegistry.ScreenDistortionScreenShader.GetShader().Shader.Parameters["wiggleSpeed"].SetValue(2f);
```

### Movie Bars (Cinematic Effect)
```csharp
// Set movie bar effect
ScreenEffectSystem.SetMovieBarEffect(
    barOffset,              // How much screen to cover (0-1)
    lifetime,               // Duration
    fadeInterpolantFunction // Optional fade function
);
```

### Camera Focus
```csharp
// Focus camera on a point
player.Infernum_Camera().ScreenFocusPosition = focusPoint;
player.Infernum_Camera().ScreenFocusInterpolant = 0.8f;  // 0-1 interpolation to focus point
player.Infernum_Camera().ScreenFocusHoldInPlaceTime = 60;  // Hold time in frames
```

---

## Boss AI Patterns

### State Machine Pattern
```csharp
public enum MyBossAttackState
{
    SpawnAnimation,
    Phase1Attack1,
    Phase1Attack2,
    PhaseTransition,
    Phase2Attack1,
    Phase2Attack2,
    DeathAnimation
}

public override bool PreAI(NPC npc)
{
    ref float attackState = ref npc.ai[0];
    ref float attackTimer = ref npc.ai[1];
    
    switch ((MyBossAttackState)attackState)
    {
        case MyBossAttackState.SpawnAnimation:
            DoBehavior_SpawnAnimation(npc, target, ref attackTimer);
            break;
        case MyBossAttackState.Phase1Attack1:
            DoBehavior_Phase1Attack1(npc, target, ref attackTimer);
            break;
        // ... etc
    }
    
    attackTimer++;
    return false;
}
```

### Attack Pattern Lists
```csharp
// Define attack patterns for different phases
public static readonly List<MyBossAttackState> Phase1AttackPattern = new()
{
    MyBossAttackState.Phase1Attack1,
    MyBossAttackState.Phase1Attack2,
    MyBossAttackState.Phase1Attack3,
};

public static readonly List<MyBossAttackState> Phase2AttackPattern = new()
{
    MyBossAttackState.Phase2Attack1,
    MyBossAttackState.Phase2Attack2,
    MyBossAttackState.Phase2Attack3,
};

// Select next attack
public static void SelectNextAttack(NPC npc)
{
    float lifeRatio = npc.life / (float)npc.lifeMax;
    var pattern = lifeRatio > 0.5f ? Phase1AttackPattern : Phase2AttackPattern;
    
    ref float attackState = ref npc.ai[0];
    ref float attackIndex = ref npc.Infernum().ExtraAI[0];
    
    attackIndex = (attackIndex + 1) % pattern.Count;
    attackState = (float)pattern[(int)attackIndex];
    npc.ai[1] = 0f;  // Reset attack timer
    npc.netUpdate = true;
}
```

### Teleport Behavior
```csharp
public static void DoBehavior_Teleport(NPC npc, Player target, ref float attackTimer, ref float teleportFadeInterpolant)
{
    int fadeOutTime = 15;
    int teleportDelay = 20;
    int fadeInTime = 15;
    
    // Fade out
    if (attackTimer < fadeOutTime)
    {
        teleportFadeInterpolant = attackTimer / fadeOutTime;
        npc.Opacity = 1f - teleportFadeInterpolant;
    }
    
    // Teleport
    if (attackTimer == teleportDelay)
    {
        Vector2 teleportPosition = target.Center + Main.rand.NextVector2CircularEdge(400f, 400f);
        npc.Center = teleportPosition;
        npc.netUpdate = true;
        
        // Teleport VFX
        for (int i = 0; i < 30; i++)
        {
            CloudParticle cloud = new(npc.Center, Main.rand.NextVector2Circular(6f, 6f), themeColor, Color.DarkGray, 36, Main.rand.NextFloat(1.4f, 1.6f));
            GeneralParticleHandler.SpawnParticle(cloud);
        }
    }
    
    // Fade in
    if (attackTimer > teleportDelay && attackTimer < teleportDelay + fadeInTime)
    {
        float fadeProgress = (attackTimer - teleportDelay) / fadeInTime;
        teleportFadeInterpolant = 1f - fadeProgress;
        npc.Opacity = fadeProgress;
    }
    
    if (attackTimer >= teleportDelay + fadeInTime)
    {
        SelectNextAttack(npc);
    }
}
```

### Charge/Dash Attack
```csharp
public static void DoBehavior_Charge(NPC npc, Player target, ref float attackTimer)
{
    int chargeDelay = 30;
    int chargeTime = 40;
    int recoveryTime = 20;
    float chargeSpeed = 35f;
    int chargeCount = 3;
    
    ref float chargeCounter = ref npc.Infernum().ExtraAI[0];
    
    // Aim at target before charging
    if (attackTimer < chargeDelay)
    {
        float aimSpeed = 0.08f;
        npc.velocity = npc.velocity.MoveTowards(npc.SafeDirectionTo(target.Center) * 5f, aimSpeed);
        
        // Charge windup VFX
        if (attackTimer % 5 == 0)
        {
            CustomParticles.GenericFlare(npc.Center, themeColor, 0.5f * (attackTimer / chargeDelay), 15);
        }
    }
    
    // Begin charge
    if (attackTimer == chargeDelay)
    {
        npc.velocity = npc.SafeDirectionTo(target.Center + target.velocity * 10f) * chargeSpeed;
        
        // Charge VFX
        SoundEngine.PlaySound(SoundID.Roar, npc.Center);
        ScreenEffectSystem.SetFlashEffect(npc.Center, 1f, 45);
        target.Infernum_Camera().CurrentScreenShakePower = 3f;
        Utilities.CreateShockwave(npc.Center, 4, 15, 192f);
    }
    
    // During charge - add friction
    if (attackTimer > chargeDelay && attackTimer < chargeDelay + chargeTime)
    {
        npc.velocity *= 0.98f;
    }
    
    // Recovery and next charge
    if (attackTimer >= chargeDelay + chargeTime + recoveryTime)
    {
        chargeCounter++;
        if (chargeCounter >= chargeCount)
        {
            chargeCounter = 0f;
            SelectNextAttack(npc);
        }
        else
        {
            attackTimer = 0f;
        }
    }
}
```

### Phase Transition
```csharp
public static void DoBehavior_PhaseTransition(NPC npc, Player target, ref float attackTimer, ref float phaseTransitionIntensity)
{
    int transitionDuration = 180;
    
    // Build up intensity
    phaseTransitionIntensity = Utils.GetLerpValue(0f, transitionDuration * 0.7f, attackTimer, true);
    
    // Visual effects that scale with intensity
    if (attackTimer % 10 == 0)
    {
        for (int i = 0; i < (int)(8 * phaseTransitionIntensity); i++)
        {
            float hue = (Main.GameUpdateCount * 0.02f + i * 0.16f) % 1f;
            Color fractalColor = Main.hslToRgb(hue, 1f, 0.85f);
            CustomParticles.GenericFlare(npc.Center + Main.rand.NextVector2Circular(50f, 50f), fractalColor, 0.4f * phaseTransitionIntensity, 25);
        }
    }
    
    // Progressive screen shake
    target.Infernum_Camera().CurrentScreenShakePower = phaseTransitionIntensity * 12f;
    
    // Climax at end
    if (attackTimer == transitionDuration - 20)
    {
        // Big explosion
        GeneralParticleHandler.SpawnParticle(new ElectricExplosionRing(npc.Center, Vector2.Zero, CalamityUtils.ExoPalette, 3f, 90));
        ScreenEffectSystem.SetFlashEffect(npc.Center, 2f, 45);
        ScreenEffectSystem.SetBlurEffect(npc.Center, 2f, 90);
        Utilities.CreateShockwave(npc.Center);
        
        // Massive particle burst
        for (int i = 0; i < 50; i++)
        {
            Vector2 sparkVelocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(5f, 30f);
            GeneralParticleHandler.SpawnParticle(new SparkParticle(npc.Center, sparkVelocity, false, 60, 2f, themeColor));
        }
    }
    
    if (attackTimer >= transitionDuration)
    {
        phaseTransitionIntensity = 0f;
        // Transition to phase 2 attacks
        SelectNextAttack(npc);
    }
}
```

### Death Animation
```csharp
public static void DoBehavior_DeathAnimation(NPC npc, Player target, ref float deathAnimationTimer)
{
    int deathDuration = 180;
    npc.dontTakeDamage = true;
    npc.velocity *= 0.95f;
    
    // Periodic explosions
    if (deathAnimationTimer % 15 == 0)
    {
        Vector2 explosionPos = npc.Center + Main.rand.NextVector2Circular(npc.width, npc.height);
        
        GeneralParticleHandler.SpawnParticle(new PulseRing(explosionPos, Vector2.Zero, Color.Red, 0f, 3.5f, 50));
        
        for (int i = 0; i < 8; i++)
        {
            Vector2 sparkVelocity = -Vector2.UnitY.RotatedByRandom(1.23f) * Main.rand.NextFloat(6f, 14f);
            GeneralParticleHandler.SpawnParticle(new SquishyLightParticle(explosionPos, sparkVelocity, 1f, Color.Red, 55, 1f, 5f));
        }
    }
    
    // Screen shake builds up
    target.Infernum_Camera().CurrentScreenShakePower = deathAnimationTimer / deathDuration * 15f;
    
    // Final explosion
    if (deathAnimationTimer >= deathDuration)
    {
        PerformDeathExplosion(npc);
    }
    
    deathAnimationTimer++;
}

public static void PerformDeathExplosion(NPC npc)
{
    SoundEngine.PlaySound(SoundID.DD2_BetsyDeath, npc.Center);
    
    // Electric explosion ring
    GeneralParticleHandler.SpawnParticle(new ElectricExplosionRing(npc.Center, Vector2.Zero, CalamityUtils.ExoPalette, 3f, 90));
    
    // Sparks
    for (int i = 0; i < 40; i++)
    {
        Vector2 sparkVelocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(5f, 24f);
        GeneralParticleHandler.SpawnParticle(new SparkParticle(npc.Center, sparkVelocity, Main.rand.NextBool(4), 60, 2f, Color.Gold));
        
        Color arcColor = Color.Lerp(Color.Yellow, Color.Cyan, Main.rand.NextFloat());
        GeneralParticleHandler.SpawnParticle(new ElectricArc(npc.Center, sparkVelocity, arcColor, 0.84f, 60));
    }
    
    // Smoke ring
    for (int i = 0; i < 32; i++)
    {
        Color smokeColor = Color.Lerp(Color.Yellow, Color.Cyan, Main.rand.NextFloat());
        Vector2 smokeVelocity = (TwoPi * i / 32f).ToRotationVector2() * Main.rand.NextFloat(7f, 11.5f);
        GeneralParticleHandler.SpawnParticle(new HeavySmokeParticle(npc.Center, smokeVelocity, smokeColor, 56, 2.4f, 1f));
    }
    
    // Shockwave and screen effects
    Utilities.CreateShockwave(npc.Center);
    ScreenEffectSystem.SetFlashEffect(npc.Center, 2f, 60);
    ScreenEffectSystem.SetBlurEffect(npc.Center, 2f, 60);
    
    npc.life = 0;
    npc.checkDead();
}
```

---

## Explosion and Impact Effects

### Fire Explosion
```csharp
public static void CreateFireExplosion(Vector2 position, float scale = 1f)
{
    // Light particles
    for (int i = 0; i < 20; i++)
    {
        Vector2 pos = position + Main.rand.NextVector2Circular(20f, 20f);
        var light = new GlowyLightParticle(
            pos, 
            position.DirectionTo(pos) * Main.rand.NextFloat(3f, 5f),
            Main.rand.NextBool() ? Color.Orange : Color.OrangeRed, 
            60, 
            Main.rand.NextFloat(0.85f, 1.15f) * scale, 
            Main.rand.NextFloat(0.95f, 1.05f), 
            false
        );
        GeneralParticleHandler.SpawnParticle(light);
    }
    
    // Fire mist
    for (int i = 0; i < 30; i++)
    {
        MediumMistParticle fire = new(
            position + Main.rand.NextVector2Circular(80f, 80f), 
            Vector2.Zero,
            Main.rand.NextBool() ? Color.Orange : Color.OrangeRed,
            Color.Gray, 
            Main.rand.NextFloat(0.85f, 1.15f) * scale, 
            Main.rand.NextFloat(220f, 250f)
        );
        GeneralParticleHandler.SpawnParticle(fire);
    }
}
```

### Generic Dust Explosion
```csharp
public static void CreateGenericDustExplosion(Vector2 spawnPosition, int dustType, int dustPerBurst, float burstSpeed, float baseScale)
{
    float burstDirectionVariance = 3;
    for (int j = 0; j < 10; j++)
    {
        burstDirectionVariance += j * 2;
        for (int k = 0; k < dustPerBurst; k++)
        {
            Dust burstDust = Dust.NewDustPerfect(spawnPosition, dustType);
            burstDust.scale = baseScale * Main.rand.NextFloat(0.8f, 1.2f);
            burstDust.position = spawnPosition + Main.rand.NextVector2Circular(10f, 10f);
            burstDust.velocity = Main.rand.NextVector2Square(-burstDirectionVariance, burstDirectionVariance).SafeNormalize(Vector2.UnitY) * burstSpeed;
            burstDust.noGravity = true;
        }
        burstSpeed += 3f;
    }
}
```

### Radial Dust Burst
```csharp
// Circular dust burst
for (int i = 0; i < 36; i++)
{
    Dust energy = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.TheDestroyer);
    energy.velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(3f, 7f);
    energy.noGravity = true;
}

// Precise radial burst
int numDust = 18;
for (int i = 0; i < numDust; i++)
{
    Vector2 ringVelocity = (TwoPi * i / numDust).ToRotationVector2().RotatedBy(direction.ToRotation() + PiOver2) * 5f;
    Dust magic = Dust.NewDustPerfect(position, DustID.RainbowMk2);
    magic.color = themeColor;
    magic.velocity = ringVelocity;
    magic.noGravity = true;
}
```

---

## Utility Methods

### Lerp Value (Inverse Lerp)
```csharp
// Gets a 0-1 value based on where 'value' falls between 'from' and 'to'
float progress = Utils.GetLerpValue(startTime, endTime, currentTime, true);  // true clamps to 0-1
```

### Safe Direction
```csharp
Vector2 direction = npc.SafeDirectionTo(target.Center);
Vector2 normalizedVelocity = velocity.SafeNormalize(Vector2.UnitY);
```

### Random Vectors
```csharp
Vector2 randomCircular = Main.rand.NextVector2Circular(radius, radius);
Vector2 randomCircularEdge = Main.rand.NextVector2CircularEdge(radius, radius);
Vector2 randomUnit = Main.rand.NextVector2Unit();
Vector2 randomSquare = Main.rand.NextVector2Square(-range, range);
```

### Color Lerping
```csharp
// Basic lerp
Color result = Color.Lerp(Color.Red, Color.Blue, progress);

// Multi-color lerp
Color multiColor = LumUtils.MulticolorLerp(
    (Main.GlobalTimeWrappedHourly * 2f + offset) % 1f, 
    Color.Yellow, Color.Pink, Color.HotPink, Color.Goldenrod, Color.Orange
);
```

### Extra AI Storage
```csharp
// Access extra AI slots (when npc.ai[] isn't enough)
ref float customValue = ref npc.Infernum().ExtraAI[0];
ref float anotherValue = ref npc.Infernum().ExtraAI[1];
// Can store many float values
```

---

## Implementation Examples

### Complete Impact Effect
```csharp
public static void CreateMajorImpact(Vector2 position, Color primaryColor, Color secondaryColor, float scale = 1f)
{
    // Pulse ring
    GeneralParticleHandler.SpawnParticle(new PulseRing(position, Vector2.Zero, primaryColor, 0f, 3.5f * scale, 50));
    
    // Bloom
    GeneralParticleHandler.SpawnParticle(new StrongBloom(position, Vector2.Zero, secondaryColor * 0.6f, 2f * scale, 35));
    
    // Sparks in all directions
    for (int i = 0; i < 30; i++)
    {
        Vector2 sparkVelocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(5f, 20f);
        Color sparkColor = Color.Lerp(primaryColor, secondaryColor, Main.rand.NextFloat());
        GeneralParticleHandler.SpawnParticle(new SparkParticle(position, sparkVelocity, false, 45, 2f * scale, sparkColor));
    }
    
    // Smoke ring
    for (int i = 0; i < 16; i++)
    {
        Vector2 smokeVelocity = (TwoPi * i / 16f).ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
        GeneralParticleHandler.SpawnParticle(new HeavySmokeParticle(position, smokeVelocity, primaryColor, 40, 1.5f * scale, 1f));
    }
    
    // Screen effects
    ScreenEffectSystem.SetFlashEffect(position, 1f, 30);
    Main.LocalPlayer.Infernum_Camera().CurrentScreenShakePower = 5f;
    Utilities.CreateShockwave(position, 2, 8, 120f);
}
```

### Projectile with Full VFX
```csharp
public class MyProjectile : ModProjectile, IPixelPrimitiveDrawer
{
    internal PrimitiveTrailCopy TrailDrawer;
    
    public override void AI()
    {
        // Trail particles every few frames
        if (Projectile.timeLeft % 3 == 0)
        {
            CustomParticles.GenericFlare(Projectile.Center, themeColor, 0.3f, 12);
        }
        
        // Periodic glow particles
        if (Main.rand.NextBool(4))
        {
            var glow = new GenericGlowParticle(
                Projectile.Center, 
                -Projectile.velocity * 0.1f, 
                themeColor, 
                0.25f, 
                15, 
                true
            );
            MagnumParticleHandler.SpawnParticle(glow);
        }
    }
    
    public override void OnKill(int timeLeft)
    {
        // Impact VFX
        GeneralParticleHandler.SpawnParticle(new PulseRing(Projectile.Center, Vector2.Zero, themeColor, 0f, 2.5f, 40));
        GeneralParticleHandler.SpawnParticle(new StrongBloom(Projectile.Center, Vector2.Zero, themeColor * 0.5f, 1.5f, 25));
        
        for (int i = 0; i < 20; i++)
        {
            Vector2 sparkVelocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(3f, 12f);
            GeneralParticleHandler.SpawnParticle(new SparkParticle(Projectile.Center, sparkVelocity, false, 30, 1.5f, themeColor));
        }
        
        // Sound
        SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode, Projectile.Center);
    }
    
    public float WidthFunction(float completionRatio) => Lerp(30f, 0f, completionRatio) * Projectile.scale;
    
    public Color ColorFunction(float completionRatio) => Color.Lerp(themeColor, Color.Transparent, completionRatio);
    
    public void DrawPixelPrimitives(SpriteBatch spriteBatch)
    {
        TrailDrawer ??= new PrimitiveTrailCopy(WidthFunction, ColorFunction, null, true, GameShaders.Misc["CalamityMod:ImpFlameTrail"]);
        GameShaders.Misc["CalamityMod:ImpFlameTrail"].SetShaderTexture(ModContent.Request<Texture2D>("YourMod/YourTexture"));
        TrailDrawer.DrawPixelated(Projectile.oldPos, Projectile.Size * 0.5f - Main.screenPosition, 30);
    }
}
```

---

## Summary of Key Takeaways for MagnumOpus

1. **Layer multiple particle types** for rich effects (PulseRing + StrongBloom + Sparks + Smoke)

2. **Use screen effects** for dramatic moments (Flash, Blur, Shockwave, Screen Shake)

3. **Primitive trails with shaders** create professional-looking projectile trails

4. **State machine AI** provides clean, maintainable boss behavior

5. **Progressive VFX** that scale with attack charge/phase transition intensity

6. **Color gradient particles** using `Color.Lerp()` for smooth color transitions

7. **Radial burst patterns** using `(TwoPi * i / count).ToRotationVector2()` for circular effects

8. **Death animations** should build intensity with periodic explosions before a final climax

9. **Teleport effects** need both source and destination VFX with proper fade interpolation

10. **Attack telegraphs** give players time to react while building visual tension

---

*Document generated for MagnumOpus mod development. Reference InfernumMode repository for additional implementation details.*
---

# MagnumOpus Implementation - Infernum-Style Systems

The following sections document the Infernum-style systems that have been implemented directly into MagnumOpus. These are ready-to-use utilities that follow the patterns described in this reference document.

---

## CHUNK 1: Infernum-Style Particles

**Location:** `Common/Systems/InfernumStyleParticles.cs`

### Implemented Particle Types

All particles inherit from `MagnumOpus.Common.Systems.Particles.Particle` and work with `MagnumParticleHandler.SpawnParticle()`.

#### PulseRingParticle
**Use for:** Shockwaves, explosions, phase transitions, energy bursts

```csharp
// Basic expanding ring
var ring = new PulseRingParticle(position, Vector2.Zero, color, startScale: 0f, endScale: 3.5f, lifetime: 50);
MagnumParticleHandler.SpawnParticle(ring);

// With rotation
var rotatingRing = new PulseRingParticle(position, Vector2.Zero, color, 0f, 4f, 60, rotationSpeed: 0.1f);
MagnumParticleHandler.SpawnParticle(rotatingRing);
```

#### StrongBloomParticle
**Use for:** Impact highlights, energy bursts, bright flashes

```csharp
// Bright bloom effect
var bloom = new StrongBloomParticle(position, Vector2.Zero, color * 0.6f, scale: 2f, lifetime: 35);
MagnumParticleHandler.SpawnParticle(bloom);
```

#### CloudSmokeParticle
**Use for:** Fire explosions, teleport effects, impact clouds

```csharp
// Fire cloud with color transition
var cloud = new CloudSmokeParticle(
    position, 
    Main.rand.NextVector2Circular(2f, 2f),
    startColor: Color.Orange,
    endColor: Color.DarkGray,
    lifetime: 40,
    scale: 1.5f
);
MagnumParticleHandler.SpawnParticle(cloud);
```

#### DenseSmokeParticle
**Use for:** Heavy explosions, death effects, atmospheric smoke

```csharp
// Heavy smoke with optional glow
var smoke = new DenseSmokeParticle(
    position,
    velocity: Main.rand.NextVector2Unit() * 5f,
    color: Color.Black,
    lifetime: 56,
    scale: 2.4f,
    opacity: 1f,
    rotationSpeed: 0.02f,
    glowing: true
);
MagnumParticleHandler.SpawnParticle(smoke);
```

#### DirectionalSparkParticle
**Use for:** Impacts, explosions, electrical effects

```csharp
// Explosion sparks
for (int i = 0; i < 40; i++)
{
    Vector2 velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(5f, 24f);
    var spark = new DirectionalSparkParticle(position, velocity, affectedByGravity: false, 60, scale: 2f, color);
    MagnumParticleHandler.SpawnParticle(spark);
}
```

#### ElectricArcParticle
**Use for:** Lightning effects, electrical attacks, death animations

```csharp
// Electric arcs
for (int i = 0; i < 12; i++)
{
    Vector2 velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(8f, 20f);
    var arc = new ElectricArcParticle(position, velocity, color, intensity: 0.8f, lifetime: 40);
    MagnumParticleHandler.SpawnParticle(arc);
}
```

#### SquishyLightParticle
**Use for:** Soft magical effects, energy gathering, ambient particles

```csharp
// Soft squishing light
var light = new SquishyLightParticle(position, velocity, scale: 1f, color, lifetime: 30, squish: 0f, squishSpeed: 0.1f);
MagnumParticleHandler.SpawnParticle(light);
```

#### FlareShineParticle
**Use for:** Dramatic impacts, death effects, climactic moments

```csharp
// Cross flare effect
var flare = new FlareShineParticle(
    position,
    Vector2.Zero,
    mainColor: primaryColor,
    bloomColor: secondaryColor,
    startScale: 0f,
    endScale: new Vector2(10f),
    lifetime: 60
);
MagnumParticleHandler.SpawnParticle(flare);
```

#### CritSparkParticle
**Use for:** Critical hits, powerful impacts

```csharp
// Crit spark with color transition
var critSpark = new CritSparkParticle(
    position,
    velocity,
    baseColor: Color.Gold,
    secondaryColor: Color.Red,
    scale: 2f,
    lifetime: 30,
    decayRate: 0.01f,
    bloomScale: 2f
);
MagnumParticleHandler.SpawnParticle(critSpark);
```

#### MistFireParticle
**Use for:** Atmospheric fire, mist effects, ambient particles

```csharp
// Fire mist
var mist = new MistFireParticle(
    position,
    Main.rand.NextVector2Circular(2f, 2f),
    startColor: Color.Orange,
    endColor: Color.DarkRed,
    scale: 1.2f,
    lifetime: 45
);
MagnumParticleHandler.SpawnParticle(mist);
```

---

## CHUNK 2: Screen Effect System

**Location:** `Common/Systems/ScreenEffectSystem.cs`

### MagnumScreenEffects Class

Central ModSystem for managing screen shake, flash, and blur effects.

#### Screen Shake
```csharp
// Add screen shake (accumulates with existing shake)
MagnumScreenEffects.AddScreenShake(8f);

// Set screen shake (won't decrease if already higher)
MagnumScreenEffects.SetScreenShake(12f);

// Get current shake power
float currentShake = MagnumScreenEffects.GetScreenShakePower();
```

#### Flash Effect
```csharp
// Create a flash effect at position
MagnumScreenEffects.SetFlashEffect(position, intensity: 1.5f, lifetime: 30);
```

#### Blur Effect
```csharp
// Create a blur effect emanating from position
MagnumScreenEffects.SetBlurEffect(position, intensity: 1f, lifetime: 45);
```

### ShockwaveUtility Class

Helper for creating visual shockwave effects.

```csharp
// Basic shockwave
ShockwaveUtility.CreateShockwave(position, rippleCount: 2, rippleSize: 8, rippleSpeed: 75f, color: Color.White);

// Themed shockwave with color gradient
ShockwaveUtility.CreateThemedShockwave(position, primaryColor, secondaryColor, scale: 1.5f);
```

### ExplosionUtility Class

Helper for creating layered explosion effects.

#### Fire Explosion
```csharp
// Fire explosion with smoke, sparks, and bloom
ExplosionUtility.CreateFireExplosion(position, primaryColor: Color.Orange, secondaryColor: Color.Red, scale: 1f);
```

#### Energy Explosion
```csharp
// Energy explosion with electric arcs and pulse rings
ExplosionUtility.CreateEnergyExplosion(position, primaryColor: Color.Cyan, scale: 1.2f);
```

#### Death Explosion (Boss Deaths)
```csharp
// Massive death explosion for boss kills
ExplosionUtility.CreateDeathExplosion(position, primaryColor, secondaryColor, scale: 1.5f);
```

#### Dust Explosion
```csharp
// Simple dust explosion
ExplosionUtility.CreateDustExplosion(position, DustID.Torch, count: 30, speed: 10f, scale: 1.5f);
```

---

## CHUNK 3: Boss AI Utilities

**Location:** `Common/Systems/BossAIUtilities.cs`

### BossAttackState Enum

Pre-defined attack states for state machine-based boss AI.

```csharp
public enum BossAttackState
{
    Idle = 0,
    PhaseTransition = 1,
    Despawning = 2,
    DeathAnimation = 3,
    // Attack states 100+
    Attack1 = 100, Attack2 = 101, Attack3 = 102, // ... etc
    // Special attacks 200+
    SpecialAttack1 = 200, SpecialAttack2 = 201,
    // Enraged attacks 300+
    EnragedAttack1 = 300, EnragedAttack2 = 301
}
```

### Attack Selection

```csharp
// Select random attack avoiding repeat
BossAttackState[] availableAttacks = { BossAttackState.Attack1, BossAttackState.Attack2, BossAttackState.Attack3 };
BossAttackState nextAttack = BossAIUtilities.SelectNextAttack(availableAttacks, lastAttack);

// Weighted attack selection
var weights = new Dictionary<BossAttackState, float>
{
    { BossAttackState.Attack1, 1f },
    { BossAttackState.Attack2, 2f },   // Twice as likely
    { BossAttackState.Attack3, 0.5f }  // Half as likely
};
BossAttackState weightedAttack = BossAIUtilities.SelectWeightedAttack(weights, lastAttack);
```

### Movement Behaviors

```csharp
// Smooth fly toward target
BossAIUtilities.SmoothFlyToward(npc, targetPosition, speed: 15f, turnResistance: 10f);

// Direct charge/dash
BossAIUtilities.DoCharge(npc, chargeDestination, chargeSpeed: 25f);

// Slow down
BossAIUtilities.SlowDown(npc, slowdownFactor: 0.95f);

// Hover around position with oscillation
BossAIUtilities.HoverAround(npc, hoverPosition, speed: 8f, amplitude: 50f, timer: npc.ai[0]);

// Circle around center point
float angle = 0f;
BossAIUtilities.CircleAround(npc, center, radius: 300f, angularSpeed: 0.02f, ref angle);
```

### Teleport Behaviors

```csharp
// Basic teleport with VFX
BossAIUtilities.TeleportTo(npc, targetPosition, primaryColor: Color.Purple, useParticles: true);

// Teleport above target
BossAIUtilities.TeleportAboveTarget(npc, target, heightOffset: 400f, primaryColor);

// Teleport to side of target
BossAIUtilities.TeleportToSideOfTarget(npc, target, horizontalOffset: 500f, primaryColor);

// Random teleport around target
BossAIUtilities.TeleportRandomlyAroundTarget(npc, target, minDistance: 200f, maxDistance: 600f, primaryColor);
```

### Phase Transitions

```csharp
// Call every frame during transition state
bool transitionComplete = BossAIUtilities.DoPhaseTransition(
    npc, 
    transitionTimer: (int)npc.ai[1], 
    totalTransitionTime: 120, 
    primaryColor, 
    secondaryColor
);

if (transitionComplete)
{
    // Move to next phase
    npc.ai[0] = (float)BossAttackState.Attack1;
    npc.ai[1] = 0;
}
```

### Death Animations

```csharp
// Call every frame during death state
bool deathComplete = BossAIUtilities.DoDeathAnimation(
    npc,
    deathTimer: (int)npc.ai[1],
    totalDeathTime: 180,
    primaryColor,
    secondaryColor
);

if (deathComplete)
{
    npc.life = 0;
    npc.HitEffect();
    npc.active = false;
}
```

### Utility Methods

```csharp
// Get target player
Player target = BossAIUtilities.GetTarget(npc);

// Check despawn condition
if (BossAIUtilities.ShouldDespawn(npc))
{
    npc.ai[0] = (float)BossAttackState.Despawning;
}

// Rotate toward target
BossAIUtilities.RotateTowardTarget(npc, targetPosition, rotationSpeed: 0.1f);

// Animate sprite frames
int frameCounter = 0;
BossAIUtilities.AnimateFrames(npc, frameCount: 4, framesPerAnimation: 6, ref frameCounter);

// Create warning telegraph line
BossAIUtilities.CreateTelegraphLine(npc.Center, target.Center, Color.Red, width: 2f);

// Create warning telegraph circle
BossAIUtilities.CreateTelegraphCircle(target.Center, radius: 150f, Color.Red, dustCount: 36);
```

### NPC Extension Methods

```csharp
// Safe direction to target
Vector2 direction = npc.SafeDirectionTo(targetPosition);

// Angle to target
float angle = npc.AngleTo(targetPosition);

// Distance check
bool inRange = npc.WithinRange(targetPosition, range: 500f);
```

### Float Extension Methods

```csharp
// Angle to rotation vector
Vector2 direction = angle.ToRotationVector2();

// Smooth angle lerp (handles wrap-around)
float newRotation = currentRotation.AngleLerp(targetRotation, 0.1f);
```

---

## CHUNK 4: Complete Boss AI Example

Here's a template for implementing a boss with these systems:

```csharp
using MagnumOpus.Common.Systems;

public class MyBoss : ModNPC
{
    // AI slots
    public ref float CurrentAttack => ref NPC.ai[0];
    public ref float AttackTimer => ref NPC.ai[1];
    public ref float PhaseState => ref NPC.ai[2];
    public ref float MiscCounter => ref NPC.ai[3];
    
    private BossAttackState LastAttack = BossAttackState.Idle;
    
    public override void AI()
    {
        Player target = BossAIUtilities.GetTarget(NPC);
        
        // Despawn check
        if (BossAIUtilities.ShouldDespawn(NPC))
        {
            CurrentAttack = (float)BossAttackState.Despawning;
        }
        
        switch ((BossAttackState)CurrentAttack)
        {
            case BossAttackState.Idle:
                DoIdle(target);
                break;
                
            case BossAttackState.PhaseTransition:
                DoPhaseTransition();
                break;
                
            case BossAttackState.DeathAnimation:
                DoDeath();
                break;
                
            case BossAttackState.Attack1:
                DoAttack1(target);
                break;
                
            // ... more attacks
        }
        
        AttackTimer++;
    }
    
    private void DoIdle(Player target)
    {
        BossAIUtilities.HoverAround(NPC, target.Center + new Vector2(0, -300f), 8f, 30f, AttackTimer);
        
        if (AttackTimer >= 60)
        {
            // Select next attack
            BossAttackState[] attacks = { BossAttackState.Attack1, BossAttackState.Attack2, BossAttackState.Attack3 };
            BossAttackState next = BossAIUtilities.SelectNextAttack(attacks, LastAttack);
            LastAttack = next;
            CurrentAttack = (float)next;
            AttackTimer = 0;
        }
    }
    
    private void DoPhaseTransition()
    {
        bool complete = BossAIUtilities.DoPhaseTransition(NPC, (int)AttackTimer, 120, 
            ThemedParticles.EroicaCrimson, ThemedParticles.EroicaGold);
        
        if (complete)
        {
            CurrentAttack = (float)BossAttackState.Idle;
            AttackTimer = 0;
            PhaseState++;
        }
    }
    
    private void DoDeath()
    {
        bool complete = BossAIUtilities.DoDeathAnimation(NPC, (int)AttackTimer, 180,
            ThemedParticles.EroicaCrimson, ThemedParticles.EroicaGold);
        
        if (complete)
        {
            NPC.life = 0;
            NPC.HitEffect();
            NPC.active = false;
        }
    }
    
    private void DoAttack1(Player target)
    {
        // Windup phase (0-30)
        if (AttackTimer < 30)
        {
            BossAIUtilities.SlowDown(NPC, 0.95f);
            
            // Telegraph
            if (AttackTimer % 5 == 0)
            {
                BossAIUtilities.CreateTelegraphLine(NPC.Center, target.Center, Color.Red);
            }
        }
        // Charge phase (30-60)
        else if (AttackTimer < 60)
        {
            if (AttackTimer == 30)
            {
                BossAIUtilities.DoCharge(NPC, target.Center, 25f);
                MagnumScreenEffects.AddScreenShake(5f);
                ShockwaveUtility.CreateShockwave(NPC.Center, 2, 8, 75f, Color.Red);
            }
        }
        // Recovery (60+)
        else
        {
            BossAIUtilities.SlowDown(NPC, 0.9f);
            
            if (AttackTimer >= 90)
            {
                CurrentAttack = (float)BossAttackState.Idle;
                AttackTimer = 0;
            }
        }
    }
    
    public override bool CheckDead()
    {
        if ((BossAttackState)CurrentAttack != BossAttackState.DeathAnimation)
        {
            CurrentAttack = (float)BossAttackState.DeathAnimation;
            AttackTimer = 0;
            NPC.dontTakeDamage = true;
            NPC.life = 1;
            return false;
        }
        return true;
    }
}
```

---

## CHUNK 5: VFX Effect Combos

### Standard Impact Combo
```csharp
public static void StandardImpact(Vector2 position, Color primary, Color secondary, float scale = 1f)
{
    // Pulse ring
    var ring = new PulseRingParticle(position, Vector2.Zero, primary, 0f, 2.5f * scale, 30);
    MagnumParticleHandler.SpawnParticle(ring);
    
    // Bloom
    var bloom = new StrongBloomParticle(position, Vector2.Zero, primary * 0.6f, 1.5f * scale, 20);
    MagnumParticleHandler.SpawnParticle(bloom);
    
    // Sparks
    for (int i = 0; i < 15; i++)
    {
        Vector2 velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(3f, 12f) * scale;
        var spark = new DirectionalSparkParticle(position, velocity, false, 30, 1.5f * scale, primary);
        MagnumParticleHandler.SpawnParticle(spark);
    }
    
    // Screen shake
    MagnumScreenEffects.AddScreenShake(3f * scale);
}
```

### Major Explosion Combo
```csharp
public static void MajorExplosion(Vector2 position, Color primary, Color secondary, float scale = 1f)
{
    // Multiple pulse rings
    for (int i = 0; i < 3; i++)
    {
        float ringScale = (1f + i * 0.3f) * scale;
        Color ringColor = Color.Lerp(primary, secondary, i / 3f);
        var ring = new PulseRingParticle(position, Vector2.Zero, ringColor, 0f, 3f * ringScale, 35 + i * 10);
        MagnumParticleHandler.SpawnParticle(ring);
    }
    
    // Strong bloom
    var bloom = new StrongBloomParticle(position, Vector2.Zero, primary, 3f * scale, 30);
    MagnumParticleHandler.SpawnParticle(bloom);
    
    // Electric arcs
    for (int i = 0; i < 8; i++)
    {
        Vector2 velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(10f, 25f) * scale;
        var arc = new ElectricArcParticle(position, velocity, secondary, 1f, 45);
        MagnumParticleHandler.SpawnParticle(arc);
    }
    
    // Sparks burst
    for (int i = 0; i < 30; i++)
    {
        Vector2 velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(5f, 20f) * scale;
        Color sparkColor = Main.rand.NextBool() ? primary : secondary;
        var spark = new DirectionalSparkParticle(position, velocity, Main.rand.NextBool(4), 50, 2f * scale, sparkColor);
        MagnumParticleHandler.SpawnParticle(spark);
    }
    
    // Smoke ring
    for (int i = 0; i < 16; i++)
    {
        float angle = MathHelper.TwoPi * i / 16f;
        Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 10f) * scale;
        Color smokeColor = Color.Lerp(primary, secondary, Main.rand.NextFloat());
        var smoke = new DenseSmokeParticle(position, velocity, smokeColor, 50, 2f * scale, 0.8f);
        MagnumParticleHandler.SpawnParticle(smoke);
    }
    
    // Flare shine
    var flare = new FlareShineParticle(position, Vector2.Zero, primary, secondary, 0f, new Vector2(8f * scale), 50);
    MagnumParticleHandler.SpawnParticle(flare);
    
    // Screen effects
    MagnumScreenEffects.AddScreenShake(12f * scale);
    MagnumScreenEffects.SetFlashEffect(position, 1.5f * scale, 25);
}
```

### Teleport Effect Combo
```csharp
public static void TeleportEffect(Vector2 position, Color color, float scale = 1f)
{
    // Pulse ring
    var ring = new PulseRingParticle(position, Vector2.Zero, color, 0f, 2f * scale, 25);
    MagnumParticleHandler.SpawnParticle(ring);
    
    // Bloom
    var bloom = new StrongBloomParticle(position, Vector2.Zero, color, 2f * scale, 20);
    MagnumParticleHandler.SpawnParticle(bloom);
    
    // Vertical sparks
    for (int i = 0; i < 12; i++)
    {
        Vector2 velocity = new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-10f, -5f)) * scale;
        var spark = new DirectionalSparkParticle(position, velocity, false, 35, 1.5f * scale, color);
        MagnumParticleHandler.SpawnParticle(spark);
    }
    
    // Dust burst
    for (int i = 0; i < 20; i++)
    {
        Vector2 velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(3f, 8f) * scale;
        Dust dust = Dust.NewDustPerfect(position, DustID.RainbowMk2, velocity, 0, color, 1.5f * scale);
        dust.noGravity = true;
        dust.fadeIn = 1.3f;
    }
    
    MagnumScreenEffects.AddScreenShake(3f * scale);
}
```

---

## File Summary

| File | Purpose |
|------|---------|
| `Common/Systems/InfernumStyleParticles.cs` | 10 Infernum-style particle types |
| `Common/Systems/ScreenEffectSystem.cs` | Screen shake, flash, blur, shockwave, explosion utilities |
| `Common/Systems/BossAIUtilities.cs` | Boss AI state machine helpers, movement, teleport, phase transitions |
| `Common/Systems/VFXCombos.cs` | Pre-built VFX combinations for common use cases |

All systems are ready to use and integrate with existing MagnumOpus content. Use `MagnumParticleHandler.SpawnParticle()` for particles and call utility methods directly.

---

## CHUNK 6: VFX Combos Quick Reference

**Location:** `Common/Systems/VFXCombos.cs`

Pre-built effect combinations for instant high-quality visuals.

### Impact Effects

```csharp
// Standard projectile/attack impact
VFXCombos.StandardImpact(position, primaryColor, secondaryColor, scale: 1f);

// Heavy impact for powerful attacks
VFXCombos.HeavyImpact(position, primaryColor, secondaryColor, scale: 1f);

// Critical hit impact with flare
VFXCombos.CriticalImpact(position, primaryColor, secondaryColor, scale: 1f);
```

### Explosions

```csharp
// Fire explosion with smoke
VFXCombos.FireExplosion(position, flameColor, smokeColor, scale: 1f);

// Energy/magical explosion
VFXCombos.EnergyExplosion(position, energyColor, scale: 1f);

// Major explosion (boss attacks, powerful abilities)
VFXCombos.MajorExplosion(position, primaryColor, secondaryColor, scale: 1f);

// Boss death explosion (maximum visual impact)
VFXCombos.BossDeathExplosion(position, primaryColor, secondaryColor, scale: 1f);
```

### Teleportation

```csharp
// Single teleport burst (call at departure OR arrival)
VFXCombos.TeleportBurst(position, color, scale: 1f);

// Complete teleport effect (handles both departure and arrival)
VFXCombos.DramaticTeleport(departurePos, arrivalPos, color, scale: 1f);
```

### Attack Windup/Release

```csharp
// During windup - call every frame
// chargeProgress should go from 0.0 to 1.0
VFXCombos.ChargePulse(position, color, chargeProgress, scale: 1f);

// When attack releases - call once
VFXCombos.AttackRelease(position, primaryColor, secondaryColor, scale: 1f);
```

### Trail Effects (call every 3-5 frames)

```csharp
// Basic glowing trail
VFXCombos.ProjectileTrail(Projectile.Center, Projectile.velocity, color, scale: 1f);

// Fire trail with smoke and embers
VFXCombos.FireTrail(Projectile.Center, Projectile.velocity, flameColor, scale: 1f);

// Electric trail with arcs
VFXCombos.ElectricTrail(Projectile.Center, Projectile.velocity, electricColor, scale: 1f);
```

### Ambient Effects (call every frame)

```csharp
// Random sparkles in radius
VFXCombos.AmbientSparkles(position, radius: 50f, color, scale: 1f);

// Orbiting particles around center
VFXCombos.OrbitingParticles(center, radius: 40f, primaryColor, secondaryColor, scale: 1f);
```

### Shockwave Effects

```csharp
// Ground-based shockwave (landing, stomps)
VFXCombos.GroundShockwave(position, color, scale: 1f);

// Aerial shockwave (mid-air explosions)
VFXCombos.AerialShockwave(position, color, scale: 1f);
```

---

## Complete Usage Example: Boss Attack with VFXCombos

```csharp
public class ExampleBoss : ModNPC
{
    private int attackTimer = 0;
    private const int WINDUP_TIME = 60;
    private const int CHARGE_TIME = 30;
    
    public override void AI()
    {
        Player target = BossAIUtilities.GetTarget(NPC);
        
        // Windup phase
        if (attackTimer < WINDUP_TIME)
        {
            float chargeProgress = (float)attackTimer / WINDUP_TIME;
            
            // Visual windup - converging particles
            VFXCombos.ChargePulse(NPC.Center, Color.Red, chargeProgress, scale: 1.5f);
            
            // Telegraph line to target
            if (attackTimer % 5 == 0)
                BossAIUtilities.CreateTelegraphLine(NPC.Center, target.Center, Color.Red * 0.5f);
        }
        // Attack release
        else if (attackTimer == WINDUP_TIME)
        {
            // Fire release effect
            VFXCombos.AttackRelease(NPC.Center, Color.Red, Color.Orange, scale: 1.5f);
            
            // Start charge toward player
            BossAIUtilities.DoCharge(NPC, target.Center, 25f);
        }
        // During charge
        else if (attackTimer < WINDUP_TIME + CHARGE_TIME)
        {
            // Trail effect every 3 frames
            if (attackTimer % 3 == 0)
                VFXCombos.FireTrail(NPC.Center, NPC.velocity, Color.Orange);
        }
        // Attack end
        else
        {
            // Heavy impact at charge end
            VFXCombos.HeavyImpact(NPC.Center, Color.Red, Color.Orange, scale: 1.2f);
            
            // Reset attack
            attackTimer = -1;
        }
        
        attackTimer++;
    }
    
    public override void OnKill()
    {
        // Death explosion
        VFXCombos.BossDeathExplosion(NPC.Center, Color.Red, Color.Orange, scale: 2f);
    }
}
```

---

## Quick Selection Guide

| Situation | Method to Use |
|-----------|---------------|
| Projectile hits enemy | `StandardImpact` or `HeavyImpact` |
| Critical hit | `CriticalImpact` |
| Explosion spell | `EnergyExplosion` or `FireExplosion` |
| Boss phase transition | `MajorExplosion` |
| Boss death | `BossDeathExplosion` |
| Boss teleports | `DramaticTeleport` or `TeleportBurst` |
| Attack charging | `ChargePulse` (every frame) |
| Attack fires | `AttackRelease` |
| Projectile flying | `ProjectileTrail`, `FireTrail`, or `ElectricTrail` (every 3-5 frames) |
| Ambient aura | `AmbientSparkles` or `OrbitingParticles` (every frame) |
| Ground slam | `GroundShockwave` |
| Mid-air blast | `AerialShockwave` |
