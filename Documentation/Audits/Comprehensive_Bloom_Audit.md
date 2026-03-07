# Comprehensive Bloom Draw Call Audit

> **Read-only audit** — every bloom `.Draw()` call across all 20 audited files.
> For each draw: exact texture, scale expression, texture size (px), and rendered pixel size.

---

## Texture Size Reference

| Texture Reference | Actual Asset Path | Size (px) |
|---|---|---|
| `MagnumTextureRegistry.GetBloom()` / `GetPointBloom()` | `Assets/VFX Asset Library/GlowAndBloom/PointBloom` | **2160** |
| `MagnumTextureRegistry.GetRadialBloom()` | `Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom` | **2160** |
| `MagnumTextureRegistry.GetSoftGlow()` | `Assets/SandboxLastPrism/Orbs/SoftGlow` | **512** |
| Direct load `VFX Asset Library/GlowAndBloom/SoftGlow` | (different file!) | **1024** |
| `SPFTextures.SoftGlow` | `Assets/VFX Asset Library/GlowAndBloom/SoftGlow` | **1024** |
| `_softRadialBloom` (Fate weapons) | `Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom` | **2160** |
| `_bloomCircle` (Fate weapons) | `Assets/VFX Asset Library/GlowAndBloom/PointBloom` | **2160** |
| `_starFlare` / `StarFlare` | `Assets/VFX Asset Library/GlowAndBloom/StarFlare` | **1024** |
| `GlowOrb` | `Assets/VFX Asset Library/GlowAndBloom/GlowOrb` | **1024** |
| `MS Star Flare` (theme specific) | `Assets/VFX Asset Library/Theme Specific/Moonlight Sonata/Glow and Bloom/MS Star Flare` | **1024** |
| `MS Glow Orb` (theme specific) | `Assets/VFX Asset Library/Theme Specific/Moonlight Sonata/Glow and Bloom/MS Glow Orb` | **1024** |
| `SoftCircle` (masks) | `Assets/VFX Asset Library/MasksAndShapes/SoftCircle` | **2160** |
| `BloomCircle` (particle) | `Assets/Textures/Particles/Particle_BloomCircle` | **200** |
| `4PointedStarSoft` | `Assets/Particles Asset Library/Stars/4PointedStarSoft` | **32** |
| `4PointedStarHard` (SparkleHard) | `Assets/Particles Asset Library/Stars/4PointedStarHard` | **~32** |
| `ThinTall4PointedStar` (SparkleThin) | `Assets/Particles Asset Library/Stars/ThinTall4PointedStar` | **~32×128** |
| `_glowTex` (Fate swing weapons) | `Assets/SandboxLastPrism/Orbs/SoftGlow` | **512** |
| `GoliathTextures.SoftRadialBloom` | `Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom` | **2160** |
| `CometTextures.SoftRadialBloom` | `Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom` | **2160** |

### ⚠ TWO-FILE GOTCHA

- `MagnumTextureRegistry.SoftGlow` → `SandboxLastPrism/Orbs/SoftGlow` = **512px**
- `VFX Asset Library/GlowAndBloom/SoftGlow` (direct load / SPFTextures) = **1024px**

These are DIFFERENT files with DIFFERENT sizes referenced by the same name.

---

## File-by-File Audit

### 1. SakurasBlossomSwing.cs
**Path:** `Content/Eroica/Weapons/SakurasBlossom/SakurasBlossomSwing.cs`
**Texture:** `MagnumTextureRegistry.GetBloom()` → **PointBloom 2160px**

| Line(s) | Context | Scale Expression | Typical Scale | Rendered px |
|---|---|---|---|---|
| ~651-668 | Trail bloom points (loop) | `MathHelper.Lerp(0.08f, 0.22f, progress) * widthScale` | 0.08–0.22 | **173–475** |
| ~761 | Blade tip bloom layer 1 | `0.3f + phase * 0.05f` | 0.30–0.40 | **648–864** |
| ~764 | Blade tip bloom layer 2 | `0.45f + phase * 0.07f` | 0.45–0.59 | **972–1274** |
| ~808 | Meditation inner dissolve | `0.5f` | 0.50 | **1080** |
| ~813 | Meditation outer haze | `0.9f` | 0.90 | **1944** |

---

### 2. CodaHeldSwing.cs
**Path:** `Content/Fate/ResonantWeapons/CodaOfAnnihilation/Projectiles/CodaHeldSwing.cs`

**Impact Flash (Lines 246-255):**

| Line | Texture | Scale | Tex Size | Rendered px |
|---|---|---|---|---|
| 246 | SoftRadialBloom | `1.8f` | 2160 | **3888** ⚠ |
| 248 | SoftRadialBloom | `1.2f` | 2160 | **2592** ⚠ |
| 250 | SoftRadialBloom | `0.7f` | 2160 | **1512** |
| 253 | PointBloom | `0.35f` | 2160 | **756** |

**CrescentBloom (Lines 474-492) — 7 draws, `i` = intensity ~0.68–0.85:**

| Line | Texture | Scale Expression | @ i=0.85 | Rendered px |
|---|---|---|---|---|
| 474 | SoftRadialBloom | `1.6f * i` | 1.36 | **2938** ⚠ |
| 477 | SoftRadialBloom | `1.1f * i` | 0.935 | **2020** |
| 480 | SoftRadialBloom | `0.65f * i` | 0.553 | **1194** |
| 483 | PointBloom | `0.35f * i` | 0.298 | **643** |
| 486 | PointBloom | `0.18f * i` | 0.153 | **330** |
| 489 | StarFlare | `0.4f * i` | 0.34 | **348** |
| 492 | StarFlare | `0.25f * i` | 0.213 | **218** |

---

### 3. OpusSwingProjectile.cs
**Path:** `Content/Fate/ResonantWeapons/OpusUltima/Projectiles/OpusSwingProjectile.cs`

**CrescentBloom (Lines 571-593) — identical pattern to Coda:**

| Line | Texture | Scale Expression | @ i=0.85 | Rendered px |
|---|---|---|---|---|
| 571 | SoftRadialBloom | `1.6f * i` | 1.36 | **2938** ⚠ |
| 574 | SoftRadialBloom | `1.1f * i` | 0.935 | **2020** |
| 577 | SoftRadialBloom | `0.65f * i` | 0.553 | **1194** |
| 580 | PointBloom | `0.35f * i` | 0.298 | **643** |
| 583 | PointBloom | `0.18f * i` | 0.153 | **330** |
| 586 | StarFlare | `0.4f * i` | 0.34 | **348** |
| 589 | StarFlare | `0.25f * i` | 0.213 | **218** |

**Tip Lens Flare (Lines 814-820):**
**Texture:** `_glowTex` = SLP/Orbs/SoftGlow = **512px**

| Line | Scale Expression | Typical Scale | Rendered px |
|---|---|---|---|
| 814 | `0.35f * flarePulse` | ~0.28–0.35 | **143–179** |
| 817 | `0.5f * flarePulse * (0.5f + combo*0.5f)` | ~0.2–0.5 | **102–256** |
| 820 | `0.18f * flarePulse` | ~0.14–0.18 | **72–92** |

**Combo Aura (Lines 865-880):**
**Texture:** `_glowTex` = **512px**

| Line | Scale Expression | Typical Scale | Rendered px |
|---|---|---|---|
| 865 | `0.12f * pulse` | ~0.10–0.12 | **51–61** |
| 868 | `0.18f * pulse` | ~0.15–0.18 | **77–92** |
| 872 | `0.4f + combo * 0.4f` | 0.4–0.8 | **205–410** |
| 877 | `coreScale * 1.5f` | ~0.6–1.2 | **307–614** |

---

### 4. RequiemSwingProjectile.cs
**Path:** `Content/Fate/ResonantWeapons/RequiemOfReality/Projectiles/RequiemSwingProjectile.cs`

**CrescentBloom (Lines 533-560) — identical structure:**

| Line | Texture | Scale Expression | @ i=0.85 | Rendered px |
|---|---|---|---|---|
| 533 | SoftRadialBloom | `1.6f * i` | 1.36 | **2938** ⚠ |
| 536 | SoftRadialBloom | `1.1f * i` | 0.935 | **2020** |
| 539 | SoftRadialBloom | `0.65f * i` | 0.553 | **1194** |
| 542 | PointBloom | `0.35f * i` | 0.298 | **643** |
| 545 | PointBloom | `0.18f * i` | 0.153 | **330** |
| 548 | StarFlare | `0.4f * i` | 0.34 | **348** |
| 551 | StarFlare | `0.25f * i` | 0.213 | **218** |

**Tip Lens Flare (Lines 805-807):**
**Texture:** `_glowTex` = SLP/Orbs/SoftGlow = **512px**

| Line | Scale Expression | Typical Scale | Rendered px |
|---|---|---|---|
| 805 | `0.35f * fp` | ~0.28–0.35 | **143–179** |
| 807 | `0.18f * fp` | ~0.14–0.18 | **72–92** |

**Combo Aura (Lines 853-859):**
**Texture:** `_glowTex` = **512px**

| Line | Scale Expression | Typical Scale | Rendered px |
|---|---|---|---|
| 853-857 | `0.5f + s * 0.15f` (per splotch, s=0..4) | 0.5–1.1 | **256–563** |
| 859 | `0.7f` (center haze) | 0.70 | **358** |

---

### 5. ConductorSwingProjectile.cs
**Path:** `Content/Fate/ResonantWeapons/TheConductorsLastConstellation/Projectiles/ConductorSwingProjectile.cs`

**CrescentBloom (Lines 632-654) — identical structure:**

| Line | Texture | Scale Expression | @ i=0.85 | Rendered px |
|---|---|---|---|---|
| 632 | SoftRadialBloom | `1.6f * i` | 1.36 | **2938** ⚠ |
| 635 | SoftRadialBloom | `1.1f * i` | 0.935 | **2020** |
| 638 | SoftRadialBloom | `0.65f * i` | 0.553 | **1194** |
| 641 | PointBloom | `0.35f * i` | 0.298 | **643** |
| 644 | PointBloom | `0.18f * i` | 0.153 | **330** |
| 647 | StarFlare | `0.4f * i` | 0.34 | **348** |
| 650 | StarFlare | `0.25f * i` | 0.213 | **218** |

**Tip Conductor Flare (Lines 928-930):**
**Texture:** `_glowTex` = SLP/Orbs/SoftGlow = **512px**

| Line | Scale Expression | Typical Scale | Rendered px |
|---|---|---|---|
| 928 | `0.35f * fp` | ~0.28–0.35 | **143–179** |
| 930 | `0.18f * fp` | ~0.14–0.18 | **72–92** |

---

### 6. FractalSwingProjectile.cs
**Path:** `Content/Fate/ResonantWeapons/FractalOfTheStars/Projectiles/FractalSwingProjectile.cs`

**CrescentBloom (Lines 632-669) — 6 layers, `crescentScale (cS)` = `0.35f + 0.3f * combo` (0.35–0.65):**

| Line | Texture | Scale Expression | @ cS=0.65 | Rendered px |
|---|---|---|---|---|
| 635 | SoftRadialBloom | `cS * 2.0f` | 1.30 | **2808** ⚠ |
| 641 | SoftRadialBloom | `cS * 1.3f` | 0.845 | **1825** |
| 647 | SoftRadialBloom | `cS * 1.6f * orbPulse` | ~1.04 | **2246** ⚠ |
| 653 | PointBloom | `cS * 0.6f` | 0.39 | **842** |
| 657 | PointBloom | `cS * 0.25f` | 0.163 | **352** |
| 661 | StarFlare | `Vector2(cS*0.3f, cS*0.8f)` | (0.195, 0.52) | **(200, 533)** |
| 665 | StarFlare | `Vector2(cS*0.25f, cS*0.6f)` | (0.163, 0.39) | **(167, 400)** |

**Tip Star Flare (Lines 910-912):**
**Texture:** `_glowTex` = SLP/Orbs/SoftGlow = **512px**

| Line | Scale Expression | Typical Scale | Rendered px |
|---|---|---|---|
| 910 | `0.35f * fp` | ~0.28–0.35 | **143–179** |
| 912 | `0.18f * fp` | ~0.14–0.18 | **72–92** |

---

### 7. CodaZenithSword.cs
**Path:** `Content/Fate/ResonantWeapons/CodaOfAnnihilation/Projectiles/CodaZenithSword.cs`

| Line | Texture | Via | Scale Expression | Tex Size | Rendered px |
|---|---|---|---|---|---|
| 460 | SoftRadialBloom | `GetRadialBloom()` | `1.44f * pulse` | 2160 | **~3110** ⚠ |
| 470 | SoftGlow (SLP) | `GetSoftGlow()` | `0.96f` | 512 | **491** |
| 480 | PointBloom | `GetPointBloom()` | `0.56f` | 2160 | **1210** |
| 488 | SoftGlow (SLP) | `GetSoftGlow()` | `0.28f` | 512 | **143** |
| 500 | 4PointedStarSoft | `GetStar4Soft()` | `0.64f` | 32 | **20** |
| 501 | 4PointedStarSoft | `GetStar4Soft()` | `0.48f` | 32 | **15** |
| 511 | ThinTall4PointedStar | `GetStarThin()` | `0.44f` | ~32w | **14w** |

---

### 8. ResonanceRapidBullet.cs
**Path:** `Content/Fate/ResonantWeapons/ResonanceOfABygoneReality/Projectiles/ResonanceRapidBullet.cs`

**Impact Flash (Lines 188-197) — 6 draws:**

| Line | Texture | Scale | Tex Size | Rendered px |
|---|---|---|---|---|
| 188 | SoftRadialBloom | `0.8f` | 2160 | **1728** |
| 189 | SoftRadialBloom | `0.5f` | 2160 | **1080** |
| 191 | PointBloom | `0.4f` | 2160 | **864** |
| 193 | PointBloom | `0.22f` | 2160 | **475** |
| 195 | StarFlare | `0.3f` | 1024 | **307** |
| 197 | StarFlare | `0.2f` | 1024 | **205** |

**Blade Spawn Flash (Lines 284-296) — 8 draws:**

| Line | Texture | Scale | Tex Size | Rendered px |
|---|---|---|---|---|
| 284 | SoftRadialBloom | `1.4f` | 2160 | **3024** ⚠ |
| 286 | SoftRadialBloom | `0.9f` | 2160 | **1944** |
| 288 | SoftRadialBloom | `0.6f` | 2160 | **1296** |
| 290 | PointBloom | `0.45f` | 2160 | **972** |
| 292 | PointBloom | `0.25f` | 2160 | **540** |
| 294 | StarFlare | `0.55f` | 1024 | **563** |
| 295 | StarFlare | `0.4f` | 1024 | **410** |
| 296 | StarFlare | `0.65f` | 1024 | **666** |

**Trail Bloom (Line 404):**

| Line | Texture | Scale Expression | Range | Rendered px |
|---|---|---|---|---|
| 404 | SoftRadialBloom | `(0.35f - progress*0.2f) * pulse` | 0.15–0.35 | **324–756** |

**Body Bloom (Lines 443-449) — 7 draws:**

| Line | Texture | Scale Expression | Typical | Rendered px |
|---|---|---|---|---|
| 443 | SoftRadialBloom | `0.55f * pulse` | ~0.47–0.55 | **1015–1188** |
| 444 | SoftRadialBloom | `0.4f * pulse` | ~0.34–0.4 | **734–864** |
| 445 | SoftRadialBloom | `0.3f * pulse` | ~0.26–0.3 | **562–648** |
| 446 | PointBloom | `0.22f * pulse` | ~0.19–0.22 | **410–475** |
| 447 | PointBloom | `0.13f * pulse` | ~0.11–0.13 | **238–281** |
| 448 | StarFlare | `0.15f * pulse` | ~0.13–0.15 | **133–154** |
| 449 | StarFlare | `0.11f * pulse` | ~0.09–0.11 | **92–113** |

---

### 9. RequiemRealityTear.cs
**Path:** `Content/Fate/ResonantWeapons/RequiemOfReality/Projectiles/RequiemRealityTear.cs`
**Texture:** `_glowTex` = SLP/Orbs/SoftGlow = **512px** (for layers 1-3); `_supernovaTex` = custom FA Supernova Core (unknown size)

| Line | Texture | Scale Expression | Typical Scale | Rendered px |
|---|---|---|---|---|
| 169 | SoftGlow (SLP) | `1.8f * pulse` | ~1.53–1.8 | **783–922** |
| 172 | SoftGlow (SLP) | `2.4f` | 2.40 | **1229** |
| 207 | FA Supernova Core | `0.35f * superPulse` | ~0.30–0.35 | **unknown** |

---

### 10. BlackSwanFlareProj.cs
**Path:** `Content/SwanLake/ResonantWeapons/CalloftheBlackSwan/Projectiles/BlackSwanFlareProj.cs`

**Fallback Trail (Lines 360-361):**
**Texture:** `MagnumTextureRegistry.GetSoftGlow()` = SLP/Orbs/SoftGlow = **512px**

| Line | Scale Expression | Range | Rendered px |
|---|---|---|---|
| 360 | `MathHelper.Lerp(0.12f, 0.03f, t)` | 0.03–0.12 | **15–61** |
| 361 | `trailScale * 0.4f` | 0.012–0.048 | **6–25** |

**5-Layer Bloom Stack (Lines 396-449):**
`baseScale` = 0.35–0.50 (empowered up to ×1.3)

| Line | Texture | Scale Expression | @ base=0.5 | Rendered px |
|---|---|---|---|---|
| 396 | SoftRadialBloom or SoftGlow (SLP) | `baseScale * 2.8f` | 1.40 | **SRB: 3024** ⚠ or **SG: 717** |
| 406 | SoftGlow (SLP) | `baseScale * 1.4f` | 0.70 | **358** |
| 416 | SoftGlow (SLP) | `baseScale * 0.8f` | 0.40 | **205** |
| 426 | PointBloom | `baseScale * 0.5f` | 0.25 | **540** |
| 436 | Star4Soft (32px) | `baseScale * 0.35f` | 0.175 | **6** |

**Empowered Additional (Lines 440-449):**

| Line | Texture | Scale Expression | @ base=0.5 | Rendered px |
|---|---|---|---|---|
| 440 | GlowOrb / SoftRadialBloom / SoftGlow | `baseScale * 4f` | 2.0 | **SRB: 4320** ⚠ |
| 445 | Same orbTex | `baseScale * 3.2f` | 1.6 | **SRB: 3456** ⚠ |

---

### 11. DualFatedChimeSwingProj.cs
**Path:** `Content/LaCampanella/ResonantWeapons/DualFatedChime/Projectiles/DualFatedChimeSwingProj.cs`
**Texture:** Direct load `VFX Asset Library/GlowAndBloom/SoftGlow` = **1024px**
`phaseScale` = `1 + combo * 0.1f` (~1.0–1.5)

| Line | Scale Expression | @ combo=3, Proj.scale=1 | Rendered px |
|---|---|---|---|
| 764 | `1.4f * Projectile.scale * phaseScale` | 1.82 | **1864** |
| 768 | `0.7f * Projectile.scale * phaseScale` | 0.91 | **932** |
| 772 | `0.3f * Projectile.scale * phaseScale` | 0.39 | **400** |
| 778 | `2.0f * Projectile.scale * phaseScale` (phase≥3 only) | 2.60 | **2662** ⚠ |

---

### 12. InfernalGeyserProj.cs
**Path:** `Content/LaCampanella/ResonantWeapons/IgnitionOfTheBell/Projectiles/InfernalGeyserProj.cs`
**Texture:** Direct load `VFX Asset Library/GlowAndBloom/SoftGlow` = **1024px**

**Shader Pass Geyser Layers (Line 261) — per-layer in loop:**

| Line | Scale Expression | Typical Range | Rendered px |
|---|---|---|---|
| 261 | `widthScale = (1-t*0.4) * (IsSmall?0.6:1.0)` via shader | 0.36–1.0 | **369–1024** |

**Non-Shader Fallback Layers (Lines 284-300) — 4 draws per loop iteration:**

| Line | Context | Scale Expression | Full-size Range | Rendered px |
|---|---|---|---|---|
| 284 | Outer glow | `widthScale * 1.5f` | 0.54–1.5 | **553–1536** |
| 289 | Mid body | `widthScale` | 0.36–1.0 | **369–1024** |
| 294 | Core | `widthScale * 0.3f` | 0.108–0.3 | **111–307** |
| 300 | Flicker | `widthScale * 0.6f` | 0.216–0.6 | **221–614** |

**Base Bloom (Line 307):**

| Line | Scale | Rendered px |
|---|---|---|
| 307 | `IsSmall ? 0.8f : 1.5f` | **819 / 1536** |

**Tip Glow (Line 314):**

| Line | Scale Expression | Range | Rendered px |
|---|---|---|---|
| 314 | `(IsSmall ? 0.35f : 0.6f) * tipPulse` | 0.28–0.6 | **287–614** |

---

### 13. IgnitionThrustProj.cs
**Path:** `Content/LaCampanella/ResonantWeapons/IgnitionOfTheBell/Projectiles/IgnitionThrustProj.cs`
**Texture:** Direct load `VFX Asset Library/GlowAndBloom/SoftGlow` = **1024px**

| Line | Context | Scale | Rendered px |
|---|---|---|---|
| 502 | Crimson outer | `1.0f` | **1024** |
| 507 | Magma mid | `0.55f` | **563** |
| 512 | White-hot core | `0.2f` | **205** |

---

### 14. EmpoweredLightningProj.cs
**Path:** `Content/LaCampanella/ResonantWeapons/FangOfTheInfiniteBell/Projectiles/EmpoweredLightningProj.cs`
**Texture:** Direct load `VFX Asset Library/GlowAndBloom/SoftGlow` = **1024px**

| Line | Context | Scale Expression | Typical | Rendered px |
|---|---|---|---|---|
| 234 | Outer glow | `1f * fade` | 0.0–1.0 | **0–1024** |
| 236 | Inner core | `0.35f * fade` | 0.0–0.35 | **0–358** |

---

### 15. SerenadeBeam.cs
**Path:** `Content/MoonlightSonata/Weapons/MoonlightsCalling/Projectiles/SerenadeBeam.cs`
`bounceIntensity` = 0.7–1.0; `pulse` = 0.85–1.15

| Line | Texture | Scale Expression | @ bounce=1.0, pulse=1.0 | Rendered px |
|---|---|---|---|---|
| 480 | SoftRadialBloom (2160) | `1.8f * bounceIntensity * pulse` | 1.80 | **3888** ⚠ |
| 487 | SoftRadialBloom (2160) | `1.1f * bounceIntensity * pulse` | 1.10 | **2376** ⚠ |
| 494 | PointBloom (2160) | `0.7f * bounceIntensity * pulse` | 0.70 | **1512** |
| 500 | PointBloom (2160) | `0.35f * bounceIntensity * pulse` | 0.35 | **756** |

---

### 16. EternalMoonSwing.cs
**Path:** `Content/MoonlightSonata/Weapons/EternalMoon/Projectiles/EternalMoonSwing.cs`
`crescentScale` = `0.4f + 0.3f * _phaseIntensity` (0.4–0.7); `Projectile.scale` ≈ 1.0

| Line | Texture | Scale Expression | @ cS=0.7, pScale=1 | Rendered px |
|---|---|---|---|---|
| 1113 | SoftRadialBloom (2160) | `crescentScale * 1.8f * Projectile.scale` | 1.26 | **2722** ⚠ |
| 1118 | SoftRadialBloom (2160) | `crescentScale * 1.2f * Projectile.scale` | 0.84 | **1814** |
| 1123 | PointBloom (2160) | `crescentScale * 0.5f * Projectile.scale` | 0.35 | **756** |
| 1128 | PointBloom (2160) | `crescentScale * 0.2f * Projectile.scale` | 0.14 | **302** |
| 1136 | MS Star Flare (1024) | `crescentScale * 0.35f * Projectile.scale` | 0.245 | **251** |
| 1146 | MS Glow Orb (1024) | `crescentScale * 1.4f * Projectile.scale * orbPulse` | ~0.98 | **1004** |

---

### 17. CrescentMoonProj.cs
**Path:** `Content/MoonlightSonata/Weapons/IncisorOfMoonlight/Projectiles/CrescentMoonProj.cs`
`pulse` = 0.85–1.15; `Projectile.scale` ≈ 1.0

**Head Draws:**

| Line | Texture | Scale Expression | Typical | Rendered px |
|---|---|---|---|---|
| 136 | SoftRadialBloom (2160) | `0.5f * Projectile.scale` | 0.50 | **1080** |
| 141 | 4PointedStarSoft (32) | `0.3f * pulse` | ~0.30 | **10** |
| 147 | 4PointedStarSoft (32) | `0.15f * pulse` | ~0.15 | **5** |

**Trail Afterimage Loop (Lines 153-163):**

| Line | Texture | Scale Expression | Range (fade=0–1) | Rendered px |
|---|---|---|---|---|
| 158 | SoftRadialBloom (2160) | `0.3f * fade * Projectile.scale` | 0–0.3 | **0–648** |
| 162 | 4PointedStarSoft (32) | `0.25f * fade` | 0–0.25 | **0–8** |

---

### 18. GoliathOfMoonlight.cs
**Path:** `Content/MoonlightSonata/Minions/GoliathOfMoonlight/GoliathOfMoonlight.cs`
**Texture:** `GoliathTextures.SoftRadialBloom` = **2160px** (both methods)

**DrawRiftAmbient (Lines 752-762):**
`pulse` = 0.15 baseline, up to 0.30 during beam charge

| Line | Context | Scale | Rendered px |
|---|---|---|---|
| 755 | Rift outer | `0.4f` | **864** |
| 758 | Inner rift | `0.25f` | **540** |
| 762 | Phase accent | `0.8f` | **1728** |

**DrawGoliathGlow (Lines 790-814):**

| Line | Context | Scale Expression | Typical | Rendered px |
|---|---|---|---|---|
| 793 | Eye/core glow | `0.3f` | 0.30 | **648** |
| 799 | Beam charge glow | `0.4f + chargeProgress * 0.3f` | 0.4–0.7 | **864–1512** |
| 806 | Full moon radiance | `0.5f` | 0.50 | **1080** |
| 813 | Conductor mode ring | `1.0f` | 1.00 | **2160** ⚠ |

---

### 19. ResurrectionProjectile.cs
**Path:** `Content/MoonlightSonata/Weapons/ResurrectionOfTheMoon/Projectiles/ResurrectionProjectile.cs`

**Noise-Masked Orb (Line 448) — shader pass:**
**Texture:** `_softCircleTex` = SoftCircle = **2160px**

| Line | Scale Expression | Range | Rendered px |
|---|---|---|---|
| 448 | `0.18f + CometPhase * 0.06f` | 0.18–0.24 | **389–518** |

**Bloom Layers (Lines 460-466):**
**Texture:** `CometTextures.SoftRadialBloom` = **2160px**

| Line | Context | Scale Expression | Range | Rendered px |
|---|---|---|---|---|
| 462 | Outer bloom halo | `0.18f + CometPhase * 0.08f` | 0.18–0.26 | **389–562** |
| 467 | Inner core glow | `0.1f + CometPhase * 0.04f` | 0.10–0.14 | **216–302** |

---

### 20. CometCore.cs
**Path:** `Content/MoonlightSonata/Weapons/ResurrectionOfTheMoon/Projectiles/CometCore.cs`

**DrawHeadGlow (Lines 309-325):**

| Line | Texture | Scale | Tex Size | Rendered px |
|---|---|---|---|---|
| 309 | SoftRadialBloom (`CometTextures`) | `0.35f` | 2160 | **756** |
| 312 | SoftRadialBloom (`CometTextures`) | `0.22f` | 2160 | **475** |
| 315 | SoftRadialBloom (`CometTextures`) | `0.16f` | 2160 | **346** |
| 321 | SoftGlow (`SPFTextures`) | `0.25f` | 1024 | **256** |
| 325 | SoftGlow (`SPFTextures`) | `0.12f` | 1024 | **123** |

**Sparkle Trail Points (Lines 485-486) — per trail point (every 3rd):**
**Textures:** SparkleHard (~32px), SparkleThin (~32×128)

| Line | Texture | Scale Expression | Range | Rendered px |
|---|---|---|---|---|
| 485 | SparkleHard | `0.08f + flash * 0.12f` | 0.08–0.20 | **3–6** |
| 486 | SparkleThin | `scale * 0.7f` | 0.056–0.14 | **2–4w, 7–18h** |

---

## Summary of Largest Bloom Draws (⚠ Potential Issues)

All draws rendering > **2000px** on screen:

| File | Texture | Scale | Rendered px |
|---|---|---|---|
| **BlackSwanFlareProj.cs** (empowered) | SoftRadialBloom | `baseScale * 4f` | **4320** |
| **CodaHeldSwing.cs** (impact) | SoftRadialBloom | `1.8f` | **3888** |
| **SerenadeBeam.cs** (halo) | SoftRadialBloom | `1.8f` | **3888** |
| **BlackSwanFlareProj.cs** (empowered) | SoftRadialBloom | `baseScale * 3.2f` | **3456** |
| **CodaZenithSword.cs** | SoftRadialBloom | `1.44f * pulse` | **3110** |
| **ResonanceRapidBullet.cs** (spawn) | SoftRadialBloom | `1.4f` | **3024** |
| **BlackSwanFlareProj.cs** (bloom) | SoftRadialBloom | `baseScale * 2.8f` | **3024** |
| **CodaHeldSwing.cs** (crescent) | SoftRadialBloom | `1.6f * i` | **2938** |
| **OpusSwingProjectile.cs** (crescent) | SoftRadialBloom | `1.6f * i` | **2938** |
| **RequiemSwingProjectile.cs** (crescent) | SoftRadialBloom | `1.6f * i` | **2938** |
| **ConductorSwingProjectile.cs** (crescent) | SoftRadialBloom | `1.6f * i` | **2938** |
| **FractalSwingProjectile.cs** (crescent) | SoftRadialBloom | `cS * 2.0f` | **2808** |
| **EternalMoonSwing.cs** (crescent) | SoftRadialBloom | `cS * 1.8f` | **2722** |
| **DualFatedChimeSwingProj.cs** (phase≥3) | SoftGlow (1024) | `2.0f * pScale` | **2662** |
| **CodaHeldSwing.cs** (impact) | SoftRadialBloom | `1.2f` | **2592** |
| **SerenadeBeam.cs** (spectral ring) | SoftRadialBloom | `1.1f` | **2376** |
| **FractalSwingProjectile.cs** (crescent) | SoftRadialBloom | `cS * 1.6f * orbPulse` | **2246** |
| **GoliathOfMoonlight.cs** (conductor ring) | SoftRadialBloom | `1.0f` | **2160** |
| **All 5 Fate CrescentBloom** files | SoftRadialBloom | `1.1f * i` | **2020** |

### Key Observations

1. **SoftRadialBloom (2160px)** is the dominant offender — it's drawn at scales ≥1.0 in most CrescentBloom implementations, producing 2000-4000px rendered sprites.

2. **Five Fate weapons share identical CrescentBloom code** (Coda, Opus, Requiem, Conductor, Fractal) — fixing one pattern fixes all five.

3. **BlackSwanFlareProj empowered mode** is the single largest rendered bloom in the project at **4320px** (`SoftRadialBloom × 4.0`).

4. **Impact/spawn flash draws** in CodaHeldSwing and ResonanceRapidBullet push SoftRadialBloom to 3024-3888px.

5. **Moonlight Sonata files** (Goliath, EternalMoon, Serenade, Resurrection, CometCore) are generally more conservative, with the exception of SerenadeBeam's wide halo.

6. **La Campanella files** use the 1024px SoftGlow (direct load), keeping rendered sizes under 1600px in most cases. DualFatedChime phase≥3 spikes to 2662px.

7. **Small particle star textures** (32px) are appropriately scaled and pose no size concerns.
