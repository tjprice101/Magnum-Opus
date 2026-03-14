// SwanMoodTransition.fx -- Swan Lake phase transition effect (4-phase aware)
//
// Phase 1->2 (Graceful->Tempest):   White field CRACKS. Black revealed underneath. Prismatic bleed at crack edges.
// Phase 2->3 (Tempest->Duality):    Both black/white shatter. Kaleidoscope geometric shards. Alternating panels.
// Any->Phase 4 (Death):             All color drains. World goes gray. Mourning dissolve.

sampler uImage0 : register(s0);
sampler uNoiseTex : register(s1);

float uTransitionProgress;  // 0-1 over the transition
float4 uFromColor;
float4 uToColor;
float uIntensity;
float uTime;
float uPhase;   // target phase being transitioned TO (2, 3, or 4)
float uDrain;   // 0-1

float3 hsl2rgb(float h, float s, float l)
{
    float3 rgb = clamp(abs(fmod(h * 6.0 + float3(0, 4, 2), 6.0) - 3.0) - 1.0, 0.0, 1.0);
    return l + s * (rgb - 0.5) * (1.0 - abs(2.0 * l - 1.0));
}

float4 PS_MoodTransition(float2 uv : TEXCOORD0) : COLOR0
{
    float2 centered = uv - 0.5;
    float dist = length(centered);
    float angle = atan2(centered.y, centered.x);
    float progress = uTransitionProgress;

    // Palette constants
    float3 pureWhite = float3(0.96, 0.96, 1.0);
    float3 voidBlack = float3(0.02, 0.02, 0.03);
    float3 deadGray  = float3(0.45, 0.45, 0.46);

    // Noise for fracture/shatter patterns
    float n = tex2D(uNoiseTex, uv * 2.5 + float2(uTime * 0.2, 0)).r;
    float featherN = tex2D(uNoiseTex, float2(angle / 6.283 * 3.0, dist * 5.0 + uTime * 0.5)).r;

    float3 color = float3(0, 0, 0);
    float alpha = 0.0;

    if (uPhase < 2.5)
    {
        // Transition to Phase 2: White field CRACKS revealing black underneath
        float crackThreshold = progress * 1.2;
        float reveal = step(n, crackThreshold);

        // Crack edge detection
        float crackEdge = smoothstep(0.06, 0.0, abs(n - crackThreshold));

        // White surface, black revealed underneath
        color = lerp(pureWhite, voidBlack, reveal);

        // Prismatic bleed at crack edges -- first color in the fight
        float hue = frac(n * 3.0 + uTime * 0.15);
        float3 rainbow = hsl2rgb(hue, 0.95, 0.7);
        color = lerp(color, rainbow, crackEdge * 0.85);

        // Feather wisps at edges
        float feathers = smoothstep(0.6, 0.9, featherN) * crackEdge;
        color += pureWhite * feathers * 0.3;

        alpha = (reveal * 0.15 + crackEdge * uIntensity + feathers * 0.4);
        alpha *= sin(progress * 3.14159);
    }
    else if (uPhase < 3.5)
    {
        // Transition to Phase 3: Kaleidoscope shatter -- both colors break
        float shardAngle = frac(angle / 6.283 * 6.0);
        float shardPattern = step(0.5, frac(shardAngle * 3.0 + n * 2.0));

        // Both black and white shatter into alternating geometric panels
        float shatterThreshold = progress * 1.3;
        float shatter = step(n * (0.5 + shardPattern * 0.5), shatterThreshold);

        color = lerp(pureWhite, voidBlack, shardPattern * shatter);

        // Prismatic at shard boundaries
        float shardEdge = smoothstep(0.05, 0.0, abs(n - shatterThreshold * 0.8));
        float hue = frac(angle / 6.283 + uTime * 0.2);
        float3 rainbow = hsl2rgb(hue, 1.0, 0.7);
        color = lerp(color, rainbow, shardEdge * 0.7);

        alpha = (shatter * 0.2 + shardEdge * uIntensity * 0.8);
        alpha *= sin(progress * 3.14159);
    }
    else
    {
        // Transition to Phase 4: Mourning dissolve -- all drains to gray
        float grayReveal = step(n, progress * 1.1);
        float grayEdge = smoothstep(0.04, 0.0, abs(n - progress * 1.1));

        // Color drains away -- the reveal is absence, not a new color
        float3 fromCol = lerp(uFromColor.rgb, deadGray, progress);
        color = lerp(fromCol, deadGray, grayReveal);

        // Edge is desaturated silver -- no prismatic, just loss
        color = lerp(color, float3(0.7, 0.7, 0.72), grayEdge * 0.6);

        alpha = (grayReveal * 0.1 + grayEdge * uIntensity * 0.7);
        alpha *= sin(progress * 3.14159);
    }

    return float4(color, 1) * saturate(alpha);
}

technique Technique1
{
    pass MoodTransition { PixelShader = compile ps_3_0 PS_MoodTransition(); }
}
