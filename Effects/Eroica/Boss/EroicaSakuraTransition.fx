// ══════════════════════════════════════════════════════════╁E
// EroicaSakuraTransition.fx  EEroica boss phase transition
// Sakura petal dissolve/reveal with golden light breakthrough.
// Used during Phase 1ↁE transition and HP tier changes.
// ══════════════════════════════════════════════════════════╁E

sampler uImage0 : register(s0);
sampler uNoiseTex : register(s1);
float uTransitionProgress;  // 0 = not started, 1 = complete
float4 uFromColor;          // Scarlet pre-transition
float4 uToColor;            // Gold post-transition
float uIntensity;
float uTime;

float hash(float2 p)
{
    return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
}

float4 PS_SakuraTransition(float2 uv : TEXCOORD0) : COLOR0
{
    float2 centered = uv - 0.5;
    float dist = length(centered);
    float angle = atan2(centered.y, centered.x);
    
    // Petal-shaped mask (5 petals)
    float petalAngle = fmod(angle + 3.14159, 3.14159 * 2.0 / 5.0) - 3.14159 / 5.0;
    float petalDist = cos(petalAngle) * 0.4;
    float petalMask = smoothstep(petalDist + 0.1, petalDist, dist);
    
    // Noise drives the dissolve threshold
    float n = tex2D(uNoiseTex, uv * 2.0 + float2(uTime * 0.3, 0)).r;
    float dissolveThreshold = uTransitionProgress * 1.2;
    
    // Petal reveal: noise < threshold means revealed
    float revealed = step(n, dissolveThreshold);
    
    // Edge glow at the dissolve boundary
    float edgeDist = abs(n - dissolveThreshold);
    float edgeGlow = smoothstep(0.08, 0.0, edgeDist);
    
    // Color blend
    float4 baseColor = lerp(uFromColor, uToColor, uTransitionProgress);
    float4 edgeColor = float4(1.0, 0.85, 0.6, 1.0); // Warm gold-white edge
    
    // Sakura scatter: small bright dots floating upward
    float scatter = hash(floor(uv * 30.0 + float2(0, -uTime * 1.5)));
    float sakura = step(0.96, scatter) * uTransitionProgress;
    float4 sakuraColor = float4(1.0, 0.6, 0.75, 1.0); // Pink
    
    float4 color = baseColor * revealed;
    color += edgeColor * edgeGlow * uIntensity * 2.0;
    color += sakuraColor * sakura * 0.8;
    
    float alpha = (revealed * 0.3 + edgeGlow * uIntensity + sakura) * petalMask;
    alpha *= sin(uTransitionProgress * 3.14159); // Fade in then out
    
    return color * saturate(alpha);
}

technique Technique1
{
    pass SakuraTransition
    {
        PixelShader = compile ps_3_0 PS_SakuraTransition();
    }
}
