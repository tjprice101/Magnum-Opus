// BlitzerExplosionFlash.fx - Explosion flash on attacks
sampler uImage0 : register(s0);
float2 uCenter;
float uRadius;
float uIntensity;
float4 uPrimaryColor;
float4 uSecondaryColor;
float uTime;

float hash(float2 p) {
    return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
}

float noise(float2 p) {
    float2 i = floor(p);
    float2 f = frac(p);
    f = f * f * (3.0 - 2.0 * f);
    float a = hash(i);
    float b = hash(i + float2(1.0, 0.0));
    float c = hash(i + float2(0.0, 1.0));
    float d = hash(i + float2(1.0, 1.0));
    return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
}

float4 PS_BlitzerExplosionFlash(float2 uv : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, uv);
    if (base.a < 0.01) return float4(0, 0, 0, 0);

    float2 offset = uv - uCenter;
    float dist = length(offset);
    float angle = atan2(offset.y, offset.x);

    // Radial explosion burst  Ebright center fading outward
    float burstRadius = uTime * uRadius * 1.5;
    float burstFront = 1.0 - smoothstep(0.0, 0.12, abs(dist - burstRadius));
    float coreFlash = pow(saturate(1.0 - dist / (burstRadius * 0.4 + 0.01)), 3.0);

    // Jagged explosion edges
    float jagNoise = noise(float2(angle * 4.0, dist * 10.0 - uTime * 8.0));
    float jagged = smoothstep(0.35, 0.65, jagNoise);

    // Shrapnel debris
    float shrapnel = noise(uv * 25.0 + float2(uTime * 3.0, -uTime * 2.0));
    shrapnel = pow(saturate(shrapnel), 5.0) * burstFront;

    // Funeral explosion palette  Edark crimson flash with hot core
    float3 darkCrimson = float3(0.6, 0.05, 0.03);
    float3 flashWhite = float3(1.0, 0.85, 0.7);
    float3 embersOrange = float3(0.95, 0.4, 0.05);

    float3 flashColor = lerp(darkCrimson, embersOrange, burstFront * jagged);
    flashColor = lerp(flashColor, flashWhite, coreFlash);

    // Rapid fadeout
    float fadeOut = saturate(1.0 - uTime * 0.7);
    float totalStrength = saturate(burstFront + coreFlash * 0.8 + shrapnel * 0.3);
    totalStrength *= fadeOut * uIntensity;

    float3 result = lerp(base.rgb, flashColor, totalStrength * 0.9);
    result += flashWhite * coreFlash * fadeOut * 0.3;

    return float4(saturate(result), base.a);
}

technique Technique1
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 PS_BlitzerExplosionFlash();
    }
}
