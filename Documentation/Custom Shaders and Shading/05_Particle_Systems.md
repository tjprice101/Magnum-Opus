# Particle Systems Reference

> **Complete documentation of particle types, spawning patterns, and usage from FargosSoulsDLC.**

---

## Particle System Architecture

### GeneralParticleHandler

The central particle manager. All particles go through this system.

```csharp
// Spawning particles
GeneralParticleHandler.SpawnParticle(new ParticleType(...));

// The handler manages:
// - Particle lifecycle (spawn, update, draw, kill)
// - Batched rendering for performance
// - Automatic cleanup
```

### Base Particle Class Pattern

```csharp
public class CustomParticle : Particle
{
    public override string Texture => "Path/To/Texture";
    
    // Core properties
    public Color StartColor;
    public Color EndColor;
    public Vector2 StartScale;
    public Vector2 EndScale;
    public int Lifetime;
    public int Time;
    
    public override void Update()
    {
        Time++;
        float progress = Time / (float)Lifetime;
        
        // Interpolate properties
        Color = Color.Lerp(StartColor, EndColor, progress);
        Scale = Vector2.Lerp(StartScale, EndScale, progress);
        
        // Physics
        Velocity *= 0.95f; // Drag
        Position += Velocity;
        
        // Kill when done
        if (Time >= Lifetime)
            Kill();
    }
    
    public override void CustomDraw(SpriteBatch spriteBatch)
    {
        // Custom drawing logic
    }
}
```

---

## Core Particle Types

### BloomPixelParticle

**Purpose:** Glowing pixel that scales and fades between colors

```csharp
public class BloomPixelParticle : Particle
{
    public Color StartColor;
    public Color EndColor;
    public Vector2 StartScale;
    public Vector2 EndScale;
    
    public BloomPixelParticle(
        Vector2 position, 
        Vector2 velocity, 
        Color startColor, 
        Color endColor, 
        int lifetime,
        Vector2 startScale, 
        Vector2 endScale)
    {
        Position = position;
        Velocity = velocity;
        StartColor = startColor;
        EndColor = endColor;
        Lifetime = lifetime;
        StartScale = startScale;
        EndScale = endScale;
    }
    
    public override void Update()
    {
        Time++;
        float progress = Time / (float)Lifetime;
        
        Color = Color.Lerp(StartColor, EndColor, progress);
        Scale = Vector2.Lerp(StartScale, EndScale, progress);
        
        Position += Velocity;
        Velocity *= 0.96f;
        
        if (Time >= Lifetime)
            Kill();
    }
    
    public override void CustomDraw(SpriteBatch spriteBatch)
    {
        Texture2D bloom = MiscTexturesRegistry.BloomCircleSmall.Value;
        Vector2 drawPos = Position - Main.screenPosition;
        spriteBatch.Draw(bloom, drawPos, null, Color with { A = 0 }, 
            0f, bloom.Size() * 0.5f, Scale, 0, 0f);
    }
}
```

**Usage:**
```csharp
// Fire/plasma particles
GeneralParticleHandler.SpawnParticle(new BloomPixelParticle(
    position: Projectile.Center,
    velocity: -Projectile.velocity * 0.2f,
    startColor: Color.Orange,
    endColor: Color.Red,
    lifetime: 20,
    startScale: Vector2.One * 1.5f,
    endScale: Vector2.One * 0.3f));
```

---

### GlowySquareParticle

**Purpose:** Glowing square/diamond that rotates and fades

```csharp
public class GlowySquareParticle : Particle
{
    public Color ParticleColor;
    public float StartScale;
    public bool UseGravity;
    
    public GlowySquareParticle(
        Vector2 position,
        Vector2 velocity,
        Color color,
        float scale,
        int lifetime,
        bool useGravity = false)
    {
        Position = position;
        Velocity = velocity;
        ParticleColor = color;
        StartScale = scale;
        Lifetime = lifetime;
        UseGravity = useGravity;
        Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
    }
    
    public override void Update()
    {
        Time++;
        float progress = Time / (float)Lifetime;
        
        Position += Velocity;
        
        if (UseGravity)
            Velocity.Y += 0.15f;
        else
            Velocity *= 0.95f;
        
        Rotation += Velocity.Length() * 0.05f;
        Scale = StartScale * (1f - progress);
        Color = ParticleColor * (1f - progress);
        
        if (Time >= Lifetime)
            Kill();
    }
    
    public override void CustomDraw(SpriteBatch spriteBatch)
    {
        Texture2D tex = ModContent.Request<Texture2D>("Path/To/SquareTexture").Value;
        Vector2 drawPos = Position - Main.screenPosition;
        spriteBatch.Draw(tex, drawPos, null, Color with { A = 0 },
            Rotation, tex.Size() * 0.5f, Scale, 0, 0f);
    }
}
```

**Usage:**
```csharp
// Sparks/debris
for (int i = 0; i < 12; i++)
{
    Vector2 vel = Main.rand.NextVector2CircularEdge(8f, 8f);
    GeneralParticleHandler.SpawnParticle(new GlowySquareParticle(
        position: explosionCenter,
        velocity: vel,
        color: Color.Cyan,
        scale: 0.5f,
        lifetime: 25,
        useGravity: true));
}
```

---

### LineParticle

**Purpose:** Stretchy line that shrinks from endpoints

```csharp
public class LineParticle : Particle
{
    public Color StartColor;
    public Color? EndColor;
    public float StartLength;
    public bool FadeIn;
    
    public LineParticle(
        Vector2 position,
        Vector2 velocity,
        Color color,
        float length,
        int lifetime,
        bool fadeIn = false,
        Color? endColor = null)
    {
        Position = position;
        Velocity = velocity;
        StartColor = color;
        EndColor = endColor;
        StartLength = length;
        Lifetime = lifetime;
        FadeIn = fadeIn;
        Rotation = velocity.ToRotation();
    }
    
    public override void Update()
    {
        Time++;
        float progress = Time / (float)Lifetime;
        
        // Fade in/out
        float opacity = FadeIn ? 
            (progress < 0.3f ? progress / 0.3f : 1f - (progress - 0.3f) / 0.7f) :
            (1f - progress);
        
        Color currentColor = EndColor.HasValue ? 
            Color.Lerp(StartColor, EndColor.Value, progress) : StartColor;
        Color = currentColor * opacity;
        
        Scale = StartLength * (1f - progress * 0.5f);
        
        Position += Velocity;
        Velocity *= 0.92f;
        
        if (Time >= Lifetime)
            Kill();
    }
    
    public override void CustomDraw(SpriteBatch spriteBatch)
    {
        Texture2D lineTex = MiscTexturesRegistry.BloomLineTexture.Value;
        Vector2 drawPos = Position - Main.screenPosition;
        Vector2 origin = new Vector2(0, lineTex.Height * 0.5f);
        
        spriteBatch.Draw(lineTex, drawPos, null, Color with { A = 0 },
            Rotation, origin, new Vector2(Scale, 0.5f), 0, 0f);
    }
}
```

**Usage:**
```csharp
// Electric arcs
for (int i = 0; i < 8; i++)
{
    float angle = MathHelper.TwoPi * i / 8f;
    Vector2 vel = angle.ToRotationVector2() * 6f;
    GeneralParticleHandler.SpawnParticle(new LineParticle(
        position: Projectile.Center,
        velocity: vel,
        color: Color.Cyan,
        length: 0.4f,
        lifetime: 15,
        fadeIn: false,
        endColor: Color.White));
}
```

---

### StrongBloom

**Purpose:** Large intense bloom flash

```csharp
public class StrongBloom : Particle
{
    public Color BloomColor;
    public float StartScale;
    
    public StrongBloom(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
    {
        Position = position;
        Velocity = velocity;
        BloomColor = color;
        StartScale = scale;
        Lifetime = lifetime;
    }
    
    public override void Update()
    {
        Time++;
        float progress = Time / (float)Lifetime;
        
        // Quick expand then fade
        float scaleProgress = progress < 0.2f ? 
            progress / 0.2f : 
            1f - (progress - 0.2f) / 0.8f;
        
        Scale = StartScale * (0.7f + scaleProgress * 0.3f);
        Color = BloomColor * (1f - progress);
        
        Position += Velocity;
        
        if (Time >= Lifetime)
            Kill();
    }
    
    public override void CustomDraw(SpriteBatch spriteBatch)
    {
        Texture2D bloom = MiscTexturesRegistry.BloomCircleSmall.Value;
        Vector2 drawPos = Position - Main.screenPosition;
        
        // Multiple layers for intensity
        spriteBatch.Draw(bloom, drawPos, null, Color with { A = 0 } * 0.5f,
            0f, bloom.Size() * 0.5f, Scale * 1.5f, 0, 0f);
        spriteBatch.Draw(bloom, drawPos, null, Color with { A = 0 } * 0.8f,
            0f, bloom.Size() * 0.5f, Scale, 0, 0f);
        spriteBatch.Draw(bloom, drawPos, null, Color.White with { A = 0 } * 0.5f,
            0f, bloom.Size() * 0.5f, Scale * 0.5f, 0, 0f);
    }
}
```

**Usage:**
```csharp
// Impact flash
GeneralParticleHandler.SpawnParticle(new StrongBloom(
    position: impactPos,
    velocity: Vector2.Zero,
    color: Color.Orange,
    scale: 2f,
    lifetime: 15));
```

---

### DirectionalStrongBloom

**Purpose:** Elongated bloom flash in a direction

```csharp
public class DirectionalStrongBloom : Particle
{
    public Color BloomColor;
    public float StartScale;
    public float StretchFactor;
    
    public DirectionalStrongBloom(
        Vector2 position, 
        Vector2 velocity, 
        Color color, 
        float scale, 
        int lifetime,
        float stretch = 3f)
    {
        Position = position;
        Velocity = velocity;
        BloomColor = color;
        StartScale = scale;
        Lifetime = lifetime;
        StretchFactor = stretch;
        Rotation = velocity.ToRotation();
    }
    
    public override void CustomDraw(SpriteBatch spriteBatch)
    {
        float progress = Time / (float)Lifetime;
        Texture2D bloom = MiscTexturesRegistry.BloomCircleSmall.Value;
        Vector2 drawPos = Position - Main.screenPosition;
        
        Vector2 scale = new Vector2(StartScale * StretchFactor, StartScale) * (1f - progress);
        Color drawColor = BloomColor with { A = 0 } * (1f - progress);
        
        spriteBatch.Draw(bloom, drawPos, null, drawColor,
            Rotation, bloom.Size() * 0.5f, scale, 0, 0f);
    }
}
```

---

### HeavySmokeParticle

**Purpose:** Billowing smoke with drift and fade

```csharp
public class HeavySmokeParticle : Particle
{
    public Color SmokeColor;
    public float StartScale;
    public float EndScale;
    public float RotationSpeed;
    public bool AffectedByGravity;
    
    public HeavySmokeParticle(
        Vector2 position,
        Vector2 velocity,
        Color color,
        int lifetime,
        float startScale,
        float endScale,
        float rotationSpeed,
        bool affectedByGravity)
    {
        Position = position;
        Velocity = velocity;
        SmokeColor = color;
        Lifetime = lifetime;
        StartScale = startScale;
        EndScale = endScale;
        RotationSpeed = rotationSpeed;
        AffectedByGravity = affectedByGravity;
        Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
    }
    
    public override void Update()
    {
        Time++;
        float progress = Time / (float)Lifetime;
        
        // Scale up as it dissipates
        Scale = MathHelper.Lerp(StartScale, EndScale, progress);
        
        // Fade out
        Color = SmokeColor * (1f - progress);
        
        // Movement
        Position += Velocity;
        Velocity *= 0.97f;
        
        if (AffectedByGravity)
            Velocity.Y -= 0.02f; // Rise
        
        // Drift
        Velocity.X += Main.rand.NextFloat(-0.05f, 0.05f);
        
        // Rotate
        Rotation += RotationSpeed;
        
        if (Time >= Lifetime)
            Kill();
    }
    
    public override void CustomDraw(SpriteBatch spriteBatch)
    {
        Texture2D smokeTex = ModContent.Request<Texture2D>("Path/To/SmokeTexture").Value;
        Vector2 drawPos = Position - Main.screenPosition;
        spriteBatch.Draw(smokeTex, drawPos, null, Color,
            Rotation, smokeTex.Size() * 0.5f, Scale, 0, 0f);
    }
}
```

**Usage:**
```csharp
// Explosion smoke
for (int i = 0; i < 10; i++)
{
    Vector2 smokeVel = Main.rand.NextVector2Circular(4f, 4f);
    GeneralParticleHandler.SpawnParticle(new HeavySmokeParticle(
        position: explosionCenter + Main.rand.NextVector2Circular(20f, 20f),
        velocity: smokeVel,
        color: Color.DarkGray,
        lifetime: 50,
        startScale: 0.5f,
        endScale: 1.5f,
        rotationSpeed: 0.02f,
        affectedByGravity: true));
}
```

---

### HeavyStoneDebrisParticle

**Purpose:** Heavy debris with gravity and bounce

```csharp
public class HeavyStoneDebrisParticle : Particle
{
    public Color DebrisColor;
    public float StartScale;
    
    public HeavyStoneDebrisParticle(
        Vector2 position,
        Vector2 velocity,
        Color color,
        float scale,
        int lifetime)
    {
        Position = position;
        Velocity = velocity;
        DebrisColor = color;
        StartScale = scale;
        Lifetime = lifetime;
        Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
    }
    
    public override void Update()
    {
        Time++;
        float progress = Time / (float)Lifetime;
        
        // Heavy gravity
        Velocity.Y += 0.4f;
        
        // Air resistance
        Velocity.X *= 0.98f;
        
        Position += Velocity;
        
        // Spin based on velocity
        Rotation += Velocity.X * 0.05f;
        
        // Fade out
        Color = DebrisColor * (1f - progress);
        Scale = StartScale * (1f - progress * 0.3f);
        
        // Tile collision (optional)
        if (Collision.SolidCollision(Position, 4, 4))
        {
            Velocity.Y *= -0.5f; // Bounce
            Velocity.X *= 0.7f;
        }
        
        if (Time >= Lifetime)
            Kill();
    }
}
```

---

## Fire Particle System (Old Duke)

### FireParticle (High-Performance)

**Purpose:** Optimized fire particles for large quantities

```csharp
// Spawning via dedicated manager
OldDukeFireParticleSystemManager.SpawnParticle(
    position: fireOrigin,
    velocity: fireVelocity,
    color: Color.Green,
    scale: 1f,
    lifetime: 30);
```

**The FastParticle System:**

```csharp
public class FastParticle
{
    public Vector2 Position;
    public Vector2 Velocity;
    public Color Color;
    public float Scale;
    public float Rotation;
    public int Time;
    public int Lifetime;
    
    // No inheritance overhead - struct-like performance
}

public class OldDukeFireParticleSystemManager : ModSystem
{
    private static List<FastParticle> particles = new();
    private static Texture2D[] fireTextures;
    
    public static void SpawnParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
    {
        particles.Add(new FastParticle
        {
            Position = position,
            Velocity = velocity,
            Color = color,
            Scale = scale,
            Lifetime = lifetime,
            Time = 0,
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi)
        });
    }
    
    public override void PostUpdateDusts()
    {
        // Update all particles
        for (int i = particles.Count - 1; i >= 0; i--)
        {
            var p = particles[i];
            p.Time++;
            p.Position += p.Velocity;
            p.Velocity *= 0.95f;
            p.Velocity.Y -= 0.1f; // Rise
            p.Rotation += 0.02f;
            
            if (p.Time >= p.Lifetime)
                particles.RemoveAt(i);
            else
                particles[i] = p;
        }
    }
    
    public void DrawParticles(SpriteBatch spriteBatch)
    {
        // Batch all fire particles
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);
        
        foreach (var p in particles)
        {
            float progress = p.Time / (float)p.Lifetime;
            Texture2D tex = fireTextures[p.Time / 5 % fireTextures.Length]; // Animate
            float alpha = 1f - progress;
            float scale = p.Scale * (1f + progress * 0.5f);
            
            spriteBatch.Draw(tex, p.Position - Main.screenPosition, null,
                p.Color * alpha, p.Rotation, tex.Size() * 0.5f, scale, 0, 0f);
        }
        
        spriteBatch.End();
    }
}
```

---

## Metaball Systems

### BileMetaball (Old Duke)

**Purpose:** Slimy blob effects that merge together

```csharp
public class BileMetaball : MetaballType
{
    public override string MetaballTexture => "Path/To/MetaballCircle";
    
    // Particles in this metaball system
    private List<BileParticle> particles = new();
    
    public static void SpawnParticle(Vector2 position, Vector2 velocity, float size, int lifetime)
    {
        Instance.particles.Add(new BileParticle
        {
            Position = position,
            Velocity = velocity,
            Size = size,
            Lifetime = lifetime
        });
    }
    
    public override void Update()
    {
        for (int i = particles.Count - 1; i >= 0; i--)
        {
            var p = particles[i];
            p.Time++;
            p.Position += p.Velocity;
            p.Velocity.Y += 0.15f; // Gravity
            p.Velocity *= 0.98f;
            
            if (p.Time >= p.Lifetime)
                particles.RemoveAt(i);
            else
                particles[i] = p;
        }
    }
    
    public override void DrawToTarget(SpriteBatch spriteBatch)
    {
        Texture2D circle = ModContent.Request<Texture2D>(MetaballTexture).Value;
        
        foreach (var p in particles)
        {
            float progress = p.Time / (float)p.Lifetime;
            float size = p.Size * (1f - progress * 0.3f);
            
            // Encode data in color channels:
            // R = lifetime ratio (for palette lookup)
            // G = darkening factor
            // B = dissolve interpolant
            Color dataColor = new Color(
                (byte)(progress * 255),
                (byte)(0),
                (byte)(progress > 0.7f ? (progress - 0.7f) / 0.3f * 255 : 0),
                255);
            
            spriteBatch.Draw(circle, p.Position - Main.screenPosition, null,
                dataColor, 0f, circle.Size() * 0.5f, size, 0, 0f);
        }
    }
    
    public override void DrawAfterTiles(SpriteBatch spriteBatch)
    {
        // Apply metaball shader
        ManagedShader shader = ShaderManager.GetShader("FargowiltasCrossmod.OldDukeBileMetaballShader");
        
        // Set gradient palette (green bile colors)
        Vector3[] bileGradient = {
            new Color(50, 150, 50).ToVector3(),   // Bright green
            new Color(30, 100, 30).ToVector3(),   // Medium green
            new Color(20, 60, 20).ToVector3(),    // Dark green
        };
        shader.TrySetParameter("gradient", bileGradient);
        shader.TrySetParameter("gradientCount", 3f);
        shader.SetTexture(NoiseTexturesRegistry.BubblyNoise.Value, 2, SamplerState.LinearWrap);
        
        // Draw the render target with shader
        spriteBatch.PrepareForShaders();
        shader.Apply();
        spriteBatch.Draw(MetaballTarget, Vector2.Zero, Color.White);
        spriteBatch.ResetToDefault();
    }
}
```

---

### HeatDistortionMetaball

**Purpose:** Screen-space heat shimmer effect

```csharp
public class HeatDistortionMetaball : MetaballType
{
    public static void SpawnParticle(Vector2 position, float intensity)
    {
        Instance.particles.Add(new HeatParticle
        {
            Position = position,
            Intensity = intensity,
            Lifetime = 30
        });
    }
    
    // Renders to a separate target that's used by screen filter
    public override void DrawToTarget(SpriteBatch spriteBatch)
    {
        Texture2D circle = MiscTexturesRegistry.BloomCircleSmall.Value;
        
        foreach (var p in particles)
        {
            float progress = p.Time / (float)p.Lifetime;
            float intensity = p.Intensity * (1f - progress);
            
            // Intensity encoded in alpha
            spriteBatch.Draw(circle, p.Position - Main.screenPosition, null,
                Color.White * intensity, 0f, circle.Size() * 0.5f, 2f, 0, 0f);
        }
    }
}

// Screen filter that uses the heat metaball target
public class HeatDistortionFilter : ModSystem
{
    public override void PostDrawTiles()
    {
        if (!HeatDistortionMetaball.HasActiveParticles)
            return;
        
        ManagedShader shader = ShaderManager.GetShader("FargowiltasCrossmod.HeatDistortionFilter");
        shader.SetTexture(HeatDistortionMetaball.Target, 2, SamplerState.LinearClamp);
        shader.SetTexture(NoiseTexturesRegistry.PerlinNoise.Value, 3, SamplerState.LinearWrap);
        shader.TrySetParameter("globalTime", Main.GlobalTimeWrappedHourly);
        
        // Apply to screen
        Main.spriteBatch.PrepareForShaders();
        shader.Apply();
        Main.spriteBatch.Draw(Main.screenTarget, Vector2.Zero, Color.White);
        Main.spriteBatch.ResetToDefault();
    }
}
```

---

## Particle Spawning Patterns

### Radial Burst

```csharp
public static void SpawnRadialBurst(Vector2 center, int count, float speed, Color color)
{
    for (int i = 0; i < count; i++)
    {
        float angle = MathHelper.TwoPi * i / count;
        Vector2 velocity = angle.ToRotationVector2() * speed;
        
        GeneralParticleHandler.SpawnParticle(new GlowySquareParticle(
            center, velocity, color, 0.5f, 25, true));
    }
}
```

### Cone Spray

```csharp
public static void SpawnConeSpray(Vector2 origin, Vector2 direction, float coneAngle, int count, float speed, Color color)
{
    float baseAngle = direction.ToRotation();
    
    for (int i = 0; i < count; i++)
    {
        float angle = baseAngle + Main.rand.NextFloat(-coneAngle, coneAngle);
        float particleSpeed = speed * Main.rand.NextFloat(0.7f, 1.3f);
        Vector2 velocity = angle.ToRotationVector2() * particleSpeed;
        
        GeneralParticleHandler.SpawnParticle(new BloomPixelParticle(
            origin, velocity, color, color * 0.5f, 20, Vector2.One, Vector2.One * 0.3f));
    }
}
```

### Trail Along Path

```csharp
public static void SpawnTrailAlongPath(Vector2[] positions, Color startColor, Color endColor)
{
    for (int i = 0; i < positions.Length; i++)
    {
        float progress = i / (float)positions.Length;
        Color color = Color.Lerp(startColor, endColor, progress);
        Vector2 velocity = Main.rand.NextVector2Circular(1f, 1f);
        
        GeneralParticleHandler.SpawnParticle(new BloomPixelParticle(
            positions[i], velocity, color, color * 0.3f, 15,
            Vector2.One * (1f - progress), Vector2.One * 0.2f));
    }
}
```

### Vortex/Spiral

```csharp
public static void SpawnVortexParticles(Vector2 center, float radius, int count, Color color, bool inward)
{
    for (int i = 0; i < count; i++)
    {
        float angle = MathHelper.TwoPi * i / count + Main.GlobalTimeWrappedHourly * 2f;
        float currentRadius = inward ? radius * (1f - (float)i / count) : radius * (float)i / count;
        Vector2 position = center + angle.ToRotationVector2() * currentRadius;
        
        // Velocity tangent to circle
        Vector2 tangent = (angle + MathHelper.PiOver2).ToRotationVector2();
        Vector2 velocity = tangent * 2f;
        if (inward)
            velocity += (center - position).SafeNormalize(Vector2.Zero) * 3f;
        
        GeneralParticleHandler.SpawnParticle(new GlowySquareParticle(
            position, velocity, color, 0.4f, 20, false));
    }
}
```

---

## MagnumOpus Particle Adaptation

### Theme-Colored Particle Helpers

```csharp
public static class MagnumParticleHelpers
{
    // Fate theme particles
    public static void SpawnFateImpact(Vector2 position, float scale)
    {
        // Core white flash
        GeneralParticleHandler.SpawnParticle(new StrongBloom(
            position, Vector2.Zero, Color.White, scale * 2f, 15));
        
        // Dark pink/purple layers
        GeneralParticleHandler.SpawnParticle(new StrongBloom(
            position, Vector2.Zero, new Color(180, 50, 100), scale * 1.5f, 18));
        
        // Radial glowing squares
        for (int i = 0; i < 12; i++)
        {
            float angle = MathHelper.TwoPi * i / 12f;
            float progress = i / 12f;
            Color particleColor = Color.Lerp(new Color(180, 50, 100), new Color(255, 60, 80), progress);
            Vector2 velocity = angle.ToRotationVector2() * (6f + Main.rand.NextFloat(4f));
            
            GeneralParticleHandler.SpawnParticle(new GlowySquareParticle(
                position, velocity, particleColor, 0.5f * scale, 25, true));
        }
    }
    
    // La Campanella fire particles
    public static void SpawnCampanellaFlame(Vector2 position, Vector2 direction)
    {
        // Fire bloom
        GeneralParticleHandler.SpawnParticle(new BloomPixelParticle(
            position, direction * 0.5f + Main.rand.NextVector2Circular(1f, 1f),
            new Color(255, 100, 0), new Color(100, 20, 0), 25,
            Vector2.One * 1.2f, Vector2.One * 0.3f));
        
        // Smoke
        GeneralParticleHandler.SpawnParticle(new HeavySmokeParticle(
            position, direction * 0.2f,
            Color.Black * 0.6f, 40, 0.4f, 1f, 0.02f, true));
    }
    
    // Swan Lake feather particles
    public static void SpawnSwanFeatherBurst(Vector2 position, int count)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = MathHelper.TwoPi * i / count;
            Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
            
            // Alternate white/black
            Color featherColor = i % 2 == 0 ? Color.White : new Color(30, 30, 40);
            
            // Add rainbow shimmer
            if (Main.rand.NextBool(4))
            {
                float hue = Main.rand.NextFloat();
                featherColor = Main.hslToRgb(hue, 0.8f, 0.7f);
            }
            
            GeneralParticleHandler.SpawnParticle(new FeatherParticle(
                position, velocity, featherColor, 0.4f, 40));
        }
    }
}
```

---

*Extracted from FargosSoulsDLC for MagnumOpus VFX development reference.*
