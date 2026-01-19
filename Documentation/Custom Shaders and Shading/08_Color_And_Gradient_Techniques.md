# Color and Gradient Techniques

> **Advanced color manipulation, gradient systems, and palette management from FargosSoulsDLC.**

---

## Color Fundamentals

### The `with { A = 0 }` Pattern

**The most important color pattern for additive blending:**

```csharp
// CRITICAL: Remove alpha channel for proper additive blending
Color glowColor = baseColor with { A = 0 };

// Why this matters:
// - Additive blending adds RGB values together
// - Alpha channel in additive blend causes incorrect results
// - Setting A = 0 ensures pure color addition
```

**Comparison:**
```csharp
// ❌ WRONG - Alpha interferes with additive blend
Main.spriteBatch.Draw(bloom, pos, null, Color.Cyan, ...);

// ✅ CORRECT - Pure additive color
Main.spriteBatch.Draw(bloom, pos, null, Color.Cyan with { A = 0 }, ...);

// ✅ ALSO CORRECT - With opacity multiplier
Main.spriteBatch.Draw(bloom, pos, null, Color.Cyan with { A = 0 } * 0.5f, ...);
```

---

### Color Multiplication for Opacity

```csharp
// Multiply entire color (including RGB) by factor
Color faded = baseColor * 0.5f;  // 50% brightness + 50% alpha

// Multiply just alpha
Color semiTransparent = baseColor * new Color(1f, 1f, 1f, 0.5f);

// For additive bloom - multiply after removing alpha
Color glowFaded = Color.Cyan with { A = 0 } * 0.7f;
```

---

## Gradient Lerping Patterns

### Basic Color.Lerp

```csharp
// Linear interpolation between two colors
float progress = 0.5f; // 0 to 1
Color result = Color.Lerp(Color.Red, Color.Blue, progress);

// Usage in particle lifetime
float lifetimeProgress = particle.Time / (float)particle.Lifetime;
Color currentColor = Color.Lerp(startColor, endColor, lifetimeProgress);
```

### Multi-Step Gradients

```csharp
// 3-color gradient: Red -> Yellow -> White
public static Color ThreeColorGradient(float progress)
{
    if (progress < 0.5f)
        return Color.Lerp(Color.Red, Color.Yellow, progress * 2f);
    else
        return Color.Lerp(Color.Yellow, Color.White, (progress - 0.5f) * 2f);
}

// 4-color gradient
public static Color FourColorGradient(float progress, Color c1, Color c2, Color c3, Color c4)
{
    if (progress < 0.33f)
        return Color.Lerp(c1, c2, progress * 3f);
    else if (progress < 0.66f)
        return Color.Lerp(c2, c3, (progress - 0.33f) * 3f);
    else
        return Color.Lerp(c3, c4, (progress - 0.66f) * 3f);
}
```

### Dynamic Palette Array

```csharp
public static Color PaletteLerp(Color[] palette, float progress)
{
    float scaledProgress = progress * (palette.Length - 1);
    int startIndex = (int)scaledProgress;
    int endIndex = Math.Min(startIndex + 1, palette.Length - 1);
    float localProgress = scaledProgress - startIndex;
    
    return Color.Lerp(palette[startIndex], palette[endIndex], localProgress);
}

// Usage
Color[] firePalette = { Color.Yellow, Color.Orange, Color.Red, Color.DarkRed };
Color fireColor = PaletteLerp(firePalette, particle.LifetimeProgress);
```

---

## HLSL Palette Lerping

### Basic PaletteLerp Function

```hlsl
float gradientCount;
float3 gradient[8];  // Up to 8 colors

float3 PaletteLerp(float interpolant)
{
    // Clamp to valid range
    interpolant = saturate(interpolant);
    
    // Calculate indices
    float scaledInterpolant = interpolant * (gradientCount - 1);
    int startIndex = (int)scaledInterpolant;
    int endIndex = min(startIndex + 1, gradientCount - 1);
    
    // Local interpolation
    float localInterpolant = frac(scaledInterpolant);
    
    return lerp(gradient[startIndex], gradient[endIndex], localInterpolant);
}
```

### Setting Gradient in C#

```csharp
// Create gradient array
Vector3[] gradient = new Vector3[]
{
    new Color(255, 200, 100).ToVector3(),  // Bright
    new Color(255, 100, 50).ToVector3(),   // Medium
    new Color(200, 50, 20).ToVector3(),    // Dark
    new Color(100, 20, 10).ToVector3(),    // Very dark
};

// Set shader parameters
shader.TrySetParameter("gradient", gradient);
shader.TrySetParameter("gradientCount", (float)gradient.Length);
```

---

## HSL Color Manipulation

### Converting to/from HSL

```csharp
// Terraria's built-in HSL conversion
float hue = 0.5f;        // 0-1 range
float saturation = 1.0f; // 0-1 range  
float lightness = 0.5f;  // 0-1 range

Color hslColor = Main.hslToRgb(hue, saturation, lightness);
```

### Rainbow Cycling

```csharp
// Smooth rainbow cycle based on time
public static Color GetRainbowColor(float timeOffset = 0f)
{
    float hue = (Main.GlobalTimeWrappedHourly * 0.5f + timeOffset) % 1f;
    return Main.hslToRgb(hue, 1f, 0.6f);
}

// Rainbow with specific cycle speed
public static Color GetRainbowColor(float cycleSpeed, float offset)
{
    float hue = (Main.GlobalTimeWrappedHourly * cycleSpeed + offset) % 1f;
    return Main.hslToRgb(hue, 1f, 0.6f);
}

// Usage in radial burst
for (int i = 0; i < 12; i++)
{
    float hueOffset = (float)i / 12f;
    Color rainbowColor = GetRainbowColor(hueOffset);
    SpawnParticle(position, velocity, rainbowColor);
}
```

### Hue Shifting

```csharp
// Shift hue of existing color
public static Color ShiftHue(Color baseColor, float hueShift)
{
    // Convert to HSL (approximate)
    float r = baseColor.R / 255f;
    float g = baseColor.G / 255f;
    float b = baseColor.B / 255f;
    
    float max = Math.Max(r, Math.Max(g, b));
    float min = Math.Min(r, Math.Min(g, b));
    float lightness = (max + min) / 2f;
    
    float saturation = 0;
    float hue = 0;
    
    if (max != min)
    {
        float d = max - min;
        saturation = lightness > 0.5f ? d / (2f - max - min) : d / (max + min);
        
        if (max == r)
            hue = (g - b) / d + (g < b ? 6f : 0f);
        else if (max == g)
            hue = (b - r) / d + 2f;
        else
            hue = (r - g) / d + 4f;
        
        hue /= 6f;
    }
    
    // Apply shift
    hue = (hue + hueShift) % 1f;
    if (hue < 0) hue += 1f;
    
    return Main.hslToRgb(hue, saturation, lightness);
}
```

---

## Theme Color Systems

### FargosSoulsDLC ExoMech Palette Example

```csharp
public static class ExoMechPalette
{
    // Exo palette - shifts through these colors over time
    public static readonly Color[] ExoColors = new Color[]
    {
        new Color(250, 200, 100),  // Gold
        new Color(255, 150, 50),   // Orange
        new Color(255, 100, 100),  // Red-orange
        new Color(255, 50, 100),   // Pink-red
        new Color(200, 50, 150),   // Magenta
        new Color(150, 50, 200),   // Purple
        new Color(100, 100, 255),  // Blue
        new Color(50, 150, 255),   // Cyan-blue
        new Color(50, 200, 200),   // Cyan
        new Color(100, 255, 150),  // Green-cyan
        new Color(150, 255, 100),  // Green
        new Color(200, 255, 50),   // Yellow-green
    };
    
    public static Color GetExoColor(float offset = 0f)
    {
        float time = Main.GlobalTimeWrappedHourly * 0.5f + offset;
        return PaletteLerp(ExoColors, time % 1f);
    }
}
```

### MagnumOpus Theme Palettes

```csharp
public static class MagnumThemePalettes
{
    // La Campanella - Black to Orange infernal
    public static readonly Color[] LaCampanella = new Color[]
    {
        new Color(20, 15, 20),     // Deep black
        new Color(80, 30, 20),     // Dark ember
        new Color(150, 60, 20),    // Ember
        new Color(255, 100, 0),    // Bright orange
        new Color(255, 200, 50),   // Golden flame tip
    };
    
    // Eroica - Scarlet to Gold heroic
    public static readonly Color[] Eroica = new Color[]
    {
        new Color(100, 20, 20),    // Deep scarlet
        new Color(180, 40, 40),    // Rich red
        new Color(220, 80, 50),    // Red-orange
        new Color(255, 150, 50),   // Orange-gold
        new Color(255, 215, 100),  // Bright gold
    };
    
    // Swan Lake - Monochrome with prismatic
    public static readonly Color[] SwanLake = new Color[]
    {
        new Color(255, 255, 255),  // Pure white
        new Color(220, 220, 230),  // Silver
        new Color(180, 180, 200),  // Gray-blue
        new Color(100, 100, 120),  // Dark gray
        new Color(30, 30, 40),     // Near black
    };
    
    // Moonlight Sonata - Purple to Blue lunar
    public static readonly Color[] MoonlightSonata = new Color[]
    {
        new Color(50, 20, 80),     // Deep purple
        new Color(80, 40, 140),    // Purple
        new Color(100, 80, 180),   // Violet
        new Color(120, 140, 220),  // Lavender-blue
        new Color(180, 200, 255),  // Pale blue
    };
    
    // Enigma Variations - Black to Purple to Green
    public static readonly Color[] EnigmaVariations = new Color[]
    {
        new Color(15, 10, 20),     // Void black
        new Color(60, 20, 80),     // Deep purple
        new Color(120, 50, 160),   // Purple
        new Color(80, 150, 100),   // Teal transition
        new Color(50, 220, 100),   // Eerie green
    };
    
    // Fate - Black to Pink to Red cosmic
    public static readonly Color[] Fate = new Color[]
    {
        new Color(15, 5, 20),      // Cosmic black
        new Color(80, 20, 60),     // Dark magenta
        new Color(180, 50, 100),   // Dark pink
        new Color(255, 80, 120),   // Bright pink
        new Color(255, 60, 80),    // Bright red
        new Color(255, 255, 255),  // White flash
    };
    
    // Helper to get color from any palette
    public static Color GetThemeColor(Color[] palette, float progress)
    {
        progress = Math.Clamp(progress, 0f, 1f);
        float scaledProgress = progress * (palette.Length - 1);
        int startIndex = (int)scaledProgress;
        int endIndex = Math.Min(startIndex + 1, palette.Length - 1);
        float localProgress = scaledProgress - startIndex;
        
        return Color.Lerp(palette[startIndex], palette[endIndex], localProgress);
    }
}
```

---

## Color Animation Patterns

### Pulsing Color

```csharp
// Sinusoidal pulse between two colors
public static Color PulsingColor(Color baseColor, Color peakColor, float speed = 1f)
{
    float pulse = (MathF.Sin(Main.GlobalTimeWrappedHourly * speed * MathHelper.TwoPi) + 1f) * 0.5f;
    return Color.Lerp(baseColor, peakColor, pulse);
}

// Usage
Color heartbeatRed = PulsingColor(Color.DarkRed, Color.Red, 2f);
```

### Breathing Opacity

```csharp
// Smooth opacity variation
public static float BreathingOpacity(float minOpacity, float maxOpacity, float speed = 1f)
{
    float breath = (MathF.Sin(Main.GlobalTimeWrappedHourly * speed * MathHelper.TwoPi) + 1f) * 0.5f;
    return MathHelper.Lerp(minOpacity, maxOpacity, breath);
}

// Usage
float glowOpacity = BreathingOpacity(0.3f, 0.8f, 1.5f);
Main.spriteBatch.Draw(bloom, pos, null, Color.Cyan with { A = 0 } * glowOpacity, ...);
```

### Flickering

```csharp
// Random flicker effect
public static float FlickerIntensity(float baseIntensity, float flickerAmount)
{
    // Smooth random-ish flicker using sin waves at different frequencies
    float flicker1 = MathF.Sin(Main.GlobalTimeWrappedHourly * 20f);
    float flicker2 = MathF.Sin(Main.GlobalTimeWrappedHourly * 37f);
    float flicker3 = MathF.Sin(Main.GlobalTimeWrappedHourly * 53f);
    float combinedFlicker = (flicker1 + flicker2 + flicker3) / 3f;
    
    return baseIntensity + combinedFlicker * flickerAmount;
}

// Usage for fire
float fireIntensity = FlickerIntensity(0.8f, 0.2f);
```

### Color Strobe

```csharp
// Sharp color switching
public static Color StrobeColor(Color[] colors, float frequency)
{
    int index = (int)(Main.GlobalTimeWrappedHourly * frequency) % colors.Length;
    return colors[index];
}

// Smooth transition strobe
public static Color SmoothStrobe(Color[] colors, float frequency)
{
    float progress = (Main.GlobalTimeWrappedHourly * frequency) % colors.Length;
    int index = (int)progress;
    int nextIndex = (index + 1) % colors.Length;
    float localProgress = progress - index;
    
    return Color.Lerp(colors[index], colors[nextIndex], localProgress);
}
```

---

## Spatial Color Variations

### Radial Gradient

```csharp
// Color based on distance from center
public static Color RadialGradient(Vector2 position, Vector2 center, float radius, 
    Color innerColor, Color outerColor)
{
    float distance = Vector2.Distance(position, center);
    float progress = Math.Clamp(distance / radius, 0f, 1f);
    return Color.Lerp(innerColor, outerColor, progress);
}
```

### Angular Gradient

```csharp
// Color based on angle from center
public static Color AngularGradient(Vector2 position, Vector2 center, Color[] palette)
{
    Vector2 offset = position - center;
    float angle = MathF.Atan2(offset.Y, offset.X);
    float progress = (angle + MathHelper.Pi) / MathHelper.TwoPi; // Normalize to 0-1
    
    return MagnumThemePalettes.GetThemeColor(palette, progress);
}
```

### Spiral Gradient

```csharp
// Combines radial and angular for spiral effect
public static Color SpiralGradient(Vector2 position, Vector2 center, float radius,
    Color[] palette, float spiralTightness = 2f)
{
    Vector2 offset = position - center;
    float distance = offset.Length();
    float angle = MathF.Atan2(offset.Y, offset.X);
    
    // Combine angle and distance for spiral
    float spiralProgress = (angle / MathHelper.TwoPi + distance / radius * spiralTightness) % 1f;
    if (spiralProgress < 0) spiralProgress += 1f;
    
    return MagnumThemePalettes.GetThemeColor(palette, spiralProgress);
}
```

---

## Particle System Color Patterns

### Gradient Burst Pattern

```csharp
// Radial burst with gradient colors
public static void GradientBurst(Vector2 center, Color[] palette, int count, float speed)
{
    for (int i = 0; i < count; i++)
    {
        float angle = MathHelper.TwoPi * i / count;
        float progress = (float)i / count;
        
        Color particleColor = MagnumThemePalettes.GetThemeColor(palette, progress);
        Vector2 velocity = angle.ToRotationVector2() * speed;
        
        GeneralParticleHandler.SpawnParticle(new GlowySquareParticle(
            center, velocity, particleColor, 0.5f, 25, true));
    }
}
```

### Lifetime Color Transition

```csharp
public class GradientParticle : Particle
{
    public Color[] Palette;
    
    public override void Update()
    {
        Time++;
        float progress = Time / (float)Lifetime;
        
        // Get color from palette based on lifetime
        Color = MagnumThemePalettes.GetThemeColor(Palette, progress);
        
        // Fade out near end
        if (progress > 0.7f)
            Color *= 1f - (progress - 0.7f) / 0.3f;
        
        if (Time >= Lifetime)
            Kill();
    }
}
```

### Random Variation Within Theme

```csharp
// Get random color from theme's range
public static Color GetRandomThemeColor(Color[] palette)
{
    float progress = Main.rand.NextFloat();
    return MagnumThemePalettes.GetThemeColor(palette, progress);
}

// Get color with slight random variation
public static Color GetVariedThemeColor(Color baseColor, float variation = 0.1f)
{
    float rVar = Main.rand.NextFloat(-variation, variation);
    float gVar = Main.rand.NextFloat(-variation, variation);
    float bVar = Main.rand.NextFloat(-variation, variation);
    
    return new Color(
        Math.Clamp(baseColor.R / 255f + rVar, 0f, 1f),
        Math.Clamp(baseColor.G / 255f + gVar, 0f, 1f),
        Math.Clamp(baseColor.B / 255f + bVar, 0f, 1f)
    );
}
```

---

## HLSL Color Techniques

### Smooth Color Transitions in Shaders

```hlsl
// Smooth step for color transitions
float3 SmoothColorLerp(float3 colorA, float3 colorB, float t)
{
    // Smoother than linear lerp
    float smoothT = t * t * (3 - 2 * t);  // Smoothstep formula
    return lerp(colorA, colorB, smoothT);
}
```

### Additive Glow Calculation

```hlsl
// Calculate additive glow color
float4 CalculateGlow(float4 baseColor, float glowIntensity)
{
    // Remove alpha for additive
    float3 glowColor = baseColor.rgb * glowIntensity;
    return float4(glowColor, 0);  // Alpha = 0 for additive
}
```

### Color Grading in Shader

```hlsl
// Apply color grade/tint
float4 ApplyColorGrade(float4 color, float3 tint, float intensity)
{
    float3 graded = lerp(color.rgb, color.rgb * tint, intensity);
    return float4(graded, color.a);
}

// Desaturate
float3 Desaturate(float3 color, float amount)
{
    float gray = dot(color, float3(0.299, 0.587, 0.114));
    return lerp(color, float3(gray, gray, gray), amount);
}
```

---

## MagnumOpus Color Utilities

### Complete Theme Color Helper

```csharp
namespace MagnumOpus.Common.Systems
{
    public static class ThemeColors
    {
        // Quick access to theme-appropriate colors
        
        public static Color GetLaCampanella(float progress) =>
            MagnumThemePalettes.GetThemeColor(MagnumThemePalettes.LaCampanella, progress);
            
        public static Color GetEroica(float progress) =>
            MagnumThemePalettes.GetThemeColor(MagnumThemePalettes.Eroica, progress);
            
        public static Color GetSwanLake(float progress) =>
            MagnumThemePalettes.GetThemeColor(MagnumThemePalettes.SwanLake, progress);
            
        public static Color GetMoonlightSonata(float progress) =>
            MagnumThemePalettes.GetThemeColor(MagnumThemePalettes.MoonlightSonata, progress);
            
        public static Color GetEnigmaVariations(float progress) =>
            MagnumThemePalettes.GetThemeColor(MagnumThemePalettes.EnigmaVariations, progress);
            
        public static Color GetFate(float progress) =>
            MagnumThemePalettes.GetThemeColor(MagnumThemePalettes.Fate, progress);
        
        // Get pulse color for theme
        public static Color GetPulsingThemeColor(Color[] palette, float speed = 1f)
        {
            float pulse = (MathF.Sin(Main.GlobalTimeWrappedHourly * speed * MathHelper.TwoPi) + 1f) * 0.5f;
            return MagnumThemePalettes.GetThemeColor(palette, pulse);
        }
        
        // Swan Lake rainbow shimmer
        public static Color GetSwanRainbow(float offset = 0f)
        {
            float hue = (Main.GlobalTimeWrappedHourly * 0.3f + offset) % 1f;
            Color rainbow = Main.hslToRgb(hue, 0.8f, 0.7f);
            // Blend with white for Swan Lake ethereal look
            return Color.Lerp(Color.White, rainbow, 0.6f);
        }
        
        // Fate cosmic shimmer
        public static Color GetFateCosmicShimmer(float offset = 0f)
        {
            float progress = (Main.GlobalTimeWrappedHourly * 0.5f + offset) % 1f;
            Color baseColor = GetFate(progress);
            // Add white star sparkle
            float sparkle = MathF.Pow(MathF.Sin(Main.GlobalTimeWrappedHourly * 10f + offset * 20f), 10f);
            return Color.Lerp(baseColor, Color.White, sparkle * 0.5f);
        }
    }
}
```

---

*Extracted from FargosSoulsDLC for MagnumOpus VFX development reference.*
