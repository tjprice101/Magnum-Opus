// StolenValorMinionLink.fx - Visual link to orbiting minions
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

float4 PS_StolenValorMinionLink(float2 uv : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, uv);
    if (base.a < 0.01) return float4(0, 0, 0, 0);

    // Link rendered as a strip  EX is along the link, Y is width
    float linkProgress = uv.x;
    float linkWidth = abs(uv.y - 0.5) * 2.0;

    // Energy chain links  Erepeating segments
    float chainFreq = 10.0;
    float chainPhase = frac(linkProgress * chainFreq - uTime * 2.0);
    float chainLink = smoothstep(0.0, 0.15, chainPhase) * smoothstep(0.5, 0.35, chainPhase);

    // Corruption crackle along the link
    float crackle = noise(uv * 15.0 + float2(-uTime * 4.0, uTime * 1.0));
    float static1 = noise(uv * 40.0 + float2(uTime * 8.0, 0.0));
    float staticFlash = step(0.9, static1) * 0.7;

    // Energy flow  Epulsing from center to edges
    float energyFlow = sin(linkProgress * 20.0 - uTime * 6.0) * 0.5 + 0.5;
    float corePulse = 0.6 + 0.4 * sin(uTime * 2.5);

    // Corrupted tether palette
    float3 darkChain = float3(0.2, 0.12, 0.05);
    float3 tarnishedGold = float3(0.65, 0.5, 0.12);
    float3 corruptFlash = float3(0.8, 0.2, 0.1);
    float3 dimWhite = float3(0.7, 0.65, 0.55);

    float3 linkColor = lerp(darkChain, tarnishedGold, chainLink);
    linkColor = lerp(linkColor, corruptFlash, crackle * 0.4);
    linkColor += dimWhite * staticFlash;

    float edgeFade = 1.0 - smoothstep(0.2, 0.7, linkWidth);
    float endFade = smoothstep(0.0, 0.1, linkProgress) * smoothstep(1.0, 0.9, linkProgress);

    float alpha = edgeFade * endFade * corePulse * uIntensity;
    alpha *= (0.5 + chainLink * 0.3 + energyFlow * 0.2);

    float3 result = lerp(base.rgb, linkColor, alpha * 0.85);

    return float4(saturate(result), base.a * edgeFade * endFade * 0.8);
}

technique Technique1
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 PS_StolenValorMinionLink();
    }
}
