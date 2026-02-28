using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

namespace MagnumOpus.Content.OdeToJoy.Weapons.RoseThornChainsaw.Utilities
{
    /// <summary>
    /// Self-contained utility library for Rose Thorn Chainsaw.
    /// Palette: thorny greens, venomous violets, rose-gold sparks.
    /// </summary>
    public static class RoseThornChainsawUtils
    {
        // ── PALETTE ──
        public static readonly Color[] BladePalette = new Color[]
        {
            new Color(15, 40, 20),       // Deep thorn shadow
            new Color(34, 100, 34),      // Forest green
            new Color(76, 175, 80),      // Verdant green
            new Color(180, 80, 160),     // Venom violet
            new Color(255, 182, 193),    // Rose pink
            new Color(255, 215, 0),      // Golden pollen
        };

        public static readonly Color ThornGreen = new Color(34, 100, 34);
        public static readonly Color VenomViolet = new Color(180, 80, 160);
        public static readonly Color RosePink = new Color(255, 182, 193);
        public static readonly Color GoldenPollen = new Color(255, 215, 0);
        public static readonly Color VerdantGreen = new Color(76, 175, 80);
        public static readonly Color DeepThorn = new Color(15, 40, 20);
        public static readonly Color WhiteBloom = new Color(255, 255, 255);
        public static readonly Color SunlightYellow = new Color(255, 250, 205);
        public static readonly Color LoreColor = new Color(255, 200, 50);

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

        public static Color GetChainColor(float t) =>
            MulticolorLerp(t, ThornGreen, VenomViolet, RosePink, GoldenPollen);

        public static Color Additive(Color c) => c with { A = 0 };
        public static Color Additive(Color c, float opacity) => c with { A = 0 } * opacity;

        // ── EASING ──

        public static float EaseOutPoly(float t, float power = 2f) =>
            1f - (float)Math.Pow(1f - MathHelper.Clamp(t, 0f, 1f), power);

        public static float EaseInPoly(float t, float power = 2f) =>
            (float)Math.Pow(MathHelper.Clamp(t, 0f, 1f), power);

        // ── SPRITEBATCH HELPERS ──

        public static void BeginAdditive(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public static void BeginDefault(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        // ── ENTITY HELPERS ──

        public static NPC ClosestNPC(Vector2 position, float maxDist, int[] exclude = null)
        {
            NPC closest = null;
            float closestDist = maxDist;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                if (exclude != null && Array.IndexOf(exclude, i) >= 0) continue;
                float dist = Vector2.Distance(position, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = npc;
                }
            }
            return closest;
        }
    }
}
