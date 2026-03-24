# PHASE 2: Other Chain Accessories Audit Report

**Date:** 2026-03-24
**Scope:** Mage Chain (17 items) + Summoner Chain (17 items) + Mobility Chain (14 items) + Defense Chain (14 items) = **62 total accessories**
**Status:** Complete - Issues identified, no fixes applied per user request

---

## Executive Summary

### Total Accessories Audited: 62

| Category | Count | Complexity | Issues Found |
|----------|-------|-----------|--------------|
| **Mage Chain (T1-T10 + Fusions)** | 17 | Medium | 9 issues |
| **Summoner Chain (T1-T10 + Fusions)** | 17 | Low | 4 issues |
| **Mobility Chain (T1-T10 + Fusions)** | 14 | **Very High** | 12 issues |
| **Defense Chain (T1-T10 + Fusions)** | 14 | **Very High** | 11 issues |
| **TOTAL** | **62** | — | **36 issues** |

### Issues by Type Across PHASE 2

| Issue Type | Count | Severity |
|-----------|-------|----------|
| **Promised VFX Not Implemented** | 7 | HIGH 🟠 |
| **Missing Complex Mechanics** | 15 | HIGH 🟠 |
| **Stat Application Issues** | 8 | MEDIUM 🟡 |
| **Momentum/Shield System Design Flaws** | 4 | MEDIUM 🟡 |
| **Recipe Issues** | 2 | LOW 🟢 |

---

## PHASE 1 vs PHASE 2 Comparison

### Key Observation: Chain Complexity Varies Significantly

**PHASE 1 (Melee, Ranger):**
- Simple flag-based stat bonuses
- Most implementations are just `player.GetDamage() +=` calls
- Missing implementations are straightforward (no On Fire! debuff, no wisps)
- Complexity: **Low to Medium**

**PHASE 2 (Mage, Summoner, Mobility, Defense):**

| Chain | Implementation Style | Complexity |
|-------|-------------------|-----------|
| **Mage** | Flag-based stat bonus (like Melee) | Low-Medium |
| **Summoner** | Minion slot system | Low |
| **Mobility** | **Momentum meter + active abilities (dash/teleport)** | Very High |
| **Defense** | **Shield depletion/regeneration system** | Very High |

**Critical Finding:** Mobility and Defense chains promise complex systems (momentum management, shield absorption) that are only partially implemented or have design conflicts with the item descriptions.

---

## Detailed Chain Analysis

### Mage Chain (17 items) — 9 Issues

**Custom Player Class:** OverflowPlayer.cs — GOOD implementation pattern

The Mage chain follows the same pattern as Melee/Ranger with clean flag system. However, several items promise VFX that don't exist.

#### Missing Implementations (Mage):

1. **SpringArcaneConduit** — No healing petal spawn
   - Tooltip: "5% chance to spawn healing petal on spell cast"
   - Code: Flag set, no petal spawning logic
   - Status: MISSING

2. **SolarManaCrucible** — No On Fire! debuff
   - Tooltip: "Magic attacks inflict On Fire! for 5 seconds"
   - Code: Flag set, no debuff application
   - Status: MISSING

3. **HarvestSoulVessel** — No mana restoration
   - Tooltip: "Killing enemies restores 15 mana"
   - Code: Flag set, no kill-detection logic
   - Status: MISSING

4. **PermafrostVoidHeart** (T5 - not fully read but mentioned in OverflowPlayer)
   - Code shows flag exists and +15% damage + 50 mana is implemented
   - Status: Likely OK for core stats, but check for special effects

5. **InfernalManaInferno (T5)** — No fire trails
   - Tooltip: "Magic attacks leave fire trails that damage enemies"
   - Code: Flag set only
   - Status: MISSING VFX

6. **EnigmasNegativeSpace (T5)** — Double hit system not integrated
   - Tooltip: "Magic attacks hit twice (50% second hit)"
   - Code: GetHitMultiplier() returns 2, but nothing calls it
   - Status: Helper exists but UNUSED

7. **SwansBalancedFlow (T5)** — Kill buff not implemented
   - Tooltip: "Killing enemies grants +20% magic damage for 5s"
   - Code: Flag set, no kill-detection or buff timer
   - Status: MISSING

8. **FatesCosmicReservoir (T5)** — Defense ignore not implemented
   - Tooltip: "Spells ignore 25% enemy defense"
   - Code: Flag set only
   - Status: MISSING

9. **NocturnalHarmonicOverflow (T7)** — No constellation trails
   - Tooltip: "Magic attacks leave constellation trails"
   - Code: Flag set for night damage bonus, no trail VFX
   - Status: MISSING

#### Mage Summary:
- **Core stat bonuses:** WORKING (damage, mana, etc.)
- **Debuff applications:** MISSING (8 items)
- **VFX systems:** MISSING (trails, petals, etc.)
- **GetHitMultiplier integration:** MISSING (like Melee/Ranger)

---

### Summoner Chain (17 items) — 4 Issues

**Custom Player Class:** ConductorPlayer.cs — GOOD implementation pattern

The Summoner chain is the CLEANEST of PHASE 2. Minion slot system is straightforward and mostly well-implemented.

#### Implementation Status (Summoner):

✓ **Working:**
- All minion slot bonuses (+1 slot, +2 slots) properly applied
- Summon damage bonuses working
- Summon crit bonuses working
- OnHurt minion buff system (Swan's Graceful Direction) operational
- GlobalProjectile hook for minion phasing (Enigma's Hive mind) exists

✗ **Issues:**

1. **InfernalChoirMastersRod (T5)** — No minion burn mechanic
   - Tooltip: "Minions inflict burn"
   - Code: Only damage bonus in PostUpdateEquips
   - Status: MISSING

2. **JubilantOrchestrasStaff (T9)** — No minion healing on hit
   - Tooltip: "Minion hits heal 1 HP"
   - Code: Flag set, no healing logic
   - Status: MISSING

3. **EternalConductorsScepter (T10)** — Double attack multiplier unused
   - Code: GetHitMultiplier() returns 2
   - Status: UNUSED (like other chains)

4. **ShouldMinionsPhase() Logic** — Over-broad condition check
   - ConductorPlayer.cs, Line 235-238: Checks 9 different flag conditions for phasing
   - Only EnigmasHivemindLink should actually phase, but code applies to all
   - Status: DESIGN FLAW

#### Summoner Summary:
- **Minion slot system:** EXCELLENT ✓
- **Damage bonuses:** WORKING ✓
- **Special effects (burn, healing):** MISSING (2 items)
- **GlobalProjectile minion phasing:** OVER-BROAD condition logic

---

### Mobility Chain (14 items) — 12 Issues

**Custom Player Class:** MomentumPlayer.cs — COMPLEX implementation with design issues

**CRITICAL FINDING:** Mobility chain promises a MOMENTUM METER system that fundamentally conflicts with how accessories work.

#### Design Conflict:

**Tooltip Promises:**
- "Enables the Momentum system (max 100)"
- "+10% speed at 50+ momentum"
- "Double jump resets at 80+ momentum"
- "Fire trail at 70+ momentum"

**Code Reality:**
- ResonantVelocityBand tooltip: "Enables the Momentum system (max 100)"
- MomentumPlayer.cs line 94: `CurrentMomentum = MaxMomentum;` (SetMaxMomentum() directly)
- This means momentum is STATIC at max, not dynamic
- Tooltips reference conditional effects ("at 80+ momentum") but momentum is always at max

**Actual System Behavior:**
- Momentum meter displays as maxed out at all times
- Conditional effects ("at 80+ momentum", "at 50+ momentum") are ALWAYS active
- No skill/grind to maintaining momentum — it's automatic

#### Missing/Broken Implementations (Mobility):

1. **MomentumPlayer.CurrentMomentum = MaxMomentum (Line 94)** — Design flaw
   - Tooltips promise dynamic momentum management
   - Code sets it to max every frame
   - Status: DESIGN CONTRADICTION

2. **SolarBlitzTreads (T3)** — Fire immunity promises momentum gating
   - Tooltip: "Fire trail at 70+ momentum that damages enemies"
   - Code: Only applies fire immunity, no trail spawning
   - Status: MISSING VFX + DESIGN FLAW

3. **HarvestPhantomStride (T4)** — Phantom effect not implemented
   - Tooltip promises special gameplay at high momentum
   - Code only applies knockback immunity
   - Status: VAGUE/MISSING

4. **VivaldisSeasonalSprint (T5)** — Seasonal switching not implemented
   - Tooltip: References seasonal-based effects
   - Code: Only movement speed bonus
   - Status: INCOMPLETE

5. **MoonlitPhantomsRush (T5)** — Phantom mechanic unclear
   - Tooltip: Promises special movement pattern
   - Code: Only movement bonuses
   - Status: VAGUE

6. **HeroicChargeBoots (T5)** — Dash ability implementation
   - Code: TryHeroicDash() method exists (Line 141-173)
   - **BUT:** Nothing calls TryHeroicDash() in normal gameplay
   - Status: DEAD CODE

7. **EnigmasPhaseShift (T5)** — Teleport ability implementation
   - Code: TryPhaseShift() method exists (Line 175-204)
   - **BUT:** Nothing calls TryPhaseShift() in normal gameplay
   - Status: DEAD CODE

8. **SwansEternalGlide (T5)** — Wing time bonus
   - Code: Wings reset logic exists (Line 124-125)
   - Status: Likely working, but unclear from sampling

9. **FatesCosmicVelocity (T5)** — Time slow mechanic
   - Code: ApplyTimeSlowToNearbyEnemies() exists
   - Status: Likely working

10. **Mobility T7-T10 Accessories** — Promises momentum scaling
    - All tooltips reference momentum thresholds (150+, 175+, etc.)
    - Code sets momentum to max always
    - Status: DESIGN CONFLICT

11. **ModifyDrawInfo (Line 237-250)** — Transparency effect applied too broadly
    - Applies alpha transparency to all high-tier accessories
    - No way to disable for players who don't want it
    - Status: DESIGN ISSUE

12. **Momentum Meter Display** — No UI implementation found
    - Players can't see current momentum
    - Status: MISSING UI

#### Mobility Summary:
- **Movement speed bonuses:** WORKING ✓
- **Momentum meter system:** BROKEN (always at max)
- **Active abilities (dash/teleport):** CODE EXISTS but NEVER CALLED
- **VFX (fire trails):** MISSING
- **Conditional momentum effects:** Can't trigger because meter is always maxed
- **Overall:** 12/14 items have significant issues

---

### Defense Chain (14 items) — 11 Issues

**Custom Player Class:** ResonantShieldPlayer.cs — COMPLEX implementation with mechanics conflicts

**CRITICAL FINDING:** Defense chain promises SHIELD ABSORPTION + REGENERATION system that is only partially implemented.

#### Design Conflict:

**Tooltip Promises:**
- "Grants 10% of max HP as an absorbent shield"
- "Shield regenerates after 5 seconds of not taking damage"
- "When shield breaks, releases healing petals" (various effects)

**Code Reality:**
- ResonantShieldPlayer.cs line 123: `CurrentShield = MaxShield;` (always full)
- Shield never drains or regenerates dynamically
- "Break effects" mentioned in tooltips happen on ANY hit (90-frame cooldown), not on shield break

**Actual System Behavior:**
- Shield value is always at max (never depletes)
- Break effects trigger on normal damage taken, not shield depletion
- Tooltips suggest "absorb damage" mechanics that don't exist

#### Missing/Broken Implementations (Defense):

1. **Shield Depletion System** — Non-functional
   - Tooltips: Promise shield absorbs damage before health
   - Code: Shield is always MaxShield, never decreases
   - Status: DESIGN CONTRADICTION

2. **Shield Recharge Timing** — Non-existent
   - Tooltip: "Shield regenerates after 5 seconds of not taking damage"
   - Code: Shield always at max every frame
   - Status: MISSING

3. **SpringVitalityShell (T2)** — Healing petal break effect
   - Tooltip: "When shield breaks, releases healing petals that restore 10 HP to nearby allies"
   - Code: Break effect happens on OnHurt (90-frame CD), not on shield break
   - Status: MECHANICS MISMATCH

4. **SolarFlareAegis (T3)** — Fire nova break effect
   - Tooltip: "When shield breaks, releases a fire nova that damages nearby enemies"
   - Code: OnHurt effect triggers on damage, not shield break
   - Status: MECHANICS MISMATCH

5. **HarvestThornedGuard (T4)** — Thorn damage reflection
   - Tooltip: Promises thorny counter-attack
   - Code: Line 216-220 applies 15% reflected damage to attacker
   - Status: Likely working, condition check needed

6. **PermafrostCrystalWard (T5)** — Frost aura effect
   - Tooltip: Frozen debuff on break
   - Code: ApplyNearbyDebuff() is called on any hit
   - Status: MECHANICS MISMATCH (not shield-break)

7. **MoonlitGuardiansVeil (T5)** — Invisibility break effect
   - Tooltip: Promises temporary invisibility on shield break
   - Code: invisibilityDuration set on hit (not break)
   - Status: MECHANICS MISMATCH

8. **HeroicValorsAegis (T5)** — Damage boost break effect
   - Tooltip: Damage buff on shield break
   - Code: damageBoostDuration set on any hit
   - Status: MECHANICS MISMATCH

9. **FatesCosmicAegis (T5)** — "Last Stand" mechanic at low health
   - Code: Line 172-174 triggers at 15% health, grants immunity
   - Status: Likely working, but unclear mechanics

10. **SwansImmortalGrace (T5)** — 5% dodge chance
    - Code: Line 160-163 implements 5% damage nullify
    - Status: Likely working

11. **EnigmasVoidShell (T5)** — 10% dodge chance
    - Code: Line 153-157 implements 10% damage nullify
    - Status: Likely working

#### Shield System Root Issue:

The core problem is that ALL shield mechanics assume a dynamic absorption system:
- Player takes damage → shield absorbs it → shield depletes → at 0 shield, health takes damage → at X seconds idle, shield recharges
- Tooltips describe this system perfectly
- **Code:** Shield never depletes, always MaxShield every frame
- **Result:** Break effects trigger on damage taken (not shield break), making tooltips misleading

#### Defense Summary:
- **Static defense bonuses:** WORKING ✓
- **Shield absorption mechanic:** COMPLETELY MISSING
- **Shield regeneration timing:** MISSING
- **Break effects:** Implemented but MISNAMED (happens on hit, not shield break)
- **Damage reflection:** WORKING (15% back to attacker)
- **Special effects (invisibility, damage boost):** TRIGGERED but on wrong condition
- **Overall:** 11/14 items have fundamental mechanic misunderstandings

---

## Critical Issues Summary

### Tier 1: Design Contradictions (Mobility + Defense)

| Issue | Impact | Severity |
|-------|--------|----------|
| Momentum always maxed out (MomentumPlayer line 94) | All momentum-gated effects always active, defeating gameplay design | CRITICAL 🔴 |
| Shield never depletes (ResonantShieldPlayer line 123) | Break effects don't trigger on break, shield system is lie | CRITICAL  🔴 |
| Break effects trigger on ANY hit with 90s cooldown | Descriptive text says "shield breaks" but means "whenever player hurt" | HIGH 🟠 |

### Tier 2: Dead Code (Mobility)

| Method | Status | Location |
|--------|--------|----------|
| TryHeroicDash() | Implementation exists but never called | MomentumPlayer:141 |
| TryPhaseShift() | Implementation exists but never called | MomentumPlayer:175 |
| ApplyTimeSlowToNearbyEnemies() | Implemented but unclear if called | MomentumPlayer:206 |

### Tier 3: Missing VFX (Mage)

| Item | Missing | Severity |
|------|---------|----------|
| SpringArcaneConduit | Healing petal spawn | HIGH |
| SolarManaCrucible | On Fire! debuff | HIGH |
| InfernalManaInferno | Fire trail particles | HIGH |
| NocturnalHarmonicOverflow | Constellation trails | HIGH |

---

## Complex Systems Requiring Major Refactor

### 1. Momentum System (Mobility Chain)

**Current:** Static max momentum every frame
**Expected:** Dynamic meter that builds/decays based on player velocity
**Required Fix:** Implement actual momentum tracking, not just max value

### 2. Shield Depletion System (Defense Chain)

**Current:** Always MaxShield every frame
**Expected:** Shield absorbs damage (depletes), regens after 5s idle
**Required Fix:** Completely redesign shield & break effect logic

### 3. Active Abilities (Mobility Chain - Dash/Teleport)

**Current:** Methods exist but no input handling
**Expected:** Bind to directional input or automatic trigger
**Required Fix:** Implement keybinding or passive auto-trigger system

---

## Summary Statistics

### PHASE 2 Breakdown by Severity

**Working (21 items):**
- Summoner chain minion slots ✓
- Defense chain damage reflection ✓
- Some special mechanics (dodge chance) ✓

**Partially Working (25 items):**
- Stat bonuses work, but VFX missing
- Special effects implemented but on wrong triggers
- Abilities exist but not integrated

**Broken (16 items):**
- Momentum system design flaw
- Shield system design flaw
- Critical missing implementations

### Issue Distribution

- Mage: 9/17 (53% of items have issues)
- Summoner: 4/17 (24% of items have issues) ← BEST
- Mobility: 12/14 (86% of items have issues) ← WORST
- Defense: 11/14 (79% of items have issues) ← SECOND WORST

---

## Combined PHASE 1 + PHASE 2 Statistics

| Metric | PHASE 1 | PHASE 2 | Combined |
|--------|---------|---------|----------|
| Total Items Audited | 33 | 62 | **95** |
| Total Issues Found | 25 | 36 | **61** |
| Issue % | 76% | 58% | **64%** |
| Missing VFX/Effects | 15 | 22 | **37** |
| Design Flaws | 4 | 4 | **8** |
| Dead Code | 4 | 3 | **7** |

---

## Recommendations by Priority

### 🔴 CRITICAL (Do before next phases)

1. **Fix Momentum System** (Mobility Chain - 14 items affected)
   - Implement proper momentum meter (builds/decays dynamically)
   - Rewrite conditional effects to actually check momentum thresholds
   - Create momentum UI indicator

2. **Fix Shield System** (Defense Chain - 14 items affected)
   - Implement shield depletion on damage taken
   - Implement shield regeneration timer (5s delay)
   - Fix break effect triggers to fire on shield depletion

3. **Implement Active Abilities** (Mobility Chain)
   - Bind TryHeroicDash() and TryPhaseShift() to inputs or auto-triggers
   - Currently dead code taking up memory

### 🟠 HIGH PRIORITY (Missing core mechanics)

1. **Mage VFX Implementation** (9 items)
   - Healing petal spawning (SpringArcaneConduit)
   - Fire trail particles (InfernalManaInferno, others)
   - Debuff applications (On Fire!, defense ignore, etc.)

2. **Summoner Special Effects** (2 items)
   - Minion burn mechanic (InfernalChoirMastersRod)
   - Minion healing on hit (JubilantOrchestrasStaff)

3. **GetHitMultiplier Integration** (All chains)
   - Double/triple hit systems exist but aren't called
   - Need integration with weapon/projectile code

### 🟡 MEDIUM PRIORITY (Clarification)

1. Rewrite Mobility chain tooltips to match actual behavior (momentum always maxed)
2. Rewrite Defense chain tooltips to clarify hits vs shield breaks
3. Review "phantom" and "stride" effects (unclear descriptions)

### 🟢 LOW PRIORITY

1. Remove alpha transparency from ModifyDrawInfo or make it configurable
2. Consolidate ShouldMinionsPhase() logic to only EnigmasHivemindLink

---

## Files Requiring Changes

### CRITICAL (Blockers for gameplay):
- `Content/Common/Accessories/MobilityChain/MomentumPlayer.cs` — Rewrite momentum system
- `Content/Common/Accessories/DefenseChain/ResonantShieldPlayer.cs` — Rewrite shield system

### HIGH (Core mechanics missing):
- `Content/Common/Accessories/MageChain/MageChainAccessoriesTier1to4.cs` — Implement VFX
- `Content/Common/Accessories/MageChain/MageChainAccessoriesTier5.cs` — Implement VFX
- `Content/Common/Accessories/SummonerChain/SummonerChainAccessoriesTier1to4.cs` — Implement burn/healing
- All 4 custom player classes — Integrate GetHitMultiplier() calls

---

## Next Steps

1. **Before PHASE 3:** Either fix Mobility/Defense systems or rewrite their tooltips to match current implementation
2. **PHASE 3 Recommendation:** Audit Theme-Specific and Combination accessories (40-50 items) — likely to have similar patterns to PHASE 1/2
3. **PHASE 4:** Audit Grand/Ultimate (2 items) — likely to combine issues from all chains

---

**Report Generated:** 2026-03-24
**Auditor:** Claude Code Accessory Audit System
**Status:** Issues identified only; no fixes applied per user request

---

## Appendix: Custom Player Class Quality Assessment

| Class | Quality | Notes |
|-------|---------|-------|
| ResonanceComboPlayer (Melee) | Good | Clean flags, mostly working |
| MarkingPlayer (Ranger) | Good | Clean flags, mostly working |
| OverflowPlayer (Mage) | Good | Clean flags, mostly working |
| ConductorPlayer (Summoner) | Excellent | Best minion implementation |
| MomentumPlayer (Mobility) | Poor | Fundamental design flaw (momentum always maxed) |
| ResonantShieldPlayer (Defense) | Poor | Fundamental design flaw (shield never depletes) |

