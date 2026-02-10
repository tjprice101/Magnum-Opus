// =============================================================================
// MagnumOpus Advanced Trail Shader - 5 Unique Trail Styles
// =============================================================================
// Style 1: FLAME - Fire trail with ember particles and heat shimmer
// Style 2: ICE - Crystalline frozen trail with frost particles
// Style 3: LIGHTNING - Electric crackling trail with energy arcs
// Style 4: NATURE - Organic vine/petal trail with growth animation
// Style 5: COSMIC - Starfield/nebula trail with constellation patterns
// =============================================================================

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float3 uColor;
float3 uSecondaryColor;
float3 uTertiaryColor;
float uOpacity;
float uTime;
float uIntensity;
float uTrailLength;
float uStyleParam1;
float uStyleParam2;

// =============================================================================
// UTILITY FUNCTIONS
// =============================================================================

float QuadraticBump(float x)
{
    return x * (4.0 - x * 4.0);
}

float InverseLerp(float a, float b, float t)
{
    return saturate((t - a) / (b - a));
}

float Convert01To010(float x)
{
    return x < 0.5 ? x * 2.0 : (1.0 - x) * 2.0;
}

float Hash(float2 p)
{
    return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
}

float Noise2D(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);
    f = f * f * (3.0 - 2.0 * f);
    
    float a = Hash(i);
    float b = Hash(i + float2(1.0, 0.0));
    float c = Hash(i + float2(0.0, 1.0));
    float d = Hash(i + float2(1.0, 1.0));
    
    return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
}

float FBM(float2 p, int octaves)
{
    float value = 0.0;
    float amplitude = 0.5;
    float frequency = 1.0;
    
    for (int i = 0; i < octaves; i++)
    {
        value += amplitude * Noise2D(p * frequency);
        amplitude *= 0.5;
        frequency *= 2.0;
    }
    
    return value;
}

// =============================================================================
// STYLE 1: FLAME TRAIL
// Fire trail with rising ember particles and heat distortion
// =============================================================================

float4 FlameTrail(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseColor = tex2D(uImage0, coords);
    
    // Completion ratio for fade
    float completion = coords.x;
    
    // Width fade using QuadraticBump
    float widthFactor = QuadraticBump(completion);
    
    // Vertical distortion (flames rise)
    float2 distortedCoords = coords;
    distortedCoords.y += (completion - 0.5) * sin(uTime * 8.0 + coords.x * 20.0) * 0.1;
    distortedCoords.x -= uTime * 0.3; // Flames flow backward
    
    // Fire noise layers
    float noise1 = Noise2D(distortedCoords * 15.0);
    float noise2 = Noise2D(distortedCoords * 30.0 - uTime * 2.0);
    float fireNoise = noise1 * 0.6 + noise2 * 0.4;
    
    // Ember particles
    float embers = 0.0;
    for (int i = 0; i < 4; i++)
    {
        float2 emberCoord = coords;
        emberCoord.x -= uTime * (0.5 + i * 0.2);
        emberCoord.y += sin(coords.x * 30.0 + i * 2.0 + uTime * 3.0) * 0.1;
        float ember = Noise2D(emberCoord * 40.0 + i * 100.0);
        ember = smoothstep(0.8, 0.95, ember);
        embers += ember;
    }
    embers = saturate(embers);
    
    // Fire color gradient (dark red -> orange -> yellow -> white at core)
    float3 fireColor;
    float heat = saturate(widthFactor * (1.0 - completion) * 2.0);
    
    if (heat < 0.25)
        fireColor = lerp(float3(0.2, 0.0, 0.0), float3(0.8, 0.1, 0.0), heat * 4.0);
    else if (heat < 0.5)
        fireColor = lerp(float3(0.8, 0.1, 0.0), float3(1.0, 0.5, 0.0), (heat - 0.25) * 4.0);
    else if (heat < 0.75)
        fireColor = lerp(float3(1.0, 0.5, 0.0), float3(1.0, 0.9, 0.3), (heat - 0.5) * 4.0);
    else
        fireColor = lerp(float3(1.0, 0.9, 0.3), float3(1.0, 1.0, 0.9), (heat - 0.75) * 4.0);
    
    fireColor *= uColor;
    
    // Apply noise to edges
    float edgeFade = abs(coords.y - 0.5) * 2.0;
    float noisyEdge = smoothstep(1.0, 0.5 - fireNoise * 0.3, edgeFade);
    
    // Flickering
    float flicker = sin(uTime * 20.0 + coords.x * 50.0) * 0.1 + 0.9;
    
    float3 finalColor = fireColor * widthFactor * noisyEdge * flicker * uIntensity;
    finalColor += float3(1.0, 0.5, 0.1) * embers * 2.0; // Bright embers
    finalColor *= baseColor.rgb;
    
    float opacity = widthFactor * noisyEdge * uOpacity * sampleColor.a;
    
    return float4(finalColor, opacity);
}

// =============================================================================
// STYLE 2: ICE TRAIL
// Crystalline frozen trail with frost particles and sharp edges
// =============================================================================

float4 IceTrail(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseColor = tex2D(uImage0, coords);
    
    float completion = coords.x;
    float widthFactor = QuadraticBump(completion);
    
    // Crystalline pattern (sharp geometric shapes)
    float2 crystalCoords = coords * float2(20.0, 10.0);
    float2 cellId = floor(crystalCoords);
    float2 cellUV = frac(crystalCoords);
    
    // Hexagonal-ish crystal pattern
    float crystal = 0.0;
    float2 centers[3] = { float2(0.25, 0.25), float2(0.75, 0.25), float2(0.5, 0.75) };
    for (int i = 0; i < 3; i++)
    {
        float dist = length(cellUV - centers[i]);
        float edge = smoothstep(0.35, 0.3, dist);
        float innerEdge = smoothstep(0.2, 0.25, dist);
        crystal += edge * innerEdge * (Hash(cellId + i) * 0.5 + 0.5);
    }
    crystal = saturate(crystal);
    
    // Frost particles (small bright spots)
    float frost = 0.0;
    for (int i = 0; i < 3; i++)
    {
        float frostNoise = Noise2D(coords * (50.0 + i * 20.0) + uTime * 0.1);
        frost += smoothstep(0.85, 0.95, frostNoise);
    }
    frost = saturate(frost);
    
    // Shimmer effect
    float shimmer = sin(coords.x * 100.0 - uTime * 2.0 + coords.y * 30.0);
    shimmer = smoothstep(0.8, 1.0, shimmer) * 0.3;
    
    // Ice colors (light blue core, white edges, dark blue accents)
    float3 coreColor = uColor;
    float3 edgeColor = float3(0.9, 0.95, 1.0);
    float3 accentColor = uSecondaryColor;
    
    float edgeDist = abs(coords.y - 0.5) * 2.0;
    float3 iceColor = lerp(coreColor, edgeColor, edgeDist * 0.5);
    iceColor += accentColor * crystal * 0.3;
    
    // Sharp edge fade
    float edgeFade = smoothstep(1.0, 0.7, edgeDist);
    
    float3 finalColor = iceColor * widthFactor * uIntensity;
    finalColor += float3(0.8, 0.9, 1.0) * frost * 1.5;
    finalColor += float3(1.0, 1.0, 1.0) * shimmer;
    finalColor *= baseColor.rgb;
    
    float opacity = widthFactor * edgeFade * uOpacity * sampleColor.a;
    
    return float4(finalColor, opacity);
}

// =============================================================================
// STYLE 3: LIGHTNING TRAIL
// Electric crackling trail with energy arcs and bright core
// =============================================================================

float4 LightningTrail(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseColor = tex2D(uImage0, coords);
    
    float completion = coords.x;
    float widthFactor = QuadraticBump(completion);
    
    // Main lightning bolt (jagged center line)
    float boltY = 0.5;
    float boltOffset = 0.0;
    
    // Create jagged path using layered noise
    for (int i = 0; i < 4; i++)
    {
        float freq = 20.0 + i * 15.0;
        float amp = 0.08 / (i + 1);
        boltOffset += sin(coords.x * freq + uTime * (5.0 + i * 2.0) + i * 5.0) * amp;
    }
    
    float distToBolt = abs(coords.y - (boltY + boltOffset));
    
    // Sharp lightning core
    float core = smoothstep(0.03, 0.0, distToBolt);
    
    // Electric glow around core
    float glow = smoothstep(0.15, 0.0, distToBolt);
    
    // Secondary arcs branching off
    float arcs = 0.0;
    for (int i = 0; i < 3; i++)
    {
        float arcX = frac(coords.x * 3.0 + i * 0.33 + uTime * 0.5);
        float arcY = 0.5 + boltOffset + sin(arcX * 20.0 + i * 2.0) * 0.15 * (arcX);
        float arcDist = length(float2(arcX - 0.5, coords.y - arcY) * float2(0.3, 1.0));
        arcs += smoothstep(0.08, 0.02, arcDist) * step(0.3, arcX);
    }
    arcs = saturate(arcs);
    
    // Crackling energy particles
    float crackle = Noise2D(coords * 100.0 + uTime * 10.0);
    crackle = smoothstep(0.85, 1.0, crackle) * glow * 2.0;
    
    // Lightning colors (white core, colored glow)
    float3 coreColor = float3(1.0, 1.0, 1.0);
    float3 glowColor = uColor;
    float3 arcColor = uSecondaryColor;
    
    float3 finalColor = coreColor * core * 2.0;
    finalColor += glowColor * glow * 0.8;
    finalColor += arcColor * arcs * 0.6;
    finalColor += glowColor * crackle;
    
    finalColor *= widthFactor * uIntensity * baseColor.rgb;
    
    // Flicker effect
    float flicker = sin(uTime * 30.0) * sin(uTime * 47.0) * 0.2 + 0.8;
    finalColor *= flicker;
    
    float opacity = (core + glow * 0.5 + arcs * 0.3) * widthFactor * uOpacity * sampleColor.a;
    
    return float4(finalColor, opacity);
}

// =============================================================================
// STYLE 4: NATURE TRAIL
// Organic vine/petal trail with growth animation
// =============================================================================

float4 NatureTrail(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseColor = tex2D(uImage0, coords);
    
    float completion = coords.x;
    float widthFactor = QuadraticBump(completion);
    
    // Main vine (wavy organic line)
    float vineY = 0.5 + sin(coords.x * 15.0 + uTime) * 0.1;
    vineY += sin(coords.x * 8.0 - uTime * 0.5) * 0.05;
    float distToVine = abs(coords.y - vineY);
    
    // Vine thickness varies
    float vineThickness = 0.03 + sin(coords.x * 20.0) * 0.01;
    float vine = smoothstep(vineThickness, vineThickness * 0.5, distToVine);
    
    // Leaves/petals growing from vine
    float leaves = 0.0;
    for (int i = 0; i < 5; i++)
    {
        float leafX = frac(coords.x * 5.0 + i * 0.2);
        float leafSide = (i % 2) * 2.0 - 1.0; // Alternate sides
        float leafY = vineY + leafSide * 0.1 * sin(leafX * 3.14159);
        
        // Leaf shape (ellipse)
        float2 leafCenter = float2(0.5, leafY);
        float2 toLeaf = coords - float2(frac(coords.x * 5.0 + i * 0.2) * 0.2 + 0.4, leafY);
        toLeaf.x *= 2.0; // Elongate
        float leafDist = length(toLeaf);
        
        // Growth animation
        float growthPhase = frac(uTime * 0.3 + i * 0.2);
        float leafSize = 0.08 * smoothstep(0.0, 0.3, growthPhase);
        
        leaves += smoothstep(leafSize, leafSize * 0.7, leafDist) * smoothstep(1.0, 0.7, completion);
    }
    leaves = saturate(leaves);
    
    // Floating pollen/spores
    float pollen = 0.0;
    for (int i = 0; i < 4; i++)
    {
        float2 pollenCoord = coords;
        pollenCoord.x -= uTime * 0.1 * (i + 1);
        pollenCoord.y += sin(coords.x * 10.0 + i * 2.0 + uTime) * 0.1;
        float p = Noise2D(pollenCoord * 50.0 + i * 10.0);
        pollen += smoothstep(0.88, 0.95, p);
    }
    pollen = saturate(pollen) * widthFactor;
    
    // Nature colors
    float3 vineColor = uColor * 0.4; // Dark green
    float3 leafColor = uColor; // Bright green
    float3 pollenColor = uSecondaryColor; // Yellow/pink
    
    float3 finalColor = vineColor * vine;
    finalColor += leafColor * leaves * 0.8;
    finalColor += pollenColor * pollen * 1.5;
    
    finalColor *= widthFactor * uIntensity * baseColor.rgb;
    
    // Gentle sway in opacity
    float sway = sin(coords.x * 5.0 + uTime * 2.0) * 0.1 + 0.9;
    
    float opacity = (vine + leaves * 0.7 + pollen * 0.5) * widthFactor * uOpacity * sampleColor.a * sway;
    
    return float4(finalColor, opacity);
}

// =============================================================================
// STYLE 5: COSMIC TRAIL
// Starfield/nebula trail with constellation patterns
// =============================================================================

float4 CosmicTrail(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseColor = tex2D(uImage0, coords);
    
    float completion = coords.x;
    float widthFactor = QuadraticBump(completion);
    
    // Nebula cloud (FBM noise)
    float2 nebulaCoords = coords * float2(5.0, 3.0) - uTime * 0.1;
    float nebula = FBM(nebulaCoords, 4);
    nebula = smoothstep(0.3, 0.7, nebula);
    
    // Stars (sharp bright points)
    float stars = 0.0;
    for (int i = 0; i < 3; i++)
    {
        float2 starCoord = coords * (30.0 + i * 20.0);
        float2 starId = floor(starCoord);
        float2 starUV = frac(starCoord);
        
        // Random star position within cell
        float2 starPos = float2(Hash(starId), Hash(starId + 100.0));
        float starDist = length(starUV - starPos);
        
        // Twinkle
        float twinkle = sin(uTime * 5.0 + Hash(starId) * 10.0) * 0.3 + 0.7;
        
        // Star brightness based on random
        float brightness = Hash(starId + 200.0);
        stars += smoothstep(0.05, 0.0, starDist) * brightness * twinkle * step(0.7, brightness);
    }
    stars = saturate(stars);
    
    // Constellation lines (connect nearby bright stars)
    float constellations = 0.0;
    float2 constCoord = coords * 15.0;
    float2 constId = floor(constCoord);
    
    // Simple line pattern
    float lineX = abs(frac(constCoord.x) - 0.5);
    float lineY = abs(frac(constCoord.y) - 0.5);
    float crossLine = min(lineX, lineY);
    constellations = smoothstep(0.02, 0.0, crossLine) * Hash(constId) * 0.3;
    
    // Cosmic colors (deep purples, blues, with pink/cyan accents)
    float3 nebulaColor1 = uColor;
    float3 nebulaColor2 = uSecondaryColor;
    float3 starColor = float3(1.0, 1.0, 1.0);
    
    // Nebula gradient
    float3 nebulaFinal = lerp(nebulaColor1, nebulaColor2, nebula);
    
    float3 finalColor = nebulaFinal * nebula * 0.6;
    finalColor += starColor * stars * 2.0;
    finalColor += uTertiaryColor * constellations * 0.5;
    
    // Edge glow (galaxy edge effect)
    float edgeDist = abs(coords.y - 0.5) * 2.0;
    float edgeGlow = smoothstep(1.0, 0.6, edgeDist) * smoothstep(0.0, 0.4, edgeDist);
    finalColor += uSecondaryColor * edgeGlow * 0.3;
    
    finalColor *= widthFactor * uIntensity * baseColor.rgb;
    
    float opacity = (nebula * 0.5 + stars + edgeGlow * 0.5 + constellations) * widthFactor * uOpacity * sampleColor.a;
    
    return float4(finalColor, opacity);
}

// =============================================================================
// TECHNIQUES
// =============================================================================

technique FlameTechnique
{
    pass FlamePass
    {
        PixelShader = compile ps_2_0 FlameTrail();
    }
}

technique IceTechnique
{
    pass IcePass
    {
        PixelShader = compile ps_2_0 IceTrail();
    }
}

technique LightningTechnique
{
    pass LightningPass
    {
        PixelShader = compile ps_2_0 LightningTrail();
    }
}

technique NatureTechnique
{
    pass NaturePass
    {
        PixelShader = compile ps_2_0 NatureTrail();
    }
}

technique CosmicTechnique
{
    pass CosmicPass
    {
        PixelShader = compile ps_2_0 CosmicTrail();
    }
}
