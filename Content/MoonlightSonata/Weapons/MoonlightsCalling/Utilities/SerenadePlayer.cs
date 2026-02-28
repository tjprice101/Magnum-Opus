using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.MoonlightsCalling.Utilities
{
    /// <summary>
    /// Per-player state tracker for Moonlight's Calling — "The Serenade".
    /// Tracks Serenade Mode cooldown, prismatic charge, and right-click state.
    /// </summary>
    public sealed class SerenadePlayer : ModPlayer
    {
        /// <summary>Remaining Serenade Mode cooldown ticks (3s = 180 ticks).</summary>
        public int SerenadeCooldown;

        /// <summary>Whether the player is currently channeling Serenade Mode.</summary>
        public bool SerenadeActive;

        /// <summary>Set by HoldItem each frame — detects when weapon is held.</summary>
        public bool RightClickListener;

        /// <summary>Set by HoldItem each frame — used for aim direction.</summary>
        public bool MouseWorldListener;

        /// <summary>Prismatic charges — incremented by beam bounces, consumed by spectral events.</summary>
        public int PrismaticCharge;

        /// <summary>Maximum prismatic charges.</summary>
        public const int MaxCharge = 10;

        /// <summary>Serenade Mode cooldown in ticks (3 seconds).</summary>
        public const int SerenadeCooldownTime = 180;

        public override void ResetEffects()
        {
            RightClickListener = false;
            MouseWorldListener = false;
        }

        public override void PostUpdate()
        {
            if (SerenadeCooldown > 0)
                SerenadeCooldown--;
        }

        /// <summary>Start Serenade Mode cooldown (called when Serenade ends).</summary>
        public void StartCooldown()
        {
            SerenadeCooldown = SerenadeCooldownTime;
            SerenadeActive = false;
        }

        /// <summary>Add prismatic charge from beam bounce events.</summary>
        public void AddCharge(int amount = 1)
        {
            PrismaticCharge = System.Math.Min(PrismaticCharge + amount, MaxCharge);
        }

        /// <summary>Whether Serenade Mode is off cooldown.</summary>
        public bool CanSerenade => SerenadeCooldown <= 0;
    }

    public static class SerenadePlayerExtensions
    {
        /// <summary>Shorthand to get the SerenadePlayer for a player.</summary>
        public static SerenadePlayer Serenade(this Player player) => player.GetModPlayer<SerenadePlayer>();
    }
}
