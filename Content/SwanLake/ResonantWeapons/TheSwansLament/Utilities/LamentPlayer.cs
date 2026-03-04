using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.TheSwansLament.Utilities
{
    /// <summary>
    /// Per-player state for The Swan's Lament (OVERHAUL).
    /// 
    /// Tracks:
    /// • Shot counter — every 6th shot fires a Destruction Halo instead of normal bullets
    /// • Lament's Echo — killing enemies temporarily increases fire rate and spread
    /// • Lamentation stacks per NPC (tracked in LamentGlobalNPC)
    /// </summary>
    public class LamentPlayer : ModPlayer
    {
        /// <summary>Timer for Lament's Echo buff (ticks remaining).</summary>
        public int EchoTimer;
        public bool IsEchoActive => EchoTimer > 0;

        /// <summary>Kill counter during echo — more kills extend and enhance.</summary>
        public int EchoKills;

        /// <summary>Shot counter for Destruction Halo — every 6th shot fires a halo.</summary>
        public int ShotCounter;

        /// <summary>Number of shots between Destruction Halos.</summary>
        public const int HaloInterval = 6;

        /// <summary>Advance shot counter, returns true if this is a Destruction Halo shot.</summary>
        public bool AdvanceShotCounter()
        {
            ShotCounter++;
            if (ShotCounter >= HaloInterval)
            {
                ShotCounter = 0;
                return true;
            }
            return false;
        }

        public void RegisterKill()
        {
            EchoTimer = Math.Min(EchoTimer + 180, 300); // Add 3s, cap 5s
            EchoKills++;
        }

        public override void PostUpdate()
        {
            EchoTimer = Math.Max(0, EchoTimer - 1);
            if (EchoTimer == 0)
                EchoKills = 0;
        }

        /// <summary>Fire rate multiplier (lower = faster).</summary>
        public float FireRateMult => IsEchoActive ? MathHelper.Lerp(1f, 0.5f, Math.Min(EchoKills / 5f, 1f)) : 1f;

        /// <summary>Spread angle multiplier.</summary>
        public float SpreadMult => IsEchoActive ? MathHelper.Lerp(1f, 1.8f, Math.Min(EchoKills / 5f, 1f)) : 1f;

        public override void OnRespawn() { EchoTimer = 0; EchoKills = 0; ShotCounter = 0; }
    }

    public static class LamentPlayerExt
    {
        public static LamentPlayer Lament(this Player player) => player.GetModPlayer<LamentPlayer>();
    }
}
