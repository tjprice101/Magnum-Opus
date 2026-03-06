# La Campanella Theme — Crash & VFX Audit Report

> **Scope:** All files under `Content/LaCampanella/` including shared infrastructure, all 7 Resonant Weapons, and debuffs.  
> **Focus:** Runtime crashes, SpriteBatch state corruption, shader failures, null references, and VFX correctness.  

---

## Executive Summary

| Severity | Count | Description |
|----------|-------|-------------|
| **WILL CRASH** | 1 | Missing `sb.End()` before `sb.Begin()` — guaranteed `InvalidOperationException` |
| **HIGH RISK** | 5 | Unprotected SpriteBatch transitions, complex state machines with silent error suppression |
| **LOW RISK** | 12 | Bare catch blocks hiding errors, ImmediateLoad in draw methods, minor state concerns |
| **SAFE** | ~20 | Shader loaders, utility classes, particle base types, vertex types |

**The most likely crash source is `DualFatedChimeSwingProj.cs`** (6+ SpriteBatch transitions with silent catch blocks) followed by **`InfernalGeyserProj.cs`** (missing End before Begin — guaranteed crash if shader is loaded).

---

## Systemic Issues (All 7 Weapons)

### 1. Bare `catch { }` Blocks Hide Real Errors — ALL WEAPONS
**Risk: HIGH (silent corruption)**

Every single projectile uses `try { sb.End(); } catch { }` and `try { /* draw code */ } catch { }` patterns. This means:
- If a SpriteBatch End fails, it's silently ignored — the next Begin will throw
- If a shader parameter name is wrong, it's silently ignored — no visual output, no error logged
- If a texture fails to load, it's silently ignored — invisible projectile with no diagnostic

**Recommendation:** Replace bare `catch { }` with `catch (Exception ex) { Mod.Logger.Warn($"VFX failure in {name}: {ex.Message}"); }` to diagnose issues without crashing.

### 2. `ImmediateLoad` Textures Inside Draw Methods — ALL WEAPONS
**Risk: LOW (performance + potential stutter)**

Every PreDraw method loads textures with `AssetRequestMode.ImmediateLoad` per frame:
```csharp
var tex = ModContent.Request<Texture2D>(path, AssetRequestMode.ImmediateLoad).Value;
```
This forces synchronous I/O on every draw frame. The shared `LaCampanellaThemeTextures` class caches correctly with `??=` pattern, but individual projectile textures (bloom, noise, weapon sprites) are re-requested every frame.

**Recommendation:** Cache texture requests as static fields or use `AssetRequestMode.AsyncLoad` with null checks.

### 3. Static `_lastParticleDrawFrame` Deduplication — 4 WEAPONS
**Risk: LOW (visual, not crash)**

`ChimeCycloneProj`, `GrandCrescendoWaveProj`, and other projectiles use a static `_lastParticleDrawFrame` to avoid double-drawing particles. But since this is static per *type*, if two projectiles of the same type exist, only the first one's PreDraw will trigger particle rendering. This is intentional but can cause confusing visual gaps when debugging.

---

## Per-Weapon Audit

---

### 1. DualFatedChime (Melee Sword) — **PRIMARY CRASH SUSPECT**

#### DualFatedChimeSwingProj.cs — **HIGH RISK**
**Path:** `Content/LaCampanella/ResonantWeapons/DualFatedChime/Projectiles/DualFatedChimeSwingProj.cs`  
**Lines:** 842 total

This file has the most complex PreDraw in the entire theme with **6+ SpriteBatch state transitions** across multiple private methods called from PreDraw.

**Issue 1: Silent SpriteBatch corruption cascade (Lines ~630-680)**
```csharp
// PreDraw calls these in sequence:
DrawSlashTrail(sb);      // End → Begin(Immediate, shader) → End → Begin(AlphaBlend)
DrawBloomUnderlays(sb);  // End → Begin(TrueAdditive) → draw → End → Begin(AlphaBlend)
DrawBlade(sb, lightColor);
// + theme accents section: End → Begin(TrueAdditive) → draw → End → Begin(AlphaBlend)
// + particle section
```
Each transition uses `try { sb.End(); } catch { }`. If ANY End fails silently, the next Begin throws `InvalidOperationException` — but that's ALSO caught by `catch { }`. The result is complete visual breakdown where subsequent draw calls silently fail and nothing renders, eventually leading to SpriteBatch corruption that crashes the NEXT projectile's draw call.

**Issue 2: Noise texture loaded per-frame with ImmediateLoad (Lines ~683-691)**
```csharp
try { noiseTex = ModContent.Request<Texture2D>("...", AssetRequestMode.ImmediateLoad)?.Value; } catch { }
```
Inside `DrawSlashTrail`, a noise texture is loaded synchronously every frame. On texture load failure, the bare catch suppresses the error.

**Issue 3: Shader parameter names unchecked (Lines ~670-680)**
```csharp
try { shader.Shader.Parameters["fireColor"]?.SetValue(...); } catch { }
```
While the `?.` prevents null reference, a misspelled parameter name returns null and the shader renders without that parameter — causing visual artifacts instead of the intended effect, with no diagnostic.

**Verdict:** This is almost certainly the DualFatedChime crasher. The cascading SpriteBatch state corruption across 6+ transitions with silent catches means one failure propagates invisibly until something explodes.

---

#### BellFlameWaveProj.cs — **LOW RISK**
**Path:** `Content/LaCampanella/ResonantWeapons/DualFatedChime/Projectiles/BellFlameWaveProj.cs`

PreDraw calls `DrawFlameTrail` (End → primitive render → Begin) then `DrawFlameCore` (End → Begin → draw → End → Begin). Four SpriteBatch transitions. Uses try/catch/finally pattern in the inner methods which is safer than the swing projectile.

**No critical bugs found.** Follows the standard pattern with adequate catch/finally blocks.

---

#### InfernoWaltzProj.cs — **LOW RISK**
**Path:** `Content/LaCampanella/ResonantWeapons/DualFatedChime/Projectiles/InfernoWaltzProj.cs`

PreDraw calls `DrawWaltzAura` (End/Begin/End/Begin) + `DrawWaltzParticles`. Four transitions. Standard pattern.

**No critical bugs found.**

---

#### DualFatedChime.cs (Item) — **SAFE**
Uses `TextureAssets.Item[Type].Value` in PostDrawInWorld — safe since the item must be loaded to render.

#### DualFatedChimeParticleHandler.cs — **SAFE**
DrawAllParticles uses 2-pass draw with try/catch/finally. Finally block properly restores SpriteBatch.

#### DualFatedChimePrimitiveRenderer.cs — **SAFE**
Saves/restores all GraphicsDevice states in try/finally. Returns early if vertexCount < 4. Proper null checks.

#### DualFatedChimeShaderLoader.cs — **SAFE**
Bool flag + try/catch getter pattern. All getters return null on failure.

#### DualFatedChimeUtils.cs — **SAFE**
Pure utility, no draw methods.

#### DualFatedChimePlayer.cs — **SAFE**
Simple ModPlayer state tracking.

#### DualFatedChimeParticle.cs — **SAFE**
Abstract base class.

#### DualFatedChimeParticleTypes.cs — **LOW RISK**
Four particle types load textures with `ImmediateLoad` in Draw() using static caching (`??=`). `MusicalFlameParticle` has nested try/catch for MusicNote fallback to SoftGlow. All safe patterns.

#### DualFatedChimeVertexType.cs — **SAFE**
Pure data struct.

---

### 2. FangOfTheInfiniteBell (Magic Weapon) — **LOW RISK**

#### InfiniteBellOrbProj.cs — **LOW RISK**
**Path:** `Content/LaCampanella/ResonantWeapons/FangOfTheInfiniteBell/Projectiles/InfiniteBellOrbProj.cs`  
**Lines:** 587 total

Standard PreDraw pattern with try/catch/finally for additive section. Uses `handler?.DrawAllParticles(sb)` — safe null check. Trail ring buffer uses modulo indexing — bounds safe.

Bloom overlay section uses try/catch/finally pattern. No critical bugs.

#### EmpoweredLightningProj.cs — **LOW RISK**
**Path:** `Content/LaCampanella/ResonantWeapons/FangOfTheInfiniteBell/Projectiles/EmpoweredLightningProj.cs`

Uses `DrawImpactBloom` with try/catch/finally. Unusual indentation on some lines suggests copy-paste origin but logic is sound. Finally block properly restores state.

#### FangOfTheInfiniteBellShaderLoader.cs — **SAFE**

---

### 3. GrandioseChime (Ranged Weapon) — **HIGH RISK (2 files)**

#### BellfireNoteProj.cs — **HIGH RISK**
**Path:** `Content/LaCampanella/ResonantWeapons/GrandioseChime/Projectiles/BellfireNoteProj.cs`

**CRITICAL: Unprotected SpriteBatch transitions**
```csharp
// Around lines 100-109:
sb.End();   // ← NO try/catch — will throw if SpriteBatch not in Begin state
sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, ...);
// ... draws ...
sb.End();   // ← NO try/catch
sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, ...);
```
These bare End/Begin calls will crash if the SpriteBatch is in an unexpected state. Unlike every other projectile in this theme, this one has **zero error handling** around its blend state transitions.

**Fix:** Wrap in try/catch/finally:
```csharp
try { sb.End(); } catch { }
try
{
    sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, ...);
    // draws
}
catch { }
finally
{
    try { sb.End(); } catch { }
    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, ...);
}
```

#### KillEchoProj.cs — **HIGH RISK**
**Path:** `Content/LaCampanella/ResonantWeapons/GrandioseChime/Projectiles/KillEchoProj.cs`

**CRITICAL: Completely unprotected PreDraw**
```csharp
public override bool PreDraw(ref Color lightColor)
{
    SpriteBatch sb = Main.spriteBatch;
    var tex = ModContent.Request<Texture2D>(Texture, AssetRequestMode.ImmediateLoad).Value; // ← Can throw!

    sb.End();   // ← NO try/catch
    sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, ...);
    // ... draws ...
    sb.End();   // ← NO try/catch
    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, ...);
    return false;
}
```
- `.Value` on the texture request with no null check — throws `NullReferenceException` if asset is missing
- Bare `sb.End()`/`sb.Begin()` with no error handling at all
- No outer try/catch — any exception here propagates up to Terraria's draw loop

**Fix:** Add outer try/catch + null check on texture + protected End/Begin.

#### NoteMineProj.cs — **LOW RISK**
Standard pattern with try/catch/finally for the additive section. Outer try/catch exists. Safe.

#### GrandioseBeamProj.cs — **LOW RISK**
Standard pattern. Loads texture with ImmediateLoad in PreDraw (performance concern only).

#### GrandioseChimeShaderLoader.cs — **SAFE**

---

### 4. IgnitionOfTheBell (Melee Spear) — **WILL CRASH (1 file)**

#### InfernalGeyserProj.cs — **WILL CRASH**
**Path:** `Content/LaCampanella/ResonantWeapons/IgnitionOfTheBell/Projectiles/InfernalGeyserProj.cs`  
**Lines:** ~280 (DrawGeyserPillar method)

**CRITICAL BUG: Missing sb.End() before sb.Begin()**

```csharp
public override bool PreDraw(ref Color lightColor)
{
    SpriteBatch sb = Main.spriteBatch;
    DrawGeyserPillar(sb);  // ← SpriteBatch is ALREADY in Begin state from Terraria
    return false;
}

private void DrawGeyserPillar(SpriteBatch sb)
{
    // ... setup ...
    if (geyserShader != null && heightMult > 0.1f)
    {
        // ... shader params ...
        sb.Begin(SpriteSortMode.Immediate, MagnumBlendStates.ShaderAdditive, ...);
        //     ↑↑↑ CRASH: Begin called without End! SpriteBatch is already started!
```

When `PreDraw` is called by Terraria, the SpriteBatch is already in an active `Begin` state. `DrawGeyserPillar` jumps directly to `sb.Begin(Immediate, ...)` without first calling `sb.End()`. This throws:
```
System.InvalidOperationException: Begin cannot be called again until End has been successfully called.
```

This crash occurs **only when the geyser shader loads successfully** (`geyserShader != null`) AND the geyser height is > 10% (`heightMult > 0.1f`). If the shader fails to load, it skips to the non-shader path which has the same bug — `sb.Begin(Deferred, TrueAdditive, ...)` without End.

**Fix:** Add `try { sb.End(); } catch { }` before the first `sb.Begin()` in DrawGeyserPillar.

#### ChimeCycloneProj.cs — **LOW RISK**
Uses try/catch/finally in DrawCycloneAura. Finally block restores SpriteBatch. Outer PreDraw has try/catch. Safe.

#### IgnitionThrustProj.cs — **LOW RISK**
Standard try/catch/finally pattern for additive sections. DrawLanceSprite has a nested fire overlay section with its own try/catch/finally. Safe.

#### IgnitionOfTheBellShaderLoader.cs — **SAFE**

---

### 5. InfernalChimesCalling (Summoner Staff) — **LOW RISK**

#### CampanellaChoirMinion.cs — **LOW RISK**
**Path:** `Content/LaCampanella/ResonantWeapons/InfernalChimesCalling/Projectiles/CampanellaChoirMinion.cs`  
**Lines:** 641 total

Complex minion with bell formation, sequential attacks, and crescendo mechanic. PreDraw has multiple nested SpriteBatch transitions:
1. Trail rendering (2-pass shader)
2. Sprite + bloom overlay (try/catch/finally)
3. Crescendo charge shader aura (nested try/catch)
4. Theme accents (End/Begin/End/Begin)
5. Outer try/catch

The crescendo shader section (lines ~550-590) has a nested try/catch inside a try/catch/finally — this is correct and handles shader failure gracefully by falling back to the TrueAdditive state.

**Minor issue:** The theme accents section (lines ~600-610) uses bare `try { sb.End(); } catch { }` outside the finally block. If this End fails silently, the next Begin may duplicate.

#### MinionShockwaveProj.cs — **LOW RISK**
Standard triple-nested try/catch/finally pattern. Shader section has its own catch block that restores TrueAdditive state. Outer finally restores AlphaBlend. Clean.

#### InfernalChimesCallingShaderLoader.cs — **SAFE**

---

### 6. PiercingBellsResonance (Ranged Gun) — **HIGH RISK (2 files)**

#### ResonantNoteProj.cs — **HIGH RISK**
**Path:** `Content/LaCampanella/ResonantWeapons/PiercingBellsResonance/Projectiles/ResonantNoteProj.cs`

**Unprotected SpriteBatch transitions at outer level:**
```csharp
if (bloomTex != null)
{
    sb.End();   // ← NO try/catch
    sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, ...);
    sb.Draw(...);
    
    // ... shader section with try/catch (fine) ...
    
    sb.End();   // ← NO try/catch
    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, ...);
}
```
The outer End/Begin calls have no error protection. If the inner shader section leaves the SpriteBatch in an unexpected state (e.g., shader catch block fails its own restoration), the outer End will throw.

#### SeekingCrystalProj.cs — **HIGH RISK**
**Path:** `Content/LaCampanella/ResonantWeapons/PiercingBellsResonance/Projectiles/SeekingCrystalProj.cs`

Same pattern as ResonantNoteProj — unprotected outer End/Begin:
```csharp
sb.End();   // ← NO try/catch
sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, ...);
// ... shader section with try/catch ...
sb.End();   // ← NO try/catch  
sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, ...);
```

Also has `ModContent.Request<Texture2D>(...).Value` for bloom texture with no null check — will throw if asset is missing.

#### StaccatoBulletProj.cs — **LOW RISK**
Standard pattern with try/catch for transitions. Safe.

#### ResonantBlastProj.cs — **LOW RISK**
Triple-nested try/catch/finally. Clean implementation.

#### PiercingBellsResonanceShaderLoader.cs — **SAFE**

---

### 7. SymphonicBellfireAnnihilator (Ranged Launcher) — **LOW RISK**

#### BellfireRocketProj.cs — **LOW RISK**
Standard pattern. Uses `npc.SimpleStrikeNPC()` for AoE in OnKill — correct. Try/catch/finally for additive section.

#### GrandCrescendoWaveProj.cs — **LOW RISK**
Well-structured triple-nested try/catch/finally. Shader section has proper catch/restore. Clean implementation. Static `_lastParticleDrawFrame` for particle dedup — intentional.

#### SymphonicBellfireShaderLoader.cs — **SAFE**

---

### Shared Infrastructure

#### LaCampanellaShaderManager.cs — **SAFE**
All shader accesses use `ShaderLoader.HasShader()` checks. Null-conditional `?.SetValue()` on parameters. Early returns on null shader. Helper methods `BeginShaderAdditive` and `RestoreSpriteBatch` are correct.

#### LaCampanellaVFXLibrary.cs — **SAFE**
All draw helpers (DrawRadialSlashStar, DrawPowerEffectRing, DrawImpactEllipse, DrawBeamLensFlare, DrawInfernalBeamRing, DrawBrightStar, DrawThemeImpactRing, DrawThemeStarFlare, DrawThemeBellFlare, DrawThemeImpactFull) use:
- `if (tex == null || !tex.IsLoaded) return;` — proper null/load guard
- `{A = 0}` additive pattern — correct for AlphaBlend spritebatch with additive appearance
- No SpriteBatch state changes — caller must set blend state beforehand

`LaCampanellaThemeTextures` class uses `ModContent.HasAsset(path)` before requesting, returns null on failure. `??=` caching pattern. Safe.

#### BellResonanceNPC.cs — **SAFE**
GlobalNPC with InstancePerEntity. Bell Shatter damage uses `NPC.HitInfo` struct correctly. DrawEffects spawns dust — lightweight. No SpriteBatch manipulation.

#### ResonantTollNPC.cs — **SAFE** (per grep result, standard GlobalNPC debuff)

---

## Priority Fix List

### P0 — Fix Immediately (WILL CRASH)

| File | Issue | Fix |
|------|-------|-----|
| **InfernalGeyserProj.cs** `DrawGeyserPillar()` | Missing `sb.End()` before `sb.Begin(Immediate, ...)` — crashes when shader is loaded | Add `try { sb.End(); } catch { }` before the first `sb.Begin()` inside `DrawGeyserPillar` |

### P1 — Fix Before Release (HIGH RISK)

| File | Issue | Fix |
|------|-------|-----|
| **DualFatedChimeSwingProj.cs** | 6+ SpriteBatch transitions with bare `catch { }` — silent state corruption cascades, most likely cause of reported DualFatedChime crashes | Refactor to use try/catch/finally pattern with logging; reduce transition count if possible |
| **KillEchoProj.cs** | Zero error handling in PreDraw — bare End/Begin + unguarded `.Value` on texture request | Wrap entire PreDraw in try/catch; add null check on texture; protect End/Begin |
| **BellfireNoteProj.cs** | Unprotected `sb.End()`/`sb.Begin()` calls | Wrap transitions in try/catch/finally |
| **ResonantNoteProj.cs** | Unprotected outer `sb.End()`/`sb.Begin()` around bloom block | Wrap in try/catch/finally |
| **SeekingCrystalProj.cs** | Unprotected `sb.End()`/`sb.Begin()` + unguarded `.Value` on bloom texture | Add try/catch + null checks |

### P2 — Improve Quality (LOW RISK)

| Category | Files | Fix |
|----------|-------|-----|
| Add logging to catch blocks | ALL projectile files | Replace `catch { }` with `catch (Exception ex) { Mod.Logger.Warn(...); }` |
| Cache texture requests | ALL projectile PreDraw methods | Use static `??=` fields instead of per-frame `ImmediateLoad` |
| Validate asset paths at load time | LaCampanellaThemeTextures | Log warning if `ModContent.HasAsset()` returns false for any expected asset |

---

## PreDraw Return Value Check

All 20 projectile files correctly `return false` from PreDraw, preventing Terraria's default draw from running after custom rendering. **No double-draw issues.**

---

## Summary by File

| File | Risk | Key Issue |
|------|------|-----------|
| DualFatedChimeSwingProj.cs | **HIGH** | 6+ transitions, silent catch cascade |
| BellFlameWaveProj.cs | LOW | Standard pattern |
| InfernoWaltzProj.cs | LOW | Standard pattern |
| DualFatedChime.cs | SAFE | — |
| DualFatedChimeParticleHandler.cs | SAFE | — |
| DualFatedChimePrimitiveRenderer.cs | SAFE | — |
| DualFatedChimeShaderLoader.cs | SAFE | — |
| DualFatedChimeUtils.cs | SAFE | — |
| DualFatedChimePlayer.cs | SAFE | — |
| DualFatedChimeParticle.cs | SAFE | — |
| DualFatedChimeParticleTypes.cs | LOW | ImmediateLoad in Draw (cached) |
| DualFatedChimeVertexType.cs | SAFE | — |
| InfiniteBellOrbProj.cs | LOW | Standard pattern |
| EmpoweredLightningProj.cs | LOW | Standard pattern |
| FangOfTheInfiniteBellShaderLoader.cs | SAFE | — |
| GrandioseBeamProj.cs | LOW | ImmediateLoad in PreDraw |
| BellfireNoteProj.cs | **HIGH** | Unprotected End/Begin |
| NoteMineProj.cs | LOW | Standard pattern |
| KillEchoProj.cs | **HIGH** | Zero error handling, unguarded .Value |
| IgnitionThrustProj.cs | LOW | Standard pattern |
| InfernalGeyserProj.cs | **CRASH** | Missing End before Begin |
| ChimeCycloneProj.cs | LOW | Standard pattern |
| CampanellaChoirMinion.cs | LOW | Complex but adequate error handling |
| MinionShockwaveProj.cs | LOW | Clean triple-nested pattern |
| StaccatoBulletProj.cs | LOW | Standard pattern |
| ResonantBlastProj.cs | LOW | Clean implementation |
| ResonantNoteProj.cs | **HIGH** | Unprotected outer End/Begin |
| SeekingCrystalProj.cs | **HIGH** | Unprotected End/Begin + unguarded .Value |
| BellfireRocketProj.cs | LOW | Standard pattern |
| GrandCrescendoWaveProj.cs | LOW | Clean implementation |
| LaCampanellaShaderManager.cs | SAFE | — |
| LaCampanellaVFXLibrary.cs | SAFE | — |
| BellResonanceNPC.cs | SAFE | — |
| All 7 ShaderLoader files | SAFE | — |
