using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Content.DiesIrae;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.DiesIrae.Weapons.EclipseOfWrath.Utilities
{
    /// <summary>
    /// Static VFX utility for Eclipse of Wrath.
    /// Dual-render eclipse aesthetic  Edark disc center with ragged fire corona.
    /// Most visually dramatic magic weapon in Dies Irae.
    /// </summary>
    public static class EclipseOfWrathUtils
    {
        public static Color EclipseDark => new Color(15, 5, 5);
        public static Color CoronaEmber => DiesIraePalette.EmberOrange;
        public static Color CoronaGold => DiesIraePalette.JudgmentGold;
        public static Color ShardCrimson => DiesIraePalette.BloodRed;
        public static Color FlashWhite => DiesIraePalette.WrathWhite;

        /// <summary>
        /// Draws the eclipse orb  Edual render: dark disc (alpha blend) + fire corona (additive).
        /// Must be called in two passes.
        /// Pass 1 (AlphaBlend): DrawEclipseDisc()
        /// Pass 2 (Additive): DrawEclipseCorona()
        /// </summary>
        public static void DrawEclipseDisc(SpriteBatch sb, Vector2 worldPos, float timer, float scale = 1f)
        {
            Texture2D glow = MagnumTextureRegistry.GetHardCircle();
            if (glow == null) glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow == null) return;
            Vector2 origin = glow.Size() / 2f;
            Vector2 pos = worldPos - Main.screenPosition;

            // Dark disc  Enear-black, occludes background
            sb.Draw(glow, pos, null, EclipseDark * 0.95f, timer * 0.01f, origin,
                0.04f * scale, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Draws the fire corona ring around the eclipse (additive pass).
        /// </summary>
        public static void DrawEclipseCorona(SpriteBatch sb, Vector2 worldPos, float timer, float scale = 1f)
        {
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow == null) return;
            Vector2 origin = glow.Size() / 2f;
            Vector2 pos = worldPos - Main.screenPosition;

            float pulse = 0.8f + 0.2f * (float)Math.Sin(timer * 0.2f);

            // Corona ring  Elarger than disc, ember-gold
            Texture2D halo = MagnumTextureRegistry.GetHaloRing();
            if (halo != null)
            {
                Vector2 hOrigin = halo.Size() / 2f;
                sb.Draw(halo, pos, null, CoronaEmber * 0.6f * pulse, timer * 0.015f, hOrigin,
                    0.06f * scale, SpriteEffects.None, 0f);
                sb.Draw(halo, pos, null, CoronaGold * 0.3f * pulse, -timer * 0.01f, hOrigin,
                    0.075f * scale, SpriteEffects.None, 0f);
            }

            // Outer flare wisps
            sb.Draw(glow, pos, null, CoronaEmber * 0.3f * pulse, 0f, origin,
                0.08f * scale, SpriteEffects.None, 0f);

            // Spinning cross flare
            Texture2D flare = MagnumTextureRegistry.GetShineFlare4Point();
            if (flare != null)
            {
                Vector2 fOrigin = flare.Size() / 2f;
                sb.Draw(flare, pos, null, CoronaGold * 0.2f * pulse, timer * 0.05f, fOrigin,
                    0.04f * scale, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Draws a wrath shard  Esmall piercing fragment with tight trail.
        /// </summary>
        public static void DrawWrathShardBody(SpriteBatch sb, Vector2 worldPos, float rotation, float timer)
        {
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow == null) return;
            Vector2 origin = glow.Size() / 2f;
            Vector2 pos = worldPos - Main.screenPosition;

            float pulse = 0.85f + 0.15f * (float)Math.Sin(timer * 0.3f);

            // Ember outer
            sb.Draw(glow, pos, null, CoronaEmber * 0.5f * pulse, 0f, origin,
                0.025f, SpriteEffects.None, 0f);
            // Hot core
            sb.Draw(glow, pos, null, FlashWhite * 0.6f * pulse, 0f, origin,
                0.012f, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Eclipse orb split  Eburst VFX when orb fragments.
        /// </summary>
        public static void DoEclipseSplit(Vector2 center)
        {
            if (Main.dedServ) return;

            // Spiral shrapnel burst
            for (int i = 0; i < 30; i++)
            {
                float angle = MathHelper.TwoPi / 30 * i + Main.rand.NextFloat() * 0.2f;
                float speed = 3f + Main.rand.NextFloat() * 4f;
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;

                int dustType = Main.rand.NextBool() ? DustID.Torch : DustID.Smoke;
                Dust d = Dust.NewDustPerfect(center, dustType, vel, dustType == DustID.Smoke ? 150 : 0, default, 1.3f);
                d.noGravity = true;
                d.fadeIn = 0.5f;
            }

            // Center flash
            for (int i = 0; i < 6; i++)
            {
                Dust d = Dust.NewDustPerfect(center, DustID.GoldFlame,
                    Main.rand.NextVector2Circular(2f, 2f), 0, default, 1.5f);
                d.noGravity = true;
            }
        }

        /// <summary>
        /// Eclipse field darkness zone  Elingering dark area.
        /// </summary>
        public static void DrawEclipseField(SpriteBatch sb, Vector2 center, float radius, float life, float timer)
        {
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow == null) return;
            Vector2 origin = glow.Size() / 2f;
            Vector2 pos = center - Main.screenPosition;

            // Dark overlay  Edrawn in alpha blend mode to actually darken
            // Here we draw a dimming layer
            float pulse = 0.9f + 0.1f * (float)Math.Sin(timer * 0.1f);

            // Ember fire rim at edges
            Texture2D halo = MagnumTextureRegistry.GetHaloRing();
            if (halo != null)
            {
                Vector2 hOrigin = halo.Size() / 2f;
                float hScale = radius / (halo.Width * 0.5f);
                sb.Draw(halo, pos, null, CoronaEmber * 0.3f * life * pulse, timer * 0.01f, hOrigin,
                    hScale, SpriteEffects.None, 0f);
            }

            // Center dark glow (additive dark hint)
            sb.Draw(glow, pos, null, ShardCrimson * 0.15f * life, 0f, origin,
                radius / (glow.Width * 0.5f), SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Corona Flare crit explosion  EXSlash fire nova on crit hits.
        /// </summary>
        public static void DoCoronaFlare(Vector2 center)
        {
            if (Main.dedServ) return;

            // Big radial spark burst
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi / 20 * i;
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * (4f + Main.rand.NextFloat() * 4f);
                Dust d = Dust.NewDustPerfect(center, DustID.GoldFlame, vel, 0, default, 1.8f);
                d.noGravity = true;
            }

            // X-pattern flash
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.PiOver4 + MathHelper.PiOver2 * i;
                for (int j = 0; j < 5; j++)
                {
                    Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * (2f + j * 2f);
                    Dust d = Dust.NewDustPerfect(center, DustID.Torch, vel, 0, default, 1.5f);
                    d.noGravity = true;
                }
            }
        }

        // ─────────── THEME TEXTURE ACCENTS ───────────

        /// <summary>
        /// Draws theme-textured eclipse accents. Call under Additive blend.
        /// </summary>
        public static void DrawThemeAccents(SpriteBatch sb, Vector2 worldPos, float scale, float intensity = 1f)
        {
            DiesIraeVFXLibrary.DrawThemeStarFlare(sb, worldPos, scale, intensity * 0.6f);
            float rot = (float)Main.GameUpdateCount * 0.02f;
            DiesIraeVFXLibrary.DrawThemeImpactRing(sb, worldPos, scale, intensity * 0.5f, rot);
            DiesIraeVFXLibrary.DrawCrackedEarthOverlay(sb, worldPos, scale * 0.6f, intensity * 0.3f);
        }
    }
}