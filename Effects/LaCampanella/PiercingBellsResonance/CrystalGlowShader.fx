// =============================================================================
// Piercing Bells Resonance  ECrystal Glow Shader (Enhanced)
// =============================================================================
// Ethereal crystal glow for seeking crystal projectiles. Sharp 6-fold
// diamond facets with internal fire caustics that shift as orientation
// changes. Surface reflection sparkle flashes at facet vertices.
// Prismatic edge dispersion separates warm colours at the crystal boundary.
// A pulsing internal heartbeat drives the inner fire intensity.
// =============================================================================

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uTime;
float uIntensity;
float uOverbrightMult;
float uNoiseScale;
float uHasSecondaryTex;

float4 ApplyOverbright(float3 c, float a) { return float4(c * uOverbrightMult, a); }
float HashNoise(float2 p) { return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453); }

float SmoothNoise(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);
    f = f * f * (3.0 - 2.0 * f);
    float a = HashNoise(i);
    float b = HashNoise(i + float2(1, 0));
    float c = HashNoise(i + float2(0, 1));
    float d = HashNoise(i + float2(1, 1));
    return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
}

matrix uWorldViewProjection;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float3 TextureCoordinates : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float3 TextureCoordinates : TEXCOORD0;
};

VertexShaderOutput TrailVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;
    output.Position = mul(input.Position, uWorldViewProjection);
    output.Color = input.Color;
    output.TextureCoordinates = input.TextureCoordinates;
    return output;
}

float4 CrystalGlowPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);

    float2 centred = coords - 0.5;
    float dist = length(centred) * 2.0;
    float angle = atan2(centred.y, centred.x);

    // ---- Sharp 6-fold faceted crystal structure ----
    float facetAngle = angle + uTime * 0.4;
    float facet6 = cos(facetAngle * 3.0);
    float facetEdge = pow(saturate(abs(facet6)), 8.0);
    float facetBody = pow(saturate(facet6 * 0.5 + 0.5), 0.3);

    // ---- Gaussian core with faceted modulation ----
    float coreGlow = exp(-dist * dist / 0.12);
    float facetedGlow = coreGlow * (0.7 + facetBody * 0.3);

    // ---- Internal fire caustics (shifting pattern inside crystal) ----
    float2 causticUV = float2(
        angle * 0.318 + uTime * 0.5,
        dist * uNoiseScale * 4.0 + sin(angle * 3.0 + uTime) * 0.3
    );
    float caustics = SmoothNoise(causticUV * 3.0);
    caustics = pow(caustics, 1.5) * coreGlow;

    // ---- Surface sparkle flashes at facet vertices ----
    float sparkleAngle = facetAngle * 6.0;
    float sparklePhase = frac(uTime * 2.0 + sparkleAngle * 0.159);
    float sparkle = pow(saturate(cos(sparkleAngle) * 0.5 + 0.5), 16.0);
    sparkle *= pow(saturate(1.0 - abs(sparklePhase - 0.5) * 4.0), 2.0);
    sparkle *= saturate(dist * 3.0 - 0.3) * saturate(1.0 - dist);

    // ---- Prismatic edge dispersion ----
    float edgeBand = saturate(dist * 2.0 - 0.5) * saturate(1.0 - dist);
    float dispersion = edgeBand * facetEdge;

    // ---- Pulsing internal heartbeat ----
    float heartbeat = sin(uTime * 5.0) * 0.5 + 0.5;
    heartbeat = pow(heartbeat, 2.0);
    float innerPulse = saturate(1.0 - dist * 2.5) * (0.6 + heartbeat * 0.4);

    // Secondary texture
    float2 secUV = coords * 2.0;
    float4 noiseTex = tex2D(uImage1, secUV);
    float texVal = lerp(1.0, noiseTex.r * 0.4 + 0.7, uHasSecondaryTex * 0.3);

    // ---- 5-stop colour gradient ----
    float heat = facetedGlow * (0.6 + caustics * 0.4);
    float3 cDark  = float3(0.10, 0.04, 0.02);
    float3 cAmber = uColor * 0.5;
    float3 cFlame = uColor;
    float3 cGold  = uSecondaryColor;
    float3 cWhite = float3(1.0, 0.96, 0.88);

    float3 color = cDark;
    color = lerp(color, cAmber, smoothstep(0.0,  0.2,  heat));
    color = lerp(color, cFlame, smoothstep(0.2,  0.4,  heat));
    color = lerp(color, cGold,  smoothstep(0.4,  0.65, heat));
    color = lerp(color, cWhite, smoothstep(0.65, 0.9,  heat));

    // Facet edge highlights
    color += uSecondaryColor * facetEdge * coreGlow * 0.4;
    // Inner heartbeat pulse
    color = lerp(color, cWhite, innerPulse * 0.3);
    // Surface sparkles flash white
    color += cWhite * sparkle * 0.8;
    // Prismatic dispersion: slight warm-shift at edges
    color += float3(0.3, 0.15, 0.0) * dispersion * 0.3;

    color *= texVal;

    // ---- Composite ----
    float outerFade = smoothstep(1.0, 0.6, dist);
    float alpha = (facetedGlow * 0.4 + caustics * 0.2 + innerPulse * 0.2 + sparkle * 0.15 + dispersion * 0.1)
                * outerFade * uOpacity * baseTex.a;
    float3 finalColor = color * uIntensity * baseTex.rgb;

    return ApplyOverbright(finalColor, alpha * sampleColor.a);
}

technique AutoPass
{
    pass P0
    {
        VertexShader = compile vs_3_0 TrailVS();
        PixelShader = compile ps_3_0 CrystalGlowPS();
    }
}
