using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Optimization;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.MoonlightSonata.Enemies
{
    /// <summary>
    /// Shared VFX helper for all Moonlight Sonata enemies.
    /// Centralizes visual effects for WaningDeer, its projectiles, and chandeliers.
    /// Call from AI(), HitEffect(), OnKill(), and attack handler methods.
    /// </summary>
    public static class MoonlightEnemyVFX
    {
        // =====================================================================
        //  WANING DEER — AMBIENT AURA
        // =====================================================================

        /// <summary>
        /// Per-frame ambient particle effects for the Waning Deer.
        /// Produces converging palette dust, cycling particles, sparkles,
        /// attack-enhanced particles during high aura intensity, and sparse music notes.
        /// </summary>
        public static void WaningDeerAmbientAura(NPC npc, float auraIntensity)
        {
            if (Main.dedServ) return;

            // Converging palette dust aura
            if (Main.rand.NextBool(3))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float radius = 50f + Main.rand.NextFloat(20f);
                Vector2 dustPos = npc.Center + angle.ToRotationVector2() * radius;
                Vector2 toCenter = (npc.Center - dustPos).SafeNormalize(Vector2.Zero) * 1.5f;
                Color dustColor = Color.Lerp(MoonlightSonataPalette.DarkPurple, MoonlightSonataPalette.IceBlue, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(dustPos, DustID.PurpleTorch, toCenter, 80, dustColor, 1.2f);
                d.noGravity = true;
                d.fadeIn = 1.0f;
            }

            // Ambient dark purple / ice blue cycling particles
            if (Main.rand.NextBool(8))
            {
                int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.IceTorch;
                Color col = Main.rand.NextBool() ? MoonlightSonataPalette.DarkPurple : MoonlightSonataPalette.IceBlue;
                Dust d = Dust.NewDustDirect(npc.position, npc.width, npc.height, dustType,
                    0f, 0f, 100, col, 1.0f);
                d.noGravity = true;
                d.velocity = Main.rand.NextVector2Circular(1f, 1f);
            }

            // Occasional sparkle flare
            if (Main.rand.NextBool(12))
            {
                Vector2 sparkPos = npc.Center + Main.rand.NextVector2Circular(30f, 30f);
                CustomParticles.MoonlightFlare(sparkPos, 0.3f);
            }

            // Enhanced particles during attack states
            if (auraIntensity > 0.5f && Main.rand.NextBool(6))
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector2 pos = npc.Center + Main.rand.NextVector2Circular(25f, 25f);
                    CustomParticles.MoonlightFlare(pos, 0.25f + auraIntensity * 0.15f);
                }
            }

            // Sparse music notes
            if (Main.rand.NextBool(20))
            {
                MoonlightVFXLibrary.SpawnMusicNotes(npc.Center, 1, 25f, 0.7f, 0.9f, 30);
            }
        }

        // =====================================================================
        //  WANING DEER — ALERT / IDLE BURST
        // =====================================================================

        /// <summary>
        /// Player detection alert burst — crescendo flash with music notes.
        /// </summary>
        public static void WaningDeerAlertBurst(Vector2 pos)
        {
            if (Main.dedServ) return;

            CustomParticles.MoonlightCrescendo(pos, 1f);
            MoonlightVFXLibrary.SpawnMusicNotes(pos, 2, 20f, 0.8f, 1.0f, 25);
        }

        // =====================================================================
        //  WANING DEER — ATTACK VFX
        // =====================================================================

        /// <summary>
        /// Lunar beam sweep windup — converging dust toward cast origin.
        /// </summary>
        public static void LunarBeamWindupVFX(Vector2 pos, float progress)
        {
            if (Main.dedServ) return;

            int dustCount = 3 + (int)(progress * 5);
            for (int i = 0; i < dustCount; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float radius = 40f * (1f - progress * 0.5f);
                Vector2 dustPos = pos + angle.ToRotationVector2() * radius;
                Vector2 toCenter = (pos - dustPos).SafeNormalize(Vector2.Zero) * (2f + progress * 3f);
                Color col = Color.Lerp(MoonlightSonataPalette.DarkPurple, MoonlightSonataPalette.IceBlue, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(dustPos, DustID.PurpleTorch, toCenter, 60, col, 1.1f + progress * 0.5f);
                d.noGravity = true;
            }
        }

        /// <summary>
        /// Frost nova windup — 8-point orbiting ice dust ring.
        /// </summary>
        public static void FrostNovaWindupVFX(Vector2 center, float time, float progress)
        {
            if (Main.dedServ) return;

            for (int i = 0; i < 8; i++)
            {
                float pointAngle = time * 2f + MathHelper.TwoPi * i / 8f;
                float radius = 35f + MathF.Sin(time * 3f + i) * 5f;
                Vector2 dustPos = center + pointAngle.ToRotationVector2() * radius;
                Dust d = Dust.NewDustPerfect(dustPos, DustID.IceTorch,
                    Vector2.Zero, 80, MoonlightSonataPalette.IceBlue, 1.0f + progress * 0.5f);
                d.noGravity = true;
            }
        }

        /// <summary>
        /// Frost nova burst — radial ice explosion with gradient halo rings.
        /// </summary>
        public static void FrostNovaBurstVFX(Vector2 center, float radius = 200f)
        {
            if (Main.dedServ) return;

            MoonlightVFXLibrary.SpawnGradientHaloRings(center, 3, 0.8f);
            CustomParticles.HaloRing(center, MoonlightSonataPalette.IceBlue, 0.5f, 16);

            int dustCount = 20;
            for (int i = 0; i < dustCount; i++)
            {
                float angle = MathHelper.TwoPi * i / dustCount;
                float speed = Main.rand.NextFloat(6f, 12f);
                Vector2 vel = angle.ToRotationVector2() * speed;
                Dust d = Dust.NewDustPerfect(center, DustID.IceTorch, vel, 60,
                    Color.Lerp(MoonlightSonataPalette.DarkPurple, MoonlightSonataPalette.IceBlue, (float)i / dustCount), 1.4f);
                d.noGravity = true;
            }

            Lighting.AddLight(center, MoonlightSonataPalette.IceBlue.ToVector3() * 1.2f);
        }

        /// <summary>
        /// Crescent barrage windup — arc dust with palette gradient.
        /// </summary>
        public static void CrescentBarrageWindupVFX(Vector2 pos, float aimAngle, float progress)
        {
            if (Main.dedServ) return;

            int arcDustCount = 3 + (int)(progress * 4);
            for (int i = 0; i < arcDustCount; i++)
            {
                float spread = MathHelper.ToRadians(30f);
                float angle = aimAngle + Main.rand.NextFloat(-spread, spread);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                Color col = Color.Lerp(MoonlightSonataPalette.Violet, MoonlightSonataPalette.IceBlue, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.PurpleTorch, vel, 60, col, 1.1f);
                d.noGravity = true;
            }
        }

        /// <summary>
        /// Crescent barrage per-burst flash.
        /// </summary>
        public static void CrescentBarrageFireVFX(Vector2 pos)
        {
            if (Main.dedServ) return;
            CustomParticles.MoonlightFlare(pos, 0.6f);
        }

        /// <summary>
        /// Abyssal orbs windup — 6-point orbiting shadowflame ring.
        /// </summary>
        public static void AbyssalOrbsWindupVFX(Vector2 center, float time)
        {
            if (Main.dedServ) return;

            for (int i = 0; i < 6; i++)
            {
                float angle = time * 1.5f + MathHelper.TwoPi * i / 6f;
                float radius = 30f + MathF.Sin(time * 2f + i) * 8f;
                Vector2 dustPos = center + angle.ToRotationVector2() * radius;
                Dust d = Dust.NewDustPerfect(dustPos, DustID.Shadowflame,
                    Vector2.Zero, 80, MoonlightSonataPalette.NightPurple, 1.2f);
                d.noGravity = true;
            }
        }

        /// <summary>
        /// Abyssal orbs burst — dark purple shadowflame explosion.
        /// </summary>
        public static void AbyssalOrbsBurstVFX(Vector2 center)
        {
            if (Main.dedServ) return;

            for (int i = 0; i < 15; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                Dust d = Dust.NewDustPerfect(center, DustID.Shadowflame, vel, 60,
                    MoonlightSonataPalette.DarkPurple, 1.3f);
                d.noGravity = true;
            }
        }

        /// <summary>
        /// Moonlight Apocalypse warning circle — 16-point dust ring + rising ice dust.
        /// </summary>
        public static void MoonlightApocalypseWarningVFX(Vector2 center, float radius, float progress)
        {
            if (Main.dedServ) return;

            // Warning circle
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 dustPos = center + angle.ToRotationVector2() * radius;
                Dust d = Dust.NewDustPerfect(dustPos, DustID.PurpleTorch,
                    Vector2.Zero, 80, MoonlightSonataPalette.Violet, 1.0f + progress * 0.5f);
                d.noGravity = true;
            }

            // Rising ice dust
            if (Main.rand.NextBool(2))
            {
                Vector2 risePos = center + Main.rand.NextVector2Circular(radius, radius);
                Dust d = Dust.NewDustPerfect(risePos, DustID.IceTorch,
                    new Vector2(0, -2f - progress * 3f), 60, MoonlightSonataPalette.IceBlue, 1.2f);
                d.noGravity = true;
            }
        }

        /// <summary>
        /// Moonlight Apocalypse beam impact — massive burst with god rays and screen distortion.
        /// </summary>
        public static void MoonlightApocalypseImpactVFX(Vector2 impactPos)
        {
            if (Main.dedServ) return;

            // Radial dust burst
            for (int i = 0; i < 40; i++)
            {
                float angle = MathHelper.TwoPi * i / 40f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 12f);
                Color col = Color.Lerp(MoonlightSonataPalette.DarkPurple, MoonlightSonataPalette.IceBlue, (float)i / 40f);
                Dust d = Dust.NewDustPerfect(impactPos, DustID.PurpleTorch, vel, 60, col, 1.5f);
                d.noGravity = true;
            }

            MoonlightVFXLibrary.SpawnGradientHaloRings(impactPos, 4, 1.2f);

            GodRaySystem.CreateBurst(impactPos, MoonlightSonataPalette.Violet, 6, 80f, 20,
                GodRaySystem.GodRayStyle.Explosion, MoonlightSonataPalette.IceBlue);

            if (AdaptiveQualityManager.Instance?.CurrentQuality >= AdaptiveQualityManager.QualityLevel.Medium)
            {
                ScreenDistortionManager.TriggerRipple(impactPos, MoonlightSonataPalette.DarkPurple, 0.4f, 20);
            }

            Lighting.AddLight(impactPos, MoonlightSonataPalette.IceBlue.ToVector3() * 1.5f);
        }

        // =====================================================================
        //  WANING DEER — HIT / DEATH
        // =====================================================================

        /// <summary>
        /// Generic enemy hit flash — palette-cycling dust burst.
        /// Call from HitEffect() when the enemy is not dead.
        /// </summary>
        public static void EnemyHitFlash(Vector2 pos, int width, int height)
        {
            if (Main.dedServ) return;

            for (int i = 0; i < 12; i++)
            {
                int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.IceTorch;
                Color col = Main.rand.NextBool() ? MoonlightSonataPalette.DarkPurple : MoonlightSonataPalette.IceBlue;
                Dust d = Dust.NewDustDirect(pos - new Vector2(width / 2, height / 2), width, height,
                    dustType, 0f, 0f, 80, col, 1.2f);
                d.noGravity = true;
                d.velocity = Main.rand.NextVector2Circular(4f, 4f);
            }
        }

        /// <summary>
        /// Waning Deer death VFX — massive burst with dust explosion, snow debris,
        /// crescendo, gradient halos, god rays, screen distortion, music notes, and screen shake.
        /// </summary>
        public static void WaningDeerDeathVFX(Vector2 center)
        {
            if (Main.dedServ) return;

            // Massive dust explosion
            for (int i = 0; i < 50; i++)
            {
                int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.IceTorch;
                Color col = Color.Lerp(MoonlightSonataPalette.DarkPurple, MoonlightSonataPalette.IceBlue, Main.rand.NextFloat());
                Vector2 vel = Main.rand.NextVector2Circular(15f, 15f);
                Dust d = Dust.NewDustPerfect(center, dustType, vel, 60, col, 1.5f);
                d.noGravity = true;
            }

            // Snow debris
            for (int i = 0; i < 30; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(8f, 8f);
                vel.Y -= 3f;
                Dust d = Dust.NewDustPerfect(center, DustID.Snow, vel, 0, default, 1.0f);
                d.noGravity = false;
            }

            // Crescendo flash + gradient halos
            CustomParticles.MoonlightCrescendo(center, 1.5f);
            MoonlightVFXLibrary.SpawnGradientHaloRings(center, 5, 1.5f);

            // God rays
            GodRaySystem.CreateBurst(center, MoonlightSonataPalette.Violet, 8, 100f, 25,
                GodRaySystem.GodRayStyle.Explosion, MoonlightSonataPalette.IceBlue);

            // Screen distortion
            if (AdaptiveQualityManager.Instance?.CurrentQuality >= AdaptiveQualityManager.QualityLevel.Medium)
            {
                ScreenDistortionManager.TriggerRipple(center, MoonlightSonataPalette.DarkPurple, 0.5f, 25);
            }

            // Music note scatter
            MoonlightVFXLibrary.SpawnMusicNotes(center, 5, 30f, 0.85f, 1.1f, 35);

            // Screen shake
            MagnumScreenEffects.AddScreenShake(3f);

            Lighting.AddLight(center, MoonlightSonataPalette.MoonWhite.ToVector3() * 1.5f);
        }

        /// <summary>
        /// Jump trail dust — sparse ice torch trail while deer is jumping.
        /// Call every 3 frames from jump handling.
        /// </summary>
        public static void JumpTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Dust d = Dust.NewDustPerfect(pos, DustID.IceTorch,
                -velocity * 0.3f + Main.rand.NextVector2Circular(1f, 1f),
                60, MoonlightSonataPalette.IceBlue, 1.0f);
            d.noGravity = true;
        }

        // =====================================================================
        //  CHANDELIER ORB — AMBIENT + DEATH
        // =====================================================================

        /// <summary>
        /// Per-frame ambient VFX for the WaningDeer Chandelier orbiting projectile.
        /// Palette dust, snow sparkles, snowflakes, and music notes.
        /// </summary>
        public static void ChandelierAmbientVFX(Projectile proj)
        {
            if (Main.dedServ) return;

            float lightPulse = 0.8f + MathF.Sin(Main.GameUpdateCount * 0.06f) * 0.2f;
            Lighting.AddLight(proj.Center, MoonlightSonataPalette.IceBlue.ToVector3() * 0.5f * lightPulse +
                MoonlightSonataPalette.DarkPurple.ToVector3() * 0.3f * lightPulse);

            // Palette-coloured dust
            if (Main.rand.NextBool(18))
            {
                int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.IceTorch;
                Color col = Main.rand.NextBool() ? MoonlightSonataPalette.DarkPurple : MoonlightSonataPalette.IceBlue;
                Dust d = Dust.NewDustDirect(proj.position, proj.width, proj.height, dustType,
                    0f, 0f, 80, col, 0.9f);
                d.noGravity = true;
                d.velocity = Main.rand.NextVector2Circular(0.5f, 0.5f);
            }

            // Snow sparkle
            if (Main.rand.NextBool(25))
            {
                Dust d = Dust.NewDustDirect(proj.position, proj.width, proj.height,
                    DustID.BlueFairy, 0f, 0f, 100, default, 0.6f);
                d.noGravity = true;
                d.velocity = Main.rand.NextVector2Circular(0.3f, 0.3f);
            }

            // Occasional snowflake
            if (Main.rand.NextBool(40))
            {
                Dust d = Dust.NewDustDirect(proj.position, proj.width, proj.height,
                    DustID.Snow, 0f, 0f, 0, default, 0.8f);
                d.velocity = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(0.5f, 1.5f));
            }

            // Music notes
            if (Main.rand.NextBool(30))
            {
                MoonlightVFXLibrary.SpawnMusicNotes(proj.Center, 1, 8f, 0.55f, 0.72f, 28);
            }
        }

        /// <summary>
        /// Chandelier death VFX — impact burst with gradient dust scatter.
        /// </summary>
        public static void ChandelierDeathVFX(Vector2 center)
        {
            if (Main.dedServ) return;

            MoonlightVFXLibrary.ProjectileImpact(center, 0.4f);

            for (int i = 0; i < 6; i++)
            {
                float progress = (float)i / 6f;
                Color col = Color.Lerp(MoonlightSonataPalette.DarkPurple, MoonlightSonataPalette.IceBlue, progress);
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Dust d = Dust.NewDustPerfect(center, DustID.PurpleTorch, vel, 60, col, 1.1f);
                d.noGravity = true;
            }
        }

        // =====================================================================
        //  ENEMY PROJECTILE TRAILS — shared across Snow/Homing/Crescent projectiles
        // =====================================================================

        /// <summary>
        /// Per-frame trail VFX for Moonlight enemy projectiles.
        /// Produces purple/ice cycling dust trail, snow particles, sparkles, and music notes.
        /// </summary>
        public static void EnemyProjectileTrail(Projectile proj)
        {
            if (Main.dedServ) return;

            Lighting.AddLight(proj.Center, MoonlightSonataPalette.IceBlue.ToVector3() * 0.5f);

            // Trail particles
            if (Main.rand.NextBool(2))
            {
                int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.IceTorch;
                Color col = Color.Lerp(MoonlightSonataPalette.DarkPurple, MoonlightSonataPalette.IceBlue, Main.rand.NextFloat());
                Dust d = Dust.NewDustDirect(proj.position, proj.width, proj.height,
                    dustType, 0f, 0f, 80, col, 1.1f);
                d.noGravity = true;
                d.velocity = -proj.velocity * 0.2f + Main.rand.NextVector2Circular(0.5f, 0.5f);
            }

            // Snow particles
            if (Main.rand.NextBool(4))
            {
                Dust d = Dust.NewDustDirect(proj.position, proj.width, proj.height,
                    DustID.Snow, 0f, 0f, 0, default, 0.7f);
                d.velocity = -proj.velocity * 0.1f + Main.rand.NextVector2Circular(0.5f, 0.5f);
            }

            // Sparkle particles
            if (Main.rand.NextBool(6))
            {
                Dust d = Dust.NewDustDirect(proj.position, proj.width, proj.height,
                    DustID.BlueFairy, 0f, 0f, 100, default, 0.6f);
                d.noGravity = true;
                d.velocity = Main.rand.NextVector2Circular(0.5f, 0.5f);
            }

            // Music notes
            if (Main.rand.NextBool(12))
            {
                MoonlightVFXLibrary.SpawnMusicNotes(proj.Center, 1, 8f, 0.6f, 0.75f, 25);
            }
        }

        /// <summary>
        /// Enemy projectile impact VFX — impact burst + halo ring.
        /// Call from OnKill().
        /// </summary>
        public static void EnemyProjectileImpact(Vector2 center, float intensity = 0.5f)
        {
            if (Main.dedServ) return;

            MoonlightVFXLibrary.ProjectileImpact(center, intensity);
            CustomParticles.HaloRing(center, MoonlightSonataPalette.IceBlue, 0.25f * intensity, 14);
        }

        // =====================================================================
        //  HOMING ORB — TRAIL + DEATH (WaningDeerHomingOrb)
        // =====================================================================

        /// <summary>
        /// Per-frame trail VFX for the homing orb — orbiting moon motes,
        /// sparkle trail, moonlight glow trail, palette dust, and music notes.
        /// </summary>
        public static void HomingOrbTrailVFX(Projectile proj)
        {
            if (Main.dedServ) return;

            float time = Main.GameUpdateCount * 0.05f;

            // Orbiting moon dust motes
            if (Main.rand.NextBool(3))
            {
                float moteAngle = time + Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 motePos = proj.Center + moteAngle.ToRotationVector2() * 12f;
                CustomParticles.MoonlightFlare(motePos, 0.2f);
            }

            // Sparkle trail
            if (Main.rand.NextBool(3))
            {
                Vector2 sparkPos = proj.Center + Main.rand.NextVector2Circular(8f, 8f);
                Color sparkColor = Color.Lerp(MoonlightSonataPalette.Violet, MoonlightSonataPalette.IceBlue, Main.rand.NextFloat());
                var sparkle = new SparkleParticle(sparkPos, -proj.velocity * 0.15f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    sparkColor, 0.2f, 18);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // Moonlight glow trail
            if (Main.rand.NextBool(2))
            {
                Color trailCol = Color.Lerp(MoonlightSonataPalette.DarkPurple, MoonlightSonataPalette.Lavender, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(proj.Center, DustID.PurpleTorch,
                    -proj.velocity * 0.2f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    80, trailCol, 1.1f);
                d.noGravity = true;
            }

            // Music notes
            if (Main.rand.NextBool(8))
            {
                MoonlightVFXLibrary.SpawnMusicNotes(proj.Center, 1, 8f, 0.7f, 0.85f, 25);
            }

            // Pulsing light
            float pulse = 0.8f + MathF.Sin(Main.GameUpdateCount * 0.08f) * 0.2f;
            Color lightCol = Color.Lerp(MoonlightSonataPalette.DarkPurple, MoonlightSonataPalette.IceBlue, pulse * 0.5f);
            Lighting.AddLight(proj.Center, lightCol.ToVector3() * 0.6f);
        }

        /// <summary>
        /// Homing orb death VFX — impact burst with halo ring and music notes.
        /// </summary>
        public static void HomingOrbDeathVFX(Vector2 center)
        {
            if (Main.dedServ) return;

            MoonlightVFXLibrary.ProjectileImpact(center, 0.6f);
            CustomParticles.HaloRing(center, MoonlightSonataPalette.IceBlue, 0.3f, 16);
            MoonlightVFXLibrary.SpawnMusicNotes(center, 3, 20f, 0.8f, 1.0f, 28);
        }

        // =====================================================================
        //  DYNAMIC LIGHTING
        // =====================================================================

        /// <summary>
        /// Add standard Moonlight enemy ambient pulsing light.
        /// </summary>
        public static void EnemyAmbientLight(Vector2 pos, float intensity = 0.6f)
        {
            float pulse = 0.8f + MathF.Sin(Main.GameUpdateCount * 0.04f) * 0.2f;
            Lighting.AddLight(pos, MoonlightSonataPalette.Violet.ToVector3() * intensity * pulse);
        }
    }
}
