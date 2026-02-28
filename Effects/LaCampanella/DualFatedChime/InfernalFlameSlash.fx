// =============================================================================
// Dual-Fated Chime — Infernal Flame Slash Shader (Enhanced)
// =============================================================================
// Heavy greatsword slash arc rendered as a massive sheet of bell-fire.
// Multi-octave FBM turbulence drives organic flame shapes. Bell-toll pulse
// nodes create rhythmic intensity peaks along the arc. Ember detachment
// at outer edges and volumetric smoke layering add depth.
// UV.x = progress along swing arc, UV.y = cross-section (0=outer, 1=inner).
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
float uSecondaryTexScale;
float uSecondaryTexScroll;

float4 ApplyOverbright(float3 color, float alpha) { return float4(color * uOverbrightMult, alpha); }

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

// 4-octave FBM for rich organic flame shapes
float FBM(float2 uv)
{
    float value = 0.0;
    float amp = 0.5;
    float2 p = uv;
    value += SmoothNoise(p) * amp; p *= 2.07; amp *= 0.5;
    value += SmoothNoise(p) * amp; p *= 2.03; amp *= 0.5;
    value += SmoothNoise(p) * amp; p *= 2.01; amp *= 0.5;
    value += SmoothNoise(p) * amp;
    return value;
}

// Asymmetric cross-section: sharp core, smoky outer with ember fringe
float SlashProfile(float y)
{
    float center = abs(y - 0.5) * 2.0;
    float core = exp(-center * center * 8.0);        // Tight gaussian core
    float body = saturate(1.0 - center * 1.1);       // Linear body
    body *= body;
    return core * 0.6 + body * 0.4;
}

float4 InfernalFlameSlashPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);

    // Asymmetric slash profile with sharp hot core
    float profile = SlashProfile(coords.y);
    float coreIntensity = exp(-pow(abs(coords.y - 0.5) * 4.0, 2.0));

    // FBM flame turbulence — two layers scrolling at different speeds
    float2 flameUV1 = coords * float2(uNoiseScale * 2.5, 3.0);
    flameUV1.x -= uTime * uScrollSpeed * 1.4;
    flameUV1.y += sin(coords.x * 6.0 + uTime * 4.0) * 0.06;

    float2 flameUV2 = coords * float2(uNoiseScale * 1.8, 2.0);
    flameUV2.x -= uTime * uScrollSpeed * 0.7;
    flameUV2.y -= cos(coords.x * 4.5 + uTime * 3.0) * 0.04;

    float flame1 = FBM(flameUV1 + float2(uTime * 1.5, 0));
    float flame2 = FBM(flameUV2 + float2(uTime * 0.8, uTime * 0.3));
    float flame = saturate(flame1 * 0.65 + flame2 * 0.45 - 0.08);

    // Secondary noise texture blending for extra detail
    float2 secUV = coords * uSecondaryTexScale;
    secUV.x -= uTime * uSecondaryTexScroll;
    float4 noiseTex = tex2D(uImage1, secUV);
    float texDetail = lerp(1.0, 0.5 + noiseTex.r * 0.7, uHasSecondaryTex);
    flame *= texDetail;

    // Bell-toll pulse nodes — rhythmic intensity peaks along the arc
    float bellFreq = 5.0;
    float bellToll = sin(coords.x * bellFreq * 6.28318 - uTime * 6.0);
    bellToll = pow(saturate(bellToll), 3.0);  // Sharp bright peaks
    float bellBrightness = bellToll * profile * 0.35;

    // 5-stop fire gradient: smoke → ember → flame → gold → white-hot
    float3 smokeColor = float3(0.04, 0.015, 0.005);
    float3 emberColor = float3(0.5, 0.12, 0.02);
    float3 flameColor = uColor;
    float3 goldColor = uSecondaryColor;
    float3 whiteHot = float3(1.0, 0.96, 0.88);

    float gradientT = saturate(profile * flame * 1.6);
    float3 color = lerp(smokeColor, emberColor, saturate(gradientT * 3.0));
    color = lerp(color, flameColor, saturate(gradientT * 2.0 - 0.3));
    color = lerp(color, goldColor, saturate(gradientT * 1.5 - 0.6));
    color = lerp(color, whiteHot, coreIntensity * saturate(gradientT - 0.5) * 0.7);

    // Bell-toll highlights punch through at pulse nodes
    color = lerp(color, whiteHot, bellBrightness);

    // Ember detachment at outer edges — scattered bright sparks
    float edgeDist = abs(coords.y - 0.5) * 2.0;
    float emberZone = saturate(edgeDist - 0.5) * 2.0;
    float emberSparks = HashNoise(coords * float2(40.0, 20.0) + uTime * float2(3.0, 1.0));
    emberSparks = step(0.92, emberSparks) * emberZone * flame;
    color += float3(1.0, 0.7, 0.2) * emberSparks * 2.0;

    // Fire flicker — compound frequency for organic feel
    float flicker = sin(uTime * 14.0 + coords.x * 8.0) * 0.08;
    flicker += sin(uTime * 23.0 + coords.x * 13.0) * 0.05;
    flicker = flicker + 0.87;

    // Trail fade with smooth attack and bell-shaped decay
    float trailFade = saturate(coords.x * 8.0);              // Quick fade-in
    trailFade *= exp(-coords.x * 1.2);                        // Exponential decay
    trailFade *= 1.0 + bellToll * 0.15;                       // Brighter at bell nodes

    float alpha = profile * trailFade * flame * uOpacity * baseTex.a;
    alpha += emberSparks * 0.3;  // Sparks contribute to alpha
    float3 finalColor = color * uIntensity * flicker * baseTex.rgb;

    return ApplyOverbright(finalColor, saturate(alpha) * sampleColor.a);
}

technique InfernalFlameMain
{
    pass P0
    {
        PixelShader = compile ps_2_0 InfernalFlameSlashPS();
    }
}
