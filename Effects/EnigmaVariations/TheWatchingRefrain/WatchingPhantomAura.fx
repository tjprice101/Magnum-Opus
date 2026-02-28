// ============================================================================
// WatchingPhantomAura.fx — TheWatchingRefrain phantom aura trail
// UNIQUE SIGNATURE: Watching eyes — procedural iris/pupil patterns that
// form and dissolve within the phantom's trail. Each eye tracks slightly,
// with dark pupil centers surrounded by green iris glow. The phantom is
// literally watching through its own trail. Eldritch, unnerving.
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

// Hash for eye placement
float hash1(float2 p)
{
    return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
}

float2 hash2(float2 p)
{
    p = float2(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)));
    return frac(sin(p) * 43758.5453);
}

float4 PS_Ghost(VertexOutput input) : COLOR0
{
    float2 coords = input.TexCoord.xy;
    float widthFactor = input.TexCoord.z;
    coords.y = (coords.y - 0.5) / max(widthFactor, 0.001) + 0.5;

    // Grid of potential eye locations
    float eyeGridX = 4.0;
    float eyeGridY = 3.0;
    float2 eyeUV = float2(coords.x * eyeGridX - uTime * 0.3, coords.y * eyeGridY);
    float2 eyeCell = floor(eyeUV);
    float2 eyeFrac = frac(eyeUV) - 0.5; // centered in cell

    // Per-cell random: does this cell have an eye?
    float eyePresence = hash1(eyeCell);
    float hasEye = step(0.55 - uIntensity * 0.15, eyePresence); // more intensity = more eyes

    // Eye center with slight random offset
    float2 eyeOffset = (hash2(eyeCell) - 0.5) * 0.3;
    float2 eyePos = eyeFrac - eyeOffset;

    // Horizontal ellipse for eye shape
    float2 eyeScale = float2(1.0, 1.8); // wider than tall
    float eyeDist = length(eyePos * eyeScale);

    // Eye components
    float irisRadius = 0.3;
    float pupilRadius = 0.12 + 0.04 * sin(uTime * 2.0 + eyePresence * 10.0); // pulsing pupil
    float eyeOutline = smoothstep(0.38, 0.32, eyeDist); // eye shape
    float iris = smoothstep(irisRadius + 0.04, irisRadius - 0.04, eyeDist);
    float pupil = smoothstep(pupilRadius + 0.03, pupilRadius - 0.01, eyeDist);

    // Eye blink cycle (per-eye timing)
    float blinkCycle = sin(uTime * 1.5 + eyePresence * 20.0);
    float blink = smoothstep(-0.7, -0.5, blinkCycle); // mostly open, brief blinks
    iris *= blink * hasEye;
    pupil *= blink * hasEye;
    eyeOutline *= blink * hasEye;

    // Colors
    // Iris: eerie green glow
    float3 irisColor = uSecondaryColor * 1.5 * iris * (1.0 - pupil);
    // Add slight iris ring detail
    float irisRing = smoothstep(0.15, 0.2, eyeDist) * iris;
    irisColor += uSecondaryColor * 0.5 * irisRing;

    // Pupil: near-black void
    float3 pupilColor = float3(0.02, 0.005, 0.03) * pupil;

    // Eye outline: faint purple
    float3 outlineColor = uColor * 0.4 * (eyeOutline - iris);

    // Base phantom texture (between eyes)
    float edgeDist = abs(coords.y - 0.5) * 2.0;
    float edgeFade = 1.0 - smoothstep(0.5, 1.0, edgeDist);

    // Phase-shifting noise for ghostly base
    float2 noiseUV1 = float2(coords.x * 2.0 - uTime * 0.5, coords.y * 2.0 + uTime * 0.2);
    float noise = tex2D(uImage1, noiseUV1).r;
    float ghostBase = noise * 0.3 * edgeFade * (1.0 - eyeOutline * 0.8);

    // Ghostly flicker
    float flicker = sin(uTime * 6.0 + coords.x * 4.0) * 0.1 + 0.9;

    float3 baseColor = uColor * ghostBase * flicker;
    float3 eyeColor = irisColor + outlineColor;
    eyeColor = lerp(eyeColor, pupilColor, pupil * 0.9);

    float3 finalColor = baseColor + eyeColor;

    // Alpha: ghost base + eye adds
    float ghostAlpha = ghostBase * 0.6;
    float eyeAlpha = eyeOutline * 0.8;
    float finalAlpha = saturate(ghostAlpha + eyeAlpha) * edgeFade * uOpacity * flicker * input.Color.a;

    return float4(finalColor, finalAlpha);
}

float4 PS_Glow(VertexOutput input) : COLOR0
{
    float2 coords = input.TexCoord.xy;
    float widthFactor = input.TexCoord.z;
    coords.y = (coords.y - 0.5) / max(widthFactor, 0.001) + 0.5;

    float edgeDist = abs(coords.y - 0.5) * 2.0;
    float glow = exp(-edgeDist * edgeDist * 2.5);

    // Eerie green-purple alternation
    float shift = sin(uTime * 2.0 + coords.x * 3.0) * 0.5 + 0.5;
    float3 glowColor = lerp(uColor * 0.5, uSecondaryColor * 0.4, shift);

    float pulse = sin(uTime * 3.0) * 0.1 + 0.9;
    float glowAlpha = glow * uOpacity * 0.3 * uIntensity * pulse * input.Color.a;

    return float4(glowColor, glowAlpha);
}

technique WatchingPhantomGhost
{
    pass P0
    {
        VertexShader = compile vs_3_0 VS_Main();
        PixelShader = compile ps_3_0 PS_Ghost();
    }
}

technique WatchingPhantomGlow
{
    pass P0
    {
        VertexShader = compile vs_3_0 VS_Main();
        PixelShader = compile ps_3_0 PS_Glow();
    }
}
