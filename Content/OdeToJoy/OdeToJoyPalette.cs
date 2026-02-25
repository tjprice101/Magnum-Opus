using Microsoft.Xna.Framework;
using System;
using Terraria;

namespace MagnumOpus.Content.OdeToJoy
{
    /// <summary>
    /// Canonical single source-of-truth for ALL Ode to Joy theme colors.
    /// Every weapon, projectile, accessory, boss, and VFX helper MUST
    /// reference this file instead of hardcoding Color values inline.
    ///
    /// Palette philosophy (musical dynamics):
    ///   [0] Pianissimo  — deepest shadow
    ///   [1] Piano       — dark body
    ///   [2] Mezzo       — primary readable color
    ///   [3] Forte       — bright accent
    ///   [4] Fortissimo  — near-white highlight
    ///   [5] Sforzando   — white-hot core
    ///
    /// Theme identity: Beethoven's triumphant finale. Blossoming nature,
    /// joyous celebration, golden radiance. Verdant green growth, rose pink
    /// petals, golden pollen, sunlit warmth, pure white bloom.
    /// </summary>
    public static class OdeToJoyPalette
    {
        // =================================================================
        //  CORE THEME COLORS  (the 6 pillars every Ode to Joy effect uses)
        // =================================================================

        /// <summary>Deep forest shadow — vine roots, leaf undersides, dark earth.</summary>
        public static readonly Color DeepForest = new Color(20, 50, 25);

        /// <summary>Verdant green — growth, nature, the heartbeat of Ode to Joy.</summary>
        public static readonly Color VerdantGreen = new Color(76, 175, 80);

        /// <summary>Rose pink — blossoms, beauty, petal unfurling.</summary>
        public static readonly Color RosePink = new Color(255, 182, 193);

        /// <summary>Golden pollen — joy, radiance, the triumphant climax.</summary>
        public static readonly Color GoldenPollen = new Color(255, 215, 0);

        /// <summary>Sunlight yellow — warm radiance, near-white golden warmth.</summary>
        public static readonly Color SunlightYellow = new Color(255, 250, 205);

        /// <summary>White bloom — pure triumph, peak celebration.</summary>
        public static readonly Color WhiteBloom = new Color(255, 255, 255);

        // =================================================================
        //  EXTENDED PALETTE  (specific use-cases across Ode to Joy content)
        // =================================================================

        /// <summary>Leaf green for trail ends, vine accents, forest depth.</summary>
        public static readonly Color LeafGreen = new Color(34, 139, 34);

        /// <summary>Petal pink — hot pink accent for bloom cores, petal edges.</summary>
        public static readonly Color PetalPink = new Color(255, 105, 180);

        /// <summary>Bud green — lighter spring-time green for new growth.</summary>
        public static readonly Color BudGreen = new Color(100, 200, 100);

        /// <summary>Warm amber — sunset warmth for projectile cores.</summary>
        public static readonly Color WarmAmber = new Color(255, 180, 50);

        /// <summary>Pollen gold — bright warm gold for sparkle highlights.</summary>
        public static readonly Color PollenGold = new Color(255, 230, 140);

        /// <summary>Moss shadow — dark green for trailing vine ends.</summary>
        public static readonly Color MossShadow = new Color(40, 80, 40);

        /// <summary>Rose white — pale petal for gentle edges.</summary>
        public static readonly Color RoseWhite = new Color(255, 235, 240);

        // =================================================================
        //  TOOLTIP / UI COLORS
        // =================================================================

        /// <summary>Lore text color for ModifyTooltips.</summary>
        public static readonly Color LoreText = new Color(76, 175, 80);

        /// <summary>Special weapon effect tooltip color.</summary>
        public static readonly Color EffectTooltip = new Color(255, 215, 100);

        // =================================================================
        //  6-COLOR BLADE PALETTES  (per-weapon musical scales)
        // =================================================================

        /// <summary>
        /// Verdant Fury blade palette — deep forest ascending to golden bloom.
        /// Natural, fierce, triumphant. Every swing is a vine unfurling.
        /// </summary>
        public static readonly Color[] VerdantFuryBlade = new Color[]
        {
            new Color(15, 40, 20),      // [0] Pianissimo — deep forest shadow
            new Color(34, 100, 40),     // [1] Piano — dark leaf
            new Color(76, 175, 80),     // [2] Mezzo — verdant green body
            new Color(130, 210, 100),   // [3] Forte — bright spring green
            new Color(200, 235, 120),   // [4] Fortissimo — golden-green blaze
            new Color(255, 250, 205),   // [5] Sforzando — white-hot sunlight
        };

        /// <summary>
        /// Rose Blossom blade palette — bud pink blooming to golden pollen.
        /// Delicate yet fierce. Every swing is a petal storm.
        /// </summary>
        public static readonly Color[] RoseBlossomBlade = new Color[]
        {
            new Color(80, 30, 50),      // [0] Pianissimo — deep bud crimson
            new Color(180, 80, 120),    // [1] Piano — opening blossom
            new Color(255, 150, 180),   // [2] Mezzo — rose pink body
            new Color(255, 182, 193),   // [3] Forte — full bloom pink
            new Color(255, 215, 140),   // [4] Fortissimo — golden pollen
            new Color(255, 250, 230),   // [5] Sforzando — white-hot center
        };

        /// <summary>
        /// Golden Triumph blade palette — warm amber building to radiant white.
        /// Jubilant, ascending, celebratory. Every swing is a victory fanfare.
        /// </summary>
        public static readonly Color[] GoldenTriumphBlade = new Color[]
        {
            new Color(60, 40, 10),      // [0] Pianissimo — dark amber shadow
            new Color(160, 110, 20),    // [1] Piano — warm bronze
            new Color(255, 180, 50),    // [2] Mezzo — golden amber body
            new Color(255, 215, 0),     // [3] Forte — bright gold
            new Color(255, 240, 150),   // [4] Fortissimo — radiant yellow
            new Color(255, 255, 240),   // [5] Sforzando — white-hot radiance
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
        /// Standard Ode to Joy gradient: VerdantGreen → GoldenPollen over 0→1.
        /// Use for generic theme effects, halos, cascading rings.
        /// </summary>
        public static Color GetGradient(float progress)
            => Color.Lerp(VerdantGreen, GoldenPollen, progress);

        /// <summary>
        /// Petal gradient: RosePink → PetalPink → WhiteBloom over 0→1.
        /// Use for petal-themed projectile trails and impacts.
        /// </summary>
        public static Color GetPetalGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(RosePink, PetalPink, progress * 2f);
            return Color.Lerp(PetalPink, WhiteBloom, (progress - 0.5f) * 2f);
        }

        /// <summary>
        /// Garden gradient: DeepForest → VerdantGreen → GoldenPollen → WhiteBloom over 0→1.
        /// Use for full-spectrum nature effects.
        /// </summary>
        public static Color GetGardenGradient(float progress)
        {
            if (progress < 0.33f)
                return Color.Lerp(DeepForest, VerdantGreen, progress * 3f);
            if (progress < 0.66f)
                return Color.Lerp(VerdantGreen, GoldenPollen, (progress - 0.33f) * 3f);
            return Color.Lerp(GoldenPollen, WhiteBloom, (progress - 0.66f) * 3f);
        }

        /// <summary>
        /// Blossom gradient: VerdantGreen → RosePink → GoldenPollen → WhiteBloom.
        /// Cycling gradient for chromatic blossom effects.
        /// </summary>
        public static Color GetBlossomGradient(float progress)
        {
            if (progress < 0.25f)
                return Color.Lerp(VerdantGreen, RosePink, progress * 4f);
            if (progress < 0.5f)
                return Color.Lerp(RosePink, GoldenPollen, (progress - 0.25f) * 4f);
            if (progress < 0.75f)
                return Color.Lerp(GoldenPollen, WhiteBloom, (progress - 0.5f) * 4f);
            return Color.Lerp(WhiteBloom, VerdantGreen, (progress - 0.75f) * 4f);
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
        /// Standard 3-layer PreDrawInWorld bloom for Ode to Joy items.
        /// Call from PreDrawInWorld with additive SpriteBatch.
        /// </summary>
        public static void DrawItemBloom(
            Microsoft.Xna.Framework.Graphics.SpriteBatch sb,
            Microsoft.Xna.Framework.Graphics.Texture2D tex,
            Microsoft.Xna.Framework.Vector2 pos,
            Microsoft.Xna.Framework.Vector2 origin,
            float rotation,
            float scale,
            float pulse)
        {
            // Layer 1: Outer verdant green aura
            sb.Draw(tex, pos, null, Additive(VerdantGreen, 0.40f), rotation, origin, scale * 1.08f * pulse,
                Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0f);
            // Layer 2: Middle rose pink glow
            sb.Draw(tex, pos, null, Additive(RosePink, 0.30f), rotation, origin, scale * 1.04f * pulse,
                Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0f);
            // Layer 3: Inner golden-white core
            sb.Draw(tex, pos, null, Additive(SunlightYellow, 0.22f), rotation, origin, scale * 1.01f * pulse,
                Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0f);
        }

        // =================================================================
        //  6-COLOR MASTER PALETTE INTERPOLATION
        // =================================================================

        /// <summary>
        /// The 6 canonical palette stops as an array for indexed interpolation.
        /// [0] DeepForest → [1] VerdantGreen → [2] RosePink → [3] GoldenPollen → [4] SunlightYellow → [5] WhiteBloom.
        /// </summary>
        private static readonly Color[] MasterPalette =
        {
            DeepForest, VerdantGreen, RosePink, GoldenPollen, SunlightYellow, WhiteBloom
        };

        /// <summary>
        /// Lerp through the 6-colour master palette.
        /// t=0 → DeepForest (Pianissimo), t=1 → WhiteBloom (Sforzando).
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
        /// Works with VerdantFuryBlade, RoseBlossomBlade, GoldenTriumphBlade, etc.
        /// </summary>
        public static Color GetBladeGradient(Color[] bladePalette, float progress)
            => PaletteLerp(bladePalette, progress);

        // =================================================================
        //  HSL CYCLING (for shimmer / color oscillation effects)
        // =================================================================

        /// <summary>
        /// Get a hue-shifted Ode to Joy color for shimmer effects.
        /// Stays within the green→gold hue range (0.22→0.18).
        /// </summary>
        public static Color GetShimmer(float time, float sat = 0.85f, float lum = 0.55f)
        {
            float hue = (time * 0.02f) % 1f;
            hue = 0.22f + hue * 0.14f; // Green→gold hue range
            return Main.hslToRgb(hue, sat, lum);
        }

        /// <summary>
        /// Get a hue-shifted Petal color for petal shimmer effects.
        /// Stays within the rose→magenta hue range (0.90→0.98).
        /// </summary>
        public static Color GetPetalShimmer(float time, float sat = 0.80f, float lum = 0.70f)
        {
            float hue = (time * 0.02f) % 1f;
            hue = 0.90f + hue * 0.08f; // Rose-pink hue range
            return Main.hslToRgb(hue, sat, lum);
        }

        /// <summary>
        /// Get a hue-shifted golden color for pollen shimmer effects.
        /// Stays within the gold→yellow hue range (0.12→0.18).
        /// </summary>
        public static Color GetGoldenShimmer(float time, float sat = 0.90f, float lum = 0.60f)
        {
            float hue = (time * 0.02f) % 1f;
            hue = 0.12f + hue * 0.06f; // Gold-yellow hue range
            return Main.hslToRgb(hue, sat, lum);
        }
    }
}
