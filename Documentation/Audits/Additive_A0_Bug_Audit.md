# Additive Blend State + A=0 Color Bug Audit

## Bug Description

**Pattern:** Code draws with `A = 0` colors while the SpriteBatch is in `BlendState.Additive`.

**Why this is a bug:** Under `BlendState.Additive`, the source blend factor is `Blend.SourceAlpha`. The GPU computes:
```
finalRGB = srcRGB × srcAlpha + destRGB × 1
```
When `srcAlpha = 0`, `srcRGB × 0 = 0` — **the draw is completely invisible**.

**The intended "A=0 additive trick"** only works under `BlendState.AlphaBlend` (premultiplied alpha), where the source blend factor is `Blend.One`:
```
finalRGB = srcRGB × 1 + destRGB × (1 - srcAlpha)
```
Here the RGB is added directly regardless of alpha, while `1 - srcAlpha = 1` means nothing is subtracted from the destination — producing a pure additive glow.

**Fix:** Either:
1. Switch the SpriteBatch to `BlendState.AlphaBlend` (and keep `A = 0`) — the premultiplied "additive trick"
2. Keep `BlendState.Additive` and remove `A = 0` (set alpha to the desired opacity, e.g., `color * 0.5f` where color has full alpha)

---

## Summary Statistics

| Category | Files | Bug Instances |
|----------|-------|---------------|
| Direct `with { A = 0 }` / `.A = 0` in Additive batches | **112** | **537** |
| `Palette.Additive()` / `Utils.Additive()` helpers in Additive batches | **24** | **189** |
| **Combined (some overlap)** | **~130** | **~726** |

---

## Bug Patterns Found

### Pattern 1: `with { A = 0 }` inside explicit `BlendState.Additive`
```csharp
sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, ...);
sb.Draw(tex, pos, null, SomeColor with { A = 0 } * 0.5f, ...); // INVISIBLE
sb.End();
```

### Pattern 2: `.A = 0` assignment inside Additive batch
```csharp
sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, ...);
Color c = SomeColor;
c.A = 0; // Makes it invisible
sb.Draw(tex, pos, null, c * 0.5f, ...); // INVISIBLE
sb.End();
```

### Pattern 3: Palette Additive helpers inside Additive batch
```csharp
SomeUtils.BeginAdditive(sb); // Opens BlendState.Additive
sb.Draw(tex, pos, null, CodaUtils.Additive(color, 0.5f), ...); // A=0 → INVISIBLE
sb.End();
```
Affected helpers (all produce `A = 0`):
- `CodaUtils.Additive(c, opacity)` → `new Color(c.R, c.G, c.B, 0) * opacity`
- `SymphonyUtils.Additive(c, opacity)` → `new Color(R*op, G*op, B*op, 0)`
- `RequiemUtils.Additive(c, opacity)` — same pattern
- `SwanLakePalette.Additive(c)` → `c with { A = 0 }`
- `NachtmusikPalette.Additive(c)` / `MoonlightSonataPalette.Additive(c)` / `LaCampanellaPalette.Additive(c)` / `FatePalette.Additive(c)` / `EroicaPalette.Additive(c)` / `EnigmaPalette.Additive(c)` — all `c with { A = 0 }`
- `DualFatedChimeUtils.Additive` / `IgnitionOfTheBellUtils.Additive` / `FangOfTheInfiniteBellUtils.Additive` / `InfernalChimesCallingUtils.Additive` / `PearlescentUtils.Additive` / `BlackSwanUtils.Additive` / `CrescendoUtils.Additive` — all produce A=0

---

## Affected Files by Theme

### Autumn (1 file, 5 instances)

| File | Lines | Additive Source | Bug Count |
|------|-------|-----------------|-----------|
| `Autumn\Projectiles\TwilightBolt.cs` | L254, 256, 258, 260, 268 | `BlendState.Additive` @L249 | 5 |

### Clair de Lune (30 files, ~155 instances)

| File | Additive Source | Bug Count |
|------|-----------------|-----------|
| `ClairDeLune\Weapons\AutomatonsTuningFork\Projectiles\AutomatonMinionProjectile.cs` | `BlendState.Additive` @L334 | 8 |
| `ClairDeLune\Weapons\AutomatonsTuningFork\Projectiles\ConductorFinalNoteProjectile.cs` | `BlendState.Additive` @L271 | 4 |
| `ClairDeLune\Weapons\AutomatonsTuningFork\Projectiles\FrequencyZoneProjectile.cs` | `BlendState.Additive` @L288 | 4 |
| `ClairDeLune\Weapons\AutomatonsTuningFork\Projectiles\PerfectResonanceProjectile.cs` | `BlendState.Additive` @L224 | 4 |
| `ClairDeLune\Weapons\Chronologicality\Projectiles\ChronologicalitySwing.cs` | `BlendState.Additive` @L393/439/468/517 | 14 |
| `ClairDeLune\Weapons\Chronologicality\Projectiles\ClockworkOverflowProjectile.cs` | `BlendState.Additive` @L358 | 8 |
| `ClairDeLune\Weapons\Chronologicality\Projectiles\TemporalEchoProjectile.cs` | `BlendState.Additive` @L201 | 4 |
| `ClairDeLune\Weapons\Chronologicality\Projectiles\TimeSlowFieldProjectile.cs` | `BlendState.Additive` @L180 | 3 |
| `ClairDeLune\Weapons\ClockworkGrimoire\Projectiles\HourBeamProjectile.cs` | `BlendState.Additive` @L233 | 10 |
| `ClairDeLune\Weapons\ClockworkGrimoire\Projectiles\MinuteOrbProjectile.cs` | `BlendState.Additive` @L235 | 5 |
| `ClairDeLune\Weapons\ClockworkGrimoire\Projectiles\PendulumZoneProjectile.cs` | `BlendState.Additive` @L206 | 6 |
| `ClairDeLune\Weapons\ClockworkGrimoire\Projectiles\SecondBoltProjectile.cs` | `BlendState.Additive` @L114 | 3 |
| `ClairDeLune\Weapons\ClockworkHarmony\Projectiles\DriveGearProjectile.cs` | `BlendState.Additive` @L172 | 5 |
| `ClairDeLune\Weapons\ClockworkHarmony\Projectiles\GearMeshZoneProjectile.cs` | `BlendState.Additive` @L218 | 6 |
| `ClairDeLune\Weapons\ClockworkHarmony\Projectiles\MediumGearProjectile.cs` | `BlendState.Additive` @L189 | 4 |
| `ClairDeLune\Weapons\ClockworkHarmony\Projectiles\SmallGearProjectile.cs` | `BlendState.Additive` @L225 | 5 |
| `ClairDeLune\Weapons\CogAndHammer\Projectiles\ClockworkBombProjectile.cs` | `BlendState.Additive` @L287 | 4 |
| `ClairDeLune\Weapons\CogAndHammer\Projectiles\GearShrapnelProjectile.cs` | `BlendState.Additive` @L119 | 3 |
| `ClairDeLune\Weapons\CogAndHammer\Projectiles\MasterMechanismBombProjectile.cs` | `BlendState.Additive` @L253 | 6 |
| `ClairDeLune\Weapons\CogAndHammer\Projectiles\StickyBombProjectile.cs` | `BlendState.Additive` @L326 | 5 |
| `ClairDeLune\Weapons\GearDrivenArbiter\Projectiles\ArbiterGearProjectile.cs` | `BlendState.Additive` @L220 | 5 |
| `ClairDeLune\Weapons\GearDrivenArbiter\Projectiles\ArbiterMinionProjectile.cs` | `BlendState.Additive` @L267 | 4 |
| `ClairDeLune\Weapons\GearDrivenArbiter\Projectiles\ArbiterVerdictProjectile.cs` | `BlendState.Additive` @L247 | 7 |
| `ClairDeLune\Weapons\LunarPhylactery\Projectiles\MoonlightBeamProjectile.cs` | `BlendState.Additive` @L251/269 | 4 |
| `ClairDeLune\Weapons\LunarPhylactery\Projectiles\MoonlightSentinelProjectile.cs` | `BlendState.Additive` @L275 | 4 |
| `ClairDeLune\Weapons\MidnightMechanism\Projectiles\MechanismBulletProjectile.cs` | `BlendState.Additive` @L220 | 4 |
| `ClairDeLune\Weapons\MidnightMechanism\Projectiles\MechanismEjectGearProjectile.cs` | `BlendState.Additive` @L186 | 3 |
| `ClairDeLune\Weapons\MidnightMechanism\Projectiles\MidnightStrikeShotProjectile.cs` | `BlendState.Additive` @L274 | 5 |
| `ClairDeLune\Weapons\OrreryOfDreams\Projectiles\DreamAlignmentProjectile.cs` | `BlendState.Additive` @L187/281 | 4 |
| `ClairDeLune\Weapons\OrreryOfDreams\Projectiles\DreamSphereProjectile.cs` | `BlendState.Additive` @L226/276 | 5 |
| `ClairDeLune\Weapons\OrreryOfDreams\Projectiles\InnerSphereBoltProjectile.cs` | `BlendState.Additive` @L173 | 4 |
| `ClairDeLune\Weapons\OrreryOfDreams\Projectiles\MiddleSphereOrbProjectile.cs` | `BlendState.Additive` @L222 | 3 |
| `ClairDeLune\Weapons\OrreryOfDreams\Projectiles\OuterSphereBombProjectile.cs` | `BlendState.Additive` @L215/288 | 5 |
| `ClairDeLune\Weapons\RequiemOfTime\Projectiles\ForwardFieldProjectile.cs` | `BlendState.Additive` @L230 | 4 |
| `ClairDeLune\Weapons\RequiemOfTime\Projectiles\ReverseFieldProjectile.cs` | `BlendState.Additive` @L212 | 5 |
| `ClairDeLune\Weapons\RequiemOfTime\Projectiles\TemporalParadoxProjectile.cs` | `BlendState.Additive` @L296 | 7 |
| `ClairDeLune\Weapons\StarfallWhisper\Projectiles\TemporalArrowProjectile.cs` | `BlendState.Additive` @L198/253/332 | 9 |
| `ClairDeLune\Weapons\StarfallWhisper\Projectiles\TemporalFractureProjectile.cs` | `BlendState.Additive` @L262 | 5 |
| `ClairDeLune\Weapons\TemporalPiercer\Projectiles\FrozenMomentProjectile.cs` | `BlendState.Additive` @L225 | 6 |
| `ClairDeLune\Weapons\TemporalPiercer\Projectiles\TemporalThrustProjectile.cs` | `BlendState.Additive` @L276 | 4 |
| `ClairDeLune\Weapons\TemporalPiercer\Projectiles\TimePierceBoomerangProjectile.cs` | `BlendState.Additive` @L258 | 3 |

### Dies Irae (4 files, 13 instances)

| File | Additive Source | Bug Count |
|------|-----------------|-----------|
| `DiesIrae\Weapons\ExecutionersVerdict\Projectiles\ExecutionersVerdictSwing.cs` | `BlendState.Additive` @L254 | 1 |
| `DiesIrae\Weapons\ExecutionersVerdict\Projectiles\VerdictBolt.cs` | `BlendState.Additive` @L144/237 | 5 |
| `DiesIrae\Weapons\WrathsCleaver\Projectiles\WrathCrystallizedFlame.cs` | `BlendState.Additive` @L171 | 6 |
| `DiesIrae\Weapons\WrathsCleaver\Projectiles\WrathsCleaverSwing.cs` | `BlendState.Additive` @L276 | 1 |

### Enigma Variations (1 file, 7 instances)

| File | Additive Source | Bug Count |
|------|-----------------|-----------|
| `EnigmaVariations\Bosses\EnigmaBossProjectiles.cs` | `BlendState.Additive` @L85/164/384 | 7 |

### Eroica (17 files, ~80 instances)

| File | Additive Source | Bug Count |
|------|-----------------|-----------|
| `Eroica\Minions\SakuraFlameProjectile.cs` | `BlendState.Additive` @L275 | 2 |
| `Eroica\Minions\SakuraOfFate.cs` | `BlendState.Additive` @L489, `BeginShaderAdditive` @L620/745 | 6 |
| `Eroica\Projectiles\BlossomOfTheSakuraBulletProjectile.cs` | `BeginAdditive` @L468, `BeginShaderAdditive` @L556/659 | 6 |
| `Eroica\Projectiles\BlossomVFXHelpers.cs` | `BlendState.Additive` @L101 | 1 |
| `Eroica\Projectiles\EroicaSplittingOrb.cs` | `BlendState.Additive` @L236 | 2 |
| `Eroica\Projectiles\FractalVFXHelpers.cs` | `BlendState.Additive` @L82 | 1 |
| `Eroica\Projectiles\FuneralPrayerBeam.cs` | `BeginAdditive` @L470, `BeginShaderAdditive` @L561/638 | 6 |
| `Eroica\Projectiles\FuneralPrayerProjectile.cs` | `BeginAdditive` @L374, `BeginShaderAdditive` @L420/518/550 | 6 |
| `Eroica\Projectiles\FuneralPrayerRicochetBeam.cs` | `BeginAdditive` @L470, `BeginShaderAdditive` @L561/646 | 7 |
| `Eroica\Projectiles\FuneralVFXHelpers.cs` | `BlendState.Additive` @L91 | 1 |
| `Eroica\Projectiles\PiercingLightOfTheSakuraProjectile.cs` | `BeginAdditive` @L463/639, `BeginShaderAdditive` @L506/688 | 9 |
| `Eroica\Projectiles\PiercingVFXHelpers.cs` | `BlendState.Additive` @L90 | 1 |
| `Eroica\Projectiles\SakuraLightning.cs` | `BlendState.Additive` @L188 | 3 |
| `Eroica\Projectiles\SakurasBlossomSpectral.cs` | `BlendState.Additive` @L233/292/343 | 6 |
| `Eroica\Projectiles\TriumphantFractalProjectile.cs` | `BeginAdditive` @L461/687, `BeginShaderAdditive` @L549/739 | 10 |
| `Eroica\Weapons\CelestialValor\CelestialValorProjectile.cs` | `BlendState.Additive` @L148 | 3 |
| `Eroica\Weapons\CelestialValor\CelestialValorSwing.cs` | `BeginAdditive` @L520/647/685, `BeginShaderAdditive` @L724/805/874/938 | 11 |
| `Eroica\Weapons\CelestialValor\Projectiles\ValorBeam.cs` | `BlendState.Additive` @L137 | 2 |
| `Eroica\Weapons\CelestialValor\Projectiles\ValorBoom.cs` | `BlendState.Additive` @L154 | 3 |
| `Eroica\Weapons\CelestialValor\Projectiles\ValorSlash.cs` | `BlendState.Additive` @L125 | 2 |
| `Eroica\Weapons\SakurasBlossom\SakurasBlossomSwing.cs` | `BeginAdditive` @L461/594/636, `BeginShaderAdditive` @L670/739/785/845 | 11 |

### Fate (13 files, ~115 instances — includes Palette.Additive helpers)

| File | Additive Source | Bug Count |
|------|-----------------|-----------|
| `Fate\ResonantWeapons\CodaOfAnnihilation\Projectiles\CodaHeldSwing.cs` | `BeginAdditive` | 11+ |
| `Fate\ResonantWeapons\DestinysCrescendo\Projectiles\CrescendoDeityMinion.cs` | `BeginAdditive` | varies |
| `Fate\ResonantWeapons\FractalOfTheStars\Projectiles\FractalOrbitBlade.cs` | `BeginAdditive` | varies |
| `Fate\ResonantWeapons\FractalOfTheStars\Projectiles\FractalSwingProjectile.cs` | `BeginAdditive` @L660 | 6+ |
| `Fate\ResonantWeapons\LightOfTheFuture\Projectiles\LightAcceleratingBullet.cs` | `BlendState.Additive` @L410 | 2+ |
| `Fate\ResonantWeapons\OpusUltima\Projectiles\OpusEnergyBallProjectile.cs` | `BeginAdditive` | varies |
| `Fate\ResonantWeapons\OpusUltima\Projectiles\OpusSwingProjectile.cs` | `BeginAdditive` | 23+ |
| `Fate\ResonantWeapons\RequiemOfReality\Projectiles\RequiemCosmicNote.cs` | `BlendState.Additive` | varies |
| `Fate\ResonantWeapons\RequiemOfReality\Projectiles\RequiemRealityTear.cs` | `BlendState.Additive` @L159 | 6 |
| `Fate\ResonantWeapons\RequiemOfReality\Projectiles\RequiemSpectralBlade.cs` | `BlendState.Additive` | varies |
| `Fate\ResonantWeapons\RequiemOfReality\Projectiles\RequiemSwingProjectile.cs` | `BeginAdditive` | 21+ |
| `Fate\ResonantWeapons\SymphonysEnd\SymphonysEndItem.cs` | `BlendState.Additive` | 3 |
| `Fate\ResonantWeapons\SymphonysEnd\Projectiles\SymphonyBladeFragment.cs` | `BeginAdditive` | varies |
| `Fate\ResonantWeapons\SymphonysEnd\Projectiles\SymphonySpiralBlade.cs` | `BeginAdditive` | 13+ |
| `Fate\ResonantWeapons\TheConductorsLastConstellation\Projectiles\ConductorSwingProjectile.cs` | `BeginAdditive` | varies |
| `Fate\ResonantWeapons\TheConductorsLastConstellation\Projectiles\ConductorSwordProjectile.cs` | `BeginAdditive` | varies |

### La Campanella (8 files, ~40 instances — includes Palette.Additive helpers)

| File | Additive Source | Bug Count |
|------|-----------------|-----------|
| `LaCampanella\ResonantWeapons\GrandioseChime\Projectiles\BellfireNoteProj.cs` | `BlendState.Additive` @L84 | 1 |
| `LaCampanella\ResonantWeapons\GrandioseChime\Projectiles\KillEchoProj.cs` | `BlendState.Additive` @L145 | 2 |
| `LaCampanella\ResonantWeapons\DualFatedChime\Projectiles\BellFlameWaveProj.cs` | `BeginAdditive` | varies |
| `LaCampanella\ResonantWeapons\DualFatedChime\Projectiles\DualFatedChimeSwingProjectile.cs` | `BeginAdditive` | varies |
| `LaCampanella\ResonantWeapons\DualFatedChime\Projectiles\InfernoWaltzProj.cs` | `BeginAdditive` | 3+ |
| `LaCampanella\ResonantWeapons\FangOfTheInfiniteBell\Projectiles\EmpoweredLightning...` | `BeginAdditive` | varies |
| `LaCampanella\ResonantWeapons\FangOfTheInfiniteBell\Projectiles\InfiniteBellOrb...` | `BeginAdditive` | varies |
| `LaCampanella\ResonantWeapons\IgnitionOfTheBell\Projectiles\ChimeCycloneProj.cs` | `BeginAdditive` | varies |
| `LaCampanella\ResonantWeapons\IgnitionOfTheBell\Projectiles\InfernalGeyserProj.cs` | `BeginAdditive` | varies |
| `LaCampanella\ResonantWeapons\InfernalChimesCalling\Projectiles\CampanellaChorus...` | `BeginAdditive` | varies |
| `LaCampanella\ResonantWeapons\InfernalChimesCalling\Projectiles\MinionShockwave...` | `BeginAdditive` | varies |

### Moonlight Sonata (12 files, ~40 instances)

| File | Additive Source | Bug Count |
|------|-----------------|-----------|
| `MoonlightSonata\Weapons\EternalMoon\Projectiles\EternalMoonGhost.cs` | `BlendState.Additive` @L143 | 2 |
| `MoonlightSonata\Weapons\EternalMoon\Projectiles\EternalMoonTidalDetonation.cs` | `BlendState.Additive` @L247 | 2 |
| `MoonlightSonata\Weapons\EternalMoon\Projectiles\EternalMoonWave.cs` | `BlendState.Additive` @L182 | 2 |
| `MoonlightSonata\Weapons\IncisorOfMoonlight\Projectiles\CrescentMoonProj.cs` | `BlendState.Additive` @L127 | 4 |
| `MoonlightSonata\Weapons\IncisorOfMoonlight\Projectiles\CrescentWaveProj.cs` | `BlendState.Additive` @L131 | 4 |
| `MoonlightSonata\Weapons\IncisorOfMoonlight\Projectiles\LunarBeamProj.cs` | `BlendState.Additive` @L109 | 3 |
| `MoonlightSonata\Weapons\IncisorOfMoonlight\Projectiles\LunarNova.cs` | `BlendState.Additive` @L96 | 2 |
| `MoonlightSonata\Weapons\MoonlightsCalling\Projectiles\SerenadeBeam.cs` | `BlendState.Additive` @L473 | 4 |
| `MoonlightSonata\Weapons\MoonlightsCalling\Projectiles\SerenadeHoldout.cs` | `BlendState.Additive` @L588 | 3 |
| `MoonlightSonata\Weapons\ResurrectionOfTheMoon\Projectiles\CometCore.cs` | `BlendState.Additive` @L300 | 5 |
| `MoonlightSonata\Weapons\ResurrectionOfTheMoon\Projectiles\ResurrectionProjectile.cs` | `BlendState.Additive` @L405 | 4 |
| `MoonlightSonata\Weapons\ResurrectionOfTheMoon\Projectiles\SupernovaShell.cs` | `BlendState.Additive` @L621 | 3 |
| `MoonlightSonata\Weapons\StaffOfTheLunarPhases\Projectiles\GoliathDevastatingBeam.cs` | `BlendState.Additive` @L347 | 3 |
| `MoonlightSonata\Weapons\StaffOfTheLunarPhases\Projectiles\GoliathMoonlightBeam.cs` | `BlendState.Additive` @L333 | 4 |

### Nachtmusik (14 files, ~95 instances)

| File | Additive Source | Bug Count |
|------|-----------------|-----------|
| `Nachtmusik\Weapons\CelestialChorusBaton\Projectiles\NocturnalGuardianMinion.cs` | `BeginShaderAdditive` @L148 | 4 |
| `Nachtmusik\Weapons\ConductorOfConstellations\Projectiles\StellarConductorMinion.cs` | `BeginShaderAdditive` @L135 | 6 |
| `Nachtmusik\Weapons\ConstellationPiercer\ConstellationPiercer.cs` | `BlendState.Additive` @L112 | 3 |
| `Nachtmusik\Weapons\ConstellationPiercer\Projectiles\ConstellationBoltProjectile.cs` | `BeginAdditive` @L246 | 5 |
| `Nachtmusik\Weapons\GalacticOverture\Projectiles\CelestialMuseMinion.cs` | `BeginShaderAdditive` @L116 | 4 |
| `Nachtmusik\Weapons\MidnightsCrescendo\MidnightsCrescendo.cs` | `BlendState.Additive` @L148 | 3 |
| `Nachtmusik\Weapons\MidnightsCrescendo\Projectiles\CrescendoWaveProjectile.cs` | `BlendState.Additive` @L215 | 8 |
| `Nachtmusik\Weapons\MidnightsCrescendo\Projectiles\MidnightsCrescendoSwing.cs` | `BeginShaderAdditive` @L382, `BeginAdditive` @L490 | 7 |
| `Nachtmusik\Weapons\NebulasWhisper\NebulasWhisper.cs` | `BlendState.Additive` @L122 | 3 |
| `Nachtmusik\Weapons\NebulasWhisper\Projectiles\NebulaWhisperShot.cs` | `BeginShaderAdditive` @L203, `BeginAdditive` @L256 | 7 |
| `Nachtmusik\Weapons\NocturnalExecutioner\NocturnalExecutioner.cs` | `BlendState.Additive` @L180 | 3 |
| `Nachtmusik\Weapons\NocturnalExecutioner\Projectiles\NocturnalBladeProjectiles.cs` | `BlendState.Additive` @L143/318, `BeginAdditive` @L157 | 5 |
| `Nachtmusik\Weapons\NocturnalExecutioner\Projectiles\NocturnalExecutionerSwing.cs` | `BeginShaderAdditive` @L328, `BeginAdditive` @L399 | 9 |
| `Nachtmusik\Weapons\RequiemOfTheCosmos\Projectiles\CosmicRequiemOrbProjectile.cs` | `BeginShaderAdditive` @L194, `BlendState.Additive` @L212 | 8 |
| `Nachtmusik\Weapons\SerenadeOfDistantStars\SerenadeOfDistantStars.cs` | `BlendState.Additive` @L112 | 3 |
| `Nachtmusik\Weapons\SerenadeOfDistantStars\Projectiles\SerenadeStarProjectile.cs` | `BeginShaderAdditive` @L260, `BeginAdditive` @L276 | 7 |
| `Nachtmusik\Weapons\StarweaversGrimoire\Projectiles\StarweaverOrbProjectile.cs` | `BeginShaderAdditive` @L160, `BlendState.Additive` @L178 | 6 |
| `Nachtmusik\Weapons\TwilightSeverance\TwilightSeverance.cs` | `BlendState.Additive` @L185 | 3 |
| `Nachtmusik\Weapons\TwilightSeverance\Projectiles\TwilightSeveranceSwing.cs` | `BeginShaderAdditive` @L287, `BeginAdditive` @L353 | 7 |
| `Nachtmusik\Weapons\TwilightSeverance\Projectiles\TwilightSlashProjectile.cs` | `BlendState.Additive` @L208 | 5 |

### Seasons / Spring (4 files, 28 instances)

| File | Additive Source | Bug Count |
|------|-----------------|-----------|
| `Seasons\Projectiles\VivaldiSeasonalWave.cs` | `BlendState.Additive` @L318 | 5 |
| `Seasons\Projectiles\VivaldiSpiritMinions.cs` | `BlendState.Additive` @L274/921/1213 | 6 |
| `Seasons\Weapons\FourSeasonsBlade.cs` | `BlendState.Additive` @L229 | 5 |
| `Spring\Projectiles\BloomArrow.cs` | `BlendState.Additive` @L255/474/602 | 12 |

### Swan Lake (1 file, 6 instances)

| File | Additive Source | Bug Count |
|------|-----------------|-----------|
| `SwanLake\Accessories\DualFeatherQuiver.cs` | `BlendState.Additive` @L667/913 | 6 |

---

## Root Cause Analysis

This bug is **systematic and pervasive** because the codebase has a widespread misunderstanding of when `A = 0` produces additive glow:

1. **Palette `Additive()` helpers** (e.g., `SwanLakePalette.Additive(c)` → `c with { A = 0 }`) were designed for the "premultiplied additive trick" that works under `BlendState.AlphaBlend`. But they are routinely called inside `BlendState.Additive` batches where `A = 0` makes draws invisible.

2. **`BeginAdditive()` / `BeginShaderAdditive()` helpers** all use `BlendState.Additive` (not `BlendState.AlphaBlend`), and code inside these blocks consistently applies `with { A = 0 }` — defeating the draws.

3. The pattern has been copy-pasted across **all themes** with the same incorrect assumption.

## Recommended Fix Strategy

### Option A: Change Additive Helpers to Use AlphaBlend (Preferred)
Change `BeginAdditive()` and similar helpers to use `BlendState.AlphaBlend` instead of `BlendState.Additive`. This makes the `A = 0` trick work correctly everywhere.

```csharp
// BEFORE (bugged):
public static void BeginAdditive(SpriteBatch sb) {
    sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, ...);
}

// AFTER (fixed):
public static void BeginAdditive(SpriteBatch sb) {
    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, ...);
}
```

**Caveat:** This changes behavior for any draws in these batches that do NOT use `A = 0`. Under `BlendState.Additive`, a color like `new Color(255, 0, 0, 128)` contributes `128/255 * RGB` additively. Under `BlendState.AlphaBlend`, the same color would contribute full RGB but also subtract `128/255` from the destination. The visual result differs. Each batch would need review.

### Option B: Remove A=0 and Keep BlendState.Additive
Remove `with { A = 0 }` / `.A = 0` from all colors inside Additive batches. Colors should use normal alpha for opacity control.

```csharp
// BEFORE (bugged):
sb.Draw(tex, pos, null, SomeColor with { A = 0 } * 0.5f, ...);

// AFTER (fixed — let alpha carry the opacity):
sb.Draw(tex, pos, null, SomeColor * 0.5f, ...);
```

This is the simpler per-file fix but requires touching all 726+ instances.

### Option C: Hybrid Approach
1. For bloom/glow overlays where the "A=0 additive trick" is specifically desired: switch those specific batches to `BlendState.AlphaBlend`
2. For draws that genuinely want additive blending: remove `A = 0` and keep `BlendState.Additive`

---

## Details File

The complete line-by-line stateful scan output is at:
`Documentation/Audits/Additive_A0_StatefulScan.txt`

This file lists every bug instance with:
- Line number of the bug
- Line number where the Additive batch begins
- Which method/pattern triggered the Additive state
- The actual code on that line
