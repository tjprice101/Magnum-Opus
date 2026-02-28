using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.EternalMoon.Utilities
{
    /// <summary>
    /// Self-contained utility library for the Eternal Moon weapon system.
    /// Contains easing functions, piecewise animation, color helpers, and SpriteBatch extensions.
    /// Inspired by the Exoblade architecture but themed for Moonlight Sonata's lunar tidal identity.
    /// </summary>
    public static class EternalMoonUtils
    {
        // =====================================================================
        // MOONLIGHT SONATA PALETTE (Eternal Moon blade-specific)
        // =====================================================================

        /// <summary>Night void — Pianissimo</summary>
        public static readonly Color NightPurple = new(40, 10, 60);
        /// <summary>Deep indigo — Piano</summary>
        public static readonly Color DarkPurple = new(75, 0, 130);
        /// <summary>Violet body — Mezzo</summary>
        public static readonly Color Violet = new(138, 43, 226);
        /// <summary>Ice blue flare — Forte</summary>
        public static readonly Color IceBlue = new(135, 206, 250);
        /// <summary>Crescent glow — Fortissimo</summary>
        public static readonly Color CrescentGlow = new(170, 225, 255);
        /// <summary>Moonbeam white — Sforzando</summary>
        public static readonly Color MoonWhite = new(230, 235, 255);

        /// <summary>Extended palette: vivid blue for impacts</summary>
        public static readonly Color LunarShine = new(120, 190, 255);
        /// <summary>Extended palette: cool blue-white for intense centers</summary>
        public static readonly Color StarCore = new(210, 225, 255);

        /// <summary>The 6-color blade gradient mapped to musical dynamics (pianissimo → sforzando).</summary>
        public static readonly Color[] LunarPalette = new Color[]
        {
            NightPurple,   // [0] Pianissimo
            DarkPurple,    // [1] Piano
            Violet,        // [2] Mezzo
            IceBlue,       // [3] Forte
            CrescentGlow,  // [4] Fortissimo
            MoonWhite,     // [5] Sforzando
        };

        // =====================================================================
        // EASING FUNCTIONS
        // =====================================================================

        public delegate float EasingFunction(float amount, int degree);

        public static float LinearEasing(float amount, int degree) => amount;

        public static float SineInEasing(float amount, int degree) =>
            1f - (float)Math.Cos(amount * MathHelper.Pi / 2f);

        public static float SineOutEasing(float amount, int degree) =>
            (float)Math.Sin(amount * MathHelper.Pi / 2f);

        public static float SineInOutEasing(float amount, int degree) =>
            -((float)Math.Cos(amount * MathHelper.Pi) - 1) / 2f;

        public static float SineBumpEasing(float amount, int degree) =>
            (float)Math.Sin(amount * MathHelper.Pi);

        public static float PolyInEasing(float amount, int degree) =>
            (float)Math.Pow(amount, degree);

        public static float PolyOutEasing(float amount, int degree) =>
            1f - (float)Math.Pow(1f - amount, degree);

        public static float PolyInOutEasing(float amount, int degree) =>
            amount < 0.5f
                ? (float)Math.Pow(2, degree - 1) * (float)Math.Pow(amount, degree)
                : 1f - (float)Math.Pow(-2 * amount + 2, degree) / 2f;

        public static float ExpInEasing(float amount, int degree) =>
            amount == 0f ? 0f : (float)Math.Pow(2, 10f * amount - 10f);

        public static float ExpOutEasing(float amount, int degree) =>
            amount == 1f ? 1f : 1f - (float)Math.Pow(2, -10f * amount);

        public static float CircInEasing(float amount, int degree) =>
            1f - (float)Math.Sqrt(1 - Math.Pow(amount, 2f));

        public static float CircOutEasing(float amount, int degree) =>
            (float)Math.Sqrt(1 - Math.Pow(amount - 1f, 2f));

        // =====================================================================
        // CURVE SEGMENT & PIECEWISE ANIMATION
        // =====================================================================

        public struct CurveSegment
        {
            public EasingFunction easing;
            public float startingX;
            public float startingHeight;
            public float elevationShift;
            public int degree;

            public float EndingHeight => startingHeight + elevationShift;

            public CurveSegment(EasingFunction mode, float startX, float startHeight,
                float shift, int degree = 1)
            {
                easing = mode;
                startingX = startX;
                startingHeight = startHeight;
                elevationShift = shift;
                this.degree = degree;
            }
        }

        /// <summary>
        /// Evaluates a piecewise animation curve at the given progress (0→1).
        /// Each CurveSegment defines a portion of the timeline with its own easing.
        /// </summary>
        public static float PiecewiseAnimation(float progress, params CurveSegment[] segments)
        {
            if (segments.Length == 0) return 0f;
            if (segments[0].startingX != 0) segments[0].startingX = 0;
            progress = MathHelper.Clamp(progress, 0f, 1f);

            float ratio = 0f;
            for (int i = 0; i <= segments.Length - 1; i++)
            {
                CurveSegment segment = segments[i];
                float startPoint = segment.startingX;
                float endPoint = 1f;

                if (progress < segment.startingX) continue;
                if (i < segments.Length - 1)
                {
                    if (segments[i + 1].startingX <= progress) continue;
                    endPoint = segments[i + 1].startingX;
                }

                float segmentLength = endPoint - startPoint;
                float segmentProgress = (progress - segment.startingX) / segmentLength;
                ratio = segment.startingHeight;

                if (segment.easing != null)
                    ratio += segment.easing(segmentProgress, segment.degree) * segment.elevationShift;
                else
                    ratio += LinearEasing(segmentProgress, segment.degree) * segment.elevationShift;
                break;
            }
            return ratio;
        }

        // =====================================================================
        // COLOR HELPERS
        // =====================================================================

        /// <summary>
        /// Cycles through a color array based on a 0→1 interpolant.
        /// </summary>
        public static Color MulticolorLerp(float increment, params Color[] colors)
        {
            increment %= 0.999f;
            int count = colors.Length;
            int index = (int)(increment * count);
            float segmentProgress = increment * count % 1f;

            int nextIndex = (index + 1) % count;
            return Color.Lerp(colors[index], colors[nextIndex], segmentProgress);
        }

        /// <summary>
        /// Samples the 6-color lunar palette at position t (0→1).
        /// 0 = NightPurple (pianissimo), 1 = MoonWhite (sforzando).
        /// </summary>
        public static Color GetLunarGradient(float t)
        {
            return MulticolorLerp(MathHelper.Clamp(t, 0f, 0.999f), LunarPalette);
        }

        /// <summary>
        /// Returns a lunar palette color with optional white push for brilliance.
        /// </summary>
        public static Color GetLunarGradientWithWhitePush(float t, float whitePush = 0f)
        {
            Color c = GetLunarGradient(t);
            return Color.Lerp(c, MoonWhite, whitePush);
        }

        /// <summary>
        /// Returns the lunar phase progression as a display string.
        /// </summary>
        public static string GetLunarPhaseName(int phase)
        {
            return phase switch
            {
                0 => "New Moon",
                1 => "Waxing Crescent",
                2 => "Half Moon",
                3 => "Waning Gibbous",
                4 => "Full Moon",
                _ => "New Moon"
            };
        }

        /// <summary>
        /// Returns the intensity multiplier for the current lunar phase (0→4).
        /// </summary>
        public static float GetPhaseIntensity(int phase)
        {
            return phase switch
            {
                0 => 0.25f,
                1 => 0.45f,
                2 => 0.65f,
                3 => 0.85f,
                4 => 1.0f,
                _ => 0.25f
            };
        }

        // =====================================================================
        // SPRITEBATCH HELPERS
        // =====================================================================

        /// <summary>
        /// Transitions SpriteBatch into Immediate sort mode for shader application.
        /// </summary>
        public static void EnterShaderRegion(this SpriteBatch spriteBatch,
            BlendState blendState = null)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, blendState ?? BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Restores SpriteBatch to default deferred mode after shader region.
        /// </summary>
        public static void ExitShaderRegion(this SpriteBatch spriteBatch,
            BlendState blendState = null)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, blendState ?? BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Draws the item's glowmask in world using a single-frame texture.
        /// </summary>
        public static void DrawItemGlowmaskSingleFrame(this Item item, SpriteBatch spriteBatch,
            float rotation, Texture2D glowTexture)
        {
            Vector2 origin = glowTexture.Size() / 2f;
            spriteBatch.Draw(glowTexture, item.Center - Main.screenPosition,
                null, Color.White, rotation, origin, 1f, SpriteEffects.None, 0f);
        }

        // =====================================================================
        // ENTITY HELPERS
        // =====================================================================

        /// <summary>
        /// Returns a safe normalized direction from the entity to a destination.
        /// Falls back to the provided default if distance is negligible.
        /// </summary>
        public static Vector2 SafeDirectionTo(this Entity entity, Vector2 destination,
            Vector2? fallback = null)
        {
            Vector2 diff = destination - entity.Center;
            if (diff.LengthSquared() < 0.0001f)
                return fallback ?? Vector2.Zero;
            diff.Normalize();
            return diff;
        }

        /// <summary>
        /// Finds the closest NPC to a given origin within max distance.
        /// </summary>
        public static NPC ClosestNPCAt(this Vector2 origin, float maxDistance,
            bool ignoreTiles = false)
        {
            NPC closest = null;
            float bestDist = maxDistance;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || !npc.CanBeChasedBy()) continue;
                float dist = Vector2.Distance(origin, npc.Center);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    closest = npc;
                }
            }
            return closest;
        }

        /// <summary>
        /// Generates a random velocity with direction and speed variation.
        /// </summary>
        public static Vector2 RandomVelocity(float directionMult, float minSpeed, float maxSpeed)
        {
            Vector2 dir = Main.rand.NextVector2Unit();
            return dir * Main.rand.NextFloat(minSpeed, maxSpeed) * directionMult;
        }

        /// <summary>
        /// Performs direct lifesteal from an NPC hit.
        /// </summary>
        public static void DoLifestealDirect(this Player player, NPC target, int amount,
            float visualScale = 0.3f)
        {
            if (amount <= 0) return;
            player.HealEffect(amount);
            player.statLife = Math.Min(player.statLife + amount, player.statLifeMax2);
        }
    }
}
