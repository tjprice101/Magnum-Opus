# Enigma Variations — Crash & VFX Audit Report

**Audited**: All 8 weapons under `Content/EnigmaVariations/ResonantWeapons/`  
**Scope**: Runtime crash risks, VFX issues, PreDraw return values, SpriteBatch state management  
**Date**: Auto-generated audit

---

## EXECUTIVE SUMMARY

| Weapon | Rating | Critical Issues |
|--------|--------|-----------------|
| **CipherNocturne** | ✅ LOW RISK | Safest pattern. Uses `?.Value` everywhere. No `catch {}`. |
| **DissonanceOfSecrets** | 🔴 WILL CRASH | Wrong path for EN Enigma Eye. Direct `.Value` without null checks. |
| **FugueOfTheUnknown** | 🔴 WILL CRASH | SpriteBatch.Draw in OnHitNPC (update phase). Wrong EN Enigma Eye path. |
| **TacetsEnigma** | 🔴 WILL CRASH | Wrong path for EN Enigma Eye. Direct `.Value` without null checks. |
| **TheSilentMeasure** | 🟠 HIGH RISK | Direct `EnigmaThemeTextures.ENxxx.Value` (no `?.`). Bare `catch {}` blocks. |
| **TheUnresolvedCadence** | ✅ LOW RISK | Uses `?.Value` in swing file. Direct `.Value` on ImmediateLoad (paths OK). |
| **TheWatchingRefrain** | 🟠 HIGH RISK | Direct `EnigmaThemeTextures.ENxxx.Value` (no `?.`) at 11 locations. Bare `catch {}`. |
| **VariationsOfTheVoid** | ⚠️ MEDIUM RISK | `LFTextures.xxx.Value` without null checks. Complex shader pipeline. |

### Systemic Issue: `EnigmaThemeTextures.LoadTex()` Can Return Null

The root cause of most crash risks in this theme:

```csharp
// EnigmaThemeTextures.cs — LoadTex returns null if asset missing
private static Asset<Texture2D> LoadTex(string path)
{
    if (!ModContent.HasAsset(path)) return null;  // ← CAN RETURN NULL
    return ModContent.Request<Texture2D>(path, AssetRequestMode.ImmediateLoad);
}
```

All `ENStarFlare`, `ENPowerEffectRing`, `ENEnigmaEye` properties can return `null`. Code doing `.Value` without `?.` will throw `NullReferenceException` if assets are ever missing.

**Currently the assets DO exist** (verified), so these won't crash *today*. But any asset rename/deletion breaks multiple weapons instantly.

### Systemic Issue: Wrong Path for EN Enigma Eye

Three weapons use hardcoded `ModContent.Request` with the **wrong subdirectory** for EN Enigma Eye:

| Weapon | Code Path Used | Actual Asset Location |
|--------|---------------|----------------------|
| DissonanceOfSecrets | `.../Impact Effects/EN Enigma Eye` | `.../Particles/EN Enigma Eye.png` |
| FugueOfTheUnknown | `.../Impact Effects/EN Enigma Eye` | `.../Particles/EN Enigma Eye.png` |
| TacetsEnigma | `.../Impact Effects/EN Enigma Eye` | `.../Particles/EN Enigma Eye.png` |

The asset **does not exist** at the `Impact Effects/` path. `ImmediateLoad` on a nonexistent path will either crash with `AssetLoadException` or return a failed-load Asset whose `.Value` is null/throws.

---

## WEAPON-BY-WEAPON AUDIT

---

### 1. CipherNocturne (Magic — Channeled Beam)

**Files:**
- [CipherNocturne.cs](Content/EnigmaVariations/ResonantWeapons/CipherNocturne/CipherNocturne.cs) (785 lines)
- [CipherUtils.cs](Content/EnigmaVariations/ResonantWeapons/CipherNocturne/Utilities/CipherUtils.cs)
- [CipherShaderLoader.cs](Content/EnigmaVariations/ResonantWeapons/CipherNocturne/Shaders/CipherShaderLoader.cs)
- Particles/ (3 files), Primitives/ (3 files), Dusts/ (1 file)

**Rating: ✅ LOW RISK**

**Crash Risks: None Critical**

- **EnigmaThemeTextures access** — Uses the SAFE `?.Value` pattern everywhere:
  - [Line 319](Content/EnigmaVariations/ResonantWeapons/CipherNocturne/CipherNocturne.cs#L319): `EnigmaThemeTextures.ENStarFlare?.Value` ✅
  - [Line 333](Content/EnigmaVariations/ResonantWeapons/CipherNocturne/CipherNocturne.cs#L333): `EnigmaThemeTextures.ENPowerEffectRing?.Value` ✅
  - [Line 348](Content/EnigmaVariations/ResonantWeapons/CipherNocturne/CipherNocturne.cs#L348): `EnigmaThemeTextures.ENEnigmaEye?.Value` ✅

- **SpriteBatch management** — Uses `CipherUtils.EnterAdditiveShaderRegion(sb)` / `ExitShaderRegion(sb)` — clean helper methods with proper Begin/End pairs.

- **No bare `catch {}` blocks** — GPU primitive sections don't have silent exception swallowing.

**PreDraw Return:** `return false` ✅ (both RealityUnravelerBeam and RealitySnapBack)

**VFX Issues: None significant.**

**Minor Concerns:**
- No try/catch around GPU primitive rendering sections. If a primitive renderer throws (e.g., device lost), the SpriteBatch will be left in the wrong state. This is minor since primitive renderers have their own null guards.

---

### 2. DissonanceOfSecrets (Magic — Growing Orb + Homing Bolts)

**Files:**
- [DissonanceOfSecrets.cs](Content/EnigmaVariations/ResonantWeapons/DissonanceOfSecrets/DissonanceOfSecrets.cs) (726 lines)
- [DissonanceUtils.cs](Content/EnigmaVariations/ResonantWeapons/DissonanceOfSecrets/Utilities/DissonanceUtils.cs)
- [DissonanceShaderLoader.cs](Content/EnigmaVariations/ResonantWeapons/DissonanceOfSecrets/Shaders/DissonanceShaderLoader.cs)
- Particles/ (3 files), Primitives/ (3 files), Dusts/ (1 file)

**Rating: 🔴 WILL CRASH**

**Crash Risk 1 — CRITICAL: Wrong EN Enigma Eye Path**
- [Line 245](Content/EnigmaVariations/ResonantWeapons/DissonanceOfSecrets/DissonanceOfSecrets.cs#L245) (Riddlebolt.PreDraw):
  ```csharp
  Texture2D enigmaEye = ModContent.Request<Texture2D>(
      "MagnumOpus/Assets/VFX Asset Library/Theme Specific/Enigma/Impact Effects/EN Enigma Eye",
      AssetRequestMode.ImmediateLoad).Value;
  ```
  **Asset does NOT exist at `Impact Effects/`** — actual location is `Particles/EN Enigma Eye.png`.
  This code runs in a conditional (`Main.GameUpdateCount % 30 < 10`) so it crashes ~33% of the time when Riddlebolt renders.

**Crash Risk 2 — HIGH: Direct `.Value` on ImmediateLoad**
- [Line 116](Content/EnigmaVariations/ResonantWeapons/DissonanceOfSecrets/DissonanceOfSecrets.cs#L116): `ModContent.Request<Texture2D>("...SoftRadialBloom", ImmediateLoad).Value` — path exists, OK
- [Line 227](Content/EnigmaVariations/ResonantWeapons/DissonanceOfSecrets/DissonanceOfSecrets.cs#L227): `ModContent.Request<Texture2D>("...EN Star Flare", ImmediateLoad).Value` — path exists, OK
- [Line 235](Content/EnigmaVariations/ResonantWeapons/DissonanceOfSecrets/DissonanceOfSecrets.cs#L235): `ModContent.Request<Texture2D>("...EN Power Effect Ring", ImmediateLoad).Value` — path exists, OK
- [Line 503](Content/EnigmaVariations/ResonantWeapons/DissonanceOfSecrets/DissonanceOfSecrets.cs#L503), [Line 598](Content/EnigmaVariations/ResonantWeapons/DissonanceOfSecrets/DissonanceOfSecrets.cs#L598), [Line 606](Content/EnigmaVariations/ResonantWeapons/DissonanceOfSecrets/DissonanceOfSecrets.cs#L606): Same pattern for bloom/starflare/ring — paths correct
- All these use hardcoded paths with no caching — loads texture every frame. Not a crash, but a performance concern.

**PreDraw Return:** `return false` ✅ (RiddleCascadeOrb), `return false` ✅ (Riddlebolt)

**VFX Issues:**
- All texture loads are done per-frame with `ImmediateLoad` instead of using `EnigmaThemeTextures` cached properties — performance waste.

---

### 3. FugueOfTheUnknown (Summoner — Orbiting Voice Projectiles)

**Files:**
- [FugueOfTheUnknown.cs](Content/EnigmaVariations/ResonantWeapons/FugueOfTheUnknown/FugueOfTheUnknown.cs) (777 lines)
- [FugueUtils.cs](Content/EnigmaVariations/ResonantWeapons/FugueOfTheUnknown/Utilities/FugueUtils.cs)
- [FugueShaderLoader.cs](Content/EnigmaVariations/ResonantWeapons/FugueOfTheUnknown/Shaders/FugueShaderLoader.cs)
- Particles/ (3 files), Primitives/ (3 files), Dusts/ (1 file)

**Rating: 🔴 WILL CRASH**

**Crash Risk 1 — CRITICAL: SpriteBatch.Draw() During OnHitNPC (Update Phase)**
- [Lines 571-586](Content/EnigmaVariations/ResonantWeapons/FugueOfTheUnknown/FugueOfTheUnknown.cs#L571-L586) (EchoMarkNPC.OnHitNPC — Harmonic Convergence VFX):
  ```csharp
  // Called during game update phase — Main.spriteBatch is NOT begun!
  Main.spriteBatch.Draw(convRingTex, convDrawPos, null, FugueUtils.VoicePurple * 0.6f, ...);
  Main.spriteBatch.Draw(convFlareTex, convDrawPos, null, FugueUtils.HarmonicWhite * 0.7f, ...);
  ```
  `OnHitNPC` executes during the game UPDATE phase, not the DRAW phase. `Main.spriteBatch` has not been `Begin()`'d at this point. **FNA throws `InvalidOperationException: Begin must be called before calling Draw`**.
  
  This crashes whenever a player accumulates 3+ Echo Mark stacks on an enemy and triggers Harmonic Convergence.

**Crash Risk 2 — CRITICAL: Wrong EN Enigma Eye Path**
- [Line 345](Content/EnigmaVariations/ResonantWeapons/FugueOfTheUnknown/FugueOfTheUnknown.cs#L345) (FugueVoiceProjectile.OnHitNPC):
  ```csharp
  Texture2D enigmaEye = ModContent.Request<Texture2D>(
      "MagnumOpus/Assets/VFX Asset Library/Theme Specific/Enigma/Impact Effects/EN Enigma Eye",
      AssetRequestMode.ImmediateLoad).Value;
  ```
  Wrong path — see systemic issue above. However, this is also in OnHitNPC (update phase), so the SpriteBatch issue would crash first.

**Crash Risk 3 — Direct `.Value` Without Protection**
- [Lines 177-180](Content/EnigmaVariations/ResonantWeapons/FugueOfTheUnknown/FugueOfTheUnknown.cs#L177-L180): Four direct `.Value` calls on `ImmediateLoad` in FugueVoiceProjectile.PreDraw — paths are correct.

**PreDraw Return:** `return false` ✅ (FugueVoiceProjectile)

**VFX Issues:**
- Lines 571-586: Even if the SpriteBatch crash were fixed, drawing additive VFX textures during OnHitNPC (AlphaBlend mode) would show black edges — the textures are designed for additive blending.
- Per-frame `ImmediateLoad` texture fetching — performance concern.

---

### 4. TacetsEnigma (Ranged — Dual-Fire Bullets)

**Files:**
- [TacetsEnigma.cs](Content/EnigmaVariations/ResonantWeapons/TacetsEnigma/TacetsEnigma.cs) (673 lines)
- [TacetUtils.cs](Content/EnigmaVariations/ResonantWeapons/TacetsEnigma/Utilities/TacetUtils.cs)
- [TacetShaderLoader.cs](Content/EnigmaVariations/ResonantWeapons/TacetsEnigma/Shaders/TacetShaderLoader.cs)
- Particles/ (3 files), Primitives/ (3 files), Dusts/ (1 file)

**Rating: 🔴 WILL CRASH**

**Crash Risk 1 — CRITICAL: Wrong EN Enigma Eye Path**
- [Line 529](Content/EnigmaVariations/ResonantWeapons/TacetsEnigma/TacetsEnigma.cs#L529) (TacetParadoxBolt.PreDraw):
  ```csharp
  Texture2D enigmaEye = ModContent.Request<Texture2D>(
      "MagnumOpus/Assets/VFX Asset Library/Theme Specific/Enigma/Impact Effects/EN Enigma Eye",
      AssetRequestMode.ImmediateLoad).Value;
  ```
  Wrong path — asset is at `Particles/EN Enigma Eye.png`, not `Impact Effects/`. This runs every frame the TacetParadoxBolt (enhanced bullet) renders, which is frequent during combat.

**Crash Risk 2 — Direct `.Value` on ImmediateLoad**
- [Line 176](Content/EnigmaVariations/ResonantWeapons/TacetsEnigma/TacetsEnigma.cs#L176): `SoftRadialBloom` — path OK
- [Line 256](Content/EnigmaVariations/ResonantWeapons/TacetsEnigma/TacetsEnigma.cs#L256): `EN Star Flare` — path OK
- [Lines 414-415](Content/EnigmaVariations/ResonantWeapons/TacetsEnigma/TacetsEnigma.cs#L414-L415): `SoftRadialBloom` + `Texture` (self) — paths OK
- [Line 511](Content/EnigmaVariations/ResonantWeapons/TacetsEnigma/TacetsEnigma.cs#L511): `EN Star Flare` — path OK
- [Line 521](Content/EnigmaVariations/ResonantWeapons/TacetsEnigma/TacetsEnigma.cs#L521): `EN Power Effect Ring` — path OK

**PreDraw Return:** `return false` ✅ (TacetEnigmaShot), `return false` ✅ (TacetParadoxBolt)

**VFX Issues:**
- Per-frame `ImmediateLoad` texture fetching (no caching). Performance waste.

---

### 5. TheSilentMeasure (Ranged — Sniper/Bow with Homing Seekers)

**Files:**
- [TheSilentMeasure.cs](Content/EnigmaVariations/ResonantWeapons/TheSilentMeasure/TheSilentMeasure.cs) (856 lines)
- [SilentUtils.cs](Content/EnigmaVariations/ResonantWeapons/TheSilentMeasure/Utilities/SilentUtils.cs)
- [SilentShaderLoader.cs](Content/EnigmaVariations/ResonantWeapons/TheSilentMeasure/Shaders/SilentShaderLoader.cs)
- Particles/ (3 files), Primitives/ (3 files), Dusts/ (1 file)

**Rating: 🟠 HIGH RISK**

**Crash Risk 1 — Direct `EnigmaThemeTextures.ENxxx.Value` Without Null-Conditional**

All 6 locations use `.Value` without `?.` — crashes if `LoadTex()` returns null:

| Line | Projectile | Texture |
|------|-----------|---------|
| [191](Content/EnigmaVariations/ResonantWeapons/TheSilentMeasure/TheSilentMeasure.cs#L191) | QuestionSeekerBolt | `ENStarFlare.Value` |
| [200](Content/EnigmaVariations/ResonantWeapons/TheSilentMeasure/TheSilentMeasure.cs#L200) | QuestionSeekerBolt | `ENPowerEffectRing.Value` |
| [466](Content/EnigmaVariations/ResonantWeapons/TheSilentMeasure/TheSilentMeasure.cs#L466) | HomingQuestionSeeker | `ENStarFlare.Value` |
| [680](Content/EnigmaVariations/ResonantWeapons/TheSilentMeasure/TheSilentMeasure.cs#L680) | ParadoxPiercingBolt | `ENStarFlare.Value` |
| [691](Content/EnigmaVariations/ResonantWeapons/TheSilentMeasure/TheSilentMeasure.cs#L691) | ParadoxPiercingBolt | `ENPowerEffectRing.Value` |
| [700](Content/EnigmaVariations/ResonantWeapons/TheSilentMeasure/TheSilentMeasure.cs#L700) | ParadoxPiercingBolt | `ENEnigmaEye.Value` |

**Currently safe** because assets exist. Would crash instantly if any asset file is renamed/deleted.

**Crash Risk 2 — Bare `catch {}` Blocks (Silent Exception Swallowing)**
- [Line 159](Content/EnigmaVariations/ResonantWeapons/TheSilentMeasure/TheSilentMeasure.cs#L159): QuestionSeekerBolt GPU primitive section
- [Line 434](Content/EnigmaVariations/ResonantWeapons/TheSilentMeasure/TheSilentMeasure.cs#L434): HomingQuestionSeeker GPU primitive section
- [Line 648](Content/EnigmaVariations/ResonantWeapons/TheSilentMeasure/TheSilentMeasure.cs#L648): ParadoxPiercingBolt GPU primitive section

These `catch {}` blocks wrap `sb.End()` + GPU primitive rendering + `sb.Begin()`. If an exception occurs inside (shader failure, device lost, vertex buffer disposed), it is **silently swallowed**. The SpriteBatch then enters the additive Begin without having properly ended the previous state, which can cascade into further errors.

**PreDraw Return:** `return false` ✅ (all 3 projectiles)

**VFX Issues:** None beyond the above.

---

### 6. TheUnresolvedCadence (Melee — Ultimate Broadsword)

**Files:**
- [TheUnresolvedCadence.cs](Content/EnigmaVariations/ResonantWeapons/TheUnresolvedCadence/TheUnresolvedCadence.cs) (projectiles: DimensionalSlash, ParadoxCollapseUltimate)
- [TheUnresolvedCadenceItem.cs](Content/EnigmaVariations/ResonantWeapons/TheUnresolvedCadence/TheUnresolvedCadenceItem.cs) (item definition)
- [TheUnresolvedCadenceSwing.cs](Content/EnigmaVariations/ResonantWeapons/TheUnresolvedCadence/TheUnresolvedCadenceSwing.cs) (667 lines, swing projectile)
- [CadenceUtils.cs](Content/EnigmaVariations/ResonantWeapons/TheUnresolvedCadence/Utilities/CadenceUtils.cs)
- [CadenceShaderLoader.cs](Content/EnigmaVariations/ResonantWeapons/TheUnresolvedCadence/Shaders/CadenceShaderLoader.cs)
- Particles/ (3 files), Primitives/ (3 files), Dusts/ (1 file)

**Rating: ✅ LOW RISK**

**EnigmaThemeTextures in Swing File — SAFE Pattern:**
- [Line 470](Content/EnigmaVariations/ResonantWeapons/TheUnresolvedCadence/TheUnresolvedCadenceSwing.cs#L470): `EnigmaThemeTextures.ENStarFlare?.Value` ✅
- [Line 484](Content/EnigmaVariations/ResonantWeapons/TheUnresolvedCadence/TheUnresolvedCadenceSwing.cs#L484): `EnigmaThemeTextures.ENPowerEffectRing?.Value` ✅
- [Line 499](Content/EnigmaVariations/ResonantWeapons/TheUnresolvedCadence/TheUnresolvedCadenceSwing.cs#L499): `EnigmaThemeTextures.ENEnigmaEye?.Value` ✅

**Direct `.Value` on ImmediateLoad in Projectile File — Paths Correct:**
- [Lines 49, 75-76](Content/EnigmaVariations/ResonantWeapons/TheUnresolvedCadence/TheUnresolvedCadence.cs#L49) (DimensionalSlash): `SoftRadialBloom`, `EN Star Flare` — both paths valid
- [Lines 246, 259, 261-262](Content/EnigmaVariations/ResonantWeapons/TheUnresolvedCadence/TheUnresolvedCadence.cs#L246) (ParadoxCollapseUltimate): `SoftRadialBloom`, `EN Star Flare`, `EN Power Effect Ring` — all paths valid

**No `catch {}` blocks.**

**PreDraw Return:** `return false` ✅ (DimensionalSlash, ParadoxCollapseUltimate)

**Minor Concerns:**
- Static `inevitabilityStacks` field in TheUnresolvedCadenceItem — persists across worlds/sessions. Not a crash, but could carry state between worlds.
- Per-frame `ImmediateLoad` in projectile file PreDraw — performance concern.

---

### 7. TheWatchingRefrain (Summoner — Phantom Minion + Zone Control)

**Files:**
- [TheWatchingRefrain.cs](Content/EnigmaVariations/ResonantWeapons/TheWatchingRefrain/TheWatchingRefrain.cs) (1133 lines — largest file)
- [WatchingUtils.cs](Content/EnigmaVariations/ResonantWeapons/TheWatchingRefrain/Utilities/WatchingUtils.cs)
- [WatchingShaderLoader.cs](Content/EnigmaVariations/ResonantWeapons/TheWatchingRefrain/Shaders/WatchingShaderLoader.cs)
- Particles/ (3 files), Primitives/ (3 files), Dusts/ (1 file)

Contains: TheWatchingRefrain (ModItem), UnsolvedPhantomBuff, UnsolvedPhantomMinion, PhantomBolt, PhantomRift, MysteryZone — 6 classes

**Rating: 🟠 HIGH RISK**

**Crash Risk 1 — Direct `EnigmaThemeTextures.ENxxx.Value` Without Null-Conditional**

**11 locations** use `.Value` without `?.` — the MOST of any weapon:

| Line | Class | Texture |
|------|-------|---------|
| [205](Content/EnigmaVariations/ResonantWeapons/TheWatchingRefrain/TheWatchingRefrain.cs#L205) | UnsolvedPhantomMinion | `ENStarFlare.Value` |
| [216](Content/EnigmaVariations/ResonantWeapons/TheWatchingRefrain/TheWatchingRefrain.cs#L216) | UnsolvedPhantomMinion | `ENPowerEffectRing.Value` |
| [225](Content/EnigmaVariations/ResonantWeapons/TheWatchingRefrain/TheWatchingRefrain.cs#L225) | UnsolvedPhantomMinion | `ENEnigmaEye.Value` |
| [508](Content/EnigmaVariations/ResonantWeapons/TheWatchingRefrain/TheWatchingRefrain.cs#L508) | PhantomBolt | `ENStarFlare.Value` |
| [519](Content/EnigmaVariations/ResonantWeapons/TheWatchingRefrain/TheWatchingRefrain.cs#L519) | PhantomBolt | `ENPowerEffectRing.Value` |
| [743](Content/EnigmaVariations/ResonantWeapons/TheWatchingRefrain/TheWatchingRefrain.cs#L743) | PhantomRift | `ENStarFlare.Value` |
| [754](Content/EnigmaVariations/ResonantWeapons/TheWatchingRefrain/TheWatchingRefrain.cs#L754) | PhantomRift | `ENPowerEffectRing.Value` |
| [763](Content/EnigmaVariations/ResonantWeapons/TheWatchingRefrain/TheWatchingRefrain.cs#L763) | PhantomRift | `ENEnigmaEye.Value` |
| [975](Content/EnigmaVariations/ResonantWeapons/TheWatchingRefrain/TheWatchingRefrain.cs#L975) | MysteryZone | `ENPowerEffectRing.Value` |
| [985](Content/EnigmaVariations/ResonantWeapons/TheWatchingRefrain/TheWatchingRefrain.cs#L985) | MysteryZone | `ENStarFlare.Value` |
| [993](Content/EnigmaVariations/ResonantWeapons/TheWatchingRefrain/TheWatchingRefrain.cs#L993) | MysteryZone | `ENEnigmaEye.Value` |

**Currently safe** because assets exist. Would crash at 11 different render paths if any asset is missing.

**Crash Risk 2 — Bare `catch {}` Blocks**
- [Line 172](Content/EnigmaVariations/ResonantWeapons/TheWatchingRefrain/TheWatchingRefrain.cs#L172): UnsolvedPhantomMinion GPU primitives
- [Line 487](Content/EnigmaVariations/ResonantWeapons/TheWatchingRefrain/TheWatchingRefrain.cs#L487): PhantomBolt GPU primitives
- [Line 721](Content/EnigmaVariations/ResonantWeapons/TheWatchingRefrain/TheWatchingRefrain.cs#L721): PhantomRift GPU primitives
- [Line 942](Content/EnigmaVariations/ResonantWeapons/TheWatchingRefrain/TheWatchingRefrain.cs#L942): MysteryZone GPU primitives

Same silent-exception pattern as TheSilentMeasure. Swallows shader/GPU failures without recovery.

**PreDraw Return:** `return false` ✅ (all 4 projectile classes)

**VFX Issues:**
- Per-frame `ImmediateLoad` for `SoftRadialBloom` texture in all 4 projectiles — should use a cached reference.
- MysteryZone's `DealZoneDamage()` uses `SimpleStrikeNPC` which bypasses i-frame systems — intentional but aggressive.

---

### 8. VariationsOfTheVoid (Melee — Tri-Beam Convergence Sword)

**Files:**
- [VariationsOfTheVoid.cs](Content/EnigmaVariations/ResonantWeapons/VariationsOfTheVoid/VariationsOfTheVoid.cs) (619 lines — VoidConvergenceBeamSet, VoidResonanceExplosion)
- [VariationsOfTheVoidItem.cs](Content/EnigmaVariations/ResonantWeapons/VariationsOfTheVoid/VariationsOfTheVoidItem.cs) (item definition)
- [VariationsOfTheVoidSwing.cs](Content/EnigmaVariations/ResonantWeapons/VariationsOfTheVoid/VariationsOfTheVoidSwing.cs) (658 lines, swing projectile)
- [VoidVariationUtils.cs](Content/EnigmaVariations/ResonantWeapons/VariationsOfTheVoid/Utilities/VoidVariationUtils.cs)
- [VoidVariationShaderLoader.cs](Content/EnigmaVariations/ResonantWeapons/VariationsOfTheVoid/Shaders/VoidVariationShaderLoader.cs)
- Particles/ (3 files), Primitives/ (3 files), Dusts/ (1 file)

**Rating: ⚠️ MEDIUM RISK**

**EnigmaThemeTextures in Swing File — SAFE Pattern:**
- [Line 464](Content/EnigmaVariations/ResonantWeapons/VariationsOfTheVoid/VariationsOfTheVoidSwing.cs#L464): `EnigmaThemeTextures.ENStarFlare?.Value` ✅
- [Line 478](Content/EnigmaVariations/ResonantWeapons/VariationsOfTheVoid/VariationsOfTheVoidSwing.cs#L478): `EnigmaThemeTextures.ENPowerEffectRing?.Value` ✅
- [Line 493](Content/EnigmaVariations/ResonantWeapons/VariationsOfTheVoid/VariationsOfTheVoidSwing.cs#L493): `EnigmaThemeTextures.ENEnigmaEye?.Value` ✅

**Crash Risk 1 — LFTextures Direct `.Value` Without Protection**

VoidConvergenceBeamSet.PreDraw accesses `LFTextures` (LaserFoundation texture registry) assets directly. These are loaded as `static readonly` with `ImmediateLoad` at class initialization:

| Line | Texture |
|------|---------|
| [69](Content/EnigmaVariations/ResonantWeapons/VariationsOfTheVoid/VariationsOfTheVoid.cs#L69) | `LFTextures.BeamAlphaMask.Value` |
| [70](Content/EnigmaVariations/ResonantWeapons/VariationsOfTheVoid/VariationsOfTheVoid.cs#L70) | `LFTextures.GradEnigma.Value` |
| [74-77](Content/EnigmaVariations/ResonantWeapons/VariationsOfTheVoid/VariationsOfTheVoid.cs#L74-L77) | `LFTextures.DetailThinGlowLine.Value`, `.DetailSpark.Value`, `.DetailExtra.Value`, `.DetailTrailLoop.Value` |
| [143-146](Content/EnigmaVariations/ResonantWeapons/VariationsOfTheVoid/VariationsOfTheVoid.cs#L143-L146) | `LFTextures.LensFlare.Value`, `.StarFlare.Value`, `.GlowOrb.Value`, `.SoftGlow.Value` |

If any LFTextures asset path is wrong, the `LFTextures` class will fail to initialize (static constructor exception), which would crash every weapon that uses LaserFoundation — not just this one. Unlikely but catastrophic.

**Crash Risk 2 — Shader Parameter Access Without Null Check**

VoidConvergenceBeamSet.PreDraw loads a beam shader and accesses parameters directly:
```csharp
beamShader.Parameters["WorldViewProjection"].SetValue(...);
beamShader.Parameters["onTex"].SetValue(LFTextures.BeamAlphaMask.Value);
```
The shader is loaded inline (`ModContent.Request<Effect>(...).Value`), so if the shader asset is missing, `.Value` would fail. Also, if any parameter name doesn't match the shader's declared parameters, `.Parameters["xxx"]` returns null and `.SetValue()` throws `NullReferenceException`.

**PreDraw Return:** `return false` ✅ (VoidConvergenceBeamSet, VoidResonanceExplosion)

**VFX Issues:** None significant.

---

## SHARED INFRASTRUCTURE FILES

### EnigmaThemeTextures.cs
- [File](Content/EnigmaVariations/EnigmaThemeTextures.cs) — Texture registry
- `LoadTex()` returns null for missing assets ← **ROOT CAUSE of null-safety issues**
- All property getters use lazy initialization with `??=`
- **Recommendation**: Add a null-guard wrapper method or change `LoadTex()` to throw rather than return null silently.

### EnigmaVFXLibrary.cs
- Uses SAFE `?.Value` pattern for all `EnigmaThemeTextures` access ✅
- Uses `MagnumTextureRegistry.GetBloom()` with null check ✅
- `AddPulsingLight` is safe (just calls `Lighting.AddLight`)

### EnigmaShaderHelper.cs
- `DrawShaderOverlay` has proper null guards: `if (shader == null || !ShaderLoader.ShadersEnabled || drawTexture == null) return;` ✅
- Has proper try/catch with SpriteBatch recovery in the catch block ✅
- This is the GOLD STANDARD for how all shader-drawing code should work.

### All 8 Utility Files (*Utils.cs)
- All follow identical safe patterns: palette definitions, easing functions, SpriteBatch helpers
- `EnterAdditiveShaderRegion()`, `ExitShaderRegion()` — clean Begin/End pairs ✅
- `DrawThemeAccents()` delegates to `EnigmaVFXLibrary` which has null guards ✅
- No crash risks in any utility file.

### All 8 Shader Loaders (*ShaderLoader.cs)
- Use `Mod.Assets.Request<Effect>(path, ImmediateLoad)` — will crash if shader .fxc file is missing
- Register into `GameShaders.Misc` dictionary — standard tModLoader pattern
- Low risk as long as compiled shader files exist.

### All 8 Primitive Renderers (*PrimitiveRenderer.cs)
- All have proper null guards: `if (_vertexBuffer == null || positions == null || positions.Count < 2) return;` ✅
- Proper `Main.QueueMainThreadAction` for GPU resource creation/disposal ✅
- MaxVertices/MaxIndices bounds checking ✅
- No crash risks in renderers themselves.

---

## FIX PRIORITY LIST

### Priority 1 — WILL CRASH (Fix Immediately)

1. **Fix EN Enigma Eye path** in 3 weapons — change `Impact Effects/EN Enigma Eye` to `Particles/EN Enigma Eye`:
   - [DissonanceOfSecrets.cs L245](Content/EnigmaVariations/ResonantWeapons/DissonanceOfSecrets/DissonanceOfSecrets.cs#L245)
   - [FugueOfTheUnknown.cs L345](Content/EnigmaVariations/ResonantWeapons/FugueOfTheUnknown/FugueOfTheUnknown.cs#L345)
   - [TacetsEnigma.cs L529](Content/EnigmaVariations/ResonantWeapons/TacetsEnigma/TacetsEnigma.cs#L529)

2. **Remove SpriteBatch.Draw calls from OnHitNPC** in FugueOfTheUnknown (lines 571-586). Move these draws to a particle system or flag-based PreDraw approach that renders during the draw phase.

### Priority 2 — HIGH RISK (Fix Soon)

3. **Add null-conditional `?.` to all `EnigmaThemeTextures` accesses** in:
   - TheSilentMeasure.cs — 6 locations (lines 191, 200, 466, 680, 691, 700)
   - TheWatchingRefrain.cs — 11 locations (lines 205, 216, 225, 508, 519, 743, 754, 763, 975, 985, 993)
   - Pattern: Change `EnigmaThemeTextures.ENStarFlare.Value` → `EnigmaThemeTextures.ENStarFlare?.Value` with null check before drawing.

4. **Replace bare `catch {}` blocks** with at least `catch { sb.End(); }` + re-Begin, or adopt the `EnigmaShaderHelper.DrawShaderOverlay` recovery pattern:
   - TheSilentMeasure.cs — 3 locations (lines 159, 434, 648)
   - TheWatchingRefrain.cs — 4 locations (lines 172, 487, 721, 942)

### Priority 3 — Code Quality (When Convenient)

5. **Replace per-frame `ModContent.Request<Texture2D>(..., ImmediateLoad).Value` with `EnigmaThemeTextures` cached properties** (or field-level caching) across all weapons. Every `ImmediateLoad` per frame does a dictionary lookup + potential disk IO — wasteful when called 60 times/second.

6. **Consider making `EnigmaThemeTextures.LoadTex()` throw** instead of returning null. If an asset is missing, it's a build/deployment error that should fail loudly at load time, not silently at render time.

7. **Static `inevitabilityStacks` in TheUnresolvedCadenceItem** — add a `ModSystem.OnWorldUnload` hook to reset this between worlds.
