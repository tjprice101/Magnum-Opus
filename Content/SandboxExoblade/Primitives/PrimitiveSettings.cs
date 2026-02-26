using System;
using Microsoft.Xna.Framework;
using Terraria.Graphics.Shaders;

namespace MagnumOpus.Content.SandboxExoblade.Primitives
{
    public enum PrimitiveTextureMode { Normalized, Distance }
    public enum PrimitiveJoinStyle { Flat, Smooth, Miter }
    public enum PrimitiveSmoothingType { CatmullRom, Cardinal, Linear, Hermite, CubicBezier }
    public enum PrimitiveFrameTransportMode { Basic, ParallelTransport }
    public enum PrimitiveTopology { TriangleList, TriangleStrip }
    public enum PrimitiveCapStyle { None, Flat }

    public readonly struct PrimitiveSettings
    {
        public delegate float VertexWidthFunction(float trailLengthInterpolant, Vector2 vertexPosition);
        public delegate Color VertexColorFunction(float trailLengthInterpolant, Vector2 vertexPosition);
        public delegate Vector2 VertexOffsetFunction(float trailLengthInterpolant, Vector2 vertexPosition);

        public readonly VertexWidthFunction WidthFunction;
        public readonly VertexColorFunction ColorFunction;
        public readonly VertexOffsetFunction OffsetFunction;
        public readonly bool Smoothen;
        public readonly bool Pixelate;
        public readonly bool UseUnscaledMatrices;
        public readonly MiscShaderData Shader;
        public readonly (Vector2, Vector2)? InitialVertexPositionsOverride;
        public readonly PrimitiveTextureMode TextureCoordinateMode;
        public readonly float TextureCycleLength;
        public readonly float TextureScrollOffset;
        public readonly Func<float, float> TextureCoordinateFunction;
        public readonly PrimitiveJoinStyle JoinStyle;
        public readonly float JoinMiterLimit;
        public readonly bool DebugWireframe;
        public readonly Color WireframeColor;
        public readonly PrimitiveSmoothingType SmoothingType;
        public readonly int SmoothingSegments;
        public readonly float SmoothingTension;
        public readonly PrimitiveFrameTransportMode FrameTransportMode;
        public readonly PrimitiveTopology Topology;
        public readonly PrimitiveCapStyle CapStyle;

        public PrimitiveSettings(VertexWidthFunction widthFunction, VertexColorFunction colorFunction, VertexOffsetFunction offsetFunction = null, bool smoothen = true, bool pixelate = false, MiscShaderData shader = null, bool useUnscaledMatrices = false, (Vector2, Vector2)? initialVertexPositionsOverride = null, PrimitiveTextureMode textureCoordinateMode = PrimitiveTextureMode.Normalized, float textureCycleLength = 1f, float textureScrollOffset = 0f, Func<float, float> textureCoordinateFunction = null, PrimitiveJoinStyle joinStyle = PrimitiveJoinStyle.Smooth, float joinMiterLimit = 4f, bool debugWireframe = false, Color? wireframeColor = null, PrimitiveSmoothingType smoothingType = PrimitiveSmoothingType.CatmullRom, int smoothingSegments = 0, float smoothingTension = 0f, PrimitiveFrameTransportMode frameTransportMode = PrimitiveFrameTransportMode.ParallelTransport, PrimitiveTopology topology = PrimitiveTopology.TriangleStrip, PrimitiveCapStyle capStyle = PrimitiveCapStyle.None)
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
            TextureCycleLength = Math.Abs(textureCycleLength) <= PrimitiveRenderer.Epsilon ? 1f : textureCycleLength;
            TextureScrollOffset = textureScrollOffset;
            TextureCoordinateFunction = textureCoordinateFunction;
            JoinStyle = joinStyle;
            JoinMiterLimit = Math.Max(joinMiterLimit, 1f);
            DebugWireframe = debugWireframe;
            WireframeColor = wireframeColor ?? Color.LimeGreen;
            SmoothingType = smoothingType;
            SmoothingSegments = Math.Max(smoothingSegments, 0);
            SmoothingTension = smoothingTension;
            FrameTransportMode = frameTransportMode;
            Topology = topology;
            CapStyle = PrimitiveCapStyle.Flat;
        }
    }
}
