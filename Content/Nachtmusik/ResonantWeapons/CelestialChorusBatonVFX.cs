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
    /// VFX helper for the Celestial Chorus Baton summon weapon.
    /// Choral harmony of nocturnal guardians — deep blue guardian aura,
    /// harmonic starlit sparkles, chorale music note cascades, celestial
    /// resonance rings. The conductor's baton calls forth a chorus of stars.
    /// </summary>
    public static class CelestialChorusBatonVFX
    {
        // =====================================================================
        //  HOLD ITEM VFX
        // =====================================================================

        public static void HoldItemVFX(Player player, int minionCount)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Choral resonance aura — scales with minion count
            if (Main.rand.NextBool(6))
            {
                float pulse = MathF.Sin(time * 0.04f + minionCount * 0.3f) * 0.5f + 0.5f;
                Vector2 offset = Main.rand.NextVector2Circular(22f, 22f);
                Color auraColor = Color.Lerp(NachtmusikPalette.DeepBlue, NachtmusikPalette.StarlitBlue, pulse);
                var glow = new GenericGlowParticle(center + offset, Main.rand.NextVector2Circular(0.4f, 0.4f),
                    auraColor * 0.35f, 0.16f, 20, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // Harmonic star motes — more with more minions
            if (Main.rand.NextBool(25 - Math.Min(minionCount * 3, 15)))
            {
                Vector2 notePos = center + Main.rand.NextVector2Circular(30f, 30f);
                Color noteColor = NachtmusikPalette.GetCelestialGradient(Main.rand.NextFloat());
                NachtmusikVFXLibrary.SpawnMusicNotes(notePos, 1, 8f, 0.7f, 0.85f, 25);
            }

            // Conductor's pulse light
            float intensity = 0.2f + minionCount * 0.04f;
            float lightPulse = 0.3f + MathF.Sin(time * 0.05f) * 0.1f;
            Lighting.AddLight(center, NachtmusikPalette.DeepBlue.ToVector3() * lightPulse * intensity);
        }

        // =====================================================================
        //  PREDRAW IN WORLD BLOOM
        // =====================================================================

        public static void PreDrawInWorldBloom(SpriteBatch sb, Texture2D tex,
            Vector2 pos, Vector2 origin, float rotation, float scale)
        {
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.04f) * 0.06f;
            NachtmusikPalette.DrawItemBloom(sb, tex, pos, origin, rotation, scale, pulse);
        }

        // =====================================================================
        //  SUMMON VFX
        // =====================================================================

        /// <summary>
        /// One-shot VFX when the Nocturnal Guardian is summoned.
        /// Celestial chorus flash, harmonic ring, starburst, music notes.
        /// </summary>
        public static void SummonVFX(Vector2 spawnPos)
        {
            if (Main.dedServ) return;

            // Summoning flash — celestial blue-white
            try { CustomParticles.GenericFlare(spawnPos, NachtmusikPalette.StarWhite, 0.7f, 18); } catch { }
            try { CustomParticles.GenericFlare(spawnPos, NachtmusikPalette.StarlitBlue, 0.5f, 16); } catch { }

            // Harmonic halo rings
            NachtmusikVFXLibrary.SpawnGradientHaloRings(spawnPos, 4, 0.3f);

            // Starburst cascade — the guardian emerges
            NachtmusikVFXLibrary.SpawnStarburstCascade(spawnPos, 8, 5f, 0.3f);

            // Celestial dust burst
            NachtmusikVFXLibrary.SpawnRadialDustBurst(spawnPos, 10, 4f);

            // Chorus music notes — the baton's call
            NachtmusikVFXLibrary.SpawnMusicNotes(spawnPos, 4, 25f, 0.8f, 1.0f, 30);

            // Bloom flash
            NachtmusikVFXLibrary.DrawBloom(spawnPos, 0.5f);

            Lighting.AddLight(spawnPos, NachtmusikPalette.StarWhite.ToVector3() * 0.8f);
        }

        // =====================================================================
        //  MINION AMBIENT VFX
        // =====================================================================

        /// <summary>
        /// Per-frame guardian ambient: deep blue celestial aura, orbiting
        /// starlit dust, twinkling sparkles, music note wisps.
        /// </summary>
        public static void MinionAmbientVFX(Vector2 pos, float visibility)
        {
            if (Main.dedServ) return;

            // Guardian aura dust (2 per frame)
            for (int d = 0; d < 2; d++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(15f, 15f);
                Color col = NachtmusikPalette.GetCelestialGradient(Main.rand.NextFloat(0.2f, 0.7f));
                Dust dust = Dust.NewDustPerfect(pos + offset, DustID.BlueTorch,
                    Main.rand.NextVector2Circular(1f, 1f), 0, col, (1.1f + Main.rand.NextFloat(0.3f)) * visibility);
                dust.noGravity = true;
                dust.fadeIn = 1.3f;
            }

            // Starlit sparkle accent (1-in-3)
            if (Main.rand.NextBool(3) && visibility > 0.5f)
            {
                var sparkle = new SparkleParticle(pos + Main.rand.NextVector2Circular(18f, 18f),
                    Main.rand.NextVector2Circular(1f, 1f),
                    NachtmusikPalette.StarWhite * visibility, 0.35f, 18);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // Celestial shimmer glow (1-in-4)
            if (Main.rand.NextBool(4) && visibility > 0.5f)
            {
                Color shimmer = NachtmusikPalette.GetStarlitShimmer();
                var glow = new GenericGlowParticle(pos + Main.rand.NextVector2Circular(12f, 12f),
                    Main.rand.NextVector2Circular(0.8f, 0.8f),
                    shimmer * visibility * 0.7f, 0.3f, 16, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // Orbiting star motes (periodic)
            if (Main.GameUpdateCount % 15 == 0)
            {
                float baseAngle = Main.GameUpdateCount * 0.04f;
                for (int i = 0; i < 3; i++)
                {
                    float angle = baseAngle + MathHelper.TwoPi * i / 3f;
                    Vector2 motePos = pos + angle.ToRotationVector2() * 25f;
                    try { CustomParticles.GenericFlare(motePos,
                        NachtmusikPalette.GetCelestialGradient((float)i / 3f) * visibility, 0.3f * visibility, 12); } catch { }
                }
            }

            // Music note wisps (1-in-8)
            if (Main.rand.NextBool(8) && visibility > 0.5f)
                NachtmusikVFXLibrary.SpawnMusicNotes(pos, 1, 12f, 0.7f, 0.85f, 22);

            // Guardian light
            float pulse = 0.35f + MathF.Sin((float)Main.timeForVisualEffects * 0.1f) * 0.08f;
            Lighting.AddLight(pos, NachtmusikPalette.StarlitBlue.ToVector3() * pulse * visibility);
        }

        // =====================================================================
        //  MINION ATTACK VFX
        // =====================================================================

        /// <summary>
        /// Guardian dash attack launch: celestial flash, directional
        /// star trail sparks, harmonic burst, music notes.
        /// </summary>
        public static void MinionAttackVFX(Vector2 minionPos, Vector2 direction)
        {
            if (Main.dedServ) return;

            // Attack flash — starlit blue
            try { CustomParticles.GenericFlare(minionPos, NachtmusikPalette.StarlitBlue, 0.6f, 14); } catch { }
            try { CustomParticles.HaloRing(minionPos, NachtmusikPalette.DeepBlue, 0.35f, 12); } catch { }

            // Directional star trail
            for (int i = 0; i < 5; i++)
            {
                float spread = Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 vel = direction.RotatedBy(spread) * Main.rand.NextFloat(3f, 6f);
                Color sparkColor = NachtmusikPalette.GetCelestialGradient((float)i / 5f);
                var spark = new GlowSparkParticle(minionPos, vel, sparkColor, 0.25f, 14);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Music notes — the chorus strikes
            NachtmusikVFXLibrary.SpawnMusicNotes(minionPos, 2, 15f, 0.7f, 0.9f, 22);

            // Twinkling stars
            NachtmusikVFXLibrary.SpawnTwinklingStars(minionPos, 2, 10f);

            Lighting.AddLight(minionPos, NachtmusikPalette.StarlitBlue.ToVector3() * 0.6f);
        }

        // =====================================================================
        //  MINION IMPACT VFX
        // =====================================================================

        /// <summary>
        /// Guardian on-hit: celestial impact, gradient halos,
        /// star dust burst, harmonic sparkle ring, music notes.
        /// </summary>
        public static void MinionImpactVFX(Vector2 hitPos)
        {
            if (Main.dedServ) return;

            NachtmusikVFXLibrary.SpawnGradientHaloRings(hitPos, 3, 0.25f);
            NachtmusikVFXLibrary.SpawnRadialDustBurst(hitPos, 8, 4f);
            NachtmusikVFXLibrary.SpawnTwinklingStars(hitPos, 3, 12f);
            NachtmusikVFXLibrary.SpawnMusicNotes(hitPos, 2, 15f, 0.7f, 0.9f, 25);

            // Harmonic sparkle ring
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                Color col = NachtmusikPalette.GetCelestialGradient((float)i / 6f);
                var spark = new GlowSparkParticle(hitPos, vel, col, 0.22f, 14);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            Lighting.AddLight(hitPos, NachtmusikPalette.StarWhite.ToVector3() * 0.6f);
        }

        // =====================================================================
        //  DESPAWN VFX
        // =====================================================================

        public static void DespawnVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            NachtmusikVFXLibrary.SpawnRadialDustBurst(pos, 6, 3f);
            NachtmusikVFXLibrary.SpawnTwinklingStars(pos, 3, 10f);
            try { CustomParticles.HaloRing(pos, NachtmusikPalette.StarlitBlue * 0.5f, 0.2f, 10); } catch { }
        }
    }
}
