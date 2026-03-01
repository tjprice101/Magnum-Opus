// ══════════════════════════════════════════════════════════╁E
// OdePetalStorm.fx  EOde to Joy boss attack shader
// Swirling petal storm: golden petals spiral outward from
// center in a radial vortex with warm amber wind currents.
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

float4 PS_PetalStorm(float2 uv : TEXCOORD0) : COLOR0
{
    float2 centered = uv - 0.5;
    float dist = length(centered);
    float angle = atan2(centered.y, centered.x);

    // Spiral vortex  Eangle offset increases with distance
    float spiralAngle = angle + dist * 8.0 - uTime * 3.0;

    // Petal shapes in spiral arms
    float spiralArms = sin(spiralAngle * 3.0) * 0.5 + 0.5;
    spiralArms = pow(spiralArms, 2.0);
    float spiralMask = spiralArms * smoothstep(0.5, 0.05, dist);

    // Individual petal noise  Efloating shapes
    float2 petalUV = float2(angle / 6.28 + uTime * 0.5, dist * 4.0 - uTime * 2.0);
    float petalNoise = tex2D(uNoiseTex, petalUV).r;
    float petals = smoothstep(0.5, 0.8, petalNoise) * smoothstep(0.5, 0.1, dist);

    // Wind current streaks
    float2 windUV = float2(uv.x * 3.0 - uTime * 4.0, uv.y * 2.0 + uTime * 0.5);
    float wind = tex2D(uNoiseTex, windUV).r;
    float windStreaks = smoothstep(0.6, 0.8, wind) * 0.3;

    // Center eye  Ecalm golden glow
    float eye = smoothstep(0.1, 0.0, dist) * 1.5;

    // Colors: warm gold petals, amber wind, bright gold center
    float4 petalGold = uColor;
    float4 amberWind = float4(0.9, 0.6, 0.1, 1.0);
    float4 brightGold = float4(1.0, 0.92, 0.6, 1.0);

    float4 color = petalGold * (spiralMask + petals) * uIntensity;
    color += amberWind * windStreaks * uIntensity;
    color += brightGold * eye * uIntensity;

    return color;
}

technique Technique1
{
    pass PetalStorm
    {
        PixelShader = compile ps_3_0 PS_PetalStorm();
    }
}
