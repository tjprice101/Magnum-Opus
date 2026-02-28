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
    /// Shader-driven VFX for Nebula's Whisper — the splitting nebula blast ranged weapon.
    /// Uses NebulaScatter.fx for gaseous nebula cloud trails.
    /// Soft, atmospheric, dreamy — projectiles split like nebula fragments.
    /// </summary>
    public static class NebulasWhisperVFX
    {
        // =====================================================================
        //  HoldItemVFX — Nebula haze ambient
        // =====================================================================
        public static void HoldItemVFX(Player player)
        {
            if (Main.rand.NextBool(4))
            {
                // Drifting nebula wisps
                Vector2 offset = Main.rand.NextVector2Circular(28f, 28f);
                Vector2 vel = Main.rand.NextVector2Circular(0.4f, 0.4f);

                Dust d = Dust.NewDustPerfect(player.Center + offset, DustID.PinkTorch, vel, 0, default, 0.5f);
                d.noGravity = true;
                d.fadeIn = 0.8f;
            }

            if (Main.rand.NextBool(8))
            {
                // Cosmic purple accent mote
                Dust p = Dust.NewDustPerfect(player.Center + Main.rand.NextVector2Circular(20f, 20f),
                    DustID.PurpleTorch, Main.rand.NextVector2Circular(0.2f, 0.2f), 0, default, 0.4f);
                p.noGravity = true;
            }

            NachtmusikVFXLibrary.AddNachtmusikLight(player.Center, 0.2f);
        }

        // =====================================================================
        //  PreDrawInWorldBloom
        // =====================================================================
        public static void PreDrawInWorldBloom(SpriteBatch sb, Texture2D tex, Vector2 pos,
            Vector2 origin, float rotation, float scale)
        {
            NachtmusikVFXLibrary.DrawNachtmusikBloomStack(sb, pos,
                NachtmusikPalette.CosmicPurple, NachtmusikPalette.NebulaPink, scale * 0.3f, 0.4f);
        }

        // =====================================================================
        //  MuzzleFlashVFX — Nebula gas discharge
        // =====================================================================
        public static void MuzzleFlashVFX(Vector2 muzzlePos, Vector2 direction)
        {
            // Soft nebula gas puff
            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = direction * (2f + Main.rand.NextFloat() * 2f)
                    + Main.rand.NextVector2Circular(2f, 2f);
                int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.PinkTorch;
                Dust d = Dust.NewDustPerfect(muzzlePos, dustType, vel, 0, default, 0.9f);
                d.noGravity = true;
                d.fadeIn = 1f;
            }

            NachtmusikVFXLibrary.SpawnMusicNotes(muzzlePos, 1, 12f, 0.4f, 0.6f, 20);
            NachtmusikVFXLibrary.DrawBloom(muzzlePos, 0.3f, 0.6f);
            NachtmusikVFXLibrary.AddPaletteLighting(muzzlePos, 0.4f, 0.5f);
        }

        // =====================================================================
        //  ProjectileTrailVFX — Nebula cloud trail
        // =====================================================================
        public static void ProjectileTrailVFX(Vector2 pos, Vector2 velocity)
        {
            // Gaseous trail puffs
            Vector2 dustVel = -velocity * 0.15f + Main.rand.NextVector2Circular(1.5f, 1.5f);
            int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.PinkTorch;
            Dust d = Dust.NewDustPerfect(pos, dustType, dustVel, 0, default, 0.7f);
            d.noGravity = true;
            d.fadeIn = 0.9f;

            if (Main.rand.NextBool(5))
            {
                NachtmusikVFXLibrary.SpawnTwinklingStars(pos, 1, 8f);
            }

            NachtmusikVFXLibrary.AddPaletteLighting(pos, 0.4f, 0.3f);
        }

        // =====================================================================
        //  SmallHitVFX — Nebula fragment impact
        // =====================================================================
        public static void SmallHitVFX(Vector2 hitPos)
        {
            NachtmusikVFXLibrary.ProjectileImpact(hitPos, 0.7f);

            // Nebula gas burst on impact
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi * i / 5f;
                Vector2 vel = angle.ToRotationVector2() * (2f + Main.rand.NextFloat() * 1.5f);
                int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.PinkTorch;
                Dust d = Dust.NewDustPerfect(hitPos, dustType, vel, 0, default, 0.8f);
                d.noGravity = true;
            }

            NachtmusikVFXLibrary.SpawnMusicNotes(hitPos, 1, 10f, 0.3f, 0.6f, 18);
            NachtmusikVFXLibrary.DrawBloom(hitPos, 0.25f, 0.5f);
        }

        // =====================================================================
        //  ProjectileDeathVFX — Nebula dissipation
        // =====================================================================
        public static void ProjectileDeathVFX(Vector2 pos)
        {
            // Soft gas cloud dissipation
            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);
                int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.PinkTorch;
                Dust d = Dust.NewDustPerfect(pos, dustType, vel, 0, default, 0.8f);
                d.noGravity = true;
                d.fadeIn = 1f;
            }
            NachtmusikVFXLibrary.SpawnMusicNotes(pos, 1, 12f, 0.4f, 0.6f, 18);
        }
    }
}
