using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.Fate.Enemies
{
    /// <summary>
    /// VFX helper for Fate-themed enemies: Herald of Fate (mini-boss) and
    /// Fate, The Warden of Universal Melodies (endgame boss).
    /// Uses FatePalette and FateVFXLibrary for canonical colours and shared effects.
    /// </summary>
    public static class FateEnemyVFX
    {
        // =====================================================================
        //  HERALD OF FATE — MINI-BOSS VFX
        // =====================================================================

        // ===== AMBIENT VFX =====

        /// <summary>
        /// Per-frame ambient particles for the Herald of Fate.
        /// Cosmic glow particles, occasional glyphs, star sparkles.
        /// </summary>
        public static void HeraldAmbientVFX(Vector2 center, float width, float height)
        {
            if (Main.dedServ) return;

            // Cosmic glow particles (1-in-4)
            if (Main.rand.NextBool(4))
            {
                Vector2 offset = Main.rand.NextVector2Circular(width * 0.5f, height * 0.5f);
                Color col = Main.rand.NextBool() ? FatePalette.DarkPink : FatePalette.FatePurple;
                var particle = new GenericGlowParticle(center + offset,
                    Main.rand.NextVector2Circular(1f, 1f), col * 0.6f, 0.25f, 20, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }

            // Occasional glyph (1-in-20)
            if (Main.rand.NextBool(20))
            {
                Vector2 glyphPos = center + Main.rand.NextVector2Circular(60f, 60f);
                CustomParticles.Glyph(glyphPos, FatePalette.FatePurple * 0.7f, 0.3f, -1);
            }

            // Star sparkles (1-in-8)
            if (Main.rand.NextBool(8))
            {
                Vector2 sparklePos = center + Main.rand.NextVector2Circular(50f, 50f);
                CustomParticles.GenericFlare(sparklePos, FatePalette.WhiteCelestial * 0.5f, 0.15f, 10);
            }
        }

        /// <summary>
        /// Orbiting glyph particles around the Herald.
        /// </summary>
        public static void HeraldOrbitingGlyphs(Vector2 center, float glyphRotation)
        {
            if (Main.dedServ) return;

            if (Main.rand.NextBool(15))
            {
                float angle = glyphRotation + Main.rand.NextFloat(MathHelper.TwoPi);
                float radius = 60f + Main.rand.NextFloat(20f);
                Vector2 glyphPos = center + angle.ToRotationVector2() * radius;
                CustomParticles.Glyph(glyphPos, FatePalette.DarkPink * 0.8f, 0.25f, -1);
            }
        }

        // ===== COSMIC BURST =====

        /// <summary>
        /// Cosmic burst explosion for Herald spawn, death, and enrage.
        /// Central flash + cascading halos + glyph burst + radial particles + star sparkles.
        /// </summary>
        public static void HeraldCosmicBurst(Vector2 position, float scale)
        {
            if (Main.dedServ) return;

            // Central flash cascade
            CustomParticles.GenericFlare(position, FatePalette.WhiteCelestial, 1.2f * scale, 25);
            CustomParticles.GenericFlare(position, FatePalette.BrightCrimson, 0.9f * scale, 22);
            CustomParticles.GenericFlare(position, FatePalette.DarkPink, 0.7f * scale, 20);

            // Cascading halo rings
            FateVFXLibrary.SpawnGradientHaloRings(position, 6, 0.3f * scale);

            // Glyph burst
            FateVFXLibrary.SpawnGlyphBurst(position, 8, 6f * scale);

            // Radial glow particles
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 vel = angle.ToRotationVector2() * (8f * scale);
                Color col = Color.Lerp(FatePalette.DarkPink, FatePalette.BrightCrimson, Main.rand.NextFloat());
                var particle = new GenericGlowParticle(position, vel, col, 0.4f * scale, 25, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }

            // Star sparkles
            FateVFXLibrary.SpawnStarSparkles(position, 12, 40f * scale, 0.3f * scale);

            // Music notes
            FateVFXLibrary.SpawnMusicNotes(position, 4, 30f * scale, 0.8f, 1.0f, 28);

            Lighting.AddLight(position, FatePalette.BrightCrimson.ToVector3() * 1.5f * scale);
        }

        // ===== ATTACK VFX =====

        /// <summary>
        /// Slash trail VFX for Herald's Cosmic Rend attack.
        /// Directional glow particles with central flare.
        /// </summary>
        public static void HeraldSlashVFX(Vector2 position, Vector2 direction)
        {
            if (Main.dedServ) return;

            for (int i = 0; i < 8; i++)
            {
                float spread = MathHelper.ToRadians(30f);
                Vector2 vel = direction.RotatedBy(Main.rand.NextFloat(-spread, spread)) * Main.rand.NextFloat(4f, 10f);
                Color col = Color.Lerp(FatePalette.DarkPink, FatePalette.BrightCrimson, Main.rand.NextFloat());
                var particle = new GenericGlowParticle(position, vel, col, 0.35f, 18, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }

            CustomParticles.GenericFlare(position, FatePalette.WhiteCelestial, 0.6f, 12);

            // Music note accent
            FateVFXLibrary.SpawnMusicNotes(position, 1, 10f, 0.7f, 0.85f, 18);

            Lighting.AddLight(position, FatePalette.BrightCrimson.ToVector3() * 0.6f);
        }

        /// <summary>
        /// Converging warning particles during Herald attack windups.
        /// Particles spiral inward toward center as progress increases.
        /// </summary>
        public static void HeraldWindupVFX(Vector2 center, float progress, int particleCount, float maxRadius)
        {
            if (Main.dedServ) return;

            for (int i = 0; i < particleCount; i++)
            {
                float angle = MathHelper.TwoPi * i / particleCount;
                float radius = maxRadius * (1f - progress * 0.5f);
                Vector2 pos = center + angle.ToRotationVector2() * radius;
                Color col = FatePalette.GetCosmicGradient(progress);
                CustomParticles.GenericFlare(pos, col, 0.3f + progress * 0.3f, 12);
            }
        }

        /// <summary>
        /// Void collapse vortex VFX — spinning dark energy around center.
        /// </summary>
        public static void HeraldVoidCollapseVFX(Vector2 center, float timer)
        {
            if (Main.dedServ) return;

            // Spinning vortex particles
            for (int i = 0; i < 4; i++)
            {
                float angle = timer * 0.15f + MathHelper.PiOver2 * i;
                float radius = 40f + MathF.Sin(timer * 0.1f + i) * 20f;
                Vector2 pos = center + angle.ToRotationVector2() * radius;
                CustomParticles.GenericFlare(pos, FatePalette.CosmicVoid, 0.4f, 8);
            }

            // Central void glow
            CustomParticles.GenericFlare(center, FatePalette.DarkPink,
                0.6f + MathF.Sin(timer * 0.2f) * 0.2f, 5);

            Lighting.AddLight(center, FatePalette.FatePurple.ToVector3() * 0.5f);
        }

        /// <summary>
        /// Fate Sealed ultimate — growing glyph circle around target during charge.
        /// </summary>
        public static void HeraldFateSealedChargeVFX(Vector2 targetCenter, float progress, int glyphCount, float maxRadius)
        {
            if (Main.dedServ) return;

            // Growing glyph circle
            for (int i = 0; i < glyphCount; i++)
            {
                float angle = MathHelper.TwoPi * i / glyphCount;
                Vector2 glyphPos = targetCenter + angle.ToRotationVector2() * (maxRadius * progress);
                Color col = Color.Lerp(FatePalette.FatePurple, FatePalette.BrightCrimson, progress);
                CustomParticles.Glyph(glyphPos, col, 0.4f + progress * 0.3f, -1);
            }

            // Converging energy
            for (int i = 0; i < 8; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float radius = 400f * (1f - progress * 0.7f);
                Vector2 pos = targetCenter + angle.ToRotationVector2() * radius;
                Vector2 vel = (targetCenter - pos).SafeNormalize(Vector2.Zero) * (5f + progress * 10f);
                Color col = Color.Lerp(FatePalette.DarkPink, FatePalette.WhiteCelestial, progress);
                var particle = new GenericGlowParticle(pos, vel, col, 0.35f, 18, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }
        }

        /// <summary>
        /// Aftershock ring particles expanding outward after Fate Sealed explosion.
        /// </summary>
        public static void HeraldAftershockRing(Vector2 center, float timer, int ringCount)
        {
            if (Main.dedServ) return;

            for (int i = 0; i < ringCount; i++)
            {
                float angle = MathHelper.TwoPi * i / ringCount;
                Vector2 pos = center + angle.ToRotationVector2() * (timer * 15f);
                CustomParticles.GenericFlare(pos, FatePalette.BrightCrimson, 0.5f, 10);
            }
        }

        // ===== HERALD PROJECTILE VFX =====

        /// <summary>
        /// Trail VFX for CosmicRendSlash projectile.
        /// </summary>
        public static void CosmicRendTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            if (Main.rand.NextBool(2))
            {
                var particle = new GenericGlowParticle(pos, -velocity * 0.1f,
                    FatePalette.DarkPink * 0.7f, 0.3f, 15, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }

            Lighting.AddLight(pos, FatePalette.DarkPink.ToVector3() * 0.4f);
        }

        /// <summary>
        /// Kill VFX for CosmicRendSlash projectile.
        /// </summary>
        public static void CosmicRendKillVFX(Vector2 pos)
        {
            if (Main.dedServ) return;
            CustomParticles.GenericFlare(pos, FatePalette.BrightCrimson, 0.6f, 15);
            FateVFXLibrary.SpawnMusicNotes(pos, 1, 8f, 0.7f, 0.85f, 14);
        }

        /// <summary>
        /// Trail VFX for StellarBolt projectile.
        /// </summary>
        public static void StellarBoltTrailVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            if (Main.rand.NextBool(3))
                CustomParticles.GenericFlare(pos, FatePalette.WhiteCelestial * 0.5f, 0.15f, 8);

            Lighting.AddLight(pos, FatePalette.DarkPink.ToVector3() * 0.3f);
        }

        /// <summary>
        /// Kill VFX for StellarBolt projectile.
        /// </summary>
        public static void StellarBoltKillVFX(Vector2 pos)
        {
            if (Main.dedServ) return;
            CustomParticles.GenericFlare(pos, FatePalette.BrightCrimson, 0.5f, 12);
            CustomParticles.HaloRing(pos, FatePalette.DarkPink, 0.3f, 10);
        }

        /// <summary>
        /// PreDraw bloom for StellarBolt projectile.
        /// </summary>
        public static void StellarBoltPreDraw(SpriteBatch sb, Texture2D tex, Vector2 drawPos, Vector2 origin, float rotation)
        {
            float pulse = 0.9f + MathF.Sin(Main.GameUpdateCount * 0.2f) * 0.1f;

            FateVFXLibrary.BeginFateAdditive(sb);

            sb.Draw(tex, drawPos, null, FatePalette.FatePurple * 0.5f, rotation, origin, 0.4f * pulse, SpriteEffects.None, 0f);
            sb.Draw(tex, drawPos, null, FatePalette.DarkPink * 0.6f, rotation, origin, 0.25f * pulse, SpriteEffects.None, 0f);
            sb.Draw(tex, drawPos, null, Color.White * 0.7f, rotation, origin, 0.12f * pulse, SpriteEffects.None, 0f);

            FateVFXLibrary.EndFateAdditive(sb);
        }

        /// <summary>
        /// Trail VFX for ConstellationBeam projectile.
        /// </summary>
        public static void ConstellationBeamTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            var particle = new GenericGlowParticle(pos, -velocity * 0.05f,
                FatePalette.BrightCrimson * 0.8f, 0.35f, 12, true);
            MagnumParticleHandler.SpawnParticle(particle);

            if (Main.rand.NextBool(3))
                CustomParticles.GenericFlare(pos, Color.White * 0.6f, 0.2f, 6);

            Lighting.AddLight(pos, FatePalette.BrightCrimson.ToVector3() * 0.5f);
        }

        /// <summary>
        /// Kill VFX for ConstellationBeam projectile.
        /// </summary>
        public static void ConstellationBeamKillVFX(Vector2 pos)
        {
            if (Main.dedServ) return;
            CustomParticles.GenericFlare(pos, FatePalette.BrightCrimson, 0.8f, 18);
            CustomParticles.HaloRing(pos, FatePalette.DarkPink, 0.5f, 15);
        }

        /// <summary>
        /// Fading trail VFX for RealityFractureTrail projectile.
        /// </summary>
        public static void RealityFractureTrailVFX(Vector2 pos, float alphaFraction)
        {
            if (Main.dedServ) return;

            if (Main.rand.NextBool(5))
            {
                CustomParticles.GenericFlare(pos + Main.rand.NextVector2Circular(20f, 20f),
                    FatePalette.FatePurple * 0.4f, 0.2f, 10);
            }

            Lighting.AddLight(pos, FatePalette.FatePurple.ToVector3() * 0.2f * alphaFraction);
        }

        /// <summary>
        /// Trail VFX for FateSealedShard projectile.
        /// </summary>
        public static void FateSealedShardTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            if (Main.rand.NextBool(4))
            {
                var particle = new GenericGlowParticle(pos, -velocity * 0.1f,
                    FatePalette.DarkPink * 0.6f, 0.2f, 12, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }

            Lighting.AddLight(pos, FatePalette.BrightCrimson.ToVector3() * 0.25f);
        }

        /// <summary>
        /// Kill VFX for FateSealedShard projectile.
        /// </summary>
        public static void FateSealedShardKillVFX(Vector2 pos)
        {
            if (Main.dedServ) return;
            CustomParticles.GenericFlare(pos, FatePalette.DarkPink, 0.4f, 10);
        }

        // =====================================================================
        //  HERALD PREDRAW HELPERS
        // =====================================================================

        /// <summary>
        /// Get afterimage trail color for Herald's PreDraw afterimages.
        /// </summary>
        public static Color HeraldAfterimageColor(float progress)
        {
            return Color.Lerp(FatePalette.DarkPink, FatePalette.FatePurple, progress) * (1f - progress) * 0.4f;
        }

        /// <summary>
        /// Get additive glow color for Herald's PreDraw glow layer.
        /// </summary>
        public static Color HeraldGlowColor(bool isEnraged, float cosmicGlow)
        {
            Color glowColor = isEnraged ? FatePalette.BrightCrimson : FatePalette.DarkPink;
            return glowColor * 0.3f * cosmicGlow;
        }

        // =====================================================================
        //  FATE WARDEN OF MELODIES — BOSS VFX
        // =====================================================================

        // ===== AMBIENT VFX =====

        /// <summary>
        /// Per-frame ambient particles for the Warden boss.
        /// Orbiting glyphs, star sparkles, cosmic cloud trail during movement.
        /// </summary>
        public static void BossAmbientVFX(Vector2 center, float glyphOrbitAngle, Vector2 velocity)
        {
            if (Main.dedServ) return;

            // Orbiting glyphs (periodic)
            if (Main.GameUpdateCount % 20 == 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    float angle = glyphOrbitAngle + MathHelper.TwoPi * i / 3f;
                    float radius = 70f + MathF.Sin(Main.GameUpdateCount * 0.03f + i) * 15f;
                    Vector2 glyphPos = center + angle.ToRotationVector2() * radius;
                    CustomParticles.Glyph(glyphPos, FatePalette.DarkPink * 0.6f, 0.35f, -1);
                }
            }

            // Star sparkles (1-in-8)
            if (Main.rand.NextBool(8))
            {
                Vector2 starOffset = Main.rand.NextVector2Circular(100f, 100f);
                CustomParticles.GenericFlare(center + starOffset, FatePalette.WhiteCelestial * 0.4f, 0.2f, 12);
            }

            // Cosmic cloud trail while moving (1-in-3)
            if (velocity.Length() > 5f && Main.rand.NextBool(3))
            {
                Vector2 cloudOffset = Main.rand.NextVector2Circular(20f, 20f);
                var cloud = new GenericGlowParticle(center + cloudOffset, -velocity * 0.1f,
                    FatePalette.FatePurple * 0.4f, 0.3f, 20, true);
                MagnumParticleHandler.SpawnParticle(cloud);
            }

            // Music notes (1-in-15)
            if (Main.rand.NextBool(15))
                FateVFXLibrary.SpawnMusicNotes(center, 1, 20f, 0.7f, 0.85f, 25);
        }

        // ===== BOSS TELEPORT VFX =====

        /// <summary>
        /// Boss teleport departure VFX — glyph burst and cosmic flash.
        /// </summary>
        public static void BossTeleportDeparture(Vector2 position)
        {
            if (Main.dedServ) return;

            CustomParticles.GenericFlare(position, FatePalette.WhiteCelestial, 1.2f, 20);
            FateVFXLibrary.SpawnGlyphBurst(position, 6, 4f);

            // Cosmic cloud puff
            for (int i = 0; i < 8; i++)
            {
                Vector2 cloudVel = Main.rand.NextVector2Circular(4f, 4f);
                var cloud = new GenericGlowParticle(position, cloudVel,
                    FatePalette.FatePurple * 0.6f, 0.4f, 25, true);
                MagnumParticleHandler.SpawnParticle(cloud);
            }
        }

        /// <summary>
        /// Boss teleport arrival VFX — cascading star burst with glyph burst.
        /// </summary>
        public static void BossTeleportArrival(Vector2 position)
        {
            if (Main.dedServ) return;

            CustomParticles.GenericFlare(position, FatePalette.WhiteCelestial, 1.5f, 28);

            // Cascading star burst arrival
            for (int i = 0; i < 6; i++)
            {
                float progress = i / 6f;
                int starsInRing = 6 + i;
                for (int star = 0; star < starsInRing; star++)
                {
                    float angle = MathHelper.TwoPi * star / starsInRing + i * 0.3f;
                    Vector2 starPos = position + angle.ToRotationVector2() * (25f + i * 12f);
                    Vector2 outVel = angle.ToRotationVector2() * (1.5f + i * 0.3f);
                    Color col = FatePalette.GetCosmicGradient(progress);
                    var starBurst = new StarBurstParticle(starPos, outVel, col, 0.25f + i * 0.04f, 15 + i * 3);
                    MagnumParticleHandler.SpawnParticle(starBurst);
                }
            }

            FateVFXLibrary.SpawnGlyphBurst(position, 8, 6f);
            FateVFXLibrary.SpawnMusicNotes(position, 2, 15f, 0.8f, 1.0f, 22);
        }

        // ===== BOSS SPAWN VFX =====

        /// <summary>
        /// Boss spawn — converging cosmic particles during spawn intro.
        /// </summary>
        public static void BossSpawnConverging(Vector2 center, float progress)
        {
            if (Main.dedServ) return;

            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                float radius = 200f * (1f - progress * 0.6f);
                Vector2 pos = center + angle.ToRotationVector2() * radius;
                CustomParticles.GenericFlare(pos, FatePalette.GetCosmicGradient(progress), 0.4f + progress * 0.3f, 18);
            }

            // Spiral glyph
            CustomParticles.Glyph(center + Main.rand.NextVector2Circular(100f * (1f - progress), 100f * (1f - progress)),
                FatePalette.DarkPink, 0.4f, -1);
        }

        /// <summary>
        /// Boss grand entrance explosion — massive star burst cascade.
        /// </summary>
        public static void BossEntranceExplosion(Vector2 center)
        {
            if (Main.dedServ) return;

            CustomParticles.GenericFlare(center, FatePalette.WhiteCelestial, 2f, 30);

            // 12-ring cascading star burst
            for (int i = 0; i < 12; i++)
            {
                float progress = i / 12f;
                int starsInRing = 8 + i;
                for (int star = 0; star < starsInRing; star++)
                {
                    float angle = MathHelper.TwoPi * star / starsInRing + i * 0.25f;
                    Vector2 starPos = center + angle.ToRotationVector2() * (30f + i * 18f);
                    Vector2 outVel = angle.ToRotationVector2() * (2f + i * 0.5f);
                    Color col = FatePalette.GetCosmicGradient(progress);
                    var starBurst = new StarBurstParticle(starPos, outVel, col, 0.25f + i * 0.05f, 18 + i * 4);
                    MagnumParticleHandler.SpawnParticle(starBurst);
                }
            }

            FateVFXLibrary.SpawnGlyphBurst(center, 12, 8f);
            FateVFXLibrary.SpawnMusicNotes(center, 6, 40f, 0.85f, 1.2f, 35);
            Lighting.AddLight(center, FatePalette.BrightCrimson.ToVector3() * 2.0f);
        }

        // ===== BOSS DIFFICULTY CHANGE VFX =====

        /// <summary>
        /// Boss difficulty tier change VFX — cosmic burst and glyph burst.
        /// </summary>
        public static void BossDifficultyChangeVFX(Vector2 center, int tier)
        {
            if (Main.dedServ) return;

            CustomParticles.GenericFlare(center, FatePalette.WhiteCelestial, 1.3f, 25);

            int particleCount = 10 + tier * 4;
            for (int i = 0; i < particleCount; i++)
            {
                float angle = MathHelper.TwoPi * i / particleCount;
                Color col = FatePalette.GetCosmicGradient((float)i / particleCount);
                CustomParticles.GenericFlare(center + angle.ToRotationVector2() * 70f, col, 0.6f, 18);
            }

            FateVFXLibrary.SpawnGlyphBurst(center, 8, 5f);
            FateVFXLibrary.SpawnMusicNotes(center, 3, 25f, 0.8f, 1.0f, 25);
        }

        // ===== BOSS STAR CASCADE (REUSABLE) =====

        /// <summary>
        /// Cascading star burst — reusable for boss attacks, awakening, death.
        /// Spawns concentric rings of StarBurstParticle with gradient colors.
        /// </summary>
        public static void BossStarCascade(Vector2 center, int ringCount, float baseRadius = 30f,
            float radiusPerRing = 15f, float baseSpeed = 2f, float speedPerRing = 0.4f)
        {
            if (Main.dedServ) return;

            for (int i = 0; i < ringCount; i++)
            {
                float progress = (float)i / ringCount;
                int starsInRing = 6 + i;
                for (int star = 0; star < starsInRing; star++)
                {
                    float angle = MathHelper.TwoPi * star / starsInRing + i * 0.25f;
                    Vector2 starPos = center + angle.ToRotationVector2() * (baseRadius + i * radiusPerRing);
                    Vector2 outVel = angle.ToRotationVector2() * (baseSpeed + i * speedPerRing);
                    Color col = FatePalette.GetCosmicGradient(progress);
                    var starBurst = new StarBurstParticle(starPos, outVel, col, 0.35f + i * 0.05f, 25);
                    MagnumParticleHandler.SpawnParticle(starBurst);
                }
            }
        }

        // ===== BOSS AWAKENING VFX =====

        /// <summary>
        /// Awakening Phase 1: Collapse — flickering, failing cosmic energy.
        /// </summary>
        public static void BossAwakeningCollapse(Vector2 center, int timer)
        {
            if (Main.dedServ) return;

            if (timer % 8 == 0)
            {
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi * i / 6f + timer * 0.02f;
                    float radius = 100f + MathF.Sin(timer * 0.3f) * 30f;
                    Vector2 pos = center + angle.ToRotationVector2() * radius;
                    Color col = FatePalette.GetCosmicGradient(Main.rand.NextFloat()) * (0.3f + Main.rand.NextFloat() * 0.4f);
                    CustomParticles.GenericFlare(pos, col, 0.3f, 12);
                }
            }
        }

        /// <summary>
        /// Awakening Phase 2: Rising defiance — intensifying converging energy.
        /// </summary>
        public static void BossAwakeningRising(Vector2 center, float progress, int timer)
        {
            if (Main.dedServ) return;

            if (timer % 4 == 0)
            {
                int particleCount = (int)(8 + progress * 20);
                for (int i = 0; i < particleCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / particleCount + timer * 0.08f;
                    float radius = 200f * (1f - progress * 0.6f);
                    Vector2 pos = center + angle.ToRotationVector2() * radius;
                    Color col = Color.Lerp(FatePalette.DarkPink, FatePalette.WhiteCelestial, progress);
                    CustomParticles.GenericFlare(pos, col, 0.4f + progress * 0.5f, 20);
                }

                // Converging glyphs
                FateVFXLibrary.SpawnGlyphCircle(center, 6, 80f * (1f - progress * 0.5f), 0.06f);
            }
        }

        /// <summary>
        /// Awakening Phase 3: True form emergence — cosmic energy spiral.
        /// </summary>
        public static void BossAwakeningEmergence(Vector2 center, float progress, int timer)
        {
            if (Main.dedServ) return;

            // Intense cosmic energy spiral
            if (timer % 3 == 0)
            {
                int spiralCount = 3;
                for (int arm = 0; arm < spiralCount; arm++)
                {
                    float baseAngle = timer * 0.12f + MathHelper.TwoPi * arm / spiralCount;
                    for (int point = 0; point < 8; point++)
                    {
                        float spiralAngle = baseAngle + point * 0.3f;
                        float spiralRadius = 30f + point * 20f;
                        Vector2 spiralPos = center + spiralAngle.ToRotationVector2() * spiralRadius;
                        Color col = FatePalette.GetCosmicGradient((float)point / 8f + progress);
                        CustomParticles.GenericFlare(spiralPos, col, 0.5f, 15);
                    }
                }
            }

            // Star particles exploding outward
            if (timer % 5 == 0)
            {
                for (int i = 0; i < 12; i++)
                {
                    Vector2 starVel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(6f, 14f);
                    var star = new GenericGlowParticle(center, starVel,
                        FatePalette.WhiteCelestial * 0.8f, 0.35f, 25, true);
                    MagnumParticleHandler.SpawnParticle(star);
                }
            }

            // Periodic star cascade rings
            if (timer % 10 == 0)
            {
                int ringIndex = timer / 10;
                BossStarCascade(center, 1, 35f + ringIndex * 15f, 0f, 2f + ringIndex * 0.4f, 0f);
            }
        }

        /// <summary>
        /// Awakening Phase 4: Rebirth explosion — massive cosmic detonation.
        /// </summary>
        public static void BossRebirthExplosion(Vector2 center)
        {
            if (Main.dedServ) return;

            // Massive white flash
            CustomParticles.GenericFlare(center, FatePalette.WhiteCelestial, 4f, 45);
            CustomParticles.GenericFlare(center, FatePalette.BrightCrimson, 3f, 40);

            // 20 cascading star burst rings — grand rebirth
            BossStarCascade(center, 20, 40f, 22f, 3f, 0.7f);

            // Massive glyph explosion
            FateVFXLibrary.SpawnGlyphBurst(center, 30, 18f);

            // Radial cosmic storm
            for (int i = 0; i < 50; i++)
            {
                float angle = MathHelper.TwoPi * i / 50f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(12f, 24f);
                Color col = FatePalette.GetCosmicGradient((float)i / 50f);
                var cosmicSpark = new GenericGlowParticle(center, vel, col, 0.6f, 40, true);
                MagnumParticleHandler.SpawnParticle(cosmicSpark);
            }

            // Music note explosion
            FateVFXLibrary.SpawnMusicNotes(center, 10, 60f, 0.9f, 1.3f, 40);

            // Constellation burst
            FateVFXLibrary.SpawnConstellationBurst(center, 10, 100f, 1.2f);

            Lighting.AddLight(center, FatePalette.SupernovaWhite.ToVector3() * 2.5f);
        }

        // ===== BOSS DEATH VFX =====

        /// <summary>
        /// Boss death buildup — converging particles and glyph circles.
        /// </summary>
        public static void BossDeathBuildup(Vector2 center, float progress, int timer)
        {
            if (Main.dedServ) return;

            if (timer % 5 == 0)
            {
                int particleCount = (int)(8 + progress * 16);
                for (int i = 0; i < particleCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / particleCount + timer * 0.05f;
                    float radius = 150f * (1f - progress * 0.5f);
                    Vector2 pos = center + angle.ToRotationVector2() * radius;
                    Color col = FatePalette.GetCosmicGradient(progress);
                    CustomParticles.GenericFlare(pos, col, 0.4f + progress * 0.4f, 18);
                }
            }

            if (timer % 10 == 0)
            {
                FateVFXLibrary.SpawnGlyphCircle(center, 8, 100f * (1f - progress * 0.4f), 0.04f);
            }
        }

        /// <summary>
        /// Boss death climax explosion — massive cascading cosmic explosion.
        /// </summary>
        public static void BossDeathExplosion(Vector2 center)
        {
            if (Main.dedServ) return;

            // Massive cosmic flash
            CustomParticles.GenericFlare(center, FatePalette.WhiteCelestial, 3f, 40);
            CustomParticles.GenericFlare(center, FatePalette.DarkPink, 2.5f, 35);
            CustomParticles.GenericFlare(center, FatePalette.BrightCrimson, 2f, 30);

            // 16-ring cascading star burst
            BossStarCascade(center, 16, 40f, 30f, 3f, 0.5f);

            // Glyph burst
            FateVFXLibrary.SpawnGlyphBurst(center, 20, 12f);

            // Radial spark storm
            for (int i = 0; i < 30; i++)
            {
                float angle = MathHelper.TwoPi * i / 30f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(8f, 16f);
                Color col = FatePalette.GetCosmicGradient((float)i / 30f);
                var spark = new GenericGlowParticle(center, vel, col, 0.5f, 35, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Music notes
            FateVFXLibrary.SpawnMusicNotes(center, 8, 50f, 0.85f, 1.2f, 35);

            // Constellation burst
            FateVFXLibrary.SpawnConstellationBurst(center, 8, 80f, 1.0f);

            Lighting.AddLight(center, FatePalette.SupernovaWhite.ToVector3() * 2.0f);
        }

        // ===== BOSS ATTACK-SPECIFIC VFX =====

        /// <summary>
        /// Boss attack release burst — used when attacks fire.
        /// Central flare + glyph accent + star sparkles.
        /// </summary>
        public static void BossAttackReleaseBurst(Vector2 center, float intensity)
        {
            if (Main.dedServ) return;

            CustomParticles.GenericFlare(center, FatePalette.BrightCrimson, 0.6f * intensity, 14);
            CustomParticles.GenericFlare(center, FatePalette.DarkPink, 0.4f * intensity, 12);
            FateVFXLibrary.SpawnStarSparkles(center, 4, 20f * intensity, 0.18f);
            FateVFXLibrary.SpawnMusicNotes(center, 2, 15f, 0.7f, 0.9f, 20);
        }

        /// <summary>
        /// Boss Universal Judgment charge VFX — converging cosmic particles.
        /// </summary>
        public static void BossJudgmentChargeVFX(Vector2 center, float progress, int timer)
        {
            if (Main.dedServ) return;

            if (timer % 4 == 0)
            {
                int particleCount = (int)(8 + progress * 12);
                for (int i = 0; i < particleCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / particleCount + timer * 0.06f;
                    float radius = 250f * (1f - progress * 0.6f);
                    Vector2 pos = center + angle.ToRotationVector2() * radius;
                    Color col = FatePalette.GetCosmicGradient(progress);
                    CustomParticles.GenericFlare(pos, col, 0.3f + progress * 0.4f, 12);
                }
            }

            // Orbiting glyphs
            if (timer % 8 == 0)
            {
                FateVFXLibrary.SpawnGlyphCircle(center, 6, 100f * (1f - progress * 0.3f), 0.03f);
            }
        }

        /// <summary>
        /// Boss Universal Judgment fire VFX — massive star cascade.
        /// </summary>
        public static void BossJudgmentFireVFX(Vector2 center)
        {
            if (Main.dedServ) return;

            CustomParticles.GenericFlare(center, FatePalette.WhiteCelestial, 1.8f, 28);

            // 8-ring cascading star burst
            BossStarCascade(center, 8, 30f, 15f, 2f, 0.4f);

            FateVFXLibrary.SpawnGlyphBurst(center, 10, 7f);
            FateVFXLibrary.SpawnMusicNotes(center, 4, 30f, 0.85f, 1.1f, 28);
        }

        /// <summary>
        /// Boss Final Melody glyph circle VFX — forming glyph circle during charge.
        /// </summary>
        public static void BossFinalMelodyGlyphs(Vector2 center, float progress, int timer)
        {
            if (Main.dedServ) return;

            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f + timer * 0.03f;
                float radius = 200f - progress * 50f;
                Vector2 pos = center + angle.ToRotationVector2() * radius;
                Color col = FatePalette.GetCosmicGradient(progress);
                CustomParticles.Glyph(pos, col, 0.5f, -1);
            }
        }

        /// <summary>
        /// Boss Final Melody grand finale VFX — the climactic explosion.
        /// </summary>
        public static void BossFinalMelodyFinale(Vector2 center)
        {
            if (Main.dedServ) return;

            CustomParticles.GenericFlare(center, FatePalette.WhiteCelestial, 2.5f, 35);

            // Epic 12-ring cascading star burst
            BossStarCascade(center, 12, 35f, 20f, 2.5f, 0.6f);

            FateVFXLibrary.SpawnGlyphBurst(center, 16, 10f);
            FateVFXLibrary.SpawnMusicNotes(center, 6, 40f, 0.9f, 1.2f, 35);
            FateVFXLibrary.SpawnConstellationBurst(center, 7, 70f, 1.0f);

            Lighting.AddLight(center, FatePalette.SupernovaWhite.ToVector3() * 2.0f);
        }

        // ===== BOSS PREDRAW HELPERS =====

        /// <summary>
        /// Get afterimage trail color for boss PreDraw afterimages.
        /// </summary>
        public static Color BossAfterimageColor(float progress)
        {
            Color col = FatePalette.GetCosmicGradient(progress) * ((1f - progress) * 0.4f);
            return col with { A = 0 };
        }

        /// <summary>
        /// Get additive glow underlay color for boss PreDraw.
        /// </summary>
        public static Color BossGlowColor()
        {
            Color glowColor = FatePalette.BrightCrimson * 0.3f;
            return glowColor with { A = 0 };
        }
    }
}
