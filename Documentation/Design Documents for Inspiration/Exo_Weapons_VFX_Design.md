# Exo-Tier Endgame Weapons - VFX Design Document

## 🚨🚨🚨 CRITICAL: READ TRUE_VFX_STANDARDS.md FIRST 🚨🚨🚨

> **Before implementing ANY effects from this document, read [../Guides/TRUE_VFX_STANDARDS.md](../Guides/TRUE_VFX_STANDARDS.md).**
>
> The Ark of the Cosmos patterns in this document are EXACTLY what we should be doing:
> - **Curved, flowing trails** with sine-wave movement
> - **Dense particle trails** littering the air with sparkles
> - **Color oscillation** through gradients
> - **Constellation connections** linking projectiles
>
> **Gold Standard = Calamity Mod Source Code.** Study Exoblade, Ark of the Cosmos, Galaxia, Photoviscerator, The Oracle, and Scarlet Devil.

---

## Overview

Calamity's endgame Exo-tier weapons feature some of the most sophisticated visual effects in modded Terraria:
- **Ark of the Cosmos**: Scissor-blade melee with constellation chains
- **Exoblade**: Power sword with dash attack and homing beams
- **Photoviscerator**: Flamethrower with metaball particles
- **Exo Color Palette**: Cycling cyan/lime/yellow/orange gradient

---

## The Exo Color Palette

### Core Colors
```csharp
// CalamityUtils.ExoPalette - signature endgame aesthetic
public static readonly Color[] ExoPalette = new Color[]
{
    Color.Cyan,        // Start
    Color.Lime,        // Early mid
    Color.GreenYellow, // Mid
    Color.Goldenrod,   // Late mid
    Color.Orange       // End
};

// Multicolor lerp for smooth cycling
public static Color MulticolorLerp(float progress, params Color[] colors)
{
    progress = MathHelper.Clamp(progress, 0f, 0.999f);
    int colorCount = colors.Length;
    float scaledProgress = progress * (colorCount - 1);
    int index1 = (int)scaledProgress;
    int index2 = Math.Min(index1 + 1, colorCount - 1);
    float localProgress = scaledProgress - index1;
    
    return Color.Lerp(colors[index1], colors[index2], localProgress);
}
```

### Dynamic Color Usage
```csharp
// Get time-based cycling color
Color GetExoColor(float offset = 0f)
{
    float hue = (Main.GlobalTimeWrappedHourly * 0.5f + offset) % 1f;
    return CalamityUtils.MulticolorLerp(hue, CalamityUtils.ExoPalette);
}

// Per-projectile color offset for variety
Color mainColor = MulticolorLerp((Main.GlobalTimeWrappedHourly * 0.5f + Projectile.whoAmI * 0.12f) % 1, 
    Color.Cyan, Color.Lime, Color.GreenYellow, Color.Goldenrod, Color.Orange);
Color secondaryColor = MulticolorLerp((Main.GlobalTimeWrappedHourly * 0.5f + Projectile.whoAmI * 0.12f + 0.2f) % 1,
    Color.Cyan, Color.Lime, Color.GreenYellow, Color.Goldenrod, Color.Orange);
```

---

## Ark of the Cosmos

### Weapon Overview
- **Type**: True melee scissor-blade
- **Combo System**: Multiple swing types with throw and snap mechanic
- **Special**: Constellation chain attack between targets

### Swing Animation System
```csharp
// Piecewise animation curves
public CurveSegment startup = new CurveSegment(EasingType.SineBump, 0f, 0f, 0.25f);
public CurveSegment swing = new CurveSegment(EasingType.SineOut, 0.1f, 0.25f, 0.75f);

internal float SwingRatio() => PiecewiseAnimation(SwingCompletion, new CurveSegment[] { startup, swing });

// "Swirl Swing" - full 360+ rotation attack
if (SwirlSwing)
{
    float startRot = (MathHelper.Pi - MathHelper.PiOver4) * SwingDirection;
    float endRot = -(MathHelper.TwoPi + MathHelper.PiOver4 * 1.5f) * SwingDirection;
    Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.Lerp(startRot, endRot, SwirlRatio());
}
```

### Circular Smear VFX
```csharp
void DoParticleEffects(bool swirlSwing)
{
    if (swirlSwing)
    {
        // Dynamic scale based on swing progression
        Projectile.scale = 1.6f + ((float)Math.Sin(SwirlRatio() * MathHelper.Pi) * 1f);
        
        // Smokey circular smear
        Color currentColor = Color.Chocolate * (MathHelper.Clamp((float)Math.Sin((SwirlRatio() - 0.2f) * MathHelper.Pi), 0f, 1f) * 0.8f);
        
        if (smear == null)
        {
            smear = new CircularSmearSmokeyVFX(Owner.Center, currentColor, Projectile.rotation, Projectile.scale * 2.4f);
            GeneralParticleHandler.SpawnParticle(smear);
        }
        else
        {
            // Update smear position and rotation
            smear.Rotation = Projectile.rotation + MathHelper.PiOver4 + (Owner.direction < 0 ? MathHelper.PiOver4 * 4f : 0f);
            smear.Time = 0;
            smear.Position = Owner.Center;
            smear.Scale = MathHelper.Lerp(2.6f, 3.5f, (Projectile.scale - 1.6f) / 1f);
            smear.Color = currentColor;
        }
    }
}
```

### Constellation Chain Attack
```csharp
// Spawn constellation link between blade and cursor
public class ArkoftheCosmosConstellation : ModProjectile
{
    public List<Particle> Particles;
    const float ConstellationSwapTime = 15;
    
    public override void AI()
    {
        if (Timer % ConstellationSwapTime == 0)
        {
            Particles.Clear();
            
            // Generate random constellation pattern
            float constellationColorHue = Main.rand.NextFloat();
            Color constellationColor = Main.hslToRgb(constellationColorHue, 1, 0.8f);
            
            Vector2 previousStar = AnchorStart;
            
            // Create star points along line
            for (int i = 0; i < starCount; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(50f, 50f);
                Vector2 starPos = Vector2.Lerp(AnchorStart, AnchorEnd, (float)i / starCount) + offset;
                
                // Line between stars
                Particle line = new BloomLineVFX(previousStar, starPos - previousStar, 
                    0.05f, constellationColor);
                BootlegSpawnParticle(line);
                
                // Star point
                Particle star = new GenericSparkle(starPos, Vector2.Zero, 
                    Color.White, constellationColor, 0.5f, 15, 0f, 2f);
                BootlegSpawnParticle(star);
                
                previousStar = starPos;
            }
        }
        
        // Color cycling over time
        foreach (var particle in Particles)
        {
            particle.Color = Main.hslToRgb(Main.rgbToHsl(particle.Color).X + 0.02f, 
                Main.rgbToHsl(particle.Color).Y, Main.rgbToHsl(particle.Color).Z);
        }
    }
    
    public override bool PreDraw(ref Color lightColor)
    {
        Main.spriteBatch.EnterShaderRegion(BlendState.Additive);
        foreach (Particle particle in Particles)
            particle.CustomDraw(Main.spriteBatch);
        Main.spriteBatch.ExitShaderRegion();
        return false;
    }
}
```

### CritSpark Particles on Swing
```csharp
// Sparkles trail the blade during swings
if (Main.rand.NextBool())
{
    float maxDistance = Projectile.scale * 78f;
    Vector2 distance = Main.rand.NextVector2Circular(maxDistance, maxDistance);
    Vector2 angularVelocity = Utils.SafeNormalize(distance.RotatedBy(MathHelper.PiOver2 * Owner.direction), Vector2.Zero) * 2 * (1f + distance.Length() / 15f);
    
    Particle glitter = new CritSpark(Projectile.Center + distance, Owner.velocity + angularVelocity, 
        Color.White, glitterColor, 1f + 1 * (distance.Length() / maxDistance), 10, 0.05f, 3f);
    GeneralParticleHandler.SpawnParticle(glitter);
}
```

### Throw and Snap Mechanic
```csharp
// Scissor throw with snap window
public CurveSegment shoot = new CurveSegment(EasingType.PolyIn, 0f, 1f, -0.2f, 3);
public CurveSegment remain = new CurveSegment(EasingType.Linear, SnapWindowStart, 0.8f, 0f);
public CurveSegment retract = new CurveSegment(EasingType.SineIn, SnapWindowEnd, 1f, -1f);

// Snap attack when clicking at right time
if (!OwnerCanShoot && Combo == 2 && ThrowCompletion >= (SnapWindowStart - 0.1f) && ThrowCompletion < SnapWindowEnd)
{
    Particle snapSpark = new GenericSparkle(Projectile.Center, Owner.velocity - Utils.SafeNormalize(Projectile.velocity, Vector2.Zero), 
        Color.White, Color.OrangeRed, Main.rand.NextFloat(1f, 2f), 10 + Main.rand.Next(10), 0.1f, 3f);
    GeneralParticleHandler.SpawnParticle(snapSpark);
    
    // Screen shake on snap
    Main.LocalPlayer.Calamity().GeneralScreenShakePower = 3;
}
```

### Heavy Smoke Trail
```csharp
// Colored smoke during special attacks
for (float i = 0f; i <= 1; i += 0.5f)
{
    Vector2 smokepos = Projectile.Center + (Projectile.rotation.ToRotationVector2() * (30 + 50 * i) * Projectile.scale);
    Vector2 smokespeed = Projectile.rotation.ToRotationVector2().RotatedBy(-MathHelper.PiOver2 * Owner.direction) * 20f * scaleFactor + Owner.velocity;
    
    Particle smoke = new HeavySmokeParticle(smokepos, smokespeed, 
        Color.Lerp(Color.DodgerBlue, Color.MediumVioletRed, i), 
        6 + Main.rand.Next(5), scaleFactor * Main.rand.NextFloat(2.8f, 3.1f), 
        Opacity + Main.rand.NextFloat(0f, 0.2f), 0f, false, 0, true);
    GeneralParticleHandler.SpawnParticle(smoke);
}
```

---

## Exoblade

### Weapon Overview
- **Type**: Large power sword with projectile mode
- **Combo System**: Swing + Dash + Beam attacks
- **Special**: Player dash attack on hit, homing beams during swing

### Swing State Machine
```csharp
public enum SwingState
{
    Swinging,   // Normal melee attack
    BonkDash    // Player dashes forward with blade
}

// Piecewise animation for swing progression
public CurveSegment SlowStart = new(SineBumpEasing, 0f, -0.8f, 0.1f);
public CurveSegment SwingFast = new(PolyInEasing, 0.27f, -0.7f, 1.6f, 4);
public CurveSegment EndSwing = new(PolyOutEasing, 0.85f, 0.9f, 0.1f, 2);

public float SwingAngleShiftAtProgress(float progress) => 
    MaxSwingAngle * PiecewiseAnimation(progress, new CurveSegment[] { SlowStart, SwingFast, EndSwing });
```

### Dynamic Lighting During Swing
```csharp
void DoBehavior_Swinging()
{
    // Color shifts from green-yellow to pink based on progression
    Lighting.AddLight(Owner.MountedCenter + SwordDirection * 100, 
        Color.Lerp(Color.GreenYellow, Color.DeepPink, (float)Math.Pow(Progression, 3)).ToVector3() * 1.6f * 
        (float)Math.Sin(Progression * MathHelper.Pi));
}
```

### Homing Beams During Swing
```csharp
// Create homing beams at intervals during swing
int beamShootStart = (int)(SwingTime * 0.6f);
int beamShootPeriod = (int)(SwingTime * 0.4f);
int beamShootEnd = beamShootStart + beamShootPeriod;
beamShootPeriod /= (Exoblade.BeamsPerSwing - 1);

if (Timer >= beamShootStart && Timer < beamShootEnd && (Timer - beamShootStart) % beamShootPeriod == 0)
{
    // Spawn homing Exobeam projectile
    Projectile.NewProjectile(source, Owner.MountedCenter + SwordDirection * 50f, 
        SwordDirection * 10f, ModContent.ProjectileType<Exobeam>(), beamDamage, 0f);
}
```

### Dash Attack (BonkDash)
```csharp
void DoBehavior_BonkDash()
{
    // Player velocity carried by sword
    Owner.velocity = SwordDirection * dashSpeed;
    
    // Dust trail during dash
    Color dustColor = Main.hslToRgb(Main.rand.NextFloat(), 1f, 0.9f);
    Dust must = Dust.NewDustPerfect(Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale, 
        267, SwordDirection.RotatedBy(-MathHelper.PiOver2 * Direction) * 2.6f, 0, dustColor);
    must.scale = 0.3f;
    must.fadeIn = Main.rand.NextFloat() * 1.2f;
    must.noGravity = true;
    
    // Streak particles during dash
    if (Main.rand.NextBool(6) && LungeProgression < 0.8f)
    {
        Vector2 particleSpeed = SwordDirection * -1 * Main.rand.NextFloat(6f, 10f);
        Particle energyLeak = new SquishyLightParticle(Owner.MountedCenter + Main.rand.NextVector2Circular(20f, 20f), 
            particleSpeed, Main.rand.NextFloat(0.3f, 0.6f), Color.GreenYellow, 30, 3.4f, 4.5f, hueShift: 0.02f);
        GeneralParticleHandler.SpawnParticle(energyLeak);
    }
}
```

### Primitive Trail Rendering
```csharp
void DrawPierceTrail()
{
    Main.spriteBatch.EnterShaderRegion();
    
    Color mainColor = MulticolorLerp((Main.GlobalTimeWrappedHourly * 2f) % 1, 
        Color.Cyan, Color.Lime, Color.GreenYellow, Color.Goldenrod, Color.Orange);
    Color secondaryColor = MulticolorLerp((Main.GlobalTimeWrappedHourly * 2f + 0.2f) % 1, 
        Color.Cyan, Color.Lime, Color.GreenYellow, Color.Goldenrod, Color.Orange);
    
    // Apply shader
    GameShaders.Misc["CalamityMod:ExobladePierce"].SetShaderTexture(TrailTex);
    GameShaders.Misc["CalamityMod:ExobladePierce"].UseImage2("Images/Extra_189");
    GameShaders.Misc["CalamityMod:ExobladePierce"].UseColor(mainColor);
    GameShaders.Misc["CalamityMod:ExobladePierce"].UseSecondaryColor(secondaryColor);
    GameShaders.Misc["CalamityMod:ExobladePierce"].Apply();
    
    // Render trail using oldPos array
    int numPointsRendered = 30;
    int numPointsProvided = 60;
    var positionsToUse = Projectile.oldPos.Take(numPointsProvided).ToArray();
    PrimitiveRenderer.RenderTrail(positionsToUse, new(PierceWidthFunction, PierceColorFunction, 
        (_) => trailOffset, shader: GameShaders.Misc["CalamityMod:ExobladePierce"]), numPointsRendered);
    
    Main.spriteBatch.ExitShaderRegion();
}
```

### Blade Drawing with Swing Shader
```csharp
void DrawBlade()
{
    if (State == SwingState.Swinging)
    {
        // Use swing distortion shader
        Effect swingFX = Filters.Scene["CalamityMod:SwingSprite"].GetShader().Shader;
        swingFX.Parameters["rotation"].SetValue(SwingAngleShift + MathHelper.PiOver4 + (Direction == -1 ? MathHelper.Pi : 0f));
        swingFX.Parameters["pommelToOriginPercent"].SetValue(0.05f);
        swingFX.Parameters["color"].SetValue(Color.White.ToVector4());
        
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, 
            DepthStencilState.None, RasterizerState.CullNone, swingFX, Main.GameViewMatrix.TransformationMatrix);
    }
    
    // Energy glow offset effect
    float energyPower = (float)Math.Pow(Progression, 2f) * 0.8f;
    for (int i = 0; i < 4; i++)
    {
        Vector2 drawOffset = (MathHelper.TwoPi * i / 4f + BaseRotation).ToRotationVector2() * energyPower * Projectile.scale * 7f;
        Main.spriteBatch.Draw(texture, drawPosition + drawOffset, null, 
            Color.Lerp(Color.Goldenrod, Color.MediumTurquoise, Progression) with { A = 0 } * 0.16f, 
            rotation, origin, Projectile.scale, direction, 0);
    }
}
```

---

## Exobeam (Homing Projectile)

### Trail Configuration
```csharp
public float TrailWidth(float completionRatio)
{
    // Tapered trail width
    float width = Utils.GetLerpValue(1f, 0.4f, completionRatio, true) * 
        (float)Math.Sin(Math.Acos(1 - Utils.GetLerpValue(0f, 0.15f, completionRatio, true)));
    width *= Utils.GetLerpValue(0f, 0.1f, Projectile.timeLeft / 600f, true);
    return width * MaxWidth;
}

public Color TrailColor(float completionRatio)
{
    Color baseColor = Color.Lerp(Color.Cyan, new Color(0, 0, 255), completionRatio);
    return baseColor;
}
```

### Multi-Layer Bloom Drawing
```csharp
public override bool PreDraw(ref Color lightColor)
{
    // Get cycling colors
    Color mainColor = MulticolorLerp((Main.GlobalTimeWrappedHourly * 0.5f + Projectile.whoAmI * 0.12f) % 1, 
        Color.Cyan, Color.Lime, Color.GreenYellow, Color.Goldenrod, Color.Orange);
    
    // Draw bloom under trail
    Main.EntitySpriteDraw(bloomTex, Projectile.oldPos[2] + Projectile.Size / 2f - Main.screenPosition, null, 
        (mainColor * 0.1f) with { A = 0 }, 0, bloomTex.Size() / 2f, 1.3f * Projectile.scale, 0, 0);
    Main.EntitySpriteDraw(bloomTex, Projectile.oldPos[1] + Projectile.Size / 2f - Main.screenPosition, null, 
        (mainColor * 0.5f) with { A = 0 }, 0, bloomTex.Size() / 2f, 0.34f * Projectile.scale, 0, 0);
    
    // Shader trail
    Main.spriteBatch.EnterShaderRegion();
    // ... trail rendering
    Main.spriteBatch.ExitShaderRegion();
    
    // Bloom above trail
    Main.EntitySpriteDraw(bloomTex, Projectile.Center - Main.screenPosition, null,
        (mainColor * 0.4f) with { A = 0 }, 0, bloomTex.Size() / 2f, 0.8f, 0, 0);
}
```

---

## Photoviscerator (Exo Flamethrower)

### Weapon Overview
- **Type**: Flamethrower with two attack modes
- **Left Click**: Continuous flame stream with waving light projectiles
- **Right Click**: Explosive flare cluster bombs

### Metaball Particle System
```csharp
void AI()
{
    // PhotoMetaball creates smooth, blobby flame effect
    PhotoMetaball.SpawnParticle(Projectile.Center, 54);
    PhotoMetaball2.SpawnParticle(Projectile.Center, 50);
    
    // Spark color cycling
    sparkColor = Main.rand.Next(4) switch
    {
        0 => Color.Red,
        1 => Color.MediumTurquoise,
        2 => Color.Orange,
        _ => Color.LawnGreen,
    };
}
```

### Left Click - ExoFire Stream
```csharp
void ExoFireAI()
{
    // Multiple metaball layers for depth
    if (PhotoTimer == 0)
        PhotoMetaball3.SpawnParticle(Projectile.Center + Owner.velocity, 42 - Time * 0.165f);
    if (PhotoTimer == 1)
        PhotoMetaball3.SpawnParticle(Projectile.Center + Owner.velocity, 37 - Time * 0.088f + 20);
    
    PhotoMetaball4.SpawnParticle(Projectile.Center + Owner.velocity, 37 - Time * 0.088f);
    
    // Occasional rising sparks
    if (Main.rand.NextBool(35))
    {
        Dust dust = Dust.NewDustPerfect(Projectile.Center, 263, new Vector2(0, -5).RotatedByRandom(0.05f) * Main.rand.NextFloat(0.3f, 1.6f));
        dust.noGravity = true;
        dust.scale = Main.rand.NextFloat(0.3f, 1f);
        dust.color = sparkColor;
    }
}
```

### ExoLight - Waving Light Projectiles
```csharp
// Left click spawns waving light orbs that orbit target
public override void AI()
{
    // Waving motion perpendicular to travel direction
    Projectile.Center += (Vector2.UnitY * MathF.Sin(Time / 70f * MathHelper.TwoPi) * 75f * YDirection)
        .RotatedBy(Projectile.velocity.ToRotation());
    
    // Later, orbits around target
    if (Time >= 60f && Time < 120f)
    {
        float radius = MaxRadius * Utils.GetLerpValue(60f, 75f, Time, true) * Utils.GetLerpValue(120f, 105f, Time, true);
        radius *= 1f + MathF.Cos(Main.GlobalTimeWrappedHourly / 24f) * 0.25f;
        Projectile.Center = Destination + ((Time - 60) / 60f * MathHelper.ToRadians(720f) + 
            (YDirection == -1).ToInt() * MathHelper.Pi).ToRotationVector2() * radius;
    }
}
```

### ExoLight Kill Explosion
```csharp
public override void OnKill(int timeLeft)
{
    float scaleBonus = Time >= 120f ? Main.rand.NextFloat(3.4f, 4.2f) : Main.rand.NextFloat(0.8f, 1.6f);
    SoundEngine.PlaySound(ExplosionSound, Projectile.Center);
    
    // Radial particle burst
    float numberOfDusts = Time >= 120f ? 30 : 20;
    float rotFactor = 360f / numberOfDusts;
    for (int i = 0; i < numberOfDusts; i++)
    {
        sparkColor = Main.rand.Next(4) switch
        {
            0 => Color.Red,
            1 => Color.MediumTurquoise,
            2 => Color.Orange,
            _ => Color.LawnGreen,
        };
        
        float rot = MathHelper.ToRadians(i * rotFactor);
        Vector2 offset = (Vector2.UnitX * Main.rand.NextFloat(0.2f, 3.1f)).RotatedBy(rot * Main.rand.NextFloat(1.1f, 9.1f));
        Vector2 velOffset = (Vector2.UnitX * Main.rand.NextFloat(0.2f, 3.1f)).RotatedBy(rot * Main.rand.NextFloat(1.1f, 9.1f));
        
        SquishyLightParticle exoEnergy = new(Projectile.Center + offset, 
            velOffset * (Main.rand.NextFloat(0.5f, 3.5f) + scaleBonus * 0.65f), 
            Time >= 120f ? 0.7f : 0.5f, sparkColor, Time >= 120f ? 50 : 35);
        GeneralParticleHandler.SpawnParticle(exoEnergy);
    }
}
```

### Right Click - ExoFlareCluster
```csharp
// Invisible bomb that spawns particles on hit
public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
{
    for (int i = 0; i < 10; i++)
    {
        sparkColor = Main.rand.Next(4) switch
        {
            0 => Color.Red,
            1 => Color.MediumTurquoise,
            2 => Color.Orange,
            _ => Color.LawnGreen,
        };
        
        float rot = MathHelper.ToRadians(i * 36);
        Vector2 offset = (Vector2.UnitX * Main.rand.NextFloat(0.2f, 3.1f)).RotatedBy(rot * Main.rand.NextFloat(1.1f, 9.1f));
        Vector2 velOffset = (Vector2.UnitX * Main.rand.NextFloat(0.2f, 3.1f)).RotatedBy(rot * Main.rand.NextFloat(1.1f, 9.1f));
        
        SquishyLightParticle exoEnergy = new(Projectile.Center + offset, velOffset * Main.rand.NextFloat(0.5f, 2.5f), 
            0.5f, sparkColor, 35);
        GeneralParticleHandler.SpawnParticle(exoEnergy);
    }
}
```

### Holdout Energy Particles
```csharp
void PhotovisceratorHoldoutAI()
{
    // Energy particles while holding weapon
    Color energyColor = Main.rand.Next(4) switch
    {
        0 => Color.Red,
        1 => Color.MediumTurquoise,
        2 => Color.Orange,
        _ => Color.LawnGreen,
    };
    
    SquishyLightParticle exoEnergy = new(flamePosition - verticalOffset * 26f, 
        flameAngle * Main.rand.NextFloat(0.8f, 3.6f), 0.25f, energyColor, 20);
    GeneralParticleHandler.SpawnParticle(exoEnergy);
}
```

---

## Key Particle Types Used

| Particle | Description | Usage |
|----------|-------------|-------|
| `CircularSmearSmokeyVFX` | Smoky swing arc | Melee smears |
| `TrientCircularSmear` | Clean swing arc | Fast swings |
| `CritSpark` | Glittering spark | Hit effects, trails |
| `GenericSparkle` | Star-like sparkle | Constellation, impacts |
| `HeavySmokeParticle` | Thick smoke | Swing trails, explosions |
| `SquishyLightParticle` | Soft glowing orb | Energy effects |
| `SparkParticle` | Sharp spark | Impacts, explosions |
| `BloomLineVFX` | Glowing line | Constellation connections |
| `PulseRing` | Expanding ring | Snap attack feedback |
| `DirectionalPulseRing` | Directed pulse | Attack telegraphs |
| `LineVFX` | Sharp line | Slash finishers |

---

## Implementation Tips for MagnumOpus

### Adapting Exo Weapon Concepts

1. **Color Palette Cycling**: Replace ExoPalette with your theme's gradient
2. **Smear Effects**: CircularSmear for dramatic melee swings
3. **Constellation Chains**: Connecting lines between targets/points
4. **Homing Sub-Projectiles**: Beams spawned during main attack
5. **Dash Attack Mode**: Player movement tied to weapon

### VFX Layering Technique
```csharp
// Layer order for maximum impact:
// 1. Background bloom (large, soft, low opacity)
// 2. Shader trail (primitive renderer)
// 3. Core sprite
// 4. Foreground bloom (small, bright)
// 5. Particle effects

public override bool PreDraw(ref Color lightColor)
{
    DrawBackgroundBloom();      // Large soft glow
    DrawShaderTrail();          // Primitive trail
    DrawMainSprite();           // The weapon/projectile
    DrawForegroundBloom();      // Bright highlights
    DrawParticles();            // Sparks, smoke, etc.
    return false;
}
```

### Key Takeaways
1. **Piecewise Animation**: CurveSegment system for complex swing arcs
2. **Shader Trails**: PrimitiveRenderer with custom shaders
3. **Multi-Layer Bloom**: Multiple bloom draws at different scales
4. **State Machines**: SwingState enum for combo systems
5. **Metaball Particles**: Soft, blobby flame effects
6. **Color Cycling**: Time-based hue shifts for dynamic visuals
