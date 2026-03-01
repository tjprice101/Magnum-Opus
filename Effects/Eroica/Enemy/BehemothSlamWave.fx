// BehemothSlamWave.fx - Ground-slam shockwave visual
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

float4 PS_BehemothSlamWave(float2 uv : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, uv);
    if (base.a < 0.01) return float4(0, 0, 0, 0);

    float2 offset = uv - uCenter;
    float dist = length(offset);
    float angle = atan2(offset.y, offset.x);

    // Expanding shockwave ring driven by uTime as expansion factor
    float waveRadius = uTime * uRadius;
    float ringDist = abs(dist - waveRadius);
    float ringSharp = 1.0 - smoothstep(0.0, 0.08, ringDist);

    // Secondary ripple behind the main wave
    float ripple = abs(dist - waveRadius * 0.7);
    float rippleSharp = (1.0 - smoothstep(0.0, 0.05, ripple)) * 0.5;

    // Ground debris noise
    float debris = noise(uv * 20.0 + float2(angle * 2.0, uTime * 1.0));
    debris = pow(saturate(debris), 3.0) * ringSharp;

    // Radial cracks emanating from impact
    float crackPattern = noise(float2(angle * 5.0, dist * 15.0));
    float cracks = smoothstep(0.48, 0.52, crackPattern) * saturate(1.0 - dist / (uRadius * 0.5));

    // Crimson-gold impact palette
    float3 impactCrimson = float3(0.85, 0.15, 0.05);
    float3 shockGold = float3(1.0, 0.85, 0.35);
    float3 dustBrown = float3(0.45, 0.3, 0.15);

    float3 waveColor = lerp(impactCrimson, shockGold, ringSharp);
    waveColor = lerp(waveColor, dustBrown, debris * 0.5);

    float fadeOut = saturate(1.0 - uTime * 0.5);
    float totalStrength = (ringSharp + rippleSharp + cracks * 0.3) * fadeOut * uIntensity;

    float3 result = lerp(base.rgb, waveColor, saturate(totalStrength) * 0.85);
    result += shockGold * ringSharp * fadeOut * 0.2;

    return float4(saturate(result), base.a);
}

technique Technique1
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 PS_BehemothSlamWave();
    }
}
