using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Content.DiesIrae;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.DiesIrae.Weapons.ChainOfJudgment.Utilities
{
    /// <summary>
    /// Static VFX utility class for Chain of Judgment.
    /// Metallic chain palette  Edark iron body with ember-orange heat glow between links.
    /// Heavier, industrial aesthetic compared to the fiery Wrath's Cleaver.
    /// </summary>
    public static class ChainOfJudgmentUtils
    {
        // ══════════════════════════════════════════════════════════════╁E
        //  CHAIN-SPECIFIC PALETTE  EIron + Heat + Gold judgment
        // ══════════════════════════════════════════════════════════════╁E

        /// <summary>Dark iron  Ethe cold metal body of chain links.</summary>
        public static readonly Color IronGrey = new Color(60, 55, 50);

        /// <summary>Heated iron  Elinks glowing from friction and hellfire.</summary>
        public static readonly Color HeatedIron = new Color(120, 70, 40);

        /// <summary>Swing palette: iron ↁEheated ↁEember ↁEgold ↁEbone ↁEwrath core.</summary>
        public static readonly Color[] ChainPalette = new Color[]
        {
            IronGrey,
            HeatedIron,
            DiesIraePalette.EmberOrange,
            DiesIraePalette.JudgmentGold,
            DiesIraePalette.BoneWhite,
            DiesIraePalette.WrathWhite
        };

        // Convenience
        public static Color EmberOrange => DiesIraePalette.EmberOrange;
        public static Color JudgmentGold => DiesIraePalette.JudgmentGold;
        public static Color BloodRed => DiesIraePalette.BloodRed;

        // ══════════════════════════════════════════════════════════════╁E
        //  PALETTE INTERPOLATION
        // ══════════════════════════════════════════════════════════════╁E

        /// <summary>Sample chain palette at position t ∁E[0,1]. 0 = cold iron, 1 = wrath-hot core.</summary>
        public static Color GetPaletteColor(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            int count = ChainPalette.Length;
            float scaledT = t * (count - 1);
            int lower = (int)scaledT;
            int upper = Math.Min(lower + 1, count - 1);
            float frac = scaledT - lower;
            return Color.Lerp(ChainPalette[lower], ChainPalette[upper], frac);
        }

        // ══════════════════════════════════════════════════════════════╁E
        //  VFX DRAWING HELPERS
        // ══════════════════════════════════════════════════════════════╁E

        /// <summary>
        /// Draws a multi-layer bloom stack centered at worldPosition.
        /// 4 layers: outer iron aura ↁEblood red mid ↁEember body ↁEgold core.
        /// </summary>
        public static void DrawBloomStack(SpriteBatch sb, Vector2 worldPosition, float intensity = 1f, float baseScale = 0.10f)
        {
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow == null) return;
            Vector2 origin = glow.Size() / 2f;
            Vector2 pos = worldPosition - Main.screenPosition;

            sb.Draw(glow, pos, null, IronGrey * 0.25f * intensity, 0f, origin, baseScale * 1.6f, SpriteEffects.None, 0f);
            sb.Draw(glow, pos, null, BloodRed * 0.35f * intensity, 0f, origin, baseScale * 1.2f, SpriteEffects.None, 0f);
            sb.Draw(glow, pos, null, EmberOrange * 0.55f * intensity, 0f, origin, baseScale * 0.85f, SpriteEffects.None, 0f);
            sb.Draw(glow, pos, null, JudgmentGold * 0.7f * intensity, 0f, origin, baseScale * 0.45f, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Draws a tip glow with radial flare at the chain's end.
        /// </summary>
        public static void DrawTipBloom(SpriteBatch sb, Vector2 worldPosition, float timer, float pulse = 1f)
        {
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            Texture2D radial = MagnumTextureRegistry.GetRadialBloom();
            if (glow == null) return;

            Vector2 glowOrigin = glow.Size() / 2f;
            Vector2 drawPos = worldPosition - Main.screenPosition;

            float p = 0.7f + 0.3f * (float)Math.Sin(timer * 0.15f) * pulse;

            // Outer iron radiance
            sb.Draw(glow, drawPos, null, IronGrey * 0.3f * p, 0f, glowOrigin, 0.08f, SpriteEffects.None, 0f);
            // Mid ember
            sb.Draw(glow, drawPos, null, EmberOrange * 0.55f * p, 0f, glowOrigin, 0.05f, SpriteEffects.None, 0f);
            // Gold core
            sb.Draw(glow, drawPos, null, JudgmentGold * 0.7f * p, 0f, glowOrigin, 0.03f, SpriteEffects.None, 0f);

            // Radial flare
            if (radial != null)
            {
                Vector2 radOrigin = radial.Size() / 2f;
                sb.Draw(radial, drawPos, null, EmberOrange * 0.4f * p, timer * 0.02f, radOrigin, 0.04f, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Renders the chain body as segmented glow points between two world positions.
        /// Jitter offsets simulate chain links rattling.
        /// </summary>
        public static void DrawChainBody(SpriteBatch sb, Vector2 startWorld, Vector2 endWorld, float timer)
        {
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow == null) return;
            Vector2 origin = glow.Size() / 2f;

            float totalDist = Vector2.Distance(startWorld, endWorld);
            int segCount = Math.Max(3, (int)(totalDist / 14f));

            for (int i = 0; i < segCount; i++)
            {
                float t = i / (float)segCount;
                Vector2 basePos = Vector2.Lerp(startWorld, endWorld, t);

                // Chain link jitter
                float jX = (float)Math.Sin(t * 15f + timer * 0.3f) * 3f;
                float jY = (float)Math.Cos(t * 12f + timer * 0.25f) * 3f;
                Vector2 pos = basePos + new Vector2(jX, jY) - Main.screenPosition;

                float sizeMult = MathHelper.Lerp(1.0f, 0.55f, t);
                float heatPulse = 0.6f + 0.4f * (float)Math.Sin(t * 8f + timer * 0.2f);

                Color ironColor = Color.Lerp(IronGrey, EmberOrange, heatPulse * 0.3f);
                Color emberColor = Color.Lerp(EmberOrange, JudgmentGold, t * 0.3f);

                // Outer segment
                sb.Draw(glow, pos, null, ironColor * 0.5f * sizeMult, 0f, origin, 0.04f * sizeMult, SpriteEffects.None, 0f);
                // Inner ember
                sb.Draw(glow, pos, null, emberColor * 0.65f * heatPulse * sizeMult, 0f, origin, 0.025f * sizeMult, SpriteEffects.None, 0f);
                // Hot core
                sb.Draw(glow, pos, null, JudgmentGold * 0.4f * heatPulse * sizeMult, 0f, origin, 0.015f * sizeMult, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Spawns a chain impact burst  Emetallic sparks + binding ring feedback.
        /// </summary>
        public static void DoChainImpact(Vector2 worldPosition, Vector2 hitDirection)
        {
            if (Main.dedServ) return;

            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f) + hitDirection * 0.3f;
                Color sparkColor = Main.rand.NextBool() ? EmberOrange : IronGrey;
                Dust d = Dust.NewDustPerfect(worldPosition, Terraria.ID.DustID.Torch, vel, 0, sparkColor, 1.2f);
                d.noGravity = true;
                d.fadeIn = 0.8f;
            }

            // Gold flash
            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);
                Dust g = Dust.NewDustPerfect(worldPosition, Terraria.ID.DustID.GoldFlame, vel, 0, JudgmentGold, 1.5f);
                g.noGravity = true;
            }
        }

        /// <summary>
        /// Spawns shrapnel burst for Fully Bound enemy death  E
        /// metal debris + ember sparks in spiral pattern.
        /// </summary>
        public static void DoShrapnelBurst(Vector2 worldPosition, int sparkCount = 30)
        {
            if (Main.dedServ) return;

            Color[] shrapColors = { IronGrey, EmberOrange, JudgmentGold, DiesIraePalette.CharcoalBlack };

            for (int i = 0; i < sparkCount; i++)
            {
                float angle = MathHelper.TwoPi * i / sparkCount + Main.rand.NextFloat(-0.15f, 0.15f);
                float speed = Main.rand.NextFloat(3f, 8f);
                Vector2 vel = angle.ToRotationVector2() * speed;
                Color c = shrapColors[Main.rand.Next(shrapColors.Length)];

                Dust d = Dust.NewDustPerfect(worldPosition + Main.rand.NextVector2Circular(6, 6),
                    Terraria.ID.DustID.Torch, vel, 0, c, Main.rand.NextFloat(0.8f, 1.6f));
                d.noGravity = Main.rand.NextBool(3); // Some fall, some float
                d.fadeIn = 1.0f;
            }

            // Central flash
            for (int i = 0; i < 6; i++)
            {
                Dust f = Dust.NewDustPerfect(worldPosition, Terraria.ID.DustID.GoldFlame,
                    Main.rand.NextVector2Circular(3f, 3f), 0, JudgmentGold, 2.0f);
                f.noGravity = true;
            }
        }

        // ─────────── THEME TEXTURE ACCENTS ───────────

        /// <summary>
        /// Draws theme-textured chain VFX accents. Call under Additive blend.
        /// </summary>
        public static void DrawThemeAccents(SpriteBatch sb, Vector2 worldPos, float scale, float intensity = 1f)
        {
            DiesIraeVFXLibrary.DrawThemeStarFlare(sb, worldPos, scale, intensity * 0.4f);
            float rot = (float)Main.GameUpdateCount * 0.02f;
            DiesIraeVFXLibrary.DrawThemeImpactRing(sb, worldPos, scale * 0.8f, intensity * 0.35f, rot);
        }
    }
}