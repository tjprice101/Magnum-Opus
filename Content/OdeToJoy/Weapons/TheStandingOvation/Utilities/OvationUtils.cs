using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

namespace MagnumOpus.Content.OdeToJoy.Weapons.TheStandingOvation.Utilities
{
    /// <summary>
    /// Self-contained utility library for The Standing Ovation summon weapon.
    /// Provides palette colors, lerp helpers, additive blending,
    /// SpriteBatch state management, and NPC targeting — all scoped to this weapon.
    /// </summary>
    public static class OvationUtils
    {
        // ── THE STANDING OVATION PALETTE (6-color gradient) ──
        // Ode to Joy: warm gold, radiant amber, jubilant light, rose pink, verdant green
        public static readonly Color[] StagePalette = new Color[]
        {
            new Color(80, 20, 20),       // Pianissimo  - CurtainRed (deep stage curtain)
            new Color(200, 160, 40),     // Piano       - StageGold (warm amber stage light)
            new Color(255, 220, 70),     // Mezzo       - SpotlightGold (bright jubilant spotlight)
            new Color(230, 150, 160),    // Forte       - RoseApplause (rose pink applause glow)
            new Color(255, 250, 235),    // Fortissimo  - JoyfulWhite (near-white radiance)
            new Color(80, 200, 60),      // Sforzando   - EncoreGreen (verdant encore burst)
        };

        public static readonly Color CurtainRed = new Color(80, 20, 20);
        public static readonly Color StageGold = new Color(200, 160, 40);
        public static readonly Color SpotlightGold = new Color(255, 220, 70);
        public static readonly Color RoseApplause = new Color(230, 150, 160);
        public static readonly Color JoyfulWhite = new Color(255, 250, 235);
        public static readonly Color EncoreGreen = new Color(80, 200, 60);

        // ── COLOR HELPERS ──

        /// <summary>
        /// Lerps across the 6-color StagePalette. t in [0,1].
        /// </summary>
        public static Color PaletteLerp(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            float scaled = t * (StagePalette.Length - 1);
            int lo = (int)scaled;
            int hi = Math.Min(lo + 1, StagePalette.Length - 1);
            return Color.Lerp(StagePalette[lo], StagePalette[hi], scaled - lo);
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
