using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

namespace MagnumOpus.Content.Nachtmusik
{
    /// <summary>
    /// Canonical single source-of-truth for ALL Nachtmusik theme colors.
    /// Every weapon, projectile, accessory, minion, enemy, and VFX helper MUST
    /// reference this file instead of hardcoding Color values inline.
    ///
    /// Palette philosophy (musical dynamics):
    ///   [0] Pianissimo  — deepest shadow (midnight void)
    ///   [1] Piano       — dark body (nocturnal depths)
    ///   [2] Mezzo       — primary readable color (starlit blue)
    ///   [3] Forte       — bright accent (star white)
    ///   [4] Fortissimo  — near-white highlight (moonlit silver)
    ///   [5] Sforzando   — white-hot core (twinkling white)
    ///
    /// Theme identity: Mozart's serenade — playful night music, starlit elegance,
    /// nocturnal charm, twinkling stars, the Queen of Radiance's celestial grace.
    /// </summary>
    public static class NachtmusikPalette
    {
        // =================================================================
        //  CORE THEME COLORS  (the 6 pillars every Nachtmusik effect uses)
        // =================================================================

        /// <summary>Deep midnight void — shadows, outer glow, darkest aura.</summary>
        public static readonly Color MidnightBlue = new Color(15, 15, 45);

        /// <summary>Nocturnal depths — the solemnity of night's embrace.</summary>
        public static readonly Color DeepBlue = new Color(30, 50, 120);

        /// <summary>Starlit blue — the heartbeat of Nachtmusik's serenade.</summary>
        public static readonly Color StarlitBlue = new Color(80, 120, 200);

        /// <summary>Star white — bright starlight streaming through the night.</summary>
        public static readonly Color StarWhite = new Color(200, 210, 240);

        /// <summary>Moonlit silver — near-white lunar shimmer, the queen's glow.</summary>
        public static readonly Color MoonlitSilver = new Color(230, 235, 245);

        /// <summary>Twinkling white — the sforzando peak, celestial zenith.</summary>
        public static readonly Color TwinklingWhite = new Color(248, 250, 255);

        // =================================================================
        //  CONVENIENCE ACCESSORS
        // =================================================================

        /// <summary>Warm radiant gold for celestial accents and queen's radiance.</summary>
        public static readonly Color RadianceGold = new Color(255, 215, 0);

        /// <summary>Warm star gold for comet trails and impact highlights.</summary>
        public static readonly Color StarGold = new Color(255, 230, 150);

        /// <summary>Cool moonlight silver for sparkle accents and contrast dust.</summary>
        public static readonly Color Silver = new Color(210, 220, 240);

        // =================================================================
        //  EXTENDED PALETTE  (specific use-cases across Nachtmusik content)
        // =================================================================

        /// <summary>Cosmic night void for deepest shadows and aura blend.</summary>
        public static readonly Color CosmicVoid = new Color(10, 10, 30);

        /// <summary>Deep cosmic purple for royal accent effects.</summary>
        public static readonly Color CosmicPurple = new Color(75, 50, 130);

        /// <summary>Medium violet for celestial midrange highlights.</summary>
        public static readonly Color Violet = new Color(123, 104, 238);

        /// <summary>Nebula pink for constellation accent shimmer.</summary>
        public static readonly Color NebulaPink = new Color(180, 120, 200);

        /// <summary>Dark dusk violet for twilight transition effects.</summary>
        public static readonly Color DuskViolet = new Color(100, 80, 180);

        /// <summary>Night sky blue for ambient serenade backgrounds.</summary>
        public static readonly Color NightSkyBlue = new Color(25, 25, 112);

        /// <summary>Constellation blue for starfield patterns.</summary>
        public static readonly Color ConstellationBlue = new Color(60, 90, 180);

        /// <summary>Starlight core for intense bright centers.</summary>
        public static readonly Color StarlightCore = new Color(240, 245, 255);

        /// <summary>Warm serenade glow for playful musical accents.</summary>
        public static readonly Color SerenadeGlow = new Color(200, 190, 255);

        /// <summary>Nocturnal teal for subtle cool-shift accents.</summary>
        public static readonly Color NocturnalTeal = new Color(60, 140, 180);

        // =================================================================
        //  TOOLTIP / UI COLORS
        // =================================================================

        /// <summary>Lore text color for ModifyTooltips.</summary>
        public static readonly Color LoreText = new Color(80, 120, 200);

        /// <summary>Special weapon effect tooltip color.</summary>
        public static readonly Color EffectTooltip = new Color(200, 210, 240);

        // =================================================================
        //  6-COLOR SWING PALETTES  (per-weapon musical scales)
        // =================================================================

        /// <summary>
        /// NocturnalExecutioner blade palette — midnight authority ascending to gold flash.
        /// Heavy, commanding, the executioner's power. Every swing is a royal decree.
        /// </summary>
        public static readonly Color[] NocturnalExecutionerBlade = new Color[]
        {
            new Color(15, 15, 45),      // [0] Pianissimo — midnight void
            new Color(50, 30, 100),     // [1] Piano — cosmic purple depths
            new Color(123, 104, 238),   // [2] Mezzo — violet authority
            new Color(200, 210, 240),   // [3] Forte — starlit flash
            new Color(255, 230, 150),   // [4] Fortissimo — gold radiance
            new Color(255, 248, 235),   // [5] Sforzando — white-hot decree
        };

        /// <summary>
        /// MidnightsCrescendo blade palette — deep blue building to starlit climax.
        /// Rising intensity, musical crescendo. Every swing builds toward brilliance.
        /// </summary>
        public static readonly Color[] MidnightsCrescendoBlade = new Color[]
        {
            new Color(20, 20, 55),      // [0] Pianissimo — deep night
            new Color(30, 50, 120),     // [1] Piano — nocturnal base
            new Color(80, 120, 200),    // [2] Mezzo — starlit blue crescendo
            new Color(160, 180, 240),   // [3] Forte — rising brilliance
            new Color(230, 235, 245),   // [4] Fortissimo — moonlit silver peak
            new Color(248, 250, 255),   // [5] Sforzando — twinkling white climax
        };

        /// <summary>
        /// TwilightSeverance blade palette — dusk transition through starlit edge.
        /// Boundary between day and night, twilight's razor. Every cut severs light from dark.
        /// </summary>
        public static readonly Color[] TwilightSeveranceBlade = new Color[]
        {
            new Color(25, 20, 60),      // [0] Pianissimo — dusk shadow
            new Color(100, 80, 180),    // [1] Piano — dusk violet edge
            new Color(80, 120, 200),    // [2] Mezzo — starlit blue body
            new Color(180, 120, 200),   // [3] Forte — nebula pink accent
            new Color(210, 220, 240),   // [4] Fortissimo — silver twilight
            new Color(248, 250, 255),   // [5] Sforzando — white severance
        };

        /// <summary>
        /// ConstellationPiercer projectile palette — pinpoint starlight through deep sky.
        /// Precise, piercing, each shot draws a constellation line.
        /// </summary>
        public static readonly Color[] ConstellationPiercerShot = new Color[]
        {
            new Color(15, 15, 45),      // [0] Pianissimo — night sky void
            new Color(60, 90, 180),     // [1] Piano — constellation blue
            new Color(80, 120, 200),    // [2] Mezzo — starlit blue
            new Color(200, 210, 240),   // [3] Forte — star white
            new Color(255, 230, 150),   // [4] Fortissimo — star gold
            new Color(255, 255, 240),   // [5] Sforzando — star point white
        };

        /// <summary>
        /// NebulasWhisper projectile palette — soft cosmic whisper through nebula mist.
        /// Gentle, ethereal, each shot is a whispered secret of the cosmos.
        /// </summary>
        public static readonly Color[] NebulasWhisperShot = new Color[]
        {
            new Color(30, 25, 70),      // [0] Pianissimo — deep nebula
            new Color(75, 50, 130),     // [1] Piano — cosmic purple
            new Color(180, 120, 200),   // [2] Mezzo — nebula pink
            new Color(200, 190, 255),   // [3] Forte — serenade glow
            new Color(230, 235, 245),   // [4] Fortissimo — moonlit silver
            new Color(248, 250, 255),   // [5] Sforzando — whisper white
        };

        /// <summary>
        /// SerenadeOfDistantStars projectile palette — warm starlight melody through night sky.
        /// Romantic, sweeping, each shot is a note in the night's song.
        /// </summary>
        public static readonly Color[] SerenadeOfDistantStarsShot = new Color[]
        {
            new Color(15, 15, 45),      // [0] Pianissimo — midnight base
            new Color(30, 50, 120),     // [1] Piano — deep blue
            new Color(80, 120, 200),    // [2] Mezzo — starlit blue
            new Color(200, 210, 240),   // [3] Forte — star white
            new Color(230, 235, 245),   // [4] Fortissimo — moonlit silver
            new Color(255, 250, 240),   // [5] Sforzando — warm starlight
        };

        /// <summary>
        /// StarweaversGrimoire cast palette — mystical star-weaving through arcane night.
        /// Intricate, magical, each cast weaves a new pattern in the stars.
        /// </summary>
        public static readonly Color[] StarweaversGrimoireCast = new Color[]
        {
            new Color(20, 15, 50),      // [0] Pianissimo — arcane void
            new Color(75, 50, 130),     // [1] Piano — cosmic purple weave
            new Color(123, 104, 238),   // [2] Mezzo — violet thread
            new Color(200, 190, 255),   // [3] Forte — serenade glow
            new Color(230, 235, 245),   // [4] Fortissimo — woven starlight
            new Color(248, 250, 255),   // [5] Sforzando — star pattern white
        };

        /// <summary>
        /// RequiemOfTheCosmos cast palette — somber cosmic finale from void to supernova.
        /// Grand, final, each cast echoes through the cosmos.
        /// </summary>
        public static readonly Color[] RequiemOfTheCosmosCast = new Color[]
        {
            new Color(10, 10, 30),      // [0] Pianissimo — cosmic void
            new Color(30, 50, 120),     // [1] Piano — deep blue requiem
            new Color(80, 120, 200),    // [2] Mezzo — starlit blue
            new Color(255, 215, 0),     // [3] Forte — radiance gold
            new Color(255, 240, 200),   // [4] Fortissimo — supernova glow
            new Color(255, 252, 245),   // [5] Sforzando — cosmic white
        };

        /// <summary>
        /// CelestialChorusBaton minion palette — orchestral celestial harmony.
        /// Graceful, choral, the baton commands celestial voices.
        /// </summary>
        public static readonly Color[] CelestialChorusMinion = new Color[]
        {
            new Color(15, 15, 45),      // [0] Pianissimo — night void
            new Color(60, 90, 180),     // [1] Piano — constellation blue
            new Color(123, 104, 238),   // [2] Mezzo — violet harmony
            new Color(200, 210, 240),   // [3] Forte — star white
            new Color(230, 235, 245),   // [4] Fortissimo — moonlit silver
            new Color(248, 250, 255),   // [5] Sforzando — chorus white
        };

        /// <summary>
        /// GalacticOverture minion palette — grand opening through galactic radiance.
        /// Sweeping, dramatic, the overture that announces the queen's arrival.
        /// </summary>
        public static readonly Color[] GalacticOvertureMinion = new Color[]
        {
            new Color(10, 10, 30),      // [0] Pianissimo — galactic void
            new Color(30, 50, 120),     // [1] Piano — deep blue opening
            new Color(80, 120, 200),    // [2] Mezzo — starlit blue fanfare
            new Color(255, 215, 0),     // [3] Forte — radiance gold
            new Color(255, 230, 150),   // [4] Fortissimo — star gold
            new Color(255, 248, 235),   // [5] Sforzando — overture white
        };

        /// <summary>
        /// ConductorOfConstellations minion palette — commanding starfield authority.
        /// Precise, commanding, stars bow to the conductor's will.
        /// </summary>
        public static readonly Color[] ConductorOfConstellationsMinion = new Color[]
        {
            new Color(15, 15, 45),      // [0] Pianissimo — midnight void
            new Color(60, 90, 180),     // [1] Piano — constellation blue
            new Color(80, 120, 200),    // [2] Mezzo — starlit blue
            new Color(200, 210, 240),   // [3] Forte — star white command
            new Color(255, 230, 150),   // [4] Fortissimo — star gold baton
            new Color(248, 250, 255),   // [5] Sforzando — conductor's white
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
        /// Standard Nachtmusik gradient: DeepBlue -> StarlitBlue over 0->1.
        /// Use for generic theme effects, halos, cascading rings.
        /// </summary>
        public static Color GetGradient(float progress)
            => Color.Lerp(DeepBlue, StarlitBlue, progress);

        /// <summary>
        /// Celestial gradient: MidnightBlue -> StarlitBlue -> TwinklingWhite over 0->1.
        /// Use for star-cycling effects and celestial overlays.
        /// </summary>
        public static Color GetCelestialGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(MidnightBlue, StarlitBlue, progress * 2f);
            return Color.Lerp(StarlitBlue, TwinklingWhite, (progress - 0.5f) * 2f);
        }

        /// <summary>
        /// Nocturnal gradient: DeepBlue -> Violet -> StarGold over 0->1.
        /// Use for the Queen's radiance bursts, golden accents on starlit foundation.
        /// </summary>
        public static Color GetNocturnalGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(DeepBlue, Violet, progress * 2f);
            return Color.Lerp(Violet, StarGold, (progress - 0.5f) * 2f);
        }

        /// <summary>
        /// Starfield gradient: CosmicVoid -> ConstellationBlue -> StarWhite over 0->1.
        /// Use for constellation patterns and starfield effects.
        /// </summary>
        public static Color GetStarfieldGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(CosmicVoid, ConstellationBlue, progress * 2f);
            return Color.Lerp(ConstellationBlue, StarWhite, (progress - 0.5f) * 2f);
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
        /// Standard 4-layer PreDrawInWorld bloom for Nachtmusik items.
        /// Call from PreDrawInWorld with additive SpriteBatch.
        /// Nocturnal: DeepBlue outer -> StarlitBlue mid -> StarWhite inner -> TwinklingWhite core.
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
            // Layer 1: Outer deep blue aura
            sb.Draw(tex, pos, null, Additive(DeepBlue, 0.35f), rotation, origin, scale * 1.10f * pulse,
                SpriteEffects.None, 0f);
            // Layer 2: Mid starlit blue glow
            sb.Draw(tex, pos, null, Additive(StarlitBlue, 0.30f), rotation, origin, scale * 1.06f * pulse,
                SpriteEffects.None, 0f);
            // Layer 3: Inner star white shimmer
            sb.Draw(tex, pos, null, Additive(StarWhite, 0.25f), rotation, origin, scale * 1.03f * pulse,
                SpriteEffects.None, 0f);
            // Layer 4: Core twinkling white
            sb.Draw(tex, pos, null, Additive(TwinklingWhite, 0.20f), rotation, origin, scale * 1.01f * pulse,
                SpriteEffects.None, 0f);
        }

        // =================================================================
        //  6-COLOR MASTER PALETTE INTERPOLATION
        // =================================================================

        /// <summary>
        /// The 6 canonical palette stops as an array for indexed interpolation.
        /// [0] MidnightBlue -> [1] DeepBlue -> [2] StarlitBlue -> [3] StarWhite -> [4] MoonlitSilver -> [5] TwinklingWhite.
        /// </summary>
        private static readonly Color[] MasterPalette =
        {
            MidnightBlue, DeepBlue, StarlitBlue, StarWhite, MoonlitSilver, TwinklingWhite
        };

        /// <summary>
        /// Lerp through the 6-colour master palette.
        /// t=0 -> MidnightBlue (Pianissimo), t=1 -> TwinklingWhite (Sforzando).
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
        /// </summary>
        public static Color GetBladeGradient(Color[] bladePalette, float progress)
            => PaletteLerp(bladePalette, progress);

        // =================================================================
        //  HSL CYCLING (for shimmer / color oscillation effects)
        // =================================================================

        /// <summary>
        /// Get a hue-shifted Nachtmusik color for shimmer effects.
        /// Stays within the blue-indigo hue range (0.58->0.72) for nocturnal feel.
        /// </summary>
        public static Color GetShimmer(float time, float sat = 0.65f, float lum = 0.65f)
        {
            float hue = (time * 0.02f) % 1f;
            hue = 0.58f + hue * 0.14f; // Clamp to blue-indigo hue range
            return Main.hslToRgb(hue, sat, lum);
        }

        /// <summary>
        /// Get a hue-shifted starlit color for twinkling effects.
        /// Sweeps through the blue->violet->gold hue range with playful bounce.
        /// </summary>
        public static Color GetStarlitShimmer(float time, float sat = 0.55f, float lum = 0.75f)
        {
            float hue = (time * 0.015f) % 1f;
            hue = 0.55f + hue * 0.20f; // Clamp to blue-violet range
            return Main.hslToRgb(hue, sat, lum);
        }

        /// <summary>
        /// Get a cycling celestial radiance color that pulses between starlit blue and gold.
        /// The queen's signature radiance shimmer.
        /// </summary>
        public static Color GetRadianceShimmer(float time)
        {
            float t = (float)Math.Sin(time * 0.04f) * 0.5f + 0.5f;
            return Color.Lerp(StarlitBlue, RadianceGold, t * 0.6f);
        }

        /// <summary>Parameterless overload using current game time.</summary>
        public static Color GetShimmer() => GetShimmer((float)Main.timeForVisualEffects);

        /// <summary>Parameterless overload using current game time.</summary>
        public static Color GetStarlitShimmer() => GetStarlitShimmer((float)Main.timeForVisualEffects);

        /// <summary>Parameterless overload using current game time.</summary>
        public static Color GetRadianceShimmer() => GetRadianceShimmer((float)Main.timeForVisualEffects);
    }
}
