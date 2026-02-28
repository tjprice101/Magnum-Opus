// =============================================================================
// Fractal of the Stars — Star Fracture Shader
// =============================================================================
// Geometric fractal explosion pattern — recursive star shapes expanding outward.
// Used for the Star Fracture combo finisher explosion effect.
// Radial pattern with fractal branching and golden fire.
// =============================================================================

sampler uImage0 : register(s0);

float3 uColor;           // Primary: StarGold
float3 uSecondaryColor;  // Secondary: FractalPurple
float uOpacity;
float uTime;
float uIntensity;
float uOverbrightMult;

float4 ApplyOverbright(float3 c, float a) { return float4(c * uOverbrightMult, a); }

float HashNoise(float2 p) { return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453); }

float SmoothNoise(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);
    f = f * f * (3.0 - 2.0 * f);
    float a = HashNoise(i);
    float b = HashNoise(i + float2(1.0, 0.0));
    float c = HashNoise(i + float2(0.0, 1.0));
    float d = HashNoise(i + float2(1.0, 1.0));
    return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
}

float QuadraticBump(float x) { return x * (4.0 - x * 4.0); }

float4 StarFracturePS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);

    // Convert to radial coordinates from centre
    float2 centre = float2(0.5, 0.5);
    float2 delta = coords - centre;
    float dist = length(delta);
    float angle = atan2(delta.y, delta.x);

    // Expanding ring (based on time)
    float ringDist = frac(uTime * 0.8);
    float ring = 1.0 - saturate(abs(dist - ringDist) / 0.06);
    ring = ring * ring;

    // Star arm pattern: 5-fold symmetry
    float arms = 5.0;
    float armAngle = frac(angle / (2.0 * 3.14159) * arms);
    float armShape = saturate(1.0 - abs(armAngle - 0.5) * 6.0);
    armShape = armShape * armShape;

    // Fractal branching: second layer at double frequency
    float branch = saturate(1.0 - abs(frac(angle / (2.0 * 3.14159) * arms * 2.0) - 0.5) * 8.0);
    branch = branch * branch * 0.4;

    // Radial rays emanating outward
    float rays = saturate(1.0 - dist * 1.8);
    float rayPattern = armShape + branch;
    rays *= rayPattern;

    // Central star burst
    float centre_glow = saturate(1.0 - dist / 0.15);
    centre_glow = centre_glow * centre_glow * centre_glow;

    // Outer fractal dust
    float dust = SmoothNoise(coords * 30.0 + uTime * 2.0);
    dust = dust * saturate(dist - 0.2) * 0.3;

    // Shockwave ring
    float shockwave = ring * (armShape * 0.7 + 0.3);

    // Color: void centre → purple mid → gold arms → white-hot core
    float3 voidCol = float3(0.04, 0.02, 0.10);
    float3 purpleCol = uSecondaryColor;
    float3 goldCol = uColor;
    float3 whiteHot = float3(1.0, 1.0, 0.94);

    float3 color = voidCol;
    color = lerp(color, purpleCol, rays * 0.8);
    color = lerp(color, goldCol, (rays + shockwave) * 0.6);
    color = lerp(color, whiteHot, centre_glow * 0.9);
    color += goldCol * dust;

    float alpha = (rays * 0.3 + shockwave * 0.3 + centre_glow * 0.3 + dust * 0.1);
    alpha *= uOpacity * sampleColor.a * baseTex.a;
    alpha *= saturate(1.0 - dist * 1.2); // Fade at edges

    float3 finalColor = color * uIntensity * baseTex.rgb;

    return ApplyOverbright(finalColor, alpha);
}

technique StarFractureMain
{
    pass P0
    {
        PixelShader = compile ps_3_0 StarFracturePS();
    }
}
