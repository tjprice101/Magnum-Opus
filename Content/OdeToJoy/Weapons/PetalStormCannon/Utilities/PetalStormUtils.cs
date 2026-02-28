using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

namespace MagnumOpus.Content.OdeToJoy.Weapons.PetalStormCannon.Utilities
{
    /// <summary>
    /// Self-contained utility library for the Petal Storm Cannon.
    /// Provides palette colors, lerp helpers, additive blending,
    /// SpriteBatch state management, and NPC targeting — all scoped to this weapon.
    /// </summary>
    public static class PetalStormUtils
    {
        // ── PETAL STORM CANNON PALETTE (6-color gradient) ──
        public static readonly Color[] Palette = new Color[]
        {
            new Color(30, 50, 30),       // Pianissimo - gunmetal green
            new Color(140, 100, 40),     // Piano - cannon bronze
            new Color(240, 180, 40),     // Mezzo - amber flame
            new Color(230, 100, 120),    // Forte - rose burst
            new Color(255, 230, 80),     // Fortissimo - golden explosion
            new Color(255, 255, 240),    // Sforzando - white flash
        };

        public static readonly Color GunmetalGreen = new Color(30, 50, 30);
        public static readonly Color CannonBronze = new Color(140, 100, 40);
        public static readonly Color AmberFlame = new Color(240, 180, 40);
        public static readonly Color RoseBurst = new Color(230, 100, 120);
        public static readonly Color GoldenExplosion = new Color(255, 230, 80);
        public static readonly Color WhiteFlash = new Color(255, 255, 240);

        // ── COLOR HELPERS ──

        /// <summary>
        /// Lerps across the 6-color palette. t=0 is GunmetalGreen, t=1 is WhiteFlash.
        /// </summary>
        public static Color PaletteLerp(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            float scaled = t * (Palette.Length - 1);
            int lo = (int)scaled;
            int hi = Math.Min(lo + 1, Palette.Length - 1);
            return Color.Lerp(Palette[lo], Palette[hi], scaled - lo);
        }

        /// <summary>
        /// General multi-color lerp across an arbitrary array of colors.
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
        /// Returns a copy of the color with Alpha = 0, for additive blending.
        /// </summary>
        public static Color Additive(Color c) => c with { A = 0 };

        /// <summary>
        /// Returns a copy of the color with Alpha = 0 and multiplied by opacity, for additive blending.
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
