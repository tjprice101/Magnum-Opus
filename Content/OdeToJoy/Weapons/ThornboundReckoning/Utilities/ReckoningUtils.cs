using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ThornboundReckoning.Utilities
{
    /// <summary>
    /// Self-contained utility library for Thornbound Reckoning.
    /// Provides palette colors, lerp helpers, additive blending,
    /// SpriteBatch state management, and NPC targeting — all scoped to this weapon.
    /// </summary>
    public static class ReckoningUtils
    {
        // ── THORNBOUND RECKONING PALETTE (6-color gradient) ──
        public static readonly Color[] BladePalette = new Color[]
        {
            new Color(25, 60, 20),       // Pianissimo - deep thorn
            new Color(40, 120, 30),      // Piano - forest green
            new Color(180, 200, 50),     // Mezzo - verdant gold
            new Color(220, 150, 100),    // Forte - rose gold
            new Color(255, 200, 50),     // Fortissimo - jubilant gold
            new Color(255, 245, 220),    // Sforzando - white bloom
        };

        public static readonly Color DeepThorn = new Color(25, 60, 20);
        public static readonly Color ForestGreen = new Color(40, 120, 30);
        public static readonly Color VerdantGold = new Color(180, 200, 50);
        public static readonly Color RoseGold = new Color(220, 150, 100);
        public static readonly Color JubilantGold = new Color(255, 200, 50);
        public static readonly Color WhiteBloom = new Color(255, 245, 220);

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
