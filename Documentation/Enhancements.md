# MagnumOpus Enhancements - Phased Implementation Guide

> **This document is organized by IMPLEMENTATION PRIORITY, not gameplay progression.**
> Complete each phase before moving to the next. Foundation items unlock everything else.

---

# 📋 PHASE OVERVIEW

| Phase | Focus | Asset Count | Priority | Status |
|-------|-------|-------------|----------|--------|
| **Phase 1** | Foundation Materials (Bars, Essences, Enemy Drops) | 41 items | 🔴 CRITICAL | ✅ COMPLETE |
| **Phase 2** | Four Seasons Content (Bosses + Base Accessories) | ~20 items | 🟠 HIGH | ✅ COMPLETE |
| **Phase 3** | Main Theme Expansions (New Materials + Accessories) | ~23 items | 🟡 MEDIUM | ✅ COMPLETE |
| **Phase 4** | Combination Accessories (Multi-theme Combos) | ~10 items | 🟢 LOWER | ✅ COMPLETE |
| **Phase 5** | Fate Tier & Ultimate Items | ~14 items | 🔵 ENDGAME | ✅ COMPLETE |
| **Phase 6** | Utilities & Polish | ~15 items | ⚪ OPTIONAL | ⏳ Pending |

---

# ✅ PHASE 1: FOUNDATION MATERIALS - COMPLETE
*All 41 items implemented with code, textures, recipes, and enemy drops*

## 1.1 Pre-Hardmode World Drops (8 items)

These drop from enemies/chests and are crafting ingredients for everything.

| Item | Source | Sprite Size | Notes |
|------|--------|-------------|-------|
| **Resonant Crystal Shard** | Underground caverns (ore veins) | 16x16 | Glowing crystal, multi-theme |
| **Minor Music Note** | Surface enemies (common) | 12x12 | Simple quarter note |
| **Faded Sheet Music** | Underground chests (rare) | 20x16 | Aged paper scroll |
| **Broken Baton** | Dungeon chests (rare) | 24x8 | Snapped conductor's baton |
| **Tuning Fork** | Cavern enemies (uncommon) | 16x20 | Metal fork, vibration lines |
| **Old Metronome** | Surface chests (uncommon) | 14x18 | Wooden triangle metronome |
| **Rusted Clef** | Ice biome chests (rare) | 12x14 | Treble clef, rusted metal |
| **Dull Resonator** | Jungle chests (rare) | 14x14 | Circular resonating disc |

### Pre-Hardmode Dormant Cores (4 items)
*Found in world-gen chests, become active in Hardmode*

| Item | Found In | Sprite Size | Colors |
|------|----------|-------------|--------|
| **Dormant Spring Core** | Forest/Jungle chests | 18x18 | Pale white, pink/light blue hints |
| **Dormant Summer Core** | Desert/Ocean chests | 18x18 | Faded orange/white |
| **Dormant Autumn Core** | Cavern/Underground chests | 18x18 | Muted white/brown/orange |
| **Dormant Winter Core** | Ice biome chests | 18x18 | White/pale light blue |

---

## 1.2 Four Seasons Bars & Essences (8 items)

**Bars are crafted from enemy drops + essences. NO new world-gen ores.**

### Spring Materials
| Item | Type | Sprite Size | Colors |
|------|------|-------------|--------|
| **Vernal Bar** | Bar | 20x14 | Polished white-pink with blue tint |
| **Spring's Harmonic Essence** | Essence | 14x14 | Pink petal glow, white center |

*Recipe: 3 Petals of Rebirth + 1 Spring's Harmonic Essence = 2 Vernal Bars*

### Summer Materials
| Item | Type | Sprite Size | Colors |
|------|------|-------------|--------|
| **Solstice Bar** | Bar | 20x14 | Radiant orange with white sheen |
| **Summer's Harmonic Essence** | Essence | 14x14 | Orange sun burst, white center |

*Recipe: 3 Embers of Intensity + 1 Summer's Harmonic Essence = 2 Solstice Bars*

### Autumn Materials
| Item | Type | Sprite Size | Colors |
|------|------|-------------|--------|
| **Harvest Bar** | Bar | 20x14 | Polished white-brown with orange tint |
| **Autumn's Harmonic Essence** | Essence | 14x14 | Dark orange glow, white wisps |

*Recipe: 3 Leaves of Ending + 1 Autumn's Harmonic Essence = 2 Harvest Bars*

### Winter Materials
| Item | Type | Sprite Size | Colors |
|------|------|-------------|--------|
| **Permafrost Bar** | Bar | 20x14 | Frosted white with light blue sheen |
| **Winter's Harmonic Essence** | Essence | 14x14 | White snowflake, light blue glow |

*Recipe: 3 Shards of Stillness + 1 Winter's Harmonic Essence = 2 Permafrost Bars*

---

## 1.3 Boss Drop Materials (4 Seasonal Resonant Energies)

*Unique boss-only drops, one per boss. Higher tier crafting ingredient.*

| Item | Boss Source | Sprite Size | Colors |
|------|-------------|-------------|--------|
| **Spring Resonant Energy** | Primavera | 20x20 | White/pink/light blue swirl |
| **Summer Resonant Energy** | L'Estate | 20x20 | Blazing orange/white |
| **Autumn Resonant Energy** | Autunno | 20x20 | White/brown/dark orange fade |
| **Winter Resonant Energy** | L'Inverno | 20x20 | White/light blue crystalline |

*Note: Theme Resonant Energies (Moonlight, Eroica, Campanella, Enigma, Swan Lake, Fate) already exist in the mod*

---

## 1.4 Enemy Drop Materials (24 items)

*Drops from specific enemies - used for bars AND accessories*

### Spring Enemy Drops
| Item | Source | Rate | Sprite Size | Use |
|------|--------|------|-------------|-----|
| **Petal of Rebirth** | Plantera's Tentacles, Jungle HM enemies | 8% / 3% | 14x14 | **Primary bar material** |
| **Vernal Dust** | Jungle enemies (HM) | 5% | 10x10 | Accessory crafting |
| **Rainbow Petal** | Rainbow Slime | 10% | 14x14 | Accessory crafting |

### Summer Enemy Drops
| Item | Source | Rate | Sprite Size | Use |
|------|--------|------|-------------|-----|
| **Ember of Intensity** | Solar Pillar enemies, Lava enemies | 5% / 3% | 14x14 | **Primary bar material** |
| **Sunfire Core** | Mothron | 15% | 16x16 | Accessory crafting |
| **Heat Scale** | Lava Bat, Fire Imp, Hell enemies | 6% | 14x12 | Accessory crafting |

### Autumn Enemy Drops
| Item | Source | Rate | Sprite Size | Use |
|------|--------|------|-------------|-----|
| **Leaf of Ending** | Pumpking, Eclipse enemies | 12% / 3% | 14x14 | **Primary bar material** |
| **Twilight Wing Fragment** | Mothron | 10% | 18x14 | Accessory crafting |
| **Death's Note** | Reaper | 8% | 12x16 | Accessory crafting |
| **Decay Fragment** | Any Eclipse enemy | 4% | 10x10 | Accessory crafting |

### Winter Enemy Drops
| Item | Source | Rate | Sprite Size | Use |
|------|--------|------|-------------|-----|
| **Shard of Stillness** | Ice Queen, HM Ice enemies | 15% / 2% | 14x14 | **Primary bar material** |
| **Frozen Core** | Ice Golem | 20% | 18x18 | Accessory crafting |
| **Icicle Coronet** | Ice Queen | 10% | 22x14 | Accessory crafting |
| **Permafrost Shard** | Any HM Ice enemy | 3% | 10x10 | Accessory crafting |

### Theme Enemy Drops (Post-Moon Lord)
| Item | Source | Rate | Sprite Size |
|------|--------|------|-------------|
| **Lunar Essence** | Moonlight enemies | 2% | 12x12 |
| **Valor Essence** | Eroica enemies | 2% | 12x12 |
| **Bell Essence** | La Campanella enemies | 2% | 12x12 |
| **Mystery Essence** | Enigma enemies | 2% | 12x12 |
| **Grace Essence** | Swan Lake enemies | 2% | 12x12 |
| **Fate Essence** | Fate enemies | 2% | 12x12 |

---

## 1.5 Phase 1 Asset Checklist

✅ **PHASE 1 COMPLETE** - All items implemented with code, textures, recipes, and enemy drops.

```
PRE-HARDMODE DROPS (8 items) ✅ COMPLETE
[X] Resonant Crystal Shard - Content/Materials/Foundation/
[X] Minor Music Note - Content/Materials/Foundation/
[X] Faded Sheet Music - Content/Materials/Foundation/
[X] Broken Baton - Content/Materials/Foundation/
[X] Tuning Fork - Content/Materials/Foundation/
[X] Old Metronome - Content/Materials/Foundation/
[X] Rusted Clef - Content/Materials/Foundation/
[X] Dull Resonator - Content/Materials/Foundation/

DORMANT CORES (4 items) ✅ COMPLETE
[X] Dormant Spring Core - Content/Spring/Materials/
[X] Dormant Summer Core - Content/Summer/Materials/
[X] Dormant Autumn Core - Content/Autumn/Materials/
[X] Dormant Winter Core - Content/Winter/Materials/

SEASONAL BARS (4 items) ✅ COMPLETE
[X] Vernal Bar - Content/Spring/Materials/
[X] Solstice Bar - Content/Summer/Materials/
[X] Harvest Bar - Content/Autumn/Materials/
[X] Permafrost Bar - Content/Winter/Materials/

SEASONAL HARMONIC ESSENCES (4 items) ✅ COMPLETE
[X] Spring's Harmonic Essence (BlossomEssence.cs) - Content/Spring/Materials/ (drops from Plantera)
[X] Summer's Harmonic Essence (SolarEssence.cs) - Content/Summer/Materials/ (drops from Golem)
[X] Autumn's Harmonic Essence (DecayEssence.cs) - Content/Autumn/Materials/ (drops from Pumpking)
[X] Winter's Harmonic Essence (FrostEssence.cs) - Content/Winter/Materials/ (drops from Ice Queen)

SEASONAL RESONANT ENERGIES (4 items) ✅ COMPLETE
[X] Spring Resonant Energy - Content/Spring/Materials/
[X] Summer Resonant Energy - Content/Summer/Materials/
[X] Autumn Resonant Energy - Content/Autumn/Materials/
[X] Winter Resonant Energy - Content/Winter/Materials/

ENEMY DROPS - PRIMARY BAR MATERIALS (4 items) ✅ COMPLETE
[X] Petal of Rebirth - Content/Spring/Materials/ (Plantera's Tentacles, Jungle HM)
[X] Ember of Intensity - Content/Summer/Materials/ (Solar Pillar, Lava enemies)
[X] Leaf of Ending - Content/Autumn/Materials/ (Pumpking, Eclipse enemies)
[X] Shard of Stillness - Content/Winter/Materials/ (Ice Queen, HM Ice enemies)

ENEMY DROPS - ACCESSORY MATERIALS (11 items) ✅ COMPLETE
[X] Vernal Dust - Content/Spring/Materials/
[X] Rainbow Petal - Content/Spring/Materials/
[X] Sunfire Core - Content/Summer/Materials/
[X] Heat Scale - Content/Summer/Materials/
[X] Twilight Wing Fragment - Content/Autumn/Materials/
[X] Death's Note - Content/Autumn/Materials/
[X] Decay Fragment - Content/Autumn/Materials/
[X] Frozen Core - Content/Winter/Materials/
[X] Icicle Coronet - Content/Winter/Materials/
[X] Permafrost Shard - Content/Winter/Materials/

THEME ESSENCES (6 items) ✅ COMPLETE
[X] Lunar Essence - Content/Materials/EnemyDrops/
[X] Valor Essence - Content/Materials/EnemyDrops/
[X] Bell Essence - Content/Materials/EnemyDrops/
[X] Mystery Essence - Content/Materials/EnemyDrops/
[X] Grace Essence - Content/Materials/EnemyDrops/
[X] Fate Essence - Content/Materials/EnemyDrops/

TOTAL PHASE 1: 41 items ✅ ALL IMPLEMENTED
- All .cs files with proper namespaces
- All .png textures in correct locations
- All recipes registered
- Enemy drops configured in Common/Systems/FoundationMaterialDrops.cs
```

---

# 🟠 PHASE 2: FOUR SEASONS CONTENT
*Requires Phase 1 materials*

## 2.1 Four Seasons Bosses (4 bosses)

| Boss | Tier | Sprite Size | Theme Colors |
|------|------|-------------|--------------|
| **Primavera, Herald of Bloom** | Post-Wall of Flesh | 120x120+ | White (#FFFFFF), Pink (#FFB7C5), Light Blue (#ADD8E6) |
| **L'Estate, Lord of the Zenith** | Post-Mech Bosses | 140x140+ | Orange (#FF8C00), White (#FFFFFF) |
| **Autunno, the Withering Maestro** | Post-Plantera | 130x130+ | White (#FFFFFF), Brown (#8B4513), Dark Orange (#FF4500) |
| **L'Inverno, the Silent Finale** | Post-Golem | 150x150+ | White (#FFFFFF), Light Blue (#ADD8E6) |

*Each boss needs: Main sprite, Phase 2 sprite (if applicable), Projectile sprites, Summon item*

---

## 2.2 Seasonal Base Accessories (12 items)

### Spring Accessories
| # | Item | Recipe Summary | Sprite Size |
|---|------|----------------|-------------|
| 4 | **Petal Shield** | 12 Vernal Bars + Petal of Rebirth | 28x28 |
| 5 | **Growth Band** | 10 Vernal Bars + Blossom Essence | 26x26 |
| 6 | **Bloom Crest** | Petal Shield + Growth Band + Spring Resonant Energy | 30x30 |

### Summer Accessories
| # | Item | Recipe Summary | Sprite Size |
|---|------|----------------|-------------|
| 7 | **Sunfire Pendant** | 12 Solstice Bars + Sunfire Core | 28x28 |
| 8 | **Zenith Band** | 10 Solstice Bars + Solar Essence | 26x26 |
| 9 | **Radiant Crown** | Sunfire Pendant + Zenith Band + Summer Resonant Energy | 32x28 |

### Autumn Accessories
| # | Item | Recipe Summary | Sprite Size |
|---|------|----------------|-------------|
| 10 | **Reaper's Charm** | 12 Harvest Bars + Death's Note | 28x28 |
| 11 | **Twilight Ring** | 10 Harvest Bars + Twilight Wing Fragment | 24x24 |
| 12 | **Harvest Mantle** | Reaper's Charm + Twilight Ring + Autumn Resonant Energy | 32x30 |

### Winter Accessories
| # | Item | Recipe Summary | Sprite Size |
|---|------|----------------|-------------|
| 13 | **Frostbite Amulet** | 12 Permafrost Bars + Frozen Core | 28x28 |
| 14 | **Stillness Shrine** | 10 Permafrost Bars + Shard of Stillness | 26x26 |
| 15 | **Glacial Heart** | Frostbite Amulet + Stillness Shrine + Winter Resonant Energy | 30x30 |

---

## 2.3 Seasonal Combination Accessories (4 items)

| # | Item | Recipe Summary | Sprite Size |
|---|------|----------------|-------------|
| 16 | **Relic of the Equinox** | Bloom Crest + Harvest Mantle + Essences | 32x32 |
| 17 | **Solstice Ring** | Radiant Crown + Glacial Heart + Essences | 32x32 |
| 18 | **Cycle of Seasons** | All 4 Base Accessories + Essences | 34x34 |
| 19 | **Vivaldi's Masterwork** | Equinox + Solstice + Cycle + All Seasonal Resonant Energies | 36x36 |

---

## 2.4 Phase 2 Asset Checklist

✅ **PHASE 2 COMPLETE** - All 4 bosses and 16 accessories implemented with code, textures, VFX, and localization.

```
BOSSES (4 bosses - multiple sprites each) ✅ COMPLETE
[X] Primavera (main, projectiles, summon item) - 28,000 HP, Post-WoF
[X] L'Estate (main, projectiles, summon item) - 42,000 HP, Post-Mech
[X] Autunno (main, projectiles, summon item) - 52,000 HP, Post-Plantera
[X] L'Inverno (main, projectiles, summon item) - 88,000 HP, Post-Golem

BASE ACCESSORIES (12 items) ✅ COMPLETE
[X] Petal Shield - Content/Spring/Accessories/
[X] Growth Band - Content/Spring/Accessories/
[X] Bloom Crest - Content/Spring/Accessories/
[X] Sunfire Pendant - Content/Summer/Accessories/
[X] Zenith Band - Content/Summer/Accessories/
[X] Radiant Crown - Content/Summer/Accessories/
[X] Reaper's Charm - Content/Autumn/Accessories/
[X] Twilight Ring - Content/Autumn/Accessories/
[X] Harvest Mantle - Content/Autumn/Accessories/
[X] Frostbite Amulet - Content/Winter/Accessories/
[X] Stillness Shrine - Content/Winter/Accessories/
[X] Glacial Heart - Content/Winter/Accessories/

COMBINATION ACCESSORIES (4 items) ✅ COMPLETE
[X] Relic of the Equinox - Content/Seasons/Accessories/
[X] Solstice Ring - Content/Seasons/Accessories/
[X] Cycle of Seasons - Content/Seasons/Accessories/
[X] Vivaldi's Masterwork - Content/Seasons/Accessories/

TOTAL PHASE 2: 4 bosses + 16 accessories ✅ ALL IMPLEMENTED
- All .cs files with proper namespaces
- All .png textures in correct locations
- Boss stats balanced to vanilla progression
- All recipes registered
- Full localization in en-US_Mods.MagnumOpus.hjson
```

---

# 🟡 PHASE 3: MAIN THEME EXPANSIONS
*Adds new materials and accessories to existing themes*

## 3.1 Pre-Hardmode Base Accessories (3 items)

| # | Item | Recipe Summary | Sprite Size |
|---|------|----------------|-------------|
| 1 | **Composer's Notebook** | 5 Resonant Crystal Shards + Faded Sheet Music | 22x26 |
| 2 | **Resonant Pendant** | Tuning Fork + Dull Resonator + 3 Shards | 20x24 |
| 3 | **Melodic Charm** | Composer's Notebook + Resonant Pendant | 26x26 |

## 3.2 Theme Resonance Materials

> **NOTE:** Theme Resonance Ores and Cores already exist in the mod. Use the existing items:
> - **Moonlight Sonata:** MoonlitResonanceOre, ResonantCoreOfMoonlightSonata
> - **Eroica:** EroicaResonanceOre, ResonantCoreOfEroica  
> - **La Campanella:** LaCampanellaResonanceOre, ResonantCoreOfLaCampanella
> - **Enigma:** EnigmaResonanceOre, ResonantCoreOfEnigma
> - **Swan Lake:** SwanLakeResonanceOre, ResonantCoreOfSwanLake
> - **Fate:** FateResonanceOre, ResonantCoreOfFate

## 3.3 Theme Accessories (10 items)

### Moonlight Sonata
| # | Item | Sprite Size |
|---|------|-------------|Phase 
| 20 | **Adagio Pendant** | 28x28 |
| 21 | **Sonata's Embrace** | 32x32 |

### Eroica
| # | Item | Sprite Size |
|---|------|-------------|
| 22 | **Badge of Valor** | 28x28 |
| 23 | **Hero's Symphony** | 32x32 |

### La Campanella
| # | Item | Sprite Size |
|---|------|-------------|
| 24 | **Chime of Flames** | 28x28 |
| 25 | **Infernal Virtuoso** | 32x32 |

### Enigma Variations
| # | Item | Sprite Size |
|---|------|-------------|
| 26 | **Puzzle Fragment** | 28x28 |
| 27 | **Riddle of the Void** | 32x32 |

### Swan Lake
| # | Item | Sprite Size |
|---|------|-------------|
| 28 | **Plume of Elegance** | 28x28 |
| 29 | **Swan's Chromatic Diadem** | 32x32 |

---

## 3.4 Phase 3 Asset Checklist

✅ **PHASE 3 COMPLETE** - All theme accessories implemented with code, VFX, recipes, and localization.

```
PRE-HARDMODE ACCESSORIES (3 items) ✅ COMPLETE
[X] Composer's Notebook - Content/Common/Accessories/
[X] Resonant Pendant - Content/Common/Accessories/
[X] Melodic Charm - Content/Common/Accessories/

THEME ACCESSORIES (10 items) ✅ COMPLETE
[X] Adagio Pendant - Content/MoonlightSonata/Accessories/
[X] Sonata's Embrace - Content/MoonlightSonata/Accessories/
[X] Badge of Valor - Content/Eroica/Accessories/
[X] Hero's Symphony - Content/Eroica/Accessories/
[X] Chime of Flames - Content/LaCampanella/Accessories/
[X] Infernal Virtuoso - Content/LaCampanella/Accessories/
[X] Puzzle Fragment - Content/EnigmaVariations/Accessories/
[X] Riddle of the Void - Content/EnigmaVariations/Accessories/
[X] Plume of Elegance - Content/SwanLake/Accessories/
[X] Swan's Chromatic Diadem - Content/SwanLake/Accessories/

TOTAL PHASE 3: 13 items ✅ ALL IMPLEMENTED
```

---

# 🟢 PHASE 4: COMBINATION ACCESSORIES
*Requires Phase 2 & 3 accessories*

## 4.1 Two-Theme Combinations (6 items)

| # | Item | Combines | Sprite Size |
|---|------|----------|-------------|
| 30 | **Nocturne of Azure Flames** | Moonlight + La Campanella | 34x34 |
| 31 | **Valse Macabre** | Eroica + Enigma | 34x34 |
| 32 | **Reverie of the Silver Swan** | Moonlight + Swan Lake | 34x34 |
| 33 | **Fantasia of Burning Grace** | La Campanella + Swan Lake | 34x34 |
| 34 | **Triumphant Arabesque** | Eroica + Swan Lake | 34x34 |
| 35 | **Inferno of Lost Shadows** | La Campanella + Enigma | 34x34 |

## 4.2 Three-Theme Combinations (4 items)

| # | Item | Combines | Sprite Size |
|---|------|----------|-------------|
| 36 | **Trinity of Night** | Moonlight + Campanella + Enigma | 36x36 |
| 37 | **Adagio of Radiant Valor** | Eroica + Moonlight + Swan Lake | 36x36 |
| 38 | **Requiem of the Enigmatic Flame** | Campanella + Enigma + Swan Lake | 36x36 |
| 39 | **Complete Harmony** | All 5 Themes | 38x38 |

---

## 4.3 Phase 4 Asset Checklist

✅ **PHASE 4 COMPLETE** - All combination accessories implemented with code, VFX, recipes, and localization.

```
TWO-THEME COMBOS (6 items) ✅ COMPLETE
[X] Nocturne of Azure Flames - Content/Common/Accessories/TwoThemeCombinationAccessories.cs
[X] Valse Macabre - Content/Common/Accessories/TwoThemeCombinationAccessories.cs
[X] Reverie of the Silver Swan - Content/Common/Accessories/TwoThemeCombinationAccessories.cs
[X] Fantasia of Burning Grace - Content/Common/Accessories/TwoThemeCombinationAccessories.cs
[X] Triumphant Arabesque - Content/Common/Accessories/TwoThemeCombinationAccessories.cs
[X] Inferno of Lost Shadows - Content/Common/Accessories/TwoThemeCombinationAccessories.cs

THREE-THEME COMBOS (4 items) ✅ COMPLETE
[X] Trinity of Night - Content/Common/Accessories/ThreeThemeCombinationAccessories.cs
[X] Adagio of Radiant Valor - Content/Common/Accessories/ThreeThemeCombinationAccessories.cs
[X] Requiem of the Enigmatic Flame - Content/Common/Accessories/ThreeThemeCombinationAccessories.cs
[X] Complete Harmony - Content/Common/Accessories/ThreeThemeCombinationAccessories.cs

TOTAL PHASE 4: 10 accessories ✅ ALL IMPLEMENTED
```

---

# 🔵 PHASE 5: FATE TIER & ULTIMATE
*Endgame content, requires everything previous*

## 5.1 Fate Vanilla Upgrade Accessories (5 items)

*Combines vanilla accessories with Fate materials*

| # | Item | Vanilla Components | Sprite Size | Status |
|---|------|-------------------|-------------|--------|
| 40 | **Paradox Chronometer** | Melee Accessory - Fate Materials | 36x36 | ✅ DONE |
| 41 | **Constellation Compass** | Ranged Accessory - Fate Materials | 34x34 | ✅ DONE |
| 42 | **Astral Conduit** | Magic Accessory - Fate Materials | 32x32 | ✅ DONE |
| 43 | **Machination of the Event Horizon** | Master Ninja Gear + Terraspark Boots + Frog Leg + Fate Materials | 36x36 | ✅ DONE |
| 44 | **Orrery of Infinite Orbits** | Summon Accessory - Fate Materials | 38x38 | ✅ DONE |

## 5.2 Grand Combinations (5 items)

| # | Item | Combines | Sprite Size |
|---|------|----------|-------------|
| 45 | **Opus of Four Movements** | Complete Harmony + Vivaldi's Masterwork | 40x40 |
| 46 | **Cosmic Warden's Regalia** | All 5 Fate Vanilla Upgrades | 42x42 |
| 47 | **Seasonal Destiny** | Vivaldi's Masterwork + Paradox Chronometer | 38x38 |
| 48 | **Theme Wanderer** | Complete Harmony + Machination of the Event Horizon | 38x38 |
| 49 | **Summoner's Magnum Opus** | Complete Harmony + Orrery of Infinite Orbits | 40x40 |

## 5.3 Season + Theme Hybrids (3 items)

| # | Item | Combines | Sprite Size |
|---|------|----------|-------------|
| 50 | **Spring's Moonlit Garden** | Bloom Crest + Sonata's Embrace | 36x36 |
| 51 | **Summer's Infernal Peak** | Radiant Crown + Infernal Virtuoso | 36x36 |
| 52 | **Winter's Enigmatic Silence** | Glacial Heart + Riddle of the Void | 36x36 |

## 5.4 THE ULTIMATE (1 item)

| # | Item | Recipe | Sprite Size |
|---|------|--------|-------------|
| 53 | **Coda of Absolute Harmony** | Opus + Regalia + All Hybrids + Coda of Annihilation (sacrifice) | 48x48 |

---

## 5.5 Phase 5 Asset Checklist

✅ **PHASE 5 COMPLETE** - All Fate tier and ultimate accessories implemented with code, VFX, recipes, and localization.

```
FATE VANILLA UPGRADES (5 items) ✅ COMPLETE
[X] Paradox Chronometer (Melee) - Content/Fate/Accessories/
[X] Constellation Compass (Ranged) - Content/Fate/Accessories/
[X] Astral Conduit (Magic) - Content/Fate/Accessories/
[X] Machination of the Event Horizon (Movement) - Content/Fate/Accessories/
[X] Orrery of Infinite Orbits (Summon) - Content/Fate/Accessories/

GRAND COMBINATIONS (5 items) ✅ COMPLETE
[X] Opus of Four Movements - Content/Common/Accessories/GrandCombinationAccessories.cs
[X] Cosmic Warden's Regalia - Content/Common/Accessories/GrandCombinationAccessories.cs
[X] Seasonal Destiny - Content/Common/Accessories/GrandCombinationAccessories.cs
[X] Theme Wanderer - Content/Common/Accessories/GrandCombinationAccessories.cs
[X] Summoner's Magnum Opus - Content/Common/Accessories/GrandCombinationAccessories.cs

SEASON-THEME HYBRIDS (3 items) ✅ COMPLETE
[X] Spring's Moonlit Garden - Content/Common/Accessories/SeasonThemeHybridAccessories.cs
[X] Summer's Infernal Peak - Content/Common/Accessories/SeasonThemeHybridAccessories.cs
[X] Winter's Enigmatic Silence - Content/Common/Accessories/SeasonThemeHybridAccessories.cs

ULTIMATE (1 item) ✅ COMPLETE
[X] Coda of Absolute Harmony - Content/Common/Accessories/UltimateAccessory.cs

TOTAL PHASE 5: 14 accessories ✅ ALL IMPLEMENTED
```

---

# ⚪ PHASE 6: UTILITIES & POLISH
*Optional quality-of-life items*

## 6.1 Informational Items (5 items)

| # | Item | Function | Sprite Size |
|---|------|----------|-------------|
| 54 | **Harmonic Tuner** | Detailed damage numbers | 18x22 |
| 55 | **Composer's Lens** | Enemy info display | 20x20 |
| 56 | **Fate's Metronome** | Attack speed BPM display | 16x22 |
| 57 | **Seasonal Calendar** | Season bonus tracker | 24x20 |
| 58 | **Symphony Analyzer** | Full combat analysis | 26x24 |

## 6.2 Consumable Potions (5 items)

| # | Item | Effect | Sprite Size |
|---|------|--------|-------------|
| 59 | **Potion of Resonance** | +10% damage, 8 min | 12x20 |
| 60 | **Greater Potion of Resonance** | +20% damage, +10% crit | 14x22 |
| 61 | **Elixir of the Maestro** | +35% damage, +20% crit, +15% speed | 16x24 |
| 62 | **Seasonal Tonic** | Current season +25% | 14x22 |
| 63 | **Cosmic Draught** | All bonuses, 15 min, 1/day | 18x26 |

## 6.3 Permanent Upgrades (5 items)

| # | Item | Source | Sprite Size |
|---|------|--------|-------------|
| 64 | **Crystallized Harmony** | Any MagnumOpus boss (5%) | 16x16 |
| 65 | **Seasonal Attunement** | Four Seasons bosses (10%) | 18x18 |
| 66 | **Theme Resonance Crystal** | Theme bosses (5%) | 18x18 |
| 67 | **Fate's Blessing** | Fate boss (100%, once) | 20x20 |
| 68 | **Coda's Echo** | Fate boss after Coda (25%) | 22x22 |

---

## 6.4 Phase 6 Asset Checklist

```
INFORMATIONAL (5 items)
[ ] Harmonic Tuner
[ ] Composer's Lens
[ ] Fate's Metronome
[ ] Seasonal Calendar
[ ] Symphony Analyzer

POTIONS (5 items)
[ ] Potion of Resonance
[ ] Greater Potion of Resonance
[ ] Elixir of the Maestro
[ ] Seasonal Tonic
[ ] Cosmic Draught

PERMANENT UPGRADES (5 items)
[ ] Crystallized Harmony
[ ] Seasonal Attunement
[ ] Theme Resonance Crystal
[ ] Fate's Blessing
[ ] Coda's Echo

TOTAL PHASE 6: 15 item sprites
```

---

# 📊 COMPLETE ASSET SUMMARY

| Phase | Items | Priority |
|-------|-------|----------|
| Phase 1 - Foundation Materials | ~41 items | 🔴 DO FIRST |
| Phase 2 - Four Seasons | 4 bosses + 16 accessories | 🟠 SECOND |
| Phase 3 - Theme Expansions | 23 items | 🟡 THIRD |
| Phase 4 - Combinations | 10 accessories | 🟢 FOURTH |
| Phase 5 - Fate & Ultimate | 14 accessories | 🔵 FIFTH |
| Phase 6 - Utilities | 15 items | ⚪ LAST |

**TOTAL: ~119 new item sprites + 4 boss sprite sets (NO new ore tiles)**

---

# 🎨 COLOR REFERENCE

| Theme | Primary | Secondary | Accent |
|-------|---------|-----------|--------|
| **Spring** | #FFFFFF (White) | #FFB7C5 (Pink) | #ADD8E6 (Light Blue) |
| **Summer** | #FF8C00 (Orange) | #FFFFFF (White) | #FFD700 (Gold) |
| **Autumn** | #FFFFFF (White) | #8B4513 (Brown) | #FF4500 (Dark Orange) |
| **Winter** | #FFFFFF (White) | #ADD8E6 (Light Blue) | #E0FFFF (Pale Cyan) |
| **Moonlight** | #4B0082 (Purple) | #C0C0C0 (Silver) | #87CEEB (Blue) |
| **Eroica** | #8B0000 (Scarlet) | #DC143C (Crimson) | #FFD700 (Gold) |
| **Campanella** | #1E1419 (Black) | #FF8C00 (Orange) | #DAA520 (Gold) |
| **Enigma** | #501478 (Purple) | #0F0A14 (Black) | #32DC64 (Green) |
| **Swan Lake** | #FFFFFF (White) | #1E1E28 (Black) | Rainbow |
| **Fate** | #080314 (Black) | #B43264 (Pink) | #FF3C50 (Red) |

---

# 🖼️ MIDJOURNEY PROMPT TEMPLATES

> **STYLE MANDATE:** All prompts follow this format for consistency:
> "Concept art for a side-view idle pixel art sprite of [ITEM] made of [MATERIALS] with [ACCENTS] created by music in the style of Terraria, [DESCRIPTION], detailed, ornate design like a royal mechanism, full-view --v 7.0"

---

## 🎵 FOUNDATION MATERIALS (Phase 1)

### Pre-Hardmode World Drops

**Resonant Crystal Shard** (16x16)
```
Concept art for a side-view idle pixel art sprite of an ancient celestial crystal shard themed around "harmonic resonance" made of purple and pink crystal with silver and gold accents created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in soft purple and white ethereal flames, tiny music notes and resonant sparkles float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Minor Music Note** (12x12)
```
Concept art for a side-view idle pixel art sprite of an ancient celestial music note themed around "musical harmony" made of golden and white metal with silver accents created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in soft golden and white magical flames, tiny stars and musical sparkles float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Faded Sheet Music** (20x16)
```
Concept art for a side-view idle pixel art sprite of an ancient celestial sheet music themed around "forgotten melodies" made of cream and brown parchment with gold and silver accents created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in soft golden and brown nostalgic flames, ghostly music notes and faded stars float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Broken Baton** (24x8)
```
Concept art for a side-view idle pixel art sprite of an ancient celestial broken baton themed around "conductor's legacy" made of brown mahogany and ivory with silver and gold accents created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in soft golden and brown magical sparks, musical energy and fading notes float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Tuning Fork** (16x20)
```
Concept art for a side-view idle pixel art sprite of an ancient celestial tuning fork themed around "perfect harmony" made of silver and blue metal with gold accents created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in soft blue and silver resonant waves, sound waves and harmonic rings float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Old Metronome** (14x18)
```
Concept art for a side-view idle pixel art sprite of an ancient celestial metronome themed around "eternal rhythm" made of brown oak and brass with silver and gold accents created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in soft brass and golden temporal flames, clockwork gears and rhythmic energy float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Rusted Clef** (12x14)
```
Concept art for a side-view idle pixel art sprite of an ancient celestial treble clef themed around "frozen memories" made of rusted gold and ice blue metal with silver frost accents created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in soft blue and white frozen flames, ice crystals and frost patterns float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Dull Resonator** (14x14)
```
Concept art for a side-view idle pixel art sprite of an ancient celestial resonator disc themed around "dormant power" made of bronze and green-tinted metal with silver ring accents created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in soft bronze and green dormant flames, concentric rings and sleeping energy float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

### Pre-Hardmode Dormant Cores

**Dormant Spring Core** (18x18)
```
Concept art for a side-view idle pixel art sprite of an ancient celestial orb themed around "Spring" made of pale pink and white crystal with light blue and silver accents created by music in the style of Terraria, radiating a gentle aura, music notes surround it, soft pink and white dormant flames flicker within, cherry blossom petals and tiny buds float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Dormant Summer Core** (18x18)
```
Concept art for a side-view idle pixel art sprite of an ancient celestial orb themed around "Summer" made of pale orange and white crystal with golden and brass accents created by music in the style of Terraria, radiating a gentle aura, music notes surround it, soft orange and gold dormant embers flicker within, sun rays and heat wisps float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Dormant Autumn Core** (18x18)
```
Concept art for a side-view idle pixel art sprite of an ancient celestial orb themed around "Autumn" made of brown and dark orange crystal with copper and silver accents created by music in the style of Terraria, radiating a gentle aura, music notes surround it, soft brown and orange dormant twilight flames flicker within, falling leaves and harvest wisps float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Dormant Winter Core** (18x18)
```
Concept art for a side-view idle pixel art sprite of an ancient celestial orb themed around "Winter" made of white and light blue crystal with platinum and silver accents created by music in the style of Terraria, radiating a gentle aura, music notes surround it, soft blue and white dormant frost flames flicker within, snowflakes and ice crystals float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

### Seasonal Bars

**Vernal Bar** (20x14)
*Recipe: 3 Petals of Rebirth + 1 Blossom Essence @ Mythril Anvil = 2 Vernal Bars*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial metal bar themed around "Spring" made of pink and white metal with light blue veins and silver accents created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in soft pink and white spring flames, cherry blossom petals and flower buds float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Solstice Bar** (20x14)
*Recipe: 3 Embers of Intensity + 1 Solar Essence @ Mythril Anvil = 2 Solstice Bars*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial metal bar themed around "Summer" made of orange and white-hot metal with golden sun patterns and silver accents created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in blazing orange and golden summer flames, solar flares and heat waves float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Harvest Bar** (20x14)
*Recipe: 3 Leaves of Ending + 1 Decay Essence @ Mythril Anvil = 2 Harvest Bars*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial metal bar themed around "Autumn" made of brown and dark orange metal with copper veins and silver accents created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in warm brown and orange autumn flames, falling leaves and twilight wisps float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Permafrost Bar** (20x14)
*Recipe: 3 Shards of Stillness + 1 Frost Essence @ Mythril Anvil = 2 Permafrost Bars*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial metal bar themed around "Winter" made of white and light blue ice-metal with frost patterns and platinum accents created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in serene blue and white winter frost flames, snowflakes and ice crystals float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

### Seasonal Essences

**Blossom Essence** (14x14)
*Source: Crafted from 10 Petals of Rebirth + 5 Vernal Dust @ Mythril Anvil*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial essence orb themed around "Spring" made of brilliant pink and white swirling energy with light blue sparkles and silver accents created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in vibrant pink and white spring flames, cherry blossoms and morning dew droplets float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Solar Essence** (14x14)
*Source: Crafted from 10 Embers of Intensity + 5 Heat Scales @ Mythril Anvil*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial essence orb themed around "Summer" made of blazing orange and white-hot swirling energy with golden corona and silver accents created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in intense orange and golden solar flames, solar flares and sunspots float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Decay Essence** (14x14)
*Source: Crafted from 10 Leaves of Ending + 5 Decay Fragments @ Mythril Anvil*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial essence orb themed around "Autumn" made of dark orange and brown swirling energy with copper wisps and silver accents created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in warm orange and brown autumn twilight flames, falling leaves and fading wisps float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Frost Essence** (14x14)
*Source: Crafted from 10 Shards of Stillness + 5 Permafrost Shards @ Mythril Anvil*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial essence orb themed around "Winter" made of white and light blue swirling frost energy with ice fractals and platinum accents created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in serene blue and white winter frost flames, snowflakes and ice shards float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

### Seasonal Resonant Energies (Boss Drops)

**Spring Resonant Energy** (20x20)
*Source: 100% drop from Primavera, Herald of Bloom*
```
Concept art for a side-view idle pixel art sprite of an ancient divine essence orb themed around "Spring" made of brilliant pink and white and light blue swirling divine energy with silver floral filigree accents created by music in the style of Terraria, radiating a powerful divine aura, music notes surround it, ignited in radiant pink and white spring flames, cherry blossoms and symbols of rebirth float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Summer Resonant Energy** (20x20)
*Source: 100% drop from L'Estate, Lord of the Zenith*
```
Concept art for a side-view idle pixel art sprite of an ancient divine essence orb themed around "Summer" made of explosive orange and white and golden swirling divine solar energy with golden corona accents created by music in the style of Terraria, radiating a powerful divine aura, music notes surround it, ignited in blinding orange and golden summer solar flames, solar flares and heat waves float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Autumn Resonant Energy** (20x20)
*Source: 100% drop from Autunno, the Withering Maestro*
```
Concept art for a side-view idle pixel art sprite of an ancient divine essence orb themed around "Autumn" made of haunting brown and dark orange and white swirling divine twilight energy with silver twilight accents created by music in the style of Terraria, radiating a powerful divine aura, music notes surround it, ignited in warm brown and orange autumn twilight flames, falling leaves and twilight memories float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Winter Resonant Energy** (20x20)
*Source: 100% drop from L'Inverno, the Silent Finale*
```
Concept art for a side-view idle pixel art sprite of an ancient divine essence orb themed around "Winter" made of crystalline white and light blue swirling divine frost energy with platinum filigree accents created by music in the style of Terraria, radiating a powerful divine aura, music notes surround it, ignited in serene blue and white winter frost flames, snowflakes and ice crystals float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

### Primary Enemy Drop Materials

**Petal of Rebirth** (14x14)
*Source: 8% from Plantera's Tentacles, 3% from Hardmode Jungle enemies*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial petal themed around "Spring" made of luminous pink petal energy with white edges and light blue veins and silver accents created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in soft pink and white spring flames, tiny flower buds and renewal sparkles float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Ember of Intensity** (14x14)
*Source: 5% from Solar Pillar enemies, 3% from Lava enemies*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial ember themed around "Summer" made of blazing orange crystal with white-hot core and golden flare accents created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in intense orange and golden solar flames, sparks and heat waves float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Leaf of Ending** (14x14)
*Source: 12% from Pumpking, 3% from Solar Eclipse enemies*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial leaf themed around "Autumn" made of withered white and brown leaf with dark orange veins and silver twilight accents created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in warm brown and orange twilight flames, decay patterns and fading wisps float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Shard of Stillness** (14x14)
*Source: 15% from Ice Queen, 2% from Hardmode Ice enemies*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial ice shard themed around "Winter" made of crystalline white ice with light blue frost patterns and silver filigree accents created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in serene blue and white frost flames, snowflakes and frozen notes float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

### Secondary Enemy Drop Materials

**Vernal Dust** (10x10)
*Source: 5% from Hardmode Jungle enemies*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial dust pile themed around "Spring" made of sparkling pink and white particles with light blue sparkles and silver pollen accents created by music in the style of Terraria, radiating a gentle aura, music notes surround it, soft pink and white spring sparkles drift around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Rainbow Petal** (14x14)
*Source: 10% from Rainbow Slime*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial petal themed around "prismatic beauty" made of iridescent rainbow-shifting metal with chromatic surface and silver shimmer accents created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in shifting rainbow and prismatic flames, colorful sparkles and light refractions float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Sunfire Core** (16x16)
*Source: 15% from Mothron*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial core orb themed around "Solar Fury" made of blazing orange sphere with white plasma tendrils and golden corona and silver containment accents created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in intense orange and golden solar flames, miniature solar flares and heat tendrils float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Heat Scale** (14x12)
*Source: 6% from Lava Bat, Fire Imp, Hell enemies*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial scale themed around "Infernal Fire" made of dark red scale with orange ember edges and golden heat lines and silver accents created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in warm red and orange ember flames, heat wisps and fire energy float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Twilight Wing Fragment** (18x14)
*Source: 10% from Mothron*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial wing fragment themed around "Twilight" made of dark brown membrane with orange sunset gradient and white starlight specks and silver dusk accents created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in warm orange and brown twilight flames, sunset colors and starlight wisps float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Death's Note** (12x16)
*Source: 8% from Reaper*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial music note themed around "Endings" made of ghostly white note with dark orange decay aura and brown withering edges and silver spectral accents created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in ghostly white and orange fading flames, spectral wisps and decay energy float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Decay Fragment** (10x10)
*Source: 4% from any Solar Eclipse enemy*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial fragment themed around "Entropy" made of dark brown crystallized entropy with orange rot patterns and white bone edges and silver preservation accents created by music in the style of Terraria, radiating a gentle aura, music notes surround it, soft brown and orange decay wisps drift around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Frozen Core** (18x18)
*Source: 20% from Ice Golem*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial ice core themed around "Absolute Zero" made of large white ice sphere with light blue crystalline structures and silver frost rune accents created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in serene blue and white frost flames, frozen crystalline formations and ice shards float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Icicle Coronet** (22x14)
*Source: 10% from Ice Queen*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial crown fragment themed around "Winter Royalty" made of elegant white ice with light blue diamond gems and frozen musical symbols and silver royal filigree accents created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in regal blue and white frost flames, ice diamonds and royal frost energy float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Permafrost Shard** (10x10)
*Source: 3% from any Hardmode Ice enemy*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial ice shard themed around "Eternal Frost" made of tiny white crystal with light blue frost veins and silver ice dust accents created by music in the style of Terraria, radiating a gentle aura, music notes surround it, soft blue and white frost sparkles drift around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

### Theme Enemy Drops (Post-Moon Lord)

**Lunar Essence** (12x12)
*Source: 2% from Moonlight Sonata theme enemies*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial essence themed around "Moonlight Sonata" made of deep purple and silver swirling moonlight energy with pale blue crescent accents created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in ethereal purple and silver lunar flames, crescent moons and starlight wisps float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Valor Essence** (12x12)
*Source: 2% from Eroica theme enemies*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial essence themed around "Eroica" made of scarlet and gold swirling heroic energy with silver laurel accents created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in triumphant scarlet and golden heroic flames, sakura petals and triumph symbols float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Bell Essence** (12x12)
*Source: 2% from La Campanella theme enemies*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial essence themed around "La Campanella" made of black and orange swirling infernal bell energy with golden chime accents created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in blazing orange and black infernal flames, infinity bells and fire sparks float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Mystery Essence** (12x12)
*Source: 2% from Enigma Variations theme enemies*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial essence themed around "Enigma Variations" made of deep purple and eerie green swirling mysterious energy with silver question mark accents created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in mysterious purple and green enigmatic flames, watching eyes and question marks float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Grace Essence** (12x12)
*Source: 2% from Swan Lake theme enemies*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial essence themed around "Swan Lake" made of white and black swirling graceful energy with rainbow shimmer and silver feather accents created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in elegant white and black balletic flames, swan feathers and prismatic sparkles float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Fate Essence** (12x12)
*Source: 2% from Fate theme enemies*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial essence themed around "Fate" made of black and dark pink and crimson swirling cosmic destiny energy with white star sparkle accents created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in dark prismatic black and pink celestial flames, ancient glyphs and star particles float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

---

## 🎭 SEASONAL ACCESSORIES (Phase 2)

### Spring Base Accessories

**Petal Shield** (38x38)
*Recipe: 12 Vernal Bars + 5 Petals of Rebirth + 3 Blossom Essence @ Mythril Anvil*
*Effect: +8% damage reduction, taking damage spawns healing petals*
```
Concept art for a side-view idle pixel art sprite of an ancient legendary petal shield made of crystallized cherry blossom energy with ornate white shield frame and pink cherry blossom patterns and light blue gem center and silver filigree trim created by music in the style of Terraria, radiating gentle protective vitality, ornate frame of musical notes and flower petals orbit in gentle spiral around its surface, spring guardian's protection forged from living blossoms, delicate yet impossibly strong with veins of renewal energy pulsing through each petal layer, taking damage causes healing petals to scatter protectively, detailed, ornate design like a royal mechanism, full-view --v 7.0
```

**Growth Band** (38x38)
*Recipe: 10 Vernal Bars + 3 Blossom Essence + 5 Vernal Dust @ Mythril Anvil*
*Effect: +12% life regeneration, standing still causes flowers to bloom around you*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial band themed around "Living Growth" made of polished white gold with pink floral engravings and light blue crystal centerpiece and silver vine accents created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in soft green and pink renewal flames, tiny vines and flower buds float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Bloom Crest** (38x38)
*Recipe: Petal Shield + Growth Band + Spring Resonant Energy @ Mythril Anvil*
*Effect: All spring bonuses combined, +15% damage during daytime*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial crest themed around "Divine Spring" made of magnificent white and pink spring metal with light blue gemstone centerpiece and golden divine frame and silver spring filigree created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in radiant pink and golden morning light flames, cherry blossoms and rays of dawn float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

### Summer Base Accessories

**Sunfire Pendant** (38x38)
*Recipe: 12 Solstice Bars + 1 Sunfire Core + 3 Solar Essence @ Mythril Anvil*
*Effect: +15% damage, attacks inflict "Scorched" burning debuff*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial pendant themed around "Summer's Fury" made of radiant orange gemstone set in white gold frame with golden sun ray prongs and silver heat-resistant chain created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in blazing orange and white solar flames, solar flares and heat waves float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Zenith Band** (38x38)
*Recipe: 10 Solstice Bars + 3 Solar Essence + 5 Heat Scales @ Mythril Anvil*
*Effect: +10% attack speed, moving fast leaves a fire trail*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial band themed around "Solar Peak" made of blazing orange metal with white diamond accents and golden sun symbols and silver cooling runes created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in intense orange and golden blazing flames, heat distortion waves and fire sparks float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Radiant Crown** (38x38)
*Recipe: Sunfire Pendant + Zenith Band + Summer Resonant Energy @ Mythril Anvil*
*Effect: All summer bonuses combined, immune to "On Fire!" debuff, +20% damage at midday*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial crown themed around "Divine Summer" made of golden orange crown metal with white diamond peaks and solar flare gems erupting upward and silver protective runes created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in brilliant golden and white stellar flames, solar coronas and heat waves float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

### Autumn Base Accessories

**Reaper's Charm** (38x38)
*Recipe: 12 Harvest Bars + 1 Death's Note + 3 Decay Essence @ Mythril Anvil*
*Effect: +12% critical strike chance, killing enemies has 5% chance to drop souls*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial charm themed around "Autumn's End" made of haunting white bone carved with brown decay patterns and dark orange runes and silver death symbols created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in eerie orange and pale brown twilight flames, ghostly wisps and falling leaves float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Twilight Ring** (38x38)
*Recipe: 10 Harvest Bars + 1 Twilight Wing Fragment + 5 Decay Fragments @ Mythril Anvil*
*Effect: +8% dodge chance at dusk/dawn, enemies drop more coins*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial ring themed around "Twilight Hour" made of elegant white gold with brown autumn leaf patterns and dark orange sunset gems and silver dusk traces created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in warm orange and purple dusk flames, sunset rays and autumn leaves float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Harvest Mantle** (38x38)
*Recipe: Reaper's Charm + Twilight Ring + Autumn Resonant Energy @ Mythril Anvil*
*Effect: All autumn bonuses combined, +25% damage to enemies below 25% HP*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial mantle themed around "Divine Autumn" made of elegant white and brown mantle metal with dark orange amber gemstones arranged like falling leaves and silver twilight filigree created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in melancholy orange and deep brown decay flames, withered petals and spectral leaves float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

### Winter Base Accessories

**Frostbite Amulet** (38x38)
*Recipe: 12 Permafrost Bars + 1 Frozen Core + 3 Frost Essence @ Mythril Anvil*
*Effect: +15 defense, attacks inflict "Frostburn" debuff*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial amulet themed around "Eternal Frost" made of pristine white ice crystal set in light blue frozen metal frame with silver frost patterns and platinum chain created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in serene blue and white frost flames, snowflakes and ice crystals float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Stillness Shrine** (38x38)
*Recipe: 10 Permafrost Bars + 5 Shards of Stillness + 5 Permafrost Shards @ Mythril Anvil*
*Effect: Standing still grants +20% damage reduction, enemies nearby are slowed*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial band themed around "Perfect Silence" made of crystalline white ice metal with light blue ice gem and frozen silver filigree created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in calm white and pale blue stillness flames, frozen time particles and suspended snowflakes float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Glacial Heart** (38x38)
*Recipe: Frostbite Amulet + Stillness Shrine + Winter Resonant Energy @ Mythril Anvil*
*Effect: All winter bonuses combined, brief invincibility when HP drops below 20%*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial heart themed around "Divine Winter" made of crystalline white heart-shaped gem with light blue ice veins spreading from center and silver divine frame created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in pristine white and ethereal blue absolute zero flames, frozen crystals and gentle snowflakes float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

### Seasonal Combination Accessories

**Relic of the Equinox** (38x38)
*Recipe: Bloom Crest + Harvest Mantle + 5 each Blossom & Decay Essence @ Ancient Manipulator*
*Effect: Spring and Autumn bonuses, damage boost at dawn and dusk*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial band themed around "Perfect Balance" made of harmoniously blended white-pink spring blossoms on one half and white-brown autumn leaves on the other with silver equilibrium line and golden balance gem at center created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in pink and orange transitional flames, cherry blossoms transforming into withered leaves float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Solstice Ring** (38x38)
*Recipe: Radiant Crown + Glacial Heart + 5 each Solar & Frost Essence @ Ancient Manipulator*
*Effect: Summer and Winter bonuses, damage boost at noon and midnight*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial ring themed around "Extreme Duality" made of blazing orange summer flames on one half fused with crystalline white-blue winter ice on the other with platinum steam line where elements meet and diamond dual-core gem at center created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in impossible fire and frost flames together, solar flares and frozen icicles float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Cycle of Seasons** (38x38)
*Recipe: Bloom Crest + Radiant Crown + Harvest Mantle + Glacial Heart + 10 of each Seasonal Essence @ Ancient Manipulator*
*Effect: All four seasonal bonuses at reduced strength, bonuses rotate every 5 minutes*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial cycle themed around "Eternal Rotation" made of four seamlessly blended quadrants of pink spring and white summer and brown autumn and blue winter flowing into each other with golden celestial frame and rainbow prismatic gem at center created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in cycling seasonal flames of all four colors, cherry blossoms and flames and leaves and snow float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Vivaldi's Masterwork** (38x38)
*Recipe: Relic of the Equinox + Solstice Ring + Cycle of Seasons + All 4 Seasonal Resonant Energies @ Ancient Manipulator*
*Effect: Full strength of all seasons simultaneously, The Four Seasons plays faintly*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial masterwork themed around "The Four Seasons" made of celestial gold frame containing swirling vortex of all four seasons in perfect harmony with pink spring blossoms and orange summer flames and brown autumn leaves and blue winter snowflakes eternally dancing together with diamond constellation gems created by music in the style of Terraria, radiating a transcendent aura, music notes surround it, ignited in all four seasonal flames unified in symphony, elements of spring summer autumn and winter float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

---

## 🎪 BOSS SPRITES (Phase 2)

**Primavera, Herald of Bloom** (120x120+)
*Summoned with: Blossom Wreath (crafted from 20 Petals of Rebirth + 10 Vernal Bars + Cherry Blossom obtained in Jungle)*
*Spawns: Post-Eye of Cthulhu, in Forest/Jungle biome*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial boss entity themed around "Spring's Awakening" made of living cherry blossoms and white flower petals interwoven with soft light blue morning breeze and pink gemstone accents created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in gentle pink and green renewal flames, cherry blossom petals and morning dew and flower buds float around it and are apart of its design, detailed boss sprite, silver ornate design like a royal mechanism, full-view --v 7.0
```

**L'Estate, Lord of the Zenith** (140x140+)
*Summoned with: Solar Medallion (crafted from 20 Embers of Intensity + 15 Solstice Bars + Sun Stone)*
*Spawns: Post-Skeletron, during daytime in Desert/Ocean*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial boss entity themed around "Summer's Peak" made of pure solar flame and brilliant orange fire interwoven with white-hot plasma currents and golden crystallized sunlight armor created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in blazing orange and golden stellar flames, solar flares and heat waves and sun coronas float around it and are apart of its design, detailed boss sprite, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Autunno, the Withering Maestro** (130x130+)
*Summoned with: Twilight Score (crafted from 20 Leaves of Ending + 15 Harvest Bars + obtained from Pumpkin Moon)*
*Spawns: Post-Wall of Flesh (Early Hardmode), during evening*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial boss entity themed around "Autumn's Requiem" made of elegant skeletal form draped in flowing white robes with brown decay patterns and dark orange embers along the edges created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in melancholy orange and deep brown twilight flames, falling leaves and ghostly wisps and dying petals float around it and are apart of its design, detailed boss sprite, silver ornate design like a royal mechanism, full-view --v 7.0
```

**L'Inverno, the Silent Finale** (150x150+)
*Summoned with: Frozen Symphony (crafted from 20 Shards of Stillness + 20 Permafrost Bars + Frost Core)*
*Spawns: Post-Mechanical Bosses, in Snow biome*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial boss entity themed around "Winter's Silence" made of towering crystalline pristine white ice and light blue permafrost in perfect geometric stillness with platinum frozen accents created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in serene white and ethereal blue absolute zero flames, frozen snowflakes and ice crystals and suspended frost particles float around it and are apart of its design, detailed boss sprite, silver ornate design like a royal mechanism, full-view --v 7.0
```

---

## 🎼 THEME ACCESSORIES (Phase 3-4)

### Pre-Hardmode Base Accessories

**Composer's Notebook** (38x38)
*Recipe: 5 Resonant Crystal Shards + 1 Faded Sheet Music + 20 Paper @ Work Bench*
*Effect: +5% all damage, shows enemy health bars*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial notebook themed around "Musical Beginnings" made of elegant leather-bound journal with cream pages and golden musical staff lines and soft purple glowing notes drifting from pages with silver pen tucked into binding created by music in the style of Terraria, radiating a gentle aura, music notes surround it, soft purple and gold musical energy floats around it and is apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Resonant Pendant** (38x38)
*Recipe: 1 Tuning Fork + 1 Dull Resonator + 3 Resonant Crystal Shards @ Work Bench*
*Effect: +3% damage, enemies have small chance to drop Minor Music Notes*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial pendant themed around "Harmonic Frequency" made of small circular metal disc with silver tuning fork shape at center and soft purple vibration lines emanating outward and golden chain of linked musical notes created by music in the style of Terraria, radiating a gentle aura, music notes surround it, harmonic sound waves and tuning resonance float around it and is apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Melodic Charm** (38x38)
*Recipe: Composer's Notebook + Resonant Pendant + 5 Minor Music Notes @ Work Bench*
*Effect: Combined bonuses, +8% damage, mana regeneration improved*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial charm themed around "First Melody" made of ornate golden charm shaped like musical note with small purple gem and pages motifs and tuning fork elements integrated with silver accents created by music in the style of Terraria, radiating a gentle aura, music notes surround it, soft purple and golden musical energy floats around it and is apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

### Moonlight Sonata Theme (Post-Moon Lord)

**Adagio Pendant** (38x38)
*Recipe: 15 Moonlit Resonance Cores + Melodic Charm + Moonlight Resonant Energy @ Ancient Manipulator*
*Effect: +12% damage at night, +15% crit chance under moonlight, mana costs reduced 10%*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial pendant themed around "Moonlight Sonata" made of elegant deep purple crescent shape with pristine silver accents and pale blue moonstone at center with ghostly music notes created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in ethereal purple and silver lunar flames, crescent moons and starlight wisps float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Sonata's Embrace** (38x38)
*Recipe: Adagio Pendant + 25 Moonlit Resonance Cores + 10 Lunar Essence @ Ancient Manipulator*
*Effect: All Moonlight bonuses maximized, enemies hit are "Moonstruck" (slowed, reduced damage)*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial embrace themed around "Moonlight Sonata" made of elegant purple and silver crescent embracing pale blue lunar gem with stars and moon phases orbiting and translucent melodic waves emanating outward created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in transcendent purple and silver ethereal lunar flames, crescent moons and starlight and melodic waves float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

### Eroica Theme (Post-Moon Lord)

**Badge of Valor** (38x38)
*Recipe: 15 Heroic Resonance Cores + Melodic Charm + Eroica Resonant Energy @ Ancient Manipulator*
*Effect: +15% melee damage, +10% melee speed, brief invulnerability after killing an enemy*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial badge themed around "Eroica" made of scarlet and gold shield-shaped badge with crimson gem center and golden laurel wreath border with tiny sakura petals drifting around created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in triumphant scarlet and golden heroic flames, sakura petals and laurel leaves float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Hero's Symphony** (38x38)
*Recipe: Badge of Valor + 25 Heroic Resonance Cores + 10 Valor Essence @ Ancient Manipulator*
*Effect: All Eroica bonuses maximized, kills trigger "Heroic Surge" (+25% damage for 5s)*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial symphony themed around "Eroica" made of scarlet crimson and brilliant gold crest with heroic phoenix motifs and blazing gems with sakura petals and heroic symbols orbiting created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in legendary scarlet and golden triumphant flames, phoenix feathers and sakura petals and victory symbols float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

### La Campanella Theme (Post-Moon Lord)

**Chime of Flames** (38x38)
*Recipe: 15 Infernal Resonance Cores + Melodic Charm + La Campanella Resonant Energy @ Ancient Manipulator*
*Effect: +15% magic damage, spells leave fire trails, attacks have chance to ring (stun)*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial chime themed around "La Campanella" made of smoky black bell shape with blazing orange flames licking upward and golden accents with visible heat waves rippling outward created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in infernal orange and black hellfire flames, infinity bells and fire sparks float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Infernal Virtuoso** (38x38)
*Recipe: Chime of Flames + 25 Infernal Resonance Cores + 10 Bell Essence @ Ancient Manipulator*
*Effect: All Campanella bonuses maximized, spells ring the bell (AoE fire damage on hit)*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial virtuoso themed around "La Campanella" made of black and orange bell-shaped pendant with golden flame filigree and hellfire gems with flames dancing around like fingers on keys created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in devastating black and orange infernal flames, infinity bells and flame wisps and smoke tendrils float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

### Enigma Variations Theme (Post-Moon Lord)

**Puzzle Fragment** (38x38)
*Recipe: 15 Void Resonance Cores + Melodic Charm + Enigma Resonant Energy @ Ancient Manipulator*
*Effect: +12% all damage, attacks have 8% chance to inflict "Paradox" (random debuff)*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial fragment themed around "Enigma Variations" made of deep purple and void black geometric fragment with eerie green glowing edges and shifting runes with question marks flickering in and out created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in mysterious purple and green enigmatic flames, watching eyes and question marks float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Riddle of the Void** (38x38)
*Recipe: Puzzle Fragment + 25 Void Resonance Cores + 10 Mystery Essence @ Ancient Manipulator*
*Effect: All Enigma bonuses maximized, "Paradox" stacks 5 times then explodes*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial riddle themed around "Enigma Variations" made of shifting geometric artifact of deep purple void black and eerie green flame with watching eyes peering from shadows around its impossible form created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in cosmic purple and green void flames, watching eyes and shifting question marks and void tendrils float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

### Swan Lake Theme (Post-Moon Lord)

**Plume of Elegance** (38x38)
*Recipe: 15 Prismatic Resonance Cores + Melodic Charm + Swan Lake Resonant Energy @ Ancient Manipulator*
*Effect: +10% all damage, +15% movement speed, dodging leaves rainbow afterimages*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial feather themed around "Swan Lake" made of pristine white swan feather with black tip and subtle rainbow prismatic shimmer along edges with graceful curves created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in elegant white and black balletic flames with rainbow shimmer, swan feathers and prismatic sparkles float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Swan's Chromatic Diadem** (38x38)
*Recipe: Plume of Elegance + 25 Prismatic Resonance Cores + 10 Grace Essence @ Ancient Manipulator*
*Effect: All Swan Lake bonuses maximized, perfect dodges trigger "Dying Swan" (massive damage burst)*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial crown themed around "Swan Lake" made of elegant white and black ballet tiara with rainbow prismatic gems catching impossible light and swan feathers framing the piece created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in elegant white and black flames with rainbow prismatic shimmer, swan feathers and rainbow sparkles and graceful trails float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

### Two-Theme Combination Accessories

**Lunar Flames** (38x38)
*Recipe: Sonata's Embrace + Infernal Virtuoso + 15 each Moonlit & Infernal Resonance Cores @ Ancient Manipulator*
*Effect: Moonlight + Campanella bonuses, fire burns blue at night*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial fusion themed around "Moonlight and Fire" made of half deep purple crescent moon fused with half blazing orange bell with silver and gold energies swirling together and blue-tinged flames created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in ethereal blue lunar flames and orange infernal flames, crescent moons and bells and blue fire float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Heroic Enigma** (38x38)
*Recipe: Hero's Symphony + Riddle of the Void + 15 each Heroic & Void Resonance Cores @ Ancient Manipulator*
*Effect: Eroica + Enigma bonuses, heroic kills spread Paradox*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial fusion themed around "Valor and Mystery" made of half scarlet-gold heroic crest fused with half purple-green shifting puzzle with sakura petals transforming into question marks created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in triumphant gold and mysterious green flames, sakura petals and question marks float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Graceful Sonata** (38x38)
*Recipe: Sonata's Embrace + Swan's Chromatic Diadem + 15 each Moonlit & Prismatic Resonance Cores @ Ancient Manipulator*
*Effect: Moonlight + Swan Lake bonuses, night dancing grants buffs*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial fusion themed around "Moonlight and Grace" made of deep purple crescent intertwined with black and white swan feathers with silver moonlight meeting rainbow prismatic shimmer created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in ethereal purple and elegant white flames with rainbow accents, crescent moons and swan feathers and rainbow sparkles float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Blazing Swan** (38x38)
*Recipe: Infernal Virtuoso + Swan's Chromatic Diadem + 15 each Infernal & Prismatic Resonance Cores @ Ancient Manipulator*
*Effect: Campanella + Swan Lake bonuses, fire trails shimmer with rainbow*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial fusion themed around "Fire and Grace" made of black and orange flames embracing white and black feathers with rainbow fire trailing from impossible dance created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in infernal orange and elegant white flames with prismatic shimmer, bells and swan feathers and rainbow fire float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Valor's Symphonic Grace** (38x38)
*Recipe: Hero's Symphony + Swan's Chromatic Diadem + 15 each Heroic & Prismatic Resonance Cores @ Ancient Manipulator*
*Effect: Eroica + Swan Lake bonuses, graceful kills trigger heroic surges*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial fusion themed around "Valor and Grace" made of scarlet-gold valor crest intertwined with white-black swan elements with sakura petals meeting swan feathers in rainbow shimmer created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in triumphant gold and elegant white flames with rainbow accents, sakura petals and swan feathers float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Void Flames** (38x38)
*Recipe: Infernal Virtuoso + Riddle of the Void + 15 each Infernal & Void Resonance Cores @ Ancient Manipulator*
*Effect: Campanella + Enigma bonuses, fire inflicts random Paradox debuffs*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial fusion themed around "Fire and Mystery" made of black and orange flames burning with eerie green edges and purple void shadows with bells ringing impossible sounds created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in infernal orange and mysterious green chaotic flames, bells and watching eyes and void tendrils float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

### Three-Theme Combination Accessories

**Trinity of Night** (38x38)
*Recipe: Lunar Flames + Riddle of the Void + 20 each Moonlit & Infernal & Void Resonance Cores @ Ancient Manipulator*
*Effect: Moonlight + Campanella + Enigma combined, ultimate darkness theme*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial trinity themed around "Absolute Darkness" made of deep purple moonlight and black-orange flames and purple-green void swirling together in perfect trinity with three crescents three flames three mysteries becoming one created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in unified purple and orange and green dark flames, crescent moons and bells and watching eyes float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Heroic Grace** (38x38)
*Recipe: Hero's Symphony + Graceful Sonata + 20 each Heroic & Moonlit & Prismatic Resonance Cores @ Ancient Manipulator*
*Effect: Eroica + Moonlight + Swan Lake combined, ultimate noble theme*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial trinity themed around "Noble Harmony" made of scarlet-gold heroism and purple-silver moonlight and white-black elegance unified harmoniously with sakura and lunar rays and feathers intermingling created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in triumphant gold and ethereal purple and elegant white flames, sakura petals and crescent moons and swan feathers float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Blazing Enigma** (38x38)
*Recipe: Blazing Swan + Riddle of the Void + 20 each Infernal & Prismatic & Void Resonance Cores @ Ancient Manipulator*
*Effect: Campanella + Enigma + Swan Lake combined, ultimate chaos theme*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial trinity themed around "Beautiful Chaos" made of black-orange flames and purple-green mystery and white-black grace colliding with fire dancing with questions and feathers burning with paradoxes created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in chaotic orange and green and rainbow flames, bells and watching eyes and burning feathers float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Complete Harmony** (38x38)
*Recipe: All 5 Theme Ultimate Accessories (Sonata's Embrace + Hero's Symphony + Infernal Virtuoso + Riddle of the Void + Swan's Chromatic Diadem) + 50 of each Theme Resonance Core @ Ancient Manipulator*
*Effect: ALL five themes combined at full strength, ultimate musical achievement*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial harmony themed around "Perfect Symphony" made of magnificent artifact combining purple-silver moonlight and scarlet-gold heroism and black-orange flames and purple-green mystery and white-black-rainbow grace with all five compositions in visible harmony created by music in the style of Terraria, radiating a transcendent aura, music notes surround it, ignited in all five theme flames unified in symphony, elements of all five themes float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

---

## ⭐ FATE TIER & ULTIMATE (Phase 5)

### Fate Theme Resonance Materials

**Fate Resonant Ore** (16x16)
*Source: Post-Moon Lord world generation in all biomes (rare), glows through blocks*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial ore themed around "Fate" made of deep black crystalline ore with dark pink veins and crimson star-like sparkles with galaxies seeming to swirl within created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in cosmic dark pink and crimson flames, galaxies and constellations and stars float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Fate Resonant Core** (18x18)
*Recipe: 5 Fate Resonant Ore @ Ancient Manipulator*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial core themed around "Fate" made of polished deep black sphere with dark pink swirling accretion patterns and bright crimson star points with miniature galaxies orbiting within created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in cosmic dark pink and crimson celestial flames, galaxies and constellations and star points float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

### Fate Vanilla Upgrade Accessories (Already Implemented)

**Paradox Chronometer** (38x38) ✅ DONE
*Recipe: Celestial Shell + Celestial Stone + 25 Fate Resonant Cores + Fate Resonant Energy @ Ancient Manipulator*
*Effect: +20% melee damage/speed, day/night bonuses always active, time anomaly attacks*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial chronometer themed around "Fate" made of impossible clock artifact with deep black frame and dark pink accretion disk at center with crimson constellation hands pointing to multiple times and silver celestial mechanisms orbiting created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in cosmic dark pink and crimson temporal flames, galaxies and clock gears and time distortions float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Constellation Compass** (38x38) ✅ DONE
*Recipe: Sniper Scope + Recon Scope + Magic Quiver + 25 Fate Resonant Cores + Fate Resonant Energy @ Ancient Manipulator*
*Effect: +25% ranged damage, bullets become star-traced, enemies marked by constellations*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial compass themed around "Fate" made of deep black compass body with dark pink constellation lines etched across surface and crimson cardinal star points with silver star maps orbiting created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in cosmic dark pink and crimson stellar flames, constellations and star maps and targeting lines float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Astral Conduit** (38x38) ✅ DONE
*Recipe: Celestial Emblem + Sorcerer Emblem + Arcane Flower + 25 Fate Resonant Cores + Fate Resonant Energy @ Ancient Manipulator*
*Effect: +25% magic damage, -20% mana cost, spells channel cosmic energy*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial conduit themed around "Fate" made of deep black crystal conduit with dark pink energy veins pulsing through structure and crimson star core blazing within with silver celestial magic circles orbiting created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in cosmic dark pink and crimson magical flames, galaxies and magic circles and cosmic mana float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Machination of the Event Horizon** (38x38) ✅ DONE
*Recipe: Master Ninja Gear + Terraspark Boots + Frog Leg + 30 Fate Resonant Cores + Fate Resonant Energy @ Ancient Manipulator*
*Effect: Ultimate mobility, brief invulnerability dash, gravity manipulation*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial machination themed around "Fate" made of swirling deep black hole artifact with dark pink accretion disk ring and crimson energy jets streaming outward with silver celestial mechanisms orbiting created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in cosmic dark pink and crimson gravitational flames, black hole energy and light bending and escape velocity float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Orrery of Infinite Orbits** (38x38) ✅ DONE
*Recipe: Papyrus Scarab + Necromantic Scroll + Pygmy Necklace + 25 Fate Resonant Cores + Fate Resonant Energy @ Ancient Manipulator*
*Effect: +3 minion slots, minions orbit in cosmic patterns, summons deal stellar damage*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial orrery themed around "Fate" made of intricate deep black orrery with dark pink planet spheres and crimson sun core at center with silver mechanical arms holding orbiting celestial bodies created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in cosmic dark pink and crimson orbital flames, planets and orbiting paths and constellation lines float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

### Grand Combination Accessories

**Opus of Four Movements** (38x38)
*Recipe: Complete Harmony + Vivaldi's Masterwork + All 9 Resonant Energies (4 Seasonal + 5 Theme) @ Ancient Manipulator*
*Effect: ALL seasons AND all themes combined, ultimate pre-Fate musical achievement*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial opus themed around "Complete Musical Mastery" made of magnificent circular artifact with sections representing spring pink summer orange autumn brown winter blue AND moonlight purple eroica scarlet campanella black-orange enigma purple-green swan lake white-rainbow created by music in the style of Terraria, radiating a transcendent aura, music notes surround it, ignited in all seasonal and theme flames unified, elements of all four seasons and five themes float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Cosmic Warden's Regalia** (38x38)
*Recipe: All 5 Fate Vanilla Upgrade Accessories + 50 Fate Resonant Cores + All 5 Theme Resonant Energies @ Ancient Manipulator*
*Effect: ALL Fate accessory bonuses combined, ultimate cosmic authority*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial regalia themed around "Fate" made of magnificent deep black cosmic crest with all five Fate upgrades represented as dark pink and crimson orbital elements with chronometer compass conduit event horizon and orrery unified as one cosmic regalia created by music in the style of Terraria, radiating a transcendent aura, music notes surround it, ignited in cosmic dark pink and crimson universal flames, galaxies and all five Fate elements float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Seasonal Destiny** (38x38)
*Recipe: Vivaldi's Masterwork + Paradox Chronometer + 30 Fate Resonant Cores @ Ancient Manipulator*
*Effect: All seasons + cosmic time manipulation, seasonal bonuses enhanced by fate*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial destiny themed around "Fate and Seasons" made of Vivaldi's four-part artifact merged with deep black cosmic clockwork with spring pink summer orange autumn brown winter blue sections orbiting around dark pink temporal core created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in all four seasonal flames with cosmic temporal energy, seasonal elements and fate threads and clock gears float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Theme Wanderer** (38x38)
*Recipe: Complete Harmony + Machination of the Event Horizon + 30 Fate Resonant Cores @ Ancient Manipulator*
*Effect: All five themes + cosmic mobility, switching themes mid-combat*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial wanderer themed around "Fate and Themes" made of the five-theme harmony artifact orbited by deep black event horizon ring with purple moonlight scarlet heroism black-orange flames purple-green mystery and white-rainbow grace bending around cosmic core created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in all five theme flames with cosmic travel energy, elements of all themes and dimensional rifts float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Summoner's Magnum Opus** (38x38)
*Recipe: Complete Harmony + Orrery of Infinite Orbits + 30 Fate Resonant Cores @ Ancient Manipulator*
*Effect: All themes + ultimate summoning, minions gain theme abilities*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial summon themed around "Fate and Themes" made of the five-theme harmony artifact at center with deep black orrery arms extending outward with each orbit path representing a different theme-empowered minion created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in all five theme flames with cosmic orbital energy, themed minion representations and orbit paths float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

### Season + Theme Hybrid Accessories

**Spring's Moonlit Garden** (38x38)
*Recipe: Bloom Crest + Sonata's Embrace + 15 each Vernal Bars & Moonlit Resonance Cores @ Ancient Manipulator*
*Effect: Spring + Moonlight, healing blooms under moonlight*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial garden themed around "Spring and Moonlight" made of white-pink spring blossoms growing from purple-silver moonlit soil with cherry blossoms blooming in crescent moon shapes created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in gentle pink and ethereal purple lunar flames, cherry blossoms and crescent moons float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Summer's Infernal Peak** (38x38)
*Recipe: Radiant Crown + Infernal Virtuoso + 15 each Solstice Bars & Infernal Resonance Cores @ Ancient Manipulator*
*Effect: Summer + Campanella, maximum fire damage at noon*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial peak themed around "Summer and Fire" made of radiant orange solar crown fused with black-orange infernal bell with the sun and damned bell ringing together in fiery unison created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in blazing orange and infernal black solar flames, solar flares and infinity bells float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Winter's Enigmatic Silence** (38x38)
*Recipe: Glacial Heart + Riddle of the Void + 15 each Permafrost Bars & Void Resonance Cores @ Ancient Manipulator*
*Effect: Winter + Enigma, frozen paradoxes, silence becomes mystery*
```
Concept art for a side-view idle pixel art sprite of an ancient celestial silence themed around "Winter and Mystery" made of white-blue frozen heart encased in purple-green shifting void patterns with snowflakes freezing into question marks created by music in the style of Terraria, radiating a powerful aura, music notes surround it, ignited in serene blue and mysterious green frozen void flames, frozen question marks and void snowflakes float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

### THE ULTIMATE ACCESSORY

**Coda of Absolute Harmony** (38x38)
*Recipe: Opus of Four Movements + Cosmic Warden's Regalia + Spring's Moonlit Garden + Summer's Infernal Peak + Winter's Enigmatic Silence + Coda of Annihilation (CONSUMED) @ Ancient Manipulator*
*Effect: EVERYTHING combined, the ultimate accessory in MagnumOpus, music itself made wearable*
```
Concept art for a side-view idle pixel art sprite of an ancient transcendent masterpiece themed around "Absolute Musical Divinity" made of brilliant white core with EVERY color spiraling outward in perfect harmony with all four seasons pink orange brown blue AND all five themes moonlight purple eroica scarlet campanella black-orange enigma purple-green swan lake white-rainbow AND fate black-pink-crimson ALL visible as unified spiraling waves created by music in the style of Terraria, radiating the most powerful aura possible, music notes surround it, ignited in EVERY flame color unified in divine symphony, galaxies constellations music notes flowers flames snowflakes feathers questions heroes bells moons all orbit around it and are apart of its design, detailed, silver ornate design like the mechanism of creation itself, full-view --v 7.0
```

---

## 💊 UTILITIES (Phase 6)

### Potions

**Minor Resonance Tonic** (14x20)
*Recipe: 2 Minor Music Notes + Bottled Water + Daybloom @ Placed Bottle*
*Effect: +5% all damage for 4 minutes*
```
Concept art for a side-view idle pixel art sprite of an ancient basic Minor Resonance Tonic made of simple glass bottle with pale lavender liquid and tiny music note bubbles rising with cork stopper adorned with musical clef symbol created by music in the style of Terraria, radiating beginner's overwhelming musical enhancement potential, ornate frame of faint harmonic glow emanating from its simple form as the basic potion for aspiring musicians grants modest power, detailed, ornate design like a royal mechanism, full-view --v 7.0
```

**Harmonic Elixir** (14x22)
*Recipe: 5 Music Notes + Bottled Water + Moonglow + Fireblossom @ Alchemy Table*
*Effect: +10% all damage, +5% crit chance for 6 minutes*
```
Concept art for a side-view idle pixel art sprite of an ancient intermediate Harmonic Elixir made of elegant bottle with swirling dual-tone purple and gold liquid with music notes spiraling upward within and treble clef shaped stopper created by music in the style of Terraria, radiating balanced overwhelming musical power enhancement, ornate frame of harmonious energy pulsing gently from its elegant form as the intermediate musician's enhancement potion grants significant power, detailed, ornate design like a royal mechanism, full-view --v 7.0
```

**Elixir of the Maestro** (16x24)
*Recipe: 1 of each Resonant Energy (9 total) + Greater Healing Potion + Bottled Honey @ Alchemy Table*
*Effect: +15% all damage, +10% crit, +10% attack speed, music notes orbit you for 8 minutes*
```
Concept art for a side-view idle pixel art sprite of an ancient legendary Elixir of the Maestro made of ornate golden bottle with swirling rainbow liquid shifting through all theme colors with musical note bubbles rising eternally within and crystalline stopper carved as conductor's baton created by music in the style of Terraria, radiating divine overwhelming performance mastery, ornate frame of drinking granting sight of combat's hidden rhythms as the legendary conductor's brew of ultimate musical enhancement transforms the imbiber, detailed, ornate design like a royal mechanism, full-view --v 7.0
```

**Seasonal Draught** (14x22)
*Recipe: 1 of each Seasonal Essence (4) + Bottled Water + Waterleaf @ Alchemy Table*
*Effect: Gain current season's buff (changes based on in-game time) for 8 minutes*
```
Concept art for a side-view idle pixel art sprite of an ancient attuned Seasonal Draught made of unique bottle where liquid visibly shifts through spring pink summer orange autumn brown winter blue in slow eternal cycle with tiny flower petal and snowflake particles suspended within and stopper marked with four-season wheel created by music in the style of Terraria, radiating seasonal overwhelming attunement harmony, ornate frame of the potion attuning drinker to the cycle of seasons granting current season's power, detailed, ornate design like a royal mechanism, full-view --v 7.0
```

**Fate's Cosmic Brew** (16x24)
*Recipe: 10 Fate Resonant Cores + Greater Healing Potion + all 4 Lunar Fragments (5 each) @ Ancient Manipulator*
*Effect: +20% all damage, brief invulnerability when hit (10s cooldown) for 10 minutes*
```
Concept art for a side-view idle pixel art sprite of an ancient cosmic Fate's Cosmic Brew made of deep black bottle with dark pink and crimson cosmic liquid swirling with visible star points and constellations with galaxies forming and dissolving within the liquid and stopper as tiny orrery mechanism created by music in the style of Terraria, radiating cosmic overwhelming destiny empowerment, ornate frame of drinking aligning the imbiber with the cosmos as the endgame potion contains destiny itself, detailed, ornate design like a royal mechanism, full-view --v 7.0
```

### Permanent Upgrades

**Harmonic Resonator Fragment** (38x38)
*Recipe: 50 Minor Music Notes + 25 Music Notes + 1 of each Seasonal Essence @ Mythril Anvil*
*Use: Permanently increases max mana by 20 (one-time use)*
```
Concept art for a side-view idle pixel art sprite of an ancient crystallized Harmonic Resonator Fragment made of translucent crystal shard with visible music staff lines running through it and notes frozen mid-composition with four seasonal colors swirling at its core created by music in the style of Terraria, radiating permanent overwhelming resonance attunement, ornate frame of consuming the fragment forever attuning mana to music as the crystallized fragment of pure musical energy grants permanent mana enhancement, detailed, ornate design like a royal mechanism, full-view --v 7.0
```

**Conductor's Insight** (38x38)
*Recipe: 1 of each Resonant Energy (9 total) + 20 Luminite Bars @ Ancient Manipulator*
*Use: Permanently grants 5% increased all damage (one-time use)*
```
Concept art for a side-view idle pixel art sprite of an ancient divine Conductor's Insight made of floating orb containing visible musical knowledge as swirling notation and composition fragments with all nine theme colors orbiting within in perfect balance created by music in the style of Terraria, radiating eternal overwhelming musical comprehension, ornate frame of consuming the orb granting permanent understanding of combat's rhythm as the crystallized wisdom of the great composers becomes one with the user, detailed, ornate design like a royal mechanism, full-view --v 7.0
```

**Fate's Blessing** (38x38)
*Recipe: 50 Fate Resonant Cores + Fate Resonant Energy + 10 Luminite Bars @ Ancient Manipulator*
*Use: Permanently grants Fate Sight (enemies glow, boss health bars more detailed)*
```
Concept art for a side-view idle pixel art sprite of an ancient cosmic Fate's Blessing made of impossible geometric crystal with deep black core and dark pink veins and crimson star points pulsing with the universe's approval made manifest created by music in the style of Terraria, radiating reality-affirming overwhelming cosmic approval, ornate frame of holding the crystal conveying sense of cosmic acceptance as one-time consumption permanently attunes vision to destiny's threads, divine cosmic favor crystallized, detailed, ornate design like a royal mechanism, full-view --v 7.0
```

**Coda's Echo** (38x38)
*Recipe: Coda of Absolute Harmony (NOT consumed) + 100 Fate Resonant Cores + 1 of each material type (Bars Essences Cores) @ Ancient Manipulator*
*Use: Permanently grants +5% to ALL stats, music notes passively orbit you forever*
```
Concept art for a side-view idle pixel art sprite of an ancient divine Coda's Echo made of shard of pure harmonic energy containing visible echo of the Coda of Absolute Harmony with EVERY color spiraling within including all seasons all themes and cosmic fate energy with every note of every composition echoing infinitely within created by music in the style of Terraria, radiating existence-resonating overwhelming divine echo, ornate frame of consuming the crystal permanently attuning the user's very soul to the music of existence itself as the resonance fragment of the ultimate artifact becomes one with the imbiber, detailed, ornate design like a royal mechanism, full-view --v 7.0
```

**Symphony of Ages** (38x38)
*Recipe: Coda's Echo + Opus of Four Movements + Cosmic Warden's Regalia @ Ancient Manipulator*
*Use: Permanently unlocks "Composer Mode" - can change background music at will, small passive bonuses everywhere*
```
Concept art for a side-view idle pixel art sprite of an ancient transcendent Symphony of Ages made of magnificent crystal containing the entirety of MagnumOpus's musical journey from first resonant shards to cosmic fate power all visible in swirling layers with every seasonal boss every theme every composition playing eternally within in perfect unified symphony created by music in the style of Terraria, radiating existence-conducting overwhelming transcendent mastery, ornate frame of consuming the crystal granting mastery over the music of existence itself with ability to conduct reality's soundtrack as the ultimate permanent upgrade crystallizes all of music history, detailed, ornate design like a royal mechanism, full-view --v 7.0
```

---

*Document Version 2.2 - Reorganized by Implementation Phase + Complete Detailed Prompts*
*All prompts rewritten to: "Concept art for a side-view idle pixel art sprite of [ITEM] in the style of Terraria..."*
*All recipes include specific quantities and crafting stations*
*Last Updated: Session Date*

---

# 🚀 PHASE 7: PROGRESSIVE ACCESSORY CHAINS & UTILITY
*Items that grow with the player - combine and upgrade throughout progression*

> **FOUR SEASONS BOSS PLACEMENT:**
> - 🌸 **Primavera (Spring)** - Post-Eye of Cthulhu
> - ☀️ **L'Estate (Summer)** - Post-Skeletron  
> - 🍂 **Autunno (Autumn)** - Post-Wall of Flesh (Early Hardmode)
> - ❄️ **L'Inverno (Winter)** - Post-Mechanical Bosses

> **FULL PROGRESSION:** Eye of Cthulhu → **Primavera** → Skeletron → **L'Estate** → Wall of Flesh → **Autunno** → Mech Bosses → **L'Inverno** → Plantera → Golem → Moon Lord → Moonlight → Eroica → La Campanella → Enigma → Swan Lake → Fate

---

# 🎯 PROGRESSIVE ACCESSORY CHAINS BY CLASS

The core philosophy: **Unique mechanics that complement vanilla, not replace it.** Each chain introduces a NEW SYSTEM that works alongside vanilla gear, providing playstyles and options that don't exist in base Terraria.

---

## 7.1 ⚔️ MELEE ACCESSORY CHAIN — "Resonance Combo System"

> **Design Philosophy:** Vanilla melee has damage/speed boosts (Fire Gauntlet, Emblems). This chain introduces a **COMBO SYSTEM** — consecutive hits build Resonance stacks that unlock special attacks. Works WITH vanilla accessories, not instead of them.

### Tier 1: Pre-Hardmode Foundation
| Item | Recipe/Source | Effect |
|------|---------------|--------|
| **Resonant Rhythm Band** | 10 Resonant Crystal Shards + Band of Regeneration | Hitting enemies builds **Resonance** (max 10). Lose 1 stack/2s of not hitting. Visual note indicator |
| **Spring Tempo Charm** | Resonant Rhythm Band + 15 Vernal Bars + Primavera drop | Resonance max 15. At 10+ stacks: +8% melee speed |

### Tier 2: Mid Pre-Hardmode (Post-L'Estate)
| Item | Recipe/Source | Effect |
|------|---------------|--------|
| **Solar Crescendo Ring** | Spring Tempo Charm + 15 Solstice Bars + L'Estate drop | Resonance max 20. At 15+ stacks: melee attacks inflict "Scorched" (fire DoT that stacks) |

### Tier 3: Early Hardmode (Post-Autunno)
| Item | Recipe/Source | Effect |
|------|---------------|--------|
| **Harvest Rhythm Signet** | Solar Crescendo Ring + 20 Harvest Bars + Autunno drop | Resonance max 25. At 20+ stacks: 1% lifesteal. Losing stacks grants brief regen |

### Tier 4: Post-Mech (Post-L'Inverno)
| Item | Recipe/Source | Effect |
|------|---------------|--------|
| **Permafrost Cadence Seal** | Harvest Rhythm Signet + 25 Permafrost Bars + L'Inverno drop | Resonance max 30. At 25+ stacks: hits freeze nearby enemies briefly |
| **Vivaldi's Tempo Master** | Permafrost Cadence Seal + Cycle of Seasons (Post-Plantera) | Resonance max 40. **Consume 30 stacks** to unleash a seasonal elemental burst (changes with biome) |

### Tier 5: Post-Moon Lord Theme Chain
| Item | Recipe | Effect | Tier |
|------|--------|--------|------|
| **Moonlit Sonata Band** | Vivaldi's Tempo Master + 20 Moonlight Resonant Cores | Resonance decays slower at night. Consume 25 stacks: summon moonbeam slash | T1 |
| **Heroic Crescendo** | Moonlit Sonata Band + 20 Eroica Resonant Cores | Crits grant +2 Resonance. Consume 30 stacks: heroic charge dash | T2 |
| **Infernal Fortissimo** | Heroic Crescendo + 20 La Campanella Resonant Cores | Max Resonance 50. Consume 40 stacks: bell shockwave (huge AoE) | T3 |
| **Enigma's Dissonance** | Infernal Fortissimo + 20 Enigma Resonant Cores | Resonance randomly fluctuates ±3, but at 45+ enemies take "Paradox" DoT | T4 |
| **Swan's Perfect Measure** | Enigma's Dissonance + 20 Swan Lake Resonant Cores | Graceful hits (not taking damage) build +2 Resonance. Consume 35: feather storm | T5 |
| **Fate's Cosmic Symphony** | Swan's Perfect Measure + 30 Fate Resonant Cores + Fate Resonant Energy | Max 60 Resonance. Consume 50: reality-rending slash that hits all on-screen enemies | T6 |

---

## 7.2 🏹 RANGED ACCESSORY CHAIN — "Marked for Death System"

> **Design Philosophy:** Vanilla ranged has ammo conservation and crit. This chain introduces **MARK** mechanics — hitting enemies marks them, and marked enemies take bonus effects. Synergizes with vanilla gear.

### Tier 1: Pre-Hardmode Foundation
| Item | Recipe/Source | Effect |
|------|---------------|--------|
| **Resonant Spotter** | 10 Resonant Crystal Shards + Binoculars | Ranged attacks **Mark** enemies for 5s (visual indicator). Marked enemies glow slightly |
| **Spring Hunter's Lens** | Resonant Spotter + 15 Vernal Bars + Primavera drop | Marks last 8s. Hitting marked enemies has 10% chance to drop hearts |

### Tier 2: Mid Pre-Hardmode (Post-L'Estate)
| Item | Recipe/Source | Effect |
|------|---------------|--------|
| **Solar Tracker's Badge** | Spring Hunter's Lens + 15 Solstice Bars + L'Estate drop | Marks last 10s. Marked enemies take +5% damage from ALL sources (helps the whole team!) |

### Tier 3: Early Hardmode (Post-Autunno)
| Item | Recipe/Source | Effect |
|------|---------------|--------|
| **Harvest Reaper's Mark** | Solar Tracker's Badge + 20 Harvest Bars + Autunno drop | Marked enemies explode on death (small AoE, 50% weapon damage). Chain marking |

### Tier 4: Post-Mech (Post-L'Inverno)
| Item | Recipe/Source | Effect |
|------|---------------|--------|
| **Permafrost Hunter's Eye** | Harvest Reaper's Mark + 25 Permafrost Bars + L'Inverno drop | Marked enemies are slowed 15%. Killing marked enemy refreshes marks on nearby enemies |
| **Vivaldi's Seasonal Sight** | Permafrost Hunter's Eye + Cycle of Seasons (Post-Plantera) | Marks apply seasonal debuff (burn/chill/wither/bloom). Mark duration 15s |

### Tier 5: Post-Moon Lord Theme Chain
| Item | Recipe | Effect | Tier |
|------|--------|--------|------|
| **Moonlit Predator's Gaze** | Vivaldi's Seasonal Sight + 20 Moonlight Resonant Cores | Can mark up to 8 enemies. Marked enemies visible through walls | T1 |
| **Heroic Deadeye** | Moonlit Predator's Gaze + 20 Eroica Resonant Cores | Marked enemies take +8% damage. First shot on marked enemy is auto-crit | T2 |
| **Infernal Executioner's Brand** | Heroic Deadeye + 20 La Campanella Resonant Cores | Marked enemies burn. Death explosion radius +50% | T3 |
| **Enigma's Paradox Mark** | Infernal Executioner's Brand + 20 Enigma Resonant Cores | Marks can spread to unmarked enemies on hit (15% chance). Marks enemies in other dimensions? | T4 |
| **Swan's Graceful Hunt** | Enigma's Paradox Mark + 20 Swan Lake Resonant Cores | Perfect shots (no damage taken for 3s) apply "Swan Mark" — +15% crit chance against target | T5 |
| **Fate's Cosmic Verdict** | Swan's Graceful Hunt + 30 Fate Resonant Cores + Fate Resonant Energy | Marked enemies take +12% damage. Killing marked boss drops bonus loot bag | T6 |

---

## 7.3 ✨ MAGIC ACCESSORY CHAIN — "Harmonic Overflow System"

> **Design Philosophy:** Vanilla magic focuses on mana cost/regen. This chain introduces **OVERFLOW** — spending mana beyond 0 (going negative) triggers special effects instead of stopping your spells. High risk, high reward.

### Tier 1: Pre-Hardmode Foundation
| Item | Recipe/Source | Effect |
|------|---------------|--------|
| **Resonant Overflow Gem** | 10 Resonant Crystal Shards + Mana Regeneration Band | Can cast spells up to -20 mana. While negative: -25% magic damage, +50% mana regen |
| **Spring Arcane Conduit** | Resonant Overflow Gem + 15 Vernal Bars + Primavera drop | Overflow to -40 mana. While negative: spells leave healing petal trails |

### Tier 2: Mid Pre-Hardmode (Post-L'Estate)
| Item | Recipe/Source | Effect |
|------|---------------|--------|
| **Solar Mana Crucible** | Spring Arcane Conduit + 15 Solstice Bars + L'Estate drop | Overflow to -60 mana. While negative: spells inflict "Sunburn" debuff |

### Tier 3: Early Hardmode (Post-Autunno)
| Item | Recipe/Source | Effect |
|------|---------------|--------|
| **Harvest Soul Vessel** | Solar Mana Crucible + 20 Harvest Bars + Autunno drop | Overflow to -80 mana. Killing enemies while negative restores +15 mana instantly |

### Tier 4: Post-Mech (Post-L'Inverno)
| Item | Recipe/Source | Effect |
|------|---------------|--------|
| **Permafrost Void Heart** | Harvest Soul Vessel + 25 Permafrost Bars + L'Inverno drop | Overflow to -100 mana. While negative: spells have +15% damage (risk/reward!) |
| **Vivaldi's Harmonic Core** | Permafrost Void Heart + Cycle of Seasons (Post-Plantera) | Overflow to -120 mana. Recovering from negative mana releases a seasonal burst |

### Tier 5: Post-Moon Lord Theme Chain
| Item | Recipe | Effect | Tier |
|------|--------|--------|------|
| **Moonlit Overflow Star** | Vivaldi's Harmonic Core + 20 Moonlight Resonant Cores | At exactly 0 mana: next spell costs 0. Precision timing reward | T1 |
| **Heroic Arcane Surge** | Moonlit Overflow Star + 20 Eroica Resonant Cores | Going negative triggers brief invincibility (1s). 30s cooldown | T2 |
| **Infernal Mana Inferno** | Heroic Arcane Surge + 20 La Campanella Resonant Cores | While negative: leave fire trail. Enemies in trail take DoT | T3 |
| **Enigma's Negative Space** | Infernal Mana Inferno + 20 Enigma Resonant Cores | Overflow to -150. At -100 or below: spells hit twice but you take 5% max HP/s | T4 |
| **Swan's Balanced Flow** | Enigma's Negative Space + 20 Swan Lake Resonant Cores | Gain "Grace" buff when recovering from negative. Grace: +20% damage for 5s | T5 |
| **Fate's Cosmic Reservoir** | Swan's Balanced Flow + 30 Fate Resonant Cores + Fate Resonant Energy | Overflow to -200. At -150: spells bend reality, hitting enemies through walls | T6 |

---

## 7.4 🐉 SUMMON ACCESSORY CHAIN — "Conductor's Baton System"

> **Design Philosophy:** Vanilla summoner has +slots/+damage. This chain introduces **CONDUCTING** — actively directing your minions with special commands for burst damage windows. More engaged summoner gameplay.

### Tier 1: Pre-Hardmode Foundation
| Item | Recipe/Source | Effect |
|------|---------------|--------|
| **Resonant Conductor's Wand** | 10 Resonant Crystal Shards + Flinx Fur | Right-click to **Conduct**: all minions focus one enemy for 3s (+20% damage to target). 15s cooldown |
| **Spring Maestro's Badge** | Resonant Conductor's Wand + 15 Vernal Bars + Primavera drop | Conduct cooldown 12s. Conducted minions heal you 1HP/hit during focus |

### Tier 2: Mid Pre-Hardmode (Post-L'Estate)
| Item | Recipe/Source | Effect |
|------|---------------|--------|
| **Solar Director's Crest** | Spring Maestro's Badge + 15 Solstice Bars + L'Estate drop | Conduct cooldown 10s. Focus target takes "Performed" debuff: -5 defense |

### Tier 3: Early Hardmode (Post-Autunno)
| Item | Recipe/Source | Effect |
|------|---------------|--------|
| **Harvest Beastlord's Horn** | Solar Director's Crest + 20 Harvest Bars + Autunno drop | Conduct grants minions +30% damage during focus. Killing conducted target extends buff 2s |

### Tier 4: Post-Mech (Post-L'Inverno)
| Item | Recipe/Source | Effect |
|------|---------------|--------|
| **Permafrost Commander's Crown** | Harvest Beastlord's Horn + 25 Permafrost Bars + L'Inverno drop | Conduct cooldown 8s. Conducted target is slowed 25% |
| **Vivaldi's Orchestra Baton** | Permafrost Commander's Crown + Cycle of Seasons (Post-Plantera) | **New command**: Double-tap Conduct for "Scatter" — minions spread to all nearby enemies briefly |

### Tier 5: Post-Moon Lord Theme Chain
| Item | Recipe | Effect | Tier |
|------|--------|--------|------|
| **Moonlit Symphony Wand** | Vivaldi's Orchestra Baton + 20 Moonlight Resonant Cores | Conducting at night: +10% minion damage globally for duration | T1 |
| **Heroic General's Baton** | Moonlit Symphony Wand + 20 Eroica Resonant Cores | Conduct grants minions brief invincibility (1s). Rally your troops! | T2 |
| **Infernal Choir Master's Rod** | Heroic General's Baton + 20 La Campanella Resonant Cores | Conducted minions explode on hit (doesn't kill them). +50% damage as AoE | T3 |
| **Enigma's Hivemind Link** | Infernal Choir Master's Rod + 20 Enigma Resonant Cores | Minions can phase through blocks during Conduct. Ambush from anywhere | T4 |
| **Swan's Graceful Direction** | Enigma's Hivemind Link + 20 Swan Lake Resonant Cores | Perfect Conduct (full HP): minions deal double damage for focus duration | T5 |
| **Fate's Cosmic Dominion** | Swan's Graceful Direction + 30 Fate Resonant Cores + Fate Resonant Energy | Conduct cooldown 5s. "Finale": hold Conduct 2s to sacrifice all minions for massive single hit | T6 |

---

## 7.5 🛡️ DEFENSE ACCESSORY CHAIN — "Resonant Shield System"

> **Design Philosophy:** Vanilla defense has DR/dodge/immunities. This chain introduces **RESONANT SHIELD** — a regenerating barrier that absorbs hits, with unique effects when the shield breaks. Tactical tanking.

### Tier 1: Pre-Hardmode Foundation
| Item | Recipe/Source | Effect |
|------|---------------|--------|
| **Resonant Barrier Core** | 10 Resonant Crystal Shards + Shackle | Gain a **Resonant Shield** equal to 10% max HP. Regenerates when not hit for 5s |
| **Spring Vitality Shell** | Resonant Barrier Core + 15 Vernal Bars + Primavera drop | Shield = 15% max HP. When shield breaks: release healing petals (10 HP to nearby allies) |

### Tier 2: Mid Pre-Hardmode (Post-L'Estate)
| Item | Recipe/Source | Effect |
|------|---------------|--------|
| **Solar Flare Aegis** | Spring Vitality Shell + 15 Solstice Bars + L'Estate drop | Shield = 20% max HP. When shield breaks: release fire nova (damages nearby enemies) |

### Tier 3: Early Hardmode (Post-Autunno)
| Item | Recipe/Source | Effect |
|------|---------------|--------|
| **Harvest Thorned Guard** | Solar Flare Aegis + 20 Harvest Bars + Autunno drop | Shield = 25% max HP. While shield active: thorns (return 15% damage to melee attackers) |

### Tier 4: Post-Mech (Post-L'Inverno)
| Item | Recipe/Source | Effect |
|------|---------------|--------|
| **Permafrost Crystal Ward** | Harvest Thorned Guard + 25 Permafrost Bars + L'Inverno drop | Shield = 30% max HP. When shield breaks: freeze attackers for 1s |
| **Vivaldi's Seasonal Bulwark** | Permafrost Crystal Ward + Cycle of Seasons (Post-Plantera) | Shield = 35% max HP. Shield break effect changes with season (heal/fire/thorns/freeze) |

### Tier 5: Post-Moon Lord Theme Chain
| Item | Recipe | Effect | Tier |
|------|--------|--------|------|
| **Moonlit Guardian's Veil** | Vivaldi's Seasonal Bulwark + 20 Moonlight Resonant Cores | Shield regens faster at night. Break effect: brief invisibility (2s) | T1 |
| **Heroic Valor's Aegis** | Moonlit Guardian's Veil + 20 Eroica Resonant Cores | Break effect: gain +15% damage for 5s. Turn defense into offense! | T2 |
| **Infernal Bell's Fortress** | Heroic Valor's Aegis + 20 La Campanella Resonant Cores | Shield = 40% HP. Break releases massive bell shockwave | T3 |
| **Enigma's Void Shell** | Infernal Bell's Fortress + 20 Enigma Resonant Cores | While shield active: 10% chance to phase through attacks entirely | T4 |
| **Swan's Immortal Grace** | Enigma's Void Shell + 20 Swan Lake Resonant Cores | Shield = 50% HP. At full shield: gain +5% dodge chance | T5 |
| **Fate's Cosmic Aegis** | Swan's Immortal Grace + 30 Fate Resonant Cores + Fate Resonant Energy | Shield = 60% HP. Break triggers "Last Stand": invincible for 3s, once per 2 minutes | T6 |

---

## 7.6 ⚡ MOBILITY ACCESSORY CHAIN — "Momentum System"

> **Design Philosophy:** Vanilla mobility has speed/flight/dash. This chain introduces **MOMENTUM** — moving continuously builds speed and power. Rewards constant motion and aggressive positioning.

### Tier 1: Pre-Hardmode Foundation
| Item | Recipe/Source | Effect |
|------|---------------|--------|
| **Resonant Velocity Band** | 10 Resonant Crystal Shards + Aglet + Anklet of the Wind | Moving builds **Momentum** (max 100). Standing still loses 5/s. Visual speed lines at high momentum |
| **Spring Zephyr Boots** | Resonant Velocity Band + 15 Vernal Bars + Primavera drop | At 50+ Momentum: +10% move speed. At 80+: double jump resets mid-air |

### Tier 2: Mid Pre-Hardmode (Post-L'Estate)
| Item | Recipe/Source | Effect |
|------|---------------|--------|
| **Solar Blitz Treads** | Spring Zephyr Boots + 15 Solstice Bars + L'Estate drop | At 70+ Momentum: leave fire trail that damages enemies. Running is attacking! |

### Tier 3: Early Hardmode (Post-Autunno)
| Item | Recipe/Source | Effect |
|------|---------------|--------|
| **Harvest Phantom Stride** | Solar Blitz Treads + 20 Harvest Bars + Autunno drop | At 80+ Momentum: phase through enemies (no contact damage). Ghost running |

### Tier 4: Post-Mech (Post-L'Inverno)
| Item | Recipe/Source | Effect |
|------|---------------|--------|
| **Permafrost Avalanche Step** | Harvest Phantom Stride + 25 Permafrost Bars + L'Inverno drop | At 90+ Momentum: leave ice trail that slows enemies. At 100: brief ice dash |
| **Vivaldi's Seasonal Sprint** | Permafrost Avalanche Step + Cycle of Seasons (Post-Plantera) | Momentum max 120. Trail effect changes with season |

### Tier 5: Post-Moon Lord Theme Chain
| Item | Recipe | Effect | Tier |
|------|--------|--------|------|
| **Moonlit Phantom's Rush** | Vivaldi's Seasonal Sprint + 20 Moonlight Resonant Cores | At 100+ Momentum: become semi-transparent, enemies target you less | T1 |
| **Heroic Charge Boots** | Moonlit Phantom's Rush + 20 Eroica Resonant Cores | **Consume 80 Momentum** to dash-attack (damages enemies in path) | T2 |
| **Infernal Meteor Stride** | Heroic Charge Boots + 20 La Campanella Resonant Cores | At 100+ Momentum while falling: create impact crater on landing | T3 |
| **Enigma's Phase Shift** | Infernal Meteor Stride + 20 Enigma Resonant Cores | Consume 100 Momentum: short-range teleport in movement direction | T4 |
| **Swan's Eternal Glide** | Enigma's Phase Shift + 20 Swan Lake Resonant Cores | Momentum decays 50% slower. At max: flight doesn't consume wing time | T5 |
| **Fate's Cosmic Velocity** | Swan's Eternal Glide + 30 Fate Resonant Cores + Fate Resonant Energy | Momentum max 150. At 150: **time slows 20%** for enemies near you | T6 |

---

# 🎁 ONE-OFF UTILITY ITEMS

## 7.7 MOUNTS

| Item | Recipe/Source | Speed | Flight | Notes |
|------|---------------|-------|--------|-------|
| **Resonant Slime Saddle** | Slime Saddle + 15 Resonant Crystal Shards | 35 mph | Bounce | Musical bounce, note particles |
| **Primavera's Petal Steed** | Primavera boss (10%) | 42 mph | 3s hover | Pink unicorn, petal trail |
| **L'Estate's Sun Chariot** | L'Estate boss (10%) | 48 mph | 4s | Blazing chariot, fire trail |
| **Autunno's Phantom Horse** | Autunno boss (10%) | 50 mph | 5s glide | Ghostly steed, leaf trail |
| **L'Inverno's Frost Sleigh** | L'Inverno boss (10%) | 45 mph | 6s glide | Ice sleigh, freeze trail |
| **Vivaldi's Seasonal Carriage** | All 4 Season Mounts + Cycle of Seasons | 58 mph | Infinite | Transforms per season |
| **Cosmic Throne of Fate** | Fate Resonant Energy + 50 Fate Resonant Cores + All Theme Resonant Energies | 95 mph | Infinite | Constellation throne |

## 7.8 LIGHT PETS

| Item | Source | Notes |
|------|--------|-------|
| **Minor Note Pet** | 15 Minor Music Notes + Shadow Key | Floating music note |
| **Spring Sprite** | Primavera boss (20%) | Pink fairy with petals |
| **Summer Wisp** | L'Estate boss (20%) | Orange flame wisp |
| **Autumn Phantom** | Autunno boss (20%) | Ghost leaf |
| **Winter Frost** | L'Inverno boss (20%) | Snowflake sprite |
| **Moonlit Wisp** | Moonlight boss (15%) | Purple crescent |
| **Heroic Spirit** | Eroica boss (15%) | Mini phoenix |
| **Infernal Chime** | La Campanella boss (15%) | Floating bell |
| **Void Eye** | Enigma boss (15%) | Watching eye |
| **Prismatic Feather** | Swan Lake boss (15%) | Rainbow feather |
| **Cosmic Shard** | Fate boss (15%) | Star fragment |

## 7.9 VANITY & COSMETICS

### Seasonal Dyes
| Item | Recipe | Effect |
|------|--------|--------|
| **Vernal Bloom Dye** | 5 Vernal Bars + Strange Plant | Pink/white petals |
| **Solstice Flame Dye** | 5 Solstice Bars + Strange Plant | Animated fire |
| **Harvest Twilight Dye** | 5 Harvest Bars + Strange Plant | Falling leaves |
| **Permafrost Crystal Dye** | 5 Permafrost Bars + Strange Plant | Frost sparkle |
| **Seasonal Cycle Dye** | All 4 Season Dyes | Shifts through seasons |

### Theme Dyes
| Item | Recipe | Effect |
|------|--------|--------|
| **Moonlit Glow Dye** | Moonlight Resonant Core ×5 + Strange Plant | Purple/silver moon phases |
| **Heroic Valor Dye** | Eroica Resonant Core ×5 + Strange Plant | Scarlet/gold sakura |
| **Infernal Bell Dye** | La Campanella Resonant Core ×5 + Strange Plant | Black/orange flames |
| **Enigmatic Void Dye** | Enigma Resonant Core ×5 + Strange Plant | Purple/green eyes |
| **Swan Grace Dye** | Swan Lake Resonant Core ×5 + Strange Plant | White/black rainbow |
| **Cosmic Fate Dye** | Fate Resonant Core ×5 + Strange Plant | Black/pink stars |

### Boss Vanity Sets (5% drop each)
- **Primavera's Garb** - Flower dress, petal crown
- **L'Estate's Regalia** - Solar robes, flame crown
- **Autunno's Mantle** - Ghostly robes, leaf crown
- **L'Inverno's Vestments** - Ice robes, crystal crown
- **Maestro's Formal Attire** (1% any boss) - Conductor's tuxedo

---

## 7.10 Phase 7 Asset Prompts - Progressive Chain Key Items

### Melee Chain Sprites

**Resonant Rhythm Band** (38x38)
```
Concept art for a side-view idle pixel art sprite of an ancient celestial wristband themed around "Resonant Rhythm" made of leather embedded with pale lavender crystal shards pulsing in rhythmic patterns created by music in the style of Terraria, radiating a nascent melee potential aura, music notes surround it, ignited in soft lavender flames, beat lines and tempo markers float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Vivaldi's Tempo Master** (38x38)
```
Concept art for a side-view idle pixel art sprite of an ancient celestial gauntlet themed around "Four Seasons Combat" made of elegant design with pink spring petals orange summer flames brown autumn leaves and blue winter frost swirling around a central resonant gem created by music in the style of Terraria, radiating a seasonal mastery aura, music notes surround it, ignited in seasonal gradient flames, petals flames leaves and snowflakes orbit around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Fate's Cosmic Symphony** (38x38)
```
Concept art for a side-view idle pixel art sprite of an ancient celestial gauntlet themed around "Cosmic Melee Divinity" made of deep black material with dark pink constellation lines and crimson star gems at each knuckle created by music in the style of Terraria, radiating a cosmic melee aura, music notes surround it, ignited in black-pink-crimson cosmic flames, constellations galaxies and reality tears float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

### Ranged Chain Sprites

**Resonant Spotter** (38x38)
```
Concept art for a side-view idle pixel art sprite of an ancient celestial binoculars themed around "Target Marking" made of resonant crystal lens with pale purple glow and targeting reticle patterns created by music in the style of Terraria, radiating a hunter's potential aura, music notes surround it, ignited in pale purple flames, targeting marks and reticle symbols float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Vivaldi's Seasonal Sight** (38x38)
```
Concept art for a side-view idle pixel art sprite of an ancient celestial monocle themed around "Four Seasons Hunting" made of elegant golden frame with lens shifting between pink spring orange summer brown autumn and blue winter clarity created by music in the style of Terraria, radiating a seasonal hunting aura, music notes surround it, ignited in seasonal gradient flames, targeting symbols and seasonal marks float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Fate's Cosmic Verdict** (38x38)
```
Concept art for a side-view idle pixel art sprite of an ancient celestial targeting device themed around "Cosmic Judgment" made of deep black frame with dark pink crosshairs and crimson destiny threads visible in the constellation reticle created by music in the style of Terraria, radiating a cosmic hunter's authority aura, music notes surround it, ignited in black-pink-crimson cosmic flames, destiny threads and star crosshairs float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

### Magic Chain Sprites

**Resonant Overflow Gem** (38x38)
```
Concept art for a side-view idle pixel art sprite of an ancient celestial crystal gem themed around "Mana Overflow" made of cracked crystal leaking pale blue mana energy from its fractures created by music in the style of Terraria, radiating an unstable magical aura, music notes surround it, ignited in pale blue overflow flames, mana streams and fracture energy float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Vivaldi's Harmonic Core** (38x38)
```
Concept art for a side-view idle pixel art sprite of an ancient celestial crystal heart themed around "Four Seasons Magic" made of pink spring blue winter orange summer and brown autumn energy in pulsing concentric layers created by music in the style of Terraria, radiating a seasonal arcane aura, music notes surround it, ignited in seasonal gradient flames, mana swirls and seasonal energy float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Fate's Cosmic Reservoir** (38x38)
```
Concept art for a side-view idle pixel art sprite of an ancient celestial crystal sphere themed around "Cosmic Magic Infinity" made of deep black material with dark pink mana veins and crimson star core containing infinite negative space created by music in the style of Terraria, radiating a cosmic arcane aura, music notes surround it, ignited in black-pink-crimson cosmic flames, galaxies and mana dimensions float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

### Summon Chain Sprites

**Resonant Conductor's Wand** (38x38)
```
Concept art for a side-view idle pixel art sprite of an ancient celestial baton themed around "Minion Conducting" made of simple wand with resonant crystal tip glowing pale lavender with visible command lines created by music in the style of Terraria, radiating a summoner's potential aura, music notes surround it, ignited in pale lavender flames, conductor lines and command threads float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Vivaldi's Orchestra Baton** (38x38)
```
Concept art for a side-view idle pixel art sprite of an ancient celestial golden baton themed around "Four Seasons Conducting" made of elegant design with pink spring orange summer brown autumn and blue winter gems set along its length with seasonal energy flowing between created by music in the style of Terraria, radiating a seasonal commanding aura, music notes surround it, ignited in seasonal gradient flames, conductor waves and minion threads float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Fate's Cosmic Dominion** (38x38)
```
Concept art for a side-view idle pixel art sprite of an ancient celestial conductor's baton themed around "Cosmic Summoner Divinity" made of deep black material with dark pink constellation lines and crimson star blazing at its tip created by music in the style of Terraria, radiating a cosmic summoner aura, music notes surround it, ignited in black-pink-crimson cosmic flames, cosmic threads and minion chains float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

### Defense Chain Sprites

**Resonant Barrier Core** (38x38)
```
Concept art for a side-view idle pixel art sprite of an ancient celestial protective core themed around "Shield Generation" made of crystalline material with visible shield energy emanating outward in gentle waves created by music in the style of Terraria, radiating a guardian's potential aura, music notes surround it, ignited in pale shield-blue flames, barrier waves and protective energy float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Vivaldi's Seasonal Bulwark** (38x38)
```
Concept art for a side-view idle pixel art sprite of an ancient celestial shield core themed around "Four Seasons Defense" made of four seasonal quadrants with green spring barrier golden summer wall amber autumn ward and ice winter shield rotating in protective harmony created by music in the style of Terraria, radiating a seasonal defensive aura, music notes surround it, ignited in seasonal gradient flames, shield fragments and seasonal barriers float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Fate's Cosmic Aegis** (38x38)
```
Concept art for a side-view idle pixel art sprite of an ancient celestial shield core themed around "Cosmic Immortal Protection" made of deep black material with dark pink constellation barrier patterns and crimson star gems forming protective constellation created by music in the style of Terraria, radiating a cosmic guardian aura, music notes surround it, ignited in black-pink-crimson cosmic flames, galaxies and constellation barriers float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

### Mobility Chain Sprites

**Resonant Velocity Band** (38x38)
```
Concept art for a side-view idle pixel art sprite of an ancient celestial ankle band themed around "Momentum Building" made of sleek design with resonant crystal glowing brighter as momentum builds with speed lines forming created by music in the style of Terraria, radiating a nascent velocity aura, music notes surround it, ignited in pale speed flames, speed lines and motion trails float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Vivaldi's Seasonal Sprint** (38x38)
```
Concept art for a side-view idle pixel art sprite of an ancient celestial boots themed around "Four Seasons Mobility" made of elegant design with green spring breezes golden summer heat amber autumn winds and icy winter momentum swirling in perpetual motion created by music in the style of Terraria, radiating a seasonal velocity aura, music notes surround it, ignited in seasonal gradient flames, speed lines and seasonal trails float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Fate's Cosmic Velocity** (38x38)
```
Concept art for a side-view idle pixel art sprite of an ancient celestial boots themed around "Cosmic Speed Transcendence" made of deep black material with dark pink speed lines and crimson star trails streaming through space-time created by music in the style of Terraria, radiating a cosmic velocity aura, music notes surround it, ignited in black-pink-crimson cosmic flames, galaxies blurring and time distortions float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

### Mount Sprites

**Vivaldi's Seasonal Carriage** (64x48)
```
Concept art for a side-view idle pixel art sprite of an ancient celestial carriage mount themed around "Four Seasons Transport" made of ornate design shifting between pink spring flowers orange summer flames brown autumn leaves and blue winter frost with four seasonal horses pulling in harmony created by music in the style of Terraria, radiating a seasonal glory aura, music notes surround it, ignited in seasonal gradient flames, petals flames leaves snowflakes and seasonal trails float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

**Cosmic Throne of Fate** (72x56)
```
Concept art for a side-view idle pixel art sprite of an ancient celestial throne mount themed around "Cosmic Majesty" made of deep black material with dark pink constellation cushioning and crimson star gem armrests with galaxies orbiting beneath created by music in the style of Terraria, radiating a divine cosmic presence aura, music notes surround it, ignited in black-pink-crimson cosmic flames, constellations galaxies and star trails float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

### Light Pet Sprites

**Cosmic Shard Pet** (16x16)
```
Concept art for a side-view idle pixel art sprite of an ancient celestial light pet themed around "Cosmic Companion" made of small deep black crystal shard with dark pink veins and crimson star core that orbits and rotates created by music in the style of Terraria, radiating a tiny cosmic companion warmth, music notes surround it, ignited in soft black-pink cosmic flames, tiny stars and cosmic sparkles float around it and are apart of its design, detailed, silver ornate design like a royal mechanism, full-view --v 7.0
```

---

## 7.11 Phase 7 Asset Checklist

```
PROGRESSIVE ACCESSORY CHAINS (36 items per chain × 6 chains = 36 items)
MELEE CHAIN (6 tiers)
[ ] Rhythmic Wristband → Spring Bloom Gauntlet → Solar Fury Gauntlet →
    Harvest Reaper's Grip → Permafrost Warrior's Grasp → Vivaldi's Masterwork →
    Moonlit → Heroic → Infernal → Enigma's → Swan's → Fate's Cosmic Grip

RANGED CHAIN (6 tiers)
[ ] Resonant Quiver → Spring Petal → Solar Blaze → Harvest Moon →
    Permafrost Arsenal → Vivaldi's → Moonlit → Heroic → Infernal →
    Enigma's → Swan's → Fate's Cosmic Trajectory

MAGIC CHAIN (6 tiers)  
[ ] Harmonic Focus → Spring Arcane → Solar Mage's → Harvest Witch's →
    Permafrost Mage's → Vivaldi's → Moonlit → Heroic → Infernal →
    Enigma's → Swan's → Fate's Cosmic Conduit

SUMMON CHAIN (6 tiers)
[ ] Conductor's Fragment → Spring Whistle → Solar Commander's → 
    Harvest Necromancer's → Permafrost Beast Master's → Vivaldi's →
    Moonlit → Heroic → Infernal → Enigma's → Swan's → Fate's Orchestra

DEFENSE CHAIN (6 tiers)
[ ] Resonant Guard → Spring Vitality → Solar Bulwark → Harvest Undying →
    Permafrost Aegis → Vivaldi's Fortress → Moonlit → Heroic → Infernal →
    Enigma's → Swan's → Fate's Cosmic Immortality

MOBILITY CHAIN (6 tiers)
[ ] Harmonic Dash Band → Spring Breeze → Solar Sprint → Harvest Wind Walker →
    Permafrost Blizzard → Vivaldi's Step → Moonlit → Heroic → Infernal →
    Enigma's → Swan's → Fate's Cosmic Velocity

MOUNTS (7 items)
[ ] Resonant Slime Saddle
[ ] Primavera's Petal Steed
[ ] L'Estate's Sun Chariot
[ ] Autunno's Phantom Horse
[ ] L'Inverno's Frost Sleigh
[ ] Vivaldi's Seasonal Carriage
[ ] Cosmic Throne of Fate

LIGHT PETS (11 items)
[ ] Minor Note Pet
[ ] 4× Seasonal Pets
[ ] 6× Theme Pets

VANITY (26 items)
[ ] 5× Seasonal Dyes
[ ] 6× Theme Dyes
[ ] 5× Boss Vanity Sets (15 pieces)

TOTAL PHASE 7: ~80 unique item sprites
(Many accessories are upgrades that could share base sprites with palette swaps)
```

---

# 📊 UPDATED COMPLETE ASSET SUMMARY

| Phase | Items | Priority |
|-------|-------|----------|
| Phase 1 - Foundation Materials | ~41 items | 🔴 DO FIRST |
| Phase 2 - Four Seasons | 4 bosses + 16 accessories | 🟠 SECOND |
| Phase 3 - Theme Expansions | 23 items | 🟡 THIRD |
| Phase 4 - Combinations | 10 accessories | 🟢 FOURTH |
| Phase 5 - Fate & Ultimate | 14 accessories | 🔵 FIFTH |
| Phase 6 - Utilities | 15 items | ⚪ SIXTH |
| **Phase 7 - Progressive Chains & Utility** | **~80 items** | 🟣 SEVENTH |

**GRAND TOTAL: ~199 new item sprites + 4 boss sprite sets**

---

*Document Version 3.2 - ALL Midjourney Prompts Rewritten*
*Format: "Concept art for a side-view idle pixel art sprite of [ITEM] in the style of Terraria..."*
*All recipes include specific quantities and crafting stations*
*Last Updated: Current Session*
