# Boss Attack Brainstorming

> **50 Foundational Attack Patterns for Boss Implementation**
> 
> This document contains generic, reusable attack patterns that can be themed and adapted for any boss. Names are intentionally mechanical/descriptive to serve as implementation foundations.

---

## 🌍 CATEGORY 1: ENVIRONMENTAL MANIPULATION

### 1. **LocalizedGravityInversion**
- **Mechanic:** Creates circular zones where gravity is reversed for entities inside.
- **Parameters:** `zoneRadius`, `duration`, `zoneCount`, `gravityStrength`
- **Implementation:** Apply negative Y velocity to players in zone. Zone positions can be random or pattern-based.

### 2. **GroundSinkingZones**
- **Mechanic:** Marked ground areas that pull players downward. Standing still increases sink rate.
- **Parameters:** `sinkRate`, `zoneRadius`, `damagePerSecond`, `zoneCount`
- **Implementation:** Apply downward velocity + damage over time to players in zone. Movement resets sink progress.

### 3. **ScreenMirrorEffect**
- **Mechanic:** Visual left-right or up-down inversion of the game view. Controls remain normal.
- **Parameters:** `duration`, `mirrorAxis` (horizontal/vertical/both)
- **Implementation:** Shader or render target flip. Purely visual disorientation.

### 4. **ProjectileTimeZones**
- **Mechanic:** Spherical areas where projectile velocity is modified (slowed or sped up).
- **Parameters:** `zoneRadius`, `timeMultiplier`, `duration`, `affectsPlayerProjectiles`, `affectsBossProjectiles`
- **Implementation:** Modify projectile velocity when inside zone bounds.

### 5. **SequentialPlatformCollapse**
- **Mechanic:** Platforms/ground segments are marked in sequence, then collapse after delay.
- **Parameters:** `collapseDelay`, `collapseSequenceInterval`, `platformCount`, `respawnTime`
- **Implementation:** Mark tiles, start timer, destroy/disable on timer completion.

---

## 🔷 CATEGORY 2: GEOMETRIC PATTERN ATTACKS

### 6. **PointConnectLaserGrid**
- **Mechanic:** Multiple points spawn, then laser beams connect them in a pattern (triangle, star, grid).
- **Parameters:** `pointCount`, `pattern` (triangle/square/star/random), `telegraphTime`, `beamDuration`, `beamWidth`
- **Implementation:** Spawn point markers, calculate connection lines, activate beams after telegraph.

### 7. **SpiralProjectilePattern**
- **Mechanic:** Projectiles spawn in an expanding spiral pattern from a center point.
- **Parameters:** `armCount`, `projectilesPerArm`, `spiralTightness`, `expansionSpeed`, `projectileSpeed`
- **Implementation:** Use polar coordinates with incrementing angle and radius for spawn positions.

### 8. **RecursiveSplitExplosion**
- **Mechanic:** Explosion spawns smaller explosions, which spawn even smaller ones. Recursive depth.
- **Parameters:** `recursionDepth`, `splitCount`, `sizeReductionFactor`, `delayBetweenSplits`
- **Implementation:** On explosion, spawn N smaller projectiles at reduced scale. Each can trigger same logic.

### 9. **ShapedProjectileFormation**
- **Mechanic:** Projectiles arranged in geometric shapes (L, T, square, line) that move as a unit.
- **Parameters:** `shape`, `formationSize`, `moveSpeed`, `rotationSpeed`, `projectileSpacing`
- **Implementation:** Define shape template, spawn projectiles at relative positions, move formation as group.

### 10. **ConcentricRotatingRings**
- **Mechanic:** Multiple rings of projectiles rotate around center, each ring rotating opposite direction.
- **Parameters:** `ringCount`, `projectilesPerRing`, `ringRadii[]`, `rotationSpeeds[]`, `gapSize`
- **Implementation:** Each ring is independent rotation. Player must weave through gaps between rings.

---

## 🎵 CATEGORY 3: TIMING-BASED ATTACKS

### 11. **RhythmicPulseDamage**
- **Mechanic:** Boss emits damage pulses at regular intervals. Proximity determines damage.
- **Parameters:** `pulseInterval`, `pulseRadius`, `damageAtCenter`, `damageFalloff`
- **Implementation:** Timer-based pulse emission. Damage = base * (1 - distance/radius).

### 12. **HorizontalLaneProjectiles**
- **Mechanic:** Arena divided into horizontal lanes. Projectiles spawn on specific lanes at specific times.
- **Parameters:** `laneCount`, `laneHeight`, `projectilePattern[]`, `spawnInterval`
- **Implementation:** Define which lanes are dangerous per time unit. Visual lane markers help readability.

### 13. **EscalatingIntensityAttack**
- **Mechanic:** Attack starts slow/weak, accelerates and intensifies over duration until final burst.
- **Parameters:** `initialRate`, `finalRate`, `rampDuration`, `finalBurstDamage`, `interruptThreshold`
- **Implementation:** Lerp between initial and final parameters over time. Optional cancel on damage threshold.

### 14. **TemporarySafeZoneSpawns**
- **Mechanic:** During dangerous attack, small safe zones appear briefly. Player must reach them.
- **Parameters:** `safeZoneRadius`, `safeZoneDuration`, `spawnInterval`, `dangerousDamage`
- **Implementation:** Everything outside safe zone deals damage. Safe zone relocates periodically.

### 15. **OffBeatAttackPattern**
- **Mechanic:** Attacks occur on unexpected timing (syncopated). Pattern is consistent but non-obvious.
- **Parameters:** `beatInterval`, `offBeatOffset`, `attacksPerCycle`
- **Implementation:** Instead of attacking on beat, attack between beats. Rewards pattern learning.

---

## 👁️ CATEGORY 4: TRACKING & TARGETING ATTACKS

### 16. **DelayedPositionTargeting**
- **Mechanic:** Attack targets where player WAS, not where they ARE. Past positions become danger zones.
- **Parameters:** `positionDelay` (frames), `attackCount`, `attackInterval`
- **Implementation:** Store player position history. Attack targets position from N frames ago.

### 17. **EntityGazeTracking**
- **Mechanic:** A tracking element (eye, cursor) follows player. Fires attack in look direction after delay.
- **Parameters:** `trackingSpeed`, `chargeTime`, `attackType`, `trackingDuration`
- **Implementation:** Rotate toward player at limited speed. Fire in facing direction after charge completes.

### 18. **FakeTelegraphMisdirection**
- **Mechanic:** Shows obvious telegraph for attack direction A, then attacks from direction B.
- **Parameters:** `fakeTelegraphDuration`, `realTelegraphDuration`, `fakeDirection`, `realDirection`
- **Implementation:** Display fake windup, brief pause, then real attack from different angle.

### 19. **InvisibilityPhase**
- **Mechanic:** Boss or projectiles become invisible/transparent. Audio cues indicate position.
- **Parameters:** `invisDuration`, `visibilityLevel` (0-1), `audioIntensity`
- **Implementation:** Set alpha to near-zero. Play positional audio. Optional shimmer effect.

### 20. **MultipleEntityOneReal**
- **Mechanic:** Multiple identical copies appear. Only one is "real" (can be damaged/deals full damage).
- **Parameters:** `copyCount`, `realEntityTell` (subtle visual difference), `copyDamageMult`
- **Implementation:** Spawn copies with slight visual variation on real one. Copies deal reduced/no damage.

---

## 🔄 CATEGORY 5: STATE & TRANSFORMATION

### 21. **CyclingElementalState**
- **Mechanic:** Boss cycles through distinct states, each with different attack properties and vulnerabilities.
- **Parameters:** `stateCount`, `stateDuration`, `stateProperties[]` (damage type, color, attack set)
- **Implementation:** Timer-based state machine. Each state loads different attack pool and visuals.

### 22. **SizeOscillation**
- **Mechanic:** Boss scale changes over time. Larger = slower movement, bigger hitbox. Smaller = faster, smaller hitbox.
- **Parameters:** `minScale`, `maxScale`, `oscillationPeriod`, `scaleAffectsSpeed`, `scaleAffectsDamage`
- **Implementation:** Sinusoidal scale interpolation. Adjust movement speed inversely to scale.

### 23. **BehaviorModeSwitch**
- **Mechanic:** Boss has distinct behavior modes (aggressive, defensive, erratic, evasive) that swap based on triggers.
- **Parameters:** `modes[]`, `modeTriggers` (HP threshold, time, player action), `modeProperties[]`
- **Implementation:** State machine with trigger conditions. Each mode has different AI parameters.

### 24. **EntitySplitAndMerge**
- **Mechanic:** Boss splits into multiple smaller entities that fight independently, then merge back together.
- **Parameters:** `splitCount`, `splitDuration`, `sharedHP`, `mergeAttack`
- **Implementation:** Spawn child NPCs on split. Track combined HP. Despawn children and restore boss on merge.

### 25. **PermanentFormEvolution**
- **Mechanic:** Boss permanently transforms at HP thresholds. Each form has unique attacks and appearance.
- **Parameters:** `formCount`, `hpThresholds[]`, `formProperties[]`
- **Implementation:** On HP threshold, play transformation, load new sprite/attack set. No reverting.

---

## 🎯 CATEGORY 6: PROJECTILE BEHAVIOR VARIANTS

### 26. **ReturnToSourceProjectile**
- **Mechanic:** Projectiles travel outward, pause at max range, then return to spawn point.
- **Parameters:** `outwardSpeed`, `pauseDuration`, `returnSpeed`, `maxRange`
- **Implementation:** Phase-based AI: outward phase, pause phase, return phase.

### 27. **ChargeBasedAttraction**
- **Mechanic:** Projectiles and player have +/- charges. Same charge = repulsion, opposite = attraction.
- **Parameters:** `attractionStrength`, `chargeSwapMethod` (dash, timer, item), `projectileCharge`
- **Implementation:** Calculate force vector between charged entities. Apply to projectile velocity.

### 28. **SplitOnDamageProjectile**
- **Mechanic:** Large projectile splits into smaller ones when struck by player attack.
- **Parameters:** `splitCount`, `childScale`, `splitVelocitySpread`, `splitOnlyFromPlayerDamage`
- **Implementation:** OnHit check for player projectile. Spawn children with velocity variance.

### 29. **OrbitingShieldProjectiles**
- **Mechanic:** Projectiles orbit boss, blocking player attacks. Gaps in orbit allow shots through.
- **Parameters:** `orbitRadius`, `orbitSpeed`, `shieldCount`, `shieldHP`, `respawnTime`
- **Implementation:** Circular motion around boss center. Shields have HP and can be destroyed.

### 30. **AngularHomingProjectile**
- **Mechanic:** Homing projectile that can only turn at fixed angles (90°, 45°). Creates grid-like paths.
- **Parameters:** `turnAngle`, `turnInterval`, `maxTurns`, `moveSpeed`
- **Implementation:** On turn interval, calculate new direction snapped to angle. Apply until next turn.

---

## 🏟️ CATEGORY 7: ARENA CONTROL

### 31. **ShrinkingBoundary**
- **Mechanic:** Arena boundaries close in over time, reducing safe space. Resets after reaching minimum.
- **Parameters:** `shrinkSpeed`, `minArenaSize`, `boundaryDamage`, `resetBehavior`
- **Implementation:** Move damage walls inward. Deal damage on contact. Reset or hold at minimum.

### 32. **TileConversionSpread**
- **Mechanic:** Boss contact converts tiles to damaging terrain. Conversion spreads over time.
- **Parameters:** `conversionRadius`, `spreadRate`, `spreadCap`, `damagePerTile`
- **Implementation:** Mark tiles as converted on boss contact. Spread to adjacent tiles over time.

### 33. **IsolatedArenaPhase**
- **Mechanic:** Player is pulled into isolated mini-arena for survival challenge. Must complete to return.
- **Parameters:** `arenaSize`, `challengeDuration`, `challengeAttacks[]`, `exitCondition`
- **Implementation:** Teleport player to separate area. Run challenge sequence. Return on completion.

### 34. **RotatingArenaView**
- **Mechanic:** Arena visually rotates around boss. Player's relative position changes without movement.
- **Parameters:** `rotationSpeed`, `rotationDirection`, `affectsProjectiles`
- **Implementation:** Rotate camera/background. Optionally rotate projectile spawn positions to match.

### 35. **MovingSafeZone**
- **Mechanic:** Only one small area is safe. Safe area relocates periodically. Must chase the safe zone.
- **Parameters:** `safeRadius`, `relocateInterval`, `transitionTime`, `outsideDamage`
- **Implementation:** Mark safe position. Deal damage outside. Move safe position on timer.

---

## ⚔️ CATEGORY 8: INTERACTIVE MECHANICS

### 36. **TimedParryWindow**
- **Mechanic:** Specific attacks can be parried by player attack during brief window. Parry damages boss.
- **Parameters:** `parryWindowStart`, `parryWindowDuration`, `parryDamage`, `failedParryPenalty`
- **Implementation:** Mark attack as parryable. Check for player attack collision during window frames.

### 37. **ProjectileAbsorptionOrb**
- **Mechanic:** Slow-moving orb absorbs player projectiles it contacts. Grows larger. Fires back when full.
- **Parameters:** `moveSpeed`, `absorptionCap`, `growthPerAbsorb`, `returnDamageMultiplier`
- **Implementation:** Track absorbed damage. Scale orb. Fire at player when threshold reached.

### 38. **ReflectableEnvironmentObject**
- **Mechanic:** Boss spawns objects player can strike to launch at boss, dealing significant damage.
- **Parameters:** `objectHP`, `launchSpeed`, `bossHitDamage`, `objectSpawnInterval`
- **Implementation:** Spawn hittable projectile. On player hit, reverse direction toward boss.

### 39. **ResourceDrainAttack**
- **Mechanic:** Boss drains player resource (mana, held charge, buff duration) and uses it against them.
- **Parameters:** `drainRate`, `drainType`, `stolenResourceUse`
- **Implementation:** Reduce player resource value. Boss performs attack scaled to stolen amount.

### 40. **AttackMimicry**
- **Mechanic:** Boss copies player's most recent attack type and uses mirrored version against them.
- **Parameters:** `mimicDelay`, `mimicDamageScale`, `mimicDuration`
- **Implementation:** Track last player weapon/attack used. Boss performs similar attack pattern.

---

## 🎬 CATEGORY 9: VISUAL SPECTACLE ATTACKS

### 41. **FullDarknessPhase**
- **Mechanic:** All light sources extinguished. Only boss attacks and glowing elements visible.
- **Parameters:** `darknessDuration`, `bossGlowIntensity`, `attackGlowIntensity`
- **Implementation:** Override lighting to minimal. Boss/attack sprites use self-illumination.

### 42. **SlowMotionExecution**
- **Mechanic:** Game speed dramatically reduced for high-damage attack. Extra time to react but less space.
- **Parameters:** `slowFactor`, `slowDuration`, `attackDuringSlowmo`
- **Implementation:** Reduce Main.timeScale or manually slow all velocities. Attack is visually impressive.

### 43. **FloorHazardWaves**
- **Mechanic:** Ground becomes hazardous in waves. Player must use platforms or flight to survive.
- **Parameters:** `waveDuration`, `safeZones[]`, `hazardDamage`, `wavePattern`
- **Implementation:** All ground-level tiles deal damage. Mark safe platforms visually.

### 44. **TelegraphedMassiveStrike**
- **Mechanic:** Huge attack with very long telegraph. Massive visual but avoidable with positioning.
- **Parameters:** `telegraphDuration`, `impactRadius`, `impactDamage`, `telegraphVisual`
- **Implementation:** Large visual warning (shadow, descending object). Devastating damage on impact.

### 45. **DenseSlowProjectileField**
- **Mechanic:** Extremely dense projectile curtain moving very slowly. Navigation puzzle through static field.
- **Parameters:** `projectileDensity`, `fieldSpeed`, `fieldWidth`, `gapSize`
- **Implementation:** Spawn dense grid of slow projectiles. Player weaves through at normal speed.

---

## 🎲 CATEGORY 10: SPECIAL MECHANICS

### 46. **RandomAttackSelection**
- **Mechanic:** Attack chosen randomly from pool with visible selection process. Player sees what's coming.
- **Parameters:** `attackPool[]`, `selectionVisualDuration`, `selectionBias`
- **Implementation:** Display attack options. Animate selection. Execute chosen attack.

### 47. **DistractionEntitySpawns**
- **Mechanic:** Minor entities spawn that harass player with non-lethal interference during boss attacks.
- **Parameters:** `spawnCount`, `spawnInterval`, `distractionDamage`, `distractionHP`
- **Implementation:** Spawn weak enemies that chip damage or obscure vision. Player can kill for drops.

### 48. **ReverseHealingTether**
- **Mechanic:** Damage player takes is prevented but heals boss for greater amount. Avoiding damage is critical.
- **Parameters:** `tetherDuration`, `healMultiplier`, `maxHealPerSecond`
- **Implementation:** Intercept player damage. Cancel damage. Heal boss for multiplied amount.

### 49. **DamageBasedAttackCancel**
- **Mechanic:** Multiple attacks telegraph simultaneously. One with most player damage is cancelled.
- **Parameters:** `attackCount`, `damageWindow`, `cancelledAttackCount`
- **Implementation:** Display attack sigils with damage counters. Remove lowest-damaged from execution.

### 50. **MetaGameInterference**
- **Mechanic:** Attack affects game UI elements - health bar moves, fake error messages, input display scramble.
- **Parameters:** `interferenceType`, `interferenceDuration`, `actualMechanicBehindInterference`
- **Implementation:** Modify UI positions temporarily. All "broken" elements have underlying real mechanic.

---

## 📋 IMPLEMENTATION TEMPLATE

```csharp
/// <summary>
/// [ATTACK_NAME] - [Brief description]
/// Category: [Category name]
/// </summary>
private void Attack_[AttackName](Player target)
{
    // PARAMETERS (adjust per difficulty tier)
    int paramA = baseValue + difficultyTier * scaling;
    float paramB = baseFloat - difficultyTier * reduction;
    
    // PHASE 0: TELEGRAPH
    if (SubPhase == 0)
    {
        float progress = Timer / (float)telegraphTime;
        
        // Visual warnings
        BossVFXOptimizer.WarningMethod(...);
        
        if (Timer >= telegraphTime)
        {
            Timer = 0;
            SubPhase = 1;
        }
    }
    // PHASE 1: EXECUTION
    else if (SubPhase == 1)
    {
        // Spawn projectiles / deal damage / apply effect
        if (Timer == 1 && Main.netMode != NetmodeID.MultiplayerClient)
        {
            // Attack logic
        }
        
        if (Timer >= executionTime)
        {
            Timer = 0;
            SubPhase = 2;
        }
    }
    // PHASE 2: RECOVERY
    else
    {
        if (Timer >= recoveryTime)
        {
            EndAttack();
        }
    }
}
```

---

## 📊 PARAMETER REFERENCE

| Parameter Type | Common Names | Typical Range | Scaling Direction |
|---------------|--------------|---------------|-------------------|
| **Timing** | `telegraphTime`, `duration`, `interval` | 20-120 frames | Decrease with difficulty |
| **Count** | `projectileCount`, `waveCount`, `zoneCount` | 3-30 | Increase with difficulty |
| **Speed** | `moveSpeed`, `rotationSpeed`, `projectileSpeed` | 5-25 | Increase with difficulty |
| **Size** | `radius`, `width`, `zoneSize` | 50-300 pixels | Decrease safe zones, increase danger zones |
| **Damage** | `baseDamage`, `damagePerTick` | 50-150 | Slight increase with difficulty |

---

## 🔗 COMBINATION SUGGESTIONS

**High Difficulty Combos:**
- `ShrinkingBoundary` + `ConcentricRotatingRings` = Shrinking space + rotating hazards
- `DelayedPositionTargeting` + `DenseSlowProjectileField` = Past and present both dangerous
- `InvisibilityPhase` + `RhythmicPulseDamage` = Audio-based survival

**Fair But Challenging Combos:**
- `PointConnectLaserGrid` + `TemporarySafeZoneSpawns` = Learn pattern, find safety
- `ReturnToSourceProjectile` + `SpiralProjectilePattern` = Dodge twice, learn spiral
- `EntitySplitAndMerge` + `BehaviorModeSwitch` = Split entities have different modes

---

## 📝 IMPLEMENTATION NOTES

1. **All attacks need telegraph phase** - Minimum 20-30 frames of visual warning.

2. **Parameters should scale with `difficultyTier`** - Use the standard 3-tier HP-based system.

3. **Use BossVFXOptimizer methods** - `WarningLine`, `SafeZoneRing`, `DangerZoneRing`, `ConvergingWarning`, etc.

4. **Mix projectile types** - Use `BossProjectileHelper` variety: `SpawnHostileOrb`, `SpawnAcceleratingBolt`, `SpawnWaveProjectile`.

5. **Include recovery windows** - Player needs time to reposition and deal damage between attacks.

6. **Theme the visuals, not the mechanics** - Same `PointConnectLaserGrid` can be fire beams, ice lines, void tethers, etc.
