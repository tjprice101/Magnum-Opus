using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

namespace MagnumOpus.Content.MoonlightSonata
{
    /// <summary>
    /// Canonical single source-of-truth for ALL Moonlight Sonata theme colors.
    /// Every weapon, projectile, accessory, minion, enemy, and VFX helper MUST
    /// reference this file instead of hardcoding Color values inline.
    ///
    /// Palette philosophy (musical dynamics):
    ///   [0] Pianissimo  — deepest shadow (night void)
    ///   [1] Piano       — dark body (royal purple)
    ///   [2] Mezzo       — primary readable color (violet)
    ///   [3] Forte       — bright accent (lavender)
    ///   [4] Fortissimo  — near-white highlight (ice blue)
    ///   [5] Sforzando   — white-hot core (moon white)
    /// </summary>
    public static class MoonlightSonataPalette
    {
        // =================================================================
        //  CORE THEME COLORS  (the 6 pillars every Moonlight effect uses)
        // =================================================================

        /// <summary>Deep midnight void — shadows, outer glow, darkest aura.</summary>
        public static readonly Color NightPurple = new Color(40, 10, 60);

        /// <summary>Royal indigo — the solemnity of moonlit darkness.</summary>
        public static readonly Color DarkPurple = new Color(75, 0, 130);

        /// <summary>Resonant violet — the heartbeat of Moonlight Sonata.</summary>
        public static readonly Color Violet = new Color(138, 43, 226);

        /// <summary>Soft lavender — pale moonbeams through cloud.</summary>
        public static readonly Color Lavender = new Color(180, 150, 255);

        /// <summary>Glacial ice blue — cold lunar brilliance.</summary>
        public static readonly Color IceBlue = new Color(135, 206, 250);

        /// <summary>Pure moon white — the sforzando peak, lunar zenith.</summary>
        public static readonly Color MoonWhite = new Color(240, 235, 255);

        // =================================================================
        //  CONVENIENCE ACCESSORS
        // =================================================================

        /// <summary>Cool silver for sparkle accents and contrast dust.</summary>
        public static readonly Color Silver = new Color(220, 220, 235);

        /// <summary>Warm-shifted lavender for weapon sprite bloom.</summary>
        public static readonly Color WeaponLavender = new Color(200, 170, 255);

        // =================================================================
        //  EXTENDED PALETTE  (specific use-cases across Moonlight content)
        // =================================================================

        /// <summary>Cosmic void black for deepest shadows and aura blend.</summary>
        public static readonly Color CosmicVoid = new Color(20, 8, 40);

        /// <summary>Gravity-well purple for gravitational and orbital effects.</summary>
        public static readonly Color GravityWell = new Color(100, 60, 180);

        /// <summary>Prismatic violet for refraction and beam effects.</summary>
        public static readonly Color PrismViolet = new Color(160, 80, 255);

        /// <summary>Refracted blue for prismatic beam secondaries.</summary>
        public static readonly Color RefractedBlue = new Color(100, 200, 255);

        /// <summary>Crescent gold for lunar phase accents and cores.</summary>
        public static readonly Color CrescentGold = new Color(255, 240, 180);

        /// <summary>Moonrise gold for warm comet and impact accents.</summary>
        public static readonly Color MoonriseGold = new Color(255, 210, 150);

        /// <summary>Star core white-gold for intense bright centers.</summary>
        public static readonly Color StarCore = new Color(255, 240, 220);

        /// <summary>Deep space violet for crater and impact ring shadows.</summary>
        public static readonly Color DeepSpaceViolet = new Color(50, 20, 100);

        /// <summary>Energy tendril lavender for crackling arc effects.</summary>
        public static readonly Color EnergyTendril = new Color(180, 140, 255);

        /// <summary>Nebula purple for cosmic mid-range effects.</summary>
        public static readonly Color NebulaPurple = new Color(150, 80, 220);

        // =================================================================
        //  TOOLTIP / UI COLORS
        // =================================================================

        /// <summary>Lore text color for ModifyTooltips.</summary>
        public static readonly Color LoreText = new Color(138, 43, 226);

        /// <summary>Special weapon effect tooltip color.</summary>
        public static readonly Color EffectTooltip = new Color(180, 150, 255);

        // =================================================================
        //  6-COLOR SWING PALETTES  (per-weapon musical scales)
        // =================================================================

        /// <summary>
        /// EternalMoon blade palette — lunar eclipse ascending to crescent gold.
        /// Majestic, sweeping, the adagio's power. Every swing is a moonrise.
        /// </summary>
        public static readonly Color[] EternalMoonBlade = new Color[]
        {
            new Color(40, 10, 60),      // [0] Pianissimo — night void
            new Color(75, 0, 130),      // [1] Piano — deep indigo
            new Color(138, 43, 226),    // [2] Mezzo — violet body
            new Color(135, 206, 250),   // [3] Forte — ice blue flare
            new Color(255, 240, 180),   // [4] Fortissimo — crescent gold
            new Color(255, 250, 240),   // [5] Sforzando — moonbeam white
        };

        /// <summary>
        /// IncisorOfMoonlight blade palette — deep resonance through harmonic white.
        /// Precise, laser-sharp, crystalline. Every cut is a tuning fork strike.
        /// </summary>
        public static readonly Color[] IncisorBlade = new Color[]
        {
            new Color(90, 50, 160),     // [0] Pianissimo — deep resonance
            new Color(170, 140, 255),   // [1] Piano — frequency pulse
            new Color(230, 235, 255),   // [2] Mezzo — resonant silver
            new Color(135, 206, 250),   // [3] Forte — ice blue clarity
            new Color(220, 230, 255),   // [4] Fortissimo — crystal edge
            new Color(255, 250, 245),   // [5] Sforzando — harmonic white
        };

        /// <summary>
        /// MoonlightsCalling blade palette — dark purple through refracted blue.
        /// Prismatic, bouncing, playful-yet-arcane. Every beam is a moonlit prism.
        /// </summary>
        public static readonly Color[] MoonlightsCallingBeam = new Color[]
        {
            new Color(75, 0, 130),      // [0] Pianissimo — dark purple base
            new Color(160, 80, 255),    // [1] Piano — prism violet
            new Color(138, 43, 226),    // [2] Mezzo — violet body
            new Color(100, 200, 255),   // [3] Forte — refracted blue
            new Color(135, 206, 250),   // [4] Fortissimo — ice blue shimmer
            new Color(240, 235, 255),   // [5] Sforzando — moon white
        };

        /// <summary>
        /// ResurrectionOfTheMoon palette — deep space through comet core.
        /// Heavy, impactful, meteoric. Every shot is a celestial crash.
        /// </summary>
        public static readonly Color[] ResurrectionComet = new Color[]
        {
            new Color(50, 20, 100),     // [0] Pianissimo — deep space violet
            new Color(100, 80, 200),    // [1] Piano — impact crater
            new Color(180, 120, 255),   // [2] Mezzo — comet trail
            new Color(255, 210, 150),   // [3] Forte — moonrise gold
            new Color(255, 230, 200),   // [4] Fortissimo — comet core
            new Color(255, 248, 230),   // [5] Sforzando — white-hot impact
        };

        /// <summary>
        /// StaffOfTheLunarPhases palette — cycling through all moon phases.
        /// Mystical, phase-shifting, enigmatic. Every cast is a lunar cycle.
        /// </summary>
        public static readonly Color[] LunarPhasesCast = new Color[]
        {
            new Color(20, 8, 40),       // [0] Pianissimo — new moon void
            new Color(75, 0, 130),      // [1] Piano — waxing crescent
            new Color(138, 43, 226),    // [2] Mezzo — first quarter violet
            new Color(180, 150, 255),   // [3] Forte — waxing gibbous lavender
            new Color(135, 206, 250),   // [4] Fortissimo — full moon ice blue
            new Color(240, 235, 255),   // [5] Sforzando — supermoon white
        };

        /// <summary>
        /// GoliathOfMoonlight palette — cosmic void through star core.
        /// Cosmic, gravitational, overwhelming. Every attack bends spacetime.
        /// </summary>
        public static readonly Color[] GoliathCosmic = new Color[]
        {
            new Color(20, 8, 40),       // [0] Pianissimo — cosmic void
            new Color(100, 60, 180),    // [1] Piano — gravity well
            new Color(150, 80, 220),    // [2] Mezzo — nebula purple
            new Color(180, 140, 255),   // [3] Forte — energy tendril
            new Color(135, 206, 250),   // [4] Fortissimo — ice blue brilliance
            new Color(255, 240, 220),   // [5] Sforzando — star core
        };

        // =================================================================
        //  GRADIENT HELPERS
        // =================================================================

        /// <summary>
        /// Lerp through any 6-color palette. t=0 returns [0], t=1 returns [5].
        /// </summary>
        public static Color PaletteLerp(Color[] palette, float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            float scaled = t * (palette.Length - 1);
            int lo = (int)scaled;
            int hi = Math.Min(lo + 1, palette.Length - 1);
            return Color.Lerp(palette[lo], palette[hi], scaled - lo);
        }

        /// <summary>
        /// Standard Moonlight gradient: DarkPurple -> IceBlue over 0->1.
        /// Use for generic theme effects, halos, cascading rings.
        /// </summary>
        public static Color GetGradient(float progress)
            => Color.Lerp(DarkPurple, IceBlue, progress);

        /// <summary>
        /// Lunar phase gradient: NightPurple -> Violet -> MoonWhite over 0->1.
        /// Use for phase-cycling effects and crescent overlays.
        /// </summary>
        public static Color GetLunarGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(NightPurple, Violet, progress * 2f);
            return Color.Lerp(Violet, MoonWhite, (progress - 0.5f) * 2f);
        }

        /// <summary>
        /// Crescent gradient: DarkPurple -> CrescentGold over 0->1.
        /// Use for crescent moon motifs, lunar accents.
        /// </summary>
        public static Color GetCrescentGradient(float progress)
            => Color.Lerp(DarkPurple, CrescentGold, progress);

        /// <summary>
        /// Cosmic gradient: CosmicVoid -> NebulaPurple -> StarCore over 0->1.
        /// Use for Goliath, gravitational effects, cosmic impacts.
        /// </summary>
        public static Color GetCosmicGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(CosmicVoid, NebulaPurple, progress * 2f);
            return Color.Lerp(NebulaPurple, StarCore, (progress - 0.5f) * 2f);
        }

        /// <summary>
        /// Get an additive-safe color (A=0) for bloom stacking.
        /// </summary>
        public static Color Additive(Color c) => c with { A = 0 };

        /// <summary>
        /// Get an additive-safe color with opacity multiplier.
        /// </summary>
        public static Color Additive(Color c, float opacity) => c with { A = 0 } * opacity;

        // =================================================================
        //  PREDRAW BLOOM LAYER PRESETS
        // =================================================================

        /// <summary>
        /// Standard 3-layer PreDrawInWorld bloom for Moonlight items.
        /// Call from PreDrawInWorld with additive SpriteBatch.
        /// </summary>
        public static void DrawItemBloom(
            SpriteBatch sb,
            Texture2D tex,
            Vector2 pos,
            Vector2 origin,
            float rotation,
            float scale,
            float pulse)
        {
            // Layer 1: Outer dark purple aura
            sb.Draw(tex, pos, null, Additive(DarkPurple, 0.40f), rotation, origin, scale * 1.08f * pulse,
                SpriteEffects.None, 0f);
            // Layer 2: Middle violet glow
            sb.Draw(tex, pos, null, Additive(Violet, 0.30f), rotation, origin, scale * 1.04f * pulse,
                SpriteEffects.None, 0f);
            // Layer 3: Inner ice-blue core
            sb.Draw(tex, pos, null, Additive(IceBlue, 0.22f), rotation, origin, scale * 1.01f * pulse,
                SpriteEffects.None, 0f);
        }

        // =================================================================
        //  6-COLOR MASTER PALETTE INTERPOLATION
        // =================================================================

        /// <summary>
        /// The 6 canonical palette stops as an array for indexed interpolation.
        /// [0] NightPurple -> [1] DarkPurple -> [2] Violet -> [3] Lavender -> [4] IceBlue -> [5] MoonWhite.
        /// </summary>
        private static readonly Color[] MasterPalette =
        {
            NightPurple, DarkPurple, Violet, Lavender, IceBlue, MoonWhite
        };

        /// <summary>
        /// Lerp through the 6-colour master palette.
        /// t=0 -> NightPurple (Pianissimo), t=1 -> MoonWhite (Sforzando).
        /// </summary>
        public static Color GetPaletteColor(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            float scaled = t * (MasterPalette.Length - 1);
            int idx = (int)scaled;
            int next = Math.Min(idx + 1, MasterPalette.Length - 1);
            return Color.Lerp(MasterPalette[idx], MasterPalette[next], scaled - idx);
        }

        /// <summary>
        /// Palette colour with Calamity-style white push for perceived brilliance.
        /// push=0 returns pure palette, push=1 returns full white.
        /// Typical usage: push 0.35-0.55 for trail/bloom cores.
        /// </summary>
        public static Color GetPaletteColorWithWhitePush(float t, float push)
        {
            Color baseCol = GetPaletteColor(t);
            return Color.Lerp(baseCol, Color.White, MathHelper.Clamp(push, 0f, 1f));
        }

        /// <summary>
        /// Generic blade gradient through any blade palette array.
        /// progress=0 returns palette[0], progress=1 returns palette[last].
        /// Works with EternalMoonBlade, IncisorBlade, MoonlightsCallingBeam, etc.
        /// </summary>
        public static Color GetBladeGradient(Color[] bladePalette, float progress)
            => PaletteLerp(bladePalette, progress);

        // =================================================================
        //  HSL CYCLING (for shimmer / color oscillation effects)
        // =================================================================

        /// <summary>
        /// Get a hue-shifted Moonlight color for shimmer effects.
        /// Stays within the purple-blue hue range (0.72->0.83).
        /// </summary>
        public static Color GetShimmer(float time, float sat = 0.70f, float lum = 0.60f)
        {
            float hue = (time * 0.02f) % 1f;
            hue = 0.72f + hue * 0.11f; // Clamp to purple-blue hue range
            return Main.hslToRgb(hue, sat, lum);
        }

        /// <summary>
        /// Get a hue-shifted crescent color for lunar phase shimmer effects.
        /// Sweeps through the violet->blue->silver hue range (0.68->0.80).
        /// </summary>
        public static Color GetLunarShimmer(float time, float sat = 0.65f, float lum = 0.65f)
        {
            float hue = (time * 0.015f) % 1f;
            hue = 0.68f + hue * 0.12f; // Clamp to violet-blue-silver hue range
            return Main.hslToRgb(hue, sat, lum);
        }
    }
}
