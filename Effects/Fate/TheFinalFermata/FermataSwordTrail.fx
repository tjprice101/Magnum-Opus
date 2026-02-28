// FermataSwordTrail.fx — Trail shader for Fermata spectral swords.
// Two techniques: SwordTrailMain (colored gradient trail) and SwordTrailGlow (additive bloom trail).
// Target: ps_2_0  |  Standard uniforms.

sampler uImage0 : register(s0);

float uTime;
float4 uColor;
float uOpacity;
float uSaturation;
float uIntensity;

// Palette colors (set from C#)
float4 uPrimaryColor;    // FermataPurple
float4 uSecondaryColor;  // FermataCrimson

// === MAIN TRAIL TECHNIQUE ===

float4 PS_SwordTrailMain(float2 coords : TEXCOORD0) : COLOR0
{
    // coords.x = along trail (0=tip, 1=tail), coords.y = across width
    float4 texSample = tex2D(uImage0, coords);
    
    // Width fade: strongest at center, fading at edges
    float edgeFade = 1.0 - abs(coords.y * 2.0 - 1.0);
    edgeFade = edgeFade * edgeFade;
    
    // Length fade: strongest at tip, fading toward tail
    float lengthFade = 1.0 - coords.x;
    lengthFade = lengthFade * lengthFade;
    
    // Gradient from primary (tip) to secondary (tail)
    float4 gradient = lerp(uPrimaryColor, uSecondaryColor, coords.x);
    
    // Time-based shimmer
    float shimmer = sin(coords.x * 12.0 + uTime * 3.0) * 0.15 + 0.85;
    
    float4 result = gradient * texSample * edgeFade * lengthFade * shimmer;
    result.a *= uOpacity;
    result.rgb *= uIntensity;
    
    return result;
}

// === GLOW TRAIL TECHNIQUE ===

float4 PS_SwordTrailGlow(float2 coords : TEXCOORD0) : COLOR0
{
    float4 texSample = tex2D(uImage0, coords);
    
    // Soft gaussian-like falloff across width
    float center = abs(coords.y * 2.0 - 1.0);
    float glow = exp(-center * center * 4.0);
    
    // Length fade
    float lengthFade = (1.0 - coords.x);
    
    // Pulsing glow
    float pulse = sin(uTime * 2.0 + coords.x * 6.0) * 0.2 + 0.8;
    
    float4 result = uColor * glow * lengthFade * pulse * texSample;
    result.a *= uOpacity * 0.6;
    result.rgb *= uIntensity * 1.5;
    
    return result;
}

technique SwordTrailMain
{
    pass Pass0
    {
        PixelShader = compile ps_2_0 PS_SwordTrailMain();
    }
}

technique SwordTrailGlow
{
    pass Pass0
    {
        PixelShader = compile ps_2_0 PS_SwordTrailGlow();
    }
}
