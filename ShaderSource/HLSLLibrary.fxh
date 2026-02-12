//=============================================================================
// MAGNUMOPUS HLSL LIBRARY
// Comprehensive utility functions for all MagnumOpus shaders
// Include with: #include "HLSLLibrary.fxh"
//=============================================================================

#ifndef MAGNUMOPUS_HLSL_LIBRARY
#define MAGNUMOPUS_HLSL_LIBRARY

//-----------------------------------------------------------------------------
// MATHEMATICAL UTILITIES
//-----------------------------------------------------------------------------

// QuadraticBump - Universal edge fade pattern
// Input: 0.0 → Output: 0.0
// Input: 0.5 → Output: 1.0 (peak)
// Input: 1.0 → Output: 0.0
float QuadraticBump(float x)
{
    return x * (4.0 - x * 4.0);
}

// InverseLerp - Get normalized progress between two values
// Returns 0 when t == a, returns 1 when t == b
float InverseLerp(float a, float b, float t)
{
    return saturate((t - a) / (b - a));
}

// Convert01To010 - Triangle wave: 0→1→0 over 0→1 input
// Great for ping-pong animations
float Convert01To010(float x)
{
    return x < 0.5 ? x * 2.0 : (1.0 - x) * 2.0;
}

// SineBump - Smooth sine-based bump (alternative to QuadraticBump)
float SineBump(float x)
{
    return sin(x * 3.14159265);
}

// Remap - Map a value from one range to another
float Remap(float value, float inMin, float inMax, float outMin, float outMax)
{
    return outMin + (value - inMin) * (outMax - outMin) / (inMax - inMin);
}

// Mod - Safe modulo operation (handles negative numbers)
float SafeMod(float x, float y)
{
    return x - y * floor(x / y);
}

float2 SafeMod2(float2 x, float y)
{
    return x - y * floor(x / y);
}

//-----------------------------------------------------------------------------
// EASING FUNCTIONS
//-----------------------------------------------------------------------------

// Ease In (slow start, fast end)
float EaseIn(float t, float power)
{
    return pow(t, power);
}

// Ease Out (fast start, slow end)
float EaseOut(float t, float power)
{
    return 1.0 - pow(1.0 - t, power);
}

// Ease In-Out (slow start and end)
float EaseInOut(float t)
{
    return t < 0.5
        ? 2.0 * t * t
        : 1.0 - 2.0 * (1.0 - t) * (1.0 - t);
}

// Smoothstep with adjustable edges
float SmoothstepCustom(float edge0, float edge1, float x)
{
    float t = saturate((x - edge0) / (edge1 - edge0));
    return t * t * (3.0 - 2.0 * t);
}

// Smootherstep - Ken Perlin's improved version (5th order)
float Smootherstep(float edge0, float edge1, float x)
{
    float t = saturate((x - edge0) / (edge1 - edge0));
    return t * t * t * (t * (t * 6.0 - 15.0) + 10.0);
}

// Elastic ease out
float EaseOutElastic(float t)
{
    float c4 = (2.0 * 3.14159265) / 3.0;
    return t == 0.0 ? 0.0 : t == 1.0 ? 1.0 :
        pow(2.0, -10.0 * t) * sin((t * 10.0 - 0.75) * c4) + 1.0;
}

// Bounce ease out
float EaseOutBounce(float t)
{
    float n1 = 7.5625;
    float d1 = 2.75;
    
    if (t < 1.0 / d1)
        return n1 * t * t;
    else if (t < 2.0 / d1)
        return n1 * (t -= 1.5 / d1) * t + 0.75;
    else if (t < 2.5 / d1)
        return n1 * (t -= 2.25 / d1) * t + 0.9375;
    else
        return n1 * (t -= 2.625 / d1) * t + 0.984375;
}

//-----------------------------------------------------------------------------
// HASH / RANDOM FUNCTIONS
//-----------------------------------------------------------------------------

// Simple hash (GPU-friendly pseudo-random)
float Hash(float2 p)
{
    return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
}

// Hash for single value
float Hash1D(float n)
{
    return frac(sin(n) * 43758.5453);
}

// Hash for 2D output
float2 Hash2D(float2 p)
{
    p = float2(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)));
    return frac(sin(p) * 43758.5453);
}

// Hash for 3D input
float Hash3D(float3 p)
{
    return frac(sin(dot(p, float3(127.1, 311.7, 74.7))) * 43758.5453);
}

// Integer hash (for seeded random)
float IntHash(int2 p)
{
    int n = p.x * 73856093 ^ p.y * 19349663;
    n = (n << 13) ^ n;
    return frac(float((n * (n * n * 15731 + 789221) + 1376312589) & 0x7fffffff) / 1073741823.5);
}

//-----------------------------------------------------------------------------
// NOISE FUNCTIONS
//-----------------------------------------------------------------------------

// 2D Value Noise (fastest, blocky)
float ValueNoise(float2 uv)
{
    float2 i = floor(uv);
    float2 f = frac(uv);
    
    // Smooth interpolation
    f = f * f * (3.0 - 2.0 * f);
    
    // Four corners
    float a = Hash(i);
    float b = Hash(i + float2(1, 0));
    float c = Hash(i + float2(0, 1));
    float d = Hash(i + float2(1, 1));
    
    // Bilinear interpolation
    return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
}

// 2D Perlin-style gradient noise
float2 RandomGradient(float2 p)
{
    p = SafeMod2(p, 289.0);
    float x = (34.0 * p.x + 1.0) * p.x;
    x = SafeMod(x, 289.0) + p.y;
    x = (34.0 * x + 1.0) * x;
    x = SafeMod(x, 289.0);
    x = frac(x / 41.0) * 2.0 - 1.0;
    return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
}

float PerlinNoise(float2 uv)
{
    float2 i = floor(uv);
    float2 f = frac(uv);
    
    // Quintic interpolation (smoother than cubic)
    float2 u = f * f * f * (f * (f * 6.0 - 15.0) + 10.0);
    
    float2 ga = RandomGradient(i + float2(0, 0));
    float2 gb = RandomGradient(i + float2(1, 0));
    float2 gc = RandomGradient(i + float2(0, 1));
    float2 gd = RandomGradient(i + float2(1, 1));
    
    float va = dot(ga, f - float2(0, 0));
    float vb = dot(gb, f - float2(1, 0));
    float vc = dot(gc, f - float2(0, 1));
    float vd = dot(gd, f - float2(1, 1));
    
    return lerp(lerp(va, vb, u.x), lerp(vc, vd, u.x), u.y);
}

// 2D Simplex Noise (best quality/performance)
float3 Mod289_3(float3 x) { return x - floor(x * (1.0 / 289.0)) * 289.0; }
float2 Mod289_2(float2 x) { return x - floor(x * (1.0 / 289.0)) * 289.0; }
float3 Permute(float3 x) { return Mod289_3(((x * 34.0) + 1.0) * x); }

float SimplexNoise(float2 v)
{
    const float4 C = float4(0.211324865405187,  // (3.0-sqrt(3.0))/6.0
                            0.366025403784439,  // 0.5*(sqrt(3.0)-1.0)
                            -0.577350269189626, // -1.0 + 2.0 * C.x
                            0.024390243902439); // 1.0 / 41.0
    
    float2 i = floor(v + dot(v, C.yy));
    float2 x0 = v - i + dot(i, C.xx);
    
    float2 i1 = (x0.x > x0.y) ? float2(1.0, 0.0) : float2(0.0, 1.0);
    float4 x12 = x0.xyxy + C.xxzz;
    x12.xy -= i1;
    
    i = Mod289_2(i);
    float3 p = Permute(Permute(i.y + float3(0.0, i1.y, 1.0)) + i.x + float3(0.0, i1.x, 1.0));
    
    float3 m = max(0.5 - float3(dot(x0, x0), dot(x12.xy, x12.xy), dot(x12.zw, x12.zw)), 0.0);
    m = m * m;
    m = m * m;
    
    float3 x = 2.0 * frac(p * C.www) - 1.0;
    float3 h = abs(x) - 0.5;
    float3 ox = floor(x + 0.5);
    float3 a0 = x - ox;
    
    m *= 1.79284291400159 - 0.85373472095314 * (a0 * a0 + h * h);
    
    float3 g;
    g.x = a0.x * x0.x + h.x * x0.y;
    g.yz = a0.yz * x12.xz + h.yz * x12.yw;
    return 130.0 * dot(m, g);
}

// Fractional Brownian Motion (multi-octave noise)
float FBM(float2 uv, int octaves, float persistence, float lacunarity)
{
    float value = 0.0;
    float amplitude = 0.5;
    float frequency = 1.0;
    
    for (int i = 0; i < octaves; i++)
    {
        value += amplitude * SimplexNoise(uv * frequency);
        frequency *= lacunarity;  // Increase frequency
        amplitude *= persistence; // Decrease amplitude
    }
    
    return value;
}

// Turbulence (absolute value FBM - more cloudy)
float Turbulence(float2 uv, int octaves)
{
    float value = 0.0;
    float amplitude = 0.5;
    float frequency = 1.0;
    
    for (int i = 0; i < octaves; i++)
    {
        value += amplitude * abs(SimplexNoise(uv * frequency));
        frequency *= 2.0;
        amplitude *= 0.5;
    }
    
    return value;
}

// Domain warping (distorted noise)
float WarpedNoise(float2 uv, float time)
{
    float2 q = float2(
        SimplexNoise(uv + time * 0.1),
        SimplexNoise(uv + float2(5.2, 1.3))
    );
    
    float2 r = float2(
        SimplexNoise(uv + 4.0 * q + time * 0.2),
        SimplexNoise(uv + 4.0 * q + float2(2.8, 3.1))
    );
    
    return SimplexNoise(uv + 4.0 * r);
}

//-----------------------------------------------------------------------------
// 2D SIGNED DISTANCE FIELDS (SDFs)
//-----------------------------------------------------------------------------

// Circle
float SDFCircle(float2 p, float radius)
{
    return length(p) - radius;
}

// Box
float SDFBox(float2 p, float2 size)
{
    float2 d = abs(p) - size;
    return length(max(d, 0.0)) + min(max(d.x, d.y), 0.0);
}

// Rounded box
float SDFRoundedBox(float2 p, float2 size, float radius)
{
    float2 d = abs(p) - size + radius;
    return length(max(d, 0.0)) - radius;
}

// Line segment
float SDFSegment(float2 p, float2 a, float2 b)
{
    float2 pa = p - a;
    float2 ba = b - a;
    float h = clamp(dot(pa, ba) / dot(ba, ba), 0.0, 1.0);
    return length(pa - ba * h);
}

// Hexagon
float SDFHexagon(float2 p, float radius)
{
    const float3 k = float3(-0.866025404, 0.5, 0.577350269);
    p = abs(p);
    p -= 2.0 * min(dot(k.xy, p), 0.0) * k.xy;
    p -= float2(clamp(p.x, -k.z * radius, k.z * radius), radius);
    return length(p) * sign(p.y);
}

// Star
float SDFStar5(float2 p, float r, float rf)
{
    const float2 k1 = float2(0.809016994, -0.587785252);
    const float2 k2 = float2(-k1.x, k1.y);
    p.x = abs(p.x);
    p -= 2.0 * max(dot(k1, p), 0.0) * k1;
    p -= 2.0 * max(dot(k2, p), 0.0) * k2;
    p.x = abs(p.x);
    p.y -= r;
    float2 ba = rf * float2(-k1.y, k1.x) - float2(0, 1);
    float h = clamp(dot(p, ba) / dot(ba, ba), 0.0, r);
    return length(p - ba * h) * sign(p.y * ba.x - p.x * ba.y);
}

// SDF Operations
float SDFUnion(float d1, float d2) { return min(d1, d2); }
float SDFSubtract(float d1, float d2) { return max(-d1, d2); }
float SDFIntersect(float d1, float d2) { return max(d1, d2); }

// Smooth union (metaball blend)
float SDFSmoothUnion(float d1, float d2, float k)
{
    float h = clamp(0.5 + 0.5 * (d2 - d1) / k, 0.0, 1.0);
    return lerp(d2, d1, h) - k * h * (1.0 - h);
}

float SDFSmoothSubtract(float d1, float d2, float k)
{
    float h = clamp(0.5 - 0.5 * (d2 + d1) / k, 0.0, 1.0);
    return lerp(d2, -d1, h) + k * h * (1.0 - h);
}

float SDFSmoothIntersect(float d1, float d2, float k)
{
    float h = clamp(0.5 - 0.5 * (d2 - d1) / k, 0.0, 1.0);
    return lerp(d2, d1, h) + k * h * (1.0 - h);
}

//-----------------------------------------------------------------------------
// COLOR UTILITIES
//-----------------------------------------------------------------------------

// RGB to HSV
float3 RGBtoHSV(float3 rgb)
{
    float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    float4 p = lerp(float4(rgb.bg, K.wz), float4(rgb.gb, K.xy), step(rgb.b, rgb.g));
    float4 q = lerp(float4(p.xyw, rgb.r), float4(rgb.r, p.yzx), step(p.x, rgb.r));
    
    float d = q.x - min(q.w, q.y);
    float e = 1.0e-10;
    return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

// HSV to RGB
float3 HSVtoRGB(float3 hsv)
{
    float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    float3 p = abs(frac(hsv.xxx + K.xyz) * 6.0 - K.www);
    return hsv.z * lerp(K.xxx, saturate(p - K.xxx), hsv.y);
}

// Hue shift
float3 HueShift(float3 color, float shift)
{
    float3 hsv = RGBtoHSV(color);
    hsv.x = frac(hsv.x + shift);
    return HSVtoRGB(hsv);
}

// Luminance
float Luminance(float3 color)
{
    return dot(color, float3(0.299, 0.587, 0.114));
}

// Contrast
float3 Contrast(float3 color, float contrast)
{
    return (color - 0.5) * contrast + 0.5;
}

// Saturation
float3 Saturation(float3 color, float sat)
{
    float gray = Luminance(color);
    return lerp(float3(gray, gray, gray), color, sat);
}

//-----------------------------------------------------------------------------
// TONE MAPPING
//-----------------------------------------------------------------------------

// Reinhard (simple)
float3 ToneMapReinhard(float3 color)
{
    return color / (1.0 + color);
}

// Reinhard extended (with white point)
float3 ToneMapReinhardExtended(float3 color, float whitePoint)
{
    float3 numerator = color * (1.0 + color / (whitePoint * whitePoint));
    return numerator / (1.0 + color);
}

// Uncharted 2 filmic
float3 Uncharted2Curve(float3 x)
{
    float A = 0.15, B = 0.50, C = 0.10;
    float D = 0.20, E = 0.02, F = 0.30;
    return ((x * (A * x + C * B) + D * E) / (x * (A * x + B) + D * F)) - E / F;
}

float3 ToneMapUncharted2(float3 color)
{
    float W = 11.2;
    return Uncharted2Curve(color * 2.0) / Uncharted2Curve(W);
}

// ACES filmic (industry standard)
float3 ToneMapACES(float3 color)
{
    float a = 2.51, b = 0.03;
    float c = 2.43, d = 0.59, e = 0.14;
    return saturate((color * (a * color + b)) / (color * (c * color + d) + e));
}

//-----------------------------------------------------------------------------
// UV MANIPULATION
//-----------------------------------------------------------------------------

// Rotate UV around center
float2 RotateUV(float2 uv, float angle)
{
    float2 center = float2(0.5, 0.5);
    float cosA = cos(angle);
    float sinA = sin(angle);
    float2x2 rot = float2x2(cosA, -sinA, sinA, cosA);
    return mul(rot, uv - center) + center;
}

// Scale UV from center
float2 ScaleUV(float2 uv, float scale)
{
    return (uv - 0.5) * scale + 0.5;
}

// Tile UV
float2 TileUV(float2 uv, float2 tiles)
{
    return frac(uv * tiles);
}

// Polar coordinates (cartesian to polar)
float2 ToPolar(float2 uv)
{
    float2 centered = uv - 0.5;
    float radius = length(centered);
    float angle = atan2(centered.y, centered.x);
    return float2(radius, angle);
}

// Cartesian coordinates (polar to cartesian)
float2 ToCartesian(float2 polar)
{
    return float2(cos(polar.y), sin(polar.y)) * polar.x + 0.5;
}

//-----------------------------------------------------------------------------
// DISTORTION EFFECTS
//-----------------------------------------------------------------------------

// Wave distortion
float2 WaveDistort(float2 uv, float time, float amplitude, float frequency)
{
    return uv + float2(
        sin(uv.y * frequency + time) * amplitude,
        cos(uv.x * frequency + time) * amplitude
    );
}

// Radial distortion (barrel/pincushion)
float2 RadialDistort(float2 uv, float strength)
{
    float2 center = float2(0.5, 0.5);
    float2 offset = uv - center;
    float dist = length(offset);
    float factor = 1.0 + strength * dist * dist;
    return center + offset * factor;
}

// Noise distortion
float2 NoiseDistort(float2 uv, float strength, float scale)
{
    float noiseX = SimplexNoise(uv * scale);
    float noiseY = SimplexNoise(uv * scale + float2(100, 100));
    return uv + float2(noiseX, noiseY) * strength;
}

//-----------------------------------------------------------------------------
// LIGHTING UTILITIES
//-----------------------------------------------------------------------------

// Lambert diffuse
float LambertDiffuse(float3 normal, float3 lightDir)
{
    return saturate(dot(normal, lightDir));
}

// Blinn-Phong specular
float BlinnPhongSpecular(float3 normal, float3 lightDir, float3 viewDir, float shininess)
{
    float3 halfDir = normalize(lightDir + viewDir);
    return pow(saturate(dot(normal, halfDir)), shininess);
}

// Fresnel (Schlick approximation)
float FresnelSchlick(float3 normal, float3 viewDir, float F0)
{
    float cosTheta = saturate(dot(normal, viewDir));
    return F0 + (1.0 - F0) * pow(1.0 - cosTheta, 5.0);
}

// Rim lighting
float RimLight(float3 normal, float3 viewDir, float power)
{
    float rim = 1.0 - saturate(dot(viewDir, normal));
    return pow(rim, power);
}

//-----------------------------------------------------------------------------
// POST-PROCESSING
//-----------------------------------------------------------------------------

// Vignette
float Vignette(float2 uv, float intensity, float smoothness)
{
    float2 centered = uv - 0.5;
    float dist = length(centered);
    return 1.0 - smoothstep(0.5 - smoothness, 0.5, dist * intensity);
}

// Chromatic aberration
float4 ChromaticAberration(sampler2D tex, float2 uv, float strength)
{
    float2 center = float2(0.5, 0.5);
    float2 offset = (uv - center) * strength;
    
    float r = tex2D(tex, uv - offset).r;
    float g = tex2D(tex, uv).g;
    float b = tex2D(tex, uv + offset).b;
    
    return float4(r, g, b, 1.0);
}

// Film grain
float FilmGrain(float2 uv, float time, float intensity)
{
    float noise = Hash(uv + float2(time, time * 0.7));
    return (noise - 0.5) * intensity;
}

// Scanlines
float Scanlines(float2 uv, float density, float intensity)
{
    return 1.0 - sin(uv.y * density * 3.14159) * intensity;
}

//-----------------------------------------------------------------------------
// GRADIENT HELPERS
//-----------------------------------------------------------------------------

// Multi-stop gradient lookup
float4 GradientLookup(float t, float4 colors[4], int numStops)
{
    float scaledT = t * (numStops - 1);
    int index = (int)floor(scaledT);
    float localT = frac(scaledT);
    
    if (index >= numStops - 1)
        return colors[numStops - 1];
    
    return lerp(colors[index], colors[index + 1], localT);
}

// Fire gradient (red → orange → yellow → white)
float3 FireGradient(float t)
{
    float3 c1 = float3(0.1, 0.0, 0.0);   // Dark red
    float3 c2 = float3(1.0, 0.2, 0.0);   // Orange-red
    float3 c3 = float3(1.0, 0.6, 0.0);   // Orange
    float3 c4 = float3(1.0, 1.0, 0.4);   // Yellow
    float3 c5 = float3(1.0, 1.0, 1.0);   // White
    
    if (t < 0.25) return lerp(c1, c2, t * 4.0);
    if (t < 0.50) return lerp(c2, c3, (t - 0.25) * 4.0);
    if (t < 0.75) return lerp(c3, c4, (t - 0.50) * 4.0);
    return lerp(c4, c5, (t - 0.75) * 4.0);
}

// Ice gradient (dark blue → cyan → white)
float3 IceGradient(float t)
{
    float3 c1 = float3(0.0, 0.1, 0.3);   // Dark blue
    float3 c2 = float3(0.0, 0.4, 0.8);   // Blue
    float3 c3 = float3(0.3, 0.8, 1.0);   // Cyan
    float3 c4 = float3(0.9, 1.0, 1.0);   // White
    
    if (t < 0.33) return lerp(c1, c2, t * 3.0);
    if (t < 0.66) return lerp(c2, c3, (t - 0.33) * 3.0);
    return lerp(c3, c4, (t - 0.66) * 3.0);
}

// Rainbow gradient
float3 RainbowGradient(float t)
{
    return HSVtoRGB(float3(t, 1.0, 1.0));
}

#endif // MAGNUMOPUS_HLSL_LIBRARY
