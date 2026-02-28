using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ElysianVerdict.Utilities
{
    /// <summary>
    /// Self-contained utility library for Elysian Verdict.
    /// Provides palette colors, lerp helpers, additive blending,
    /// SpriteBatch state management, and NPC targeting — all scoped to this weapon.
    /// </summary>
    public static class ElysianUtils
    {
        // ── ELYSIAN VERDICT PALETTE (6-color gradient) ──
        public static readonly Color[] VerdictPalette = new Color[]
        {
            new Color(20, 60, 20),       // Pianissimo - verdant deep
            new Color(50, 150, 40),      // Piano - vine green
            new Color(220, 190, 50),     // Mezzo - elysian gold
            new Color(220, 130, 140),    // Forte - rose judgment
            new Color(255, 220, 70),     // Fortissimo - golden verdict
            new Color(255, 250, 230),    // Sforzando - pure radiance
        };

        public static readonly Color VerdantDeep = new Color(20, 60, 20);
        public static readonly Color VineGreen = new Color(50, 150, 40);
        public static readonly Color ElysianGold = new Color(220, 190, 50);
        public static readonly Color RoseJudgment = new Color(220, 130, 140);
        public static readonly Color GoldenVerdict = new Color(255, 220, 70);
        public static readonly Color PureRadiance = new Color(255, 250, 230);

        // ── COLOR HELPERS ──

        /// <summary>
        /// Lerps across the 6-color VerdictPalette based on t (0..1).
        /// </summary>
        public static Color PaletteLerp(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            float scaled = t * (VerdictPalette.Length - 1);
            int lo = (int)scaled;
            int hi = Math.Min(lo + 1, VerdictPalette.Length - 1);
            return Color.Lerp(VerdictPalette[lo], VerdictPalette[hi], scaled - lo);
        }

        /// <summary>
        /// Lerps across an arbitrary color array.
        /// </summary>
        public static Color MulticolorLerp(float t, params Color[] colors)
        {
            t = MathHelper.Clamp(t, 0f, 0.999f);
            float scaled = t * (colors.Length - 1);
            int lo = (int)scaled;
            int hi = Math.Min(lo + 1, colors.Length - 1);
            return Color.Lerp(colors[lo], colors[hi], scaled - lo);
        }

        /// <summary>
        /// Returns the color with alpha = 0 (additive-ready).
        /// </summary>
        public static Color Additive(Color c) => c with { A = 0 };

        /// <summary>
        /// Returns the color with alpha = 0, multiplied by opacity.
        /// </summary>
        public static Color Additive(Color c, float opacity) => c with { A = 0 } * opacity;

        // ── SPRITEBATCH STATE MANAGEMENT ──

        public static void BeginAdditive(SpriteBatch sb)
        {
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public static void BeginDefault(SpriteBatch sb)
        {
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        // ── NPC TARGETING ──

        /// <summary>
        /// Returns the closest hostile NPC within the given range, or null if none found.
        /// </summary>
        public static NPC ClosestNPC(Vector2 position, float maxRange)
        {
            NPC closest = null;
            float closestDist = maxRange * maxRange;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.immortal)
                    continue;

                float dist = Vector2.DistanceSquared(position, npc.Center);
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
