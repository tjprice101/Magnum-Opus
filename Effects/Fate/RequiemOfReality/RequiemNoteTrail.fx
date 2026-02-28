// =============================================================================
// Requiem of Reality — Note Trail Shader
// =============================================================================
// Trail for seeking cosmic music note projectiles. Ethereal wisps with
// harmonic overtone waves that pulse along the trail. Pink-to-silver gradient
// with void edges. Notes leave a "musical resonance" — frequency harmonics
// visible in the trail cross-section.
// =============================================================================

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float3 uColor;           // DarkPink
float3 uSecondaryColor;  // ConstellationSilver
float uOpacity;
float uTime;
float uIntensity;
float uOverbrightMult;
float uScrollSpeed;
float uNoiseScale;
float uHasSecondaryTex;

float4 ApplyOverbright(float3 c, float a) { return float4(c * uOverbrightMult, a); }
float HashNoise(float2 p) { return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453); }

float4 NoteTrailPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);

    float progress = coords.x;
    float cross = abs(coords.y - 0.5) * 2.0;

    // Cross-section: narrow ethereal wisp
    float core = saturate(1.0 - cross / 0.25);
    core = core * core;
    float body = saturate(1.0 - cross);

    // Harmonic overtone waves (3 frequencies like musical overtones)
    float scrollX = progress - uTime * uScrollSpeed;
    float fundamental = sin(scrollX * 15.0) * 0.5 + 0.5;
    float harmonic2 = sin(scrollX * 30.0 + 1.57) * 0.5 + 0.5;
    float harmonic3 = sin(scrollX * 45.0 + 3.14) * 0.5 + 0.5;
    float harmony = fundamental * 0.5 + harmonic2 * 0.3 + harmonic3 * 0.2;

    // Cross-section ripple (standing wave visualization)
    float standingWave = sin(cross * 6.28 * 3.0) * 0.5 + 0.5;
    standingWave = lerp(1.0, standingWave, 0.2);

    // Ethereal noise wisps
    float2 noiseUV = float2(scrollX * uNoiseScale, coords.y * 2.0);
    float wisps = HashNoise(noiseUV * 4.0);
    wisps = wisps * body * 0.3;

    // Head glow
    float head = saturate(1.0 - progress * 3.0);
    head = head * head;

    // Secondary texture
    float2 secUV = float2(scrollX * 2.0, coords.y);
    float4 secTex = tex2D(uImage1, secUV);
    float detail = lerp(1.0, secTex.r, uHasSecondaryTex * 0.2);

    // Color: DarkPink body → ConstellationSilver harmonics → SupernovaWhite core
    float3 color = uColor * body * 0.6;
    color = lerp(color, uSecondaryColor, harmony * body * 0.5);
    color = lerp(color, float3(1.0, 0.95, 0.92), core * 0.7);
    color += float3(1.0, 0.95, 0.92) * head * 0.4;
    color *= standingWave;
    color += float3(0.47, 0.12, 0.39) * wisps;
    color *= detail;

    float alpha = (body * 0.4 + core * 0.4 + head * 0.2) * harmony;
    alpha *= (1.0 - progress * 0.5) * uOpacity * sampleColor.a * baseTex.a;
    float3 finalColor = color * uIntensity * baseTex.rgb;

    return ApplyOverbright(finalColor, alpha);
}

technique NoteTrailMain
{
    pass P0
    {
        PixelShader = compile ps_3_0 NoteTrailPS();
    }
}
