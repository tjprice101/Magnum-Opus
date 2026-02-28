using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

namespace MagnumOpus.Content.OdeToJoy.Weapons.AnthemOfGlory.Utilities
{
    /// <summary>
    /// Self-contained utility library for Anthem of Glory.
    /// Provides palette colors, lerp helpers, additive blending,
    /// SpriteBatch state management, and NPC targeting — all scoped to this weapon.
    /// </summary>
    public static class AnthemUtils
    {
        // ── ANTHEM OF GLORY PALETTE (6-color gradient) ──
        public static readonly Color[] GloryPalette = new Color[]
        {
            new Color(60, 40, 10),       // Pianissimo - deep bronze
            new Color(200, 160, 40),     // Piano - rich gold
            new Color(255, 200, 50),     // Mezzo - brilliant amber
            new Color(255, 245, 200),    // Forte - glory white
            new Color(230, 150, 150),    // Fortissimo - rose tint
            new Color(200, 220, 255),    // Sforzando - lightning blue
        };

        public static readonly Color DeepBronze = new Color(60, 40, 10);
        public static readonly Color RichGold = new Color(200, 160, 40);
        public static readonly Color BrilliantAmber = new Color(255, 200, 50);
        public static readonly Color GloryWhite = new Color(255, 245, 200);
        public static readonly Color RoseTint = new Color(230, 150, 150);
        public static readonly Color LightningBlue = new Color(200, 220, 255);

        // ── COLOR HELPERS ──

        /// <summary>
        /// Lerps across the 6-color GloryPalette based on t (0..1).
        /// </summary>
        public static Color PaletteLerp(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            float scaled = t * (GloryPalette.Length - 1);
            int lo = (int)scaled;
            int hi = Math.Min(lo + 1, GloryPalette.Length - 1);
            return Color.Lerp(GloryPalette[lo], GloryPalette[hi], scaled - lo);
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
        public static NPC ClosestNPC(Vector2 position, float maxRange, int excludeWhoAmI = -1)
        {
            NPC closest = null;
            float closestDist = maxRange * maxRange;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.immortal)
                    continue;
                if (npc.whoAmI == excludeWhoAmI)
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
