using Microsoft.Xna.Framework;
using System;
using Terraria;

namespace MagnumOpus.Content.ClairDeLune.Weapons.Chronologicality.Utilities
{
    /// <summary>
    /// Static utility functions for Chronologicality's animation, easing, and VFX helpers.
    /// Follows the same pattern as EternalMoonUtils.
    /// </summary>
    public static class ChronologicalityUtils
    {
        // ═══════════════════════════════════════════
        //  EASING FUNCTIONS
        // ═══════════════════════════════════════════

        public static float SmoothStep(float t) => t * t * (3f - 2f * t);
        public static float SineOut(float t) => MathF.Sin(t * MathHelper.PiOver2);
        public static float SineIn(float t) => 1f - MathF.Cos(t * MathHelper.PiOver2);
        public static float PolyIn(float t, int degree = 3) => MathF.Pow(t, degree);
        public static float PolyOut(float t, int degree = 3) => 1f - MathF.Pow(1f - t, degree);

        // ═══════════════════════════════════════════
        //  COMBO PHASE PARAMETERS
        // ═══════════════════════════════════════════

        /// <summary>
        /// Returns (arcDegrees, swingDuration, bladeReach, damageMultiplier, screenShake) for each phase.
        /// </summary>
        public static (float arc, int duration, float reach, float dmgMult, float shake) GetPhaseParams(int phase)
        {
            return phase switch
            {
                0 => (270f, 36, 100f, 2.0f, 4f),  // Hour Hand — slow, heavy, wide
                1 => (180f, 24, 85f, 1.5f, 2f),    // Minute Hand — mid speed
                _ => (90f, 14, 75f, 0.8f, 0.5f),   // Second Hand — rapid flurry
            };
        }

        /// <summary>Number of Second Hand flurry strikes.</summary>
        public const int SecondHandStrikes = 3;

        // ═══════════════════════════════════════════
        //  COLOR HELPERS
        // ═══════════════════════════════════════════

        /// <summary>
        /// Returns the smear color set for a given combo phase.
        /// </summary>
        public static Color[] GetPhaseColors(int phase)
        {
            return phase switch
            {
                0 => new[] // Hour Hand: deep temporal, gold accents
                {
                    ClairDeLunePalette.NightMist,
                    ClairDeLunePalette.SoftBlue,
                    ClairDeLunePalette.ClockworkBrass,
                },
                1 => new[] // Minute Hand: balanced blue-pearl
                {
                    ClairDeLunePalette.MidnightBlue,
                    ClairDeLunePalette.PearlBlue,
                    ClairDeLunePalette.PearlWhite,
                },
                _ => new[] // Second Hand: bright, sharp
                {
                    ClairDeLunePalette.SoftBlue,
                    ClairDeLunePalette.PearlWhite,
                    ClairDeLunePalette.WhiteHot,
                },
            };
        }

        /// <summary>
        /// Returns the per-weapon 6-color swing palette for Chronologicality.
        /// </summary>
        public static Color[] SwingPalette => ClairDeLunePalette.ChronologicalityBlade;

        // ═══════════════════════════════════════════
        //  TARGETING
        // ═══════════════════════════════════════════

        /// <summary>
        /// Finds the closest targetable NPC within range of a given position.
        /// </summary>
        public static NPC FindClosestTarget(Vector2 position, float range = 800f)
        {
            NPC closest = null;
            float closestDist = range;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.immortal) continue;
                float dist = Vector2.Distance(position, npc.Center);
                if (dist < closestDist) { closestDist = dist; closest = npc; }
            }
            return closest;
        }

        // ═══════════════════════════════════════════
        //  ANIMATION MATH
        // ═══════════════════════════════════════════

        /// <summary>
        /// Computes the swing angle at a given progress for the specified phase.
        /// Uses a two-phase ease: wind-up (slow) → sweep (fast) → settle (decel).
        /// </summary>
        public static float GetSwingAngle(float progress, int phase, float startAngle, int direction)
        {
            var (arc, _, _, _, _) = GetPhaseParams(phase);
            float arcRadians = MathHelper.ToRadians(arc);

            // Three-segment piecewise animation matching clock-hand weight
            float eased;
            if (progress < 0.2f)
            {
                // Wind-up: slow start (sine-in)
                float t = progress / 0.2f;
                eased = SineIn(t) * 0.15f;
            }
            else if (progress < 0.8f)
            {
                // Main sweep: fast acceleration (polynomial)
                float t = (progress - 0.2f) / 0.6f;
                eased = 0.15f + PolyIn(t, phase == 0 ? 2 : 3) * 0.75f;
            }
            else
            {
                // Settle: deceleration overshoot
                float t = (progress - 0.8f) / 0.2f;
                eased = 0.9f + SineOut(t) * 0.1f;
            }

            return startAngle + arcRadians * eased * direction;
        }
    }
}
