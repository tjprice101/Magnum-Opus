// ══════════════════════════════════════════════════════════╁E
// EroicaHeroicTrail.fx  EEroica boss dash/movement trail
// Flaming golden trail with embedded scarlet embers that 
// flow along the trail path with forward motion blur.
// ══════════════════════════════════════════════════════════╁E

sampler uImage0 : register(s0);
sampler uNoiseTex : register(s1);
float4 uColor;
float uTrailWidth;
float uFadeRate;
float uTime;

float4 PS_HeroicTrail(float2 uv : TEXCOORD0) : COLOR0
{
    // Trail UV: x = along trail, y = across trail width
    float trailProgress = uv.x;  // 0 = newest, 1 = oldest
    float trailWidth = abs(uv.y - 0.5) * 2.0;  // 0 = center, 1 = edge
    
    // Flowing fire noise
    float2 noiseUV = float2(uv.x * 3.0 - uTime * 2.0, uv.y * 2.0 + uTime * 0.5);
    float fireNoise = tex2D(uNoiseTex, noiseUV).r;
    
    // Edge flame distortion
    float edgeFlame = smoothstep(0.8, 0.2, trailWidth + fireNoise * 0.3);
    
    // Ember hotspots
    float2 emberUV = float2(uv.x * 5.0 - uTime * 4.0, uv.y * 3.0);
    float embers = tex2D(uNoiseTex, emberUV).r;
    embers = smoothstep(0.7, 0.9, embers);
    
    // Age fade
    float ageFade = pow(1.0 - trailProgress, uFadeRate * 2.0);
    
    // Color: gold core, scarlet edges, white-hot embers
    float4 goldCore = uColor;
    float4 scarletEdge = float4(0.8, 0.2, 0.15, 1.0);
    float4 whiteHot = float4(1.0, 0.95, 0.85, 1.0);
    
    float4 baseColor = lerp(scarletEdge, goldCore, edgeFlame);
    baseColor = lerp(baseColor, whiteHot, embers * 0.7);
    
    float alpha = edgeFlame * ageFade * uTrailWidth;
    alpha += embers * ageFade * 0.5;  // Ember glow persists slightly
    
    return baseColor * saturate(alpha);
}

technique Technique1
{
    pass HeroicTrail
    {
        PixelShader = compile ps_3_0 PS_HeroicTrail();
    }
}
