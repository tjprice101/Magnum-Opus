using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Fate.ResonantWeapons.LightOfTheFuture.Utilities
{
    /// <summary>
    /// Self-contained color palette, easing functions, and SpriteBatch helpers
    /// for Light of the Future — The Cosmic Railgun.
    /// 
    /// Palette (cosmic dynamics):
    ///   [0] VoidBlack     — (10, 3, 18)     deep space void
    ///   [1] TrailViolet   — (140, 40, 200)  violet energy wake
    ///   [2] LaserCyan     — (60, 220, 255)  bright laser blue
    ///   [3] MuzzleGold    — (255, 200, 60)  muzzle flash gold
    ///   [4] PlasmaWhite   — (230, 245, 255) white-hot plasma
    ///   [5] ImpactCrimson — (240, 50, 80)   impact red
    /// </summary>
    public static class LightUtils
    {
        // ======================== COLOR PALETTE ========================

        public static readonly Color VoidBlack = new Color(10, 3, 18);
        public static readonly Color TrailViolet = new Color(140, 40, 200);
        public static readonly Color LaserCyan = new Color(60, 220, 255);
        public static readonly Color MuzzleGold = new Color(255, 200, 60);
        public static readonly Color PlasmaWhite = new Color(230, 245, 255);
        public static readonly Color ImpactCrimson = new Color(240, 50, 80);

        /// <summary>Softer cyan for secondary accents.</summary>
        public static readonly Color SoftCyan = new Color(100, 200, 240);
        /// <summary>Deep violet for smoke/nebula wisps.</summary>
        public static readonly Color DeepViolet = new Color(80, 20, 140);
        /// <summary>Hot white core for beam centres.</summary>
        public static readonly Color HotCore = new Color(255, 255, 255);
        /// <summary>Constellation silver for star connection lines.</summary>
        public static readonly Color ConstellationSilver = new Color(200, 210, 240);

        /// <summary>Ordered palette from darkest (pp) to brightest (sfz).</summary>
        public static readonly Color[] LightPalette = new Color[]
        {
            VoidBlack,        // [0]
            TrailViolet,      // [1]
            LaserCyan,        // [2]
            MuzzleGold,       // [3]
            PlasmaWhite,      // [4]
            ImpactCrimson,    // [5]
        };

        /// <summary>Lerp through the 6-color palette. t: 0..1</summary>
        public static Color PaletteLerp(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            float scaledT = t * (LightPalette.Length - 1);
            int lo = (int)scaledT;
            int hi = Math.Min(lo + 1, LightPalette.Length - 1);
            float frac = scaledT - lo;
            return Color.Lerp(LightPalette[lo], LightPalette[hi], frac);
        }

        /// <summary>Bullet trail gradient: void → violet → cyan → white as speed increases.</summary>
        public static Color BulletGradient(float speedRatio)
        {
            speedRatio = MathHelper.Clamp(speedRatio, 0f, 1f);
            if (speedRatio < 0.33f)
                return Color.Lerp(VoidBlack, TrailViolet, speedRatio / 0.33f);
            if (speedRatio < 0.66f)
                return Color.Lerp(TrailViolet, LaserCyan, (speedRatio - 0.33f) / 0.33f);
            return Color.Lerp(LaserCyan, PlasmaWhite, (speedRatio - 0.66f) / 0.34f);
        }

        /// <summary>Rocket gradient: crimson → gold → white.</summary>
        public static Color RocketGradient(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            if (t < 0.5f)
                return Color.Lerp(ImpactCrimson, MuzzleGold, t / 0.5f);
            return Color.Lerp(MuzzleGold, PlasmaWhite, (t - 0.5f) / 0.5f);
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

        public static float CubicOut(float t) { float inv = 1f - t; return 1f - inv * inv * inv; }

        public static float ExpIn(float t) => t <= 0f ? 0f : MathF.Pow(2f, 10f * t - 10f);
        public static float ExpOut(float t) => t >= 1f ? 1f : 1f - MathF.Pow(2f, -10f * t);

        public static float CircOut(float t) => MathF.Sqrt(1f - MathF.Pow(t - 1f, 2f));

        // ======================== SPRITEBATCH HELPERS ========================

        private static bool _inShaderRegion = false;

        /// <summary>Restart SpriteBatch in Additive+Immediate mode for shader usage.</summary>
        public static void EnterShaderRegion(SpriteBatch sb)
        {
            if (_inShaderRegion) return;
            sb.End();
            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp,
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
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
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
        /// Draw a multi-layer additive bloom using the Light of the Future palette.
        /// Layers: VoidBlack → DeepViolet → LaserCyan → PlasmaWhite → HotCore
        /// </summary>
        public static void DrawLightBloom(SpriteBatch sb, Texture2D glowTex, Vector2 worldPos,
            float baseScale, float intensity, float time)
        {
            Vector2 drawPos = worldPos - Main.screenPosition;
            Vector2 origin = glowTex.Size() / 2f;
            float pulse = 1f + MathF.Sin(time * 0.05f) * 0.06f;

            sb.Draw(glowTex, drawPos, null, Additive(VoidBlack, 0.15f * intensity),
                0f, origin, baseScale * 2.2f * pulse, SpriteEffects.None, 0f);

            sb.Draw(glowTex, drawPos, null, Additive(DeepViolet, 0.25f * intensity),
                0f, origin, baseScale * 1.6f * pulse, SpriteEffects.None, 0f);

            sb.Draw(glowTex, drawPos, null, Additive(LaserCyan, 0.35f * intensity),
                0f, origin, baseScale * 1.2f * pulse, SpriteEffects.None, 0f);

            sb.Draw(glowTex, drawPos, null, Additive(PlasmaWhite, 0.4f * intensity),
                0f, origin, baseScale * 0.8f * pulse, SpriteEffects.None, 0f);

            sb.Draw(glowTex, drawPos, null, Additive(HotCore, 0.5f * intensity),
                0f, origin, baseScale * 0.4f * pulse, SpriteEffects.None, 0f);
        }

        /// <summary>Draw item sprite with bloom layers behind it.</summary>
        public static void DrawItemBloom(SpriteBatch sb, Texture2D tex, Vector2 pos,
            Vector2 origin, float rotation, float scale, float pulse)
        {
            sb.Draw(tex, pos, null, Additive(TrailViolet, 0.2f), rotation, origin, scale * 1.15f * pulse, SpriteEffects.None, 0f);
            sb.Draw(tex, pos, null, Additive(LaserCyan, 0.15f), rotation, origin, scale * 1.08f * pulse, SpriteEffects.None, 0f);
            sb.Draw(tex, pos, null, Additive(PlasmaWhite, 0.12f), rotation, origin, scale * 1.04f, SpriteEffects.None, 0f);
        }
    }
}
