using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using MagnumOpus.Content.SwanLake;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.CalloftheBlackSwan.Utilities
{
    /// <summary>
    /// Self-contained utility library for Call of the Black Swan.
    /// Includes easing functions, curve segments, palette helpers, and SpriteBatch extensions.
    /// Completely independent of any shared mod infrastructure.
    /// </summary>
    public static class BlackSwanUtils
    {
        #region Color Palette — Monochrome Duality

        /// <summary>Core 6-color palette: obsidian black → pure white.</summary>
        public static readonly Color[] DualityPalette = new Color[]
        {
            new Color(15, 15, 25),    // Void Black
            new Color(50, 50, 65),    // Obsidian Shadow
            new Color(120, 120, 140), // Twilight Silver
            new Color(180, 185, 200), // Pale Moonstone
            new Color(220, 225, 235), // Swan Silver
            new Color(245, 245, 255), // Pristine White
        };

        /// <summary>Empowered palette with prismatic edge.</summary>
        public static readonly Color[] EmpoweredPalette = new Color[]
        {
            new Color(40, 10, 60),    // Deep Violet
            new Color(80, 30, 120),   // Royal Purple
            new Color(160, 100, 220), // Amethyst Glow
            new Color(220, 180, 255), // Lavender Light
            new Color(255, 220, 240), // Rose Pearl
            new Color(255, 255, 255), // Pure Flash
        };

        /// <summary>Feather color — warm ivory.</summary>
        public static readonly Color FeatherWhite = new Color(248, 245, 255);

        /// <summary>Feather color — deep obsidian.</summary>
        public static readonly Color FeatherBlack = new Color(15, 15, 25);

        /// <summary>Lore tooltip color.</summary>
        public static readonly Color LoreColor = new Color(240, 240, 255);

        /// <summary>Get a cycling rainbow color.</summary>
        public static Color GetRainbow(float offset = 0f)
        {
            float hue = (Main.GameUpdateCount * 0.012f + offset) % 1f;
            return Main.hslToRgb(hue, 0.85f, 0.8f);
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

        /// <summary>Get duality gradient (black ↔ white with midpoint shimmer).</summary>
        public static Color GetDualityGradient(float t)
        {
            return MulticolorLerp(t, DualityPalette);
        }

        /// <summary>Make a color additive-friendly.</summary>
        public static Color Additive(Color c, float opacity = 1f)
        {
            return new Color(c.R, c.G, c.B, 0) * opacity;
        }

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
        /// Evaluate a piecewise animation curve at the given progress (0→1).
        /// </summary>
        public static float PiecewiseAnimation(float progress, CurveSegment[] segments)
        {
            progress = MathHelper.Clamp(progress, 0f, 1f);

            // Find which segment we're in
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

            // Calculate the end of this segment (start of next segment, or 1.0)
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
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>Begin additive + immediate for shader glow passes.</summary>
        public static void BeginShaderAdditive(this SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>Restore standard SpriteBatch state.</summary>
        public static void RestoreSpriteBatch(this SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        #endregion

        #region Geometry Helpers

        /// <summary>Safe direction calculation that won't return NaN.</summary>
        public static Vector2 SafeDirectionTo(this Vector2 from, Vector2 to, Vector2 fallback = default)
        {
            Vector2 diff = to - from;
            float length = diff.Length();
            if (length < 0.0001f) return fallback;
            return diff / length;
        }

        /// <summary>Find the closest NPC within range of a position.</summary>
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

        /// <summary>Angle-limited rotation toward a target angle.</summary>
        public static float AngleTowards(float currentAngle, float targetAngle, float maxTurn)
        {
            float diff = MathHelper.WrapAngle(targetAngle - currentAngle);
            return currentAngle + MathHelper.Clamp(diff, -maxTurn, maxTurn);
        }

        #endregion

        // ─────────── THEME TEXTURE ACCENTS ───────────

        public static void DrawThemeAccents(SpriteBatch sb, Vector2 worldPos, float scale, float intensity = 1f)
        {
            SwanLakeVFXLibrary.DrawThemeCrystalAccent(sb, worldPos, scale, intensity * 0.5f);
            float rot = (float)Main.GameUpdateCount * 0.02f;
            SwanLakeVFXLibrary.DrawThemeImpactRing(sb, worldPos, scale, intensity * 0.4f, rot);
        }
    }
}
