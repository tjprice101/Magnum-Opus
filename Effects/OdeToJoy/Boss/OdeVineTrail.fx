// ══════════════════════════════════════════════════════════╁E
// OdeVineTrail.fx  EOde to Joy boss movement trail
// Growing vine trail with curling tendrils and small leaf
// bursts, warm green with golden light threading through.
// ══════════════════════════════════════════════════════════╁E

sampler uImage0 : register(s0);
sampler uNoiseTex : register(s1);
float4 uColor;
float uTrailWidth;
float uFadeRate;
float uTime;

float4 PS_VineTrail(float2 uv : TEXCOORD0) : COLOR0
{
    float trailProgress = uv.x;
    float trailWidth = abs(uv.y - 0.5) * 2.0;

    // Vine core  Esinusoidal center line with noise wobble
    float2 vineUV = float2(uv.x * 3.0 - uTime * 1.0, uv.y);
    float vineWobble = tex2D(uNoiseTex, vineUV * float2(1, 0.5)).r * 0.15;
    float vineCenter = abs(uv.y - 0.5 + vineWobble - 0.075);
    float vineMask = smoothstep(0.06, 0.02, vineCenter);

    // Tendril curls branching outward
    float2 tendrilUV = float2(uv.x * 6.0 - uTime * 2.0, uv.y * 4.0);
    float tendrilNoise = tex2D(uNoiseTex, tendrilUV).r;
    float tendrils = smoothstep(0.55, 0.7, tendrilNoise) * smoothstep(1.0, 0.3, trailWidth);

    // Leaf burst spots
    float2 leafUV = float2(uv.x * 10.0 - uTime * 1.5, uv.y * 5.0);
    float leafNoise = tex2D(uNoiseTex, leafUV).r;
    float leaves = smoothstep(0.8, 0.9, leafNoise) * smoothstep(0.8, 0.2, trailWidth);

    // Golden light threading along the vine
    float2 lightUV = float2(uv.x * 2.0 - uTime * 3.0, 0.5);
    float lightPulse = tex2D(uNoiseTex, lightUV).r;
    float goldenLight = smoothstep(0.5, 0.8, lightPulse) * vineMask;

    // Age fade
    float ageFade = pow(1.0 - trailProgress, uFadeRate * 2.0);

    // Colors: deep green vine, lighter green tendrils, gold light
    float4 vineGreen = uColor;
    float4 leafGreen = float4(0.3, 0.7, 0.2, 1.0);
    float4 goldLight = float4(1.0, 0.85, 0.3, 1.0);

    float4 color = vineGreen * (vineMask + tendrils * 0.7);
    color += leafGreen * leaves * 0.8;
    color += goldLight * goldenLight * 0.6;

    float alpha = (vineMask + tendrils * 0.5 + leaves * 0.4 + goldenLight * 0.3) * ageFade * uTrailWidth;

    return color * saturate(alpha);
}

technique Technique1
{
    pass VineTrail
    {
        PixelShader = compile ps_3_0 PS_VineTrail();
    }
}
