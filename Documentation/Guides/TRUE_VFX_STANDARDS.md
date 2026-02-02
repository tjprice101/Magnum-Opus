# TRUE VFX STANDARDS - The Real Way to Make Weapons Shine

> **THIS DOCUMENT SUPERSEDES ALL PREVIOUS VFX DOCUMENTATION.**
> 
> This is what "good" actually looks like. Not generic flares. Not basic orbs. Not copy-pasted PNG projectiles. **Real, layered, dynamic, MUSICAL visual effects.**

---

## 🚨 THE CORE PROBLEM (READ THIS FIRST)

### What We've Been Doing Wrong

1. **"Slapping a flare" on a PreDraw** - Drawing a single flare texture is NOT a visual effect. It's lazy.
2. **Projectiles are translucent orbs** - They hit enemies and "puff away." No impact. No character. No identity.
3. **Effects are too dim** - Things should GLOW, SHIMMER, SPARKLE. Not fade into the background.
4. **Effects are sometimes too large** - Bigger is not better. Vanilla weapons prove small but vibrant wins.
5. **No musical identity** - This is a MUSIC MOD. Where are the music notes orbiting projectiles? Where are the staff lines in trails?
6. **No trailing curves** - Everything is rigid. Ark of the Cosmos has sine-wave trails that CURVE and FLOW. Ours are static lines.
7. **Sword arcs unused** - We have 9 premade SwordArc PNGs. They're barely being used for melee weapon swings.
8. **Wave projectiles are PNG copy-paste** - Real wave effects layer sword arcs with glows, dusts, and blooms.

---

## ✅ THE GOLD STANDARD: Iridescent Wingspan

**This is what a GOOD projectile looks like. Study it. Learn it. Replicate this quality.**

### What Iridescent Wingspan Does RIGHT:

```csharp
// 1. HEAVY DUST TRAILS - Constant flow, not sparse dots
for (int i = 0; i < 2; i++)
{
    Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(5f, 5f), dustType,
        -Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(2f, 2f),
        100, trailColor, 1.8f);  // Scale 1.8f - VISIBLE!
    d.noGravity = true;
    d.fadeIn = 1.4f;  // Fades IN, not out immediately
}

// 2. CONTRASTING SPARKLES - Opposite color for visual pop
if (Main.rand.NextBool(2))
{
    Dust opp = Dust.NewDustPerfect(Projectile.Center, oppositeDustType,
        -Projectile.velocity * 0.15f, 0, oppositeColor, 1.4f);
    opp.noGravity = true;
}

// 3. FREQUENT FLARES - 1-in-2 chance, not 1-in-10
if (Main.rand.NextBool(2))
{
    CustomParticles.GenericFlare(Projectile.Center + offset, trailColor, 0.5f, 18);
}

// 4. RAINBOW SHIMMER - Color shifts using Main.hslToRgb
if (Main.rand.NextBool(3))
{
    float hue = Main.rand.NextFloat();
    Color rainbow = Main.hslToRgb(hue, 1f, 0.7f);
    Dust r = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowTorch, vel, 0, rainbow, 1.5f);
}

// 5. LAYERED PREDRAW with multiple glow layers
Main.EntitySpriteDraw(texture, drawPos, null, glowColor * 0.5f, rot, origin, scale * 1.4f, ...);
Main.EntitySpriteDraw(texture, drawPos, null, glowColor * 0.3f, rot, origin, scale * 1.2f, ...);
Main.EntitySpriteDraw(texture, drawPos, null, Color.White, rot, origin, scale, ...);
```

### The Iridescent Wingspan Formula:

| Layer | What It Does | Frequency |
|-------|--------------|-----------|
| Heavy dust trail | Creates dense visible wake | Every frame, 2+ particles |
| Contrasting sparkles | Visual contrast and pop | 1-in-2 frames |
| Frequent flares | Adds brightness and glow | 1-in-2 frames |
| Rainbow/color shift | Dynamic hue cycling | 1-in-3 frames |
| Pearlescent shimmer | Extra color variation | 1-in-4 frames |
| Fractal gem effects | Theme-specific sparkle | 1-in-8 frames |
| Music notes | Musical identity | 1-in-6 frames |
| Multi-layer PreDraw | Glowing projectile body | Every frame |

---

## 🎯 HOW TO BUILD A PROPER PROJECTILE

### Step 1: The Layered Flare Core

**DO NOT** just draw one flare. Layer multiple flare textures, spinning them slightly:

```csharp
public override bool PreDraw(ref Color lightColor)
{
    // Load multiple flare textures
    Texture2D flare1 = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare").Value;
    Texture2D flare2 = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare3").Value;
    Texture2D flare3 = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare5").Value;
    Texture2D softGlow = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow2").Value;
    
    Vector2 drawPos = Projectile.Center - Main.screenPosition;
    float time = Main.GameUpdateCount * 0.05f;
    float pulse = 1f + (float)Math.Sin(time * 2f) * 0.15f;
    
    // Switch to additive blending
    Main.spriteBatch.End();
    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, ...);
    
    // LAYER 1: Soft glow base (large, dim)
    Main.spriteBatch.Draw(softGlow, drawPos, null, themeColor * 0.3f, 0f, 
        softGlow.Size() / 2f, 0.8f * pulse, SpriteEffects.None, 0f);
    
    // LAYER 2: First flare (spinning clockwise)
    Main.spriteBatch.Draw(flare1, drawPos, null, themeColor * 0.6f, time, 
        flare1.Size() / 2f, 0.5f * pulse, SpriteEffects.None, 0f);
    
    // LAYER 3: Second flare (spinning counter-clockwise, offset)
    Main.spriteBatch.Draw(flare2, drawPos, null, secondaryColor * 0.5f, -time * 0.7f, 
        flare2.Size() / 2f, 0.4f * pulse, SpriteEffects.None, 0f);
    
    // LAYER 4: Third flare (different rotation speed)
    Main.spriteBatch.Draw(flare3, drawPos, null, accentColor * 0.7f, time * 1.3f, 
        flare3.Size() / 2f, 0.35f * pulse, SpriteEffects.None, 0f);
    
    // LAYER 5: Bright white core
    Main.spriteBatch.Draw(flare1, drawPos, null, Color.White * 0.8f, 0f, 
        flare1.Size() / 2f, 0.2f, SpriteEffects.None, 0f);
    
    Main.spriteBatch.End();
    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, ...);
    
    return false; // Don't draw default sprite
}
```

**This creates a SHIMMERING, UNIQUE projectile core instead of a static boring flare.**

### Step 2: The Dense Trail

```csharp
public override void AI()
{
    // DENSE DUST TRAIL - Every single frame!
    for (int i = 0; i < 2; i++)
    {
        Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(6f, 6f);
        Vector2 dustVel = -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1.5f, 1.5f);
        
        // Main trail dust
        Dust main = Dust.NewDustPerfect(dustPos, themeDustType, dustVel, 0, themeColor, 1.5f);
        main.noGravity = true;
        main.fadeIn = 1.2f;
    }
    
    // CONTRASTING SPARKLE - 1 in 2
    if (Main.rand.NextBool(2))
    {
        Dust contrast = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch, 
            -Projectile.velocity * 0.1f, 0, Color.White, 1.0f);
        contrast.noGravity = true;
    }
    
    // FLARES LITTERING THE AIR - 1 in 2
    if (Main.rand.NextBool(2))
    {
        Vector2 flarePos = Projectile.Center + Main.rand.NextVector2Circular(8f, 8f);
        CustomParticles.GenericFlare(flarePos, themeColor, 0.4f, 15);
    }
    
    // COLOR OSCILLATION - Hue shifts over time
    if (Main.rand.NextBool(3))
    {
        float hue = (Main.GameUpdateCount * 0.02f + Main.rand.NextFloat(0.1f)) % 1f;
        // Constrain hue to theme range (e.g., pink range: 0.85-0.95)
        hue = themeHueMin + (hue * (themeHueMax - themeHueMin));
        Color shiftedColor = Main.hslToRgb(hue, 0.9f, 0.75f);
        CustomParticles.GenericFlare(Projectile.Center, shiftedColor, 0.35f, 12);
    }
}
```

### Step 3: Orbiting Music Notes

**This is a MUSIC MOD. Music notes should ORBIT the projectile!**

```csharp
// In AI():
float orbitAngle = Main.GameUpdateCount * 0.08f;
float orbitRadius = 15f + (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 5f;

// Spawn orbiting music notes that LOCK TO the projectile
if (Main.rand.NextBool(8))
{
    for (int i = 0; i < 3; i++)
    {
        float noteAngle = orbitAngle + MathHelper.TwoPi * i / 3f;
        Vector2 noteOffset = noteAngle.ToRotationVector2() * orbitRadius;
        Vector2 notePos = Projectile.Center + noteOffset;
        
        // Note velocity matches projectile + slight outward drift
        Vector2 noteVel = Projectile.velocity * 0.8f + noteAngle.ToRotationVector2() * 0.5f;
        
        // VISIBLE SCALE (0.7f+)
        ThemedParticles.MusicNote(notePos, noteVel, themeColor, 0.75f, 30);
        
        // Sparkle companion
        var sparkle = new SparkleParticle(notePos, noteVel * 0.5f, Color.White * 0.6f, 0.25f, 20);
        MagnumParticleHandler.SpawnParticle(sparkle);
    }
}
```

### Step 4: The Impact

**Impacts should be GLIMMERS, not puffs.**

```csharp
public override void OnKill(int timeLeft)
{
    // ===== CENTRAL GLIMMER =====
    // Multiple layered flares spinning
    for (int layer = 0; layer < 4; layer++)
    {
        float layerScale = 0.3f + layer * 0.15f;
        float layerAlpha = 0.8f - layer * 0.15f;
        float rotation = layer * MathHelper.PiOver4;
        Color layerColor = Color.Lerp(Color.White, themeColor, layer / 4f);
        CustomParticles.GenericFlare(Projectile.Center, layerColor * layerAlpha, layerScale, 18 - layer * 2);
    }
    
    // ===== EXPANDING GLOW RING =====
    for (int ring = 0; ring < 3; ring++)
    {
        Color ringColor = Color.Lerp(themeColor, secondaryColor, ring / 3f);
        CustomParticles.HaloRing(Projectile.Center, ringColor, 0.3f + ring * 0.12f, 12 + ring * 3);
    }
    
    // ===== RADIAL SPARKLE BURST =====
    for (int i = 0; i < 12; i++)
    {
        float angle = MathHelper.TwoPi * i / 12f;
        Vector2 sparkleVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
        Color sparkleColor = Color.Lerp(themeColor, Color.White, i / 12f);
        
        var sparkle = new SparkleParticle(Projectile.Center, sparkleVel, sparkleColor, 0.4f, 25);
        MagnumParticleHandler.SpawnParticle(sparkle);
    }
    
    // ===== DUST EXPLOSION FOR DENSITY =====
    for (int i = 0; i < 15; i++)
    {
        Vector2 dustVel = Main.rand.NextVector2Circular(5f, 5f);
        Dust d = Dust.NewDustPerfect(Projectile.Center, themeDustType, dustVel, 0, themeColor, 1.3f);
        d.noGravity = true;
        d.fadeIn = 1f;
    }
    
    // ===== MUSIC NOTE FINALE =====
    ThemedParticles.MusicNoteBurst(Projectile.Center, themeColor, 6, 4f);
    
    // ===== BRIGHT LIGHTING =====
    Lighting.AddLight(Projectile.Center, themeColor.ToVector3() * 1.5f);
}
```

---

## ⚔️ HOW TO BUILD A PROPER MELEE SWING

### USE THE SWORD ARC ASSETS!

We have **9 SwordArc PNGs**. USE THEM:

```csharp
public override void MeleeEffects(Player player, Rectangle hitbox)
{
    // ===== SWORD ARC SLASH EFFECT =====
    // Use SwordArc textures for the actual slash visual
    if (swingProgress > 0.2f && swingProgress < 0.8f)
    {
        Texture2D arc1 = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SwordArc1").Value;
        Texture2D arc2 = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SwordArc3").Value;
        
        Vector2 arcPos = player.Center + swingDirection * 40f;
        float arcRot = swingDirection.ToRotation();
        
        // Draw layered arcs in PostDraw or use particle system
        // Arc 1 - main slash
        // Arc 2 - secondary glow layer (larger, dimmer, offset rotation)
        // Arc 3 - trailing afterimage
    }
    
    // ===== DENSE DUST TRAIL =====
    for (int i = 0; i < 3; i++)
    {
        Vector2 dustPos = hitbox.Center.ToVector2() + Main.rand.NextVector2Circular(hitbox.Width / 2, hitbox.Height / 2);
        Dust d = Dust.NewDustPerfect(dustPos, themeDustType, 
            player.velocity * 0.3f + Main.rand.NextVector2Circular(2f, 2f), 0, themeColor, 1.5f);
        d.noGravity = true;
        d.fadeIn = 1.3f;
    }
    
    // ===== SPARKLES FOR SHIMMER =====
    if (Main.rand.NextBool(2))
    {
        Vector2 sparklePos = hitbox.Center.ToVector2() + Main.rand.NextVector2Circular(10f, 10f);
        var sparkle = new SparkleParticle(sparklePos, Main.rand.NextVector2Circular(2f, 2f), 
            Color.White * 0.8f, 0.35f, 20);
        MagnumParticleHandler.SpawnParticle(sparkle);
    }
    
    // ===== FREQUENT FLARES =====
    if (Main.rand.NextBool(2))
    {
        Vector2 flarePos = hitbox.Center.ToVector2() + Main.rand.NextVector2Circular(12f, 12f);
        CustomParticles.GenericFlare(flarePos, themeColor, 0.4f, 12);
    }
    
    // ===== COLOR OSCILLATION =====
    if (Main.rand.NextBool(3))
    {
        float hue = (Main.GameUpdateCount * 0.02f) % 1f;
        hue = themeHueMin + hue * (themeHueMax - themeHueMin);
        Color shiftColor = Main.hslToRgb(hue, 0.85f, 0.75f);
        CustomParticles.GenericFlare(hitbox.Center.ToVector2(), shiftColor, 0.35f, 10);
    }
    
    // ===== MUSIC NOTES IN SWING =====
    if (Main.rand.NextBool(5))
    {
        Vector2 notePos = hitbox.Center.ToVector2() + Main.rand.NextVector2Circular(15f, 15f);
        Vector2 noteVel = swingDirection.RotatedByRandom(0.5f) * Main.rand.NextFloat(1f, 3f);
        ThemedParticles.MusicNote(notePos, noteVel, themeColor * 0.9f, 0.8f, 35);
    }
}
```

### Wave Projectiles (The RIGHT Way)

**DO NOT copy-paste a PNG upward and downward. That's not a wave.**

```csharp
// A wave projectile should:
// 1. Use SwordArc textures layered
// 2. Have a flowing, curved trail
// 3. Include bloom and glow
// 4. Have dust for density

public override bool PreDraw(ref Color lightColor)
{
    Texture2D arc = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SwordArc2").Value;
    Texture2D glow = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow3").Value;
    
    Vector2 drawPos = Projectile.Center - Main.screenPosition;
    float rotation = Projectile.velocity.ToRotation();
    
    Main.spriteBatch.End();
    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, ...);
    
    // Glow base
    Main.spriteBatch.Draw(glow, drawPos, null, themeColor * 0.4f, rotation, 
        glow.Size() / 2f, 1.2f, SpriteEffects.None, 0f);
    
    // Main arc
    Main.spriteBatch.Draw(arc, drawPos, null, themeColor * 0.9f, rotation, 
        arc.Size() / 2f, 1f, SpriteEffects.None, 0f);
    
    // Bright edge
    Main.spriteBatch.Draw(arc, drawPos, null, Color.White * 0.6f, rotation, 
        arc.Size() / 2f, 0.8f, SpriteEffects.None, 0f);
    
    // Secondary arc layer (offset)
    Main.spriteBatch.Draw(arc, drawPos, null, secondaryColor * 0.5f, rotation + 0.1f, 
        arc.Size() / 2f, 1.1f, SpriteEffects.None, 0f);
    
    Main.spriteBatch.End();
    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, ...);
    
    return false;
}

public override void AI()
{
    // Flowing curved trail
    for (int i = 0; i < 3; i++)
    {
        Vector2 trailOffset = Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2) 
            * (float)Math.Sin(Projectile.timeLeft * 0.2f + i) * 8f;
        Vector2 trailPos = Projectile.Center + trailOffset - Projectile.velocity * (i * 0.2f);
        
        Dust d = Dust.NewDustPerfect(trailPos, themeDustType, -Projectile.velocity * 0.1f, 0, themeColor, 1.2f);
        d.noGravity = true;
    }
}
```

---

## 🎵 MUSICAL IDENTITY REQUIREMENTS

### Every Theme Projectile MUST Have:

1. **Orbiting or trailing music notes** - Not random spawn, but INTENTIONAL placement
2. **Staff line accents** (where appropriate) - Musical bars flowing behind
3. **Harmonic color shifts** - Colors that oscillate like sound waves
4. **Impact "chord"** - Death effects that feel like a musical resolve

### Music Note Visibility Rules:

| Scale | Visibility | Use Case |
|-------|------------|----------|
| 0.25f | INVISIBLE | Never use |
| 0.4f | Barely visible | Never use |
| 0.6f | Minimum visible | Only for tiny accents |
| 0.7f | Good | Standard trail notes |
| 0.8f | Great | Main projectile notes |
| 1.0f | Bold | Impact/finale notes |

---

## 🌟 THE ARK OF THE COSMOS INSPIRATION

### What Makes Ark of the Cosmos Special:

1. **Sine-wave trails** - Projectiles don't fly straight. They CURVE and FLOW.
2. **Diamond-shaped shards** - Unique projectile shapes, not generic orbs
3. **Homing with grace** - Seeks enemies but in elegant arcs
4. **Trail that BENDS** - The trail follows the curved path

### Implementing Curved Trails:

```csharp
// Store position history for curved trail
private Vector2[] positionHistory = new Vector2[15];
private int historyIndex = 0;

public override void AI()
{
    // Store position
    positionHistory[historyIndex] = Projectile.Center;
    historyIndex = (historyIndex + 1) % positionHistory.Length;
    
    // Sine-wave movement (like Ark of the Cosmos)
    float waveOffset = (float)Math.Sin(Projectile.timeLeft * 0.15f) * 3f;
    Vector2 perpendicular = Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2);
    Projectile.Center += perpendicular * waveOffset * 0.1f;
    
    // Draw curved trail using position history
    for (int i = 0; i < positionHistory.Length - 1; i++)
    {
        int index = (historyIndex + i) % positionHistory.Length;
        int nextIndex = (historyIndex + i + 1) % positionHistory.Length;
        
        if (positionHistory[index] == Vector2.Zero || positionHistory[nextIndex] == Vector2.Zero)
            continue;
        
        float progress = i / (float)positionHistory.Length;
        Color trailColor = Color.Lerp(themeColor, secondaryColor, progress) * (1f - progress);
        float trailScale = 0.3f * (1f - progress * 0.7f);
        
        // Draw dust at each history point
        if (Main.rand.NextBool(2))
        {
            Dust d = Dust.NewDustPerfect(positionHistory[index], themeDustType, Vector2.Zero, 0, trailColor, trailScale * 3f);
            d.noGravity = true;
        }
    }
}
```

---

## 🔥 BRIGHTNESS AND SATURATION

### The Problem: Effects Are Too Dim

**Fix: Increase saturation, use brighter base colors, add white highlights**

```csharp
// ❌ TOO DIM
Color dimColor = themeColor * 0.3f;
CustomParticles.GenericFlare(pos, dimColor, 0.2f, 10);

// ✅ BRIGHT AND VIBRANT
Color brightColor = themeColor; // Full saturation
CustomParticles.GenericFlare(pos, brightColor, 0.5f, 15);
CustomParticles.GenericFlare(pos, Color.White * 0.6f, 0.25f, 12); // White highlight layer
```

### Saturation Guidelines:

| Effect Type | Alpha Multiplier | Scale |
|-------------|------------------|-------|
| Trail dust | 0.8f - 1.0f | 1.2f - 1.8f |
| Flares | 0.5f - 0.8f | 0.3f - 0.5f |
| Glow layers | 0.3f - 0.5f | 0.8f - 1.4f |
| White highlights | 0.4f - 0.8f | 0.15f - 0.3f |
| Impact bursts | 0.7f - 1.0f | 0.4f - 0.6f |

---

## 📋 CHECKLIST FOR EVERY WEAPON EFFECT

Before considering a weapon complete, verify:

### Projectiles:
- [ ] Core uses **multiple layered flares** spinning at different speeds
- [ ] Trail has **dense dust** (2+ per frame)
- [ ] Trail has **contrasting sparkles** (1-in-2)
- [ ] Trail has **frequent flares** littering the air (1-in-2)
- [ ] Colors **oscillate** using Main.hslToRgb within theme hue range
- [ ] **Music notes** orbit or trail the projectile (scale 0.7f+)
- [ ] Impact is a **glimmer**, not a puff (layered flares + rings + sparks)
- [ ] **Lighting** is bright (1.0f+ intensity)

### Melee Swings:
- [ ] Uses **SwordArc textures** for slash visuals
- [ ] Has **dense dust trail** during swing
- [ ] Has **sparkle shimmer** accents
- [ ] Has **color oscillation** within theme range
- [ ] Has **music notes** scattered in swing
- [ ] Wave projectiles use **layered arcs with glow**, not PNG copy-paste

### Impacts/Explosions:
- [ ] Central **glimmer** (multiple spinning flares)
- [ ] **Expanding rings** (3+ layers, gradient colors)
- [ ] **Radial sparkle burst**
- [ ] **Dust explosion** for density
- [ ] **Music note finale**
- [ ] **Bright lighting** pulse

---

## 🎯 FINAL WORDS

**The weapons look great statically. But when you USE them, they fall apart.**

That ends now.

Every projectile should be a **spinning, shimmering, glowing, musically-accompanied visual symphony**.

Not a translucent orb that puffs away.

Not a single flare slapped on a PreDraw.

Not a rigid, non-flowing trail.

**LAYER IT. SPIN IT. SHIMMER IT. MAKE IT MUSICAL. MAKE IT BRIGHT. MAKE IT UNIQUE.**

Swan Lake is the standard. Iridescent Wingspan is the template. Ark of the Cosmos is the inspiration.

Now make every other theme live up to that.
