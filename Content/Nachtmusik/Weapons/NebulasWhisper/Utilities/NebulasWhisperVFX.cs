using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.Nachtmusik.Weapons.NebulasWhisper.Utilities
{
    /// <summary>
    /// Static VFX helper for Nebula's Whisper — the expanding nebula cannon.
    /// Soft nebula puff muzzle flash, ambient nebula mist, lingering residue,
    /// and massive convergence Whisper Storm effect.
    /// </summary>
    public static class NebulasWhisperVFX
    {
        // =====================================================================
        //  MuzzleFlashVFX — Soft nebula puff, cosmic motes
        // =====================================================================
        public static void MuzzleFlashVFX(Vector2 muzzlePos, Vector2 direction)
        {
            // === WIDE NEBULA GAS DISCHARGE — broad atmospheric cone ===
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
                    DustID.PurpleTorch, cloudVel, 60, default, 0.8f);
                cloud.noGravity = true;
                cloud.fadeIn = 1.2f;
            }

            // Cosmic mote accents
            for (int i = 0; i < 3; i++)
            {
                var mote = new GenericGlowParticle(
                    muzzlePos + Main.rand.NextVector2Circular(8f, 8f),
                    direction * Main.rand.NextFloat(0.5f, 1.5f),
                    NachtmusikPalette.CosmicPurple * 0.6f, 0.2f, 18, true);
                MagnumParticleHandler.SpawnParticle(mote);
            }

            // Deep cosmic shimmer accent
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
        //  HoldItemVFX — Ambient nebula mist around player
        // =====================================================================
        public static void HoldItemVFX(Player player)
        {
            float time = (float)Main.timeForVisualEffects * 0.03f;

            // Outer cosmic purple nebula layer — slow, wide atmospheric drift
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

            // Inner violet core — tighter orbit, warmer tone
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

            // Iridescent shimmer — rare bright spark
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
        //  NebulaResidueVFX — Lingering cloud particles at position
        // =====================================================================
        public static void NebulaResidueVFX(Vector2 pos)
        {
            // Lingering nebula puffs — slow drifting fog
            for (int i = 0; i < 3; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(12f, 12f);
                Vector2 driftVel = Main.rand.NextVector2Circular(0.3f, 0.3f) + new Vector2(0f, -0.15f);
                int dustType = i % 2 == 0 ? DustID.PurpleTorch : DustID.PinkTorch;
                Dust d = Dust.NewDustPerfect(pos + offset, dustType, driftVel, 100, default, 0.65f);
                d.noGravity = true;
                d.fadeIn = 1.3f;
            }

            // Faint glow particle marking the residue
            var residue = new GenericGlowParticle(
                pos, Main.rand.NextVector2Circular(0.1f, 0.1f),
                NachtmusikPalette.Violet * 0.4f, 0.15f, 60, true);
            MagnumParticleHandler.SpawnParticle(residue);
        }

        // =====================================================================
        //  WhisperStormVFX — Massive convergence effect
        // =====================================================================
        public static void WhisperStormVFX(Vector2 targetPos)
        {
            // === MASSIVE INWARD-STREAMING CONVERGENCE ===
            for (int i = 0; i < 30; i++)
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float dist = 150f + Main.rand.NextFloat() * 100f;
                Vector2 spawnPos = targetPos + angle.ToRotationVector2() * dist;
                Vector2 vel = (targetPos - spawnPos).SafeNormalize(Vector2.Zero) * (5f + Main.rand.NextFloat() * 4f);

                int dustType;
                switch (i % 3)
                {
                    case 0: dustType = DustID.PurpleTorch; break;
                    case 1: dustType = DustID.BlueTorch; break;
                    default: dustType = DustID.PinkTorch; break;
                }

                Dust d = Dust.NewDustPerfect(spawnPos, dustType, vel, 40, default, 1.4f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            // Central convergence flash
            CustomParticles.GenericFlare(targetPos, NachtmusikPalette.Violet, 0.8f, 20);
            CustomParticles.GenericFlare(targetPos, NachtmusikPalette.CosmicPurple, 0.6f, 18);
            CustomParticles.GenericFlare(targetPos, Color.White, 0.5f, 16);

            // Halo rings expanding outward from center
            for (int ring = 0; ring < 3; ring++)
            {
                Color ringColor = NachtmusikPalette.PaletteLerp(NachtmusikPalette.NebulasWhisperShot,
                    ring / 3f);
                CustomParticles.HaloRing(targetPos, ringColor, 0.4f + ring * 0.15f, 16 + ring * 4);
            }

            // Music note cascade
            NachtmusikVFXLibrary.SpawnMusicNotes(targetPos, 5, 30f, 0.6f, 0.9f, 28);
            NachtmusikVFXLibrary.DrawBloom(targetPos, 0.6f, 0.9f);

            Lighting.AddLight(targetPos, NachtmusikPalette.Violet.ToVector3() * 1.0f);
        }
    }
}
