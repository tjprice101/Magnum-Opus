// =============================================================================
// Symphonic Bellfire Annihilator  EExplosion Shader (Enhanced)
// =============================================================================
// Massive detonation with mushroom-cloud structure. A rising hot centre
// spreads into a flattening cap. Multi-ring blast wavefronts propagate
// outward with bell-toll shockwave character. FBM fire turbulence fills
// the interior. Radial debris streaks and a scorched hollow centre give
// depth and violence to each impact.
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
float uPhase;         // 0=fresh detonation -> 1=fading smoke
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

float FBM(float2 p)
{
    float v = 0.0;
    v += SmoothNoise(p) * 0.5;
    v += SmoothNoise(p * 2.03 + 1.7) * 0.25;
    v += SmoothNoise(p * 4.01 + 3.3) * 0.125;
    v += SmoothNoise(p * 7.97 + 5.1) * 0.0625;
    return v / 0.9375;
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

float4 ExplosionPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);

    float2 centred = coords - 0.5;
    float dist = length(centred) * 2.0;
    float angle = atan2(centred.y, centred.x);
    float expand = saturate(uPhase);

    // ---- Mushroom cloud structure: rising stem + spreading cap ----
    float stemRadius = 0.15 * (1.0 - expand * 0.3);
    float capRadius = expand * 0.85;
    float stem = exp(-dist * dist / (stemRadius * stemRadius * 0.5)) * saturate(1.0 - expand * 1.2);
    float cap = exp(-(dist - capRadius * 0.5) * (dist - capRadius * 0.5) / (capRadius * capRadius * 0.3));
    cap *= saturate(dist * 2.0);

    // ---- Multi-ring blast wavefronts ----
    float front1R = expand * 0.92;
    float front2R = expand * 0.65;
    float front1 = exp(-(dist - front1R) * (dist - front1R) / 0.006);
    float front2 = exp(-(dist - front2R) * (dist - front2R) / 0.004) * 0.5;

    // ---- FBM fire turbulence inside blast ----
    float2 fireUV = float2(angle * 0.318 * uNoiseScale, dist * 3.0 + uTime * 0.3);
    float fire = FBM(fireUV * 3.0);
    fire *= saturate(capRadius - dist + 0.1);

    // ---- Radial debris ray streaks ----
    float debris = pow(saturate(cos(angle * 10.0) * 0.5 + 0.5), 10.0);
    float debrisMask = exp(-abs(dist - capRadius * 0.4) * 5.0);
    debris *= debrisMask * saturate(expand * 4.0);

    // ---- Scorched dark hollow centre ----
    float scorch = exp(-dist * dist / 0.01);
    scorch *= saturate(expand * 2.0 - 0.3);

    // Secondary texture for smoke detail
    float2 secUV = float2(angle * 0.318, dist * 2.0);
    float4 secTex = tex2D(uImage1, secUV);
    float smokeDetail = lerp(1.0, secTex.r * 0.5 + 0.6, uHasSecondaryTex * 0.3);

    // ---- 5-stop colour gradient ----
    float3 cScorch = float3(0.03, 0.02, 0.01);
    float3 cSmoke  = float3(0.12, 0.06, 0.03);
    float3 cFlame  = uColor;
    float3 cBright = uSecondaryColor;
    float3 cWhite  = float3(1.0, 0.96, 0.86);

    float fireIntensity = (stem + cap * 0.5 + fire * 0.5);
    float3 color = cSmoke;
    color = lerp(color, cFlame,  smoothstep(0.1,  0.35, fireIntensity));
    color = lerp(color, cBright, smoothstep(0.35, 0.6,  fireIntensity));
    color = lerp(color, cWhite,  smoothstep(0.6,  0.9,  fireIntensity));

    // Blast wavefronts flash bright
    color += cWhite * front1 * 0.6;
    color += cBright * front2 * 0.5;
    // Debris rays
    color += float3(1.0, 0.75, 0.3) * debris * 0.5;
    // Scorched centre goes dark
    color = lerp(color, cScorch, scorch * 0.7);

    color *= smokeDetail;

    // ---- Fade as explosion dissipates ----
    float fadeFactor = saturate(1.0 - expand * expand * 0.6);
    float bellToll = pow(saturate(cos(uTime * 6.0) * 0.5 + 0.5), 4.0) * 0.08 + 0.92;

    float alpha = (stem * 0.2 + cap * 0.3 + front1 * 0.2 + front2 * 0.1 + fire * 0.15 + debris * 0.1)
                * fadeFactor * uOpacity * bellToll * baseTex.a;
    float3 finalColor = color * uIntensity * bellToll * baseTex.rgb;

    return ApplyOverbright(finalColor, alpha * sampleColor.a);
}

technique AutoPass
{
    pass P0
    {
        VertexShader = compile vs_3_0 TrailVS();
        PixelShader = compile ps_3_0 ExplosionPS();
    }
}
