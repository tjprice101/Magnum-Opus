// ══════════════════════════════════════════════════════════╁E
// EroicaValorAura.fx  EEroica boss ambient presence shader
// A pulsing heroic aura of scarlet and gold energy with 
// radial waves and sakura petal-like noise distortion.
// ══════════════════════════════════════════════════════════╁E

sampler uImage0 : register(s0);
float2 uCenter;
float uRadius;
float uIntensity;
float4 uPrimaryColor;   // Gold
float4 uSecondaryColor;  // Scarlet
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

float4 PS_ValorAura(float2 uv : TEXCOORD0) : COLOR0
{
    float2 centered = uv - 0.5;
    float dist = length(centered);
    float angle = atan2(centered.y, centered.x);
    
    // Radial falloff with pulsing
    float pulse = sin(uTime * 3.0) * 0.1 + 1.0;
    float radiusNorm = uRadius / 200.0 * pulse;
    float falloff = 1.0 - smoothstep(0.0, radiusNorm, dist);
    
    // Heroic wave rings expanding outward
    float wave1 = sin(dist * 30.0 - uTime * 5.0) * 0.5 + 0.5;
    float wave2 = sin(dist * 20.0 - uTime * 3.0 + 1.5) * 0.5 + 0.5;
    float waves = wave1 * 0.6 + wave2 * 0.4;
    
    // Sakura-petal noise pattern rotating slowly
    float n = noise(float2(angle * 3.0 + uTime * 0.5, dist * 10.0 - uTime));
    float petalPattern = smoothstep(0.3, 0.7, n);
    
    // Color lerp between gold and scarlet based on angle + noise
    float colorMix = sin(angle * 3.0 + uTime * 2.0) * 0.5 + 0.5;
    colorMix = lerp(colorMix, petalPattern, 0.4);
    float4 color = lerp(uPrimaryColor, uSecondaryColor, colorMix);
    
    // Final composite
    float alpha = falloff * (waves * 0.5 + 0.5) * uIntensity;
    alpha *= lerp(1.0, petalPattern * 0.8 + 0.2, 0.5);
    
    return color * alpha;
}

technique Technique1
{
    pass ValorAura
    {
        PixelShader = compile ps_3_0 PS_ValorAura();
    }
}
