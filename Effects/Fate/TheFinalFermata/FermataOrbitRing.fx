// FermataOrbitRing.fx — Hexagonal orbit ring visualization.
// Renders the geometric formation ring where spectral swords orbit.
// Target: ps_2_0  |  Standard uniforms.

sampler uImage0 : register(s0);

float uTime;
float4 uColor;
float uOpacity;
float uIntensity;
float uRadius;       // Current orbit radius in UV space
float uSwordCount;   // Number of active swords (3 or 6)

// Constants
static const float PI = 3.14159265;
static const float TWO_PI = 6.28318530;

float4 PS_OrbitRing(float2 coords : TEXCOORD0) : COLOR0
{
    // Center UV
    float2 center = coords * 2.0 - 1.0;
    float dist = length(center);
    float angle = atan2(center.y, center.x);
    
    // Ring band: glow centered on the orbit radius
    float ringDist = abs(dist - uRadius);
    float ring = exp(-ringDist * ringDist * 80.0);
    
    // Hexagonal modulation: create bright nodes at sword positions
    float nodeCount = max(uSwordCount, 3.0);
    float nodeAngle = frac(angle / TWO_PI * nodeCount + uTime * 0.1) * TWO_PI;
    float nodePulse = pow(max(0.0, cos(nodeAngle)), 8.0);
    
    // Connecting lines between nodes (subtle)
    float lineGlow = pow(max(0.0, cos(angle * nodeCount + uTime * 0.5)), 2.0) * 0.3;
    
    // Rotating temporal particles along the ring
    float particleAngle = angle + uTime * 0.8;
    float particles = pow(max(0.0, sin(particleAngle * 12.0)), 16.0) * ring;
    
    // Combine
    float brightness = ring * (0.4 + nodePulse * 0.6 + lineGlow) + particles * 0.3;
    
    // Color: base ring color with gold highlights at nodes
    float4 ringColor = uColor;
    float4 nodeHighlight = float4(1.0, 0.82, 0.27, 1.0); // TimeGold
    float4 finalColor = lerp(ringColor, nodeHighlight, nodePulse * 0.5);
    
    float4 result = finalColor * brightness;
    result.a *= uOpacity;
    result.rgb *= uIntensity;
    
    return result;
}

technique OrbitRing
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 PS_OrbitRing();
    }
}
