// Inferno Trail Shader - Wrath's Cleaver afterswing trail
// Creates a burning ember trail that dissipates into ash

sampler uImage0 : register(s0); // Trail UV texture
sampler uImage1 : register(s1); // Noise texture

float uTime;
float3 uColor;          // Primary (InfernalRed)
float3 uSecondaryColor; // Secondary (EmberOrange)
float uOpacity;
float uIntensity;
float uScrollSpeed;
float uOverbrightMult;

float4 InfernoTrailMain(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Scrolling noise for fire effect
    float2 noiseUV1 = float2(coords.x * 3.0 - uTime * uScrollSpeed, coords.y * 2.0);
    float2 noiseUV2 = float2(coords.x * 2.0 + uTime * uScrollSpeed * 0.7, coords.y * 1.5 + uTime * 0.3);
    
    float noise1 = tex2D(uImage1, noiseUV1).r;
    float noise2 = tex2D(uImage1, noiseUV2).r;
    float combinedNoise = (noise1 + noise2) * 0.5;
    
    // Trail fades along length (coords.x: 0=tip, 1=base)
    float lengthFade = pow(1.0 - coords.x, 1.5);
    
    // Width fade with ember dissolution
    float widthFade = smoothstep(0.0, 0.2 + combinedNoise * 0.15, coords.y) * 
                      smoothstep(1.0, 0.8 - combinedNoise * 0.15, coords.y);
    
    // Rising embers effect - noise-driven breakup at trail edges
    float emberBreakup = smoothstep(0.3, 0.7, combinedNoise + lengthFade * 0.3);
    
    // Color interpolation: hot core to cooler edges
    float heatMap = pow(saturate(1.0 - abs(coords.y - 0.5) * 2.0), 1.5) * lengthFade;
    float3 trailColor = lerp(uColor, uSecondaryColor, heatMap);
    
    // White-hot center line
    float coreLine = pow(saturate(1.0 - abs(coords.y - 0.5) * 4.0), 3.0) * lengthFade;
    trailColor = lerp(trailColor, float3(1, 0.98, 0.9), coreLine * 0.7);
    
    float4 baseTexture = tex2D(uImage0, coords);
    float alpha = baseTexture.a * widthFade * lengthFade * emberBreakup * uOpacity * uIntensity;
    
    return float4(trailColor * uOverbrightMult, 1.0) * alpha * sampleColor;
}

// Ember particle glow layer
float4 InfernoTrailEmbers(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 noiseUV = float2(coords.x * 4.0 - uTime * 1.5, coords.y * 3.0 + uTime * 0.5);
    float noise = tex2D(uImage1, noiseUV).r;
    
    // Sporadic bright points (embers)
    float emberThreshold = 0.72 - coords.x * 0.2;
    float embers = smoothstep(emberThreshold, emberThreshold + 0.08, noise);
    
    float lengthFade = pow(1.0 - coords.x, 2.0);
    float widthFade = smoothstep(0.0, 0.3, coords.y) * smoothstep(1.0, 0.7, coords.y);
    
    float3 emberColor = lerp(uSecondaryColor, float3(1, 0.9, 0.7), embers * 0.5);
    float alpha = embers * lengthFade * widthFade * uOpacity * 0.8;
    
    return float4(emberColor * uOverbrightMult * 0.6, 1.0) * alpha * sampleColor;
}

technique InfernoTrailTechnique
{
    pass InfernoMain
    {
        PixelShader = compile ps_3_0 InfernoTrailMain();
    }
}

technique InfernoEmbersTechnique
{
    pass InfernoEmbers
    {
        PixelShader = compile ps_3_0 InfernoTrailEmbers();
    }
}
