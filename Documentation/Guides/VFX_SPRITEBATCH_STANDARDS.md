# VFX SpriteBatch Standards - MagnumOpus

> **CRITICAL: Follow these standards for all VFX rendering to prevent visual artifacts and crashes.**

---

## ⚠️ SHADER STATUS: DISABLED

> **Custom HLSL shaders are currently DISABLED in MagnumOpus.**

### Root Cause
- **FNA/MojoShader Incompatibility**: tModLoader uses FNA which requires MojoShader-compatible `.fxb` format effect files
- Our shaders were compiled with DirectX HLSL to `.xnb` format, causing: `"MOJOSHADER_compileEffect Error: Not an Effects Framework binary"`
- See `Common/Systems/Shaders/ShaderLoader.cs` for implementation details

### Current VFX Architecture
MagnumOpus uses **particle-based rendering** as the primary VFX approach:
- ✅ `BloomRenderer.cs` - Multi-layer bloom stacking without shaders
- ✅ `EnhancedTrailRenderer.cs` - Primitive trail rendering with SpriteBatch
- ✅ `MagnumParticleHandler.cs` - Object-pooled particle system
- ✅ All VFX utilities work without shaders

### To Enable Shaders (Future Work)
1. Compile HLSL shaders using `fxc.exe` with `/T fx_2_0` for MojoShader compatibility
2. Or use tModLoader's built-in shader compilation with proper Effect Framework targets
3. Update `ShaderLoader.cs` to load the compiled `.fxb` files

### Impact on Code
- `SpriteSortMode.Immediate` is **still valid** for future shader support
- All shader-dependent code paths have particle-based fallbacks
- `ShaderLoader.GetShader()` returns `null` when shaders disabled

---

## 📚 Table of Contents

1. [SpriteSortMode Rules](#1-spritesortmode-rules)
2. [SpriteBatch State Management Pattern](#2-spritebatch-state-management-pattern)
3. [Calling VFX Systems That Manage Their Own State](#3-calling-vfx-systems-that-manage-their-own-state)
4. [Matrix Selection Guidelines](#4-matrix-selection-guidelines)
5. [Texture Caching Best Practices](#5-texture-caching-best-practices)
6. [Common Mistakes and Fixes](#6-common-mistakes-and-fixes)

---

## 1. SpriteSortMode Rules

### When to Use Each Mode

| Mode | Shaders Work? | Batching | Use Case |
|------|---------------|----------|----------|
| `SpriteSortMode.Deferred` | ❌ NO | ✅ Yes | Standard drawing, bloom layers, particles |
| `SpriteSortMode.Immediate` | ✅ YES | ❌ No | HLSL shaders, RadialScrollSystem, custom effects |

### CRITICAL RULE

```csharp
// ❌ WRONG: Shaders will NOT work with Deferred mode
spriteBatch.Begin(SpriteSortMode.Deferred, ..., effect: myShader, ...);
spriteBatch.Draw(...); // Shader is IGNORED!

// ✅ CORRECT: Shaders REQUIRE Immediate mode
spriteBatch.Begin(SpriteSortMode.Immediate, ..., effect: myShader, ...);
myShader.CurrentTechnique.Passes["MyPass"].Apply(); // MUST call Apply() before Draw()
spriteBatch.Draw(...); // Shader is applied!
```

---

## 2. SpriteBatch State Management Pattern

### The Standard Restart Pattern

When switching SpriteBatch modes (e.g., AlphaBlend → Additive → Shader → AlphaBlend):

```csharp
public override bool PreDraw(ref Color lightColor)
{
    SpriteBatch sb = Main.spriteBatch;
    
    // ====== STEP 1: End current batch ======
    sb.End();
    
    // ====== STEP 2: Begin with NEW mode (all params explicit!) ======
    sb.Begin(
        SpriteSortMode.Deferred,           // Sort mode
        BlendState.Additive,               // Blend mode
        SamplerState.LinearClamp,          // Texture sampling
        DepthStencilState.None,            // Depth testing
        RasterizerState.CullNone,          // Face culling
        null,                              // Effect (shader) - null for no shader
        Main.GameViewMatrix.TransformationMatrix  // Transform matrix
    );
    
    // ... draw additive content ...
    
    // ====== STEP 3: Restore original state ======
    sb.End();
    sb.Begin(
        SpriteSortMode.Deferred,
        BlendState.AlphaBlend,             // Back to alpha blend
        SamplerState.LinearClamp,
        DepthStencilState.None,
        RasterizerState.CullNone,
        null,
        Main.GameViewMatrix.TransformationMatrix
    );
    
    return true; // Draw default sprite
}
```

### ⚠️ CRITICAL: Specify ALL Parameters

**NEVER** do this:

```csharp
// ❌ WRONG: Missing parameters inherit RANDOM values
sb.End();
sb.Begin(SpriteSortMode.Deferred, BlendState.Additive); // Missing 5 parameters!
```

**ALWAYS** do this:

```csharp
// ✅ CORRECT: All 7 parameters explicit
sb.End();
sb.Begin(
    SpriteSortMode.Deferred,
    BlendState.Additive,
    SamplerState.LinearClamp,
    DepthStencilState.None,
    RasterizerState.CullNone,
    null,
    Main.GameViewMatrix.TransformationMatrix
);
```

---

## 3. Calling VFX Systems That Manage Their Own State

### The Problem

Some VFX systems (like `RadialScrollSystem`) manage their own SpriteBatch state internally:

```csharp
// Inside RadialScrollSystem.DrawOrb():
sb.End();
sb.Begin(Immediate, Additive, ...); // Switches to Immediate
// ... draw with shader ...
sb.End();
sb.Begin(Deferred, AlphaBlend, ...); // ALWAYS restores to AlphaBlend!
```

If you call this **in the middle** of your additive section, it will break subsequent draws:

```csharp
// ❌ WRONG: DrawOrb breaks additive section
sb.Begin(Additive);
DrawSomeBloom();              // Works
RadialScrollSystem.DrawOrb(); // Internally switches to AlphaBlend!
DrawMoreBloom();              // BROKEN - now in AlphaBlend mode!
sb.End();
```

### The Solution: Call State-Managing Systems LAST

```csharp
// ✅ CORRECT: RadialScrollSystem called at END of additive section
sb.End();
sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, ...);

// Draw all additive content FIRST
DrawSomeBloom();
DrawMoreBloom();
DrawTrailEffects();
DrawGlowLayers();

// LAST: Call system that manages its own state
// RadialScrollSystem.DrawOrb() will restore to AlphaBlend internally
RadialScrollSystem.DrawOrb(position, size, config);

// No need to restore AlphaBlend - DrawOrb already did it!
```

### Conditional Calls Pattern

When the VFX system call is conditional, add an else branch:

```csharp
sb.End();
sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, ...);

// Draw all additive content
DrawBloom();
DrawTrail();

// Conditional orb (called LAST in additive section)
if (progress > 0.1f && progress < 0.9f)
{
    // RadialScrollSystem will restore to AlphaBlend
    RadialScrollSystem.DrawOrb(position, size, config);
}
else
{
    // If we didn't call RadialScrollSystem, manually restore
    sb.End();
    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, ...);
}
// State is now AlphaBlend either way
```

---

## 4. Matrix Selection Guidelines

### Which Matrix to Use

| Drawing Context | Matrix | Why |
|-----------------|--------|-----|
| **World-space effects** (trails, beams, projectiles) | `Main.GameViewMatrix.TransformationMatrix` | Follows camera zoom/pan |
| **Screen-space UI** (buffs, health bars) | `Main.UIScaleMatrix` | Fixed to screen |
| **Full-screen post-process** | `Matrix.Identity` | No transformation |

### Example: World-Space Weapon Effect

```csharp
// ✅ CORRECT: World-space matrix for weapon effects
spriteBatch.Begin(
    SpriteSortMode.Deferred,
    BlendState.Additive,
    SamplerState.LinearClamp,
    DepthStencilState.None,
    RasterizerState.CullNone,
    null,
    Main.GameViewMatrix.TransformationMatrix  // World-space
);

// Draw at world position (camera-relative)
Vector2 worldPos = Projectile.Center - Main.screenPosition;
spriteBatch.Draw(glowTexture, worldPos, ...);
```

### Example: Screen-Space UI Overlay

```csharp
// ✅ CORRECT: UI matrix for screen-space effects
spriteBatch.Begin(
    SpriteSortMode.Deferred,
    BlendState.AlphaBlend,
    SamplerState.PointClamp,  // Often point sampling for pixel-perfect UI
    DepthStencilState.None,
    RasterizerState.CullNone,
    null,
    Main.UIScaleMatrix  // Screen-space
);

// Draw at screen position (absolute)
Vector2 screenPos = new Vector2(100, 100);
spriteBatch.Draw(iconTexture, screenPos, ...);
```

---

## 5. Texture Caching Best Practices

### ❌ WRONG: Loading Every Frame

```csharp
public override bool PreDraw(ref Color lightColor)
{
    // BAD: Texture request happens EVERY FRAME
    Texture2D bloom = ModContent.Request<Texture2D>("Path/Bloom").Value;
    Texture2D trail = ModContent.Request<Texture2D>("Path/Trail").Value;
    // ... draw ...
}
```

### ✅ CORRECT: Cache in SetDefaults

```csharp
private static Texture2D _bloomTexture;
private static Texture2D _trailTexture;

public override void SetDefaults()
{
    // Cache textures once during initialization
    _bloomTexture ??= ModContent.Request<Texture2D>("Path/Bloom", AssetRequestMode.ImmediateLoad).Value;
    _trailTexture ??= ModContent.Request<Texture2D>("Path/Trail", AssetRequestMode.ImmediateLoad).Value;
}

public override bool PreDraw(ref Color lightColor)
{
    // Fast access to cached textures
    spriteBatch.Draw(_bloomTexture, ...);
    spriteBatch.Draw(_trailTexture, ...);
}
```

### For Dynamic Textures (Noise, etc.)

```csharp
private static Texture2D _noiseTexture;

public override void SetDefaults()
{
    // Use VFXTextureRegistry for VFX textures with fallbacks
    _noiseTexture ??= VFXTextureRegistry.GetNoiseTexture(NoiseType.Perlin);
}
```

---

## 6. Common Mistakes and Fixes

### Mistake 1: Shader Not Working

**Symptom:** Effect parameter has no visual impact.

```csharp
// ❌ WRONG: Shader ignored in Deferred mode
sb.Begin(SpriteSortMode.Deferred, effect: shader);
sb.Draw(...);

// ✅ FIX: Use Immediate mode + Apply()
sb.Begin(SpriteSortMode.Immediate, ..., effect: shader);
shader.CurrentTechnique.Passes["PassName"].Apply();
sb.Draw(...);
```

### Mistake 2: Additive Draws Appearing AlphaBlend

**Symptom:** Bloom/glow looks dark or has visible edges.

```csharp
// ❌ WRONG: Calling RadialScrollSystem mid-additive
sb.Begin(Additive);
DrawBloom();
RadialScrollSystem.DrawOrb(...);  // Breaks additive!
DrawMoreBloom();                   // Now in AlphaBlend mode!

// ✅ FIX: Call RadialScrollSystem LAST
sb.Begin(Additive);
DrawBloom();
DrawMoreBloom();
RadialScrollSystem.DrawOrb(...);  // Restores AlphaBlend at end
// No need to restore - already AlphaBlend
```

### Mistake 3: Missing Matrix Causes Offset

**Symptom:** Effects appear at wrong position after zoom/pan.

```csharp
// ❌ WRONG: Missing transformation matrix
sb.Begin(SpriteSortMode.Deferred, BlendState.Additive);

// ✅ FIX: Include transformation matrix
sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, 
    SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
    null, Main.GameViewMatrix.TransformationMatrix);
```

### Mistake 4: Nested End/Begin Without Full Restore

**Symptom:** Random visual glitches, wrong blend modes.

```csharp
// ❌ WRONG: Not restoring all state
sb.End();
sb.Begin(Additive); // Missing 5 parameters!
// ... draw ...
sb.End();
sb.Begin(); // Missing ALL parameters!

// ✅ FIX: Always specify all 7 parameters
sb.End();
sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
    SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
    null, Main.GameViewMatrix.TransformationMatrix);
// ... draw ...
sb.End();
sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
    SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
    null, Main.GameViewMatrix.TransformationMatrix);
```

---

## Quick Reference: Standard PreDraw Template

```csharp
public override bool PreDraw(ref Color lightColor)
{
    SpriteBatch sb = Main.spriteBatch;
    Player player = Main.player[Projectile.owner];
    
    // Calculate common values
    Vector2 drawPos = Projectile.Center - Main.screenPosition;
    float progress = /* your progress calculation */;
    
    // ===== PHASE 1: ADDITIVE BLOOM/GLOW =====
    sb.End();
    sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
        SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
        null, Main.GameViewMatrix.TransformationMatrix);
    
    // Draw all additive content here
    DrawBloomLayers(sb, drawPos, progress);
    DrawTrailEffects(sb, drawPos, progress);
    DrawGlowEffects(sb, drawPos, progress);
    
    // ===== PHASE 2: VFX SYSTEMS (call LAST) =====
    // These systems manage their own state and restore to AlphaBlend
    if (shouldDrawOrb)
    {
        RadialScrollSystem.DrawOrb(drawPos, orbSize, orbConfig);
        // State is now AlphaBlend
    }
    else
    {
        // Manual restore if we didn't call the system
        sb.End();
        sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
            SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
            null, Main.GameViewMatrix.TransformationMatrix);
    }
    
    // ===== PHASE 3: DRAW DEFAULT SPRITE =====
    // State is AlphaBlend, ready for default draw
    return true;
}
```

---

## Systems That Manage Their Own SpriteBatch State

| System | File | Behavior |
|--------|------|----------|
| `RadialScrollSystem.DrawOrb()` | `Common/Systems/VFX/Core/RadialScrollSystem.cs` | Restores to AlphaBlend |
| `RadialScrollSystem.DrawPortal()` | Same | Restores to AlphaBlend |
| `RadialScrollSystem.DrawAura()` | Same | Restores to AlphaBlend |
| `BloomPostProcess.Apply()` | `Common/Systems/VFX/Bloom/BloomPostProcess.cs` | Restores to AlphaBlend |

**Always call these systems at the END of your custom rendering section.**

---

*Last Updated: VFXPlus SpriteBatch Standards Compliance Fix - All 6 VFXPlus weapons corrected*
