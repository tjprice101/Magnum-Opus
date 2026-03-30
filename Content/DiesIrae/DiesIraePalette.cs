using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

namespace MagnumOpus.Content.DiesIrae
{
    /// <summary>
    /// Canonical single source-of-truth for ALL Dies Irae theme colors.
    /// Every weapon, projectile, accessory, minion, enemy, and VFX helper MUST
    /// reference this file instead of hardcoding Color values inline.
    ///
    /// Palette philosophy (musical dynamics):
    ///   [0] Pianissimo  — deepest shadow (charcoal black, ash and ruin)
    ///   [1] Piano       — dark body (blood red, dried blood)
    ///   [2] Mezzo       — primary readable color (infernal red, hellfire body)
    ///   [3] Forte       — bright accent (judgment gold, divine wrath)
    ///   [4] Fortissimo  — near-white highlight (bone white, bleached ash)
    ///   [5] Sforzando   — white-hot core (wrath white, day of reckoning)
    /// </summary>
    public static class DiesIraePalette
    {
        // =================================================================
        //  CORE THEME COLORS  (the 6 pillars every Dies Irae effect uses)
        // =================================================================

        /// <summary>Charcoal black — ash, shadow, the void between flames.</summary>
        public static readonly Color CharcoalBlack = new Color(25, 20, 25);

        /// <summary>Blood red — dried blood, crimson depths, the stain of judgment.</summary>
        public static readonly Color BloodRed = new Color(130, 0, 0);

        /// <summary>Infernal red — hellfire body, the primary flame of wrath.</summary>
        public static readonly Color InfernalRed = new Color(200, 30, 30);

        /// <summary>Judgment gold — divine wrath aureate, the light of condemnation.</summary>
        public static readonly Color JudgmentGold = new Color(200, 170, 50);

        /// <summary>Bone white — bleached bone, ash highlights, remnants of the judged.</summary>
        public static readonly Color BoneWhite = new Color(230, 220, 200);

        /// <summary>Wrath white — white-hot core, the sforzando peak of divine fury.</summary>
        public static readonly Color WrathWhite = new Color(255, 250, 240);

        // =================================================================
        //  CONVENIENCE ACCESSORS
        // =================================================================

        /// <summary>Ember orange for trailing fire particles and secondary flames.</summary>
        public static readonly Color EmberOrange = new Color(255, 69, 0);

        /// <summary>Hellfire gold for intense golden flame accents.</summary>
        public static readonly Color HellfireGold = new Color(255, 180, 50);

        // =================================================================
        //  EXTENDED PALETTE  (specific use-cases across Dies Irae content)
        // =================================================================

        /// <summary>Deep crimson for mid-range fire shadows and dark flame edges.</summary>
        public static readonly Color Crimson = new Color(200, 30, 30);

        /// <summary>Charred black — the darkest shadow of spent fire and soot.</summary>
        public static readonly Color CharredBlack = new Color(25, 20, 15);

        /// <summary>Ash gray for crumbling stone debris and cooling embers.</summary>
        public static readonly Color AshGray = new Color(140, 130, 120);

        /// <summary>Doom purple for ecclesiastical dread accents and judgment aura.</summary>
        public static readonly Color DoomPurple = new Color(80, 20, 60);

        /// <summary>Parchment for ancient scroll/rune accents and holy text glow.</summary>
        public static readonly Color Parchment = new Color(210, 195, 160);

        /// <summary>Smoldering ember for slow-burning residual fire effects.</summary>
        public static readonly Color SmolderingEmber = new Color(180, 40, 10);

        /// <summary>Stone gray for crumbling architecture debris and dust.</summary>
        public static readonly Color StoneGray = new Color(100, 95, 90);

        /// <summary>Infernal white — the brightest fire core before pure white.</summary>
        public static readonly Color InfernalWhite = new Color(255, 240, 220);

        /// <summary>Dark blood for deepest blood pool / stain effects.</summary>
        public static readonly Color DarkBlood = new Color(80, 0, 0);

        /// <summary>Wrathful flame — intense orange-red for active wrath state.</summary>
        public static readonly Color WrathfulFlame = new Color(240, 60, 20);

        // =================================================================
        //  TOOLTIP / UI COLORS
        // =================================================================

        /// <summary>Lore text color for ModifyTooltips.</summary>
        public static readonly Color LoreText = new Color(200, 50, 30);

        /// <summary>Special weapon effect tooltip color.</summary>
        public static readonly Color EffectTooltip = new Color(220, 170, 60);

        // =================================================================
        //  6-COLOR SWING PALETTES  (per-weapon musical scales)
        // =================================================================

        /// <summary>
        /// WrathsCleaver blade palette — blood-soaked fury, ascending from shadow to wrath.
        /// Raw, brutal, the executioner's crescendo. Every swing is a sentence.
        /// </summary>
        public static readonly Color[] WrathsCleaverBlade = new Color[]
        {
            new Color(30, 10, 15),      // [0] Pianissimo — dried blood shadow
            new Color(130, 0, 0),       // [1] Piano — blood red body
            new Color(200, 30, 30),     // [2] Mezzo — infernal red edge
            new Color(240, 60, 20),     // [3] Forte — wrathful flame
            new Color(255, 180, 50),    // [4] Fortissimo — hellfire gold
            new Color(255, 245, 230),   // [5] Sforzando — wrath white flash
        };

        /// <summary>
        /// ChainOfJudgment blade palette — shackled fire, judgment links.
        /// Methodical, binding, inescapable. Every strike chains the condemned.
        /// </summary>
        public static readonly Color[] ChainOfJudgmentBlade = new Color[]
        {
            new Color(25, 15, 20),      // [0] Pianissimo — iron shadow
            new Color(100, 10, 10),     // [1] Piano — rusted blood
            new Color(180, 40, 30),     // [2] Mezzo — heated chain
            new Color(200, 170, 50),    // [3] Forte — judgment gold link
            new Color(230, 220, 200),   // [4] Fortissimo — bone white
            new Color(255, 250, 240),   // [5] Sforzando — purifying flash
        };

        /// <summary>
        /// ExecutionersVerdict blade palette — guillotine descent, final judgment.
        /// Absolute, irreversible, the blade falls. Every hit is a verdict.
        /// </summary>
        public static readonly Color[] ExecutionersVerdictBlade = new Color[]
        {
            new Color(20, 10, 15),      // [0] Pianissimo — execution void
            new Color(80, 0, 0),        // [1] Piano — dark blood
            new Color(200, 30, 30),     // [2] Mezzo — infernal judgment
            new Color(255, 69, 0),      // [3] Forte — ember orange flash
            new Color(230, 220, 200),   // [4] Fortissimo — bone verdict
            new Color(255, 250, 240),   // [5] Sforzando — execution white
        };

        /// <summary>
        /// SinCollector projectile palette — sin-seeking bullet, divine trajectory.
        /// Precise, seeking, the rifleman of judgment. Every shot collects a sin.
        /// </summary>
        public static readonly Color[] SinCollectorBeam = new Color[]
        {
            new Color(30, 15, 10),      // [0] Pianissimo — gunsmoke shadow
            new Color(130, 0, 0),       // [1] Piano — blood trail
            new Color(200, 30, 30),     // [2] Mezzo — infernal tracer
            new Color(200, 170, 50),    // [3] Forte — judgment gold
            new Color(255, 180, 50),    // [4] Fortissimo — hellfire gold
            new Color(255, 250, 240),   // [5] Sforzando — flash core
        };

        /// <summary>
        /// DamnationsCannon projectile palette — explosive damnation, volcanic wrath.
        /// Heavy, devastating, apocalyptic. Every shell is damnation incarnate.
        /// </summary>
        public static readonly Color[] DamnationsCannonShell = new Color[]
        {
            new Color(25, 10, 5),       // [0] Pianissimo — blast shadow
            new Color(180, 40, 10),     // [1] Piano — smoldering ember
            new Color(255, 69, 0),      // [2] Mezzo — ember orange body
            new Color(255, 180, 50),    // [3] Forte — hellfire gold
            new Color(230, 220, 200),   // [4] Fortissimo — bone shrapnel
            new Color(255, 245, 220),   // [5] Sforzando — detonation white
        };

        /// <summary>
        /// StaffOfFinalJudgment cast palette — divine condemnation, holy wrath.
        /// Deliberate, absolute, ecclesiastical. Every cast is a final sentence.
        /// </summary>
        public static readonly Color[] StaffOfFinalJudgmentCast = new Color[]
        {
            new Color(25, 20, 25),      // [0] Pianissimo — charcoal void
            new Color(80, 20, 60),      // [1] Piano — doom purple
            new Color(200, 30, 30),     // [2] Mezzo — infernal red
            new Color(200, 170, 50),    // [3] Forte — judgment gold
            new Color(230, 220, 200),   // [4] Fortissimo — bone white
            new Color(255, 250, 240),   // [5] Sforzando — divine white
        };

        /// <summary>
        /// DeathTollingBell summon palette — funeral bell, the toll of doom.
        /// Solemn, ominous, inescapable. Every toll is a death sentence.
        /// </summary>
        public static readonly Color[] DeathTollingBellAura = new Color[]
        {
            new Color(20, 15, 20),      // [0] Pianissimo — funeral shadow
            new Color(100, 0, 0),       // [1] Piano — death red
            new Color(180, 30, 25),     // [2] Mezzo — tolling flame
            new Color(200, 170, 50),    // [3] Forte — judgment gold
            new Color(210, 195, 160),   // [4] Fortissimo — parchment
            new Color(255, 245, 230),   // [5] Sforzando — tolling flash
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
        /// Standard Dies Irae gradient: BloodRed -> JudgmentGold over 0->1.
        /// Use for generic theme effects, halos, wrath rings.
        /// </summary>
        public static Color GetGradient(float progress)
            => Color.Lerp(BloodRed, JudgmentGold, progress);

        /// <summary>
        /// Fire gradient: CharcoalBlack -> InfernalRed -> WrathWhite over 0->1.
        /// Use for hellfire weapon effects and infernal trails.
        /// </summary>
        public static Color GetFireGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(CharcoalBlack, InfernalRed, progress * 2f);
            return Color.Lerp(InfernalRed, WrathWhite, (progress - 0.5f) * 2f);
        }

        /// <summary>
        /// Wrath gradient: BloodRed -> InfernalRed -> HellfireGold over 0->1.
        /// Use for wrath state effects and escalating fury VFX.
        /// </summary>
        public static Color GetWrathGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(BloodRed, InfernalRed, progress * 2f);
            return Color.Lerp(InfernalRed, HellfireGold, (progress - 0.5f) * 2f);
        }

        /// <summary>
        /// Bone gradient: CharcoalBlack -> StoneGray -> BoneWhite over 0->1.
        /// Use for crumbling stone debris and ash effects.
        /// </summary>
        public static Color GetBoneGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(CharcoalBlack, StoneGray, progress * 2f);
            return Color.Lerp(StoneGray, BoneWhite, (progress - 0.5f) * 2f);
        }

        /// <summary>
        /// Judgment gradient: DoomPurple -> JudgmentGold -> WrathWhite over 0->1.
        /// Use for divine judgment beams and ecclesiastical effects.
        /// </summary>
        public static Color GetJudgmentGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(DoomPurple, JudgmentGold, progress * 2f);
            return Color.Lerp(JudgmentGold, WrathWhite, (progress - 0.5f) * 2f);
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
        /// Standard 3-layer PreDrawInWorld bloom for Dies Irae items.
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
            // Layer 1: Outer blood red aura
            sb.Draw(tex, pos, null, Additive(BloodRed, 0.40f), rotation, origin, scale * 1.08f * pulse,
                SpriteEffects.None, 0f);
            // Layer 2: Middle infernal red glow
            sb.Draw(tex, pos, null, Additive(InfernalRed, 0.30f), rotation, origin, scale * 1.04f * pulse,
                SpriteEffects.None, 0f);
            // Layer 3: Inner judgment gold core
            sb.Draw(tex, pos, null, Additive(JudgmentGold, 0.22f), rotation, origin, scale * 1.01f * pulse,
                SpriteEffects.None, 0f);
        }

        // =================================================================
        //  6-COLOR MASTER PALETTE INTERPOLATION
        // =================================================================

        /// <summary>
        /// The 6 canonical palette stops as an array for indexed interpolation.
        /// [0] CharcoalBlack -> [1] BloodRed -> [2] InfernalRed -> [3] JudgmentGold -> [4] BoneWhite -> [5] WrathWhite.
        /// </summary>
        private static readonly Color[] MasterPalette =
        {
            CharcoalBlack, BloodRed, InfernalRed, JudgmentGold, BoneWhite, WrathWhite
        };

        /// <summary>
        /// Sample the Dies Irae gradient LUT texture via VFXLibrary.
        /// t=0 -> left edge (dark), t=1 -> right edge (bright).
        /// </summary>
        public static Color GetPaletteColor(float t)
        {
            return PaletteLerp(MasterPalette, t);
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
        /// Works with WrathsCleaverBlade, ChainOfJudgmentBlade, etc.
        /// </summary>
        public static Color GetBladeGradient(Color[] bladePalette, float progress)
            => PaletteLerp(bladePalette, progress);

        // =================================================================
        //  HSL CYCLING (for shimmer / color oscillation effects)
        // =================================================================

        /// <summary>
        /// Get a hue-shifted Dies Irae color for shimmer effects.
        /// Stays within the red-gold hue range (0.97->0.05 wrapping).
        /// </summary>
        public static Color GetShimmer(float time, float sat = 0.95f, float lum = 0.50f)
        {
            float hue = (time * 0.015f) % 1f;
            hue = hue * 0.08f; // Clamp to blood-red hue range near 0
            return Main.hslToRgb(hue, sat, lum);
        }

        /// <summary>
        /// Get a hue-shifted judgment color for divine shimmer effects.
        /// Sweeps through the gold-amber hue range (0.08->0.14).
        /// </summary>
        public static Color GetJudgmentShimmer(float time, float sat = 0.90f, float lum = 0.55f)
        {
            float hue = (time * 0.012f) % 1f;
            hue = 0.08f + hue * 0.06f; // Clamp to gold-amber hue range
            return Main.hslToRgb(hue, sat, lum);
        }
    }
}
