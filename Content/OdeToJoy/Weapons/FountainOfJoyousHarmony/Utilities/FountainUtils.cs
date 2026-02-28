using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

namespace MagnumOpus.Content.OdeToJoy.Weapons.FountainOfJoyousHarmony.Utilities
{
    /// <summary>
    /// Self-contained utility library for the Fountain of Joyous Harmony summon weapon.
    /// Provides palette colors, lerp helpers, additive blending,
    /// SpriteBatch state management, and NPC targeting — all scoped to this weapon.
    /// </summary>
    public static class FountainUtils
    {
        // ── FOUNTAIN PALETTE (6-color gradient) ──
        // Ode to Joy: deep water, aqua glow, golden spray, rose splash, healing green, fountain white
        public static readonly Color[] FountainPalette = new Color[]
        {
            new Color(20, 40, 80),       // Pianissimo  - DeepWater (deep pool base)
            new Color(60, 160, 200),     // Piano       - AquaGlow (shimmering water glow)
            new Color(240, 200, 60),     // Mezzo       - GoldenSpray (radiant golden spray)
            new Color(230, 140, 150),    // Forte       - RoseSplash (rose petal splash)
            new Color(80, 220, 80),      // Fortissimo  - HealingGreen (verdant healing pulse)
            new Color(240, 250, 255),    // Sforzando   - FountainWhite (crystal white mist)
        };

        public static readonly Color DeepWater = new Color(20, 40, 80);
        public static readonly Color AquaGlow = new Color(60, 160, 200);
        public static readonly Color GoldenSpray = new Color(240, 200, 60);
        public static readonly Color RoseSplash = new Color(230, 140, 150);
        public static readonly Color HealingGreen = new Color(80, 220, 80);
        public static readonly Color FountainWhite = new Color(240, 250, 255);

        // ── COLOR HELPERS ──

        /// <summary>
        /// Lerps across the 6-color FountainPalette. t in [0,1].
        /// </summary>
        public static Color PaletteLerp(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            float scaled = t * (FountainPalette.Length - 1);
            int lo = (int)scaled;
            int hi = Math.Min(lo + 1, FountainPalette.Length - 1);
            return Color.Lerp(FountainPalette[lo], FountainPalette[hi], scaled - lo);
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
