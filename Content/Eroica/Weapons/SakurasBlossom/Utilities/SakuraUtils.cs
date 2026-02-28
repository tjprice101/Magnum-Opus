using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace MagnumOpus.Content.Eroica.Weapons.SakurasBlossom.Utilities
{
    public static class SakuraUtils
    {
        #region Sakura Palette — Eroica petal-to-crimson fire gradient

        public static readonly Color BlackBark = new Color(25, 12, 15);
        public static readonly Color DeepCrimson = new Color(140, 25, 35);
        public static readonly Color SakuraPink = new Color(255, 150, 180);
        public static readonly Color PetalBlush = new Color(255, 190, 210);
        public static readonly Color CrimsonFlame = new Color(220, 50, 50);
        public static readonly Color GoldenPollen = new Color(255, 210, 80);
        public static readonly Color SpringWhite = new Color(255, 245, 240);
        public static readonly Color HotCore = new Color(255, 235, 200);

        public static readonly Color[] SakuraPalette = new Color[]
        {
            BlackBark, DeepCrimson, CrimsonFlame, SakuraPink, PetalBlush, GoldenPollen, HotCore, SpringWhite
        };

        #endregion

        #region Easing Functions

        public delegate float EasingFunction(float amount, int degree);

        public static float LinearEasing(float amount, int degree) => amount;
        public static float SineInEasing(float amount, int degree) => 1f - (float)Math.Cos(amount * MathHelper.Pi / 2f);
        public static float SineOutEasing(float amount, int degree) => (float)Math.Sin(amount * MathHelper.Pi / 2f);
        public static float SineInOutEasing(float amount, int degree) => -((float)Math.Cos(amount * MathHelper.Pi) - 1) / 2f;
        public static float SineBumpEasing(float amount, int degree) => (float)Math.Sin(amount * MathHelper.Pi);
        public static float PolyInEasing(float amount, int degree) => (float)Math.Pow(amount, degree);
        public static float PolyOutEasing(float amount, int degree) => 1f - (float)Math.Pow(1f - amount, degree);
        public static float PolyInOutEasing(float amount, int degree) => amount < 0.5f ? (float)Math.Pow(2, degree - 1) * (float)Math.Pow(amount, degree) : 1f - (float)Math.Pow(-2 * amount + 2, degree) / 2f;
        public static float ExpInEasing(float amount, int degree) => amount == 0f ? 0f : (float)Math.Pow(2, 10f * amount - 10f);
        public static float ExpOutEasing(float amount, int degree) => amount == 1f ? 1f : 1f - (float)Math.Pow(2, -10f * amount);
        public static float CircInEasing(float amount, int degree) => 1f - (float)Math.Sqrt(1 - Math.Pow(amount, 2f));
        public static float CircOutEasing(float amount, int degree) => (float)Math.Sqrt(1 - Math.Pow(amount - 1f, 2f));

        #endregion

        #region CurveSegment + PiecewiseAnimation

        public struct CurveSegment
        {
            public EasingFunction Easing;
            public float StartX;
            public float StartY;
            public float Lift;
            public int Power;

            public CurveSegment(EasingFunction easing, float startX, float startY, float lift, int power = 1)
            {
                Easing = easing;
                StartX = startX;
                StartY = startY;
                Lift = lift;
                Power = power;
            }
        }

        public static float PiecewiseAnimation(float progress, params CurveSegment[] segments)
        {
            progress = MathHelper.Clamp(progress, 0f, 1f);
            for (int i = segments.Length - 1; i >= 0; i--)
            {
                if (progress >= segments[i].StartX)
                {
                    float segStart = segments[i].StartX;
                    float segEnd = (i < segments.Length - 1) ? segments[i + 1].StartX : 1f;
                    float segLength = segEnd - segStart;
                    if (segLength <= 0f) segLength = 0.001f;
                    float localProgress = (progress - segStart) / segLength;
                    return segments[i].StartY + segments[i].Lift * segments[i].Easing(localProgress, segments[i].Power);
                }
            }
            return segments[0].StartY;
        }

        #endregion

        #region Color Helpers

        public static Color MulticolorLerp(float increment, params Color[] colors)
        {
            increment = MathHelper.Clamp(increment, 0f, 0.999f);
            float scaledIncrement = increment * (colors.Length - 1);
            int startIndex = (int)scaledIncrement;
            float localLerp = scaledIncrement - startIndex;
            return Color.Lerp(colors[startIndex], colors[Math.Min(startIndex + 1, colors.Length - 1)], localLerp);
        }

        public static Color GetPetalGradient(float t)
        {
            return MulticolorLerp(t, PetalBlush, SakuraPink, CrimsonFlame, DeepCrimson);
        }

        public static Color GetSakuraFireGradient(float t)
        {
            return MulticolorLerp(t, DeepCrimson, CrimsonFlame, SakuraPink, GoldenPollen, HotCore);
        }

        public static Color GetBlossomGradient(float t, float whitePush = 0f)
        {
            Color base_ = MulticolorLerp(t, SakuraPalette);
            return Color.Lerp(base_, SpringWhite, whitePush);
        }

        #endregion

        #region SpriteBatch Extensions

        public static void EnterShaderRegion(this SpriteBatch spriteBatch, BlendState blendState = null, Effect effect = null, Matrix? matrix = null)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, blendState ?? BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer,
                effect, matrix ?? Main.GameViewMatrix.TransformationMatrix);
        }

        public static void ExitShaderRegion(this SpriteBatch spriteBatch, Matrix? matrix = null)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer,
                null, matrix ?? Main.GameViewMatrix.TransformationMatrix);
        }

        #endregion

        #region Entity Helpers

        public static Vector2 SafeDirectionTo(this Entity entity, Vector2 destination, Vector2? fallback = null)
        {
            Vector2 diff = destination - entity.Center;
            return diff.SafeNormalize(fallback ?? Vector2.Zero);
        }

        public static NPC ClosestNPCAt(this Vector2 origin, float maxDistance, bool bossPriority = false)
        {
            NPC closest = null;
            float closestDist = maxDistance;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || !npc.CanBeChasedBy()) continue;
                float dist = Vector2.Distance(origin, npc.Center);
                if (dist < closestDist)
                {
                    if (bossPriority && npc.boss) { closest = npc; closestDist = dist; }
                    else if (!bossPriority || closest == null || !closest.boss) { closest = npc; closestDist = dist; }
                }
            }
            return closest;
        }

        public static void DoLifestealDirect(this Player player, NPC target, int amount)
        {
            if (amount <= 0 || player.moonLeech) return;
            player.statLife += amount;
            player.HealEffect(amount);
            if (player.statLife > player.statLifeMax2) player.statLife = player.statLifeMax2;
        }

        #endregion
    }
}
