using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MagnumOpus.Common;
using MagnumOpus.Content.ClairDeLune;
using Terraria;
using Terraria.ID;

namespace MagnumOpus.Content.ClairDeLune.Weapons.CogAndHammer.Utilities
{
    /// <summary>
    /// Self-contained utility library for the Cog and Hammer weapon system.
    /// Heavy brass palette  Eclockwork bombs across moonlit sky, explosive mechanical power.
    /// Contains easing functions, piecewise animation, color helpers, and SpriteBatch extensions.
    /// </summary>
    public static class CogAndHammerUtils
    {
        // ══════╁EWEAPON PALETTE ══════╁E

        /// <summary>
        /// Cog and Hammer launch gradient  Eclockwork bombs with brass, gold, and pearl blue.
        /// NightMist -> ClockworkBrass -> MoonbeamGold -> PearlBlue -> PearlWhite -> WhiteHot.
        /// </summary>
        public static readonly Color[] WeaponPalette = new Color[]
        {
            ClairDeLunePalette.NightMist,
            ClairDeLunePalette.ClockworkBrass,
            ClairDeLunePalette.MoonbeamGold,
            ClairDeLunePalette.PearlBlue,
            ClairDeLunePalette.PearlWhite,
            ClairDeLunePalette.WhiteHot,
        };

        public static readonly Color LoreColor = ClairDeLunePalette.LoreText;

        // ══════╁ECOLOR HELPERS ══════╁E

        public static Color MulticolorLerp(float t, params Color[] colors)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            float scaled = t * (colors.Length - 1);
            int lo = (int)scaled;
            int hi = Math.Min(lo + 1, colors.Length - 1);
            return Color.Lerp(colors[lo], colors[hi], scaled - lo);
        }

        public static Color GetWeaponGradient(float t) => MulticolorLerp(t, WeaponPalette);

        public static Color Additive(Color c, float opacity = 1f) => (c with { A = 0 }) * opacity;

        // ══════╁EEASING FUNCTIONS ══════╁E

        public delegate float EasingFunction(float t, int degree = 0);

        public static float LinearEasing(float t, int d = 0) => t;
        public static float SineInEasing(float t, int d = 0) => 1f - MathF.Cos(t * MathHelper.PiOver2);
        public static float SineOutEasing(float t, int d = 0) => MathF.Sin(t * MathHelper.PiOver2);
        public static float SineInOutEasing(float t, int d = 0) => -(MathF.Cos(MathHelper.Pi * t) - 1f) / 2f;
        public static float SineBumpEasing(float t, int d = 0) => MathF.Sin(MathHelper.Pi * t);
        public static float PolyInEasing(float t, int d = 3) => MathF.Pow(t, d);
        public static float PolyOutEasing(float t, int d = 3) => 1f - MathF.Pow(1f - t, d);
        public static float PolyInOutEasing(float t, int d = 3) => t < 0.5f ? MathF.Pow(2f, d - 1) * MathF.Pow(t, d) : 1f - MathF.Pow(-2f * t + 2f, d) / 2f;
        public static float ExpInEasing(float t, int d = 0) => t <= 0f ? 0f : MathF.Pow(2f, 10f * t - 10f);
        public static float ExpOutEasing(float t, int d = 0) => t >= 1f ? 1f : 1f - MathF.Pow(2f, -10f * t);
        public static float CircInEasing(float t, int d = 0) => 1f - MathF.Sqrt(1f - t * t);
        public static float CircOutEasing(float t, int d = 0) { float u = t - 1f; return MathF.Sqrt(1f - u * u); }

        // ══════╁ECURVE SEGMENTS ══════╁E

        public struct CurveSegment
        {
            public EasingFunction Easing;
            public float StartX;
            public float StartHeight;
            public float ElevationShift;
            public int Degree;

            public CurveSegment(EasingFunction easing, float startX, float startHeight, float elevationShift, int degree = 0)
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
            int active = 0;
            for (int i = segments.Length - 1; i >= 0; i--)
                if (progress >= segments[i].StartX) { active = i; break; }
            var seg = segments[active];
            float nextX = active < segments.Length - 1 ? segments[active + 1].StartX : 1f;
            float segLen = nextX - seg.StartX;
            float local = segLen > 0f ? (progress - seg.StartX) / segLen : 1f;
            return seg.StartHeight + seg.ElevationShift * seg.Easing(local, seg.Degree);
        }

        // ══════╁ESPRITEBATCH HELPERS ══════╁E

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
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public static void RestoreSpriteBatch(this SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        // ══════╁EGEOMETRY HELPERS ══════╁E

        public static Vector2 SafeDirectionTo(Vector2 from, Vector2 to, Vector2 fallback = default)
        {
            Vector2 diff = to - from;
            float len = diff.Length();
            return len < 0.0001f ? (fallback == default ? Vector2.UnitY : fallback) : diff / len;
        }

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
                {
                    best = n;
                    bestDist = d;
                }
            }
            return best;
        }

        public static float AngleTowards(float current, float target, float maxDelta)
        {
            float diff = MathHelper.WrapAngle(target - current);
            return current + MathHelper.Clamp(diff, -maxDelta, maxDelta);
        }

        // ══════╁ETHEME ACCENTS ══════╁E

        public static void DrawThemeAccents(SpriteBatch sb, Vector2 worldPos, float scale, float intensity)
        {
            Vector2 drawPos = worldPos - Main.screenPosition;
            float rot = (float)Main.GameUpdateCount * 0.02f;
            float pulse = 0.85f + 0.15f * MathF.Sin((float)Main.GameUpdateCount * 0.05f);
            Color glowColor = Additive(WeaponPalette[2], intensity * 0.5f * pulse);
            Color ringColor = Additive(WeaponPalette[3], intensity * 0.4f * pulse);

            Texture2D glow = Terraria.GameContent.TextureAssets.Extra[ExtrasID.ThePerfectGlow].Value;
            sb.Draw(glow, drawPos, null, glowColor, 0f, glow.Size() / 2f, scale * 0.6f, SpriteEffects.None, 0f);
            sb.Draw(glow, drawPos, null, ringColor, rot, glow.Size() / 2f, scale * 0.9f, SpriteEffects.None, 0f);
        }
    }
}
