using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.ResurrectionOfTheMoon.Utilities
{
    /// <summary>
    /// Per-player state tracker for Resurrection of the Moon — "The Final Movement".
    /// Tracks active chamber type, reload state, and synergy bonuses.
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
        }

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

        /// <summary>Whether the weapon can fire (not reloading).</summary>
        public bool CanFire => !IsReloading;

        /// <summary>Reload progress from 0 (just started) to 1 (reload complete).</summary>
        public float ReloadProgress => ReloadTimerMax > 0 ? 1f - (ReloadTimer / (float)ReloadTimerMax) : 1f;

        /// <summary>Add comet charge from ricochets/pierces.</summary>
        public void AddCharge(int amount = 1)
        {
            CometCharge = System.Math.Min(CometCharge + amount, MaxCharge);
        }
    }

    /// <summary>Extension method for convenient access.</summary>
    public static class CometPlayerExtensions
    {
        public static CometPlayer Resurrection(this Player player)
            => player.GetModPlayer<CometPlayer>();
    }
}
