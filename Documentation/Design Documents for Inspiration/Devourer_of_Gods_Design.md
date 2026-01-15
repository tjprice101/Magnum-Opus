# Devourer of Gods - Boss Design Document

## Overview

The Devourer of Gods (DoG) is Calamity's iconic cosmic worm boss featuring:
- **Multi-segment worm architecture** with Head, Body, and Tail
- **Portal teleportation system** between phases
- **Laser wall attacks** with complex geometric patterns
- **Cosmic Guardian minions** spawned at phase transitions
- **Phase-based difficulty scaling** with unique attack patterns per phase

---

## Worm Segment Architecture

### Segment Linking System
```csharp
// Head controls the worm, stores segment references
NPC.ai[0] = state/timer
NPC.ai[1] = previous segment whoAmI
NPC.ai[2] = head whoAmI (for body/tail to reference)
NPC.ai[3] = next segment whoAmI

// Body/Tail follow the segment ahead of them
public ref float AheadSegmentIndex => ref NPC.ai[1];
public ref float HeadIndex => ref NPC.ai[2];
```

### Segment Communication Pattern
```csharp
// Body and Tail segments access head state via:
NPC head = Main.npc[(int)HeadIndex];
if (head.ai[0] == someState) { /* react accordingly */ }

// Segments follow with smooth interpolation:
Vector2 targetPos = aheadSegment.Center;
float distance = Vector2.Distance(NPC.Center, targetPos);
NPC.velocity = Vector2.Normalize(targetPos - NPC.Center) * Math.Min(distance, speed);
```

### Segment Damage Resistance
- **Head**: Takes full damage, main target
- **Body**: Reduced damage (typically 50-80%), provides lasers
- **Tail**: High damage resistance, portal entry point

---

## Phase System

### Phase Thresholds (lifeRatio based)
```csharp
// Phase transition points
bool phase2 = lifeRatio < 0.6f;   // 60% HP
bool phase4 = lifeRatio < 0.5f;   // 50% HP  
bool phase5 = lifeRatio < 0.4f;   // 40% HP - Cosmic Guardians spawn
bool phase6 = lifeRatio < 0.2f;   // 20% HP - Intense phase
bool phase7 = lifeRatio < 0.15f;  // 15% HP - Final stand
```

### Phase 2 Portal Transition
```csharp
// DoG enters portal for dramatic phase transition
bool AttemptingToEnterPortal;
bool AwaitingPhase2Teleport;

// Portal entry sequence:
// 1. Head moves toward portal position
// 2. Segments follow, disappearing into portal
// 3. Screen effect (flash, shake)
// 4. Boss emerges at new location with enhanced abilities
```

---

## Laser Wall Attack System

### Laser Wall Types (Phase 1)
```csharp
public enum LaserWallType
{
    DiagonalRight,      // Diagonal pattern slanting right
    DiagonalLeft,       // Diagonal pattern slanting left
    DiagonalHorizontal, // Horizontal sweep
    DiagonalCross       // X-shaped pattern (most dangerous)
}
```

### Laser Wall Types (Phase 2 - Enhanced)
```csharp
public enum LaserWallType_Phase2
{
    Normal,             // Standard wall pattern
    Offset,             // Staggered positions
    DiagonalHorizontal, // Horizontal with diagonal elements
    MultiLayered,       // Multiple overlapping walls
    DiagonalVertical    // Vertical sweep pattern
}
```

### Laser Wall Phase State Machine
```csharp
public enum LaserWallPhase
{
    SetUp = 0,          // Preparing laser positions
    FireLaserWalls = 1, // Lasers actively firing
    End = 2             // Cleanup phase
}
```

### Laser Wall Configuration
```csharp
// Phase 2 laser wall parameters
const int shotSpacingMax_Phase2 = 1470;  // Maximum spacing between laser shots
const int spacingVar_Phase2 = 105;        // Spacing variation
const int totalShots_Phase2 = 28;         // Total lasers per wall

// Spawn pattern:
Vector2 spawnOffset = new Vector2(1200f, 0); // Spawn 1200 units from player
Vector2 laserSpawnPos = player.Center + spawnOffset.RotatedBy(wallAngle);
```

### Laser Wall Spawning Pattern
```csharp
// Create a wall of lasers perpendicular to player
for (int i = 0; i < totalShots; i++)
{
    float spacing = baseSpacing + Main.rand.NextFloat(-spacingVar, spacingVar);
    Vector2 laserPos = wallStart + wallDirection * (i * spacing);
    Vector2 laserVelocity = (player.Center - laserPos).SafeNormalize(Vector2.Zero) * laserSpeed;
    
    Projectile.NewProjectile(source, laserPos, laserVelocity, laserType, damage, 0f);
}
```

---

## Body Segment Laser Barrage

### Segment-Based Laser Firing
```csharp
// Every Nth segment fires lasers during certain phases
int segmentIndex = GetSegmentIndex();
if (segmentIndex % 15 == 0 && !InLaserWallPhase)
{
    // Fire laser at player with slight spread
    Vector2 toPlayer = (player.Center - NPC.Center).SafeNormalize(Vector2.Zero);
    Vector2 laserVel = toPlayer.RotatedByRandom(0.1f) * laserSpeed;
    
    Projectile.NewProjectile(source, NPC.Center, laserVel, laserType, damage, 0f);
}
```

### Laser Barrage Timing
```csharp
// Control barrage rate with cooldown
ref float laserCooldown => ref NPC.localAI[0];
laserCooldown++;
if (laserCooldown >= laserDelay)
{
    laserCooldown = 0f;
    FireLaserBarrage();
}
```

---

## Cosmic Guardian Minions

### Guardian Spawning
```csharp
// Spawn Cosmic Guardians at phase thresholds
if (phase5 && !spawnedGuardians)
{
    for (int i = 0; i < guardianCount; i++)
    {
        float angle = MathHelper.TwoPi * i / guardianCount;
        Vector2 spawnPos = NPC.Center + angle.ToRotationVector2() * spawnRadius;
        
        // Spawn guardian head - it creates its own body/tail
        NPC.NewNPC(source, (int)spawnPos.X, (int)spawnPos.Y, CosmicGuardianHead);
    }
    spawnedGuardians = true;
}
```

### Guardian Behavior
- Smaller worm segments that orbit/protect DoG
- Independent AI but coordinate with main boss
- Must be killed to reduce DoG's defenses or progress fight

---

## Movement Patterns

### Ground Phase Movement
```csharp
// When near ground, use burrowing behavior
if (NearGround)
{
    // Sine wave burrowing pattern
    float burrowAngle = Main.GameUpdateCount * burrowFrequency;
    Vector2 burrowOffset = new Vector2(0, (float)Math.Sin(burrowAngle) * burrowAmplitude);
    targetPosition += burrowOffset;
}
```

### Flying Phase Movement
```csharp
// Aggressive pursuit in open air
Vector2 toPlayer = player.Center - NPC.Center;
float distance = toPlayer.Length();

if (distance > chaseRange)
{
    // Fast approach
    NPC.velocity = toPlayer.SafeNormalize(Vector2.Zero) * maxSpeed;
}
else
{
    // Circle player with varying radius
    float orbitAngle = Main.GameUpdateCount * orbitSpeed;
    Vector2 orbitPos = player.Center + orbitAngle.ToRotationVector2() * orbitRadius;
    NPC.velocity = (orbitPos - NPC.Center) * acceleration;
}
```

### Charge Attack Pattern
```csharp
// Telegraph charge with warning
if (chargeTelegraphTimer > 0)
{
    chargeTelegraphTimer--;
    // Visual telegraph - glow, particles, sound cue
    SpawnTelegraphParticles();
}
else if (isCharging)
{
    // Commit to charge direction
    NPC.velocity = chargeDirection * chargeSpeed;
    
    // Spawn trail particles during charge
    SpawnChargeTrail();
}
```

---

## Visual Effects Inspiration

### Cosmic Dust Trail
```csharp
// Segments leave cosmic particle trails
if (Main.rand.NextBool(3))
{
    Dust cosmic = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, 
        DustID.PurpleTorch, 0f, 0f, 100, default, 1.5f);
    cosmic.noGravity = true;
    cosmic.velocity = -NPC.velocity * 0.1f;
}
```

### Portal VFX
```csharp
// Portal entrance effect
void PortalEntranceVFX(Vector2 portalCenter)
{
    // Swirling particle vortex
    for (int i = 0; i < 30; i++)
    {
        float angle = MathHelper.TwoPi * i / 30f + Main.GameUpdateCount * 0.1f;
        float radius = 50f + (float)Math.Sin(Main.GameUpdateCount * 0.2f) * 20f;
        Vector2 pos = portalCenter + angle.ToRotationVector2() * radius;
        
        // Cosmic colored particles spiraling inward
        Dust spiral = Dust.NewDustPerfect(pos, cosmicDustType, 
            (portalCenter - pos).SafeNormalize(Vector2.Zero) * 3f);
        spiral.noGravity = true;
        spiral.scale = 1.5f;
    }
    
    // Central flash
    CustomParticles.GenericFlare(portalCenter, Color.Cyan, 1.5f, 20);
}
```

### Laser Beam Effects
```csharp
// Laser telegraph before firing
void LaserTelegraph(Vector2 start, Vector2 end)
{
    // Thin warning line
    DrawLaserLine(start, end, Color.Red * 0.5f, 2f);
    
    // Particles along trajectory
    Vector2 direction = (end - start).SafeNormalize(Vector2.Zero);
    float length = Vector2.Distance(start, end);
    
    for (float i = 0; i < length; i += 50f)
    {
        Vector2 pos = start + direction * i;
        CustomParticles.GenericGlow(pos, Vector2.Zero, Color.Red * 0.3f, 0.2f, 10, true);
    }
}
```

---

## Death Animation

### Phase 1: Building Intensity
```csharp
void DeathAnimation_Buildup(int timer)
{
    float intensity = timer / 120f;
    
    // Increasing screen shake
    MagnumScreenEffects.AddScreenShake(intensity * 10f);
    
    // Particles emanating from all segments
    foreach (var segment in AllSegments)
    {
        if (Main.rand.NextBool(3))
        {
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f;
                Vector2 vel = angle.ToRotationVector2() * (intensity * 8f);
                CustomParticles.GenericFlare(segment.Center, Color.Cyan, 0.5f + intensity * 0.5f, 20);
            }
        }
    }
}
```

### Phase 2: Climax
```csharp
void DeathAnimation_Climax()
{
    // Sky flash effect
    ScreenEffects.TriggerFlash(Color.Cyan, 1f);
    
    // Massive explosion at each segment
    foreach (var segment in AllSegments)
    {
        CustomParticles.ExplosionBurst(segment.Center, Color.Cyan, 30, 15f);
        CustomParticles.HaloRing(segment.Center, Color.White, 1.5f, 30);
        
        // Cosmic dust explosion
        for (int i = 0; i < 50; i++)
        {
            Dust cosmic = Dust.NewDustDirect(segment.position, segment.width, segment.height,
                cosmicDustType, 0f, 0f, 0, default, 2f);
            cosmic.velocity = Main.rand.NextVector2CircularEdge(10f, 10f);
            cosmic.noGravity = true;
        }
    }
    
    // Screen shake
    MagnumScreenEffects.AddScreenShake(20f);
}
```

---

## Implementation Tips for MagnumOpus

### Adapting DoG Concepts
1. **Laser Walls** → Musical note walls that sweep across screen
2. **Portal Teleport** → Dramatic phase transition with theme-appropriate VFX
3. **Cosmic Guardians** → Themed minion sub-bosses matching your musical score
4. **Segment Architecture** → Multi-part bosses with linked segments

### Musical Theme Integration
- Replace cosmic/purple particles with theme-appropriate colors
- Laser walls become walls of music notes or theme symbols
- Portal effects use theme's color palette
- Death animation becomes a dramatic musical crescendo

### Key Takeaways
1. **Phased Difficulty**: Use lifeRatio thresholds for distinct battle phases
2. **Geometric Attacks**: Walls, circles, crosses create visually interesting dodging
3. **Multi-Entity Coordination**: Segments and minions working together
4. **Dramatic Transitions**: Portals/teleports for memorable phase changes
5. **Escalating Intensity**: Each phase more visually spectacular than the last
