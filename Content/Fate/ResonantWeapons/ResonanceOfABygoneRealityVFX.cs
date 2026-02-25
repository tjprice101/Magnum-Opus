using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.Fate.ResonantWeapons
{
    /// <summary>
    /// VFX helper for Resonance of a Bygone Reality — ranged rapid-fire, 400 dmg.
    /// Rapid-fire cosmic bullets. Every 5th hit spawns a spectral blade.
    /// Palette: FatePalette.BygoneResonance
    /// (CosmicVoid -> NebulaMist -> NebulaPurple -> CosmicRose -> StarGold -> ConstellationSilver)
    /// </summary>
    public static class ResonanceOfABygoneRealityVFX
    {
        // ===== HOLD ITEM VFX =====

        /// <summary>
        /// Per-frame held-item VFX: rapid energy particles flow toward the weapon,
        /// glyphs drift near the weapon position, star particles provide ambient sparkle,
        /// and an energy buildup light pulses at the weapon grip.
        /// </summary>
        public static void HoldItemVFX(Player player)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;
            Vector2 weaponTip = center + new Vector2(player.direction * 28f, -6f);

            // Rapid energy particles flowing toward weapon (1-in-4)
            if (Main.rand.NextBool(4))
            {
                float inAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                float inDist = Main.rand.NextFloat(25f, 45f);
                Vector2 startPos = weaponTip + inAngle.ToRotationVector2() * inDist;
                Vector2 vel = (weaponTip - startPos).SafeNormalize(Vector2.Zero) * 2.5f;
                Color col = FatePalette.GetCosmicGradient(Main.rand.NextFloat());
                var glow = new GenericGlowParticle(startPos, vel, col * 0.6f, 0.18f, 16, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // Glyphs near weapon position (1-in-15)
            if (Main.rand.NextBool(15))
            {
                Vector2 glyphPos = weaponTip + Main.rand.NextVector2Circular(12f, 12f);
                Color glyphCol = FatePalette.PaletteLerp(FatePalette.BygoneResonance, Main.rand.NextFloat(0.2f, 0.6f));
                CustomParticles.Glyph(glyphPos, glyphCol, 0.2f, -1);
            }

            // Star particles ambient (1-in-8)
            if (Main.rand.NextBool(8))
            {
                Vector2 starPos = center + Main.rand.NextVector2Circular(30f, 30f);
                Color starCol = Main.rand.NextBool(3) ? FatePalette.StarGold : FatePalette.ConstellationSilver;
                var star = new GenericGlowParticle(starPos, Main.rand.NextVector2Circular(0.5f, 0.5f),
                    starCol * 0.5f, 0.16f, 15, true);
                MagnumParticleHandler.SpawnParticle(star);
            }

            // Energy buildup light at weapon
            float pulse = 0.25f + MathF.Sin(time * 0.07f) * 0.1f;
            Color lightCol = Color.Lerp(FatePalette.NebulaPurple, FatePalette.CosmicRose, pulse);
            Lighting.AddLight(weaponTip, lightCol.ToVector3() * pulse);
        }

        // ===== PREDRAW IN WORLD BLOOM =====

        /// <summary>
        /// Standard 3-layer Fate item bloom for the weapon sprite.
        /// </summary>
        public static void PreDrawInWorldBloom(
            SpriteBatch sb, Texture2D tex, Vector2 pos,
            Vector2 origin, float rotation, float scale)
        {
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.04f) * 0.03f;
            FatePalette.DrawItemBloom(sb, tex, pos, origin, rotation, scale, pulse);
        }

        // ===== MUZZLE FLASH VFX =====

        /// <summary>
        /// One-shot muzzle flash VFX when firing a cosmic bullet.
        /// Small FateVFXLibrary.ProjectileImpact at 0.3f intensity,
        /// occasional glyph, and directional spark particles.
        /// </summary>
        public static void MuzzleFlashVFX(Vector2 muzzlePos, Vector2 direction)
        {
            if (Main.dedServ) return;

            // Small projectile impact flash
            FateVFXLibrary.ProjectileImpact(muzzlePos, 0.3f);

            // Occasional glyph at muzzle (1-in-5)
            if (Main.rand.NextBool(5))
            {
                Color glyphCol = FatePalette.PaletteLerp(FatePalette.BygoneResonance, Main.rand.NextFloat(0.3f, 0.7f));
                CustomParticles.Glyph(muzzlePos, glyphCol, 0.2f, -1);
            }

            // Directional spark particles — 3 sparks along firing direction
            for (int i = 0; i < 3; i++)
            {
                Vector2 sparkVel = direction * Main.rand.NextFloat(3f, 6f)
                    + direction.RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-1.5f, 1.5f);
                Color sparkCol = FatePalette.PaletteLerp(FatePalette.BygoneResonance, Main.rand.NextFloat(0.3f, 0.8f));
                var spark = new GlowSparkParticle(muzzlePos, sparkVel, sparkCol, 0.2f, 10);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Muzzle dust puff
            Dust d = Dust.NewDustPerfect(muzzlePos, DustID.PinkTorch,
                direction * 2f + Main.rand.NextVector2Circular(1f, 1f), 0,
                FatePalette.CosmicRose, 1.2f);
            d.noGravity = true;

            Lighting.AddLight(muzzlePos, FatePalette.CosmicRose.ToVector3() * 0.5f);
        }

        // ===== BULLET TRAIL VFX =====

        /// <summary>
        /// Per-frame trail VFX for rapid-fire cosmic bullet projectiles.
        /// Fast, tight trail using the BygoneResonance palette with nebula mist wisps.
        /// </summary>
        public static void BulletTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 trailDir = -velocity.SafeNormalize(Vector2.Zero);

            // Primary trail glow — tight, fast particles
            Color trailCol = FatePalette.PaletteLerp(FatePalette.BygoneResonance, Main.rand.NextFloat(0.2f, 0.8f));
            var glow = new GenericGlowParticle(pos + Main.rand.NextVector2Circular(3f, 3f),
                trailDir * 1.2f + Main.rand.NextVector2Circular(0.4f, 0.4f),
                trailCol * 0.75f, 0.16f, 12, true);
            MagnumParticleHandler.SpawnParticle(glow);

            // Fast sparks along trail (1-in-2)
            if (Main.rand.NextBool(2))
            {
                Vector2 sparkVel = trailDir * Main.rand.NextFloat(1f, 2.5f) + Main.rand.NextVector2Circular(1f, 1f);
                Color sparkCol = FatePalette.PaletteLerp(FatePalette.BygoneResonance, Main.rand.NextFloat(0.4f, 1f));
                var spark = new GlowSparkParticle(pos, sparkVel, sparkCol * 0.8f, 0.14f, 8);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Nebula mist wisps trailing (1-in-4)
            if (Main.rand.NextBool(4))
            {
                Color mistCol = Color.Lerp(FatePalette.NebulaMist, FatePalette.NebulaPurple, Main.rand.NextFloat()) * 0.35f;
                var mist = new GenericGlowParticle(pos + Main.rand.NextVector2Circular(5f, 5f),
                    trailDir * 0.4f + Main.rand.NextVector2Circular(0.6f, 0.6f),
                    mistCol, 0.14f, 16, true);
                MagnumParticleHandler.SpawnParticle(mist);
            }

            // Torch dust for visibility (1-in-3)
            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustPerfect(pos, DustID.PinkTorch,
                    trailDir * Main.rand.NextFloat(0.5f, 2f), 0,
                    FatePalette.CosmicRose, 1.0f);
                d.noGravity = true;
            }

            Lighting.AddLight(pos, FatePalette.NebulaPurple.ToVector3() * 0.25f);
        }

        // ===== BULLET IMPACT VFX =====

        /// <summary>
        /// Impact VFX when a cosmic bullet strikes an enemy.
        /// Lightweight melee impact with star sparkle accents
        /// and bygone echo particles fading outward.
        /// </summary>
        public static void BulletImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            // Lightweight shared impact
            FateVFXLibrary.MeleeImpact(pos, 0);

            // Additional star sparkles
            FateVFXLibrary.SpawnStarSparkles(pos, 4, 20f, 0.2f);

            // Halo ring
            CustomParticles.HaloRing(pos, FatePalette.CosmicRose, 0.25f, 12);

            // Small radial dust
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                Color col = FatePalette.PaletteLerp(FatePalette.BygoneResonance, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.PinkTorch, vel, 0, col, 1.2f);
                d.noGravity = true;
            }

            // Bygone echo glow particles — fading phantom embers
            for (int i = 0; i < 3; i++)
            {
                Vector2 echoVel = Main.rand.NextVector2Circular(2f, 2f);
                Color echoCol = Color.Lerp(FatePalette.NebulaMist, FatePalette.ConstellationSilver, Main.rand.NextFloat()) * 0.5f;
                var echo = new GenericGlowParticle(pos + Main.rand.NextVector2Circular(8f, 8f),
                    echoVel, echoCol, 0.16f, 16, true);
                MagnumParticleHandler.SpawnParticle(echo);
            }

            // Occasional glyph accent at impact (1-in-3)
            if (Main.rand.NextBool(3))
            {
                Color glyphCol = FatePalette.PaletteLerp(FatePalette.BygoneResonance, Main.rand.NextFloat(0.3f, 0.7f));
                CustomParticles.Glyph(pos, glyphCol * 0.6f, 0.18f, -1);
            }

            Lighting.AddLight(pos, FatePalette.CosmicRose.ToVector3() * 0.5f);
        }

        // ===== SPECTRAL BLADE SPAWN VFX =====

        /// <summary>
        /// Dramatic VFX when the 5th-hit spectral blade spawns.
        /// Uses FateVFXLibrary.FinisherSlam at 0.5f intensity,
        /// glyph burst, constellation burst, and music notes.
        /// </summary>
        public static void SpectralBladeSpawnVFX(Vector2 spawnPos)
        {
            if (Main.dedServ) return;

            // Finisher slam at reduced intensity — dramatic but not overwhelming
            FateVFXLibrary.FinisherSlam(spawnPos, 0.5f);

            // Glyph burst radiating outward
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 glyphPos = spawnPos + angle.ToRotationVector2() * 25f;
                Color glyphCol = FatePalette.PaletteLerp(FatePalette.BygoneResonance, (float)i / 6f);
                CustomParticles.Glyph(glyphPos, glyphCol, 0.28f, -1);
            }

            // Constellation burst — stars forming a pattern
            FateVFXLibrary.SpawnConstellationBurst(spawnPos, 5, 45f, 0.8f);

            // Music notes — the bygone melody manifests
            FateVFXLibrary.SpawnMusicNotes(spawnPos, 4, 25f, 0.8f, 1.0f, 30);

            // Central flares
            CustomParticles.GenericFlare(spawnPos, FatePalette.ConstellationSilver, 0.6f, 18);
            CustomParticles.GenericFlare(spawnPos, FatePalette.CosmicRose, 0.5f, 16);

            // Bloom flash
            FateVFXLibrary.DrawBloom(spawnPos, 0.55f);

            Lighting.AddLight(spawnPos, FatePalette.StarGold.ToVector3() * 1.0f);
        }
    }
}
