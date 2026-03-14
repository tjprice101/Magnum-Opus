// L'Estate - Lightning-Infused Zenith Beam
// Vertical beam with jagged electric edges, branch tendrils,
// scrolling energy texture. Used for Phase 2-3 beam attacks.

sampler uImage0 : register(s0);
float uTransitionProgress;
float4 uFromColor;
float4 uToColor;
float uIntensity;
float uTime;

float hash11(float p)
{
    p = frac(p * 0.1031);
    p *= p + 33.33;
    p *= p + p;
    return frac(p);
}

float hash12(float2 p)
{
    float3 p3 = frac(float3(p.xyx) * 0.1031);
    p3 += dot(p3, p3.yzx + 33.33);
    return frac((p3.x + p3.y) * p3.z);
}

float noise2D(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);
    f = f * f * (3.0 - 2.0 * f);
    float a = hash12(i);
    float b = hash12(i + float2(1.0, 0.0));
    float c = hash12(i + float2(0.0, 1.0));
    float d = hash12(i + float2(1.0, 1.0));
    return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
}

float4 PS_ZenithBeam(float2 uv : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, uv);

    // Beam shape with jagged electric edges
    float beamWidth = 0.06 + uTransitionProgress * 0.14;
    float beamDist = abs(uv.x - 0.5);

    // Jagged edge displacement via hash per vertical segment
    float segments = 30.0;
    float segY = floor(uv.y * segments);
    float segFrac = frac(uv.y * segments);
    float jag0 = (hash11(segY * 7.13) - 0.5) * 0.04;
    float jag1 = (hash11((segY + 1.0) * 7.13) - 0.5) * 0.04;
    float jagOffset = lerp(jag0, jag1, smoothstep(0.0, 1.0, segFrac));
    float jaggedDist = abs(uv.x - 0.5 + jagOffset);

    float beamMask = smoothstep(beamWidth, beamWidth * 0.2, jaggedDist);

    // Beam descends as transition progresses
    float beamTop = 1.0 - uTransitionProgress;
    float beamY = smoothstep(beamTop, beamTop + 0.08, 1.0 - uv.y);
    beamMask *= beamY;

    // Scrolling energy patterns inside beam
    float energy = noise2D(float2(uv.x * 15.0, uv.y * 6.0 - uTime * 4.0));
    float energyMask = smoothstep(0.35, 0.65, energy) * beamMask;

    // Branch tendril sparks from beam edges
    float branchSeg = floor(uv.y * segments * 0.5);
    float branchActive = step(0.65, hash11(branchSeg * 13.37 + uTime * 0.5));
    float branchOffset = (hash11(branchSeg * 5.1) - 0.3) * 0.15;
    float branchDist = abs(uv.x - (0.5 + branchOffset));
    float branch = smoothstep(0.01, 0.0, branchDist) * branchActive * beamY * 0.4;

    // Lightning flicker
    float flicker = step(0.25, frac(uTime * 12.0 + hash11(segY * 3.7)));

    // Phase blend
    float phaseMix = smoothstep(0.3, 0.7, uTransitionProgress);

    // Colors: white-hot core, golden beam body
    float4 phaseColor = lerp(uFromColor, uToColor, phaseMix);
    float4 whiteCore = float4(1.0, 0.98, 0.92, 1.0);
    float4 goldenBeam = float4(1.0, 0.85, 0.35, 1.0);

    float beamGrad = saturate(jaggedDist / beamWidth);
    float4 beamColor = lerp(whiteCore, goldenBeam, beamGrad);

    float4 result = base;
    result.rgb = lerp(result.rgb, phaseColor.rgb, phaseMix * uIntensity * 0.3);
    result.rgb += beamColor.rgb * beamMask * uIntensity * flicker;
    result.rgb += goldenBeam.rgb * energyMask * uIntensity * 0.5;
    result.rgb += whiteCore.rgb * branch * uIntensity * flicker;

    return result;
}

technique Technique1
{
    pass ZenithBeam
    {
        PixelShader = compile ps_3_0 PS_ZenithBeam();
    }
}
