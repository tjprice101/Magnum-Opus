using System;
using Microsoft.Xna.Framework;
using Terraria.Graphics.Shaders;

namespace MagnumOpus.Common.Systems.VFX.Trails
{
    public enum GPUPrimitiveTextureMode { Normalized, Distance }
    public enum GPUPrimitiveJoinStyle { Flat, Smooth, Miter }
    public enum GPUPrimitiveSmoothingType { CatmullRom, Cardinal, Linear, Hermite, CubicBezier }
    public enum GPUPrimitiveFrameTransport { Basic, ParallelTransport }
    public enum GPUPrimitiveTopology { TriangleList, TriangleStrip }
    public enum GPUPrimitiveCapStyle { None, Flat }

    /// <summary>
    /// Settings for the GPU primitive trail renderer.
    /// Shared system adapted from IncisorPrimitiveSettings.
    /// </summary>
    public readonly struct GPUPrimitiveSettings
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
        public readonly GPUPrimitiveTextureMode TextureCoordinateMode;
        public readonly float TextureCycleLength;
        public readonly float TextureScrollOffset;
        public readonly Func<float, float> TextureCoordinateFunction;
        public readonly GPUPrimitiveJoinStyle JoinStyle;
        public readonly float JoinMiterLimit;
        public readonly GPUPrimitiveSmoothingType SmoothingType;
        public readonly int SmoothingSegments;
        public readonly float SmoothingTension;
        public readonly GPUPrimitiveFrameTransport FrameTransportMode;
        public readonly GPUPrimitiveTopology Topology;
        public readonly GPUPrimitiveCapStyle CapStyle;

        public GPUPrimitiveSettings(
            VertexWidthFunction widthFunction,
            VertexColorFunction colorFunction,
            VertexOffsetFunction offsetFunction = null,
            bool smoothen = true,
            bool pixelate = false,
            MiscShaderData shader = null,
            bool useUnscaledMatrices = false,
            (Vector2, Vector2)? initialVertexPositionsOverride = null,
            GPUPrimitiveTextureMode textureCoordinateMode = GPUPrimitiveTextureMode.Normalized,
            float textureCycleLength = 1f,
            float textureScrollOffset = 0f,
            Func<float, float> textureCoordinateFunction = null,
            GPUPrimitiveJoinStyle joinStyle = GPUPrimitiveJoinStyle.Smooth,
            float joinMiterLimit = 4f,
            GPUPrimitiveSmoothingType smoothingType = GPUPrimitiveSmoothingType.CatmullRom,
            int smoothingSegments = 0,
            float smoothingTension = 0f,
            GPUPrimitiveFrameTransport frameTransportMode = GPUPrimitiveFrameTransport.ParallelTransport,
            GPUPrimitiveTopology topology = GPUPrimitiveTopology.TriangleStrip,
            GPUPrimitiveCapStyle capStyle = GPUPrimitiveCapStyle.None)
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
            TextureCycleLength = Math.Abs(textureCycleLength) <= GPUPrimitiveTrailRenderer.Epsilon ? 1f : textureCycleLength;
            TextureScrollOffset = textureScrollOffset;
            TextureCoordinateFunction = textureCoordinateFunction;
            JoinStyle = joinStyle;
            JoinMiterLimit = Math.Max(joinMiterLimit, 1f);
            SmoothingType = smoothingType;
            SmoothingSegments = Math.Max(smoothingSegments, 0);
            SmoothingTension = smoothingTension;
            FrameTransportMode = frameTransportMode;
            Topology = topology;
            CapStyle = GPUPrimitiveCapStyle.Flat;
        }
    }
}
