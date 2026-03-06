// =============================================================================
// Grandiose Chime  EBarrage Shader (Enhanced)
// =============================================================================
// Rapid-fire burning note projectile trail. Strong staccato rhythm is
// VISIBLE  Eeach note pulses bright/dim in rapid succession like a
// pianist's hammers striking bells. Compressed fire dart shape with
// brilliant head flare, musical tremolo vibration in the body, and
// scattered ember sparks in the wake.
// =============================================================================

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uTime;
float uIntensity;
float uOverbrightMult;
float uScrollSpeed;
float uNoiseScale;
float uHasSecondaryTex;

float4 ApplyOverbright(float3 c, float a) { return float4(c * uOverbrightMult, a); }
float HashNoise(float2 p) { return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453); }

float SmoothNoise(float2 uv)
{
    float2 i = floor(uv);
    float2 f = frac(uv);
    float2 u = f * f * (3.0 - 2.0 * f);
    float a = HashNoise(i);
    float b = HashNoise(i + float2(1.0, 0.0));
    float c = HashNoise(i + float2(0.0, 1.0));
    float d = HashNoise(i + float2(1.0, 1.0));
    return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
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

float4 BarragePS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);
    float progress = coords.x;  // 0=head, 1=tail
    float yDist = abs(coords.y - 0.5) * 2.0;

    // --- Compressed fire-dart cross-section ---
    float taper = lerp(1.0, 0.10, progress * progress);
    float core = exp(-pow(yDist / max(taper * 0.3, 0.01), 2.0) * 10.0);
    float body = exp(-pow(yDist / max(taper, 0.01), 2.0) * 4.0);

    // --- Staccato rhythm: sharp bright/dim alternation ---
    float staccatoFreq = 45.0;
    float staccatoPhase = coords.x * staccatoFreq - uTime * uScrollSpeed * 4.0;
    float staccato = sin(staccatoPhase);
    staccato = smoothstep(-0.2, 0.3, staccato);  // Sharp attack, soft release
    staccato = staccato * 0.30 + 0.70;

    // --- Musical tremolo vibration (cross-section oscillates) ---
    float tremolo = sin(coords.x * 20.0 + uTime * 15.0) * 0.015 * body;
    float tremoloCore = exp(-pow((yDist + tremolo) / max(taper * 0.3, 0.01), 2.0) * 10.0);
    core = max(core, tremoloCore * 0.7);

    // --- Fire turbulence (smooth, not grainy) ---
    float2 fireUV = float2(coords.x * uNoiseScale * 2.0 - uTime * uScrollSpeed * 1.5,
                            coords.y * 4.0);
    float turb = SmoothNoise(fireUV * 3.0);
    float turb2 = SmoothNoise(fireUV * 6.0 + float2(2.1, 0.7));
    float fire = turb * 0.6 + turb2 * 0.4;
    fire = fire * 0.35 + 0.65;

    // --- Brilliant head flare ---
    float headGlow = exp(-progress * progress * 8.0);
    float headFlare = exp(-progress * 20.0) * 0.6;  // Very sharp leading edge flash

    // --- Ember wake sparks ---
    float wakeZone = saturate(progress * 2.0 - 0.5);
    float2 sparkUV = coords * float2(35.0, 12.0) + uTime * float2(3.0, 0.8);
    float sparks = HashNoise(sparkUV);
    sparks = step(0.94, sparks) * wakeZone * body;

    // Secondary texture
    float2 secUV = float2(coords.x * 2.5 - uTime * 0.6, coords.y);
    float4 secTex = tex2D(uImage1, secUV);
    float secVal = lerp(1.0, 0.7 + secTex.r * 0.5, uHasSecondaryTex * 0.3);

    // --- 4-stop color gradient ---
    float3 whiteHotHead = float3(1.0, 0.97, 0.90);
    float3 goldBody = uSecondaryColor;
    float3 orangeFlame = uColor;
    float3 smokyTail = uColor * 0.25;
    float3 sparkColor = float3(1.0, 0.8, 0.3);

    float3 color = lerp(whiteHotHead, goldBody, saturate(progress * 2.5));
    color = lerp(color, orangeFlame, saturate(progress * 1.8 - 0.4));
    color = lerp(color, smokyTail, saturate(progress * 1.5 - 0.7));
    color = lerp(color, whiteHotHead, headFlare);
    color += sparkColor * sparks * 2.5;

    color *= staccato * fire * secVal;

    // --- Trail fade ---
    float trailFade = exp(-progress * 1.8);
    trailFade *= saturate(progress * 12.0);  // Quick onset

    float alpha = (body * 0.4 + core * 0.5 + headFlare * 0.3 + sparks * 0.2)
                  * trailFade * uOpacity * sampleColor.a * baseTex.a;
    float3 finalColor = color * uIntensity * baseTex.rgb;

    return ApplyOverbright(finalColor, saturate(alpha));
}

technique AutoPass
{
    pass P0
    {
        VertexShader = compile vs_3_0 TrailVS();
        PixelShader = compile ps_3_0 BarragePS();
    }
}
