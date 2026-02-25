using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Bloom;
using MagnumOpus.Common.Systems.VFX.Screen;

namespace MagnumOpus.Content.OdeToJoy.Weapons.Ranged
{
    /// <summary>
    /// Per-weapon VFX helper for all 3 Ode to Joy ranged weapons.
    /// Each method is a self-contained VFX call-site using OdeToJoyPalette colors,
    /// OdeToJoyVFXLibrary impacts/dust/music-notes, and {A=0} additive bloom.
    ///
    /// Weapons covered:
    ///   1. ThePollinator       — Seed launcher, homing pollen seeds burst into petals (3200 dmg)
    ///   2. PetalStormCannon    — Heavy cannon, petal bombs create lingering storms (4800 dmg)
    ///   3. ThornSprayRepeater  — Fast thorn crossbow, thorns stick and explode (2400 dmg)
    /// </summary>
    public static class OdeToJoyRangedVFX
    {
        // =================================================================
        //  1. THE POLLINATOR — golden pollen clouds, seed bursts, nature's spreading
        // =================================================================

        #region ThePollinator

        /// <summary>
        /// Per-frame ambient VFX while the player holds The Pollinator.
        /// Gentle golden pollen sparkles drift around the weapon.
        /// </summary>
        public static void PollinatorHoldItemVFX(Player player)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Pollen sparkle motes drifting upward
            if (Main.rand.NextBool(6))
            {
                Vector2 pos = center + Main.rand.NextVector2Circular(18f, 18f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -0.8f - Main.rand.NextFloat(0.5f));
                Color col = Color.Lerp(OdeToJoyPalette.GoldenPollen, OdeToJoyPalette.SunlightYellow, Main.rand.NextFloat());
                var sparkle = new GenericGlowParticle(pos, vel, col * 0.4f, 0.14f, 18, true);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // Occasional green growth mote
            if (Main.rand.NextBool(10))
            {
                Vector2 pos = center + Main.rand.NextVector2Circular(14f, 14f);
                Vector2 vel = new Vector2(0, -0.4f) + Main.rand.NextVector2Circular(0.3f, 0.3f);
                Color col = OdeToJoyPalette.BudGreen * 0.35f;
                var mote = new GenericGlowParticle(pos, vel, col, 0.10f, 16, true);
                MagnumParticleHandler.SpawnParticle(mote);
            }

            // Music note (rare)
            if (Main.rand.NextBool(14))
                OdeToJoyVFXLibrary.SpawnMusicNotes(center, 1, 12f, 0.65f, 0.8f, 22);

            // Pulsing warm light
            float pulse = 0.25f + MathF.Sin(time * 0.05f) * 0.08f;
            Lighting.AddLight(center, OdeToJoyPalette.GoldenPollen.ToVector3() * pulse);
        }

        /// <summary>
        /// 3-layer additive item bloom for The Pollinator in the world.
        /// Uses OdeToJoyPalette.DrawItemBloom for canonical glow.
        /// </summary>
        public static void PollinatorPreDrawInWorldBloom(
            SpriteBatch sb, Texture2D tex, Vector2 pos, Vector2 origin, float rotation, float scale)
        {
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.04f) * 0.06f;
            OdeToJoyPalette.DrawItemBloom(sb, tex, pos, origin, rotation, scale, pulse);
        }

        /// <summary>
        /// Muzzle flash on every shot of The Pollinator.
        /// BlossomImpact + petal music notes + pollen sparkles.
        /// </summary>
        public static void PollinatorMuzzleFlashVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            OdeToJoyVFXLibrary.BlossomImpact(pos, 0.5f);
            OdeToJoyVFXLibrary.SpawnPetalMusicNotes(pos, 2, 15f);
            OdeToJoyVFXLibrary.SpawnPollenSparkles(pos, 4, 20f);

            Lighting.AddLight(pos, OdeToJoyPalette.GoldenPollen.ToVector3() * 0.7f);
        }

        /// <summary>
        /// Enhanced burst VFX on every 4th shot of The Pollinator.
        /// GardenImpact + MusicNoteBurst for a celebratory pollen explosion.
        /// </summary>
        public static void PollinatorBurstShotVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            OdeToJoyVFXLibrary.GardenImpact(pos, 0.8f);
            OdeToJoyVFXLibrary.MusicNoteBurst(pos, OdeToJoyPalette.GoldenPollen, 6, 4f);

            Lighting.AddLight(pos, OdeToJoyPalette.SunlightYellow.ToVector3() * 1.0f);
        }

        /// <summary>
        /// Per-frame trail VFX for pollen seed projectiles.
        /// Green-gold GenericGlowParticle gradient + GoldCoin dust pollen.
        /// </summary>
        public static void PollenSeedTrailVFX(Vector2 center, Vector2 velocity)
        {
            if (Main.dedServ) return;

            // Green-gold gradient glow trail
            if (Main.rand.NextBool(2))
            {
                Color trailCol = Color.Lerp(OdeToJoyPalette.VerdantGreen, OdeToJoyPalette.GoldenPollen, Main.rand.NextFloat());
                var trail = new GenericGlowParticle(
                    center + Main.rand.NextVector2Circular(5f, 5f),
                    -velocity * 0.1f + Main.rand.NextVector2Circular(0.3f, 0.3f),
                    trailCol * 0.6f, 0.22f, 14, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            // GoldCoin dust — drifting pollen
            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustPerfect(center, DustID.GoldCoin,
                    Main.rand.NextVector2Circular(1f, 1f), 100, OdeToJoyPalette.GoldenPollen, 0.8f);
                d.noGravity = true;
            }

            Lighting.AddLight(center, OdeToJoyPalette.GoldenPollen.ToVector3() * 0.4f);
        }

        /// <summary>
        /// PreDraw bloom for pollen seed projectiles.
        /// 3-layer additive: GoldenPollen outer, VerdantGreen mid, White core, pulsing.
        /// </summary>
        public static void PollenSeedPreDraw(
            SpriteBatch sb, Texture2D tex, Vector2 drawPos, Vector2 origin, float rotation, float scale)
        {
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.15f) * 0.15f;

            // Layer 1: Outer golden pollen aura
            sb.Draw(tex, drawPos, null,
                (OdeToJoyPalette.GoldenPollen with { A = 0 }) * 0.45f, rotation, origin,
                scale * 1.10f * pulse, SpriteEffects.None, 0f);

            // Layer 2: Mid verdant green glow
            sb.Draw(tex, drawPos, null,
                (OdeToJoyPalette.VerdantGreen with { A = 0 }) * 0.35f, rotation * 0.7f, origin,
                scale * 1.05f * pulse, SpriteEffects.None, 0f);

            // Layer 3: White-hot core
            sb.Draw(tex, drawPos, null,
                (Color.White with { A = 0 }) * 0.55f, rotation * 1.3f, origin,
                scale * 0.85f * pulse, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Death/burst VFX when a pollen seed explodes into homing petals.
        /// ProjectileImpact + MusicNoteBurst for a full petal cascade.
        /// </summary>
        public static void PollenSeedBurstVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            OdeToJoyVFXLibrary.ProjectileImpact(pos, 1f);
            OdeToJoyVFXLibrary.MusicNoteBurst(pos, OdeToJoyPalette.GoldenPollen, 5, 4f);

            Lighting.AddLight(pos, OdeToJoyPalette.GoldenPollen.ToVector3() * 1.0f);
        }

        /// <summary>
        /// Per-frame trail VFX for homing petal sub-projectiles.
        /// PetalGradient glow particles trailing behind each petal.
        /// </summary>
        public static void HomingPetalTrailVFX(Vector2 center, Vector2 velocity)
        {
            if (Main.dedServ) return;

            if (Main.rand.NextBool(2))
            {
                Color glowCol = OdeToJoyPalette.GetPetalGradient(Main.rand.NextFloat());
                var trail = new GenericGlowParticle(
                    center + Main.rand.NextVector2Circular(3f, 3f),
                    -velocity * 0.08f + Main.rand.NextVector2Circular(0.4f, 0.4f),
                    glowCol * 0.55f, 0.18f, 12, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            Lighting.AddLight(center, OdeToJoyPalette.RosePink.ToVector3() * 0.3f);
        }

        /// <summary>
        /// On-hit impact VFX for homing petal sub-projectiles.
        /// BlossomImpact at moderate scale.
        /// </summary>
        public static void HomingPetalImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            OdeToJoyVFXLibrary.BlossomImpact(pos, 0.5f);

            Lighting.AddLight(pos, OdeToJoyPalette.RosePink.ToVector3() * 0.6f);
        }

        /// <summary>
        /// Death VFX for homing petal sub-projectiles.
        /// Rose petals scatter + quick bloom burst.
        /// </summary>
        public static void HomingPetalDeathVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            OdeToJoyVFXLibrary.SpawnRosePetals(pos, 3, 20f);
            OdeToJoyVFXLibrary.BloomBurst(pos, 0.5f);

            Lighting.AddLight(pos, OdeToJoyPalette.RosePink.ToVector3() * 0.5f);
        }

        #endregion

        // =================================================================
        //  2. PETAL STORM CANNON — massive pink/rose explosions, swirling petal storms
        // =================================================================

        #region PetalStormCannon

        /// <summary>
        /// 3-layer additive item bloom for the Petal Storm Cannon in the world.
        /// Uses OdeToJoyPalette.DrawItemBloom for canonical glow.
        /// </summary>
        public static void CannonPreDrawInWorldBloom(
            SpriteBatch sb, Texture2D tex, Vector2 pos, Vector2 origin, float rotation, float scale)
        {
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.04f) * 0.06f;
            OdeToJoyPalette.DrawItemBloom(sb, tex, pos, origin, rotation, scale, pulse);
        }

        /// <summary>
        /// Massive muzzle flash for the Petal Storm Cannon.
        /// GardenImpact + directional sparks + music notes + petal glow spread.
        /// </summary>
        public static void CannonMuzzleFlashVFX(Vector2 pos, Vector2 direction)
        {
            if (Main.dedServ) return;

            // Massive garden impact
            OdeToJoyVFXLibrary.GardenImpact(pos, 0.8f);

            // Directional sparks from muzzle along fire direction
            OdeToJoyVFXLibrary.SpawnDirectionalSparks(pos, direction, 6, 6f);

            // Music notes
            OdeToJoyVFXLibrary.SpawnMusicNotes(pos, 5, 30f, 0.8f, 1.1f, 30);

            // 8 petal gradient glow particles spread in firing direction
            for (int i = 0; i < 8; i++)
            {
                float spreadAngle = Main.rand.NextFloat(-0.8f, 0.8f);
                Vector2 vel = direction.RotatedBy(spreadAngle) * Main.rand.NextFloat(2f, 5f);
                Color col = OdeToJoyPalette.GetPetalGradient(Main.rand.NextFloat());
                var glow = new GenericGlowParticle(pos, vel, col * 0.65f, 0.35f, 22, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            Lighting.AddLight(pos, OdeToJoyPalette.RosePink.ToVector3() * 1.0f);
        }

        /// <summary>
        /// Per-frame trail VFX for petal bomb projectiles.
        /// 2 garden gradient GenericGlowParticles + SparkleParticle with GoldenPollen.
        /// </summary>
        public static void PetalBombTrailVFX(Vector2 center, Vector2 velocity)
        {
            if (Main.dedServ) return;

            // 2 garden gradient glow particles
            for (int i = 0; i < 2; i++)
            {
                Color trailCol = OdeToJoyPalette.GetGardenGradient(Main.rand.NextFloat());
                var trail = new GenericGlowParticle(
                    center + Main.rand.NextVector2Circular(8f, 8f),
                    -velocity * 0.12f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    trailCol * 0.55f, 0.30f, 18, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            // Golden pollen sparkle
            if (Main.rand.NextBool(2))
            {
                var sparkle = new SparkleParticle(
                    center + Main.rand.NextVector2Circular(10f, 10f),
                    Main.rand.NextVector2Circular(1f, 1f),
                    OdeToJoyPalette.GoldenPollen, 0.28f, 15);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            Lighting.AddLight(center, OdeToJoyPalette.RosePink.ToVector3() * 0.5f);
        }

        /// <summary>
        /// PreDraw bloom for petal bomb projectiles.
        /// 3-layer additive: RosePink outer, VerdantGreen mid, White core pulsing.
        /// </summary>
        public static void PetalBombPreDraw(
            SpriteBatch sb, Texture2D tex, Vector2 drawPos, Vector2 origin, float rotation, float scale)
        {
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.12f) * 0.18f;

            // Layer 1: Outer rose pink aura
            sb.Draw(tex, drawPos, null,
                (OdeToJoyPalette.RosePink with { A = 0 }) * 0.50f, rotation, origin,
                scale * 1.12f * pulse, SpriteEffects.None, 0f);

            // Layer 2: Mid verdant green glow
            sb.Draw(tex, drawPos, null,
                (OdeToJoyPalette.VerdantGreen with { A = 0 }) * 0.40f, -rotation * 0.7f, origin,
                scale * 1.05f * pulse, SpriteEffects.None, 0f);

            // Layer 3: White-hot core
            sb.Draw(tex, drawPos, null,
                (Color.White with { A = 0 }) * 0.60f, rotation * 1.2f, origin,
                scale * 0.80f * pulse, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Massive explosion VFX when a petal bomb detonates.
        /// FinisherSlam at full intensity for massive bloom + god rays.
        /// </summary>
        public static void PetalBombExplosionVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            OdeToJoyVFXLibrary.FinisherSlam(pos, 1.0f);

            Lighting.AddLight(pos, OdeToJoyPalette.WhiteBloom.ToVector3() * 1.5f);
        }

        /// <summary>
        /// Per-frame VFX for the lingering petal storm AoE zone.
        /// Swirling petal particles at radius scaled by lifePercent,
        /// golden sparkles, and occasional music notes.
        /// </summary>
        public static void LingeringStormVFX(Vector2 center, float lifePercent)
        {
            if (Main.dedServ) return;

            // Swirling petal particles at radius proportional to remaining life
            if (Main.rand.NextBool(2))
            {
                float angle = (float)Main.timeForVisualEffects * 0.1f + Main.rand.NextFloat() * MathHelper.TwoPi;
                float radius = Main.rand.NextFloat(30f, 100f) * lifePercent;
                Vector2 offset = angle.ToRotationVector2() * radius;
                Vector2 vel = offset.RotatedBy(MathHelper.PiOver2).SafeNormalize(Vector2.Zero) * 3f;

                Color petalCol = OdeToJoyPalette.GetPetalGradient(Main.rand.NextFloat());
                var petal = new GenericGlowParticle(
                    center + offset, vel,
                    petalCol * 0.55f * lifePercent, 0.30f, 20, true);
                MagnumParticleHandler.SpawnParticle(petal);
            }

            // Golden sparkles
            if (Main.rand.NextBool(3))
            {
                var sparkle = new SparkleParticle(
                    center + Main.rand.NextVector2Circular(80f * lifePercent, 80f * lifePercent),
                    Main.rand.NextVector2Circular(2f, 2f),
                    OdeToJoyPalette.GoldenPollen * lifePercent, 0.25f, 15);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // Music notes (occasional)
            if (Main.rand.NextBool(8))
                OdeToJoyVFXLibrary.SpawnMusicNotes(center + Main.rand.NextVector2Circular(60f, 60f), 1, 10f, 0.65f, 0.85f, 22);

            Lighting.AddLight(center, OdeToJoyPalette.RosePink.ToVector3() * 0.45f * lifePercent);
        }

        /// <summary>
        /// Impact VFX when the lingering petal storm hits an NPC.
        /// BlossomImpact at moderate scale.
        /// </summary>
        public static void LingeringStormImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            OdeToJoyVFXLibrary.BlossomImpact(pos, 0.4f);

            Lighting.AddLight(pos, OdeToJoyPalette.RosePink.ToVector3() * 0.5f);
        }

        #endregion

        // =================================================================
        //  3. THORN SPRAY REPEATER — rapid green thorn trails, accumulating vine explosions
        // =================================================================

        #region ThornSprayRepeater

        /// <summary>
        /// 3-layer additive item bloom for the Thorn Spray Repeater in the world.
        /// Uses OdeToJoyPalette.DrawItemBloom for canonical glow.
        /// </summary>
        public static void RepeaterPreDrawInWorldBloom(
            SpriteBatch sb, Texture2D tex, Vector2 pos, Vector2 origin, float rotation, float scale)
        {
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.04f) * 0.06f;
            OdeToJoyPalette.DrawItemBloom(sb, tex, pos, origin, rotation, scale, pulse);
        }

        /// <summary>
        /// Small fast muzzle flash for the rapid-fire Thorn Spray Repeater.
        /// Directional sparks + vine trail dust, kept light for high fire rate.
        /// </summary>
        public static void RepeaterMuzzleFlashVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            // Small directional sparks (few, to stay light at high fire rate)
            Vector2 dir = Main.rand.NextVector2Unit();
            OdeToJoyVFXLibrary.SpawnDirectionalSparks(pos, dir, 3, 4f);

            // Vine trail dust at muzzle
            OdeToJoyVFXLibrary.SpawnVineTrailDust(pos, dir * 3f);

            Lighting.AddLight(pos, OdeToJoyPalette.VerdantGreen.ToVector3() * 0.5f);
        }

        /// <summary>
        /// Per-frame trail VFX for flying thorn bolt projectiles.
        /// Thin green GenericGlowParticle trail.
        /// </summary>
        public static void ThornBoltTrailVFX(Vector2 center, Vector2 velocity)
        {
            if (Main.dedServ) return;

            if (Main.rand.NextBool(2))
            {
                Color trailCol = Color.Lerp(OdeToJoyPalette.VerdantGreen, OdeToJoyPalette.LeafGreen, Main.rand.NextFloat());
                var trail = new GenericGlowParticle(
                    center + Main.rand.NextVector2Circular(3f, 3f),
                    -velocity * 0.08f + Main.rand.NextVector2Circular(0.3f, 0.3f),
                    trailCol * 0.55f, 0.16f, 10, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            Lighting.AddLight(center, OdeToJoyPalette.VerdantGreen.ToVector3() * 0.25f);
        }

        /// <summary>
        /// Per-frame VFX while a thorn is stuck in an enemy.
        /// Pulsing golden warning particles that increase at warningProgress approaches 1.
        /// warningProgress: 0 = just stuck, 1 = about to explode.
        /// </summary>
        public static void ThornBoltStuckVFX(Vector2 pos, float warningProgress)
        {
            if (Main.dedServ) return;

            // Increasing glow intensity as explosion nears
            float intensity = warningProgress * warningProgress; // Quadratic ramp
            int spawnChance = Math.Max(1, (int)(6 - warningProgress * 4)); // More frequent near detonation

            if (Main.rand.NextBool(spawnChance))
            {
                Color warnCol = Color.Lerp(OdeToJoyPalette.VerdantGreen, OdeToJoyPalette.GoldenPollen, warningProgress);
                var glow = new GenericGlowParticle(
                    pos + Main.rand.NextVector2Circular(5f, 5f),
                    Main.rand.NextVector2Circular(1.5f, 1.5f),
                    warnCol * (0.3f + intensity * 0.5f),
                    0.15f + intensity * 0.15f,
                    10, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // Light ramps from green to golden as warning progresses
            Color lightCol = Color.Lerp(OdeToJoyPalette.VerdantGreen, OdeToJoyPalette.GoldenPollen, warningProgress);
            Lighting.AddLight(pos, lightCol.ToVector3() * (0.2f + intensity * 0.4f));
        }

        /// <summary>
        /// Explosion VFX when stuck thorns detonate, scaled by number of thorns.
        /// GardenImpact intensity + MusicNoteBurst + radial dust burst.
        /// </summary>
        public static void ThornBoltExplodeVFX(Vector2 pos, int thornCount)
        {
            if (Main.dedServ) return;

            // Scale impact with thorn count — more thorns = bigger explosion
            float impactScale = 0.5f + thornCount * 0.15f;
            impactScale = MathHelper.Clamp(impactScale, 0.5f, 2.0f);

            OdeToJoyVFXLibrary.GardenImpact(pos, impactScale);
            OdeToJoyVFXLibrary.MusicNoteBurst(pos, OdeToJoyPalette.VerdantGreen, 4 + thornCount, 4f + thornCount * 0.5f);
            OdeToJoyVFXLibrary.SpawnRadialDustBurst(pos, 8 + thornCount * 3, 5f + thornCount);

            Lighting.AddLight(pos, OdeToJoyPalette.GoldenPollen.ToVector3() * (0.8f + thornCount * 0.2f));
        }

        /// <summary>
        /// Simple death VFX when a thorn bolt expires without sticking.
        /// Vine trail dust + small bloom burst.
        /// </summary>
        public static void ThornBoltDeathVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            OdeToJoyVFXLibrary.SpawnVineTrailDust(pos, Main.rand.NextVector2Circular(2f, 2f));
            OdeToJoyVFXLibrary.BloomBurst(pos, 0.3f);

            Lighting.AddLight(pos, OdeToJoyPalette.VerdantGreen.ToVector3() * 0.4f);
        }

        #endregion
    }
}
