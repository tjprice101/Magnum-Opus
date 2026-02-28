// =============================================================================
// Coda of Annihilation — Swing Arc Shader
// =============================================================================
// Renders the held swing arc/smear with cosmic fire and musical pulse effects.
// Wide, fiery arc with cosmic gradient and noise distortion.
// =============================================================================

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uTime;
float uIntensity;

float QuadBump(float x) { return x * (4.0 - x * 4.0); }

float HashNoise(float2 p)
{
    return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
}

float4 SwingArcMainPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);
    
    // Arc cross-section with heavier center
    float edgeFade = QuadBump(coords.y);
    edgeFade *= edgeFade; // Sharper concentration toward center
    
    // Scrolling cosmic fire
    float2 fireUV = coords;
    fireUV.x -= uTime * 1.2;
    fireUV.y += sin(coords.x * 4.0 + uTime * 5.0) * 0.08;
    
    float noise = HashNoise(fireUV * 6.0 + uTime);
    float fireIntensity = noise * 0.7 + 0.3;
    
    // Musical pulse — ripples along the arc
    float pulse = sin(coords.x * 14.0 - uTime * 10.0) * 0.5 + 0.5;
    pulse = pulse * pulse * 0.4;
    
    // Color: cosmic gradient from weapon tint to annihilation white
    float3 color = lerp(uColor, uSecondaryColor, coords.x * 0.6);
    color += float3(0.4, 0.3, 0.5) * pulse;
    color *= fireIntensity;
    
    // Alpha
    float lengthFade = 1.0 - coords.x * coords.x;
    float alpha = edgeFade * lengthFade * uOpacity * uIntensity * baseTex.a;
    
    return float4(color * alpha, alpha);
}

technique SwingArcMain
{
    pass SwingArcMainPass
    {
        PixelShader = compile ps_2_0 SwingArcMainPS();
    }
}
