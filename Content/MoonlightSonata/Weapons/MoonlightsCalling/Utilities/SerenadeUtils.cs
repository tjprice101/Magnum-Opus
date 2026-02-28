using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.MoonlightsCalling.Utilities
{
    /// <summary>
    /// Self-contained utility library for Moonlight's Calling — "The Serenade".
    /// Prismatic palette, easing functions, beam helpers, and rendering utilities.
    /// </summary>
    public static class SerenadeUtils
    {
        // =================================================================
        // PRISMATIC PALETTE
        // =================================================================

        /// <summary>Dark purple base — Pianissimo (quiet).</summary>
        public static readonly Color DarkPurple = new(75, 0, 130);

        /// <summary>Prism violet — Piano.</summary>
        public static readonly Color PrismViolet = new(160, 80, 255);

        /// <summary>Violet body — Mezzo.</summary>
        public static readonly Color Violet = new(138, 43, 226);

        /// <summary>Refracted blue — Forte.</summary>
        public static readonly Color RefractedBlue = new(100, 200, 255);

        /// <summary>Ice blue shimmer — Fortissimo.</summary>
        public static readonly Color IceBlue = new(135, 206, 250);

        /// <summary>Moon white — Sforzando (accent flash).</summary>
        public static readonly Color MoonWhite = new(240, 235, 255);

        /// <summary>6-color beam palette array matching MoonlightSonataPalette.MoonlightsCallingBeam.</summary>
        public static readonly Color[] BeamPalette = new[]
        {
            DarkPurple, PrismViolet, Violet, RefractedBlue, IceBlue, MoonWhite
        };

        // =================================================================
        // SPECTRAL COLORS (for spectral child beams)
        // =================================================================

        /// <summary>Spectral Red — long wavelength.</summary>
        public static readonly Color SpectralRed = new(255, 80, 80);

        /// <summary>Spectral Orange.</summary>
        public static readonly Color SpectralOrange = new(255, 160, 50);

        /// <summary>Spectral Yellow.</summary>
        public static readonly Color SpectralYellow = new(255, 230, 60);

        /// <summary>Spectral Green.</summary>
        public static readonly Color SpectralGreen = new(80, 255, 120);

        /// <summary>Spectral Blue.</summary>
        public static readonly Color SpectralBlue = new(60, 140, 255);

        /// <summary>Spectral Indigo/Violet.</summary>
        public static readonly Color SpectralIndigo = new(160, 60, 255);

        /// <summary>7-color spectral array for child beam chromatic splitting.</summary>
        public static readonly Color[] SpectralColors = new[]
        {
            SpectralRed, SpectralOrange, SpectralYellow, SpectralGreen,
            SpectralBlue, SpectralIndigo, PrismViolet
        };

        // =================================================================
        // EASING FUNCTIONS
        // =================================================================

        /// <summary>Smooth sine ease-out (decelerating).</summary>
        public static float SineOut(float t) => MathF.Sin(t * MathHelper.PiOver2);

        /// <summary>Sine ease-in (accelerating).</summary>
        public static float SineIn(float t) => 1f - MathF.Cos(t * MathHelper.PiOver2);

        /// <summary>Polynomial ease-in (t^degree).</summary>
        public static float PolyIn(float t, int degree = 3) => MathF.Pow(t, degree);

        /// <summary>Polynomial ease-out (1 - (1-t)^degree).</summary>
        public static float PolyOut(float t, int degree = 3) => 1f - MathF.Pow(1f - t, degree);

        /// <summary>Exponential ease-out — sharp start, asymptotic end.</summary>
        public static float ExpoOut(float t) => t >= 1f ? 1f : 1f - MathF.Pow(2f, -10f * t);

        /// <summary>Smooth bump that rises and falls: sin(π * t).</summary>
        public static float SineBump(float t) => MathF.Sin(MathHelper.Pi * t);

        /// <summary>Quadratic bump peaking at 0.5.</summary>
        public static float QuadBump(float t) => t * (4f - t * 4f);

        // =================================================================
        // COLOR HELPERS
        // =================================================================

        /// <summary>Multistep color lerp through the beam palette.</summary>
        public static Color GetBeamGradient(float t)
        {
            return MulticolorLerp(t, BeamPalette);
        }

        /// <summary>Get a spectral color for a child beam index (0-6).</summary>
        public static Color GetSpectralColor(int index)
        {
            return SpectralColors[Math.Clamp(index, 0, SpectralColors.Length - 1)];
        }

        /// <summary>Multistep color interpolation through an array.</summary>
        public static Color MulticolorLerp(float t, Color[] colors)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            float segment = t * (colors.Length - 1);
            int low = (int)segment;
            int high = Math.Min(low + 1, colors.Length - 1);
            float lerp = segment - low;
            return Color.Lerp(colors[low], colors[high], lerp);
        }

        /// <summary>Get bounce intensity scaling (more bounces = stronger).</summary>
        public static float GetBounceIntensity(int bounceCount, int maxBounces = 5)
        {
            return 0.3f + 0.7f * (bounceCount / (float)maxBounces);
        }

        /// <summary>Get spectral spread factor based on bounce count.</summary>
        public static float GetSpectralSpread(int bounceCount, int maxBounces = 5)
        {
            return bounceCount / (float)maxBounces;
        }

        // =================================================================
        // SPRITEBATCH HELPERS
        // =================================================================

        /// <summary>Ends the current SpriteBatch and begins a new one with the specified blend mode.</summary>
        public static void EnterShaderRegion(SpriteBatch spriteBatch, BlendState blend = null)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, blend ?? BlendState.Additive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>Restores default SpriteBatch state after shader rendering.</summary>
        public static void ExitShaderRegion(SpriteBatch spriteBatch)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.PointClamp, DepthStencilState.None,
                RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        // =================================================================
        // ENTITY HELPERS
        // =================================================================

        /// <summary>Safe direction from source to target (fallback to Vector2.UnitY).</summary>
        public static Vector2 SafeDirectionTo(this Entity entity, Vector2 target)
        {
            Vector2 delta = target - entity.Center;
            return delta.LengthSquared() < 0.0001f ? Vector2.UnitY : Vector2.Normalize(delta);
        }

        /// <summary>Find closest NPC within range of a position.</summary>
        public static NPC ClosestNPCAt(Vector2 center, float maxRange, bool ignoreFriendly = true)
        {
            NPC closest = null;
            float bestDist = maxRange * maxRange;

            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (!npc.active || npc.dontTakeDamage) continue;
                if (ignoreFriendly && npc.friendly) continue;
                if (npc.immortal) continue;

                float d = Vector2.DistanceSquared(center, npc.Center);
                if (d < bestDist)
                {
                    bestDist = d;
                    closest = npc;
                }
            }
            return closest;
        }

        /// <summary>Perform life steal for the given amount, respecting vanilla cooldown/limits.</summary>
        public static void DoLifesteal(this Player player, NPC target, int healAmount)
        {
            if (healAmount <= 0 || player.moonLeech) return;
            player.HealEffect(healAmount);
            player.statLife = Math.Min(player.statLife + healAmount, player.statLifeMax2);
        }

        // =================================================================
        // MATH HELPERS
        // =================================================================

        /// <summary>Get a random direction vector constrained within a cone around the base direction.</summary>
        public static Vector2 RandomConeDirection(Vector2 baseDir, float halfAngle)
        {
            float angle = baseDir.ToRotation() + Main.rand.NextFloat(-halfAngle, halfAngle);
            return angle.ToRotationVector2();
        }

        /// <summary>Rotate a vector by the given angle in radians.</summary>
        public static Vector2 RotateBy(this Vector2 vec, float radians)
        {
            float cos = MathF.Cos(radians);
            float sin = MathF.Sin(radians);
            return new Vector2(vec.X * cos - vec.Y * sin, vec.X * sin + vec.Y * cos);
        }

        /// <summary>Linear interpolation clamping t to 0-1.</summary>
        public static float Lerp(float a, float b, float t) => a + (b - a) * MathHelper.Clamp(t, 0f, 1f);
    }
}
