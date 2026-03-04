using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.Nachtmusik.Weapons.ConstellationPiercer.Utilities
{
    /// <summary>
    /// Static VFX helper for Constellation Piercer — precision celestial rifle.
    /// Cosmic blue directional muzzle flash, subtle crosshair precision aura,
    /// star point creation flash, and constellation line VFX between star points.
    /// </summary>
    public static class ConstellationPiercerVFX
    {
        // =====================================================================
        //  MuzzleFlashVFX — Cosmic blue directional burst
        // =====================================================================
        public static void MuzzleFlashVFX(Vector2 muzzlePos, Vector2 direction)
        {
            // === CONSTELLATION-POINT PRECISION FLASH ===
            NachtmusikVFXLibrary.SpawnStarBurst(muzzlePos, 5, 0.4f);

            // Precision directional cosmic blue sparks
            for (int i = 0; i < 6; i++)
            {
                float spread = MathHelper.ToRadians(8f);
                Vector2 vel = direction.RotatedBy(Main.rand.NextFloat(-spread, spread))
                    * (4f + Main.rand.NextFloat() * 3f);
                Dust d = Dust.NewDustPerfect(muzzlePos, DustID.BlueTorch, vel, 0, default, 0.9f);
                d.noGravity = true;
                d.fadeIn = 0.9f;
            }

            // Stellar spark accent — silver highlights
            for (int i = 0; i < 3; i++)
            {
                Vector2 vel = direction.RotatedBy(Main.rand.NextFloat(-0.15f, 0.15f)) * (2f + Main.rand.NextFloat() * 2f);
                Dust s = Dust.NewDustPerfect(muzzlePos, DustID.SilverCoin, vel, 0,
                    NachtmusikPalette.StarWhite, 0.7f);
                s.noGravity = true;
            }

            // Crosshair flash lines — 4 sharp dust lines outward from muzzle
            for (int i = 0; i < 4; i++)
            {
                float angle = direction.ToRotation() + MathHelper.PiOver2 * i;
                Vector2 lineDir = angle.ToRotationVector2();
                for (int j = 1; j <= 3; j++)
                {
                    Dust line = Dust.NewDustPerfect(muzzlePos + lineDir * (j * 6f),
                        DustID.BlueTorch, lineDir * 0.8f, 0, default, 0.5f - j * 0.1f);
                    line.noGravity = true;
                    line.fadeIn = 0.4f;
                }
            }

            // Gold accent flash
            Dust gold = Dust.NewDustPerfect(muzzlePos, DustID.Enchanted_Gold,
                direction * 1.5f, 0, default, 0.6f);
            gold.noGravity = true;

            // Music note on fire
            NachtmusikVFXLibrary.SpawnMusicNotes(muzzlePos, 1, 10f, 0.4f, 0.6f, 18);
            NachtmusikVFXLibrary.DrawBloom(muzzlePos, 0.35f, 0.75f);
            NachtmusikVFXLibrary.AddPaletteLighting(muzzlePos, 0.3f, 0.65f);
        }

        // =====================================================================
        //  HoldItemVFX — Cosmic precision aura
        // =====================================================================
        public static void HoldItemVFX(Player player)
        {
            float time = (float)Main.timeForVisualEffects * 0.05f;

            // === CONSTELLATION TARGETING RETICLE === 4 fixed star points in crosshair formation
            for (int i = 0; i < 4; i++)
            {
                float baseAngle = MathHelper.PiOver2 * i + time * 0.5f;
                float radius = 18f + (float)Math.Sin(time * 2f + i * MathHelper.PiOver2) * 4f;
                Vector2 starPos = player.Center + new Vector2(
                    (float)Math.Cos(baseAngle) * radius,
                    (float)Math.Sin(baseAngle) * radius);

                if (Main.rand.NextBool(3))
                {
                    Dust d = Dust.NewDustPerfect(starPos, DustID.BlueTorch,
                        Vector2.Zero, 0, default, 0.4f);
                    d.noGravity = true;
                    d.fadeIn = 0.5f;
                }
            }

            // Occasional gold center flash — crosshair lock accent
            if (Main.rand.NextBool(6))
            {
                Dust gold = Dust.NewDustPerfect(player.Center + Main.rand.NextVector2Circular(5f, 5f),
                    DustID.Enchanted_Gold, Vector2.Zero, 0, default, 0.35f);
                gold.noGravity = true;
                gold.fadeIn = 0.5f;
            }

            // Occasional twinkling star
            if (Main.rand.NextBool(10))
            {
                NachtmusikVFXLibrary.SpawnTwinklingStars(player.Center, 1, 22f);
            }

            // Cosmic blue light
            Lighting.AddLight(player.Center, NachtmusikPalette.ConstellationBlue.ToVector3() * 0.2f);
        }

        // =====================================================================
        //  StarPointCreationVFX — Small star flash when enemy becomes Star Point
        // =====================================================================
        public static void StarPointCreationVFX(Vector2 pos)
        {
            // Sharp 4-point flash
            NachtmusikVFXLibrary.SpawnStarBurst(pos, 4, 0.3f);

            // Quick constellation blue burst
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f + Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 vel = angle.ToRotationVector2() * (2f + Main.rand.NextFloat() * 1.5f);
                Dust d = Dust.NewDustPerfect(pos, DustID.BlueTorch, vel, 0, default, 0.65f);
                d.noGravity = true;
                d.fadeIn = 0.7f;
            }

            // Star white center ping
            CustomParticles.GenericFlare(pos, NachtmusikPalette.StarWhite, 0.3f, 12);
            CustomParticles.GenericFlare(pos, NachtmusikPalette.ConstellationBlue, 0.4f, 14);

            Lighting.AddLight(pos, NachtmusikPalette.StarlightCore.ToVector3() * 0.5f);
        }

        // =====================================================================
        //  ConstellationLineVFX — Brief luminous line between star points
        // =====================================================================
        public static void ConstellationLineVFX(Vector2 from, Vector2 to)
        {
            float dist = Vector2.Distance(from, to);
            int steps = Math.Max(6, (int)(dist / 10f));
            float totalDist = dist;

            for (int i = 0; i <= steps; i++)
            {
                float t = i / (float)steps;
                Vector2 pos = Vector2.Lerp(from, to, t);

                // Alternating blue and silver line particles
                Color lineColor = t % 0.2f < 0.1f
                    ? NachtmusikPalette.ConstellationBlue
                    : NachtmusikPalette.StarWhite;

                var glow = new GenericGlowParticle(
                    pos + Main.rand.NextVector2Circular(2f, 2f),
                    Vector2.Zero,
                    lineColor * 0.7f, 0.18f, 20, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // Bright node at each endpoint
            CustomParticles.GenericFlare(from, NachtmusikPalette.StarlightCore, 0.25f, 16);
            CustomParticles.GenericFlare(to, NachtmusikPalette.StarlightCore, 0.25f, 16);

            // Midpoint accent
            Vector2 mid = (from + to) * 0.5f;
            NachtmusikVFXLibrary.SpawnTwinklingStars(mid, 1, 8f);
        }
    }
}
