// ============================================================
//  GearSwing.fx — Clockwork Harmony (Melee Sword)
//  Clair de Lune — "Music Box Pendulum Arc"
//
//  Brass-gold pendulum sweep with embedded clockwork gears.
//  The arc swings like a grand clockwork pendulum, scattering
//  tiny gear-tooth notches along its edge.
//  Two techniques: GearSwingArc, GearSwingTrail
// ============================================================

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float4 uColor;
float4 uSecondaryColor;
float  uOpacity;
float  uTime;
float  uIntensity;
float  uOverbrightMult;
float  uScrollSpeed;
float  uDistortionAmt;
bool   uHasSecondaryTex;
float  uSecondaryTexScale;
float2 uSecondaryTexScroll;
float  uPhase; // Combo phase 0..1

float4 GearSwingArcPS(float2 uv : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, uv);

    // Gear-tooth pattern along the sweep edge
    float edge = 1.0 - abs(uv.y - 0.5) * 2.0;
    float gearTeeth = abs(sin(uv.x * 40.0 + uTime * uScrollSpeed * 2.0));
    gearTeeth = step(0.7, gearTeeth) * edge;

    // Pendulum energy concentration at center
    float center = pow(edge, 2.0);
    float sweep = pow(1.0 - uv.x, 0.8); // Brighter at swing start

    // Brass-gold noise layer
    float noiseVal = 0.5;
    if (uHasSecondaryTex)
    {
        float2 noiseUV = uv * uSecondaryTexScale + float2(-uTime * uScrollSpeed * 0.5, 0.0);
        noiseVal = tex2D(uImage1, noiseUV).r;
    }

    // Clockwork brass→moonbeam gold gradient
    float3 brassColor = uColor.rgb;
    float3 goldColor = uSecondaryColor.rgb;
    float3 whiteHot = float3(0.96, 0.97, 1.0);

    float phaseBlend = sin(uPhase * 3.14159);
    float3 color = lerp(brassColor, goldColor, uv.x * 0.6 + noiseVal * 0.3);
    color += whiteHot * center * sweep * 0.3 * phaseBlend;
    color += brassColor * gearTeeth * 0.4;

    float3 finalColor = color * uIntensity;
    finalColor *= (1.0 + uOverbrightMult * center * sweep * 0.4);

    float alpha = base.a * uOpacity * (center * 0.6 + gearTeeth * 0.2 + sweep * 0.2);

    return float4(finalColor, alpha);
}

float4 GearSwingTrailPS(float2 uv : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, uv);

    // Lingering pendulum afterswing trail
    float trailFade = pow(1.0 - uv.x, 1.5);
    float edge = 1.0 - abs(uv.y - 0.5) * 2.0;
    float trail = trailFade * pow(edge, 3.0);

    float3 trailColor = lerp(uColor.rgb, uSecondaryColor.rgb, 0.5) * uIntensity * 0.6;
    trailColor *= uOverbrightMult;

    float alpha = base.a * uOpacity * trail * 0.5;

    return float4(trailColor, alpha);
}

technique GearSwingArc
{
    pass P0 { PixelShader = compile ps_3_0 GearSwingArcPS(); }
}

technique GearSwingTrail
{
    pass P0 { PixelShader = compile ps_3_0 GearSwingTrailPS(); }
}
