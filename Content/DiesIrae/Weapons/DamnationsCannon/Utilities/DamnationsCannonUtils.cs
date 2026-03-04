using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Content.DiesIrae;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.DiesIrae.Weapons.DamnationsCannon.Utilities
{
    /// <summary>
    /// Static VFX utility for Damnation's Cannon.
    /// Heavy artillery aesthetic  Emassive blooms, thick trails, apocalyptic fire.
    /// </summary>
    public static class DamnationsCannonUtils
    {
        public static Color WrathRed => new Color(220, 40, 20);
        public static Color HellfireWhite => DiesIraePalette.WrathWhite;
        public static Color EmberOrange => DiesIraePalette.EmberOrange;
        public static Color JudgmentGold => DiesIraePalette.JudgmentGold;
        public static Color CharcoalBlack => DiesIraePalette.CharcoalBlack;

        /// <summary>
        /// Draws the wrath ball body  Elarge 3-layer bloom orb with churning fire look.
        /// </summary>
        public static void DrawWrathBallBody(SpriteBatch sb, Vector2 worldPos, float timer, float scale = 1f)
        {
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow == null) return;
            Vector2 origin = glow.Size() / 2f;
            Vector2 pos = worldPos - Main.screenPosition;

            float pulse = 0.85f + 0.15f * (float)Math.Sin(timer * 0.25f);
            float corePulse = 0.7f + 0.3f * (float)Math.Sin(timer * 0.35f + 1f);

            // BIG outer glow  Ewrath red
            sb.Draw(glow, pos, null, WrathRed * 0.35f * pulse, 0f, origin,
                0.15f * scale, SpriteEffects.None, 0f);
            // Mid ember corona
            sb.Draw(glow, pos, null, EmberOrange * 0.55f * pulse, 0f, origin,
                0.09f * scale, SpriteEffects.None, 0f);
            // Gold-white core
            sb.Draw(glow, pos, null, HellfireWhite * 0.6f * corePulse, 0f, origin,
                0.04f * scale, SpriteEffects.None, 0f);

            // Radial flare
            Texture2D radial = MagnumTextureRegistry.GetRadialBloom();
            if (radial != null)
            {
                Vector2 rOrigin = radial.Size() / 2f;
                sb.Draw(radial, pos, null, EmberOrange * 0.3f * pulse,
                    timer * 0.03f, rOrigin, 0.07f * scale, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Draws a bloom for the shrapnel projectiles  Esmaller, faster.
        /// </summary>
        public static void DrawShrapnelBloom(SpriteBatch sb, Vector2 worldPos, float timer)
        {
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow == null) return;
            Vector2 origin = glow.Size() / 2f;
            Vector2 pos = worldPos - Main.screenPosition;

            float p = 0.7f + 0.3f * (float)Math.Sin(timer * 0.3f);

            sb.Draw(glow, pos, null, WrathRed * 0.4f * p, 0f, origin, 0.04f, SpriteEffects.None, 0f);
            sb.Draw(glow, pos, null, EmberOrange * 0.55f * p, 0f, origin, 0.025f, SpriteEffects.None, 0f);
            sb.Draw(glow, pos, null, JudgmentGold * 0.6f * p, 0f, origin, 0.012f, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Massive explosion dust burst  Efire rain + smoke ring.
        /// </summary>
        public static void DoExplosion(Vector2 worldPos, int sparkCount = 40)
        {
            if (Main.dedServ) return;

            // Fire rain sparks (upward cone, heavy gravity)
            for (int i = 0; i < sparkCount; i++)
            {
                float angle = -MathHelper.PiOver2 + Main.rand.NextFloat(-0.8f, 0.8f);
                float speed = Main.rand.NextFloat(4f, 10f);
                Vector2 vel = angle.ToRotationVector2() * speed;

                Color c = Main.rand.Next(4) switch
                {
                    0 => EmberOrange,
                    1 => WrathRed,
                    2 => JudgmentGold,
                    _ => CharcoalBlack
                };

                Dust d = Dust.NewDustPerfect(worldPos + Main.rand.NextVector2Circular(12, 12),
                    DustID.Torch, vel, 0, c, Main.rand.NextFloat(1.0f, 1.8f));
                d.noGravity = false; // let them rain down
                d.fadeIn = 1.2f;
            }

            // Smoke ring
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi * i / 20f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                Dust s = Dust.NewDustPerfect(worldPos, DustID.Smoke, vel, 100,
                    new Color(40, 30, 30), Main.rand.NextFloat(1.5f, 2.5f));
                s.noGravity = true;
            }

            // Center flash
            for (int i = 0; i < 8; i++)
            {
                Dust f = Dust.NewDustPerfect(worldPos, DustID.GoldFlame,
                    Main.rand.NextVector2Circular(2, 2), 0, JudgmentGold, 2.0f);
                f.noGravity = true;
            }
        }

        /// <summary>
        /// Spawns the persistent hellfire zone dust ring.
        /// Called per-frame for the zone's lifetime.
        /// </summary>
        public static void SpawnHellfireZoneDust(Vector2 worldPos, float radius)
        {
            if (Main.dedServ || Main.rand.NextBool(3)) return;

            float angle = Main.rand.NextFloat(MathHelper.TwoPi);
            float dist = Main.rand.NextFloat(radius * 0.3f, radius);
            Vector2 dustPos = worldPos + angle.ToRotationVector2() * dist;

            Dust d = Dust.NewDustPerfect(dustPos, DustID.Torch,
                new Vector2(0, Main.rand.NextFloat(-2f, -0.5f)),
                0, Main.rand.NextBool() ? EmberOrange : WrathRed,
                Main.rand.NextFloat(0.6f, 1.2f));
            d.noGravity = true;
        }

        // ─────────── THEME TEXTURE ACCENTS ───────────

        /// <summary>
        /// Draws theme-textured cannon VFX accents. Call under Additive blend.
        /// </summary>
        public static void DrawThemeAccents(SpriteBatch sb, Vector2 worldPos, float scale, float intensity = 1f)
        {
            DiesIraeVFXLibrary.DrawThemeStarFlare(sb, worldPos, scale, intensity * 0.5f);
            float rot = (float)Main.GameUpdateCount * 0.02f;
            DiesIraeVFXLibrary.DrawThemeImpactRing(sb, worldPos, scale, intensity * 0.4f, rot);
            DiesIraeVFXLibrary.DrawCrackedEarthOverlay(sb, worldPos, scale * 0.5f, intensity * 0.25f);
        }
    }
}