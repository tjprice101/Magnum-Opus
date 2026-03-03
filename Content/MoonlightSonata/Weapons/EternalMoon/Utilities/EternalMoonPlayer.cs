using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.EternalMoon.Utilities
{
    /// <summary>
    /// Tracks per-player state for the Eternal Moon weapon system.
    /// Manages lunar combo phase, tidal phase meter, echoing tides, dash state, and mouse targeting.
    /// </summary>
    public class EternalMoonPlayer : ModPlayer
    {
        /// <summary>Current lunar combo phase: 0=New Moon, 1=Waxing, 2=Half, 3=Waning, 4=Full Moon.</summary>
        public int LunarPhase;

        /// <summary>Timer that resets the combo if the player stops swinging.</summary>
        public int ComboResetTimer;

        /// <summary>Whether the player is currently surging (dash attack).</summary>
        public bool SurgingForward;

        /// <summary>Right-click listener state for alt-fire.</summary>
        public bool RightClickListener;

        /// <summary>Tracked mouse position for targeting.</summary>
        public bool MouseWorldListener;

        /// <summary>Maximum ticks before combo resets.</summary>
        public const int ComboResetTime = 120;

        // === TIDAL PHASE METER ===
        /// <summary>Tidal phase: 0=Low Tide, 1=Flood, 2=High Tide, 3=Tsunami.</summary>
        public int TidalPhase;

        /// <summary>Tidal energy accumulated from swings (0 to MaxTidalEnergy).</summary>
        public float TidalEnergy;

        /// <summary>Maximum tidal energy before reaching Tsunami phase.</summary>
        public const float MaxTidalEnergy = 100f;

        /// <summary>Energy thresholds for each tidal phase.</summary>
        public static readonly float[] TidalThresholds = { 0f, 25f, 55f, 85f };

        /// <summary>Timer for tidal energy decay when not swinging.</summary>
        public int TidalDecayTimer;

        /// <summary>Tidal decay delay in ticks before energy starts draining.</summary>
        public const int TidalDecayDelay = 90;

        /// <summary>Tidal energy decay rate per tick.</summary>
        public const float TidalDecayRate = 0.5f;

        // === ECHOING TIDES ===
        /// <summary>Count of consecutive swings (resets with combo).</summary>
        public int SwingCount;

        /// <summary>Whether the next swing should echo the previous 3 as ghostly replays.</summary>
        public bool ShouldEchoTides => SwingCount > 0 && SwingCount % 4 == 0;

        // === GRAVITATIONAL PULL ===
        /// <summary>Active gravitational pull centers with remaining duration.</summary>
        public Vector2 GravityPullCenter;
        public int GravityPullTimer;
        public const int GravityPullDuration = 60;
        public const float GravityPullRadius = 200f;
        public const float GravityPullStrength = 0.8f;

        /// <summary>Tidal phase names for UI display.</summary>
        public static readonly string[] TidalPhaseNames = { "Low Tide", "Flood", "High Tide", "Tsunami" };

        /// <summary>Tidal phase colors for UI display.</summary>
        public static readonly Color[] TidalPhaseColors =
        {
            new Color(75, 0, 130),     // Low Tide — dark purple
            new Color(138, 43, 226),   // Flood — violet
            new Color(135, 206, 250),  // High Tide — ice blue
            new Color(240, 235, 255),  // Tsunami — moon white
        };

        public override void ResetEffects()
        {
            if (!RightClickListener)
                SurgingForward = false;

            RightClickListener = false;
            MouseWorldListener = false;
        }

        public override void PostUpdate()
        {
            if (ComboResetTimer > 0)
            {
                ComboResetTimer--;
                if (ComboResetTimer <= 0)
                {
                    LunarPhase = 0;
                    SwingCount = 0;
                }
            }

            // Tidal energy decay
            if (TidalDecayTimer > 0)
                TidalDecayTimer--;
            else if (TidalEnergy > 0)
            {
                TidalEnergy = Math.Max(0, TidalEnergy - TidalDecayRate);
                UpdateTidalPhase();
            }

            // Gravitational pull effect on nearby NPCs
            if (GravityPullTimer > 0)
            {
                GravityPullTimer--;
                ApplyGravitationalPull();
            }
        }

        /// <summary>
        /// Advances the lunar combo phase by 1, wrapping at 5 (back to 0 after Full Moon).
        /// Resets the combo timer. Adds tidal energy from the swing.
        /// </summary>
        public void AdvancePhase()
        {
            LunarPhase = (LunarPhase + 1) % 5;
            ComboResetTimer = ComboResetTime;
            SwingCount++;

            // Add tidal energy per swing (more at higher lunar phases)
            float energyGain = 8f + LunarPhase * 3f;
            TidalEnergy = Math.Min(TidalEnergy + energyGain, MaxTidalEnergy);
            TidalDecayTimer = TidalDecayDelay;
            UpdateTidalPhase();
        }

        /// <summary>Updates tidal phase based on current energy level.</summary>
        private void UpdateTidalPhase()
        {
            int newPhase = 0;
            for (int i = TidalThresholds.Length - 1; i >= 0; i--)
            {
                if (TidalEnergy >= TidalThresholds[i])
                {
                    newPhase = i;
                    break;
                }
            }
            TidalPhase = newPhase;
        }

        /// <summary>Starts a gravitational pull effect at the given center.</summary>
        public void StartGravitationalPull(Vector2 center)
        {
            GravityPullCenter = center;
            GravityPullTimer = GravityPullDuration;
        }

        /// <summary>Applies gravitational pull to nearby NPCs.</summary>
        private void ApplyGravitationalPull()
        {
            float pullFactor = GravityPullTimer / (float)GravityPullDuration;
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                float dist = Vector2.Distance(npc.Center, GravityPullCenter);
                if (dist > GravityPullRadius || dist < 10f) continue;

                Vector2 pullDir = (GravityPullCenter - npc.Center).SafeNormalize(Vector2.Zero);
                float strength = GravityPullStrength * pullFactor * (1f - dist / GravityPullRadius);
                npc.velocity += pullDir * strength;
            }
        }

        /// <summary>
        /// Tidal phase multiplier for trail width and VFX intensity (1.0 to 2.0).
        /// </summary>
        public float TidalPhaseMultiplier => 1f + TidalPhase * 0.33f;

        /// <summary>
        /// Returns whether the current phase is Full Moon (final combo step).
        /// </summary>
        public bool IsFullMoon => LunarPhase == 4;

        /// <summary>
        /// Returns whether the current phase spawns ghost reflections (Half Moon).
        /// </summary>
        public bool IsHalfMoon => LunarPhase == 2;

        /// <summary>
        /// Returns whether tidal phase is at Tsunami (max power).
        /// </summary>
        public bool IsTsunami => TidalPhase == 3;
    }

    /// <summary>
    /// Extension methods for accessing EternalMoonPlayer data.
    /// </summary>
    public static class EternalMoonPlayerExtensions
    {
        public static EternalMoonPlayer EternalMoon(this Player player) =>
            player.GetModPlayer<EternalMoonPlayer>();
    }
}
