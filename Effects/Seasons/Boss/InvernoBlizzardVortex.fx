// ══════════════════════════════════════════════════════════╁E
// InvernoBlizzardVortex.fx  ESeasons/Inverno attack flash
// Blizzard vortex swirl  Eice-blue spiral with frost crystal
// patterns radiating from the center of the attack burst.
// ══════════════════════════════════════════════════════════╁E

sampler uImage0 : register(s0);
float2 uCenter;
float uIntensity;
float4 uColor;
float uTime;

float hash(float2 p)
{
    return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
}

float noise(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);
    f = f * f * (3.0 - 2.0 * f);
    float a = hash(i);
    float b = hash(i + float2(1.0, 0.0));
    float c = hash(i + float2(0.0, 1.0));
    float d = hash(i + float2(1.0, 1.0));
    return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
}

float4 PS_BlizzardVortex(float2 uv : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, uv);
    float2 delta = uv - uCenter;
    float dist = length(delta);
    float angle = atan2(delta.y, delta.x);

    // Spiral vortex  Elogarithmic spiral arms
    float spiralAngle = angle - dist * 12.0 + uTime * 6.0;
    float spiral = sin(spiralAngle * 3.0) * 0.5 + 0.5;
    spiral = pow(spiral, 2.0) * smoothstep(0.5, 0.0, dist);

    // Frost crystal pattern  E6-fold symmetry
    float crystalAngle = angle + uTime * 1.5;
    float crystal = abs(sin(crystalAngle * 3.0));
    crystal = pow(crystal, 4.0);
    float crystalBranch = abs(sin(crystalAngle * 6.0 + dist * 18.0));
    crystalBranch = pow(crystalBranch, 8.0) * smoothstep(0.35, 0.0, dist);

    // Radial falloff
    float radialFade = exp(-dist * 4.5) * uIntensity;

    // Snowflake sparkle
    float sparkle = hash(floor(float2(uv.x * 40.0 + uTime, uv.y * 40.0)));
    float sparkleMask = step(0.93, sparkle) * radialFade;
    float twinkle = sin(uTime * 5.0 + sparkle * 30.0) * 0.4 + 0.6;

    // Colors: ice blue core, frost white spirals, silver crystals
    float4 iceBlue = uColor;
    float4 frostWhite = float4(0.85, 0.92, 1.0, 1.0);
    float4 silverCrystal = float4(0.75, 0.82, 0.95, 1.0);

    float spiralMix = spiral * 0.6 + crystal * 0.4;
    float4 vortexColor = lerp(iceBlue, frostWhite, spiralMix);

    float alpha = (spiral * 0.5 + crystalBranch * 0.4 + sparkleMask * 0.3) * radialFade;

    float4 result = base;
    result.rgb += vortexColor.rgb * alpha;
    result.rgb += silverCrystal.rgb * crystalBranch * radialFade * 0.7;
    result.rgb += frostWhite.rgb * sparkleMask * twinkle * 1.2;

    return result;
}

technique Technique1
{
    pass BlizzardVortex
    {
        PixelShader = compile ps_3_0 PS_BlizzardVortex();
    }
}
