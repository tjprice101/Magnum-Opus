using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MagnumOpus.Content.Fate;
using System;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Fate.ResonantWeapons.FractalOfTheStars.Utilities
{
    /// <summary>
    /// Self-contained color palette, easing functions, and SpriteBatch helpers
    /// for Fractal of the Stars. Star/constellation themed — radically different from Requiem.
    ///
    /// Musical dynamics palette:
    ///   [0] Pianissimo = StarVoid (deep void)
    ///   [1] Piano      = FractalPurple (bright star purple)
    ///   [2] Mezzo      = StarGold (warm star gold)
    ///   [3] Forte      = NebulaPink (nebula pink)
    ///   [4] Fortissimo = ConstellationWhite (cool starlight white)
    ///   [5] Sforzando  = SupernovaFlash (hot white)
    /// </summary>
    public static class FractalUtils
    {
        // ======================== COLOR PALETTE ========================

        public static readonly Color StarVoid = new Color(10, 5, 25);
        public static readonly Color FractalPurple = new Color(100, 40, 180);
        public static readonly Color StarGold = new Color(255, 200, 50);
        public static readonly Color NebulaPink = new Color(200, 80, 160);
        public static readonly Color ConstellationWhite = new Color(220, 230, 255);
        public static readonly Color SupernovaFlash = new Color(255, 255, 240);
        public static readonly Color DeepIndigo = new Color(40, 20, 100);
        public static readonly Color StarfieldBlue = new Color(60, 100, 200);
        public static readonly Color GalacticAmber = new Color(240, 170, 60);
        public static readonly Color CelestialLavender = new Color(170, 130, 220);

        /// <summary>Ordered palette from darkest (pp) to brightest (sfz).</summary>
        public static readonly Color[] FractalPalette = new Color[]
        {
            StarVoid,              // [0] Pianissimo
            FractalPurple,         // [1] Piano
            StarGold,              // [2] Mezzo
            NebulaPink,            // [3] Forte
            ConstellationWhite,    // [4] Fortissimo
            SupernovaFlash,        // [5] Sforzando
        };

        /// <summary>Lerp through the 6-color palette. t: 0..1</summary>
        public static Color PaletteLerp(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            float scaledT = t * (FractalPalette.Length - 1);
            int lo = (int)scaledT;
            int hi = Math.Min(lo + 1, FractalPalette.Length - 1);
            float frac = scaledT - lo;
            return Color.Lerp(FractalPalette[lo], FractalPalette[hi], frac);
        }

        /// <summary>Stellar gradient: void → purple → gold → white</summary>
        public static Color GetStellarGradient(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            if (t < 0.33f)
                return Color.Lerp(StarVoid, FractalPurple, t / 0.33f);
            if (t < 0.66f)
                return Color.Lerp(FractalPurple, StarGold, (t - 0.33f) / 0.33f);
            return Color.Lerp(StarGold, SupernovaFlash, (t - 0.66f) / 0.34f);
        }

        /// <summary>Constellation gradient: dark indigo → starfield blue → white shimmer</summary>
        public static Color GetConstellationGradient(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            if (t < 0.5f)
                return Color.Lerp(DeepIndigo, StarfieldBlue, t / 0.5f);
            return Color.Lerp(StarfieldBlue, ConstellationWhite, (t - 0.5f) / 0.5f);
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
        /// Draw a multi-layer additive bloom at a position using the star-themed palette.
        /// Layers: StarVoid base → FractalPurple → StarGold → NebulaPink inner → White core
        /// </summary>
        public static void DrawFractalBloom(SpriteBatch sb, Texture2D glowTex, Vector2 worldPos,
            float baseScale, float intensity, float time)
        {
            Vector2 drawPos = worldPos - Main.screenPosition;
            Vector2 origin = glowTex.Size() / 2f;
            float pulse = 1f + MathF.Sin(time * 0.05f) * 0.06f;

            // Layer 1: Deep void base (widest)
            sb.Draw(glowTex, drawPos, null, Additive(StarVoid, 0.15f * intensity),
                0f, origin, baseScale * 2.2f * pulse, SpriteEffects.None, 0f);

            // Layer 2: Purple star haze
            sb.Draw(glowTex, drawPos, null, Additive(FractalPurple, 0.25f * intensity),
                0f, origin, baseScale * 1.6f * pulse, SpriteEffects.None, 0f);

            // Layer 3: Golden star fire
            sb.Draw(glowTex, drawPos, null, Additive(StarGold, 0.35f * intensity),
                0f, origin, baseScale * 1.2f * pulse, SpriteEffects.None, 0f);

            // Layer 4: Nebula pink inner glow
            sb.Draw(glowTex, drawPos, null, Additive(NebulaPink, 0.4f * intensity),
                0f, origin, baseScale * 0.8f * pulse, SpriteEffects.None, 0f);

            // Layer 5: Supernova white-hot core
            sb.Draw(glowTex, drawPos, null, Additive(SupernovaFlash, 0.5f * intensity),
                0f, origin, baseScale * 0.4f * pulse, SpriteEffects.None, 0f);
        }

        /// <summary>Draw item sprite with bloom layers behind it.</summary>
        public static void DrawItemBloom(SpriteBatch sb, Texture2D tex, Vector2 pos,
            Vector2 origin, float rotation, float scale, float pulse)
        {
            sb.Draw(tex, pos, null, Additive(FractalPurple, 0.2f), rotation, origin, scale * 1.15f * pulse, SpriteEffects.None, 0f);
            sb.Draw(tex, pos, null, Additive(StarGold, 0.15f), rotation, origin, scale * 1.08f * pulse, SpriteEffects.None, 0f);
            sb.Draw(tex, pos, null, Additive(NebulaPink, 0.12f), rotation, origin, scale * 1.04f, SpriteEffects.None, 0f);
        }

        /// <summary>Get a star-like shimmer color that cycles through gold → white → purple based on time.</summary>
        public static Color GetStarShimmer(float time, float speed = 1f)
        {
            float t = (MathF.Sin(time * speed) + 1f) * 0.5f;
            if (t < 0.33f)
                return Color.Lerp(StarGold, ConstellationWhite, t / 0.33f);
            if (t < 0.66f)
                return Color.Lerp(ConstellationWhite, CelestialLavender, (t - 0.33f) / 0.33f);
            return Color.Lerp(CelestialLavender, StarGold, (t - 0.66f) / 0.34f);
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
