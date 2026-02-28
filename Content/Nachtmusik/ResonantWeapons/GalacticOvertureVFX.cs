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
    /// Shader-driven VFX for Galactic Overture — the sweeping orchestral summoner staff.
    /// Uses OvertureAura.fx with wave-front radiance and musical staff lines.
    /// Grand, sweeping, orchestral — the Muse paints the battlefield with cosmic light.
    /// </summary>
    public static class GalacticOvertureVFX
    {
        // =====================================================================
        //  HoldItemVFX — Orchestral anticipation aura
        // =====================================================================
        public static void HoldItemVFX(Player player, int minionCount)
        {
            // Sweeping wave-like motes circling outward — the overture builds
            if (Main.rand.NextBool(3))
            {
                float time = (float)Main.timeForVisualEffects * 0.025f;
                float angle = time + Main.rand.NextFloat() * MathHelper.TwoPi;
                float radius = 22f + minionCount * 5f + (float)Math.Sin(time * 3f) * 6f;
                Vector2 orbPos = player.Center + new Vector2(
                    (float)Math.Cos(angle) * radius,
                    (float)Math.Sin(angle) * radius * 0.4f);

                int dustType = Main.rand.NextBool() ? DustID.GoldFlame : DustID.PurpleTorch;
                Dust d = Dust.NewDustPerfect(orbPos, dustType,
                    Main.rand.NextVector2Circular(0.4f, 0.4f), 0, default, 0.5f);
                d.noGravity = true;
                d.fadeIn = 0.7f;
            }

            // Occasional twinkling star accent
            if (Main.rand.NextBool(8))
            {
                NachtmusikVFXLibrary.SpawnTwinklingStars(player.Center, 1, 26f + minionCount * 3f);
            }

            NachtmusikVFXLibrary.AddNachtmusikLight(player.Center, 0.2f + minionCount * 0.04f);
        }

        // =====================================================================
        //  PreDrawInWorldBloom
        // =====================================================================
        public static void PreDrawInWorldBloom(SpriteBatch sb, Texture2D tex, Vector2 pos,
            Vector2 origin, float rotation, float scale)
        {
            NachtmusikVFXLibrary.DrawNachtmusikBloomStack(sb, pos,
                NachtmusikPalette.RadianceGold, NachtmusikPalette.CosmicPurple, scale * 0.25f, 0.35f);
        }

        // =====================================================================
        //  SummonVFX — Grand overture opening
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

            NachtmusikVFXLibrary.SpawnMusicNotes(spawnPos, 3, 20f, 0.5f, 0.8f, 24);
            NachtmusikVFXLibrary.DrawBloom(spawnPos, 0.6f, 0.9f);
            NachtmusikVFXLibrary.AddPaletteLighting(spawnPos, 0.7f, 0.6f);
        }

        // =====================================================================
        //  MinionAmbientVFX — Muse's celestial presence
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

        // =====================================================================
        //  MinionAttackVFX — Sweeping orchestral strike
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
        //  ProjectileTrailVFX — Cosmic light trail
        // =====================================================================
        public static void ProjectileTrailVFX(Vector2 pos, Vector2 velocity)
        {
            // Dual-tone trail — gold core, purple edge
            Vector2 dustVel = -velocity * 0.12f + Main.rand.NextVector2Circular(1f, 1f);
            Dust gold = Dust.NewDustPerfect(pos, DustID.GoldFlame, dustVel, 0, default, 0.6f);
            gold.noGravity = true;
            gold.fadeIn = 0.8f;

            if (Main.rand.NextBool(3))
            {
                Vector2 purpleVel = -velocity * 0.08f + Main.rand.NextVector2Circular(1.5f, 1.5f);
                Dust purple = Dust.NewDustPerfect(pos, DustID.PurpleTorch, purpleVel, 0, default, 0.45f);
                purple.noGravity = true;
            }

            if (Main.rand.NextBool(6))
            {
                NachtmusikVFXLibrary.SpawnTwinklingStars(pos, 1, 8f);
            }

            NachtmusikVFXLibrary.AddPaletteLighting(pos, 0.3f, 0.3f);
        }

        // =====================================================================
        //  ProjectileImpactVFX — Orchestral crescendo impact
        // =====================================================================
        public static void ProjectileImpactVFX(Vector2 hitPos)
        {
            NachtmusikVFXLibrary.ProjectileImpact(hitPos, 0.9f);

            // Radiant burst — the crescendo hits
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * (3f + Main.rand.NextFloat() * 2f);
                int dustType = i % 2 == 0 ? DustID.GoldFlame : DustID.PurpleTorch;
                Dust d = Dust.NewDustPerfect(hitPos, dustType, vel, 0, default, 0.8f);
                d.noGravity = true;
            }

            NachtmusikVFXLibrary.SpawnMusicNotes(hitPos, 2, 14f, 0.5f, 0.7f, 22);
            NachtmusikVFXLibrary.DrawBloom(hitPos, 0.4f, 0.65f);
        }

        // =====================================================================
        //  DespawnVFX — Overture's final bow
        // =====================================================================
        public static void DespawnVFX(Vector2 pos)
        {
            // Graceful ascending dissipation
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);
                vel.Y -= 1.5f; // Rise upward gracefully
                int dustType = Main.rand.NextBool() ? DustID.GoldFlame : DustID.PurpleTorch;
                Dust d = Dust.NewDustPerfect(pos, dustType, vel, 0, default, 0.6f);
                d.noGravity = true;
                d.fadeIn = 1f;
            }

            NachtmusikVFXLibrary.SpawnTwinklingStars(pos, 3, 18f);
            NachtmusikVFXLibrary.SpawnMusicNotes(pos, 2, 16f, 0.4f, 0.6f, 20);
        }
    }
}
