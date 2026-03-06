using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using MagnumOpus.Content.SwanLake;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.TheSwansLament.Utilities
{
    /// <summary>
    /// Self-contained utilities for The Swan's Lament.
    /// Theme: Mourning, destruction, catharsis. Dark with prismatic flashes — like
    /// light breaking through grief. Black/grey base with sudden prismatic reveals.
    /// </summary>
    public static class LamentUtils
    {
        public static readonly Color MourningBlack = new Color(18, 18, 22);
        public static readonly Color GriefGrey = new Color(100, 100, 108);
        public static readonly Color CatharsisWhite = new Color(240, 240, 248);
        public static readonly Color RevelationWhite = new Color(255, 255, 255);
        public static readonly Color LoreColor = new Color(240, 240, 255);

        /// <summary>Lament palette — neutral black→grey→white mourning ramp. No purple cast.</summary>
        public static readonly Color[] LamentPalette = new Color[]
        {
            new Color(20, 20, 25),
            new Color(70, 70, 78),
            new Color(130, 130, 140),
            new Color(195, 195, 205),
            new Color(235, 235, 245),
            new Color(255, 255, 255),
        };

        public static Color GetLamentGradient(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            float scaled = t * (LamentPalette.Length - 1);
            int lo = (int)scaled;
            int hi = Math.Min(lo + 1, LamentPalette.Length - 1);
            return Color.Lerp(LamentPalette[lo], LamentPalette[hi], scaled - lo);
        }

        /// <summary>
        /// Sudden prismatic flash — mostly dark, but at intervals reveals full rainbow.
        /// </summary>
        public static Color GetGriefFlash(float t)
        {
            float flash = GetGriefFlashIntensity(t);
            if (flash > 0.1f)
            {
                float hue = (t * 2f + (float)Main.GameUpdateCount * 0.005f) % 1f;
                // Desaturated pastel rainbow flash — not vivid, prismatic over white
                return Color.Lerp(GriefGrey, Main.hslToRgb(hue, 0.4f, 0.88f), flash);
            }
            return Color.Lerp(MourningBlack, GriefGrey, t);
        }

        /// <summary>
        /// Returns the raw flash intensity (0-1) for the grief flash effect.
        /// Use this when you need a float, use GetGriefFlash for a Color.
        /// </summary>
        public static float GetGriefFlashIntensity(float t)
        {
            return (float)Math.Pow(Math.Max(0, Math.Sin(t * MathHelper.TwoPi * 3f)), 8);
        }

        public static void BeginAdditive(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public static void RestoreSpriteBatch(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        // ─────────── THEME TEXTURE ACCENTS ───────────

        public static void DrawThemeAccents(SpriteBatch sb, Vector2 worldPos, float scale, float intensity = 1f)
        {
            SwanLakeVFXLibrary.DrawThemeCrystalAccent(sb, worldPos, scale, intensity * 0.5f);
            float rot = (float)Main.GameUpdateCount * 0.02f;
            SwanLakeVFXLibrary.DrawThemeImpactRing(sb, worldPos, scale, intensity * 0.4f, rot);
        }
    }
}
