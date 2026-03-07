using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MagnumOpus.Content.Fate;
using System;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Fate.ResonantWeapons.TheConductorsLastConstellation.Utilities
{
    /// <summary>
    /// Self-contained color palette, easing functions, and SpriteBatch helpers
    /// for The Conductor's Last Constellation. Conductor/baton themed — electric
    /// cyan lightning, deep void, and celestial starlight.
    ///
    /// Musical dynamics palette:
    /// Musical dynamics palette (Fate LUT ramp with rainbow shimmer):
    ///   [0] Pianissimo = VoidBlack (deep void darkness)
    ///   [1] Piano      = BatonPurple (conductor's baton purple)
    ///   [2] Mezzo      = ConductorCrimson (Fate crimson-pink)
    ///   [3] Forte      = LightningGold (warm cosmic gold)
    ///   [4] Fortissimo = StarSilver (constellation silver)
    ///   [5] Sforzando  = CelestialWhite (pure celestial flash)
    /// </summary>
    public static class ConductorUtils
    {
        // ======================== COLOR PALETTE (Fate LUT Ramp) ========================

        public static readonly Color VoidBlack = new Color(8, 5, 15);
        public static readonly Color BatonPurple = new Color(130, 50, 180);
        public static readonly Color ConductorCyan = new Color(200, 50, 120);    // Fate crimson-pink (was cyan)
        public static readonly Color LightningGold = new Color(255, 180, 60);    // Warm cosmic gold (was yellow-gold)
        public static readonly Color StarSilver = new Color(200, 190, 220);
        public static readonly Color CelestialWhite = new Color(240, 245, 255);
        public static readonly Color DeepIndigo = new Color(25, 10, 50);
        public static readonly Color ElectricBlue = new Color(140, 80, 200);     // Now violet-leaning (was blue)
        public static readonly Color CosmicPink = new Color(200, 80, 160);
        public static readonly Color ThunderAmber = new Color(255, 160, 60);

        /// <summary>Ordered palette from darkest (pp) to brightest (sfz).</summary>
        public static readonly Color[] ConductorPalette = new Color[]
        {
            VoidBlack,          // [0] Pianissimo
            BatonPurple,        // [1] Piano
            ConductorCyan,      // [2] Mezzo
            LightningGold,      // [3] Forte
            StarSilver,         // [4] Fortissimo
            CelestialWhite,     // [5] Sforzando
        };

        /// <summary>Lerp through the 6-color palette. t: 0..1</summary>
        public static Color PaletteLerp(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            float scaledT = t * (ConductorPalette.Length - 1);
            int lo = (int)scaledT;
            int hi = Math.Min(lo + 1, ConductorPalette.Length - 1);
            float frac = scaledT - lo;
            return Color.Lerp(ConductorPalette[lo], ConductorPalette[hi], frac);
        }

        /// <summary>Lightning gradient: void → purple → crimson-pink → cosmic gold → white (Fate LUT ramp)</summary>
        public static Color GetLightningGradient(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            if (t < 0.25f)
                return Color.Lerp(VoidBlack, BatonPurple, t / 0.25f);
            if (t < 0.5f)
                return Color.Lerp(BatonPurple, ConductorCyan, (t - 0.25f) / 0.25f);
            if (t < 0.75f)
                return Color.Lerp(ConductorCyan, LightningGold, (t - 0.5f) / 0.25f);
            return Color.Lerp(LightningGold, CelestialWhite, (t - 0.75f) / 0.25f);
        }

        /// <summary>Conductor baton gradient: deep indigo → violet → silver</summary>
        public static Color GetBatonGradient(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            if (t < 0.5f)
                return Color.Lerp(DeepIndigo, ElectricBlue, t / 0.5f);
            return Color.Lerp(ElectricBlue, StarSilver, (t - 0.5f) / 0.5f);
        }

        /// <summary>Push any color toward white by amount (0..1).</summary>
        public static Color WithWhitePush(Color c, float amount)
            => Color.Lerp(c, Color.White, MathHelper.Clamp(amount, 0f, 1f));

        /// <summary>Additive-safe color (premultiplied alpha, A=0).</summary>
        public static Color Additive(Color c, float opacity = 1f)
            => new Color(c.R, c.G, c.B, 0) * opacity;

        // ======================== EASING FUNCTIONS ========================

        public static float SineIn(float t) => 1f - MathF.Cos(t * MathHelper.PiOver2);
        public static float SineOut(float t) => MathF.Sin(t * MathHelper.PiOver2);
        public static float SineInOut(float t) => -(MathF.Cos(MathHelper.Pi * t) - 1f) / 2f;

        public static float QuadIn(float t) => t * t;
        public static float QuadOut(float t) => 1f - (1f - t) * (1f - t);
        public static float QuadInOut(float t) => t < 0.5f ? 2f * t * t : 1f - MathF.Pow(-2f * t + 2f, 2f) / 2f;

        public static float CubicIn(float t) => t * t * t;
        public static float CubicOut(float t) { float inv = 1f - t; return 1f - inv * inv * inv; }

        public static float ExpIn(float t) => t <= 0f ? 0f : MathF.Pow(2f, 10f * t - 10f);
        public static float ExpOut(float t) => t >= 1f ? 1f : 1f - MathF.Pow(2f, -10f * t);

        public static float CircOut(float t) => MathF.Sqrt(1f - MathF.Pow(t - 1f, 2f));

        public static float BackOut(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * MathF.Pow(t - 1f, 3f) + c1 * MathF.Pow(t - 1f, 2f);
        }

        public static float BounceOut(float t)
        {
            if (t < 1f / 2.75f) return 7.5625f * t * t;
            if (t < 2f / 2.75f) { t -= 1.5f / 2.75f; return 7.5625f * t * t + 0.75f; }
            if (t < 2.5f / 2.75f) { t -= 2.25f / 2.75f; return 7.5625f * t * t + 0.9375f; }
            t -= 2.625f / 2.75f;
            return 7.5625f * t * t + 0.984375f;
        }

        // ======================== CURVE SEGMENT ANIMATION ========================

        public struct CurveSegment
        {
            public float StartX;
            public float EndX;
            public float StartY;
            public float EndY;
            public Func<float, float> Easing;

            public CurveSegment(float startX, float endX, float startY, float endY, Func<float, float> easing = null)
            {
                StartX = startX; EndX = endX; StartY = startY; EndY = endY;
                Easing = easing ?? ((t) => t);
            }
        }

        /// <summary>Evaluate a piecewise animation curve at time t (0..1).</summary>
        public static float PiecewiseAnimation(float t, params CurveSegment[] segments)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            foreach (var seg in segments)
            {
                if (t >= seg.StartX && t <= seg.EndX)
                {
                    float localT = (t - seg.StartX) / Math.Max(seg.EndX - seg.StartX, 0.0001f);
                    return MathHelper.Lerp(seg.StartY, seg.EndY, seg.Easing(localT));
                }
            }
            return segments.Length > 0 ? segments[^1].EndY : 0f;
        }

        // ======================== SPRITEBATCH HELPERS ========================

        private static bool _inShaderRegion = false;

        /// <summary>Restart SpriteBatch in Additive+Immediate mode for shader usage.</summary>
        public static void EnterShaderRegion(SpriteBatch sb)
        {
            if (_inShaderRegion) return;
            sb.End();
            sb.Begin(SpriteSortMode.Immediate, MagnumBlendStates.ShaderAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            _inShaderRegion = true;
        }

        /// <summary>Restart SpriteBatch back to normal deferred mode.</summary>
        public static void ExitShaderRegion(SpriteBatch sb)
        {
            if (!_inShaderRegion) return;
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            _inShaderRegion = false;
        }

        /// <summary>Begin additive blending (no shader).</summary>
        public static void BeginAdditive(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>Return to default alpha blending.</summary>
        public static void EndAdditive(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        // ======================== ENTITY HELPERS ========================

        public static Vector2 SafeDirectionTo(Vector2 from, Vector2 to)
        {
            Vector2 diff = to - from;
            return diff == Vector2.Zero ? Vector2.UnitX : Vector2.Normalize(diff);
        }

        public static NPC ClosestNPCAt(Vector2 pos, float maxDist)
        {
            NPC best = null;
            float bestDist = maxDist;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                float d = Vector2.Distance(pos, npc.Center);
                if (d < bestDist) { best = npc; bestDist = d; }
            }
            return best;
        }

        // ======================== BLOOM & GLOW HELPERS ========================

        /// <summary>
        /// Draw a multi-layer additive bloom at a position using the conductor palette.
        /// Layers: VoidBlack base → BatonPurple → ConductorCyan → LightningGold inner → White core
        /// </summary>
        public static void DrawConductorBloom(SpriteBatch sb, Texture2D glowTex, Vector2 worldPos,
            float baseScale, float intensity, float time)
        {
            Vector2 drawPos = worldPos - Main.screenPosition;
            Vector2 origin = glowTex.Size() / 2f;
            float pulse = 1f + MathF.Sin(time * 0.05f) * 0.06f;

            // Layer 1: Deep void base (widest)
            sb.Draw(glowTex, drawPos, null, Additive(VoidBlack, 0.15f * intensity),
                0f, origin, baseScale * 2.2f * pulse, SpriteEffects.None, 0f);

            // Layer 2: Baton purple haze
            sb.Draw(glowTex, drawPos, null, Additive(BatonPurple, 0.25f * intensity),
                0f, origin, baseScale * 1.6f * pulse, SpriteEffects.None, 0f);

            // Layer 3: Conductor cyan electric
            sb.Draw(glowTex, drawPos, null, Additive(ConductorCyan, 0.35f * intensity),
                0f, origin, baseScale * 1.2f * pulse, SpriteEffects.None, 0f);

            // Layer 4: Lightning gold inner glow
            sb.Draw(glowTex, drawPos, null, Additive(LightningGold, 0.4f * intensity),
                0f, origin, baseScale * 0.8f * pulse, SpriteEffects.None, 0f);

            // Layer 5: Celestial white-hot core
            sb.Draw(glowTex, drawPos, null, Additive(CelestialWhite, 0.5f * intensity),
                0f, origin, baseScale * 0.4f * pulse, SpriteEffects.None, 0f);
        }

        /// <summary>Draw item sprite with bloom layers behind it.</summary>
        public static void DrawItemBloom(SpriteBatch sb, Texture2D tex, Vector2 pos,
            Vector2 origin, float rotation, float scale, float pulse)
        {
            sb.Draw(tex, pos, null, Additive(BatonPurple, 0.2f), rotation, origin, scale * 1.15f * pulse, SpriteEffects.None, 0f);
            sb.Draw(tex, pos, null, Additive(ConductorCyan, 0.15f), rotation, origin, scale * 1.08f * pulse, SpriteEffects.None, 0f);
            sb.Draw(tex, pos, null, Additive(LightningGold, 0.12f), rotation, origin, scale * 1.04f, SpriteEffects.None, 0f);
        }

        /// <summary>Get a rainbow-tinted shimmer that cycles through the Fate palette with HSL rainbow accents.</summary>
        public static Color GetConductorShimmer(float time, float speed = 1f)
        {
            float t = (MathF.Sin(time * speed) + 1f) * 0.5f;
            // Base: cycle through Fate LUT ramp
            Color baseColor;
            if (t < 0.33f)
                baseColor = Color.Lerp(ConductorCyan, LightningGold, t / 0.33f);
            else if (t < 0.66f)
                baseColor = Color.Lerp(LightningGold, CelestialWhite, (t - 0.33f) / 0.33f);
            else
                baseColor = Color.Lerp(CelestialWhite, ConductorCyan, (t - 0.66f) / 0.34f);
            // Rainbow accent: blend in HSL-cycling hue for prismatic shimmer
            float hue = (time * 0.12f * speed) % 1f;
            Color rainbow = Main.hslToRgb(hue, 0.75f, 0.7f);
            return Color.Lerp(baseColor, rainbow, 0.25f);
        }

        // ─────────── THEME TEXTURE ACCENTS ───────────

        /// <summary>
        /// Draws theme-textured accents. Call under Additive blend.
        /// </summary>
        public static void DrawThemeAccents(SpriteBatch sb, Vector2 worldPos, float scale, float intensity = 1f)
        {
            FateVFXLibrary.DrawThemeCelestialGlyph(sb, worldPos, scale, intensity * 0.5f);
            float rot = (float)Main.GameUpdateCount * 0.02f;
            FateVFXLibrary.DrawThemeImpactRing(sb, worldPos, scale, intensity * 0.4f, rot);
        }
    }
}
