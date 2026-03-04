using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using MagnumOpus.Content.LaCampanella;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.PiercingBellsResonance.Utilities
{
    /// <summary>
    /// Self-contained utility class for PiercingBellsResonance — rapid-fire ranged gun.
    /// Scorching Staccato acceleration, resonant blast every 20th shot, homing note + crystal sub-projectiles.
    /// </summary>
    public static class PiercingBellsResonanceUtils
    {
        // ──── Color Palettes ────
        // Staccato = rapid-fire bullet palette: dark ember → hot orange → muzzle flash white
        public static readonly Color[] StaccatoPalette = new Color[]
        {
            new Color(60, 20, 10),    // Dark ember
            new Color(200, 80, 20),   // Searing orange
            new Color(255, 160, 50),  // Hot gold
            new Color(255, 220, 150), // Muzzle flash
        };

        // Resonance = 20th-shot blast palette: deep crimson → infernal orange → bell gold → resonant white
        public static readonly Color[] ResonancePalette = new Color[]
        {
            new Color(120, 20, 10),   // Deep crimson
            new Color(255, 100, 20),  // Infernal orange
            new Color(255, 200, 60),  // Bell gold
            new Color(255, 240, 200), // Resonant white
        };

        // Crystal palette for seeking crystal sub-projectiles
        public static readonly Color[] CrystalPalette = new Color[]
        {
            new Color(180, 60, 20),   // Molten ruby
            new Color(255, 130, 40),  // Flame amber
            new Color(255, 200, 100), // Crystal gold
        };

        public static readonly Color LoreColor = new Color(255, 140, 40);

        // ──── Color Helpers ────
        public static Color MulticolorLerp(float t, params Color[] colors)
        {
            t = MathHelper.Clamp(t, 0f, 0.999f);
            float scaled = t * (colors.Length - 1);
            int index = (int)scaled;
            float frac = scaled - index;
            if (index >= colors.Length - 1) return colors[colors.Length - 1];
            return Color.Lerp(colors[index], colors[index + 1], frac);
        }

        public static Color GetMuzzleFlicker(float time)
        {
            float flicker = (float)(Math.Sin(time * 15f) * 0.3f + Math.Sin(time * 23f) * 0.2f + 0.5f);
            return MulticolorLerp(flicker, StaccatoPalette);
        }

        // ──── Math / Easing ────
        public static float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);
        public static float EaseInQuad(float t) => t * t;
        public static float EaseOutBack(float t) { float c = 1.70158f; return 1f + (c + 1f) * (float)Math.Pow(t - 1, 3) + c * (float)Math.Pow(t - 1, 2); }
        public static float EaseInOutCubic(float t) => t < 0.5f ? 4f * t * t * t : 1f - (float)Math.Pow(-2 * t + 2, 3) / 2f;
        public static float SmoothStep(float t) => t * t * (3f - 2f * t);

        // ──── SpriteBatch Helpers ────
        public static void BeginAdditive(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public static void BeginAlpha(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
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
