using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Content.DiesIrae;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.DiesIrae.Weapons.ArbitersSentence.Utilities
{
    /// <summary>
    /// Static VFX utility for Arbiter's Sentence.
    /// Precision flamethrower aesthetic  Etight focused flames, judgment scar marks, sentence cage.
    /// </summary>
    public static class ArbitersSentenceUtils
    {
        // Precision palette  Econtrolled fire, not wild
        public static Color PrecisionCrimson => new Color(180, 20, 15);
        public static Color JudgmentEmber => new Color(240, 100, 30);
        public static Color SentenceGold => DiesIraePalette.JudgmentGold;
        public static Color FocusWhite => DiesIraePalette.WrathWhite;
        public static Color CharcoalBlack => DiesIraePalette.CharcoalBlack;

        /// <summary>
        /// Draws a precision flame bullet body  Etight 3-layer bloom.
        /// Tighter and more controlled than Damnation's Cannon massive blooms.
        /// </summary>
        public static void DrawFlameBulletBody(SpriteBatch sb, Vector2 worldPos, float rotation, float timer, int flameStacks = 0)
        {
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow == null) return;
            Vector2 origin = glow.Size() / 2f;
            Vector2 pos = worldPos - Main.screenPosition;

            // Intensity scales slightly with Judgment Flame stacks on target
            float stackMult = 1f + flameStacks * 0.1f;
            float pulse = 0.9f + 0.1f * (float)Math.Sin(timer * 0.3f);

            // Outer crimson halo  Esmall, focused
            sb.Draw(glow, pos, null, PrecisionCrimson * 0.4f * pulse * stackMult, 0f, origin,
                0.05f, SpriteEffects.None, 0f);
            // Mid ember ring
            sb.Draw(glow, pos, null, JudgmentEmber * 0.6f * pulse * stackMult, 0f, origin,
                0.03f, SpriteEffects.None, 0f);
            // Tight hot core
            sb.Draw(glow, pos, null, FocusWhite * 0.7f * pulse, 0f, origin,
                0.015f, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Draws the Sentence Cage around a sentenced enemy  Etight VoronoiCell-patterned ring.
        /// Burns in crimson-gold cage bars.
        /// </summary>
        public static void DrawSentenceCage(SpriteBatch sb, Vector2 worldPos, float timer, float intensity = 1f)
        {
            Texture2D glow = MagnumTextureRegistry.GetHaloRing();
            if (glow == null) glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow == null) return;

            Vector2 origin = glow.Size() / 2f;
            Vector2 pos = worldPos - Main.screenPosition;

            float cagePulse = 0.8f + 0.2f * (float)Math.Sin(timer * 0.15f);
            float rot = timer * 0.02f;

            // Outer cage ring  Ecrimson
            sb.Draw(glow, pos, null, PrecisionCrimson * 0.5f * intensity * cagePulse, rot, origin,
                0.08f, SpriteEffects.None, 0f);
            // Inner cage ring  Egold
            sb.Draw(glow, pos, null, SentenceGold * 0.6f * intensity * cagePulse, -rot * 0.7f, origin,
                0.055f, SpriteEffects.None, 0f);
            // Core ring  Eember
            sb.Draw(glow, pos, null, JudgmentEmber * 0.7f * intensity, rot * 1.3f, origin,
                0.035f, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Draws flame scar marks on a hit enemy  Evisual indicator of Judgment Flame stacks.
        /// </summary>
        public static void DrawFlameScars(SpriteBatch sb, Vector2 worldPos, int stackCount, float timer)
        {
            Texture2D streak = MagnumTextureRegistry.GetBeamStreak();
            if (streak == null) streak = MagnumTextureRegistry.GetSoftGlow();
            if (streak == null) return;

            Vector2 origin = streak.Size() / 2f;
            Vector2 pos = worldPos - Main.screenPosition;

            for (int i = 0; i < stackCount; i++)
            {
                float angle = MathHelper.TwoPi / 5f * i + timer * 0.01f;
                float offset = 12f + 4f * (float)Math.Sin(timer * 0.1f + i * 1.2f);
                Vector2 scarPos = pos + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * offset;

                float scarRot = angle + MathHelper.PiOver4;
                Color scarColor = Color.Lerp(PrecisionCrimson, SentenceGold, (float)i / 5f) * 0.7f;

                sb.Draw(streak, scarPos, null, scarColor, scarRot, origin,
                    new Vector2(0.03f, 0.008f), SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Flame impact burst  Etight precision impact, not explosive.
        /// </summary>
        public static void DoFlameImpact(Vector2 position, int intensity = 1)
        {
            // === Color-ramped sparkle explosion VFX ===
            DiesIraeVFXLibrary.SpawnColorRampedSparkleExplosion(position, 8, 5f, 0.3f);

            // === Color-ramped sparkle explosion VFX ===
            DiesIraeVFXLibrary.SpawnColorRampedSparkleExplosion(position, 8, 5f, 0.3f);

            if (Main.dedServ) return;

            // Directional sparks  Efewer, tighter than Damnation's Cannon
            int sparkCount = 6 + intensity * 2;
            for (int i = 0; i < sparkCount; i++)
            {
                float angle = MathHelper.TwoPi / sparkCount * i + Main.rand.NextFloat() * 0.3f;
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * (2f + Main.rand.NextFloat() * 3f);
                Dust d = Dust.NewDustPerfect(position, DustID.Torch, vel, 0, default, 1.2f);
                d.noGravity = true;
                d.fadeIn = 0.6f;
            }

            // Small gold center flash
            for (int i = 0; i < 3; i++)
            {
                Dust d = Dust.NewDustPerfect(position, DustID.GoldFlame, Vector2.Zero, 0, default, 0.8f);
                d.noGravity = true;
                d.velocity = Main.rand.NextVector2Circular(1f, 1f);
            }
        }

        /// <summary>
        /// Flame transfer visual  Ewhen a sentenced enemy dies and flame jumps to nearest target.
        /// Spawns a wisp trail of embers between two points.
        /// </summary>
        public static void DoFlameTransfer(Vector2 fromPos, Vector2 toPos)
        {
            if (Main.dedServ) return;

            Vector2 dir = toPos - fromPos;
            float dist = dir.Length();
            if (dist < 1f) return;
            dir /= dist;

            int steps = (int)(dist / 8f);
            for (int i = 0; i < steps; i++)
            {
                float t = (float)i / steps;
                Vector2 pos = Vector2.Lerp(fromPos, toPos, t);
                Vector2 offset = new Vector2(Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f));

                Color c = Color.Lerp(PrecisionCrimson, SentenceGold, t);
                Dust d = Dust.NewDustPerfect(pos + offset, DustID.Torch, dir * 2f, 0, default, 1f - t * 0.5f);
                d.noGravity = true;
                d.fadeIn = 0.3f;
            }

            // Pickup flash at destination
            for (int i = 0; i < 5; i++)
            {
                Dust d = Dust.NewDustPerfect(toPos, DustID.GoldFlame, Main.rand.NextVector2Circular(3f, 3f), 0, default, 1.2f);
                d.noGravity = true;
            }
        }

        /// <summary>
        /// Focus crosshair dust  Eglow particles forming a subtle crosshair on focused target.
        /// </summary>
        public static void SpawnFocusDust(Vector2 targetPos)
        {
            if (Main.dedServ) return;

            // 4 cardinal points forming a crosshair
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.PiOver2 * i;
                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 16f;
                Dust d = Dust.NewDustPerfect(targetPos + offset, DustID.GoldFlame, -offset * 0.05f, 0, default, 0.6f);
                d.noGravity = true;
                d.fadeIn = 0.5f;
            }
        }

        /// <summary>
        /// Ember ground residue  Esmall lingering ember particles for purgatory embers.
        /// </summary>
        public static void SpawnPurgatoryEmberDust(Vector2 position, float radius = 20f)
        {
            if (Main.dedServ) return;

            if (Main.rand.NextBool(3))
            {
                Vector2 offset = Main.rand.NextVector2Circular(radius, radius * 0.5f);
                Dust d = Dust.NewDustPerfect(position + offset, DustID.Torch,
                    new Vector2(0f, -Main.rand.NextFloat(0.5f, 1.5f)), 0, default, 0.8f);
                d.noGravity = true;
                d.fadeIn = 0.4f;
            }
            if (Main.rand.NextBool(5))
            {
                Dust d = Dust.NewDustPerfect(position + Main.rand.NextVector2Circular(radius, radius * 0.3f),
                    DustID.Smoke, new Vector2(0f, -0.3f), 100, default, 0.5f);
                d.noGravity = true;
            }
        }

        // ─────────── THEME TEXTURE ACCENTS ───────────

        /// <summary>
        /// Draws theme-textured flame accents. Call under Additive blend.
        /// </summary>
        public static void DrawThemeAccents(SpriteBatch sb, Vector2 worldPos, float scale, float intensity = 1f)
        {
            DiesIraeVFXLibrary.DrawThemeStarFlare(sb, worldPos, scale, intensity * 0.5f);
            float rot = (float)Main.GameUpdateCount * 0.02f;
            DiesIraeVFXLibrary.DrawThemeImpactRing(sb, worldPos, scale, intensity * 0.4f, rot);
        }
    }
}