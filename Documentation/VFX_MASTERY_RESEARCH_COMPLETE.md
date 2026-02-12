# VFX Mastery Research - Complete Knowledge Base

> **COMPREHENSIVE DOCUMENTATION OF ALL VFX PATTERNS, TECHNIQUES, AND SYSTEMS**
> 
> This document captures everything learned from MonoGame, tModLoader, HLSL shaders,
> FargosSoulsDLC patterns, and Calamity Mod techniques.
>
> **STATUS: ENHANCED** - All core VFX systems have been reviewed and enhanced.
>
> ---
>
> ## 📖 Related Documentation
>
> | Document | Purpose |
> |----------|---------|
> | **[VFX_CORE_CONCEPTS_PART2.md](VFX_CORE_CONCEPTS_PART2.md)** | Bezier curves, particle architecture, billboarding, GC optimization |
> | **[HLSL_GRAPHICS_DEEP_DIVE.md](HLSL_GRAPHICS_DEEP_DIVE.md)** | Complete HLSL language reference, noise functions, SDFs, color grading, tone mapping, performance optimization |
> | **[Enhanced_VFX_System.md](Guides/Enhanced_VFX_System.md)** | MagnumOpus VFX API usage guide, themed particles, bloom patterns |
> | **[TRUE_VFX_STANDARDS.md](Guides/TRUE_VFX_STANDARDS.md)** | Visual effect quality standards, projectile patterns |
>
> ### Shader Library
>
> | File | Description |
> |------|-------------|
> | `ShaderSource/HLSLLibrary.fxh` | Reusable HLSL utility functions (noise, SDFs, easing, color utilities) |
> | `ShaderSource/AdvancedTrailShader.fx` | 5 trail styles: Flame, Ice, Lightning, Nature, Cosmic |
> | `ShaderSource/AdvancedBloomShader.fx` | 5 bloom styles: Ethereal, Infernal, Celestial, Chromatic, Void |
> | `ShaderSource/AdvancedDistortionShader.fx` | Screen distortions: Ripple, Heat, Chromatic, Eclipse, Reality Tear |
> | `ShaderSource/MetaballEdgeShader.fx` | Sobel edge detection with glow and pulse animation |
>
> ---
> 
> **NEW SYSTEMS CREATED:**
> - `RenderTargetPool.cs` (Common/Systems/VFX/Core/) - Unified render target management with transient/persistent pooling
> - `PixelatedTrailRenderer.cs` (Common/Systems/VFX/Trails/) - FargosSoulsDLC-style pixelated trails with render target support
> - `HLSLLibrary.fxh` (ShaderSource/) - Comprehensive HLSL utility library with noise, SDFs, color utilities, easing functions
> - `DynamicLightSystem.cs` (Common/Systems/VFX/Core/) - Advanced lighting with point/directional/spotlight lights and light cookies
> - `MultiBeamController.cs` (Common/Systems/VFX/Core/) - Synchronized multi-beam management with state machine (Idle/Charging/Firing/Dissipating)
> - `ObjectPool.cs` (Common/Systems/VFX/Core/) - Generic object pooling with IPoolable interface and PoolManager
> - `ParticlePoolSystem.cs` (Common/Systems/VFX/Core/) - High-performance pooled particle system with ParticleSettings presets
> - `FrustumCuller.cs` (Common/Systems/VFX/Core/) - Screen visibility testing, spatial partitioning, and hierarchical culling
> - `BloomPostProcess.cs` (Common/Systems/VFX/Bloom/) - Full-screen bloom post-processing pipeline with render targets
> - `SegmentedBeamRenderer.cs` (Common/Systems/VFX/Beams/) - Three-part beam architecture (Muzzle/Body/Impact) with wave distortion and corona particles
> - `TaperCurves.cs` (Common/Systems/VFX/Core/) - Width tapering functions: Linear, EaseOut, EaseIn, SmoothStep, Exponential, Bulge, Wave, Bezier
> - `GlowRenderer.cs` (Common/Systems/VFX/Bloom/) - Multi-layer glow system with theme-specific profiles (LaCampanella, Eroica, Fate, etc.)
> - `ImpactEffectManager.cs` (Common/Systems/VFX/Effects/) - Staged impact choreography: Anticipation → Impact → Shockwave → Debris → Aftermath
> - `ScreenShakeManager.cs` (Common/Systems/VFX/Effects/) - Camera shake system with multiple decay curves and trauma accumulation
> - `TileDustSpawner.cs` (Common/Systems/VFX/Effects/) - Material-based dust spawning with 14 material types and specialized effects
> - `LODManager.cs` (Common/Systems/VFX/Optimization/) - Level of Detail management with distance-based rendering thresholds (High/Medium/Low/VeryLow/Culled)
> - `LODBeamRenderer.cs` (Common/Systems/VFX/Optimization/) - LOD-aware beam rendering with automatic segment/particle/glow layer adjustment
> - `AdaptiveLODSystem.cs` (Common/Systems/VFX/Optimization/) - ModSystem managing update frequency for ILODUpdatable objects based on camera distance
> - `TextureAtlas.cs` (Common/Systems/VFX/Optimization/) - Texture atlas builder and manager for batch rendering optimization
> - `BatchedParticleRenderer.cs` (Common/Systems/VFX/Optimization/) - Queue-based particle batching with state-sorted rendering
> - `VertexBufferPool.cs` (Common/Systems/VFX/Optimization/) - Reusable vertex buffer pool with RingVertexBuffer for zero-allocation updates
> - `AdaptiveQualityManager.cs` (Common/Systems/VFX/Optimization/) - ModSystem auto-adjusting VFX quality based on frame rate (Ultra/High/Medium/Low/Potato)
> - `ConditionalEffectRenderer.cs` (Common/Systems/VFX/Optimization/) - Conditional effect rendering based on quality settings and visibility
> - `PerformanceProfiler.cs` (Common/Systems/VFX/Optimization/) - Custom profiler with timing, call counts, memory tracking, and ProfileScope pattern
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

## 14. Advanced Systems Reference

### 14.1 DynamicLightSystem

**File:** `Common/Systems/VFX/Core/DynamicLightSystem.cs`

Advanced lighting overlay system with multiple light types:

| Light Type | Description | Use Case |
|------------|-------------|----------|
| **Point** | Radial light with inverse-square falloff | Explosions, glowing projectiles |
| **Directional** | Parallel rays in a direction | Laser beams, god rays |
| **Spotlight** | Cone-shaped with inner/outer angles | Focused effects, boss attacks |

```csharp
// Add a point light
DynamicLightSystem.AddLight(
    position, 
    color, 
    radius: 200f, 
    intensity: 1.5f
);

// Add a directional light
DynamicLightSystem.AddDirectionalLight(
    position, 
    direction, 
    color, 
    length: 400f, 
    width: 50f, 
    intensity: 1.0f
);

// Add a spotlight
DynamicLightSystem.AddSpotlight(
    position, 
    direction, 
    color, 
    range: 300f, 
    innerConeAngle: MathHelper.ToRadians(15f),
    outerConeAngle: MathHelper.ToRadians(30f),
    intensity: 2.0f
);

// Add animated lights
DynamicLightSystem.AddPulsingLight(position, color, radius, baseIntensity, pulseAmount, pulseSpeed);
DynamicLightSystem.AddFlickeringLight(position, color, radius, intensity, flickerSpeed);

// Light cookies (textured lights)
Texture2D cookie = LightCookie.GenerateSoftCircle(device, 64);
Texture2D caustics = LightCookie.GenerateCaustics(device, 64);
```

### 14.2 MultiBeamController

**File:** `Common/Systems/VFX/Core/MultiBeamController.cs`

Synchronized multi-beam management with state machines:

**Beam States:**
| State | Description |
|-------|-------------|
| `Idle` | Beam not visible |
| `Charging` | Building up with jitter |
| `Firing` | Full intensity, wave motion |
| `Dissipating` | Fading out |

```csharp
// Create a multi-beam controller
var controller = new MultiBeamController(origin, target, beamCount: 5);
controller.SpreadAngle = MathHelper.ToRadians(60f);
controller.ChargeTime = 1.0f;
controller.DissipateTime = 0.5f;

// State transitions
controller.StartCharging();  // Idle → Charging
controller.StartFiring();    // Charging → Firing
controller.StartDissipating(); // Firing → Dissipating

// Converging beam pattern (beams start spread, converge to target)
var converging = new ConvergingBeamSystem(origin, target, beamCount: 6);
converging.StartSpread = 100f;
converging.ConvergeDuration = 1.0f;

// Pulsing beam array (synchronized pulse animation)
var pulsing = new PulsingBeamArray(origin, target, beamCount: 4);
pulsing.PulseFrequency = 2.0f;
pulsing.PulseAmplitude = 0.3f;
pulsing.PulseOffset = 0.25f; // Phase offset between beams
```

### 14.3 ObjectPool<T>

**File:** `Common/Systems/VFX/Core/ObjectPool.cs`

Generic object pooling to eliminate GC allocations:

```csharp
// Create a pool
var pool = new ObjectPool<MyParticle>(
    initialSize: 100,
    maxSize: 500,
    createFunc: () => new MyParticle(),
    onAcquire: p => p.OnSpawn(),
    onRelease: p => p.OnDespawn()
);

// Get an object (reuses from pool or creates new)
MyParticle particle = pool.Get();

// Return to pool when done
pool.Return(particle);

// IPoolable interface for automatic Reset()
public class MyParticle : IPoolable
{
    public void Reset()
    {
        // Reset state for reuse
    }
}

// PoolManager for centralized pools
PoolManager.RegisterPool<MyParticle>("particles", 100, 500);
MyParticle p = PoolManager.Get<MyParticle>("particles");
PoolManager.Return(p, "particles");
```

### 14.4 ParticlePoolSystem

**File:** `Common/Systems/VFX/Core/ParticlePoolSystem.cs`

High-performance pooled particle system with presets:

```csharp
// Create a particle system
var system = new ParticlePoolSystem(maxParticles: 1000);

// Use presets
system.Spawn(position, velocity, ParticleSettings.Explosion());
system.Spawn(position, velocity, ParticleSettings.Sparkle());
system.Spawn(position, velocity, ParticleSettings.Smoke());
system.Spawn(position, velocity, ParticleSettings.Fire());
system.Spawn(position, velocity, ParticleSettings.MusicNote());
system.Spawn(position, velocity, ParticleSettings.Beam());

// Burst spawn
system.SpawnBurst(center, count: 20, ParticleSettings.Explosion());

// Cone spawn (directional)
system.SpawnCone(position, direction, coneAngle, count, settings);

// Line spawn (between points)
system.SpawnLine(start, end, count, settings);

// Ring spawn (circular)
system.SpawnRing(center, radius, count, settings);

// Update and draw
system.Update();
system.Draw(spriteBatch, texture);
system.DrawAdditive(spriteBatch, texture);
```

### 14.5 FrustumCuller

**File:** `Common/Systems/VFX/Core/FrustumCuller.cs`

Screen visibility testing for VFX optimization:

```csharp
// Simple visibility tests
bool visible = FrustumCuller.IsPointVisible(worldPosition);
bool visible = FrustumCuller.IsRectangleVisible(bounds);
bool visible = FrustumCuller.IsCircleVisible(center, radius);
bool visible = FrustumCuller.IsLineVisible(start, end);
bool visible = FrustumCuller.IsBeamVisible(start, end, width);

// Culled beam renderer
var renderer = new CulledBeamRenderer();
renderer.AddBeam(start, end, width, color);
renderer.Draw(spriteBatch);

// Statistics
float ratio = renderer.CullRatio; // 0.0 = all visible, 1.0 = all culled

// Hierarchical culling (spatial partitioning)
var culler = new HierarchicalCuller<MyBeam>(cellSize: 256);
culler.Insert(beam, bounds);
culler.Update(); // Must call after adding all items

// Get visible items
foreach (var beam in culler.GetVisibleItems())
{
    beam.Draw(spriteBatch);
}
```

### 14.6 BloomPostProcess

**File:** `Common/Systems/VFX/Bloom/BloomPostProcess.cs`

Full-screen bloom post-processing pipeline:

```csharp
// Initialize (call once)
BloomPostProcess.Initialize(graphicsDevice, screenWidth, screenHeight);

// Configure settings
BloomPostProcess.Threshold = 0.8f;    // Brightness threshold (0-1)
BloomPostProcess.Intensity = 1.5f;     // Bloom brightness
BloomPostProcess.BlurSize = 2.0f;      // Blur kernel size
BloomPostProcess.BlurPasses = 2;       // Number of blur iterations
BloomPostProcess.ResolutionDivisor = 2; // Downscale factor (performance)

// Apply bloom (wrap your scene rendering)
BloomPostProcess.Apply(spriteBatch, () =>
{
    // Draw your scene here
    DrawScene();
});

// Or apply with custom shader
BloomPostProcess.ApplyWithShader(spriteBatch, customEffect, () =>
{
    DrawScene();
});

// Alternative: Kawase blur (faster, softer)
BloomPostProcess.ApplyKawaseBlur(spriteBatch, iterations: 4, () =>
{
    DrawScene();
});

// Handle window resize
BloomPostProcess.Resize(newWidth, newHeight);

// Cleanup
BloomPostProcess.Dispose();
```

**Bloom Pipeline:**
1. Capture scene to render target
2. Extract bright pixels (threshold pass)
3. Horizontal Gaussian blur
4. Vertical Gaussian blur
5. Composite with additive blending

---

## 15. Beam, Glow, Impact & Screen Shake Systems (Parts 3.1-3.7)

### 15.1 SegmentedBeamRenderer

**File:** `Common/Systems/VFX/Beams/SegmentedBeamRenderer.cs`

Three-part beam architecture for high-quality laser/beam effects:

```csharp
// Create a segmented beam
var beam = new SegmentedBeam(
    start: player.Center,
    end: targetPos,
    color: Color.Cyan,
    width: 30f
);

// Draw the beam (calls all three sections)
beam.Draw(Main.spriteBatch);

// Or draw sections individually
beam.Muzzle.Draw(Main.spriteBatch);
beam.Body.Draw(Main.spriteBatch);
beam.Impact.Draw(Main.spriteBatch);

// Animate over time
beam.Update();  // Advances waveOffset, coronaAngle, etc.
```

**Visual Hierarchy (Brightness):**
| Section | Purpose | Brightness |
|---------|---------|------------|
| Muzzle (Start) | Energy emission, corona effect | 100% |
| Body (Middle) | Main beam with wave distortion | 60% |
| Impact (End) | Hit point with spark particles | 80% |

**Key Features:**
- **Wave Distortion:** Sine-wave vertical offset for organic feel
- **Corona Effect:** Rotating energy particles around muzzle
- **UV Scrolling:** Body texture scrolls for flow animation
- **Spark Particles:** Sparks emit from impact point

### 15.2 TaperCurves

**File:** `Common/Systems/VFX/Core/TaperCurves.cs`

Width tapering functions for beams, trails, and effects:

```csharp
// Basic taper functions (progress 0→1)
float width = TaperCurves.Linear(progress) * baseWidth;      // Constant
float width = TaperCurves.EaseOut(progress) * baseWidth;     // Fast start, slow end
float width = TaperCurves.EaseIn(progress) * baseWidth;      // Slow start, fast end
float width = TaperCurves.SmoothStep(progress) * baseWidth;  // S-curve
float width = TaperCurves.Exponential(progress, 2f) * baseWidth; // Power curve
float width = TaperCurves.Bulge(progress, 0.5f) * baseWidth; // Thick middle
float width = TaperCurves.Wave(progress, 3) * baseWidth;     // Sinusoidal ripples
float width = TaperCurves.BezierTaper(progress, 1f, 1.2f, 0.8f, 0f) * baseWidth; // Custom curve

// FargosSoulsDLC-style InverseLerpBump (peaks in middle)
float opacity = TaperCurves.InverseLerpBump(0.02f, 0.15f, 0.85f, 0.98f, progress);

// Dynamic modulation
float modulated = TaperCurves.ApplyPulse(width, time, amplitude: 0.1f, frequency: 5f);
float jittered = TaperCurves.ApplyJitter(width, Main.rand, intensity: 0.05f);

// Use preset via enum
Func<float, float> taper = TaperCurves.GetTaperFunction(TaperType.EaseOut);
```

**TaperType Enum:**
`Linear`, `EaseIn`, `EaseOut`, `SmoothStep`, `Exponential`, `Bulge`, `Wave`, `InverseLerpBump`

### 15.3 GlowRenderer

**File:** `Common/Systems/VFX/Bloom/GlowRenderer.cs`

Multi-layer glow system with predefined profiles:

```csharp
// Basic multi-layer glow
GlowRenderer.DrawLayeredGlow(
    spriteBatch,
    texture,
    position,
    baseColor,
    scale: 1f,
    GlowRenderer.SoftProfile  // Predefined layer configuration
);

// Theme-specific glow effects
GlowRenderer.DrawLaCampanellaGlow(spriteBatch, texture, position, scale, intensity);
GlowRenderer.DrawEroicaGlow(spriteBatch, texture, position, scale, intensity);
GlowRenderer.DrawFateGlow(spriteBatch, texture, position, scale, intensity);
GlowRenderer.DrawMoonlightGlow(spriteBatch, texture, position, scale, intensity);
GlowRenderer.DrawSwanLakeGlow(spriteBatch, texture, position, scale, intensity);
GlowRenderer.DrawEnigmaGlow(spriteBatch, texture, position, scale, intensity);

// Animated glow variants
GlowRenderer.DrawPulsingGlow(spriteBatch, texture, position, color, scale, time);
GlowRenderer.DrawChargeGlow(spriteBatch, texture, position, color, scale, progress); // 0→1 charge
GlowRenderer.DrawFadingGlow(spriteBatch, texture, position, color, scale, progress); // Fade out
GlowRenderer.DrawImpactGlow(spriteBatch, texture, position, color, scale, progress); // Expand + fade
```

**Predefined Profiles:**
| Profile | Layers | Use Case |
|---------|--------|----------|
| `SoftProfile` | 4 layers, wide spread | Ambient glows, magic effects |
| `SharpProfile` | 4 layers, tight core | Bright points, stars |
| `EnergyBeamProfile` | 6 layers, elongated | Laser/beam centers |
| `ExplosionProfile` | 5 layers, rapid falloff | Impact explosions |
| `CosmicProfile` | 5 layers, deep colors | Fate/celestial effects |
| `InfernalProfile` | 5 layers, warm tints | La Campanella fire effects |
| `HeroicProfile` | 5 layers, gold tints | Eroica triumphant effects |

### 15.4 ImpactEffectManager

**File:** `Common/Systems/VFX/Effects/ImpactEffectManager.cs`

Choreographed multi-stage impact effects:

```csharp
// Spawn a basic impact effect
ImpactEffectManager.SpawnImpact(position, color, intensity: 1.5f);

// Spawn themed impacts
ImpactEffectManager.SpawnLaCampanellaImpact(position, intensity);
ImpactEffectManager.SpawnEroicaImpact(position, intensity);
ImpactEffectManager.SpawnFateImpact(position, intensity);
ImpactEffectManager.SpawnMoonlightImpact(position, intensity);
ImpactEffectManager.SpawnSwanLakeImpact(position, intensity);
ImpactEffectManager.SpawnEnigmaImpact(position, intensity);

// Custom impact configuration
var impact = new ImpactEffect(position, color, intensity)
{
    ShockwaveSpeed = 200f,
    ShockwaveRings = 3,
    SparkCount = 20,
    DebrisCount = 15,
    SmokeCount = 10,
    ScreenShakeIntensity = 5f
};
ImpactEffectManager.AddEffect(impact);
```

**Impact Stages (Timeline):**
| Stage | Frames | Description |
|-------|--------|-------------|
| Anticipation | 0-5 | Converging particles, energy buildup |
| Impact | 5-10 | Flash, core explosion |
| Shockwave | 10-20 | Expanding ring |
| Debris | 20-60 | Sparks, fragments settling |
| Aftermath | 60+ | Smoke dissipation |

### 15.5 ScreenShakeManager

**File:** `Common/Systems/VFX/Effects/ScreenShakeManager.cs`

Camera shake system with trauma accumulation:

```csharp
// Basic shake
ScreenShakeManager.Instance.AddShake(intensity: 10f, duration: 30);

// Impact shake (sharp attack, quick decay)
ScreenShakeManager.Instance.AddImpactShake(position, intensity: 15f);

// Explosion shake (builds then decays)
ScreenShakeManager.Instance.AddExplosionShake(position, intensity: 20f, radius: 500f);

// Directional shake (pushes camera in direction)
ScreenShakeManager.Instance.AddDirectionalShake(direction, intensity: 8f, duration: 20);

// Continuous rumble
ScreenShakeManager.Instance.AddRumbleShake(intensity: 3f, duration: 120);

// Trauma system (accumulates and affects intensity)
ScreenShakeManager.Instance.AddTrauma(0.3f);  // 0-1 scale

// Get current shake offset (apply in ModifyTransformMatrix)
Vector2 offset = ScreenShakeManager.Instance.GetOffset();
```

**Decay Curves:**
`Linear`, `Exponential`, `EaseIn`, `EaseOut`, `Bounce`

### 15.6 TileDustSpawner

**File:** `Common/Systems/VFX/Effects/TileDustSpawner.cs`

Material-based dust/debris interaction system:

```csharp
// Spawn impact dust at world position (auto-detects material)
TileDustSpawner.SpawnImpactDust(worldPosition, velocity, count: 8);

// Spawn trail dust (for moving projectiles)
TileDustSpawner.SpawnTrailDust(worldPosition, velocity, interval: 3);

// Spawn beam trail (for laser effects hitting surfaces)
TileDustSpawner.SpawnBeamTrailDust(worldPosition, beamDirection, intensity: 0.5f);

// Spawn destruction debris (for breaking tiles)
TileDustSpawner.SpawnDestructionDebris(worldPosition, count: 20);

// Tile raycast (find collision point)
if (TileDustSpawner.TileRaycast(start, end, out Vector2 hitPoint, out Vector2 hitNormal, out Tile hitTile))
{
    // Hit detected
    TileDustSpawner.SpawnImpactDust(hitPoint, -hitNormal * 5f, 12);
}

// Get material type for a tile
MaterialType material = TileDustSpawner.GetMaterialType(tile);
```

**Material Types (14):**
| Material | Special Effects |
|----------|----------------|
| `Stone` | Gray dust, rock fragments |
| `Dirt` | Brown dust, soil particles |
| `Wood` | Wood splinters, sawdust |
| `Metal` | Sparks, metallic particles |
| `Ice` | Ice mist, crystal shards |
| `Flesh` | Red particles |
| `Crystal` | Shimmer sparkles |
| `Sand` | Sandy particles |
| `Glass` | Glass shards |
| `Corruption` | Evil purple dust |
| `Crimson` | Crimson red dust |
| `Hallow` | Sparkle effects |
| `Jungle` | Green particles |
| `Lava` | Ember particles |

---

## 16. Optimization Systems (Part 4 Continued)

### 16.1 LODManager (Level of Detail)

**File:** `Common/Systems/VFX/Optimization/LODManager.cs`

Distance-based LOD system for automatic detail scaling:

```csharp
// Get LOD level based on camera distance
LODLevel level = LODManager.GetLODLevel(worldPosition);

// LOD levels and their distance thresholds
public enum LODLevel
{
    High,     // 0-400 pixels: Full quality
    Medium,   // 400-800 pixels: Reduced quality
    Low,      // 800-1200 pixels: Minimal quality
    VeryLow,  // 1200-1600 pixels: Bare minimum
    Culled    // 1600+ pixels: Don't render
}

// Get quality multipliers
float quality = LODManager.GetQualityMultiplier(level);  // 1.0, 0.6, 0.3, 0.1
int updateFreq = LODManager.GetUpdateFrequency(level);   // 1, 2, 4, 8 frames
int segments = LODManager.GetSegmentCount(level, 20);    // Scales beam segments
int particles = LODManager.GetParticleCount(level, 50);  // Scales particle counts

// Blend between LOD levels for smooth transitions
float blend = LODManager.GetLODBlendFactor(worldPosition);
```

### 16.2 LODBeamRenderer

**File:** `Common/Systems/VFX/Optimization/LODBeamRenderer.cs`

LOD-aware beam rendering with automatic detail adjustment:

```csharp
// Create LOD-aware beam configuration
var config = BeamLODConfig.High;  // Preset configs: High, Medium, Low, VeryLow

// Or create custom config
var config = new BeamLODConfig
{
    SegmentCount = 20,
    ParticleCount = 50,
    GlowLayers = 4,
    ShaderQuality = 1.0f
};

// Draw LOD-aware beam
LODBeamRenderer.DrawBeamWithLOD(
    spriteBatch: Main.spriteBatch,
    start: startPosition,
    end: endPosition,
    width: 30f,
    color: Color.Cyan,
    glowTexture: glowTex
);

// Access config presets
BeamLODConfig highConfig = BeamLODConfig.High;   // 20 segments, 50 particles, 4 glow layers
BeamLODConfig medConfig = BeamLODConfig.Medium;  // 12 segments, 25 particles, 3 glow layers
BeamLODConfig lowConfig = BeamLODConfig.Low;     // 6 segments, 10 particles, 2 glow layers
```

### 16.3 AdaptiveLODSystem

**File:** `Common/Systems/VFX/Optimization/AdaptiveLODSystem.cs`

ModSystem that manages update frequency for registered objects:

```csharp
// Implement ILODUpdatable on your VFX object
public class MyBeamEffect : ILODUpdatable
{
    public void Update() { /* Update logic */ }
    public Vector2 GetPosition() => position;
    public bool NeedsUpdateWhenInvisible() => false;
}

// Register objects for automatic LOD-based updates
AdaptiveLODSystem.Register(myEffect);

// Statistics
int registeredCount = AdaptiveLODSystem.RegisteredCount;
int updatedThisFrame = AdaptiveLODSystem.UpdatedThisFrame;
int skippedThisFrame = AdaptiveLODSystem.SkippedThisFrame;
```

### 16.4 TextureAtlas

**File:** `Common/Systems/VFX/Optimization/TextureAtlas.cs`

Texture atlas for batch rendering optimization:

```csharp
// Build atlas from multiple textures
var builder = new AtlasBuilder();
builder.AddTexture("Glow", glowTexture);
builder.AddTexture("Spark", sparkTexture);
builder.AddTexture("Flare", flareTexture);
TextureAtlas atlas = builder.Build(graphicsDevice);

// Use atlas for batched drawing
atlas.Draw(
    spriteBatch,
    regionName: "Glow",
    position: drawPosition,
    color: Color.White,
    rotation: 0f,
    scale: Vector2.One
);

// Get UV region for custom rendering
Rectangle region = atlas.GetRegion("Spark");
```

### 16.5 BatchedParticleRenderer

**File:** `Common/Systems/VFX/Optimization/BatchedParticleRenderer.cs`

Queue-based particle batching with single draw call:

```csharp
// Create batched renderer with atlas
var renderer = new BatchedParticleRenderer(atlas);

// Queue particles (no draw call yet)
renderer.Queue("Glow", position, color, rotation, scale);
renderer.Queue("Spark", position2, color2);

// Burst helper (queues multiple particles)
renderer.QueueBurst("Glow", center, 20, radius, color);

// Line helper (particles along a line)
renderer.QueueLine("Spark", start, end, 10, color);

// Flush all queued particles in one draw call
renderer.Flush(Main.spriteBatch);

// State-sorted renderer (minimizes state changes)
var sortedRenderer = new StateSortedRenderer();
sortedRenderer.Queue(texture1, position1, color1, BlendState.Additive);
sortedRenderer.Queue(texture2, position2, color2, BlendState.Additive);
sortedRenderer.Queue(texture3, position3, color3, BlendState.AlphaBlend);
sortedRenderer.Flush(Main.spriteBatch);  // Groups by blend state automatically
```

### 16.6 VertexBufferPool

**File:** `Common/Systems/VFX/Optimization/VertexBufferPool.cs`

Reusable vertex buffer pool to minimize GPU allocations:

```csharp
// Create pool for specific vertex type
var pool = new VertexBufferPool<VertexPositionColorTexture>(
    graphicsDevice,
    initialCapacity: 1024,
    maxCapacity: 10000
);

// Rent buffer (returns to pool when done)
var buffer = pool.Rent(vertexCount);

// Use the buffer
buffer.SetData(vertices);
graphicsDevice.SetVertexBuffer(buffer);
graphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, vertexCount - 2);

// Return to pool
pool.Return(buffer);

// Ring buffer for streaming data (zero-allocation updates)
var ringBuffer = new RingVertexBuffer<VertexPositionColorTexture>(graphicsDevice, 4096);

// Write vertices with automatic wrap-around
int startVertex = ringBuffer.Write(vertices, vertexCount);

// Draw from ring buffer
graphicsDevice.SetVertexBuffer(ringBuffer.Buffer);
graphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, startVertex, vertexCount - 2);

// Cleanup
pool.TrimExcess();  // Return unused buffers to GC
pool.Dispose();
```

### 16.7 AdaptiveQualityManager

**File:** `Common/Systems/VFX/Optimization/AdaptiveQualityManager.cs`

ModSystem that auto-adjusts VFX quality based on frame rate:

```csharp
// Quality levels
public enum VFXQuality { Ultra, High, Medium, Low, Potato }

// Get current quality level
VFXQuality level = AdaptiveQualityManager.CurrentQuality;

// Quality-adjusted values
int maxParticles = AdaptiveQualityManager.MaxParticles;  // 2000→100 based on quality
int bloomLayers = AdaptiveQualityManager.BloomLayers;    // 5→1 based on quality
float particleQuality = AdaptiveQualityManager.ParticleQuality;  // 1.0→0.1

// Feature toggles (automatically adjusted)
bool enableGlow = AdaptiveQualityManager.EnableGlow;
bool enableShaders = AdaptiveQualityManager.EnableShaders;
bool enableTrails = AdaptiveQualityManager.EnableTrails;
bool enableScreenEffects = AdaptiveQualityManager.EnableScreenEffects;

// Frame rate thresholds (configurable)
// Ultra: 58+ FPS
// High: 50-58 FPS
// Medium: 40-50 FPS
// Low: 30-40 FPS
// Potato: <30 FPS

// Force a quality level (overrides automatic)
AdaptiveQualityManager.ForceQuality(VFXQuality.High);
AdaptiveQualityManager.ClearForce();  // Resume automatic

// Debug info
float fps = AdaptiveQualityManager.CurrentFPS;
float avgFrameTime = AdaptiveQualityManager.AverageFrameTime;
```

### 16.8 ConditionalEffectRenderer

**File:** `Common/Systems/VFX/Optimization/ConditionalEffectRenderer.cs`

Conditional effect rendering based on quality and visibility:

```csharp
// Check if effects should render
bool shouldGlow = ConditionalEffectRenderer.ShouldRenderGlow();
bool shouldSpawnParticles = ConditionalEffectRenderer.ShouldSpawnParticles(worldPosition);
bool shouldBloom = ConditionalEffectRenderer.ShouldApplyBloom();
bool shouldShaders = ConditionalEffectRenderer.ShouldUseShaders();
bool shouldDistortion = ConditionalEffectRenderer.ShouldApplyDistortion();

// Get quality-adjusted counts
int bloomLayers = ConditionalEffectRenderer.GetBloomLayers();  // 1-5 based on quality
int particleCount = ConditionalEffectRenderer.GetAdjustedParticleCount(worldPos, baseCount);

// Get quality-adjusted scale/opacity
float adjustedScale = ConditionalEffectRenderer.GetAdjustedGlowScale(1.0f);
float adjustedOpacity = ConditionalEffectRenderer.GetAdjustedGlowOpacity(1.0f);

// Force toggles for testing
ConditionalEffectRenderer.ForceDisableGlow = true;
ConditionalEffectRenderer.ForceDisableParticles = true;
ConditionalEffectRenderer.ForceDisableBloom = true;
```

### 16.9 PerformanceProfiler

**File:** `Common/Systems/VFX/Optimization/PerformanceProfiler.cs`

Custom profiler for identifying VFX bottlenecks:

```csharp
// Basic timing
PerformanceProfiler.BeginSample("TrailRendering");
// ... do work ...
PerformanceProfiler.EndSample("TrailRendering");

// Using pattern (auto-ends on dispose)
using (new ProfileScope("ParticleUpdate"))
{
    // ... work automatically timed ...
}

// Get statistics
var stats = PerformanceProfiler.GetStatistics("TrailRendering");
double avgMs = stats.AverageMs;
double minMs = stats.MinMs;
double maxMs = stats.MaxMs;
int callCount = stats.CallCount;
double totalMs = stats.TotalMs;

// Memory profiling
MemoryProfiler.BeginSample("ParticleSystem");
// ... allocations ...
MemoryProfiler.EndSample("ParticleSystem");
long allocated = MemoryProfiler.GetAllocated("ParticleSystem");

// GC monitoring
int gcCount = MemoryProfiler.GetGCCount();
long totalMemory = MemoryProfiler.GetTotalMemory();

// Report all statistics
PerformanceProfiler.ReportAll();  // Logs to console
MemoryProfiler.ReportAll();

// Reset for fresh measurements
PerformanceProfiler.Reset();
MemoryProfiler.Reset();
```

### 16.10 Optimization Best Practices

**LOD Guidelines:**
```csharp
// Use LOD for ALL distance-based effects
Vector2 pos = myEffect.Position;
LODLevel lod = LODManager.GetLODLevel(pos);

if (lod == LODLevel.Culled)
    return;  // Don't render

int particles = LODManager.GetParticleCount(lod, baseCount);
int segments = LODManager.GetSegmentCount(lod, baseSegments);
```

**Batching Guidelines:**
```csharp
// Group draws by texture/state
var renderer = new BatchedParticleRenderer(atlas);
foreach (var particle in particles)
    renderer.Queue(particle.Type, particle.Position, particle.Color);
renderer.Flush(spriteBatch);  // Single draw call
```

**Profiling Guidelines:**
```csharp
// Profile hot paths
using (new ProfileScope("HotPath"))
{
    ExpensiveOperation();
}

// Check for problems
if (PerformanceProfiler.GetStatistics("HotPath").AverageMs > 2.0)
    Debug.Log("Performance warning: HotPath taking too long");
```

---

*Last Updated: VFX Parts 3.1-3.7 + Part 4 Optimization Implementation Complete*
