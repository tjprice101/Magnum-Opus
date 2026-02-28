// ============================================================================
// DissonanceOrbAura.fx — DissonanceOfSecrets orb aura
// UNIQUE SIGNATURE: Counter-rotating arcane circles — two concentric ring
// patterns rotating in opposite directions, with bright intersection nodes
// where ring elements cross. Runic sigil aesthetic with glowing node points.
// The forbidden knowledge encoded in geometric wheel patterns.
// ============================================================================

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uTime;
float uIntensity;
matrix uWorldViewProjection;

struct VertexInput
{
    float2 Position : POSITION0;
    float4 Color : COLOR0;
    float3 TexCoord : TEXCOORD0;
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

float4 PS_OrbAura(VertexOutput input) : COLOR0
{
    float2 coords = input.TexCoord.xy;
    float widthFactor = input.TexCoord.z;
    coords.y = (coords.y - 0.5) / max(widthFactor, 0.001) + 0.5;

    float2 center = float2(0.5, 0.5);
    float2 toCenter = coords - center;
    float dist = length(toCenter);
    float angle = atan2(toCenter.y, toCenter.x);

    // Outer ring — rotates clockwise, 7 evenly-spaced nodes
    float outerAngle = angle + uTime * 1.2;
    float outerRingDist = abs(dist - 0.35);
    float outerRing = smoothstep(0.03, 0.0, outerRingDist);
    // 7 bright nodes on outer ring
    float outerNodes = pow(max(0.0, cos(outerAngle * 3.5)), 8.0); // 7 peaks
    float outerNodeGlow = outerNodes * smoothstep(0.06, 0.0, outerRingDist);

    // Inner ring — rotates counter-clockwise, 5 nodes
    float innerAngle = angle - uTime * 1.8;
    float innerRingDist = abs(dist - 0.2);
    float innerRing = smoothstep(0.025, 0.0, innerRingDist);
    // 5 bright nodes on inner ring
    float innerNodes = pow(max(0.0, cos(innerAngle * 2.5)), 8.0); // 5 peaks
    float innerNodeGlow = innerNodes * smoothstep(0.05, 0.0, innerRingDist);

    // Connecting spokes — lines from outer nodes to inner ring
    float spokeCount = 7.0;
    float spokeAngle = frac((outerAngle + 3.14159) / (6.2831 / spokeCount));
    float spoke = smoothstep(0.06, 0.0, abs(spokeAngle - 0.5));
    spoke *= smoothstep(0.18, 0.2, dist) * smoothstep(0.37, 0.35, dist); // between rings
    spoke *= 0.4;

    // Intersection brightness — where inner and outer features overlap
    float intersection = outerNodeGlow * innerRing + innerNodeGlow * outerRing;
    intersection = saturate(intersection * 3.0);

    // Noise for organic irregularity
    float2 noiseUV = float2(angle * 0.3 + uTime * 0.15, dist * 3.0);
    float noise = tex2D(uImage1, noiseUV).r;

    // Core glow
    float coreGlow = exp(-dist * dist * 12.0) * uIntensity * 0.6;

    // Radial fade
    float radialFade = 1.0 - smoothstep(0.3, 0.5, dist);

    // Color composition
    // Rings: purple
    float3 ringColor = uColor * (outerRing + innerRing) * 0.7;
    // Nodes: bright green
    float3 nodeColor = uSecondaryColor * 2.0 * (outerNodeGlow + innerNodeGlow);
    // Intersections: white-green flash
    float3 interColor = lerp(uSecondaryColor, float3(1, 1, 1), 0.4) * intersection * 1.5;
    // Spokes: dim green
    float3 spokeColor = uSecondaryColor * 0.5 * spoke;
    // Core: purple-white
    float3 coreColor = lerp(uColor, float3(1, 1, 1), 0.3) * coreGlow;

    float3 finalColor = ringColor + nodeColor + interColor + spokeColor + coreColor;
    finalColor *= uIntensity;

    // Angular breathing
    float breath = sin(angle * 3.0 + uTime * 2.0) * 0.08 + 0.92;
    finalColor *= breath;

    float alphaContent = outerRing + innerRing + outerNodeGlow + innerNodeGlow +
                          intersection + spoke + coreGlow;
    float finalAlpha = saturate(alphaContent) * radialFade * uOpacity * input.Color.a;

    return float4(finalColor, finalAlpha);
}

float4 PS_OrbGlow(VertexOutput input) : COLOR0
{
    float2 coords = input.TexCoord.xy;
    float widthFactor = input.TexCoord.z;
    coords.y = (coords.y - 0.5) / max(widthFactor, 0.001) + 0.5;

    float2 center = float2(0.5, 0.5);
    float dist = length(coords - center);

    float glow = exp(-dist * dist * 4.0);
    float pulse = sin(uTime * 2.5) * 0.12 + 0.88;
    float3 glowColor = lerp(uColor, uSecondaryColor, dist * 1.5) * pulse;

    float glowAlpha = glow * uOpacity * 0.3 * uIntensity * input.Color.a;

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
