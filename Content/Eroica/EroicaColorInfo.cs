using Microsoft.Xna.Framework;
using System;

namespace MagnumOpus.Content.Eroica
{
    /// <summary>
    /// Eroica color information and per-dust configuration.
    /// Follows the SLPColorInfo pattern — provides dust color palettes,
    /// gradient speeds, and per-weapon color type enums for clean VFX code.
    /// </summary>
    public static class EroicaColorInfo
    {
        // ═══════════════════════════════════════════════════════
        //  DUST COLOR TYPES — used by Eroica custom dusts
        // ═══════════════════════════════════════════════════════

        public enum EroicaDustColorType
        {
            /// <summary>Scarlet → Gold heroic fire gradient.</summary>
            HeroicFlame,
            /// <summary>Sakura pink → Pollen gold blossom gradient.</summary>
            SakuraBlossom,
            /// <summary>Deep scarlet → Crimson funeral pyre gradient.</summary>
            FuneralPyre,
            /// <summary>Gold → Hot white-gold triumphant gradient.</summary>
            TriumphantGold,
            /// <summary>Pure white-hot core for intense moments.</summary>
            WhiteHotCore,
            /// <summary>Crimson → Sakura romantic warrior gradient.</summary>
            ValorRomance,
        }

        // ═══════════════════════════════════════════════════════
        //  COLOR RESOLVERS
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// Get a color from an Eroica dust color type at a given progress (0–1).
        /// </summary>
        public static Color GetDustColor(EroicaDustColorType colorType, float progress)
        {
            progress = MathHelper.Clamp(progress, 0f, 1f);
            return colorType switch
            {
                EroicaDustColorType.HeroicFlame => Color.Lerp(EroicaPalette.Scarlet, EroicaPalette.Gold, progress),
                EroicaDustColorType.SakuraBlossom => Color.Lerp(EroicaPalette.Sakura, EroicaPalette.PollenGold, progress),
                EroicaDustColorType.FuneralPyre => Color.Lerp(EroicaPalette.DeepScarlet, EroicaPalette.BladeCrimson, progress),
                EroicaDustColorType.TriumphantGold => Color.Lerp(EroicaPalette.Gold, EroicaPalette.HotCore, progress),
                EroicaDustColorType.WhiteHotCore => Color.Lerp(EroicaPalette.OrangeGold, EroicaPalette.HotCore, progress),
                EroicaDustColorType.ValorRomance => Color.Lerp(EroicaPalette.BladeCrimson, EroicaPalette.Sakura, progress),
                _ => EroicaPalette.Gold,
            };
        }

        /// <summary>
        /// Get a random color within a dust type's gradient range.
        /// </summary>
        public static Color GetRandomDustColor(EroicaDustColorType colorType)
        {
            return GetDustColor(colorType, Main.rand.NextFloat());
        }

        // ═══════════════════════════════════════════════════════
        //  GRADIENT SPEEDS — how fast colors cycle
        // ═══════════════════════════════════════════════════════

        /// <summary>Base gradient cycling speed for ambient effects (slow shimmer).</summary>
        public const float AmbientGradientSpeed = 0.015f;

        /// <summary>Combat gradient speed for active weapon effects.</summary>
        public const float CombatGradientSpeed = 0.04f;

        /// <summary>Impact gradient speed for hit/explosion effects.</summary>
        public const float ImpactGradientSpeed = 0.08f;

        /// <summary>Finisher gradient speed for climactic combo enders.</summary>
        public const float FinisherGradientSpeed = 0.12f;

        // ═══════════════════════════════════════════════════════
        //  PER-WEAPON ACCENT COLORS
        // ═══════════════════════════════════════════════════════

        // Celestial Valor — Heroic flame sword
        public static readonly Color ValorCoreFlame = new Color(255, 120, 40);
        public static readonly Color ValorAuraGlow = new Color(200, 80, 30);
        public static readonly Color ValorImpactFlash = new Color(255, 200, 120);
        public static readonly Color ValorTrailEdge = new Color(180, 40, 40);

        // Sakura's Blossom — Petal storm blade
        public static readonly Color BlossomCoreGlow = new Color(255, 160, 190);
        public static readonly Color BlossomPetalDrift = new Color(255, 200, 210);
        public static readonly Color BlossomImpactBurst = new Color(255, 180, 140);

        // Funeral Prayer — Somber pyre staff
        public static readonly Color FuneralEmberGlow = new Color(180, 50, 30);
        public static readonly Color FuneralSmokeWisp = new Color(80, 30, 40);
        public static readonly Color FuneralPrayerLight = new Color(240, 160, 80);

        // Triumphant Fractal — Geometric golden staff
        public static readonly Color FractalCoreGold = new Color(255, 220, 80);
        public static readonly Color FractalGeometryEdge = new Color(200, 160, 40);
        public static readonly Color FractalBurstWhite = new Color(255, 250, 230);

        // Blossom of the Sakura — Assault rifle
        public static readonly Color BarrelHeatGlow = new Color(255, 100, 60);
        public static readonly Color MuzzleFlashCore = new Color(255, 240, 200);
        public static readonly Color TracerTrail = new Color(255, 150, 100);

        // Piercing Light — Precision rifle
        public static readonly Color ChargeOrbitGlow = new Color(255, 180, 60);
        public static readonly Color SuperShotCore = new Color(255, 255, 220);
        public static readonly Color SuperShotTrail = new Color(255, 200, 80);

        // Finality of the Sakura — Summoner staff
        public static readonly Color SummonCircleGlow = new Color(200, 50, 80);
        public static readonly Color MinionFlameCore = new Color(180, 30, 50);
        public static readonly Color MinionTrailEdge = new Color(255, 120, 160);
    }
}
