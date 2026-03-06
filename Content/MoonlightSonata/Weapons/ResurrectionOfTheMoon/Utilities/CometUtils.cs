using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.ResurrectionOfTheMoon.Utilities
{
    /// <summary>
    /// Self-contained utility library for Resurrection of the Moon — "The Final Movement".
    /// Comet palette, easing functions, projectile helpers, and rendering utilities.
    /// </summary>
    public static class CometUtils
    {
        // =================================================================
        // COMET PALETTE (matches MoonlightSonataPalette.ResurrectionComet)
        // =================================================================

        /// <summary>Deep space violet — Pianissimo (quiet).</summary>
        public static readonly Color DeepSpaceViolet = new(50, 20, 100);

        /// <summary>Impact crater blue-violet — Piano.</summary>
        public static readonly Color ImpactCrater = new(100, 80, 200);

        /// <summary>Comet trail purple — Mezzo.</summary>
        public static readonly Color CometTrail = new(180, 120, 255);

        /// <summary>Lunar shine blue — Forte.</summary>
        public static readonly Color LunarShine = new(120, 190, 255);

        /// <summary>Comet core white-blue — Fortissimo.</summary>
        public static readonly Color CometCoreWhite = new(210, 225, 255);

        /// <summary>Frigid impact white — Sforzando (accent flash).</summary>
        public static readonly Color FrigidImpact = new(235, 240, 255);

        /// <summary>6-color comet palette array matching MoonlightSonataPalette.ResurrectionComet.</summary>
        public static readonly Color[] CometPalette = new[]
        {
            DeepSpaceViolet, ImpactCrater, CometTrail, LunarShine, CometCoreWhite, FrigidImpact
        };

        // =================================================================
        // CHAMBER-SPECIFIC COLORS
        // =================================================================

        /// <summary>Standard round glow color — bright crater violet-white.</summary>
        public static readonly Color StandardRoundColor = new(180, 160, 255);

        /// <summary>Comet Core glow color — burning gold-white.</summary>
        public static readonly Color CometCoreColor = new(255, 220, 160);

        /// <summary>Supernova shell glow color — deep violet-magenta.</summary>
        public static readonly Color SupernovaColor = new(200, 80, 255);

        /// <summary>Impact flash color — near-white with violet tint.</summary>
        public static readonly Color ImpactFlash = new(230, 220, 255);

        // =================================================================
        // EASING FUNCTIONS
        // =================================================================

        /// <summary>Smooth sine ease-out (decelerating).</summary>
        public static float SineOut(float t) => MathF.Sin(t * MathHelper.PiOver2);

        /// <summary>Sine ease-in (accelerating).</summary>
        public static float SineIn(float t) => 1f - MathF.Cos(t * MathHelper.PiOver2);

        /// <summary>Polynomial ease-in (quadratic by default).</summary>
        public static float PolyIn(float t, float power = 2f) => MathF.Pow(t, power);

        /// <summary>Polynomial ease-out.</summary>
        public static float PolyOut(float t, float power = 2f) => 1f - MathF.Pow(1f - t, power);

        /// <summary>Exponential ease-out (fast start, slow end).</summary>
        public static float ExpoOut(float t) => t >= 1f ? 1f : 1f - MathF.Pow(2f, -10f * t);

        /// <summary>Sine bump (0→1→0 over [0..1]).</summary>
        public static float SineBump(float t) => MathF.Sin(t * MathHelper.Pi);

        /// <summary>Quadratic bump (0→1→0).</summary>
        public static float QuadBump(float t) => 4f * t * (1f - t);

        // =================================================================
        // PALETTE HELPERS
        // =================================================================

        /// <summary>
        /// Multi-color lerp through the comet palette.
        /// Input t=[0..1] maps across all palette entries.
        /// </summary>
        public static Color MulticolorLerp(float t, Color[] colors)
        {
            t = MathHelper.Clamp(t, 0f, 0.999f);
            float scaled = t * (colors.Length - 1);
            int index = (int)scaled;
            float frac = scaled - index;
            return Color.Lerp(colors[index], colors[Math.Min(index + 1, colors.Length - 1)], frac);
        }

        /// <summary>Get a gradient color along the comet palette for a given intensity (0=cold, 1=hot).</summary>
        public static Color GetCometGradient(float intensity)
            => MulticolorLerp(intensity, CometPalette);

        /// <summary>Get bounce intensity multiplier (escalates with each ricochet).</summary>
        public static float GetBounceIntensity(int bounceCount, int maxBounces)
            => MathHelper.Clamp(bounceCount / (float)maxBounces, 0f, 1f);

        /// <summary>Get the comet phase for the shader (0=cold first shot, 1=white-hot max ricochets).</summary>
        public static float GetCometPhase(int bounceCount, int maxBounces)
            => MathHelper.Clamp(bounceCount / (float)maxBounces, 0f, 1f);

        // =================================================================
        // SPRITE BATCH HELPERS
        // =================================================================

        /// <summary>End default sprite batch and restart with shader-compatible settings.</summary>
        public static void EnterShaderRegion(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Immediate, MagnumBlendStates.ShaderAdditive,
                SamplerState.LinearWrap, DepthStencilState.None,
                Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>End shader region and return to default settings.</summary>
        public static void ExitShaderRegion(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        // =================================================================
        // ENTITY HELPERS
        // =================================================================

        /// <summary>Get the safe direction from one point to another.</summary>
        public static Vector2 SafeDirectionTo(this Entity entity, Vector2 target, Vector2 fallback = default)
        {
            Vector2 diff = target - entity.Center;
            float len = diff.Length();
            return len < 0.0001f ? (fallback == default ? Vector2.UnitY : fallback) : diff / len;
        }

        /// <summary>Find the closest targetable NPC within range of a position.</summary>
        public static int ClosestNPCAt(Vector2 position, float range, bool lineOfSight = false)
        {
            int closest = -1;
            float closestDist = range;
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (!npc.CanBeChasedBy()) continue;
                float dist = Vector2.Distance(position, npc.Center);
                if (dist < closestDist)
                {
                    if (lineOfSight && !Collision.CanHitLine(position, 1, 1, npc.position, npc.width, npc.height))
                        continue;
                    closestDist = dist;
                    closest = npc.whoAmI;
                }
            }
            return closest;
        }

        /// <summary>Generate a random direction within a cone.</summary>
        public static Vector2 RandomConeDirection(Vector2 baseDir, float halfAngle)
        {
            float angle = baseDir.ToRotation() + Main.rand.NextFloat(-halfAngle, halfAngle);
            return angle.ToRotationVector2() * baseDir.Length();
        }

        /// <summary>Rotate a vector by an angle in radians.</summary>
        public static Vector2 RotateBy(this Vector2 v, float radians)
            => Vector2.Transform(v, Matrix.CreateRotationZ(radians));
    }
}
