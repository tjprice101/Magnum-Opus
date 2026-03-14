// SwanFeatherTrail.fx -- Swan Lake boss movement trail (4-phase aware)
//
// Phase 1 (White Swan):   White feather vane pattern. Clean barbs, precise edges. No rainbow.
// Phase 2 (Black Swan):   Trail splits -- white vane + dark mirror vane beneath. Prismatic at boundary.
// Phase 3 (Duality War):  Rapid white/black segment alternation along length. Interference prismatic.
// Phase 4 (Death of Swan): Drains to gray. Vane dissolves into noise. Ghost feather fading.

sampler uImage0 : register(s0);
sampler uNoiseTex : register(s1);

float4 uColor;
float uTrailWidth;
float uFadeRate;
float uTime;
float uPhase;  // 1-4
float uDrain;  // 0-1

float3 hsl2rgb(float h, float s, float l)
{
    float3 rgb = clamp(abs(fmod(h * 6.0 + float3(0, 4, 2), 6.0) - 3.0) - 1.0, 0.0, 1.0);
    return l + s * (rgb - 0.5) * (1.0 - abs(2.0 * l - 1.0));
}

float4 PS_FeatherTrail(float2 uv : TEXCOORD0) : COLOR0
{
    float trail = uv.x;
    float width = abs(uv.y - 0.5) * 2.0;

    // Feather shape from noise
    float featherNoise = tex2D(uNoiseTex, float2(uv.x * 3.0 - uTime, uv.y * 2.0)).r;
    float featherShape = smoothstep(1.0, 0.1, width + featherNoise * 0.3);

    // Feather vane barb pattern -- precise stepped edges
    float vane = sin(uv.y * 40.0 + uv.x * 10.0 - uTime * 5.0);
    vane = smoothstep(0.5, 0.8, vane) * featherShape;

    float ageFade = pow(1.0 - trail, uFadeRate);

    // Palette constants
    float3 pureWhite = float3(0.96, 0.96, 1.0);
    float3 voidBlack = float3(0.02, 0.02, 0.03);
    float3 deadGray  = float3(0.45, 0.45, 0.46);

    float3 color = pureWhite;
    float alpha = 0.0;

    if (uPhase < 1.5)
    {
        // Phase 1: Clean white feather vane. No rainbow at all.
        color = pureWhite;
        color += vane * 0.15;
        alpha = featherShape * ageFade * uTrailWidth;
    }
    else if (uPhase < 2.5)
    {
        // Phase 2: Split trail -- white vane + dark mirror beneath
        float mirrorVane = sin(uv.y * 40.0 + uv.x * 10.0 - uTime * 5.0 + 3.14159);
        mirrorVane = smoothstep(0.5, 0.8, mirrorVane) * featherShape;

        // Dark mirror offset slightly behind the white trail
        float darkNoise = tex2D(uNoiseTex, float2(uv.x * 3.0 - uTime * 0.8, uv.y * 2.0 + 0.1)).r;
        float darkShape = smoothstep(1.0, 0.15, width + darkNoise * 0.35);

        float3 whiteVane = pureWhite * (featherShape + vane * 0.15);
        float3 blackVane = voidBlack * darkShape;

        // Prismatic shimmer at the boundary between white and black regions
        float boundary = abs(featherShape - darkShape);
        boundary = smoothstep(0.0, 0.15, boundary) * (1.0 - smoothstep(0.15, 0.35, boundary));

        float hue = frac(trail * 2.0 + uTime * 0.2);
        float3 rainbow = hsl2rgb(hue, 0.9, 0.65);

        color = lerp(whiteVane, blackVane, (1.0 - featherShape) * 0.5);
        color = lerp(color, rainbow, boundary * 0.6);

        alpha = max(featherShape, darkShape * 0.7) * ageFade * uTrailWidth;
    }
    else if (uPhase < 3.5)
    {
        // Phase 3: Rapid alternation along trail length
        float segVal = frac(uv.x * 6.0 + uTime * 4.0);
        float segment = step(0.5, segVal);
        float3 segColor = lerp(pureWhite, voidBlack, segment);

        // Prismatic flash at segment boundaries
        float nearHalf = abs(segVal - 0.5);
        float nearEdge = min(segVal, 1.0 - segVal);
        float boundDist = min(nearHalf, nearEdge);
        float interference = 1.0 - smoothstep(0.0, 0.1, boundDist);

        float hue = frac(uv.x * 3.0 + uTime * 0.5);
        float3 rainbow = hsl2rgb(hue, 1.0, 0.7);

        color = segColor * (featherShape + vane * 0.1);
        color = lerp(color, rainbow, interference * 0.8);

        alpha = featherShape * ageFade * uTrailWidth;
    }
    else
    {
        // Phase 4: Drain to gray. Vane dissolves. Ghost of feather.
        float dissolution = featherNoise * uDrain;
        float dissolvedShape = smoothstep(dissolution, dissolution + 0.1, featherShape);

        float3 drainedColor = lerp(pureWhite, deadGray, uDrain);
        color = drainedColor * dissolvedShape;

        alpha = dissolvedShape * ageFade * uTrailWidth * (1.0 - uDrain * 0.9);
    }

    return float4(color, 1) * saturate(alpha);
}

technique Technique1
{
    pass FeatherTrail { PixelShader = compile ps_3_0 PS_FeatherTrail(); }
}
