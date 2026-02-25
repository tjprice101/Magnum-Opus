using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons
{
    /// <summary>
    /// VFX helper for the Ignition of the Bell melee thrust weapon.
    /// Handles thrust trail, blazing wave trails, impact VFX,
    /// and ignition burst effects.
    /// </summary>
    public static class IgnitionOfTheBellVFX
    {
        // =====================================================================
        //  HOLD ITEM VFX
        // =====================================================================

        /// <summary>
        /// Per-frame held-item VFX: smoldering ember glow with heat distortion feel.
        /// </summary>
        public static void HoldItemVFX(Player player)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Smoldering ember particles
            if (Main.rand.NextBool(5))
            {
                Vector2 pos = center + Main.rand.NextVector2Circular(15f, 15f);
                Vector2 vel = new Vector2(0, -0.5f) + Main.rand.NextVector2Circular(0.3f, 0.3f);
                Color col = Color.Lerp(LaCampanellaPalette.EmberRed, LaCampanellaPalette.FlameYellow, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 0.7f);
                d.noGravity = true;
                d.fadeIn = 0.5f;
            }

            // Light smoke wisps
            LaCampanellaVFXLibrary.SpawnAmbientSmoke(center, 20f);

            float pulse = 0.4f + MathF.Sin(time * 0.05f) * 0.1f;
            Lighting.AddLight(center, LaCampanellaPalette.EmberRed.ToVector3() * pulse);
        }

        // =====================================================================
        //  THRUST TRAIL VFX
        // =====================================================================

        /// <summary>
        /// Per-frame thrust projectile trail VFX.
        /// Intense fire trail with ember scatter and smoke.
        /// </summary>
        public static void ThrustTrailVFX(Vector2 tipPos, Vector2 thrustDirection, float progress)
        {
            if (Main.dedServ) return;

            // Intense fire dust along thrust path
            for (int i = 0; i < 3; i++)
            {
                Vector2 vel = -thrustDirection * Main.rand.NextFloat(2f, 4f)
                    + Main.rand.NextVector2Circular(1f, 1f);
                Color col = LaCampanellaPalette.PaletteLerp(LaCampanellaPalette.IgnitionCast, progress);
                Dust d = Dust.NewDustPerfect(tipPos, DustID.Torch, vel, 0, col, 1.6f);
                d.noGravity = true;
            }

            // Ember scatter at tip
            LaCampanellaVFXLibrary.SpawnEmberScatter(tipPos, 2, 2f);

            // Smoke trail
            if (Main.rand.NextBool(2))
                LaCampanellaVFXLibrary.SpawnHeavySmoke(tipPos, 1, 0.5f, 1.5f, 30);

            // Music note every few frames
            if (Main.rand.NextBool(6))
                LaCampanellaVFXLibrary.SpawnMusicNotes(tipPos, 1, 10f, 0.7f, 0.9f, 25);

            Lighting.AddLight(tipPos, LaCampanellaPalette.InfernalOrange.ToVector3() * (0.6f + progress * 0.3f));
        }

        // =====================================================================
        //  BLAZING WAVE TRAIL VFX
        // =====================================================================

        /// <summary>
        /// Per-frame blazing wave projectile trail.
        /// Fire waves that fan out from the thrust.
        /// </summary>
        public static void BlazingWaveTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            // Fire wave trail
            Vector2 perpendicular = new Vector2(-velocity.Y, velocity.X).SafeNormalize(Vector2.Zero);
            for (int i = 0; i < 2; i++)
            {
                float side = i == 0 ? 1f : -1f;
                Vector2 dustPos = pos + perpendicular * side * Main.rand.NextFloat(3f, 8f);
                Vector2 vel = -velocity.SafeNormalize(Vector2.Zero) * 1.5f + perpendicular * side * 0.5f;
                Color col = Color.Lerp(LaCampanellaPalette.InfernalOrange, LaCampanellaPalette.FlameYellow,
                    Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(dustPos, DustID.Torch, vel, 0, col, 1.3f);
                d.noGravity = true;
            }

            // Center fire
            if (Main.rand.NextBool(2))
            {
                Color coreCol = LaCampanellaPalette.MoltenCore;
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, -velocity * 0.1f, 0, coreCol, 1.0f);
                d.noGravity = true;
            }

            Lighting.AddLight(pos, LaCampanellaPalette.FlameYellow.ToVector3() * 0.5f);
        }

        // =====================================================================
        //  THRUST IMPACT
        // =====================================================================

        /// <summary>
        /// Thrust on-hit impact VFX — volcanic ignition burst.
        /// </summary>
        public static void ThrustImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            // Core ignition bloom
            LaCampanellaVFXLibrary.DrawBloom(pos, 0.6f);

            // Radial fire burst
            LaCampanellaVFXLibrary.SpawnRadialDustBurst(pos, 14, 6f);

            // Ember scatter (volcanic)
            LaCampanellaVFXLibrary.SpawnEmberScatter(pos, 8, 4f);

            // Bell chime ring
            CustomParticles.LaCampanellaImpactBurst(pos, 8);

            // Music notes
            LaCampanellaVFXLibrary.SpawnMusicNotes(pos, 3, 25f);

            // Smoke burst
            LaCampanellaVFXLibrary.SpawnHeavySmoke(pos, 3, 0.7f, 2.5f, 40);

            Lighting.AddLight(pos, LaCampanellaPalette.InfernalOrange.ToVector3() * 0.9f);
        }

        // =====================================================================
        //  BLAZING WAVE IMPACT
        // =====================================================================

        /// <summary>
        /// Blazing wave projectile on-hit/death VFX.
        /// </summary>
        public static void BlazingWaveImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            LaCampanellaVFXLibrary.ProjectileImpact(pos, 0.6f);

            // Extra flame fan-out
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 5f);
                Color col = LaCampanellaPalette.GetFireGradient((float)i / 8f);
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 1.2f);
                d.noGravity = true;
            }
        }

        // =====================================================================
        //  IGNITION ERUPTION (special activation)
        // =====================================================================

        /// <summary>
        /// Full ignition eruption VFX — volcanic activation burst.
        /// Use for special moves or maximum-charge attacks.
        /// </summary>
        public static void IgnitionEruptionVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            LaCampanellaVFXLibrary.InfernalEruption(pos, 0.8f);
        }
    }
}
