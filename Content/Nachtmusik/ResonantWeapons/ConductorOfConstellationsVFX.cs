using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.Nachtmusik.ResonantWeapons
{
    /// <summary>
    /// Shader-driven VFX for Conductor of Constellations — the grand conductor summoner staff.
    /// Uses StellarConductorAura.fx for orbiting constellation web aura.
    /// Authoritative, grand, cosmic — the conductor commands the stars themselves.
    /// The most complex summoner VFX, with constellation web patterns and grand conductor finales.
    /// </summary>
    public static class ConductorOfConstellationsVFX
    {
        // =====================================================================
        //  HoldItemVFX — Conductor's commanding presence
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
        //  PreDrawInWorldBloom
        // =====================================================================
        public static void PreDrawInWorldBloom(SpriteBatch sb, Texture2D tex, Vector2 pos,
            Vector2 origin, float rotation, float scale)
        {
            NachtmusikVFXLibrary.DrawNachtmusikBloomStack(sb, pos,
                NachtmusikPalette.ConstellationBlue, NachtmusikPalette.StarGold, scale * 0.3f, 0.4f);
        }

        // =====================================================================
        //  SummonVFX — Constellation web summoning ritual
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

            NachtmusikVFXLibrary.SpawnMusicNotes(spawnPos, 3, 24f, 0.6f, 0.9f, 26);
            NachtmusikVFXLibrary.DrawBloom(spawnPos, 0.7f, 1f);
            NachtmusikVFXLibrary.AddPaletteLighting(spawnPos, 0.8f, 0.7f);
        }

        // =====================================================================
        //  MinionAmbientVFX — Stellar conductor's commanded presence
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

            if (Main.rand.NextBool(12))
            {
                NachtmusikVFXLibrary.SpawnTwinklingStars(pos, 1, 14f);
            }

            NachtmusikVFXLibrary.AddNachtmusikLight(pos, 0.15f * visibility);
        }

        // =====================================================================
        //  MinionAttackVFX — Conductor's baton strike
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
        //  ProjectileTrailVFX — Constellation beam trail
        // =====================================================================
        public static void ProjectileTrailVFX(Vector2 pos, Vector2 velocity)
        {
            // Clean blue constellation energy
            Vector2 dustVel = -velocity * 0.1f + Main.rand.NextVector2Circular(0.8f, 0.8f);
            Dust d = Dust.NewDustPerfect(pos, DustID.BlueTorch, dustVel, 0, default, 0.55f);
            d.noGravity = true;
            d.fadeIn = 0.7f;

            // Gold accent motes
            if (Main.rand.NextBool(4))
            {
                Dust g = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(5f, 5f),
                    DustID.GoldFlame, -velocity * 0.05f, 0, default, 0.35f);
                g.noGravity = true;
            }

            if (Main.rand.NextBool(5))
            {
                NachtmusikVFXLibrary.SpawnTwinklingStars(pos, 1, 8f);
            }

            NachtmusikVFXLibrary.AddPaletteLighting(pos, 0.35f, 0.3f);
        }

        // =====================================================================
        //  ProjectileImpactVFX — Constellation detonation
        // =====================================================================
        public static void ProjectileImpactVFX(Vector2 hitPos)
        {
            NachtmusikVFXLibrary.ProjectileImpact(hitPos, 1f);

            // Mini constellation burst — star points explode outward
            for (int i = 0; i < 7; i++)
            {
                float angle = MathHelper.TwoPi * i / 7f;
                Vector2 vel = angle.ToRotationVector2() * (3.5f + Main.rand.NextFloat() * 2f);
                Dust d = Dust.NewDustPerfect(hitPos, DustID.BlueTorch, vel, 0, default, 0.8f);
                d.noGravity = true;
            }

            // Gold web lines between impact points
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f + MathHelper.PiOver4;
                Vector2 vel = angle.ToRotationVector2() * (2f + Main.rand.NextFloat());
                Dust g = Dust.NewDustPerfect(hitPos, DustID.GoldFlame, vel, 0, default, 0.5f);
                g.noGravity = true;
            }

            NachtmusikVFXLibrary.SpawnMusicNotes(hitPos, 2, 14f, 0.5f, 0.8f, 22);
            NachtmusikVFXLibrary.DrawBloom(hitPos, 0.45f, 0.7f);
        }

        // =====================================================================
        //  GrandConductorVFX — The conductor's ultimate command
        // =====================================================================
        public static void GrandConductorVFX(Vector2 pos, float intensity = 1f)
        {
            // Massive constellation explosion — all star points ignite
            int starCount = (int)(12 * intensity);
            for (int i = 0; i < starCount; i++)
            {
                float angle = MathHelper.TwoPi * i / starCount;
                float radius = 16f * intensity;
                Vector2 starPos = pos + angle.ToRotationVector2() * radius;
                Vector2 vel = angle.ToRotationVector2() * (4f + Main.rand.NextFloat() * 3f) * intensity;

                Dust d = Dust.NewDustPerfect(starPos, DustID.BlueTorch, vel, 0, default, 1f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            // Grand golden radiance burst
            for (int i = 0; i < (int)(8 * intensity); i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f) * intensity;
                Dust g = Dust.NewDustPerfect(pos, DustID.GoldFlame, vel, 0, default, 0.9f);
                g.noGravity = true;
                g.fadeIn = 1.1f;
            }

            // Constellation web connecting lines — radiant
            for (int i = 0; i < 5; i++)
            {
                float angleA = MathHelper.TwoPi * i / 5f;
                float angleB = MathHelper.TwoPi * ((i + 2) % 5) / 5f;
                Vector2 start = pos + angleA.ToRotationVector2() * 24f * intensity;
                Vector2 end = pos + angleB.ToRotationVector2() * 24f * intensity;
                int steps = 3;
                for (int s = 0; s <= steps; s++)
                {
                    Vector2 lp = Vector2.Lerp(start, end, s / (float)steps);
                    Dust ld = Dust.NewDustPerfect(lp, DustID.GoldFlame,
                        Main.rand.NextVector2Circular(0.5f, 0.5f), 0, default, 0.6f * intensity);
                    ld.noGravity = true;
                }
            }

            NachtmusikVFXLibrary.SpawnMusicNotes(pos, (int)(4 * intensity), 20f, 0.6f, 1f, 28);
            NachtmusikVFXLibrary.SpawnTwinklingStars(pos, (int)(5 * intensity), 30f);
            NachtmusikVFXLibrary.DrawBloom(pos, 0.6f * intensity, 1f);
            NachtmusikVFXLibrary.AddPaletteLighting(pos, 0.8f * intensity, 0.8f);
        }

        // =====================================================================
        //  DespawnVFX — Constellation fades to silence
        // =====================================================================
        public static void DespawnVFX(Vector2 pos)
        {
            // Stars scatter outward and fade
            for (int i = 0; i < 7; i++)
            {
                float angle = MathHelper.TwoPi * i / 7f;
                Vector2 vel = angle.ToRotationVector2() * (1.5f + Main.rand.NextFloat());
                vel.Y -= 0.8f;
                Dust d = Dust.NewDustPerfect(pos, DustID.BlueTorch, vel, 0, default, 0.6f);
                d.noGravity = true;
                d.fadeIn = 0.9f;
            }

            // Gold farewell sparkle
            for (int i = 0; i < 4; i++)
            {
                Dust g = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(10f, 10f),
                    DustID.GoldFlame, new Vector2(0, -1f), 0, default, 0.5f);
                g.noGravity = true;
            }

            NachtmusikVFXLibrary.SpawnTwinklingStars(pos, 4, 20f);
            NachtmusikVFXLibrary.SpawnMusicNotes(pos, 2, 16f, 0.4f, 0.7f, 22);
        }
    }
}
