using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.Nachtmusik.Weapons.CelestialChorusBaton.Utilities
{
    /// <summary>
    /// VFX for Celestial Chorus Baton — the musical summoner staff.
    /// Choral, harmonic, conducting — the baton conducts a chorus of celestial guardians.
    /// </summary>
    public static class CelestialChorusBatonVFX
    {
        // =====================================================================
        //  SummonVFX — Cosmic entrance burst with expanding star ring
        // =====================================================================
        public static void SummonVFX(Vector2 spawnPos)
        {
            // Ring of music notes around spawn point — the chorus calls the guardian
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 ringPos = spawnPos + angle.ToRotationVector2() * 32f;
                NachtmusikVFXLibrary.SpawnMusicNotes(ringPos, 1, 8f, 0.5f, 0.8f, 22);
            }

            // Central cosmic flash
            for (int i = 0; i < 10; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                Dust d = Dust.NewDustPerfect(spawnPos, DustID.BlueTorch, vel, 0, default, 0.9f);
                d.noGravity = true;
                d.fadeIn = 1f;
            }

            // Gold accent sparks radiating outward
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 vel = angle.ToRotationVector2() * (2f + Main.rand.NextFloat() * 2f);
                Dust d = Dust.NewDustPerfect(spawnPos, DustID.GoldFlame, vel, 0, default, 0.6f);
                d.noGravity = true;
            }

            // Expanding star ring — sparkle particles
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi * i / 5f;
                Vector2 sparklePos = spawnPos + angle.ToRotationVector2() * 20f;
                var sparkle = new SparkleParticle(sparklePos, angle.ToRotationVector2() * 2f, NachtmusikPalette.StarWhite * 0.7f, 0.3f, 20);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            NachtmusikVFXLibrary.DrawBloom(spawnPos, 0.5f, 0.8f);
            NachtmusikVFXLibrary.AddPaletteLighting(spawnPos, 0.6f, 0.6f);
        }

        // =====================================================================
        //  HoldItemVFX — Conductor's harmonic presence, scales with minion count
        // =====================================================================
        public static void HoldItemVFX(Player player, int minionCount)
        {
            float time = (float)Main.timeForVisualEffects * 0.04f;

            // Music staff lines around the player
            if (Main.rand.NextBool(4))
            {
                int staffLine = Main.rand.Next(5);
                float yPosition = -14f + staffLine * 7f;
                float xShift = (float)Math.Sin(time * 2f + staffLine) * 16f;
                Vector2 staffPos = player.Center + new Vector2(xShift, yPosition);

                Dust d = Dust.NewDustPerfect(staffPos, DustID.BlueTorch,
                    new Vector2(0.4f * (staffLine % 2 == 0 ? 1 : -1), 0), 0, default, 0.35f);
                d.noGravity = true;
                d.fadeIn = 0.5f;
            }

            // Music note motes orbit the player — count scales with active minions
            int noteFreq = Math.Max(1, 6 - minionCount);
            if (Main.rand.NextBool(noteFreq))
            {
                float angle = time * 1.2f + Main.rand.NextFloat() * MathHelper.TwoPi;
                float radius = 20f + minionCount * 4f;
                Vector2 orbPos = player.Center + new Vector2(
                    (float)Math.Cos(angle) * radius,
                    (float)Math.Sin(angle) * radius * 0.5f);

                NachtmusikVFXLibrary.SpawnMusicNotes(orbPos, 1, 6f, 0.3f, 0.6f, 16);
            }

            // Harmonic resonance pulses — expanding ring dust
            if (Main.rand.NextBool(15))
            {
                int ringPoints = 6 + minionCount;
                float ringRadius = 18f;
                for (int i = 0; i < ringPoints; i++)
                {
                    float angle = MathHelper.TwoPi * i / ringPoints;
                    Vector2 ringPos = player.Center + angle.ToRotationVector2() * ringRadius;
                    Dust ring = Dust.NewDustPerfect(ringPos, DustID.BlueTorch,
                        angle.ToRotationVector2() * 0.8f, 0, default, 0.3f);
                    ring.noGravity = true;
                    ring.fadeIn = 0.4f;
                }
            }

            NachtmusikVFXLibrary.AddNachtmusikLight(player.Center, 0.18f + minionCount * 0.03f);
        }

        // =====================================================================
        //  MinionAmbientVFX — Guardian's subtle trailing particles
        // =====================================================================
        public static void MinionAmbientVFX(Vector2 pos, float visibility)
        {
            if (Main.rand.NextBool(4))
            {
                Vector2 offset = Main.rand.NextVector2Circular(16f, 16f);
                Dust d = Dust.NewDustPerfect(pos + offset, DustID.BlueTorch,
                    Main.rand.NextVector2Circular(0.2f, 0.2f), 0, default, 0.35f * visibility);
                d.noGravity = true;
                d.fadeIn = 0.6f;
            }

            if (Main.rand.NextBool(10))
            {
                NachtmusikVFXLibrary.SpawnMusicNotes(pos, 1, 12f, 0.2f * visibility, 0.5f, 16);
            }

            NachtmusikVFXLibrary.AddNachtmusikLight(pos, 0.1f * visibility);
        }

        // =====================================================================
        //  MinionImpactVFX — Celestial slash impact flash + cosmic sparks
        // =====================================================================
        public static void MinionImpactVFX(Vector2 hitPos)
        {
            NachtmusikVFXLibrary.ProjectileImpact(hitPos, 0.6f);

            // Resonant chime burst
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi * i / 5f;
                Vector2 vel = angle.ToRotationVector2() * (2f + Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(hitPos, DustID.BlueTorch, vel, 0, default, 0.6f);
                d.noGravity = true;
            }

            // Cosmic sparks
            for (int i = 0; i < 3; i++)
            {
                var spark = new SparkleParticle(hitPos, Main.rand.NextVector2Circular(3f, 3f),
                    NachtmusikPalette.StarWhite * 0.6f, 0.22f, 14);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            NachtmusikVFXLibrary.SpawnMusicNotes(hitPos, 2, 10f, 0.3f, 0.6f, 16);
        }
    }
}
