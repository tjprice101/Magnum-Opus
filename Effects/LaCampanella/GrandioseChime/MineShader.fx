// =============================================================================
// Grandiose Chime  EMine Shader (Enhanced)
// =============================================================================
// Proximity mine with bell-mandala alarm pattern. Dormant: slow pulsing glow
// with faint concentric music staff circles. Armed: rapid alarm flash with
// expanding danger rings, fire corona at outer perimeter, and rotating
// angular warning geometry. Centre features a bright bell-glyph.
// Distinct from all other weapons  Ea STATIC radial trap effect.
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
float uPhase;         // 0=dormant, 1=armed/triggered
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

float4 MinePS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);

    float2 centred = coords - 0.5;
    float dist = length(centred) * 2.0;
    float angle = atan2(centred.y, centred.x);
    float armed = saturate(uPhase);

    // --- Core glow: bell-shaped (bright center, dip, bright rim) ---
    float coreGlow = exp(-dist * dist * 10.0);
    float rimGlow = exp(-pow((dist - 0.8) * 5.0, 2.0));
    float bellShape = coreGlow + rimGlow * armed * 0.4;

    // --- Alarm pulse (accelerates with arm state) ---
    float alarmRate = lerp(1.5, 12.0, armed);
    float alarm = sin(uTime * alarmRate);
    alarm = smoothstep(-0.3, 0.5, alarm);  // Sharp onset, soft falloff
    alarm = lerp(0.55, 1.0, alarm);

    // --- Concentric alarm rings (more rings when armed) ---
    float ringCount = lerp(3.0, 6.0, armed);
    float ringSpeed = lerp(2.0, 6.0, armed);
    float rings = sin((dist * ringCount - uTime * ringSpeed) * 6.28318) * 0.5 + 0.5;
    rings = pow(rings, lerp(4.0, 2.5, armed));  // Sharp dormant, broader armed
    rings *= saturate(1.0 - dist) * saturate(dist * 3.0);  // Ring band mask

    // --- Rotating angular warning geometry (8-fold when armed) ---
    float warnAngle = angle * 4.0 + uTime * 2.0 * armed;
    float warnPattern = cos(warnAngle) * 0.5 + 0.5;
    warnPattern = pow(warnPattern, 3.0);
    float warnBand = exp(-pow((dist - 0.55) * 6.0, 2.0));
    float warnings = warnPattern * warnBand * armed;

    // --- Fire corona at outer perimeter ---
    float coronaZone = saturate(dist - 0.5) * saturate(1.1 - dist);
    float2 fireUV = float2(angle * 2.0 + uTime * 0.4, dist * uNoiseScale * 3.0);
    float fireTurb = SmoothNoise(fireUV * 4.0);
    float corona = fireTurb * coronaZone * (0.3 + armed * 0.7);

    // --- Glyph overlay from secondary texture ---
    float4 secTex = tex2D(uImage1, coords);
    float glyphMask = lerp(0.0, secTex.r, uHasSecondaryTex) * coreGlow;

    // --- Ember sparks (scattered at perimeter when armed) ---
    float2 sparkUV = float2(angle * 12.0, dist * 20.0) + uTime * float2(1.5, 0.3);
    float sparks = HashNoise(sparkUV);
    sparks = step(0.93, sparks) * coronaZone * armed;

    // --- Color: dormant amber ↁEarmed fierce orange ↁEtriggered white-flash ---
    float3 dormantColor = uColor * 0.5;
    float3 armedColor = uColor;
    float3 dangerColor = uSecondaryColor;
    float3 flashColor = float3(1.0, 0.95, 0.82);
    float3 sparkColor = float3(1.0, 0.7, 0.2);

    float3 color = lerp(dormantColor, armedColor, armed);
    color = lerp(color, flashColor, coreGlow * (0.3 + armed * 0.5));
    color += dangerColor * rings * 0.5;
    color += armedColor * warnings * 0.4;
    color += uColor * corona * 0.6;
    color += flashColor * glyphMask * 0.35;
    color += sparkColor * sparks * 2.5;

    // --- Alarm-reactive alpha ---
    float alpha = (bellShape * alarm + rings * 0.3 + warnings * 0.15 + corona * 0.2 + sparks * 0.2)
                  * uOpacity * sampleColor.a * baseTex.a;
    alpha *= lerp(0.4, 1.0, armed);
    float3 finalColor = color * uIntensity * baseTex.rgb;

    return ApplyOverbright(finalColor, saturate(alpha));
}

technique AutoPass
{
    pass P0
    {
        PixelShader = compile ps_3_0 MinePS();
    }
}
