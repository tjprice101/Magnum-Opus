// ═══════════════════════════════════════════════════════════
// DiesApocalypseRay.fx — Dies Irae boss beam attack shader
// Apocalyptic beam of divine wrath: blood red core with
// crimson corona, ember debris, and heat distortion edges.
// ═══════════════════════════════════════════════════════════

sampler uImage0 : register(s0);
sampler uNoiseTex : register(s1);
float4 uColor;
float uIntensity;
float uTime;

float4 PS_ApocalypseRay(float2 uv : TEXCOORD0) : COLOR0
{
    float2 centered = uv - float2(0.5, 0.5);
    float distY = abs(centered.y);

    // Beam body — horizontal beam with noise turbulence
    float2 beamUV = float2(uv.x * 2.0 - uTime * 4.0, uv.y * 1.5);
    float beamNoise = tex2D(uNoiseTex, beamUV).r;
    float beamWidth = 0.15 + beamNoise * 0.05;
    float beamCore = smoothstep(beamWidth, 0.0, distY);

    // Corona flare — wider and softer
    float corona = smoothstep(0.3, 0.0, distY) * 0.4;

    // Fire turbulence along beam edges
    float2 turbUV = float2(uv.x * 4.0 - uTime * 6.0, uv.y * 3.0 + uTime);
    float turb = tex2D(uNoiseTex, turbUV).r;
    float fireFringe = smoothstep(0.1, 0.2, distY) * smoothstep(0.35, 0.2, distY) * turb;

    // Ember debris flying outward from beam
    float2 debrisUV = float2(uv.x * 8.0 - uTime * 3.0, uv.y * 6.0 + uTime * 2.0);
    float debris = tex2D(uNoiseTex, debrisUV).r;
    float debrisMask = smoothstep(0.85, 0.95, debris) * smoothstep(0.4, 0.15, distY);

    // Colors: white-hot core, blood red mid, dark crimson outer, orange embers
    float4 whiteHot = float4(1.0, 0.92, 0.85, 1.0);
    float4 bloodRed = uColor;
    float4 darkCrimson = float4(0.4, 0.02, 0.0, 1.0);
    float4 emberOrange = float4(1.0, 0.45, 0.05, 1.0);

    float4 color = whiteHot * pow(beamCore, 2.0) * uIntensity;
    color += bloodRed * beamCore * uIntensity * 0.6;
    color += darkCrimson * corona * uIntensity;
    color += bloodRed * fireFringe * uIntensity * 0.8;
    color += emberOrange * debrisMask * uIntensity;

    return color;
}

technique Technique1
{
    pass ApocalypseRay
    {
        PixelShader = compile ps_3_0 PS_ApocalypseRay();
    }
}
