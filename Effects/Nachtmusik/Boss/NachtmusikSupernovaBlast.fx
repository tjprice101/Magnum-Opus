// ══════════════════════════════════════════════════════════╁E
// NachtmusikSupernovaBlast.fx  ENachtmusik boss attack shader
// Supernova explosion VFX: radial starburst with shockwave
// ring, cosmic blue core, silver-white outer corona.
// ══════════════════════════════════════════════════════════╁E

sampler uImage0 : register(s0);
sampler uNoiseTex : register(s1);
float4 uColor;
float uIntensity;
float uTime;

float hash(float2 p)
{
    return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
}

float4 PS_SupernovaBlast(float2 uv : TEXCOORD0) : COLOR0
{
    float2 centered = uv - 0.5;
    float dist = length(centered);
    float angle = atan2(centered.y, centered.x);

    // Expanding shockwave ring
    float ringRadius = frac(uTime * 0.8) * 0.5;
    float ring = smoothstep(0.03, 0.0, abs(dist - ringRadius));
    ring *= (1.0 - frac(uTime * 0.8)); // Fades as it expands

    // Radial starburst rays
    float rays = abs(sin(angle * 8.0 + uTime * 2.0));
    rays = pow(rays, 4.0) * smoothstep(0.5, 0.0, dist);

    // Core glow  Eintense white-blue center
    float core = smoothstep(0.15, 0.0, dist);
    float corePulse = sin(uTime * 10.0) * 0.2 + 0.8;

    // Debris noise  Eejected matter
    float debris = tex2D(uNoiseTex, float2(angle / 6.28 + 0.5, dist * 3.0 - uTime * 2.0)).r;
    float debrisMask = smoothstep(0.4, 0.7, debris) * smoothstep(0.5, 0.1, dist);

    // Colors: cosmic blue core, white ring, silver outer debris
    float4 coreColor = float4(0.4, 0.6, 1.0, 1.0);
    float4 whiteHot = float4(1.0, 0.97, 1.0, 1.0);
    float4 silverDebris = uColor;

    float4 color = coreColor * core * corePulse * uIntensity * 2.0;
    color += whiteHot * ring * uIntensity * 1.5;
    color += uColor * rays * uIntensity * 0.6;
    color += silverDebris * debrisMask * uIntensity * 0.5;

    return color;
}

technique Technique1
{
    pass SupernovaBlast
    {
        PixelShader = compile ps_3_0 PS_SupernovaBlast();
    }
}
