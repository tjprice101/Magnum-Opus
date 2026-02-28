using System;
using Microsoft.Xna.Framework;
using Terraria.Graphics.Shaders;

namespace MagnumOpus.Content.Eroica.Weapons.BlossomOfTheSakura.Primitives
{
    public readonly struct BlossomTrailSettings
    {
        public delegate float VertexWidthFunction(float completionRatio);
        public delegate Color VertexColorFunction(float completionRatio);
        public delegate Vector2 VertexOffsetFunction(float completionRatio);

        public readonly VertexWidthFunction WidthFunction;
        public readonly VertexColorFunction ColorFunction;
        public readonly VertexOffsetFunction OffsetFunction;
        public readonly bool Smoothen;
        public readonly MiscShaderData Shader;
        public readonly float TextureScrollOffset;

        public BlossomTrailSettings(
            VertexWidthFunction widthFunction,
            VertexColorFunction colorFunction,
            VertexOffsetFunction offsetFunction = null,
            bool smoothen = true,
            MiscShaderData shader = null,
            float textureScrollOffset = 0f)
        {
            WidthFunction = widthFunction;
            ColorFunction = colorFunction;
            OffsetFunction = offsetFunction;
            Smoothen = smoothen;
            Shader = shader;
            TextureScrollOffset = textureScrollOffset;
        }
    }
}
