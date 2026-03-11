using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.DiesIrae;

namespace MagnumOpus.Content.DiesIrae.Weapons.WrathsCleaver.Utilities
{
    /// <summary>
    /// Static utility class for Wrath's Cleaver VFX configuration.
    /// Provides palette, bloom stacking, smear arc parameters, and
    /// shared VFX helpers for the cleaver swing and sub-projectiles.
    /// </summary>
    public static class WrathsCleaverUtils
    {
        // ══════════════════════════════════════════════════════════╁E
        //  PALETTE  E6-color musical dynamics for the cleaver swing
        // ══════════════════════════════════════════════════════════╁E

        /// <summary>
        /// Wrath's Cleaver swing palette  Echarcoal ↁEblood ↁEember ↁEgold ↁEbone ↁEwrath-white.
        /// Aggressive, fire-heavy. The raw wrath of the first movement.
        /// </summary>
        public static readonly Color[] SwingPalette = new Color[]
        {
            new Color(25, 20, 25),     // [0] Pianissimo  Echarcoal black
            new Color(130, 0, 0),      // [1] Piano  Eblood red
            new Color(255, 69, 0),     // [2] Mezzo  Eember orange
            new Color(200, 170, 50),   // [3] Forte  Ejudgment gold
            new Color(230, 220, 200),  // [4] Fortissimo  Ebone white
            new Color(255, 250, 240),  // [5] Sforzando  Ewrath white
        };

        // ══════════════════════════════════════════════════════════╁E
        //  CONVENIENCE COLORS
        // ══════════════════════════════════════════════════════════╁E

        public static readonly Color CharcoalBlack = DiesIraePalette.CharcoalBlack;
        public static readonly Color BloodRed = DiesIraePalette.BloodRed;
        public static readonly Color EmberOrange = DiesIraePalette.EmberOrange;
        public static readonly Color JudgmentGold = DiesIraePalette.JudgmentGold;
        public static readonly Color BoneWhite = DiesIraePalette.BoneWhite;
        public static readonly Color WrathWhite = DiesIraePalette.WrathWhite;
        public static readonly Color InfernalRed = DiesIraePalette.InfernalRed;

        // ══════════════════════════════════════════════════════════╁E
        //  PALETTE INTERPOLATION
        // ══════════════════════════════════════════════════════════╁E

        /// <summary>
        /// Sample the Dies Irae gradient LUT texture.
        /// t=0 → left edge (dark), t=1 → right edge (bright).
        /// </summary>
        public static Color GetPaletteColor(float t)
        {
            return DiesIraeVFXLibrary.SampleLUT(t);
        }

        /// <summary>Palette color with white push for brilliance on cores/bloom.</summary>
        public static Color GetPaletteColorBright(float t, float push = 0.35f)
        {
            Color baseCol = GetPaletteColor(t);
            return Color.Lerp(baseCol, Color.White, MathHelper.Clamp(push, 0f, 1f));
        }

        // ══════════════════════════════════════════════════════════╁E
        //  BLOOM STACKING
        // ══════════════════════════════════════════════════════════╁E

        /// <summary>
        /// Draws a 4-layer additive bloom stack at the given position.
        /// Must be called while SpriteBatch is in Additive blend mode.
        /// </summary>
        public static void DrawBloomStack(SpriteBatch sb, Vector2 screenPos, float baseScale,
            float intensity, int comboStep)
        {
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow == null) return;
            Vector2 origin = glow.Size() * 0.5f;

            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 0.1f;
            float stepMult = 1f + comboStep * 0.15f;

            // Layer 1: Wide outer glow  Eblood red
            Color c1 = BloodRed * (0.2f * intensity * stepMult);
            c1.A = 0;
            sb.Draw(glow, screenPos, null, c1, 0f, origin, baseScale * 2.5f * pulse, SpriteEffects.None, 0f);

            // Layer 2: Mid glow  Einfernal red
            Color c2 = InfernalRed * (0.35f * intensity * stepMult);
            c2.A = 0;
            sb.Draw(glow, screenPos, null, c2, 0f, origin, baseScale * 1.6f * pulse, SpriteEffects.None, 0f);

            // Layer 3: Inner glow  Eember orange
            Color c3 = EmberOrange * (0.5f * intensity * stepMult);
            c3.A = 0;
            sb.Draw(glow, screenPos, null, c3, 0f, origin, baseScale * 1.0f * pulse, SpriteEffects.None, 0f);

            // Layer 4: Hot core  Ejudgment gold
            Color c4 = JudgmentGold * (0.7f * intensity * stepMult);
            c4.A = 0;
            sb.Draw(glow, screenPos, null, c4, 0f, origin, baseScale * 0.5f, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Draws the tip bloom for the blade with counter-rotating star flares.
        /// Uses DiesIrae theme-specific star flare and radial slash star textures.
        /// Falls back to universal registry if theme textures unavailable.
        /// </summary>
        public static void DrawTipBloom(SpriteBatch sb, Vector2 tipScreen, float intensity, int comboStep)
        {
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            // Theme-specific star flare — hellfire pointed burst
            Texture2D star = DiesIraeThemeTextures.DIStarFlare?.Value ?? MagnumTextureRegistry.GetStar4Hard();
            // Theme-specific radial slash star — judgment impact ring
            Texture2D radial = DiesIraeThemeTextures.DIRadialSlashStar?.Value ?? MagnumTextureRegistry.GetRadialBloom();
            if (glow == null) return;

            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.15f;
            float stepScale = 0.04f + comboStep * 0.015f;

            // Outer radial bloom (capped 300px max)
            if (radial != null)
            {
                Vector2 radOrigin = radial.Size() * 0.5f;
                Color radColor = EmberOrange * (0.5f * intensity);
                radColor.A = 0;
                sb.Draw(radial, tipScreen, null, radColor, 0f, radOrigin, MathHelper.Min(stepScale * 1.8f * pulse, 0.139f), SpriteEffects.None, 0f);
            }

            // Mid glow
            Vector2 glowOrigin = glow.Size() * 0.5f;
            Color midColor = JudgmentGold * (0.6f * intensity);
            midColor.A = 0;
            sb.Draw(glow, tipScreen, null, midColor, 0f, glowOrigin, stepScale * 1.5f * pulse, SpriteEffects.None, 0f);

            // Core point
            Color coreColor = WrathWhite * (0.8f * intensity);
            coreColor.A = 0;
            sb.Draw(glow, tipScreen, null, coreColor, 0f, glowOrigin, stepScale * 0.6f, SpriteEffects.None, 0f);

            // Counter-rotating star flares
            if (star != null)
            {
                Vector2 starOrigin = star.Size() * 0.5f;
                float rot1 = Main.GameUpdateCount * 0.03f;
                float rot2 = -Main.GameUpdateCount * 0.02f;
                Color starColor = EmberOrange * (0.4f * intensity);
                starColor.A = 0;
                sb.Draw(star, tipScreen, null, starColor, rot1, starOrigin, stepScale * 0.8f, SpriteEffects.None, 0f);
                sb.Draw(star, tipScreen, null, starColor * 0.6f, rot2, starOrigin, stepScale * 1.1f, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Draws heavy hellfire smoke particles along the blade path.
        /// Returns the number of particles spawned this frame.
        /// </summary>
        public static int SpawnSwingDust(Vector2 playerCenter, Vector2 swordDir, float bladeLength,
            int comboStep, float progression, int direction)
        {
            int count = 0;
            float swingSpeed = Math.Abs(progression - Math.Max(0, progression - 0.02f));

            if (swingSpeed > 0.005f)
            {
                // Fire dust along the blade
                int dustCount = 2 + comboStep;
                for (int i = 0; i < dustCount; i++)
                {
                    if (!Main.rand.NextBool(Math.Max(1, 3 - comboStep)))
                        continue;

                    float along = Main.rand.NextFloat(0.3f, 1f);
                    Vector2 pos = playerCenter + swordDir * bladeLength * along;
                    Vector2 perp = swordDir.RotatedBy(MathHelper.PiOver2 * direction);
                    Vector2 vel = perp * Main.rand.NextFloat(1f, 3f) + swordDir * Main.rand.NextFloat(-1f, 1f);

                    Dust d = Dust.NewDustPerfect(pos, Terraria.ID.DustID.Torch, vel, 0,
                        GetPaletteColor(along), 1.5f + comboStep * 0.3f);
                    d.noGravity = true;
                    d.fadeIn = 1.2f;
                    count++;
                }

                // Ember sparks near the tip
                if (Main.rand.NextBool(2))
                {
                    Vector2 tipPos = playerCenter + swordDir * bladeLength;
                    Vector2 sparkVel = Main.rand.NextVector2Circular(4f, 4f);
                    Dust s = Dust.NewDustPerfect(tipPos, Terraria.ID.DustID.Torch, sparkVel, 0,
                        EmberOrange, 0.8f + comboStep * 0.2f);
                    s.noGravity = true;
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Wrath impact VFX  Emulti-layered dust burst at hit location.
        /// </summary>
        public static void DoHitImpact(Vector2 hitPos, int comboStep)
        {
            // === Color-ramped sparkle explosion VFX ===
            DiesIraeVFXLibrary.SpawnColorRampedSparkleExplosion(hitPos, 8, 5f, 0.3f);

            // Radial fire dust burst
            int dustCount = 8 + comboStep * 4;
            for (int i = 0; i < dustCount; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(8f, 8f);
                Color col = GetPaletteColor(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(hitPos + Main.rand.NextVector2Circular(8f, 8f),
                    Terraria.ID.DustID.Torch, vel, 0, col, 1.4f + comboStep * 0.2f);
                d.noGravity = true;
            }

            // Ember scatter
            for (int i = 0; i < 4 + comboStep * 2; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f) + new Vector2(0, -2f);
                Dust d = Dust.NewDustPerfect(hitPos, Terraria.ID.DustID.Torch, vel, 0,
                    EmberOrange, 0.6f + comboStep * 0.15f);
                d.noGravity = true;
            }

            // Ash scatter
            for (int i = 0; i < 3 + comboStep; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f) + new Vector2(0, -1f);
                Dust d = Dust.NewDustPerfect(hitPos, Terraria.ID.DustID.Smoke, vel, 100,
                    DiesIraePalette.AshGray, 1.0f);
                d.noGravity = false;
            }
        }

        // ══════════════════════════════════════════════════════════╗
        //  THEME TEXTURE VFX — Dies Irae-specific impact/bloom layers
        // ══════════════════════════════════════════════════════════╝

        /// <summary>
        /// Draws a themed judgment impact ring using DiesIraeThemeTextures.
        /// Renders the Power Effect Ring and Harmonic Impact overlaid at the hit position.
        /// Must be called while SpriteBatch is in Additive blend mode.
        /// </summary>
        public static void DrawJudgmentImpactRing(SpriteBatch sb, Vector2 screenPos, float scale, float intensity, float rotation)
        {
            // Layer 1: Power Effect Ring — expanding wrath ring
            Texture2D ring = DiesIraeThemeTextures.DIPowerEffectRing?.Value;
            if (ring != null)
            {
                Vector2 origin = ring.Size() * 0.5f;
                Color ringColor = BloodRed * (0.6f * intensity);
                ringColor.A = 0;
                sb.Draw(ring, screenPos, null, ringColor, rotation, origin, scale * 0.15f, SpriteEffects.None, 0f);

                Color innerRing = EmberOrange * (0.4f * intensity);
                innerRing.A = 0;
                sb.Draw(ring, screenPos, null, innerRing, -rotation * 0.7f, origin, scale * 0.1f, SpriteEffects.None, 0f);
            }

            // Layer 2: Harmonic Impact — shockwave overlay
            Texture2D impact = DiesIraeThemeTextures.DIHarmonicImpact?.Value;
            if (impact != null)
            {
                Vector2 impOrigin = impact.Size() * 0.5f;
                Color impColor = JudgmentGold * (0.5f * intensity);
                impColor.A = 0;
                sb.Draw(impact, screenPos, null, impColor, rotation * 1.3f, impOrigin, scale * 0.12f, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Draws theme-specific hellfire star flares at a position.
        /// Uses DI Star Flare textures for pointed burst accents.
        /// Must be called while SpriteBatch is in Additive blend mode.
        /// </summary>
        public static void DrawHellfireStarFlare(SpriteBatch sb, Vector2 screenPos, float scale, float intensity)
        {
            Texture2D flare = DiesIraeThemeTextures.DIStarFlare?.Value;
            Texture2D flare2 = DiesIraeThemeTextures.DIStarFlare2?.Value;

            if (flare != null)
            {
                Vector2 origin = flare.Size() * 0.5f;
                float rot = Main.GameUpdateCount * 0.04f;
                Color c = EmberOrange * (0.5f * intensity);
                c.A = 0;
                sb.Draw(flare, screenPos, null, c, rot, origin, scale * 0.08f, SpriteEffects.None, 0f);
            }

            if (flare2 != null)
            {
                Vector2 origin = flare2.Size() * 0.5f;
                float rot = -Main.GameUpdateCount * 0.03f;
                Color c = JudgmentGold * (0.35f * intensity);
                c.A = 0;
                sb.Draw(flare2, screenPos, null, c, rot, origin, scale * 0.06f, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Draws a themed radial slash burst at impact positions using DI Radial Slash Star.
        /// Must be called while SpriteBatch is in Additive blend mode.
        /// </summary>
        public static void DrawRadialSlashBurst(SpriteBatch sb, Vector2 screenPos, float scale, float intensity, float rotation)
        {
            Texture2D slashStar = DiesIraeThemeTextures.DIRadialSlashStar?.Value;
            if (slashStar == null) return;

            Vector2 origin = slashStar.Size() * 0.5f;

            // Outer blood-red slash star
            Color outer = BloodRed * (0.4f * intensity);
            outer.A = 0;
            sb.Draw(slashStar, screenPos, null, outer, rotation, origin, scale * 0.14f, SpriteEffects.None, 0f);

            // Inner ember-orange slash star (counter-rotated)
            Color inner = EmberOrange * (0.6f * intensity);
            inner.A = 0;
            sb.Draw(slashStar, screenPos, null, inner, -rotation * 0.5f, origin, scale * 0.08f, SpriteEffects.None, 0f);

            // Core gold flash
            Color core = JudgmentGold * (0.7f * intensity);
            core.A = 0;
            sb.Draw(slashStar, screenPos, null, core, rotation * 1.5f, origin, scale * 0.04f, SpriteEffects.None, 0f);
        }
    }
}
