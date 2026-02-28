using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

namespace MagnumOpus.Content.OdeToJoy.Weapons.TheGardenersFury.Utilities
{
    /// <summary>
    /// Self-contained utility library for The Gardener's Fury rapier.
    /// Provides palette colors, lerp helpers, additive blending,
    /// SpriteBatch state management, and NPC targeting — all scoped to this weapon.
    /// </summary>
    public static class GardenersUtils
    {
        // ── THE GARDENER'S FURY PALETTE (6-color gradient) ──
        public static readonly Color[] GardenPalette = new Color[]
        {
            new Color(20, 40, 15),       // Pianissimo  — LeafShadow
            new Color(50, 130, 40),      // Piano       — StemGreen
            new Color(200, 170, 50),     // Mezzo       — GoldenPetal
            new Color(230, 140, 140),    // Forte       — RoseBlush
            new Color(255, 210, 60),     // Fortissimo  — JubilantGold
            new Color(255, 250, 230),    // Sforzando   — SunlightWhite
        };

        public static readonly Color LeafShadow = new Color(20, 40, 15);
        public static readonly Color StemGreen = new Color(50, 130, 40);
        public static readonly Color GoldenPetal = new Color(200, 170, 50);
        public static readonly Color RoseBlush = new Color(230, 140, 140);
        public static readonly Color JubilantGold = new Color(255, 210, 60);
        public static readonly Color SunlightWhite = new Color(255, 250, 230);

        // ── COLOR HELPERS ──

        /// <summary>
        /// Smooth multi-stop lerp across the full 6-color garden palette.
        /// t = 0 → LeafShadow, t = 1 → SunlightWhite.
        /// </summary>
        public static Color PaletteLerp(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            float scaled = t * (GardenPalette.Length - 1);
            int lo = (int)scaled;
            int hi = Math.Min(lo + 1, GardenPalette.Length - 1);
            return Color.Lerp(GardenPalette[lo], GardenPalette[hi], scaled - lo);
        }

        /// <summary>
        /// Multi-stop lerp across an arbitrary color array.
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
        /// Returns a fully additive copy (A=0) at full opacity.
        /// </summary>
        public static Color Additive(Color c) => c with { A = 0 };

        /// <summary>
        /// Returns an additive copy (A=0) scaled by opacity.
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
