using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

namespace MagnumOpus.Content.EnigmaVariations
{
    /// <summary>
    /// Canonical single source-of-truth for ALL Enigma Variations theme colors.
    /// Every weapon, projectile, accessory, minion, enemy, and VFX helper MUST
    /// reference this file instead of hardcoding Color values inline.
    ///
    /// Palette philosophy (musical dynamics):
    ///   [0] Pianissimo  — void black (the unknowable dark)
    ///   [1] Piano       — deep purple (arcane shadow)
    ///   [2] Mezzo       — purple (the enigma's body)
    ///   [3] Forte       — green flame (eerie revelation)
    ///   [4] Fortissimo  — bright green (mystery unveiled)
    ///   [5] Sforzando   — white-green flash (the answer)
    /// </summary>
    public static class EnigmaPalette
    {
        // =================================================================
        //  CORE THEME COLORS  (the 6 pillars every Enigma effect uses)
        // =================================================================

        /// <summary>Void black — the unknowable, deepest mystery, absolute darkness.</summary>
        public static readonly Color VoidBlack = new Color(15, 10, 20);

        /// <summary>Deep purple — arcane shadow, the question forming in darkness.</summary>
        public static readonly Color DeepPurple = new Color(80, 20, 120);

        /// <summary>Enigma purple — the heartbeat of the mystery, swirling arcane energy.</summary>
        public static readonly Color Purple = new Color(140, 60, 200);

        /// <summary>Eerie green flame — revelation, the unknowable made glimpsed.</summary>
        public static readonly Color GreenFlame = new Color(50, 220, 100);

        /// <summary>Bright green — the mystery unveiled, piercing clarity.</summary>
        public static readonly Color BrightGreen = new Color(120, 255, 160);

        /// <summary>White-green flash — the sforzando peak, the answer blazing forth.</summary>
        public static readonly Color WhiteGreenFlash = new Color(220, 255, 230);

        // =================================================================
        //  CONVENIENCE ACCESSORS
        // =================================================================

        /// <summary>Standard enigma green used across most weapon files (alias for GreenFlame).</summary>
        public static readonly Color EnigmaGreen = GreenFlame;

        /// <summary>Standard enigma purple used across most weapon files (alias for Purple).</summary>
        public static readonly Color EnigmaPurple = Purple;

        /// <summary>Warm-shifted purple for weapon sprite bloom.</summary>
        public static readonly Color WeaponPurple = new Color(160, 80, 220);

        // =================================================================
        //  EXTENDED PALETTE  (specific use-cases across Enigma content)
        // =================================================================

        /// <summary>Absolute void for the deepest shadow layers and background aura.</summary>
        public static readonly Color AbsoluteVoid = new Color(8, 5, 12);

        /// <summary>Watching eye green for eye particle cores and gaze effects.</summary>
        public static readonly Color EyeGreen = new Color(80, 255, 130);

        /// <summary>Mystery glyph purple for orbiting glyph accents.</summary>
        public static readonly Color GlyphPurple = new Color(120, 50, 180);

        /// <summary>Void flame for dark fire effects blending void into flame.</summary>
        public static readonly Color VoidFlame = new Color(60, 180, 90);

        /// <summary>Paradox purple-green for paradox brand effects and debuff visuals.</summary>
        public static readonly Color ParadoxShift = new Color(100, 140, 180);

        /// <summary>Riddle shimmer for homing projectile and riddlebolt trails.</summary>
        public static readonly Color RiddleShimmer = new Color(100, 200, 160);

        /// <summary>Cipher dark for channeled beam cores and shadow interiors.</summary>
        public static readonly Color CipherDark = new Color(40, 15, 60);

        /// <summary>Arcane flash for impact cores and detonation centers.</summary>
        public static readonly Color ArcaneFlash = new Color(180, 255, 200);

        /// <summary>Mystery mist for ambient particle drifts and aura edges.</summary>
        public static readonly Color MysteryMist = new Color(100, 80, 140);

        /// <summary>Unresolved tension for dissonant energy and unstable effects.</summary>
        public static readonly Color UnresolvedTension = new Color(160, 100, 220);

        // =================================================================
        //  TOOLTIP / UI COLORS
        // =================================================================

        /// <summary>Lore text color for ModifyTooltips.</summary>
        public static readonly Color LoreText = new Color(140, 60, 200);

        /// <summary>Special weapon effect tooltip color.</summary>
        public static readonly Color EffectTooltip = new Color(50, 220, 100);

        // =================================================================
        //  6-COLOR SWING PALETTES  (per-weapon musical scales)
        // =================================================================

        /// <summary>
        /// CipherNocturne beam palette — void through piercing green revelation.
        /// Channeled, building, reality-unraveling. Every thread pulled reveals more.
        /// </summary>
        public static readonly Color[] CipherNocturneBeam = new Color[]
        {
            new Color(15, 10, 20),      // [0] Pianissimo — void black
            new Color(80, 20, 120),     // [1] Piano — deep purple
            new Color(140, 60, 200),    // [2] Mezzo — enigma purple
            new Color(50, 220, 100),    // [3] Forte — green flame
            new Color(120, 255, 160),   // [4] Fortissimo — bright green
            new Color(220, 255, 230),   // [5] Sforzando — white-green flash
        };

        /// <summary>
        /// DissonanceOfSecrets orb palette — deep mystery swelling to cascade.
        /// Growing, orbiting, cascading. Every riddle births more questions.
        /// </summary>
        public static readonly Color[] DissonanceOrb = new Color[]
        {
            new Color(8, 5, 12),        // [0] Pianissimo — absolute void
            new Color(60, 15, 100),     // [1] Piano — deep mystery
            new Color(140, 60, 200),    // [2] Mezzo — enigma purple
            new Color(80, 255, 130),    // [3] Forte — watching eye green
            new Color(50, 220, 100),    // [4] Fortissimo — green flame
            new Color(180, 255, 200),   // [5] Sforzando — arcane flash
        };

        /// <summary>
        /// FugueOfTheUnknown palette — layered voices of mystery interweaving.
        /// Contrapuntal, layered, multiplying. Every voice adds another secret.
        /// </summary>
        public static readonly Color[] FugueVoices = new Color[]
        {
            new Color(15, 10, 20),      // [0] Pianissimo — void black
            new Color(100, 50, 180),    // [1] Piano — arcane mid-purple
            new Color(160, 80, 220),    // [2] Mezzo — weapon purple
            new Color(100, 200, 160),   // [3] Forte — riddle shimmer
            new Color(50, 220, 100),    // [4] Fortissimo — green flame
            new Color(220, 255, 230),   // [5] Sforzando — white-green flash
        };

        /// <summary>
        /// TacetsEnigma palette — the silence between mysteries, negative space.
        /// Silent, void-like, absent. Every pause hides a deeper truth.
        /// </summary>
        public static readonly Color[] TacetSilence = new Color[]
        {
            new Color(8, 5, 12),        // [0] Pianissimo — absolute void
            new Color(40, 15, 60),      // [1] Piano — cipher dark
            new Color(80, 20, 120),     // [2] Mezzo — deep purple
            new Color(140, 60, 200),    // [3] Forte — enigma purple
            new Color(60, 180, 90),     // [4] Fortissimo — void flame
            new Color(120, 255, 160),   // [5] Sforzando — bright green
        };

        /// <summary>
        /// TheSilentMeasure palette — calculated precision in the void.
        /// Measured, precise, surgical. Every cut is deliberate silence.
        /// </summary>
        public static readonly Color[] SilentMeasure = new Color[]
        {
            new Color(15, 10, 20),      // [0] Pianissimo — void black
            new Color(80, 20, 120),     // [1] Piano — deep purple
            new Color(120, 50, 180),    // [2] Mezzo — glyph purple
            new Color(160, 100, 220),   // [3] Forte — unresolved tension
            new Color(80, 255, 130),    // [4] Fortissimo — eye green
            new Color(220, 255, 230),   // [5] Sforzando — white-green flash
        };

        /// <summary>
        /// TheUnresolvedCadence palette — tension that never resolves.
        /// Unstable, shifting, questioning. Every swing asks "what if?"
        /// </summary>
        public static readonly Color[] UnresolvedCadence = new Color[]
        {
            new Color(40, 15, 60),      // [0] Pianissimo — cipher dark
            new Color(100, 40, 160),    // [1] Piano — restless purple
            new Color(160, 80, 220),    // [2] Mezzo — weapon purple
            new Color(140, 60, 200),    // [3] Forte — enigma purple
            new Color(50, 220, 100),    // [4] Fortissimo — green flame
            new Color(180, 255, 200),   // [5] Sforzando — arcane flash
        };

        /// <summary>
        /// TheWatchingRefrain palette — eyes everywhere, repeated vigilance.
        /// Watchful, recurring, omniscient. Every refrain sees deeper.
        /// </summary>
        public static readonly Color[] WatchingRefrain = new Color[]
        {
            new Color(15, 10, 20),      // [0] Pianissimo — void black
            new Color(60, 15, 100),     // [1] Piano — deep mystery
            new Color(140, 60, 200),    // [2] Mezzo — enigma purple
            new Color(80, 255, 130),    // [3] Forte — eye green
            new Color(120, 255, 160),   // [4] Fortissimo — bright green
            new Color(220, 255, 230),   // [5] Sforzando — white-green flash
        };

        /// <summary>
        /// VariationsOfTheVoid palette — the void itself as the theme.
        /// Cosmic, empty, absolute. Every variation reveals the void's depth.
        /// </summary>
        public static readonly Color[] VoidVariations = new Color[]
        {
            new Color(8, 5, 12),        // [0] Pianissimo — absolute void
            new Color(15, 10, 20),      // [1] Piano — void black
            new Color(80, 20, 120),     // [2] Mezzo — deep purple
            new Color(140, 60, 200),    // [3] Forte — enigma purple
            new Color(60, 180, 90),     // [4] Fortissimo — void flame
            new Color(100, 200, 160),   // [5] Sforzando — riddle shimmer
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
        /// Standard Enigma gradient: DeepPurple -> GreenFlame over 0->1.
        /// Use for generic theme effects, halos, cascading rings.
        /// This is the classic purple-to-green gradient used across all Enigma content.
        /// </summary>
        public static Color GetGradient(float progress)
            => Color.Lerp(DeepPurple, GreenFlame, progress);

        /// <summary>
        /// Enigma void gradient: VoidBlack -> Purple -> GreenFlame over 0->1.
        /// Use for void swirl effects, mystery dissolves, depth transitions.
        /// </summary>
        public static Color GetVoidGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(VoidBlack, Purple, progress * 2f);
            return Color.Lerp(Purple, GreenFlame, (progress - 0.5f) * 2f);
        }

        /// <summary>
        /// Enigma full gradient: VoidBlack -> DeepPurple -> Purple -> GreenFlame over 0->1.
        /// This is the GetEnigmaGradient() that was duplicated in every weapon file.
        /// Use as the drop-in replacement for all inline GetEnigmaGradient methods.
        /// </summary>
        public static Color GetEnigmaGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(VoidBlack, Purple, progress * 2f);
            return Color.Lerp(Purple, GreenFlame, (progress - 0.5f) * 2f);
        }

        /// <summary>
        /// Arcane revelation gradient: Purple -> BrightGreen -> WhiteGreenFlash over 0->1.
        /// Use for impact explosions, detonation reveals, mystery unraveling.
        /// </summary>
        public static Color GetRevelationGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(Purple, BrightGreen, progress * 2f);
            return Color.Lerp(BrightGreen, WhiteGreenFlash, (progress - 0.5f) * 2f);
        }

        /// <summary>
        /// Eye gaze gradient: DeepPurple -> EyeGreen over 0->1.
        /// Use for watching eye effects, gaze particles, omniscient overlays.
        /// </summary>
        public static Color GetEyeGradient(float progress)
            => Color.Lerp(DeepPurple, EyeGreen, progress);

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
        /// Standard 3-layer PreDrawInWorld bloom for Enigma items.
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
            // Layer 1: Outer deep purple aura
            sb.Draw(tex, pos, null, Additive(DeepPurple, 0.40f), rotation, origin, scale * 1.08f * pulse,
                SpriteEffects.None, 0f);
            // Layer 2: Middle enigma purple glow
            sb.Draw(tex, pos, null, Additive(Purple, 0.30f), rotation, origin, scale * 1.04f * pulse,
                SpriteEffects.None, 0f);
            // Layer 3: Inner green flame core
            sb.Draw(tex, pos, null, Additive(GreenFlame, 0.22f), rotation, origin, scale * 1.01f * pulse,
                SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Enhanced 4-layer PreDrawInWorld bloom with green-purple color shift.
        /// For higher-tier Enigma items with more intense mystery aura.
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
            Color midColor = Color.Lerp(Purple, GreenFlame, colorShift);

            // Layer 1: Outer deep purple aura
            sb.Draw(tex, pos, null, Additive(DeepPurple, 0.35f), rotation, origin, scale * 1.12f * pulse,
                SpriteEffects.None, 0f);
            // Layer 2: Middle shifting glow
            sb.Draw(tex, pos, null, Additive(midColor, 0.28f), rotation, origin, scale * 1.06f * pulse,
                SpriteEffects.None, 0f);
            // Layer 3: Inner green flame
            sb.Draw(tex, pos, null, Additive(GreenFlame, 0.25f), rotation, origin, scale * 1.03f * pulse,
                SpriteEffects.None, 0f);
            // Layer 4: Core white-green flash
            sb.Draw(tex, pos, null, Additive(WhiteGreenFlash, 0.15f), rotation, origin, scale * 1.00f * pulse,
                SpriteEffects.None, 0f);
        }

        // =================================================================
        //  6-COLOR MASTER PALETTE INTERPOLATION
        // =================================================================

        /// <summary>
        /// The 6 canonical palette stops as an array for indexed interpolation.
        /// [0] VoidBlack -> [1] DeepPurple -> [2] Purple -> [3] GreenFlame -> [4] BrightGreen -> [5] WhiteGreenFlash.
        /// </summary>
        private static readonly Color[] MasterPalette =
        {
            VoidBlack, DeepPurple, Purple, GreenFlame, BrightGreen, WhiteGreenFlash
        };

        /// <summary>
        /// Lerp through the 6-colour master palette.
        /// t=0 -> VoidBlack (Pianissimo), t=1 -> WhiteGreenFlash (Sforzando).
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
        /// Works with CipherNocturneBeam, DissonanceOrb, FugueVoices, etc.
        /// </summary>
        public static Color GetBladeGradient(Color[] bladePalette, float progress)
            => PaletteLerp(bladePalette, progress);

        // =================================================================
        //  HSL CYCLING (for shimmer / color oscillation effects)
        // =================================================================

        /// <summary>
        /// Get a hue-shifted Enigma color for shimmer effects.
        /// Cycles through the purple-green hue range (0.28->0.45) for void energy.
        /// </summary>
        public static Color GetShimmer(float time, float sat = 0.85f, float lum = 0.65f)
        {
            float hue = (time * 0.02f) % 1f;
            hue = 0.28f + hue * 0.17f; // Clamp to purple-green void range
            return Main.hslToRgb(hue, sat, lum);
        }

        /// <summary>
        /// Get a hue-shifted mystery color for arcane shimmer effects.
        /// Sweeps through the deep purple -> eerie green hue range (0.75->0.40 wrapping).
        /// </summary>
        public static Color GetMysteryShimmer(float time, float sat = 0.80f, float lum = 0.55f)
        {
            float hue = (time * 0.015f) % 1f;
            // Sweep from purple (0.78) through to green (0.38)
            hue = 0.78f - hue * 0.40f;
            if (hue < 0f) hue += 1f;
            return Main.hslToRgb(hue, sat, lum);
        }
    }
}
