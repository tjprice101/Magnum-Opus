using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.MoonlightSonata.Weapons.EternalMoon.Projectiles;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.EternalMoon.Utilities
{
    /// <summary>
    /// Tracks per-player state for the Eternal Moon weapon system.
    /// Manages lunar combo phase, charge meter, echoing tides, and gravitational pull.
    /// </summary>
    public class EternalMoonPlayer : ModPlayer
    {
        /// <summary>Current lunar combo phase: 0=New Moon, 1=Waxing, 2=Half, 3=Waning, 4=Full Moon.</summary>
        public int LunarPhase;

        /// <summary>Timer that resets the combo if the player stops swinging.</summary>
        public int ComboResetTimer;

        /// <summary>Maximum ticks before combo resets.</summary>
        public const int ComboResetTime = 120;

        // === CHARGE METER ===
        /// <summary>Charge level from 0 to 1. Replaces the old tidal energy system.</summary>
        public float Charge = 0f;

        public const float ChargePerHit = 0.04f;
        public const float ChargePerKill = 0.15f;
        public const float MaxCharge = 1.0f;

        /// <summary>Whether the player is currently holding Eternal Moon.</summary>
        public bool IsHoldingEternalMoon = false;

        /// <summary>Whether charge is at maximum.</summary>
        public bool IsChargeFull => Charge >= MaxCharge;

        // === ECHOING TIDES ===
        /// <summary>Count of consecutive swings (resets with combo).</summary>
        public int SwingCount;

        /// <summary>Whether the next swing should echo the previous 3 as ghostly replays.</summary>
        public bool ShouldEchoTides => SwingCount > 0 && SwingCount % 4 == 0;

        // === GRAVITATIONAL PULL ===
        public Vector2 GravityPullCenter;
        public int GravityPullTimer;
        public const int GravityPullDuration = 60;
        public const float GravityPullRadius = 200f;
        public const float GravityPullStrength = 0.8f;

        /// <summary>
        /// VFX intensity multiplier derived from charge level (1.0 to 2.0).
        /// Replaces the old TidalPhaseMultiplier. Higher charge = more intense VFX.
        /// </summary>
        public float ChargeIntensityMultiplier => 1f + Charge;

        public bool IsFullMoon => LunarPhase == 4;
        public bool IsHalfMoon => LunarPhase == 2;

        private static HashSet<int> _eternalMoonProjectileTypes;

        public void AddCharge(float amount)
        {
            Charge = MathHelper.Clamp(Charge + amount, 0f, MaxCharge);
        }

        public void ConsumeCharge()
        {
            Charge = 0f;
        }

        public override void ResetEffects()
        {
            IsHoldingEternalMoon = false;
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

            // Gravitational pull effect on nearby NPCs
            if (GravityPullTimer > 0)
            {
                GravityPullTimer--;
                ApplyGravitationalPull();
            }
        }

        /// <summary>
        /// Advances the lunar combo phase by 1, wrapping at 5 (back to 0 after Full Moon).
        /// Resets the combo timer. Adds charge from the swing.
        /// </summary>
        public void AdvancePhase()
        {
            LunarPhase = (LunarPhase + 1) % 5;
            ComboResetTimer = ComboResetTime;
            SwingCount++;

            // Add charge per swing (more at higher lunar phases)
            float chargeGain = ChargePerHit + LunarPhase * 0.01f;
            AddCharge(chargeGain);
        }

        /// <summary>Starts a gravitational pull effect at the given center.</summary>
        public void StartGravitationalPull(Vector2 center)
        {
            GravityPullCenter = center;
            GravityPullTimer = GravityPullDuration;
        }

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

        private static HashSet<int> GetEternalMoonProjectileTypes()
        {
            if (_eternalMoonProjectileTypes == null)
            {
                _eternalMoonProjectileTypes = new HashSet<int>
                {
                    ModContent.ProjectileType<EternalMoonSwing>(),
                    ModContent.ProjectileType<EternalMoonWave>(),
                    ModContent.ProjectileType<EternalMoonGhost>(),
                    ModContent.ProjectileType<EternalMoonCrescentSlash>(),
                    ModContent.ProjectileType<EternalMoonTidalDetonation>(),
                };
            }
            return _eternalMoonProjectileTypes;
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!GetEternalMoonProjectileTypes().Contains(proj.type)) return;

            float charge = ChargePerHit;
            if (target.life <= 0)
                charge = ChargePerKill;

            AddCharge(charge);
        }

        public override void Unload()
        {
            _eternalMoonProjectileTypes = null;
        }
    }

    public static class EternalMoonPlayerExtensions
    {
        public static EternalMoonPlayer EternalMoon(this Player player) =>
            player.GetModPlayer<EternalMoonPlayer>();
    }
}
