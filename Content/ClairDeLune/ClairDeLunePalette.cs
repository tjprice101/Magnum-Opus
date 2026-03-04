using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

namespace MagnumOpus.Content.ClairDeLune
{
    /// <summary>
    /// Canonical single source-of-truth for ALL Clair de Lune theme colors.
    /// Every weapon, projectile, accessory, minion, enemy, and VFX helper MUST
    /// reference this file instead of hardcoding Color values inline.
    ///
    /// Palette philosophy (musical dynamics):
    ///   [0] Pianissimo  — night mist (deep blue-gray fog)
    ///   [1] Piano       — midnight blue (rich moonlit depth)
    ///   [2] Mezzo       — soft blue (the dreamy heartbeat)
    ///   [3] Forte       — pearl blue (luminous accent)
    ///   [4] Fortissimo  — pearl white (near-white shimmer)
    ///   [5] Sforzando   — white hot (moonbeam brilliance peak)
    ///
    /// Theme identity: Debussy's moonlit reverie. Dreamy, celestial,
    /// soft pearl-like shimmer. Gentle flowing water, starlit clouds,
    /// impressionistic haze — married with clockwork precision and
    /// temporal power for the supreme final boss tier.
    /// </summary>
    public static class ClairDeLunePalette
    {
        // =================================================================
        //  CORE THEME COLORS  (the 6 pillars every Clair de Lune effect uses)
        // =================================================================

        /// <summary>Deep misty night — the quietest opening, fog over moonlit water.</summary>
        public static readonly Color NightMist = new Color(35, 45, 75);

        /// <summary>Rich midnight blue — the depth beneath still water, moonlit solemnity.</summary>
        public static readonly Color MidnightBlue = new Color(60, 80, 140);

        /// <summary>Dreamy soft blue — the heartbeat of Clair de Lune, impressionistic reverie.</summary>
        public static readonly Color SoftBlue = new Color(100, 140, 200);

        /// <summary>Pearl-tinted blue — luminous moonbeam through cloud, gentle brilliance.</summary>
        public static readonly Color PearlBlue = new Color(160, 195, 235);

        /// <summary>Shimmering pearl white — the fortissimo, moonlight on still water.</summary>
        public static readonly Color PearlWhite = new Color(220, 230, 245);

        /// <summary>Moonbeam white-hot brilliance — the sforzando peak, temporal zenith.</summary>
        public static readonly Color WhiteHot = new Color(245, 248, 255);

        // =================================================================
        //  CONVENIENCE ACCESSORS
        // =================================================================

        /// <summary>Standard dreamy blue used across most weapon files (alias for SoftBlue).</summary>
        public static readonly Color DreamBlue = SoftBlue;

        /// <summary>Standard moonbeam glow used across most weapon files (alias for PearlBlue).</summary>
        public static readonly Color MoonbeamGlow = PearlBlue;

        /// <summary>Warm-shifted pearl for weapon sprite bloom.</summary>
        public static readonly Color WeaponPearl = new Color(180, 210, 240);

        // =================================================================
        //  EXTENDED PALETTE  (specific use-cases across Clair de Lune content)
        // =================================================================

        /// <summary>Absolute deep night for the darkest shadow layers and void aura.</summary>
        public static readonly Color DeepNight = new Color(20, 25, 50);

        /// <summary>Water surface reflection — flowing, shimmering mid-blue.</summary>
        public static readonly Color WaterSurface = new Color(80, 120, 185);

        /// <summary>Impressionistic dream haze for blur and mist overlay effects.</summary>
        public static readonly Color DreamHaze = new Color(120, 150, 210);

        /// <summary>Warm moonbeam gold for crescent accents and warm highlights.</summary>
        public static readonly Color MoonbeamGold = new Color(255, 240, 200);

        /// <summary>Starlight silver for star particle effects and sparkle dust.</summary>
        public static readonly Color StarlightSilver = new Color(200, 210, 240);

        /// <summary>Cloud silver for mist drift particles and atmospheric haze.</summary>
        public static readonly Color CloudSilver = new Color(180, 195, 220);

        /// <summary>Clockwork brass for gear accents and mechanism motifs.</summary>
        public static readonly Color ClockworkBrass = new Color(205, 170, 100);

        /// <summary>Temporal energy for crimson-shifted temporal power accents.</summary>
        public static readonly Color TemporalCrimson = new Color(180, 80, 120);

        /// <summary>Pearl shimmer for ambient sparkle and pearl-like reflections.</summary>
        public static readonly Color PearlShimmer = new Color(200, 215, 240);

        /// <summary>Moonlit frost for cold crystalline accents and impact rings.</summary>
        public static readonly Color MoonlitFrost = new Color(190, 220, 250);

        /// <summary>Pearl frost — crystalline icy white-blue for piercing lance effects (alias for MoonlitFrost).</summary>
        public static readonly Color PearlFrost = MoonlitFrost;

        /// <summary>Silver lining — bright silver-white for secondary accents and sigil highlights (alias for StarlightSilver).</summary>
        public static readonly Color SilverLining = StarlightSilver;

        // =================================================================
        //  TOOLTIP / UI COLORS
        // =================================================================

        /// <summary>Lore text color for ModifyTooltips.</summary>
        public static readonly Color LoreText = new Color(100, 140, 200);

        /// <summary>Special weapon effect tooltip color.</summary>
        public static readonly Color EffectTooltip = new Color(160, 195, 235);

        // =================================================================
        //  6-COLOR SWING PALETTES  (per-weapon musical scales)
        // =================================================================

        /// <summary>
        /// Chronologicality blade palette — temporal drill cutting through time itself.
        /// Powerful, relentless, time-rending. Every thrust tears the fabric of reality.
        /// </summary>
        public static readonly Color[] ChronologicalityBlade = new Color[]
        {
            new Color(35, 45, 75),      // [0] Pianissimo — night mist
            new Color(60, 80, 140),     // [1] Piano — midnight depth
            new Color(100, 140, 200),   // [2] Mezzo — soft blue body
            new Color(180, 80, 120),    // [3] Forte — temporal crimson flare
            new Color(220, 230, 245),   // [4] Fortissimo — pearl white
            new Color(245, 248, 255),   // [5] Sforzando — white-hot temporal rift
        };

        /// <summary>
        /// Temporal Piercer lance palette — crystalline lance of frozen time.
        /// Precise, piercing, crystalline. Every thrust is a needle through eternity.
        /// </summary>
        public static readonly Color[] TemporalPiercerLance = new Color[]
        {
            new Color(50, 60, 100),     // [0] Pianissimo — frosted night
            new Color(80, 120, 185),    // [1] Piano — water surface shimmer
            new Color(160, 195, 235),   // [2] Mezzo — pearl blue body
            new Color(190, 220, 250),   // [3] Forte — moonlit frost
            new Color(220, 230, 245),   // [4] Fortissimo — pearl white
            new Color(255, 255, 255),   // [5] Sforzando — absolute crystal white
        };

        /// <summary>
        /// Clockwork Harmony greatsword palette — brass mechanism meets moonlit steel.
        /// Heavy, rhythmic, mechanical. Every swing is a clockwork pendulum.
        /// </summary>
        public static readonly Color[] ClockworkHarmonyBlade = new Color[]
        {
            new Color(35, 45, 75),      // [0] Pianissimo — night mist
            new Color(120, 100, 60),    // [1] Piano — aged brass shadow
            new Color(205, 170, 100),   // [2] Mezzo — clockwork brass body
            new Color(160, 195, 235),   // [3] Forte — pearl blue accent
            new Color(255, 240, 200),   // [4] Fortissimo — moonbeam gold
            new Color(245, 248, 255),   // [5] Sforzando — white-hot mechanism
        };

        /// <summary>
        /// Clockwork Grimoire spell palette — arcane pages of temporal knowledge.
        /// Mystical, channeled, building. Every spell is a page turning in eternity.
        /// </summary>
        public static readonly Color[] ClockworkGrimoireCast = new Color[]
        {
            new Color(20, 25, 50),      // [0] Pianissimo — deep night
            new Color(60, 80, 140),     // [1] Piano — midnight blue
            new Color(100, 140, 200),   // [2] Mezzo — soft blue body
            new Color(160, 195, 235),   // [3] Forte — pearl blue
            new Color(200, 215, 240),   // [4] Fortissimo — pearl shimmer
            new Color(245, 248, 255),   // [5] Sforzando — white-hot arcane flash
        };

        /// <summary>
        /// Orrery of Dreams orbit palette — celestial spheres in dreamy orbit.
        /// Cosmic, orbiting, dreamlike. Every sphere is a dream in motion.
        /// </summary>
        public static readonly Color[] OrreryOfDreamsOrbit = new Color[]
        {
            new Color(20, 25, 50),      // [0] Pianissimo — cosmic deep night
            new Color(60, 80, 140),     // [1] Piano — midnight orbit
            new Color(120, 150, 210),   // [2] Mezzo — dream haze body
            new Color(200, 210, 240),   // [3] Forte — starlight silver
            new Color(255, 240, 200),   // [4] Fortissimo — moonbeam gold
            new Color(255, 255, 255),   // [5] Sforzando — cosmic brilliance
        };

        /// <summary>
        /// Requiem of Time sweep palette — time-freeze magic sword.
        /// Sweeping, freezing, absolute. Every arc stops the clock.
        /// </summary>
        public static readonly Color[] RequiemOfTimeSweep = new Color[]
        {
            new Color(35, 45, 75),      // [0] Pianissimo — night mist
            new Color(80, 120, 185),    // [1] Piano — water surface
            new Color(180, 80, 120),    // [2] Mezzo — temporal crimson body
            new Color(160, 195, 235),   // [3] Forte — pearl blue
            new Color(220, 230, 245),   // [4] Fortissimo — pearl white
            new Color(245, 248, 255),   // [5] Sforzando — white-hot time freeze
        };

        /// <summary>
        /// Starfall Whisper shot palette — sniper rifle tracing starlit paths.
        /// Precise, distant, starlit. Every shot is a falling star.
        /// </summary>
        public static readonly Color[] StarfallWhisperShot = new Color[]
        {
            new Color(20, 25, 50),      // [0] Pianissimo — deep night sky
            new Color(60, 80, 140),     // [1] Piano — midnight blue
            new Color(100, 140, 200),   // [2] Mezzo — soft blue trail
            new Color(200, 210, 240),   // [3] Forte — starlight silver
            new Color(255, 240, 200),   // [4] Fortissimo — moonbeam gold
            new Color(255, 255, 255),   // [5] Sforzando — star impact white
        };

        /// <summary>
        /// Midnight Mechanism burst palette — gatling gun spinning in the dark.
        /// Relentless, mechanical, intensifying. Every bullet is a clock tick.
        /// </summary>
        public static readonly Color[] MidnightMechanismBurst = new Color[]
        {
            new Color(35, 45, 75),      // [0] Pianissimo — night mist
            new Color(120, 100, 60),    // [1] Piano — brass mechanism
            new Color(205, 170, 100),   // [2] Mezzo — clockwork brass body
            new Color(180, 80, 120),    // [3] Forte — temporal crimson flare
            new Color(255, 240, 200),   // [4] Fortissimo — moonbeam gold
            new Color(245, 248, 255),   // [5] Sforzando — muzzle flash white
        };

        /// <summary>
        /// Cog and Hammer launch palette — clockwork bombs across moonlit sky.
        /// Heavy, explosive, mechanical. Every launch is a cannon's report.
        /// </summary>
        public static readonly Color[] CogAndHammerLaunch = new Color[]
        {
            new Color(35, 45, 75),      // [0] Pianissimo — night mist
            new Color(60, 80, 140),     // [1] Piano — midnight blue
            new Color(205, 170, 100),   // [2] Mezzo — clockwork brass
            new Color(160, 195, 235),   // [3] Forte — pearl blue
            new Color(220, 230, 245),   // [4] Fortissimo — pearl white
            new Color(245, 248, 255),   // [5] Sforzando — detonation white
        };

        /// <summary>
        /// Lunar Phylactery summon palette — dreamy soul vessel channeling moonlight.
        /// Ethereal, accumulating, luminous. Every kill empowers the vessel.
        /// </summary>
        public static readonly Color[] LunarPhylacterySummon = new Color[]
        {
            new Color(20, 25, 50),      // [0] Pianissimo — deep night
            new Color(60, 80, 140),     // [1] Piano — midnight blue
            new Color(100, 140, 200),   // [2] Mezzo — soft blue body
            new Color(160, 195, 235),   // [3] Forte — pearl blue
            new Color(220, 230, 245),   // [4] Fortissimo — pearl white
            new Color(245, 248, 255),   // [5] Sforzando — soul release white
        };

        /// <summary>
        /// Gear-Driven Arbiter summon palette — clockwork judge marking targets.
        /// Judicial, mechanical, precise. Every mark is a temporal verdict.
        /// </summary>
        public static readonly Color[] GearDrivenArbiterSummon = new Color[]
        {
            new Color(35, 45, 75),      // [0] Pianissimo — night mist
            new Color(120, 100, 60),    // [1] Piano — aged brass
            new Color(205, 170, 100),   // [2] Mezzo — clockwork brass body
            new Color(180, 80, 120),    // [3] Forte — temporal crimson judgment
            new Color(255, 240, 200),   // [4] Fortissimo — moonbeam gold
            new Color(245, 248, 255),   // [5] Sforzando — verdict white
        };

        /// <summary>
        /// Automaton's Tuning Fork summon palette — resonant support field.
        /// Harmonic, supportive, amplifying. Every resonance strengthens allies.
        /// </summary>
        public static readonly Color[] AutomatonTuningForkSummon = new Color[]
        {
            new Color(20, 25, 50),      // [0] Pianissimo — deep night
            new Color(80, 120, 185),    // [1] Piano — water surface
            new Color(120, 150, 210),   // [2] Mezzo — dream haze body
            new Color(200, 210, 240),   // [3] Forte — starlight silver
            new Color(200, 215, 240),   // [4] Fortissimo — pearl shimmer
            new Color(245, 248, 255),   // [5] Sforzando — resonance peak white
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
        /// Standard Clair de Lune gradient: MidnightBlue -> PearlBlue over 0->1.
        /// Use for generic theme effects, halos, cascading rings.
        /// The classic night-to-moonbeam gradient used across all Clair de Lune content.
        /// </summary>
        public static Color GetGradient(float progress)
            => Color.Lerp(MidnightBlue, PearlBlue, progress);

        /// <summary>
        /// Moonlit water gradient: NightMist -> SoftBlue -> PearlWhite over 0->1.
        /// Use for dreamy water reflections, flowing trails, impressionistic effects.
        /// </summary>
        public static Color GetMoonlitGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(NightMist, SoftBlue, progress * 2f);
            return Color.Lerp(SoftBlue, PearlWhite, (progress - 0.5f) * 2f);
        }

        /// <summary>
        /// Clair de Lune full palette gradient: NightMist -> SoftBlue -> PearlWhite over 0->1.
        /// Drop-in replacement for all inline GetClairDeLuneGradient methods.
        /// </summary>
        public static Color GetClairDeLuneGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(NightMist, SoftBlue, progress * 2f);
            return Color.Lerp(SoftBlue, PearlWhite, (progress - 0.5f) * 2f);
        }

        /// <summary>
        /// Pearl shimmer gradient: SoftBlue -> PearlWhite -> WhiteHot over 0->1.
        /// Use for pearl sparkle effects, moonbeam cores, bright shimmer accents.
        /// </summary>
        public static Color GetPearlGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(SoftBlue, PearlWhite, progress * 2f);
            return Color.Lerp(PearlWhite, WhiteHot, (progress - 0.5f) * 2f);
        }

        /// <summary>
        /// Clockwork gradient: NightMist -> ClockworkBrass -> MoonbeamGold over 0->1.
        /// Use for gear mechanisms, brass accents, clockwork weapon effects.
        /// </summary>
        public static Color GetClockworkGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(NightMist, ClockworkBrass, progress * 2f);
            return Color.Lerp(ClockworkBrass, MoonbeamGold, (progress - 0.5f) * 2f);
        }

        /// <summary>
        /// Temporal energy gradient: DeepNight -> TemporalCrimson -> PearlWhite over 0->1.
        /// Use for temporal power effects, time distortion, energy bursts.
        /// </summary>
        public static Color GetTemporalGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(DeepNight, TemporalCrimson, progress * 2f);
            return Color.Lerp(TemporalCrimson, PearlWhite, (progress - 0.5f) * 2f);
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
        /// Standard 3-layer PreDrawInWorld bloom for Clair de Lune items.
        /// Dreamy soft blue outer -> pearl blue mid -> pearl white inner.
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
            // Layer 1: Outer midnight blue aura
            sb.Draw(tex, pos, null, Additive(MidnightBlue, 0.40f), rotation, origin, scale * 1.08f * pulse,
                SpriteEffects.None, 0f);
            // Layer 2: Middle soft blue glow
            sb.Draw(tex, pos, null, Additive(SoftBlue, 0.30f), rotation, origin, scale * 1.04f * pulse,
                SpriteEffects.None, 0f);
            // Layer 3: Inner pearl white core
            sb.Draw(tex, pos, null, Additive(PearlWhite, 0.22f), rotation, origin, scale * 1.01f * pulse,
                SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Enhanced 4-layer PreDrawInWorld bloom with pearl-blue color shift.
        /// For higher-tier Clair de Lune items with more intense moonlit aura.
        /// </summary>
        public static void DrawItemBloomEnhanced(
            SpriteBatch sb,
            Texture2D tex,
            Vector2 pos,
            Vector2 origin,
            float rotation,
            float scale,
            float pulse,
            float time)
        {
            float colorShift = (float)Math.Sin(time * 0.6f) * 0.5f + 0.5f;
            Color midColor = Color.Lerp(SoftBlue, PearlBlue, colorShift);

            // Layer 1: Outer midnight blue aura
            sb.Draw(tex, pos, null, Additive(MidnightBlue, 0.35f), rotation, origin, scale * 1.12f * pulse,
                SpriteEffects.None, 0f);
            // Layer 2: Middle shifting dreamy glow
            sb.Draw(tex, pos, null, Additive(midColor, 0.28f), rotation, origin, scale * 1.06f * pulse,
                SpriteEffects.None, 0f);
            // Layer 3: Inner pearl blue
            sb.Draw(tex, pos, null, Additive(PearlBlue, 0.25f), rotation, origin, scale * 1.03f * pulse,
                SpriteEffects.None, 0f);
            // Layer 4: Core white-hot moonbeam
            sb.Draw(tex, pos, null, Additive(WhiteHot, 0.15f), rotation, origin, scale * 1.00f * pulse,
                SpriteEffects.None, 0f);
        }

        // =================================================================
        //  6-COLOR MASTER PALETTE INTERPOLATION
        // =================================================================

        /// <summary>
        /// The 6 canonical palette stops as an array for indexed interpolation.
        /// [0] NightMist -> [1] MidnightBlue -> [2] SoftBlue -> [3] PearlBlue -> [4] PearlWhite -> [5] WhiteHot.
        /// </summary>
        private static readonly Color[] MasterPalette =
        {
            NightMist, MidnightBlue, SoftBlue, PearlBlue, PearlWhite, WhiteHot
        };

        /// <summary>
        /// Lerp through the 6-colour master palette.
        /// t=0 -> NightMist (Pianissimo), t=1 -> WhiteHot (Sforzando).
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
        /// Works with ChronologicalityBlade, TemporalPiercerLance, ClockworkHarmonyBlade, etc.
        /// </summary>
        public static Color GetBladeGradient(Color[] bladePalette, float progress)
            => PaletteLerp(bladePalette, progress);

        // =================================================================
        //  HSL CYCLING (for shimmer / color oscillation effects)
        // =================================================================

        /// <summary>
        /// Get a hue-shifted Clair de Lune color for shimmer effects.
        /// Stays within the blue-pearl hue range (0.55->0.68) for dreamy moonlit shimmer.
        /// </summary>
        public static Color GetShimmer(float time, float sat = 0.55f, float lum = 0.70f)
        {
            float hue = (time * 0.015f) % 1f;
            hue = 0.55f + hue * 0.13f; // Clamp to blue-pearl hue range
            return Main.hslToRgb(hue, sat, lum);
        }

        /// <summary>
        /// Get a hue-shifted pearl color for pearl shimmer effects.
        /// Sweeps through the soft-blue -> pearl-white range with low saturation.
        /// </summary>
        public static Color GetPearlShimmer(float time, float sat = 0.30f, float lum = 0.82f)
        {
            float hue = (time * 0.012f) % 1f;
            hue = 0.57f + hue * 0.10f; // Narrow pearl-blue hue band
            return Main.hslToRgb(hue, sat, lum);
        }

        /// <summary>
        /// Get a hue-shifted clockwork color for mechanism shimmer effects.
        /// Sweeps through the brass-gold hue range (0.10->0.15).
        /// </summary>
        public static Color GetClockworkShimmer(float time, float sat = 0.60f, float lum = 0.55f)
        {
            float hue = (time * 0.02f) % 1f;
            hue = 0.10f + hue * 0.05f; // Narrow brass-gold hue band
            return Main.hslToRgb(hue, sat, lum);
        }
    }
}
