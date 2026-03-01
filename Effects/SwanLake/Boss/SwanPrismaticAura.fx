// SwanPrismaticAura.fx  ESwan Lake boss presence aura
// A graceful prismatic rainbow shimmer that shifts between 
// monochrome (black/white) and full rainbow based on mood state
sampler uImage0 : register(s0);
float2 uCenter;
float uRadius;
float uIntensity;
float4 uPrimaryColor;   // White
float4 uSecondaryColor;  // Black
float uTime;

float3 hsl2rgb(float h, float s, float l)
{
    float3 rgb = clamp(abs(fmod(h * 6.0 + float3(0, 4, 2), 6.0) - 3.0) - 1.0, 0.0, 1.0);
    return l + s * (rgb - 0.5) * (1.0 - abs(2.0 * l - 1.0));
}

float4 PS_PrismaticAura(float2 uv : TEXCOORD0) : COLOR0
{
    float2 centered = uv - 0.5;
    float dist = length(centered);
    float angle = atan2(centered.y, centered.x);
    float radiusNorm = uRadius / 200.0;
    float falloff = 1.0 - smoothstep(0.0, radiusNorm, dist);
    // Prismatic rainbow ring
    float hue = frac(angle / 6.283 + uTime * 0.1);
    float3 rainbow = hsl2rgb(hue, 0.8, 0.6);
    // Elegant wave pattern (ballet-like flowing arcs)
    float wave = sin(angle * 4.0 + dist * 20.0 - uTime * 3.0) * 0.5 + 0.5;
    float wave2 = sin(angle * 6.0 - dist * 15.0 + uTime * 2.0) * 0.5 + 0.5;
    float pattern = wave * 0.6 + wave2 * 0.4;
    // Feather streak accents
    float feather = sin(angle * 12.0 + uTime * 4.0);
    feather = smoothstep(0.7, 1.0, feather) * falloff;
    // Monochrome base with rainbow accents
    float3 mono = lerp(uSecondaryColor.rgb, uPrimaryColor.rgb, pattern);
    float3 color = lerp(mono, rainbow, 0.3 + feather * 0.4);
    float alpha = falloff * pattern * uIntensity;
    alpha += feather * 0.3;
    return float4(color, 1) * saturate(alpha);
}

technique Technique1
{
    pass PrismaticAura { PixelShader = compile ps_3_0 PS_PrismaticAura(); }
}
