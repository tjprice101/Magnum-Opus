using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using MagnumOpus.Content.LaCampanella;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.IgnitionOfTheBell.Utilities
{
    /// <summary>
    /// Self-contained utilities for IgnitionOfTheBell — the infernal lance.
    /// Deep crimson/magma palette, thrust-focused geometry, and directional VFX helpers.
    /// </summary>
    public static class IgnitionOfTheBellUtils
    {
        #region Color Palette — Piercing Flame

        /// <summary>
        /// Thrust lance palette — obsidian → deep red → blazing orange → white core.
        /// More crimson/magma-focused than DualFatedChime's bright orange/gold.
        /// </summary>
        public static readonly Color[] ThrustPalette = new Color[]
        {
            new Color(15, 5, 10),     // Obsidian Shadow
            new Color(100, 15, 0),    // Deep Maroon
            new Color(200, 40, 0),    // Crimson Blaze
            new Color(255, 120, 20),  // Magma Orange
            new Color(255, 200, 100), // Flame Core
            new Color(255, 240, 210), // White-Hot Tip
        };

        /// <summary>
        /// Cyclone palette — swirling shades from volcanic core to cherry burst.
        /// </summary>
        public static readonly Color[] CyclonePalette = new Color[]
        {
            new Color(30, 5, 0),      // Volcanic Shadow
            new Color(140, 20, 0),    // Deep Crimson
            new Color(220, 60, 10),   // Cherry Fire
            new Color(255, 140, 30),  // Cyclone Gold
            new Color(255, 210, 80),  // Bright Swirl
            new Color(255, 250, 200), // Flash Core
        };

        public static readonly Color LoreColor = new Color(255, 140, 40);

        #endregion

        #region Color Helpers

        public static Color MulticolorLerp(float t, params Color[] colors)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            int segments = colors.Length - 1;
            float scaledT = t * segments;
            int index = (int)scaledT;
            if (index >= segments) return colors[segments];
            float localT = scaledT - index;
            return Color.Lerp(colors[index], colors[index + 1], localT);
        }

        public static Color GetThrustGradient(float t) => MulticolorLerp(t, ThrustPalette);
        public static Color GetCycloneGradient(float t) => MulticolorLerp(t, CyclonePalette);

        public static Color Additive(Color c, float opacity = 1f)
        {
            return new Color(c.R, c.G, c.B, 0) * opacity;
        }

        public static Color GetMagmaFlicker(float offset = 0f)
        {
            float time = (float)Main.timeForVisualEffects * 0.05f + offset * 6.28f;
            float flicker = 0.5f + 0.5f * (float)Math.Sin(time * 3.7f + Math.Sin(time * 1.3f));
            return MulticolorLerp(flicker, ThrustPalette);
        }

        #endregion

        #region Easing Functions

        public static float LinearEasing(float t) => t;
        public static float SineInEasing(float t) => 1f - (float)Math.Cos(t * Math.PI / 2.0);
        public static float SineOutEasing(float t) => (float)Math.Sin(t * Math.PI / 2.0);
        public static float SineInOutEasing(float t) => -(float)(Math.Cos(Math.PI * t) - 1.0) / 2f;
        public static float SineBumpEasing(float t) => (float)Math.Sin(t * Math.PI);

        public static float PolyInEasing(float t) => t * t * t;
        public static float PolyOutEasing(float t) { float u = 1f - t; return 1f - u * u * u; }
        public static float PolyInOutEasing(float t) =>
            t < 0.5f ? 4f * t * t * t : 1f - (float)Math.Pow(-2.0 * t + 2.0, 3) / 2f;

        public static float ExpInEasing(float t) => t <= 0f ? 0f : (float)Math.Pow(2, 10 * t - 10);
        public static float ExpOutEasing(float t) => t >= 1f ? 1f : 1f - (float)Math.Pow(2, -10 * t);

        #endregion

        #region CurveSegment — Piecewise Animation (Thrust Extension Curves)

        public struct CurveSegment
        {
            public float StartProgress;
            public float EndProgress;
            public float StartValue;
            public float EndValue;
            public Func<float, float> Easing;

            public CurveSegment(float startProg, float endProg, float startVal, float endVal, Func<float, float> easing = null)
            {
                StartProgress = startProg;
                EndProgress = endProg;
                StartValue = startVal;
                EndValue = endVal;
                Easing = easing ?? LinearEasing;
            }
        }

        public static float PiecewiseAnimation(float progress, CurveSegment[] segments)
        {
            progress = MathHelper.Clamp(progress, 0f, 1f);
            for (int i = 0; i < segments.Length; i++)
            {
                var seg = segments[i];
                if (progress >= seg.StartProgress && progress <= seg.EndProgress)
                {
                    float segLength = seg.EndProgress - seg.StartProgress;
                    float localT = segLength > 0 ? (progress - seg.StartProgress) / segLength : 0f;
                    float easedT = seg.Easing(localT);
                    return MathHelper.Lerp(seg.StartValue, seg.EndValue, easedT);
                }
            }
            return segments.Length > 0 ? segments[segments.Length - 1].EndValue : 0f;
        }

        #endregion

        #region SpriteBatch Helpers

        public static void EnterShaderRegion(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public static void ExitShaderRegion(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public static void BeginAdditive(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public static void BeginShaderAdditive(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public static void RestoreSpriteBatch(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        #endregion

        #region Geometry Helpers

        public static Vector2 SafeDirectionTo(this Vector2 from, Vector2 to)
        {
            Vector2 diff = to - from;
            float len = diff.Length();
            return len < 0.0001f ? Vector2.Zero : diff / len;
        }

        public static NPC ClosestNPCAt(Vector2 position, float maxRange)
        {
            NPC best = null;
            float bestDist = maxRange;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                float dist = Vector2.Distance(position, npc.Center);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = npc;
                }
            }
            return best;
        }

        /// <summary>
        /// Angle-towards helper for thrust projectile homing.
        /// </summary>
        public static float AngleTowards(float current, float target, float maxDelta)
        {
            float diff = MathHelper.WrapAngle(target - current);
            if (Math.Abs(diff) <= maxDelta) return target;
            return current + Math.Sign(diff) * maxDelta;
        }

        #endregion

        // ─────────── THEME TEXTURE ACCENTS ───────────

        public static void DrawThemeAccents(SpriteBatch sb, Vector2 worldPos, float scale, float intensity = 1f)
        {
            LaCampanellaVFXLibrary.DrawThemeStarFlare(sb, worldPos, scale, intensity * 0.5f);
            float rot = (float)Main.GameUpdateCount * 0.02f;
            LaCampanellaVFXLibrary.DrawThemeImpactRing(sb, worldPos, scale, intensity * 0.4f, rot);
        }
    }
}
