using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

namespace MagnumOpus.Content.LaCampanella
{
    /// <summary>
    /// Canonical single source-of-truth for ALL La Campanella theme colors.
    /// Every weapon, projectile, accessory, minion, enemy, and VFX helper MUST
    /// reference this file instead of hardcoding Color values inline.
    ///
    /// Palette philosophy (musical dynamics):
    ///   [0] Pianissimo  — deepest shadow (soot black, smoke base)
    ///   [1] Piano       — dark body (deep ember)
    ///   [2] Mezzo       — primary readable color (infernal orange)
    ///   [3] Forte       — bright accent (flame yellow)
    ///   [4] Fortissimo  — near-white highlight (bell gold)
    ///   [5] Sforzando   — white-hot core (bell ring flash)
    /// </summary>
    public static class LaCampanellaPalette
    {
        // =================================================================
        //  CORE THEME COLORS  (the 6 pillars every La Campanella effect uses)
        // =================================================================

        /// <summary>Soot black — smoke, ash, deepest shadow of the furnace.</summary>
        public static readonly Color SootBlack = new Color(20, 15, 20);

        /// <summary>Deep ember — the smoldering coals beneath the bell.</summary>
        public static readonly Color DeepEmber = new Color(180, 60, 0);

        /// <summary>Infernal orange — the heartbeat of La Campanella's fire.</summary>
        public static readonly Color InfernalOrange = new Color(255, 100, 0);

        /// <summary>Flame yellow — bright ascending flame accent.</summary>
        public static readonly Color FlameYellow = new Color(255, 200, 50);

        /// <summary>Bell gold — the resonant warmth of the bell's body.</summary>
        public static readonly Color BellGold = new Color(218, 165, 32);

        /// <summary>White-hot — the sforzando peak, bell ring flash.</summary>
        public static readonly Color WhiteHot = new Color(255, 240, 200);

        // =================================================================
        //  CONVENIENCE ACCESSORS
        // =================================================================

        /// <summary>Warm bronze for bell metal accents and contrast dust.</summary>
        public static readonly Color BellBronze = new Color(180, 140, 80);

        /// <summary>Bright chime shimmer for bell ring highlight effects.</summary>
        public static readonly Color ChimeShimmer = new Color(255, 240, 180);

        // =================================================================
        //  EXTENDED PALETTE  (specific use-cases across La Campanella content)
        // =================================================================

        /// <summary>Heavy smoke gray for billowing smoke effects.</summary>
        public static readonly Color SmokeGray = new Color(80, 75, 70);

        /// <summary>Ember red for glowing ember particles and smoldering edges.</summary>
        public static readonly Color EmberRed = new Color(200, 50, 20);

        /// <summary>Molten core for inner flame centers and projectile cores.</summary>
        public static readonly Color MoltenCore = new Color(255, 180, 50);

        /// <summary>Ash gray for post-impact debris and cooling effects.</summary>
        public static readonly Color AshGray = new Color(120, 110, 100);

        /// <summary>Infernal red for deep fire shadows and dark flame edges.</summary>
        public static readonly Color InfernalRed = new Color(200, 40, 30);

        /// <summary>Bright chime white for bell ring peak flash.</summary>
        public static readonly Color BellChime = new Color(255, 255, 220);

        /// <summary>Dark smoke for shadowy fire edges and smoky aura.</summary>
        public static readonly Color DarkSmoke = new Color(50, 40, 35);

        /// <summary>Bright flame for fast-moving fire trail cores.</summary>
        public static readonly Color BrightFlame = new Color(255, 150, 30);

        /// <summary>Warm amber for mid-range fire glow effects.</summary>
        public static readonly Color WarmAmber = new Color(255, 170, 60);

        /// <summary>Forge heat for weapon forge and crafting VFX.</summary>
        public static readonly Color ForgeHeat = new Color(240, 120, 20);

        // =================================================================
        //  TOOLTIP / UI COLORS
        // =================================================================

        /// <summary>Lore text color for ModifyTooltips.</summary>
        public static readonly Color LoreText = new Color(255, 140, 40);

        /// <summary>Special weapon effect tooltip color.</summary>
        public static readonly Color EffectTooltip = new Color(255, 200, 80);

        // =================================================================
        //  6-COLOR SWING PALETTES  (per-weapon musical scales)
        // =================================================================

        /// <summary>
        /// DualFatedChime blade palette — infernal waltz, fire dance ascending to white-hot.
        /// Passionate, whirling, the virtuoso's crescendo. Every swing is a flame dance.
        /// </summary>
        public static readonly Color[] DualFatedChimeBlade = new Color[]
        {
            new Color(30, 15, 10),      // [0] Pianissimo — charred shadow
            new Color(180, 40, 10),     // [1] Piano — deep infernal red
            new Color(255, 100, 0),     // [2] Mezzo — infernal orange body
            new Color(255, 180, 30),    // [3] Forte — bright flame
            new Color(255, 220, 80),    // [4] Fortissimo — golden fire
            new Color(255, 245, 200),   // [5] Sforzando — white-hot flash
        };

        /// <summary>
        /// FangOfTheInfiniteBell blade palette — bronze fang through bell gold.
        /// Sharp, metallic, resonant. Every strike rings like a bell.
        /// </summary>
        public static readonly Color[] FangBlade = new Color[]
        {
            new Color(40, 30, 15),      // [0] Pianissimo — tarnished shadow
            new Color(140, 100, 40),    // [1] Piano — dark bronze
            new Color(180, 140, 80),    // [2] Mezzo — bell bronze body
            new Color(218, 165, 32),    // [3] Forte — bell gold
            new Color(255, 220, 100),   // [4] Fortissimo — bright bell shimmer
            new Color(255, 245, 210),   // [5] Sforzando — bell ring white
        };

        /// <summary>
        /// IgnitionOfTheBell cast palette — ember ignition through infernal eruption.
        /// Explosive, volcanic, the fire ignites. Every cast is a furnace blast.
        /// </summary>
        public static readonly Color[] IgnitionCast = new Color[]
        {
            new Color(60, 20, 5),       // [0] Pianissimo — deep ember void
            new Color(200, 50, 20),     // [1] Piano — ember red
            new Color(255, 100, 0),     // [2] Mezzo — ignition orange
            new Color(255, 200, 50),    // [3] Forte — flame yellow
            new Color(255, 240, 120),   // [4] Fortissimo — bright ignition
            new Color(255, 250, 220),   // [5] Sforzando — white-hot ignition
        };

        /// <summary>
        /// InfernalBellMinion palette — smoky minion with fire spirit core.
        /// Smoldering, spectral, always burning. Every action trails fire.
        /// </summary>
        public static readonly Color[] InfernalMinionAura = new Color[]
        {
            new Color(25, 15, 10),      // [0] Pianissimo — smoke shadow
            new Color(120, 50, 10),     // [1] Piano — smoldering ember
            new Color(200, 80, 10),     // [2] Mezzo — fire spirit orange
            new Color(255, 150, 30),    // [3] Forte — bright flame core
            new Color(255, 200, 80),    // [4] Fortissimo — golden fire
            new Color(255, 240, 180),   // [5] Sforzando — spirit white
        };

        /// <summary>
        /// InfernalChimesCalling projectile palette — chiming fire projectiles.
        /// Resonant, ringing, fire-borne bells. Every shot chimes.
        /// </summary>
        public static readonly Color[] InfernalChimesBeam = new Color[]
        {
            new Color(40, 20, 5),       // [0] Pianissimo — dark smoke
            new Color(180, 60, 0),      // [1] Piano — deep ember
            new Color(255, 120, 20),    // [2] Mezzo — chiming orange
            new Color(218, 165, 32),    // [3] Forte — bell gold
            new Color(255, 240, 180),   // [4] Fortissimo — chime shimmer
            new Color(255, 255, 230),   // [5] Sforzando — bell ring white
        };

        /// <summary>
        /// LaCampanellaRanger projectile palette — ember comet to gold trail.
        /// Fast, fiery, streaking. Every shot is a fire comet.
        /// </summary>
        public static readonly Color[] RangerComet = new Color[]
        {
            new Color(50, 20, 10),      // [0] Pianissimo — smoky shadow
            new Color(200, 50, 20),     // [1] Piano — ember red trail
            new Color(255, 100, 0),     // [2] Mezzo — fire comet body
            new Color(255, 180, 50),    // [3] Forte — molten core
            new Color(255, 215, 100),   // [4] Fortissimo — golden comet
            new Color(255, 245, 210),   // [5] Sforzando — white-hot core
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
        /// Standard La Campanella gradient: DeepEmber -> BellGold over 0->1.
        /// Use for generic theme effects, halos, cascading rings.
        /// </summary>
        public static Color GetGradient(float progress)
            => Color.Lerp(DeepEmber, BellGold, progress);

        /// <summary>
        /// Fire gradient: SootBlack -> InfernalOrange -> WhiteHot over 0->1.
        /// Use for flame-heavy weapon effects and infernal trails.
        /// </summary>
        public static Color GetFireGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(SootBlack, InfernalOrange, progress * 2f);
            return Color.Lerp(InfernalOrange, WhiteHot, (progress - 0.5f) * 2f);
        }

        /// <summary>
        /// Bell gradient: BellBronze -> BellGold -> BellChime over 0->1.
        /// Use for bell motifs, chime effects, metallic accents.
        /// </summary>
        public static Color GetBellGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(BellBronze, BellGold, progress * 2f);
            return Color.Lerp(BellGold, BellChime, (progress - 0.5f) * 2f);
        }

        /// <summary>
        /// Smoke gradient: SootBlack -> SmokeGray -> AshGray over 0->1.
        /// Use for heavy smoke billow effects and ash debris.
        /// </summary>
        public static Color GetSmokeGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(SootBlack, SmokeGray, progress * 2f);
            return Color.Lerp(SmokeGray, AshGray, (progress - 0.5f) * 2f);
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
        /// Standard 3-layer PreDrawInWorld bloom for La Campanella items.
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
            // Layer 1: Outer deep ember aura
            sb.Draw(tex, pos, null, Additive(DeepEmber, 0.40f), rotation, origin, scale * 1.08f * pulse,
                SpriteEffects.None, 0f);
            // Layer 2: Middle infernal orange glow
            sb.Draw(tex, pos, null, Additive(InfernalOrange, 0.30f), rotation, origin, scale * 1.04f * pulse,
                SpriteEffects.None, 0f);
            // Layer 3: Inner bell gold core
            sb.Draw(tex, pos, null, Additive(BellGold, 0.22f), rotation, origin, scale * 1.01f * pulse,
                SpriteEffects.None, 0f);
        }

        // =================================================================
        //  6-COLOR MASTER PALETTE INTERPOLATION
        // =================================================================

        /// <summary>
        /// The 6 canonical palette stops as an array for indexed interpolation.
        /// [0] SootBlack -> [1] DeepEmber -> [2] InfernalOrange -> [3] FlameYellow -> [4] BellGold -> [5] WhiteHot.
        /// </summary>
        private static readonly Color[] MasterPalette =
        {
            SootBlack, DeepEmber, InfernalOrange, FlameYellow, BellGold, WhiteHot
        };

        /// <summary>
        /// Lerp through the 6-colour master palette.
        /// t=0 -> SootBlack (Pianissimo), t=1 -> WhiteHot (Sforzando).
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
        /// Works with DualFatedChimeBlade, FangBlade, IgnitionCast, etc.
        /// </summary>
        public static Color GetBladeGradient(Color[] bladePalette, float progress)
            => PaletteLerp(bladePalette, progress);

        // =================================================================
        //  HSL CYCLING (for shimmer / color oscillation effects)
        // =================================================================

        /// <summary>
        /// Get a hue-shifted La Campanella color for shimmer effects.
        /// Stays within the orange-gold hue range (0.05->0.13).
        /// </summary>
        public static Color GetShimmer(float time, float sat = 0.95f, float lum = 0.60f)
        {
            float hue = (time * 0.02f) % 1f;
            hue = 0.05f + hue * 0.08f; // Clamp to orange-gold hue range
            return Main.hslToRgb(hue, sat, lum);
        }

        /// <summary>
        /// Get a hue-shifted bell chime color for metallic shimmer effects.
        /// Sweeps through the gold->amber hue range (0.10->0.16).
        /// </summary>
        public static Color GetBellShimmer(float time, float sat = 0.85f, float lum = 0.65f)
        {
            float hue = (time * 0.015f) % 1f;
            hue = 0.10f + hue * 0.06f; // Clamp to gold-amber hue range
            return Main.hslToRgb(hue, sat, lum);
        }
    }
}
