using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.CalloftheBlackSwan.Utilities
{
    /// <summary>
    /// Per-player state tracking for Call of the Black Swan.
    /// 
    /// OVERHAUL — Grace / Dark Mirror system:
    /// • Successive hits without getting hit build Grace stacks (max 5)
    /// • Each Grace stack: +8% swing speed, trail becomes more prismatic
    /// • At max Grace, next swing releases Prismatic Swan projectile
    /// • Taking damage while swinging converts Grace → Dark Mirror stacks
    /// • Dark Mirror: +15% damage but -5% speed per stack
    /// 
    /// Also tracks combo state and the legacy empowerment system (flare hits).
    /// </summary>
    public class BlackSwanPlayer : ModPlayer
    {
        #region Grace / Dark Mirror System

        /// <summary>Grace stacks — built by hitting without being hit. Max 5.</summary>
        public int GraceStacks;

        /// <summary>Dark Mirror stacks — converted from Grace when player takes damage mid-swing. Max 5.</summary>
        public int DarkMirrorStacks;

        /// <summary>Maximum Grace/Dark Mirror stacks.</summary>
        public const int MaxStacks = 5;

        /// <summary>Whether the player is currently mid-swing (for Dark Mirror conversion).</summary>
        public bool IsSwinging;

        /// <summary>Timer for Grace decay — stacks decay after 4 seconds of not hitting.</summary>
        public int GraceDecayTimer;

        /// <summary>Ticks before Grace stacks begin to decay.</summary>
        public const int GraceDecayDelay = 240; // 4 seconds

        /// <summary>Timer for Dark Mirror decay — stacks decay after 6 seconds.</summary>
        public int DarkMirrorDecayTimer;

        /// <summary>Ticks before Dark Mirror stacks decay.</summary>
        public const int DarkMirrorDecayDelay = 360; // 6 seconds

        /// <summary>Whether the Prismatic Swan has been released this Grace cycle.</summary>
        public bool PrismaticSwanReleased;

        #endregion

        #region Empowerment System (Legacy — Flare-Hit Based)

        /// <summary>Number of flare hits accumulated toward empowerment.</summary>
        public int FlareHitCount;

        /// <summary>Whether the next swing is empowered.</summary>
        public bool IsEmpowered;

        /// <summary>Remaining ticks of empowerment window.</summary>
        public int EmpowermentTimer;

        /// <summary>Flares needed to trigger empowerment.</summary>
        public const int FlaresNeeded = 3;

        /// <summary>Duration of empowerment window in ticks (5 seconds).</summary>
        public const int EmpowermentDuration = 300;

        #endregion

        #region Combo Tracking

        /// <summary>Current combo step (0, 1, or 2).</summary>
        public int ComboStep;

        /// <summary>Ticks since last swing, for combo reset.</summary>
        public int ComboResetTimer;

        /// <summary>Frames of inactivity before combo resets.</summary>
        public const int ComboResetDelay = 45;

        #endregion

        public override void ResetEffects()
        {
            // Tick down empowerment
            if (EmpowermentTimer > 0)
            {
                EmpowermentTimer--;
                if (EmpowermentTimer <= 0)
                {
                    IsEmpowered = false;
                    FlareHitCount = 0;
                }
            }

            // Tick down combo reset
            if (ComboResetTimer > 0)
            {
                ComboResetTimer--;
                if (ComboResetTimer <= 0)
                    ComboStep = 0;
            }

            // Grace decay
            if (GraceDecayTimer > 0)
            {
                GraceDecayTimer--;
            }
            else if (GraceStacks > 0)
            {
                GraceStacks--;
                GraceDecayTimer = 60; // 1 second between each stack decay
            }

            // Dark Mirror decay
            if (DarkMirrorDecayTimer > 0)
            {
                DarkMirrorDecayTimer--;
            }
            else if (DarkMirrorStacks > 0)
            {
                DarkMirrorStacks--;
                DarkMirrorDecayTimer = 60;
            }
        }

        /// <summary>Register a successful hit — builds Grace stacks.</summary>
        public void RegisterHit()
        {
            if (GraceStacks < MaxStacks)
            {
                GraceStacks++;
            }
            GraceDecayTimer = GraceDecayDelay;

            // Check if max Grace reached
            if (GraceStacks >= MaxStacks && !PrismaticSwanReleased)
            {
                // Prismatic Swan will be released on the next swing
            }
        }

        /// <summary>Called when the player takes damage — converts Grace to Dark Mirror if swinging.</summary>
        public override void OnHurt(Player.HurtInfo info)
        {
            if (IsSwinging && GraceStacks > 0)
            {
                // Convert Grace → Dark Mirror
                DarkMirrorStacks = Math.Min(DarkMirrorStacks + GraceStacks, MaxStacks);
                DarkMirrorDecayTimer = DarkMirrorDecayDelay;
                GraceStacks = 0;
                GraceDecayTimer = 0;
                PrismaticSwanReleased = false;
            }
        }

        /// <summary>Consume Grace for Prismatic Swan release.</summary>
        public void ConsumePrismaticSwan()
        {
            PrismaticSwanReleased = true;
            GraceStacks = 0;
            GraceDecayTimer = 0;
        }

        /// <summary>Get the swing speed multiplier from Grace stacks.</summary>
        public float GetGraceSpeedMultiplier()
        {
            float graceBonus = 1f + GraceStacks * 0.08f;
            float mirrorPenalty = 1f - DarkMirrorStacks * 0.05f;
            return graceBonus * mirrorPenalty;
        }

        /// <summary>Get the damage multiplier from Dark Mirror stacks.</summary>
        public float GetDarkMirrorDamageMultiplier()
        {
            return 1f + DarkMirrorStacks * 0.15f;
        }

        /// <summary>Whether the player has max Grace and is ready for Prismatic Swan.</summary>
        public bool IsMaxGrace => GraceStacks >= MaxStacks && !PrismaticSwanReleased;

        /// <summary>Prismatic intensity for trail rendering (0-1 based on Grace stacks).</summary>
        public float PrismaticIntensity => (float)GraceStacks / MaxStacks;

        /// <summary>Register a flare hit. Triggers empowerment at threshold.</summary>
        public void RegisterFlareHit()
        {
            FlareHitCount++;
            if (FlareHitCount >= FlaresNeeded)
            {
                IsEmpowered = true;
                EmpowermentTimer = EmpowermentDuration;
                FlareHitCount = 0;
            }
        }

        /// <summary>Consume the empowerment for an empowered swing.</summary>
        public void ConsumeEmpowerment()
        {
            IsEmpowered = false;
            EmpowermentTimer = 0;
        }

        /// <summary>Advance the combo step and reset the timer.</summary>
        public void AdvanceCombo()
        {
            ComboStep = (ComboStep + 1) % 3;
            ComboResetTimer = ComboResetDelay;
        }
    }

    /// <summary>Extension method for convenient access.</summary>
    public static class BlackSwanPlayerExtensions
    {
        public static BlackSwanPlayer BlackSwan(this Player player)
            => player.GetModPlayer<BlackSwanPlayer>();
    }
}
