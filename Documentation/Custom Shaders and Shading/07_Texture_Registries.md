# Texture Registries Reference

> **Complete documentation of MiscTexturesRegistry and NoiseTexturesRegistry from FargosSoulsDLC.**
> 
> These registries provide essential textures for VFX rendering.

---

## MiscTexturesRegistry

### Overview

The `MiscTexturesRegistry` provides pre-loaded textures for common VFX elements.

```csharp
// Usage pattern
Texture2D bloom = MiscTexturesRegistry.BloomCircleSmall.Value;
Main.spriteBatch.Draw(bloom, position, null, color, rotation, bloom.Size() * 0.5f, scale, 0, 0f);
```

---

### Bloom Textures

#### BloomCircleSmall

**Path:** `Assets/Textures/BloomCircleSmall.png`

**Description:** Soft circular glow texture, the most commonly used bloom texture.

**Characteristics:**
- Gaussian falloff from center
- Fully white (tint with color)
- Smooth alpha gradient

**Common Uses:**
```csharp
// Basic bloom
Texture2D bloom = MiscTexturesRegistry.BloomCircleSmall.Value;
Main.spriteBatch.Draw(bloom, drawPos, null, 
    Color.Cyan with { A = 0 } * 0.5f,  // Remove alpha channel for additive
    0f, bloom.Size() * 0.5f, 1f, 0, 0f);

// Multi-layer bloom stack
for (int i = 0; i < 4; i++)
{
    float layerScale = 1f + i * 0.3f;
    float layerOpacity = 0.5f / (i + 1);
    Main.spriteBatch.Draw(bloom, drawPos, null,
        color with { A = 0 } * layerOpacity,
        0f, bloom.Size() * 0.5f, layerScale, 0, 0f);
}
```

---

#### BloomCircleMedium / BloomCircleLarge

**Description:** Larger variants for bigger effects.

**Use Cases:**
- Boss explosions
- Large impact effects
- Environmental lighting

---

### Flare Textures

#### ShineFlareTexture

**Path:** `Assets/Textures/ShineFlare.png`

**Description:** 4-pointed star flare for bright highlights.

**Characteristics:**
- Sharp pointed rays
- Bright center
- Used for gleaming/shining effects

**Common Uses:**
```csharp
// Electric gleam on metal
Texture2D shine = MiscTexturesRegistry.ShineFlareTexture.Value;
float gleamAngle = Main.GlobalTimeWrappedHourly * 2f;
float gleamIntensity = 0.5f + MathF.Sin(Main.GlobalTimeWrappedHourly * 5f) * 0.3f;

Main.spriteBatch.Draw(shine, position - Main.screenPosition, null,
    Color.Cyan with { A = 0 } * gleamIntensity,
    gleamAngle, shine.Size() * 0.5f, 0.5f, 0, 0f);

// Weapon highlight
Main.spriteBatch.Draw(shine, weaponTip - Main.screenPosition, null,
    Color.White with { A = 0 } * 0.8f,
    0f, shine.Size() * 0.5f, 0.3f, 0, 0f);
```

---

#### CrossFlareTexture

**Description:** Cross/plus shaped flare.

**Use Cases:**
- Impact points
- Energy crossings
- Intersection highlights

---

### Utility Textures

#### Pixel

**Path:** `Assets/Textures/Pixel.png`

**Description:** Single white pixel, scaled for various uses.

**Common Uses:**
```csharp
// Full-screen shader quad
Texture2D pixel = MiscTexturesRegistry.Pixel.Value;
Main.spriteBatch.Draw(pixel, Vector2.Zero, null, Color.White,
    0f, Vector2.Zero, new Vector2(Main.screenWidth, Main.screenHeight), 0, 0f);

// Line drawing
Vector2 direction = (endPos - startPos);
float length = direction.Length();
float rotation = direction.ToRotation();
Main.spriteBatch.Draw(pixel, startPos - Main.screenPosition, null,
    lineColor, rotation, new Vector2(0, 0.5f), new Vector2(length, lineWidth), 0, 0f);
```

---

#### InvisiblePixel

**Description:** Fully transparent pixel for placeholder/invisible projectiles.

**Use Cases:**
```csharp
// Invisible projectile that relies entirely on particles
public override string Texture => MiscTexturesRegistry.InvisiblePixel.Path;
```

---

#### BloomLineTexture

**Description:** Horizontal line with bloom falloff on edges.

**Common Uses:**
```csharp
// Laser/beam rendering
Texture2D line = MiscTexturesRegistry.BloomLineTexture.Value;
float rotation = beamDirection.ToRotation();
Vector2 scale = new Vector2(beamLength / line.Width, beamWidth / line.Height);

Main.spriteBatch.Draw(line, beamStart - Main.screenPosition, null,
    beamColor with { A = 0 }, rotation, new Vector2(0, line.Height * 0.5f), scale, 0, 0f);
```

---

### Noise Textures (in MiscTexturesRegistry)

#### WavyBlotchNoise

**Description:** Organic, wavy noise pattern.

**Characteristics:**
- Medium-frequency blobs
- Smooth transitions
- Good for distortion

**Shader Uses:**
```hlsl
sampler wavyNoise : register(s1);
float noise = tex2D(wavyNoise, coords * 2 + time * 0.1).r;
coords += noise * distortionAmount;
```

**C# Binding:**
```csharp
shader.SetTexture(MiscTexturesRegistry.WavyBlotchNoise.Value, 1, SamplerState.LinearWrap);
```

---

#### DendriticNoise

**Description:** Branch-like, tree/vein noise pattern.

**Characteristics:**
- Organic branching structure
- Good for lightning, cracks, veins

**Shader Uses:**
```hlsl
// Lightning-like effect
float dendrite = tex2D(dendriticNoise, coords);
float lightning = step(0.7, dendrite); // Sharp threshold
```

---

### Gradient Textures

#### HorizontalGradient

**Description:** Black to white horizontal gradient.

**Use Cases:**
```csharp
// Smooth fade effect
// Primitive trail UV mapping
```

---

## NoiseTexturesRegistry

### Overview

Provides specialized noise textures for shader effects.

```csharp
// Usage pattern
shader.SetTexture(NoiseTexturesRegistry.PerlinNoise.Value, 1, SamplerState.LinearWrap);
```

---

### Core Noise Textures

#### PerlinNoise

**Path:** `Assets/Textures/Noise/PerlinNoise.png`

**Description:** Classic Perlin noise, smooth organic variation.

**Characteristics:**
- Smooth gradients
- No sharp edges
- Good for general-purpose noise

**Shader Uses:**
```hlsl
// Basic noise sampling
float noise = tex2D(perlinNoise, coords * noiseScale);

// Scrolling noise for animation
float animatedNoise = tex2D(perlinNoise, coords + float2(time * 0.1, 0));

// Octave blending
float n1 = tex2D(perlinNoise, coords);
float n2 = tex2D(perlinNoise, coords * 2) * 0.5;
float n3 = tex2D(perlinNoise, coords * 4) * 0.25;
float combinedNoise = n1 + n2 + n3;
```

**Common Applications:**
- Flame/fire effects
- Cloud/smoke movement
- General distortion
- Dissolve effects

---

#### BubblyNoise

**Path:** `Assets/Textures/Noise/BubblyNoise.png`

**Description:** Cellular/bubbly noise pattern.

**Characteristics:**
- Circular blob shapes
- Cell-like structure
- Sharp-ish edges between cells

**Shader Uses:**
```hlsl
// Metaball surface detail
float bubbles = tex2D(bubblyNoise, worldUV * 3);
surfaceColor *= 0.8 + bubbles * 0.4;

// Dissolve with cell-like holes
float dissolveNoise = tex2D(bubblyNoise, coords);
clip(dissolveNoise - dissolveProgress);
```

**Common Applications:**
- Bile/slime surfaces
- Metaball detail
- Organic dissolve
- Water caustics

---

#### ElectricNoise

**Path:** `Assets/Textures/Noise/ElectricNoise.png`

**Description:** Jagged, electric/lightning-like noise.

**Characteristics:**
- Sharp, branching patterns
- High contrast
- Good for electrical effects

**Shader Uses:**
```hlsl
// Electric arcs
float electric = tex2D(electricNoise, coords * float2(4, 1) + float2(time, 0));
float arc = step(0.6, electric);

// Tesla explosion
float intensity = tex2D(electricNoise, coords * 3 + lifetimeRatio);
ringColor += intensity * 0.3;
```

**Common Applications:**
- Tesla/electric explosions
- Lightning overlays
- Energy weapon edges
- Plasma effects

---

#### CrackedNoiseA / CrackedNoiseB

**Path:** `Assets/Textures/Noise/CrackedNoiseA.png`

**Description:** Cracked/fractured noise pattern.

**Characteristics:**
- Sharp crack lines
- Voronoi-like cells
- Good for damage/destruction

**Shader Uses:**
```hlsl
// Laser edge cracks
float cracks = tex2D(crackedNoise, coords * float2(4, 1) + float2(scrollOffset, 0));
float highlight = pow(cracks, 2);
laserColor += highlight * glowIntensity;

// Shattering effect
float shatter = tex2D(crackedNoise, coords);
float crackIntensity = smoothstep(shatterProgress - 0.1, shatterProgress, shatter);
```

**Common Applications:**
- Laser beam detail
- Breaking/shattering effects
- Damage cracks
- Lava/magma veins

---

### Fire Textures

#### FireParticleA / FireParticleB

**Path:** `Assets/Textures/Noise/FireParticleA.png`, `FireParticleB.png`

**Description:** Animated fire particle frames.

**Characteristics:**
- Flame-shaped alpha
- Turbulent detail
- Designed for particle animation

**Shader Uses:**
```hlsl
// Blend between two fire frames
float4 fireA = tex2D(fireTextureA, coords);
float4 fireB = tex2D(fireTextureB, coords);
float4 fire = lerp(fireA, fireB, sin(lifetime * 3.14) * 0.5 + 0.5);
```

**Common Applications:**
- Fire particle rendering
- Flame jet trails
- Explosion particles
- Thruster effects

---

### Specialized Noise Textures

#### BinaryPoem

**Description:** High-contrast binary/digital noise.

**Use Cases:**
- Glitch effects
- Digital corruption
- Hologram noise

---

#### CloudDensityMap

**Description:** Large-scale cloud density pattern.

**Use Cases:**
- Sky rendering
- Fog density
- Large-scale atmospheric effects

---

## Texture Loading Patterns

### Lazy Loading with Asset Requests

```csharp
public static class MiscTexturesRegistry
{
    private static Asset<Texture2D> _bloomCircleSmall;
    
    public static Asset<Texture2D> BloomCircleSmall
    {
        get
        {
            _bloomCircleSmall ??= ModContent.Request<Texture2D>("ModName/Assets/Textures/BloomCircleSmall");
            return _bloomCircleSmall;
        }
    }
    
    // Usage: MiscTexturesRegistry.BloomCircleSmall.Value
}
```

### Immediate Loading

```csharp
public override void Load()
{
    // Load immediately for textures used constantly
    BloomTexture = ModContent.Request<Texture2D>("Path/To/Bloom", AssetRequestMode.ImmediateLoad).Value;
}
```

---

## Sampler State Usage

### LinearWrap (Most Common)

```csharp
shader.SetTexture(texture, 1, SamplerState.LinearWrap);
```

**When to Use:**
- Noise textures that need tiling
- Scrolling textures
- Any texture sampled outside [0,1] UV range

---

### LinearClamp

```csharp
shader.SetTexture(texture, 1, SamplerState.LinearClamp);
```

**When to Use:**
- Render targets
- Screen-space textures
- Textures that should NOT repeat

---

### PointClamp (Pixelated)

```csharp
shader.SetTexture(texture, 1, SamplerState.PointClamp);
```

**When to Use:**
- Pixelated effects
- Preserving sharp pixel edges
- Low-res aesthetic

---

## MagnumOpus Texture Registry

### Recommended Structure

```csharp
namespace MagnumOpus.Common.Systems
{
    public static class MagnumTextureRegistry
    {
        // Bloom textures
        public static Asset<Texture2D> BloomCircle { get; private set; }
        public static Asset<Texture2D> BloomLine { get; private set; }
        
        // Flare textures
        public static Asset<Texture2D> ShineFlare { get; private set; }
        public static Asset<Texture2D> CrossFlare { get; private set; }
        
        // Noise textures
        public static Asset<Texture2D> PerlinNoise { get; private set; }
        public static Asset<Texture2D> WavyNoise { get; private set; }
        public static Asset<Texture2D> CrackedNoise { get; private set; }
        public static Asset<Texture2D> ElectricNoise { get; private set; }
        
        // Theme-specific textures
        public static Asset<Texture2D> MusicNoteParticle { get; private set; }
        public static Asset<Texture2D> GlyphParticle { get; private set; }
        public static Asset<Texture2D> FeatherParticle { get; private set; }
        public static Asset<Texture2D> SakuraPetalParticle { get; private set; }
        
        // Utility
        public static Asset<Texture2D> Pixel { get; private set; }
        public static Asset<Texture2D> InvisiblePixel { get; private set; }
        
        public static void Load(Mod mod)
        {
            string basePath = "MagnumOpus/Assets/Particles/";
            string noisePath = "MagnumOpus/Assets/Noise/";
            
            // Load bloom textures
            BloomCircle = ModContent.Request<Texture2D>(basePath + "SoftGlow");
            BloomLine = ModContent.Request<Texture2D>(basePath + "BloomLine");
            
            // Load flares
            ShineFlare = ModContent.Request<Texture2D>(basePath + "EnergyFlare");
            CrossFlare = ModContent.Request<Texture2D>(basePath + "CrossFlare");
            
            // Load noise
            PerlinNoise = ModContent.Request<Texture2D>(noisePath + "PerlinNoise");
            WavyNoise = ModContent.Request<Texture2D>(noisePath + "WavyNoise");
            CrackedNoise = ModContent.Request<Texture2D>(noisePath + "CrackedNoise");
            ElectricNoise = ModContent.Request<Texture2D>(noisePath + "ElectricNoise");
            
            // Load theme particles
            MusicNoteParticle = ModContent.Request<Texture2D>(basePath + "MusicNote");
            GlyphParticle = ModContent.Request<Texture2D>(basePath + "Glyphs1");
            FeatherParticle = ModContent.Request<Texture2D>(basePath + "Feather");
            SakuraPetalParticle = ModContent.Request<Texture2D>(basePath + "SakuraPetal");
            
            // Utility
            Pixel = ModContent.Request<Texture2D>(basePath + "Pixel");
            InvisiblePixel = ModContent.Request<Texture2D>(basePath + "Invisible");
        }
        
        public static void Unload()
        {
            BloomCircle = null;
            BloomLine = null;
            // ... etc
        }
    }
}
```

### Usage in MagnumOpus

```csharp
// In particle rendering
Texture2D bloom = MagnumTextureRegistry.BloomCircle.Value;
Main.spriteBatch.Draw(bloom, position, null, color with { A = 0 }, ...);

// In shader setup
shader.SetTexture(MagnumTextureRegistry.PerlinNoise.Value, 1, SamplerState.LinearWrap);
shader.SetTexture(MagnumTextureRegistry.CrackedNoise.Value, 2, SamplerState.LinearWrap);
```

---

## Texture Requirements for MagnumOpus

### Must-Have Textures

| Texture | Purpose | Existing? |
|---------|---------|-----------|
| SoftGlow2-4.png | Bloom circle | ✅ Yes |
| EnergyFlare.png, EnergyFlare4.png | Bright flare (2 variants) | ✅ Yes |
| MusicNote.png + 5 named variants | Musical particles (6 total) | ✅ Yes |
| Glyphs1-12.png | Arcane glyphs | ✅ Yes |
| EnigmaEye1.png + 7 named variants | Enigma theme eyes (8 total) | ✅ Yes |

### Textures to Create

| Texture | Purpose | Priority |
|---------|---------|----------|
| BloomLine.png | Beam/laser base | High |
| ShineFlare.png | 4-point star | High |
| PerlinNoise.png | General noise | High |
| CrackedNoise.png | Laser detail | Medium |
| ElectricNoise.png | Electric effects | Medium |
| WavyNoise.png | Distortion | Medium |
| Pixel.png | 1x1 white pixel | High |
| Invisible.png | 1x1 transparent | High |

### Noise Texture Specifications

```
All noise textures should be:
- Size: 256x256 or 512x512
- Format: PNG, grayscale
- Tileable: Yes (seamless edges)
- Bit depth: 8-bit grayscale

PerlinNoise: Smooth organic gradients
CrackedNoise: Sharp cell boundaries, Voronoi-like
ElectricNoise: Jagged branching patterns
WavyNoise: Medium-frequency organic blobs
```

---

*Extracted from FargosSoulsDLC for MagnumOpus VFX development reference.*
