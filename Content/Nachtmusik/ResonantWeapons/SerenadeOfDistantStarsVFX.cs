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
    /// VFX helper for the Serenade of Distant Stars ranged weapon.
    /// Warm starlit melody flowing through the night sky — sweeping comet trails,
    /// warm silver shimmer, romantic starlight cascades, and gentle moonlit accents.
    /// Each shot is a note in the night's eternal song.
    /// </summary>
    public static class SerenadeOfDistantStarsVFX
    {
        // =====================================================================
        //  HOLD ITEM VFX
        // =====================================================================

        public static void HoldItemVFX(Player player)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Warm serenade aura
            if (Main.rand.NextBool(6))
            {
                Vector2 offset = Main.rand.NextVector2Circular(22f, 22f);
                Color serenadeColor = NachtmusikPalette.GetRadianceShimmer(time + Main.rand.NextFloat(3f));
                var glow = new GenericGlowParticle(center + offset,
                    new Vector2(0, -0.2f) + Main.rand.NextVector2Circular(0.3f, 0.3f),
                    serenadeColor * 0.3f, 0.16f, 20, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // Distant star twinkle
            if (Main.rand.NextBool(10))
            {
                Vector2 starPos = center + Main.rand.NextVector2Circular(30f, 30f);
                Color twinkle = NachtmusikPalette.GetStarlitShimmer(time + Main.rand.NextFloat(5f));
                try { CustomParticles.GenericFlare(starPos, twinkle * 0.35f, 0.12f, 10); } catch { }
            }

            NachtmusikVFXLibrary.AddRadianceLight(center, time, 0.2f);
        }

        // =====================================================================
        //  PREDRAW IN WORLD BLOOM
        // =====================================================================

        public static void PreDrawInWorldBloom(SpriteBatch sb, Texture2D tex,
            Vector2 pos, Vector2 origin, float rotation, float scale)
        {
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.04f) * 0.03f;
            NachtmusikPalette.DrawItemBloom(sb, tex, pos, origin, rotation, scale, pulse);
        }

        // =====================================================================
        //  MUZZLE FLASH VFX
        // =====================================================================

        public static void MuzzleFlashVFX(Vector2 muzzlePos, Vector2 direction)
        {
            if (Main.dedServ) return;

            // Warm starlit flash
            try { CustomParticles.GenericFlare(muzzlePos, NachtmusikPalette.StarWhite, 0.55f, 10); } catch { }
            try { CustomParticles.GenericFlare(muzzlePos, NachtmusikPalette.StarlitBlue * 0.7f, 0.4f, 8); } catch { }

            // Sweeping comet sparks
            for (int i = 0; i < 5; i++)
            {
                float spread = Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 vel = direction.RotatedBy(spread) * Main.rand.NextFloat(3f, 6f);
                Color sparkColor = NachtmusikPalette.GetCelestialGradient((float)i / 5f);
                var spark = new GlowSparkParticle(muzzlePos, vel, sparkColor, 0.2f, 14);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Serenade music notes
            NachtmusikVFXLibrary.SpawnMusicNotes(muzzlePos, 2, 10f, 0.7f, 0.9f, 22);

            Lighting.AddLight(muzzlePos, NachtmusikPalette.StarWhite.ToVector3() * 0.6f);
        }

        // =====================================================================
        //  PROJECTILE TRAIL VFX
        // =====================================================================

        public static void ProjectileTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 away = -velocity.SafeNormalize(Vector2.Zero);

            // Warm comet trail — radiant beam with starlit shimmer
            NachtmusikVFXLibrary.SpawnRadiantBeamTrail(pos, velocity, 1f);

            // Silver moonlit dust
            Dust d = Dust.NewDustPerfect(pos, DustID.BlueTorch, away * 0.8f + Main.rand.NextVector2Circular(0.4f, 0.4f),
                0, NachtmusikPalette.MoonlitSilver, 1.3f);
            d.noGravity = true;

            // Warm serenade shimmer (1-in-3)
            if (Main.rand.NextBool(3))
            {
                Color shimmerColor = NachtmusikPalette.GetRadianceShimmer(Main.GameUpdateCount);
                try { CustomParticles.GenericFlare(pos + Main.rand.NextVector2Circular(5f, 5f),
                    shimmerColor * 0.4f, 0.18f, 12); } catch { }
            }

            // Music note melody (1-in-6)
            if (Main.rand.NextBool(6))
                NachtmusikVFXLibrary.SpawnMusicNotes(pos, 1, 6f, 0.7f, 0.85f, 22);

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
            NachtmusikVFXLibrary.SpawnMusicNotes(hitPos, 3, 18f, 0.7f, 1.0f, 25);

            // Serenade warm sparkle cascade
            for (int i = 0; i < 4; i++)
            {
                Color warmColor = NachtmusikPalette.GetCelestialGradient((float)i / 4f);
                try { CustomParticles.GenericFlare(hitPos + Main.rand.NextVector2Circular(12f, 12f),
                    warmColor, 0.3f, 16); } catch { }
            }

            Lighting.AddLight(hitPos, NachtmusikPalette.StarWhite.ToVector3() * 0.6f);
        }

        // =====================================================================
        //  DEATH VFX
        // =====================================================================

        public static void ProjectileDeathVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            NachtmusikVFXLibrary.SpawnRadialDustBurst(pos, 6, 3f);
            NachtmusikVFXLibrary.SpawnTwinklingStars(pos, 3, 10f);
            NachtmusikVFXLibrary.SpawnShatteredStarlight(pos, 4, 4f, 0.6f, false);
            try { CustomParticles.HaloRing(pos, NachtmusikPalette.StarlitBlue, 0.2f, 10); } catch { }
        }
    }
}
