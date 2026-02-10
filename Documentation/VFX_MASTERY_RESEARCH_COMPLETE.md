# VFX Mastery Research - Complete Knowledge Base

> **COMPREHENSIVE DOCUMENTATION OF ALL VFX PATTERNS, TECHNIQUES, AND SYSTEMS**
> 
> This document captures everything learned from MonoGame, tModLoader, HLSL shaders,
> FargosSoulsDLC patterns, and Calamity Mod techniques.
>
> **STATUS: ENHANCED** - All core VFX systems have been reviewed and enhanced.
> 
> **NEW SYSTEMS CREATED:**
> - `RenderTargetPool.cs` (Common/Systems/VFX/Core/) - Unified render target management with transient/persistent pooling
> - `PixelatedTrailRenderer.cs` (Common/Systems/VFX/Trails/) - FargosSoulsDLC-style pixelated trails with render target support
>
> **EXISTING SYSTEMS VERIFIED AS COMPREHENSIVE:**
> - `MagnumParticleHandler.cs` - Object pooling, batched blend modes, { A = 0 } pattern, aggressive culling
> - `VFXUtilities.cs` - QuadraticBump, PaletteLerp, motion blur, Catmull-Rom interpolation
> - `VFXTextureRegistry.cs` - Noise textures, LUTs, beams, masks, all with fallback generation
> - `EnhancedTrailRenderer.cs` - Multi-pass rendering, interpolation, theme integration
> - `BloomRenderer.cs` - Multi-layer stacking (4 layers), pulse/charge/impact animations

---

## 📚 Table of Contents

1. [MonoGame GraphicsDevice API](#1-monogame-graphicsdevice-api)
2. [RenderTarget2D for Multi-Pass Rendering](#2-rendertarget2d-for-multi-pass-rendering)
3. [Effect/Shader Management](#3-effectshader-management)
4. [BlendState Modes](#4-blendstate-modes)
5. [Primitive Trail Rendering](#5-primitive-trail-rendering)
6. [Multi-Layer Bloom Stacking](#6-multi-layer-bloom-stacking)
7. [HLSL Shader Patterns](#7-hlsl-shader-patterns)
8. [Width & Color Function Patterns](#8-width--color-function-patterns)
9. [Interpolated Rendering (144Hz+)](#9-interpolated-rendering-144hz)
10. [Screen Effects & Post-Processing](#10-screen-effects--post-processing)
11. [Particle System Optimization](#11-particle-system-optimization)
12. [Implementation Reference](#12-implementation-reference)

---

## 1. MonoGame GraphicsDevice API

### Core Graphics Device Methods

```csharp
// Get the graphics device
GraphicsDevice device = Main.instance.GraphicsDevice;

// Set render state
device.BlendState = BlendState.Additive;           // Blend mode
device.DepthStencilState = DepthStencilState.None; // Disable depth testing
device.RasterizerState = RasterizerState.CullNone; // Draw both sides
device.SamplerStates[0] = SamplerState.LinearClamp; // Texture filtering

// Set render target (null = back buffer)
device.SetRenderTarget(myRenderTarget);
// ... draw to render target ...
device.SetRenderTarget(null);
```

### Primitive Drawing Methods

```csharp
// Method 1: DrawPrimitives with VertexBuffer
device.SetVertexBuffer(vertexBuffer);
device.Indices = indexBuffer;
device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexCount, 0, triangleCount);

// Method 2: DrawUserPrimitives (no buffer setup needed)
device.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices, 0, primitiveCount);

// Method 3: DrawUserIndexedPrimitives (most flexible)
device.DrawUserIndexedPrimitives(
    PrimitiveType.TriangleList,
    vertices, 0, vertexCount,
    indices, 0, triangleCount
);
```

### PrimitiveType Options

| Type | Description | Use Case |
|------|-------------|----------|
| `TriangleList` | Independent triangles | Complex shapes, meshes |
| `TriangleStrip` | Connected triangles | **Trails, beams** (most efficient) |
| `LineList` | Independent lines | Debug visualization |
| `LineStrip` | Connected lines | Simple trails, outlines |

### Vertex Types

```csharp
// Standard vertex types (or create custom with VertexDeclaration)
VertexPositionColor         // Position + Color
VertexPositionColorTexture  // Position + Color + UV (most common for trails)
VertexPositionNormalTexture // Position + Normal + UV
VertexPositionTexture       // Position + UV only

// Example vertex creation for trail
var vertex = new VertexPositionColorTexture(
    new Vector3(screenX, screenY, 0),        // Position (2D = Z=0)
    color,                                     // Color (with A=0 for additive)
    new Vector2(completionRatio, vCoord)      // UV (X=progress, Y=0 or 1)
);
```

---

## 2. RenderTarget2D for Multi-Pass Rendering

### Creating Render Targets

```csharp
// Basic render target (matches screen size)
RenderTarget2D target = new RenderTarget2D(
    device,
    Main.screenWidth,
    Main.screenHeight,
    false,                          // No mipmaps for 2D
    SurfaceFormat.Color,           // Standard RGBA8
    DepthFormat.None               // No depth buffer needed
);

// With specific options
RenderTarget2D target = new RenderTarget2D(
    device,
    width, height,
    mipMap: false,
    preferredFormat: SurfaceFormat.Color,
    preferredDepthFormat: DepthFormat.None,
    preferredMultiSampleCount: 0,
    usage: RenderTargetUsage.PreserveContents // Keep contents after unbinding
);
```

### Multi-Pass Rendering Pattern

```csharp
// Step 1: Render scene to target
device.SetRenderTarget(sceneTarget);
device.Clear(Color.Transparent);
// ... draw scene ...

// Step 2: Apply post-process shader
device.SetRenderTarget(processedTarget);
device.Clear(Color.Transparent);
spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, 
    effect: blurShader);
spriteBatch.Draw(sceneTarget, Vector2.Zero, Color.White);
spriteBatch.End();

// Step 3: Composite to screen
device.SetRenderTarget(null);
spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);
spriteBatch.Draw(processedTarget, Vector2.Zero, Color.White);
spriteBatch.End();
```

### Bloom Implementation with Render Targets

```csharp
// Pass 1: Draw bright objects to bloom target
device.SetRenderTarget(bloomTarget);
device.Clear(Color.Transparent);
DrawBrightObjects(); // Objects that should glow

// Pass 2: Horizontal blur
device.SetRenderTarget(blurHTarget);
ApplyShader(horizontalBlurShader, bloomTarget);

// Pass 3: Vertical blur
device.SetRenderTarget(blurVTarget);
ApplyShader(verticalBlurShader, blurHTarget);

// Pass 4: Composite bloom over scene
device.SetRenderTarget(null);
DrawScene();
spriteBatch.Begin(BlendState.Additive);
spriteBatch.Draw(blurVTarget, Vector2.Zero, Color.White * bloomIntensity);
spriteBatch.End();
```

---

## 3. Effect/Shader Management

### Loading Shaders in tModLoader

```csharp
// Method 1: ModContent.Request (compiled .xnb)
Asset<Effect> shaderAsset = ModContent.Request<Effect>("ModName/Assets/Shaders/MyShader");
Effect shader = shaderAsset.Value;

// Method 2: From embedded byte array (runtime compilation)
// Note: Requires Effect Constructor
byte[] shaderBytecode = LoadShaderBytecode("path/to/shader.fxb");
Effect shader = new Effect(device, shaderBytecode);
```

### Setting Shader Parameters

```csharp
// Set individual parameters
shader.Parameters["uColor"]?.SetValue(color.ToVector4());
shader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
shader.Parameters["uOpacity"]?.SetValue(opacity);
shader.Parameters["uNoiseTexture"]?.SetValue(noiseTexture);

// Set matrices
shader.Parameters["WorldViewProjection"]?.SetValue(
    Matrix.CreateOrthographicOffCenter(
        0, Main.screenWidth, Main.screenHeight, 0, 0, 1
    )
);
```

### Applying Shaders to SpriteBatch

```csharp
// Method 1: Pass effect to Begin
spriteBatch.Begin(
    SpriteSortMode.Immediate,      // Required for shader params
    BlendState.Additive,
    SamplerState.LinearClamp,
    DepthStencilState.None,
    RasterizerState.CullNone,
    effect: shader,
    Main.GameViewMatrix.TransformationMatrix
);

// Method 2: Apply technique then draw
shader.CurrentTechnique = shader.Techniques["BloomPass"];
shader.CurrentTechnique.Passes[0].Apply();
// Draw primitives directly to device
```

### MiscShaderData Integration (tModLoader)

```csharp
// Register shader with GameShaders.Misc
public override void Load()
{
    GameShaders.Misc["MagnumOpus:TrailShader"] = new MiscShaderData(
        new Ref<Effect>(shader),
        "TrailPass"
    );
}

// Use in trail rendering
var shaderData = GameShaders.Misc["MagnumOpus:TrailShader"];
shaderData.Apply();
```

---

## 4. BlendState Modes

### Standard Blend States

```csharp
// AlphaBlend: Standard transparency
// Result = Source * SourceAlpha + Dest * (1 - SourceAlpha)
BlendState.AlphaBlend

// Additive: Glow/bloom effects
// Result = Source * SourceAlpha + Dest * One
BlendState.Additive

// NonPremultiplied: For non-premultiplied alpha textures
BlendState.NonPremultiplied

// Opaque: No blending
BlendState.Opaque
```

### Custom Blend States

```csharp
// Multiplicative (for shadows/darkening)
BlendState multiply = new BlendState
{
    ColorSourceBlend = Blend.DestinationColor,
    ColorDestinationBlend = Blend.Zero,
    AlphaSourceBlend = Blend.DestinationAlpha,
    AlphaDestinationBlend = Blend.Zero
};

// Screen blend (for lighting)
BlendState screen = new BlendState
{
    ColorSourceBlend = Blend.One,
    ColorDestinationBlend = Blend.InverseSourceColor,
    AlphaSourceBlend = Blend.One,
    AlphaDestinationBlend = Blend.InverseSourceAlpha
};
```

### **CRITICAL: The { A = 0 } Pattern**

```csharp
// ❌ WRONG: Alpha channel causes additive issues
Color glowColor = new Color(255, 100, 50, 255);

// ✅ CORRECT: Remove alpha for proper additive blending
Color glowColor = new Color(255, 100, 50, 0);
// Or using C# 9+ with-expression:
Color glowColor = baseColor with { A = 0 };

// Draw with opacity multiplier instead
spriteBatch.Draw(texture, pos, null, glowColor * opacity, ...);
```

**Why this matters:**
- Additive blending formula: `Result = Source * SourceAlpha + Dest * One`
- If SourceAlpha = 255 (1.0), the source is fully added
- If SourceAlpha = 0, the source contribution is controlled by color multiplier
- This gives finer control and prevents color oversaturation

---

## 5. Primitive Trail Rendering

### IPixelatedPrimitiveRenderer Interface (FargosSoulsDLC Pattern)

```csharp
public interface IPixelatedPrimitiveRenderer
{
    float WidthFunction(float completionRatio);
    Color ColorFunction(float completionRatio);
    void DrawPrimitives();
}
```

### PrimitiveSettings Struct

```csharp
public struct PrimitiveSettings
{
    public Func<float, float> WidthFunction;    // Trail width at progress
    public Func<float, Color> ColorFunction;    // Trail color at progress
    public Func<float, Vector2> OffsetFunction; // Optional wobble/offset
    public bool Pixelate;                        // Use pixelation render target
    public bool Smoothen;                        // Apply Catmull-Rom smoothing
    public MiscShaderData Shader;                // Optional shader
    
    public static PrimitiveSettings Create(
        Func<float, float> width,
        Func<float, Color> color,
        Func<float, Vector2> offset = null,
        bool smoothen = true,
        MiscShaderData shader = null)
    {
        return new PrimitiveSettings
        {
            WidthFunction = width,
            ColorFunction = color,
            OffsetFunction = offset,
            Smoothen = smoothen,
            Shader = shader
        };
    }
}
```

### Trail Vertex Generation

```csharp
public static void GenerateTrailVertices(
    Vector2[] positions,
    PrimitiveSettings settings,
    VertexPositionColorTexture[] vertices,
    int maxVertices)
{
    int count = Math.Min(positions.Length, maxVertices / 2);
    
    for (int i = 0; i < count; i++)
    {
        float progress = (float)i / (count - 1);
        float width = settings.WidthFunction(progress);
        Color color = settings.ColorFunction(progress).WithoutAlpha();
        
        // Calculate direction (average of neighbors for smoothness)
        Vector2 dir;
        if (i == 0)
            dir = (positions[1] - positions[0]).SafeNormalize(Vector2.UnitY);
        else if (i == count - 1)
            dir = (positions[i] - positions[i - 1]).SafeNormalize(Vector2.UnitY);
        else
            dir = (positions[i + 1] - positions[i - 1]).SafeNormalize(Vector2.UnitY);
        
        Vector2 perp = new Vector2(-dir.Y, dir.X);
        Vector2 pos = positions[i] - Main.screenPosition;
        
        // Apply offset if provided
        if (settings.OffsetFunction != null)
            pos += settings.OffsetFunction(progress);
        
        // Top and bottom vertices
        Vector2 top = pos + perp * width * 0.5f;
        Vector2 bot = pos - perp * width * 0.5f;
        
        vertices[i * 2] = new VertexPositionColorTexture(
            new Vector3(top, 0), color, new Vector2(progress, 0));
        vertices[i * 2 + 1] = new VertexPositionColorTexture(
            new Vector3(bot, 0), color, new Vector2(progress, 1));
    }
}
```

### Catmull-Rom Trail Smoothing

```csharp
public static List<Vector2> SmoothTrail(List<Vector2> positions, int resolution = 50)
{
    if (positions.Count < 4)
        return positions;
    
    List<Vector2> result = new List<Vector2>(resolution);
    
    for (int i = 0; i < resolution; i++)
    {
        float t = (float)i / (resolution - 1) * (positions.Count - 1);
        int segment = (int)t;
        float localT = t - segment;
        
        // Clamp indices
        int p0 = Math.Max(0, segment - 1);
        int p1 = segment;
        int p2 = Math.Min(positions.Count - 1, segment + 1);
        int p3 = Math.Min(positions.Count - 1, segment + 2);
        
        result.Add(CatmullRom(
            positions[p0], positions[p1],
            positions[p2], positions[p3], localT));
    }
    
    return result;
}

private static Vector2 CatmullRom(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
{
    float t2 = t * t;
    float t3 = t2 * t;
    
    return 0.5f * (
        2f * p1 +
        (-p0 + p2) * t +
        (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
        (-p0 + 3f * p1 - 3f * p2 + p3) * t3
    );
}
```

---

## 6. Multi-Layer Bloom Stacking

### The FargosSoulsDLC Bloom Pattern

```csharp
/// <summary>
/// Standard 4-layer bloom stack for high-quality glow effects.
/// Each layer gets progressively smaller and brighter.
/// </summary>
public static void DrawBloomStack(
    SpriteBatch sb,
    Vector2 position,
    Color color,
    float scale = 1f,
    float intensity = 1f)
{
    Texture2D bloom = GetBloomTexture();
    Vector2 origin = bloom.Size() * 0.5f;
    
    // CRITICAL: Remove alpha for additive blending
    Color c = color with { A = 0 };
    
    // Layer 1: Outer glow (largest, most transparent)
    sb.Draw(bloom, position, null, c * 0.30f * intensity,
        0f, origin, scale * 2.0f, SpriteEffects.None, 0f);
    
    // Layer 2: Middle glow
    sb.Draw(bloom, position, null, c * 0.50f * intensity,
        0f, origin, scale * 1.4f, SpriteEffects.None, 0f);
    
    // Layer 3: Inner bloom
    sb.Draw(bloom, position, null, c * 0.70f * intensity,
        0f, origin, scale * 0.9f, SpriteEffects.None, 0f);
    
    // Layer 4: Bright white core
    sb.Draw(bloom, position, null, Color.White with { A = 0 } * 0.85f * intensity,
        0f, origin, scale * 0.4f, SpriteEffects.None, 0f);
}
```

### Bloom Layer Configuration Table

| Layer | Scale | Opacity | Color | Purpose |
|-------|-------|---------|-------|---------|
| 1 (Outer) | 2.0x | 0.30 | Theme color | Soft ambient glow |
| 2 (Middle) | 1.4x | 0.50 | Theme color | Main bloom body |
| 3 (Inner) | 0.9x | 0.70 | Theme color | Concentrated glow |
| 4 (Core) | 0.4x | 0.85 | White | Hot bright center |

### Animated Bloom Patterns

```csharp
// Pulsing bloom
float pulse = MathF.Cos(Main.GlobalTimeWrappedHourly * 48f) * 0.1f + 1f;
float animatedScale = baseScale * pulse;

// Charge-up bloom (progress 0 → 1)
float chargeScale = 0.2f + chargeProgress * 1.8f;
float chargeOpacity = 0.3f + chargeProgress * 0.6f;

// Impact/fade-out bloom (progress 0 → 1)
float fadeScale = 0.5f + VFXUtilities.EaseOut(progress, 3f) * 1.5f;
float fadeOpacity = 1f - VFXUtilities.EaseIn(progress, 2f);

// Breathing bloom (slower, organic)
float breath = VFXUtilities.SineBump((time % period) / period);
float breathScale = baseScale * (0.8f + breath * 0.4f);
```

### Shine Flare (4-Point Star)

```csharp
public static void DrawShineFlare(
    SpriteBatch sb,
    Vector2 position,
    Color color,
    float scale,
    float rotation = 0f)
{
    Texture2D flare = GetShineFlareTexture();
    Texture2D bloom = GetBloomTexture();
    Color c = color with { A = 0 };
    
    // Background bloom glow
    sb.Draw(bloom, position, null, c * 0.4f,
        0f, bloom.Size() * 0.5f, scale * 1.5f, SpriteEffects.None, 0f);
    
    // Main flare (4-point star)
    sb.Draw(flare, position, null, c,
        rotation, flare.Size() * 0.5f, scale, SpriteEffects.None, 0f);
    
    // Bright core
    sb.Draw(bloom, position, null, Color.White with { A = 0 } * 0.8f,
        0f, bloom.Size() * 0.5f, scale * 0.3f, SpriteEffects.None, 0f);
}
```

---

## 7. HLSL Shader Patterns

### QuadraticBump Function (Universal Fade Pattern)

```hlsl
// The most important shader utility function
// Input: 0.0 → Output: 0.0
// Input: 0.5 → Output: 1.0 (peak)
// Input: 1.0 → Output: 0.0
float QuadraticBump(float x)
{
    return x * (4.0 - x * 4.0);
}

// Usage: Trail/beam that's bright in the middle, fades at edges
float edgeFade = QuadraticBump(uv.y); // UV.y goes 0→1 across width
return color * edgeFade;
```

### Standard Trail Shader

```hlsl
sampler uImage0 : register(s0);
float uTime;
float4 uColor;
float uOpacity;

float4 PSMain(float2 uv : TEXCOORD0) : COLOR0
{
    // Get base color
    float4 sampleColor = tex2D(uImage0, uv);
    
    // Apply quadratic bump for edge-to-center fade
    float edgeFade = QuadraticBump(uv.y);
    
    // Progress fade (x = completion ratio along trail)
    float progressFade = 1.0 - uv.x;
    
    // Combine
    float4 result = sampleColor * uColor;
    result.a *= edgeFade * progressFade * uOpacity;
    
    // Remove alpha channel for additive
    result.a = 0;
    
    return result;
}
```

### Gradient/Palette Lerping

```hlsl
// Lerp through a color palette based on progress
uniform float4 uPalette[8];
uniform int uPaletteSize;

float4 PaletteLerp(float progress)
{
    float scaled = progress * (uPaletteSize - 1);
    int index1 = (int)scaled;
    int index2 = min(index1 + 1, uPaletteSize - 1);
    float t = frac(scaled);
    
    return lerp(uPalette[index1], uPalette[index2], t);
}
```

### Noise-Based Effects

```hlsl
sampler noiseTexture : register(s1);

float4 PSNoise(float2 uv : TEXCOORD0) : COLOR0
{
    // Sample noise with animation
    float2 noiseUV = uv + float2(uTime * 0.1, uTime * 0.05);
    float noise = tex2D(noiseTexture, noiseUV).r;
    
    // Use noise to modulate alpha
    float alpha = smoothstep(0.3, 0.7, noise);
    
    return uColor * alpha;
}
```

---

## 8. Width & Color Function Patterns

### Width Functions

```csharp
// 1. Linear Taper (most common)
// Thick at start, thin at end
WidthFunction LinearTaper(float startWidth)
    => progress => startWidth * (1f - progress);

// 2. Quadratic Bump (thick middle)
// Thin at both ends, thick in middle - great for energy beams
WidthFunction QuadraticBumpWidth(float maxWidth)
    => progress => maxWidth * MathF.Sin(progress * MathHelper.Pi);

// 3. InverseLerp with Bump (FargosSoulsDLC pattern)
// Ramps up early, holds, tapers at end
WidthFunction InverseLerpBumpWidth(float min, float max)
    => progress =>
    {
        float t = VFXUtilities.InverseLerp(0.06f, 0.27f, progress);
        t *= VFXUtilities.InverseLerp(0.9f, 0.72f, progress);
        return MathHelper.Lerp(min, max, t);
    };

// 4. Constant with fade
WidthFunction ConstantFade(float width, float fadeStart = 0.8f)
    => progress =>
    {
        if (progress < fadeStart) return width;
        float fade = (progress - fadeStart) / (1f - fadeStart);
        return width * (1f - fade);
    };
```

### Color Functions

```csharp
// 1. Solid fade
ColorFunction SolidFade(Color c, float opacity = 1f)
    => progress => c * opacity * (1f - progress);

// 2. Two-color gradient
ColorFunction Gradient(Color start, Color end)
    => progress => Color.Lerp(start, end, progress);

// 3. Palette gradient
ColorFunction PaletteGradient(Color[] palette)
    => progress => VFXUtilities.PaletteLerp(palette, progress);

// 4. InverseLerp bump (bright middle, fades at edges)
ColorFunction BumpOpacity(Color c)
    => progress =>
    {
        float fade = VFXUtilities.InverseLerpBump(0.02f, 0.05f, 0.81f, 0.95f, progress);
        return c * fade;
    };

// 5. Hue shift
ColorFunction HueShift(float startHue, float endHue, float saturation = 1f)
    => progress =>
    {
        float hue = MathHelper.Lerp(startHue, endHue, progress);
        return Main.hslToRgb(hue, saturation, 0.7f);
    };
```

---

## 9. Interpolated Rendering (144Hz+)

### Why Interpolation Matters

At 60 FPS game logic with 144+ Hz display:
- Objects move in 60 FPS discrete steps
- Display refreshes 2-3x per logic frame
- Without interpolation: visible stuttering
- With interpolation: buttery smooth movement

### PartialTicks Pattern

```csharp
public static class InterpolatedRenderer
{
    // Calculated each frame by tModLoader
    public static float PartialTicks => Main.GlobalTimeWrappedHourly % 1f;
    
    /// <summary>
    /// Get smoothly interpolated position between last and current.
    /// </summary>
    public static Vector2 GetInterpolatedPosition(Vector2 previous, Vector2 current)
    {
        return Vector2.Lerp(previous, current, PartialTicks);
    }
    
    /// <summary>
    /// Get interpolated center for a projectile.
    /// </summary>
    public static Vector2 GetInterpolatedCenter(Projectile proj)
    {
        // Uses projectile's internal interpolation if available
        return proj.Center;
    }
    
    /// <summary>
    /// Interpolate a rotation value.
    /// </summary>
    public static float GetInterpolatedRotation(float previous, float current)
    {
        return MathHelper.Lerp(previous, current, PartialTicks);
    }
}
```

### Implementing in Projectile

```csharp
public class SmoothProjectile : ModProjectile
{
    private Vector2 _previousPosition;
    private float _previousRotation;
    
    public override void AI()
    {
        // Store current as previous BEFORE updating
        _previousPosition = Projectile.Center;
        _previousRotation = Projectile.rotation;
        
        // Normal AI logic...
    }
    
    public override bool PreDraw(ref Color lightColor)
    {
        // Use interpolated values for drawing
        Vector2 drawPos = InterpolatedRenderer.GetInterpolatedPosition(
            _previousPosition, Projectile.Center) - Main.screenPosition;
        float drawRot = InterpolatedRenderer.GetInterpolatedRotation(
            _previousRotation, Projectile.rotation);
        
        // Draw at interpolated position
        Main.spriteBatch.Draw(texture, drawPos, null, lightColor, 
            drawRot, origin, scale, SpriteEffects.None, 0f);
        
        return false;
    }
}
```

---

## 10. Screen Effects & Post-Processing

### Screen Shake

```csharp
public static class MagnumScreenEffects
{
    private static float _shakeIntensity;
    private static float _shakeDuration;
    private static int _shakeTimer;
    
    public static void AddScreenShake(float intensity, int duration = 10)
    {
        _shakeIntensity = Math.Max(_shakeIntensity, intensity);
        _shakeDuration = Math.Max(_shakeDuration, duration);
        _shakeTimer = 0;
    }
    
    public static Vector2 GetShakeOffset()
    {
        if (_shakeIntensity <= 0) return Vector2.Zero;
        
        float progress = (float)_shakeTimer / _shakeDuration;
        float currentIntensity = _shakeIntensity * (1f - progress);
        
        return new Vector2(
            Main.rand.NextFloat(-1f, 1f) * currentIntensity,
            Main.rand.NextFloat(-1f, 1f) * currentIntensity
        );
    }
}
```

### Chromatic Aberration

```csharp
public static void DrawChromaticAberration(
    SpriteBatch sb, 
    Texture2D scene,
    float intensity,
    Vector2 center)
{
    Vector2 offset = (center - new Vector2(Main.screenWidth, Main.screenHeight) * 0.5f);
    offset.Normalize();
    offset *= intensity;
    
    // Red channel - offset toward center
    sb.Draw(scene, Vector2.Zero - offset, Color.Red);
    
    // Green channel - no offset
    sb.Draw(scene, Vector2.Zero, Color.Green);
    
    // Blue channel - offset away from center
    sb.Draw(scene, Vector2.Zero + offset, Color.Blue);
}
```

### Screen Distortion (Ripple)

```csharp
// In shader:
float2 distortionCenter;
float distortionRadius;
float distortionStrength;

float4 PSDistort(float2 uv : TEXCOORD0) : COLOR0
{
    float2 toCenter = distortionCenter - uv;
    float dist = length(toCenter);
    
    if (dist < distortionRadius)
    {
        float ripple = sin(dist * 20.0 - uTime * 5.0);
        float falloff = 1.0 - (dist / distortionRadius);
        uv += normalize(toCenter) * ripple * distortionStrength * falloff;
    }
    
    return tex2D(sceneTexture, uv);
}
```

---

## 11. Particle System Optimization

### Object Pooling

```csharp
public class ParticlePool<T> where T : Particle, new()
{
    private readonly Queue<T> _pool = new Queue<T>();
    private readonly int _maxPoolSize;
    
    public ParticlePool(int maxSize = 500)
    {
        _maxPoolSize = maxSize;
    }
    
    public T Get()
    {
        if (_pool.Count > 0)
        {
            T particle = _pool.Dequeue();
            particle.Reset();
            return particle;
        }
        return new T();
    }
    
    public void Return(T particle)
    {
        if (_pool.Count < _maxPoolSize)
        {
            _pool.Enqueue(particle);
        }
    }
}
```

### Batched Rendering by BlendState

```csharp
public void Draw(SpriteBatch sb)
{
    // Sort particles by blend mode
    var additiveParticles = _particles.Where(p => p.BlendMode == BlendState.Additive);
    var alphaParticles = _particles.Where(p => p.BlendMode == BlendState.AlphaBlend);
    
    // Batch 1: All additive particles
    sb.End();
    sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, ...);
    foreach (var p in additiveParticles)
        p.Draw(sb);
    
    // Batch 2: All alpha-blend particles
    sb.End();
    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, ...);
    foreach (var p in alphaParticles)
        p.Draw(sb);
}
```

### Culling Off-Screen Particles

```csharp
public bool IsOnScreen(Vector2 worldPosition, float buffer = 100f)
{
    Vector2 screenPos = worldPosition - Main.screenPosition;
    return screenPos.X > -buffer && 
           screenPos.X < Main.screenWidth + buffer &&
           screenPos.Y > -buffer && 
           screenPos.Y < Main.screenHeight + buffer;
}

public void Update()
{
    for (int i = _particles.Count - 1; i >= 0; i--)
    {
        var p = _particles[i];
        
        // Skip off-screen particles (but still age them)
        if (!IsOnScreen(p.Position, 200f))
        {
            p.Age++;
            continue;
        }
        
        p.Update();
        
        if (p.IsDead)
        {
            _pool.Return(p);
            _particles.RemoveAt(i);
        }
    }
}
```

---

## 12. Implementation Reference

### File Locations in MagnumOpus

| System | Path |
|--------|------|
| Shader Loader | `Common/Systems/Shaders/ShaderLoader.cs` |
| Shader System | `Common/Systems/Shaders/MagnumShaderSystem.cs` |
| Trail Renderer | `Common/Systems/VFX/Trails/EnhancedTrailRenderer.cs` |
| Bloom Renderer | `Common/Systems/VFX/Bloom/BloomRenderer.cs` |
| Particle Handler | `Common/Systems/Particles/MagnumParticleHandler.cs` |
| VFX Utilities | `Common/Systems/VFX/Core/VFXUtilities.cs` |
| Theme Palettes | `Common/Systems/VFX/Core/MagnumThemePalettes.cs` |
| Texture Registry | `Common/Systems/VFX/Core/MagnumTextureRegistry.cs` |
| SpriteBatch Manager | `Common/Systems/VFX/Core/SpriteBatchStateManager.cs` |
| Screen Effects | `Common/Systems/VFX/Screen/` |

### HLSL Shader Sources

| Shader | Path | Status |
|--------|------|--------|
| Trail Shader | `ShaderSource/AdvancedTrailShader.fx` | Uncompiled |
| Bloom Shader | `ShaderSource/AdvancedBloomShader.fx` | Uncompiled |
| Metaball Shader | `ShaderSource/MetaballEdgeShader.fx` | Uncompiled |
| Fire Shader | `ShaderSource/CalamityFireShader.fx` | Uncompiled |
| Screen FX | `ShaderSource/ScreenFX.fx` | Uncompiled |

### Quick Reference Patterns

```csharp
// ALWAYS use for additive bloom
Color glowColor = baseColor with { A = 0 };

// Standard bloom stack call
BloomRenderer.DrawBloomStack(spriteBatch, worldPos, color, scale);

// Multi-pass trail rendering
EnhancedTrailRenderer.RenderMultiPassTrail(positions, widthFunc, colorFunc);

// Theme-based effects
var palette = MagnumThemePalettes.GetPalette("Fate");
var gradientColor = VFXUtilities.PaletteLerp(palette, progress);

// Interpolated position for smooth drawing
Vector2 drawPos = InterpolatedRenderer.GetInterpolatedPosition(previous, current);

// SpriteBatch state preservation
using (new SpriteBatchScope(Main.spriteBatch))
{
    Main.spriteBatch.Begin(BlendState.Additive, ...);
    // Draw effects
    // Auto-restores previous state on dispose
}
```

---

## 13. New Systems Created

### RenderTargetPool (`Common/Systems/VFX/Core/RenderTargetPool.cs`)

Unified render target management system following FargosSoulsDLC patterns.

**Features:**
- **Transient Pool**: Size-hashed pooling for temporary render targets
- **Persistent Targets**: Named targets that survive across frames
- **Resolution Change Handling**: Auto-recreates targets when window resizes
- **TransientTargetScope**: IDisposable pattern for auto-return to pool

```csharp
// Transient usage (auto-returns on dispose)
using (RenderTargetPool.TransientTargetScope(width, height, out var target))
{
    RenderTargetPool.SetAndClear(target, Color.Transparent);
    // Draw to target
    RenderTargetPool.RestoreBackBuffer();
    // Draw target to screen
}

// Persistent usage
var persistentTarget = RenderTargetPool.GetPersistent("MyEffect", width, height);
```

### PixelatedTrailRenderer (`Common/Systems/VFX/Trails/PixelatedTrailRenderer.cs`)

Pixelated primitive trail rendering for that classic Terraria aesthetic.

**Features:**
- Draws trails to low-resolution render target
- Upscales with nearest-neighbor filtering (SamplerState.PointClamp)
- Multi-pass bloom support with pixelation
- Automatic interpolation for 144Hz+ smooth rendering
- Catmull-Rom spline smoothing

```csharp
// Basic pixelated trail
PixelatedTrailRenderer.DrawPixelatedTrail(
    Projectile.oldPos,
    progress => 20f * (1f - progress),  // Width function
    progress => Color.Cyan * (1f - progress),  // Color function
    pixelScale: 2  // 2x pixelation (half resolution)
);

// Pixelated trail with bloom
PixelatedTrailRenderer.DrawPixelatedBloomTrail(
    Projectile.oldPos,
    widthFunc, colorFunc,
    pixelScale: 2,
    bloomMultiplier: 2.5f
);

// Themed projectile trail
PixelatedTrailRenderer.DrawThemedPixelatedTrail(
    Projectile,
    themeName: "Fate",
    width: 16f,
    pixelScale: 2
);
```

---

## Summary

This document captures the complete VFX knowledge base from:
- **MonoGame API**: GraphicsDevice, BlendState, RenderTarget2D, Effect
- **FargosSoulsDLC Patterns**: Multi-layer bloom, { A = 0 }, PrimitiveSettings
- **Calamity Patterns**: Trail rendering, metaballs, screen effects
- **HLSL Shaders**: QuadraticBump, palette lerping, noise effects
- **Optimization**: Object pooling, batched rendering, culling

Use this as a reference when implementing or enhancing VFX systems.

---

*Last Updated: Documentation generated from research phase*
