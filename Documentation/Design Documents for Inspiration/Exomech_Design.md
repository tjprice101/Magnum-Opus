# Exo Mechs - Multi-Boss Design Document

## Overview

The Exo Mechs are Calamity's ultimate endgame multi-boss encounter featuring:
- **Four Coordinated Mechs**: Ares, Apollo, Artemis, and Thanatos
- **Phase-Based Mech Activation**: Only some mechs active at once
- **Secondary Phase State Machine**: Passive/Active states
- **Berserk Mode**: Final mech goes all-out when others die
- **Exo Palette Color Cycling**: Signature visual identity
- **Draedon Controller**: NPC that manages spawning and dialogue

---

## Multi-Mech Coordination System

### Global Mech Indices
```csharp
// CalamityGlobalNPC stores indices for cross-mech reference
public static int draedonExoMechPrime = -1;      // Ares (mechanical arms boss)
public static int draedonExoMechWorm = -1;       // Thanatos (worm mech)
public static int draedonExoMechTwinGreen = -1;  // Apollo (green twin)
public static int draedonExoMechTwinRed = -1;    // Artemis (red twin)

// Access other mechs:
NPC ares = Main.npc[CalamityGlobalNPC.draedonExoMechPrime];
NPC thanatos = Main.npc[CalamityGlobalNPC.draedonExoMechWorm];
```

### Mech Activity Check
```csharp
int GetActiveExoMechCount()
{
    int count = 0;
    if (CalamityGlobalNPC.draedonExoMechPrime != -1 && Main.npc[CalamityGlobalNPC.draedonExoMechPrime].active)
        count++;
    if (CalamityGlobalNPC.draedonExoMechWorm != -1 && Main.npc[CalamityGlobalNPC.draedonExoMechWorm].active)
        count++;
    if (CalamityGlobalNPC.draedonExoMechTwinGreen != -1 && Main.npc[CalamityGlobalNPC.draedonExoMechTwinGreen].active)
        count++;
    // Apollo and Artemis count as one "unit"
    return count;
}
```

---

## Secondary Phase State Machine

### Phase States
```csharp
public enum SecondaryPhase
{
    Nothing = 0,        // Normal attack behavior
    Passive = 1,        // Reduced aggression, following
    PassiveAndImmune = 2 // Fully passive, cannot be damaged
}

// Stored in NPC.newAI[1]
public SecondaryPhase CurrentSecondaryPhase 
{
    get => (SecondaryPhase)NPC.newAI[1];
    set => NPC.newAI[1] = (float)value;
}
```

### Phase Transition Logic
```csharp
void UpdateSecondaryPhase()
{
    float myLifeRatio = NPC.life / (float)NPC.lifeMax;
    
    // Check if another mech is below 70% HP
    bool otherMechLowHP = CheckIfOtherMechBelow(0.7f);
    
    if (otherMechLowHP && myLifeRatio > 0.7f)
    {
        // Go passive while another mech is being focused
        CurrentSecondaryPhase = SecondaryPhase.Passive;
    }
    else if (myLifeRatio < 0.4f || IsLastMechAlive())
    {
        // Berserk mode - fight to the death
        CurrentSecondaryPhase = SecondaryPhase.Nothing;
        EnterBerserkMode();
    }
}
```

### Behavior Modification Based on Phase
```csharp
void AI()
{
    switch (CurrentSecondaryPhase)
    {
        case SecondaryPhase.Nothing:
            FullAggressiveAI();
            break;
            
        case SecondaryPhase.Passive:
            // Reduced attack frequency, stay at distance
            PassiveFollowAI();
            AttackWithReducedFrequency();
            break;
            
        case SecondaryPhase.PassiveAndImmune:
            // Just hover nearby, no attacks
            PassiveFollowAI();
            NPC.dontTakeDamage = true;
            break;
    }
}
```

---

## Berserk Mode

### Trigger Conditions
```csharp
bool ShouldGoBerserk()
{
    float lifeRatio = NPC.life / (float)NPC.lifeMax;
    int otherExoMechsAlive = GetActiveExoMechCount() - 1;
    
    // Berserk if:
    // 1. Below 40% HP, OR
    // 2. Last mech alive and below 70% HP
    return lifeRatio < 0.4f || (otherExoMechsAlive == 0 && lifeRatio < 0.7f);
}
```

### Berserk Effects
```csharp
void EnterBerserkMode()
{
    if (!isBerserk)
    {
        isBerserk = true;
        
        // Visual effect
        SpawnBerserkTransitionVFX();
        
        // Stat changes
        attackSpeedMultiplier = 1.5f;
        movementSpeedMultiplier = 1.3f;
        damageMultiplier = 1.2f;
        
        // Enable all attack patterns
        unlockAllAttacks = true;
        
        // Screen shake
        MagnumScreenEffects.AddScreenShake(15f);
    }
}
```

---

## Individual Mech Designs

### Ares (Mechanical Arms Boss)

#### Structure
```csharp
// AresBody spawns arm weapons
public static void SpawnArms(int bodyWhoAmI)
{
    // Spawn four arms around the body
    NPC.NewNPC(source, spawnX, spawnY, ModContent.NPCType<AresLaserCannon>(), 
        ai0: bodyWhoAmI);  // Left laser
    NPC.NewNPC(source, spawnX, spawnY, ModContent.NPCType<AresTeslaCannon>(), 
        ai0: bodyWhoAmI);  // Right tesla
    NPC.NewNPC(source, spawnX, spawnY, ModContent.NPCType<AresPlasmaFlamethrower>(), 
        ai0: bodyWhoAmI);  // Left plasma
    NPC.NewNPC(source, spawnX, spawnY, ModContent.NPCType<AresGaussNuke>(), 
        ai0: bodyWhoAmI);  // Right nuke
}
```

#### Arm Positioning System
```csharp
// ai[3] controls arm configuration (0-5 different formations)
void UpdateArmPositions()
{
    int formation = (int)NPC.ai[3];
    
    switch (formation)
    {
        case 0: // Default spread
            leftLaserOffset = new Vector2(-100, 50);
            rightTeslaOffset = new Vector2(100, 50);
            leftPlasmaOffset = new Vector2(-80, -30);
            rightNukeOffset = new Vector2(80, -30);
            break;
            
        case 1: // Attack formation
            // Arms point toward player
            break;
            
        case 2: // Defense formation
            // Arms shield body
            break;
            // ... additional formations
    }
}
```

#### Arm Weapon Attacks
```csharp
// AresLaserCannon - Sustained laser beam
void LaserCannonAI()
{
    if (attackPhase == AttackPhase.Charging)
    {
        // Build up energy, telegraph with glow
        chargeProgress++;
        SpawnChargingParticles();
    }
    else if (attackPhase == AttackPhase.Firing)
    {
        // Fire sustained deathray
        if (deathrayProjectile == -1)
        {
            deathrayProjectile = Projectile.NewProjectile(source, 
                NPC.Center, aimDirection, 
                ModContent.ProjectileType<AresDeathray>(), damage, 0f);
        }
        
        // Update deathray rotation to track player
        if (Main.projectile[deathrayProjectile].active)
        {
            Main.projectile[deathrayProjectile].rotation = aimRotation;
        }
    }
}

// AresTeslaCannon - Chain lightning
void TeslaCannonAI()
{
    if (fireTimer >= fireInterval)
    {
        fireTimer = 0;
        
        // Fire tesla orb that chains between enemies
        Projectile.NewProjectile(source, NPC.Center, toPlayer * teslaSpeed,
            ModContent.ProjectileType<AresTeslaOrb>(), damage, 0f);
    }
}

// AresGaussNuke - Homing explosive
void GaussNukeAI()
{
    if (nukeCooldown <= 0)
    {
        nukeCooldown = nukeCooldownMax;
        
        // Fire slow, powerful homing nuke
        Projectile.NewProjectile(source, NPC.Center, toPlayer * nukeSpeed,
            ModContent.ProjectileType<AresGaussNukeProjectile>(), nukeDamage, 0f);
    }
}
```

---

### Apollo & Artemis (The Twins)

#### HP Linking System
```csharp
// Apollo and Artemis share HP - damage one, damage both
void SyncHP()
{
    NPC other = Main.npc[otherTwinIndex];
    
    if (NPC.life > other.life)
    {
        NPC.life = other.life;
    }
}

// Called when either twin takes damage
public override void OnHitByProjectile(Projectile proj, NPC.HitInfo hit, int damageDone)
{
    SyncHP();
}
```

#### Apollo (Green Twin) - Plasma Weapon
```csharp
void ApolloAI()
{
    // Fast, aggressive charges
    if (chargePhase)
    {
        NPC.velocity = chargeDirection * chargeSpeed;
        SpawnPlasmaTrail();
    }
    
    // Plasma cannon fire
    if (fireTimer >= fireInterval)
    {
        Vector2 plasmaVel = toPlayer * plasmaSpeed;
        Projectile.NewProjectile(source, NPC.Center, plasmaVel,
            ModContent.ProjectileType<ApolloPlasma>(), damage, 0f);
    }
}
```

#### Artemis (Red Twin) - Laser Weapon
```csharp
void ArtemisAI()
{
    // More methodical, sustained fire
    if (laserPhase)
    {
        // Fire continuous laser beam
        if (laserProjectile == -1)
        {
            laserProjectile = Projectile.NewProjectile(source, NPC.Center,
                aimDirection, ModContent.ProjectileType<ArtemisLaser>(), damage, 0f);
        }
        
        // Sweep laser toward player
        laserRotation = Utils.AngleLerp(laserRotation, targetRotation, 0.05f);
    }
}
```

#### Twin Coordinated Attacks
```csharp
void TwinSyncAttack()
{
    NPC apollo = Main.npc[apolloIndex];
    NPC artemis = Main.npc[artemisIndex];
    
    // Position on opposite sides of player
    Vector2 apolloTarget = player.Center + new Vector2(-300, 0);
    Vector2 artemisTarget = player.Center + new Vector2(300, 0);
    
    // Simultaneous charge from both sides
    if (syncChargeReady)
    {
        apollo.velocity = (player.Center - apollo.Center).SafeNormalize(Vector2.Zero) * chargeSpeed;
        artemis.velocity = (player.Center - artemis.Center).SafeNormalize(Vector2.Zero) * chargeSpeed;
    }
}
```

---

### Thanatos (Worm Mech)

#### Segment Structure
```csharp
// Similar to DoG - Head, Body, Tail segments
public class ThanatosHead : ModNPC
{
    // Controls worm movement
    // Spawns body segments on initialization
}

public class ThanatosBody : ModNPC
{
    // Opens to fire lasers
    // References head via ai[2]
    
    public bool IsOpen => NPC.ai[3] == 1f;
}

public class ThanatosTail : ModNPC
{
    // End segment, high defense
}
```

#### Segment Opening Mechanic
```csharp
void BodySegmentAI()
{
    // Segments open to attack, closed for defense
    if (headIsAttacking && segmentIndex % 5 == attackWave)
    {
        // Open this segment
        NPC.ai[3] = 1f;  // Open state
        
        // Fire laser when open
        if (openTimer > fireDelay)
        {
            Vector2 laserVel = toPlayer * laserSpeed;
            Projectile.NewProjectile(source, NPC.Center, laserVel,
                ModContent.ProjectileType<ThanatosLaser>(), damage, 0f);
        }
    }
    else
    {
        NPC.ai[3] = 0f;  // Closed state
        // High defense when closed
    }
}
```

#### Deathray Phase
```csharp
void ThanatosDeathrayAttack()
{
    // Head opens for massive sweeping laser
    if (deathrayPhase)
    {
        // Slow rotation deathray
        deathrayRotation += deathrayRotateSpeed;
        
        if (deathrayProjectile == -1)
        {
            deathrayProjectile = Projectile.NewProjectile(source, NPC.Center,
                deathrayRotation.ToRotationVector2(), 
                ModContent.ProjectileType<ThanatosBeam>(), deathrayDamage, 0f);
        }
        
        // Particle effects during deathray
        SpawnDeathrayParticles();
    }
}
```

---

## Exo Mechdusa Fusion Mode

### Fusion Trigger
```csharp
// When all mechs reach critical HP simultaneously, they fuse
void CheckFusionCondition()
{
    if (AllMechsBelowThreshold(0.4f))
    {
        if (!fusionTriggered)
        {
            TriggerExoMechdusa();
        }
    }
}
```

### Fusion Sequence
```csharp
void TriggerExoMechdusa()
{
    fusionTriggered = true;
    
    // Dramatic pause
    foreach (var mech in AllMechs)
    {
        mech.velocity = Vector2.Zero;
        mech.dontTakeDamage = true;
    }
    
    // Converge to center
    Vector2 fusionCenter = GetAverageMechPosition();
    
    // Fusion VFX
    SpawnFusionConvergeParticles(fusionCenter);
    
    // After delay, spawn Exo Mechdusa and despawn individual mechs
    if (fusionTimer >= fusionDelay)
    {
        NPC.NewNPC(source, fusionCenter, ModContent.NPCType<ExoMechdusa>());
        
        foreach (var mech in AllMechs)
        {
            mech.active = false;
        }
    }
}
```

---

## Draedon Controller

### Spawning System
```csharp
// Draedon.cs handles mech spawning and coordination
public static void SummonExoMech(Player player, int mechType)
{
    Vector2 spawnPos;
    
    switch (mechType)
    {
        case 0: // Thanatos
            spawnPos = player.Center + new Vector2(0, 2100);  // Below
            break;
        case 1: // Ares
            spawnPos = player.Center + new Vector2(0, -1400); // Above
            break;
        case 2: // Twins
            Vector2 apolloPos = player.Center + new Vector2(-1100, -1600);
            Vector2 artemisPos = player.Center + new Vector2(1100, -1600);
            NPC.NewNPC(source, apolloPos, ModContent.NPCType<Apollo>());
            NPC.NewNPC(source, artemisPos, ModContent.NPCType<Artemis>());
            return;
    }
    
    NPC.NewNPC(source, spawnPos, GetMechType(mechType));
}
```

### Dialogue System
```csharp
// Draedon provides commentary during fight
void UpdateDialogue()
{
    if (firstMechDefeated && !shownFirstDeathDialogue)
    {
        DisplayDraedonText("Impressive. But the test continues.");
        shownFirstDeathDialogue = true;
    }
    
    if (AllMechsDefeated())
    {
        DisplayDraedonText("Your power exceeds my calculations...");
    }
}
```

---

## Exo Palette Visual System

### Color Palette
```csharp
// CalamityUtils.ExoPalette - cycling rainbow for Exo effects
public static readonly Color[] ExoPalette = new Color[]
{
    Color.Cyan,
    Color.Lime,
    Color.GreenYellow,
    Color.Goldenrod,
    Color.Orange
};

// Get cycling color:
Color GetExoColor(float offset = 0f)
{
    float hue = (Main.GlobalTimeWrappedHourly * 0.5f + offset) % 1f;
    return CalamityUtils.MulticolorLerp(hue, ExoPalette);
}
```

### Particle Effects
```csharp
void SpawnExoParticles()
{
    // Exo-themed spark burst
    for (int i = 0; i < 5; i++)
    {
        Color exoColor = CalamityUtils.MulticolorLerp(Main.rand.NextFloat(), CalamityUtils.ExoPalette);
        Vector2 velocity = Main.rand.NextVector2CircularEdge(5f, 5f);
        
        SparkParticle spark = new SparkParticle(NPC.Center, velocity, false, 30, 1.3f, exoColor);
        GeneralParticleHandler.SpawnParticle(spark);
    }
}
```

### Trail Effects
```csharp
// Exo-themed primitive trail rendering
void DrawExoTrail()
{
    Color mainColor = CalamityUtils.MulticolorLerp(
        (Main.GlobalTimeWrappedHourly * 2f) % 1f, CalamityUtils.ExoPalette);
    Color secondaryColor = CalamityUtils.MulticolorLerp(
        (Main.GlobalTimeWrappedHourly * 2f + 0.2f) % 1f, CalamityUtils.ExoPalette);
    
    GameShaders.Misc["CalamityMod:ExobladePierce"].UseColor(mainColor);
    GameShaders.Misc["CalamityMod:ExobladePierce"].UseSecondaryColor(secondaryColor);
    
    PrimitiveRenderer.RenderTrail(positions, new(WidthFunction, ColorFunction, OffsetFunction,
        shader: GameShaders.Misc["CalamityMod:ExobladePierce"]), 30);
}
```

---

## Phase Flow Summary

### Overall Fight Phases

| Phase | Active Mechs | Condition |
|-------|--------------|-----------|
| 1 | Player's choice | Initial summon |
| 2 | First mech | Other mechs passive |
| 3 | First mech < 70% | Others become active |
| 4 | One mech < 40% | That mech goes berserk |
| 5 | First death | Remaining mechs enrage |
| 6 | All < 40% | Fusion to Exo Mechdusa (optional) |

---

## Implementation Tips for MagnumOpus

### Multi-Boss Coordination
1. **Global Indices**: Store boss references in ModSystem for cross-reference
2. **Secondary Phases**: Use state machine for active/passive transitions
3. **HP Linking**: Consider shared or synchronized health pools
4. **Berserk Mechanics**: Last boss standing gets significant buffs

### Visual Theme Adaptation
- Replace Exo Palette with your theme's gradient colors
- Maintain the cycling/rainbow effect for dynamic visuals
- Each "mech" can have unique sub-theme within overall palette

### Attack Variety
- **Ares-style**: Stationary boss with multiple attack points (arms/turrets)
- **Twin-style**: Two coordinated entities attacking in sync
- **Thanatos-style**: Long entity with opening/closing vulnerability windows

### Key Takeaways
1. **Cross-NPC Communication**: Use global indices and AI arrays
2. **Phase State Machine**: SecondaryPhase enum controls behavior modes
3. **Berserk Trigger**: Clear conditions for escalation
4. **Arm/Segment Architecture**: Sub-NPCs attached to main body
5. **Color Identity**: Unified palette cycling for theme cohesion
