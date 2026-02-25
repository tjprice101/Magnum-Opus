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
    /// VFX helper for the Nebula's Whisper ranged weapon.
    /// Soft cosmic whispers through nebula mist — gentle purple-pink trails,
    /// ethereal cloud particles, whispering glow dissolves, and delicate
    /// cosmic sparkle accents. Each shot is a whispered secret of the cosmos.
    /// </summary>
    public static class NebulasWhisperVFX
    {
        // =====================================================================
        //  HOLD ITEM VFX
        // =====================================================================

        public static void HoldItemVFX(Player player)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Ethereal nebula mist
            if (Main.rand.NextBool(6))
            {
                Vector2 offset = Main.rand.NextVector2Circular(22f, 22f);
                Color mistColor = Color.Lerp(NachtmusikPalette.CosmicPurple, NachtmusikPalette.NebulaPink,
                    Main.rand.NextFloat());
                var cloud = new GenericGlowParticle(center + offset,
                    Main.rand.NextVector2Circular(0.3f, 0.3f),
                    mistColor * 0.3f, 0.2f, 22, true);
                MagnumParticleHandler.SpawnParticle(cloud);
            }

            // Gentle serenade shimmer
            if (Main.rand.NextBool(15))
            {
                try { CustomParticles.GenericFlare(center + Main.rand.NextVector2Circular(18f, 18f),
                    NachtmusikPalette.SerenadeGlow * 0.3f, 0.12f, 12); } catch { }
            }

            NachtmusikVFXLibrary.AddNachtmusikLight(center, 0.15f);
        }

        // =====================================================================
        //  PREDRAW IN WORLD BLOOM
        // =====================================================================

        public static void PreDrawInWorldBloom(SpriteBatch sb, Texture2D tex,
            Vector2 pos, Vector2 origin, float rotation, float scale)
        {
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.035f) * 0.03f;
            NachtmusikPalette.DrawItemBloom(sb, tex, pos, origin, rotation, scale, pulse);
        }

        // =====================================================================
        //  MUZZLE FLASH VFX
        // =====================================================================

        public static void MuzzleFlashVFX(Vector2 muzzlePos, Vector2 direction)
        {
            if (Main.dedServ) return;

            // Soft nebula puff
            try { CustomParticles.GenericFlare(muzzlePos, NachtmusikPalette.NebulaPink * 0.7f, 0.5f, 12); } catch { }
            try { CustomParticles.GenericFlare(muzzlePos, NachtmusikPalette.SerenadeGlow * 0.5f, 0.35f, 10); } catch { }

            // Gentle directional wisps
            for (int i = 0; i < 4; i++)
            {
                float spread = Main.rand.NextFloat(-0.4f, 0.4f);
                Vector2 vel = direction.RotatedBy(spread) * Main.rand.NextFloat(2f, 5f);
                Color wispColor = Color.Lerp(NachtmusikPalette.CosmicPurple, NachtmusikPalette.NebulaPink,
                    Main.rand.NextFloat());
                var wisp = new GenericGlowParticle(muzzlePos, vel, wispColor * 0.6f, 0.2f, 16, true);
                MagnumParticleHandler.SpawnParticle(wisp);
            }

            // Whisper music note
            NachtmusikVFXLibrary.SpawnMusicNotes(muzzlePos, 1, 8f, 0.7f, 0.85f, 22);

            Lighting.AddLight(muzzlePos, NachtmusikPalette.NebulaPink.ToVector3() * 0.5f);
        }

        // =====================================================================
        //  PROJECTILE TRAIL VFX
        // =====================================================================

        public static void ProjectileTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            // Nebula cloud trail — soft, ethereal
            NachtmusikVFXLibrary.SpawnCloudTrail(pos, velocity, 0.8f);

            // Purple-pink wisping dust
            if (Main.rand.NextBool(2))
            {
                Vector2 vel = Main.rand.NextVector2Circular(1f, 1f);
                Color dustColor = Color.Lerp(NachtmusikPalette.CosmicPurple, NachtmusikPalette.NebulaPink,
                    Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.PurpleTorch, vel, 0, dustColor, 1.1f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            // Serenade glow sparkle (1-in-5)
            if (Main.rand.NextBool(5))
            {
                try { CustomParticles.GenericFlare(pos + Main.rand.NextVector2Circular(6f, 6f),
                    NachtmusikPalette.SerenadeGlow * 0.5f, 0.18f, 12); } catch { }
            }

            // Music note accent (1-in-8)
            if (Main.rand.NextBool(8))
                NachtmusikVFXLibrary.SpawnMusicNotes(pos, 1, 6f, 0.7f, 0.85f, 20);

            NachtmusikVFXLibrary.AddNachtmusikLight(pos, 0.25f);
        }

        // =====================================================================
        //  HIT VFX
        // =====================================================================

        public static void SmallHitVFX(Vector2 hitPos)
        {
            if (Main.dedServ) return;

            // Nebula puff impact
            for (int i = 0; i < 5; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                Color puffColor = Color.Lerp(NachtmusikPalette.CosmicPurple, NachtmusikPalette.NebulaPink, Main.rand.NextFloat());
                var puff = new GenericGlowParticle(hitPos + Main.rand.NextVector2Circular(8f, 8f),
                    vel, puffColor * 0.5f, 0.25f, 18, true);
                MagnumParticleHandler.SpawnParticle(puff);
            }

            NachtmusikVFXLibrary.SpawnGradientHaloRings(hitPos, 2, 0.2f);
            NachtmusikVFXLibrary.SpawnTwinklingStars(hitPos, 3, 12f);
            NachtmusikVFXLibrary.SpawnMusicNotes(hitPos, 2, 12f, 0.7f, 0.9f, 22);

            Lighting.AddLight(hitPos, NachtmusikPalette.NebulaPink.ToVector3() * 0.5f);
        }

        // =====================================================================
        //  DEATH VFX
        // =====================================================================

        public static void ProjectileDeathVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            // Nebula mist dissolve
            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);
                Color cloudColor = Color.Lerp(NachtmusikPalette.CosmicPurple, NachtmusikPalette.NebulaPink, Main.rand.NextFloat());
                var cloud = new GenericGlowParticle(pos + Main.rand.NextVector2Circular(8f, 8f),
                    vel, cloudColor * 0.4f, 0.2f, 16, true);
                MagnumParticleHandler.SpawnParticle(cloud);
            }
            try { CustomParticles.HaloRing(pos, NachtmusikPalette.NebulaPink * 0.5f, 0.18f, 10); } catch { }
        }
    }
}
