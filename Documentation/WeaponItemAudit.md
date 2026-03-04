# Comprehensive Weapon Item File Audit

**Date:** Generated audit of all 44 weapon item files across 6 themes  
**Standard:** Foundation Weapons Pattern (XML summary, SetStaticDefaults, SetDefaults order, ModifyTooltips structure, AddRecipes, method order)

---

## Executive Summary

### Systemic Issues (Cross-Theme)

| Issue | Affected Weapons | Severity |
|-------|-----------------|----------|
| **Missing AddRecipes** | 37 of 44 weapons | HIGH |
| **Missing SetStaticDefaults** | 30 of 44 weapons | MEDIUM |
| **Missing XML Summary** | 7 of 44 weapons | MEDIUM |
| **Inconsistent tooltip keys** | 5 of 44 weapons | LOW |
| **Wrong lore color** | 2 of 44 weapons | MEDIUM |
| **Wrong rarity** | 1 of 44 weapons | HIGH |
| **Utility class color mismatches** | 3 Utils files | LOW |

### Theme Health Scorecard

| Theme | Weapons | XML Summary | SetStaticDefaults | AddRecipes | Lore Color | Tooltip Keys | Overall |
|-------|---------|-------------|-------------------|------------|------------|-------------|---------|
| **Moonlight Sonata** | 5 | 5/5 ✅ | 1/5 ⚠️ | 5/5 ✅ | 5/5 ✅ | 5/5 ✅ | GOOD |
| **Eroica** | 7 | 0/7 ❌ | 0/7 ❌ | 0/7 ❌ | 7/7 ✅ | 7/7 ✅ | POOR |
| **La Campanella** | 7 | 7/7 ✅ | 2/7 ⚠️ | 0/7 ❌ | 7/7 ✅ | 7/7 ✅ | FAIR |
| **Enigma Variations** | 8 | 8/8 ✅ | 0/8 ❌ | 0/8 ❌ | 8/8 ✅ | 7/8 ⚠️ | FAIR |
| **Swan Lake** | 7 | 7/7 ✅ | 5/7 ⚠️ | 1/7 ❌ | 6/7 ⚠️ | 7/7 ✅ | FAIR |
| **Fate** | 10 | 10/10 ✅ | 0/10 ❌ | 1/10 ❌ | 9/10 ⚠️ | 6/10 ⚠️ | FAIR |

---

## Theme 1: Moonlight Sonata (5 weapons)

**Expected Lore Color:** `new Color(140, 100, 200)` — Purple

### EternalMoon
**File:** `Content/MoonlightSonata/Weapons/EternalMoon/EternalMoon.cs`

| Check | Status | Detail |
|-------|--------|--------|
| XML Summary | ✅ | Detailed — mentions 5-phase lunar combo, Lunar Surge, etc. |
| SetStaticDefaults | ❌ MISSING | — |
| SetDefaults order | ⚠️ | width/height before damage; DamageType after useStyle (non-standard) |
| ModifyTooltips | ✅ | Effect1-4, Phase, Lore |
| Lore format | ✅ | Single-quoted: `"'The eternal cycle made blade — each swing echoes moonlight on water'"` |
| Lore color | ✅ | `new Color(140, 100, 200)` |
| AddRecipes | ✅ | Real recipe (MoonlightsResonantEnergy, ResonantCoreOfMoonlightSonata, ShardsOfMoonlitTempo) |
| Method order | ✅ | SetDefaults → ModifyTooltips → Shoot → CanShoot → HoldItem → AltFunctionUse → CanHitNPC → CanHitPvp → AddRecipes |

**Issues:** Missing SetStaticDefaults.

---

### IncisorOfMoonlight
**File:** `Content/MoonlightSonata/Weapons/IncisorOfMoonlight/IncisorOfMoonlight.cs`

| Check | Status | Detail |
|-------|--------|--------|
| XML Summary | ✅ | Present |
| SetStaticDefaults | ❌ MISSING | — |
| SetDefaults order | ⚠️ | `noMelee` set late (after channel); split property assignment |
| ModifyTooltips | ✅ | Effect1-3, Lore |
| Lore format | ✅ | Single-quoted: `"'A blade forged from crystallized moonlight — each swing traces a constellation'"` |
| Lore color | ✅ | `new Color(140, 100, 200)` |
| AddRecipes | ✅ | Real recipe present |
| Method order | ⚠️ | ModifyTooltips before AltFunctionUse (non-standard but acceptable) |

**Issues:**
1. ❌ **WRONG RARITY**: Uses `EroicaRainbowRarity` instead of a Moonlight Sonata rarity. This is a cross-theme contamination bug.
2. Missing SetStaticDefaults.

---

### MoonlightsCalling
**File:** `Content/MoonlightSonata/Weapons/MoonlightsCalling/MoonlightsCalling.cs`

| Check | Status | Detail |
|-------|--------|--------|
| XML Summary | ✅ | Present |
| SetStaticDefaults | ❌ MISSING | — |
| SetDefaults order | ✅ | Standard order |
| ModifyTooltips | ✅ | Effect1-6, Resonance, Cooldown, Lore |
| Lore format | ✅ | Single-quoted: `"'The moon whispers secrets to those who listen — each note a color, each color a truth'"` |
| Lore color | ✅ | `new Color(140, 100, 200)` |
| AddRecipes | ✅ | Real recipe present |
| Method order | ✅ | Standard |

**Issues:** Missing SetStaticDefaults. Uses `Item.staff[Item.type] = true` in SetDefaults (should be in SetStaticDefaults).

---

### ResurrectionOfTheMoon
**File:** `Content/MoonlightSonata/Weapons/ResurrectionOfTheMoon/ResurrectionOfTheMoon.cs`

| Check | Status | Detail |
|-------|--------|--------|
| XML Summary | ✅ | Present |
| SetStaticDefaults | ✅ | `ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true` |
| SetDefaults order | ✅ | Standard order |
| ModifyTooltips | ✅ | Chamber, LunarPhase, Effect1-7, Reload, Synergy, Lore |
| Lore format | ✅ | Single-quoted: `"'From death comes rebirth in silver light — the final movement that silences all'"` |
| Lore color | ✅ | `new Color(140, 100, 200)` |
| AddRecipes | ✅ | Real recipe present |
| Method order | ✅ | Standard |

**Issues:** None — this is the **gold standard** for Moonlight Sonata weapons.

---

### StaffOfTheLunarPhases
**File:** `Content/MoonlightSonata/Weapons/StaffOfTheLunarPhases/StaffOfTheLunarPhases.cs`

| Check | Status | Detail |
|-------|--------|--------|
| XML Summary | ✅ | Present |
| SetStaticDefaults | ❌ MISSING | — |
| SetDefaults order | ✅ | Standard order |
| ModifyTooltips | ✅ | Effect1-6, LunarPhase, ConductorActive, Lore |
| Lore format | ✅ | Single-quoted: `"'The conductor raises the baton — and the moonlight obeys'"` |
| Lore color | ✅ | `new Color(140, 100, 200)` |
| AddRecipes | ✅ | Real recipe present |
| Method order | ✅ | Standard |

**Issues:** Missing SetStaticDefaults.

---

## Theme 2: Eroica (7 weapons)

**Expected Lore Color:** `new Color(200, 50, 50)` — Scarlet  
**⚠️ THEME-WIDE ISSUES:** ALL 7 weapons are missing XML Summary, SetStaticDefaults, and AddRecipes. This is the least mature theme.

### BlossomOfTheSakura
**File:** `Content/Eroica/Weapons/BlossomOfTheSakura/BlossomOfTheSakura.cs` (~55 lines)

| Check | Status | Detail |
|-------|--------|--------|
| XML Summary | ❌ MISSING | — |
| SetStaticDefaults | ❌ MISSING | — |
| SetDefaults | ✅ | damage 75, Ranged, useTime 4, useAmmo Bullet |
| ModifyTooltips | ✅ | Effect1-3, Lore |
| Lore format | ✅ | Single-quoted: `"'The blossoms do not choose where they fall; they trust the wind.'"` |
| Lore color | ✅ | Uses `EroicaPalette.Scarlet` = `(200, 50, 50)` verified |
| AddRecipes | ❌ MISSING | — |
| Shoot override | ❌ MISSING | Weapon fires no projectiles despite being Ranged |

**Issues:** Stub weapon — missing XML Summary, SetStaticDefaults, AddRecipes, and Shoot override.

---

### CelestialValor
**File:** `Content/Eroica/Weapons/CelestialValor/CelestialValor.cs` (~55 lines)

| Check | Status | Detail |
|-------|--------|--------|
| XML Summary | ❌ MISSING | — |
| SetStaticDefaults | ❌ MISSING | — |
| SetDefaults | ✅ | damage 320, MeleeNoSpeed, useTime 20 |
| ModifyTooltips | ✅ | Effect1-4, Lore |
| Lore format | ✅ | Single-quoted: `"'To wield valor is to accept that every victory demands sacrifice.'"` |
| Lore color | ✅ | `new Color(200, 50, 50)` direct |
| AddRecipes | ❌ MISSING | — |

**Issues:** Missing XML Summary, SetStaticDefaults, AddRecipes.

---

### FinalityOfTheSakura
**File:** `Content/Eroica/Weapons/FinalityOfTheSakura/FinalityOfTheSakura.cs` (~55 lines)

| Check | Status | Detail |
|-------|--------|--------|
| XML Summary | ❌ MISSING | — |
| SetStaticDefaults | ❌ MISSING | — |
| SetDefaults | ✅ | damage 320, Summon, mana 20, useTime 36 |
| ModifyTooltips | ✅ | Effect1-3, Lore |
| Lore format | ✅ | Single-quoted: `"'The sakura does not mourn its own falling; it becomes the wind.'"` |
| Lore color | ✅ | `new Color(200, 50, 50)` direct |
| AddRecipes | ❌ MISSING | — |
| Shoot | ✅ | Has AddBuff + NewProjectile |

**Issues:** Missing XML Summary, SetStaticDefaults, AddRecipes.

---

### FuneralPrayer
**File:** `Content/Eroica/Weapons/FuneralPrayer/FuneralPrayer.cs` (~60 lines)

| Check | Status | Detail |
|-------|--------|--------|
| XML Summary | ❌ MISSING | Has method-level summary on `RegisterBeamHit` but no class summary |
| SetStaticDefaults | ❌ MISSING | — |
| SetDefaults | ✅ | damage 340, Magic, mana 14, useTime 18 |
| ModifyTooltips | ✅ | Effect1-3, Lore |
| Lore format | ✅ | Single-quoted: `"'We pray not for victory. We pray for those who ensured it.'"` |
| Lore color | ✅ | `new Color(200, 50, 50)` direct |
| AddRecipes | ❌ MISSING | — |
| Shoot | ❌ MISSING | Magic weapon without Shoot override |

**Issues:** Missing XML Summary, SetStaticDefaults, AddRecipes, Shoot override.

---

### PiercingLightOfTheSakura
**File:** `Content/Eroica/Weapons/PiercingLightOfTheSakura/PiercingLightOfTheSakura.cs` (~70 lines)

| Check | Status | Detail |
|-------|--------|--------|
| XML Summary | ❌ MISSING | — |
| SetStaticDefaults | ❌ MISSING | — |
| SetDefaults | ✅ | damage 155, Ranged, useTime 8, useAmmo Bullet |
| ModifyTooltips | ✅ | Effect1-3, Lore |
| Lore format | ✅ | Single-quoted: `"'The light that pierces is the one that never faltered.'"` |
| Lore color | ✅ | `new Color(200, 50, 50)` direct |
| AddRecipes | ❌ MISSING | — |
| Shoot | ✅ | Has Culmination mechanic |

**Issues:** Missing XML Summary, SetStaticDefaults, AddRecipes.

---

### SakurasBlossom
**File:** `Content/Eroica/Weapons/SakurasBlossom/SakurasBlossom.cs` (~55 lines)

| Check | Status | Detail |
|-------|--------|--------|
| XML Summary | ❌ MISSING | — |
| SetStaticDefaults | ❌ MISSING | — |
| SetDefaults | ✅ | damage 350, MeleeNoSpeed, useTime 20 |
| ModifyTooltips | ✅ | Effect1-3, Lore |
| Lore format | ✅ | Single-quoted: `"'Every petal that falls is a promise kept.'"` |
| Lore color | ✅ | `new Color(200, 50, 50)` direct |
| AddRecipes | ❌ MISSING | — |

**Issues:** Missing XML Summary, SetStaticDefaults, AddRecipes.

---

### TriumphantFractal
**File:** `Content/Eroica/Weapons/TriumphantFractal/TriumphantFractal.cs` (~55 lines)

| Check | Status | Detail |
|-------|--------|--------|
| XML Summary | ❌ MISSING | — |
| SetStaticDefaults | ❌ MISSING | — |
| SetDefaults | ✅ | damage 518, Magic, mana 19, useTime 25 |
| ModifyTooltips | ✅ | Effect1-3, Lore |
| Lore format | ✅ | Single-quoted: `"'In every fragment of heroism, the whole sacrifice echoes.'"` |
| Lore color | ✅ | `new Color(200, 50, 50)` direct |
| AddRecipes | ❌ MISSING | — |

**Issues:** Missing XML Summary, SetStaticDefaults, AddRecipes.

---

## Theme 3: La Campanella (7 weapons)

**Expected Lore Color:** `new Color(255, 140, 40)` — Infernal Orange

### DualFatedChime
**File:** `Content/LaCampanella/ResonantWeapons/DualFatedChime/DualFatedChime.cs` (~180 lines)

| Check | Status | Detail |
|-------|--------|--------|
| XML Summary | ✅ | Detailed — mentions 5-phase Inferno Waltz combo |
| SetStaticDefaults | ✅ | `Item.ResearchUnlockCount = 1` |
| SetDefaults | ✅ | damage 380, MeleeNoSpeed, useTime 16 |
| ModifyTooltips | ✅ | Effect1-4, Lore |
| Lore format | ✅ | Single-quoted: `"'Two bells toll as one — their song turns steel to cinder'"` |
| Lore color | ✅ | `DualFatedChimeUtils.LoreColor` = `(255, 140, 40)` verified |
| AddRecipes | ❌ MISSING | — |
| Method order | ✅ | Standard |

**Issues:** Missing AddRecipes. Has Texture override, PostDrawInWorld, HoldItem, Shoot, CanShoot.

---

### FangOfTheInfiniteBell
**File:** `Content/LaCampanella/ResonantWeapons/FangOfTheInfiniteBell/FangOfTheInfiniteBell.cs` (~120 lines)

| Check | Status | Detail |
|-------|--------|--------|
| XML Summary | ✅ | Present |
| SetStaticDefaults | ❌ MISSING | — |
| SetDefaults | ✅ | damage 95, Magic, mana 12, useTime 18 |
| ModifyTooltips | ✅ | Effect1-4, Lore |
| Lore format | ✅ | Single-quoted: `"'Infinity is not a destination; it is a bell that rings without ceasing.'"` |
| Lore color | ✅ | `FangOfTheInfiniteBellUtils.LoreColor` = `(255, 140, 40)` verified |
| AddRecipes | ❌ MISSING | — |

**Issues:** Missing SetStaticDefaults, AddRecipes.

---

### GrandioseChime
**File:** `Content/LaCampanella/ResonantWeapons/GrandioseChime/GrandioseChime.cs` (~145 lines)

| Check | Status | Detail |
|-------|--------|--------|
| XML Summary | ✅ | Present |
| SetStaticDefaults | ❌ MISSING | — |
| SetDefaults | ✅ | damage 240, Ranged, useTime 18, useAmmo Bullet |
| ModifyTooltips | ✅ | Effect1-4, Lore |
| Lore format | ✅ | Single-quoted: `"'When the grand chime sounds, the world holds its breath.'"` |
| Lore color | ✅ | `new Color(255, 140, 40)` direct |
| AddRecipes | ❌ MISSING | — |

**Notes:** Class is `GrandioseChimeItem` with `override string Name => "GrandioseChime"`. Missing SetStaticDefaults, AddRecipes.

---

### IgnitionOfTheBell
**File:** `Content/LaCampanella/ResonantWeapons/IgnitionOfTheBell/IgnitionOfTheBell.cs` (~130 lines)

| Check | Status | Detail |
|-------|--------|--------|
| XML Summary | ✅ | Present |
| SetStaticDefaults | ❌ MISSING | — |
| SetDefaults | ✅ | damage 340, MeleeNoSpeed, useTime 18 |
| ModifyTooltips | ✅ | Effect1-3, Lore |
| Lore format | ✅ | Single-quoted: `"'The first spark was all it took. The bell has been burning ever since.'"` |
| Lore color | ✅ | `IgnitionOfTheBellUtils.LoreColor` = `(255, 140, 40)` verified |
| AddRecipes | ❌ MISSING | — |

**Issues:** Missing SetStaticDefaults, AddRecipes.

---

### InfernalChimesCalling
**File:** `Content/LaCampanella/ResonantWeapons/InfernalChimesCalling/InfernalChimesCalling.cs` (~130 lines)

| Check | Status | Detail |
|-------|--------|--------|
| XML Summary | ✅ | Present |
| SetStaticDefaults | ✅ | `GamepadWholeScreenUseRange`, `LockOnIgnoresCollision` |
| SetDefaults | ✅ | damage 145, Summon, mana 20, useTime 36 |
| ModifyTooltips | ✅ | Effect1-4, Lore |
| Lore format | ✅ | Single-quoted: `"'The choir sings not hymns of peace, but anthems of annihilation.'"` |
| Lore color | ✅ | `InfernalChimesCallingUtils.LoreColor` = `(255, 140, 40)` verified |
| AddRecipes | ❌ MISSING | — |

**Notes:** Class is `InfernalChimesCallingItem` with `override string Name => "InfernalChimesCalling"`.

---

### PiercingBellsResonance
**File:** `Content/LaCampanella/ResonantWeapons/PiercingBellsResonance/PiercingBellsResonance.cs` (~180 lines)

| Check | Status | Detail |
|-------|--------|--------|
| XML Summary | ✅ | Present |
| SetStaticDefaults | ❌ MISSING | — |
| SetDefaults | ✅ | damage 165, Ranged, useTime 12, useAmmo Bullet |
| ModifyTooltips | ✅ | Effect1-5, Lore |
| Lore format | ✅ | Single-quoted: `"'A single note, perfectly placed, can shatter a fortress.'"` |
| Lore color | ✅ | `new Color(255, 140, 40)` direct |
| AddRecipes | ❌ MISSING | — |

**Notes:** Class is `PiercingBellsResonanceItem` with `override string Name => "PiercingBellsResonance"`.

---

### SymphonicBellfireAnnihilator
**File:** `Content/LaCampanella/ResonantWeapons/SymphonicBellfireAnnihilator/SymphonicBellfireAnnihilator.cs` (~150 lines)

| Check | Status | Detail |
|-------|--------|--------|
| XML Summary | ✅ | Present |
| SetStaticDefaults | ❌ MISSING | — |
| SetDefaults | ✅ | damage 494, Ranged, useTime 40, useAmmo Rocket |
| ModifyTooltips | ✅ | Effect1-5, Lore |
| Lore format | ✅ | Single-quoted: `"'When the bell toll becomes a bombardment, even silence trembles.'"` |
| Lore color | ✅ | `new Color(255, 140, 40)` direct |
| AddRecipes | ❌ MISSING | — |

**Notes:** Class is `SymphonicBellfireAnnihilatorItem` with `override string Name`.

---

## Theme 4: Enigma Variations (8 weapons)

**Expected Lore Color:** `new Color(140, 60, 200)` — Void Purple  
**Note:** Uses `EnigmaPurple` constant throughout. Two weapons use `MeleeSwingItemBase`.

### CipherNocturne
**File:** `Content/EnigmaVariations/ResonantWeapons/CipherNocturne/CipherNocturne.cs` (608 lines total, item ~100 lines)

| Check | Status | Detail |
|-------|--------|--------|
| XML Summary | ✅ | Detailed |
| SetStaticDefaults | ❌ MISSING | — |
| SetDefaults | ✅ | damage 290, Magic, mana 6, useTime 10 |
| ModifyTooltips | ✅ | Effect1-6, Lore |
| Lore format | ✅ | Single-quoted: `"'Every glyph is a locked door. The key is always pain.'"` |
| Lore color | ✅ | `EnigmaPurple` = `(140, 60, 200)` |
| AddRecipes | ❌ MISSING | — |

**Notes:** Projectile class `RealityUnravelerBeam` is in same file — should be separated.

---

### DissonanceOfSecrets
**File:** `Content/EnigmaVariations/ResonantWeapons/DissonanceOfSecrets/DissonanceOfSecrets.cs` (537 lines)

| Check | Status | Detail |
|-------|--------|--------|
| XML Summary | ✅ | Present |
| SetStaticDefaults | ❌ MISSING | — |
| SetDefaults | ✅ | damage 275, Magic, mana 20, useTime 35 |
| ModifyTooltips | ✅ | Effect1-6, Lore |
| Lore format | ✅ | Single-quoted: `"'The truth and the lie travel side by side. Which strikes first is the secret.'"` |
| Lore color | ✅ | `EnigmaPurple` = `(140, 60, 200)` |
| AddRecipes | ❌ MISSING | — |

**Notes:** Projectile in same file — should be separated.

---

### FugueOfTheUnknown
**File:** `Content/EnigmaVariations/ResonantWeapons/FugueOfTheUnknown/FugueOfTheUnknown.cs` (660 lines)

| Check | Status | Detail |
|-------|--------|--------|
| XML Summary | ✅ | Present |
| SetStaticDefaults | ❌ MISSING | — |
| SetDefaults | ✅ | damage 252, Magic, mana 12, useTime 20 |
| ModifyTooltips | ✅ | Effect1-6, Lore |
| Lore format | ✅ | Single-quoted: `"'The first note was a question. The echoes were answers in a language no one speaks.'"` |
| Lore color | ✅ | `EnigmaPurple` = `(140, 60, 200)` |
| AddRecipes | ❌ MISSING | — |

---

### TacetsEnigma
**File:** `Content/EnigmaVariations/ResonantWeapons/TacetsEnigma/TacetsEnigma.cs` (506 lines)

| Check | Status | Detail |
|-------|--------|--------|
| XML Summary | ✅ | Present |
| SetStaticDefaults | ❌ MISSING | — |
| SetDefaults | ✅ | damage 265, Ranged, useTime 18, useAmmo Bullet |
| ModifyTooltips | ✅ | Effect1-6, Lore |
| Lore format | ✅ | Single-quoted: `"'Where silence gathers, the enigma strikes.'"` |
| Lore color | ✅ | `EnigmaPurple` = `(140, 60, 200)` |
| AddRecipes | ❌ MISSING | — |

---

### TheSilentMeasure
**File:** `Content/EnigmaVariations/ResonantWeapons/TheSilentMeasure/TheSilentMeasure.cs` (683 lines)

| Check | Status | Detail |
|-------|--------|--------|
| XML Summary | ✅ | Present |
| SetStaticDefaults | ❌ MISSING | — |
| SetDefaults | ✅ | damage 245, Ranged, useTime 22, useAmmo Arrow |
| ModifyTooltips | ✅ | Effect1-6, Lore |
| Lore format | ✅ | Single-quoted: `"'The most terrifying sound is the one that comes after silence.'"` |
| Lore color | ✅ | `EnigmaPurple` = `(140, 60, 200)` |
| AddRecipes | ❌ MISSING | — |

---

### TheUnresolvedCadence
**File:** `Content/EnigmaVariations/ResonantWeapons/TheUnresolvedCadence/TheUnresolvedCadenceItem.cs` (~175 lines)

| Check | Status | Detail |
|-------|--------|--------|
| XML Summary | ✅ | Present |
| SetStaticDefaults | N/A | Handled by `MeleeSwingItemBase` |
| SetWeaponDefaults | ✅ | damage 600, useTime 22, knockBack 7 |
| AddWeaponTooltips | ✅ | Effect1-6, Lore |
| Lore format | ✅ | Single-quoted: `"'The question was never meant to have an answer.'"` |
| Lore color | ✅ | `EnigmaPurple` = `(140, 60, 200)` via `GetLoreColor()` override |
| AddRecipes | ❌ MISSING | Base class doesn't provide it |

**Notes:** Uses `MeleeSwingItemBase` → different method signatures (`SetWeaponDefaults`, `AddWeaponTooltips`, `GetLoreColor`).

---

### TheWatchingRefrain
**File:** `Content/EnigmaVariations/ResonantWeapons/TheWatchingRefrain/TheWatchingRefrain.cs` (895 lines)

| Check | Status | Detail |
|-------|--------|--------|
| XML Summary | ✅ | Very detailed |
| SetStaticDefaults | ❌ MISSING | — |
| SetDefaults | ✅ | damage 220, Summon, mana 14, useTime 30 |
| ModifyTooltips | ⚠️ | Uses non-standard keys: `EnigmaEffect`, `EnigmaEffect2-6`, `EnigmaLore` |
| Lore format | ✅ | Single-quoted: `"'It does not blink. It does not forget. It simply watches you unravel.'"` |
| Lore color | ✅ | `EnigmaPurple` = `(140, 60, 200)` |
| AddRecipes | ❌ MISSING | — |

**Issues:** ⚠️ Tooltip keys should be standardized to `Effect1-6` and `Lore`.

---

### VariationsOfTheVoid
**File:** `Content/EnigmaVariations/ResonantWeapons/VariationsOfTheVoid/VariationsOfTheVoidItem.cs` (~110 lines)

| Check | Status | Detail |
|-------|--------|--------|
| XML Summary | ✅ | Present |
| SetStaticDefaults | N/A | Handled by `MeleeSwingItemBase` |
| SetWeaponDefaults | ✅ | damage 380, useTime 18, knockBack 6 |
| AddWeaponTooltips | ✅ | Effect1-6, Lore |
| Lore format | ✅ | Single-quoted: `"'Every void remembers the shape of what it consumed.'"` |
| Lore color | ✅ | `EnigmaPurple` = `(140, 60, 200)` via `GetLoreColor()` override |
| AddRecipes | ❌ MISSING | — |

**Notes:** Uses `MeleeSwingItemBase`.

---

## Theme 5: Swan Lake (7 weapons)

**Expected Lore Color:** `new Color(240, 240, 255)` — Pure White

### CalloftheBlackSwan
**File:** `Content/SwanLake/ResonantWeapons/CalloftheBlackSwan/CalloftheBlackSwan.cs` (229 lines)

| Check | Status | Detail |
|-------|--------|--------|
| XML Summary | ✅ | Detailed combat system description |
| SetStaticDefaults | ✅ | `Item.ResearchUnlockCount = 1` |
| SetDefaults | ✅ | damage 400, MeleeNoSpeed, useTime 28 |
| ModifyTooltips | ✅ | Effect1-4, Lore |
| Lore format | ✅ | Single-quoted: `"'She danced not for love, but for the ruin of those who watch.'"` |
| Lore color | ✅ | `new Color(240, 240, 255)` direct |
| AddRecipes | ❌ MISSING | — |

**Notes:** `BlackSwanUtils.LoreColor` is `(220, 225, 235)` — doesn't match theme standard, but item uses direct color so no bug. Utils should be corrected for consistency.

---

### CallofthePearlescentLake
**File:** `Content/SwanLake/ResonantWeapons/CallofthePearlescentLake/CallofthePearlescentLake.cs`

| Check | Status | Detail |
|-------|--------|--------|
| XML Summary | ✅ | Present |
| SetStaticDefaults | ✅ | `Item.ResearchUnlockCount = 1` |
| SetDefaults | ✅ | damage 380, Ranged, useTime 8, useAmmo Bullet |
| ModifyTooltips | ✅ | Effect1-4, Lore |
| Lore format | ✅ | Single-quoted: `"'The lake does not forgive those who disturb its surface.'"` |
| Lore color | ✅ | `new Color(240, 240, 255)` direct |
| AddRecipes | ❌ MISSING | — |

**Notes:** `PearlescentUtils.LoreColor` is `(220, 225, 235)` — doesn't match theme standard, but item uses direct color. Utils should be corrected.

---

### ChromaticSwanSong
**File:** `Content/SwanLake/ResonantWeapons/ChromaticSwanSong/ChromaticSwanSong.cs` (203 lines)

| Check | Status | Detail |
|-------|--------|--------|
| XML Summary | ✅ | Present |
| SetStaticDefaults | ✅ | `Item.ResearchUnlockCount = 1` |
| SetDefaults | ✅ | damage 290, Magic, mana 8, useTime 12 |
| ModifyTooltips | ✅ | Effect1-4, Lore |
| Lore format | ✅ | Single-quoted: `"'The final song is always the most beautiful. It has to be.'"` |
| Lore color | ✅ | `ChromaticSwanUtils.LoreColor` = `(240, 240, 255)` verified |
| AddRecipes | ❌ MISSING | — |

---

### FeatheroftheIridescentFlock
**File:** `Content/SwanLake/ResonantWeapons/FeatheroftheIridescentFlock/FeatheroftheIridescentFlock.cs`

| Check | Status | Detail |
|-------|--------|--------|
| XML Summary | ✅ | Present |
| SetStaticDefaults | ✅ | `ResearchUnlockCount = 1`, `StaffMinionSlotsRequired` |
| SetDefaults | ✅ | damage 260, Summon, mana 20, useTime 30 |
| ModifyTooltips | ✅ | Effect1-4, Lore |
| Lore format | ✅ | Single-quoted: `"'Alone, a swan is beautiful. Together, they are devastating.'"` |
| Lore color | ✅ | `FlockUtils.LoreColor` = `(240, 240, 255)` verified |
| AddRecipes | ❌ MISSING | — |

---

### IridescentWingspan
**File:** `Content/SwanLake/ResonantWeapons/IridescentWingspan/IridescentWingspan.cs` (206 lines)

| Check | Status | Detail |
|-------|--------|--------|
| XML Summary | ✅ | Present |
| SetStaticDefaults | ✅ | `ResearchUnlockCount`, `staff[Type]` |
| SetDefaults | ✅ | damage 420, Magic, mana 16, useTime 18 |
| ModifyTooltips | ✅ | Effect1-4, Lore |
| Lore format | ✅ | Single-quoted: `"'To witness the full wingspan is to know both the beauty and the death.'"` |
| Lore color | ✅ | `new Color(240, 240, 255)` direct |
| AddRecipes | ❌ MISSING | — |

---

### TheSwansLament
**File:** `Content/SwanLake/ResonantWeapons/TheSwansLament/TheSwansLament.cs`

| Check | Status | Detail |
|-------|--------|--------|
| XML Summary | ✅ | Present |
| SetStaticDefaults | ❌ MISSING | — |
| SetDefaults | ✅ | damage 180, Ranged, useTime 35, useAmmo Bullet |
| ModifyTooltips | ✅ | Effect1-5, Lore |
| Lore format | ✅ | Single-quoted: `"'Each shot is a tear, and each tear is a farewell.'"` |
| Lore color | ✅ | `new Color(240, 240, 255)` direct |
| AddRecipes | ❌ MISSING | — |

**Notes:** `LamentUtils.LoreColor` is `(180, 175, 200)` — wrong value, but item uses direct color. Utils should be corrected.

---

### FeathersCall (Transformation Item)
**File:** `Content/SwanLake/Items/FeathersCall/FeathersCall.cs` (801 lines)

| Check | Status | Detail |
|-------|--------|--------|
| XML Summary | ✅ | Present |
| SetStaticDefaults | ✅ | `ResearchUnlockCount = 1` |
| SetDefaults | ✅ | useTime 30, HoldUp style, non-damage transformation item |
| ModifyTooltips | ✅ | Effect1-4, Lore |
| Lore format | ✅ | Single-quoted: `"'Become the swan, and let the melody flow through you'"` |
| Lore color | ❌ WRONG | `new Color(220, 225, 235)` — should be `(240, 240, 255)` |
| AddRecipes | ✅ | Present (empty — boss drop only, documented in comment) |

**Issues:** ❌ **Wrong lore color** — uses `(220, 225, 235)` instead of Swan Lake standard `(240, 240, 255)`.

---

## Theme 6: Fate (10 weapons)

**Expected Lore Color:** `new Color(180, 40, 80)` — Cosmic Crimson  
**⚠️ THEME-WIDE ISSUES:** All 10 weapons missing SetStaticDefaults. 9 of 10 missing AddRecipes. 4 of 10 use non-standard tooltip keys.

### CodaOfAnnihilation (The Zenith)
**File:** `Content/Fate/ResonantWeapons/CodaOfAnnihilation/CodaOfAnnihilationItem.cs` (287 lines)

| Check | Status | Detail |
|-------|--------|--------|
| XML Summary | ✅ | Present — describes Zenith-equivalent weapon |
| SetStaticDefaults | ❌ MISSING | — |
| SetDefaults | ✅ | damage 1350, Melee, useTime 18 |
| ModifyTooltips | ⚠️ | Uses non-standard keys: `FateEffect`, `FateSpecial`, `FateStacks`, `FateDetonate`, `FateFinale` |
| Lore format | ✅ | Single-quoted: `"'All melodies find their end here. This is the final bar.'"` |
| Lore color | ❌ WRONG | `CodaUtils.LoreColor` = `(220, 60, 90)` — should be `(180, 40, 80)` |
| AddRecipes | ✅ | Extensive recipe using weapons from all 6 themes + Zenith + energies |
| Method order | ⚠️ | AddRecipes before ModifyTooltips (non-standard) |

**Issues:**
1. ❌ **Wrong lore color** — `CodaUtils.LoreColor` = `(220, 60, 90)`, should be `(180, 40, 80)`.
2. ⚠️ Non-standard tooltip keys.
3. ⚠️ AddRecipes placed before ModifyTooltips.
4. Missing SetStaticDefaults.

---

### DestinysCrescendo
**File:** `Content/Fate/ResonantWeapons/DestinysCrescendo/DestinysCrescendoItem.cs`

| Check | Status | Detail |
|-------|--------|--------|
| XML Summary | ✅ | Present |
| SetStaticDefaults | ❌ MISSING | — |
| SetDefaults | ✅ | damage 400, Summon, mana 20, useTime 36 |
| ModifyTooltips | ⚠️ | Non-standard keys: `FateEffect`, `FateSpecial`, `Escalation`, `Beams`, `Presence`, `Reset` |
| Lore format | ✅ | Single-quoted: `"'At the crescendo, even gods must answer the conductor's call'"` |
| Lore color | ✅ | `new Color(180, 40, 80)` direct |
| AddRecipes | ❌ MISSING | — |

**Issues:** Missing SetStaticDefaults, AddRecipes. Non-standard tooltip keys.

---

### FractalOfTheStars
**File:** `Content/Fate/ResonantWeapons/FractalOfTheStars/FractalOfTheStarsItem.cs`

| Check | Status | Detail |
|-------|--------|--------|
| XML Summary | ✅ | Detailed self-contained system description |
| SetStaticDefaults | ❌ MISSING | — |
| SetDefaults | ✅ | damage 850, Melee, useTime 22 |
| ModifyTooltips | ✅ | Effect1-5, Lore |
| Lore format | ✅ | Single-quoted: `"'Every star contains a universe. Every universe, another blade.'"` |
| Lore color | ✅ | `new Color(180, 40, 80)` direct |
| AddRecipes | ❌ MISSING | — |

---

### LightOfTheFuture
**File:** `Content/Fate/ResonantWeapons/LightOfTheFuture/LightOfTheFutureItem.cs` (252 lines)

| Check | Status | Detail |
|-------|--------|--------|
| XML Summary | ✅ | Detailed self-contained system description |
| SetStaticDefaults | ❌ MISSING | — |
| SetDefaults | ✅ | damage 680, Ranged, useTime 30, useAmmo Bullet |
| ModifyTooltips | ✅ | Effect1-5, Lore |
| Lore format | ✅ | Single-quoted: `"'Aim not where they are, but where fate decrees they shall be.'"` |
| Lore color | ✅ | `new Color(180, 40, 80)` direct |
| AddRecipes | ❌ MISSING | — |

---

### OpusUltima
**File:** `Content/Fate/ResonantWeapons/OpusUltima/OpusUltimaItem.cs`

| Check | Status | Detail |
|-------|--------|--------|
| XML Summary | ✅ | Detailed self-contained system description |
| SetStaticDefaults | ❌ MISSING | — |
| SetDefaults | ✅ | damage 720, Melee, useTime 28 |
| ModifyTooltips | ✅ | Effect1-5, Lore |
| Lore format | ✅ | Single-quoted: `"'This is not a weapon. This is the masterwork — the opus that was always meant to be.'"` |
| Lore color | ✅ | `new Color(180, 40, 80)` direct |
| AddRecipes | ❌ MISSING | — |

---

### ResonanceOfABygoneReality
**File:** `Content/Fate/ResonantWeapons/ResonanceOfABygoneReality/ResonanceOfABygoneRealityItem.cs`

| Check | Status | Detail |
|-------|--------|--------|
| XML Summary | ✅ | Present |
| SetStaticDefaults | ❌ MISSING | — |
| SetDefaults | ✅ | damage 400, Ranged, useTime 6, useAmmo Bullet |
| ModifyTooltips | ⚠️ | Non-standard keys: `FateEffect`, `FateSpecial`, `FateResonance`, `FateFade` |
| Lore format | ✅ | Single-quoted: `"'What you hear is the echo of a universe that no longer exists.'"` |
| Lore color | ✅ | `new Color(180, 40, 80)` direct |
| AddRecipes | ❌ MISSING | — |

**Issues:** Non-standard tooltip keys.

---

### RequiemOfReality
**File:** `Content/Fate/ResonantWeapons/RequiemOfReality/RequiemOfRealityItem.cs`

| Check | Status | Detail |
|-------|--------|--------|
| XML Summary | ✅ | Detailed self-contained system description |
| SetStaticDefaults | ❌ MISSING | — |
| SetDefaults | ✅ | damage 740, Melee, useTime 20 |
| ModifyTooltips | ✅ | Effect1-4, Lore |
| Lore format | ✅ | Single-quoted: `"'Reality sang its last note when this blade was forged'"` |
| Lore color | ✅ | `new Color(180, 40, 80)` direct |
| AddRecipes | ❌ MISSING | — |

---

### SymphonysEnd
**File:** `Content/Fate/ResonantWeapons/SymphonysEnd/SymphonysEndItem.cs`

| Check | Status | Detail |
|-------|--------|--------|
| XML Summary | ✅ | Present |
| SetStaticDefaults | ❌ MISSING | — |
| SetDefaults | ✅ | damage 500, Magic, mana 8, useTime 8 |
| ModifyTooltips | ✅ | Effect1-5, Lore |
| Lore format | ✅ | Single-quoted: `"'The symphony does not fade. It ENDS.'"` |
| Lore color | ✅ | `new Color(180, 40, 80)` direct |
| AddRecipes | ❌ MISSING | — |

---

### TheConductorsLastConstellation
**File:** `Content/Fate/ResonantWeapons/TheConductorsLastConstellation/TheConductorsLastConstellationItem.cs`

| Check | Status | Detail |
|-------|--------|--------|
| XML Summary | ✅ | Detailed self-contained system description |
| SetStaticDefaults | ❌ MISSING | — |
| SetDefaults | ✅ | damage 780, Melee, useTime 22 |
| ModifyTooltips | ✅ | Effect1-6, Lore |
| Lore format | ✅ | Single-quoted: `"'The last constellation he drew was a blade aimed at eternity'"` |
| Lore color | ✅ | `new Color(180, 40, 80)` direct |
| AddRecipes | ❌ MISSING | — |

---

### TheFinalFermata
**File:** `Content/Fate/ResonantWeapons/TheFinalFermata/TheFinalFermataItem.cs`

| Check | Status | Detail |
|-------|--------|--------|
| XML Summary | ✅ | Present |
| SetStaticDefaults | ❌ MISSING | — |
| SetDefaults | ✅ | damage 520, Magic, mana 30, useTime 45 |
| ModifyTooltips | ⚠️ | Non-standard keys: `FateEffect`, `FateSpecial`, `FatePower`, `FateAlign`, `FateSustain`, `FateSpecial2` |
| Lore format | ✅ | Single-quoted: `"'Time holds its breath. The fermata decides when it exhales.'"` |
| Lore color | ✅ | `new Color(180, 40, 80)` direct |
| AddRecipes | ❌ MISSING | — |

**Issues:** Non-standard tooltip keys.

---

## Priority Remediation List

### 🔴 HIGH Priority (Bugs / Wrong Data)

| # | Issue | File | Fix |
|---|-------|------|-----|
| 1 | **Wrong rarity** — IncisorOfMoonlight uses `EroicaRainbowRarity` | IncisorOfMoonlight.cs:78 | Change to MoonlightSonata rarity |
| 2 | **Wrong lore color** — CodaOfAnnihilation uses `(220, 60, 90)` | CodaUtils.cs:38 | Change to `(180, 40, 80)` |
| 3 | **Wrong lore color** — FeathersCall uses `(220, 225, 235)` | FeathersCall.cs | Change to `(240, 240, 255)` |
| 4 | **Missing AddRecipes** — 37 weapons have no recipes | See full list below | Add recipes for all weapons |

### 🟡 MEDIUM Priority (Missing Standard Elements)

| # | Issue | Affected | Fix |
|---|-------|----------|-----|
| 5 | **Missing SetStaticDefaults** | 30 weapons | Add `SetStaticDefaults` with at minimum `Item.ResearchUnlockCount = 1` |
| 6 | **Missing XML Summary** | All 7 Eroica weapons | Add class-level `/// <summary>` doc comments |
| 7 | **Non-standard tooltip keys** | TheWatchingRefrain, CodaOfAnnihilation, DestinysCrescendo, ResonanceOfABygoneReality, TheFinalFermata | Standardize to `Effect1`, `Effect2`, ..., `Lore` |

### 🟢 LOW Priority (Consistency Cleanup)

| # | Issue | Affected | Fix |
|---|-------|----------|-----|
| 8 | **Utils LoreColor mismatches** | BlackSwanUtils `(220,225,235)`, PearlescentUtils `(220,225,235)`, LamentUtils `(180,175,200)` | Update Utils to `(240,240,255)` to match theme standard |
| 9 | **MoonlightsCalling staff[] in SetDefaults** | MoonlightsCalling.cs | Move `Item.staff[Item.type] = true` to SetStaticDefaults |
| 10 | **Enigma projectiles in item files** | CipherNocturne, DissonanceOfSecrets | Consider separating projectile classes into own files |
| 11 | **Method order: AddRecipes before ModifyTooltips** | CodaOfAnnihilationItem.cs | Move ModifyTooltips before AddRecipes |

### Weapons Missing AddRecipes (37 total)

**Eroica (7):** BlossomOfTheSakura, CelestialValor, FinalityOfTheSakura, FuneralPrayer, PiercingLightOfTheSakura, SakurasBlossom, TriumphantFractal

**La Campanella (7):** DualFatedChime, FangOfTheInfiniteBell, GrandioseChime, IgnitionOfTheBell, InfernalChimesCalling, PiercingBellsResonance, SymphonicBellfireAnnihilator

**Enigma Variations (8):** CipherNocturne, DissonanceOfSecrets, FugueOfTheUnknown, TacetsEnigma, TheSilentMeasure, TheUnresolvedCadence, TheWatchingRefrain, VariationsOfTheVoid

**Swan Lake (6):** CalloftheBlackSwan, CallofthePearlescentLake, ChromaticSwanSong, FeatheroftheIridescentFlock, IridescentWingspan, TheSwansLament

**Fate (9):** DestinysCrescendo, FractalOfTheStars, LightOfTheFuture, OpusUltima, ResonanceOfABygoneReality, RequiemOfReality, SymphonysEnd, TheConductorsLastConstellation, TheFinalFermata

### Weapons Missing SetStaticDefaults (30 total)

**Moonlight Sonata (4):** EternalMoon, IncisorOfMoonlight, MoonlightsCalling, StaffOfTheLunarPhases

**Eroica (7):** All 7 weapons

**La Campanella (5):** FangOfTheInfiniteBell, GrandioseChime, IgnitionOfTheBell, PiercingBellsResonance, SymphonicBellfireAnnihilator

**Enigma Variations (6):** CipherNocturne, DissonanceOfSecrets, FugueOfTheUnknown, TacetsEnigma, TheSilentMeasure, TheWatchingRefrain (TheUnresolvedCadence and VariationsOfTheVoid use base class)

**Swan Lake (1):** TheSwansLament

**Fate (10):** All 10 weapons
