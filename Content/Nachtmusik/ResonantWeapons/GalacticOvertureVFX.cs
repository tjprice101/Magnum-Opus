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
    /// VFX helper for the Galactic Overture summon weapon.
    /// Grand galactic melody — sweeping golden-blue muse trails,
    /// musical projectile cascades, serenade shimmer aura, cosmic
    /// notation bursts. The muse performs an overture across the sky.
    /// </summary>
    public static class GalacticOvertureVFX
    {
        // =====================================================================
        //  HOLD ITEM VFX
        // =====================================================================

        public static void HoldItemVFX(Player player, int minionCount)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Overture gold aura — warm serenade glow
            if (Main.rand.NextBool(6))
            {
                Vector2 offset = Main.rand.NextVector2Circular(24f, 24f);
                Color auraColor = Color.Lerp(NachtmusikPalette.RadianceGold, NachtmusikPalette.SerenadeGlow,
                    Main.rand.NextFloat());
                var glow = new GenericGlowParticle(center + offset, new Vector2(0, -0.3f),
                    auraColor * 0.3f, 0.15f, 18, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // Musical notation motes — ascending
            if (Main.rand.NextBool(25 - Math.Min(minionCount * 3, 15)))
            {
                Vector2 notePos = center + new Vector2(Main.rand.NextFloat(-25f, 25f), 10f);
                NachtmusikVFXLibrary.SpawnMusicNotes(notePos, 1, 8f, 0.7f, 0.85f, 25);
            }

            // Warm golden light
            float intensity = 0.2f + minionCount * 0.04f;
            Lighting.AddLight(center, NachtmusikPalette.RadianceGold.ToVector3() * 0.2f * intensity);
        }

        // =====================================================================
        //  PREDRAW IN WORLD BLOOM
        // =====================================================================

        public static void PreDrawInWorldBloom(SpriteBatch sb, Texture2D tex,
            Vector2 pos, Vector2 origin, float rotation, float scale)
        {
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.045f) * 0.05f;
            NachtmusikPalette.DrawItemBloom(sb, tex, pos, origin, rotation, scale, pulse);
        }

        // =====================================================================
        //  SUMMON VFX
        // =====================================================================

        /// <summary>
        /// One-shot VFX when the Celestial Muse is summoned.
        /// Golden overture flash, radiance burst, ascending music note cascade.
        /// </summary>
        public static void SummonVFX(Vector2 spawnPos)
        {
            if (Main.dedServ) return;

            // Golden entrance flash
            try { CustomParticles.GenericFlare(spawnPos, NachtmusikPalette.RadianceGold, 0.7f, 16); } catch { }
            try { CustomParticles.GenericFlare(spawnPos, NachtmusikPalette.StarWhite, 0.5f, 14); } catch { }
            try { CustomParticles.HaloRing(spawnPos, NachtmusikPalette.CosmicPurple, 0.4f, 15); } catch { }

            // Radiance burst
            NachtmusikVFXLibrary.SpawnRadianceBurst(spawnPos, 8, 5f);

            // Gradient halos
            NachtmusikVFXLibrary.SpawnGradientHaloRings(spawnPos, 3, 0.3f);

            // Overture music note cascade — grand musical entrance
            NachtmusikVFXLibrary.SpawnMusicNotes(spawnPos, 5, 25f, 0.8f, 1.0f, 30);

            // Twinkling stars
            NachtmusikVFXLibrary.SpawnTwinklingStars(spawnPos, 4, 20f);

            // Bloom
            NachtmusikVFXLibrary.DrawBloom(spawnPos, 0.45f);

            Lighting.AddLight(spawnPos, NachtmusikPalette.RadianceGold.ToVector3() * 0.8f);
        }

        // =====================================================================
        //  MINION AMBIENT VFX
        // =====================================================================

        /// <summary>
        /// Per-frame muse ambient: golden serenade aura, hovering sparkles,
        /// ascending music notation, warm shimmer glow.
        /// </summary>
        public static void MinionAmbientVFX(Vector2 pos, float visibility)
        {
            if (Main.dedServ) return;

            // Golden muse aura dust
            if (Main.rand.NextBool(2))
            {
                Vector2 offset = Main.rand.NextVector2Circular(12f, 12f);
                Color col = Color.Lerp(NachtmusikPalette.RadianceGold, NachtmusikPalette.StarGold,
                    Main.rand.NextFloat());
                Dust dust = Dust.NewDustPerfect(pos + offset, DustID.GoldFlame,
                    new Vector2(0, -0.5f) + Main.rand.NextVector2Circular(0.5f, 0.5f), 0,
                    col, (1.0f + Main.rand.NextFloat(0.3f)) * visibility);
                dust.noGravity = true;
            }

            // Serenade shimmer sparkle (1-in-3)
            if (Main.rand.NextBool(3) && visibility > 0.5f)
            {
                var sparkle = new SparkleParticle(pos + Main.rand.NextVector2Circular(15f, 15f),
                    new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -0.8f),
                    NachtmusikPalette.SerenadeGlow * visibility, 0.35f, 18);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // Warm radiance glow (1-in-4)
            if (Main.rand.NextBool(4) && visibility > 0.5f)
            {
                Color shimmer = NachtmusikPalette.GetRadianceShimmer();
                var glow = new GenericGlowParticle(pos + Main.rand.NextVector2Circular(10f, 10f),
                    new Vector2(0, -0.6f),
                    shimmer * visibility * 0.6f, 0.28f, 16, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // Flare accent (1-in-3)
            if (Main.rand.NextBool(3) && visibility > 0.4f)
            {
                try { CustomParticles.GenericFlare(pos + Main.rand.NextVector2Circular(10f, 10f),
                    NachtmusikPalette.RadianceGold * visibility * 0.5f, 0.3f, 12); } catch { }
            }

            // Music note — the muse's melody (1-in-6)
            if (Main.rand.NextBool(6) && visibility > 0.5f)
                NachtmusikVFXLibrary.SpawnMusicNotes(pos, 1, 12f, 0.7f, 0.85f, 25);

            // Golden muse light
            float pulse = 0.3f + MathF.Sin((float)Main.timeForVisualEffects * 0.1f) * 0.08f;
            Lighting.AddLight(pos, NachtmusikPalette.RadianceGold.ToVector3() * pulse * visibility);
        }

        // =====================================================================
        //  MINION ATTACK VFX
        // =====================================================================

        /// <summary>
        /// Muse fires a musical projectile: golden flash, radiance sparks,
        /// music note burst along firing direction.
        /// </summary>
        public static void MinionAttackVFX(Vector2 minionPos, Vector2 direction)
        {
            if (Main.dedServ) return;

            // Attack flash — golden radiance
            try { CustomParticles.GenericFlare(minionPos, NachtmusikPalette.RadianceGold, 0.5f, 12); } catch { }

            // Directional radiance sparks
            for (int i = 0; i < 4; i++)
            {
                float spread = Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 vel = direction.RotatedBy(spread) * Main.rand.NextFloat(2f, 5f);
                Color sparkColor = Color.Lerp(NachtmusikPalette.RadianceGold, NachtmusikPalette.StarWhite,
                    (float)i / 4f);
                var spark = new GlowSparkParticle(minionPos, vel, sparkColor, 0.22f, 12);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Music notes — the muse plays
            NachtmusikVFXLibrary.SpawnMusicNotes(minionPos, 2, 12f, 0.7f, 0.9f, 20);

            Lighting.AddLight(minionPos, NachtmusikPalette.RadianceGold.ToVector3() * 0.5f);
        }

        // =====================================================================
        //  MUSE PROJECTILE TRAIL VFX
        // =====================================================================

        /// <summary>
        /// Per-frame trail for the muse's musical projectiles:
        /// golden radiance dust, serenade shimmer, ascending notes.
        /// </summary>
        public static void ProjectileTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            // Golden radiance trail dust
            Dust d = Dust.NewDustPerfect(pos, DustID.GoldFlame,
                Main.rand.NextVector2Circular(0.5f, 0.5f), 0,
                NachtmusikPalette.RadianceGold, 1.2f);
            d.noGravity = true;

            // Serenade shimmer (1-in-3)
            if (Main.rand.NextBool(3))
            {
                try { CustomParticles.GenericFlare(pos + Main.rand.NextVector2Circular(4f, 4f),
                    NachtmusikPalette.SerenadeGlow * 0.5f, 0.18f, 10); } catch { }
            }

            // Music note trail (1-in-8)
            if (Main.rand.NextBool(8))
                NachtmusikVFXLibrary.SpawnMusicNotes(pos, 1, 5f, 0.7f, 0.85f, 18);

            NachtmusikVFXLibrary.AddRadianceLight(pos, Main.GameUpdateCount, 0.3f);
        }

        // =====================================================================
        //  PROJECTILE IMPACT VFX
        // =====================================================================

        /// <summary>
        /// Muse projectile on-hit: golden radiance flash, star sparks,
        /// music notes, twinkling stars.
        /// </summary>
        public static void ProjectileImpactVFX(Vector2 hitPos)
        {
            if (Main.dedServ) return;

            NachtmusikVFXLibrary.SpawnGradientHaloRings(hitPos, 2, 0.2f);
            NachtmusikVFXLibrary.SpawnRadialDustBurst(hitPos, 6, 3f);
            NachtmusikVFXLibrary.SpawnRadianceBurst(hitPos, 4, 3f);
            NachtmusikVFXLibrary.SpawnTwinklingStars(hitPos, 2, 10f);
            NachtmusikVFXLibrary.SpawnMusicNotes(hitPos, 2, 12f, 0.7f, 0.9f, 22);

            Lighting.AddLight(hitPos, NachtmusikPalette.RadianceGold.ToVector3() * 0.5f);
        }

        // =====================================================================
        //  DESPAWN VFX
        // =====================================================================

        public static void DespawnVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            NachtmusikVFXLibrary.SpawnRadialDustBurst(pos, 5, 3f);
            NachtmusikVFXLibrary.SpawnRadianceBurst(pos, 3, 2f);
            try { CustomParticles.HaloRing(pos, NachtmusikPalette.RadianceGold * 0.4f, 0.18f, 10); } catch { }
        }
    }
}
