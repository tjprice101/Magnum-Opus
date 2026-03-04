using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.Nachtmusik.Weapons.GalacticOverture.Utilities
{
    /// <summary>
    /// VFX for Galactic Overture — the sweeping orchestral summoner staff.
    /// Grand, sweeping, orchestral — the Muse paints the battlefield with cosmic light.
    /// Golden radiance with cosmic purple accents.
    /// </summary>
    public static class GalacticOvertureVFX
    {
        // =====================================================================
        //  SummonVFX — Musical sparkle entrance
        // =====================================================================
        public static void SummonVFX(Vector2 spawnPos)
        {
            // Expanding wave rings — the first note of the overture
            for (int ring = 0; ring < 2; ring++)
            {
                float radius = 24f + ring * 20f;
                int points = 10 + ring * 4;
                for (int i = 0; i < points; i++)
                {
                    float angle = MathHelper.TwoPi * i / points;
                    Vector2 vel = angle.ToRotationVector2() * (1.5f + ring * 1.5f);
                    int dustType = ring == 0 ? DustID.GoldFlame : DustID.PurpleTorch;
                    Dust d = Dust.NewDustPerfect(spawnPos + angle.ToRotationVector2() * 8f,
                        dustType, vel, 0, default, 0.8f);
                    d.noGravity = true;
                    d.fadeIn = 1f;
                }
            }

            // Musical sparkle entrance particles
            for (int i = 0; i < 6; i++)
            {
                var sparkle = new SparkleParticle(spawnPos, Main.rand.NextVector2Circular(3f, 3f),
                    NachtmusikPalette.RadianceGold * 0.7f, 0.3f, 20);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            NachtmusikVFXLibrary.SpawnMusicNotes(spawnPos, 3, 20f, 0.5f, 0.8f, 24);
            NachtmusikVFXLibrary.DrawBloom(spawnPos, 0.6f, 0.9f);
            NachtmusikVFXLibrary.AddPaletteLighting(spawnPos, 0.7f, 0.6f);
        }

        // =====================================================================
        //  HoldItemVFX — Golden baton aura, scales with minion count
        // =====================================================================
        public static void HoldItemVFX(Player player, int minionCount)
        {
            float time = (float)Main.timeForVisualEffects * 0.03f;

            // Sweeping wave arcs — golden dust in wide sine-wave orchestral arcs
            if (Main.rand.NextBool(2))
            {
                float wavePhase = time * 2f + Main.rand.NextFloat() * MathHelper.Pi;
                float waveX = (float)Math.Sin(wavePhase) * (22f + minionCount * 4f);
                float waveY = (float)Math.Cos(wavePhase * 0.5f) * 10f;
                Vector2 wavePos = player.Center + new Vector2(waveX, waveY);
                Vector2 waveVel = new Vector2((float)Math.Cos(wavePhase), -(float)Math.Sin(wavePhase * 0.5f)) * 0.4f;

                int dustType = Main.rand.NextBool(3) ? DustID.PurpleTorch : DustID.GoldFlame;
                Dust d = Dust.NewDustPerfect(wavePos, dustType, waveVel, 0, default, 0.5f);
                d.noGravity = true;
                d.fadeIn = 0.7f;
            }

            // Expanding outward golden radiance pulses
            if (Main.rand.NextBool(6))
            {
                float pulse = (float)Math.Sin(time * 4f) * 0.5f + 0.5f;
                float expandRadius = 16f + pulse * 12f;
                float expandAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 expandPos = player.Center + expandAngle.ToRotationVector2() * expandRadius;
                Dust g = Dust.NewDustPerfect(expandPos, DustID.GoldFlame,
                    expandAngle.ToRotationVector2() * 0.3f * pulse, 0, default, 0.4f + pulse * 0.2f);
                g.noGravity = true;
                g.fadeIn = 0.6f;
            }

            // Twinkling accent
            if (Main.rand.NextBool(8))
            {
                NachtmusikVFXLibrary.SpawnTwinklingStars(player.Center, 1, 26f + minionCount * 3f);
            }

            NachtmusikVFXLibrary.AddNachtmusikLight(player.Center, 0.22f + minionCount * 0.04f);
        }

        // =====================================================================
        //  MinionAttackVFX — Fire burst toward target
        // =====================================================================
        public static void MinionAttackVFX(Vector2 minionPos, Vector2 direction)
        {
            // Arc of golden energy in sweep direction
            for (int i = 0; i < 6; i++)
            {
                float spread = MathHelper.ToRadians(30f);
                float angle = direction.ToRotation() + MathHelper.Lerp(-spread, spread, i / 5f);
                Vector2 vel = angle.ToRotationVector2() * (3f + Main.rand.NextFloat() * 2f);
                Dust d = Dust.NewDustPerfect(minionPos, DustID.GoldFlame, vel, 0, default, 0.7f);
                d.noGravity = true;
            }

            // Purple accent
            for (int i = 0; i < 3; i++)
            {
                Vector2 vel = direction * 2f + Main.rand.NextVector2Circular(1.5f, 1.5f);
                Dust d = Dust.NewDustPerfect(minionPos, DustID.PurpleTorch, vel, 0, default, 0.5f);
                d.noGravity = true;
            }

            NachtmusikVFXLibrary.SpawnMusicNotes(minionPos, 1, 12f, 0.4f, 0.7f, 20);
            NachtmusikVFXLibrary.DrawBloom(minionPos, 0.3f, 0.5f);
        }

        // =====================================================================
        //  MinionAmbientVFX — Gentle golden motes around the muse
        // =====================================================================
        public static void MinionAmbientVFX(Vector2 pos, float visibility)
        {
            if (Main.rand.NextBool(3))
            {
                // Cosmic purple drift around the muse
                Vector2 offset = Main.rand.NextVector2Circular(14f, 14f);
                Dust d = Dust.NewDustPerfect(pos + offset, DustID.PurpleTorch,
                    Main.rand.NextVector2Circular(0.3f, 0.3f), 0, default, 0.4f * visibility);
                d.noGravity = true;
                d.fadeIn = 0.7f;
            }

            // Occasional gold sparkle
            if (Main.rand.NextBool(8))
            {
                Dust g = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(10f, 10f),
                    DustID.GoldFlame, Vector2.Zero, 0, default, 0.3f * visibility);
                g.noGravity = true;
            }

            NachtmusikVFXLibrary.AddNachtmusikLight(pos, 0.12f * visibility);
        }
    }
}
