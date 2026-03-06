using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using MagnumOpus.Content.LaCampanella;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.InfernalChimesCalling.Utilities
{
    /// <summary>
    /// Self-contained utilities for InfernalChimesCalling — the summoner staff.
    /// Choir bell minion palette and VFX helpers.
    /// </summary>
    public static class InfernalChimesCallingUtils
    {
        public static readonly Color[] ChoirPalette = new Color[]
        {
            new Color(30, 10, 5),     // Shadowed Bell
            new Color(150, 40, 0),    // Bronze Ember
            new Color(255, 110, 15),  // Ringing Orange
            new Color(255, 185, 50),  // Choral Gold
            new Color(255, 225, 130), // Bright Chime
            new Color(255, 250, 220), // Resonant White
        };

        public static readonly Color[] ShockwavePalette = new Color[]
        {
            new Color(40, 10, 0),
            new Color(180, 50, 0),
            new Color(255, 130, 20),
            new Color(255, 200, 60),
            new Color(255, 240, 150),
            new Color(255, 255, 240),
        };

        public static readonly Color LoreColor = new Color(255, 140, 40);

        public static Color MulticolorLerp(float t, params Color[] colors)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            int seg = colors.Length - 1;
            float st = t * seg;
            int idx = (int)st;
            if (idx >= seg) return colors[seg];
            return Color.Lerp(colors[idx], colors[idx + 1], st - idx);
        }

        public static Color GetChoirGradient(float t) => MulticolorLerp(t, ChoirPalette);
        public static Color GetShockwaveGradient(float t) => MulticolorLerp(t, ShockwavePalette);
        public static Color Additive(Color c, float opacity = 1f) => new Color(c.R, c.G, c.B, 0) * opacity;

        public static Color GetChoirFlicker(float offset = 0f)
        {
            float time = (float)Main.timeForVisualEffects * 0.04f + offset * 6.28f;
            float flicker = 0.5f + 0.5f * (float)Math.Sin(time * 2.8f + Math.Sin(time * 1.4f));
            return MulticolorLerp(flicker, ChoirPalette);
        }

        public static void BeginAdditive(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public static void RestoreSpriteBatch(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public static Vector2 SafeDirectionTo(this Vector2 from, Vector2 to)
        {
            Vector2 diff = to - from;
            float len = diff.Length();
            return len < 0.0001f ? Vector2.Zero : diff / len;
        }

        public static NPC ClosestNPCAt(Vector2 position, float maxRange)
        {
            NPC best = null; float bestDist = maxRange;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                float dist = Vector2.Distance(position, npc.Center);
                if (dist < bestDist) { bestDist = dist; best = npc; }
            }
            return best;
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
