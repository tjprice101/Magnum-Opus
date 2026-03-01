// =============================================================================
// Incisor of Moonlight  ESwing Sprite Rotation Shader
// =============================================================================
// Rotates the square weapon sprite around its pommel origin to render
// the blade during swing animations. Only works on square sprites.
// =============================================================================

float rotation;
float pommelToOriginPercent;
float4 color;

texture sampleTexture;
sampler2D Texture1Sampler = sampler_state
{
    texture = <sampleTexture>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = wrap;
};

float realCos(float value)
{
    return sin(value + 1.57079);
}

float4 main(float2 uv : TEXCOORD) : COLOR
{
    float2x2 rotate = float2x2(realCos(rotation), -sin(rotation), sin(rotation), realCos(rotation));
    float spriteDiagonal = 1 / (sqrt(2) / 4);
    float spriteDiagonalReal = spriteDiagonal * (1 / (1 - pommelToOriginPercent));

    float2x2 downscale = float2x2(spriteDiagonalReal, 0, 0, spriteDiagonalReal);
    float displaceFromOrigin = (1 / (1 - pommelToOriginPercent)) * (1 - 0.5 * (1 - pommelToOriginPercent));

    uv += float2(-0.5, -0.5);
    uv = mul(uv, rotate);
    uv = mul(uv, downscale);
    uv += float2(-displaceFromOrigin, displaceFromOrigin);
    uv += float2(0.5, 0.5);

    // Crop out-of-bounds pixels
    if (uv.x < 0 || uv.x >= 1 || uv.y < 0 || uv.y >= 1)
        return float4(0, 0, 0, 0);

    return tex2D(Texture1Sampler, uv) * color;
}

technique Technique1
{
    pass IncisorSwingPass
    {
        PixelShader = compile ps_3_0 main();
    }
}
