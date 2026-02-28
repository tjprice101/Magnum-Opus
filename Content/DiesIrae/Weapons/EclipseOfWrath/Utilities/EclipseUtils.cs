using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

namespace MagnumOpus.Content.DiesIrae.Weapons.EclipseOfWrath.Utilities
{
    /// <summary>
    /// Self-contained utility library for Eclipse of Wrath.
    /// Dark sun aesthetic — corona flares, burning eclipse shadows.
    /// </summary>
    public static class EclipseUtils
    {
        // ── ECLIPSE PALETTE (dark sun corona gradient) ──
        public static readonly Color[] CoronaPalette = new Color[]
        {
            new Color(20, 5, 5),         // Umbra - total eclipse darkness
            new Color(120, 10, 0),       // Inner corona - deep red
            new Color(200, 50, 15),      // Mid corona - burning orange-red
            new Color(255, 120, 30),     // Outer corona - solar flare orange
            new Color(255, 200, 80),     // Chromosphere - hot gold
            new Color(255, 255, 220),    // Photosphere - solar white
        };

        public static readonly Color Umbra = new Color(20, 5, 5);
        public static readonly Color InnerCorona = new Color(120, 10, 0);
        public static readonly Color MidCorona = new Color(200, 50, 15);
        public static readonly Color OuterCorona = new Color(255, 120, 30);
        public static readonly Color SolarGold = new Color(255, 200, 80);
        public static readonly Color SolarWhite = new Color(255, 255, 220);
        public static readonly Color EclipseBlood = new Color(139, 0, 0);
        public static readonly Color EclipseSmoke = new Color(35, 20, 25);

        public static Color CoronaLerp(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            float scaled = t * (CoronaPalette.Length - 1);
            int lo = (int)scaled;
            int hi = Math.Min(lo + 1, CoronaPalette.Length - 1);
            return Color.Lerp(CoronaPalette[lo], CoronaPalette[hi], scaled - lo);
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

        // ── EASING ──
        public static float EaseOutPoly(float t, float power = 2f) => 1f - (float)Math.Pow(1f - MathHelper.Clamp(t, 0f, 1f), power);
        public static float EaseInPoly(float t, float power = 2f) => (float)Math.Pow(MathHelper.Clamp(t, 0f, 1f), power);

        /// <summary>Solar pulse: oscillates between min and max intensity at given frequency.</summary>
        public static float SolarPulse(float time, float frequency = 3f, float min = 0.8f, float max = 1.2f)
        {
            float t = ((float)Math.Sin(time * frequency) + 1f) * 0.5f;
            return MathHelper.Lerp(min, max, t);
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

        // ── NPC TARGETING ──
        public static NPC ClosestNPCAt(Vector2 pos, float maxDist)
        {
            NPC best = null;
            float bestDist = maxDist;
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (!npc.CanBeChasedBy()) continue;
                float dist = Vector2.Distance(pos, npc.Center);
                if (dist < bestDist)
                {
                    best = npc;
                    bestDist = dist;
                }
            }
            return best;
        }
    }
}
