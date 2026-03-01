// PrimaGraceTrail.fx - Graceful ballet trail, elegant monochrome flow
sampler uImage0 : register(s0);
float2 uCenter;
float uRadius;
float uIntensity;
float4 uPrimaryColor;
float4 uSecondaryColor;
float uTime;

float hash(float2 p) {
    return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
}

float noise(float2 p) {
    float2 i = floor(p);
    float2 f = frac(p);
    f = f * f * (3.0 - 2.0 * f);
    float a = hash(i);
    float b = hash(i + float2(1.0, 0.0));
    float c = hash(i + float2(0.0, 1.0));
    float d = hash(i + float2(1.0, 1.0));
    return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
}

float4 PS_PrimaGraceTrail(float2 uv : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, uv);
    if (base.a < 0.01) return float4(0, 0, 0, 0);

    float trailProgress = uv.x;
    float trailWidth = abs(uv.y - 0.5) * 2.0;

    // Ballet ribbon  Esmooth sinusoidal wave
    float ribbon = sin(trailProgress * 8.0 - uTime * 2.5 + trailWidth * 3.0);
    ribbon = ribbon * 0.5 + 0.5;

    // Flowing silk texture
    float silk = noise(uv * 4.0 + float2(-uTime * 0.8, uTime * 0.3));
    float silkFine = noise(uv * 9.0 + float2(-uTime * 1.2, 0.0));

    // Monochrome elegance  Ewhite to deep black
    float3 brightWhite = float3(0.96, 0.96, 1.0);
    float3 pearlGray = float3(0.7, 0.68, 0.72);
    float3 deepBlack = float3(0.05, 0.05, 0.07);

    float3 trailColor = lerp(pearlGray, brightWhite, ribbon);
    trailColor = lerp(trailColor, deepBlack, (1.0 - silk) * 0.25);

    // Faint prismatic edge
    float prismHint = sin(trailProgress * 15.0 + uTime * 3.0) * 0.5 + 0.5;
    float3 prismEdge = float3(0.9, 0.85, 1.0) * prismHint;

    float edgeFade = 1.0 - smoothstep(0.35, 0.85, trailWidth);
    float tailFade = smoothstep(0.0, 0.1, trailProgress) * smoothstep(1.0, 0.65, trailProgress);
    float gracePulse = 0.8 + 0.2 * sin(uTime * 1.8 + trailProgress * 4.0);

    float alpha = edgeFade * tailFade * gracePulse * uIntensity;
    float3 result = lerp(base.rgb, trailColor, alpha * 0.8);
    result += prismEdge * edgeFade * tailFade * silkFine * 0.06;

    return float4(saturate(result), base.a * edgeFade * tailFade);
}

technique Technique1
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 PS_PrimaGraceTrail();
    }
}
