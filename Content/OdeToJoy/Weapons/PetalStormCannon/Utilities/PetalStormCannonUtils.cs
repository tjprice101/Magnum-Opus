using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using MagnumOpus.Content.OdeToJoy;

namespace MagnumOpus.Content.OdeToJoy.Weapons.PetalStormCannon.Utilities
{
    /// <summary>
    /// Self-contained utility library for Petal Storm Cannon.
    /// Includes easing functions, curve segments, palette helpers, and SpriteBatch extensions.
    /// Completely independent of any shared mod infrastructure.
    /// Palette: ranged petal explosions — dark rose through rose red and petal pink to bloom gold brilliance.
    /// </summary>
    public static class PetalStormCannonUtils
    {
        #region Color Palette — Petal Storm

        /// <summary>Core 6-color palette: dark rose → rose red → petal pink → bloom gold → cream → white.</summary>
        public static readonly Color[] WeaponPalette = new Color[]
        {
            new Color(130, 50, 70),            // Dark rose
            OdeToJoyPalette.RosePink,          // Rose red (230, 120, 150)
            OdeToJoyPalette.PetalPink,         // Petal pink (240, 170, 180)
            OdeToJoyPalette.GoldenPollen,      // Bloom gold (255, 210, 60)
            OdeToJoyPalette.WhiteBloom,        // Cream (255, 250, 235)
            new Color(255, 255, 255),          // White
        };

        /// <summary>Lore tooltip color — Ode to Joy warm gold.</summary>
        public static readonly Color LoreColor = new Color(255, 200, 50);

        /// <summary>Get a cycling petal-pink hue — warm rose drifting through petal pink.</summary>
        public static Color GetJoyCycle(float offset = 0f)
        {
            float hue = (Main.GameUpdateCount * 0.015f + offset) % 1f;
            hue = 0.93f + hue * 0.12f; // Rose-petal hue range 0.93-1.05 (wraps)
            if (hue > 1f) hue -= 1f;
            return Main.hslToRgb(hue, 0.75f, 0.70f);
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

        /// <summary>Get weapon gradient from the 6-color palette.</summary>
        public static Color GetWeaponGradient(float t)
            => MulticolorLerp(MathHelper.Clamp(t, 0f, 0.999f), WeaponPalette);

        /// <summary>Make a color additive-friendly (zero alpha for additive blending).</summary>
        public static Color Additive(Color c, float opacity = 1f)
            => new Color(c.R, c.G, c.B, 0) * opacity;

        #endregion

        #region Easing Functions

        public delegate float EasingFunction(float amount, int degree);

        public static float LinearEasing(float amount, int degree) => amount;
        public static float SineInEasing(float amount, int degree) => 1f - (float)Math.Cos(amount * MathHelper.Pi / 2f);
        public static float SineOutEasing(float amount, int degree) => (float)Math.Sin(amount * MathHelper.Pi / 2f);
        public static float SineInOutEasing(float amount, int degree) => -((float)Math.Cos(amount * MathHelper.Pi) - 1) / 2f;
        public static float SineBumpEasing(float amount, int degree) => (float)Math.Sin(amount * MathHelper.Pi);
        public static float PolyInEasing(float amount, int degree) => (float)Math.Pow(amount, degree);
        public static float PolyOutEasing(float amount, int degree) => 1f - (float)Math.Pow(1f - amount, degree);
        public static float PolyInOutEasing(float amount, int degree) => amount < 0.5f
            ? (float)Math.Pow(2, degree - 1) * (float)Math.Pow(amount, degree)
            : 1f - (float)Math.Pow(-2 * amount + 2, degree) / 2f;
        public static float ExpInEasing(float amount, int degree) => amount == 0f ? 0f : (float)Math.Pow(2, 10f * amount - 10f);
        public static float ExpOutEasing(float amount, int degree) => amount == 1f ? 1f : 1f - (float)Math.Pow(2, -10f * amount);
        public static float CircInEasing(float amount, int degree) => 1f - (float)Math.Sqrt(1 - Math.Pow(amount, 2f));
        public static float CircOutEasing(float amount, int degree) => (float)Math.Sqrt(1 - Math.Pow(amount - 1f, 2f));

        #endregion

        #region CurveSegment — Piecewise Animation

        public struct CurveSegment
        {
            public EasingFunction easing;
            public float startingX;
            public float startingHeight;
            public float elevationShift;
            public int degree;
            public float EndingHeight => startingHeight + elevationShift;

            public CurveSegment(EasingFunction mode, float startX, float startHeight, float shift, int degree = 1)
            {
                easing = mode; startingX = startX; startingHeight = startHeight;
                elevationShift = shift; this.degree = degree;
            }
        }

        public static float PiecewiseAnimation(float progress, params CurveSegment[] segments)
        {
            if (segments.Length == 0) return 0f;
            if (segments[0].startingX != 0) segments[0].startingX = 0;
            progress = MathHelper.Clamp(progress, 0f, 1f);
            float ratio = 0f;
            for (int i = 0; i <= segments.Length - 1; i++)
            {
                CurveSegment seg = segments[i];
                float endPoint = 1f;
                if (progress < seg.startingX) continue;
                if (i < segments.Length - 1)
                {
                    if (segments[i + 1].startingX <= progress) continue;
                    endPoint = segments[i + 1].startingX;
                }
                float len = endPoint - seg.startingX;
                float segProg = (progress - seg.startingX) / len;
                ratio = seg.startingHeight;
                ratio += (seg.easing ?? LinearEasing)(segProg, seg.degree) * seg.elevationShift;
                break;
            }
            return ratio;
        }

        #endregion

        #region SpriteBatch Helpers

        public static void EnterShaderRegion(this SpriteBatch sb, BlendState blend = null)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Immediate, blend ?? BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer,
                null, Main.GameViewMatrix.TransformationMatrix);
        }

        public static void ExitShaderRegion(this SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer,
                null, Main.GameViewMatrix.TransformationMatrix);
        }

        public static void BeginAdditive(this SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);
        }

        public static void BeginShaderAdditive(this SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None, Main.Rasterizer,
                null, Main.GameViewMatrix.TransformationMatrix);
        }

        public static void RestoreSpriteBatch(this SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer,
                null, Main.GameViewMatrix.TransformationMatrix);
        }

        #endregion

        #region Geometry Helpers

        public static Vector2 SafeDirectionTo(this Entity entity, Vector2 destination, Vector2? fallback = null)
            => (destination - entity.Center).SafeNormalize(fallback ?? Vector2.Zero);

        public static NPC ClosestNPCAt(this Vector2 origin, float maxDist, bool ignoreTiles = true)
        {
            NPC closest = null;
            float best = maxDist;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || !npc.CanBeChasedBy()) continue;
                float d = Vector2.Distance(origin, npc.Center);
                bool canHit = ignoreTiles || Collision.CanHit(origin, 1, 1, npc.Center, 1, 1);
                if (d < best && canHit) { best = d; closest = npc; }
            }
            return closest;
        }

        public static float AngleTowards(this Entity entity, Vector2 destination)
            => (destination - entity.Center).ToRotation();

        #endregion

        #region Theme Accents

        public static void DrawThemeAccents(SpriteBatch sb, Vector2 worldPos, float scale, float intensity = 1f)
        {
            try { OdeToJoyVFXLibrary.DrawThemeBlossomAccent(sb, worldPos, scale, intensity * 0.5f); } catch { }
            float rot = (float)Main.GameUpdateCount * 0.02f;
            try { OdeToJoyVFXLibrary.DrawThemeImpactRing(sb, worldPos, scale, intensity * 0.4f, rot); } catch { }
        }

        #endregion
    }
}
