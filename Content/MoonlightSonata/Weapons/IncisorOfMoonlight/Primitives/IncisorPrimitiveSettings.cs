using System;
using Microsoft.Xna.Framework;
using Terraria.Graphics.Shaders;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight.Primitives
{
    public enum IncisorTextureMode { Normalized, Distance }
    public enum IncisorJoinStyle { Flat, Smooth, Miter }
    public enum IncisorSmoothingType { CatmullRom, Cardinal, Linear, Hermite, CubicBezier }
    public enum IncisorFrameTransport { Basic, ParallelTransport }
    public enum IncisorTopology { TriangleList, TriangleStrip }
    public enum IncisorCapStyle { None, Flat }

    public readonly struct IncisorPrimitiveSettings
    {
        public delegate float VertexWidthFunction(float completionRatio, Vector2 vertexPosition);
        public delegate Color VertexColorFunction(float completionRatio, Vector2 vertexPosition);
        public delegate Vector2 VertexOffsetFunction(float completionRatio, Vector2 vertexPosition);

        public readonly VertexWidthFunction WidthFunction;
        public readonly VertexColorFunction ColorFunction;
        public readonly VertexOffsetFunction OffsetFunction;
        public readonly bool Smoothen;
        public readonly bool Pixelate;
        public readonly bool UseUnscaledMatrices;
        public readonly MiscShaderData Shader;
        public readonly (Vector2, Vector2)? InitialVertexPositionsOverride;
        public readonly IncisorTextureMode TextureCoordinateMode;
        public readonly float TextureCycleLength;
        public readonly float TextureScrollOffset;
        public readonly Func<float, float> TextureCoordinateFunction;
        public readonly IncisorJoinStyle JoinStyle;
        public readonly float JoinMiterLimit;
        public readonly IncisorSmoothingType SmoothingType;
        public readonly int SmoothingSegments;
        public readonly float SmoothingTension;
        public readonly IncisorFrameTransport FrameTransportMode;
        public readonly IncisorTopology Topology;
        public readonly IncisorCapStyle CapStyle;

        public IncisorPrimitiveSettings(
            VertexWidthFunction widthFunction,
            VertexColorFunction colorFunction,
            VertexOffsetFunction offsetFunction = null,
            bool smoothen = true,
            bool pixelate = false,
            MiscShaderData shader = null,
            bool useUnscaledMatrices = false,
            (Vector2, Vector2)? initialVertexPositionsOverride = null,
            IncisorTextureMode textureCoordinateMode = IncisorTextureMode.Normalized,
            float textureCycleLength = 1f,
            float textureScrollOffset = 0f,
            Func<float, float> textureCoordinateFunction = null,
            IncisorJoinStyle joinStyle = IncisorJoinStyle.Smooth,
            float joinMiterLimit = 4f,
            IncisorSmoothingType smoothingType = IncisorSmoothingType.CatmullRom,
            int smoothingSegments = 0,
            float smoothingTension = 0f,
            IncisorFrameTransport frameTransportMode = IncisorFrameTransport.ParallelTransport,
            IncisorTopology topology = IncisorTopology.TriangleStrip,
            IncisorCapStyle capStyle = IncisorCapStyle.None)
        {
            WidthFunction = widthFunction;
            ColorFunction = colorFunction;
            OffsetFunction = offsetFunction;
            Smoothen = smoothen;
            Pixelate = pixelate;
            Shader = shader;
            UseUnscaledMatrices = useUnscaledMatrices;
            InitialVertexPositionsOverride = initialVertexPositionsOverride;
            TextureCoordinateMode = textureCoordinateMode;
            TextureCycleLength = Math.Abs(textureCycleLength) <= IncisorPrimitiveRenderer.Epsilon ? 1f : textureCycleLength;
            TextureScrollOffset = textureScrollOffset;
            TextureCoordinateFunction = textureCoordinateFunction;
            JoinStyle = joinStyle;
            JoinMiterLimit = Math.Max(joinMiterLimit, 1f);
            SmoothingType = smoothingType;
            SmoothingSegments = Math.Max(smoothingSegments, 0);
            SmoothingTension = smoothingTension;
            FrameTransportMode = frameTransportMode;
            Topology = topology;
            CapStyle = IncisorCapStyle.Flat;
        }
    }
}
