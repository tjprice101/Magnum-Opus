# Curated Boss Effects and How-To Guide

## 📖 Overview

This document provides a comprehensive deep-dive into all MagnumOpus bosses, their attack patterns, AI systems, and visual effects implementations. Use this as a reference for creating consistent, high-quality boss content.

---

## ⭐⭐⭐ THE CARDINAL RULE: EVERY ATTACK IS UNIQUE ⭐⭐⭐

> **THIS IS THE ABSOLUTE #1 RULE FOR BOSS DESIGN.**

### Every Boss Attack Must Have Its Own Visual Identity

If a boss has 8 attacks, those 8 attacks must be **COMPLETELY VISUALLY DIFFERENT**:

| Attack | Telegraph | Execution | Impact | Unique Element |
|--------|-----------|-----------|--------|----------------|
| Attack 1 | Converging music notes | Radial burst with safe arc | Harmonic shockwave + note scatter | Notes spiral inward during charge |
| Attack 2 | Warning lines + ground markers | Dash with afterimages | Rising flame pillars along path | Flame trails linger and damage |
| Attack 3 | Orbiting glyph circle | Laser beam sweep | Chromatic aberration pulse | Beam leaves burning glyphs |
| Attack 4 | Feathers gathering | Feather storm barrage | Crystalline shard explosion | Feathers home then explode |
| Attack 5 | Eye formation watching player | Targeted orb volley | Eyes remain watching, then burst | Orbs connect with light beams |
| Attack 6 | Smoke cloud buildup | Ground slam shockwave | Smoke explosion + ember rain | Ground cracks with fire |
| Attack 7 | Star constellation forming | Star projectile rain | Constellation connects on impact | Stars leave sparkle trails |
| Attack 8 | Reality distortion effect | Screen-wide wave attack | Chromatic scatter + reality crack | Brief color inversion |

**EVERY attack tells its own story. EVERY attack is memorable.**

### The Forbidden Pattern for Boss Attacks

```csharp
// ❌ FORBIDDEN - Generic, boring boss attack
private void Attack_GenericBurst()
{
    // Spawn some projectiles in a circle
    for (int i = 0; i < 8; i++)
    {
        float angle = MathHelper.TwoPi * i / 8f;
        Vector2 vel = angle.ToRotationVector2() * 10f;
        Projectile.NewProjectile(NPC.Center, vel, projType, damage, 0f);
    }
    // Some generic particles
    CustomParticles.GenericFlare(NPC.Center, themeColor, 0.5f, 15);
}
// This is LAZY. Every attack deserves unique VFX.
```

---

## 🎵 MUSIC NOTES IN BOSS FIGHTS

> **THIS IS A MUSIC MOD. BOSS ATTACKS MUST INCLUDE VISIBLE MUSIC NOTES.**

### Where Music Notes MUST Appear in Boss Fights:

1. **Attack Telegraphs** - Notes converging during charge-up
2. **Projectile Trails** - Music notes scattered in wake
3. **Impact Effects** - Note bursts on hits
4. **Death Explosions** - Massive note cascade
5. **Ambient Effects** - Floating notes around boss

### Music Note Scale Requirements (CRITICAL)

```csharp
// ❌ INVISIBLE - Scale too small
ThemedParticles.MusicNote(pos, vel, color, 0.25f, 20, 1); // CAN'T SEE THIS

// ✅ VISIBLE - Proper scale with bloom
float scale = Main.rand.NextFloat(0.7f, 1.0f);
float shimmer = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.15f;
ThemedParticles.MusicNote(pos, vel, color, scale * shimmer, 30, Main.rand.Next(1, 7));

// Add sparkle companions for extra visibility
CustomParticles.PrismaticSparkle(pos, color, 0.35f, Main.rand.Next(1, 16));
```

---

## 🎨 USE ALL AVAILABLE PARTICLE ASSETS

> **You have 80+ custom particle PNGs. USE THEM in boss attacks.**

### Required Particle Mix Per Attack

**EVERY boss attack effect MUST combine:**
1. At least **2 different custom particle types**
2. At least **1 vanilla Dust type** for density
3. At least **1 music-related particle** (MusicNote, staff lines)
4. **Multi-layer bloom** for glow effects

### Available Assets (USE ALL OF THESE)

| Category | Files | Use For |
|----------|-------|---------|
| MusicNote (6) | `MusicNote1-6.png` | EVERY attack telegraph, trail, impact |
| EnergyFlare (7) | `EnergyFlare1-7.png` | Attack cores, charge effects |
| GlowingHalo (5) | `GlowingHalo1-6.png` | Shockwaves, expansion rings |
| PrismaticSparkle (15) | `PrismaticSparkle1-15.png` | Sparkle accents everywhere |
| Glyphs (12) | `Glyphs1-12.png` | Magic circles, enchantment effects |
| SwanFeather (10) | `SwanFeather1-10.png` | Swan Lake attacks |
| EnigmaEye (8) | `EnigmaEye1-8.png` | Enigma watching effects |
| SwordArc (9) | `SwordArc1-9.png` | Melee boss attacks |

### Vanilla Dust for Density

```csharp
// ALWAYS add vanilla dust to boss attacks
DustID.MagicMirror      // Magical shimmer
DustID.Enchanted_Gold   // Golden sparkles
DustID.PurpleTorch      // Purple flames
DustID.Electric         // Electric sparks
DustID.GemAmethyst      // Purple gems
```

---

## 🏗️ BOSS ARCHITECTURE FUNDAMENTALS

### AI State Machine Pattern

All bosses use an enum-based state machine with the following structure:

```csharp
// NPC.ai array usage (standard across all bosses):
// ai[0] = BossPhase/State (main state machine)
// ai[1] = Timer (frame counter within current state)
// ai[2] = CurrentAttack/AttackPattern
// ai[3] = SubPhase (attack sub-step counter)

private BossPhase State
{
    get => (BossPhase)NPC.ai[0];
    set => NPC.ai[0] = (float)value;
}

private int Timer
{
    get => (int)NPC.ai[1];
    set => NPC.ai[1] = value;
}
```

### Difficulty Tier System

All bosses scale difficulty based on remaining HP:

```csharp
private void UpdateDifficultyTier()
{
    float hpPercent = (float)NPC.life / NPC.lifeMax;
    // Tier 0: 100-70% HP
    // Tier 1: 70-40% HP  
    // Tier 2: 40-0% HP
    int newTier = hpPercent > 0.7f ? 0 : (hpPercent > 0.4f ? 1 : 2);
}
```

### Attack Selection Pattern

```csharp
private void SelectNextAttack(Player target)
{
    // 1. Build attack pool based on difficulty tier
    List<AttackPattern> pool = new List<AttackPattern> { /* core attacks */ };
    
    if (difficultyTier >= 1) pool.Add(/* phase 2 attacks */);
    if (difficultyTier >= 2) pool.Add(/* phase 3 attacks */);
    
    // 2. Remove last used attack to prevent repetition
    pool.Remove(lastAttack);
    
    // 3. Random selection
    CurrentAttack = pool[Main.rand.Next(pool.Count)];
    lastAttack = CurrentAttack;
    
    // 4. Reset state
    Timer = 0;
    SubPhase = 0;
    State = BossPhase.Attack;
}
```

---

## 🦅 EROICA, GOD OF VALOR

**File:** `Content/Eroica/Bosses/EroicasRetribution.cs`  
**HP:** 450,000 | **Defense:** 80 | **Contact Damage:** 95  
**Theme:** Heroic triumph, sakura petals, scarlet → gold gradient

### Theme Colors
```csharp
private static readonly Color EroicaGold = new Color(255, 200, 80);
private static readonly Color EroicaScarlet = new Color(200, 50, 50);
private static readonly Color EroicaCrimson = new Color(180, 30, 60);
private static readonly Color SakuraPink = new Color(255, 150, 180);
```

### Phase System
| Phase | HP Range | Available Attacks |
|-------|----------|-------------------|
| Phase 1 | 100% | Kill 3 Flames of Valor (invulnerable) |
| Phase 2A | 100-70% | SwordDash, HeroicBarrage, GoldenRain |
| Phase 2B | 70-40% | +ValorCross, SakuraStorm, TriumphantCharge |
| Phase 2C | 40-0% | +PhoenixDive, HeroesJudgment, UltimateValor |

### Attack Breakdown

#### **SwordDash** (Core)
- **Type:** Multi-dash attack with projectile pressure
- **Telegraph:** 24 frame warning with trajectory line (red)
- **Execution:** 3-5 dashes at 42-58 speed
- **VFX:** Warning line → converging particles → sakura trail

```csharp
// Key implementation pattern:
if (SubPhase == 0) // Telegraph
{
    BossVFXOptimizer.WarningLine(NPC.Center, dashDirection, 600f, 12, WarningType.Danger);
    BossVFXOptimizer.ConvergingWarning(NPC.Center, 60f, progress, EroicaGold, 6);
}
else if (SubPhase == 1) // Execute dash
{
    NPC.velocity = dashDirection * (42f + difficultyTier * 8f);
    BossVFXOptimizer.AttackReleaseBurst(NPC.Center, EroicaGold, EroicaScarlet, 0.8f);
}
```

#### **HeroicBarrage** (Core)
- **Type:** Multi-wave spread shot
- **Pattern:** 9-15 projectiles in 80-100° arc
- **Mix:** Homing orbs + accelerating bolts alternating

#### **GoldenRain** (Core)
- **Type:** Area denial from above
- **Telegraph:** Ground impact warnings + spawn point flares
- **Projectiles:** Accelerating bolts + homing orbs raining down

#### **ValorCross** (Phase 2B)
- **Type:** 8-arm star burst pattern
- **Telegraph:** Warning lines for each arm direction
- **Waves:** 3-5 patterns with rotation offset

#### **SakuraStorm** (Phase 2B)
- **Type:** Orbiting boss with spiral projectiles
- **Movement:** Boss circles player, radius shrinks with aggression
- **Pattern:** 4-6 spiral arms of wave projectiles

#### **PhoenixDive** (Phase 2C)
- **Type:** Teleport-above + dive bomb
- **Telegraph:** Clear dive trajectory + ground impact zone
- **Impact:** Radial projectile burst on landing

#### **HeroesJudgment** (Phase 2C) ⭐ SIGNATURE ATTACK
- **Type:** Spectacle radial burst with safe arc
- **Telegraph:** Converging particles + safe zone ring + safe arc indicator
- **Safe Arc:** 22-28° gap aimed at player (tighter at higher difficulty)
- **Waves:** 3-5 waves of 40-60 projectiles each

```csharp
// Safe arc implementation:
float safeAngle = (target.Center - NPC.Center).ToRotation();
float safeArc = MathHelper.ToRadians(22f - difficultyTier * 2f);

for (int i = 0; i < projectileCount; i++)
{
    float angle = MathHelper.TwoPi * i / projectileCount;
    float angleDiff = MathHelper.WrapAngle(angle - safeAngle);
    if (Math.Abs(angleDiff) < safeArc) continue; // Skip safe zone
    // Spawn projectile...
}
```

#### **UltimateValor** (Phase 2C Ultimate)
- **Type:** Multi-phase ultimate attack
- **Includes:** Charge up → rapid dashes → projectile storm → finale burst

---

## 🔔 LA CAMPANELLA, CHIME OF LIFE

**File:** `Content/LaCampanella/Bosses/LaCampanellaChimeOfLife.cs`  
**HP:** 400,000 | **Defense:** 75 | **Contact Damage:** 110  
**Theme:** Infernal bell, black smoke, orange fire

### Theme Colors
```csharp
private static readonly Color CampanellaOrange = new Color(255, 140, 40);
private static readonly Color CampanellaGold = new Color(255, 200, 80);
private static readonly Color CampanellaBlack = new Color(30, 20, 25);
private static readonly Color CampanellaCrimson = new Color(200, 50, 30);
```

### Phase System
| Phase | HP Range | Available Attacks |
|-------|----------|-------------------|
| Phase 1 | 100-65% | BellSlam, TollWave, EmberShower |
| Phase 2 | 65-35% | +FireWallSweep, ChimeRings, InfernoCircle, RhythmicToll, InfernalJudgment, BellLaserGrid |
| Phase 3 | 35-0% | +TripleSlam, InfernalTorrent, InfernoCage, ResonantShock, GrandFinale |

### Unique Mechanics

**Ground-Based Movement:**
```csharp
// La Campanella stays grounded, tracks player horizontally
if (Math.Abs(NPC.Center.X - targetX) > 150f)
{
    float dir = Math.Sign(targetX - NPC.Center.X);
    NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, dir * moveSpeed, 0.06f);
}
```

### Attack Breakdown

#### **BellSlam** (Core)
- **Type:** Jump + slam attack
- **Telegraph:** Converging particles + ground impact warnings
- **Impact:** Horizontal shockwave projectiles

#### **TollWave** (Core)
- **Type:** Radial burst on each bell toll
- **Pattern:** 8-12 projectiles per toll, 3-5 tolls total
- **Sound:** Bell chime synced with projectile spawn

#### **InfernalJudgment** (Phase 2) ⭐ SIGNATURE ATTACK
- **Type:** Hero's Judgment style spectacle
- **Telegraph:** Converging fire + safe zone ring
- **Safe Arc:** 25° gap for player escape

#### **BellLaserGrid** (Phase 2)
- **Type:** Crossing laser beam pattern
- **Telegraph:** Laser beam warning lines
- **Pattern:** 4+ intersecting beams from boss

#### **ResonantShock** (Phase 3)
- **Type:** Electrical pulse wave
- **Telegraph:** Electrical buildup warning
- **Pattern:** Expanding ring of electrical projectiles

#### **RhythmicToll** (Phase 2) - BULLET HELL
- **Type:** Calamity-style rhythm attack
- **Pattern:** Timed projectile waves synced to music tempo (150 BPM)
- **Mechanic:** 24 frames = 1 beat, projectiles on beats

---

## 🦢 SWAN LAKE, THE MONOCHROMATIC FRACTAL

**File:** `Content/SwanLake/Bosses/SwanLakeTheMonochromaticFractal.cs`  
**HP:** 950,000 | **Defense:** 110 | **Contact Damage:** 170  
**Theme:** Ballet elegance, monochrome with rainbow shimmer

### Mood System (DoG-Inspired)
```csharp
private enum BossMood
{
    Graceful,    // 100-60% - Elegant dance, measured attacks
    Tempest,     // 60-30% - Storm intensifies, aggressive
    DyingSwan    // 30-0% - Tragic finale, desperate beauty
}
```

### Theme Colors
```csharp
// Primary: Black and White contrast
// Accent: Rainbow/prismatic shimmer
Color.White, new Color(30, 30, 40) // Black
Main.hslToRgb(hue, 1f, 0.85f) // Rainbow cycling
```

### Attack Breakdown

#### **FeatherCascade** (Easy)
- **Type:** Feather projectile spread
- **Pattern:** Fan of white/black feathers

#### **PrismaticRing** (Easy)
- **Type:** Expanding ring of prismatic projectiles
- **VFX:** Rainbow color cycling

#### **DualSlash** (Medium)
- **Type:** Twin arc slashes
- **Pattern:** X-pattern beam attacks

#### **LightningStorm** (Large)
- **Type:** Fractal lightning strikes
- **VFX:** MagnumVFX.DrawSwanLakeLightning

#### **MonochromaticApocalypse** (Ultimate)
- **Type:** Rotating laser beam
- **Pattern:** Full 360° rotation, must dodge through gaps

#### **SwanSerenade** (Spectacle) ⭐ SIGNATURE ATTACK
- **Type:** Hero's Judgment style with rainbow theme
- **Telegraph:** Converging rainbow particles + safe zone
- **VFX:** Prismatic sparkles, rainbow halos

#### **FractalLaser** (Phase 2)
- **Type:** Crossing laser beam grid
- **Telegraph:** Rainbow warning lines
- **Pattern:** Multiple intersecting beams

#### **ChromaticSurge** (Phase 3)
- **Type:** Rainbow electrical pulses
- **Telegraph:** Rainbow electrical buildup
- **Pattern:** Expanding chromatic waves

### Ambient VFX System

Swan Lake has the most elaborate ambient particle system:

```csharp
private void SpawnAmbientParticles()
{
    // 1. FEATHER WALTZ - Orbiting feathers in spiral
    // 2. PRISMATIC LIGHT STREAMS - Rainbow bleeding through
    // 3. MONOCHROME CONTRAST RIPPLES - Black/white alternating
    // 4. GRACEFUL PARTICLE CURRENTS - Figure-8 patterns
    // 5. ETHEREAL MIST - Stage fog effect
    // 6. BALLET SPOTLIGHT EFFECTS - Following spotlights
    // 7. DYING SWAN TEARS - Crystalline tears in final phase
    // 8. MOOD-SPECIFIC EFFECTS - Different per mood
}
```

---

## 🕷️ ENIGMA, THE HOLLOW MYSTERY

**File:** `Content/EnigmaVariations/Bosses/EnigmaTheHollowMystery.cs`  
**HP:** 380,000 | **Defense:** 65 | **Contact Damage:** 100  
**Theme:** Void mystery, purple → green gradient, watching eyes

### Theme Colors
```csharp
private static readonly Color EnigmaBlack = new Color(15, 10, 20);
private static readonly Color EnigmaPurple = new Color(140, 60, 200);
private static readonly Color EnigmaDeepPurple = new Color(80, 20, 120);
private static readonly Color EnigmaGreen = new Color(50, 220, 100);
private static readonly Color EnigmaVoid = new Color(30, 15, 40);
```

### Unique Mechanics

**Teleportation System:**
```csharp
private void AI_Teleport(Player target)
{
    if (isFading) // Departure
    {
        NPC.alpha = Math.Min(255, NPC.alpha + 12);
        CustomParticles.GlyphBurst(NPC.Center, EnigmaPurple, 6, 4f);
    }
    else // Arrival
    {
        NPC.alpha = Math.Max(0, NPC.alpha - 15);
        CustomParticles.EnigmaEyeGaze(eyePos, EnigmaPurple, 0.45f, target.Center);
    }
}
```

### Attack Breakdown

#### **VoidLunge** (Core)
- **Type:** Teleport + dash attack
- **Pattern:** Multiple teleport-dash sequences

#### **EyeVolley** (Core)
- **Type:** Eye projectile spread
- **VFX:** Enigma eye particles watching player

#### **ParadoxRing** (Core)
- **Type:** Expanding ring with gaps
- **Mechanic:** Player must find gap to escape

#### **ParadoxWeb** (Phase 2) - BULLET HELL
- **Type:** Spider web projectile pattern
- **Pattern:** Concentric web rings

#### **ParadoxJudgment** (Phase 2) ⭐ SIGNATURE ATTACK
- **Type:** Hero's Judgment with void theme
- **Telegraph:** Void convergence + eye formation
- **VFX:** Glyph circles, watching eyes

#### **VoidLaserWeb** (Phase 2)
- **Type:** Spider web laser pattern
- **Telegraph:** Multiple crossing warning lines
- **Pattern:** Web-like intersecting beams

#### **EntropicSurge** (Phase 3)
- **Type:** Void electrical pulses
- **Telegraph:** Purple-green electrical buildup
- **Pattern:** Expanding void energy waves

#### **RealityZones** (Phase 3) - ENVIRONMENT
- **Type:** Arena trap zones
- **Mechanic:** Void zones that damage on contact

---

## 🎯 BOSS VFX OPTIMIZER SYSTEM

**File:** `Common/Systems/BossVFXOptimizer.cs`

### Core Optimization Methods

```csharp
// Frame-skip particle spawning
public static void OptimizedFlare(Vector2 position, Color color, float scale, int lifetime, int frameInterval = 2)
{
    if (Main.GameUpdateCount % (frameInterval * FrameSkipMult) != 0) return;
    CustomParticles.GenericFlare(position, color, scale * QualityMult, lifetime);
}
```

### Warning Type Enum

```csharp
public enum WarningType
{
    Safe,      // Cyan - "Stand here to be safe"
    Caution,   // Yellow - "This area will be dangerous soon"
    Danger,    // Red - "Projectiles incoming on this path"
    Imminent   // White - "Attack is about to hit NOW"
}
```

### Warning Methods

| Method | Purpose | Usage |
|--------|---------|-------|
| `WarningFlare` | Single point indicator | Mark spawn points |
| `WarningLine` | Trajectory line | Show projectile paths |
| `SafeZoneRing` | Safe area circle | Show where to stand |
| `DangerZoneRing` | Danger area circle | Show where NOT to stand |
| `ConvergingWarning` | Charge-up effect | Show attack building |
| `SafeArcIndicator` | Arc gap marker | Show escape route |
| `GroundImpactWarning` | Landing zone | Show slam impacts |
| `LaserBeamWarning` | Beam path | Show laser trajectory |
| `ElectricalBuildupWarning` | Shock warning | Show electrical charge |

### Attack Release Methods

```csharp
// Standard attack release VFX
public static void AttackReleaseBurst(Vector2 center, Color primaryColor, Color secondaryColor, float scale = 1f)
{
    CustomParticles.GenericFlare(center, Color.White, 1.2f * scale, 20);
    CustomParticles.GenericFlare(center, primaryColor, 0.9f * scale, 18);
    // + halos, sparks, etc.
}
```

---

## 📐 ATTACK DESIGN PATTERNS

### Pattern 1: Telegraph → Execute → Recovery

```csharp
if (SubPhase == 0) // Telegraph
{
    // Show warnings
    BossVFXOptimizer.WarningLine(...);
    BossVFXOptimizer.ConvergingWarning(...);
    
    if (Timer >= telegraphTime)
    {
        Timer = 0;
        SubPhase = 1;
    }
}
else if (SubPhase == 1) // Execute
{
    // Do the attack
    NPC.velocity = dashDirection * speed;
    SpawnProjectiles();
    
    if (Timer >= executionTime)
    {
        Timer = 0;
        SubPhase = 2;
    }
}
else // Recovery
{
    NPC.velocity *= 0.9f;
    if (Timer >= recoveryTime)
        EndAttack();
}
```

### Pattern 2: Multi-Wave Attack

```csharp
if (SubPhase < waveCount)
{
    if (Timer == chargeTime)
    {
        SpawnProjectileWave(SubPhase);
        BossVFXOptimizer.AttackReleaseBurst(...);
    }
    
    if (Timer >= waveDelay)
    {
        Timer = 0;
        SubPhase++;
    }
}
else
{
    if (Timer >= cooldown)
        EndAttack();
}
```

### Pattern 3: Safe Arc Radial Burst

```csharp
// Calculate safe direction
float safeAngle = (target.Center - NPC.Center).ToRotation();
float safeArc = MathHelper.ToRadians(25f);

// Spawn projectiles with gap
for (int i = 0; i < projectileCount; i++)
{
    float angle = MathHelper.TwoPi * i / projectileCount;
    float angleDiff = MathHelper.WrapAngle(angle - safeAngle);
    
    if (Math.Abs(angleDiff) < safeArc) continue; // Skip safe zone
    
    Vector2 vel = angle.ToRotationVector2() * speed;
    BossProjectileHelper.SpawnHostileOrb(NPC.Center, vel, damage, color, homing);
}

// Show safe arc warning
BossVFXOptimizer.SafeArcIndicator(NPC.Center, safeAngle, safeArc * 2f, 150f, 6);
```

---

## 🎨 VFX LAYERING TECHNIQUE

### Standard Impact VFX Stack

```csharp
// Layer 1: White core flash (brightest)
CustomParticles.GenericFlare(pos, Color.White, 1.2f, 25);

// Layer 2: Primary theme color
CustomParticles.GenericFlare(pos, primaryColor, 0.9f, 22);

// Layer 3: Secondary theme color
CustomParticles.GenericFlare(pos, secondaryColor, 0.7f, 20);

// Layer 4: Cascading halos
for (int i = 0; i < 6; i++)
{
    Color ringColor = Color.Lerp(primaryColor, secondaryColor, i / 6f);
    CustomParticles.HaloRing(pos, ringColor, 0.3f + i * 0.1f, 15 + i * 2);
}

// Layer 5: Theme particles (sakura, feathers, etc.)
ThemedParticles.SakuraPetals(pos, 15, 60f);
```

---

## 🔧 PROJECTILE HELPER METHODS

**File:** `Common/Systems/AggressiveBossProjectiles.cs`

| Method | Description | Best For |
|--------|-------------|----------|
| `SpawnHostileOrb` | Basic homing projectile | Tracking pressure |
| `SpawnAcceleratingBolt` | Speeds up over time | Punishing hesitation |
| `SpawnWaveProjectile` | Sinusoidal movement | Pattern variety |
| `SpawnExplosiveOrb` | Explodes on timer | Area denial |
| `SpawnDelayedDetonation` | Stationary then explodes | Zone control |
| `SpawnBoomerang` | Returns to boss | Coverage attacks |

```csharp
// Usage examples:
BossProjectileHelper.SpawnHostileOrb(pos, vel, 80, color, 0.03f); // 3% homing
BossProjectileHelper.SpawnAcceleratingBolt(pos, vel, 80, color, 15f); // 15 acceleration
BossProjectileHelper.SpawnWaveProjectile(pos, vel, 80, color, 4f); // 4 amplitude
```

---

## ✅ BOSS IMPLEMENTATION CHECKLIST

### Required Components:
- [ ] Theme color palette defined
- [ ] BossPhase enum with all states
- [ ] AttackPattern enum with all attacks
- [ ] Difficulty tier scaling (3 tiers)
- [ ] Health bar registration
- [ ] Music track assignment
- [ ] Bestiary entry
- [ ] Debuff immunities

### Required Per Attack:
- [ ] Warning/telegraph phase (minimum 20 frames)
- [ ] Clear visual warning indicators
- [ ] Projectile variety (mix 2+ types)
- [ ] Safe zone/escape route
- [ ] Attack release VFX burst
- [ ] Recovery window for player

### Performance Requirements:
- [ ] Use BossVFXOptimizer methods
- [ ] Frame-skip particle spawning
- [ ] Quality scaling under load
- [ ] Maximum 50 projectiles per wave
