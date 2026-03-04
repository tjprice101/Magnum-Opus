using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using MagnumOpus.Content.LaCampanella;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.GrandioseChime.Utilities
{
    /// <summary>
    /// Utility class for GrandioseChime — beam-style ranged weapon.
    /// Every 3rd shot = bellfire barrage (7 burning notes), every 4th shot = music note mines, kill echoes.
    /// </summary>
    public static class GrandioseChimeUtils
    {
        // ──── Color Palettes ────
        // Beam = primary shot color: dark bronze → infernal orange → beam gold → searing white
        public static readonly Color[] BeamPalette = new Color[]
        {
            new Color(80, 30, 10),    // Dark bronze
            new Color(220, 100, 20),  // Infernal orange
            new Color(255, 180, 50),  // Beam gold
            new Color(255, 235, 180), // Searing white
        };

        // Barrage = bellfire barrage palette: ember red → fire orange → note gold
        public static readonly Color[] BarragePalette = new Color[]
        {
            new Color(180, 40, 15),   // Ember red
            new Color(255, 120, 30),  // Fire orange
            new Color(255, 200, 70),  // Note gold
        };

        // Mine = music note mine palette: dark violet → chime purple → detonation gold
        public static readonly Color[] MinePalette = new Color[]
        {
            new Color(90, 30, 80),    // Dark violet
            new Color(180, 70, 160),  // Chime purple
            new Color(255, 200, 80),  // Detonation gold
        };

        // Echo = kill echo palette: spectral teal → ghost white → fade
        public static readonly Color[] EchoPalette = new Color[]
        {
            new Color(80, 180, 160),  // Spectral teal
            new Color(200, 220, 255), // Ghost white
            new Color(255, 200, 100), // Echo gold
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

        public static Color GetBeamFlicker(float time)
        {
            float flicker = (float)(Math.Sin(time * 12f) * 0.25f + Math.Sin(time * 19f) * 0.15f + 0.5f);
            return MulticolorLerp(flicker, BeamPalette);
        }

        // ──── Easing ────
        public static float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);
        public static float EaseInQuad(float t) => t * t;
        public static float SmoothStep(float t) => t * t * (3f - 2f * t);

        // ──── SpriteBatch Helpers ────
        public static void BeginAdditive(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp,
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
