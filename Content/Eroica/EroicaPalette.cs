using Microsoft.Xna.Framework;
using System;
using Terraria;

namespace MagnumOpus.Content.Eroica
{
    /// <summary>
    /// Canonical single source-of-truth for ALL Eroica theme colors.
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
    /// </summary>
    public static class EroicaPalette
    {
        // =================================================================
        //  CORE THEME COLORS  (the 5 pillars every Eroica effect uses)
        // =================================================================

        /// <summary>Smoky black undertone — shadows, smoke base, outer glow.</summary>
        public static readonly Color Black = new Color(30, 20, 25);

        /// <summary>Deep, authoritative scarlet — the heartbeat of Eroica.</summary>
        public static readonly Color Scarlet = new Color(200, 50, 50);

        /// <summary>Intense crimson — blood, passion, inner fire.</summary>
        public static readonly Color Crimson = new Color(220, 50, 50);

        /// <summary>Triumphant gold — valor, victory, the climax.</summary>
        public static readonly Color Gold = new Color(255, 215, 0);

        /// <summary>Delicate sakura pink — petals, grace, the softer side of heroism.</summary>
        public static readonly Color Sakura = new Color(255, 150, 180);

        // =================================================================
        //  EXTENDED PALETTE  (specific use-cases across Eroica content)
        // =================================================================

        /// <summary>Dark shadow scarlet for trail ends, outer bloom rings.</summary>
        public static readonly Color DeepScarlet = new Color(139, 0, 0);

        /// <summary>Red-orange flame for mid-swing energy.</summary>
        public static readonly Color Flame = new Color(255, 100, 50);

        /// <summary>Warm orange-gold for projectile cores.</summary>
        public static readonly Color OrangeGold = new Color(255, 180, 60);

        /// <summary>Hot golden white for sforzando/core flare.</summary>
        public static readonly Color HotCore = new Color(255, 240, 200);

        /// <summary>Pale sakura for gentle petal edges.</summary>
        public static readonly Color SakuraPale = new Color(255, 200, 220);

        /// <summary>Warm pollen gold for blossom highlight accents.</summary>
        public static readonly Color PollenGold = new Color(255, 230, 140);

        /// <summary>Dark crimson variant for blade rendering accents.</summary>
        public static readonly Color BladeCrimson = new Color(180, 30, 60);

        // =================================================================
        //  TOOLTIP / UI COLORS
        // =================================================================

        /// <summary>Lore text color for ModifyTooltips.</summary>
        public static readonly Color LoreText = new Color(200, 50, 50);

        /// <summary>Special weapon effect tooltip color.</summary>
        public static readonly Color EffectTooltip = new Color(255, 200, 100);

        // =================================================================
        //  6-COLOR SWING PALETTES  (per-weapon musical scales)
        // =================================================================

        /// <summary>
        /// CelestialValor blade palette — scarlet fire ascending to molten gold.
        /// Heroic, bold, triumphant. Every swing is a fanfare.
        /// </summary>
        public static readonly Color[] CelestialValorBlade = new Color[]
        {
            new Color(80, 15, 15),      // [0] Pianissimo — deep shadow scarlet
            new Color(160, 35, 35),     // [1] Piano — dark crimson
            new Color(200, 50, 50),     // [2] Mezzo — scarlet body
            new Color(220, 100, 40),    // [3] Forte — flame orange
            new Color(255, 180, 60),    // [4] Fortissimo — golden blaze
            new Color(255, 240, 200),   // [5] Sforzando — white-hot gold
        };

        /// <summary>
        /// SakurasBlossom blade palette — bud crimson blooming to pollen gold.
        /// Delicate yet fierce. Every swing is a petal unfurling.
        /// </summary>
        public static readonly Color[] SakurasBlossomBlade = new Color[]
        {
            new Color(100, 20, 35),     // [0] Pianissimo — deep bud crimson
            new Color(180, 50, 70),     // [1] Piano — opening blossom
            new Color(255, 120, 150),   // [2] Mezzo — sakura pink body
            new Color(255, 170, 190),   // [3] Forte — pale petal glow
            new Color(255, 210, 140),   // [4] Fortissimo — golden pollen
            new Color(255, 245, 220),   // [5] Sforzando — white-hot bloom center
        };

        /// <summary>
        /// FuneralPrayer blade palette — grief-darkened scarlet to eulogy gold.
        /// Somber, heavy, final. Every swing is a last rite.
        /// </summary>
        public static readonly Color[] FuneralPrayerBlade = new Color[]
        {
            new Color(40, 8, 12),       // [0] Pianissimo — funeral shadow
            new Color(100, 20, 25),     // [1] Piano — dried blood
            new Color(160, 40, 40),     // [2] Mezzo — mourning scarlet
            new Color(200, 80, 50),     // [3] Forte — ember glow
            new Color(240, 160, 80),    // [4] Fortissimo — requiem gold
            new Color(255, 230, 190),   // [5] Sforzando — spirit white
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
        /// Standard Eroica gradient: Scarlet → Gold over 0→1.
        /// Use for generic theme effects, halos, cascading rings.
        /// </summary>
        public static Color GetGradient(float progress)
            => Color.Lerp(Scarlet, Gold, progress);

        /// <summary>
        /// Sakura gradient: SakuraPink → Gold over 0→1.
        /// Use for petal-themed projectile trails and impacts.
        /// </summary>
        public static Color GetSakuraGradient(float progress)
            => Color.Lerp(Sakura, Gold, progress);

        /// <summary>
        /// Fire gradient: DeepScarlet → Flame → HotCore over 0→1.
        /// Use for infernal / flame-heavy weapon effects.
        /// </summary>
        public static Color GetFireGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(DeepScarlet, Flame, progress * 2f);
            return Color.Lerp(Flame, HotCore, (progress - 0.5f) * 2f);
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
        /// Standard 3-layer PreDrawInWorld bloom for Eroica items.
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
            // Layer 1: Outer dark crimson aura
            sb.Draw(tex, pos, null, Additive(Scarlet, 0.40f), rotation, origin, scale * 1.08f * pulse,
                Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0f);
            // Layer 2: Middle sakura/orange glow
            sb.Draw(tex, pos, null, Additive(Sakura, 0.30f), rotation, origin, scale * 1.04f * pulse,
                Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0f);
            // Layer 3: Inner golden-white core
            sb.Draw(tex, pos, null, Additive(HotCore, 0.22f), rotation, origin, scale * 1.01f * pulse,
                Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0f);
        }

        // =================================================================
        //  HSL CYCLING (for shimmer / color oscillation effects)
        // =================================================================

        /// <summary>
        /// Get a hue-shifted Eroica color for shimmer effects.
        /// Stays within the scarlet→gold hue range (0.0→0.12).
        /// </summary>
        public static Color GetShimmer(float time, float sat = 0.95f, float lum = 0.60f)
        {
            float hue = (time * 0.02f) % 1f;
            hue = hue * 0.12f; // Clamp to scarlet→gold hue range
            return Main.hslToRgb(hue, sat, lum);
        }

        /// <summary>
        /// Get a hue-shifted Sakura color for petal shimmer effects.
        /// Stays within the rose→magenta hue range (0.90→0.98).
        /// </summary>
        public static Color GetSakuraShimmer(float time, float sat = 0.90f, float lum = 0.65f)
        {
            float hue = (time * 0.02f) % 1f;
            hue = 0.90f + hue * 0.08f; // Clamp to sakura hue range
            return Main.hslToRgb(hue, sat, lum);
        }
    }
}
