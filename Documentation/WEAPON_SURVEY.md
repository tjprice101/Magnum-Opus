# MagnumOpus — Complete Weapon Survey

> Generated from full codebase analysis. Every weapon item class is listed with its theme, weapon class, description, sub-projectiles, VFX systems, and custom shaders.

**Total weapons: ~120** (across 14 themes + 2 sandbox + 1 foundation set)

---

## Table of Contents

1. [Moonlight Sonata](#moonlight-sonata-6-weapons)
2. [Eroica](#eroica-7-weapons)
3. [La Campanella](#la-campanella-7-weapons)
4. [Enigma Variations](#enigma-variations-8-weapons)
5. [Swan Lake](#swan-lake-7-weapons)
6. [Fate](#fate-10-weapons)
7. [Dies Irae](#dies-irae-12-weapons)
8. [Ode to Joy](#ode-to-joy-12-weapons)
9. [Nachtmusik](#nachtmusik-11-weapons)
10. [Clair de Lune](#clair-de-lune-12-weapons)
11. [Seasons (Vivaldi's Arsenal)](#seasons-vivaldis-arsenal-4-weapons)
12. [Spring](#spring-4-weapons)
13. [Summer](#summer-4-weapons)
14. [Autumn](#autumn-4-weapons)
15. [Winter](#winter-4-weapons)
16. [Sandbox (Test Weapons)](#sandbox-test-weapons-2-weapons)
17. [Foundation Weapons (Test/Debug)](#foundation-weapons-testdebug-15-weapons)
18. [Documentation Folder Contents](#documentation-folder-contents)

---

## Moonlight Sonata (6 weapons)

### 1. Incisor of Moonlight
| Field | Value |
|-------|-------|
| **Class** | `IncisorOfMoonlight` |
| **File** | `Content/MoonlightSonata/Weapons/IncisorOfMoonlight/IncisorOfMoonlight.cs` |
| **Weapon Class** | Melee (MeleeNoSpeed) |
| **Description** | True melee broadsword with multi-phase combo. Constellation slash creators, crescent moon waves, staccato/orbiting note projectiles, lunar nova detonation, lunar beam. |
| **Projectiles** | IncisorSwingProj, ConstellationSlashCreator, ConstellationSlash, CrescentMoonProj, CrescentWaveProj, StaccatoNoteProj, OrbitingNoteProj, LunarNova, LunarBeamProj |
| **Particles** | IncisorParticle, ConstellationSparkParticle, LunarMoteParticle, MoonlightMistParticle |
| **Primitives** | IncisorVertex, IncisorSettings, IncisorTrailRenderer |
| **Shaders** | IncisorShaderLoader |
| **Buffs/Debuffs** | LunarResonanceDebuff, MoonlitStasis, MoonlitSilenceDebuff |

### 2. Eternal Moon
| Field | Value |
|-------|-------|
| **Class** | `EternalMoon` |
| **File** | `Content/MoonlightSonata/Weapons/EternalMoon/EternalMoon.cs` |
| **Weapon Class** | Melee (MeleeNoSpeed) |
| **Description** | Endgame melee greatsword with tidal wave swings, ghost projections, crescent slashes, and tidal detonations. Full self-contained VFX. |
| **Projectiles** | EternalMoonSwing, EternalMoonWave, EternalMoonGhost, EternalMoonCrescentSlash, EternalMoonTidalDetonation |
| **Particles** | LunarParticle system |
| **Primitives** | LunarTrailRenderer |
| **Shaders** | EternalMoonShaderLoader |
| **Buffs/Debuffs** | EternalMoonDebuffs |

### 3. Resurrection of the Moon
| Field | Value |
|-------|-------|
| **Class** | `ResurrectionOfTheMoon` |
| **File** | `Content/MoonlightSonata/Weapons/ResurrectionOfTheMoon/ResurrectionOfTheMoon.cs` |
| **Weapon Class** | Ranged |
| **Description** | Ranged weapon that fires supernova shells and comet core projectiles with lunar impact effects. |
| **Projectiles** | ResurrectionProjectile, SupernovaShell, CometCore |
| **Particles** | CometParticle system |
| **Primitives** | CometTrailRenderer |
| **Shaders** | CometShaderLoader |
| **Buffs/Debuffs** | LunarImpact |

### 4. Moonlight's Calling
| Field | Value |
|-------|-------|
| **Class** | `MoonlightsCalling` |
| **File** | `Content/MoonlightSonata/Weapons/MoonlightsCalling/MoonlightsCalling.cs` |
| **Weapon Class** | Magic |
| **Description** | Channeled magic serenade weapon. Holdout beam with spectral child beams and prismatic detonation on release. |
| **Projectiles** | SerenadeHoldout, SerenadeBeam, SpectralChildBeam, PrismaticDetonation |
| **Particles** | SerenadeParticle |
| **Primitives** | SerenadeTrailRenderer |
| **Shaders** | SerenadeShaderLoader |
| **Buffs/Debuffs** | MusicalDissonance |

### 5. Staff of the Lunar Phases
| Field | Value |
|-------|-------|
| **Class** | `StaffOfTheLunarPhases` |
| **File** | `Content/MoonlightSonata/Weapons/StaffOfTheLunarPhases/StaffOfTheLunarPhases.cs` |
| **Weapon Class** | Summon |
| **Description** | Summon staff that calls the Goliath of Moonlight. Minion fires moonlight beams and devastating charged beams. |
| **Projectiles** | GoliathMoonlightBeam, GoliathDevastatingBeam |
| **Particles** | GoliathParticle system |
| **Primitives** | GoliathTrailRenderer |
| **Shaders** | GoliathShaderLoader |
| **Dusts** | GoliathDust |

### 6. Four Seasons Blade *(cross-listed under Seasons)*
| Field | Value |
|-------|-------|
| **Class** | `FourSeasonsBlade` (extends `MeleeSwingItemBase`) |
| **File** | `Content/Seasons/Weapons/FourSeasonsBlade/FourSeasonsBlade.cs` |
| **Weapon Class** | Melee (MeleeNoSpeed) |
| **Description** | Cycles through Spring→Summer→Autumn→Winter combo phases. Each season has unique debuffs, healing, and VFX. Every 4th complete cycle triggers Crescendo burst. |
| **Projectiles** | FourSeasonsBladeSwing |

---

## Eroica (7 weapons)

### 1. Celestial Valor
| Field | Value |
|-------|-------|
| **Class** | `CelestialValor` |
| **File** | `Content/Eroica/ResonantWeapons/CelestialValor/CelestialValor.cs` |
| **Weapon Class** | Melee (MeleeNoSpeed) |
| **Description** | Heroic broadsword with valor slash arcs, beam projectiles, and multi-layered heroic VFX. |
| **Projectiles** | CelestialValorSwing, CelestialValorProjectile, ValorSlashCreator, ValorSlash, ValorBoom, ValorBeam |
| **Particles** | ValorParticle |
| **Primitives** | ValorTrailRenderer |
| **Shaders** | ValorShaderLoader |
| **Dusts** | FlameRibbonDust, ValorEmberDust, ValorCrestDust, HeroicSparkDust, HeroicEmberDust |
| **Buffs/Debuffs** | ValorDebuffs |

### 2. Sakura's Blossom
| Field | Value |
|-------|-------|
| **Class** | `SakurasBlossom` |
| **File** | `Content/Eroica/ResonantWeapons/SakurasBlossom/SakurasBlossom.cs` |
| **Weapon Class** | Melee (MeleeNoSpeed) |
| **Description** | Sakura-petal-themed melee swing weapon with cherry blossom trail effects. |
| **Projectiles** | SakurasBlossomSwing |
| **Particles** | SakuraParticle |
| **Primitives** | SakuraTrailRenderer |
| **Shaders** | SakuraShaderLoader |
| **Dusts** | SakuraPetalDust |
| **Buffs/Debuffs** | SakuraDebuffs |

### 3. Blossom of the Sakura
| Field | Value |
|-------|-------|
| **Class** | `BlossomOfTheSakura` |
| **File** | `Content/Eroica/ResonantWeapons/BlossomOfTheSakura/BlossomOfTheSakura.cs` |
| **Weapon Class** | Ranged |
| **Description** | Ranged weapon with sakura blossom themed projectiles and tracer effects. |
| **Particles** | BlossomParticle |
| **Primitives** | BlossomTrailRenderer |
| **Shaders** | BlossomShaderLoader |
| **Dusts** | BlossomTracerDust |

### 4. Piercing Light of the Sakura
| Field | Value |
|-------|-------|
| **Class** | `PiercingLightOfTheSakura` |
| **File** | `Content/Eroica/ResonantWeapons/PiercingLightOfTheSakura/PiercingLightOfTheSakura.cs` |
| **Weapon Class** | Ranged |
| **Description** | Piercing ranged weapon with radiant sakura light projectiles. |
| **Particles** | PiercingParticle |
| **Primitives** | PiercingTrailRenderer |
| **Shaders** | PiercingShaderLoader |
| **Dusts** | PiercingLightDust |

### 5. Triumphant Fractal
| Field | Value |
|-------|-------|
| **Class** | `TriumphantFractal` |
| **File** | `Content/Eroica/ResonantWeapons/TriumphantFractal/TriumphantFractal.cs` |
| **Weapon Class** | Magic |
| **Description** | Fractal magic weapon with heroic energy projectiles. |
| **Particles** | FractalParticle |
| **Primitives** | FractalTrailRenderer |
| **Shaders** | FractalShaderLoader |
| **Dusts** | FractalDust |

### 6. Funeral Prayer
| Field | Value |
|-------|-------|
| **Class** | `FuneralPrayer` |
| **File** | `Content/Eroica/ResonantWeapons/FuneralPrayer/FuneralPrayer.cs` |
| **Weapon Class** | Magic |
| **Description** | Dark heroic magic weapon themed around sacrifice. Funeral ash effects. |
| **Particles** | FuneralParticle |
| **Primitives** | FuneralTrailRenderer |
| **Shaders** | FuneralShaderLoader |
| **Dusts** | FuneralAshDust |

### 7. Finality of the Sakura
| Field | Value |
|-------|-------|
| **Class** | `FinalityOfTheSakura` |
| **File** | `Content/Eroica/ResonantWeapons/FinalityOfTheSakura/FinalityOfTheSakura.cs` |
| **Weapon Class** | Summon |
| **Description** | Summoner weapon themed around the final fall of sakura petals. |
| **Particles** | FinalityParticle |
| **Primitives** | FinalityTrailRenderer |
| **Shaders** | FinalityShaderLoader |
| **Dusts** | FinalityDust |

---

## La Campanella (7 weapons)

### 1. Dual Fated Chime
| Field | Value |
|-------|-------|
| **Class** | `DualFatedChime` |
| **File** | `Content/LaCampanella/ResonantWeapons/DualFatedChime/DualFatedChime.cs` |
| **Weapon Class** | Melee (MeleeNoSpeed) |
| **Description** | Twin chime melee weapon with inferno waltz swing pattern and bell flame wave projectiles. |
| **Projectiles** | DualFatedChimeSwingProj, InfernoWaltzProj, BellFlameWaveProj |
| **Particles** | DualFatedChimeParticle |
| **Primitives** | DualFatedChimePrimitiveRenderer |
| **Shaders** | DualFatedChimeShaderLoader |

### 2. Ignition of the Bell
| Field | Value |
|-------|-------|
| **Class** | `IgnitionOfTheBell` |
| **File** | `Content/LaCampanella/ResonantWeapons/IgnitionOfTheBell/IgnitionOfTheBell.cs` |
| **Weapon Class** | Melee (MeleeNoSpeed) |
| **Description** | Thrust-based melee weapon with infernal geyser pillars and chime cyclone attacks. |
| **Projectiles** | IgnitionThrustProj, InfernalGeyserProj, ChimeCycloneProj |
| **Particles** | IgnitionOfTheBellParticle |
| **Primitives** | IgnitionOfTheBellPrimitiveRenderer |
| **Shaders** | IgnitionOfTheBellShaderLoader |

### 3. Symphonic Bellfire Annihilator
| Field | Value |
|-------|-------|
| **Class** | `SymphonicBellfireAnnihilatorItem` |
| **File** | `Content/LaCampanella/ResonantWeapons/SymphonicBellfireAnnihilator/SymphonicBellfireAnnihilatorItem.cs` |
| **Weapon Class** | Ranged |
| **Description** | Heavy ranged weapon firing grand crescendo waves and bellfire rockets. Buff-stacking system. |
| **Projectiles** | GrandCrescendoWaveProj, BellfireRocketProj |
| **Particles** | SymphonicBellfireParticle |
| **Primitives** | SymphonicBellfirePrimitiveRenderer |
| **Shaders** | SymphonicBellfireShaderLoader |
| **Buffs** | GrandCrescendoBuff, BellfireCrescendoBuff |

### 4. Piercing Bell's Resonance
| Field | Value |
|-------|-------|
| **Class** | `PiercingBellsResonanceItem` |
| **File** | `Content/LaCampanella/ResonantWeapons/PiercingBellsResonance/PiercingBellsResonanceItem.cs` |
| **Weapon Class** | Ranged |
| **Description** | Precision ranged weapon with staccato bullets, seeking crystals, resonant note projectiles, and resonant blast detonation. |
| **Projectiles** | StaccatoBulletProj, SeekingCrystalProj, ResonantNoteProj, ResonantBlastProj |
| **Particles** | PiercingBellsParticle |
| **Primitives** | PiercingBellsPrimitiveRenderer |
| **Shaders** | PiercingBellsResonanceShaderLoader |

### 5. Grandiose Chime
| Field | Value |
|-------|-------|
| **Class** | `GrandioseChimeItem` |
| **File** | `Content/LaCampanella/ResonantWeapons/GrandioseChime/GrandioseChimeItem.cs` |
| **Weapon Class** | Ranged |
| **Description** | Ranged weapon firing grandiose beams, bellfire notes, deployable note mines, and kill echo chain projectiles. |
| **Projectiles** | GrandioseBeamProj, BellfireNoteProj, NoteMineProj, KillEchoProj |
| **Particles** | GrandioseChimeParticle |
| **Primitives** | GrandioseChimePrimitiveRenderer |
| **Shaders** | GrandioseChimeShaderLoader |

### 6. Fang of the Infinite Bell
| Field | Value |
|-------|-------|
| **Class** | `FangOfTheInfiniteBell` |
| **File** | `Content/LaCampanella/ResonantWeapons/FangOfTheInfiniteBell/FangOfTheInfiniteBell.cs` |
| **Weapon Class** | Magic |
| **Description** | Magic weapon launching infinite bell orbs and empowered lightning projectiles. Stacking damage buff system. |
| **Projectiles** | InfiniteBellOrbProj, EmpoweredLightningProj |
| **Particles** | FangOfTheInfiniteBellParticle |
| **Primitives** | FangOfTheInfiniteBellPrimitiveRenderer |
| **Shaders** | FangOfTheInfiniteBellShaderLoader |
| **Buffs** | InfiniteBellDamageBuff, InfiniteBellEmpoweredBuff |

### 7. Infernal Chimes' Calling
| Field | Value |
|-------|-------|
| **Class** | `InfernalChimesCallingItem` |
| **File** | `Content/LaCampanella/ResonantWeapons/InfernalChimesCalling/InfernalChimesCallingItem.cs` |
| **Weapon Class** | Summon |
| **Description** | Summon staff that calls a Campanella Choir minion. Minion fires shockwave projectiles. |
| **Projectiles** | CampanellaChoirMinion, MinionShockwaveProj |
| **Particles** | InfernalChimesParticle |
| **Primitives** | InfernalChimesPrimitiveRenderer |
| **Shaders** | InfernalChimesCallingShaderLoader |
| **Buffs** | CampanellaChoirBuff |

---

## Enigma Variations (8 weapons)

### 1. The Unresolved Cadence
| Field | Value |
|-------|-------|
| **Class** | `TheUnresolvedCadenceItem` (extends `MeleeSwingItemBase`) |
| **File** | `Content/EnigmaVariations/ResonantWeapons/Items/TheUnresolvedCadenceItem.cs` |
| **Weapon Class** | Melee |
| **Description** | Ultimate Enigma broadsword. Every swing stacks Inevitability on enemies; at 10 stacks triggers Paradox Collapse. Spawns DimensionalSlash sub-projectiles. On-hit: ParadoxBrand + SeekingCrystals. |

### 2. Variations of the Void
| Field | Value |
|-------|-------|
| **Class** | `VariationsOfTheVoidItem` (extends `MeleeSwingItemBase`) |
| **File** | `Content/EnigmaVariations/ResonantWeapons/Items/VariationsOfTheVoidItem.cs` |
| **Weapon Class** | Melee |
| **Description** | Every third strike summons three converging void beams. Beams resonate when aligned. Hits apply Paradox Brand. |

### 3. Dissonance of Secrets
| Field | Value |
|-------|-------|
| **Class** | `DissonanceOfSecrets` |
| **File** | `Content/EnigmaVariations/ResonantWeapons/DissonanceOfSecrets/DissonanceOfSecrets.cs` |
| **Weapon Class** | Magic |
| **Description** | Enigma magic weapon shrouded in secrecy and dissonant void energy. |
| **Particles** | DissonanceParticle |
| **Primitives** | DissonancePrimitiveRenderer |
| **Shaders** | DissonanceShaderLoader |
| **Dusts** | DissonanceSecretDust |

### 4. Fugue of the Unknown
| Field | Value |
|-------|-------|
| **Class** | `FugueOfTheUnknown` |
| **File** | `Content/EnigmaVariations/ResonantWeapons/FugueOfTheUnknown/FugueOfTheUnknown.cs` |
| **Weapon Class** | Magic |
| **Description** | Fugue-structured magic weapon with echoing unknown energy. |
| **Particles** | FugueParticle |
| **Primitives** | FuguePrimitiveRenderer |
| **Shaders** | FugueShaderLoader |
| **Dusts** | FugueEchoDust |

### 5. Cipher Nocturne
| Field | Value |
|-------|-------|
| **Class** | `CipherNocturne` |
| **File** | `Content/EnigmaVariations/ResonantWeapons/CipherNocturne/CipherNocturne.cs` |
| **Weapon Class** | Magic |
| **Description** | Encrypted void magic weapon with cipher-themed effects. |
| **Particles** | CipherParticle |
| **Primitives** | CipherPrimitiveRenderer |
| **Shaders** | CipherShaderLoader |
| **Dusts** | CipherVoidDust |

### 6. The Silent Measure
| Field | Value |
|-------|-------|
| **Class** | `TheSilentMeasure` |
| **File** | `Content/EnigmaVariations/ResonantWeapons/TheSilentMeasure/TheSilentMeasure.cs` |
| **Weapon Class** | Ranged |
| **Description** | Ranged weapon embodying silence and measured restraint. |
| **Particles** | SilentParticle |
| **Primitives** | SilentPrimitiveRenderer |
| **Shaders** | SilentShaderLoader |
| **Dusts** | SilentMeasureDust |

### 7. Tacet's Enigma
| Field | Value |
|-------|-------|
| **Class** | `TacetsEnigma` |
| **File** | `Content/EnigmaVariations/ResonantWeapons/TacetsEnigma/TacetsEnigma.cs` |
| **Weapon Class** | Ranged |
| **Description** | Ranged weapon themed around the musical "tacet" (silence). |
| **Particles** | TacetParticle |
| **Primitives** | TacetPrimitiveRenderer |
| **Shaders** | TacetShaderLoader |
| **Dusts** | TacetSilenceDust |

### 8. The Watching Refrain
| Field | Value |
|-------|-------|
| **Class** | `TheWatchingRefrain` |
| **File** | `Content/EnigmaVariations/ResonantWeapons/TheWatchingRefrain/TheWatchingRefrain.cs` |
| **Weapon Class** | Summon |
| **Description** | Summoner weapon themed around watching eyes and repeating refrains. |
| **Shaders** | WatchingShaderLoader |

---

## Swan Lake (7 weapons)

### 1. Call of the Black Swan
| Field | Value |
|-------|-------|
| **Class** | `CalloftheBlackSwan` |
| **File** | `Content/SwanLake/ResonantWeapons/CalloftheBlackSwan/CalloftheBlackSwan.cs` |
| **Weapon Class** | Melee (MeleeNoSpeed) |
| **Description** | Melee sword with black swan swing arcs and flare projectiles. |
| **Projectiles** | BlackSwanSwingProj, BlackSwanFlareProj |
| **Particles** | BlackSwanParticle |
| **Primitives** | BlackSwanPrimitiveRenderer |
| **Shaders** | BlackSwanShaderLoader |

### 2. The Swan's Lament
| Field | Value |
|-------|-------|
| **Class** | `TheSwansLament` |
| **File** | `Content/SwanLake/ResonantWeapons/TheSwansLament/TheSwansLament.cs` |
| **Weapon Class** | Ranged |
| **Description** | Ranged weapon firing lament bullets and destruction halo projectiles. |
| **Projectiles** | LamentBulletProj, DestructionHaloProj |
| **Particles** | LamentParticle |
| **Primitives** | LamentPrimitiveRenderer |
| **Shaders** | LamentShaderLoader |

### 3. Call of the Pearlescent Lake
| Field | Value |
|-------|-------|
| **Class** | `CallofthePearlescentLake` |
| **File** | `Content/SwanLake/ResonantWeapons/CallofthePearlescentLake/CallofthePearlescentLake.cs` |
| **Weapon Class** | Ranged |
| **Description** | Ranged weapon launching pearlescent rocket projectiles. |
| **Projectiles** | PearlescentRocketProj |
| **Particles** | PearlescentParticle |
| **Primitives** | PearlescentPrimitiveRenderer |
| **Shaders** | PearlescentShaderLoader |

### 4. Iridescent Wingspan
| Field | Value |
|-------|-------|
| **Class** | `IridescentWingspan` |
| **File** | `Content/SwanLake/ResonantWeapons/IridescentWingspan/IridescentWingspan.cs` |
| **Weapon Class** | Magic |
| **Description** | Magic weapon firing iridescent wingspan bolts with rainbow shimmer. |
| **Projectiles** | WingspanBoltProj |
| **Particles** | WingspanParticle |
| **Primitives** | WingspanPrimitiveRenderer |
| **Shaders** | WingspanShaderLoader |

### 5. Chromatic Swan Song
| Field | Value |
|-------|-------|
| **Class** | `ChromaticSwanSong` |
| **File** | `Content/SwanLake/ResonantWeapons/ChromaticSwanSong/ChromaticSwanSong.cs` |
| **Weapon Class** | Magic |
| **Description** | Magic weapon firing chromatic bolts with aria detonation explosions. |
| **Projectiles** | ChromaticBoltProj, AriaDetonationProj |
| **Particles** | ChromaticParticle |
| **Primitives** | ChromaticPrimitiveRenderer |
| **Shaders** | ChromaticShaderLoader |

### 6. Feather of the Iridescent Flock
| Field | Value |
|-------|-------|
| **Class** | `FeatheroftheIridescentFlock` |
| **File** | `Content/SwanLake/ResonantWeapons/FeatheroftheIridescentFlock/FeatheroftheIridescentFlock.cs` |
| **Weapon Class** | Summon |
| **Description** | Summon weapon calling an iridescent crystal minion. |
| **Projectiles** | IridescentCrystalProj |
| **Particles** | FlockParticle |
| **Primitives** | FlockPrimitiveRenderer |
| **Shaders** | FlockShaderLoader |
| **Buffs** | IridescentFlockBuff |

### 7. Feather's Call *(Special — not a standard weapon)*
| Field | Value |
|-------|-------|
| **Class** | `FeathersCall` |
| **File** | `Content/SwanLake/ResonantWeapons/FeathersCall/FeathersCall.cs` |
| **Weapon Class** | Special (Transformation Item) |
| **Description** | Rare 1% drop from Swan Lake boss. Drains mana continuously to transform the player into a mini Swan Lake boss with unique attacks. Not a traditional weapon. |

---

## Fate (10 weapons)

### 1. Requiem of Reality
| Field | Value |
|-------|-------|
| **Class** | `RequiemOfRealityItem` |
| **File** | `Content/Fate/ResonantWeapons/RequiemOfReality/RequiemOfRealityItem.cs` |
| **Weapon Class** | Melee |
| **Description** | Fate-theme melee sword with spectral blade projectiles and cosmic note attacks. |
| **Projectiles** | RequiemSwingProjectile, RequiemSpectralBlade, RequiemCosmicNote |
| **Particles** | RequiemParticleHandler |
| **Primitives** | RequiemTrailRenderer |
| **Shaders** | RequiemShaderLoader |
| **Utilities** | RequiemPlayer, RequiemUtils |

### 2. The Conductor's Last Constellation
| Field | Value |
|-------|-------|
| **Class** | `TheConductorsLastConstellationItem` |
| **File** | `Content/Fate/ResonantWeapons/TheConductorsLastConstellation/TheConductorsLastConstellationItem.cs` |
| **Weapon Class** | Melee |
| **Description** | Fate melee sword with held swing projectile and sword beam release. Self-contained VFX system. |
| **Projectiles** | ConductorSwingProjectile, ConductorSwordBeam |
| **Particles** | ConductorParticleHandler, ConductorParticleTypes |
| **Primitives** | ConductorTrailRenderer, ConductorTrailSettings, ConductorVertexType |
| **Shaders** | ConductorShaderLoader |
| **Utilities** | ConductorPlayer, ConductorUtils |

### 3. Coda of Annihilation
| Field | Value |
|-------|-------|
| **Class** | `CodaOfAnnihilationItem` |
| **File** | `Content/Fate/ResonantWeapons/CodaOfAnnihilation/CodaOfAnnihilationItem.cs` |
| **Weapon Class** | Melee |
| **Description** | Zenith-style Fate melee weapon. Fires Coda Zenith Sword projectiles and held swing. Self-contained VFX. |
| **Projectiles** | CodaZenithSword, CodaHeldSwing |
| **Particles** | CodaParticleHandler, CodaParticleTypes |
| **Primitives** | CodaTrailRenderer, CodaVertexType |
| **Shaders** | CodaShaderLoader |
| **Utilities** | CodaPlayer, CodaUtils |

### 4. Opus Ultima
| Field | Value |
|-------|-------|
| **Class** | `OpusUltimaItem` |
| **File** | `Content/Fate/ResonantWeapons/OpusUltima/OpusUltimaItem.cs` |
| **Weapon Class** | Melee |
| **Description** | The Magnum Opus — culmination of all musical training. 3-movement combo (Exposition→Development→Recapitulation). Each swing fires energy balls that explode into 5 homing seekers. On melee hit: DestinyCollapse + seeking crystal shards. 720 damage. |
| **Projectiles** | OpusSwingProjectile, OpusEnergyBallProjectile |
| **Particles** | OpusParticleHandler, OpusParticleTypes |
| **Primitives** | OpusTrailRenderer, OpusTrailSettings, OpusVertexType |
| **Shaders** | OpusShaderLoader |
| **Utilities** | OpusPlayer, OpusUtils |

### 5. Fractal of the Stars
| Field | Value |
|-------|-------|
| **Class** | `FractalOfTheStarsItem` |
| **File** | `Content/Fate/ResonantWeapons/FractalOfTheStars/FractalOfTheStarsItem.cs` |
| **Weapon Class** | Melee |
| **Description** | Blade forged from shattered constellations. 3-phase combo: Horizontal Sweep→Rising Uppercut→Gravity Slam. On hit: spawns orbiting spectral star blades (max 6). Every 3rd hit: Star Fracture geometric explosion. Orbit blades fire prismatic beams. 850 damage. |
| **Projectiles** | FractalSwingProjectile, FractalOrbitBlade |
| **Particles** | FractalParticleHandler |
| **Primitives** | FractalTrailRenderer |
| **Shaders** | FractalShaderLoader |
| **Utilities** | FractalPlayer |

### 6. Resonance of a Bygone Reality
| Field | Value |
|-------|-------|
| **Class** | `ResonanceOfABygoneRealityItem` |
| **File** | `Content/Fate/ResonantWeapons/ResonanceOfABygoneReality/ResonanceOfABygoneRealityItem.cs` |
| **Weapon Class** | Ranged |
| **Description** | Fate ranged weapon with spectral blade and rapid bullet projectiles. |
| **Projectiles** | ResonanceSpectralBlade, ResonanceRapidBullet |
| **Particles** | ResonanceParticle |
| **Primitives** | ResonanceTrailRenderer |
| **Shaders** | ResonanceShaderLoader |

### 7. Light of the Future
| Field | Value |
|-------|-------|
| **Class** | `LightOfTheFutureItem` |
| **File** | `Content/Fate/ResonantWeapons/LightOfTheFuture/LightOfTheFutureItem.cs` |
| **Weapon Class** | Ranged |
| **Description** | Fate ranged weapon firing accelerating light bullets and cosmic rocket projectiles. |
| **Projectiles** | LightAcceleratingBullet, LightCosmicRocket |
| **Particles** | LightParticle |
| **Shaders** | LightShaderLoader |

### 8. The Final Fermata
| Field | Value |
|-------|-------|
| **Class** | `TheFinalFermataItem` |
| **File** | `Content/Fate/ResonantWeapons/TheFinalFermata/TheFinalFermataItem.cs` |
| **Weapon Class** | Magic |
| **Description** | Fate magic weapon launching spectral swords and slash wave projectiles. |
| **Projectiles** | FermataSpectralSwordNew, FermataSlashWave |
| **Particles** | FermataParticle |
| **Primitives** | FermataTrailRenderer |
| **Shaders** | FermataShaderLoader |

### 9. Symphony's End
| Field | Value |
|-------|-------|
| **Class** | `SymphonysEndItem` |
| **File** | `Content/Fate/ResonantWeapons/SymphonysEnd/SymphonysEndItem.cs` |
| **Weapon Class** | Magic |
| **Description** | Where all melodies find their conclusion. Rapid-fire magic wand unleashing spiraling spectral blades that corkscrew toward cursor. Blades shatter into 4 fragments on contact. 500 damage, 8 useTime. |
| **Particles** | SymphonyParticle |
| **Shaders** | SymphonyShaderLoader |

### 10. Destiny's Crescendo
| Field | Value |
|-------|-------|
| **Class** | `DestinysCrescendoItem` |
| **File** | `Content/Fate/ResonantWeapons/DestinysCrescendo/DestinysCrescendoItem.cs` |
| **Weapon Class** | Summon |
| **Description** | Summon staff calling a Crescendo Deity minion that fires cosmic beams. |
| **Projectiles** | CrescendoDeityMinion, CrescendoCosmicBeam |
| **Particles** | CrescendoParticleHandler |
| **Shaders** | CrescendoShaderLoader |
| **Buffs** | CrescendoDeityBuff |

---

## Dies Irae (12 weapons)

### 1. Wrath's Cleaver
| Field | Value |
|-------|-------|
| **Class** | `WrathsCleaver` |
| **File** | `Content/DiesIrae/Weapons/WrathsCleaver/WrathsCleaver.cs` |
| **Weapon Class** | Melee |
| **Description** | Wrath-fueled melee cleaver with crystallized flame projectiles. |
| **Projectiles** | WrathsCleaverSwing, WrathCrystallizedFlame |
| **Particles** | WrathParticle |
| **Primitives** | WrathTrailRenderer |
| **Shaders** | WrathsCleaverShaderLoader |
| **Buffs/Debuffs** | WrathsCleaverDebuffs |

### 2. Executioner's Verdict
| Field | Value |
|-------|-------|
| **Class** | `ExecutionersVerdict` |
| **File** | `Content/DiesIrae/Weapons/ExecutionersVerdict/ExecutionersVerdict.cs` |
| **Weapon Class** | Melee |
| **Description** | Heavy judgment-themed melee weapon with verdict VFX. |
| **Particles** | VerdictParticle |
| **Primitives** | VerdictVertexType |
| **Shaders** | ExecutionersVerdictShaderLoader |
| **Buffs/Debuffs** | ExecutionersVerdictDebuffs |

### 3. Chain of Judgment
| Field | Value |
|-------|-------|
| **Class** | `ChainOfJudgment` |
| **File** | `Content/DiesIrae/Weapons/ChainOfJudgment/ChainOfJudgment.cs` |
| **Weapon Class** | Melee |
| **Description** | Chain-whip melee weapon with judgment chain projectiles. |
| **Projectiles** | JudgmentChainProjectile |
| **Particles** | ChainParticle |

### 4. Sin Collector
| Field | Value |
|-------|-------|
| **Class** | `SinCollector` |
| **File** | `Content/DiesIrae/Weapons/SinCollector/SinCollector.cs` |
| **Weapon Class** | Ranged |
| **Description** | Ranged weapon firing sin bullet projectiles that collect enemy sins. |
| **Projectiles** | SinBulletProjectile |
| **Particles** | SinParticle |

### 5. Damnation's Cannon
| Field | Value |
|-------|-------|
| **Class** | `DamnationsCannon` |
| **File** | `Content/DiesIrae/Weapons/DamnationsCannon/DamnationsCannon.cs` |
| **Weapon Class** | Ranged |
| **Description** | Heavy ranged cannon lobbing ignited wrath ball projectiles. |
| **Projectiles** | IgnitedWrathBallProjectile |
| **Particles** | DamnationParticle |

### 6. Arbiter's Sentence
| Field | Value |
|-------|-------|
| **Class** | `ArbitersSentence` |
| **File** | `Content/DiesIrae/Weapons/ArbitersSentence/ArbitersSentence.cs` |
| **Weapon Class** | Ranged |
| **Description** | Ranged weapon firing judgment flame projectiles. |
| **Projectiles** | JudgmentFlameProjectile |
| **Particles** | ArbiterParticle |

### 7. Staff of Final Judgment
| Field | Value |
|-------|-------|
| **Class** | `StaffOfFinalJudgment` |
| **File** | `Content/DiesIrae/Weapons/StaffOfFinalJudgment/StaffOfFinalJudgment.cs` |
| **Weapon Class** | Magic |
| **Description** | Magic staff summoning floating ignition projectiles. |
| **Projectiles** | FloatingIgnitionProjectile |
| **Particles** | JudgmentParticle |

### 8. Grimoire of Condemnation
| Field | Value |
|-------|-------|
| **Class** | `GrimoireOfCondemnation` |
| **File** | `Content/DiesIrae/Weapons/GrimoireOfCondemnation/GrimoireOfCondemnation.cs` |
| **Weapon Class** | Magic |
| **Description** | Dark grimoire magic weapon of condemnation. |

### 9. Eclipse of Wrath
| Field | Value |
|-------|-------|
| **Class** | `EclipseOfWrath` |
| **File** | `Content/DiesIrae/Weapons/EclipseOfWrath/EclipseOfWrath.cs` |
| **Weapon Class** | Magic |
| **Description** | Magic weapon launching eclipse orbs that split into wrath shards. |
| **Projectiles** | EclipseOrbProjectile, EclipseWrathShard |
| **Particles** | EclipseParticle |
| **Primitives** | EclipseTrailRenderer |
| **Shaders** | EclipseShaderLoader |

### 10. Death Tolling Bell
| Field | Value |
|-------|-------|
| **Class** | `DeathTollingBell` |
| **File** | `Content/DiesIrae/Weapons/DeathTollingBell/DeathTollingBell.cs` |
| **Weapon Class** | Summon |
| **Description** | Summon staff calling a bell tolling minion that fires toll wave projectiles. |
| **Projectiles** | BellTollingMinion, BellTollWaveProjectile |
| **Particles** | BellParticle |
| **Shaders** | BellShaderLoader |
| **Buffs** | DeathTollingBellBuff |

### 11. Harmony of Judgment
| Field | Value |
|-------|-------|
| **Class** | `HarmonyOfJudgment` |
| **File** | `Content/DiesIrae/Weapons/HarmonyOfJudgment/HarmonyOfJudgment.cs` |
| **Weapon Class** | Summon |
| **Description** | Summon staff deploying a judgment sigil minion. |
| **Projectiles** | JudgmentSigilMinion |
| **Particles** | HarmonyParticle |
| **Buffs** | HarmonyOfJudgmentBuff |

### 12. Wrathful Contract
| Field | Value |
|-------|-------|
| **Class** | `WrathfulContract` |
| **File** | `Content/DiesIrae/Weapons/WrathfulContract/WrathfulContract.cs` |
| **Weapon Class** | Summon |
| **Description** | Summon staff calling a wrath demon minion bound by contract. |
| **Projectiles** | WrathDemonMinion |
| **Particles** | ContractParticle |
| **Buffs** | WrathfulContractBuff |

---

## Ode to Joy (12 weapons)

### 1. Thornbound Reckoning
| Field | Value |
|-------|-------|
| **Class** | `ThornboundReckoning` |
| **File** | `Content/OdeToJoy/Weapons/ThornboundReckoning/ThornboundReckoning.cs` |
| **Weapon Class** | Melee |
| **Description** | Melee sword sending vine wave projectiles on swing. |
| **Projectiles** | VineWaveProjectile |
| **Particles** | ReckoningParticle |

### 2. The Gardener's Fury
| Field | Value |
|-------|-------|
| **Class** | `TheGardenersFury` |
| **File** | `Content/OdeToJoy/Weapons/TheGardenersFury/TheGardenersFury.cs` |
| **Weapon Class** | Melee |
| **Description** | Melee weapon with botanical fury projectiles. |
| **Projectiles** | GardenerFuryProjectile |
| **Particles** | GardenersParticle |

### 3. Rose Thorn Chainsaw
| Field | Value |
|-------|-------|
| **Class** | `RoseThornChainsaw` |
| **File** | `Content/OdeToJoy/Weapons/RoseThornChainsaw/RoseThornChainsaw.cs` |
| **Weapon Class** | Melee (MeleeNoSpeed) |
| **Description** | Chainsaw-style held melee weapon with thorn projectile. |
| **Projectiles** | RoseThornChainsawProjectile |
| **Particles** | ThornParticle |

### 4. Thorn Spray Repeater
| Field | Value |
|-------|-------|
| **Class** | `ThornSprayRepeater` |
| **File** | `Content/OdeToJoy/Weapons/ThornSprayRepeater/ThornSprayRepeater.cs` |
| **Weapon Class** | Ranged |
| **Description** | Rapid-fire ranged weapon spraying thorn projectiles. |
| **Projectiles** | ThornSprayProjectiles |
| **Particles** | ThornSprayParticle |

### 5. The Pollinator
| Field | Value |
|-------|-------|
| **Class** | `ThePollinator` |
| **File** | `Content/OdeToJoy/Weapons/ThePollinator/ThePollinator.cs` |
| **Weapon Class** | Ranged |
| **Description** | Ranged weapon firing pollinator projectiles. |
| **Projectiles** | PollinatorProjectiles |
| **Particles** | PollinatorParticle |

### 6. Petal Storm Cannon
| Field | Value |
|-------|-------|
| **Class** | `PetalStormCannon` |
| **File** | `Content/OdeToJoy/Weapons/PetalStormCannon/PetalStormCannon.cs` |
| **Weapon Class** | Ranged |
| **Description** | Heavy ranged cannon firing petal storm barrages. |
| **Projectiles** | PetalStormProjectiles |
| **Particles** | PetalStormParticle |

### 7. Anthem of Glory
| Field | Value |
|-------|-------|
| **Class** | `AnthemOfGlory` |
| **File** | `Content/OdeToJoy/Weapons/AnthemOfGlory/AnthemOfGlory.cs` |
| **Weapon Class** | Magic |
| **Description** | Magic weapon channeling a glorious anthem. |
| **Projectiles** | AnthemProjectiles |
| **Particles** | AnthemParticle |

### 8. Hymn of the Victorious
| Field | Value |
|-------|-------|
| **Class** | `HymnOfTheVictorious` |
| **File** | `Content/OdeToJoy/Weapons/HymnOfTheVictorious/HymnOfTheVictorious.cs` |
| **Weapon Class** | Magic |
| **Description** | Magic weapon singing a victorious hymn. |
| **Projectiles** | HymnProjectiles |
| **Particles** | HymnParticle |

### 9. Elysian Verdict
| Field | Value |
|-------|-------|
| **Class** | `ElysianVerdict` |
| **File** | `Content/OdeToJoy/Weapons/ElysianVerdict/ElysianVerdict.cs` |
| **Weapon Class** | Magic |
| **Description** | Magic weapon delivering an Elysian judgment. |
| **Projectiles** | ElysianProjectiles |
| **Particles** | ElysianParticle |

### 10. Triumphant Chorus
| Field | Value |
|-------|-------|
| **Class** | `TriumphantChorus` |
| **File** | `Content/OdeToJoy/Weapons/TriumphantChorus/TriumphantChorus.cs` |
| **Weapon Class** | Summon |
| **Description** | Summon weapon calling a chorus of triumphant minions. |
| **Projectiles** | ChorusProjectiles |
| **Particles** | ChorusParticle |
| **Buffs** | TriumphantChorusBuff |

### 11. The Standing Ovation
| Field | Value |
|-------|-------|
| **Class** | `TheStandingOvation` |
| **File** | `Content/OdeToJoy/Weapons/TheStandingOvation/TheStandingOvation.cs` |
| **Weapon Class** | Summon |
| **Description** | Summon weapon that receives a standing ovation — crowd-themed minions. |
| **Projectiles** | OvationProjectiles |
| **Particles** | OvationParticle |
| **Buffs** | StandingOvationBuff |

### 12. Fountain of Joyous Harmony
| Field | Value |
|-------|-------|
| **Class** | `FountainOfJoyousHarmony` |
| **File** | `Content/OdeToJoy/Weapons/FountainOfJoyousHarmony/FountainOfJoyousHarmony.cs` |
| **Weapon Class** | Summon |
| **Description** | Summon weapon creating a fountain of joyous harmony. |
| **Projectiles** | FountainProjectiles |
| **Particles** | FountainParticle |
| **Buffs** | JoyousFountainBuff |

---

## Nachtmusik (11 weapons)

### 1. Nocturnal Executioner
| Field | Value |
|-------|-------|
| **Class** | `NocturnalExecutioner` (extends `MeleeSwingItemBase`) |
| **File** | `Content/Nachtmusik/ResonantWeapons/NachtmusikMeleeWeapons.cs` |
| **Weapon Class** | Melee (MeleeNoSpeed) |
| **Description** | Heavy cosmic greatsword, 1850 damage. Devastating 3-phase combo with cosmic trails. Execution Charge system (0-100): right-click at 50+ charge fires 5-blade fan (2.5x damage) with screen shake. Orbiting purple/gold particles at high charge. |
| **Projectiles** | NocturnalBladeProjectile |
| **VFX** | NocturnalExecutionerVFX (HoldItemVFX, FinisherVFX) |
| **Buffs/Debuffs** | NocturnalExecutionerDebuffs |

### 2. Midnight's Crescendo
| Field | Value |
|-------|-------|
| **Class** | `MidnightsCrescendo` (extends `MeleeSwingItemBase`) |
| **File** | `Content/Nachtmusik/ResonantWeapons/NachtmusikMeleeWeapons.cs` |
| **Weapon Class** | Melee (MeleeNoSpeed) |
| **Description** | Rapid 3-phase combo that builds momentum. Crescendo Stacks system (max 15): each hit adds a stack (+12% damage, +2% crit per stack). Stacks decay after 1.5s. At 8+ stacks, swings release crescendo waves. Inflicts Celestial Harmony. |
| **VFX** | MidnightsCrescendoVFX |

### 3. Twilight Severance
| Field | Value |
|-------|-------|
| **Class** | `TwilightSeverance` (extends `MeleeSwingItemBase`) |
| **File** | `Content/Nachtmusik/ResonantWeapons/NachtmusikMeleeWeapons.cs` |
| **Weapon Class** | Melee (MeleeNoSpeed) |
| **Description** | Ultra-fast katana, 1450 damage, 25% crit. Ultra-fast 3-phase combo. Twilight Charge system (0-100): builds on swing (+5), decays idle. Every 3rd slash fires perpendicular blade waves. Right-click at full charge: Dimension Sever (3x damage fan). Inflicts Celestial Harmony. |
| **VFX** | TwilightSeveranceVFX |

### 4. Constellation Piercer
| Field | Value |
|-------|-------|
| **Class** | `ConstellationPiercer` |
| **File** | `Content/Nachtmusik/ResonantWeapons/NachtmusikRangedWeapons.cs` |
| **Weapon Class** | Ranged |
| **Description** | Nachtmusik ranged weapon themed around piercing constellations. |
| **VFX** | ConstellationPiercerVFX |

### 5. Nebula's Whisper
| Field | Value |
|-------|-------|
| **Class** | `NebulasWhisper` |
| **File** | `Content/Nachtmusik/ResonantWeapons/NachtmusikRangedWeapons.cs` |
| **Weapon Class** | Ranged |
| **Description** | Nachtmusik ranged weapon with nebula-whisper projectiles. |
| **VFX** | NebulasWhisperVFX |

### 6. Serenade of Distant Stars
| Field | Value |
|-------|-------|
| **Class** | `SerenadeOfDistantStars` |
| **File** | `Content/Nachtmusik/ResonantWeapons/NachtmusikRangedWeapons.cs` |
| **Weapon Class** | Ranged |
| **Description** | Nachtmusik ranged weapon serenading distant stars. |
| **VFX** | SerenadeOfDistantStarsVFX |

### 7. Starweaver's Grimoire
| Field | Value |
|-------|-------|
| **Class** | `StarweaversGrimoire` |
| **File** | `Content/Nachtmusik/ResonantWeapons/NachtmusikMagicWeapons.cs` |
| **Weapon Class** | Magic |
| **Description** | Nachtmusik magic tome weaving starlight spells. |
| **VFX** | StarweaversGrimoireVFX |

### 8. Requiem of the Cosmos
| Field | Value |
|-------|-------|
| **Class** | `RequiemOfTheCosmos` |
| **File** | `Content/Nachtmusik/ResonantWeapons/NachtmusikMagicWeapons.cs` |
| **Weapon Class** | Magic |
| **Description** | Nachtmusik cosmic requiem magic weapon. |
| **VFX** | RequiemOfTheCosmosVFX |

### 9. Celestial Chorus Baton
| Field | Value |
|-------|-------|
| **Class** | `CelestialChorusBaton` |
| **File** | `Content/Nachtmusik/ResonantWeapons/NachtmusikSummonWeapons.cs` |
| **Weapon Class** | Summon |
| **Description** | Summon baton conducting a celestial chorus. |
| **VFX** | CelestialChorusBatonVFX |

### 10. Galactic Overture
| Field | Value |
|-------|-------|
| **Class** | `GalacticOverture` |
| **File** | `Content/Nachtmusik/ResonantWeapons/NachtmusikSummonWeapons.cs` |
| **Weapon Class** | Summon |
| **Description** | Summon weapon performing a galactic overture. |
| **VFX** | GalacticOvertureVFX |

### 11. Conductor of Constellations
| Field | Value |
|-------|-------|
| **Class** | `ConductorOfConstellations` |
| **File** | `Content/Nachtmusik/ResonantWeapons/NachtmusikSummonWeapons.cs` |
| **Weapon Class** | Summon |
| **Description** | Summon weapon conducting an orchestra of constellations. |
| **VFX** | ConductorOfConstellationsVFX |

---

## Clair de Lune (12 weapons)

*All weapons defined in multi-class files with co-located projectile classes. Time/clockwork theme.*

### Melee (3)

#### 1. Chronologicality
| Field | Value |
|-------|-------|
| **Class** | `Chronologicality` |
| **File** | `Content/ClairDeLune/Weapons/Melee/ClairDeLuneMeleeWeapons.cs` (line 30) |
| **Weapon Class** | Melee (MeleeNoSpeed) |
| **Description** | Rapidly tears through enemies with temporal gears. |
| **Projectiles** | ChronologicalityProjectile, TemporalRiftProjectile |
| **VFX** | ClairDeLuneMeleeVFX |

#### 2. Temporal Piercer
| Field | Value |
|-------|-------|
| **Class** | `TemporalPiercer` |
| **File** | `Content/ClairDeLune/Weapons/Melee/ClairDeLuneMeleeWeapons.cs` (line 471) |
| **Weapon Class** | Melee |
| **Description** | Thrusting attacks pierce through time itself. |
| **Projectiles** | TemporalPiercerProjectile, TimeFractureExplosion |
| **VFX** | ClairDeLuneMeleeVFX |

#### 3. Clockwork Harmony
| Field | Value |
|-------|-------|
| **Class** | `ClockworkHarmony` |
| **File** | `Content/ClairDeLune/Weapons/Melee/ClairDeLuneMeleeWeapons.cs` (line 751) |
| **Weapon Class** | Melee |
| **Description** | On swing releases cascading gear waves. |
| **Projectiles** | GearWaveProjectile, SynchronizedGearExplosion |
| **VFX** | ClairDeLuneMeleeVFX |

### Ranged (3)

#### 4. Starfall Whisper
| Field | Value |
|-------|-------|
| **Class** | `StarfallWhisper` |
| **File** | `Content/ClairDeLune/Weapons/Ranged/ClairDeLuneRangedWeapons.cs` (line 29) |
| **Weapon Class** | Ranged |
| **Description** | Converts bullets into time-piercing crystal bolts. |
| **Projectiles** | StarfallBoltProjectile, StarfallRiftProjectile, StarfallCrystalProjectile |
| **VFX** | ClairDeLuneRangedVFX |

#### 5. Midnight Mechanism
| Field | Value |
|-------|-------|
| **Class** | `MidnightMechanism` |
| **File** | `Content/ClairDeLune/Weapons/Ranged/ClairDeLuneRangedWeapons.cs` (line 415) |
| **Weapon Class** | Ranged |
| **Description** | Clockwork gatling with spin-up mechanic. |
| **Projectiles** | MechanismBoltProjectile, SynchronizedGearBoltProjectile |
| **VFX** | ClairDeLuneRangedVFX |

#### 6. Cog and Hammer
| Field | Value |
|-------|-------|
| **Class** | `CogAndHammer` |
| **File** | `Content/ClairDeLune/Weapons/Ranged/ClairDeLuneRangedWeapons.cs` (line 713) |
| **Weapon Class** | Ranged |
| **Description** | Fires massive clockwork bombs. |
| **Projectiles** | ClockworkBombProjectile, GearShrapnelProjectile |
| **VFX** | ClairDeLuneRangedVFX |

### Magic (3)

#### 7. Clockwork Grimoire
| Field | Value |
|-------|-------|
| **Class** | `ClockworkGrimoire` |
| **File** | `Content/ClairDeLune/Weapons/Magic/ClairDeLuneMagicWeapons.cs` (line 31) |
| **Weapon Class** | Magic |
| **Description** | A tome of temporal magic — cycles through 4 spell modes. |
| **Projectiles** | GrimoireLightningProjectile, GrimoireCrystalProjectile, GrimoireGearProjectile, GrimoireTimeFractureProjectile |
| **VFX** | ClairDeLuneMagicVFX |

#### 8. Orrery of Dreams
| Field | Value |
|-------|-------|
| **Class** | `OrreryOfDreams` |
| **File** | `Content/ClairDeLune/Weapons/Magic/ClairDeLuneMagicWeapons.cs` (line 659) |
| **Weapon Class** | Magic |
| **Description** | Summons a clockwork orrery with orbiting celestial spheres. |
| **Projectiles** | OrreryControllerProjectile, OrreryOrbitProjectile, OrreryBeamProjectile, CosmicLaserProjectile |
| **VFX** | ClairDeLuneMagicVFX |

#### 9. Requiem of Time
| Field | Value |
|-------|-------|
| **Class** | `RequiemOfTime` |
| **File** | `Content/ClairDeLune/Weapons/Magic/ClairDeLuneMagicWeapons.cs` (line 1043) |
| **Weapon Class** | Magic |
| **Description** | Time-themed requiem magic weapon. |
| **VFX** | ClairDeLuneMagicVFX |

### Summon (3)

#### 10. Lunar Phylactery
| Field | Value |
|-------|-------|
| **Class** | `LunarPhylactery` |
| **File** | `Content/ClairDeLune/Weapons/Summon/ClairDeLuneSummonWeapons.cs` (line 31) |
| **Weapon Class** | Summon |
| **Description** | Summons a lunar phylactery minion that fires beams. |
| **Projectiles** | LunarPhylacteryMinionProjectile, PhylacteryBeamProjectile |
| **Buffs** | LunarPhylacteryBuff |
| **VFX** | ClairDeLuneSummonVFX |

#### 11. Gear-Driven Arbiter
| Field | Value |
|-------|-------|
| **Class** | `GearDrivenArbiter` |
| **File** | `Content/ClairDeLune/Weapons/Summon/ClairDeLuneSummonWeapons.cs` (line 469) |
| **Weapon Class** | Summon |
| **Description** | Summons a gear-driven arbiter minion that flings gear projectiles. |
| **Projectiles** | GearDrivenArbiterMinionProjectile, ArbiterGearProjectile |
| **Buffs** | GearDrivenArbiterBuff |
| **Debuffs** | TemporalJudgmentDebuff (via TemporalJudgmentGlobalNPC) |
| **VFX** | ClairDeLuneSummonVFX |

#### 12. Automaton's Tuning Fork
| Field | Value |
|-------|-------|
| **Class** | `AutomatonsTuningFork` |
| **File** | `Content/ClairDeLune/Weapons/Summon/ClairDeLuneSummonWeapons.cs` (line 910) |
| **Weapon Class** | Summon |
| **Description** | Summons an automaton minion attuned by tuning fork resonance. |
| **Projectiles** | AutomatonsTuningForkMinionProjectile |
| **Buffs** | AutomatonsTuningForkBuff |
| **VFX** | ClairDeLuneSummonVFX |

---

## Seasons — Vivaldi's Arsenal (4 weapons)

### 1. Four Seasons Blade
| Field | Value |
|-------|-------|
| **Class** | `FourSeasonsBlade` (extends `MeleeSwingItemBase`) |
| **File** | `Content/Seasons/Weapons/FourSeasonsBlade/FourSeasonsBlade.cs` |
| **Weapon Class** | Melee (MeleeNoSpeed) |
| **Description** | Cycles through Spring→Summer→Autumn→Winter combo phases. Each season applies unique debuffs, healing, and VFX. Every 4th complete cycle triggers Crescendo burst. |
| **Projectiles** | FourSeasonsBladeSwing |

### 2. Seasonal Bow
| Field | Value |
|-------|-------|
| **Class** | `SeasonalBow` |
| **File** | `Content/Seasons/Weapons/SeasonalBow/SeasonalBow.cs` |
| **Weapon Class** | Ranged |
| **Description** | Bow that cycles through seasonal arrow types. |

### 3. Concerto of Seasons
| Field | Value |
|-------|-------|
| **Class** | `ConcertoOfSeasons` |
| **File** | `Content/Seasons/Weapons/ConcertoOfSeasons/ConcertoOfSeasons.cs` |
| **Weapon Class** | Magic |
| **Description** | Magic concerto channeling all four seasons. |

### 4. Vivaldi's Baton
| Field | Value |
|-------|-------|
| **Class** | `VivaldisBaton` |
| **File** | `Content/Seasons/Weapons/VivaldisBaton/VivaldisBaton.cs` |
| **Weapon Class** | Summon |
| **Description** | Summon baton conducting the seasonal orchestra. |

---

## Spring (4 weapons)

### 1. Blossom's Edge
| Field | Value |
|-------|-------|
| **Class** | `BlossomsEdge` |
| **File** | `Content/Spring/Weapons/BlossomsEdge/BlossomsEdge.cs` |
| **Weapon Class** | Melee (MeleeNoSpeed) |
| **Description** | Spring melee sword with blossom swing arcs. |
| **Projectiles** | BlossomsEdgeSwing |

### 2. Petal Storm Bow
| Field | Value |
|-------|-------|
| **Class** | `PetalStormBow` |
| **File** | `Content/Spring/Weapons/PetalStormBow/PetalStormBow.cs` |
| **Weapon Class** | Ranged |
| **Description** | Bow firing petal storm arrows. |

### 3. Vernal Scepter
| Field | Value |
|-------|-------|
| **Class** | `VernalScepter` |
| **File** | `Content/Spring/Weapons/VernalScepter/VernalScepter.cs` |
| **Weapon Class** | Magic |
| **Description** | Spring magic scepter channeling vernal energy. |

### 4. Primavera's Bloom
| Field | Value |
|-------|-------|
| **Class** | `PrimaverasBloom` |
| **File** | `Content/Spring/Weapons/PrimaverasBloom/PrimaverasBloom.cs` |
| **Weapon Class** | Summon |
| **Description** | Summon staff calling a spring bloom minion. |

---

## Summer (4 weapons)

### 1. Zenith Cleaver
| Field | Value |
|-------|-------|
| **Class** | `ZenithCleaver` |
| **File** | `Content/Summer/Weapons/ZenithCleaver/ZenithCleaver.cs` |
| **Weapon Class** | Melee (MeleeNoSpeed) |
| **Description** | Summer melee cleaver with solar heat effects. |
| **Projectiles** | ZenithCleaverSwing |

### 2. Solar Scorcher
| Field | Value |
|-------|-------|
| **Class** | `SolarScorcher` |
| **File** | `Content/Summer/Weapons/SolarScorcher/SolarScorcher.cs` |
| **Weapon Class** | Ranged |
| **Description** | Ranged weapon scorching enemies with solar fire. |

### 3. Solstice Tome
| Field | Value |
|-------|-------|
| **Class** | `SolsticeTome` |
| **File** | `Content/Summer/Weapons/SolsticeTome/SolsticeTome.cs` |
| **Weapon Class** | Magic |
| **Description** | Magic tome channeling solstice energy. |

### 4. Solar Crest
| Field | Value |
|-------|-------|
| **Class** | `SolarCrest` |
| **File** | `Content/Summer/Weapons/SolarCrest/SolarCrest.cs` |
| **Weapon Class** | Summon |
| **Description** | Summon crest calling a solar minion. |

---

## Autumn (4 weapons)

### 1. Harvest Reaper
| Field | Value |
|-------|-------|
| **Class** | `HarvestReaper` |
| **File** | `Content/Autumn/Weapons/HarvestReaper/HarvestReaper.cs` |
| **Weapon Class** | Melee (MeleeNoSpeed) |
| **Description** | Autumn scythe with harvest reaping swings. |
| **Projectiles** | HarvestReaperSwing |

### 2. Twilight Arbalest
| Field | Value |
|-------|-------|
| **Class** | `TwilightArbalest` |
| **File** | `Content/Autumn/Weapons/TwilightArbalest/TwilightArbalest.cs` |
| **Weapon Class** | Ranged |
| **Description** | Heavy crossbow firing twilight bolts. |

### 3. Withering Grimoire
| Field | Value |
|-------|-------|
| **Class** | `WitheringGrimoire` |
| **File** | `Content/Autumn/Weapons/WitheringGrimoire/WitheringGrimoire.cs` |
| **Weapon Class** | Magic |
| **Description** | Magic grimoire of autumn withering spells. |

### 4. Decay Bell
| Field | Value |
|-------|-------|
| **Class** | `DecayBell` |
| **File** | `Content/Autumn/Weapons/DecayBell/DecayBell.cs` |
| **Weapon Class** | Summon |
| **Description** | Summon bell calling a decay-themed minion. |

---

## Winter (4 weapons)

### 1. Glacial Executioner
| Field | Value |
|-------|-------|
| **Class** | `GlacialExecutioner` |
| **File** | `Content/Winter/Weapons/GlacialExecutioner/GlacialExecutioner.cs` |
| **Weapon Class** | Melee (MeleeNoSpeed) |
| **Description** | Winter melee greatsword with glacial execution swings. |
| **Projectiles** | GlacialExecutionerSwing |

### 2. Frostbite Repeater
| Field | Value |
|-------|-------|
| **Class** | `FrostbiteRepeater` |
| **File** | `Content/Winter/Weapons/FrostbiteRepeater/FrostbiteRepeater.cs` |
| **Weapon Class** | Ranged |
| **Description** | Ranged repeater with frostbite projectiles. |

### 3. Permafrost Codex
| Field | Value |
|-------|-------|
| **Class** | `PermafrostCodex` |
| **File** | `Content/Winter/Weapons/PermafrostCodex/PermafrostCodex.cs` |
| **Weapon Class** | Magic |
| **Description** | Magic codex of permafrost spells. |

### 4. Frozen Heart
| Field | Value |
|-------|-------|
| **Class** | `FrozenHeart` |
| **File** | `Content/Winter/Weapons/FrozenHeart/FrozenHeart.cs` |
| **Weapon Class** | Summon |
| **Description** | Summon staff calling a frozen heart minion. |

---

## Sandbox (Test Weapons — 2 weapons)

### 1. Exoblade (Sandbox)
| Field | Value |
|-------|-------|
| **Class** | `Exoblade` |
| **File** | `Content/SandboxExoblade/Exoblade.cs` |
| **Weapon Class** | Melee (MeleeNoSpeed) |
| **Description** | Port/adaptation of Calamity's Exoblade for VFX reference. Full self-contained weapon system with primitives, particles, shaders, dusts, buffs, and utilities. |
| **Full Subsystem** | Projectiles/, Particles/, Primitives/, Shaders/, Dusts/, Buffs/, Utilities/ |

### 2. Sandbox Last Prism
| Field | Value |
|-------|-------|
| **Class** | `SandboxLastPrism` |
| **File** | `Content/SandboxLastPrism/SandboxLastPrism.cs` |
| **Weapon Class** | Magic |
| **Description** | Reference implementation of Last Prism with full VFX pipeline. Canonical example for the SandboxLastPrism folder pattern (Dusts/, Systems/, self-contained architecture). |
| **Full Subsystem** | Dusts/Textures/, Systems/ (flash, pixelation, screen-shake) |

---

## Foundation Weapons (Test/Debug — 15 weapons)

*All located in `Content/FoundationWeapons/`. Test weapons for individual VFX techniques.*

| # | Class | File | Weapon Class | Description |
|---|-------|------|-------------|-------------|
| 1 | `AttackAnimationFoundation` | `Content/FoundationWeapons/AttackAnimationFoundation.cs` | Melee | Test weapon for attack animation system |
| 2 | `AttackFoundation` | `Content/FoundationWeapons/AttackFoundation.cs` | Melee (multi-mode) | Multi-mode test weapon that switches between all damage types |
| 3 | `SwordSmearFoundation` | `Content/FoundationWeapons/SwordSmearFoundation.cs` | Melee | Test weapon for sword smear VFX |
| 4 | `XSlashFoundation` | `Content/FoundationWeapons/XSlashFoundation.cs` | Melee | Test weapon for X-slash effect |
| 5 | `SparkleProjectileFoundation` | `Content/FoundationWeapons/SparkleProjectileFoundation.cs` | Melee | Test weapon for sparkle projectile particles |
| 6 | `SmokeFoundation` | `Content/FoundationWeapons/SmokeFoundation.cs` | Melee | Test weapon for smoke VFX |
| 7 | `ExplosionParticlesFoundation` | `Content/FoundationWeapons/ExplosionParticlesFoundation.cs` | Melee | Test weapon for explosion particle systems |
| 8 | `MaskFoundation` | `Content/FoundationWeapons/MaskFoundation.cs` | Melee | Test weapon for mask/alpha-mask VFX |
| 9 | `ThinSlashFoundation` | `Content/FoundationWeapons/ThinSlashFoundation.cs` | Magic | Test weapon for thin slash VFX |
| 10 | `RibbonFoundation` | `Content/FoundationWeapons/RibbonFoundation.cs` | Magic | Test weapon for ribbon trail VFX |
| 11 | `InfernalBeamFoundation` | `Content/FoundationWeapons/InfernalBeamFoundation.cs` | Magic | Test weapon for infernal beam rendering |
| 12 | `LaserFoundation` | `Content/FoundationWeapons/LaserFoundation.cs` | Magic | Test weapon for laser beam VFX |
| 13 | `MagicOrbFoundation` | `Content/FoundationWeapons/MagicOrbFoundation.cs` | Magic | Test weapon for magic orb projectiles |
| 14 | `ImpactFoundation` | `Content/FoundationWeapons/ImpactFoundation.cs` | Magic | Test weapon for impact VFX |
| 15 | `ThinLaserFoundation` | `Content/FoundationWeapons/ThinLaserFoundation.cs` | Melee | Test weapon for thin laser VFX |

---

## Summary Statistics

| Theme | Melee | Ranged | Magic | Summon | Special | Total |
|-------|-------|--------|-------|--------|---------|-------|
| Moonlight Sonata | 2 | 1 | 1 | 1 | — | 5 |
| Eroica | 2 | 2 | 2 | 1 | — | 7 |
| La Campanella | 2 | 3 | 1 | 1 | — | 7 |
| Enigma Variations | 2 | 2 | 3 | 1 | — | 8 |
| Swan Lake | 1 | 2 | 2 | 1 | 1 | 7 |
| Fate | 5 | 2 | 2 | 1 | — | 10 |
| Dies Irae | 3 | 3 | 3 | 3 | — | 12 |
| Ode to Joy | 3 | 3 | 3 | 3 | — | 12 |
| Nachtmusik | 3 | 3 | 2 | 3 | — | 11 |
| Clair de Lune | 3 | 3 | 3 | 3 | — | 12 |
| Seasons | 1 | 1 | 1 | 1 | — | 4 |
| Spring | 1 | 1 | 1 | 1 | — | 4 |
| Summer | 1 | 1 | 1 | 1 | — | 4 |
| Autumn | 1 | 1 | 1 | 1 | — | 4 |
| Winter | 1 | 1 | 1 | 1 | — | 4 |
| Sandbox | 1 | — | 1 | — | — | 2 |
| Foundation | 8 | — | 7 | — | — | 15 |
| **TOTAL** | **40** | **29** | **35** | **23** | **1** | **128** |

---

## Documentation Folder Contents

```
Documentation/
├── IMPLEMENTATION_STATUS.md          — Phase completion tracker (1-10)
├── Implementation_Phases.md          — Detailed phase breakdown
├── Mod_Progression.txt               — Game progression design
├── Enhancements.md                   — Planned enhancements
├── Boss_Attack_Brainstorming.md      — Boss attack design notes
├── Enigma_Fate_Item_Names.txt        — Naming reference for Enigma/Fate items
├── Assets_Needed_Buffs_Debuffs_Summons.txt — Asset tracking
├── WEAPON_SURVEY.md                  — THIS FILE
├── Reference/
│   ├── DebugSWING_Reference.cs       — Debug swing weapon reference code
│   └── DebugSWINGProj_Reference.cs   — Debug swing projectile reference code
├── AI Prompts/
│   ├── SUNO_AI_Prompts.txt           — SUNO music AI prompts
│   └── SUNO_Anthem_of_Time_Part2_Finale.md — SUNO finale prompt
└── SUNO_Music_Prompts.md             — Additional SUNO music prompts
```

### Implementation Status (from IMPLEMENTATION_STATUS.md)

| Phase | Name | Status |
|-------|------|--------|
| 1 | Core VFX Engine | ✅ Complete |
| 2 | Moonlight Sonata Content | ✅ Complete |
| 3 | Eroica Content | ✅ Complete |
| 4 | La Campanella Content | ✅ Complete |
| 5 | Enigma Variations Content | ✅ Complete |
| 6 | Swan Lake Content | ✅ Complete |
| 7 | Fate Content | ✅ Complete |
| 8 | Vivaldi's Arsenal (Seasons) | ⏳ Pending |
| 9 | Additional Themes (Dies Irae, Ode to Joy, Nachtmusik, Clair de Lune) | ✅ Complete |
| 10 | VFX Polish Pass | 🔄 In Progress |
