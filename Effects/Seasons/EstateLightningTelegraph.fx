// L'Estate - Lightning Bolt Telegraph Renderer
// Draws jagged bolt shapes on a sprite quad for attack warnings
// Natural flicker creates lightning stutter effect

sampler uImage0 : register(s0);
float uTime;
float uIntensity;
float4 uCoreColor;
float4 uGlowColor;
float uFlickerRate;

float hash11(float p)
{
    p = frac(p * 0.1031);
    p *= p + 33.33;
    p *= p + p;
    return frac(p);
}

float4 LightningPS(float2 coords : TEXCOORD0) : COLOR0
{
    float segments = 20.0;
    float segY = floor(coords.y * segments);
    float segFrac = frac(coords.y * segments);

    // Zigzag center line via hash at each segment boundary
    float cx0 = 0.5 + (hash11(segY * 7.13 + 0.5) - 0.5) * 0.4;
    float cx1 = 0.5 + (hash11((segY + 1.0) * 7.13 + 0.5) - 0.5) * 0.4;
    float centerX = lerp(cx0, cx1, smoothstep(0.0, 1.0, segFrac));

    float dist = abs(coords.x - centerX);

    // Core and glow widths
    float coreWidth = 0.025;
    float glowWidth = 0.10;
    float core = smoothstep(coreWidth, 0.0, dist);
    float glow = smoothstep(glowWidth, coreWidth, dist);

    // Natural lightning flicker: rapid on/off stutter
    float flicker1 = step(0.3, frac(uTime * uFlickerRate + hash11(segY * 3.7)));
    float flicker2 = step(0.15, frac(uTime * uFlickerRate * 1.7 + 0.5));
    float flickerMask = max(flicker1, flicker2 * 0.6);

    // Branch tendril (thinner, offset to one side)
    float branchSeg = floor(coords.y * segments * 0.5);
    float branchActive = step(0.6, hash11(branchSeg * 13.37));
    float branchOffset = (hash11(branchSeg * 5.1) - 0.3) * 0.25;
    float branchDist = abs(coords.x - (centerX + branchOffset));
    float branch = smoothstep(0.015, 0.0, branchDist) * branchActive * 0.5;

    // Vertical edge fadeout
    float yFade = smoothstep(0.0, 0.05, coords.y) * smoothstep(1.0, 0.95, coords.y);

    float4 color = uCoreColor * core + uGlowColor * glow + uGlowColor * branch;
    color *= flickerMask * uIntensity * yFade;
    color.a = saturate(core + glow * 0.5 + branch * 0.3) * flickerMask * yFade * uIntensity;

    return color;
}

technique LightningTelegraphTechnique
{
    pass P0
    {
        PixelShader = compile ps_3_0 LightningPS();
    }
}
