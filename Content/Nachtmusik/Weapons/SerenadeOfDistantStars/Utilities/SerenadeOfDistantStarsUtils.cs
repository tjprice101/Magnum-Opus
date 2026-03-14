using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace MagnumOpus.Content.Nachtmusik.Weapons.SerenadeOfDistantStars.Utilities
{
    /// <summary>
    /// Self-contained utility library for Serenade of Distant Stars.
    /// Includes easing functions, curve segments, palette helpers, and SpriteBatch extensions.
    /// </summary>
    public static class SerenadeOfDistantStarsUtils
    {
        #region Color Palette — Serenade of Distant Stars

        /// <summary>Core 6-color palette: midnight base → warm starlight.</summary>
        public static readonly Color[] WeaponPalette = NachtmusikPalette.SerenadeOfDistantStarsShot;

        /// <summary>Lore tooltip color.</summary>
        public static readonly Color LoreColor = NachtmusikPalette.LoreText;

        /// <summary>Smoothly interpolate through a color array.</summary>
        public static Color MulticolorLerp(float t, params Color[] colors)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            float scaled = t * (colors.Length - 1);
            int lo = (int)scaled;
            int hi = Math.Min(lo + 1, colors.Length - 1);
            return Color.Lerp(colors[lo], colors[hi], scaled - lo);
        }

        /// <summary>Get weapon gradient through the palette.</summary>
        public static Color GetWeaponGradient(float t) => MulticolorLerp(t, WeaponPalette);

        /// <summary>Make a color additive-friendly.</summary>
        public static Color Additive(Color c, float opacity = 1f) => (c with { A = 0 }) * opacity;

        #endregion

        #region Easing Functions

        public delegate float EasingFunction(float t, int degree);

        public static float LinearEasing(float t, int degree) => t;
        public static float SineInEasing(float t, int degree) => 1f - MathF.Cos(t * MathHelper.PiOver2);
        public static float SineOutEasing(float t, int degree) => MathF.Sin(t * MathHelper.PiOver2);
        public static float SineInOutEasing(float t, int degree) => -(MathF.Cos(MathHelper.Pi * t) - 1f) / 2f;
        public static float SineBumpEasing(float t, int degree) => MathF.Sin(MathHelper.Pi * t);

        public static float PolyInEasing(float t, int degree) => MathF.Pow(t, degree);
        public static float PolyOutEasing(float t, int degree) => 1f - MathF.Pow(1f - t, degree);
        public static float PolyInOutEasing(float t, int degree) =>
            t < 0.5f ? MathF.Pow(2f, degree - 1) * MathF.Pow(t, degree) : 1f - MathF.Pow(-2f * t + 2f, degree) / 2f;

        public static float ExpInEasing(float t, int degree) => t <= 0f ? 0f : MathF.Pow(2f, 10f * t - 10f);
        public static float ExpOutEasing(float t, int degree) => t >= 1f ? 1f : 1f - MathF.Pow(2f, -10f * t);
        public static float CircInEasing(float t, int degree) => 1f - MathF.Sqrt(1f - t * t);
        public static float CircOutEasing(float t, int degree) { float u = t - 1f; return MathF.Sqrt(1f - u * u); }

        #endregion

        #region CurveSegment — Piecewise Animation

        /// <summary>
        /// A segment of a piecewise animation curve, inspired by Calamity's CurveSegment.
        /// </summary>
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

        /// <summary>
        /// Evaluate a piecewise animation curve at the given progress (0-1).
        /// </summary>
        public static float PiecewiseAnimation(float progress, CurveSegment[] segments)
        {
            progress = MathHelper.Clamp(progress, 0f, 1f);
            int active = 0;
            for (int i = segments.Length - 1; i >= 0; i--)
                if (progress >= segments[i].StartX) { active = i; break; }

            var seg = segments[active];
            float nextX = active < segments.Length - 1 ? segments[active + 1].StartX : 1f;
            float segLen = nextX - seg.StartX;
            float local = segLen > 0f ? (progress - seg.StartX) / segLen : 1f;
            return seg.StartHeight + seg.ElevationShift * seg.Easing(local, seg.Degree);
        }

        #endregion

        #region SpriteBatch Helpers

        /// <summary>Enter immediate-mode for shader rendering.</summary>
        public static void EnterShaderRegion(this SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>Exit immediate-mode back to deferred.</summary>
        public static void ExitShaderRegion(this SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>Begin additive blending for glow effects.</summary>
        public static void BeginAdditive(this SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>Begin additive + immediate for shader glow passes.</summary>
        public static void BeginShaderAdditive(this SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Immediate, MagnumBlendStates.ShaderAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>Restore standard SpriteBatch state.</summary>
        public static void RestoreSpriteBatch(this SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        #endregion

        #region Geometry Helpers

        /// <summary>Safe direction calculation that won't return NaN.</summary>
        public static Vector2 SafeDirectionTo(Vector2 from, Vector2 to, Vector2 fallback = default)
        {
            Vector2 diff = to - from;
            float len = diff.Length();
            return len < 0.0001f ? (fallback == default ? Vector2.UnitY : fallback) : diff / len;
        }

        /// <summary>Find the closest NPC within range of a position.</summary>
        public static NPC ClosestNPCAt(Vector2 pos, float range, bool requireLoS = false)
        {
            NPC best = null;
            float bestDist = range * range;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC n = Main.npc[i];
                if (!n.active || n.friendly || n.dontTakeDamage) continue;
                float d = Vector2.DistanceSquared(pos, n.Center);
                if (d < bestDist && (!requireLoS || Collision.CanHitLine(pos, 1, 1, n.position, n.width, n.height)))
                { best = n; bestDist = d; }
            }
            return best;
        }

        /// <summary>Angle-limited rotation toward a target angle.</summary>
        public static float AngleTowards(float current, float target, float maxDelta)
        {
            float diff = MathHelper.WrapAngle(target - current);
            return current + MathHelper.Clamp(diff, -maxDelta, maxDelta);
        }

        #endregion

        #region Theme Accents

        /// <summary>Draw Nachtmusik theme accents at a world position.</summary>
        public static void DrawThemeAccents(SpriteBatch sb, Vector2 worldPos, float scale, float intensity)
        {
            try { NachtmusikVFXLibrary.DrawThemeStarAccent(sb, worldPos, scale, intensity * 0.5f); } catch { }
            float rot = (float)Main.GameUpdateCount * 0.02f;
            try { NachtmusikVFXLibrary.DrawThemeImpactRing(sb, worldPos, scale, intensity * 0.4f, rot); } catch { }
        }

        #endregion
    }
}
