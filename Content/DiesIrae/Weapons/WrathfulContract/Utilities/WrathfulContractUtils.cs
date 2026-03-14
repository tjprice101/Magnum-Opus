using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using MagnumOpus.Content.DiesIrae;

namespace MagnumOpus.Content.DiesIrae.Weapons.WrathfulContract.Utilities
{
    /// <summary>
    /// Self-contained utility class for Wrathful Contract.
    /// Palette, easings, animation curves, SpriteBatch helpers, geometry helpers.
    /// Charred bargain, demonic pact — every summon burns the contract deeper.
    /// </summary>
    public static class WrathfulContractUtils
    {
        #region Color Palette

        /// <summary>Wrathful Contract palette — charred bargain, demonic pact.</summary>
        public static readonly Color[] WeaponPalette = new Color[]
        {
            DiesIraePalette.CharredBlack,     // [0] Pianissimo — charred contract shadow
            DiesIraePalette.SmolderingEmber,  // [1] Piano — smoldering ember pact
            DiesIraePalette.InfernalRed,      // [2] Mezzo — infernal contract body
            DiesIraePalette.WrathfulFlame,    // [3] Forte — wrathful flame summoning
            DiesIraePalette.HellfireGold,     // [4] Fortissimo — hellfire gold binding
            DiesIraePalette.InfernalWhite,    // [5] Sforzando — contract fulfillment flash
        };

        /// <summary>Lore text color for ModifyTooltips.</summary>
        public static readonly Color LoreColor = DiesIraePalette.LoreText;

        /// <summary>Cycling wrath color — red hue range oscillation for shimmer effects.</summary>
        public static Color GetWrathCycle(float offset = 0f)
        {
            float hue = (Main.GameUpdateCount * 0.015f + offset) % 1f;
            hue = hue * 0.08f;
            return Main.hslToRgb(hue, 0.95f, 0.50f);
        }

        /// <summary>Smoothly interpolate through a color array.</summary>
        public static Color MulticolorLerp(float t, params Color[] colors)
        {
            t = MathHelper.Clamp(t, 0f, 0.999f);
            int count = colors.Length;
            float scaled = t * (count - 1);
            int index = (int)scaled;
            float frac = scaled - index;
            if (index >= count - 1) return colors[count - 1];
            return Color.Lerp(colors[index], colors[index + 1], frac);
        }

        /// <summary>Gets a color along the weapon gradient (0=charred shadow, 1=infernal white).</summary>
        public static Color GetWeaponGradient(float t) => MulticolorLerp(t, WeaponPalette);

        /// <summary>Make a color additive-friendly (A=0) with opacity.</summary>
        public static Color Additive(Color c, float opacity = 1f)
            => new Color(c.R, c.G, c.B, 0) * opacity;

        #endregion

        #region Easing Functions

        public delegate float EasingFunction(float t, int degree);

        public static float LinearEasing(float t, int degree) => t;
        public static float SineInEasing(float t, int degree) => 1f - (float)Math.Cos(t * MathHelper.PiOver2);
        public static float SineOutEasing(float t, int degree) => (float)Math.Sin(t * MathHelper.PiOver2);
        public static float SineInOutEasing(float t, int degree) => -(float)(Math.Cos(Math.PI * t) - 1) / 2f;
        public static float SineBumpEasing(float t, int degree) => (float)Math.Sin(t * Math.PI);

        public static float PolyInEasing(float t, int degree)
        {
            return (float)Math.Pow(t, degree);
        }

        public static float PolyOutEasing(float t, int degree)
        {
            return 1f - (float)Math.Pow(1f - t, degree);
        }

        public static float PolyInOutEasing(float t, int degree)
        {
            if (t < 0.5f)
                return (float)Math.Pow(2f * t, degree) / 2f;
            return 1f - (float)Math.Pow(-2f * t + 2f, degree) / 2f;
        }

        public static float ExpInEasing(float t, int degree) =>
            t == 0f ? 0f : (float)Math.Pow(2f, 10f * t - 10f);

        public static float ExpOutEasing(float t, int degree) =>
            t >= 1f ? 1f : 1f - (float)Math.Pow(2f, -10f * t);

        public static float CircInEasing(float t, int degree) =>
            1f - (float)Math.Sqrt(1f - t * t);

        public static float CircOutEasing(float t, int degree) =>
            (float)Math.Sqrt(1f - (t - 1f) * (t - 1f));

        #endregion

        #region CurveSegment — Piecewise Animation

        public struct CurveSegment
        {
            public EasingFunction Easing;
            public float StartX;
            public float StartHeight;
            public float ElevationShift;
            public int Degree;

            public CurveSegment(EasingFunction easing, float startX, float startHeight, float elevationShift, int degree = 2)
            {
                Easing = easing;
                StartX = startX;
                StartHeight = startHeight;
                ElevationShift = elevationShift;
                Degree = degree;
            }
        }

        public static float PiecewiseAnimation(float progress, CurveSegment[] segments)
        {
            progress = MathHelper.Clamp(progress, 0f, 1f);

            int segIdx = 0;
            for (int i = segments.Length - 1; i >= 0; i--)
            {
                if (progress >= segments[i].StartX)
                {
                    segIdx = i;
                    break;
                }
            }

            CurveSegment seg = segments[segIdx];
            float segEnd = (segIdx < segments.Length - 1) ? segments[segIdx + 1].StartX : 1f;
            float segDuration = segEnd - seg.StartX;

            if (segDuration <= 0f)
                return seg.StartHeight;

            float localProgress = MathHelper.Clamp((progress - seg.StartX) / segDuration, 0f, 1f);
            float easedProgress = seg.Easing(localProgress, seg.Degree);

            return seg.StartHeight + easedProgress * seg.ElevationShift;
        }

        #endregion

        #region SpriteBatch Helpers

        public static void EnterShaderRegion(this SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public static void ExitShaderRegion(this SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public static void BeginAdditive(this SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public static void BeginShaderAdditive(this SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Immediate, MagnumBlendStates.ShaderAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public static void RestoreSpriteBatch(this SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        #endregion

        #region Geometry Helpers

        public static Vector2 SafeDirectionTo(this Vector2 from, Vector2 to, Vector2 fallback = default)
        {
            Vector2 diff = to - from;
            float length = diff.Length();
            if (length < 0.0001f) return fallback;
            return diff / length;
        }

        public static NPC ClosestNPCAt(Vector2 position, float maxRange, bool requireLineOfSight = false)
        {
            NPC closest = null;
            float closestDist = maxRange;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.immortal)
                    continue;

                float dist = Vector2.Distance(position, npc.Center);
                if (dist < closestDist)
                {
                    if (requireLineOfSight && !Collision.CanHitLine(position, 1, 1, npc.position, npc.width, npc.height))
                        continue;
                    closestDist = dist;
                    closest = npc;
                }
            }

            return closest;
        }

        public static float AngleTowards(float currentAngle, float targetAngle, float maxTurn)
        {
            float diff = MathHelper.WrapAngle(targetAngle - currentAngle);
            return currentAngle + MathHelper.Clamp(diff, -maxTurn, maxTurn);
        }

        #endregion

        public static void DrawThemeAccents(SpriteBatch sb, Vector2 worldPos, float scale, float intensity = 1f)
        {
            try { DiesIraeVFXLibrary.DrawThemeImpactRing(sb, worldPos, scale, intensity * 0.4f, (float)Main.GameUpdateCount * 0.02f); }
            catch { }
        }
    }
}
