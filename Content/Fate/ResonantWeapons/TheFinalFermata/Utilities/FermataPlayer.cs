using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.Fate.ResonantWeapons.TheFinalFermata.Projectiles;

namespace MagnumOpus.Content.Fate.ResonantWeapons.TheFinalFermata.Utilities
{
    /// <summary>
    /// Per-player Fermata state. Tracks active swords, global sync timer,
    /// and the 90-frame synchronized slash mechanic.
    /// Access via player.Fermata() extension method.
    /// </summary>
    public class FermataModPlayer : ModPlayer
    {
        /// <summary>Current number of active Fermata swords owned by this player.</summary>
        public int ActiveSwordCount;

        /// <summary>Global sync timer — drives the 90-frame synchronized slash.</summary>
        public int SyncTimer;

        /// <summary>Set to true on the frame when a synchronized slash fires.</summary>
        public bool SyncSlashTriggered;

        /// <summary>Total casts performed (for stagger offset of second group).</summary>
        public int TotalCasts;

        // === FERMATA POWER (Doc: +10% damage per second held, max 5x at 5s) ===
        /// <summary>Ticks swords have been held without releasing. Max 300 (5s) for 5x multiplier.</summary>
        public int FermataPowerTimer;

        /// <summary>Fermata Power damage multiplier: 1.0x at 0s, up to 5.0x at 5s.</summary>
        public float FermataPowerMultiplier => 1f + MathHelper.Clamp(FermataPowerTimer / 60f, 0f, 4f);

        // === HARMONIC ALIGNMENT (Doc: 3+ swords in line/triangle = convergent crossfire) ===
        /// <summary>Whether swords are in Harmonic Alignment (3+ in a geometric formation).</summary>
        public bool IsHarmonicallyAligned;

        // === SUSTAINED NOTE (Doc: 10s held → autonomous minion, max 1) ===
        /// <summary>Whether a Sustained Note minion exists.</summary>
        public bool HasSustainedNote;

        /// <summary>Ticks until Sustained Note can be created (600 = 10s).</summary>
        public int SustainedNoteTimer;

        public override void ResetEffects()
        {
            SyncSlashTriggered = false;
            IsHarmonicallyAligned = false;
        }

        public override void PostUpdate()
        {
            // Count active fermata spectral swords
            ActiveSwordCount = Player.ownedProjectileCounts[
                ModContent.ProjectileType<FermataSpectralSwordNew>()];

            // Advance the global sync timer when swords are active
            if (ActiveSwordCount > 0)
            {
                SyncTimer++;
                FermataPowerTimer++;
                SustainedNoteTimer++;

                // Fermata Power caps at 300 ticks (5 seconds)
                if (FermataPowerTimer > 300)
                    FermataPowerTimer = 300;

                if (SyncTimer >= 90)
                {
                    SyncSlashTriggered = true;
                    SyncTimer = 0;
                }

                // Check Harmonic Alignment (3+ swords = aligned)
                if (ActiveSwordCount >= 3)
                    IsHarmonicallyAligned = true;

                // Sustained Note at 10 seconds (600 ticks)
                if (SustainedNoteTimer >= 600 && !HasSustainedNote)
                    HasSustainedNote = true;
            }
            else
            {
                SyncTimer = 0;
                TotalCasts = 0;
                FermataPowerTimer = 0;
                SustainedNoteTimer = 0;
                HasSustainedNote = false;
            }
        }

        /// <summary>Whether this player has the maximum (6) fermata swords active.</summary>
        public bool AtMaxSwords => ActiveSwordCount >= 6;

        /// <summary>Damage multiplier: 1.5x at max swords, 1.0x otherwise.</summary>
        public float DamageMultiplier => AtMaxSwords ? 1.5f : 1.0f;
    }

    /// <summary>
    /// Extension: player.Fermata() for convenient access.
    /// </summary>
    public static class FermataPlayerExtensions
    {
        public static FermataModPlayer Fermata(this Player player)
        {
            return player.GetModPlayer<FermataModPlayer>();
        }
    }
}
