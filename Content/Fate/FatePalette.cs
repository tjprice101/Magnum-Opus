using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

namespace MagnumOpus.Content.Fate
{
    /// <summary>
    /// Canonical single source-of-truth for ALL Fate theme colors.
    /// Every weapon, projectile, accessory, minion, enemy, and VFX helper MUST
    /// reference this file instead of hardcoding Color values inline.
    ///
    /// Palette philosophy (musical dynamics — Beethoven's fate knocking):
    ///   [0] Pianissimo  — cosmic void (the infinite dark between stars)
    ///   [1] Piano       — dark pink (fate's whisper, destiny stirring)
    ///   [2] Mezzo       — bright crimson (fate's heartbeat, the knock)
    ///   [3] Forte       — star gold (destiny revealed, cosmic power)
    ///   [4] Fortissimo  — white celestial (the heavens answer)
    ///   [5] Sforzando   — supernova white (fate fulfilled, blinding truth)
    /// </summary>
    public static class FatePalette
    {
        // =================================================================
        //  CORE THEME COLORS  (the 6 pillars every Fate effect uses)
        // =================================================================

        /// <summary>Cosmic void — the infinite dark between stars, the silence before fate knocks.</summary>
        public static readonly Color CosmicVoid = new Color(15, 5, 20);

        /// <summary>Dark pink — fate's whisper, destiny stirring in the darkness.</summary>
        public static readonly Color DarkPink = new Color(180, 50, 100);

        /// <summary>Bright crimson — fate's heartbeat, the knock on the door of destiny.</summary>
        public static readonly Color BrightCrimson = new Color(255, 60, 80);

        /// <summary>Star gold — destiny revealed, cosmic power blazing from the heavens.</summary>
        public static readonly Color StarGold = new Color(255, 230, 180);

        /// <summary>White celestial — the heavens answer, pure celestial radiance.</summary>
        public static readonly Color WhiteCelestial = new Color(255, 255, 255);

        /// <summary>Supernova white — fate fulfilled, blinding supernova truth.</summary>
        public static readonly Color SupernovaWhite = new Color(255, 255, 250);

        // =================================================================
        //  CONVENIENCE ACCESSORS
        // =================================================================

        /// <summary>Standard fate pink used across most weapon files (alias for DarkPink).</summary>
        public static readonly Color FatePink = DarkPink;

        /// <summary>Standard fate red used across most weapon files (alias for BrightCrimson).</summary>
        public static readonly Color FateRed = BrightCrimson;

        /// <summary>Warm-shifted crimson for weapon sprite bloom.</summary>
        public static readonly Color WeaponCrimson = new Color(255, 80, 100);

        // =================================================================
        //  EXTENDED PALETTE  (specific use-cases across Fate content)
        // =================================================================

        /// <summary>Fate purple for nebula energy and cosmic cloud interiors.</summary>
        public static readonly Color FatePurple = new Color(120, 30, 140);

        /// <summary>Nebula purple for softer cosmic cloud accents.</summary>
        public static readonly Color NebulaPurple = new Color(160, 80, 200);

        /// <summary>Cosmic black for the deepest void layers.</summary>
        public static readonly Color CosmicBlack = new Color(15, 5, 20);

        /// <summary>Fate cyan for lightning and electrical cosmic effects.</summary>
        public static readonly Color FateCyan = new Color(100, 200, 255);

        /// <summary>Constellation silver for star connection lines and glyph accents.</summary>
        public static readonly Color ConstellationSilver = new Color(200, 210, 240);

        /// <summary>Destiny flame for fiery cosmic attacks and burn effects.</summary>
        public static readonly Color DestinyFlame = new Color(255, 120, 60);

        /// <summary>Cosmic dawn for transitional glow between void and fire.</summary>
        public static readonly Color CosmicDawn = new Color(220, 100, 80);

        /// <summary>Nebula mist for ambient particle drifts and aura edges.</summary>
        public static readonly Color NebulaMist = new Color(140, 60, 120);

        /// <summary>Stellar core for the hottest centers of cosmic explosions.</summary>
        public static readonly Color StellarCore = new Color(255, 240, 220);

        /// <summary>Cosmic rose for gentler pink accents on accessories and auras.</summary>
        public static readonly Color CosmicRose = new Color(220, 80, 130);

        // =================================================================
        //  TOOLTIP / UI COLORS
        // =================================================================

        /// <summary>Lore text color for ModifyTooltips.</summary>
        public static readonly Color LoreText = new Color(180, 40, 80);

        /// <summary>Special weapon effect tooltip color.</summary>
        public static readonly Color EffectTooltip = new Color(255, 230, 180);

        // =================================================================
        //  6-COLOR SWING PALETTES  (per-weapon celestial scales)
        // =================================================================

        /// <summary>
        /// CodaOfAnnihilation palette — the final statement, annihilation incarnate.
        /// Every swing ends a universe. Void → crimson fury → supernova obliteration.
        /// </summary>
        public static readonly Color[] CodaAnnihilation = new Color[]
        {
            new Color(15, 5, 20),       // [0] Pianissimo — cosmic void
            new Color(180, 50, 100),    // [1] Piano — dark pink
            new Color(255, 60, 80),     // [2] Mezzo — bright crimson
            new Color(255, 120, 60),    // [3] Forte — destiny flame
            new Color(255, 230, 180),   // [4] Fortissimo — star gold
            new Color(255, 255, 250),   // [5] Sforzando — supernova white
        };

        /// <summary>
        /// DestinysCrescendo palette — growing cosmic power, momentum building.
        /// Each summon adds to the celestial symphony. Whisper → crescendo → apotheosis.
        /// </summary>
        public static readonly Color[] DestinyCrescendo = new Color[]
        {
            new Color(15, 5, 20),       // [0] Pianissimo — cosmic void
            new Color(120, 30, 140),    // [1] Piano — fate purple
            new Color(180, 50, 100),    // [2] Mezzo — dark pink
            new Color(255, 60, 80),     // [3] Forte — bright crimson
            new Color(255, 230, 180),   // [4] Fortissimo — star gold
            new Color(255, 255, 255),   // [5] Sforzando — white celestial
        };

        /// <summary>
        /// FractalOfTheStars palette — infinite recursive starlight, kaleidoscopic cosmos.
        /// Each fractal reveals another layer of the universe. Star → prism → infinity.
        /// </summary>
        public static readonly Color[] StarFractal = new Color[]
        {
            new Color(15, 5, 20),       // [0] Pianissimo — cosmic void
            new Color(100, 200, 255),   // [1] Piano — fate cyan
            new Color(160, 80, 200),    // [2] Mezzo — nebula purple
            new Color(255, 60, 80),     // [3] Forte — bright crimson
            new Color(255, 230, 180),   // [4] Fortissimo — star gold
            new Color(255, 255, 250),   // [5] Sforzando — supernova white
        };

        /// <summary>
        /// LightOfTheFuture palette — dawning hope, the path ahead illuminated.
        /// Every beam shows what will be. Dawn → golden promise → celestial certainty.
        /// </summary>
        public static readonly Color[] FutureLight = new Color[]
        {
            new Color(15, 5, 20),       // [0] Pianissimo — cosmic void
            new Color(220, 100, 80),    // [1] Piano — cosmic dawn
            new Color(255, 120, 60),    // [2] Mezzo — destiny flame
            new Color(255, 230, 180),   // [3] Forte — star gold
            new Color(255, 255, 255),   // [4] Fortissimo — white celestial
            new Color(255, 255, 250),   // [5] Sforzando — supernova white
        };

        /// <summary>
        /// OpusUltima palette — the magnum opus, transcendent perfection.
        /// The ultimate work, beyond all others. All colors harmonize into cosmic purity.
        /// </summary>
        public static readonly Color[] OpusUltimaPalette = new Color[]
        {
            new Color(15, 5, 20),       // [0] Pianissimo — cosmic void
            new Color(180, 50, 100),    // [1] Piano — dark pink
            new Color(255, 60, 80),     // [2] Mezzo — bright crimson
            new Color(255, 230, 180),   // [3] Forte — star gold
            new Color(255, 255, 255),   // [4] Fortissimo — white celestial
            new Color(255, 255, 250),   // [5] Sforzando — supernova white
        };

        /// <summary>
        /// RequiemOfReality palette — reality's funeral, cosmic entropy.
        /// What was real dissolves into void. Structure → collapse → void silence.
        /// </summary>
        public static readonly Color[] RealityRequiem = new Color[]
        {
            new Color(15, 5, 20),       // [0] Pianissimo — cosmic void
            new Color(120, 30, 140),    // [1] Piano — fate purple
            new Color(255, 60, 80),     // [2] Mezzo — bright crimson
            new Color(180, 50, 100),    // [3] Forte — dark pink
            new Color(200, 210, 240),   // [4] Fortissimo — constellation silver
            new Color(255, 255, 250),   // [5] Sforzando — supernova white
        };

        /// <summary>
        /// ResonanceOfABygoneReality palette — echoes of what was, ghost frequencies.
        /// Past realities bleeding through. Memory → echo → phantom → dissolution.
        /// </summary>
        public static readonly Color[] BygoneResonance = new Color[]
        {
            new Color(15, 5, 20),       // [0] Pianissimo — cosmic void
            new Color(140, 60, 120),    // [1] Piano — nebula mist
            new Color(160, 80, 200),    // [2] Mezzo — nebula purple
            new Color(220, 80, 130),    // [3] Forte — cosmic rose
            new Color(255, 230, 180),   // [4] Fortissimo — star gold
            new Color(200, 210, 240),   // [5] Sforzando — constellation silver
        };

        /// <summary>
        /// SymphonysEnd palette — the final note, cosmic silence after the last chord.
        /// Everything fades. Grand climax → resonance → silence → void.
        /// </summary>
        public static readonly Color[] SymphonyEnd = new Color[]
        {
            new Color(15, 5, 20),       // [0] Pianissimo — cosmic void
            new Color(180, 50, 100),    // [1] Piano — dark pink
            new Color(255, 60, 80),     // [2] Mezzo — bright crimson
            new Color(255, 240, 220),   // [3] Forte — stellar core
            new Color(255, 255, 255),   // [4] Fortissimo — white celestial
            new Color(255, 255, 250),   // [5] Sforzando — supernova white
        };

        /// <summary>
        /// TheConductorsLastConstellation palette — the final star map drawn.
        /// The conductor's last work inscribed across the sky. Star → constellation → eternity.
        /// </summary>
        public static readonly Color[] LastConstellation = new Color[]
        {
            new Color(15, 5, 20),       // [0] Pianissimo — cosmic void
            new Color(100, 200, 255),   // [1] Piano — fate cyan
            new Color(200, 210, 240),   // [2] Mezzo — constellation silver
            new Color(255, 230, 180),   // [3] Forte — star gold
            new Color(255, 255, 255),   // [4] Fortissimo — white celestial
            new Color(255, 255, 250),   // [5] Sforzando — supernova white
        };

        /// <summary>
        /// TheFinalFermata palette — time frozen, the held note that never ends.
        /// Suspended between sound and silence. Tension → suspension → eternal hold.
        /// </summary>
        public static readonly Color[] FinalFermata = new Color[]
        {
            new Color(15, 5, 20),       // [0] Pianissimo — cosmic void
            new Color(220, 80, 130),    // [1] Piano — cosmic rose
            new Color(255, 60, 80),     // [2] Mezzo — bright crimson
            new Color(180, 50, 100),    // [3] Forte — dark pink
            new Color(255, 230, 180),   // [4] Fortissimo — star gold
            new Color(255, 255, 250),   // [5] Sforzando — supernova white
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
        /// Standard Fate gradient: DarkPink -> BrightCrimson -> StarGold over 0->1.
        /// Use for generic theme effects, halos, cascading rings.
        /// </summary>
        public static Color GetGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(DarkPink, BrightCrimson, progress * 2f);
            return Color.Lerp(BrightCrimson, StarGold, (progress - 0.5f) * 2f);
        }

        /// <summary>
        /// Fate cosmic gradient: CosmicVoid -> DarkPink -> BrightCrimson -> StarGold over 0->1.
        /// Use for cosmic cloud effects, nebula dissolves, depth transitions.
        /// This is the canonical GetCosmicGradient that was in FateCosmicVFX.
        /// </summary>
        public static Color GetCosmicGradient(float progress)
        {
            if (progress < 0.3f)
                return Color.Lerp(CosmicVoid, DarkPink, progress / 0.3f);
            else if (progress < 0.6f)
                return Color.Lerp(DarkPink, BrightCrimson, (progress - 0.3f) / 0.3f);
            else if (progress < 0.85f)
                return Color.Lerp(BrightCrimson, FatePurple, (progress - 0.6f) / 0.25f);
            else
                return Color.Lerp(FatePurple, WhiteCelestial, (progress - 0.85f) / 0.15f);
        }

        /// <summary>
        /// Fate full gradient: CosmicVoid -> DarkPink -> BrightCrimson over 0->1.
        /// Drop-in replacement for inline gradient methods in weapon files.
        /// </summary>
        public static Color GetFateGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(CosmicVoid, BrightCrimson, progress * 2f);
            return Color.Lerp(BrightCrimson, StarGold, (progress - 0.5f) * 2f);
        }

        /// <summary>
        /// Stellar revelation gradient: BrightCrimson -> StarGold -> SupernovaWhite over 0->1.
        /// Use for impact explosions, supernova reveals, destiny unraveling.
        /// </summary>
        public static Color GetRevelationGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(BrightCrimson, StarGold, progress * 2f);
            return Color.Lerp(StarGold, SupernovaWhite, (progress - 0.5f) * 2f);
        }

        /// <summary>
        /// Nebula gradient: FatePurple -> DarkPink -> BrightCrimson over 0->1.
        /// Use for cosmic cloud effects and nebula particle colouring.
        /// </summary>
        public static Color GetNebulaGradient(float progress)
            => Color.Lerp(FatePurple, BrightCrimson, progress);

        /// <summary>
        /// Prismatic cosmic color cycling through the palette.
        /// Use for dynamic cosmic shimmer effects.
        /// </summary>
        public static Color GetPrismaticColor(float time, float offset = 0f)
        {
            float cycle = (time * 0.5f + offset) % 1f;
            return GetCosmicGradient(cycle);
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
        /// Standard 3-layer PreDrawInWorld bloom for Fate items.
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
            // Layer 1: Outer cosmic void aura
            sb.Draw(tex, pos, null, Additive(FatePurple, 0.40f), rotation, origin, scale * 1.08f * pulse,
                SpriteEffects.None, 0f);
            // Layer 2: Middle crimson fate glow
            sb.Draw(tex, pos, null, Additive(BrightCrimson, 0.30f), rotation, origin, scale * 1.04f * pulse,
                SpriteEffects.None, 0f);
            // Layer 3: Inner star gold core
            sb.Draw(tex, pos, null, Additive(StarGold, 0.22f), rotation, origin, scale * 1.01f * pulse,
                SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Enhanced 4-layer PreDrawInWorld bloom with crimson-gold color shift.
        /// For higher-tier Fate items with more intense celestial aura.
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
            float colorShift = (float)Math.Sin(time * 0.8f) * 0.5f + 0.5f;
            Color midColor = Color.Lerp(BrightCrimson, StarGold, colorShift);

            // Layer 1: Outer cosmic purple aura
            sb.Draw(tex, pos, null, Additive(FatePurple, 0.35f), rotation, origin, scale * 1.12f * pulse,
                SpriteEffects.None, 0f);
            // Layer 2: Middle shifting glow
            sb.Draw(tex, pos, null, Additive(midColor, 0.28f), rotation, origin, scale * 1.06f * pulse,
                SpriteEffects.None, 0f);
            // Layer 3: Inner star gold
            sb.Draw(tex, pos, null, Additive(StarGold, 0.25f), rotation, origin, scale * 1.03f * pulse,
                SpriteEffects.None, 0f);
            // Layer 4: Core supernova flash
            sb.Draw(tex, pos, null, Additive(SupernovaWhite, 0.15f), rotation, origin, scale * 1.00f * pulse,
                SpriteEffects.None, 0f);
        }

        // =================================================================
        //  6-COLOR MASTER PALETTE INTERPOLATION
        // =================================================================

        /// <summary>
        /// The 6 canonical palette stops as an array for indexed interpolation.
        /// [0] CosmicVoid -> [1] DarkPink -> [2] BrightCrimson -> [3] StarGold -> [4] WhiteCelestial -> [5] SupernovaWhite.
        /// </summary>
        private static readonly Color[] MasterPalette =
        {
            CosmicVoid, DarkPink, BrightCrimson, StarGold, WhiteCelestial, SupernovaWhite
        };

        /// <summary>
        /// Lerp through the 6-colour master palette.
        /// t=0 -> CosmicVoid (Pianissimo), t=1 -> SupernovaWhite (Sforzando).
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
        /// Works with CodaAnnihilation, DestinyCrescendo, StarFractal, etc.
        /// </summary>
        public static Color GetBladeGradient(Color[] bladePalette, float progress)
            => PaletteLerp(bladePalette, progress);

        // =================================================================
        //  HSL CYCLING (for shimmer / color oscillation effects)
        // =================================================================

        /// <summary>
        /// Get a hue-shifted Fate color for cosmic shimmer effects.
        /// Cycles through the pink-crimson-gold hue range for celestial energy.
        /// </summary>
        public static Color GetShimmer(float time, float sat = 0.85f, float lum = 0.65f)
        {
            float hue = (time * 0.02f) % 1f;
            hue = 0.95f + hue * 0.12f; // Clamp to red-pink cosmic range
            if (hue > 1f) hue -= 1f;
            return Main.hslToRgb(hue, sat, lum);
        }

        /// <summary>
        /// Get a hue-shifted cosmic color for celestial shimmer effects.
        /// Sweeps through the deep pink -> crimson -> gold hue range.
        /// </summary>
        public static Color GetCosmicShimmer(float time, float sat = 0.80f, float lum = 0.55f)
        {
            float hue = (time * 0.015f) % 1f;
            // Sweep from pink (0.95) through red (0.0) to gold (0.12)
            hue = 0.95f + hue * 0.17f;
            if (hue > 1f) hue -= 1f;
            return Main.hslToRgb(hue, sat, lum);
        }
    }
}
