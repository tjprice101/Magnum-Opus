using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Bosses;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems;
using static MagnumOpus.Content.DiesIrae.Bosses.Systems.DiesIraeSkySystem;

namespace MagnumOpus.Content.DiesIrae.Bosses.Systems
{
    /// <summary>
    /// Dies Irae boss attack choreography system.
    /// 10 apocalyptic attacks with sky flashes and bloom cascades on impacts.
    /// Every attack feels like divine punishment incarnate.
    /// </summary>
    public static class DiesIraeAttackVFX
    {
        private static readonly Color BloodRed = new Color(200, 30, 20);
        private static readonly Color DarkCrimson = new Color(120, 15, 15);
        private static readonly Color EmberOrange = new Color(220, 100, 30);
        private static readonly Color AshenBlack = new Color(25, 15, 10);
        private static readonly Color HellfireWhite = new Color(255, 220, 180);

        #region Core Attacks

        /// <summary>HellfireBarrage: Rapid hellfire projectile streams from multiple angles.</summary>
        public static void HellfireBarrageTelegraph(Vector2 center)
        {
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                TelegraphSystem.ThreatLine(center, angle.ToRotationVector2(), 400f, 25, BloodRed * 0.6f);
            }
            Phase10BossVFX.AccelerandoSpiral(center, EmberOrange, 1.2f);
        }

        public static void HellfireBarrageProjectile(Vector2 position)
        {
            Color color = Color.Lerp(BloodRed, EmberOrange, Main.rand.NextFloat(0.4f));
            CustomParticles.GenericFlare(position, color, 0.35f, 8);
            CustomParticles.GlowTrail(position, EmberOrange, 0.3f);
        }

        public static void HellfireBarrageImpact(Vector2 position)
        {
            MagnumScreenEffects.AddScreenShake(10f);
            TriggerHellfireFlash(5f);
            CustomParticles.DiesIraeImpactBurst(position, 8);
            CustomParticles.HaloRing(position, BloodRed, 0.5f, 15);
            MagnumParticleHandler.SpawnParticle(new BloomParticle(position, Vector2.Zero, EmberOrange, 0.5f, 12));
        }

        /// <summary>JudgmentRay: Colossal beam of divine wrath sweeping across the arena.</summary>
        public static void JudgmentRayTelegraph(Vector2 start, Vector2 direction)
        {
            TelegraphSystem.ThreatLine(start, direction, 800f, 40, BloodRed * 0.8f);
            BossVFXOptimizer.LaserBeamWarning(start,
                (float)Math.Atan2(direction.Y, direction.X), 800f, 1.0f);
            Phase10BossVFX.CrescendoDangerRings(start, EmberOrange, 1.2f);
        }

        public static void JudgmentRayFiring(Vector2 position, Vector2 direction)
        {
            TriggerJudgmentFlash(3f);
            CustomParticles.GenericFlare(position, HellfireWhite, 0.6f, 6);
            CustomParticles.GenericFlare(position, BloodRed, 0.4f, 8);
            if (Main.rand.NextBool(2))
                CustomParticles.DiesIraeHellfireBurst(position, 3);
        }

        public static void JudgmentRayImpact(Vector2 position)
        {
            MagnumScreenEffects.AddScreenShake(15f);
            TriggerJudgmentFlash(8f);
            CustomParticles.ExplosionBurst(position, EmberOrange, 12, 6f);
            CustomParticles.HaloRing(position, BloodRed, 0.7f, 18);
            BossSignatureVFX.DiesIraeWrathStrike(position, Vector2.UnitY, 1.2f);
            MagnumParticleHandler.SpawnParticle(new BloomParticle(position, Vector2.Zero, HellfireWhite, 0.8f, 15));
        }

        /// <summary>InfernalRing: Expanding ring of hellfire that closes inward.</summary>
        public static void InfernalRingTelegraph(Vector2 center)
        {
            TelegraphSystem.ConvergingRing(center, 250f, 45, EmberOrange * 0.6f);
            Phase10BossVFX.CrescendoRing(center, 250f, 250f, EmberOrange);
        }

        public static void InfernalRingPulse(Vector2 center, float currentRadius)
        {
            TriggerHellfireFlash(2f);
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f + Main.rand.NextFloat(0.2f);
                Vector2 pos = center + angle.ToRotationVector2() * currentRadius;
                CustomParticles.GenericFlare(pos, EmberOrange, 0.4f, 10);
            }
            Phase10BossVFX.CrescendoRing(center, currentRadius, currentRadius + 10f, BloodRed);
        }

        /// <summary>CondemnationStrike: Targeted divine punishment on a single player.</summary>
        public static void CondemnationStrikeTelegraph(Vector2 target)
        {
            TelegraphSystem.ImpactPoint(target, 80f, 50);
            TelegraphSystem.ConvergingRing(target, 150f, 40, BloodRed * 0.7f);
            Phase10BossVFX.FortissimoFlashWarning(target, BloodRed, 1.0f);
        }

        public static void CondemnationStrikeImpact(Vector2 position)
        {
            MagnumScreenEffects.AddScreenShake(18f);
            TriggerWrathFlash(10f);
            CustomParticles.GenericFlare(position, HellfireWhite, 1.5f, 22);
            CustomParticles.ExplosionBurst(position, BloodRed, 14, 6f);
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                CustomParticles.HaloRing(position + angle.ToRotationVector2() * 40f,
                    Color.Lerp(BloodRed, EmberOrange, i / 8f), 0.5f, 16);
            }
            BossSignatureVFX.DiesIraeWrathStrike(position, Vector2.UnitY, 1.5f);

            // Bloom cascade on condemn impact
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 vel = angle.ToRotationVector2() * 3f;
                Color bloomColor = Color.Lerp(BloodRed, HellfireWhite, i / 6f);
                MagnumParticleHandler.SpawnParticle(new BloomParticle(position, vel, bloomColor, 0.6f, 18));
            }
        }

        /// <summary>SoulHarvest: Draining attack with dark tendrils reaching toward player.</summary>
        public static void SoulHarvestTelegraph(Vector2 bossCenter, Vector2 target)
        {
            Phase10BossVFX.StaffLineLaser(bossCenter, target, DarkCrimson, 15f);
            Phase10BossVFX.ChordBuildupSpiral(bossCenter, new[] { DarkCrimson, BloodRed }, 0.6f);
        }

        public static void SoulHarvestDrain(Vector2 position, Vector2 direction, int frame)
        {
            Color color = Color.Lerp(DarkCrimson, BloodRed, (float)Math.Sin(frame * 0.15f) * 0.5f + 0.5f);
            CustomParticles.GenericFlare(position, color, 0.3f + (float)Math.Sin(frame * 0.1f) * 0.1f, 12);
            BossVFXOptimizer.ProjectileTrail(position, direction * 2f, DarkCrimson);

            if (frame % 8 == 0)
                TriggerHellfireFlash(2f);
        }

        #endregion

        #region Escalation Attacks

        /// <summary>WrathfulDescent: Massive diving attack from the sky with ground eruption.</summary>
        public static void WrathfulDescentTelegraph(Vector2 position, Vector2 target)
        {
            Vector2 dir = (target - position).SafeNormalize(Vector2.UnitY);
            TelegraphSystem.ThreatLine(position, dir, 900f, 40, EmberOrange * 0.8f);
            TelegraphSystem.ImpactPoint(target, 100f, 45);
            Phase10BossVFX.CrescendoDangerRings(target, BloodRed, 1.0f);
        }

        public static void WrathfulDescentImpact(Vector2 position)
        {
            MagnumScreenEffects.AddScreenShake(25f);
            TriggerWrathFlash(12f);
            CustomParticles.ExplosionBurst(position, EmberOrange, 18, 8f);
            CustomParticles.GenericFlare(position, HellfireWhite, 2.0f, 28);
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                CustomParticles.HaloRing(position + angle.ToRotationVector2() * 60f,
                    i % 2 == 0 ? BloodRed : EmberOrange, 0.6f, 18);
            }
            Phase10BossVFX.TimpaniDrumrollImpact(position, BloodRed, 2.0f);

            // Bloom ring on ground eruption
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * 5f;
                MagnumParticleHandler.SpawnParticle(new BloomParticle(position, vel, EmberOrange, 0.7f, 20));
            }
        }

        /// <summary>ChainOfDamnation: Linked explosive points in sequence.</summary>
        public static void ChainOfDamnationTelegraph(Vector2[] chainPoints)
        {
            for (int i = 0; i < chainPoints.Length - 1; i++)
            {
                TelegraphSystem.LaserPath(chainPoints[i], chainPoints[i + 1], 25f, 35, DarkCrimson * 0.5f);
            }
            for (int i = 0; i < chainPoints.Length; i++)
            {
                TelegraphSystem.ImpactPoint(chainPoints[i], 50f, 35);
            }
        }

        public static void ChainOfDamnationDetonation(Vector2 position, int chainIndex)
        {
            MagnumScreenEffects.AddScreenShake(12f + chainIndex * 2f);
            TriggerHellfireFlash(4f + chainIndex * 2f);
            CustomParticles.DiesIraeImpactBurst(position, 10 + chainIndex * 2);
            CustomParticles.DiesIraeHellfireBurst(position, 6 + chainIndex * 2);
            CustomParticles.HaloRing(position, BloodRed, 0.5f + chainIndex * 0.08f, 16);
            MagnumParticleHandler.SpawnParticle(new BloomParticle(position, Vector2.Zero,
                Color.Lerp(EmberOrange, HellfireWhite, chainIndex / 5f), 0.5f + chainIndex * 0.1f, 14));
        }

        /// <summary>ApocalypseRain: Massive barrage raining from the sky.</summary>
        public static void ApocalypseRainTelegraph(Vector2 targetArea)
        {
            TelegraphSystem.DangerZone(targetArea, 400f, 50, BloodRed * 0.4f);
            Phase10BossVFX.FortissimoFlashWarning(targetArea, EmberOrange, 1.5f);
            Phase10BossVFX.CrescendoDangerRings(targetArea, BloodRed, 1.5f);
        }

        public static void ApocalypseRainImpact(Vector2 position)
        {
            MagnumScreenEffects.AddScreenShake(8f);
            TriggerHellfireFlash(3f);
            CustomParticles.ExplosionBurst(position, EmberOrange, 8, 5f);
            CustomParticles.HaloRing(position, BloodRed, 0.4f, 14);
            CustomParticles.DiesIraeHellfireBurst(position, 4);
            MagnumParticleHandler.SpawnParticle(new BloomParticle(position, Vector2.Zero, EmberOrange, 0.4f, 10));
        }

        #endregion

        #region Ultimate Attacks

        /// <summary>FinalJudgment: Supreme radial attack with hellfire pillars from all directions.</summary>
        public static void FinalJudgmentTelegraph(Vector2 center)
        {
            TelegraphSystem.ConvergingRing(center, 350f, 60, BloodRed);
            Phase10BossVFX.TuttiFullEnsemble(center, new[] { DarkCrimson, BloodRed, EmberOrange, HellfireWhite }, 2.0f);
            Phase10BossVFX.FortissimoFlashWarning(center, HellfireWhite, 2.0f);
        }

        public static void FinalJudgmentRelease(Vector2 center)
        {
            MagnumScreenEffects.AddScreenShake(30f);
            TriggerJudgmentFlash(18f);
            CustomParticles.GenericFlare(center, HellfireWhite, 2.5f, 30);
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Color color = i % 3 == 0 ? HellfireWhite : (i % 3 == 1 ? BloodRed : EmberOrange);
                CustomParticles.HaloRing(center + angle.ToRotationVector2() * 100f, color, 0.6f, 20);
            }
            BossSignatureVFX.DiesIraeDayOfWrath(center, 5, 5, 2.0f);
            Phase10BossVFX.CodaFinale(center, BloodRed, AshenBlack, 2.0f);

            // Bloom judgment ring - 10 radiating bloom particles
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 vel = angle.ToRotationVector2() * 6f;
                Color bloomColor = Color.Lerp(BloodRed, HellfireWhite, i / 10f);
                MagnumParticleHandler.SpawnParticle(new BloomParticle(center, vel, bloomColor, 0.8f, 25));
            }
        }

        /// <summary>DivinePunishment: The ultimate single-target obliteration.</summary>
        public static void DivinePunishmentTelegraph(Vector2 target)
        {
            TelegraphSystem.ConvergingRing(target, 200f, 50, HellfireWhite * 0.8f);
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                TelegraphSystem.ThreatLine(target + angle.ToRotationVector2() * 300f,
                    (-angle.ToRotationVector2()), 300f, 45, BloodRed * 0.6f);
            }
            Phase10BossVFX.ChordBuildupSpiral(target, new[] { DarkCrimson, BloodRed, EmberOrange, HellfireWhite }, 1.5f);
        }

        public static void DivinePunishmentRelease(Vector2 center)
        {
            MagnumScreenEffects.AddScreenShake(35f);
            TriggerApocalypseFlash(22f);
            CustomParticles.GenericFlare(center, HellfireWhite, 3.0f, 35);
            CustomParticles.ExplosionBurst(center, BloodRed, 25, 10f);
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi * i / 20f;
                Color color = VFXIntegration.GetThemeColor("DiesIrae", i / 20f);
                CustomParticles.GenericFlare(center + angle.ToRotationVector2() * 130f, color, 1.0f, 28);
                CustomParticles.HaloRing(center + angle.ToRotationVector2() * 80f,
                    i % 2 == 0 ? BloodRed : EmberOrange, 0.6f, 22);
            }
            Phase10BossVFX.CodaFinale(center, HellfireWhite, DarkCrimson, 3.0f);
            Phase10BossVFX.CadenceFinisher(center,
                new[] { DarkCrimson, BloodRed, EmberOrange, HellfireWhite }, 1f);

            // Supernova bloom ring - 16 radiating particles
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 vel = angle.ToRotationVector2() * 7f;
                Color bloomColor = Color.Lerp(EmberOrange, HellfireWhite, i / 16f);
                MagnumParticleHandler.SpawnParticle(new BloomParticle(center, vel, bloomColor, 0.9f, 30));
            }

            // Ascending hellfire sparkles
            for (int i = 0; i < 10; i++)
            {
                Vector2 sparkPos = center + Main.rand.NextVector2Circular(60f, 60f);
                Vector2 sparkVel = new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), -Main.rand.NextFloat(3f, 6f));
                MagnumParticleHandler.SpawnParticle(new SparkleParticle(sparkPos, sparkVel, HellfireWhite, 0.5f, 30));
            }
        }

        #endregion
    }
}