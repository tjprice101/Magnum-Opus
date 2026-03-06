using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MagnumOpus.Content.Fate;
using System;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Fate.ResonantWeapons.OpusUltima.Utilities
{
    /// <summary>
    /// Self-contained color palette, easing functions, and SpriteBatch helpers
    /// for Opus Ultima — The Magnum Opus. No external VFX library dependencies.
    ///
    /// Musical dynamics palette:
    ///   [0] Pianissimo = VoidBlack (deepest void)
    ///   [1] Piano      = RoyalPurple (cosmic royalty)
    ///   [2] Mezzo      = OpusCrimson (burning crimson destiny)
    ///   [3] Forte      = OpusPink (cosmic rose)
    ///   [4] Fortissimo = GloryGold (triumphant gold)
    ///   [5] Sforzando  = OpusWhite (transcendent white)
    /// </summary>
    public static class OpusUtils
    {
        // ======================== COLOR PALETTE ========================

        public static readonly Color VoidBlack = new Color(12, 5, 18);
        public static readonly Color RoyalPurple = new Color(140, 30, 160);
        public static readonly Color OpusCrimson = new Color(220, 40, 70);
        public static readonly Color OpusPink = new Color(200, 80, 140);
        public static readonly Color GloryGold = new Color(255, 190, 40);
        public static readonly Color OpusWhite = new Color(245, 240, 255);
        public static readonly Color CosmicRose = new Color(180, 60, 120);
        public static readonly Color DestinyFlame = new Color(255, 120, 60);
        public static readonly Color StarSilver = new Color(210, 215, 240);

        /// <summary>Ordered palette from darkest (pp) to brightest (sfz).</summary>
        public static readonly Color[] OpusPalette = new Color[]
        {
            VoidBlack,       // [0] Pianissimo
            RoyalPurple,     // [1] Piano
            OpusCrimson,     // [2] Mezzo
            OpusPink,        // [3] Forte
            GloryGold,       // [4] Fortissimo
            OpusWhite,       // [5] Sforzando
        };

        /// <summary>Lerp through the 6-color palette. t: 0..1</summary>
        public static Color PaletteLerp(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            float scaledT = t * (OpusPalette.Length - 1);
            int lo = (int)scaledT;
            int hi = Math.Min(lo + 1, OpusPalette.Length - 1);
            float frac = scaledT - lo;
            return Color.Lerp(OpusPalette[lo], OpusPalette[hi], frac);
        }

        /// <summary>Cosmic gradient: void → crimson → gold → white</summary>
        public static Color GetCosmicGradient(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            if (t < 0.33f)
                return Color.Lerp(VoidBlack, OpusCrimson, t / 0.33f);
            if (t < 0.66f)
                return Color.Lerp(OpusCrimson, GloryGold, (t - 0.33f) / 0.33f);
            return Color.Lerp(GloryGold, OpusWhite, (t - 0.66f) / 0.34f);
        }

        /// <summary>Push any color toward white by amount (0..1).</summary>
        public static Color WithWhitePush(Color c, float amount)
            => Color.Lerp(c, Color.White, MathHelper.Clamp(amount, 0f, 1f));

        /// <summary>Additive-safe color (premultiplied alpha).</summary>
        public static Color Additive(Color c, float opacity = 1f)
            => new Color(c.R, c.G, c.B) * opacity;

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
        /// Draw a multi-layer additive bloom at a position using the Opus palette.
        /// Layers: VoidBlack → RoyalPurple → OpusCrimson → GloryGold → OpusWhite
        /// </summary>
        public static void DrawOpusBloom(SpriteBatch sb, Texture2D glowTex, Vector2 worldPos,
            float baseScale, float intensity, float time)
        {
            Vector2 drawPos = worldPos - Main.screenPosition;
            Vector2 origin = glowTex.Size() / 2f;
            float pulse = 1f + MathF.Sin(time * 0.05f) * 0.06f;

            sb.Draw(glowTex, drawPos, null, Additive(VoidBlack, 0.15f * intensity),
                0f, origin, baseScale * 2.2f * pulse, SpriteEffects.None, 0f);
            sb.Draw(glowTex, drawPos, null, Additive(RoyalPurple, 0.25f * intensity),
                0f, origin, baseScale * 1.6f * pulse, SpriteEffects.None, 0f);
            sb.Draw(glowTex, drawPos, null, Additive(OpusCrimson, 0.35f * intensity),
                0f, origin, baseScale * 1.2f * pulse, SpriteEffects.None, 0f);
            sb.Draw(glowTex, drawPos, null, Additive(GloryGold, 0.4f * intensity),
                0f, origin, baseScale * 0.8f * pulse, SpriteEffects.None, 0f);
            sb.Draw(glowTex, drawPos, null, Additive(OpusWhite, 0.5f * intensity),
                0f, origin, baseScale * 0.4f * pulse, SpriteEffects.None, 0f);
        }

        /// <summary>Draw item sprite with bloom layers behind it.</summary>
        public static void DrawItemBloom(SpriteBatch sb, Texture2D tex, Vector2 pos,
            Vector2 origin, float rotation, float scale, float pulse)
        {
            sb.Draw(tex, pos, null, Additive(RoyalPurple, 0.2f), rotation, origin, scale * 1.15f * pulse, SpriteEffects.None, 0f);
            sb.Draw(tex, pos, null, Additive(OpusCrimson, 0.15f), rotation, origin, scale * 1.08f * pulse, SpriteEffects.None, 0f);
            sb.Draw(tex, pos, null, Additive(GloryGold, 0.12f), rotation, origin, scale * 1.04f, SpriteEffects.None, 0f);
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
