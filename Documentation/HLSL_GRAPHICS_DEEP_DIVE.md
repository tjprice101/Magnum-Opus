# Complete HLSL & Graphics Programming Deep Dive

> **COMPREHENSIVE REFERENCE FOR MAGNUMOPUS SHADER DEVELOPMENT**
>
> This document consolidates all HLSL knowledge, shader techniques, and graphics programming
> patterns relevant to MagnumOpus VFX development.

---

## 📖 Related Documentation

| Document | Purpose |
|----------|----------|
| **[VFX_CORE_CONCEPTS_PART2.md](VFX_CORE_CONCEPTS_PART2.md)** | Bezier curves, particle architecture, billboarding, GC optimization |
| **[VFX_MASTERY_RESEARCH_COMPLETE.md](VFX_MASTERY_RESEARCH_COMPLETE.md)** | MonoGame API, BlendStates, primitive trails, bloom stacking |
| **[Enhanced_VFX_System.md](Guides/Enhanced_VFX_System.md)** | MagnumOpus VFX API usage guide, themed particles |

---

## Table of Contents

1. [Foundation Resources](#1-foundation-resources)
2. [HLSL Language Deep Dive](#2-hlsl-language-deep-dive)
3. [Shader Model Reference](#3-shader-model-reference)
4. [Core Syntax & Semantics](#4-core-syntax--semantics)
5. [Intrinsic Functions](#5-intrinsic-functions)
6. [Procedural Noise Functions](#6-procedural-noise-functions)
7. [Signed Distance Fields (SDFs)](#7-signed-distance-fields-sdfs)
8. [UV Manipulation Techniques](#8-uv-manipulation-techniques)
9. [Color Grading & Post-Processing](#9-color-grading--post-processing)
10. [Lighting Models](#10-lighting-models)
11. [Performance Optimization](#11-performance-optimization)
12. [Triangle Strip Topology](#12-triangle-strip-topology)
13. [Blending Modes](#13-blending-modes)
14. [Color Interpolation](#14-color-interpolation)
15. [MagnumOpus Shader Reference](#15-magnumopus-shader-reference)

---

## 1. Foundation Resources

### Official Documentation

| Resource | URL | Purpose |
|----------|-----|---------|
| Microsoft HLSL Reference | https://learn.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl | Complete language specification |
| DirectX Shader Compiler | https://github.com/microsoft/DirectXShaderCompiler | Modern compilation, debugging |
| NVIDIA GPU Gems | https://developer.nvidia.com/gpugems | Hardware-specific optimizations |

### Essential Books

1. **"Real-Time Rendering" (4th Edition)** - Akenine-Möller, Haines, Hoffman
   - Chapter 3: The Graphics Pipeline
   - Chapter 5: Shading Basics
   - Chapter 6: Texturing
   - Chapter 9: Physically Based Shading

2. **"GPU Gems" Series (Free PDFs)**
   - GPU Gems 1: Water simulation, subsurface scattering
   - GPU Gems 2: Refraction, toon shaders
   - GPU Gems 3: Procedural terrains, vegetation

3. **"The Cg Tutorial"** - Fernando & Kilgard
   - Free: https://developer.nvidia.com/cg-toolkit

### Interactive Learning

| Platform | URL | Focus |
|----------|-----|-------|
| ShaderToy | https://www.shadertoy.com/ | Real-time shader examples |
| Book of Shaders | https://thebookofshaders.com/ | Visual, beginner-friendly |
| Shader Playground | http://shader-playground.timjones.io/ | HLSL compilation inspection |

---

## 2. HLSL Language Deep Dive

### Data Types

```hlsl
// Scalar types
float   // 32-bit floating point (most common)
half    // 16-bit floating point (mobile optimization)
int     // 32-bit signed integer
uint    // 32-bit unsigned integer
bool    // Boolean

// Vector types (HLSL swizzling power)
float2  // 2D vector (x, y)
float3  // 3D vector (x, y, z) or (r, g, b)
float4  // 4D vector (x, y, z, w) or (r, g, b, a)

// Matrix types
float4x4  // 4x4 matrix (transformations)
float3x3  // 3x3 matrix (rotations, normals)
float2x2  // 2x2 matrix (2D transformations)

// Swizzling examples
float4 color = float4(1, 0.5, 0.2, 1);
float3 rgb = color.rgb;       // Extract RGB
float2 rg = color.rg;         // Extract RG
float4 swizzled = color.bgra; // Reorder channels
float gray = color.r;         // Single component

// Advanced swizzling
float2 uv = float2(0.5, 0.8);
float4 extended = uv.xyxy;    // (0.5, 0.8, 0.5, 0.8)
```

---

## 3. Shader Model Reference

| Shader Model | DirectX | Key Features | tModLoader Use |
|--------------|---------|--------------|----------------|
| SM 2.0 | DX9 | 64 instructions, basic loops | Legacy support |
| SM 3.0 | DX9c | Dynamic branching, 512 instructions | Common baseline |
| **SM 4.0** | DX10 | Geometry shaders, 65,536 instructions | **MonoGame target** |
| SM 5.0 | DX11 | Compute shaders, tessellation | Advanced features |
| SM 6.0+ | DX12 | Raytracing, mesh shaders | Future/cutting-edge |

**For tModLoader:** Target **Shader Model 4.0 Level 9.1** for maximum compatibility.

```hlsl
// Compile command for MagnumOpus shaders:
// mgcb /platform:Windows /profile:HiDef /importer:EffectImporter /processor:EffectProcessor

// Technique declaration for SM 4.0 Level 9.1
technique DefaultTechnique
{
    pass MainPass
    {
        VertexShader = compile vs_4_0_level_9_1 MainVS();
        PixelShader = compile ps_4_0_level_9_1 MainPS();
    }
}
```

---

## 4. Core Syntax & Semantics

### Vertex Shader Input Semantics

```hlsl
struct VertexInput
{
    float4 Position : POSITION0;    // Vertex position
    float4 Color    : COLOR0;       // Vertex color
    float2 TexCoord : TEXCOORD0;    // UV coordinates
    float3 Normal   : NORMAL0;      // Surface normal
    float3 Tangent  : TANGENT0;     // Tangent for normal mapping
};
```

### Vertex Shader Output / Pixel Shader Input

```hlsl
struct VertexOutput
{
    float4 Position : SV_POSITION;  // System Value: Screen position
    float4 Color    : COLOR0;       // Interpolated color
    float2 TexCoord : TEXCOORD0;    // Interpolated UVs
    float3 Normal   : TEXCOORD1;    // World-space normal
    float3 WorldPos : TEXCOORD2;    // World-space position
};
```

### Pixel Shader Output

```hlsl
struct PixelOutput
{
    float4 Color : SV_TARGET0;      // System Value: Render target 0
    // float4 Color1 : SV_TARGET1;  // Multiple render targets (MRT)
};
```

### Critical Semantics

| Semantic | Type | Purpose |
|----------|------|---------|
| `SV_POSITION` | System Value | Required VS output (clip space) |
| `SV_TARGET` | System Value | Render target output |
| `POSITION0`, `COLOR0`, `TEXCOORD0` | User | Custom interpolated data |

---

## 5. Intrinsic Functions

### Mathematical Operations

```hlsl
// Basic math
abs(x)          // Absolute value
sqrt(x)         // Square root
pow(x, y)       // x raised to power y
exp(x)          // e^x
log(x)          // Natural logarithm
log2(x)         // Base-2 logarithm

// Trigonometry
sin(x), cos(x), tan(x)      // Trig functions (radians)
asin(x), acos(x), atan(x)   // Inverse trig
atan2(y, x)                 // Two-argument arctangent

// Vector operations
dot(a, b)       // Dot product (returns scalar)
cross(a, b)     // Cross product (3D vectors only)
length(v)       // Vector magnitude
normalize(v)    // Unit vector
distance(a, b)  // Distance between points
reflect(i, n)   // Reflection vector
refract(i, n, eta) // Refraction vector (Snell's law)

// Interpolation
lerp(a, b, t)           // Linear interpolation: a + (b - a) * t
smoothstep(min, max, x) // Smooth Hermite interpolation
step(edge, x)           // Returns 0 if x < edge, 1 otherwise

// Clamping
clamp(x, min, max)    // Constrain x to [min, max]
saturate(x)           // Clamp to [0, 1] (very common)
min(a, b), max(a, b)  // Minimum/maximum

// Utility
frac(x)         // Fractional part (x - floor(x))
floor(x)        // Round down
ceil(x)         // Round up
round(x)        // Round to nearest
sign(x)         // Returns -1, 0, or 1
ddx(x), ddy(x)  // Screen-space derivatives (for mipmaps)
fwidth(x)       // abs(ddx(x)) + abs(ddy(x)) - for anti-aliasing
```

### Texture Sampling

```hlsl
texture2D BaseTexture;
sampler2D BaseSampler = sampler_state
{
    Texture = <BaseTexture>;
    MinFilter = Linear;  // Linear, Point, Anisotropic
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Wrap;     // Wrap, Clamp, Mirror, Border
    AddressV = Wrap;
};

// In pixel shader:
float4 color = tex2D(BaseSampler, uv);               // Basic sampling
float4 color = tex2Dlod(BaseSampler, float4(uv, 0, mipLevel)); // Specific mip
float4 color = tex2Dgrad(BaseSampler, uv, ddx, ddy); // Manual derivatives
float4 color = tex2Dbias(BaseSampler, float4(uv, 0, bias)); // Mip bias
```

### Control Flow

```hlsl
// CRITICAL: GPUs execute in "warps" (32-64 threads)
// All threads in warp execute BOTH branches if any diverge!

// Static branching (compile-time) - PREFERRED
#if ENABLE_FEATURE
    // Code only compiled if ENABLE_FEATURE is defined
#endif

// Dynamic branching (runtime - performance cost!)
// Use ONLY for large blocks of work
if (condition)
{
    // Expensive path A
}

// Better: Conditional assignment
float result = condition ? valueA : valueB;

// Best: Mathematical selection (no branching)
float result = lerp(valueA, valueB, step(0.5, condition));
```

### Loop Hints

```hlsl
// Force unroll (good for small fixed loops < 8 iterations)
[unroll]
for (int i = 0; i < 4; i++)
{
    color += tex2D(Sampler, uv + offsets[i]);
}

// Prevent unrolling (good for large loops > 16 iterations)
[loop]
for (int i = 0; i < iterations; i++)
{
    // Complex work
}
```

---

## 6. Procedural Noise Functions

### Value Noise (Simplest)

```hlsl
// Pseudo-random hash function
float hash(float2 p)
{
    p = frac(p * float2(443.897, 441.423));
    p += dot(p, p.yx + 19.19);
    return frac(p.x * p.y);
}

// 2D Value Noise
float valueNoise(float2 uv)
{
    float2 i = floor(uv);  // Integer part
    float2 f = frac(uv);   // Fractional part
    
    // Smooth interpolation (Hermite curve)
    f = f * f * (3.0 - 2.0 * f);
    
    // Four corners of cell
    float a = hash(i);
    float b = hash(i + float2(1, 0));
    float c = hash(i + float2(0, 1));
    float d = hash(i + float2(1, 1));
    
    // Bilinear interpolation
    return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
}
```

### Perlin Noise (Better Quality)

```hlsl
// Perlin gradient noise (smooth, organic)
float2 randomGradient(float2 p)
{
    p = p % 289;
    float x = (34 * p.x + 1) * p.x % 289 + p.y;
    x = (34 * x + 1) * x % 289;
    x = frac(x / 41) * 2 - 1;
    return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
}

float perlinNoise(float2 uv)
{
    float2 i = floor(uv);
    float2 f = frac(uv);
    
    // Quintic interpolation (smoother than cubic)
    float2 u = f * f * f * (f * (f * 6 - 15) + 10);
    
    // Gradients at corners
    float2 ga = randomGradient(i + float2(0, 0));
    float2 gb = randomGradient(i + float2(1, 0));
    float2 gc = randomGradient(i + float2(0, 1));
    float2 gd = randomGradient(i + float2(1, 1));
    
    // Dot products with distance vectors
    float va = dot(ga, f - float2(0, 0));
    float vb = dot(gb, f - float2(1, 0));
    float vc = dot(gc, f - float2(0, 1));
    float vd = dot(gd, f - float2(1, 1));
    
    // Interpolate
    return lerp(lerp(va, vb, u.x), lerp(vc, vd, u.x), u.y);
}
```

### Simplex Noise (Best Performance/Quality)

```hlsl
// 2D Simplex Noise - Implementation by Ian McEwan, Ashima Arts
float3 mod289(float3 x) { return x - floor(x * (1.0 / 289.0)) * 289.0; }
float2 mod289(float2 x) { return x - floor(x * (1.0 / 289.0)) * 289.0; }
float3 permute(float3 x) { return mod289(((x*34.0)+1.0)*x); }

float simplexNoise(float2 v)
{
    const float4 C = float4(0.211324865405187,  // (3.0-sqrt(3.0))/6.0
                            0.366025403784439,  // 0.5*(sqrt(3.0)-1.0)
                            -0.577350269189626, // -1.0 + 2.0 * C.x
                            0.024390243902439); // 1.0 / 41.0
    
    float2 i  = floor(v + dot(v, C.yy));
    float2 x0 = v -   i + dot(i, C.xx);
    
    float2 i1 = (x0.x > x0.y) ? float2(1.0, 0.0) : float2(0.0, 1.0);
    float4 x12 = x0.xyxy + C.xxzz;
    x12.xy -= i1;
    
    i = mod289(i);
    float3 p = permute(permute(i.y + float3(0.0, i1.y, 1.0))
                     + i.x + float3(0.0, i1.x, 1.0));
    
    float3 m = max(0.5 - float3(dot(x0,x0), dot(x12.xy,x12.xy), dot(x12.zw,x12.zw)), 0.0);
    m = m*m;
    m = m*m;
    
    float3 x = 2.0 * frac(p * C.www) - 1.0;
    float3 h = abs(x) - 0.5;
    float3 ox = floor(x + 0.5);
    float3 a0 = x - ox;
    
    m *= 1.79284291400159 - 0.85373472095314 * (a0*a0 + h*h);
    
    float3 g;
    g.x  = a0.x  * x0.x  + h.x  * x0.y;
    g.yz = a0.yz * x12.xz + h.yz * x12.yw;
    return 130.0 * dot(m, g);
}
```

### Fractional Brownian Motion (fBm)

```hlsl
// Combines multiple octaves of noise for natural patterns
float fbm(float2 uv, int octaves)
{
    float value = 0.0;
    float amplitude = 0.5;
    float frequency = 1.0;
    
    for (int i = 0; i < octaves; i++)
    {
        value += amplitude * simplexNoise(uv * frequency);
        frequency *= 2.0;  // Each octave doubles frequency
        amplitude *= 0.5;  // Each octave halves amplitude
    }
    
    return value;
}

// Usage: Organic clouds, fire, turbulence
float cloudPattern = fbm(uv * 3.0 + time * 0.1, 5);
```

### Noise Comparison

| Type | Quality | Performance | Use Case |
|------|---------|-------------|----------|
| Value Noise | ⭐⭐ | ⭐⭐⭐⭐⭐ | Simple patterns, very fast |
| Perlin Noise | ⭐⭐⭐⭐ | ⭐⭐⭐ | Organic textures, classic |
| Simplex Noise | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ | Best overall quality/speed |
| fBm | ⭐⭐⭐⭐⭐ | ⭐⭐ | Natural phenomena (expensive) |

---

## 7. Signed Distance Fields (SDFs)

**Concept:** Functions that return distance to nearest surface. Negative = inside, positive = outside, zero = on surface.

### 2D Primitive SDFs

```hlsl
// Circle
float sdCircle(float2 p, float radius)
{
    return length(p) - radius;
}

// Box (rectangle)
float sdBox(float2 p, float2 size)
{
    float2 d = abs(p) - size;
    return length(max(d, 0.0)) + min(max(d.x, d.y), 0.0);
}

// Line segment
float sdSegment(float2 p, float2 a, float2 b)
{
    float2 pa = p - a;
    float2 ba = b - a;
    float h = clamp(dot(pa, ba) / dot(ba, ba), 0.0, 1.0);
    return length(pa - ba * h);
}

// Hexagon
float sdHexagon(float2 p, float radius)
{
    const float3 k = float3(-0.866025404, 0.5, 0.577350269);
    p = abs(p);
    p -= 2.0 * min(dot(k.xy, p), 0.0) * k.xy;
    p -= float2(clamp(p.x, -k.z * radius, k.z * radius), radius);
    return length(p) * sign(p.y);
}

// Rounded box
float sdRoundedBox(float2 p, float2 b, float r)
{
    float2 d = abs(p) - b + r;
    return length(max(d, 0.0)) - r;
}

// Star (5-pointed)
float sdStar5(float2 p, float r, float rf)
{
    const float2 k1 = float2(0.809016994, -0.587785252);
    const float2 k2 = float2(-k1.x, k1.y);
    p.x = abs(p.x);
    p -= 2.0 * max(dot(k1, p), 0.0) * k1;
    p -= 2.0 * max(dot(k2, p), 0.0) * k2;
    p.x = abs(p.x);
    p.y -= r;
    float2 ba = rf * float2(-k1.y, k1.x) - float2(0, 1);
    float h = clamp(dot(p, ba) / dot(ba, ba), 0.0, r);
    return length(p - ba * h) * sign(p.y * ba.x - p.x * ba.y);
}
```

### SDF Operations

```hlsl
// Union (combine shapes)
float opUnion(float d1, float d2)
{
    return min(d1, d2);
}

// Subtraction (cut out)
float opSubtraction(float d1, float d2)
{
    return max(-d1, d2);
}

// Intersection
float opIntersection(float d1, float d2)
{
    return max(d1, d2);
}

// Smooth union (blob merge) - CRITICAL FOR METABALLS
float opSmoothUnion(float d1, float d2, float k)
{
    float h = clamp(0.5 + 0.5 * (d2 - d1) / k, 0.0, 1.0);
    return lerp(d2, d1, h) - k * h * (1.0 - h);
}

// Smooth subtraction
float opSmoothSubtraction(float d1, float d2, float k)
{
    float h = clamp(0.5 - 0.5 * (d2 + d1) / k, 0.0, 1.0);
    return lerp(d2, -d1, h) + k * h * (1.0 - h);
}

// Smooth intersection
float opSmoothIntersection(float d1, float d2, float k)
{
    float h = clamp(0.5 - 0.5 * (d2 - d1) / k, 0.0, 1.0);
    return lerp(d2, d1, h) + k * h * (1.0 - h);
}
```

### Rendering SDFs

```hlsl
float4 RenderSDF(float2 uv)
{
    float d = sdCircle(uv - 0.5, 0.3);
    
    // Sharp edge
    float sharpMask = step(0.0, d);
    
    // Soft edge (glow)
    float softMask = 1.0 - saturate(d / 0.1);
    
    // Anti-aliased edge (BEST)
    float aaEdge = smoothstep(fwidth(d), 0.0, d);
    
    // Outline only
    float outline = 1.0 - smoothstep(0.0, 0.02, abs(d));
    
    // With glow (exponential falloff)
    float glow = exp(-d * 5.0);
    
    return float4(glow.xxx, 1);
}
```

**SDF Resources:**
- Inigo Quilez: https://iquilezles.org/articles/distfunctions2d/
- Mercury Library: http://mercury.sexy/hg_sdf/

---

## 8. UV Manipulation Techniques

### Basic Transformations

```hlsl
// Tiling
float2 uvTiled = frac(uv * tileCount);

// Scrolling
float2 uvScrolled = uv + float2(time * scrollSpeedX, time * scrollSpeedY);

// Rotation
float2 RotateUV(float2 uv, float angle)
{
    float2 center = float2(0.5, 0.5);
    float cosA = cos(angle);
    float sinA = sin(angle);
    float2x2 rot = float2x2(cosA, -sinA, sinA, cosA);
    return mul(rot, uv - center) + center;
}

// Scale from center
float2 uvScaled = (uv - 0.5) * scale + 0.5;
```

### Distortion Effects

```hlsl
// Wave distortion
float2 WaveDistortion(float2 uv, float time)
{
    float2 offset;
    offset.x = sin(uv.y * 10.0 + time * 2.0) * 0.02;
    offset.y = cos(uv.x * 10.0 + time * 2.0) * 0.02;
    return uv + offset;
}

// Noise-based distortion
float2 NoiseDistortion(float2 uv, float strength)
{
    float noiseX = simplexNoise(uv * 5.0) * strength;
    float noiseY = simplexNoise(uv * 5.0 + float2(100, 100)) * strength;
    return uv + float2(noiseX, noiseY);
}

// Radial distortion (fish-eye/barrel)
float2 RadialDistortion(float2 uv, float strength)
{
    float2 center = float2(0.5, 0.5);
    float2 offset = uv - center;
    float dist = length(offset);
    float factor = 1.0 + strength * dist * dist;
    return center + offset * factor;
}

// Polar coordinates
float2 CartesianToPolar(float2 uv)
{
    float2 centered = uv - 0.5;
    float radius = length(centered);
    float angle = atan2(centered.y, centered.x);
    return float2(radius, angle);
}

float2 PolarToCartesian(float2 polar)
{
    float2 cart;
    cart.x = polar.x * cos(polar.y);
    cart.y = polar.x * sin(polar.y);
    return cart + 0.5;
}
```

---

## 9. Color Grading & Post-Processing

### Color Space Conversions

```hlsl
// RGB to HSV
float3 RGBtoHSV(float3 rgb)
{
    float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    float4 p = lerp(float4(rgb.bg, K.wz), float4(rgb.gb, K.xy), step(rgb.b, rgb.g));
    float4 q = lerp(float4(p.xyw, rgb.r), float4(rgb.r, p.yzx), step(p.x, rgb.r));
    
    float d = q.x - min(q.w, q.y);
    float e = 1.0e-10;
    return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

// HSV to RGB
float3 HSVtoRGB(float3 hsv)
{
    float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    float3 p = abs(frac(hsv.xxx + K.xyz) * 6.0 - K.www);
    return hsv.z * lerp(K.xxx, saturate(p - K.xxx), hsv.y);
}

// Hue shift
float3 HueShift(float3 color, float shift)
{
    float3 hsv = RGBtoHSV(color);
    hsv.x = frac(hsv.x + shift);
    return HSVtoRGB(hsv);
}
```

### Tone Mapping

```hlsl
// Reinhard (simple)
float3 Reinhard(float3 color)
{
    return color / (1.0 + color);
}

// Uncharted 2 (filmic)
float3 Uncharted2Tonemap(float3 x)
{
    float A = 0.15, B = 0.50, C = 0.10;
    float D = 0.20, E = 0.02, F = 0.30;
    return ((x * (A * x + C * B) + D * E) / (x * (A * x + B) + D * F)) - E / F;
}

// ACES filmic (industry standard)
float3 ACESFilm(float3 x)
{
    float a = 2.51, b = 0.03;
    float c = 2.43, d = 0.59, e = 0.14;
    return saturate((x * (a * x + b)) / (x * (c * x + d) + e));
}
```

### Bloom/Glow

```hlsl
// Bright pass filter (extract bright areas)
float4 BrightPass(float4 color, float threshold)
{
    float brightness = dot(color.rgb, float3(0.299, 0.587, 0.114));
    float contribution = max(0, brightness - threshold);
    contribution /= max(brightness, 0.00001);
    return color * contribution;
}

// Gaussian blur (separable - do horizontal and vertical passes)
float4 GaussianBlur(sampler2D tex, float2 uv, float2 direction)
{
    float weights[5] = { 0.227027, 0.1945946, 0.1216216, 0.054054, 0.016216 };
    
    float4 result = tex2D(tex, uv) * weights[0];
    
    [unroll]
    for (int i = 1; i < 5; i++)
    {
        float2 offset = direction * i;
        result += tex2D(tex, uv + offset) * weights[i];
        result += tex2D(tex, uv - offset) * weights[i];
    }
    
    return result;
}
```

### Chromatic Aberration

```hlsl
float4 ChromaticAberration(sampler2D tex, float2 uv, float strength)
{
    float2 center = float2(0.5, 0.5);
    float2 offset = (uv - center) * strength;
    
    float r = tex2D(tex, uv - offset).r;
    float g = tex2D(tex, uv).g;
    float b = tex2D(tex, uv + offset).b;
    
    return float4(r, g, b, 1.0);
}
```

### Vignette

```hlsl
float Vignette(float2 uv, float intensity, float smoothness)
{
    float2 centered = uv - 0.5;
    float dist = length(centered);
    return 1.0 - smoothstep(0.5 - smoothness, 0.5, dist * intensity);
}
```

---

## 10. Lighting Models

### Lambert (Diffuse)

```hlsl
float LambertDiffuse(float3 normal, float3 lightDir)
{
    return saturate(dot(normal, lightDir));
}
```

### Blinn-Phong (Specular)

```hlsl
float BlinnPhongSpecular(float3 normal, float3 lightDir, float3 viewDir, float shininess)
{
    float3 halfDir = normalize(lightDir + viewDir);
    return pow(saturate(dot(normal, halfDir)), shininess);
}
```

### Fresnel Effect

```hlsl
float FresnelSchlick(float3 normal, float3 viewDir, float F0)
{
    float cosTheta = saturate(dot(normal, viewDir));
    return F0 + (1.0 - F0) * pow(1.0 - cosTheta, 5.0);
}

// With roughness
float3 FresnelSchlickRoughness(float cosTheta, float3 F0, float roughness)
{
    return F0 + (max(1.0 - roughness, F0) - F0) * pow(1.0 - cosTheta, 5.0);
}
```

### Rim Lighting

```hlsl
float RimLight(float3 normal, float3 viewDir, float power)
{
    float rim = 1.0 - saturate(dot(viewDir, normal));
    return pow(rim, power);
}
```

---

## 11. Performance Optimization

### GPU Architecture Fundamentals

**Execution Model:**
- GPUs execute shaders in warps/waves (32-64 threads simultaneously)
- All threads in a warp execute the same instruction
- Divergence (different branches) = both paths executed, results masked

**Memory Hierarchy (fastest → slowest):**
1. Registers - Fastest, limited per thread
2. Shared Memory - Fast, shared within thread group
3. L1 Cache - Automatic, per SM
4. L2 Cache - Shared across GPU
5. VRAM - Main memory, slowest

### Optimization Techniques

#### 1. Minimize Texture Samples

```hlsl
// BAD: Multiple samples with same filter
float4 c1 = tex2D(Sampler, uv);
float4 c2 = tex2D(Sampler, uv + offset1);
float4 c3 = tex2D(Sampler, uv + offset2);

// GOOD: Bilinear filtering = 4 samples for free
float4 c = tex2D(Sampler, uv);  // Already interpolated
```

#### 2. Reduce Register Pressure

```hlsl
// BAD: Too many temporary variables
float temp1 = someCalculation();
float temp2 = otherCalculation();
float temp3 = temp1 * temp2;
float result = temp3 + anotherValue;

// GOOD: Inline calculations
float result = (someCalculation() * otherCalculation()) + anotherValue;
```

#### 3. Use MAD (Multiply-Add) Instructions

```hlsl
// Compiler optimizes to single MAD instruction
float result = b * c + d;  // NOT: float a = b * c; a + d;
```

#### 4. Avoid Dependent Texture Reads

```hlsl
// BAD: UV depends on previous texture read (stalls pipeline)
float4 base = tex2D(Sampler, uv);
float2 newUV = uv + base.rg * 0.1;
float4 final = tex2D(Sampler, newUV);

// BETTER: Precalculate offsets if possible
```

#### 5. Branch Prediction

```hlsl
// BAD: Complex dynamic branching
if (complexCondition) { /* Path A */ } else { /* Path B */ }

// GOOD: Predication (no branch)
float mask = complexCondition ? 1.0 : 0.0;
result = lerp(pathB_result, pathA_result, mask);
```

### Profiling Tools

| Tool | Platform | Purpose |
|------|----------|---------|
| PIX | Windows | Frame capture, shader debugging |
| RenderDoc | Cross-platform | Open-source graphics debugger |
| NVIDIA Nsight | NVIDIA | GPU profiling |
| AMD Radeon GPU Profiler | AMD | AMD-specific profiling |

### Metrics to Watch

- **Draw calls:** Minimize state changes
- **Texture binds:** Batch similar materials
- **Shader complexity:** Keep pixel shaders < 100 instructions
- **Memory bandwidth:** Reduce texture sizes, use compression

---

## 12. Triangle Strip Topology

### Theory

**Primitive Topologies:**
- **Triangle List:** Every 3 vertices = 1 triangle (inefficient for strips)
- **Triangle Strip:** Each new vertex adds a triangle using previous 2 vertices
- **Triangle Fan:** All triangles share first vertex (good for circles)

**Efficiency:**
```
Triangle List:    6 vertices for 2 triangles (v0,v1,v2, v0,v2,v3)
Triangle Strip:   4 vertices for 2 triangles (v0,v1,v2,v3)
Savings:          33% fewer vertices!
```

**Winding Order:**
- Triangle strips alternate winding automatically
- Counter-Clockwise (CCW) = Front face (visible)
- Clockwise (CW) = Back face (usually culled)

### Implementation

```csharp
// Vertex structure
public struct VertexPositionColorTexture : IVertexType
{
    public Vector3 Position;
    public Color Color;
    public Vector2 TextureCoordinate;
    
    public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(
        new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
        new VertexElement(12, VertexElementFormat.Color, VertexElementUsage.Color, 0),
        new VertexElement(16, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
    );
}

// Create segmented trail as triangle strip
public void DrawCurvedBeam(List<Vector2> points, float width, Color color)
{
    int segments = points.Count - 1;
    var vertices = new VertexPositionColorTexture[segments * 2 + 2];
    
    for (int i = 0; i <= segments; i++)
    {
        Vector2 position = points[i];
        Vector2 direction;
        
        // Calculate tangent direction
        if (i == 0)
            direction = points[1] - points[0];
        else if (i == segments)
            direction = points[i] - points[i - 1];
        else
            direction = (points[i + 1] - points[i - 1]) * 0.5f;
        
        direction.Normalize();
        Vector2 perpendicular = new Vector2(-direction.Y, direction.X);
        
        float progress = i / (float)segments;
        float currentWidth = MathHelper.Lerp(width, width * 0.5f, progress); // Taper
        
        vertices[i * 2] = new VertexPositionColorTexture
        {
            Position = new Vector3(position + perpendicular * currentWidth * 0.5f, 0),
            Color = color,
            TextureCoordinate = new Vector2(progress, 0)
        };
        
        vertices[i * 2 + 1] = new VertexPositionColorTexture
        {
            Position = new Vector3(position - perpendicular * currentWidth * 0.5f, 0),
            Color = color,
            TextureCoordinate = new Vector2(progress, 1)
        };
    }
    
    // Draw
    GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices, 0, segments * 2);
}
```

---

## 13. Blending Modes

### Blend Equation

```
FinalColor = SourceColor * SourceBlend + DestColor * DestBlend
```

### Common Blend Modes

| Mode | Source | Dest | Formula | Use |
|------|--------|------|---------|-----|
| **Alpha** | SourceAlpha | InverseSourceAlpha | `Src * α + Dst * (1-α)` | Standard transparency |
| **Additive** | One (or SourceAlpha) | One | `Src + Dst` | Light effects, glow |
| **Multiplicative** | DestColor | Zero | `Src * Dst` | Shadows, darkening |
| **Screen** | One | InverseSourceColor | `Src + Dst - Src*Dst` | Lighting effects |

### MonoGame Implementation

```csharp
// Create custom blend state
public static BlendState AdditiveBlending = new BlendState
{
    ColorSourceBlend = Blend.SourceAlpha,
    ColorDestinationBlend = Blend.One,
    ColorBlendFunction = BlendFunction.Add,
    AlphaSourceBlend = Blend.One,
    AlphaDestinationBlend = Blend.One,
    AlphaBlendFunction = BlendFunction.Add
};

// Usage
spriteBatch.Begin(
    SpriteSortMode.Deferred,
    BlendState.Additive,
    SamplerState.LinearClamp,
    DepthStencilState.None,
    RasterizerState.CullNone
);
spriteBatch.Draw(glowTexture, position, Color.White * 0.5f);
spriteBatch.End();
```

### Additive Blending Tips

1. **Use alpha for intensity control** - not transparency
2. **Black = invisible** (0 + background = background)
3. **White = maximum brightness** (1 + background = very bright)
4. **Can exceed 1.0** - causes oversaturation (blooming effect)
5. **Draw order matters less** than with alpha blending

### CRITICAL: The { A = 0 } Pattern

```csharp
// For proper additive blending, remove alpha channel
Color glowColor = baseColor with { A = 0 };
// Or: new Color(color.R, color.G, color.B, 0);

// Then use color multiplier for intensity
spriteBatch.Draw(texture, pos, null, glowColor * opacity, ...);
```

---

## 14. Color Interpolation

### Linear Interpolation (Lerp)

```hlsl
// lerp(a, b, t) = a + (b - a) * t = a * (1 - t) + b * t
// t = 0 → returns a
// t = 0.5 → returns midpoint
// t = 1 → returns b
```

### Multi-Stop Gradient

```hlsl
float4 MultiStopGradient(float t)
{
    const int numStops = 4;
    float4 colors[numStops] = {
        float4(1, 0, 0, 1),    // Red
        float4(1, 1, 0, 1),    // Yellow
        float4(0, 1, 0, 1),    // Green
        float4(0, 0, 1, 1)     // Blue
    };
    
    float scaledT = t * (numStops - 1);
    int index = floor(scaledT);
    float localT = frac(scaledT);
    
    if (index >= numStops - 1)
        return colors[numStops - 1];
    
    return lerp(colors[index], colors[index + 1], localT);
}
```

### Easing Functions

```hlsl
// Smooth step (ease in/out)
float smoothstep(float edge0, float edge1, float x)
{
    float t = saturate((x - edge0) / (edge1 - edge0));
    return t * t * (3.0 - 2.0 * t);
}

// Ease in (slow start)
float easeIn(float t) { return t * t; }

// Ease out (slow end)
float easeOut(float t) { return 1.0 - (1.0 - t) * (1.0 - t); }

// Ease in-out (slow start and end)
float easeInOut(float t)
{
    return t < 0.5
        ? 2.0 * t * t
        : 1.0 - 2.0 * (1.0 - t) * (1.0 - t);
}

// Elastic
float easeOutElastic(float t)
{
    float c4 = (2.0 * 3.14159) / 3.0;
    return t == 0.0 ? 0.0 : t == 1.0 ? 1.0 :
        pow(2.0, -10.0 * t) * sin((t * 10.0 - 0.75) * c4) + 1.0;
}

// Bounce
float easeOutBounce(float t)
{
    float n1 = 7.5625;
    float d1 = 2.75;
    
    if (t < 1.0 / d1)
        return n1 * t * t;
    else if (t < 2.0 / d1)
        return n1 * (t -= 1.5 / d1) * t + 0.75;
    else if (t < 2.5 / d1)
        return n1 * (t -= 2.25 / d1) * t + 0.9375;
    else
        return n1 * (t -= 2.625 / d1) * t + 0.984375;
}
```

---

## 15. MagnumOpus Shader Reference

### Available Shaders

| Shader | Purpose | Key Features |
|--------|---------|--------------|
| **MetaballEdgeShader.fx** | Metaball edge detection | Sobel edge, glow, pulse |
| **AdditiveMetaballEdgeShader.fx** | Additive metaball merge | Smooth blend, white push |
| **AdvancedTrailShader.fx** | 5 trail styles | Flame, Ice, Lightning, Nature, Cosmic |
| **AdvancedBloomShader.fx** | 5 bloom styles | Ethereal, Infernal, Celestial, Chromatic, Void |
| **AdvancedDistortionShader.fx** | Screen distortion | Ripple, Heat, Chromatic, Eclipse, Reality Tear |

### Common Utility Functions (Used Across Shaders)

```hlsl
// QuadraticBump - Universal edge fade
// Input 0.0 → 0.0, Input 0.5 → 1.0 (peak), Input 1.0 → 0.0
float QuadraticBump(float x)
{
    return x * (4.0 - x * 4.0);
}

// InverseLerp - Get progress between two values
float InverseLerp(float a, float b, float t)
{
    return saturate((t - a) / (b - a));
}

// Convert01To010 - 0→1→0 over 0→1 input
float Convert01To010(float x)
{
    return x < 0.5 ? x * 2.0 : (1.0 - x) * 2.0;
}

// Hash - GPU-style random
float Hash(float2 p)
{
    return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
}
```

### Compile Command

```bash
# For all MagnumOpus shaders:
mgcb /platform:Windows /profile:HiDef /importer:EffectImporter /processor:EffectProcessor ShaderName.fx
```

### Integration Pattern

```csharp
// Load shader
Effect shader = ModContent.Request<Effect>("MagnumOpus/Assets/Shaders/MyShader").Value;

// Set parameters
shader.Parameters["uTime"]?.SetValue((float)Main.GameUpdateCount / 60f);
shader.Parameters["uColor"]?.SetValue(color.ToVector3());
shader.Parameters["uIntensity"]?.SetValue(intensity);

// Apply to SpriteBatch
Main.spriteBatch.End();
Main.spriteBatch.Begin(
    SpriteSortMode.Immediate,
    BlendState.Additive,
    SamplerState.LinearClamp,
    DepthStencilState.None,
    RasterizerState.CullNone,
    shader,
    Main.GameViewMatrix.TransformationMatrix
);
// Draw...
Main.spriteBatch.End();
Main.spriteBatch.Begin(/* restore previous state */);
```

---

## Learning Path

### Week 1-2: Foundations
- Write simple vertex/pixel shader (single color output)
- Implement texture sampling
- Learn swizzling and vector math
- Practice UV transformations

### Week 3-4: Intermediate
- Implement value noise function
- Create animated effects (waves, pulses)
- Learn additive blending
- Build simple particle shader

### Week 5-6: Advanced
- Implement Perlin/Simplex noise
- Create SDF-based shapes
- Implement bloom/glow effect
- Learn color grading techniques

### Week 7-8: Mastery
- Combine multiple techniques
- Optimize shaders (profile, reduce instructions)
- Create complete VFX system
- Study professional shader code

---

*Last Updated: Consolidated from VFX research and HLSL deep dive resources*
