using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.MidnightMechanism.Utilities
{
    /// <summary>
    /// Tracks Midnight Mechanism's 5-phase spin-up, tick marks, jam state, and Midnight Strike.
    /// </summary>
    public class MidnightMechanismPlayer : ModPlayer
    {
        /// <summary>How long the player has been continuously firing (frames).</summary>
        public int ContinuousFireFrames;

        /// <summary>Number of hits landed (50 hits = 1 tick mark).</summary>
        public int HitAccumulator;

        /// <summary>Number of tick marks (12 = Midnight Strike).</summary>
        public int TickMarks;

        /// <summary>Whether the weapon is currently jammed.</summary>
        public bool IsJammed;

        /// <summary>Jam cooldown frames remaining.</summary>
        public int JamCooldown;

        /// <summary>Last fire frame (to detect stopping).</summary>
        public int LastFireFrame;

        /// <summary>Previous spin-up phase when firing stopped (for jam detection).</summary>
        public int PhaseWhenStopped;

        /// <summary>Whether Midnight Strike is ready.</summary>
        public bool MidnightReady => TickMarks >= 12;

        /// <summary>Current spin-up phase (1-5).</summary>
        public int CurrentPhase
        {
            get
            {
                float seconds = ContinuousFireFrames / 60f;
                if (seconds < 2f) return 1;
                if (seconds < 4f) return 2;
                if (seconds < 6f) return 3;
                if (seconds < 8f) return 4;
                return 5;
            }
        }

        /// <summary>Fire rate in frames between shots for current phase.</summary>
        public int GetFireDelay()
        {
            return CurrentPhase switch
            {
                1 => 20, // 3/s
                2 => 10, // 6/s
                3 => 5,  // 12/s
                4 => 3,  // ~18/s (20 ticks/s at 3 frame spacing)
                5 => 2,  // ~24/s (30 ticks/s at 2 frame spacing)
                _ => 20
            };
        }

        /// <summary>Muzzle flash scale for current phase.</summary>
        public float GetMuzzleFlashScale()
        {
            return CurrentPhase switch
            {
                1 => 0.15f,
                2 => 0.22f,
                3 => 0.3f,
                4 => 0.4f,
                5 => 0.55f,
                _ => 0.15f
            };
        }

        /// <summary>Bloom layer count for current phase.</summary>
        public int GetBloomLayers()
        {
            return CurrentPhase switch
            {
                1 => 1,
                2 => 2,
                3 => 2,
                4 => 3,
                5 => 4,
                _ => 1
            };
        }

        /// <summary>Screen shake intensity for current phase.</summary>
        public float GetScreenShake()
        {
            return CurrentPhase switch
            {
                1 => 0f,
                2 => 0f,
                3 => 0.5f,
                4 => 1.0f,
                5 => 2.0f,
                _ => 0f
            };
        }

        public void RegisterHit()
        {
            HitAccumulator++;
            if (HitAccumulator >= 50)
            {
                HitAccumulator = 0;
                if (TickMarks < 12)
                    TickMarks++;
            }
        }

        public void ConsumeMidnight()
        {
            TickMarks = 0;
            HitAccumulator = 0;
        }

        /// <summary>Called every frame by the weapon to advance spin-up.</summary>
        public void AdvanceFire()
        {
            ContinuousFireFrames++;
            LastFireFrame = (int)Main.GameUpdateCount;
        }

        /// <summary>Triggers jam — resets phase, starts cooldown.</summary>
        public void TriggerJam()
        {
            IsJammed = true;
            JamCooldown = 60; // 1 second jam
            PhaseWhenStopped = CurrentPhase;
            ContinuousFireFrames = 0;
        }

        public override void PostUpdate()
        {
            // Jam cooldown
            if (IsJammed)
            {
                JamCooldown--;
                if (JamCooldown <= 0)
                {
                    IsJammed = false;
                    JamCooldown = 0;
                }
            }

            // Detect fire stopping (if no fire in last 10 frames and was Phase 3+, jam)
            int framesSinceLastFire = (int)Main.GameUpdateCount - LastFireFrame;
            if (framesSinceLastFire > 10 && ContinuousFireFrames > 0)
            {
                int phase = CurrentPhase;
                if (phase >= 3 && !IsJammed)
                {
                    TriggerJam();
                }
                else
                {
                    ContinuousFireFrames = 0;
                }
            }
        }

        public override void OnRespawn()
        {
            ContinuousFireFrames = 0;
            HitAccumulator = 0;
            TickMarks = 0;
            IsJammed = false;
            JamCooldown = 0;
        }
    }
}
