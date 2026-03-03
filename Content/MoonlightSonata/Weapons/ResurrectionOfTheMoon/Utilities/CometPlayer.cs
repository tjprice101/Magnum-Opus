using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.ResurrectionOfTheMoon.Utilities
{
    /// <summary>
    /// Per-player state tracker for Resurrection of the Moon — "The Final Movement".
    /// Tracks active chamber type, reload state, lunar cycle phase, and synergy bonuses.
    /// </summary>
    public sealed class CometPlayer : ModPlayer
    {
        /// <summary>
        /// Current chamber selection.
        /// 0 = Standard (ricocheting), 1 = Comet Core (piercing), 2 = Supernova (artillery).
        /// </summary>
        public int ActiveChamber;

        /// <summary>Number of chamber types.</summary>
        public const int ChamberCount = 3;

        /// <summary>Whether the weapon is currently in reload animation.</summary>
        public bool IsReloading;

        /// <summary>Remaining reload ticks.</summary>
        public int ReloadTimer;

        /// <summary>Total reload time for current shot (used for progress display).</summary>
        public int ReloadTimerMax;

        /// <summary>Accumulated comet charges from ricochets/pierces — used for empowered shots.</summary>
        public int CometCharge;

        /// <summary>Maximum comet charges.</summary>
        public const int MaxCharge = 10;

        /// <summary>Reload times per chamber type (ticks). Standard=45, CometCore=60, Supernova=90.</summary>
        public static readonly int[] ReloadTimes = { 45, 60, 90 };

        /// <summary>Chamber names for tooltip display.</summary>
        public static readonly string[] ChamberNames = { "Standard", "Comet Core", "Supernova" };

        // =================================================================
        // LUNAR CYCLE PHASE SYSTEM
        // =================================================================

        /// <summary>
        /// Current lunar cycle phase. Cycles forward with each shot fired.
        /// 0 = New Moon (dark, piercing), 1 = Waxing (balanced),
        /// 2 = Full Moon (maximum AoE, brilliant), 3 = Waning (homing, spectral).
        /// </summary>
        public int LunarCyclePhase;

        /// <summary>Number of lunar cycle phases.</summary>
        public const int LunarCycleCount = 4;

        /// <summary>Total shots fired — drives the lunar cycle.</summary>
        public int ShotsFired;

        /// <summary>Phase names for display.</summary>
        public static readonly string[] LunarPhaseNames = { "New Moon", "Waxing", "Full Moon", "Waning" };

        /// <summary>Colors per lunar phase for VFX tinting.</summary>
        public static readonly Color[] LunarPhaseColors =
        {
            new Color(40, 20, 80),      // New Moon — dark void purple
            new Color(120, 100, 200),    // Waxing — brightening violet
            new Color(220, 230, 255),    // Full Moon — brilliant white-blue
            new Color(160, 140, 220)     // Waning — fading spectral lavender
        };

        /// <summary>Damage multipliers per lunar phase.</summary>
        public static readonly float[] LunarPhaseDamageMultiplier = { 0.9f, 1.0f, 1.3f, 0.85f };

        /// <summary>AoE radius multipliers per lunar phase.</summary>
        public static readonly float[] LunarPhaseAoEMultiplier = { 0.7f, 1.0f, 1.5f, 0.8f };

        /// <summary>Velocity multipliers per lunar phase.</summary>
        public static readonly float[] LunarPhaseVelocityMultiplier = { 1.15f, 1.0f, 0.85f, 1.0f };

        /// <summary>Whether the current phase is Full Moon (maximum AoE).</summary>
        public bool IsFullMoon => LunarCyclePhase == 2;

        /// <summary>Whether the current phase is New Moon (piercing, dark).</summary>
        public bool IsNewMoon => LunarCyclePhase == 0;

        /// <summary>Whether the current phase is Waning (homing, spectral).</summary>
        public bool IsWaning => LunarCyclePhase == 3;

        /// <summary>Get the current lunar phase color.</summary>
        public Color CurrentLunarColor => LunarPhaseColors[LunarCyclePhase];

        // =================================================================
        // MOONRISE CHARGE SYSTEM
        // =================================================================

        /// <summary>Moonrise charge timer (ticks held). Longer hold = bigger supernova shell.</summary>
        public int MoonriseChargeTimer;

        /// <summary>Maximum moonrise charge (2 seconds at 60fps).</summary>
        public const int MaxMoonriseCharge = 120;

        /// <summary>Whether the player is currently charging a moonrise shot.</summary>
        public bool IsCharging;

        /// <summary>Moonrise charge progress from 0 to 1.</summary>
        public float MoonriseProgress => MoonriseChargeTimer / (float)MaxMoonriseCharge;

        // =================================================================
        // LIFECYCLE
        // =================================================================

        public override void ResetEffects()
        {
        }

        public override void PostUpdate()
        {
            if (ReloadTimer > 0)
            {
                ReloadTimer--;
                if (ReloadTimer <= 0)
                    IsReloading = false;
            }

            // Moonrise charge decay when not actively holding
            if (!IsCharging && MoonriseChargeTimer > 0)
            {
                MoonriseChargeTimer = System.Math.Max(0, MoonriseChargeTimer - 3);
            }
            IsCharging = false; // Reset each frame; weapon sets it if held
        }

        // =================================================================
        // ACTIONS
        // =================================================================

        /// <summary>Cycle to the next chamber type.</summary>
        public void CycleNextChamber()
        {
            ActiveChamber = (ActiveChamber + 1) % ChamberCount;
        }

        /// <summary>Start the reload timer for the current chamber.</summary>
        public void StartReload()
        {
            int reloadTime = ReloadTimes[ActiveChamber];
            ReloadTimer = reloadTime;
            ReloadTimerMax = reloadTime;
            IsReloading = true;
        }

        /// <summary>Advance the lunar cycle after each shot.</summary>
        public void AdvanceLunarCycle()
        {
            ShotsFired++;
            LunarCyclePhase = ShotsFired % LunarCycleCount;
        }

        /// <summary>Whether the weapon can fire (not reloading).</summary>
        public bool CanFire => !IsReloading;

        /// <summary>Reload progress from 0 (just started) to 1 (reload complete).</summary>
        public float ReloadProgress => ReloadTimerMax > 0 ? 1f - (ReloadTimer / (float)ReloadTimerMax) : 1f;

        /// <summary>Add comet charge from ricochets/pierces.</summary>
        public void AddCharge(int amount = 1)
        {
            CometCharge = System.Math.Min(CometCharge + amount, MaxCharge);
        }

        /// <summary>Increment moonrise charge (called during hold).</summary>
        public void ChargeMoonrise()
        {
            IsCharging = true;
            MoonriseChargeTimer = System.Math.Min(MoonriseChargeTimer + 1, MaxMoonriseCharge);
        }
    }

    /// <summary>Extension method for convenient access.</summary>
    public static class CometPlayerExtensions
    {
        public static CometPlayer Resurrection(this Player player)
            => player.GetModPlayer<CometPlayer>();
    }
}
