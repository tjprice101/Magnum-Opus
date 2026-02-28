// ============================================================================
// SilentSeekerTrail.fx — TheSilentMeasure seeker trail
// UNIQUE SIGNATURE: Phantom echo trail — multiple time-offset ghost copies
// that fan out in V-formation behind the seeker. Each echo progressively
// fades and blurs, creating a visible silence — the trail of something
// that was never fully there. NOT dots — afterimage fan.
// ============================================================================

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uTime;
float uIntensity;
matrix uWorldViewProjection;

struct VertexInput
{
    float2 Position : POSITION0;
    float4 Color : COLOR0;
    float3 TexCoord : TEXCOORD0;
};

struct VertexOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float3 TexCoord : TEXCOORD0;
};

VertexOutput VS_Main(VertexInput input)
{
    VertexOutput output;
    output.Position = mul(float4(input.Position, 0, 1), uWorldViewProjection);
    output.Color = input.Color;
    output.TexCoord = input.TexCoord;
    return output;
}

float4 PS_SeekerFlow(VertexOutput input) : COLOR0
{
    float2 coords = input.TexCoord.xy;
    float widthFactor = input.TexCoord.z;
    coords.y = (coords.y - 0.5) / max(widthFactor, 0.001) + 0.5;

    // Core trail — the primary seeker path
    float edgeDist = abs(coords.y - 0.5) * 2.0;
    float coreTrail = exp(-edgeDist * edgeDist * 8.0);

    // Ghost echoes — 4 progressively fading copies offset in Y
    float echoSpread = 0.06 * uIntensity; // How far echoes spread vertically
    float echo1 = exp(-pow((coords.y - 0.5 + echoSpread * 1.0) * 4.0, 2.0)) * 0.6;
    float echo2 = exp(-pow((coords.y - 0.5 - echoSpread * 1.0) * 4.0, 2.0)) * 0.6;
    float echo3 = exp(-pow((coords.y - 0.5 + echoSpread * 2.5) * 3.0, 2.0)) * 0.3;
    float echo4 = exp(-pow((coords.y - 0.5 - echoSpread * 2.5) * 3.0, 2.0)) * 0.3;

    // Echoes appear more toward the tail (older trail = more spread)
    float tailFactor = smoothstep(0.0, 0.5, 1.0 - coords.x);
    echo1 *= tailFactor;
    echo2 *= tailFactor;
    echo3 *= tailFactor * 0.7;
    echo4 *= tailFactor * 0.7;

    // Flicker each echo independently for spectral feel
    float flick1 = sin(uTime * 5.0 + coords.x * 8.0) * 0.3 + 0.7;
    float flick2 = sin(uTime * 6.0 + coords.x * 10.0 + 1.5) * 0.3 + 0.7;
    float flick3 = sin(uTime * 4.0 + coords.x * 6.0 + 3.0) * 0.4 + 0.6;
    float flick4 = sin(uTime * 7.0 + coords.x * 12.0 + 4.5) * 0.4 + 0.6;

    echo1 *= flick1;
    echo2 *= flick2;
    echo3 *= flick3;
    echo4 *= flick4;

    float totalEchoes = echo1 + echo2 + echo3 + echo4;

    // Silence pulse — periodic dimming (the "silent" in Silent Measure)
    float silencePulse = sin(coords.x * 10.0 - uTime * 4.0);
    float silence = smoothstep(-0.2, 0.2, silencePulse);
    coreTrail *= lerp(0.4, 1.0, silence);

    // Noise for subtle texture
    float2 noiseUV = float2(coords.x * 3.0 - uTime * 0.5, coords.y * 2.0);
    float noise = tex2D(uImage1, noiseUV).r;

    // Core trail → violet. Echoes → green (fading)
    float3 coreColor = uColor * coreTrail * (0.8 + noise * 0.2);
    float3 echoColor = uSecondaryColor * totalEchoes * 0.8;
    // Outermost echoes shift toward pale white-green
    float3 outerEcho = lerp(uSecondaryColor, float3(0.7, 0.9, 0.8), 0.4) * (echo3 + echo4) * 0.5;

    float3 finalColor = coreColor + echoColor + outerEcho;
    finalColor *= uIntensity;

    // Edge fade — the fan width is controlled by echoes
    float fanWidth = 0.5 + totalEchoes * 0.5;
    float edgeFade = 1.0 - smoothstep(fanWidth * 0.5, fanWidth, edgeDist);

    float combinedAlpha = coreTrail + totalEchoes * 0.6;
    float finalAlpha = edgeFade * combinedAlpha * uOpacity * input.Color.a;

    return float4(finalColor, saturate(finalAlpha));
}

technique SilentSeekerFlow
{
    pass P0
    {
        VertexShader = compile vs_3_0 VS_Main();
        PixelShader = compile ps_3_0 PS_SeekerFlow();
    }
}
