# MagnumOpus Enhancements - Phased Implementation Guide

> **This document is organized by IMPLEMENTATION PRIORITY, not gameplay progression.**
> Complete each phase before moving to the next. Foundation items unlock everything else.

---

# 📋 PHASE OVERVIEW

| Phase | Focus | Asset Count | Priority |
|-------|-------|-------------|----------|
| **Phase 1** | Foundation Materials (Bars, Essences, Enemy Drops) | ~41 items | 🔴 CRITICAL |
| **Phase 2** | Four Seasons Content (Bosses + Base Accessories) | ~20 items | 🟠 HIGH |
| **Phase 3** | Main Theme Expansions (New Materials + Accessories) | ~23 items | 🟡 MEDIUM |
| **Phase 4** | Combination Accessories (Multi-theme Combos) | ~10 items | 🟢 LOWER |
| **Phase 5** | Fate Tier & Ultimate Items | ~14 items | 🔵 ENDGAME |
| **Phase 6** | Utilities & Polish | ~15 items | ⚪ OPTIONAL |

---

# 🔴 PHASE 1: FOUNDATION MATERIALS
*Get these assets FIRST - everything else depends on them*

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
| **Blossom Essence** | Essence | 14x14 | Pink petal glow, white center |

*Recipe: 3 Petals of Rebirth + 1 Blossom Essence = 2 Vernal Bars*

### Summer Materials
| Item | Type | Sprite Size | Colors |
|------|------|-------------|--------|
| **Solstice Bar** | Bar | 20x14 | Radiant orange with white sheen |
| **Solar Essence** | Essence | 14x14 | Orange sun burst, white center |

*Recipe: 3 Embers of Intensity + 1 Solar Essence = 2 Solstice Bars*

### Autumn Materials
| Item | Type | Sprite Size | Colors |
|------|------|-------------|--------|
| **Harvest Bar** | Bar | 20x14 | Polished white-brown with orange tint |
| **Decay Essence** | Essence | 14x14 | Dark orange glow, white wisps |

*Recipe: 3 Leaves of Ending + 1 Decay Essence = 2 Harvest Bars*

### Winter Materials
| Item | Type | Sprite Size | Colors |
|------|------|-------------|--------|
| **Permafrost Bar** | Bar | 20x14 | Frosted white with light blue sheen |
| **Frost Essence** | Essence | 14x14 | White snowflake, light blue glow |

*Recipe: 3 Shards of Stillness + 1 Frost Essence = 2 Permafrost Bars*

---

## 1.3 Boss Drop Materials (8 Harmonic Essences)

*Unique boss-only drops, one per boss. Higher tier crafting ingredient.*

| Item | Boss Source | Sprite Size | Colors |
|------|-------------|-------------|--------|
| **Spring's Harmonic Essence** | Primavera | 20x20 | White/pink/light blue swirl |
| **Summer's Harmonic Essence** | L'Estate | 20x20 | Blazing orange/white |
| **Autumn's Harmonic Essence** | Autunno | 20x20 | White/brown/dark orange fade |
| **Winter's Harmonic Essence** | L'Inverno | 20x20 | White/light blue crystalline |
| **Moonlight's Harmonic Essence** | Moonlit Maestro | 20x20 | Purple/silver lunar |
| **Eroica's Harmonic Essence** | God of Valor | 20x20 | Scarlet/gold heroic |
| **Campanella's Harmonic Essence** | Chime of Life | 20x20 | Black/orange flames |
| **Enigma's Harmonic Essence** | Hollow Mystery | 20x20 | Purple/green void |

*Note: Swan Lake & Fate Harmonic Essences already exist*

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
| **Crown of Frost** | Ice Queen | 10% | 22x14 | Accessory crafting |
| **Permafrost Shard** | Any HM Ice enemy | 3% | 10x10 | Accessory crafting |

### Theme Enemy Drops (Post-Moon Lord)
| Item | Source | Rate | Sprite Size |
|------|--------|------|-------------|
| **Lunar Essence** | Moonlight enemies | 2% | 12x12 |
| **Valor Essence** | Eroica enemies | 2% | 12x12 |
| **Bell Essence** | La Campanella enemies | 2% | 12x12 |
| **Mystery Essence** | Enigma enemies | 2% | 12x12 |
| **Grace Essence** | Swan Lake enemies | 2% | 12x12 |

---

## 1.5 Phase 1 Asset Checklist

```
PRE-HARDMODE DROPS (12 items)
[ ] Resonant Crystal Shard
[ ] Minor Music Note
[ ] Faded Sheet Music
[ ] Broken Baton
[ ] Tuning Fork
[ ] Old Metronome
[ ] Rusted Clef
[ ] Dull Resonator
[ ] Dormant Spring Core
[ ] Dormant Summer Core
[ ] Dormant Autumn Core
[ ] Dormant Winter Core

SEASONAL BARS (4 items)
[ ] Vernal Bar
[ ] Solstice Bar
[ ] Harvest Bar
[ ] Permafrost Bar

SEASONAL ESSENCES (4 items)
[ ] Blossom Essence
[ ] Solar Essence
[ ] Decay Essence
[ ] Frost Essence

HARMONIC ESSENCES (6 items) - 2 already exist
[ ] Spring's Harmonic Essence
[ ] Summer's Harmonic Essence
[ ] Autumn's Harmonic Essence
[ ] Winter's Harmonic Essence
[ ] Moonlight's Harmonic Essence
[ ] Eroica's Harmonic Essence
[ ] Campanella's Harmonic Essence
[ ] Enigma's Harmonic Essence

ENEMY DROPS - PRIMARY BAR MATERIALS (4 items)
[ ] Petal of Rebirth (Spring bars)
[ ] Ember of Intensity (Summer bars)
[ ] Leaf of Ending (Autumn bars)
[ ] Shard of Stillness (Winter bars)

ENEMY DROPS - ACCESSORY MATERIALS (15 items)
[ ] Vernal Dust
[ ] Rainbow Petal
[ ] Sunfire Core
[ ] Heat Scale
[ ] Twilight Wing Fragment
[ ] Death's Note
[ ] Decay Fragment
[ ] Frozen Core
[ ] Crown of Frost
[ ] Permafrost Shard
[ ] Lunar Essence
[ ] Valor Essence
[ ] Bell Essence
[ ] Mystery Essence
[ ] Grace Essence

TOTAL PHASE 1: ~41 item sprites (NO tile sprites needed)
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
| 6 | **Bloom Crest** | Petal Shield + Growth Band + Spring Harmonic | 30x30 |

### Summer Accessories
| # | Item | Recipe Summary | Sprite Size |
|---|------|----------------|-------------|
| 7 | **Sunfire Pendant** | 12 Solstice Bars + Sunfire Core | 28x28 |
| 8 | **Zenith Band** | 10 Solstice Bars + Solar Essence | 26x26 |
| 9 | **Radiant Crown** | Sunfire Pendant + Zenith Band + Summer Harmonic | 32x28 |

### Autumn Accessories
| # | Item | Recipe Summary | Sprite Size |
|---|------|----------------|-------------|
| 10 | **Reaper's Charm** | 12 Harvest Bars + Death's Note | 28x28 |
| 11 | **Twilight Ring** | 10 Harvest Bars + Twilight Wing Fragment | 24x24 |
| 12 | **Harvest Mantle** | Reaper's Charm + Twilight Ring + Autumn Harmonic | 32x30 |

### Winter Accessories
| # | Item | Recipe Summary | Sprite Size |
|---|------|----------------|-------------|
| 13 | **Frostbite Amulet** | 12 Permafrost Bars + Frozen Core | 28x28 |
| 14 | **Stillness Band** | 10 Permafrost Bars + Shard of Stillness | 26x26 |
| 15 | **Glacial Heart** | Frostbite Amulet + Stillness Band + Winter Harmonic | 30x30 |

---

## 2.3 Seasonal Combination Accessories (4 items)

| # | Item | Recipe Summary | Sprite Size |
|---|------|----------------|-------------|
| 16 | **Equinox Band** | Bloom Crest + Harvest Mantle + Essences | 32x32 |
| 17 | **Solstice Ring** | Radiant Crown + Glacial Heart + Essences | 32x32 |
| 18 | **Cycle of Seasons** | All 4 Base Accessories + Essences | 34x34 |
| 19 | **Vivaldi's Masterwork** | Equinox + Solstice + Cycle + All Harmonics | 36x36 |

---

## 2.4 Phase 2 Asset Checklist

```
BOSSES (4 bosses - multiple sprites each)
[ ] Primavera (main, projectiles, summon item)
[ ] L'Estate (main, projectiles, summon item)
[ ] Autunno (main, projectiles, summon item)
[ ] L'Inverno (main, projectiles, summon item)

BASE ACCESSORIES (12 items)
[ ] Petal Shield
[ ] Growth Band
[ ] Bloom Crest
[ ] Sunfire Pendant
[ ] Zenith Band
[ ] Radiant Crown
[ ] Reaper's Charm
[ ] Twilight Ring
[ ] Harvest Mantle
[ ] Frostbite Amulet
[ ] Stillness Band
[ ] Glacial Heart

COMBINATION ACCESSORIES (4 items)
[ ] Equinox Band
[ ] Solstice Ring
[ ] Cycle of Seasons
[ ] Vivaldi's Masterwork

TOTAL PHASE 2: 4 bosses (12+ sprites) + 16 accessory sprites
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

## 3.2 Theme Resonance Materials (10 items)

*New ore variants for each main theme - world-gen post-Moon Lord*

| Item | Theme | Sprite Size | Colors |
|------|-------|-------------|--------|
| **Moonlit Resonance Ore** | Moonlight Sonata | 16x16 | Purple/silver |
| **Moonlit Resonance Core** | Moonlight Sonata | 18x18 | Refined purple |
| **Heroic Resonance Ore** | Eroica | 16x16 | Scarlet/gold |
| **Heroic Resonance Core** | Eroica | 18x18 | Refined scarlet |
| **Infernal Resonance Ore** | La Campanella | 16x16 | Black/orange |
| **Infernal Resonance Core** | La Campanella | 18x18 | Refined black-orange |
| **Void Resonance Ore** | Enigma | 16x16 | Purple/green |
| **Void Resonance Core** | Enigma | 18x18 | Refined void |
| **Prismatic Resonance Ore** | Swan Lake | 16x16 | White/rainbow |
| **Prismatic Resonance Core** | Swan Lake | 18x18 | Refined prismatic |

## 3.3 Theme Accessories (10 items)

### Moonlight Sonata
| # | Item | Sprite Size |
|---|------|-------------|
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
| 28 | **Feather of Grace** | 28x28 |
| 29 | **Monochromatic Crown** | 32x32 |

---

## 3.4 Phase 3 Asset Checklist

```
PRE-HARDMODE ACCESSORIES (3 items)
[ ] Composer's Notebook
[ ] Resonant Pendant
[ ] Melodic Charm

THEME RESONANCE MATERIALS (10 items)
[ ] Moonlit Resonance Ore
[ ] Moonlit Resonance Core
[ ] Heroic Resonance Ore
[ ] Heroic Resonance Core
[ ] Infernal Resonance Ore
[ ] Infernal Resonance Core
[ ] Void Resonance Ore
[ ] Void Resonance Core
[ ] Prismatic Resonance Ore
[ ] Prismatic Resonance Core

THEME ACCESSORIES (10 items)
[ ] Adagio Pendant
[ ] Sonata's Embrace
[ ] Badge of Valor
[ ] Hero's Symphony
[ ] Chime of Flames
[ ] Infernal Virtuoso
[ ] Puzzle Fragment
[ ] Riddle of the Void
[ ] Feather of Grace
[ ] Monochromatic Crown

TOTAL PHASE 3: 23 item sprites
```

---

# 🟢 PHASE 4: COMBINATION ACCESSORIES
*Requires Phase 2 & 3 accessories*

## 4.1 Two-Theme Combinations (6 items)

| # | Item | Combines | Sprite Size |
|---|------|----------|-------------|
| 30 | **Lunar Flames** | Moonlight + La Campanella | 34x34 |
| 31 | **Heroic Enigma** | Eroica + Enigma | 34x34 |
| 32 | **Graceful Sonata** | Moonlight + Swan Lake | 34x34 |
| 33 | **Blazing Swan** | La Campanella + Swan Lake | 34x34 |
| 34 | **Valor's Mystery** | Eroica + Swan Lake | 34x34 |
| 35 | **Void Flames** | La Campanella + Enigma | 34x34 |

## 4.2 Three-Theme Combinations (4 items)

| # | Item | Combines | Sprite Size |
|---|------|----------|-------------|
| 36 | **Trinity of Night** | Moonlight + Campanella + Enigma | 36x36 |
| 37 | **Heroic Grace** | Eroica + Moonlight + Swan Lake | 36x36 |
| 38 | **Blazing Enigma** | Campanella + Enigma + Swan Lake | 36x36 |
| 39 | **Complete Harmony** | All 5 Themes | 38x38 |

---

## 4.3 Phase 4 Asset Checklist

```
TWO-THEME COMBOS (6 items)
[ ] Lunar Flames
[ ] Heroic Enigma
[ ] Graceful Sonata
[ ] Blazing Swan
[ ] Valor's Mystery
[ ] Void Flames

THREE-THEME COMBOS (4 items)
[ ] Trinity of Night
[ ] Heroic Grace
[ ] Blazing Enigma
[ ] Complete Harmony

TOTAL PHASE 4: 10 accessory sprites
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

```
FATE VANILLA UPGRADES (5 items)
[X] Paradox Chronometer (Melee) - IMPLEMENTED
[X] Constellation Compass (Ranged) - IMPLEMENTED
[X] Astral Conduit (Magic) - IMPLEMENTED
[X] Machination of the Event Horizon (Movement) - IMPLEMENTED
[X] Orrery of Infinite Orbits (Summon) - IMPLEMENTED

GRAND COMBINATIONS (5 items)
[ ] Opus of Four Movements
[ ] Cosmic Warden's Regalia
[ ] Seasonal Destiny
[ ] Theme Wanderer
[ ] Summoner's Magnum Opus

SEASON-THEME HYBRIDS (3 items)
[ ] Spring's Moonlit Garden
[ ] Summer's Infernal Peak
[ ] Winter's Enigmatic Silence

ULTIMATE (1 item)
[ ] Coda of Absolute Harmony

TOTAL PHASE 5: 14 accessory sprites
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

> **STYLE MANDATE:** Everything in MagnumOpus should look WILD, EPIC, and MUSICAL. 
> These aren't generic fantasy items—they're instruments of symphonic destruction.
> Think: "What if a classical composer became a god and forged weapons from pure music?"

---

## 🎵 FOUNDATION MATERIALS (Phase 1)

### Seasonal Bars (Refined Musical Metal)
```
terraria item sprite, Vernal Bar, legendary refined metal ingot radiating spring energy,
pristine white metallic surface with delicate pink cherry blossom veins and soft light blue luminescent edges,
tiny music notes and flower petals seem frozen within the metal, ethereal morning dew shimmer,
20x14 pixels, masterwork pixel art, brilliant highlights, transparent background,
epic fantasy game item, luminous magical material --ar 3:2 --s 250
```

```
terraria item sprite, Solstice Bar, blazing summer metal ingot pulsing with solar intensity,
radiant orange core with white-hot edges, heat waves distorting around it,
musical staff lines etched into surface glowing like sun rays, looks almost too bright to touch,
20x14 pixels, masterwork pixel art, intense radiance, transparent background,
epic fantasy game item, divine solar material --ar 3:2 --s 250
```

```
terraria item sprite, Harvest Bar, twilight metal ingot infused with autumn's final breath,
pale white surface with deep brown wood-grain patterns and dark orange ember veins,
fallen leaves and fading music notes crystallized within, melancholic beauty,
20x14 pixels, masterwork pixel art, warm fading glow, transparent background,
epic fantasy game item, bittersweet magical material --ar 3:2 --s 250
```

```
terraria item sprite, Permafrost Bar, eternal winter metal ingot frozen in absolute zero,
crystalline white ice-metal with light blue frost patterns crackling across surface,
snowflakes and frozen musical notes suspended in perfect stillness within,
20x14 pixels, masterwork pixel art, cold radiance, transparent background,
epic fantasy game item, divine frozen material --ar 3:2 --s 250
```

### Seasonal Essences (Condensed Musical Energy)
```
terraria item sprite, Blossom Essence, swirling orb of concentrated spring vitality,
brilliant pink energy core with white petals and light blue sparkles orbiting within,
tiny cherry blossoms and music notes dance inside like a snow globe of rebirth,
14x14 pixels, ethereal glow, magical particle effects, transparent background,
epic fantasy essence, divine spring energy crystallized --ar 1:1 --s 250
```

```
terraria item sprite, Solar Essence, miniature captured sun radiating summer's fury,
blazing orange plasma sphere with white corona flares bursting outward,
music notes burn like solar flares around the edges, almost painful to look at,
14x14 pixels, intense radiant glow, solar particle effects, transparent background,
epic fantasy essence, bottled solar apocalypse --ar 1:1 --s 250
```

```
terraria item sprite, Decay Essence, haunting orb containing autumn's dying melody,
dark orange and brown energy swirling like falling leaves in a vortex,
white wisps of fading life and ghostly music notes spiral toward the center,
14x14 pixels, melancholic glow, decay particle effects, transparent background,
epic fantasy essence, crystallized ending --ar 1:1 --s 250
```

```
terraria item sprite, Frost Essence, frozen teardrop of eternal winter silence,
pristine white crystalline sphere with light blue ice fractals spreading within,
frozen snowflakes and crystallized music notes suspended in perfect stillness,
14x14 pixels, cold ethereal glow, ice particle effects, transparent background,
epic fantasy essence, absolute zero condensed --ar 1:1 --s 250
```

### Harmonic Essences (Boss-Tier Musical Souls)
```
terraria item sprite, Spring's Harmonic Essence, divine crystallized symphony of rebirth,
magnificent swirling vortex of white pink and light blue energies dancing together,
cherry blossoms music notes and new life spiral outward from radiant core,
looks like holding a piece of spring's first dawn, overwhelming vitality,
20x20 pixels, divine glow effects, masterwork detail, transparent background,
legendary boss drop, concentrated seasonal divinity --ar 1:1 --s 300
```

```
terraria item sprite, Summer's Harmonic Essence, captured heart of the blazing sun,
explosive fusion of orange and white energies like a miniature supernova,
solar flares music staffs and heat waves radiate from impossibly bright center,
looks like holding raw solar fury, burns with eternal summer passion,
20x20 pixels, blinding radiance, masterwork detail, transparent background,
legendary boss drop, concentrated seasonal divinity --ar 1:1 --s 300
```

```
terraria item sprite, Autumn's Harmonic Essence, crystallized final movement of the dying year,
haunting fusion of white brown and dark orange in eternal spiral descent,
falling leaves fading notes and twilight memories frozen in amber moment,
looks like holding the last sunset of autumn, beautiful melancholy,
20x20 pixels, warm fading glow, masterwork detail, transparent background,
legendary boss drop, concentrated seasonal divinity --ar 1:1 --s 300
```

```
terraria item sprite, Winter's Harmonic Essence, frozen silence of the world's end,
crystalline fusion of white and light blue in perfect geometric stillness,
snowflakes ice crystals and silent notes suspended in absolute zero,
looks like holding the final breath before spring, serene finality,
20x20 pixels, cold divine radiance, masterwork detail, transparent background,
legendary boss drop, concentrated seasonal divinity --ar 1:1 --s 300
```

### Enemy Drop Materials
```
terraria item sprite, Petal of Rebirth, legendary flower petal radiating life force,
luminous pink petal with white edges and light blue magical veins,
tiny music notes seem to grow from it like seeds, pulses with spring vitality,
14x14 pixels, soft ethereal glow, transparent background,
rare magical drop, essence of renewal --ar 1:1 --s 200
```

```
terraria item sprite, Ember of Intensity, fragment of captured sunfire,
blazing orange crystal shard with white-hot core and golden sparks,
heat waves distort around it, musical energy crackles like solar flares,
14x14 pixels, intense fiery glow, transparent background,
rare magical drop, piece of summer's fury --ar 1:1 --s 200
```

```
terraria item sprite, Leaf of Ending, final leaf from the tree of seasons,
withered white leaf with brown decay spreading and dark orange veins,
ghostly music notes fade from its surface, beautiful in its death,
14x14 pixels, melancholic fading glow, transparent background,
rare magical drop, autumn's last breath --ar 1:1 --s 200
```

```
terraria item sprite, Shard of Stillness, frozen fragment of absolute silence,
crystalline white ice shard with light blue frost patterns,
frozen music notes trapped within, radiates peaceful cold,
14x14 pixels, cold serene glow, transparent background,
rare magical drop, winter's eternal quiet --ar 1:1 --s 200
```

---

## 🎭 SEASONAL ACCESSORIES (Phase 2)

### Base Tier Accessories
```
terraria accessory sprite, Petal Shield, legendary spring guardian's protection,
ornate white shield with pink cherry blossom patterns and light blue gem center,
musical notes and flower petals orbit around it, radiates gentle vitality,
shield of the spring herald, protector of new beginnings,
28x28 pixels, soft divine glow, masterwork detail, transparent background,
epic fantasy accessory, spring's embrace --ar 1:1 --s 250
```

```
terraria accessory sprite, Sunfire Pendant, blazing summer sun captured in jewelry,
radiant orange gemstone set in white gold frame, solar flares dance within,
musical staffs etched around setting, too bright to look at directly,
pendant of the zenith lord, carrier of eternal summer,
28x28 pixels, intense solar radiance, masterwork detail, transparent background,
epic fantasy accessory, summer's heart --ar 1:1 --s 250
```

```
terraria accessory sprite, Reaper's Charm, twilight talisman of autumn's end,
haunting white bone charm with brown decay patterns and dark orange runes,
ghostly music notes fade around it, carries the weight of endings,
charm of the withering maestro, herald of beautiful death,
28x28 pixels, melancholic glow, masterwork detail, transparent background,
epic fantasy accessory, autumn's final note --ar 1:1 --s 250
```

```
terraria accessory sprite, Frostbite Amulet, eternal winter crystallized into jewelry,
pristine white ice crystal set in light blue frozen metal frame,
snowflakes and frozen notes orbit in perfect stillness around it,
amulet of the silent finale, bringer of serene endings,
28x28 pixels, cold divine radiance, masterwork detail, transparent background,
epic fantasy accessory, winter's embrace --ar 1:1 --s 250
```

### Upgraded Tier Accessories
```
terraria accessory sprite, Bloom Crest, divine spring regalia forged from rebirth,
magnificent white and pink crest with light blue gemstone centerpiece,
cherry blossoms music notes and morning light spiral from its surface,
worn by spring's chosen herald, channel of seasonal resurrection,
30x30 pixels, overwhelming vitality glow, transparent background,
legendary fantasy accessory, spring ascended --ar 1:1 --s 300
```

```
terraria accessory sprite, Radiant Crown, blazing summer coronet of solar dominion,
golden orange crown with white diamond peaks and solar flare gems,
music staffs burn around it like the sun's corona, blindingly magnificent,
crown of the zenith lord, ruler of endless summer,
32x28 pixels, divine solar radiance, transparent background,
legendary fantasy accessory, summer crowned --ar 1:1 --s 300
```

```
terraria accessory sprite, Harvest Mantle, twilight cloak clasp of autumn's requiem,
elegant white and brown mantle pin with dark orange amber gemstones,
falling leaves and fading symphonies swirl eternally around it,
mantle of the withering maestro, conductor of beautiful endings,
32x30 pixels, warm melancholic glow, transparent background,
legendary fantasy accessory, autumn's finale --ar 1:1 --s 300
```

```
terraria accessory sprite, Glacial Heart, frozen core of eternal winter,
crystalline white heart-shaped gem with light blue ice veins,
snowflakes and silent notes suspended in absolute zero stillness,
heart of the silent finale, essence of peaceful endings,
30x30 pixels, cold divine radiance, transparent background,
legendary fantasy accessory, winter's soul --ar 1:1 --s 300
```

### Seasonal Combination Accessories
```
terraria accessory sprite, Vivaldi's Masterwork, legendary fusion of all four seasons,
magnificent circular artifact divided into four sections each season's colors,
white-pink spring flows into orange summer into brown-orange autumn into white-blue winter,
music notes from all seasons orbit in eternal cycle, overwhelming seasonal power,
looks like holding the wheel of the year itself, divine seasonal fusion,
36x36 pixels, cycling seasonal radiance, masterwork detail, transparent background,
ultimate seasonal artifact, Vivaldi's Four Seasons made manifest --ar 1:1 --s 400
```

---

## 🎼 THEME ACCESSORIES (Phase 3-4)

### Moonlight Sonata
```
terraria accessory sprite, Sonata's Embrace, crystallized moonlight symphony,
elegant purple and silver crescent pendant with lunar gems orbiting,
Beethoven's moonlit melody seems to emanate from within, peaceful nocturnal power,
worn by those who understand the moon's melancholy, channel of lunar sorrow,
32x32 pixels, soft ethereal moonlight, masterwork detail, transparent background,
legendary musical artifact, Moonlight Sonata made physical --ar 1:1 --s 300
```

### Eroica
```
terraria accessory sprite, Hero's Symphony, triumphant badge of legendary valor,
magnificent scarlet and gold crest with heroic motifs and crimson gems,
Beethoven's heroic theme thunders from within, overwhelming courage,
worn by those destined for greatness, channel of unstoppable heroism,
32x32 pixels, triumphant golden radiance, masterwork detail, transparent background,
legendary musical artifact, Eroica's heroic spirit forged --ar 1:1 --s 300
```

### La Campanella
```
terraria accessory sprite, Infernal Virtuoso, bell of the damned musician,
ornate black and orange bell-shaped pendant with golden flame accents,
Liszt's impossible melody rings eternally within, infernal virtuosity,
worn by those who sold their souls for skill, channel of hellfire mastery,
32x32 pixels, intense flame radiance, masterwork detail, transparent background,
legendary musical artifact, La Campanella's infernal chime --ar 1:1 --s 300
```

### Enigma Variations
```
terraria accessory sprite, Riddle of the Void, puzzle box of infinite mystery,
impossible geometric artifact of purple black and eerie green,
Elgar's enigma shifts and changes within, maddening unknowable patterns,
worn by those who embrace the unknown, channel of cosmic mystery,
32x32 pixels, unsettling void glow, masterwork detail, transparent background,
legendary musical artifact, Enigma Variations made tangible --ar 1:1 --s 300
```

### Swan Lake
```
terraria accessory sprite, Monochromatic Crown, ballet tiara of tragic grace,
elegant white and black crown with rainbow prismatic gems,
Tchaikovsky's tragic ballet plays eternally within, heartbreaking beauty,
worn by those who dance with death, channel of graceful doom,
32x32 pixels, prismatic elegant radiance, masterwork detail, transparent background,
legendary musical artifact, Swan Lake's dying grace --ar 1:1 --s 300
```

### Multi-Theme Combinations
```
terraria accessory sprite, Complete Harmony, fusion of all five musical themes,
magnificent artifact combining purple silver scarlet gold black orange green white black rainbow,
all five legendary compositions play in perfect harmony within, overwhelming musical power,
Moonlight Eroica Campanella Enigma and Swan Lake united as one divine symphony,
worn by the true maestro, channel of complete musical mastery,
38x38 pixels, shifting theme radiance, masterwork detail, transparent background,
ultimate musical artifact, five compositions unified --ar 1:1 --s 400
```

---

## ⭐ FATE TIER & ULTIMATE (Phase 5)

### Fate Vanilla Upgrades
```
terraria accessory sprite, Paradox Chronometer, cosmic timepiece beyond comprehension,
impossible clock artifact of black pink and crimson with celestial mechanisms,
time itself bends around it, musical notes flow backwards and forwards simultaneously,
gears made of frozen moments, hands pointing to all times at once,
36x36 pixels, reality-warping cosmic glow, masterwork detail, transparent background,
endgame divine artifact, time conquered --ar 1:1 --s 400
```

```
terraria accessory sprite, Machination of the Event Horizon, cloak clasp from beyond existence,
swirling black hole artifact with pink accretion disk and crimson energy jets,
light bends around it, musical compositions from alternate realities echo within,
worn by those who stepped beyond reality and returned, channel of cosmic evasion,
36x36 pixels, gravity-distorting void radiance, masterwork detail, transparent background,
endgame divine artifact, physics defied --ar 1:1 --s 400
```

### Grand Combinations
```
terraria accessory sprite, Opus of Four Movements, ultimate seasonal-musical fusion,
divine artifact combining all seasons AND all themes in impossible harmony,
spring summer autumn winter AND moonlight eroica campanella enigma swan lake,
the complete musical year and all compositions unified in single overwhelming piece,
looks like holding the concept of music itself, reality strains around it,
40x40 pixels, reality-transcending radiance, masterwork detail, transparent background,
divine fusion artifact, seasons and themes united --ar 1:1 --s 500
```

```
terraria accessory sprite, Cosmic Warden's Regalia, armor of the universe's guardian,
magnificent cosmic artifact combining all Fate-tier upgrades into divine regalia,
black pink crimson energy swirls with celestial mechanisms and star fragments,
time space and reality bow to its wearer, ultimate cosmic authority,
42x42 pixels, universe-commanding radiance, masterwork detail, transparent background,
divine cosmic artifact, warden of existence --ar 1:1 --s 500
```

### THE ULTIMATE
```
terraria accessory sprite, Coda of Absolute Harmony, the final note of creation,
TRANSCENDENT artifact beyond mortal comprehension, fusion of EVERYTHING,
all seasons all themes all cosmic power unified in single impossible object,
white core with every color spiraling outward in perfect musical mathematical harmony,
looking at it plays every composition simultaneously in your mind,
this is what happens when music becomes god, overwhelming divine presence,
48x48 pixels, existence-defining radiance, ultimate masterwork, transparent background,
the ultimate artifact, music itself given form, Coda of Absolute Harmony --ar 1:1 --s 750
```

---

## 🎪 BOSS SPRITES (Phase 2)

```
terraria boss sprite, Primavera Herald of Bloom, divine embodiment of spring,
magnificent humanoid figure composed of cherry blossoms white petals and light blue wind,
crown of flowering branches, gown of morning dew, conducts with flowering baton,
music notes and flower petals swirl around in eternal spring vortex,
looks like if spring itself decided to become a god and judge mortals,
120x120+ pixels, overwhelming spring vitality, boss encounter sprite, transparent background,
divine seasonal boss, Vivaldi's Spring given terrifying life --ar 1:1 --s 400
```

```
terraria boss sprite, L'Estate Lord of the Zenith, blazing god of summer's peak,
towering figure of pure solar flame orange fire and white-hot plasma,
crown of solar flares, armor of crystallized sunlight, wields conductor's staff of pure heat,
music staffs burn around like the sun's corona, heat waves distort reality,
looks like the sun itself descended to conduct the world's final summer,
140x140+ pixels, blinding solar radiance, boss encounter sprite, transparent background,
divine seasonal boss, Vivaldi's Summer made apocalyptic --ar 1:1 --s 400
```

```
terraria boss sprite, Autunno the Withering Maestro, haunting conductor of endings,
elegant skeletal figure draped in white robes with brown decay and dark orange embers,
crown of dying leaves, baton of petrified wood, conducting the world's final movement,
falling leaves and fading notes spiral around in eternal twilight descent,
looks like death learned to appreciate beauty and became an artist,
130x130+ pixels, melancholic warm glow, boss encounter sprite, transparent background,
divine seasonal boss, Vivaldi's Autumn conducting the requiem --ar 1:1 --s 400
```

```
terraria boss sprite, L'Inverno the Silent Finale, frozen god of winter's end,
towering crystalline figure of white ice and light blue frost, perfectly still,
crown of icicles, frozen robes that never move, holds silent baton of absolute zero,
snowflakes and frozen notes suspended in eternal stillness around it,
looks like if silence itself became visible and decided to end all sound,
150x150+ pixels, cold divine stillness, boss encounter sprite, transparent background,
divine seasonal boss, Vivaldi's Winter bringing eternal silence --ar 1:1 --s 400
```

---

## 💊 UTILITIES (Phase 6)

### Potions
```
terraria potion sprite, Elixir of the Maestro, legendary conductor's brew,
ornate golden bottle with swirling rainbow liquid and musical note bubbles,
drinking it lets you hear the music of combat itself, divine performance enhancement,
16x24 pixels, prismatic glow, masterwork detail, transparent background,
legendary consumable, maestro's secret --ar 2:3 --s 250
```

### Permanent Upgrades
```
terraria item sprite, Fate's Blessing, crystallized divine favor,
impossible geometric crystal of black pink and crimson cosmic energy,
holding it makes you feel like the universe itself is watching approvingly,
one-time consumption grants permanent cosmic attunement,
20x20 pixels, reality-affirming glow, masterwork detail, transparent background,
divine permanent upgrade, cosmic approval --ar 1:1 --s 300
```

```
terraria item sprite, Coda's Echo, resonance of the ultimate artifact,
fragment of pure harmonic energy containing echo of Coda of Absolute Harmony,
every color and every note condensed into single overwhelming shard,
consuming it permanently attunes you to the music of existence,
22x22 pixels, existence-resonating glow, masterwork detail, transparent background,
ultimate permanent upgrade, echo of perfection --ar 1:1 --s 400
```

---

*Document Version 2.1 - Reorganized by Implementation Phase + Epic Prompt Templates*
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
| **Fate's Cosmic Symphony** | Swan's Perfect Measure + 30 Fate Resonant Cores + Fate Harmonic | Max 60 Resonance. Consume 50: reality-rending slash that hits all on-screen enemies | T6 |

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
| **Fate's Cosmic Verdict** | Swan's Graceful Hunt + 30 Fate Resonant Cores + Fate Harmonic | Marked enemies take +12% damage. Killing marked boss drops bonus loot bag | T6 |

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
| **Fate's Cosmic Reservoir** | Swan's Balanced Flow + 30 Fate Resonant Cores + Fate Harmonic | Overflow to -200. At -150: spells bend reality, hitting enemies through walls | T6 |

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
| **Fate's Cosmic Dominion** | Swan's Graceful Direction + 30 Fate Resonant Cores + Fate Harmonic | Conduct cooldown 5s. "Finale": hold Conduct 2s to sacrifice all minions for massive single hit | T6 |

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
| **Fate's Cosmic Aegis** | Swan's Immortal Grace + 30 Fate Resonant Cores + Fate Harmonic | Shield = 60% HP. Break triggers "Last Stand": invincible for 3s, once per 2 minutes | T6 |

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
| **Fate's Cosmic Velocity** | Swan's Eternal Glide + 30 Fate Resonant Cores + Fate Harmonic | Momentum max 150. At 150: **time slows 20%** for enemies near you | T6 |

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
| **Cosmic Throne of Fate** | Fate Harmonic + 50 Fate Resonant Cores + All Theme Harmonics | 95 mph | Infinite | Constellation throne |

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

## 7.10 Phase 7 Asset Checklist

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

*Document Version 3.1 - Restructured Phase 7: Progressive Accessory Chains*
*Four Seasons spread across Eye of Cthulhu → Moon Lord progression*
*Last Updated: January 2026*
