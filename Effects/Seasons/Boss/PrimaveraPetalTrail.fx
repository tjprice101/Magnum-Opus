// ══════════════════════════════════════════════════════════
// PrimaveraPetalTrail.fx — Vivaldi's Spring boss trail
// Phase-reactive trail: flowing ribbon core with petal
// silhouettes, wind dispersion, and harmonic energy pulse.
// Parameters set by BossRenderHelper.DrawShaderTrail()
// ══════════════════════════════════════════════════════════

sampler uImage0 : register(s0);
sampler uNoiseTex : register(s1);
float4 uColor;
float uTrailWidth;
float uFadeRate;
float uTime;

float hash(float2 p)
{
    return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
}

float4 PS_PetalTrail(float2 uv : TEXCOORD0) : COLOR0
{
    float trailProgress = uv.x;   // 0 = head, 1 = tail
    float trailWidth = abs(uv.y - 0.5) * 2.0;   // 0 = center, 1 = edge

    // === RIBBON CORE: bright hot center with smooth falloff ===
    float coreWidth = smoothstep(0.6, 0.0, trailWidth);
    float hotCenter = exp(-trailWidth * trailWidth * 8.0);

    // Noise-driven cloud shape (organic undulation)
    float2 cloudUV = float2(uv.x * 2.0 - uTime * 1.8, uv.y * 1.5 + uTime * 0.15);
    float cloud = tex2D(uNoiseTex, cloudUV).r;
    float cloudShape = cloud * coreWidth;

    // === PETAL SILHOUETTES: scattered along trail ===
    float2 petalUV = float2(uv.x * 6.0 - uTime * 3.0, uv.y * 5.0 + sin(uv.x * 8.0 + uTime * 1.5) * 0.25);
    float petalNoise = tex2D(uNoiseTex, petalUV).r;
    float petals = smoothstep(0.70, 0.90, petalNoise);

    // Second petal layer — smaller, faster (musical double-time)
    float2 petal2UV = float2(uv.x * 10.0 - uTime * 4.0, uv.y * 8.0 - sin(uv.x * 12.0 + uTime * 2.0) * 0.15);
    float petal2Noise = tex2D(uNoiseTex, petal2UV).r;
    float petals2 = smoothstep(0.78, 0.92, petal2Noise) * 0.5;

    // === WIND DISPERSION: edge particles blown outward ===
    float2 windUV = float2(uv.x * 3.0 - uTime * 2.0, uv.y * 4.0);
    float windNoise = tex2D(uNoiseTex, windUV).r;
    float windEdge = smoothstep(0.3, 0.8, trailWidth) * smoothstep(1.0, 0.6, trailWidth);
    float windParticles = smoothstep(0.80, 0.95, windNoise) * windEdge * 0.6;

    // === HARMONIC ENERGY PULSE: standing wave along trail ===
    float wave = sin(uv.x * 25.0 - uTime * 4.0) * 0.5 + 0.5;
    float wavePulse = smoothstep(0.7, 1.0, wave) * coreWidth * 0.3;

    // === POLLEN SPARKLE DOTS ===
    float pollenSeed = hash(floor(uv * 50.0 + float2(-uTime * 2.5, uTime * 0.3)));
    float pollen = step(0.95, pollenSeed) * coreWidth;
    float pollenFlicker = sin(uTime * 6.0 + pollenSeed * 50.0) * 0.3 + 0.7;

    // === AGE FADE: trail fades along length ===
    float ageFade = pow(1.0 - trailProgress, uFadeRate * 2.0);

    // === EDGE FADE: soft edges, no hard cutoff ===
    float edgeFade = smoothstep(1.0, 0.7, trailWidth);

    // === COLOR COMPOSITION ===
    // Core: bright user color -> white-hot center
    float4 coreCol = lerp(uColor, float4(1.0, 0.98, 0.94, 1.0), hotCenter * 0.6);
    // Petal: slightly shifted hue toward pink
    float4 petalCol = uColor * 1.1;
    // Pollen: warm highlight
    float4 pollenCol = float4(1.0, 0.95, 0.85, 1.0);
    // Wind: dimmer version of main color
    float4 windCol = uColor * 0.7;

    float4 color = coreCol * cloudShape * 0.6;
    color += petalCol * petals * 0.8;
    color += petalCol * petals2;
    color += coreCol * wavePulse;
    color += windCol * windParticles;
    color += pollenCol * pollen * pollenFlicker * 0.4;

    float alpha = (cloudShape * 0.6 + petals * 0.5 + petals2 * 0.3 +
                   wavePulse + windParticles + pollen * 0.3) * ageFade * edgeFade * uTrailWidth;

    return color * saturate(alpha);
}

technique Technique1
{
    pass PetalTrail
    {
        PixelShader = compile ps_3_0 PS_PetalTrail();
    }
}
