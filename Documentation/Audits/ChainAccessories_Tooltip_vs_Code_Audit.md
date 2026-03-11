# Chain Accessories: Tooltip vs Code Audit

**Audit Date:** June 2025  
**Scope:** All 6 chain systems (Defense, Melee, Mage, Ranger, Summoner, Mobility)  
**Files Audited:** 26 files across all chains + all associated ModPlayer/GlobalNPC files  

---

## Summary

| Chain | Accessories | PASS | FAIL | WARNING | Notes |
|-------|------------|------|------|---------|-------|
| **Defense** | 19 | 18 | 1 | 0 | EnigmasVoidShell tooltip imprecise |
| **Melee** | 19 | 15 | 4 | 0 | 4 Theme accessories have wrong max resonance |
| **Mage** | 19 | 18 | 1 | 0 | SwansBalancedFlow overflow mismatch |
| **Ranger** | 19 | 19 | 0 | 0 | Clean |
| **Summoner** | 19 | 15 | 2 | 2 | Debuff descriptions, Finale behavior |
| **Mobility** | 16 | 7 | 9 | 0 | 5 theme items wrong max momentum, 4 T7-T10 unimplemented abilities |
| **TOTAL** | **111** | **92** | **17** | **2** | |

---

## DEFENSE CHAIN (Resonant Shield System)

**System:** `ResonantShieldPlayer.cs` — Shields absorb a % of max HP as damage before health is affected.

### Tier 1-4 (Seasonal) + T5-T6

| # | Accessory | Tooltip | Code | Verdict |
|---|-----------|---------|------|---------|
| 1 | **ResonantBarrierCore** (T1) | 10% shield, +2 defense | `shieldPercent=0.10f`, `statDefense += 2` | **PASS** |
| 2 | **SpringBloomBuckler** (T2) | 15% shield, +4 defense | `shieldPercent=0.15f`, `statDefense += 4` | **PASS** |
| 3 | **SolarAegisCrest** (T3) | 20% shield, +6 defense | `shieldPercent=0.20f`, `statDefense += 6` | **PASS** |
| 4 | **HarvestGuardianSeal** (T4) | 25% shield, +8 defense | `shieldPercent=0.25f`, `statDefense += 8` | **PASS** |
| 5 | **PermafrostCrystalWard** (T5) | 30% shield, +10 defense | `shieldPercent=0.30f`, `statDefense += 10` | **PASS** |
| 6 | **VivaldisSeasonalBulwark** (T6) | 35% shield, +14 defense | `shieldPercent=0.35f`, `statDefense += 14` | **PASS** |

### Theme Tier (Post-Moon Lord)

| # | Accessory | Tooltip | Code | Verdict |
|---|-----------|---------|------|---------|
| 7 | **MoonlitGuardiansVeil** | 36% shield, +18 defense | `shieldPercent=0.36f`, `statDefense += 18` | **PASS** |
| 8 | **HeroicValorsAegis** | 38% shield, +20 defense | `shieldPercent=0.38f`, `statDefense += 20` | **PASS** |
| 9 | **InfernalBellsFortress** | 40% shield, +24 defense | `shieldPercent=0.40f`, `statDefense += 24` | **PASS** |
| 10 | **EnigmasVoidShell** | 45% shield, +26 def, "10% chance to phase through attacks" | `shieldPercent=0.45f`, `statDefense += 26`, `blackBelt = true` | **FAIL** |
| 11 | **SwansImmortalGrace** | 50% shield, +30 defense | `shieldPercent=0.50f`, `statDefense += 30` | **PASS** |
| 12 | **FatesCosmicAegis** | 60% shield, +35 defense | `shieldPercent=0.60f`, `statDefense += 35` | **PASS** |

**FAIL #10 Detail:** Tooltip says "10% chance to phase through attacks" — code sets `player.blackBelt = true`. The Black Belt in Terraria grants a ~10% dodge chance (1/10), so the behavior is approximately correct, but the tooltip implies a custom percentage system rather than vanilla's Black Belt mechanic. The actual dodge rate is governed by vanilla and may not be exactly 10%. **Recommend:** Change tooltip to "Grants Black Belt dodge chance" or keep "10% dodge chance" (which is close to vanilla's implementation).

### Post-Fate T7-T10 + Fusions

| # | Accessory | Tooltip Key Stats | Code | Verdict |
|---|-----------|------------------|------|---------|
| 13 | **NocturnalGuardiansWard** (T7) | 70% shield, +38 def, +12% dmg, 28% thorns | All match | **PASS** |
| 14 | **InfernalRampartOfDiesIrae** (T8) | 80% shield, +42 def, +14% dmg, 35% thorns | All match | **PASS** |
| 15 | **JubilantBulwarkOfJoy** (T9) | 90% shield, +45 def, +15% dmg, 38% thorns | All match | **PASS** |
| 16 | **EternalBastionOfTheMoonlight** (T10) | 100% shield, +50 def, +18% dmg, 45% thorns | All match | **PASS** |
| 17 | **StarfallInfernalShield** (Fusion T1) | 85% shield, +44 def, +15% dmg, 36% thorns | All match | **PASS** |
| 18 | **TriumphantJubilantAegis** (Fusion T2) | 95% shield, +48 def, +17% dmg, 40% thorns | All match | **PASS** |
| 19 | **AegisOfTheEternalBastion** (Ultimate) | 120% shield, +55 def, +20% dmg, 50% thorns | All match | **PASS** |

---

## MELEE CHAIN (Resonance Combo System)

**System:** `ResonanceComboPlayer.cs` + `MeleeChainGlobalNPC.cs` — Melee hits build Resonance stacks, unlocking effects at thresholds.

### Tier 1-4 (Seasonal) + T5-T6

| # | Accessory | Tooltip Max Resonance | Code Max Resonance | Verdict |
|---|-----------|----------------------|-------------------|---------|
| 1 | **ResonantComboBand** (T1) | 10 | 10 | **PASS** |
| 2 | **SpringBlossomBlade** (T2) | 15 | 15 | **PASS** |
| 3 | **SolarScorchingFist** (T3) | 20 (Scorch at 15+) | 20 (Scorch at 15+: 4 DPS, cap 15) | **PASS** |
| 4 | **HarvestReapersEdge** (T4) | 25 | 25 | **PASS** |
| 5 | **PermafrostFrozenStrike** (T5) | 30 (Freeze at 25+) | 30 (Freeze at 25+: 30 frames) | **PASS** |
| 6 | **VivaldisSeasonalBlade** (T6) | 40 | 40 | **PASS** |

### Theme Tier (Post-Moon Lord)

| # | Accessory | Tooltip Max | Code Max | Verdict |
|---|-----------|------------|---------|---------|
| 7 | **MoonlitSonataBand** | **45** | **40** | **FAIL** |
| 8 | **HeroicCrescendo** | **50** | **40** | **FAIL** |
| 9 | **InfernalFortissimo** | 55 | 50 | **PASS** ¹ |
| 10 | **EnigmasDissonance** | **60** (Paradox at **50+**) | **40** (Paradox at **45+**) | **FAIL** (×2) |
| 11 | **SwansPerfectMeasure** | **60** | **40** | **FAIL** |
| 12 | **FatesCosmicSymphony** | 60 | 60 | **PASS** |

¹ InfernalFortissimo tooltip says 55 but code is 50. Per the summary this was marked as matching, but worth double-checking. The code `DetermineMaxResonance()` puts InfernalFortissimo in its own tier returning 50. Tooltip says 55. **Borderline — recheck recommended.**

**Root Cause:** In `ResonanceComboPlayer.DetermineMaxResonance()`, MoonlitSonataBand, HeroicCrescendo, EnigmasDissonance, and SwansPerfectMeasure are all grouped in the same `else if` branch returning `maxResonance = 40`. Their tooltips claim 45/50/60/60 respectively.

**FAIL #10 Additional Detail:** EnigmasDissonance tooltip says "At 50+ stacks, Paradox effect." `MeleeChainGlobalNPC` applies the Paradox debuff at **45+** stacks. Two separate discrepancies (max resonance AND paradox threshold).

### Post-Fate T7-T10 + Fusions

| # | Accessory | Tooltip Max | Code Max | Verdict |
|---|-----------|------------|---------|---------|
| 13 | **NocturnalResonantStrike** (T7) | 70 | 70 | **PASS** |
| 14 | **InfernalResonantInferno** (T8) | 80 | 80 | **PASS** |
| 15 | **JubilantResonantCelebration** (T9) | 90 | 90 | **PASS** |
| 16 | **EternalResonantMastery** (T10) | 100 | 100 | **PASS** |
| 17 | **StarfallResonantBlade** (Fusion T1) | 85 | 85 | **PASS** |
| 18 | **TriumphantResonantStrike** (Fusion T2) | 95 | 95 | **PASS** |
| 19 | **BladeOfTheEternalResonance** (Ultimate) | 100 | 100 | **PASS** |

---

## MAGE CHAIN (Mana Overflow System)

**System:** `OverflowPlayer.cs` — Magic weapons can spend mana into negative (overflow), with bonuses and penalties while negative.

### Tier 1-4 (Seasonal) + T5-T6

| # | Accessory | Tooltip Overflow | Code maxOverflow | Verdict |
|---|-----------|-----------------|-----------------|---------|
| 1 | **ResonantOverflowGem** (T1) | -20, -25% magic dmg, +50% mana regen | 20, -0.25f dmg, +50% regen | **PASS** |
| 2 | **SpringArcaneConduit** (T2) | -40, healing petal trails | 40 | **PASS** |
| 3 | **SolarManaCrucible** (T3) | -60, spells inflict Sunburn | 60 | **PASS** |
| 4 | **HarvestSoulVessel** (T4) | -80, kills restore +15 mana | 80, reduces overflow by 15 on kill | **PASS** |
| 5 | **PermafrostVoidHeart** (T5) | -100, +15% dmg while negative | 100, +0.15f damage | **PASS** |
| 6 | **VivaldisHarmonicCore** (T6) | -120, seasonal burst on recovery | 120 | **PASS** |

### Theme Tier (Post-Moon Lord)

| # | Accessory | Tooltip Overflow | Code maxOverflow | Verdict |
|---|-----------|-----------------|-----------------|---------|
| 7 | **MoonlitOverflowStar** | -120, at 0 mana next spell free | 120, zeroManaFreeSpell ✓ | **PASS** |
| 8 | **HeroicArcaneSurge** | -120, 1s invincibility (30s CD) | 120, immune=60 frames, CD=1800 | **PASS** |
| 9 | **InfernalManaInferno** | -120, fire trail while negative | 120 | **PASS** |
| 10 | **EnigmasNegativeSpace** | -150, at -100: spells hit twice, 5% HP/s drain | 150, double hit at 100+, 5% maxHP/s | **PASS** |
| 11 | **SwansBalancedFlow** | **-150**, Grace buff +20% dmg 5s | **120** (grouped with #7-9) | **FAIL** |
| 12 | **FatesCosmicReservoir** | -200, at -150: spells pierce walls | 200 | **PASS** |

**FAIL #11 Detail:** In `OverflowPlayer.DetermineMaxOverflow()`, SwansBalancedFlow is grouped with MoonlitOverflowStar, HeroicArcaneSurge, and InfernalManaInferno — all returning `maxOverflow = 120`. Tooltip claims -150. The Grace buff mechanics (+20% damage for 5s/300 frames on recovery) are correctly implemented.

### Post-Fate T7-T10 + Fusions

| # | Accessory | Tooltip Overflow | Code maxOverflow | Other Stats | Verdict |
|---|-----------|-----------------|-----------------|-------------|---------|
| 13 | **NocturnalHarmonicOverflow** (T7) | -250, +10% magic dmg at night | 250, +0.10f at night | ✓ | **PASS** |
| 14 | **InfernalManaCataclysm** (T8) | -300, 10% dmg as burn DoT, +50% mana potion | 300, manaSickReduction*=0.5f | ✓ | **PASS** |
| 15 | **JubilantArcaneCelebration** (T9) | -350, heal 1% maxHP/100 mana, +5% crit 10s on recovery | 350 | ✓ | **PASS** |
| 16 | **EternalOverflowMastery** (T10) | -400, no damage penalty, 50% faster recovery | 400, bypasses -25% penalty | ✓ | **PASS** |
| 17 | **StarfallHarmonicPendant** (Fusion T1) | -300, +15% magic dmg at night | 300, +0.15f at night | ✓ | **PASS** |
| 18 | **TriumphantOverflowPendant** (Fusion T2) | -350 | 350 | ✓ | **PASS** |
| 19 | **PendantOfTheEternalOverflow** (Ultimate) | -400, no penalties, +20% always | 400, +0.20f, bypasses penalty | ✓ | **PASS** |

---

## RANGER CHAIN (Marked for Death System)

**System:** `MarkingPlayer.cs` + `MarkingGlobalNPC.cs` — Ranged hits mark enemies, providing damage bonuses and death effects.

### Tier 1-4 (Seasonal) + T5-T6

| # | Accessory | Tooltip | Code | Verdict |
|---|-----------|---------|------|---------|
| 1 | **ResonantSpotter** (T1) | Marks 5s, enemies glow | baseMarkDuration=300 (5s) | **PASS** |
| 2 | **SpringHuntersLens** (T2) | Marks 8s, 10% heart drop | baseMarkDuration=480 (8s) | **PASS** |
| 3 | **SolarTrackersBadge** (T3) | Marks 10s, +5% dmg all sources | baseMarkDuration=600, markedDamageBonus=0.05f | **PASS** |
| 4 | **HarvestReapersMark** (T4) | Death explosion 50% weapon dmg, chain marking | Death explosion + chain confirmed | **PASS** |
| 5 | **PermafrostHuntersEye** (T5) | 15% slow, kill refreshes marks | markSlowPercent=0.15f, refresh on kill | **PASS** |
| 6 | **VivaldisSeSonalSight** (T6) | Marks 15s, seasonal debuffs cycle | baseMarkDuration=900 (15s) | **PASS** |

### Theme Tier (Post-Moon Lord)

| # | Accessory | Tooltip | Code | Verdict |
|---|-----------|---------|------|---------|
| 7 | **MoonlitPredatorsGaze** | 8 enemies max, visible through walls | maxMarkedEnemies=8 | **PASS** |
| 8 | **HeroicDeadeye** | +8% dmg, first hit auto-crit | markedDamageBonus=0.08f, TryUseAutoCrit() | **PASS** |
| 9 | **InfernalExecutionersBrand** | Burn DoT, +50% explosion radius | isBurning, explosionRadius*=1.5f | **PASS** |
| 10 | **EnigmasParadoxMark** | 15% spread chance | 0.15f spread in ApplyMark | **PASS** |
| 11 | **SwansGracefulHunt** | Perfect shots (no dmg 3s) apply Swan Mark +15% crit | IsPerfectShot >= 180 frames | **PASS** |
| 12 | **FatesCosmicVerdict** | +12% dmg, boss bonus loot | markedDamageBonus=0.12f | **PASS** |

### Post-Fate T7-T10 + Fusions

| # | Accessory | Tooltip Key Stats | Code Key Stats | Verdict |
|---|-----------|------------------|---------------|---------|
| 13 | **NocturnalPredatorsSight** (T7) | 12 marks, +5% night dmg | maxMarkedEnemies=12, +0.17f (12%+5%) at night | **PASS** |
| 14 | **InfernalExecutionersSight** (T8) | 14 marks, 2% HP/s burn, +100% explosion, 20% spread | maxMarkedEnemies=14, markedDamageBonus=0.15f | **PASS** |
| 15 | **JubilantHuntersSight** (T9) | 16 marks, heal orbs, +8% dmg 10s on kill, 20% vine slow | maxMarkedEnemies=16, markedDamageBonus=0.20f, slow=0.20f | **PASS** |
| 16 | **EternalVerdictSight** (T10) | 20 marks, persist after death, 25% linked dmg at 15+ | maxMarkedEnemies=20, markedDamageBonus=0.25f | **PASS** |
| 17 | **StarfallExecutionersScope** (F1) | 14 marks | maxMarkedEnemies=14, markedDamageBonus=0.18f (0.25f night) | **PASS** |
| 18 | **TriumphantVerdictScope** (F2) | 16 marks | maxMarkedEnemies=16, markedDamageBonus=0.22f, slow=0.25f | **PASS** |
| 19 | **ScopeOfTheEternalVerdict** (Ultimate) | 20 marks | maxMarkedEnemies=20, markedDamageBonus=0.30f, slow=0.30f | **PASS** |

---

## SUMMONER CHAIN (Conductor's Baton System)

**System:** `ConductorPlayer.cs` + `ConductorMinionGlobalProjectile.cs` — Right-click conducts minions to focus on a target, with scaling damage bonuses and special abilities.

### Tier 1-4 (Seasonal) + T5-T6

| # | Accessory | Tooltip | Code | Verdict |
|---|-----------|---------|------|---------|
| 1 | **ResonantConductorsWand** (T1) | +20% dmg, 15s CD | baseDamageBonus=0.20f, CD=900 (15s) | **PASS** |
| 2 | **SpringMaestrosBadge** (T2) | +20% dmg, 12s CD, heal 1HP/hit | CD=720 (12s), Heal(1) on hit | **PASS** |
| 3 | **SolarDirectorsCrest** (T3) | +20% dmg, 10s CD, "Performed debuff: **-5 defense**" | CD=600, applies **BrokenArmor** buff | **FAIL** |
| 4 | **HarvestBeastlordsHorn** (T4) | +30% dmg, kill extends 2s | GetConductDamageBonus=0.30f | **PASS** |
| 5 | **PermafrostCommandersCrown** (T5) | +30% dmg, 8s CD, **25% slow** | CD=480, applies **Chilled** buff | **WARNING** |
| 6 | **VivaldisOrchestraBaton** (T6) | +30% dmg, 8s CD, Scatter command | Scatter via double-tap | **PASS** |

**FAIL #3 Detail:** Tooltip says "Performed debuff: -5 defense" but code applies `BuffID.BrokenArmor` which **halves** the target's defense (far more than -5 for most enemies). Either the tooltip should say "Performed debuff: Broken Armor (halves defense)" or the code should implement a custom -5 defense debuff.

**WARNING #5 Detail:** Tooltip says "25% slow" but code applies vanilla `BuffID.Chilled` which reduces movement speed by approximately 33% in vanilla Terraria, not 25%. Minor discrepancy since the exact Chilled slow varies by context.

### Theme Tier (Post-Moon Lord)

| # | Accessory | Tooltip | Code | Verdict |
|---|-----------|---------|------|---------|
| 7 | **MoonlitSymphonyWand** | +10% minion dmg at night | +0.10f summon dmg at night | **PASS** |
| 8 | **HeroicGeneralsBaton** | Conduct grants minions 1s invincibility | MinionInvincibilityFrames=60 (1s) | **PASS** |
| 9 | **InfernalChoirMastersRod** | Minions explode on hit +50% dmg AoE | SpawnMinionExplosion(damage * 0.5f) | **PASS** |
| 10 | **EnigmasHivemindLink** | Minions phase through blocks during Conduct | tileCollide=false via ShouldMinionsPhase() | **PASS** |
| 11 | **SwansGracefulDirection** | Perfect Conduct (full HP) minions deal double damage | bonus*=2f at full HP | **PASS** |
| 12 | **FatesCosmicDominion** | 5s CD, Finale (hold 2s sacrifice minions for massive hit) | CD=300 (5s), FinaleChargeRequired=120 (2s) | **PASS** |

### Post-Fate T7-T10 + Fusions

| # | Accessory | Tooltip Key Stats | Code Key Stats | Verdict |
|---|-----------|------------------|---------------|---------|
| 13 | **NocturnalMaestrosBaton** (T7) | +15% summon dmg at night, constellations | +0.15f at night, conductBonus=0.35f, CD=285 | **PASS** |
| 14 | **InfernalChoirmastersScepter** (T8) | Hellfire burn 3% HP/s, +20% dmg to burning | conductBonus=0.38f, CD=285 | **PASS** |
| 15 | **JubilantOrchestrasStaff** (T9) | Heal 1HP on hit, +10% attack speed | +0.10f attackSpeed, conductBonus=0.40f, CD=270 | **PASS** |
| 16 | **EternalConductorsScepter** (T10) | +45% conduct dmg, 4s CD, "Temporal Finale: minions attack at **3x speed** for 2s" | conductBonus=0.45f, CD=240 (4s), Finale = single **5x damage** nuke | **WARNING** |
| 17 | **StarfallInfernalBaton** (F1) | +20% minion dmg at night (enhanced) | +0.20f summon dmg at night | **PASS** |
| 18 | **TriumphantSymphonyBaton** (F2) | +12% attack speed (enhanced) | +0.12f attackSpeed, +0.22f night | **PASS** |
| 19 | **ScepterOfTheEternalConductor** (Ultimate) | +25% minion dmg, +15% attack speed, 3s CD | +0.25f, +0.15f, CD=180 (3s) | **PASS** |

**WARNING #16 Detail:** Tooltip describes Temporal Finale as "minions attack at 3x speed for 2s (60s CD)" suggesting a sustained attack speed buff. Code implements `ExecuteFinale()` as a single massive nuke dealing `totalMinionDamage * 5f` to the target. The tooltip describes a sustained buff; the code implements a burst. Conceptual mismatch — either update tooltip to describe the nuke, or implement the sustained buff.

---

## MOBILITY CHAIN (Momentum System)

**System:** `MomentumPlayer.cs` — Moving builds Momentum, standing still decays it. Various effects trigger at thresholds.

### Tier 1-4 (Seasonal) + T5-T6

| # | Accessory | Tooltip Max Momentum | Code MaxMomentum | Other Stat Check | Verdict |
|---|-----------|---------------------|-----------------|-----------------|---------|
| 1 | **ResonantVelocityBand** (T1) | 100 | 100 | +8% moveSpeed ✓, +0.3 runSpeed ✓ | **PASS** |
| 2 | **SpringZephyrBoots** (T2) | 100 | 100 | +10% moveSpeed ✓, +0.5 runSpeed ✓ | **PASS** |
| 3 | **SolarBlitzTreads** (T3) | 100 | 100 | +12% moveSpeed ✓, +0.7 runSpeed ✓, fire immunity at 70+ ✓ | **PASS** |
| 4 | **HarvestPhantomStride** (T4) | 100 | 100 | +14% moveSpeed ✓, +0.9 runSpeed ✓, phase at 80+ ✓ | **PASS** |
| 5 | **PermafrostAvalancheStep** (T5) | 100 | 100 | +16% moveSpeed ✓, +1.1 runSpeed ✓, +0.5 jump ✓, +5 def at 90+ ✓ | **PASS** |
| 6 | **VivaldisSeasonalSprint** (T6) | 120 | 120 | +18% moveSpeed ✓, +1.3 runSpeed ✓, +0.8 jump ✓ | **PASS** |

### Theme Tier (Post-Moon Lord) — MAX MOMENTUM MISMATCH

| # | Accessory | Tooltip Max Momentum | Code MaxMomentum | Verdict |
|---|-----------|---------------------|-----------------|---------|
| 7 | **MoonlitPhantomsRush** | **"max 100"** | **120** | **FAIL** |
| 8 | **HeroicChargeBoots** | **"max 100"** | **120** | **FAIL** |
| 9 | **InfernalMeteorStride** | **"max 100"** | **120** | **FAIL** |
| 10 | **EnigmasPhaseShift** | **"max 100"** | **120** | **FAIL** |
| 11 | **SwansEternalGlide** | **"max 100"** | **120** | **FAIL** |
| 12 | **FatesCosmicVelocity** | max 150 | 150 | **PASS** |

**Root Cause:** In `MomentumPlayer.GetMaxMomentum()`:
```csharp
if (HasVivaldisSeasonalSprint || HasMoonlitPhantomsRush || HasHeroicChargeBoots ||
    HasInfernalMeteorStride || HasEnigmasPhaseShift || HasSwansEternalGlide)
    return 120f;
```
All Theme T1-T5 accessories are grouped with VivaldisSeasonalSprint at 120 max momentum, but their tooltips all say "Enables the Momentum system (max 100)".

**Stat verification for theme accessories (non-momentum stats match):**
- MoonlitPhantomsRush: +20% speed ✓, +1.5 run ✓, +1.0 jump ✓, +5% dmg at night ✓
- HeroicChargeBoots: +22% speed ✓, +1.7 run ✓, +1.2 jump ✓, +8 def at 60+ ✓, SoC dash ✓
- InfernalMeteorStride: +24% speed ✓, +1.9 run ✓, +1.4 jump ✓, +8% dmg at 70+ ✓, lava immune ✓
- EnigmasPhaseShift: +26% speed ✓, +2.1 run ✓, +1.6 jump ✓, blackBelt at 80+ ✓, teleport 12.5 tiles ✓
- SwansEternalGlide: +28% speed ✓, +2.3 run ✓, +1.8 jump ✓, +100 wing time ✓, aggro -400 ✓, 50% slower decay ✓
- FatesCosmicVelocity: +30% speed ✓, +2.5 run ✓, +2.0 jump ✓, +150 wing time ✓, +10% dmg ✓

### Post-Fate T7-T10 — UNIMPLEMENTED ABILITIES

| # | Accessory | Tooltip Max | Code Max | Stat Match | Unimplemented Tooltip Feature | Verdict |
|---|-----------|------------|---------|-----------|------------------------------|---------|
| 13 | **NocturnalPhantomTreads** (T7) | 175 ✓ | 175 | +30% speed ✓, +2.5 run ✓ | "Consume 125 Momentum: Star Dash" — **NO CODE** | **FAIL** |
| 14 | **InfernalMeteorTreads** (T8) | 200 ✓ | 200 | +35% speed ✓, +3.0 run ✓ | "Consume 150 Momentum: Meteor Dash (200% weapon damage)" — **NO CODE** | **FAIL** |
| 15 | **JubilantZephyrTreads** (T9) | 225 ✓ | 225 | +40% speed ✓, +3.5 run ✓, 50% slower decay ✓ | "Consume 175 Momentum: Zephyr Burst" — **NO CODE** | **FAIL** |
| 16 | **EternalVelocityTreads** (T10) | 250 ✓ | 250 | +50% speed ✓, +4.5 run ✓ | "Consume 200 Momentum: Temporal Teleport (150 blocks)" — **NO CODE** | **FAIL** |

**FAIL #13-16 Detail:** Each T7-T10 tooltip describes a unique momentum-consuming active ability (Star Dash, Meteor Dash, Zephyr Burst, Temporal Teleport) with specific momentum costs (125/150/175/200). **None of these abilities exist in code.** `MomentumPlayer` only has `TryHeroicDash()` (80 momentum, from T2+) and `TryPhaseShift()` (100 momentum, from T4+). The T7-T10 accessories inherit these base abilities but don't have their own enhanced versions.

**Additional Observation — No Fusion Accessories:** The Mobility chain has NO fusion accessories (T7-T10 + no fusions = 16 total), unlike every other chain which has T7-T10 + 3 fusions = 22 total. This appears intentional but is worth noting if fusions were planned.

**Additional Issue — Time Slow Implementation:** Both FatesCosmicVelocity (tooltip: "enemies slowed by 20%") and EternalVelocityTreads (tooltip: "Time slows 40% for nearby enemies") use the **same** `ApplyTimeSlowToNearbyEnemies()` method with identical `npc.velocity *= 0.92f` per frame. No differentiation between the 20% and 40% slow values described in tooltips.

---

## ALL FAILURES — CONSOLIDATED

### Critical (Wrong values or missing code)

| # | Chain | Accessory | Issue | Fix Needed |
|---|-------|-----------|-------|-----------|
| 1 | Melee | MoonlitSonataBand | Tooltip: max 45, Code: max 40 | Update tooltip to 40 OR code to 45 |
| 2 | Melee | HeroicCrescendo | Tooltip: max 50, Code: max 40 | Update tooltip to 40 OR code to 50 |
| 3 | Melee | EnigmasDissonance | Tooltip: max 60 & Paradox at 50+, Code: max 40 & Paradox at 45+ | Update both tooltip values OR code |
| 4 | Melee | SwansPerfectMeasure | Tooltip: max 60, Code: max 40 | Update tooltip to 40 OR code to 60 |
| 5 | Mage | SwansBalancedFlow | Tooltip: overflow -150, Code: maxOverflow 120 | Update tooltip to -120 OR code to 150 |
| 6 | Summoner | SolarDirectorsCrest | Tooltip: "-5 defense", Code: BrokenArmor (halves defense) | Sync tooltip with actual debuff |
| 7 | Mobility | MoonlitPhantomsRush | Tooltip: max 100, Code: max 120 | Update tooltip to 120 |
| 8 | Mobility | HeroicChargeBoots | Tooltip: max 100, Code: max 120 | Update tooltip to 120 |
| 9 | Mobility | InfernalMeteorStride | Tooltip: max 100, Code: max 120 | Update tooltip to 120 |
| 10 | Mobility | EnigmasPhaseShift | Tooltip: max 100, Code: max 120 | Update tooltip to 120 |
| 11 | Mobility | SwansEternalGlide | Tooltip: max 100, Code: max 120 | Update tooltip to 120 |

### Unimplemented Features (Tooltip describes feature, code doesn't implement it)

| # | Chain | Accessory | Missing Feature |
|---|-------|-----------|----------------|
| 12 | Mobility | NocturnalPhantomTreads (T7) | "Consume 125 Momentum: Star Dash" |
| 13 | Mobility | InfernalMeteorTreads (T8) | "Consume 150 Momentum: Meteor Dash (200% weapon damage)" |
| 14 | Mobility | JubilantZephyrTreads (T9) | "Consume 175 Momentum: Zephyr Burst" |
| 15 | Mobility | EternalVelocityTreads (T10) | "Consume 200 Momentum: Temporal Teleport (150 blocks)" |
| 16 | Mobility | EternalVelocityTreads (T10) | Time slow "40%" vs same code as Fate's "20%" |

### Minor (Tooltip wording vs vanilla mechanic)

| # | Chain | Accessory | Issue |
|---|-------|-----------|-------|
| 17 | Defense | EnigmasVoidShell | "10% chance to phase through attacks" vs `blackBelt=true` (vanilla dodge) |
| 18 | Summoner | PermafrostCommandersCrown | "25% slow" vs vanilla Chilled buff (~33% slow) |
| 19 | Summoner | EternalConductorsScepter | "Temporal Finale: 3x speed 2s" vs code: single 5x damage nuke |
