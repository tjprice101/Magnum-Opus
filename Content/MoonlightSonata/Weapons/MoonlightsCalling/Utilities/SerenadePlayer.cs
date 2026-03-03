using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.MoonlightsCalling.Utilities
{
    /// <summary>
    /// Per-player state tracker for Moonlight's Calling — "The Serenade".
    /// Tracks Serenade Mode cooldown, prismatic charge, resonance building, and harmonic state.
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

        // =================================================================
        // RESONANCE BUILDING SYSTEM
        // =================================================================

        /// <summary>
        /// Current resonance level (0-4). Increases with channel time.
        /// 0 = Pianissimo (thin beam), 1 = Piano (wider + shimmer),
        /// 2 = Mezzo-Forte (spectral beams + standing wave nodes),
        /// 3 = Forte (full beam + orbiting notes + ground glow),
        /// 4 = Fortissimo (maximum power + screen tint).
        /// </summary>
        public int ResonanceLevel;

        /// <summary>How many ticks the player has been channeling.</summary>
        public int ChannelTicks;

        /// <summary>Resonance level names for display.</summary>
        public static readonly string[] ResonanceLevelNames =
            { "Pianissimo", "Piano", "Mezzo-Forte", "Forte", "Fortissimo" };

        /// <summary>Channel tick thresholds for each resonance level.</summary>
        public static readonly int[] ResonanceThresholds = { 0, 36, 72, 108, 144 };

        /// <summary>Beam width multiplier per resonance level.</summary>
        public static readonly float[] ResonanceWidthMultiplier = { 0.6f, 0.8f, 1.0f, 1.2f, 1.5f };

        /// <summary>Beam intensity per resonance level.</summary>
        public static readonly float[] ResonanceIntensity = { 0.5f, 0.7f, 0.85f, 1.0f, 1.2f };

        /// <summary>Colors per resonance level.</summary>
        public static readonly Color[] ResonanceColors =
        {
            new Color(100, 80, 180),    // Pianissimo — dim purple
            new Color(130, 110, 210),   // Piano — brighter violet
            new Color(160, 170, 240),   // Mezzo-Forte — blue-violet
            new Color(200, 210, 255),   // Forte — silver-blue
            new Color(240, 245, 255)    // Fortissimo — brilliant white
        };

        // =================================================================
        // HARMONIC NODES
        // =================================================================

        /// <summary>
        /// Number of harmonic nodes on the beam. Increases with resonance.
        /// Enemies at nodes take 1.5x damage.
        /// </summary>
        public int HarmonicNodeCount => ResonanceLevel >= 2 ? 3 + (ResonanceLevel - 2) * 2 : 0;

        /// <summary>Damage multiplier at harmonic nodes.</summary>
        public const float HarmonicNodeDamageMultiplier = 1.5f;

        /// <summary>
        /// Check if a position along the beam (0-1) is near a harmonic node.
        /// Returns true if within tolerance of a node.
        /// </summary>
        public bool IsAtHarmonicNode(float beamProgress)
        {
            if (HarmonicNodeCount <= 0) return false;

            for (int i = 1; i <= HarmonicNodeCount; i++)
            {
                float nodePos = i / (float)(HarmonicNodeCount + 1);
                if (Math.Abs(beamProgress - nodePos) < 0.04f)
                    return true;
            }
            return false;
        }

        // =================================================================
        // LIFECYCLE
        // =================================================================

        public override void ResetEffects()
        {
            RightClickListener = false;
            MouseWorldListener = false;
        }

        public override void PostUpdate()
        {
            if (SerenadeCooldown > 0)
                SerenadeCooldown--;

            // Update resonance level based on channel ticks
            UpdateResonanceLevel();

            // Decay channel ticks when not actively channeling
            if (!SerenadeActive && ChannelTicks > 0)
            {
                ChannelTicks = Math.Max(0, ChannelTicks - 2);
            }
        }

        private void UpdateResonanceLevel()
        {
            int newLevel = 0;
            for (int i = ResonanceThresholds.Length - 1; i >= 0; i--)
            {
                if (ChannelTicks >= ResonanceThresholds[i])
                {
                    newLevel = i;
                    break;
                }
            }
            ResonanceLevel = newLevel;
        }

        // =================================================================
        // ACTIONS
        // =================================================================

        /// <summary>Start Serenade Mode cooldown (called when Serenade ends).</summary>
        public void StartCooldown()
        {
            SerenadeCooldown = SerenadeCooldownTime;
            SerenadeActive = false;
            ChannelTicks = 0;
        }

        /// <summary>Increment channel ticks (called each frame during channeling).</summary>
        public void TickChannel()
        {
            SerenadeActive = true;
            ChannelTicks++;
        }

        /// <summary>Add prismatic charge from beam bounce events.</summary>
        public void AddCharge(int amount = 1)
        {
            PrismaticCharge = Math.Min(PrismaticCharge + amount, MaxCharge);
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
