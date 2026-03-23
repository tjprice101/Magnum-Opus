using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MagnumOpus.Content.Fate;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Fate.ResonantWeapons.CodaOfAnnihilation.Utilities
{
    /// <summary>
    /// Self-contained utility library for Coda of Annihilation.
    /// Color palette, weapon textures/colors, gradient helpers, SpriteBatch modes.
    /// ZERO references to shared VFX systems.
    /// </summary>
    public static class CodaUtils
    {
        #region Color Palette — Annihilation

        public static readonly Color VoidBlack = new Color(10, 4, 18);
        public static readonly Color CodaPurple = new Color(140, 30, 160);
        public static readonly Color CodaCrimson = new Color(220, 60, 90);
        public static readonly Color CodaPink = new Color(200, 80, 130);
        public static readonly Color StarGold = new Color(255, 230, 180);
        public static readonly Color AnnihilationWhite = new Color(250, 248, 255);

        /// <summary>Core 6-color palette: void → annihilation white.</summary>
        public static readonly Color[] AnnihilationPalette = new Color[]
        {
            VoidBlack,
            CodaPurple,
            CodaCrimson,
            CodaPink,
            StarGold,
            AnnihilationWhite,
        };

        /// <summary>Lore tooltip color — cosmic crimson.</summary>
        public static readonly Color LoreColor = new Color(180, 40, 80);

        #endregion

        #region 14 Weapon Colors + Textures

        /// <summary>Theme colors for each weapon index (0-13).</summary>
        public static readonly Color[] WeaponColors = new Color[]
        {
            // Moonlight Sonata — purple, light blue
            new Color(138, 43, 226),
            new Color(135, 206, 250),
            // Eroica — scarlet, gold
            new Color(255, 100, 100),
            new Color(255, 200, 80),
            // La Campanella — orange, golden orange
            new Color(255, 140, 40),
            new Color(255, 180, 60),
            // Enigma — purple, green
            new Color(140, 60, 200),
            new Color(50, 180, 100),
            // Swan Lake — white
            new Color(255, 255, 255),
            // Fate variants
            new Color(180, 50, 100),
            new Color(200, 60, 80),
            new Color(140, 50, 160),
            new Color(160, 80, 140),
            new Color(220, 80, 120),
        };

        /// <summary>Texture paths for each weapon index (0-13).</summary>
        public static readonly string[] WeaponTexturePaths = new string[]
        {
            "MagnumOpus/Content/MoonlightSonata/Weapons/IncisorOfMoonlight/IncisorOfMoonlight",
            "MagnumOpus/Content/MoonlightSonata/Weapons/EternalMoon/EternalMoon",
            "MagnumOpus/Content/Eroica/Weapons/SakurasBlossom/SakurasBlossom",
            "MagnumOpus/Content/Eroica/Weapons/CelestialValor/CelestialValor",
            "MagnumOpus/Content/LaCampanella/ResonantWeapons/IgnitionOfTheBell/IgnitionOfTheBell",
            "MagnumOpus/Content/LaCampanella/ResonantWeapons/DualFatedChime/DualFatedChime",
            "MagnumOpus/Content/EnigmaVariations/ResonantWeapons/VariationsOfTheVoid/VariationsOfTheVoid",
            "MagnumOpus/Content/EnigmaVariations/ResonantWeapons/TheUnresolvedCadence/TheUnresolvedCadence",
            "MagnumOpus/Content/SwanLake/ResonantWeapons/CalloftheBlackSwan",
            "MagnumOpus/Content/Fate/ResonantWeapons/TheConductorsLastConstellation",
            "MagnumOpus/Content/Fate/ResonantWeapons/RequiemOfReality",
            "MagnumOpus/Content/Fate/ResonantWeapons/OpusUltima",
            "MagnumOpus/Content/Fate/ResonantWeapons/FractalOfTheStars",
            "MagnumOpus/Content/Fate/ResonantWeapons/CodaOfAnnihilation",
        };

        private static Asset<Texture2D>[] _cachedTextures;
        private static bool _texturesLoaded;

        #endregion

        #region Weapon Accessors

        /// <summary>Get color for weapon index, clamped.</summary>
        public static Color GetWeaponColor(int index)
        {
            if (index >= 0 && index < WeaponColors.Length)
                return WeaponColors[index];
            return CodaCrimson;
        }

        /// <summary>Get cached texture for weapon index. Loads lazily on first call.</summary>
        public static Texture2D GetWeaponTexture(int index)
        {
            EnsureTexturesLoaded();
            if (index >= 0 && index < _cachedTextures.Length && _cachedTextures[index] != null &&
                _cachedTextures[index].State == AssetState.Loaded)
                return _cachedTextures[index].Value;
            return null;
        }

        /// <summary>
        /// Gets a normalized draw scale so that all weapon textures (regardless of source size)
        /// render at a consistent visual size (~50px). Oversized sprites get scaled down.
        /// </summary>
        public static float GetNormalizedDrawScale(Texture2D tex, float targetSize = 50f)
        {
            float maxDim = MathHelper.Max(tex.Width, tex.Height);
            if (maxDim <= targetSize) return 1f;
            return targetSize / maxDim;
        }

        private static void EnsureTexturesLoaded()
        {
            if (_texturesLoaded) return;
            _cachedTextures = new Asset<Texture2D>[WeaponTexturePaths.Length];
            for (int i = 0; i < WeaponTexturePaths.Length; i++)
            {
                try
                {
                    _cachedTextures[i] = ModContent.Request<Texture2D>(
                        WeaponTexturePaths[i], AssetRequestMode.ImmediateLoad);
                }
                catch
                {
                    _cachedTextures[i] = null;
                }
            }
            _texturesLoaded = true;
        }

        #endregion

        #region Gradient Helpers

        /// <summary>Smoothly interpolate through a color array at position t (0-1).</summary>
        public static Color MulticolorLerp(float t, params Color[] colors)
        {
            t = MathHelper.Clamp(t, 0f, 0.999f);
            int count = colors.Length;
            float scaled = t * (count - 1);
            int index = (int)scaled;
            float frac = scaled - index;
            if (index >= count - 1) return colors[count - 1];
            return Color.Lerp(colors[index], colors[index + 1], frac);
        }

        /// <summary>Get annihilation gradient (void → white).</summary>
        public static Color GetAnnihilationGradient(float t)
            => MulticolorLerp(t, AnnihilationPalette);

        /// <summary>Make a color additive-friendly (zero alpha).</summary>
        public static Color Additive(Color c, float opacity = 1f)
            => new Color(c.R, c.G, c.B, 0) * opacity;

        /// <summary>Sine ease for smooth transitions.</summary>
        public static float SineEase(float t) => (float)Math.Sin(t * MathHelper.PiOver2);

        /// <summary>Quadratic bump (0→1→0).</summary>
        public static float QuadBump(float t) => t * (4f - t * 4f);

        // Easing functions for CurveSegment animation
        public static float SineOut(float t) => MathF.Sin(t * MathHelper.PiOver2);
        public static float SineInOut(float t) => -(MathF.Cos(MathHelper.Pi * t) - 1f) / 2f;
        public static float QuadIn(float t) => t * t;
        public static float QuadOut(float t) => 1f - (1f - t) * (1f - t);
        public static float CubicIn(float t) => t * t * t;
        public static float ExpIn(float t) => t <= 0f ? 0f : MathF.Pow(2f, 10f * t - 10f);

        #endregion

        #region CurveSegment Animation

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

        #endregion

        #region SpriteBatch Modes

        /// <summary>Restart SpriteBatch in additive blending with linear clamp.</summary>
        public static void BeginAdditive(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>Restart SpriteBatch in standard alpha blend with point clamp.</summary>
        public static void BeginDefault(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        #endregion

        #region Math Helpers

        /// <summary>Smooth rotation interpolation.</summary>
        public static float LerpAngle(float from, float to, float amount)
        {
            float diff = MathHelper.WrapAngle(to - from);
            return from + diff * amount;
        }

        /// <summary>Catmull-Rom spline sample between p1 and p2.</summary>
        public static Vector2 CatmullRom(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;
            return 0.5f * (
                (2f * p1) +
                (-p0 + p2) * t +
                (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
                (-p0 + 3f * p1 - 3f * p2 + p3) * t3
            );
        }

        #endregion

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
