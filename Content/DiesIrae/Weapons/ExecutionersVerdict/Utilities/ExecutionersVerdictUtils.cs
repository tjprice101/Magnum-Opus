using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

namespace MagnumOpus.Content.DiesIrae.Weapons.ExecutionersVerdict.Utilities
{
    /// <summary>
    /// Self-contained utility library for Executioner's Verdict.
    /// Heavy, dark execution palette — cooling steel after a judgment strike.
    /// </summary>
    public static class ExecutionersVerdictUtils
    {
        // ── EXECUTIONER'S PALETTE (6-color — dark blade to blood edge to ash) ──
        public static readonly Color[] BladePalette = new Color[]
        {
            new Color(15, 5, 10),        // Pianissimo - void black
            new Color(80, 0, 10),        // Piano - dark crimson
            new Color(139, 0, 0),        // Mezzo - blood red
            new Color(180, 30, 10),      // Forte - burning crimson
            new Color(220, 80, 30),      // Fortissimo - ember glow
            new Color(240, 210, 190),    // Sforzando - ash white (cooling steel)
        };

        public static readonly Color VoidBlack = new Color(15, 5, 10);
        public static readonly Color DarkCrimson = new Color(80, 0, 10);
        public static readonly Color BloodRed = new Color(139, 0, 0);
        public static readonly Color BurningCrimson = new Color(180, 30, 10);
        public static readonly Color EmberGlow = new Color(220, 80, 30);
        public static readonly Color AshWhite = new Color(240, 210, 190);
        public static readonly Color ExecutionGold = new Color(200, 160, 50);
        public static readonly Color CharcoalSmoke = new Color(30, 25, 30);

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

        /// <summary>
        /// Gets execution color intensity. The closer to death, the more intense the glow.
        /// Below 30%: amber glow. Below 15%: blood-red pulsing. At 0%: white flash.
        /// </summary>
        public static Color GetExecutionColor(float healthPercent)
        {
            if (healthPercent > 0.30f)
                return Color.Transparent;
            if (healthPercent <= 0.15f)
                return Color.Lerp(BloodRed, AshWhite, (float)Math.Sin(Main.GlobalTimeWrappedHourly * 8f) * 0.5f + 0.5f);
            float t = 1f - (healthPercent - 0.15f) / 0.15f; // 0 at 30%, 1 at 15%
            return Color.Lerp(ExecutionGold, BloodRed, t) * (0.5f + t * 0.5f);
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

        /// <summary>
        /// Heavy impact easing: slow windup, very fast acceleration, hard stop.
        /// </summary>
        public static float GuillotineDrop(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            if (t < 0.4f)
                return (float)Math.Pow(t / 0.4f, 3f) * 0.1f; // Very slow windup
            return 0.1f + 0.9f * (float)Math.Pow((t - 0.4f) / 0.6f, 0.4f); // Explosive acceleration
        }

        // ── CURVE SEGMENT ANIMATION ──

        public struct CurveSegment
        {
            public float StartAt;
            public float Duration;
            public Func<float, float> Easing;
            public float StartValue;
            public float EndValue;

            public CurveSegment(float start, float duration, float startVal, float endVal, Func<float, float> easing = null)
            {
                StartAt = start;
                Duration = duration;
                StartValue = startVal;
                EndValue = endVal;
                Easing = easing ?? (t => t);
            }
        }

        public static float PiecewiseAnimation(float progress, CurveSegment[] segments)
        {
            float result = 0f;
            for (int i = segments.Length - 1; i >= 0; i--)
            {
                var seg = segments[i];
                if (progress >= seg.StartAt)
                {
                    float localT = MathHelper.Clamp((progress - seg.StartAt) / seg.Duration, 0f, 1f);
                    float eased = seg.Easing(localT);
                    result = MathHelper.Lerp(seg.StartValue, seg.EndValue, eased);
                    break;
                }
            }
            return result;
        }

        // ── SPRITEBATCH HELPERS ──

        public static void BeginAdditive(SpriteBatch sb)
        {
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public static void BeginAlpha(SpriteBatch sb)
        {
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public static void ResetSpriteBatch(SpriteBatch sb)
        {
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        // ── TARGETING HELPERS ──

        public static NPC ClosestNPCAt(Vector2 pos, float maxDist, bool bossPriority = true)
        {
            NPC best = null;
            float bestDist = maxDist;

            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (!npc.CanBeChasedBy()) continue;
                float dist = Vector2.Distance(pos, npc.Center);
                if (dist >= bestDist) continue;

                if (bossPriority && npc.boss && (best == null || !best.boss))
                {
                    best = npc;
                    bestDist = dist;
                }
                else if (!bossPriority || best == null || !best.boss || npc.boss)
                {
                    best = npc;
                    bestDist = dist;
                }
            }
            return best;
        }
    }
}
