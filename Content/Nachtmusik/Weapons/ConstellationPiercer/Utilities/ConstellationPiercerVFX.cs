using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX.Bloom;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Common.Systems.VFX.Effects;
using MagnumOpus.Common.Systems.VFX.Screen;
using MagnumOpus.Content.Nachtmusik;
using MagnumOpus.Content.Nachtmusik.Weapons.ConstellationPiercer.Systems;

namespace MagnumOpus.Content.Nachtmusik.Weapons.ConstellationPiercer.Utilities
{
    /// <summary>
    /// Static VFX helper for Constellation Piercer.
    /// Provides muzzle flash, hold-item ambient, and starfall VFX.
    /// </summary>
    public static class ConstellationPiercerVFX
    {
        /// <summary>
        /// Bloom pulse at barrel + expanding ring + forward sparkle motes.
        /// Called every shot in Shoot().
        /// </summary>
        public static void MuzzleFlashVFX(Vector2 muzzlePos, Vector2 direction)
        {
            // 1. Bright bloom pulse at barrel tip
            BloomRenderer.DrawBloomStackAdditive(muzzlePos,
                NachtmusikPalette.StarWhite, 0.5f, 0.6f);

            // 2. Expanding ring pulse from barrel
            GlowDustSystem.SpawnCirclePulse(muzzlePos,
                NachtmusikPalette.ConstellationBlue, 30f, 15, 2f);

            // 3. Forward-directional sparkle motes (6 in a cone)
            Vector2 perp = direction.RotatedBy(MathHelper.PiOver2);
            for (int i = 0; i < 6; i++)
            {
                float spread = Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 vel = direction * Main.rand.NextFloat(3f, 7f) + perp * spread * 3f;
                Color col = (i % 2 == 0)
                    ? NachtmusikPalette.ConstellationBlue
                    : NachtmusikPalette.StarWhite;

                GlowDustSystem.SpawnGlowPixelFast(muzzlePos, vel, col, 0.6f, 12);
            }

            // 4. Single gold accent flare
            CustomParticles.GenericFlare(muzzlePos,
                NachtmusikPalette.StarGold, 0.3f, 10);

            // 5. Brief constellation blue lighting
            Lighting.AddLight(muzzlePos, NachtmusikPalette.ConstellationBlue.ToVector3() * 0.6f);
        }

        /// <summary>
        /// Music notes + tiny stars drifting upward from weapon.
        /// Called every frame in HoldItem().
        /// </summary>
        public static void HoldItemVFX(Player player)
        {
            Vector2 weaponPos = player.MountedCenter + new Vector2(player.direction * 20f, -4f);

            // Occasional music note drifting upward (real MusicNoteParticle with wobble + bloom)
            if (Main.rand.NextBool(12))
            {
                Vector2 noteVel = new(Main.rand.NextFloat(-0.8f, 0.8f), Main.rand.NextFloat(-1.5f, -0.5f));
                Color noteCol = Color.Lerp(NachtmusikPalette.StarGold, NachtmusikPalette.ConstellationBlue,
                    Main.rand.NextFloat());
                MagnumParticleHandler.SpawnParticle(new MusicNoteParticle(
                    weaponPos, noteVel, noteCol, Main.rand.NextFloat(0.2f, 0.35f), 40));
            }

            // Occasional tiny star sparkle
            if (Main.rand.NextBool(8))
            {
                Vector2 offset = new(Main.rand.NextFloat(-10f, 10f), Main.rand.NextFloat(-8f, 8f));
                Vector2 vel = new(0f, Main.rand.NextFloat(-0.5f, -0.2f));
                Color col = Color.Lerp(NachtmusikPalette.StarWhite, NachtmusikPalette.ConstellationBlue,
                    Main.rand.NextFloat());

                GlowDustSystem.SpawnGlowPixelRise(weaponPos + offset, vel, col, 0.3f, 30);
            }

            // Subtle ambient light
            float pulse = 0.7f + 0.3f * MathF.Sin(Main.GlobalTimeWrappedHourly * 3f);
            Lighting.AddLight(weaponPos, NachtmusikPalette.ConstellationBlue.ToVector3() * 0.2f * pulse);
        }

        /// <summary>
        /// Vertical light beam from offscreen + radial sparkle burst on impact.
        /// Called every 5th shot -- strikes a random Star Point.
        /// </summary>
        public static void StarfallVFX(Vector2 targetPos)
        {
            // 1. Light pillar visual: vertical line of glow dust descending
            for (int i = 0; i < 15; i++)
            {
                float t = (float)i / 15f;
                Vector2 spawnPos = targetPos + new Vector2(Main.rand.NextFloat(-3f, 3f), -400f * (1f - t));
                Vector2 vel = new(0f, 8f + Main.rand.NextFloat(0f, 4f));
                Color col = Color.Lerp(NachtmusikPalette.StarWhite, NachtmusikPalette.ConstellationBlue, t);

                GlowDustSystem.SpawnGlowPixelFast(spawnPos, vel, col, 0.7f * (1f - t * 0.5f), 15);
            }

            // 2. Radial sparkle burst at impact point
            GlowDustSystem.SpawnGlowBurst(targetPos,
                NachtmusikPalette.StarWhite, 12, 5f, 0.9f, 20);

            // 3. Expanding ring
            GlowDustSystem.SpawnCirclePulse(targetPos,
                NachtmusikPalette.StarGold, 55f, 22, 3f);

            // 4. Bloom flash at impact
            BloomRenderer.DrawBloomStackAdditive(targetPos,
                NachtmusikPalette.StarGold, 0.7f, 0.8f);

            // 5. Star ignition sparkles
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                Color col = Color.Lerp(NachtmusikPalette.StarGold, NachtmusikPalette.TwinklingWhite,
                    Main.rand.NextFloat());

                MagnumParticleHandler.SpawnParticle(new SparkleParticle(
                    targetPos, vel, col, Main.rand.NextFloat(0.5f, 1f), Main.rand.Next(18, 30)));
            }

            // 6. Screen shake for impact
            ScreenEffectSystem.AddScreenShake(3f);

            // 7. Light flash
            Lighting.AddLight(targetPos, NachtmusikPalette.StarGold.ToVector3() * 0.8f);
        }

    }
}
