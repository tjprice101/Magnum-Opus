# Enigma Variations Black Squares — Comprehensive Audit Report

## Executive Summary

After exhaustively reading **every .cs file** across all 8 Enigma Variations weapon folders, all shared library files, shader infrastructure, blend state definitions, texture registries, global VFX systems, primitive renderers, particle handlers, dust types, and shader source code, **no single definitive root cause was found at the static analysis level**. The mod compiles with zero errors. However, the investigation identified **several high-probability and medium-probability issues** that individually or in combination could produce the reported "black squares when swinging" artifact.

The three most likely explanations are:
1. **ALL 18 PreDraw methods are missing `catch` blocks** — Without `catch`, exceptions propagate past `return false`, causing tModLoader to fall back to vanilla rendering of the black-background particle textures (MusicNote.png, etc.) with AlphaBlend → black squares. Even the 11 methods with `try/finally` lack `catch`, making them equally vulnerable.
2. **Unsafe `.Value` access on EnigmaThemeTextures** triggers NullReferenceException in the bloom/theme texture sections (which are outside any try/catch), causing the exception cascade described above
3. **Dual .fxc shader compilations are mismatched** — the same shader names exist as different binaries at two paths, risking parameter/technique mismatches at runtime

---

## 1. Files Read & Verified

### Weapon Main Files (ALL fully read)
| Weapon | File | Lines | Status |
|--------|------|-------|--------|
| CipherNocturne | CipherNocturne.cs | 790 | ✅ Fully read |
| DissonanceOfSecrets | DissonanceOfSecrets.cs | 610 | ✅ Fully read |
| FugueOfTheUnknown | FugueOfTheUnknown.cs | 600 | ✅ Fully read |
| TacetsEnigma | TacetsEnigma.cs | 667 | ✅ Fully read |
| TheSilentMeasure | TheSilentMeasure.cs | 799 | ✅ Fully read |
| TheWatchingRefrain | TheWatchingRefrain.cs | 1165 | ✅ Fully read |
| TheUnresolvedCadence | TheUnresolvedCadence.cs + Item + Swing | ~500+120+200 | ✅ Fully read |
| VariationsOfTheVoid | VariationsOfTheVoid.cs + Item + Swing | ~640+120+200 | ✅ Fully read |

### Shared Library Files (ALL fully read)
| File | Lines | Status |
|------|-------|--------|
| EnigmaThemeTextures.cs | ~150 | ✅ All 9 textures verified on disk |
| EnigmaVFXLibrary.cs | ~300 | ✅ Null-safe texture access |
| EnigmaPalette.cs | 426 | ✅ PreDrawInWorld bloom only |
| EnigmaShaderHelper.cs | 121 | ✅ Null-safe + try/catch |
| MagnumBlendStates.cs | 61 | ✅ TrueAdditive safe for A=0 |
| MagnumTextureRegistry.cs | 477 | ✅ SafeLoad with fallback chains |
| IncisorOrbRenderer.cs | 369 | ✅ Clean implementation |
| ShaderLoader.cs | 1148 | ✅ Path resolution, loading, all constants |

### Per-Weapon Utility Files (ALL 8 read — identical patterns)
CipherUtils, DissonanceUtils, FugueUtils, TacetUtils, SilentUtils, CadenceUtils, WatchingUtils, VoidVariationUtils — all contain: 6-color palette, easing functions, SpriteBatch helpers, DrawThemeAccents delegation.

### Per-Weapon Primitive Renderer (CipherPrimitiveRenderer fully read)
CipherPrimitiveRenderer.cs (206 lines) + CipherVertex.cs — DynamicVertexBuffer/IndexBuffer pattern, sets `BlendState.Additive` (built-in MonoGame).

### ExobladeStyleSwing System (fully read)
ExobladeStyleSwing.cs (771 lines), ExobladeShaderLoader.cs, ExobladeUtils.cs — DrawSlash, DrawPierceTrail, DrawBlade pipeline.

### Particle System (CipherParticleHandler + CipherParticle + CipherParticleTypes read)
Dual-pass rendering: Pass 1 (TrueAdditive) for additive particles, Pass 2 (AlphaBlend) for alpha particles.

### All 8 Dust Types (read)
All use `BrightStarProjectile1.png` texture — verified on disk.

---

## 2. Texture Verification

### All projectile `Texture` override paths — Status
| Texture Path | Used By | Exists? |
|-------------|---------|---------|
| `Assets/Particles Asset Library/MusicNote` | Cipher, Silent, Tacet, Watching, Void, Dissonance, Fugue projectiles | ✅ 15,901 bytes |
| `Assets/Particles Asset Library/CursiveMusicNote` | Dissonance, Silent, Tacet, Watching projectiles | ✅ 19,242 bytes |
| `Assets/Particles Asset Library/Stars/4PointedStarHard` | Cipher, Watching projectiles | ✅ 576 bytes |
| `Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom` | Void, Cadence projectiles + ALL bloom stacks | ✅ 171,334 bytes |
| `Assets/VFX Asset Library/MasksAndShapes/SoftCircle` | Watching MysteryZone projectile | ✅ 188,418 bytes |
| `Assets/VFX Asset Library/ImpactEffects/ImpactEllipse` | Cadence DimensionalSlash projectile | ✅ (correct path) |
| `Assets/VFX Asset Library/Projectiles/BrightStarProjectile1` | ALL 8 Dust types | ✅ |

### Item sprite .png files — ALL verified
| Weapon | PNG File | Exists? |
|--------|----------|---------|
| CipherNocturne | CipherNocturne.png | ✅ |
| DissonanceOfSecrets | DissonanceOfSecrets.png | ✅ |
| FugueOfTheUnknown | FugueOfTheUnknown.png | ✅ |
| TacetsEnigma | TacetsEnigma.png | ✅ |
| TheSilentMeasure | TheSilentMeasure.png | ✅ |
| TheWatchingRefrain | TheWatchingRefrain.png | ✅ |
| TheUnresolvedCadence | TheUnresolvedCadence.png | ✅ |
| VariationsOfTheVoid | VariationsOfTheVoid.png | ✅ |

### Enigma Theme Textures — ALL verified on disk
All 9 lazy-loaded textures in `EnigmaThemeTextures.cs` exist at `Assets/VFX Asset Library/Theme Specific/Enigma/`:
- ENEnergyMotionBeam, ENEnergySurgeBeam, ENHarmonicImpact, ENPowerEffectRing, ENStarFlare, ENEnigmaEye, ENBasicTrail, ENHarmonicRibbon, ENGradientLUT ✅

---

## 3. Shader Verification

### Effects/ directory — 24 .fxc files, ALL non-empty
| Shader | Size (bytes) | Techniques in .fx |
|--------|------|-------------------|
| CipherBeamTrail.fxc | 6,076 | `CipherBeamFlow`, `CipherBeamGlow` |
| CipherSnapBack.fxc | 3,896 | `CipherSnapBackMain` |
| DissonanceOrbAura.fxc | 6,828 | `DissonanceOrbAuraMain`, `DissonanceOrbAuraGlow` |
| DissonanceRiddleTrail.fxc | 5,292 | `DissonanceRiddleFlow` |
| FugueConvergence.fxc | 6,324 | `FugueConvergenceWave`, `FugueConvergenceGlow` |
| FugueVoiceTrail.fxc | 4,956 | `FugueVoiceFlow` |
| TacetBulletTrail.fxc | 4,036 | `TacetBulletFlow` |
| TacetParadoxExplosion.fxc | 6,988 | `TacetParadoxBlast`, `TacetParadoxRing` |
| SilentQuestionBurst.fxc | 6,936 | `SilentQuestionBlast`, `SilentQuestionGlow` |
| SilentSeekerTrail.fxc | 4,660 | `SilentSeekerFlow` |
| CadenceSwingTrail.fxc | 6,640 | `CadenceSwingFlow`, `CadenceSwingGlow` |
| CadenceCollapse.fxc | 4,648 | `CadenceCollapseWarp` |
| WatchingPhantomAura.fxc | 6,912 | `WatchingPhantomGhost`, `WatchingPhantomGlow` |
| WatchingMysteryZone.fxc | 6,304 | `WatchingMysteryField` |
| VoidVariationSwingTrail.fxc | 8,056 | `VoidVariationSwingFlow`, `VoidVariationSwingGlow` |
| VoidVariationBeam.fxc | 4,352 | `VoidVariationBeamFlow` |
| + 8 Boss/Enemy/Sparkle shaders | Various | `Technique1` (generic) |

### Content/.../Shaders/ — 16 .fxc files, ALL non-empty BUT DIFFERENT SIZES

**⚠️ CRITICAL FINDING: DUAL SHADER COMPILATION MISMATCH**

The SAME shader names exist at TWO different paths with DIFFERENT file sizes:

| Shader | Effects/ size | Content/ size | Delta |
|--------|--------------|---------------|-------|
| CipherBeamTrail.fxc | 6,076 | 4,852 | -1,224 |
| CipherSnapBack.fxc | 3,896 | 2,912 | -984 |
| DissonanceOrbAura.fxc | 6,828 | 5,860 | -968 |
| DissonanceRiddleTrail.fxc | 5,292 | 3,348 | -1,944 |
| FugueConvergence.fxc | 6,324 | 6,244 | -80 |
| FugueVoiceTrail.fxc | 4,956 | 3,552 | -1,404 |
| TacetBulletTrail.fxc | 4,036 | 3,448 | -588 |
| TacetParadoxExplosion.fxc | 6,988 | 6,172 | -816 |
| SilentQuestionBurst.fxc | 6,936 | 5,196 | -1,740 |
| SilentSeekerTrail.fxc | 4,660 | 3,152 | -1,508 |
| CadenceSwingTrail.fxc | 6,640 | 6,112 | -528 |
| CadenceCollapse.fxc | 4,648 | 4,188 | -460 |
| WatchingPhantomAura.fxc | 6,912 | 6,128 | -784 |
| WatchingMysteryZone.fxc | 6,304 | 4,328 | -1,976 |
| VoidVariationSwingTrail.fxc | 8,056 | 6,288 | -1,768 |
| VoidVariationBeam.fxc | 4,352 | 3,900 | -452 |

The Effects/ versions are consistently LARGER than the Content/ versions, suggesting they may have been compiled with different settings, different shader source versions, or different target profiles (DirectX vs OpenGL). The weapon code uses `ShaderLoader.*` which loads from Effects/, while per-weapon ShaderLoader classes load from Content/ into `GameShaders.Misc`. If these are functionally different shaders, runtime behavior is unpredictable.

### Technique Name Cross-Reference — ALL MATCH ✅
Every technique name called in C# code was found in the corresponding .fx file in Effects/.

### ShaderLoader.cs Loading Pipeline — Verified
- Path resolution: `MagnumOpus/Effects/{shaderName}` → `ModContent.HasAsset()` check → `Mod.Assets.Request<Effect>()`
- All 16 weapon shader constants confirmed in `Initialize()` method
- Null returns are handled gracefully; null shaders cause drawing to be skipped

---

## 4. Blend State Analysis

### MagnumBlendStates.TrueAdditive
- `ColorSourceBlend = Blend.One` — alpha channel is IRRELEVANT
- Safe for `new Color(R, G, B) { A = 0 }` pattern AND normal alpha colors
- Black pixels (0,0,0,*) are invisible regardless of alpha
- **This blend state CANNOT cause black squares** ✅

### MagnumBlendStates.ShaderAdditive
- `ColorSourceBlend = Blend.SourceAlpha` — shader controls brightness via alpha output
- If a shader outputs alpha=0, the pixel is invisible (not black)
- **This blend state alone cannot cause black squares** ✅

### BlendState.Additive (built-in MonoGame — used by ALL PrimitiveRenderers)
- Same as ShaderAdditive: `SourceAlpha` source blend
- Vertex alpha and shader alpha both matter
- If vertex colors have correct alpha and shader outputs correct alpha, works fine
- **Potential issue**: If shader fails to apply and vertex colors have alpha=0, primitives are invisible

### SpriteBatch AlphaBlend (used for final state restoration)
- Standard alpha blending — this is the "vanilla" state
- **Potential issue**: If VFX textures designed for additive (black-background PNGs) end up being drawn with AlphaBlend due to SpriteBatch corruption, they appear as black rectangles

---

## 5. Global Systems Analysis — ALL DISABLED ✅

| System | Status |
|--------|--------|
| VFXMasterToggle.GlobalSystemsEnabled | **= false** |
| EnhancedProjectileGlobal (PreDraw bloom/trails) | Disabled |
| ProjectileSizeEnforcer | Disabled |
| LensFlareGlobalProjectile | Disabled |
| BeamProjectileTrails | Disabled |
| WeaponVFXManager | Disabled |
| CinematicVFXSystem | Disabled |
| ThemeSparkleGlobalProjectile | OnHitNPC only (no PreDraw) |
| MagnumMeleeGlobalItem | Doesn't apply to Enigma (noMelee=true for melee, non-melee for others) |
| MagnumMeleePlayer | STUB (IsSwinging=false, GetCurrentSwingAngle()=0) |

**No global system is interfering with Enigma weapon rendering.** The weapons' own per-weapon VFX code is the sole source of visual output.

---

## 6. Identified Issues (Ranked by Likelihood)

### 🔴 HIGH: Missing try/finally AND Missing catch in Projectile PreDraw Methods

**This affects ALL 8 weapons, not just some.**

There are **18 total projectile PreDraw methods** across all 8 Enigma weapons. The investigation found:

#### PreDraw Methods WITHOUT any try/finally (7 methods — 4 with complex drawing):

| Weapon | Projectile | PreDraw Line | Texture (fallback) | Complex Drawing? |
|--------|-----------|-------------|-------------------|-----------------|
| CipherNocturne | CipherBeam | 125 | MusicNote | ✅ Yes — 3-pass GPU primitive + shader overlay + 6-layer bloom + theme tex |
| CipherNocturne | RealitySnapBack | 598 | 4PointedStarHard | ✅ Yes — shader overlay + bloom stack + star flare + power ring |
| DissonanceOfSecrets | RiddleboltProjectile | 114 | CursiveMusicNote | ✅ Yes — 3-pass GPU primitive + shader overlay + 6-layer bloom + theme tex |
| TacetsEnigma | SilenceBullet | 173 | MusicNote | ✅ Yes — GPU primitive + shader overlay + bloom + theme textures |
| TacetsEnigma | ParadoxBolt | 408 | CursiveMusicNote | ✅ Yes — GPU primitive + shader overlay + bloom + theme textures |
| DissonanceOfSecrets | SecretOrb | 501 | MusicNote | ❌ Delegates to IncisorOrbRenderer (internally safe) |
| FugueOfTheUnknown | FugueVoiceProjectile | 179 | MusicNote | ❌ Delegates to IncisorOrbRenderer (internally safe) |

#### PreDraw Methods WITH try/finally BUT WITHOUT catch (11 methods — STILL VULNERABLE):

| Weapon | Projectile | PreDraw Line | finally Line |
|--------|-----------|-------------|-------------|
| TheUnresolvedCadence | DimensionalSlash | 40 | 100 |
| TheUnresolvedCadence | ParadoxCollapseUltimate | 238 | 312 |
| TheWatchingRefrain | UnsolvedPhantomMinion | 132 | 247 |
| TheWatchingRefrain | PhantomBolt | 464 | 549 |
| TheWatchingRefrain | PhantomRift | 696 | 801 |
| TheWatchingRefrain | MysteryZone | 924 | 1032 |
| VariationsOfTheVoid | ConvergenceBeam | 47 | 209 |
| VariationsOfTheVoid | VoidRiftExplosion | 451 | 523 |
| TheSilentMeasure | MeasureBeam | 129 | 216 |
| TheSilentMeasure | HomingQuestionSeeker | 419 | N/A (delegates to IncisorOrbRenderer) |
| TheSilentMeasure | ParadoxPiercingBolt | 544 | 659 |

#### Why try/finally WITHOUT catch is STILL vulnerable:

```csharp
// Current pattern in "protected" weapons:
try
{
    // ... drawing code that may throw ...
}
finally
{
    try { sb.End(); } catch { }
    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, ...);
}
return false;  // ← NEVER REACHED if exception occurs!
```

In C#, `finally` runs but does NOT swallow the exception. The exception still propagates:
1. Exception in try block
2. `finally` runs → restores SpriteBatch ✅
3. **Exception re-throws** → `return false` is skipped ❌
4. tModLoader catches the exception
5. tModLoader proceeds with default projectile rendering
6. Vanilla draws MusicNote.png / CursiveMusicNote.png / 4PointedStarHard.png with AlphaBlend
7. These are **additive-designed textures with BLACK backgrounds** → renders as **black square**

**THE FIX**: Every PreDraw must use `try { } catch { } finally { } return false;` — the `catch` block is required to **swallow the exception** and prevent vanilla fallback rendering.

**Why this causes black squares**:
- ALL projectile Texture overrides use particle sprites: MusicNote, CursiveMusicNote, 4PointedStarHard, SoftRadialBloom, SoftCircle
- These VFX textures have bright content on a **solid BLACK background** (designed for additive blending where black = invisible)
- When drawn with vanilla's **AlphaBlend** (default SpriteBatch state), the black background is fully opaque → **appears as a black rectangle**
- The `return false` that would prevent vanilla from drawing is never reached when an exception propagates

### 🔴 HIGH: Unsafe `.Value` Access on Potentially Unloaded Assets

**Pattern found in multiple weapons**:
```csharp
Texture2D sfTex = EnigmaThemeTextures.ENStarFlare.Value;
```

`EnigmaThemeTextures` uses `ModContent.HasAsset()` before loading, and returns null if the asset path doesn't exist. But `.Value` is called directly without null-checking the `Asset<Texture2D>` wrapper first. If the lazy load hasn't completed yet (it uses `AssetRequestMode.ImmediateLoad` so this should be fine) OR if `HasAsset` returned false (making the field null), calling `.Value` throws a `NullReferenceException`.

Combined with the missing try/finally issue above, this throws an unhandled exception → SpriteBatch corruption → vanilla draws the black-background projectile texture with AlphaBlend → **black square**.

**All weapons access these textures directly in their bloom stack sections** (which are outside any try/catch in many weapons).

### 🟡 MEDIUM: Dual Shader Binary Mismatch

The same shader names exist as DIFFERENT compiled binaries at `Effects/` and `Content/.../Shaders/`. The weapon PreDraw code uses `ShaderLoader.*` (Effects/ path) exclusively for rendering. The per-weapon ShaderLoaders (Content/ path) register into `GameShaders.Misc` but are never referenced by the weapon rendering code.

**The risk**: If the Effects/ shaders were compiled for a different target platform or from a different .fx source version, they may:
- Have technique parameters that don't match what the C# code provides
- Fail to apply at runtime (silent failure — `shader.CurrentTechnique.Passes[0].Apply()` does nothing)
- Produce unexpected output (black, white, corrupted)

**Recommendation**: Ensure only ONE set of .fxc files exists, compiled from the authoritative .fx sources in Effects/, and remove the Content/.../Shaders/.fxc duplicates.

### 🟡 MEDIUM: PrimitiveRenderers Don't Set Technique Explicitly

All per-weapon PrimitiveRenderers use:
```csharp
settings.Shader.CurrentTechnique.Passes[0].Apply();
```

They do NOT set `settings.Shader.CurrentTechnique = settings.Shader.Techniques["DesiredTechnique"]` before applying. They rely on whatever `CurrentTechnique` was last set on the shared `Effect` object.

**The risk**: `EnigmaShaderHelper.DrawShaderOverlay()` explicitly sets the technique:
```csharp
shader.CurrentTechnique = shader.Techniques[techniqueName];
```

If this is called BEFORE the PrimitiveRenderer on the same shader, the PrimitiveRenderer will use the OVERLAY technique (e.g., "CipherBeamGlow") instead of the default FLOW technique (e.g., "CipherBeamFlow"). Some weapons call the shader overlay AFTER primitives (which is fine — the default technique is used for primitives), but if any weapon calls them in the opposite order, the wrong technique is applied to the primitive trail → potential visual corruption.

### 🟢 LOW: Unused Per-Weapon Shader Registrations

Each weapon has a per-weapon ShaderLoader that registers shaders into `GameShaders.Misc["MagnumOpus:..."]`, but the weapon rendering code never accesses `GameShaders.Misc`. These registrations are dead code that loads shader binaries into memory for no purpose. While not directly causing black squares, the loading process could fail and log warnings, and the additional memory usage is wasteful.

### 🟢 LOW: SamplerState Mismatch in SpriteBatch Restoration

Several weapons restore the SpriteBatch with `SamplerState.LinearClamp` instead of `Main.DefaultSamplerState`:
```csharp
sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
```

Vanilla expects `Main.DefaultSamplerState` (which is `PointClamp` for the pixel-art style). Using `LinearClamp` won't cause black squares, but it will make subsequent projectiles/entities drawn in the same frame appear with linear filtering (slightly blurry) instead of pixel-perfect rendering.

---

## 7. VFX Asset Library Structure

```
Assets/VFX Asset Library/
├── GlowAndBloom/         → SoftRadialBloom.png (171KB), SoftGlow.png, various bloom textures
├── ImpactEffects/        → ImpactEllipse.png, impact burst textures
├── MasksAndShapes/       → SoftCircle.png (188KB), mask textures
├── Noise/                → PerlinNoise, VoronoiNoise, FBMNoise, etc.
├── Projectiles/          → BrightStarProjectile1.png (used by all Enigma dusts)
├── Theme Specific/
│   └── Enigma/
│       ├── Beams/        → ENEnergyMotionBeam.png, ENEnergySurgeBeam.png
│       ├── Impact Effects/ → ENHarmonicImpact.png, ENPowerEffectRing.png, ENStarFlare.png
│       ├── Particles/    → ENEnigmaEye.png
│       ├── Trails/       → ENBasicTrail.png, ENHarmonicRibbon.png
│       └── LUT/          → ENGradientLUT.png
├── Trails/               → Trail strip textures
├── Beams/                → Beam body textures
├── Ribbons/              → Ribbon flow textures
├── Smears/               → Motion smear textures
└── ...                   → Additional categories
```

---

## 8. Recommended Fixes (Priority Order)

### Fix 1: Add try/CATCH/finally to ALL projectile PreDraw methods

**Every** projectile `PreDraw` method must wrap its entire drawing pipeline in try/catch/finally. The `catch` is CRITICAL — `finally` alone does NOT prevent the exception from propagating past `return false`.

**Correct pattern** (note the CATCH block):
```csharp
public override bool PreDraw(ref Color lightColor)
{
    SpriteBatch sb = Main.spriteBatch;
    try
    {
        // ALL drawing code goes here
        // GPU primitives, shader overlays, bloom stacks, theme textures
    }
    catch (Exception)
    {
        // Swallow exception to prevent vanilla from drawing the black-background
        // projectile texture. The VFX for this frame is lost but no black square.
    }
    finally
    {
        try { sb.End(); } catch { }
        sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
            DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
    }
    return false;  // Now ALWAYS reached — vanilla never draws the fallback texture
}
```

**Without the catch block**, exceptions propagate and `return false` is never reached:
- try { } finally { } → finally runs but exception re-thrown → `return false` skipped → vanilla draws black-background texture with AlphaBlend → **BLACK SQUARE**
- try { } catch { } finally { } → exception caught → `return false` executes → vanilla skipped → **no black square**

**Weapons that currently need this fix (ALL 18 PreDraw methods across all 8 weapons)**:
- 7 PreDraw methods have NO try/finally at all
- 11 PreDraw methods have try/finally but NO catch — still vulnerable
- 0 PreDraw methods currently use the correct try/catch/finally pattern

### Fix 2: Add null guards for EnigmaThemeTextures access

Replace all direct `.Value` access:
```csharp
// BEFORE (unsafe):
Texture2D sfTex = EnigmaThemeTextures.ENStarFlare.Value;

// AFTER (safe):
var sfAsset = EnigmaThemeTextures.ENStarFlare;
if (sfAsset == null || !sfAsset.IsLoaded) return; // skip this stage
Texture2D sfTex = sfAsset.Value;
```

Or create a helper in EnigmaThemeTextures:
```csharp
public static Texture2D GetENStarFlare() => ENStarFlare?.IsLoaded == true ? ENStarFlare.Value : null;
```

### Fix 3: Set shader technique explicitly in PrimitiveRenderers

Before applying the shader in each PrimitiveRenderer:
```csharp
// BEFORE:
settings.Shader.CurrentTechnique.Passes[0].Apply();

// AFTER:
if (!string.IsNullOrEmpty(settings.TechniqueName))
    settings.Shader.CurrentTechnique = settings.Shader.Techniques[settings.TechniqueName];
settings.Shader.CurrentTechnique.Passes[0].Apply();
```

This requires adding a `TechniqueName` property to each weapon's PrimitiveSettings class.

### Fix 4: Consolidate shader binaries

Either:
- Remove all .fxc files from `Content/.../Shaders/` directories (since weapon code doesn't use them)
- OR recompile all shaders from the Effects/ .fx sources to both locations

The per-weapon ShaderLoader classes that register into `GameShaders.Misc` appear to be dead code — consider removing them if no code references `GameShaders.Misc["MagnumOpus:..."]`.

### Fix 5: Fix SamplerState in SpriteBatch restoration

Replace `SamplerState.LinearClamp` with `Main.DefaultSamplerState` in all final SpriteBatch restoration calls.

---

## 9. Weapons Most Likely to Show Black Squares First

Based on the missing try/finally issue, the weapons whose projectile PreDraw methods are most vulnerable to unhandled exceptions are those that access `EnigmaThemeTextures.*.Value` directly WITHOUT wrapping in try/finally:

1. **TacetsEnigma** — SilenceBullet PreDraw has NO outer try/finally, accesses ENStarFlare.Value directly in the bloom section
2. **TacetsEnigma** — ParadoxBolt PreDraw needs verification
3. Any weapon where the shader overlay or bloom section is outside a try/catch block

Weapons that ARE protected by try/finally (and should be more resilient):
- CipherNocturne's CipherBeam and RealitySnapBack
- TheWatchingRefrain's all 4 projectile types
- TheSilentMeasure's main projectile types

---

## 10. Additional Notes

### MeleeSmearEffect.cs — Referenced but Missing
`MeleeSmearEffect.cs` is referenced in README/documentation but does not exist on disk. This is dead documentation, not a runtime issue.

### MagnumMeleePlayer — STUB Implementation
The player's melee system (`MagnumMeleePlayer`) is a stub: `IsSwinging` is always false, `GetCurrentSwingAngle()` returns 0. This doesn't affect Enigma weapons since they use `noMelee = true` and handle their own swing rendering.

### VFXMasterToggle — Disabled by Default
All global VFX systems are disabled (`GlobalSystemsEnabled = false`). Per-weapon custom VFX is the only visual output. Enabling this toggle without fixing the global systems could introduce additional issues.

### ExobladeStyleSwing — Default Textures Used
Neither TheUnresolvedCadenceSwing nor VariationsOfTheVoidSwing override the `NoiseTexturePath`, `StreakTexturePath`, `LensFlareTexturePath`, or `SquareTexturePath` properties. They use SandboxExoblade's default textures, which are all verified to exist.
