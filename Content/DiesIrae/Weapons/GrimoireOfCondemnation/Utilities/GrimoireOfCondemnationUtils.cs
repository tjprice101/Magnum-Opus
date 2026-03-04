using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Content.DiesIrae;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.DiesIrae.Weapons.GrimoireOfCondemnation.Utilities
{
    /// <summary>
    /// Static VFX utility for Grimoire of Condemnation.
    /// Dark channeled beam aesthetic  Ewidening cone, ritualistic sigils, condemnation flames.
    /// </summary>
    public static class GrimoireOfCondemnationUtils
    {
        public static Color CondemnCrimson => new Color(160, 15, 15);
        public static Color DarkSermonRed => DiesIraePalette.BloodRed;
        public static Color RitualEmber => DiesIraePalette.EmberOrange;
        public static Color FlashGold => DiesIraePalette.JudgmentGold;
        public static Color CrimsonVeil => new Color(80, 5, 5);

        /// <summary>
        /// Gets beam color based on channel duration  Eramps from crimson through to gold-white.
        /// </summary>
        public static Color GetBeamColor(float channelProgress)
        {
            if (channelProgress < 0.5f)
                return Color.Lerp(CondemnCrimson, RitualEmber, channelProgress * 2f);
            return Color.Lerp(RitualEmber, FlashGold, (channelProgress - 0.5f) * 2f);
        }

        /// <summary>
        /// Draws the condemnation beam body as a series of glow segments.
        /// Width widens over channel time (80 ↁE120px equivalent).
        /// </summary>
        public static void DrawCondemnationBeam(SpriteBatch sb, Vector2 start, Vector2 end, float channelProgress, float timer)
        {
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow == null) return;
            Vector2 origin = glow.Size() / 2f;

            Vector2 dir = end - start;
            float dist = dir.Length();
            if (dist < 1f) return;
            dir /= dist;

            float widthMult = 0.8f + 0.4f * channelProgress; // 80% ↁE120% width
            int segments = Math.Max(4, (int)(dist / 10f));
            Color beamColor = GetBeamColor(channelProgress);

            for (int i = 0; i <= segments; i++)
            {
                float t = (float)i / segments;
                Vector2 segPos = Vector2.Lerp(start, end, t) - Main.screenPosition;

                // Noise offset for ragged edges
                float noiseOffset = (float)Math.Sin(t * 8f + timer * 0.3f) * 4f * widthMult;
                Vector2 perp = new Vector2(-dir.Y, dir.X) * noiseOffset;
                segPos += perp;

                float scaleY = 0.015f * widthMult;
                float alpha = 0.6f + 0.2f * (float)Math.Sin(t * MathHelper.Pi);

                // Outer crimson
                sb.Draw(glow, segPos, null, CondemnCrimson * 0.3f * alpha, 0f, origin,
                    new Vector2(0.02f, scaleY * 2f), SpriteEffects.None, 0f);
                // Mid beam body
                sb.Draw(glow, segPos, null, beamColor * 0.5f * alpha, 0f, origin,
                    new Vector2(0.015f, scaleY), SpriteEffects.None, 0f);
                // Core line
                sb.Draw(glow, segPos, null, DiesIraePalette.WrathWhite * 0.4f * alpha, 0f, origin,
                    new Vector2(0.008f, scaleY * 0.4f), SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Draws the Dark Sermon ritual circle  Esigil ring with ember fire.
        /// </summary>
        public static void DrawSermonCircle(SpriteBatch sb, Vector2 center, float radius, float buildProgress, float timer)
        {
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow == null) return;
            Vector2 origin = glow.Size() / 2f;

            int ringPoints = 36;
            float rot = timer * 0.02f;

            for (int i = 0; i < ringPoints; i++)
            {
                float angle = MathHelper.TwoPi / ringPoints * i + rot;
                float displayPortion = buildProgress * ringPoints;
                if (i > displayPortion) break; // Only draw built portions

                Vector2 pos = center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                pos -= Main.screenPosition;

                float intensity = 0.4f + 0.3f * buildProgress;
                float pulse = 0.9f + 0.1f * (float)Math.Sin(angle * 3f + timer * 0.15f);

                // Outer ring glow
                sb.Draw(glow, pos, null, CondemnCrimson * intensity * pulse, 0f, origin,
                    0.02f, SpriteEffects.None, 0f);
                // Inner ring
                sb.Draw(glow, pos, null, FlashGold * (intensity * 0.6f) * pulse, 0f, origin,
                    0.01f, SpriteEffects.None, 0f);
            }

            // Center sigil glow
            if (buildProgress > 0.5f)
            {
                Vector2 cPos = center - Main.screenPosition;
                float centerIntensity = (buildProgress - 0.5f) * 2f;

                Texture2D halo = MagnumTextureRegistry.GetHaloRing();
                if (halo != null)
                {
                    Vector2 hOrigin = halo.Size() / 2f;
                    sb.Draw(halo, cPos, null, CondemnCrimson * 0.2f * centerIntensity, rot * 0.5f, hOrigin,
                        radius / halo.Width * 2f, SpriteEffects.None, 0f);
                }
            }
        }

        /// <summary>
        /// Sermon circle detonation  Emassive 8-ring expansion burst.
        /// </summary>
        public static void DoSermonDetonation(Vector2 center, float radius)
        {
            if (Main.dedServ) return;

            // Massive spark burst
            for (int i = 0; i < 50; i++)
            {
                float angle = MathHelper.TwoPi / 50 * i;
                float speed = 4f + Main.rand.NextFloat() * 6f;
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;

                Dust d = Dust.NewDustPerfect(center, DustID.Torch, vel, 0, default, 1.8f);
                d.noGravity = true;
                d.fadeIn = 1f;
            }

            // Ring fire at boundary
            for (int i = 0; i < 30; i++)
            {
                float angle = MathHelper.TwoPi / 30 * i;
                Vector2 pos = center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                Dust d = Dust.NewDustPerfect(pos, DustID.GoldFlame, Main.rand.NextVector2Circular(3f, 3f), 0, default, 1.5f);
                d.noGravity = true;
            }

            // Center gold flash
            for (int i = 0; i < 12; i++)
            {
                Dust d = Dust.NewDustPerfect(center, DustID.GoldFlame,
                    Main.rand.NextVector2Circular(5f, 5f), 0, default, 2f);
                d.noGravity = true;
            }

            // Smoke
            for (int i = 0; i < 20; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                Dust d = Dust.NewDustPerfect(center + Main.rand.NextVector2Circular(radius, radius),
                    DustID.Smoke, vel, 200, default, 1.5f);
                d.noGravity = true;
            }
        }

        /// <summary>
        /// Page Turn VFX  Eenhanced 7th cast visual.
        /// </summary>
        public static void DoPageTurn(Vector2 position)
        {
            if (Main.dedServ) return;

            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi / 8 * i;
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 3f;
                Dust d = Dust.NewDustPerfect(position, DustID.GoldFlame, vel, 0, default, 1.5f);
                d.noGravity = true;
            }
        }

        // ─────────── THEME TEXTURE ACCENTS ───────────

        /// <summary>
        /// Draws theme-textured accents. Call under Additive blend.
        /// </summary>
        public static void DrawThemeAccents(SpriteBatch sb, Vector2 worldPos, float scale, float intensity = 1f)
        {
            DiesIraeVFXLibrary.DrawThemeStarFlare(sb, worldPos, scale, intensity * 0.5f);
            float rot = (float)Main.GameUpdateCount * 0.02f;
            DiesIraeVFXLibrary.DrawThemeImpactRing(sb, worldPos, scale, intensity * 0.4f, rot);
        }
    }
}