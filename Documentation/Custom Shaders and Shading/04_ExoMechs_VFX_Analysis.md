# ExoMechs VFX Analysis

> **Comprehensive breakdown of VFX systems for Draedon's ExoMechs: Ares, Apollo, Artemis, and Hades (Thanatos).**
> 
> These bosses represent the pinnacle of FargosSoulsDLC's visual effects engineering.

---

## ExoMechs Overview

| Boss | Role | Key VFX Systems |
|------|------|-----------------|
| **Ares** | Tank/Heavy | Energy katanas, tesla orbs, pulse cannons, gauss nukes, portals |
| **Apollo** | Aerial Assault | Plasma fireballs, missile barrages, thruster trails |
| **Artemis** | Precision | Laser shots, disintegration ray, targeting lasers |
| **Hades/Thanatos** | Worm | Segment trails, super laserbeam, electric death |

---

## Ares VFX Systems

### Energy Katana Slashes

**File:** `Content/Calamity/Bosses/ExoMechs/Ares/AresEnergyKatanaSlash.cs`

```csharp
public class AresEnergyKatanaSlash : ModProjectile, IPixelatedPrimitiveRenderer
{
    // Trail positions for slash arc
    public Vector2[] SlashPositions;
    
    public override void AI()
    {
        // Build slash arc trail
        float arcProgress = (MaxTime - Projectile.timeLeft) / MaxTime;
        float currentAngle = StartAngle + (EndAngle - StartAngle) * EasingFunctions.QuadraticEaseOut(arcProgress);
        
        Vector2 slashOffset = currentAngle.ToRotationVector2() * SlashRadius;
        Projectile.Center = OwnerCenter + slashOffset;
        
        // Update trail positions
        for (int i = SlashPositions.Length - 1; i > 0; i--)
            SlashPositions[i] = SlashPositions[i - 1];
        SlashPositions[0] = Projectile.Center;
        
        // Spawn edge particles
        SpawnSlashParticles();
    }
    
    private void SpawnSlashParticles()
    {
        // Edge sparks
        for (int i = 0; i < 2; i++)
        {
            Vector2 sparkVelocity = Projectile.velocity.RotatedByRandom(0.5f) * Main.rand.NextFloat(0.3f, 0.8f);
            Color sparkColor = Color.Lerp(Color.Cyan, Color.White, Main.rand.NextFloat());
            GeneralParticleHandler.SpawnParticle(new GlowySquareParticle(
                Projectile.Center, sparkVelocity, sparkColor, 0.6f, 15, true));
        }
        
        // Glow bloom at tip
        Texture2D bloom = MiscTexturesRegistry.BloomCircleSmall.Value;
        Vector2 drawPos = Projectile.Center - Main.screenPosition;
        Main.spriteBatch.Draw(bloom, drawPos, null, 
            Color.Cyan with { A = 0 } * 0.5f, 0f, bloom.Size() * 0.5f, 1.5f, 0, 0f);
    }
    
    // IPixelatedPrimitiveRenderer implementation
    public float SlashWidthFunction(float completionRatio) =>
        60f * (1f - completionRatio) * Projectile.Opacity;
        
    public Color SlashColorFunction(float completionRatio) =>
        Color.Lerp(Color.Cyan, Color.DeepSkyBlue, completionRatio) * Projectile.Opacity;
    
    public void RenderPixelatedPrimitives(SpriteBatch spriteBatch)
    {
        ManagedShader shader = ShaderManager.GetShader("FargowiltasCrossmod.AresEnergyKatanaShader");
        shader.TrySetParameter("opacity", Projectile.Opacity);
        shader.TrySetParameter("verticalFlip", IsLeftArm ? 1f : 0f);
        
        PrimitiveSettings settings = new(SlashWidthFunction, SlashColorFunction, 
            _ => Projectile.Size * 0.5f, Pixelate: true, Shader: shader);
        PrimitiveRenderer.RenderTrail(SlashPositions, settings, 40);
    }
}
```

---

### Tesla Orb Explosion

**File:** `Content/Calamity/Bosses/ExoMechs/Ares/AresTeslaOrb.cs`

```csharp
public override void OnKill(int timeLeft)
{
    // Expanding ring explosion
    ManagedShader explosionShader = ShaderManager.GetShader("FargowiltasCrossmod.TeslaExplosionShader");
    
    // Spawn the visual effect projectile
    Projectile.NewProjectile(source, Projectile.Center, Vector2.Zero,
        ModContent.ProjectileType<TeslaExplosionVisual>(), 0, 0);
    
    // Electric arc particles
    for (int i = 0; i < 24; i++)
    {
        float angle = MathHelper.TwoPi * i / 24f;
        Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(8f, 16f);
        
        // Electric spark
        GeneralParticleHandler.SpawnParticle(new LineParticle(
            Projectile.Center, velocity, Color.Cyan, 0.4f, 20,
            fadeIn: false, endColor: Color.White));
    }
    
    // Central flash
    GeneralParticleHandler.SpawnParticle(new StrongBloom(
        Projectile.Center, Vector2.Zero, Color.Cyan, 2f, 15));
    
    // Bloom layers
    Texture2D bloom = MiscTexturesRegistry.BloomCircleSmall.Value;
    for (int layer = 0; layer < 3; layer++)
    {
        float scale = 2f + layer * 0.8f;
        float opacity = 0.4f / (layer + 1);
        Main.spriteBatch.Draw(bloom, Projectile.Center - Main.screenPosition, null,
            Color.Cyan with { A = 0 } * opacity, 0f, bloom.Size() * 0.5f, scale, 0, 0f);
    }
}
```

**TeslaExplosionVisual Shader Effect:**

```csharp
public class TeslaExplosionVisual : ModProjectile
{
    public override bool PreDraw(ref Color lightColor)
    {
        float lifetimeRatio = 1f - (Projectile.timeLeft / (float)MaxLifetime);
        
        ManagedShader shader = ShaderManager.GetShader("FargowiltasCrossmod.TeslaExplosionShader");
        shader.TrySetParameter("lifetimeRatio", lifetimeRatio);
        shader.TrySetParameter("textureSize0", new Vector2(256, 256));
        shader.SetTexture(NoiseTexturesRegistry.ElectricNoise.Value, 1, SamplerState.LinearWrap);
        
        Main.spriteBatch.PrepareForShaders();
        shader.Apply();
        
        // Draw full-screen quad with shader
        Texture2D pixel = MiscTexturesRegistry.Pixel.Value;
        Vector2 drawPos = Projectile.Center - Main.screenPosition;
        float scale = ExpansionRadius * 2f / pixel.Width;
        Main.spriteBatch.Draw(pixel, drawPos, null, Color.White, 0f, 
            pixel.Size() * 0.5f, scale, 0, 0f);
        
        Main.spriteBatch.ResetToDefault();
        return false;
    }
}
```

---

### Gauss Nuke Explosion

**File:** `Content/Calamity/Bosses/ExoMechs/Ares/AresGaussNuke.cs`

```csharp
public override void OnKill(int timeLeft)
{
    // MASSIVE explosion VFX
    
    // Screen shake
    ScreenShakeSystem.StartShake(25f, shakeStrengthDissipationIncrement: 0.5f);
    
    // Multiple expanding rings
    for (int ring = 0; ring < 3; ring++)
    {
        int delay = ring * 5;
        Projectile.NewProjectile(source, Projectile.Center, Vector2.Zero,
            ModContent.ProjectileType<GaussNukeExplosionRing>(), 0, 0, ai0: delay);
    }
    
    // Radial debris
    for (int i = 0; i < 36; i++)
    {
        float angle = MathHelper.TwoPi * i / 36f;
        Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(12f, 24f);
        
        GeneralParticleHandler.SpawnParticle(new HeavyStoneDebrisParticle(
            Projectile.Center, velocity, Color.Orange, 1f, 60));
    }
    
    // Smoke cloud
    for (int i = 0; i < 20; i++)
    {
        Vector2 smokeVelocity = Main.rand.NextVector2Circular(5f, 5f);
        GeneralParticleHandler.SpawnParticle(new HeavySmokeParticle(
            Projectile.Center + Main.rand.NextVector2Circular(30f, 30f),
            smokeVelocity, Color.Orange, 50, 2f, 1f, 0.02f, true));
    }
    
    // Central mushroom cloud
    Projectile.NewProjectile(source, Projectile.Center, Vector2.Zero,
        ModContent.ProjectileType<GaussNukeMushroomCloud>(), 0, 0);
    
    // Blinding flash
    GeneralParticleHandler.SpawnParticle(new DirectionalStrongBloom(
        Projectile.Center, Vector2.Zero, Color.White, 4f, 20));
}
```

---

### Hyperfuturistic Portal

**File:** `Content/Calamity/Bosses/ExoMechs/Ares/AresPortal.cs`

```csharp
public override bool PreDraw(ref Color lightColor)
{
    // Portal shader effect
    ManagedShader shader = ShaderManager.GetShader("FargowiltasCrossmod.HyperfuturisticPortalShader");
    
    float scale = PortalScale * (1f + MathF.Sin(Main.GlobalTimeWrappedHourly * 3f) * 0.05f);
    
    // Set swirling colors
    Vector3[] portalGradient = {
        new Color(50, 200, 255).ToVector3(),   // Cyan
        new Color(100, 50, 255).ToVector3(),   // Purple
        new Color(255, 100, 200).ToVector3(),  // Pink
    };
    shader.TrySetParameter("gradient", portalGradient);
    shader.TrySetParameter("gradientCount", 3f);
    shader.TrySetParameter("scale", scale);
    shader.TrySetParameter("biasToMainSwirlColorPower", 2f);
    shader.SetTexture(NoiseTexturesRegistry.PerlinNoise.Value, 1, SamplerState.LinearWrap);
    
    Main.spriteBatch.PrepareForShaders();
    shader.Apply();
    
    // Draw portal
    Texture2D portalTex = ModContent.Request<Texture2D>("FargowiltasCrossmod/Assets/Textures/Portal").Value;
    Vector2 drawPos = Projectile.Center - Main.screenPosition;
    Main.spriteBatch.Draw(portalTex, drawPos, null, Color.White * Projectile.Opacity,
        Projectile.rotation, portalTex.Size() * 0.5f, scale, 0, 0f);
    
    Main.spriteBatch.ResetToDefault();
    
    // Edge particles
    if (Main.rand.NextBool(3))
    {
        float edgeAngle = Main.rand.NextFloat(MathHelper.TwoPi);
        Vector2 edgePos = Projectile.Center + edgeAngle.ToRotationVector2() * PortalRadius * 0.9f;
        Vector2 edgeVel = (Projectile.Center - edgePos).SafeNormalize(Vector2.Zero) * 2f;
        
        GeneralParticleHandler.SpawnParticle(new GlowySquareParticle(
            edgePos, edgeVel, Color.Cyan, 0.4f, 20, true));
    }
    
    return false;
}
```

---

## Apollo VFX Systems

### Plasma Fireball Trail

**File:** `Content/Calamity/Bosses/ExoMechs/Apollo/ApolloPlasmaFireball.cs`

```csharp
public class ApolloPlasmaFireball : ModProjectile, IPixelatedPrimitiveRenderer
{
    public override void AI()
    {
        // Store old positions for trail
        for (int i = Projectile.oldPos.Length - 1; i > 0; i--)
        {
            Projectile.oldPos[i] = Projectile.oldPos[i - 1];
            Projectile.oldRot[i] = Projectile.oldRot[i - 1];
        }
        Projectile.oldPos[0] = Projectile.Center;
        Projectile.oldRot[0] = Projectile.rotation;
        
        // Core flame particles
        if (Main.rand.NextBool())
        {
            Vector2 particleVel = -Projectile.velocity * 0.3f + Main.rand.NextVector2Circular(2f, 2f);
            GeneralParticleHandler.SpawnParticle(new BloomPixelParticle(
                Projectile.Center, particleVel, Color.Orange, Color.Red, 20, 
                Vector2.One * Main.rand.NextFloat(1f, 2f), Vector2.One * 0.4f));
        }
        
        // Electric edge sparks
        if (Main.rand.NextBool(4))
        {
            Vector2 sparkOffset = Main.rand.NextVector2CircularEdge(Projectile.width * 0.4f, Projectile.height * 0.4f);
            GeneralParticleHandler.SpawnParticle(new LineParticle(
                Projectile.Center + sparkOffset, sparkOffset.SafeNormalize(Vector2.Zero) * 3f,
                Color.Yellow, 0.3f, 10));
        }
    }
    
    public float TrailWidthFunction(float completionRatio) =>
        30f * (1f - completionRatio) * Projectile.scale;
        
    public Color TrailColorFunction(float completionRatio) =>
        Color.Lerp(Color.Orange, Color.DarkRed, completionRatio) * (1f - completionRatio);
    
    public void RenderPixelatedPrimitives(SpriteBatch spriteBatch)
    {
        ManagedShader shader = ShaderManager.GetShader("FargowiltasCrossmod.PlasmaFlameJetShader");
        shader.TrySetParameter("localTime", Main.GlobalTimeWrappedHourly * 2f);
        shader.TrySetParameter("glowPower", 0.6f);
        shader.TrySetParameter("glowColor", Color.Yellow.ToVector4());
        shader.SetTexture(NoiseTexturesRegistry.FireParticleA.Value, 1, SamplerState.LinearWrap);
        
        PrimitiveSettings settings = new(TrailWidthFunction, TrailColorFunction,
            _ => Projectile.Size * 0.5f, Pixelate: true, Shader: shader);
        PrimitiveRenderer.RenderTrail(Projectile.oldPos, settings, 25);
    }
}
```

---

### Thruster Exhaust Trail

**File:** `Content/Calamity/Bosses/ExoMechs/Apollo/ApolloThrusterExhaust.cs`

```csharp
public void DrawThrusterExhaust(NPC apollo, Vector2 thrusterPosition, Vector2 thrusterDirection)
{
    // Build exhaust trail positions
    Vector2[] exhaustPositions = new Vector2[15];
    for (int i = 0; i < exhaustPositions.Length; i++)
    {
        float progress = i / (float)exhaustPositions.Length;
        exhaustPositions[i] = thrusterPosition - thrusterDirection * (30f + progress * 80f);
        
        // Add wobble
        exhaustPositions[i] += thrusterDirection.RotatedBy(MathHelper.PiOver2) *
            MathF.Sin(Main.GlobalTimeWrappedHourly * 10f + i * 0.5f) * 5f * progress;
    }
    
    // Render trail
    ManagedShader shader = ShaderManager.GetShader("FargowiltasCrossmod.ExoTwinThrusterShader");
    shader.TrySetParameter("glowIntensity", apollo.velocity.Length() / 30f);
    shader.SetTexture(NoiseTexturesRegistry.FireParticleB.Value, 1, SamplerState.LinearWrap);
    
    float WidthFunc(float t) => 20f * (1f - t * 0.7f);
    Color ColorFunc(float t) => Color.Lerp(Color.Cyan, Color.Blue, t) * (1f - t);
    
    PrimitiveSettings settings = new(WidthFunc, ColorFunc, _ => Vector2.Zero, Pixelate: true, Shader: shader);
    PrimitiveRenderer.RenderTrail(exhaustPositions, settings, 20);
    
    // Core glow at thruster
    Texture2D bloom = MiscTexturesRegistry.BloomCircleSmall.Value;
    Main.spriteBatch.Draw(bloom, thrusterPosition - Main.screenPosition, null,
        Color.Cyan with { A = 0 } * 0.8f, 0f, bloom.Size() * 0.5f, 1f, 0, 0f);
}
```

---

### Missile Barrage

**File:** `Content/Calamity/Bosses/ExoMechs/Apollo/ApolloMissile.cs`

```csharp
public class ApolloMissile : ModProjectile, IPixelatedPrimitiveRenderer
{
    public override void AI()
    {
        // Homing behavior
        NPC target = Main.npc[(int)Projectile.ai[0]];
        if (target.active)
        {
            Vector2 toTarget = target.Center - Projectile.Center;
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, 
                toTarget.SafeNormalize(Vector2.Zero) * MaxSpeed, HomingStrength);
        }
        
        // Smoke trail
        if (Main.rand.NextBool())
        {
            GeneralParticleHandler.SpawnParticle(new HeavySmokeParticle(
                Projectile.Center, -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f),
                Color.Gray, 30, 0.5f, 0.3f, 0.01f, true));
        }
        
        // Flame particles at exhaust
        Vector2 exhaustPos = Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.Zero) * 15f;
        GeneralParticleHandler.SpawnParticle(new BloomPixelParticle(
            exhaustPos, -Projectile.velocity * 0.2f, Color.Orange, Color.Red, 15,
            Vector2.One * 1.2f, Vector2.One * 0.5f));
    }
    
    public float TrailWidthFunction(float completionRatio) =>
        12f * (1f - completionRatio * 0.6f);
        
    public Color TrailColorFunction(float completionRatio) =>
        Color.Lerp(Color.Orange, Color.Gray, completionRatio) * (1f - completionRatio * 0.8f);
    
    public void RenderPixelatedPrimitives(SpriteBatch spriteBatch)
    {
        ManagedShader shader = ShaderManager.GetShader("FargowiltasCrossmod.MissileFlameTrailShader");
        shader.SetTexture(NoiseTexturesRegistry.FireParticleA.Value, 1, SamplerState.LinearWrap);
        
        PrimitiveSettings settings = new(TrailWidthFunction, TrailColorFunction,
            _ => Projectile.Size * 0.5f, Pixelate: true, Shader: shader);
        PrimitiveRenderer.RenderTrail(Projectile.oldPos, settings, 20);
    }
    
    public override void OnKill(int timeLeft)
    {
        // Explosion
        GeneralParticleHandler.SpawnParticle(new StrongBloom(
            Projectile.Center, Vector2.Zero, Color.Orange, 1.5f, 20));
        
        // Debris
        for (int i = 0; i < 12; i++)
        {
            Vector2 vel = Main.rand.NextVector2CircularEdge(6f, 6f);
            GeneralParticleHandler.SpawnParticle(new GlowySquareParticle(
                Projectile.Center, vel, Color.Orange, 0.5f, 20, true));
        }
        
        // Smoke
        for (int i = 0; i < 6; i++)
        {
            GeneralParticleHandler.SpawnParticle(new HeavySmokeParticle(
                Projectile.Center, Main.rand.NextVector2Circular(4f, 4f),
                Color.DarkGray, 40, 1f, 0.5f, 0.02f, true));
        }
    }
}
```

---

## Artemis VFX Systems

### Precision Laser Shot

**File:** `Content/Calamity/Bosses/ExoMechs/Artemis/ArtemisLaserShot.cs`

```csharp
public class ArtemisLaserShot : ModProjectile, IPixelatedPrimitiveRenderer
{
    public override void AI()
    {
        // Trail update
        for (int i = Projectile.oldPos.Length - 1; i > 0; i--)
            Projectile.oldPos[i] = Projectile.oldPos[i - 1];
        Projectile.oldPos[0] = Projectile.Center;
        
        // Electric particles along path
        if (Main.rand.NextBool(3))
        {
            Vector2 sparkOffset = Projectile.velocity.RotatedBy(MathHelper.PiOver2) *
                Main.rand.NextFloat(-15f, 15f);
            GeneralParticleHandler.SpawnParticle(new LineParticle(
                Projectile.Center + sparkOffset, -Projectile.velocity * 0.1f,
                Color.Cyan, 0.2f, 8));
        }
    }
    
    public float LaserWidthFunction(float completionRatio) =>
        18f * MathF.Pow(1f - completionRatio, 0.6f);
        
    public Color LaserColorFunction(float completionRatio) =>
        Color.Lerp(Color.Cyan, Color.DeepSkyBlue, completionRatio) * (1f - completionRatio * 0.3f);
    
    public void RenderPixelatedPrimitives(SpriteBatch spriteBatch)
    {
        ManagedShader shader = ShaderManager.GetShader("FargowiltasCrossmod.ArtemisLaserShotShader");
        shader.TrySetParameter("glowIntensity", 0.8f);
        shader.SetTexture(MiscTexturesRegistry.WavyBlotchNoise.Value, 1, SamplerState.LinearWrap);
        
        PrimitiveSettings settings = new(LaserWidthFunction, LaserColorFunction,
            _ => Projectile.Size * 0.5f, Pixelate: true, Shader: shader);
        PrimitiveRenderer.RenderTrail(Projectile.oldPos, settings, 30);
    }
    
    public override bool PreDraw(ref Color lightColor)
    {
        // Core glow
        Texture2D bloom = MiscTexturesRegistry.BloomCircleSmall.Value;
        Vector2 drawPos = Projectile.Center - Main.screenPosition;
        
        Main.spriteBatch.Draw(bloom, drawPos, null, Color.White with { A = 0 } * 0.6f,
            0f, bloom.Size() * 0.5f, 0.8f, 0, 0f);
        Main.spriteBatch.Draw(bloom, drawPos, null, Color.Cyan with { A = 0 } * 0.4f,
            0f, bloom.Size() * 0.5f, 1.2f, 0, 0f);
        
        return false;
    }
}
```

---

### Exothermal Disintegration Ray

**File:** `Content/Calamity/Bosses/ExoMechs/Artemis/ExothermalDisintegrationRay.cs`

```csharp
public class ExothermalDisintegrationRay : ModProjectile, IPixelatedPrimitiveRenderer
{
    public Vector2[] LaserPositions;
    
    public override void AI()
    {
        // Calculate laser endpoint via raycasting
        Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
        float maxLength = 2000f;
        
        // Build laser positions along line
        LaserPositions = new Vector2[40];
        for (int i = 0; i < LaserPositions.Length; i++)
        {
            float progress = i / (float)LaserPositions.Length;
            LaserPositions[i] = Projectile.Center + direction * maxLength * progress;
        }
        
        // Edge lightning particles
        if (Main.rand.NextBool(2))
        {
            int posIndex = Main.rand.Next(LaserPositions.Length);
            Vector2 lightningStart = LaserPositions[posIndex];
            Vector2 lightningOffset = direction.RotatedBy(MathHelper.PiOver2) *
                Main.rand.NextFloat(30f, 60f) * Main.rand.NextFloatDirection();
            
            // Mini lightning arc
            DrawMiniLightning(lightningStart, lightningStart + lightningOffset);
        }
        
        // Heat distortion
        SpawnHeatDistortion();
    }
    
    private void SpawnHeatDistortion()
    {
        // Spawn heat metaballs along laser
        foreach (Vector2 pos in LaserPositions)
        {
            if (Main.rand.NextBool(10))
            {
                HeatDistortionMetaball.SpawnParticle(pos + Main.rand.NextVector2Circular(20f, 20f), 0.5f);
            }
        }
    }
    
    public float RayWidthFunction(float completionRatio)
    {
        float baseWidth = 45f;
        float pulseIntensity = 1f + MathF.Sin(Main.GlobalTimeWrappedHourly * 15f) * 0.1f;
        return baseWidth * pulseIntensity * Projectile.Opacity;
    }
    
    public Color RayColorFunction(float completionRatio) =>
        Color.Lerp(Color.Orange, Color.Red, completionRatio) * Projectile.Opacity;
    
    public void RenderPixelatedPrimitives(SpriteBatch spriteBatch)
    {
        ManagedShader shader = ShaderManager.GetShader("FargowiltasCrossmod.ExothermalDisintegrationRayShader");
        shader.TrySetParameter("edgeGlowIntensity", 1.5f);
        shader.TrySetParameter("edgeColorSubtraction", new Vector3(0.3f, 0.1f, 0f));
        shader.SetTexture(NoiseTexturesRegistry.CrackedNoiseA.Value, 1, SamplerState.LinearWrap);
        shader.SetTexture(NoiseTexturesRegistry.ElectricNoise.Value, 2, SamplerState.LinearWrap);
        
        PrimitiveSettings settings = new(RayWidthFunction, RayColorFunction,
            _ => Vector2.Zero, Pixelate: true, Shader: shader);
        PrimitiveRenderer.RenderTrail(LaserPositions, settings, 50);
    }
}
```

---

## Hades (Thanatos) VFX Systems

### Worm Segment Trail

**File:** `Content/Calamity/Bosses/ExoMechs/Hades/HadesBodySegment.cs`

```csharp
public class HadesBodySegment : ModNPC
{
    public Vector2[] TrailPositions = new Vector2[10];
    
    public override void AI()
    {
        // Follow ahead segment
        NPC ahead = Main.npc[(int)NPC.ai[0]];
        Vector2 toAhead = ahead.Center - NPC.Center;
        float segmentDistance = 40f;
        
        if (toAhead.Length() > segmentDistance)
        {
            NPC.Center = ahead.Center - toAhead.SafeNormalize(Vector2.Zero) * segmentDistance;
        }
        NPC.rotation = toAhead.ToRotation();
        
        // Update trail
        for (int i = TrailPositions.Length - 1; i > 0; i--)
            TrailPositions[i] = TrailPositions[i - 1];
        TrailPositions[0] = NPC.Center;
        
        // Electrical sparks on segments
        if (Main.rand.NextBool(30))
        {
            Vector2 sparkPos = NPC.Center + Main.rand.NextVector2Circular(NPC.width * 0.5f, NPC.height * 0.5f);
            GeneralParticleHandler.SpawnParticle(new LineParticle(
                sparkPos, Main.rand.NextVector2Circular(3f, 3f), Color.Red, 0.3f, 10));
        }
    }
    
    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        // Draw electric gleam
        float gleamIntensity = 0.3f + MathF.Sin(Main.GlobalTimeWrappedHourly * 8f + NPC.whoAmI) * 0.2f;
        
        Texture2D shine = MiscTexturesRegistry.ShineFlareTexture.Value;
        Vector2 drawPos = NPC.Center - screenPos;
        spriteBatch.Draw(shine, drawPos, null, Color.Red with { A = 0 } * gleamIntensity,
            0f, shine.Size() * 0.5f, 0.5f, 0, 0f);
        
        return true;
    }
}
```

---

### Super Laserbeam Attack

**File:** `Content/Calamity/Bosses/ExoMechs/Hades/HadesSuperLaserbeam.cs`

```csharp
public class HadesSuperLaserbeam : ModProjectile, IPixelatedPrimitiveRenderer
{
    public float LaserLength = 3000f;
    public Vector2[] LaserPositions;
    
    public override void AI()
    {
        // Anchor to head
        NPC head = Main.npc[(int)Projectile.ai[0]];
        if (!head.active)
        {
            Projectile.Kill();
            return;
        }
        
        Projectile.Center = head.Center;
        Projectile.rotation = head.rotation;
        
        // Build beam positions
        Vector2 direction = Projectile.rotation.ToRotationVector2();
        LaserPositions = new Vector2[60];
        for (int i = 0; i < LaserPositions.Length; i++)
        {
            float progress = i / (float)LaserPositions.Length;
            LaserPositions[i] = Projectile.Center + direction * LaserLength * progress;
        }
        
        // Intense particles at edges
        SpawnEdgeParticles(direction);
        
        // Screen shake during beam
        ScreenShakeSystem.StartShake(8f);
    }
    
    private void SpawnEdgeParticles(Vector2 direction)
    {
        Vector2 perpendicular = direction.RotatedBy(MathHelper.PiOver2);
        
        for (int i = 0; i < 3; i++)
        {
            int posIndex = Main.rand.Next(LaserPositions.Length);
            Vector2 basePos = LaserPositions[posIndex];
            float side = Main.rand.NextFloat(-1f, 1f);
            Vector2 edgePos = basePos + perpendicular * side * BeamWidth(posIndex / (float)LaserPositions.Length) * 0.5f;
            
            // Plasma particles
            GeneralParticleHandler.SpawnParticle(new BloomPixelParticle(
                edgePos, perpendicular * side * 2f, Color.Red, Color.DarkRed, 20,
                Vector2.One * 1.5f, Vector2.One * 0.3f));
        }
    }
    
    private float BeamWidth(float progress)
    {
        float baseWidth = 80f;
        float taper = 1f - progress * 0.3f;
        float pulse = 1f + MathF.Sin(Main.GlobalTimeWrappedHourly * 20f) * 0.05f;
        return baseWidth * taper * pulse * Projectile.Opacity;
    }
    
    public float LaserWidthFunction(float completionRatio) => BeamWidth(completionRatio);
    
    public Color LaserColorFunction(float completionRatio) =>
        Color.Lerp(Color.Red, Color.DarkRed, completionRatio) * Projectile.Opacity;
    
    public void RenderPixelatedPrimitives(SpriteBatch spriteBatch)
    {
        ManagedShader shader = ShaderManager.GetShader("FargowiltasCrossmod.HadesLaserShader");
        shader.TrySetParameter("glowIntensity", 1.5f);
        shader.TrySetParameter("noiseScrollOffset", Main.GlobalTimeWrappedHourly);
        shader.SetTexture(NoiseTexturesRegistry.CrackedNoiseA.Value, 1, SamplerState.LinearWrap);
        
        PrimitiveSettings settings = new(LaserWidthFunction, LaserColorFunction,
            _ => Vector2.Zero, Pixelate: true, Shader: shader);
        PrimitiveRenderer.RenderTrail(LaserPositions, settings, 80);
    }
    
    public override bool PreDraw(ref Color lightColor)
    {
        // Muzzle flash at origin
        Texture2D bloom = MiscTexturesRegistry.BloomCircleSmall.Value;
        Vector2 drawPos = Projectile.Center - Main.screenPosition;
        
        for (int i = 0; i < 4; i++)
        {
            float scale = 3f - i * 0.5f;
            float opacity = 0.6f / (i + 1);
            Main.spriteBatch.Draw(bloom, drawPos, null, Color.Red with { A = 0 } * opacity,
                0f, bloom.Size() * 0.5f, scale, 0, 0f);
        }
        
        return false;
    }
}
```

---

### Death Sequence VFX

**File:** `Content/Calamity/Bosses/ExoMechs/Hades/HadesHead.cs`

```csharp
public void DeathAnimation()
{
    deathAnimationTimer++;
    
    // Phase 1: Building energy (frames 0-120)
    if (deathAnimationTimer < 120)
    {
        float intensity = deathAnimationTimer / 120f;
        
        // Pulsing glow
        float pulse = 1f + MathF.Sin(deathAnimationTimer * 0.3f) * 0.3f;
        GeneralParticleHandler.SpawnParticle(new StrongBloom(
            NPC.Center, Vector2.Zero, Color.Red, 2f * intensity * pulse, 5));
        
        // Sparks gathering
        if (deathAnimationTimer % 3 == 0)
        {
            Vector2 sparkOrigin = NPC.Center + Main.rand.NextVector2Circular(200f, 200f);
            Vector2 sparkVel = (NPC.Center - sparkOrigin).SafeNormalize(Vector2.Zero) * 8f;
            GeneralParticleHandler.SpawnParticle(new GlowySquareParticle(
                sparkOrigin, sparkVel, Color.Red, 0.5f, 30, true));
        }
        
        // Screen shake building
        ScreenShakeSystem.StartShake(intensity * 5f);
    }
    // Phase 2: Explosion (frame 120)
    else if (deathAnimationTimer == 120)
    {
        // MASSIVE explosion
        GeneralParticleHandler.SpawnParticle(new DirectionalStrongBloom(
            NPC.Center, Vector2.Zero, Color.White, 6f, 30));
        
        // Shockwave rings
        for (int ring = 0; ring < 5; ring++)
        {
            Projectile.NewProjectile(source, NPC.Center, Vector2.Zero,
                ModContent.ProjectileType<HadesDeathShockwave>(), 0, 0, ai0: ring * 3);
        }
        
        // Radial debris
        for (int i = 0; i < 50; i++)
        {
            float angle = MathHelper.TwoPi * i / 50f;
            Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(15f, 30f);
            GeneralParticleHandler.SpawnParticle(new HeavyStoneDebrisParticle(
                NPC.Center, vel, Color.Red, 1f, 80));
        }
        
        // Screen shake climax
        ScreenShakeSystem.StartShake(40f);
    }
    // Phase 3: Aftermath
    else
    {
        // Lingering sparks
        if (Main.rand.NextBool(3))
        {
            GeneralParticleHandler.SpawnParticle(new LineParticle(
                NPC.Center + Main.rand.NextVector2Circular(100f, 100f),
                Main.rand.NextVector2Circular(5f, 5f), Color.Red, 0.3f, 15));
        }
    }
}
```

---

## ExoMech Color Palettes

```csharp
public static class ExoMechColors
{
    // Ares - Cyan/Teal industrial
    public static Color AresPrimary = new Color(50, 200, 220);
    public static Color AresSecondary = new Color(30, 150, 180);
    public static Color AresGlow = new Color(100, 255, 255);
    
    // Apollo - Orange/Gold plasma
    public static Color ApolloPrimary = new Color(255, 150, 50);
    public static Color ApolloSecondary = new Color(255, 100, 0);
    public static Color ApolloGlow = new Color(255, 200, 100);
    
    // Artemis - Blue/Cyan precision
    public static Color ArtemisPrimary = new Color(100, 180, 255);
    public static Color ArtemisSecondary = new Color(50, 150, 255);
    public static Color ArtemisGlow = new Color(150, 220, 255);
    
    // Hades - Red/Crimson death
    public static Color HadesPrimary = new Color(255, 50, 50);
    public static Color HadesSecondary = new Color(200, 20, 20);
    public static Color HadesGlow = new Color(255, 100, 100);
}
```

---

## MagnumOpus Adaptation: ExoMech-style Boss VFX

### Theme-Appropriate Color Mapping

| ExoMech | MagnumOpus Theme Equivalent |
|---------|----------------------------|
| Ares (Cyan) | Swan Lake (White/Prismatic) |
| Apollo (Orange) | La Campanella (Orange/Gold) |
| Artemis (Blue) | Moonlight Sonata (Purple/Blue) |
| Hades (Red) | Fate (Black/Pink/Crimson) |

### Key Techniques to Adopt

1. **IPixelatedPrimitiveRenderer** - All laser/beam attacks
2. **Multi-layer bloom stacking** - All explosions and impacts
3. **Edge particle spawning** - Along beam edges
4. **Screen shake scaling** - Building intensity during windups
5. **Heat distortion metaballs** - For fire/plasma effects
6. **Trail position arrays** - For smooth primitive rendering

---

*Extracted from FargosSoulsDLC for MagnumOpus VFX development reference.*
