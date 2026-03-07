# Comprehensive Bloom/Glow Audit v2

**Goal:** Every bloom/glow draw where on-screen pixel size exceeds 300px.  
**Rule:** No bloom may exceed 300px on screen.  
**Date:** Current codebase state.

---

## Texture Size Reference

| Texture | Source | Size (px) | Max Scale for 300px |
|---------|--------|-----------|---------------------|
| `SoftRadialBloom` | VFX Asset Library/GlowAndBloom/ | 2160 | 0.139 |
| `PointBloom` | VFX Asset Library/GlowAndBloom/ | 2160 | 0.139 |
| `GetBloom()` | MagnumTextureRegistry (→ PointBloom → SoftRadialBloom → SoftGlow fallback) | 2160 | 0.139 |
| `GetPointBloom()` | MagnumTextureRegistry | 2160 | 0.139 |
| `GetRadialBloom()` | MagnumTextureRegistry (→ SoftRadialBloom → GetBloom fallback) | 2160 | 0.139 |
| `GetSoftGlow()` | MagnumTextureRegistry (Sandbox SoftGlow) | 512 | 0.586 |
| `SoftGlow` (VFX Library) | VFX Asset Library/GlowAndBloom/SoftGlow | 1024 | 0.293 |
| `SoftGlow` (per-weapon textures) | Per-weapon TextureClass → VFX Asset Library path | 1024 | 0.293 |
| `GlowOrb` | VFX Asset Library | 1024 | 0.293 |
| `StarFlare` | VFX Asset Library | 1024 | 0.293 |
| `BloomCircle` (Particle_BloomCircle) | Particles Asset Library | 200 | 1.500 |
| `SoftCircle` (MasksAndShapes) | VFX Asset Library | 2160 | 0.139 |

> ⚠ **TWO-FILE GOTCHA:** `MagnumTextureRegistry.SoftGlow` (512px Sandbox path) vs `VFX Asset Library/GlowAndBloom/SoftGlow` (1024px) share the same name but are DIFFERENT textures.

---

## SECTION 1: SYSTEMIC ISSUES (Affect Multiple Callers)

### 1A. All VFXLibrary `DrawXxxBloomStack()` Methods

**Affected files:**
- `Content/Eroica/EroicaVFXLibrary.cs` — `DrawEroicaBloomStack`
- `Content/MoonlightSonata/MoonlightVFXLibrary.cs` — `DrawMoonlightBloomStack`
- `Content/Nachtmusik/NachtmusikVFXLibrary.cs` — `DrawNachtmusikBloomStack`
- `Content/DiesIrae/DiesIraeVFXLibrary.cs` — `DrawDiesIraeBloomStack`
- `Content/LaCampanella/LaCampanellaVFXLibrary.cs` — `DrawLaCampanellaBloomStack`
- `Content/Fate/FateVFXLibrary.cs` — `DrawFateBloomStack`
- `Content/EnigmaVariations/EnigmaVFXLibrary.cs` — `DrawEnigmaBloomStack`
- `Content/SwanLake/SwanLakeVFXLibrary.cs` — (same pattern)

**Pattern (ALL identical):**
```csharp
// Uses GetBloom() = 2160px texture
Layer 1 (outer):  scale * 2.0   → OVERSIZED if scale > 0.069
Layer 2 (mid):    scale * 1.4   → OVERSIZED if scale > 0.099
Layer 3 (inner):  scale * 0.9   → OVERSIZED if scale > 0.154
Layer 4 (core):   scale * 0.4   → OVERSIZED if scale > 0.347
```

**Any caller passing `scale > 0.069` exceeds 300px on the outer layer.**

#### Known Callers with Oversized Scale:

| File | Line | Call | Scale Passed | Outer Layer (px) |
|------|------|------|-------------|-----------------|
| `Content/Eroica/Weapons/SakurasBlossom/SakurasBlossomSwing.cs` | 793 | `DrawEroicaBloomStack(sb, tipPos, ..., 0.25f + phase * 0.06f, ...)` | max 0.37 (phase=2) | **1,598px** 🔴 |
| `Content/LaCampanella/Accessories/LaCampanellaAccessoryVFX.cs` | 323 | `DrawBloom(center, 1.2f)` | 1.2 | **5,184px** 🔴🔴🔴 |
| `Content/LaCampanella/Accessories/LaCampanellaAccessoryVFX.cs` | 829 | `DrawBloom(targetCenter, 0.7f)` | 0.7 | **3,024px** 🔴🔴 |
| `Content/LaCampanella/Accessories/LaCampanellaAccessoryVFX.cs` | 705 | `DrawBloom(targetCenter, 0.4f)` | 0.4 | **1,728px** 🔴 |
| `Content/LaCampanella/Accessories/LaCampanellaAccessoryVFX.cs` | 874 | `DrawBloom(enemyCenter, 0.3f)` | 0.3 | **1,296px** 🔴 |
| `Content/Fate/ResonantWeapons/RequiemOfRealityVFX.cs` | 251 | `FateVFXLibrary.DrawBloom(playerCenter, 0.6f)` | 0.6 | **2,592px** 🔴🔴 |
| `Content/Nachtmusik/Weapons/NebulasWhisper/Projectiles/NebulaWhisperShot.cs` | 284 | `DrawBloom(Projectile.Center, 0.3f * currentScale, ...)` | 0.3 (if currentScale=1) | **1,296px** 🔴 |
| `Content/DiesIrae/Accessories/DiesIraeAccessoryVFX.cs` | 298 | `DrawBloom(playerCenter, 0.3f)` | 0.3 | **1,296px** 🔴 |

### 1B. All VFXLibrary `DrawBloomSandwichLayer()` Methods

**Pattern (ALL identical):**
```csharp
// Uses GetBloom() = 2160px texture
Behind layer 1: scale * 2.5   → OVERSIZED if scale > 0.056
Behind layer 2: scale * 1.6   → OVERSIZED if scale > 0.087
Front layer 1:  scale * 0.8   → OVERSIZED if scale > 0.174
Front layer 2:  scale * 0.35  → OVERSIZED if scale > 0.397
```

**No direct callers found in weapon code.** The existing audit confirms `DrawBloomSandwichLayer` is "never called" from weapons. But the method exists and would produce massive draws if ever called with typical scale values.

### 1C. `BloomRenderer.DrawBloomStack()` / `DrawBloomStackAdditive()`

**File:** `Common/Systems/VFX/Bloom/BloomRenderer.cs`

Same `scale * 2.0` outer layer pattern as VFXLibraries. All callers passing `scale > 0.069` exceed 300px.

**Known callers:** All `VFXLibrary.DrawBloom()` convenience methods delegate to `BloomRenderer.DrawBloomStackAdditive()`.

### 1D. `MeleeSwingBase.DrawLensFlareInner()`

**File:** `Common/BaseClasses/MeleeSwingBase.cs` L674

```csharp
Texture2D flareTex = MagnumTextureRegistry.GetRadialBloom(); // 2160px
float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.15f;
float baseScale = (0.25f + ComboStep * 0.08f) * pulse;
// ComboStep 0-4 → baseScale max = (0.25 + 0.32) × 1.15 = 0.655

sb.Draw(flareTex, ..., baseScale, ...);           // 2160 × 0.655 = 1,415px 🔴
sb.Draw(flareTex, ..., baseScale * 0.7f, ...);    // 2160 × 0.459 = 990px 🔴
sb.Draw(flareTex, ..., baseScale * 0.35f, ...);   // 2160 × 0.229 = 495px 🔴
```

**Max computed: 1,415px** 🔴  
**Affects ALL weapons inheriting from MeleeSwingBase** (Nachtmusik melee weapons and potentially others).

### 1E. `BloomParticles.BloomCoreParticle.CustomDraw()`

**File:** `Common/Systems/VFX/Bloom/BloomParticles.cs` L87

```csharp
Texture2D bloom = MagnumTextureRegistry.GetBloom(); // 2160px
spriteBatch.Draw(bloom, drawPos, null, drawColor, 0f, origin, _scaleVector, ...);
```

**`_scaleVector` is set by callers' `StartScale`/`EndScale`.** Any spawn with scale > 0.139 exceeds 300px. This is a systemic issue that depends on every individual particle spawn call.

---

## SECTION 2: VERIFIED OVERSIZED FILES (Current State Confirmed)

### MoonlightSonata Theme

#### 2.1 PrismaticDetonation.cs
**File:** `Content/MoonlightSonata/Weapons/MoonlightsCalling/Projectiles/PrismaticDetonation.cs`  
**Texture:** SoftRadialBloom (2160px)

| ~Line | Scale Expression | Max Scale | Max Pixels | Status |
|-------|-----------------|-----------|------------|--------|
| ~222 | `scale * 1.3` | scale up to 8 | **22,464px** | 🔴🔴🔴 ASTRONOMICAL |
| ~224 | `scale` | 8 | **17,280px** | 🔴🔴🔴 ASTRONOMICAL |
| ~228 | `scale * 0.3` | 2.4 | **5,184px** | 🔴🔴 |

```csharp
// scale ramps from 0 to ~8 during detonation
sb.Draw(softBloom, drawPos, null, ..., scale * 1.3f, ...);
sb.Draw(softBloom, drawPos, null, ..., scale, ...);
sb.Draw(softBloom, drawPos, null, ..., scale * 0.3f, ...);
```

#### 2.2 SerenadeHoldout.cs
**File:** `Content/MoonlightSonata/Weapons/MoonlightsCalling/Projectiles/SerenadeHoldout.cs`  
**Texture:** SoftRadialBloom (2160px)

| ~Line | Scale Expression | Max Scale | Max Pixels | Status |
|-------|-----------------|-----------|------------|--------|
| ~596 | `(1.5f + res * 0.2f) * intensity` | ~1.5 | **3,240px** | 🔴🔴 |
| ~600 | `(0.8f + res * 0.1f) * intensity` | ~0.8 | **1,728px** | 🔴 |
| ~603 | `0.3f * intensity` | 0.3 | **648px** | 🔴 |

#### 2.3 SpectralChildBeam.cs
**File:** `Content/MoonlightSonata/Weapons/MoonlightsCalling/Projectiles/SpectralChildBeam.cs`  
**Texture:** PointBloom (2160px)

| ~Line | Scale Expression | Max Scale | Max Pixels | Status |
|-------|-----------------|-----------|------------|--------|
| ~181 | `0.7f * pulse` | 0.77 | **1,663px** | 🔴 |
| ~182 | `0.3f * pulse` | 0.33 | **713px** | 🔴 |

#### 2.4 SerenadeBeam.cs
**File:** `Content/MoonlightSonata/Weapons/MoonlightsCalling/Projectiles/SerenadeBeam.cs`  
**Texture:** SoftRadialBloom (2160px)

| Line | Scale Expression | Max Scale | Max Pixels | Status |
|------|-----------------|-----------|------------|--------|
| 482 | `0.18f * bounceIntensity * pulse` | ~0.207 | **447px** | 🔴 |

*(Note: Line 278 is a particle spawn with scale `1.8f + intensity * 0.4f` — the particle's own Draw must be checked separately.)*

#### 2.5 EternalMoonWave.cs
**File:** `Content/MoonlightSonata/Weapons/EternalMoon/Projectiles/EternalMoonWave.cs`  
**Texture:** PointBloom (2160px)

| ~Line | Scale Expression | Max Scale | Max Pixels | Status |
|-------|-----------------|-----------|------------|--------|
| ~179 | `0.3f * Projectile.scale` | variable | **648px+** | 🔴 |

#### 2.6 GoliathMoonlightBeam.cs
**File:** `Content/MoonlightSonata/Bosses/GoliathOfMoonlight/Projectiles/GoliathMoonlightBeam.cs`  
**Texture:** SoftRadialBloom (2160px)

| ~Line | Scale Expression | Max Scale | Max Pixels | Status |
|-------|-----------------|-----------|------------|--------|
| ~342 | `outerScale * 0.7f` | max 0.385 | **832px** | 🔴 |
| ~347 | `outerScale * 0.35f` | max 0.193 | **416px** | 🔴 |
| ~354 | `coreScale` | max 0.16 | **346px** | 🔴 |

#### 2.7 GoliathDevastatingBeam.cs
**File:** `Content/MoonlightSonata/Bosses/GoliathOfMoonlight/Projectiles/GoliathDevastatingBeam.cs`  
**Texture:** SoftRadialBloom (2160px)

| ~Line | Scale Expression | Max Scale | Max Pixels | Status |
|-------|-----------------|-----------|------------|--------|
| ~355 | max 0.75f | 0.75 | **1,620px** | 🔴 |
| ~358 | max 0.45f | 0.45 | **972px** | 🔴 |
| ~363 | max 0.26f | 0.26 | **562px** | 🔴 |

#### 2.8 IncisorSwingProj.cs
**File:** `Content/MoonlightSonata/Weapons/IncisorOfMoonlight/Projectiles/IncisorSwingProj.cs`  
**Texture:** SoftRadialBloom (2160px)

| ~Line | Scale Expression | Max Scale | Max Pixels | Status |
|-------|-----------------|-----------|------------|--------|
| ~505 | `0.4f * starPulse * Projectile.scale` | ~0.4 | **864px** | 🔴 |

#### 2.9 CrescentWaveProj.cs
**File:** `Content/MoonlightSonata/Weapons/IncisorOfMoonlight/Projectiles/CrescentWaveProj.cs`  
**Texture:** SoftRadialBloom (2160px)

| ~Line | Scale Expression | Max Scale | Max Pixels | Status |
|-------|-----------------|-----------|------------|--------|
| ~140 | `ringScale * 1.5f` | ringScale = MaxRadius/texSize → huge | **8,856px+** | 🔴🔴🔴 ASTRONOMICAL |

#### 2.10 StaccatoNoteProj.cs
**File:** `Content/MoonlightSonata/Weapons/IncisorOfMoonlight/Projectiles/StaccatoNoteProj.cs`  
**Texture:** PointBloom (2160px)

| ~Line | Scale Expression | Max Scale | Max Pixels | Status |
|-------|-----------------|-----------|------------|--------|
| ~192 | `0.25f * pulse` | ~0.275 | **594px** | 🔴 |

#### 2.11 LunarNova.cs
**File:** `Content/MoonlightSonata/Weapons/ResurrectionOfTheMoon/Projectiles/LunarNova.cs`  
**Texture:** SoftRadialBloom (2160px)

| ~Line | Scale Expression | Max Scale | Max Pixels | Status |
|-------|-----------------|-----------|------------|--------|
| ~107 | `pulseScale` | max 0.8 | **1,728px** | 🔴 |
| ~114 | `coreScale` | max 0.15 | **324px** | 🔴 |

#### 2.12 OrbitingNoteProj.cs
**File:** `Content/MoonlightSonata/Weapons/ResurrectionOfTheMoon/Projectiles/OrbitingNoteProj.cs`  
**Texture:** SoftRadialBloom (2160px)

| ~Line | Scale Expression | Max Scale | Max Pixels | Status |
|-------|-----------------|-----------|------------|--------|
| ~223 | `0.35f` | 0.35 | **756px** | 🔴 |

#### 2.13 LunarBeamProj.cs
**File:** `Content/MoonlightSonata/Weapons/ResurrectionOfTheMoon/Projectiles/LunarBeamProj.cs`  
**Texture:** VFX SoftGlow (1024px) + PointBloom (2160px)

| ~Line | Texture | Scale Expression | Max Pixels | Status |
|-------|---------|-----------------|------------|--------|
| ~216 | SoftGlow 1024 | `0.3f` | **307px** | 🔴 |
| ~219 | PointBloom 2160 | `0.15f` | **324px** | 🔴 |

#### 2.14 ResurrectionProjectile.cs
**File:** `Content/MoonlightSonata/Weapons/ResurrectionOfTheMoon/Projectiles/ResurrectionProjectile.cs`  
**Texture:** SoftRadialBloom (2160px)

| ~Line | Scale Expression | Max Scale | Max Pixels | Status |
|-------|-----------------|-----------|------------|--------|
| ~459 | `outerScale` | max 0.26 | **562px** | 🔴 |

### Summer / Seasons Themes

#### 2.15 SolarOrbProjectile.cs
**File:** `Content/Summer/Weapons/SolarOrb/Projectiles/SolarOrbProjectile.cs`  
**Texture:** VFX SoftGlow (1024px) + PointBloom (2160px)

| ~Line | Texture | Scale Expression | Max Pixels | Status |
|-------|---------|-----------------|------------|--------|
| ~707 | SoftGlow 1024 | `0.4f * pulse(1.15)` | **471px** | 🔴 |
| ~710 | PointBloom 2160 | `0.2f * pulse(1.15)` | **497px** | 🔴 |

#### 2.16 VivaldiSeasonalWave.cs
**File:** `Content/Seasons/Weapons/VivaldisFourSeasons/Projectiles/VivaldiSeasonalWave.cs`  
**Texture:** GetSoftGlow (512px)

| ~Line | Scale Expression | Max Scale | Max Pixels | Status |
|-------|-----------------|-----------|------------|--------|
| ~346 | `0.9f * pulse(1.18)` | ~1.062 | **544px** | 🔴 |
| ~409 | `0.7f * pulse(1.18)` | ~0.826 | **423px** | 🔴 |

### SwanLake Theme

#### 2.17 BlackSwanSwingProj.cs
**File:** `Content/SwanLake/ResonantWeapons/TheBlackSwan/Projectiles/BlackSwanSwingProj.cs`  
**Texture:** PointBloom (2160px) + GetSoftGlow (512px)

| ~Line | Texture | Scale Expression | Max Pixels | Status |
|-------|---------|-----------------|------------|--------|
| ~1074 | PointBloom 2160 | `0.18f * Projectile.scale` | **389px** | 🔴 |
| ~1122 | SoftGlow 512 | `0.7f * Projectile.scale` (empowered) | **358px** | 🔴 |
| ~1128 | SoftGlow 512 | `0.9f * Projectile.scale` (empowered) | **461px** | 🔴 |

#### 2.18 BlackSwanFlareProj.cs
**File:** `Content/SwanLake/ResonantWeapons/TheBlackSwan/Projectiles/BlackSwanFlareProj.cs`  
**Texture:** SoftRadialBloom/BloomCircle (variable) + GetSoftGlow (512px)

| ~Line | Texture | Scale Expression | Max Pixels | Status |
|-------|---------|-----------------|------------|--------|
| ~396 | SoftRadialBloom 2160 | `baseScale * 0.28f` (empowered base=0.5) | **302px** | 🔴 |
| ~420 | SoftGlow 512 | `0.5f * 1.4f` (empowered) | **358px** | 🔴 |
| ~437 | BloomCircle/SoftRadialBloom | `baseScale * 4f` | **400px** (BC) / **4,320px** (SRB) | 🔴🔴🔴 |
| ~445 | BloomCircle/SoftRadialBloom | `baseScale * 3.2f` | **320px** (BC) / **3,456px** (SRB) | 🔴🔴 |

#### 2.19 ChromaticBoltProj.cs
**File:** `Content/SwanLake/ResonantWeapons/ChromaticSwanSong/Projectiles/ChromaticBoltProj.cs`  
**Texture:** SoftRadialBloom (2160px), PointBloom (2160px)

| ~Line | Texture | Scale Expression | Max Pixels | Status |
|-------|---------|-----------------|------------|--------|
| ~419 | SoftRadialBloom 2160 | `0.5f * empScale(max 1.4)` | **1,512px** | 🔴 |
| ~419 | PointBloom 2160 | `0.1f * 1.4f` | **302px** | 🔴 |

#### 2.20 WingspanBoltProj.cs
**File:** `Content/SwanLake/ResonantWeapons/IridescentWingspan/Projectiles/WingspanBoltProj.cs`  
**Texture:** GetSoftGlow (512px), GetPointBloom (2160px)

| ~Line | Texture | Scale Expression | Max Pixels | Status |
|-------|---------|-----------------|------------|--------|
| ~338 | SoftGlow 512 | `0.4f * 1.5f * pulse` | **307px** | 🔴 |
| ~357 | PointBloom 2160 | `0.16f * 1.5f * pulse` | **518px** | 🔴 |

#### 2.21 LamentBulletProj.cs
**File:** `Content/SwanLake/ResonantWeapons/TheSwansLament/Projectiles/LamentBulletProj.cs`  
**Texture:** GetPointBloom (2160px)

| ~Line | Scale Expression | Max Scale | Max Pixels | Status |
|-------|-----------------|-----------|------------|--------|
| ~310 | `0.16f * empScale(1.3) * pulse` | ~0.208 | **449px** | 🔴 |

#### 2.22 IridescentCrystalProj.cs
**File:** `Content/SwanLake/ResonantWeapons/FeatheroftheIridescentFlock/Projectiles/IridescentCrystalProj.cs`  
**Texture:** GetRadialBloom (2160px), GetPointBloom (2160px)

| ~Line | Texture | Scale Expression | Max Pixels | Status |
|-------|---------|-----------------|------------|--------|
| ~499 | RadialBloom 2160 | `radialScale` (max 0.512) | **1,106px** | 🔴 |
| ~555 | PointBloom 2160 | `0.14f * pulse` | **302px** | 🔴 |

#### 2.23 PearlescentRocketProj.cs
**File:** `Content/SwanLake/ResonantWeapons/CallofthePearlescentLake/Projectiles/PearlescentRocketProj.cs`  
**Texture:** GetRadialBloom (2160px), GetPointBloom (2160px)

| ~Line | Texture | Scale Expression | Max Pixels | Status |
|-------|---------|-----------------|------------|--------|
| ~329 | RadialBloom 2160 | `0.8f * variantScale(1.15) * pulse` | **1,987px** | 🔴 |
| ~339 | PointBloom 2160 | `0.22f * 1.15f * pulse` | **547px** | 🔴 |

#### 2.24 SplashZoneProj.cs
**File:** `Content/SwanLake/ResonantWeapons/CallofthePearlescentLake/Projectiles/SplashZoneProj.cs`  
**Texture:** GetPointBloom (2160px)

| ~Line | Scale Expression | Max Scale | Max Pixels | Status |
|-------|-----------------|-----------|------------|--------|
| ~152 | `0.14f * shimmer` | ~0.14 | **302px** | 🔴 (barely) |

### OdeToJoy Theme

#### 2.25 ThornboundSwingProj.cs
**File:** `Content/OdeToJoy/Weapons/Thornbound/Projectiles/ThornboundSwingProj.cs`  
**Texture:** SoftGlow (1024px per-weapon texture)

| ~Line | Scale Expression | Max Scale | Max Pixels | Status |
|-------|-----------------|-----------|------------|--------|
| ~342 | `0.22f * widthScale(max 1.4)` | 0.308 | **315px** | 🔴 |
| ~353 | `0.35f * 1.4f` (empowered) | 0.49 | **502px** | 🔴 |

#### 2.26 BotanicalBurstProjectile.cs
**File:** `Content/OdeToJoy/Weapons/GardenersFury/Projectiles/BotanicalBurstProjectile.cs`  
**Texture:** SoftGlow (1024px per-weapon texture)

| ~Line | Scale Expression | Max Scale | Max Pixels | Status |
|-------|-----------------|-----------|------------|--------|
| ~220 | `expandRadius(128) * 3 / 1024` | 0.375 | **384px** | 🔴 |

#### 2.27 ThornWallProjectile.cs
**File:** `Content/OdeToJoy/Weapons/Thornbound/Projectiles/ThornWallProjectile.cs`  
**Texture:** SoftGlow (1024px per-weapon texture)

| ~Line | Scale Expression | Max Scale | Max Pixels | Status |
|-------|-----------------|-----------|------------|--------|
| ~164 | `currentRadius(max 160 emp) * 2.5 / 1024` | 0.39 | **400px** | 🔴 (empowered only) |

#### 2.28 ChorusProjectiles.cs — ChorusSpiritProjectile
**File:** `Content/OdeToJoy/Weapons/ChorusOfNature/Projectiles/ChorusProjectiles.cs`  
**Texture:** SoftGlow (1024px per-weapon texture)

| ~Line | Scale Expression | Max Scale | Max Pixels | Status |
|-------|-----------------|-----------|------------|--------|
| ~171 | `0.45f` (shader layer) | 0.45 | **461px** | 🔴 |
| ~180 | `0.4f` (additive layer) | 0.4 | **410px** | 🔴 |

#### 2.29 ChorusProjectiles.cs — HarmonicBlastProjectile
**File:** `Content/OdeToJoy/Weapons/ChorusOfNature/Projectiles/ChorusProjectiles.cs`  
**Texture:** SoftGlow (1024px per-weapon texture)

| ~Line | Scale Expression | Max Scale | Max Pixels | Status |
|-------|-----------------|-----------|------------|--------|
| ~286 | `scale(0.3) * 1.5f` | 0.45 | **461px** | 🔴 |
| ~291 | `0.3f` (ensemble) | 0.3 | **307px** | 🔴 |

#### 2.30 OvationProjectiles.cs — OvationSpiritProjectile
**File:** `Content/OdeToJoy/Weapons/NaturesOvation/Projectiles/OvationProjectiles.cs`  
**Texture:** SoftGlow (1024px per-weapon texture)

| ~Line | Scale Expression | Max Scale | Max Pixels | Status |
|-------|-----------------|-----------|------------|--------|
| ~207 | `0.4f + meter * 0.15f` | max 0.55 | **563px** | 🔴 |
| ~217 | `0.35f + meter * 0.1f` | max 0.45 | **461px** | 🔴 |
| ~222 | `0.5f * meter` | max 0.5 | **512px** | 🔴 |

#### 2.31 PollinatorProjectiles.cs — PollenSeedProjectile
**File:** `Content/OdeToJoy/Weapons/ThePollinator/Projectiles/PollinatorProjectiles.cs`  
**Texture:** SoftGlow (1024px per-weapon texture)

| ~Line | Scale Expression | Max Scale | Max Pixels | Status |
|-------|-----------------|-----------|------------|--------|
| ~93 | `0.4f` (shader layer) | 0.4 | **410px** | 🔴 |
| ~99 | `0.45f` (additive) | 0.45 | **461px** | 🔴 |

#### 2.32 PollinatorProjectiles.cs — MassBloomProjectile
**File:** `Content/OdeToJoy/Weapons/ThePollinator/Projectiles/PollinatorProjectiles.cs`  
**Texture:** SoftGlow (1024px per-weapon texture)

| ~Line | Scale Expression | Max Scale | Max Pixels | Status |
|-------|-----------------|-----------|------------|--------|
| ~209 | `expand * 0.6f` (expand max 1.6) | 0.96 | **983px** | 🔴 |

### Nachtmusik Theme

#### 2.33 TwilightSeveranceSwing.cs
**File:** `Content/Nachtmusik/Weapons/TwilightSeverance/Projectiles/TwilightSeveranceSwing.cs`  
**Texture:** GetSoftGlow (512px) + GetRadialBloom (2160px)  
bloomScale = 0.3f + ComboStep * 0.06f → max ~0.54 (ComboStep=4)

| Line | Texture | Scale Expression | Max Pixels | Status |
|------|---------|-----------------|------------|--------|
| ~360 | SoftGlow 512 | `bloomScale * 2.2f * pulse` | 512 × 0.54 × 2.2 × 1.1 = **664px** | 🔴 |
| ~364 | SoftGlow 512 | `bloomScale * 1.4f * pulse` | 512 × 0.54 × 1.4 × 1.1 = **423px** | 🔴 |
| ~380 | RadialBloom 2160 | `bloomScale * 0.5f * pulse` | 2160 × 0.54 × 0.5 × 1.1 = **642px** | 🔴 |
| ~382 | RadialBloom 2160 | `bloomScale * 0.3f` | 2160 × 0.54 × 0.3 = **350px** | 🔴 |

#### 2.34 SerenadeStarProjectile.cs
**File:** `Content/Nachtmusik/Weapons/SerenadeOfDistantStars/Projectiles/SerenadeStarProjectile.cs`  
**Texture:** GetRadialBloom (2160px)

| Line | Scale Expression | Max Scale | Max Pixels | Status |
|------|-----------------|-----------|------------|--------|
| 323 | `0.25f * pulse` | ~0.275 | **594px** | 🔴 |

#### 2.35 CrescendoWaveProjectile.cs
**File:** `Content/Nachtmusik/Weapons/MidnightsCrescendo/Projectiles/CrescendoWaveProjectile.cs`  
**Texture:** GetBloom (2160px)  
bloomScale = 0.3f + StackIntensity * 0.25f + LifetimeProgress * 0.1f → max ~0.65

| Line | Scale Expression | Max Pixels | Status |
|------|-----------------|------------|--------|
| 254 | `bloomScale * 2f` | 2160 × 0.65 × 2 = **2,808px** | 🔴🔴 |
| 256 | `bloomScale * 1.2f` | 2160 × 0.65 × 1.2 = **1,685px** | 🔴 |
| 258 | `bloomScale * 0.5f` | 2160 × 0.65 × 0.5 = **702px** | 🔴 |

#### 2.36 CosmicRequiemOrbProjectile.cs
**File:** `Content/Nachtmusik/Weapons/RequiemOfTheCosmos/Projectiles/CosmicRequiemOrbProjectile.cs`  
**Texture:** GetSoftGlow (512px) + GetRadialBloom (2160px)  
modeScale = 1.0 (standard), 1.4 (gravity well), 2.5 (event horizon)

| ~Line | Texture | Scale Expression | Max Pixels | Status |
|-------|---------|-----------------|------------|--------|
| ~200 | SoftGlow 512 | `0.3f * 2.5 * pulse` | 512 × 0.86 = **442px** | 🔴 |
| ~244 | GetBloom? 2160 | `modeScale * 0.8f` | 2160 × 2.0 = **4,320px** | 🔴🔴🔴 |
| ~253 | RadialBloom 2160 | `modeScale * 0.5f * pulse` | 2160 × 1.375 = **2,970px** | 🔴🔴 |

### Eroica Theme

#### 2.37 SakurasBlossomSwing.cs
**File:** `Content/Eroica/Weapons/SakurasBlossom/SakurasBlossomSwing.cs`  
**Texture:** GetBloom (2160px)

| Line | Scale Expression | Max Scale | Max Pixels | Status |
|------|-----------------|-----------|------------|--------|
| 877 | `0.7f` (fallback) | 0.7 | **1,512px** | 🔴 |
| 880 | `0.4f` (fallback) | 0.4 | **864px** | 🔴 |
| 915 | `1.2f * progress` (shader burst) | 1.2 | **2,592px** | 🔴🔴 |
| 919 | `1.8f * progress` (shader burst) | 1.8 | **3,888px** | 🔴🔴🔴 |
| 933 | `1.2f * progress` (fallback burst) | 1.2 | **2,592px** | 🔴🔴 |

---

## SECTION 3: VERIFIED FIXED FILES

| File | Previous Max (from audit) | Current Max | Status |
|------|--------------------------|-------------|--------|
| `CodaHeldSwing.cs` | 3,888px | **276px** | ✅ FIXED |
| `CrescentMoonProj.cs` | 1,296px (at 0.6f) | **130px** (0.06f) | ✅ FIXED |
| `CodaZenithSword.cs` | 3,110px | **348px** (barely over) | ⚠ Nearly fixed |

---

## SECTION 4: UNVERIFIED — FROM EXISTING AUDIT (May Be Fixed)

The following were listed as oversized in `Documentation/Audits/Bloom_Scale_Audit.md` but have NOT been verified for their current state. Some may have been fixed (like CodaHeldSwing was). **Each needs verification.**

### Fate Theme

| File | Audit Max (px) | Notes |
|------|---------------|-------|
| `OpusSwingProjectile.cs` | 3,456 | PointBloom 2160 × 1.6f |
| `RequiemSwingProjectile.cs` | 3,456 | PointBloom 2160 × 1.6f |
| `ConductorSwingProjectile.cs` | 3,456 | PointBloom 2160 × 1.6f |
| `FractalSwingProjectile.cs` | 2,808 | PointBloom 2160 × 1.3f |
| `ResonanceRapidBullet.cs` | 3,024 | PointBloom 2160 × 1.4f |
| `RequiemRealityTear.cs` | 2,458 | SoftRadialBloom 2160 × ... |
| `OpusUltimaItem.cs` | 768 | GetBloom 2160 × 0.356 |

### LaCampanella Theme

| File | Audit Max (px) | Notes |
|------|---------------|-------|
| `DualFatedChimeSwingProj.cs` | 2,048 | SoftCircle 2160 × ... |
| `IgnitionThrustProj.cs` | 1,024 | Per-weapon glow × ... |
| `EmpoweredLightningProj.cs` | 1,024 | Per-weapon glow × ... |
| `InfernalGeyserProj.cs` | 1,536 | Per-weapon glow × ... |
| `CrawlerOfTheBell.cs` | 864 | SoftCircle 2160 × 0.4f |
| `CampanellaChoirMinion.cs` | 819 | Various bloom draws |
| `InfiniteBellOrbProj.cs` | 614 | GetBloom × ... |

### Eroica Theme

| File | Audit Max (px) | Notes |
|------|---------------|-------|
| `EroicasBeam.cs` | 410 | SoftGlow 1024 drawers |

### MoonlightSonata Theme (Additional)

| File | Audit Max (px) | Notes |
|------|---------------|-------|
| `EternalMoonSwing.cs` | 2,722 | Crescent bloom section (different from EternalMoonWave) |
| `GoliathOfMoonlight.cs` | 2,160 | Conductor mode bloom (not the DrawRiftGlow which is OK) |
| `GoliathMaskAura.cs` | 717 | GetBloom draws |
| `GoliathRipple.cs` | 717+ | GetBloom draws |
| `WaningDeer.cs` (enemy) | 922 | GetBloom draws |

---

## SECTION 5: UNCHECKED AREAS

The following areas have **not been analyzed** and may contain additional oversized bloom draws:

### Eroica VFX Helpers (GetBloom/GetPointBloom callers)
- `Content/Eroica/Projectiles/BlossomVFXHelpers.cs` (L293, L344)
- `Content/Eroica/Projectiles/FractalVFXHelpers.cs` (L86, L139, L174, L234, L268)
- `Content/Eroica/Projectiles/PiercingVFXHelpers.cs` (L186, L314)
- `Content/Eroica/Projectiles/FuneralVFXHelpers.cs` (L255)
- `Content/Eroica/Projectiles/TriumphantFractalProjectile.cs` (L532)
- `Content/Eroica/Minions/FinalityVFXHelpers.cs` (L89, L143, L273)
- `Content/Eroica/Weapons/CelestialValor/CelestialValorSwing.cs` (L814)

### Eroica Accessories
- `Content/Eroica/Accessories/SakurasBurningWill/HeroicSpiritMinion.cs` (L462 — GetRadialBloom)
- `Content/Eroica/Accessories/Shared/EroicaAccessoryPlayer.cs` (L556, L578 — GetPointBloom)
- `Content/Eroica/Accessories/SymphonyOfScarletFlames/PetalExplosion.cs` (L249)
- `Content/Eroica/Accessories/PyreOfTheFallenHero/PyreSlashWave.cs` (L220)

### Nachtmusik Weapons (Remaining)
- `Content/Nachtmusik/Weapons/NocturnalExecutioner/Projectiles/NocturnalExecutionerSwing.cs` (L424 — GetRadialBloom)
- `Content/Nachtmusik/Weapons/MidnightsCrescendo/Projectiles/MidnightsCrescendoSwing.cs` (L517 — GetRadialBloom)
- `Content/Nachtmusik/Weapons/ConstellationPiercer/Projectiles/ConstellationBoltProjectile.cs` (L274 — GetRadialBloom)
- `Content/Nachtmusik/Weapons/ConductorOfConstellations/Projectiles/StellarConductorMinion.cs` (L171 — GetRadialBloom)

### DiesIrae Weapons
- `Content/DiesIrae/Weapons/WrathsCleaver/Utilities/WrathsCleaverUtils.cs` (L121)
- `Content/DiesIrae/Weapons/SinCollector/Utilities/SinCollectorUtils.cs` (L96)
- `Content/DiesIrae/Weapons/ExecutionersVerdict/Utilities/ExecutionersVerdictUtils.cs` (L111)
- `Content/DiesIrae/Weapons/DamnationsCannon/Utilities/DamnationsCannonUtils.cs` (L48)
- `Content/DiesIrae/Weapons/ChainOfJudgment/Utilities/ChainOfJudgmentUtils.cs` (L87)
- `Content/DiesIrae/Weapons/DeathTollingBell/Particles/DeathTollingBellParticleTypes.cs`
- `Content/DiesIrae/Weapons/WrathfulContract/Particles/WrathfulContractParticleTypes.cs`
- `Content/DiesIrae/Weapons/HarmonyOfJudgement/Particles/HarmonyParticleTypes.cs`

### Fate Particles
- `Content/Fate/ResonantWeapons/SymphonysEnd/Particles/SymphonyParticleHandler.cs` (L52)
- `Content/Fate/ResonantWeapons/TheFinalFermata/Particles/FermataParticleHandler.cs` (L82)
- `Content/Fate/ResonantWeapons/DestinysCrescendo/Particles/CrescendoParticleHandler.cs` (L49)

### EnigmaVariations
- `Content/EnigmaVariations/EnigmaVFXLibrary.cs` (L95 — same pattern as other VFXLibraries)
- `Content/EnigmaVariations/ResonantWeapons/VariationsOfTheVoid/VariationsOfTheVoid.cs` (L451)
- `Content/EnigmaVariations/ResonantWeapons/TheUnresolvedCadence/TheUnresolvedCadence.cs` (L267)

### SwanLake Boss
- `Content/SwanLake/Bosses/SwanLakeTheMonochromaticFractal.cs` (L3319 — GetPointBloom)

### SwanLake/Chromatic/Pearlescent Particles
- `Content/SwanLake/ResonantWeapons/ChromaticSwanSong/Particles/ChromaticParticleTypes.cs` (L28, L66)
- `Content/SwanLake/ResonantWeapons/IridescentWingspan/Particles/WingspanParticleTypes.cs` (L68, L100)
- `Content/SwanLake/ResonantWeapons/CallofthePearlescentLake/Particles/PearlescentParticleTypes.cs` (L120)

### Common Systems
- `Common/Systems/VFX/Bloom/BloomParticles.cs` — 5 particle classes all using GetBloom(2160)
- `Common/Systems/VFX/Trails/EnhancedTrailRenderer.cs` (L1063)
- `Common/Systems/VFX/Trails/LayeredNebulaFog.cs` (L246)
- `Common/Systems/Bosses/BossRenderHelper.cs` (L270)

### OdeToJoy (Remaining)
- `HymnProjectiles.cs` — bloom draws not fully checked
- `ThornSprayProjectiles.cs` — bloom draws not fully checked
- `PetalStormProjectiles.cs` — not checked
- `AnthemProjectiles.cs` — not checked
- `ElysianProjectiles.cs` — not checked
- `FountainProjectiles.cs` — not checked
- `RoseThornChainsawProjectile.cs` — not checked
- `GardenerFuryProjectile.cs` — GetPhaseScale values not fully verified

### Other Themes
- Spring, Autumn, Winter, Mars, Mercury, ClairDeLune content — not checked

---

## SECTION 6: TOP 20 WORST OFFENDERS (Ranked by Max Rendered Size)

| Rank | File | Max Pixels | Texture | Scale Expression |
|------|------|------------|---------|-----------------|
| 1 | **PrismaticDetonation.cs** | **22,464px** | SoftRadialBloom 2160 | `scale(8) * 1.3f` |
| 2 | **CrescentWaveProj.cs** | **8,856px** | SoftRadialBloom 2160 | `ringScale * 1.5f` |
| 3 | **LaCampanellaAccessoryVFX.cs** L323 | **5,184px** | GetBloom 2160 via VFXLib | `DrawBloom(1.2f)` → outer × 2.0 |
| 4 | **BlackSwanFlareProj.cs** L437 | **4,320px** | SoftRadialBloom fallback | `baseScale * 4f` |
| 5 | **CosmicRequiemOrbProj.cs** L244 | **4,320px** | GetBloom? 2160 | `modeScale(2.5) * 0.8` |
| 6 | **SakurasBlossomSwing.cs** L919 | **3,888px** | GetBloom 2160 | `1.8f * progress` |
| 7 | **SerenadeHoldout.cs** L596 | **3,240px** | SoftRadialBloom 2160 | `(1.5+res*0.2)*int` |
| 8 | **LaCampanellaAccessoryVFX.cs** L829 | **3,024px** | GetBloom 2160 via VFXLib | `DrawBloom(0.7f)` → outer × 2.0 |
| 9 | **CosmicRequiemOrbProj.cs** L253 | **2,970px** | RadialBloom 2160 | `modeScale(2.5)*0.5*pulse` |
| 10 | **CrescendoWaveProjectile.cs** L254 | **2,808px** | GetBloom 2160 | `bloomScale(0.65)*2f` |
| 11 | **RequiemOfRealityVFX.cs** L251 | **2,592px** | GetBloom 2160 via VFXLib | `DrawBloom(0.6f)` → outer × 2.0 |
| 12 | **SakurasBlossomSwing.cs** L915 | **2,592px** | GetBloom 2160 | `1.2f * progress` |
| 13 | **PearlescentRocketProj.cs** L329 | **1,987px** | RadialBloom 2160 | `0.8*1.15*pulse` |
| 14 | **LunarNova.cs** L107 | **1,728px** | SoftRadialBloom 2160 | `pulseScale(0.8)` |
| 15 | **SerenadeHoldout.cs** L600 | **1,728px** | SoftRadialBloom 2160 | `(0.8+res*0.1)*int` |
| 16 | **SpectralChildBeam.cs** L181 | **1,663px** | PointBloom 2160 | `0.7f * pulse` |
| 17 | **GoliathDevastatingBeam.cs** L355 | **1,620px** | SoftRadialBloom 2160 | max 0.75f |
| 18 | **SakurasBlossomSwing.cs** L793 | **1,598px** | GetBloom 2160 via VFXLib | `DrawEroicaBloomStack(0.37)` outer |
| 19 | **SakurasBlossomSwing.cs** L877 | **1,512px** | GetBloom 2160 | `0.7f` (fallback) |
| 20 | **ChromaticBoltProj.cs** L419 | **1,512px** | SoftRadialBloom 2160 | `0.5f * empScale(1.4)` |

---

## SECTION 7: RECOMMENDED FIX — Systemic VFXLibrary Clamp

The single highest-impact fix is adding a max scale clamp inside all VFXLibrary/BloomRenderer bloom stack methods. This would protect against ALL callers regardless of what scale they pass.

**Maximum safe scale per layer (for 300px output with 2160px texture):**

| Layer | Multiplier | Max Input Scale |
|-------|-----------|----------------|
| Outer | × 2.0 | 0.069 |
| Mid | × 1.4 | 0.099 |
| Inner | × 0.9 | 0.154 |
| Core | × 0.4 | 0.347 |

**Recommended: Clamp input scale to 0.069 inside the bloom stack methods**, or reduce the layer multipliers so that the outer layer uses `scale * 0.28f` instead of `scale * 2.0f` (keeping the relative ratios but reducing absolute output).

The alternative is fixing every individual caller, which is more work but preserves the ability to pass larger scales intentionally. The systemic fix protects against future regressions.
