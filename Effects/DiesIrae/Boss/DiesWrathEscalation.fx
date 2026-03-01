// ═══════════════════════════════════════════════════════════
// DiesWrathEscalation.fx — Dies Irae boss phase escalation
// Wrath building visual: screen overlay intensifies with
// blood-red veins spreading and pulsing ember aggression.
// ═══════════════════════════════════════════════════════════

sampler uImage0 : register(s0);
sampler uNoiseTex : register(s1);
float uTransitionProgress;  // 0 = calm, 1 = max wrath
float4 uFromColor;          // Dark crimson (restrained)
float4 uToColor;            // Blood red (full wrath)
float uIntensity;
float uTime;

float noise(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);
    f = f * f * (3.0 - 2.0 * f);
    float a = frac(sin(dot(i, float2(127.1, 311.7))) * 43758.5453);
    float b = frac(sin(dot(i + float2(1,0), float2(127.1, 311.7))) * 43758.5453);
    float c = frac(sin(dot(i + float2(0,1), float2(127.1, 311.7))) * 43758.5453);
    float d = frac(sin(dot(i + float2(1,1), float2(127.1, 311.7))) * 43758.5453);
    return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
}

float4 PS_WrathEscalation(float2 uv : TEXCOORD0) : COLOR0
{
    float2 centered = uv - 0.5;
    float dist = length(centered);

    // Blood vein network — Worley-noise-like cracks spreading from edges
    float veinNoise = tex2D(uNoiseTex, uv * 3.0 + float2(uTime * 0.2, uTime * 0.1)).r;
    float veinThreshold = 1.0 - uTransitionProgress * 0.8;
    float veins = smoothstep(veinThreshold + 0.05, veinThreshold, veinNoise);

    // Pulse heartbeat — intensity surges rhythmically
    float heartbeat = pow(sin(uTime * 4.0 * (1.0 + uTransitionProgress)) * 0.5 + 0.5, 3.0);

    // Edge vignette darkening that intensifies with wrath
    float vignette = smoothstep(0.3, 0.7, dist) * uTransitionProgress;

    // Ember particles appearing at higher wrath
    float emberNoise = noise(uv * 20.0 + float2(0, -uTime * 2.0));
    float embers = step(1.0 - uTransitionProgress * 0.15, emberNoise);

    // Color: crimson veins, blood red pulse, dark vignette
    float4 baseColor = lerp(uFromColor, uToColor, uTransitionProgress);
    float4 veinColor = uToColor * 1.3;
    float4 emberColor = float4(1.0, 0.4, 0.05, 1.0);

    float4 color = float4(0, 0, 0, 0);
    color += veinColor * veins * uIntensity * (heartbeat * 0.5 + 0.5);
    color += baseColor * vignette * 0.3;
    color += baseColor * heartbeat * uIntensity * 0.2;
    color += emberColor * embers * uIntensity * uTransitionProgress;

    return color;
}

technique Technique1
{
    pass WrathEscalation
    {
        PixelShader = compile ps_3_0 PS_WrathEscalation();
    }
}
