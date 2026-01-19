# Primitive Trail Rendering System

> **Deep dive into FargosSoulsDLC's trail rendering system for lasers, projectiles, and effects.**

---

## Core Concept

The primitive trail system renders smooth, shader-driven trails by:
1. Collecting a series of positions (typically `Projectile.oldPos[]`)
2. Creating vertices along those positions with calculated widths
3. Applying a shader to color and stylize the trail
4. Optionally pixelating the result for retro aesthetics

---

## The IPixelatedPrimitiveRenderer Interface

All projectiles that render pixelated trails implement this interface:

```csharp
public class MyProjectile : ModProjectile, IPixelatedPrimitiveRenderer
{
    public void RenderPixelatedPrimitives(SpriteBatch spriteBatch)
    {
        // Trail rendering code here
    }
}
```

**Why use this?** The interface hooks into the rendering pipeline at the correct time for pixelation effects. The system batches all pixelated primitives and renders them to a lower-resolution target.

---

## PrimitiveSettings Configuration

```csharp
PrimitiveSettings settings = new(
    WidthFunction,           // Func<float, float> - completionRatio => width
    ColorFunction,           // Func<float, Color> - completionRatio => color
    _ => Projectile.Size * 0.5f,  // Func<float, Vector2> - offset from position
    Pixelate: true,          // bool - enable pixel art style
    Shader: shader           // ManagedShader - the shader to apply
);

PrimitiveRenderer.RenderTrail(positions, settings, segmentCount);
```

### Parameter Breakdown

| Parameter | Type | Purpose |
|-----------|------|---------|
| `WidthFunction` | `Func<float, float>` | Returns trail width at each point (0 = start, 1 = end) |
| `ColorFunction` | `Func<float, Color>` | Returns color at each point |
| `Offset` | `Func<float, Vector2>` | Position offset (usually half the projectile size) |
| `Pixelate` | `bool` | Whether to use pixel art rendering |
| `Shader` | `ManagedShader` | The shader to apply to the trail |

---

## Width Functions - Common Patterns

### Pattern 1: Linear Taper (Most Common)
```csharp
public float LaserWidthFunction(float completionRatio)
{
    // Starts at full width, tapers to 0
    return 50f * (1f - completionRatio);
}
```

### Pattern 2: QuadraticBump (Thick Middle)
```csharp
public float LaserWidthFunction(float completionRatio)
{
    // Thin at both ends, thick in middle
    float bump = MathF.Sin(completionRatio * MathHelper.Pi);
    return 40f * bump * Projectile.scale;
}
```

### Pattern 3: Inverse Lerp with Bump
```csharp
// From HadesLaserBurst.cs
public float LaserWidthFunction(float completionRatio)
{
    float widthInterpolant = Utilities.InverseLerp(0.06f, 0.27f, completionRatio);
    widthInterpolant *= Utilities.InverseLerp(0.9f, 0.72f, completionRatio);
    return MathHelper.Lerp(4f, 23f, widthInterpolant) * Projectile.Opacity;
}
```

### Pattern 4: Bloom Width (Larger than main trail)
```csharp
// For drawing a second, softer bloom layer
public float BloomWidthFunction(float completionRatio)
{
    return LaserWidthFunction(completionRatio) * 2.7f;  // 2.7x larger
}
```

---

## Color Functions - Common Patterns

### Pattern 1: Simple Opacity Fade
```csharp
public Color LaserColorFunction(float completionRatio)
{
    return new Color(1f, 0.1f, 0.22f) * Projectile.Opacity;
}
```

### Pattern 2: Gradient Along Trail
```csharp
public Color LaserColorFunction(float completionRatio)
{
    Color startColor = Color.Orange;
    Color endColor = Color.Red;
    return Color.Lerp(startColor, endColor, completionRatio) * Projectile.Opacity;
}
```

### Pattern 3: Bump with InverseLerp
```csharp
// From PulseBlast.cs
public Color BloomColorFunction(float completionRatio)
{
    return Projectile.GetAlpha(new Color(255, 153, 249)) * 
           LumUtils.InverseLerpBump(0.02f, 0.05f, 0.81f, 0.95f, completionRatio) * 0.3f;
}
```

### Pattern 4: Bloom Color (Dimmer version)
```csharp
public Color LaserColorFunctionBloom(float completionRatio)
{
    return LaserColorFunction(completionRatio) * 0.3f;
}
```

---

## Complete Trail Rendering Examples

### Example 1: Basic Laser (HadesLaserBurst)
```csharp
public void RenderPixelatedPrimitives(SpriteBatch spriteBatch)
{
    // Get shader
    ManagedShader shader = ShaderManager.GetShader("FargowiltasCrossmod.HadesLaserShader");
    
    // Set shader parameters
    shader.TrySetParameter("glowIntensity", 1f);
    shader.TrySetParameter("noiseScrollOffset", Projectile.identity * 0.3149f);
    
    // Set textures
    shader.SetTexture(
        ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/Cracks"), 
        1, 
        SamplerState.LinearWrap
    );
    
    // Create settings for main trail
    PrimitiveSettings settings = new(
        LaserWidthFunction, 
        LaserColorFunction, 
        _ => Projectile.Size * 0.5f, 
        Pixelate: true, 
        Shader: shader
    );
    PrimitiveRenderer.RenderTrail(Projectile.oldPos, settings, 21);
    
    // Second pass for bloom
    shader.TrySetParameter("glowIntensity", 0.76f);
    PrimitiveSettings bloomSettings = new(
        LaserWidthFunctionBloom, 
        LaserColorFunctionBloom, 
        _ => Projectile.Size * 0.5f + Projectile.velocity * 3f,  // Offset forward
        Pixelate: true, 
        Shader: shader
    );
    PrimitiveRenderer.RenderTrail(Projectile.oldPos, bloomSettings, 21);
}
```

### Example 2: Flame Jet (PlasmaFlameJet)
```csharp
public void RenderPixelatedPrimitives(SpriteBatch spriteBatch)
{
    ManagedShader flameShader = ShaderManager.GetShader("FargowiltasCrossmod.PlasmaFlameJetShader");
    
    // Flame-specific parameters
    flameShader.TrySetParameter("localTime", Main.GlobalTimeWrappedHourly + Projectile.identity * 0.412f);
    flameShader.TrySetParameter("glowPower", 2.5f);
    flameShader.TrySetParameter("glowColor", new Vector4(3f, 3f, 1.5f, 0f));  // Green-yellow glow
    flameShader.TrySetParameter("edgeFadeThreshold", 0.1f);
    flameShader.SetTexture(MiscTexturesRegistry.WavyBlotchNoise.Value, 1, SamplerState.LinearWrap);
    
    PrimitiveSettings settings = new(
        FlameJetWidthFunction, 
        FlameJetColorFunction, 
        _ => Vector2.Zero, 
        Pixelate: true, 
        Shader: flameShader
    );
    PrimitiveRenderer.RenderTrail(ControlPoints, settings, 35);
}
```

### Example 3: Double-Pass Bloom Trail (BlazingExoLaserbeam)
```csharp
public void RenderPixelatedPrimitives(SpriteBatch spriteBatch)
{
    // Gather control points
    List<Vector2> laserPositions = Projectile.GetLaserControlPoints(12, LaserbeamLength);
    
    // Bloom pass first (behind main trail)
    ManagedShader shader = ShaderManager.GetShader("FargowiltasCrossmod.PrimitiveBloomShader");
    PrimitiveSettings bloomSettings = new(
        BloomWidthFunction, 
        BloomColorFunction, 
        Pixelate: true, 
        Shader: shader
    );
    PrimitiveRenderer.RenderTrail(laserPositions, bloomSettings, 60);
    
    // Main trail pass
    ManagedShader laserShader = ShaderManager.GetShader("FargowiltasCrossmod.BlazingExoLaserbeamShader");
    laserShader.TrySetParameter("laserDirection", Projectile.velocity.SafeNormalize(Vector2.UnitX).ToVector3());
    laserShader.SetTexture(MiscTexturesRegistry.WavyBlotchNoise.Value, 1, SamplerState.LinearWrap);
    
    PrimitiveSettings laserSettings = new(
        LaserWidthFunction, 
        LaserColorFunction, 
        _ => Projectile.Size * 0.5f, 
        Pixelate: true, 
        Shader: laserShader
    );
    PrimitiveRenderer.RenderTrail(laserPositions, laserSettings, 60);
}
```

---

## Control Point Generation

### Using oldPos Array
```csharp
// Most common - use built-in position history
PrimitiveRenderer.RenderTrail(Projectile.oldPos, settings, Projectile.oldPos.Length);
```

### Using GetLaserControlPoints Extension
```csharp
// For laserbeams - generates points along the laser direction
List<Vector2> laserPositions = Projectile.GetLaserControlPoints(12, LaserbeamLength);
```

### Custom Control Points
```csharp
// For curved/bezier paths
Vector2[] controlPoints = new Vector2[PointCount];
for (int i = 0; i < controlPoints.Length; i++)
{
    float t = (i / (float)(controlPoints.Length - 1f));
    controlPoints[i] = Utilities.QuadraticBezier(startPoint, midPoint, endPoint, t);
}
```

---

## Multi-Pass Rendering Strategy

For maximum visual impact, trails often use multiple passes:

```
Pass 1: Wide, soft bloom (behind)
   - Width: 2-3x main trail
   - Color: 30-50% opacity
   - Shader: PrimitiveBloomShader

Pass 2: Main trail (middle)
   - Width: Standard width function
   - Color: Full color with opacity
   - Shader: Themed shader (laser, flame, etc.)

Pass 3: Inner bright core (front)
   - Width: 0.3-0.5x main trail
   - Color: White or bright accent
   - May use same shader with different params
```

---

## Segment Count Guidelines

| Trail Type | Recommended Segments |
|------------|---------------------|
| Short projectile trail | 15-25 |
| Medium laser | 30-45 |
| Long deathray | 50-70 |
| Curved flame jet | 35-50 |
| Smooth bezier curve | 20-30 |

Higher segment counts = smoother curves but more performance cost.

---

## MagnumOpus Adaptation Notes

1. **Create themed width functions** for each score (smooth for Moonlight, aggressive for La Campanella)
2. **Build color gradient functions** that use theme palettes
3. **Consider reducing pixelation** for MagnumOpus's smoother aesthetic
4. **Layer musical elements** - notes, staff lines as secondary trails
5. **Use QuadraticBump** for smooth intensity falloffs in HLSL shaders

---

*Extracted from FargosSoulsDLC for MagnumOpus VFX development reference.*
