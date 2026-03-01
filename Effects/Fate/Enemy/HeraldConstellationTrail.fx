// HeraldConstellationTrail.fx - Constellation trail of connected stars
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

float4 PS_HeraldConstellationTrail(float2 uv : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, uv);
    if (base.a < 0.01) return float4(0, 0, 0, 0);

    float trailProgress = uv.x;
    float trailWidth = abs(uv.y - 0.5) * 2.0;

    // Constellation star nodes  Eperiodic bright points along trail
    float starFreq = 6.0;
    float starPhase = frac(trailProgress * starFreq + uTime * 0.5);
    float starNode = pow(1.0 - smoothstep(0.0, 0.15, abs(starPhase - 0.5)), 3.0);

    // Connecting lines between stars  Ethin bright filaments
    float filament = 1.0 - smoothstep(0.0, 0.1, trailWidth);
    float filamentPulse = 0.5 + 0.5 * sin(trailProgress * 30.0 - uTime * 4.0);

    // Cosmic dust between stars
    float dust = noise(uv * 8.0 + float2(-uTime * 1.0, uTime * 0.4));
    float dustFine = noise(uv * 18.0 + float2(-uTime * 2.0, 0.0));

    // Dim twinkling field
    float twinkle = noise(uv * 35.0 + float2(uTime * 1.5, -uTime * 0.8));
    twinkle = pow(saturate(twinkle), 7.0) * 2.5;

    // Fate constellation palette
    float3 cosmicBlack = float3(0.01, 0.005, 0.02);
    float3 fatePink = float3(0.7, 0.15, 0.4);
    float3 crimsonGlow = float3(0.9, 0.2, 0.25);
    float3 starWhite = float3(1.0, 0.95, 1.0);

    // Base trail  Edark cosmic ribbon
    float3 trailColor = lerp(cosmicBlack, fatePink, dust * 0.5);
    trailColor = lerp(trailColor, crimsonGlow, filament * filamentPulse * 0.6);

    // Star nodes  Ebright white-pink
    trailColor = lerp(trailColor, starWhite, starNode * 0.9);
    trailColor += starWhite * twinkle * 0.2;

    float edgeFade = 1.0 - smoothstep(0.25, 0.75, trailWidth);
    float tailFade = smoothstep(0.0, 0.1, trailProgress) * smoothstep(1.0, 0.6, trailProgress);

    float alpha = edgeFade * tailFade * uIntensity;
    alpha *= (0.4 + dust * 0.3 + starNode * 0.3);

    float3 result = lerp(base.rgb, trailColor, alpha * 0.9);
    result += starWhite * starNode * tailFade * 0.15;

    return float4(saturate(result), base.a * edgeFade * tailFade);
}

technique Technique1
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 PS_HeraldConstellationTrail();
    }
}
