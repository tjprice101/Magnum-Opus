---
applyTo: "Effects/**/*.fx,ShaderSource/**/*.fx,Effects/**/*.fxh,ShaderSource/**/*.fxh"
---

# Shader Development Conventions — MagnumOpus

## File Organization

- **Compiled shaders** (.fx + .fxc): `Effects/<ThemeName>/<WeaponName>/`
- **Shader source**: `ShaderSource/` (development files)
- **Shared utilities**: `ShaderSource/HLSLLibrary.fxh`
- **Root shaders** (shared across themes): `Effects/` root

## Shader Registration

All shaders must be registered in `Common/Systems/Shaders/ShaderLoader.cs`. Add your shader's load call in the appropriate section. Use `ShaderRenderer.cs` for rendering utilities.

## HLSL Conventions

### Pixel Shader Signature
```hlsl
float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
```

### Naming
- Samplers: `s0` = base texture (implicit from SpriteBatch), `s1`-`s7` for additional textures
- Uniforms: `uTime`, `uColor`, `uIntensity`, `uScrollSpeed`, `uNoiseScale` — prefix with `u`
- Functions: PascalCase (`CalculateNoise`, `ApplyDistortion`)
- Techniques: `Technique1` with descriptive pass names

### Parameter Pattern
```hlsl
// Standard uniform block
float uTime;
float uIntensity;
float2 uResolution;
float4 uColor;

// Sampler declarations
sampler baseTexture : register(s0);
sampler noiseTex : register(s1);
sampler colorRamp : register(s2);
```

### Technique/Pass Structure
```hlsl
technique Technique1
{
    pass MainPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}

// Multi-pass example (bloom H+V)
technique BloomTechnique
{
    pass BlurHorizontal
    {
        PixelShader = compile ps_3_0 BlurH();
    }
    pass BlurVertical
    {
        PixelShader = compile ps_3_0 BlurV();
    }
}
```

## Primitive Shader UV Correction

When writing shaders for primitive-rendered trails/beams (as opposed to SpriteBatch draws), apply the WotG UV correction:
```hlsl
// Required for primitive mesh UV accuracy
coords.y = (coords.y - 0.5) / input.TextureCoordinates.z + 0.5;
```

## Common Patterns

### Edge Fading (every trail/beam needs this)
```hlsl
float edgeFade = smoothstep(0.0, 0.15, coords.y) * smoothstep(1.0, 0.85, coords.y);
float tipFade = smoothstep(1.0, 0.7, coords.x);
```

### LUT Color Sampling
```hlsl
float intensity = tex2D(baseTexture, coords).r;
float4 themed = tex2D(colorRamp, float2(intensity, 0));
```

## Screen-Effect Shader Patterns

### Chromatic Aberration
```hlsl
float2 dir = normalize(coords - 0.5);
float dist = length(coords - 0.5);
float offset = dist * uIntensity;
float r = tex2D(baseTexture, coords + dir * offset).r;
float g = tex2D(baseTexture, coords).g;
float b = tex2D(baseTexture, coords - dir * offset).b;
return float4(r, g, b, 1);
```

### Radial Distortion (Heat Haze / Boss Aura)
```hlsl
float2 center = uCenter;  // Distortion origin in UV space
float2 dir = coords - center;
float dist = length(dir);
float noise = tex2D(noiseTex, coords * uNoiseScale + uTime * 0.1).r;
float2 offset = normalize(dir) * noise * uIntensity * smoothstep(uRadius, 0.0, dist);
return tex2D(baseTexture, coords + offset);
```

### Vignette
```hlsl
float2 fromCenter = coords - 0.5;
float vignette = 1.0 - smoothstep(uInnerRadius, uOuterRadius, length(fromCenter));
float4 color = tex2D(baseTexture, coords);
return lerp(color, color * uTintColor, (1.0 - vignette) * uIntensity);
```

## SDF Shape Reference

Signed Distance Functions for procedural shapes in shaders:

| Shape | SDF |
|-------|-----|
| Circle | `length(p) - radius` |
| Ring | `abs(length(p) - radius) - thickness` |
| Box | `max(abs(p.x) - w, abs(p.y) - h)` |
| Star (5pt) | Use polar: `cos(5 * atan2(p.y, p.x)) * radius` blended with circle |
| Hexagon | `max(abs(p.x) * 0.866 + p.y * 0.5, abs(p.y)) - radius` |

Use `smoothstep` on SDF values for anti-aliased edges: `smoothstep(0.01, -0.01, sdf)`.

## Noise + Ramp + Mask Reuse Matrix

The same shader components combine for radically different effects:

| Noise | Color Ramp | Mask | Result |
|-------|-----------|------|--------|
| Perlin | Fire gradient | Circle SDF | Fireball aura |
| Perlin | Ice gradient | Circle SDF | Frost orb |
| Voronoi | Void gradient | Ring SDF | Portal/rift |
| FBM | Energy gradient | Screen UV | Energy field |
| Marble | Blood gradient | Radial fade | Pulsing blood aura |
| Simplex | Rainbow gradient | Star SDF | Prismatic sigil |

Always check `Assets/VFX Asset Library/NoiseTextures/` for existing noise textures before generating procedural noise in shader code.

## Radial Noise Mask Reuse Guide

A single `RadialScrollShader.fx` pattern + different noise textures = many unique effects:
- Cosmic nebula noise → Swirling galaxy portal
- Voronoi cells → Cracking/shattering aura
- Perlin smooth → Organic fire/energy aura
- FBM turbulent → Chaotic storm effect
- Marble veins → Flowing liquid metal

The key insight: **change the noise texture, not the shader code.** One well-written radial shader serves dozens of effects.

### Additive-Safe Output
If the shader's output will be drawn with `BlendState.Additive`, ensure the background is black (0,0,0) where no effect exists. Black = invisible in additive.

## Compilation

See `ShaderSource/README_SHADER_COMPILATION.md` for build instructions. Compiled `.fxc` files must be placed alongside the `.fx` source in the `Effects/` directory.

## Before Writing a New Shader

1. Check `Effects/` for existing shaders that might already solve the problem
2. Check `ShaderSource/HLSLLibrary.fxh` for shared utility functions
3. Check theme-specific shader directories (`Effects/<ThemeName>/`)
4. Search reference repos for implementations of the same technique
