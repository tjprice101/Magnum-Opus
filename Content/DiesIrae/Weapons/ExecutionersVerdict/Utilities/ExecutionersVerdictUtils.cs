using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.DiesIrae;

namespace MagnumOpus.Content.DiesIrae.Weapons.ExecutionersVerdict.Utilities
{
    /// <summary>
    /// Static utility class for Executioner's Verdict VFX.
    /// The executioner judges with precision  Econtrolled, methodical, absolute.
    /// Sharper palette transitions than Wrath's Cleaver: blood crimson ↁEjudgment gold ↁEsharp white.
    /// </summary>
    public static class ExecutionersVerdictUtils
    {
        // ══════════════════════════════════════════════════════════╁E
        //  PALETTE  E6-color judicial precision
        // ══════════════════════════════════════════════════════════╁E

        /// <summary>
        /// Executioner's Verdict swing palette  Echarcoal ↁEblood crimson ↁEjudgment gold ↁEsharp white.
        /// Controlled, clean, deliberate. Sharper transitions than Wrath's Cleaver.
        /// </summary>
        public static readonly Color[] SwingPalette = new Color[]
        {
            new Color(20, 15, 20),     // [0] Pianissimo  Evoid black
            new Color(140, 15, 15),    // [1] Piano  Eblood crimson
            new Color(220, 50, 15),    // [2] Mezzo  Ecrimson red
            new Color(255, 200, 80),   // [3] Forte  Ejudgment gold
            new Color(255, 240, 220),  // [4] Fortissimo  Ehellfire white
            new Color(255, 252, 245),  // [5] Sforzando  Eabsolute white
        };

        // ══════════════════════════════════════════════════════════╁E
        //  CONVENIENCE COLORS
        // ══════════════════════════════════════════════════════════╁E

        public static readonly Color VoidBlack = new Color(20, 15, 20);
        public static readonly Color BloodCrimson = new Color(140, 15, 15);
        public static readonly Color CrimsonRed = new Color(220, 50, 15);
        public static readonly Color JudgmentGold = DiesIraePalette.JudgmentGold;
        public static readonly Color HellfireWhite = new Color(255, 240, 220);
        public static readonly Color AbsoluteWhite = new Color(255, 252, 245);

        // ══════════════════════════════════════════════════════════╁E
        //  PALETTE INTERPOLATION
        // ══════════════════════════════════════════════════════════╁E

        public static Color GetPaletteColor(float t)
        {
            return DiesIraeVFXLibrary.SampleLUT(t);
        }

        public static Color GetPaletteColorBright(float t, float push = 0.35f)
        {
            return Color.Lerp(GetPaletteColor(t), Color.White, MathHelper.Clamp(push, 0f, 1f));
        }

        // ══════════════════════════════════════════════════════════╁E
        //  BLOOM STACKING
        // ══════════════════════════════════════════════════════════╁E

        /// <summary>
        /// Draws a 4-layer additive bloom stack  Ejudicial precision style.
        /// Clean, sharp transitions. No chaotic noise.
        /// </summary>
        public static void DrawBloomStack(SpriteBatch sb, Vector2 screenPos, float baseScale,
            float intensity, int comboStep)
        {
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow == null) return;
            Vector2 origin = glow.Size() * 0.5f;

            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.10f) * 0.08f;
            float stepMult = 1f + comboStep * 0.2f;

            // Layer 1: Wide crimson authority
            Color c1 = BloodCrimson * (0.2f * intensity * stepMult);
            c1.A = 0;
            sb.Draw(glow, screenPos, null, c1, 0f, origin, baseScale * 2.5f * pulse, SpriteEffects.None, 0f);

            // Layer 2: Mid gold judgment
            Color c2 = JudgmentGold * (0.35f * intensity * stepMult);
            c2.A = 0;
            sb.Draw(glow, screenPos, null, c2, 0f, origin, baseScale * 1.4f * pulse, SpriteEffects.None, 0f);

            // Layer 3: Inner crimson 
            Color c3 = CrimsonRed * (0.5f * intensity * stepMult);
            c3.A = 0;
            sb.Draw(glow, screenPos, null, c3, 0f, origin, baseScale * 0.9f * pulse, SpriteEffects.None, 0f);

            // Layer 4: Bright core
            Color c4 = HellfireWhite * (0.7f * intensity * stepMult);
            c4.A = 0;
            sb.Draw(glow, screenPos, null, c4, 0f, origin, baseScale * 0.4f, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Draws tip bloom for the Verdict blade  Ecleaner and more focused than Wrath's Cleaver.
        /// </summary>
        public static void DrawTipBloom(SpriteBatch sb, Vector2 tipScreen, float intensity, int comboStep)
        {
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            Texture2D star = MagnumTextureRegistry.GetStar4Hard();
            Texture2D radial = MagnumTextureRegistry.GetRadialBloom();
            if (glow == null) return;

            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 0.10f;
            float stepScale = 0.035f + comboStep * 0.012f;

            // Outer radial — crimson judgment (capped 300px max)
            if (radial != null)
            {
                Vector2 radOrigin = radial.Size() * 0.5f;
                Color radColor = CrimsonRed * (0.4f * intensity);
                radColor.A = 0;
                sb.Draw(radial, tipScreen, null, radColor, 0f, radOrigin, stepScale * 1.8f * pulse, SpriteEffects.None, 0f);
            }

            // Mid  Egold judgment
            Vector2 glowOrigin = glow.Size() * 0.5f;
            Color midColor = JudgmentGold * (0.55f * intensity);
            midColor.A = 0;
            sb.Draw(glow, tipScreen, null, midColor, 0f, glowOrigin, stepScale * 1.3f * pulse, SpriteEffects.None, 0f);

            // Core  Ewhite hot
            Color coreColor = AbsoluteWhite * (0.75f * intensity);
            coreColor.A = 0;
            sb.Draw(glow, tipScreen, null, coreColor, 0f, glowOrigin, stepScale * 0.5f, SpriteEffects.None, 0f);

            // Counter-rotating star flares  Esharper and slower than Wrath's
            if (star != null)
            {
                Vector2 starOrigin = star.Size() * 0.5f;
                float rot1 = Main.GameUpdateCount * 0.02f;
                float rot2 = -Main.GameUpdateCount * 0.015f;
                Color starColor = JudgmentGold * (0.35f * intensity);
                starColor.A = 0;
                sb.Draw(star, tipScreen, null, starColor, rot1, starOrigin, stepScale * 0.7f, SpriteEffects.None, 0f);
                sb.Draw(star, tipScreen, null, starColor * 0.5f, rot2, starOrigin, stepScale * 1.0f, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Verdict impact VFX  Ecleaner, more deliberate than Wrath's Cleaver.
        /// Judgment gold + crimson, directional slash marks.
        /// </summary>
        public static void DoHitImpact(Vector2 hitPos, int comboStep)
        {
            // === Color-ramped sparkle explosion VFX ===
            DiesIraeVFXLibrary.SpawnColorRampedSparkleExplosion(hitPos, 8, 5f, 0.3f);

            // Directional crimson dust burst
            int dustCount = 6 + comboStep * 3;
            for (int i = 0; i < dustCount; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                Dust d = Dust.NewDustPerfect(hitPos + Main.rand.NextVector2Circular(6f, 6f),
                    Terraria.ID.DustID.Torch, vel, 0,
                    GetPaletteColor(Main.rand.NextFloat(0.2f, 0.8f)),
                    1.2f + comboStep * 0.2f);
                d.noGravity = true;
            }

            // Gold judgment sparks  Etight, focused
            for (int i = 0; i < 3 + comboStep * 2; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Dust d = Dust.NewDustPerfect(hitPos, Terraria.ID.DustID.GoldFlame, vel, 0,
                    JudgmentGold, 0.7f + comboStep * 0.1f);
                d.noGravity = true;
            }
        }

        /// <summary>
        /// Spawn swing dust  Econtrolled, methodical ember trail.
        /// </summary>
        public static int SpawnSwingDust(Vector2 playerCenter, Vector2 swordDir, float bladeLength,
            int comboStep, float progression, int direction)
        {
            int count = 0;
            float swingSpeed = Math.Abs(progression - Math.Max(0, progression - 0.02f));

            if (swingSpeed > 0.005f)
            {
                int dustCount = 1 + comboStep;
                for (int i = 0; i < dustCount; i++)
                {
                    if (!Main.rand.NextBool(Math.Max(1, 3 - comboStep)))
                        continue;

                    float along = Main.rand.NextFloat(0.35f, 1f);
                    Vector2 pos = playerCenter + swordDir * bladeLength * along;
                    Vector2 perp = swordDir.RotatedBy(MathHelper.PiOver2 * direction);
                    Vector2 vel = perp * Main.rand.NextFloat(0.5f, 2f);

                    Dust d = Dust.NewDustPerfect(pos, Terraria.ID.DustID.Torch, vel, 0,
                        GetPaletteColor(along), 1.2f + comboStep * 0.25f);
                    d.noGravity = true;
                    d.fadeIn = 1.0f;
                    count++;
                }
            }

            return count;
        }

        // ─────────── THEME TEXTURE ACCENTS ───────────

        /// <summary>
        /// Draws theme-textured impact accents at the given world position.
        /// Call from PreDraw/DrawCustomVFX under Additive blend.
        /// </summary>
        public static void DrawThemeAccents(SpriteBatch sb, Vector2 worldPos, float scale, float intensity = 1f)
        {
            DiesIraeVFXLibrary.DrawThemeStarFlare(sb, worldPos, scale, intensity * 0.5f);
            float rot = (float)Main.GameUpdateCount * 0.025f;
            DiesIraeVFXLibrary.DrawThemeImpactRing(sb, worldPos, scale, intensity * 0.4f, rot);
        }
    }
}