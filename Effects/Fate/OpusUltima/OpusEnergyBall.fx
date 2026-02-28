// =============================================================================
// Opus Ultima — Energy Ball Shader
// =============================================================================
// Swirling cosmic energy orb. Dark void core with crimson-gold energy tendrils
// swirling outward, encased in a golden glory shell. Pulsates with combo power.
// =============================================================================

sampler uImage0 : register(s0);

float3 uColor;           // Primary: OpusCrimson
float3 uSecondaryColor;  // Secondary: GloryGold
float uOpacity;
float uTime;
float uIntensity;
float uOverbrightMult;
float uPhase;            // Size multiplier (1.0 normal, 1.5 massive)

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

float4 EnergyBallPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);

    // Center-relative coords
    float2 center = coords - 0.5;
    float dist = length(center);
    float angle = atan2(center.y, center.x);

    // Swirling energy tendrils
    float swirl1 = SmoothNoise(float2(angle * 3.0 + uTime * 2.0, dist * 8.0 - uTime));
    float swirl2 = SmoothNoise(float2(angle * 5.0 - uTime * 1.5, dist * 12.0 + uTime * 0.5));
    float swirl = swirl1 * 0.6 + swirl2 * 0.4;

    // Core glow: bright center fading outward
    float core = saturate(1.0 - dist * 3.0);
    core = core * core;

    // Shell: ring of energy at the edge
    float shell = saturate(1.0 - abs(dist - 0.35) * 8.0);
    shell = shell * shell;

    // Body: fills the orb area
    float body = saturate(1.0 - dist * 2.2);
    body = sqrt(body);

    // Pulsation based on time
    float pulse = sin(uTime * 5.0) * 0.1 + 0.9;

    // Color: void core → crimson swirl → gold shell → white-hot center
    float3 voidCol = float3(0.047, 0.02, 0.07);
    float3 crimsonCol = uColor;
    float3 goldCol = uSecondaryColor;
    float3 whiteHot = float3(0.96, 0.94, 1.0);

    float3 color = lerp(voidCol, crimsonCol, body * swirl);
    color = lerp(color, goldCol, shell * 0.8);
    color = lerp(color, whiteHot, core * 0.9);

    float alpha = (body * 0.4 + shell * 0.3 + core * 0.3) * pulse;
    alpha *= uOpacity * sampleColor.a * baseTex.a;
    alpha = saturate(alpha);

    float3 finalColor = color * uIntensity;

    return ApplyOverbright(finalColor, alpha);
}

technique OpusEnergyBallMain
{
    pass P0
    {
        PixelShader = compile ps_3_0 EnergyBallPS();
    }
}
