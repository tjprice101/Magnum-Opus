using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

namespace MagnumOpus.Content.DiesIrae.Weapons.WrathsCleaver.Utilities
{
    /// <summary>
    /// Self-contained utility library for Wrath's Cleaver.
    /// Provides easing functions, CurveSegment animation, color helpers,
    /// and SpriteBatch state management — all scoped to this weapon.
    /// </summary>
    public static class WrathsCleaverUtils
    {
        // ── WRATH'S CLEAVER PALETTE (6-color swing gradient) ──
        public static readonly Color[] BladePalette = new Color[]
        {
            new Color(30, 10, 15),       // Pianissimo - deepest black-red
            new Color(130, 0, 0),        // Piano - blood red
            new Color(200, 30, 30),      // Mezzo - infernal red
            new Color(240, 60, 20),      // Forte - wrathful flame
            new Color(255, 180, 50),     // Fortissimo - hellfire gold
            new Color(255, 245, 230),    // Sforzando - wrath white
        };

        public static readonly Color BloodRed = new Color(130, 0, 0);
        public static readonly Color InfernalRed = new Color(200, 30, 30);
        public static readonly Color HellfireGold = new Color(255, 180, 50);
        public static readonly Color EmberOrange = new Color(255, 69, 0);
        public static readonly Color CharcoalBlack = new Color(25, 20, 25);
        public static readonly Color WrathWhite = new Color(255, 250, 240);

        // ── COLOR HELPERS ──

        public static Color PaletteLerp(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            float scaled = t * (BladePalette.Length - 1);
            int lo = (int)scaled;
            int hi = Math.Min(lo + 1, BladePalette.Length - 1);
            return Color.Lerp(BladePalette[lo], BladePalette[hi], scaled - lo);
        }

        public static Color MulticolorLerp(float t, params Color[] colors)
        {
            t = MathHelper.Clamp(t, 0f, 0.999f);
            float scaled = t * (colors.Length - 1);
            int lo = (int)scaled;
            int hi = Math.Min(lo + 1, colors.Length - 1);
            return Color.Lerp(colors[lo], colors[hi], scaled - lo);
        }

        public static Color Additive(Color c) => c with { A = 0 };
        public static Color Additive(Color c, float opacity) => c with { A = 0 } * opacity;

        // ── EASING FUNCTIONS ──

        public static float EaseInPoly(float t, float power = 2f) => (float)Math.Pow(MathHelper.Clamp(t, 0f, 1f), power);
        public static float EaseOutPoly(float t, float power = 2f) => 1f - (float)Math.Pow(1f - MathHelper.Clamp(t, 0f, 1f), power);
        public static float EaseInOutPoly(float t, float power = 2f)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            return t < 0.5f
                ? (float)Math.Pow(2f * t, power) / 2f
                : 1f - (float)Math.Pow(2f * (1f - t), power) / 2f;
        }

        public static float SmoothStep(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            return t * t * (3f - 2f * t);
        }

        // ── CURVE SEGMENT ANIMATION ──

        public struct CurveSegment
        {
            public float StartAt;
            public float Duration;
            public Func<float, float> Easing;
            public float StartValue;
            public float EndValue;

            public CurveSegment(float startAt, float duration, Func<float, float> easing, float startValue, float endValue)
            {
                StartAt = startAt;
                Duration = duration;
                Easing = easing;
                StartValue = startValue;
                EndValue = endValue;
            }
        }

        public static float PiecewiseAnimation(float progress, params CurveSegment[] segments)
        {
            progress = MathHelper.Clamp(progress, 0f, 1f);
            float result = segments.Length > 0 ? segments[0].StartValue : 0f;
            for (int i = 0; i < segments.Length; i++)
            {
                var seg = segments[i];
                if (progress >= seg.StartAt && progress < seg.StartAt + seg.Duration)
                {
                    float localT = (progress - seg.StartAt) / seg.Duration;
                    float eased = seg.Easing != null ? seg.Easing(localT) : localT;
                    return MathHelper.Lerp(seg.StartValue, seg.EndValue, eased);
                }
                if (progress >= seg.StartAt + seg.Duration)
                    result = seg.EndValue;
            }
            return result;
        }

        // ── SPRITEBATCH HELPERS ──

        public static void EnterShaderRegion(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public static void ExitShaderRegion(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public static void BeginAdditive(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        // ── ENTITY HELPERS ──

        public static NPC ClosestNPCAt(Vector2 position, float maxDistance, bool ignoreTiles = false)
        {
            NPC closest = null;
            float closestDist = maxDistance;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                float dist = Vector2.Distance(position, npc.Center);
                if (dist < closestDist)
                {
                    if (!ignoreTiles && !Collision.CanHitLine(position, 1, 1, npc.position, npc.width, npc.height))
                        continue;
                    closestDist = dist;
                    closest = npc;
                }
            }
            return closest;
        }
    }
}
