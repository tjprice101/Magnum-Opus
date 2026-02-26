using Microsoft.Xna.Framework;

namespace MagnumOpus.Content.Eroica.Weapons.SakurasBlossom
{
    /// <summary>
    /// Self-contained color configuration for Sakura's Blossom.
    /// Follows SLPColorInfo pattern — every SakurasBlossom VFX, dust, shader, and
    /// projectile reads colors from HERE, not from EroicaVFXLibrary or hardcoded values.
    /// </summary>
    public static class SBColorInfo
    {
        // ═══════════════════════════════════════════════════════
        //  MASTER BLADE GRADIENT (canonical, from EroicaPalette)
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// 6-stop blade palette: bud crimson → full bloom → pollen gold → white-hot.
        /// [0] Pianissimo  (100, 20, 35)   deep bud crimson
        /// [1] Piano       (180, 50, 70)   opening blossom
        /// [2] Mezzo       (255, 120, 150) sakura pink body
        /// [3] Forte       (255, 170, 190) pale petal glow
        /// [4] Fortissimo  (255, 210, 140) golden pollen
        /// [5] Sforzando   (255, 245, 220) white-hot bloom center
        /// </summary>
        public static readonly Color[] BladeGradient = EroicaPalette.SakurasBlossomBlade;

        // ═══════════════════════════════════════════════════════
        //  PHASE-IDENTITY ACCENT COLORS
        // ═══════════════════════════════════════════════════════

        /// <summary>Phase 0 — Petal Slash: fresh, bright petal pink.</summary>
        public static readonly Color Phase0_PetalSlash = new Color(255, 160, 185);

        /// <summary>Phase 1 — Crimson Scatter: deep crimson-rose authority.</summary>
        public static readonly Color Phase1_CrimsonScatter = new Color(200, 60, 85);

        /// <summary>Phase 2 — Blossom Bloom: warm golden pollen burst.</summary>
        public static readonly Color Phase2_BlossomBloom = new Color(255, 210, 140);

        /// <summary>Phase 3 — Storm of Petals: white-hot bloom crescendo.</summary>
        public static readonly Color Phase3_StormOfPetals = new Color(255, 245, 220);

        // ═══════════════════════════════════════════════════════
        //  WEAPON-SPECIFIC IDENTITY COLORS
        // ═══════════════════════════════════════════════════════

        /// <summary>Unopened bud — deepest shadow undertone for trail ends.</summary>
        public static readonly Color BudCrimson = new Color(120, 25, 45);

        /// <summary>Full bloom — primary readable sakura pink.</summary>
        public static readonly Color BloomPink = new Color(255, 130, 165);

        /// <summary>Golden pollen motes — delegates to canonical EroicaPalette.PollenGold.</summary>
        public static readonly Color PollenGold = EroicaPalette.PollenGold;

        /// <summary>White-hot petal center — sforzando flare.</summary>
        public static readonly Color PetalWhite = new Color(255, 240, 235);

        /// <summary>New leaf accent — spring foliage undertone.</summary>
        public static readonly Color SpringGreen = new Color(140, 200, 120);

        /// <summary>Deep blossom heart — inner core of each petal.</summary>
        public static readonly Color BlossomCore = new Color(220, 80, 110);

        /// <summary>Floating petal edge — soft drifting petal trail color.</summary>
        public static readonly Color PetalDrifter = new Color(255, 180, 200);

        /// <summary>Sun-touched petal tip — warm highlight where light kisses blossom.</summary>
        public static readonly Color SunlitPetal = new Color(255, 220, 180);

        // ═══════════════════════════════════════════════════════
        //  PHASE-INDEXED GETTERS
        // ═══════════════════════════════════════════════════════

        /// <summary>Returns the accent color for the given combo phase.</summary>
        public static Color GetPhaseAccent(int comboStep) => comboStep switch
        {
            0 => Phase0_PetalSlash,
            1 => Phase1_CrimsonScatter,
            2 => Phase2_BlossomBloom,
            3 => Phase3_StormOfPetals,
            _ => BloomPink
        };

        /// <summary>
        /// Trail shader color pairs per phase.
        /// Primary = trail head color, Secondary = trail tail color.
        /// </summary>
        public static (Color primary, Color secondary) GetTrailColors(int comboStep) => comboStep switch
        {
            0 => (new Color(255, 150, 180), new Color(255, 210, 170)),
            1 => (new Color(200, 50, 70), new Color(255, 130, 165)),
            2 => (new Color(255, 170, 190), new Color(255, 230, 140)),
            3 => (new Color(255, 120, 150), new Color(255, 245, 220)),
            _ => (BloomPink, PollenGold)
        };

        // ═══════════════════════════════════════════════════════
        //  GRADIENT HELPERS
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// Interpolate through the 6-stop blade gradient. t=0 → BudCrimson, t=1 → WhiteHot.
        /// </summary>
        public static Color GetBlossomGradient(float t)
            => EroicaPalette.PaletteLerp(BladeGradient, t);

        /// <summary>Sakura gradient: BudCrimson → BloomPink → PollenGold.</summary>
        public static Color GetSakuraGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(BudCrimson, BloomPink, progress * 2f);
            return Color.Lerp(BloomPink, PollenGold, (progress - 0.5f) * 2f);
        }

        /// <summary>Petal gradient: BloomPink → PetalWhite for gentle fading.</summary>
        public static Color GetPetalFadeGradient(float progress)
            => Color.Lerp(BloomPink, PetalWhite, progress);

        // ═══════════════════════════════════════════════════════
        //  ADDITIVE/BLOOM HELPERS
        // ═══════════════════════════════════════════════════════

        /// <summary>Returns color with A=0 for additive-compatible bloom stacking.</summary>
        public static Color Additive(Color c) => c with { A = 0 };

        /// <summary>Returns color with A=0 and opacity multiplier.</summary>
        public static Color Additive(Color c, float opacity) => (c with { A = 0 }) * opacity;

        /// <summary>
        /// Returns a random dust color from the sakura palette for visual variety.
        /// </summary>
        public static Color RandomDustColor()
        {
            return Terraria.Main.rand.Next(4) switch
            {
                0 => BloomPink,
                1 => PollenGold,
                2 => PetalWhite,
                _ => SunlitPetal
            };
        }
    }
}
