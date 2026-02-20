// =============================================================================
// MetaballEdgeShader.fx - ps_2_0 Compatible (FNA/MojoShader)
// =============================================================================
// Downported from SM4.0 source (ShaderSource/MetaballEdgeShader.fx).
// Detects edges of metaball shapes and applies edge color with outer glow.
//
// Simplified for ps_2_0 instruction limits:
//   - Outer glow: 2 rings x 4 directions (8 samples) instead of 4x8 (32)
//   - Edge detection: 4 cardinal neighbors instead of full 3x3 Sobel
//   - Branch-free: uses step()/lerp() instead of dynamic if/else
// =============================================================================

// Metaball render target (auto-bound by SpriteBatch to slot 0)
sampler uImage0 : register(s0);

// Overlay texture (set via GraphicsDevice.Textures[1])
sampler uImage1 : register(s1);

// --- Parameters (set via Effect.Parameters in C#) ---
float2 screenSize;              // Screen dimensions in pixels
float2 layerSize;               // Layer texture dimensions in pixels
float2 layerOffset;             // UV offset for parallax scrolling
float4 edgeColor;               // Color drawn at metaball edges
float2 singleFrameScreenOffset; // Per-frame camera offset compensation
float4 layerColor;              // Per-layer tint color

float edgeThickness;            // Edge detection radius (default: 2.0)
float glowIntensity;            // Outer glow brightness (0-2, default: 0.5)
float glowFalloff;              // Glow fade exponent (1-5, default: 2.0)
float innerGlowIntensity;       // Inner bloom intensity (0-1, default: 0.3)
float time;                     // Animation time
float pulseSpeed;               // Edge pulse speed (0 = disabled)
float pulseIntensity;           // Edge pulse strength


float4 MetaballEdgePS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 pixelSize = 1.0 / screenSize;
    float4 center = tex2D(uImage0, coords);

    // --- Branch-free inside/outside mask ---
    // isOutside = 1.0 when alpha <= 0.009 (transparent / outside metaball)
    float isOutside = step(center.a, 0.009);
    float isInside  = 1.0 - isOutside;

    // =======================================================================
    // OUTER GLOW  (meaningful when outside; masked to zero when inside)
    // =======================================================================
    // Ring 1 — cardinal directions at close range
    float dist1  = edgeThickness * 2.0;
    float2 off1  = pixelSize * dist1;
    float glowAccum = 0;
    glowAccum += tex2D(uImage0, coords + float2( off1.x, 0)).a;
    glowAccum += tex2D(uImage0, coords + float2(-off1.x, 0)).a;
    glowAccum += tex2D(uImage0, coords + float2(0,  off1.y)).a;
    glowAccum += tex2D(uImage0, coords + float2(0, -off1.y)).a;

    // Ring 2 — diagonal directions at farther range
    float dist2  = edgeThickness * 4.0;
    float2 off2  = pixelSize * dist2 * 0.707;
    glowAccum += tex2D(uImage0, coords + float2( off2.x,  off2.y)).a * 0.5;
    glowAccum += tex2D(uImage0, coords + float2(-off2.x,  off2.y)).a * 0.5;
    glowAccum += tex2D(uImage0, coords + float2( off2.x, -off2.y)).a * 0.5;
    glowAccum += tex2D(uImage0, coords + float2(-off2.x, -off2.y)).a * 0.5;

    // Normalize: 4 full-weight + 4 half-weight = 6 effective samples
    glowAccum /= 6.0;

    float glow = pow(glowAccum, max(1.0, glowFalloff)) * glowIntensity;

    // Pulse animation (shared by glow and edge)
    float pulse = 1.0 + sin(time * pulseSpeed) * pulseIntensity;
    glow *= pulse;

    float4 outsideResult = edgeColor * sampleColor * glow;
    outsideResult.a = glow;

    // =======================================================================
    // EDGE DETECTION + COLORING  (meaningful when inside; masked when outside)
    // =======================================================================
    float2 es = pixelSize * edgeThickness;

    float aR = tex2D(uImage0, coords + float2( es.x, 0)).a;
    float aL = tex2D(uImage0, coords + float2(-es.x, 0)).a;
    float aU = tex2D(uImage0, coords + float2(0, -es.y)).a;
    float aD = tex2D(uImage0, coords + float2(0,  es.y)).a;

    // Gradient magnitude (simplified Sobel)
    float gx = aR - aL;
    float gy = aD - aU;
    float edgeStrength = sqrt(gx * gx + gy * gy);

    // Boundary check: count how many neighbors are empty
    float emptyNeighbors = step(aR, 0.009)
                         + step(aL, 0.009)
                         + step(aU, 0.009)
                         + step(aD, 0.009);
    emptyNeighbors *= 0.25;

    float edge = saturate(max(edgeStrength, emptyNeighbors));
    edge = saturate(edge * pulse);

    // Overlay texture with parallax scrolling
    float2 overlayUV = (coords + layerOffset + singleFrameScreenOffset)
                      * screenSize / max(layerSize, float2(1, 1));
    float4 overlay = tex2D(uImage1, overlayUV);

    // Inner glow near edges
    float innerGlow = edge * innerGlowIntensity * 0.5;

    // Compose inside color: layer-textured base blended with edge color
    float4 layerTex    = overlay * center * sampleColor * layerColor;
    float4 edgeResult  = edgeColor * sampleColor;
    float  condFactor  = 1.0 - edge;

    float4 insideResult = layerTex * condFactor + edgeResult * edge;
    insideResult.rgb   += innerGlow;
    insideResult.a      = center.a;

    // =======================================================================
    // FINAL COMPOSITE — one path chosen per pixel via masks
    // =======================================================================
    return outsideResult * isOutside + insideResult * isInside;
}


// Default technique — backward compatible
technique DefaultTechnique
{
    pass ParticlePass
    {
        PixelShader = compile ps_2_0 MetaballEdgePS();
    }
}

// Enhanced technique — same shader, separate entry point for future use
technique EnhancedTechnique
{
    pass ParticlePass
    {
        PixelShader = compile ps_2_0 MetaballEdgePS();
    }
}
