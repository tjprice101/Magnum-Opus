using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.StaffOfTheLunarPhases.Utilities
{
    /// <summary>
    /// Self-contained utility library for Staff of the Lunar Phases — "The Conductor's Baton".
    /// GoliathCosmic palette, easing functions, entity helpers, and rendering utilities.
    /// </summary>
    public static class GoliathUtils
    {
        // =================================================================
        // GOLIATH COSMIC PALETTE (matches MoonlightSonataPalette.GoliathCosmic)
        // =================================================================

        /// <summary>Pianissimo — cosmic void.</summary>
        public static readonly Color CosmicVoid = new(20, 8, 40);

        /// <summary>Piano — gravity well.</summary>
        public static readonly Color GravityWell = new(100, 60, 180);

        /// <summary>Mezzo — nebula purple.</summary>
        public static readonly Color NebulaPurple = new(150, 80, 220);

        /// <summary>Forte — energy tendril.</summary>
        public static readonly Color EnergyTendril = new(180, 140, 255);

        /// <summary>Fortissimo — ice blue brilliance.</summary>
        public static readonly Color IceBlueBrilliance = new(135, 206, 250);

        /// <summary>Sforzando — star core.</summary>
        public static readonly Color StarCore = new(210, 225, 255);

        /// <summary>6-color cosmic palette array.</summary>
        public static readonly Color[] CosmicPalette = new[]
        {
            CosmicVoid, GravityWell, NebulaPurple, EnergyTendril, IceBlueBrilliance, StarCore
        };

        // =================================================================
        // LUNAR PHASES CAST PALETTE (matches MoonlightSonataPalette.LunarPhasesCast)
        // =================================================================

        /// <summary>Pianissimo — new moon void.</summary>
        public static readonly Color NewMoonVoid = new(20, 8, 40);

        /// <summary>Piano — waxing crescent.</summary>
        public static readonly Color WaxingCrescent = new(75, 0, 130);

        /// <summary>Mezzo — first quarter violet.</summary>
        public static readonly Color FirstQuarterViolet = new(138, 43, 226);

        /// <summary>Forte — waxing gibbous lavender.</summary>
        public static readonly Color WaxingGibbous = new(180, 150, 255);

        /// <summary>Fortissimo — full moon ice blue.</summary>
        public static readonly Color FullMoonIceBlue = new(135, 206, 250);

        /// <summary>Sforzando — supermoon white.</summary>
        public static readonly Color SupermoonWhite = new(240, 235, 255);

        /// <summary>6-color cast palette array.</summary>
        public static readonly Color[] CastPalette = new[]
        {
            NewMoonVoid, WaxingCrescent, FirstQuarterViolet, WaxingGibbous, FullMoonIceBlue, SupermoonWhite
        };

        // =================================================================
        // SPECIAL COLORS
        // =================================================================

        /// <summary>Beam core color — bright ice-white with lavender tint.</summary>
        public static readonly Color BeamCore = new(220, 210, 255);

        /// <summary>Beam edge color — deep nebula purple.</summary>
        public static readonly Color BeamEdge = new(120, 60, 200);

        /// <summary>Impact flash — near-white with cosmic tint.</summary>
        public static readonly Color ImpactFlash = new(230, 225, 255);

        /// <summary>Rift ambient glow — dim gravity well.</summary>
        public static readonly Color RiftGlow = new(80, 40, 160);

        /// <summary>Conductor mode highlight — supermoon brilliance.</summary>
        public static readonly Color ConductorHighlight = new(200, 190, 255);

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
        /// Multi-color lerp through a palette.
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

        /// <summary>Get a gradient color along the cosmic palette (0=void, 1=star core).</summary>
        public static Color GetCosmicGradient(float intensity)
            => MulticolorLerp(intensity, CosmicPalette);

        /// <summary>Get a gradient color along the cast palette (0=new moon, 1=supermoon).</summary>
        public static Color GetCastGradient(float intensity)
            => MulticolorLerp(intensity, CastPalette);

        /// <summary>Get beam color blending core→edge based on beam position.</summary>
        public static Color GetBeamGradient(float t)
            => Color.Lerp(BeamCore, BeamEdge, t);

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

        /// <summary>Begin additive blend without shader (for bloom stacking).</summary>
        public static void BeginAdditive(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
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

        /// <summary>Find the closest targetable NPC within range of a position, excluding a specific NPC.</summary>
        public static int ClosestNPCAt(Vector2 position, float range, int excludeNPC = -1)
        {
            int closest = -1;
            float closestDist = range;
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (!npc.CanBeChasedBy()) continue;
                if (npc.whoAmI == excludeNPC) continue;
                float dist = Vector2.Distance(position, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = npc.whoAmI;
                }
            }
            return closest;
        }

        /// <summary>Find the closest targetable NPC near a position, excluding multiple NPCs.</summary>
        public static int ClosestNPCExcluding(Vector2 position, float range, int[] excludeNPCs)
        {
            int closest = -1;
            float closestDist = range;
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (!npc.CanBeChasedBy()) continue;
                bool excluded = false;
                if (excludeNPCs != null)
                {
                    for (int i = 0; i < excludeNPCs.Length; i++)
                    {
                        if (npc.whoAmI == excludeNPCs[i])
                        {
                            excluded = true;
                            break;
                        }
                    }
                }
                if (excluded) continue;
                float dist = Vector2.Distance(position, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = npc.whoAmI;
                }
            }
            return closest;
        }

        /// <summary>Find the closest targetable NPC near a cursor position for Conductor Mode.</summary>
        public static int ClosestNPCNearCursor(Vector2 cursorWorld, float range)
        {
            int closest = -1;
            float closestDist = range;
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (!npc.CanBeChasedBy()) continue;
                float dist = Vector2.Distance(cursorWorld, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = npc.whoAmI;
                }
            }
            return closest;
        }

        /// <summary>Rotate a vector by an angle in radians.</summary>
        public static Vector2 RotateBy(this Vector2 v, float radians)
            => Vector2.Transform(v, Matrix.CreateRotationZ(radians));
    }
}
