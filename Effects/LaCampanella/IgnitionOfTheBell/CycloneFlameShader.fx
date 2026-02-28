// =============================================================================
// Ignition of the Bell — Cyclone Flame Shader (Enhanced)
// =============================================================================
// Tightening fire tornado vortex for the cyclone AoE. Logarithmic spiral
// arms pull inward with FBM turbulence. A calm dark eye sits at the centre
// surrounded by a bright fire wall. Bell-toll pressure oscillations
// cause periodic brightening. Debris sparks scatter through the vortex.
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

float4 CycloneFlamePS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);

    float2 centred = coords - 0.5;
    float dist = length(centred) * 2.0;
    float angle = atan2(centred.y, centred.x);

    // ---- Logarithmic spirals: tightening vortex arms ----
    float spiralAngle = angle + log(max(dist, 0.01)) * 3.0 - uTime * uScrollSpeed * 3.5;
    float spiral1 = pow(saturate(cos(spiralAngle * 3.0) * 0.5 + 0.5), 2.0);
    float spiral2 = pow(saturate(cos(spiralAngle * 3.0 + 2.094) * 0.5 + 0.5), 2.5);
    float spirals = max(spiral1, spiral2 * 0.7);

    // ---- FBM turbulence on the spiral surface ----
    float2 turbUV = float2(angle * 0.5 + dist * 2.0, dist * 3.0 - uTime * 1.5);
    float turb = FBM(turbUV * uNoiseScale);

    // Secondary texture
    float2 secUV = coords * uSecondaryTexScale;
    secUV += float2(sin(uTime * 0.3), cos(uTime * 0.25)) * 0.1;
    float4 noiseTex = tex2D(uImage1, secUV);
    float texDetail = lerp(1.0, noiseTex.r * 0.6 + 0.5, uHasSecondaryTex * 0.6);

    // ---- Vortex wall: bright fire ring, dark calm eye ----
    float wallPeak = 0.55;
    float wallProfile = exp(-(dist - wallPeak) * (dist - wallPeak) / 0.06);
    float eyeMask = smoothstep(0.20, 0.10, dist);
    float outerFade = smoothstep(1.0, 0.7, dist);

    // ---- Bell-toll pressure oscillations ----
    float toll = pow(saturate(cos(dist * 6.0 - uTime * 5.0) * 0.5 + 0.5), 3.0);
    toll *= wallProfile;

    // ---- Debris spark scatter ----
    float2 debrisUV = float2(angle * 4.0 + uTime * 2.0, dist * 20.0);
    float debris = HashNoise(debrisUV);
    debris = pow(saturate(debris - 0.88) * 8.3, 2.0) * outerFade;

    // ---- 5-stop colour gradient ----
    float fireIntensity = spirals * turb * wallProfile;
    float3 cDark   = float3(0.06, 0.02, 0.01);
    float3 cEmber  = uColor * 0.45;
    float3 cFlame  = uColor;
    float3 cBright = uSecondaryColor;
    float3 cWhite  = float3(1.0, 0.95, 0.80);

    float3 color = cDark;
    color = lerp(color, cEmber,  smoothstep(0.0,  0.2,  fireIntensity));
    color = lerp(color, cFlame,  smoothstep(0.2,  0.45, fireIntensity));
    color = lerp(color, cBright, smoothstep(0.45, 0.7,  fireIntensity));
    color = lerp(color, cWhite,  smoothstep(0.7,  1.0,  fireIntensity));

    // Bell-toll nodes flash bright
    color = lerp(color, cWhite, toll * 0.5);
    // Eye is soot-dark
    color = lerp(color, cDark, eyeMask * 0.8);
    // Debris sparks flash white-gold
    color += cWhite * debris * 0.6;

    color *= texDetail;

    // ---- Composite alpha ----
    float pulse = SmoothNoise(float2(uTime * 6.0, 0.0)) * 0.1 + 0.9;
    float alpha = (wallProfile * spirals * 0.6 + toll * 0.2 + debris * 0.15 + turb * outerFade * 0.15)
                * (1.0 - eyeMask * 0.6) * uOpacity * pulse * baseTex.a;
    float3 finalColor = color * uIntensity * pulse * baseTex.rgb;

    return ApplyOverbright(finalColor, alpha * sampleColor.a);
}

technique TrailPass
{
    pass P0
    {
        PixelShader = compile ps_2_0 CycloneFlamePS();
    }
}
