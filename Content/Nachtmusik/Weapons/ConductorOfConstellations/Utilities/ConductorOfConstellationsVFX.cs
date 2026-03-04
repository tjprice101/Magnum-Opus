using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.Nachtmusik.Weapons.ConductorOfConstellations.Utilities
{
    /// <summary>
    /// VFX for Conductor of Constellations — the grand conductor summoner staff.
    /// Authoritative, grand, cosmic — the conductor commands the stars themselves.
    /// The most complex summoner VFX, with constellation web patterns and grand conductor finales.
    /// </summary>
    public static class ConductorOfConstellationsVFX
    {
        // =====================================================================
        //  SummonVFX — Grand cosmic entrance with expanding constellation ring
        // =====================================================================
        public static void SummonVFX(Vector2 spawnPos)
        {
            // Constellation points form a web pattern
            int stars = 7;
            Vector2[] starPositions = new Vector2[stars];
            for (int i = 0; i < stars; i++)
            {
                float angle = MathHelper.TwoPi * i / stars;
                float radius = 36f;
                starPositions[i] = spawnPos + angle.ToRotationVector2() * radius;

                // Bright star at each constellation point
                for (int d = 0; d < 3; d++)
                {
                    Dust star = Dust.NewDustPerfect(starPositions[i], DustID.BlueTorch,
                        Main.rand.NextVector2Circular(0.5f, 0.5f), 0, default, 0.9f);
                    star.noGravity = true;
                    star.fadeIn = 1f;
                }
            }

            // Connecting line dust between constellation points
            for (int i = 0; i < stars; i++)
            {
                int next = (i + 2) % stars;
                Vector2 start = starPositions[i];
                Vector2 end = starPositions[next];
                int steps = 4;
                for (int s = 0; s <= steps; s++)
                {
                    Vector2 linePos = Vector2.Lerp(start, end, s / (float)steps);
                    Dust ld = Dust.NewDustPerfect(linePos, DustID.GoldFlame,
                        Vector2.Zero, 0, default, 0.4f);
                    ld.noGravity = true;
                    ld.fadeIn = 0.6f;
                }
            }

            // Central burst
            for (int i = 0; i < 12; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                int dustType = Main.rand.NextBool() ? DustID.BlueTorch : DustID.GoldFlame;
                Dust d = Dust.NewDustPerfect(spawnPos, dustType, vel, 0, default, 0.8f);
                d.noGravity = true;
                d.fadeIn = 1f;
            }

            // Expanding constellation ring sparkles
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 sparklePos = spawnPos + angle.ToRotationVector2() * 24f;
                var sparkle = new SparkleParticle(sparklePos, angle.ToRotationVector2() * 2.5f,
                    NachtmusikPalette.StarWhite * 0.8f, 0.35f, 22);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            NachtmusikVFXLibrary.SpawnMusicNotes(spawnPos, 3, 24f, 0.6f, 0.9f, 26);
            NachtmusikVFXLibrary.DrawBloom(spawnPos, 0.7f, 1f);
            NachtmusikVFXLibrary.AddPaletteLighting(spawnPos, 0.8f, 0.7f);
        }

        // =====================================================================
        //  HoldItemVFX — Commanding cosmic aura with constellation threads
        // =====================================================================
        public static void HoldItemVFX(Player player, int minionCount)
        {
            // Constellation web motes — more connections with more minions
            int moteFreq = Math.Max(1, 4 - minionCount / 2);
            if (Main.rand.NextBool(moteFreq))
            {
                float time = (float)Main.timeForVisualEffects * 0.02f;
                // Multiple orbiting points at different angular speeds
                for (int n = 0; n < Math.Min(minionCount, 4); n++)
                {
                    float angleOffset = MathHelper.TwoPi * n / Math.Max(minionCount, 1);
                    float angle = time * (1f + n * 0.3f) + angleOffset;
                    float radius = 24f + n * 8f;
                    Vector2 orbPos = player.Center + new Vector2(
                        (float)Math.Cos(angle) * radius,
                        (float)Math.Sin(angle) * radius * 0.5f);

                    Dust d = Dust.NewDustPerfect(orbPos, DustID.BlueTorch,
                        Vector2.Zero, 0, default, 0.35f);
                    d.noGravity = true;
                    d.fadeIn = 0.5f;
                }
            }

            // Gold conductor baton sparkle
            if (Main.rand.NextBool(6))
            {
                Dust g = Dust.NewDustPerfect(
                    player.Center + Main.rand.NextVector2Circular(16f, 16f),
                    DustID.GoldFlame, Main.rand.NextVector2Circular(0.2f, 0.2f), 0, default, 0.4f);
                g.noGravity = true;
            }

            NachtmusikVFXLibrary.AddNachtmusikLight(player.Center, 0.2f + minionCount * 0.05f);
        }

        // =====================================================================
        //  MinionAttackVFX — Conducting gesture burst toward target
        // =====================================================================
        public static void MinionAttackVFX(Vector2 minionPos, Vector2 direction)
        {
            // Sharp constellation beam flash
            for (int i = 0; i < 5; i++)
            {
                float t = i / 4f;
                Vector2 beamPos = minionPos + direction * (10f + t * 30f);
                Dust d = Dust.NewDustPerfect(beamPos, DustID.BlueTorch,
                    direction * (1f + Main.rand.NextFloat()) + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    0, default, 0.7f * (1f - t * 0.5f));
                d.noGravity = true;
            }

            // Gold accent sparks
            for (int i = 0; i < 3; i++)
            {
                Vector2 vel = direction * 2.5f + Main.rand.NextVector2Circular(2f, 2f);
                Dust g = Dust.NewDustPerfect(minionPos, DustID.GoldFlame, vel, 0, default, 0.5f);
                g.noGravity = true;
            }

            NachtmusikVFXLibrary.SpawnMusicNotes(minionPos, 1, 12f, 0.4f, 0.7f, 20);
            NachtmusikVFXLibrary.DrawBloom(minionPos, 0.3f, 0.55f);
        }

        // =====================================================================
        //  MinionAmbientVFX — Majestic trailing starlight
        // =====================================================================
        public static void MinionAmbientVFX(Vector2 pos, float visibility)
        {
            // Constellation web fragments around the minion
            if (Main.rand.NextBool(3))
            {
                float angle = (float)Main.timeForVisualEffects * 0.03f + Main.rand.NextFloat() * MathHelper.TwoPi;
                float radius = 12f + Main.rand.NextFloat() * 8f;
                Vector2 orbPos = pos + new Vector2(
                    (float)Math.Cos(angle) * radius,
                    (float)Math.Sin(angle) * radius);

                Dust d = Dust.NewDustPerfect(orbPos, DustID.BlueTorch,
                    Main.rand.NextVector2Circular(0.15f, 0.15f), 0, default, 0.3f * visibility);
                d.noGravity = true;
                d.fadeIn = 0.5f;
            }

            // Twinkling star accents
            if (Main.rand.NextBool(12))
            {
                NachtmusikVFXLibrary.SpawnTwinklingStars(pos, 1, 14f);
            }

            NachtmusikVFXLibrary.AddNachtmusikLight(pos, 0.15f * visibility);
        }
    }
}
