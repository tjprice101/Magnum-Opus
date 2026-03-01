// EnigmaVoidAura.fx — Enigma boss ambient void presence
// Swirling void energy with watching eye patterns and eerie green flames
sampler uImage0 : register(s0);
float2 uCenter;
float uRadius;
float uIntensity;
float4 uPrimaryColor;   // Deep purple
float4 uSecondaryColor;  // Eerie green
float uTime;

float hash(float2 p) { return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453); }

float4 PS_VoidAura(float2 uv : TEXCOORD0) : COLOR0
{
    float2 centered = uv - 0.5;
    float dist = length(centered);
    float angle = atan2(centered.y, centered.x);
    float radiusNorm = uRadius / 200.0;
    float falloff = 1.0 - smoothstep(0.0, radiusNorm, dist);
    // Void swirl (spiraling inward)
    float spiral = sin(angle * 3.0 + dist * 15.0 - uTime * 2.0) * 0.5 + 0.5;
    float spiral2 = sin(angle * 5.0 - dist * 20.0 + uTime * 1.5) * 0.5 + 0.5;
    float voidSwirl = spiral * 0.6 + spiral2 * 0.4;
    // Eye patterns — occasional circular shapes that fade in/out
    float eyePhase = sin(uTime * 1.5 + angle * 2.0);
    float eyeRing = abs(dist - 0.2 - eyePhase * 0.05);
    float eyeMask = smoothstep(0.02, 0.0, eyeRing) * step(0.5, eyePhase);
    // Mystery noise
    float n = hash(floor(uv * 15.0 + float2(sin(uTime), cos(uTime))));
    float mystery = smoothstep(0.7, 0.9, n);
    // Color: void purple swirl with green flame accents
    float4 color = lerp(uPrimaryColor, uSecondaryColor, voidSwirl * 0.5);
    color += uSecondaryColor * mystery * 0.3;
    color += float4(0.5, 0.8, 0.3, 1) * eyeMask * 0.5; // Green eyes
    float alpha = falloff * (voidSwirl * 0.5 + mystery * 0.2 + eyeMask * 0.5 + 0.1) * uIntensity;
    return color * saturate(alpha);
}

technique Technique1
{
    pass VoidAura { PixelShader = compile ps_3_0 PS_VoidAura(); }
}
