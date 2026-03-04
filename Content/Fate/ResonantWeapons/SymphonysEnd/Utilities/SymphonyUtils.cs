using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MagnumOpus.Content.Fate;
using System;
using Terraria;

namespace MagnumOpus.Content.Fate.ResonantWeapons.SymphonysEnd
{
    /// <summary>
    /// Self-contained color palette and math utilities for Symphony's End.
    /// ZERO shared system references.
    /// </summary>
    public static class SymphonyUtils
    {
        // ═══════════ COLOR PALETTE — The Final Movement ═══════════

        /// <summary>The abyss before the first note.</summary>
        public static readonly Color VoidBlack = new Color(8, 4, 16);

        /// <summary>Hot pink — the wailing soprano.</summary>
        public static readonly Color SymphonyPink = new Color(220, 70, 150);

        /// <summary>Deep violet — the rumbling basso continuo.</summary>
        public static readonly Color SymphonyViolet = new Color(160, 40, 200);

        /// <summary>Harmonic blue — the resolving chord.</summary>
        public static readonly Color HarmonyBlue = new Color(80, 120, 255);

        /// <summary>Final chord white — the silence after.</summary>
        public static readonly Color FinalWhite = new Color(248, 245, 255);

        /// <summary>Dissonant red — the breaking string.</summary>
        public static readonly Color DiscordRed = new Color(240, 50, 60);

        /// <summary>Indexed palette for gradient interpolation.</summary>
        public static readonly Color[] Palette = new Color[]
        {
            VoidBlack,        // [0]
            SymphonyViolet,   // [1]
            SymphonyPink,     // [2]
            HarmonyBlue,      // [3]
            DiscordRed,       // [4]
            FinalWhite        // [5]
        };

        // ═══════════ COLOR HELPERS ═══════════

        /// <summary>Lerp through the 6-colour Symphony palette. t=0→VoidBlack, t=1→FinalWhite.</summary>
        public static Color PaletteLerp(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            float scaled = t * (Palette.Length - 1);
            int idx = (int)scaled;
            int next = Math.Min(idx + 1, Palette.Length - 1);
            return Color.Lerp(Palette[idx], Palette[next], scaled - idx);
        }

        /// <summary>Additive-friendly color with premultiplied alpha and zero alpha channel.</summary>
        public static Color Additive(Color c, float opacity)
            => new Color((int)(c.R * opacity), (int)(c.G * opacity), (int)(c.B * opacity), 0);

        /// <summary>
        /// 5-stop gradient: VoidBlack → Violet → Pink → Blue → White.
        /// </summary>
        public static Color GetSymphonyGradient(float progress)
        {
            progress = MathHelper.Clamp(progress, 0f, 1f);
            if (progress < 0.25f)
                return Color.Lerp(VoidBlack, SymphonyViolet, progress * 4f);
            if (progress < 0.5f)
                return Color.Lerp(SymphonyViolet, SymphonyPink, (progress - 0.25f) * 4f);
            if (progress < 0.75f)
                return Color.Lerp(SymphonyPink, HarmonyBlue, (progress - 0.5f) * 4f);
            return Color.Lerp(HarmonyBlue, FinalWhite, (progress - 0.75f) * 4f);
        }

        /// <summary>Random color from the Symphony palette.</summary>
        public static Color RandomPaletteColor() => PaletteLerp(Main.rand.NextFloat());

        // ═══════════ MATH HELPERS ═══════════

        /// <summary>Hermite smoothstep.</summary>
        public static float Smoothstep(float edge0, float edge1, float x)
        {
            float t = MathHelper.Clamp((x - edge0) / (edge1 - edge0), 0f, 1f);
            return t * t * (3f - 2f * t);
        }

        /// <summary>Circular offset at the given angle and radius.</summary>
        public static Vector2 HelixOffset(float angle, float radius)
            => new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * radius;

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
