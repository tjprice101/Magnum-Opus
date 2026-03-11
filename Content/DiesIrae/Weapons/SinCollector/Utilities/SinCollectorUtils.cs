using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.DiesIrae;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;

namespace MagnumOpus.Content.DiesIrae.Weapons.SinCollector.Utilities
{
    /// <summary>
    /// Static VFX utility class for Sin Collector.
    /// Palette progression: as sin count rises, colors shift from muted crimson to
    /// blazing corruption orange-gold. Sin corrupts the visuals.
    /// </summary>
    public static class SinCollectorUtils
    {
        // ═══════════════════════════════════════════════════════════════
        //  SIN PALETTE — escalates with sin count
        // ═══════════════════════════════════════════════════════════════

        public static Color SinCrimson => DiesIraePalette.BloodRed;
        public static Color EmberOrange => DiesIraePalette.EmberOrange;
        public static Color JudgmentGold => DiesIraePalette.JudgmentGold;
        public static Color CorruptionVein => new Color(100, 10, 10);

        /// <summary>
        /// Gets a color that intensifies with sin count.
        /// Low sin: muted crimson. High sin: blazing gold.
        /// </summary>
        public static Color GetSinColor(int sinCount, float extraPulse = 0f)
        {
            float t = MathHelper.Clamp(sinCount / 30f, 0f, 1f) + extraPulse;
            t = MathHelper.Clamp(t, 0f, 1f);

            if (t < 0.33f)
                return Color.Lerp(DiesIraePalette.CharcoalBlack, SinCrimson, t / 0.33f);
            if (t < 0.66f)
                return Color.Lerp(SinCrimson, EmberOrange, (t - 0.33f) / 0.33f);
            return Color.Lerp(EmberOrange, JudgmentGold, (t - 0.66f) / 0.34f);
        }

        // ═══════════════════════════════════════════════════════════════
        //  VFX DRAWING HELPERS
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Draws a multi-layer bloom for a sin bullet.
        /// Tight, focused — precision weapon aesthetic.
        /// </summary>
        public static void DrawBulletBloom(SpriteBatch sb, Vector2 worldPos, float timer, int sinCount = 0)
        {
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow == null) return;
            Vector2 origin = glow.Size() / 2f;
            Vector2 pos = worldPos - Main.screenPosition;

            float sinIntensity = 1f + sinCount / 30f * 0.5f;
            float pulse = 0.8f + 0.2f * (float)Math.Sin(timer * 0.2f);
            float i = sinIntensity * pulse;

            // Outer crimson
            sb.Draw(glow, pos, null, SinCrimson * 0.35f * i, 0f, origin, 0.05f * sinIntensity, SpriteEffects.None, 0f);
            // Mid ember
            sb.Draw(glow, pos, null, EmberOrange * 0.5f * i, 0f, origin, 0.03f * sinIntensity, SpriteEffects.None, 0f);
            // Gold core
            sb.Draw(glow, pos, null, JudgmentGold * 0.7f * i, 0f, origin, 0.015f * sinIntensity, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Spawns sin fragment wisps from enemy to player — visual for sin collection.
        /// </summary>
        public static void SpawnSinFragmentDust(Vector2 enemyPos, Vector2 playerPos)
        {
            if (Main.dedServ) return;

            Vector2 toPlayer = (playerPos - enemyPos).SafeNormalize(Vector2.UnitX);
            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = toPlayer * Main.rand.NextFloat(4f, 8f) + Main.rand.NextVector2Circular(1.5f, 1.5f);
                Dust d = Dust.NewDustPerfect(
                    enemyPos + Main.rand.NextVector2Circular(10, 10),
                    DustID.Torch, vel, 0,
                    Color.Lerp(SinCrimson, EmberOrange, Main.rand.NextFloat()), 1.0f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }
        }

        /// <summary>
        /// Draws a bloom stack for expenditure projectiles (larger presence).
        /// </summary>
        public static void DrawExpendBloom(SpriteBatch sb, Vector2 worldPos, float scale, float intensity)
        {
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            Texture2D radial = MagnumTextureRegistry.GetRadialBloom();
            if (glow == null) return;
            Vector2 origin = glow.Size() / 2f;
            Vector2 pos = worldPos - Main.screenPosition;

            sb.Draw(glow, pos, null, DiesIraePalette.BloodRed * 0.3f * intensity, 0f, origin, scale * 1.5f, SpriteEffects.None, 0f);
            sb.Draw(glow, pos, null, EmberOrange * 0.5f * intensity, 0f, origin, scale * 1.0f, SpriteEffects.None, 0f);
            sb.Draw(glow, pos, null, JudgmentGold * 0.7f * intensity, 0f, origin, scale * 0.55f, SpriteEffects.None, 0f);
            sb.Draw(glow, pos, null, DiesIraePalette.WrathWhite * 0.4f * intensity, 0f, origin, scale * 0.25f, SpriteEffects.None, 0f);

            if (radial != null)
            {
                Vector2 rOrigin = radial.Size() / 2f;
                sb.Draw(radial, pos, null, EmberOrange * 0.35f * intensity,
                    (float)Main.timeForVisualEffects * 0.02f, rOrigin, scale * 0.8f, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Impact burst for sin bullet hits — directional crimson sparks.
        /// </summary>
        public static void DoBulletImpact(Vector2 worldPos, Vector2 hitDir)
        {
            // === Color-ramped sparkle explosion VFX ===
            DiesIraeVFXLibrary.SpawnColorRampedSparkleExplosion(worldPos, 8, 5f, 0.3f);

            if (Main.dedServ) return;

            for (int i = 0; i < 8; i++)
            {
                float angle = hitDir.ToRotation() + Main.rand.NextFloat(-0.6f, 0.6f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                Dust d = Dust.NewDustPerfect(worldPos, DustID.Torch, vel, 0,
                    Main.rand.NextBool() ? SinCrimson : EmberOrange, 0.9f);
                d.noGravity = true;
            }
        }

        // ─────────── THEME TEXTURE ACCENTS ───────────

        /// <summary>
        /// Draws theme-textured bullet impact accents. Call under Additive blend.
        /// </summary>
        public static void DrawThemeAccents(SpriteBatch sb, Vector2 worldPos, float scale, float intensity = 1f)
        {
            DiesIraeVFXLibrary.DrawThemeStarFlare(sb, worldPos, scale, intensity * 0.4f);
            float rot = (float)Main.GameUpdateCount * 0.025f;
            DiesIraeVFXLibrary.DrawThemeImpactRing(sb, worldPos, scale * 0.8f, intensity * 0.35f, rot);
        }
    }
}
