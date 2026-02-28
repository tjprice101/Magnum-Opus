// =============================================================================
// Opus Ultima — Seeker Trail Shader
// =============================================================================
// Homing seeker projectile trail. Thin luminous ribbon that flows from
// crimson at the head to golden sparks at the tail, with cosmic dust accents.
// vs_3_0 + ps_3_0, width correction, uWorldViewProjection
// =============================================================================

sampler uImage0 : register(s0);

float3 uColor;           // Primary: OpusCrimson
float3 uSecondaryColor;  // Secondary: GloryGold
float uOpacity;
float uTime;
float uIntensity;
float uOverbrightMult;
float uScrollSpeed;
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

float4 ApplyOverbright(float3 c, float a) { return float4(c * uOverbrightMult, a); }

float HashNoise(float2 p) { return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453); }

float SmoothNoise(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);
    f = f * f * (3.0 - 2.0 * f);
    float a = HashNoise(i);
    float b = HashNoise(i + float2(1.0, 0.0));
    float c = HashNoise(i + float2(0.0, 1.0));
    float d = HashNoise(i + float2(1.0, 1.0));
    return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
}

float4 SeekerTrailPS(VertexOutput input) : COLOR0
{
    float2 coords = input.TexCoord.xy;
    float widthFactor = input.TexCoord.z;
    coords.y = (coords.y - 0.5) / max(widthFactor, 0.001) + 0.5;

    float progress = coords.x;  // 0=head, 1=tail
    float cross = abs(coords.y - 0.5) * 2.0;

    // Thin ribbon shape
    float ribbon = saturate(1.0 - cross);
    ribbon = ribbon * ribbon;

    // Scrolling energy flow
    float2 flowUV = float2(progress * 6.0 - uTime * uScrollSpeed, coords.y * 4.0);
    float flow = SmoothNoise(flowUV * 4.0);

    // Sparkle at tail
    float2 sparkUV = coords * float2(20.0, 6.0) + float2(uTime * 3.0, 0.0);
    float spark = HashNoise(sparkUV);
    spark = step(0.95, spark) * ribbon * progress;

    // Color: crimson head → gold body → fading sparks at tail
    float3 crimsonCol = uColor;
    float3 goldCol = uSecondaryColor;
    float3 sparkCol = float3(1.0, 0.85, 0.5);
    float3 whiteHot = float3(0.96, 0.94, 1.0);

    float3 color = lerp(crimsonCol, goldCol, progress);
    color = lerp(color, whiteHot, ribbon * (1.0 - progress) * 0.5);
    color += sparkCol * spark * 2.0;
    color *= (0.7 + flow * 0.3);

    float alpha = ribbon * (1.0 - progress * 0.7) * uOpacity * input.Color.a;
    float3 finalColor = color * uIntensity;

    return ApplyOverbright(finalColor, alpha);
}

technique OpusSeekerTrailMain
{
    pass P0
    {
        VertexShader = compile vs_3_0 VS_Main();
        PixelShader = compile ps_3_0 SeekerTrailPS();
    }
}
