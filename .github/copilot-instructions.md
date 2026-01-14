# MagnumOpus Copilot Instructions

## VFX Requirements - MANDATORY

ALL attacks, projectiles, boss abilities, weapon effects, and enemy spawns MUST use the custom particle system. Never use only vanilla Dust effects alone.

### Core Principles: VIBRANCY, UNIQUENESS, and VARIATION

**Every effect in MagnumOpus should be:**
1. **VIBRANT** - Bold, saturated colors with strong visual impact. Effects should POP on screen.
2. **UNIQUE** - Each weapon/ability should have distinct visual identity. No two weapons should look identical.
3. **VARIED** - Even within a theme, use different particle combinations, patterns, and timings.

**AVOID:**
- Generic, copy-pasted effects between weapons
- Bland, single-color explosions without gradient fading
- Reusing the exact same effect across different themes
- Low-impact, barely-visible particles

**EMBRACE:**
- Creative combinations of existing particle systems
- Theme-appropriate variations that still feel fresh
- Layered effects (particles + halos + lightning + flares)
- Color gradients that transition smoothly within theme palettes

### Design Philosophy - UNIQUE FRACTAL EFFECTS

**Every effect should be unique and include fractal-like geometric patterns when possible.** The signature look is demonstrated by Feather's Call's left-click attack:

```csharp
// FRACTAL FLARE BURST - the signature geometric look
for (int i = 0; i < 6; i++)
{
    float angle = MathHelper.TwoPi * i / 6f;
    Vector2 flareOffset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 25f;
    float hue = (Main.GameUpdateCount * 0.02f + i * 0.16f) % 1f;
    Color fractalColor = Main.hslToRgb(hue, 1f, 0.85f);
    CustomParticles.GenericFlare(position + flareOffset, fractalColor, 0.4f, 18);
}
```

**Key principles:**
- Use radial geometric patterns (6-8 point star bursts)
- Layer multiple effect types for depth
- Include mini lightning fractals for high-impact moments
- Use prismatic/rainbow color cycling where thematically appropriate
- Combine themed particles with geometric flare arrangements

---

## CRITICAL: Theme Identity & Uniqueness - MANDATORY

**Each musical score/theme MUST have its own distinct visual identity.** Do NOT copy effects between themes. While all effects should be vibrant and explosive, each theme must feel completely different.

### Why Theme Uniqueness Matters
- Players should instantly recognize which theme's weapon they're using just from the visuals
- Cross-theme copying creates bland, forgettable effects
- Each score represents a different musical/emotional tone that must translate to visuals

### Theme Visual Identities (FOLLOW STRICTLY)

| Theme | Core Feel | NEVER Copy From | Unique Elements |
|-------|-----------|-----------------|-----------------|
| **La Campanella** | Infernal bells, smoky darkness, burning orange | - | Heavy smoke, bell chimes, fire fractals |
| **Eroica** | Heroic triumph, sakura petals, fierce gold | La Campanella | Sakura petals, rising embers, triumphant bursts |
| **Swan Lake** | Elegant grace, monochromatic contrast, rainbow shimmer | Eroica | Feathers, black/white contrast, prismatic edges |
| **Moonlight Sonata** | Mystical lunar, soft purple glow, silver mist | Swan Lake | Moon phases, gentle waves, ethereal mist |
| **Enigma Variations** | Arcane mystery, swirling void, eerie green | Moonlight | Question marks, void swirls, mystery symbols |
| **Fate** | Cosmic endgame, reality-breaking, chromatic | ALL others | Screen distortions, reality tears, temporal echoes |

### FORBIDDEN Cross-Theme Copying

```csharp
// ❌ WRONG - Using La Campanella effects on Fate weapon
UnifiedVFX.LaCampanella.DeathExplosion(position, scale); // NO! Wrong theme!
ThemedParticles.LaCampanellaSparks(position, direction, count, speed); // NO!

// ✅ CORRECT - Use the weapon's own theme
UnifiedVFX.Fate.Explosion(position, scale);
// Or create unique Fate-specific effects with reality distortions
```

### Creating Unique Theme Effects

**Instead of copying, create VARIATIONS that match the theme:**

```csharp
// La Campanella explosion - smoky, fiery, bell-like
UnifiedVFX.LaCampanella.Explosion(pos, scale);
// Includes: HeavySmokeParticle, orange/black gradient, bell chime sounds

// Eroica explosion - triumphant, petal-filled, golden
UnifiedVFX.Eroica.Explosion(pos, scale);  
// Includes: SakuraPetals, scarlet/gold gradient, rising embers

// Swan Lake explosion - elegant, feathered, prismatic
UnifiedVFX.SwanLake.Explosion(pos, scale);
// Includes: SwanFeatherBurst, black/white contrast, rainbow sparkles

// Moonlight Sonata explosion - ethereal, misty, lunar
UnifiedVFX.MoonlightSonata.Explosion(pos, scale);
// Includes: Soft bloom, purple/silver mist, moon-like halos

// Enigma Variations explosion - mysterious, void-touched, arcane
UnifiedVFX.EnigmaVariations.Explosion(pos, scale);
// Includes: Void swirls, green flame accents, mystery particles

// Fate explosion - reality-shattering, cosmic, UNIQUE
UnifiedVFX.Fate.Explosion(pos, scale);
// Includes: Screen distortions, chromatic aberration, temporal echoes
// FATE MUST BE THE MOST VISUALLY DISTINCT - it's endgame content!
```

### Fate-Specific Requirements (ENDGAME)

Fate is the endgame theme and MUST include effects no other theme has:
- **Screen slice effects** - visual "cuts" across the screen
- **Reality shatter** - screen fragments briefly
- **Chromatic aberration** - RGB color separation
- **Temporal echoes** - sharp afterimage trails
- **Color inversion pulses** - brief negative flashes

```csharp
// Fate weapons should feel like they're breaking reality itself
// Every Fate effect should make the player think "whoa, that's different"
```

### Checklist Before Implementing Any Effect

1. ✅ Am I using the CORRECT theme's UnifiedVFX/ThemedParticles?
2. ✅ Does this effect feel different from other themes?
3. ✅ Am I using this theme's unique color palette?
4. ✅ For Fate: Did I include reality-distortion effects?
5. ✅ Would a player recognize the theme just from the visuals?

---

## Asset Requirements - PLACEHOLDERS

**If any texture or buff icon doesn't have an image file, use a placeholder:**

```csharp
// For items/projectiles without textures:
public override string Texture => "Terraria/Images/Item_" + ItemID.DirtBlock;
// OR use an existing mod texture:
public override string Texture => "MagnumOpus/Assets/Particles/Placeholder";

// For buffs without icons:
public override string Texture => "Terraria/Images/Buff_" + BuffID.Confused;
```

**Never leave a texture path pointing to a non-existent file.** Always fall back to a vanilla texture or existing mod asset.

---

### Required VFX Calls

Every impact, explosion, projectile spawn, attack windup, and attack firing should include a combination of:

```csharp
// Core flares (always include at least one)
CustomParticles.GenericFlare(position, themeColor, scale, lifetime);
CustomParticles.GenericGlow(position, velocity, color, scale, lifetime, fade);

// Halo/ring effects (for impacts and explosions)
CustomParticles.HaloRing(position, color, scale, lifetime);
ThemedParticles.[Theme]HaloBurst(position, scale);

// Explosion bursts (for impacts and deaths)
CustomParticles.ExplosionBurst(position, color, count, speed);

// Theme-specific effects
ThemedParticles.[Theme]Impact(position, scale);
ThemedParticles.[Theme]Shockwave(position, scale);
ThemedParticles.[Theme]Sparkles(position, count, radius);
ThemedParticles.[Theme]Sparks(position, direction, count, speed);
```

### Standard VFX Pattern

**Attack Windup:**
```csharp
// Pulsing flares that grow with charge progress
float chargeProgress = Timer / WindupTime;
CustomParticles.GenericFlare(NPC.Center, themeColor, 0.3f + chargeProgress * 0.5f, 20);
CustomParticles.HaloRing(NPC.Center, themeColor, 0.2f + chargeProgress * 0.3f, 15);
```

**Attack Firing / Projectile Spawn:**
```csharp
ThemedParticles.[Theme]HaloBurst(spawnPos, 1.2f);
CustomParticles.GenericFlare(spawnPos, primaryColor, 0.7f, 20);
CustomParticles.GenericFlare(spawnPos, secondaryColor, 0.5f, 15);
CustomParticles.HaloRing(spawnPos, primaryColor, 0.4f, 18);
```

**Impact / Explosion:**
```csharp
ThemedParticles.[Theme]Impact(position, 1.5f);
ThemedParticles.[Theme]HaloBurst(position, 1.5f);
CustomParticles.GenericFlare(position, primaryColor, 0.8f, 25);
CustomParticles.GenericFlare(position, Color.White, 0.6f, 20);
CustomParticles.HaloRing(position, primaryColor, 0.5f, 20);
CustomParticles.ExplosionBurst(position, primaryColor, 12, 10f);

// ADD FRACTAL PATTERN for unique look
for (int i = 0; i < 6; i++)
{
    float angle = MathHelper.TwoPi * i / 6f;
    Vector2 offset = angle.ToRotationVector2() * 30f;
    CustomParticles.GenericFlare(position + offset, secondaryColor, 0.4f, 15);
}
```

**Projectile Trail (periodic, every 3-5 frames):**
```csharp
if (Projectile.timeLeft % 4 == 0)
{
    CustomParticles.GenericFlare(Projectile.Center, themeColor, 0.4f, 15);
}
```

---

## MANDATORY: Gradient Color Fading Effects

**ALL weapon effects, particles, flares, explosions, and accessory effects MUST use gradient color fading.** Never use single-color explosions. Every effect should fade from one theme color to another within the same palette.

### Gradient Fading Pattern (REQUIRED)
```csharp
// USE THIS - Gradient fading from primary to secondary color
float progress = (float)i / count; // or use lifetime progress
Color gradientColor = Color.Lerp(primaryThemeColor, secondaryThemeColor, progress);
CustomParticles.GenericFlare(position, gradientColor, scale, lifetime);

// For particles with lifetime, fade over time:
var particle = new GenericGlowParticle(position, velocity, primaryColor, scale, lifetime, true)
    .WithGradient(secondaryColor); // Fades from primary to secondary over lifetime

// For radial bursts, gradient across the burst:
for (int i = 0; i < 8; i++)
{
    float progress = (float)i / 8f;
    Color gradientColor = Color.Lerp(primaryColor, secondaryColor, progress);
    float angle = MathHelper.TwoPi * i / 8f;
    CustomParticles.GenericFlare(position + angle.ToRotationVector2() * 30f, gradientColor, 0.5f, 18);
}
```

### Theme Gradient Examples
```csharp
// Moonlight Sonata: Dark Purple → Light Blue
Color.Lerp(new Color(75, 0, 130), new Color(135, 206, 250), progress)

// Eroica: Deep Scarlet → Bright Gold
Color.Lerp(new Color(139, 0, 0), new Color(255, 215, 0), progress)

// La Campanella: Black → Orange (with smoky effects)
Color.Lerp(CampanellaBlack, CampanellaOrange, progress)
// ALWAYS include HeavySmokeParticle for smoky atmosphere

// Swan Lake: Pure White → Iridescent Rainbow
Color.Lerp(Color.White, Main.hslToRgb(progress, 1f, 0.8f), progress * 0.6f)

// Enigma Variations: Black → Purple → Green Flame
Color.Lerp(Color.Lerp(new Color(20, 10, 30), new Color(120, 40, 180), progress * 2f), new Color(50, 200, 80), Math.Max(0, progress * 2f - 1f))

// Fate: White → Dark Pink → Deep Purple → Crimson (cosmic)
// Use multi-step gradient for cosmic amorphous look
```

---

## Theme Color Palettes

### La Campanella (Infernal Bell)
**Gradient: Black → Orange (with smoky effects)**
```csharp
ThemedParticles.CampanellaBlack   // (20, 15, 20) - Primary (start) - smoky darkness
ThemedParticles.CampanellaOrange  // (255, 100, 0) - Secondary (end) - infernal flames
ThemedParticles.CampanellaYellow  // (255, 200, 50) - Accent - flame highlights
ThemedParticles.CampanellaGold    // (218, 165, 32) - Accent - golden shimmer
ThemedParticles.CampanellaRed     // (200, 50, 30) - Intense/enrage
// Gradient: Color.Lerp(CampanellaBlack, CampanellaOrange, progress)
// MANDATORY: Include HeavySmokeParticle for smoky atmosphere in ALL La Campanella effects
```

### Eroica (Heroic/Epic)
**Gradient: Scarlet → Crimson → Gold**
```csharp
ThemedParticles.EroicaScarlet     // (139, 0, 0) - Primary (start)
ThemedParticles.EroicaCrimson     // (220, 50, 50) - Secondary (mid)
ThemedParticles.EroicaGold        // (255, 215, 0) - Accent (end)
ThemedParticles.EroicaSakura      // (255, 150, 180) - Sakura pink
ThemedParticles.EroicaBlack       // (30, 20, 25) - Smoke
// Gradient: Color.Lerp(EroicaScarlet, EroicaGold, progress)
```

### Swan Lake (Graceful/Ethereal)
**Gradient: Pure White → Black with Rainbow Shimmer**
```csharp
ThemedParticles.SwanWhite         // (255, 255, 255) - Primary
ThemedParticles.SwanBlack         // (20, 20, 30) - Contrast
ThemedParticles.SwanIridescent    // Rainbow cycling - use Main.hslToRgb()
ThemedParticles.SwanSilver        // (220, 225, 235) - Accent
// Gradient: Alternate white/black with rainbow shimmer overlay
```

### Moonlight Sonata (Lunar/Mystical)
**Gradient: Dark Purple → Light Blue**
```csharp
ThemedParticles.MoonlightDarkPurple  // (75, 0, 130) - Primary (start)
ThemedParticles.MoonlightViolet      // (138, 43, 226) - Mid
ThemedParticles.MoonlightLightBlue   // (135, 206, 250) - Secondary (end)
ThemedParticles.MoonlightSilver      // (220, 220, 235) - Accent
// Gradient: Color.Lerp(MoonlightDarkPurple, MoonlightLightBlue, progress)
```

### Clair de Lune (Celestial)
**Gradient: Night Mist → Pearl White**
```csharp
ThemedParticles.ClairNightMist    // (100, 120, 160) - Primary (start)
ThemedParticles.ClairSoftBlue     // (140, 170, 220) - Mid
ThemedParticles.ClairPearl        // (240, 240, 250) - Secondary (end)
// Gradient: Color.Lerp(ClairNightMist, ClairPearl, progress)
```

### Enigma Variations (Mysterious/Arcane) - NEW
**Gradient: Black → Deep Purple → Eerie Green Flame**
**Design: Mysteries, question marks, swirling unknowns**
```csharp
ThemedParticles.EnigmaBlack       // (15, 10, 20) - Primary (start) - void darkness
ThemedParticles.EnigmaDeepPurple  // (80, 20, 120) - Mid - arcane mystery
ThemedParticles.EnigmaPurple      // (140, 60, 200) - Secondary
ThemedParticles.EnigmaGreenFlame  // (50, 220, 100) - Accent (end) - eerie flame
ThemedParticles.EnigmaDarkGreen   // (30, 100, 50) - Dark green accent
// Gradient: Black → Purple → Green flame transition
// Color.Lerp(Color.Lerp(EnigmaBlack, EnigmaPurple, progress * 2f), EnigmaGreenFlame, Math.Max(0, progress * 2f - 1f))

// SPECIAL: Include question mark and mystery symbol particles
// Swirling void effects with green flame accents
// Effects should feel unknowable and arcane
```

### Fate (Cosmic Endgame) - NEW
**Gradient: White → Dark Pink → Purple → Crimson**
**Design: SHARP, PRECISE, FLASHY, CLEAN - Cosmic amorphous epic**
**REQUIRES: Visual distortions, screen effects, reality-bending visuals**
```csharp
ThemedParticles.FateWhite         // (255, 255, 255) - Primary (start) - cosmic light
ThemedParticles.FateDarkPink      // (200, 80, 120) - Secondary - destiny's edge
ThemedParticles.FatePurple        // (140, 50, 160) - Mid - fate's weave
ThemedParticles.FateCrimson       // (180, 30, 60) - Accent (end) - inevitable doom
ThemedParticles.FateBlack         // (10, 5, 15) - Void contrast
// Gradient: Multi-step cosmic gradient
// Step 1: White → Dark Pink (progress 0-0.33)
// Step 2: Dark Pink → Purple (progress 0.33-0.66)
// Step 3: Purple → Crimson (progress 0.66-1.0)

// MANDATORY VISUAL DISTORTIONS FOR FATE:
// - Screen slice effects (reality cuts)
// - Color channel separation (chromatic aberration)
// - Screen fragment shattering
// - Temporal distortion (afterimage trails)
// - Inverse color flashes
// - Reality tear effects
```

---

## Fate Visual Distortion Effects (ENDGAME EXCLUSIVE)

Fate weapons are endgame content and MUST include dramatic visual distortions:

### Screen Slice Effect
```csharp
// Creates a visual "cut" across the screen
public static void FateScreenSlice(Vector2 start, Vector2 end, float intensity)
{
    // Draw sharp white line with dark pink/purple edges
    // Chromatic aberration along the cut
    // Brief screen displacement effect
}
```

### Reality Shatter Effect
```csharp
// Screen appears to break into fragments briefly
public static void FateRealityShatter(Vector2 center, int fragments, float duration)
{
    // Screen divided into triangular fragments
    // Fragments briefly offset/rotate
    // Cosmic gradient on fragment edges
    // Sharp, precise, geometric
}
```

### Color Inversion Pulse
```csharp
// Brief negative/inverse color flash
// White becomes black, colors invert momentarily
// Creates "reality breaking" feel
```

### Temporal Echo Trail
```csharp
// Sharp afterimages with color gradient
for (int i = 0; i < 8; i++)
{
    float echoProgress = i / 8f;
    Color echoColor = Color.Lerp(FateWhite, FateCrimson, echoProgress) * (1f - echoProgress);
    // Draw sharp, precise afterimage at historical position
}
```

---

## UnifiedVFX System - PREFERRED API

**The UnifiedVFX system is the PREFERRED way to create visual effects.** It provides a consolidated API that combines all particle systems with theme-specific effects.

### Basic Usage
```csharp
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;

// Theme-based effects - PREFERRED
UnifiedVFX.[Theme].[Effect](position, scale);

// Examples:
UnifiedVFX.LaCampanella.Impact(position, 1.5f);     // Infernal bell impact
UnifiedVFX.Eroica.DeathExplosion(position, 2f);    // Heroic boss death
UnifiedVFX.SwanLake.Trail(position, velocity);      // Feather trail
UnifiedVFX.Fate.Explosion(position, 1.5f);          // Cosmic explosion
UnifiedVFX.Generic.FractalBurst(pos, color1, color2, 8, 40f, 1f); // Geometric burst
```

### Available Theme Classes

#### UnifiedVFX.LaCampanella
**Infernal Bell Theme - Black → Orange**
```csharp
// Colors
UnifiedVFX.LaCampanella.Black    // (20, 15, 20)
UnifiedVFX.LaCampanella.Orange   // (255, 100, 0)
UnifiedVFX.LaCampanella.Yellow   // (255, 200, 50)
UnifiedVFX.LaCampanella.Gold     // (218, 165, 32)

// Effects
UnifiedVFX.LaCampanella.Impact(position, scale);        // Standard impact with smoke
UnifiedVFX.LaCampanella.Explosion(position, scale);     // Major explosion
UnifiedVFX.LaCampanella.BellChime(position, scale);     // Musical bell chime
UnifiedVFX.LaCampanella.SwingAura(pos, dir, scale);     // Weapon swing
UnifiedVFX.LaCampanella.Trail(pos, vel, scale);         // Projectile trail
UnifiedVFX.LaCampanella.DeathExplosion(position, scale);// Boss death
UnifiedVFX.LaCampanella.Aura(position, radius, scale);  // Ambient aura
```

#### UnifiedVFX.Eroica
**Heroic Theme - Scarlet → Gold**
```csharp
// Colors
UnifiedVFX.Eroica.Scarlet   // (139, 0, 0)
UnifiedVFX.Eroica.Crimson   // (220, 50, 50)
UnifiedVFX.Eroica.Flame     // (255, 100, 50)
UnifiedVFX.Eroica.Gold      // (255, 215, 0)
UnifiedVFX.Eroica.Sakura    // (255, 150, 180)

// Effects
UnifiedVFX.Eroica.Impact(position, scale);
UnifiedVFX.Eroica.Explosion(position, scale);       // Includes sakura petals
UnifiedVFX.Eroica.SwingAura(pos, dir, scale);
UnifiedVFX.Eroica.Trail(pos, vel, scale);
UnifiedVFX.Eroica.PhaseTransition(position, scale); // Boss phase change
UnifiedVFX.Eroica.DeathExplosion(position, scale);
UnifiedVFX.Eroica.Aura(position, radius, scale);
```

#### UnifiedVFX.MoonlightSonata
**Lunar Theme - Purple → Blue**
```csharp
// Colors
UnifiedVFX.MoonlightSonata.DarkPurple   // (75, 0, 130)
UnifiedVFX.MoonlightSonata.MediumPurple // (138, 43, 226)
UnifiedVFX.MoonlightSonata.LightBlue    // (135, 206, 250)
UnifiedVFX.MoonlightSonata.Silver       // (220, 220, 235)

// Effects - same pattern as other themes
UnifiedVFX.MoonlightSonata.Impact(position, scale);
UnifiedVFX.MoonlightSonata.Explosion(position, scale);
// ... etc
```

#### UnifiedVFX.SwanLake
**Graceful Theme - White/Black + Rainbow**
```csharp
// Colors
UnifiedVFX.SwanLake.White   // (255, 255, 255)
UnifiedVFX.SwanLake.Black   // (20, 20, 30)
UnifiedVFX.SwanLake.Silver  // (220, 225, 235)
UnifiedVFX.SwanLake.GetRainbow(offset) // Rainbow color cycling

// Effects include feathers and prismatic effects
UnifiedVFX.SwanLake.Impact(position, scale);  // Includes feather burst
// ... etc
```

#### UnifiedVFX.EnigmaVariations
**Mysterious Theme - Black → Purple → Green**
```csharp
// Colors
UnifiedVFX.EnigmaVariations.Black       // (15, 10, 20)
UnifiedVFX.EnigmaVariations.DeepPurple  // (80, 20, 120)
UnifiedVFX.EnigmaVariations.Purple      // (140, 60, 200)
UnifiedVFX.EnigmaVariations.GreenFlame  // (50, 220, 100)

// Effects with eerie green flames
UnifiedVFX.EnigmaVariations.Impact(position, scale);
// ... etc
```

#### UnifiedVFX.Fate
**Cosmic Endgame Theme - White → Pink → Purple → Crimson**
```csharp
// Colors
UnifiedVFX.Fate.White     // (255, 255, 255)
UnifiedVFX.Fate.DarkPink  // (200, 80, 120)
UnifiedVFX.Fate.Purple    // (140, 50, 160)
UnifiedVFX.Fate.Crimson   // (180, 30, 60)

// Helper for cosmic gradient
UnifiedVFX.Fate.GetCosmicGradient(progress) // Returns gradient color 0-1

// Effects - maximum spectacle for endgame
UnifiedVFX.Fate.Impact(position, scale);
UnifiedVFX.Fate.Explosion(position, scale);    // Reality-shattering
UnifiedVFX.Fate.SwingAura(pos, dir, scale);    // Chromatic aberration
UnifiedVFX.Fate.Trail(pos, vel, scale);        // Cosmic afterimages
UnifiedVFX.Fate.DeathExplosion(position, scale); // Ultimate spectacle
```

#### UnifiedVFX.Generic
**Theme-agnostic utilities**
```csharp
UnifiedVFX.Generic.Impact(pos, primary, secondary, scale);
UnifiedVFX.Generic.Explosion(pos, primary, secondary, scale);
UnifiedVFX.Generic.DeathExplosion(pos, primary, secondary, scale);
UnifiedVFX.Generic.Teleport(departure, arrival, color, scale);
UnifiedVFX.Generic.ChargeWindup(pos, color, progress, scale);
UnifiedVFX.Generic.AttackRelease(pos, primary, secondary, scale);

// Signature MagnumOpus geometric burst
UnifiedVFX.Generic.FractalBurst(position, primary, secondary, points, radius, scale);

// Orbiting aura particles
UnifiedVFX.Generic.OrbitingAura(center, primary, secondary, radius, count, scale);
```

### Upgrade Pattern - Converting Old Effects to UnifiedVFX

**Before (old style):**
```csharp
ThemedParticles.LaCampanellaImpact(position, 1.5f);
CustomParticles.GenericFlare(position, ThemedParticles.CampanellaOrange, 0.5f, 20);
CustomParticles.HaloRing(position, ThemedParticles.CampanellaOrange, 0.4f, 18);
```

**After (UnifiedVFX - PREFERRED):**
```csharp
UnifiedVFX.LaCampanella.Impact(position, 1.5f);
// Single call does all the work with proper gradient colors and effects
```

### Boss Death Animation Example
```csharp
private void UpdateDeathAnimation()
{
    deathTimer++;
    float progress = (float)deathTimer / DeathAnimationDuration;
    
    // Phase 1: Building intensity
    if (deathTimer < 120)
    {
        float intensity = (float)deathTimer / 120f;
        
        // Fractal flare pattern with gradient
        if (deathTimer % 5 == 0)
        {
            int points = 6 + (int)(intensity * 4);
            for (int i = 0; i < points; i++)
            {
                float angle = MathHelper.TwoPi * i / points + deathTimer * 0.05f;
                Vector2 offset = angle.ToRotationVector2() * (30f + intensity * 40f);
                float gradientProgress = (float)i / points;
                Color flareColor = Color.Lerp(UnifiedVFX.Eroica.Scarlet, UnifiedVFX.Eroica.Gold, gradientProgress);
                CustomParticles.GenericFlare(NPC.Center + offset, flareColor, 0.4f + intensity * 0.4f, 15);
            }
        }
        
        MagnumScreenEffects.AddScreenShake(intensity * 3f);
    }
    // Phase 2: Climax
    else if (deathTimer == 150)
    {
        // UnifiedVFX themed death explosion
        UnifiedVFX.Eroica.DeathExplosion(NPC.Center, 1.5f);
        
        // Extra spiral galaxy effect for heroic finale
        for (int arm = 0; arm < 6; arm++)
        {
            float armAngle = MathHelper.TwoPi * arm / 6f;
            for (int point = 0; point < 8; point++)
            {
                float spiralAngle = armAngle + point * 0.4f;
                float spiralRadius = 25f + point * 18f;
                Vector2 spiralPos = NPC.Center + spiralAngle.ToRotationVector2() * spiralRadius;
                float gradientProgress = (arm * 8 + point) / 48f;
                Color galaxyColor = Color.Lerp(UnifiedVFX.Eroica.Scarlet, UnifiedVFX.Eroica.Gold, gradientProgress);
                CustomParticles.GenericFlare(spiralPos, galaxyColor, 0.5f + point * 0.05f, 25);
            }
        }
    }
}
```

### Weapon Effect Example
```csharp
public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, ...)
{
    // UnifiedVFX themed impact
    UnifiedVFX.Eroica.Impact(position, 1.2f);
    
    // Fractal flare burst with gradient
    for (int i = 0; i < 8; i++)
    {
        float angle = MathHelper.TwoPi * i / 8f;
        Vector2 flareOffset = angle.ToRotationVector2() * 35f;
        float progress = (float)i / 8f;
        Color fractalColor = Color.Lerp(UnifiedVFX.Eroica.Scarlet, UnifiedVFX.Eroica.Gold, progress);
        CustomParticles.GenericFlare(position + flareOffset, fractalColor, 0.55f, 22);
    }
    
    // Sakura petals for Eroica theme
    ThemedParticles.SakuraPetals(position, 4, 30f);
    
    return false;
}
```

### Accessory Ambient Effect Example
```csharp
public override void UpdateAccessory(Player player, bool hideVisual)
{
    if (!hideVisual)
    {
        // UnifiedVFX themed aura
        UnifiedVFX.Eroica.Aura(player.Center, 35f, 0.3f);
        
        // Orbiting gradient flares - signature geometric look
        if (Main.rand.NextBool(8))
        {
            float baseAngle = Main.GameUpdateCount * 0.025f;
            for (int i = 0; i < 3; i++)
            {
                float angle = baseAngle + MathHelper.TwoPi * i / 3f;
                float radius = 30f + (float)Math.Sin(Main.GameUpdateCount * 0.05f + i * 0.7f) * 8f;
                Vector2 flarePos = player.Center + angle.ToRotationVector2() * radius;
                float progress = (float)i / 3f;
                Color fractalColor = Color.Lerp(UnifiedVFX.Eroica.Scarlet, UnifiedVFX.Eroica.Gold, progress);
                CustomParticles.GenericFlare(flarePos, fractalColor, 0.32f, 16);
            }
        }
    }
}
```

---

## Available Particle Types

### CustomParticles (Common/Systems/Particles/)
- `GenericFlare(position, color, scale, lifetime)` - Bright point glow
- `GenericGlow(position, velocity, color, scale, lifetime, fade)` - Moving glow
- `HaloRing(position, color, scale, lifetime)` - Expanding ring
- `ExplosionBurst(position, color, count, speed)` - Radial particle spray

### ThemedParticles (Common/Systems/ThemedParticles.cs)
Each theme has:
- `[Theme]BloomBurst` - Soft bloom effect
- `[Theme]Sparkles` - Scattered sparkle particles
- `[Theme]Shockwave` - Expanding shockwave ring
- `[Theme]Sparks` - Directional spark particles
- `[Theme]Impact` - Standard impact effect
- `[Theme]Trail` - Trail effect for projectiles
- `[Theme]Aura` - Ambient aura particles
- `[Theme]HaloBurst` - Bright halo explosion
- `[Theme]BellChime` (La Campanella) - Bell-specific effect
- `[Theme]MusicNotes` - Floating music note particles

### MagnumVFX (Common/Systems/MagnumVFX.cs)
- `DrawLaCampanellaLightning(start, end, segments, amplitude, branches, branchChance)`
- `DrawEroicaLightning(...)` 
- Other themed lightning/beam effects

### Particle Classes (for custom behavior)
- `GenericGlowParticle` - Glowing particle with fade
- `GlowSparkParticle` - Spark with stretch and gravity
- `HeavySmokeParticle` - Smoke with drift

---

## Boss Requirements

### Spawn
- Boss summon items must spawn boss above ground accounting for NPC.height
- Spawn position should use tile collision checks

### Attack Windups
- Progressive VFX that scales with charge progress
- Screen shake building to attack
- Sound cues

### Attack Firing
- Full VFX burst on attack release
- Sky flash for major attacks (via LaCampanellaSkyEffect.TriggerFlash, etc.)
- Screen shake on impact

### Enrage
- Massive VFX explosion with multiple flares/halos
- Color shift to more intense variants
- Sky effect

---

## Projectile Requirements

### OnSpawn / First Frame
- Spawn flare and halo at origin

### AI (periodic)
- Trail effects every 3-5 frames
- Periodic flares for visibility

### Kill / OnHit
- Full impact VFX suite
- HaloBurst, GenericFlare, ExplosionBurst

---

## Weapon Requirements

### Melee
- Use MeleeSmearEffect for swing trails
- Impact VFX on hit

### Ranged
- Muzzle flash on fire (GenericFlare, HaloRing)
- Projectile uses standard projectile VFX

### Magic
- Cast VFX at player/weapon
- Themed particles matching spell element

### Summon
- Spawn VFX for minion appearance
- Ambient particles on minion

---

## ADVANCED VFX COMBINATIONS - UNIQUE SIGNATURE EFFECTS

### Ambient Fractal Orbit Pattern (HoldItem)
Use for weapons held by the player to create a magical aura:

```csharp
// Orbiting fractal flares - creates celestial/magical presence
if (Main.rand.NextBool(6))
{
    float baseAngle = Main.GameUpdateCount * 0.025f;
    for (int i = 0; i < 5; i++)
    {
        float angle = baseAngle + MathHelper.TwoPi * i / 5f;
        float radius = 35f + (float)Math.Sin(Main.GameUpdateCount * 0.05f + i * 0.7f) * 12f;
        Vector2 flarePos = player.Center + angle.ToRotationVector2() * radius;
        Color fractalColor = Color.Lerp(primaryColor, secondaryColor, (float)i / 5f);
        CustomParticles.GenericFlare(flarePos, fractalColor, 0.32f, 18);
    }
}
```

### Spiral Galaxy Burst (Ultimate Attacks)
```csharp
// Creates a swirling galaxy explosion effect
for (int arm = 0; arm < 6; arm++)
{
    float armAngle = MathHelper.TwoPi * arm / 6f;
    for (int point = 0; point < 8; point++)
    {
        float spiralAngle = armAngle + point * 0.4f;
        float spiralRadius = 20f + point * 15f;
        Vector2 spiralPos = position + spiralAngle.ToRotationVector2() * spiralRadius;
        float hue = (arm / 6f + point * 0.05f) % 1f;
        Color galaxyColor = Main.hslToRgb(hue, 1f, 0.8f);
        CustomParticles.GenericFlare(spiralPos, galaxyColor, 0.3f + point * 0.05f, 20 + point * 2);
    }
}
CustomParticles.HaloRing(position, Color.White, 1.2f, 30);
ThemedParticles.[Theme]Shockwave(position, 1.5f);
```

### Layered Halo Cascade (Boss Phase Transitions)
```csharp
// Multiple expanding halos with staggered timing
for (int ring = 0; ring < 8; ring++)
{
    float hue = (Main.GameUpdateCount * 0.02f + ring * 0.12f) % 1f;
    Color ringColor = Main.hslToRgb(hue, 0.9f, 0.75f);
    float scale = 0.3f + ring * 0.15f;
    int lifetime = 15 + ring * 5;
    CustomParticles.HaloRing(position, ringColor, scale, lifetime);
}
// Central white flash
CustomParticles.GenericFlare(position, Color.White, 1.5f, 10);
```

### Fractal Lightning Web (Channeled/Chain Attacks)
```csharp
// Draw lightning fractals between multiple points
List<NPC> targets = FindNearbyEnemies(range, maxTargets);
Vector2 lastPoint = sourcePosition;
foreach (var target in targets)
{
    // Main lightning
    MagnumVFX.Draw[Theme]Lightning(lastPoint, target.Center, 12, 40f, 5, 0.6f);
    
    // Mini fractal branches at impact
    for (int i = 0; i < 4; i++)
    {
        float branchAngle = MathHelper.TwoPi * i / 4f;
        Vector2 branchEnd = target.Center + branchAngle.ToRotationVector2() * 60f;
        MagnumVFX.Draw[Theme]Lightning(target.Center, branchEnd, 4, 15f, 1, 0.3f);
    }
    
    // Explosion at each node
    ThemedParticles.[Theme]Impact(target.Center, 1.2f);
    lastPoint = target.Center;
}
```

### Chromatic Distortion Pulse (Reality-Bending Effects)
```csharp
// Creates a visual distortion effect with prismatic edges
for (int layer = 0; layer < 3; layer++)
{
    // RGB separation effect - offset each color slightly
    Vector2[] offsets = { new Vector2(-3, 0), Vector2.Zero, new Vector2(3, 0) };
    Color[] colors = { Color.Red * 0.7f, Color.Green * 0.7f, Color.Blue * 0.7f };
    
    CustomParticles.HaloRing(position + offsets[layer], colors[layer], 0.8f, 25);
}

// Central prismatic burst
CustomParticles.PrismaticSparkleRainbow(position, 12);
CustomParticles.GenericFlare(position, Color.White, 1.0f, 15);
```

### Resonance Wave (Musical Theme Effects)
```csharp
// Sound-wave style expanding rings with music notes
for (int wave = 0; wave < 5; wave++)
{
    float waveDelay = wave * 0.15f;
    Color waveColor = Color.Lerp(primaryColor, secondaryColor, wave / 5f) * (1f - wave * 0.15f);
    CustomParticles.HaloRing(position, waveColor, 0.4f + wave * 0.2f, 18 + wave * 4);
}

// Floating music notes spiral outward
ThemedParticles.[Theme]MusicNotes(position, 8, 50f);
ThemedParticles.[Theme]Accidentals(position, 4, 35f);

// Central chime flare
CustomParticles.GenericFlare(position, Color.White, 0.8f, 25);
```

### Vortex Pull Effect (Gravity/Vacuum Abilities)
```csharp
// Particles spiral inward toward a point
float vortexTimer = (Main.GameUpdateCount * 0.1f) % MathHelper.TwoPi;
for (int particle = 0; particle < 20; particle++)
{
    float angle = vortexTimer + MathHelper.TwoPi * particle / 20f;
    float radius = 150f - (particle * 5f); // Spiral inward
    Vector2 particlePos = center + angle.ToRotationVector2() * radius;
    Vector2 velocity = (center - particlePos).SafeNormalize(Vector2.Zero) * 3f;
    
    float hue = (particle / 20f + Main.GameUpdateCount * 0.01f) % 1f;
    Color vortexColor = Main.hslToRgb(hue, 0.8f, 0.7f);
    
    var glow = new GenericGlowParticle(particlePos, velocity, vortexColor, 0.3f, 15, true);
    MagnumParticleHandler.SpawnParticle(glow);
}
```

### Phoenix Rebirth Burst (Death/Respawn Effects)
```csharp
// Fiery explosion that reforms into a shape
// Phase 1: Explosion outward
CustomParticles.ExplosionBurst(position, primaryColor, 30, 15f);
CustomParticles.ExplosionBurst(position, secondaryColor, 20, 12f);

// Phase 2: Feather/flame rise
for (int i = 0; i < 12; i++)
{
    float angle = MathHelper.TwoPi * i / 12f;
    Vector2 riseVel = new Vector2(0, -4f).RotatedBy(angle * 0.2f);
    CustomParticles.SwanFeatherDrift(position + Main.rand.NextVector2Circular(20f, 20f), primaryColor, 0.4f);
}

// Phase 3: Radiant halo formation
for (int ring = 0; ring < 4; ring++)
{
    CustomParticles.HaloRing(position + new Vector2(0, -ring * 25f), primaryColor * (1f - ring * 0.2f), 0.5f, 20 + ring * 5);
}
```

### Dual-Polarity Contrast (Black/White Swan Lake Effects)
```csharp
// Alternating black and white effects for monochromatic theme
for (int i = 0; i < 12; i++)
{
    float angle = MathHelper.TwoPi * i / 12f;
    Vector2 offset = angle.ToRotationVector2() * 35f;
    
    // Alternate black and white
    Color flareColor = i % 2 == 0 ? Color.White : new Color(20, 20, 30);
    CustomParticles.GenericFlare(position + offset, flareColor, 0.5f, 18);
    
    // Opposite color halo at offset
    Color haloColor = i % 2 == 0 ? new Color(20, 20, 30) : Color.White;
    CustomParticles.HaloRing(position + offset * 0.5f, haloColor * 0.5f, 0.2f, 12);
}

// Rainbow shimmer in the spaces
for (int i = 0; i < 8; i++)
{
    float hue = i / 8f;
    Color rainbow = Main.hslToRgb(hue, 1f, 0.75f);
    CustomParticles.PrismaticSparkle(position + Main.rand.NextVector2Circular(30f, 30f), rainbow, 0.3f);
}
```

### Geometric Mandala Pattern (Meditation/Charge Effects)
```csharp
// Complex layered geometric pattern
float rotationSpeed = Main.GameUpdateCount * 0.02f;

// Inner triangle
for (int i = 0; i < 3; i++)
{
    float angle = rotationSpeed + MathHelper.TwoPi * i / 3f;
    CustomParticles.GenericFlare(position + angle.ToRotationVector2() * 20f, primaryColor, 0.4f, 15);
}

// Middle hexagon (opposite rotation)
for (int i = 0; i < 6; i++)
{
    float angle = -rotationSpeed * 0.7f + MathHelper.TwoPi * i / 6f;
    CustomParticles.GenericFlare(position + angle.ToRotationVector2() * 40f, secondaryColor, 0.35f, 15);
}

// Outer nonagon
for (int i = 0; i < 9; i++)
{
    float angle = rotationSpeed * 0.5f + MathHelper.TwoPi * i / 9f;
    float hue = (i / 9f + Main.GameUpdateCount * 0.01f) % 1f;
    Color outerColor = Main.hslToRgb(hue, 0.9f, 0.8f);
    CustomParticles.GenericFlare(position + angle.ToRotationVector2() * 65f, outerColor, 0.3f, 15);
}

// Pulsing center
float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.3f + 0.7f;
CustomParticles.GenericFlare(position, Color.White, pulse, 20);
```

---

## CUSTOM LIGHTING EFFECTS

### Pulsing Aura Light
```csharp
float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 0.15f + 0.85f;
Vector3 lightColor = primaryColor.ToVector3();
Lighting.AddLight(position, lightColor * pulse * intensity);
```

### Rainbow Cycling Light
```csharp
float hue = (Main.GameUpdateCount * 0.015f) % 1f;
Vector3 rainbowLight = Main.hslToRgb(hue, 0.8f, 0.6f).ToVector3();
Lighting.AddLight(position, rainbowLight * intensity);
```

### Flickering Fire Light
```csharp
float flicker = Main.rand.NextFloat(0.7f, 1.0f);
Lighting.AddLight(position, 1f * flicker, 0.5f * flicker, 0.2f * flicker);
```

### Dual-Tone Alternating Light
```csharp
bool alternate = (Main.GameUpdateCount / 10) % 2 == 0;
Vector3 lightColor = alternate ? primaryColor.ToVector3() : secondaryColor.ToVector3();
Lighting.AddLight(position, lightColor * intensity);
```

---

## SPECIAL SHADER/VISUAL DISTORTION TECHNIQUES

### Additive Bloom Layering (PreDraw)
```csharp
spriteBatch.End();
spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, 
    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

// Draw multiple scaled layers for bloom effect
float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.15f + 1f;
spriteBatch.Draw(texture, drawPos, null, outerGlowColor * 0.4f, rotation, origin, scale * pulse * 1.4f, SpriteEffects.None, 0f);
spriteBatch.Draw(texture, drawPos, null, middleGlowColor * 0.3f, rotation, origin, scale * pulse * 1.2f, SpriteEffects.None, 0f);
spriteBatch.Draw(texture, drawPos, null, innerGlowColor * 0.25f, rotation, origin, scale * pulse * 1.05f, SpriteEffects.None, 0f);

spriteBatch.End();
spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, ...);
```

### Chromatic Aberration Effect
```csharp
// Draw RGB channels offset for distortion
Vector2 offset = Main.rand.NextVector2Circular(2f, 2f);
spriteBatch.Draw(texture, drawPos + new Vector2(-2, 0), null, Color.Red * 0.3f, rotation, origin, scale, SpriteEffects.None, 0f);
spriteBatch.Draw(texture, drawPos, null, Color.Green * 0.3f, rotation, origin, scale, SpriteEffects.None, 0f);
spriteBatch.Draw(texture, drawPos + new Vector2(2, 0), null, Color.Blue * 0.3f, rotation, origin, scale, SpriteEffects.None, 0f);
```

### Trail Echo Effect (Projectile Drawing)
```csharp
// Draw fading afterimages
for (int i = 0; i < ProjectileID.Sets.TrailCacheLength[Projectile.type]; i++)
{
    float progress = (float)i / ProjectileID.Sets.TrailCacheLength[Projectile.type];
    float trailAlpha = 1f - progress;
    float trailScale = 1f - progress * 0.3f;
    Color trailColor = Color.Lerp(primaryColor, secondaryColor, progress) * trailAlpha * 0.6f;
    
    spriteBatch.Draw(texture, Projectile.oldPos[i] - Main.screenPosition, null, 
        trailColor, Projectile.oldRot[i], origin, scale * trailScale, SpriteEffects.None, 0f);
}
```

### Screen Shake Integration
```csharp
// Small shake - weapon hits
player.GetModPlayer<ScreenShakePlayer>()?.AddShake(2f, 4);

// Medium shake - explosions
player.GetModPlayer<ScreenShakePlayer>()?.AddShake(5f, 10);

// Large shake - boss attacks, ultimates
player.GetModPlayer<ScreenShakePlayer>()?.AddShake(12f, 25);

// Dramatic shake - phase transitions
player.GetModPlayer<ScreenShakePlayer>()?.AddShake(20f, 40);
```

### Sky Flash Effect (Major Impacts)
```csharp
// Trigger sky flash for dramatic moments
LaCampanellaSkyEffect.TriggerFlash(intensity);
// OR
EroicaSkyEffect.TriggerFlash(intensity);

// Combine with particle burst for full effect
ThemedParticles.[Theme]GrandImpact(position, 2f);
```

---

## PROJECTILE TRAIL COMBINATIONS

### Basic Themed Trail
```csharp
if (Projectile.timeLeft % 3 == 0)
{
    ThemedParticles.[Theme]Trail(Projectile.Center, Projectile.velocity);
    CustomParticles.GenericFlare(Projectile.Center, primaryColor, 0.3f, 12);
}
```

### Heavy Smoke + Spark Trail (Fire Weapons)
```csharp
// Black smoke with golden sparks
if (Main.rand.NextBool(2))
{
    var smoke = new HeavySmokeParticle(
        Projectile.Center, -Projectile.velocity * 0.15f, 
        Color.Black, Main.rand.Next(25, 40), 0.3f, 0.6f, 0.02f, false);
    MagnumParticleHandler.SpawnParticle(smoke);
}
CustomParticles.GenericGlow(Projectile.Center, -Projectile.velocity * 0.1f, primaryColor, 0.25f, 10, true);
```

### Prismatic Sparkle Trail (Magic/Rainbow Weapons)
```csharp
if (Main.rand.NextBool(3))
{
    float hue = (Main.GameUpdateCount * 0.02f + Main.rand.NextFloat()) % 1f;
    Color sparkleColor = Main.hslToRgb(hue, 1f, 0.8f);
    CustomParticles.PrismaticSparkle(Projectile.Center, sparkleColor, 0.25f);
}
```

### Feather Drift Trail (Swan Lake Weapons)
```csharp
if (Main.rand.NextBool(4))
{
    CustomParticles.SwanFeatherDrift(Projectile.Center, Color.White, 0.3f);
    CustomParticles.SwanFeatherDrift(Projectile.Center, Color.Black, 0.25f);
}
```
