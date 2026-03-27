using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems.VFX.Trails
{
    /// <summary>
    /// Shared GPU primitive trail renderer for high-quality melee swing arcs.
    /// Manages DynamicVertexBuffer/IndexBuffer and renders trail geometry with custom shaders.
    /// Adapted from IncisorPrimitiveRenderer for use across all MeleeSwingBase weapons.
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public sealed class GPUPrimitiveTrailRenderer : ModSystem
    {
        private static DynamicVertexBuffer VertexBuffer;
        private static DynamicIndexBuffer IndexBuffer;
        private static GPUPrimitiveSettings MainSettings;
        private static GPUPrimitiveTopology ActiveTopology;
        private static Vector2[] MainPositions;
        private static Vector2[] MainTangents;
        private static Vector2[] MainNormals;
        private static GPUPrimitiveVertex[] MainVertices;
        private static short[] MainIndices;
        private static int[] NonSmoothIndexScratch;
        private static short StartCapCenterIndex;
        private static short EndCapCenterIndex;
        private const short MaxPositions = 1000;
        private const short MaxVertices = 3072;
        private const short MaxIndices = 8192;
        private static readonly List<Vector2> ControlPointsCache = new(MaxPositions);
        private static short PositionsIndex;
        private static float[] MainCompletionRatios;
        private static float TotalTrailLength;
        public const float Epsilon = 1e-6f;
        private static short VerticesIndex;
        private static short IndicesIndex;

        public override void OnModLoad()
        {
            Main.QueueMainThreadAction(() =>
            {
                MainPositions = new Vector2[MaxPositions];
                MainVertices = new GPUPrimitiveVertex[MaxVertices];
                MainIndices = new short[MaxIndices];
                MainCompletionRatios = new float[MaxPositions];
                MainTangents = new Vector2[MaxPositions];
                MainNormals = new Vector2[MaxPositions];
                NonSmoothIndexScratch = new int[MaxPositions];
                VertexBuffer ??= new DynamicVertexBuffer(Main.instance.GraphicsDevice, GPUPrimitiveVertex.VertexDecl, MaxVertices, BufferUsage.WriteOnly);
                IndexBuffer ??= new DynamicIndexBuffer(Main.instance.GraphicsDevice, IndexElementSize.SixteenBits, MaxIndices, BufferUsage.WriteOnly);
            });
        }

        public override void OnModUnload()
        {
            Main.QueueMainThreadAction(() =>
            {
                MainPositions = null;
                MainVertices = null;
                MainIndices = null;
                MainCompletionRatios = null;
                MainTangents = null;
                MainNormals = null;
                NonSmoothIndexScratch = null;
                VertexBuffer?.Dispose();
                VertexBuffer = null;
                IndexBuffer?.Dispose();
                IndexBuffer = null;
            });
        }

        public static void RenderTrail(List<Vector2> positions, GPUPrimitiveSettings settings, int? pointsToCreate = null)
            => RenderTrail(positions.ToArray(), settings, pointsToCreate);

        public static void RenderTrail(Vector2[] positions, GPUPrimitiveSettings settings, int? pointsToCreate = null)
        {
            if (positions.Length <= 2 || positions.Length > MaxPositions)
                return;

            int desiredPointCount = pointsToCreate ?? positions.Length;
            desiredPointCount = Math.Clamp(desiredPointCount, 2, MaxPositions);

            MainSettings = settings;
            ActiveTopology = settings.CapStyle != GPUPrimitiveCapStyle.None ? GPUPrimitiveTopology.TriangleList : settings.Topology;

            if (!AssignPoints(positions, settings, desiredPointCount))
                return;

            AssignCompletionData();

            if (PositionsIndex <= 2)
                return;

            AssignVertices();
            AssignIndices();
            PrivateRender();
        }

        private static void PrivateRender()
        {
            if (VerticesIndex <= 3) return;

            if (ActiveTopology == GPUPrimitiveTopology.TriangleList)
            {
                if (IndicesIndex < 6 || IndicesIndex % 3 != 0) return;
            }
            else if (ActiveTopology == GPUPrimitiveTopology.TriangleStrip && IndicesIndex < 4) return;

            Main.instance.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            Main.instance.GraphicsDevice.RasterizerState.ScissorTestEnable = true;
            Main.instance.GraphicsDevice.ScissorRectangle = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);

            Matrix view, projection;
            if (MainSettings.Pixelate || MainSettings.UseUnscaledMatrices)
            {
                projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);
                view = Matrix.Identity;
            }
            else
                CalculatePerspectiveMatrices(out view, out projection);

            var shaderToUse = MainSettings.Shader ?? GameShaders.Misc["MagnumOpus:GPUPrimitiveStandard"];
            shaderToUse.Shader.Parameters["uWorldViewProjection"]?.SetValue(view * projection);
            shaderToUse.Apply();

            VertexBuffer.SetData(MainVertices, 0, VerticesIndex, SetDataOptions.Discard);
            IndexBuffer.SetData(MainIndices, 0, IndicesIndex, SetDataOptions.Discard);
            Main.instance.GraphicsDevice.SetVertexBuffer(VertexBuffer);
            Main.instance.GraphicsDevice.Indices = IndexBuffer;

            PrimitiveType primitiveType = ActiveTopology == GPUPrimitiveTopology.TriangleStrip
                ? PrimitiveType.TriangleStrip : PrimitiveType.TriangleList;
            int primitiveCount = primitiveType == PrimitiveType.TriangleStrip
                ? Math.Max(IndicesIndex - 2, 0) : IndicesIndex / 3;
            Main.instance.GraphicsDevice.DrawIndexedPrimitives(primitiveType, 0, 0, VerticesIndex, 0, primitiveCount);
        }

        public static void CalculatePerspectiveMatrices(out Matrix viewMatrix, out Matrix projectionMatrix)
        {
            Vector2 zoom = Main.GameViewMatrix.Zoom;
            Matrix zoomScale = Matrix.CreateScale(zoom.X, zoom.Y, 1f);
            int w = Main.instance.GraphicsDevice.Viewport.Width;
            int h = Main.instance.GraphicsDevice.Viewport.Height;

            viewMatrix = Matrix.CreateLookAt(Vector3.Zero, Vector3.UnitZ, Vector3.Up);
            viewMatrix *= Matrix.CreateTranslation(0f, -h, 0f);
            viewMatrix *= Matrix.CreateRotationZ(MathHelper.Pi);
            if (Main.LocalPlayer.gravDir == -1f)
                viewMatrix *= Matrix.CreateScale(1f, -1f, 1f) * Matrix.CreateTranslation(0f, h, 0f);
            viewMatrix *= zoomScale;
            projectionMatrix = Matrix.CreateOrthographicOffCenter(0f, w * zoom.X, 0f, h * zoom.Y, 0f, 1f) * zoomScale;
        }

        // =====================================================================
        // POINT ASSIGNMENT — resamples input positions into MainPositions[]
        // =====================================================================

        private static bool AssignPoints(Vector2[] positions, GPUPrimitiveSettings settings, int pointsToCreate)
        {
            if (!settings.Smoothen)
            {
                PositionsIndex = 0;
                int validCount = 0;
                for (int i = 0; i < positions.Length; i++)
                {
                    if (positions[i] == Vector2.Zero) continue;
                    NonSmoothIndexScratch[validCount++] = i;
                }
                if (validCount <= 2) return false;

                int lastIndex = validCount - 1;
                float inversePointCount = 1f / (pointsToCreate - 1);
                float lastIndexF = lastIndex;

                for (int i = 0; i < pointsToCreate; i++)
                {
                    float cr = i * inversePointCount;
                    float scaledIdx = cr * lastIndexF;
                    int curIdx = (int)scaledIdx;
                    int nextIdx = Math.Min(curIdx + 1, lastIndex);
                    float localT = scaledIdx - curIdx;

                    Vector2 cur = positions[NonSmoothIndexScratch[curIdx]];
                    Vector2 next = positions[NonSmoothIndexScratch[nextIdx]];
                    Vector2 interp = Vector2.Lerp(cur, next, localT);
                    Vector2 finalPos = interp - Main.screenPosition;
                    if (settings.OffsetFunction != null)
                        finalPos += settings.OffsetFunction(cr, interp);
                    MainPositions[PositionsIndex++] = finalPos;
                }
                return true;
            }

            PositionsIndex = 0;
            List<Vector2> cp = ControlPointsCache;
            cp.Clear();
            for (int i = 0; i < positions.Length; i++)
            {
                if (positions[i] == Vector2.Zero) continue;
                float cr = i / (float)positions.Length;
                Vector2 offset = -Main.screenPosition;
                if (settings.OffsetFunction != null)
                    offset += settings.OffsetFunction(cr, positions[i]);
                cp.Add(positions[i] + offset);
            }

            int cpCount = cp.Count;
            if (cpCount <= 1) { cp.Clear(); return false; }

            int segCount = cpCount - 1;

            if (settings.SmoothingSegments > 0)
            {
                int segsPerEdge = Math.Max(1, settings.SmoothingSegments);
                var sType = settings.SmoothingType;
                for (int seg = 0; seg < segCount; seg++)
                {
                    Vector2 p0 = cp[Math.Max(seg - 1, 0)];
                    Vector2 p1 = cp[seg];
                    Vector2 p2 = cp[seg + 1];
                    Vector2 p3 = cp[Math.Min(seg + 2, cpCount - 1)];
                    for (int step = seg == 0 ? 0 : 1; step <= segsPerEdge; step++)
                    {
                        if (PositionsIndex >= MaxPositions - 1) { cp.Clear(); return true; }
                        float t = step / (float)segsPerEdge;
                        MainPositions[PositionsIndex++] = EvaluateCurve(sType, p0, p1, p2, p3, t, settings);
                    }
                }
                cp.Clear();
                return true;
            }

            PositionsIndex = 1;
            float cpCountM1 = cpCount - 1f;
            var legacyType = settings.SmoothingType;
            for (int j = 0; j < pointsToCreate; j++)
            {
                if (PositionsIndex >= MaxPositions - 1) break;
                float splineT = j / (float)pointsToCreate;
                float posOnCurve = splineT * cpCountM1;
                int localIdx = (int)posOnCurve;
                float localT = posOnCurve - localIdx;

                Vector2 p0 = cp[Math.Max(localIdx - 1, 0)];
                Vector2 p1 = cp[localIdx];
                Vector2 p2 = cp[Math.Min(localIdx + 1, cpCount - 1)];
                Vector2 p3 = cp[Math.Min(localIdx + 2, cpCount - 1)];
                MainPositions[PositionsIndex] = EvaluateCurve(legacyType, p0, p1, p2, p3, localT, settings);
                PositionsIndex++;
            }
            MainPositions[0] = cp[0];
            MainPositions[PositionsIndex] = cp[cpCount - 1];
            PositionsIndex++;
            cp.Clear();
            return true;
        }

        // =====================================================================
        // COMPLETION DATA — arc-length parameterization
        // =====================================================================

        private static void AssignCompletionData()
        {
            TotalTrailLength = 0f;
            if (PositionsIndex <= 0) return;
            MainCompletionRatios[0] = 0f;
            for (int i = 1; i < PositionsIndex; i++)
            {
                TotalTrailLength += Vector2.Distance(MainPositions[i], MainPositions[i - 1]);
                MainCompletionRatios[i] = TotalTrailLength;
            }
            if (TotalTrailLength > Epsilon)
            {
                float inv = 1f / TotalTrailLength;
                for (int i = 1; i < PositionsIndex; i++)
                    MainCompletionRatios[i] *= inv;
                MainCompletionRatios[PositionsIndex - 1] = 1f;
            }
            else
            {
                for (int i = 1; i < PositionsIndex; i++)
                    MainCompletionRatios[i] = 0f;
            }
        }

        // =====================================================================
        // VERTEX ASSIGNMENT — builds the triangle strip
        // =====================================================================

        private static void AssignVertices()
        {
            VerticesIndex = 0;
            StartCapCenterIndex = -1;
            EndCapCenterIndex = -1;
            ComputeFrameData();

            for (int i = 0; i < PositionsIndex; i++)
            {
                float cr = GetCompletionRatioForIndex(i);
                float w = Math.Max(MainSettings.WidthFunction(cr, MainPositions[i]), 0f);
                Color c = MainSettings.ColorFunction(cr, MainPositions[i]);
                float u = ComputeTextureCoord(i, cr);

                ComputeEdgePositions(i, w, out Vector2 left, out Vector2 right, out float effectiveHW);

                if (i == 0 && MainSettings.InitialVertexPositionsOverride.HasValue)
                {
                    var ov = MainSettings.InitialVertexPositionsOverride.Value;
                    if (ov.Item1 != Vector2.Zero && ov.Item2 != Vector2.Zero)
                    {
                        left = ov.Item1; right = ov.Item2;
                        effectiveHW = Math.Max(Vector2.Distance(left, right) * 0.5f, Epsilon);
                    }
                }

                effectiveHW = Math.Max(effectiveHW, Epsilon);
                Vector2 leftUV = new(u, 0.5f - effectiveHW * 0.5f);
                Vector2 rightUV = new(u, 0.5f + effectiveHW * 0.5f);
                MainVertices[VerticesIndex++] = new GPUPrimitiveVertex(left, c, leftUV, effectiveHW);
                MainVertices[VerticesIndex++] = new GPUPrimitiveVertex(right, c, rightUV, effectiveHW);
            }

            AddCaps();
        }

        private static void AddCaps()
        {
            if (MainSettings.CapStyle == GPUPrimitiveCapStyle.None || PositionsIndex <= 0) return;
            if (ActiveTopology == GPUPrimitiveTopology.TriangleStrip) return;
            StartCapCenterIndex = TryCreateCapVertex(0);
            if (PositionsIndex > 1)
                EndCapCenterIndex = TryCreateCapVertex(PositionsIndex - 1);
        }

        private static short TryCreateCapVertex(int posIdx)
        {
            if (VerticesIndex >= MaxVertices - 1) return -1;
            int leftVI = posIdx * 2;
            int rightVI = leftVI + 1;
            if (rightVI >= VerticesIndex) return -1;

            ref readonly GPUPrimitiveVertex lv = ref MainVertices[leftVI];
            ref readonly GPUPrimitiveVertex rv = ref MainVertices[rightVI];
            Vector2 centerPos = MainPositions[posIdx];
            Color centerColor = Color.Lerp(lv.Color, rv.Color, 0.5f);
            float centerHW = Math.Max(Math.Max(lv.TextureCoordinates.Z, rv.TextureCoordinates.Z), Epsilon);
            float centerU = (lv.TextureCoordinates.X + rv.TextureCoordinates.X) * 0.5f;
            Vector2 centerTC = new(centerU, 0.5f);
            short newVI = VerticesIndex;
            MainVertices[VerticesIndex++] = new GPUPrimitiveVertex(centerPos, centerColor, centerTC, centerHW);
            return newVI;
        }

        private static float GetCompletionRatioForIndex(int index)
        {
            if (PositionsIndex <= 0) return 0f;
            if (index <= 0) return MainCompletionRatios[0];
            if (index >= PositionsIndex) return MainCompletionRatios[PositionsIndex - 1];
            return MainCompletionRatios[index];
        }

        private static float ComputeTextureCoord(int index, float completionRatio)
        {
            float cr = MathHelper.Clamp(completionRatio, 0f, 1f);
            if (MainSettings.TextureCoordinateFunction != null)
                return MainSettings.TextureCoordinateFunction(cr);

            float cycleLen = MainSettings.TextureCycleLength;
            if (Math.Abs(cycleLen) <= Epsilon) cycleLen = 1f;

            return MainSettings.TextureCoordinateMode switch
            {
                GPUPrimitiveTextureMode.Distance => (cr * TotalTrailLength + MainSettings.TextureScrollOffset) / cycleLen,
                _ => cr * cycleLen + MainSettings.TextureScrollOffset,
            };
        }

        // =====================================================================
        // FRAME DATA — tangent and normal computation
        // =====================================================================

        private static void ComputeFrameData()
        {
            if (PositionsIndex <= 0) return;
            Vector2 fallbackT = Vector2.UnitX;

            for (int i = 0; i < PositionsIndex; i++)
            {
                Vector2 t = ComputeTangent(i, fallbackT);
                t = t.SafeNormalize(fallbackT.SafeNormalize(Vector2.UnitX));
                MainTangents[i] = t;
                fallbackT = t;
            }

            Vector2 prevNormal = Vector2.Zero;
            for (int i = 0; i < PositionsIndex; i++)
            {
                Vector2 tang = MainTangents[i];
                if (tang.LengthSquared() <= Epsilon) tang = fallbackT.SafeNormalize(Vector2.UnitX);

                Vector2 baseN = new(-tang.Y, tang.X);
                Vector2 normal;

                if (MainSettings.FrameTransportMode == GPUPrimitiveFrameTransport.ParallelTransport && i > 0 && prevNormal.LengthSquared() > Epsilon)
                {
                    Vector2 prevT = MainTangents[i - 1];
                    float cos = MathHelper.Clamp(Vector2.Dot(prevT, tang), -1f, 1f);
                    float sin = Cross(prevT, tang);
                    normal = new(cos * prevNormal.X - sin * prevNormal.Y, sin * prevNormal.X + cos * prevNormal.Y);
                }
                else
                    normal = baseN;

                if (normal.LengthSquared() <= Epsilon)
                    normal = prevNormal.LengthSquared() > Epsilon ? prevNormal : baseN;

                normal = normal.SafeNormalize(prevNormal.LengthSquared() > Epsilon ? prevNormal : Vector2.UnitY);
                MainNormals[i] = normal;
                prevNormal = normal;
            }
        }

        private static Vector2 ComputeTangent(int index, Vector2 fallback)
        {
            int last = PositionsIndex - 1;
            Vector2 tang;

            if (PositionsIndex <= 1) tang = fallback;
            else if (index <= 0) tang = MainPositions[1] - MainPositions[0];
            else if (index >= last) tang = MainPositions[last] - MainPositions[last - 1];
            else
            {
                Vector2 fwd = MainPositions[index + 1] - MainPositions[index];
                Vector2 bwd = MainPositions[index] - MainPositions[index - 1];
                tang = fwd + bwd;
                if (tang.LengthSquared() <= Epsilon)
                    tang = fwd.LengthSquared() >= bwd.LengthSquared() ? fwd : bwd;
            }

            if (tang.LengthSquared() <= Epsilon)
                tang = fallback.LengthSquared() > Epsilon ? fallback : Vector2.UnitX;
            return tang;
        }

        // =====================================================================
        // EDGE POSITIONS — computes left/right vertices from normals and width
        // =====================================================================

        private static void ComputeEdgePositions(int index, float halfWidth, out Vector2 left, out Vector2 right, out float effectiveHW)
        {
            Vector2 pos = MainPositions[index];
            if (halfWidth <= 0f)
            {
                left = right = pos;
                effectiveHW = Epsilon;
                return;
            }

            Vector2 defaultN = MainNormals[index];
            if (defaultN.LengthSquared() <= Epsilon) defaultN = Vector2.UnitY;

            if (MainSettings.JoinStyle == GPUPrimitiveJoinStyle.Flat || PositionsIndex <= 2 || index == 0 || index == PositionsIndex - 1)
            {
                Vector2 off = defaultN * halfWidth;
                left = pos - off; right = pos + off; effectiveHW = halfWidth;
                return;
            }

            Vector2 prevN = MainNormals[Math.Max(index - 1, 0)];
            if (prevN.LengthSquared() <= Epsilon) prevN = defaultN;
            Vector2 nextN = MainNormals[Math.Min(index + 1, PositionsIndex - 1)];
            if (nextN.LengthSquared() <= Epsilon) nextN = defaultN;

            switch (MainSettings.JoinStyle)
            {
                case GPUPrimitiveJoinStyle.Smooth:
                {
                    Vector2 avg = (prevN + defaultN + nextN) * (1f / 3f);
                    if (avg.LengthSquared() <= Epsilon) avg = defaultN;
                    Vector2 off = avg.SafeNormalize(defaultN) * halfWidth;
                    left = pos - off; right = pos + off; effectiveHW = halfWidth;
                    return;
                }
                case GPUPrimitiveJoinStyle.Miter:
                {
                    Vector2 pn = prevN.SafeNormalize(defaultN);
                    Vector2 nn = nextN.SafeNormalize(defaultN);
                    Vector2 miter = pn + nn;
                    if (miter.LengthSquared() <= Epsilon) miter = defaultN;
                    miter = miter.SafeNormalize(defaultN);
                    float denom = Vector2.Dot(miter, nn);
                    if (Math.Abs(denom) < Epsilon) denom = denom >= 0f ? Epsilon : -Epsilon;
                    float miterLen = halfWidth / denom;
                    float maxLen = halfWidth * MainSettings.JoinMiterLimit;
                    miterLen = MathHelper.Clamp(miterLen, -maxLen, maxLen);
                    Vector2 off = miter * miterLen;
                    left = pos - off; right = pos + off; effectiveHW = Math.Max(Math.Abs(miterLen), Epsilon);
                    return;
                }
                default:
                {
                    Vector2 off = defaultN * halfWidth;
                    left = pos - off; right = pos + off; effectiveHW = halfWidth;
                    return;
                }
            }
        }

        // =====================================================================
        // INDEX ASSIGNMENT
        // =====================================================================

        private static void AssignIndices()
        {
            IndicesIndex = 0;
            if (ActiveTopology == GPUPrimitiveTopology.TriangleStrip)
            {
                for (short i = 0; i < VerticesIndex && IndicesIndex < MaxIndices; i++)
                    MainIndices[IndicesIndex++] = i;
                return;
            }
            for (short i = 0; i < PositionsIndex - 2 && IndicesIndex + 5 < MaxIndices; i++)
            {
                short ci = (short)(i * 2);
                MainIndices[IndicesIndex++] = ci;
                MainIndices[IndicesIndex++] = (short)(ci + 1);
                MainIndices[IndicesIndex++] = (short)(ci + 2);
                MainIndices[IndicesIndex++] = (short)(ci + 2);
                MainIndices[IndicesIndex++] = (short)(ci + 1);
                MainIndices[IndicesIndex++] = (short)(ci + 3);
            }
            AppendCapTriangles();
        }

        private static void AppendCapTriangles()
        {
            if (MainSettings.CapStyle == GPUPrimitiveCapStyle.None || PositionsIndex <= 0) return;
            if (StartCapCenterIndex >= 0)
                AddTriangle(StartCapCenterIndex, 0, 1);
            if (EndCapCenterIndex >= 0)
            {
                short li = (short)((PositionsIndex - 1) * 2);
                short ri = (short)(li + 1);
                AddTriangle(li, ri, EndCapCenterIndex);
            }
        }

        private static void AddTriangle(short i0, short i1, short i2)
        {
            if (IndicesIndex + 2 >= MaxIndices) return;
            MainIndices[IndicesIndex++] = i0;
            MainIndices[IndicesIndex++] = i1;
            MainIndices[IndicesIndex++] = i2;
        }

        // =====================================================================
        // CURVE EVALUATION — spline interpolation for smooth trails
        // =====================================================================

        private static Vector2 EvaluateCurve(GPUPrimitiveSmoothingType type, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t, GPUPrimitiveSettings settings)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            switch (type)
            {
                case GPUPrimitiveSmoothingType.Linear:
                    return Vector2.Lerp(p1, p2, t);
                case GPUPrimitiveSmoothingType.Cardinal:
                {
                    float tension = MathHelper.Clamp(settings.SmoothingTension, -1f, 1f);
                    float scale = (1f - tension) * 0.5f;
                    Vector2 m0 = (p2 - p0) * scale;
                    Vector2 m1 = (p3 - p1) * scale;
                    return EvalHermite(p1, p2, m0, m1, t);
                }
                case GPUPrimitiveSmoothingType.Hermite:
                {
                    Vector2 m0 = 0.5f * (p2 - p0);
                    Vector2 m1 = 0.5f * (p3 - p1);
                    return EvalHermite(p1, p2, m0, m1, t);
                }
                case GPUPrimitiveSmoothingType.CubicBezier:
                {
                    Vector2 h1 = p1 + (p2 - p0) / 3f;
                    Vector2 h2 = p2 - (p3 - p1) / 3f;
                    return EvalBezier(p1, h1, h2, p2, t);
                }
                default:
                    return Vector2.CatmullRom(p0, p1, p2, p3, t);
            }
        }

        private static Vector2 EvalHermite(Vector2 s, Vector2 e, Vector2 ts, Vector2 te, float t)
        {
            float t2 = t * t, t3 = t2 * t;
            return (2f * t3 - 3f * t2 + 1f) * s + (t3 - 2f * t2 + t) * ts + (-2f * t3 + 3f * t2) * e + (t3 - t2) * te;
        }

        private static Vector2 EvalBezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            float u = 1f - t, u2 = u * u, u3 = u2 * u, t2 = t * t, t3 = t2 * t;
            return u3 * p0 + 3f * u2 * t * p1 + 3f * u * t2 * p2 + t3 * p3;
        }

        private static float Cross(Vector2 a, Vector2 b) => a.X * b.Y - a.Y * b.X;
    }
}
