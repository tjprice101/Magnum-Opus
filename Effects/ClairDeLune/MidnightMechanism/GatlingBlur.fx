// ============================================================
//  GatlingBlur.fx — Midnight Mechanism (Ranged Gatling)
//  Clair de Lune — "Clockwork Barrel Blur"
//
//  Rapid-fire motion blur from a clockwork gatling gun.
//  Barrel trails stack into a directional blur with embedded
//  brass gear-tooth muzzle flash accents.
//  Two techniques: GatlingBarrelBlur, GatlingMuzzle
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

float4 GatlingBarrelBlurPS(float2 uv : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, uv);

    // Directional motion blur — multiple offset samples along X
    float4 blur = float4(0, 0, 0, 0);
    float blurAmount = uDistortionAmt * 0.05;
    [unroll]
    for (int i = -3; i <= 3; i++)
    {
        float2 offset = float2(i * blurAmount, 0);
        blur += tex2D(uImage0, uv + offset) * (1.0 / 7.0);
    }

    // Barrel rotation streaks
    float rotStreak = abs(sin(uv.y * 25.0 + uTime * uScrollSpeed * 8.0));
    rotStreak = pow(rotStreak, 6.0);

    // Brass mechanical noise
    float mechNoise = 0.5;
    if (uHasSecondaryTex)
    {
        float2 noiseUV = uv * uSecondaryTexScale + float2(-uTime * uScrollSpeed, 0);
        mechNoise = tex2D(uImage1, noiseUV).r;
    }

    // Core trail brightness
    float centerBand = exp(-pow((uv.y - 0.5) * 2.0, 2.0) * 6.0);

    // Clockwork brass + soft blue palette
    float3 brassTrail = uColor.rgb;
    float3 blueSteel = uSecondaryColor.rgb;

    float3 color = lerp(blueSteel, brassTrail, centerBand) * blur.a;
    color += brassTrail * rotStreak * 0.3;
    color += blueSteel * mechNoise * centerBand * 0.2;

    float3 finalColor = color * uIntensity;
    finalColor *= (1.0 + uOverbrightMult * centerBand * 0.3);

    float alpha = blur.a * uOpacity * (centerBand * 0.7 + rotStreak * 0.15 + 0.15);

    return float4(finalColor, alpha);
}

float4 GatlingMuzzlePS(float2 uv : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, uv);

    // Radial muzzle flash burst
    float dist = length(uv - 0.5) * 2.0;
    float flash = exp(-dist * dist * 6.0);
    float rays = pow(abs(sin(atan2(uv.y - 0.5, uv.x - 0.5) * 4.0 + uTime * 12.0)), 4.0);
    float muzzle = flash * (0.5 + rays * 0.5);

    float3 muzzleColor = lerp(uColor.rgb, float3(0.96, 0.97, 1.0), flash * 0.6) * uIntensity * uOverbrightMult;
    float alpha = base.a * uOpacity * muzzle * 0.6;

    return float4(muzzleColor, alpha);
}

technique GatlingBarrelBlur
{
    pass P0 { PixelShader = compile ps_3_0 GatlingBarrelBlurPS(); }
}

technique GatlingMuzzle
{
    pass P0 { PixelShader = compile ps_3_0 GatlingMuzzlePS(); }
}
