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
    /// VFX helper for the Constellation Piercer ranged weapon.
    /// Pinpoint starfield precision — each shot draws a constellation line,
    /// with clean starlit trails, sharp golden star-point impacts, and
    /// constellation web patterns on hit.
    /// </summary>
    public static class ConstellationPiercerVFX
    {
        // =====================================================================
        //  HOLD ITEM VFX
        // =====================================================================

        public static void HoldItemVFX(Player player)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Constellation point aura
            if (Main.rand.NextBool(8))
            {
                float angle = time * 0.04f + Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 starPos = center + angle.ToRotationVector2() * Main.rand.NextFloat(15f, 25f);
                try { CustomParticles.GenericFlare(starPos, NachtmusikPalette.StarWhite * 0.4f, 0.15f, 12); } catch { }
            }

            // Mini constellation lines
            if (Main.rand.NextBool(25))
            {
                Vector2 start = center + Main.rand.NextVector2Circular(18f, 18f);
                Vector2 end = start + Main.rand.NextVector2Circular(15f, 15f);
                NachtmusikVFXLibrary.SpawnConstellationLine(start, end, 3);
            }

            NachtmusikVFXLibrary.AddTwinklingLight(center, time, 0.2f);
        }

        // =====================================================================
        //  PREDRAW IN WORLD BLOOM
        // =====================================================================

        public static void PreDrawInWorldBloom(SpriteBatch sb, Texture2D tex,
            Vector2 pos, Vector2 origin, float rotation, float scale)
        {
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.05f) * 0.02f;
            NachtmusikPalette.DrawItemBloom(sb, tex, pos, origin, rotation, scale, pulse);
        }

        // =====================================================================
        //  MUZZLE FLASH VFX
        // =====================================================================

        public static void MuzzleFlashVFX(Vector2 muzzlePos, Vector2 direction)
        {
            if (Main.dedServ) return;

            // Star-point flash
            try { CustomParticles.GenericFlare(muzzlePos, NachtmusikPalette.StarWhite, 0.6f, 10); } catch { }
            try { CustomParticles.GenericFlare(muzzlePos, NachtmusikPalette.ConstellationBlue, 0.4f, 8); } catch { }

            // Directed constellation sparks
            for (int i = 0; i < 6; i++)
            {
                float spread = Main.rand.NextFloat(-0.25f, 0.25f);
                Vector2 vel = direction.RotatedBy(spread) * Main.rand.NextFloat(3f, 7f);
                Color sparkColor = NachtmusikPalette.GetStarfieldGradient((float)i / 6f);
                var spark = new GlowSparkParticle(muzzlePos, vel, sparkColor, 0.22f, 12);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Star gold accent
            NachtmusikVFXLibrary.SpawnTwinklingStars(muzzlePos, 2, 8f);

            // Music note
            NachtmusikVFXLibrary.SpawnMusicNotes(muzzlePos, 1, 8f, 0.7f, 0.85f, 20);

            Lighting.AddLight(muzzlePos, NachtmusikPalette.StarWhite.ToVector3() * 0.7f);
        }

        // =====================================================================
        //  PROJECTILE TRAIL VFX
        // =====================================================================

        public static void ProjectileTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 away = -velocity.SafeNormalize(Vector2.Zero);

            // Clean constellation trail
            NachtmusikVFXLibrary.SpawnRadiantBeamTrail(pos, velocity, 0.8f);

            // Precision starlit dust
            Dust d = Dust.NewDustPerfect(pos, DustID.BlueTorch, away * 0.5f + Main.rand.NextVector2Circular(0.3f, 0.3f),
                0, NachtmusikPalette.ConstellationBlue, 1.2f);
            d.noGravity = true;

            // Star point sparkle along trail (1-in-4)
            if (Main.rand.NextBool(4))
            {
                try { CustomParticles.GenericFlare(pos + Main.rand.NextVector2Circular(4f, 4f),
                    NachtmusikPalette.StarGold, 0.2f, 10); } catch { }
            }

            // Constellation accent (1-in-8)
            if (Main.rand.NextBool(8))
                NachtmusikVFXLibrary.SpawnMusicNotes(pos, 1, 6f, 0.7f, 0.85f, 20);

            NachtmusikVFXLibrary.AddNachtmusikLight(pos, 0.35f);
        }

        // =====================================================================
        //  HIT VFX
        // =====================================================================

        public static void SmallHitVFX(Vector2 hitPos)
        {
            if (Main.dedServ) return;

            NachtmusikVFXLibrary.SpawnTwinklingStars(hitPos, 4, 15f);
            NachtmusikVFXLibrary.SpawnGradientHaloRings(hitPos, 3, 0.2f);
            NachtmusikVFXLibrary.SpawnRadialDustBurst(hitPos, 8, 4f);
            NachtmusikVFXLibrary.SpawnStarBurst(hitPos, 6, 0.25f);
            NachtmusikVFXLibrary.SpawnMusicNotes(hitPos, 2, 15f, 0.7f, 0.9f, 22);

            // Constellation web at impact
            float rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            NachtmusikVFXLibrary.SpawnConstellationCircle(hitPos, 20f, 5, rotation);

            Lighting.AddLight(hitPos, NachtmusikPalette.StarGold.ToVector3() * 0.6f);
        }

        // =====================================================================
        //  DEATH VFX
        // =====================================================================

        public static void ProjectileDeathVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            NachtmusikVFXLibrary.SpawnRadialDustBurst(pos, 6, 3f);
            NachtmusikVFXLibrary.SpawnTwinklingStars(pos, 3, 10f);
            try { CustomParticles.HaloRing(pos, NachtmusikPalette.ConstellationBlue, 0.2f, 10); } catch { }
        }
    }
}
