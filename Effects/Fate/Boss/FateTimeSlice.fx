// ══════════════════════════════════════════════════════════╁E
// FateTimeSlice.fx  EFate boss time-slice attack visual
// Reality splitting with chromatic aberration, dark void
// bleeding through fractured space with crimson/pink edges.
// ══════════════════════════════════════════════════════════╁E

sampler uImage0 : register(s0);
sampler uNoiseTex : register(s1);
float4 uColor;
float uIntensity;
float uTime;

float hash(float2 p)
{
    return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
}

float4 PS_TimeSlice(float2 uv : TEXCOORD0) : COLOR0
{
    float2 centered = uv - 0.5;

    // Diagonal slice line  Ereality crack
    float sliceAngle = uTime * 0.5;
    float2 sliceDir = float2(cos(sliceAngle), sin(sliceAngle));
    float sliceDist = abs(dot(centered, sliceDir));
    float sliceMask = smoothstep(0.03, 0.0, sliceDist);

    // Chromatic aberration offset  ERGB channels split along slice
    float aberration = uIntensity * 0.03;
    float2 offset = sliceDir * aberration;
    float r = tex2D(uImage0, uv + offset).r;
    float g = tex2D(uImage0, uv).g;
    float b = tex2D(uImage0, uv - offset).b;
    float a = tex2D(uImage0, uv).a;

    // Void bleeding through the crack
    float voidNoise = tex2D(uNoiseTex, uv * 3.0 + float2(uTime * 0.8, 0)).r;
    float voidBleed = sliceMask * voidNoise;

    // Fracture lines branching from main slice
    float fracture = hash(floor(centered * 20.0 + float2(uTime * 0.3, 0)));
    fracture = step(0.93, fracture) * smoothstep(0.15, 0.0, sliceDist);

    // Colors: chromatic split base, crimson edge glow, void black
    float4 chromatic = float4(r, g, b, a);
    float4 edgeGlow = uColor * sliceMask * uIntensity * 2.0;
    float4 voidDark = float4(0.02, 0.0, 0.03, 1.0) * voidBleed;
    float4 fractureGlow = float4(1.0, 0.4, 0.6, 1.0) * fracture * uIntensity;

    float4 result = chromatic;
    result += edgeGlow;
    result += voidDark * 0.5;
    result += fractureGlow;

    return result;
}

technique Technique1
{
    pass TimeSlice
    {
        PixelShader = compile ps_3_0 PS_TimeSlice();
    }
}
