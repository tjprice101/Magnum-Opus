using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

namespace MagnumOpus.Content.DiesIrae.Weapons.DeathTollingBell.Utilities
{
    /// <summary>
    /// Self-contained utility for Death Tolling Bell.
    /// Dark bell resonance — expanding shockwave rings with hellish overtone colors.
    /// </summary>
    public static class BellUtils
    {
        // ── BELL PALETTE (resonance gradient: dark toll → burning ring → fading echo) ──
        public static readonly Color[] ResonancePalette = new Color[]
        {
            new Color(15, 5, 5),         // Silent void
            new Color(100, 20, 0),       // Deep toll crimson
            new Color(180, 50, 10),      // Burning resonance
            new Color(255, 140, 30),     // Hellfire ring gold
            new Color(255, 200, 100),    // Echo gold
            new Color(255, 240, 200),    // Lingering white
        };

        public static readonly Color TollCrimson = new Color(100, 20, 0);
        public static readonly Color BurningResonance = new Color(180, 50, 10);
        public static readonly Color HellfireGold = new Color(255, 140, 30);
        public static readonly Color EchoGold = new Color(255, 200, 100);
        public static readonly Color BellWhite = new Color(255, 240, 200);
        public static readonly Color DarkSmoke = new Color(25, 18, 22);
        public static readonly Color EmberOrange = new Color(255, 100, 20);

        public static Color ResonanceLerp(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            float scaled = t * (ResonancePalette.Length - 1);
            int lo = (int)scaled;
            int hi = Math.Min(lo + 1, ResonancePalette.Length - 1);
            return Color.Lerp(ResonancePalette[lo], ResonancePalette[hi], scaled - lo);
        }

        public static Color MulticolorLerp(float t, params Color[] colors)
        {
            t = MathHelper.Clamp(t, 0f, 0.999f);
            float scaled = t * (colors.Length - 1);
            int lo = (int)scaled;
            int hi = Math.Min(lo + 1, colors.Length - 1);
            return Color.Lerp(colors[lo], colors[hi], scaled - lo);
        }

        public static Color Additive(Color c) => c with { A = 0 };
        public static Color Additive(Color c, float opacity) => c with { A = 0 } * opacity;

        /// <summary>
        /// Bell resonance pulse — damped oscillation like a struck bell.
        /// Returns 0..1, decaying over time with harmonic ringing.
        /// </summary>
        public static float BellDecay(float t, float decayRate = 3f, float frequency = 8f)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            float decay = (float)Math.Exp(-decayRate * t);
            float oscillation = (float)Math.Abs(Math.Sin(frequency * t * MathHelper.Pi));
            return decay * oscillation;
        }

        // ── SPRITEBATCH HELPERS ──
        public static void BeginAdditive(SpriteBatch sb)
        {
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public static void ResetSpriteBatch(SpriteBatch sb)
        {
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public static NPC ClosestNPCAt(Vector2 pos, float maxDist)
        {
            NPC best = null;
            float bestDist = maxDist;
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (!npc.CanBeChasedBy()) continue;
                float dist = Vector2.Distance(pos, npc.Center);
                if (dist < bestDist) { best = npc; bestDist = dist; }
            }
            return best;
        }
    }
}
