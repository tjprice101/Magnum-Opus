# Localization Audit Report
**File:** `Localization/en-US_Mods.MagnumOpus.hjson` (~5,600 lines)  
**Method:** Cross-referenced every key in the HJSON file against 2,488 class names extracted from all `.cs` files in the project.

---

## Summary

| Section | Total Keys | Orphaned | Duplicates |
|---------|-----------|----------|------------|
| **Items** | 638 unique | 134 | 154 |
| **Buffs** | 66 | 0 | 0 |
| **NPCs** | 51 | 1 | 0 |
| **Projectiles** | 988 | 576 | 0 |
| **Total** | 1,743 | **711** | **154** |

---

## 1. ENCODING / CHARACTER ISSUES (20 lines)

The file contains **mojibake characters** — corrupted text where special characters were lost during encoding conversion. These render as garbage characters in-game.

### ` -- ` (should be em-dash `—`)
| Line | Context |
|------|---------|
| 48 | `'The eternal cycle made blade  -- each swing echoes moonlight on water'` |
| 60 | `Rapid-fire bouncing moonlight beams with prismatic refraction  -- each bounce...` |
| 61 | `Bounces 3+ split into spectral child beams  -- light refracting through...` |
| 63 | `Right-click unleashes Serenade Mode  -- a devastating prismatic mega-beam...` |
| 66 | `'The moon whispers secrets to those who listen  -- each note a color...'` |
| 74 | `'The conductor raises the baton  -- and the moonlight obeys'` |
| 78 | `Right-click to toggle Conductor Mode  -- direct the Goliath's beams...` |
| 1080 | `5-phase Surgical Precision combo  -- Precise Incision, Crescent Cut...` |
| 1086 | `'Sharp as the moon's edge at midnight  -- a silver scalpel honed by quiet sorrow'` |
| 2445 | `'From death comes rebirth in silver light  -- the final movement that silences all...'` |
| 2448 | `Standard  -- Ricochets 10 times with escalating crater detonations` |
| 2449 | `Comet Core  -- Pierces through 5 enemies with burning ember wake` |
| 2450 | `Supernova  -- Arcing artillery that detonates in massive AoE` |

### `*` (should be `★` or similar star symbol)
| Line | Context |
|------|---------|
| 3122 | `*ULTIMATE RESONANCE SYSTEM *` |
| 3256 | `*ULTIMATE RANGER ACCESSORY *` |
| 3629 | `*ULTIMATE MAGE ACCESSORY *` |
| 3748 | `*ULTIMATE SUMMONER ACCESSORY *` |
| 3878 | `*ULTIMATE MOBILITY ACCESSORY *` |
| 4025 | `*ULTIMATE DEFENSE ACCESSORY *` |

### `->` (should be `→` or similar arrow)
| Line | Context |
|------|---------|
| 3949 | `Shield break effect cycles: Heal ->Fire ->Thorns ->Freeze` |

**Fix:** Replace all ` -- ` with ` — `, all `*` with `★`, and all `->` with `→`.

---

## 2. SPELLING MISMATCH: "Judgement" vs "Judgment"

All C# class names use **"Judgment"** (American English, no 'e'). Several HJSON keys use **"Judgement"** (British English, with 'e'), causing them to be orphaned.

| HJSON Key (wrong) | Line | Correct Class Name | Fix |
|---|---|---|---|
| `HarmonyOfJudgement` | 2062 | `HarmonyOfJudgment` | Rename key |
| `StaffOfFinalJudgement` | 2066 | `StaffOfFinalJudgment` | Rename key |
| `DiesIraeHeraldOfJudgement` (NPC) | 4408 | `DiesIraeHeraldOfJudgment` | Rename key |
| `HarmonyOfJudgementMinion` (Proj) | 4915 | No class with either spelling | Delete entry |
| `JudgementRay` (Proj) | 4916 | No class with either spelling | Delete entry |

---

## 3. ORPHANED ITEMS (134 total)

### 3a. VFX_ Prefixed Items — No corresponding class (50 entries, lines 1926–2024)
These appear to be placeholder entries for a VFX system that was never implemented. **Safe to delete all.**

<details><summary>Full list (50 entries)</summary>

| Line | Key |
|------|-----|
| 1926 | VFX_ClairDeLune_DreamyMist |
| 1928 | VFX_ClairDeLune_ImpressionistNotes |
| 1930 | VFX_ClairDeLune_LunarArcs |
| 1932 | VFX_ClairDeLune_MoonbeamRipples |
| 1934 | VFX_ClairDeLune_WaterReflection |
| 1936 | VFX_DiesIrae_ApocalypseVortex |
| 1938 | VFX_DiesIrae_BrimstoneEmbers |
| 1940 | VFX_DiesIrae_JudgmentArcs |
| 1942 | VFX_DiesIrae_RequiemNotes |
| 1944 | VFX_DiesIrae_WrathFlames |
| 1946 | VFX_Enigma_ArcaneGlyphs |
| 1948 | VFX_Enigma_GreenFlameSlash |
| 1950 | VFX_Enigma_MysteryNotes |
| 1952 | VFX_Enigma_VoidTendrils |
| 1954 | VFX_Enigma_WatchingEyes |
| 1956 | VFX_Eroica_HeroicArcs |
| 1958 | VFX_Eroica_SakuraDrift |
| 1960 | VFX_Eroica_SymphonyNotes |
| 1962 | VFX_Eroica_TriumphSparks |
| 1964 | VFX_Eroica_ValorCrescent |
| 1966 | VFX_Fate_CelestialNotes |
| 1968 | VFX_Fate_CosmicClouds |
| 1970 | VFX_Fate_GlyphOrbit |
| 1972 | VFX_Fate_RealitySlash |
| 1974 | VFX_Fate_StarStream |
| 1976 | VFX_Campanella_BellArcs |
| 1978 | VFX_Campanella_CrescentFlame |
| 1980 | VFX_Campanella_EmberSpray |
| 1982 | VFX_Campanella_MusicNotes |
| 1984 | VFX_Campanella_SmokeTrail |
| 1986 | VFX_Moonlight_CrescentMoon |
| 1988 | VFX_Moonlight_LunarMist |
| 1990 | VFX_Moonlight_LunarVortex |
| 1992 | VFX_Moonlight_NocturneNotes |
| 1994 | VFX_Moonlight_StarlightTrail |
| 1996 | VFX_Nachtmusik_EveningWaltz |
| 1998 | VFX_Nachtmusik_MoonlitArcs |
| 2000 | VFX_Nachtmusik_NightBreeze |
| 2002 | VFX_Nachtmusik_NocturneNotes |
| 2004 | VFX_Nachtmusik_StarlightSerenade |
| 2006 | VFX_Seasons_AutumnHarvest |
| 2008 | VFX_Seasons_SeasonalCycle |
| 2010 | VFX_Seasons_SpringBlossom |
| 2012 | VFX_Seasons_SummerBlaze |
| 2014 | VFX_Seasons_WinterFrost |
| 2016 | VFX_SwanLake_BalletNotes |
| 2018 | VFX_SwanLake_DoubleHelix |
| 2020 | VFX_SwanLake_FeatherWaltz |
| 2022 | VFX_SwanLake_MonochromeArcs |
| 2024 | VFX_SwanLake_PrismaticShimmer |

</details>

### 3b. "Item" Suffix Orphans — Class naming mismatch (5 entries)
These use an `Item` suffix that doesn't match their actual class name.

| Line | HJSON Key | Notes |
|------|-----------|-------|
| 2276 | InfernalCleaverItem | Check if class is `InfernalCleaver` |
| 2278 | FrostbiteEdgeItem | Check if class is `FrostbiteEdge` |
| 2280 | CosmicRendBladeItem | Check if class is `CosmicRendBlade` |
| 2282 | VerdantCrescendoItem | Check if class is `VerdantCrescendo` |
| 2284 | ArcaneHarmonicsItem | Check if class is `ArcaneHarmonics` |

### 3c. Sandbox/Foundation Test Items (2 entries)
| Line | Key |
|------|-----|
| 2290 | SandboxTerraBlade |
| 2428 | FoundationAnimeStyleAttackAnimation |

### 3d. Removed La Campanella Weapons (4 entries, lines 509–553)
| Line | Key |
|------|-----|
| 509 | GrandioseChime |
| 531 | InfernalChimesCalling |
| 542 | PiercingBellsResonance |
| 553 | SymphonicBellfireAnnihilator |

### 3e. Removed Enigma Variations Weapons (2 entries)
| Line | Key |
|------|-----|
| 573 | VariationsOfTheVoid |
| 583 | TheUnresolvedCadence |

### 3f. Removed Fate Weapons (12 entries, lines 1140–1240)
| Line | Key |
|------|-----|
| 985 | ResonanceAwakener |
| 1140 | CelestialConductorsBaton |
| 1150 | CodaOfAnnihilation |
| 1160 | DestinysCrescendo |
| 1170 | FractalOfTheStars |
| 1180 | LightOfTheFuture |
| 1190 | OpusUltima |
| 1200 | RequiemOfReality |
| 1210 | ResonanceOfABygoneReality |
| 1220 | SymphonysEnd |
| 1230 | TheConductorsLastConstellation |
| 1240 | TheFinalFermata |

### 3g. Removed/Deprecated Misc Items (various)
| Line | Key | Category |
|------|-----|----------|
| 1872 | ConductorOfConstellations | Nachtmusik? |
| 2082 | WrathsPickaxe | Dies Irae tool |
| 2084 | ConstellationQuiver | Nachtmusik quiver |

### 3h. Removed Clock/Temporal Items (10 entries, lines 2096–2146)
| Line | Key |
|------|-----|
| 2096 | ClockworkTargetingModule |
| 2098 | ConductorsTemporalBaton |
| 2100 | ResonantChronosphere |
| 2102 | TemporalWrathGauntlet |
| 2114 | ChronologistsExcavator |
| 2116 | ClockworkExcavationDrill |
| 2118 | HammerOfBrokenHours |
| 2120 | TemporalCleaver |
| 2142 | AutomatonsTuningFork |
| 2144 | GearDrivenArbiter |
| 2146 | LunarPhylactery |

### 3i. Removed Weapon Batch (36 entries, lines 2348–2418)
Appears to be an entire batch of weapons across all themes that were removed or renamed.

| Line | Key | Probable Theme |
|------|-----|---------------|
| 2348 | EroicasBanner | Eroica |
| 2350 | BlossomsFury | Eroica |
| 2352 | RiddlesRepeater | Enigma |
| 2354 | ElgarsPendulum | Enigma |
| 2356 | CiphersEdge | Enigma |
| 2358 | ArcanumCodex | Enigma |
| 2360 | WrathsExecutioner | Dies Irae |
| 2362 | RequiemCannon | Dies Irae |
| 2364 | InfernalRequiem | Dies Irae |
| 2366 | HeraldOfRuin | Dies Irae |
| 2368 | FuneralHymn | Eroica |
| 2370 | HeroicAnthem | Eroica |
| 2372 | DestinysCleaver | Fate |
| 2374 | FatumLongbow | Fate |
| 2376 | MoiraiGrimoire | Fate |
| 2378 | SpindleOfTheFates | Fate |
| 2380 | BellStriker | La Campanella |
| 2382 | CampanellasFlame | La Campanella |
| 2384 | ChimeCannon | La Campanella |
| 2386 | InfernalEtude | La Campanella |
| 2388 | ConductorsLunarBaton | Moonlight |
| 2390 | LunarElegy | Moonlight |
| 2392 | MoonlightRequiem | Moonlight |
| 2394 | SonataOfTheTides | Moonlight |
| 2396 | ConstellationArbalest | Nachtmusik |
| 2398 | NightConductorsBaton | Nachtmusik |
| 2400 | NocturnalConcerto | Nachtmusik |
| 2402 | SerenadeBlade | Nachtmusik |
| 2404 | ConductorOfCelebration | Ode to Joy |
| 2406 | HymnBallista | Ode to Joy |
| 2408 | JubilantChorus | Ode to Joy |
| 2410 | OdeEternal | Ode to Joy |
| 2412 | CygnusBow | Swan Lake |
| 2414 | OdettesAria | Swan Lake |
| 2416 | SiegfriedsOath | Swan Lake |
| 2418 | SwansLament | Swan Lake |

### 3j. Seasonal Accessory Orphans (duplicated on both lines, both orphaned) (7 entries)
These items have entries at TWO lines in the file, and NEITHER matches a class. The class was likely renamed.

| Lines | Key |
|-------|-----|
| 1752, 3541 | HarvestSoulVessel |
| 1764, 3531 | SolarManaCrucible |
| 1774, 3025 | HarvestRhythmSignet |
| 1786, 3014 | SolarCrescendoRing |
| 1822, 3162 | HarvestReapersMark |
| 1834, 3151 | SolarTrackersBadge |
| 1858, 3656 | SolarDirectorsCrest |

---

## 4. DUPLICATE ITEM KEYS (154 total)

The Items section contains **154 keys that appear twice**. The pattern: an empty stub block (around lines 1496–1868) and a full entry with DisplayName/Tooltip later (lines 2441–4021). The full entry is the one used by tModLoader; the stubs are redundant.

**Recommendation:** Delete all stub entries in the lines 1496–1868 range.

<details><summary>All 154 duplicate keys</summary>

| Key | Lines (stub, full) |
|-----|-------------------|
| HarvestMantle | 1496, 2908 |
| ReapersCharm | 1498, 2890 |
| TwilightRing | 1500, 2899 |
| AutumnResonantEnergy | 1502, 2637 |
| DeathsNote | 1504, 2724 |
| DecayEssence | 1506, 2603 |
| DecayFragment | 1508, 2733 |
| DormantAutumnCore | 1510, 2537 |
| HarvestBar | 1512, 2569 |
| LeafOfEnding | 1514, 2706 |
| TwilightWingFragment | 1516, 2715 |
| AdagioOfRadiantValor | 1518, 3362 |
| CompleteHarmony | 1522, 3391 |
| FantasiaOfBurningGrace | 1528, 3305 |
| InfernoOfLostShadows | 1530, 3331 |
| NocturneOfAzureFlames | 1534, 3264 |
| RequiemOfTheEnigmaticFlame | 1538, 3376 |
| ReverieOfTheSilverSwan | 1542, 3292 |
| TrinityOfNight | 1554, 3345 |
| TriumphantArabesque | 1556, 3318 |
| ValseMacabre | 1558, 3278 |
| CodasEcho | 1562, 3470 |
| ConductorsInsight | 1564, 3451 |
| ElixirOfTheMaestro | 1566, 3442 |
| FatesBlessing | 1568, 3460 |
| FatesCosmicBrew | 1570, 3480 |
| HarmonicElixir | 1572, 3424 |
| HarmonicResonatorFragment | 1574, 3407 |
| MinorResonanceTonic | 1576, 3415 |
| SeasonalDraught | 1578, 3433 |
| BellEssence | 1592, 2797 |
| FateEssence | 1594, 2824 |
| GraceEssence | 1596, 2815 |
| LunarEssence | 1598, 2778 |
| MysteryEssence | 1600, 2806 |
| ValorEssence | 1602, 2787 |
| BrokenBaton | 1620, 2480 |
| DullResonator | 1622, 2513 |
| FadedSheetMusic | 1624, 2472 |
| MinorMusicNote | 1626, 2463 |
| OldMetronome | 1628, 2497 |
| ResonantCrystalShard | 1630, 2455 |
| RustedClef | 1632, 2505 |
| TuningFork | 1634, 2488 |
| ResurrectionOfTheMoon | 1640, 2441 |
| CycleOfSeasons | 1642, 2968 |
| RelicOfTheEquinox | 1644, 2946 |
| SolsticeRing | 1646, 2956 |
| VivaldisMasterwork | 1648, 2980 |
| BloomCrest | 1650, 2853 |
| GrowthBand | 1652, 2843 |
| PetalShield | 1654, 2833 |
| BlossomEssence | 1656, 2585 |
| DormantSpringCore | 1658, 2521 |
| PetalOfRebirth | 1660, 2653 |
| RainbowPetal | 1662, 2671 |
| SpringResonantEnergy | 1664, 2621 |
| VernalBar | 1666, 2553 |
| VernalDust | 1668, 2662 |
| RadiantCrown | 1670, 2881 |
| SunfirePendant | 1672, 2862 |
| ZenithBand | 1674, 2872 |
| DormantSummerCore | 1676, 2529 |
| EmberOfIntensity | 1678, 2679 |
| HeatScale | 1680, 2697 |
| SolarEssence | 1682, 2594 |
| SolsticeBar | 1684, 2561 |
| SummerResonantEnergy | 1686, 2629 |
| SunfireCore | 1688, 2688 |
| FrostbiteAmulet | 1694, 2918 |
| GlacialHeart | 1696, 2937 |
| StillnessShrine | 1698, 2928 |
| DormantWinterCore | 1700, 2545 |
| FrostEssence | 1702, 2612 |
| FrozenCore | 1704, 2751 |
| IcicleCoronet | 1706, 2760 |
| PermafrostBar | 1708, 2577 |
| PermafrostShard | 1710, 2769 |
| ShardOfStillness | 1712, 2742 |
| WinterResonantEnergy | 1714, 2645 |
| EnigmasVoidShell | 1724, 3995 |
| FatesCosmicAegis | 1726, 4021 |
| HarvestThornedGuard | 1728, 3921 |
| HeroicValorsAegis | 1730, 3969 |
| InfernalBellsFortress | 1732, 3982 |
| MoonlitGuardiansVeil | 1734, 3956 |
| PermafrostCrystalWard | 1736, 3932 |
| ResonantBarrierCore | 1738, 3887 |
| SolarFlareAegis | 1740, 3909 |
| SpringVitalityShell | 1742, 3898 |
| SwansImmortalGrace | 1744, 4008 |
| VivaldisSeasonalBulwark | 1746, 3944 |
| EnigmasNegativeSpace | 1748, 3603 |
| FatesCosmicReservoir | 1750, 3625 |
| HeroicArcaneSurge | 1754, 3583 |
| InfernalManaInferno | 1756, 3593 |
| MoonlitOverflowStar | 1758, 3573 |
| PermafrostVoidHeart | 1760, 3551 |
| ResonantOverflowGem | 1762, 3511 |
| SpringArcaneConduit | 1766, 3521 |
| SwansBalancedFlow | 1768, 3614 |
| EnigmasDissonance | 1770, 3093 |
| FatesCosmicSymphony | 1772, 3118 |
| HeroicCrescendo | 1776, 3070 |
| InfernalFortissimo | 1778, 3081 |
| MoonlitSonataBand | 1780, 3059 |
| PermafrostCadenceSeal | 1782, 3037 |
| ResonantRhythmBand | 1784, 2994 |
| SpringTempoCharm | 1788, 3004 |
| SwansPerfectMeasure | 1790, 3105 |
| VivaldisTempoMaster | 1792, 3048 |
| EnigmasPhaseShift | 1794, 3852 |
| FatesCosmicVelocity | 1796, 3874 |
| HarvestPhantomStride | 1798, 3788 |
| HeroicChargeBoots | 1800, 3830 |
| InfernalMeteorStride | 1802, 3841 |
| MoonlitPhantomsRush | 1804, 3820 |
| PermafrostAvalancheStep | 1806, 3798 |
| ResonantVelocityBand | 1808, 3756 |
| SolarBlitzTreads | 1810, 3778 |
| SpringZephyrBoots | 1812, 3767 |
| SwansEternalGlide | 1814, 3863 |
| VivaldisSeasonalSprint | 1816, 3809 |
| EnigmasParadoxMark | 1818, 3229 |
| FatesCosmicVerdict | 1820, 3252 |
| HeroicDeadeye | 1824, 3207 |
| InfernalExecutionersBrand | 1826, 3218 |
| MoonlitPredatorsGaze | 1828, 3196 |
| PermafrostHuntersEye | 1830, 3172 |
| ResonantSpotter | 1832, 3131 |
| SpringHuntersLens | 1836, 3141 |
| SwansGracefulHunt | 1838, 3240 |
| VivaldisSeasonalSight | 1840, 3182 |
| EnigmasHivemindLink | 1842, 3725 |
| FatesCosmicDominion | 1844, 3744 |
| HarvestBeastlordsHorn | 1846, 3666 |
| HeroicGeneralsBaton | 1848, 3705 |
| InfernalChoirMastersRod | 1850, 3715 |
| MoonlitSymphonyWand | 1852, 3696 |
| PermafrostCommandersCrown | 1854, 3676 |
| ResonantConductorsWand | 1856, 3636 |
| SpringMaestrosBadge | 1860, 3646 |
| SwansGracefulDirection | 1862, 3735 |
| VivaldisOrchestraBaton | 1864, 3686 |
| ArcaneHarmonicPrism | 1866, 3501 |
| CrystallizedHarmony | 1868, 3491 |
| VivaldisHarmonicCore | 1924, 3562 |

</details>

---

## 5. ORPHANED NPC (1)

| Line | HJSON Key | Correct Class | Issue |
|------|-----------|---------------|-------|
| 4408 | `DiesIraeHeraldOfJudgement` | `DiesIraeHeraldOfJudgment` | Spelling: "Judgement" → "Judgment" |

---

## 6. ORPHANED PROJECTILES (576 out of 988)

**58% of all projectile entries are orphaned.** This is the largest cleanup needed.

### 6a. Theme-Prefixed Projectiles (50 entries, lines 4849–4898)
Pattern: `ClairProj_*`, `DiesIraeProj_*`, `EnigmaProj_*`, `EroicaProj_*`, `FateProj_*`, `CampanellaProj_*`, `MoonlightProj_*`, `NachtmusikProj_*`, `SeasonsProj_*`, `SwanLakeProj_*` — 5 per theme, all orphaned.

### 6b. Test/Example/Foundation/Sandbox Projectiles (~25 entries, lines 4989–5073)
Includes: `EnigmaVoidBeamExample`, `EroicaDeathRayExample`, `VFXTest*`, `VFXPlus*`, `CalamityTrailSlash`, `ShaderTestProjectile`, `SandboxTerraBlade*` variants.

### 6c. Abbreviated Weapon Projectiles (~200 entries, lines 5195–5461)
Two-letter prefix pattern (e.g., `BSBellShockwave`, `HACrescendoLance`, `IRBloodDrop`). These are projectiles for weapons that were removed in a batch refactor. Grouped by weapon abbreviation:

| Prefix | Theme | Count |
|--------|-------|-------|
| DI, HR, IR, RC, WE | Dies Irae | ~20 |
| AC, CE, EP, RR | Enigma | ~20 |
| BF, EB, FH, HA | Eroica | ~16 |
| DC, FL, MG, SF | Fate | ~20 |
| BS, CC, CF, IE | La Campanella | ~20 |
| CLB, SOT, MR, LunarElegy* | Moonlight | ~20 |
| CA, NCB, NC, NM, SB | Nachtmusik | ~20 |
| COC, HB, JC, OE, OJ | Ode to Joy | ~20 |
| CB, OA, SL, SO | Swan Lake | ~20 |

### 6d. Removed Clock/Temporal Projectiles (~40 entries, lines 4920–4951)
Associated with removed Clockwork/Temporal weapons.

### 6e. Removed Weapon-Specific Projectiles (~260 remaining entries)
Various projectiles for weapons that were renamed, removed, or refactored. Spans all themes.

---

## 7. RECOMMENDED ACTIONS

### Priority 1: Fix Encoding (High Impact, Easy)
Replace all mojibake characters:
- ` -- ` → ` — ` (13 occurrences)
- `*` → `★` (6 occurrences)  
- `->` → `→` (3 occurrences in line 3949)

### Priority 2: Fix Spelling Mismatches (Breaks Functionality)
Rename 3 keys from "Judgement" to "Judgment":
- Line 2062: `HarmonyOfJudgement` → `HarmonyOfJudgment`
- Line 2066: `StaffOfFinalJudgement` → `StaffOfFinalJudgment`
- Line 4408: `DiesIraeHeraldOfJudgement` → `DiesIraeHeraldOfJudgment`

### Priority 3: Delete Duplicate Stubs (Clutter)
Remove 154 empty stub entries in lines ~1496–1924. These are all superseded by complete entries later in the file.

### Priority 4: Delete Orphaned Entries (Major Cleanup)
- Delete 134 orphaned item entries
- Delete 576 orphaned projectile entries
- Delete 2 orphaned projectile entries for `HarmonyOfJudgementMinion` and `JudgementRay`

### Priority 5: Investigate Item-Suffix Orphans
Check whether `InfernalCleaverItem`, `FrostbiteEdgeItem`, `CosmicRendBladeItem`, `VerdantCrescendoItem`, `ArcaneHarmonicsItem` should be renamed to match their class names (likely just drop the `Item` suffix).
