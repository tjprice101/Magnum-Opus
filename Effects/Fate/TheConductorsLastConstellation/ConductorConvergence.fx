// =============================================================================
// The Conductor's Last Constellation — Convergence Explosion Shader
// =============================================================================
// Radial convergence explosion triggered on the 3rd combo swing.
// All active beams converge on the cursor with a cosmic lightning storm.
// Expanding radial rings with electric filaments and chromatic core.
// =============================================================================

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float3 uColor;           // Primary: ConductorCyan
float3 uSecondaryColor;  // Secondary: LightningGold
float uOpacity;
float uTime;
float uIntensity;
float uOverbrightMult;
float uNoiseScale;
float uPhase;            // Explosion progress (0..1)

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

float4 ConvergenceMainPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);

    float2 center = float2(0.5, 0.5);
    float2 fromCenter = coords - center;
    float dist = length(fromCenter);
    float angle = atan2(fromCenter.y, fromCenter.x);

    float progress = saturate(uPhase);

    // Expanding ring wave
    float ringDist = abs(dist - progress * 0.5);
    float ring = saturate(1.0 - ringDist / 0.04);
    ring = ring * ring;

    // Inner convergence glow
    float innerGlow = saturate(1.0 - dist / (0.3 + progress * 0.2));
    innerGlow = innerGlow * innerGlow;

    // Electric filaments radiating outward
    float2 filamentUV = float2(angle * 5.0 / 3.14159, dist * uNoiseScale + uTime * 2.0);
    float filament = SmoothNoise(filamentUV * 6.0);
    filament = smoothstep(0.55, 0.8, filament) * saturate(1.0 - dist / 0.45);

    // Lightning arc pattern
    float2 arcUV = float2(angle * 8.0 / 3.14159, dist * 10.0 - uTime * 5.0);
    float arc = SmoothNoise(arcUV * 4.0);
    arc = step(0.75, arc) * ring;

    // Color: void → purple → cyan → gold → white
    float3 voidCol = float3(0.03, 0.02, 0.06);
    float3 purpleCol = float3(0.51, 0.20, 0.71);
    float3 cyanCol = uColor;
    float3 goldCol = uSecondaryColor;
    float3 whiteHot = float3(0.94, 0.96, 1.0);

    float3 color = lerp(voidCol, purpleCol, innerGlow * 0.3);
    color = lerp(color, cyanCol, (ring + filament) * 0.5);
    color = lerp(color, goldCol, arc);
    color = lerp(color, whiteHot, innerGlow * progress * 0.6);

    float alpha = (innerGlow * 0.3 + ring * 0.3 + filament * 0.2 + arc * 0.2);
    alpha *= uOpacity * sampleColor.a * baseTex.a;
    alpha *= (1.0 - progress * 0.3); // Fade as explosion settles

    float3 finalColor = color * uIntensity * baseTex.rgb;

    return ApplyOverbright(finalColor, alpha);
}

technique ConductorConvergenceMain
{
    pass P0
    {
        PixelShader = compile ps_3_0 ConvergenceMainPS();
    }
}
