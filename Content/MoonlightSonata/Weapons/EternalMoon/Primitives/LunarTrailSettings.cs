using System;
using Microsoft.Xna.Framework;
using Terraria.Graphics.Shaders;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.EternalMoon.Primitives
{
    /// <summary>
    /// Configuration struct for the Lunar Trail Renderer.
    /// Defines how trail vertices are constructed and shaded.
    /// </summary>
    public readonly struct LunarTrailSettings
    {
        public delegate float VertexWidthFunction(float trailLengthInterpolant, Vector2 vertexPosition);
        public delegate Color VertexColorFunction(float trailLengthInterpolant, Vector2 vertexPosition);
        public delegate Vector2 VertexOffsetFunction(float trailLengthInterpolant, Vector2 vertexPosition);

        public readonly VertexWidthFunction WidthFunction;
        public readonly VertexColorFunction ColorFunction;
        public readonly VertexOffsetFunction OffsetFunction;
        public readonly bool Smoothen;
        public readonly MiscShaderData Shader;
        public readonly float TextureScrollOffset;

        public LunarTrailSettings(
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
