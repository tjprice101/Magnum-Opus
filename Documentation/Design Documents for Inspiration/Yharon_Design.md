# Yharon - Boss Design Document

## Overview

Yharon, Dragon of Rebirth is Calamity's signature dragon boss featuring:
- **Dual AI System** with distinct phase 1 and phase 2+ behaviors
- **Complex Attack Type Switch** with 8+ distinct attack patterns
- **Enrage/Arena System** enforcing player positioning
- **Fire-themed VFX** including fireballs, tornadoes, and infernal dust
- **Phase Progression** with escalating attack complexity

---

## Dual AI Architecture

### Two-Method AI System
```csharp
public override void AI()
{
    if (lifeRatio > phase2GateValue)
    {
        Yharon_AI1();  // Phase 1 behavior
    }
    else
    {
        Yharon_AI2();  // Phase 2+ behavior
    }
}
```

### Phase Gate Values (Revenge Mode)
```csharp
// Phase transition thresholds
float phase2GateValue = 0.44f;   // 44% HP - Transition to Phase 2
float phase3GateValue = 0.275f;  // 27.5% HP - Enhanced attacks
float phase4GateValue = 0.165f;  // 16.5% HP (Death Mode) - Final phase
```

### Secondary Phase System
```csharp
// Within Phase 2, sub-phases control attack patterns
int secondPhasePhase = 1; // Ranges from 1-4
// Phase 1: Basic attack rotation
// Phase 2: Adds fast charges
// Phase 3: Adds tornado attacks
// Phase 4: Maximum intensity, all attacks
```

---

## AI State Variables

### Core AI Slots
```csharp
NPC.ai[0] // Current attack state (0-17+)
NPC.ai[1] // Timer within current attack
NPC.ai[2] // Secondary timer / sub-state
NPC.ai[3] // Attack sequence counter / combo tracker

NPC.localAI[0] // Local timer
NPC.localAI[1] // Charge telegraph timer (fastChargeTelegraphTime = 120)
NPC.localAI[2] // Misc state flags
```

### Attack State Machine
```csharp
// Attack types via ai[0]
switch ((int)NPC.ai[0])
{
    case 0: HoverPhase(); break;
    case 1: TransitionState(); break;
    case 2: ChargeAttack(); break;
    case 3: FireballBreath(); break;
    case 4: FireballBarrage(); break;
    case 5: FireCircleAttack(); break;
    case 6: TornadoSpawn(); break;
    case 7: FastChargeAttack(); break;
    case 8: TeleportAttack(); break;
    case 9: SplittingFireball(); break;
    // ... additional states
}
```

---

## Attack Patterns

### Charge Attack (State 2)
```csharp
void ChargeAttack()
{
    if (NPC.ai[1] == 0f)
    {
        // Telegraph phase - face player, build up
        NPC.rotation = (player.Center - NPC.Center).ToRotation();
        ChargeDust();  // Trail particles during telegraph
    }
    
    NPC.ai[1]++;
    
    if (NPC.ai[1] >= chargeDelay)
    {
        // Execute charge
        NPC.velocity = chargeDirection * chargeSpeed;
        
        // Spawn fire trail
        SpawnChargeTrailParticles();
    }
    
    if (NPC.ai[1] >= chargeDuration)
    {
        NPC.ai[0] = 0f;  // Return to hover
        NPC.ai[1] = 0f;
    }
}
```

### Fireball Breath (States 3/4)
```csharp
void FireballBreath()
{
    // Rapid-fire fireballs while hovering
    NPC.ai[1]++;
    
    if (NPC.ai[1] % fireballInterval == 0)
    {
        Vector2 toPlayer = (player.Center - NPC.Center).SafeNormalize(Vector2.Zero);
        Vector2 fireballVel = toPlayer.RotatedByRandom(spreadAngle) * fireballSpeed;
        
        // YharonFireball projectile
        Projectile.NewProjectile(source, NPC.Center, fireballVel, 
            ModContent.ProjectileType<YharonFireball>(), damage, 0f);
        
        // Muzzle flash VFX
        SpawnFireballMuzzleFlash();
    }
    
    if (NPC.ai[1] >= breathDuration)
    {
        NPC.ai[0] = 0f;
        NPC.ai[1] = 0f;
    }
}
```

### Fire Circle Attack (State 5)
```csharp
void FireCircleAttack()
{
    // Spin and release fireballs in circular pattern
    NPC.rotation += spinSpeed;
    NPC.ai[1]++;
    
    if (NPC.ai[1] % circleFireInterval == 0)
    {
        int projectileCount = 8;
        for (int i = 0; i < projectileCount; i++)
        {
            float angle = MathHelper.TwoPi * i / projectileCount + NPC.rotation;
            Vector2 velocity = angle.ToRotationVector2() * circleFireSpeed;
            
            Projectile.NewProjectile(source, NPC.Center, velocity,
                ModContent.ProjectileType<FlareBomb>(), damage, 0f);
        }
    }
}
```

### Tornado Attack (State 6 - "Infernado")
```csharp
void TornadoSpawn()
{
    // Spawn fire tornado at player position
    if (NPC.ai[1] == 0f)
    {
        // Telegraph - mark spawn location
        Vector2 tornadoPos = player.Center;
        SpawnTornadoTelegraph(tornadoPos);
    }
    
    NPC.ai[1]++;
    
    if (NPC.ai[1] == tornadoDelay)
    {
        // Spawn Infernado2 projectile
        Projectile.NewProjectile(source, tornadoSpawnPos, Vector2.Zero,
            ModContent.ProjectileType<Infernado2>(), tornadoDamage, 0f);
    }
}
```

### Fast Charge Attack (State 7)
```csharp
void FastChargeAttack()
{
    // Telegraphed dash with increased speed
    int fastChargeTelegraphTime = 120; // Adjustable with protectionBoost
    
    if (NPC.localAI[1] < fastChargeTelegraphTime)
    {
        // Telegraph phase - glow, particles, targeting line
        NPC.localAI[1]++;
        
        // Draw line to player
        DrawChargeTelegraphLine();
        
        // Intensifying glow
        float intensity = NPC.localAI[1] / fastChargeTelegraphTime;
        SpawnTelegraphGlow(intensity);
    }
    else
    {
        // Execute fast charge
        NPC.velocity = (player.Center - NPC.Center).SafeNormalize(Vector2.Zero) * fastChargeSpeed;
        SpawnFastChargeTrail();
    }
}
```

### Teleport Attack (State 8)
```csharp
void TeleportAttack()
{
    NPC.ai[1]++;
    
    if (NPC.ai[1] == teleportWarning)
    {
        // Fade out VFX
        SpawnTeleportDepartureVFX();
        NPC.alpha = 255;  // Invisible
    }
    
    if (NPC.ai[1] == teleportMoment)
    {
        // Teleport to new position
        Vector2 newPos = player.Center + teleportOffset;
        NPC.Center = newPos;
        
        // Fade in VFX
        SpawnTeleportArrivalVFX();
    }
    
    if (NPC.ai[1] >= teleportDuration)
    {
        NPC.alpha = 0;  // Visible again
        NPC.ai[0] = 0f;
        NPC.ai[1] = 0f;
    }
}
```

### Splitting Fireball (State 9+)
```csharp
void SplittingFireball()
{
    if (NPC.ai[1] == fireDelay)
    {
        // Fire large fireball that splits on timer/impact
        Vector2 toPlayer = (player.Center - NPC.Center).SafeNormalize(Vector2.Zero);
        
        // Main fireball that will split
        Projectile.NewProjectile(source, NPC.Center, toPlayer * fireballSpeed,
            ModContent.ProjectileType<SplittingYharonFireball>(), damage, 0f);
    }
}

// In SplittingYharonFireball.cs:
public override void OnKill(int timeLeft)
{
    // Split into smaller fireballs
    for (int i = 0; i < splitCount; i++)
    {
        float angle = MathHelper.TwoPi * i / splitCount;
        Vector2 splitVel = angle.ToRotationVector2() * splitSpeed;
        
        Projectile.NewProjectile(source, Projectile.Center, splitVel,
            ModContent.ProjectileType<SmallYharonFireball>(), smallDamage, 0f);
    }
}
```

---

## Enrage System

### Arena Boundary Check
```csharp
// Define safe zone around arena
Rectangle safeBox = new Rectangle(arenaX, arenaY, arenaWidth, arenaHeight);

// Check if player is outside
bool playerOutside = !safeBox.Contains(player.Center.ToPoint());

if (playerOutside)
{
    enraged = true;
    ApplyEnrageEffects();
}
else
{
    enraged = false;
}
```

### Enrage Effects
```csharp
void ApplyEnrageEffects()
{
    // Damage boost
    protectionBoost = 1.5f;  // Affects various timings
    
    // Speed increase
    moveSpeed *= 1.3f;
    chargeSpeed *= 1.4f;
    
    // Visual indicator - fire aura intensifies
    SpawnEnrageAura();
    
    // More aggressive attack selection
    attackCooldown *= 0.7f;
}
```

---

## Visual Effects

### Charge Trail Dust
```csharp
void ChargeDust()
{
    // Fire trail during charges
    for (int i = 0; i < 5; i++)
    {
        Vector2 dustPos = NPC.Center + Main.rand.NextVector2Circular(NPC.width / 2, NPC.height / 2);
        Vector2 dustVel = -NPC.velocity * 0.3f + Main.rand.NextVector2Circular(2f, 2f);
        
        Dust fire = Dust.NewDustDirect(dustPos, 0, 0, DustID.Torch, dustVel.X, dustVel.Y);
        fire.noGravity = true;
        fire.scale = Main.rand.NextFloat(1.5f, 2.5f);
        fire.color = Color.Lerp(Color.Orange, Color.Yellow, Main.rand.NextFloat());
    }
}
```

### Flare Dust Bullet Hell
```csharp
// FlareDust projectile - creates dense bullet patterns
void SpawnFlareDustPattern()
{
    int dustCount = 20;
    for (int i = 0; i < dustCount; i++)
    {
        float angle = MathHelper.TwoPi * i / dustCount + rotationOffset;
        Vector2 velocity = angle.ToRotationVector2() * flareSpeed;
        
        // Stagger speeds for spiral effect
        velocity *= 1f + (i % 3) * 0.15f;
        
        Projectile.NewProjectile(source, NPC.Center, velocity,
            ModContent.ProjectileType<FlareDust>(), flareDamage, 0f);
    }
}
```

### Big Flare Visual Effects
```csharp
// BigFlare and BigFlare2 - dramatic visual attacks
void SpawnBigFlare()
{
    // Large central explosion
    CustomParticles.ExplosionBurst(NPC.Center, Color.OrangeRed, 50, 20f);
    
    // Expanding halo rings
    for (int i = 0; i < 5; i++)
    {
        int delay = i * 3;
        float scale = 0.5f + i * 0.3f;
        CustomParticles.HaloRing(NPC.Center, Color.Orange, scale, 25 + delay);
    }
    
    // Screen shake
    MagnumScreenEffects.AddScreenShake(15f);
    
    // Sky flash
    ScreenEffects.TriggerFlash(Color.Orange, 0.8f);
}
```

### Fire Aura Effect
```csharp
void SpawnFireAura()
{
    // Constant fire particles around boss
    if (Main.GameUpdateCount % 2 == 0)
    {
        for (int i = 0; i < 4; i++)
        {
            float angle = MathHelper.TwoPi * i / 4f + Main.GameUpdateCount * 0.05f;
            float radius = auraRadius + (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 20f;
            Vector2 particlePos = NPC.Center + angle.ToRotationVector2() * radius;
            
            Color flameColor = Color.Lerp(Color.Yellow, Color.OrangeRed, Main.rand.NextFloat());
            CustomParticles.GenericFlare(particlePos, flameColor, 0.4f, 15);
        }
    }
}
```

---

## Projectile Types Summary

| Projectile | Description | Behavior |
|------------|-------------|----------|
| `YharonFireball` | Standard fireball | Travels straight, explodes on contact |
| `FlareDust` | Small fire particle | Bullet hell patterns, short lived |
| `FlareBomb` | Explosive fire orb | Explodes after delay or on contact |
| `Infernado2` | Fire tornado | Stationary, damages on contact |
| `BigFlare` | Visual explosion | Major attack punctuation |
| `BigFlare2` | Enhanced flare | Phase transition effect |

---

## Phase Transition Effects

### Phase 2 Transition
```csharp
void Phase2Transition()
{
    // Dramatic pause
    NPC.velocity = Vector2.Zero;
    
    // Roar sound
    SoundEngine.PlaySound(YharonRoarSound, NPC.Center);
    
    // Fire explosion outward
    for (int i = 0; i < 16; i++)
    {
        float angle = MathHelper.TwoPi * i / 16f;
        Vector2 velocity = angle.ToRotationVector2() * 15f;
        
        Projectile.NewProjectile(source, NPC.Center, velocity,
            ModContent.ProjectileType<BigFlare>(), 0, 0f);
    }
    
    // Screen effects
    MagnumScreenEffects.AddScreenShake(20f);
    ScreenEffects.TriggerFlash(Color.OrangeRed, 1f);
    
    // Music transition (Calamity feature)
    // Switch to phase 2 music track
}
```

---

## Implementation Tips for MagnumOpus

### Adapting Yharon Concepts
1. **Fire Theme** → Adapt to your musical theme's color palette
2. **Tornado Attack** → Musical whirlwind with notes/symbols
3. **Charge Telegraph** → Theme-appropriate warning indicator
4. **Enrage System** → Arena enforcement with visual feedback

### Attack Pattern Variety
- Mix ranged (fireballs) and melee (charges) attacks
- Include telegraphed (fast charge) and instant (basic charge) variants
- Use bullet hell patterns (FlareDust circles) for intensity spikes
- Create environmental hazards (Infernado) for positioning pressure

### Visual Feedback Priorities
1. **Telegraph Attacks**: Always give clear warning before dangerous moves
2. **Phase Indicators**: Visual change when entering new phases
3. **Enrage Warning**: Obvious visual when player leaves arena
4. **Hit Confirmation**: Satisfying VFX when attacks connect

### Key Takeaways
1. **Dual AI Methods**: Separate logic for major phase transitions
2. **Sub-Phase System**: Gradual difficulty increase within phases
3. **Attack State Machine**: Clean switch-based attack selection
4. **Enrage Boundary**: Soft arena enforcement with consequences
5. **Visual Intensity**: Effects scale with attack danger level
