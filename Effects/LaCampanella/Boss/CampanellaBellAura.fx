// ══════════════════════════════════════════════════════════╁E
// CampanellaBellAura.fx  ELa Campanella boss presence aura
// Concentric bell-shaped resonance waves radiating outward
// with infernal orange/black smoke undulation.
// ══════════════════════════════════════════════════════════╁E

sampler uImage0 : register(s0);
float2 uCenter;
float uRadius;
float uIntensity;
float4 uPrimaryColor;   // Infernal Orange
float4 uSecondaryColor;  // Black smoke
float uTime;

float noise(float2 p)
{
    return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
}

float4 PS_BellAura(float2 uv : TEXCOORD0) : COLOR0
{
    float2 centered = uv - 0.5;
    float dist = length(centered);
    float angle = atan2(centered.y, centered.x);
    
    // Bell-shaped resonance rings (thicker waves that pulse)
    float bellWave = sin(dist * 25.0 - uTime * 4.0);
    bellWave = pow(abs(bellWave), 3.0) * sign(bellWave); // Sharper peaks
    float resonance = saturate(bellWave * 0.5 + 0.5);
    
    // Smoke undulation
    float smoke = noise(float2(angle * 5.0 + uTime, dist * 8.0 - uTime * 2.0));
    smoke = smoothstep(0.2, 0.8, smoke);
    
    // Falloff from center
    float radiusNorm = uRadius / 200.0;
    float falloff = 1.0 - smoothstep(0.0, radiusNorm, dist);
    
    // Color: orange fire core, black smoke outer
    float4 color = lerp(uSecondaryColor, uPrimaryColor, resonance * falloff);
    
    // Fire flicker
    float flicker = noise(float2(uTime * 10.0, angle * 3.0));
    color += uPrimaryColor * flicker * 0.2 * falloff;
    
    float alpha = falloff * (resonance * 0.6 + smoke * 0.3 + 0.1) * uIntensity;
    
    return color * alpha;
}

technique Technique1
{
    pass BellAura { PixelShader = compile ps_3_0 PS_BellAura(); }
}
