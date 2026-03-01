using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Bosses;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.Nachtmusik.Bosses.Systems
{
    /// <summary>
    /// Nachtmusik boss attack choreography system.
    /// 15 attacks across Phase 1 (serene nocturnal) and Phase 2 (violent cosmic fury).
    /// Phase 2 attacks are dramatically more intense visually.
    /// TwilightReversal uses inverted color VFX; QuantumBlink uses rapid position flashes.
    /// </summary>
    public static class NachtmusikAttackVFX
    {
        private static readonly Color DeepIndigo = new Color(40, 30, 100);
        private static readonly Color StarlightSilver = new Color(200, 210, 230);
        private static readonly Color CosmicBlue = new Color(80, 120, 200);
        private static readonly Color NebulaGold = new Color(220, 180, 100);

        #region Phase 1  ENocturnal Serenade

        /// <summary>StarlightWaltz: Graceful swirling starlight arcs.</summary>
        public static void StarlightWaltzTelegraph(Vector2 center)
        {
            TelegraphSystem.ConvergingRing(center, 120f, 35, StarlightSilver * 0.5f);
            Phase10BossVFX.RubatoBreath(center, StarlightSilver, 0.5f);
        }

        public static void StarlightWaltzRelease(Vector2 position, int arcIndex)
        {
            float angle = arcIndex * 0.4f;
            Color color = Color.Lerp(StarlightSilver, CosmicBlue, (float)Math.Sin(angle) * 0.5f + 0.5f);
            CustomParticles.GenericFlare(position, color, 0.35f, 12);
            BossVFXOptimizer.OptimizedFlare(position, color, 0.3f, 10);
        }

        /// <summary>ConstellationDance: Points of light forming ephemeral patterns.</summary>
        public static void ConstellationDanceTelegraph(Vector2 center)
        {
            Phase10BossVFX.NoteConstellationWarning(center, StarlightSilver, 0.6f);
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi * i / 5f;
                CustomParticles.GenericFlare(center + angle.ToRotationVector2() * 80f,
                    StarlightSilver * 0.6f, 0.3f, 18);
            }
        }

        public static void ConstellationDanceImpact(Vector2 position)
        {
            MagnumScreenEffects.AddScreenShake(6f);
            CustomParticles.HaloRing(position, StarlightSilver, 0.5f, 16);
            CustomParticles.GenericFlare(position, CosmicBlue, 0.4f, 12);
        }

        /// <summary>MoonbeamCascade: Falling streams of pale moonlight beams.</summary>
        public static void MoonbeamCascadeTelegraph(Vector2 targetArea)
        {
            TelegraphSystem.DangerZone(targetArea, 200f, 40, CosmicBlue * 0.3f);
            Phase10BossVFX.StaffLineConvergence(targetArea + new Vector2(0, -200), StarlightSilver, 0.7f);
        }

        public static void MoonbeamCascadeParticle(Vector2 position)
        {
            Color color = Color.Lerp(StarlightSilver, CosmicBlue, Main.rand.NextFloat(0.3f));
            CustomParticles.GenericFlare(position, color, 0.25f, 8);
        }

        /// <summary>NocturnalSerenade: Gentle sound wave rings.</summary>
        public static void NocturnalSerenadeTelegraph(Vector2 center)
        {
            Phase10BossVFX.ChordBuildupSpiral(center, new[] { DeepIndigo, StarlightSilver, CosmicBlue }, 0.4f);
        }

        public static void NocturnalSerenadeRelease(Vector2 center, int waveIndex)
        {
            CustomParticles.HaloRing(center, Color.Lerp(StarlightSilver, CosmicBlue, waveIndex / 5f),
                0.4f + waveIndex * 0.1f, 18);
            Phase10BossVFX.DynamicsWave(center, 0.4f + waveIndex * 0.1f, StarlightSilver);
        }

        /// <summary>CrescentSlash: Crescent-shaped projectile arcs.</summary>
        public static void CrescentSlashTelegraph(Vector2 position, Vector2 direction)
        {
            TelegraphSystem.ThreatLine(position, direction, 400f, 25, StarlightSilver * 0.6f);
            BossVFXOptimizer.WarningFlare(position, 0.5f);
        }

        public static void CrescentSlashImpact(Vector2 position)
        {
            MagnumScreenEffects.AddScreenShake(7f);
            CustomParticles.HaloRing(position, StarlightSilver, 0.5f, 15);
            CustomParticles.GenericFlare(position, CosmicBlue, 0.5f, 14);
            Phase10BossVFX.ChordResolutionBloom(position, new[] { StarlightSilver, CosmicBlue }, 0.7f);
        }

        #endregion

        #region Phase 1B  ERising Tension

        /// <summary>AuroraVeil: Shimmering curtain of light that damages on contact.</summary>
        public static void AuroraVeilTelegraph(Vector2 start, Vector2 end)
        {
            TelegraphSystem.LaserPath(start, end, 40f, 30, CosmicBlue * 0.4f);
            Phase10BossVFX.LegatoWaveWash(start, (end - start).SafeNormalize(Vector2.UnitX), CosmicBlue, (end - start).Length());
        }

        public static void AuroraVeilShimmer(Vector2 position, int frame)
        {
            Color color = Color.Lerp(CosmicBlue, NebulaGold, (float)Math.Sin(frame * 0.1f) * 0.5f + 0.5f);
            CustomParticles.GenericFlare(position, color, 0.3f, 10);
        }

        /// <summary>CosmicTempest: Swirling vortex of stellar energy.</summary>
        public static void CosmicTempestTelegraph(Vector2 center)
        {
            TelegraphSystem.ConvergingRing(center, 180f, 40, CosmicBlue * 0.6f);
            Phase10BossVFX.AccelerandoSpiral(center, CosmicBlue, 1.0f);
        }

        public static void CosmicTempestPulse(Vector2 center, int pulseIndex)
        {
            float intensity = 0.4f + pulseIndex * 0.1f;
            CustomParticles.GenericFlare(center, CosmicBlue, intensity, 14);
            CustomParticles.HaloRing(center, StarlightSilver, intensity * 0.8f, 16);
        }

        #endregion

        #region Phase 2  EViolent Cosmic Storm (Post Fake-Death)

        /// <summary>NebulaBurst: Explosive nebula cloud detonation.</summary>
        public static void NebulaBurstTelegraph(Vector2 center)
        {
            TelegraphSystem.ConvergingRing(center, 200f, 35, NebulaGold * 0.7f);
            Phase10BossVFX.FortissimoFlashWarning(center, NebulaGold, 1.0f);
        }

        public static void NebulaBurstImpact(Vector2 center)
        {
            MagnumScreenEffects.AddScreenShake(15f);
            CustomParticles.GenericFlare(center, NebulaGold, 1.5f, 22);
            CustomParticles.ExplosionBurst(center, CosmicBlue, 12, 6f);
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Color color = i % 2 == 0 ? NebulaGold : CosmicBlue;
                CustomParticles.HaloRing(center + angle.ToRotationVector2() * 50f, color, 0.5f, 16);
            }
        }

        /// <summary>GalacticJudgment: Massive radial pattern  Ethe Queen's fury unleashed.</summary>
        public static void GalacticJudgmentTelegraph(Vector2 center)
        {
            TelegraphSystem.ConvergingRing(center, 250f, 50, CosmicBlue * 0.7f);
            Phase10BossVFX.ChordBuildupSpiral(center, new[] { DeepIndigo, CosmicBlue, NebulaGold, StarlightSilver }, 1.0f);
            Phase10BossVFX.FortissimoFlashWarning(center, StarlightSilver, 1.2f);
        }

        public static void GalacticJudgmentRelease(Vector2 center)
        {
            MagnumScreenEffects.AddScreenShake(20f);
            CustomParticles.GenericFlare(center, StarlightSilver, 1.8f, 25);
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Color color = i % 3 == 0 ? NebulaGold : (i % 3 == 1 ? CosmicBlue : StarlightSilver);
                CustomParticles.HaloRing(center + angle.ToRotationVector2() * 70f, color, 0.5f, 18);
            }
            Phase10BossVFX.TuttiFullEnsemble(center, new[] { CosmicBlue, NebulaGold, StarlightSilver }, 1.5f);
        }

        /// <summary>StarfallApocalypse: Phase 2 version of starfall  E3x more projectiles, bigger explosions.</summary>
        public static void StarfallApocalypseTelegraph(Vector2 targetArea)
        {
            TelegraphSystem.DangerZone(targetArea, 350f, 50, CosmicBlue * 0.5f);
            Phase10BossVFX.CrescendoDangerRings(targetArea, NebulaGold, 1.2f);
        }

        public static void StarfallApocalypseImpact(Vector2 position)
        {
            MagnumScreenEffects.AddScreenShake(12f);
            CustomParticles.ExplosionBurst(position, NebulaGold, 10, 5f);
            CustomParticles.HaloRing(position, CosmicBlue, 0.6f, 18);
            CustomParticles.GenericFlare(position, StarlightSilver, 0.8f, 15);
        }

        /// <summary>EternalNightmare: Dark void tendrils reaching from boss.</summary>
        public static void EternalNightmareTelegraph(Vector2 center)
        {
            Phase10BossVFX.ChordBuildupSpiral(center, new[] { DeepIndigo, CosmicBlue }, 0.8f);
            Phase10BossVFX.DissonanceStorm(center, 150f, DeepIndigo, CosmicBlue);
        }

        public static void EternalNightmareRelease(Vector2 center, int tendrilIndex, Vector2 direction)
        {
            Vector2 pos = center + direction * (30f + tendrilIndex * 20f);
            Color color = Color.Lerp(DeepIndigo, CosmicBlue, tendrilIndex * 0.15f);
            CustomParticles.GenericFlare(pos, color, 0.4f + tendrilIndex * 0.05f, 16);
            BossVFXOptimizer.ProjectileTrail(pos, direction * 3f, color);
        }

        /// <summary>CelestialCharge: Blazing multi-dash attack with escalating afterimages.</summary>
        public static void CelestialChargeTelegraph(Vector2 position, Vector2 target)
        {
            Vector2 dir = (target - position).SafeNormalize(Vector2.UnitX);
            TelegraphSystem.ThreatLine(position, dir, 700f, 30, NebulaGold * 0.8f);
            Phase10BossVFX.FortissimoFlashWarning(position, NebulaGold, 1.0f);
        }

        public static void CelestialChargeAfterimage(Vector2 position, int dashNumber)
        {
            float intensity = 0.4f + dashNumber * 0.15f;
            Color trailColor = Color.Lerp(CosmicBlue, NebulaGold, dashNumber / 6f);
            CustomParticles.GenericFlare(position, trailColor, intensity, 12);
            BossVFXOptimizer.OptimizedHalo(position, trailColor, intensity, 10);
        }

        /// <summary>SupernovaCollapse: Massive stellar explosion  Ethe Queen's signature finisher.</summary>
        public static void SupernovaCollapseTelegraph(Vector2 center)
        {
            TelegraphSystem.ConvergingRing(center, 300f, 60, NebulaGold);
            Phase10BossVFX.TuttiFullEnsemble(center, new[] { CosmicBlue, NebulaGold, StarlightSilver }, 1.5f);
        }

        public static void SupernovaCollapseRelease(Vector2 center)
        {
            MagnumScreenEffects.AddScreenShake(25f);
            CustomParticles.GenericFlare(center, StarlightSilver, 2.5f, 30);
            CustomParticles.ExplosionBurst(center, NebulaGold, 20, 8f);
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Color color = Color.Lerp(CosmicBlue, NebulaGold, i / 16f);
                CustomParticles.HaloRing(center + angle.ToRotationVector2() * 90f, color, 0.6f, 20);
            }
            Phase10BossVFX.CodaFinale(center, NebulaGold, DeepIndigo, 2.0f);
        }

        #endregion

        #region Special Attacks

        /// <summary>TwilightReversal: Time-reversal with inverted color palette VFX.</summary>
        public static void TwilightReversalTelegraph(Vector2 center)
        {
            // Inverted color warning  Enormally bright colors become dark, dark become bright
            Color invertedIndigo = new Color(215, 225, 155); // Inverted DeepIndigo
            Color invertedSilver = new Color(55, 45, 25);    // Inverted StarlightSilver
            TelegraphSystem.ConvergingRing(center, 200f, 40, invertedIndigo * 0.6f);
            Phase10BossVFX.TempoShiftDistortion(center, 120f, 60f, 100f);
        }

        public static void TwilightReversalRelease(Vector2 center)
        {
            MagnumScreenEffects.AddScreenShake(18f);
            // Inverted chromatic burst
            Color invertedBlue = new Color(175, 135, 55); // Inverted CosmicBlue
            Color invertedGold = new Color(35, 75, 155);  // Inverted NebulaGold
            CustomParticles.GenericFlare(center, invertedBlue, 1.5f, 20);
            CustomParticles.GenericFlare(center, invertedGold, 1.2f, 18);
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                CustomParticles.HaloRing(center + angle.ToRotationVector2() * 60f, invertedBlue, 0.4f, 15);
            }
            Phase10BossVFX.KeyChangeFlash(center, CosmicBlue, invertedBlue, 1f);
        }

        /// <summary>QuantumBlink: Rapid multi-teleport with flash effects between positions.</summary>
        public static void QuantumBlinkFlash(Vector2 fromPos, Vector2 toPos)
        {
            // Flash at departure
            CustomParticles.GenericFlare(fromPos, StarlightSilver, 0.8f, 6);
            CustomParticles.HaloRing(fromPos, CosmicBlue * 0.6f, 0.3f, 8);

            // Flash at arrival
            CustomParticles.GenericFlare(toPos, NebulaGold, 0.9f, 8);
            CustomParticles.HaloRing(toPos, NebulaGold * 0.7f, 0.4f, 10);

            // Connecting line of fading particles
            Vector2 dir = (toPos - fromPos);
            float length = dir.Length();
            dir.Normalize();
            for (int i = 0; i < 5; i++)
            {
                Vector2 pos = fromPos + dir * (length * i / 5f);
                CustomParticles.GenericFlare(pos, StarlightSilver * 0.4f, 0.2f, 5);
            }
        }

        public static void QuantumBlinkAttack(Vector2 position)
        {
            MagnumScreenEffects.AddScreenShake(8f);
            CustomParticles.ExplosionBurst(position, NebulaGold, 8, 4f);
            Phase10BossVFX.StaccatoMultiBurst(position, NebulaGold, 3, 40f);
        }

        #endregion
    }
}
