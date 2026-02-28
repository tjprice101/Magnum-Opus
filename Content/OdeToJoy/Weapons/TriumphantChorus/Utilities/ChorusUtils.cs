using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

namespace MagnumOpus.Content.OdeToJoy.Weapons.TriumphantChorus.Utilities
{
    /// <summary>
    /// Self-contained utility library for the Triumphant Chorus summon weapon.
    /// Provides palette colors, lerp helpers, additive blending,
    /// SpriteBatch state management, and NPC targeting — all scoped to this weapon.
    /// </summary>
    public static class ChorusUtils
    {
        // ── CHORUS PALETTE (6-color gradient) ──
        // Ode to Joy: warm gold, radiant amber, jubilant light, rose pink, verdant green
        public static readonly Color[] ChorusPalette = new Color[]
        {
            new Color(50, 30, 10),       // Pianissimo  - ChoirDeep (deep warm shadow)
            new Color(190, 150, 40),     // Piano       - HarmonyGold (rich amber gold)
            new Color(255, 210, 50),     // Mezzo       - TriumphGold (brilliant triumphant gold)
            new Color(230, 140, 150),    // Forte       - CrescendoRose (jubilant rose pink)
            new Color(255, 250, 230),    // Fortissimo  - FinaleWhite (radiant jubilant light)
            new Color(100, 210, 70),     // Sforzando   - JubilantGreen (verdant celebration)
        };

        public static readonly Color ChoirDeep = new Color(50, 30, 10);
        public static readonly Color HarmonyGold = new Color(190, 150, 40);
        public static readonly Color TriumphGold = new Color(255, 210, 50);
        public static readonly Color CrescendoRose = new Color(230, 140, 150);
        public static readonly Color FinaleWhite = new Color(255, 250, 230);
        public static readonly Color JubilantGreen = new Color(100, 210, 70);

        // ── COLOR HELPERS ──

        /// <summary>
        /// Lerps across the 6-color ChorusPalette. t in [0,1].
        /// </summary>
        public static Color PaletteLerp(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            float scaled = t * (ChorusPalette.Length - 1);
            int lo = (int)scaled;
            int hi = Math.Min(lo + 1, ChorusPalette.Length - 1);
            return Color.Lerp(ChorusPalette[lo], ChorusPalette[hi], scaled - lo);
        }

        /// <summary>
        /// General-purpose multi-color lerp.
        /// </summary>
        public static Color MulticolorLerp(float t, params Color[] colors)
        {
            t = MathHelper.Clamp(t, 0f, 0.999f);
            float scaled = t * (colors.Length - 1);
            int lo = (int)scaled;
            int hi = Math.Min(lo + 1, colors.Length - 1);
            return Color.Lerp(colors[lo], colors[hi], scaled - lo);
        }

        /// <summary>Sets alpha to 0 for proper additive blending.</summary>
        public static Color Additive(Color c) => c with { A = 0 };

        /// <summary>Sets alpha to 0 and applies opacity multiplier.</summary>
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
