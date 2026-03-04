using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using MagnumOpus.Content.SwanLake;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.FeatheroftheIridescentFlock.Utilities
{
    /// <summary>
    /// Self-contained utilities for Feather of the Iridescent Flock.
    /// Iridescent = oil-slick rainbow over dark base. Deep black with shifting oil-sheen.
    /// </summary>
    public static class FlockUtils
    {
        public static readonly Color OilBlack = new Color(15, 12, 20);
        public static readonly Color ShellPink = new Color(255, 180, 200);
        public static readonly Color PetalLavender = new Color(200, 170, 240);
        public static readonly Color CrystalAqua = new Color(120, 220, 240);
        public static readonly Color LoreColor = new Color(240, 240, 255);

        public static readonly Color[] IridescentPalette = new Color[]
        {
            new Color(255, 120, 160), // Pink
            new Color(255, 200, 100), // Gold
            new Color(120, 255, 180), // Seafoam
            new Color(100, 180, 255), // Sky
            new Color(200, 120, 255), // Violet
            new Color(255, 100, 200), // Fuchsia
        };

        public static Color GetIridescent(float t)
        {
            t = ((t % 1f) + 1f) % 1f;
            float scaled = t * (IridescentPalette.Length - 1);
            int lo = (int)scaled;
            int hi = Math.Min(lo + 1, IridescentPalette.Length - 1);
            return Color.Lerp(IridescentPalette[lo], IridescentPalette[hi], scaled - lo);
        }

        public static Color GetOilSheen(float angle, float time)
        {
            float t = ((angle / MathHelper.TwoPi) + time * 0.02f) % 1f;
            return Color.Lerp(OilBlack, GetIridescent(t), 0.35f);
        }

        public static void EnterShaderRegion(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public static void BeginAdditive(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp,
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
