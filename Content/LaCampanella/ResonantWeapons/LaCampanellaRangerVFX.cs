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
    /// VFX helper for La Campanella ranger weapons (PiercingBellsResonance).
    /// Handles bullet trail, rapid-fire intensification, Resonant Bell Blast (20th shot),
    /// and fire comet projectile VFX.
    /// </summary>
    public static class LaCampanellaRangerVFX
    {
        // =====================================================================
        //  HOLD ITEM VFX
        // =====================================================================

        /// <summary>
        /// Per-frame held-item VFX: heat haze and barrel glow.
        /// </summary>
        public static void HoldItemVFX(Player player)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Barrel heat haze (rising ember particles)
            if (Main.rand.NextBool(6))
            {
                Vector2 pos = center + Main.rand.NextVector2Circular(10f, 10f);
                Vector2 vel = new Vector2(0, -0.6f) + Main.rand.NextVector2Circular(0.2f, 0.2f);
                Color col = Color.Lerp(LaCampanellaPalette.DeepEmber, LaCampanellaPalette.InfernalOrange,
                    Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 0.5f);
                d.noGravity = true;
            }

            float pulse = 0.35f + MathF.Sin(time * 0.05f) * 0.1f;
            Lighting.AddLight(center, LaCampanellaPalette.EmberRed.ToVector3() * pulse);
        }

        // =====================================================================
        //  BULLET TRAIL VFX
        // =====================================================================

        /// <summary>
        /// Per-frame bullet trail VFX for standard fire bullets.
        /// Fire comet trail with smoky tail.
        /// </summary>
        public static void BulletTrailVFX(Vector2 pos, Vector2 velocity, float fireRateMultiplier = 1f)
        {
            if (Main.dedServ) return;

            // Fire comet trail (intensity scales with fire rate)
            int dustCount = 1 + (int)(fireRateMultiplier * 0.5f);
            for (int i = 0; i < dustCount; i++)
            {
                Vector2 vel = -velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1.5f, 3f)
                    + Main.rand.NextVector2Circular(0.5f, 0.5f);
                Color col = LaCampanellaPalette.PaletteLerp(LaCampanellaPalette.RangerComet,
                    Main.rand.NextFloat(0.2f, 0.8f));
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 1.1f + fireRateMultiplier * 0.2f);
                d.noGravity = true;
            }

            // Smoky tail
            if (Main.rand.NextBool(3))
            {
                Vector2 smokeVel = -velocity.SafeNormalize(Vector2.Zero) * 0.5f + new Vector2(0, -0.3f);
                LaCampanellaVFXLibrary.SpawnHeavySmoke(pos, 1, 0.3f + fireRateMultiplier * 0.1f, 1f, 25);
            }

            // Golden sparkle
            if (Main.rand.NextBool(2))
            {
                Dust d = Dust.NewDustPerfect(pos, DustID.Enchanted_Gold,
                    Main.rand.NextVector2Circular(1f, 1f), 0, LaCampanellaPalette.MoltenCore, 0.5f);
                d.noGravity = true;
            }

            Lighting.AddLight(pos, LaCampanellaPalette.InfernalOrange.ToVector3() *
                (0.4f + fireRateMultiplier * 0.1f));
        }

        // =====================================================================
        //  RAPID FIRE INTENSIFICATION
        // =====================================================================

        /// <summary>
        /// Muzzle flash VFX on each shot, scaling with sustained fire rate.
        /// </summary>
        public static void MuzzleFlashVFX(Vector2 muzzlePos, Vector2 shootDirection, float fireRateMultiplier)
        {
            if (Main.dedServ) return;

            float intensity = 0.3f + fireRateMultiplier * 0.3f;

            // Muzzle fire burst
            for (int i = 0; i < 3 + (int)(fireRateMultiplier * 2); i++)
            {
                float spread = 0.3f - fireRateMultiplier * 0.1f; // Tighter spread at higher fire rate
                Vector2 vel = shootDirection.RotatedBy(Main.rand.NextFloat(-spread, spread))
                    * Main.rand.NextFloat(3f, 6f);
                Color col = LaCampanellaPalette.GetFireGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(muzzlePos, DustID.Torch, vel, 0, col, 1.0f + intensity);
                d.noGravity = true;
            }

            // Quick smoke puff
            LaCampanellaVFXLibrary.SpawnHeavySmoke(muzzlePos, 1, 0.3f + intensity * 0.3f, 2f, 20);

            Lighting.AddLight(muzzlePos, LaCampanellaPalette.InfernalOrange.ToVector3() * intensity);
        }

        // =====================================================================
        //  BULLET IMPACT
        // =====================================================================

        /// <summary>
        /// Bullet on-hit impact VFX.
        /// </summary>
        public static void BulletImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            LaCampanellaVFXLibrary.MeleeImpact(pos, 0);
        }

        // =====================================================================
        //  RESONANT BELL BLAST (every 20th shot)
        // =====================================================================

        /// <summary>
        /// Resonant Bell Blast VFX — the empowered 20th-shot special.
        /// Bell shockwave explosion + massive fire burst.
        /// </summary>
        public static void ResonantBellBlastVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            // Bell shockwave explosion
            LaCampanellaVFXLibrary.BellShockwaveImpact(pos, 1.2f);

            // Extra massive fire burst
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi * i / 20f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 8f);
                Color col = LaCampanellaPalette.GetFireGradient((float)i / 20f);
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 1.5f);
                d.noGravity = true;
            }

            // Massive smoke cloud
            LaCampanellaVFXLibrary.SpawnHeavySmoke(pos, 8, 1.0f, 5f, 70);

            MagnumScreenEffects.AddScreenShake(6f);
            Lighting.AddLight(pos, LaCampanellaPalette.WhiteHot.ToVector3() * 1.5f);
        }

        // =====================================================================
        //  RESONANT BELL BLAST PROJECTILE TRAIL
        // =====================================================================

        /// <summary>
        /// Enhanced trail VFX for the Resonant Bell Blast projectile.
        /// Thicker fire comet with bell chime particles.
        /// </summary>
        public static void ResonantBlastTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            // Thick fire comet trail
            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = -velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(2f, 4f)
                    + Main.rand.NextVector2Circular(1f, 1f);
                Color col = LaCampanellaPalette.PaletteLerp(LaCampanellaPalette.RangerComet,
                    Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 1.6f);
                d.noGravity = true;
            }

            // Bell chime sparkles
            if (Main.rand.NextBool(2))
            {
                Color chimeCol = LaCampanellaPalette.BellChime;
                Dust d = Dust.NewDustPerfect(pos, DustID.GoldFlame,
                    Main.rand.NextVector2Circular(1.5f, 1.5f), 0, chimeCol, 1.0f);
                d.noGravity = true;
            }

            // Smoke trail
            LaCampanellaVFXLibrary.SpawnHeavySmoke(pos, 1, 0.5f, 1.5f, 30);

            // Music notes
            if (Main.rand.NextBool(5))
                LaCampanellaVFXLibrary.SpawnMusicNotes(pos, 1, 10f, 0.7f, 0.9f, 25);

            Lighting.AddLight(pos, LaCampanellaPalette.BellGold.ToVector3() * 0.7f);
        }
    }
}
