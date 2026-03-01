// CrawlerInfernalTrail.fx - Infernal crawling trail of smoke and fire
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

float4 PS_CrawlerInfernalTrail(float2 uv : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, uv);
    if (base.a < 0.01) return float4(0, 0, 0, 0);

    float trailProgress = uv.x;
    float trailWidth = abs(uv.y - 0.5) * 2.0;

    // Rising smoke + fire turbulence
    float turbulence = noise(uv * 6.0 + float2(-uTime * 1.5, -uTime * 3.0));
    float turbulence2 = noise(uv * 12.0 + float2(uTime * 2.0, -uTime * 4.0));
    float fire = turbulence * 0.55 + turbulence2 * 0.45;

    // Crawling embers  Ebright hot spots
    float embers = noise(uv * 25.0 + float2(-uTime * 6.0, uTime * 1.0));
    embers = pow(saturate(embers), 5.0) * 3.0;

    // Smoke layer darkening from below
    float smokeRise = noise(float2(uv.x * 3.0 - uTime * 0.5, uv.y * 2.0 - uTime * 1.2));
    float smokeMask = smoothstep(0.3, 0.6, smokeRise) * (1.0 - trailWidth);

    // Palette: charcoal -> deep orange -> bright gold at core
    float3 charcoal = float3(0.08, 0.04, 0.02);
    float3 deepOrange = float3(0.9, 0.3, 0.02);
    float3 hotGold = float3(1.0, 0.85, 0.3);

    float3 trailColor = lerp(charcoal, deepOrange, fire);
    trailColor = lerp(trailColor, hotGold, embers);

    float edgeFade = 1.0 - smoothstep(0.4, 0.9, trailWidth);
    float tailFade = smoothstep(0.0, 0.15, trailProgress) * smoothstep(1.0, 0.6, trailProgress);

    float alpha = edgeFade * tailFade * uIntensity;
    float3 result = lerp(base.rgb, trailColor, alpha * 0.85);
    result -= smokeMask * 0.15;
    result += hotGold * embers * edgeFade * tailFade * 0.1;

    return float4(saturate(result), base.a * edgeFade * tailFade);
}

technique Technique1
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 PS_CrawlerInfernalTrail();
    }
}
