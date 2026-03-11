using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

namespace MagnumOpus.Content.SwanLake
{
    /// <summary>
    /// Canonical single source-of-truth for ALL Swan Lake theme colors.
    /// Every weapon, projectile, accessory, minion, enemy, boss, and VFX helper MUST
    /// reference this file instead of hardcoding Color values inline.
    ///
    /// Palette philosophy (dual-polarity ballet — musical dynamics):
    ///   [0] Pianissimo  — obsidian black (the Black Swan, Odile's shadow)
    ///   [1] Piano       — dark silver (twilight over the lake, sorrow gathering)
    ///   [2] Mezzo       — silver (moonlit ripples, the lake's surface)
    ///   [3] Forte       — pure white (the White Swan, Odette's grace)
    ///   [4] Fortissimo  — prismatic shimmer (iridescent light breaking through feathers)
    ///   [5] Sforzando   — rainbow iridescent flash (the tragic climax, all colors at once)
    ///
    /// Swan Lake's unique identity: dual-polarity. Black and White coexist
    /// as equal forces, with rainbow iridescence appearing at the boundary
    /// where they collide — like light refracting through a swan's feathers.
    /// </summary>
    public static class SwanLakePalette
    {
        // =================================================================
        //  CORE THEME COLORS  (the 6 pillars every Swan Lake effect uses)
        // =================================================================

        /// <summary>Obsidian black — the Black Swan, Odile's shadow, the curse made manifest.</summary>
        public static readonly Color ObsidianBlack = new Color(20, 20, 30);

        /// <summary>Dark silver — twilight over the lake, sorrow gathering before the ballet.</summary>
        public static readonly Color DarkSilver = new Color(80, 80, 100);

        /// <summary>Silver — moonlit ripples on the lake's surface, quiet elegance.</summary>
        public static readonly Color Silver = new Color(180, 185, 200);

        /// <summary>Pure white — the White Swan, Odette's grace, dying beauty incarnate.</summary>
        public static readonly Color PureWhite = new Color(240, 240, 250);

        /// <summary>Prismatic shimmer — iridescent light breaking through swan feathers, hope glimpsed.</summary>
        public static readonly Color PrismaticShimmer = new Color(220, 230, 255);

        /// <summary>Rainbow iridescent — the tragic climax, all colors blazing as black and white collide.</summary>
        public static readonly Color RainbowFlash = new Color(255, 255, 255);

        // =================================================================
        //  CONVENIENCE ACCESSORS  (aliases matching existing weapon code)
        // =================================================================

        /// <summary>Standard swan black used across weapon files (alias for ObsidianBlack).</summary>
        public static readonly Color SwanBlack = ObsidianBlack;

        /// <summary>Standard swan white used across weapon files (alias for PureWhite).</summary>
        public static readonly Color SwanWhite = PureWhite;

        /// <summary>Standard swan silver used across weapon files.</summary>
        public static readonly Color SwanSilver = new Color(220, 225, 235);

        /// <summary>Dark gray for mid-range monochrome effects.</summary>
        public static readonly Color SwanDarkGray = new Color(60, 60, 70);

        /// <summary>Mid gray for neutral transition effects.</summary>
        public static readonly Color SwanMidGray = new Color(140, 140, 150);

        /// <summary>Light gray for subtle highlight transitions.</summary>
        public static readonly Color SwanLightGray = new Color(200, 200, 210);

        /// <summary>Icy blue tint for cold, ethereal effects.</summary>
        public static readonly Color IcyBlue = new Color(180, 220, 255);

        // =================================================================
        //  EXTENDED PALETTE  (specific use-cases across Swan Lake content)
        // =================================================================

        /// <summary>Feather white — slightly warm white for feather particle cores.</summary>
        public static readonly Color FeatherWhite = new Color(248, 245, 255);

        /// <summary>Feather black — slightly blue-black for dark feather particles.</summary>
        public static readonly Color FeatherBlack = new Color(15, 15, 25);

        /// <summary>Lake surface — blue-tinted silver for water/lake effects.</summary>
        public static readonly Color LakeSurface = new Color(160, 180, 210);

        /// <summary>Pearlescent — warm pearlescent for iridescent shimmer highlights.</summary>
        public static readonly Color Pearlescent = new Color(240, 230, 245);

        /// <summary>Ballet pink — subtle pink tint appearing in prismatic refractions.</summary>
        public static readonly Color BalletPink = new Color(255, 200, 220);

        /// <summary>Curse violet — dark violet tint for Odile's curse effects.</summary>
        public static readonly Color CurseViolet = new Color(60, 30, 80);

        /// <summary>Monochromatic flash — high-contrast white for boss phase transitions.</summary>
        public static readonly Color MonochromaticFlash = new Color(255, 255, 255);

        /// <summary>Shadow core — deepest black for void centers and boss shadow attacks.</summary>
        public static readonly Color ShadowCore = new Color(8, 8, 15);

        /// <summary>Graceful arc — silver-blue for elegant weapon trail bodies.</summary>
        public static readonly Color GracefulArc = new Color(200, 210, 235);

        /// <summary>Dying beauty — warm white-gold for death/finisher effects.</summary>
        public static readonly Color DyingBeauty = new Color(255, 245, 230);

        // =================================================================
        //  TOOLTIP / UI COLORS
        // =================================================================

        /// <summary>Lore text color for ModifyTooltips.</summary>
        public static readonly Color LoreText = new Color(240, 240, 255);

        /// <summary>Special weapon effect tooltip color — prismatic hint.</summary>
        public static readonly Color EffectTooltip = new Color(180, 220, 255);

        // =================================================================
        //  6-COLOR SWING PALETTES  (per-weapon ballet movements)
        // =================================================================

        /// <summary>
        /// CalloftheBlackSwan palette — darkness to destructive brilliance.
        /// The Black Swan's fury: shadow, sorrow, elegance, then blinding prismatic wrath.
        /// </summary>
        public static readonly Color[] BlackSwanBlade = new Color[]
        {
            new Color(8, 8, 15),        // [0] Pianissimo — shadow core
            new Color(20, 20, 30),      // [1] Piano — obsidian black
            new Color(80, 80, 100),     // [2] Mezzo — dark silver
            new Color(200, 200, 210),   // [3] Forte — light gray
            new Color(240, 240, 250),   // [4] Fortissimo — pure white
            new Color(255, 255, 255),   // [5] Sforzando — monochromatic flash
        };

        /// <summary>
        /// ChromaticSwanSong palette — prismatic magic cascade.
        /// Every color of the spectrum singing together in a dying swan's final aria.
        /// </summary>
        public static readonly Color[] ChromaticSong = new Color[]
        {
            new Color(20, 20, 30),      // [0] Pianissimo — obsidian black
            new Color(160, 180, 210),   // [1] Piano — lake surface
            new Color(220, 225, 235),   // [2] Mezzo — swan silver
            new Color(240, 230, 245),   // [3] Forte — pearlescent
            new Color(220, 230, 255),   // [4] Fortissimo — prismatic shimmer
            new Color(255, 255, 255),   // [5] Sforzando — rainbow flash
        };

        /// <summary>
        /// TheSwansLament palette — sorrowful monochrome to rainbow grief.
        /// Each shot carries the weight of tragedy, black fading to white, then exploding in color.
        /// </summary>
        public static readonly Color[] SwansLament = new Color[]
        {
            new Color(15, 15, 25),      // [0] Pianissimo — feather black
            new Color(60, 60, 70),      // [1] Piano — dark gray
            new Color(140, 140, 150),   // [2] Mezzo — mid gray
            new Color(220, 225, 235),   // [3] Forte — swan silver
            new Color(248, 245, 255),   // [4] Fortissimo — feather white
            new Color(255, 245, 230),   // [5] Sforzando — dying beauty
        };

        /// <summary>
        /// FeatheroftheIridescentFlock palette — summoned prismatic elegance.
        /// Crystal sentinels shimmer with all colors, orbiting in graceful formation.
        /// </summary>
        public static readonly Color[] IridescentFlock = new Color[]
        {
            new Color(80, 80, 100),     // [0] Pianissimo — dark silver
            new Color(180, 185, 200),   // [1] Piano — silver
            new Color(220, 225, 235),   // [2] Mezzo — swan silver
            new Color(240, 230, 245),   // [3] Forte — pearlescent
            new Color(220, 230, 255),   // [4] Fortissimo — prismatic shimmer
            new Color(255, 255, 255),   // [5] Sforzando — rainbow flash
        };

        /// <summary>
        /// IridescentWingspan palette — ethereal wing magic.
        /// Ghostly feathers manifest as cascading prismatic energy from unfurled wings.
        /// </summary>
        public static readonly Color[] Wingspan = new Color[]
        {
            new Color(20, 20, 30),      // [0] Pianissimo — obsidian black
            new Color(60, 30, 80),      // [1] Piano — curse violet
            new Color(180, 185, 200),   // [2] Mezzo — silver
            new Color(240, 240, 250),   // [3] Forte — pure white
            new Color(240, 230, 245),   // [4] Fortissimo — pearlescent
            new Color(220, 230, 255),   // [5] Sforzando — prismatic shimmer
        };

        /// <summary>
        /// CallofthePearlescentLake palette — lake's frozen beauty.
        /// Pearlescent projectiles carry the lake's shimmer, from silver depths to iridescent surface.
        /// </summary>
        public static readonly Color[] PearlescentLake = new Color[]
        {
            new Color(80, 80, 100),     // [0] Pianissimo — dark silver
            new Color(160, 180, 210),   // [1] Piano — lake surface
            new Color(200, 210, 235),   // [2] Mezzo — graceful arc
            new Color(220, 225, 235),   // [3] Forte — swan silver
            new Color(240, 230, 245),   // [4] Fortissimo — pearlescent
            new Color(255, 255, 255),   // [5] Sforzando — rainbow flash
        };

        /// <summary>
        /// CalloftheBlackSwanSwing palette — the blade itself (separate from projectiles).
        /// Monochrome sweep from black to white, the sword's dual nature.
        /// </summary>
        public static readonly Color[] BlackSwanSwing = new Color[]
        {
            new Color(20, 20, 30),      // [0] Pianissimo — obsidian black
            new Color(60, 60, 70),      // [1] Piano — dark gray
            new Color(140, 140, 150),   // [2] Mezzo — mid gray
            new Color(200, 200, 210),   // [3] Forte — light gray
            new Color(220, 225, 235),   // [4] Fortissimo — swan silver
            new Color(240, 240, 250),   // [5] Sforzando — pure white
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
        /// Standard Swan Lake gradient: ObsidianBlack -> Silver -> PureWhite over 0->1.
        /// The classic monochrome gradient used across all Swan Lake content.
        /// </summary>
        public static Color GetGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(ObsidianBlack, Silver, progress * 2f);
            return Color.Lerp(Silver, PureWhite, (progress - 0.5f) * 2f);
        }

        /// <summary>
        /// Dual-polarity gradient: Black -> White with prismatic midpoint.
        /// At progress=0.5, the color is prismatic (cycling rainbow).
        /// Use for effects where black and white collide and create iridescence.
        /// </summary>
        public static Color GetDualPolarityGradient(float progress, float time)
        {
            if (progress < 0.4f)
                return Color.Lerp(ObsidianBlack, Silver, progress / 0.4f);
            if (progress > 0.6f)
                return Color.Lerp(Silver, PureWhite, (progress - 0.6f) / 0.4f);

            // Prismatic midpoint zone (0.4-0.6)
            float prismT = (progress - 0.4f) / 0.2f;
            Color mono = Color.Lerp(Silver, PureWhite, prismT);
            Color rainbow = GetRainbow(time + prismT);
            return Color.Lerp(mono, rainbow, 0.5f);
        }

        /// <summary>
        /// Full Swan Lake gradient: ObsidianBlack -> DarkSilver -> Silver -> PureWhite over 0->1.
        /// Drop-in replacement for any inline GetSwanLakeGradient methods.
        /// </summary>
        public static Color GetSwanLakeGradient(float progress)
        {
            if (progress < 0.33f)
                return Color.Lerp(ObsidianBlack, DarkSilver, progress / 0.33f);
            if (progress < 0.66f)
                return Color.Lerp(DarkSilver, Silver, (progress - 0.33f) / 0.33f);
            return Color.Lerp(Silver, PureWhite, (progress - 0.66f) / 0.34f);
        }

        /// <summary>
        /// Pearlescent revelation gradient: Silver -> PrismaticShimmer -> RainbowFlash over 0->1.
        /// Use for impact explosions, detonation reveals, prismatic bursts.
        /// </summary>
        public static Color GetRevelationGradient(float progress, float time)
        {
            if (progress < 0.5f)
            {
                Color silver = Color.Lerp(Silver, PrismaticShimmer, progress * 2f);
                return Color.Lerp(silver, GetRainbow(time), progress * 0.4f);
            }
            Color shimmer = Color.Lerp(PrismaticShimmer, RainbowFlash, (progress - 0.5f) * 2f);
            return Color.Lerp(shimmer, GetRainbow(time + 0.3f), 0.3f + progress * 0.3f);
        }

        /// <summary>
        /// Black Swan gradient: ShadowCore -> ObsidianBlack -> DarkSilver over 0->1.
        /// Use for dark/shadow effects, Black Swan attacks, curse visuals.
        /// </summary>
        public static Color GetBlackSwanGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(ShadowCore, ObsidianBlack, progress * 2f);
            return Color.Lerp(ObsidianBlack, DarkSilver, (progress - 0.5f) * 2f);
        }

        /// <summary>
        /// White Swan gradient: SwanSilver -> PureWhite -> FeatherWhite over 0->1.
        /// Use for light/grace effects, White Swan attacks, redemption visuals.
        /// </summary>
        public static Color GetWhiteSwanGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(SwanSilver, PureWhite, progress * 2f);
            return Color.Lerp(PureWhite, FeatherWhite, (progress - 0.5f) * 2f);
        }

        /// <summary>
        /// Get a cycling rainbow color for prismatic/iridescent effects.
        /// Swan Lake's signature: light refracting through feathers.
        /// Desaturated pastel — prismatic shimmer, not vivid neon.
        /// </summary>
        public static Color GetRainbow(float offset = 0f)
        {
            float hue = (Main.GlobalTimeWrappedHourly * 0.3f + offset) % 1f;
            Color rainbow = Main.hslToRgb(hue, 0.45f, 0.88f);
            return Color.Lerp(Color.White, rainbow, 0.55f);
        }

        /// <summary>
        /// Get a vivid rainbow color for intense prismatic effects (explosions, finishers).
        /// Still desaturated vs fully vivid — Swan Lake stays elegant.
        /// </summary>
        public static Color GetVividRainbow(float offset = 0f)
        {
            float hue = (Main.GlobalTimeWrappedHourly * 0.3f + offset) % 1f;
            return Main.hslToRgb(hue, 0.65f, 0.82f);
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
        //  6-COLOR MASTER PALETTE INTERPOLATION
        // =================================================================

        /// <summary>
        /// The 6 canonical palette stops as an array for indexed interpolation.
        /// [0] ObsidianBlack -> [1] DarkSilver -> [2] Silver -> [3] PureWhite -> [4] PrismaticShimmer -> [5] RainbowFlash.
        /// </summary>
        private static readonly Color[] MasterPalette =
        {
            ObsidianBlack, DarkSilver, Silver, PureWhite, PrismaticShimmer, RainbowFlash
        };

        /// <summary>
        /// Lerp through the 6-colour master palette.
        /// t=0 -> ObsidianBlack (Pianissimo), t=1 -> RainbowFlash (Sforzando).
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
        /// Palette colour with white push for perceived brilliance.
        /// push=0 returns pure palette, push=1 returns full white.
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
        //  HSL CYCLING (for shimmer / iridescent oscillation effects)
        // =================================================================

        /// <summary>
        /// Get a hue-shifted Swan Lake color for prismatic shimmer effects.
        /// Cycles through the full rainbow spectrum (0.0->1.0) blended with white
        /// for the signature ethereal iridescence.
        /// </summary>
        public static Color GetShimmer(float time, float sat = 0.7f, float lum = 0.85f)
        {
            float hue = (time * 0.02f) % 1f;
            Color rainbow = Main.hslToRgb(hue, sat, lum);
            return Color.Lerp(Color.White, rainbow, 0.5f);
        }

        /// <summary>
        /// Get a monochrome-shifting color for dual-polarity shimmer.
        /// Oscillates between black and white with silver midpoint.
        /// </summary>
        public static Color GetMonochromeShimmer(float time, float intensity = 1f)
        {
            float t = (float)Math.Sin(time * 0.03f) * 0.5f + 0.5f;
            Color mono = Color.Lerp(ObsidianBlack, PureWhite, t);
            return Color.Lerp(Silver, mono, intensity);
        }

        /// <summary>
        /// Get a pearlescent shifting color for elegant shimmer.
        /// Cycles through silver/white/pink/blue — pearl-like opalescence.
        /// </summary>
        public static Color GetPearlescentShimmer(float time, float sat = 0.3f, float lum = 0.9f)
        {
            float hue = (time * 0.015f) % 1f;
            Color pearl = Main.hslToRgb(hue, sat, lum);
            return Color.Lerp(SwanSilver, pearl, 0.4f);
        }
    }
}
