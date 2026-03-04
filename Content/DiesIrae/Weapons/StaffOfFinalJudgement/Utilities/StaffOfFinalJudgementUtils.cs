using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.DiesIrae;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

namespace MagnumOpus.Content.DiesIrae.Weapons.StaffOfFinalJudgement.Utilities
{
    /// <summary>
    /// Static VFX utility for Staff of Final Judgement.
    /// Fire mine aesthetic — orbs with state-driven rendering, detonation effects.
    /// </summary>
    public static class StaffOfFinalJudgementUtils
    {
        public static Color MineEmber => DiesIraePalette.EmberOrange;
        public static Color MineArmed => new Color(220, 80, 20);
        public static Color DetCrimson => DiesIraePalette.BloodRed;
        public static Color FlashGold => DiesIraePalette.JudgmentGold;
        public static Color CoreWhite => DiesIraePalette.WrathWhite;

        /// <summary>
        /// Get mine color based on state.
        /// 0 = unarmed (dim ember), 1 = armed (bright ember), 2 = near-trigger (pulsing gold).
        /// </summary>
        public static Color GetMineColor(int state, float timer)
        {
            return state switch
            {
                0 => Color.Lerp(DiesIraePalette.CharcoalBlack, MineEmber, 0.4f),
                1 => Color.Lerp(MineEmber, MineArmed, 0.5f + 0.2f * (float)Math.Sin(timer * 0.1f)),
                2 => Color.Lerp(MineArmed, FlashGold, 0.5f + 0.5f * (float)Math.Sin(timer * 0.5f)),
                _ => MineEmber,
            };
        }

        /// <summary>
        /// Draws a mine body orb — 3-layer bloom with state-driven scaling.
        /// </summary>
        public static void DrawMineBody(SpriteBatch sb, Vector2 worldPos, int state, float timer)
        {
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow == null) return;
            Vector2 origin = glow.Size() / 2f;
            Vector2 pos = worldPos - Main.screenPosition;

            Color color = GetMineColor(state, timer);

            // Scale bloom by state: unarmed small, armed medium, near-trigger large + pulsing
            float baseScale = state switch { 0 => 0.05f, 1 => 0.08f, _ => 0.10f };
            float pulse = state == 2 ? (0.85f + 0.15f * (float)Math.Sin(timer * 0.5f)) : 1f;

            // Outer glow
            sb.Draw(glow, pos, null, color * 0.3f * pulse, 0f, origin,
                baseScale * 1.8f, SpriteEffects.None, 0f);
            // Mid glow
            sb.Draw(glow, pos, null, Color.Lerp(color, FlashGold, 0.3f) * 0.5f * pulse, 0f, origin,
                baseScale, SpriteEffects.None, 0f);
            // Core
            sb.Draw(glow, pos, null, CoreWhite * 0.6f * pulse, 0f, origin,
                baseScale * 0.4f, SpriteEffects.None, 0f);

            // Spinning flare for armed/triggered mines
            if (state >= 1)
            {
                Texture2D flare = MagnumTextureRegistry.GetShineFlare4Point();
                if (flare != null)
                {
                    Vector2 fOrigin = flare.Size() / 2f;
                    float rot = timer * (state == 2 ? 0.08f : 0.03f);
                    sb.Draw(flare, pos, null, FlashGold * 0.3f * pulse, rot, fOrigin,
                        baseScale * 0.6f, SpriteEffects.None, 0f);
                }
            }
        }

        /// <summary>
        /// Draws connecting beams between mines (purgatory field lines).
        /// Simple point-to-point glow line.
        /// </summary>
        public static void DrawFieldLine(SpriteBatch sb, Vector2 from, Vector2 to, float timer)
        {
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow == null) return;
            Vector2 origin = glow.Size() / 2f;

            Vector2 dir = to - from;
            float dist = dir.Length();
            if (dist < 1f) return;
            dir /= dist;

            int segments = (int)(dist / 12f);
            for (int i = 0; i <= segments; i++)
            {
                float t = (float)i / segments;
                Vector2 pos = Vector2.Lerp(from, to, t) - Main.screenPosition;
                float wave = (float)Math.Sin(t * MathHelper.TwoPi * 2f + timer * 0.2f) * 3f;
                Vector2 perp = new Vector2(-dir.Y, dir.X) * wave;
                pos += perp;

                Color c = Color.Lerp(MineEmber, FlashGold, t) * 0.4f;
                sb.Draw(glow, pos, null, c, 0f, origin, 0.012f, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Mine detonation VFX — radial spark burst + smoke ring.
        /// </summary>
        public static void DoDetonation(Vector2 position, bool isChain = false)
        {
            if (Main.dedServ) return;

            int sparkCount = isChain ? 50 : 40;
            float speedMult = isChain ? 1.3f : 1f;

            // Radial fire sparks
            for (int i = 0; i < sparkCount; i++)
            {
                float angle = MathHelper.TwoPi / sparkCount * i + Main.rand.NextFloat() * 0.2f;
                float speed = (3f + Main.rand.NextFloat() * 5f) * speedMult;
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;
                vel.Y -= 1f; // Slight upward bias

                Dust d = Dust.NewDustPerfect(position, Terraria.ID.DustID.Torch, vel, 0, default, 1.4f);
                d.noGravity = true;
                d.fadeIn = 0.6f;
            }

            // Smoke ring
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi / 20f * i;
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 2f;
                Dust d = Dust.NewDustPerfect(position, Terraria.ID.DustID.Smoke, vel, 150, default, 1.5f);
                d.noGravity = true;
            }

            // Center gold flash
            for (int i = 0; i < 8; i++)
            {
                Dust d = Dust.NewDustPerfect(position, Terraria.ID.DustID.GoldFlame,
                    Main.rand.NextVector2Circular(2f, 2f), 0, default, 1.5f);
                d.noGravity = true;
            }
        }

        /// <summary>
        /// Judgment Storm VFX — massive screen-wide fire rain effect.
        /// Called when 3+ mines detonate within 1s.
        /// </summary>
        public static void DoJudgmentStorm(Vector2 epicenter)
        {
            if (Main.dedServ) return;

            // Massive spark shower from above
            for (int i = 0; i < 60; i++)
            {
                Vector2 spawnPos = epicenter + new Vector2(Main.rand.NextFloat(-400f, 400f), -Main.rand.NextFloat(200f, 400f));
                Vector2 vel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(4f, 8f));
                Dust d = Dust.NewDustPerfect(spawnPos, Terraria.ID.DustID.Torch, vel, 0, default, 1.8f);
                d.noGravity = false;
                d.fadeIn = 1f;
            }

            // Center blast
            for (int i = 0; i < 15; i++)
            {
                Dust d = Dust.NewDustPerfect(epicenter, Terraria.ID.DustID.GoldFlame,
                    Main.rand.NextVector2Circular(6f, 6f), 0, default, 2f);
                d.noGravity = true;
            }
        }

        // ─────────── THEME TEXTURE ACCENTS ───────────

        /// <summary>
        /// Draws theme-textured staff accents. Call under Additive blend.
        /// </summary>
        public static void DrawThemeAccents(SpriteBatch sb, Vector2 worldPos, float scale, float intensity = 1f)
        {
            DiesIraeVFXLibrary.DrawThemeStarFlare(sb, worldPos, scale, intensity * 0.5f);
            float rot = (float)Main.GameUpdateCount * 0.02f;
            DiesIraeVFXLibrary.DrawThemeImpactRing(sb, worldPos, scale, intensity * 0.4f, rot);
        }
    }
}
