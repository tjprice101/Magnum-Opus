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
    /// VFX helper for Light of the Future — ranged sniper weapon (680 dmg).
    /// Fires slow rounds that accelerate, pierce, and explode into 3 homing rockets.
    /// Handles hold-item ambient, item bloom, muzzle flash, bullet trail (accelerating),
    /// bullet explosion, and homing rocket trail effects.
    /// Call from LightOfTheFuture, FutureBulletProjectile, and HomingFutureRocket.
    /// </summary>
    public static class LightOfTheFutureVFX
    {
        // Maximum bullet speed for trail intensity scaling
        private const float MaxBulletSpeed = 28f;

        // ===== HOLD ITEM VFX =====

        /// <summary>
        /// Per-frame held-item VFX: spiraling energy particles, cosmic cloud wisps,
        /// orbiting glyphs, star particles, and accelerating intensity light.
        /// The future's light builds momentum before firing.
        /// </summary>
        public static void HoldItemVFX(Player player)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Spiraling energy particles — rising spiral around barrel
            if (Main.rand.NextBool(5))
            {
                float spiralAngle = time * 0.06f + Main.rand.NextFloat(MathHelper.TwoPi);
                float spiralRadius = 15f + MathF.Sin(time * 0.04f) * 5f;
                Vector2 muzzleBase = center + new Vector2(player.direction * 28f, -6f);
                Vector2 spiralPos = muzzleBase + spiralAngle.ToRotationVector2() * spiralRadius;
                Color spiralCol = FatePalette.PaletteLerp(FatePalette.FutureLight, Main.rand.NextFloat(0.2f, 0.7f));
                var spiral = new GenericGlowParticle(spiralPos,
                    Main.rand.NextVector2Circular(0.5f, 0.5f) + new Vector2(0, -0.3f),
                    spiralCol * 0.45f, 0.14f, 14, true);
                MagnumParticleHandler.SpawnParticle(spiral);
            }

            // Cosmic cloud wisps — nebula mist near the wielder
            if (Main.rand.NextBool(12))
                FateVFXLibrary.SpawnCosmicCloudTrail(center + Main.rand.NextVector2Circular(20f, 20f),
                    Vector2.Zero, 0.25f);

            // Orbiting glyphs — fate runes circling
            if (Main.rand.NextBool(10))
            {
                float glyphAngle = time * 0.03f;
                Vector2 glyphPos = center + glyphAngle.ToRotationVector2() * 35f;
                Color glyphCol = FatePalette.GetCosmicGradient(Main.rand.NextFloat(0.3f, 0.7f));
                CustomParticles.Glyph(glyphPos, glyphCol * 0.4f, 0.18f, -1);
            }

            // Star particles — celestial motes
            if (Main.rand.NextBool(9))
            {
                Vector2 starPos = center + Main.rand.NextVector2Circular(30f, 30f);
                Color starCol = Main.rand.NextBool(3) ? FatePalette.StarGold : FatePalette.WhiteCelestial;
                var star = new GenericGlowParticle(starPos,
                    Main.rand.NextVector2Circular(0.4f, 0.4f),
                    starCol * 0.35f, 0.12f, 14, true);
                MagnumParticleHandler.SpawnParticle(star);
            }

            // Music notes — the future's overture
            if (Main.rand.NextBool(12))
                FateVFXLibrary.SpawnMusicNotes(center, 1, 15f, 0.65f, 0.85f, 25);

            // Accelerating intensity light — builds anticipation
            float pulse = 0.2f + MathF.Sin(time * 0.08f) * 0.12f;
            Lighting.AddLight(center, FatePalette.DestinyFlame.ToVector3() * pulse);
        }

        // ===== PREDRAW IN WORLD BLOOM =====

        /// <summary>
        /// Standard 3-layer Fate item bloom for the sniper weapon sprite.
        /// Uses DrawItemBloom for consistent cosmic glow across all Fate items.
        /// </summary>
        public static void PreDrawInWorldBloom(
            SpriteBatch sb,
            Texture2D tex,
            Vector2 pos, Vector2 origin, float rotation, float scale)
        {
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.04f) * 0.06f;
            FatePalette.DrawItemBloom(sb, tex, pos, origin, rotation, scale, pulse);
        }

        // ===== MUZZLE FLASH VFX =====

        /// <summary>
        /// Muzzle flash VFX when the sniper fires. Projectile impact at 0.5 scale,
        /// directional glyph burst, cosmic cloud at muzzle, and bright muzzle light.
        /// The future illuminates the present with cosmic fire.
        /// </summary>
        public static void MuzzleFlashVFX(Vector2 muzzlePos, Vector2 direction)
        {
            if (Main.dedServ) return;

            Vector2 normDir = direction.SafeNormalize(Vector2.UnitX);

            // Projectile-scale impact flash at muzzle
            FateVFXLibrary.ProjectileImpact(muzzlePos, 0.5f);

            // Directional glyph burst — runes erupting along the shot axis
            for (int i = 0; i < 4; i++)
            {
                Vector2 glyphPos = muzzlePos + normDir.RotatedByRandom(0.4f) * (10f + i * 8f);
                Color glyphCol = FatePalette.PaletteLerp(FatePalette.FutureLight, (float)i / 4f);
                CustomParticles.Glyph(glyphPos, glyphCol * 0.6f, 0.22f, -1);
            }

            // Cosmic cloud at muzzle — nebula discharge
            FateVFXLibrary.SpawnCosmicCloudTrail(muzzlePos, normDir * 3f, 0.6f);

            // Directional dust cone — tight but intense
            for (int i = 0; i < 5; i++)
            {
                Vector2 dustVel = normDir.RotatedByRandom(0.3f) * Main.rand.NextFloat(4f, 8f);
                Color dustCol = FatePalette.GetRevelationGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(muzzlePos, DustID.PinkTorch, dustVel, 0, dustCol, 1.2f);
                d.noGravity = true;
            }

            // Central muzzle flare — bright destiny fire
            CustomParticles.GenericFlare(muzzlePos, FatePalette.DestinyFlame, 0.6f, 16);
            CustomParticles.HaloRing(muzzlePos, FatePalette.StarGold * 0.6f, 0.3f, 12);

            // Music notes — the shot breaks the silence
            FateVFXLibrary.SpawnMusicNotes(muzzlePos, 2, 12f, 0.8f, 1.0f, 22);

            // Bright muzzle light
            Lighting.AddLight(muzzlePos, FatePalette.StarGold.ToVector3() * 0.9f);
        }

        // ===== BULLET TRAIL VFX =====

        /// <summary>
        /// Per-frame accelerating bullet trail VFX. More particles spawn at higher speed.
        /// GenericGlowParticle using FutureLight palette lerp based on speed/maxSpeed.
        /// Star sparkle accents at high velocity. The future accelerates toward destiny.
        /// </summary>
        public static void BulletTrailVFX(Vector2 pos, Vector2 velocity, float speed)
        {
            if (Main.dedServ) return;

            Vector2 awayDir = -velocity.SafeNormalize(Vector2.Zero);
            float speedRatio = MathHelper.Clamp(speed / MaxBulletSpeed, 0f, 1f);

            // Primary glow trail — intensity scales with speed
            int trailCount = 1 + (int)(speedRatio * 2f);
            for (int i = 0; i < trailCount; i++)
            {
                Color trailCol = FatePalette.PaletteLerp(FatePalette.FutureLight, speedRatio);
                float trailScale = 0.15f + speedRatio * 0.12f;
                var trail = new GenericGlowParticle(pos + Main.rand.NextVector2Circular(4f, 4f),
                    awayDir * Main.rand.NextFloat(0.5f, 1.5f) + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    trailCol * 0.55f, trailScale, 12, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            // Star sparkle accents at high velocity
            if (speedRatio > 0.5f && Main.rand.NextBool(4))
            {
                Color starCol = Main.rand.NextBool(3) ? FatePalette.StarGold : FatePalette.WhiteCelestial;
                var star = new GenericGlowParticle(pos + Main.rand.NextVector2Circular(6f, 6f),
                    Main.rand.NextVector2Circular(0.5f, 0.5f),
                    starCol * 0.5f * speedRatio, 0.14f, 10, true);
                MagnumParticleHandler.SpawnParticle(star);
            }

            // Accelerating dust trail — denser at higher speed
            if (Main.rand.NextBool(2))
            {
                Color dustCol = FatePalette.GetRevelationGradient(speedRatio);
                Dust d = Dust.NewDustPerfect(pos, DustID.PinkTorch,
                    awayDir * Main.rand.NextFloat(1f, 2f), 0, dustCol, 0.9f + speedRatio * 0.4f);
                d.noGravity = true;
            }

            // Cosmic cloud trail at high speed (1-in-5)
            if (speedRatio > 0.6f && Main.rand.NextBool(5))
                FateVFXLibrary.SpawnCosmicCloudTrail(pos, velocity, 0.3f);

            // Glyph accent (1-in-8)
            if (Main.rand.NextBool(8))
                FateVFXLibrary.SpawnGlyphAccent(pos, 0.15f);

            // Music note trail (1-in-7)
            if (Main.rand.NextBool(7))
                FateVFXLibrary.SpawnMusicNotes(pos, 1, 6f, 0.65f, 0.85f, 18);

            // Dynamic light intensity scales with speed
            Lighting.AddLight(pos, FatePalette.DestinyFlame.ToVector3() * (0.25f + speedRatio * 0.35f));
        }

        // ===== BULLET EXPLOSION VFX =====

        /// <summary>
        /// VFX when the bullet explodes on impact, splitting into homing rockets.
        /// FinisherSlam at 0.8 scale plus additional cosmic cloud burst
        /// and constellation burst. The future arrives — now.
        /// </summary>
        public static void BulletExplosionVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            // Finisher slam as explosion base — massive cosmic detonation
            FateVFXLibrary.FinisherSlam(pos, 0.8f);

            // Additional cosmic cloud burst — nebula shockwave
            FateVFXLibrary.SpawnCosmicCloudBurst(pos, 0.7f, 14);

            // Constellation burst — the future's star map shatters
            FateVFXLibrary.SpawnConstellationBurst(pos, 6, 55f, 0.8f);

            // Central bright flash — destiny fire detonation
            CustomParticles.GenericFlare(pos, FatePalette.StarGold, 0.7f, 18);
            CustomParticles.GenericFlare(pos, FatePalette.DestinyFlame, 0.55f, 20);

            // Expanding gradient ring — shockwave of light
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 7f);
                Color ringCol = FatePalette.PaletteLerp(FatePalette.FutureLight, (float)i / 10f);
                var ring = new GenericGlowParticle(pos, vel,
                    ringCol * 0.7f, 0.3f, 18, true);
                MagnumParticleHandler.SpawnParticle(ring);
            }

            // Three directional flares — one for each homing rocket
            for (int i = 0; i < 3; i++)
            {
                float angle = MathHelper.TwoPi * i / 3f;
                Vector2 flarePos = pos + angle.ToRotationVector2() * 18f;
                CustomParticles.GenericFlare(flarePos, FatePalette.DestinyFlame, 0.4f, 14);
            }

            // Music notes — the explosion sings destiny
            FateVFXLibrary.SpawnMusicNotes(pos, 4, 25f, 0.8f, 1.0f, 28);

            Lighting.AddLight(pos, FatePalette.StarGold.ToVector3() * 1.3f);
        }

        // ===== HOMING ROCKET TRAIL VFX =====

        /// <summary>
        /// Per-frame trail VFX for homing rockets spawned from bullet explosion.
        /// Smaller trail using DestinyFlame to StarGold gradient,
        /// spark particles, and occasional music note.
        /// Each rocket carries a fragment of the future's purpose.
        /// </summary>
        public static void HomingRocketTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 awayDir = -velocity.SafeNormalize(Vector2.Zero);

            // Primary glow trail — DestinyFlame to StarGold gradient
            if (Main.rand.NextBool(2))
            {
                float gradientT = (Main.GameUpdateCount * 0.03f) % 1f;
                Color trailCol = Color.Lerp(FatePalette.DestinyFlame, FatePalette.StarGold, gradientT);
                var trail = new GenericGlowParticle(pos + Main.rand.NextVector2Circular(3f, 3f),
                    awayDir * Main.rand.NextFloat(0.5f, 1.2f) + Main.rand.NextVector2Circular(0.4f, 0.4f),
                    trailCol * 0.5f, 0.15f, 12, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            // Spark particles — fiery rocket exhaust
            if (Main.rand.NextBool(3))
            {
                Vector2 sparkVel = awayDir * Main.rand.NextFloat(1f, 2.5f) + Main.rand.NextVector2Circular(0.8f, 0.8f);
                var spark = new GlowSparkParticle(pos, sparkVel,
                    FatePalette.DestinyFlame * 0.6f, 0.12f, 10);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Fire dust trail
            if (Main.rand.NextBool(3))
            {
                Color dustCol = Color.Lerp(FatePalette.DestinyFlame, FatePalette.StarGold, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.RedTorch,
                    awayDir * Main.rand.NextFloat(0.5f, 1.5f), 0, dustCol, 0.8f);
                d.noGravity = true;
            }

            // Occasional music note — the rocket hums its purpose (1-in-8)
            if (Main.rand.NextBool(8))
                FateVFXLibrary.SpawnMusicNotes(pos, 1, 5f, 0.6f, 0.75f, 14);

            Lighting.AddLight(pos, FatePalette.DestinyFlame.ToVector3() * 0.3f);
        }

        // ===== ROCKET IMPACT VFX =====

        /// <summary>
        /// On-hit VFX when a homing rocket strikes an enemy.
        /// Smaller explosion with melee impact base, fire dust burst, and flare.
        /// </summary>
        public static void RocketImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            // Melee impact as base
            FateVFXLibrary.MeleeImpact(pos, 0);

            // Fire-colored flare
            CustomParticles.GenericFlare(pos, FatePalette.DestinyFlame, 0.45f, 14);
            CustomParticles.HaloRing(pos, FatePalette.StarGold * 0.6f, 0.25f, 12);

            // Fire dust burst
            for (int i = 0; i < 6; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(4f, 4f);
                Color dustCol = Color.Lerp(FatePalette.DestinyFlame, FatePalette.StarGold, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.RedTorch, dustVel, 0, dustCol, 1.1f);
                d.noGravity = true;
            }

            // Star sparkles at impact
            FateVFXLibrary.SpawnStarSparkles(pos, 3, 15f, 0.18f);

            // Music note
            FateVFXLibrary.SpawnMusicNotes(pos, 1, 10f, 0.7f, 0.9f, 20);

            Lighting.AddLight(pos, FatePalette.DestinyFlame.ToVector3() * 0.7f);
        }

        // ===== TRAIL RENDERING FUNCTIONS =====

        /// <summary>
        /// Trail color function for sniper bullet projectile trails.
        /// Uses FutureLight palette with additive-safe output.
        /// </summary>
        public static Color FutureTrailColor(float completionRatio)
        {
            Color c = FatePalette.PaletteLerp(FatePalette.FutureLight,
                0.2f + completionRatio * 0.6f);
            float fade = 1f - MathF.Pow(completionRatio, 1.3f);
            return (c * fade) with { A = 0 };
        }

        /// <summary>
        /// Trail width function for sniper bullet trails.
        /// Cosmic beam width with thin precision taper.
        /// </summary>
        public static float FutureTrailWidth(float completionRatio)
            => FateVFXLibrary.CosmicBeamWidth(completionRatio, 10f);

        /// <summary>
        /// Trail color function for homing rocket trails.
        /// DestinyFlame to StarGold gradient with additive-safe output.
        /// </summary>
        public static Color RocketTrailColor(float completionRatio)
        {
            Color c = Color.Lerp(FatePalette.DestinyFlame, FatePalette.StarGold, completionRatio);
            float fade = 1f - MathF.Pow(completionRatio, 1.5f);
            return (c * fade) with { A = 0 };
        }

        /// <summary>
        /// Trail width function for homing rocket trails.
        /// Narrower than main bullet for the smaller rockets.
        /// </summary>
        public static float RocketTrailWidth(float completionRatio)
            => FateVFXLibrary.CosmicBeamWidth(completionRatio, 7f);
    }
}
