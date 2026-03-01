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
            float time = (float)Main.timeForVisualEffects * 0.03f;

            // === OUTER COSMIC PURPLE NEBULA LAYER === Slow, wide, atmospheric drift
            if (Main.rand.NextBool(3))
            {
                float outerAngle = time * 0.4f + Main.rand.NextFloat() * MathHelper.Pi;
                float outerRadius = 28f + Main.rand.NextFloat() * 15f;
                Vector2 outerPos = player.Center + new Vector2(
                    (float)Math.Cos(outerAngle) * outerRadius,
                    (float)Math.Sin(outerAngle) * outerRadius * 0.8f);
                Vector2 driftVel = new Vector2(
                    (float)Math.Sin(time * 0.7f) * 0.25f,
                    (float)Math.Cos(time * 0.5f) * 0.2f);

                Dust outer = Dust.NewDustPerfect(outerPos, DustID.PurpleTorch,
                    driftVel, 0, default, 0.7f);
                outer.noGravity = true;
                outer.fadeIn = 1.0f;
            }

            // === INNER NEBULA PINK CORE === Tighter orbit, warmer tone
            if (Main.rand.NextBool(4))
            {
                float innerAngle = time * 0.8f + Main.rand.NextFloat() * MathHelper.Pi;
                float innerRadius = 14f + Main.rand.NextFloat() * 10f;
                Vector2 innerPos = player.Center + new Vector2(
                    (float)Math.Cos(innerAngle) * innerRadius,
                    (float)Math.Sin(innerAngle) * innerRadius * 0.6f);

                Dust inner = Dust.NewDustPerfect(innerPos, DustID.PinkTorch,
                    Main.rand.NextVector2Circular(0.2f, 0.2f), 0, default, 0.55f);
                inner.noGravity = true;
                inner.fadeIn = 0.8f;
            }

            // === IRIDESCENT SHIMMER === Rare bright spark deep within the nebula
            if (Main.rand.NextBool(12))
            {
                Vector2 shimmerPos = player.Center + Main.rand.NextVector2Circular(16f, 16f);
                Dust shimmer = Dust.NewDustPerfect(shimmerPos, DustID.Enchanted_Gold,
                    Vector2.Zero, 0, default, 0.3f);
                shimmer.noGravity = true;
                shimmer.fadeIn = 0.4f;
            }

            NachtmusikVFXLibrary.AddNachtmusikLight(player.Center, 0.22f);
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
            // === WIDE NEBULA GAS DISCHARGE === Broad atmospheric cone
            for (int i = 0; i < 8; i++)
            {
                float spread = MathHelper.ToRadians(35f);
                Vector2 vel = direction.RotatedBy(Main.rand.NextFloat(-spread, spread))
                    * (1.5f + Main.rand.NextFloat() * 2.5f);
                int dustType = Main.rand.NextBool(3) ? DustID.PinkTorch : DustID.PurpleTorch;
                Dust d = Dust.NewDustPerfect(muzzlePos, dustType, vel, 0, default, 1.0f);
                d.noGravity = true;
                d.fadeIn = 1.1f;
            }

            // Lingering gas cloud at muzzle
            for (int i = 0; i < 4; i++)
            {
                Vector2 cloudVel = Main.rand.NextVector2Circular(1f, 1f);
                Dust cloud = Dust.NewDustPerfect(muzzlePos + Main.rand.NextVector2Circular(6f, 6f),
                    DustID.PurpleTorch, cloudVel, 0, default, 0.8f);
                cloud.noGravity = true;
                cloud.fadeIn = 1.2f;
            }

            // Deep cosmic accent flash
            if (Main.rand.NextBool(2))
            {
                Dust shimmer = Dust.NewDustPerfect(muzzlePos, DustID.Enchanted_Gold,
                    direction * 0.5f, 0, default, 0.35f);
                shimmer.noGravity = true;
            }

            NachtmusikVFXLibrary.SpawnMusicNotes(muzzlePos, 1, 14f, 0.4f, 0.6f, 22);
            NachtmusikVFXLibrary.DrawBloom(muzzlePos, 0.35f, 0.65f);
            NachtmusikVFXLibrary.AddPaletteLighting(muzzlePos, 0.4f, 0.55f);
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
