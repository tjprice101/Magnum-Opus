using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Fate.ResonantWeapons.SymphonysEnd
{
    /// <summary>
    /// Per-player state for Symphony's End.
    /// Tracks firing cadence, active blade count, wand crackle intensity,
    /// Crescendo Mode escalation, Diminuendo cooldown, and Final Note charge.
    /// </summary>
    public class SymphonyPlayer : ModPlayer
    {
        /// <summary>Number of SymphonySpiralBlade projectiles currently alive.</summary>
        public int ActiveBladeCount;

        /// <summary>Frames elapsed since last blade was fired.</summary>
        public int FramesSinceLastFire;

        /// <summary>True while the wand is held (enables crackle VFX).</summary>
        public bool IsHoldingWand;

        /// <summary>0-1 intensity that ramps up with rapid fire.</summary>
        public float FireIntensity;

        // === CRESCENDO MODE (Doc: 3s continuous fire = faster + bigger) ===
        /// <summary>Ticks of continuous firing. At 180 (3s), enters Crescendo Mode.</summary>
        public int ContinuousFireTimer;

        /// <summary>Whether Crescendo Mode is active (3+ seconds continuous fire).</summary>
        public bool IsCrescendoMode => ContinuousFireTimer >= 180;

        /// <summary>Fire rate multiplier: 1.0 normal, 0.67 during Crescendo (50% faster).</summary>
        public float FireRateMultiplier => IsCrescendoMode ? 0.67f : 1f;

        /// <summary>Blade scale multiplier: 1.0 normal, 1.5 during Crescendo.</summary>
        public float BladeScaleMultiplier => IsCrescendoMode ? 1.5f : 1f;

        /// <summary>Fragment count: 4 normal, 6 during Crescendo.</summary>
        public int FragmentCount => IsCrescendoMode ? 6 : 4;

        // === DIMINUENDO (Doc: 2s after stopping fire, accuracy decreases, +20% damage) ===
        /// <summary>Diminuendo timer: counts down from 120 (2s) when fire stops.</summary>
        public int DiminuendoTimer;

        /// <summary>Whether Diminuendo is active (stopped firing within 2s).</summary>
        public bool IsDiminuendo => DiminuendoTimer > 0 && FramesSinceLastFire > 5;

        /// <summary>Damage multiplier during Diminuendo: 1.2x.</summary>
        public float DiminuendoDamageMultiplier => IsDiminuendo ? 1.2f : 1f;

        // === FINAL NOTE (Doc: exactly 10s continuous = giant 5x blade) ===
        /// <summary>Whether Final Note has been triggered (10s continuous fire).</summary>
        public bool FinalNoteReady => ContinuousFireTimer >= 600;

        /// <summary>Whether Final Note was already fired this cycle.</summary>
        public bool FinalNoteFired;

        public override void ResetEffects()
        {
            IsHoldingWand = false;
            FramesSinceLastFire++;

            // Decay fire intensity when not actively firing
            if (FramesSinceLastFire > 15)
            {
                FireIntensity = MathHelper.Lerp(FireIntensity, 0f, 0.05f);

                // Start Diminuendo when fire stops after building up
                if (ContinuousFireTimer > 30 && DiminuendoTimer <= 0)
                    DiminuendoTimer = 120; // 2 seconds

                ContinuousFireTimer = 0;
                FinalNoteFired = false;
            }

            // Diminuendo decay
            if (DiminuendoTimer > 0)
                DiminuendoTimer--;
        }

        /// <summary>Called by the item each time a blade is fired.</summary>
        public void OnFire()
        {
            FramesSinceLastFire = 0;
            ContinuousFireTimer++;
            FireIntensity = MathHelper.Clamp(FireIntensity + 0.15f, 0f, 1f);
            DiminuendoTimer = 0; // Cancel Diminuendo when actively firing
        }
    }

    /// <summary>
    /// Extension: <c>player.Symphony()</c> returns the <see cref="SymphonyPlayer"/> instance.
    /// </summary>
    public static class SymphonyPlayerExtensions
    {
        public static SymphonyPlayer Symphony(this Player player)
            => player.GetModPlayer<SymphonyPlayer>();
    }
}
