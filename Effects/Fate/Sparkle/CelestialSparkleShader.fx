// CelestialSparkleShader.fx — Fate Theme Sparkle Shader
//
// Transforms star sprites into cosmic, destiny-infused celestial sparkles.
// Visual identity: cosmic inevitability, stellar power, nebula-colored grandeur.
//
// UNIQUE FEATURES (vs other theme sparkles):
// - Nebula color drift: sparkle colors cycle through the full Fate palette
//   (dark pink → bright crimson → star gold → celestial white) based on
//   intensity, creating a cosmic temperature map
// - Constellation connection flash: at peak brightness, thin lines briefly
//   appear connecting to the sparkle's cardinal points (like drawing constellations)
// - Cosmic rotation: slowest rotation of all themes — majestic, inevitable rotation
//   like celestial bodies
// - Stellar nucleosynthesis core: the center has a dense, layered glow
//   that pulses like a star's core fusion
// - Chromatic aberration fringe: slight RGB channel offset at edges for
//   a cosmic, high-energy feel

sampler uImage0 : register(s0);

float uTime;
float flashPhase;
float flashSpeed;
float flashPower;
float baseAlpha;
float shimmerIntensity;
float3 primaryColor;    // BrightCrimson/DarkPink
float3 accentColor;     // StarGold/NebulaPurple
float3 highlightColor;  // WhiteCelestial/SupernovaWhite

float3 hsv2rgb(float3 c)
{
    float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}

float4 CelestialSparklePS(float4 screenspace : TEXCOORD0) : COLOR0
{
    float2 uv = screenspace.xy;
    float4 texColor = tex2D(uImage0, uv);
    if (texColor.a < 0.01) return float4(0, 0, 0, 0);

    float2 centered = uv - 0.5;
    float dist = length(centered);
    float angle = atan2(centered.y, centered.x);

    // ---- COSMIC ROTATION (majestic, slow, inevitable) ----
    float cosmicAngle = angle + uTime * flashSpeed * 0.3 + flashPhase;

    // ---- 4-POINT CELESTIAL STAR ----
    float star4 = sin(cosmicAngle * 4.0) * 0.5 + 0.5;

    // ---- 8-POINT COSMIC RAYS (celestial radiance) ----
    float star8 = sin(cosmicAngle * 8.0 + uTime * flashSpeed * 0.5 + 1.0) * 0.5 + 0.5;

    // ---- NEBULA DRIFT ----
    // Slow, vast color shifting like cosmic gas clouds
    float nebulaDrift = sin(dist * 6.0 + uTime * flashSpeed * 0.4 + angle * 0.5 + flashPhase) * 0.5 + 0.5;

    // ---- CONSTELLATION CONNECTION FLASH ----
    // At peak brightness, lines appear at 4 cardinal angles
    float lineWidth = 0.04;
    float cardinalDist = min(
        min(abs(sin(cosmicAngle)), abs(cos(cosmicAngle))),
        min(abs(sin(cosmicAngle + 0.785)), abs(cos(cosmicAngle + 0.785)))
    );
    float constellation = smoothstep(lineWidth, lineWidth * 0.3, cardinalDist);
    constellation *= smoothstep(0.0, 0.15, dist) * smoothstep(0.5, 0.3, dist);

    // ---- STELLAR NUCLEOSYNTHESIS CORE ----
    // Dense layered glow at center — like a star's fusion core
    float corePulse = 0.7 + 0.3 * sin(uTime * flashSpeed * 1.8 + flashPhase * 2.0);
    float stellarCore = smoothstep(0.2, 0.0, dist) * corePulse;
    float stellarShell = smoothstep(0.35, 0.15, dist) * (1.0 - stellarCore * 0.5);

    // ---- COMBINED SHIMMER ----
    float shimmer = (star4 * 0.35 + star8 * 0.25 + nebulaDrift * 0.25 + constellation * 0.15);
    shimmer = lerp(1.0, shimmer, shimmerIntensity);

    // ---- DESTINY FLASH PEAKS ----
    // Cosmic sparkles have moderate-high power — dramatic but not frantic
    float destinyFlash = pow(saturate(star4 * star8 * 2.0), flashPower);
    float globalFlash = sin(uTime * flashSpeed * 2.2 + flashPhase);
    globalFlash = pow(saturate(globalFlash), flashPower * 0.5);

    // ---- CHROMATIC ABERRATION FRINGE ----
    // At edges, sample the texture at slightly offset UVs for RGB split
    float chromaOffset = 0.008 * smoothstep(0.15, 0.4, dist);
    float2 chromaDir = normalize(centered + 0.001);
    float rChannel = tex2D(uImage0, uv + chromaDir * chromaOffset).r;
    float bChannel = tex2D(uImage0, uv - chromaDir * chromaOffset).b;
    float3 chromaColor = float3(rChannel, texColor.g, bChannel);
    float chromaBlend = smoothstep(0.15, 0.35, dist) * 0.4;

    // ---- NEBULA COLOR RAMP (intensity → color) ----
    // Low intensity = dark pink, Medium = bright crimson, High = star gold → white
    float intensity = shimmer * stellarCore * 0.5 + shimmer * 0.5;
    float3 nebulaColor = lerp(
        primaryColor,
        lerp(accentColor, highlightColor, saturate(intensity * 1.5 - 0.5)),
        saturate(intensity)
    );

    // ---- COSMIC PRISMATIC ----
    float hue = frac(0.85 + (angle + 3.14159) / 6.28318 * 0.15 + uTime * flashSpeed * 0.04);
    float3 cosmicPrism = hsv2rgb(float3(hue, 0.4, 1.0));
    float prismStrength = smoothstep(0.1, 0.32, dist) * smoothstep(0.5, 0.35, dist) * 0.3;

    // ---- RADIAL BLOOM ----
    float bloom = smoothstep(0.5, 0.0, dist);
    float innerBloom = smoothstep(0.2, 0.0, dist);

    // ---- COLOR COMPOSITE ----
    float luminance = dot(texColor.rgb, float3(0.299, 0.587, 0.114));

    // Nebula color-ramped base
    float3 baseLayer = nebulaColor * luminance * shimmer;

    // Chromatic aberration blend at edges
    float3 chromaLayer = (chromaColor - texColor.rgb) * chromaBlend * luminance;

    // Cosmic prismatic edge
    float3 prismLayer = cosmicPrism * prismStrength * luminance * 0.4;

    // Destiny flash
    float3 dazzleLayer = highlightColor * destinyFlash * 1.4;

    // Constellation line flash (only visible at high flash)
    float3 constLayer = highlightColor * constellation * destinyFlash * 0.6;

    // Global flash
    float3 flashLayer = lerp(accentColor, highlightColor, 0.4) * globalFlash * innerBloom * 0.45;

    // Stellar core glow — dense, bright
    float3 stellarGlow = highlightColor * stellarCore * 0.55;
    float3 shellGlow = lerp(primaryColor, accentColor, 0.5) * stellarShell * 0.2;

    float3 finalColor = baseLayer + chromaLayer + prismLayer + dazzleLayer + constLayer
                       + flashLayer + stellarGlow + shellGlow;
    finalColor *= 1.4;

    float finalAlpha = texColor.a * baseAlpha * (shimmer * 0.25 + 0.75);
    return float4(finalColor * finalAlpha, finalAlpha);
}

technique Technique1
{
    pass CelestialSparklePass
    {
        PixelShader = compile ps_3_0 CelestialSparklePS();
    }
}
