using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using MagnumOpus.Content.LaCampanella;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.FangOfTheInfiniteBell.Utilities
{
    /// <summary>
    /// Self-contained utilities for FangOfTheInfiniteBell — the arcane bell staff.
    /// Mystical violet-flame palette with golden empowerment accents.
    /// </summary>
    public static class FangOfTheInfiniteBellUtils
    {
        #region Color Palette — Arcane Bell

        /// <summary>Normal mode: dark violet → deep fire → amber → gold core</summary>
        public static readonly Color[] ArcanePalette = new Color[]
        {
            new Color(25, 10, 30),    // Void Violet
            new Color(90, 20, 60),    // Deep Magenta
            new Color(180, 50, 20),   // Arcane Fire
            new Color(255, 140, 30),  // Bell Gold
            new Color(255, 210, 100), // Bright Amber
            new Color(255, 245, 210), // Holy Flash
        };

        /// <summary>Empowered mode: electric gold → crackling white → lightning blue highlights</summary>
        public static readonly Color[] EmpoweredPalette = new Color[]
        {
            new Color(60, 40, 0),     // Dark Gold
            new Color(200, 150, 0),   // Rich Gold
            new Color(255, 220, 50),  // Electric Gold
            new Color(255, 255, 150), // Lightning Yellow
            new Color(200, 230, 255), // Electric Blue
            new Color(255, 255, 255), // Pure White
        };

        public static readonly Color LoreColor = new Color(255, 140, 40);

        #endregion

        #region Color Helpers

        public static Color MulticolorLerp(float t, params Color[] colors)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            int segments = colors.Length - 1;
            float scaledT = t * segments;
            int index = (int)scaledT;
            if (index >= segments) return colors[segments];
            float localT = scaledT - index;
            return Color.Lerp(colors[index], colors[index + 1], localT);
        }

        public static Color GetArcaneGradient(float t) => MulticolorLerp(t, ArcanePalette);
        public static Color GetEmpoweredGradient(float t) => MulticolorLerp(t, EmpoweredPalette);

        public static Color Additive(Color c, float opacity = 1f) => new Color(c.R, c.G, c.B, 0) * opacity;

        public static Color GetArcaneFlicker(float offset = 0f)
        {
            float time = (float)Main.timeForVisualEffects * 0.05f + offset * 6.28f;
            float flicker = 0.5f + 0.5f * (float)Math.Sin(time * 3.2f + Math.Sin(time * 1.7f));
            return MulticolorLerp(flicker, ArcanePalette);
        }

        public static Color GetEmpoweredFlicker(float offset = 0f)
        {
            float time = (float)Main.timeForVisualEffects * 0.07f + offset * 6.28f;
            float flicker = 0.5f + 0.5f * (float)Math.Sin(time * 5f + Math.Sin(time * 2.3f));
            return MulticolorLerp(flicker, EmpoweredPalette);
        }

        #endregion

        #region SpriteBatch Helpers

        public static void BeginAdditive(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public static void RestoreSpriteBatch(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        #endregion

        #region Geometry Helpers

        public static Vector2 SafeDirectionTo(this Vector2 from, Vector2 to)
        {
            Vector2 diff = to - from;
            float len = diff.Length();
            return len < 0.0001f ? Vector2.Zero : diff / len;
        }

        public static NPC ClosestNPCAt(Vector2 position, float maxRange)
        {
            NPC best = null;
            float bestDist = maxRange;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                float dist = Vector2.Distance(position, npc.Center);
                if (dist < bestDist) { bestDist = dist; best = npc; }
            }
            return best;
        }

        #endregion

        // ─────────── THEME TEXTURE ACCENTS ───────────

        public static void DrawThemeAccents(SpriteBatch sb, Vector2 worldPos, float scale, float intensity = 1f)
        {
            LaCampanellaVFXLibrary.DrawThemeStarFlare(sb, worldPos, scale, intensity * 0.5f);
            float rot = (float)Main.GameUpdateCount * 0.02f;
            LaCampanellaVFXLibrary.DrawThemeImpactRing(sb, worldPos, scale, intensity * 0.4f, rot);
        }
    }
}
