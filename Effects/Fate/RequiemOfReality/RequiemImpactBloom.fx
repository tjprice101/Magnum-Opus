// =============================================================================
// Requiem of Reality — Impact Bloom Shader
// =============================================================================
// Directional impact bloom that fires from the hit point. Multi-ring
// shockwave with radial debris streaks and a white-hot epicentre.
// uPhase drives the expansion (0=fresh → 1=fully dissipated).
// =============================================================================

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float3 uColor;           // BrightCrimson
float3 uSecondaryColor;  // SupernovaWhite
float uOpacity;
float uTime;
float uIntensity;
float uOverbrightMult;
float uNoiseScale;
float uPhase;            // Expansion progress 0→1
float uHasSecondaryTex;

float4 ApplyOverbright(float3 c, float a) { return float4(c * uOverbrightMult, a); }
float HashNoise(float2 p) { return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453); }

float4 ImpactBloomPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);

    float2 centred = coords - 0.5;
    float dist = length(centred) * 2.0;
    float angle = atan2(centred.y, centred.x);

    float expand = saturate(uPhase);

    // Epicentre glow
    float epicentre = saturate(1.0 - dist / max(expand * 0.25, 0.01));
    epicentre = epicentre * epicentre * epicentre;

    // Shockwave ring
    float ringRadius = expand * 0.8;
    float ringWidth = 0.06 + expand * 0.04;
    float ring = saturate(1.0 - abs(dist - ringRadius) / ringWidth);
    ring = ring * ring;

    // Second echo ring (smaller, delayed)
    float ring2Radius = expand * 0.5;
    float ring2 = saturate(1.0 - abs(dist - ring2Radius) / 0.04);
    ring2 = ring2 * ring2 * 0.5;

    // Radial debris streaks (10 spokes)
    float debris = cos(angle * 5.0) * 0.5 + 0.5;
    debris = pow(debris, 6.0);
    debris *= saturate(dist / max(expand * 0.7, 0.01)) * saturate(1.0 - dist / max(expand * 0.9, 0.01));

    // Noise organic breakup
    float2 noiseUV = float2(angle * 0.318 * uNoiseScale, dist * 3.0);
    float noise = HashNoise(noiseUV * 5.0);

    // Secondary texture
    float4 secTex = tex2D(uImage1, coords * 2.0);
    float detail = lerp(1.0, secTex.r, uHasSecondaryTex * 0.2);

    // Color: white-hot epicentre → crimson ring → dark pink fade → void
    float3 whiteHot = uSecondaryColor;
    float3 ringColor = uColor;
    float3 fadeColor = float3(0.47, 0.12, 0.39);

    float3 color = fadeColor * debris * 0.3;
    color += ringColor * (ring + ring2);
    color = lerp(color, whiteHot, epicentre);
    color *= noise * 0.3 + 0.7;
    color *= detail;

    float fadeFactor = 1.0 - expand * expand * 0.7;
    float alpha = (epicentre * 0.4 + ring * 0.3 + ring2 * 0.1 + debris * 0.2);
    alpha *= fadeFactor * uOpacity * sampleColor.a * baseTex.a;
    float3 finalColor = color * uIntensity * baseTex.rgb;

    return ApplyOverbright(finalColor, alpha);
}

technique ImpactBloomMain
{
    pass P0
    {
        PixelShader = compile ps_3_0 ImpactBloomPS();
    }
}
