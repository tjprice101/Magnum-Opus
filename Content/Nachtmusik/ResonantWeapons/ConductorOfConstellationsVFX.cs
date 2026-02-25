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
    /// VFX helper for the Conductor of Constellations summon weapon.
    /// The supreme cosmic conductor — commanding constellation authority,
    /// orbiting star formations, baton-strike constellation flashes,
    /// grand stellar orchestration. The ultimate summon weapon of Nachtmusik.
    /// </summary>
    public static class ConductorOfConstellationsVFX
    {
        // =====================================================================
        //  HOLD ITEM VFX
        // =====================================================================

        public static void HoldItemVFX(Player player, int minionCount)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Grand conductor's aura — orbiting constellation particles
            if (Main.rand.NextBool(4))
            {
                float orbitAngle = time * 0.03f + Main.rand.NextFloat(MathHelper.TwoPi);
                float radius = 25f + MathF.Sin(time * 0.02f) * 5f;
                Vector2 orbitPos = center + orbitAngle.ToRotationVector2() * radius;
                Color auraColor = NachtmusikPalette.GetStarfieldGradient(Main.rand.NextFloat());
                var glow = new GenericGlowParticle(orbitPos,
                    (center - orbitPos).SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2) * 0.5f,
                    auraColor * 0.4f, 0.18f, 20, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // Orbiting glyphs — constellation authority
            if (Main.rand.NextBool(18))
            {
                float glyphAngle = time * 0.025f;
                Vector2 glyphPos = center + glyphAngle.ToRotationVector2() * 30f;
                try { CustomParticles.Glyph(glyphPos, NachtmusikPalette.CosmicPurple * 0.4f, 0.25f, -1); } catch { }
            }

            // Mini constellation patterns
            if (Main.rand.NextBool(30))
            {
                Vector2 start = center + Main.rand.NextVector2Circular(25f, 25f);
                Vector2 end = start + Main.rand.NextVector2Circular(20f, 20f);
                NachtmusikVFXLibrary.SpawnConstellationLine(start, end, 3);
            }

            // Music notes — the conductor's authority grows with minions
            if (Main.rand.NextBool(25 - Math.Min(minionCount * 4, 18)))
            {
                Vector2 notePos = center + Main.rand.NextVector2Circular(35f, 35f);
                NachtmusikVFXLibrary.SpawnMusicNotes(notePos, 1, 10f, 0.7f, 0.9f, 25);
            }

            // Grand conductor light
            float intensity = 0.25f + minionCount * 0.05f;
            Lighting.AddLight(center, NachtmusikPalette.CosmicPurple.ToVector3() * 0.25f * intensity);
        }

        // =====================================================================
        //  PREDRAW IN WORLD BLOOM
        // =====================================================================

        public static void PreDrawInWorldBloom(SpriteBatch sb, Texture2D tex,
            Vector2 pos, Vector2 origin, float rotation, float scale)
        {
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.035f) * 0.06f;
            NachtmusikPalette.DrawItemBloom(sb, tex, pos, origin, rotation, scale, pulse);
        }

        // =====================================================================
        //  SUMMON VFX
        // =====================================================================

        /// <summary>
        /// One-shot VFX when the Stellar Conductor is summoned.
        /// Grand celestial explosion, constellation circle, glyph burst,
        /// massive music note cascade — the ultimate summon entrance.
        /// </summary>
        public static void SummonVFX(Vector2 spawnPos)
        {
            if (Main.dedServ) return;

            // Grand entrance explosion — multi-layer
            try { CustomParticles.GenericFlare(spawnPos, NachtmusikPalette.StarWhite, 0.9f, 20); } catch { }
            try { CustomParticles.GenericFlare(spawnPos, NachtmusikPalette.RadianceGold, 0.7f, 18); } catch { }
            try { CustomParticles.GenericFlare(spawnPos, NachtmusikPalette.CosmicPurple, 0.5f, 16); } catch { }

            // Constellation circle — the stellar conductor's domain
            float rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            NachtmusikVFXLibrary.SpawnConstellationCircle(spawnPos, 50f, 8, rotation);

            // Glyph burst — authority materializes
            NachtmusikVFXLibrary.SpawnGlyphBurst(spawnPos, 6, 4f, 0.35f);

            // Massive gradient halo cascade
            NachtmusikVFXLibrary.SpawnGradientHaloRings(spawnPos, 5, 0.35f);
            NachtmusikVFXLibrary.SpawnRadianceHaloRings(spawnPos, 4, 0.3f);

            // Starburst — stars radiate outward
            NachtmusikVFXLibrary.SpawnStarburstCascade(spawnPos, 12, 6f, 0.35f);

            // Radial dust burst
            NachtmusikVFXLibrary.SpawnRadialDustBurst(spawnPos, 14, 5f);

            // Grand music note shower — the overture begins
            NachtmusikVFXLibrary.SpawnMusicNotes(spawnPos, 6, 30f, 0.8f, 1.0f, 35);

            // Grand bloom
            NachtmusikVFXLibrary.DrawBloom(spawnPos, 0.65f);

            Lighting.AddLight(spawnPos, NachtmusikPalette.StarWhite.ToVector3() * 1.0f);
        }

        // =====================================================================
        //  MINION AMBIENT VFX
        // =====================================================================

        /// <summary>
        /// Per-frame stellar conductor ambient: grand constellation orbit,
        /// cosmic purple authority aura, commanding starlight pulses,
        /// orbiting glyphs, music note trails.
        /// </summary>
        public static void MinionAmbientVFX(Vector2 pos, float visibility)
        {
            if (Main.dedServ) return;

            float time = (float)Main.timeForVisualEffects;

            // Grand cosmic dust aura (2 per frame)
            for (int d = 0; d < 2; d++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(18f, 18f);
                Color col = NachtmusikPalette.GetNocturnalGradient(Main.rand.NextFloat());
                Dust dust = Dust.NewDustPerfect(pos + offset, DustID.PurpleTorch,
                    Main.rand.NextVector2Circular(1.2f, 1.2f), 0, col,
                    (1.2f + Main.rand.NextFloat(0.3f)) * visibility);
                dust.noGravity = true;
                dust.fadeIn = 1.4f;
            }

            // Golden authority sparkles (1-in-2)
            if (Main.rand.NextBool(2) && visibility > 0.5f)
            {
                var sparkle = new SparkleParticle(pos + Main.rand.NextVector2Circular(20f, 20f),
                    Main.rand.NextVector2Circular(1.2f, 1.2f),
                    NachtmusikPalette.RadianceGold * visibility * 0.7f, 0.4f, 20);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // Cosmic shimmer glow (1-in-3)
            if (Main.rand.NextBool(3) && visibility > 0.5f)
            {
                Color shimmer = NachtmusikPalette.GetShimmer();
                var glow = new GenericGlowParticle(pos + Main.rand.NextVector2Circular(15f, 15f),
                    Main.rand.NextVector2Circular(1f, 1f),
                    shimmer * visibility * 0.8f, 0.35f, 18, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // Radiance flares (1-in-3)
            if (Main.rand.NextBool(3) && visibility > 0.4f)
            {
                try { CustomParticles.GenericFlare(pos + Main.rand.NextVector2Circular(12f, 12f),
                    NachtmusikPalette.GetCelestialGradient(Main.rand.NextFloat()) * visibility * 0.6f,
                    0.35f, 14); } catch { }
            }

            // Orbiting constellation ring (periodic)
            if (Main.GameUpdateCount % 20 == 0)
            {
                NachtmusikVFXLibrary.SpawnOrbitingGlyphs(pos, 4, 35f * visibility, time * 0.03f);
            }

            // Orbiting star motes (periodic)
            if (Main.GameUpdateCount % 12 == 0)
            {
                float baseAngle = Main.GameUpdateCount * 0.05f;
                for (int i = 0; i < 4; i++)
                {
                    float angle = baseAngle + MathHelper.TwoPi * i / 4f;
                    Vector2 motePos = pos + angle.ToRotationVector2() * 30f;
                    try { CustomParticles.GenericFlare(motePos,
                        NachtmusikPalette.GetStarfieldGradient((float)i / 4f) * visibility,
                        0.35f * visibility, 14); } catch { }
                }
            }

            // Music notes — the conductor's melody (1-in-5)
            if (Main.rand.NextBool(5) && visibility > 0.5f)
                NachtmusikVFXLibrary.SpawnMusicNotes(pos, 1, 15f, 0.75f, 0.9f, 25);

            // Grand conductor light — slightly stronger
            float pulse = 0.4f + MathF.Sin(time * 0.1f) * 0.1f;
            Lighting.AddLight(pos, NachtmusikPalette.CosmicPurple.ToVector3() * pulse * visibility);
        }

        // =====================================================================
        //  MINION ATTACK VFX
        // =====================================================================

        /// <summary>
        /// Stellar conductor commands a star attack: constellation flash,
        /// radial star sparks, glyph burst, grand music note cascade.
        /// </summary>
        public static void MinionAttackVFX(Vector2 minionPos, Vector2 direction)
        {
            if (Main.dedServ) return;

            // Command flash — multi-layer
            try { CustomParticles.GenericFlare(minionPos, NachtmusikPalette.RadianceGold, 0.6f, 14); } catch { }
            try { CustomParticles.HaloRing(minionPos, NachtmusikPalette.CosmicPurple, 0.4f, 14); } catch { }

            // Constellation flash at attack point
            float rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            NachtmusikVFXLibrary.SpawnConstellationCircle(minionPos, 30f, 5, rotation);

            // Directed star sparks
            for (int i = 0; i < 6; i++)
            {
                float spread = Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 vel = direction.RotatedBy(spread) * Main.rand.NextFloat(3f, 7f);
                Color sparkColor = NachtmusikPalette.GetStarfieldGradient((float)i / 6f);
                var spark = new GlowSparkParticle(minionPos, vel, sparkColor, 0.28f, 16);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Glyph burst
            NachtmusikVFXLibrary.SpawnGlyphBurst(minionPos, 3, 3f, 0.3f);

            // Music notes — the conductor commands
            NachtmusikVFXLibrary.SpawnMusicNotes(minionPos, 3, 18f, 0.8f, 1.0f, 25);

            // Twinkling stars
            NachtmusikVFXLibrary.SpawnTwinklingStars(minionPos, 3, 12f);

            Lighting.AddLight(minionPos, NachtmusikPalette.RadianceGold.ToVector3() * 0.7f);
        }

        // =====================================================================
        //  STAR PROJECTILE TRAIL VFX
        // =====================================================================

        /// <summary>
        /// Per-frame trail for the conductor's star attack projectiles:
        /// radiant beam trail, cosmic dust, constellation sparkles.
        /// </summary>
        public static void ProjectileTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            // Radiant beam trail
            NachtmusikVFXLibrary.SpawnRadiantBeamTrail(pos, velocity, 0.9f);

            // Cosmic purple dust
            Dust d = Dust.NewDustPerfect(pos, DustID.PurpleTorch,
                Main.rand.NextVector2Circular(0.8f, 0.8f), 0,
                NachtmusikPalette.CosmicPurple, 1.3f);
            d.noGravity = true;

            // Golden radiance sparkle (1-in-3)
            if (Main.rand.NextBool(3))
            {
                try { CustomParticles.GenericFlare(pos + Main.rand.NextVector2Circular(5f, 5f),
                    NachtmusikPalette.RadianceGold * 0.5f, 0.2f, 10); } catch { }
            }

            // Constellation accent (1-in-6)
            if (Main.rand.NextBool(6))
                NachtmusikVFXLibrary.SpawnMusicNotes(pos, 1, 6f, 0.7f, 0.85f, 20);

            NachtmusikVFXLibrary.AddNachtmusikLight(pos, 0.4f);
        }

        // =====================================================================
        //  STAR IMPACT VFX
        // =====================================================================

        /// <summary>
        /// Star attack on-hit: constellation web, gradient halos,
        /// starburst, radiance burst, grand music notes.
        /// </summary>
        public static void ProjectileImpactVFX(Vector2 hitPos)
        {
            if (Main.dedServ) return;

            // Constellation web at impact
            float rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            NachtmusikVFXLibrary.SpawnConstellationCircle(hitPos, 25f, 5, rotation);

            NachtmusikVFXLibrary.SpawnGradientHaloRings(hitPos, 3, 0.25f);
            NachtmusikVFXLibrary.SpawnRadialDustBurst(hitPos, 10, 5f);
            NachtmusikVFXLibrary.SpawnRadianceBurst(hitPos, 6, 4f);
            NachtmusikVFXLibrary.SpawnTwinklingStars(hitPos, 4, 15f);
            NachtmusikVFXLibrary.SpawnStarBurst(hitPos, 8, 0.28f);
            NachtmusikVFXLibrary.SpawnMusicNotes(hitPos, 3, 18f, 0.8f, 1.0f, 28);

            // Shattered starlight
            NachtmusikVFXLibrary.SpawnShatteredStarlight(hitPos, 4, 4f, 0.5f, true);

            Lighting.AddLight(hitPos, NachtmusikPalette.StarWhite.ToVector3() * 0.7f);
        }

        // =====================================================================
        //  GRAND CONDUCTOR SPECIAL VFX
        // =====================================================================

        /// <summary>
        /// Grand conductor special ability: constellation explosion,
        /// orbiting glyph ring, massive starburst cascade, golden supernova.
        /// Triggered on every Nth attack from the Stellar Conductor.
        /// </summary>
        public static void GrandConductorVFX(Vector2 pos, float intensity = 1f)
        {
            if (Main.dedServ) return;

            NachtmusikVFXLibrary.ProjectileImpact(pos, intensity);

            // Grand constellation circle
            NachtmusikVFXLibrary.SpawnConstellationCircle(pos, 50f * intensity, 8,
                Main.rand.NextFloat(MathHelper.TwoPi));

            // Orbiting glyph authority ring
            NachtmusikVFXLibrary.SpawnOrbitingGlyphs(pos, 8, 40f * intensity,
                Main.GameUpdateCount * 0.03f);

            // Massive starburst cascade
            NachtmusikVFXLibrary.SpawnStarburstCascade(pos, 10, 7f * intensity, 0.4f * intensity);

            // Golden supernova radiance
            NachtmusikVFXLibrary.SpawnRadianceHaloRings(pos, 6, 0.35f * intensity);

            // Grand music note shower
            NachtmusikVFXLibrary.SpawnMusicNotes(pos, 5, 30f * intensity, 0.8f, 1.0f, 30);

            Lighting.AddLight(pos, NachtmusikPalette.StarWhite.ToVector3() * intensity);
        }

        // =====================================================================
        //  DESPAWN VFX
        // =====================================================================

        public static void DespawnVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            NachtmusikVFXLibrary.SpawnRadialDustBurst(pos, 8, 4f);
            NachtmusikVFXLibrary.SpawnTwinklingStars(pos, 4, 12f);
            NachtmusikVFXLibrary.SpawnShatteredStarlight(pos, 3, 3f, 0.4f, true);
            try { CustomParticles.HaloRing(pos, NachtmusikPalette.CosmicPurple * 0.5f, 0.25f, 12); } catch { }
        }
    }
}
