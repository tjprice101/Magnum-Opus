using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MagnumOpus.Content.Fate;
using System;
using Terraria;

namespace MagnumOpus.Content.Fate.ResonantWeapons.DestinysCrescendo
{
    /// <summary>
    /// Self-contained color palette, gradient helpers, and SpriteBatch mode toggles
    /// for the Destiny's Crescendo cosmic deity summoner weapon.
    /// ZERO shared system references.
    /// </summary>
    public static class CrescendoUtils
    {
        // ═══════════ COLOR PALETTE — The Conductor's Deity ═══════════

        /// <summary>The void between stars — the silence before the conductor raises the baton.</summary>
        public static readonly Color VoidBlack = new Color(15, 5, 20);

        /// <summary>Deep deity purple — the cosmic entity's core resonance.</summary>
        public static readonly Color DeityPurple = new Color(120, 30, 140);

        /// <summary>Crescendo pink — fate's rising melody given form.</summary>
        public static readonly Color CrescendoPink = new Color(180, 50, 100);

        /// <summary>Divine crimson — the deity's wrath made manifest.</summary>
        public static readonly Color DivineCrimson = new Color(255, 60, 80);

        /// <summary>Star gold — celestial authority blazing.</summary>
        public static readonly Color StarGold = new Color(255, 230, 180);

        /// <summary>Celestial white — the heavens answer the crescendo.</summary>
        public static readonly Color CelestialWhite = new Color(255, 255, 255);

        /// <summary>Indexed palette for gradient interpolation.</summary>
        public static readonly Color[] Palette = new Color[]
        {
            VoidBlack,       // [0] Pianissimo
            DeityPurple,     // [1] Piano
            CrescendoPink,   // [2] Mezzo
            DivineCrimson,   // [3] Forte
            StarGold,        // [4] Fortissimo
            CelestialWhite   // [5] Sforzando
        };

        // ═══════════ COLOR HELPERS ═══════════

        /// <summary>Lerp through the 6-colour Crescendo palette. t=0→VoidBlack, t=1→CelestialWhite.</summary>
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
        /// 5-stop gradient: VoidBlack → DeityPurple → CrescendoPink → DivineCrimson → StarGold → CelestialWhite.
        /// Maps 0→1 through the full cosmic spectrum.
        /// </summary>
        public static Color GetCrescendoGradient(float progress)
        {
            progress = MathHelper.Clamp(progress, 0f, 1f);
            if (progress < 0.2f)
                return Color.Lerp(VoidBlack, DeityPurple, progress * 5f);
            if (progress < 0.4f)
                return Color.Lerp(DeityPurple, CrescendoPink, (progress - 0.2f) * 5f);
            if (progress < 0.6f)
                return Color.Lerp(CrescendoPink, DivineCrimson, (progress - 0.4f) * 5f);
            if (progress < 0.8f)
                return Color.Lerp(DivineCrimson, StarGold, (progress - 0.6f) * 5f);
            return Color.Lerp(StarGold, CelestialWhite, (progress - 0.8f) * 5f);
        }

        /// <summary>Random color from the Crescendo palette.</summary>
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

        /// <summary>Catmull-Rom interpolation between four points.</summary>
        public static Vector2 CatmullRom(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;
            return 0.5f * (
                2f * p1 +
                (-p0 + p2) * t +
                (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
                (-p0 + 3f * p1 - 3f * p2 + p3) * t3
            );
        }

        // ═══════════ SPRITEBATCH MODE TOGGLES ═══════════

        /// <summary>Switch SpriteBatch to Additive blend mode with standard settings.</summary>
        public static void BeginAdditive(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>Restore SpriteBatch to standard AlphaBlend mode.</summary>
        public static void BeginAlpha(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
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
