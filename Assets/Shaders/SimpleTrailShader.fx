// =============================================================================
// MagnumOpus Simple Trail Shader - PS 2.0 Compatible
// =============================================================================
// Simplified trail effects within PS 2.0 limits
// 5 passes: Flame, Ice, Lightning, Nature, Cosmic
// =============================================================================

sampler uImage0 : register(s0);

float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uTime;
float uIntensity;
float uProgress; // 0-1 along trail length

// =============================================================================
// STYLE 1: FLAME - Fire trail
// =============================================================================
float4 FlameTrail(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseColor = tex2D(uImage0, coords);
    
    // Width falloff with flicker
    float edge = abs(coords.y - 0.5) * 2.0;
    float flicker = sin(coords.x * 20.0 + uTime * 10.0) * 0.1 + 0.9;
    float alpha = saturate(1.0 - edge * 1.5) * flicker;
    
    // Orange to red gradient
    float3 flameColor = lerp(uColor, uSecondaryColor, edge);
    flameColor *= baseColor.rgb * uIntensity;
    
    // Fade toward end
    alpha *= saturate(1.0 - coords.x);
    
    return float4(flameColor, alpha * uOpacity * sampleColor.a * baseColor.a);
}

// =============================================================================
// STYLE 2: ICE - Crystalline trail
// =============================================================================
float4 IceTrail(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseColor = tex2D(uImage0, coords);
    
    // Sharp crystalline edges
    float edge = abs(coords.y - 0.5) * 2.0;
    float crystal = step(0.4, sin(coords.x * 30.0 + edge * 10.0) * 0.5 + 0.5);
    float alpha = saturate(1.0 - edge * 1.3) * (0.7 + crystal * 0.3);
    
    // Ice blue to white
    float3 iceColor = lerp(uColor, float3(1, 1, 1), edge * 0.5);
    iceColor *= baseColor.rgb * uIntensity;
    
    // Fade toward end
    alpha *= saturate(1.0 - coords.x);
    
    return float4(iceColor, alpha * uOpacity * sampleColor.a * baseColor.a);
}

// =============================================================================
// STYLE 3: LIGHTNING - Electric trail
// =============================================================================
float4 LightningTrail(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseColor = tex2D(uImage0, coords);
    
    // Jagged electric pattern
    float edge = abs(coords.y - 0.5) * 2.0;
    float jag = sin(coords.x * 50.0 + uTime * 15.0) * 0.15;
    float bolt = saturate(1.0 - abs(edge - 0.3 + jag) * 5.0);
    
    // Electric flicker
    float flicker = sin(uTime * 20.0) * 0.3 + 0.7;
    float alpha = bolt * flicker;
    
    // White core, colored edge
    float3 lightningColor = lerp(float3(1, 1, 1), uColor, edge);
    lightningColor *= baseColor.rgb * uIntensity;
    
    // Fade toward end
    alpha *= saturate(1.0 - coords.x * 0.8);
    
    return float4(lightningColor, alpha * uOpacity * sampleColor.a * baseColor.a);
}

// =============================================================================
// STYLE 4: NATURE - Organic vine trail
// =============================================================================
float4 NatureTrail(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseColor = tex2D(uImage0, coords);
    
    // Flowing organic shape
    float edge = abs(coords.y - 0.5) * 2.0;
    float wave = sin(coords.x * 8.0 - uTime * 2.0) * 0.1;
    float alpha = saturate(1.0 - (edge + wave) * 1.2);
    
    // Green gradient
    float3 vineColor = lerp(uColor, uSecondaryColor, coords.x);
    vineColor *= baseColor.rgb * uIntensity;
    
    // Fade toward end
    alpha *= saturate(1.0 - coords.x);
    
    return float4(vineColor, alpha * uOpacity * sampleColor.a * baseColor.a);
}

// =============================================================================
// STYLE 5: COSMIC - Starfield trail
// =============================================================================
float4 CosmicTrail(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseColor = tex2D(uImage0, coords);
    
    // Nebula-like gradient
    float edge = abs(coords.y - 0.5) * 2.0;
    float alpha = saturate(1.0 - edge * 1.2);
    
    // Color shift
    float shift = coords.x + uTime * 0.2;
    float3 cosmicColor;
    cosmicColor.r = sin(shift * 3.0) * 0.3 + 0.7;
    cosmicColor.g = sin(shift * 3.0 + 2.0) * 0.3 + 0.7;
    cosmicColor.b = sin(shift * 3.0 + 4.0) * 0.3 + 0.7;
    
    cosmicColor *= uColor * baseColor.rgb * uIntensity;
    
    // Fade toward end
    alpha *= saturate(1.0 - coords.x);
    
    return float4(cosmicColor, alpha * uOpacity * sampleColor.a * baseColor.a);
}

// =============================================================================
// TECHNIQUES
// =============================================================================

technique FlameTechnique
{
    pass FlamePass
    {
        PixelShader = compile ps_2_0 FlameTrail();
    }
}

technique IceTechnique
{
    pass IcePass
    {
        PixelShader = compile ps_2_0 IceTrail();
    }
}

technique LightningTechnique
{
    pass LightningPass
    {
        PixelShader = compile ps_2_0 LightningTrail();
    }
}

technique NatureTechnique
{
    pass NaturePass
    {
        PixelShader = compile ps_2_0 NatureTrail();
    }
}

technique CosmicTechnique
{
    pass CosmicPass
    {
        PixelShader = compile ps_2_0 CosmicTrail();
    }
}
