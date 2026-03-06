using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using MagnumOpus.Content.LaCampanella;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.SymphonicBellfireAnnihilator.Utilities
{
    /// <summary>
    /// Utility class for SymphonicBellfireAnnihilator — rocket launcher with volley system.
    /// 5+5 volley, Grand Crescendo wave, BellfireCrescendo (+10% dmg/speed 30s), GrandCrescendo buffs.
    /// </summary>
    public static class SymphonicBellfireUtils
    {
        // ──── Color Palettes ────
        // Rocket = primary rocket palette: gunmetal → ember red → fire orange → ignition white
        public static readonly Color[] RocketPalette = new Color[]
        {
            new Color(50, 25, 15),    // Gunmetal soot
            new Color(180, 50, 20),   // Ember red
            new Color(255, 130, 30),  // Fire orange
            new Color(255, 220, 160), // Ignition white
        };

        // Crescendo = Grand Crescendo wave palette: deep crimson → blazing gold → divine white → pure highlight
        public static readonly Color[] CrescendoPalette = new Color[]
        {
            new Color(140, 20, 10),   // Deep crimson
            new Color(255, 160, 30),  // Blazing gold
            new Color(255, 240, 200), // Divine white
            new Color(255, 255, 240), // Pure highlight
        };

        // Volley = volley tracker VFX: dark → building → peak
        public static readonly Color[] VolleyPalette = new Color[]
        {
            new Color(100, 40, 15),   // Dark coal
            new Color(220, 90, 25),   // Building fire
            new Color(255, 180, 60),  // Volley peak
        };

        public static readonly Color LoreColor = new Color(255, 140, 40);

        public static Color MulticolorLerp(float t, params Color[] colors)
        {
            t = MathHelper.Clamp(t, 0f, 0.999f);
            float scaled = t * (colors.Length - 1);
            int index = (int)scaled;
            float frac = scaled - index;
            if (index >= colors.Length - 1) return colors[colors.Length - 1];
            return Color.Lerp(colors[index], colors[index + 1], frac);
        }

        public static Color GetRocketFlicker(float time)
        {
            float flicker = (float)(Math.Sin(time * 10f) * 0.3f + Math.Sin(time * 17f) * 0.2f + 0.5f);
            return MulticolorLerp(flicker, RocketPalette);
        }

        // ──── Easing ────
        public static float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);
        public static float EaseInQuad(float t) => t * t;
        public static float SmoothStep(float t) => t * t * (3f - 2f * t);

        // ──── SpriteBatch Helpers ────
        public static void BeginAdditive(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public static void RestoreVanilla(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        // ─────────── THEME TEXTURE ACCENTS ───────────

        public static void DrawThemeAccents(SpriteBatch sb, Vector2 worldPos, float scale, float intensity = 1f)
        {
            LaCampanellaVFXLibrary.DrawThemeStarFlare(sb, worldPos, scale, intensity * 0.5f);
            float rot = (float)Main.GameUpdateCount * 0.02f;
            LaCampanellaVFXLibrary.DrawThemeImpactRing(sb, worldPos, scale, intensity * 0.4f, rot);
        }
    }
}
