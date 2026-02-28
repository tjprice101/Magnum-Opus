// ============================================================================
// DissonanceOrbAura.fx — DissonanceOfSecrets orb aura shader
// Renders the growing cascade orb's radial aura with pulsing concentric
// rings of purple/green energy, noise distortion, and soft bloom glow
// ============================================================================

sampler uImage0 : register(s0);  // Base texture
sampler uImage1 : register(s1);  // Noise texture

float3 uColor;           // Primary orb color (secret purple)
float3 uSecondaryColor;  // Secondary color (cascade green)
float uOpacity;           // Overall opacity
float uTime;              // Elapsed time for animation
float uIntensity;         // Orb size / charge intensity (ramps 0→1)
matrix uWorldViewProjection;

struct VertexInput
{
    float2 Position : POSITION0;
    float4 Color : COLOR0;
    float3 TexCoord : TEXCOORD0;  // .xy = UV, .z = width correction
};

struct VertexOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float3 TexCoord : TEXCOORD0;
};

VertexOutput VS_Main(VertexInput input)
{
    VertexOutput output;
    output.Position = mul(float4(input.Position, 0, 1), uWorldViewProjection);
    output.Color = input.Color;
    output.TexCoord = input.TexCoord;
    return output;
}

// --- Technique 1: Radial aura with pulsing concentric rings ---

float4 PS_OrbAura(VertexOutput input) : COLOR0
{
    float2 coords = input.TexCoord.xy;

    // Width correction from vertex data
    float widthFactor = input.TexCoord.z;
    coords.y = (coords.y - 0.5) / max(widthFactor, 0.001) + 0.5;

    // Radial distance from center
    float2 center = float2(0.5, 0.5);
    float2 toCenter = coords - center;
    float dist = length(toCenter);
    float angle = atan2(toCenter.y, toCenter.x);

    // Sample noise for organic distortion
    float2 noiseUV = float2(angle * 0.3 + uTime * 0.2, dist * 3.0 - uTime * 0.4);
    float noise = tex2D(uImage1, noiseUV).r;

    // Concentric rings — multiple frequencies for layered complexity
    float ring1 = sin((dist * 18.0 - uTime * 2.5) + noise * 2.0) * 0.5 + 0.5;
    float ring2 = sin((dist * 12.0 + uTime * 1.8) + noise * 1.5) * 0.5 + 0.5;
    float rings = ring1 * 0.6 + ring2 * 0.4;

    // Radial fade — stronger at center, fades at edges
    float radialFade = 1.0 - smoothstep(0.2, 0.5, dist);

    // Color: purple core shifting to green at ring peaks, modulated by noise
    float colorMix = saturate(rings * 0.7 + noise * 0.3);
    float3 auraColor = lerp(uColor, uSecondaryColor, colorMix);

    // Intensity scales with orb charge
    float brightness = rings * radialFade * uIntensity * 1.2;

    // Add subtle angular variation for mystery
    float angularVar = sin(angle * 5.0 + uTime) * 0.1 + 0.9;
    brightness *= angularVar;

    float3 finalColor = auraColor * brightness;
    float finalAlpha = radialFade * rings * uOpacity * uIntensity * input.Color.a;

    return float4(finalColor, finalAlpha);
}

// --- Technique 2: Soft Gaussian-like bloom glow around the orb ---

float4 PS_OrbGlow(VertexOutput input) : COLOR0
{
    float2 coords = input.TexCoord.xy;
    float widthFactor = input.TexCoord.z;
    coords.y = (coords.y - 0.5) / max(widthFactor, 0.001) + 0.5;

    // Radial distance from center
    float2 center = float2(0.5, 0.5);
    float dist = length(coords - center);

    // Gaussian-like soft glow falloff
    float glow = exp(-dist * dist * 5.0);

    // Gentle pulse with time
    float pulse = sin(uTime * 2.0) * 0.12 + 0.88;

    // Mix colors based on radial distance — purple at core, green-tinted at edge
    float3 glowColor = lerp(uColor, uSecondaryColor, dist * 1.5) * pulse;

    float glowAlpha = glow * uOpacity * 0.35 * uIntensity * input.Color.a;

    return float4(glowColor, glowAlpha);
}

technique DissonanceOrbAuraMain
{
    pass P0
    {
        VertexShader = compile vs_3_0 VS_Main();
        PixelShader = compile ps_3_0 PS_OrbAura();
    }
}

technique DissonanceOrbAuraGlow
{
    pass P0
    {
        VertexShader = compile vs_3_0 VS_Main();
        PixelShader = compile ps_3_0 PS_OrbGlow();
    }
}
