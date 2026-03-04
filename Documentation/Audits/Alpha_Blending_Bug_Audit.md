# Alpha Blending Bug Audit

**Scope**: All `Content/` directories except `MoonlightSonata` (already fixed) and `FoundationWeapons` (templates).  
**Scanned Themes**: Eroica, EnigmaVariations, SwanLake, DiesIrae, OdeToJoy, LaCampanella, Fate, Nachtmusik, ClairDeLune, Mars, Mercury  
**Bug Pattern**: Glow/bloom textures (designed for additive rendering with black backgrounds) drawn in default `BlendState.AlphaBlend` without the `A = 0` premultiplied alpha trick or an explicit `BlendState.Additive` SpriteBatch restart. This causes visible black rectangles/halos around glow effects.

---

## Summary

| Theme | Buggy Files | Total Bug Locations |
|-------|------------|-------------------|
| EnigmaVariations | 1 | 12 draw calls across 4 projectile classes |
| SwanLake | 1 | 2 draw calls |
| LaCampanella | 4 | 5 draw calls |
| Fate | 1 | 6 draw calls |
| **Total** | **7 files** | **25 buggy draw calls** |

**Themes with no bugs found**: Eroica, DiesIrae, OdeToJoy, Nachtmusik, ClairDeLune, LaCampanella (all existing weapons/projectiles properly use additive helpers or `A = 0` trick), Mars, Mercury (no bloom draw files).

---

## Bug Details

### 1. EnigmaBossProjectiles.cs (EnigmaVariations)

**File**: `Content/EnigmaVariations/Bosses/EnigmaBossProjectiles.cs`

All four projectile classes in this file draw glow textures in `PreDraw` (default AlphaBlend) without any additive blending. Two classes partially use `A = 0` for segment draws but miss it on auxiliary draws (source orbs, end caps).

#### EnigmaWebGlob.PreDraw — Lines 84–86

| Line | Draw Call | Color | Issue |
|------|-----------|-------|-------|
| 84 | `Draw(glow, pos, null, EnigmaGreen, ...)` | `new Color(50, 200, 100)` full alpha | SoftGlow-style `whiteFireEyeA` drawn with A=255 — black background fully visible |
| 85 | `Draw(glow, pos, null, EnigmaPurple * 0.6f, ...)` | ~A=153 | Same texture, black halo at 40% opacity |
| 86 | `Draw(glow, pos, null, Color.White * 0.4f, ...)` | ~A=102 | Same texture, black halo at 60% opacity |

**Fix**: Either add `with { A = 0 }` to each color, or wrap the three draws in `sb.End(); sb.Begin(... BlendState.Additive ...); ... sb.End(); sb.Begin(... BlendState.AlphaBlend ...);`

#### EnigmaShockwave.PreDraw — Lines 361–362

| Line | Draw Call | Color | Issue |
|------|-----------|-------|-------|
| 361 | `Draw(glow, pos, null, EnigmaPurple * alpha, ...)` | Variable A based on `alpha` | `SoftCircle` glow texture drawn without additive |
| 362 | `Draw(glow, pos, null, EnigmaGreen * alpha * 0.6f, ...)` | Variable A | Same texture, smaller layer |

**Fix**: Add `with { A = 0 }` to both colors, or wrap in additive SpriteBatch.

#### EnigmaVoidBeam.PreDraw — Lines 490, 495–496

The beam body loop (L478–491) correctly sets `beamColor.A = 0` at L482, but the hot core layer and source orb draws do not:

| Line | Draw Call | Color | Issue |
|------|-----------|-------|-------|
| 490 | `Draw(glow, drawPos, null, Color.White * 0.3f * intensity, ...)` | ~A=77 | Hot core layer in beam loop — missing `A = 0` |
| 495 | `Draw(glow, sourcePos, null, EnigmaGreen * intensity, ...)` | Variable A | Source orb — no `A = 0` |
| 496 | `Draw(glow, sourcePos, null, EnigmaPurple * 0.5f * intensity, ...)` | Variable A | Source orb outer — no `A = 0` |

**Fix**: L490: Change `Color.White * 0.3f * intensity` to `(Color.White * 0.3f * intensity) with { A = 0 }`. L495–496: Add `with { A = 0 }` to both colors.

#### EnigmaMazeWall.PreDraw — Lines 627–628

The wall segment loop (L611–622) correctly sets `wallColor.A = 0` at L615, but end cap draws do not:

| Line | Draw Call | Color | Issue |
|------|-----------|-------|-------|
| 627 | `Draw(glow, startPos, null, EnigmaPurple * intensity, ...)` | Variable A | Start cap — no `A = 0` |
| 628 | `Draw(glow, endPos, null, EnigmaGreen * intensity, ...)` | Variable A | End cap — no `A = 0` |

**Fix**: Add `with { A = 0 }` to both colors.

---

### 2. DualFeatherQuiver.cs (SwanLake)

**File**: `Content/SwanLake/Accessories/DualFeatherQuiver.cs`  
**Class**: `RainbowFeatherProjectile` (inner projectile class)  
**Method**: `PreDraw` (L658–692)

| Line | Draw Call | Color | Issue |
|------|-----------|-------|-------|
| 679 | `Draw(glowTex, trailPos, null, trailColor, ...)` | `Main.hslToRgb(hue, 1f, 0.7f) * fade * 0.6f` ~A=153 | `SoftGlow` trail afterimage — rainbow glow with black halo |
| 687 | `Draw(glow, drawPos, null, glowColor, ...)` | `Main.hslToRgb(hue, 1f, 0.7f) * 0.7f` ~A=178 | `SoftGlow` outline glow — rainbow glow with black halo |

**Fix**: Either add `with { A = 0 }` to both trail colors, or wrap the glow draws in an additive SpriteBatch section using `SwanLakeVFXLibrary.BeginSwanAdditive(spriteBatch)` / `EndSwanAdditive()`.

---

### 3. BellfireNoteProj.cs (LaCampanella)

**File**: `Content/LaCampanella/ResonantWeapons/GrandioseChime/Projectiles/BellfireNoteProj.cs`  
**Method**: `PreDraw` (L71–86)

| Line | Draw Call | Color | Issue |
|------|-----------|-------|-------|
| 83 | `Draw(bloomTex, ..., GrandioseChimeUtils.BarragePalette[1] * 0.2f, ...)` | ~A=51 | `SoftGlow` fire glow behind note — black halo at 80% background bleed |

**Fix**: Add `with { A = 0 }` to the bloom color.

---

### 4. KillEchoProj.cs (LaCampanella)

**File**: `Content/LaCampanella/ResonantWeapons/GrandioseChime/Projectiles/KillEchoProj.cs`  
**Method**: `PreDraw` (L138–155)  
**Note**: This projectile's `Texture` property IS `SoftGlow` (L21), so the main sprite draw AND the glow are both SoftGlow.

| Line | Draw Call | Color | Issue |
|------|-----------|-------|-------|
| 146 | `Draw(tex, ..., echoColor, ...)` | `EchoPalette[1] * fade * 0.5f` ~A=128 | `SoftGlow` echo orb body — black halo |
| 151 | `Draw(tex, ..., glow, ...)` | `EchoPalette[0] * fade * 0.3f` ~A=77 | `SoftGlow` outer glow ring — black halo |

**Fix**: Add `with { A = 0 }` to both colors.

---

### 5. ResonantNoteProj.cs (LaCampanella)

**File**: `Content/LaCampanella/ResonantWeapons/PiercingBellsResonance/Projectiles/ResonantNoteProj.cs`  
**Method**: `PreDraw` (L82–104)

| Line | Draw Call | Color | Issue |
|------|-----------|-------|-------|
| 100 | `Draw(bloomTex, ..., aura, ...)` | `ResonancePalette[1] * 0.15f * fade` ~A=38 | `SoftGlow` aura scaled to damage radius — subtle but visible black halo |

**Fix**: Add `with { A = 0 }` to the aura color.

---

### 6. SeekingCrystalProj.cs (LaCampanella)

**File**: `Content/LaCampanella/ResonantWeapons/PiercingBellsResonance/Projectiles/SeekingCrystalProj.cs`  
**Method**: `PreDraw` (L126–148)

| Line | Draw Call | Color | Issue |
|------|-----------|-------|-------|
| 143 | `Draw(bloomTex, ..., auraColor, ...)` | `CrystalPalette[2] * 0.2f` ~A=51 | `SoftGlow` crystal aura glow — black halo at 80% background bleed |

**Fix**: Add `with { A = 0 }` to the aura color.

---

### 7. RequiemRealityTear.cs (Fate)

**File**: `Content/Fate/ResonantWeapons/RequiemOfReality/Projectiles/RequiemRealityTear.cs`  
**Method**: `PreDraw` (L144–208)

This is the most severe case — 6 glow/VFX draw calls across 4 layers, all in default AlphaBlend with no additive switching.

| Line | Draw Call | Color | Issue |
|------|-----------|-------|-------|
| 161 | `Draw(_glowTex.Value, ..., RequiemUtils.DarkPink * 0.3f * opacity * pulse, ...)` | ~A=77 | `SoftGlow` wide underlayer — large black halo |
| 164 | `Draw(_glowTex.Value, ..., RequiemUtils.BrightCrimson * 0.2f * opacity, ...)` | ~A=51 | `SoftGlow` wider glow — even larger black halo |
| 174 | `Draw(_glyphTex.Value, ..., RequiemUtils.FatePurple * 0.5f * opacity, ...)` | ~A=128 | Celestial Glyph texture — likely black-bg VFX asset |
| 185 | `Draw(tex.Value, ..., RequiemUtils.BrightCrimson * 0.7f * opacity * pulse, ...)` | ~A=178 | Rift texture — visible black overlay |
| 189 | `Draw(tex.Value, ..., RequiemUtils.SupernovaWhite * 0.4f * opacity, ...)` | ~A=102 | Inner bright core — black halo |
| 199 | `Draw(_supernovaTex.Value, ..., RequiemUtils.SupernovaWhite * 0.5f * opacity * superPulse, ...)` | ~A=128 | Supernova Core hotspot — black halo |

**Fix**: Wrap all draws in `RequiemUtils.BeginAdditive(sb)` / `RequiemUtils.EndAdditive(sb)` (already available in the weapon's utility class — used by RequiemCosmicNote.cs in the same weapon folder), OR add `with { A = 0 }` to each color.

---

## Recommended Fix Pattern

For each buggy draw call, use ONE of these approaches:

### Option A: `with { A = 0 }` trick (no SpriteBatch restart needed)

```csharp
// Before (buggy):
sb.Draw(glow, pos, null, SomeColor * 0.3f, 0f, origin, scale, SpriteEffects.None, 0f);

// After (fixed):
sb.Draw(glow, pos, null, (SomeColor * 0.3f) with { A = 0 }, 0f, origin, scale, SpriteEffects.None, 0f);
```

**Pros**: No SpriteBatch restart cost. Can mix additive glow and alpha-blended opaque draws in the same batch.  
**Cons**: Slightly different from true additive (premultiplied math). Must add to every draw call individually.

### Option B: Theme-specific additive helpers (SpriteBatch restart)

```csharp
// Before (buggy):
sb.Draw(glow, pos, null, SomeColor * 0.3f, ...);

// After (fixed):
ThemeVFXLibrary.BeginThemeAdditive(sb);  // sb.End() + sb.Begin(Additive)
sb.Draw(glow, pos, null, SomeColor * 0.3f, ...);
ThemeVFXLibrary.EndThemeAdditive(sb);    // sb.End() + sb.Begin(AlphaBlend)
```

Available helpers: `EroicaVFXLibrary.BeginEroicaAdditive`, `SwanLakeVFXLibrary.BeginSwanAdditive`, `LaCampanellaVFXLibrary.BeginLaCampanellaAdditive`, `FateVFXLibrary.BeginFateAdditive`, `DiesIraeVFXLibrary.BeginDiesIraeAdditive`, plus weapon-specific helpers like `RequiemUtils.BeginAdditive`, `ResonanceUtils.BeginAdditive`.

**Pros**: True additive blending. Group multiple glow draws in one additive section.  
**Cons**: Two SpriteBatch restarts per section (small perf cost).

---

## Methodology & False Positive Analysis

### Scan Approach

1. PowerShell regex scan across all `.cs` files in target directories
2. Matched files containing glow texture references (`SoftGlow`, `PointBloom`, `SoftRadialBloom`, `SoftCircle`, `whiteFireEyeA`, `GlowOrb`, `LensFlare`, `StarFlare`, `HaloRing`, `FlareCore`, `GlowAndBloom`, `SoftGlow64`, etc.)
3. Filtered to files with `.Draw(` or `.EntitySpriteDraw(` calls
4. Excluded files matching ANY additive pattern: `BlendState.Additive`, `Begin*Additive`, `EnterAdditiveShaderRegion`, `UseAdditiveBlend => true`, `.A = 0`, `with { A = 0 }`, `new Color(..., 0)`, `.Additive(`

### Confirmed False Positives (investigated and cleared)

| File | Theme | Reason Not a Bug |
|------|-------|-----------------|
| EroicaTextures.cs, ChainsawTextures.cs, GardenerFuryTextures.cs, ThornboundTextures.cs | Eroica, OdeToJoy | Texture registry classes — define texture paths, no Draw calls |
| EroicaAccessoryPlayer.cs | Eroica | Uses PointBloom as tiny 3–4px cropped diamond UI markers (not glow usage) |
| SwansMark.cs | SwanLake | Correctly uses `new Color(R, G, B, 0)` constructor with A=0 |
| PinkFlamingBolt.cs | Eroica | Uses `EroicaVFXLibrary.BeginEroicaAdditive()` before bloom draws |
| CipherNocturne.cs, TacetsEnigma.cs | EnigmaVariations | Use `*Utils.EnterAdditiveShaderRegion(sb)` before bloom draws |
| 7 DiesIrae Utils files | DiesIrae | Utility draw methods — all callers (`JudgmentFlameProjectile`, `JudgmentChainProjectile`, `IgnitedWrathBallProjectile`, `EclipseOrbProjectile`, `BlazingShardProjectile`, `SinBulletProjectile`, `FloatingIgnitionProjectile`) set up `BlendState.Additive` before calling |
| 3 DiesIrae ParticleTypes | DiesIrae | Custom particle handlers (`BellParticle`, `SigilParticle`, `DemonParticle` base classes) have `UseAdditiveBlend => true` and their handlers draw additive particles in `BlendState.Additive` |
| 4 LaCampanella ParticleTypes | LaCampanella | Base classes (`GrandioseChimeParticle`, `InfernalChimesParticle`, `PiercingBellsParticle`, `SymphonicBellfireParticle`) all default `UseAdditiveBlend => true`; glow-drawing particles inherit this default. Only non-glow particles (note sprites, smoke) override to `false`. Custom handlers batch by blend mode. |
| ResonanceTrailRenderer.cs | Fate | Uses `SoftGlow` as a 1x1 pixel source rect for line segments (not glow usage). Callers use `ResonanceUtils.BeginAdditive(sb)`. |
| LightAcceleratingBullet.cs | Fate | Correctly uses `coreCol with { A = 0 }` and `bloomCol with { A = 0 }` |
| RequiemCosmicNote.cs | Fate | Uses `RequiemUtils.BeginAdditive(sb)` before glow draws |
| All OdeToJoy projectiles | OdeToJoy | Use `OdeToJoyShaders.BeginAdditiveBatch(sb)` before glow draws |
| All ClairDeLune files | ClairDeLune | All 40+ bloom files use `begin*Additive` or `BlendState.Additive` |
| MovementI/II/III.cs | Eroica | Boss PreDraw draws NPC sprite (proper alpha PNG) for afterimages — not black-bg glow textures |
| ConstellationBoltProjectile.cs | Nachtmusik | Uses `BeginAdditive` helper |
