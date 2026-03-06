using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;
using ReLogic.Content;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.PiercingBellsResonance.Primitives
{
    /// <summary>
    /// Configuration for Piercing Bells' bullet/crystal/blast primitive trails.
    /// </summary>
    public class BulletTrailSettings
    {
        public string TrailTexturePath = "MagnumOpus/Assets/SandboxLastPrism/Trails/Trail5Loop";
        public Color ColorStart = Color.White;
        public Color ColorEnd = Color.Transparent;
        public float Width = 8f;
        public float BloomIntensity = 0.3f;

        /// <summary>Custom width function (overrides Width if set). Parameter: completion ratio 0..1.</summary>
        public Func<float, float> WidthFunc = null;

        /// <summary>Custom color function (overrides ColorStart/End lerp if set). Parameter: completion ratio 0..1.</summary>
        public Func<float, Color> ColorFunc = null;

        /// <summary>Custom shader for rendering. If null, falls back to BasicEffect.</summary>
        public MiscShaderData Shader = null;

        /// <summary>Whether to use Catmull-Rom smoothing on positions.</summary>
        public bool Smoothen = true;

        public float GetWidth(float t) => WidthFunc != null ? WidthFunc(t) : Width * (1f - t * 0.7f);
        public Color GetColor(float t) => ColorFunc != null ? ColorFunc(t) : Color.Lerp(ColorStart, ColorEnd, t);
    }

    /// <summary>
    /// GPU primitive trail renderer for Piercing Bells' Resonance weapons.
    /// Builds triangle-strip ribbon mesh and renders with custom pixel shaders.
    /// </summary>
    public class PiercingBellsPrimitiveRenderer : IDisposable
    {
        private DynamicVertexBuffer vertexBuffer;
        private DynamicIndexBuffer indexBuffer;
        private PiercingBellsVertexType[] vertices;
        private short[] indices;
        private bool disposed;
        private const int MaxVertices = 2048;
        private const int MaxIndices = 6144;

        public PiercingBellsPrimitiveRenderer()
        {
            vertices = new PiercingBellsVertexType[MaxVertices];
            indices = new short[MaxIndices];
        }

        private void EnsureBuffers(GraphicsDevice device)
        {
            if (vertexBuffer == null || vertexBuffer.IsDisposed)
                vertexBuffer = new DynamicVertexBuffer(device, PiercingBellsVertexType._vertexDeclaration, MaxVertices, BufferUsage.WriteOnly);
            if (indexBuffer == null || indexBuffer.IsDisposed)
                indexBuffer = new DynamicIndexBuffer(device, IndexElementSize.SixteenBits, MaxIndices, BufferUsage.WriteOnly);
        }

        public void DrawTrail(SpriteBatch sb, List<Vector2> points, BulletTrailSettings settings, Vector2 screenOffset)
        {
            if (points == null || points.Count < 2 || disposed) return;

            var validPoints = new List<Vector2>(points.Count);
            foreach (var p in points)
                if (p != Vector2.Zero) validPoints.Add(p);
            if (validPoints.Count < 2) return;

            Vector2[] finalPositions = settings.Smoothen && validPoints.Count > 2
                ? SmoothPositions(validPoints, validPoints.Count)
                : validPoints.ToArray();

            if (finalPositions.Length < 2) return;

            float[] completionRatios = ComputeCompletionRatios(finalPositions);
            int vertexCount = 0;
            int indexCount = 0;

            for (int i = 0; i < finalPositions.Length; i++)
            {
                float t = completionRatios[i];
                float halfWidth = settings.GetWidth(t) * 0.5f;
                Color color = settings.GetColor(t);

                Vector2 tangent;
                if (i == 0) tangent = finalPositions[1] - finalPositions[0];
                else if (i == finalPositions.Length - 1) tangent = finalPositions[i] - finalPositions[i - 1];
                else tangent = finalPositions[i + 1] - finalPositions[i - 1];

                if (tangent.LengthSquared() < 0.0001f) tangent = Vector2.UnitX;
                tangent.Normalize();

                Vector2 normal = new Vector2(-tangent.Y, tangent.X);
                Vector2 screenPos = finalPositions[i] - screenOffset;

                int vi = vertexCount;
                float widthCorrection = Math.Max(halfWidth, 0.01f);
                vertices[vi] = new PiercingBellsVertexType(screenPos + normal * halfWidth, color, new Vector3(t, 0f, widthCorrection));
                vertices[vi + 1] = new PiercingBellsVertexType(screenPos - normal * halfWidth, color, new Vector3(t, 1f, widthCorrection));
                vertexCount += 2;

                if (i > 0)
                {
                    int prev = vi - 2;
                    indices[indexCount++] = (short)prev;
                    indices[indexCount++] = (short)(prev + 1);
                    indices[indexCount++] = (short)vi;
                    indices[indexCount++] = (short)(prev + 1);
                    indices[indexCount++] = (short)(vi + 1);
                    indices[indexCount++] = (short)vi;
                }

                if (vertexCount >= MaxVertices - 2 || indexCount >= MaxIndices - 6) break;
            }

            if (vertexCount < 4 || indexCount < 6) return;

            var device = Main.graphics.GraphicsDevice;
            if (device == null || device.IsDisposed) return;

            EnsureBuffers(device);

            var oldRasterizer = device.RasterizerState;
            var oldBlendState = device.BlendState;
            var oldDepthStencil = device.DepthStencilState;
            var oldSampler0 = device.SamplerStates[0];

            try
            {
                Texture2D trailTex = null;
                try { trailTex = ModContent.Request<Texture2D>(settings.TrailTexturePath, AssetRequestMode.ImmediateLoad)?.Value; }
                catch { }

                sb.End();

                device.RasterizerState = RasterizerState.CullNone;
                device.BlendState = MagnumBlendStates.TrueAdditive;
                device.DepthStencilState = DepthStencilState.None;
                device.SamplerStates[0] = SamplerState.LinearWrap;

                if (trailTex != null)
                    device.Textures[0] = trailTex;

                if (settings.Shader != null && settings.Shader.Shader != null)
                {
                    Matrix view = Matrix.CreateLookAt(Vector3.Zero, Vector3.UnitZ, Vector3.Up);
                    Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);
                    Matrix wvp = view * projection;
                    settings.Shader.Shader.Parameters["uWorldViewProjection"]?.SetValue(wvp);
                    settings.Shader.Apply();
                }
                else
                {
                    return;
                }

                vertexBuffer.SetData(vertices, 0, vertexCount, SetDataOptions.Discard);
                indexBuffer.SetData(indices, 0, indexCount, SetDataOptions.Discard);
                device.SetVertexBuffer(vertexBuffer);
                device.Indices = indexBuffer;
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexCount, 0, indexCount / 3);
            }
            finally
            {
                device.RasterizerState = oldRasterizer;
                device.BlendState = oldBlendState;
                device.DepthStencilState = oldDepthStencil;
                device.SamplerStates[0] = oldSampler0;
                device.SetVertexBuffer(null);
                device.Indices = null;

                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }

            if (settings.BloomIntensity > 0f && validPoints.Count > 0)
            {
                try
                {
                    var bloomTex = ModContent.Request<Texture2D>(
                        "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow",
                        AssetRequestMode.ImmediateLoad)?.Value;
                    if (bloomTex != null)
                    {
                        try { sb.End(); } catch { }
                        sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                            DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                        Color bloomCol = settings.ColorStart * settings.BloomIntensity;
                        float bloomScale = settings.GetWidth(0f) * 0.025f;
                        sb.Draw(bloomTex, validPoints[0] - screenOffset, null, bloomCol, 0f,
                            bloomTex.Size() / 2f, bloomScale, SpriteEffects.None, 0f);

                        try { sb.End(); } catch { }
                        sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                            DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                    }
                }
                catch { }
            }
        }

        #region Position Processing

        private static float[] ComputeCompletionRatios(Vector2[] positions)
        {
            float[] ratios = new float[positions.Length];
            if (positions.Length <= 1) { ratios[0] = 0f; return ratios; }
            float totalLength = 0f;
            for (int i = 1; i < positions.Length; i++)
                totalLength += Vector2.Distance(positions[i - 1], positions[i]);
            if (totalLength < 0.001f)
            {
                for (int i = 0; i < positions.Length; i++)
                    ratios[i] = (float)i / (positions.Length - 1);
                return ratios;
            }
            float accumulated = 0f;
            ratios[0] = 0f;
            for (int i = 1; i < positions.Length; i++)
            {
                accumulated += Vector2.Distance(positions[i - 1], positions[i]);
                ratios[i] = accumulated / totalLength;
            }
            return ratios;
        }

        private static Vector2[] SmoothPositions(List<Vector2> positions, int outputCount)
        {
            if (positions.Count < 3) return positions.ToArray();
            Vector2[] result = new Vector2[outputCount];
            for (int i = 0; i < outputCount; i++)
            {
                float t = (float)i / Math.Max(outputCount - 1, 1);
                float scaled = t * (positions.Count - 1);
                int p1 = (int)scaled;
                int p0 = Math.Max(p1 - 1, 0);
                int p2 = Math.Min(p1 + 1, positions.Count - 1);
                int p3 = Math.Min(p1 + 2, positions.Count - 1);
                float frac = scaled - p1;
                result[i] = CatmullRom(positions[p0], positions[p1], positions[p2], positions[p3], frac);
            }
            return result;
        }

        private static Vector2 CatmullRom(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            float t2 = t * t, t3 = t2 * t;
            return 0.5f * (
                (2f * p1) + (-p0 + p2) * t +
                (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
                (-p0 + 3f * p1 - 3f * p2 + p3) * t3);
        }

        #endregion

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            vertexBuffer?.Dispose();
            indexBuffer?.Dispose();
        }
    }
}