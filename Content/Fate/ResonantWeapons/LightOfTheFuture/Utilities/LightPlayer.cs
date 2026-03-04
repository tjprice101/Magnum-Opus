using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Fate.ResonantWeapons.LightOfTheFuture.Utilities
{
    /// <summary>
    /// Per-weapon ModPlayer tracking shot counter and combo state
    /// for Light of the Future — The Cosmic Railgun.
    /// Every 3rd shot fires 3 homing rockets.
    /// </summary>
    public class LightPlayer : ModPlayer
    {
        /// <summary>Shot counter (1-based, resets to 0 after every 3rd shot triggers rockets).</summary>
        public int ShotCounter;

        /// <summary>Ticks since last shot. Combo resets after 120 ticks of inactivity.</summary>
        public int ComboResetTimer;
        private const int ComboResetDelay = 120;

        /// <summary>Current combo intensity (0..1). Grows with sustained fire, used for VFX scaling.</summary>
        public float ComboIntensity;

        /// <summary>Total shots fired (for escalating visual effects).</summary>
        public int TotalShots;

        /// <summary>Whether the last shot was a rocket volley trigger.</summary>
        public bool JustFiredRockets;

        // === FUTURE SIGHT (Doc mechanic: hold 2+ seconds shows targeting reticle) ===
        /// <summary>Ticks the fire button has been held continuously.</summary>
        public int HoldTimer;

        /// <summary>Whether Future Sight reticle should be displayed (held 2+ seconds).</summary>
        public bool FutureSightActive => HoldTimer >= 120;

        // === CASCADE (Doc mechanic: peak-speed kill spawns 2 smaller bullets) ===
        /// <summary>Number of cascade chain kills in the current chain.</summary>
        public int CascadeChain;

        /// <summary>Cooldown between cascade triggers (prevents infinite chains).</summary>
        public int CascadeCooldown;

        public override void ResetEffects()
        {
            JustFiredRockets = false;
        }

        public override void PostUpdate()
        {
            if (ComboResetTimer > 0)
            {
                ComboResetTimer--;
                if (ComboResetTimer <= 0)
                {
                    ShotCounter = 0;
                    ComboIntensity = 0f;
                }
            }

            // Decay combo intensity slowly
            ComboIntensity *= 0.995f;

            // Future Sight hold timer
            if (Player.channel || Player.itemAnimation > 0)
                HoldTimer++;
            else
                HoldTimer = 0;

            // Cascade cooldown
            if (CascadeCooldown > 0)
                CascadeCooldown--;
            else
                CascadeChain = 0;
        }

        /// <summary>
        /// Called on each shot. Returns true if this is a rocket volley shot (every 3rd).
        /// </summary>
        public bool OnShot()
        {
            ShotCounter++;
            TotalShots++;
            ComboResetTimer = ComboResetDelay;

            // Build intensity with each shot
            ComboIntensity = MathHelper.Clamp(ComboIntensity + 0.2f, 0f, 1f);

            if (ShotCounter >= 3)
            {
                ShotCounter = 0;
                JustFiredRockets = true;
                ComboIntensity = 1f;
                return true;
            }

            return false;
        }
    }

    public static class LightPlayerExtensions
    {
        public static LightPlayer LightOfFuture(this Player player)
            => player.GetModPlayer<LightPlayer>();
    }
}
