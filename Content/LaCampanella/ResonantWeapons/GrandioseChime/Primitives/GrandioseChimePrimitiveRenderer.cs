using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using ReLogic.Content;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.GrandioseChime.Primitives
{
    public class GrandioseBeamTrailSettings
    {
        public string TrailTexturePath = "MagnumOpus/Assets/SandboxLastPrism/Trails/Trail5Loop";
        public Color ColorStart = Color.White;
        public Color ColorEnd = Color.Transparent;
        public float Width = 12f;
        public float BloomIntensity = 0.4f;
    }

    public class GrandioseChimePrimitiveRenderer : IDisposable
    {
        private DynamicVertexBuffer vertexBuffer;
        private DynamicIndexBuffer indexBuffer;
        private GrandioseChimeVertexType[] vertices;
        private short[] indices;
        private bool disposed;
        private const int MaxVertices = 1024;
        private const int MaxIndices = 3072;

        public GrandioseChimePrimitiveRenderer()
        {
            var device = Main.graphics.GraphicsDevice;
            vertices = new GrandioseChimeVertexType[MaxVertices];
            indices = new short[MaxIndices];
            vertexBuffer = new DynamicVertexBuffer(device, GrandioseChimeVertexType._vertexDeclaration, MaxVertices, BufferUsage.WriteOnly);
            indexBuffer = new DynamicIndexBuffer(device, IndexElementSize.SixteenBits, MaxIndices, BufferUsage.WriteOnly);
        }

        public void DrawTrail(SpriteBatch sb, List<Vector2> points, GrandioseBeamTrailSettings settings, Vector2 screenPos)
        {
            if (points == null || points.Count < 2 || disposed) return;

            int segCount = points.Count - 1;
            int vertCount = (segCount + 1) * 2;
            int idxCount = segCount * 6;
            if (vertCount > MaxVertices || idxCount > MaxIndices) return;

            for (int i = 0; i <= segCount; i++)
            {
                float t = (float)i / segCount;
                Vector2 pos = points[i] - screenPos;

                Vector2 dir;
                if (i == 0) dir = Vector2.Normalize(points[1] - points[0]);
                else if (i == segCount) dir = Vector2.Normalize(points[i] - points[i - 1]);
                else dir = Vector2.Normalize(points[i + 1] - points[i - 1]);

                Vector2 perp = new Vector2(-dir.Y, dir.X);
                float width = settings.Width * (1f - t * 0.6f);
                Color color = Color.Lerp(settings.ColorStart, settings.ColorEnd, t);

                vertices[i * 2] = new GrandioseChimeVertexType(pos + perp * width, color, new Vector3(t, 0f, 1f));
                vertices[i * 2 + 1] = new GrandioseChimeVertexType(pos - perp * width, color, new Vector3(t, 1f, 1f));
            }

            int idx = 0;
            for (int i = 0; i < segCount; i++)
            {
                int v = i * 2;
                indices[idx++] = (short)v;
                indices[idx++] = (short)(v + 1);
                indices[idx++] = (short)(v + 2);
                indices[idx++] = (short)(v + 1);
                indices[idx++] = (short)(v + 3);
                indices[idx++] = (short)(v + 2);
            }

            try
            {
                var device = Main.graphics.GraphicsDevice;
                vertexBuffer.SetData(vertices, 0, vertCount);
                indexBuffer.SetData(indices, 0, idxCount);

                var trailTex = ModContent.Request<Texture2D>(settings.TrailTexturePath, AssetRequestMode.ImmediateLoad).Value;

                sb.End();

                BlendState oldBlend = device.BlendState;
                DepthStencilState oldDepth = device.DepthStencilState;
                RasterizerState oldRaster = device.RasterizerState;
                try
                {
                    device.SetVertexBuffer(vertexBuffer);
                    device.Indices = indexBuffer;
                    device.Textures[0] = trailTex;
                    device.SamplerStates[0] = SamplerState.LinearClamp;
                    device.BlendState = BlendState.Additive;
                    device.DepthStencilState = DepthStencilState.None;
                    device.RasterizerState = RasterizerState.CullNone;

                    var basicEffect = new BasicEffect(device)
                    {
                        VertexColorEnabled = true,
                        TextureEnabled = true,
                        Texture = trailTex,
                        World = Matrix.Identity,
                        View = Matrix.Identity,
                        Projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1)
                    };
                    foreach (var pass in basicEffect.CurrentTechnique.Passes) pass.Apply();

                    device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertCount, 0, idxCount / 3);
                }
                finally
                {
                    device.BlendState = oldBlend;
                    device.DepthStencilState = oldDepth;
                    device.RasterizerState = oldRaster;
                    device.SetVertexBuffer(null);
                    device.Indices = null;
                }

                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                if (settings.BloomIntensity > 0f && points.Count > 0)
                {
                    var bloomTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow", AssetRequestMode.ImmediateLoad).Value;
                    Color bloomCol = settings.ColorStart * settings.BloomIntensity;
                    sb.Draw(bloomTex, points[0] - screenPos, null, bloomCol, 0f,
                        bloomTex.Size() / 2f, settings.Width * 0.015f, SpriteEffects.None, 0f);
                }
            }
            catch
            {
                try
                {
                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                        DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                }
                catch { }
            }
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            vertexBuffer?.Dispose();
            indexBuffer?.Dispose();
        }
    }
}