# MagnumOpus Shader Compilation Guide

## Overview

The MagnumOpus mod uses custom HLSL shaders for visual effects. These `.fx` files must be compiled to `.xnb` format for tModLoader to load them.

## Current Status

- **9 shader files** exist in this folder (`.fx` format)
- **MagnumShaderSystem.cs** handles shader loading with graceful fallback
- Without compiled shaders, the mod uses **additive blending fallback** (still looks good!)

---

## Option 1: Using MonoGame Content Builder (MGCB) - RECOMMENDED

### Prerequisites
1. Install [MonoGame SDK](https://monogame.net/downloads/)
2. Or install via Visual Studio: Extensions → Manage Extensions → MonoGame

### Steps

1. **Create a Content.mgcb file** in `Assets/Shaders/`:

```
#----------------------------- Global Properties ----------------------------#

/outputDir:../../bin/Debug/net6.0/Shaders
/intermediateDir:obj
/platform:Windows
/config:
/profile:HiDef
/compress:False

#-------------------------------- References ---------------------------------#

#---------------------------------- Content ----------------------------------#

# Trail Shader
/importer:EffectImporter
/processor:EffectProcessor
/build:TrailShader.fx

# Bloom Shader  
/importer:EffectImporter
/processor:EffectProcessor
/build:SimpleBloomShader.fx

# Screen Effects Shader
/importer:EffectImporter
/processor:EffectProcessor
/build:ScreenEffectsShader.fx
```

2. **Run MGCB** from command line:
```cmd
mgcb /build:Content.mgcb
```

3. **Copy the generated .xnb files** to the same folder as the .fx files

---

## Option 2: Using EasyShader Mod (Runtime Compilation)

Some mods use the **EasyShader** mod as a dependency for runtime shader compilation.

### Setup
1. Add `modReferences = EasyShader` to `build.txt`
2. Use EasyShader's API instead of raw Effect loading

### Note
This creates a dependency and may not work in all environments.

---

## Option 3: TMLShaderCompiler External Tool

Community tool specifically for tModLoader shader compilation.

1. Download TMLShaderCompiler
2. Point it to your `.fx` files
3. It generates `.xnb` files

---

## Shader Registration (Already Implemented)

The `MagnumShaderSystem.cs` properly implements:

### ✅ Barrier 1: Compilation
- Attempts to load compiled `.xnb` shaders
- Falls back gracefully if not found

### ✅ Barrier 2: Loading into Memory
- Uses `Asset<Effect>` with `ModContent.Request<Effect>` (1.4.5+ pattern)
- Registers with `GameShaders.Misc` for trail/bloom shaders
- Registers with `Filters.Scene` for screen effects

### ✅ Barrier 3: Parameter Passing
- Updates `uTime`, `uColor`, `uOpacity`, `uIntensity` per-frame
- Uses `MiscShaderData.UseColor()`, `UseOpacity()`, etc.

### ✅ Barrier 4: Common Pitfalls Handled
- Coordinate system: converts world → screen → normalized (0,0 = top-left)
- Pass names match between C# and .fx ("DefaultPass")
- Uses SamplerState.LinearClamp in BeginShaderBatch()

### ✅ Barrier 5: Draw Hooks
- `BeginShaderBatch()` / `EndShaderBatch()` helpers
- `DrawWithBloom()` for easy shader application
- Proper SpriteBatch state management

---

## Fallback Behavior

When shaders are NOT compiled, the mod uses:

1. **Multi-layer additive blending** for bloom effects
2. **Standard primitive rendering** for trails
3. **Color layering** for glow effects

The fallback looks very good and is used by the VFX system automatically.

---

## Testing Shaders

To verify shaders are loading:

1. Check the tModLoader log for messages like:
   - `MagnumShaderSystem: Loaded TrailShader`
   - `MagnumShaderSystem: Initialized with X shaders.`

2. If you see:
   - `MagnumShaderSystem: VFX will use fallback rendering (no shaders).`
   - This means shaders aren't compiled but the mod will work fine.

---

## Shader Files Reference

| File | Purpose | Registered As |
|------|---------|---------------|
| `TrailShader.fx` | Projectile/weapon trails | `MagnumOpus:Trail` |
| `SimpleBloomShader.fx` | Glow/bloom effects | `MagnumOpus:Bloom` |
| `ScreenEffectsShader.fx` | Screen distortion/aberration | `MagnumOpus:ScreenDistortion` |
| `AdvancedTrailShader.fx` | Enhanced trails (future) | - |
| `AdvancedBloomShader.fx` | Enhanced bloom (future) | - |
| `BloomShader.fx` | Full-featured bloom | - |
