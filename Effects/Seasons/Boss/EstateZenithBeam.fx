// ══════════════════════════════════════════════════════════╁E
// EstateZenithBeam.fx  ESeasons/Estate phase transition
// Zenith beam descending from sky  Evertical light shaft
// with heat distortion, transitioning between boss phases.
// ══════════════════════════════════════════════════════════╁E

sampler uImage0 : register(s0);
float uTransitionProgress;
float4 uFromColor;
float4 uToColor;
float uIntensity;
float uTime;

float hash(float2 p)
{
    return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
}

float noise(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);
    f = f * f * (3.0 - 2.0 * f);
    float a = hash(i);
    float b = hash(i + float2(1.0, 0.0));
    float c = hash(i + float2(0.0, 1.0));
    float d = hash(i + float2(1.0, 1.0));
    return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
}

float4 PS_ZenithBeam(float2 uv : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, uv);

    // Vertical beam shaft  Ecentered, narrow column of light
    float beamWidth = 0.08 + uTransitionProgress * 0.12;
    float beamDist = abs(uv.x - 0.5);
    float beamMask = smoothstep(beamWidth, beamWidth * 0.3, beamDist);

    // Beam descends from top as transition progresses
    float beamTop = 1.0 - uTransitionProgress;
    float beamY = smoothstep(beamTop, beamTop + 0.1, 1.0 - uv.y);
    beamMask *= beamY;

    // Heat distortion noise  Eshimmer along beam edges
    float haze = noise(float2(uv.x * 20.0 + uTime * 3.0, uv.y * 8.0 - uTime * 5.0));
    float hazeMask = smoothstep(0.35, 0.7, haze);
    float edgeHaze = smoothstep(beamWidth * 0.3, beamWidth, beamDist) * (1.0 - smoothstep(beamWidth, beamWidth * 1.5, beamDist));
    edgeHaze *= hazeMask * 0.5;

    // Phase blend
    float phaseMix = smoothstep(0.3, 0.7, uTransitionProgress);

    // Sun particles drifting down in the beam
    float particles = hash(floor(float2(uv.x * 30.0, uv.y * 40.0 - uTime * 3.0)));
    float particleMask = step(0.94, particles) * beamMask;

    // Colors: white-hot core, golden beam body, orange heat shimmer
    float4 phaseColor = lerp(uFromColor, uToColor, phaseMix);
    float4 whiteCore = float4(1.0, 0.98, 0.92, 1.0);
    float4 goldenBeam = float4(1.0, 0.85, 0.35, 1.0);
    float4 orangeHaze = float4(1.0, 0.6, 0.15, 1.0);

    float beamGrad = saturate(beamDist / beamWidth);
    float4 beamColor = lerp(whiteCore, goldenBeam, beamGrad);

    float4 result = base;
    result.rgb = lerp(result.rgb, phaseColor.rgb, phaseMix * uIntensity * 0.4);
    result.rgb += beamColor.rgb * beamMask * uIntensity;
    result.rgb += orangeHaze.rgb * edgeHaze * uIntensity;
    result.rgb += whiteCore.rgb * particleMask * uIntensity * 1.5;

    return result;
}

technique Technique1
{
    pass ZenithBeam
    {
        PixelShader = compile ps_3_0 PS_ZenithBeam();
    }
}
